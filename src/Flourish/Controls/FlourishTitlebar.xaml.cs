using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using AckSS.Flourish.Abstract;
using TextChangedEventArgs = System.Windows.Controls.TextChangedEventArgs;
using UserControl = System.Windows.Controls.UserControl;
using WpfPanel = System.Windows.Controls.Panel;

namespace AckSS.Flourish.Controls;

internal partial class FlourishTitlebar : UserControl
{
    private const string DefaultIconUri = "pack://application:,,,/Flourish;component/Assets/favicon.ico";
    private static readonly ImageSource? DefaultLogoSource = CreateDefaultLogoSource();
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

    public event EventHandler<string>? SearchTextChanged;

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

    public void SetLogo(ImageSource? logoSource, string fallbackText)
    {
        var effectiveLogoSource = logoSource ?? DefaultLogoSource;
        if (effectiveLogoSource is not null)
        {
            LogoImage.Source = effectiveLogoSource;
            LogoImage.Visibility = Visibility.Visible;
            LogoFallback.Visibility = Visibility.Collapsed;
            return;
        }

        LogoFallback.Text = string.IsNullOrWhiteSpace(fallbackText) ? "F" : fallbackText[..1];
        LogoImage.Visibility = Visibility.Collapsed;
        LogoFallback.Visibility = Visibility.Visible;
    }

    public void SetBreadcrumbNavigationState(
        bool isVisible,
        bool canGoBack,
        bool canGoForward
    )
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
    }

    public void SetThemeToggleState(FlourishTheme requestedTheme, FlourishTheme effectiveTheme)
    {
        ThemeToggleButtonIcon.Text = requestedTheme switch
        {
            FlourishTheme.System => "\uE713",
            FlourishTheme.Dark => "\uE708",
            _ => "\uE706",
        };

        var effectiveThemeText = effectiveTheme == FlourishTheme.Dark ? "Dark" : "Light";
        ThemeToggleButton.ToolTip = requestedTheme == FlourishTheme.System
            ? $"Theme: System ({effectiveThemeText})"
            : $"Theme: {effectiveThemeText}";
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

    public void SetRegionContent(
        FlourishRegion region,
        IReadOnlyList<FrameworkElement> elements
    )
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
                throw new ArgumentOutOfRangeException(nameof(region), region, "Unsupported title bar region.");
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
        var showProfileArea = isProfileEnabled || hasProfileRegionContent;
        ProfileRegionContainer.Visibility = ToVisibility(showProfileArea);
        ProfileHost.Visibility = ToVisibility(isProfileEnabled && !hasProfileRegionContent);
        TitlebarProfileRegionHost.Visibility = ToVisibility(hasProfileRegionContent);
    }

    private static void SetPanelContent(
        WpfPanel host,
        IReadOnlyList<FrameworkElement> elements
    )
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
        try
        {
            var image = new BitmapImage();
            image.BeginInit();
            image.UriSource = new Uri(DefaultIconUri, UriKind.Absolute);
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
    }

    private static ImageSource TrimTransparentPixels(BitmapSource source)
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
