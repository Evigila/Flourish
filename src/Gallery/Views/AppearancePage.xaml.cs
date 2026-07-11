using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using ArkheideSystem.Flourish.Abstract;

namespace ArkheideSystem.Gallery.Views;

public partial class AppearancePage : Page
{
    private readonly IThemeService theme;
    private readonly IFontService font;
    private readonly IToolTipService toolTips;
    private readonly IMotionService motion;
    private readonly IMaterialEffectService material;

    public AppearancePage(
        IThemeService theme,
        IFontService font,
        IToolTipService toolTips,
        IMotionService motion,
        IMaterialEffectService material
    )
    {
        this.theme = theme;
        this.font = font;
        this.toolTips = toolTips;
        this.motion = motion;
        this.material = material;
        InitializeComponent();

        ThemeBox.ItemsSource = Enum.GetValues<FlourishTheme>();
        PageTransitionBox.ItemsSource = Enum.GetValues<FlourishPageTransition>();
        NavigationTransitionBox.ItemsSource = Enum.GetValues<FlourishNavigationPanelTransition>();
        MaterialBox.ItemsSource = Enum.GetValues<MaterialEffect>();

        Loaded += Page_Loaded;
        Unloaded += Page_Unloaded;
        RefreshAll();
    }

    private void Page_Loaded(object sender, RoutedEventArgs e)
    {
        Page_Unloaded(sender, e);
        theme.ThemeChanged += RuntimeState_Changed;
        font.Changed += RuntimeState_Changed;
        toolTips.Changed += RuntimeState_Changed;
        motion.Changed += RuntimeState_Changed;
        material.Changed += RuntimeState_Changed;
        RefreshAll();
    }

    private void Page_Unloaded(object sender, RoutedEventArgs e)
    {
        theme.ThemeChanged -= RuntimeState_Changed;
        font.Changed -= RuntimeState_Changed;
        toolTips.Changed -= RuntimeState_Changed;
        motion.Changed -= RuntimeState_Changed;
        material.Changed -= RuntimeState_Changed;
    }

    private void RuntimeState_Changed(object? sender, EventArgs e)
    {
        Dispatcher.BeginInvoke(RefreshAll);
    }

    private void ApplyTheme_Click(object sender, RoutedEventArgs e)
    {
        if (ThemeBox.SelectedItem is FlourishTheme selected)
        {
            Execute(() => theme.SetTheme(selected), ThemeStatusText);
        }
    }

    private void ToggleTheme_Click(object sender, RoutedEventArgs e)
    {
        Execute(theme.ToggleTheme, ThemeStatusText);
    }

    private void ApplyFont_Click(object sender, RoutedEventArgs e)
    {
        Execute(
            () =>
            {
                var size = ParseDouble(FontSizeBox.Text, "base font size");
                font.SetFont(FontFamilyBox.Text, size);
                font.SetIconFontFamily(IconFontFamilyBox.Text);
            },
            FontStatusText
        );
    }

    private void ApplyPageFontOverride_Click(object sender, RoutedEventArgs e)
    {
        Execute(
            () =>
            {
                double? fontSize = string.IsNullOrWhiteSpace(PageOverrideFontSizeBox.Text)
                    ? null
                    : ParseDouble(PageOverrideFontSizeBox.Text, "page override font size");
                font.SetOverrideFont<AppearancePage>(
                    PageOverrideFontFamilyBox.Text,
                    fontSize
                );
            },
            PageFontOverrideStatusText
        );
    }

    private void ClearPageFontOverride_Click(object sender, RoutedEventArgs e)
    {
        Execute(
            () => font.ClearOverrideFont<AppearancePage>(),
            PageFontOverrideStatusText
        );
    }

    private void ConfigureToolTip_Click(object sender, RoutedEventArgs e)
    {
        Execute(
            () =>
                toolTips.Configure(
                    ParseInt(ToolTipDelayBox.Text, "tooltip delay"),
                    ParseDouble(ToolTipMarginBox.Text, "tooltip margin")
                ),
            ToolTipStatusText
        );
    }

    private void ToggleToolTip_Click(object sender, RoutedEventArgs e)
    {
        Execute(() => toolTips.SetEnabled(!toolTips.Current.IsEnabled), ToolTipStatusText);
    }

    private void ApplyMotion_Click(object sender, RoutedEventArgs e)
    {
        Execute(
            () =>
            {
                if (PageTransitionBox.SelectedItem is not FlourishPageTransition pageTransition
                    || NavigationTransitionBox.SelectedItem
                        is not FlourishNavigationPanelTransition navigationTransition)
                {
                    throw new InvalidOperationException("Select both transition modes.");
                }

                motion.SetEnabled(MotionEnabledBox.IsChecked == true);
                motion.SetPageTransition(
                    pageTransition,
                    TimeSpan.FromMilliseconds(ParseDouble(PageDurationBox.Text, "page duration"))
                );
                motion.SetNavigationPanelTransition(
                    navigationTransition,
                    TimeSpan.FromMilliseconds(
                        ParseDouble(NavigationDurationBox.Text, "navigation duration")
                    )
                );
                motion.SetHoverReveal(HoverRevealBox.IsChecked == true);
                motion.SetRespectSystemReducedMotion(ReducedMotionBox.IsChecked == true);
            },
            MotionStatusText
        );
    }

    private void ApplyMaterial_Click(object sender, RoutedEventArgs e)
    {
        Execute(
            () =>
            {
                if (MaterialBox.SelectedItem is not MaterialEffect effect)
                {
                    throw new InvalidOperationException("Select a material effect.");
                }

                material.SetEffect(effect);
                material.SetDarkMode(MaterialDarkModeBox.IsChecked == true);
            },
            MaterialStatusText
        );
    }

    private void Execute(Action action, TextBlock status)
    {
        try
        {
            action();
            RefreshAll();
        }
        catch (Exception error)
        {
            status.Text = error.Message;
        }
    }

    private void RefreshAll()
    {
        ThemeBox.SelectedItem = theme.CurrentTheme;
        ThemeStatusText.Text =
            $"Requested: {theme.CurrentTheme}  |  Effective: {theme.EffectiveTheme}  |  Dark: {theme.IsDark}";

        FontFamilyBox.Text = font.FontFamily;
        FontSizeBox.Text = font.FontSize.ToString("0.##", CultureInfo.CurrentCulture);
        IconFontFamilyBox.Text = font.IconFontFamily;
        FontStatusText.Text =
            $"Text: {font.FontFamily}, {font.FontSize:0.##} pt  |  Icons: {font.IconFontFamily}";

        if (font.PageOverrides.TryGetValue(typeof(AppearancePage), out var pageOverride))
        {
            PageOverrideFontFamilyBox.Text = pageOverride.FontFamily;
            PageOverrideFontSizeBox.Text = pageOverride.FontSize?.ToString(
                "0.##",
                CultureInfo.CurrentCulture
            ) ?? string.Empty;
            PageFontOverrideStatusText.Text = pageOverride.FontSize is { } overrideSize
                ? $"AppearancePage override: {pageOverride.FontFamily}, {overrideSize:0.##} pt."
                : $"AppearancePage override: {pageOverride.FontFamily}; size follows global {font.FontSize:0.##} pt.";
        }
        else
        {
            PageFontOverrideStatusText.Text =
                $"No page override. AppearancePage follows {font.FontFamily}, {font.FontSize:0.##} pt.";
        }

        var currentToolTips = toolTips.Current;
        ToolTipDelayBox.Text = currentToolTips.InitialShowDelayMilliseconds.ToString(
            CultureInfo.CurrentCulture
        );
        ToolTipMarginBox.Text = currentToolTips.SpawnableMargin.ToString(
            "0.##",
            CultureInfo.CurrentCulture
        );
        ToggleToolTipButton.Content = currentToolTips.IsEnabled
            ? "Disable tooltips"
            : "Enable tooltips";
        ToolTipStatusText.Text =
            $"Enabled: {currentToolTips.IsEnabled}  |  Delay: {currentToolTips.InitialShowDelayMilliseconds} ms  |  Margin: {currentToolTips.SpawnableMargin:0.##}";

        var currentMotion = motion.Current;
        MotionEnabledBox.IsChecked = currentMotion.IsEnabled;
        PageTransitionBox.SelectedItem = currentMotion.PageTransition;
        PageDurationBox.Text = currentMotion.PageTransitionDuration.TotalMilliseconds.ToString(
            "0",
            CultureInfo.CurrentCulture
        );
        NavigationTransitionBox.SelectedItem = currentMotion.NavigationPanelTransition;
        NavigationDurationBox.Text =
            currentMotion.NavigationPanelTransitionDuration.TotalMilliseconds.ToString(
                "0",
                CultureInfo.CurrentCulture
            );
        HoverRevealBox.IsChecked = currentMotion.IsHoverRevealEnabled;
        ReducedMotionBox.IsChecked = currentMotion.RespectSystemReducedMotion;
        MotionStatusText.Text =
            $"Animation allowed now: {motion.CanAnimate}  |  Hover duration: {currentMotion.HoverRevealAnimationDuration.TotalMilliseconds:0} ms";

        MaterialBox.SelectedItem = material.CurrentEffect;
        MaterialDarkModeBox.IsChecked = material.IsDarkMode;
        MaterialStatusText.Text =
            $"Requested: {material.CurrentEffect}  |  Supported: {material.IsSupported(material.CurrentEffect)}  |  Applied: {material.IsApplied}";
    }

    private static double ParseDouble(string text, string name)
    {
        if (!double.TryParse(text, NumberStyles.Float, CultureInfo.CurrentCulture, out var value))
        {
            throw new ArgumentException($"Enter a valid {name}.");
        }

        return value;
    }

    private static int ParseInt(string text, string name)
    {
        if (!int.TryParse(text, NumberStyles.Integer, CultureInfo.CurrentCulture, out var value))
        {
            throw new ArgumentException($"Enter a valid {name}.");
        }

        return value;
    }
}
