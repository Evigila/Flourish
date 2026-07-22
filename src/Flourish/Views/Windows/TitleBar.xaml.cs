using System.Windows;
using System.Windows.Automation;
using System.Windows.Input;
using System.Windows.Media;
using ArkheideSystem.Flourish.Abstract;
using ArkheideSystem.Flourish.Controls;
using ArkheideSystem.Flourish.Internal.Imaging;
using ArkheideSystem.Flourish.Services;
using TextChangedEventArgs = System.Windows.Controls.TextChangedEventArgs;
using SelectionChangedEventArgs = System.Windows.Controls.SelectionChangedEventArgs;
using SelectionChangedEventHandler = System.Windows.Controls.SelectionChangedEventHandler;
using UserControl = System.Windows.Controls.UserControl;
using WpfPanel = System.Windows.Controls.Panel;

namespace ArkheideSystem.Flourish.Views.Windows;

internal partial class FlourishTitlebar : UserControl
{
    private readonly TitleBarBreadcrumbVisibilityState breadcrumbVisibility = new();
    private readonly ProfileImageBrushCache profileImageCache = new();
    private FlourishLocalizationService? localizationService;
    private bool hasProfileRegionContent;
    private bool isApplyingSearchText;
    private bool isProfileEnabled;

    public FlourishTitlebar()
    {
        InitializeComponent();
        UpdateBreadcrumbNavigationVisibility();
    }

    public event EventHandler? BackRequested;

    public event EventHandler? ForwardRequested;

    public event EventHandler? NavigationToggleRequested;

    public event EventHandler? MinimizeRequested;

    public event EventHandler? MaximizeRequested;

    public event EventHandler? CloseRequested;

    public event EventHandler? DragRequested;

    public event EventHandler? ToggleWindowStateRequested;

    public event EventHandler? ThemeToggleRequested;

    public event EventHandler? ProfileToggleRequested;

    public event EventHandler? LogoHoverRequested;

    public event EventHandler? LogoClickRequested;

    public event SelectionChangedEventHandler? TitleSelectionChanged;

    public event EventHandler? TitleDropDownOpened;

    public event EventHandler? InteractionStarted;

    public event EventHandler<string>? SearchTextChanged;

    public void ApplyLocale(FlourishLocalizationService localization)
    {
        localizationService = localization ?? throw new ArgumentNullException(nameof(localization));
        BackButton.ToolTip = GetToolTipContent(localization.Get(FlourishLocaleKeys.TitleBarBack));
        ForwardButton.ToolTip = GetToolTipContent(localization.Get(FlourishLocaleKeys.TitleBarForward));
        NavigationToggleButton.ToolTip = GetToolTipContent(
            localization.Get(FlourishLocaleKeys.TitleBarToggleNavigation)
        );
        ThemeToggleButton.ToolTip = GetToolTipContent(
            localization.Get(FlourishLocaleKeys.TitleBarTheme)
        );
        ProfileButton.ToolTip = GetToolTipContent(
            localization.Get(FlourishLocaleKeys.TitleBarProfile)
        );
        LogoButton.ToolTip = GetToolTipContent(
            localization.Get(FlourishLocaleKeys.TitleBarApplicationInfo)
        );
        TitleComboBox.ToolTip = GetToolTipContent(
            localization.Get(FlourishLocaleKeys.TitleBarProjectMenu)
        );
        MinimizeButton.ToolTip = GetToolTipContent(
            localization.Get(FlourishLocaleKeys.TitleBarMinimize)
        );
        MaximizeButton.ToolTip = GetToolTipContent(
            localization.Get(FlourishLocaleKeys.TitleBarMaximize)
        );
        CloseButton.ToolTip = GetToolTipContent(localization.Get(FlourishLocaleKeys.TitleBarClose));
    }

    public void SetDisplayTitle(string title)
    {
        AutomationProperties.SetName(TitleComboBox, title);
    }

    internal FlourishComboBox TitleSelector => TitleComboBox;

    public void SetSearchPlaceholder(string placeholder)
    {
        SearchBox.Placeholder = placeholder;
    }

    public void SetSearchText(string text)
    {
        text ??= string.Empty;
        if (string.Equals(SearchBox.Text, text, StringComparison.Ordinal))
        {
            return;
        }

        isApplyingSearchText = true;
        try
        {
            SearchBox.Text = text;
            SearchBox.CaretIndex = text.Length;
        }
        finally
        {
            isApplyingSearchText = false;
        }
    }

    public void FocusSearchBox()
    {
        SearchBox.Focus();
        Keyboard.Focus(SearchBox);
        SearchBox.SelectAll();
    }

    public ImageSource? SetLogo(ImageSource? logoSource, string fallbackText)
    {
        if (logoSource is not null)
        {
            LogoImage.Source = logoSource;
            LogoImage.Visibility = Visibility.Visible;
            LogoFallback.Visibility = Visibility.Collapsed;
            return logoSource;
        }

        LogoImage.Source = null;
        LogoFallback.Text = string.IsNullOrWhiteSpace(fallbackText) ? "F" : fallbackText[..1];
        LogoImage.Visibility = Visibility.Collapsed;
        LogoFallback.Visibility = Visibility.Visible;
        return null;
    }

    public void SetBreadcrumbNavigationState(bool isVisible, bool canGoBack, bool canGoForward)
    {
        breadcrumbVisibility.SetNavigationState(isVisible, canGoBack, canGoForward);
        UpdateBreadcrumbNavigationVisibility();
    }

    public void SetMaximizeEnabled(bool enabled)
    {
        MaximizeButton.IsEnabled = enabled;
    }

    public void SetMaximized(bool isMaximized)
    {
        MaximizeButtonIcon.Text = isMaximized ? "\uE923" : "\uE922";
        if (localizationService is not null)
        {
            MaximizeButton.ToolTip = GetToolTipContent(
                localizationService.Get(
                    isMaximized
                        ? FlourishLocaleKeys.TitleBarRestore
                        : FlourishLocaleKeys.TitleBarMaximize
                )
            );
        }
    }

    public void SetThemeToggleState(FlourishTheme requestedTheme, FlourishTheme effectiveTheme)
    {
        ThemeToggleButtonIcon.Data =
            effectiveTheme == FlourishTheme.Dark
                ? TitleBarVisualAssets.MoonIconGeometry
                : TitleBarVisualAssets.SunIconGeometry;

        if (localizationService is null)
        {
            return;
        }

        var effectiveThemeText = localizationService.Get(
            effectiveTheme == FlourishTheme.Dark
                ? FlourishLocaleKeys.ThemeDark
                : FlourishLocaleKeys.ThemeLight
        );
        ThemeToggleButton.ToolTip = GetToolTipContent(
            requestedTheme == FlourishTheme.System
                ? localizationService.Format(
                    FlourishLocaleKeys.TitleBarThemeSystem,
                    effectiveThemeText
                )
                : localizationService.Format(
                    FlourishLocaleKeys.TitleBarThemeCurrent,
                    effectiveThemeText
                )
        );
    }

    public void SetProfile(ProfileUser profile)
    {
        ArgumentNullException.ThrowIfNull(profile);

        var imageBrush = profileImageCache.Get(profile.ImagePath);
        if (!ReferenceEquals(ProfileAvatarImage.Fill, imageBrush))
        {
            ProfileAvatarImage.Fill = imageBrush;
        }

        var hasImage = imageBrush is not null;
        ProfileAvatarImage.Visibility = ToVisibility(hasImage);
        ProfileInitialsText.Text = profile.Initials;
        ProfileInitialsText.Visibility = ToVisibility(!hasImage);
        ProfileButton.ToolTip = GetToolTipContent(profile.DisplayName);
    }

    public Rect GetProfileButtonBounds(UIElement relativeTo)
    {
        ArgumentNullException.ThrowIfNull(relativeTo);
        var topLeft = ProfileButton.TranslatePoint(new System.Windows.Point(), relativeTo);
        return new Rect(
            topLeft,
            new System.Windows.Size(ProfileButton.ActualWidth, ProfileButton.ActualHeight)
        );
    }

    public Rect GetLogoButtonBounds(UIElement relativeTo)
    {
        return GetElementBounds(LogoButton, relativeTo);
    }

    public FrameworkElement GetLogoButtonAnchor() => LogoButton;

    public void ConfigureVisibility(
        bool enableSearch,
        bool enableBreadcrumb,
        bool enableNavToggle,
        bool enableLogo,
        bool enableTitle,
        bool enableThemeToggle,
        bool enableProfile
    )
    {
        breadcrumbVisibility.SetFeatureEnabled(enableBreadcrumb);
        UpdateBreadcrumbNavigationVisibility();
        NavigationToggleButton.Visibility = ToVisibility(enableNavToggle);
        LogoButton.Visibility = ToVisibility(enableLogo);
        TitleComboBox.Visibility = ToVisibility(enableTitle);
        BrandHost.Visibility = ToVisibility(enableLogo || enableTitle);
        SearchBox.Visibility = ToVisibility(enableSearch);
        ThemeToggleButton.Visibility = ToVisibility(enableThemeToggle);
        isProfileEnabled = enableProfile;
        UpdateProfileRegionVisibility();
    }

    public void SetRegionContent(FlourishRegion region, IReadOnlyList<FrameworkElement> elements)
    {
        switch (region)
        {
            case FlourishRegion.TitlebarStart:
                SetPanelContent(TitlebarStartRegionHost, elements);
                break;
            case FlourishRegion.TitlebarCenter:
                SetPanelContent(TitlebarCenterRegionHost, elements);
                break;
            case FlourishRegion.TitlebarEnd:
                SetPanelContent(TitlebarEndRegionHost, elements);
                break;
            case FlourishRegion.TitlebarProfile:
                SetPanelContent(TitlebarProfileRegionHost, elements);
                hasProfileRegionContent = elements.Count > 0;
                UpdateProfileRegionVisibility();
                break;
            default:
                throw new ArgumentOutOfRangeException(
                    nameof(region),
                    region,
                    "Unsupported title bar region."
                );
        }
    }

    private void BackButton_Click(object sender, RoutedEventArgs e)
    {
        BackRequested?.Invoke(this, EventArgs.Empty);
    }

    private void ForwardButton_Click(object sender, RoutedEventArgs e)
    {
        ForwardRequested?.Invoke(this, EventArgs.Empty);
    }

    private void NavigationToggleButton_Click(object sender, RoutedEventArgs e)
    {
        NavigationToggleRequested?.Invoke(this, EventArgs.Empty);
    }

    private void MinimizeButton_Click(object sender, RoutedEventArgs e)
    {
        MinimizeRequested?.Invoke(this, EventArgs.Empty);
    }

    private void MaximizeButton_Click(object sender, RoutedEventArgs e)
    {
        MaximizeRequested?.Invoke(this, EventArgs.Empty);
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        CloseRequested?.Invoke(this, EventArgs.Empty);
    }

    private void ThemeToggleButton_Click(object sender, RoutedEventArgs e)
    {
        ThemeToggleRequested?.Invoke(this, EventArgs.Empty);
    }

    private void ProfileButton_Click(object sender, RoutedEventArgs e)
    {
        ProfileToggleRequested?.Invoke(this, EventArgs.Empty);
    }

    private void LogoButton_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
    {
        LogoHoverRequested?.Invoke(this, EventArgs.Empty);
    }

    private void LogoButton_Click(object sender, RoutedEventArgs e)
    {
        LogoClickRequested?.Invoke(this, EventArgs.Empty);
    }

    private void TitleComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        TitleSelectionChanged?.Invoke(this, e);
    }

    private void TitleComboBox_DropDownOpened(object? sender, EventArgs e)
    {
        TitleDropDownOpened?.Invoke(this, EventArgs.Empty);
    }

    private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (!isApplyingSearchText)
        {
            SearchTextChanged?.Invoke(this, SearchBox.Text);
        }
    }

    private void Titlebar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ButtonState != MouseButtonState.Pressed)
        {
            return;
        }

        if (e.ClickCount == 2)
        {
            ToggleWindowStateRequested?.Invoke(this, EventArgs.Empty);
            return;
        }

        DragRequested?.Invoke(this, EventArgs.Empty);
    }

    private void Titlebar_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        InteractionStarted?.Invoke(this, EventArgs.Empty);
    }

    private static Visibility ToVisibility(bool visible)
    {
        return visible ? Visibility.Visible : Visibility.Collapsed;
    }

    private static Rect GetElementBounds(FrameworkElement element, UIElement relativeTo)
    {
        ArgumentNullException.ThrowIfNull(relativeTo);
        var topLeft = element.TranslatePoint(new System.Windows.Point(), relativeTo);
        return new Rect(
            topLeft,
            new System.Windows.Size(element.ActualWidth, element.ActualHeight)
        );
    }

    private void UpdateBreadcrumbNavigationVisibility()
    {
        BreadcrumbNavigationHost.Visibility = ToVisibility(breadcrumbVisibility.IsVisible);
        BackButton.Visibility = ToVisibility(breadcrumbVisibility.IsBackVisible);
        BackButton.IsEnabled = breadcrumbVisibility.CanGoBack;
        ForwardButton.Visibility = ToVisibility(breadcrumbVisibility.IsForwardVisible);
        ForwardButton.IsEnabled = breadcrumbVisibility.CanGoForward;
    }

    private void UpdateProfileRegionVisibility()
    {
        ProfileRegionContainer.Visibility = ToVisibility(isProfileEnabled);
        ProfileButton.Visibility = ToVisibility(isProfileEnabled && !hasProfileRegionContent);
        TitlebarProfileRegionHost.Visibility = ToVisibility(
            isProfileEnabled && hasProfileRegionContent
        );
    }

    private static object GetToolTipContent(object content)
    {
        return content;
    }

    private static void SetPanelContent(WpfPanel host, IReadOnlyList<FrameworkElement> elements)
    {
        for (var index = 0; index < elements.Count; index++)
        {
            var element = elements[index];
            if (index < host.Children.Count && ReferenceEquals(host.Children[index], element))
            {
                continue;
            }

            var existingIndex = host.Children.IndexOf(element);
            if (existingIndex >= 0)
            {
                host.Children.RemoveAt(existingIndex);
            }

            host.Children.Insert(index, element);
        }

        while (host.Children.Count > elements.Count)
        {
            host.Children.RemoveAt(host.Children.Count - 1);
        }

        host.Visibility = ToVisibility(elements.Count > 0);
    }
}

internal sealed class TitleBarBreadcrumbVisibilityState
{
    public bool CanGoBack { get; private set; }

    public bool CanGoForward { get; private set; }

    public bool IsBackVisible => IsVisible && (CanGoBack || !CanGoForward);

    public bool IsForwardVisible => IsVisible && CanGoForward;

    public bool IsVisible => IsFeatureEnabled && IsNavigationVisible;

    private bool IsFeatureEnabled { get; set; }

    private bool IsNavigationVisible { get; set; }

    public void SetFeatureEnabled(bool enabled)
    {
        IsFeatureEnabled = enabled;
    }

    public void SetNavigationState(bool isVisible, bool canGoBack, bool canGoForward)
    {
        IsNavigationVisible = isVisible;
        CanGoBack = canGoBack;
        CanGoForward = canGoForward;
    }
}
