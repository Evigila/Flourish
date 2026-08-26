using System;
using System.Threading;
using System.Threading.Tasks;

using ArkheideSystem.Flourish.Abstract;
using ArkheideSystem.Flourish.Localization;

namespace ArkheideSystem.Flourish.Profile;

internal sealed class ProfileService : IProfileService
{
    private readonly IProfileAuthService authService;
    private readonly ProfileSecretStore secretStore;
    private readonly FlourishLocalizationService localizationService;
    private ProfileUser defaultProfile;
    private readonly SemaphoreSlim gate = new(1, 1);
    private FlourishProfileState current;
    private StoredProfileCredentials? currentCredentials;
    private bool isInitialized;

    public ProfileService(
        IProfileAuthService authService,
        ProfileSecretStore secretStore,
        FlourishProfileOptions options,
        FlourishLocalizationService localizationService
    )
    {
        this.authService = authService;
        this.secretStore = secretStore;
        this.localizationService = localizationService;
        var nameOrder = options.NameOrder;
        defaultProfile = new ProfileUser(
            string.IsNullOrWhiteSpace(options.DefaultFirstName)
                ? localizationService.Get(FlourishLocaleKeys.ProfileDefaultName)
                : options.DefaultFirstName,
            options.DefaultLastName,
            nameOrder,
            options.DefaultImagePath
        );
        current = new FlourishProfileState(defaultProfile, ProfileLoginState.SignedOut);
    }

    public FlourishProfileState Current => Volatile.Read(ref current);

    public event EventHandler<FlourishStateChangedEventArgs<FlourishProfileState>>? Changed;

    internal async Task RestoreAsync(CancellationToken cancellationToken = default)
    {
        FlourishStateChangedEventArgs<FlourishProfileState>? changed = null;
        await gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            if (isInitialized)
            {
                return;
            }

            isInitialized = true;
            var stored = await secretStore.ReadAsync(cancellationToken).ConfigureAwait(false);
            if (stored is null)
            {
                return;
            }

            if (!stored.RememberLogin)
            {
                await secretStore.ClearAsync(cancellationToken).ConfigureAwait(false);
                return;
            }

            if (!stored.TryGetName(out var storedName))
            {
                await secretStore.ClearAsync(cancellationToken).ConfigureAwait(false);
                return;
            }

            var request = new ProfileSignInRequest(
                storedName.FirstName,
                storedName.LastName,
                stored.Password,
                Current.NameOrder,
                stored.ImagePath
            );
            var result = await authService
                .AuthenticateAsync(request, cancellationToken)
                .ConfigureAwait(false);
            if (!result.Succeeded)
            {
                await secretStore.ClearAsync(cancellationToken).ConfigureAwait(false);
                return;
            }

            currentCredentials = StoredProfileCredentials.Create(
                storedName.FirstName,
                storedName.LastName,
                stored.Password,
                stored.ImagePath,
                rememberLogin: true
            );

            var profile = new ProfileUser(
                storedName.FirstName,
                storedName.LastName,
                Current.NameOrder,
                stored.ImagePath
            );
            changed = PublishState(profile, ProfileLoginState.SignedInRemembered);
        }
        finally
        {
            gate.Release();
        }

        RaiseChanged(changed);
    }

    public async Task<ProfileAuthenticationResult> SignInAsync(
        ProfileSignInRequest request,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(request);

        var normalizedRequest = new ProfileSignInRequest(
            request.FirstName?.Trim() ?? string.Empty,
            request.LastName?.Trim() ?? string.Empty,
            request.Password ?? string.Empty,
            Current.NameOrder,
            string.IsNullOrWhiteSpace(request.ImagePath) ? null : request.ImagePath.Trim()
        );
        if (string.IsNullOrWhiteSpace(normalizedRequest.DisplayName))
        {
            return ProfileAuthenticationResult.Failure(
                localizationService.Get(FlourishLocaleKeys.ProfileEnterName)
            );
        }

        var result = await authService
            .AuthenticateAsync(normalizedRequest, cancellationToken)
            .ConfigureAwait(false);
        if (!result.Succeeded)
        {
            return result;
        }

        FlourishStateChangedEventArgs<FlourishProfileState>? changed;
        await gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            var stored = StoredProfileCredentials.Create(
                normalizedRequest.FirstName,
                normalizedRequest.LastName,
                normalizedRequest.Password,
                normalizedRequest.ImagePath,
                rememberLogin: false
            );

            isInitialized = true;
            currentCredentials = stored;
            var profile = new ProfileUser(
                normalizedRequest.FirstName,
                normalizedRequest.LastName,
                Current.NameOrder,
                normalizedRequest.ImagePath
            );
            changed = PublishState(profile, ProfileLoginState.SignedIn);
        }
        finally
        {
            gate.Release();
        }

        RaiseChanged(changed);
        return result;
    }

    public async Task SetRememberLoginAsync(
        bool rememberLogin,
        CancellationToken cancellationToken = default
    )
    {
        FlourishStateChangedEventArgs<FlourishProfileState>? changed = null;
        await gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            var state = Current;
            if (currentCredentials is null || state.LoginState == ProfileLoginState.SignedOut)
            {
                throw new InvalidOperationException(
                    localizationService.Get(FlourishLocaleKeys.ProfileRememberLoginRequiresSignIn)
                );
            }

            var nextState = rememberLogin
                ? ProfileLoginState.SignedInRemembered
                : ProfileLoginState.SignedIn;
            if (currentCredentials.RememberLogin == rememberLogin && state.LoginState == nextState)
            {
                return;
            }

            var updatedCredentials = currentCredentials with { RememberLogin = rememberLogin };
            if (rememberLogin)
            {
                await secretStore
                    .SaveAsync(updatedCredentials, cancellationToken)
                    .ConfigureAwait(false);
            }
            else
            {
                await secretStore.ClearAsync(cancellationToken).ConfigureAwait(false);
            }
            currentCredentials = updatedCredentials;
            changed = PublishState(state.Profile, nextState);
        }
        finally
        {
            gate.Release();
        }

        RaiseChanged(changed);
    }

    public async Task SetNameOrderAsync(
        NameOrder nameOrder,
        CancellationToken cancellationToken = default
    )
    {
        if (!Enum.IsDefined(nameOrder))
        {
            throw new ArgumentOutOfRangeException(nameof(nameOrder), nameOrder, null);
        }

        FlourishStateChangedEventArgs<FlourishProfileState>? changed = null;
        await gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            var state = Current;
            if (state.NameOrder == nameOrder)
            {
                return;
            }

            defaultProfile = WithNameOrder(defaultProfile, nameOrder);
            var profile =
                state.LoginState == ProfileLoginState.SignedOut
                    ? defaultProfile
                    : WithNameOrder(state.Profile, nameOrder);
            changed = PublishState(profile, state.LoginState);
        }
        finally
        {
            gate.Release();
        }

        RaiseChanged(changed);
    }

    public async Task SignOutAsync(CancellationToken cancellationToken = default)
    {
        ProfileUser signedInProfile;
        await gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            signedInProfile = Current.Profile;
        }
        finally
        {
            gate.Release();
        }

        Exception? signOutError = null;
        try
        {
            await authService
                .SignOutAsync(signedInProfile, cancellationToken)
                .ConfigureAwait(false);
        }
        catch (Exception error) when (error is not OperationCanceledException)
        {
            signOutError = error;
        }

        FlourishStateChangedEventArgs<FlourishProfileState>? changed;
        await gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            await secretStore.ClearAsync(cancellationToken).ConfigureAwait(false);
            currentCredentials = null;
            changed = PublishState(defaultProfile, ProfileLoginState.SignedOut);
        }
        finally
        {
            gate.Release();
        }

        RaiseChanged(changed);
        if (signOutError is not null)
        {
            throw signOutError;
        }
    }

    private FlourishStateChangedEventArgs<FlourishProfileState>? PublishState(
        ProfileUser profile,
        ProfileLoginState loginState
    )
    {
        var previous = Current;
        var next = new FlourishProfileState(profile, loginState);
        if (next == previous)
        {
            return null;
        }

        Volatile.Write(ref current, next);
        return new FlourishStateChangedEventArgs<FlourishProfileState>(next);
    }

    private static ProfileUser WithNameOrder(ProfileUser profile, NameOrder nameOrder)
    {
        return new ProfileUser(profile.FirstName, profile.LastName, nameOrder, profile.ImagePath);
    }

    private void RaiseChanged(FlourishStateChangedEventArgs<FlourishProfileState>? changed)
    {
        if (changed is not null)
        {
            Changed?.Invoke(this, changed);
        }
    }
}
