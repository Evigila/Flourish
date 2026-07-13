using System.Windows;
using System.Windows.Interop;
using ArkheideSystem.Flourish.Abstract;
using ArkheideSystem.Flourish.Internal.Configuration;
using ArkheideSystem.Flourish.Themes;
using Microsoft.Win32;
using Application = System.Windows.Application;

namespace ArkheideSystem.Flourish.Services;

internal sealed class ThemeService(
    FlourishShellOptions shellOptions,
    AppPreferenceService preferenceService
) : IThemeService
{
    private const int WmSettingChange = 0x001A;
    private const int WmThemeChanged = 0x031A;
    private const string LightThemeSource = "/Flourish;component/Themes/Colors/Colors.Light.xaml";
    private const string DarkThemeSource = "/Flourish;component/Themes/Colors/Colors.Dark.xaml";
    private const string PaletteHostSource = "/Flourish;component/Themes/Colors/Colors.xaml";
    private const string PersonalizeRegistryPath =
        @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize";
    private const string AppsUseLightThemeValue = "AppsUseLightTheme";

    private readonly object gate = new();
    private readonly Dictionary<Window, HwndSource> hooksByWindow = [];
    private readonly HashSet<Window> attachedWindows = [];
    private readonly HashSet<Window> pendingHookWindows = [];
    private FlourishTheme currentTheme = FlourishTheme.Light;
    private FlourishTheme effectiveTheme = FlourishTheme.Light;
    private bool isInitialized;

    public event EventHandler<FlourishThemeChangedEventArgs>? ThemeChanged;

    public FlourishTheme CurrentTheme
    {
        get
        {
            lock (gate)
            {
                return currentTheme;
            }
        }
    }

    public FlourishTheme EffectiveTheme
    {
        get
        {
            lock (gate)
            {
                return effectiveTheme;
            }
        }
    }

    public bool IsDark => EffectiveTheme == FlourishTheme.Dark;

    public void Initialize(Application application)
    {
        ArgumentNullException.ThrowIfNull(application);

        FlourishTheme initialTheme;
        lock (gate)
        {
            if (isInitialized)
            {
                return;
            }

            initialTheme = shellOptions.IsThemeEnabled
                ? preferenceService.ReadTheme() ?? shellOptions.DefaultTheme
                : FlourishTheme.Light;
            currentTheme = initialTheme;
            effectiveTheme = ResolveTheme(initialTheme);
            isInitialized = true;
        }

        ApplyApplicationResources(application, EffectiveTheme);
    }

    public void Attach(Window window)
    {
        ArgumentNullException.ThrowIfNull(window);
        lock (gate)
        {
            attachedWindows.Add(window);
        }

        if (!shellOptions.IsThemeEnabled)
        {
            return;
        }

        RunOnWindowDispatcher(window, () => EnsureHook(window));
    }

    public void Detach(Window window)
    {
        lock (gate)
        {
            attachedWindows.Remove(window);
        }

        RunOnWindowDispatcher(window, () => DetachCore(window));
    }

    private void DetachCore(Window window)
    {
        if (pendingHookWindows.Remove(window))
        {
            window.SourceInitialized -= Window_SourceInitialized;
        }

        if (!hooksByWindow.Remove(window, out var source))
        {
            return;
        }

        source.RemoveHook(WndProc);
    }

    public void ToggleTheme()
    {
        var nextTheme = CurrentTheme switch
        {
            FlourishTheme.System => IsDark ? FlourishTheme.Light : FlourishTheme.Dark,
            FlourishTheme.Light => FlourishTheme.Dark,
            FlourishTheme.Dark => FlourishTheme.System,
            _ => FlourishTheme.System,
        };

        SetTheme(nextTheme);
    }

    public void SetTheme(FlourishTheme theme)
    {
        ValidateTheme(theme, nameof(theme));
        bool requestedThemeChanged;
        Window[] windows;
        var runtimeApplied = new TaskCompletionSource<bool>(
            TaskCreationOptions.RunContinuationsAsynchronously
        );
        lock (gate)
        {
            requestedThemeChanged = currentTheme != theme;
            preferenceService.QueueThemeSave(theme, runtimeApplied.Task);
            shellOptions.IsThemeEnabled = true;
            currentTheme = theme;
            windows = attachedWindows.ToArray();
        }

        try
        {
            foreach (var window in windows)
            {
                RunOnWindowDispatcher(window, () => EnsureHook(window));
            }

            ApplyTheme(
                notify: true,
                forceNotify: requestedThemeChanged,
                onApplied: () => runtimeApplied.TrySetResult(true)
            );
        }
        catch
        {
            runtimeApplied.TrySetResult(false);
            throw;
        }
    }

    private void EnsureHook(Window window)
    {
        lock (gate)
        {
            if (!attachedWindows.Contains(window))
            {
                return;
            }
        }

        if (new WindowInteropHelper(window).Handle != IntPtr.Zero)
        {
            AttachHook(window);
            return;
        }

        if (pendingHookWindows.Add(window))
        {
            window.SourceInitialized += Window_SourceInitialized;
        }
    }

    private void Window_SourceInitialized(object? sender, EventArgs e)
    {
        if (sender is not Window window || !pendingHookWindows.Remove(window))
        {
            return;
        }

        window.SourceInitialized -= Window_SourceInitialized;
        AttachHook(window);
    }

    private void AttachHook(Window window)
    {
        if (hooksByWindow.ContainsKey(window))
        {
            return;
        }

        var source = HwndSource.FromHwnd(new WindowInteropHelper(window).Handle);
        if (source is null)
        {
            return;
        }

        source.AddHook(WndProc);
        hooksByWindow[window] = source;
    }

    private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        if (
            (msg == WmSettingChange || msg == WmThemeChanged)
            && CurrentTheme == FlourishTheme.System
        )
        {
            ApplyTheme(notify: true);
        }

        return IntPtr.Zero;
    }

    private void ApplyTheme(
        bool notify,
        bool forceNotify = false,
        Action? onApplied = null
    )
    {
        var application = Application.Current;
        void ApplyCore()
        {
            FlourishTheme requested;
            FlourishTheme effective;
            bool changed;
            lock (gate)
            {
                requested = currentTheme;
                effective = ResolveTheme(requested);
                changed = effective != effectiveTheme;
                effectiveTheme = effective;
            }

            if (application is not null)
            {
                ApplyApplicationResources(application, effective);
            }

            // Persistence may proceed once the runtime resources have been applied.
            // Release it before user callbacks: a ThemeChanged handler is allowed to
            // synchronously persist another setting without waiting behind this theme.
            onApplied?.Invoke();

            if (notify && (changed || forceNotify))
            {
                ThemeChanged?.Invoke(
                    this,
                    new FlourishThemeChangedEventArgs(requested, effective)
                );
            }
        }

        if (application is null || application.Dispatcher.CheckAccess())
        {
            ApplyCore();
            return;
        }

        application.Dispatcher.Invoke(ApplyCore);
    }

    private FlourishTheme ResolveTheme(FlourishTheme theme)
    {
        return theme switch
        {
            FlourishTheme.Dark => FlourishTheme.Dark,
            FlourishTheme.Light => FlourishTheme.Light,
            FlourishTheme.System => IsSystemDarkTheme()
                ? FlourishTheme.Dark
                : FlourishTheme.Light,
            _ => FlourishTheme.Light,
        };
    }

    private static void ApplyApplicationResources(Application application, FlourishTheme theme)
    {
        FlourishThemeResources.EnsureMerged(application.Resources);
        ApplyThemePalette(application.Resources, theme);
    }

    internal static void ApplyThemePalette(ResourceDictionary resources, FlourishTheme theme)
    {
        ArgumentNullException.ThrowIfNull(resources);
        var themeRoot = FlourishThemeResources.FindThemeRoot(resources)
            ?? throw new InvalidOperationException(
                $"The resource graph does not contain {FlourishThemeResources.GenericThemeSource}."
            );
        var paletteHost = FindPaletteHost(themeRoot)
            ?? throw new InvalidOperationException(
                $"The Flourish theme graph does not contain a canonical Flourish palette."
            );
        var source = theme == FlourishTheme.Dark ? DarkThemeSource : LightThemeSource;
        if (IsSource(paletteHost, source))
        {
            return;
        }

        // Colors.xaml is itself loaded through ResourceDictionary.Source. Replacing its
        // nested MergedDictionaries entry can update future lookups without invalidating
        // DynamicResource expressions that already resolved through the source-loaded
        // dictionary. Reusing the host object and changing its Source asks WPF to perform
        // one atomic resource invalidation for every existing consumer.
        paletteHost.Source = new Uri(source, UriKind.Relative);
    }

    private static ResourceDictionary? FindPaletteHost(ResourceDictionary dictionary)
    {
        return FlourishThemeResources.FindInGraph(
            dictionary,
            static candidate =>
                IsSource(candidate, PaletteHostSource)
                || IsSource(candidate, LightThemeSource)
                || IsSource(candidate, DarkThemeSource)
        );
    }

    private static bool IsSource(ResourceDictionary dictionary, string source)
    {
        return string.Equals(
            dictionary.Source?.OriginalString.Replace('\\', '/'),
            source,
            StringComparison.OrdinalIgnoreCase
        );
    }

    private static bool IsSystemDarkTheme()
    {
        if (!OperatingSystem.IsWindows())
        {
            return false;
        }

        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(PersonalizeRegistryPath);
            return key?.GetValue(AppsUseLightThemeValue) is int useLightTheme
                && useLightTheme == 0;
        }
        catch (ObjectDisposedException)
        {
            return false;
        }
        catch (UnauthorizedAccessException)
        {
            return false;
        }
    }

    private static void ValidateTheme(FlourishTheme theme, string parameterName)
    {
        if (!Enum.IsDefined(theme))
        {
            throw new ArgumentOutOfRangeException(parameterName, theme, "Unknown theme.");
        }
    }

    private static void RunOnWindowDispatcher(Window window, Action action)
    {
        if (window.Dispatcher.CheckAccess())
        {
            action();
            return;
        }

        window.Dispatcher.Invoke(action);
    }
}
