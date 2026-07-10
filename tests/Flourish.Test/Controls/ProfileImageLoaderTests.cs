using System.IO;
using ArkheideSystem.Flourish.Controls;

namespace ArkheideSystem.Flourish.Test.Controls;

public sealed class ProfileImageLoaderTests
{
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
}
