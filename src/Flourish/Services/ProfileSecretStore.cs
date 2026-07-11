using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;

namespace ArkheideSystem.Flourish.Services;

internal sealed class ProfileSecretStore
{
    private const string SecretKey = "Flourish:Profile:Credential";
    private static readonly JsonSerializerOptions SerializerOptions = new(
        JsonSerializerDefaults.Web
    );
    private readonly SemaphoreSlim gate = new(1, 1);
    private readonly IConfigurationRoot? configurationRoot;
    private readonly JsonConfigurationProvider? secretsProvider;
    private readonly string? secretPath;
    private readonly byte[]? entropy;

    public ProfileSecretStore(IConfiguration configuration)
    {
        ArgumentNullException.ThrowIfNull(configuration);

        configurationRoot = configuration as IConfigurationRoot;
        (secretsProvider, secretPath) = FindUserSecretsProvider(configurationRoot);
        if (secretPath is not null)
        {
            entropy = SHA256.HashData(
                Encoding.UTF8.GetBytes(Path.GetFullPath(secretPath).ToUpperInvariant())
            );
        }
    }

    public async Task<StoredProfileCredentials?> ReadAsync(
        CancellationToken cancellationToken = default
    )
    {
        await gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            if (
                secretsProvider is null
                || !secretsProvider.TryGet(SecretKey, out var encryptedValue)
                || string.IsNullOrWhiteSpace(encryptedValue)
            )
            {
                return null;
            }

            try
            {
                return Decrypt(encryptedValue);
            }
            catch (CryptographicException)
            {
                await WriteValueCoreAsync(null, cancellationToken).ConfigureAwait(false);
                return null;
            }
            catch (FormatException)
            {
                await WriteValueCoreAsync(null, cancellationToken).ConfigureAwait(false);
                return null;
            }
            catch (JsonException)
            {
                await WriteValueCoreAsync(null, cancellationToken).ConfigureAwait(false);
                return null;
            }
        }
        finally
        {
            gate.Release();
        }
    }

    public async Task SaveAsync(
        StoredProfileCredentials credentials,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(credentials);
        EnsureUserSecretsAreConfigured();

        await gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            await WriteValueCoreAsync(Encrypt(credentials), cancellationToken)
                .ConfigureAwait(false);
        }
        finally
        {
            gate.Release();
        }
    }

    public async Task ClearAsync(CancellationToken cancellationToken = default)
    {
        if (secretPath is null)
        {
            return;
        }

        await gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            await WriteValueCoreAsync(null, cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            gate.Release();
        }
    }

    private string Encrypt(StoredProfileCredentials credentials)
    {
        var encryptionEntropy = entropy
            ?? throw new InvalidOperationException(
                "Profile credential encryption requires a configured User Secrets provider."
            );
        var plainBytes = JsonSerializer.SerializeToUtf8Bytes(
            credentials,
            SerializerOptions
        );
        try
        {
            var encryptedBytes = ProtectedData.Protect(
                plainBytes,
                encryptionEntropy,
                DataProtectionScope.CurrentUser
            );
            return Convert.ToBase64String(encryptedBytes);
        }
        finally
        {
            CryptographicOperations.ZeroMemory(plainBytes);
        }
    }

    private StoredProfileCredentials? Decrypt(string encryptedValue)
    {
        var encryptionEntropy = entropy
            ?? throw new InvalidOperationException(
                "Profile credential decryption requires a configured User Secrets provider."
            );
        var encryptedBytes = Convert.FromBase64String(encryptedValue);
        var plainBytes = ProtectedData.Unprotect(
            encryptedBytes,
            encryptionEntropy,
            DataProtectionScope.CurrentUser
        );
        try
        {
            return JsonSerializer.Deserialize<StoredProfileCredentials>(
                plainBytes,
                SerializerOptions
            );
        }
        finally
        {
            CryptographicOperations.ZeroMemory(plainBytes);
        }
    }

    private async Task WriteValueCoreAsync(
        string? encryptedValue,
        CancellationToken cancellationToken
    )
    {
        var writableSecretPath = secretPath
            ?? throw new InvalidOperationException(
                "Profile credential persistence requires a configured User Secrets provider."
            );
        var root = await ReadSecretDocumentAsync(writableSecretPath, cancellationToken)
            .ConfigureAwait(false);

        SetCredentialValue(root, encryptedValue);

        if (root.Count == 0)
        {
            if (File.Exists(writableSecretPath))
            {
                File.Delete(writableSecretPath);
            }

            configurationRoot?.Reload();
            return;
        }

        var directory = Path.GetDirectoryName(writableSecretPath)
            ?? throw new InvalidOperationException("The user secrets path has no directory.");
        Directory.CreateDirectory(directory);
        var temporaryPath = Path.Combine(
            directory,
            $".{Path.GetFileName(writableSecretPath)}.{Guid.NewGuid():N}.tmp"
        );

        try
        {
            await File.WriteAllTextAsync(
                    temporaryPath,
                    root.ToJsonString(new JsonSerializerOptions { WriteIndented = true }),
                    cancellationToken
                )
                .ConfigureAwait(false);
            File.Move(temporaryPath, writableSecretPath, overwrite: true);
            configurationRoot?.Reload();
        }
        finally
        {
            if (File.Exists(temporaryPath))
            {
                File.Delete(temporaryPath);
            }
        }
    }

    private static async Task<JsonObject> ReadSecretDocumentAsync(
        string secretPath,
        CancellationToken cancellationToken
    )
    {
        if (!File.Exists(secretPath))
        {
            return [];
        }

        try
        {
            var json = await File.ReadAllTextAsync(secretPath, cancellationToken)
                .ConfigureAwait(false);
            return JsonNode.Parse(json) as JsonObject
                ?? throw new InvalidDataException(
                    "The User Secrets file must contain a JSON object and was not modified."
                );
        }
        catch (JsonException error)
        {
            throw new InvalidDataException(
                "The User Secrets file contains invalid JSON and was not modified.",
                error
            );
        }
    }

    private static void SetCredentialValue(JsonObject root, string? encryptedValue)
    {
        var flatPropertyName = FindPropertyName(root, SecretKey);
        var flourishPropertyName = FindPropertyName(root, "Flourish");
        var flourish = flourishPropertyName is null
            ? null
            : root[flourishPropertyName] as JsonObject;
        var profilePropertyName = flourish is null
            ? null
            : FindPropertyName(flourish, "Profile");
        var profile = profilePropertyName is null
            ? null
            : flourish![profilePropertyName] as JsonObject;
        var credentialPropertyName = profile is null
            ? null
            : FindPropertyName(profile, "Credential");

        if (encryptedValue is not null)
        {
            if (flatPropertyName is not null)
            {
                root[flatPropertyName] = encryptedValue;
            }
            else if (profile is not null)
            {
                profile[credentialPropertyName ?? "Credential"] = encryptedValue;
            }
            else
            {
                root[SecretKey] = encryptedValue;
            }

            return;
        }

        if (flatPropertyName is not null)
        {
            root.Remove(flatPropertyName);
        }

        if (profile is null || credentialPropertyName is null)
        {
            return;
        }

        profile.Remove(credentialPropertyName);
        if (profile.Count == 0 && flourish is not null && profilePropertyName is not null)
        {
            flourish.Remove(profilePropertyName);
        }

        if (flourish?.Count == 0 && flourishPropertyName is not null)
        {
            root.Remove(flourishPropertyName);
        }
    }

    private static string? FindPropertyName(JsonObject parent, string propertyName)
    {
        return parent
            .Select(property => property.Key)
            .FirstOrDefault(existingName =>
                string.Equals(existingName, propertyName, StringComparison.OrdinalIgnoreCase)
            );
    }

    private static (
        JsonConfigurationProvider? Provider,
        string? Path
    ) FindUserSecretsProvider(IConfigurationRoot? configurationRoot)
    {
        if (configurationRoot is null)
        {
            return (null, null);
        }

        foreach (
            var provider in configurationRoot.Providers
                .OfType<JsonConfigurationProvider>()
                .Reverse()
        )
        {
            var sourcePath = provider.Source.Path;
            if (
                string.IsNullOrWhiteSpace(sourcePath)
                || !string.Equals(
                    Path.GetFileName(sourcePath),
                    "secrets.json",
                    StringComparison.OrdinalIgnoreCase
                )
            )
            {
                continue;
            }

            var physicalPath = provider.Source.FileProvider
                ?.GetFileInfo(sourcePath)
                .PhysicalPath;
            if (!string.IsNullOrWhiteSpace(physicalPath))
            {
                return (provider, Path.GetFullPath(physicalPath));
            }
        }

        return (null, null);
    }

    private void EnsureUserSecretsAreConfigured()
    {
        if (secretsProvider is not null && secretPath is not null)
        {
            return;
        }

        throw new InvalidOperationException(
            "Remembered profile credentials require User Secrets. "
                + "Set <UserSecretsId> in the application project so Flourish can add "
                + "its User Secrets provider to the Host configuration."
        );
    }
}
