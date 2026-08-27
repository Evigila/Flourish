using ArkheideSystem.Flourish.Appearance;
using ArkheideSystem.Flourish.Layout;
using ArkheideSystem.Flourish.Motion;
using ArkheideSystem.Flourish.Navigation;
using ArkheideSystem.Flourish.Profile;
using ArkheideSystem.Flourish.Projects;
using ArkheideSystem.Flourish.Shell.Regions;
using ArkheideSystem.Flourish.Shell.StatusBar;
using ArkheideSystem.Flourish.Shell.TitleBar;
using ArkheideSystem.Flourish.Shell.Toolbar;
using ArkheideSystem.Flourish.ToolTips;
using ArkheideSystem.Flourish.Windowing;

namespace ArkheideSystem.Flourish.Hosting;

/// <summary>
/// Owns the build-time option objects that are later registered independently with DI.
/// </summary>
internal sealed class ApplicationOptions
{
    internal AppearanceOptions Appearance { get; } = new();
    internal WindowOptions Window { get; } = new();
    internal LayoutOptions Layout { get; } = new();
    internal NavigationOptions Navigation { get; } = new();
    internal ProjectOptions Projects { get; } = new();
    internal TitleBarOptions TitleBar { get; } = new();
    internal ToolbarOptions Toolbar { get; } = new();
    internal StatusBarOptions StatusBar { get; } = new();
    internal ShellRegionOptions Regions { get; } = new();
    internal MotionOptions Motion { get; } = new();
    internal ToolTipOptions Tips { get; } = new();
    internal ProfileOptions Profile { get; } = new();
}
