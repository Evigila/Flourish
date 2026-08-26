using System;
using System.ComponentModel;
using System.Threading;

using ArkheideSystem.Flourish.Abstract;
using System.Windows;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using ArkheideSystem.Flourish.Navigation;
using Application = System.Windows.Application;

namespace ArkheideSystem.Flourish.Motion;

internal sealed class FlourishMotionService : IMotionService, IDisposable
{
    private const string HoverRevealEnabledResourceKey = "FlourishHoverRevealEnabled";
    private const string HoverRevealDurationResourceKey = "FlourishHoverRevealDuration";
    private readonly Lock gate = new();
    private readonly FlourishMotionOptions options;
    private readonly Func<bool> isSystemAnimationEnabled;
    private Dispatcher? applicationDispatcher;
    private ResourceDictionary? applicationResources;
    private FlourishMotionSettings current;
    private bool observesSystemAnimation;
    private bool isDisposed;

    public FlourishMotionService(FlourishMotionOptions options)
        : this(options, static () => SystemParameters.ClientAreaAnimation)
    {
        SystemParameters.StaticPropertyChanged += SystemParameters_StaticPropertyChanged;
        observesSystemAnimation = true;
        RefreshSystemAnimationState();
    }

    internal FlourishMotionService(
        FlourishMotionOptions options,
        Func<bool> isSystemAnimationEnabled
    )
    {
        this.options = options ?? throw new ArgumentNullException(nameof(options));
        this.isSystemAnimationEnabled =
            isSystemAnimationEnabled
            ?? throw new ArgumentNullException(nameof(isSystemAnimationEnabled));
        current = CaptureSettings();
    }

    public FlourishMotionSettings Current => Volatile.Read(ref current);

    public event EventHandler<
        FlourishStateTransitionEventArgs<FlourishMotionSettings>
    >? Changed;

    public void Dispose()
    {
        var unsubscribe = false;
        lock (gate)
        {
            if (isDisposed)
            {
                return;
            }

            isDisposed = true;
            unsubscribe = observesSystemAnimation;
            observesSystemAnimation = false;
            applicationDispatcher = null;
            applicationResources = null;
        }

        if (unsubscribe)
        {
            SystemParameters.StaticPropertyChanged -= SystemParameters_StaticPropertyChanged;
        }
    }

    public void SetEnabled(bool enabled)
    {
        UpdateOptions(() => options.IsEnabled = enabled);
    }

    public void SetPageTransition(FlourishPageTransition transition, TimeSpan? duration = null)
    {
        ValidateEnum(transition, nameof(transition));
        if (duration.HasValue)
        {
            ValidateDuration(duration.Value, nameof(duration));
        }

        UpdateOptions(() =>
        {
            options.PageTransition = transition;
            if (duration.HasValue)
            {
                options.PageTransitionDuration = duration.Value;
            }
        });
    }

    public void SetNavigationPanelTransition(
        FlourishNavigationPanelTransition transition,
        TimeSpan? duration = null
    )
    {
        ValidateEnum(transition, nameof(transition));
        if (duration.HasValue)
        {
            ValidateDuration(duration.Value, nameof(duration));
        }

        UpdateOptions(() =>
        {
            options.NavigationPanelTransition = transition;
            if (duration.HasValue)
            {
                options.NavigationPanelTransitionDuration = duration.Value;
            }
        });
    }

    public void SetHoverReveal(bool enabled, TimeSpan? duration = null)
    {
        if (duration.HasValue)
        {
            ValidateDuration(duration.Value, nameof(duration));
        }

        UpdateOptions(() =>
        {
            options.IsHoverRevealEnabled = enabled;
            if (duration.HasValue)
            {
                options.HoverRevealAnimationDuration = duration.Value;
            }
        });
    }

    public void SetRespectSystemReducedMotion(bool enabled)
    {
        UpdateOptions(() => options.RespectSystemReducedMotion = enabled);
    }

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
            FlourishMotionSettings settings;
            lock (gate)
            {
                applicationDispatcher = dispatcher;
                applicationResources = resources;
                settings = Volatile.Read(ref current);
            }

            ApplyResources(resources, settings);
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

    internal void AnimateNavigationPane(
        NavigationPaneTransitionController controller,
        NavigationPaneTransitionTarget target,
        double committedWidth,
        double targetWidth,
        double maximumPaneWidth,
        double referenceDistance,
        Action? completed = null
    )
    {
        ArgumentNullException.ThrowIfNull(controller);
        var settings = Current;

        if (
            !settings.CanAnimate
            || settings.NavigationPanelTransition == FlourishNavigationPanelTransition.None
            || (!controller.IsActive && AreClose(committedWidth, targetWidth))
        )
        {
            controller.Cancel();
            completed?.Invoke();
            return;
        }

        if (
            !controller.Start(
                target,
                committedWidth,
                targetWidth,
                maximumPaneWidth,
                referenceDistance,
                settings.NavigationPanelTransitionDuration,
                CreateEase(),
                completed ?? (static () => { })
            )
        )
        {
            completed?.Invoke();
        }
    }

    internal void AnimatePageEntrance(
        PageTransitionController controller,
        PageTransitionTarget target
    )
    {
        ArgumentNullException.ThrowIfNull(controller);
        var settings = Current;

        if (!settings.CanAnimate || settings.PageTransition == FlourishPageTransition.None)
        {
            controller.Cancel();
            return;
        }

        controller.Start(
            target,
            settings.PageTransition,
            settings.PageTransitionDuration,
            CreateEase(),
            static () => { }
        );
    }

    private static CubicEase CreateEase()
    {
        return new CubicEase { EasingMode = EasingMode.EaseOut };
    }

    private static bool AreClose(double first, double second)
    {
        return Math.Abs(first - second) < 0.5;
    }

    private void UpdateOptions(Action update)
    {
        FlourishMotionSettings previous;
        FlourishMotionSettings current;
        Dispatcher? dispatcher;
        ResourceDictionary? resources;
        lock (gate)
        {
            previous = Volatile.Read(ref this.current);
            update();
            current = CaptureSettings();
            if (previous == current)
            {
                return;
            }

            Volatile.Write(ref this.current, current);

            dispatcher = applicationDispatcher;
            resources = applicationResources;
        }

        PublishChange(previous, current, dispatcher, resources);
    }

    private void PublishChange(
        FlourishMotionSettings previous,
        FlourishMotionSettings current,
        Dispatcher? dispatcher,
        ResourceDictionary? resources
    )
    {
        if (dispatcher is not null && resources is not null)
        {
            if (dispatcher.CheckAccess())
            {
                var effective = Current;
                ApplyResources(resources, effective);
                RaiseChanged(previous, effective);
            }
            else
            {
                dispatcher.Invoke(() =>
                {
                    var effective = Current;
                    ApplyResources(resources, effective);
                    RaiseChanged(previous, effective);
                });
            }

            return;
        }

        RaiseChanged(previous, current);
    }

    private void RaiseChanged(FlourishMotionSettings previous, FlourishMotionSettings current)
    {
        Changed?.Invoke(
            this,
            new FlourishStateTransitionEventArgs<FlourishMotionSettings>(previous, current)
        );
    }

    private FlourishMotionSettings CaptureSettings()
    {
        var canAnimate =
            options.IsEnabled
            && (
                !options.RespectSystemReducedMotion
                || isSystemAnimationEnabled()
            );
        return new FlourishMotionSettings(
            options.IsEnabled,
            options.PageTransition,
            options.PageTransitionDuration,
            options.NavigationPanelTransition,
            options.NavigationPanelTransitionDuration,
            options.IsHoverRevealEnabled,
            options.HoverRevealAnimationDuration,
            options.RespectSystemReducedMotion,
            canAnimate
        );
    }

    private void SystemParameters_StaticPropertyChanged(
        object? sender,
        PropertyChangedEventArgs e
    )
    {
        if (e.PropertyName == nameof(SystemParameters.ClientAreaAnimation))
        {
            RefreshSystemAnimationState();
        }
    }

    private void RefreshSystemAnimationState()
    {
        FlourishMotionSettings previous;
        FlourishMotionSettings current;
        Dispatcher? dispatcher;
        ResourceDictionary? resources;
        lock (gate)
        {
            if (isDisposed)
            {
                return;
            }

            previous = Volatile.Read(ref this.current);
            current = CaptureSettings();
            if (previous.CanAnimate == current.CanAnimate)
            {
                return;
            }

            Volatile.Write(ref this.current, current);
            dispatcher = applicationDispatcher;
            resources = applicationResources;
        }

        PublishChange(previous, current, dispatcher, resources);
    }

    private void ApplyResources(ResourceDictionary resources, FlourishMotionSettings settings)
    {
        var isHoverRevealEnabled = settings.CanAnimate && settings.IsHoverRevealEnabled;
        SetResource(
            resources,
            HoverRevealDurationResourceKey,
            settings.HoverRevealAnimationDuration
        );
        SetResource(resources, HoverRevealEnabledResourceKey, isHoverRevealEnabled);
    }

    private static void SetResource(ResourceDictionary resources, string key, object value)
    {
        if (resources.Contains(key) && Equals(resources[key], value))
        {
            return;
        }

        resources[key] = value;
    }

    private static void ValidateDuration(TimeSpan duration, string parameterName)
    {
        if (duration <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(
                parameterName,
                duration,
                "Duration must be greater than zero."
            );
        }
    }

    private static void ValidateEnum<TEnum>(TEnum value, string parameterName)
        where TEnum : struct, Enum
    {
        if (!Enum.IsDefined(value))
        {
            throw new ArgumentOutOfRangeException(parameterName, value, "Unknown value.");
        }
    }
}
