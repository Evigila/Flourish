using System.IO;
using System.Reflection;
using ArkheideSystem.Flourish.Abstract;
using ArkheideSystem.Flourish.Internal.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.CommandLine;
using Microsoft.Extensions.Configuration.EnvironmentVariables;
using Microsoft.Extensions.Configuration.Json;
using Microsoft.Extensions.Configuration.UserSecrets;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;

namespace ArkheideSystem.Flourish.Internal.Composition;

internal sealed class DefaultFlourishBuilder
    : FlourishBuilderMutationGuard,
        IFlourishBuilder
{
    private readonly FlourishShellOptions shellOptions = new();
    private readonly FlourishDataOptions dataOptions = new();
    private readonly IHostBuilder hostBuilder;
    private readonly List<Action<IFlourishDataBuilder>> dataConfigurations = [];
    private readonly List<Action<HostBuilderContext, IFlourishConfigurationBuilder>>
        configurationConfigurations = [];
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

    public DefaultFlourishBuilder(string[] args)
    {
        ArgumentNullException.ThrowIfNull(args);
        hostBuilder = CreateHostBuilder(
            args,
            dataOptions,
            configurationConfigurations
        );
    }

    public IFlourishBuilder ConfigData(Action<IFlourishDataBuilder> configureData)
    {
        ThrowIfFrozen();
        ArgumentNullException.ThrowIfNull(configureData);
        dataConfigurations.Add(configureData);
        return this;
    }

    public IFlourishBuilder ConfigConfiguration(
        Action<HostBuilderContext, IFlourishConfigurationBuilder> configure
    )
    {
        ThrowIfFrozen();
        ArgumentNullException.ThrowIfNull(configure);
        configurationConfigurations.Add(configure);
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

        ApplyDataConfigurations();
        ValidateStoragePaths();

        var compositionRoot = new FlourishCompositionRoot(
            shellOptions,
            dataOptions,
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

    private void ApplyDataConfigurations()
    {
        var dataBuilder = new FlourishDataBuilder(dataOptions);
        try
        {
            dataBuilder.InitLocale();
            foreach (var configure in dataConfigurations)
            {
                configure(dataBuilder);
            }
        }
        finally
        {
            dataBuilder.Freeze();
        }
    }

    private void ValidateStoragePaths()
    {
        if (
            string.Equals(
                dataOptions.AppSettingsFilePath,
                dataOptions.ProjectCatalogFilePath,
                StringComparison.OrdinalIgnoreCase
            )
        )
        {
            throw new InvalidOperationException(
                "The appsettings file and project catalog must use different paths."
            );
        }
    }

    private static IHostBuilder CreateHostBuilder(
        string[] args,
        FlourishDataOptions dataOptions,
        IReadOnlyList<Action<HostBuilderContext, IFlourishConfigurationBuilder>>
            configurationConfigurations
    )
    {
        var builder = Host.CreateDefaultBuilder(args).UseContentRoot(AppContext.BaseDirectory);
        builder.ConfigureAppConfiguration((context, configuration) =>
        {
            UseTargetedAppSettingsProvider(configuration, dataOptions.AppSettingsFilePath);
            AddEntryAssemblyUserSecrets(configuration);
            var applicationSources = new List<IConfigurationSource>();
            foreach (var configure in configurationConfigurations)
            {
                var configurationBuilder = new FlourishConfigurationBuilder();
                try
                {
                    configure(context, configurationBuilder);
                }
                finally
                {
                    configurationBuilder.Freeze();
                }

                applicationSources.AddRange(configurationBuilder.Sources);
            }

            InsertApplicationConfigurationSources(
                configuration,
                applicationSources
            );
        });
        return builder;
    }

    internal static void InsertApplicationConfigurationSources(
        IConfigurationBuilder configuration,
        IReadOnlyList<IConfigurationSource> sources
    )
    {
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentNullException.ThrowIfNull(sources);

        var insertionIndex = configuration.Sources
            .Select((source, index) => (source, index))
            .Where(item =>
                item.source is EnvironmentVariablesConfigurationSource
                or CommandLineConfigurationSource
            )
            .Select(item => item.index)
            .DefaultIfEmpty(configuration.Sources.Count)
            .First();

        foreach (var source in sources)
        {
            configuration.Sources.Insert(insertionIndex++, source);
        }
    }

    internal static void UseTargetedAppSettingsProvider(
        IConfigurationBuilder configuration,
        string appSettingsFilePath
    )
    {
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentException.ThrowIfNullOrWhiteSpace(appSettingsFilePath);
        var targetPath = Path.GetFullPath(appSettingsFilePath, AppContext.BaseDirectory);
        if (
            configuration.Sources.OfType<FlourishAppSettingsConfigurationSource>().Any()
        )
        {
            return;
        }

        var baseSourceEntry = configuration.Sources
            .Select((source, index) => (source, index))
            .FirstOrDefault(item =>
                item.source is JsonConfigurationSource json
                && string.Equals(
                    json.Path?.Replace('\\', '/'),
                    "appsettings.json",
                    StringComparison.OrdinalIgnoreCase
                )
            );
        var defaultPath = Path.GetFullPath(
            Path.Combine(AppContext.BaseDirectory, "appsettings.json")
        );
        if (
            baseSourceEntry.source is JsonConfigurationSource baseSource
            && string.Equals(targetPath, defaultPath, StringComparison.OrdinalIgnoreCase)
        )
        {
            configuration.Sources[baseSourceEntry.index] =
                new FlourishAppSettingsConfigurationSource
                {
                    FileProvider = baseSource.FileProvider,
                    Path = baseSource.Path!,
                    Optional = baseSource.Optional,
                    ReloadDelay = baseSource.ReloadDelay,
                    ReloadOnChange = false,
                    WatchForChanges = baseSource.ReloadOnChange,
                    LoadOnlyFlourishSection = false,
                    OnLoadException = baseSource.OnLoadException,
                };
            return;
        }

        var flourishSource = new FlourishAppSettingsConfigurationSource
        {
            Path = targetPath,
            Optional = true,
            ReloadDelay = 250,
            ReloadOnChange = false,
            WatchForChanges = true,
        };
        flourishSource.ResolveFileProvider();
        var insertionIndex =
            baseSourceEntry.source is null ? 0 : baseSourceEntry.index;
        configuration.Sources.Insert(insertionIndex, flourishSource);
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
