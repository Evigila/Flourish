using System.Windows.Input;

namespace ArkheideSystem.Flourish.Abstract.Essential;

/// <summary>
/// Registers, resolves, and dispatches keyboard shortcuts while an application is running.
/// </summary>
public interface IShortcutService
{
    /// <summary>
    /// Gets immutable snapshots of all active shortcut registrations.
    /// </summary>
    IReadOnlyList<ShortcutRegistrationInfo> Registrations { get; }

    /// <summary>
    /// Occurs after the shortcut registration collection changes.
    /// </summary>
    event EventHandler<ShortcutRegistryChangedEventArgs>? Changed;

    /// <summary>
    /// Registers a key gesture that dispatches a command key.
    /// </summary>
    /// <param name="gesture">The key gesture to register.</param>
    /// <param name="commandKey">The stable command key to dispatch.</param>
    /// <param name="parameter">An optional command parameter.</param>
    /// <param name="options">Optional scope, conflict, and priority behavior.</param>
    /// <returns>A lease that unregisters only this shortcut when disposed.</returns>
    IShortcutRegistration Register(
        KeyGesture gesture,
        string commandKey,
        object? parameter = null,
        ShortcutRegistrationOptions? options = null
    );

    /// <summary>
    /// Resolves the highest-precedence shortcut for a gesture and active context.
    /// </summary>
    /// <param name="gesture">The key gesture to resolve.</param>
    /// <param name="context">The optional active window and page context.</param>
    /// <param name="registration">The resolved shortcut snapshot, or <see langword="null" />.</param>
    /// <returns><see langword="true" /> when a shortcut was resolved; otherwise, <see langword="false" />.</returns>
    bool TryResolve(
        KeyGesture gesture,
        ShortcutResolutionContext? context,
        out ShortcutRegistrationInfo? registration
    );

    /// <summary>
    /// Resolves and dispatches a keyboard shortcut.
    /// </summary>
    /// <param name="gesture">The key gesture to execute.</param>
    /// <param name="context">The optional active window and page context.</param>
    /// <param name="cancellationToken">A token that requests cancellation.</param>
    /// <returns>The captured command outcome, or a not-handled result when no shortcut matches.</returns>
    ValueTask<CommandResult> ExecuteAsync(
        KeyGesture gesture,
        ShortcutResolutionContext? context = null,
        CancellationToken cancellationToken = default
    );
}
