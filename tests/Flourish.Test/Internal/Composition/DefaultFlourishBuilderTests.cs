using System.IO;
using System.Reflection;
using System.Reflection.Emit;
using System.Windows.Controls;
using ArkheideSystem.Flourish.Abstract;
using ArkheideSystem.Flourish.Internal.Composition;
using ArkheideSystem.Flourish.Internal.Configuration;
using ArkheideSystem.Flourish.Services;
using Microsoft.Extensions.Configuration;
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

        Assert.Throws<ArgumentNullException>(() => builder.ConfigureData(null!));
        Assert.Throws<ArgumentNullException>(() => builder.ConfigureServices(null!));
        Assert.Throws<ArgumentNullException>(() => builder.ConfigureShell(null!));
        Assert.Throws<ArgumentNullException>(() => builder.ConfigureTitleBar(null!));
        Assert.Throws<ArgumentNullException>(() => builder.ConfigureNavigation(null!));
        Assert.Throws<ArgumentNullException>(() => builder.ConfigureCustomHandler(null!));
        Assert.Throws<ArgumentNullException>(() => builder.ConfigureDynamicToolbar(null!));
        Assert.Throws<ArgumentNullException>(() => builder.ConfigureMotion(null!));
        Assert.Throws<ArgumentNullException>(() => builder.ConfigureWindow(null!));
        Assert.Throws<ArgumentNullException>(() => builder.ConfigureStatusBar(null!));
    }

    [Fact]
    public void Build_AppliesConfigurationCallbacksAndPreservesRegistrationOrder()
    {
        var marker = new object();
        var builder = FlourishBuilder
            .CreateDefaultBuilder([])
            .ConfigureData(data => data.SetLocale("CN"))
            .ConfigureServices((_, services) => services.AddSingleton(marker))
            .ConfigureShell(shell =>
                shell
                    .UseMultiProject()
                    .UseNavigation()
                    .UseCenterContent(enabled: true, contentWidth: 900)
                    .UseTips(350)
                    .UseGlobalFont("Arial", 13, 15, 17, 19, 22, 28)
                    .UseMaterialEffect(MaterialEffect.None)
                    .UseStatusBar()
            )
            .ConfigureShell(shell => shell.UseStatusBar(enabled: false))
            .ConfigureTitleBar(titlebar =>
                titlebar
                    .SetLogo(
                        showApplicationTitle: false,
                        showApplicationSubTitle: true,
                        showProjectTitle: true
                    )
                    .SetApplicationTitle("Test Shell")
                    .SetApplicationSubTitle("Test workspace")
                    .SetUnnamedProjectPlaceholder("Untitled project")
                    .SetProfile(NameOrder.LastFirst)
                    .SetThemeToggle(FlourishTheme.Dark)
            )
            .ConfigureCustomHandler(custom =>
                custom.Add(FlourishRegion.TitlebarStart, _ => null!)
            )
            .ConfigureDynamicToolbar(toolbar =>
                toolbar.CreateToolbarItems<TestPage>(
                    new FlourishToolbarItem("Refresh", "R", "test.refresh")
                )
            )
            .ConfigureMotion(motion => motion.RespectSystemReducedMotion(enabled: false))
            .ConfigureWindow(window => window.UseTopmost())
            .ConfigureStatusBar(statusBar => statusBar.AddStatusItem("Ready", "R"));

        using var flourish = builder.Build();
        var options = flourish.GetRequiredService<FlourishShellOptions>();
        var dataOptions = flourish.GetRequiredService<FlourishDataOptions>();
        var projects = flourish.GetRequiredService<IProjectService>();

        Assert.Same(marker, flourish.GetRequiredService<object>());
        Assert.Equal("CN", dataOptions.Locale);
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
        var statusItem = Assert.Single(options.StatusItems);
        Assert.Equal("Ready", statusItem.Text);
        Assert.Equal("R", statusItem.IconGlyph);
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
    public void Build_UsesTheTargetedProviderAndHostsTheSamePreferenceService()
    {
        using var flourish = FlourishBuilder
            .CreateDefaultBuilder([])
            .ConfigureServices((_, services) =>
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
            .ConfigureServices((_, services) =>
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

        DefaultFlourishBuilder.UseTargetedAppSettingsProvider(configuration);
        DefaultFlourishBuilder.UseTargetedAppSettingsProvider(configuration);

        Assert.Equal(3, configuration.Sources.Count);
        var replacement = Assert.IsType<FlourishAppSettingsConfigurationSource>(
            configuration.Sources[0]
        );
        Assert.Equal(baseAppSettings.Path, replacement.Path);
        Assert.Equal(baseAppSettings.Optional, replacement.Optional);
        Assert.Equal(baseAppSettings.ReloadDelay, replacement.ReloadDelay);
        Assert.False(replacement.ReloadOnChange);
        Assert.True(replacement.WatchForChanges);
        Assert.Same(environmentAppSettings, configuration.Sources[1]);
        Assert.Same(higherPrioritySource, configuration.Sources[2]);
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
}
