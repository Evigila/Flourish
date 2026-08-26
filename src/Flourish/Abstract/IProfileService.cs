using System;
using System.Threading;
using System.Threading.Tasks;

namespace ArkheideSystem.Flourish.Abstract;

/// <summary>
/// Maintains the active Flourish profile and its login state.
/// </summary>
public interface IProfileService
{
    /// <summary>
    /// Gets the current immutable profile state.
    /// </summary>
    FlourishProfileState Current { get; }

    /// <summary>
    /// Occurs when the profile or login state changes.
    /// </summary>
    event EventHandler<FlourishStateChangedEventArgs<FlourishProfileState>>? Changed;

    /// <summary>
    /// Authenticates and activates a profile for the current session.
    /// </summary>
    /// <param name="request">The profile sign-in request.</param>
    /// <param name="cancellationToken">A token that requests cancellation.</param>
    /// <returns>The authentication outcome.</returns>
    Task<ProfileAuthenticationResult> SignInAsync(
        ProfileSignInRequest request,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Changes whether the active login should be restored on the next startup.
    /// </summary>
    /// <param name="rememberLogin">Whether to restore the active login on the next startup.</param>
    /// <param name="cancellationToken">A token that requests cancellation.</param>
    /// <returns>A task that completes when the setting is applied.</returns>
    Task SetRememberLoginAsync(
        bool rememberLogin,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Changes the name order used by the default and active profiles.
    /// </summary>
    /// <param name="nameOrder">The order used to display profile names and initials.</param>
    /// <param name="cancellationToken">A token that requests cancellation.</param>
    /// <returns>A task that completes when the setting is applied.</returns>
    Task SetNameOrderAsync(
        NameOrder nameOrder,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Signs out and removes the persisted profile credentials.
    /// </summary>
    /// <param name="cancellationToken">A token that requests cancellation.</param>
    /// <returns>A task that completes when sign-out finishes.</returns>
    Task SignOutAsync(CancellationToken cancellationToken = default);
}
