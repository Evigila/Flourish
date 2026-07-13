namespace ArkheideSystem.Flourish.Abstract;

/// <summary>
/// Authenticates Flourish profile sign-in requests.
/// </summary>
public interface IProfileAuthService
{
    /// <summary>
    /// Authenticates the supplied credentials.
    /// </summary>
    /// <param name="request">The profile sign-in request.</param>
    /// <param name="cancellationToken">A token that requests cancellation.</param>
    /// <returns>The authentication outcome.</returns>
    Task<ProfileAuthenticationResult> AuthenticateAsync(
        ProfileSignInRequest request,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Completes any provider-specific sign-out work.
    /// </summary>
    /// <param name="profile">The profile being signed out.</param>
    /// <param name="cancellationToken">A token that requests cancellation.</param>
    /// <returns>A task that completes when sign-out work finishes.</returns>
    Task SignOutAsync(
        ProfileUser profile,
        CancellationToken cancellationToken = default
    );
}
