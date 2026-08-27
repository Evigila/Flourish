using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;
using ArkheideSystem.Flourish.Abstract;
using ArkheideSystem.Flourish.Configuration;
using ArkheideSystem.Flourish.Localization;
using ArkheideSystem.Flourish.Profile;
using ArkheideSystem.Flourish.Test.Infrastructure;

using System.IO;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;

namespace ArkheideSystem.Flourish.Test.Services;

public sealed class ProfileServiceTests
{
    [Fact]
    public async Task SetNameOrderAsync_UpdatesSignedOutDefaultProfileAndRaisesChange()
    {
        var localization = new LocalizationService(new ApplicationDataOptions());
        var sut = new ProfileService(
            new SimpleProfileAuthService(localization),
            new ProfileSecretStore(new ConfigurationBuilder().Build()),
            new ProfileOptions
            {
                DefaultFirstName = "Ada",
                DefaultLastName = "Lovelace",
                NameOrder = NameOrder.FirstLast,
            },
            localization
        );
        StateChangedEventArgs<ProfileState>? change = null;
        sut.Changed += (_, eventArgs) => change = eventArgs;

        await sut.SetNameOrderAsync(NameOrder.LastFirst);

        Assert.Equal(NameOrder.LastFirst, sut.Current.NameOrder);
        Assert.Equal(ProfileLoginState.SignedOut, sut.Current.LoginState);
        Assert.Equal("Lovelace Ada", sut.Current.Profile.DisplayName);
        Assert.Equal("LA", sut.Current.Profile.Initials);
        Assert.NotNull(change);
        Assert.Same(sut.Current.Profile, change.Current.Profile);
        Assert.Equal(ProfileLoginState.SignedOut, change.Current.LoginState);
    }

    [Fact]
    public async Task SetNameOrderAsync_UpdatesSignedInProfileWithoutChangingSessionData()
    {
        var localization = new LocalizationService(new ApplicationDataOptions());
        var sut = new ProfileService(
            new SimpleProfileAuthService(localization),
            new ProfileSecretStore(new ConfigurationBuilder().Build()),
            new ProfileOptions(),
            localization
        );
        await sut.SignInAsync(
            new ProfileSignInRequest("Ada", "Lovelace", "secret", NameOrder.FirstLast, "avatar.png")
        );
        var changes = new List<StateChangedEventArgs<ProfileState>>();
        sut.Changed += (_, eventArgs) => changes.Add(eventArgs);

        await sut.SetNameOrderAsync(NameOrder.LastFirst);

        Assert.Equal(NameOrder.LastFirst, sut.Current.NameOrder);
        Assert.Equal(ProfileLoginState.SignedIn, sut.Current.LoginState);
        Assert.Equal("Ada", sut.Current.Profile.FirstName);
        Assert.Equal("Lovelace", sut.Current.Profile.LastName);
        Assert.Equal("avatar.png", sut.Current.Profile.ImagePath);
        Assert.Equal("Lovelace Ada", sut.Current.Profile.DisplayName);
        var change = Assert.Single(changes);
        Assert.Same(sut.Current.Profile, change.Current.Profile);
        Assert.Equal(ProfileLoginState.SignedIn, change.Current.LoginState);

        await sut.SetRememberLoginAsync(rememberLogin: false);
        Assert.Equal(ProfileLoginState.SignedIn, sut.Current.LoginState);
    }

    [Fact]
    public async Task SetNameOrderAsync_WithCurrentValue_DoesNotRaiseChange()
    {
        var localization = new LocalizationService(new ApplicationDataOptions());
        var sut = new ProfileService(
            new SimpleProfileAuthService(localization),
            new ProfileSecretStore(new ConfigurationBuilder().Build()),
            new ProfileOptions { NameOrder = NameOrder.LastFirst },
            localization
        );
        var changeCount = 0;
        sut.Changed += (_, _) => changeCount++;

        await sut.SetNameOrderAsync(NameOrder.LastFirst);

        Assert.Equal(0, changeCount);
        Assert.Equal(NameOrder.LastFirst, sut.Current.NameOrder);
    }

    [Fact]
    public async Task SetNameOrderAsync_WithUnknownValue_PreservesCurrentState()
    {
        var localization = new LocalizationService(new ApplicationDataOptions());
        var sut = new ProfileService(
            new SimpleProfileAuthService(localization),
            new ProfileSecretStore(new ConfigurationBuilder().Build()),
            new ProfileOptions(),
            localization
        );
        var originalProfile = sut.Current.Profile;

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
            sut.SetNameOrderAsync((NameOrder)42)
        );

        Assert.Equal(NameOrder.FirstLast, sut.Current.NameOrder);
        Assert.Same(originalProfile, sut.Current.Profile);
    }

    [Fact]
    public async Task SignInWithoutUserSecrets_RemainsInMemoryAndRememberFailsTransactionally()
    {
        var localization = new LocalizationService(new ApplicationDataOptions());
        var secretStore = new ProfileSecretStore(new ConfigurationBuilder().Build());
        var sut = new ProfileService(
            new SimpleProfileAuthService(localization),
            secretStore,
            new ProfileOptions(),
            localization
        );

        var result = await sut.SignInAsync(
            new ProfileSignInRequest("Ada", "Lovelace", "secret", NameOrder.FirstLast)
        );

        Assert.True(result.Succeeded);
        Assert.Equal(ProfileLoginState.SignedIn, sut.Current.LoginState);
        Assert.Equal("Ada Lovelace", sut.Current.Profile.DisplayName);

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            sut.SetRememberLoginAsync(rememberLogin: true)
        );

        Assert.Contains("<UserSecretsId>", exception.Message);
        Assert.Equal(ProfileLoginState.SignedIn, sut.Current.LoginState);
        Assert.Equal("Ada Lovelace", sut.Current.Profile.DisplayName);
    }

    [Theory]
    [InlineData(0, "Ada", "Lovelace")]
    [InlineData(StoredProfileCredentials.CurrentSchemaVersion, null, null)]
    public async Task RestoreAsync_WithUnsupportedOrNamelessCredentials_ClearsStoredValue(
        int schemaVersion,
        string? firstName,
        string? lastName
    )
    {
        using var directory = new TemporaryDirectory();
        var secretPath = Path.Combine(directory.Path, "secrets.json");
        await File.WriteAllTextAsync(secretPath, "{}");
        using var fileProvider = new PhysicalFileProvider(directory.Path);
        var configuration = new ConfigurationBuilder()
            .AddJsonFile(fileProvider, "secrets.json", optional: true, reloadOnChange: false)
            .Build();
        var secretStore = new ProfileSecretStore(configuration);
        await secretStore.SaveAsync(
            new StoredProfileCredentials
            {
                SchemaVersion = schemaVersion,
                FirstName = firstName,
                LastName = lastName,
                Password = "secret",
                RememberLogin = true,
            }
        );
        var localization = new LocalizationService(new ApplicationDataOptions());
        var sut = new ProfileService(
            new SimpleProfileAuthService(localization),
            secretStore,
            new ProfileOptions(),
            localization
        );

        await sut.RestoreAsync();

        Assert.Equal(ProfileLoginState.SignedOut, sut.Current.LoginState);
        Assert.Null(await secretStore.ReadAsync());
    }
}
