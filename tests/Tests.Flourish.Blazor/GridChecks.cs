using System.Net;
using System.Text.RegularExpressions;
using ArkheideSystem.Flourish.Blazor;
using ArkheideSystem.Flourish.Blazor.Abstract;
using ArkheideSystem.Flourish.Blazor.Components;
using ArkheideSystem.Flourish.Blazor.Components.Patterns;
using ArkheideSystem.Flourish.Blazor.Components.Primitives;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;

internal static class GridChecks
{
    internal static void Register(List<(string Name, Func<Task> Run)> tests)
    {
        tests.Add(("centered page variants retain direct and form sections without a compact spacing contract", async () =>
        {
            RenderFragment content = builder =>
            {
                builder.OpenComponent<Section>(0);
                builder.AddAttribute(1, nameof(Section.Title), "Direct section");
                builder.CloseComponent();
                builder.OpenComponent<FormLayout>(2);
                builder.AddAttribute(3, nameof(FormLayout.ChildContent), (RenderFragment)(form =>
                {
                    form.OpenComponent<Section>(0);
                    form.AddAttribute(1, nameof(Section.Title), "Form section");
                    form.CloseComponent();
                }));
                builder.CloseComponent();
            };
            foreach (var container in Enum.GetValues<CenteredContainer>())
            {
                var html = await Render<PageBody>(new()
                {
                    [nameof(PageBody.Fluid)] = false, [nameof(PageBody.CenteredContainer)] = container,
                    [nameof(PageBody.ChildContent)] = content
                });
                Require(html.Contains("f-page-centered-expanded", StringComparison.Ordinal) == (container == CenteredContainer.Expanded), "The production page did not select its centered variant.");
                Require(Regex.Matches(html, "<section").Count == 2 && html.Contains("Direct section") && html.Contains("Form section"), "Changing horizontal geometry lost a direct or form section.");
                Require(!html.Contains("f-page-compact-spacing", StringComparison.Ordinal), "The retired compact spacing mode still changes section composition.");
            }
        }));
        tests.Add(("full-height spreadsheet composes an immediately compact heading and one native grid in either surface", async () =>
        {
            RenderFragment body = builder =>
            {
                builder.OpenComponent<PageBody>(0);
                builder.AddAttribute(1, nameof(PageBody.FullWidth), true);
                builder.AddAttribute(2, nameof(PageBody.FillHeight), true);
                builder.AddAttribute(3, nameof(PageBody.ChildContent), (RenderFragment)(content =>
                {
                    content.OpenComponent<PageHeading>(0);
                    content.AddAttribute(1, nameof(PageHeading.Title), "Editable records");
                    content.AddAttribute(2, nameof(PageHeading.Compact), true);
                    content.CloseComponent();
                    content.OpenComponent<EditingGrid>(3);
                    content.AddAttribute(4, nameof(EditingGrid.Columns), new[] { new GridColumn("name", "Name") });
                    content.AddAttribute(5, nameof(EditingGrid.Rows), new[] { new GridRow("one", [new("Alpha", "Alpha")]) });
                    content.CloseComponent();
                }));
                builder.CloseComponent();
            };
            foreach (var html in new[] {
                await Render<NavigationSurface>(new() { [nameof(NavigationSurface.ChildContent)] = body }),
                await Render<ContentSurface>(new() { [nameof(ContentSurface.ChildContent)] = body }) })
            {
                Require(html.Contains("f-page-full f-page-fill-height", StringComparison.Ordinal), "The full-stage layout was not selected through PageBody.");
                Require(Attribute(Tag(html, "header"), "data-heading-mode") == "compact", "The first render needs scrolling before its heading becomes compact.");
                Require(Regex.IsMatch(html, "</header>\\s*<div[^>]*class=\"spreadsheet-scroll\""), "The remaining space has an extra wrapper, action area or explanatory content.");
                Require(Regex.Matches(html, "role=\"grid\"").Count == 1 && html.Contains("Alpha", StringComparison.Ordinal), "The direct native spreadsheet was lost or duplicated.");
                Require(!html.Contains("<button", StringComparison.Ordinal) && !html.Contains("f-heading-actions", StringComparison.Ordinal), "The full-stage example includes unrelated action controls.");
            }
            var defaultBody = await Render<PageBody>(new());
            Require(!defaultBody.Contains("f-page-fill-height", StringComparison.Ordinal), "Ordinary pages unexpectedly opted into fixed-height scrolling.");
        }));
        tests.Add(("spreadsheet layout retains bounded scrolling and roomy automatic columns without host skins", async () =>
        {
            var html = await Render<EditingGrid>(Parameters([new("name", "Name", Identity: true), new("amount", "Amount")],
                [new("one", [new("Alpha", "Alpha"), new("12", "12")], Dirty: true)]));
            Require(Regex.IsMatch(html, "data-column-key=\"name\"[^>]*style=\"width:280px\"")
                && Regex.IsMatch(html, "data-column-key=\"amount\"[^>]*style=\"width:220px\""), "The spreadsheet starts with compressed record columns.");
            Require(html.Contains("data-column-minimum-width=\"100\"", StringComparison.Ordinal), "Roomier automatic widths removed user-controlled shrinking.");
            var root = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../.."));
            var framework = await File.ReadAllTextAsync(Path.Combine(root, "src/Flourish.Blazor/Flourish.Blazor.Framework/wwwroot/primitives/EditingGrid.css"));
            var design = await File.ReadAllTextAsync(Path.Combine(root, "src/Flourish.Blazor/Flourish.Blazor.Design/wwwroot/primitives/EditingGrid.css"));
            Require(framework.Contains("max-height: 65vh;", StringComparison.Ordinal) && framework.Contains("height: 74px;", StringComparison.Ordinal)
                && framework.Contains("overscroll-behavior: contain;", StringComparison.Ordinal), "Spreadsheet scrolling or readable headers lost the historical layout.");
            var viewport = Regex.Match(design, @"\.spreadsheet-scroll\s*\{([^}]*)\}").Groups[1].Value;
            Require(viewport.Contains("border-radius: 0;", StringComparison.Ordinal)
                && viewport.Contains("border: 1px solid var(--f-border);", StringComparison.Ordinal)
                && design.Contains(".editing-grid-table tr.is-dirty .identity-column", StringComparison.Ordinal), "The spreadsheet lost its square framed viewport or draft-row cue.");
        }));
        tests.Add(("grid template uses the existing cell shell and preserves complete values and errors", async () =>
        {
            IReadOnlyList<GridColumn> columns = [new("tags", "Tags")];
            GridRow row = new("one", [new("alpha;beta", "2 selected", GridEditorKind.Template, Disabled: true, Error: "Choose available tags")]);
            GridCellContext? observed = null;
            var parameters = Parameters(columns, [row]);
            parameters[nameof(EditingGrid.CellTemplate)] = (RenderFragment<GridCellContext>)(context => builder =>
            {
                observed = context;
                builder.OpenComponent<ArkheideSystem.Flourish.Blazor.Components.Button>(0);
                builder.AddAttribute(1, "Disabled", context.Disabled);
                builder.AddAttribute(2, "aria-describedby", context.DescribedBy);
                builder.AddAttribute(3, "Text", context.Cell.Display);
                builder.CloseComponent();
            });
            var html = await Render<EditingGrid>(parameters);
            var cell = Cells(html).Single();
            Require(observed is { RowIndex: 0, ColumnIndex: 0, Disabled: true } && observed.Row.Key.Equals("one") && observed.Column.Key == "tags", "Template coordinates or availability were lost.");
            Require(Attribute(Tag(cell, "td"), "data-cell-value") == "alpha;beta", "Copying a composite cell lost the full value.");
            Require(Attribute(Tag(cell, "td"), "aria-invalid") == "true" && Attribute(Tag(cell, "button"), "aria-describedby") == observed!.DescribedBy, "Template validation metadata is detached from its actual error.");
            Require(Regex.IsMatch(Tag(cell, "fieldset"), @"\bdisabled(?:\s|=|>)") && Regex.IsMatch(Tag(cell, "button"), @"\bdisabled(?:\s|=|>)"), "The template does not honor its availability boundary.");
            Require(cell.Contains("f-button", StringComparison.Ordinal) && !cell.Contains("<input", StringComparison.Ordinal), "A parallel default editor was created behind the template.");
        }));
        tests.Add(("grid ends at its final declared column instead of creating a viewport filler column", async () =>
        {
            IReadOnlyList<GridColumn> columns = [new("name", "Name", Identity: true), new("image", "Image")];
            var html = await Render<EditingGrid>(Parameters(columns,
                [new("one", [new("Alpha", "Alpha"), new(null, "View image", GridEditorKind.Link, ReadOnly: true, LinkHref: "/images/alpha")])]));
            Require(Regex.Matches(html, @"<col\b").Count == columns.Count
                && Regex.Matches(html, @"<th\b").Count == columns.Count
                && Regex.Matches(html, @"<td\b").Count == columns.Count,
                "A blank structural column remains after the final declared field.");
            Require(!html.Contains("data-column-fill", StringComparison.Ordinal)
                && !html.Contains("spreadsheet-fill", StringComparison.Ordinal), "The retired filler contract is still rendered.");
            Require(Attribute(Tag(html, "table"), "style") == "width:500px"
                && Attribute(Tag(html, "table"), "aria-colcount") == "2", "The table width or accessibility dimensions include undeclared space.");
            Require(Attribute(Tag(Cells(html).Last(), "a"), "href") == "/images/alpha", "Removing the tail broke the final link editor.");
            var empty = await Render<EditingGrid>(Parameters(columns, []));
            Require(Attribute(Tag(empty, "td"), "colspan") == "2", "The empty state spans an undeclared filler column.");
            var root = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../.."));
            var framework = await File.ReadAllTextAsync(Path.Combine(root, "src/Flourish.Blazor/Flourish.Blazor.Framework/wwwroot/primitives/EditingGrid.css"));
            var table = Regex.Match(framework, @"\.editing-grid-table\s*\{([^}]*)\}").Groups[1].Value;
            Require(table.Contains("min-width: 0;", StringComparison.Ordinal)
                && !table.Contains("min-width: 100%;", StringComparison.Ordinal), "Viewport surplus is still forced into declared column widths.");
        }));
        tests.Add(("grid template kind fails closed when its native editor slot is missing", async () =>
        {
            await RejectAsync(Parameters([new("tags", "Tags")], [new("one", [new("alpha", "Alpha", GridEditorKind.Template)])]), nameof(EditingGrid.CellTemplate));
        }));
        tests.Add(("grid rejects ambiguous migrated column keys before rendering or changing host data", async () =>
        {
            var changes = 0;
            var parameters = Parameters([new("name", "Name"), new("name", "Alias")],
                [new("one", [new("Alice", "Alice"), new("Other", "Other")])]);
            parameters[nameof(EditingGrid.CellChanged)] = EventCallback.Factory.Create<GridCellChange>(new object(), (GridCellChange _) => changes++);
            await RejectAsync(parameters, nameof(EditingGrid.Columns));
            Require(changes == 0, "Invalid column definitions changed host data.");
        }));
        tests.Add(("grid rejects both truncated and oversized migrated rows instead of misaligning edits", async () =>
        {
            IReadOnlyList<GridColumn> columns = [new("name", "Name"), new("amount", "Amount")];
            await RejectAsync(Parameters(columns, [new("short", [new("Alice", "Alice")])]), nameof(EditingGrid.Rows));
            await RejectAsync(Parameters(columns, [new("long", [new("Alice", "Alice"), new("2", "2"), new("extra", "Extra")])]), nameof(EditingGrid.Rows));
        }));
        tests.Add(("mixed grid migration preserves editor kinds, formatted display, and encoded host text", async () =>
        {
            var html = await Render<EditingGrid>(Parameters(MixedColumns, [MixedRow]));
            var cells = Cells(html);
            Require(cells.Count == MixedColumns.Count, "The migration lost or duplicated a data cell.");
            Require(Attribute(Tag(cells[0], "input"), "value") == "<Unsafe & name>", "Text editing lost the host value.");
            Require(Attribute(Tag(cells[0], "input"), "aria-required") == "true", "Required cell semantics were lost.");
            Require(Attribute(Tag(cells[1], "input"), "inputmode") == "decimal" && Attribute(Tag(cells[1], "input"), "value") == "12.50", "Decimal editing localized the underlying value.");
            Require(cells[1].Contains("R$ 12,50", StringComparison.Ordinal), "Formatted display was replaced by its raw edit value.");
            Require(Attribute(Tag(cells[2], "input"), "type") == "date" && Attribute(Tag(cells[2], "input"), "value") == "2026-10-03", "A migrated date stopped using the native date editor.");
            Require(Tag(cells[3], "textarea").Length > 0 && cells[3].Contains("&lt;unsafe memo&gt;", StringComparison.Ordinal), "Multiline content was lost or became markup.");
            Require(Attribute(Tag(cells[4], "select"), "value") == "ready" && cells[4].Contains("Ready &amp; confirmed", StringComparison.Ordinal), "Select choices or the host selection were lost.");
            Require(Attribute(Tag(cells[5], "input"), "data-input-mask") == "000.000.000-00" && Attribute(Tag(cells[5], "input"), "value") == "123.456.789-00", "The migrated masked editor no longer formats its native value.");
            Require(Attribute(Tag(cells[6], "a"), "href") == "/items/alpha?x=1&y=2", "The host link destination was rewritten.");
            Require(html.Contains("&lt;Unsafe &amp; name&gt;", StringComparison.Ordinal) && !html.Contains("<unsafe memo>", StringComparison.Ordinal), "Consumer content escaped HTML encoding.");
            Require(Attribute(Tag(html, "table"), "aria-colcount") == "7" && Attribute(Tag(html, "table"), "aria-rowcount") == "2", "Grid dimensions do not describe actual records.");
        }));
        tests.Add(("grid errors identify invalid native editors and resolve both descriptions", async () =>
        {
            IReadOnlyList<GridColumn> columns = [new("document", "Document"), new("memo", "Memo")];
            GridRow row = new("invalid", [
                new("123", "123", GridEditorKind.Masked, Mask: "000.000", Error: "Invalid <document>", SecondaryError: "Row needs review"),
                new("Notes", "Notes", GridEditorKind.Multiline, Error: "Memo is too long")]);
            var html = await Render<EditingGrid>(Parameters(columns, [row]));
            var cells = Cells(html);
            foreach (var cell in cells)
            {
                Require(Attribute(Tag(cell, "td"), "aria-invalid") == "true", "An invalid grid cell is not announced as invalid.");
                var editor = Tag(cell, "input", "textarea");
                Require(Attribute(editor, "aria-invalid") == "true", "A native editor does not expose the host error.");
                var descriptions = Attribute(editor, "aria-describedby").Split(' ', StringSplitOptions.RemoveEmptyEntries);
                Require(descriptions.Length > 0 && descriptions.All(id => html.Contains($"id=\"{id}\"", StringComparison.Ordinal)), "An editor points at a missing error description.");
            }
            Require(Attribute(Tag(cells[0], "input"), "aria-describedby").Split(' ', StringSplitOptions.RemoveEmptyEntries).Length == 2, "Primary and row errors were collapsed into one relationship.");
            Require(html.Contains("Invalid &lt;document&gt;", StringComparison.Ordinal) && !html.Contains("Invalid <document>", StringComparison.Ordinal), "Error text became HTML.");
        }));
        tests.Add(("grid-wide edit lock disables every editable native control while preserving displayed records", async () =>
        {
            var parameters = Parameters(MixedColumns, [MixedRow]);
            parameters[nameof(EditingGrid.EditDisabled)] = true;
            var html = await Render<EditingGrid>(parameters);
            Require(Attribute(Tag(html, "div"), "data-edit-disabled") == "true", "The interaction layer lost the edit lock.");
            var cells = Cells(html);
            foreach (var cell in cells)
            {
                Require(Attribute(Tag(cell, "td"), "aria-readonly") == "true", "A locked cell still announces writable state.");
                var editor = TryTag(cell, "input", "textarea", "select");
                if (editor is not null) Require(Regex.IsMatch(editor, @"\bdisabled(?:\s|=|>)"), "An editable native control remains enabled under the grid-wide lock.");
            }
            Require(html.Contains("R$ 12,50", StringComparison.Ordinal) && cells.Count == MixedColumns.Count, "Locking removed or replaced existing record data.");
        }));
        tests.Add(("individual readonly and disabled cells retain honest grid accessibility state", async () =>
        {
            IReadOnlyList<GridColumn> columns = [new("locked", "Locked"), new("disabled", "Disabled"), new("editable", "Editable")];
            GridRow row = new("policies", [new("A", "A", ReadOnly: true), new("B", "B", Disabled: true), new("C", "C")]);
            var cells = Cells(await Render<EditingGrid>(Parameters(columns, [row])));
            Require(Attribute(Tag(cells[0], "td"), "data-readonly") == "true" && Attribute(Tag(cells[0], "td"), "aria-readonly") == "true", "The read-only edit policy contradicts the announced grid state.");
            Require(Attribute(Tag(cells[1], "td"), "aria-readonly") == "true", "A disabled cell announces writable state.");
            Require(Attribute(Tag(cells[2], "td"), "aria-readonly") == "false", "A normal editable cell became read-only.");
        }));
        tests.Add(("empty grid describes its actual dimensions and remains safely retainable while hidden", async () =>
        {
            var parameters = Parameters([new("name", "Name"), new("amount", "Amount")], []);
            parameters[nameof(EditingGrid.EmptyMessage)] = "No migrated <records>";
            parameters[nameof(EditingGrid.Hidden)] = true;
            var html = await Render<EditingGrid>(parameters);
            Require(Regex.IsMatch(Tag(html, "div"), @"\bhidden(?:\s|=|>)"), "A hidden grid is still exposed as active UI.");
            Require(Attribute(Tag(html, "table"), "aria-rowcount") == "1" && Cells(html).Count == 0, "The empty grid invented a record.");
            Require(html.Contains("No migrated &lt;records&gt;", StringComparison.Ordinal), "The consumer empty-state label was lost or not encoded.");
        }));
        tests.Add(("Framework-only DI renders navigation and content patterns with native primitives", async () =>
        {
            using var provider = Services().BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true, ValidateOnBuild = true });
            using var scope = provider.CreateScope();
            Require(scope.ServiceProvider.GetService<IAppearanceService>() is null, "The native test accidentally registered a Design service.");
            Require(scope.ServiceProvider.GetRequiredService<ITablePreferences>() is TablePreferences, "Native table preferences require an external host service.");
            Require(!typeof(EditingGrid).Assembly.GetReferencedAssemblies().Any(assembly => assembly.Name == "Flourish.Blazor.Design"), "Framework depends on the optional Design assembly.");
            await using var renderer = new HtmlRenderer(scope.ServiceProvider, provider.GetRequiredService<ILoggerFactory>());
            RenderFragment body = builder =>
            {
                builder.OpenComponent<DataTable<NativeRecord>>(0);
                builder.AddAttribute(1, "Label", "Native records");
                builder.AddAttribute(2, "Items", new[] { new NativeRecord("Alpha <native>") });
                builder.AddAttribute(3, "Columns", new[] { new TableColumn<NativeRecord>("name", "Name", item => item.Name) });
                builder.AddAttribute(4, "PreferenceKey", "native-records");
                builder.AddAttribute(5, "Searchable", true);
                builder.CloseComponent();
                builder.OpenComponent<Card>(6);
                builder.AddAttribute(7, "ChildContent", (RenderFragment)(content => content.AddContent(0, "Alpha <native>")));
                builder.CloseComponent();
                builder.OpenComponent<ToggleSwitch>(8);
                builder.AddAttribute(9, "Label", "Native toggle");
                builder.AddAttribute(10, "Value", true);
                builder.CloseComponent();
                builder.OpenComponent<EditingGrid>(11);
                builder.AddAttribute(12, "Columns", new[] { new GridColumn("name", "Name") });
                builder.AddAttribute(13, "Rows", new[] { new GridRow("one", new[] { new GridCell("Alpha", "Alpha") }) });
                builder.CloseComponent();
            };
            var navigation = await RenderWith<NavigationSurface>(renderer, new() {
                ["ContentId"] = "native-navigation-content", ["ChildContent"] = body,
                ["PrimaryNavigation"] = (RenderFragment)(builder => builder.AddContent(0, "Allowed module"))
            });
            var content = await RenderWith<ContentSurface>(renderer, new() { ["ContentId"] = "native-account-content", ["ChildContent"] = body });
            Require(navigation.Contains("application-stage", StringComparison.Ordinal) && navigation.Contains("Allowed module", StringComparison.Ordinal), "The navigation surface did not compose the host slots.");
            Require(content.Contains("content-stage", StringComparison.Ordinal) && content.Contains("native-account-content", StringComparison.Ordinal), "The content surface did not expose its content target.");
            foreach (var html in new[] { navigation, content })
            {
                Require(html.Contains("Alpha &lt;native&gt;", StringComparison.Ordinal) && html.Contains("<article class=\"f-card\">", StringComparison.Ordinal) && html.Contains("role=\"switch\"", StringComparison.Ordinal) && html.Contains("role=\"grid\"", StringComparison.Ordinal), "A Framework-only production control failed to render within a pattern.");
                Require(!html.Contains("f-root", StringComparison.Ordinal) && !html.Contains("--f-primary", StringComparison.Ordinal), "A Framework pattern introduced skin or theme markup.");
            }
        }));
    }

    private static readonly IReadOnlyList<GridColumn> MixedColumns = [
        new("name", "Name", Required: true, Identity: true), new("amount", "Amount"), new("date", "Date"),
        new("memo", "Memo"), new("status", "Status"), new("document", "Document"), new("link", "Link")];
    private static readonly GridRow MixedRow = new("mixed", [
        new("<Unsafe & name>", "<Unsafe & name>", Required: true, MaximumLength: 24),
        new("12.50", "R$ 12,50", GridEditorKind.Decimal), new("2026-10-03", "03/10/2026", GridEditorKind.Date),
        new("First line\n<unsafe memo>", "First line", GridEditorKind.Multiline, MaximumLength: 240),
        new("ready", "Ready & confirmed", GridEditorKind.Select, Options: [new("ready", "Ready & confirmed"), new("pending", "Pending")], AllowEmpty: false),
        new("12345678900", "123.456.789-00", GridEditorKind.Masked, Mask: "000.000.000-00"),
        new(null, "Open <item>", GridEditorKind.Link, ReadOnly: true, LinkHref: "/items/alpha?x=1&y=2", LinkLabel: "Open Alpha")], Dirty: true, Label: "Migrated record");

    private static Dictionary<string, object?> Parameters(IReadOnlyList<GridColumn> columns, IReadOnlyList<GridRow> rows) =>
        new() { [nameof(EditingGrid.Columns)] = columns, [nameof(EditingGrid.Rows)] = rows };
    private static ServiceCollection Services()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton<NavigationManager>(new TestNavigation());
        services.AddSingleton<IJSRuntime>(new FakeJs());
        services.AddFlourishFramework();
        return services;
    }
    private static async Task<string> Render<T>(Dictionary<string, object?> parameters) where T : IComponent
    {
        using var provider = Services().BuildServiceProvider();
        using var scope = provider.CreateScope();
        await using var renderer = new HtmlRenderer(scope.ServiceProvider, provider.GetRequiredService<ILoggerFactory>());
        return await RenderWith<T>(renderer, parameters);
    }
    private static Task<string> RenderWith<T>(HtmlRenderer renderer, Dictionary<string, object?> parameters) where T : IComponent =>
        renderer.Dispatcher.InvokeAsync(async () => (await renderer.RenderComponentAsync<T>(ParameterView.FromDictionary(parameters))).ToHtmlString());
    private static async Task RejectAsync(Dictionary<string, object?> parameters, string parameter)
    {
        try { await Render<EditingGrid>(parameters); }
        catch (ArgumentException error) when (error.ParamName == parameter) { return; }
        throw new InvalidOperationException($"Invalid migrated grid data was not rejected through {parameter}.");
    }
    private static List<string> Cells(string html) => Regex.Matches(html, @"<td\b[^>]*>[\s\S]*?</td>")
        .Select(match => match.Value).Where(cell => Attribute(Tag(cell, "td"), "role") == "gridcell").ToList();
    private static string Tag(string html, params string[] names) => TryTag(html, names) ?? throw new InvalidOperationException($"Missing {string.Join('/', names)} element.");
    private static string? TryTag(string html, params string[] names)
    {
        var match = Regex.Match(html, $@"<(?:{string.Join('|', names)})\b[^>]*>");
        return match.Success ? match.Value : null;
    }
    private static string Attribute(string tag, string name)
    {
        var match = Regex.Match(tag, $"\\b{Regex.Escape(name)}=\"([^\"]*)\"");
        return match.Success ? WebUtility.HtmlDecode(match.Groups[1].Value) : "";
    }
    private static void Require(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }
    private sealed record NativeRecord(string Name);
}
