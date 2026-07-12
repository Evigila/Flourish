using System.IO;
using System.Text;
using System.Text.Json;
using ArkheideSystem.Flourish.Abstract;
using ArkheideSystem.Flourish.Internal.Configuration;
using ArkheideSystem.Flourish.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Primitives;
using Moq;

namespace ArkheideSystem.Flourish.Test.Services;

public sealed class AppPreferenceServiceTests
{
    [Fact]
    public void ReadTheme_WhenConfigurationValueDoesNotExist_ReturnsNull()
    {
        using var directory = new TemporaryDirectory();
        using var sut = CreateService(directory.Path);

        var theme = sut.ReadTheme();

        Assert.Null(theme);
    }

    [Theory]
    [InlineData("System", FlourishTheme.System)]
    [InlineData("light", FlourishTheme.Light)]
    [InlineData("DARK", FlourishTheme.Dark)]
    public void ReadTheme_UsesHostConfiguration(string value, FlourishTheme expected)
    {
        using var directory = new TemporaryDirectory();
        WriteAppSettings(
            directory.Path,
            $$"""
            {
              "Flourish": {
                "Preferences": {
                  "Theme": "{{value}}"
                }
              }
            }
            """
        );
        using var sut = CreateService(directory.Path);

        var theme = sut.ReadTheme();

        Assert.Equal(expected, theme);
    }

    [Fact]
    public void ReadTheme_WhenConfigurationValueIsInvalid_ReturnsNull()
    {
        using var directory = new TemporaryDirectory();
        WriteAppSettings(
            directory.Path,
            """
            {
              "Flourish": {
                "Preferences": {
                  "Theme": "Sepia"
                }
              }
            }
            """
        );
        using var sut = CreateService(directory.Path);

        var theme = sut.ReadTheme();

        Assert.Null(theme);
    }

    [Fact]
    public async Task SaveTheme_CreatesAppSettingsAndRoundTripsThroughHostConfiguration()
    {
        using var directory = new TemporaryDirectory();
        using var sut = CreateService(directory.Path);

        sut.SaveTheme(FlourishTheme.Dark);
        await sut.FlushThemeSavesAsync();

        Assert.True(File.Exists(sut.AppSettingsFilePath));
        Assert.Equal(FlourishTheme.Dark, sut.ReadTheme());
        Assert.Empty(
            Directory.EnumerateFiles(directory.Path, ".appsettings.json.*.tmp")
        );

        using var document = JsonDocument.Parse(File.ReadAllText(sut.AppSettingsFilePath));
        Assert.Equal(
            "Dark",
            document.RootElement
                .GetProperty("Flourish")
                .GetProperty("Preferences")
                .GetProperty("Theme")
                .GetString()
        );
    }

    [Fact]
    public async Task SaveTheme_PreservesUnrelatedAppSettingsAndExistingPropertyCasing()
    {
        using var directory = new TemporaryDirectory();
        WriteAppSettings(
            directory.Path,
            """
            {
              "Logging": {
                "LogLevel": {
                  "Default": "Information"
                }
              },
              "flourish": {
                "FeatureFlag": true,
                "preferences": {
                  "WindowMode": "Compact"
                }
              }
            }
            """
        );
        using var sut = CreateService(directory.Path);

        sut.SaveTheme(FlourishTheme.System);
        await sut.FlushThemeSavesAsync();

        using var document = JsonDocument.Parse(File.ReadAllText(sut.AppSettingsFilePath));
        Assert.Equal(
            "Information",
            document.RootElement
                .GetProperty("Logging")
                .GetProperty("LogLevel")
                .GetProperty("Default")
                .GetString()
        );
        var flourish = document.RootElement.GetProperty("flourish");
        Assert.True(flourish.GetProperty("FeatureFlag").GetBoolean());
        var preferences = flourish.GetProperty("preferences");
        Assert.Equal("Compact", preferences.GetProperty("WindowMode").GetString());
        Assert.Equal("System", preferences.GetProperty("Theme").GetString());
    }

    [Fact]
    public async Task SaveTheme_WhenAppSettingsContainsInvalidJson_DoesNotOverwriteFile()
    {
        using var directory = new TemporaryDirectory();
        using var sut = CreateService(directory.Path);
        const string invalidJson = "{ invalid json";
        WriteAppSettings(directory.Path, invalidJson);

        sut.SaveTheme(FlourishTheme.Light);
        var exception = await Assert.ThrowsAsync<InvalidDataException>(() =>
            sut.FlushThemeSavesAsync().AsTask()
        );

        Assert.Contains("invalid JSON", exception.Message);
        Assert.Equal(invalidJson, File.ReadAllText(sut.AppSettingsFilePath));
    }

    [Fact]
    public async Task SaveTheme_WhenFlourishSectionIsNotAnObject_DoesNotOverwriteFile()
    {
        using var directory = new TemporaryDirectory();
        const string originalJson = """
            {
              "Flourish": "invalid section"
            }
            """;
        WriteAppSettings(directory.Path, originalJson);
        using var sut = CreateService(directory.Path);

        sut.SaveTheme(FlourishTheme.Light);
        var exception = await Assert.ThrowsAsync<InvalidDataException>(() =>
            sut.FlushThemeSavesAsync().AsTask()
        );

        Assert.Contains("Flourish", exception.Message);
        Assert.Equal(originalJson, File.ReadAllText(sut.AppSettingsFilePath));
    }

    [Fact]
    public async Task SaveTheme_WhenCalledConcurrently_LeavesValidAppSettings()
    {
        using var directory = new TemporaryDirectory();
        using var sut = CreateService(directory.Path);
        var themes = new[]
        {
            FlourishTheme.System,
            FlourishTheme.Light,
            FlourishTheme.Dark,
        };

        Parallel.For(0, 24, index => sut.SaveTheme(themes[index % themes.Length]));
        await sut.FlushThemeSavesAsync();

        using var document = JsonDocument.Parse(File.ReadAllText(sut.AppSettingsFilePath));
        var persistedTheme = document.RootElement
            .GetProperty("Flourish")
            .GetProperty("Preferences")
            .GetProperty("Theme")
            .GetString();
        Assert.True(Enum.TryParse<FlourishTheme>(persistedTheme, out var parsedTheme));
        Assert.Contains(parsedTheme, themes);
        Assert.Empty(
            Directory.EnumerateFiles(directory.Path, ".appsettings.json.*.tmp")
        );
    }

    [Fact]
    public async Task UpdateAsync_AppliesTransactionAtomicallyAndReloadsConfiguration()
    {
        using var directory = new TemporaryDirectory();
        WriteAppSettings(
            directory.Path,
            """
            {
              "Feature": {
                "Existing": 1,
                "Items": ["first"],
                "RemoveMe": true
              }
            }
            """
        );
        using var sut = CreateService(directory.Path);

        var result = await sut.UpdateAsync(editor =>
        {
            editor.Set("Feature:Enabled", true);
            editor.Set<object?>("Feature:NullValue", null);
            editor.Merge("Feature", new { Existing = 2, Added = "value" });
            editor.Append("Feature:Items", "second");
            editor.Remove("Feature:RemoveMe");
        });

        Assert.True(result.Changed);
        Assert.True(result.ConfigurationReloaded);
        Assert.Equal(sut.AppSettingsFilePath, result.FilePath);
        Assert.Empty(
            Directory.EnumerateFiles(directory.Path, ".appsettings.json.*.tmp")
        );
        using var document = JsonDocument.Parse(File.ReadAllText(result.FilePath));
        var feature = document.RootElement.GetProperty("Feature");
        Assert.True(feature.GetProperty("Enabled").GetBoolean());
        Assert.Equal(JsonValueKind.Null, feature.GetProperty("NullValue").ValueKind);
        Assert.Equal(2, feature.GetProperty("Existing").GetInt32());
        Assert.Equal("value", feature.GetProperty("Added").GetString());
        Assert.Equal(
            new string?[] { "first", "second" },
            feature
                .GetProperty("Items")
                .EnumerateArray()
                .Select(item => item.GetString())
                .ToArray()
        );
        Assert.False(feature.TryGetProperty("RemoveMe", out _));
    }

    [Fact]
    public async Task UpdateAsync_WhenTransactionDoesNotChangeAnything_DoesNotCreateFile()
    {
        using var directory = new TemporaryDirectory();
        using var sut = CreateService(directory.Path);

        var result = await sut.RemoveAsync("Missing:Value");

        Assert.False(result.Changed);
        Assert.False(result.ConfigurationReloaded);
        Assert.False(File.Exists(result.FilePath));
    }

    [Fact]
    public async Task MergeAsync_WhenTargetIsNotAnObject_PreservesOriginalFile()
    {
        using var directory = new TemporaryDirectory();
        const string originalJson = "{ \"Feature\": 1 }";
        WriteAppSettings(directory.Path, originalJson);
        using var sut = CreateService(directory.Path);

        await Assert.ThrowsAsync<InvalidDataException>(async () =>
            await sut.MergeAsync("Feature", new { Enabled = true })
        );

        Assert.Equal(originalJson, File.ReadAllText(sut.AppSettingsFilePath));
    }

    [Fact]
    public async Task UpdateAsync_EditorCannotBeUsedAfterTransactionCompletes()
    {
        using var directory = new TemporaryDirectory();
        using var sut = CreateService(directory.Path);
        IAppSettingsEditor? capturedEditor = null;

        await sut.UpdateAsync(editor =>
        {
            capturedEditor = editor;
            editor.Set("Feature:Value", 1);
        });

        Assert.NotNull(capturedEditor);
        Assert.Throws<ObjectDisposedException>(() =>
            capturedEditor.Set("Feature:Value", 2)
        );
    }

    [Fact]
    public async Task ConcurrentUpdates_AreSerializedAndTheLaterCallWins()
    {
        using var directory = new TemporaryDirectory();
        using var sut = CreateService(directory.Path);
        using var firstEntered = new ManualResetEventSlim();
        using var releaseFirst = new ManualResetEventSlim();
        var first = sut
            .UpdateAsync(editor =>
            {
                firstEntered.Set();
                releaseFirst.Wait();
                editor.Set("Feature:First", true);
                editor.Set("Feature:Shared", "first");
            })
            .AsTask();
        Task<AppSettingsUpdateResult>? second = null;

        try
        {
            Assert.True(firstEntered.Wait(TimeSpan.FromSeconds(5)));
            second = sut
                .UpdateAsync(editor =>
                {
                    editor.Set("Feature:Second", true);
                    editor.Set("Feature:Shared", "second");
                })
                .AsTask();
            Assert.False(first.IsCompleted);
            Assert.False(second.IsCompleted);
        }
        finally
        {
            releaseFirst.Set();
        }

        await Task.WhenAll(first, second!);

        using var document = JsonDocument.Parse(File.ReadAllText(sut.FilePath));
        var feature = document.RootElement.GetProperty("Feature");
        Assert.True(feature.GetProperty("First").GetBoolean());
        Assert.True(feature.GetProperty("Second").GetBoolean());
        Assert.Equal("second", feature.GetProperty("Shared").GetString());
    }

    [Fact]
    public async Task UpdateAsync_CanceledWhileQueued_DoesNotInvokeItsEditor()
    {
        using var directory = new TemporaryDirectory();
        using var sut = CreateService(directory.Path);
        using var firstEntered = new ManualResetEventSlim();
        using var releaseFirst = new ManualResetEventSlim();
        var first = sut
            .UpdateAsync(editor =>
            {
                firstEntered.Set();
                releaseFirst.Wait();
                editor.Set("Feature:First", true);
            })
            .AsTask();
        using var cancellation = new CancellationTokenSource();
        var editorInvoked = false;
        Task<AppSettingsUpdateResult>? canceled = null;

        try
        {
            Assert.True(firstEntered.Wait(TimeSpan.FromSeconds(5)));
            canceled = sut
                .UpdateAsync(
                    editor =>
                    {
                        editorInvoked = true;
                        editor.Set("Feature:Canceled", true);
                    },
                    cancellation.Token
                )
                .AsTask();
            cancellation.Cancel();
        }
        finally
        {
            releaseFirst.Set();
        }

        await first;
        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => canceled!);
        var final = await sut.SetAsync("Feature:Final", true);

        Assert.True(final.Changed);
        Assert.False(editorInvoked);
        using var document = JsonDocument.Parse(File.ReadAllText(sut.FilePath));
        var feature = document.RootElement.GetProperty("Feature");
        Assert.False(feature.TryGetProperty("Canceled", out _));
        Assert.True(feature.GetProperty("Final").GetBoolean());
    }

    [Fact]
    public async Task UpdateAsync_CompletesAfterTargetedConfigurationNotification()
    {
        using var directory = new TemporaryDirectory();
        var unrelatedSource = new CountingConfigurationSource();
        var configuration = new ConfigurationBuilder()
            .SetBasePath(directory.Path)
            .Add(
                new FlourishAppSettingsConfigurationSource
                {
                    Path = "appsettings.json",
                    Optional = true,
                    ReloadOnChange = false,
                    WatchForChanges = false,
                }
            )
            .Add(unrelatedSource)
            .Build();
        var hostEnvironment = new Mock<IHostEnvironment>();
        hostEnvironment.SetupGet(environment => environment.ContentRootPath)
            .Returns(directory.Path);
        using var sut = new AppPreferenceService(
            configuration,
            hostEnvironment.Object
        );
        using var runtimeConfiguration = new FlourishConfigurationService(configuration);
        var changeCount = 0;
        string? valueObservedByEvent = null;
        runtimeConfiguration.Changed += (_, _) =>
        {
            changeCount++;
            valueObservedByEvent = runtimeConfiguration["Feature:Value"];
        };

        var result = await sut.SetAsync("Feature:Value", "updated");

        Assert.True(result.ConfigurationReloaded);
        Assert.Equal("updated", configuration["Feature:Value"]);
        Assert.Equal("updated", runtimeConfiguration.Current["Feature:Value"]);
        Assert.Equal("updated", valueObservedByEvent);
        Assert.Equal(1, changeCount);
        Assert.Equal(1, unrelatedSource.Provider.LoadCount);
    }

    [Fact]
    public async Task TargetedReload_PreservesHigherPriorityConfigurationValues()
    {
        using var directory = new TemporaryDirectory();
        var configuration = new ConfigurationBuilder()
            .SetBasePath(directory.Path)
            .Add(
                new FlourishAppSettingsConfigurationSource
                {
                    Path = "appsettings.json",
                    Optional = true,
                    ReloadOnChange = false,
                    WatchForChanges = false,
                }
            )
            .AddInMemoryCollection(
                new Dictionary<string, string?>
                {
                    ["Feature:Value"] = "higher-priority",
                }
            )
            .Build();
        var hostEnvironment = new Mock<IHostEnvironment>();
        hostEnvironment.SetupGet(environment => environment.ContentRootPath)
            .Returns(directory.Path);
        using var sut = new AppPreferenceService(
            configuration,
            hostEnvironment.Object
        );

        var result = await sut.SetAsync("Feature:Value", "base-value");

        Assert.True(result.ConfigurationReloaded);
        Assert.Equal("higher-priority", configuration["Feature:Value"]);
        using var document = JsonDocument.Parse(File.ReadAllText(sut.FilePath));
        Assert.Equal(
            "base-value",
            document.RootElement.GetProperty("Feature").GetProperty("Value").GetString()
        );
    }

    [Fact]
    public async Task UpdateAsync_RejectsANestedTransactionWithoutStoppingTheWorker()
    {
        using var directory = new TemporaryDirectory();
        using var sut = CreateService(directory.Path);

        var error = await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            await sut.UpdateAsync(_ =>
                sut.SetAsync("Nested:Value", true).GetAwaiter().GetResult()
            )
        );
        var recovery = await sut.SetAsync("Feature:Recovered", true);

        Assert.Contains("cannot start another transaction", error.Message);
        Assert.True(recovery.Changed);
    }

    [Fact]
    public async Task UpdateAsync_RejectsANestedTransactionAcrossTaskRun()
    {
        using var directory = new TemporaryDirectory();
        using var sut = CreateService(directory.Path);

        var error = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            sut.UpdateAsync(_ =>
                    Task.Run(() => sut.SetAsync("Nested:Value", true).AsTask())
                        .WaitAsync(TimeSpan.FromSeconds(1))
                        .GetAwaiter()
                        .GetResult()
                )
                .AsTask()
        );

        Assert.Contains("cannot start another transaction", error.Message);
        Assert.False(File.Exists(sut.FilePath));
    }

    [Fact]
    public async Task HostedService_CanRestartAndAcceptNewTransactions()
    {
        using var directory = new TemporaryDirectory();
        using var sut = CreateService(directory.Path);

        var beforeRestart = await sut
            .SetAsync("Feature:BeforeRestart", true)
            .AsTask()
            .WaitAsync(TimeSpan.FromSeconds(5));
        await sut.StopAsync(CancellationToken.None);
        await sut.StartAsync(CancellationToken.None);
        var afterRestart = await sut
            .SetAsync("Feature:AfterRestart", true)
            .AsTask()
            .WaitAsync(TimeSpan.FromSeconds(5));

        Assert.True(beforeRestart.Changed);
        Assert.True(afterRestart.Changed);
        using var document = JsonDocument.Parse(File.ReadAllText(sut.FilePath));
        Assert.True(
            document.RootElement
                .GetProperty("Feature")
                .GetProperty("BeforeRestart")
                .GetBoolean()
        );
        Assert.True(
            document.RootElement
                .GetProperty("Feature")
                .GetProperty("AfterRestart")
                .GetBoolean()
        );
    }

    [Fact]
    public async Task ConfigurationChanged_RejectsAReentrantTransactionWithoutDeadlocking()
    {
        using var directory = new TemporaryDirectory();
        var configuration = CreateConfiguration(directory.Path);
        using var configurationDisposal = (IDisposable)configuration;
        var hostEnvironment = new Mock<IHostEnvironment>();
        hostEnvironment.SetupGet(environment => environment.ContentRootPath)
            .Returns(directory.Path);
        using var sut = new AppPreferenceService(
            configuration,
            hostEnvironment.Object
        );
        using var runtimeConfiguration = new FlourishConfigurationService(configuration);
        Exception? reentrantError = null;
        var callbackInvoked = 0;
        runtimeConfiguration.Changed += (_, _) =>
        {
            if (Interlocked.Exchange(ref callbackInvoked, 1) != 0)
            {
                return;
            }

            reentrantError = Record.Exception(() =>
                sut.SetAsync("Feature:Nested", true)
                    .AsTask()
                    .WaitAsync(TimeSpan.FromSeconds(1))
                    .GetAwaiter()
                    .GetResult()
            );
        };

        var result = await sut
            .SetAsync("Feature:Value", "updated")
            .AsTask()
            .WaitAsync(TimeSpan.FromSeconds(5));

        Assert.True(result.Changed);
        var error = Assert.IsType<InvalidOperationException>(reentrantError);
        Assert.Contains("cannot start another transaction", error.Message);
        Assert.Null(configuration["Feature:Nested"]);
    }

    [Fact]
    public async Task RapidThemeChanges_CoalesceAndPersistTheLatestTheme()
    {
        using var directory = new TemporaryDirectory();
        var configuration = CreateConfiguration(directory.Path);
        var hostEnvironment = new Mock<IHostEnvironment>();
        hostEnvironment.SetupGet(environment => environment.ContentRootPath)
            .Returns(directory.Path);
        using var sut = new AppPreferenceService(
            configuration,
            hostEnvironment.Object
        );
        using var runtimeConfiguration = new FlourishConfigurationService(configuration);
        var changeCount = 0;
        runtimeConfiguration.Changed += (_, _) => changeCount++;
        using var blockerEntered = new ManualResetEventSlim();
        using var releaseBlocker = new ManualResetEventSlim();
        var blocker = sut
            .UpdateAsync(_ =>
            {
                blockerEntered.Set();
                releaseBlocker.Wait();
            })
            .AsTask();
        Task? flush = null;

        try
        {
            Assert.True(blockerEntered.Wait(TimeSpan.FromSeconds(5)));
            sut.SaveTheme(FlourishTheme.System);
            sut.SaveTheme(FlourishTheme.Light);
            sut.SaveTheme(FlourishTheme.Dark);
            flush = sut.FlushThemeSavesAsync().AsTask();
            Assert.False(flush.IsCompleted);
        }
        finally
        {
            releaseBlocker.Set();
        }

        await blocker;
        await flush!;

        Assert.Equal("Dark", configuration["Flourish:Preferences:Theme"]);
        Assert.Equal(1, changeCount);
    }

    [Fact]
    public async Task ThemeCoalescing_PersistsTheLastSuccessfullyAppliedTheme()
    {
        using var directory = new TemporaryDirectory();
        using var sut = CreateService(directory.Path);
        using var blockerEntered = new ManualResetEventSlim();
        using var releaseBlocker = new ManualResetEventSlim();
        var blocker = sut
            .UpdateAsync(_ =>
            {
                blockerEntered.Set();
                releaseBlocker.Wait();
            })
            .AsTask();
        Task? flush = null;

        try
        {
            Assert.True(blockerEntered.Wait(TimeSpan.FromSeconds(5)));
            sut.QueueThemeSave(FlourishTheme.Light, Task.FromResult(true));
            sut.QueueThemeSave(FlourishTheme.Dark, Task.FromResult(false));
            flush = sut.FlushThemeSavesAsync().AsTask();
        }
        finally
        {
            releaseBlocker.Set();
        }

        await blocker.WaitAsync(TimeSpan.FromSeconds(5));
        await flush!.WaitAsync(TimeSpan.FromSeconds(5));

        Assert.Equal(FlourishTheme.Light, sut.ReadTheme());
    }

    [Fact]
    public async Task ThemePersistenceFailure_DoesNotStopLaterQueuedWrites()
    {
        using var directory = new TemporaryDirectory();
        const string invalidJson = "{ invalid json";
        using var sut = CreateService(directory.Path);
        WriteAppSettings(directory.Path, invalidJson);

        sut.SaveTheme(FlourishTheme.Dark);
        await Assert.ThrowsAsync<InvalidDataException>(() =>
            sut.FlushThemeSavesAsync().AsTask()
        );
        Assert.Equal(invalidJson, File.ReadAllText(sut.FilePath));

        WriteAppSettings(directory.Path, "{}");
        sut.SaveTheme(FlourishTheme.Light);
        await sut.FlushThemeSavesAsync();

        using var document = JsonDocument.Parse(File.ReadAllText(sut.FilePath));
        Assert.Equal(
            "Light",
            document.RootElement
                .GetProperty("Flourish")
                .GetProperty("Preferences")
                .GetProperty("Theme")
                .GetString()
        );
    }

    [Fact]
    public async Task ExternalAppSettingsChange_ReloadsTheTargetProvider()
    {
        using var directory = new TemporaryDirectory();
        WriteAppSettings(
            directory.Path,
            """
            {
              "Feature": {
                "Value": "before"
              }
            }
            """
        );
        var configuration = new ConfigurationBuilder()
            .SetBasePath(directory.Path)
            .Add(
                new FlourishAppSettingsConfigurationSource
                {
                    Path = "appsettings.json",
                    Optional = true,
                    ReloadDelay = 20,
                    ReloadOnChange = false,
                    WatchForChanges = true,
                }
            )
            .Build();
        using var configurationDisposal = (IDisposable)configuration;
        var completion = new TaskCompletionSource<string?>(
            TaskCreationOptions.RunContinuationsAsynchronously
        );
        var changeCount = 0;
        using var subscription = ChangeToken.OnChange(
            configuration.GetReloadToken,
            () =>
            {
                Interlocked.Increment(ref changeCount);
                completion.TrySetResult(configuration["Feature:Value"]);
            }
        );
        var replacementPath = Path.Combine(directory.Path, ".external-appsettings.tmp");
        File.WriteAllText(
            replacementPath,
            """
            {
              "Feature": {
                "Value": "after"
              }
            }
            """
        );

        File.Move(
            replacementPath,
            Path.Combine(directory.Path, "appsettings.json"),
            overwrite: true
        );

        Assert.Equal(
            "after",
            await completion.Task.WaitAsync(TimeSpan.FromSeconds(5))
        );
        Assert.Equal("after", configuration["Feature:Value"]);
        Assert.True(Volatile.Read(ref changeCount) >= 1);
    }

    [Fact]
    public async Task ReapplyingThePersistedContent_DoesNotRaiseASecondChange()
    {
        using var directory = new TemporaryDirectory();
        var configuration = new ConfigurationBuilder()
            .SetBasePath(directory.Path)
            .Add(
                new FlourishAppSettingsConfigurationSource
                {
                    Path = "appsettings.json",
                    Optional = true,
                    ReloadDelay = 20,
                    ReloadOnChange = false,
                    WatchForChanges = false,
                }
            )
            .Build();
        using var configurationDisposal = (IDisposable)configuration;
        var hostEnvironment = new Mock<IHostEnvironment>();
        hostEnvironment.SetupGet(environment => environment.ContentRootPath)
            .Returns(directory.Path);
        using var sut = new AppPreferenceService(
            configuration,
            hostEnvironment.Object
        );
        var changeCount = 0;
        using var subscription = ChangeToken.OnChange(
            configuration.GetReloadToken,
            () => Interlocked.Increment(ref changeCount)
        );

        var result = await sut.SetAsync("Feature:Value", "updated");
        var provider = Assert.Single(
            configuration.Providers.OfType<FlourishAppSettingsConfigurationProvider>()
        );
        Assert.True(provider.Apply(File.ReadAllBytes(sut.FilePath)));

        Assert.True(result.ConfigurationReloaded);
        Assert.Equal("updated", configuration["Feature:Value"]);
        Assert.Equal(1, Volatile.Read(ref changeCount));
    }

    [Fact]
    public async Task ExternalInvalidJson_InvokesLoadHandlerAndLaterRecovers()
    {
        using var directory = new TemporaryDirectory();
        WriteAppSettings(
            directory.Path,
            """
            {
              "Feature": {
                "Value": "before"
              }
            }
            """
        );
        var loadError = new TaskCompletionSource<Exception>(
            TaskCreationOptions.RunContinuationsAsynchronously
        );
        var source = new FlourishAppSettingsConfigurationSource
        {
            Path = "appsettings.json",
            Optional = true,
            ReloadDelay = 20,
            ReloadOnChange = false,
            WatchForChanges = true,
            OnLoadException = context =>
            {
                context.Ignore = true;
                loadError.TrySetResult(context.Exception);
            },
        };
        var configuration = new ConfigurationBuilder()
            .SetBasePath(directory.Path)
            .Add(source)
            .Build();
        using var configurationDisposal = (IDisposable)configuration;

        ReplaceAppSettings(directory.Path, "{ invalid json", "invalid");

        var error = await loadError.Task.WaitAsync(TimeSpan.FromSeconds(5));
        Assert.IsType<FormatException>(error);
        Assert.Equal("before", configuration["Feature:Value"]);

        var recovered = new TaskCompletionSource<string?>(
            TaskCreationOptions.RunContinuationsAsynchronously
        );
        using var subscription = ChangeToken.OnChange(
            configuration.GetReloadToken,
            () => recovered.TrySetResult(configuration["Feature:Value"])
        );
        ReplaceAppSettings(
            directory.Path,
            """
            {
              "Feature": {
                "Value": "after"
              }
            }
            """,
            "recovered"
        );

        Assert.Equal(
            "after",
            await recovered.Task.WaitAsync(TimeSpan.FromSeconds(5))
        );
    }

    [Fact]
    public void TargetedApply_DoesNotOverwriteNewerFileContent()
    {
        using var directory = new TemporaryDirectory();
        WriteAppSettings(directory.Path, "{}");
        var configuration = CreateConfiguration(directory.Path);
        using var configurationDisposal = (IDisposable)configuration;
        var provider = Assert.Single(
            configuration.Providers.OfType<FlourishAppSettingsConfigurationProvider>()
        );
        const string staleContent =
            """
            {
              "Feature": {
                "Value": "stale"
              }
            }
            """;
        const string newerContent =
            """
            {
              "Feature": {
                "Value": "newer"
              }
            }
            """;
        ReplaceAppSettings(directory.Path, newerContent, "newer");

        Assert.True(provider.Apply(Encoding.UTF8.GetBytes(staleContent)));

        Assert.Equal("newer", configuration["Feature:Value"]);
    }

    private static AppPreferenceService CreateService(string contentRootPath)
    {
        var configuration = CreateConfiguration(contentRootPath);
        var hostEnvironment = new Mock<IHostEnvironment>();
        hostEnvironment.SetupGet(environment => environment.ContentRootPath)
            .Returns(contentRootPath);
        return new AppPreferenceService(configuration, hostEnvironment.Object);
    }

    private static IConfigurationRoot CreateConfiguration(string contentRootPath)
    {
        return new ConfigurationBuilder()
            .SetBasePath(contentRootPath)
            .Add(
                new FlourishAppSettingsConfigurationSource
                {
                    Path = "appsettings.json",
                    Optional = true,
                    ReloadOnChange = false,
                    WatchForChanges = false,
                }
            )
            .Build();
    }

    private static void WriteAppSettings(string directoryPath, string json)
    {
        File.WriteAllText(Path.Combine(directoryPath, "appsettings.json"), json);
    }

    private static void ReplaceAppSettings(
        string directoryPath,
        string json,
        string temporaryName
    )
    {
        var temporaryPath = Path.Combine(directoryPath, $".{temporaryName}.tmp");
        File.WriteAllText(temporaryPath, json);
        File.Move(
            temporaryPath,
            Path.Combine(directoryPath, "appsettings.json"),
            overwrite: true
        );
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

    private sealed class CountingConfigurationSource : IConfigurationSource
    {
        public CountingConfigurationProvider Provider { get; } = new();

        public IConfigurationProvider Build(IConfigurationBuilder builder)
        {
            return Provider;
        }
    }

    private sealed class CountingConfigurationProvider : ConfigurationProvider
    {
        public int LoadCount { get; private set; }

        public override void Load()
        {
            LoadCount++;
        }
    }
}
