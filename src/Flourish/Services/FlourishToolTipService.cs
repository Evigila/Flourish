using System.Windows;
using System.Windows.Threading;
using ArkheideSystem.Flourish.Abstract;
using ArkheideSystem.Flourish.Internal.Configuration;
using Application = System.Windows.Application;

namespace ArkheideSystem.Flourish.Services;

internal sealed class FlourishToolTipService(FlourishShellOptions options) : IToolTipService
{
    private const string InitialShowDelayResourceKey = "FlourishToolTipInitialShowDelay";
    private const string SpawnableMarginResourceKey = "FlourishToolTipSpawnableMargin";
    private const string IsEnabledResourceKey = "FlourishToolTipsEnabled";
    private readonly Lock gate = new();
    private Dispatcher? applicationDispatcher;
    private ResourceDictionary? applicationResources;
    private ResourceDictionary? appliedResources;
    private FlourishToolTipSettings? appliedSettings;
    private long mutationGeneration;

    public FlourishToolTipSettings Current
    {
        get
        {
            lock (gate)
            {
                return CaptureSettings();
            }
        }
    }

    public event EventHandler<FlourishToolTipChangedEventArgs>? Changed;

    internal void Attach(Application application)
    {
        ArgumentNullException.ThrowIfNull(application);
        Attach(application.Dispatcher, application.Resources);
    }

    internal void Attach(Dispatcher dispatcher, ResourceDictionary resources)
    {
        ArgumentNullException.ThrowIfNull(dispatcher);
        ArgumentNullException.ThrowIfNull(resources);

        void AttachCore()
        {
            FlourishToolTipSettings settings;
            lock (gate)
            {
                applicationDispatcher = dispatcher;
                applicationResources = resources;
                settings = CaptureSettings();
            }

            if (ReferenceEquals(appliedResources, resources) && appliedSettings == settings)
            {
                return;
            }

            ApplyResources(resources, settings);
            appliedResources = resources;
            appliedSettings = settings;
        }

        if (dispatcher.CheckAccess())
        {
            AttachCore();
        }
        else
        {
            dispatcher.Invoke(AttachCore);
        }
    }

    public void SetEnabled(bool enabled)
    {
        ExecuteMutation(() =>
        {
            var previous = CaptureSettings();
            if (previous.IsEnabled == enabled)
            {
                return null;
            }

            options.IsTipsEnabled = enabled;
            return CreateMutation(previous, CaptureSettings());
        });
    }

    public void Configure(int initialShowDelayMilliseconds, double spawnableMargin = 5)
    {
        if (initialShowDelayMilliseconds < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(initialShowDelayMilliseconds),
                initialShowDelayMilliseconds,
                "Tooltip delay cannot be negative."
            );
        }

        if (!double.IsFinite(spawnableMargin) || spawnableMargin < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(spawnableMargin),
                spawnableMargin,
                "Tooltip margin must be a non-negative finite number."
            );
        }

        ExecuteMutation(() =>
        {
            var previous = CaptureSettings();
            options.IsTipsEnabled = true;
            options.Tips.InitialShowDelayMilliseconds = initialShowDelayMilliseconds;
            options.Tips.SpawnableMargin = spawnableMargin;
            var current = CaptureSettings();
            return previous == current ? null : CreateMutation(previous, current);
        });
    }

    private void ExecuteMutation(Func<ToolTipMutation?> mutationFactory)
    {
        Dispatcher? dispatcher;
        ToolTipMutation? detachedMutation;
        lock (gate)
        {
            dispatcher = applicationDispatcher;
            detachedMutation = dispatcher is null ? mutationFactory() : null;
        }

        if (dispatcher is null)
        {
            PublishMutation(detachedMutation, null);
            return;
        }

        void ExecuteOnDispatcher()
        {
            ToolTipMutation? mutation;
            ResourceDictionary? resources;
            lock (gate)
            {
                mutation = mutationFactory();
                resources = applicationResources;
            }

            PublishMutation(mutation, resources);
        }

        if (dispatcher.CheckAccess())
        {
            ExecuteOnDispatcher();
        }
        else
        {
            dispatcher.Invoke(ExecuteOnDispatcher);
        }
    }

    private void PublishMutation(ToolTipMutation? mutation, ResourceDictionary? resources)
    {
        if (mutation is not { } value)
        {
            return;
        }

        if (resources is null)
        {
            PublishDetachedMutation(value);
            return;
        }

        lock (gate)
        {
            if (value.Generation != mutationGeneration)
            {
                return;
            }
        }

        ApplyResources(resources, value.Current);
        appliedResources = resources;
        appliedSettings = value.Current;
        RaiseChanged(value);
    }

    private void PublishDetachedMutation(ToolTipMutation mutation)
    {
        Dispatcher? dispatcher;
        lock (gate)
        {
            if (mutation.Generation != mutationGeneration)
            {
                return;
            }

            dispatcher = applicationDispatcher;
            if (dispatcher is null)
            {
                RaiseChanged(mutation);
                return;
            }
        }

        void RaiseIfCurrent()
        {
            lock (gate)
            {
                if (mutation.Generation != mutationGeneration)
                {
                    return;
                }
            }

            RaiseChanged(mutation);
        }

        if (dispatcher.CheckAccess())
        {
            RaiseIfCurrent();
        }
        else
        {
            dispatcher.Invoke(RaiseIfCurrent);
        }
    }

    private void RaiseChanged(ToolTipMutation mutation)
    {
        Changed?.Invoke(
            this,
            new FlourishToolTipChangedEventArgs(mutation.Previous, mutation.Current)
        );
    }

    private ToolTipMutation CreateMutation(
        FlourishToolTipSettings previous,
        FlourishToolTipSettings current
    )
    {
        mutationGeneration++;
        return new ToolTipMutation(previous, current, mutationGeneration);
    }

    private FlourishToolTipSettings CaptureSettings()
    {
        return new FlourishToolTipSettings(
            options.IsTipsEnabled,
            options.Tips.InitialShowDelayMilliseconds,
            options.Tips.SpawnableMargin
        );
    }

    private static void ApplyResources(
        ResourceDictionary resources,
        FlourishToolTipSettings settings
    )
    {
        SetResourceIfChanged(resources, IsEnabledResourceKey, settings.IsEnabled);
        SetResourceIfChanged(
            resources,
            InitialShowDelayResourceKey,
            settings.IsEnabled ? settings.InitialShowDelayMilliseconds : int.MaxValue
        );
        SetResourceIfChanged(
            resources,
            SpawnableMarginResourceKey,
            settings.IsEnabled ? settings.SpawnableMargin : 0d
        );
    }

    private static void SetResourceIfChanged(ResourceDictionary resources, string key, object value)
    {
        var hasDirectKey = resources.Keys.Cast<object>().Contains(key);
        if (hasDirectKey && Equals(resources[key], value))
        {
            return;
        }

        resources[key] = value;
    }

    private sealed record ToolTipMutation(
        FlourishToolTipSettings Previous,
        FlourishToolTipSettings Current,
        long Generation
    );
}
