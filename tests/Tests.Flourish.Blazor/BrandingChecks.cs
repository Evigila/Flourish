using System.Net;
using System.Text.RegularExpressions;
using ArkheideSystem.Flourish.Blazor;
using ArkheideSystem.Flourish.Blazor.Abstract;
using ArkheideSystem.Flourish.Blazor.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

internal static class BrandingChecks
{
    private const string DefaultLogo = "_content/Arkheide.Flourish.Blazor.Framework/browse.svg";

    internal static void Register(List<(string Name, Func<Task> Run)> tests)
    {
        tests.Add(("project defaults supply a browse logo and tab fallback without enabling the top bar", async () =>
        {
            var document = await Render(_ => { });
            Require(IconHref(document.Head) == DefaultLogo, "The default tab icon does not fall back to browse.");
            Require(document.Body.Contains("f-no-titlebar", StringComparison.Ordinal), "Project defaults enabled a top bar.");
            Require(document.Head.Contains("framework.css", StringComparison.Ordinal), "Branding replaced the Framework stylesheet head content.");
            var visible = await Render(framework => framework.ConfigureTopBar(top => top.DisplayLogo().DisplayProjectName()));
            Require(visible.Body.Contains($"src=\"{DefaultLogo}\"", StringComparison.Ordinal)
                && visible.Body.Contains("class=\"f-application-title\">Application", StringComparison.Ordinal),
                "The enabled top bar lost its default identity.");
        }));

        tests.Add(("favicon precedence and explicit resets do not depend on setter order", async () =>
        {
            var scenarios = new (Action<IProjectBuilder> Configure, string Expected)[]
            {
                (project => project.SetLogo("logo.svg"), "logo.svg"),
                (project => project.SetFavicon("tab.ico").SetLogo("logo.svg"), "tab.ico"),
                (project => project.SetLogo("logo.svg").SetFavicon("tab.png"), "tab.png"),
                (project => project.SetFavicon("tab.svg").SetLogo("logo.svg").SetFavicon(), "logo.svg"),
                (project => project.SetFavicon("tab.svg").SetLogo("logo.svg").SetFavicon(""), "logo.svg"),
                (project => project.SetLogo("logo.svg").SetLogo(), DefaultLogo),
            };
            foreach (var scenario in scenarios)
            {
                var document = await Render(framework => framework.ConfigureProject(scenario.Configure));
                Require(IconHref(document.Head) == scenario.Expected, "Favicon precedence/reset failed.");
                Require(Regex.Matches(document.Head, "rel=\"icon\"").Count == 1, "More than one framework favicon was emitted.");
            }
        }));

        tests.Add(("all top bar identity display combinations preserve favicon and accessible branding", async () =>
        {
            foreach (var logo in new[] { true, false })
            foreach (var name in new[] { true, false })
            {
                var document = await Render(framework => framework
                    .ConfigureProject(project => project.SetProjectName("My project").SetLogo("logo.svg", "Logo description").SetFavicon("tab.svg"))
                    .ConfigureTopBar(top => top.DisplayLogo(logo).DisplayProjectName(name)));
                Require(IconHref(document.Head) == "tab.svg", "Top bar display changed the tab icon.");
                Require(document.Body.Contains("class=\"f-application-logo\"", StringComparison.Ordinal) == logo, "DisplayLogo was ignored.");
                Require(document.Body.Contains("class=\"f-application-title\"", StringComparison.Ordinal) == name, "DisplayProjectName was ignored.");
                Require(document.Body.Contains("class=\"f-application-brand\"", StringComparison.Ordinal) == (logo || name), "An empty brand link remains.");
                if (logo || name) Require(document.Body.Contains("aria-label=\"My project\"", StringComparison.Ordinal), "The brand link lost its accessible name.");
                if (logo) Require(document.Body.Contains("alt=\"Logo description\"", StringComparison.Ordinal), "Explicit logo alternative text was lost.");
            }
        }));

        tests.Add(("project configuration remains immutable after registration", () =>
        {
            IProjectBuilder? captured = null;
            ITopBarBuilder? topBar = null;
            var services = new ServiceCollection();
            services.AddFlourishFramework(framework => framework
                .ConfigureProject(project => { captured = project; project.SetProjectName("Saved"); })
                .ConfigureTopBar(top => topBar = top));
            Throws<InvalidOperationException>(() => captured!.SetProjectName("Late"));
            Throws<InvalidOperationException>(() => captured!.SetLogo("late.svg"));
            Throws<InvalidOperationException>(() => captured!.SetFavicon("late.ico"));
            Throws<InvalidOperationException>(() => topBar!.DisplayLogo(false));
            Throws<InvalidOperationException>(() => topBar!.DisplayProjectName(false));
            return Task.CompletedTask;
        }));

        tests.Add(("project identity validates local paths and encodes names and asset attributes", async () =>
        {
            foreach (var path in new[] { "https://example.test/icon.svg", "//example.test/icon.svg", "javascript:alert(1)", "a\\b.svg", " " })
            {
                Throws<ArgumentException>(() => new ServiceCollection().AddFlourishFramework(framework => framework.ConfigureProject(project => project.SetLogo(path))));
                Throws<ArgumentException>(() => new ServiceCollection().AddFlourishFramework(framework => framework.ConfigureProject(project => project.SetFavicon(path))));
            }
            Throws<ArgumentException>(() => new ServiceCollection().AddFlourishFramework(framework => framework.ConfigureProject(project => project.SetProjectName(" "))));
            Throws<ArgumentNullException>(() => new ServiceCollection().AddFlourishFramework(framework => framework.ConfigureProject(null!)));
            var document = await Render(framework => framework
                .ConfigureProject(project => project.SetProjectName("<Unsafe>").SetLogo("logo.svg", "<description>").SetFavicon("tab.svg?x=\"value\"&y=1"))
                .ConfigureTopBar(top => top.DisplayLogo().DisplayProjectName()));
            Require(document.Body.Contains("&lt;Unsafe&gt;", StringComparison.Ordinal)
                && !document.Body.Contains("<Unsafe>", StringComparison.Ordinal), "Project name became markup.");
            Require(IconHref(document.Head) == "tab.svg?x=\"value\"&y=1"
                && !document.Head.Contains("x=\"value\"", StringComparison.Ordinal), "Tab asset attributes were not encoded.");
        }));

        tests.Add(("the public project builder replaces old top bar name and icon setters", () =>
        {
            Require(typeof(IFrameworkBuilder).GetMethod("ConfigureProject")?.GetParameters().Single().ParameterType == typeof(Action<IProjectBuilder>), "ConfigureProject lost its lambda builder contract.");
            Require(typeof(ITopBarBuilder).GetMethod("SetAppName") is null
                && typeof(ITopBarBuilder).GetMethod("SetIcon") is null
                && typeof(ITopBarBuilder).GetMethod("DisplayIcon") is null, "Removed top bar APIs remain public.");
            foreach (var name in new[] { "DisplayLogo", "DisplayProjectName" })
            {
                var parameter = typeof(ITopBarBuilder).GetMethod(name)?.GetParameters().Single();
                Require(parameter?.IsOptional == true && Equals(parameter.DefaultValue, true), "Display defaults must be true.");
            }
            return Task.CompletedTask;
        }));
    }

    private static async Task<(string Head, string Body)> Render(Action<IFrameworkBuilder> configure)
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton<Microsoft.AspNetCore.Components.NavigationManager>(new TestNavigation());
        services.AddSingleton<Microsoft.JSInterop.IJSRuntime>(new FakeJs());
        services.AddFlourishFramework(configure);
        using var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();
        await using var renderer = new HtmlRenderer(scope.ServiceProvider, provider.GetRequiredService<ILoggerFactory>());
        return await renderer.Dispatcher.InvokeAsync(async () =>
        {
            var head = await renderer.RenderComponentAsync<HeadOutlet>();
            var layout = await renderer.RenderComponentAsync<ApplicationLayout>();
            return (head.ToHtmlString(), layout.ToHtmlString());
        });
    }

    private static string? IconHref(string head)
    {
        var match = Regex.Match(head, "<link rel=\"icon\" href=\"([^\"]+)\"");
        return match.Success ? WebUtility.HtmlDecode(match.Groups[1].Value) : null;
    }

    private static void Require(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }

    private static void Throws<T>(Action action) where T : Exception
    {
        try { action(); }
        catch (T) { return; }
        throw new InvalidOperationException($"Expected {typeof(T).Name}.");
    }
}
