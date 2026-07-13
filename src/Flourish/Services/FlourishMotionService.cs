using System.Windows;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using ArkheideSystem.Flourish.Abstract;
using ArkheideSystem.Flourish.Internal.Configuration;
using ArkheideSystem.Flourish.Internal.Interaction;
using Application = System.Windows.Application;

namespace ArkheideSystem.Flourish.Services;

internal sealed class FlourishMotionService : IMotionService
{
    private const string HoverRevealEnabledResourceKey = "FlourishHoverRevealEnabled";
    private const string HoverRevealDurationResourceKey = "FlourishHoverRevealDuration";
    private readonly object gate = new();
    private readonly FlourishShellOptions options;
    private readonly Func<bool> isSystemAnimationEnabled;
    private Dispatcher? applicationDispatcher;
    private ResourceDictionary? applicationResources;

    public FlourishMotionService(FlourishShellOptions options)
        : this(options, static () => SystemParameters.ClientAreaAnimation) { }

    internal FlourishMotionService(
        FlourishShellOptions options,
        Func<bool> isSystemAnimationEnabled
    )
    {
        this.options = options ?? throw new ArgumentNullException(nameof(options));
        this.isSystemAnimationEnabled =
            isSystemAnimationEnabled
            ?? throw new ArgumentNullException(nameof(isSystemAnimationEnabled));
    }

    public FlourishMotionSettings Current
    {
        get
        {
            lock (gate)
            {
                return CaptureSettings();
            }
        }
    }

    public bool CanAnimate => CanAnimateSettings(Current);

    public event EventHandler<FlourishMotionChangedEventArgs>? Changed;

    public void SetEnabled(bool enabled)
    {
        UpdateOptions(() => options.Motion.IsEnabled = enabled);
    }

    public void SetPageTransition(
        FlourishPageTransition transition,
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
            options.Motion.PageTransition = transition;
            if (duration.HasValue)
            {
                options.Motion.PageTransitionDuration = duration.Value;
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
            options.Motion.NavigationPanelTransition = transition;
            if (duration.HasValue)
            {
                options.Motion.NavigationPanelTransitionDuration = duration.Value;
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
            options.Motion.IsHoverRevealEnabled = enabled;
            if (duration.HasValue)
            {
                options.Motion.HoverRevealAnimationDuration = duration.Value;
            }
        });
    }

    public void SetRespectSystemReducedMotion(bool enabled)
    {
        UpdateOptions(() => options.Motion.RespectSystemReducedMotion = enabled);
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
                settings = CaptureSettings();
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
            !CanAnimateSettings(settings)
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

        if (
            !CanAnimateSettings(settings)
            || settings.PageTransition == FlourishPageTransition.None
        )
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
            previous = CaptureSettings();
            update();
            current = CaptureSettings();
            if (previous == current)
            {
                return;
            }

            dispatcher = applicationDispatcher;
            resources = applicationResources;
        }

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

    private void RaiseChanged(
        FlourishMotionSettings previous,
        FlourishMotionSettings current
    )
    {
        Changed?.Invoke(
            this,
            new FlourishMotionChangedEventArgs(
                previous,
                current,
                CanAnimateSettings(current)
            )
        );
    }

    private FlourishMotionSettings CaptureSettings()
    {
        return new FlourishMotionSettings(
            options.Motion.IsEnabled,
            options.Motion.PageTransition,
            options.Motion.PageTransitionDuration,
            options.Motion.NavigationPanelTransition,
            options.Motion.NavigationPanelTransitionDuration,
            options.Motion.IsHoverRevealEnabled,
            options.Motion.HoverRevealAnimationDuration,
            options.Motion.RespectSystemReducedMotion
        );
    }

    private bool CanAnimateSettings(FlourishMotionSettings settings)
    {
        return settings.IsEnabled
            && (!settings.RespectSystemReducedMotion || isSystemAnimationEnabled());
    }

    private void ApplyResources(
        ResourceDictionary resources,
        FlourishMotionSettings settings
    )
    {
        var isHoverRevealEnabled =
            CanAnimateSettings(settings) && settings.IsHoverRevealEnabled;
        SetResource(
            resources,
            HoverRevealDurationResourceKey,
            settings.HoverRevealAnimationDuration
        );
        SetResource(resources, HoverRevealEnabledResourceKey, isHoverRevealEnabled);
    }

    private static void SetResource(
        ResourceDictionary resources,
        string key,
        object value
    )
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
