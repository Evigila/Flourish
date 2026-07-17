using ArkheideSystem.Flourish.Abstract;
using ArkheideSystem.Flourish.Internal.Configuration;
using ArkheideSystem.Flourish.Internal.Composition;
using System.Windows.Media;

namespace ArkheideSystem.Flourish.Test.Internal.Composition;

public sealed class FlourishShellBuilderTests
{
    [Fact]
    public void CenterContent_WhenNotConfigured_RemainsDisabled()
    {
        var options = new FlourishShellOptions();

        Assert.False(options.IsCenterContentEnabled);
        Assert.Equal(0, options.CenterContentWidth);
        Assert.False(options.IsMultiProjectEnabled);
    }

    [Fact]
    public void FeatureMethods_ConfigureAndEnableFeatures()
    {
        var options = new FlourishShellOptions();
        var sut = new FlourishShellBuilder(options);
        var themeColors = new FlourishThemeColors(
            Color.FromRgb(0x12, 0x34, 0x56),
            Color.FromRgb(0x65, 0x43, 0x21),
            Color.FromRgb(0x7D, 0x4C, 0xDB)
        );

        Assert.Same(sut, sut.UseTitleBar());
        Assert.Same(sut, sut.UseMultiProject());
        Assert.Same(sut, sut.UseNavigation());
        Assert.Same(sut, sut.UseCenterContent(enabled: true, contentWidth: 1080));
        Assert.Same(sut, sut.UseDynamicToolbar());
        Assert.Same(sut, sut.UseTips(350));
        Assert.Same(sut, sut.UseMotion());
        Assert.Same(sut, sut.UseMaterialEffect(MaterialEffect.Mica));
        Assert.Same(sut, sut.UseThemeColors(themeColors));
        Assert.Same(sut, sut.UseCornerRadius(5));
        Assert.Same(sut, sut.UseGlobalFont("Arial", 13, 15, 17, 19, 22, 28));
        Assert.Same(
            sut,
            sut.SetOverrideFont<OverrideFontPage>("Consolas", 14, 16, 18, 20, 24, 30)
        );
        Assert.Same(sut, sut.UseStatusBar());

        Assert.True(options.IsTitlebarEnabled);
        Assert.True(options.IsMultiProjectEnabled);
        Assert.True(options.IsNavigationPanelEnabled);
        Assert.True(options.IsCenterContentEnabled);
        Assert.Equal(1080, options.CenterContentWidth);
        Assert.True(options.IsDynamicToolbarEnabled);
        Assert.True(options.IsTipsEnabled);
        Assert.Equal(350, options.Tips.InitialShowDelayMilliseconds);
        Assert.Equal(5, options.Tips.SpawnableMargin);
        Assert.True(options.Motion.IsEnabled);
        Assert.True(options.IsMaterialEffectEnabled);
        Assert.Equal(MaterialEffect.Mica, options.MaterialEffect);
        Assert.Equal(themeColors, options.ThemeColors);
        Assert.Equal(5, options.CornerRadius);
        Assert.Equal("Arial", options.FontFamily);
        Assert.Equal(13, options.FontSizeSmall);
        Assert.Equal(15, options.FontSizeStandard);
        Assert.Equal(17, options.FontSizeIcon);
        Assert.Equal(19, options.FontSizeLarge);
        Assert.Equal(22, options.FontSizeExtraLarge);
        Assert.Equal(28, options.FontSizeHeaderSize);
        var pageOverride = Assert.Single(options.PageFontOverridesByPageType);
        Assert.Equal(typeof(OverrideFontPage), pageOverride.Key);
        Assert.Equal("Consolas", pageOverride.Value.FontFamily);
        Assert.Equal(14, pageOverride.Value.SmallFontSize);
        Assert.Equal(16, pageOverride.Value.StandardFontSize);
        Assert.Equal(18, pageOverride.Value.IconFontSize);
        Assert.Equal(20, pageOverride.Value.LargeFontSize);
        Assert.Equal(24, pageOverride.Value.ExtraLargeFontSize);
        Assert.Equal(30, pageOverride.Value.HeaderSizeFontSize);
        Assert.True(options.IsStatusBarEnabled);
    }

    [Fact]
    public void FontOptions_UseTheCanonicalTextAndIconDefaults()
    {
        var options = new FlourishShellOptions();

        Assert.Equal("Segoe UI", options.FontFamily);
        Assert.Equal(12, options.FontSizeSmall);
        Assert.Equal(14, options.FontSizeStandard);
        Assert.Equal(16, options.FontSizeIcon);
        Assert.Equal(16, options.FontSizeLarge);
        Assert.Equal(24, options.FontSizeExtraLarge);
        Assert.Equal(32, options.FontSizeHeaderSize);
    }

    [Fact]
    public void BooleanFeatureMethods_WithFalse_DisableFeatures()
    {
        var options = new FlourishShellOptions
        {
            IsTitlebarEnabled = true,
            IsMultiProjectEnabled = true,
            IsNavigationPanelEnabled = true,
            IsCenterContentEnabled = true,
            IsDynamicToolbarEnabled = true,
            IsStatusBarEnabled = true,
        };
        options.Motion.IsEnabled = true;
        var sut = new FlourishShellBuilder(options);

        Assert.Same(sut, sut.UseTitleBar(false));
        Assert.Same(sut, sut.UseMultiProject(false));
        Assert.Same(sut, sut.UseNavigation(false));
        Assert.Same(sut, sut.UseCenterContent(enabled: false, contentWidth: 1200));
        Assert.Same(sut, sut.UseDynamicToolbar(false));
        Assert.Same(sut, sut.UseMotion(false));
        Assert.Same(sut, sut.UseStatusBar(false));

        Assert.False(options.IsTitlebarEnabled);
        Assert.False(options.IsMultiProjectEnabled);
        Assert.False(options.IsNavigationPanelEnabled);
        Assert.False(options.IsCenterContentEnabled);
        Assert.False(options.IsDynamicToolbarEnabled);
        Assert.False(options.Motion.IsEnabled);
        Assert.False(options.IsStatusBarEnabled);
    }

    [Fact]
    public void UseMaterialEffect_WithNone_DisablesMaterialEffect()
    {
        var options = new FlourishShellOptions
        {
            IsMaterialEffectEnabled = true,
            MaterialEffect = MaterialEffect.Mica,
        };
        var sut = new FlourishShellBuilder(options);

        var result = sut.UseMaterialEffect(MaterialEffect.None);

        Assert.Same(sut, result);
        Assert.False(options.IsMaterialEffectEnabled);
        Assert.Equal(MaterialEffect.None, options.MaterialEffect);
    }

    [Fact]
    public void UseTips_WithNegativeDelay_ThrowsArgumentOutOfRangeException()
    {
        var sut = new FlourishShellBuilder(new FlourishShellOptions());

        var exception = Assert.Throws<ArgumentOutOfRangeException>(() => sut.UseTips(-1));

        Assert.Equal("delay", exception.ParamName);
    }

    [Fact]
    public void UseMaterialEffect_WithUnknownValue_ThrowsArgumentOutOfRangeException()
    {
        var sut = new FlourishShellBuilder(new FlourishShellOptions());

        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            sut.UseMaterialEffect((MaterialEffect)int.MaxValue)
        );

        Assert.Equal("effect", exception.ParamName);
    }

    [Fact]
    public void UseThemeColors_WithNull_ThrowsArgumentNullException()
    {
        var sut = new FlourishShellBuilder(new FlourishShellOptions());

        var exception = Assert.Throws<ArgumentNullException>(() =>
            sut.UseThemeColors(null!)
        );

        Assert.Equal("colors", exception.ParamName);
    }

    [Fact]
    public void ThemeColors_WithTranslucentColor_ThrowsArgumentException()
    {
        var exception = Assert.Throws<ArgumentException>(() =>
            new FlourishThemeColors(
                Color.FromArgb(0x80, 0x12, 0x34, 0x56),
                Colors.Black,
                Colors.White
            )
        );

        Assert.Equal("primary", exception.ParamName);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    [InlineData(double.NegativeInfinity)]
    public void UseCornerRadius_WithInvalidValue_ThrowsArgumentOutOfRangeException(
        double radius
    )
    {
        var sut = new FlourishShellBuilder(new FlourishShellOptions());

        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            sut.UseCornerRadius(radius)
        );

        Assert.Equal("radius", exception.ParamName);
    }

    [Fact]
    public void UseCornerRadius_WithZero_AllowsSquareGeometry()
    {
        var options = new FlourishShellOptions();
        var sut = new FlourishShellBuilder(options);

        Assert.Same(sut, sut.UseCornerRadius(0));
        Assert.Equal(0, options.CornerRadius);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    [InlineData(double.NegativeInfinity)]
    public void UseCenterContent_WithInvalidWidth_ThrowsArgumentOutOfRangeException(
        double contentWidth
    )
    {
        var sut = new FlourishShellBuilder(new FlourishShellOptions());

        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            sut.UseCenterContent(enabled: true, contentWidth: contentWidth)
        );

        Assert.Equal("contentWidth", exception.ParamName);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void UseGlobalFont_WithMissingFamily_ThrowsArgumentException(
        string? fontFamily
    )
    {
        var sut = new FlourishShellBuilder(new FlourishShellOptions());

        var exception = Assert.Throws<ArgumentException>(() =>
            sut.UseGlobalFont(fontFamily!, 12, 14, 16, 16, 24, 32)
        );

        Assert.Equal("fontFamily", exception.ParamName);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    [InlineData(double.NegativeInfinity)]
    public void UseGlobalFont_WithInvalidTier_ThrowsArgumentOutOfRangeException(
        double size
    )
    {
        var sut = new FlourishShellBuilder(new FlourishShellOptions());

        Assert.Equal(
            "smallFontSize",
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                sut.UseGlobalFont("Segoe UI", size, 14, 16, 18, 20, 24)
            ).ParamName
        );
        Assert.Equal(
            "standardFontSize",
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                sut.UseGlobalFont("Segoe UI", 12, size, 16, 18, 20, 24)
            ).ParamName
        );
        Assert.Equal(
            "iconFontSize",
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                sut.UseGlobalFont("Segoe UI", 12, 14, size, 18, 20, 24)
            ).ParamName
        );
        Assert.Equal(
            "largeFontSize",
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                sut.UseGlobalFont("Segoe UI", 12, 14, 16, size, 20, 24)
            ).ParamName
        );
        Assert.Equal(
            "extraLargeFontSize",
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                sut.UseGlobalFont("Segoe UI", 12, 14, 16, 18, size, 24)
            ).ParamName
        );
        Assert.Equal(
            "headerSizeFontSize",
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                sut.UseGlobalFont("Segoe UI", 12, 14, 16, 18, 20, size)
            ).ParamName
        );
    }

    [Fact]
    public void UseGlobalFont_WithIndependentPositiveTiers_AcceptsEqualAndUnorderedSizes()
    {
        var options = new FlourishShellOptions();
        var sut = new FlourishShellBuilder(options);

        sut.UseGlobalFont("Segoe UI", 30, 14, 16, 16, 12, 10);

        Assert.Equal(30, options.FontSizeSmall);
        Assert.Equal(14, options.FontSizeStandard);
        Assert.Equal(16, options.FontSizeIcon);
        Assert.Equal(16, options.FontSizeLarge);
        Assert.Equal(12, options.FontSizeExtraLarge);
        Assert.Equal(10, options.FontSizeHeaderSize);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void SetOverrideFont_WithMissingFamily_ThrowsArgumentException(
        string? fontFamily
    )
    {
        var sut = new FlourishShellBuilder(new FlourishShellOptions());

        var exception = Assert.Throws<ArgumentException>(() =>
            sut.SetOverrideFont<OverrideFontPage>(fontFamily!, null, null, null, null, null, null)
        );

        Assert.Equal("fontFamily", exception.ParamName);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    [InlineData(double.NegativeInfinity)]
    public void SetOverrideFont_WithInvalidTier_ThrowsArgumentOutOfRangeException(
        double size
    )
    {
        var sut = new FlourishShellBuilder(new FlourishShellOptions());

        Assert.Equal(
            "smallFontSize",
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                sut.SetOverrideFont<OverrideFontPage>("Consolas", size, null, null, null, null, null)
            ).ParamName
        );
        Assert.Equal(
            "standardFontSize",
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                sut.SetOverrideFont<OverrideFontPage>("Consolas", null, size, null, null, null, null)
            ).ParamName
        );
        Assert.Equal(
            "iconFontSize",
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                sut.SetOverrideFont<OverrideFontPage>("Consolas", null, null, size, null, null, null)
            ).ParamName
        );
        Assert.Equal(
            "largeFontSize",
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                sut.SetOverrideFont<OverrideFontPage>("Consolas", null, null, null, size, null, null)
            ).ParamName
        );
        Assert.Equal(
            "extraLargeFontSize",
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                sut.SetOverrideFont<OverrideFontPage>("Consolas", null, null, null, null, size, null)
            ).ParamName
        );
        Assert.Equal(
            "headerSizeFontSize",
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                sut.SetOverrideFont<OverrideFontPage>("Consolas", null, null, null, null, null, size)
            ).ParamName
        );
    }

    [Fact]
    public void SetOverrideFont_WithIndependentPositiveTiers_AcceptsEqualAndUnorderedSizes()
    {
        var options = new FlourishShellOptions();
        var sut = new FlourishShellBuilder(options);

        sut.SetOverrideFont<OverrideFontPage>(
            "Consolas",
            30,
            14,
            16,
            16,
            12,
            10
        );

        Assert.Equal(
            new FlourishPageFontOverride("Consolas", 30, 14, 16, 16, 12, 10),
            options.PageFontOverridesByPageType[typeof(OverrideFontPage)]
        );
    }

    [Fact]
    public void SetOverrideFont_WithAbstractPage_ThrowsArgumentException()
    {
        var sut = new FlourishShellBuilder(new FlourishShellOptions());

        var exception = Assert.Throws<ArgumentException>(() =>
            sut.SetOverrideFont<AbstractOverrideFontPage>("Consolas", null, null, null, null, null, null)
        );

        Assert.Equal("TPage", exception.ParamName);
    }

    private sealed class OverrideFontPage : System.Windows.Controls.Page { }

    private abstract class AbstractOverrideFontPage : System.Windows.Controls.Page { }
}
