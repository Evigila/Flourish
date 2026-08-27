using System;

namespace ArkheideSystem.Flourish.Abstract;

/// <summary>
/// Describes a runtime font change.
/// </summary>
public sealed class FontChangedEventArgs : EventArgs
{
    /// <summary>
    /// Initializes a font change notification with its affected scope.
    /// </summary>
    public FontChangedEventArgs(
        FontState current,
        FontChangeKind changeKind,
        Type? affectedPageType = null
    )
    {
        ArgumentNullException.ThrowIfNull(current);

        if (!Enum.IsDefined(changeKind))
        {
            throw new ArgumentOutOfRangeException(nameof(changeKind), changeKind, "Unknown value.");
        }

        if (changeKind == FontChangeKind.PageOverride && affectedPageType is null)
        {
            throw new ArgumentNullException(nameof(affectedPageType));
        }

        Current = current;
        ChangeKind = changeKind;
        AffectedPageType = affectedPageType;
    }

    /// <summary>Gets the state after the change.</summary>
    public FontState Current { get; }

    /// <summary>
    /// Gets the scope of the change.
    /// </summary>
    public FontChangeKind ChangeKind { get; }

    /// <summary>
    /// Gets the configured page type affected by an override change, when applicable.
    /// </summary>
    public Type? AffectedPageType { get; }

}
