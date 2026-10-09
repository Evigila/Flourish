using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Reflection;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using ArkheideSystem.Flourish.WPF.Abstract;
using F = ArkheideSystem.Flourish.WPF.Controls;

namespace ArkheideSystem.Gallery.Flourish.WPF;

/// <summary>Executable samples use the production entry directly and keep business state in the host.</summary>
public static partial class Samples
{
    public static object Create(Type componentType, Window owner, ITextProvider texts)
    {
        ArgumentNullException.ThrowIfNull(componentType);
        ArgumentNullException.ThrowIfNull(owner);
        ArgumentNullException.ThrowIfNull(texts);
        return componentType.Name switch
        {
            "Button" => Buttons(),
            "UniformGridButton" => GridButtons(),
            "SplitButton" => SplitActions(),
            "ActionMenu" => ActionMenu(),
            "TextBox" => Labeled(new F.TextBox { Placeholder = "Name", Text = "Ada Lovelace" }, "Name"),
            "NumberBox" => Labeled(new F.NumberBox { Value = 12.5, Minimum = 0, Maximum = 100, FormatCulture = texts.FormatCulture }, "Quantity (0–100)"),
            "DateBox" => Labeled(new F.DateBox { SelectedDate = new DateTime(2026, 10, 7) }, "Date"),
            "CheckBox" => new F.CheckBox { Content = "Send a copy", IsChecked = true },
            "ToggleSwitch" => new F.ToggleSwitch { Content = "Enable reminders", IsChecked = true },
            "SelectBox" => Labeled(new F.SelectBox { Options = Choices, SelectedValue = "design" }, "Department"),
            "ReferenceDropdown" => References(),
            "SearchBox" => Search(),
            "SearchAutocomplete" => Autocomplete(),
            "MaskedInput" => MaskedCode(),
            "MultiSelectBox" => MultipleSelection(),
            "DataTable" => Records(texts),
            "DataSearch" => ColumnSearch(texts),
            "DataPager" => Paging(texts),
            "ListView" => new F.ListView { Columns = Columns, ItemsSource = RecordsSource(), Label = "Read-only records" },
            "EditingGrid" => Spreadsheet(),
            "LineChart" => Chart(texts),
            "Dialog" => Dialogs(owner),
            "ApplicationShell" => Shell(),
            _ => ContentSample(componentType, owner, texts)
        };
    }

    private static IReadOnlyList<SelectOption> Choices { get; } =
        [new("design", "Design"), new("engineering", "Engineering"), new("archived", "Archived", Disabled: true)];
    private static IReadOnlyList<TableColumn> Columns { get; } =
    [
        new("id", "ID", nameof(SampleRecord.Id), Width: 90, CardField: TableCardField.Identifier),
        new("name", "Name", nameof(SampleRecord.Name), Width: 240, CardField: TableCardField.Title),
        new("department", "Department", nameof(SampleRecord.Department)),
        new("amount", "Amount", nameof(SampleRecord.Amount), Width: 140, Format: "N2", CardField: TableCardField.Metric)
    ];
    private static ObservableCollection<SampleRecord> RecordsSource() => new(Enumerable.Range(1, 24)
        .Select(index => new SampleRecord { Id = index, Name = "Record " + index, Department = index % 2 == 0 ? "Design" : "Engineering", Amount = index * 12.5 }));

    private static F.FormLayout Group(params UIElement[] children)
    {
        var layout = new F.FormLayout { Columns = 1 };
        foreach (var child in children) layout.Children.Add(child);
        return layout;
    }
    private static F.Field Labeled(UIElement input, string label) => new() { Label = label, Content = input };
    private static F.Notice Status(string text = "Ready") => new() { Title = text };
    private static F.Button Action(string text, Action action, ButtonVariant variant = ButtonVariant.Secondary)
    {
        var button = new F.Button { Content = text, Variant = variant };
        button.Click += (_, _) => action();
        return button;
    }

    private static object Buttons()
    {
        var status = Status();
        var actions = new F.InlineActions();
        foreach (var variant in Enum.GetValues<ButtonVariant>())
            actions.Children.Add(Action(variant.ToString(), () => status.Title = variant + " clicked", variant));
        var busy = new F.Button { Content = "Run operation", BusyLabel = "Processing…" };
        busy.Click += async (_, _) =>
        {
            busy.Busy = true;
            status.Title = "Processing…";
            await Task.Delay(700);
            busy.Busy = false;
            status.Title = "Operation finished";
        };
        var structured = new F.Button { Text = "Open record", Description = "Record details", TrailingText = "24", Variant = ButtonVariant.Elevated };
        structured.Click += (_, _) => status.Title = "Structured button clicked";
        return Group(actions, busy, structured, new F.Button { Content = "Disabled", Disabled = true }, status);
    }
    private static object GridButtons()
    {
        var status = Status();
        var variants = new F.FormLayout { Columns = 2 };
        foreach (var variant in Enum.GetValues<UniformGridVariant>())
        {
            var value = variant;
            var grid = new F.UniformGrid { Shape = UniformGridShape.Rectangle, Variant = value, CellHeight = 100 };
            var button = new F.UniformGridButton { Text = value.ToString() };
            button.Click += (_, _) => status.Title = value + " selected";
            grid.Children.Add(button);
            variants.Children.Add(grid);
        }
        return Group(variants, status);
    }
    private static object SplitActions()
    {
        var status = Status();
        var split = new F.SplitButton { Text = "Save", Actions = [new("Save a copy", () => { status.Title = "Copy saved"; return Task.CompletedTask; }), new("Unavailable action", () => Task.CompletedTask, Disabled: true)] };
        split.Click += (_, _) => status.Title = "Saved";
        var lockPrimary = new F.ToggleSwitch { Content = "Disable main action" };
        lockPrimary.Click += (_, _) => split.PrimaryDisabled = lockPrimary.IsChecked == true;
        return Group(split, lockPrimary, status);
    }
    private static object ActionMenu()
    {
        var status = Status();
        var menu = new F.ActionMenu { Text = "Actions", Actions = [new("Open", () => { status.Title = "Opened"; return Task.CompletedTask; }), new("Remove", () => { status.Title = "Remove requested"; return Task.CompletedTask; }, Destructive: true), new("Unavailable", () => Task.CompletedTask, Disabled: true)] };
        return Group(menu, status);
    }
    private static object Search()
    {
        var status = Status("Type a query; Enter searches immediately; Escape clears");
        var search = new F.SearchBox { Placeholder = "Search records", DebounceMilliseconds = 350 };
        search.SearchRequested += (_, query) => status.Title = "Query: " + query;
        return Group(search, status);
    }
    private static object Autocomplete()
    {
        var status = Status();
        var search = new F.SearchAutocomplete
        {
            Search = async (query, cancellation) =>
            {
                await Task.Delay(150, cancellation);
                return Choices.Where(option => option.Text.Contains(query, StringComparison.OrdinalIgnoreCase));
            }
        };
        search.SelectionChanged += (_, _) => status.Title = "Selected: " + search.SelectedValue;
        return Group(Labeled(search, "Search suggestions"), status);
    }
    private static object References()
    {
        var references = new[] { new ReferenceItem(1, "Design"), new ReferenceItem(2, "Engineering") };
        var selector = new F.ReferenceDropdown { References = references, SelectedValue = 1 };
        var status = Status("Type a new reference and press Enter to request creation");
        selector.SelectionChanged += (_, _) => status.Title = "Reference ID: " + selector.SelectedValue;
        selector.CreateRequested += (_, name) =>
        {
            var added = new ReferenceItem(references.Length + 1, name);
            references = [.. references, added];
            selector.References = references;
            selector.SelectedValue = added.Id;
            status.Title = "Created: " + name;
        };
        return Group(Labeled(selector, "Reference"), status);
    }
    private static object MultipleSelection()
    {
        var options = new[] { new MultiSelectOption("reading", "Reading", true, CanDeselect: false, CanReorder: false), new("photography", "Photography"), new("discussion", "Discussion"), new("outdoors", "Outdoor exploration"), new("unavailable", "Unavailable", Disabled: true) };
        var selector = new F.MultiSelectBox { Options = options, MinimumSelected = 1, MaximumSelected = 3, ReorderEnabled = true, Searchable = true };
        var status = Status("Search activity tags or create a new tag; Reading is required; choose at most three.");
        selector.Changed += (_, change) =>
        {
            options = change.OrderedKeys.Select(key => options.Single(item => item.Key == key) with { Selected = change.SelectedKeys.Contains(key) }).ToArray();
            selector.Options = options;
            status.Title = "Selected: " + string.Join(", ", options.Where(option => option.Selected).Select(option => option.Label));
        };
        selector.CreateRequested = async label =>
        {
            await Task.Delay(120);
            if (options.Any(item => string.Equals(item.Label.Trim(), label.Trim(), StringComparison.OrdinalIgnoreCase))) return false;
            options = [.. options, new(Guid.NewGuid().ToString("N"), label, Selected: true)];
            selector.Options = options;
            status.Title = "Created activity tag: " + label;
            return true;
        };
        return Group(Labeled(selector, "Activity tags"), status);
    }
    private static object MaskedCode()
    {
        var numeric = new F.MaskedInput { Mask = "000-000", Value = "123456", MaxLength = 7 };
        var alphanumeric = new F.MaskedInput { Mask = "AA-000", Value = "AB123", MaxLength = 6 };
        var status = Status();
        void Update() => status.Title = $"Raw: {numeric.Value} / {alphanumeric.Value}; display: {numeric.Text} / {alphanumeric.Text}; complete: {numeric.IsComplete} / {alphanumeric.IsComplete}";
        numeric.TextChanged += (_, _) => numeric.Dispatcher.InvokeAsync(Update);
        alphanumeric.TextChanged += (_, _) => alphanumeric.Dispatcher.InvokeAsync(Update);
        Update();
        return Group(Labeled(numeric, "Code (000-000)"), Labeled(alphanumeric, "Reference (AA-000)"), status);
    }
    private static object Records(ITextProvider? texts = null)
    {
        var status = Status();
        var records = RecordsSource();
        F.DataTable? table = null;
        table = new F.DataTable
        {
            Columns = Columns, ItemsSource = records, Height = 460, PageSize = 8, TextProvider = texts,
            Actions = new[] { new RowAction("Increase", row => { ((SampleRecord)row).Amount += 10; table?.Refresh(); return Task.CompletedTask; }) }
        };
        table.RowOpened += (_, row) => status.Title = "Opened " + ((SampleRecord)row).Name;
        table.SearchChanged += (_, query) => status.Title = "Search: " + query.Value;
        table.PageChanged += (_, page) => status.Title = "Page " + page;
        var remote = new F.ToggleSwitch { Content = "Remote search demonstration" };
        remote.Click += (_, _) =>
        {
            table.SearchMode = remote.IsChecked == true ? TableSearchMode.Remote : TableSearchMode.Local;
            table.ItemsSource = records;
        };
        table.SearchChanged += (_, query) =>
        {
            if (table.SearchMode == TableSearchMode.Remote)
                table.ItemsSource = new ObservableCollection<SampleRecord>(records.Where(record =>
                    (query.ColumnKey switch
                    {
                        "id" => record.Id.ToString(texts?.FormatCulture),
                        "department" => record.Department,
                        "amount" => record.Amount.ToString("N2", texts?.FormatCulture),
                        _ => record.Name
                    }).Contains(query.Value, StringComparison.OrdinalIgnoreCase)));
        };
        return Group(remote, table, status);
    }
    private static object ColumnSearch(ITextProvider texts)
    {
        var status = Status();
        var search = new F.DataSearch { Columns = Columns, IncludeAllColumns = true, TextProvider = texts };
        search.Changed += (_, request) => status.Title = (request.ColumnKey ?? "All columns") + ": " + request.Value;
        return Group(search, status);
    }
    private static object Paging(ITextProvider texts)
    {
        var status = Status();
        var pager = new F.DataPager { TotalCount = 124, PageSize = 10, TextProvider = texts };
        pager.PageChanged += (_, page) => status.Title = "Page " + page;
        return Group(pager, status);
    }

    private static object ContentSample(Type type, Window owner, ITextProvider texts) => SimpleSamples.Create(type, owner, texts);

    public sealed class SampleRecord : INotifyPropertyChanged
    {
        public int Id { get; init; }
        public string Name { get; set; } = "";
        public string Department { get; set; } = "";
        private double amount;
        public double Amount { get => amount; set { amount = value; PropertyChanged?.Invoke(this, new(nameof(Amount))); } }
        public event PropertyChangedEventHandler? PropertyChanged;
    }
}
