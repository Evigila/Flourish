using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using Microsoft.Extensions.Primitives;

namespace ArkheideSystem.Flourish.Internal.Configuration;

internal sealed class FlourishAppSettingsConfigurationSource : JsonConfigurationSource
{
    public bool WatchForChanges { get; init; }

    public override IConfigurationProvider Build(IConfigurationBuilder builder)
    {
        EnsureDefaults(builder);
        return new FlourishAppSettingsConfigurationProvider(this);
    }
}

internal sealed class FlourishAppSettingsConfigurationProvider
    : JsonConfigurationProvider,
        IDisposable
{
    private readonly CancellationTokenSource disposal = new();
    private readonly object reloadGate = new();
    private readonly FlourishAppSettingsConfigurationSource source;
    private readonly IDisposable? watchRegistration;
    private byte[]? lastContentHash;
    private int reloadGeneration;
    private bool isDisposed;

    public FlourishAppSettingsConfigurationProvider(
        FlourishAppSettingsConfigurationSource source
    ) : base(source)
    {
        this.source = source;
        if (
            source.WatchForChanges
            && source.FileProvider is not null
            && !string.IsNullOrWhiteSpace(source.Path)
        )
        {
            watchRegistration = ChangeToken.OnChange(
                () => source.FileProvider.Watch(source.Path),
                QueueReload
            );
        }
    }

    public override void Load()
    {
        lock (reloadGate)
        {
            base.Load();
            lastContentHash = null;
        }
    }

    internal bool Apply(byte[] utf8Json)
    {
        ArgumentNullException.ThrowIfNull(utf8Json);
        var expectedHash = ComputeHash(utf8Json);
        lock (reloadGate)
        {
            if (isDisposed)
            {
                return false;
            }

            var currentContent = ReadCurrentFileContent();
            if (currentContent is null)
            {
                ReloadMissingFileCore();
                return true;
            }

            var currentHash = ComputeHash(currentContent);
            if (!ContentHashesMatch(expectedHash, currentHash))
            {
                utf8Json = currentContent;
                expectedHash = currentHash;
            }

            ApplyCore(utf8Json, expectedHash);
            return true;
        }
    }

    private bool ApplyCore(byte[] utf8Json, byte[] contentHash)
    {
        lock (reloadGate)
        {
            if (isDisposed)
            {
                return false;
            }

            if (ContentHashesMatch(lastContentHash, contentHash))
            {
                return false;
            }

            var previous = new Dictionary<string, string?>(
                Data,
                StringComparer.OrdinalIgnoreCase
            );
            using var stream = new MemoryStream(utf8Json, writable: false);
            base.Load(stream);
            lastContentHash = contentHash;
            var changed = !ConfigurationDataMatches(previous, Data);
            if (changed)
            {
                OnReload();
            }

            return changed;
        }
    }

    private void QueueReload()
    {
        var generation = Interlocked.Increment(ref reloadGeneration);
        _ = ReloadAfterDelayAsync(generation);
    }

    private async Task ReloadAfterDelayAsync(int generation)
    {
        try
        {
            await Task.Delay(source.ReloadDelay, disposal.Token).ConfigureAwait(false);
            ReloadCurrentFile(generation);
        }
        catch (OperationCanceledException) when (disposal.IsCancellationRequested)
        {
        }
        catch (Exception error)
        {
            if (!HandleReloadException(error))
            {
                throw;
            }
        }
    }

    private void ReloadCurrentFile(int generation)
    {
        lock (reloadGate)
        {
            if (
                isDisposed
                || generation != Volatile.Read(ref reloadGeneration)
            )
            {
                return;
            }

            // Read beneath the same gate used by the explicit writer Apply path.
            // Otherwise an old watcher buffer can overwrite a newer internal write.
            var content = ReadCurrentFileContent();
            if (generation != Volatile.Read(ref reloadGeneration))
            {
                return;
            }

            if (content is null)
            {
                ReloadMissingFileCore();
                return;
            }

            ApplyCore(content, ComputeHash(content));
        }
    }

    private void ReloadMissingFileCore()
    {
        var changed = Data.Count > 0;
        Data = new Dictionary<string, string?>(StringComparer.OrdinalIgnoreCase);
        lastContentHash = null;
        if (changed)
        {
            OnReload();
        }
    }

    private byte[]? ReadCurrentFileContent()
    {
        var fileInfo = source.FileProvider!.GetFileInfo(source.Path!);
        if (!fileInfo.Exists)
        {
            return null;
        }

        using var stream = fileInfo.CreateReadStream();
        using var buffer = new MemoryStream();
        stream.CopyTo(buffer);
        return buffer.ToArray();
    }

    private bool HandleReloadException(Exception error)
    {
        if (source.OnLoadException is null)
        {
            return false;
        }

        var context = new FileLoadExceptionContext
        {
            Provider = this,
            Exception = error,
        };
        source.OnLoadException(context);
        return context.Ignore;
    }

    private static byte[] ComputeHash(byte[] content)
    {
        return System.Security.Cryptography.SHA256.HashData(content);
    }

    private static bool ContentHashesMatch(byte[]? left, byte[] right)
    {
        return left is not null
            && System.Security.Cryptography.CryptographicOperations.FixedTimeEquals(
                left,
                right
            );
    }

    private static bool ConfigurationDataMatches(
        IDictionary<string, string?> left,
        IDictionary<string, string?> right
    )
    {
        if (left.Count != right.Count)
        {
            return false;
        }

        foreach (var (key, value) in left)
        {
            if (
                !right.TryGetValue(key, out var other)
                || !string.Equals(value, other, StringComparison.Ordinal)
            )
            {
                return false;
            }
        }

        return true;
    }

    void IDisposable.Dispose()
    {
        lock (reloadGate)
        {
            if (isDisposed)
            {
                return;
            }

            isDisposed = true;
        }

        disposal.Cancel();
        watchRegistration?.Dispose();
        disposal.Dispose();
        base.Dispose();
    }
}
