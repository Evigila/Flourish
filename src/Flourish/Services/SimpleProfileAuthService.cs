using ArkheideSystem.Flourish.Abstract;

namespace ArkheideSystem.Flourish.Services;

internal sealed class SimpleProfileAuthService(
    FlourishLocalizationService localizationService
) : IProfileAuthService
{
    public Task<ProfileAuthenticationResult> AuthenticateAsync(
        ProfileSignInRequest request,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(request);
        cancellationToken.ThrowIfCancellationRequested();

        if (string.IsNullOrWhiteSpace(request.DisplayName))
        {
            return Task.FromResult(
                ProfileAuthenticationResult.Failure(
                    localizationService.Get(FlourishLocaleKeys.ProfileEnterName)
                )
            );
        }

        if (string.IsNullOrWhiteSpace(request.Password))
        {
            return Task.FromResult(
                ProfileAuthenticationResult.Failure(
                    localizationService.Get(FlourishLocaleKeys.ProfileEnterPassword)
                )
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
