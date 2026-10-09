using System.Collections.Specialized;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;

namespace ArkheideSystem.Flourish.WPF.Controls;

/// <summary>One native offer controller: retained cards expand on hover/focus; rotation never moves focus.</summary>
public class OfferStage : ContentControl
{
    private readonly OfferLayout panel = new();
    private readonly Button rotation = new() { Text = "Pause rotation", Variant = Abstract.ButtonVariant.Quiet, HorizontalAlignment = HorizontalAlignment.Right, Margin = new Thickness(0, 12, 0, 0) };
    private readonly DispatcherTimer timer = new();
    private OfferCard[] cards = [];
    private int index;
    public OfferStage()
    {
        var layout = new StackPanel(); layout.Children.Add(panel); layout.Children.Add(rotation); Content = layout;
        rotation.Click += (_, _) => { if (IsPaused) ResumeRotation(); else PauseRotation(); };
        timer.Tick += (_, _) => { if (CanRotate) Activate((index + 1) % cards.Length); };
        Loaded += (_, _) => UpdateMode(); Unloaded += (_, _) => timer.Stop(); SizeChanged += (_, _) => UpdateMode();
        MouseEnter += (_, _) => UpdateTimer(); MouseLeave += (_, _) => UpdateTimer();
        GotKeyboardFocus += (_, _) => UpdateTimer(); LostKeyboardFocus += (_, _) => Dispatcher.BeginInvoke(UpdateTimer, DispatcherPriority.Input);
        PreviewKeyDown += OnKeyDown;
        SetResourceReference(Motion.ReducedProperty, "Flourish.Motion.Reduced");
        AutomationProperties.SetName(this, "Offers");
    }
    public static readonly DependencyProperty OffersProperty = DependencyProperty.Register(nameof(Offers), typeof(IEnumerable<OfferCard>), typeof(OfferStage), new PropertyMetadata(null, (d, e) =>
    {
        var stage = (OfferStage)d;
        if (e.OldValue is INotifyCollectionChanged previous) CollectionChangedEventManager.RemoveHandler(previous, stage.OffersChanged);
        if (e.NewValue is INotifyCollectionChanged current) CollectionChangedEventManager.AddHandler(current, stage.OffersChanged);
        stage.Refresh();
    }));
    public static readonly DependencyProperty AutoRotateProperty = DependencyProperty.Register(nameof(AutoRotate), typeof(bool), typeof(OfferStage), new PropertyMetadata(true, (d, e) => ((OfferStage)d).UpdateTimer()));
    public static readonly DependencyProperty RotationIntervalMillisecondsProperty = DependencyProperty.Register(nameof(RotationIntervalMilliseconds), typeof(int), typeof(OfferStage), new PropertyMetadata(2200, (d, e) => ((OfferStage)d).UpdateTimer()), value => (int)value >= 1000);
    public static readonly DependencyProperty PauseRotationLabelProperty = DependencyProperty.Register(nameof(PauseRotationLabel), typeof(string), typeof(OfferStage), new PropertyMetadata("Pause rotation", (d, e) => ((OfferStage)d).UpdateTimer()));
    public static readonly DependencyProperty ResumeRotationLabelProperty = DependencyProperty.Register(nameof(ResumeRotationLabel), typeof(string), typeof(OfferStage), new PropertyMetadata("Resume rotation", (d, e) => ((OfferStage)d).UpdateTimer()));
    public static readonly DependencyProperty RotationControlIconOnlyProperty = DependencyProperty.Register(nameof(RotationControlIconOnly), typeof(bool), typeof(OfferStage), new PropertyMetadata(false, (d, e) => ((OfferStage)d).UpdateTimer()));
    private static readonly DependencyPropertyKey IsEnhancedPropertyKey = DependencyProperty.RegisterReadOnly(nameof(IsEnhanced), typeof(bool), typeof(OfferStage), new PropertyMetadata(false));
    public static readonly DependencyProperty IsEnhancedProperty = IsEnhancedPropertyKey.DependencyProperty;
    public IEnumerable<OfferCard>? Offers { get => (IEnumerable<OfferCard>?)GetValue(OffersProperty); set => SetValue(OffersProperty, value); }
    public bool AutoRotate { get => (bool)GetValue(AutoRotateProperty); set => SetValue(AutoRotateProperty, value); }
    public int RotationIntervalMilliseconds { get => (int)GetValue(RotationIntervalMillisecondsProperty); set => SetValue(RotationIntervalMillisecondsProperty, value); }
    public string PauseRotationLabel { get => (string)GetValue(PauseRotationLabelProperty); set => SetValue(PauseRotationLabelProperty, value); }
    public string ResumeRotationLabel { get => (string)GetValue(ResumeRotationLabelProperty); set => SetValue(ResumeRotationLabelProperty, value); }
    public bool RotationControlIconOnly { get => (bool)GetValue(RotationControlIconOnlyProperty); set => SetValue(RotationControlIconOnlyProperty, value); }
    public bool IsEnhanced => (bool)GetValue(IsEnhancedProperty);
    public bool IsPaused { get; private set; }
    public string? ActiveId => cards.ElementAtOrDefault(index)?.Id;
    public event EventHandler<string>? ActiveChanged;
    private bool CanRotate => IsLoaded && IsEnhanced && AutoRotate && !IsPaused && cards.Length > 1 && !IsMouseOver && !IsKeyboardFocusWithin;
    protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e) { base.OnPropertyChanged(e); if (e.Property == Motion.ReducedProperty) UpdateMode(); }
    private void OffersChanged(object? sender, NotifyCollectionChangedEventArgs args) => Refresh();
    private void Refresh()
    {
        var active = ActiveId;
        var values = Offers?.ToArray() ?? [];
        if (values.Any(card => string.IsNullOrWhiteSpace(card.Id) || card.Id.Any(char.IsWhiteSpace)) || values.Select(card => card.Id).Distinct(StringComparer.Ordinal).Count() != values.Length)
            throw new ArgumentException("OfferStage requires unique nonempty card Ids without whitespace.");
        foreach (var card in cards) { card.MouseEnter -= CardEntered; card.GotKeyboardFocus -= CardFocused; card.IsEnhanced = false; card.IsActive = false; }
        cards = values; panel.Children.Clear();
        foreach (var card in cards)
        {
            card.Focusable = true; card.MouseEnter += CardEntered; card.GotKeyboardFocus += CardFocused;
            AutomationProperties.SetName(card, card.Title); panel.Children.Add(card);
        }
        index = Math.Max(0, Array.FindIndex(cards, card => card.Id == active));
        for (var i = 0; i < cards.Length; i++) cards[i].IsActive = i == index;
        UpdateMode();
        if (ActiveId is { } current && current != active) ActiveChanged?.Invoke(this, current);
    }
    private void CardEntered(object sender, MouseEventArgs e)
    {
        if (sender is OfferCard card && !cards.Any(item => item.IsKeyboardFocusWithin)) Activate(Array.IndexOf(cards, card));
    }
    private void CardFocused(object sender, KeyboardFocusChangedEventArgs e) { if (sender is OfferCard card) Activate(Array.IndexOf(cards, card)); }
    public bool Activate(string id) { var target = Array.FindIndex(cards, card => card.Id == id); if (target < 0) return false; Activate(target); return true; }
    private void Activate(int target)
    {
        if (target < 0 || target >= cards.Length) return;
        var changed = index != target; index = target;
        for (var i = 0; i < cards.Length; i++) cards[i].IsActive = i == index;
        panel.InvalidateMeasure();
        if (changed && ActiveId is { } id) ActiveChanged?.Invoke(this, id);
    }
    private void OnKeyDown(object sender, KeyEventArgs args)
    {
        if (Keyboard.FocusedElement is not OfferCard focused || cards.Length == 0) return;
        var position = Array.IndexOf(cards, focused);
        var target = args.Key switch { Key.Right or Key.Down => (position + 1) % cards.Length, Key.Left or Key.Up => (position + cards.Length - 1) % cards.Length, Key.Home => 0, Key.End => cards.Length - 1, _ => -1 };
        if (target < 0) return;
        Activate(target); cards[target].Focus(); args.Handled = true;
    }
    public void PauseRotation() { IsPaused = true; UpdateTimer(); }
    public void ResumeRotation() { IsPaused = false; UpdateTimer(); }
    private void UpdateMode()
    {
        var enhanced = ActualWidth >= 1100 && !Motion.GetReduced(this);
        SetValue(IsEnhancedPropertyKey, enhanced); panel.Enhanced = enhanced;
        foreach (var card in cards) card.IsEnhanced = enhanced;
        UpdateTimer();
    }
    private void UpdateTimer()
    {
        timer.Stop(); timer.Interval = TimeSpan.FromMilliseconds(RotationIntervalMilliseconds);
        rotation.Visibility = AutoRotate && IsEnhanced ? Visibility.Visible : Visibility.Collapsed;
        var caption = IsPaused ? ResumeRotationLabel : PauseRotationLabel;
        rotation.Text = RotationControlIconOnly ? "" : caption;
        rotation.Icon = RotationControlIconOnly ? IsPaused ? "play_arrow" : "pause" : "";
        rotation.ToolTip = caption; AutomationProperties.SetName(rotation, caption);
        if (CanRotate) timer.Start();
    }

    private sealed class OfferLayout : Panel
    {
        private bool enhanced;
        public bool Enhanced { get => enhanced; set { if (value == enhanced) return; enhanced = value; InvalidateMeasure(); } }
        protected override Size MeasureOverride(Size available)
        {
            var width = double.IsFinite(available.Width) ? available.Width : 1180;
            if (Enhanced)
            {
                var tracks = Math.Max(1, InternalChildren.Count + 1); var track = Math.Max(0, (width - (InternalChildren.Count - 1) * 10) / tracks);
                foreach (OfferCard card in InternalChildren) card.Measure(new Size(track * (card.IsActive ? 2 : 1), 360));
                return new Size(width, InternalChildren.Count > 0 ? 360 : 0);
            }
            var columns = width <= 600 ? 1 : 2; var cell = Math.Max(0, (width - (columns - 1) * 10) / columns); var height = 0d;
            for (var row = 0; row * columns < InternalChildren.Count; row++)
            {
                var rowHeight = 0d;
                for (var col = 0; col < columns && row * columns + col < InternalChildren.Count; col++)
                { var child = InternalChildren[row * columns + col]; child.Measure(new Size(cell, double.PositiveInfinity)); rowHeight = Math.Max(rowHeight, child.DesiredSize.Height); }
                height += rowHeight + (row > 0 ? 10 : 0);
            }
            return new Size(width, height);
        }
        protected override Size ArrangeOverride(Size finalSize)
        {
            if (Enhanced)
            {
                var tracks = Math.Max(1, InternalChildren.Count + 1); var track = Math.Max(0, (finalSize.Width - (InternalChildren.Count - 1) * 10) / tracks); var left = 0d;
                foreach (OfferCard card in InternalChildren) { var width = track * (card.IsActive ? 2 : 1); card.Arrange(new Rect(left, 0, width, 360)); left += width + 10; }
            }
            else
            {
                var columns = finalSize.Width <= 600 ? 1 : 2; var cell = Math.Max(0, (finalSize.Width - (columns - 1) * 10) / columns); var top = 0d;
                for (var row = 0; row * columns < InternalChildren.Count; row++)
                {
                    var rowHeight = 0d;
                    for (var col = 0; col < columns && row * columns + col < InternalChildren.Count; col++) rowHeight = Math.Max(rowHeight, InternalChildren[row * columns + col].DesiredSize.Height);
                    for (var col = 0; col < columns && row * columns + col < InternalChildren.Count; col++) InternalChildren[row * columns + col].Arrange(new Rect(col * (cell + 10), top, cell, rowHeight));
                    top += rowHeight + 10;
                }
            }
            return finalSize;
        }
    }
}
