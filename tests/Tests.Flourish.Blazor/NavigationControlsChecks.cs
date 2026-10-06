using System.Reflection;
using ArkheideSystem.Flourish.Blazor;
using ArkheideSystem.Flourish.Blazor.Components;
using ArkheideSystem.Flourish.Blazor.Components.Primitives;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;

internal static class NavigationControlsChecks
{
    internal static void Register(List<(string Name, Func<Task> Run)> tests)
    {
        tests.Add(("both production primary navigation entries share the canonical icon-size role", async () =>
        {
            var html = await Render<PrimaryNavigationItem>(new() { [nameof(PrimaryNavigationItem.Id)] = "inventory", [nameof(PrimaryNavigationItem.Label)] = "Inventory", [nameof(PrimaryNavigationItem.Href)] = "/inventory", [nameof(PrimaryNavigationItem.Icon)] = "inventory_2" });
            Require(html.Contains("primary-nav-item", StringComparison.Ordinal) && html.Contains("primary-navigation-icon", StringComparison.Ordinal) && html.Contains("data-icon=\"inventory_2\"", StringComparison.Ordinal), "The primary navigation entry bypassed the production Icon.");
            var rootPath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../.."));
            var css = File.ReadAllText(Path.Combine(rootPath, "src/Flourish.Blazor/Flourish.Blazor.Design/wwwroot/foundation.css"));
            Require(css.Contains(".f-primary-item .f-icon,.primary-nav-item .primary-navigation-icon { --f-icon-size:var(--f-icon-primary-size,24px); width:1em; height:1em; }", StringComparison.Ordinal), "A production primary navigation family missed the shared 24px role.");
        }));
        tests.Add(("secondary navigation uses one canonical selection state for explicit and route matches", async () =>
        {
            foreach (var parameters in new Dictionary<string, object?>[]
            {
                new() { [nameof(SecondaryNavigationItem.Label)] = "Current explicit", [nameof(SecondaryNavigationItem.Href)] = "/other", [nameof(SecondaryNavigationItem.IsCurrent)] = true },
                new() { [nameof(SecondaryNavigationItem.Label)] = "Current route", [nameof(SecondaryNavigationItem.Href)] = "/report", [nameof(SecondaryNavigationItem.Match)] = Microsoft.AspNetCore.Components.Routing.NavLinkMatch.All }
            })
            {
                var html = await Render<SecondaryNavigationItem>(parameters);
                Require(html.Contains("f-secondary-item is-selected", StringComparison.Ordinal) && !html.Contains("is-active", StringComparison.Ordinal), "A secondary entry bypassed the shared navigation selection state.");
                if (parameters.TryGetValue(nameof(SecondaryNavigationItem.IsCurrent), out var explicitCurrent) && explicitCurrent is true)
                    Require(html.Contains("aria-current=\"page\"", StringComparison.Ordinal), "The explicit current secondary entry lost its accessible page state.");
            }
            var inactive = await Render<SecondaryNavigationItem>(new() { [nameof(SecondaryNavigationItem.Label)] = "Other", [nameof(SecondaryNavigationItem.Href)] = "/other" });
            Require(!inactive.Contains("is-selected", StringComparison.Ordinal) && !inactive.Contains("aria-current=", StringComparison.Ordinal), "An unrelated secondary entry became selected.");
        }));
        tests.Add(("configured shells and composed secondary entries share the bounded label and full-title contract", async () =>
        {
            const string longLabel = "Plugin de sincronização de pedidos e produtos entre estabelecimentos <draft>";
            var standalone = await Render<SecondaryNavigationItem>(new()
            {
                [nameof(SecondaryNavigationItem.Label)] = longLabel,
                [nameof(SecondaryNavigationItem.Href)] = "/report",
                [nameof(SecondaryNavigationItem.IsCurrent)] = true
            });
            var services = new ServiceCollection(); services.AddLogging();
            services.AddFlourishFramework(framework => framework.ConfigureNavigation(navigation => navigation
                .AddNav("Registry", "inventory", "/registry", secondary => secondary.AddSubNav(longLabel, "article", "/report", exact: true))));
            services.AddSingleton<IJSRuntime>(new CountingJs()); services.AddSingleton<NavigationManager>(new FixedNavigation());
            using var provider = services.BuildServiceProvider();
            await using var renderer = new HtmlRenderer(provider, provider.GetRequiredService<ILoggerFactory>());
            var configured = await renderer.Dispatcher.InvokeAsync(async () => (await renderer.RenderComponentAsync<ApplicationShell>()).ToHtmlString());
            foreach (var html in new[] { standalone, configured })
            {
                var anchor = System.Text.RegularExpressions.Regex.Match(html, "<a\\b(?=[^>]*\\bhref=\"/report\")[^>]*>(?<content>.*?)</a>", System.Text.RegularExpressions.RegexOptions.Singleline);
                Require(anchor.Success && System.Net.WebUtility.HtmlDecode(anchor.Value).Contains($"title=\"{longLabel}\"", StringComparison.Ordinal), "A navigation entry lost its complete title when the visible label was constrained.");
                Require(System.Net.WebUtility.HtmlDecode(anchor.Groups["content"].Value).Contains($"<span class=\"f-navigation-label\">{longLabel}</span>", StringComparison.Ordinal), "A navigation entry rendered bare text instead of the canonical shrinking label.");
                Require(!html.Contains("<draft>", StringComparison.Ordinal), "The shared label bypassed Razor encoding.");
                Require(anchor.Value.Contains("is-selected", StringComparison.Ordinal), "The compared production entry did not preserve current-route state.");
            }
        }));
        tests.Add(("navigation document replacement recreates its outline and top control with the actual content", async () =>
        {
            var activator = new CaptureActivator();
            var services = new ServiceCollection(); services.AddLogging(); services.AddFlourishFramework();
            services.AddSingleton<IJSRuntime>(new CountingJs()); services.AddSingleton<NavigationManager>(new FixedNavigation()); services.AddSingleton<IComponentActivator>(activator);
            using var provider = services.BuildServiceProvider();
            await using var renderer = new HtmlRenderer(provider, provider.GetRequiredService<ILoggerFactory>());
            await renderer.Dispatcher.InvokeAsync(async () =>
            {
                Dictionary<string, object?> Parameters(string route, string title) => new()
                {
                    [nameof(ArkheideSystem.Flourish.Blazor.Components.Patterns.NavigationSurface.ContentId)] = "workspace-content",
                    [nameof(ArkheideSystem.Flourish.Blazor.Components.Patterns.NavigationSurface.DocumentKey)] = route,
                    [nameof(ArkheideSystem.Flourish.Blazor.Components.Patterns.NavigationSurface.ChildContent)] = (RenderFragment)(builder => builder.AddContent(0, title))
                };
                var output = await renderer.RenderComponentAsync<ArkheideSystem.Flourish.Blazor.Components.Patterns.NavigationSurface>(ParameterView.FromDictionary(Parameters("/dashboard", "Dashboard")));
                var surface = activator.Instances.OfType<ArkheideSystem.Flourish.Blazor.Components.Patterns.NavigationSurface>().Single();
                var oldNavigator = activator.Instances.OfType<SectionNavigator>().Single();
                var oldTop = activator.Instances.OfType<BackToTop>().Single();
                await oldNavigator.UpdateSectionsAsync([new("monthly-summary", "Monthly summary"), new("weekly-orders", "Weekly orders")]);
                Require(output.ToHtmlString().Contains("href=\"#weekly-orders\"", StringComparison.Ordinal), "The actual initial navigator did not receive discovered headings.");
                await surface.SetParametersAsync(ParameterView.FromDictionary(Parameters("/inventory", "Inventory")));
                var currentNavigator = activator.Instances.OfType<SectionNavigator>().Last();
                var currentTop = activator.Instances.OfType<BackToTop>().Last();
                Require(activator.Instances.OfType<SectionNavigator>().Count() == 2 && activator.Instances.OfType<BackToTop>().Count() == 2
                    && !ReferenceEquals(oldNavigator, currentNavigator) && !ReferenceEquals(oldTop, currentTop), "Replacing the keyed main retained controls attached to the retired document.");
                Require(output.ToHtmlString().Contains("Inventory", StringComparison.Ordinal) && !output.ToHtmlString().Contains("#weekly-orders", StringComparison.Ordinal), "A new document retained the previous route outline.");
                await oldNavigator.UpdateSectionsAsync([new("stale-callback", "Stale callback")]);
                await currentNavigator.UpdateSectionsAsync([new("stock", "Stock")]);
                Require(output.ToHtmlString().Contains("href=\"#stock\"", StringComparison.Ordinal) && !output.ToHtmlString().Contains("#stale-callback", StringComparison.Ordinal), "A disposed navigator published a late callback into the new document.");
                await surface.SetParametersAsync(ParameterView.FromDictionary(Parameters("/inventory", "Inventory refreshed")));
                Require(activator.Instances.OfType<SectionNavigator>().Count() == 2 && activator.Instances.OfType<BackToTop>().Count() == 2, "Refreshing one document needlessly replaced its active controllers.");
            });
        }));
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
            Require(html.Contains("data-series=\"1\"", StringComparison.Ordinal) && !html.Contains("<g data-series=\"0\"", StringComparison.Ordinal)
                && html.Contains("data-f-selection-key=\"count\"", StringComparison.Ordinal), "Toggling visibility reindexed a series or removed its actual display option.");
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
