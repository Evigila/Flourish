using System.Collections.Generic;

using ArkheideSystem.Flourish.Abstract;

namespace ArkheideSystem.Flourish.Shell.StatusBar;

internal sealed class StatusBarOptions
{
    public bool IsStatusBarEnabled { get; set; }

    public bool IsLANConnectionStatusEnabled { get; set; }

    public bool IsPowerStatusEnabled { get; set; }

    public List<StatusBarItem> StatusItems { get; } = [];
}
