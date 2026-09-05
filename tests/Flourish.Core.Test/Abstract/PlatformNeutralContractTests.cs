using ArkheideSystem.Flourish.Abstract;
using Xunit;

namespace ArkheideSystem.Flourish.Core.Test.Abstract;

public sealed class PlatformNeutralContractTests
{
    [Theory]
    [InlineData(ApplicationTheme.System, ApplicationTheme.Dark, true)]
    [InlineData(ApplicationTheme.Dark, ApplicationTheme.Light, false)]
    public void ThemeState_ReportsTheEffectiveTheme(
        ApplicationTheme requested,
        ApplicationTheme effective,
        bool isDark
    )
    {
        var state = new ThemeState(requested, effective);

        Assert.Equal(requested, state.RequestedTheme);
        Assert.Equal(effective, state.EffectiveTheme);
        Assert.Equal(isDark, state.IsDark);
    }

    [Fact]
    public void NavigationPanelState_PreservesPlatformNeutralLayoutValues()
    {
        var state = new NavigationPanelState(
            IsEnabled: true,
            IsOpen: false,
            Direction: NavigationPanelDirection.Right,
            OpenWidth: 280,
            ClosedWidth: 48,
            MinWidth: 200,
            MaxWidth: 420,
            Version: 3
        );

        Assert.True(state.IsEnabled);
        Assert.False(state.IsOpen);
        Assert.Equal(NavigationPanelDirection.Right, state.Direction);
        Assert.Equal(280, state.OpenWidth);
        Assert.Equal(48, state.ClosedWidth);
        Assert.Equal(200, state.MinWidth);
        Assert.Equal(420, state.MaxWidth);
        Assert.Equal(3, state.Version);
    }

    [Fact]
    public void NavigationAndShortcutEnums_PreserveTheirPublicValues()
    {
        Assert.Equal(0, (int)NavigationPanelDirection.Left);
        Assert.Equal(1, (int)NavigationPanelDirection.Right);
        Assert.Equal(0, (int)PageCacheMode.Enabled);
        Assert.Equal(1, (int)PageCacheMode.Disabled);
        Assert.Equal(0, (int)ShortcutScope.Application);
        Assert.Equal(2, (int)ShortcutScope.Page);
        Assert.Equal(0, (int)ShortcutConflictPolicy.Reject);
        Assert.Equal(2, (int)ShortcutConflictPolicy.Append);
        Assert.Equal(0, (int)ShellRegion.TitleBarStart);
        Assert.Equal(13, (int)ShellRegion.TitleBarApplicationInfo);
    }
}
