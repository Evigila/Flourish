using ArkheideSystem.Flourish.Abstract;
using ArkheideSystem.Flourish.Internal.Configuration;

namespace ArkheideSystem.Flourish.Services;

internal sealed class FlourishStatusService : IStatusBarService
{
    private readonly Lock gate = new();
    private readonly FlourishShellOptions options;
    private readonly Dictionary<string, CancellationTokenSource> expirations = new(
        StringComparer.Ordinal
    );
    private readonly Dictionary<string, Guid> leases = new(StringComparer.Ordinal);
    private long version;

    public FlourishStatusService(FlourishShellOptions options)
    {
        this.options = options ?? throw new ArgumentNullException(nameof(options));
        NormalizeSeedIds(options.StatusItems);
    }

    public event EventHandler<FlourishStatusBarChangedEventArgs>? Changed;

    public FlourishStatusBarSnapshot Current
    {
        get
        {
            lock (gate)
            {
                return CreateSnapshot();
            }
        }
    }

    public void SetEnabled(bool enabled)
    {
        Mutate(
            () =>
                SetIfChanged(
                    options.IsStatusBarEnabled,
                    enabled,
                    value => options.IsStatusBarEnabled = value
                ),
            FlourishRuntimeChangeKind.Updated,
            itemId: null
        );
    }

    public void SetLanStatusEnabled(bool enabled)
    {
        Mutate(
            () =>
                SetIfChanged(
                    options.IsLANConnectionStatusEnabled,
                    enabled,
                    value => options.IsLANConnectionStatusEnabled = value
                ),
            FlourishRuntimeChangeKind.Updated,
            itemId: null
        );
    }

    public void SetPowerStatusEnabled(bool enabled)
    {
        Mutate(
            () =>
                SetIfChanged(
                    options.IsPowerStatusEnabled,
                    enabled,
                    value => options.IsPowerStatusEnabled = value
                ),
            FlourishRuntimeChangeKind.Updated,
            itemId: null
        );
    }

    public void Add(FlourishStatusItem item, int? index = null)
    {
        ValidateItem(item);
        Mutate(
            () =>
            {
                if (FindIndex(item.Id) >= 0)
                {
                    throw new InvalidOperationException(
                        $"Status item ID '{item.Id}' is already registered."
                    );
                }

                CancelExpiration(item.Id);
                leases.Remove(item.Id);
                Insert(item, index);
                return true;
            },
            FlourishRuntimeChangeKind.Added,
            item.Id
        );
    }

    public void Upsert(FlourishStatusItem item, int? index = null)
    {
        ValidateItem(item);
        Mutate(
            () =>
            {
                var oldIndex = FindIndex(item.Id);
                if (oldIndex >= 0)
                {
                    options.StatusItems.RemoveAt(oldIndex);
                }

                CancelExpiration(item.Id);
                leases.Remove(item.Id);
                Insert(item, index ?? (oldIndex >= 0 ? oldIndex : null));
                return true;
            },
            FlourishRuntimeChangeKind.Updated,
            item.Id
        );
    }

    public void UpdateText(string id, string text)
    {
        UpdateText(id, text, lease: null);
    }

    private void UpdateText(string id, string text, Guid? lease)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            throw new ArgumentException("Status text cannot be empty.", nameof(text));
        }

        UpdateItem(id, item => item with { Text = text }, lease);
    }

    public void UpdateIcon(string id, string iconGlyph)
    {
        UpdateIcon(id, iconGlyph, lease: null);
    }

    private void UpdateIcon(string id, string iconGlyph, Guid? lease)
    {
        ArgumentNullException.ThrowIfNull(iconGlyph);
        UpdateItem(id, item => item with { IconGlyph = iconGlyph }, lease);
    }

    public void SetItemVisible(string id, bool visible)
    {
        UpdateItem(id, item => item with { IsVisible = visible });
    }

    public void Move(string id, int newIndex)
    {
        id = ValidateId(id, nameof(id));
        Mutate(
            () =>
            {
                var oldIndex = FindIndex(id);
                if (oldIndex < 0)
                {
                    throw new KeyNotFoundException($"Status item ID '{id}' was not found.");
                }

                if (newIndex < 0 || newIndex >= options.StatusItems.Count)
                {
                    throw new ArgumentOutOfRangeException(nameof(newIndex));
                }

                if (oldIndex == newIndex)
                {
                    return false;
                }

                var item = options.StatusItems[oldIndex];
                options.StatusItems.RemoveAt(oldIndex);
                options.StatusItems.Insert(newIndex, item);
                return true;
            },
            FlourishRuntimeChangeKind.Moved,
            id
        );
    }

    public bool Remove(string id)
    {
        id = ValidateId(id, nameof(id));
        return RemoveCore(id, lease: null);
    }

    public void Clear()
    {
        Mutate(
            () =>
            {
                if (options.StatusItems.Count == 0)
                {
                    return false;
                }

                foreach (var expiration in expirations.Values)
                {
                    expiration.Cancel();
                    expiration.Dispose();
                }

                expirations.Clear();
                leases.Clear();
                options.StatusItems.Clear();
                return true;
            },
            FlourishRuntimeChangeKind.Reset,
            itemId: null
        );
    }

    public IStatusBarItemHandle Show(
        string id,
        string text,
        string iconGlyph,
        TimeSpan? duration = null
    )
    {
        var item = new FlourishStatusItem(id, text, iconGlyph);
        ValidateItem(item);
        if (duration is { } timeout && timeout <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(duration), "Duration must be positive.");
        }

        var lease = Guid.NewGuid();
        CancellationToken? expirationToken = null;
        Mutate(
            () =>
            {
                var existingIndex = FindIndex(id);
                if (existingIndex >= 0)
                {
                    options.StatusItems[existingIndex] = item;
                }
                else
                {
                    options.StatusItems.Add(item);
                }

                CancelExpiration(id);
                leases[id] = lease;
                if (duration is not null)
                {
                    var expiration = new CancellationTokenSource();
                    expirationToken = expiration.Token;
                    expirations[id] = expiration;
                }

                return true;
            },
            FlourishRuntimeChangeKind.Updated,
            id
        );

        if (duration is { } delay && expirationToken is { } cancellationToken)
        {
            _ = ExpireAsync(id, lease, delay, cancellationToken);
        }

        return new StatusBarItemHandle(this, id, lease);
    }

    private async Task ExpireAsync(
        string id,
        Guid lease,
        TimeSpan duration,
        CancellationToken cancellationToken
    )
    {
        try
        {
            await Task.Delay(duration, cancellationToken).ConfigureAwait(false);
            RemoveCore(id, lease);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { }
    }

    private bool RemoveCore(string id, Guid? lease)
    {
        var removed = false;
        Mutate(
            () =>
            {
                if (
                    lease is not null
                    && (!leases.TryGetValue(id, out var currentLease) || currentLease != lease)
                )
                {
                    return false;
                }

                var index = FindIndex(id);
                if (index < 0)
                {
                    return false;
                }

                options.StatusItems.RemoveAt(index);
                CancelExpiration(id);
                leases.Remove(id);
                removed = true;
                return true;
            },
            FlourishRuntimeChangeKind.Removed,
            id
        );
        return removed;
    }

    private void UpdateItem(
        string id,
        Func<FlourishStatusItem, FlourishStatusItem> update,
        Guid? lease = null
    )
    {
        id = ValidateId(id, nameof(id));
        Mutate(
            () =>
            {
                if (
                    lease is not null
                    && (!leases.TryGetValue(id, out var currentLease) || currentLease != lease)
                )
                {
                    return false;
                }

                var index = FindIndex(id);
                if (index < 0)
                {
                    throw new KeyNotFoundException($"Status item ID '{id}' was not found.");
                }

                var replacement = update(options.StatusItems[index]);
                ValidateItem(replacement);
                if (!StringComparer.Ordinal.Equals(id, replacement.Id))
                {
                    throw new InvalidOperationException(
                        "A status item update cannot change its stable ID."
                    );
                }

                if (options.StatusItems[index] == replacement)
                {
                    return false;
                }

                options.StatusItems[index] = replacement;
                return true;
            },
            FlourishRuntimeChangeKind.Updated,
            id
        );
    }

    private void Mutate(Func<bool> mutation, FlourishRuntimeChangeKind changeKind, string? itemId)
    {
        FlourishStatusBarSnapshot snapshot;
        lock (gate)
        {
            if (!mutation())
            {
                return;
            }

            version++;
            snapshot = CreateSnapshot();
        }

        Changed?.Invoke(this, new FlourishStatusBarChangedEventArgs(snapshot, changeKind, itemId));
    }

    private FlourishStatusBarSnapshot CreateSnapshot()
    {
        return new FlourishStatusBarSnapshot(
            options.IsStatusBarEnabled,
            options.IsLANConnectionStatusEnabled,
            options.IsPowerStatusEnabled,
            options.StatusItems.ToArray(),
            version
        );
    }

    private int FindIndex(string id)
    {
        return options.StatusItems.FindIndex(item => StringComparer.Ordinal.Equals(item.Id, id));
    }

    private void Insert(FlourishStatusItem item, int? index)
    {
        if (index is null)
        {
            options.StatusItems.Add(item);
            return;
        }

        if (index < 0 || index > options.StatusItems.Count)
        {
            throw new ArgumentOutOfRangeException(nameof(index));
        }

        options.StatusItems.Insert(index.Value, item);
    }

    private void CancelExpiration(string id)
    {
        if (!expirations.Remove(id, out var expiration))
        {
            return;
        }

        expiration.Cancel();
        expiration.Dispose();
    }

    private static bool SetIfChanged(bool current, bool value, Action<bool> setter)
    {
        if (current == value)
        {
            return false;
        }

        setter(value);
        return true;
    }

    private static void ValidateItem(FlourishStatusItem item)
    {
        ArgumentNullException.ThrowIfNull(item);
        ValidateId(item.Id, nameof(item.Id));
        if (string.IsNullOrWhiteSpace(item.Text))
        {
            throw new ArgumentException("Status text cannot be empty.", nameof(item));
        }

        ArgumentNullException.ThrowIfNull(item.IconGlyph);
    }

    private static string ValidateId(string id, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            throw new ArgumentException("A stable ID is required.", parameterName);
        }

        return id;
    }

    private static void NormalizeSeedIds(List<FlourishStatusItem> items)
    {
        var usedIds = new HashSet<string>(StringComparer.Ordinal);
        for (var index = 0; index < items.Count; index++)
        {
            var item = items[index];
            var baseId = string.IsNullOrWhiteSpace(item.Id) ? $"status:{index}" : item.Id;
            var id = baseId;
            var suffix = 2;
            while (!usedIds.Add(id))
            {
                id = $"{baseId}:{suffix++}";
            }

            if (!StringComparer.Ordinal.Equals(item.Id, id))
            {
                items[index] = item with { Id = id };
            }
        }
    }

    private sealed class StatusBarItemHandle(FlourishStatusService owner, string id, Guid lease)
        : IStatusBarItemHandle
    {
        private FlourishStatusService? owner = owner;

        public string Id { get; } = id;

        public void UpdateText(string text)
        {
            owner?.UpdateText(Id, text, lease);
        }

        public void UpdateIcon(string iconGlyph)
        {
            owner?.UpdateIcon(Id, iconGlyph, lease);
        }

        public void Dispose()
        {
            Interlocked.Exchange(ref owner, null)?.RemoveCore(Id, lease);
        }
    }
}
