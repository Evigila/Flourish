namespace ArkheideSystem.Flourish.Abstract.Runtime;

/// <summary>
/// Coordinates shell close behavior and runtime close guards.
/// </summary>
public interface IWindowCloseService
{
    /// <summary>
    /// Gets the action the shell takes after all registered guards allow closing.
    /// </summary>
    WindowCloseBehavior Behavior { get; }

    /// <summary>
    /// Changes the action taken after all registered guards allow closing.
    /// </summary>
    /// <param name="behavior">The default close behavior.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="behavior"/> is not defined.</exception>
    void SetBehavior(WindowCloseBehavior behavior);

    /// <summary>
    /// Registers an ordered asynchronous guard that can veto close requests.
    /// </summary>
    /// <param name="id">A non-empty identifier unique among active guard registrations.</param>
    /// <param name="guard">The guard callback.</param>
    /// <param name="order">The evaluation order; lower values run first, then identifiers are compared ordinally.</param>
    /// <returns>A lease that unregisters the guard when disposed.</returns>
    /// <exception cref="ArgumentException"><paramref name="id"/> is empty or whitespace.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="guard"/> is <see langword="null"/>.</exception>
    /// <exception cref="InvalidOperationException">An active guard already uses <paramref name="id"/>.</exception>
    IWindowCloseGuardRegistration RegisterGuard(
        string id,
        Func<WindowCloseContext, CancellationToken, ValueTask<WindowCloseDecision>> guard,
        int order = 0
    );

    /// <summary>
    /// Evaluates a snapshot of the registered guards until one cancels the request.
    /// </summary>
    /// <param name="reason">The source of the close request.</param>
    /// <param name="cancellationToken">A token that cancels evaluation.</param>
    /// <returns><see langword="true"/> when every guard allows closing; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="reason"/> is not defined.</exception>
    /// <exception cref="OperationCanceledException"><paramref name="cancellationToken"/> is canceled.</exception>
    ValueTask<bool> CanCloseAsync(
        WindowCloseRequestReason reason,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Asks the attached shell to process a close request, including guards and the selected behavior.
    /// </summary>
    /// <param name="reason">The source of the close request.</param>
    /// <param name="cancellationToken">A token that cancels close processing.</param>
    /// <returns><see langword="true"/> when the attached shell handled the request; <see langword="false"/> when no shell is attached or it declined the request.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="reason"/> is not defined.</exception>
    /// <exception cref="OperationCanceledException"><paramref name="cancellationToken"/> is canceled.</exception>
    ValueTask<bool> RequestCloseAsync(
        WindowCloseRequestReason reason = WindowCloseRequestReason.Application,
        CancellationToken cancellationToken = default
    );
}
