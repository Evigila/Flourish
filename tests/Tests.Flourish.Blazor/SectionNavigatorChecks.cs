using ArkheideSystem.Flourish.Blazor.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;

internal static class SectionNavigatorChecks
{
    internal static void Register(List<(string Name, Func<Task> Run)> tests)
    {
        tests.Add(("explicit section navigation preserves native fragment links and accessible labels without JS", async () =>
        {
            IReadOnlyList<SectionNavigator.Entry> entries = [new("summary", "Summary <draft>"), new("details", "Details")];
            var html = await Render(new() { [nameof(SectionNavigator.ContentId)] = "report-content", [nameof(SectionNavigator.Items)] = entries, [nameof(SectionNavigator.Label)] = "Report sections" });
            Require(html.Contains("aria-label=\"Report sections\"", StringComparison.Ordinal) && html.Contains("data-content-id=\"report-content\"", StringComparison.Ordinal), "The navigator lost its host target or name.");
            Require(html.Contains("href=\"#summary\"", StringComparison.Ordinal) && html.Contains("href=\"#details\"", StringComparison.Ordinal), "Native links require JavaScript to work.");
            Require(html.Contains("Summary &lt;draft&gt;", StringComparison.Ordinal) && !html.Contains("Summary <draft>", StringComparison.Ordinal), "Section titles bypassed HTML encoding.");
            Require(html.Contains("role=\"tooltip\"", StringComparison.Ordinal) && html.Contains("aria-describedby=", StringComparison.Ordinal), "Section label descriptions were lost.");
            Require(!html.Contains(" hidden", StringComparison.Ordinal), "Explicit sections are hidden during SSR.");
        }));
        tests.Add(("section navigation validates host identifiers and remains empty before automatic discovery", async () =>
        {
            var html = await Render(new() { [nameof(SectionNavigator.ContentId)] = "report-content" });
            Require(html.Contains(" hidden", StringComparison.Ordinal) && !html.Contains("href=", StringComparison.Ordinal), "Automatic discovery invented sections before mounting.");
            foreach (var entries in new IReadOnlyList<SectionNavigator.Entry>[]
            {
                [new("same", "One"), new("same", "Two")], [new("not valid", "One")], [new("valid", " ")]
            })
            {
                var rejected = false;
                try { await Render(new() { [nameof(SectionNavigator.ContentId)] = "report-content", [nameof(SectionNavigator.Items)] = entries }); }
                catch (ArgumentException) { rejected = true; }
                Require(rejected, "Invalid sections reached the rendered outline.");
            }
        }));
    }
    private static async Task<string> Render(Dictionary<string, object?> parameters)
    {
        var services = new ServiceCollection();
        services.AddLogging(); services.AddSingleton<IJSRuntime>(new NoJs());
        using var provider = services.BuildServiceProvider();
        await using var renderer = new HtmlRenderer(provider, provider.GetRequiredService<ILoggerFactory>());
        return await renderer.Dispatcher.InvokeAsync(async () => (await renderer.RenderComponentAsync<SectionNavigator>(ParameterView.FromDictionary(parameters))).ToHtmlString());
    }
    private static void Require(bool condition, string message) { if (!condition) throw new InvalidOperationException(message); }
    private sealed class NoJs : IJSRuntime
    {
        public ValueTask<TValue> InvokeAsync<TValue>(string identifier, object?[]? args) => throw new InvalidOperationException("SSR invoked JavaScript.");
        public ValueTask<TValue> InvokeAsync<TValue>(string identifier, CancellationToken cancellationToken, object?[]? args) => InvokeAsync<TValue>(identifier, args);
    }
}
