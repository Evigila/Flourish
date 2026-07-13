using System.Windows;
using System.Windows.Media;
using ArkheideSystem.Flourish.Internal.Configuration;
using ArkheideSystem.Flourish.Internal.Composition;

namespace ArkheideSystem.Flourish.Test.Internal.Composition;

public sealed class FlourishWindowPropertyBuilderTests
{
    [Fact]
    public void ConfigurationMethods_WithValidValues_UpdateOptionsAndReturnBuilder()
    {
        var options = new FlourishShellOptions();
        var sut = new FlourishWindowPropertyBuilder(options);

        Assert.Same(sut, sut.SetWindowSize(1440, 900));
        Assert.Same(sut, sut.SetWindowMinSize(640, 480));
        Assert.Same(sut, sut.SetWindowMaxSize(2560, 1440));
        Assert.Same(sut, sut.SetManualWindowPosition(-120, 45));
        Assert.Same(sut, sut.SetWindowState(WindowState.Maximized));
        Assert.Same(sut, sut.SetWindowResizeMode(ResizeMode.NoResize));
        Assert.Same(sut, sut.UseTextStrategy());
        Assert.Same(sut, sut.SnapsToDevicePixels());
        Assert.Same(sut, sut.UseLayoutRounding());
        Assert.Same(sut, sut.UseTopmost());
        Assert.Same(sut, sut.ShowInTaskbar(false));
        Assert.Same(sut, sut.SetTrayExit());

        Assert.Equal(1440, options.WindowWidth);
        Assert.Equal(900, options.WindowHeight);
        Assert.Equal(640, options.WindowMinWidth);
        Assert.Equal(480, options.WindowMinHeight);
        Assert.Equal(2560, options.WindowMaxWidth);
        Assert.Equal(1440, options.WindowMaxHeight);
        Assert.Equal(-120, options.WindowLeft);
        Assert.Equal(45, options.WindowTop);
        Assert.Equal(WindowStartupLocation.Manual, options.WindowStartupLocation);
        Assert.Equal(WindowState.Maximized, options.WindowState);
        Assert.Equal(ResizeMode.NoResize, options.WindowResizeMode);
        Assert.Equal(TextFormattingMode.Display, options.WindowTextFormattingMode);
        Assert.Equal(TextRenderingMode.ClearType, options.WindowTextRenderingMode);
        Assert.True(options.WindowSnapsToDevicePixels);
        Assert.True(options.WindowUseLayoutRounding);
        Assert.True(options.WindowTopmost);
        Assert.False(options.WindowShowInTaskbar);
        Assert.True(options.IsTrayExitEnabled);
    }

    [Fact]
    public void RenderingMethods_WithExplicitValues_UpdateOptionsAndReturnBuilder()
    {
        var options = new FlourishShellOptions
        {
            WindowTextFormattingMode = TextFormattingMode.Display,
            WindowTextRenderingMode = TextRenderingMode.ClearType,
            WindowSnapsToDevicePixels = true,
            WindowUseLayoutRounding = true,
        };
        var sut = new FlourishWindowPropertyBuilder(options);

        Assert.Same(
            sut,
            sut.UseTextStrategy(TextFormattingMode.Ideal, TextRenderingMode.Grayscale)
        );
        Assert.Same(sut, sut.SnapsToDevicePixels(false));
        Assert.Same(sut, sut.UseLayoutRounding(false));

        Assert.Equal(TextFormattingMode.Ideal, options.WindowTextFormattingMode);
        Assert.Equal(TextRenderingMode.Grayscale, options.WindowTextRenderingMode);
        Assert.False(options.WindowSnapsToDevicePixels);
        Assert.False(options.WindowUseLayoutRounding);
    }

    [Fact]
    public void RenderingOptions_WithoutConfiguration_RemainUnset()
    {
        var options = new FlourishShellOptions();

        Assert.Null(options.WindowTextFormattingMode);
        Assert.Null(options.WindowTextRenderingMode);
        Assert.Null(options.WindowSnapsToDevicePixels);
        Assert.Null(options.WindowUseLayoutRounding);
    }

    [Fact]
    public void SetTrayExit_WithFalse_DisablesTrayExit()
    {
        var options = new FlourishShellOptions { IsTrayExitEnabled = true };
        var sut = new FlourishWindowPropertyBuilder(options);

        var result = sut.SetTrayExit(false);

        Assert.Same(sut, result);
        Assert.False(options.IsTrayExitEnabled);
    }

    [Fact]
    public void SetWindowMaxSize_WithPositiveInfinity_UpdatesOptions()
    {
        var options = new FlourishShellOptions();
        var sut = new FlourishWindowPropertyBuilder(options);

        var result = sut.SetWindowMaxSize();

        Assert.Same(sut, result);
        Assert.Equal(double.PositiveInfinity, options.WindowMaxWidth);
        Assert.Equal(double.PositiveInfinity, options.WindowMaxHeight);
    }

    [Fact]
    public void SetWindowPosition_WithNonManualValue_ClearsManualCoordinates()
    {
        var options = new FlourishShellOptions();
        var sut = new FlourishWindowPropertyBuilder(options);
        sut.SetManualWindowPosition(15, 25);

        var result = sut.SetWindowPosition(WindowStartupLocation.CenterOwner);

        Assert.Same(sut, result);
        Assert.Equal(WindowStartupLocation.CenterOwner, options.WindowStartupLocation);
        Assert.Null(options.WindowLeft);
        Assert.Null(options.WindowTop);
    }

    [Fact]
    public void SetWindowPosition_WithManualValue_PreservesCoordinates()
    {
        var options = new FlourishShellOptions { WindowLeft = 15, WindowTop = 25 };
        var sut = new FlourishWindowPropertyBuilder(options);

        sut.SetWindowPosition(WindowStartupLocation.Manual);

        Assert.Equal(WindowStartupLocation.Manual, options.WindowStartupLocation);
        Assert.Equal(15, options.WindowLeft);
        Assert.Equal(25, options.WindowTop);
    }

    [Theory]
    [InlineData("width", 0)]
    [InlineData("width", -1)]
    [InlineData("width", double.NaN)]
    [InlineData("width", double.PositiveInfinity)]
    [InlineData("height", 0)]
    [InlineData("height", -1)]
    [InlineData("height", double.NaN)]
    [InlineData("height", double.NegativeInfinity)]
    public void SetWindowSize_WithNonPositiveOrNonFiniteValue_ThrowsArgumentOutOfRangeException(
        string parameterName,
        double value
    )
    {
        var sut = new FlourishWindowPropertyBuilder(new FlourishShellOptions());

        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
        {
            if (parameterName == "width")
            {
                sut.SetWindowSize(value, 720);
            }
            else
            {
                sut.SetWindowSize(1100, value);
            }
        });

        Assert.Equal(parameterName, exception.ParamName);
    }

    [Theory]
    [InlineData("minWidth", 0)]
    [InlineData("minWidth", double.NaN)]
    [InlineData("minHeight", -1)]
    [InlineData("minHeight", double.PositiveInfinity)]
    public void SetWindowMinSize_WithNonPositiveOrNonFiniteValue_ThrowsArgumentOutOfRangeException(
        string parameterName,
        double value
    )
    {
        var sut = new FlourishWindowPropertyBuilder(new FlourishShellOptions());

        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
        {
            if (parameterName == "minWidth")
            {
                sut.SetWindowMinSize(value, 560);
            }
            else
            {
                sut.SetWindowMinSize(820, value);
            }
        });

        Assert.Equal(parameterName, exception.ParamName);
    }

    [Theory]
    [InlineData("maxWidth", 0)]
    [InlineData("maxWidth", double.NaN)]
    [InlineData("maxHeight", -1)]
    [InlineData("maxHeight", double.NegativeInfinity)]
    public void SetWindowMaxSize_WithNonPositiveOrInvalidValue_ThrowsArgumentOutOfRangeException(
        string parameterName,
        double value
    )
    {
        var sut = new FlourishWindowPropertyBuilder(new FlourishShellOptions());

        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
        {
            if (parameterName == "maxWidth")
            {
                sut.SetWindowMaxSize(value, 1080);
            }
            else
            {
                sut.SetWindowMaxSize(1920, value);
            }
        });

        Assert.Equal(parameterName, exception.ParamName);
    }

    [Theory]
    [InlineData("minWidth")]
    [InlineData("minHeight")]
    public void SetWindowMinSize_WhenMinimumExceedsMaximum_ThrowsArgumentOutOfRangeException(
        string parameterName
    )
    {
        var options = new FlourishShellOptions
        {
            WindowMaxWidth = 1000,
            WindowMaxHeight = 700,
        };
        var sut = new FlourishWindowPropertyBuilder(options);

        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
        {
            if (parameterName == "minWidth")
            {
                sut.SetWindowMinSize(1001, 600);
            }
            else
            {
                sut.SetWindowMinSize(900, 701);
            }
        });

        Assert.Equal(parameterName, exception.ParamName);
    }

    [Theory]
    [InlineData("maxWidth")]
    [InlineData("maxHeight")]
    public void SetWindowMaxSize_WhenMaximumIsBelowMinimum_ThrowsArgumentOutOfRangeException(
        string parameterName
    )
    {
        var options = new FlourishShellOptions
        {
            WindowMinWidth = 800,
            WindowMinHeight = 600,
        };
        var sut = new FlourishWindowPropertyBuilder(options);

        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
        {
            if (parameterName == "maxWidth")
            {
                sut.SetWindowMaxSize(799, 900);
            }
            else
            {
                sut.SetWindowMaxSize(1000, 599);
            }
        });

        Assert.Equal(parameterName, exception.ParamName);
    }

    [Theory]
    [InlineData("left", double.NaN)]
    [InlineData("left", double.PositiveInfinity)]
    [InlineData("top", double.NaN)]
    [InlineData("top", double.NegativeInfinity)]
    public void SetManualWindowPosition_WithNonFiniteValue_ThrowsArgumentOutOfRangeException(
        string parameterName,
        double value
    )
    {
        var sut = new FlourishWindowPropertyBuilder(new FlourishShellOptions());

        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
        {
            if (parameterName == "left")
            {
                sut.SetManualWindowPosition(value, 0);
            }
            else
            {
                sut.SetManualWindowPosition(0, value);
            }
        });

        Assert.Equal(parameterName, exception.ParamName);
    }

    [Fact]
    public void SetWindowState_WithUndefinedValue_ThrowsArgumentOutOfRangeException()
    {
        var sut = new FlourishWindowPropertyBuilder(new FlourishShellOptions());

        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            sut.SetWindowState((WindowState)int.MaxValue)
        );

        Assert.Equal("windowState", exception.ParamName);
    }

    [Fact]
    public void SetWindowResizeMode_WithUndefinedValue_ThrowsArgumentOutOfRangeException()
    {
        var sut = new FlourishWindowPropertyBuilder(new FlourishShellOptions());

        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            sut.SetWindowResizeMode((ResizeMode)int.MaxValue)
        );

        Assert.Equal("resizeMode", exception.ParamName);
    }

    [Fact]
    public void UseTextStrategy_WithUndefinedFormattingMode_ThrowsArgumentOutOfRangeException()
    {
        var options = new FlourishShellOptions();
        var sut = new FlourishWindowPropertyBuilder(options);

        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            sut.UseTextStrategy((TextFormattingMode)int.MaxValue)
        );

        Assert.Equal("textFormattingMode", exception.ParamName);
        Assert.Null(options.WindowTextFormattingMode);
        Assert.Null(options.WindowTextRenderingMode);
    }

    [Fact]
    public void UseTextStrategy_WithUndefinedRenderingMode_DoesNotPartiallyUpdateOptions()
    {
        var options = new FlourishShellOptions();
        var sut = new FlourishWindowPropertyBuilder(options);

        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            sut.UseTextStrategy(
                TextFormattingMode.Display,
                (TextRenderingMode)int.MaxValue
            )
        );

        Assert.Equal("textRenderingMode", exception.ParamName);
        Assert.Null(options.WindowTextFormattingMode);
        Assert.Null(options.WindowTextRenderingMode);
    }
}
