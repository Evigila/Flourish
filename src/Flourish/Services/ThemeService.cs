using System.Windows;
using System.Windows.Interop;
using AcksheedSys.Flourish.Abstract;
using AcksheedSys.Flourish.Configuration;
using Microsoft.Win32;
using Application = System.Windows.Application;

namespace AcksheedSys.Flourish.Services;

internal sealed class ThemeService(
    FlourishShellOptions shellOptions,
    AppPreferenceService preferenceService
)
{
    private const int WmSettingChange = 0x001A;
    private const int WmThemeChanged = 0x031A;
    private const string LightThemeSource = "/Flourish;component/Themes/Colors.Light.xaml";
    private const string DarkThemeSource = "/Flourish;component/Themes/Colors.Dark.xaml";
    private const string PersonalizeRegistryPath =
        @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize";
    private const string AppsUseLightThemeValue = "AppsUseLightTheme";

    private readonly Dictionary<Window, HwndSource> hooksByWindow = [];
    private bool isInitialized;

    public event EventHandler<FlourishThemeChangedEventArgs>? ThemeChanged;

    public FlourishTheme CurrentTheme { get; private set; } = FlourishTheme.Light;

    public FlourishTheme EffectiveTheme { get; private set; } = FlourishTheme.Light;

    public bool IsDark => EffectiveTheme == FlourishTheme.Dark;

    public void Initialize(Application application)
    {
        ArgumentNullException.ThrowIfNull(application);

        if (isInitialized)
        {
            return;
        }

        CurrentTheme = shellOptions.IsThemeEnabled
            ? preferenceService.ReadTheme() ?? shellOptions.DefaultTheme
            : FlourishTheme.Light;
        EffectiveTheme = ResolveTheme(CurrentTheme);
        ApplyApplicationResources(application, EffectiveTheme);
        isInitialized = true;
    }

    public void Attach(Window window)
    {
        if (!shellOptions.IsThemeEnabled)
        {
            return;
        }

        if (new WindowInteropHelper(window).Handle != IntPtr.Zero)
        {
            AttachHook(window);
            return;
        }

        window.SourceInitialized += Window_SourceInitialized;

        void Window_SourceInitialized(object? sender, EventArgs e)
        {
            window.SourceInitialized -= Window_SourceInitialized;
            AttachHook(window);
        }
    }

    public void Detach(Window window)
    {
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
        if (!shellOptions.IsThemeEnabled)
        {
            return;
        }

        var requestedThemeChanged = CurrentTheme != theme;
        CurrentTheme = theme;
        preferenceService.SaveTheme(theme);
        ApplyTheme(notify: true, forceNotify: requestedThemeChanged);
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

    private void ApplyTheme(bool notify, bool forceNotify = false)
    {
        var application = Application.Current;
        if (application is null)
        {
            return;
        }

        var effectiveTheme = ResolveTheme(CurrentTheme);
        var changed = effectiveTheme != EffectiveTheme;
        EffectiveTheme = effectiveTheme;
        ApplyApplicationResources(application, effectiveTheme);

        if (notify && (changed || forceNotify))
        {
            ThemeChanged?.Invoke(
                this,
                new FlourishThemeChangedEventArgs(CurrentTheme, EffectiveTheme)
            );
        }
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
        RemoveThemeResources(application.Resources.MergedDictionaries);

        application.Resources.MergedDictionaries.Add(
            new ResourceDictionary
            {
                Source = new Uri(
                    theme == FlourishTheme.Dark ? DarkThemeSource : LightThemeSource,
                    UriKind.Relative
                ),
            }
        );
    }

    private static void RemoveThemeResources(IList<ResourceDictionary> dictionaries)
    {
        for (var index = dictionaries.Count - 1; index >= 0; index--)
        {
            var source = dictionaries[index].Source?.OriginalString;
            if (source is LightThemeSource or DarkThemeSource)
            {
                dictionaries.RemoveAt(index);
            }
        }
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
}
