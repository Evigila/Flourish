using System;
using System.Collections.Generic;
using System.Linq;

namespace ArkheideSystem.Flourish.Abstract;

/// <summary>Provides transactional access to the platform-neutral navigation menu state.</summary>
public interface INavigationMenuStateService
{
    /// <summary>Gets the latest immutable navigation menu snapshot.</summary>
    NavigationMenuSnapshot Current { get; }

    /// <summary>Occurs once after a transaction commits a changed menu state.</summary>
    event EventHandler<StateTransitionEventArgs<NavigationMenuSnapshot>>? Changed;

    /// <summary>Applies a group of menu edits as one atomic transaction.</summary>
    /// <remarks>
    /// The transaction is discarded when the callback or final-state validation fails. A
    /// transaction that produces the existing state does not advance the snapshot version or
    /// raise <see cref="Changed" />.
    /// </remarks>
    void Set(Action<INavigationMenuEditor> update);
}

/// <summary>Edits the navigation menu within a transaction.</summary>
public interface INavigationMenuEditor
{
    /// <summary>Appends a navigation group to the end of the menu.</summary>
    void AddGroup(string id, string? title = null);

    /// <summary>Inserts a navigation group at a zero-based index.</summary>
    void SetGroupIndex(string id, int index, string? title = null);

    /// <summary>Removes a group and all items it contains.</summary>
    bool RemoveGroup(string id);

    /// <summary>Moves a group to a zero-based index.</summary>
    void SetGroupOrder(string id, int newIndex);

    /// <summary>Changes a group's visible title.</summary>
    void SetGroupTitle(string id, string? title);

    /// <summary>Appends an item to the end of a scrollable navigation group.</summary>
    void AddItem(string groupId, NavigationMenuItem item);

    /// <summary>Inserts an item at a zero-based index in a scrollable navigation group.</summary>
    void SetItemIndex(string groupId, NavigationMenuItem item, int index);

    /// <summary>Appends an item to the end of the fixed bottom section.</summary>
    void AddFixedItem(NavigationMenuItem item);

    /// <summary>Inserts an item at a zero-based index in the fixed bottom section.</summary>
    void SetFixedItemIndex(NavigationMenuItem item, int index);

    /// <summary>Adds the item or replaces the existing item with the same stable ID.</summary>
    void SetItem(
        string? groupId,
        NavigationMenuItem item,
        bool isFixed = false,
        int? index = null
    );

    /// <summary>Removes an item by stable ID.</summary>
    bool RemoveItem(string id);

    /// <summary>Moves an item to another group or to the fixed section.</summary>
    void SetItemPosition(string id, string? targetGroupId, int newIndex, bool isFixed = false);

    /// <summary>Replaces an item using a transformation callback.</summary>
    void SetItem(
        string id,
        Func<NavigationMenuItem, NavigationMenuItem> update
    );

    /// <summary>Shows or hides an item without changing its tree expansion state.</summary>
    void SetItemVisible(string id, bool visible);

    /// <summary>Enables or disables interaction with an item.</summary>
    void SetItemEnabled(string id, bool enabled);

    /// <summary>Expands or collapses the children of a parent item.</summary>
    void SetItemExpanded(string id, bool expanded);
}

/// <summary>Identifies the behavior of a navigation menu item.</summary>
public enum NavigationMenuItemKind
{
    /// <summary>The item navigates to a registered route.</summary>
    Page,

    /// <summary>The item dispatches a command.</summary>
    Command,
}

/// <summary>Describes a runtime navigation menu item.</summary>
public sealed record NavigationMenuItem
{
    /// <summary>Creates a navigation menu item.</summary>
    public NavigationMenuItem(
        string id,
        string label,
        NavigationMenuItemKind kind,
        string? iconGlyph = null,
        string? navigationKey = null,
        string? commandKey = null,
        string? parentId = null
    )
    {
        Id = id;
        Label = label;
        Kind = kind;
        IconGlyph = iconGlyph ?? string.Empty;
        NavigationKey = navigationKey;
        CommandKey = commandKey;
        ParentId = parentId;
    }

    /// <summary>Creates an item that navigates to a registered route.</summary>
    public static NavigationMenuItem Page(
        string id,
        string navigationKey,
        string label,
        string? iconGlyph = null,
        string? parentId = null
    ) => new(
        id,
        label,
        NavigationMenuItemKind.Page,
        iconGlyph,
        navigationKey,
        parentId: parentId
    );

    /// <summary>Creates an item that dispatches a command.</summary>
    public static NavigationMenuItem Command(
        string id,
        string label,
        string? iconGlyph = null,
        string? commandKey = null,
        string? parentId = null
    ) => new(
        id,
        label,
        NavigationMenuItemKind.Command,
        iconGlyph,
        commandKey: commandKey,
        parentId: parentId
    );

    /// <summary>Gets the stable item ID.</summary>
    public string Id { get; init; }

    /// <summary>Gets the visible label.</summary>
    public string Label { get; init; }

    /// <summary>Gets the item behavior.</summary>
    public NavigationMenuItemKind Kind { get; init; }

    /// <summary>Gets the optional icon glyph.</summary>
    public string IconGlyph { get; init; }

    /// <summary>Gets the route used by a page item.</summary>
    public string? NavigationKey { get; init; }

    /// <summary>Gets the command key used by a command item.</summary>
    public string? CommandKey { get; init; }

    /// <summary>Gets the stable ID of this item's parent, if any.</summary>
    public string? ParentId { get; init; }

    /// <summary>Gets whether the item is explicitly visible.</summary>
    public bool IsVisible { get; init; } = true;

    /// <summary>Gets whether the item accepts interaction.</summary>
    public bool IsEnabled { get; init; } = true;

    /// <summary>Gets whether a parent item is expanded.</summary>
    public bool IsExpanded { get; init; }
}

/// <summary>Represents a navigation group in a menu snapshot.</summary>
public sealed record NavigationMenuGroup
{
    private IReadOnlyList<NavigationMenuItem> items = Array.Empty<NavigationMenuItem>();

    /// <summary>Creates an immutable navigation group snapshot.</summary>
    public NavigationMenuGroup(
        string Id,
        string? Title,
        IReadOnlyList<NavigationMenuItem> Items
    )
    {
        this.Id = Id;
        this.Title = Title;
        this.Items = Items;
    }

    /// <summary>Gets the stable group ID.</summary>
    public string Id { get; init; }

    /// <summary>Gets the optional visible title.</summary>
    public string? Title { get; init; }

    /// <summary>Gets the immutable item snapshot.</summary>
    public IReadOnlyList<NavigationMenuItem> Items
    {
        get => items;
        init
        {
            ArgumentNullException.ThrowIfNull(value);
            items = Array.AsReadOnly(value.ToArray());
        }
    }

    /// <summary>Deconstructs the group snapshot.</summary>
    public void Deconstruct(
        out string id,
        out string? title,
        out IReadOnlyList<NavigationMenuItem> items
    )
    {
        id = Id;
        title = Title;
        items = Items;
    }
}

/// <summary>Represents an immutable navigation menu snapshot.</summary>
public sealed record NavigationMenuSnapshot
{
    private IReadOnlyList<NavigationMenuGroup> groups = Array.Empty<NavigationMenuGroup>();
    private IReadOnlyList<NavigationMenuItem> fixedItems = Array.Empty<NavigationMenuItem>();

    /// <summary>Creates an immutable navigation menu snapshot.</summary>
    public NavigationMenuSnapshot(
        IReadOnlyList<NavigationMenuGroup> Groups,
        IReadOnlyList<NavigationMenuItem> FixedItems,
        long Version
    )
    {
        this.Groups = Groups;
        this.FixedItems = FixedItems;
        this.Version = Version;
    }

    /// <summary>Gets the immutable group snapshot.</summary>
    public IReadOnlyList<NavigationMenuGroup> Groups
    {
        get => groups;
        init
        {
            ArgumentNullException.ThrowIfNull(value);
            groups = Array.AsReadOnly(value.ToArray());
        }
    }

    /// <summary>Gets the immutable fixed-item snapshot.</summary>
    public IReadOnlyList<NavigationMenuItem> FixedItems
    {
        get => fixedItems;
        init
        {
            ArgumentNullException.ThrowIfNull(value);
            fixedItems = Array.AsReadOnly(value.ToArray());
        }
    }

    /// <summary>Gets the snapshot version.</summary>
    public long Version { get; init; }

    /// <summary>Deconstructs the menu snapshot.</summary>
    public void Deconstruct(
        out IReadOnlyList<NavigationMenuGroup> groups,
        out IReadOnlyList<NavigationMenuItem> fixedItems,
        out long version
    )
    {
        groups = Groups;
        fixedItems = FixedItems;
        version = Version;
    }
}
