using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using ArkheideSystem.Flourish.Abstract;
using ArkheideSystem.Flourish.Internal.Configuration;
using ArkheideSystem.Flourish.Themes;
using Microsoft.Win32;
using Application = System.Windows.Application;
using Color = System.Windows.Media.Color;
using Colors = System.Windows.Media.Colors;

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
        ApplyStyleOverrides(application.Resources, shellOptions, EffectiveTheme);
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
                ApplyStyleOverrides(application.Resources, shellOptions, effective);
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

    internal static void ApplyStyleOverrides(
        ResourceDictionary resources,
        FlourishShellOptions options,
        FlourishTheme effectiveTheme = FlourishTheme.Light
    )
    {
        ArgumentNullException.ThrowIfNull(resources);
        ArgumentNullException.ThrowIfNull(options);

        if (options.ThemeColors is { } colors)
        {
            ApplyColorOverrides(resources, colors, effectiveTheme);
        }

        if (options.CornerRadius is { } radius)
        {
            var cornerRadius = new CornerRadius(radius);
            resources["FlourishControlCornerRadius"] = cornerRadius;
            resources["FlourishSurfaceCornerRadius"] = cornerRadius;
            resources["FlourishOverlayCornerRadius"] = cornerRadius;
            resources["FlourishDialogCornerRadius"] = cornerRadius;
            resources["FlourishDialogFooterCornerRadius"] =
                new CornerRadius(0, 0, radius, radius);
        }
    }

    private static void ApplyColorOverrides(
        ResourceDictionary resources,
        FlourishThemeColors colors,
        FlourishTheme effectiveTheme
    )
    {
        var isDark = effectiveTheme == FlourishTheme.Dark;
        var neutralBackground = isDark
            ? Color.FromRgb(0x14, 0x14, 0x14)
            : Colors.White;
        var controlBackground = isDark
            ? Color.FromRgb(0x29, 0x29, 0x29)
            : Colors.White;
        var neutralForeground = isDark
            ? Color.FromRgb(0xF8, 0xF8, 0xFA)
            : Color.FromRgb(0x1B, 0x1B, 0x1F);
        var cardLayer = isDark
            ? Color.FromArgb(0xF0, 0x2B, 0x2D, 0x31)
            : Color.FromArgb(0xFA, 0xFF, 0xFF, 0xFF);
        var cardBackgroundOnNeutral = Composite(cardLayer, neutralBackground);
        var cardBackgroundOnControl = Composite(cardLayer, controlBackground);
        var foregroundTarget = isDark ? Colors.White : Colors.Black;
        var primaryForeground = EnsureContrast(
            colors.Primary,
            neutralBackground,
            foregroundTarget
        );
        var secondaryForeground = EnsureContrast(
            colors.Secondary,
            neutralBackground,
            foregroundTarget
        );
        var accentForeground = EnsureContrast(
            colors.Accent,
            neutralBackground,
            foregroundTarget
        );
        var primaryHover = Blend(
            colors.Primary,
            isDark ? Colors.White : Colors.Black,
            0.12
        );
        var primaryPressed = Blend(colors.Primary, Colors.Black, 0.24);
        var primarySurface = CreateSurface(colors.Primary);
        var foregroundOnPrimary = GetContrastingForeground(colors.Primary);
        var secondaryHover = Blend(
            colors.Secondary,
            isDark ? Colors.White : Colors.Black,
            0.12
        );
        var secondaryPressed = Blend(colors.Secondary, Colors.Black, 0.24);
        var secondarySurface = CreateSurface(colors.Secondary);
        var foregroundOnSecondary = GetContrastingForeground(colors.Secondary);
        var accentHover = Blend(
            colors.Accent,
            isDark ? Colors.White : Colors.Black,
            0.12
        );
        var accentPressed = Blend(colors.Accent, Colors.Black, 0.24);
        var accentSurface = CreateSurface(colors.Accent);
        var foregroundOnAccent = GetContrastingForeground(colors.Accent);
        var selectedBackground = Composite(
            Color.FromArgb(0x2E, colors.Primary.R, colors.Primary.G, colors.Primary.B),
            neutralBackground
        );
        var hoverReveal = CreateContrastSafeReveal(
            colors.Primary,
            isDark ? (byte)0x66 : (byte)0x59,
            (neutralForeground, neutralBackground),
            (neutralForeground, controlBackground),
            (neutralForeground, cardBackgroundOnNeutral),
            (neutralForeground, cardBackgroundOnControl)
        );
        var selectedForeground = EnsureContrastAcrossBackgrounds(
            primaryForeground,
            foregroundTarget,
            selectedBackground,
            Composite(hoverReveal, selectedBackground)
        );
        hoverReveal = CreateContrastSafeReveal(
            colors.Primary,
            hoverReveal.A,
            (neutralForeground, neutralBackground),
            (neutralForeground, controlBackground),
            (neutralForeground, cardBackgroundOnNeutral),
            (neutralForeground, cardBackgroundOnControl),
            (selectedForeground, selectedBackground)
        );
        var pressedReveal = CreateContrastSafeReveal(
            primaryPressed,
            isDark ? (byte)0x73 : (byte)0x66,
            (neutralForeground, neutralBackground),
            (neutralForeground, controlBackground),
            (neutralForeground, cardBackgroundOnNeutral),
            (neutralForeground, cardBackgroundOnControl)
        );

        resources["FlourishPrimaryColor"] = colors.Primary;
        resources["FlourishSecondaryColor"] = colors.Secondary;
        resources["FlourishAccentColor"] = colors.Accent;

        SetBrush(resources, "FlourishPrimaryBrush", colors.Primary);
        SetBrush(resources, "FlourishPrimaryHoverBrush", primaryHover);
        SetBrush(resources, "FlourishPrimaryPressedBrush", primaryPressed);
        SetBrush(resources, "FlourishPrimarySurfaceBrush", primarySurface);
        SetBrush(resources, "FlourishForegroundOnPrimaryBrush", foregroundOnPrimary);
        SetBrush(resources, "FlourishPrimaryForegroundBrush", primaryForeground);
        SetBrush(resources, "FlourishPrimaryBackgroundBrush", colors.Primary);
        SetBrush(resources, "FlourishPrimaryBackgroundHoverBrush", primaryHover);
        SetBrush(resources, "FlourishPrimaryBackgroundPressedBrush", primaryPressed);

        SetBrush(resources, "FlourishSecondaryBrush", colors.Secondary);
        SetBrush(resources, "FlourishSecondaryHoverBrush", secondaryHover);
        SetBrush(resources, "FlourishSecondaryPressedBrush", secondaryPressed);
        SetBrush(resources, "FlourishSecondarySurfaceBrush", secondarySurface);
        SetBrush(resources, "FlourishForegroundOnSecondaryBrush", foregroundOnSecondary);
        SetBrush(resources, "FlourishSecondaryForegroundBrush", secondaryForeground);

        SetBrush(resources, "FlourishAccentBrush", colors.Accent);
        SetBrush(resources, "FlourishAccentHoverBrush", accentHover);
        SetBrush(resources, "FlourishAccentPressedBrush", accentPressed);
        SetBrush(resources, "FlourishAccentSurfaceBrush", accentSurface);
        SetBrush(resources, "FlourishForegroundOnAccentBrush", foregroundOnAccent);
        SetBrush(resources, "FlourishAccentForegroundBrush", accentForeground);
        SetHeroBackground(resources, colors);

        // Keep existing Flourish controls aligned with the new semantic palette.
        SetBrush(resources, "FlourishBrandForegroundBrush", primaryForeground);
        SetBrush(resources, "FlourishBrandBackgroundBrush", colors.Primary);
        SetBrush(resources, "FlourishBrandBackgroundHoverBrush", primaryHover);
        SetBrush(resources, "FlourishBrandBackgroundPressedBrush", primaryPressed);
        SetBrush(resources, "FlourishForegroundOnBrandBrush", foregroundOnPrimary);
        SetBrush(resources, "FlourishControlStrokeFocusBrush", accentForeground);
        SetBrush(resources, "FlourishControlSelectedBrush", selectedBackground);
        SetBrush(resources, "FlourishControlSelectedForegroundBrush", selectedForeground);
        SetBrush(resources, "FlourishHoverRevealBrush", hoverReveal);
        SetBrush(resources, "FlourishPressedRevealBrush", pressedReveal);
        SetBrush(resources, "FlourishControlSelectedHoverBrush", hoverReveal);
        SetBrush(resources, "FlourishSelectionBackgroundBrush", selectedBackground);
        SetBrush(resources, "FlourishProfileBackgroundBrush", secondarySurface);
        SetBrush(resources, "FlourishProfileForegroundBrush", secondaryForeground);
        SetBrush(resources, "FlourishMessageBoxInfoIconBackgroundBrush", primarySurface);
        SetBrush(resources, "FlourishMessageBoxInfoIconForegroundBrush", primaryForeground);
        SetBrush(resources, "FlourishPrimaryButtonHoverBrush", primaryHover);
        SetBrush(resources, "FlourishPrimaryButtonPressedBrush", primaryPressed);
    }

    private static void SetBrush(ResourceDictionary resources, string key, Color color)
    {
        var brush = new SolidColorBrush(color);
        if (brush.CanFreeze)
        {
            brush.Freeze();
        }

        resources[key] = brush;
    }

    private static void SetHeroBackground(
        ResourceDictionary resources,
        FlourishThemeColors colors
    )
    {
        var brush = new LinearGradientBrush
        {
            StartPoint = new System.Windows.Point(0, 0),
            EndPoint = new System.Windows.Point(1, 1),
            GradientStops =
            {
                new GradientStop(CreateSurface(colors.Primary), 0),
                new GradientStop(CreateSurface(colors.Secondary), 0.68),
                new GradientStop(CreateSurface(colors.Accent), 1),
            },
        };
        if (brush.CanFreeze)
        {
            brush.Freeze();
        }

        resources["FlourishHeroBackgroundBrush"] = brush;
    }

    private static Color Blend(Color source, Color target, double amount)
    {
        return Color.FromArgb(
            source.A,
            BlendChannel(source.R, target.R, amount),
            BlendChannel(source.G, target.G, amount),
            BlendChannel(source.B, target.B, amount)
        );
    }

    private static byte BlendChannel(byte source, byte target, double amount)
    {
        return (byte)Math.Round(source + ((target - source) * amount));
    }

    private static Color CreateSurface(Color color)
    {
        return Color.FromArgb(Math.Min(color.A, (byte)0x24), color.R, color.G, color.B);
    }

    private static Color CreateContrastSafeReveal(
        Color color,
        byte maximumAlpha,
        params (Color Foreground, Color Background)[] surfaces
    )
    {
        const double minimumContrast = 4.5;
        for (var alpha = (int)maximumAlpha; alpha >= 0; alpha--)
        {
            var candidate = Color.FromArgb((byte)alpha, color.R, color.G, color.B);
            var isReadable = true;
            foreach (var (foreground, background) in surfaces)
            {
                if (
                    GetContrastRatio(foreground, Composite(candidate, background))
                    < minimumContrast
                )
                {
                    isReadable = false;
                    break;
                }
            }

            if (isReadable)
            {
                return candidate;
            }
        }

        return Color.FromArgb(0, color.R, color.G, color.B);
    }

    private static Color EnsureContrast(
        Color foreground,
        Color background,
        Color target
    )
    {
        const double minimumContrast = 4.5;
        if (GetContrastRatio(foreground, background) >= minimumContrast)
        {
            return foreground;
        }

        for (var step = 1; step <= 20; step++)
        {
            var candidate = Blend(foreground, target, step / 20d);
            if (GetContrastRatio(candidate, background) >= minimumContrast)
            {
                return candidate;
            }
        }

        return target;
    }

    private static Color EnsureContrastAcrossBackgrounds(
        Color foreground,
        Color target,
        params Color[] backgrounds
    )
    {
        const double minimumContrast = 4.5;
        if (
            backgrounds.All(
                background => GetContrastRatio(foreground, background) >= minimumContrast
            )
        )
        {
            return foreground;
        }

        for (var step = 1; step <= 20; step++)
        {
            var candidate = Blend(foreground, target, step / 20d);
            if (
                backgrounds.All(
                    background => GetContrastRatio(candidate, background) >= minimumContrast
                )
            )
            {
                return candidate;
            }
        }

        return target;
    }

    private static Color Composite(Color foreground, Color background)
    {
        var alpha = foreground.A / 255d;
        return Color.FromRgb(
            (byte)Math.Round((foreground.R * alpha) + (background.R * (1 - alpha))),
            (byte)Math.Round((foreground.G * alpha) + (background.G * (1 - alpha))),
            (byte)Math.Round((foreground.B * alpha) + (background.B * (1 - alpha)))
        );
    }

    private static double GetContrastRatio(Color first, Color second)
    {
        var lighter = Math.Max(GetRelativeLuminance(first), GetRelativeLuminance(second));
        var darker = Math.Min(GetRelativeLuminance(first), GetRelativeLuminance(second));
        return (lighter + 0.05) / (darker + 0.05);
    }

    private static Color GetContrastingForeground(Color background)
    {
        return GetRelativeLuminance(background) > 0.179 ? Colors.Black : Colors.White;
    }

    private static double GetRelativeLuminance(Color color)
    {
        return (0.2126 * Linearize(color.R))
            + (0.7152 * Linearize(color.G))
            + (0.0722 * Linearize(color.B));
    }

    private static double Linearize(byte channel)
    {
        var value = channel / 255d;
        return value <= 0.04045
            ? value / 12.92
            : Math.Pow((value + 0.055) / 1.055, 2.4);
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
