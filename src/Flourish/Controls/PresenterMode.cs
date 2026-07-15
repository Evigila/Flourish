namespace ArkheideSystem.Flourish.Controls;

/// <summary>
/// Describes how a presenter is arranged relative to a control's textual content.
/// </summary>
public enum PresenterMode
{
    /// <summary>The presenter and textual content occupy separate layout regions.</summary>
    Split,

    /// <summary>The presenter fills the control and the textual content is drawn over it.</summary>
    Overlay,
}
