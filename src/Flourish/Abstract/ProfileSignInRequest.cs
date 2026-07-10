namespace ArkheideSystem.Flourish.Abstract;

/// <summary>
/// Contains the credentials and display information submitted for profile login.
/// </summary>
public sealed class ProfileSignInRequest
{
    /// <summary>
    /// Initializes a profile sign-in request.
    /// </summary>
    /// <param name="userName">The user display name.</param>
    /// <param name="password">The password supplied by the user.</param>
    /// <param name="imagePath">An optional profile image path.</param>
    public ProfileSignInRequest(
        string userName,
        string password,
        string? imagePath = null
    )
    {
        UserName = userName;
        Password = password;
        ImagePath = imagePath;
    }

    /// <summary>
    /// Gets the submitted user display name.
    /// </summary>
    public string UserName { get; }

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
