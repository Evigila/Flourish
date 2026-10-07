using System.Text.RegularExpressions;
using ArkheideSystem.Flourish.Blazor;
using ArkheideSystem.Flourish.Blazor.Components;
using ArkheideSystem.Flourish.Blazor.Hosting;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

internal static class PresentationIdentityChecks
{
    internal static void Register(List<(string Name, Func<Task> Run)> tests)
    {
        tests.Add(("identity summaries render their own encoded account heading and identifier without page chrome", async () =>
        {
            using var services = Services();
            await using var renderer = Renderer(services);
            foreach (var level in Enumerable.Range(1, 6))
            {
                var html = await Render<IdentityCard>(renderer, new()
                {
                    [nameof(IdentityCard.Title)] = "Member <safe> & name",
                    [nameof(IdentityCard.HeadingLevel)] = level,
                    [nameof(IdentityCard.Columns)] = false,
                    [nameof(IdentityCard.ChildContent)] = (RenderFragment)(builder =>
                    {
                        builder.OpenComponent<CopyText>(0);
                        builder.AddAttribute(1, nameof(CopyText.Value), "ID-<safe>&001");
                        builder.CloseComponent();
                    })
                });
                Require(html.Contains($"<h{level} class=\"f-identity-title\">Member &lt;safe&gt; &amp; name</h{level}>", StringComparison.Ordinal), "The summary discarded heading semantics or escaped identity content.");
                Require(html.Contains("<code class=\"f-copy-text\">ID-&lt;safe&gt;&amp;001</code>", StringComparison.Ordinal), "The real CopyText identifier was changed or rendered unsafely.");
                Require(!html.Contains("f-page-heading", StringComparison.Ordinal) && !html.Contains("f-identity-columns", StringComparison.Ordinal), "An identity name inherited sticky page chrome or unwanted columns.");
            }
            Require(new IdentityCard().HeadingLevel == 2, "Ordinary summary headings no longer default to H2.");
            var untitled = await Render<IdentityCard>(renderer, new() { [nameof(IdentityCard.Title)] = " " });
            Require(!Regex.IsMatch(untitled, @"<h[1-6]\b"), "An absent name produced an empty heading.");
            foreach (var level in new[] { 0, 7, int.MaxValue })
            {
                try { await Render<IdentityCard>(renderer, new() { [nameof(IdentityCard.HeadingLevel)] = level }); }
                catch (ArgumentOutOfRangeException error) when (error.ParamName == nameof(IdentityCard.HeadingLevel)) { continue; }
                throw new InvalidOperationException("An invalid identity heading level reached rendering.");
            }
        }));
        tests.Add(("identity cards bound page gutters and keep account headings start aligned with bold identifiers", () =>
        {
            var framework = ReadSource("src/Flourish.Blazor/Flourish.Blazor.Framework/wwwroot/framework.css");
            var design = ReadSource("src/Flourish.Blazor/Flourish.Blazor.Design/wwwroot/controls.css");
            var pageCard = Rule(framework, ".f-page-body > .f-identity-card");
            Require(pageCard.Contains("width:auto", StringComparison.Ordinal), "A full-width card adds page gutters outside its 100% width and overflows the viewport.");
            var card = Rule(framework, ".f-identity-card");
            Require(card.Contains("box-sizing:border-box", StringComparison.Ordinal) && card.Contains("min-width:0", StringComparison.Ordinal)
                && card.Contains("max-width:100%", StringComparison.Ordinal) && card.Contains("text-align:start", StringComparison.Ordinal), "Card geometry does not shrink or retain start alignment.");
            var heading = Rule(framework, ".f-identity-card > .f-identity-title");
            Require(heading.Contains("overflow-wrap:anywhere", StringComparison.Ordinal) && heading.Contains("text-align:start", StringComparison.Ordinal), "Long account headings escape their card or lose start alignment.");
            Require(Rule(design, ".f-identity-card > h1.f-identity-title").Contains("font-size:var(--f-type-page,50px)", StringComparison.Ordinal), "An account name lost the standard large heading scale.");
            var identifier = Rule(design, ".f-identity-content .f-copy-text");
            Require(identifier.Contains("font-weight:740", StringComparison.Ordinal) && identifier.Contains("color:inherit", StringComparison.Ordinal), "Identifiers no longer use bold paired card text.");
            return Task.CompletedTask;
        }));
        tests.Add(("footer instance project names are encoded and leave global project identity and optional copyright untouched", async () =>
        {
            using var services = Services();
            await using var renderer = Renderer(services);
            var options = services.GetRequiredService<ApplicationOptions>();
            var original = options.Project.Name;
            var configured = await Render<PresentationFooter>(renderer, new());
            var custom = await Render<PresentationFooter>(renderer, new()
            {
                [nameof(PresentationFooter.ProjectName)] = "Organization <safe> & name",
                [nameof(PresentationFooter.Copyright)] = " "
            });
            Require(custom.Contains("<strong>Organization &lt;safe&gt; &amp; name</strong>", StringComparison.Ordinal)
                && custom.Contains("aria-hidden=\"true\">Organization &lt;safe&gt; &amp; name</span>", StringComparison.Ordinal), "Footer instance identity or watermark did not use the same encoded override.");
            Require(!custom.Contains("©", StringComparison.Ordinal) && !custom.Contains("<span> </span>", StringComparison.Ordinal), "An omitted copyright created a notice or empty content.");
            Require(options.Project.Name == original && (await Render<PresentationFooter>(renderer, new())) == configured, "A footer override mutated shared project configuration or another footer.");
        }));
    }

    private static ServiceProvider Services()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddFlourishFramework(builder => builder.ConfigureProject(project => project.SetProjectName("Configured project")));
        return services.BuildServiceProvider();
    }
    private static HtmlRenderer Renderer(IServiceProvider services) => new(services, services.GetRequiredService<ILoggerFactory>());
    private static Task<string> Render<T>(HtmlRenderer renderer, Dictionary<string, object?> parameters) where T : IComponent => renderer.Dispatcher.InvokeAsync(async () =>
        (await renderer.RenderComponentAsync<T>(ParameterView.FromDictionary(parameters))).ToHtmlString());
    private static string Rule(string css, string selector)
    {
        var match = Regex.Match(css, Regex.Escape(selector) + @"\s*\{([^}]*)\}");
        Require(match.Success, "Missing library CSS selector: " + selector);
        return match.Groups[1].Value;
    }
    private static string ReadSource(string path)
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !Directory.Exists(Path.Combine(directory.FullName, "src", "Flourish.Blazor"))) directory = directory.Parent;
        return File.ReadAllText(Path.Combine(directory?.FullName ?? throw new InvalidOperationException("Repository root not found."), path));
    }
    private static void Require(bool condition, string message) { if (!condition) throw new InvalidOperationException(message); }
}
