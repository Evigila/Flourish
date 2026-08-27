using System;
using System.Collections.Generic;

namespace ArkheideSystem.Flourish.Abstract;

/// <summary>Provides runtime control over the Flourish dynamic toolbar.</summary>
public interface IToolbarService
{
    /// <summary>Occurs after the toolbar state changes.</summary>
    event EventHandler<ToolbarChangedEventArgs>? Changed;

    /// <summary>Gets an immutable snapshot of all toolbar definitions.</summary>
    ToolbarSnapshot Current { get; }

    /// <summary>Enables or disables the complete toolbar surface.</summary>
    void SetEnabled(bool enabled);

    /// <summary>Replaces the default toolbar used when no page-specific toolbar exists.</summary>
    void SetDefault(IEnumerable<ToolbarItem> items);

    /// <summary>Replaces the toolbar for a page type.</summary>
    void Set(Type pageType, IEnumerable<ToolbarItem> items, bool iconOnly = true);

    /// <summary>Replaces the toolbar for a page type.</summary>
    void Set<TPage>(IEnumerable<ToolbarItem> items, bool iconOnly = true)
        where TPage : System.Windows.Controls.Page => Set(typeof(TPage), items, iconOnly);

    /// <summary>Adds an item to the selected toolbar.</summary>
    void AddItem(ToolbarItem item, Type? pageType = null, int? index = null);

    /// <summary>Adds or replaces an item by stable ID.</summary>
    void SetItem(ToolbarItem item, Type? pageType = null, int? index = null);

    /// <summary>Removes an item by stable ID.</summary>
    bool Remove(string id, Type? pageType = null);

    /// <summary>Removes all items from the selected toolbar.</summary>
    void RemoveAll(Type? pageType = null);

    /// <summary>Moves an item to a zero-based index.</summary>
    void SetOrder(string id, int newIndex, Type? pageType = null);

    /// <summary>Enables or disables an item.</summary>
    void SetItemEnabled(string id, bool enabled, Type? pageType = null);

    /// <summary>Shows or hides an item.</summary>
    void SetItemVisible(string id, bool visible, Type? pageType = null);

    /// <summary>Changes the icon-only presentation mode of a page toolbar.</summary>
    void SetIconOnly(Type pageType, bool iconOnly);
}

/// <summary>Describes a page-specific toolbar.</summary>
public sealed record PageToolbarSnapshot(
    Type PageType,
    bool IconOnly,
    IReadOnlyList<ToolbarItem> Items
);

/// <summary>Represents all current toolbar definitions.</summary>
public sealed record ToolbarSnapshot(
    bool IsEnabled,
    IReadOnlyList<ToolbarItem> DefaultItems,
    IReadOnlyDictionary<Type, PageToolbarSnapshot> Pages,
    long Version
);

/// <summary>Provides data for <see cref="IToolbarService.Changed" />.</summary>
public sealed class ToolbarChangedEventArgs(
    ToolbarSnapshot current,
    CollectionChangeKind changeKind,
    Type? pageType,
    string? itemId
) : EventArgs
{
    /// <summary>Gets the new state.</summary>
    public ToolbarSnapshot Current { get; } = current;

    /// <summary>Gets the mutation kind.</summary>
    public CollectionChangeKind ChangeKind { get; } = changeKind;

    /// <summary>Gets the affected page type, or <see langword="null" /> for the default/global toolbar.</summary>
    public Type? PageType { get; } = pageType;

    /// <summary>Gets the affected item ID, if applicable.</summary>
    public string? ItemId { get; } = itemId;
}
