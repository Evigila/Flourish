using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using ArkheideSystem.Flourish.Abstract;

namespace ArkheideSystem.Flourish.Shell.StatusBar;

internal sealed class StatusBarService : IStatusBarService, IDisposable
{
    private readonly Lock gate = new();
    private readonly StatusBarOptions options;
    private readonly Dictionary<string, CancellationTokenSource> expirations = new(
        StringComparer.Ordinal
    );
    private readonly Dictionary<string, Guid> leases = new(StringComparer.Ordinal);
    private StatusBarSnapshot current;
    private long version;
    private bool isDisposed;

    public StatusBarService(StatusBarOptions options)
    {
        this.options = options ?? throw new ArgumentNullException(nameof(options));
        NormalizeSeedIds(options.StatusItems);
        current = CreateSnapshot();
    }

    public event EventHandler<StatusBarChangedEventArgs>? Changed;

    public StatusBarSnapshot Current
    {
        get
        {
            ObjectDisposedException.ThrowIf(Volatile.Read(ref isDisposed), this);
            return Volatile.Read(ref current);
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
            CollectionChangeKind.Updated,
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
            CollectionChangeKind.Updated,
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
            CollectionChangeKind.Updated,
            itemId: null
        );
    }

    public void AddStatusItem(StatusBarItem item, int? index = null)
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
            CollectionChangeKind.Added,
            item.Id
        );
    }

    public void SetItem(StatusBarItem item, int? index = null)
    {
        ValidateItem(item);
        Mutate(
            () =>
            {
                int oldIndex = FindIndex(item.Id);
                if (oldIndex >= 0)
                {
                    options.StatusItems.RemoveAt(oldIndex);
                }

                CancelExpiration(item.Id);
                leases.Remove(item.Id);
                Insert(item, index ?? (oldIndex >= 0 ? oldIndex : null));
                return true;
            },
            CollectionChangeKind.Updated,
            item.Id
        );
    }

    public void SetItemText(string id, string text)
    {
        SetItemText(id, text, lease: null);
    }

    private void SetItemText(string id, string text, Guid? lease)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            throw new ArgumentException("Status text cannot be empty.", nameof(text));
        }

        UpdateItem(id, item => item with { Text = text }, lease);
    }

    public void SetItemIcon(string id, string iconGlyph)
    {
        SetItemIcon(id, iconGlyph, lease: null);
    }

    private void SetItemIcon(string id, string iconGlyph, Guid? lease)
    {
        ArgumentNullException.ThrowIfNull(iconGlyph);
        UpdateItem(id, item => item with { IconGlyph = iconGlyph }, lease);
    }

    public void SetItemVisible(string id, bool visible)
    {
        UpdateItem(id, item => item with { IsVisible = visible });
    }

    public void SetOrder(string id, int newIndex)
    {
        id = ValidateId(id, nameof(id));
        Mutate(
            () =>
            {
                int oldIndex = FindIndex(id);
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

                StatusBarItem item = options.StatusItems[oldIndex];
                options.StatusItems.RemoveAt(oldIndex);
                options.StatusItems.Insert(newIndex, item);
                return true;
            },
            CollectionChangeKind.Moved,
            id
        );
    }

    public bool Remove(string id)
    {
        id = ValidateId(id, nameof(id));
        return RemoveCore(id, lease: null, throwIfDisposed: true);
    }

    public void RemoveAll()
    {
        Mutate(
            () =>
            {
                if (options.StatusItems.Count == 0)
                {
                    return false;
                }

                foreach (CancellationTokenSource expiration in expirations.Values)
                {
                    expiration.Cancel();
                    expiration.Dispose();
                }

                expirations.Clear();
                leases.Clear();
                options.StatusItems.Clear();
                return true;
            },
            CollectionChangeKind.Reset,
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
        var item = new StatusBarItem(id, text, iconGlyph);
        ValidateItem(item);
        if (duration is { } timeout && timeout <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(duration), "Duration must be positive.");
        }

        Guid lease = Guid.NewGuid();
        CancellationToken? expirationToken = null;
        Mutate(
            () =>
            {
                int existingIndex = FindIndex(id);
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
            CollectionChangeKind.Updated,
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
            RemoveCore(id, lease, throwIfDisposed: false);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { }
        catch (Exception error)
        {
            Debug.WriteLine($"Flourish status item expiration failed: {error}");
        }
    }

    private bool RemoveCore(string id, Guid? lease, bool throwIfDisposed)
    {
        bool removed = false;
        Mutate(
            () =>
            {
                if (
                    lease is not null
                    && (!leases.TryGetValue(id, out Guid currentLease) || currentLease != lease)
                )
                {
                    return false;
                }

                int index = FindIndex(id);
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
            CollectionChangeKind.Removed,
            id,
            throwIfDisposed
        );
        return removed;
    }

    private bool IsLeaseRegistered(string id, Guid lease)
    {
        lock (gate)
        {
            return !isDisposed
                && leases.TryGetValue(id, out Guid currentLease)
                && currentLease == lease
                && FindIndex(id) >= 0;
        }
    }

    private void UpdateItem(
        string id,
        Func<StatusBarItem, StatusBarItem> update,
        Guid? lease = null
    )
    {
        id = ValidateId(id, nameof(id));
        Mutate(
            () =>
            {
                if (
                    lease is not null
                    && (!leases.TryGetValue(id, out Guid currentLease) || currentLease != lease)
                )
                {
                    return false;
                }

                int index = FindIndex(id);
                if (index < 0)
                {
                    throw new KeyNotFoundException($"Status item ID '{id}' was not found.");
                }

                StatusBarItem replacement = update(options.StatusItems[index]);
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
            CollectionChangeKind.Updated,
            id
        );
    }

    private void Mutate(
        Func<bool> mutation,
        CollectionChangeKind changeKind,
        string? itemId,
        bool throwIfDisposed = true
    )
    {
        StatusBarSnapshot snapshot;
        lock (gate)
        {
            if (isDisposed)
            {
                if (throwIfDisposed)
                {
                    throw new ObjectDisposedException(nameof(StatusBarService));
                }

                return;
            }

            if (!mutation())
            {
                return;
            }

            version++;
            snapshot = CreateSnapshot();
            Volatile.Write(ref current, snapshot);
        }

        Changed?.Invoke(this, new StatusBarChangedEventArgs(snapshot, changeKind, itemId));
    }

    public void Dispose()
    {
        CancellationTokenSource[] pendingExpirations;
        lock (gate)
        {
            if (isDisposed)
            {
                return;
            }

            Volatile.Write(ref isDisposed, true);
            pendingExpirations = expirations.Values.ToArray();
            expirations.Clear();
            leases.Clear();
        }

        foreach (CancellationTokenSource expiration in pendingExpirations)
        {
            try
            {
                expiration.Cancel();
            }
            catch (Exception error)
            {
                Debug.WriteLine($"Flourish status item cancellation failed: {error}");
            }
            finally
            {
                expiration.Dispose();
            }
        }
    }

    private StatusBarSnapshot CreateSnapshot()
    {
        return new StatusBarSnapshot(
            options.IsStatusBarEnabled,
            options.IsLANConnectionStatusEnabled,
            options.IsPowerStatusEnabled,
            Array.AsReadOnly(options.StatusItems.ToArray()),
            version
        );
    }

    private int FindIndex(string id)
    {
        return options.StatusItems.FindIndex(item => StringComparer.Ordinal.Equals(item.Id, id));
    }

    private void Insert(StatusBarItem item, int? index)
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
        if (!expirations.Remove(id, out CancellationTokenSource? expiration))
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

    private static void ValidateItem(StatusBarItem item)
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

    private static void NormalizeSeedIds(List<StatusBarItem> items)
    {
        var usedIds = new HashSet<string>(StringComparer.Ordinal);
        for (int index = 0; index < items.Count; index++)
        {
            StatusBarItem item = items[index];
            string baseId = string.IsNullOrWhiteSpace(item.Id) ? $"status:{index}" : item.Id;
            string id = baseId;
            int suffix = 2;
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

    private sealed class StatusBarItemHandle(
        StatusBarService owner,
        string id,
        Guid lease
    ) : IStatusBarItemHandle
    {
        private StatusBarService? owner = owner;

        public string Id { get; } = id;

        public bool IsRegistered =>
            Volatile.Read(ref owner) is { } currentOwner
            && currentOwner.IsLeaseRegistered(Id, lease);

        public void SetText(string text)
        {
            owner?.SetItemText(Id, text, lease);
        }

        public void SetIcon(string iconGlyph)
        {
            owner?.SetItemIcon(Id, iconGlyph, lease);
        }

        public void Dispose()
        {
            Interlocked.Exchange(ref owner, null)
                ?.RemoveCore(Id, lease, throwIfDisposed: false);
        }
    }
}
