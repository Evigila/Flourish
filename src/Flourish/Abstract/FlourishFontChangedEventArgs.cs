namespace ArkheideSystem.Flourish.Abstract;

/// <summary>
/// Describes a runtime font change.
/// </summary>
public sealed class FlourishFontChangedEventArgs : EventArgs
{
    /// <summary>
    /// Initializes a global text-font change notification.
    /// </summary>
    public FlourishFontChangedEventArgs(
        string fontFamily,
        string iconFontFamily,
        double fontSize
    )
        : this(
            fontFamily,
            iconFontFamily,
            fontSize,
            FlourishFontChangeKind.GlobalText,
            null
        )
    { }

    /// <summary>
    /// Initializes a font change notification with its affected scope.
    /// </summary>
    public FlourishFontChangedEventArgs(
        string fontFamily,
        string iconFontFamily,
        double fontSize,
        FlourishFontChangeKind changeKind,
        Type? affectedPageType = null
    )
    {
        if (!Enum.IsDefined(changeKind))
        {
            throw new ArgumentOutOfRangeException(nameof(changeKind), changeKind, "Unknown value.");
        }

        if (
            changeKind == FlourishFontChangeKind.PageOverride
            && affectedPageType is null
        )
        {
            throw new ArgumentNullException(nameof(affectedPageType));
        }

        FontFamily = fontFamily;
        IconFontFamily = iconFontFamily;
        FontSize = fontSize;
        ChangeKind = changeKind;
        AffectedPageType = affectedPageType;
    }

    /// <summary>
    /// Gets the current text font family name.
    /// </summary>
    public string FontFamily { get; }

    /// <summary>
    /// Gets the current icon font family name.
    /// </summary>
    public string IconFontFamily { get; }

    /// <summary>
    /// Gets the current base font size.
    /// </summary>
    public double FontSize { get; }

    /// <summary>
    /// Gets the scope of the change.
    /// </summary>
    public FlourishFontChangeKind ChangeKind { get; }

    /// <summary>
    /// Gets the configured page type affected by an override change, when applicable.
    /// </summary>
    public Type? AffectedPageType { get; }
}
