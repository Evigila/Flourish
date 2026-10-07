using System.Globalization;
using ArkheideSystem.Flourish.Blazor;
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
        tests.Add(("navigation shells inherit configured width and scope centered gutter scaling", async () =>
        {
            var previous = CultureInfo.CurrentCulture;
            try
            {
                CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("pt-BR");
                var half = await Render(0.5, "height:440px", 1400);
                Require(half.Contains("height:440px;--f-content-width:1400px;--f-centered-gutter-scale:0.5"), "Configuration or invariant-culture scaling did not reach the real shell.");
                var standard = await Render();
                Require(standard.Contains("--f-content-width:1180px;--f-centered-gutter-scale:1"), "A separately rendered shell inherited another shell's configuration.");
                Require(new NavigationSurface().CenteredContentGutterScale == 1, "Existing shells no longer retain standard spacing.");
                Require((await Render(0)).Contains("--f-centered-gutter-scale:0"), "Zero gutter scale was rejected.");
            }
            finally { CultureInfo.CurrentCulture = previous; }
        }));
        tests.Add(("navigation shells reject invalid centered gutter scales", async () =>
        {
            foreach (var scale in new[] { -0.01, 1.01, double.NaN, double.PositiveInfinity, double.NegativeInfinity })
            {
                try { await Render(scale); }
                catch (ArgumentOutOfRangeException error) when (error.ParamName == nameof(NavigationSurface.CenteredContentGutterScale)) { continue; }
                throw new InvalidOperationException("An invalid centered gutter scale reached rendering.");
            }
        }));
        tests.Add(("centered gutter scaling includes minimum gutters and leaves other layouts unchanged", () =>
        {
            var path = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../..", "src/Flourish.Blazor/Flourish.Blazor.Design/wwwroot/foundation.css"));
            var css = File.ReadAllText(path);
            var framework = File.ReadAllText(Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../..", "src/Flourish.Blazor/Flourish.Blazor.Framework/wwwroot/framework.css")));
            Require(framework.Contains(".f-page-centered { max-width:calc(100% - max(0px,calc(100% - var(--f-content-width,100%)))*var(--f-centered-gutter-scale,1)); margin-inline:auto; }"), "Framework-only centered layout ignored the scale.");
            Require(css.Contains(".f-page-centered { --f-content-gutter:calc(max(var(--f-page-gutter),calc((100% - var(--f-content-width))/2))*var(--f-centered-gutter-scale,1)); }"), "The library no longer scales the full per-side gutter.");
            Require(css.Contains(".f-page-fluid { --f-content-gutter:max(var(--f-page-gutter),calc((100% - var(--f-content-width))/6)); }")
                && css.Contains(".f-page-full { --f-content-gutter:0px; }"), "Centered scaling leaked into fluid or full-width layouts.");
            return Task.CompletedTask;
        }));
    }

    private static async Task<string> Render(double? scale = null, string? style = null, int width = 1180)
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddFlourishFramework(options => options.ConfigureLayout(layout => layout.SetContentWidth(width)));
        services.AddSingleton<IJSRuntime>(new NoJs());
        services.AddSingleton<NavigationManager>(new FixedNavigation());
        using var provider = services.BuildServiceProvider();
        await using var renderer = new HtmlRenderer(provider, provider.GetRequiredService<ILoggerFactory>());
        var parameters = new Dictionary<string, object?> { [nameof(NavigationSurface.Style)] = style };
        if (scale is not null) parameters[nameof(NavigationSurface.CenteredContentGutterScale)] = scale.Value;
        return await renderer.Dispatcher.InvokeAsync(async () => (await renderer.RenderComponentAsync<NavigationSurface>(ParameterView.FromDictionary(parameters))).ToHtmlString());
    }

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
