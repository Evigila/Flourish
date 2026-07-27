using System.IO;
using ArkheideSystem.Flourish.Abstract;
using ArkheideSystem.Flourish.Internal.Configuration;
using ArkheideSystem.Flourish.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;

namespace ArkheideSystem.Flourish.Test.Services;

public sealed class ProfileServiceTests
{
    [Fact]
    public async Task SetNameOrderAsync_UpdatesSignedOutDefaultProfileAndRaisesChange()
    {
        var localization = new FlourishLocalizationService(new FlourishDataOptions());
        var sut = new ProfileService(
            new SimpleProfileAuthService(localization),
            new ProfileSecretStore(new ConfigurationBuilder().Build()),
            new FlourishProfileOptions
            {
                DefaultFirstName = "Ada",
                DefaultLastName = "Lovelace",
                NameOrder = NameOrder.FirstLast,
            },
            localization
        );
        ProfileChangedEventArgs? change = null;
        sut.ProfileChanged += (_, eventArgs) => change = eventArgs;

        await sut.SetNameOrderAsync(NameOrder.LastFirst);

        Assert.Equal(NameOrder.LastFirst, sut.NameOrder);
        Assert.Equal(ProfileLoginState.SignedOut, sut.LoginState);
        Assert.Equal("Lovelace Ada", sut.CurrentProfile.DisplayName);
        Assert.Equal("LA", sut.CurrentProfile.Initials);
        Assert.NotNull(change);
        Assert.Same(sut.CurrentProfile, change.Profile);
        Assert.Equal(ProfileLoginState.SignedOut, change.LoginState);
    }

    [Fact]
    public async Task SetNameOrderAsync_UpdatesSignedInProfileWithoutChangingSessionData()
    {
        var localization = new FlourishLocalizationService(new FlourishDataOptions());
        var sut = new ProfileService(
            new SimpleProfileAuthService(localization),
            new ProfileSecretStore(new ConfigurationBuilder().Build()),
            new FlourishProfileOptions(),
            localization
        );
        await sut.SignInAsync(
            new ProfileSignInRequest(
                "Ada",
                "Lovelace",
                "secret",
                NameOrder.FirstLast,
                "avatar.png"
            )
        );
        var changes = new List<ProfileChangedEventArgs>();
        sut.ProfileChanged += (_, eventArgs) => changes.Add(eventArgs);

        await sut.SetNameOrderAsync(NameOrder.LastFirst);

        Assert.Equal(NameOrder.LastFirst, sut.NameOrder);
        Assert.Equal(ProfileLoginState.SignedIn, sut.LoginState);
        Assert.Equal("Ada", sut.CurrentProfile.FirstName);
        Assert.Equal("Lovelace", sut.CurrentProfile.LastName);
        Assert.Equal("avatar.png", sut.CurrentProfile.ImagePath);
        Assert.Equal("Lovelace Ada", sut.CurrentProfile.DisplayName);
        var change = Assert.Single(changes);
        Assert.Same(sut.CurrentProfile, change.Profile);
        Assert.Equal(ProfileLoginState.SignedIn, change.LoginState);

        await sut.SetRememberLoginAsync(rememberLogin: false);
        Assert.Equal(ProfileLoginState.SignedIn, sut.LoginState);
    }

    [Fact]
    public async Task SetNameOrderAsync_WithCurrentValue_DoesNotRaiseChange()
    {
        var localization = new FlourishLocalizationService(new FlourishDataOptions());
        var sut = new ProfileService(
            new SimpleProfileAuthService(localization),
            new ProfileSecretStore(new ConfigurationBuilder().Build()),
            new FlourishProfileOptions { NameOrder = NameOrder.LastFirst },
            localization
        );
        var changeCount = 0;
        sut.ProfileChanged += (_, _) => changeCount++;

        await sut.SetNameOrderAsync(NameOrder.LastFirst);

        Assert.Equal(0, changeCount);
        Assert.Equal(NameOrder.LastFirst, sut.NameOrder);
    }

    [Fact]
    public async Task SetNameOrderAsync_WithUnknownValue_PreservesCurrentState()
    {
        var localization = new FlourishLocalizationService(new FlourishDataOptions());
        var sut = new ProfileService(
            new SimpleProfileAuthService(localization),
            new ProfileSecretStore(new ConfigurationBuilder().Build()),
            new FlourishProfileOptions(),
            localization
        );
        var originalProfile = sut.CurrentProfile;

        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
            sut.SetNameOrderAsync((NameOrder)42)
        );

        Assert.Equal(NameOrder.FirstLast, sut.NameOrder);
        Assert.Same(originalProfile, sut.CurrentProfile);
    }

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

    [Theory]
    [InlineData(0, "Ada", "Lovelace")]
    [InlineData(StoredProfileCredentials.CurrentSchemaVersion, null, null)]
    public async Task InitializeAsync_WithUnsupportedOrNamelessCredentials_ClearsStoredValue(
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
            .AddJsonFile(
                fileProvider,
                "secrets.json",
                optional: true,
                reloadOnChange: false
            )
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
        var localization = new FlourishLocalizationService(new FlourishDataOptions());
        var sut = new ProfileService(
            new SimpleProfileAuthService(localization),
            secretStore,
            new FlourishProfileOptions(),
            localization
        );

        await sut.InitializeAsync();

        Assert.Equal(ProfileLoginState.SignedOut, sut.LoginState);
        Assert.Null(await secretStore.ReadAsync());
    }

    private sealed class TemporaryDirectory : IDisposable
    {
        public TemporaryDirectory()
        {
            Path = System.IO.Path.Combine(
                System.IO.Path.GetTempPath(),
                "Flourish.Test",
                Guid.NewGuid().ToString("N")
            );
            Directory.CreateDirectory(Path);
        }

        public string Path { get; }

        public void Dispose()
        {
            if (Directory.Exists(Path))
            {
                Directory.Delete(Path, recursive: true);
            }
        }
    }
}
