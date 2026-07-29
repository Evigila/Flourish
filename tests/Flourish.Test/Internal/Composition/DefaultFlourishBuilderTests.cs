using System.IO;
using System.Reflection;
using System.Reflection.Emit;
using System.Windows.Controls;
using ArkheideSystem.Flourish.Abstract;
using ArkheideSystem.Flourish.Internal.Composition;
using ArkheideSystem.Flourish.Internal.Configuration;
using ArkheideSystem.Flourish.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.CommandLine;
using Microsoft.Extensions.Configuration.EnvironmentVariables;
using Microsoft.Extensions.Configuration.Json;
using Microsoft.Extensions.Configuration.Memory;
using Microsoft.Extensions.Configuration.UserSecrets;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace ArkheideSystem.Flourish.Test.Internal.Composition;

public sealed class DefaultFlourishBuilderTests
{
    [Fact]
    public void ConfigureMethods_WithNullCallbacks_ThrowArgumentNullExceptionImmediately()
    {
        var builder = FlourishBuilder.CreateDefaultBuilder([]);

        Assert.Throws<ArgumentNullException>(() => builder.ConfigData(null!));
        Assert.Throws<ArgumentNullException>(() =>
            builder.ConfigConfiguration(null!)
        );
        Assert.Throws<ArgumentNullException>(() => builder.ConfigServices(null!));
        Assert.Throws<ArgumentNullException>(() => builder.ConfigShell(null!));
        Assert.Throws<ArgumentNullException>(() => builder.ConfigTitleBar(null!));
        Assert.Throws<ArgumentNullException>(() => builder.ConfigNavigation(null!));
        Assert.Throws<ArgumentNullException>(() => builder.ConfigCustomHandler(null!));
        Assert.Throws<ArgumentNullException>(() => builder.ConfigDynamicToolbar(null!));
        Assert.Throws<ArgumentNullException>(() => builder.ConfigMotion(null!));
        Assert.Throws<ArgumentNullException>(() => builder.ConfigWindow(null!));
        Assert.Throws<ArgumentNullException>(() => builder.ConfigStatusBar(null!));
    }

    [Fact]
    public void Build_FreezesTopLevelBuilderAndCanOnlyRunOnce()
    {
        var builder = FlourishBuilder.CreateDefaultBuilder([]);

        using var flourish = builder.Build();

        Assert.Throws<InvalidOperationException>(() => builder.ConfigShell(_ => { }));
        Assert.Throws<InvalidOperationException>(() =>
            builder.ConfigConfiguration((_, _) => { })
        );
        Assert.Throws<InvalidOperationException>(() => builder.Build());
    }

    [Fact]
    public void Build_AppliesRegisteredApplicationConfigurationSource()
    {
        HostBuilderContext? capturedContext = null;
        using var flourish = FlourishBuilder
            .CreateDefaultBuilder([])
            .ConfigConfiguration((context, configuration) =>
            {
                capturedContext = context;
                configuration.AddConfigurationSource(
                    new MemoryConfigurationSource
                    {
                        InitialData =
                        [
                            KeyValuePair.Create<string, string?>(
                                "Flourish:Test:AdditionalConfiguration",
                                "configured"
                            ),
                        ],
                    }
                );
            })
            .Build();

        var configuration = flourish.GetRequiredService<IConfiguration>();

        Assert.NotNull(capturedContext);
        Assert.Equal(
            "configured",
            configuration["Flourish:Test:AdditionalConfiguration"]
        );
    }

    [Fact]
    public void Build_UsesRegisteredConfigurationFile()
    {
        using var directory = new TemporaryDirectory();
        var path = Path.Combine(directory.Path, "appsettings.User.json");
        File.WriteAllText(
            path,
            """{"Application":{"DisplayName":"Foobar"}}"""
        );

        using var flourish = FlourishBuilder
            .CreateDefaultBuilder([])
            .ConfigConfiguration((_, configuration) =>
                configuration.UseConfigurationFile(
                    path,
                    optional: false,
                    reloadOnChange: false
                )
            )
            .Build();

        Assert.Equal(
            "Foobar",
            flourish.GetRequiredService<IConfiguration>()["Application:DisplayName"]
        );
    }

    [Fact]
    public void Build_CommandLineOverridesRegisteredApplicationSource()
    {
        const string key = "Application:Priority";
        using var flourish = FlourishBuilder
            .CreateDefaultBuilder([$"--{key}=command-line"])
            .ConfigConfiguration((_, configuration) =>
                configuration.AddConfigurationSource(
                    new MemoryConfigurationSource
                    {
                        InitialData =
                        [
                            KeyValuePair.Create<string, string?>(key, "registered"),
                        ],
                    }
                )
            )
            .Build();

        Assert.Equal(
            "command-line",
            flourish.GetRequiredService<IConfiguration>()[key]
        );
    }

    [Fact]
    public void Build_RegisteredApplicationSourcesPreserveRegistrationOrder()
    {
        const string key = "Application:RegistrationOrder";
        using var flourish = FlourishBuilder
            .CreateDefaultBuilder([])
            .ConfigConfiguration((_, configuration) =>
                configuration.AddConfigurationSource(
                    new MemoryConfigurationSource
                    {
                        InitialData =
                        [
                            KeyValuePair.Create<string, string?>(key, "first"),
                        ],
                    }
                )
            )
            .ConfigConfiguration((_, configuration) =>
                configuration.AddConfigurationSource(
                    new MemoryConfigurationSource
                    {
                        InitialData =
                        [
                            KeyValuePair.Create<string, string?>(key, "second"),
                        ],
                    }
                )
            )
            .Build();

        Assert.Equal(
            "second",
            flourish.GetRequiredService<IConfiguration>()[key]
        );
    }

    [Fact]
    public void Build_FreezesEachConfigurationBuilderWhenItsCallbackCompletes()
    {
        IFlourishConfigurationBuilder? captured = null;
        var secondCallbackObservedFrozenBuilder = false;

        using var flourish = FlourishBuilder
            .CreateDefaultBuilder([])
            .ConfigConfiguration((_, configuration) => captured = configuration)
            .ConfigConfiguration((_, _) =>
            {
                Assert.Throws<InvalidOperationException>(() =>
                    captured!.AddConfigurationSource(
                        new MemoryConfigurationSource()
                    )
                );
                secondCallbackObservedFrozenBuilder = true;
            })
            .Build();

        Assert.True(secondCallbackObservedFrozenBuilder);
    }

    [Fact]
    public void Build_FreezesCapturedStartupBuildersAfterTheirCallbacksComplete()
    {
        IFlourishDataBuilder? data = null;
        IFlourishConfigurationBuilder? applicationConfiguration = null;
        IFlourishShellBuilder? shell = null;
        IFlourishProfileBuilder? profile = null;
        IFlourishTitlebarBuilder? titleBar = null;
        IFlourishNavigationBuilder? navigation = null;
        IFlourishNavigationGroupBuilder? navigationGroup = null;
        IFlourishCustomHandlerBuilder? customHandler = null;
        IFlourishDynamicToolbarBuilder? toolbar = null;
        IFlourishMotionBuilder? motion = null;
        IFlourishWindowPropertyBuilder? window = null;
        IFlourishStatusBarBuilder? statusBar = null;

        var builder = FlourishBuilder
            .CreateDefaultBuilder([])
            .ConfigData(value => data = value)
            .ConfigConfiguration((_, value) => applicationConfiguration = value)
            .ConfigShell(value => shell = value)
            .ConfigProfile(value => profile = value)
            .ConfigTitleBar(value => titleBar = value)
            .ConfigNavigation(value =>
            {
                navigation = value;
                value.AddGroup(configureGroup: group => navigationGroup = group);
            })
            .ConfigCustomHandler(value => customHandler = value)
            .ConfigDynamicToolbar(value => toolbar = value)
            .ConfigMotion(value => motion = value)
            .ConfigWindow(value => window = value)
            .ConfigStatusBar(value => statusBar = value);

        using var flourish = builder.Build();

        Assert.Throws<InvalidOperationException>(() => data!.InitLocale());
        Assert.Throws<InvalidOperationException>(() =>
            applicationConfiguration!.AddConfigurationSource(
                new MemoryConfigurationSource()
            )
        );
        Assert.Throws<InvalidOperationException>(() => shell!.UseTitleBar());
        Assert.Throws<InvalidOperationException>(() => profile!.InitProfilePage<TestPage>());
        Assert.Throws<InvalidOperationException>(() => titleBar!.InitApplicationTitle());
        Assert.Throws<InvalidOperationException>(() => navigation!.InitInitiallyOpen());
        Assert.Throws<InvalidOperationException>(() =>
            navigationGroup!.AddNavigableItem("Late item", null, null)
        );
        Assert.Throws<InvalidOperationException>(() =>
            customHandler!.InitRegionContent(
                FlourishRegion.TitlebarEnd,
                _ => new Border()
            )
        );
        Assert.Throws<InvalidOperationException>(() =>
            toolbar!.InitToolbarItems<TestPage>()
        );
        Assert.Throws<InvalidOperationException>(() => motion!.UsePageTransition());
        Assert.Throws<InvalidOperationException>(() => window!.UseTopmost());
        Assert.Throws<InvalidOperationException>(() => statusBar!.UsePowerStatus());
    }

    [Fact]
    public void Build_AppliesConfigurationCallbacksAndPreservesRegistrationOrder()
    {
        var marker = new object();
        var builder = FlourishBuilder
            .CreateDefaultBuilder([])
            .ConfigData(data => data.InitLocale("zh-CN"))
            .ConfigServices((_, services) => services.AddSingleton(marker))
            .ConfigShell(shell =>
                shell
                    .UseMultiProject()
                    .UseNavigation()
                    .UseCenterContent(enabled: true, contentWidth: 900)
                    .UseTips(enabled: true, delay: 350)
                    .InitGlobalFont("Arial", 13, 15, 17, 19, 22, 28)
                    .UseMaterialEffect(
                        enabled: false,
                        effect: MaterialEffect.None
                    )
                    .UseStatusBar()
            )
            .ConfigShell(shell => shell.UseStatusBar(enabled: false))
            .ConfigTitleBar(titlebar =>
                titlebar
                    .UseLogo(
                        showApplicationTitle: false,
                        showApplicationSubTitle: true,
                        showProjectTitle: true
                    )
                    .InitApplicationTitle("Test Shell")
                    .InitApplicationSubTitle("Test workspace")
                    .InitUnnamedProjectPlaceholder("Untitled project")
                    .UseProfile(nameOrder: NameOrder.LastFirst)
                    .UseThemeToggle(mode: FlourishTheme.Dark)
            )
            .ConfigCustomHandler(custom =>
                custom.InitRegionContent(FlourishRegion.TitlebarStart, _ => null!)
            )
            .ConfigDynamicToolbar(toolbar =>
                toolbar.InitToolbarItems<TestPage>(
                    new FlourishToolbarItem("Refresh", "R", "test.refresh")
                )
            )
            .ConfigMotion(motion => motion.UseSystemReducedMotion(enabled: false))
            .ConfigWindow(window => window.UseTopmost())
            .ConfigStatusBar(statusBar => statusBar.InitStatusItem("Ready", "R"));

        using var flourish = builder.Build();
        var options = flourish.GetRequiredService<FlourishShellOptions>();
        var dataOptions = flourish.GetRequiredService<FlourishDataOptions>();
        var projects = flourish.GetRequiredService<IProjectService>();

        Assert.Same(marker, flourish.GetRequiredService<object>());
        Assert.Equal("zh-CN", dataOptions.Locale);
        Assert.True(options.IsMultiProjectEnabled);
        Assert.True(projects.Current.IsMultiProjectEnabled);
        Assert.Equal(0, projects.Current.Version);
        Assert.True(options.IsNavigationPanelEnabled);
        Assert.True(options.IsCenterContentEnabled);
        Assert.Equal(900, options.CenterContentWidth);
        Assert.False(options.IsStatusBarEnabled);
        Assert.Equal("Test Shell", options.ApplicationTitle);
        Assert.Equal("Test workspace", options.ApplicationSubtitle);
        Assert.Equal("Untitled project", options.UnnamedProjectPlaceholder);
        Assert.True(options.IsTitlebarLogoEnabled);
        Assert.False(options.ShowApplicationTitleInLogoFlyout);
        Assert.True(options.ShowApplicationSubtitleInLogoFlyout);
        Assert.True(options.ShowProjectTitleInLogoFlyout);
        Assert.True(options.IsTitlebarTitleEnabled);
        Assert.True(options.IsProfileEnabled);
        Assert.True(options.IsTitlebarProfileEnabled);
        Assert.Equal(NameOrder.LastFirst, options.Profile.NameOrder);
        Assert.True(options.IsThemeEnabled);
        Assert.True(options.IsTitlebarThemeToggleEnabled);
        Assert.Single(options.RegionContents);
        Assert.Single(options.DynamicToolbarItems[typeof(TestPage)]);
        Assert.Equal(350, options.Tips.InitialShowDelayMilliseconds);
        Assert.False(options.Motion.RespectSystemReducedMotion);
        Assert.True(options.WindowTopmost);
        Assert.Equal("Arial", options.FontFamily);
        Assert.Equal(13, options.FontSizeSmall);
        Assert.Equal(15, options.FontSizeStandard);
        Assert.Equal(17, options.FontSizeIcon);
        Assert.Equal(19, options.FontSizeLarge);
        Assert.Equal(22, options.FontSizeExtraLarge);
        Assert.Equal(28, options.FontSizeHeaderSize);
        Assert.Equal(MaterialEffect.None, options.MaterialEffect);
        Assert.False(options.IsMaterialEffectEnabled);
        Assert.Equal(FlourishTheme.Dark, options.DefaultTheme);
        Assert.Collection(
            options.StatusItems,
            statusItem =>
            {
                Assert.Equal("OK", statusItem.Text);
                Assert.Equal("\uE930", statusItem.IconGlyph);
            },
            statusItem =>
            {
                Assert.Equal("Ready", statusItem.Text);
                Assert.Equal("R", statusItem.IconGlyph);
            }
        );
    }

    [Fact]
    public void Build_UsesExecutableDirectoryAsContentRoot()
    {
        using var flourish = FlourishBuilder.CreateDefaultBuilder([]).Build();

        var environment = flourish.GetRequiredService<IHostEnvironment>();

        Assert.Equal(
            Path.TrimEndingDirectorySeparator(Path.GetFullPath(AppContext.BaseDirectory)),
            Path.TrimEndingDirectorySeparator(Path.GetFullPath(environment.ContentRootPath))
        );
    }

    [Fact]
    public void Build_CustomStoragePathsDriveConfigurationAndStoresIndependently()
    {
        using var directory = new TemporaryDirectory();
        var appSettingsPath = Path.Combine(
            directory.Path,
            "appsettings.Flourish.json"
        );
        var projectCatalogPath = Path.Combine(directory.Path, "catalog", "projects.json");
        File.WriteAllText(
            appSettingsPath,
            """{"Flourish":{"Preferences":{"Locale":"zh-CN"}}}"""
        );

        using var flourish = FlourishBuilder
            .CreateDefaultBuilder([])
            .ConfigData(data => data
                .InitAppSettingsFilePath(appSettingsPath)
                .InitProjectCatalogFilePath(projectCatalogPath))
            .Build();

        var data = flourish.GetRequiredService<FlourishDataOptions>();
        var configuration = flourish.GetRequiredService<IConfiguration>();
        var settings = flourish.GetRequiredService<IFlourishSettingsStore>();
        var catalog = flourish.GetRequiredService<ProjectCatalogStore>();

        Assert.Equal("zh-CN", data.Locale);
        Assert.Equal("zh-CN", configuration["Flourish:Preferences:Locale"]);
        Assert.Equal(Path.GetFullPath(appSettingsPath), settings.FilePath);
        Assert.Equal(Path.GetFullPath(projectCatalogPath), catalog.FilePath);
    }

    [Fact]
    public void Build_WhenStoragePathsAreTheSame_ThrowsInvalidOperationException()
    {
        var builder = FlourishBuilder
            .CreateDefaultBuilder([])
            .ConfigData(data =>
            {
                data.InitAppSettingsFilePath("Data/shared.json");
                data.InitProjectCatalogFilePath("Data/shared.json");
            });

        Assert.Throws<InvalidOperationException>(() => builder.Build());
    }

    [Fact]
    public void Build_UsesTheTargetedProviderAndHostsTheSamePreferenceService()
    {
        using var flourish = FlourishBuilder
            .CreateDefaultBuilder([])
            .ConfigServices((_, services) =>
            {
                services.AddSingleton<TestHostedService>();
                services.AddSingleton<IHostedService>(provider =>
                    provider.GetRequiredService<TestHostedService>()
                );
            })
            .Build();
        var configuration = Assert.IsAssignableFrom<IConfigurationRoot>(
            flourish.GetRequiredService<IConfiguration>()
        );
        var preferences = flourish.GetRequiredService<AppPreferenceService>();
        var hostedServices = flourish
            .GetRequiredService<IEnumerable<IHostedService>>()
            .ToArray();

        Assert.Single(
            configuration.Providers.OfType<FlourishAppSettingsConfigurationProvider>()
        );
        Assert.Same(preferences, hostedServices[0]);
        var commandParserIndex = Array.FindIndex(
            hostedServices,
            service => service is CommandParserHostedService
        );
        var applicationServiceIndex = Array.FindIndex(
            hostedServices,
            service => service is TestHostedService
        );
        Assert.Equal(1, commandParserIndex);
        Assert.True(applicationServiceIndex > commandParserIndex);
        Assert.True(
            Array.FindIndex(
                hostedServices,
                service => service is FlourishBackgroundTaskService
            ) > 0
        );
    }

    [Fact]
    public async Task StartAndStop_ActivateRegisteredCommandParsers()
    {
        using var flourish = FlourishBuilder
            .CreateDefaultBuilder([])
            .ConfigServices((_, services) =>
                services.AddCommandParser<TestCommandParser>()
            )
            .Build();
        var commands = flourish.GetRequiredService<ICommandRegistry>();

        Assert.False(commands.Contains("test.hosted"));
        flourish.Start();

        Assert.True(commands.Contains("test.hosted"));
        await flourish.StopAsync();
        Assert.False(commands.Contains("test.hosted"));
    }

    [Fact]
    public void AddEntryAssemblyUserSecrets_PreservesDefaultHostPrecedenceAndAvoidsDuplicates()
    {
        var appSettingsSource = new JsonConfigurationSource
        {
            Path = "appsettings.json",
            Optional = true,
        };
        var higherPrioritySource = new MemoryConfigurationSource();
        var configuration = new ConfigurationBuilder();
        configuration.Sources.Add(appSettingsSource);
        configuration.Sources.Add(higherPrioritySource);
        var entryAssembly = CreateAssemblyWithUserSecretsId();

        DefaultFlourishBuilder.AddEntryAssemblyUserSecrets(
            configuration,
            entryAssembly
        );
        DefaultFlourishBuilder.AddEntryAssemblyUserSecrets(
            configuration,
            entryAssembly
        );

        Assert.Equal(3, configuration.Sources.Count);
        Assert.Same(appSettingsSource, configuration.Sources[0]);
        Assert.IsType<JsonConfigurationSource>(configuration.Sources[1]);
        Assert.Same(higherPrioritySource, configuration.Sources[2]);
    }

    [Fact]
    public void UseTargetedAppSettingsProvider_ReplacesTheBaseSourceInPlace()
    {
        var baseAppSettings = new JsonConfigurationSource
        {
            Path = "appsettings.json",
            Optional = true,
            ReloadOnChange = true,
            ReloadDelay = 125,
        };
        var environmentAppSettings = new JsonConfigurationSource
        {
            Path = "appsettings.Development.json",
            Optional = true,
            ReloadOnChange = true,
        };
        var higherPrioritySource = new MemoryConfigurationSource();
        var configuration = new ConfigurationBuilder();
        configuration.Sources.Add(baseAppSettings);
        configuration.Sources.Add(environmentAppSettings);
        configuration.Sources.Add(higherPrioritySource);

        var appSettingsPath = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
        DefaultFlourishBuilder.UseTargetedAppSettingsProvider(
            configuration,
            appSettingsPath
        );
        DefaultFlourishBuilder.UseTargetedAppSettingsProvider(
            configuration,
            appSettingsPath
        );

        Assert.Equal(3, configuration.Sources.Count);
        var replacement = Assert.IsType<FlourishAppSettingsConfigurationSource>(
            configuration.Sources[0]
        );
        Assert.Equal(baseAppSettings.Path, replacement.Path);
        Assert.Equal(baseAppSettings.Optional, replacement.Optional);
        Assert.Equal(baseAppSettings.ReloadDelay, replacement.ReloadDelay);
        Assert.False(replacement.ReloadOnChange);
        Assert.True(replacement.WatchForChanges);
        Assert.False(replacement.LoadOnlyFlourishSection);
        Assert.Same(environmentAppSettings, configuration.Sources[1]);
        Assert.Same(higherPrioritySource, configuration.Sources[2]);
    }

    [Fact]
    public void UseTargetedAppSettingsProvider_CustomFilePreservesHostSourcesAndPrecedence()
    {
        using var directory = new TemporaryDirectory();
        var baseAppSettings = new JsonConfigurationSource
        {
            Path = "appsettings.json",
            Optional = true,
        };
        var environmentAppSettings = new JsonConfigurationSource
        {
            Path = "appsettings.Development.json",
            Optional = true,
        };
        var higherPrioritySource = new MemoryConfigurationSource();
        var configuration = new ConfigurationBuilder();
        configuration.Sources.Add(baseAppSettings);
        configuration.Sources.Add(environmentAppSettings);
        configuration.Sources.Add(higherPrioritySource);

        DefaultFlourishBuilder.UseTargetedAppSettingsProvider(
            configuration,
            Path.Combine(directory.Path, "appsettings.Flourish.json")
        );

        Assert.Equal(4, configuration.Sources.Count);
        var flourishSource = Assert.IsType<FlourishAppSettingsConfigurationSource>(
            configuration.Sources[0]
        );
        Assert.True(flourishSource.LoadOnlyFlourishSection);
        Assert.Same(baseAppSettings, configuration.Sources[1]);
        Assert.Same(environmentAppSettings, configuration.Sources[2]);
        Assert.Same(higherPrioritySource, configuration.Sources[3]);
    }

    [Fact]
    public void InsertApplicationConfigurationSources_InsertsBeforeEnvironmentAndCommandLine()
    {
        var baseAppSettings = new JsonConfigurationSource
        {
            Path = "appsettings.json",
            Optional = true,
        };
        var userSecrets = new JsonConfigurationSource
        {
            Path = "secrets.json",
            Optional = true,
        };
        var environment = new EnvironmentVariablesConfigurationSource();
        var commandLine = new CommandLineConfigurationSource { Args = [] };
        var first = new MemoryConfigurationSource();
        var second = new MemoryConfigurationSource();
        var configuration = new ConfigurationBuilder();
        configuration.Sources.Add(baseAppSettings);
        configuration.Sources.Add(userSecrets);
        configuration.Sources.Add(environment);
        configuration.Sources.Add(commandLine);

        DefaultFlourishBuilder.InsertApplicationConfigurationSources(
            configuration,
            [first, second]
        );

        Assert.Collection(
            configuration.Sources,
            source => Assert.Same(baseAppSettings, source),
            source => Assert.Same(userSecrets, source),
            source => Assert.Same(first, source),
            source => Assert.Same(second, source),
            source => Assert.Same(environment, source),
            source => Assert.Same(commandLine, source)
        );
    }

    private static Assembly CreateAssemblyWithUserSecretsId()
    {
        var assembly = AssemblyBuilder.DefineDynamicAssembly(
            new AssemblyName($"Flourish.UserSecrets.Test.{Guid.NewGuid():N}"),
            AssemblyBuilderAccess.Run
        );
        var constructor = typeof(UserSecretsIdAttribute).GetConstructor([typeof(string)])
            ?? throw new InvalidOperationException("UserSecretsIdAttribute constructor was not found.");
        assembly.SetCustomAttribute(
            new CustomAttributeBuilder(
                constructor,
                [$"ArkheideSystem.Flourish.Test.{Guid.NewGuid():N}"]
            )
        );
        return assembly;
    }

    private sealed class TestPage : Page { }

    private sealed class TestHostedService : IHostedService
    {
        public Task StartAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }

    private sealed class TestCommandParser : ICommandParser
    {
        public void RegisterCommands(ICommandRegistrar commands)
        {
            commands.Register("test.hosted", static () => { });
        }
    }

    private sealed class TemporaryDirectory : IDisposable
    {
        public TemporaryDirectory()
        {
            Path = System.IO.Path.Combine(
                System.IO.Path.GetTempPath(),
                $"flourish-builder-{Guid.NewGuid():N}"
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
