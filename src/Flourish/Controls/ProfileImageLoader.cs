using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ArkheideSystem.Flourish.Controls;

internal static class ProfileImageLoader
{
    public static ImageSource? Load(string? imagePath)
    {
        if (string.IsNullOrWhiteSpace(imagePath))
        {
            return null;
        }

        try
        {
            var uri = CreateUri(imagePath.Trim());
            var image = new BitmapImage();
            image.BeginInit();
            image.CacheOption = BitmapCacheOption.OnLoad;
            image.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
            image.UriSource = uri;
            image.EndInit();
            if (image.CanFreeze)
            {
                image.Freeze();
            }

            return image;
        }
        catch (Exception error)
            when (error is IOException
                or InvalidOperationException
                or NotSupportedException
                or UnauthorizedAccessException
                or UriFormatException)
        {
            return null;
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
}
