using System.Collections.Generic;

using ArkheideSystem.Flourish.Abstract;

namespace ArkheideSystem.Flourish.Shell.StatusBar;

internal sealed class FlourishStatusBarOptions
{
    public bool IsStatusBarEnabled { get; set; }
    public bool IsLANConnectionStatusEnabled { get; set; }
    public bool IsPowerStatusEnabled { get; set; }
    public List<FlourishStatusItem> StatusItems { get; } = [];
}
