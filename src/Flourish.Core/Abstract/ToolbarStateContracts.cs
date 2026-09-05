using System;
using System.Collections.Generic;

namespace ArkheideSystem.Flourish.Abstract;

/// <summary>Provides platform-neutral runtime control over dynamic toolbar state.</summary>
/// <remarks>
/// View-specific toolbars use stable, case-sensitive string keys. A <see langword="null" /> view
/// key selects the default toolbar. Empty and whitespace view keys are invalid.
/// </remarks>
public interface IToolbarStateService
{
    /// <summary>Occurs after the toolbar state changes.</summary>
    event EventHandler<ToolbarStateChangedEventArgs>? Changed;

    /// <summary>Gets an immutable snapshot of all toolbar definitions.</summary>
    ToolbarStateSnapshot Current { get; }

    /// <summary>Enables or disables the complete toolbar surface.</summary>
    void SetEnabled(bool enabled);

    /// <summary>Replaces the default toolbar used when no view-specific toolbar exists.</summary>
    void SetDefault(IEnumerable<ToolbarItem> items);

    /// <summary>Replaces the toolbar associated with a stable view key.</summary>
    void SetView(string viewKey, IEnumerable<ToolbarItem> items, bool iconOnly = true);

    /// <summary>Adds an item to the selected toolbar.</summary>
    void AddItem(ToolbarItem item, string? viewKey = null, int? index = null);

    /// <summary>Adds or replaces an item by stable ID.</summary>
    void SetItem(ToolbarItem item, string? viewKey = null, int? index = null);

    /// <summary>Removes an item by stable ID.</summary>
    bool Remove(string id, string? viewKey = null);

    /// <summary>Removes all items from the selected toolbar.</summary>
    void RemoveAll(string? viewKey = null);

    /// <summary>Moves an item to a zero-based index.</summary>
    void SetOrder(string id, int newIndex, string? viewKey = null);

    /// <summary>Enables or disables an item.</summary>
    void SetItemEnabled(string id, bool enabled, string? viewKey = null);

    /// <summary>Shows or hides an item.</summary>
    void SetItemVisible(string id, bool visible, string? viewKey = null);

    /// <summary>Changes the icon-only presentation mode of a view toolbar.</summary>
    void SetIconOnly(string viewKey, bool iconOnly);

    /// <summary>
    /// Resolves the toolbar items for a view, falling back to the default items when the surface is
    /// disabled or the view has no definition.
    /// </summary>
    IReadOnlyList<ToolbarItem> GetItems(string? viewKey = null);

    /// <summary>Gets a view's icon-only mode, defaulting to <see langword="true" />.</summary>
    bool GetIconOnly(string viewKey);
}

/// <summary>Describes a toolbar item displayed by an application shell.</summary>
public sealed record ToolbarItem
{
    /// <summary>Creates a toolbar item.</summary>
    /// <param name="displayName">The text displayed for the toolbar item.</param>
    /// <param name="iconGlyph">The icon glyph displayed for the toolbar item.</param>
    /// <param name="commandKey">The optional command key dispatched through <see cref="ICommandDispatcher" />.</param>
    public ToolbarItem(string displayName, string iconGlyph, string? commandKey = null)
    {
        DisplayName = displayName;
        IconGlyph = iconGlyph;
        CommandKey = commandKey;
        Id = CreateDefaultId(displayName, commandKey);
    }

    /// <summary>Gets the stable identifier used by runtime mutations.</summary>
    public string Id { get; init; }

    /// <summary>Gets the text displayed for the toolbar item.</summary>
    public string DisplayName { get; init; }

    /// <summary>Gets the icon glyph displayed for the toolbar item.</summary>
    public string IconGlyph { get; init; }

    /// <summary>Gets the optional command key dispatched through <see cref="ICommandDispatcher" />.</summary>
    public string? CommandKey { get; init; }

    /// <summary>Gets whether the item is visible.</summary>
    public bool IsVisible { get; init; } = true;

    /// <summary>Gets whether the item accepts interaction.</summary>
    public bool IsEnabled { get; init; } = true;

    private static string CreateDefaultId(string displayName, string? commandKey)
    {
        string? candidate = string.IsNullOrWhiteSpace(commandKey) ? displayName : commandKey;
        candidate = string.IsNullOrWhiteSpace(candidate) ? "item" : candidate.Trim();
        return $"toolbar:{candidate}";
    }
}

/// <summary>Describes a toolbar associated with a stable view key.</summary>
public sealed record ToolbarViewState(
    string ViewKey,
    bool IconOnly,
    IReadOnlyList<ToolbarItem> Items
);

/// <summary>Represents all current platform-neutral toolbar definitions.</summary>
public sealed record ToolbarStateSnapshot(
    bool IsEnabled,
    IReadOnlyList<ToolbarItem> DefaultItems,
    IReadOnlyDictionary<string, ToolbarViewState> Views,
    long Version
);

/// <summary>Provides data for <see cref="IToolbarStateService.Changed" />.</summary>
public sealed class ToolbarStateChangedEventArgs : EventArgs
{
    /// <summary>Initializes toolbar state change data.</summary>
    public ToolbarStateChangedEventArgs(
        ToolbarStateSnapshot current,
        CollectionChangeKind changeKind,
        string? viewKey,
        string? itemId
    )
    {
        Current = current;
        ChangeKind = changeKind;
        ViewKey = viewKey;
        ItemId = itemId;
    }

    /// <summary>Gets the new state.</summary>
    public ToolbarStateSnapshot Current { get; }

    /// <summary>Gets the mutation kind.</summary>
    public CollectionChangeKind ChangeKind { get; }

    /// <summary>Gets the affected view key, or <see langword="null" /> for the default toolbar.</summary>
    public string? ViewKey { get; }

    /// <summary>Gets the affected item ID, if applicable.</summary>
    public string? ItemId { get; }
}
