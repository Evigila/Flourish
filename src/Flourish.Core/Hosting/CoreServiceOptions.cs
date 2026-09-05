using System;

using ArkheideSystem.Flourish.Configuration;
using ArkheideSystem.Flourish.Layout;
using ArkheideSystem.Flourish.Localization;
using ArkheideSystem.Flourish.Motion;
using ArkheideSystem.Flourish.Profile;
using ArkheideSystem.Flourish.Projects;
using ArkheideSystem.Flourish.Shell.StatusBar;
using ArkheideSystem.Flourish.Shell.TitleBar;

namespace ArkheideSystem.Flourish.Hosting;

/// <summary>
/// Carries the configured option instances across the platform composition boundary.
/// </summary>
internal sealed class CoreServiceOptions
{
    internal CoreServiceOptions(
        ApplicationDataOptions data,
        LocalizationService localization,
        LayoutOptions layout,
        ProjectOptions projects,
        TitleBarOptions titleBar,
        StatusBarOptions statusBar,
        MotionOptions motion,
        ProfileOptions profile
    )
    {
        Data = data ?? throw new ArgumentNullException(nameof(data));
        Localization =
            localization ?? throw new ArgumentNullException(nameof(localization));
        Layout = layout ?? throw new ArgumentNullException(nameof(layout));
        Projects = projects ?? throw new ArgumentNullException(nameof(projects));
        TitleBar = titleBar ?? throw new ArgumentNullException(nameof(titleBar));
        StatusBar = statusBar ?? throw new ArgumentNullException(nameof(statusBar));
        Motion = motion ?? throw new ArgumentNullException(nameof(motion));
        Profile = profile ?? throw new ArgumentNullException(nameof(profile));
    }

    internal ApplicationDataOptions Data { get; }

    internal LocalizationService Localization { get; }

    internal LayoutOptions Layout { get; }

    internal ProjectOptions Projects { get; }

    internal TitleBarOptions TitleBar { get; }

    internal StatusBarOptions StatusBar { get; }

    internal MotionOptions Motion { get; }

    internal ProfileOptions Profile { get; }
}
