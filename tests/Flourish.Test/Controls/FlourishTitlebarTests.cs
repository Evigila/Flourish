using System.Windows.Media;
using System.Windows.Media.Imaging;
using ArkheideSystem.Flourish.Controls;

namespace ArkheideSystem.Flourish.Test.Controls;

public sealed class FlourishTitlebarTests
{
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
            FlourishTitlebar.TrimTransparentPixels(source)
        );

        Assert.Equal(2, result.PixelWidth);
        Assert.Equal(2, result.PixelHeight);
    }
}
