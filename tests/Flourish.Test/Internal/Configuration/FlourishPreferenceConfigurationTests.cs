using System.Windows;
using ArkheideSystem.Flourish.Abstract;
using ArkheideSystem.Flourish.Internal.Configuration;
using Microsoft.Extensions.Configuration;

namespace ArkheideSystem.Flourish.Test.Internal.Configuration;

public sealed class FlourishPreferenceConfigurationTests
{
    [Fact]
    public void Apply_WhenPreferenceIsDisabled_KeepsBuilderFallback()
    {
        var configuration = Build(
            ("Flourish:Preferences:Locale", "zh-CN"),
            ("Flourish:Preferences:Window:Size:Width", "900"),
            ("Flourish:Preferences:Window:Size:Height", "700")
        );
        var data = new FlourishDataOptions
        {
            Locale = "en-US",
            UsePersistedLocale = false,
        };
        var shell = new FlourishShellOptions
        {
            WindowWidth = 1200,
            WindowHeight = 800,
            UsePersistedWindowSize = false,
        };

        FlourishPreferenceConfiguration.Apply(configuration, data, shell);

        Assert.Equal("en-US", data.Locale);
        Assert.Equal(1200, shell.WindowWidth);
        Assert.Equal(800, shell.WindowHeight);
    }

    [Fact]
    public void Apply_WhenPreferenceIsEnabled_AppliesCompleteValidatedGroups()
    {
        var configuration = Build(
            ("Flourish:Preferences:Locale", "zh-cn"),
            ("Flourish:Preferences:Window:Size:Width", "900"),
            ("Flourish:Preferences:Window:Size:Height", "700"),
            ("Flourish:Preferences:Window:Position:Left", "40"),
            ("Flourish:Preferences:Window:Position:Top", "60"),
            ("Flourish:Preferences:Window:State", "Maximized"),
            ("Flourish:Preferences:Navigation:IsOpen", "false"),
            ("Flourish:Preferences:Navigation:OpenWidth", "320"),
            ("Flourish:Preferences:Interaction:SmoothScrolling", "false")
        );
        var data = new FlourishDataOptions { Locale = "en-US", UsePersistedLocale = true };
        var shell = new FlourishShellOptions
        {
            WindowMinWidth = 100,
            WindowMinHeight = 100,
            WindowMaxWidth = 2000,
            WindowMaxHeight = 2000,
            NavigationPaneMinWidth = 180,
            NavigationPaneMaxWidth = 520,
            UsePersistedWindowSize = true,
            UsePersistedWindowPosition = true,
            UsePersistedWindowState = true,
            UsePersistedNavigationOpenState = true,
            UsePersistedNavigationWidth = true,
            UsePersistedSmoothScroll = true,
        };

        FlourishPreferenceConfiguration.Apply(configuration, data, shell);

        Assert.Equal("zh-CN", data.Locale);
        Assert.Equal(900, shell.WindowWidth);
        Assert.Equal(700, shell.WindowHeight);
        Assert.Equal(40, shell.WindowLeft);
        Assert.Equal(60, shell.WindowTop);
        Assert.Equal(WindowStartupLocation.Manual, shell.WindowStartupLocation);
        Assert.Equal(WindowState.Maximized, shell.WindowState);
        Assert.False(shell.IsNavigationPanelInitiallyOpen);
        Assert.Equal(320, shell.OpenPaneWidth);
        Assert.False(shell.IsSmoothScrollingEnabled);
    }

    [Fact]
    public void Apply_WhenCompositeValueIsIncomplete_KeepsEntireFallbackGroup()
    {
        var configuration = Build(
            ("Flourish:Preferences:Window:Size:Width", "900"),
            ("Flourish:Preferences:Window:Position:Left", "40")
        );
        var shell = new FlourishShellOptions
        {
            WindowWidth = 1200,
            WindowHeight = 800,
            WindowLeft = 10,
            WindowTop = 20,
            WindowStartupLocation = WindowStartupLocation.Manual,
            UsePersistedWindowSize = true,
            UsePersistedWindowPosition = true,
        };

        FlourishPreferenceConfiguration.Apply(
            configuration,
            new FlourishDataOptions(),
            shell
        );

        Assert.Equal(1200, shell.WindowWidth);
        Assert.Equal(800, shell.WindowHeight);
        Assert.Equal(10, shell.WindowLeft);
        Assert.Equal(20, shell.WindowTop);
    }

    [Fact]
    public void Apply_DoesNotRestoreMinimizedState()
    {
        var configuration = Build(("Flourish:Preferences:Window:State", "Minimized"));
        var shell = new FlourishShellOptions
        {
            WindowState = WindowState.Maximized,
            UsePersistedWindowState = true,
        };

        FlourishPreferenceConfiguration.Apply(
            configuration,
            new FlourishDataOptions(),
            shell
        );

        Assert.Equal(WindowState.Maximized, shell.WindowState);
    }

    [Fact]
    public void Apply_WhenPersistedLocaleIsInvalid_KeepsBuilderFallback()
    {
        var configuration = Build(("Flourish:Preferences:Locale", "zh--CN"));
        var data = new FlourishDataOptions
        {
            Locale = "en-US",
            UsePersistedLocale = true,
        };

        FlourishPreferenceConfiguration.Apply(
            configuration,
            data,
            new FlourishShellOptions()
        );

        Assert.Equal("en-US", data.Locale);
    }

    [Fact]
    public void Apply_RestoresLastNavigationOnlyWhenRouteStillExists()
    {
        var configuration = Build(
            ("Flourish:Preferences:Navigation:LastKey", "Reports")
        );
        var shell = new FlourishShellOptions
        {
            InitialNavigationKey = "Home",
            InitialNavigationPageType = typeof(HomePage),
            UsePersistedLastNavigation = true,
        };
        shell.InitialNavigationRoutes.Add(
            new FlourishNavigationRoute("Home", typeof(HomePage))
        );
        shell.InitialNavigationRoutes.Add(
            new FlourishNavigationRoute("Reports", typeof(ReportsPage))
        );

        FlourishPreferenceConfiguration.Apply(
            configuration,
            new FlourishDataOptions(),
            shell
        );

        Assert.Equal("Reports", shell.InitialNavigationKey);
        Assert.Equal(typeof(ReportsPage), shell.InitialNavigationPageType);
    }

    private static IConfiguration Build(params (string Key, string Value)[] values) =>
        new ConfigurationBuilder()
            .AddInMemoryCollection(values.ToDictionary(item => item.Key, item => item.Value)!)
            .Build();

    private sealed class HomePage : System.Windows.Controls.Page;

    private sealed class ReportsPage : System.Windows.Controls.Page;
}
