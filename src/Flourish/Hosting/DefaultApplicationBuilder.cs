using System.Linq;

using System;
using System.Collections.Generic;

using ArkheideSystem.Flourish.Abstract;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using ArkheideSystem.Flourish.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.CommandLine;
using Microsoft.Extensions.Configuration.EnvironmentVariables;
using Microsoft.Extensions.Configuration.Json;
using Microsoft.Extensions.Configuration.UserSecrets;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;

namespace ArkheideSystem.Flourish.Hosting;

internal sealed class DefaultApplicationBuilder : BuilderMutationGuard, IApplicationBuilder
{
    private readonly ApplicationOptions applicationOptions = new();
    private readonly ApplicationDataOptions dataOptions = new();
    private readonly IHostBuilder hostBuilder;
    private readonly List<Action<IDataBuilder>> dataConfigurations = [];
    private readonly List<
        Action<HostBuilderContext, IConfigurationBuilder>
    > configurationConfigurations = [];
    private readonly List<Action<HostBuilderContext, IServiceCollection>> serviceConfigurations =
    [];
    private readonly List<Action<IAppearanceBuilder>> appearanceConfigurations = [];
    private readonly List<Action<IFontBuilder>> fontConfigurations = [];
    private readonly List<Action<ILayoutBuilder>> layoutConfigurations = [];
    private readonly List<Action<IToolTipBuilder>> toolTipConfigurations = [];
    private readonly List<Action<IProjectBuilder>> projectConfigurations = [];
    private readonly List<Action<ITitleBarBuilder>> titleBarConfigurations = [];
    private readonly List<Action<INavigationBuilder>> navigationConfigurations = [];
    private readonly List<Action<ICustomContentBuilder>> customHandlerConfigurations = [];
    private readonly List<Action<IToolbarBuilder>> toolbarConfigurations = [];
    private readonly List<Action<IMotionBuilder>> motionConfigurations = [];
    private readonly List<Action<IWindowBuilder>> windowConfigurations = [];
    private readonly List<Action<IStatusBarBuilder>> statusBarConfigurations = [];

    public DefaultApplicationBuilder(string[] args)
    {
        ArgumentNullException.ThrowIfNull(args);
        hostBuilder = CreateHostBuilder(args, dataOptions, configurationConfigurations);
    }

    public IApplicationBuilder ConfigureData(Action<IDataBuilder> configureData) =>
        RegisterConfiguration(configureData, dataConfigurations);

    public IApplicationBuilder ConfigureConfiguration(
        Action<HostBuilderContext, IConfigurationBuilder> configure
    ) => RegisterConfiguration(configure, configurationConfigurations);

    public IApplicationBuilder ConfigureServices(
        Action<HostBuilderContext, IServiceCollection> configureServices
    ) => RegisterConfiguration(configureServices, serviceConfigurations);

    public IApplicationBuilder ConfigureAppearance(
        Action<IAppearanceBuilder> configureAppearance
    ) => RegisterConfiguration(configureAppearance, appearanceConfigurations);

    public IApplicationBuilder ConfigureFont(Action<IFontBuilder> configureFont) =>
        RegisterConfiguration(configureFont, fontConfigurations);

    public IApplicationBuilder ConfigureLayout(Action<ILayoutBuilder> configureLayout) =>
        RegisterConfiguration(configureLayout, layoutConfigurations);

    public IApplicationBuilder ConfigureToolTips(Action<IToolTipBuilder> configureToolTips) =>
        RegisterConfiguration(configureToolTips, toolTipConfigurations);

    public IApplicationBuilder ConfigureProjects(Action<IProjectBuilder> configureProjects) =>
        RegisterConfiguration(configureProjects, projectConfigurations);

    public IApplicationBuilder ConfigureTitleBar(Action<ITitleBarBuilder> configureTitleBar) =>
        RegisterConfiguration(configureTitleBar, titleBarConfigurations);

    public IApplicationBuilder ConfigureNavigation(
        Action<INavigationBuilder> configureNavigation
    ) => RegisterConfiguration(configureNavigation, navigationConfigurations);

    public IApplicationBuilder ConfigureContent(
        Action<ICustomContentBuilder> configureCustomHandler
    ) => RegisterConfiguration(configureCustomHandler, customHandlerConfigurations);

    public IApplicationBuilder ConfigureToolbar(
        Action<IToolbarBuilder> configureToolbar
    ) => RegisterConfiguration(configureToolbar, toolbarConfigurations);

    public IApplicationBuilder ConfigureMotion(Action<IMotionBuilder> configureMotion) =>
        RegisterConfiguration(configureMotion, motionConfigurations);

    public IApplicationBuilder ConfigureWindow(Action<IWindowBuilder> configureWindow) =>
        RegisterConfiguration(configureWindow, windowConfigurations);

    public IApplicationBuilder ConfigureStatusBar(Action<IStatusBarBuilder> configureStatusBar) =>
        RegisterConfiguration(configureStatusBar, statusBarConfigurations);

    private IApplicationBuilder RegisterConfiguration<TDelegate>(
        TDelegate configure,
        ICollection<TDelegate> configurations,
        [CallerArgumentExpression(nameof(configure))] string? parameterName = null
    )
        where TDelegate : Delegate
    {
        ThrowIfFrozen();
        ArgumentNullException.ThrowIfNull(configure, parameterName);
        configurations.Add(configure);
        return this;
    }

    public IApplicationRuntime Build()
    {
        if (!TryFreeze())
        {
            throw new InvalidOperationException(
                "The Flourish builder can only build one application runtime."
            );
        }

        ApplyDataConfigurations();
        ValidateStoragePaths();

        var compositionRoot = new ApplicationCompositionRoot(
            applicationOptions,
            dataOptions,
            serviceConfigurations,
            appearanceConfigurations,
            fontConfigurations,
            layoutConfigurations,
            toolTipConfigurations,
            projectConfigurations,
            titleBarConfigurations,
            navigationConfigurations,
            customHandlerConfigurations,
            toolbarConfigurations,
            motionConfigurations,
            windowConfigurations,
            statusBarConfigurations
        );

        hostBuilder.ConfigureServices(compositionRoot.ConfigureServices);
        return new HostedApplicationRuntime(hostBuilder.Build());
    }

    private void ApplyDataConfigurations()
    {
        var dataBuilder = new DataBuilder(dataOptions);
        try
        {
            dataBuilder.SetLocale();
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
        ApplicationDataOptions dataOptions,
        IReadOnlyList<
            Action<HostBuilderContext, IConfigurationBuilder>
        > configurationConfigurations
    )
    {
        var builder = Host.CreateDefaultBuilder(args).UseContentRoot(AppContext.BaseDirectory);
        builder.ConfigureAppConfiguration(
            (context, configuration) =>
            {
                UseTargetedAppSettingsProvider(configuration, dataOptions.AppSettingsFilePath);
                AddEntryAssemblyUserSecrets(configuration);
                var applicationSources = new List<IConfigurationSource>();
                foreach (var configure in configurationConfigurations)
                {
                    var applicationConfiguration = new ConfigurationBuilder()
                        .SetBasePath(AppContext.BaseDirectory);
                    configure(context, applicationConfiguration);
                    applicationSources.AddRange(applicationConfiguration.Sources);
                }

                InsertApplicationConfigurationSources(configuration, applicationSources);
            }
        );
        return builder;
    }

    internal static void InsertApplicationConfigurationSources(
        IConfigurationBuilder configuration,
        IReadOnlyList<IConfigurationSource> sources
    )
    {
        ArgumentNullException.ThrowIfNull(configuration);
        ArgumentNullException.ThrowIfNull(sources);

        var insertionIndex = configuration
            .Sources.Select((source, index) => (source, index))
            .Where(item =>
                item.source
                    is EnvironmentVariablesConfigurationSource
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
        if (configuration.Sources.OfType<AppSettingsConfigurationSource>().Any())
        {
            return;
        }

        var baseSourceEntry = configuration
            .Sources.Select((source, index) => (source, index))
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
                new AppSettingsConfigurationSource
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

        var flourishSource = new AppSettingsConfigurationSource
        {
            Path = targetPath,
            Optional = true,
            ReloadDelay = 250,
            ReloadOnChange = false,
            WatchForChanges = true,
        };
        flourishSource.ResolveFileProvider();
        var insertionIndex = baseSourceEntry.source is null ? 0 : baseSourceEntry.index;
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

        var secretPath = Path.GetFullPath(PathHelper.GetSecretsPathFromSecretsId(userSecretsId));
        var isAlreadyRegistered = configuration
            .Sources.OfType<JsonConfigurationSource>()
            .Any(source => IsSourceForPath(source, secretPath));
        if (!isAlreadyRegistered)
        {
            var insertionIndex = configuration
                .Sources.Select((source, index) => (source, index))
                .Where(item => item.source is JsonConfigurationSource)
                .Select(item => item.index + 1)
                .DefaultIfEmpty(0)
                .Last();
            configuration.AddUserSecrets(entryAssembly, optional: true, reloadOnChange: true);

            var userSecretsSource = configuration.Sources[^1];
            configuration.Sources.RemoveAt(configuration.Sources.Count - 1);
            configuration.Sources.Insert(insertionIndex, userSecretsSource);
        }
    }

    private static bool IsSourceForPath(JsonConfigurationSource source, string expectedPath)
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
