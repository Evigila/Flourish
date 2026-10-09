using System.Collections;
using System.Collections.Specialized;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using ArkheideSystem.Flourish.WPF.Abstract;

namespace ArkheideSystem.Flourish.WPF.Controls;

/// <summary>The sole interactive record entry. Consumers own remote data, authorization and edit actions.</summary>
public sealed class DataTable : UserControl
{
    private readonly DataSearch search = new() { Margin = new Thickness(0, 0, 0, 14) };
    private readonly DataPager pager = new();
    private readonly MultiSelectBox display = new() { ReorderEnabled = true, Text = "Display" };
    private readonly DataGrid grid = DataPresentation.Grid();
    private readonly WrapPanel cards = new();
    private readonly Border frame;
    private readonly TextBlock status = DataPresentation.Text(string.Empty, true);
    private readonly ContentControl bulkActions = new();
    private readonly CheckBox selectAll = new() { IsThreeState = true };
    private readonly Grid bulkValues = new() { Margin = new Thickness(0, 8, 0, 8) };
    private readonly StackPanel bulkLayout = new();
    private IReadOnlyList<TableColumn> renderedBulkColumns = [];
    private Func<TableColumn, object?>? renderedBulkTemplate;
    private readonly Button tableButton = new() { Content = "List", Variant = ButtonVariant.Secondary, Margin = new Thickness(10, 0, 0, 0) };
    private readonly Button cardsButton = new() { Content = "Cards", Variant = ButtonVariant.Quiet, Margin = new Thickness(4, 0, 10, 0) };
    private readonly List<string> columnOrder = [];
    private readonly HashSet<string> knownColumns = new(StringComparer.Ordinal);
    private readonly HashSet<string> hiddenColumns = new(StringComparer.Ordinal);
    private IReadOnlyList<TableColumn> columns = [];
    private IReadOnlyList<object> rows = [];
    private readonly ObservableCollection<object> renderedRows = [];
    private readonly Dictionary<string, double> manualWidths = new(StringComparer.Ordinal);
    private readonly List<(DataGridColumn Column, EventHandler Changed)> widthSubscriptions = [];
    private readonly HashSet<object> editSelection = new(ReferenceEqualityComparer.Instance);
    private readonly Dictionary<object, List<WeakReference<ActionMenu>>> actionMenus = new(ReferenceEqualityComparer.Instance);
    private IReadOnlyList<TableColumn> renderedColumns = [];
    private DataTemplate? renderedTemplate;
    private bool renderedActions;
    private bool renderedSorting;
    private CultureInfo? renderedCulture;
    private readonly DataText text;
    private string? sortKey;
    private TableSortDirection sortDirection;
    private string? restoredPreference;
    private bool updating;

    public DataTable()
    {
        text = new DataText(this, Refresh);
        AutomationProperties.SetName(display, "Display");
        frame = DataPresentation.Frame(grid);
        frame.Margin = new Thickness(0, 14, 0, 0);
        var toolbar = new DockPanel();
        var choices = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right };
        choices.Children.Add(tableButton); choices.Children.Add(cardsButton); choices.Children.Add(display);
        DockPanel.SetDock(choices, Dock.Right); toolbar.Children.Add(choices); toolbar.Children.Add(pager);
        var layout = new Grid();
        layout.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        layout.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        layout.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        layout.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        layout.RowDefinitions.Add(new RowDefinition());
        bulkLayout.Children.Add(selectAll); bulkLayout.Children.Add(bulkActions); bulkLayout.Children.Add(bulkValues);
        selectAll.Click += (_, _) => SelectAllLoadedForEditing(selectAll.IsChecked == true);
        Grid.SetRow(toolbar, 1); Grid.SetRow(bulkLayout, 2); Grid.SetRow(status, 3); Grid.SetRow(frame, 4);
        layout.Children.Add(search); layout.Children.Add(toolbar); layout.Children.Add(bulkLayout); layout.Children.Add(status); layout.Children.Add(frame);
        Content = layout;
        SetResourceReference(FontFamilyProperty, "Flourish.FontFamily"); FontSize = 17;
        SetResourceReference(ForegroundProperty, "Flourish.Brush.Text");
        search.Changed += (_, request) =>
        {
            updating = true; SetCurrentValue(SearchQueryProperty, request.Value); SetCurrentValue(SearchColumnKeyProperty, request.ColumnKey);
            SetCurrentValue(CurrentPageProperty, 1); updating = false; Refresh(); SearchChanged?.Invoke(this, request);
        };
        pager.PageChanged += (_, page) => { SetCurrentValue(CurrentPageProperty, page); PageChanged?.Invoke(this, page); };
        display.Changed += (_, change) => ApplyDisplay(change);
        tableButton.Click += (_, _) => SetCurrentValue(ViewProperty, TableView.Table);
        cardsButton.Click += (_, _) => SetCurrentValue(ViewProperty, TableView.Cards);
        grid.Sorting += OnSorting;
        grid.ItemsSource = renderedRows;
        grid.MouseDoubleClick += (_, args) =>
        {
            if (args.OriginalSource is DependencyObject source && FindAncestor<System.Windows.Controls.Primitives.ButtonBase>(source) is null
                && FindAncestor<DataGridRow>(source) is { Item: var item }) Open(item);
        };
        grid.KeyDown += (_, args) => { if (args.Key == Key.Enter && grid.SelectedItem is { } item && !BulkEditing) { Open(item); args.Handled = true; } };
        grid.SelectionChanged += (_, args) =>
        {
            if (!BulkEditing || updating) return;
            foreach (var item in args.RemovedItems) editSelection.Remove(item);
            foreach (var item in args.AddedItems) editSelection.Add(item);
            SetCurrentValue(EditSelectionProperty, editSelection.ToArray());
            EditSelectionChanged?.Invoke(this, editSelection.ToArray());
        };
        Loaded += (_, _) => Refresh();
    }

    public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register(nameof(ItemsSource), typeof(IEnumerable), typeof(DataTable), new PropertyMetadata(null, SourceChanged));
    public IEnumerable? ItemsSource { get => (IEnumerable?)GetValue(ItemsSourceProperty); set => SetValue(ItemsSourceProperty, value); }
    public static readonly DependencyProperty ColumnsProperty = DependencyProperty.Register(nameof(Columns), typeof(IEnumerable), typeof(DataTable), new PropertyMetadata(null, SourceChanged));
    public IEnumerable? Columns { get => (IEnumerable?)GetValue(ColumnsProperty); set => SetValue(ColumnsProperty, value); }
    public static readonly DependencyProperty ActionsProperty = DependencyProperty.Register(nameof(Actions), typeof(IEnumerable), typeof(DataTable), new PropertyMetadata(null, SourceChanged));
    public IEnumerable? Actions { get => (IEnumerable?)GetValue(ActionsProperty); set => SetValue(ActionsProperty, value); }
    public static readonly DependencyProperty SearchableProperty = Property(nameof(Searchable), true);
    public bool Searchable { get => (bool)GetValue(SearchableProperty); set => SetValue(SearchableProperty, value); }
    public static readonly DependencyProperty SearchDisabledProperty = Property(nameof(SearchDisabled), false);
    public bool SearchDisabled { get => (bool)GetValue(SearchDisabledProperty); set => SetValue(SearchDisabledProperty, value); }
    public static readonly DependencyProperty SearchModeProperty = Property(nameof(SearchMode), TableSearchMode.Local);
    public TableSearchMode SearchMode { get => (TableSearchMode)GetValue(SearchModeProperty); set => SetValue(SearchModeProperty, value); }
    public static readonly DependencyProperty SearchQueryProperty = Property(nameof(SearchQuery), string.Empty);
    public string SearchQuery { get => (string)GetValue(SearchQueryProperty); set => SetValue(SearchQueryProperty, value); }
    public static readonly DependencyProperty SearchColumnKeyProperty = DependencyProperty.Register(nameof(SearchColumnKey), typeof(string), typeof(DataTable), new PropertyMetadata(null, Refresh));
    public string? SearchColumnKey { get => (string?)GetValue(SearchColumnKeyProperty); set => SetValue(SearchColumnKeyProperty, value); }
    public static readonly DependencyProperty PageSizeProperty = DependencyProperty.Register(nameof(PageSize), typeof(int), typeof(DataTable), new PropertyMetadata(10, Refresh), value => (int)value > 0);
    public int PageSize { get => (int)GetValue(PageSizeProperty); set => SetValue(PageSizeProperty, value); }
    public static readonly DependencyProperty CurrentPageProperty = Property(nameof(CurrentPage), 1);
    public int CurrentPage { get => (int)GetValue(CurrentPageProperty); set => SetValue(CurrentPageProperty, value); }
    public static readonly DependencyProperty ViewProperty = Property(nameof(View), TableView.Table);
    public TableView View { get => (TableView)GetValue(ViewProperty); set => SetValue(ViewProperty, value); }
    public static readonly DependencyProperty PurposeProperty = Property(nameof(Purpose), TablePurpose.Registry);
    public TablePurpose Purpose { get => (TablePurpose)GetValue(PurposeProperty); set => SetValue(PurposeProperty, value); }
    public static readonly DependencyProperty LoadingProperty = Property(nameof(Loading), false);
    public bool Loading { get => (bool)GetValue(LoadingProperty); set => SetValue(LoadingProperty, value); }
    public static readonly DependencyProperty ErrorProperty = DependencyProperty.Register(nameof(Error), typeof(string), typeof(DataTable), new PropertyMetadata(null, Refresh));
    public string? Error { get => (string?)GetValue(ErrorProperty); set => SetValue(ErrorProperty, value); }
    public static readonly DependencyProperty EmptyMessageProperty = Property(nameof(EmptyMessage), "No items.");
    public string EmptyMessage { get => (string)GetValue(EmptyMessageProperty); set => SetValue(EmptyMessageProperty, value); }
    public static readonly DependencyProperty LabelProperty = Property(nameof(Label), "Records");
    public string Label { get => (string)GetValue(LabelProperty); set => SetValue(LabelProperty, value); }
    public static readonly DependencyProperty IsLimitedProperty = Property(nameof(IsLimited), false);
    public bool IsLimited { get => (bool)GetValue(IsLimitedProperty); set => SetValue(IsLimitedProperty, value); }
    public static readonly DependencyProperty BulkEditingProperty = Property(nameof(BulkEditing), false);
    public bool BulkEditing { get => (bool)GetValue(BulkEditingProperty); set => SetValue(BulkEditingProperty, value); }
    public static readonly DependencyProperty EditingBusyProperty = Property(nameof(EditingBusy), false);
    public bool EditingBusy { get => (bool)GetValue(EditingBusyProperty); set => SetValue(EditingBusyProperty, value); }
    public static readonly DependencyProperty BulkEditActionsProperty = DependencyProperty.Register(nameof(BulkEditActions), typeof(object), typeof(DataTable), new PropertyMetadata(null, Refresh));
    public object? BulkEditActions { get => GetValue(BulkEditActionsProperty); set => SetValue(BulkEditActionsProperty, value); }
    public static readonly DependencyProperty EditSelectionProperty = DependencyProperty.Register(nameof(EditSelection), typeof(IEnumerable), typeof(DataTable), new PropertyMetadata(null, SelectionChanged));
    public IEnumerable? EditSelection { get => (IEnumerable?)GetValue(EditSelectionProperty); set => SetValue(EditSelectionProperty, value); }
    public static readonly DependencyProperty PreferenceKeyProperty = DependencyProperty.Register(nameof(PreferenceKey), typeof(string), typeof(DataTable), new PropertyMetadata(null, Refresh));
    public string? PreferenceKey { get => (string?)GetValue(PreferenceKeyProperty); set => SetValue(PreferenceKeyProperty, value); }
    public static readonly DependencyProperty PreferencesProperty = DependencyProperty.Register(nameof(Preferences), typeof(ITablePreferences), typeof(DataTable), new PropertyMetadata(null, (sender, args) =>
    { var table = (DataTable)sender; table.restoredPreference = null; table.Refresh(); }));
    public ITablePreferences? Preferences { get => (ITablePreferences?)GetValue(PreferencesProperty); set => SetValue(PreferencesProperty, value); }
    /// <summary>Native context exposes Item, Column and Display. The template visual tree is retained across context updates.</summary>
    public static readonly DependencyProperty CellTemplateProperty = DependencyProperty.Register(nameof(CellTemplate), typeof(DataTemplate), typeof(DataTable), new PropertyMetadata(null, Refresh));
    public DataTemplate? CellTemplate { get => (DataTemplate?)GetValue(CellTemplateProperty); set => SetValue(CellTemplateProperty, value); }
    /// <summary>Consumer-supplied ordinary Flourish controls for values to apply to selected records.</summary>
    public Func<TableColumn, object?>? BulkEditCell { get; set; }
    public Func<object, bool>? RowOpenAvailable { get; set; }
    public Func<object, string>? RowLabel { get; set; }
    public event EventHandler<TableSearchRequest>? SearchChanged;
    public event EventHandler<int>? PageChanged;
    public event EventHandler<object>? RowOpened;
    public event EventHandler<IReadOnlyList<object>>? EditSelectionChanged;
    public IReadOnlyList<object> VisibleItems => rows;
    public string? SortKey => sortKey;
    public TableSortDirection SortDirection => sortDirection;
    public IReadOnlyDictionary<string, double> ColumnWidths => new ReadOnlyDictionary<string, double>(manualWidths);
    public event EventHandler<MultiSelectChange>? DisplayChanged;
    public static readonly DependencyProperty TextProviderProperty = DependencyProperty.Register(nameof(TextProvider), typeof(ITextProvider), typeof(DataTable), new PropertyMetadata(null, (sender, args) => ((DataTable)sender).text.Provider = (ITextProvider?)args.NewValue));
    public ITextProvider? TextProvider { get => (ITextProvider?)GetValue(TextProviderProperty); set => SetValue(TextProviderProperty, value); }
    public CultureInfo? Culture { get; set; }
    private CultureInfo FormatCulture => Culture ?? text.Culture;

    public void Refresh()
    {
        if (updating) return;
        updating = true;
        try
        {
            columns = Columns?.Cast<TableColumn>().ToArray() ?? [];
            TableData.ValidateColumns(columns);
            var label = Label == "Records" ? text.Get("Table_Label", "Records") : Label;
            AutomationProperties.SetName(this, label); AutomationProperties.SetName(grid, label); AutomationProperties.SetName(cards, label);
            SynchronizeColumns();
            RestoreSort();
            search.Columns = columns; search.IncludeAllColumns = SearchMode == TableSearchMode.Local;
            search.TextProvider = TextProvider; pager.TextProvider = TextProvider;
            search.Value = SearchQuery; search.ColumnKey = SearchColumnKey;
            search.Visibility = Searchable ? Visibility.Visible : Visibility.Collapsed;
            search.IsEnabled = !SearchDisabled && !BulkEditing && !EditingBusy;
            var source = ItemsSource?.Cast<object>().ToArray() ?? [];
            var processed = SearchMode == TableSearchMode.Local ? TableData.Filter(source, columns, SearchQuery, SearchColumnKey, FormatCulture) : source;
            processed = TableData.Sort(processed, columns.FirstOrDefault(item => item.Key == sortKey), sortDirection, FormatCulture);
            var page = TableData.ClampPage(CurrentPage, processed.Count, PageSize);
            SetCurrentValue(CurrentPageProperty, page);
            rows = processed.Skip((page - 1) * PageSize).Take(PageSize).ToArray();
            pager.PageSize = PageSize; pager.TotalCount = processed.Count; pager.CurrentPage = page; pager.IsLimited = IsLimited;
            display.MinimumSelected = columns.Count > 0 ? 1 : 0;
            display.Options = columnOrder.Select(key => columns.Single(item => item.Key == key)).Select(item => new MultiSelectOption(item.Key, item.Label, !hiddenColumns.Contains(item.Key),
                CanDeselect: item.CanHide && (columns.Count - hiddenColumns.Count > 1 || hiddenColumns.Contains(item.Key)), CanReorder: item.CanReorder)).ToArray();
            var registry = Purpose == TablePurpose.Registry;
            tableButton.Visibility = cardsButton.Visibility = display.Visibility = registry ? Visibility.Visible : Visibility.Collapsed;
            display.IsEnabled = tableButton.IsEnabled = cardsButton.IsEnabled = !BulkEditing && !EditingBusy;
            display.ReorderEnabled = View == TableView.Table;
            tableButton.Variant = View == TableView.Table ? ButtonVariant.Secondary : ButtonVariant.Quiet;
            cardsButton.Variant = View == TableView.Cards ? ButtonVariant.Secondary : ButtonVariant.Quiet;
            tableButton.Content = text.Get("Table_List", "List"); cardsButton.Content = text.Get("Table_Cards", "Cards");
            display.Text = text.Get("Table_Display", "Display");
            AutomationProperties.SetName(display, display.Text);
            grid.SelectionMode = BulkEditing ? DataGridSelectionMode.Extended : DataGridSelectionMode.Single;
            grid.IsEnabled = !EditingBusy; bulkActions.Content = BulkEditing ? BulkEditActions : null;
            bulkLayout.Visibility = BulkEditing ? Visibility.Visible : Visibility.Collapsed;
            selectAll.Content = text.Get("Table_SelectAllLoaded", "Select all loaded records"); selectAll.IsEnabled = !EditingBusy && source.Length > 0;
            var selectedCount = source.Count(item => editSelection.Contains(item));
            selectAll.IsChecked = selectedCount == 0 ? false : selectedCount == source.Length ? true : null;
            RenderBulkValues();
            RefreshActionMenus();
            status.Visibility = Loading || !string.IsNullOrWhiteSpace(Error) ? Visibility.Visible : Visibility.Collapsed;
            status.Text = !string.IsNullOrWhiteSpace(Error) ? Error : Loading ? text.Get("Table_Loading", "Loading…") : string.Empty;
            status.Margin = new Thickness(24, 20, 24, 20);
            status.SetResourceReference(TextBlock.ForegroundProperty, string.IsNullOrWhiteSpace(Error) ? "Flourish.Brush.Muted" : "Flourish.Brush.Danger");
            AutomationProperties.SetLiveSetting(status, string.IsNullOrWhiteSpace(Error) ? AutomationLiveSetting.Polite : AutomationLiveSetting.Assertive);
            if (rows.Count == 0)
            {
                frame.Child = new TextBlock { Text = Loading || !string.IsNullOrWhiteSpace(Error) ? string.Empty : EmptyMessage == "No items." ? text.Get("Table_Empty", "No items.") : EmptyMessage, Margin = new Thickness(24, 20, 24, 20) };
            }
            else if (View == TableView.Cards && registry && !BulkEditing) RenderCards();
            else RenderGrid();
        }
        finally { updating = false; }
    }

    private void SynchronizeColumns()
    {
        var keys = columns.Select(item => item.Key).ToHashSet(StringComparer.Ordinal);
        knownColumns.IntersectWith(keys); hiddenColumns.IntersectWith(keys); columnOrder.RemoveAll(key => !keys.Contains(key));
        foreach (var key in manualWidths.Keys.Where(key => !keys.Contains(key)).ToArray()) manualWidths.Remove(key);
        foreach (var column in columns)
        {
            if (!columnOrder.Contains(column.Key, StringComparer.Ordinal)) columnOrder.Add(column.Key);
            if (knownColumns.Add(column.Key) && !column.DefaultVisible && column.CanHide) hiddenColumns.Add(column.Key);
            if (!column.CanHide) hiddenColumns.Remove(column.Key);
        }
        if (columns.Count > 0 && hiddenColumns.Count == columns.Count) hiddenColumns.Remove(columnOrder[0]);
        if (sortKey is not null && columns.All(item => item.Key != sortKey || !item.Sortable)) { sortKey = null; sortDirection = TableSortDirection.Default; }
    }

    private IReadOnlyList<TableColumn> VisibleColumns => columnOrder.Where(key => !hiddenColumns.Contains(key)).Select(key => columns.Single(item => item.Key == key)).ToArray();

    private void RenderGrid()
    {
        var visible = VisibleColumns;
        var hasActions = !BulkEditing && (Actions?.Cast<RowAction>().Any() == true || RowOpened is not null);
        var sorting = Purpose == TablePurpose.Registry && !BulkEditing && !EditingBusy;
        if (renderedColumns.SequenceEqual(visible) && ReferenceEquals(renderedTemplate, CellTemplate) && renderedActions == hasActions && renderedSorting == sorting && Equals(renderedCulture, FormatCulture))
        {
            foreach (var native in grid.Columns)
            {
                native.SortDirection = sortKey != native.SortMemberPath ? null : sortDirection == TableSortDirection.Descending ? ListSortDirection.Descending : ListSortDirection.Ascending;
                if (columns.FirstOrDefault(column => column.Key == native.SortMemberPath) is { } definition) native.Header = Header(definition);
            }
            ReconcileRows();
            DataPresentation.RefreshCells(grid);
            frame.Child = grid;
            return;
        }
        DetachColumnWidths();
        grid.Columns.Clear();
        foreach (var column in visible)
        {
            var native = DataPresentation.Column(column, FormatCulture, CellTemplate);
            native.Header = Header(column);
            native.CanUserSort = column.Sortable && Purpose == TablePurpose.Registry && !BulkEditing && !EditingBusy;
            if (manualWidths.TryGetValue(column.Key, out var width)) native.Width = width;
            native.SortDirection = sortKey != column.Key ? null : sortDirection == TableSortDirection.Descending ? ListSortDirection.Descending : ListSortDirection.Ascending;
            grid.Columns.Add(native);
            EventHandler changed = (_, _) => { if (!updating && native.Width.IsAbsolute) manualWidths[column.Key] = native.Width.Value; };
            DependencyPropertyDescriptor.FromProperty(DataGridColumn.WidthProperty, typeof(DataGridColumn)).AddValueChanged(native, changed);
            widthSubscriptions.Add((native, changed));
        }
        if (hasActions)
        {
            var factory = new FrameworkElementFactory(typeof(ContentControl));
            factory.SetBinding(ContentControl.ContentProperty, new Binding { Converter = new ActionsConverter(this) });
            grid.Columns.Add(new DataGridTemplateColumn { Header = text.Get("Table_Actions", "Actions"), SortMemberPath = string.Empty, Width = 80, CanUserSort = false, CellTemplate = new DataTemplate { VisualTree = factory } });
        }
        renderedColumns = visible; renderedTemplate = CellTemplate; renderedActions = hasActions; renderedSorting = sorting;
        renderedCulture = FormatCulture;
        ReconcileRows(); frame.Child = grid;
    }

    private void RenderCards()
    {
        cards.Children.Clear();
        foreach (var item in rows)
        {
            var content = new StackPanel();
            var visible = VisibleColumns;
            for (var index = 0; index < visible.Count; index++)
            {
                var column = visible[index];
                var value = TableData.Display(item, column, FormatCulture);
                var presenter = new ContentControl { Content = CellTemplate is null ? CardContent(item, column, value) : new TableCellContext(item, column, value), ContentTemplate = CellTemplate, Margin = new Thickness(0, 0, 0, 8), HorizontalContentAlignment = HorizontalAlignment.Stretch };
                if (index == 0 || column.CardField == TableCardField.Title) presenter.FontWeight = FontWeights.Bold;
                content.Children.Add(presenter);
            }
            content.Children.Add(ActionsFor(item));
            var card = DataPresentation.Frame(content); card.Padding = new Thickness(24); card.Margin = new Thickness(0, 0, 20, 20); card.Width = 300;
            card.MouseLeftButtonDown += (_, args) =>
            {
                if (args.ClickCount == 2 && args.OriginalSource is DependencyObject source && FindAncestor<System.Windows.Controls.Primitives.ButtonBase>(source) is null) Open(item);
            };
            cards.Children.Add(card);
        }
        var scroll = new ScrollViewer { Content = cards, HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled, VerticalScrollBarVisibility = ScrollBarVisibility.Auto };
        scroll.SetResourceReference(StyleProperty, "Flourish.ScrollViewer");
        frame.Child = scroll;
    }

    private UIElement CardContent(object item, TableColumn column, string display)
    {
        if (column.CardField == TableCardField.Image)
        {
            var source = TableData.Value(item, column) as ImageSource;
            if (source is null && !string.IsNullOrWhiteSpace(display))
            {
                try { source = new ImageSourceConverter().ConvertFromInvariantString(display) as ImageSource; }
                catch (Exception exception) when (exception is NotSupportedException or FormatException or System.IO.IOException) { }
            }
            if (source is not null)
            {
                var image = new Image { Source = source, Height = 24, Stretch = Stretch.Uniform, HorizontalAlignment = HorizontalAlignment.Left };
                AutomationProperties.SetName(image, column.Label); return image;
            }
            return DataPresentation.Text(text.Get("Table_NoImage", "No image"), true);
        }
        var textBlock = DataPresentation.Text(display);
        textBlock.TextWrapping = TextWrapping.NoWrap; textBlock.TextTrimming = TextTrimming.CharacterEllipsis;
        textBlock.Height = 24; textBlock.ToolTip = display;
        return textBlock;
    }

    private void RenderBulkValues()
    {
        if (!BulkEditing || BulkEditCell is null) return;
        var visible = VisibleColumns;
        if (renderedBulkColumns.SequenceEqual(visible) && ReferenceEquals(renderedBulkTemplate, BulkEditCell))
        {
            for (var index = 0; index < visible.Count; index++)
                bulkValues.ColumnDefinitions[index].Width = new GridLength(manualWidths.GetValueOrDefault(visible[index].Key, visible[index].Width));
            return;
        }
        bulkValues.Children.Clear(); bulkValues.ColumnDefinitions.Clear();
        foreach (var column in visible)
        {
            bulkValues.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(manualWidths.GetValueOrDefault(column.Key, column.Width)), MinWidth = 72 });
            var content = new ContentControl { Content = BulkEditCell(column), Margin = new Thickness(8) };
            Grid.SetColumn(content, bulkValues.ColumnDefinitions.Count - 1); bulkValues.Children.Add(content);
        }
        renderedBulkColumns = visible; renderedBulkTemplate = BulkEditCell;
    }

    private UIElement ActionsFor(object item)
    {
        var menu = new ActionMenu { Text = "⋮", Variant = ButtonVariant.Quiet, Actions = BuildActions(item), Margin = new Thickness(8), IsEnabled = !EditingBusy };
        AutomationProperties.SetName(menu, $"{text.Get("Table_Actions", "Actions")}: {RecordLabel(item)}");
        if (!actionMenus.TryGetValue(item, out var instances)) actionMenus[item] = instances = [];
        instances.Add(new WeakReference<ActionMenu>(menu));
        return menu;
    }

    private IReadOnlyList<MenuAction> BuildActions(object item)
    {
        var actions = new List<MenuAction>();
        if (RowOpened is not null && RowOpenAvailable?.Invoke(item) != false)
            actions.Add(new MenuAction(text.Get("Table_Open", "Open"), () => { Open(item); return Task.CompletedTask; }, EditingBusy || BulkEditing));
        foreach (var action in (Actions?.Cast<RowAction>() ?? []).Where(action => action.IsAvailable?.Invoke(item) != false))
            actions.Add(new MenuAction(action.Text, () => action.OnClick(item), action.Disabled?.Invoke(item) == true || EditingBusy, action.Destructive));
        return actions;
    }

    private void RefreshActionMenus()
    {
        foreach (var pair in actionMenus.ToArray())
        {
            pair.Value.RemoveAll(reference => !reference.TryGetTarget(out _));
            if (pair.Value.Count == 0) { actionMenus.Remove(pair.Key); continue; }
            foreach (var reference in pair.Value)
                if (reference.TryGetTarget(out var menu))
                { menu.Actions = BuildActions(pair.Key); menu.IsEnabled = !EditingBusy; AutomationProperties.SetName(menu, $"{text.Get("Table_Actions", "Actions")}: {RecordLabel(pair.Key)}"); }
        }
    }

    private string RecordLabel(object item) => RowLabel?.Invoke(item)
        ?? (columns.FirstOrDefault() is { } column ? TableData.Display(item, column, FormatCulture) : text.Get("Table_RecordName", "record"));

    private void Open(object item)
    {
        if (!BulkEditing && !EditingBusy && !Loading && RowOpenAvailable?.Invoke(item) != false) RowOpened?.Invoke(this, item);
    }

    private void ApplyDisplay(MultiSelectChange change)
    {
        if (BulkEditing || EditingBusy) return;
        var keys = columns.Select(item => item.Key).ToHashSet(StringComparer.Ordinal);
        if (change.OrderedKeys.Count != keys.Count || change.OrderedKeys.Distinct(StringComparer.Ordinal).Count() != keys.Count
            || !keys.SetEquals(change.OrderedKeys) || !change.SelectedKeys.IsSubsetOf(keys) || (keys.Count > 0 && change.SelectedKeys.Count == 0))
            throw new ArgumentException("Display changes must contain every stable column key and at least one selected column.", nameof(change));
        if (columns.Any(column => !column.CanHide && !change.SelectedKeys.Contains(column.Key))
            || columns.Any(column => !column.CanReorder && columnOrder.IndexOf(column.Key) != change.OrderedKeys.ToList().IndexOf(column.Key)))
            throw new ArgumentException("Fixed columns must retain their selection and ordering position.", nameof(change));
        columnOrder.Clear(); columnOrder.AddRange(change.OrderedKeys);
        hiddenColumns.Clear(); hiddenColumns.UnionWith(keys.Except(change.SelectedKeys)); Refresh();
        DisplayChanged?.Invoke(this, change);
    }

    private void OnSorting(object? sender, DataGridSortingEventArgs args)
    {
        args.Handled = true;
        var column = columns.FirstOrDefault(item => item.Key == args.Column.SortMemberPath);
        if (column is null || !column.Sortable || Purpose != TablePurpose.Registry || BulkEditing || EditingBusy) return;
        sortDirection = sortKey != column.Key ? TableSortDirection.Descending : sortDirection switch
        {
            TableSortDirection.Descending => TableSortDirection.Ascending,
            TableSortDirection.Ascending => TableSortDirection.Default,
            _ => TableSortDirection.Descending
        };
        sortKey = sortDirection == TableSortDirection.Default ? null : column.Key;
        if (!string.IsNullOrWhiteSpace(PreferenceKey)) Preferences?.Set(PreferenceKey, sortKey is null ? null : new TableSortPreference(sortKey, sortDirection == TableSortDirection.Descending));
        SetCurrentValue(CurrentPageProperty, 1); Refresh();
    }

    private void RestoreSort()
    {
        if (columns.Count == 0) return;
        if (restoredPreference == PreferenceKey) return;
        restoredPreference = PreferenceKey; sortKey = null; sortDirection = TableSortDirection.Default;
        if (!string.IsNullOrWhiteSpace(PreferenceKey) && Preferences?.Get(PreferenceKey) is { } saved && columns.Any(item => item.Key == saved.SortKey && item.Sortable))
        { sortKey = saved.SortKey; sortDirection = saved.Descending ? TableSortDirection.Descending : TableSortDirection.Ascending; }
    }

    private object Header(TableColumn column)
    {
        var hint = sortKey != column.Key ? text.Get("Table_SortDescending", "sort descending")
            : sortDirection == TableSortDirection.Descending ? text.Get("Table_SortAscending", "sort ascending") : text.Get("Table_SortDefault", "restore default order");
        return new TextBlock { Text = column.Label, TextTrimming = TextTrimming.CharacterEllipsis,
            ToolTip = column.Sortable && Purpose == TablePurpose.Registry ? $"{column.Label}: {hint}" : column.Label };
    }

    private static DependencyProperty Property<T>(string name, T value) => DependencyProperty.Register(name, typeof(T), typeof(DataTable), new FrameworkPropertyMetadata(value, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, Refresh));
    private static void Refresh(DependencyObject sender, DependencyPropertyChangedEventArgs args) => ((DataTable)sender).Refresh();
    private static void SourceChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
    {
        var table = (DataTable)sender;
        if (args.OldValue is INotifyCollectionChanged old) CollectionChangedEventManager.RemoveHandler(old, table.CollectionChanged);
        if (args.NewValue is INotifyCollectionChanged current) CollectionChangedEventManager.AddHandler(current, table.CollectionChanged);
        table.Refresh();
    }
    private void CollectionChanged(object? sender, NotifyCollectionChangedEventArgs args) => Refresh();
    public void SetColumnWidth(string key, double width)
    {
        if (!double.IsFinite(width) || !columns.Any(column => column.Key == key)) throw new ArgumentException("Column widths require a known key and finite value.", nameof(width));
        manualWidths[key] = Math.Clamp(width, 72, 100_000);
        if (grid.Columns.FirstOrDefault(column => column.SortMemberPath == key) is { } native) native.Width = manualWidths[key];
        RenderBulkValues();
    }
    public void ResetColumnWidth(string key)
    {
        var definition = columns.FirstOrDefault(column => column.Key == key) ?? throw new ArgumentException("The column key is unknown.", nameof(key));
        manualWidths.Remove(key);
        if (grid.Columns.FirstOrDefault(column => column.SortMemberPath == key) is { } native)
        { updating = true; native.Width = definition.Width; updating = false; }
    }
    private void DetachColumnWidths()
    {
        foreach (var subscription in widthSubscriptions)
            DependencyPropertyDescriptor.FromProperty(DataGridColumn.WidthProperty, typeof(DataGridColumn)).RemoveValueChanged(subscription.Column, subscription.Changed);
        widthSubscriptions.Clear();
    }
    private void ReconcileRows()
    {
        for (var index = 0; index < rows.Count; index++)
        {
            if (index < renderedRows.Count && ReferenceEquals(renderedRows[index], rows[index])) continue;
            var match = -1;
            for (var candidate = index + 1; candidate < renderedRows.Count; candidate++)
                if (ReferenceEquals(renderedRows[candidate], rows[index])) { match = candidate; break; }
            if (match >= 0) renderedRows.Move(match, index); else renderedRows.Insert(index, rows[index]);
        }
        while (renderedRows.Count > rows.Count) renderedRows.RemoveAt(renderedRows.Count - 1);
        if (BulkEditing)
        {
            grid.SelectedItems.Clear();
            foreach (var item in renderedRows.Where(item => editSelection.Contains(item))) grid.SelectedItems.Add(item);
        }
    }
    public void SelectAllLoadedForEditing(bool selected)
    {
        if (!BulkEditing || EditingBusy) return;
        foreach (var item in ItemsSource?.Cast<object>() ?? [])
            if (selected) editSelection.Add(item); else editSelection.Remove(item);
        SetCurrentValue(EditSelectionProperty, editSelection.ToArray());
        EditSelectionChanged?.Invoke(this, editSelection.ToArray());
    }
    private static void SelectionChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
    {
        var table = (DataTable)sender;
        table.editSelection.Clear(); table.editSelection.UnionWith(((IEnumerable?)args.NewValue)?.Cast<object>() ?? []);
        table.Refresh();
    }
    private static T? FindAncestor<T>(DependencyObject source) where T : DependencyObject
    {
        while (source is not null)
        {
            if (source is T match) return match;
            source = source is ContentElement content ? ContentOperations.GetParent(content) : System.Windows.Media.VisualTreeHelper.GetParent(source);
        }
        return null;
    }
    private sealed class ActionsConverter(DataTable owner) : IValueConverter
    {
        public object? Convert(object value, Type targetType, object parameter, CultureInfo culture) => value is null ? null : owner.ActionsFor(value);
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotSupportedException();
    }
}
