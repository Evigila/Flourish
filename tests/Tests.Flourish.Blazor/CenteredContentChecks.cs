using System.Globalization;
using System.Text.RegularExpressions;
using ArkheideSystem.Flourish.Blazor;
using ArkheideSystem.Flourish.Blazor.Abstract;
using ArkheideSystem.Flourish.Blazor.Components;
using ArkheideSystem.Flourish.Blazor.Components.Patterns;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;

internal static class CenteredContentChecks
{
    internal static void Register(List<(string Name, Func<Task> Run)> tests)
    {
        tests.Add(("navigation shells configure content width without owning page gutter variants", async () =>
        {
            var previous = CultureInfo.CurrentCulture;
            try
            {
                CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("pt-BR");
                var wide = await Render<NavigationSurface>(new() { [nameof(NavigationSurface.Style)] = "height:440px" }, width: 1400);
                Require(wide.Contains("height:440px;--f-content-width:1400px"), "The configured content width did not reach the real shell.");
                var standard = await Render<NavigationSurface>(new());
                Require(standard.Contains("--f-content-width:1180px"), "A separately rendered shell inherited another shell's configuration.");
                Require(!wide.Contains("--f-centered-gutter-scale") && !standard.Contains("--f-centered-gutter-scale"), "The shell still owns a page-local centered variant.");
            }
            finally { CultureInfo.CurrentCulture = previous; }
        }));
        tests.Add(("PageBody exposes standard and expanded centered containers with a standard default", async () =>
        {
            Require(new PageBody().CenteredContainer == CenteredContainer.Standard, "The public centered container default is not Standard.");
            Require(Enum.GetNames<CenteredContainer>().SequenceEqual(new[] { "Standard", "Expanded" }), "Centered containers have an alternate contract or aliases.");
            var ordinary = await Render<PageBody>(new());
            var expanded = await Render<PageBody>(new() { [nameof(PageBody.CenteredContainer)] = CenteredContainer.Expanded });
            Require(HasClass(ordinary, "f-page-centered") && !HasClass(ordinary, "f-page-centered-expanded"), "An omitted container does not retain standard centered geometry.");
            Require(HasClass(expanded, "f-page-centered") && HasClass(expanded, "f-page-centered-expanded"), "Expanded centered geometry did not reach the production page.");
            Require(!ordinary.Contains("style=") && !expanded.Contains("style="), "Container geometry leaked into host inline styles.");
        }));
        tests.Add(("page width precedence ignores centered variants outside effective centered layout", async () =>
        {
            foreach (var container in Enum.GetValues<CenteredContainer>())
            {
                var inheritedFluid = await Render<PageBody>(new() { [nameof(PageBody.CenteredContainer)] = container }, fluid: true);
                var explicitFluid = await Render<PageBody>(new() { [nameof(PageBody.Fluid)] = true, [nameof(PageBody.CenteredContainer)] = container });
                foreach (var html in new[] { inheritedFluid, explicitFluid })
                    Require(HasClass(html, "f-page-fluid") && !HasClass(html, "f-page-centered-expanded"), "A centered variant changed an inherited or explicit fluid page.");
                foreach (var fluid in new bool?[] { null, false, true })
                {
                    var html = await Render<PageBody>(new()
                    {
                        [nameof(PageBody.Fluid)] = fluid, [nameof(PageBody.FullWidth)] = true,
                        [nameof(PageBody.CenteredContainer)] = container
                    }, fluid: true);
                    Require(HasClass(html, "f-page-full") && !HasClass(html, "f-page-fluid") && !HasClass(html, "f-page-centered-expanded"), "FullWidth did not override fluid defaults and centered geometry.");
                }
            }
            var explicitCentered = await Render<PageBody>(new() { [nameof(PageBody.Fluid)] = false, [nameof(PageBody.CenteredContainer)] = CenteredContainer.Expanded }, fluid: true);
            Require(HasClass(explicitCentered, "f-page-centered-expanded"), "An explicit centered page cannot override a fluid application default.");
        }));
        tests.Add(("PageBody rejects unknown centered containers even when the width mode ignores valid variants", async () =>
        {
            foreach (var value in new[] { -1, 2, int.MaxValue })
            foreach (var (fluid, fullWidth) in new[] { (false, false), (true, false), (true, true) })
            {
                try
                {
                    await Render<PageBody>(new()
                    {
                        [nameof(PageBody.CenteredContainer)] = (CenteredContainer)value,
                        [nameof(PageBody.Fluid)] = fluid, [nameof(PageBody.FullWidth)] = fullWidth
                    });
                }
                catch (ArgumentOutOfRangeException error) when (error.ParamName == nameof(PageBody.CenteredContainer)) { continue; }
                throw new InvalidOperationException("An unknown centered container reached rendering.");
            }
        }));
        tests.Add(("a nested standard PageBody resets its expanded parent's centered geometry", async () =>
        {
            RenderFragment child = builder =>
            {
                builder.OpenComponent<PageBody>(0);
                builder.AddAttribute(1, nameof(PageBody.ChildContent), (RenderFragment)(content => content.AddContent(0, "Nested standard content")));
                builder.CloseComponent();
            };
            var html = await Render<PageBody>(new()
            {
                [nameof(PageBody.CenteredContainer)] = CenteredContainer.Expanded,
                [nameof(PageBody.ChildContent)] = child
            });
            Require(Regex.Matches(html, "class=\"f-page-body f-page-centered").Count == 2
                && Regex.Matches(html, "f-page-centered-expanded").Count == 1
                && html.Contains("Nested standard content"), "Nested pages do not retain independent centered contracts.");
            var framework = ReadSource("src/Flourish.Blazor/Flourish.Blazor.Framework/wwwroot/framework.css");
            var body = Rule(framework, ".f-page-body");
            var expanded = Rule(framework, ".f-page-centered-expanded");
            Require(body.Contains("--f-centered-gutter-scale:1") && expanded.Contains("--f-centered-gutter-scale:0.5"), "A nested page inherits expanded geometry instead of resetting to Standard.");
            Require(framework.IndexOf(".f-page-centered-expanded", StringComparison.Ordinal) > framework.IndexOf(".f-page-body {", StringComparison.Ordinal), "The default reset wins over the expanded variant.");
        }));
        tests.Add(("centered containers reduce minimum and wide-screen gutters without changing other layouts or vertical rhythm", () =>
        {
            var css = ReadSource("src/Flourish.Blazor/Flourish.Blazor.Design/wwwroot/foundation.css");
            var framework = ReadSource("src/Flourish.Blazor/Flourish.Blazor.Framework/wwwroot/framework.css");
            Require(framework.Contains(".f-page-centered { max-width:calc(100% - max(0px,calc(100% - var(--f-content-width,100%)))*var(--f-centered-gutter-scale,1)); margin-inline:auto; }"), "Framework-only centered layout ignored the page variant.");
            Require(css.Contains(".f-page-centered { --f-content-gutter:calc(max(var(--f-page-gutter),calc((100% - var(--f-content-width))/2))*var(--f-centered-gutter-scale,1)); }"), "Design no longer scales the full per-side centered gutter.");
            Require(css.Contains(".f-page-fluid { --f-content-gutter:max(var(--f-page-gutter),calc((100% - var(--f-content-width))/6)); }")
                && css.Contains(".f-page-full { --f-content-gutter:0px; }"), "Centered scaling leaked into fluid or full-width layouts.");
            var expanded = Rule(framework, ".f-page-centered-expanded");
            Require(!Regex.IsMatch(expanded, @"(?:padding|margin)(?:-[a-z]+)?\s*:"), "Expanded horizontal geometry also changes section spacing.");
            Require(!framework.Contains("f-page-compact-spacing") && !css.Contains("f-page-compact-spacing"), "The retired compact form spacing behavior survives.");
            Require(Rule(css, ".f-section").Contains("padding-top:clamp(44px,7vw,84px)"), "Ordinary section spacing changed with the container width.");
            return Task.CompletedTask;
        }));
    }

    private static async Task<string> Render<T>(Dictionary<string, object?> parameters, bool fluid = false, int width = 1180) where T : IComponent
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddFlourishFramework(options => options.ConfigureLayout(layout => layout.SetContentWidth(width).SetFluidContent(fluid)));
        services.AddSingleton<IJSRuntime>(new NoJs());
        services.AddSingleton<NavigationManager>(new FixedNavigation());
        using var provider = services.BuildServiceProvider();
        await using var renderer = new HtmlRenderer(provider, provider.GetRequiredService<ILoggerFactory>());
        return await renderer.Dispatcher.InvokeAsync(async () => (await renderer.RenderComponentAsync<T>(ParameterView.FromDictionary(parameters))).ToHtmlString());
    }

    private static bool HasClass(string html, string value) => Regex.Match(html, "class=\"([^\"]*)\"").Groups[1].Value
        .Split(' ', StringSplitOptions.RemoveEmptyEntries).Contains(value);
    private static string Rule(string css, string selector)
    {
        var match = Regex.Match(css, Regex.Escape(selector) + @"\s*\{([^}]*)\}");
        Require(match.Success, "Missing production rule " + selector);
        return match.Groups[1].Value;
    }
    private static string ReadSource(string path) => File.ReadAllText(Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../..", path)));
    private static void Require(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }

    private sealed class NoJs : IJSRuntime
    {
        public ValueTask<TValue> InvokeAsync<TValue>(string identifier, object?[]? args) => new(default(TValue)!);
        public ValueTask<TValue> InvokeAsync<TValue>(string identifier, CancellationToken cancellationToken, object?[]? args) => new(default(TValue)!);
    }
    private sealed class FixedNavigation : NavigationManager
    {
        public FixedNavigation() => Initialize("https://tests.example/", "https://tests.example/");
        protected override void NavigateToCore(string uri, bool forceLoad) => throw new NotSupportedException();
    }
}
