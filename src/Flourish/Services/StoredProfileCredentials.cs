namespace ArkheideSystem.Flourish.Services;

internal sealed record StoredProfileCredentials
{
    public const int CurrentSchemaVersion = 1;

    public int SchemaVersion { get; init; } = CurrentSchemaVersion;

    public string? FirstName { get; init; }

    public string? LastName { get; init; }

    public string Password { get; init; } = string.Empty;

    public string? ImagePath { get; init; }

    public bool RememberLogin { get; init; }

    public bool IsSupportedSchema => SchemaVersion == CurrentSchemaVersion;

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

    public bool TryGetName(out (string FirstName, string LastName) name)
    {
        if (!IsSupportedSchema)
        {
            name = default;
            return false;
        }

        var firstName = FirstName?.Trim() ?? string.Empty;
        var lastName = LastName?.Trim() ?? string.Empty;
        if (firstName.Length == 0 && lastName.Length == 0)
        {
            name = default;
            return false;
        }

        name = (firstName, lastName);
        return true;
    }
}
