using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Threading;
using ArkheideSystem.Flourish.WPF.Abstract;

namespace ArkheideSystem.Flourish.WPF.Controls;

/// <summary>One awaited native modal lifecycle for confirmation, keyed views and bottom-sheet presentation.</summary>
public class Dialog : ContentControl, IDisposable
{
    static Dialog() => DefaultStyleKeyProperty.OverrideMetadata(typeof(Dialog), new FrameworkPropertyMetadata(typeof(Dialog)));
    public static readonly DependencyProperty TitleProperty = DependencyProperty.Register(nameof(Title), typeof(string), typeof(Dialog), new PropertyMetadata(""));
    public static readonly DependencyProperty ActionsProperty = DependencyProperty.Register(nameof(Actions), typeof(object), typeof(Dialog), new PropertyMetadata(null));
    public static readonly DependencyProperty BusyProperty = DependencyProperty.Register(nameof(Busy), typeof(bool), typeof(Dialog), new PropertyMetadata(false));
    public static readonly DependencyProperty DismissibleProperty = DependencyProperty.Register(nameof(Dismissible), typeof(bool), typeof(Dialog), new PropertyMetadata(true));
    public static readonly DependencyProperty CloseLabelProperty = DependencyProperty.Register(nameof(CloseLabel), typeof(string), typeof(Dialog), new PropertyMetadata("Close"));
    public static readonly DependencyProperty PresentationProperty = DependencyProperty.Register(nameof(Presentation), typeof(DialogPresentation), typeof(Dialog), new PropertyMetadata(DialogPresentation.Centered));
    public static readonly DependencyProperty ViewProperty = DependencyProperty.Register(nameof(View), typeof(string), typeof(Dialog), new PropertyMetadata("", ViewChanged));
    public static readonly DependencyProperty ViewsProperty = DependencyProperty.Register(nameof(Views), typeof(IEnumerable<DialogView>), typeof(Dialog), new PropertyMetadata(null, ViewChanged));
    public string Title { get => (string)GetValue(TitleProperty); set => SetValue(TitleProperty, value); }
    public object? Actions { get => GetValue(ActionsProperty); set => SetValue(ActionsProperty, value); }
    public bool Busy { get => (bool)GetValue(BusyProperty); set => SetValue(BusyProperty, value); }
    public bool Dismissible { get => (bool)GetValue(DismissibleProperty); set => SetValue(DismissibleProperty, value); }
    public string CloseLabel { get => (string)GetValue(CloseLabelProperty); set => SetValue(CloseLabelProperty, value); }
    public DialogPresentation Presentation { get => (DialogPresentation)GetValue(PresentationProperty); set => SetValue(PresentationProperty, value); }
    public string View { get => (string)GetValue(ViewProperty); set => SetValue(ViewProperty, value); }
    public IEnumerable<DialogView>? Views { get => (IEnumerable<DialogView>?)GetValue(ViewsProperty); set => SetValue(ViewsProperty, value); }
    public Func<Task<bool>>? CanClose { get; set; }
    public bool IsOpen => window is not null;
    public event EventHandler? Opened;
    public event EventHandler? Closed;
    private Window? window;
    private TaskCompletionSource<object?>? completion;
    private CancellationTokenRegistration cancellation;
    private IInputElement? origin;
    private Button? close;
    private Window? closingWindow;
    private bool allowedClose, disposed;
    private AdornerLayer? backdropLayer;
    private Backdrop? backdrop;
    private object? result;
    private static void ViewChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var dialog = (Dialog)d;
        if (dialog.Views is null) return;
        var views = dialog.Views.ToArray();
        if (views.Select(v => v.Key).Distinct(StringComparer.Ordinal).Count() != views.Length) throw new ArgumentException("Dialog view keys must be unique.");
        var selected = views.FirstOrDefault(v => v.Key == dialog.View);
        dialog.SetCurrentValue(ContentProperty, selected?.Content);
    }
    public override void OnApplyTemplate()
    {
        if (close is not null) close.Click -= CloseClick;
        base.OnApplyTemplate(); close = GetTemplateChild("PART_Close") as Button;
        if (close is not null) close.Click += CloseClick;
    }
    private async void CloseClick(object sender, RoutedEventArgs e) { if (Dismissible) await CloseAsync(); }
    public Task<object?> ShowAsync(Window owner, CancellationToken token = default)
    {
        ArgumentNullException.ThrowIfNull(owner); Dispatcher.VerifyAccess();
        ObjectDisposedException.ThrowIf(disposed, this);
        if (IsOpen) throw new InvalidOperationException("This Dialog is already open.");
        if (token.IsCancellationRequested) return Task.FromResult<object?>(null);
        origin = Keyboard.FocusedElement; result = null; allowedClose = false;
        completion = new TaskCompletionSource<object?>(TaskCreationOptions.RunContinuationsAsynchronously);
        var opened = window = new Window { Owner = owner, Content = this, Title = Title, WindowStyle = WindowStyle.None,
            AllowsTransparency = true, Background = Brushes.Transparent,
            ResizeMode = ResizeMode.NoResize, ShowInTaskbar = false, SizeToContent = SizeToContent.Height,
            WindowStartupLocation = WindowStartupLocation.CenterOwner, Width = Math.Max(160, Math.Min(640, owner.ActualWidth - 40)),
            MaxHeight = Math.Max(160, owner.ActualHeight - 40) };
        FrameworkResources.ShareResources(owner, window);
        window.SetResourceReference(Window.ForegroundProperty, "Flourish.Brush.Text");
        if (Presentation == DialogPresentation.BottomSheet)
        {
            window.Width = Math.Max(160, owner.ActualWidth); window.WindowStartupLocation = WindowStartupLocation.Manual;
            window.Left = owner.Left; window.Top = owner.Top + owner.ActualHeight - window.MaxHeight;
            window.SizeChanged += (_, _) => { if (ReferenceEquals(window, opened)) opened.Top = owner.Top + owner.ActualHeight - opened.ActualHeight; };
        }
        window.Closing += WindowClosing;
        window.Closed += WindowClosed;
        window.PreviewKeyDown += (_, e) => { if (e.Key == Key.Escape) { e.Handled = true; if (Dismissible) _ = CloseAsync(); } };
        cancellation = token.Register(() => Dispatcher.BeginInvoke(() => { if (ReferenceEquals(window, opened)) ForceClose(null); }));
        var task = completion.Task;
        Dispatcher.BeginInvoke(DispatcherPriority.Normal, () =>
        {
            if (!ReferenceEquals(window, opened)) return;
            AddBackdrop(owner);
            Opened?.Invoke(this, EventArgs.Empty);
            if (ReferenceEquals(window, opened)) opened.ShowDialog();
        });
        return task;
    }
    public async Task<bool> CloseAsync(object? value = null)
    {
        Dispatcher.VerifyAccess();
        if (window is not { } current || Busy || ReferenceEquals(closingWindow, current)) return false;
        closingWindow = current;
        try
        {
            if (CanClose is not null && !await CanClose()) return false;
            if (!ReferenceEquals(window, current) || Busy) return false;
            ForceClose(value); return true;
        }
        finally { if (ReferenceEquals(closingWindow, current)) closingWindow = null; }
    }
    private void WindowClosing(object? sender, CancelEventArgs e)
    {
        if (allowedClose || disposed) return;
        e.Cancel = true;
        if (!Dismissible) return;
        var current = sender;
        Dispatcher.BeginInvoke(() => { if (ReferenceEquals(window, current)) _ = CloseAsync(); });
    }
    private void ForceClose(object? value)
    {
        if (window is null) return;
        result = value; allowedClose = true; window.Close();
    }
    private void WindowClosed(object? sender, EventArgs e)
    {
        if (sender is Window closed) { closed.Content = null; closed.Closing -= WindowClosing; closed.Closed -= WindowClosed; }
        window = null; cancellation.Dispose();
        if (backdrop is not null) backdropLayer?.Remove(backdrop);
        backdrop = null; backdropLayer = null;
        var pending = completion; completion = null;
        if (origin is UIElement element && element.IsVisible && element.IsEnabled) Keyboard.Focus(origin);
        origin = null; pending?.TrySetResult(result); Closed?.Invoke(this, EventArgs.Empty);
    }
    public void Dispose() { if (disposed) return; Dispatcher.VerifyAccess(); disposed = true; ForceClose(null); cancellation.Dispose(); }
    private void AddBackdrop(Window owner)
    {
        if (owner.Content is not UIElement content || AdornerLayer.GetAdornerLayer(content) is not { } layer) return;
        backdropLayer = layer; backdrop = new Backdrop(content);
        FrameworkResources.ShareResources(owner, backdrop);
        backdrop.SetResourceReference(Backdrop.FillProperty, "Flourish.Brush.Overlay");
        layer.Add(backdrop);
    }
    private sealed class Backdrop(UIElement adorned) : Adorner(adorned)
    {
        public static readonly DependencyProperty FillProperty = DependencyProperty.Register(nameof(Fill), typeof(Brush), typeof(Backdrop), new FrameworkPropertyMetadata(Brushes.Transparent, FrameworkPropertyMetadataOptions.AffectsRender));
        public Brush Fill { get => (Brush)GetValue(FillProperty); set => SetValue(FillProperty, value); }
        protected override void OnRender(DrawingContext dc) => dc.DrawRectangle(Fill, null, new Rect(AdornedElement.RenderSize));
    }
}

public class DropdownSurface : HeaderedContentControl
{
    static DropdownSurface() => DefaultStyleKeyProperty.OverrideMetadata(typeof(DropdownSurface), new FrameworkPropertyMetadata(typeof(DropdownSurface)));
    public static readonly DependencyProperty IsOpenProperty = DependencyProperty.Register(nameof(IsOpen), typeof(bool), typeof(DropdownSurface), new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));
    public bool IsOpen { get => (bool)GetValue(IsOpenProperty); set => SetValue(IsOpenProperty, value); }
    public DropdownSurface() { Unloaded += (_, _) => SetCurrentValue(IsOpenProperty, false); }
    public override void OnApplyTemplate()
    {
        base.OnApplyTemplate();
        if (GetTemplateChild("PART_Popup") is System.Windows.Controls.Primitives.Popup { Child: FrameworkElement child }) FrameworkResources.ShareResources(this, child);
    }
    protected override void OnPreviewKeyDown(KeyEventArgs e)
    {
        if (e.Key == Key.Escape && IsOpen) { SetCurrentValue(IsOpenProperty, false); e.Handled = true; Focus(); }
        base.OnPreviewKeyDown(e);
    }
}
public class NoticeTrigger : Button
{
    static NoticeTrigger() => DefaultStyleKeyProperty.OverrideMetadata(typeof(NoticeTrigger), new FrameworkPropertyMetadata(typeof(NoticeTrigger)));
    public static readonly DependencyProperty NoticeProperty = DependencyProperty.Register(nameof(Notice), typeof(object), typeof(NoticeTrigger), new PropertyMetadata(null, (d,e) => ((NoticeTrigger)d).RefreshNotice()));
    public static readonly DependencyProperty KindProperty = DependencyProperty.Register(nameof(Kind), typeof(NoticeKind), typeof(NoticeTrigger), new PropertyMetadata(NoticeKind.Warning));
    public object? Notice { get => GetValue(NoticeProperty); set => SetValue(NoticeProperty, value); }
    public NoticeKind Kind { get => (NoticeKind)GetValue(KindProperty); set => SetValue(KindProperty, value); }
    private ToolTip? noticeTip;
    public NoticeTrigger()
    {
        System.Windows.Automation.AutomationProperties.SetName(this, Kind.ToString());
        Unloaded += (_, _) => { if (noticeTip is not null) noticeTip.IsOpen = false; };
        IsEnabledChanged += (_, _) => { if (!IsEnabled && noticeTip is not null) noticeTip.IsOpen = false; };
    }
    public override void OnApplyTemplate() { base.OnApplyTemplate(); RefreshNotice(); }
    private void RefreshNotice()
    {
        if (noticeTip is not null) { noticeTip.IsOpen = false; noticeTip.Content = null; }
        noticeTip = Notice is null ? null : new ToolTip { Content = Notice, PlacementTarget = this, StaysOpen = false, MaxWidth = 620, Placement = System.Windows.Controls.Primitives.PlacementMode.Bottom };
        if (noticeTip is not null) { FrameworkResources.ShareResources(this, noticeTip); noticeTip.SetResourceReference(StyleProperty, "Flourish.ToolTip"); }
        SetCurrentValue(ToolTipProperty, noticeTip);
    }
    protected override void OnClick() { base.OnClick(); if (IsEnabled && noticeTip is not null) noticeTip.IsOpen = true; }
    protected override void OnGotKeyboardFocus(KeyboardFocusChangedEventArgs e) { base.OnGotKeyboardFocus(e); if (IsEnabled && noticeTip is not null) noticeTip.IsOpen = true; }
    protected override void OnLostKeyboardFocus(KeyboardFocusChangedEventArgs e) { base.OnLostKeyboardFocus(e); if (noticeTip is not null) noticeTip.IsOpen = false; }
    protected override void OnPreviewKeyDown(KeyEventArgs e) { if (e.Key == Key.Escape && noticeTip?.IsOpen == true) { noticeTip.IsOpen = false; e.Handled = true; } base.OnPreviewKeyDown(e); }
    protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);
        if (e.Property == KindProperty && string.IsNullOrWhiteSpace(Text)) System.Windows.Automation.AutomationProperties.SetName(this, Kind.ToString());
    }
}
