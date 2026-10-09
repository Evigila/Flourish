using System.Collections;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using ArkheideSystem.Flourish.WPF.Abstract;

namespace ArkheideSystem.Flourish.WPF.Controls;

/// <summary>The single column-search renderer, standalone or composed by DataTable.</summary>
public sealed class DataSearch : UserControl
{
    private readonly SelectBox column = new() { MinWidth = 160 };
    private readonly TextBox query = new() { MinWidth = 200, Margin = new Thickness(12, 0, 0, 0) };
    private readonly Field columnField;
    private readonly Field queryField;
    private bool updating;
    private readonly DataText text;

    public DataSearch()
    {
        text = new DataText(this, Refresh);
        var panel = new Grid();
        panel.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        panel.ColumnDefinitions.Add(new ColumnDefinition());
        query.Margin = new Thickness(0);
        columnField = new Field { Label = "Search by", Content = column };
        queryField = new Field { Label = "Content", Content = query, Margin = new Thickness(12, 0, 0, 0) };
        Grid.SetColumn(queryField, 1);
        panel.Children.Add(columnField);
        panel.Children.Add(queryField);
        Content = panel;
        AutomationProperties.SetName(column, "Search by");
        AutomationProperties.SetName(query, "Content");
        column.SelectionChanged += (_, _) => Publish();
        query.TextChanged += (_, _) => Publish();
    }

    public static readonly DependencyProperty ColumnsProperty = DependencyProperty.Register(nameof(Columns), typeof(IEnumerable), typeof(DataSearch), new PropertyMetadata(null, Refresh));
    public IEnumerable? Columns { get => (IEnumerable?)GetValue(ColumnsProperty); set => SetValue(ColumnsProperty, value); }
    public static readonly DependencyProperty ColumnKeyProperty = DependencyProperty.Register(nameof(ColumnKey), typeof(string), typeof(DataSearch), new PropertyMetadata(null, Refresh));
    public string? ColumnKey { get => (string?)GetValue(ColumnKeyProperty); set => SetValue(ColumnKeyProperty, value); }
    public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(nameof(Value), typeof(string), typeof(DataSearch), new FrameworkPropertyMetadata(string.Empty, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, Refresh));
    public string Value { get => (string)GetValue(ValueProperty); set => SetValue(ValueProperty, value); }
    public static readonly DependencyProperty IncludeAllColumnsProperty = DependencyProperty.Register(nameof(IncludeAllColumns), typeof(bool), typeof(DataSearch), new PropertyMetadata(false, Refresh));
    public bool IncludeAllColumns { get => (bool)GetValue(IncludeAllColumnsProperty); set => SetValue(IncludeAllColumnsProperty, value); }
    public static readonly DependencyProperty MaximumLengthProperty = DependencyProperty.Register(nameof(MaximumLength), typeof(int), typeof(DataSearch), new PropertyMetadata(200, Refresh), value => (int)value > 0);
    public int MaximumLength { get => (int)GetValue(MaximumLengthProperty); set => SetValue(MaximumLengthProperty, value); }
    public event EventHandler<TableSearchRequest>? Changed;
    public static readonly DependencyProperty TextProviderProperty = DependencyProperty.Register(nameof(TextProvider), typeof(ITextProvider), typeof(DataSearch), new PropertyMetadata(null, (sender, args) => ((DataSearch)sender).text.Provider = (ITextProvider?)args.NewValue));
    public ITextProvider? TextProvider { get => (ITextProvider?)GetValue(TextProviderProperty); set => SetValue(TextProviderProperty, value); }

    private static void Refresh(DependencyObject sender, DependencyPropertyChangedEventArgs args) => ((DataSearch)sender).Refresh();
    private void Refresh()
    {
        if (updating) return;
        updating = true;
        try
        {
            var columns = Columns?.Cast<TableColumn>().ToArray() ?? [];
            TableData.ValidateColumns(columns);
            var available = columns.Where(item => item.Searchable).ToArray();
            var options = available.Select(item => new SelectOption(item.Key, item.Label)).ToList();
            if (IncludeAllColumns) options.Insert(0, new SelectOption(null, text.Get("Table_AllColumns", "All columns")));
            column.Options = options;
            var effective = available.Any(item => item.Key == ColumnKey) ? ColumnKey : IncludeAllColumns ? null : available.FirstOrDefault()?.Key;
            SetCurrentValue(ColumnKeyProperty, effective);
            column.SelectedValue = effective;
            if (effective is null && options.Count > 0) column.SelectedIndex = 0;
            query.MaxLength = MaximumLength;
            query.Text = Value ?? string.Empty;
            panelVisibility(available.Length > 0);
            var label = available.FirstOrDefault(item => item.Key == effective)?.Label ?? text.Get("Table_SearchContent", "Content");
            columnField.Label = text.Get("Table_SearchBy", "Search by");
            AutomationProperties.SetName(column, columnField.Label);
            AutomationProperties.SetName(query, label);
            queryField.Label = label;
        }
        finally { updating = false; }
    }

    private void panelVisibility(bool available) => ((UIElement)Content).Visibility = available ? Visibility.Visible : Visibility.Collapsed;

    private void Publish()
    {
        if (updating || !IsEnabled) return;
        var key = column.SelectedValue as string;
        var value = query.Text ?? string.Empty;
        updating = true;
        SetCurrentValue(ColumnKeyProperty, key);
        SetCurrentValue(ValueProperty, value);
        updating = false;
        Changed?.Invoke(this, new TableSearchRequest(key, value));
    }
}
