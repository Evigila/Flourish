using System.Windows;
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

        Assert.Same(sut, sut.InitWindowSize(1440, 900));
        Assert.Same(sut, sut.InitWindowMinSize(640, 480));
        Assert.Same(sut, sut.InitWindowMaxSize(2560, 1440));
        Assert.Same(sut, sut.InitManualWindowPosition(-120, 45));
        Assert.Same(sut, sut.InitWindowState(WindowState.Maximized));
        Assert.Same(sut, sut.InitWindowResizeMode(ResizeMode.NoResize));
        Assert.Same(sut, sut.UseTopmost());
        Assert.Same(sut, sut.InitShownInTaskbar(false));
        Assert.Same(sut, sut.UseTrayExit());

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
        Assert.True(options.WindowTopmost);
        Assert.False(options.WindowShowInTaskbar);
        Assert.True(options.IsTrayExitEnabled);
    }

    [Fact]
    public void SetTrayExit_WithFalse_DisablesTrayExit()
    {
        var options = new FlourishShellOptions { IsTrayExitEnabled = true };
        var sut = new FlourishWindowPropertyBuilder(options);

        var result = sut.UseTrayExit(false);

        Assert.Same(sut, result);
        Assert.False(options.IsTrayExitEnabled);
    }

    [Fact]
    public void PreferenceAwareMethods_EnableTheirIndependentPolicies()
    {
        var options = new FlourishShellOptions();
        var sut = new FlourishWindowPropertyBuilder(options);

        sut.InitWindowSize(1280, 720, true)
            .InitManualWindowPosition(20, 30, true)
            .InitWindowState(WindowState.Normal, true)
            .UseTopmost(false, true)
            .UseTrayExit(true, true);

        Assert.True(options.UsePersistedWindowSize);
        Assert.True(options.UsePersistedWindowPosition);
        Assert.True(options.UsePersistedWindowState);
        Assert.True(options.UsePersistedWindowTopmost);
        Assert.True(options.UsePersistedTrayExit);
    }

    [Fact]
    public void SetWindowMaxSize_WithPositiveInfinity_UpdatesOptions()
    {
        var options = new FlourishShellOptions();
        var sut = new FlourishWindowPropertyBuilder(options);

        var result = sut.InitWindowMaxSize();

        Assert.Same(sut, result);
        Assert.Equal(double.PositiveInfinity, options.WindowMaxWidth);
        Assert.Equal(double.PositiveInfinity, options.WindowMaxHeight);
    }

    [Fact]
    public void SetWindowPosition_WithNonManualValue_ClearsManualCoordinates()
    {
        var options = new FlourishShellOptions();
        var sut = new FlourishWindowPropertyBuilder(options);
        sut.InitManualWindowPosition(15, 25);

        var result = sut.InitWindowPosition(WindowStartupLocation.CenterOwner);

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

        sut.InitWindowPosition(WindowStartupLocation.Manual);

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
                sut.InitWindowSize(value, 720);
            }
            else
            {
                sut.InitWindowSize(1100, value);
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
                sut.InitWindowMinSize(value, 560);
            }
            else
            {
                sut.InitWindowMinSize(820, value);
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
                sut.InitWindowMaxSize(value, 1080);
            }
            else
            {
                sut.InitWindowMaxSize(1920, value);
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
                sut.InitWindowMinSize(1001, 600);
            }
            else
            {
                sut.InitWindowMinSize(900, 701);
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
                sut.InitWindowMaxSize(799, 900);
            }
            else
            {
                sut.InitWindowMaxSize(1000, 599);
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
                sut.InitManualWindowPosition(value, 0);
            }
            else
            {
                sut.InitManualWindowPosition(0, value);
            }
        });

        Assert.Equal(parameterName, exception.ParamName);
    }

    [Fact]
    public void SetWindowState_WithUndefinedValue_ThrowsArgumentOutOfRangeException()
    {
        var sut = new FlourishWindowPropertyBuilder(new FlourishShellOptions());

        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            sut.InitWindowState((WindowState)int.MaxValue)
        );

        Assert.Equal("windowState", exception.ParamName);
    }

    [Fact]
    public void SetWindowResizeMode_WithUndefinedValue_ThrowsArgumentOutOfRangeException()
    {
        var sut = new FlourishWindowPropertyBuilder(new FlourishShellOptions());

        var exception = Assert.Throws<ArgumentOutOfRangeException>(() =>
            sut.InitWindowResizeMode((ResizeMode)int.MaxValue)
        );

        Assert.Equal("resizeMode", exception.ParamName);
    }

}
