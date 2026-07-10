using System.Globalization;

namespace ArkheideSystem.Flourish.Abstract;

/// <summary>
/// Represents the user information displayed by the Flourish profile surface.
/// </summary>
public sealed record ProfileUser
{
    /// <summary>
    /// Initializes a profile user.
    /// </summary>
    /// <param name="userName">The non-empty display name.</param>
    /// <param name="imagePath">An optional local or pack URI image path.</param>
    public ProfileUser(string userName, string? imagePath = null)
        : this(ParseDisplayName(userName, NameOrder.FirstLast), NameOrder.FirstLast, imagePath)
    {
    }

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
    /// Gets the same formatted profile display name as <see cref="DisplayName" />.
    /// </summary>
    public string UserName => DisplayName;

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

    internal static (string FirstName, string LastName) ParseDisplayName(
        string? displayName,
        NameOrder nameOrder
    )
    {
        if (!Enum.IsDefined(nameOrder))
        {
            throw new ArgumentOutOfRangeException(nameof(nameOrder), nameOrder, null);
        }

        var words = (displayName ?? string.Empty).Split(
            (char[]?)null,
            StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries
        );
        if (words.Length == 0)
        {
            return (string.Empty, string.Empty);
        }

        if (words.Length == 1)
        {
            return (words[0], string.Empty);
        }

        return nameOrder == NameOrder.FirstLast
            ? (string.Join(' ', words[..^1]), words[^1])
            : (string.Join(' ', words[1..]), words[0]);
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
