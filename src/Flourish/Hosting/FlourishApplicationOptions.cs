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
internal sealed class FlourishApplicationOptions
{
    internal FlourishAppearanceOptions Appearance { get; } = new();
    internal FlourishWindowOptions Window { get; } = new();
    internal FlourishLayoutOptions Layout { get; } = new();
    internal FlourishNavigationOptions Navigation { get; } = new();
    internal FlourishProjectOptions Projects { get; } = new();
    internal FlourishTitleBarOptions TitleBar { get; } = new();
    internal FlourishToolbarOptions Toolbar { get; } = new();
    internal FlourishStatusBarOptions StatusBar { get; } = new();
    internal FlourishRegionOptions Regions { get; } = new();
    internal FlourishMotionOptions Motion { get; } = new();
    internal FlourishTipOptions Tips { get; } = new();
    internal FlourishProfileOptions Profile { get; } = new();
}
