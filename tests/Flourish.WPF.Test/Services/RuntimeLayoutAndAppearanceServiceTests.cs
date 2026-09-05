using System;
using Xunit;
using ArkheideSystem.Flourish.Abstract;
using ArkheideSystem.Flourish.Appearance;
using ArkheideSystem.Flourish.Layout;

using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;

namespace ArkheideSystem.Flourish.WPF.Test.Services;

public sealed class RuntimeLayoutAndAppearanceServiceTests
{
    [Fact]
    public void ContentLayoutService_UsesStartupStateAndSuppressesNoOpChanges()
    {
        var sut = new ContentLayoutService(
            new LayoutOptions { IsCenterContentEnabled = true, CenterContentWidth = 960 }
        );
        var changes = 0;
        sut.Changed += (_, _) => changes++;

        sut.SetCenterContent(true, 960);
        sut.SetCenterContent(false, 1080);

        Assert.Equal(1, changes);
        Assert.Equal(new ContentLayoutSettings(false, 1080, 1), sut.Current);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    public void ContentLayoutService_RejectsInvalidWidths(double width)
    {
        var sut = new ContentLayoutService(new LayoutOptions());

        Assert.Throws<ArgumentOutOfRangeException>(() => sut.SetCenterContent(true, width));
    }

    [Fact]
    public void AppearanceService_AppliesAndClearsOwnedOverrides()
    {
        var resources = new ResourceDictionary();
        var colors = new ThemeColors(Colors.Red, Colors.Green, Colors.Blue);
        var sut = new AppearanceService(new AppearanceOptions());
        sut.Attach(Dispatcher.CurrentDispatcher, resources, ApplicationTheme.Light);

        sut.SetAppearance(colors, 7);

        var overrides = Assert.Single(resources.MergedDictionaries);
        Assert.Equal(Colors.Red, overrides["FlourishPrimaryColor"]);
        Assert.Equal(new CornerRadius(7), overrides["FlourishSurfaceCornerRadius"]);

        sut.SetAppearance(colors: null, cornerRadius: null);

        Assert.Empty(overrides);
        Assert.Equal(2, sut.Current.Version);
    }

    [Fact]
    public void AppearanceService_RaisesOneChangeForAtomicUpdate()
    {
        var sut = new AppearanceService(new AppearanceOptions());
        StateTransitionEventArgs<AppearanceSettings>? change = null;
        var changes = 0;
        sut.Changed += (_, args) =>
        {
            changes++;
            change = args;
        };
        var colors = new ThemeColors(Colors.Red, Colors.Green, Colors.Blue);

        sut.SetAppearance(colors, 4);
        sut.SetAppearance(colors, 4);

        Assert.Equal(1, changes);
        Assert.Null(change!.Previous.ThemeColors);
        Assert.Equal(colors, change.Current.ThemeColors);
        Assert.Equal(4, change.Current.CornerRadius);
    }
}
