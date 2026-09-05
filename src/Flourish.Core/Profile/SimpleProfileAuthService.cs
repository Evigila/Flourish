using System;

using System.Threading;
using System.Threading.Tasks;

using ArkheideSystem.Flourish.Abstract;
using ArkheideSystem.Flourish.Localization;
namespace ArkheideSystem.Flourish.Profile;

internal sealed class SimpleProfileAuthService(ILocalizationService localizationService)
    : IProfileAuthService
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
                    localizationService.Get(LocaleKeys.ProfileEnterName)
                )
            );
        }

        if (string.IsNullOrWhiteSpace(request.Password))
        {
            return Task.FromResult(
                ProfileAuthenticationResult.Failure(
                    localizationService.Get(LocaleKeys.ProfileEnterPassword)
                )
            );
        }

        return Task.FromResult(ProfileAuthenticationResult.Success());
    }

    public Task SignOutAsync(ProfileUser profile, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(profile);
        cancellationToken.ThrowIfCancellationRequested();
        return Task.CompletedTask;
    }
}
