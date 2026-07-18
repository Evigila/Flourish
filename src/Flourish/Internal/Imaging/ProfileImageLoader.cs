using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ArkheideSystem.Flourish.Internal.Imaging;

internal static class ProfileImageLoader
{
    internal const int MaximumProfilePixelDimension = 256;

    public static ImageSource? Load(string? imagePath)
    {
        if (string.IsNullOrWhiteSpace(imagePath))
        {
            return null;
        }

        try
        {
            var uri = CreateUri(imagePath.Trim());
            var pixelSize = ReadPixelSize(uri);
            var image = new BitmapImage();
            image.BeginInit();
            image.CacheOption = BitmapCacheOption.OnLoad;
            image.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
            image.UriSource = uri;
            ApplyDecodeSize(image, pixelSize);
            image.EndInit();
            if (image.CanFreeze)
            {
                image.Freeze();
            }

            return image;
        }
        catch (Exception error)
            when (error is ArgumentException
                or IOException
                or FormatException
                or InvalidOperationException
                or NotSupportedException
                or OverflowException
                or UnauthorizedAccessException
                or UriFormatException)
        {
            return null;
        }
    }

    internal static ImageBrush? CreateBrush(ImageSource? imageSource)
    {
        if (imageSource is null)
        {
            return null;
        }

        var brush = new ImageBrush(imageSource) { Stretch = Stretch.UniformToFill };
        if (brush.CanFreeze)
        {
            brush.Freeze();
        }

        return brush;
    }

    private static PixelSize ReadPixelSize(Uri uri)
    {
        if (uri.IsFile)
        {
            using var stream = File.OpenRead(uri.LocalPath);
            return ReadPixelSize(stream);
        }

        var decoder = BitmapDecoder.Create(
            uri,
            BitmapCreateOptions.DelayCreation | BitmapCreateOptions.PreservePixelFormat,
            BitmapCacheOption.None
        );
        var frame = decoder.Frames[0];
        return new PixelSize(frame.PixelWidth, frame.PixelHeight);
    }

    private static PixelSize ReadPixelSize(Stream stream)
    {
        var decoder = BitmapDecoder.Create(
            stream,
            BitmapCreateOptions.DelayCreation | BitmapCreateOptions.PreservePixelFormat,
            BitmapCacheOption.None
        );
        var frame = decoder.Frames[0];
        return new PixelSize(frame.PixelWidth, frame.PixelHeight);
    }

    private static void ApplyDecodeSize(BitmapImage image, PixelSize pixelSize)
    {
        if (
            pixelSize.Width <= MaximumProfilePixelDimension
            && pixelSize.Height <= MaximumProfilePixelDimension
        )
        {
            return;
        }

        if (pixelSize.Width >= pixelSize.Height)
        {
            image.DecodePixelWidth = MaximumProfilePixelDimension;
        }
        else
        {
            image.DecodePixelHeight = MaximumProfilePixelDimension;
        }
    }

    private static Uri CreateUri(string imagePath)
    {
        if (Uri.TryCreate(imagePath, UriKind.Absolute, out var absoluteUri))
        {
            return absoluteUri;
        }

        return new Uri(Path.GetFullPath(imagePath, AppContext.BaseDirectory));
    }

    private readonly record struct PixelSize(int Width, int Height);
}
