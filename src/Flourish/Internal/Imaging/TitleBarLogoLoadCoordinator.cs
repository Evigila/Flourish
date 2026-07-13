using System.IO;
using System.Windows.Media;

namespace ArkheideSystem.Flourish.Internal.Imaging;

internal sealed class TitleBarLogoLoadCoordinator(
    Func<string, CancellationToken, Task<ImageSource?>> loadAsync
) : IDisposable
{
    private readonly object gate = new();
    private readonly Func<string, CancellationToken, Task<ImageSource?>> loadAsync =
        loadAsync ?? throw new ArgumentNullException(nameof(loadAsync));
    private string? currentPath;
    private ImageSource? cachedSource;
    private bool hasCachedResult;
    private long generation;
    private Task<TitleBarLogoLoadResult>? pendingTask;
    private CancellationTokenSource? pendingCancellation;
    private bool disposed;

    public TitleBarLogoLoadRequest Request(string? logoPath)
    {
        var normalizedPath = NormalizePath(logoPath);
        CancellationTokenSource? previousCancellation = null;
        TitleBarLogoLoadRequest request;

        lock (gate)
        {
            ObjectDisposedException.ThrowIf(disposed, this);

            if (string.Equals(currentPath, normalizedPath, StringComparison.Ordinal))
            {
                if (pendingTask is not null)
                {
                    return new TitleBarLogoLoadRequest(
                        normalizedPath,
                        generation,
                        IsNewRequest: false,
                        pendingTask
                    );
                }

                if (hasCachedResult)
                {
                    return CreateCachedRequest(normalizedPath);
                }
            }

            previousCancellation = pendingCancellation;
            pendingCancellation = null;
            pendingTask = null;
            currentPath = normalizedPath;
            cachedSource = null;
            hasCachedResult = false;
            generation++;

            if (normalizedPath is null)
            {
                hasCachedResult = true;
                request = CreateCachedRequest(normalizedPath, isNewRequest: true);
            }
            else
            {
                var cancellation = new CancellationTokenSource();
                var completion = new TaskCompletionSource<TitleBarLogoLoadResult>(
                    TaskCreationOptions.RunContinuationsAsynchronously
                );
                pendingCancellation = cancellation;
                pendingTask = completion.Task;
                request = new TitleBarLogoLoadRequest(
                    normalizedPath,
                    generation,
                    IsNewRequest: true,
                    completion.Task
                );
                _ = LoadAndCompleteAsync(
                    normalizedPath,
                    generation,
                    cancellation,
                    completion
                );
            }
        }

        CancelAndDispose(previousCancellation);
        return request;
    }

    public bool IsCurrent(TitleBarLogoLoadResult result)
    {
        lock (gate)
        {
            return !disposed
                && result.IsCurrent
                && result.Generation == generation
                && string.Equals(result.Path, currentPath, StringComparison.Ordinal);
        }
    }

    public void Dispose()
    {
        CancellationTokenSource? cancellation;
        lock (gate)
        {
            if (disposed)
            {
                return;
            }

            disposed = true;
            generation++;
            cancellation = pendingCancellation;
            pendingCancellation = null;
            pendingTask = null;
            cachedSource = null;
            hasCachedResult = false;
        }

        CancelAndDispose(cancellation);
    }

    internal static string? NormalizePath(string? logoPath)
    {
        if (string.IsNullOrWhiteSpace(logoPath))
        {
            return null;
        }

        var trimmedPath = logoPath.Trim();
        try
        {
            if (Uri.TryCreate(trimmedPath, UriKind.Absolute, out var absoluteUri))
            {
                return absoluteUri.AbsoluteUri;
            }

            if (Path.IsPathFullyQualified(trimmedPath))
            {
                return Path.GetFullPath(trimmedPath);
            }
        }
        catch (Exception error)
            when (error is ArgumentException
                or NotSupportedException
                or PathTooLongException
                or UriFormatException)
        {
            // The image loader will treat the original value as an invalid path and cache
            // the fallback result. Normalization itself must never block a title-bar update.
        }

        return trimmedPath;
    }

    private TitleBarLogoLoadRequest CreateCachedRequest(
        string? normalizedPath,
        bool isNewRequest = false
    )
    {
        return new TitleBarLogoLoadRequest(
            normalizedPath,
            generation,
            isNewRequest,
            Task.FromResult(
                new TitleBarLogoLoadResult(
                    normalizedPath,
                    cachedSource,
                    generation,
                    IsCurrent: true
                )
            )
        );
    }

    private async Task LoadAndCompleteAsync(
        string normalizedPath,
        long requestGeneration,
        CancellationTokenSource cancellation,
        TaskCompletionSource<TitleBarLogoLoadResult> completion
    )
    {
        ImageSource? source = null;
        Exception? failure = null;
        try
        {
            source = await loadAsync(normalizedPath, cancellation.Token).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (cancellation.IsCancellationRequested)
        {
            // A newer path or shell shutdown superseded this request.
        }
        catch (Exception error)
        {
            failure = error;
        }

        var isCurrent = false;
        var ownsCancellation = false;
        lock (gate)
        {
            isCurrent =
                !disposed
                && requestGeneration == generation
                && string.Equals(normalizedPath, currentPath, StringComparison.Ordinal);
            if (isCurrent)
            {
                ownsCancellation = ReferenceEquals(pendingCancellation, cancellation);
                pendingCancellation = null;
                pendingTask = null;
                if (failure is null)
                {
                    cachedSource = source;
                    hasCachedResult = true;
                }
            }
        }

        if (ownsCancellation)
        {
            cancellation.Dispose();
        }

        if (failure is not null)
        {
            completion.TrySetException(failure);
            return;
        }

        completion.TrySetResult(
            new TitleBarLogoLoadResult(
                normalizedPath,
                source,
                requestGeneration,
                isCurrent
            )
        );
    }

    private static void CancelAndDispose(CancellationTokenSource? cancellation)
    {
        if (cancellation is null)
        {
            return;
        }

        try
        {
            cancellation.Cancel();
        }
        finally
        {
            cancellation.Dispose();
        }
    }
}

internal readonly record struct TitleBarLogoLoadRequest(
    string? Path,
    long Generation,
    bool IsNewRequest,
    Task<TitleBarLogoLoadResult> Completion
);

internal readonly record struct TitleBarLogoLoadResult(
    string? Path,
    ImageSource? Source,
    long Generation,
    bool IsCurrent
);
