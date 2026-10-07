using System.Text.RegularExpressions;
using ArkheideSystem.Flourish.Blazor.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

internal static class EmptyStateChecks
{
    internal static void Register(List<(string Name, Func<Task> Run)> tests)
    {
        tests.Add(("standard empty state keeps description and action composition without watermark layout", async () =>
        {
            var html = await Render(new()
            {
                [nameof(EmptyState.Title)] = "No records",
                [nameof(EmptyState.ChildContent)] = Content("Description"),
                [nameof(EmptyState.Actions)] = Content("Next action")
            });
            Require(html.Contains("f-empty-title") && html.Contains("Description") && html.Contains("f-inline-actions")
                && html.Contains("Next action") && !html.Contains("data-presentation"), "Standard presentation changed its existing content contract.");
        }));
        tests.Add(("watermark empty state renders one encoded passive title without controls or heading semantics", async () =>
        {
            var html = await Render(new() { [nameof(EmptyState.Variant)] = EmptyStateVariant.Watermark, [nameof(EmptyState.Title)] = "Welcome <safe>" });
            Require(html.Contains("data-presentation=\"watermark\"") && html.Contains("Welcome &lt;safe&gt;"), "Watermark title or presentation is missing.");
            Require(Regex.Matches(html, "class=\"f-empty-title\"").Count == 1 && !Regex.IsMatch(html, "<(?:h[1-6]|button|a|img|svg)\\b")
                && !html.Contains("f-inline-actions"), "Watermark introduced navigation headings or interactive content.");
        }));
        tests.Add(("empty state rejects undefined variants and interactive watermark composition", async () =>
        {
            await Reject<ArgumentOutOfRangeException>(new() { [nameof(EmptyState.Variant)] = (EmptyStateVariant)99 });
            await Reject<InvalidOperationException>(new() { [nameof(EmptyState.Variant)] = EmptyStateVariant.Watermark, [nameof(EmptyState.ChildContent)] = Content("Description") });
            await Reject<InvalidOperationException>(new() { [nameof(EmptyState.Variant)] = EmptyStateVariant.Watermark, [nameof(EmptyState.Actions)] = Content("Action") });
        }));
        tests.Add(("watermark layout fills the unused page area with bounded responsive muted text and has an executable Gallery scene", () =>
        {
            var framework = Read("src/Flourish.Blazor/Flourish.Blazor.Framework/wwwroot/framework.css");
            var design = Read("src/Flourish.Blazor/Flourish.Blazor.Design/wwwroot/controls.css");
            Require(framework.Contains(".f-page-body:has(> .f-empty-state[data-presentation=watermark]) { display:flex; flex-direction:column; min-height:100%")
                && framework.Contains("place-items:center; flex:1 0 auto") && framework.Contains("max-width:100%; margin:0; overflow-wrap:anywhere"), "Watermark cannot fill and bound the available page area.");
            Require(design.Contains("font-size:clamp(40px,6vw,88px)") && design.Contains("color:var(--f-muted);margin:0"), "Watermark typography or theme role is missing.");
            var sample = Read("src/Gallery.Flourish.Blazor/Components/Samples/Data/EmptyStateSample.razor");
            Require(sample.Contains("<PageBody>") && sample.Contains("<PageHeading") && sample.Contains("Variant=\"EmptyStateVariant.Watermark\""), "Gallery lacks the real page composition.");
            return Task.CompletedTask;
        }));
    }

    private static RenderFragment Content(string text) => builder => builder.AddContent(0, text);

    private static async Task<string> Render(Dictionary<string, object?> parameters)
    {
        using var services = new ServiceCollection().AddLogging().BuildServiceProvider();
        await using var renderer = new HtmlRenderer(services, services.GetRequiredService<ILoggerFactory>());
        return await renderer.Dispatcher.InvokeAsync(async () =>
            (await renderer.RenderComponentAsync<EmptyState>(ParameterView.FromDictionary(parameters))).ToHtmlString());
    }

    private static async Task Reject<TException>(Dictionary<string, object?> parameters) where TException : Exception
    {
        try { await Render(parameters); }
        catch (TException) { return; }
        throw new InvalidOperationException("Expected " + typeof(TException).Name);
    }

    private static string Read(string path)
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !Directory.Exists(Path.Combine(directory.FullName, "src", "Flourish.Blazor"))) directory = directory.Parent;
        return File.ReadAllText(Path.Combine(directory?.FullName ?? throw new InvalidOperationException("Repository root not found."), path));
    }

    private static void Require(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }
}
