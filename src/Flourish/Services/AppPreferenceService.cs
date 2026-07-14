using System.IO;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Threading.Channels;
using ArkheideSystem.Flourish.Abstract;
using ArkheideSystem.Flourish.Internal.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace ArkheideSystem.Flourish.Services;

internal sealed class AppPreferenceService : IAppSettingsStore, IHostedService, IDisposable
{
    private const string AppSettingsFileName = "appsettings.json";
    private const string ThemeConfigurationKey = "Flourish:Preferences:Theme";
    private static readonly TimeSpan ThemeWriteCoalescingDelay =
        TimeSpan.FromMilliseconds(50);
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = true,
    };
    private readonly IConfiguration configuration;
    private readonly FlourishAppSettingsConfigurationProvider? appSettingsProvider;
    private readonly IHostEnvironment hostEnvironment;
    private readonly ILogger<AppPreferenceService> logger;
    private readonly AsyncLocal<bool> isEditorActive = new();
    private readonly object lifecycleGate = new();
    private Channel<PreferenceWorkItem> workItems = CreateWorkItemChannel();
    private Task? workerTask;
    private Exception? lastThemePersistenceError;
    private bool isAcceptingWork = true;
    private bool isDisposed;

    public AppPreferenceService(
        IConfiguration configuration,
        IHostEnvironment hostEnvironment,
        ILogger<AppPreferenceService> logger
    )
    {
        this.configuration = configuration
            ?? throw new ArgumentNullException(nameof(configuration));
        this.hostEnvironment = hostEnvironment
            ?? throw new ArgumentNullException(nameof(hostEnvironment));
        this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
        appSettingsProvider = (configuration as IConfigurationRoot)
            ?.Providers.OfType<FlourishAppSettingsConfigurationProvider>()
            .FirstOrDefault();
    }

    internal AppPreferenceService(
        IConfiguration configuration,
        IHostEnvironment hostEnvironment
    ) : this(configuration, hostEnvironment, NullLogger<AppPreferenceService>.Instance) { }

    public string FilePath =>
        Path.Combine(hostEnvironment.ContentRootPath, AppSettingsFileName);

    public FlourishTheme? ReadTheme()
    {
        var value = configuration[ThemeConfigurationKey];
        if (
            string.IsNullOrWhiteSpace(value)
            || !Enum.TryParse(value, ignoreCase: true, out FlourishTheme theme)
            || !Enum.IsDefined(theme)
        )
        {
            return null;
        }

        return theme;
    }

    public void SaveTheme(FlourishTheme theme)
    {
        QueueThemeSave(theme, Task.FromResult(true));
    }

    internal void QueueThemeSave(FlourishTheme theme, Task<bool> runtimeApplied)
    {
        if (!Enum.IsDefined(theme))
        {
            throw new ArgumentOutOfRangeException(nameof(theme), theme, "Unknown theme.");
        }

        ArgumentNullException.ThrowIfNull(runtimeApplied);

        lock (lifecycleGate)
        {
            ThrowIfNotAcceptingWork();
            EnqueueCore(new ThemePreferenceWorkItem(theme, runtimeApplied));
        }
    }

    internal async ValueTask FlushThemeSavesAsync(
        CancellationToken cancellationToken = default
    )
    {
        var completion = new TaskCompletionSource<bool>(
            TaskCreationOptions.RunContinuationsAsynchronously
        );
        lock (lifecycleGate)
        {
            ThrowIfNotAcceptingWork();
            EnqueueCore(new FlushThemePreferenceWorkItem(completion));
        }

        await completion.Task.WaitAsync(cancellationToken).ConfigureAwait(false);
    }

    public ValueTask<AppSettingsUpdateResult> UpdateAsync(
        Action<IAppSettingsEditor> update,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(update);
        cancellationToken.ThrowIfCancellationRequested();
        if (isEditorActive.Value)
        {
            throw new InvalidOperationException(
                "An appsettings transaction cannot start another transaction."
            );
        }

        var completion = new TaskCompletionSource<AppSettingsUpdateResult>(
            TaskCreationOptions.RunContinuationsAsynchronously
        );
        lock (lifecycleGate)
        {
            ThrowIfNotAcceptingWork();
            EnqueueCore(
                new UpdatePreferenceWorkItem(update, cancellationToken, completion)
            );
        }

        return new ValueTask<AppSettingsUpdateResult>(completion.Task);
    }

    public ValueTask<AppSettingsUpdateResult> SetAsync<T>(
        string path,
        T value,
        CancellationToken cancellationToken = default
    )
    {
        return UpdateAsync(editor => editor.Set(path, value), cancellationToken);
    }

    public ValueTask<AppSettingsUpdateResult> RemoveAsync(
        string path,
        CancellationToken cancellationToken = default
    )
    {
        return UpdateAsync(editor => editor.Remove(path), cancellationToken);
    }

    public ValueTask<AppSettingsUpdateResult> MergeAsync<T>(
        string path,
        T value,
        CancellationToken cancellationToken = default
    )
    {
        return UpdateAsync(editor => editor.Merge(path, value), cancellationToken);
    }

    public ValueTask<AppSettingsUpdateResult> AppendAsync<T>(
        string path,
        T value,
        CancellationToken cancellationToken = default
    )
    {
        return UpdateAsync(editor => editor.Append(path, value), cancellationToken);
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        lock (lifecycleGate)
        {
            ObjectDisposedException.ThrowIf(isDisposed, this);
            if (isAcceptingWork)
            {
                return Task.CompletedTask;
            }

            if (workerTask is { IsCompleted: false })
            {
                throw new InvalidOperationException(
                    "The previous appsettings worker has not stopped yet."
                );
            }

            workItems = CreateWorkItemChannel();
            workerTask = null;
            lastThemePersistenceError = null;
            isAcceptingWork = true;
        }

        return Task.CompletedTask;
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        Task? worker;
        lock (lifecycleGate)
        {
            if (isAcceptingWork)
            {
                isAcceptingWork = false;
                workItems.Writer.TryComplete();
            }

            worker = workerTask;
        }

        if (worker is not null)
        {
            await worker.WaitAsync(cancellationToken).ConfigureAwait(false);
        }
    }

    public void Dispose()
    {
        lock (lifecycleGate)
        {
            if (isDisposed)
            {
                return;
            }

            isDisposed = true;
        }

        try
        {
            StopAsync(CancellationToken.None).GetAwaiter().GetResult();
        }
        catch (Exception error)
        {
            TryLogError(error, "Failed to flush appsettings updates during shutdown.");
        }
    }

    private void EnqueueCore(PreferenceWorkItem workItem)
    {
        workerTask ??= Task.Run(() => ProcessWorkItemsAsync(workItems.Reader));
        if (!workItems.Writer.TryWrite(workItem))
        {
            throw new InvalidOperationException("The appsettings update queue is closed.");
        }
    }

    private void ThrowIfNotAcceptingWork()
    {
        ObjectDisposedException.ThrowIf(isDisposed, this);
        if (!isAcceptingWork)
        {
            throw new InvalidOperationException(
                "The appsettings update queue is stopping and cannot accept new work."
            );
        }
    }

    private async Task ProcessWorkItemsAsync(ChannelReader<PreferenceWorkItem> reader)
    {
        await foreach (var workItem in reader.ReadAllAsync())
        {
            switch (workItem)
            {
                case UpdatePreferenceWorkItem update:
                    await ProcessUpdateAsync(update).ConfigureAwait(false);
                    break;
                case ThemePreferenceWorkItem theme:
                    await ProcessThemeSaveAsync(reader, theme)
                        .ConfigureAwait(false);
                    break;
                case FlushThemePreferenceWorkItem flush:
                    if (lastThemePersistenceError is { } error)
                    {
                        flush.Completion.TrySetException(error);
                    }
                    else
                    {
                        flush.Completion.TrySetResult(true);
                    }
                    break;
            }
        }
    }

    private List<ThemePreferenceWorkItem> CoalesceThemeWorkItems(
        ChannelReader<PreferenceWorkItem> reader,
        ThemePreferenceWorkItem current
    )
    {
        List<ThemePreferenceWorkItem> coalescedItems = [current];
        while (
            reader.TryPeek(out var next)
            && next is ThemePreferenceWorkItem
            && reader.TryRead(out var coalesced)
        )
        {
            coalescedItems.Add((ThemePreferenceWorkItem)coalesced);
        }

        return coalescedItems;
    }

    private async Task ProcessThemeSaveAsync(
        ChannelReader<PreferenceWorkItem> reader,
        ThemePreferenceWorkItem first
    )
    {
        await Task.Delay(ThemeWriteCoalescingDelay).ConfigureAwait(false);
        var coalesced = CoalesceThemeWorkItems(reader, first);
        ThemePreferenceWorkItem? latestApplied = null;
        foreach (var candidate in coalesced)
        {
            if (await candidate.RuntimeApplied.ConfigureAwait(false))
            {
                latestApplied = candidate;
            }
        }

        if (latestApplied is null)
        {
            return;
        }

        try
        {
            await ExecuteUpdateAsync(
                    editor =>
                        editor.Set(
                            ThemeConfigurationKey,
                            latestApplied.Theme.ToString()
                        ),
                    CancellationToken.None
                )
                .ConfigureAwait(false);
            lastThemePersistenceError = null;
        }
        catch (Exception error)
        {
            lastThemePersistenceError = error;
            TryLogError(
                error,
                "Failed to persist the Flourish theme preference {Theme}.",
                latestApplied.Theme
            );
        }
    }

    private async Task ProcessUpdateAsync(UpdatePreferenceWorkItem workItem)
    {
        if (workItem.CancellationToken.IsCancellationRequested)
        {
            workItem.Completion.TrySetCanceled(workItem.CancellationToken);
            return;
        }

        try
        {
            var result = await ExecuteUpdateAsync(
                    workItem.Update,
                    workItem.CancellationToken
                )
                .ConfigureAwait(false);
            workItem.Completion.TrySetResult(result);
        }
        catch (OperationCanceledException)
            when (workItem.CancellationToken.IsCancellationRequested)
        {
            workItem.Completion.TrySetCanceled(workItem.CancellationToken);
        }
        catch (Exception error)
        {
            workItem.Completion.TrySetException(error);
        }
    }

    private async ValueTask<AppSettingsUpdateResult> ExecuteUpdateAsync(
        Action<IAppSettingsEditor> update,
        CancellationToken cancellationToken
    )
    {
        var wasEditorActive = isEditorActive.Value;
        isEditorActive.Value = true;
        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            var root = await ReadAppSettingsAsync(cancellationToken).ConfigureAwait(false);
            var editor = new AppSettingsEditor(root);
            try
            {
                update(editor);
            }
            finally
            {
                editor.Complete();
            }

            if (!editor.IsChanged)
            {
                return new AppSettingsUpdateResult(FilePath, false, false);
            }

            cancellationToken.ThrowIfCancellationRequested();
            var content = await WriteAppSettingsAsync(root, cancellationToken)
                .ConfigureAwait(false);
            var reloaded = ReloadConfiguration(content);
            return new AppSettingsUpdateResult(FilePath, true, reloaded);
        }
        finally
        {
            isEditorActive.Value = wasEditorActive;
        }
    }

    private async ValueTask<JsonObject> ReadAppSettingsAsync(
        CancellationToken cancellationToken
    )
    {
        if (!File.Exists(FilePath))
        {
            return [];
        }

        try
        {
            await using var stream = new FileStream(
                FilePath,
                new FileStreamOptions
                {
                    Mode = FileMode.Open,
                    Access = FileAccess.Read,
                    Share = FileShare.ReadWrite | FileShare.Delete,
                    Options = FileOptions.Asynchronous | FileOptions.SequentialScan,
                }
            );
            var node = await JsonNode.ParseAsync(
                stream,
                documentOptions: new JsonDocumentOptions
                {
                    AllowTrailingCommas = true,
                    CommentHandling = JsonCommentHandling.Skip,
                },
                cancellationToken: cancellationToken
            ).ConfigureAwait(false);
            return node as JsonObject
                ?? throw new InvalidDataException(
                    $"{AppSettingsFileName} must contain a JSON object."
                );
        }
        catch (JsonException error)
        {
            throw new InvalidDataException(
                $"{AppSettingsFileName} contains invalid JSON and was not changed.",
                error
            );
        }
    }

    private async ValueTask<byte[]> WriteAppSettingsAsync(
        JsonObject root,
        CancellationToken cancellationToken
    )
    {
        var directory = Path.GetDirectoryName(FilePath)
            ?? throw new InvalidOperationException(
                $"{AppSettingsFileName} has no parent directory."
            );
        Directory.CreateDirectory(directory);
        var temporaryPath = Path.Combine(
            directory,
            $".{AppSettingsFileName}.{Guid.NewGuid():N}.tmp"
        );
        var content = Encoding.UTF8.GetBytes(root.ToJsonString(SerializerOptions));

        try
        {
            await using (var stream = new FileStream(
                temporaryPath,
                new FileStreamOptions
                {
                    Mode = FileMode.CreateNew,
                    Access = FileAccess.Write,
                    Share = FileShare.None,
                    Options = FileOptions.Asynchronous | FileOptions.SequentialScan,
                }
            ))
            {
                await stream.WriteAsync(content, cancellationToken).ConfigureAwait(false);
                await stream.FlushAsync(cancellationToken).ConfigureAwait(false);
            }

            cancellationToken.ThrowIfCancellationRequested();
            File.Move(temporaryPath, FilePath, overwrite: true);
            return content;
        }
        finally
        {
            if (File.Exists(temporaryPath))
            {
                File.Delete(temporaryPath);
            }
        }
    }

    private bool ReloadConfiguration(byte[] content)
    {
        if (appSettingsProvider is not null)
        {
            return appSettingsProvider.Apply(content);
        }

        if (configuration is not IConfigurationRoot configurationRoot)
        {
            return false;
        }

        configurationRoot.Reload();
        return true;
    }

    private void TryLogError(
        Exception error,
        string message,
        params object?[] arguments
    )
    {
        try
        {
            logger.LogError(error, message, arguments);
        }
        catch (Exception)
        {
            // Logging must never stop the single settings writer.
        }
    }

    private static Channel<PreferenceWorkItem> CreateWorkItemChannel()
    {
        return Channel.CreateUnbounded<PreferenceWorkItem>(
            new UnboundedChannelOptions
            {
                AllowSynchronousContinuations = false,
                SingleReader = true,
                SingleWriter = false,
            }
        );
    }

    private abstract record PreferenceWorkItem;

    private sealed record UpdatePreferenceWorkItem(
        Action<IAppSettingsEditor> Update,
        CancellationToken CancellationToken,
        TaskCompletionSource<AppSettingsUpdateResult> Completion
    ) : PreferenceWorkItem;

    private sealed record ThemePreferenceWorkItem(
        FlourishTheme Theme,
        Task<bool> RuntimeApplied
    )
        : PreferenceWorkItem;

    private sealed record FlushThemePreferenceWorkItem(
        TaskCompletionSource<bool> Completion
    ) : PreferenceWorkItem;

    private sealed class AppSettingsEditor(JsonObject root) : IAppSettingsEditor
    {
        private bool isActive = true;

        public bool IsChanged { get; private set; }

        public void Set<T>(string path, T value)
        {
            EnsureActive();
            var segments = ParsePath(path);
            var parent = GetOrCreateParent(root, segments);
            var propertyName = segments[^1];
            var existingName = FindPropertyName(parent, propertyName);
            var node = Serialize(value);
            if (
                existingName is not null
                && JsonNode.DeepEquals(parent[existingName], node)
            )
            {
                return;
            }

            parent[existingName ?? propertyName] = node;
            IsChanged = true;
        }

        public bool Remove(string path)
        {
            EnsureActive();
            var segments = ParsePath(path);
            if (!TryGetParent(root, segments, out var parent))
            {
                return false;
            }

            var existingName = FindPropertyName(parent, segments[^1]);
            if (existingName is null || !parent.Remove(existingName))
            {
                return false;
            }

            IsChanged = true;
            return true;
        }

        public void Merge<T>(string path, T value)
        {
            EnsureActive();
            var incoming = Serialize(value) as JsonObject
                ?? throw new ArgumentException(
                    "A merged appsettings value must serialize to a JSON object.",
                    nameof(value)
                );
            var segments = ParsePath(path);
            var parent = GetOrCreateParent(root, segments);
            var propertyName = segments[^1];
            var existingName = FindPropertyName(parent, propertyName);
            JsonObject target;
            if (existingName is null)
            {
                target = [];
                parent[propertyName] = target;
                IsChanged = true;
            }
            else
            {
                target = parent[existingName] as JsonObject
                    ?? throw new InvalidDataException(
                        $"The {AppSettingsFileName} value '{path}' must be a JSON object."
                    );
            }

            IsChanged |= MergeObjects(target, incoming);
        }

        public void Append<T>(string path, T value)
        {
            EnsureActive();
            var segments = ParsePath(path);
            var parent = GetOrCreateParent(root, segments);
            var propertyName = segments[^1];
            var existingName = FindPropertyName(parent, propertyName);
            JsonArray target;
            if (existingName is null)
            {
                target = [];
                parent[propertyName] = target;
            }
            else
            {
                target = parent[existingName] as JsonArray
                    ?? throw new InvalidDataException(
                        $"The {AppSettingsFileName} value '{path}' must be a JSON array."
                    );
            }

            target.Add(Serialize(value));
            IsChanged = true;
        }

        public void Complete()
        {
            isActive = false;
        }

        private void EnsureActive()
        {
            ObjectDisposedException.ThrowIf(!isActive, this);
        }

        private static JsonNode? Serialize<T>(T value)
        {
            return JsonSerializer.SerializeToNode(value, SerializerOptions);
        }

        private static string[] ParsePath(string path)
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(path);
            var segments = path.Split(':', StringSplitOptions.TrimEntries);
            if (segments.Any(string.IsNullOrWhiteSpace))
            {
                throw new ArgumentException(
                    "An appsettings path cannot contain an empty segment.",
                    nameof(path)
                );
            }

            return segments;
        }

        private static JsonObject GetOrCreateParent(JsonObject root, string[] segments)
        {
            var current = root;
            for (var index = 0; index < segments.Length - 1; index++)
            {
                var segment = segments[index];
                var existingName = FindPropertyName(current, segment);
                if (existingName is null)
                {
                    var created = new JsonObject();
                    current[segment] = created;
                    current = created;
                    continue;
                }

                current = current[existingName] as JsonObject
                    ?? throw new InvalidDataException(
                        $"The {AppSettingsFileName} value '{string.Join(':', segments[..(index + 1)])}' must be a JSON object."
                    );
            }

            return current;
        }

        private static bool TryGetParent(
            JsonObject root,
            string[] segments,
            out JsonObject parent
        )
        {
            parent = root;
            for (var index = 0; index < segments.Length - 1; index++)
            {
                var existingName = FindPropertyName(parent, segments[index]);
                if (existingName is null || parent[existingName] is not JsonObject child)
                {
                    return false;
                }

                parent = child;
            }

            return true;
        }

        private static bool MergeObjects(JsonObject target, JsonObject incoming)
        {
            var changed = false;
            foreach (var (name, incomingValue) in incoming)
            {
                var existingName = FindPropertyName(target, name);
                if (
                    incomingValue is JsonObject incomingObject
                    && existingName is not null
                    && target[existingName] is JsonObject targetObject
                )
                {
                    changed |= MergeObjects(targetObject, incomingObject);
                    continue;
                }

                if (
                    existingName is not null
                    && JsonNode.DeepEquals(target[existingName], incomingValue)
                )
                {
                    continue;
                }

                target[existingName ?? name] = incomingValue?.DeepClone();
                changed = true;
            }

            return changed;
        }

        private static string? FindPropertyName(JsonObject parent, string propertyName)
        {
            return parent
                .Select(property => property.Key)
                .FirstOrDefault(existingName =>
                    string.Equals(
                        existingName,
                        propertyName,
                        StringComparison.OrdinalIgnoreCase
                    )
                );
        }
    }
}
