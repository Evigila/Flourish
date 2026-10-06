using System.Globalization;
using System.Net;
using System.Linq.Expressions;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Components.Forms;
using ApplicationTheme = ArkheideSystem.Flourish.Abstract.ApplicationTheme;
using NotificationSeverity = ArkheideSystem.Flourish.Abstract.NotificationSeverity;
using CommandExecutionStatus = ArkheideSystem.Flourish.Abstract.CommandExecutionStatus;
using CommandSource = ArkheideSystem.Flourish.Abstract.CommandSource;
using ICommandDispatcher = ArkheideSystem.Flourish.Abstract.ICommandDispatcher;
using ICommandParser = ArkheideSystem.Flourish.Abstract.ICommandParser;
using ICommandRegistrar = ArkheideSystem.Flourish.Abstract.ICommandRegistrar;
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
string? AttributeValue(string tag, string attribute) {
    var match = Regex.Match(tag, $"(?:^|\\s){Regex.Escape(attribute)}=\"([^\"]*)\"");
    return match.Success ? WebUtility.HtmlDecode(match.Groups[1].Value) : null;
}
IReadOnlyList<string> OpeningTags(string html, string tag) => Regex.Matches(html, $"<{Regex.Escape(tag)}\\b[^>]*>").Select(match => match.Value).ToArray();
IReadOnlyList<string> TagsWithClass(string html, string cssClass) => Regex.Matches(html, "<[a-z][a-z0-9]*\\b[^>]*>")
    .Select(match => match.Value).Where(tag => AttributeValue(tag, "class")?.Split(' ', StringSplitOptions.RemoveEmptyEntries).Contains(cssClass) == true).ToArray();
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

Test("contracts and framework keep one-way dependencies after Shared consolidation", () => {
    var contracts = typeof(IFrameworkBuilder).Assembly;
    Equal(contracts, typeof(AppearanceState).Assembly);
    Equal(contracts, typeof(TableColumn<Record>).Assembly);
    Equal(contracts, typeof(ArkheideSystem.Flourish.Blazor.Components.Primitives.GridCell).Assembly);
    Equal(typeof(ApplicationTheme).Assembly, typeof(NotificationSeverity).Assembly);
    Equal(typeof(Button).Assembly, typeof(TableData<Record>).Assembly);
    Equal(typeof(Button).Assembly, typeof(ArkheideSystem.Flourish.Blazor.Components.Primitives.InputMaskFormatter).Assembly);
    Check(!contracts.GetReferencedAssemblies().Any(reference => reference.Name is "Flourish.Blazor.Framework" or "Flourish.Blazor.Design" or "Flourish.Blazor.Shared"),
        "Contracts must not depend on rendering, optional design or the retired Shared assembly.");
    Check(!typeof(Button).Assembly.GetReferencedAssemblies().Any(reference => reference.Name is "Flourish.Blazor.Design" or "Flourish.Blazor.Shared"),
        "Framework must keep optional design and retired Shared outside its dependency graph.");
});
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
    var services = new ServiceCollection(); services.AddFlourishFramework(); services.AddFlourishDesign();
    using var provider = services.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true });
    using var first = provider.CreateScope(); using var second = provider.CreateScope();
    var a = first.ServiceProvider.GetRequiredService<IAppearanceService>(); var b = second.ServiceProvider.GetRequiredService<IAppearanceService>();
    a.SetColors("#ffffff", "#ffeebb"); a.SetTheme(ApplicationTheme.Dark);
    Equal("#FFFFFF", a.Current.Primary); Equal(ApplicationTheme.Dark, a.Current.Theme);
    Equal("#153A32", b.Current.Primary); Equal(ApplicationTheme.Light, b.Current.Theme);
});
Test("unchanged appearance does not emit duplicate state notifications", () => {
    var services = new ServiceCollection(); services.AddFlourishFramework(); services.AddFlourishDesign(); using var provider = services.BuildServiceProvider(); using var scope = provider.CreateScope();
    var appearance = scope.ServiceProvider.GetRequiredService<IAppearanceService>(); var count = 0;
    appearance.Changed += (_, _) => count++;
    appearance.SetColors("#153a32", "#16745f"); appearance.SetTheme(ApplicationTheme.Light); Equal(0, count);
    appearance.SetTheme(ApplicationTheme.Dark); appearance.SetTheme(ApplicationTheme.Dark); Equal(1, count);
});
Test("duplicate framework registration is rejected", () => { var services = new ServiceCollection(); services.AddFlourishFramework(); services.AddFlourishDesign(); Throws<InvalidOperationException>(() => services.AddFlourishFramework()); });
Test("captured startup and nested builders cannot mutate completed options", () => {
    IFrameworkBuilder? framework = null; IProjectBuilder? project = null; ITopBarBuilder? top = null;
    IAppearanceBuilder? appearance = null; ILayoutBuilder? layout = null; ISubNavigationBuilder? secondary = null;
    var services = new ServiceCollection();
    services.AddFlourishFramework(builder => {
        framework = builder;
        builder.ConfigureProject(value => { project = value; value.SetProjectName("Test"); });
        builder.ConfigureTopBar(value => { top = value; value.SetSearch(); });
        builder.ConfigureLayout(value => layout = value);
        builder.ConfigureNavigation(value => value.AddNav("Pages", "description", "/", value => { secondary = value; value.AddSubNav("Home", "home", "/", exact: true); }));
    });
    services.AddFlourishDesign(value => appearance = value);
    Throws<InvalidOperationException>(() => framework!.ConfigureTopBar(_ => { }));
    Throws<InvalidOperationException>(() => project!.SetProjectName("Late"));
    Throws<InvalidOperationException>(() => top!.SetSearch(false));
    Throws<InvalidOperationException>(() => appearance!.SetTheme(ApplicationTheme.Dark));
    Throws<InvalidOperationException>(() => layout!.SetContentWidth(900));
    Throws<InvalidOperationException>(() => secondary!.AddSubNav("Late", "description", "/late"));
});
Test("navigation rejects external/scheme-relative routes and duplicate destinations", () => {
    foreach (var href in new[] { "https://example.test", "//example.test", "javascript:alert(1)", "/a\\b" }) {
        var services = new ServiceCollection();
        Throws<ArgumentException>(() => services.AddFlourishFramework(framework => framework.ConfigureNavigation(nav => nav.AddNav("Pages", "description", "/pages", secondary => secondary.AddSubNav("Bad", "description", href)))));
    }
    var duplicate = new ServiceCollection();
    Throws<ArgumentException>(() => duplicate.AddFlourishFramework(framework => framework.ConfigureNavigation(nav => nav.AddNav("One", "description", "/same").AddNav("Two", "description", "/same"))));
});
Test("framework registration keeps commands scoped to each user circuit", () => {
    var services = new ServiceCollection();
    services.AddScoped<CommandState>();
    services.AddFlourishFramework(framework => framework
        .ConfigureProject(project => project.SetProjectName("Test"))
        .ConfigureTopBar(top => top.AddMenu("Actions", menu => menu.AddMenuItem("Run", TestCommandParser.CommandKey)))
        .ConfigureNavigation(navigation => navigation
            .AddNav("Home", "home", "/", exact: true)
            .AddNavButton("Run", "play_arrow", TestCommandParser.CommandKey)
            .AddFixedNav("Settings", "settings", "/settings")
            .AddFixedNavButton("Run fixed", "play_arrow", TestCommandParser.CommandKey))
        .SetCommandParser<TestCommandParser>());
    using var provider = services.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true });
    using var first = provider.CreateScope();
    using var second = provider.CreateScope();
    var firstDispatcher = first.ServiceProvider.GetRequiredService<ICommandDispatcher>();
    var secondDispatcher = second.ServiceProvider.GetRequiredService<ICommandDispatcher>();
    Check(firstDispatcher.CanExecute(TestCommandParser.CommandKey, source: CommandSource.Navigation), "Configured command was not registered in the first scope.");
    Equal(CommandExecutionStatus.Handled, firstDispatcher.ExecuteAsync(TestCommandParser.CommandKey).AsTask().GetAwaiter().GetResult().Status);
    Equal(1, first.ServiceProvider.GetRequiredService<CommandState>().Count);
    Equal(0, second.ServiceProvider.GetRequiredService<CommandState>().Count);
});
AsyncTest("primary and secondary landing routes can also be their selected child pages", async () => {
    var html = await RenderFramework(framework => framework.ConfigureNavigation(nav => nav
        .AddNav("Module", "widgets", "/module", secondary => secondary
            .AddSubNav("Start", "home", "/module", exact: true)
            .AddSubNav("Detail", "description", "/module/detail"))), "/module");
    Check(html.Contains("href=\"/module\" class=\"f-secondary-item is-selected\""), "The landing page was not selected in secondary navigation.");
    var duplicateChildren = new ServiceCollection();
    Throws<ArgumentException>(() => duplicateChildren.AddFlourishFramework(framework => framework.ConfigureNavigation(nav => nav
        .AddNav("Module", "widgets", "/module", secondary => secondary
            .AddSubNav("Start", "home", "/module")
            .AddSubNav("Repeated", "home", "/module/")))));
    var duplicateModules = new ServiceCollection();
    Throws<ArgumentException>(() => duplicateModules.AddFlourishFramework(framework => framework.ConfigureNavigation(nav => nav
        .AddNav("One", "home", "/same").AddNav("Two", "home", "/same"))));
    var nested = await RenderFramework(framework => framework.ConfigureNavigation(nav => nav
        .AddNav("Controls", "widgets", "/controls", secondary => secondary
            .AddSubNav("Inputs", "input", "/controls/inputs", third => third
                .AddSubNav("Input overview", "home", "/controls/inputs", exact: true)
                .AddSubNav("Select", "list", "/controls/inputs/select")))), "/controls/inputs");
    var currentChild = TagsWithClass(nested, "f-third-item").Single(tag => AttributeValue(tag, "aria-current") == "page");
    Equal("/controls/inputs", AttributeValue(currentChild, "href"));
    Equal("true", AttributeValue(TagsWithClass(nested, "f-secondary-toggle").Single(), "aria-expanded"));
    var duplicateLandingChildren = new ServiceCollection();
    Throws<ArgumentException>(() => duplicateLandingChildren.AddFlourishFramework(framework => framework.ConfigureNavigation(nav => nav
        .AddNav("Controls", "widgets", "/controls", secondary => secondary
            .AddSubNav("Inputs", "input", "/controls/inputs", third => third
                .AddSubNav("Input overview", "home", "/controls/inputs")
                .AddSubNav("Repeated overview", "home", "/CONTROLS/INPUTS/"))))));
});
Test("leaf and branch secondary builders freeze at completion", () => {
    ISubNavigationBuilder? secondary = null, third = null;
    var services = new ServiceCollection();
    services.AddFlourishFramework(framework => framework.ConfigureNavigation(nav => nav
        .AddNav("Controls", "widgets", "/controls", value => {
            secondary = value;
            value.AddSubNav("Overview", "home", "/controls", true, false)
                .AddSubNav("Inputs", "input", "/controls/inputs", children => {
                    third = children;
                    children.AddSubNav("Select", "list", "/controls/inputs/select", true, false);
                });
        })));
    Throws<InvalidOperationException>(() => secondary!.AddSubNav("Late", "description", "/late"));
    Throws<InvalidOperationException>(() => third!.AddSubNav("Late child", "description", "/late-child"));
    Throws<InvalidOperationException>(() => third!.AddSubNav("Late branch", "description", "/late-branch", children => children.AddSubNav("Leaf", "description", "/late-leaf")));
});
Test("nested navigation rejects unsafe and duplicated routes across the entire shell", () => {
    foreach (var target in new[] { "https://example.test", "//example.test", "javascript:alert(1)", "/bad\\route" }) {
        var services = new ServiceCollection();
        Throws<ArgumentException>(() => services.AddFlourishFramework(framework => framework.ConfigureNavigation(nav => nav
            .AddNav("Controls", "widgets", "/controls", secondary => secondary
                .AddSubNav("Inputs", "input", "/controls/inputs", third => third.AddSubNav("Unsafe", "description", target))))));
    }
    var repeatedChildren = new ServiceCollection();
    Throws<ArgumentException>(() => repeatedChildren.AddFlourishFramework(framework => framework.ConfigureNavigation(nav => nav
        .AddNav("Controls", "widgets", "/controls", secondary => secondary
            .AddSubNav("Inputs", "input", "/controls/inputs", third => third
                .AddSubNav("Select", "list", "/controls/inputs/select")
                .AddSubNav("Repeated", "list", "/CONTROLS/INPUTS/SELECT/"))))));
    var repeatedAcrossLevels = new ServiceCollection();
    Throws<ArgumentException>(() => repeatedAcrossLevels.AddFlourishFramework(framework => framework.ConfigureNavigation(nav => nav
        .AddNav("Controls", "widgets", "/controls", secondary => secondary
            .AddSubNav("Inputs", "input", "/controls/inputs", third => third.AddSubNav("Select", "list", "/controls/select"))
            .AddSubNav("Repeated", "description", "/controls/select")))));
    var repeatedAcrossModules = new ServiceCollection();
    Throws<ArgumentException>(() => repeatedAcrossModules.AddFlourishFramework(framework => framework.ConfigureNavigation(nav => nav
        .AddNav("Controls", "widgets", "/controls", secondary => secondary
            .AddSubNav("Inputs", "input", "/controls/inputs", third => third.AddSubNav("Select", "list", "/controls/select")))
        .AddNav("Repeated module", "description", "/controls/select"))));
});
Test("navigation children remain optional and record copies expose their current child collection", () => {
    var leaf = new NavigationItem("Leaf", "/leaf");
    Equal(0, leaf.ChildItems.Count);
    IReadOnlyList<NavigationItem> children = [leaf];
    var branch = leaf with { Children = children };
    Check(ReferenceEquals(children, branch.ChildItems), "A record copy retained stale child metadata.");
    Equal(0, (branch with { Children = null }).ChildItems.Count);
});
AsyncTest("the bundled icon catalog renders only official names", async () => {
    Equal("description", new Icon().Name);
    Equal("description", new NavigationItem("Default", "/default").Icon);
    var defaultHtml = await Render<Icon>(new());
    Check(defaultHtml.Contains("data-icon=\"description\""), "The default icon retained a deleted alias.");
    Equal(4299, IconCatalog.Names.Count, "The pinned Material Symbols name map did not load.");
    Check(IconCatalog.Contains("inventory_2") && IconCatalog.Contains("search"), "Official icon names were missing.");
    Check(!IconCatalog.Contains("user") && !IconCatalog.Contains("arrow-left"), "Retired icon aliases remain available.");
    Check(!IconCatalog.Contains("an_unknown_icon") && !IconCatalog.Contains(null), "Unknown names were accepted.");
    var html = await Render<Icon>(new() { ["Name"] = "person" });
    Check(html.Contains("data-icon=\"person\"") && html.Contains("aria-hidden=\"true\""), "The official icon or decorative semantics were lost.");
    Check(!html.Contains(">person<"), "The icon rendered a text ligature instead of a mapped glyph.");
});
AsyncTest("program configuration renders top bar menus, explicit navigation and fixed commands", async () => {
    var html = await RenderFramework(framework => framework
        .ConfigureProject(project => project
            .SetProjectName("Configured application")
            .SetLogo("app.svg", "Application icon"))
        .ConfigureTopBar(top => top
            .AddMenu("Actions", menu => menu.AddMenuItem("Run action", TestCommandParser.CommandKey))
            .InjectToLeft<TestInjectedComponent>()
            .InjectToCenter<TestInjectedComponent>()
            .InjectToRight<TestInjectedComponent>())
        .ConfigureNavigation(navigation => navigation
            .AddNav("Records", "list", "/records", secondary => secondary.AddSubNav("Detail", "description", "/records/sample"))
            .AddNavButton("Run", "play_arrow", TestCommandParser.CommandKey)
            .AddFixedNav("Settings", "settings", "/settings")
            .AddFixedNavButton("Run fixed", "play_arrow", TestCommandParser.CommandKey))
        .SetCommandParser<TestCommandParser>(), "/records/sample");
    Check(html.Contains("Configured application"), "Configured application name was not rendered.");
    Check(html.Contains("src=\"app.svg\""), "Configured application icon was not rendered.");
    Check(html.Contains("Run action"), "Configured top bar menu item was not rendered.");
    Check(html.Contains("f-action-menu-hover"), "Configured top bar menus did not use hover mode.");
    Equal(3, html.Split("Scoped injection", StringSplitOptions.None).Length - 1, "Configured top bar components were not created in all three regions.");
    foreach (var track in new[] { "start", "center", "end" })
        Check(TagsWithClass(html, "f-titlebar-" + track).Single() is { } tag && AttributeValue(tag, "class")!.Split(' ').Contains("f-topbar-slot"), "Configured top-bar injection bypasses common centering: " + track);
    Check(html.Contains("f-secondary-navigation") && html.Contains("href=\"/records/sample\" class=\"f-secondary-item is-selected\""), "Explicit secondary navigation was not selected.");
    Check(html.Contains("f-primary-navigation-fixed") && html.Contains("aria-label=\"Run fixed\""), "Fixed command navigation was not rendered.");
});
Test("palette configuration emits mode seeds without deriving extra colors", () => {
    var defaults = AppearancePalette.Create("#153a32", "#16745f");
    Equal("#153A32", defaults.Primary); Equal("#16745F", defaults.Accent);
    Equal("#BBD7C9", defaults.DarkPrimary); Equal("#75CBB2", defaults.DarkAccent);
    var custom = AppearancePalette.Create("#55aaee", "#ffee99");
    Equal("#55AAEE", custom.Primary); Equal(custom.Primary, custom.DarkPrimary);
    Equal("#FFEE99", custom.Accent); Equal(custom.Accent, custom.DarkAccent);
    Check(defaults.CssVariables.Contains("--f-primary-light:#153A32", StringComparison.Ordinal)
        && defaults.CssVariables.Contains("--f-accent-light:#16745F", StringComparison.Ordinal)
        && defaults.CssVariables.Contains("--f-primary-dark:#BBD7C9", StringComparison.Ordinal)
        && defaults.CssVariables.Contains("--f-accent-dark:#75CBB2", StringComparison.Ordinal)
        && !defaults.CssVariables.Contains("--f-primary:", StringComparison.Ordinal),
        "Palette output bypassed mode-specific role selection.");
    Check(AppearancePalette.Contrast("#000000", "#FFFFFF") > 20
        && AppearancePalette.Contrast("#153A32", "#153A32") == 1,
        "The retained contrast utility no longer reports standard ratios.");
});
Test("invalid colors and CSS font declarations cannot reach theme variables", () => {
    foreach (var value in new[] { "red", "#fff", "#GG0000", "#000000;background:red" }) Throws<ArgumentException>(() => AppearancePalette.Create(value, "#000000"));
    var services = new ServiceCollection(); Throws<ArgumentException>(() => services.AddFlourishDesign(appearance => appearance.SetFont("Segoe UI; color:red")));
});

async Task<string> Render<TComponent>(Dictionary<string, object?>? parameters = null, Action<IFrameworkBuilder>? configure = null, string path = "/records") where TComponent : IComponent {
    var services = new ServiceCollection();
    services.AddLogging(); services.AddSingleton<NavigationManager>(new TestNavigation(path)); services.AddSingleton<IJSRuntime>(new FakeJs());
    services.AddFlourishFramework(builder => {
        builder.ConfigureProject(project => project.SetProjectName("Test Application")).ConfigureTopBar(_ => { });
        if (configure is not null) configure(builder);
        else builder.ConfigureNavigation(nav => nav.AddNav("Records", "list", "/records", secondary => secondary
            .AddSubNav("List", "list", "/records", exact: true)
            .AddSubNav("Unavailable", "block", "/disabled", disabled: true)));
    });
    using var provider = services.BuildServiceProvider(); using var scope = provider.CreateScope();
    await using var renderer = new HtmlRenderer(scope.ServiceProvider, provider.GetRequiredService<ILoggerFactory>());
    return await renderer.Dispatcher.InvokeAsync(async () => {
        var output = await renderer.RenderComponentAsync<TComponent>(ParameterView.FromDictionary(parameters ?? new()));
        return output.ToHtmlString();
    });
}
async Task<string> RenderFramework(Action<IFrameworkBuilder> configure, string path) {
    var services = new ServiceCollection();
    services.AddLogging();
    services.AddScoped<CommandState>();
    services.AddSingleton<NavigationManager>(new TestNavigation(path));
    services.AddSingleton<IJSRuntime>(new FakeJs());
    services.AddFlourishFramework(configure);
    using var provider = services.BuildServiceProvider();
    using var scope = provider.CreateScope();
    await using var renderer = new HtmlRenderer(scope.ServiceProvider, provider.GetRequiredService<ILoggerFactory>());
    return await renderer.Dispatcher.InvokeAsync(async () => {
        var output = await renderer.RenderComponentAsync<ApplicationShell>();
        return output.ToHtmlString();
    });
}
var pageRecords = Enumerable.Range(1, 25).Select(id => new Record(id, $"Record {id:00}", $"r{id}@example.test", id, new(2026, 1, 1))).ToArray();
AsyncTest("table defaults to a 10-row page with synchronized controls", async () => {
    var html = WebUtility.HtmlDecode(await Render<DataTable<Record>>(new() { ["Items"] = pageRecords, ["Columns"] = columns, ["ItemKey"] = (Func<Record, object>)(row => row.Id) }));
    Equal(11, html.Split("<tr", StringSplitOptions.None).Length - 1);
    Check(html.Contains("Items 1-10 / Total 25"), "Actual loaded item count was lost.");
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
    Equal(1, TagsWithClass(html, "f-sr-only").Count, "Busy status is not functionally hidden.");
    Equal("status", AttributeValue(TagsWithClass(html, "f-busy-text").Single(), "role"));
});
AsyncTest("standard button variants expose one explicit identity per variant", async () => {
    foreach (var (variant, expectedClass) in new[] {
        (ButtonVariant.Primary, "f-button-primary"), (ButtonVariant.Secondary, "f-button-secondary"),
        (ButtonVariant.Danger, "f-button-danger"), (ButtonVariant.Quiet, "f-button-quiet"),
        (ButtonVariant.Underline, "f-button-underline"), (ButtonVariant.Elevated, "f-button-elevated") }) {
        var html = await Render<Button>(new() { ["Variant"] = variant });
        Equal(1, TagsWithClass(html, expectedClass).Count);
        Equal(0, TagsWithClass(html, "f-uniform-grid-button").Count);
    }
});
AsyncTest("button text and icon parameters infer icon-only geometry while preserving child content", async () => {
    var labeled = await Render<Button>(new() { ["Icon"] = "add", ["Text"] = "Create <record>" });
    Equal(1, TagsWithClass(labeled, "f-icon").Count);
    Equal(1, TagsWithClass(labeled, "f-button-text").Count);
    Equal(0, TagsWithClass(labeled, "f-button-icon").Count);
    Check(labeled.Contains("Create &lt;record&gt;"), "Text was not encoded.");
    var icon = await Render<Button>(new() { ["Icon"] = "add", ["AdditionalAttributes"] = new Dictionary<string, object> { ["aria-label"] = "Create record" } });
    Equal(1, TagsWithClass(icon, "f-button-icon").Count);
    Equal("Create record", AttributeValue(OpeningTags(icon, "button").Single(), "aria-label"));
    var slotted = await Render<Button>(new() { ["Icon"] = "add", ["ChildContent"] = (RenderFragment)(builder => builder.AddContent(0, "Slotted label")) });
    Equal(0, TagsWithClass(slotted, "f-button-icon").Count);
    Check(slotted.Contains("Slotted label"), "The semantic content slot was dropped.");
    var blank = await Render<Button>();
    Equal(0, TagsWithClass(blank, "f-icon").Count, "An empty default Icon rendered a fallback glyph.");
});
AsyncTest("button href renders a link and unavailable links cannot retain navigation targets", async () => {
    var link = await Render<Button>(new() { ["Href"] = "/records", ["Text"] = "Records", ["Variant"] = ButtonVariant.Underline });
    var anchor = OpeningTags(link, "a").Single();
    Equal("/records", AttributeValue(anchor, "href"));
    Equal(0, OpeningTags(link, "button").Count);
    Equal(1, TagsWithClass(link, "f-button-underline").Count);
    foreach (var unavailableParameter in new[] { "Disabled", "Busy" }) {
        var disabled = await Render<Button>(new() { ["Href"] = "/records", [unavailableParameter] = true,
            ["AdditionalAttributes"] = new Dictionary<string, object> { ["HREF"] = "/other", ["tabindex"] = 0 } });
        var blocked = OpeningTags(disabled, "a").Single();
        Equal<string?>(null, AttributeValue(blocked, "href"));
        Check(!Regex.IsMatch(blocked, "\\bhref=", RegexOptions.IgnoreCase), "An unmatched attribute restored a blocked navigation target.");
        Equal("true", AttributeValue(blocked, "aria-disabled"));
        Equal("-1", AttributeValue(blocked, "tabindex"));
        Equal("link", AttributeValue(blocked, "role"));
    }
});
AsyncTest("fragment buttons remain on their current document under a root base href", async () => {
    const string path = "/controls/actions/buttonsample?mode=draft#old-status";
    foreach (var href in new[] { "#button-draft-status", "#" }) {
        var button = await Render<Button>(new() { ["Href"] = href }, path: path);
        var split = await Render<SplitButton>(new() { ["Href"] = href }, path: path);
        var grid = await Render<UniformGridButton>(new() { ["Href"] = href }, path: path);
        foreach (var html in new[] { button, split, grid })
            Equal("https://test.example/controls/actions/buttonsample?mode=draft" + href,
                AttributeValue(OpeningTags(html, "a").Single(), "href"));
    }
    var routed = await Render<Button>(new() { ["Href"] = "../records?view=cards#result" }, path: path);
    Equal("../records?view=cards#result", AttributeValue(OpeningTags(routed, "a").Single(), "href"));
    var external = await Render<SplitButton>(new() { ["Href"] = "https://other.example/help#buttons" }, path: path);
    Equal("https://other.example/help#buttons", AttributeValue(OpeningTags(external, "a").Single(), "href"));
});
AsyncTest("page heading parent navigation uses the standard underline button above the title", async () => {
    var html = await Render<PageHeading>(new() { ["Title"] = "Draft", ["ParentLabel"] = "Records", ["ParentHref"] = "/records" });
    var parent = TagsWithClass(html, "f-button-underline").Single();
    Equal("/records", AttributeValue(parent, "href"));
    Equal(1, TagsWithClass(html, "f-icon").Count);
    Check(html.IndexOf(parent, StringComparison.Ordinal) < html.IndexOf("<h1", StringComparison.Ordinal), "Parent navigation was rendered after the title.");
    Check(html.Contains("Records"), "The parent button lost its text.");
});
AsyncTest("uniform grid button forwards native and busy semantics without a wrapping element", async () => {
    var html = await Render<UniformGridButton>(new() {
        ["Type"] = "submit", ["Busy"] = true, ["BusyLabel"] = "Saving local draft", ["Class"] = "host-action",
        ["AdditionalAttributes"] = new Dictionary<string, object> { ["aria-label"] = "Save draft" },
        ["ChildContent"] = (RenderFragment)(builder => builder.AddContent(0, "Save <record>")) });
    var button = OpeningTags(html, "button").Single();
    Equal("submit", AttributeValue(button, "type"));
    Equal("true", AttributeValue(button, "aria-busy"));
    Equal("Save draft", AttributeValue(button, "aria-label"));
    Equal(1, TagsWithClass(html, "f-uniform-grid-button").Count);
    Equal(1, TagsWithClass(html, "host-action").Count);
    Check(button.Contains("disabled") && html.Contains("Saving local draft") && html.Contains("Save &lt;record&gt;"), "The composed grid button lost busy state or encoded content.");
    Check(!html.Contains("<div"), "A grid button introduced a wrapper and broke direct grid-cell placement.");
    var labeled = await Render<UniformGridButton>(new() { ["Icon"] = "save", ["Text"] = "Save draft" });
    Equal(1, TagsWithClass(labeled, "f-icon").Count);
    Equal(1, TagsWithClass(labeled, "f-uniform-cell-text").Count);
    Check(labeled.Contains("Save draft"), "The grid button did not forward its text parameter.");
});
AsyncTest("shared button behavior suppresses disabled and busy callbacks", async () => {
    var count = 0;
    var action = new Button();
    ParameterView.FromDictionary(new Dictionary<string, object?> {
        ["OnClick"] = EventCallback.Factory.Create<MouseEventArgs>(new object(), () => count++)
    }).SetParameterProperties(action);
    var activate = typeof(Button).GetMethod("ClickAsync", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!;
    async Task Click() => await (Task)activate.Invoke(action, [new MouseEventArgs()])!;
    void State(bool disabled, bool busy) => ParameterView.FromDictionary(new Dictionary<string, object?> {
        ["Disabled"] = disabled, ["Busy"] = busy
    }).SetParameterProperties(action);
    State(true, false); await Click();
    State(false, true); await Click();
    Equal(0, count);
    State(false, false); await Click();
    Equal(1, count);
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
    var html = await Render<Dialog>(new() { ["Presentation"] = DialogPresentation.BottomSheet, ["Id"] = "review", ["Title"] = "Review <record>", ["IsOpen"] = true, ["Busy"] = true, ["CloseLabel"] = "Close review" });
    Check(html.Contains("f-bottom-sheet") && html.Contains("aria-modal=\"true\"") && html.Contains("aria-labelledby=\"review-title\""), "Modal title relationship is missing.");
    Check(html.Contains("aria-busy=\"true\"") && html.Contains("disabled") && html.Contains("aria-label=\"Close review\""), "A Busy sheet can be dismissed through its close control.");
    Check(html.Contains("Review &lt;record&gt;"), "Dialog title was not encoded.");
});
AsyncTest("notice indicators expose named glyph buttons and unique encoded descriptions", async () => {
    var names = new Dictionary<NotificationSeverity, string> {
        [NotificationSeverity.Error] = "Error",
        [NotificationSeverity.Warning] = "Warning",
        [NotificationSeverity.Success] = "Success",
        [NotificationSeverity.Information] = "Information"
    };
    var ids = new HashSet<string>(StringComparer.Ordinal);
    foreach (var (severity, name) in names) {
        var html = await Render<ArkheideSystem.Flourish.Blazor.Components.Primitives.NoticeTrigger>(new() {
            ["Severity"] = severity, ["ChildContent"] = (RenderFragment)(builder => builder.AddContent(0, "Help <unsafe>")) });
        Check(html.Contains($"aria-label=\"{name}\""), "The glyph button lost its severity name.");
        var description = Regex.Match(html, "aria-describedby=\"([^\"]+)\"").Groups[1].Value;
        Check(ids.Add(description) && html.Contains($"id=\"{description}\""), "Descriptions have duplicate or unresolved IDs.");
        Check(html.Contains("Help &lt;unsafe&gt;") && html.Contains("role=\"note\""), "Help text lost encoding or note semantics.");
        Check(Regex.IsMatch(html, "<svg[^>]*aria-hidden=\"true\"[^>]*focusable=\"false\""), "The decorative glyph became a second focus/name target.");
        var button = Regex.Match(html, "<button[^>]*>([\\s\\S]*?)</button>").Groups[1].Value;
        Check(!button.Contains(name), "The old text severity badge remained in the button.");
    }
});
AsyncTest("subtle notice presentation retains its explicit information severity", async () => {
    var html = await Render<ArkheideSystem.Flourish.Blazor.Components.Primitives.NoticeTrigger>(new() {
        ["Severity"] = NotificationSeverity.Information, ["Subtle"] = true,
        ["ChildContent"] = (RenderFragment)(builder => builder.AddContent(0, "Additional <information>")) });
    Check(TagsWithClass(html, "notice-subtle").Count == 1 && html.Contains("aria-label=\"Information\""), "Subtle presentation changed the information severity or lost its named trigger.");
    Check(html.Contains("Additional &lt;information&gt;") && html.Contains("role=\"note\""), "Subtle help text lost encoding or note semantics.");
});
AsyncTest("dialog close guard retains values on refusal and rejects concurrent close", async () => {
    await using var fixture = new DialogFixture(); var closed = 0; var gateCalls = 0;
    await fixture.Render<Dialog>(new() { ["Title"] = "Guard", ["IsOpen"] = true, ["Busy"] = true,
        ["IsOpenChanged"] = EventCallback.Factory.Create<bool>(fixture, (bool _) => closed++) });
    var dialog = fixture.Component<Dialog>();
    Task Set(Dictionary<string, object?> parameters) => fixture.Dispatch(() => dialog.SetParametersAsync(ParameterView.FromDictionary(parameters)));
    await fixture.Dispatch(() => dialog.RequestCloseAsync()); Equal(0, closed);
    await Set(new() { ["Busy"] = false, ["CanClose"] = (Func<Task<bool>>)(() => Task.FromResult(false)) });
    await fixture.Dispatch(() => dialog.RequestCloseAsync()); Equal(0, closed);
    var completion = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
    await Set(new() { ["CanClose"] = (Func<Task<bool>>)(() => { gateCalls++; return completion.Task; }) });
    Task? first = null;
    await fixture.Dispatch(() => { first = dialog.RequestCloseAsync(); return Task.CompletedTask; });
    await fixture.Dispatch(() => dialog.RequestCloseAsync()); Equal(1, gateCalls);
    completion.SetResult(true); await first!; Equal(1, closed);
    await fixture.Dispatch(async () => { await dialog.DisposeAsync(); await dialog.RequestCloseAsync(); }); Equal(1, closed);
});

AsyncTest("configured allowed navigation and runtime title remain distinct from omitted routes", async () => {
    var html = await Render<ApplicationShell>(new() { ["ApplicationTitle"] = "Organization <North>" },
        configure: framework => framework.ConfigureNavigation(nav => nav
            .AddNav("Allowed module", "description", "/records", secondary => secondary.AddSubNav("Allowed page", "description", "/records", exact: true))));
    Check(html.Contains("Allowed module") && html.Contains("Allowed page") && !html.Contains("Unavailable"), "The configured permission-filtered navigation gained an omitted route.");
    Check(html.Contains("Organization &lt;North&gt;") && !html.Contains("Test Application"), "Runtime organization title was not substituted and encoded.");
    var empty = await Render<ApplicationShell>(new() { ["ApplicationTitle"] = "Account" }, configure: framework => framework.ConfigureNavigation(_ => { }));
    Check(empty.Contains("f-no-navigation") && !empty.Contains("f-primary-navigation"), "An empty configured navigation gained startup routes.");
});
AsyncTest("nested routes select only the most specific secondary item", async () => {
    Action<IFrameworkBuilder> configure = framework => framework.ConfigureNavigation(nav => nav
        .AddNav("Records", "list", "/records", secondary => secondary.AddSubNav("List", "list", "/records").AddSubNav("Detail", "description", "/records/sample")));
    var detail = await Render<ApplicationShell>(configure: configure, path: "/records/sample");
    Equal(1, detail.Split("aria-current=\"page\"", StringSplitOptions.None).Length - 1);
    Check(detail.Contains("href=\"/records/sample\" class=\"f-secondary-item is-selected\""), "The parent route displaced the specific detail page.");
    var other = await Render<ApplicationShell>(configure: configure, path: "/records/other");
    Equal(1, other.Split("aria-current=\"page\"", StringSplitOptions.None).Length - 1);
    Check(other.Contains("href=\"/records\" class=\"f-secondary-item is-selected\""), "The parent stopped matching an unlisted record.");
});
AsyncTest("selected third routes open their parent inside one secondary rail and share one child panel", async () => {
    var html = WebUtility.HtmlDecode(await RenderFramework(framework => framework.ConfigureNavigation(nav => nav
        .AddNav("Controls", "widgets", "/controls", secondary => secondary
            .AddSubNav("Inputs", "input", "/controls/inputs", third => third
                .AddSubNav("Select", "list", "/controls/inputs/select")
                .AddSubNav("Select details", "description", "/controls/inputs/select/details", exact: true)))), "/controls/inputs/select/details?source=test#example"));
    Equal(1, TagsWithClass(html, "f-secondary-navigation").Count, "Third-level pages created another secondary rail.");
    Equal(1, TagsWithClass(html, "f-third-navigation").Count, "Siblings were split into separate child panels.");
    Equal(2, TagsWithClass(html, "f-third-item").Count, "The shared child panel lost a configured sibling.");
    var selected = OpeningTags(html, "a").Where(tag => AttributeValue(tag, "aria-current") == "page").ToArray();
    Equal(1, selected.Length, "More than one destination was announced as the current page.");
    Equal("/controls/inputs/select/details", AttributeValue(selected[0], "href"));
    var parent = OpeningTags(html, "a").Single(tag => AttributeValue(tag, "href") == "/controls/inputs");
    Equal("true", AttributeValue(parent, "aria-current"), "The selected child lost its parent branch indication.");
    Equal("Inputs", AttributeValue(parent, "title"), "A truncated branch no longer exposes its full label.");
    var toggle = TagsWithClass(html, "f-secondary-toggle").Single();
    Equal("button", Regex.Match(toggle, "^<([a-z]+)").Groups[1].Value, "Branch expansion is not a native button.");
    Check(parent != toggle && !TagsWithClass(html, "f-secondary-item").Contains(toggle), "Branch navigation and disclosure were merged into one action.");
    Check(AttributeValue(parent, "class")!.Split(' ').Contains("is-selected") && AttributeValue(toggle, "class")!.Split(' ').Contains("is-selected"), "The selected branch and its triangle do not share their selected style.");
    Equal<string?>(null, AttributeValue(toggle, "aria-current"), "The disclosure is incorrectly announced as a second route.");
    Equal<string?>(null, AttributeValue(parent, "aria-expanded"), "The route link also controls the expansion state.");
    Equal("true", AttributeValue(toggle, "aria-expanded"), "The directly loaded child did not open its branch.");
    var panel = TagsWithClass(html, "f-third-navigation").Single();
    Check(AttributeValue(toggle, "aria-controls") is { Length: > 0 } controlled && controlled == AttributeValue(panel, "id"), "The disclosure does not control its unique child panel.");
    Check(!Regex.IsMatch(panel, "\\shidden(?:[\\s=>])"), "The selected branch remained hidden.");
    Equal(1, TagsWithClass(html, "f-expansion-indicator").Count, "The branch does not own exactly one shared triangle.");
    Equal(1, TagsWithClass(html, "f-split-button").Count, "The category link and disclosure do not share the standard SplitButton.");
    Check(parent.StartsWith("<a ") && toggle.StartsWith("<button "), "The category destination and disclosure are not independent native controls.");
    Equal("true", AttributeValue(TagsWithClass(html, "f-expansion-indicator").Single(), "data-expanded"), "The expanded branch lacks its shared marker state.");
});
AsyncTest("unselected branches remain collapsed with accessible disclosure relationships", async () => {
    var html = WebUtility.HtmlDecode(await RenderFramework(framework => framework.ConfigureNavigation(nav => nav
        .AddNav("Controls", "widgets", "/controls", secondary => secondary
            .AddSubNav("Overview", "home", "/controls", exact: true)
            .AddSubNav("Inputs", "input", "/controls/inputs", third => third.AddSubNav("Select", "list", "/controls/inputs/select"))
            .AddSubNav("Data", "table_chart", "/controls/data", third => third.AddSubNav("Table", "table_chart", "/controls/data/table")))), "/controls"));
    var toggles = TagsWithClass(html, "f-secondary-toggle");
    var panels = TagsWithClass(html, "f-third-navigation");
    Equal(2, toggles.Count); Equal(2, panels.Count);
    Check(toggles.All(tag => AttributeValue(tag, "aria-expanded") == "false"), "An unrelated branch opened by default.");
    Check(toggles.All(tag => !AttributeValue(tag, "class")!.Split(' ').Contains("is-selected")), "A collapsed unrelated branch triangle gained selected styling.");
    Check(panels.All(tag => Regex.IsMatch(tag, "\\shidden(?:[\\s=>])")), "A collapsed branch still exposes its child links.");
    Check(toggles.Select(tag => AttributeValue(tag, "aria-controls")).Order().SequenceEqual(panels.Select(tag => AttributeValue(tag, "id")).Order()), "Disclosures refer to missing child panels.");
    Equal(2, panels.Select(tag => AttributeValue(tag, "id")).Distinct().Count(), "Sibling branches reuse a panel ID.");
    Equal(2, TagsWithClass(html, "f-expansion-indicator").Count, "A leaf gained a triangle or a branch lost its shared triangle.");
    Check(TagsWithClass(html, "f-expansion-indicator").All(tag => AttributeValue(tag, "data-expanded") == "false"), "Collapsed branches lack their shared marker state.");
    var leaf = TagsWithClass(html, "f-secondary-item").Single(tag => AttributeValue(tag, "href") == "/controls");
    Equal("Overview", AttributeValue(leaf, "title"), "A truncated leaf no longer exposes its full label.");
    Equal<string?>(null, AttributeValue(leaf, "aria-expanded"), "A leaf exposes a nonexistent expansion state.");
});
AsyncTest("third-route specificity selects its owning module even outside its parent's path", async () => {
    var html = await RenderFramework(framework => framework.ConfigureNavigation(nav => nav
        .AddNav("Controls", "widgets", "/controls", secondary => secondary
            .AddSubNav("Inputs", "input", "/controls/inputs", third => third.AddSubNav("Select", "list", "/examples/select")))
        .AddNav("Examples", "description", "/examples")), "/examples/select/usage");
    var selectedPrimary = TagsWithClass(html, "f-primary-item").Single(tag => AttributeValue(tag, "aria-current") == "true");
    Equal("Controls", AttributeValue(selectedPrimary, "aria-label"));
    var selectedPage = OpeningTags(html, "a").Single(tag => AttributeValue(tag, "aria-current") == "page");
    Equal("/examples/select", AttributeValue(selectedPage, "href"));
    Equal("true", AttributeValue(TagsWithClass(html, "f-secondary-toggle").Single(), "aria-expanded"));
});
AsyncTest("disabled third leaves cannot displace an enabled prefix route", async () => {
    var html = await RenderFramework(framework => framework.ConfigureNavigation(nav => nav
        .AddNav("Controls", "widgets", "/controls", secondary => secondary
            .AddSubNav("Inputs", "input", "/controls/inputs", third => third
                .AddSubNav("Select", "list", "/controls/inputs/select")
                .AddSubNav("Unavailable detail", "description", "/controls/inputs/select/details", exact: true, disabled: true)))), "/controls/inputs/select/details");
    var selected = OpeningTags(html, "a").Single(tag => AttributeValue(tag, "aria-current") == "page");
    Equal("/controls/inputs/select", AttributeValue(selected, "href"));
    var disabled = TagsWithClass(html, "f-third-item").Single(tag => AttributeValue(tag, "aria-disabled") == "true");
    Equal<string?>(null, AttributeValue(disabled, "href"));
    Equal("-1", AttributeValue(disabled, "tabindex"));
    Equal<string?>(null, AttributeValue(disabled, "aria-current"));
});
AsyncTest("a shorter child prefix cannot displace its more specific parent destination", async () => {
    var html = await RenderFramework(framework => framework.ConfigureNavigation(nav => nav
        .AddNav("Settings", "settings", "/preferences", secondary => secondary
            .AddSubNav("Advanced", "settings", "/settings/advanced", third => third
                .AddSubNav("All settings", "list", "/settings")))), "/settings/advanced/details");
    var selected = OpeningTags(html, "a").Single(tag => AttributeValue(tag, "aria-current") == "page");
    Equal("/settings/advanced", AttributeValue(selected, "href"));
    Check(AttributeValue(TagsWithClass(html, "f-secondary-toggle").Single(), "class")!.Split(' ').Contains("is-selected"), "A current parent page did not select its adjacent triangle.");
    Check(!TagsWithClass(html, "f-third-item").Any(tag => AttributeValue(tag, "aria-current") == "page"), "A shorter child route overrode the more specific parent.");
});
AsyncTest("disabled parent branches disable their descendants and exclude them from route selection", async () => {
    var html = await Render<ApplicationShell>(configure: framework => framework.ConfigureNavigation(nav => nav
        .AddNav("Controls", "widgets", "/controls", secondary => secondary
            .AddSubNav("Overview", "home", "/controls")
            .AddSubNav("Unavailable category", "block", "/controls/blocked", third => third
                .AddSubNav("Unavailable leaf", "description", "/controls/blocked/leaf"), disabled: true))), path: "/controls/blocked/leaf");
    var selected = OpeningTags(html, "a").Single(tag => AttributeValue(tag, "aria-current") == "page");
    Equal("/controls", AttributeValue(selected, "href"));
    var child = TagsWithClass(html, "f-third-item").Single();
    Equal("true", AttributeValue(child, "aria-disabled"));
    Equal<string?>(null, AttributeValue(child, "href"));
    Equal("-1", AttributeValue(child, "tabindex"));
    var toggle = TagsWithClass(html, "f-secondary-toggle").Single();
    Check(Regex.IsMatch(toggle, "\\sdisabled(?:[\\s=>])"), "A disabled category can still be expanded.");
    Equal("true", AttributeValue(toggle, "aria-disabled"));
    Check(!AttributeValue(toggle, "class")!.Split(' ').Contains("is-selected"), "A disabled triangle gained selected styling.");
    var parent = TagsWithClass(html, "f-secondary-item").Single(tag => AttributeValue(tag, "title") == "Unavailable category");
    Equal<string?>(null, AttributeValue(parent, "href"), "A disabled branch retained its navigation target.");
    Equal("-1", AttributeValue(parent, "tabindex"));
    Equal("false", AttributeValue(toggle, "aria-expanded"));
});
AsyncTest("branch collapse survives same-path rerenders while navigation reopens the current branch per shell instance", async () => {
    var navigation = new TestNavigation("/controls/inputs/select");
    var activator = new ShellActivator();
    var services = new ServiceCollection();
    services.AddLogging(); services.AddSingleton<NavigationManager>(navigation); services.AddSingleton<IJSRuntime>(new FakeJs());
    services.AddSingleton<IComponentActivator>(activator);
    services.AddFlourishFramework(framework => framework.ConfigureNavigation(nav => nav
        .AddNav("Controls", "widgets", "/controls", secondary => secondary
            .AddSubNav("Inputs", "input", "/controls/inputs", third => third
                .AddSubNav("Select", "list", "/controls/inputs/select", exact: true)
                .AddSubNav("Text", "text_fields", "/controls/inputs/text", exact: true)))));
    using var provider = services.BuildServiceProvider(); using var firstScope = provider.CreateScope();
    await using var renderer = new HtmlRenderer(firstScope.ServiceProvider, provider.GetRequiredService<ILoggerFactory>());
    await renderer.Dispatcher.InvokeAsync(async () => {
        var parameters = new Dictionary<string, object?>();
        var output = await renderer.RenderComponentAsync<ApplicationShell>(ParameterView.FromDictionary(parameters));
        var shell = activator.Instances.Single();
        var configuredPrimaryNavigation = (IReadOnlyList<NavigationEntry>)typeof(ApplicationShell)
            .GetProperty("PrimaryEntries", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!.GetValue(shell)!;
        var branch = configuredPrimaryNavigation.Single().SecondaryItems.Single();
        var toggleMethod = typeof(ApplicationShell).GetMethod("ToggleItem", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
            ?? throw new InvalidOperationException("Navigation branch handler was not found.");
        var toggle = EventCallback.Factory.Create(shell, (Action)(() => toggleMethod.Invoke(shell, [branch])));
        var openNavigationMethod = typeof(ApplicationShell).GetMethod("ToggleNavigation", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
            ?? throw new InvalidOperationException("Mobile navigation handler was not found.");
        var openNavigation = EventCallback.Factory.Create(shell, (Action)(() => openNavigationMethod.Invoke(shell, null)));
        void VerifyExpansion(string html, bool expanded) {
            var button = TagsWithClass(html, "f-secondary-toggle").Single();
            Equal(expanded ? "true" : "false", AttributeValue(button, "aria-expanded"));
            var link = TagsWithClass(html, "f-secondary-item").Single(tag => AttributeValue(tag, "href") == branch.Href);
            Check(AttributeValue(link, "class")!.Split(' ').Contains("is-selected") && AttributeValue(button, "class")!.Split(' ').Contains("is-selected"), "Collapsing the current branch separated its link and triangle selected styles.");
            var panel = TagsWithClass(html, "f-third-navigation").Single();
            Equal(!expanded, Regex.IsMatch(panel, "\\shidden(?:[\\s=>])"), "Branch visibility diverged from its disclosure state.");
            Equal(AttributeValue(button, "aria-controls"), AttributeValue(panel, "id"));
        }
        VerifyExpansion(output.ToHtmlString(), true);
        var panelId = AttributeValue(TagsWithClass(output.ToHtmlString(), "f-third-navigation").Single(), "id");
        await openNavigation.InvokeAsync();
        var originalUri = navigation.Uri;
        void VerifyRailAndRoute() {
            Equal(originalUri, navigation.Uri, "A branch toggle navigated to its configured category route.");
            Equal("true", AttributeValue(TagsWithClass(output.ToHtmlString(), "f-root").Single(), "data-navigation-open"), "A branch toggle dismissed the open mobile rail.");
        }
        await toggle.InvokeAsync();
        VerifyExpansion(output.ToHtmlString(), false);
        VerifyRailAndRoute();
        await toggle.InvokeAsync();
        VerifyExpansion(output.ToHtmlString(), true);
        VerifyRailAndRoute();
        await toggle.InvokeAsync();
        VerifyExpansion(output.ToHtmlString(), false);
        VerifyRailAndRoute();
        parameters[nameof(ApplicationShell.ApplicationTitle)] = "Updated application";
        await shell.SetParametersAsync(ParameterView.FromDictionary(parameters));
        Equal(1, activator.Instances.Count, "A routine parameter change replaced the shell under test.");
        VerifyExpansion(output.ToHtmlString(), false);
        Equal(panelId, AttributeValue(TagsWithClass(output.ToHtmlString(), "f-third-navigation").Single(), "id"), "Rerendering changed the disclosure's panel identity.");
        navigation.NavigateTo("/controls/inputs/text");
        VerifyExpansion(output.ToHtmlString(), true);
        Equal("/controls/inputs/text", AttributeValue(TagsWithClass(output.ToHtmlString(), "f-third-item").Single(tag => AttributeValue(tag, "aria-current") == "page"), "href"));
        await toggle.InvokeAsync();
        VerifyExpansion(output.ToHtmlString(), false);
        navigation.NavigateTo("/controls");
        await toggle.InvokeAsync();
        var unrelatedHtml = output.ToHtmlString();
        var unrelatedToggle = TagsWithClass(unrelatedHtml, "f-secondary-toggle").Single();
        var unrelatedLink = TagsWithClass(unrelatedHtml, "f-secondary-item").Single(tag => AttributeValue(tag, "href") == branch.Href);
        Equal("true", AttributeValue(unrelatedToggle, "aria-expanded"));
        Check(!AttributeValue(unrelatedLink, "class")!.Split(' ').Contains("is-selected") && !AttributeValue(unrelatedToggle, "class")!.Split(' ').Contains("is-selected"), "Expanding an unrelated branch selected its link or triangle.");
        navigation.NavigateTo("/controls/inputs/text");
        VerifyExpansion(output.ToHtmlString(), true);
        await toggle.InvokeAsync();
        VerifyExpansion(output.ToHtmlString(), false);
        using var secondScope = provider.CreateScope();
        await using var secondRenderer = new HtmlRenderer(secondScope.ServiceProvider, provider.GetRequiredService<ILoggerFactory>());
        await secondRenderer.Dispatcher.InvokeAsync(async () => {
            var secondOutput = await secondRenderer.RenderComponentAsync<ApplicationShell>(ParameterView.FromDictionary(parameters));
            Equal(2, activator.Instances.Count);
            Check(!ReferenceEquals(shell, activator.Instances[1]), "Another user scope reused the original shell instance.");
            VerifyExpansion(secondOutput.ToHtmlString(), true);
        });
        VerifyExpansion(output.ToHtmlString(), false);
    });
});
AsyncTest("rail-only navigation omits secondary chrome while keeping the module", async () => {
    var html = await Render<ApplicationShell>(configure: framework => framework.ConfigureNavigation(nav => nav.AddNav("Overview", "home", "/", exact: true)));
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

AsyncTest("application layout owns document scroll while embedded shells opt out", async () => {
    RenderFragment body = builder => builder.AddContent(0, "Embedded content");
    var application = await Render<ApplicationLayout>(new() { ["Body"] = body });
    var shell = TagsWithClass(application, "f-shell").Single();
    Check(shell.Contains("data-f-document-shell"), "The default application layout did not claim the viewport.");
    var embedded = await Render<ApplicationLayout>(new() { ["Body"] = body, ["OwnsDocument"] = false });
    Check(!TagsWithClass(embedded, "f-shell").Single().Contains("data-f-document-shell"), "An embedded layout locked the host document.");
    var direct = await Render<ApplicationShell>(new() { ["ChildContent"] = body });
    Check(!TagsWithClass(direct, "f-shell").Single().Contains("data-f-document-shell"), "The low-level shell unexpectedly claimed its host viewport.");
});
GridChecks.Register(tests);
DropdownChecks.Register(tests);
LifecycleChecks.Register(tests);
DataTableChecks.Register(tests);
MultiSelectBoxChecks.Register(tests);
ListViewChecks.Register(tests);
SplitButtonChecks.Register(tests);
ButtonUnavailableChecks.Register(tests);
DialogResultChecks.Register(tests);
ValidationMessagesChecks.Register(tests);
NavigationChoicesChecks.Register(tests);
DialogViewChecks.Register(tests);
NavigationGuardChecks.Register(tests);
NoCompatibilityApiChecks.Register(tests);
DisplayBoardChecks.Register(tests);
UniformGridChecks.Register(tests);
SectionNavigatorChecks.Register(tests);
NavigationControlsChecks.Register(tests);
DataSearchChecks.Register(tests);
BrandingChecks.Register(tests);
TextChecks.Register(tests);
InputMigrationChecks.Register(tests);
ControlTextChecks.Register(tests);
PresentationChecks.Register(tests);
ApiFinalAuditChecks.Register(tests);
InteropFinalAuditChecks.Register(tests);
AccessExampleChecks.Register(tests);
ArkheideSystem.Tests.Flourish.Blazor.ComponentInventoryChecks.Register(tests);
Test("catalog covers component parameters and constructor defaults", ArkheideSystem.Tests.Flourish.Blazor.CatalogChecks.Run);

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
internal sealed class ShellActivator : IComponentActivator {
    internal List<ApplicationShell> Instances { get; } = [];
    public IComponent CreateInstance(Type componentType) {
        var component = (IComponent)Activator.CreateInstance(componentType)!;
        if (component is ApplicationShell shell) Instances.Add(shell);
        return component;
    }
}

internal sealed class FormModel { public string? Name { get; set; } public decimal? Amount { get; set; } }
internal sealed class CommandState { public int Count { get; set; } }
internal sealed class TestCommandParser(CommandState state) : ICommandParser
{
    internal const string CommandKey = "test.run";
    public void RegisterCommands(ICommandRegistrar commands) => commands.Register(CommandKey, () => state.Count++);
}
internal sealed class TestInjectedComponent : ComponentBase
{
    [Inject] public CommandState State { get; set; } = null!;
    protected override void BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder builder) =>
        builder.AddContent(0, $"Scoped injection {State.Count}");
}
