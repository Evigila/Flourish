using System.Net;
using System.Reflection;
using System.Text.RegularExpressions;
using ArkheideSystem.Flourish.Blazor;
using ArkheideSystem.Flourish.Blazor.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.RenderTree;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using Primitive = ArkheideSystem.Flourish.Blazor.Components.Primitives;

internal static class DataSearchChecks
{
    private static readonly TableColumn<string>[] Columns = [new("first", "First <field>", value => value), new("hidden", "Not searchable", value => value, Searchable:false), new("second", "Second", value => value)];
    internal static void Register(List<(string Name, Func<Task> Run)> tests)
    {
        tests.Add(("canonical column search renders actual standard fields and native controller markers", async () =>
        {
            await WithComponent<DataSearch<string>>(new() { ["Columns"] = Columns, ["Id"] = "product-search", ["MaximumLength"] = 12, ["Value"] = "host query" }, (_, components, html) =>
            {
                var output=html();
                Require(components.OfType<Field>().Count() == 2 && components.OfType<StandaloneSelectBox<string>>().Count() == 1 && components.OfType<StandaloneTextBox>().Count() == 1, "Search bypassed the standard Field or input components.");
                Require(output.Contains("for=\"product-search-column\"", StringComparison.Ordinal) && output.Contains("id=\"product-search-column\"", StringComparison.Ordinal)
                    && output.Contains("for=\"product-search\"", StringComparison.Ordinal) && output.Contains("id=\"product-search\"", StringComparison.Ordinal), "Field captions do not identify their real inputs.");
                Require(output.Contains("data-f-search-column", StringComparison.Ordinal) && output.Contains("data-f-search-query", StringComparison.Ordinal), "Progressive controller markers were lost.");
                Require(output.Contains("type=\"search\"", StringComparison.Ordinal) && output.Contains("maxlength=\"12\"", StringComparison.Ordinal) && output.Contains("autocomplete=\"off\"", StringComparison.Ordinal), "Native search semantics or maximum length changed.");
                Require(!output.Contains("Not searchable", StringComparison.Ordinal) && output.Contains("First &lt;field&gt;", StringComparison.Ordinal) && !output.Contains("First <field>", StringComparison.Ordinal), "Unavailable columns or raw host markup reached the selector.");
                Require(!output.Contains("All columns", StringComparison.Ordinal), "The first-column contract silently enabled all-column search.");
                return Task.CompletedTask;
            });
        }));
        tests.Add(("canonical search defaults to the first searchable key and preserves controlled query values", async () =>
        {
            var calls = new List<TableSearchRequest>();
            await WithComponent<DataSearch<string>>(new() { ["Columns"] = Columns, ["ColumnKey"] = "hidden", ["Value"] = "retained", ["Changed"] = Callback(calls) }, async (search, _, _) =>
            {
                await Invoke(search, "ChangeValueAsync", "new");
                Require(calls.SequenceEqual([new TableSearchRequest("first", "new")]), "An unavailable incoming key did not fall back to the first searchable column.");
                await Invoke(search, "ChangeColumnAsync", "second");
                Require(calls[^1] == new TableSearchRequest("second", "retained") && search.Value == "retained" && search.ColumnKey == "hidden", "The component mutated host-owned query parameters.");
            });
        }));
        tests.Add(("all-column search is explicit and uses a nullable shared key rather than a second query contract", async () =>
        {
            var calls = new List<TableSearchRequest>();
            await WithComponent<DataSearch<string>>(new() { ["Columns"] = Columns, ["IncludeAllColumns"] = true, ["Changed"] = Callback(calls) }, async (search, _, html) =>
            {
                Require(html().Contains("All columns", StringComparison.Ordinal) && html().Contains(">Content</label>", StringComparison.Ordinal), "The explicit all-column label or content field disappeared.");
                await Invoke(search, "ChangeValueAsync", "all"); await Invoke(search, "ChangeColumnAsync", "");
                Require(calls.Count == 2 && calls[0] == new TableSearchRequest(null, "all") && calls[1] == new TableSearchRequest(null, ""), "All-column selection did not use the canonical null key.");
            });
        }));
        tests.Add(("disabled and empty canonical searches suppress even forged callback invocations", async () =>
        {
            foreach (var empty in new[] { false, true })
            {
                var calls = new List<TableSearchRequest>();
                await WithComponent<DataSearch<string>>(new() { ["Columns"] = empty ? Array.Empty<TableColumn<string>>() : Columns, ["Disabled"] = !empty, ["Changed"] = Callback(calls) }, async (search, _, html) =>
                {
                    await Invoke(search, "ChangeValueAsync", "forged"); await Invoke(search, "ChangeColumnAsync", "first");
                    Require(calls.Count == 0, "A disabled or empty search forwarded a query.");
                    Require(empty ? !html().Contains("<input", StringComparison.Ordinal) : html().Contains(" disabled", StringComparison.Ordinal), "Disabled/empty native behavior was not preserved.");
                });
            }
        }));
        tests.Add(("canonical search rejects nonsearchable unknown and forbidden all-column selections", async () =>
        {
            var calls = new List<TableSearchRequest>();
            await WithComponent<DataSearch<string>>(new() { ["Columns"] = Columns, ["Changed"] = Callback(calls) }, async (search, _, _) =>
            {
                foreach (var key in new[] { "hidden", "unknown", "" }) await Invoke(search, "ChangeColumnAsync", key);
                Require(calls.Count == 0, "A forged column option changed the query scope.");
            });
        }));
        tests.Add(("canonical search enforces callback length without splitting a UTF16 surrogate pair", async () =>
        {
            var calls = new List<TableSearchRequest>();
            await WithComponent<DataSearch<string>>(new() { ["Columns"] = Columns, ["MaximumLength"] = 2, ["Changed"] = Callback(calls) }, async (search, _, _) =>
            {
                await Invoke(search, "ChangeValueAsync", "abcd"); await Invoke(search, "ChangeValueAsync", "A😀B"); await Invoke(search, "ChangeValueAsync", null);
                Require(calls.Select(call => call.Value).SequenceEqual(["ab", "A", ""]), "Length enforcement lost Unicode boundaries or the empty-query request.");
            });
        }));
        tests.Add(("canonical search permits host refusal and rerenders the controlled input state", async () =>
        {
            var calls = new List<TableSearchRequest>();
            await WithComponent<DataSearch<string>>(new() { ["Columns"] = Columns, ["ColumnKey"] = "first", ["Value"] = "kept", ["Changed"] = Callback(calls) }, async (_, components, html) =>
            {
                var input = components.OfType<StandaloneTextBox>().Single();
                await Invoke(input, "ChangeAsync", new ChangeEventArgs { Value = "rejected" });
                var selector = components.OfType<StandaloneSelectBox<string>>().Single();
                await Invoke(selector, "ChangeAsync", new ChangeEventArgs { Value = "second" });
                Require(calls.SequenceEqual([new TableSearchRequest("first", "rejected"), new TableSearchRequest("second", "kept")]), "The shared inputs changed callback semantics.");
                Require(html().Contains("value=\"kept\"", StringComparison.Ordinal) && html().Contains("value=\"first\"", StringComparison.Ordinal), "Refused controlled values remained optimistic rather than resetting.");
            });
        }));
        tests.Add(("canonical search renders the same-row standard filter slot without another search family", async () =>
        {
            RenderFragment slot = builder => { builder.OpenComponent<StandaloneCheckBox>(0); builder.AddAttribute(1, nameof(StandaloneCheckBox.Label), "Include archived"); builder.CloseComponent(); };
            await WithComponent<DataSearch<string>>(new() { ["Columns"] = Columns, ["ChildContent"] = slot, ["Class"] = "business-filter", ["AdditionalAttributes"] = new Dictionary<string, object> { ["data-test"] = "search" } }, (_, components, html) =>
            {
                Require(components.OfType<StandaloneCheckBox>().Count() == 1 && html().Contains("f-data-search-content", StringComparison.Ordinal) && html().Contains("Include archived", StringComparison.Ordinal), "The filter slot was detached from the search row.");
                Require(html().Contains("f-data-search business-filter", StringComparison.Ordinal) && html().Contains("data-test=\"search\"", StringComparison.Ordinal), "Composition attributes were lost.");
                return Task.CompletedTask;
            });
        }));
        tests.Add(("canonical search isolates search and filter events from ancestor editing forms", async () =>
        {
            var trace = new List<string>();
            RenderFragment slot = builder => { builder.OpenComponent<StandaloneCheckBox>(0); builder.AddAttribute(1, nameof(StandaloneCheckBox.Label), "Include archived"); builder.AddAttribute(2, nameof(StandaloneCheckBox.ValueChanged), EventCallback.Factory.Create<bool>(trace, (bool value) => trace.Add("filter:" + value))); builder.CloseComponent(); };
            await WithComponent<DataSearch<string>>(new() { ["Columns"] = Columns, ["ChildContent"] = slot, ["Changed"] = EventCallback.Factory.Create<TableSearchRequest>(trace, (TableSearchRequest request) => trace.Add("search:" + request.ColumnKey + ":" + request.Value)) }, async (search, components, _) =>
            {
                Require(Isolated(search, "oninput") && Isolated(search, "onchange"), "Search/filter events may reach an ancestor editing form and create a false dirty draft.");
                await Invoke(components.OfType<StandaloneTextBox>().Single(), "ChangeAsync", new ChangeEventArgs { Value = "needle" });
                await Invoke(components.OfType<StandaloneSelectBox<string>>().Single(), "ChangeAsync", new ChangeEventArgs { Value = "second" });
                await Invoke(components.OfType<StandaloneCheckBox>().Single(), "ChangeAsync", new ChangeEventArgs { Value = true });
                Require(trace.SequenceEqual(["search:first:needle", "search:second:", "filter:True"]), "Event isolation suppressed or duplicated the intended controlled search/filter callbacks.");
            });
        }));
        tests.Add(("canonical search honors explicit shared TableText without interpreting host labels as tokens", async () =>
        {
            var text = new TableText(SearchBy:"Host by", AllColumns:"Host all", SearchContent:"Host query");
            await WithComponent<DataSearch<string>>(new() { ["Columns"] = Columns, ["IncludeAllColumns"] = true, ["Text"] = text }, (_, _, html) =>
            {
                var output=WebUtility.HtmlDecode(html());
                Require(output.Contains("Host by", StringComparison.Ordinal) && output.Contains("Host all", StringComparison.Ordinal) && output.Contains("Host query", StringComparison.Ordinal), "Explicit shared table labels were ignored.");
                return Task.CompletedTask;
            });
        }));
        tests.Add(("shared search CSS does not reskin labels or checkboxes in the host filter slot", () =>
        {
            var root = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../.."));
            var frameworkRoot = Path.Combine(root, "src", "Flourish.Blazor", "Flourish.Blazor.Framework", "wwwroot");
            var designRoot = Path.Combine(root, "src", "Flourish.Blazor", "Flourish.Blazor.Design", "wwwroot");
            var framework = File.ReadAllText(Path.Combine(frameworkRoot, "framework.css"));
            var design = File.ReadAllText(Path.Combine(designRoot, "data.css"));
            var shared = File.ReadAllText(Path.Combine(frameworkRoot, "data-search.css"));
            Require(!Regex.IsMatch(framework + design + shared, @"\.f-data-search\s+(?:label|input|select)\b"), "A broad search selector overrides production filter-slot labels or checkbox geometry.");
            Require(shared.Contains(".f-data-search > .f-field", StringComparison.Ordinal) && shared.Contains(".f-data-search .f-field-control > .f-input", StringComparison.Ordinal), "Search field sizing is no longer bounded to the standard field/input shell.");
            Require(shared.Contains(".f-data-search-content { display:flex", StringComparison.Ordinal), "The generic filter slot lost its same-row layout.");
            return Task.CompletedTask;
        }));
        tests.Add(("canonical search validates identifiers positive lengths and shared unique column keys", async () =>
        {
            foreach (var parameters in new Dictionary<string, object?>[]
            {
                new() { ["Id"] = " " }, new() { ["Id"] = "not valid" }, new() { ["MaximumLength"] = 0 },
                new() { ["Columns"] = new TableColumn<string>[] { new("same", "One", item => item), new("same", "Two", item => item) } },
                new() { ["Columns"] = new TableColumn<string>[] { new(" ", "One", item => item) } }
            })
            {
                var rejected=false;
                try { await WithComponent<DataSearch<string>>(parameters, (_, _, _) => Task.CompletedTask); } catch (ArgumentException) { rejected=true; }
                Require(rejected, "Invalid search state reached the native controls.");
            }
        }));
        tests.Add(("legacy column search is a true adapter to the same renderer and preserves its callback payload", async () =>
        {
            var requests = new List<Primitive.DataSearchRequest>();
            var oldColumns = new Primitive.DataColumn<string>[] { new("first", "First <field>", value => value), new("second", "Second", value => value) };
            await WithComponent<Primitive.DataSearch<string>>(new() { ["Columns"] = oldColumns, ["Value"] = "legacy", ["Changed"] = EventCallback.Factory.Create<Primitive.DataSearchRequest>(requests, (Primitive.DataSearchRequest request) => requests.Add(request)) }, async (_, components, html) =>
            {
                var search = components.OfType<DataSearch<string>>().Single();
                Require(components.OfType<Field>().Count() == 2 && html().Contains("f-data-search", StringComparison.Ordinal) && !html().Contains("quick-filter-field", StringComparison.Ordinal), "Legacy search retained its old independent renderer.");
                await Invoke(search, "ChangeValueAsync", "new");
                Require(requests.SequenceEqual([new Primitive.DataSearchRequest("first", "new")]), "Compatibility forwarding changed its first-column payload.");
            });
        }));
    }
    private static EventCallback<TableSearchRequest> Callback(List<TableSearchRequest> calls) => EventCallback.Factory.Create<TableSearchRequest>(calls, (TableSearchRequest request) => calls.Add(request));
    private static Task Invoke(object target, string method, object? value) => (Task)target.GetType().GetMethod(method, BindingFlags.NonPublic | BindingFlags.Instance)!.Invoke(target, [value])!;
#pragma warning disable BL0006
    private static bool Isolated(object target, string name)
    {
        using var builder = new RenderTreeBuilder();
        target.GetType().GetMethod("BuildRenderTree", BindingFlags.NonPublic | BindingFlags.Instance)!.Invoke(target, [builder]);
        var frames = builder.GetFrames();
        for (var index = 0; index < frames.Count; index++)
        {
            var frame = frames.Array[index];
            if (frame.FrameType == RenderTreeFrameType.Attribute && frame.AttributeName == "__internal_stopPropagation_" + name && frame.AttributeValue is true) return true;
        }
        return false;
    }
#pragma warning restore BL0006
    private static async Task WithComponent<T>(Dictionary<string, object?> parameters, Func<T, List<IComponent>, Func<string>, Task> verify) where T : IComponent
    {
        var activator=new CaptureActivator(); var services=new ServiceCollection(); services.AddLogging(); services.AddFlourishFramework(); services.AddSingleton<IJSRuntime>(new NoJs()); services.AddSingleton<NavigationManager>(new TestNavigation()); services.AddSingleton<IComponentActivator>(activator);
        using var provider=services.BuildServiceProvider(); await using var renderer=new HtmlRenderer(provider, provider.GetRequiredService<ILoggerFactory>());
        await renderer.Dispatcher.InvokeAsync(async () => { var output=await renderer.RenderComponentAsync<T>(ParameterView.FromDictionary(parameters)); await verify((T)activator.Components.First(component => component.GetType()==typeof(T)), activator.Components, output.ToHtmlString); });
    }
    private static void Require(bool condition, string message) { if(!condition) throw new InvalidOperationException(message); }
    private sealed class CaptureActivator : IComponentActivator { internal List<IComponent> Components {get;}=[]; public IComponent CreateInstance(Type type) { var component=(IComponent)Activator.CreateInstance(type)!; Components.Add(component); return component; } }
    private sealed class NoJs : IJSRuntime { public ValueTask<TValue> InvokeAsync<TValue>(string identifier, object?[]? args) => throw new InvalidOperationException("SSR invoked JavaScript."); public ValueTask<TValue> InvokeAsync<TValue>(string identifier, CancellationToken token, object?[]? args) => InvokeAsync<TValue>(identifier,args); }
}
