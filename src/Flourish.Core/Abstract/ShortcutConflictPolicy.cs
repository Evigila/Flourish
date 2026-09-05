namespace ArkheideSystem.Flourish.Abstract;

/// <summary>
/// Specifies how shortcut registration handles an identical gesture in the same scope.
/// </summary>
public enum ShortcutConflictPolicy
{
    /// <summary>
    /// Reject the new shortcut by throwing an exception.
    /// </summary>
    Reject = 0,

    /// <summary>
    /// Remove conflicting shortcuts before registering the new shortcut.
    /// </summary>
    Replace,

    /// <summary>
    /// Keep conflicting shortcuts and resolve them by priority and registration order.
    /// </summary>
    Append,
}
