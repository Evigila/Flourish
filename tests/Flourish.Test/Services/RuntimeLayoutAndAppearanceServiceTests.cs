using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using ArkheideSystem.Flourish.Abstract;
using ArkheideSystem.Flourish.Internal.Configuration;
using ArkheideSystem.Flourish.Services;

namespace ArkheideSystem.Flourish.Test.Services;

public sealed class RuntimeLayoutAndAppearanceServiceTests
{
    [Fact]
    public void ContentLayoutService_UsesStartupStateAndSuppressesNoOpChanges()
    {
        var sut = new ContentLayoutService(
            new FlourishShellOptions
            {
                IsCenterContentEnabled = true,
                CenterContentWidth = 960,
            }
        );
        var changes = 0;
        sut.Changed += (_, _) => changes++;

        sut.SetCenterContent(true, 960);
        sut.SetCenterContent(false, 1080);

        Assert.Equal(1, changes);
        Assert.Equal(
            new FlourishContentLayoutSettings(false, 1080, 1),
            sut.Current
        );
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    public void ContentLayoutService_RejectsInvalidWidths(double width)
    {
        var sut = new ContentLayoutService(new FlourishShellOptions());

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            sut.SetCenterContent(true, width)
        );
    }

    [Fact]
    public void AppearanceService_AppliesAndClearsOwnedOverrides()
    {
        var resources = new ResourceDictionary();
        var colors = new FlourishThemeColors(Colors.Red, Colors.Green, Colors.Blue);
        var sut = new AppearanceService(new FlourishShellOptions());
        sut.Attach(Dispatcher.CurrentDispatcher, resources, FlourishTheme.Light);

        sut.SetAppearance(colors, 7);

        var overrides = Assert.Single(resources.MergedDictionaries);
        Assert.Equal(Colors.Red, overrides["FlourishPrimaryColor"]);
        Assert.Equal(
            new CornerRadius(7),
            overrides["FlourishSurfaceCornerRadius"]
        );

        sut.SetAppearance(colors: null, cornerRadius: null);

        Assert.Empty(overrides);
        Assert.Equal(2, sut.Current.Version);
    }

    [Fact]
    public void AppearanceService_RaisesOneChangeForAtomicUpdate()
    {
        var sut = new AppearanceService(new FlourishShellOptions());
        FlourishAppearanceChangedEventArgs? change = null;
        var changes = 0;
        sut.Changed += (_, args) =>
        {
            changes++;
            change = args;
        };
        var colors = new FlourishThemeColors(Colors.Red, Colors.Green, Colors.Blue);

        sut.SetAppearance(colors, 4);
        sut.SetAppearance(colors, 4);

        Assert.Equal(1, changes);
        Assert.Null(change!.Previous.ThemeColors);
        Assert.Equal(colors, change.Current.ThemeColors);
        Assert.Equal(4, change.Current.CornerRadius);
    }
}
