using System.IO;
using System.Reflection;
using ArkheideSystem.Flourish.Abstract;
using ArkheideSystem.Flourish.Internal.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using Microsoft.Extensions.Configuration.UserSecrets;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;

namespace ArkheideSystem.Flourish.Internal.Composition;

internal sealed class DefaultFlourishBuilder(string[] args)
    : FlourishBuilderMutationGuard,
        IFlourishBuilder
{
    private readonly IHostBuilder hostBuilder = CreateHostBuilder(args);
    private readonly FlourishShellOptions shellOptions = new();
    private readonly FlourishDataOptions dataOptions = new();
    private readonly List<Action<IFlourishDataBuilder>> dataConfigurations = [];
    private readonly List<Action<HostBuilderContext, IServiceCollection>> serviceConfigurations =
    [];
    private readonly List<Action<IFlourishShellBuilder>> shellConfigurations = [];
    private readonly List<Action<IFlourishProfileBuilder>> profileConfigurations = [];
    private readonly List<Action<IFlourishTitlebarBuilder>> titleBarConfigurations = [];
    private readonly List<Action<IFlourishNavigationBuilder>> navigationConfigurations = [];
    private readonly List<Action<IFlourishCustomHandlerBuilder>> customHandlerConfigurations = [];
    private readonly List<Action<IFlourishDynamicToolbarBuilder>> toolbarConfigurations = [];
    private readonly List<Action<IFlourishMotionBuilder>> motionConfigurations = [];
    private readonly List<Action<IFlourishWindowPropertyBuilder>> windowConfigurations = [];
    private readonly List<Action<IFlourishStatusBarBuilder>> statusBarConfigurations = [];

    public IFlourishBuilder ConfigData(Action<IFlourishDataBuilder> configureData)
    {
        ThrowIfFrozen();
        ArgumentNullException.ThrowIfNull(configureData);
        dataConfigurations.Add(configureData);
        return this;
    }

    public IFlourishBuilder ConfigServices(
        Action<HostBuilderContext, IServiceCollection> configureServices
    )
    {
        ThrowIfFrozen();
        ArgumentNullException.ThrowIfNull(configureServices);
        serviceConfigurations.Add(configureServices);
        return this;
    }

    public IFlourishBuilder ConfigShell(Action<IFlourishShellBuilder> configureShell)
    {
        ThrowIfFrozen();
        ArgumentNullException.ThrowIfNull(configureShell);
        shellConfigurations.Add(configureShell);
        return this;
    }

    public IFlourishBuilder ConfigProfile(
        Action<IFlourishProfileBuilder> configureProfile
    )
    {
        ThrowIfFrozen();
        ArgumentNullException.ThrowIfNull(configureProfile);
        profileConfigurations.Add(configureProfile);
        return this;
    }

    public IFlourishBuilder ConfigTitleBar(
        Action<IFlourishTitlebarBuilder> configureTitleBar
    )
    {
        ThrowIfFrozen();
        ArgumentNullException.ThrowIfNull(configureTitleBar);
        titleBarConfigurations.Add(configureTitleBar);
        return this;
    }

    public IFlourishBuilder ConfigNavigation(
        Action<IFlourishNavigationBuilder> configureNavigation
    )
    {
        ThrowIfFrozen();
        ArgumentNullException.ThrowIfNull(configureNavigation);
        navigationConfigurations.Add(configureNavigation);
        return this;
    }

    public IFlourishBuilder ConfigCustomHandler(
        Action<IFlourishCustomHandlerBuilder> configureCustomHandler
    )
    {
        ThrowIfFrozen();
        ArgumentNullException.ThrowIfNull(configureCustomHandler);
        customHandlerConfigurations.Add(configureCustomHandler);
        return this;
    }

    public IFlourishBuilder ConfigDynamicToolbar(
        Action<IFlourishDynamicToolbarBuilder> configureToolbar
    )
    {
        ThrowIfFrozen();
        ArgumentNullException.ThrowIfNull(configureToolbar);
        toolbarConfigurations.Add(configureToolbar);
        return this;
    }

    public IFlourishBuilder ConfigMotion(Action<IFlourishMotionBuilder> configureMotion)
    {
        ThrowIfFrozen();
        ArgumentNullException.ThrowIfNull(configureMotion);
        motionConfigurations.Add(configureMotion);
        return this;
    }

    public IFlourishBuilder ConfigWindow(
        Action<IFlourishWindowPropertyBuilder> configureWindow
    )
    {
        ThrowIfFrozen();
        ArgumentNullException.ThrowIfNull(configureWindow);
        windowConfigurations.Add(configureWindow);
        return this;
    }

    public IFlourishBuilder ConfigStatusBar(
        Action<IFlourishStatusBarBuilder> configureStatusBar
    )
    {
        ThrowIfFrozen();
        ArgumentNullException.ThrowIfNull(configureStatusBar);
        statusBarConfigurations.Add(configureStatusBar);
        return this;
    }

    public IFlourish Build()
    {
        if (!TryFreeze())
        {
            throw new InvalidOperationException(
                "The Flourish builder can only build one application runtime."
            );
        }

        var compositionRoot = new FlourishCompositionRoot(
            shellOptions,
            dataOptions,
            dataConfigurations,
            serviceConfigurations,
            shellConfigurations,
            profileConfigurations,
            titleBarConfigurations,
            navigationConfigurations,
            customHandlerConfigurations,
            toolbarConfigurations,
            motionConfigurations,
            windowConfigurations,
            statusBarConfigurations
        );

        hostBuilder.ConfigureServices(compositionRoot.ConfigServices);
        return new FlourishRuntime(hostBuilder.Build());
    }

    private static IHostBuilder CreateHostBuilder(string[] args)
    {
        var builder = Host.CreateDefaultBuilder(args).UseContentRoot(AppContext.BaseDirectory);
        builder.ConfigureAppConfiguration((_, configuration) =>
        {
            UseTargetedAppSettingsProvider(configuration);
            AddEntryAssemblyUserSecrets(configuration);
        });
        return builder;
    }

    internal static void UseTargetedAppSettingsProvider(
        IConfigurationBuilder configuration
    )
    {
        ArgumentNullException.ThrowIfNull(configuration);
        for (var index = 0; index < configuration.Sources.Count; index++)
        {
            if (configuration.Sources[index] is FlourishAppSettingsConfigurationSource)
            {
                return;
            }

            if (
                configuration.Sources[index] is not JsonConfigurationSource source
                || !string.Equals(
                    source.Path?.Replace('\\', '/'),
                    "appsettings.json",
                    StringComparison.OrdinalIgnoreCase
                )
            )
            {
                continue;
            }

            configuration.Sources[index] = new FlourishAppSettingsConfigurationSource
            {
                FileProvider = source.FileProvider,
                Path = source.Path!,
                Optional = source.Optional,
                ReloadDelay = source.ReloadDelay,
                ReloadOnChange = false,
                WatchForChanges = source.ReloadOnChange,
                OnLoadException = source.OnLoadException,
            };
            return;
        }
    }

    internal static void AddEntryAssemblyUserSecrets(
        IConfigurationBuilder configuration,
        Assembly? entryAssembly = null
    )
    {
        ArgumentNullException.ThrowIfNull(configuration);

        entryAssembly ??= Assembly.GetEntryAssembly();
        var userSecretsId = entryAssembly
            ?.GetCustomAttribute<UserSecretsIdAttribute>()
            ?.UserSecretsId;
        if (string.IsNullOrWhiteSpace(userSecretsId) || entryAssembly is null)
        {
            return;
        }

        var secretPath = Path.GetFullPath(
            PathHelper.GetSecretsPathFromSecretsId(userSecretsId)
        );
        var isAlreadyRegistered = configuration.Sources
            .OfType<JsonConfigurationSource>()
            .Any(source => IsSourceForPath(source, secretPath));
        if (!isAlreadyRegistered)
        {
            var insertionIndex = configuration.Sources
                .Select((source, index) => (source, index))
                .Where(item => item.source is JsonConfigurationSource)
                .Select(item => item.index + 1)
                .DefaultIfEmpty(0)
                .Last();
            configuration.AddUserSecrets(
                entryAssembly,
                optional: true,
                reloadOnChange: true
            );

            var userSecretsSource = configuration.Sources[^1];
            configuration.Sources.RemoveAt(configuration.Sources.Count - 1);
            configuration.Sources.Insert(insertionIndex, userSecretsSource);
        }
    }

    private static bool IsSourceForPath(
        JsonConfigurationSource source,
        string expectedPath
    )
    {
        if (string.IsNullOrWhiteSpace(source.Path))
        {
            return false;
        }

        if (Path.IsPathRooted(source.Path))
        {
            return string.Equals(
                Path.GetFullPath(source.Path),
                expectedPath,
                StringComparison.OrdinalIgnoreCase
            );
        }

        if (source.FileProvider is null)
        {
            return string.Equals(
                Path.GetFileName(source.Path),
                "secrets.json",
                StringComparison.OrdinalIgnoreCase
            );
        }

        var physicalPath = source.FileProvider.GetFileInfo(source.Path).PhysicalPath;
        if (
            string.IsNullOrWhiteSpace(physicalPath)
            && source.FileProvider is PhysicalFileProvider physicalFileProvider
        )
        {
            physicalPath = Path.Combine(physicalFileProvider.Root, source.Path);
        }

        return string.IsNullOrWhiteSpace(physicalPath)
            ? string.Equals(
                Path.GetFileName(source.Path),
                "secrets.json",
                StringComparison.OrdinalIgnoreCase
            )
            : string.Equals(
                Path.GetFullPath(physicalPath),
                expectedPath,
                StringComparison.OrdinalIgnoreCase
            );
    }
}
