namespace ArkheideSystem.Flourish.Abstract;

/// <summary>
/// Identifies a Flourish shell feature.
/// </summary>
public enum ShellFeature
{
    /// <summary>
    /// The complete title bar surface.
    /// </summary>
    TitleBar,

    /// <summary>
    /// The navigation panel and its items.
    /// </summary>
    Navigation,

    /// <summary>
    /// The contextual dynamic toolbar.
    /// </summary>
    DynamicToolbar,

    /// <summary>
    /// The status bar content surface.
    /// </summary>
    StatusContent,

    /// <summary>
    /// Shell tooltips and their presentation.
    /// </summary>
    ToolTips,

    /// <summary>
    /// Shell transitions and other motion effects.
    /// </summary>
    Motion,

    /// <summary>
    /// The profile entry point and flyout.
    /// </summary>
    Profile,
}
