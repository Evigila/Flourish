namespace ArkheideSystem.Flourish.Abstract.Essential;

/// <summary>
/// Owns the lifetime of a runtime command registration.
/// </summary>
public interface ICommandRegistration : IDisposable
{
    /// <summary>
    /// Gets the unique registration identifier.
    /// </summary>
    Guid Id { get; }

    /// <summary>
    /// Gets the registered command key.
    /// </summary>
    string CommandKey { get; }

    /// <summary>
    /// Gets a value indicating whether this registration is still active.
    /// </summary>
    bool IsRegistered { get; }

    /// <summary>
    /// Notifies listeners that this registration's availability should be queried again.
    /// </summary>
    void NotifyCanExecuteChanged();
}
