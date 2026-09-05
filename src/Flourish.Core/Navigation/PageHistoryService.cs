using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;

namespace ArkheideSystem.Flourish.Navigation;

internal sealed class PageHistoryService
{
    internal const int DefaultMaximumEntries = 100;

    private readonly LinkedList<NavigationStackEntry> backStack = new();
    private readonly LinkedList<NavigationStackEntry> forwardStack = new();
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

    public IReadOnlyCollection<NavigationStackEntry> BackStack
    {
        get
        {
            lock (gate)
            {
                return CreateSnapshot(backStack);
            }
        }
    }

    public IReadOnlyCollection<NavigationStackEntry> ForwardStack
    {
        get
        {
            lock (gate)
            {
                return CreateSnapshot(forwardStack);
            }
        }
    }

    public void Push(NavigationStackEntry entry)
    {
        ArgumentNullException.ThrowIfNull(entry);
        lock (gate)
        {
            Push(backStack, entry);
        }
    }

    public void PushForward(NavigationStackEntry entry)
    {
        ArgumentNullException.ThrowIfNull(entry);
        lock (gate)
        {
            Push(forwardStack, entry);
        }
    }

    public bool TryPopBack(out NavigationStackEntry entry)
    {
        lock (gate)
        {
            return TryPop(backStack, out entry);
        }
    }

    public bool TryPopForward(out NavigationStackEntry entry)
    {
        lock (gate)
        {
            return TryPop(forwardStack, out entry);
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

    public bool RemoveWhere(Func<NavigationStackEntry, bool> predicate)
    {
        ArgumentNullException.ThrowIfNull(predicate);
        lock (gate)
        {
            return RemoveFromHistory(backStack, predicate)
                | RemoveFromHistory(forwardStack, predicate);
        }
    }

    private void Push(
        LinkedList<NavigationStackEntry> history,
        NavigationStackEntry entry
    )
    {
        history.AddFirst(entry);
        if (history.Count > maximumEntries)
        {
            history.RemoveLast();
        }
    }

    private static bool TryPop(
        LinkedList<NavigationStackEntry> history,
        out NavigationStackEntry entry
    )
    {
        if (history.First is not { } first)
        {
            entry = default!;
            return false;
        }

        entry = first.Value;
        history.RemoveFirst();
        return true;
    }

    private static IReadOnlyCollection<NavigationStackEntry> CreateSnapshot(
        IEnumerable<NavigationStackEntry> history
    )
    {
        return new ReadOnlyCollection<NavigationStackEntry>(history.ToArray());
    }

    private static bool RemoveFromHistory(
        LinkedList<NavigationStackEntry> history,
        Func<NavigationStackEntry, bool> predicate
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
