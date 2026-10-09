using System.Collections;
using System.Collections.Specialized;
using System.Globalization;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using ArkheideSystem.Flourish.WPF.Abstract;

namespace ArkheideSystem.Flourish.WPF.Controls;

/// <summary>Spreadsheet editing proposes typed changes while the consumer owns data and persistence.</summary>
public sealed class EditingGrid : UserControl
{
    private readonly DataGrid grid = DataPresentation.Grid(true);
    private readonly Border frame;
    private readonly ValidationMessages error = new();
    private IReadOnlyList<GridColumn> columns = [];
    private bool refreshing;
    private bool editEnding;
    private bool refreshQueued;
    private readonly DataText text;

    public EditingGrid()
    {
        text = new DataText(this, Refresh);
        frame = DataPresentation.Frame(grid);
        var layout = new DockPanel(); DockPanel.SetDock(error, Dock.Bottom);
        error.Margin = new Thickness(0, 8, 0, 0); error.Visibility = Visibility.Collapsed;
        layout.Children.Add(error); layout.Children.Add(frame); Content = layout;
        SetResourceReference(FontFamilyProperty, "Flourish.FontFamily"); FontSize = 17;
        grid.CanUserSortColumns = false;
        grid.BeginningEdit += (_, args) =>
        {
            var column = columns.FirstOrDefault(item => item.Key == args.Column.SortMemberPath);
            args.Cancel = EditDisabled || column is null || column.ReadOnly || column.Editor == GridEditorKind.Link;
            if (!args.Cancel) { ClearError(); EditBeginning?.Invoke(this, args.Row.Item); }
        };
        grid.CellEditEnding += OnCellEditEnding;
        grid.PreparingCellForEdit += (_, args) =>
        {
            Dispatcher.BeginInvoke(new Action(() =>
            {
                if (FindChild<TextBox>(args.EditingElement) is { } input)
                {
                    input.Focus();
                    if (args.EditingEventArgs is TextCompositionEventArgs composition) { input.Text = composition.Text; input.CaretIndex = input.Text.Length; }
                    else input.SelectAll();
                }
                else if (FindChild<SelectBox>(args.EditingElement) is { } select) select.Focus();
                else if (FindChild<DateBox>(args.EditingElement) is { } date) date.Focus();
                else args.EditingElement.MoveFocus(new TraversalRequest(FocusNavigationDirection.First));
            }));
        };
        grid.PreviewKeyDown += OnKeyDown;
        Loaded += (_, _) => Refresh();
    }

    public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register(nameof(ItemsSource), typeof(IEnumerable), typeof(EditingGrid), new PropertyMetadata(null, SourceChanged));
    public IEnumerable? ItemsSource { get => (IEnumerable?)GetValue(ItemsSourceProperty); set => SetValue(ItemsSourceProperty, value); }
    public static readonly DependencyProperty ColumnsProperty = DependencyProperty.Register(nameof(Columns), typeof(IEnumerable), typeof(EditingGrid), new PropertyMetadata(null, SourceChanged));
    public IEnumerable? Columns { get => (IEnumerable?)GetValue(ColumnsProperty); set => SetValue(ColumnsProperty, value); }
    public static readonly DependencyProperty EditDisabledProperty = DependencyProperty.Register(nameof(EditDisabled), typeof(bool), typeof(EditingGrid), new PropertyMetadata(false, Refresh));
    public bool EditDisabled { get => (bool)GetValue(EditDisabledProperty); set => SetValue(EditDisabledProperty, value); }
    public string Label { get; set; } = "Editable grid";
    public string EmptyMessage { get; set; } = "No records found.";
    public CultureInfo? Culture { get; set; }
    private CultureInfo FormatCulture => Culture ?? text.Culture;
    public static readonly DependencyProperty TextProviderProperty = DependencyProperty.Register(nameof(TextProvider), typeof(ITextProvider), typeof(EditingGrid), new PropertyMetadata(null, (sender, args) => ((EditingGrid)sender).text.Provider = (ITextProvider?)args.NewValue));
    public ITextProvider? TextProvider { get => (ITextProvider?)GetValue(TextProviderProperty); set => SetValue(TextProviderProperty, value); }
    /// <summary>A native template editor is required for Template columns; its DataContext is the source item.</summary>
    public Func<GridColumn, DataTemplate?>? EditorTemplate { get; set; }
    /// <summary>Reads the proposed value from a Template editor on commit.</summary>
    public Func<GridColumn, FrameworkElement, object?>? TemplateValue { get; set; }
    public event EventHandler<GridCellChange>? CellChanged;
    public event EventHandler<IReadOnlyList<GridCellChange>>? CellsChanged;
    public event EventHandler<GridEditError>? EditError;
    public event EventHandler<object>? EditBeginning;
    public event EventHandler? EditCanceled;
    public event EventHandler? SaveRequested;
    public event EventHandler? UndoRequested;
    public event EventHandler? RedoRequested;

    public void Refresh()
    {
        if (editEnding)
        {
            if (!refreshQueued)
            {
                refreshQueued = true;
                Dispatcher.BeginInvoke(new Action(() => { refreshQueued = false; Refresh(); }));
            }
            return;
        }
        if (refreshing) return;
        refreshing = true;
        try
        {
            columns = Columns?.Cast<GridColumn>().ToArray() ?? [];
            var keys = new HashSet<string>(StringComparer.Ordinal);
            foreach (var column in columns)
            {
                if (string.IsNullOrWhiteSpace(column.Key) || string.IsNullOrWhiteSpace(column.Label) || !keys.Add(column.Key)
                    || !double.IsFinite(column.Width) || column.Width < 100 || column.MaximumLength < 0)
                    throw new ArgumentException("Editing columns require unique keys, labels, widths of at least 100 and nonnegative maximum lengths.", nameof(Columns));
                if (column.Editor == GridEditorKind.Template && (EditorTemplate?.Invoke(column) is null || TemplateValue is null))
                    throw new ArgumentException("Template columns require EditorTemplate and TemplateValue.", nameof(EditorTemplate));
                if (column.Editor == GridEditorKind.Masked && string.IsNullOrEmpty(column.Mask))
                    throw new ArgumentException("Masked editors require an explicit MaskedInput mask.", nameof(Columns));
                if (column.Editor == GridEditorKind.Link && column.LinkAction is null)
                    throw new ArgumentException("Link cells require a consumer-owned LinkAction.", nameof(Columns));
            }
            AutomationProperties.SetName(grid, Label == "Editable grid" ? text.Get("Grid_Label", "Editable grid") : Label);
            var widths = grid.Columns.ToDictionary(item => item.SortMemberPath, item => item.Width, StringComparer.Ordinal);
            grid.Columns.Clear();
            foreach (var column in columns)
            {
                var read = new TableColumn(column.Key, column.Label, column.BindingPath ?? column.Key, Width: column.Width);
                var native = (DataGridTemplateColumn)DataPresentation.Column(read, FormatCulture, column.Editor == GridEditorKind.Link ? LinkTemplate(column) : null);
                native.IsReadOnly = column.ReadOnly || column.Editor == GridEditorKind.Link;
                native.CanUserSort = false; native.MinWidth = 100;
                native.CellEditingTemplate = CreateEditor(column);
                if (widths.TryGetValue(column.Key, out var width)) native.Width = width;
                grid.Columns.Add(native);
            }
            grid.IsReadOnly = EditDisabled;
            var rows = ItemsSource?.Cast<object>().ToArray() ?? [];
            grid.ItemsSource = rows;
            frame.Child = rows.Length == 0 ? new TextBlock { Text = EmptyMessage == "No records found." ? text.Get("Grid_Empty", "No records found.") : EmptyMessage, Margin = new Thickness(24, 20, 24, 20) } : grid;
        }
        finally { refreshing = false; }
    }

    private DataTemplate CreateEditor(GridColumn column)
    {
        if (column.Editor == GridEditorKind.Template) return EditorTemplate!(column)!;
        if (column.Editor == GridEditorKind.Select)
        {
            var select = new FrameworkElementFactory(typeof(SelectBox));
            select.SetValue(SelectBox.OptionsProperty, column.Options ?? []);
            select.SetBinding(System.Windows.Controls.Primitives.Selector.SelectedValueProperty, new Binding(column.BindingPath ?? column.Key) { Mode = BindingMode.OneWay });
            select.SetValue(FrameworkElement.MarginProperty, new Thickness(8));
            select.SetValue(AutomationProperties.NameProperty, column.Label);
            return new DataTemplate { VisualTree = select };
        }
        if (column.Editor == GridEditorKind.Date)
        {
            var date = new FrameworkElementFactory(typeof(DateBox));
            date.SetBinding(DatePicker.SelectedDateProperty, new Binding(column.BindingPath ?? column.Key) { Mode = BindingMode.OneWay, Converter = new EditDateConverter() });
            date.SetValue(FrameworkElement.MarginProperty, new Thickness(8));
            date.SetValue(AutomationProperties.NameProperty, column.Label);
            return new DataTemplate { VisualTree = date };
        }
        var text = new FrameworkElementFactory(column.Editor == GridEditorKind.Masked ? typeof(MaskedInput) : typeof(TextBox));
        if (column.Editor == GridEditorKind.Masked) text.SetValue(MaskedInput.MaskProperty, column.Mask);
        text.SetBinding(column.Editor == GridEditorKind.Masked ? MaskedInput.ValueProperty : System.Windows.Controls.TextBox.TextProperty, new Binding(column.BindingPath ?? column.Key)
        { Mode = BindingMode.OneWay, Converter = new EditValueConverter(column), ConverterCulture = FormatCulture });
        text.SetValue(System.Windows.Controls.TextBox.MaxLengthProperty, column.Editor == GridEditorKind.Masked ? 0 : column.MaximumLength);
        text.SetValue(FrameworkElement.MarginProperty, new Thickness(8));
        text.SetValue(AutomationProperties.NameProperty, column.Label);
        if (column.Editor == GridEditorKind.Multiline)
        {
            text.SetValue(System.Windows.Controls.TextBox.AcceptsReturnProperty, true);
            text.SetValue(System.Windows.Controls.TextBox.TextWrappingProperty, TextWrapping.Wrap);
            text.SetValue(FrameworkElement.MinHeightProperty, 64d);
        }
        return new DataTemplate { VisualTree = text };
    }

    private DataTemplate LinkTemplate(GridColumn column)
    {
        var factory = new FrameworkElementFactory(typeof(Button));
        factory.SetValue(Button.VariantProperty, ButtonVariant.Underline);
        factory.SetValue(UIElement.IsEnabledProperty, !EditDisabled);
        factory.SetValue(FrameworkElement.MarginProperty, new Thickness(16, 8, 16, 8));
        factory.SetBinding(Button.TextProperty, new Binding(nameof(TableCellContext.Display)));
        factory.AddHandler(System.Windows.Controls.Primitives.ButtonBase.ClickEvent, new RoutedEventHandler(async (sender, _) =>
        {
            if (sender is not Button { DataContext: TableCellContext context } link) return;
            link.IsEnabled = false;
            try { await column.LinkAction!(context.Item); }
            finally { link.IsEnabled = !EditDisabled; }
        }));
        return new DataTemplate { VisualTree = factory };
    }

    private void OnCellEditEnding(object? sender, DataGridCellEditEndingEventArgs args)
    {
        editEnding = true;
        try { CommitCell(args); }
        finally { editEnding = false; }
    }

    private void CommitCell(DataGridCellEditEndingEventArgs args)
    {
        if (args.EditAction == DataGridEditAction.Cancel) { EditCanceled?.Invoke(this, EventArgs.Empty); return; }
        var column = columns.Single(item => item.Key == args.Column.SortMemberPath);
        if (EditDisabled || column.ReadOnly) { args.Cancel = true; return; }
        object? proposed = column.Editor switch
        {
            GridEditorKind.Template => TemplateValue!(column, args.EditingElement),
            GridEditorKind.Select => FindChild<SelectBox>(args.EditingElement)?.SelectedValue,
            GridEditorKind.Date => FindChild<DateBox>(args.EditingElement)?.SelectedDate,
            GridEditorKind.Masked => FindChild<MaskedInput>(args.EditingElement)?.Value,
            _ => FindChild<TextBox>(args.EditingElement)?.Text
        };
        var sourceValue = TableData.Read(args.Row.Item, column.BindingPath ?? column.Key);
        if (!TryValidate(proposed, ValueType(args.Row.Item, column), column, out var typed, out var message))
        { args.Cancel = true; ShowError(args.Row.Item, column, message!); return; }
        ClearError();
        if (!Equals(sourceValue, typed)) CellChanged?.Invoke(this, new GridCellChange(args.Row.Item, column.Key, typed));
    }

    private bool TryValidate(object? value, Type? targetType, GridColumn column, out object? typed, out string? message)
    {
        typed = value; message = null;
        var input = Convert.ToString(value, FormatCulture) ?? string.Empty;
        if (column.Editor == GridEditorKind.Masked) { input = InputMaskFormatter.Normalize(column.Mask!, input); typed = input; }
        var acceptsNull = targetType is null || !targetType.IsValueType || Nullable.GetUnderlyingType(targetType) is not null;
        targetType = targetType is null ? null : Nullable.GetUnderlyingType(targetType) ?? targetType;
        if (acceptsNull && string.IsNullOrWhiteSpace(input) && column.Editor is GridEditorKind.Date or GridEditorKind.Decimal) typed = null;
        if (column.Required && string.IsNullOrWhiteSpace(input)) message = $"{column.Label}: {text.Get("Input_Required", "Required")}";
        else if (column.MaximumLength > 0 && input.Length > column.MaximumLength) message = $"{column.Label} exceeds {column.MaximumLength} characters.";
        else if (column.Editor == GridEditorKind.Decimal && !string.IsNullOrWhiteSpace(input))
        {
            if (decimal.TryParse(input, NumberStyles.Number, FormatCulture, out var number)) typed = number;
            else message = $"{column.Label}: {text.Get("Input_NumberError", "Enter a valid number.")}";
        }
        else if (column.Editor == GridEditorKind.Date && !string.IsNullOrWhiteSpace(input))
        {
            if (value is DateTime picked) typed = targetType == typeof(DateOnly) ? DateOnly.FromDateTime(picked) : picked;
            else if (DateOnly.TryParseExact(input, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var date)) typed = targetType == typeof(DateTime) ? date.ToDateTime(TimeOnly.MinValue) : date;
            else message = text.Get("Input_DateError", "The {0} field must be a valid date.", column.Label);
        }
        else if (column.Editor == GridEditorKind.Masked && !string.IsNullOrEmpty(input))
        {
            if (input.Length != column.Mask!.Count(InputMaskFormatter.IsSlot))
                message = $"{column.Label} does not match the expected format.";
        }
        var selected = typed;
        if (message is null && column.Editor == GridEditorKind.Select && column.Options is { } options && options.All(option => !Equals(option.Value, selected) || option.Disabled))
            message = $"{column.Label}: {text.Get("Input_SelectError", "Select a valid option.")}";
        if (message is null && targetType is not null && typed is not null && !targetType.IsInstanceOfType(typed) && targetType != typeof(string) && column.Editor != GridEditorKind.Template)
        {
            try { typed = Convert.ChangeType(typed, Nullable.GetUnderlyingType(targetType) ?? targetType, FormatCulture); }
            catch (Exception exception) when (exception is InvalidCastException or FormatException or OverflowException) { message = $"{column.Label} has an invalid value."; }
        }
        if (targetType == typeof(string)) typed = input;
        message ??= column.Validate?.Invoke(typed);
        return message is null;
    }

    private void OnKeyDown(object sender, KeyEventArgs args)
    {
        if (EditDisabled) return;
        if (Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
        {
            var nativeEditing = FindAncestor<DataGridCell>(args.OriginalSource as DependencyObject)?.IsEditing == true;
            switch (args.Key)
            {
                case Key.S: if (grid.CommitEdit(DataGridEditingUnit.Cell, true)) SaveRequested?.Invoke(this, EventArgs.Empty); args.Handled = true; break;
                case Key.Z when !nativeEditing:
                    if (Keyboard.Modifiers.HasFlag(ModifierKeys.Shift)) RedoRequested?.Invoke(this, EventArgs.Empty); else UndoRequested?.Invoke(this, EventArgs.Empty);
                    args.Handled = true; break;
                case Key.Y when !nativeEditing: RedoRequested?.Invoke(this, EventArgs.Empty); args.Handled = true; break;
                case Key.V when FindAncestor<TextBox>(args.OriginalSource as DependencyObject) is null: Paste(); args.Handled = true; break;
                case Key.Enter:
                    object? value = FindAncestor<SelectBox>(args.OriginalSource as DependencyObject)?.SelectedValue;
                    if (FindAncestor<TextBox>(args.OriginalSource as DependencyObject) is { } input) value = input.Text;
                    else if (FindAncestor<DateBox>(args.OriginalSource as DependencyObject) is { } date) value = date.SelectedDate;
                    else if (!nativeEditing && grid.CurrentCell.Item is { } item && grid.CurrentCell.Column is { } selectedColumn)
                    {
                        var definition = columns.Single(column => column.Key == selectedColumn.SortMemberPath);
                        value = TableData.Read(item, definition.BindingPath ?? definition.Key);
                    }
                    ProposeSelected(value); args.Handled = true; break;
            }
        }
        if (args.Key == Key.Delete && Keyboard.Modifiers == ModifierKeys.None && FindAncestor<TextBox>(args.OriginalSource as DependencyObject) is null)
        {
            ProposeSelected(string.Empty); args.Handled = true;
        }
    }

    public void Paste()
    {
        if (EditDisabled || !Clipboard.ContainsText()) return;
        var text = Clipboard.GetText();
        if (text.Length > 1_048_576) { ShowGeneralError(this.text.Get("Grid_PasteTooLarge", "Pasted content must not exceed 2 MB.")); return; }
        var source = grid.ItemsSource?.Cast<object>().ToArray() ?? [];
        var startRow = Array.IndexOf(source, grid.CurrentCell.Item);
        var startColumn = grid.CurrentCell.Column?.DisplayIndex ?? -1;
        if (startRow < 0 || startColumn < 0) return;
        var proposals = new List<GridCellChange>();
        var lines = text.Replace("\r\n", "\n", StringComparison.Ordinal).Replace('\r', '\n').TrimEnd('\n').Split('\n');
        for (var row = 0; row < lines.Length && startRow + row < source.Length; row++)
        {
            var values = lines[row].Split('\t');
            for (var columnIndex = 0; columnIndex < values.Length && startColumn + columnIndex < columns.Count; columnIndex++)
            {
                var column = columns[startColumn + columnIndex];
                if (column.ReadOnly || column.Editor is GridEditorKind.Link or GridEditorKind.Template) continue;
                var item = source[startRow + row]; var existing = TableData.Read(item, column.BindingPath ?? column.Key);
                if (!TryValidate(values[columnIndex], ValueType(item, column), column, out var typed, out var message)) { ShowError(item, column, message!); return; }
                proposals.Add(new GridCellChange(item, column.Key, typed));
            }
        }
        Publish(proposals);
    }

    private void ProposeSelected(object? value)
    {
        var proposals = new List<GridCellChange>();
        foreach (var cell in grid.SelectedCells)
        {
            var column = columns.Single(item => item.Key == cell.Column.SortMemberPath);
            if (column.ReadOnly || column.Editor is GridEditorKind.Link or GridEditorKind.Template) continue;
            var existing = TableData.Read(cell.Item, column.BindingPath ?? column.Key);
            if (!TryValidate(value, ValueType(cell.Item, column), column, out var typed, out var message)) { ShowError(cell.Item, column, message!); return; }
            proposals.Add(new GridCellChange(cell.Item, column.Key, typed));
        }
        Publish(proposals);
    }

    private void Publish(IReadOnlyList<GridCellChange> changes)
    {
        ClearError();
        if (CellsChanged is not null) CellsChanged.Invoke(this, changes);
        else foreach (var change in changes) CellChanged?.Invoke(this, change);
    }
    private void ShowError(object item, GridColumn column, string message) { ShowGeneralError(message); EditError?.Invoke(this, new GridEditError(item, column.Key, message)); }
    private void ShowGeneralError(string message) { error.Messages = new[] { message }; error.Visibility = Visibility.Visible; AutomationProperties.SetLiveSetting(error, AutomationLiveSetting.Assertive); }
    private void ClearError() { error.Messages = Array.Empty<string>(); error.Visibility = Visibility.Collapsed; }
    private static void Refresh(DependencyObject sender, DependencyPropertyChangedEventArgs args) => ((EditingGrid)sender).Refresh();
    private static void SourceChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
    {
        var owner = (EditingGrid)sender;
        if (args.OldValue is INotifyCollectionChanged old) CollectionChangedEventManager.RemoveHandler(old, owner.CollectionChanged);
        if (args.NewValue is INotifyCollectionChanged current) CollectionChangedEventManager.AddHandler(current, owner.CollectionChanged);
        owner.Refresh();
    }
    private void CollectionChanged(object? sender, NotifyCollectionChangedEventArgs args) => Refresh();
    private static Type? ValueType(object item, GridColumn column)
    {
        var type = item.GetType();
        foreach (var segment in (column.BindingPath ?? column.Key).Split('.'))
        {
            var property = type.GetProperty(segment);
            if (property is null) return TableData.Read(item, column.BindingPath ?? column.Key)?.GetType();
            type = property.PropertyType;
        }
        return type;
    }
    private static T? FindChild<T>(DependencyObject parent) where T : DependencyObject
    {
        if (parent is T found) return found;
        for (var index = 0; index < System.Windows.Media.VisualTreeHelper.GetChildrenCount(parent); index++)
            if (FindChild<T>(System.Windows.Media.VisualTreeHelper.GetChild(parent, index)) is { } child) return child;
        return null;
    }
    private static T? FindAncestor<T>(DependencyObject? source) where T : DependencyObject
    {
        while (source is not null) { if (source is T found) return found; source = source is ContentElement content ? ContentOperations.GetParent(content) : System.Windows.Media.VisualTreeHelper.GetParent(source); }
        return null;
    }
    private sealed class EditValueConverter(GridColumn column) : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture) => value switch
        {
            DateOnly date when column.Editor == GridEditorKind.Date => date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            DateTime date when column.Editor == GridEditorKind.Date => date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
            _ => System.Convert.ToString(value, culture) ?? string.Empty
        };
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotSupportedException();
    }
    private sealed class EditDateConverter : IValueConverter
    {
        public object? Convert(object value, Type targetType, object parameter, CultureInfo culture) => value is DateOnly date ? date.ToDateTime(TimeOnly.MinValue) : value is DateTime time ? time : null;
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) => throw new NotSupportedException();
    }
}
