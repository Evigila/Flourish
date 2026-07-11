using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using ArkheideSystem.Flourish.Abstract;
using ArkheideSystem.Flourish.Configuration;
using Brushes = System.Windows.Media.Brushes;
using Colors = System.Windows.Media.Colors;
using MediaBrush = System.Windows.Media.Brush;
using MediaColor = System.Windows.Media.Color;

namespace ArkheideSystem.Flourish.Services;

internal sealed class MaterialEffectService(FlourishShellOptions? options = null)
    : IMaterialEffectService
{
    private const int Succeeded = 0;
    private const int DwmwaUseImmersiveDarkMode = 20;
    private const int DwmwaSystemBackdropType = 38;
    private const int DwmsbtNone = 1;
    private const int DwmsbtMainWindow = 2;
    private const int WmDwmCompositionChanged = 0x031E;

    private readonly object stateGate = new();
    private Window? owner;
    private HwndSource? hwndSource;
    private MediaBrush? originalBackground;
    private MediaColor originalCompositionBackground;
    private bool hasOriginalCompositionBackground;
    private bool isSourceInitializationPending;

    public MaterialEffect CurrentEffect { get; private set; } =
        options is { IsMaterialEffectEnabled: true }
            ? options.MaterialEffect
            : MaterialEffect.None;

    public bool IsApplied { get; private set; }

    public bool IsDarkMode { get; private set; }

    public event EventHandler<FlourishMaterialEffectChangedEventArgs>? Changed;

    public bool IsSupported(MaterialEffect effect)
    {
        ValidateEffect(effect, nameof(effect));
        return effect switch
        {
            MaterialEffect.None => true,
            MaterialEffect.Mica => IsSystemBackdropSupported(),
            _ => false,
        };
    }

    public void SetEffect(MaterialEffect effect)
    {
        ValidateEffect(effect, nameof(effect));
        bool effectChanged;
        lock (stateGate)
        {
            effectChanged = CurrentEffect != effect;
            CurrentEffect = effect;
            if (options is not null)
            {
                options.MaterialEffect = effect;
                options.IsMaterialEffectEnabled = effect != MaterialEffect.None;
            }
        }

        var attachedOwner = owner;
        if (attachedOwner is not null)
        {
            RunOnWindowDispatcher(attachedOwner, () => ApplyCurrentEffectCore(attachedOwner));
        }
        else
        {
            lock (stateGate)
            {
                IsApplied = false;
            }
        }

        if (effectChanged)
        {
            RaiseChanged();
        }
    }

    public void SetDarkMode(bool isDarkMode)
    {
        lock (stateGate)
        {
            if (IsDarkMode == isDarkMode)
            {
                return;
            }

            IsDarkMode = isDarkMode;
        }
        var attachedOwner = owner;
        if (attachedOwner is not null)
        {
            RunOnWindowDispatcher(attachedOwner, () =>
            {
                var hwnd = new WindowInteropHelper(attachedOwner).Handle;
                ApplyDarkMode(hwnd, isDarkMode);
            });
        }

        RaiseChanged();
    }

    internal void Attach(Window window, MaterialEffect effect)
    {
        ArgumentNullException.ThrowIfNull(window);
        ValidateEffect(effect, nameof(effect));

        if (owner is not null && owner != window && isSourceInitializationPending)
        {
            owner.SourceInitialized -= Owner_SourceInitialized;
            isSourceInitializationPending = false;
        }

        var ownerChanged = owner != window;
        if (ownerChanged && hwndSource is not null)
        {
            hwndSource.RemoveHook(WindowProc);
            hwndSource = null;
        }

        owner = window;
        if (ownerChanged)
        {
            originalBackground = window.Background;
            hasOriginalCompositionBackground = false;
        }
        lock (stateGate)
        {
            CurrentEffect = effect;
        }

        if (new WindowInteropHelper(window).Handle != IntPtr.Zero)
        {
            AttachWindowSource(window);
            ApplyCurrentEffectCore(window);
            return;
        }

        if (!isSourceInitializationPending)
        {
            isSourceInitializationPending = true;
            window.SourceInitialized += Owner_SourceInitialized;
        }
    }

    internal void Detach(Window window)
    {
        if (owner != window)
        {
            return;
        }

        if (isSourceInitializationPending)
        {
            window.SourceInitialized -= Owner_SourceInitialized;
            isSourceInitializationPending = false;
        }

        hwndSource?.RemoveHook(WindowProc);
        hwndSource = null;
        RunOnWindowDispatcher(window, () => RemoveEffectCore(window));
        owner = null;
    }

    internal void SetDarkMode(Window window, bool isDarkMode)
    {
        ArgumentNullException.ThrowIfNull(window);
        if (owner != window)
        {
            Attach(window, CurrentEffect);
        }

        SetDarkMode(isDarkMode);
    }

    internal void Reapply(Window window)
    {
        ArgumentNullException.ThrowIfNull(window);
        if (!ReferenceEquals(owner, window))
        {
            return;
        }

        RunOnWindowDispatcher(window, () => ApplyCurrentEffectCore(window));
    }

    private void Owner_SourceInitialized(object? sender, EventArgs e)
    {
        if (sender is not Window window || owner != window)
        {
            return;
        }

        window.SourceInitialized -= Owner_SourceInitialized;
        isSourceInitializationPending = false;
        AttachWindowSource(window);
        ApplyCurrentEffectCore(window);
    }

    private void AttachWindowSource(Window window)
    {
        var hwnd = new WindowInteropHelper(window).Handle;
        var source = hwnd == IntPtr.Zero ? null : HwndSource.FromHwnd(hwnd);
        if (ReferenceEquals(hwndSource, source))
        {
            return;
        }

        hwndSource?.RemoveHook(WindowProc);
        hwndSource = source;
        hwndSource?.AddHook(WindowProc);
    }

    private IntPtr WindowProc(
        IntPtr hwnd,
        int message,
        IntPtr wParam,
        IntPtr lParam,
        ref bool handled
    )
    {
        if (
            message == WmDwmCompositionChanged
            && owner is { } attachedOwner
            && new WindowInteropHelper(attachedOwner).Handle == hwnd
        )
        {
            ApplyCurrentEffectCore(attachedOwner);
        }

        return IntPtr.Zero;
    }

    private void ApplyCurrentEffectCore(Window window)
    {
        if (CurrentEffect == MaterialEffect.None)
        {
            RemoveEffectCore(window);
            return;
        }

        if (!IsSupported(CurrentEffect))
        {
            RemoveEffectCore(window);
            return;
        }

        switch (CurrentEffect)
        {
            case MaterialEffect.Mica:
                ApplyMicaCore(window);
                break;
            case MaterialEffect.None:
                RemoveEffectCore(window);
                break;
            default:
                throw new ArgumentOutOfRangeException(
                    nameof(CurrentEffect),
                    CurrentEffect,
                    "Unknown material effect."
                );
        }
    }

    private void ApplyMicaCore(Window window)
    {
        var hwnd = new WindowInteropHelper(window).Handle;
        if (hwnd == IntPtr.Zero)
        {
            lock (stateGate)
            {
                IsApplied = false;
            }
            return;
        }

        window.Background = Brushes.Transparent;
        if (HwndSource.FromHwnd(hwnd) is { CompositionTarget: { } compositionTarget })
        {
            if (!hasOriginalCompositionBackground)
            {
                originalCompositionBackground = compositionTarget.BackgroundColor;
                hasOriginalCompositionBackground = true;
            }

            compositionTarget.BackgroundColor = Colors.Transparent;
        }

        var frameMargins = DwmFrameMargins.ExtendAcrossClientArea;
        var frameExtended =
            DwmExtendFrameIntoClientArea(hwnd, ref frameMargins) == Succeeded;

        var backdropType = DwmsbtMainWindow;
        var backdropApplied =
            DwmSetWindowAttribute(
                hwnd,
                DwmwaSystemBackdropType,
                ref backdropType,
                Marshal.SizeOf<int>()
            ) == Succeeded;
        lock (stateGate)
        {
            IsApplied = frameExtended && backdropApplied;
        }
        ApplyDarkMode(hwnd, IsDarkMode);
    }

    private void RemoveEffectCore(Window window)
    {
        var hwnd = new WindowInteropHelper(window).Handle;
        if (hwnd != IntPtr.Zero)
        {
            var frameMargins = DwmFrameMargins.None;
            DwmExtendFrameIntoClientArea(hwnd, ref frameMargins);
        }

        if (hwnd != IntPtr.Zero && IsSystemBackdropSupported())
        {
            var backdropType = DwmsbtNone;
            DwmSetWindowAttribute(
                hwnd,
                DwmwaSystemBackdropType,
                ref backdropType,
                Marshal.SizeOf<int>()
            );
        }

        window.Background = originalBackground;
        if (
            hwnd != IntPtr.Zero
            && hasOriginalCompositionBackground
            && HwndSource.FromHwnd(hwnd) is { CompositionTarget: { } compositionTarget }
        )
        {
            compositionTarget.BackgroundColor = originalCompositionBackground;
        }

        lock (stateGate)
        {
            IsApplied = false;
        }
        ApplyDarkMode(hwnd, IsDarkMode);
    }

    private void RaiseChanged()
    {
        void RaiseCore()
        {
            MaterialEffect effect;
            bool isApplied;
            bool isDarkMode;
            lock (stateGate)
            {
                effect = CurrentEffect;
                isApplied = IsApplied;
                isDarkMode = IsDarkMode;
            }

            Changed?.Invoke(
                this,
                new FlourishMaterialEffectChangedEventArgs(
                    effect,
                    IsSupported(effect),
                    isApplied,
                    isDarkMode
                )
            );
        }

        var attachedOwner = owner;
        if (attachedOwner is null || attachedOwner.Dispatcher.CheckAccess())
        {
            RaiseCore();
            return;
        }

        attachedOwner.Dispatcher.Invoke(RaiseCore);
    }

    private static void ApplyDarkMode(IntPtr hwnd, bool isDarkMode)
    {
        if (hwnd == IntPtr.Zero || !IsImmersiveDarkModeSupported())
        {
            return;
        }

        var darkMode = isDarkMode ? 1 : 0;
        DwmSetWindowAttribute(
            hwnd,
            DwmwaUseImmersiveDarkMode,
            ref darkMode,
            Marshal.SizeOf<int>()
        );
    }

    private static bool IsSystemBackdropSupported()
    {
        return OperatingSystem.IsWindowsVersionAtLeast(10, 0, 22621);
    }

    private static bool IsImmersiveDarkModeSupported()
    {
        return OperatingSystem.IsWindowsVersionAtLeast(10, 0, 22000);
    }

    private static void ValidateEffect(MaterialEffect effect, string parameterName)
    {
        if (!Enum.IsDefined(effect))
        {
            throw new ArgumentOutOfRangeException(
                parameterName,
                effect,
                "Unknown material effect."
            );
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

    [DllImport("dwmapi.dll", PreserveSig = true)]
    private static extern int DwmExtendFrameIntoClientArea(
        IntPtr hwnd,
        ref DwmFrameMargins margins
    );

    [DllImport("dwmapi.dll", PreserveSig = true)]
    private static extern int DwmSetWindowAttribute(
        IntPtr hwnd,
        int dwAttribute,
        ref int pvAttribute,
        int cbAttribute
    );

    [StructLayout(LayoutKind.Sequential)]
    private readonly struct DwmFrameMargins
    {
        public static DwmFrameMargins ExtendAcrossClientArea => new(-1, -1, -1, -1);

        public static DwmFrameMargins None => new(0, 0, 0, 0);

        private readonly int leftWidth;

        private readonly int rightWidth;

        private readonly int topHeight;

        private readonly int bottomHeight;

        private DwmFrameMargins(
            int leftWidth,
            int rightWidth,
            int topHeight,
            int bottomHeight
        )
        {
            this.leftWidth = leftWidth;
            this.rightWidth = rightWidth;
            this.topHeight = topHeight;
            this.bottomHeight = bottomHeight;
        }
    }
}
