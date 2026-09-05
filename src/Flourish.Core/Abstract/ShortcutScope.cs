namespace ArkheideSystem.Flourish.Abstract;

/// <summary>
/// Identifies the lifetime and resolution scope of a keyboard shortcut.
/// </summary>
public enum ShortcutScope
{
    /// <summary>
    /// The shortcut is eligible throughout the application.
    /// </summary>
    Application = 0,

    /// <summary>
    /// The shortcut is eligible for a matching active window.
    /// </summary>
    Window,

    /// <summary>
    /// The shortcut is eligible for a matching active page.
    /// </summary>
    Page,
}
