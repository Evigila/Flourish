using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using ArkheideSystem.Flourish.Abstract;
using ArkheideSystem.Flourish.Internal.Configuration;
using Brushes = System.Windows.Media.Brushes;
using Colors = System.Windows.Media.Colors;
using MediaBrush = System.Windows.Media.Brush;
using MediaColor = System.Windows.Media.Color;

namespace ArkheideSystem.Flourish.Services;

internal sealed class MaterialEffectService : IMaterialEffectService
{
    private const int Succeeded = 0;
    private const int DwmwaUseImmersiveDarkMode = 20;
    private const int DwmwaSystemBackdropType = 38;
    private const int DwmwaLegacyMicaEffect = 1029;
    private const int DwmsbtNone = 1;
    private const int DwmsbtMainWindow = 2;
    private const int DwmsbtTransientWindow = 3;
    private const int DwmsbtTabbedWindow = 4;
    private const int WcaAccentPolicy = 19;
    private const int AccentDisabled = 0;
    private const int AccentEnableAcrylicBlurBehind = 4;
    private const int WmDwmCompositionChanged = 0x031E;
    private const int LightAcrylicTint = unchecked((int)0xCCFCFAF7);
    private const int DarkAcrylicTint = unchecked((int)0xCC202020);

    private readonly Lock stateGate = new();
    private readonly FlourishShellOptions? options;
    private readonly MaterialEffectPlatform platform;
    private Window? owner;
    private HwndSource? hwndSource;
    private MediaBrush? originalBackground;
    private object? originalBackgroundResourceKey;
    private MediaColor originalCompositionBackground;
    private bool hasOriginalCompositionBackground;
    private bool isSourceInitializationPending;

    public MaterialEffectService(FlourishShellOptions? options = null)
        : this(options, MaterialEffectPlatform.Current) { }

    internal MaterialEffectService(
        FlourishShellOptions? options,
        MaterialEffectPlatform platform
    )
    {
        this.options = options;
        this.platform = platform;
        CurrentEffect = options is not { IsMaterialEffectEnabled: false }
            ? options?.MaterialEffect ?? MaterialEffect.Auto
            : MaterialEffect.None;
        ValidateEffect(CurrentEffect, nameof(options));
    }

    public MaterialEffect CurrentEffect { get; private set; }

    public MaterialEffect EffectiveEffect => platform.Resolve(CurrentEffect);

    public bool IsApplied { get; private set; }

    public bool IsDarkMode { get; private set; }

    public event EventHandler<FlourishMaterialEffectChangedEventArgs>? Changed;

    public bool IsSupported(MaterialEffect effect)
    {
        ValidateEffect(effect, nameof(effect));
        return platform.IsSupported(effect);
    }

    public void SetEffect(MaterialEffect effect)
    {
        ValidateEffect(effect, nameof(effect));
        EnsureEffectSupported(effect);
        lock (stateGate)
        {
            if (CurrentEffect == effect)
            {
                return;
            }

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

        RaiseChanged();
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
            RunOnWindowDispatcher(
                attachedOwner,
                () =>
                {
                    if (
                        EffectiveEffect == MaterialEffect.Acrylic
                        && platform.SupportsAccentAcrylic
                    )
                    {
                        ApplyCurrentEffectCore(attachedOwner);
                    }
                    else
                    {
                        var hwnd = new WindowInteropHelper(attachedOwner).Handle;
                        ApplyDarkMode(hwnd, isDarkMode);
                    }
                }
            );
        }

        RaiseChanged();
    }

    internal void Attach(Window window, MaterialEffect effect, object? backgroundResourceKey = null)
    {
        ArgumentNullException.ThrowIfNull(window);
        ValidateEffect(effect, nameof(effect));
        EnsureEffectSupported(effect);

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
            originalBackgroundResourceKey = backgroundResourceKey;
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
        originalBackgroundResourceKey = null;
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
        var backend = platform.ResolveBackend(CurrentEffect);
        if (backend == MaterialEffectBackend.None)
        {
            RemoveEffectCore(window);
            return;
        }

        var hwnd = new WindowInteropHelper(window).Handle;
        if (hwnd == IntPtr.Zero)
        {
            lock (stateGate)
            {
                IsApplied = false;
            }
            return;
        }

        ClearNativeEffect(hwnd);
        PrepareTransparentSurface(window, hwnd);

        var applied = backend switch
        {
            MaterialEffectBackend.SystemMica =>
                ApplySystemBackdrop(hwnd, DwmsbtMainWindow),
            MaterialEffectBackend.LegacyMica => ApplyLegacyMica(hwnd),
            MaterialEffectBackend.SystemAcrylic =>
                ApplySystemBackdrop(hwnd, DwmsbtTransientWindow),
            MaterialEffectBackend.AccentAcrylic => ApplyAccentAcrylic(hwnd),
            MaterialEffectBackend.SystemMicaAlt =>
                ApplySystemBackdrop(hwnd, DwmsbtTabbedWindow),
            _ => false,
        };

        if (!applied)
        {
            ClearNativeEffect(hwnd);
            RestoreSurface(window, hwnd);
        }

        lock (stateGate)
        {
            IsApplied = applied;
        }
        ApplyDarkMode(hwnd, IsDarkMode);
    }

    private void PrepareTransparentSurface(Window window, IntPtr hwnd)
    {
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
    }

    private static bool ApplySystemBackdrop(IntPtr hwnd, int backdropType)
    {
        var frameMargins = DwmFrameMargins.ExtendAcrossClientArea;
        var frameExtended = DwmExtendFrameIntoClientArea(hwnd, ref frameMargins) == Succeeded;
        var backdropApplied =
            DwmSetWindowAttribute(
                hwnd,
                DwmwaSystemBackdropType,
                ref backdropType,
                Marshal.SizeOf<int>()
            ) == Succeeded;
        return frameExtended && backdropApplied;
    }

    private static bool ApplyLegacyMica(IntPtr hwnd)
    {
        var frameMargins = DwmFrameMargins.ExtendAcrossClientArea;
        var frameExtended = DwmExtendFrameIntoClientArea(hwnd, ref frameMargins) == Succeeded;
        var enabled = 1;
        var micaApplied =
            DwmSetWindowAttribute(
                hwnd,
                DwmwaLegacyMicaEffect,
                ref enabled,
                Marshal.SizeOf<int>()
            ) == Succeeded;
        return frameExtended && micaApplied;
    }

    private bool ApplyAccentAcrylic(IntPtr hwnd)
    {
        var frameMargins = DwmFrameMargins.None;
        var frameReset = DwmExtendFrameIntoClientArea(hwnd, ref frameMargins) == Succeeded;
        return frameReset
            && SetAccentPolicy(
                hwnd,
                AccentEnableAcrylicBlurBehind,
                IsDarkMode ? DarkAcrylicTint : LightAcrylicTint
            );
    }

    private void RemoveEffectCore(Window window)
    {
        var hwnd = new WindowInteropHelper(window).Handle;
        if (hwnd != IntPtr.Zero)
        {
            ClearNativeEffect(hwnd);
        }

        RestoreSurface(window, hwnd);

        lock (stateGate)
        {
            IsApplied = false;
        }
        ApplyDarkMode(hwnd, IsDarkMode);
    }

    private void ClearNativeEffect(IntPtr hwnd)
    {
        var frameMargins = DwmFrameMargins.None;
        DwmExtendFrameIntoClientArea(hwnd, ref frameMargins);

        if (platform.SupportsSystemBackdrop)
        {
            var backdropType = DwmsbtNone;
            DwmSetWindowAttribute(
                hwnd,
                DwmwaSystemBackdropType,
                ref backdropType,
                Marshal.SizeOf<int>()
            );
        }

        if (platform.SupportsLegacyMica)
        {
            var disabled = 0;
            DwmSetWindowAttribute(
                hwnd,
                DwmwaLegacyMicaEffect,
                ref disabled,
                Marshal.SizeOf<int>()
            );
        }

        if (platform.SupportsAccentAcrylic)
        {
            SetAccentPolicy(hwnd, AccentDisabled, 0);
        }
    }

    private void RestoreSurface(Window window, IntPtr hwnd)
    {
        if (originalBackgroundResourceKey is { } resourceKey)
        {
            // The shell background is a DynamicResource. Restoring the brush instance
            // captured before a material would pin the window to the theme that was active at
            // attach time, so restore the resource expression and resolve today's value.
            window.SetResourceReference(Window.BackgroundProperty, resourceKey);
        }
        else
        {
            window.Background = originalBackground;
        }
        if (
            hwnd != IntPtr.Zero
            && hasOriginalCompositionBackground
            && HwndSource.FromHwnd(hwnd) is { CompositionTarget: { } compositionTarget }
        )
        {
            compositionTarget.BackgroundColor = originalCompositionBackground;
        }
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

    private void ApplyDarkMode(IntPtr hwnd, bool isDarkMode)
    {
        if (hwnd == IntPtr.Zero || !platform.IsWindows11OrLater)
        {
            return;
        }

        var darkMode = isDarkMode ? 1 : 0;
        DwmSetWindowAttribute(hwnd, DwmwaUseImmersiveDarkMode, ref darkMode, Marshal.SizeOf<int>());
    }

    private void EnsureEffectSupported(MaterialEffect effect)
    {
        if (effect is MaterialEffect.Auto or MaterialEffect.None || IsSupported(effect))
        {
            return;
        }

        throw new PlatformNotSupportedException(
            $"Material effect '{effect}' requires {platform.DescribeRequirement(effect)}."
        );
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

    private static bool SetAccentPolicy(IntPtr hwnd, int state, int gradientColor)
    {
        var policy = new AccentPolicy
        {
            AccentState = state,
            GradientColor = gradientColor,
        };
        var policySize = Marshal.SizeOf<AccentPolicy>();
        var policyPointer = Marshal.AllocHGlobal(policySize);
        try
        {
            Marshal.StructureToPtr(policy, policyPointer, fDeleteOld: false);
            var data = new WindowCompositionAttributeData
            {
                Attribute = WcaAccentPolicy,
                Data = policyPointer,
                SizeOfData = policySize,
            };
            return SetWindowCompositionAttribute(hwnd, ref data);
        }
        finally
        {
            Marshal.FreeHGlobal(policyPointer);
        }
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

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool SetWindowCompositionAttribute(
        IntPtr hwnd,
        ref WindowCompositionAttributeData data
    );

    [StructLayout(LayoutKind.Sequential)]
    private struct AccentPolicy
    {
        public int AccentState;

        public int AccentFlags;

        public int GradientColor;

        public int AnimationId;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct WindowCompositionAttributeData
    {
        public int Attribute;

        public IntPtr Data;

        public int SizeOfData;
    }

    [StructLayout(LayoutKind.Sequential)]
    private readonly struct DwmFrameMargins
    {
        public static DwmFrameMargins ExtendAcrossClientArea => new(-1, -1, -1, -1);

        public static DwmFrameMargins None => new(0, 0, 0, 0);

        private readonly int leftWidth;

        private readonly int rightWidth;

        private readonly int topHeight;

        private readonly int bottomHeight;

        private DwmFrameMargins(int leftWidth, int rightWidth, int topHeight, int bottomHeight)
        {
            this.leftWidth = leftWidth;
            this.rightWidth = rightWidth;
            this.topHeight = topHeight;
            this.bottomHeight = bottomHeight;
        }
    }
}
