using System.Windows.Media;

namespace ArkheideSystem.Flourish.Internal.Imaging;

internal sealed class ProfileImageBrushCache
{
    private string? imagePath;
    private ImageBrush? brush;
    private bool isInitialized;

    internal ImageBrush? Get(string? imagePath)
    {
        var normalizedPath = NormalizePath(imagePath);
        if (
            !isInitialized
            || !string.Equals(this.imagePath, normalizedPath, StringComparison.Ordinal)
        )
        {
            SetCore(normalizedPath, ProfileImageLoader.Load(normalizedPath));
        }

        return brush;
    }

    internal ImageBrush? Set(string? imagePath, ImageSource? imageSource)
    {
        SetCore(NormalizePath(imagePath), imageSource);
        return brush;
    }

    private void SetCore(string? normalizedPath, ImageSource? imageSource)
    {
        imagePath = normalizedPath;
        brush = ProfileImageLoader.CreateBrush(imageSource);
        isInitialized = true;
    }

    private static string? NormalizePath(string? imagePath)
    {
        return string.IsNullOrWhiteSpace(imagePath) ? null : imagePath.Trim();
    }
}
