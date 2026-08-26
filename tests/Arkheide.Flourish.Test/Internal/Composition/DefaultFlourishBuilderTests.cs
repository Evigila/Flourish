using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using ArkheideSystem.Flourish.Abstract;
using ArkheideSystem.Flourish.Appearance;
using ArkheideSystem.Flourish.BackgroundTasks;
using ArkheideSystem.Flourish.Commands;
using ArkheideSystem.Flourish.Configuration;
using ArkheideSystem.Flourish.Hosting;
using ArkheideSystem.Flourish.Layout;
using ArkheideSystem.Flourish.Motion;
using ArkheideSystem.Flourish.Navigation;
using ArkheideSystem.Flourish.Profile;
using ArkheideSystem.Flourish.Projects;
using ArkheideSystem.Flourish.Shell.Regions;
using ArkheideSystem.Flourish.Shell.StatusBar;
using ArkheideSystem.Flourish.Shell.TitleBar;
using ArkheideSystem.Flourish.Shell.Toolbar;
using ArkheideSystem.Flourish.ToolTips;
using ArkheideSystem.Flourish.Windowing;
using ArkheideSystem.Flourish.Test.Infrastructure;

using System.IO;
using System.Reflection;
using System.Reflection.Emit;
using System.Windows.Controls;
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

        Assert.Equal(
            "configureData",
            Assert.Throws<ArgumentNullException>(() => builder.ConfigureData(null!)).ParamName
        );
        Assert.Equal(
            "configure",
            Assert.Throws<ArgumentNullException>(() => builder.ConfigureConfiguration(null!)).ParamName
        );
        Assert.Equal(
            "configureServices",
            Assert.Throws<ArgumentNullException>(() => builder.ConfigureServices(null!)).ParamName
        );
        Assert.Equal(
            "configureAppearance",
            Assert.Throws<ArgumentNullException>(() => builder.ConfigureAppearance(null!)).ParamName
        );
        Assert.Equal(
            "configureFont",
            Assert.Throws<ArgumentNullException>(() => builder.ConfigureFont(null!)).ParamName
        );
        Assert.Equal(
            "configureLayout",
            Assert.Throws<ArgumentNullException>(() => builder.ConfigureLayout(null!)).ParamName
        );
        Assert.Equal(
            "configureToolTips",
            Assert.Throws<ArgumentNullException>(() => builder.ConfigureToolTips(null!)).ParamName
        );
        Assert.Equal(
            "configureProjects",
            Assert.Throws<ArgumentNullException>(() => builder.ConfigureProjects(null!)).ParamName
        );
        Assert.Equal(
            "configureTitleBar",
            Assert.Throws<ArgumentNullException>(() => builder.ConfigureTitleBar(null!)).ParamName
        );
        Assert.Equal(
            "configureNavigation",
            Assert.Throws<ArgumentNullException>(() => builder.ConfigureNavigation(null!)).ParamName
        );
        Assert.Equal(
            "configureCustomHandler",
            Assert.Throws<ArgumentNullException>(() => builder.ConfigureContent(null!)).ParamName
        );
        Assert.Equal(
            "configureToolbar",
            Assert
                .Throws<ArgumentNullException>(() => builder.ConfigureToolbar(null!))
                .ParamName
        );
        Assert.Equal(
            "configureMotion",
            Assert.Throws<ArgumentNullException>(() => builder.ConfigureMotion(null!)).ParamName
        );
        Assert.Equal(
            "configureWindow",
            Assert.Throws<ArgumentNullException>(() => builder.ConfigureWindow(null!)).ParamName
        );
        Assert.Equal(
            "configureStatusBar",
            Assert.Throws<ArgumentNullException>(() => builder.ConfigureStatusBar(null!)).ParamName
        );
    }

    [Fact]
    public void Build_FreezesTopLevelBuilderAndCanOnlyRunOnce()
    {
        var builder = FlourishBuilder.CreateDefaultBuilder([]);

        using var flourish = builder.Build();

        Assert.Throws<InvalidOperationException>(() => builder.ConfigureLayout(_ => { }));
        Assert.Throws<InvalidOperationException>(() => builder.ConfigureData(null!));
        Assert.Throws<InvalidOperationException>(() => builder.ConfigureConfiguration((_, _) => { }));
        Assert.Throws<InvalidOperationException>(() => builder.Build());
    }

    [Fact]
    public void Build_AppliesRegisteredApplicationConfigurationSource()
    {
        HostBuilderContext? capturedContext = null;
        using var flourish = FlourishBuilder
            .CreateDefaultBuilder([])
            .ConfigureConfiguration(
                (context, configuration) =>
                {
                    capturedContext = context;
                    configuration.Add(
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
                }
            )
            .Build();

        var configuration = flourish.GetRequiredService<IConfiguration>();

        Assert.NotNull(capturedContext);
        Assert.Equal("configured", configuration["Flourish:Test:AdditionalConfiguration"]);
    }

    [Fact]
    public void Build_UsesRegisteredConfigurationFile()
    {
        using var directory = new TemporaryDirectory();
        var path = Path.Combine(directory.Path, "appsettings.User.json");
        File.WriteAllText(path, """{"Application":{"DisplayName":"Foobar"}}""");

        using var flourish = FlourishBuilder
            .CreateDefaultBuilder([])
            .ConfigureConfiguration(
                (_, configuration) =>
                    configuration.AddJsonFile(path, optional: false, reloadOnChange: false)
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
            .ConfigureConfiguration(
                (_, configuration) =>
                    configuration.Add(
                        new MemoryConfigurationSource
                        {
                            InitialData = [KeyValuePair.Create<string, string?>(key, "registered")],
                        }
                    )
            )
            .Build();

        Assert.Equal("command-line", flourish.GetRequiredService<IConfiguration>()[key]);
    }

    [Fact]
    public void Build_RegisteredApplicationSourcesPreserveRegistrationOrder()
    {
        const string key = "Application:RegistrationOrder";
        using var flourish = FlourishBuilder
            .CreateDefaultBuilder([])
            .ConfigureConfiguration(
                (_, configuration) =>
                    configuration.Add(
                        new MemoryConfigurationSource
                        {
                            InitialData = [KeyValuePair.Create<string, string?>(key, "first")],
                        }
                    )
            )
            .ConfigureConfiguration(
                (_, configuration) =>
                    configuration.Add(
                        new MemoryConfigurationSource
                        {
                            InitialData = [KeyValuePair.Create<string, string?>(key, "second")],
                        }
                    )
            )
            .Build();

        Assert.Equal("second", flourish.GetRequiredService<IConfiguration>()[key]);
    }

    [Fact]
    public void Build_FreezesCapturedStartupBuildersAfterTheirCallbacksComplete()
    {
        IDataBuilder? data = null;
        IAppearanceBuilder? appearance = null;
        IFontBuilder? font = null;
        ILayoutBuilder? layout = null;
        IToolTipBuilder? toolTips = null;
        IProjectBuilder? projects = null;
        ITitleBarBuilder? titleBar = null;
        INavigationBuilder? navigation = null;
        INavigationGroupBuilder? navigationGroup = null;
        ICustomContentBuilder? customHandler = null;
        IToolbarBuilder? toolbar = null;
        IMotionBuilder? motion = null;
        IWindowBuilder? window = null;
        IStatusBarBuilder? statusBar = null;

        var builder = FlourishBuilder
            .CreateDefaultBuilder([])
            .ConfigureData(value => data = value)
            .ConfigureAppearance(value => appearance = value)
            .ConfigureFont(value => font = value)
            .ConfigureLayout(value => layout = value)
            .ConfigureToolTips(value => toolTips = value)
            .ConfigureProjects(value => projects = value)
            .ConfigureTitleBar(value => titleBar = value)
            .ConfigureNavigation(value =>
            {
                navigation = value;
                value.AddGroup(configureGroup: group => navigationGroup = group);
            })
            .ConfigureContent(value => customHandler = value)
            .ConfigureToolbar(value => toolbar = value)
            .ConfigureMotion(value => motion = value)
            .ConfigureWindow(value => window = value)
            .ConfigureStatusBar(value => statusBar = value);

        using var flourish = builder.Build();

        Assert.Throws<InvalidOperationException>(() => data!.SetLocale());
        Assert.Throws<InvalidOperationException>(() => appearance!.SetEffect());
        Assert.Throws<InvalidOperationException>(() => font!.SetFont());
        Assert.Throws<InvalidOperationException>(() => layout!.SetCenterContent());
        Assert.Throws<InvalidOperationException>(() => toolTips!.SetEnabled());
        Assert.Throws<InvalidOperationException>(() => projects!.SetMultiProjectEnabled());
        Assert.Throws<InvalidOperationException>(() => titleBar!.SetApplicationTitle());
        Assert.Throws<InvalidOperationException>(() => titleBar!.SetProfilePage<TestPage>());
        Assert.Throws<InvalidOperationException>(() => navigation!.SetInitiallyOpen());
        Assert.Throws<InvalidOperationException>(() =>
            navigationGroup!.AddNavigableItem("Late item", null, null)
        );
        Assert.Throws<InvalidOperationException>(() =>
            customHandler!.AddRegionContent(FlourishRegion.TitleBarEnd, _ => new Border())
        );
        Assert.Throws<InvalidOperationException>(() => toolbar!.Set<TestPage>());
        Assert.Throws<InvalidOperationException>(() => motion!.SetPageTransition());
        Assert.Throws<InvalidOperationException>(() => window!.SetTopmost());
        Assert.Throws<InvalidOperationException>(() => statusBar!.SetPowerStatusEnabled());
    }

    [Fact]
    public void Build_AppliesConfigurationCallbacksAndPreservesRegistrationOrder()
    {
        var marker = new object();
        var builder = FlourishBuilder
            .CreateDefaultBuilder([])
            .ConfigureData(data => data.SetLocale("zh-CN", usePersistedPreference: false))
            .ConfigureServices((_, services) => services.AddSingleton(marker))
            .ConfigureProjects(projects => projects.SetMultiProjectEnabled())
            .ConfigureNavigation(navigation => navigation.SetEnabled())
            .ConfigureLayout(layout =>
                layout.SetCenterContent(
                    enabled: true,
                    contentWidth: 900,
                    usePersistedPreference: false
                )
            )
            .ConfigureToolTips(toolTips => toolTips.SetEnabled().SetSettings(350))
            .ConfigureFont(font =>
                font.SetFont("Arial", 13, 15, 17, 19, 22, 28, usePersistedPreference: false)
            )
            .ConfigureAppearance(appearance =>
                appearance.SetEffect(
                    enabled: false,
                    effect: MaterialEffect.None,
                    usePersistedPreference: false
                )
            )
            .ConfigureStatusBar(statusBar => statusBar.SetEnabled())
            .ConfigureStatusBar(statusBar => statusBar.SetEnabled(enabled: false))
            .ConfigureTitleBar(titlebar =>
                titlebar
                    .SetLogo(
                        showApplicationTitle: false,
                        showApplicationSubtitle: true,
                        showProjectTitle: true
                    )
                    .SetApplicationTitle("Test Shell")
                    .SetApplicationSubtitle("Test workspace")
                    .SetUnnamedProjectPlaceholder("Untitled project")
                    .SetProfile(
                        nameOrder: NameOrder.LastFirst,
                        usePersistedPreference: false
                    )
                    .SetThemeToggle(
                        mode: FlourishTheme.Dark,
                        usePersistedPreference: false
                    )
            )
            .ConfigureContent(custom =>
                custom.AddRegionContent(FlourishRegion.TitleBarStart, _ => null!)
            )
            .ConfigureToolbar(toolbar =>
                toolbar.Set<TestPage>(
                    new FlourishToolbarItem("Refresh", "R", "cmd_test_refresh")
                )
            )
            .ConfigureMotion(motion =>
                motion.SetRespectSystemReducedMotion(
                    enabled: false,
                    usePersistedPreference: false
                )
            )
            .ConfigureWindow(window => window.SetTopmost(usePersistedPreference: false))
            .ConfigureStatusBar(statusBar => statusBar.AddStatusItem("Ready", "R"));

        using var flourish = builder.Build();
        var projectOptions = flourish.GetRequiredService<FlourishProjectOptions>();
        var navigationOptions = flourish.GetRequiredService<FlourishNavigationOptions>();
        var layoutOptions = flourish.GetRequiredService<FlourishLayoutOptions>();
        var statusOptions = flourish.GetRequiredService<FlourishStatusBarOptions>();
        var titleBarOptions = flourish.GetRequiredService<FlourishTitleBarOptions>();
        var profileOptions = flourish.GetRequiredService<FlourishProfileOptions>();
        var appearanceOptions = flourish.GetRequiredService<FlourishAppearanceOptions>();
        var regionOptions = flourish.GetRequiredService<FlourishRegionOptions>();
        var toolbarOptions = flourish.GetRequiredService<FlourishToolbarOptions>();
        var tipOptions = flourish.GetRequiredService<FlourishTipOptions>();
        var motionOptions = flourish.GetRequiredService<FlourishMotionOptions>();
        var windowOptions = flourish.GetRequiredService<FlourishWindowOptions>();
        var dataOptions = flourish.GetRequiredService<FlourishDataOptions>();
        var projects = flourish.GetRequiredService<IProjectService>();

        Assert.Same(marker, flourish.GetRequiredService<object>());
        Assert.Equal("zh-CN", dataOptions.Locale);
        Assert.True(projectOptions.IsMultiProjectEnabled);
        Assert.True(projects.Current.IsMultiProjectEnabled);
        Assert.Equal(0, projects.Current.Version);
        Assert.True(navigationOptions.IsNavigationPanelEnabled);
        Assert.True(layoutOptions.IsCenterContentEnabled);
        Assert.Equal(900, layoutOptions.CenterContentWidth);
        Assert.False(statusOptions.IsStatusBarEnabled);
        Assert.Equal("Test Shell", titleBarOptions.ApplicationTitle);
        Assert.Equal("Test workspace", titleBarOptions.ApplicationSubtitle);
        Assert.Equal("Untitled project", projectOptions.UnnamedProjectPlaceholder);
        Assert.True(titleBarOptions.IsTitlebarLogoEnabled);
        Assert.False(titleBarOptions.ShowApplicationTitleInLogoFlyout);
        Assert.True(titleBarOptions.ShowApplicationSubtitleInLogoFlyout);
        Assert.True(titleBarOptions.ShowProjectTitleInLogoFlyout);
        Assert.True(titleBarOptions.IsTitlebarTitleEnabled);
        Assert.True(profileOptions.IsProfileEnabled);
        Assert.True(titleBarOptions.IsTitlebarProfileEnabled);
        Assert.Equal(NameOrder.LastFirst, profileOptions.NameOrder);
        Assert.True(appearanceOptions.IsThemeEnabled);
        Assert.True(titleBarOptions.IsTitlebarThemeToggleEnabled);
        Assert.Single(regionOptions.RegionContents);
        Assert.Single(toolbarOptions.DynamicToolbarItems[typeof(TestPage)]);
        Assert.Equal(350, tipOptions.InitialShowDelayMilliseconds);
        Assert.False(motionOptions.RespectSystemReducedMotion);
        Assert.True(windowOptions.WindowTopmost);
        Assert.Equal("Arial", appearanceOptions.FontFamily);
        Assert.Equal(13, appearanceOptions.FontSizeSmall);
        Assert.Equal(15, appearanceOptions.FontSizeStandard);
        Assert.Equal(17, appearanceOptions.FontSizeIcon);
        Assert.Equal(19, appearanceOptions.FontSizeLarge);
        Assert.Equal(22, appearanceOptions.FontSizeExtraLarge);
        Assert.Equal(28, appearanceOptions.FontSizeHeaderSize);
        Assert.Equal(MaterialEffect.None, appearanceOptions.MaterialEffect);
        Assert.False(appearanceOptions.IsMaterialEffectEnabled);
        Assert.Equal(FlourishTheme.Dark, appearanceOptions.DefaultTheme);
        Assert.Collection(
            statusOptions.StatusItems,
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
        var appSettingsPath = Path.Combine(directory.Path, "appsettings.Flourish.json");
        var projectCatalogPath = Path.Combine(directory.Path, "catalog", "projects.json");
        File.WriteAllText(appSettingsPath, """{"Flourish":{"Preferences":{"Locale":"zh-CN"}}}""");

        using var flourish = FlourishBuilder
            .CreateDefaultBuilder([])
            .ConfigureData(data =>
                data.SetAppSettingsFilePath(appSettingsPath)
                    .SetProjectCatalogFilePath(projectCatalogPath)
            )
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
            .ConfigureData(data =>
            {
                data.SetAppSettingsFilePath("Data/shared.json");
                data.SetProjectCatalogFilePath("Data/shared.json");
            });

        Assert.Throws<InvalidOperationException>(() => builder.Build());
    }

    [Fact]
    public void Build_UsesTheTargetedProviderAndHostsTheSamePreferenceService()
    {
        using var flourish = FlourishBuilder
            .CreateDefaultBuilder([])
            .ConfigureServices(
                (_, services) =>
                {
                    services.AddSingleton<TestHostedService>();
                    services.AddSingleton<IHostedService>(provider =>
                        provider.GetRequiredService<TestHostedService>()
                    );
                }
            )
            .Build();
        var configuration = Assert.IsAssignableFrom<IConfigurationRoot>(
            flourish.GetRequiredService<IConfiguration>()
        );
        var preferences = flourish.GetRequiredService<AppPreferenceService>();
        var hostedServices = flourish.GetRequiredService<IEnumerable<IHostedService>>().ToArray();

        Assert.Single(configuration.Providers.OfType<FlourishAppSettingsConfigurationProvider>());
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
            Array.FindIndex(hostedServices, service => service is FlourishBackgroundTaskService) > 0
        );
    }

    [Fact]
    public async Task StartAndStop_ActivateRegisteredCommandParsers()
    {
        using var flourish = FlourishBuilder
            .CreateDefaultBuilder([])
            .ConfigureServices((_, services) => services.AddCommandParser<TestCommandParser>())
            .Build();
        var commands = flourish.GetRequiredService<ICommandRegistry>();

        Assert.False(commands.Contains("cmd_test_hosted"));
        flourish.Start();

        Assert.True(commands.Contains("cmd_test_hosted"));
        await flourish.StopAsync();
        Assert.False(commands.Contains("cmd_test_hosted"));
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

        DefaultFlourishBuilder.AddEntryAssemblyUserSecrets(configuration, entryAssembly);
        DefaultFlourishBuilder.AddEntryAssemblyUserSecrets(configuration, entryAssembly);

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
        DefaultFlourishBuilder.UseTargetedAppSettingsProvider(configuration, appSettingsPath);
        DefaultFlourishBuilder.UseTargetedAppSettingsProvider(configuration, appSettingsPath);

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
        var userSecrets = new JsonConfigurationSource { Path = "secrets.json", Optional = true };
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
        var constructor =
            typeof(UserSecretsIdAttribute).GetConstructor([typeof(string)])
            ?? throw new InvalidOperationException(
                "UserSecretsIdAttribute constructor was not found."
            );
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
            commands.Register("cmd_test_hosted", static () => { });
        }
    }
}
