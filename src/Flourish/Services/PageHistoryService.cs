namespace ArkheideSystem.Flourish.Services;

internal sealed class PageHistoryService
{
    internal const int DefaultMaximumEntries = 100;

    private readonly LinkedList<FlourishPageStackEntry> backStack = new();
    private readonly LinkedList<FlourishPageStackEntry> forwardStack = new();
    private readonly Lock gate = new();
    private readonly int maximumEntries;

    public PageHistoryService()
        : this(DefaultMaximumEntries) { }

    internal PageHistoryService(int maximumEntries)
    {
        if (maximumEntries <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(maximumEntries),
                maximumEntries,
                "The navigation history capacity must be greater than zero."
            );
        }

        this.maximumEntries = maximumEntries;
    }

    public bool CanGoBack
    {
        get
        {
            lock (gate)
            {
                return backStack.Count > 0;
            }
        }
    }

    public bool CanGoForward
    {
        get
        {
            lock (gate)
            {
                return forwardStack.Count > 0;
            }
        }
    }

    public IReadOnlyCollection<FlourishPageStackEntry> BackStack
    {
        get
        {
            lock (gate)
            {
                return backStack.ToArray();
            }
        }
    }

    public IReadOnlyCollection<FlourishPageStackEntry> ForwardStack
    {
        get
        {
            lock (gate)
            {
                return forwardStack.ToArray();
            }
        }
    }

    public void Push(FlourishPageStackEntry entry)
    {
        ArgumentNullException.ThrowIfNull(entry);
        lock (gate)
        {
            Push(backStack, entry);
        }
    }

    public void PushForward(FlourishPageStackEntry entry)
    {
        ArgumentNullException.ThrowIfNull(entry);
        lock (gate)
        {
            Push(forwardStack, entry);
        }
    }

    public bool TryPopBack(out FlourishPageStackEntry entry)
    {
        lock (gate)
        {
            if (backStack.Count == 0)
            {
                entry = default!;
                return false;
            }

            entry = backStack.First!.Value;
            backStack.RemoveFirst();
            return true;
        }
    }

    public bool TryPopForward(out FlourishPageStackEntry entry)
    {
        lock (gate)
        {
            if (forwardStack.Count == 0)
            {
                entry = default!;
                return false;
            }

            entry = forwardStack.First!.Value;
            forwardStack.RemoveFirst();
            return true;
        }
    }

    public void ClearForward()
    {
        lock (gate)
        {
            forwardStack.Clear();
        }
    }

    public void ClearBack()
    {
        lock (gate)
        {
            backStack.Clear();
        }
    }

    public void Clear()
    {
        lock (gate)
        {
            backStack.Clear();
            forwardStack.Clear();
        }
    }

    public void Remove(string navigationKey)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(navigationKey);
        RemoveWhere(entry =>
            StringComparer.Ordinal.Equals(entry.NavigationKey, navigationKey)
        );
    }

    public bool RemoveWhere(Func<FlourishPageStackEntry, bool> predicate)
    {
        ArgumentNullException.ThrowIfNull(predicate);
        lock (gate)
        {
            return RemoveFromHistory(backStack, predicate)
                | RemoveFromHistory(forwardStack, predicate);
        }
    }

    private void Push(
        LinkedList<FlourishPageStackEntry> history,
        FlourishPageStackEntry entry
    )
    {
        history.AddFirst(entry);
        if (history.Count > maximumEntries)
        {
            history.RemoveLast();
        }
    }

    private static bool RemoveFromHistory(
        LinkedList<FlourishPageStackEntry> history,
        Func<FlourishPageStackEntry, bool> predicate
    )
    {
        var removed = false;
        var node = history.First;
        while (node is not null)
        {
            var next = node.Next;
            if (predicate(node.Value))
            {
                history.Remove(node);
                removed = true;
            }

            node = next;
        }

        return removed;
    }
}
