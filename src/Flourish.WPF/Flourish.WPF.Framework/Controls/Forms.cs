using System.Windows;
using System.Windows.Controls;
using System.Windows.Automation;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows.Markup;

namespace ArkheideSystem.Flourish.WPF.Controls;

public class ValidationMessages : ItemsControl
{
    static ValidationMessages() => DefaultStyleKeyProperty.OverrideMetadata(typeof(ValidationMessages), new FrameworkPropertyMetadata(typeof(ValidationMessages)));
    public static readonly DependencyProperty MessagesProperty = DependencyProperty.Register(nameof(Messages), typeof(IEnumerable<string>), typeof(ValidationMessages), new PropertyMetadata(null, (d, e) => ((ValidationMessages)d).SetCurrentValue(ItemsSourceProperty, e.NewValue)));
    public IEnumerable<string>? Messages { get => (IEnumerable<string>?)GetValue(MessagesProperty); set => SetValue(MessagesProperty, value); }
}
public class Field : ContentControl
{
    static Field() => DefaultStyleKeyProperty.OverrideMetadata(typeof(Field), new FrameworkPropertyMetadata(typeof(Field)));
    public Field()
    {
        Validation.AddErrorHandler(this, (_, _) => UpdateErrors());
        Loaded += (_, _) => { detached = false; Observe(); };
        Unloaded += (_, _) => { detached = true; Detach(); };
    }
    public static readonly DependencyProperty LabelProperty = DependencyProperty.Register(nameof(Label), typeof(string), typeof(Field), new PropertyMetadata(""));
    public static readonly DependencyProperty RequiredProperty = DependencyProperty.Register(nameof(Required), typeof(bool), typeof(Field), new PropertyMetadata(false));
    public static readonly DependencyProperty ErrorsProperty = DependencyProperty.Register(nameof(Errors), typeof(IEnumerable<string>), typeof(Field), new PropertyMetadata(null, (d,e) => ((Field)d).Observe()));
    private static readonly DependencyPropertyKey MessagesPropertyKey = DependencyProperty.RegisterReadOnly(nameof(Messages), typeof(IEnumerable<string>), typeof(Field), new PropertyMetadata(Array.Empty<string>()));
    public static readonly DependencyProperty MessagesProperty = MessagesPropertyKey.DependencyProperty;
    public string Label { get => (string)GetValue(LabelProperty); set => SetValue(LabelProperty, value); }
    public bool Required { get => (bool)GetValue(RequiredProperty); set => SetValue(RequiredProperty, value); }
    public IEnumerable<string>? Errors { get => (IEnumerable<string>?)GetValue(ErrorsProperty); set => SetValue(ErrorsProperty, value); }
    public IEnumerable<string> Messages => (IEnumerable<string>)GetValue(MessagesProperty);
    private DependencyObject? observed;
    private DependencyPropertyDescriptor? descriptor;
    private INotifyCollectionChanged? nativeErrors, externalErrors;
    private bool detached;
    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        Observe();
    }
    protected override void OnContentChanged(object oldContent, object newContent)
    {
        base.OnContentChanged(oldContent, newContent); Observe();
    }
    private void Observe()
    {
        Detach();
        if (Content is DependencyObject input)
        {
            if (GetTemplateChild("PART_Label") is UIElement label) AutomationProperties.SetLabeledBy(input, label);
            if (!detached)
            {
                observed = input;
                descriptor = DependencyPropertyDescriptor.FromProperty(Validation.ErrorsProperty, input.GetType());
                descriptor?.AddValueChanged(input, NativeErrorsChanged);
            }
        }
        if (!detached && Errors is INotifyCollectionChanged collection) { externalErrors = collection; collection.CollectionChanged += ErrorsChanged; }
        UpdateErrors();
    }
    private void NativeErrorsChanged(object? sender, EventArgs e) => UpdateErrors();
    private void ErrorsChanged(object? sender, NotifyCollectionChangedEventArgs e) => UpdateErrors();
    private void Detach()
    {
        if (observed is not null) descriptor?.RemoveValueChanged(observed, NativeErrorsChanged);
        if (nativeErrors is not null) nativeErrors.CollectionChanged -= ErrorsChanged;
        if (externalErrors is not null) externalErrors.CollectionChanged -= ErrorsChanged;
        observed = null; descriptor = null; nativeErrors = null; externalErrors = null;
    }
    private void UpdateErrors()
    {
        var collection = !detached && Content is DependencyObject bound ? (INotifyCollectionChanged)Validation.GetErrors(bound) : null;
        if (!ReferenceEquals(nativeErrors, collection))
        {
            if (nativeErrors is not null) nativeErrors.CollectionChanged -= ErrorsChanged;
            nativeErrors = collection;
            if (nativeErrors is not null) nativeErrors.CollectionChanged += ErrorsChanged;
        }
        var native = Content is DependencyObject input ? Validation.GetErrors(input).Select(error => error.ErrorContent?.ToString() ?? "Invalid value") : [];
        SetValue(MessagesPropertyKey, (Errors ?? []).Concat(native).Distinct(StringComparer.Ordinal).ToArray());
    }
}
[ContentProperty(nameof(Fields))]
public class FormGroup : Card
{
    static FormGroup() => DefaultStyleKeyProperty.OverrideMetadata(typeof(FormGroup), new FrameworkPropertyMetadata(typeof(FormGroup)));
    private readonly FormLayout layout = new() { Columns = 1 };
    public FormGroup() => Content = layout;
    public static readonly DependencyProperty ColumnsProperty = DependencyProperty.Register(nameof(Columns), typeof(int), typeof(FormGroup), new PropertyMetadata(1, (d, e) => ((FormGroup)d).ApplyColumns()), value => (int)value > 0);
    public static readonly DependencyProperty DisabledProperty = DependencyProperty.Register(nameof(Disabled), typeof(bool), typeof(FormGroup), new PropertyMetadata(false, (d, e) => d.CoerceValue(IsEnabledProperty)));
    public int Columns { get => (int)GetValue(ColumnsProperty); set => SetValue(ColumnsProperty, value); }
    public bool Disabled { get => (bool)GetValue(DisabledProperty); set => SetValue(DisabledProperty, value); }
    public UIElementCollection Fields => layout.Children;
    protected override bool IsEnabledCore => base.IsEnabledCore && !Disabled;
    protected override void OnContentChanged(object oldContent, object newContent) { base.OnContentChanged(oldContent, newContent); ApplyColumns(); }
    private void ApplyColumns() { if (Content is FormLayout current) current.SetCurrentValue(FormLayout.ColumnsProperty, Columns); }
}
public class InlineActions : WrapPanel
{
    public InlineActions() { HorizontalAlignment = HorizontalAlignment.Center; Margin = new Thickness(0, 16, 0, 0); }
    protected override Size MeasureOverride(Size constraint)
    {
        foreach (UIElement child in InternalChildren) if (child is FrameworkElement element && element.ReadLocalValue(MarginProperty) == DependencyProperty.UnsetValue) element.Margin = new Thickness(0, 0, 12, 12);
        return base.MeasureOverride(constraint);
    }
}
public class FormActions : UniformGrid
{
    public FormActions() { Columns = 2; NarrowColumns = 2; CellHeight = 96; Variant = Abstract.UniformGridVariant.Filled; Margin = new Thickness(0, 44, 0, 0); }
    protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);
        if (e.Property == ColumnsProperty)
        {
            if (Columns is < 1 or > 5) throw new ArgumentOutOfRangeException(nameof(Columns));
            SetCurrentValue(NarrowColumnsProperty, Math.Min(2, Columns));
        }
    }
}
public class FormLayout : Panel
{
    public static readonly DependencyProperty ColumnsProperty = DependencyProperty.Register(nameof(Columns), typeof(int), typeof(FormLayout), new FrameworkPropertyMetadata(2, FrameworkPropertyMetadataOptions.AffectsMeasure), v => (int)v > 0);
    public int Columns { get => (int)GetValue(ColumnsProperty); set => SetValue(ColumnsProperty, value); }
    public static readonly DependencyProperty BreakpointProperty = DependencyProperty.Register(nameof(Breakpoint), typeof(double), typeof(FormLayout), new FrameworkPropertyMetadata(760d, FrameworkPropertyMetadataOptions.AffectsMeasure));
    public static readonly DependencyProperty OccupyEmptyColumnsProperty = DependencyProperty.Register(nameof(OccupyEmptyColumns), typeof(bool), typeof(FormLayout), new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.AffectsMeasure));
    public double Breakpoint { get => (double)GetValue(BreakpointProperty); set => SetValue(BreakpointProperty, value); }
    public bool OccupyEmptyColumns { get => (bool)GetValue(OccupyEmptyColumnsProperty); set => SetValue(OccupyEmptyColumnsProperty, value); }
    private double[] heights = [];
    private UIElement[] children = [];
    private int CountColumns(double width) => width <= Breakpoint ? 1 : OccupyEmptyColumns ? Columns : Math.Max(1, Math.Min(Columns, children.Length));
    protected override Size MeasureOverride(Size available)
    {
        children = InternalChildren.Cast<UIElement>().Where(c => c.Visibility != Visibility.Collapsed).ToArray();
        var columns = CountColumns(available.Width); var width = double.IsFinite(available.Width) ? available.Width : 720;
        var cellWidth = Math.Max(0, (width - 24 * (columns - 1)) / columns);
        heights = new double[(children.Length + columns - 1) / columns];
        for (var i = 0; i < children.Length; i++) { children[i].Measure(new Size(cellWidth, double.PositiveInfinity)); heights[i / columns] = Math.Max(heights[i / columns], children[i].DesiredSize.Height); }
        return new Size(width, heights.Sum() + Math.Max(0, heights.Length - 1) * 24);
    }
    protected override Size ArrangeOverride(Size finalSize)
    {
        var columns = CountColumns(finalSize.Width); var width = Math.Max(0, (finalSize.Width - 24 * (columns - 1)) / columns); double top = 0;
        for (var i = 0; i < children.Length; i++) { var row = i / columns; children[i].Arrange(new Rect(i % columns * (width + 24), top, width, heights.ElementAtOrDefault(row))); if (i % columns == columns - 1) top += heights.ElementAtOrDefault(row) + 24; }
        return finalSize;
    }
}
