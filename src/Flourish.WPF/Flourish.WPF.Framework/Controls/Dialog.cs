using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Controls.Primitives;
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
    public static readonly DependencyProperty StrengthProperty = DependencyProperty.Register(nameof(Strength), typeof(PopupStrength), typeof(Dialog), new PropertyMetadata(PopupStrength.Weak), value => Enum.IsDefined((PopupStrength)value));
    public static readonly DependencyProperty DismissibleProperty = DependencyProperty.Register(nameof(Dismissible), typeof(bool), typeof(Dialog), new PropertyMetadata(true));
    public static readonly DependencyProperty CloseLabelProperty = DependencyProperty.Register(nameof(CloseLabel), typeof(string), typeof(Dialog), new PropertyMetadata("Close"));
    public static readonly DependencyProperty PresentationProperty = DependencyProperty.Register(nameof(Presentation), typeof(DialogPresentation), typeof(Dialog), new PropertyMetadata(DialogPresentation.Centered));
    public static readonly DependencyProperty ViewProperty = DependencyProperty.Register(nameof(View), typeof(string), typeof(Dialog), new PropertyMetadata("", ViewChanged));
    public static readonly DependencyProperty ViewsProperty = DependencyProperty.Register(nameof(Views), typeof(IEnumerable<DialogView>), typeof(Dialog), new PropertyMetadata(null, ViewChanged));
    public string Title { get => (string)GetValue(TitleProperty); set => SetValue(TitleProperty, value); }
    public object? Actions { get => GetValue(ActionsProperty); set => SetValue(ActionsProperty, value); }
    public bool Busy { get => (bool)GetValue(BusyProperty); set => SetValue(BusyProperty, value); }
    public PopupStrength Strength { get => (PopupStrength)GetValue(StrengthProperty); set => SetValue(StrengthProperty, value); }
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
    private Border? dialogSurface;
    private TaskCompletionSource<object?>? completion;
    private CancellationTokenRegistration cancellation;
    private IInputElement? origin;
    private Button? close;
    private Window? closingWindow;
    private bool allowedClose, disposed;
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
        // One modal owner-sized surface receives backdrop input; a disabled owner cannot receive outside clicks.
        var overlay = new Grid();
        overlay.SetResourceReference(Panel.BackgroundProperty, "Flourish.Brush.Overlay");
        var surface = dialogSurface = new Border { Child = this, VerticalAlignment = Presentation == DialogPresentation.BottomSheet ? VerticalAlignment.Bottom : VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Center, Width = Presentation == DialogPresentation.BottomSheet ? Math.Max(160, owner.ActualWidth) : Math.Max(160, Math.Min(640, owner.ActualWidth - 40)),
            MaxHeight = Math.Max(160, owner.ActualHeight - 40) };
        overlay.Children.Add(surface);
        overlay.MouseDown += (_, e) =>
        {
            if (ReferenceEquals(e.OriginalSource, overlay) && Strength == PopupStrength.Weak && Dismissible) { e.Handled = true; _ = CloseAsync(); }
        };
        var opened = window = new Window { Owner = owner, Content = overlay, Title = Title, WindowStyle = WindowStyle.None,
            AllowsTransparency = true, Background = Brushes.Transparent,
            ResizeMode = ResizeMode.NoResize, ShowInTaskbar = false, WindowStartupLocation = WindowStartupLocation.Manual,
            Left = owner.Left, Top = owner.Top, Width = Math.Max(160, owner.ActualWidth), Height = Math.Max(160, owner.ActualHeight) };
        FrameworkResources.ShareResources(owner, window);
        window.SetResourceReference(Window.ForegroundProperty, "Flourish.Brush.Text");
        window.Closing += WindowClosing;
        window.Closed += WindowClosed;
        window.PreviewKeyDown += (_, e) => { if (e.Key == Key.Escape) { e.Handled = true; if (Strength == PopupStrength.Weak && Dismissible) _ = CloseAsync(); } };
        cancellation = token.Register(() => Dispatcher.BeginInvoke(() => { if (ReferenceEquals(window, opened)) ForceClose(null); }));
        var task = completion.Task;
        Dispatcher.BeginInvoke(DispatcherPriority.Normal, () =>
        {
            if (!ReferenceEquals(window, opened)) return;
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
        if (Strength == PopupStrength.Strong || !Dismissible) return;
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
        if (dialogSurface is not null) dialogSurface.Child = null;
        dialogSurface = null;
        window = null; cancellation.Dispose();
        var pending = completion; completion = null;
        if (origin is UIElement element && element.IsVisible && element.IsEnabled) Keyboard.Focus(origin);
        origin = null; pending?.TrySetResult(result); Closed?.Invoke(this, EventArgs.Empty);
    }
    public void Dispose() { if (disposed) return; Dispatcher.VerifyAccess(); disposed = true; ForceClose(null); cancellation.Dispose(); }
}

public class DropdownSurface : HeaderedContentControl
{
    static DropdownSurface() => DefaultStyleKeyProperty.OverrideMetadata(typeof(DropdownSurface), new FrameworkPropertyMetadata(typeof(DropdownSurface)));
    public static readonly DependencyProperty IsOpenProperty = DependencyProperty.Register(nameof(IsOpen), typeof(bool), typeof(DropdownSurface), new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault));
    public static readonly DependencyProperty StrengthProperty = DependencyProperty.Register(nameof(Strength), typeof(PopupStrength), typeof(DropdownSurface), new PropertyMetadata(PopupStrength.Weak, (d, e) => ((DropdownSurface)d).UpdateStrength()), value => Enum.IsDefined((PopupStrength)value));
    public bool IsOpen { get => (bool)GetValue(IsOpenProperty); set => SetValue(IsOpenProperty, value); }
    public PopupStrength Strength { get => (PopupStrength)GetValue(StrengthProperty); set => SetValue(StrengthProperty, value); }
    private Popup? popup;
    public DropdownSurface()
    {
        Unloaded += (_, _) => SetCurrentValue(IsOpenProperty, false);
        IsEnabledChanged += (_, _) => { if (!IsEnabled) SetCurrentValue(IsOpenProperty, false); };
    }
    public override void OnApplyTemplate()
    {
        if (popup?.Child is UIElement previous) previous.PreviewKeyDown -= PopupKeyDown;
        base.OnApplyTemplate();
        popup = GetTemplateChild("PART_Popup") as Popup;
        if (popup?.Child is FrameworkElement child) { FrameworkResources.ShareResources(this, child); child.PreviewKeyDown += PopupKeyDown; }
        UpdateStrength();
    }
    private void UpdateStrength() { if (popup is not null) popup.SetCurrentValue(Popup.StaysOpenProperty, Strength == PopupStrength.Strong); }
    private void PopupKeyDown(object sender, KeyEventArgs e) => HandleDismissKey(e);
    private void HandleDismissKey(KeyEventArgs e)
    {
        if (e.Key != Key.Escape || !IsOpen) return;
        e.Handled = true;
        if (Strength == PopupStrength.Weak) Close();
    }
    /// <summary>Explicit completion or cancellation; preserves the IsOpen binding.</summary>
    public void Close() { SetCurrentValue(IsOpenProperty, false); Focus(); }
    protected override void OnPreviewKeyDown(KeyEventArgs e) { HandleDismissKey(e); base.OnPreviewKeyDown(e); }
}
/// <summary>An owned notice surface can contain ordinary Card actions; passive native tooltips remain separate.</summary>
public class NoticeTrigger : Button
{
    static NoticeTrigger() => DefaultStyleKeyProperty.OverrideMetadata(typeof(NoticeTrigger), new FrameworkPropertyMetadata(typeof(NoticeTrigger)));
    public static readonly DependencyProperty NoticeProperty = DependencyProperty.Register(nameof(Notice), typeof(object), typeof(NoticeTrigger), new PropertyMetadata(null, (d,e) => ((NoticeTrigger)d).RefreshNotice()));
    public static readonly DependencyProperty KindProperty = DependencyProperty.Register(nameof(Kind), typeof(NoticeKind), typeof(NoticeTrigger), new PropertyMetadata(NoticeKind.Warning));
    public object? Notice { get => GetValue(NoticeProperty); set => SetValue(NoticeProperty, value); }
    public NoticeKind Kind { get => (NoticeKind)GetValue(KindProperty); set => SetValue(KindProperty, value); }
    public static readonly DependencyProperty StrengthProperty = DependencyProperty.Register(nameof(Strength), typeof(PopupStrength), typeof(NoticeTrigger), new PropertyMetadata(PopupStrength.Weak, StrengthChanged), value => Enum.IsDefined((PopupStrength)value));
    public PopupStrength Strength { get => (PopupStrength)GetValue(StrengthProperty); set => SetValue(StrengthProperty, value); }
    public bool IsOpen => noticePopup?.IsOpen == true;
    private Popup? noticePopup;
    private bool restoringFocus;
    private readonly DispatcherTimer hoverOpen = new();
    private readonly DispatcherTimer hoverClose = new() { Interval = TimeSpan.FromMilliseconds(160) };
    private static void StrengthChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var owner = (NoticeTrigger)d; owner.hoverClose.Stop();
        if (owner.noticePopup is not null) owner.noticePopup.SetCurrentValue(Popup.StaysOpenProperty, owner.Strength == PopupStrength.Strong);
    }
    public NoticeTrigger()
    {
        System.Windows.Automation.AutomationProperties.SetName(this, Kind.ToString());
        hoverOpen.Tick += (_, _) => { hoverOpen.Stop(); if (IsMouseOver) Open(); };
        hoverClose.Tick += (_, _) => { hoverClose.Stop(); if (Strength == PopupStrength.Weak && !IsMouseOver && noticePopup?.Child is UIElement { IsMouseOver: false }) Close(); };
        MouseEnter += (_, _) => { hoverClose.Stop(); if (!IsOpen) { hoverOpen.Interval = TimeSpan.FromMilliseconds(ToolTipService.GetInitialShowDelay(this)); hoverOpen.Start(); } };
        MouseLeave += (_, _) => { hoverOpen.Stop(); if (Strength == PopupStrength.Weak) hoverClose.Start(); };
        Unloaded += (_, _) => Close();
        IsEnabledChanged += (_, _) => { if (!IsEnabled) Close(); };
    }
    public override void OnApplyTemplate() { base.OnApplyTemplate(); RefreshNotice(); }
    private void RefreshNotice()
    {
        Close();
        if (noticePopup?.Child is ContentControl previous) previous.Content = null;
        if (noticePopup is not null) noticePopup.Child = null;
        noticePopup = null;
        SetCurrentValue(ToolTipProperty, null);
        if (Notice is null) return;
        var content = new ContentControl { Content = Notice, MaxWidth = 620 };
        FrameworkResources.ShareResources(this, content);
        content.SetResourceReference(StyleProperty, "Flourish.NoticePopupSurface");
        noticePopup = new Popup { Child = content, PlacementTarget = this, Placement = PlacementMode.Bottom,
            AllowsTransparency = true, StaysOpen = Strength == PopupStrength.Strong };
        content.MouseEnter += (_, _) => hoverClose.Stop();
        content.MouseLeave += (_, _) => { if (Strength == PopupStrength.Weak) hoverClose.Start(); };
        content.PreviewKeyDown += PopupKeyDown;
    }
    public void Open() { hoverOpen.Stop(); hoverClose.Stop(); if (IsEnabled && noticePopup is not null) noticePopup.SetCurrentValue(Popup.IsOpenProperty, true); }
    /// <summary>Explicit completion or cancellation of notice actions.</summary>
    public void Close()
    {
        hoverOpen.Stop(); hoverClose.Stop();
        var restore = noticePopup?.Child is UIElement { IsKeyboardFocusWithin: true };
        if (noticePopup is not null) noticePopup.SetCurrentValue(Popup.IsOpenProperty, false);
        if (!restore || !IsVisible || !IsEnabled) return;
        restoringFocus = true;
        try { Focus(); }
        finally { restoringFocus = false; }
    }
    private void PopupKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key != Key.Escape || !IsOpen) return;
        e.Handled = true;
        if (Strength == PopupStrength.Weak) Close();
    }
    protected override void OnClick() { base.OnClick(); Open(); }
    protected override void OnGotKeyboardFocus(KeyboardFocusChangedEventArgs e) { base.OnGotKeyboardFocus(e); if (!restoringFocus) Open(); }
    protected override void OnLostKeyboardFocus(KeyboardFocusChangedEventArgs e)
    {
        base.OnLostKeyboardFocus(e);
        if (Strength == PopupStrength.Strong) return;
        Dispatcher.BeginInvoke(DispatcherPriority.Input, () =>
        {
            if (Strength == PopupStrength.Weak && !IsKeyboardFocusWithin && noticePopup?.Child is UIElement { IsKeyboardFocusWithin: false }) Close();
        });
    }
    protected override void OnPreviewKeyDown(KeyEventArgs e) { PopupKeyDown(this, e); base.OnPreviewKeyDown(e); }
    protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);
        if (e.Property == KindProperty && string.IsNullOrWhiteSpace(Text)) System.Windows.Automation.AutomationProperties.SetName(this, Kind.ToString());
    }
}
