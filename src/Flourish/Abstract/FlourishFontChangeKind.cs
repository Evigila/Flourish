namespace ArkheideSystem.Flourish.Abstract;

/// <summary>
/// Identifies the scope of a runtime font change.
/// </summary>
public enum FlourishFontChangeKind
{
    /// <summary>
    /// The global text font family or typography scale changed.
    /// </summary>
    GlobalText,

    /// <summary>
    /// The global icon font family changed.
    /// </summary>
    Icon,

    /// <summary>
    /// A page-specific font override changed.
    /// </summary>
    PageOverride,
}
