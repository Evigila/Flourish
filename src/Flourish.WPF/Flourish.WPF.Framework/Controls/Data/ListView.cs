using System.Collections;
using System.Collections.Specialized;
using System.Globalization;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using ArkheideSystem.Flourish.WPF.Abstract;

namespace ArkheideSystem.Flourish.WPF.Controls;

/// <summary>A read-only comparison scenario sharing TableColumn and TableData with DataTable.</summary>
public sealed class ListView : UserControl
{
    private readonly DataGrid grid = DataPresentation.Grid();
    private readonly Border frame;
    private readonly DataText text;
    public ListView()
    {
        text = new DataText(this, Refresh);
        frame = DataPresentation.Frame(grid); Content = frame;
        grid.CanUserSortColumns = false; grid.IsHitTestVisible = true;
        grid.SetResourceReference(FrameworkElement.StyleProperty, "Flourish.ReadOnlyDataGrid");
        SetResourceReference(FontFamilyProperty, "Flourish.FontFamily"); FontSize = 17;
        Loaded += (_, _) => Refresh();
    }
    public static readonly DependencyProperty ItemsSourceProperty = DependencyProperty.Register(nameof(ItemsSource), typeof(IEnumerable), typeof(ListView), new PropertyMetadata(null, SourceChanged));
    public IEnumerable? ItemsSource { get => (IEnumerable?)GetValue(ItemsSourceProperty); set => SetValue(ItemsSourceProperty, value); }
    public static readonly DependencyProperty ColumnsProperty = DependencyProperty.Register(nameof(Columns), typeof(IEnumerable), typeof(ListView), new PropertyMetadata(null, SourceChanged));
    public IEnumerable? Columns { get => (IEnumerable?)GetValue(ColumnsProperty); set => SetValue(ColumnsProperty, value); }
    public string EmptyMessage { get; set; } = "No items.";
    public string Label { get; set; } = "Records";
    public string? RowHeaderKey { get; set; }
    public string? Caption { get; set; }
    public CultureInfo? Culture { get; set; }
    public static readonly DependencyProperty TextProviderProperty = DependencyProperty.Register(nameof(TextProvider), typeof(ITextProvider), typeof(ListView), new PropertyMetadata(null, (sender, args) => ((ListView)sender).text.Provider = (ITextProvider?)args.NewValue));
    public ITextProvider? TextProvider { get => (ITextProvider?)GetValue(TextProviderProperty); set => SetValue(TextProviderProperty, value); }
    public static readonly DependencyProperty CellTemplateProperty = DependencyProperty.Register(nameof(CellTemplate), typeof(DataTemplate), typeof(ListView), new PropertyMetadata(null, (sender, args) => ((ListView)sender).Refresh()));
    public DataTemplate? CellTemplate { get => (DataTemplate?)GetValue(CellTemplateProperty); set => SetValue(CellTemplateProperty, value); }
    public void Refresh()
    {
        var items = ItemsSource?.Cast<object>().ToArray() ?? [];
        var columns = Columns?.Cast<TableColumn>().ToArray() ?? [];
        TableData.ValidateColumns(columns);
        if (items.Length > 0 && columns.Length == 0) throw new ArgumentException("A populated list requires at least one column.", nameof(Columns));
        if (RowHeaderKey is not null && columns.All(column => column.Key != RowHeaderKey)) throw new ArgumentException("A row header must name a supplied column.", nameof(RowHeaderKey));
        AutomationProperties.SetName(grid, Caption ?? (Label == "Records" ? text.Get("Table_Label", "Records") : Label));
        grid.Columns.Clear();
        foreach (var column in columns)
        {
            var native = DataPresentation.Column(column, Culture ?? text.Culture, CellTemplate, true, column.Key == RowHeaderKey);
            native.CanUserSort = false;
            grid.Columns.Add(native);
        }
        grid.ItemsSource = items;
        frame.Child = items.Length == 0 ? new TextBlock { Text = EmptyMessage == "No items." ? text.Get("Table_Empty", "No items.") : EmptyMessage, Margin = new Thickness(24, 20, 24, 20) } : grid;
    }
    private static void SourceChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
    {
        var list = (ListView)sender;
        if (args.OldValue is INotifyCollectionChanged old) CollectionChangedEventManager.RemoveHandler(old, list.CollectionChanged);
        if (args.NewValue is INotifyCollectionChanged current) CollectionChangedEventManager.AddHandler(current, list.CollectionChanged);
        list.Refresh();
    }
    private void CollectionChanged(object? sender, NotifyCollectionChangedEventArgs args) => Refresh();
}
