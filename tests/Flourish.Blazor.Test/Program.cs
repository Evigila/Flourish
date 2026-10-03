using System.Globalization;
using System.Net;
using System.Linq.Expressions;
using Microsoft.AspNetCore.Components.Forms;
using ApplicationTheme = ArkheideSystem.Flourish.Abstract.ApplicationTheme;
using ArkheideSystem.Flourish.Blazor;
using ArkheideSystem.Flourish.Blazor.Abstract;
using ArkheideSystem.Flourish.Blazor.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;

var tests = new List<(string Name, Func<Task> Run)>();
void Test(string name, Action run) => tests.Add((name, () => { run(); return Task.CompletedTask; }));
void AsyncTest(string name, Func<Task> run) => tests.Add((name, run));
void Check(bool condition, string message) { if (!condition) throw new InvalidOperationException(message); }
void Equal<T>(T expected, T actual, string? message = null) => Check(EqualityComparer<T>.Default.Equals(expected, actual), message ?? $"Expected {expected}, received {actual}.");
void Throws<T>(Action action) where T : Exception { try { action(); } catch (T) { return; } throw new InvalidOperationException($"Expected {typeof(T).Name}."); }
void Order(IEnumerable<int> expected, IReadOnlyList<Record> actual) => Check(expected.SequenceEqual(actual.Select(row => row.Id)), $"Unexpected order: {string.Join(',', actual.Select(row => row.Id))}.");
var culture = CultureInfo.GetCultureInfo("pt-BR");
TableColumn<Record> name = new("name", "Name", row => row.Name);
TableColumn<Record> amount = new("amount", "Amount", row => row.Amount, Format: row => row.Amount?.ToString("C", culture));
TableColumn<Record> date = new("date", "Date", row => row.Date);
TableColumn<Record> email = new("email", "Email", row => row.Email);
TableColumn<Record> internalKey = new("internal", "Internal", row => row.Id, Searchable: false);
IReadOnlyList<TableColumn<Record>> columns = [name, amount, date, email, internalKey];
IReadOnlyList<Record> records = [
    new(1, "João", "joao@example.test", 2m, new(2025, 12, 31)),
    new(2, "Água", "water@example.test", 10m, new(2026, 1, 2)),
    new(3, "agua", "plain@example.test", null, null),
    new(4, null, "nobody@example.test", 2m, new(2025, 1, 9))];

Test("numeric sorting uses values instead of formatted currency", () => {
    Order([1, 4, 2, 3], TableData<Record>.Sort(records, amount, TableSortDirection.Ascending, culture));
    Order([2, 1, 4, 3], TableData<Record>.Sort(records, amount, TableSortDirection.Descending, culture));
});
Test("date sorting is chronological across localized month/year boundaries", () => {
    Order([4, 1, 2, 3], TableData<Record>.Sort(records, date, TableSortDirection.Ascending, culture));
    Order([2, 1, 4, 3], TableData<Record>.Sort(records, date, TableSortDirection.Descending, culture));
});
Test("localized equal keys retain input order and null remains last both ways", () => {
    Order([2, 3, 1, 4], TableData<Record>.Sort(records, name, TableSortDirection.Ascending, culture));
    Order([1, 2, 3, 4], TableData<Record>.Sort(records, name, TableSortDirection.Descending, culture));
});
Test("default order restores the supplied sequence", () => Order([1, 2, 3, 4], TableData<Record>.Sort(records, amount, TableSortDirection.Default, culture)));
Test("single-column search cannot accidentally match another property", () => {
    Order([1], TableData<Record>.Filter(records, columns, "JOAO", "name", culture));
    Equal(0, TableData<Record>.Filter(records, columns, "example.test", "name", culture).Count);
    Order([1, 2, 3, 4], TableData<Record>.Filter(records, columns, "EXAMPLE.TEST", "email", culture));
});
Test("all-column search covers declared searchable data but excludes internal fields", () => {
    Order([2, 3], TableData<Record>.Filter(records, columns, "água", null, culture));
    Equal(0, TableData<Record>.Filter(records, [internalKey], "1", null, culture).Count);
});
Test("search formatting follows the visible culture", () => {
    Order([2], TableData<Record>.Filter(records, [amount], "10,00", "amount", culture));
    Order([1], TableData<Record>.Filter(records, [date], "31/12/2025", "date", culture));
});
Test("empty queries retain complete input and nulls have no fictional display value", () => {
    Order([1, 2, 3, 4], TableData<Record>.Filter(records, columns, "  ", "name", culture));
    Equal(string.Empty, TableData<Record>.Display(records[3], name, culture));
});
Test("page bounds survive deletion, empty results, and invalid input", () => {
    Equal(2, TableData<Record>.ClampPage(8, 21, 20));
    Equal(1, TableData<Record>.ClampPage(0, 21, 20));
    Equal(1, TableData<Record>.ClampPage(9, 0, 20));
    Throws<ArgumentOutOfRangeException>(() => TableData<Record>.ClampPage(1, 8, 0));
});
Test("runtime appearance is scoped to the user circuit", () => {
    var services = new ServiceCollection(); services.AddFlourish();
    using var provider = services.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true });
    using var first = provider.CreateScope(); using var second = provider.CreateScope();
    var a = first.ServiceProvider.GetRequiredService<IAppearanceService>(); var b = second.ServiceProvider.GetRequiredService<IAppearanceService>();
    a.SetColors("#ffffff", "#ffeebb"); a.SetTheme(ApplicationTheme.Dark);
    Equal("#FFFFFF", a.Current.Primary); Equal(ApplicationTheme.Dark, a.Current.Theme);
    Equal("#153A32", b.Current.Primary); Equal(ApplicationTheme.Light, b.Current.Theme);
});
Test("unchanged appearance does not emit duplicate state notifications", () => {
    var services = new ServiceCollection(); services.AddFlourish(); using var provider = services.BuildServiceProvider(); using var scope = provider.CreateScope();
    var appearance = scope.ServiceProvider.GetRequiredService<IAppearanceService>(); var count = 0;
    appearance.Changed += (_, _) => count++;
    appearance.SetColors("#153a32", "#16745f"); appearance.SetTheme(ApplicationTheme.Light); Equal(0, count);
    appearance.SetTheme(ApplicationTheme.Dark); appearance.SetTheme(ApplicationTheme.Dark); Equal(1, count);
});
Test("duplicate host registration is rejected", () => { var services = new ServiceCollection(); services.AddFlourish(); Throws<InvalidOperationException>(() => services.AddFlourish()); });
Test("captured startup and nested builders cannot mutate completed options", () => {
    IApplicationBuilder? app = null; ITitleBarBuilder? title = null; IAppearanceBuilder? appearance = null; ILayoutBuilder? layout = null; INavigationGroupBuilder? group = null;
    var services = new ServiceCollection();
    services.AddFlourish(builder => {
        app = builder;
        builder.UseTitleBar(value => { title = value; value.SetApplicationTitle("Test"); });
        builder.ConfigureAppearance(value => appearance = value);
        builder.ConfigureLayout(value => layout = value);
        builder.UseNavigation(value => value.AddGroup("pages", "Pages", "page", value => { group = value; value.AddItem("Home", "/"); }));
    });
    Throws<InvalidOperationException>(() => app!.UseTitleBar());
    Throws<InvalidOperationException>(() => title!.SetApplicationTitle("Late"));
    Throws<InvalidOperationException>(() => appearance!.SetTheme(ApplicationTheme.Dark));
    Throws<InvalidOperationException>(() => layout!.SetContentWidth(900));
    Throws<InvalidOperationException>(() => group!.AddItem("Late", "/late"));
    Throws<InvalidOperationException>(() => group!.SetSecondaryNavigation(false));
});
Test("navigation rejects external/scheme-relative routes and duplicate keys", () => {
    foreach (var href in new[] { "https://example.test", "//example.test", "javascript:alert(1)", "/a\\b" }) {
        var services = new ServiceCollection();
        Throws<ArgumentException>(() => services.AddFlourish(app => app.UseNavigation(nav => nav.AddGroup("pages", "Pages", "page", group => group.AddItem("Bad", href)))));
    }
    var duplicate = new ServiceCollection();
    Throws<ArgumentException>(() => duplicate.AddFlourish(app => app.UseNavigation(nav => { nav.AddGroup("same", "One", "page", group => group.AddItem("Home", "/")); nav.AddGroup("same", "Two", "page", group => group.AddItem("Next", "/next")); })));
});
Test("arbitrary palettes provide AA text on primary/accent and both reference surfaces", () => {
    foreach (var primary in new[] { "#FFFFFF", "#000000", "#777777", "#FFEE99", "#FF00FF", "#153A32", "#55AAEE" })
    foreach (var accent in new[] { "#FFFFFF", "#000000", "#777777", "#FFEE99", "#16745F" }) {
        var palette = AppearancePalette.Create(primary, accent);
        Check(AppearancePalette.Contrast(primary, palette.PrimaryInk) >= 4.5, $"Primary foreground fails AA: {primary}/{palette.PrimaryInk}");
        Check(AppearancePalette.Contrast(accent, palette.AccentInk) >= 4.5, $"Accent foreground fails AA: {accent}/{palette.AccentInk}");
        Check(AppearancePalette.Contrast("#FFFFFF", palette.AccentText) >= 4.5, "Light text contrast fails.");
        Check(AppearancePalette.Contrast("#272E34", palette.DarkAccentText) >= 4.5, "Dark text contrast fails.");
    }
});
Test("invalid colors and CSS font declarations cannot reach theme variables", () => {
    foreach (var value in new[] { "red", "#fff", "#GG0000", "#000000;background:red" }) Throws<ArgumentException>(() => AppearancePalette.Create(value, "#000000"));
    var services = new ServiceCollection(); Throws<ArgumentException>(() => services.AddFlourish(builder => builder.ConfigureAppearance(appearance => appearance.SetFont("Segoe UI; color:red"))));
});

async Task<string> Render<TComponent>(Dictionary<string, object?>? parameters = null, Action<IApplicationBuilder>? configure = null, string path = "/records") where TComponent : IComponent {
    var services = new ServiceCollection();
    services.AddLogging(); services.AddSingleton<NavigationManager>(new TestNavigation(path)); services.AddSingleton<IJSRuntime>(new FakeJs());
    services.AddFlourish(configure ?? (builder => builder.UseTitleBar(title => title.SetApplicationTitle("Test Application")).UseNavigation(nav => nav.AddGroup("records", "Records", "list", group => group.AddItem("List", "/records", exact: true).AddItem("Unavailable", "/disabled", disabled: true)))));
    using var provider = services.BuildServiceProvider(); using var scope = provider.CreateScope();
    await using var renderer = new HtmlRenderer(scope.ServiceProvider, provider.GetRequiredService<ILoggerFactory>());
    return await renderer.Dispatcher.InvokeAsync(async () => {
        var output = await renderer.RenderComponentAsync<TComponent>(ParameterView.FromDictionary(parameters ?? new()));
        return output.ToHtmlString();
    });
}
var pageRecords = Enumerable.Range(1, 25).Select(id => new Record(id, $"Record {id:00}", $"r{id}@example.test", id, new(2026, 1, 1))).ToArray();
AsyncTest("table renders one 20-row page with synchronized controls", async () => {
    var html = WebUtility.HtmlDecode(await Render<DataTable<Record>>(new() { ["Items"] = pageRecords, ["Columns"] = columns, ["ItemKey"] = (Func<Record, object>)(row => row.Id) }));
    Equal(21, html.Split("<tr", StringSplitOptions.None).Length - 1);
    Check(html.Contains("Items 1–20 of") && html.Contains(">25</strong>"), "Actual loaded item count was lost.");
    Equal(2, html.Split("aria-label=\"Next page\"", StringSplitOptions.None).Length - 1);
    Check(html.Contains("data-f-resize=\"name\"") && html.Contains("aria-valuemin=\"72\""), "Accessible column resizing is absent.");
    Check(html.Contains("Search by") && html.Contains("All columns") && html.Contains("Amount"), "Column search labels are missing.");
});
AsyncTest("cards expose exactly the same columns without duplicated visible field captions", async () => {
    var html = WebUtility.HtmlDecode(await Render<DataTable<Record>>(new() { ["Items"] = records, ["Columns"] = columns, ["View"] = TableView.Cards }));
    Equal(records.Count, html.Split("role=\"listitem\"", StringSplitOptions.None).Length - 1);
    Check(html.Contains("Name: </span>João") && html.Contains("Email: </span>joao@example.test"), "Cards do not expose matching data fields.");
    Check(!html.Contains("<table"), "The hidden table was still rendered as active presentation.");
});
AsyncTest("empty/loading/error have distinct honest status semantics", async () => {
    var empty = await Render<DataTable<Record>>(new() { ["Items"] = Array.Empty<Record>(), ["Columns"] = columns });
    Check(empty.Contains("No items.") && empty.Contains("role=\"status\""), "Empty state is not announced.");
    var loading = await Render<DataTable<Record>>(new() { ["Items"] = Array.Empty<Record>(), ["Columns"] = columns, ["Loading"] = true });
    Check(loading.Contains("aria-busy=\"true\"") && loading.Contains("Loading"), "Loading semantics are missing.");
    Check(!loading.Contains("f-data-count") && !loading.Contains("No items."), "Loading fabricated empty counts or an empty result.");
    var error = await Render<DataTable<Record>>(new() { ["Items"] = Array.Empty<Record>(), ["Columns"] = columns, ["Error"] = "Read failed" });
    Check(error.Contains("role=\"alert\"") && error.Contains("Read failed") && !error.Contains("No items.") && !error.Contains("f-data-count"), "Error became a fictional empty success.");
});
AsyncTest("row text, labels, and formatted values are HTML encoded", async () => {
    var malicious = new[] { new Record(1, "<script>alert('record')</script>", "mail@example.test", 1, null) };
    var html = await Render<DataTable<Record>>(new() { ["Items"] = malicious, ["Columns"] = new[] { new TableColumn<Record>("name", "<b>Name</b>", row => row.Name) } });
    Check(html.Contains("&lt;script&gt;") && html.Contains("&lt;b&gt;"), "Consumer text did not remain encoded.");
    Check(!html.Contains("<script>") && !html.Contains("<b>Name</b>"), "Consumer text became executable markup.");
});
AsyncTest("table without row actions does not invent a default opening menu", async () => {
    var html = await Render<DataTable<Record>>(new() { ["Items"] = records, ["Columns"] = columns });
    Check(!html.Contains("f-action-menu") && !html.Contains("f-data-openable"), "A default record action was inferred.");
});
AsyncTest("row opening is explicit and availability keeps denied actions out of menus", async () => {
    var parameters = new Dictionary<string, object?> {
        ["Items"] = records,
        ["Columns"] = columns,
        ["OnRowOpen"] = EventCallback.Factory.Create<Record>(new object(), (Record _) => { }),
        ["Actions"] = new[] { new RowAction<Record>("Archive", _ => Task.CompletedTask, row => row.Id == 1, Destructive: true) }
    };
    var html = await Render<DataTable<Record>>(parameters);
    Equal(records.Count, html.Split(">Open</button>", StringSplitOptions.None).Length - 1);
    Equal(1, html.Split(">Archive</button>", StringSplitOptions.None).Length - 1);
    Check(html.Contains("f-data-openable") && html.Contains("aria-haspopup=\"menu\""), "Explicit open/menu semantics were lost.");
    Check(html.IndexOf(">Open</button>", StringComparison.Ordinal) < html.IndexOf(">Archive</button>", StringComparison.Ordinal), "Open is not the first record menu action.");
});

AsyncTest("shell owns configured navigation and labels with disabled route semantics", async () => {
    var html = await Render<ApplicationShell>(new() { ["ChildContent"] = (RenderFragment)(builder => { builder.OpenElement(0, "h1"); builder.AddContent(1, "<Unsafe title>"); builder.CloseElement(); }) });
    Check(html.Contains("Test Application") && html.Contains("Main navigation") && html.Contains("f-content-scroll"), "Configured shell was not assembled.");
    Check(html.Contains("aria-current=\"page\"") && html.Contains("aria-disabled=\"true\""), "Navigation state is not accessible.");
    Check(html.Contains("&lt;Unsafe title&gt;") && !html.Contains("<Unsafe title>"), "Shell child text was not encoded.");
});

AsyncTest("button Busy preserves native submit and disabled semantics", async () => {
    var html = await Render<Button>(new() { ["Type"] = "submit", ["Busy"] = true, ["ChildContent"] = (RenderFragment)(builder => builder.AddContent(0, "Save <record>")) });
    Check(html.Contains("type=\"submit\"") && html.Contains("disabled") && html.Contains("aria-busy=\"true\""), "Busy submit is not disabled and announced.");
    Check(html.Contains("Save &lt;record&gt;"), "Button text was not encoded.");
});
AsyncTest("field labels identify native inputs and errors retain accessible relationships", async () => {
    var model = new FormModel(); var context = new EditContext(model); var messages = new ValidationMessageStore(context);
    messages.Add(new FieldIdentifier(model, nameof(model.Name)), "Name is required.");
    RenderFragment body = builder => {
        builder.OpenComponent<Field>(0); builder.AddAttribute(1, "Id", "name"); builder.AddAttribute(2, "Label", "Name"); builder.AddAttribute(3, "Required", true);
        builder.AddAttribute(4, "For", (Expression<Func<object?>>)(() => model.Name));
        builder.AddAttribute(5, "ChildContent", (RenderFragment)(input => {
            input.OpenComponent<TextBox>(0); input.AddAttribute(1, "Value", model.Name);
            input.AddAttribute(2, "ValueExpression", (Expression<Func<string?>>)(() => model.Name)); input.CloseComponent();
        })); builder.CloseComponent();
    };
    var html = await Render<CascadingValue<EditContext>>(new() { ["Value"] = context, ["ChildContent"] = body });
    Check(html.Contains("for=\"name\"") && html.Contains("id=\"name\""), "Label and native input were not associated.");
    Check(html.Contains("required") && html.Contains("aria-invalid=\"true\""), "Required/invalid input state is missing.");
    Check(html.Contains("aria-describedby=\"name-error\"") && html.Contains("id=\"name-error\"") && html.Contains("Name is required."), "Validation error is not described by the input.");
});
AsyncTest("consumer field errors remain visible and mark the input invalid", async () => {
    var model = new FormModel();
    var html = await Render<Field>(new() { ["Id"] = "external", ["Label"] = "Name", ["Error"] = "Provider rejected this value.", ["ChildContent"] = (RenderFragment)(builder => {
        builder.OpenComponent<TextBox>(0); builder.AddAttribute(1, "Value", model.Name); builder.AddAttribute(2, "ValueExpression", (Expression<Func<string?>>)(() => model.Name)); builder.CloseComponent();
    }) });
    Check(html.Contains("aria-describedby=\"external-error\"") && html.Contains("aria-invalid=\"true\""), "An explicit consumer error was not reflected in the native input state.");
});
AsyncTest("form layout composes model-bound multiline input without interpolating markup", async () => {
    var model = new FormModel { Name = "<unsafe textarea>" };
    var html = await Render<FormLayout>(new() { ["Columns"] = 2, ["ChildContent"] = (RenderFragment)(builder => {
        builder.OpenComponent<TextBox>(0); builder.AddAttribute(1, "Id", "notes"); builder.AddAttribute(2, "Multiline", true); builder.AddAttribute(3, "Value", model.Name);
        builder.AddAttribute(4, "ValueExpression", (Expression<Func<string?>>)(() => model.Name)); builder.CloseComponent();
    }) });
    Check(html.Contains("--f-form-columns:2") && html.Contains("<textarea") && html.Contains("id=\"notes\""), "Form composition lost its model-bound control.");
    Check(html.Contains("&lt;unsafe textarea&gt;") && !html.Contains("<unsafe textarea>"), "Textarea content became HTML.");
});
AsyncTest("bottom sheet keeps its accessible title and Busy close policy", async () => {
    var html = await Render<BottomSheet>(new() { ["Id"] = "review", ["Title"] = "Review <record>", ["IsOpen"] = true, ["Busy"] = true, ["CloseLabel"] = "Close review" });
    Check(html.Contains("f-bottom-sheet") && html.Contains("aria-modal=\"true\"") && html.Contains("aria-labelledby=\"review-title\""), "Modal title relationship is missing.");
    Check(html.Contains("aria-busy=\"true\"") && html.Contains("disabled") && html.Contains("aria-label=\"Close review\""), "A Busy sheet can be dismissed through its close control.");
    Check(html.Contains("Review &lt;record&gt;"), "Dialog title was not encoded.");
});
AsyncTest("dialog close guard retains values on refusal and rejects concurrent close", async () => {
    var dialog = new Dialog(); var closed = 0; var gateCalls = 0;
    void Set(string property, object? value) => typeof(Dialog).GetProperty(property)!.SetValue(dialog, value);
    Set("IsOpen", true); Set("Busy", true); Set("IsOpenChanged", EventCallback.Factory.Create<bool>(new object(), (bool _) => closed++));
    await dialog.RequestCloseAsync(); Equal(0, closed);
    Set("Busy", false); Set("CanClose", (Func<Task<bool>>)(() => Task.FromResult(false)));
    await dialog.RequestCloseAsync(); Equal(0, closed);
    var completion = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
    Set("CanClose", (Func<Task<bool>>)(() => { gateCalls++; return completion.Task; }));
    var first = dialog.RequestCloseAsync(); await dialog.RequestCloseAsync(); Equal(1, gateCalls);
    completion.SetResult(true); await first; Equal(1, closed);
    await dialog.DisposeAsync(); await dialog.RequestCloseAsync(); Equal(1, closed);
});

AsyncTest("host-supplied navigation and title override configured shell data", async () => {
    IReadOnlyList<NavigationGroup> runtime = [new("allowed", "Allowed module", "page", [new("Allowed page", "/records", Exact: true)])];
    var html = await Render<ApplicationShell>(new() { ["NavigationGroups"] = runtime, ["ApplicationTitle"] = "Organization <North>" });
    Check(html.Contains("Allowed module") && html.Contains("Allowed page") && !html.Contains("Unavailable"), "Shell merged configured navigation into the host's filtered override.");
    Check(html.Contains("Organization &lt;North&gt;") && !html.Contains("Test Application"), "Runtime organization title was not substituted and encoded.");
    var empty = await Render<ApplicationShell>(new() { ["NavigationGroups"] = Array.Empty<NavigationGroup>(), ["ApplicationTitle"] = "Account" });
    Check(empty.Contains("f-no-navigation") && !empty.Contains("f-primary-navigation"), "An empty host navigation leaked startup routes.");
});
AsyncTest("nested routes select only the most specific secondary item", async () => {
    IReadOnlyList<NavigationGroup> runtime = [new("records", "Records", "list", [new("List", "/records"), new("Detail", "/records/sample")])];
    var detail = await Render<ApplicationShell>(new() { ["NavigationGroups"] = runtime }, path: "/records/sample");
    Equal(1, detail.Split("aria-current=\"page\"", StringSplitOptions.None).Length - 1);
    Check(detail.Contains("href=\"/records/sample\" class=\"f-secondary-item is-selected\""), "The parent route displaced the specific detail page.");
    var other = await Render<ApplicationShell>(new() { ["NavigationGroups"] = runtime }, path: "/records/other");
    Equal(1, other.Split("aria-current=\"page\"", StringSplitOptions.None).Length - 1);
    Check(other.Contains("href=\"/records\" class=\"f-secondary-item is-selected\""), "The parent stopped matching an unlisted record.");
});
AsyncTest("rail-only navigation omits secondary chrome while keeping the module", async () => {
    IReadOnlyList<NavigationGroup> runtime = [new("overview", "Overview", "home", [new("Home", "/", Exact: true)], SecondaryNavigation: false)];
    var html = await Render<ApplicationShell>(new() { ["NavigationGroups"] = runtime });
    Check(html.Contains("f-no-secondary") && html.Contains("f-primary-navigation") && !html.Contains("f-secondary-navigation"), "A module without secondary pages reserved a phantom rail.");
});
AsyncTest("shell brand and start slots render without host-owned layout markup", async () => {
    RenderFragment brand = b => b.AddContent(0, "Brand glyph");
    RenderFragment start = b => b.AddContent(0, "Service menu");
    var html = await Render<ApplicationShell>(new() { ["TitleBarBrand"] = brand, ["TitleBarStart"] = start });
    Check(html.Contains("f-application-logo") && html.Contains("Brand glyph") && html.Contains("f-titlebar-start") && html.Contains("Service menu"), "Shell slots lost their library-owned tracks.");
});
AsyncTest("unknown progress stays indeterminate while actual values expose progress semantics", async () => {
    var unknown = await Render<ProgressBar>(new() { ["StatusText"] = "Waiting for server" });
    Check(unknown.Contains("role=\"progressbar\"") && unknown.Contains("f-progress-indeterminate") && !unknown.Contains("aria-valuenow") && !unknown.Contains("0%"), "Unknown progress fabricated a zero value.");
    var actual = await Render<ProgressRing>(new() { ["Value"] = 37.5d, ["StatusText"] = "Preparing", ["Stopped"] = true });
    Check(actual.Contains("aria-valuenow=\"37.5\"") && actual.Contains("aria-valuetext=\"Preparing\"") && actual.Contains("f-progress-stopped"), "Actual progress or its stopped state was lost.");
    foreach (var value in new[] { -1d, 101d, double.NaN, double.PositiveInfinity }) {
        var rejected = false;
        try { await Render<ProgressBar>(new() { ["Value"] = value }); } catch (ArgumentOutOfRangeException) { rejected = true; }
        Check(rejected, "A nonfinite or out-of-range progress value reached the DOM.");
    }
});
AsyncTest("native numeric values remain invariant under a decimal-comma locale", async () => {
    var previous = CultureInfo.CurrentCulture;
    try {
        CultureInfo.CurrentCulture = culture;
        var model = new FormModel { Amount = 12.5m };
        var html = await Render<NumberBox<decimal?>>(new() { ["Id"] = "amount", ["Value"] = model.Amount, ["ValueExpression"] = (Expression<Func<decimal?>>)(() => model.Amount) });
        Check(html.Contains("type=\"number\"") && html.Contains("value=\"12.5\"") && !html.Contains("value=\"12,5\""), "Localized numeric formatting invalidated a native number input.");
    } finally { CultureInfo.CurrentCulture = previous; }
});

var passed = 0;
foreach (var test in tests) {
    try { await test.Run(); Console.WriteLine($"PASS {test.Name}"); passed++; }
    catch (Exception error) { Console.Error.WriteLine($"FAIL {test.Name}: {error}"); }
}
Console.WriteLine($"{passed}/{tests.Count} checks passed.");
return passed == tests.Count ? 0 : 1;

internal sealed record Record(int Id, string? Name, string Email, decimal? Amount, DateOnly? Date);
internal sealed class TestNavigation : NavigationManager {
    internal TestNavigation(string path = "/records") { Initialize("https://test.example/", new Uri(new Uri("https://test.example/"), path).ToString()); }
    protected override void NavigateToCore(string uri, bool forceLoad) { Uri = ToAbsoluteUri(uri).ToString(); NotifyLocationChanged(false); }
}
internal sealed class FakeJs : IJSRuntime {
    public ValueTask<TValue> InvokeAsync<TValue>(string identifier, object?[]? args) => new(default(TValue)!);
    public ValueTask<TValue> InvokeAsync<TValue>(string identifier, CancellationToken cancellationToken, object?[]? args) => new(default(TValue)!);
}

internal sealed class FormModel { public string? Name { get; set; } public decimal? Amount { get; set; } }
