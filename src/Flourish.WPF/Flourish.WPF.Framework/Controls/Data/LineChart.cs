using System.Collections;
using System.Collections.Specialized;
using System.Globalization;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using ArkheideSystem.Flourish.WPF.Abstract;

namespace ArkheideSystem.Flourish.WPF.Controls;

/// <summary>Native vector plotting with the same general display selection as DataTable.</summary>
public sealed class LineChart : UserControl
{
    private readonly MultiSelectBox display = new() { ReorderEnabled = true, Text = "Display" };
    private readonly ChartPlot plot = new();
    private readonly TextBlock heading = DataPresentation.Text(string.Empty);
    private readonly WrapPanel legend = new();
    private readonly TextBlock empty = DataPresentation.Text("No series selected.", true);
    private readonly ContentControl controls = new();
    private readonly Grid body = new();
    private readonly List<string> order = [];
    private readonly HashSet<string> known = new(StringComparer.Ordinal);
    private readonly HashSet<string> hidden = new(StringComparer.Ordinal);
    private IReadOnlyList<ChartSeries> series = [];
    private readonly DataText text;
    private bool updating;

    public LineChart()
    {
        text = new DataText(this, Refresh);
        var toolbar = new DockPanel { Margin = new Thickness(0, 0, 0, 16) };
        display.Margin = new Thickness(12, 0, 0, 0); DockPanel.SetDock(display, Dock.Right); toolbar.Children.Add(display);
        controls.HorizontalAlignment = HorizontalAlignment.Right; DockPanel.SetDock(controls, Dock.Right); toolbar.Children.Add(controls);
        heading.FontSize = 26; heading.FontWeight = FontWeights.Bold; heading.VerticalAlignment = VerticalAlignment.Center;
        toolbar.Children.Add(heading);
        var layout = new Grid();
        layout.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        layout.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        layout.RowDefinitions.Add(new RowDefinition());
        layout.Children.Add(toolbar); Grid.SetRow(legend, 1); layout.Children.Add(legend); Grid.SetRow(body, 2); layout.Children.Add(body);
        Content = layout; SetResourceReference(FontFamilyProperty, "Flourish.FontFamily"); FontSize = 17;
        SetResourceReference(ForegroundProperty, "Flourish.Brush.Text");
        plot.MinHeight = 260;
        AutomationProperties.SetName(display, "Display");
        display.Changed += (_, change) => ApplyDisplay(change);
        Loaded += (_, _) => Refresh();
    }

    public static readonly DependencyProperty LabelsProperty = DependencyProperty.Register(nameof(Labels), typeof(IEnumerable), typeof(LineChart), new PropertyMetadata(null, SourceChanged));
    public IEnumerable? Labels { get => (IEnumerable?)GetValue(LabelsProperty); set => SetValue(LabelsProperty, value); }
    public static readonly DependencyProperty SeriesProperty = DependencyProperty.Register(nameof(Series), typeof(IEnumerable), typeof(LineChart), new PropertyMetadata(null, SourceChanged));
    public IEnumerable? Series { get => (IEnumerable?)GetValue(SeriesProperty); set => SetValue(SeriesProperty, value); }
    public static readonly DependencyProperty TitleProperty = DependencyProperty.Register(nameof(Title), typeof(string), typeof(LineChart), new PropertyMetadata("Chart", Refresh));
    public string Title { get => (string)GetValue(TitleProperty); set => SetValue(TitleProperty, value); }
    public static readonly DependencyProperty HeadingProperty = DependencyProperty.Register(nameof(Heading), typeof(string), typeof(LineChart), new PropertyMetadata(null, Refresh));
    public string? Heading { get => (string?)GetValue(HeadingProperty); set => SetValue(HeadingProperty, value); }
    public static readonly DependencyProperty DescriptionProperty = DependencyProperty.Register(nameof(Description), typeof(string), typeof(LineChart), new PropertyMetadata(null, Refresh));
    public string? Description { get => (string?)GetValue(DescriptionProperty); set => SetValue(DescriptionProperty, value); }
    public static readonly DependencyProperty IndependentScalesProperty = Property(nameof(IndependentScales), false);
    public bool IndependentScales { get => (bool)GetValue(IndependentScalesProperty); set => SetValue(IndependentScalesProperty, value); }
    public static readonly DependencyProperty ShowLegendProperty = Property(nameof(ShowLegend), true);
    public bool ShowLegend { get => (bool)GetValue(ShowLegendProperty); set => SetValue(ShowLegendProperty, value); }
    public static readonly DependencyProperty ShowDisplayOptionsProperty = Property(nameof(ShowDisplayOptions), true);
    public bool ShowDisplayOptions { get => (bool)GetValue(ShowDisplayOptionsProperty); set => SetValue(ShowDisplayOptionsProperty, value); }
    public static readonly DependencyProperty ControlsContentProperty = DependencyProperty.Register(nameof(ControlsContent), typeof(object), typeof(LineChart), new PropertyMetadata(null, Refresh));
    public object? ControlsContent { get => GetValue(ControlsContentProperty); set => SetValue(ControlsContentProperty, value); }
    public string EmptyText { get; set; } = "No series selected.";
    public string MaximumLabel { get; set; } = "Maximum";
    public IReadOnlyList<ChartSeries> VisibleSeries => order.Where(key => !hidden.Contains(key)).Select(key => series.Single(item => item.Key == key)).ToArray();
    public static readonly DependencyProperty TextProviderProperty = DependencyProperty.Register(nameof(TextProvider), typeof(ITextProvider), typeof(LineChart), new PropertyMetadata(null, (sender, args) => ((LineChart)sender).text.Provider = (ITextProvider?)args.NewValue));
    public ITextProvider? TextProvider { get => (ITextProvider?)GetValue(TextProviderProperty); set => SetValue(TextProviderProperty, value); }
    public CultureInfo? Culture { get; set; }
    private CultureInfo FormatCulture => Culture ?? text.Culture;
    public event EventHandler<MultiSelectChange>? DisplayChanged;

    public void SetData(IEnumerable<ChartPointLabel> labels, IEnumerable<ChartSeries> series)
    {
        ArgumentNullException.ThrowIfNull(labels); ArgumentNullException.ThrowIfNull(series);
        updating = true;
        try { SetCurrentValue(LabelsProperty, labels.ToArray()); SetCurrentValue(SeriesProperty, series.ToArray()); }
        finally { updating = false; }
        Refresh();
    }

    public void Refresh()
    {
        if (updating) return;
        var labels = Labels?.Cast<ChartPointLabel>().ToArray() ?? [];
        series = Series?.Cast<ChartSeries>().ToArray() ?? [];
        if (string.IsNullOrWhiteSpace(Title)) throw new ArgumentException("Charts require a readable title.", nameof(Title));
        var labelKeys = new HashSet<string>(StringComparer.Ordinal);
        if (labels.Any(label => string.IsNullOrWhiteSpace(label.Key) || string.IsNullOrWhiteSpace(label.Text) || !labelKeys.Add(label.Key)))
            throw new ArgumentException("Chart labels require unique stable keys and readable text.", nameof(Labels));
        var keys = new HashSet<string>(StringComparer.Ordinal);
        foreach (var item in series)
            if (string.IsNullOrWhiteSpace(item.Key) || string.IsNullOrWhiteSpace(item.Label) || !keys.Add(item.Key)
                || item.Values is null || item.Values.Count != labels.Length || item.Values.Any(value => !double.IsFinite(value))
                || (item.Maximum is { } maximum && (!double.IsFinite(maximum) || maximum <= 0 || maximum < item.Values.DefaultIfEmpty(0).Max())))
                throw new ArgumentException("Series require unique keys, readable labels, finite values for every label and a positive maximum containing all values.", nameof(Series));
        known.IntersectWith(keys); hidden.IntersectWith(keys); order.RemoveAll(key => !keys.Contains(key));
        foreach (var item in series)
        {
            if (!order.Contains(item.Key, StringComparer.Ordinal)) order.Add(item.Key);
            if (known.Add(item.Key) && !item.Visible) hidden.Add(item.Key);
        }
        heading.Text = Heading ?? string.Empty; heading.Visibility = string.IsNullOrWhiteSpace(Heading) ? Visibility.Collapsed : Visibility.Visible;
        controls.Content = ControlsContent;
        display.Options = order.Select(key => series.Single(item => item.Key == key)).Select(item => new MultiSelectOption(item.Key, item.Label, !hidden.Contains(item.Key))).ToArray();
        display.Text = text.Get("Table_Display", "Display");
        AutomationProperties.SetName(display, display.Text);
        display.Visibility = ShowDisplayOptions ? Visibility.Visible : Visibility.Collapsed;
        legend.Children.Clear();
        var visible = VisibleSeries;
        foreach (var item in visible)
        {
            var maximum = Scale(item, visible, IndependentScales).Maximum;
            var label = DataPresentation.Text($"{item.Label}   {(MaximumLabel == "Maximum" ? text.Get("Chart_Maximum", "Maximum") : MaximumLabel)} {Format(item, maximum, FormatCulture)}");
            label.FontWeight = FontWeights.SemiBold; label.Margin = new Thickness(0, 0, 24, 12);
            label.SetResourceReference(TextBlock.ForegroundProperty, ChartPlot.ColorKeys[Array.IndexOf(series.ToArray(), item) % 4]);
            legend.Children.Add(label);
        }
        legend.Visibility = ShowLegend ? Visibility.Visible : Visibility.Collapsed;
        body.Children.Clear();
        if (labels.Length == 0 || visible.Count == 0) { empty.Text = EmptyText == "No series selected." ? text.Get("Chart_NoSeries", "No series selected.") : EmptyText; body.Children.Add(empty); }
        else
        {
            plot.Labels = labels; plot.Series = visible; plot.AllSeries = series; plot.IndependentScales = IndependentScales;
            plot.FormatCulture = FormatCulture;
            AutomationProperties.SetName(plot, Title);
            // Native accessibility exposes the same data that the browser's hidden comparison table exposes.
            AutomationProperties.SetHelpText(plot, string.Join("\n", new[] { Description ?? string.Empty }.Concat(labels.Select((label, index) =>
                $"{label.Tooltip ?? label.Text}: {string.Join("; ", visible.Select(item => $"{item.Label} {Format(item, item.Values[index], FormatCulture)}"))}"))));
            body.Children.Add(plot); plot.InvalidateVisual();
        }
    }

    private void ApplyDisplay(MultiSelectChange change)
    {
        var keys = series.Select(item => item.Key).ToHashSet(StringComparer.Ordinal);
        if (change.OrderedKeys.Count != keys.Count || change.OrderedKeys.Distinct(StringComparer.Ordinal).Count() != keys.Count
            || !keys.SetEquals(change.OrderedKeys) || !change.SelectedKeys.IsSubsetOf(keys))
            throw new ArgumentException("Display changes must contain every stable series key.", nameof(change));
        order.Clear(); order.AddRange(change.OrderedKeys); hidden.Clear(); hidden.UnionWith(keys.Except(change.SelectedKeys)); Refresh();
        DisplayChanged?.Invoke(this, change);
    }

    internal static (double Minimum, double Maximum) Scale(ChartSeries item, IReadOnlyList<ChartSeries> visible, bool independent)
    {
        var sources = independent ? new[] { item } : visible;
        return (Math.Min(0, sources.SelectMany(series => series.Values).DefaultIfEmpty(0).Min()),
            sources.Select(series => series.Maximum ?? Math.Max(1, series.Values.DefaultIfEmpty(0).Max())).DefaultIfEmpty(1).Max());
    }
    internal static string Format(ChartSeries item, double value, CultureInfo? culture = null) => item.Format?.Invoke(value) ?? value.ToString("N", culture ?? CultureInfo.CurrentCulture);
    private static DependencyProperty Property(string name, bool value) => DependencyProperty.Register(name, typeof(bool), typeof(LineChart), new PropertyMetadata(value, Refresh));
    private static void Refresh(DependencyObject sender, DependencyPropertyChangedEventArgs args) => ((LineChart)sender).Refresh();
    private static void SourceChanged(DependencyObject sender, DependencyPropertyChangedEventArgs args)
    {
        var chart = (LineChart)sender;
        if (args.OldValue is INotifyCollectionChanged old) CollectionChangedEventManager.RemoveHandler(old, chart.CollectionChanged);
        if (args.NewValue is INotifyCollectionChanged current) CollectionChangedEventManager.AddHandler(current, chart.CollectionChanged);
        chart.Refresh();
    }
    private void CollectionChanged(object? sender, NotifyCollectionChangedEventArgs args) => Refresh();
}

internal sealed class ChartPlot : FrameworkElement
{
    internal static readonly string[] ColorKeys = ["Flourish.Brush.Primary", "Flourish.Brush.Accent", "Flourish.Brush.Information", "Flourish.Brush.Danger"];
    private static readonly DependencyProperty[] Colors = Enumerable.Range(0, 4).Select(index => DependencyProperty.Register($"SeriesBrush{index}", typeof(Brush), typeof(ChartPlot), new FrameworkPropertyMetadata(Brushes.Black, FrameworkPropertyMetadataOptions.AffectsRender))).ToArray();
    private static readonly DependencyProperty GridBrushProperty = DependencyProperty.Register("GridBrush", typeof(Brush), typeof(ChartPlot), new FrameworkPropertyMetadata(Brushes.LightGray, FrameworkPropertyMetadataOptions.AffectsRender));
    private static readonly DependencyProperty TextBrushProperty = DependencyProperty.Register("TextBrush", typeof(Brush), typeof(ChartPlot), new FrameworkPropertyMetadata(Brushes.Gray, FrameworkPropertyMetadataOptions.AffectsRender));
    private readonly List<(Point Point, string Tooltip)> points = [];
    internal IReadOnlyList<ChartPointLabel> Labels { get; set; } = [];
    internal IReadOnlyList<ChartSeries> Series { get; set; } = [];
    internal IReadOnlyList<ChartSeries> AllSeries { get; set; } = [];
    internal bool IndependentScales { get; set; }
    internal CultureInfo FormatCulture { get; set; } = CultureInfo.CurrentCulture;

    internal ChartPlot()
    {
        Focusable = true;
        for (var index = 0; index < 4; index++) SetResourceReference(Colors[index], ColorKeys[index]);
        SetResourceReference(GridBrushProperty, "Flourish.Brush.Border"); SetResourceReference(TextBrushProperty, "Flourish.Brush.Muted");
        MouseMove += (_, args) =>
        {
            var location = args.GetPosition(this);
            var closest = points.OrderBy(item => (item.Point - location).LengthSquared).FirstOrDefault();
            ToolTip = (closest.Point - location).Length <= 16 ? closest.Tooltip : null;
        };
        MouseLeave += (_, _) => ToolTip = null;
    }

    protected override void OnRender(DrawingContext drawing)
    {
        base.OnRender(drawing); points.Clear();
        if (ActualWidth <= 0 || ActualHeight <= 0 || Labels.Count == 0 || Series.Count == 0) return;
        var left = 40d; var right = Math.Max(left, ActualWidth - 40); var top = 24d; var bottom = Math.Max(top, ActualHeight - 42);
        drawing.DrawRectangle(Brushes.Transparent, null, new Rect(RenderSize));
        for (var index = 0; index <= 4; index++)
            drawing.DrawLine(new Pen((Brush)GetValue(GridBrushProperty), 1), new Point(left, top + index * (bottom - top) / 4), new Point(right, top + index * (bottom - top) / 4));
        foreach (var series in Series)
        {
            var brush = (Brush)GetValue(Colors[AllSeries.ToList().IndexOf(series) % 4]);
            var scale = LineChart.Scale(series, Series, IndependentScales);
            // Normalize before subtraction so opposite large finite values do not overflow the range.
            var magnitude = Math.Max(Math.Abs(scale.Minimum), Math.Abs(scale.Maximum));
            var minimum = scale.Minimum / magnitude; var maximum = scale.Maximum / magnitude;
            var coordinates = series.Values.Select((value, index) => new Point(Labels.Count == 1 ? (left + right) / 2 : left + index * (right - left) / (Labels.Count - 1),
                top + (1 - (value / magnitude - minimum) / (maximum - minimum)) * (bottom - top))).ToArray();
            var geometry = new StreamGeometry();
            using (var context = geometry.Open()) { context.BeginFigure(coordinates[0], false, false); context.PolyLineTo(coordinates.Skip(1).ToArray(), true, false); }
            geometry.Freeze(); drawing.DrawGeometry(null, new Pen(brush, 3), geometry);
            for (var index = 0; index < coordinates.Length; index++)
            {
                drawing.DrawEllipse(brush, null, coordinates[index], 5, 5);
                points.Add((coordinates[index], $"{series.Label} — {Labels[index].Tooltip ?? Labels[index].Text}: {LineChart.Format(series, series.Values[index], FormatCulture)}"));
            }
        }
        for (var index = 0; index < Labels.Count; index++)
        {
            var text = new FormattedText(Labels[index].Text, FormatCulture, FlowDirection.LeftToRight,
                new Typeface(TryFindResource("Flourish.FontFamily") as FontFamily ?? new FontFamily("Segoe UI"), FontStyles.Normal, FontWeights.Normal, FontStretches.Normal),
                14, (Brush)GetValue(TextBrushProperty), VisualTreeHelper.GetDpi(this).PixelsPerDip);
            var x = Labels.Count == 1 ? (left + right) / 2 : left + index * (right - left) / (Labels.Count - 1);
            drawing.DrawText(text, new Point(Math.Clamp(x - text.Width / 2, 0, Math.Max(0, ActualWidth - text.Width)), bottom + 14));
        }
    }
    protected override AutomationPeer OnCreateAutomationPeer() => new PlotPeer(this);
    private sealed class PlotPeer(ChartPlot owner) : FrameworkElementAutomationPeer(owner)
    {
        protected override AutomationControlType GetAutomationControlTypeCore() => AutomationControlType.Image;
        protected override string GetClassNameCore() => nameof(LineChart);
    }
}
