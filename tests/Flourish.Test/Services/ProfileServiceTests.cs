using ArkheideSystem.Flourish.Abstract;
using ArkheideSystem.Flourish.Internal.Configuration;
using ArkheideSystem.Flourish.Services;
using Microsoft.Extensions.Configuration;

namespace ArkheideSystem.Flourish.Test.Services;

public sealed class ProfileServiceTests
{
    [Fact]
    public async Task SignInWithoutUserSecrets_RemainsInMemoryAndRememberFailsTransactionally()
    {
        var localization = new FlourishLocalizationService(new FlourishDataOptions());
        var secretStore = new ProfileSecretStore(
            new ConfigurationBuilder().Build()
        );
        var sut = new ProfileService(
            new SimpleProfileAuthService(localization),
            secretStore,
            new FlourishProfileOptions(),
            localization
        );

        var result = await sut.SignInAsync(
            new ProfileSignInRequest(
                "Ada",
                "Lovelace",
                "secret",
                NameOrder.FirstLast
            )
        );

        Assert.True(result.Succeeded);
        Assert.Equal(ProfileLoginState.SignedIn, sut.LoginState);
        Assert.Equal("Ada Lovelace", sut.CurrentProfile.DisplayName);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            sut.SetRememberLoginAsync(rememberLogin: true)
        );

        Assert.Contains("<UserSecretsId>", exception.Message);
        Assert.Equal(ProfileLoginState.SignedIn, sut.LoginState);
        Assert.Equal("Ada Lovelace", sut.CurrentProfile.DisplayName);
    }
}
