namespace ArkheideSystem.Flourish.Services;

internal sealed record StoredProfileCredentials(
    string UserName,
    string Password,
    string? ImagePath,
    bool RememberLogin
);
