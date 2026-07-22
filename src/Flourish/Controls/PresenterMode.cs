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

    /// <summary>The presenter fills the control and the textual content is drawn over it.</summary>
    Overlay,
}
