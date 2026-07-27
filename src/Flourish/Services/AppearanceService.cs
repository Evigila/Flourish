using System.Windows;
using System.Windows.Threading;
using ArkheideSystem.Flourish.Abstract;
using ArkheideSystem.Flourish.Internal.Configuration;
using Application = System.Windows.Application;

namespace ArkheideSystem.Flourish.Services;

internal sealed class AppearanceService : IAppearanceService
{
    private readonly Lock gate = new();
    private FlourishThemeColors? themeColors;
    private double? cornerRadius;
    private FlourishTheme effectiveTheme = FlourishTheme.Light;
    private Dispatcher? applicationDispatcher;
    private ResourceDictionary? overrideResources;
    private long version;

    public AppearanceService(FlourishShellOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        themeColors = options.ThemeColors;
        cornerRadius = options.CornerRadius;
    }

    public FlourishAppearanceSettings Current
    {
        get
        {
            lock (gate)
            {
                return CreateSnapshot();
            }
        }
    }

    public event EventHandler<FlourishAppearanceChangedEventArgs>? Changed;

    public void SetThemeColors(FlourishThemeColors? colors)
    {
        Update(current => (colors, current.CornerRadius));
    }

    public void SetCornerRadius(double? radius)
    {
        Update(current => (current.ThemeColors, radius));
    }

    public void SetAppearance(FlourishThemeColors? colors, double? cornerRadius)
    {
        Update(_ => (colors, cornerRadius));
    }

    private void Update(
        Func<FlourishAppearanceSettings, (FlourishThemeColors? Colors, double? CornerRadius)> update
    )
    {
        FlourishAppearanceSettings previous;
        FlourishAppearanceSettings current;
        Dispatcher? dispatcher;
        ResourceDictionary? resources;
        FlourishTheme theme;
        lock (gate)
        {
            var next = update(CreateSnapshot());
            ValidateCornerRadius(next.CornerRadius);

            if (Equals(themeColors, next.Colors) && cornerRadius == next.CornerRadius)
            {
                return;
            }

            previous = CreateSnapshot();
            themeColors = next.Colors;
            cornerRadius = next.CornerRadius;
            version++;
            current = CreateSnapshot();
            dispatcher = applicationDispatcher;
            resources = overrideResources;
            theme = effectiveTheme;
        }

        void ApplyAndNotify()
        {
            if (resources is not null)
            {
                Apply(resources, current, theme);
            }

            Changed?.Invoke(this, new FlourishAppearanceChangedEventArgs(previous, current));
        }

        if (dispatcher is null || dispatcher.CheckAccess())
        {
            ApplyAndNotify();
        }
        else
        {
            dispatcher.Invoke(ApplyAndNotify);
        }
    }

    internal void Attach(Application application, FlourishTheme theme)
    {
        ArgumentNullException.ThrowIfNull(application);
        Attach(application.Dispatcher, application.Resources, theme);
    }

    internal void Attach(
        Dispatcher dispatcher,
        ResourceDictionary applicationResources,
        FlourishTheme theme
    )
    {
        ArgumentNullException.ThrowIfNull(dispatcher);
        ArgumentNullException.ThrowIfNull(applicationResources);
        var resources = new ResourceDictionary();

        void AttachCore()
        {
            FlourishAppearanceSettings snapshot;
            lock (gate)
            {
                applicationDispatcher = dispatcher;
                effectiveTheme = theme;
                overrideResources = resources;
                snapshot = CreateSnapshot();
            }

            applicationResources.MergedDictionaries.Add(resources);
            Apply(resources, snapshot, theme);
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

    internal void Reapply(FlourishTheme theme)
    {
        Dispatcher? dispatcher;
        ResourceDictionary? resources;
        FlourishAppearanceSettings snapshot;
        lock (gate)
        {
            effectiveTheme = theme;
            dispatcher = applicationDispatcher;
            resources = overrideResources;
            snapshot = CreateSnapshot();
        }

        if (resources is null)
        {
            return;
        }

        void ApplyCore() => Apply(resources, snapshot, theme);
        if (dispatcher is null || dispatcher.CheckAccess())
        {
            ApplyCore();
        }
        else
        {
            dispatcher.Invoke(ApplyCore);
        }
    }

    private static void Apply(
        ResourceDictionary resources,
        FlourishAppearanceSettings settings,
        FlourishTheme theme
    )
    {
        resources.Clear();
        ThemeService.ApplyStyleOverrides(
            resources,
            settings.ThemeColors,
            settings.CornerRadius,
            theme
        );
    }

    private FlourishAppearanceSettings CreateSnapshot() =>
        new(themeColors, cornerRadius, version);

    private static void ValidateCornerRadius(double? radius)
    {
        if (radius is { } value && (!double.IsFinite(value) || value < 0))
        {
            throw new ArgumentOutOfRangeException(
                nameof(radius),
                radius,
                "Corner radius must be finite and non-negative."
            );
        }
    }
}
