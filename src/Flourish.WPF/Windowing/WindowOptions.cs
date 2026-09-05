using System.Windows;

namespace ArkheideSystem.Flourish.Windowing;

internal sealed class WindowOptions
{
    public double WindowWidth { get; set; } = 1536;
    public double WindowHeight { get; set; } = 864;
    public double WindowMinWidth { get; set; } = 1280;
    public double WindowMinHeight { get; set; } = 720;
    public double WindowMaxWidth { get; set; } = double.PositiveInfinity;
    public double WindowMaxHeight { get; set; } = double.PositiveInfinity;
    public double? WindowLeft { get; set; }
    public double? WindowTop { get; set; }
    public WindowStartupLocation WindowStartupLocation { get; set; } = WindowStartupLocation.CenterScreen;
    public WindowState WindowState { get; set; } = WindowState.Normal;
    public ResizeMode WindowResizeMode { get; set; } = ResizeMode.CanResize;
    public bool WindowTopmost { get; set; }
    public bool WindowShowInTaskbar { get; set; } = true;
    public bool IsTrayExitEnabled { get; set; }
    public bool UsePersistedWindowSize { get; set; } = true;
    public bool UsePersistedWindowPosition { get; set; } = true;
    public bool UsePersistedWindowState { get; set; } = true;
    public bool UsePersistedWindowTopmost { get; set; } = true;
    public bool UsePersistedTrayExit { get; set; } = true;
}
