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
        : this(
            ProfileUser.ParseDisplayName(userName, NameOrder.FirstLast),
            password,
            NameOrder.FirstLast,
            imagePath
        )
    {
    }

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
    /// Gets the same formatted sign-in name as <see cref="DisplayName" />.
    /// </summary>
    public string UserName => DisplayName;

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
