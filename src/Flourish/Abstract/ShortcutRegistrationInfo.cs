using System.Windows.Input;

namespace ArkheideSystem.Flourish.Abstract;

/// <summary>
/// Provides an immutable snapshot of a registered keyboard shortcut.
/// </summary>
public sealed class ShortcutRegistrationInfo
{
    internal ShortcutRegistrationInfo(
        Guid id,
        KeyGesture gesture,
        string commandKey,
        object? parameter,
        ShortcutScope scope,
        string? scopeKey,
        int priority,
        bool allowWhenTextInputFocused
    )
    {
        Id = id;
        Gesture = gesture;
        CommandKey = commandKey;
        Parameter = parameter;
        Scope = scope;
        ScopeKey = scopeKey;
        Priority = priority;
        AllowWhenTextInputFocused = allowWhenTextInputFocused;
    }

    /// <summary>
    /// Gets the unique shortcut registration identifier.
    /// </summary>
    public Guid Id { get; }

    /// <summary>
    /// Gets the registered key gesture.
    /// </summary>
    public KeyGesture Gesture { get; }

    /// <summary>
    /// Gets the command key dispatched by the shortcut.
    /// </summary>
    public string CommandKey { get; }

    /// <summary>
    /// Gets the optional command parameter.
    /// </summary>
    public object? Parameter { get; }

    /// <summary>
    /// Gets the shortcut scope.
    /// </summary>
    public ShortcutScope Scope { get; }

    /// <summary>
    /// Gets the optional matching window or page key.
    /// </summary>
    public string? ScopeKey { get; }

    /// <summary>
    /// Gets the shortcut priority.
    /// </summary>
    public int Priority { get; }

    /// <summary>
    /// Gets whether the shortcut can execute while a text input control has keyboard focus.
    /// </summary>
    public bool AllowWhenTextInputFocused { get; }
}
