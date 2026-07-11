using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Shell;
using ArkheideSystem.Flourish.Abstract;
using Brushes = System.Windows.Media.Brushes;
using Colors = System.Windows.Media.Colors;

namespace ArkheideSystem.Flourish.Services;

internal sealed class MaterialEffectService
{
    private const int Succeeded = 0;
    private const int DwmwaUseImmersiveDarkMode = 20;
    private const int DwmwaSystemBackdropType = 38;
    private const int DwmsbtMainWindow = 2;

    public MaterialEffect CurrentEffect { get; private set; }

    public bool IsApplied { get; private set; }

    public bool IsDarkMode { get; private set; }

    public void Attach(Window window, MaterialEffect effect)
    {
        CurrentEffect = effect;
        if (effect == MaterialEffect.None)
        {
            return;
        }

        if (!IsSystemBackdropSupported())
        {
            return;
        }

        if (new WindowInteropHelper(window).Handle != IntPtr.Zero)
        {
            Apply(window, effect);
            return;
        }

        window.SourceInitialized += Window_SourceInitialized;

        void Window_SourceInitialized(object? sender, EventArgs e)
        {
            window.SourceInitialized -= Window_SourceInitialized;
            Apply(window, effect);
        }
    }

    private void Apply(Window window, MaterialEffect effect)
    {
        if (effect != MaterialEffect.Mica)
        {
            return;
        }

        var hwnd = new WindowInteropHelper(window).Handle;
        if (hwnd == IntPtr.Zero)
        {
            return;
        }

        window.Background = Brushes.Transparent;
        if (HwndSource.FromHwnd(hwnd) is { CompositionTarget: { } compositionTarget })
        {
            compositionTarget.BackgroundColor = Colors.Transparent;
        }

        if (WindowChrome.GetWindowChrome(window) is { } chrome)
        {
            chrome.GlassFrameThickness = new Thickness(-1);
        }

        var backdropType = DwmsbtMainWindow;
        IsApplied =
            DwmSetWindowAttribute(
                hwnd,
                DwmwaSystemBackdropType,
                ref backdropType,
                Marshal.SizeOf<int>()
            ) == Succeeded;

        ApplyDarkMode(hwnd, IsDarkMode);
    }

    public void SetDarkMode(Window window, bool isDarkMode)
    {
        IsDarkMode = isDarkMode;
        if (!IsImmersiveDarkModeSupported())
        {
            return;
        }

        if (new WindowInteropHelper(window).Handle != IntPtr.Zero)
        {
            ApplyDarkMode(new WindowInteropHelper(window).Handle, isDarkMode);
            return;
        }

        window.SourceInitialized += Window_SourceInitialized;

        void Window_SourceInitialized(object? sender, EventArgs e)
        {
            window.SourceInitialized -= Window_SourceInitialized;
            ApplyDarkMode(new WindowInteropHelper(window).Handle, isDarkMode);
        }
    }

    private static void ApplyDarkMode(IntPtr hwnd, bool isDarkMode)
    {
        if (hwnd == IntPtr.Zero || !IsImmersiveDarkModeSupported())
        {
            return;
        }

        var darkMode = isDarkMode ? 1 : 0;
        DwmSetWindowAttribute(hwnd, DwmwaUseImmersiveDarkMode, ref darkMode, Marshal.SizeOf<int>());
    }

    private static bool IsSystemBackdropSupported()
    {
        return OperatingSystem.IsWindowsVersionAtLeast(10, 0, 22621);
    }

    private static bool IsImmersiveDarkModeSupported()
    {
        return OperatingSystem.IsWindowsVersionAtLeast(10, 0, 22000);
    }

    [DllImport("dwmapi.dll", PreserveSig = true)]
    private static extern int DwmSetWindowAttribute(
        IntPtr hwnd,
        int dwAttribute,
        ref int pvAttribute,
        int cbAttribute
    );
}
