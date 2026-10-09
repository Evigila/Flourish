using System.Windows;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Controls;
using System.Windows.Media;
using System.Globalization;
using Microsoft.Win32;

namespace ArkheideSystem.Flourish.WPF.Controls;

public class ProgressBar : System.Windows.Controls.ProgressBar
{
    static ProgressBar() => DefaultStyleKeyProperty.OverrideMetadata(typeof(ProgressBar), new FrameworkPropertyMetadata(typeof(ProgressBar)));
    public static readonly DependencyProperty StatusProperty = DependencyProperty.Register(nameof(Status), typeof(string), typeof(ProgressBar), new PropertyMetadata(""));
    public static readonly DependencyProperty ShowPercentageProperty = DependencyProperty.Register(nameof(ShowPercentage), typeof(bool), typeof(ProgressBar), new PropertyMetadata(true));
    public string Status { get => (string)GetValue(StatusProperty); set => SetValue(StatusProperty, value); }
    public bool ShowPercentage { get => (bool)GetValue(ShowPercentageProperty); set => SetValue(ShowPercentageProperty, value); }
}

public class ProgressRing : FrameworkElement
{
    public static readonly DependencyProperty ValueProperty = DependencyProperty.Register(nameof(Value), typeof(double), typeof(ProgressRing), new FrameworkPropertyMetadata(0d, FrameworkPropertyMetadataOptions.AffectsRender), value => double.IsFinite((double)value) && (double)value >= 0 && (double)value <= 100);
    public static readonly DependencyProperty IsIndeterminateProperty = DependencyProperty.Register(nameof(IsIndeterminate), typeof(bool), typeof(ProgressRing), new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.AffectsRender));
    public static readonly DependencyProperty IsRunningProperty = DependencyProperty.Register(nameof(IsRunning), typeof(bool), typeof(ProgressRing), new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.AffectsRender));
    public double Value { get => (double)GetValue(ValueProperty); set => SetValue(ValueProperty, value); }
    public bool IsIndeterminate { get => (bool)GetValue(IsIndeterminateProperty); set => SetValue(IsIndeterminateProperty, value); }
    public bool IsRunning { get => (bool)GetValue(IsRunningProperty); set => SetValue(IsRunningProperty, value); }
    private TimeSpan currentTime;
    public ProgressRing() { Width = Height = 72; Loaded += (_, _) => CompositionTarget.Rendering += RenderFrame; Unloaded += (_, _) => CompositionTarget.Rendering -= RenderFrame; }
    private void RenderFrame(object? sender, EventArgs e)
    {
        if (!IsIndeterminate || !IsRunning || !SystemParameters.ClientAreaAnimation || TryFindResource("Flourish.Motion.Reduced") is true) return;
        if (e is RenderingEventArgs frame) currentTime = frame.RenderingTime;
        InvalidateVisual();
    }
    protected override Size MeasureOverride(Size availableSize) => new(72, 72);
    protected override void OnRender(DrawingContext dc)
    {
        base.OnRender(dc);
        var size = Math.Min(RenderSize.Width, RenderSize.Height); if (size <= 0) return;
        var center = new Point(RenderSize.Width / 2, RenderSize.Height / 2); var radius = size / 2 - 5;
        var foreground = TryFindResource("Flourish.Brush.Accent") as Brush ?? SystemColors.HighlightBrush;
        var track = TryFindResource("Flourish.Brush.Border") as Brush ?? SystemColors.GrayTextBrush;
        dc.DrawEllipse(null, new Pen(track, 4.5), center, radius, radius);
        var angle = IsIndeterminate ? .72 * Math.PI : Math.Min(Value, 99.999) / 100 * 2 * Math.PI;
        var rotation = IsIndeterminate && IsRunning && SystemParameters.ClientAreaAnimation && TryFindResource("Flourish.Motion.Reduced") is not true ? currentTime.TotalSeconds / 2.2 * 2 * Math.PI : 0;
        var start = new Point(center.X + radius * Math.Sin(rotation), center.Y - radius * Math.Cos(rotation));
        var end = new Point(center.X + radius * Math.Sin(rotation + angle), center.Y - radius * Math.Cos(rotation + angle));
        var geometry = new StreamGeometry(); using (var context = geometry.Open()) { context.BeginFigure(start, false, false); context.ArcTo(end, new Size(radius, radius), 0, angle > Math.PI, SweepDirection.Clockwise, true, false); }
        dc.DrawGeometry(null, new Pen(foreground, 4.5) { StartLineCap = PenLineCap.Round, EndLineCap = PenLineCap.Round }, geometry);
        if (!IsIndeterminate)
        {
            var text = new FormattedText($"{Value:0}%", CultureInfo.CurrentCulture, FlowDirection, new Typeface(new FontFamily("Segoe UI"), FontStyles.Normal, FontWeights.SemiBold, FontStretches.Normal), 17, foreground, VisualTreeHelper.GetDpi(this).PixelsPerDip);
            dc.DrawText(text, new Point(center.X - text.Width / 2, center.Y - text.Height / 2));
        }
    }
    protected override AutomationPeer OnCreateAutomationPeer() => new ProgressPeer(this);
    private sealed class ProgressPeer(ProgressRing owner) : FrameworkElementAutomationPeer(owner), IRangeValueProvider
    {
        protected override string GetClassNameCore() => nameof(ProgressRing);
        protected override AutomationControlType GetAutomationControlTypeCore() => AutomationControlType.ProgressBar;
        public override object? GetPattern(PatternInterface patternInterface) => patternInterface == PatternInterface.RangeValue && !owner.IsIndeterminate ? this : base.GetPattern(patternInterface);
        public bool IsReadOnly => true;
        public double LargeChange => 0;
        public double SmallChange => 0;
        public double Maximum => 100;
        public double Minimum => 0;
        public double Value => owner.Value;
        public void SetValue(double value) => throw new InvalidOperationException("Progress is read-only.");
    }
}

public class FilePicker : Control
{
    static FilePicker() => DefaultStyleKeyProperty.OverrideMetadata(typeof(FilePicker), new FrameworkPropertyMetadata(typeof(FilePicker)));
    public static readonly DependencyProperty TextProperty = DependencyProperty.Register(nameof(Text), typeof(string), typeof(FilePicker), new PropertyMetadata("Choose files"));
    public string Text { get => (string)GetValue(TextProperty); set => SetValue(TextProperty, value); }
    public string Filter { get; set; } = "All files|*.*";
    public bool Multiple { get; set; }
    public IReadOnlyList<string> SelectedFiles { get; private set; } = [];
    public event EventHandler<IReadOnlyList<string>>? FilesSelected;
    private Button? pick;
    public override void OnApplyTemplate() { if (pick is not null) pick.Click -= Pick; base.OnApplyTemplate(); pick = GetTemplateChild("PART_Pick") as Button; if (pick is not null) pick.Click += Pick; }
    private void Pick(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog { Filter = Filter, Multiselect = Multiple, CheckFileExists = true };
        if (dialog.ShowDialog(Window.GetWindow(this)) != true) return;
        SelectedFiles = Array.AsReadOnly(dialog.FileNames); FilesSelected?.Invoke(this, SelectedFiles);
    }
}
