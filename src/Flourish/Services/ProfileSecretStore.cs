using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using ArkheideSystem.Flourish.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.UserSecrets;

namespace ArkheideSystem.Flourish.Services;

internal sealed class ProfileSecretStore
{
    private const string SecretKey = "Flourish.Profile.Credential";
    private static readonly JsonSerializerOptions SerializerOptions = new(
        JsonSerializerDefaults.Web
    );
    private readonly SemaphoreSlim gate = new(1, 1);
    private readonly string secretId;
    private readonly byte[] entropy;

    public ProfileSecretStore(
        FlourishDataOptions dataOptions,
        FlourishShellOptions shellOptions
    )
    {
        secretId = CreateSecretId(dataOptions, shellOptions);
        entropy = SHA256.HashData(Encoding.UTF8.GetBytes(secretId));
    }

    public async Task<StoredProfileCredentials?> ReadAsync(
        CancellationToken cancellationToken = default
    )
    {
        await gate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            var secretPath = PathHelper.GetSecretsPathFromSecretsId(secretId);
            if (!File.Exists(secretPath))
            {
                return null;
            }

            var configuration = new ConfigurationBuilder()
                .AddUserSecrets(secretId, reloadOnChange: false)
                .Build();
            var encryptedValue = configuration[SecretKey];
            if (string.IsNullOrWhiteSpace(encryptedValue))
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
        var plainBytes = JsonSerializer.SerializeToUtf8Bytes(
            credentials,
            SerializerOptions
        );
        try
        {
            var encryptedBytes = ProtectedData.Protect(
                plainBytes,
                entropy,
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
        var encryptedBytes = Convert.FromBase64String(encryptedValue);
        var plainBytes = ProtectedData.Unprotect(
            encryptedBytes,
            entropy,
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
        var secretPath = PathHelper.GetSecretsPathFromSecretsId(secretId);
        var root = await ReadSecretDocumentAsync(secretPath, cancellationToken)
            .ConfigureAwait(false);

        if (encryptedValue is null)
        {
            root.Remove(SecretKey);
        }
        else
        {
            root[SecretKey] = encryptedValue;
        }

        if (root.Count == 0)
        {
            if (File.Exists(secretPath))
            {
                File.Delete(secretPath);
            }

            return;
        }

        var directory = Path.GetDirectoryName(secretPath)
            ?? throw new InvalidOperationException("The user secrets path has no directory.");
        Directory.CreateDirectory(directory);
        var temporaryPath = Path.Combine(
            directory,
            $".{Path.GetFileName(secretPath)}.{Guid.NewGuid():N}.tmp"
        );

        try
        {
            await File.WriteAllTextAsync(
                    temporaryPath,
                    root.ToJsonString(new JsonSerializerOptions { WriteIndented = true }),
                    cancellationToken
                )
                .ConfigureAwait(false);
            File.Move(temporaryPath, secretPath, overwrite: true);
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
            return JsonNode.Parse(json) as JsonObject ?? [];
        }
        catch (JsonException)
        {
            return [];
        }
    }

    private static string CreateSecretId(
        FlourishDataOptions dataOptions,
        FlourishShellOptions shellOptions
    )
    {
        var entryAssemblyName =
            Assembly.GetEntryAssembly()?.GetName().Name ?? "Application";
        var companyName = string.IsNullOrWhiteSpace(dataOptions.CompanyName)
            ? "ArkheideSystem"
            : dataOptions.CompanyName;
        var appName = string.IsNullOrWhiteSpace(dataOptions.AppName)
            ? string.IsNullOrWhiteSpace(shellOptions.Title)
                ? entryAssemblyName
                : shellOptions.Title
            : dataOptions.AppName;
        var identityBytes = Encoding.UTF8.GetBytes(
            $"{companyName}|{appName}|{entryAssemblyName}"
        );
        var hash = Convert.ToHexString(SHA256.HashData(identityBytes));
        return $"ArkheideSystem.Flourish.Profile.{hash[..24]}";
    }
}
