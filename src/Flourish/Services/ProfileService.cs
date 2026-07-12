using ArkheideSystem.Flourish.Abstract;
using ArkheideSystem.Flourish.Internal.Configuration;

namespace ArkheideSystem.Flourish.Services;

internal sealed class ProfileService : IProfileService
{
    private readonly IProfileAuthService authService;
    private readonly ProfileSecretStore secretStore;
    private readonly FlourishLocalizationService localizationService;
    private readonly ProfileUser defaultProfile;
    private readonly NameOrder nameOrder;
    private readonly SemaphoreSlim gate = new(1, 1);
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
        nameOrder = options.NameOrder;
        defaultProfile = new ProfileUser(
            string.IsNullOrWhiteSpace(options.DefaultFirstName)
                ? localizationService.Get(FlourishLocaleKeys.ProfileDefaultName)
                : options.DefaultFirstName,
            options.DefaultLastName,
            nameOrder,
            options.DefaultImagePath
        );
        CurrentProfile = defaultProfile;
    }

    public ProfileUser CurrentProfile { get; private set; }

    public ProfileLoginState LoginState { get; private set; } =
        ProfileLoginState.SignedOut;

    public event EventHandler<ProfileChangedEventArgs>? ProfileChanged;

    public async Task InitializeAsync(CancellationToken cancellationToken = default)
    {
        ProfileChangedEventArgs? changed = null;
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

            if (!stored.TryGetName(nameOrder, out var storedName))
            {
                if (!stored.UsesFutureSchema)
                {
                    await secretStore.ClearAsync(cancellationToken).ConfigureAwait(false);
                }

                return;
            }

            var request = new ProfileSignInRequest(
                storedName.FirstName,
                storedName.LastName,
                stored.Password,
                nameOrder,
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
            if (stored.SchemaVersion < StoredProfileCredentials.CurrentSchemaVersion)
            {
                await secretStore
                    .SaveAsync(currentCredentials, cancellationToken)
                    .ConfigureAwait(false);
            }

            CurrentProfile = new ProfileUser(
                storedName.FirstName,
                storedName.LastName,
                nameOrder,
                stored.ImagePath
            );
            LoginState = ProfileLoginState.SignedInRemembered;
            changed = CreateChangedEventArgs();
        }
        finally
        {
            gate.Release();
        }

        RaiseProfileChanged(changed);
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
            nameOrder,
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

        var authenticatedProfile = new ProfileUser(
            normalizedRequest.FirstName,
            normalizedRequest.LastName,
            nameOrder,
            normalizedRequest.ImagePath
        );

        ProfileChangedEventArgs changed;
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
            CurrentProfile = authenticatedProfile;
            LoginState = ProfileLoginState.SignedIn;
            changed = CreateChangedEventArgs();
        }
        finally
        {
            gate.Release();
        }

        RaiseProfileChanged(changed);
        return result;
    }

    public async Task SetRememberLoginAsync(
        bool rememberLogin,
        CancellationToken cancellationToken = default
    )
    {
        ProfileChangedEventArgs? changed = null;
        await gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            if (currentCredentials is null || LoginState == ProfileLoginState.SignedOut)
            {
                throw new InvalidOperationException(
                    localizationService.Get(
                        FlourishLocaleKeys.ProfileRememberLoginRequiresSignIn
                    )
                );
            }

            var nextState = rememberLogin
                ? ProfileLoginState.SignedInRemembered
                : ProfileLoginState.SignedIn;
            if (
                currentCredentials.RememberLogin == rememberLogin
                && LoginState == nextState
            )
            {
                return;
            }

            var updatedCredentials = currentCredentials with
            {
                RememberLogin = rememberLogin,
            };
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
            LoginState = nextState;
            changed = CreateChangedEventArgs();
        }
        finally
        {
            gate.Release();
        }

        RaiseProfileChanged(changed);
    }

    public async Task SignOutAsync(CancellationToken cancellationToken = default)
    {
        ProfileUser signedInProfile;
        await gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            signedInProfile = CurrentProfile;
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

        ProfileChangedEventArgs changed;
        await gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            await secretStore.ClearAsync(cancellationToken).ConfigureAwait(false);
            currentCredentials = null;
            CurrentProfile = defaultProfile;
            LoginState = ProfileLoginState.SignedOut;
            changed = CreateChangedEventArgs();
        }
        finally
        {
            gate.Release();
        }

        RaiseProfileChanged(changed);
        if (signOutError is not null)
        {
            throw signOutError;
        }
    }

    private ProfileChangedEventArgs CreateChangedEventArgs()
    {
        return new ProfileChangedEventArgs(CurrentProfile, LoginState);
    }

    private void RaiseProfileChanged(ProfileChangedEventArgs? changed)
    {
        if (changed is not null)
        {
            ProfileChanged?.Invoke(this, changed);
        }
    }
}
