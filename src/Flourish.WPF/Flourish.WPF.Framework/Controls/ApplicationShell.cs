using System.Windows;
using System.Windows.Controls;
using System.Windows.Automation;
using System.Windows.Input;
using ArkheideSystem.Flourish.WPF.Abstract;

namespace ArkheideSystem.Flourish.WPF.Controls;

public class NavigationGuard : ContentControl
{
    public Func<string, Task<bool>>? CanNavigate { get; set; }
    public Task<bool> CheckAsync(string route) => CanNavigate?.Invoke(route) ?? Task.FromResult(true);
}
public class ApplicationShell : ContentControl
{
    static ApplicationShell() => DefaultStyleKeyProperty.OverrideMetadata(typeof(ApplicationShell), new FrameworkPropertyMetadata(typeof(ApplicationShell)));
    public ApplicationShell()
    {
        Loaded += (_, _) => { Subscribe(); Refresh(); };
        Unloaded += (_, _) => { SetCurrentValue(NavigationOpenProperty, false); Unsubscribe(); };
        SizeChanged += (_, _) => UpdateLayoutState();
    }
    public static readonly DependencyProperty OptionsProperty = DependencyProperty.Register(nameof(Options), typeof(FrameworkOptions), typeof(ApplicationShell), new PropertyMetadata(null, OptionsChanged));
    public static readonly DependencyProperty NavigationOpenProperty = DependencyProperty.Register(nameof(NavigationOpen), typeof(bool), typeof(ApplicationShell), new PropertyMetadata(false, NavigationStateChanged));
    private static readonly DependencyPropertyKey IsNarrowPropertyKey = DependencyProperty.RegisterReadOnly(nameof(IsNarrow), typeof(bool), typeof(ApplicationShell), new PropertyMetadata(false));
    private static readonly DependencyPropertyKey IsCompactPropertyKey = DependencyProperty.RegisterReadOnly(nameof(IsCompact), typeof(bool), typeof(ApplicationShell), new PropertyMetadata(false));
    private static readonly DependencyPropertyKey HasNavigationPropertyKey = DependencyProperty.RegisterReadOnly(nameof(HasNavigation), typeof(bool), typeof(ApplicationShell), new PropertyMetadata(false));
    private static readonly DependencyPropertyKey HasSecondaryPropertyKey = DependencyProperty.RegisterReadOnly(nameof(HasSecondary), typeof(bool), typeof(ApplicationShell), new PropertyMetadata(false));
    private static readonly DependencyPropertyKey HasNavigationTreePropertyKey = DependencyProperty.RegisterReadOnly(nameof(HasNavigationTree), typeof(bool), typeof(ApplicationShell), new PropertyMetadata(false));
    public static readonly DependencyProperty IsNarrowProperty = IsNarrowPropertyKey.DependencyProperty;
    public static readonly DependencyProperty IsCompactProperty = IsCompactPropertyKey.DependencyProperty;
    public static readonly DependencyProperty HasNavigationProperty = HasNavigationPropertyKey.DependencyProperty;
    public static readonly DependencyProperty HasSecondaryProperty = HasSecondaryPropertyKey.DependencyProperty;
    public static readonly DependencyProperty HasNavigationTreeProperty = HasNavigationTreePropertyKey.DependencyProperty;
    public FrameworkOptions? Options { get => (FrameworkOptions?)GetValue(OptionsProperty); set => SetValue(OptionsProperty, value); }
    /// <summary>Opens the secondary overlay in the narrow layout; desktop navigation remains visible.</summary>
    public bool NavigationOpen { get => (bool)GetValue(NavigationOpenProperty); set => SetValue(NavigationOpenProperty, value); }
    public bool IsNarrow => (bool)GetValue(IsNarrowProperty);
    public bool IsCompact => (bool)GetValue(IsCompactProperty);
    public bool HasNavigation => (bool)GetValue(HasNavigationProperty);
    public bool HasSecondary => (bool)GetValue(HasSecondaryProperty);
    public bool HasNavigationTree => (bool)GetValue(HasNavigationTreeProperty);
    public string? SelectedRoute { get; private set; }
    public NavigationGuard? Guard { get; set; }
    public event EventHandler<string>? Navigated;
    private StackPanel? primary, secondary, topRight;
    private Border? secondaryHost, logoHost;
    private System.Windows.Controls.TextBlock? title;
    private Image? logo;
    private Button? navigationToggle, navigationBackdrop;
    private IInputElement? returnFocus;
    private ITextProvider? subscribed;
    private long revision;
    private readonly Dictionary<string, bool> expanded = new(StringComparer.Ordinal);
    private static void OptionsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    { var shell = (ApplicationShell)d; shell.revision++; shell.Unsubscribe(); if (shell.IsLoaded) shell.Subscribe(); shell.Refresh(); }
    private static void NavigationStateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var shell = (ApplicationShell)d;
        shell.RefreshLabels();
        if (shell.IsNarrow && shell.NavigationOpen && shell.HasSecondary)
        {
            shell.returnFocus ??= Keyboard.FocusedElement;
            shell.Dispatcher.InvokeAsync(() =>
            {
                if (shell.IsNarrow && shell.NavigationOpen) shell.NavigationTargets().OfType<SecondaryNavigationItem>().FirstOrDefault()?.Focus();
            }, System.Windows.Threading.DispatcherPriority.Input);
        }
        else if (shell.returnFocus is { } target)
        {
            shell.returnFocus = null;
            Keyboard.Focus(target);
        }
    }
    public override void OnApplyTemplate()
    {
        if (navigationToggle is not null) navigationToggle.Click -= ToggleNavigation;
        if (navigationBackdrop is not null) navigationBackdrop.Click -= CloseNavigation;
        base.OnApplyTemplate(); primary = GetTemplateChild("PART_Primary") as StackPanel; secondary = GetTemplateChild("PART_Secondary") as StackPanel;
        secondaryHost = GetTemplateChild("PART_SecondaryHost") as Border;
        topRight = GetTemplateChild("PART_TopRight") as StackPanel; title = GetTemplateChild("PART_Title") as System.Windows.Controls.TextBlock; logo = GetTemplateChild("PART_Logo") as Image;
        logoHost = GetTemplateChild("PART_LogoHost") as Border;
        navigationToggle = GetTemplateChild("PART_NavigationToggle") as Button;
        navigationBackdrop = GetTemplateChild("PART_NavigationBackdrop") as Button;
        if (navigationToggle is not null) navigationToggle.Click += ToggleNavigation;
        if (navigationBackdrop is not null) navigationBackdrop.Click += CloseNavigation;
        Refresh();
    }
    private void ToggleNavigation(object sender, RoutedEventArgs e)
    { SetCurrentValue(NavigationOpenProperty, !NavigationOpen); }
    private void CloseNavigation(object sender, RoutedEventArgs e)
    { SetCurrentValue(NavigationOpenProperty, false); navigationToggle?.Focus(); }
    protected override void OnPreviewKeyDown(KeyEventArgs e)
    {
        base.OnPreviewKeyDown(e);
        if (!IsNarrow || !NavigationOpen || !HasSecondary) return;
        if (e.Key == Key.Escape)
        {
            SetCurrentValue(NavigationOpenProperty, false); navigationToggle?.Focus(); e.Handled = true;
        }
        else if (e.Key == Key.Tab)
        {
            var targets = NavigationTargets().ToArray();
            if (targets.Length == 0) return;
            var index = Array.FindIndex(targets, target => ReferenceEquals(target, Keyboard.FocusedElement));
            var next = (Keyboard.Modifiers & ModifierKeys.Shift) != 0 ? index <= 0 ? targets.Length - 1 : index - 1 : index < 0 || index == targets.Length - 1 ? 0 : index + 1;
            targets[next].Focus(); e.Handled = true;
        }
    }
    private IEnumerable<Button> NavigationTargets()
    {
        if (navigationToggle is { IsVisible: true, IsEnabled: true }) yield return navigationToggle;
        if (secondary is not null)
            foreach (var row in secondary.Children.OfType<DockPanel>())
                foreach (var button in row.Children.OfType<Button>().OrderBy(button => button is SecondaryNavigationItem ? 0 : 1))
                    if (button.IsVisible && button.IsEnabled) yield return button;
        if (navigationBackdrop is { IsVisible: true, IsEnabled: true }) yield return navigationBackdrop;
    }
    private void UpdateLayoutState()
    {
        var narrow = ActualWidth > 0 && ActualWidth <= 760;
        if (narrow != IsNarrow)
        {
            SetValue(IsNarrowPropertyKey, narrow);
            SetCurrentValue(NavigationOpenProperty, false);
            RefreshNavigation();
        }
        SetValue(IsCompactPropertyKey, ActualWidth > 0 && ActualWidth <= 520);
        if (secondary is not null) secondary.Orientation = IsNarrow && !NavigationOpen && !HasNavigationTree ? Orientation.Horizontal : Orientation.Vertical;
        if (secondaryHost is not null)
        {
            secondaryHost.Width = IsNarrow ? NavigationOpen ? Math.Min(232, Math.Max(0, ActualWidth - 58)) : double.NaN : 232;
            secondaryHost.Padding = IsNarrow ? NavigationOpen ? new Thickness(12, 20, 12, 20) : new Thickness(12, 8, 12, 8) : new Thickness(18);
            secondaryHost.MaxHeight = IsNarrow && HasNavigationTree && !NavigationOpen ? Math.Max(0, ActualHeight * .4) : double.PositiveInfinity;
            secondaryHost.BorderThickness = IsNarrow && !NavigationOpen ? new Thickness(0, 0, 0, 1) : new Thickness(0, 0, 1, 0);
        }
        if (title is not null) title.Visibility = IsCompact && Options?.Logo is not null ? Visibility.Collapsed : Visibility.Visible;
    }
    private void Subscribe() { if (subscribed is null && Options?.TextProvider is { } provider) { subscribed = provider; provider.Changed += TextChanged; } }
    private void Unsubscribe() { if (subscribed is not null) { subscribed.Changed -= TextChanged; subscribed = null; } }
    private void TextChanged(object? sender, EventArgs e)
    {
        var provider = subscribed;
        if (provider is null) return;
        void Update() { if (ReferenceEquals(subscribed, provider)) RefreshLabels(); }
        if (Dispatcher.CheckAccess()) Update(); else Dispatcher.BeginInvoke(Update);
    }
    private string Resolve(string label, TextReference? reference) => reference is not null && Options?.TextProvider is { } provider ? provider.Get(reference) : label;
    private string Ui(string token, string fallback) => Options?.TextProvider?.Get(new TextReference("Flourish", token, fallback)) ?? fallback;
    private void RefreshLabels()
    {
        RefreshNavigation();
        if (title is not null) title.Text = Options is { } options ? Resolve(options.ProjectName, options.ProjectNameReference) : string.Empty;
        if (navigationToggle is not null)
        {
            var label = Ui(NavigationOpen ? "Shell_CloseNavigation" : "Shell_OpenNavigation", NavigationOpen ? "Close navigation" : "Open navigation");
            navigationToggle.ToolTip = label; AutomationProperties.SetName(navigationToggle, label);
        }
        if (navigationBackdrop is not null) AutomationProperties.SetName(navigationBackdrop, Ui("Shell_CloseNavigation", "Close navigation"));
    }
    private void Refresh()
    {
        RefreshLabels();
        if (logo is not null) logo.Source = Options?.Logo;
        if (logoHost is not null) logoHost.Visibility = Options?.Logo is null ? Visibility.Collapsed : Visibility.Visible;
        if (topRight is not null)
        {
            topRight.Children.Clear();
            if (Options is { } options) foreach (var factory in options.TopRight) if (factory() is UIElement element) topRight.Children.Add(element);
        }
        UpdateLayoutState();
    }
    private void RefreshNavigation()
    {
        var options = Options;
        var selectedEntry = options?.Navigation.FirstOrDefault(entry => FrameworkBuilder.Flatten([entry]).Any(item => item.Route == SelectedRoute));
        SetValue(HasNavigationPropertyKey, options?.Navigation.Count > 0);
        SetValue(HasSecondaryPropertyKey, selectedEntry?.Children.Count > 0);
        SetValue(HasNavigationTreePropertyKey, selectedEntry?.Children.Any(entry => entry.Children.Count > 0) == true);
        primary?.Children.Clear(); secondary?.Children.Clear();
        if (options is not null && primary is not null) foreach (var entry in options.Navigation)
        {
            var selected = ReferenceEquals(entry, selectedEntry);
            var label = Resolve(entry.Label, entry.LabelReference);
            var button = new PrimaryNavigationItem { Icon = entry.Icon, ToolTip = label, Disabled = entry.Disabled, IsSelected = selected };
            AutomationProperties.SetName(button, label); button.Click += async (_, _) => await NavigateAsync(entry.Route); primary.Children.Add(button);
            if (selected) foreach (var child in entry.Children) AddSecondary(child, 0);
        }
        UpdateLayoutState();
    }
    private void AddSecondary(NavigationEntry entry, int depth)
    {
        if (secondary is null) return;
        var horizontal = IsNarrow && !NavigationOpen && !HasNavigationTree;
        var row = new DockPanel { Margin = horizontal ? new Thickness(0, 0, 4, 0) : new Thickness(0, 0, 0, 6) };
        var label = Resolve(entry.Label, entry.LabelReference);
        if (entry.Children.Count > 0)
        {
            var isExpanded = expanded.GetValueOrDefault(entry.Route, FrameworkBuilder.Flatten([entry]).Any(e => e.Route == SelectedRoute));
            var toggle = new Button { Text = isExpanded ? "▴" : "▾", Variant = ButtonVariant.Quiet, Width = 36, Padding = new Thickness(0), Disabled = entry.Disabled };
            AutomationProperties.SetName(toggle, $"{Ui(isExpanded ? "Shell_Collapse" : "Shell_Expand", isExpanded ? "Collapse" : "Expand")} {label}");
            toggle.Click += (_, _) => { expanded[entry.Route] = !isExpanded; RefreshNavigation(); }; DockPanel.SetDock(toggle, Dock.Right); row.Children.Add(toggle);
        }
        var button = new SecondaryNavigationItem { Text = label, Icon = entry.Icon, ToolTip = label, Disabled = entry.Disabled, IsSelected = entry.Route == SelectedRoute, Margin = new Thickness(depth * 12, 0, 0, 0) };
        button.Click += async (_, _) => await NavigateAsync(entry.Route); row.Children.Add(button); secondary.Children.Add(row);
        if (entry.Children.Count > 0 && expanded.GetValueOrDefault(entry.Route, FrameworkBuilder.Flatten([entry]).Any(e => e.Route == SelectedRoute))) foreach (var child in entry.Children) AddSecondary(child, depth + 1);
    }
    public async Task<bool> NavigateAsync(string route)
    {
        Dispatcher.VerifyAccess(); ArgumentException.ThrowIfNullOrWhiteSpace(route);
        var entry = Options is { } options ? FrameworkBuilder.Flatten(options.Navigation).FirstOrDefault(e => e.Route == route) : null;
        if (entry is null || entry.Disabled) return false;
        if (route == SelectedRoute) { SetCurrentValue(NavigationOpenProperty, false); return false; }
        var current = ++revision;
        if (Guard is not null && !await Guard.CheckAsync(route) || current != revision) return false;
        var content = entry.Content();
        SetCurrentValue(ContentProperty, content); SelectedRoute = route; SetCurrentValue(NavigationOpenProperty, false); RefreshLabels();
        if (GetTemplateChild("PART_Scroll") is ScrollViewer scroll) scroll.ScrollToTop();
        if (content is UIElement target) target.MoveFocus(new System.Windows.Input.TraversalRequest(System.Windows.Input.FocusNavigationDirection.First));
        Navigated?.Invoke(this, route); return true;
    }
}
