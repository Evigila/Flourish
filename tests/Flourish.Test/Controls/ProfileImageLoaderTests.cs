using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ArkheideSystem.Flourish.Internal.Imaging;

namespace ArkheideSystem.Flourish.Test.Controls;

public sealed class ProfileImageLoaderTests
{
    [Theory]
    [InlineData("png", 800, 400, 256, 128)]
    [InlineData("jpg", 800, 400, 256, 128)]
    [InlineData("png", 400, 800, 128, 256)]
    [InlineData("jpg", 400, 800, 128, 256)]
    [InlineData("png", 512, 512, 256, 256)]
    [InlineData("png", 64, 32, 64, 32)]
    [InlineData("jpg", 64, 32, 64, 32)]
    public void Load_BoundsLongestEdgeWithoutUpscalingAndFreezes(
        string format,
        int width,
        int height,
        int expectedWidth,
        int expectedHeight
    )
    {
        using var imageFile = TemporaryImageFile.Create(format, width, height);

        var image = Assert.IsAssignableFrom<BitmapSource>(
            ProfileImageLoader.Load(imageFile.Path)
        );

        Assert.Equal(256, ProfileImageLoader.MaximumProfilePixelDimension);
        Assert.Equal(expectedWidth, image.PixelWidth);
        Assert.Equal(expectedHeight, image.PixelHeight);
        Assert.True(image.IsFrozen);
    }

    [Fact]
    public void Load_TransparentPngPreservesTransparentAndOpaquePixels()
    {
        using var imageFile = TemporaryImageFile.Create(
            "png",
            width: 512,
            height: 512,
            transparent: true
        );
        var image = Assert.IsAssignableFrom<BitmapSource>(
            ProfileImageLoader.Load(imageFile.Path)
        );
        var converted = new FormatConvertedBitmap(image, PixelFormats.Bgra32, null, 0);

        Assert.Equal(0, ReadAlpha(converted, x: 0, y: 0));
        Assert.Equal(255, ReadAlpha(converted, x: image.PixelWidth / 2, y: image.PixelHeight / 2));
    }

    [Fact]
    public void Load_OnLoadReleasesFileAndKeepsPixelsAvailable()
    {
        using var imageFile = TemporaryImageFile.Create("png", width: 800, height: 400);
        var image = Assert.IsAssignableFrom<BitmapSource>(
            ProfileImageLoader.Load(imageFile.Path)
        );

        using (
            File.Open(
                imageFile.Path,
                FileMode.Open,
                FileAccess.ReadWrite,
                FileShare.None
            )
        ) { }

        Assert.Equal(256, image.PixelWidth);
        Assert.Equal(128, image.PixelHeight);
        Assert.True(image.IsFrozen);
    }

    [Fact]
    public void Load_WithInvalidImageFile_ReturnsNull()
    {
        var imagePath = Path.Combine(
            Path.GetTempPath(),
            $"flourish-invalid-profile-{Guid.NewGuid():N}.png"
        );
        try
        {
            File.WriteAllText(imagePath, "not an image");

            Assert.Null(ProfileImageLoader.Load(imagePath));
        }
        finally
        {
            File.Delete(imagePath);
        }
    }

    [Fact]
    public void BrushCache_GetSameTrimmedPathReturnsFrozenBrushByReference()
    {
        using var imageFile = TemporaryImageFile.Create("png", width: 512, height: 512);
        var cache = new ProfileImageBrushCache();

        var first = cache.Get($"  {imageFile.Path}  ");
        var second = cache.Get(imageFile.Path);

        Assert.NotNull(first);
        Assert.Same(first, second);
        Assert.Equal(Stretch.UniformToFill, first.Stretch);
        Assert.True(first.IsFrozen);
    }

    [Fact]
    public void BrushCache_ChangingPathReplacesSingleCachedBrush()
    {
        using var firstFile = TemporaryImageFile.Create("png", width: 512, height: 512);
        using var secondFile = TemporaryImageFile.Create("jpg", width: 800, height: 400);
        var cache = new ProfileImageBrushCache();

        var first = cache.Get(firstFile.Path);
        var second = cache.Get(secondFile.Path);

        Assert.NotNull(first);
        Assert.NotNull(second);
        Assert.NotSame(first, second);
        Assert.Same(second, cache.Get(secondFile.Path));
        Assert.NotSame(first, cache.Get(firstFile.Path));
    }

    [Fact]
    public void BrushCache_SetExplicitlyRefreshesSamePathWithPreloadedSource()
    {
        using var cachedFile = TemporaryImageFile.Create("png", width: 512, height: 512);
        using var replacementFile = TemporaryImageFile.Create("jpg", width: 800, height: 400);
        var cache = new ProfileImageBrushCache();
        var initial = cache.Get(cachedFile.Path);
        var preloaded = Assert.IsAssignableFrom<ImageSource>(
            ProfileImageLoader.Load(replacementFile.Path)
        );

        var refreshed = cache.Set($" {cachedFile.Path} ", preloaded);

        Assert.NotNull(initial);
        Assert.NotNull(refreshed);
        Assert.NotSame(initial, refreshed);
        Assert.Same(refreshed, cache.Get(cachedFile.Path));
        Assert.Same(preloaded, refreshed.ImageSource);
        Assert.Equal(Stretch.UniformToFill, refreshed.Stretch);
        Assert.True(refreshed.IsFrozen);
    }

    [Fact]
    public void BrushCache_NullAndInvalidInputsFallBackToNull()
    {
        using var imageFile = TemporaryImageFile.Create("png", width: 512, height: 512);
        var cache = new ProfileImageBrushCache();

        Assert.Null(cache.Get(null));
        Assert.Null(cache.Get("   "));
        Assert.Null(cache.Get(Path.Combine(Path.GetTempPath(), $"missing-{Guid.NewGuid():N}.png")));
        Assert.Null(cache.Set(imageFile.Path, null));
        Assert.Null(cache.Get(imageFile.Path));
    }

    private static byte ReadAlpha(BitmapSource source, int x, int y)
    {
        var pixel = new byte[4];
        source.CopyPixels(new Int32Rect(x, y, 1, 1), pixel, stride: 4, offset: 0);
        return pixel[3];
    }

    private sealed class TemporaryImageFile : IDisposable
    {
        private TemporaryImageFile(string path)
        {
            Path = path;
        }

        public string Path { get; }

        public static TemporaryImageFile Create(
            string format,
            int width,
            int height,
            bool transparent = false
        )
        {
            var path = System.IO.Path.Combine(
                System.IO.Path.GetTempPath(),
                $"flourish-profile-{Guid.NewGuid():N}.{format}"
            );
            var stride = checked(width * 4);
            var pixels = new byte[checked(stride * height)];
            if (transparent)
            {
                FillOpaqueCenter(pixels, width, height, stride);
            }
            else
            {
                FillOpaque(pixels);
            }

            var source = BitmapSource.Create(
                width,
                height,
                96,
                96,
                PixelFormats.Bgra32,
                palette: null,
                pixels,
                stride
            );
            BitmapEncoder encoder = format switch
            {
                "png" => new PngBitmapEncoder(),
                "jpg" => new JpegBitmapEncoder { QualityLevel = 90 },
                _ => throw new ArgumentOutOfRangeException(nameof(format)),
            };
            encoder.Frames.Add(BitmapFrame.Create(source));
            using (var stream = File.Create(path))
            {
                encoder.Save(stream);
            }

            return new TemporaryImageFile(path);
        }

        public void Dispose()
        {
            File.Delete(Path);
        }

        private static void FillOpaque(byte[] pixels)
        {
            for (var offset = 0; offset < pixels.Length; offset += 4)
            {
                pixels[offset] = 0x40;
                pixels[offset + 1] = 0x80;
                pixels[offset + 2] = 0xC0;
                pixels[offset + 3] = 0xFF;
            }
        }

        private static void FillOpaqueCenter(byte[] pixels, int width, int height, int stride)
        {
            for (var y = height / 4; y < height * 3 / 4; y++)
            {
                for (var x = width / 4; x < width * 3 / 4; x++)
                {
                    var offset = (y * stride) + (x * 4);
                    pixels[offset] = 0x20;
                    pixels[offset + 1] = 0x60;
                    pixels[offset + 2] = 0xA0;
                    pixels[offset + 3] = 0xFF;
                }
            }
        }
    }
}
