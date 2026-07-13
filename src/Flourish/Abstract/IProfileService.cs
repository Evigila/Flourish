namespace ArkheideSystem.Flourish.Abstract;

/// <summary>
/// Maintains the active Flourish profile and its login state.
/// </summary>
public interface IProfileService
{
    /// <summary>
    /// Gets the profile currently displayed by the shell.
    /// </summary>
    ProfileUser CurrentProfile { get; }

    /// <summary>
    /// Gets the current login state.
    /// </summary>
    ProfileLoginState LoginState { get; }

    /// <summary>
    /// Occurs when the profile or login state changes.
    /// </summary>
    event EventHandler<ProfileChangedEventArgs>? ProfileChanged;

    /// <summary>
    /// Initializes profile state and restores a remembered login when stored credentials are available.
    /// </summary>
    /// <param name="cancellationToken">A token that requests cancellation.</param>
    /// <returns>A task that completes when initialization finishes.</returns>
    Task InitializeAsync(CancellationToken cancellationToken = default);

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
    /// Signs out and removes the persisted profile credentials.
    /// </summary>
    /// <param name="cancellationToken">A token that requests cancellation.</param>
    /// <returns>A task that completes when sign-out finishes.</returns>
    Task SignOutAsync(CancellationToken cancellationToken = default);
}
