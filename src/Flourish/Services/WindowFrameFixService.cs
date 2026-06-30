using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using AcksheedSys.Flourish.Abstract;

namespace AcksheedSys.Flourish.Services;

internal sealed class WindowFrameFixService : IWindowFrameFixService
{
    private const int WmGetMinMaxInfo = 0x0024;

    private HwndSource? hwndSource;

    public void Attach(Window window)
    {
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

    private void AttachHook(Window window)
    {
        var hwnd = new WindowInteropHelper(window).Handle;
        if (hwnd == IntPtr.Zero)
        {
            return;
        }

        hwndSource = HwndSource.FromHwnd(hwnd);
        hwndSource?.AddHook(WindowProc);
        window.Closed += (_, _) => hwndSource?.RemoveHook(WindowProc);
    }

    private IntPtr WindowProc(
        IntPtr hwnd,
        int message,
        IntPtr wParam,
        IntPtr lParam,
        ref bool handled
    )
    {
        if (message != WmGetMinMaxInfo)
        {
            return IntPtr.Zero;
        }

        WmGetMinMaxInfoCore(hwnd, lParam);
        handled = true;
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

    private enum MonitorOptions : uint
    {
        MonitorDefaultToNearest = 0x00000002,
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
