using System;
using System.Collections.Generic;
using System.Linq;

using CKey = ArkheideSystem.Essential.Culture.Key;
using Localizer = ArkheideSystem.Essential.Culture.Localizer;
using InputKey = System.Windows.Input.Key;
using ArkheideSystem.Flourish.Abstract;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using ArkheideSystem.Flourish.Controls;
using ComboBoxItem = ArkheideSystem.Flourish.Controls.ComboBoxItem;

namespace ArkheideSystem.Gallery.Views;

public partial class AppearancePage : Page
{
    private readonly IThemeService theme;
    private readonly IFontService font;
    private readonly IMaterialEffectService material;
    private readonly IScrollService scroll;
    private readonly IAppearanceService appearance;
    private readonly IContentLayoutService contentLayout;
    private readonly IReadOnlyList<ComboBoxItem> materialOptions;
    private bool isRefreshing;

    public AppearancePage(
        IThemeService theme,
        IFontService font,
        IMaterialEffectService material,
        IScrollService scroll,
        IAppearanceService appearance,
        IContentLayoutService contentLayout
    )
    {
        this.theme = theme;
        this.font = font;
        this.material = material;
        this.scroll = scroll;
        this.appearance = appearance;
        this.contentLayout = contentLayout;
        materialOptions =
        [
            CreateMaterialOption(MaterialEffect.Auto),
            CreateMaterialOption(MaterialEffect.None),
            CreateMaterialOption(MaterialEffect.Mica),
            CreateMaterialOption(MaterialEffect.Acrylic),
            CreateMaterialOption(MaterialEffect.MicaAlt),
        ];
        InitializeComponent();

        ThemeBox.ItemsSource = Enum.GetValues<ApplicationTheme>();
        MaterialBox.ItemsSource = materialOptions;

        Loaded += Page_Loaded;
        Unloaded += Page_Unloaded;
        RefreshAll();
    }

    private void Page_Loaded(object sender, RoutedEventArgs e)
    {
        Page_Unloaded(sender, e);
        theme.Changed += RuntimeState_Changed;
        font.Changed += RuntimeState_Changed;
        material.Changed += RuntimeState_Changed;
        scroll.Changed += RuntimeState_Changed;
        appearance.Changed += RuntimeState_Changed;
        contentLayout.Changed += RuntimeState_Changed;
        Localizer.Current.Changed += GalleryLocalization_Changed;
        RefreshAll();
    }

    private void Page_Unloaded(object sender, RoutedEventArgs e)
    {
        theme.Changed -= RuntimeState_Changed;
        font.Changed -= RuntimeState_Changed;
        material.Changed -= RuntimeState_Changed;
        scroll.Changed -= RuntimeState_Changed;
        appearance.Changed -= RuntimeState_Changed;
        contentLayout.Changed -= RuntimeState_Changed;
        Localizer.Current.Changed -= GalleryLocalization_Changed;
    }

    private void RuntimeState_Changed(object? sender, EventArgs e)
    {
        Dispatcher.BeginInvoke(RefreshAll);
    }

    private void GalleryLocalization_Changed(object? sender, EventArgs e)
    {
        Dispatcher.BeginInvoke(RefreshMaterialOptionText);
    }

    private void ApplyTheme_Click(object sender, RoutedEventArgs e)
    {
        if (ThemeBox.SelectedItem is ApplicationTheme selected)
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
                    ParseDouble(IconFontSizeBox.Text, "standard icon font size"),
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
                        "page override standard icon font size"
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
            () => font.RemoveOverrideFont<AppearancePage>(),
            PageFontOverrideOutput,
            () =>
                Localizer.Parse(
                    CKey.Runtime_AppearancePageTypographyOverrideCleared_812D6991
                )
        );
    }

    private void MaterialBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (
            !CanApplyImmediately
            || MaterialBox.SelectedItem is not ComboBoxItem { Tag: MaterialEffect effect }
        )
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

    private void SmoothScrollingBox_Changed(object sender, RoutedEventArgs e)
    {
        if (CanApplyImmediately)
        {
            scroll.SetSmoothScrollingEnabled(SmoothScrollingBox.IsChecked == true);
        }
    }

    private void Palette_Click(object sender, RoutedEventArgs e)
    {
        Execute(
            () =>
                appearance.SetThemeColors(
                    new ThemeColors(
                        Color.FromRgb(0x3B, 0x82, 0xF6),
                        Color.FromRgb(0x8B, 0x5C, 0xF6),
                        Color.FromRgb(0x06, 0xB6, 0xD4)
                    )
                ),
            AppearanceOutput,
            FormatAppearanceOutput
        );
    }

    private void ClearAppearance_Click(object sender, RoutedEventArgs e)
    {
        Execute(
            () => appearance.SetAppearance(colors: null, cornerRadius: null),
            AppearanceOutput,
            FormatAppearanceOutput
        );
    }

    private void CornerRadiusBox_LostFocus(object sender, RoutedEventArgs e) =>
        CommitCornerRadius();

    private void CornerRadiusBox_KeyDown(object sender, KeyEventArgs e) =>
        CommitOnEnter(e, CommitCornerRadius);

    private void CommitCornerRadius()
    {
        if (!CanApplyImmediately)
        {
            return;
        }

        Execute(
            () =>
                appearance.SetCornerRadius(
                    ParseNullableDouble(CornerRadiusBox.Text, "corner radius")
                ),
            AppearanceOutput,
            FormatAppearanceOutput
        );
    }

    private void ContentLayout_Changed(object sender, RoutedEventArgs e) => CommitContentLayout();

    private void ContentWidthBox_LostFocus(object sender, RoutedEventArgs e) =>
        CommitContentLayout();

    private void ContentWidthBox_KeyDown(object sender, KeyEventArgs e) =>
        CommitOnEnter(e, CommitContentLayout);

    private void CommitContentLayout()
    {
        if (!CanApplyImmediately)
        {
            return;
        }

        try
        {
            contentLayout.SetCenterContent(
                CenterContentBox.IsChecked == true,
                ParseDouble(ContentWidthBox.Text, "content width")
            );
        }
        catch
        {
            RefreshAll();
        }
    }

    private bool CanApplyImmediately => IsLoaded && !isRefreshing;

    private static void CommitOnEnter(KeyEventArgs e, Action commit)
    {
        if (e.Key != InputKey.Enter)
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
            output.WriteLine(
                Localizer.Parse(CKey.Dynamic_Error0_43F78154, error.Message)
            );
        }
    }

    private void RefreshAll()
    {
        isRefreshing = true;
        try
        {
            ThemeBox.SelectedItem = theme.Current.RequestedTheme;

            var fontState = font.Current;
            FontFamilyBox.Text = fontState.FontFamily;
            SmallFontSizeBox.Text = fontState.SmallFontSize.ToString("0.##", CultureInfo.CurrentCulture);
            StandardFontSizeBox.Text = fontState.StandardFontSize.ToString(
                "0.##",
                CultureInfo.CurrentCulture
            );
            IconFontSizeBox.Text = fontState.IconFontSize.ToString("0.##", CultureInfo.CurrentCulture);
            LargeFontSizeBox.Text = fontState.LargeFontSize.ToString("0.##", CultureInfo.CurrentCulture);
            ExtraLargeFontSizeBox.Text = fontState.ExtraLargeFontSize.ToString(
                "0.##",
                CultureInfo.CurrentCulture
            );
            HeaderSizeFontSizeBox.Text = fontState.HeaderSizeFontSize.ToString(
                "0.##",
                CultureInfo.CurrentCulture
            );
            IconFontFamilyBox.Text = fontState.IconFontFamily;

            if (fontState.PageOverrides.TryGetValue(typeof(AppearancePage), out var pageOverride))
            {
                PageOverrideFontFamilyBox.Text = pageOverride.FontFamily;
                PageOverrideSmallFontSizeBox.Text =
                    pageOverride.SmallFontSize?.ToString("0.##", CultureInfo.CurrentCulture)
                    ?? string.Empty;
                PageOverrideStandardFontSizeBox.Text =
                    pageOverride.StandardFontSize?.ToString("0.##", CultureInfo.CurrentCulture)
                    ?? string.Empty;
                PageOverrideIconFontSizeBox.Text =
                    pageOverride.IconFontSize?.ToString("0.##", CultureInfo.CurrentCulture)
                    ?? string.Empty;
                PageOverrideLargeFontSizeBox.Text =
                    pageOverride.LargeFontSize?.ToString("0.##", CultureInfo.CurrentCulture)
                    ?? string.Empty;
                PageOverrideExtraLargeFontSizeBox.Text =
                    pageOverride.ExtraLargeFontSize?.ToString("0.##", CultureInfo.CurrentCulture)
                    ?? string.Empty;
                PageOverrideHeaderSizeFontSizeBox.Text =
                    pageOverride.HeaderSizeFontSize?.ToString("0.##", CultureInfo.CurrentCulture)
                    ?? string.Empty;
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

            MaterialBox.SelectedItem = materialOptions.Single(option =>
                Equals(option.Tag, material.Current.RequestedEffect)
            );
            MaterialDarkModeBox.IsChecked = material.Current.IsDarkMode;
            SmoothScrollingBox.IsChecked = scroll.Current.IsSmoothScrollingEnabled;
            var appearanceState = appearance.Current;
            CornerRadiusBox.Text =
                appearanceState.CornerRadius?.ToString("0.##", CultureInfo.CurrentCulture)
                ?? string.Empty;
            var layoutState = contentLayout.Current;
            CenterContentBox.IsChecked = layoutState.IsCenterContentEnabled;
            ContentWidthBox.Text = layoutState.ContentWidth.ToString(
                "0.##",
                CultureInfo.CurrentCulture
            );
        }
        finally
        {
            isRefreshing = false;
        }
    }

    private string FormatThemeOutput() =>
        Localizer.Parse(
            CKey.Runtime_ThemeUpdatedRequested0Effective1Dark2_10410DE8,
            theme.Current.RequestedTheme,
            theme.Current.EffectiveTheme,
            theme.Current.IsDark
        );

    private string FormatTypographyOutput() =>
        Localizer.Parse(
            CKey.Runtime_TypographyUpdatedText01Icons2_6EFDFFD3,
            font.Current.FontFamily,
            FormatScale(
                font.Current.SmallFontSize,
                font.Current.StandardFontSize,
                font.Current.IconFontSize,
                font.Current.LargeFontSize,
                font.Current.ExtraLargeFontSize,
                font.Current.HeaderSizeFontSize
            ),
            font.Current.IconFontFamily
        );

    private string FormatPageTypographyOutput()
    {
        if (!font.Current.PageOverrides.TryGetValue(typeof(AppearancePage), out var pageOverride))
        {
            return Localizer.Parse(
                CKey.Runtime_AppearancePageTypographyOverrideWasNotApplied_E8E937EF
            );
        }

        return Localizer.Parse(
            CKey.Runtime_AppearancePageTypographyOverrideApplied01_C79A613D,
            pageOverride.FontFamily,
            FormatScale(
                pageOverride.SmallFontSize ?? font.Current.SmallFontSize,
                pageOverride.StandardFontSize ?? font.Current.StandardFontSize,
                pageOverride.IconFontSize ?? font.Current.IconFontSize,
                pageOverride.LargeFontSize ?? font.Current.LargeFontSize,
                pageOverride.ExtraLargeFontSize ?? font.Current.ExtraLargeFontSize,
                pageOverride.HeaderSizeFontSize ?? font.Current.HeaderSizeFontSize
            )
        );
    }

    private string FormatMaterialOutput() =>
        Localizer.Parse(
            CKey.Runtime_WindowMaterialUpdatedRequested0Effective1Supported2Applied3DarkM_AA9929C6,
            material.Current.RequestedEffect,
            material.Current.EffectiveEffect,
            material.Current.IsSupported,
            material.Current.IsApplied,
            material.Current.IsDarkMode
        );

    private ComboBoxItem CreateMaterialOption(MaterialEffect effect)
    {
        var isSupported = material.IsSupported(effect);
        var option = new ComboBoxItem { Tag = effect, IsEnabled = isSupported };
        ApplyMaterialOptionText(option, effect, isSupported);
        return option;
    }

    private void RefreshMaterialOptionText()
    {
        foreach (var option in materialOptions)
        {
            if (option.Tag is MaterialEffect effect)
            {
                ApplyMaterialOptionText(option, effect, option.IsEnabled);
            }
        }
    }

    private void ApplyMaterialOptionText(
        ComboBoxItem option,
        MaterialEffect effect,
        bool isSupported
    )
    {
        option.Content =
            effect == MaterialEffect.Auto
                ? Localizer.Parse(CKey.Runtime_AutoSystemDefault_FAE8027B)
            : isSupported ? effect.ToString()
            : Localizer.Parse(CKey.Runtime_Text0Unsupported_2326A1BB, effect);
        option.ToolTip = isSupported
            ? null
            : Localizer.Parse(
                CKey.Runtime_ThisMaterialIsUnavailableOnThisWindowsVersion_44BE2E27
            );
    }

    private string FormatAppearanceOutput()
    {
        var current = appearance.Current;
        return Localizer.Parse(
            CKey.Runtime_AppearanceUpdatedPalette0CornerRadius1_3790C925,
            Localizer.Parse(
                current.ThemeColors is null
                    ? CKey.Runtime_Standard_FE6D3468
                    : CKey.Runtime_Custom_6CDFD271
            ),
            current.CornerRadius?.ToString("0.##", CultureInfo.CurrentCulture)
                ?? Localizer.Parse(CKey.Runtime_Standard_FE6D3468)
        );
    }

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

    private string FormatScale(
        double smallFontSize,
        double standardFontSize,
        double iconFontSize,
        double largeFontSize,
        double extraLargeFontSize,
        double headerSizeFontSize
    )
    {
        return Localizer.Parse(
            CKey.Runtime_Small00Standard10Icon20Large30ExtraLarge40Header50DIP_0BEDC756,
            smallFontSize,
            standardFontSize,
            iconFontSize,
            largeFontSize,
            extraLargeFontSize,
            headerSizeFontSize
        );
    }
}
