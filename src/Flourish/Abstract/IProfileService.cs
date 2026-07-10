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
    /// Restores a remembered login, or removes credentials from a previous unremembered login.
    /// </summary>
    Task InitializeAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Authenticates and activates a profile for the current session.
    /// </summary>
    Task<ProfileAuthenticationResult> SignInAsync(
        ProfileSignInRequest request,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Changes whether the active login should be restored on the next startup.
    /// </summary>
    Task SetRememberLoginAsync(
        bool rememberLogin,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Signs out and removes the persisted profile credentials.
    /// </summary>
    Task SignOutAsync(CancellationToken cancellationToken = default);
}
