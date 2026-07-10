namespace ArkheideSystem.Flourish.Abstract;

/// <summary>
/// Authenticates Flourish profile credentials. Register a custom implementation before
/// Flourish builds to replace the built-in non-empty credential check.
/// </summary>
public interface IProfileAuthService
{
    /// <summary>
    /// Authenticates the supplied credentials.
    /// </summary>
    Task<ProfileAuthenticationResult> AuthenticateAsync(
        ProfileSignInRequest request,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Completes any provider-specific sign-out work.
    /// </summary>
    Task SignOutAsync(
        ProfileUser profile,
        CancellationToken cancellationToken = default
    );
}
