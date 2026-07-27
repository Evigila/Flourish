using System.Windows.Controls;

namespace ArkheideSystem.Flourish.Abstract.Runtime;

/// <summary>Provides runtime control over the Flourish dynamic toolbar.</summary>
public interface IToolbarService
{
    /// <summary>Occurs after the toolbar state changes.</summary>
    event EventHandler<FlourishToolbarChangedEventArgs>? Changed;

    /// <summary>Gets an immutable snapshot of all toolbar definitions.</summary>
    FlourishToolbarSnapshot Current { get; }

    /// <summary>Enables or disables the complete toolbar surface.</summary>
    void SetEnabled(bool enabled);

    /// <summary>Replaces the default toolbar used when no page-specific toolbar exists.</summary>
    void ReplaceDefault(IEnumerable<FlourishToolbarItem> items);

    /// <summary>Replaces the toolbar for a page type.</summary>
    void Replace(Type pageType, IEnumerable<FlourishToolbarItem> items, bool iconOnly = true);

    /// <summary>Replaces the toolbar for a page type.</summary>
    void Replace<TPage>(IEnumerable<FlourishToolbarItem> items, bool iconOnly = true)
        where TPage : Page;

    /// <summary>Adds an item to the selected toolbar.</summary>
    void Add(FlourishToolbarItem item, Type? pageType = null, int? index = null);

    /// <summary>Adds or replaces an item by stable ID.</summary>
    void Upsert(FlourishToolbarItem item, Type? pageType = null, int? index = null);

    /// <summary>Removes an item by stable ID.</summary>
    bool Remove(string id, Type? pageType = null);

    /// <summary>Removes all items from the selected toolbar.</summary>
    void Clear(Type? pageType = null);

    /// <summary>Moves an item to a zero-based index.</summary>
    void Move(string id, int newIndex, Type? pageType = null);

    /// <summary>Enables or disables an item.</summary>
    void SetItemEnabled(string id, bool enabled, Type? pageType = null);

    /// <summary>Shows or hides an item.</summary>
    void SetItemVisible(string id, bool visible, Type? pageType = null);

    /// <summary>Changes the icon-only presentation mode of a page toolbar.</summary>
    void SetIconOnly(Type pageType, bool iconOnly);
}

/// <summary>Describes a page-specific toolbar.</summary>
public sealed record FlourishPageToolbarSnapshot(
    Type PageType,
    bool IconOnly,
    IReadOnlyList<FlourishToolbarItem> Items
);

/// <summary>Represents all current toolbar definitions.</summary>
public sealed record FlourishToolbarSnapshot(
    bool IsEnabled,
    IReadOnlyList<FlourishToolbarItem> DefaultItems,
    IReadOnlyDictionary<Type, FlourishPageToolbarSnapshot> Pages,
    long Version
);

/// <summary>Provides data for <see cref="IToolbarService.Changed" />.</summary>
public sealed class FlourishToolbarChangedEventArgs(
    FlourishToolbarSnapshot current,
    FlourishRuntimeChangeKind changeKind,
    Type? pageType,
    string? itemId
) : EventArgs
{
    /// <summary>Gets the new state.</summary>
    public FlourishToolbarSnapshot Current { get; } = current;

    /// <summary>Gets the mutation kind.</summary>
    public FlourishRuntimeChangeKind ChangeKind { get; } = changeKind;

    /// <summary>Gets the affected page type, or <see langword="null" /> for the default/global toolbar.</summary>
    public Type? PageType { get; } = pageType;

    /// <summary>Gets the affected item ID, if applicable.</summary>
    public string? ItemId { get; } = itemId;
}
