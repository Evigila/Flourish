using System.Windows;
using System.Windows.Threading;
using ArkheideSystem.Flourish.Abstract;
using ArkheideSystem.Flourish.Internal.Configuration;
using Application = System.Windows.Application;

namespace ArkheideSystem.Flourish.Services;

internal sealed class ScrollService : IScrollService
{
    internal const string SmoothScrollingResourceKey = "FlourishSmoothScrollingEnabled";

    private readonly Lock gate = new();
    private bool isSmoothScrollingEnabled;
    private Dispatcher? applicationDispatcher;
    private ResourceDictionary? applicationResources;
    private long version;

    public ScrollService(FlourishShellOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        isSmoothScrollingEnabled = options.IsSmoothScrollingEnabled;
    }

    public event EventHandler<FlourishScrollChangedEventArgs>? Changed;

    public FlourishScrollSettings GetCurrent()
    {
        lock (gate)
        {
            return CreateSnapshot();
        }
    }

    public void SetSmoothScrollingEnabled(bool enabled)
    {
        FlourishScrollSettings previous;
        FlourishScrollSettings current;
        Dispatcher? dispatcher;
        ResourceDictionary? resources;
        lock (gate)
        {
            if (isSmoothScrollingEnabled == enabled)
            {
                return;
            }

            previous = CreateSnapshot();
            isSmoothScrollingEnabled = enabled;
            version++;
            current = CreateSnapshot();
            dispatcher = applicationDispatcher;
            resources = applicationResources;
        }

        void ApplyAndNotify()
        {
            if (resources is not null)
            {
                resources[SmoothScrollingResourceKey] = enabled;
            }

            Changed?.Invoke(this, new FlourishScrollChangedEventArgs(previous, current));
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
            bool enabled;
            lock (gate)
            {
                applicationDispatcher = dispatcher;
                applicationResources = resources;
                enabled = isSmoothScrollingEnabled;
            }

            resources[SmoothScrollingResourceKey] = enabled;
        }

        if (dispatcher.CheckAccess())
        {
            AttachCore();
            return;
        }

        dispatcher.Invoke(AttachCore);
    }

    private FlourishScrollSettings CreateSnapshot() =>
        new(isSmoothScrollingEnabled, version);
}
