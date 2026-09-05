using System;
using System.Collections.Generic;

namespace ArkheideSystem.Flourish.Abstract;

/// <summary>Configures the application shell status bar.</summary>
public interface IStatusBarBuilder
{
    /// <summary>Enables or disables the persistent status bar.</summary>
    IStatusBarBuilder SetEnabled(bool enabled = true);

    /// <summary>Adds a status item with display text and an icon glyph.</summary>
    /// <param name="displayText">The status item display text.</param>
    /// <param name="iconGlyph">The icon glyph displayed before the text.</param>
    /// <returns>The current builder for chained configuration.</returns>
    IStatusBarBuilder AddStatusItem(string displayText = "OK", string iconGlyph = "\uE930");

    /// <summary>Enables the built-in LAN connection status.</summary>
    /// <returns>The current builder for chained configuration.</returns>
    IStatusBarBuilder SetLanStatusEnabled(bool enabled = true);

    /// <summary>Enables the built-in power status.</summary>
    /// <returns>The current builder for chained configuration.</returns>
    IStatusBarBuilder SetPowerStatusEnabled(bool enabled = true);
}

/// <summary>Provides runtime control over the status bar.</summary>
public interface IStatusBarService
{
    /// <summary>Occurs after status content or visibility changes.</summary>
    event EventHandler<StatusBarChangedEventArgs>? Changed;

    /// <summary>Gets an immutable snapshot of the status bar state.</summary>
    StatusBarSnapshot Current { get; }

    /// <summary>Enables or disables application-provided status content.</summary>
    void SetEnabled(bool enabled);

    /// <summary>Enables or disables the built-in LAN indicator.</summary>
    void SetLanStatusEnabled(bool enabled);

    /// <summary>Enables or disables the built-in power indicator.</summary>
    void SetPowerStatusEnabled(bool enabled);

    /// <summary>Adds a status item.</summary>
    void AddStatusItem(StatusBarItem item, int? index = null);

    /// <summary>Adds or replaces a status item by stable ID.</summary>
    void SetItem(StatusBarItem item, int? index = null);

    /// <summary>Updates the text of a status item.</summary>
    void SetItemText(string id, string text);

    /// <summary>Updates the icon of a status item.</summary>
    void SetItemIcon(string id, string iconGlyph);

    /// <summary>Shows or hides a status item.</summary>
    void SetItemVisible(string id, bool visible);

    /// <summary>Moves a status item to a zero-based index.</summary>
    void SetOrder(string id, int newIndex);

    /// <summary>Removes a status item.</summary>
    bool Remove(string id);

    /// <summary>Removes all application-provided status items.</summary>
    void RemoveAll();

    /// <summary>
    /// Adds or replaces a status item and returns a handle that removes it when disposed.
    /// </summary>
    IStatusBarItemHandle Show(
        string id,
        string text,
        string iconGlyph,
        TimeSpan? duration = null
    );
}

/// <summary>Controls the lifetime of a status item.</summary>
public interface IStatusBarItemHandle : IRegistration
{
    /// <summary>Gets the stable item ID.</summary>
    string Id { get; }

    /// <summary>Updates the item's text.</summary>
    void SetText(string text);

    /// <summary>Updates the item's icon glyph.</summary>
    void SetIcon(string iconGlyph);
}

/// <summary>Describes a status item.</summary>
public sealed record StatusBarItem
{
    /// <summary>Creates a status item with an automatically derived ID.</summary>
    public StatusBarItem(string text, string iconGlyph)
        : this(CreateDefaultId(text), text, iconGlyph) { }

    /// <summary>Creates a status item with a stable ID.</summary>
    public StatusBarItem(string id, string text, string iconGlyph)
    {
        Id = id;
        Text = text;
        IconGlyph = iconGlyph;
    }

    /// <summary>Gets the stable item ID.</summary>
    public string Id { get; init; }

    /// <summary>Gets the visible text.</summary>
    public string Text { get; init; }

    /// <summary>Gets the icon glyph.</summary>
    public string IconGlyph { get; init; }

    /// <summary>Gets whether the item is visible.</summary>
    public bool IsVisible { get; init; } = true;

    private static string CreateDefaultId(string text)
    {
        string value = string.IsNullOrWhiteSpace(text) ? "status" : text.Trim();
        return $"status:{value}";
    }
}

/// <summary>Represents the current status bar state.</summary>
public sealed record StatusBarSnapshot(
    bool IsEnabled,
    bool IsLanStatusEnabled,
    bool IsPowerStatusEnabled,
    IReadOnlyList<StatusBarItem> Items,
    long Version
);

/// <summary>Provides data for <see cref="IStatusBarService.Changed" />.</summary>
public sealed class StatusBarChangedEventArgs : EventArgs
{
    /// <summary>Initializes status bar change data.</summary>
    public StatusBarChangedEventArgs(
        StatusBarSnapshot current,
        CollectionChangeKind changeKind,
        string? itemId
    )
    {
        Current = current;
        ChangeKind = changeKind;
        ItemId = itemId;
    }

    /// <summary>Gets the new state.</summary>
    public StatusBarSnapshot Current { get; }

    /// <summary>Gets the mutation kind.</summary>
    public CollectionChangeKind ChangeKind { get; }

    /// <summary>Gets the affected item ID, if applicable.</summary>
    public string? ItemId { get; }
}
