using System.Net;
using System.Reflection;
using System.Text.RegularExpressions;
using ArkheideSystem.Flourish.Blazor;
using ArkheideSystem.Flourish.Blazor.Abstract;
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
        tests.Add(("bulk completion uses one ordinary action aligned by the library rather than a wizard grid", async () =>
        {
            var root = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../.."));
            var framework = await File.ReadAllTextAsync(Path.Combine(root, "src/Flourish.Blazor/Flourish.Blazor.Framework/wwwroot/data-table.css"));
            Require(Regex.IsMatch(framework, @"\.f-data-bulk-actions\s*\{[^}]*justify-content:flex-end"), "The bulk action region does not align completion to its trailing edge.");
            var sample = await File.ReadAllTextAsync(Path.Combine(root, "src/Gallery.Flourish.Blazor/Components/Samples/Data/DataTableSample.razor"));
            var actions = Regex.Match(sample, @"<BulkEditActions>([\s\S]*?)</BulkEditActions>").Groups[1].Value;
            Require(Regex.Matches(actions, @"<Button\b").Count == 1 && !actions.Contains("UniformGrid", StringComparison.Ordinal), "The bulk example mixes completion with a wizard or an extra action.");
            Require(actions.Contains("RecordDemoIntent(); DemoBulk = false;", StringComparison.Ordinal), "Completion discarded the example's editing intent callback.");
            var parameters = Parameters(pageSize: 2);
            parameters[nameof(DataTable<Row>.BulkEditing)] = true;
            parameters[nameof(DataTable<Row>.BulkEditActions)] = (RenderFragment)(builder =>
            {
                builder.OpenComponent<Button>(0);
                builder.AddAttribute(1, nameof(Button.Text), "Concluir");
                builder.CloseComponent();
            });
            await WithTable(parameters, (_, html) =>
            {
                var region = Regex.Match(html(), @"<div class=""f-data-bulk-actions""[^>]*>([\s\S]*?)</div>").Groups[1].Value;
                Require(Regex.Matches(region, @"<button\b").Count == 1 && Regex.Replace(region, "<[^>]*>", string.Empty).Trim() == "Concluir", "The production table invented a refresh action or rewrote the completion label.");
                return Task.CompletedTask;
            });
        }));
        tests.Add(("record tables and column search export one contract with no retired API aliases", () =>
        {
            var framework = typeof(DataTable<>).Assembly;
            Require(framework.GetExportedTypes().Where(type => type.Name == "DataTable`1").SequenceEqual([typeof(DataTable<>)]), "A second table renderer remains exported.");
            Require(framework.GetExportedTypes().Where(type => type.Name == "DataSearch`1").SequenceEqual([typeof(DataSearch<>)]), "A second search renderer remains exported.");
            foreach (var name in new[] { "DataTable`1", "DataSearch`1", "DataFilter", "DataSorter", "TablePreferences" })
                Require(framework.GetType("ArkheideSystem.Flourish.Blazor.Components.Primitives." + name) is null, "A retired table/search entry remains available: " + name);
            foreach (var name in new[] { "DataColumn`1", "DataSearchRequest", "DataCellContext`1", "CardField", "TablePurpose", "SortDirection" })
                Require(typeof(TableColumn<>).Assembly.GetType("ArkheideSystem.Flourish.Blazor.Components.Primitives." + name) is null, "A retired table contract remains available: " + name);
            Require(typeof(DataTable<>).GetProperty("ShowSortControls") is null, "The deleted independent sorting controller can still be enabled.");
            return Task.CompletedTask;
        }));
        tests.Add(("record sorting is exposed only by table headers and never by independent tools", async () =>
        {
            foreach (var progressive in new[] { false, true })
            {
                var parameters = Parameters(pageSize: 2);
                parameters[nameof(DataTable<Row>.Progressive)] = progressive;
                await WithTable(parameters, (_, html) =>
                {
                    var rendered = html();
                    Require(!rendered.Contains("data-f-sort-reset", StringComparison.Ordinal) && !rendered.Contains("data-f-sort-direction", StringComparison.Ordinal), "A deleted standalone sort tool was rendered.");
                    var header = Regex.Match(rendered, "<thead>[\\s\\S]*?</thead>").Value;
                    Require(Tags(header, "button", "data-f-sort-key").Count == 2
                        && Regex.Matches(rendered, "data-f-sort-key=").Count == 2, "A sorting entry exists outside the table header.");
                    return Task.CompletedTask;
                });
            }
        }));
        tests.Add(("display choices themselves are full-row drag and keyboard targets without a separate handle", async () =>
        {
            foreach (var view in new[] { TableView.Table, TableView.Cards })
            {
                var parameters = Parameters(pageSize: 1);
                parameters[nameof(DataTable<Row>.View)] = view;
                await WithTable(parameters, (_, html) =>
                {
                    var choices = Tags(html(), "div", "data-f-selection-key");
                    Require(choices.Count == 2 && choices.All(tag => Attribute(tag, "class") == "f-multi-select-option"), "The display drag target is not the complete item.");
                    Require(choices.All(tag => Attribute(tag, "draggable") == (view == TableView.Table ? "true" : "false")
                        && Attribute(tag, "aria-disabled") == "false"
                        && Attribute(tag, "tabindex") == (view == TableView.Table ? "0" : "-1")), "Table and Cards do not share explicit reorder availability.");
                    Require(Tags(html(), "button", "data-f-selection-key").Count == 0 && !html().Contains("f-data-reorder", StringComparison.Ordinal), "A separate reorder handle remains in the display choices.");
                    return Task.CompletedTask;
                });
            }
        }));
        tests.Add(("canonical remote search forwards host query without filtering returned rows", async () =>
        {
            TableSearchRequest? changed = null;
            var parameters = Parameters(pageSize: 50);
            parameters[nameof(DataTable<Row>.Searchable)] = true;
            parameters[nameof(DataTable<Row>.SearchMode)] = TableSearchMode.Remote;
            parameters[nameof(DataTable<Row>.SearchQuery)] = "server token";
            parameters[nameof(DataTable<Row>.SearchColumnKey)] = "name";
            parameters[nameof(DataTable<Row>.SearchMaximumLength)] = 12;
            parameters[nameof(DataTable<Row>.SearchChanged)] = EventCallback.Factory.Create<TableSearchRequest>(new object(), request => changed = request);
            await WithTable(parameters, async (table, html) =>
            {
                Require(RowCount(html()) == 25, "Remote results were incorrectly filtered again in the browser component.");
                await table.QueryAsync("1234567890123456");
                Require(changed == new TableSearchRequest("name", "123456789012"), "Remote query changed column or exceeded its declared maximum.");
                Require(RowCount(html()) == 25, "The remote callback removed host-owned rows.");
            });
        }));
        tests.Add(("canonical progressive directories expose only readable SSR rows and explicit native transports", async () =>
        {
            var parameters = Parameters(pageSize: 1);
            parameters[nameof(DataTable<Row>.Progressive)] = true;
            parameters[nameof(DataTable<Row>.NativeRowOpen)] = true;
            parameters[nameof(DataTable<Row>.RowActionsContent)] = (RenderFragment<Row>)NativeMenu;
            await WithTable(parameters, (_, html) =>
            {
                Require(RowCount(html()) == 25 && Regex.Matches(html(), "data-record-open").Count == 25, "Standalone fallback discarded rows or duplicated native transports.");
                Require(html().Contains("data-f-progressive=\"true\"", StringComparison.Ordinal)
                    && html().Contains("f-data-openable", StringComparison.Ordinal), "Standalone directory did not use the canonical native controller.");
                Require(!html().Contains("data-f-values", StringComparison.Ordinal) && !html().Contains("data-f-columns", StringComparison.Ordinal), "Progressive mode serialized hidden business metadata.");
                return Task.CompletedTask;
            });
            foreach (var column in new[]
            {
                new TableColumn<Row>("name", "Name", row => row.Name, DefaultVisible: false),
                new TableColumn<Row>("name", "Name", row => row.Name, SortValue: row => row.Detail),
                new TableColumn<Row>("name", "Name", row => row.Name, SearchValue: row => row.Detail)
            })
            {
                parameters[nameof(DataTable<Row>.Columns)] = new[] { column };
                var refused = false;
                try { await WithTable(parameters, (_, _) => Task.CompletedTask); }
                catch (InvalidOperationException exception) when (exception.Message.Contains("already-visible", StringComparison.Ordinal)) { refused = true; }
                Require(refused, "Progressive directory accepted hidden defaults or independent metadata.");
            }
        }));
        tests.Add(("canonical bulk editing composes standard selectors and locks record opening while busy", async () =>
        {
            var opened = 0;
            var parameters = Parameters(pageSize: 1);
            parameters[nameof(DataTable<Row>.Items)] = new[] { new Row("One", "Detail") };
            parameters[nameof(DataTable<Row>.BulkEditing)] = true;
            parameters[nameof(DataTable<Row>.EditingBusy)] = true;
            parameters[nameof(DataTable<Row>.AllEditSelected)] = true;
            parameters[nameof(DataTable<Row>.IsSelectedForEditing)] = (Func<Row, bool>)(_ => true);
            parameters[nameof(DataTable<Row>.OnRowOpen)] = EventCallback.Factory.Create<Row>(new object(), _ => opened++);
            parameters[nameof(DataTable<Row>.BulkEditCell)] = (RenderFragment<TableColumn<Row>>)(column => builder => builder.AddContent(0, "Edit " + column.Header));
            await WithTable(parameters, async (table, html) =>
            {
                Require(Regex.Matches(html(), "class=\"f-checkbox").Count == 2 && html().Contains("f-data-bulk-row", StringComparison.Ordinal), "Bulk editing did not use real Flourish selectors and editor cells.");
                Require(!await table.CanLeavePageAsync(), "Busy bulk editing permitted page navigation.");
                await table.OpenAsync(table.Items[0]);
                Require(opened == 0 && !html().Contains("f-data-openable", StringComparison.Ordinal), "Bulk editor kept an active record-opening action.");
            });
        }));
        tests.Add(("canonical order controls preserve fixed slots and imperative widths are finite and resettable", async () =>
        {
            var parameters = Parameters(pageSize: 1);
            parameters[nameof(DataTable<Row>.Columns)] = new[]
            {
                new TableColumn<Row>("name", "Name", row => row.Name),
                new TableColumn<Row>("fixed", "Fixed", row => row.Name, CanHide: false, CanReorder: false),
                new TableColumn<Row>("detail", "Detail", row => row.Detail)
            };
            await WithTable(parameters, async (table, html) =>
            {
                await table.Display!.ApplyAsync(["detail","fixed","name"],["name","fixed","detail"]);
                Require(Tags(html(), "col", "data-f-column").Select(tag => Attribute(tag, "data-f-column")).SequenceEqual(new[] { "detail", "fixed", "name" }), "A fixed column changed slots during keyboard ordering.");
                await table.Display!.ApplyAsync(["name","fixed","detail"],["name","fixed","detail"]);
                Require(Tags(html(), "col", "data-f-column").Select(tag => Attribute(tag, "data-f-column")).SequenceEqual(new[] { "name", "fixed", "detail" }), "Dragging forwards did not reach the original target slot.");
                await table.Display!.ApplyAsync(["detail","fixed","name"],["name","fixed","detail"]);
                Require(Tags(html(), "col", "data-f-column").Select(tag => Attribute(tag, "data-f-column")).SequenceEqual(new[] { "detail", "fixed", "name" }), "Dragging backwards did not preserve the fixed slot.");
                await table.Display!.ApplyAsync(["detail","name","fixed"],["name","fixed","detail"]);
                Require(Tags(html(), "col", "data-f-column").Select(tag => Attribute(tag, "data-f-column")).SequenceEqual(new[] { "detail", "fixed", "name" }), "Dragging into a fixed column moved it.");
                await table.SetColumnWidth("name", 300000);
                Require(Regex.IsMatch(html(), "<col[^>]*data-f-column=\"name\"[^>]*style=\"width:100000px\""), "Manual width did not enforce the finite maximum.");
                Require(Tags(html(), "button", "data-f-resize").Any(tag => Attribute(tag, "data-f-resize") == "name" && Attribute(tag, "aria-valuenow") == "100000"), "The manual width and accessible resize value diverged before JavaScript enhancement.");
                await table.SetColumnWidth("name", double.NaN);
                await table.ResetColumnWidth("name");
                Require(!Regex.IsMatch(html(), "<col[^>]*data-f-column=\"name\"[^>]*style="), "Reset did not release the canonical manual width.");
                Require(Tags(html(), "button", "data-f-resize").Any(tag => Attribute(tag, "data-f-resize") == "name" && Attribute(tag, "aria-valuenow") == "72"), "Reset did not restore the accessible initial measurement boundary.");
            });
        }));
        tests.Add(("canonical Pool retains every editor while only the current page is visible", async () =>
        {
            var parameters = Parameters(pageSize: 1);
            parameters[nameof(DataTable<Row>.Purpose)] = TablePurpose.Pool;
            parameters[nameof(DataTable<Row>.Columns)] = new[] { new TableColumn<Row>("name", "Name", row => row.Name, UsesTemplate: true) };
            parameters[nameof(DataTable<Row>.CellTemplate)] = (RenderFragment<TableCellContext<Row>>)(cell => builder =>
            {
                builder.OpenElement(0, "input"); builder.AddAttribute(1, "value", cell.Item.Name); builder.AddAttribute(2, "data-editor", cell.Item.Name); builder.CloseElement();
            });
            await WithTable(parameters, async (table, html) =>
            {
                Require(RowCount(html()) == 25 && Regex.Matches(html(), "data-editor=").Count == 25, "Pool discarded off-page editor instances.");
                Require(Regex.Matches(html(), "<tr[^>]* hidden").Count == 24, "Pool did not hide exactly its off-page rows.");
                await table.ChangeAsync("data-f-page", null, "2");
                Require(RowCount(html()) == 25 && Regex.Matches(html(), "data-editor=").Count == 25, "Changing a Pool page replaced the retained editors.");
                Require(Regex.IsMatch(html(), "<tr[^>]*><td[^>]*><span[^>]*><input[^>]*data-editor=\"Name 2\""), "The second editor did not become visible.");
            });
        }));
        tests.Add(("canonical typed column separates display sorting search and initial visibility", async () =>
        {
            var parameters = Parameters(pageSize: 50);
            var rows = new[] { new Row("2", "search-only"), new Row("10", "other") };
            parameters[nameof(DataTable<Row>.Items)] = rows;
            parameters[nameof(DataTable<Row>.Columns)] = new[]
            {
                new TableColumn<Row>("name", "Name", row => row.Name, SortValue: row => int.Parse(row.Name), SearchValue: row => row.Detail),
                new TableColumn<Row>("detail", "Detail", row => row.Detail, DefaultVisible: false)
            };
            parameters[nameof(DataTable<Row>.Searchable)] = true;
            await WithTable(parameters, async (table, html) =>
            {
                Require(!Regex.IsMatch(html(), "<col[^>]*data-f-column=\"detail\""), "Default hidden column was rendered.");
                await table.SortAsync("name");
                Require(html().IndexOf("title=\"10\"", StringComparison.Ordinal) < html().IndexOf("title=\"2\"", StringComparison.Ordinal), "Canonical sorting used display strings instead of typed values.");
                await table.QueryAsync("search-only");
                Require(RowCount(html()) == 1 && html().Contains("title=\"2\"", StringComparison.Ordinal), "Search ignored the explicit independent search value.");
            });
        }));
        tests.Add(("canonical native GET opening uses one real standard menu link and respects unavailable rows", async () =>
        {
            var parameters = Parameters(pageSize: 1);
            parameters[nameof(DataTable<Row>.RowOpenHref)] = (Func<Row, string?>)(row => "/records/" + row.Name);
            await WithTable(parameters, (_, html) =>
            {
                Require(html().Contains("data-record-open", StringComparison.Ordinal) && html().Contains("href=\"/records/Name 1\"", StringComparison.Ordinal), "Native opening did not render a genuine link.");
                Require(html().Contains("data-f-native-open=\"true\"", StringComparison.Ordinal), "Native opening did not select the canonical synchronous controller.");
                Require(Regex.Matches(html(), "<a[^>]*data-record-open").Count == 1
                    && Regex.Matches(html(), "href=\"/records/Name 1\"").Count == 1, "Default opening was duplicated.");
                return Task.CompletedTask;
            });
            parameters[nameof(DataTable<Row>.RowOpenAvailable)] = (Func<Row, bool>)(_ => false);
            await WithTable(parameters, (_, html) =>
            {
                Require(!html().Contains("data-record-open", StringComparison.Ordinal) && !html().Contains("f-data-openable", StringComparison.Ordinal), "Unavailable native row remained interactive.");
                return Task.CompletedTask;
            });
        }));
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
                var current = Regex.Match(pager, @"<input\b[^>]*\bdata-f-page(?:\s|=|>)[^>]*>").Index;
                Require(previous >= 0 && previous < next && next < current, "Pager controls are not ordered previous, next, current.");
                return Task.CompletedTask;
            });
        }));
        tests.Add(("display options and sortable headers use shared component semantics", async () =>
        {
            await WithTable(Parameters(pageSize: 1), (_, html) =>
            {
                var rendered = html();
                Require(rendered.Contains("f-multi-select-panel f-dropdown-panel", StringComparison.Ordinal), "The display selector did not use the shared dropdown panel.");
                Require(rendered.Contains("f-dropdown-item is-selected", StringComparison.Ordinal), "Visible columns did not expose the shared selected state.");
                var display = Regex.Matches(rendered, @"<details\b[^>]*data-f-table-selection[\s\S]*?</details>").Single().Value;
                var displayTrigger = Regex.Match(display, @"<summary\b[^>]*>[\s\S]*?</summary>").Value;
                Require(Attribute(displayTrigger, "class").Split(' ', StringSplitOptions.RemoveEmptyEntries).Contains("f-button-secondary")
                    && displayTrigger.Contains("data-icon=\"remove_red_eye\"", StringComparison.Ordinal)
                    && Regex.IsMatch(displayTrigger, @"<span[^>]*class=""f-sr-only""[^>]*data-f-selection-label[^>]*>Display</span>")
                    && !displayTrigger.Contains("f-expansion-indicator", StringComparison.Ordinal), "Display options lost the shared icon trigger, accessible label or icon-only presentation.");
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
                Require(selects.Length == 3 && selects.All(tag => Attribute(tag, "class").Split(' ', StringSplitOptions.RemoveEmptyEntries).Contains("f-input")
                    && Attribute(tag, "class").Split(' ', StringSplitOptions.RemoveEmptyEntries).Contains("f-select")),
                    "Search and top/bottom page-size selectors are not styled as standard SelectBox controls.");
                return Task.CompletedTask;
            });
        }));
        tests.Add(("default item ranges omit the item prefix and quantity labels stay concise", async () =>
        {
            foreach (var progressive in new[] { false, true })
            {
                var parameters = Parameters(pageSize: 2);
                parameters[nameof(DataTable<Row>.Progressive)] = progressive;
                await WithTable(parameters, (_, html) =>
                {
                    var rendered = WebUtility.HtmlDecode(html());
                    var count = Regex.Match(rendered, @"<div class=""f-data-count"">([\s\S]*?)</div>").Groups[1].Value;
                    Require(count == "<span>1-2 / Total 25</span>", "The default item range regained an item prefix or lost its range/total.");
                    Require(Regex.Matches(rendered, @"<label class=""f-data-page-size"">\s*<span>Quantity</span>").Count == 2, "Page-size selectors kept the verbose quantity label.");
                    if (progressive)
                        Require(!Attribute(Tags(rendered, "div", "data-f-range-format").Single(), "data-f-range-format").Contains("{0}", StringComparison.Ordinal), "The browser pager received a prefixed default range.");
                    return Task.CompletedTask;
                });
                parameters[nameof(DataTable<Row>.Items)] = Array.Empty<Row>();
                await WithTable(parameters, (_, html) =>
                {
                    Require(WebUtility.HtmlDecode(html()).Contains("<span>0-0 / Total 0</span>", StringComparison.Ordinal), "The empty default range lost its unprefixed zero values.");
                    return Task.CompletedTask;
                });
            }
        }));
        tests.Add(("display and advanced editing keep secondary icon triggers and explicit capability boundaries", async () =>
        {
            foreach (var capabilities in new[] { 0, 1, 2, 3 })
            foreach (var state in new[] { "ready", "bulk", "busy" })
            {
                var requested = 0;
                var parameters = Parameters(pageSize: 2);
                if ((capabilities & 1) != 0)
                    parameters[nameof(DataTable<Row>.BulkEditRequested)] = EventCallback.Factory.Create(new object(), () => requested++);
                if ((capabilities & 2) != 0)
                    parameters[nameof(DataTable<Row>.SpreadsheetEditHref)] = "/edit-grid?scope=selected";
                parameters[nameof(DataTable<Row>.BulkEditing)] = state == "bulk";
                parameters[nameof(DataTable<Row>.EditingBusy)] = state == "busy";
                await WithTable(parameters, async (table, html) =>
                {
                    var rendered = WebUtility.HtmlDecode(html());
                    var advanced = Regex.Matches(rendered, @"<details\b[^>]*data-f-native-menu[\s\S]*?</details>").Single().Value;
                    var advancedTrigger = Regex.Match(advanced, @"<summary\b[^>]*>[\s\S]*?</summary>").Value;
                    var display = Regex.Matches(rendered, @"<details\b[^>]*data-f-table-selection[\s\S]*?</details>").Single().Value;
                    var displayTrigger = Regex.Match(display, @"<summary\b[^>]*>[\s\S]*?</summary>").Value;
                    foreach (var trigger in new[] { advancedTrigger, displayTrigger })
                        Require(Attribute(trigger, "class").Split(' ', StringSplitOptions.RemoveEmptyEntries).Contains("f-button-secondary")
                            && Attribute(trigger, "class").Split(' ', StringSplitOptions.RemoveEmptyEntries).Contains("f-button-icon"), "Table controls do not share the secondary icon variant.");
                    Require(advancedTrigger.Contains("data-icon=\"stylus_fountain_pen\"", StringComparison.Ordinal)
                        && !advancedTrigger.Contains(">Advanced editing<", StringComparison.Ordinal), "Advanced editing did not render only the requested icon.");
                    Require(displayTrigger.Contains("data-icon=\"remove_red_eye\"", StringComparison.Ordinal)
                        && Regex.IsMatch(displayTrigger, @"<span[^>]*class=""f-sr-only""[^>]*>Display</span>")
                        && !displayTrigger.Contains("f-expansion-indicator", StringComparison.Ordinal), "Display kept a visible caption or dropdown marker instead of its accessible icon trigger.");
                    var disabled = capabilities == 0 || state != "ready";
                    Require(Attribute(advancedTrigger, "aria-disabled") == disabled.ToString().ToLowerInvariant()
                        && Regex.IsMatch(Regex.Match(advanced, @"<details\b[^>]*>").Value, @"\binert(?:\s|=|>)") == disabled, "Advanced editing was hidden or its disabled/inert boundaries disagree.");
                    var links = Tags(advanced, "a", "role");
                    Require(links.Count == ((capabilities & 2) != 0 ? 1 : 0), "Spreadsheet capability did not preserve exactly one native link.");
                    if (links.Count != 0)
                        Require(Attribute(links[0], "href") == (state == "ready" ? "/edit-grid?scope=selected" : string.Empty), "The spreadsheet link lost its destination or remained active while editing.");
                    var bulk = table.MenuButtons.SingleOrDefault(button => button.OnClick.HasDelegate && button.Href is null
                        && button.AdditionalAttributes?.ContainsKey("role") == true);
                    Require((bulk is not null) == ((capabilities & 1) != 0), "The bulk menu item did not follow the declared capability.");
                    if (bulk is not null)
                    {
                        await (Task)typeof(Button).GetMethod("ClickAsync", BindingFlags.Instance | BindingFlags.NonPublic)!.Invoke(bulk, [new MouseEventArgs()])!;
                        Require(requested == (state == "ready" ? 1 : 0), "The real bulk Button callback was lost or bypassed the disabled boundary.");
                    }
                });
            }
        }));
        tests.Add(("shared dropdown options override ordinary Button flex centering", () =>
        {
            var rootPath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../.."));
            var framework = ExpandMenuCss(Path.Combine(rootPath, "src/Flourish.Blazor/Flourish.Blazor.Framework/wwwroot/framework.css"));
            var design = ExpandMenuCss(Path.Combine(rootPath, "src/Flourish.Blazor/Flourish.Blazor.Design/wwwroot/design.css"));
            Require(MenuItemProperty(framework, "display") == "block"
                && MenuItemProperty(framework, "text-align") == "start", "Unskinned native menu items lost their start alignment.");
            var combined = framework + "\n" + design;
            Require(MenuItemProperty(combined, "display") == "flex"
                && MenuItemProperty(combined, "justify-content") == "flex-start"
                && MenuItemProperty(combined, "text-align") == "start", "The Design cascade retained ordinary Button centering for menu options.");
            return Task.CompletedTask;
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
                await table.Display!.ApplyAsync(["name","detail"],["name"]);
                var rendered = html();
                Require(rendered.Contains("data-f-column=\"name\" data-f-last-column=\"true\"", StringComparison.Ordinal), "Hiding the last column did not transfer the uncapped marker.");
                Require(!Regex.IsMatch(rendered, @"<col\b[^>]*data-f-column=""detail"""), "The hidden data column remained in the sizing colgroup.");
                Require(!rendered.Contains("class=\"f-data-spacer\" data-f-last-column", StringComparison.Ordinal), "The spacer was treated as a data column.");
                Require(!rendered.Contains("class=\"f-data-actions\" data-f-last-column", StringComparison.Ordinal), "The action column was treated as a data column.");
            });
        }));
        tests.Add(("native row templates use the same ActionMenu in table and cards without a duplicate Open", async () =>
        {
            var rootPath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../.."));
            var framework = await File.ReadAllTextAsync(Path.Combine(rootPath, "src/Flourish.Blazor/Flourish.Blazor.Framework/wwwroot/framework.css"));
            Require(framework.Contains(".f-action-menu[data-f-native-menu]:not([data-f-enhanced])[open] > .f-menu-panel { display:block; position:static; inset:auto; }", StringComparison.Ordinal),
                "An unenhanced native menu must remain in flow so the last table row or card cannot clip its actions.");
            foreach (var view in new[] { TableView.Table, TableView.Cards })
            {
                var parameters = Parameters(pageSize: 2);
                parameters[nameof(DataTable<Row>.View)] = view;
                parameters[nameof(DataTable<Row>.OnRowOpen)] = EventCallback.Factory.Create<Row>(new object(), (Row _) => { });
                parameters[nameof(DataTable<Row>.RowActionsContent)] = (RenderFragment<Row>)NativeMenu;
                await WithTable(parameters, (_, html) =>
                {
                    var rendered = html();
                    Require(Regex.Matches(rendered, "class=\"f-action-menu ").Count == 3, "Native rows and the persistent advanced control did not share the standard ActionMenu.");
                    var nativeMenus = Regex.Matches(rendered, @"<details\b[^>]*data-f-native-menu[\s\S]*?</details>").Where(menu => menu.Value.Contains("data-record-open", StringComparison.Ordinal)).ToArray();
                    Require(nativeMenus.Length == 2 && nativeMenus.All(menu => !menu.Value.Contains("popover=", StringComparison.Ordinal)), "Static native action slots were hidden behind a JavaScript-only popover.");
                    Require(!rendered.Contains("row-action-menu", StringComparison.Ordinal), "A second menu family entered the standard table.");
                    Require(Regex.Matches(rendered, "data-record-open").Count == 2, "The declared native opening transports were lost or duplicated.");
                    Require(rendered.Contains("href=\"/native-entry/?record=1\"", StringComparison.Ordinal)
                        && rendered.Contains("method=\"post\"", StringComparison.Ordinal)
                        && rendered.Contains("action=\"/native-entry/\"", StringComparison.Ordinal)
                        && rendered.Contains("target=\"_blank\"", StringComparison.Ordinal)
                        && rendered.Contains("data-enhance=\"false\"", StringComparison.Ordinal), "Native GET / POST / new-tab attributes were changed.");
                    Require(Regex.Matches(rendered, "data-f-native-open=\"true\"").Count == 2, "Both views lost their explicit native opening markers.");
                    Require(!rendered.Contains(">Open</button>", StringComparison.Ordinal), "The native template received a duplicate automatic Open action.");
                    return Task.CompletedTask;
                });
            }
        }));
        tests.Add(("row opening availability guards generated menus, markers and callback invocation", async () =>
        {
            foreach (var view in new[] { TableView.Table, TableView.Cards })
            {
                var opened = new List<Row>();
                var parameters = Parameters(pageSize: 2);
                parameters[nameof(DataTable<Row>.View)] = view;
                parameters[nameof(DataTable<Row>.OnRowOpen)] = EventCallback.Factory.Create<Row>(opened, (Row row) => opened.Add(row));
                parameters[nameof(DataTable<Row>.RowOpenAvailable)] = (Func<Row, bool>)(row => row.Name == "Name 1");
                await WithTable(parameters, async (table, html) =>
                {
                    Require(Regex.Matches(html(), "f-data-openable").Count == 1, "Unavailable rows were marked interactive.");
                    Require(Regex.Matches(html(), ">Open</button>").Count == 1, "Unavailable rows received the generated opening action.");
                    await table.OpenAsync(new("Name 2", "Detail 2"));
                    await table.OpenAsync(new("Name 1", "Detail 1"));
                    Require(opened.Count == 1 && opened[0].Name == "Name 1", "Opening bypassed the current per-row guard.");
                });
            }
        }));
        tests.Add(("native action content alone does not invent default row opening", async () =>
        {
            var parameters = Parameters(pageSize: 1);
            parameters[nameof(DataTable<Row>.RowActionsContent)] = (RenderFragment<Row>)NativeMenu;
            await WithTable(parameters, (_, html) =>
            {
                Require(html().Contains("data-record-open", StringComparison.Ordinal), "The native menu content was lost.");
                Require(!html().Contains("f-data-openable", StringComparison.Ordinal)
                    && !html().Contains("data-f-native-open", StringComparison.Ordinal), "Content alone invented an opening callback.");
                return Task.CompletedTask;
            });
        }));
        tests.Add(("standard table rejects competing structured and native menu bindings", async () =>
        {
            var parameters = Parameters(pageSize: 1, withActions: true);
            parameters[nameof(DataTable<Row>.RowActionsContent)] = (RenderFragment<Row>)NativeMenu;
            try { await WithTable(parameters, (_, _) => Task.CompletedTask); }
            catch (InvalidOperationException exception) when (exception.Message.Contains("Actions or RowActionsContent", StringComparison.Ordinal)) { return; }
            throw new InvalidOperationException("Competing action sources were accepted or silently discarded.");
        }));
        tests.Add(("standard table restores and saves sort with the existing scoped preference service", async () =>
        {
            ITablePreferences preferences = null!;
            var parameters = Parameters(pageSize: 2);
            parameters[nameof(DataTable<Row>.PreferenceKey)] = "account.directory";
            await WithTable(parameters, async (table, html) =>
            {
                Require(Regex.IsMatch(html(), @"<th\b[^>]*data-f-column=""name""[^>]*aria-sort=""ascending"""), "The stored ascending sort was not restored.");
                await table.SortAsync("name");
                Require(!preferences.TryGetSort("account.directory", out _), "Default order did not remove the stored preference.");
                await table.SortAsync("detail");
                Require(preferences.TryGetSort("account.directory", out var stored)
                    && stored.SortKey == "detail" && stored.Descending, "Sorting did not use the existing service.");
                Require(!preferences.TryGetSort("account.other", out _), "A sibling list acquired another list's preference.");
            }, provider =>
            {
                preferences = provider.GetRequiredService<ITablePreferences>();
                preferences.SetSort("account.directory", new("name", false));
            });
        }));
        tests.Add(("changing a preference key isolates sort and invalid stored columns are removed", async () =>
        {
            ITablePreferences preferences = null!;
            var parameters = Parameters(pageSize: 1);
            parameters[nameof(DataTable<Row>.PreferenceKey)] = "account.first";
            await WithTable(parameters, async (table, html) =>
            {
                Require(html().Contains("aria-sort=\"descending\"", StringComparison.Ordinal), "The first list's sort was not restored.");
                await table.ParametersAsync(new() { [nameof(DataTable<Row>.PreferenceKey)] = "account.second" });
                Require(!html().Contains("aria-sort=\"descending\"", StringComparison.Ordinal), "The second list inherited the first list's sort.");
                Require(!preferences.TryGetSort("account.second", out _), "A missing column retained a stale preference.");
                Require(preferences.TryGetSort("account.first", out _), "Changing scopes erased the first list's preference.");
            }, provider =>
            {
                preferences = provider.GetRequiredService<ITablePreferences>();
                preferences.SetSort("account.first", new("name", true));
                preferences.SetSort("account.second", new("deleted-column", true));
            });
        }));
        tests.Add(("standard ActionMenu native content keeps disabled boundaries and rejects competing Actions", async () =>
        {
            var services = new ServiceCollection();
            services.AddLogging().AddFlourishFramework();
            services.AddSingleton<IJSRuntime, NoopJsRuntime>();
            using var provider = services.BuildServiceProvider();
            using var scope = provider.CreateScope();
            await using var renderer = new HtmlRenderer(scope.ServiceProvider, provider.GetRequiredService<ILoggerFactory>());
            await renderer.Dispatcher.InvokeAsync(async () =>
            {
                var native = NativeMenu(new("Name 1", "Detail 1"));
                var output = await renderer.RenderComponentAsync<ActionMenu>(ParameterView.FromDictionary(new Dictionary<string, object?>
                {
                    [nameof(ActionMenu.ChildContent)] = native,
                    [nameof(ActionMenu.Disabled)] = true
                }));
                var rendered = output.ToHtmlString();
                Require(rendered.Contains("class=\"f-action-menu ", StringComparison.Ordinal)
                    && Regex.IsMatch(rendered, @"<details\b[^>]*\binert(?:\s|=|>)"), "The native menu bypassed the shared disabled boundary.");
                try
                {
                    await renderer.RenderComponentAsync<ActionMenu>(ParameterView.FromDictionary(new Dictionary<string, object?>
                    {
                        [nameof(ActionMenu.ChildContent)] = native,
                        [nameof(ActionMenu.Actions)] = new[] { new MenuAction("Competing", () => Task.CompletedTask) }
                    }));
                }
                catch (InvalidOperationException exception) when (exception.Message.Contains("Actions or ChildContent", StringComparison.Ordinal)) { return; }
                throw new InvalidOperationException("ActionMenu accepted two independent content contracts at once.");
            });
        }));
        tests.Add(("native ActionMenu enhances one SSR DOM and detaches its old branch before switching", async () =>
        {
            var menu = new ActionMenuProbe();
            var javascript = new MenuJavaScript();
            await WithMenu(menu, javascript, async output =>
            {
                Require(output().Contains("<details", StringComparison.Ordinal) && !output().Contains("popover=", StringComparison.Ordinal), "SSR did not keep a usable native disclosure.");
                await menu.AfterRenderAsync();
                await menu.AfterRenderAsync();
                Require(javascript.Module.Calls.SequenceEqual(new[] { "attachDisclosureMenu" }), "The native branch did not use the existing disclosure controller.");
                Require(output().Contains("popover=\"manual\"", StringComparison.Ordinal)
                    && Regex.Matches(output(), "data-record-open").Count == 1, "Enhancement duplicated native content or did not adopt the top layer.");
                Require(!output().Contains("f-dropdown-surface", StringComparison.Ordinal), "The enhanced top layer retained static details geometry, including viewport-wide min-width.");
                await menu.ParametersAsync(new()
                {
                    [nameof(ActionMenu.ChildContent)] = null,
                    [nameof(ActionMenu.Actions)] = new[] { new MenuAction("Ordinary", () => Task.CompletedTask) }
                });
                await menu.AfterRenderAsync();
                await menu.ParametersAsync(new()
                {
                    [nameof(ActionMenu.Actions)] = Array.Empty<MenuAction>(),
                    [nameof(ActionMenu.ChildContent)] = NativeMenu(new("Name 1", "Detail 1"))
                });
                await menu.AfterRenderAsync();
                Require(javascript.Module.Calls.SequenceEqual(new[] { "attachDisclosureMenu", "detachMenu", "attachMenu", "detachMenu", "attachDisclosureMenu" }),
                    "Branch switching retained an old controller or changed the ordinary menu path.");
                await menu.DisposeAsync();
                await menu.DisposeAsync();
                Require(javascript.Module.Calls.Last() == "detachMenu" && javascript.Module.DisposeCount == 1, "The attached menu was not disposed exactly once.");
            });
        }));
        tests.Add(("native ActionMenu discards a delayed browser import after disposal", async () =>
        {
            var menu = new ActionMenuProbe();
            var javascript = new MenuJavaScript(delayed: true);
            await WithMenu(menu, javascript, async _ =>
            {
                var loading = menu.AfterRenderAsync();
                await menu.DisposeAsync();
                javascript.CompleteImport();
                await loading;
                Require(javascript.Module.DisposeCount == 1 && javascript.Module.Calls.Count == 0,
                    "A late module was retained or reattached after the native menu left.");
            });
        }));
        tests.Add(("native ActionMenu rejects unsupported hover instead of silently ignoring the public parameter", async () =>
        {
            var services = new ServiceCollection();
            services.AddLogging().AddFlourishFramework();
            services.AddSingleton<IJSRuntime, NoopJsRuntime>();
            using var provider = services.BuildServiceProvider();
            using var scope = provider.CreateScope();
            await using var renderer = new HtmlRenderer(scope.ServiceProvider, provider.GetRequiredService<ILoggerFactory>());
            await renderer.Dispatcher.InvokeAsync(async () =>
            {
                try
                {
                    await renderer.RenderComponentAsync<ActionMenu>(ParameterView.FromDictionary(new Dictionary<string, object?>
                    {
                        [nameof(ActionMenu.ChildContent)] = NativeMenu(new("Name 1", "Detail 1")),
                        [nameof(ActionMenu.OpenOnHover)] = true
                    }));
                }
                catch (InvalidOperationException exception) when (exception.Message.Contains("OpenOnHover", StringComparison.Ordinal)) { return; }
                throw new InvalidOperationException("Unsupported native hover was silently ignored.");
            });
        }));
    }

    private static async Task WithMenu(ActionMenuProbe menu, MenuJavaScript javascript, Func<Func<string>, Task> verify)
    {
        var services = new ServiceCollection();
        services.AddLogging().AddFlourishFramework();
        services.AddSingleton<IJSRuntime>(javascript);
        services.AddSingleton<IComponentActivator>(new MenuActivator(menu));
        using var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();
        await using var renderer = new HtmlRenderer(scope.ServiceProvider, provider.GetRequiredService<ILoggerFactory>());
        await renderer.Dispatcher.InvokeAsync(async () =>
        {
            var output = await renderer.RenderComponentAsync<ActionMenuProbe>(ParameterView.FromDictionary(new Dictionary<string, object?>
            {
                [nameof(ActionMenu.ChildContent)] = NativeMenu(new("Name 1", "Detail 1"))
            }));
            await verify(output.ToHtmlString);
        });
    }

    private static RenderFragment NativeMenu(Row row) => builder =>
    {
        var link = row.Name == "Name 1";
        builder.OpenElement(0, link ? "a" : "form");
        if (link) builder.AddAttribute(1, "href", "/native-entry/?record=1");
        else
        {
            builder.AddAttribute(2, "method", "post");
            builder.AddAttribute(3, "action", "/native-entry/");
            builder.AddAttribute(4, "data-enhance", "false");
        }
        builder.AddAttribute(5, "target", "_blank");
        builder.AddAttribute(6, "rel", "noopener");
        builder.AddAttribute(7, "data-record-open", true);
        if (!link) { builder.OpenElement(8, "button"); builder.AddAttribute(9, "type", "submit"); }
        builder.AddAttribute(10, "role", "menuitem");
        builder.AddAttribute(11, "class", "f-menu-item f-dropdown-item");
        builder.AddContent(12, "Native open");
        if (!link) builder.CloseElement();
        builder.CloseElement();
    };

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

    private static async Task WithTable(Dictionary<string, object?> parameters, Func<DataTableProbe, Func<string>, Task> verify,
        Action<IServiceProvider>? prepare = null)
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
        prepare?.Invoke(scope.ServiceProvider);
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

    private static string ExpandMenuCss(string path)
    {
        var source = Regex.Replace(File.ReadAllText(path), @"/\*[\s\S]*?\*/", string.Empty);
        return Regex.Replace(source, "@import\\s+url\\(['\"]([^'\"]+)['\"]\\)\\s*;", match =>
            ExpandMenuCss(Path.GetFullPath(Path.Combine(Path.GetDirectoryName(path)!, match.Groups[1].Value))));
    }

    // Resolve only the base declarations that apply to a Button menu item. State and
    // component-specific selectors are outside this alignment regression's scope.
    private static string MenuItemProperty(string css, string property)
    {
        var winner = string.Empty;
        var specificity = -1;
        foreach (Match rule in Regex.Matches(css, @"(?<selector>[^{}]+)\{(?<body>[^{}]*)\}"))
        foreach (var selector in rule.Groups["selector"].Value.Split(',').Select(value => Regex.Replace(value.Trim(), @"\s+", " ")))
        {
            var score = selector switch
            {
                ".f-button" or ".f-menu-item" => 1,
                ".f-dropdown-panel .f-dropdown-item" => 2,
                _ => -1
            };
            if (score < specificity || score < 0) continue;
            foreach (Match declaration in Regex.Matches(rule.Groups["body"].Value, $@"(?:^|;)\s*{Regex.Escape(property)}\s*:\s*(?<value>[^;}}]+)"))
            {
                winner = declaration.Groups["value"].Value.Trim();
                specificity = score;
            }
        }
        return winner;
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
            if (component is MultiSelectBox display && Table is not null) Table.Display = display;
            if (component is Button button && Table is not null) Table.MenuButtons.Add(button);
            return component;
        }
    }

    private sealed class DataTableProbe : DataTable<Row>
    {
        public DataTableProbe() { }
        internal MultiSelectBox? Display { get; set; }
        internal List<Button> MenuButtons { get; } = [];

        internal Task OpenAsync(Row row) => (Task)typeof(DataTable<Row>).GetMethod("Open", BindingFlags.Instance | BindingFlags.NonPublic)!.Invoke(this, [row])!;
        internal async Task QueryAsync(string value)
        {
            var column = (string)typeof(DataTable<Row>).GetField("_searchColumn", BindingFlags.Instance | BindingFlags.NonPublic)!.GetValue(this)!;
            await (Task)typeof(DataTable<Row>).GetMethod("ChangeSearchAsync", BindingFlags.Instance | BindingFlags.NonPublic)!.Invoke(this, [new TableSearchRequest(column, value)])!;
            StateHasChanged();
        }
        internal Task ParametersAsync(Dictionary<string, object?> parameters) => SetParametersAsync(ParameterView.FromDictionary(parameters));

        internal Task SortAsync(string key)
        {
            typeof(DataTable<Row>).GetMethod("SortColumn", BindingFlags.Instance | BindingFlags.NonPublic)!.Invoke(this, [Columns.Single(column => column.Key == key)]);
            StateHasChanged();
            return Task.CompletedTask;
        }

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

    private sealed class ActionMenuProbe : ActionMenu
    {
        public ActionMenuProbe() { }
        internal Task AfterRenderAsync() => base.OnAfterRenderAsync(false);
        internal Task ParametersAsync(Dictionary<string, object?> parameters) => SetParametersAsync(ParameterView.FromDictionary(parameters));
    }

    private sealed class MenuActivator(ActionMenuProbe menu) : IComponentActivator
    {
        public IComponent CreateInstance(Type componentType) => componentType == typeof(ActionMenuProbe)
            ? menu : (IComponent)Activator.CreateInstance(componentType)!;
    }

    private sealed class MenuJavaScript(bool delayed = false) : IJSRuntime
    {
        private readonly TaskCompletionSource<IJSObjectReference> import = new(TaskCreationOptions.RunContinuationsAsynchronously);
        internal MenuModule Module { get; } = new();
        internal void CompleteImport() => import.TrySetResult(Module);
        public ValueTask<TValue> InvokeAsync<TValue>(string identifier, object?[]? args)
        {
            if (identifier != "import") throw new InvalidOperationException($"Unexpected browser call: {identifier}");
            return delayed ? new ValueTask<TValue>(AwaitImport<TValue>()) : ValueTask.FromResult((TValue)(object)Module);
        }
        private async Task<TValue> AwaitImport<TValue>() => (TValue)(object)await import.Task;
        public ValueTask<TValue> InvokeAsync<TValue>(string identifier, CancellationToken cancellationToken, object?[]? args) => InvokeAsync<TValue>(identifier, args);
    }

    private sealed class MenuModule : IJSObjectReference
    {
        internal List<string> Calls { get; } = [];
        internal int DisposeCount;
        public ValueTask<TValue> InvokeAsync<TValue>(string identifier, object?[]? args)
        {
            Calls.Add(identifier);
            return ValueTask.FromResult(default(TValue)!);
        }
        public ValueTask<TValue> InvokeAsync<TValue>(string identifier, CancellationToken cancellationToken, object?[]? args) => InvokeAsync<TValue>(identifier, args);
        public ValueTask DisposeAsync() { DisposeCount++; return ValueTask.CompletedTask; }
    }
}
