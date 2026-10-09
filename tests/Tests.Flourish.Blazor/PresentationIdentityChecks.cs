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
        tests.Add(("ordinary Cards compose encoded account titles and CopyText identifiers without page chrome", async () =>
        {
            using var services = Services();
            await using var renderer = Renderer(services);
            var html = await Render<Card>(renderer, new()
            {
                [nameof(Card.Title)] = "Member <safe> & name",
                [nameof(Card.ChildContent)] = (RenderFragment)(builder =>
                {
                    builder.OpenComponent<CopyText>(0);
                    builder.AddAttribute(1, nameof(CopyText.Value), "ID-<safe>&001");
                    builder.CloseComponent();
                })
            });
            Require(html.Contains("<article class=\"f-card\">", StringComparison.Ordinal), "An account summary bypassed the ordinary Card renderer.");
            Require(html.Contains("<h3>Member &lt;safe&gt; &amp; name</h3>", StringComparison.Ordinal), "The Card discarded its existing H3 title semantics or encoded account content.");
            Require(html.Contains("<code class=\"f-copy-text\">ID-&lt;safe&gt;&amp;001</code>", StringComparison.Ordinal), "The real CopyText identifier was changed or rendered unsafely.");
            Require(!html.Contains("f-page-heading", StringComparison.Ordinal) && !Regex.IsMatch(html, @"<h[12]\b"), "A Card account title inherited page heading chrome or introduced a second page heading.");
            var untitled = await Render<Card>(renderer, new() { [nameof(Card.Title)] = "" });
            Require(!Regex.IsMatch(untitled, @"<h[1-6]\b"), "An absent name produced an empty heading.");
        }));
        tests.Add(("retired IdentityCard has no exported renderer catalog entry or dedicated style family", () =>
        {
            Require(typeof(Card).Assembly.GetType("ArkheideSystem.Flourish.Blazor.Components.IdentityCard") is null,
                "The overlapping IdentityCard renderer is still exported.");
            Require(!ComponentUsageCatalog.Entries.Keys.Any(type => type.Name == "IdentityCard"), "The retired renderer remains in production usage guidance.");
            var framework = ReadSource("src/Flourish.Blazor/Flourish.Blazor.Framework/wwwroot/framework.css");
            var design = ReadSource("src/Flourish.Blazor/Flourish.Blazor.Design/wwwroot/controls.css");
            Require(!framework.Contains("f-identity-", StringComparison.Ordinal) && !design.Contains("f-identity-", StringComparison.Ordinal),
                "Dedicated IdentityCard geometry or paint survives the renderer retirement.");
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
    private static string ReadSource(string path)
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !Directory.Exists(Path.Combine(directory.FullName, "src", "Flourish.Blazor"))) directory = directory.Parent;
        return File.ReadAllText(Path.Combine(directory?.FullName ?? throw new InvalidOperationException("Repository root not found."), path));
    }
    private static void Require(bool condition, string message) { if (!condition) throw new InvalidOperationException(message); }
}
