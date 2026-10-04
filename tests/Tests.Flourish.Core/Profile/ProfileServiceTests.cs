using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

using ArkheideSystem.Flourish.Abstract;
using ArkheideSystem.Flourish.Configuration;
using ArkheideSystem.Flourish.Localization;
using ArkheideSystem.Flourish.Profile;

using Xunit;

namespace ArkheideSystem.Tests.Flourish.Core.Profile;

public sealed class ProfileServiceTests
{
    [Fact]
    public async Task SetNameOrderAsync_UpdatesSignedOutDefaultAndSuppressesNoOp()
    {
        var sut = CreateService(
            options: new ProfileOptions
            {
                DefaultFirstName = "Ada",
                DefaultLastName = "Lovelace",
                NameOrder = NameOrder.FirstLast,
            }
        );
        var changes = new List<ProfileState>();
        sut.Changed += (_, args) => changes.Add(args.Current);

        await sut.SetNameOrderAsync(NameOrder.LastFirst);
        await sut.SetNameOrderAsync(NameOrder.LastFirst);

        Assert.Equal(ProfileLoginState.SignedOut, sut.Current.LoginState);
        Assert.Equal("Lovelace Ada", sut.Current.Profile.DisplayName);
        Assert.Equal("LA", sut.Current.Profile.Initials);
        Assert.Single(changes);
        await Assert.ThrowsAsync<ArgumentOutOfRangeException>(() =>
            sut.SetNameOrderAsync((NameOrder)42)
        );
    }

    [Fact]
    public async Task SignInAsync_NormalizesRequestAndPublishesSignedInState()
    {
        var auth = new RecordingAuthService();
        var sut = CreateService(auth);
        var changes = new List<ProfileState>();
        sut.Changed += (_, args) => changes.Add(args.Current);

        var result = await sut.SignInAsync(
            new ProfileSignInRequest(
                "  Ada ",
                " Lovelace  ",
                "secret",
                NameOrder.LastFirst,
                " avatar.png "
            )
        );

        Assert.True(result.Succeeded);
        Assert.NotNull(auth.LastRequest);
        Assert.Equal("Ada", auth.LastRequest.FirstName);
        Assert.Equal("Lovelace", auth.LastRequest.LastName);
        Assert.Equal(NameOrder.FirstLast, auth.LastRequest.NameOrder);
        Assert.Equal("avatar.png", auth.LastRequest.ImagePath);
        Assert.Equal(ProfileLoginState.SignedIn, sut.Current.LoginState);
        Assert.Equal("Ada Lovelace", sut.Current.Profile.DisplayName);
        Assert.Single(changes);
    }

    [Fact]
    public async Task SignInAsync_AuthenticationFailurePreservesSignedOutState()
    {
        var auth = new RecordingAuthService
        {
            AuthenticationResult = ProfileAuthenticationResult.Failure("denied"),
        };
        var store = new RecordingCredentialStore();
        var sut = CreateService(auth, store);

        var result = await sut.SignInAsync(
            new ProfileSignInRequest("Ada", "", "secret", NameOrder.FirstLast)
        );

        Assert.False(result.Succeeded);
        Assert.Equal(ProfileLoginState.SignedOut, sut.Current.LoginState);
        Assert.Equal(0, store.SaveCount);
    }

    [Fact]
    public async Task RememberLogin_SavesAndClearsThroughCredentialPort()
    {
        var store = new RecordingCredentialStore();
        var sut = CreateService(store: store);
        await sut.SignInAsync(
            new ProfileSignInRequest("Ada", "Lovelace", "secret", NameOrder.FirstLast)
        );

        await sut.SetRememberLoginAsync(true);

        Assert.Equal(ProfileLoginState.SignedInRemembered, sut.Current.LoginState);
        Assert.Equal(1, store.SaveCount);
        Assert.NotNull(store.Stored);
        Assert.True(store.Stored.RememberLogin);
        Assert.Equal("secret", store.Stored.Password);

        await sut.SetRememberLoginAsync(false);

        Assert.Equal(ProfileLoginState.SignedIn, sut.Current.LoginState);
        Assert.Equal(1, store.ClearCount);
        Assert.Null(store.Stored);
    }

    [Fact]
    public async Task RememberLogin_SaveFailureDoesNotPublishRememberedState()
    {
        var store = new RecordingCredentialStore
        {
            SaveException = new InvalidOperationException("storage unavailable"),
        };
        var sut = CreateService(store: store);
        await sut.SignInAsync(
            new ProfileSignInRequest("Ada", "Lovelace", "secret", NameOrder.FirstLast)
        );

        var error = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            sut.SetRememberLoginAsync(true)
        );

        Assert.Equal("storage unavailable", error.Message);
        Assert.Equal(ProfileLoginState.SignedIn, sut.Current.LoginState);
    }

    [Fact]
    public async Task RememberLogin_WhileSignedOutUsesLocalizedFailure()
    {
        var sut = CreateService();

        var error = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            sut.SetRememberLoginAsync(true)
        );

        Assert.Equal("Sign in to change this option.", error.Message);
    }

    [Fact]
    public async Task RestoreAsync_AuthenticatesRememberedCredentialsOnlyOnce()
    {
        var store = new RecordingCredentialStore
        {
            Stored = StoredProfileCredentials.Create(
                "Ada",
                "Lovelace",
                "secret",
                "avatar.png",
                rememberLogin: true
            ),
        };
        var auth = new RecordingAuthService();
        var sut = CreateService(auth, store);

        await sut.RestoreAsync();
        await sut.RestoreAsync();

        Assert.Equal(1, store.ReadCount);
        Assert.Equal(1, auth.AuthenticateCount);
        Assert.Equal(ProfileLoginState.SignedInRemembered, sut.Current.LoginState);
        Assert.Equal("Ada Lovelace", sut.Current.Profile.DisplayName);
        Assert.Equal("avatar.png", sut.Current.Profile.ImagePath);
    }

    [Theory]
    [InlineData(false, StoredProfileCredentials.CurrentSchemaVersion, "Ada", "Lovelace")]
    [InlineData(true, 0, "Ada", "Lovelace")]
    [InlineData(true, StoredProfileCredentials.CurrentSchemaVersion, null, null)]
    public async Task RestoreAsync_InvalidCredentialsAreCleared(
        bool rememberLogin,
        int schemaVersion,
        string? firstName,
        string? lastName
    )
    {
        var store = new RecordingCredentialStore
        {
            Stored = new StoredProfileCredentials
            {
                SchemaVersion = schemaVersion,
                FirstName = firstName,
                LastName = lastName,
                Password = "secret",
                RememberLogin = rememberLogin,
            },
        };
        var auth = new RecordingAuthService();
        var sut = CreateService(auth, store);

        await sut.RestoreAsync();

        Assert.Equal(ProfileLoginState.SignedOut, sut.Current.LoginState);
        Assert.Null(store.Stored);
        Assert.Equal(1, store.ClearCount);
        Assert.Equal(0, auth.AuthenticateCount);
    }

    [Fact]
    public async Task RestoreAsync_AuthenticationFailureClearsCredentials()
    {
        var store = new RecordingCredentialStore
        {
            Stored = StoredProfileCredentials.Create(
                "Ada",
                "Lovelace",
                "wrong",
                null,
                rememberLogin: true
            ),
        };
        var auth = new RecordingAuthService
        {
            AuthenticationResult = ProfileAuthenticationResult.Failure("denied"),
        };
        var sut = CreateService(auth, store);

        await sut.RestoreAsync();

        Assert.Equal(ProfileLoginState.SignedOut, sut.Current.LoginState);
        Assert.Null(store.Stored);
        Assert.Equal(1, store.ClearCount);
    }

    [Fact]
    public async Task SignOut_ResetsAndClearsEvenWhenProviderSignOutFails()
    {
        var store = new RecordingCredentialStore();
        var auth = new RecordingAuthService();
        var sut = CreateService(auth, store);
        await sut.SignInAsync(
            new ProfileSignInRequest("Ada", "Lovelace", "secret", NameOrder.FirstLast)
        );
        await sut.SetRememberLoginAsync(true);
        auth.SignOutException = new InvalidOperationException("provider failed");

        var error = await Assert.ThrowsAsync<InvalidOperationException>(() => sut.SignOutAsync());

        Assert.Equal("provider failed", error.Message);
        Assert.Equal(ProfileLoginState.SignedOut, sut.Current.LoginState);
        Assert.Null(store.Stored);
        Assert.Equal(1, auth.SignOutCount);
    }

    [Fact]
    public async Task SetNameOrderAsync_UpdatesSignedInProfileWithoutLosingSessionData()
    {
        var sut = CreateService();
        await sut.SignInAsync(
            new ProfileSignInRequest(
                "Ada",
                "Lovelace",
                "secret",
                NameOrder.FirstLast,
                "avatar.png"
            )
        );

        await sut.SetNameOrderAsync(NameOrder.LastFirst);

        Assert.Equal(ProfileLoginState.SignedIn, sut.Current.LoginState);
        Assert.Equal("Lovelace Ada", sut.Current.Profile.DisplayName);
        Assert.Equal("avatar.png", sut.Current.Profile.ImagePath);
    }

    private static ProfileService CreateService(
        RecordingAuthService? auth = null,
        RecordingCredentialStore? store = null,
        ProfileOptions? options = null
    )
    {
        var localization = new LocalizationService(
            new ApplicationDataOptions(),
            Path.Combine(Path.GetTempPath(), $"Flourish.Core.Profile-{Guid.NewGuid():N}")
        );
        return new ProfileService(
            auth ?? new RecordingAuthService(),
            store ?? new RecordingCredentialStore(),
            options ?? new ProfileOptions(),
            localization
        );
    }

    private sealed class RecordingCredentialStore : IProfileCredentialStore
    {
        public StoredProfileCredentials? Stored { get; set; }

        public Exception? SaveException { get; init; }

        public int ReadCount { get; private set; }

        public int SaveCount { get; private set; }

        public int ClearCount { get; private set; }

        public Task<StoredProfileCredentials?> ReadAsync(
            CancellationToken cancellationToken = default
        )
        {
            cancellationToken.ThrowIfCancellationRequested();
            ReadCount++;
            return Task.FromResult(Stored);
        }

        public Task SaveAsync(
            StoredProfileCredentials credentials,
            CancellationToken cancellationToken = default
        )
        {
            cancellationToken.ThrowIfCancellationRequested();
            SaveCount++;
            if (SaveException is not null)
            {
                return Task.FromException(SaveException);
            }

            Stored = credentials;
            return Task.CompletedTask;
        }

        public Task ClearAsync(CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            ClearCount++;
            Stored = null;
            return Task.CompletedTask;
        }
    }

    private sealed class RecordingAuthService : IProfileAuthService
    {
        public ProfileAuthenticationResult AuthenticationResult { get; init; } =
            ProfileAuthenticationResult.Success();

        public Exception? SignOutException { get; set; }

        public ProfileSignInRequest? LastRequest { get; private set; }

        public int AuthenticateCount { get; private set; }

        public int SignOutCount { get; private set; }

        public Task<ProfileAuthenticationResult> AuthenticateAsync(
            ProfileSignInRequest request,
            CancellationToken cancellationToken = default
        )
        {
            cancellationToken.ThrowIfCancellationRequested();
            LastRequest = request;
            AuthenticateCount++;
            return Task.FromResult(AuthenticationResult);
        }

        public Task SignOutAsync(
            ProfileUser profile,
            CancellationToken cancellationToken = default
        )
        {
            cancellationToken.ThrowIfCancellationRequested();
            SignOutCount++;
            return SignOutException is null
                ? Task.CompletedTask
                : Task.FromException(SignOutException);
        }
    }
}

public sealed class SimpleProfileAuthServiceTests
{
    [Fact]
    public async Task AuthenticateAsync_ValidatesNamePasswordAndCancellation()
    {
        var localization = new LocalizationService(
            new ApplicationDataOptions(),
            Path.Combine(Path.GetTempPath(), $"Flourish.Core.Auth-{Guid.NewGuid():N}")
        );
        var sut = new SimpleProfileAuthService(localization);

        var noName = await sut.AuthenticateAsync(
            new ProfileSignInRequest("", "", "secret", NameOrder.FirstLast)
        );
        var noPassword = await sut.AuthenticateAsync(
            new ProfileSignInRequest("Ada", "", "", NameOrder.FirstLast)
        );
        var success = await sut.AuthenticateAsync(
            new ProfileSignInRequest("Ada", "", "secret", NameOrder.FirstLast)
        );
        using var canceled = new CancellationTokenSource();
        canceled.Cancel();

        Assert.Equal("Enter a name.", noName.ErrorMessage);
        Assert.Equal("Enter a password.", noPassword.ErrorMessage);
        Assert.True(success.Succeeded);
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            sut.AuthenticateAsync(
                new ProfileSignInRequest("Ada", "", "secret", NameOrder.FirstLast),
                canceled.Token
            )
        );
    }
}
