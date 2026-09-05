using System;
using System.Threading;
using System.Threading.Tasks;

namespace ArkheideSystem.Flourish.Abstract;

/// <summary>Specifies the default behavior of an application-shell close request.</summary>
public enum WindowCloseBehavior
{
    /// <summary>Uses the shell's normal confirmation flow before exiting.</summary>
    Prompt,

    /// <summary>Closes the application without the standard confirmation prompt after guards allow it.</summary>
    Close,

    /// <summary>Hides the shell window in the notification area instead of exiting.</summary>
    MinimizeToTray,
}

/// <summary>Identifies the source of an application-shell close request.</summary>
public enum WindowCloseRequestReason
{
    /// <summary>The request originated from a title-bar command.</summary>
    TitleBar,

    /// <summary>The request originated from the native window close operation.</summary>
    Window,

    /// <summary>The request originated from the notification-area menu.</summary>
    Tray,

    /// <summary>The request originated from application code.</summary>
    Application,
}

/// <summary>Represents the result returned by a runtime close guard.</summary>
public enum WindowCloseDecision
{
    /// <summary>Allows close evaluation to continue to the next guard.</summary>
    Allow,

    /// <summary>Stops close evaluation and keeps the application open.</summary>
    Cancel,
}

/// <summary>Describes an application-shell close request passed to runtime close guards.</summary>
/// <param name="reason">The source of the close request.</param>
/// <param name="services">The application's service provider.</param>
public sealed class WindowCloseContext(
    WindowCloseRequestReason reason,
    IServiceProvider services
)
{
    /// <summary>Gets the source of the close request.</summary>
    public WindowCloseRequestReason Reason { get; } = reason;

    /// <summary>Gets the application service provider for resolving guard dependencies.</summary>
    public IServiceProvider Services { get; } = services;
}

/// <summary>Represents the active application-shell close policy.</summary>
public sealed record WindowCloseState(WindowCloseBehavior Behavior);

/// <summary>Coordinates application-shell close behavior and runtime close guards.</summary>
public interface IWindowCloseService
{
    /// <summary>Gets an immutable snapshot of the active close policy.</summary>
    WindowCloseState Current { get; }

    /// <summary>Occurs after the active close policy changes.</summary>
    event EventHandler<StateChangedEventArgs<WindowCloseState>>? Changed;

    /// <summary>Changes the action taken after all registered guards allow closing.</summary>
    /// <param name="behavior">The default close behavior.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="behavior"/> is not defined.</exception>
    void SetBehavior(WindowCloseBehavior behavior);

    /// <summary>Registers an ordered asynchronous guard that can veto close requests.</summary>
    /// <param name="id">A non-empty identifier unique among active guard registrations.</param>
    /// <param name="guard">The guard callback.</param>
    /// <param name="order">The evaluation order; lower values run first, then identifiers are compared ordinally.</param>
    /// <returns>A lease that unregisters the guard when disposed.</returns>
    /// <exception cref="ArgumentException"><paramref name="id"/> is empty or whitespace.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="guard"/> is <see langword="null"/>.</exception>
    /// <exception cref="InvalidOperationException">An active guard already uses <paramref name="id"/>.</exception>
    IRegistration RegisterGuard(
        string id,
        Func<WindowCloseContext, CancellationToken, ValueTask<WindowCloseDecision>> guard,
        int order = 0
    );

    /// <summary>Evaluates a snapshot of the registered guards until one cancels the request.</summary>
    /// <param name="reason">The source of the close request.</param>
    /// <param name="cancellationToken">A token that cancels evaluation.</param>
    /// <returns><see langword="true"/> when every guard allows closing; otherwise, <see langword="false"/>.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="reason"/> is not defined.</exception>
    /// <exception cref="OperationCanceledException"><paramref name="cancellationToken"/> is canceled.</exception>
    ValueTask<bool> CanCloseAsync(
        WindowCloseRequestReason reason,
        CancellationToken cancellationToken = default
    );

    /// <summary>Asks the attached platform shell to process a close request.</summary>
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
