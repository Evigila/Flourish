using System.Text.Json.Serialization;
using ArkheideSystem.Flourish.Abstract;

namespace ArkheideSystem.Flourish.Services;

internal sealed record StoredProfileCredentials
{
    public const int CurrentSchemaVersion = 2;

    public int SchemaVersion { get; init; } = 1;

    public string? FirstName { get; init; }

    public string? LastName { get; init; }

    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public string? UserName { get; init; }

    public string Password { get; init; } = string.Empty;

    public string? ImagePath { get; init; }

    public bool RememberLogin { get; init; }

    public bool UsesFutureSchema => SchemaVersion > CurrentSchemaVersion;

    public static StoredProfileCredentials Create(
        string firstName,
        string lastName,
        string password,
        string? imagePath,
        bool rememberLogin
    )
    {
        return new StoredProfileCredentials
        {
            SchemaVersion = CurrentSchemaVersion,
            FirstName = firstName,
            LastName = lastName,
            Password = password,
            ImagePath = imagePath,
            RememberLogin = rememberLogin,
        };
    }

    public bool TryGetName(
        NameOrder nameOrder,
        out (string FirstName, string LastName) name
    )
    {
        if (UsesFutureSchema)
        {
            name = default;
            return false;
        }

        var firstName = FirstName?.Trim() ?? string.Empty;
        var lastName = LastName?.Trim() ?? string.Empty;
        if (firstName.Length == 0 && lastName.Length == 0)
        {
            name = ProfileUser.ParseDisplayName(UserName, nameOrder);
            return name.FirstName.Length > 0 || name.LastName.Length > 0;
        }

        name = (firstName, lastName);
        return true;
    }
}
