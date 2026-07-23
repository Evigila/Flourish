using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ArkheideSystem.Flourish.Abstract;
using ArkheideSystem.Flourish.Controls;

namespace ArkheideSystem.Gallery.Views;

public partial class AppearancePage : Page
{
    private readonly IThemeService theme;
    private readonly IFontService font;
    private readonly IMaterialEffectService material;
    private bool isRefreshing;

    public AppearancePage(
        IThemeService theme,
        IFontService font,
        IMaterialEffectService material
    )
    {
        this.theme = theme;
        this.font = font;
        this.material = material;
        InitializeComponent();

        ThemeBox.ItemsSource = Enum.GetValues<FlourishTheme>();
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
        material.Changed += RuntimeState_Changed;
        RefreshAll();
    }

    private void Page_Unloaded(object sender, RoutedEventArgs e)
    {
        theme.ThemeChanged -= RuntimeState_Changed;
        font.Changed -= RuntimeState_Changed;
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
            Execute(() => theme.SetTheme(selected), ThemeOutput, FormatThemeOutput);
        }
    }

    private void ThemeBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (CanApplyImmediately)
        {
            ApplyTheme_Click(sender, new RoutedEventArgs());
        }
    }

    private void ToggleTheme_Click(object sender, RoutedEventArgs e)
    {
        Execute(theme.ToggleTheme, ThemeOutput, FormatThemeOutput);
    }

    private void ApplyFont_Click(object sender, RoutedEventArgs e)
    {
        Execute(
            () =>
            {
                font.SetFont(
                    FontFamilyBox.Text,
                    ParseDouble(SmallFontSizeBox.Text, "small font size"),
                    ParseDouble(StandardFontSizeBox.Text, "standard font size"),
                    ParseDouble(IconFontSizeBox.Text, "icon font size"),
                    ParseDouble(LargeFontSizeBox.Text, "large font size"),
                    ParseDouble(ExtraLargeFontSizeBox.Text, "extra-large font size"),
                    ParseDouble(HeaderSizeFontSizeBox.Text, "header font size")
                );
                font.SetIconFontFamily(IconFontFamilyBox.Text);
            },
            FontOutput,
            FormatTypographyOutput
        );
    }

    private void TypographyBox_LostFocus(object sender, RoutedEventArgs e) => CommitTypography();

    private void TypographyBox_KeyDown(object sender, KeyEventArgs e) =>
        CommitOnEnter(e, CommitTypography);

    private void CommitTypography()
    {
        if (CanApplyImmediately)
        {
            ApplyFont_Click(this, new RoutedEventArgs());
        }
    }

    private void ApplyPageFontOverride_Click(object sender, RoutedEventArgs e)
    {
        Execute(
            () =>
            {
                font.SetOverrideFont<AppearancePage>(
                    PageOverrideFontFamilyBox.Text,
                    ParseNullableDouble(
                        PageOverrideSmallFontSizeBox.Text,
                        "page override small font size"
                    ),
                    ParseNullableDouble(
                        PageOverrideStandardFontSizeBox.Text,
                        "page override standard font size"
                    ),
                    ParseNullableDouble(
                        PageOverrideIconFontSizeBox.Text,
                        "page override icon font size"
                    ),
                    ParseNullableDouble(
                        PageOverrideLargeFontSizeBox.Text,
                        "page override large font size"
                    ),
                    ParseNullableDouble(
                        PageOverrideExtraLargeFontSizeBox.Text,
                        "page override extra-large font size"
                    ),
                    ParseNullableDouble(
                        PageOverrideHeaderSizeFontSizeBox.Text,
                        "page override header font size"
                    )
                );
            },
            PageFontOverrideOutput,
            FormatPageTypographyOutput
        );
    }

    private void PageOverrideBox_LostFocus(object sender, RoutedEventArgs e) =>
        CommitPageOverride();

    private void PageOverrideBox_KeyDown(object sender, KeyEventArgs e) =>
        CommitOnEnter(e, CommitPageOverride);

    private void CommitPageOverride()
    {
        if (CanApplyImmediately)
        {
            ApplyPageFontOverride_Click(this, new RoutedEventArgs());
        }
    }

    private void ClearPageFontOverride_Click(object sender, RoutedEventArgs e)
    {
        Execute(
            () => font.ClearOverrideFont<AppearancePage>(),
            PageFontOverrideOutput,
            () => "AppearancePage typography override cleared."
        );
    }

    private void MaterialBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (!CanApplyImmediately || MaterialBox.SelectedItem is not MaterialEffect effect)
        {
            return;
        }

        Execute(() => material.SetEffect(effect), MaterialOutput, FormatMaterialOutput);
    }

    private void MaterialDarkModeBox_Changed(object sender, RoutedEventArgs e)
    {
        if (CanApplyImmediately)
        {
            Execute(
                () => material.SetDarkMode(MaterialDarkModeBox.IsChecked == true),
                MaterialOutput,
                FormatMaterialOutput
            );
        }
    }

    private bool CanApplyImmediately => IsLoaded && !isRefreshing;

    private static void CommitOnEnter(KeyEventArgs e, Action commit)
    {
        if (e.Key != Key.Enter)
        {
            return;
        }

        commit();
        e.Handled = true;
    }

    private void Execute(Action action, OutputCard output, Func<string> successMessage)
    {
        try
        {
            action();
            RefreshAll();
            output.WriteLine(successMessage());
        }
        catch (Exception error)
        {
            output.WriteLine($"Error: {error.Message}");
        }
    }

    private void RefreshAll()
    {
        isRefreshing = true;
        try
        {
            ThemeBox.SelectedItem = theme.CurrentTheme;

            FontFamilyBox.Text = font.FontFamily;
            SmallFontSizeBox.Text = font.SmallFontSize.ToString(
                "0.##",
                CultureInfo.CurrentCulture
            );
            StandardFontSizeBox.Text = font.StandardFontSize.ToString(
                "0.##",
                CultureInfo.CurrentCulture
            );
            IconFontSizeBox.Text = font.IconFontSize.ToString(
                "0.##",
                CultureInfo.CurrentCulture
            );
            LargeFontSizeBox.Text = font.LargeFontSize.ToString(
                "0.##",
                CultureInfo.CurrentCulture
            );
            ExtraLargeFontSizeBox.Text = font.ExtraLargeFontSize.ToString(
                "0.##",
                CultureInfo.CurrentCulture
            );
            HeaderSizeFontSizeBox.Text = font.HeaderSizeFontSize.ToString(
                "0.##",
                CultureInfo.CurrentCulture
            );
            IconFontFamilyBox.Text = font.IconFontFamily;

            if (font.PageOverrides.TryGetValue(typeof(AppearancePage), out var pageOverride))
            {
                PageOverrideFontFamilyBox.Text = pageOverride.FontFamily;
                PageOverrideSmallFontSizeBox.Text = pageOverride.SmallFontSize?.ToString(
                    "0.##",
                    CultureInfo.CurrentCulture
                ) ?? string.Empty;
                PageOverrideStandardFontSizeBox.Text = pageOverride.StandardFontSize?.ToString(
                    "0.##",
                    CultureInfo.CurrentCulture
                ) ?? string.Empty;
                PageOverrideIconFontSizeBox.Text = pageOverride.IconFontSize?.ToString(
                    "0.##",
                    CultureInfo.CurrentCulture
                ) ?? string.Empty;
                PageOverrideLargeFontSizeBox.Text = pageOverride.LargeFontSize?.ToString(
                    "0.##",
                    CultureInfo.CurrentCulture
                ) ?? string.Empty;
                PageOverrideExtraLargeFontSizeBox.Text =
                    pageOverride.ExtraLargeFontSize?.ToString(
                        "0.##",
                        CultureInfo.CurrentCulture
                    ) ?? string.Empty;
                PageOverrideHeaderSizeFontSizeBox.Text =
                    pageOverride.HeaderSizeFontSize?.ToString(
                        "0.##",
                        CultureInfo.CurrentCulture
                    ) ?? string.Empty;
            }
            else
            {
                PageOverrideSmallFontSizeBox.Text = string.Empty;
                PageOverrideStandardFontSizeBox.Text = string.Empty;
                PageOverrideIconFontSizeBox.Text = string.Empty;
                PageOverrideLargeFontSizeBox.Text = string.Empty;
                PageOverrideExtraLargeFontSizeBox.Text = string.Empty;
                PageOverrideHeaderSizeFontSizeBox.Text = string.Empty;
            }

            MaterialBox.SelectedItem = material.CurrentEffect;
            MaterialDarkModeBox.IsChecked = material.IsDarkMode;
        }
        finally
        {
            isRefreshing = false;
        }
    }

    private string FormatThemeOutput() =>
        $"Theme updated: requested {theme.CurrentTheme}; effective {theme.EffectiveTheme}; dark {theme.IsDark}.";

    private string FormatTypographyOutput() =>
        $"Typography updated: text {font.FontFamily}; {FormatScale(font.SmallFontSize, font.StandardFontSize, font.IconFontSize, font.LargeFontSize, font.ExtraLargeFontSize, font.HeaderSizeFontSize)}; icons {font.IconFontFamily}.";

    private string FormatPageTypographyOutput()
    {
        if (!font.PageOverrides.TryGetValue(typeof(AppearancePage), out var pageOverride))
        {
            return "AppearancePage typography override was not applied.";
        }

        return $"AppearancePage typography override applied: {pageOverride.FontFamily}; {FormatScale(pageOverride.SmallFontSize ?? font.SmallFontSize, pageOverride.StandardFontSize ?? font.StandardFontSize, pageOverride.IconFontSize ?? font.IconFontSize, pageOverride.LargeFontSize ?? font.LargeFontSize, pageOverride.ExtraLargeFontSize ?? font.ExtraLargeFontSize, pageOverride.HeaderSizeFontSize ?? font.HeaderSizeFontSize)}.";
    }

    private string FormatMaterialOutput() =>
        $"Window material updated: requested {material.CurrentEffect}; supported {material.IsSupported(material.CurrentEffect)}; applied {material.IsApplied}; dark mode {material.IsDarkMode}.";

    private static double ParseDouble(string text, string name)
    {
        if (!double.TryParse(text, NumberStyles.Float, CultureInfo.CurrentCulture, out var value))
        {
            throw new ArgumentException($"Enter a valid {name}.");
        }

        return value;
    }

    private static double? ParseNullableDouble(string text, string name)
    {
        return string.IsNullOrWhiteSpace(text) ? null : ParseDouble(text, name);
    }

    private static string FormatScale(
        double smallFontSize,
        double standardFontSize,
        double iconFontSize,
        double largeFontSize,
        double extraLargeFontSize,
        double headerSizeFontSize
    )
    {
        return $"small {smallFontSize:0.##}, standard {standardFontSize:0.##}, icon {iconFontSize:0.##}, large {largeFontSize:0.##}, extra-large {extraLargeFontSize:0.##}, header {headerSizeFontSize:0.##} DIP";
    }

}
