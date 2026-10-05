using System.Globalization;
using System.Net;
using System.Reflection;
using System.Text.RegularExpressions;
using ArkheideSystem.Flourish.Blazor;
using ArkheideSystem.Flourish.Blazor.Abstract;
using ArkheideSystem.Flourish.Blazor.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;

internal static class ListViewChecks
{
    internal static void Register(List<(string Name, Func<Task> Run)> tests)
    {
        tests.Add(("readonly list renders every row in supplied order without JavaScript or table operations", async () =>
        {
            var rows = Enumerable.Range(1, 25).Reverse().Select(index => new Row(index, $"Record {index:00}", index, null, true)).ToArray();
            var html = await Render(Parameters(rows));
            var bodyRows = BodyRows(html);
            Require(bodyRows.Count == 25, "A readonly list silently paginated or filtered the supplied records.");
            Require(bodyRows.Select(row => CellTexts(row)[0]).SequenceEqual(rows.Select(row => row.Name)), "A readonly list reordered the supplied records.");
            Require(Regex.IsMatch(html, "class=\"f-data f-list-view(?:\\s|\")"), "The list did not reuse the standard data surface.");
            Require(html.Contains("class=\"f-data-table\"", StringComparison.Ordinal), "The list did not reuse the standard table style.");
            Require(!Regex.IsMatch(html, @"<(?:button|input|select|details|a)\b|\b(?:onclick|ondblclick|onchange|aria-sort|data-f-table|data-f-column|data-f-page)\b|f-data-(?:toolbar|resize|actions|spacer|sort|pager)"),
                "A readonly list rendered table controllers or interactive row actions.");
        }));

        tests.Add(("readonly list shares column metadata and cell context contracts without exposing dynamic API", () =>
        {
            var type = typeof(ListView<Row>);
            var parameters = type.GetProperties().Where(property => property.IsDefined(typeof(ParameterAttribute), true)).ToArray();
            foreach (var name in new[] { "Actions", "Searchable", "ShowSearch", "View", "PageSize", "PageChanged", "CurrentPage", "OnRowOpen", "Loading", "PreferenceKey", "SelectionChanged", "BulkEditing" })
                Require(parameters.All(parameter => parameter.Name != name), "The readonly list exposes dynamic API: " + name);
            Require(type.GetProperty(nameof(ListView<Row>.Columns))!.PropertyType == typeof(IReadOnlyList<TableColumn<Row>>), "The list introduced a different column contract.");
            Require(typeof(TableCellContext<Row>).Assembly == typeof(TableColumn<Row>).Assembly, "Cell templates moved contracts into the rendering assembly.");
            foreach (var name in new[] { nameof(ListView<Row>.Items), nameof(ListView<Row>.Columns) })
                Require(type.GetProperty(name)!.IsDefined(typeof(EditorRequiredAttribute), true), name + " is not editor-required.");
            return Task.CompletedTask;
        }));

        tests.Add(("readonly list keeps column order and ignores sorting searching and visibility metadata", async () =>
        {
            var columns = new TableColumn<Row>[]
            {
                new("amount", "Amount", row => row.Amount, Sortable: true, Searchable: false, CanHide: true),
                new("name", "Name", row => row.Name, Sortable: false, Searchable: true, CanHide: false)
            };
            var html = await Render(Parameters([new(1, "Second column", 12m, null, false)], columns));
            var headers = Regex.Match(html, @"<thead>[\s\S]*?</thead>").Value;
            Require(CellTexts(headers).SequenceEqual(new[] { "Amount", "Name" }), "Declared columns were hidden or reordered.");
            Require(CellTexts(BodyRows(html).Single()).SequenceEqual(new[] { "12", "Second column" }), "Column values did not follow declared order.");
        }));

        tests.Add(("readonly list formats culture dates nulls and explicit format while encoding all default text", async () =>
        {
            var culture = CultureInfo.GetCultureInfo("pt-BR");
            var date = new DateTime(2026, 10, 5);
            var row = new Row(1, "<script>alert('unsafe')</script> & text", 1234.5m, date, true);
            TableColumn<Row>[] columns =
            [
                new("name", "Name <img>", item => item.Name),
                new("amount", "Amount", item => item.Amount),
                new("date", "Date", item => item.Date),
                new("currency", "Currency", item => item.Amount, Format: item => item.Amount?.ToString("C", culture)),
                new("null", "Null", _ => null),
                new("null-format", "Null format", _ => "Ignored raw value", Format: _ => null)
            ];
            var parameters = Parameters([row], columns);
            parameters[nameof(ListView<Row>.Culture)] = culture;
            var html = await Render(parameters);
            Require(CellTexts(BodyRows(html).Single()).SequenceEqual(new[]
            {
                row.Name!, "1234,5", date.ToString("d", culture), row.Amount!.Value.ToString("C", culture), string.Empty, string.Empty
            }), "Readonly formatting diverged from the shared TableColumn formatter.");
            Require(!html.Contains("<script>", StringComparison.Ordinal) && !html.Contains("<img>", StringComparison.Ordinal)
                && html.Contains("&lt;script&gt;", StringComparison.Ordinal) && html.Contains("Name &lt;img&gt;", StringComparison.Ordinal),
                "A header or default cell value was rendered as untrusted markup.");
        }));

        tests.Add(("readonly list exposes accessible scroll region caption column and row headers", async () =>
        {
            var parameters = Parameters([new(1, "Feature", 0m, null, true)]);
            parameters[nameof(ListView<Row>.Label)] = "Comparison & values";
            parameters[nameof(ListView<Row>.Caption)] = "Feature <caption>";
            parameters[nameof(ListView<Row>.RowHeaderKey)] = "name";
            var html = await Render(parameters);
            var region = Regex.Match(html, "<div[^>]*class=\"f-data-scroll\"[^>]*>").Value;
            Require(Attribute(region, "role") == "region" && Attribute(region, "tabindex") == "0"
                && Attribute(region, "aria-label") == "Comparison & values", "The overflow region has no keyboard access or accessible label.");
            Require(Regex.IsMatch(html, "<caption class=\"f-sr-only\">Feature &lt;caption&gt;</caption>"), "The accessible caption was absent or unencoded.");
            Require(Regex.Matches(html, "<th scope=\"col\">").Count == 2, "Column headers lost their native table scope.");
            Require(Regex.Matches(html, "<th scope=\"row\">").Count == 1, "The designated row-header column did not use th scope=row.");
            Require(Regex.Matches(BodyRows(html).Single(), @"<td\b").Count == 1, "A row header was duplicated as an ordinary data cell.");
        }));

        tests.Add(("readonly list custom cell template receives original item column and formatted display text", async () =>
        {
            var row = new Row(1, "Feature", 0m, null, true);
            var contexts = new List<TableCellContext<Row>>();
            TableColumn<Row>[] columns = [new("name", "Feature", item => item.Name), new("enabled", "Enabled", item => item.Available, Format: _ => "Host display")];
            var parameters = Parameters([row], columns);
            parameters[nameof(ListView<Row>.RowHeaderKey)] = "name";
            parameters[nameof(ListView<Row>.CellTemplate)] = (RenderFragment<TableCellContext<Row>>)(cell => builder =>
            {
                contexts.Add(cell);
                if (cell.Column.Key == "enabled")
                {
                    builder.OpenElement(0, "span");
                    builder.AddAttribute(1, "aria-label", cell.Item.Available ? "Yes" : "No");
                    builder.AddContent(2, cell.Item.Available ? "✔" : "✘");
                    builder.CloseElement();
                }
                else builder.AddContent(3, cell.Text);
            });
            var html = await Render(parameters);
            Require(contexts.Count == 2 && contexts.All(context => ReferenceEquals(context.Item, row)), "The template did not receive the original item for each cell.");
            Require(ReferenceEquals(contexts[0].Column, columns[0]) && ReferenceEquals(contexts[1].Column, columns[1])
                && contexts[1].Text == "Host display", "The template lost the shared column metadata or formatted text.");
            Require(html.Contains("aria-label=\"Yes\"", StringComparison.Ordinal) && WebUtility.HtmlDecode(html).Contains("✔", StringComparison.Ordinal), "The template's accessible readonly indicator was lost.");
            Require(!html.Contains("<button", StringComparison.Ordinal), "The template was wrapped in an action control.");
        }));

        tests.Add(("empty readonly list preserves headers and renders the host empty status and caption fallback", async () =>
        {
            var parameters = Parameters(Array.Empty<Row>());
            parameters[nameof(ListView<Row>.Label)] = "Available licences";
            parameters[nameof(ListView<Row>.EmptyMessage)] = "No <licences>";
            var html = await Render(parameters);
            Require(Regex.Matches(html, "<th scope=\"col\">").Count == 2, "An empty list discarded its declared headers.");
            Require(html.Contains("<caption class=\"f-sr-only\">Available licences</caption>", StringComparison.Ordinal), "An omitted caption did not use the region label.");
            Require(Regex.IsMatch(html, "<td colspan=\"2\"><span class=\"f-data-cell\" role=\"status\">No &lt;licences&gt;</span></td>"), "The empty status has incorrect column span, text or accessibility semantics.");
        }));

        tests.Add(("empty readonly list accepts zero columns but populated lists reject them", async () =>
        {
            var html = await Render(Parameters(Array.Empty<Row>(), Array.Empty<TableColumn<Row>>()));
            Require(html.Contains("colspan=\"1\"", StringComparison.Ordinal) && html.Contains("No items.", StringComparison.Ordinal), "An empty zero-column list has no valid empty state.");
            await Reject(Parameters([new(1, "Record", null, null, false)], Array.Empty<TableColumn<Row>>()), "Columns");
        }));

        tests.Add(("readonly list validates null inputs before rendering", async () =>
        {
            foreach (var name in new[] { nameof(ListView<Row>.Items), nameof(ListView<Row>.Columns), nameof(ListView<Row>.Culture) })
            {
                var parameters = Parameters(Array.Empty<Row>());
                parameters[name] = null;
                await Reject(parameters, name);
            }
        }));

        tests.Add(("readonly list rejects null blank and duplicate column keys and missing value selectors", async () =>
        {
            foreach (var columns in new TableColumn<Row>[][]
            {
                [null!], [new(" ", "Blank", item => item.Name)],
                [new("name", "First", item => item.Name), new("name", "Duplicate", item => item.Name)]
            }) await Reject(Parameters(Array.Empty<Row>(), columns), "Columns");
            await Reject(Parameters(Array.Empty<Row>(), [new("name", "Name", null!)]), null);
        }));

        tests.Add(("readonly list rejects a row-header key outside the declared columns", async () =>
        {
            foreach (var key in new[] { "missing", "NAME", string.Empty })
            {
                var parameters = Parameters(Array.Empty<Row>());
                parameters[nameof(ListView<Row>.RowHeaderKey)] = key;
                await Reject(parameters, nameof(ListView<Row>.RowHeaderKey));
            }
        }));

        tests.Add(("readonly list preserves host root attributes while keeping its standard classes", async () =>
        {
            var parameters = Parameters(Array.Empty<Row>());
            parameters[nameof(ListView<Row>.Class)] = "comparison";
            parameters[nameof(ListView<Row>.AdditionalAttributes)] = new Dictionary<string, object> { ["id"] = "licences", ["data-owner"] = "host", ["aria-describedby"] = "comparison-help" };
            var html = await Render(parameters);
            var root = Regex.Match(html, @"^<div\b[^>]*>").Value;
            Require(Attribute(root, "id") == "licences" && Attribute(root, "data-owner") == "host"
                && Attribute(root, "aria-describedby") == "comparison-help", "The list discarded root protocol or accessible attributes.");
            Require(new[] { "f-data", "f-list-view", "comparison" }.All(cssClass => Attribute(root, "class").Split(' ', StringSplitOptions.RemoveEmptyEntries).Contains(cssClass)), "Host classes displaced the standard list classes.");
        }));

        tests.Add(("readonly list refreshes scoped text and format culture and releases its subscription", async () =>
        {
            using var provider = Services(translated: true);
            using var scope = provider.CreateScope();
            using var independent = provider.CreateScope();
            var text = (TrackingTextProvider)scope.ServiceProvider.GetRequiredService<ITextProvider>();
            var other = (TrackingTextProvider)independent.ServiceProvider.GetRequiredService<ITextProvider>();
            await using var renderer = new HtmlRenderer(scope.ServiceProvider, provider.GetRequiredService<ILoggerFactory>());
            var parameters = Parameters([new(1, "Record", 1234.5m, null, true)]);
            var output = await renderer.Dispatcher.InvokeAsync(() => renderer.RenderComponentAsync<ListView<Row>>(ParameterView.FromDictionary(parameters)));
            Require(text.Subscribers == 1, "The list did not subscribe exactly once to its scoped text provider.");
            var initial = await renderer.Dispatcher.InvokeAsync(output.ToHtmlString);
            Require(initial.Contains("en-US:Flourish/Table_Label", StringComparison.Ordinal) && CellTexts(BodyRows(initial).Single())[1] == "1234.5", "Initial scoped text or culture was lost.");
            await Task.Run(() => text.Select("pt-BR"));
            var changed = await renderer.Dispatcher.InvokeAsync(output.ToHtmlString);
            Require(changed.Contains("pt-BR:Flourish/Table_Label", StringComparison.Ordinal) && CellTexts(BodyRows(changed).Single())[1] == "1234,5", "Text changes did not refresh static list formatting.");
            Require(text.Subscribers == 1 && other.Culture == "en-US", "A refresh duplicated subscriptions or changed another circuit's culture.");
            await renderer.DisposeAsync();
            Require(text.Subscribers == 0, "The disposed readonly list retained its text subscription.");
        }));

        tests.Add(("readonly list scoped empty defaults refresh while explicit label caption text and culture win", async () =>
        {
            using var provider = Services(translated: true);
            using var scope = provider.CreateScope();
            var text = (TrackingTextProvider)scope.ServiceProvider.GetRequiredService<ITextProvider>();
            text.Select("pt-BR");
            var defaults = await Render(scope, Parameters(Array.Empty<Row>()));
            Require(defaults.Contains("pt-BR:Flourish/Table_Label", StringComparison.Ordinal) && defaults.Contains("pt-BR:Flourish/Table_Empty", StringComparison.Ordinal), "The empty list bypassed scoped library defaults.");
            var explicitEmpty = Parameters(Array.Empty<Row>());
            explicitEmpty[nameof(ListView<Row>.Label)] = "Records";
            explicitEmpty[nameof(ListView<Row>.Caption)] = "Host caption";
            explicitEmpty[nameof(ListView<Row>.EmptyMessage)] = "No items.";
            var empty = await Render(scope, explicitEmpty);
            Require(empty.Contains("aria-label=\"Records\"", StringComparison.Ordinal) && empty.Contains("Host caption", StringComparison.Ordinal)
                && empty.Contains("No items.", StringComparison.Ordinal) && !empty.Contains("Flourish/Table_", StringComparison.Ordinal), "Explicit literals equal to original defaults were replaced by translations.");
            var explicitCulture = Parameters([new(1, "Record", 1234.5m, null, true)]);
            explicitCulture[nameof(ListView<Row>.Culture)] = CultureInfo.GetCultureInfo("en-US");
            var populated = await Render(scope, explicitCulture);
            Require(CellTexts(BodyRows(populated).Single())[1] == "1234.5", "The scoped format culture replaced an explicit host culture.");
        }));
    }

    private static Dictionary<string, object?> Parameters(IReadOnlyList<Row> rows, IReadOnlyList<TableColumn<Row>>? columns = null) => new()
    {
        [nameof(ListView<Row>.Items)] = rows,
        [nameof(ListView<Row>.Columns)] = columns ?? new TableColumn<Row>[] { new("name", "Name", row => row.Name), new("amount", "Amount", row => row.Amount) },
        [nameof(ListView<Row>.ItemKey)] = (Func<Row, object>)(row => row.Id)
    };

    private static ServiceProvider Services(bool translated = false)
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddFlourishFramework();
        if (translated) services.AddScoped<ITextProvider, TrackingTextProvider>();
        var provider = services.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true });
        Require(provider.GetService<IJSRuntime>() is null, "The static-list fixture must not accidentally provide a JavaScript runtime.");
        return provider;
    }

    private static async Task<string> Render(Dictionary<string, object?> parameters)
    {
        using var provider = Services();
        using var scope = provider.CreateScope();
        return await Render(scope, parameters);
    }

    private static async Task<string> Render(IServiceScope scope, Dictionary<string, object?> parameters)
    {
        await using var renderer = new HtmlRenderer(scope.ServiceProvider, scope.ServiceProvider.GetRequiredService<ILoggerFactory>());
        return await renderer.Dispatcher.InvokeAsync(async () => (await renderer.RenderComponentAsync<ListView<Row>>(ParameterView.FromDictionary(parameters))).ToHtmlString());
    }

    private static async Task Reject(Dictionary<string, object?> parameters, string? name)
    {
        try { await Render(parameters); }
        catch (ArgumentException error) when (name is null || error.ParamName == name) { return; }
        throw new InvalidOperationException("Invalid readonly list " + name + " was accepted.");
    }

    private static IReadOnlyList<string> BodyRows(string html) => Regex.Matches(Regex.Match(html, @"<tbody>[\s\S]*?</tbody>").Value, @"<tr\b[^>]*>[\s\S]*?</tr>").Select(match => match.Value).ToArray();
    private static IReadOnlyList<string> CellTexts(string html) => Regex.Matches(html, @"<(?:th|td)\b[^>]*>([\s\S]*?)</(?:th|td)>").Select(match => WebUtility.HtmlDecode(Regex.Replace(match.Groups[1].Value, "<[^>]*>", string.Empty))).ToArray();
    private static string Attribute(string tag, string name)
    {
        var match = Regex.Match(tag, $"(?:^|\\s){Regex.Escape(name)}=\"([^\"]*)\"");
        return match.Success ? WebUtility.HtmlDecode(match.Groups[1].Value) : string.Empty;
    }
    private static void Require(bool condition, string message) { if (!condition) throw new InvalidOperationException(message); }
    private sealed record Row(int Id, string? Name, decimal? Amount, DateTime? Date, bool Available);
}
