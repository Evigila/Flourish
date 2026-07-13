namespace ArkheideSystem.Flourish.Abstract;

/// <summary>
/// Provides transactional runtime editing of the Flourish navigation menu.
/// </summary>
public interface INavigationMenuService
{
    /// <summary>Occurs after a successful menu transaction is committed.</summary>
    event EventHandler<FlourishNavigationMenuChangedEventArgs>? Changed;

    /// <summary>Gets an immutable snapshot of the current menu.</summary>
    FlourishNavigationMenuSnapshot Current { get; }

    /// <summary>
    /// Applies one or more changes atomically. If validation fails, no changes are committed.
    /// </summary>
    /// <param name="update">The editing callback.</param>
    void Update(Action<INavigationMenuEditor> update);
}

/// <summary>Edits the navigation menu within a transaction.</summary>
public interface INavigationMenuEditor
{
    /// <summary>Adds a navigation group.</summary>
    void AddGroup(string id, string? title = null, int? index = null);

    /// <summary>Removes a group and all items it contains.</summary>
    bool RemoveGroup(string id);

    /// <summary>Moves a group to a zero-based index.</summary>
    void MoveGroup(string id, int newIndex);

    /// <summary>Changes a group's visible title.</summary>
    void SetGroupTitle(string id, string? title);

    /// <summary>Adds an item to a scrollable navigation group.</summary>
    void AddItem(string groupId, FlourishNavigationMenuItem item, int? index = null);

    /// <summary>Adds an item to the fixed bottom section.</summary>
    void AddFixedItem(FlourishNavigationMenuItem item, int? index = null);

    /// <summary>Adds the item or replaces the existing item with the same stable ID.</summary>
    void UpsertItem(
        string? groupId,
        FlourishNavigationMenuItem item,
        bool isFixed = false,
        int? index = null
    );

    /// <summary>Removes an item by stable ID.</summary>
    bool RemoveItem(string id);

    /// <summary>Moves an item to another group or to the fixed section.</summary>
    void MoveItem(string id, string? targetGroupId, int newIndex, bool isFixed = false);

    /// <summary>Replaces an item using a transformation callback.</summary>
    void UpdateItem(
        string id,
        Func<FlourishNavigationMenuItem, FlourishNavigationMenuItem> update
    );

    /// <summary>Shows or hides an item without changing its tree expansion state.</summary>
    void SetItemVisible(string id, bool visible);

    /// <summary>Enables or disables interaction with an item.</summary>
    void SetItemEnabled(string id, bool enabled);

    /// <summary>Expands or collapses the children of a parent item.</summary>
    void SetItemExpanded(string id, bool expanded);
}

/// <summary>Identifies the behavior of a navigation menu item.</summary>
public enum FlourishNavigationMenuItemKind
{
    /// <summary>The item navigates to a registered Flourish route.</summary>
    Page,

    /// <summary>The item dispatches a Flourish command.</summary>
    Command,
}

/// <summary>Describes a runtime navigation menu item.</summary>
public sealed record FlourishNavigationMenuItem
{
    /// <summary>Creates a navigation menu item.</summary>
    public FlourishNavigationMenuItem(
        string id,
        string label,
        FlourishNavigationMenuItemKind kind,
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
    public static FlourishNavigationMenuItem Page(
        string id,
        string navigationKey,
        string label,
        string? iconGlyph = null,
        string? parentId = null
    ) => new(
        id,
        label,
        FlourishNavigationMenuItemKind.Page,
        iconGlyph,
        navigationKey,
        parentId: parentId
    );

    /// <summary>Creates an item that dispatches a command.</summary>
    public static FlourishNavigationMenuItem Command(
        string id,
        string label,
        string? iconGlyph = null,
        string? commandKey = null,
        string? parentId = null
    ) => new(
        id,
        label,
        FlourishNavigationMenuItemKind.Command,
        iconGlyph,
        commandKey: commandKey,
        parentId: parentId
    );

    /// <summary>Gets the stable item ID.</summary>
    public string Id { get; init; }

    /// <summary>Gets the visible label.</summary>
    public string Label { get; init; }

    /// <summary>Gets the item behavior.</summary>
    public FlourishNavigationMenuItemKind Kind { get; init; }

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
public sealed record FlourishNavigationMenuGroup(
    string Id,
    string? Title,
    IReadOnlyList<FlourishNavigationMenuItem> Items
);

/// <summary>Represents an immutable navigation menu snapshot.</summary>
public sealed record FlourishNavigationMenuSnapshot(
    IReadOnlyList<FlourishNavigationMenuGroup> Groups,
    IReadOnlyList<FlourishNavigationMenuItem> FixedItems,
    long Version
);

/// <summary>Provides data for <see cref="INavigationMenuService.Changed" />.</summary>
public sealed class FlourishNavigationMenuChangedEventArgs(
    FlourishNavigationMenuSnapshot previous,
    FlourishNavigationMenuSnapshot current
) : EventArgs
{
    /// <summary>Gets the state before the transaction.</summary>
    public FlourishNavigationMenuSnapshot Previous { get; } = previous;

    /// <summary>Gets the committed state.</summary>
    public FlourishNavigationMenuSnapshot Current { get; } = current;
}
