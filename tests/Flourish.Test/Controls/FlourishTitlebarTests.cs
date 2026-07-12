using System.Windows.Media;
using System.Windows.Media.Imaging;
using ArkheideSystem.Flourish.Controls;
using ArkheideSystem.Flourish.Internal.Imaging;
using ArkheideSystem.Flourish.Views.Windows;

namespace ArkheideSystem.Flourish.Test.Controls;

public sealed class FlourishTitlebarTests
{
    [Fact]
    public void BreadcrumbFeatureRefresh_DoesNotForceAnEmptyNavigationHostVisible()
    {
        var state = new TitleBarBreadcrumbVisibilityState();

        state.SetFeatureEnabled(true);
        state.SetNavigationState(isVisible: false, canGoBack: false, canGoForward: false);

        Assert.False(state.IsVisible);
        Assert.False(state.IsBackVisible);
        Assert.False(state.IsForwardVisible);

        // Navigation-panel and search runtime changes both refresh the feature flags.
        // Reapplying an enabled feature must not replace the current navigation state.
        state.SetFeatureEnabled(true);
        state.SetFeatureEnabled(true);

        Assert.False(state.IsVisible);
        Assert.False(state.IsBackVisible);
        Assert.False(state.IsForwardVisible);
    }

    [Fact]
    public void BreadcrumbFeatureToggle_PreservesTheCurrentNavigationState()
    {
        var state = new TitleBarBreadcrumbVisibilityState();
        state.SetNavigationState(isVisible: true, canGoBack: true, canGoForward: true);

        state.SetFeatureEnabled(true);

        Assert.True(state.IsVisible);
        Assert.True(state.IsBackVisible);
        Assert.True(state.IsForwardVisible);

        state.SetFeatureEnabled(false);

        Assert.False(state.IsVisible);
        Assert.False(state.IsBackVisible);
        Assert.False(state.IsForwardVisible);

        state.SetFeatureEnabled(true);

        Assert.True(state.IsVisible);
        Assert.True(state.IsBackVisible);
        Assert.True(state.IsForwardVisible);
    }

    [Fact]
    public void AlwaysVisibleBreadcrumb_LeavesOneDisabledBackButtonWithoutHistory()
    {
        var state = new TitleBarBreadcrumbVisibilityState();
        state.SetFeatureEnabled(true);
        state.SetNavigationState(isVisible: true, canGoBack: false, canGoForward: false);

        Assert.True(state.IsVisible);
        Assert.True(state.IsBackVisible);
        Assert.False(state.IsForwardVisible);
        Assert.False(state.CanGoBack);
    }

    [Fact]
    public void TrimTransparentPixels_RemovesTransparentImageMargin()
    {
        const int width = 4;
        const int height = 4;
        const int bytesPerPixel = 4;
        var stride = width * bytesPerPixel;
        var pixels = new byte[stride * height];
        for (var y = 1; y <= 2; y++)
        {
            for (var x = 1; x <= 2; x++)
            {
                pixels[y * stride + x * bytesPerPixel + 3] = byte.MaxValue;
            }
        }

        var source = BitmapSource.Create(
            width,
            height,
            96,
            96,
            PixelFormats.Bgra32,
            null,
            pixels,
            stride
        );

        var result = Assert.IsAssignableFrom<BitmapSource>(
            TitleBarVisualAssets.TrimTransparentPixels(source)
        );

        Assert.Equal(2, result.PixelWidth);
        Assert.Equal(2, result.PixelHeight);
        Assert.True(result.IsFrozen);
    }

    [Fact]
    public void ThemeIconGeometries_AreFrozenForCrossThreadReuse()
    {
        Assert.True(TitleBarVisualAssets.SunIconGeometry.IsFrozen);
        Assert.True(TitleBarVisualAssets.MoonIconGeometry.IsFrozen);
    }
}
