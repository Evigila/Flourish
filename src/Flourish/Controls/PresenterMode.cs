namespace ArkheideSystem.Flourish.Controls;

/// <summary>
/// Describes how presentation content is composed with a presenter's copy and body.
/// </summary>
public enum PresenterMode
{
    /// <summary>
    /// Copy and body occupy one horizontal column while presentation content occupies the
    /// opposing column.
    /// </summary>
    Split,

    /// <summary>
    /// Presentation content occupies the upper region while copy and body content occupy the
    /// lower, left-aligned region.
    /// </summary>
    TopDown,

    /// <summary>The presenter fills the control and the textual content is drawn over it.</summary>
    Overlay,
}
