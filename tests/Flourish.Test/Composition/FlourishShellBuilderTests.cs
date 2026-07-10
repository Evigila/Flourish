using ArkheideSystem.Flourish.Abstract;
using ArkheideSystem.Flourish.Configuration;
using ArkheideSystem.Flourish.Composition;

namespace ArkheideSystem.Flourish.Test.Composition;

public sealed class FlourishShellBuilderTests
{
    [Fact]
    public void FeatureMethods_ConfigureAndEnableFeatures()
    {
        var options = new FlourishShellOptions();
        var sut = new FlourishShellBuilder(options);

        Assert.Same(sut, sut.UseTitleBar());
        Assert.Same(sut, sut.UseNavigation());
        Assert.Same(sut, sut.UseDynamicToolbar());
        Assert.Same(sut, sut.UseTips(350));
        Assert.Same(sut, sut.UseMotion());
        Assert.Same(sut, sut.UseMaterialEffect(MaterialEffect.Mica));
        Assert.Same(sut, sut.UseGlobalFont("Arial", 15));
        Assert.Same(sut, sut.UseStatusBar());

        Assert.True(options.IsTitlebarEnabled);
        Assert.True(options.IsNavigationPanelEnabled);
        Assert.True(options.IsDynamicToolbarEnabled);
        Assert.True(options.IsTipsEnabled);
        Assert.Equal(350, options.Tips.InitialShowDelayMilliseconds);
        Assert.Equal(5, options.Tips.SpawnableMargin);
        Assert.True(options.Motion.IsEnabled);
        Assert.True(options.IsMaterialEffectEnabled);
        Assert.Equal(MaterialEffect.Mica, options.MaterialEffect);
        Assert.Equal("Arial", options.FontFamily);
        Assert.Equal(15, options.FontSize);
        Assert.True(options.IsStatusBarEnabled);
    }

    [Fact]
    public void BooleanFeatureMethods_WithFalse_DisableFeatures()
    {
        var options = new FlourishShellOptions
        {
            IsTitlebarEnabled = true,
            IsNavigationPanelEnabled = true,
            IsDynamicToolbarEnabled = true,
            IsStatusBarEnabled = true,
        };
        options.Motion.IsEnabled = true;
        var sut = new FlourishShellBuilder(options);

        Assert.Same(sut, sut.UseTitleBar(false));
        Assert.Same(sut, sut.UseNavigation(false));
        Assert.Same(sut, sut.UseDynamicToolbar(false));
        Assert.Same(sut, sut.UseMotion(false));
        Assert.Same(sut, sut.UseStatusBar(false));

        Assert.False(options.IsTitlebarEnabled);
        Assert.False(options.IsNavigationPanelEnabled);
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
            sut.UseGlobalFont(fontFamily!)
        );

        Assert.Equal("fontFamily", exception.ParamName);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    [InlineData(double.NegativeInfinity)]
    public void UseGlobalFont_WithInvalidSize_ThrowsArgumentOutOfRangeException(
        double fontSize
    )
    {
        var sut = new FlourishShellBuilder(new FlourishShellOptions());

        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            sut.UseGlobalFont("Segoe UI", fontSize)
        );

        Assert.Equal("fontSize", exception.ParamName);
    }
}
