using System;
using System.Collections.Generic;

using ArkheideSystem.Flourish.Abstract;

namespace ArkheideSystem.Flourish.Shell.Toolbar;

internal sealed class ToolbarOptions
{
    public bool IsDynamicToolbarEnabled { get; set; }
    public List<ToolbarItem> ToolbarItems { get; } = [];
    public Dictionary<Type, IReadOnlyList<ToolbarItem>> DynamicToolbarItems { get; } = [];
    public Dictionary<Type, bool> DynamicToolbarIconModes { get; } = [];
}
