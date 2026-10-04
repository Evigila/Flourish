using System.Net;
using System.Text.RegularExpressions;
using ArkheideSystem.Flourish.Blazor;
using ArkheideSystem.Flourish.Blazor.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.RenderTree;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;

internal static class DataTableChecks
{
    internal static void Register(List<(string Name, Func<Task> Run)> tests)
    {
        tests.Add(("pager places previous and next before the current page control", async () =>
        {
            await WithTable(Parameters(pageSize: 1), (_, html) =>
            {
                var pagerStart = html().IndexOf("class=\"f-data-pager\"", StringComparison.Ordinal);
                var pagerEnd = html().IndexOf("</div>", pagerStart, StringComparison.Ordinal);
                Require(pagerStart >= 0 && pagerEnd > pagerStart, "The pager was not rendered.");
                var pager = html()[pagerStart..pagerEnd];
                var previous = pager.IndexOf("aria-label=\"Previous page\"", StringComparison.Ordinal);
                var next = pager.IndexOf("aria-label=\"Next page\"", StringComparison.Ordinal);
                var current = pager.IndexOf("data-f-page", StringComparison.Ordinal);
                Require(previous >= 0 && previous < next && next < current, "Pager controls are not ordered previous, next, current.");
                return Task.CompletedTask;
            });
        }));
        tests.Add(("display options and sortable headers use shared component semantics", async () =>
        {
            await WithTable(Parameters(pageSize: 1), (_, html) =>
            {
                var rendered = html();
                Require(rendered.Contains("f-data-display-options f-dropdown-panel", StringComparison.Ordinal), "The display selector did not use the shared dropdown panel.");
                Require(rendered.Contains("f-dropdown-item is-selected", StringComparison.Ordinal), "Visible columns did not expose the shared selected state.");
                Require(Regex.IsMatch(rendered, "<summary[^>]*class=\"f-dropdown-trigger\"[^>]*>Display\\s*<span[^>]*f-expansion-indicator"), "Display options lost the shared trigger or expansion marker.");
                Require(rendered.Contains("f-button-quiet", StringComparison.Ordinal)
                    && rendered.Contains("f-data-sort-label", StringComparison.Ordinal)
                    && rendered.Contains("f-data-sort-text", StringComparison.Ordinal),
                    "Sortable headers did not use Quiet buttons with a local label interaction boundary.");
                return Task.CompletedTask;
            });
        }));
        tests.Add(("search and both page-size selectors use the standard SelectBox style hooks", async () =>
        {
            var parameters = Parameters();
            parameters[nameof(DataTable<Row>.Searchable)] = true;
            await WithTable(parameters, (_, html) =>
            {
                var selects = Regex.Matches(html(), @"<select\b[^>]*>").Select(match => match.Value).ToArray();
                Require(selects.Length == 3 && selects.All(tag => Attribute(tag, "class") == "f-input f-select"),
                    "Search and top/bottom page-size selectors are not styled as standard SelectBox controls.");
                return Task.CompletedTask;
            });
        }));
        tests.Add(("item range supports localized spacing and unframed totals for populated and empty tables", async () =>
        {
            var parameters = Parameters();
            parameters[nameof(DataTable<Row>.Text)] = new TableText(Items: "项", Total: "共", ItemRangeFormat: "{0}{1}-{2} / {3}{4}");
            await WithTable(parameters, (_, html) =>
            {
                var count = Regex.Match(WebUtility.HtmlDecode(html()), @"<div class=""f-data-count"">([\s\S]*?)</div>").Groups[1].Value;
                Require(count == "<span>项1-10 / 共25</span>", "The localized item range contained extra frames or incorrect spacing.");
                return Task.CompletedTask;
            });
            parameters[nameof(DataTable<Row>.Items)] = Array.Empty<Row>();
            await WithTable(parameters, (_, html) =>
            {
                Require(WebUtility.HtmlDecode(html()).Contains("<span>项0-0 / 共0</span>", StringComparison.Ordinal),
                    "An empty successful table lost the zero item range.");
                return Task.CompletedTask;
            });
        }));
        tests.Add(("default and custom page sizes render in both synchronized selectors", async () =>
        {
            await WithTable(Parameters(), (_, html) =>
            {
                var rendered = html();
                var selectors = Tags(rendered, "select", "data-f-page-size");
                Require(selectors.Count == 2 && selectors.All(tag => Attribute(tag, "value") == "10"), "The two default page-size selectors are not synchronized at 10.");
                Require(RowCount(rendered) == 10, "The default page size did not limit the first page to 10 records.");
                return Task.CompletedTask;
            });
            await WithTable(Parameters(pageSize: 1), (_, html) =>
            {
                var rendered = html();
                var selectors = Tags(rendered, "select", "data-f-page-size");
                Require(selectors.Count == 2 && selectors.All(tag => Attribute(tag, "value") == "1"), "A nonstandard consumer page size was not selected in both controls.");
                var customOptions = Regex.Matches(rendered, @"(<option\b[^>]*>)([\s\S]*?)</option>")
                    .Count(match => Attribute(match.Groups[1].Value, "value") == "1"
                        && WebUtility.HtmlDecode(match.Groups[2].Value) == "1");
                Require(customOptions == 2, "A nonstandard consumer page size was omitted from the available choices.");
                return Task.CompletedTask;
            });
        }));
        tests.Add(("changing either page-size selector resets paging and raises the bind callback", async () =>
        {
            var changed = 0;
            var parameters = Parameters(pageSize: 10);
            parameters[nameof(DataTable<Row>.PageSizeChanged)] = EventCallback.Factory.Create<int>(new object(), value => changed = value);
            await WithTable(parameters, async (table, html) =>
            {
                await table.ChangeAsync("data-f-page", null, "3");
                Require(Tags(html(), "input", "data-f-page").All(tag => Attribute(tag, "value") == "3"), "The setup did not reach the last page.");
                await table.ChangeAsync("data-f-page-size", null, "20");
                var rendered = html();
                Require(changed == 20, "PageSizeChanged did not receive the selected value.");
                Require(Tags(rendered, "select", "data-f-page-size").All(tag => Attribute(tag, "value") == "20"), "Top and bottom page-size controls diverged.");
                Require(Tags(rendered, "input", "data-f-page").All(tag => Attribute(tag, "value") == "1"), "Changing page size did not reset the current page to one.");
                Require(RowCount(rendered) == 20, "Changing page size did not rebuild the visible records.");
            });
        }));
        tests.Add(("hiding the last data column transfers the uncapped marker without involving spacer or actions", async () =>
        {
            await WithTable(Parameters(pageSize: 1, withActions: true), async (table, html) =>
            {
                Require(html().Contains("data-f-column=\"detail\" data-f-last-column=\"true\"", StringComparison.Ordinal), "The initial last visible data column was not marked.");
                await table.ChangeAsync("data-f-display-column", "detail", "false");
                var rendered = html();
                Require(rendered.Contains("data-f-column=\"name\" data-f-last-column=\"true\"", StringComparison.Ordinal), "Hiding the last column did not transfer the uncapped marker.");
                Require(!Regex.IsMatch(rendered, @"<col\b[^>]*data-f-column=""detail"""), "The hidden data column remained in the sizing colgroup.");
                Require(!rendered.Contains("class=\"f-data-spacer\" data-f-last-column", StringComparison.Ordinal), "The spacer was treated as a data column.");
                Require(!rendered.Contains("class=\"f-data-actions\" data-f-last-column", StringComparison.Ordinal), "The action column was treated as a data column.");
            });
        }));
    }

    private static Dictionary<string, object?> Parameters(int? pageSize = null, bool withActions = false)
    {
        var parameters = new Dictionary<string, object?>
        {
            [nameof(DataTable<Row>.Items)] = Enumerable.Range(1, 25).Select(index => new Row($"Name {index}", $"Detail {index}")).ToArray(),
            [nameof(DataTable<Row>.Columns)] = new[]
            {
                new TableColumn<Row>("name", "Name", row => row.Name),
                new TableColumn<Row>("detail", "Detail", row => row.Detail)
            },
            [nameof(DataTable<Row>.Actions)] = withActions
                ? new[] { new RowAction<Row>("Open", _ => Task.CompletedTask) }
                : Array.Empty<RowAction<Row>>(),
            [nameof(DataTable<Row>.Searchable)] = false
        };
        if (pageSize is not null) parameters[nameof(DataTable<Row>.PageSize)] = pageSize.Value;
        return parameters;
    }

    private static async Task WithTable(Dictionary<string, object?> parameters, Func<DataTableProbe, Func<string>, Task> verify)
    {
        var activator = new CapturingActivator();
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton<NavigationManager>(new TestNavigation());
        services.AddSingleton<IJSRuntime, NoopJsRuntime>();
        services.AddSingleton<IComponentActivator>(activator);
        services.AddFlourishFramework();
        using var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();
        await using var renderer = new HtmlRenderer(scope.ServiceProvider, provider.GetRequiredService<ILoggerFactory>());
        await renderer.Dispatcher.InvokeAsync(async () =>
        {
            var output = await renderer.RenderComponentAsync<DataTableProbe>(ParameterView.FromDictionary(parameters));
            await verify(activator.Table ?? throw new InvalidOperationException("The renderer did not construct the data table."), output.ToHtmlString);
        });
    }

    private static List<string> Tags(string html, string name, string attribute) =>
        Regex.Matches(html, $@"<{name}\b[^>]*\b{Regex.Escape(attribute)}(?:\s|=|>)[^>]*>").Select(match => match.Value).ToList();

    private static string Attribute(string tag, string name)
    {
        var match = Regex.Match(tag, $"\\b{Regex.Escape(name)}=\"([^\"]*)\"");
        return match.Success ? WebUtility.HtmlDecode(match.Groups[1].Value) : string.Empty;
    }

    private static int RowCount(string html)
    {
        var body = Regex.Match(html, @"<tbody>[\s\S]*?</tbody>");
        return body.Success ? Regex.Matches(body.Value, @"<tr\b").Count : 0;
    }

    private static void Require(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }

    private sealed record Row(string Name, string Detail);

    private sealed class CapturingActivator : IComponentActivator
    {
        internal DataTableProbe? Table { get; private set; }
        public IComponent CreateInstance(Type componentType)
        {
            var component = (IComponent)Activator.CreateInstance(componentType)!;
            if (component is DataTableProbe table) Table = table;
            return component;
        }
    }

    private sealed class DataTableProbe : DataTable<Row>
    {
        public DataTableProbe() { }

#pragma warning disable BL0006
        internal Task ChangeAsync(string marker, string? expected, string value)
        {
            using var builder = new RenderTreeBuilder();
            base.BuildRenderTree(builder);
            var frames = builder.GetFrames();
            for (var markerIndex = 0; markerIndex < frames.Count; markerIndex++)
            {
                var markerFrame = frames.Array[markerIndex];
                if (markerFrame.FrameType != RenderTreeFrameType.Attribute || markerFrame.AttributeName != marker) continue;
                if (expected is not null && !string.Equals(Convert.ToString(markerFrame.AttributeValue), expected, StringComparison.Ordinal)) continue;
                var elementIndex = markerIndex - 1;
                while (elementIndex >= 0 && frames.Array[elementIndex].FrameType == RenderTreeFrameType.Attribute) elementIndex--;
                if (elementIndex < 0 || frames.Array[elementIndex].FrameType != RenderTreeFrameType.Element) continue;
                var end = elementIndex + frames.Array[elementIndex].ElementSubtreeLength;
                for (var index = elementIndex + 1; index < end && frames.Array[index].FrameType == RenderTreeFrameType.Attribute; index++)
                {
                    var frame = frames.Array[index];
                    if (frame.AttributeName != "onchange") continue;
                    var args = new ChangeEventArgs { Value = value };
                    return frame.AttributeValue switch
                    {
                        EventCallback<ChangeEventArgs> callback => callback.InvokeAsync(args),
                        EventCallback callback => callback.InvokeAsync(args),
                        MulticastDelegate callback => ((IHandleEvent)this).HandleEventAsync(new EventCallbackWorkItem(callback), args),
                        _ => throw new InvalidOperationException($"The {marker} control has no invocable change callback.")
                    };
                }
            }
            throw new InvalidOperationException($"The {marker} control was not rendered.");
        }
#pragma warning restore BL0006
    }

    private sealed class NoopJsRuntime : IJSRuntime
    {
        public ValueTask<TValue> InvokeAsync<TValue>(string identifier, object?[]? args) => ValueTask.FromResult(default(TValue)!);
        public ValueTask<TValue> InvokeAsync<TValue>(string identifier, CancellationToken cancellationToken, object?[]? args) => ValueTask.FromResult(default(TValue)!);
    }
}
