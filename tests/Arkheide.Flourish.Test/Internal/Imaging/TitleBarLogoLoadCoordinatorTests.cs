using System.IO;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using ArkheideSystem.Flourish.Internal.Imaging;

namespace ArkheideSystem.Flourish.Test.Internal.Imaging;

public sealed class TitleBarLogoLoadCoordinatorTests
{
    [Fact]
    public async Task SamePath_ReusesTheCompletedNegativeResult()
    {
        var loadCount = 0;
        using var sut = new TitleBarLogoLoadCoordinator(
            (path, cancellationToken) =>
            {
                loadCount++;
                return Task.FromResult<ImageSource?>(null);
            }
        );

        var first = sut.Request("Assets/logo.png");
        var firstResult = await first.Completion;
        var second = sut.Request(" Assets/logo.png ");
        var secondResult = await second.Completion;

        Assert.True(first.IsNewRequest);
        Assert.False(second.IsNewRequest);
        Assert.Equal(1, loadCount);
        Assert.True(sut.IsCurrent(firstResult));
        Assert.True(sut.IsCurrent(secondResult));
    }

    [Fact]
    public async Task NewPath_CancelsAndRejectsAnOlderCompletion()
    {
        var completions = new Dictionary<string, TaskCompletionSource<ImageSource?>>(
            StringComparer.Ordinal
        );
        var cancellationTokens = new Dictionary<string, CancellationToken>(
            StringComparer.Ordinal
        );
        using var sut = new TitleBarLogoLoadCoordinator(
            (path, cancellationToken) =>
            {
                cancellationTokens[path] = cancellationToken;
                var completion = new TaskCompletionSource<ImageSource?>(
                    TaskCreationOptions.RunContinuationsAsynchronously
                );
                completions[path] = completion;
                return completion.Task;
            }
        );

        var first = sut.Request("A.png");
        var second = sut.Request("B.png");
        Assert.True(cancellationTokens[first.Path!].IsCancellationRequested);

        completions[second.Path!].SetResult(null);
        var secondResult = await second.Completion;
        completions[first.Path!].SetResult(null);
        var firstResult = await first.Completion;

        Assert.True(secondResult.IsCurrent);
        Assert.True(sut.IsCurrent(secondResult));
        Assert.False(firstResult.IsCurrent);
        Assert.False(sut.IsCurrent(firstResult));
    }

    [Fact]
    public void TransparentPixelScan_RejectsImagesOutsideTheDimensionBudget()
    {
        var width = TitleBarVisualAssets.MaximumLogoPixelDimension + 1;
        const int height = 1;
        var stride = checked(width * 4);
        var source = BitmapSource.Create(
            width,
            height,
            96,
            96,
            PixelFormats.Bgra32,
            null,
            new byte[stride * height],
            stride
        );

        Assert.Throws<InvalidDataException>(() =>
            TitleBarVisualAssets.TrimTransparentPixels(source)
        );
    }
}
