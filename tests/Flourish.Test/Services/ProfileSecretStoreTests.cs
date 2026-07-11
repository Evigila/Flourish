using System.IO;
using System.Text.Json;
using ArkheideSystem.Flourish.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.FileProviders;

namespace ArkheideSystem.Flourish.Test.Services;

public sealed class ProfileSecretStoreTests
{
    [Fact]
    public async Task SaveReadAndClearAsync_UseTheConfiguredUserSecretsProvider()
    {
        using var directory = new TemporaryDirectory();
        var secretPath = Path.Combine(directory.Path, "secrets.json");
        await File.WriteAllTextAsync(secretPath, "{ \"Other\": \"value\" }");
        using var fileProvider = new PhysicalFileProvider(directory.Path);
        var configuration = new ConfigurationBuilder()
            .AddJsonFile(
                fileProvider,
                "secrets.json",
                optional: true,
                reloadOnChange: false
            )
            .Build();
        var sut = new ProfileSecretStore(configuration);
        var credentials = StoredProfileCredentials.Create(
            "Ada",
            "Lovelace",
            "secret",
            imagePath: null,
            rememberLogin: true
        );

        await sut.SaveAsync(credentials);

        var restored = await sut.ReadAsync();
        Assert.NotNull(restored);
        Assert.Equal("Ada", restored.FirstName);
        Assert.Equal("Lovelace", restored.LastName);
        Assert.Equal("secret", restored.Password);
        Assert.True(restored.RememberLogin);

        await sut.ClearAsync();

        using var document = JsonDocument.Parse(await File.ReadAllTextAsync(secretPath));
        Assert.Equal("value", document.RootElement.GetProperty("Other").GetString());
        Assert.False(
            document.RootElement.TryGetProperty(
                "Flourish:Profile:Credential",
                out _
            )
        );
    }

    [Fact]
    public async Task SaveAsync_WithoutUserSecretsProvider_ThrowsHelpfulException()
    {
        var configuration = new ConfigurationBuilder().Build();
        var sut = new ProfileSecretStore(configuration);
        var credentials = StoredProfileCredentials.Create(
            "Ada",
            "Lovelace",
            "secret",
            imagePath: null,
            rememberLogin: true
        );

        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            sut.SaveAsync(credentials)
        );

        Assert.Contains("<UserSecretsId>", exception.Message);
    }

    [Fact]
    public async Task SaveAsync_WhenUserSecretsJsonIsCorrupt_DoesNotOverwriteTheFile()
    {
        using var directory = new TemporaryDirectory();
        var secretPath = Path.Combine(directory.Path, "secrets.json");
        await File.WriteAllTextAsync(secretPath, "{}");
        using var fileProvider = new PhysicalFileProvider(directory.Path);
        var configuration = new ConfigurationBuilder()
            .AddJsonFile(
                fileProvider,
                "secrets.json",
                optional: true,
                reloadOnChange: false
            )
            .Build();
        var sut = new ProfileSecretStore(configuration);
        const string corruptJson = "{ invalid json";
        await File.WriteAllTextAsync(secretPath, corruptJson);
        var credentials = StoredProfileCredentials.Create(
            "Ada",
            "Lovelace",
            "secret",
            imagePath: null,
            rememberLogin: true
        );

        await Assert.ThrowsAsync<InvalidDataException>(() => sut.SaveAsync(credentials));

        Assert.Equal(corruptJson, await File.ReadAllTextAsync(secretPath));
    }

    [Fact]
    public async Task SaveAndClearAsync_PreserveAnExistingHierarchicalSecretSection()
    {
        using var directory = new TemporaryDirectory();
        var secretPath = Path.Combine(directory.Path, "secrets.json");
        await File.WriteAllTextAsync(
            secretPath,
            """
            {
              "Flourish": {
                "Profile": {
                  "Other": "value"
                }
              }
            }
            """
        );
        using var fileProvider = new PhysicalFileProvider(directory.Path);
        var configuration = new ConfigurationBuilder()
            .AddJsonFile(
                fileProvider,
                "secrets.json",
                optional: true,
                reloadOnChange: false
            )
            .Build();
        var sut = new ProfileSecretStore(configuration);
        var credentials = StoredProfileCredentials.Create(
            "Ada",
            "Lovelace",
            "secret",
            imagePath: null,
            rememberLogin: true
        );

        await sut.SaveAsync(credentials);

        using (var document = JsonDocument.Parse(await File.ReadAllTextAsync(secretPath)))
        {
            var profile = document.RootElement
                .GetProperty("Flourish")
                .GetProperty("Profile");
            Assert.Equal("value", profile.GetProperty("Other").GetString());
            Assert.True(profile.TryGetProperty("Credential", out _));
            Assert.False(
                document.RootElement.TryGetProperty(
                    "Flourish:Profile:Credential",
                    out _
                )
            );
        }

        await sut.ClearAsync();

        using var clearedDocument = JsonDocument.Parse(
            await File.ReadAllTextAsync(secretPath)
        );
        var clearedProfile = clearedDocument.RootElement
            .GetProperty("Flourish")
            .GetProperty("Profile");
        Assert.Equal("value", clearedProfile.GetProperty("Other").GetString());
        Assert.False(clearedProfile.TryGetProperty("Credential", out _));
    }

    private sealed class TemporaryDirectory : IDisposable
    {
        public TemporaryDirectory()
        {
            Path = System.IO.Path.Combine(
                System.IO.Path.GetTempPath(),
                "Flourish.Test",
                Guid.NewGuid().ToString("N")
            );
            Directory.CreateDirectory(Path);
        }

        public string Path { get; }

        public void Dispose()
        {
            if (Directory.Exists(Path))
            {
                Directory.Delete(Path, recursive: true);
            }
        }
    }
}
