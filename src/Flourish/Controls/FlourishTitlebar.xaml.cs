using System.IO;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ArkheideSystem.Flourish.Abstract;
using ArkheideSystem.Flourish.Services;
using TextChangedEventArgs = System.Windows.Controls.TextChangedEventArgs;
using UserControl = System.Windows.Controls.UserControl;
using WpfPanel = System.Windows.Controls.Panel;

namespace ArkheideSystem.Flourish.Controls;

internal partial class FlourishTitlebar : UserControl
{
    private const string DefaultIconUri =
        "pack://application:,,,/Flourish;component/Assets/favicon.ico";
    private const string SunIconData =
        "M12,2 L12,4 M12,20 L12,22 M4.93,4.93 L6.34,6.34 "
        + "M17.66,17.66 L19.07,19.07 M2,12 L4,12 M20,12 L22,12 "
        + "M4.93,19.07 L6.34,17.66 M17.66,6.34 L19.07,4.93 "
        + "M8,12 C8,9.79 9.79,8 12,8 C14.21,8 16,9.79 16,12 "
        + "C16,14.21 14.21,16 12,16 C9.79,16 8,14.21 8,12 Z";
    private const string MoonIconData =
        "M21,12.79 C20.17,13.07 19.29,13.22 18.38,13.22 "
        + "C13.35,13.22 9.28,9.14 9.28,4.11 C9.28,3.2 9.42,2.32 9.7,1.49 "
        + "C5.78,2.58 2.9,6.18 2.9,10.45 C2.9,15.42 6.93,19.45 11.9,19.45 "
        + "C16.17,19.45 19.77,16.57 20.86,12.66 C20.91,12.7 20.96,12.75 21,12.79 Z";
    private static readonly ImageSource? DefaultLogoSource = CreateDefaultLogoSource();
    private static readonly Geometry SunIconGeometry = CreateFrozenGeometry(SunIconData);
    private static readonly Geometry MoonIconGeometry = CreateFrozenGeometry(MoonIconData);
    private FlourishLocalizationService? localizationService;
    private bool hasProfileRegionContent;
    private bool isProfileEnabled;

    public FlourishTitlebar()
    {
        InitializeComponent();
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

    public event EventHandler<string>? SearchTextChanged;

    public void ApplyLocale(FlourishLocalizationService localization)
    {
        localizationService = localization ?? throw new ArgumentNullException(nameof(localization));
        BackButton.ToolTip = localization.Get(FlourishLocaleKeys.TitleBarBack);
        ForwardButton.ToolTip = localization.Get(FlourishLocaleKeys.TitleBarForward);
        NavigationToggleButton.ToolTip = localization.Get(
            FlourishLocaleKeys.TitleBarToggleNavigation
        );
        ThemeToggleButton.ToolTip = localization.Get(FlourishLocaleKeys.TitleBarTheme);
        ProfileButton.ToolTip = localization.Get(FlourishLocaleKeys.TitleBarProfile);
        MinimizeButton.ToolTip = localization.Get(FlourishLocaleKeys.TitleBarMinimize);
        MaximizeButton.ToolTip = localization.Get(FlourishLocaleKeys.TitleBarMaximize);
        CloseButton.ToolTip = localization.Get(FlourishLocaleKeys.TitleBarClose);
    }

    public void SetTitle(string title)
    {
        TitleText.Text = title;
    }

    public void SetSubtitle(string subtitle)
    {
        SubtitleText.Text = subtitle;
    }

    public void SetSearchPlaceholder(string placeholder)
    {
        SearchPlaceholderText.Text = placeholder;
        UpdateSearchPlaceholderVisibility();
    }

    public void SetSearchText(string text)
    {
        SearchBox.Text = text ?? string.Empty;
        SearchBox.CaretIndex = SearchBox.Text.Length;
        UpdateSearchPlaceholderVisibility();
    }

    public void FocusSearchBox()
    {
        SearchBox.Focus();
        Keyboard.Focus(SearchBox);
        SearchBox.SelectAll();
    }

    public ImageSource? SetLogo(string? logoPath, string fallbackText)
    {
        var effectiveLogoSource = LoadLogoSource(logoPath) ?? DefaultLogoSource;
        if (effectiveLogoSource is not null)
        {
            LogoImage.Source = effectiveLogoSource;
            LogoImage.Visibility = Visibility.Visible;
            LogoFallback.Visibility = Visibility.Collapsed;
            return effectiveLogoSource;
        }

        LogoFallback.Text = string.IsNullOrWhiteSpace(fallbackText) ? "F" : fallbackText[..1];
        LogoImage.Visibility = Visibility.Collapsed;
        LogoFallback.Visibility = Visibility.Visible;
        return null;
    }

    public void SetBreadcrumbNavigationState(bool isVisible, bool canGoBack, bool canGoForward)
    {
        BreadcrumbNavigationHost.Visibility = ToVisibility(isVisible);
        BackButton.Visibility = ToVisibility(isVisible && (canGoBack || !canGoForward));
        BackButton.IsEnabled = canGoBack;
        ForwardButton.Visibility = ToVisibility(isVisible && canGoForward);
        ForwardButton.IsEnabled = canGoForward;
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
            MaximizeButton.ToolTip = localizationService.Get(
                isMaximized
                    ? FlourishLocaleKeys.TitleBarRestore
                    : FlourishLocaleKeys.TitleBarMaximize
            );
        }
    }

    public void SetThemeToggleState(FlourishTheme requestedTheme, FlourishTheme effectiveTheme)
    {
        ThemeToggleButtonIcon.Data =
            effectiveTheme == FlourishTheme.Dark ? MoonIconGeometry : SunIconGeometry;

        if (localizationService is null)
        {
            return;
        }

        var effectiveThemeText = localizationService.Get(
            effectiveTheme == FlourishTheme.Dark
                ? FlourishLocaleKeys.ThemeDark
                : FlourishLocaleKeys.ThemeLight
        );
        ThemeToggleButton.ToolTip =
            requestedTheme == FlourishTheme.System
                ? localizationService.Format(
                    FlourishLocaleKeys.TitleBarThemeSystem,
                    effectiveThemeText
                )
                : localizationService.Format(
                    FlourishLocaleKeys.TitleBarThemeCurrent,
                    effectiveThemeText
                );
    }

    public void SetProfile(ProfileUser profile)
    {
        ArgumentNullException.ThrowIfNull(profile);

        var imageSource = ProfileImageLoader.Load(profile.ImagePath);
        ProfileAvatarImage.Fill = imageSource is null
            ? null
            : new ImageBrush(imageSource) { Stretch = Stretch.UniformToFill };
        ProfileAvatarImage.Visibility = ToVisibility(imageSource is not null);
        ProfileInitialsText.Text = profile.Initials;
        ProfileInitialsText.Visibility = ToVisibility(imageSource is null);
        ProfileButton.ToolTip = profile.DisplayName;
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

    public void ConfigureVisibility(
        bool enableSearch,
        bool enableBreadcrumb,
        bool enableNavToggle,
        bool enableLogo,
        bool enableTitle,
        bool enableSubTitle,
        bool enableThemeToggle,
        bool enableProfile
    )
    {
        BreadcrumbNavigationHost.Visibility = ToVisibility(enableBreadcrumb);
        NavigationToggleButton.Visibility = ToVisibility(enableNavToggle);
        LogoHost.Visibility = ToVisibility(enableLogo);
        TitleText.Visibility = ToVisibility(enableTitle);
        SubtitleText.Visibility = ToVisibility(enableSubTitle);
        SubtitleText.Margin =
            enableTitle && enableSubTitle ? new Thickness(8, 1, 0, 0) : new Thickness();
        TitleTextHost.Visibility = ToVisibility(enableTitle || enableSubTitle);
        BrandHost.Visibility = ToVisibility(enableLogo || enableTitle || enableSubTitle);
        SearchBoxHost.Visibility = ToVisibility(enableSearch);
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

    private void SearchBox_GotKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
    {
        UpdateSearchPlaceholderVisibility();
    }

    private void SearchBox_LostKeyboardFocus(object sender, KeyboardFocusChangedEventArgs e)
    {
        UpdateSearchPlaceholderVisibility();
    }

    private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        UpdateSearchPlaceholderVisibility();
        SearchTextChanged?.Invoke(this, SearchBox.Text);
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

    private static Visibility ToVisibility(bool visible)
    {
        return visible ? Visibility.Visible : Visibility.Collapsed;
    }

    private void UpdateProfileRegionVisibility()
    {
        ProfileRegionContainer.Visibility = ToVisibility(isProfileEnabled);
        ProfileButton.Visibility = ToVisibility(isProfileEnabled && !hasProfileRegionContent);
        TitlebarProfileRegionHost.Visibility = ToVisibility(
            isProfileEnabled && hasProfileRegionContent
        );
    }

    private static void SetPanelContent(WpfPanel host, IReadOnlyList<FrameworkElement> elements)
    {
        host.Children.Clear();
        foreach (var element in elements)
        {
            host.Children.Add(element);
        }

        host.Visibility = ToVisibility(elements.Count > 0);
    }

    private static ImageSource? CreateDefaultLogoSource()
    {
        return LoadLogoSource(DefaultIconUri);
    }

    private static ImageSource? LoadLogoSource(string? logoPath)
    {
        if (string.IsNullOrWhiteSpace(logoPath))
        {
            return null;
        }

        try
        {
            var image = new BitmapImage();
            image.BeginInit();
            image.UriSource = new Uri(logoPath, UriKind.RelativeOrAbsolute);
            image.CacheOption = BitmapCacheOption.OnLoad;
            image.EndInit();
            return TrimTransparentPixels(image);
        }
        catch (InvalidOperationException)
        {
            return null;
        }
        catch (IOException)
        {
            return null;
        }
        catch (NotSupportedException)
        {
            return null;
        }
        catch (UriFormatException)
        {
            return null;
        }
    }

    private static Geometry CreateFrozenGeometry(string pathData)
    {
        var geometry = Geometry.Parse(pathData);
        geometry.Freeze();
        return geometry;
    }

    internal static ImageSource TrimTransparentPixels(BitmapSource source)
    {
        var bitmap = new FormatConvertedBitmap(source, PixelFormats.Bgra32, null, 0);
        var width = bitmap.PixelWidth;
        var height = bitmap.PixelHeight;
        var stride = width * 4;
        var pixels = new byte[stride * height];
        bitmap.CopyPixels(pixels, stride, 0);

        var left = width;
        var top = height;
        var right = -1;
        var bottom = -1;

        for (var y = 0; y < height; y++)
        {
            var rowOffset = y * stride;
            for (var x = 0; x < width; x++)
            {
                var alpha = pixels[rowOffset + x * 4 + 3];
                if (alpha == 0)
                {
                    continue;
                }

                left = Math.Min(left, x);
                top = Math.Min(top, y);
                right = Math.Max(right, x);
                bottom = Math.Max(bottom, y);
            }
        }

        if (right < left || bottom < top)
        {
            source.Freeze();
            return source;
        }

        var cropped = new CroppedBitmap(
            bitmap,
            new Int32Rect(left, top, right - left + 1, bottom - top + 1)
        );
        cropped.Freeze();
        return cropped;
    }

    private void UpdateSearchPlaceholderVisibility()
    {
        SearchPlaceholderText.Visibility =
            string.IsNullOrEmpty(SearchBox.Text) && !SearchBox.IsKeyboardFocusWithin
                ? Visibility.Visible
                : Visibility.Collapsed;
    }
}
