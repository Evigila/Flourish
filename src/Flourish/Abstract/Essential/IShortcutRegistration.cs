namespace ArkheideSystem.Flourish.Abstract.Essential;

/// <summary>
/// Owns the lifetime of a keyboard shortcut registration.
/// </summary>
public interface IShortcutRegistration : IDisposable
{
    /// <summary>
    /// Gets the unique registration identifier.
    /// </summary>
    Guid Id { get; }

    /// <summary>
    /// Gets a value indicating whether this shortcut remains registered.
    /// </summary>
    bool IsRegistered { get; }
}
