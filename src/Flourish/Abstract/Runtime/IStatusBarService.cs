namespace ArkheideSystem.Flourish.Abstract.Runtime;

/// <summary>Provides runtime control over the Flourish status bar.</summary>
public interface IStatusBarService
{
    /// <summary>Occurs after status content or visibility changes.</summary>
    event EventHandler<FlourishStatusBarChangedEventArgs>? Changed;

    /// <summary>Gets an immutable snapshot of the status bar state.</summary>
    FlourishStatusBarSnapshot Current { get; }

    /// <summary>Enables or disables application-provided status content.</summary>
    void SetEnabled(bool enabled);

    /// <summary>Enables or disables the built-in LAN indicator.</summary>
    void SetLanStatusEnabled(bool enabled);

    /// <summary>Enables or disables the built-in power indicator.</summary>
    void SetPowerStatusEnabled(bool enabled);

    /// <summary>Adds a status item.</summary>
    void Append(FlourishStatusItem item, int? index = null);

    /// <summary>Adds or replaces a status item by stable ID.</summary>
    void SetItem(FlourishStatusItem item, int? index = null);

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
public interface IStatusBarItemHandle : IDisposable
{
    /// <summary>Gets the stable item ID.</summary>
    string Id { get; }

    /// <summary>Updates the item's text.</summary>
    void SetText(string text);

    /// <summary>Updates the item's icon glyph.</summary>
    void SetIcon(string iconGlyph);
}

/// <summary>Describes a status item.</summary>
public sealed record FlourishStatusItem
{
    /// <summary>Creates a status item with an automatically derived ID.</summary>
    public FlourishStatusItem(string text, string iconGlyph)
        : this(CreateDefaultId(text), text, iconGlyph) { }

    /// <summary>Creates a status item with a stable ID.</summary>
    public FlourishStatusItem(string id, string text, string iconGlyph)
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
        var value = string.IsNullOrWhiteSpace(text) ? "status" : text.Trim();
        return $"status:{value}";
    }
}

/// <summary>Represents the current status bar state.</summary>
public sealed record FlourishStatusBarSnapshot(
    bool IsEnabled,
    bool IsLanStatusEnabled,
    bool IsPowerStatusEnabled,
    IReadOnlyList<FlourishStatusItem> Items,
    long Version
);

/// <summary>Provides data for <see cref="IStatusBarService.Changed" />.</summary>
public sealed class FlourishStatusBarChangedEventArgs(
    FlourishStatusBarSnapshot current,
    FlourishRuntimeChangeKind changeKind,
    string? itemId
) : EventArgs
{
    /// <summary>Gets the new state.</summary>
    public FlourishStatusBarSnapshot Current { get; } = current;

    /// <summary>Gets the mutation kind.</summary>
    public FlourishRuntimeChangeKind ChangeKind { get; } = changeKind;

    /// <summary>Gets the affected item ID, if applicable.</summary>
    public string? ItemId { get; } = itemId;
}
