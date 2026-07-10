using ArkheideSystem.Flourish.Abstract;

namespace ArkheideSystem.Flourish.Services;

internal sealed class SimpleProfileAuthService : IProfileAuthService
{
    public Task<ProfileAuthenticationResult> AuthenticateAsync(
        ProfileSignInRequest request,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(request);
        cancellationToken.ThrowIfCancellationRequested();

        if (string.IsNullOrWhiteSpace(request.UserName))
        {
            return Task.FromResult(
                ProfileAuthenticationResult.Failure("Enter a user name.")
            );
        }

        if (string.IsNullOrWhiteSpace(request.Password))
        {
            return Task.FromResult(
                ProfileAuthenticationResult.Failure("Enter a password.")
            );
        }

        return Task.FromResult(ProfileAuthenticationResult.Success());
    }

    public Task SignOutAsync(
        ProfileUser profile,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(profile);
        cancellationToken.ThrowIfCancellationRequested();
        return Task.CompletedTask;
    }
}
