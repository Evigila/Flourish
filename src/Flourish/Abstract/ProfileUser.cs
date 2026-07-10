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
    {
        if (string.IsNullOrWhiteSpace(userName))
        {
            throw new ArgumentException("User name cannot be empty.", nameof(userName));
        }

        UserName = userName.Trim();
        ImagePath = string.IsNullOrWhiteSpace(imagePath) ? null : imagePath.Trim();
    }

    /// <summary>
    /// Gets the user display name.
    /// </summary>
    public string UserName { get; }

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
            var words = UserName.Split(
                (char[]?)null,
                StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries
            );
            if (words.Length == 0)
            {
                return "U";
            }

            var first = StringInfo.GetNextTextElement(words[0]);
            var last = words.Length > 1
                ? StringInfo.GetNextTextElement(words[^1])
                : string.Empty;
            return string.Concat(first, last).ToUpperInvariant();
        }
    }
}
