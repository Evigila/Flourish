using System;
using System.Threading;

using ArkheideSystem.Flourish.Abstract;
using System.Windows;
using System.Windows.Threading;
using Application = System.Windows.Application;

namespace ArkheideSystem.Flourish.Layout;

internal sealed class ScrollService : IScrollService
{
    internal const string SmoothScrollingResourceKey = "FlourishSmoothScrollingEnabled";

    private readonly Lock gate = new();
    private bool isSmoothScrollingEnabled;
    private Dispatcher? applicationDispatcher;
    private ResourceDictionary? applicationResources;
    private ScrollSettings current;
    private long version;

    public ScrollService(LayoutOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);
        isSmoothScrollingEnabled = options.IsSmoothScrollingEnabled;
        current = CreateSnapshot();
    }

    public event EventHandler<StateTransitionEventArgs<ScrollSettings>>? Changed;

    public ScrollSettings Current => Volatile.Read(ref current);

    public void SetSmoothScrollingEnabled(bool enabled)
    {
        ScrollSettings previous;
        ScrollSettings current;
        Dispatcher? dispatcher;
        ResourceDictionary? resources;
        lock (gate)
        {
            if (isSmoothScrollingEnabled == enabled)
            {
                return;
            }

            previous = Volatile.Read(ref this.current);
            isSmoothScrollingEnabled = enabled;
            version++;
            current = CreateSnapshot();
            Volatile.Write(ref this.current, current);
            dispatcher = applicationDispatcher;
            resources = applicationResources;
        }

        void ApplyAndNotify()
        {
            if (resources is not null)
            {
                resources[SmoothScrollingResourceKey] = enabled;
            }

            Changed?.Invoke(this, new StateTransitionEventArgs<ScrollSettings>(previous, current));
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

    private ScrollSettings CreateSnapshot() => new(isSmoothScrollingEnabled, version);
}
