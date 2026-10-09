using System.ComponentModel;
using System.Windows;
using System.Windows.Threading;
using ThemeMode = ArkheideSystem.Flourish.WPF.Abstract.ThemeMode;
using Microsoft.Win32;

namespace ArkheideSystem.Flourish.WPF;

/// <summary>Owns a scoped Design dictionary and its native Windows appearance subscriptions.</summary>
public sealed class ThemeSession : IDisposable
{
    private readonly FrameworkElement scope;
    private readonly ResourceDictionary resources = new();
    private AppearancePalette palette;
    private bool subscribed;
    private bool disposed;

    internal ThemeSession(FrameworkElement scope, ThemeMode mode, AppearancePalette palette)
    {
        this.scope = scope;
        this.palette = palette;
        Mode = mode;
        Refresh();
        scope.Resources.MergedDictionaries.Add(resources);
        scope.Loaded += Loaded;
        scope.Unloaded += Unloaded;
        if (scope is Window window) window.Closed += WindowClosed;
        if (scope.IsLoaded) Subscribe();
    }

    public ThemeMode Mode { get; private set; }
    public ThemeMode EffectiveTheme { get; private set; }
    public AppearancePalette Palette => palette;
    public bool HighContrast { get; private set; }
    public bool ReduceMotion { get; private set; }
    public event EventHandler? Changed;

    public void SetTheme(ThemeMode mode)
    {
        Check();
        DesignResources.ValidateTheme(mode);
        if (mode == Mode) return;
        Mode = mode;
        Refresh();
    }

    public void SetColors(string primary, string accent)
    {
        Check();
        var next = AppearancePalette.Create(primary, accent);
        if (next == palette) return;
        palette = next;
        Refresh();
    }

    private void Check()
    {
        scope.Dispatcher.VerifyAccess();
        ObjectDisposedException.ThrowIf(disposed, this);
    }

    private void Loaded(object sender, RoutedEventArgs args)
    {
        if (disposed) return;
        Subscribe();
        Refresh();
    }

    private void Unloaded(object sender, RoutedEventArgs args) => Unsubscribe();
    private void WindowClosed(object? sender, EventArgs args) => Dispose();

    private void Subscribe()
    {
        if (subscribed || disposed) return;
        SystemParameters.StaticPropertyChanged += SystemParameterChanged;
        SystemEvents.UserPreferenceChanged += UserPreferenceChanged;
        subscribed = true;
    }

    private void Unsubscribe()
    {
        if (!subscribed) return;
        SystemParameters.StaticPropertyChanged -= SystemParameterChanged;
        SystemEvents.UserPreferenceChanged -= UserPreferenceChanged;
        subscribed = false;
    }

    private void SystemParameterChanged(object? sender, PropertyChangedEventArgs args) => RefreshFromSystem();
    private void UserPreferenceChanged(object sender, UserPreferenceChangedEventArgs args) => RefreshFromSystem();

    private void RefreshFromSystem()
    {
        if (disposed || !subscribed || scope.Dispatcher.HasShutdownStarted) return;
        if (scope.Dispatcher.CheckAccess()) Refresh();
        else scope.Dispatcher.BeginInvoke(DispatcherPriority.DataBind, new Action(() =>
        {
            if (!disposed && subscribed) Refresh();
        }));
    }

    private void Refresh()
    {
        EffectiveTheme = DesignResources.Resolve(Mode);
        HighContrast = SystemParameters.HighContrast;
        ReduceMotion = !SystemParameters.ClientAreaAnimation || HighContrast;
        DesignResources.Populate(resources, EffectiveTheme, palette);
        Changed?.Invoke(this, EventArgs.Empty);
    }

    public void Dispose()
    {
        scope.Dispatcher.VerifyAccess();
        if (disposed) return;
        disposed = true;
        Unsubscribe();
        scope.Loaded -= Loaded;
        scope.Unloaded -= Unloaded;
        if (scope is Window window) window.Closed -= WindowClosed;
        scope.Resources.MergedDictionaries.Remove(resources);
        Changed = null;
    }
}
