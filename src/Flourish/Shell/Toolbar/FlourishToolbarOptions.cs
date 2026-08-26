using System;
using System.Collections.Generic;

using ArkheideSystem.Flourish.Abstract;

namespace ArkheideSystem.Flourish.Shell.Toolbar;

internal sealed class FlourishToolbarOptions
{
    public bool IsDynamicToolbarEnabled { get; set; }
    public List<FlourishToolbarItem> ToolbarItems { get; } = [];
    public Dictionary<Type, IReadOnlyList<FlourishToolbarItem>> DynamicToolbarItems { get; } = [];
    public Dictionary<Type, bool> DynamicToolbarIconModes { get; } = [];
}
