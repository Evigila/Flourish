using System.Globalization;
using System.Linq.Expressions;
using System.Net;
using System.Reflection;
using ArkheideSystem.Flourish.Blazor;
using ArkheideSystem.Flourish.Blazor.Abstract;
using ArkheideSystem.Flourish.Blazor.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.Web.HtmlRendering;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using Primitive = ArkheideSystem.Flourish.Blazor.Components.Primitives;
using Pattern = ArkheideSystem.Flourish.Blazor.Components.Patterns;

internal static class ControlTextChecks
{
    internal static void Register(List<(string Name, Func<Task> Run)> tests)
    {
        tests.Add(("primitive table SSR emits explicit pressed states for sort and keyboard reorder controls", async () =>
        {
            using var services = Services();
            using var scope = services.CreateScope();
            var activator = scope.ServiceProvider.GetRequiredService<CaptureActivator>();
            await using var renderer = Renderer(scope);
            Primitive.DataColumn<string> name = new("name", "Name", item => item);
            Primitive.DataColumn<string> hidden = new("hidden", "Hidden", item => item, defaultVisible: false);
            var output = await renderer.Dispatcher.InvokeAsync(() => renderer.RenderComponentAsync<Primitive.DataTable<string>>(
                ParameterView.FromDictionary(new Dictionary<string, object?>
                {
                    ["PreferenceKey"] = "aria.ssr", ["Items"] = new[] { "Record" },
                    ["Columns"] = new[] { name, hidden }, ["ShowSortControls"] = true, ["CardValueLines"] = 2
                })));
            var table = activator.Components.OfType<Primitive.DataTable<string>>().Single();
            await renderer.Dispatcher.InvokeAsync(() =>
            {
                var html = output.ToHtmlString();
                Require(System.Text.RegularExpressions.Regex.Matches(html, "aria-pressed=\"(?:true|false)\"").Count == 7,
                    "SSR must emit a valid true/false pressed state for every sort and reorder choice.");
                Require(html.Contains("--f-card-value-lines:2;--f-card-value-height:48px", StringComparison.Ordinal),
                    "A host's explicit two-line card geometry was lost.");
                  Require(System.Text.RegularExpressions.Regex.Matches(html, "data-page-controls").Count == 2,
                      "Table pagination must expose synchronized controls above and below the records.");
                  var ranges = System.Text.RegularExpressions.Regex.Matches(html, @"<span\b[^>]*\sdata-page-range(?:\s|>)[^>]*>");
                  Require(ranges.Count == 1 && ranges[0].Value.Contains("aria-live=\"polite\"", StringComparison.Ordinal)
                      && html.IndexOf(" data-page-range", StringComparison.Ordinal)
                          < html.IndexOf("data-table-pagination-bottom", StringComparison.Ordinal),
                      "Only the upper pager may announce the range; the lower pager must not duplicate its live region.");
                table.GetType().GetMethod("SetSortKey", BindingFlags.Instance | BindingFlags.NonPublic)!.Invoke(table, ["hidden"]);
                typeof(ComponentBase).GetMethod("StateHasChanged", BindingFlags.Instance | BindingFlags.NonPublic)!.Invoke(table, null);
                html = output.ToHtmlString();
                Require(System.Text.RegularExpressions.Regex.IsMatch(html, "<button[^>]*aria-pressed=\"true\"[^>]*>Hidden</button>"),
                    "Selecting a hidden sort target must expose its pressed state to assistive technology.");
                table.GetType().GetMethod("ReorderColumnWithKeyboard", BindingFlags.Instance | BindingFlags.NonPublic)!
                    .Invoke(table, [new KeyboardEventArgs { Key = " " }, name]);
                typeof(ComponentBase).GetMethod("StateHasChanged", BindingFlags.Instance | BindingFlags.NonPublic)!.Invoke(table, null);
                html = output.ToHtmlString();
                var handles = System.Text.RegularExpressions.Regex.Matches(html, "<button[^>]*column-drag-handle[^>]*>");
                Require(handles[0].Value.Contains("aria-pressed=\"true\"", StringComparison.Ordinal),
                    "Keyboard reorder activation must emit an explicit pressed state.");
            });
        }));

        tests.Add(("offer pause and resume update pressed state with scoped defaults while explicit labels remain literal", async () =>
        {
            using var services = Services();
            using var scope = services.CreateScope();
            var provider = (TrackingTextProvider)scope.ServiceProvider.GetRequiredService<ITextProvider>();
            var activator = scope.ServiceProvider.GetRequiredService<CaptureActivator>();
            await using var renderer = Renderer(scope);
            var output = await renderer.Dispatcher.InvokeAsync(() => renderer.RenderComponentAsync<OfferStage>(ParameterView.Empty));
            var stage = activator.Components.OfType<OfferStage>().Single();
            var toggle = typeof(OfferStage).GetMethod("ToggleRotation", BindingFlags.Instance | BindingFlags.NonPublic)!;
            var render = typeof(ComponentBase).GetMethod("StateHasChanged", BindingFlags.Instance | BindingFlags.NonPublic)!;
            await renderer.Dispatcher.InvokeAsync(() =>
            {
                RequireRotationState(output.ToHtmlString(), "en-US:Flourish/Offer_PauseRotation", false);
                typeof(OfferStage).GetField("initialized", BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(stage, true);
                toggle.Invoke(stage, null);
                render.Invoke(stage, null);
                RequireRotationState(output.ToHtmlString(), "en-US:Flourish/Offer_ResumeRotation", true);
                Require(!System.Text.RegularExpressions.Regex.IsMatch(output.ToHtmlString(), @"<button\b[^>]*\sdisabled(?:=|\s|>)"),
                    "An initialized rotation control remained unavailable.");
            });
            await Task.Run(() => provider.Select("pt-BR"));
            await renderer.Dispatcher.InvokeAsync(() => RequireRotationState(output.ToHtmlString(), "pt-BR:Flourish/Offer_ResumeRotation", true));
            await renderer.Dispatcher.InvokeAsync(async () =>
            {
                await stage.SetParametersAsync(ParameterView.FromDictionary(new Dictionary<string, object?>
                {
                    ["PauseRotationLabel"] = "Pause rotation", ["ResumeRotationLabel"] = "Resume rotation"
                }));
                RequireRotationState(output.ToHtmlString(), "Resume rotation", true);
                Require(!output.ToHtmlString().Contains("Flourish/Offer_", StringComparison.Ordinal), "An explicit default-shaped resume label was translated.");
                toggle.Invoke(stage, null);
                render.Invoke(stage, null);
                RequireRotationState(output.ToHtmlString(), "Pause rotation", false);
                toggle.Invoke(stage, null);
                render.Invoke(stage, null);
                RequireRotationState(output.ToHtmlString(), "Resume rotation", true);
            });
            await renderer.DisposeAsync();
            Require(provider.Subscribers == 0, "Asynchronous offer-stage disposal retained its text subscription.");
        }));

        tests.Add(("generic control and pattern defaults refresh per scope and every subscription is released", async () =>
        {
            using var services = Services();
            using var scope = services.CreateScope();
            using var independent = services.CreateScope();
            var provider = (TrackingTextProvider)scope.ServiceProvider.GetRequiredService<ITextProvider>();
            var other = (TrackingTextProvider)independent.ServiceProvider.GetRequiredService<ITextProvider>();
            var activator = scope.ServiceProvider.GetRequiredService<CaptureActivator>();
            await using var renderer = Renderer(scope);
            var fixtures = Fixtures();
            var outputs = new List<(Fixture Fixture, HtmlRootComponent Output)>();
            foreach (var fixture in fixtures)
                outputs.Add((fixture, await renderer.Dispatcher.InvokeAsync(() =>
                    renderer.RenderComponentAsync(fixture.Type, ParameterView.FromDictionary(fixture.Parameters)))));
            foreach (var (fixture, output) in outputs)
                Require(await renderer.Dispatcher.InvokeAsync(() => HasText(fixture, output, activator, "en-US")), "Missing default text for " + fixture.Type.Name);
            var subscriptions = provider.Subscribers;
            Require(subscriptions >= fixtures.Count, "Some controls failed to subscribe.");
            await Task.Run(() => provider.Select("pt-BR"));
            foreach (var (fixture, output) in outputs)
                Require(await renderer.Dispatcher.InvokeAsync(() => HasText(fixture, output, activator, "pt-BR")), "Text did not refresh for " + fixture.Type.Name);
            Require(provider.Subscribers == subscriptions && other.Culture == "en-US", "Refresh duplicated subscriptions or leaked culture to another scope.");
            await renderer.DisposeAsync();
            Require(provider.Subscribers == 0, "A synchronous or asynchronous control retained its scoped provider.");
            provider.Select("zh-CN");
        }));

        tests.Add(("explicit generic control text remains literal even when equal to its original default", async () =>
        {
            using var services = Services();
            using var scope = services.CreateScope();
            var activator = scope.ServiceProvider.GetRequiredService<CaptureActivator>();
            await using var renderer = Renderer(scope);
            var provider = (TrackingTextProvider)scope.ServiceProvider.GetRequiredService<ITextProvider>();
            provider.Select("zh-CN");
            foreach (var fixture in Fixtures().Where(fixture => fixture.ExplicitParameter is not null))
            {
                fixture.Parameters[fixture.ExplicitParameter!] = fixture.OriginalDefault;
                var output = await renderer.Dispatcher.InvokeAsync(() => renderer.RenderComponentAsync(fixture.Type, ParameterView.FromDictionary(fixture.Parameters)));
                var component = activator.Components.Last(component => component.GetType() == fixture.Type);
                var text = await renderer.Dispatcher.InvokeAsync(() => fixture.Property is not null
                    ? component.GetType().GetProperty(fixture.Property, BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(component)?.ToString()
                    : WebUtility.HtmlDecode(output.ToHtmlString()));
                Require(text?.Contains(fixture.OriginalDefault!, StringComparison.Ordinal) == true, "Explicit literal was replaced for " + fixture.Type.Name);
                Require(text?.Contains("zh-CN:Flourish/" + fixture.Token, StringComparison.Ordinal) == false, "Explicit old default was treated as omitted for " + fixture.Type.Name);
            }
            await renderer.DisposeAsync();
            Require(provider.Subscribers == 0, "Explicit text controls retained subscriptions.");
        }));

        tests.Add(("bound numeric and select validation use live scoped defaults while caller messages win", async () =>
        {
            decimal number = 2;
            int choice = 1;
            using var services = Services();
            using var scope = services.CreateScope();
            var activator = scope.ServiceProvider.GetRequiredService<CaptureActivator>();
            var provider = (TrackingTextProvider)scope.ServiceProvider.GetRequiredService<ITextProvider>();
            await using var renderer = Renderer(scope);
            var cases = new (Type Type, string Token, Dictionary<string, object?> Parameters)[]
            {
                (typeof(NumberBox<decimal>), "Input_NumberError", new() { ["Value"] = number, ["ValueExpression"] = (Expression<Func<decimal>>)(() => number) }),
                (typeof(SelectBox<int>), "Input_SelectError", new() { ["Value"] = choice, ["ValueExpression"] = (Expression<Func<int>>)(() => choice) })
            };
            foreach (var test in cases)
            {
                await renderer.Dispatcher.InvokeAsync(() => renderer.RenderComponentAsync(test.Type, ParameterView.FromDictionary(test.Parameters)));
                var component = activator.Components.Last(component => component.GetType() == test.Type);
                Require(ParseError(component) == "en-US:Flourish/" + test.Token, "Input used an untranslated validation default.");
            }
            await Task.Run(() => provider.Select("pt-BR"));
            foreach (var test in cases)
            {
                var component = activator.Components.Last(component => component.GetType() == test.Type);
                Require(ParseError(component) == "pt-BR:Flourish/" + test.Token, "Input validation did not use the current scoped culture.");
                test.Parameters["ParsingErrorMessage"] = "Host validation";
                await renderer.Dispatcher.InvokeAsync(() => renderer.RenderComponentAsync(test.Type, ParameterView.FromDictionary(test.Parameters)));
                Require(ParseError(activator.Components.Last(component => component.GetType() == test.Type)) == "Host validation", "Caller input validation message was replaced.");
            }
            await renderer.DisposeAsync();
            Require(provider.Subscribers == 0, "InputBase disposal failed to unsubscribe its text provider.");
        }));

        tests.Add(("editable grid translates stream error tokens before calling the host", async () =>
        {
            using var services = Services();
            using var scope = services.CreateScope();
            var activator = scope.ServiceProvider.GetRequiredService<CaptureActivator>();
            var provider = (TrackingTextProvider)scope.ServiceProvider.GetRequiredService<ITextProvider>();
            provider.Select("pt-BR");
            string? message = null;
            await using var renderer = Renderer(scope);
            await renderer.Dispatcher.InvokeAsync(() => renderer.RenderComponentAsync<Primitive.EditingGrid>(ParameterView.FromDictionary(new Dictionary<string, object?>
            {
                ["Interactions"] = new Primitive.GridInteractions { ReportError = text => { message = text; return Task.CompletedTask; } }
            })));
            var grid = activator.Components.OfType<Primitive.EditingGrid>().Single();
            var report = grid.GetType().GetMethod("ReportPasteError", BindingFlags.Instance | BindingFlags.NonPublic)!;
            foreach (var token in new[] { "Grid_EditFailed", "Grid_PasteTooLarge", "Grid_EditTooLarge" })
            {
                await renderer.Dispatcher.InvokeAsync(() => (Task)report.Invoke(grid, [token])!);
                Require(message == "pt-BR:Flourish/" + token, "A grid stream error bypassed the current scoped text provider.");
            }
            await renderer.Dispatcher.InvokeAsync(() => (Task)report.Invoke(grid, ["Host supplied message"])!);
            Require(message == "Host supplied message", "Unknown host feedback was treated as a library token.");
        }));
    }

    private static List<Fixture> Fixtures() =>
    [
        new(typeof(ActionMenu), "Menu_RecordActions", "Label", "Record actions", new()),
        new(typeof(Button), "Button_Working", "BusyLabel", "Working...", new() { ["Busy"] = true }),
        new(typeof(BottomSheet), "Dialog_Close", "CloseLabel", "Close", new() { ["Title"] = "Sheet" }),
        new(typeof(LoadingState), "Table_Loading", "Message", "Loading...", new()),
        new(typeof(SearchBox), "Input_Search", "Label", "Search", new()),
        new(typeof(ToggleSwitch), "Toggle_On", "OnLabel", "On", new() { ["Value"] = true, ["Label"] = "Host label" }),
        new(typeof(ProgressBar), "Progress_Label", "Label", "Progress", new()),
        new(typeof(ProgressRing), "Progress_Label", "Label", "Progress", new()),
        new(typeof(SplitButton), "Menu_MoreActions", "SecondaryLabel", "More actions", new()),
        new(typeof(SectionNavigator), "Shell_PageContents", "Label", "On this page", new() { ["ContentId"] = "content" }),
        new(typeof(BackToTop), "Shell_Top", "Label", "Back to top", new() { ["ContentId"] = "content" }),
        new(typeof(DataSearch<string>), "Table_SearchBy", null, null, new() { ["Columns"] = new TableColumn<string>[] { new("name", "Name", item => item) } }),
        new(typeof(LineChart), "Chart_NoSeries", "EmptyText", "No series selected.", new() { ["Title"] = "Host chart" }),
        new(typeof(LineChart), "Chart_Maximum", "MaximumLabel", "Maximum", new()
        {
            ["Title"] = "Host chart", ["Labels"] = new LineChart.PointLabel[] { new("one", "One") },
            ["Series"] = new LineChart.DataSeries[] { new("one", "Host series", [1]) }
        }),
        new(typeof(LineChart), "Chart_Point", "PointCaption", "Point", new()
        {
            ["Title"] = "Host chart", ["Labels"] = new LineChart.PointLabel[] { new("one", "One") },
            ["Series"] = new LineChart.DataSeries[] { new("one", "Host series", [1]) }
        }),
        new(typeof(UniformGridButton), "Button_Working", "BusyLabel", "Working...", new() { ["Busy"] = true }),
        new(typeof(OfferStage), "Offer_PauseRotation", "PauseRotationLabel", "Pause rotation", new()),
        new(typeof(Field), "Input_Required", null, null, new() { ["Required"] = true, ["Id"] = "field", ["Label"] = "Host label" }),
        new(typeof(Primitive.BottomSheet), "Dialog_Close", "CloseLabel", "Close", new() { ["Id"] = "sheet", ["Title"] = "Sheet" }),
        new(typeof(Primitive.RowActionMenu), "Menu_RecordActions", "Label", "Record actions", new() { ["ChildContent"] = (RenderFragment)(_ => { }) }),
        new(typeof(Primitive.ReferenceDropdown<long>), "Selection_None", "EmptySelectionText", "None", new() { ["Id"] = "ref", ["LabelledBy"] = "ref-label" }),
        new(typeof(Primitive.MultiSelectDropdown<string, string>), "Selection_NoneSelected", "EmptySelectionText", "None selected", new()
        {
            ["Id"] = "multi", ["ValueSelector"] = (Func<string,string>)(item => item), ["TextSelector"] = (Func<string,string>)(item => item)
        }),
        new(typeof(Primitive.SearchAutocomplete<string>), "Search_Suggestions", "ResultsLabel", "Search suggestions", new()
        {
            ["Id"] = "search", ["ValueSelector"] = (Func<string,string>)(item => item), ["TextSelector"] = (Func<string,string>)(item => item)
        }, "EffectiveResultsLabel"),
        new(typeof(Primitive.NavigationGuard), "Navigation_Unsaved", "Message", "There are unsaved changes. Leave and discard them?", new(), "EffectiveMessage"),
        new(typeof(Primitive.ServiceMenu), "Menu_Services", "Label", "Services", new()),
        new(typeof(Primitive.InteractionBoundary), "Interaction_Unavailable", "Title", "Interaction unavailable", new() { ["Locked"] = true }),
        new(typeof(Primitive.PageContents), "Shell_PageContents", "Label", "On this page", new()),
        new(typeof(Primitive.EditingGrid), "Grid_Label", "Label", "Editable grid", new()),
        new(typeof(Primitive.DataPager), "Pager_Label", "Label", "Record pagination", new()),
        new(typeof(Primitive.DataSearch<string>), "Table_SearchBy", null, null, new()
        {
            ["Columns"] = new Primitive.DataColumn<string>[] { new("name", "Name", item => item) }
        }),
        new(typeof(Primitive.DataTable<string>), "Grid_Empty", "EmptyMessage", "No records found.", new() { ["PreferenceKey"] = "text-checks" }),
        new(typeof(Pattern.ContentSurface), "Shell_Skip", "SkipLabel", "Skip to content", new()),
        new(typeof(Pattern.NavigationSurface), "Shell_Top", "TopLabel", "Back to top", new())
    ];

    private static void RequireRotationState(string html, string label, bool paused)
    {
        var control = System.Text.RegularExpressions.Regex.Match(html, @"<button\b[^>]*class=""[^""]*\bf-button-quiet\b[^""]*""[^>]*>.*?</button>",
            System.Text.RegularExpressions.RegexOptions.Singleline);
        Require(control.Success && control.Value.Contains($"aria-pressed=\"{paused.ToString().ToLowerInvariant()}\"", StringComparison.Ordinal)
            && WebUtility.HtmlDecode(control.Value).Contains(label, StringComparison.Ordinal),
            "Rotation label and explicit pressed state diverged: " + html);
    }
    private static bool HasText(Fixture fixture, HtmlRootComponent output, CaptureActivator activator, string culture)
    {
        var value = fixture.Property is null ? WebUtility.HtmlDecode(output.ToHtmlString())
            : activator.Components.Last(component => component.GetType() == fixture.Type).GetType()
                .GetProperty(fixture.Property, BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(activator.Components.Last(component => component.GetType() == fixture.Type))?.ToString();
        return value?.Contains(culture + ":Flourish/" + fixture.Token, StringComparison.Ordinal) == true;
    }
    private static string ParseError(IComponent component)
    {
        object?[] arguments = ["invalid", null, null];
        var result = component.GetType().GetMethod("TryParseValueFromString", BindingFlags.Instance | BindingFlags.NonPublic)!.Invoke(component, arguments);
        Require(result is false, "Invalid input was accepted.");
        return arguments[2]?.ToString() ?? "";
    }
    private static ServiceProvider Services()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddFlourishFramework();
        services.AddScoped<ITextProvider, TrackingTextProvider>();
        services.AddScoped<CaptureActivator>();
        services.AddScoped<IComponentActivator>(provider => provider.GetRequiredService<CaptureActivator>());
        services.AddSingleton<NavigationManager>(new TestNavigation());
        services.AddSingleton<IJSRuntime>(new StaticJs());
        return services.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true });
    }
    private static HtmlRenderer Renderer(IServiceScope scope) => new(scope.ServiceProvider, scope.ServiceProvider.GetRequiredService<ILoggerFactory>());
    private static void Require(bool condition, string message) { if (!condition) throw new InvalidOperationException(message); }
    private sealed record Fixture(Type Type, string Token, string? ExplicitParameter, string? OriginalDefault, Dictionary<string, object?> Parameters, string? Property = null);
    private sealed class CaptureActivator : IComponentActivator
    {
        internal List<IComponent> Components { get; } = [];
        public IComponent CreateInstance(Type type)
        {
            var component = (IComponent)Activator.CreateInstance(type)!;
            Components.Add(component);
            return component;
        }
    }
    private sealed class StaticJs : IJSRuntime
    {
        public ValueTask<TValue> InvokeAsync<TValue>(string identifier, object?[]? args) => throw new InvalidOperationException("Static rendering invoked JavaScript.");
        public ValueTask<TValue> InvokeAsync<TValue>(string identifier, CancellationToken cancellationToken, object?[]? args) => InvokeAsync<TValue>(identifier, args);
    }
}
