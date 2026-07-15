using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace ArkheideSystem.Flourish.Services;

internal sealed class WindowFrameFixService
{
    private const int DwmwaWindowCornerPreference = 33;
    private const int DwmwcpRound = 2;
    private const int WmGetMinMaxInfo = 0x0024;
    private const int WmSetRedraw = 0x000B;

    private Window? attachedWindow;
    private HwndSource? hwndSource;
    private bool isSourceInitializationPending;
    private bool usesCustomFrame = true;

    internal bool IsAttachedTo(Window window) => ReferenceEquals(attachedWindow, window);

    public void Attach(Window window, bool useCustomFrame = true)
    {
        ArgumentNullException.ThrowIfNull(window);
        usesCustomFrame = useCustomFrame;
        if (
            ReferenceEquals(attachedWindow, window)
            && (hwndSource is not null || isSourceInitializationPending)
        )
        {
            return;
        }

        Detach();
        attachedWindow = window;
        window.Closed += Window_Closed;
        if (new WindowInteropHelper(window).Handle != IntPtr.Zero)
        {
            AttachHook(window);
            return;
        }

        isSourceInitializationPending = true;
        window.SourceInitialized += Window_SourceInitialized;
    }

    internal void ApplyFrameTransition(
        Window window,
        bool useCustomFrame,
        Action transition
    )
    {
        ArgumentNullException.ThrowIfNull(window);
        ArgumentNullException.ThrowIfNull(transition);

        if (!ReferenceEquals(attachedWindow, window))
        {
            Attach(window, useCustomFrame);
        }

        usesCustomFrame = useCustomFrame;
        var hwnd = new WindowInteropHelper(window).Handle;
        var suspendRedraw = hwnd != IntPtr.Zero && IsWindowVisible(hwnd);
        if (suspendRedraw)
        {
            SendMessage(hwnd, WmSetRedraw, IntPtr.Zero, IntPtr.Zero);
        }

        try
        {
            transition();
        }
        finally
        {
            if (hwnd != IntPtr.Zero)
            {
                RefreshFrameCore(hwnd);
                if (suspendRedraw)
                {
                    SendMessage(hwnd, WmSetRedraw, new IntPtr(1), IntPtr.Zero);
                    RedrawWindow(
                        hwnd,
                        IntPtr.Zero,
                        IntPtr.Zero,
                        RedrawWindowFlags.Invalidate
                            | RedrawWindowFlags.Erase
                            | RedrawWindowFlags.AllChildren
                            | RedrawWindowFlags.UpdateNow
                            | RedrawWindowFlags.Frame
                    );
                }
            }
        }
    }

    internal void RefreshFrame(Window window, bool useCustomFrame)
    {
        ArgumentNullException.ThrowIfNull(window);
        if (!ReferenceEquals(attachedWindow, window))
        {
            Attach(window, useCustomFrame);
        }

        usesCustomFrame = useCustomFrame;
        var hwnd = new WindowInteropHelper(window).Handle;
        if (hwnd != IntPtr.Zero)
        {
            RefreshFrameCore(hwnd);
        }
    }

    private void Window_SourceInitialized(object? sender, EventArgs e)
    {
        if (sender is not Window window || !ReferenceEquals(attachedWindow, window))
        {
            return;
        }

        window.SourceInitialized -= Window_SourceInitialized;
        isSourceInitializationPending = false;
        AttachHook(window);
    }

    private void Window_Closed(object? sender, EventArgs e)
    {
        if (ReferenceEquals(sender, attachedWindow))
        {
            Detach();
        }
    }

    private void Detach()
    {
        if (attachedWindow is not null)
        {
            if (isSourceInitializationPending)
            {
                attachedWindow.SourceInitialized -= Window_SourceInitialized;
            }

            attachedWindow.Closed -= Window_Closed;
        }

        hwndSource?.RemoveHook(WindowProc);
        hwndSource = null;
        attachedWindow = null;
        isSourceInitializationPending = false;
    }

    private void AttachHook(Window window)
    {
        var hwnd = new WindowInteropHelper(window).Handle;
        if (hwnd == IntPtr.Zero)
        {
            return;
        }

        var source = HwndSource.FromHwnd(hwnd);
        if (source is null)
        {
            return;
        }

        hwndSource = source;
        source.AddHook(WindowProc);
        ApplyRoundedCornerPreference(hwnd);
    }

    private static void ApplyRoundedCornerPreference(IntPtr hwnd)
    {
        if (!OperatingSystem.IsWindowsVersionAtLeast(10, 0, 22000))
        {
            return;
        }

        var cornerPreference = DwmwcpRound;
        DwmSetWindowAttribute(
            hwnd,
            DwmwaWindowCornerPreference,
            ref cornerPreference,
            Marshal.SizeOf<int>()
        );
    }

    private static void RefreshFrameCore(IntPtr hwnd)
    {
        ApplyRoundedCornerPreference(hwnd);
        SetWindowPos(
            hwnd,
            IntPtr.Zero,
            0,
            0,
            0,
            0,
            SetWindowPosFlags.NoSize
                | SetWindowPosFlags.NoMove
                | SetWindowPosFlags.NoZOrder
                | SetWindowPosFlags.NoActivate
                | SetWindowPosFlags.FrameChanged
        );
    }

    private IntPtr WindowProc(
        IntPtr hwnd,
        int message,
        IntPtr wParam,
        IntPtr lParam,
        ref bool handled
    )
    {
        if (message != WmGetMinMaxInfo || !usesCustomFrame)
        {
            return IntPtr.Zero;
        }

        WmGetMinMaxInfoCore(hwnd, lParam);

        // Keep the message available to WPF after correcting the custom frame's
        // maximized bounds. Window's native handler adds the DPI-aware MinWidth,
        // MinHeight, MaxWidth, and MaxHeight track constraints to the same struct.
        handled = false;
        return IntPtr.Zero;
    }

    private static void WmGetMinMaxInfoCore(IntPtr hwnd, IntPtr lParam)
    {
        var monitor = MonitorFromWindow(hwnd, MonitorOptions.MonitorDefaultToNearest);
        if (monitor == IntPtr.Zero)
        {
            return;
        }

        var monitorInfo = new MonitorInfo { Size = Marshal.SizeOf<MonitorInfo>() };
        if (!GetMonitorInfo(monitor, ref monitorInfo))
        {
            return;
        }

        var minMaxInfo = Marshal.PtrToStructure<MinMaxInfo>(lParam);
        var workArea = monitorInfo.WorkArea;
        var monitorArea = monitorInfo.MonitorArea;

        minMaxInfo.MaxPosition.X = Math.Abs(workArea.Left - monitorArea.Left);
        minMaxInfo.MaxPosition.Y = Math.Abs(workArea.Top - monitorArea.Top);
        minMaxInfo.MaxSize.X = Math.Abs(workArea.Right - workArea.Left);
        minMaxInfo.MaxSize.Y = Math.Abs(workArea.Bottom - workArea.Top);

        Marshal.StructureToPtr(minMaxInfo, lParam, true);
    }

    [DllImport("user32.dll", SetLastError = true)]
    private static extern IntPtr MonitorFromWindow(IntPtr hwnd, MonitorOptions flags);

    [DllImport("user32.dll", EntryPoint = "GetMonitorInfoW", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetMonitorInfo(IntPtr monitor, ref MonitorInfo monitorInfo);

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool IsWindowVisible(IntPtr hwnd);

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool RedrawWindow(
        IntPtr hwnd,
        IntPtr updateRectangle,
        IntPtr updateRegion,
        RedrawWindowFlags flags
    );

    [DllImport("user32.dll", SetLastError = true)]
    private static extern IntPtr SendMessage(
        IntPtr hwnd,
        int message,
        IntPtr wParam,
        IntPtr lParam
    );

    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool SetWindowPos(
        IntPtr hwnd,
        IntPtr insertAfter,
        int x,
        int y,
        int width,
        int height,
        SetWindowPosFlags flags
    );

    [DllImport("dwmapi.dll", PreserveSig = true)]
    private static extern int DwmSetWindowAttribute(
        IntPtr hwnd,
        int dwAttribute,
        ref int pvAttribute,
        int cbAttribute
    );

    private enum MonitorOptions : uint
    {
        MonitorDefaultToNearest = 0x00000002,
    }

    [Flags]
    private enum RedrawWindowFlags : uint
    {
        Invalidate = 0x0001,
        Erase = 0x0004,
        AllChildren = 0x0080,
        UpdateNow = 0x0100,
        Frame = 0x0400,
    }

    [Flags]
    private enum SetWindowPosFlags : uint
    {
        NoSize = 0x0001,
        NoMove = 0x0002,
        NoZOrder = 0x0004,
        NoActivate = 0x0010,
        FrameChanged = 0x0020,
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct Point
    {
        public int X;

        public int Y;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct Rect
    {
        public int Left;

        public int Top;

        public int Right;

        public int Bottom;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct MonitorInfo
    {
        public int Size;

        public Rect MonitorArea;

        public Rect WorkArea;

        public int Flags;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct MinMaxInfo
    {
        public Point Reserved;

        public Point MaxSize;

        public Point MaxPosition;

        public Point MinTrackSize;

        public Point MaxTrackSize;
    }
}
