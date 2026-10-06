using System.Reflection;
using ArkheideSystem.Flourish.Blazor;
using ArkheideSystem.Flourish.Blazor.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;

internal static class NavigationControlsChecks
{
    internal static void Register(List<(string Name, Func<Task> Run)> tests)
    {
        tests.Add(("line charts own SVG, exact accessible values and encoded host data without browser imports", async () =>
        {
            var html = await Render<LineChart>(ChartParameters());
            Require(html.Contains("class=\"f-line-chart", StringComparison.Ordinal) && html.Contains("role=\"img\"", StringComparison.Ordinal), "The actual library chart renderer was not used.");
            Require(html.Contains("aria-labelledby=\"line-chart-", StringComparison.Ordinal) && html.Contains("aria-describedby=\"line-chart-", StringComparison.Ordinal), "The chart lost its title/description linkage.");
            Require(html.Contains("Count &lt;draft&gt;", StringComparison.Ordinal) && !html.Contains("Count <draft>", StringComparison.Ordinal), "A host label escaped Razor encoding.");
            Require(html.Contains("<table class=\"f-sr-only\">", StringComparison.Ordinal) && html.Contains("<th scope=\"row\">Monday</th><td>2 units</td><td>100 credits</td>", StringComparison.Ordinal), "The chart discarded original readable values.");
            Require(html.Contains("data-independent-scales=\"false\"", StringComparison.Ordinal) && html.Contains("points=\"40,241.8 720,239.6\"", StringComparison.Ordinal), "Common scale is not the default.");
        }));
        tests.Add(("independent chart scales are explicit and preserve separate units and maxima", async () =>
        {
            var parameters = ChartParameters(); parameters[nameof(LineChart.IndependentScales)] = true;
            var html = await Render<LineChart>(parameters);
            Require(html.Contains("data-independent-scales=\"true\"", StringComparison.Ordinal) && html.Contains("points=\"40,134 720,24\"", StringComparison.Ordinal), "Independent quantity scaling changed.");
            Require(html.Contains("Maximum 4 units", StringComparison.Ordinal) && html.Contains("Maximum 200 credits", StringComparison.Ordinal), "The scale legend lost the formatter or true maxima.");
        }));
        tests.Add(("hidden chart series preserve palette identities and an empty selection does not invent data", async () =>
        {
            var parameters = ChartParameters();
            parameters[nameof(LineChart.Series)] = new LineChart.DataSeries[] { new("count", "Hidden", [2, 4], Visible:false), new("amount", "Visible", [100, 200]) };
            var html = await Render<LineChart>(parameters);
            Require(html.Contains("data-series=\"1\"", StringComparison.Ordinal) && !html.Contains("Hidden", StringComparison.Ordinal), "Toggling visibility reindexed a series or leaked hidden values.");
            parameters[nameof(LineChart.Series)] = new LineChart.DataSeries[] { new("count", "Hidden", [2, 4], Visible:false) };
            parameters[nameof(LineChart.EmptyText)] = "Select a series";
            html = await Render<LineChart>(parameters);
            Require(html.Contains("Select a series", StringComparison.Ordinal) && !html.Contains("<svg", StringComparison.Ordinal) && !html.Contains("<table", StringComparison.Ordinal), "An empty selection rendered fictional series.");
        }));
        tests.Add(("chart zero single-point negative and extreme decimal data remain finite and readable", async () =>
        {
            var parameters = ChartParameters();
            parameters[nameof(LineChart.Labels)] = new LineChart.PointLabel[] { new("one", "One") };
            parameters[nameof(LineChart.Series)] = new LineChart.DataSeries[] { new("zero", "Zero", [0]) };
            var html = await Render<LineChart>(parameters);
            Require(html.Contains("points=\"380,244\"", StringComparison.Ordinal), "The single zero point was divided by zero or placed at a false edge.");
            parameters[nameof(LineChart.Labels)] = new LineChart.PointLabel[] { new("one", "One"), new("two", "Two") };
            parameters[nameof(LineChart.Series)] = new LineChart.DataSeries[] { new("signed", "Signed", [decimal.MinValue, decimal.MaxValue]) };
            html = await Render<LineChart>(parameters);
            Require(html.Contains("points=\"40,244 720,24\"", StringComparison.Ordinal) && !html.Contains("NaN", StringComparison.Ordinal) && !html.Contains("Infinity", StringComparison.Ordinal), "Decimal-range data overflowed or lost its signed scale.");
        }));
        tests.Add(("charts reject duplicate keys mismatched values and maxima that clip original values", async () =>
        {
            foreach (var replacement in new Dictionary<string, object?>[]
            {
                new() { [nameof(LineChart.Title)] = " " },
                new() { [nameof(LineChart.Labels)] = new LineChart.PointLabel[] { new("same", "One"), new("same", "Two") } },
                new() { [nameof(LineChart.Series)] = new LineChart.DataSeries[] { new("same", "One", [1, 2]), new("same", "Two", [1, 2]) } },
                new() { [nameof(LineChart.Series)] = new LineChart.DataSeries[] { new("one", "One", [1]) } },
                new() { [nameof(LineChart.Series)] = new LineChart.DataSeries[] { new("one", "One", [1, 2], Maximum:1) } },
                new() { [nameof(LineChart.Series)] = new LineChart.DataSeries[] { new("one", "One", [0, 0], Maximum:0) } }
            })
            {
                var parameters = ChartParameters(); foreach (var (key, value) in replacement) parameters[key] = value;
                var rejected = false;
                try { await Render<LineChart>(parameters); } catch (ArgumentException) { rejected = true; }
                Require(rejected, "Malformed chart data silently reached the renderer.");
            }
        }));
        tests.Add(("back to top is the standard centered elevated icon button with an SSR fragment fallback", async () =>
        {
            var html = await Render<BackToTop>(new() { [nameof(BackToTop.ContentId)] = "report-content", [nameof(BackToTop.Label)] = "Return to report", [nameof(BackToTop.Class)] = "sample", [nameof(BackToTop.AdditionalAttributes)] = new Dictionary<string, object> { ["data-test"] = "top" } });
            Require(html.Contains("f-back-to-top sample", StringComparison.Ordinal) && html.Contains("data-test=\"top\"", StringComparison.Ordinal), "Standard root composition attributes disappeared.");
            Require(html.Contains("f-button-elevated f-button-icon", StringComparison.Ordinal) && html.Contains("href=\"https://example.test/report#report-content\"", StringComparison.Ordinal), "The control bypassed Button or lost native navigation.");
            Require(html.Contains("aria-label=\"Return to report\"", StringComparison.Ordinal) && html.Contains("data-icon=\"arrow_upward\"", StringComparison.Ordinal), "The top action lost its accessible name or standard glyph.");
            Require(!html.Contains("scroll-to-top", StringComparison.Ordinal) && !html.Contains(" hidden", StringComparison.Ordinal), "The old raw shell button or JS-only SSR state returned.");
        }));
        tests.Add(("back to top validates its actual scroll region and a finite nonnegative threshold", async () =>
        {
            foreach (var parameters in new Dictionary<string, object?>[]
            {
                new() { [nameof(BackToTop.ContentId)] = " " }, new() { [nameof(BackToTop.ContentId)] = "not valid" },
                new() { [nameof(BackToTop.ContentId)] = "report", [nameof(BackToTop.Threshold)] = -1d },
                new() { [nameof(BackToTop.ContentId)] = "report", [nameof(BackToTop.Threshold)] = double.NaN }
            })
            {
                var rejected = false;
                try { await Render<BackToTop>(parameters); } catch (ArgumentException) { rejected = true; }
                Require(rejected, "An invalid top action reached its renderer.");
            }
        }));
        tests.Add(("back to top owns one shared import and releases its controller on repeated disposal", async () =>
        {
            var js = new CountingJs();
            await WithComponent<BackToTop>(new() { [nameof(BackToTop.ContentId)] = "report", [nameof(BackToTop.Threshold)] = 120d }, js, async (component, _) =>
            {
                await AfterRender(component); await AfterRender(component);
                Require(js.Imports == 1 && js.Module.Calls.Count(call => call.Name == "synchronizeBackToTop") == 2, "Top action imported more than once or lost synchronization.");
                var args = js.Module.Calls[0].Args;
                Require((string)args[1]! == "report" && (double)args[2]! == 120d, "The top action targeted the document instead of the chosen region.");
                await component.DisposeAsync(); await component.DisposeAsync(); await AfterRender(component);
                Require(js.Module.Calls.Count(call => call.Name == "disposeBackToTop") == 1 && js.Module.Disposals == 1, "The top action retained listeners or disposed twice.");
            });
        }));
        tests.Add(("back to top coalesces delayed imports and never attaches after disposal", async () =>
        {
            var js = new CountingJs(delayed:true);
            await WithComponent<BackToTop>(new() { [nameof(BackToTop.ContentId)] = "report" }, js, async (component, _) =>
            {
                var first = AfterRender(component); var second = AfterRender(component);
                Require(js.Imports == 1, "Concurrent renders created duplicate imports.");
                await component.DisposeAsync(); js.Complete(); await Task.WhenAll(first, second);
                Require(js.Module.Calls.Count == 0 && js.Module.Disposals == 1, "A late browser module attached to a disposed control.");
            });
        }));
        tests.Add(("section navigator coalesces delayed imports and never attaches after disposal", async () =>
        {
            var js = new CountingJs(delayed:true);
            await WithComponent<SectionNavigator>(new() { [nameof(SectionNavigator.ContentId)] = "report" }, js, async (component, _) =>
            {
                var first = AfterRender(component); var second = AfterRender(component);
                Require(js.Imports == 1, "Concurrent section renders created duplicate imports.");
                await component.DisposeAsync(); js.Complete(); await Task.WhenAll(first, second);
                Require(js.Module.Calls.Count == 0 && js.Module.Disposals == 1, "A late section module attached to a disposed navigator.");
            });
        }));
    }
    private static Dictionary<string, object?> ChartParameters() => new()
    {
        [nameof(LineChart.Title)] = "Example chart",
        [nameof(LineChart.Description)] = "Fictitious values",
        [nameof(LineChart.Labels)] = new LineChart.PointLabel[] { new("mon", "Mon", "Monday"), new("tue", "Tue", "Tuesday") },
        [nameof(LineChart.Series)] = new LineChart.DataSeries[] { new("count", "Count <draft>", [2, 4], value => value + " units"), new("amount", "Amount", [100, 200], value => value + " credits") }
    };
    private static async Task<string> Render<T>(Dictionary<string, object?> parameters) where T : IComponent
    {
        var result = string.Empty;
        await WithComponent<T>(parameters, new CountingJs(), (_, html) => { result = html(); return Task.CompletedTask; });
        return result;
    }
    private static async Task WithComponent<T>(Dictionary<string, object?> parameters, CountingJs js, Func<T, Func<string>, Task> verify) where T : IComponent
    {
        var activator = new CaptureActivator();
        var services = new ServiceCollection(); services.AddLogging(); services.AddFlourishFramework();
        services.AddSingleton<IJSRuntime>(js); services.AddSingleton<NavigationManager>(new FixedNavigation()); services.AddSingleton<IComponentActivator>(activator);
        using var provider = services.BuildServiceProvider();
        await using var renderer = new HtmlRenderer(provider, provider.GetRequiredService<ILoggerFactory>());
        await renderer.Dispatcher.InvokeAsync(async () =>
        {
            var output = await renderer.RenderComponentAsync<T>(ParameterView.FromDictionary(parameters));
            await verify((T)activator.Instances.First(component => component.GetType() == typeof(T)), output.ToHtmlString);
        });
    }
    private static Task AfterRender(IComponent component) => (Task)component.GetType().GetMethod("OnAfterRenderAsync", BindingFlags.Instance | BindingFlags.NonPublic)!.Invoke(component, [false])!;
    private static void Require(bool condition, string message) { if (!condition) throw new InvalidOperationException(message); }
    private sealed class FixedNavigation : NavigationManager { internal FixedNavigation() => Initialize("https://example.test/", "https://example.test/report"); }
    private sealed class CaptureActivator : IComponentActivator
    {
        internal List<IComponent> Instances { get; } = [];
        public IComponent CreateInstance(Type componentType) { var component = (IComponent)Activator.CreateInstance(componentType)!; Instances.Add(component); return component; }
    }
    private sealed class CountingJs(bool delayed = false) : IJSRuntime
    {
        internal int Imports { get; private set; }
        internal CountingModule Module { get; } = new();
        private readonly TaskCompletionSource<IJSObjectReference> gate = new(TaskCreationOptions.RunContinuationsAsynchronously);
        internal void Complete() => gate.SetResult(Module);
        public ValueTask<TValue> InvokeAsync<TValue>(string identifier, object?[]? args) => InvokeAsync<TValue>(identifier, CancellationToken.None, args);
        public async ValueTask<TValue> InvokeAsync<TValue>(string identifier, CancellationToken cancellationToken, object?[]? args)
        {
            if (identifier != "import") throw new InvalidOperationException("Unexpected JavaScript call: " + identifier);
            Imports++;
            return (TValue)(object)(delayed ? await gate.Task : Module);
        }
    }
    private sealed class CountingModule : IJSObjectReference
    {
        internal List<(string Name, object?[] Args)> Calls { get; } = [];
        internal int Disposals { get; private set; }
        public ValueTask<TValue> InvokeAsync<TValue>(string identifier, object?[]? args) => InvokeAsync<TValue>(identifier, CancellationToken.None, args);
        public ValueTask<TValue> InvokeAsync<TValue>(string identifier, CancellationToken cancellationToken, object?[]? args) { Calls.Add((identifier, args ?? [])); return ValueTask.FromResult(default(TValue)!); }
        public ValueTask DisposeAsync() { Disposals++; return ValueTask.CompletedTask; }
    }
}
