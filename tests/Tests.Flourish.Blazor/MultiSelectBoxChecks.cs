using System.Globalization;
using System.Net;
using System.Reflection;
using System.Text.Json;
using System.Text.RegularExpressions;
using ArkheideSystem.Flourish.Blazor;
using ArkheideSystem.Flourish.Blazor.Components;
using ArkheideSystem.Flourish.Blazor.Abstract;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.Web.HtmlRendering;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;

internal static class MultiSelectBoxChecks
{
    internal static void Register(List<(string Name, Func<Task> Run)> tests)
    {
        tests.Add(("generic multi-selection defaults to empty membership and refuses excessive snapshots", async () =>
        {
            await using var f = new Fixture(); var calls = 0; MultiSelectChange? adopted = null;
            var output = await f.Render<MultiSelectBox>(new()
            {
                ["Items"] = new MultiSelectOption[] { new("one","One"), new("two","Two") },
                ["MaximumSelections"] = 1,
                ["Changed"] = EventCallback.Factory.Create<MultiSelectChange>(f, change => { calls++; adopted = change; })
            });
            var selection = f.Activator.Components.OfType<MultiSelectBox>().Single();
            Require(!selection.CanReorder && !selection.Searchable && selection.MinimumSelected == 0
                && selection.Items.All(item => !item.Selected), "General selection defaults inherited the display-only contract.");
            Require(!(await f.Read(output)).Contains("draggable=\"true\""), "General selection reordered by default.");
            await f.Dispatch(async () =>
            {
                await selection.ApplyAsync(["one","two"], ["one","two"]);
                await selection.ApplyAsync(["one","two"], ["one","one"]);
                await selection.ApplyAsync(["two","one"], ["one"]);
                Require(calls == 0, "Excessive, duplicate or reordered default selection escaped validation.");
                await selection.ApplyAsync(["one","two"], ["two"]);
            });
            Require(calls == 1 && adopted is not null && adopted.SelectedKeys.SetEquals(["two"])
                && adopted.OrderedKeys.SequenceEqual(["one","two"]), "The shared callback lost ordered or selected identity.");
        }));
        tests.Add(("multi-selection reorder accessibility labels resolve actual Blazor catalog translations", async () =>
        {
            foreach (var (culture, expected) in new[] { ("zh-CN", "调整 Account <one> 的顺序"), ("pt-BR", "Reordenar Account <one>") })
            {
                await using var f = new Fixture(texts: new CatalogTexts(culture));
                var output = await f.Render<MultiSelectBox>(new()
                {
                    ["Items"] = new MultiSelectOption[] { new("account", "Account <one>") }, ["CanReorder"] = true
                });
                var html = await f.Read(output);
                var row = Regex.Match(html, @"<div\b[^>]*data-f-selection-key=""account""[^>]*>").Value;
                Require(WebUtility.HtmlDecode(row).Contains("aria-label=\"" + expected + "\"", StringComparison.Ordinal),
                    "The current Blazor catalog did not translate the actual reorder target for " + culture);
                Require(!row.Contains("Reorder ", StringComparison.Ordinal), "The reorder target silently used its English fallback.");
            }
        }));
        tests.Add(("record tables and charts instantiate one real display renderer and contract", async () =>
        {
            foreach (var chart in new[] { false, true })
            {
                await using var f = new Fixture();
                var output = chart ? await f.Render<LineChart>(Chart())
                    : await f.Render<DataTable<string>>(new()
                    {
                        ["Items"] = new[] { "Record" }, ["Columns"] = new TableColumn<string>[] { new("name","Name",value=>value),new("detail","Detail",value=>value) }
                    });
                var display = f.Activator.Components.OfType<MultiSelectBox>().Single();
                var html = await f.Read(output);
                Require(display.Items.Count == 2 && Regex.Matches(html,"class=\"f-multi-select-box\"").Count == 1,
                    "A consumer generated a second display renderer.");
                Require(html.Contains("f-multi-select-panel f-dropdown-panel f-menu-panel") && html.Contains("popover=\"manual\"")
                    && Regex.Matches(html,"draggable=\"true\"").Count == 2, "The actual standard panel or complete-row targets were absent.");
                Require(!html.Contains("f-display-options") && !html.Contains("f-data-display"), "A retired display family survived.");
            }
        }));
        tests.Add(("display snapshots reject fixed hidden disabled unknown duplicate and missing members", async () =>
        {
            await using var f = new Fixture(); var calls = 0;
            var output = await f.Render<MultiSelectBox>(new()
            {
                ["Items"] = new MultiSelectOption[] { new("name","Name",Selected:true,CanDeselect:false,CanReorder:false),new("count","Count",Selected:true),new("locked","Locked",Selected:true,Disabled:true) },
                ["MinimumSelected"] = 1, ["CanReorder"] = true, ["Changed"] = EventCallback.Factory.Create<MultiSelectChange>(f, _=>calls++)
            });
            var display = f.Activator.Components.OfType<MultiSelectBox>().Single();
            await f.Dispatch(async () =>
            {
                await display.ApplyAsync(["name","count","locked"],["name","locked"]);
                Require(calls == 1,"A valid standard display snapshot was lost.");
                await display.ApplyAsync(["count","name","locked"],["name","count","locked"]);
                await display.ApplyAsync(["name","count","locked"],["count","locked"]);
                await display.ApplyAsync(["name","count","locked"],["name","count"]);
                await display.ApplyAsync(["name","name","locked"],["name","locked"]);
                await display.ApplyAsync(["name","count","locked"],["name","unknown"]);
                await display.ApplyAsync(["name","count"],["name","count","locked"]);
                Require(calls == 1,"An invalid display snapshot escaped the production component.");
                await display.SetParametersAsync(ParameterView.FromDictionary(new Dictionary<string,object?> { ["Disabled"] = true }));
                await display.ApplyAsync(["name","count","locked"],["name","locked"]);
                Require(calls == 1,"A delayed event changed a disabled display.");
            });
            Require((await f.Read(output)).Contains("inert"),"The disabled display did not block native activation.");
        }));
        tests.Add(("display minimum visible and cards reorder limits protect the same actual component", async () =>
        {
            await using var f = new Fixture(); var calls = 0;
            await f.Render<MultiSelectBox>(new()
            {
                ["Items"] = new MultiSelectOption[] { new("one","One",Selected:true),new("two","Two",Selected:false) },
                ["MinimumSelected"] = 1, ["CanReorder"] = false,
                ["Changed"] = EventCallback.Factory.Create<MultiSelectChange>(f,_=>calls++)
            });
            var display = f.Activator.Components.OfType<MultiSelectBox>().Single();
            await f.Dispatch(async () =>
            {
                await display.ApplyAsync(["one","two"],[]);
                await display.ApplyAsync(["two","one"],["one"]);
                Require(calls == 0,"Minimum visibility or Cards ordering was bypassed.");
                await display.ApplyAsync(["one","two"],["two"]);
                Require(calls == 1,"Turning off reorder incorrectly disabled valid visibility selection.");
            });
        }));
        tests.Add(("chart display selection and order survive real new week data without changing series identity", async () =>
        {
            await using var f = new Fixture(); var output = await f.Render<LineChart>(Chart());
            var display = f.Activator.Components.OfType<MultiSelectBox>().Single();
            var chart = f.Activator.Components.OfType<LineChart>().Single();
            await f.Dispatch(()=>display.ApplyAsync(["amount","count"],["count"]));
            var html = await f.Read(output);
            Require(html.IndexOf("data-f-selection-key=\"amount\"") < html.IndexOf("data-f-selection-key=\"count\"")
                && html.Contains("<g data-series=\"0\"") && !html.Contains("<g data-series=\"1\""),"The chart did not accept standard visibility and order.");
            var parameters = Chart(); parameters["Series"] = new LineChart.DataSeries[] { new("count","Count",[9],value=>value.ToString("0",System.Globalization.CultureInfo.InvariantCulture)),new("amount","Amount",[700]) };
            await f.Dispatch(()=>chart.SetParametersAsync(ParameterView.FromDictionary(parameters)));
            html = await f.Read(output);
            Require(html.Contains("<td>9</td>") && !html.Contains("<td>700") && !html.Contains("<g data-series=\"1\"")
                && html.IndexOf("data-f-selection-key=\"amount\"") < html.IndexOf("data-f-selection-key=\"count\""),"New week values reset display state or exposed a hidden series.");
            await f.Dispatch(()=>display.ApplyAsync(["amount","count"],[]));
            html = await f.Read(output);
            Require(!html.Contains("<svg") && !html.Contains("<table") && html.Contains("f-multi-select-panel"),"An empty chart selection hid its recovery control or invented values.");
        }));
        tests.Add(("busy table display refuses deferred visibility and reorder without losing record data", async () =>
        {
            await using var f = new Fixture();
            var output = await f.Render<DataTable<string>>(new()
            {
                ["Items"] = new[] { "Record" }, ["Columns"] = new TableColumn<string>[] { new("name","Name",value=>value),new("detail","Detail",value=>value) },
                ["EditingBusy"] = true
            });
            var display = f.Activator.Components.OfType<MultiSelectBox>().Single();
            await f.Dispatch(()=>display.ApplyAsync(["detail","name"],["name"]));
            var html = await f.Read(output);
            Require(display.Disabled && html.Contains("data-f-column=\"detail\"") && html.Contains("Record"),"A busy table changed its display or lost its record.");
        }));
        tests.Add(("display lifecycle coalesces imports and releases the current menu and proxy exactly once", async () =>
        {
            var js = new Browser();
            await using var f = new Fixture(js); await f.Render<MultiSelectBox>(new() { ["Items"] = new MultiSelectOption[] {new("one","One")} });
            var display = f.Activator.Components.OfType<MultiSelectBox>().Single();
            await f.Dispatch(async () => { await AfterRender(display); await AfterRender(display); await display.DisposeAsync(); await display.DisposeAsync(); await AfterRender(display); });
            Require(js.Imports == 1 && js.Module.Calls.Count(name=>name=="synchronize")==2
                && js.Module.Calls.Count(name=>name=="detach")==1 && js.Module.Disposals==1,"The standard display retained or duplicated browser lifetime ownership.");
        }));
        tests.Add(("a delayed display import cannot attach after its component is disposed", async () =>
        {
            var js = new Browser(delayed:true);
            await using var f = new Fixture(js); await f.Render<MultiSelectBox>(new() { ["Items"] = new MultiSelectOption[] {new("one","One")} });
            var display = f.Activator.Components.OfType<MultiSelectBox>().Single();
            await f.Dispatch(async () =>
            {
                var first = AfterRender(display); var second = AfterRender(display);
                await display.DisposeAsync(); js.Complete(); await Task.WhenAll(first,second);
            });
            Require(js.Imports==1 && js.Module.Calls.Count==0 && js.Module.Disposals==1,"A late module installed a discarded display controller.");
        }));
    }
    private static Dictionary<string,object?> Chart() => new()
    {
        ["Title"] = "Weekly values", ["Labels"] = new LineChart.PointLabel[] {new("mon","Monday")},
        ["Series"] = new LineChart.DataSeries[] {new("count","Count",[2]),new("amount","Amount",[100])}
    };
    private static Task AfterRender(MultiSelectBox component) =>
        (Task)typeof(MultiSelectBox).GetMethod("OnAfterRenderAsync",BindingFlags.Instance|BindingFlags.NonPublic)!.Invoke(component,[false])!;
    private static void Require(bool condition,string message) { if (!condition) throw new InvalidOperationException(message); }
    private sealed class Fixture : IAsyncDisposable
    {
        private readonly ServiceProvider Provider;
        private readonly IServiceScope Scope;
        private readonly HtmlRenderer Renderer;
        internal Capture Activator {get;} = new();
        internal Fixture(IJSRuntime? js=null, ITextProvider? texts=null)
        {
            var services=new ServiceCollection(); services.AddLogging(); services.AddSingleton<IComponentActivator>(Activator);
            services.AddSingleton<IJSRuntime>(js ?? new Browser());
            services.AddSingleton<NavigationManager>(new TestNavigation());
            services.AddFlourishFramework();
            if (texts is not null) services.AddSingleton<ITextProvider>(texts);
            Provider=services.BuildServiceProvider(); Scope=Provider.CreateScope();
            Renderer=new HtmlRenderer(Scope.ServiceProvider,Provider.GetRequiredService<ILoggerFactory>());
        }
        internal Task<HtmlRootComponent> Render<T>(Dictionary<string,object?> parameters) where T:IComponent =>
            Renderer.Dispatcher.InvokeAsync(()=>Renderer.RenderComponentAsync<T>(ParameterView.FromDictionary(parameters)));
        internal Task Dispatch(Func<Task> work) => Renderer.Dispatcher.InvokeAsync(work);
        internal Task<string> Read(HtmlRootComponent output) => Renderer.Dispatcher.InvokeAsync(output.ToHtmlString);
        public async ValueTask DisposeAsync() {await Renderer.DisposeAsync(); Scope.Dispose(); Provider.Dispose();}
    }
    private sealed class CatalogTexts : ITextProvider
    {
        private readonly JsonElement Catalog;
        internal CatalogTexts(string culture)
        {
            Culture = culture;
            using var stream = typeof(MultiSelectBox).Assembly.GetManifestResourceStream("Flourish.Blazor.Texts.json")
                ?? throw new InvalidOperationException("The actual Blazor text catalog was not packaged.");
            using var parsed = JsonDocument.Parse(stream);
            Catalog = parsed.RootElement.Clone();
        }
        public string Culture { get; }
        public CultureInfo FormatCulture => CultureInfo.GetCultureInfo(Culture);
        public event EventHandler? Changed { add { } remove { } }
        public string Get(TextReference text, params object?[] arguments)
        {
            var value = text.CatalogId == "Flourish" && Catalog.TryGetProperty(text.Token, out var translations)
                && translations.TryGetProperty(Culture, out var translated) ? translated.GetString()! : text.FallbackText ?? text.Token;
            return arguments.Length == 0 ? value : string.Format(FormatCulture, value, arguments);
        }
    }
    private sealed class Capture:IComponentActivator
    {
        internal List<IComponent> Components {get;}=[];
        public IComponent CreateInstance(Type type) {var component=(IComponent)System.Activator.CreateInstance(type)!; Components.Add(component); return component;}
    }
    private sealed class Browser(bool delayed=false):IJSRuntime
    {
        private readonly TaskCompletionSource<IJSObjectReference> Completion=new(TaskCreationOptions.RunContinuationsAsynchronously);
        internal BrowserModule Module {get;}=new(); internal int Imports;
        internal void Complete()=>Completion.TrySetResult(Module);
        public ValueTask<T> InvokeAsync<T>(string name,object?[]? args)
        {
            if(name!="import")throw new InvalidOperationException(name); Imports++;
            return delayed ? new ValueTask<T>(Await<T>()) : ValueTask.FromResult((T)(object)Module);
        }
        private async Task<T> Await<T>()=>(T)(object)await Completion.Task;
        public ValueTask<T> InvokeAsync<T>(string name,CancellationToken token,object?[]? args)=>InvokeAsync<T>(name,args);
    }
    private sealed class BrowserModule:IJSObjectReference
    {
        internal List<string> Calls {get;}=[]; internal int Disposals;
        public ValueTask<T> InvokeAsync<T>(string name,object?[]? args) {Calls.Add(name); return ValueTask.FromResult(default(T)!);}
        public ValueTask<T> InvokeAsync<T>(string name,CancellationToken token,object?[]? args)=>InvokeAsync<T>(name,args);
        public ValueTask DisposeAsync() {Disposals++; return ValueTask.CompletedTask;}
    }
}
