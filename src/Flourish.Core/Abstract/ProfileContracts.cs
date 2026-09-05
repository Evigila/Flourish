using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;

namespace ArkheideSystem.Flourish.Abstract;

/// <summary>
/// Specifies how profile name parts are displayed.
/// </summary>
public enum NameOrder
{
    /// <summary>
    /// Displays the first name before the last name.
    /// </summary>
    FirstLast,

    /// <summary>
    /// Displays the last name before the first name.
    /// </summary>
    LastFirst,
}

/// <summary>
/// Represents the user information displayed by the Flourish profile surface.
/// </summary>
public sealed record ProfileUser
{
    /// <summary>
    /// Initializes a profile user from separate name parts.
    /// </summary>
    /// <param name="firstName">The user's first name.</param>
    /// <param name="lastName">The user's last name.</param>
    /// <param name="nameOrder">The order used to display the name and initials.</param>
    /// <param name="imagePath">An optional local or pack URI image path.</param>
    public ProfileUser(
        string firstName,
        string lastName,
        NameOrder nameOrder,
        string? imagePath = null
    )
        : this((firstName, lastName), nameOrder, imagePath)
    {
    }

    private ProfileUser(
        (string FirstName, string LastName) name,
        NameOrder nameOrder,
        string? imagePath
    )
    {
        if (!Enum.IsDefined(nameOrder))
        {
            throw new ArgumentOutOfRangeException(nameof(nameOrder), nameOrder, null);
        }

        FirstName = NormalizeNamePart(name.FirstName);
        LastName = NormalizeNamePart(name.LastName);
        if (FirstName.Length == 0 && LastName.Length == 0)
        {
            throw new ArgumentException("At least one profile name is required.");
        }

        NameOrder = nameOrder;
        ImagePath = string.IsNullOrWhiteSpace(imagePath) ? null : imagePath.Trim();
    }

    /// <summary>
    /// Gets the user's first name.
    /// </summary>
    public string FirstName { get; }

    /// <summary>
    /// Gets the user's last name.
    /// </summary>
    public string LastName { get; }

    /// <summary>
    /// Gets the configured name order.
    /// </summary>
    public NameOrder NameOrder { get; }

    /// <summary>
    /// Gets the formatted profile display name.
    /// </summary>
    public string DisplayName => FormatDisplayName(FirstName, LastName, NameOrder);

    /// <summary>
    /// Gets the optional profile image path.
    /// </summary>
    public string? ImagePath { get; }

    /// <summary>
    /// Gets the initials used when no profile image is available.
    /// </summary>
    public string Initials
    {
        get
        {
            var firstInitial = GetInitial(FirstName);
            var lastInitial = GetInitial(LastName);
            var initials = NameOrder == NameOrder.FirstLast
                ? string.Concat(firstInitial, lastInitial)
                : string.Concat(lastInitial, firstInitial);
            return initials.Length == 0 ? "U" : initials.ToUpperInvariant();
        }
    }

    internal static string FormatDisplayName(
        string? firstName,
        string? lastName,
        NameOrder nameOrder
    )
    {
        var first = NormalizeNamePart(firstName);
        var last = NormalizeNamePart(lastName);
        return nameOrder == NameOrder.FirstLast
            ? JoinNameParts(first, last)
            : JoinNameParts(last, first);
    }

    private static string JoinNameParts(string leading, string trailing)
    {
        if (leading.Length == 0)
        {
            return trailing;
        }

        return trailing.Length == 0 ? leading : $"{leading} {trailing}";
    }

    private static string GetInitial(string value)
    {
        return value.Length == 0 ? string.Empty : StringInfo.GetNextTextElement(value);
    }

    private static string NormalizeNamePart(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? string.Empty : value.Trim();
    }
}

/// <summary>
/// Represents the current profile, login state, and name presentation settings.
/// </summary>
/// <param name="Profile">The profile currently displayed by the shell.</param>
/// <param name="LoginState">The current authentication state.</param>
public sealed record ProfileState(
    ProfileUser Profile,
    ProfileLoginState LoginState
)
{
    /// <summary>Gets the application-wide profile name order.</summary>
    public NameOrder NameOrder => Profile.NameOrder;
}

/// <summary>
/// Describes the current Flourish profile login state.
/// </summary>
public enum ProfileLoginState
{
    /// <summary>
    /// No user is currently signed in.
    /// </summary>
    SignedOut,

    /// <summary>
    /// A user is signed in for the current application session.
    /// </summary>
    SignedIn,

    /// <summary>
    /// A user is signed in and the login will be restored on the next startup.
    /// </summary>
    SignedInRemembered,
}

/// <summary>
/// Contains the credentials and display information submitted for profile login.
/// </summary>
public sealed class ProfileSignInRequest
{
    /// <summary>
    /// Initializes a profile sign-in request from separate name parts.
    /// </summary>
    /// <param name="firstName">The user's first name.</param>
    /// <param name="lastName">The user's last name.</param>
    /// <param name="password">The password supplied by the user.</param>
    /// <param name="nameOrder">The order used to display the name.</param>
    /// <param name="imagePath">An optional profile image path.</param>
    public ProfileSignInRequest(
        string firstName,
        string lastName,
        string password,
        NameOrder nameOrder,
        string? imagePath = null
    )
        : this((firstName, lastName), password, nameOrder, imagePath)
    {
    }

    private ProfileSignInRequest(
        (string FirstName, string LastName) name,
        string password,
        NameOrder nameOrder,
        string? imagePath
    )
    {
        if (!Enum.IsDefined(nameOrder))
        {
            throw new ArgumentOutOfRangeException(nameof(nameOrder), nameOrder, null);
        }

        FirstName = name.FirstName ?? string.Empty;
        LastName = name.LastName ?? string.Empty;
        Password = password ?? string.Empty;
        NameOrder = nameOrder;
        ImagePath = imagePath;
    }

    /// <summary>
    /// Gets the submitted first name.
    /// </summary>
    public string FirstName { get; }

    /// <summary>
    /// Gets the submitted last name.
    /// </summary>
    public string LastName { get; }

    /// <summary>
    /// Gets the submitted name order.
    /// </summary>
    public NameOrder NameOrder { get; }

    /// <summary>
    /// Gets the formatted submitted display name.
    /// </summary>
    public string DisplayName => ProfileUser.FormatDisplayName(FirstName, LastName, NameOrder);

    /// <summary>
    /// Gets the submitted password.
    /// </summary>
    public string Password { get; }

    /// <summary>
    /// Gets the optional submitted profile image path.
    /// </summary>
    public string? ImagePath { get; }

    /// <inheritdoc />
    public override string ToString() => $"{nameof(ProfileSignInRequest)} {{ Password = *** }}";
}

/// <summary>
/// Represents the outcome of a profile authentication attempt.
/// </summary>
public sealed class ProfileAuthenticationResult
{
    private ProfileAuthenticationResult(bool succeeded, string? errorMessage)
    {
        Succeeded = succeeded;
        ErrorMessage = errorMessage;
    }

    /// <summary>
    /// Gets a value indicating whether authentication succeeded.
    /// </summary>
    public bool Succeeded { get; }

    /// <summary>
    /// Gets the optional user-facing failure message.
    /// </summary>
    public string? ErrorMessage { get; }

    /// <summary>
    /// Creates a successful authentication result.
    /// </summary>
    public static ProfileAuthenticationResult Success() => new(true, null);

    /// <summary>
    /// Creates a failed authentication result.
    /// </summary>
    /// <param name="errorMessage">The user-facing failure message.</param>
    public static ProfileAuthenticationResult Failure(string errorMessage)
    {
        if (string.IsNullOrWhiteSpace(errorMessage))
        {
            throw new ArgumentException("Error message cannot be empty.", nameof(errorMessage));
        }

        return new(false, errorMessage);
    }
}

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

/// <summary>
/// Maintains the active Flourish profile and its login state.
/// </summary>
public interface IProfileService
{
    /// <summary>
    /// Gets the current immutable profile state.
    /// </summary>
    ProfileState Current { get; }

    /// <summary>
    /// Occurs when the profile or login state changes.
    /// </summary>
    event EventHandler<StateChangedEventArgs<ProfileState>>? Changed;

    /// <summary>
    /// Authenticates and activates a profile for the current session.
    /// </summary>
    /// <param name="request">The profile sign-in request.</param>
    /// <param name="cancellationToken">A token that requests cancellation.</param>
    /// <returns>The authentication outcome.</returns>
    Task<ProfileAuthenticationResult> SignInAsync(
        ProfileSignInRequest request,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Changes whether the active login should be restored on the next startup.
    /// </summary>
    /// <param name="rememberLogin">Whether to restore the active login on the next startup.</param>
    /// <param name="cancellationToken">A token that requests cancellation.</param>
    /// <returns>A task that completes when the setting is applied.</returns>
    Task SetRememberLoginAsync(
        bool rememberLogin,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Changes the name order used by the default and active profiles.
    /// </summary>
    /// <param name="nameOrder">The order used to display profile names and initials.</param>
    /// <param name="cancellationToken">A token that requests cancellation.</param>
    /// <returns>A task that completes when the setting is applied.</returns>
    Task SetNameOrderAsync(
        NameOrder nameOrder,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Signs out and removes the persisted profile credentials.
    /// </summary>
    /// <param name="cancellationToken">A token that requests cancellation.</param>
    /// <returns>A task that completes when sign-out finishes.</returns>
    Task SignOutAsync(CancellationToken cancellationToken = default);
}
