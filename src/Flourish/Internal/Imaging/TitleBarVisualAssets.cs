using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ArkheideSystem.Flourish.Internal.Imaging;

internal static class TitleBarVisualAssets
{
    internal const int LogoDecodePixelWidth = 256;
    internal const int MaximumLogoPixelDimension = 4096;
    internal const long MaximumLogoPixelCount = 1_048_576;
    internal const int MaximumLogoScanBufferBytes = 4 * 1_048_576;

    private const int BytesPerPixel = 4;
    private const string DefaultIconUri =
        "pack://application:,,,/Flourish;component/Assets/favicon.ico";
    private const string SunIconData =
        "M12,2 L12,4 M12,20 L12,22 M4.93,4.93 L6.34,6.34 "
        + "M17.66,17.66 L19.07,19.07 M2,12 L4,12 M20,12 L22,12 "
        + "M4.93,19.07 L6.34,17.66 M17.66,6.34 L19.07,4.93 "
        + "M8,12 C8,9.79 9.79,8 12,8 C14.21,8 16,9.79 16,12 "
        + "C16,14.21 14.21,16 12,16 C9.79,16 8,14.21 8,12 Z";
    private const string MoonIconData =
        "M21,12.79 C20.17,13.07 19.29,13.22 18.38,13.22 "
        + "C13.35,13.22 9.28,9.14 9.28,4.11 C9.28,3.2 9.42,2.32 9.7,1.49 "
        + "C5.78,2.58 2.9,6.18 2.9,10.45 C2.9,15.42 6.93,19.45 11.9,19.45 "
        + "C16.17,19.45 19.77,16.57 20.86,12.66 C20.91,12.7 20.96,12.75 21,12.79 Z";

    internal static ImageSource? DefaultLogoSource { get; } = LoadLogo(DefaultIconUri);

    internal static Geometry SunIconGeometry { get; } = CreateFrozenGeometry(SunIconData);

    internal static Geometry MoonIconGeometry { get; } = CreateFrozenGeometry(MoonIconData);

    internal static ImageSource? LoadLogo(string? logoPath)
    {
        if (string.IsNullOrWhiteSpace(logoPath))
        {
            return null;
        }

        try
        {
            var image = new BitmapImage();
            image.BeginInit();
            image.UriSource = new Uri(logoPath, UriKind.RelativeOrAbsolute);
            image.CacheOption = BitmapCacheOption.OnLoad;
            image.DecodePixelWidth = LogoDecodePixelWidth;
            image.EndInit();
            ValidatePixelBudget(image);
            return TrimTransparentPixels(image);
        }
        catch (Exception error)
            when (error is IOException
                or ArgumentException
                or FormatException
                or InvalidOperationException
                or NotSupportedException
                or OverflowException
                or UnauthorizedAccessException)
        {
            return null;
        }
    }

    internal static Task<ImageSource?> LoadLogoAsync(
        string logoPath,
        CancellationToken cancellationToken
    )
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(logoPath);
        return Task.Run<ImageSource?>(
            () =>
            {
                cancellationToken.ThrowIfCancellationRequested();
                var source = LoadLogo(logoPath);
                cancellationToken.ThrowIfCancellationRequested();
                return source;
            },
            cancellationToken
        );
    }

    internal static ImageSource TrimTransparentPixels(BitmapSource source)
    {
        ArgumentNullException.ThrowIfNull(source);

        ValidatePixelBudget(source);
        var bitmap = new FormatConvertedBitmap(source, PixelFormats.Bgra32, null, 0);
        var width = bitmap.PixelWidth;
        var height = bitmap.PixelHeight;
        var stride = checked(width * BytesPerPixel);
        var bufferLength = checked(stride * height);
        if (bufferLength > MaximumLogoScanBufferBytes)
        {
            throw new InvalidDataException(
                $"The title-bar logo needs a {bufferLength}-byte scan buffer, which exceeds the {MaximumLogoScanBufferBytes}-byte limit."
            );
        }

        var pixels = new byte[bufferLength];
        bitmap.CopyPixels(pixels, stride, 0);

        var left = width;
        var top = height;
        var right = -1;
        var bottom = -1;

        for (var y = 0; y < height; y++)
        {
            var rowOffset = y * stride;
            for (var x = 0; x < width; x++)
            {
                var alpha = pixels[rowOffset + x * BytesPerPixel + 3];
                if (alpha == 0)
                {
                    continue;
                }

                left = Math.Min(left, x);
                top = Math.Min(top, y);
                right = Math.Max(right, x);
                bottom = Math.Max(bottom, y);
            }
        }

        if (right < left || bottom < top)
        {
            return Freeze(source);
        }

        return Freeze(
            new CroppedBitmap(
                bitmap,
                new Int32Rect(left, top, right - left + 1, bottom - top + 1)
            )
        );
    }

    private static Geometry CreateFrozenGeometry(string pathData)
    {
        return Freeze(Geometry.Parse(pathData));
    }

    private static void ValidatePixelBudget(BitmapSource source)
    {
        var width = source.PixelWidth;
        var height = source.PixelHeight;
        var pixelCount = (long)width * height;
        if (
            width <= 0
            || height <= 0
            || width > MaximumLogoPixelDimension
            || height > MaximumLogoPixelDimension
            || pixelCount > MaximumLogoPixelCount
        )
        {
            throw new InvalidDataException(
                $"The decoded title-bar logo size {width}x{height} exceeds the supported pixel budget."
            );
        }
    }

    private static T Freeze<T>(T value)
        where T : Freezable
    {
        if (value.CanFreeze)
        {
            value.Freeze();
        }

        return value;
    }
}
