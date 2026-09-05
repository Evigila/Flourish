using System;
using System.Collections.Generic;

using ArkheideSystem.Flourish.Abstract;

namespace ArkheideSystem.Flourish.Shell.Toolbar;

internal sealed class ToolbarStateOptions
{
    public bool IsEnabled { get; set; }

    public List<ToolbarItem> DefaultItems { get; } = [];

    public Dictionary<string, IReadOnlyList<ToolbarItem>> ViewItems { get; } = new(
        StringComparer.Ordinal
    );

    public Dictionary<string, bool> ViewIconModes { get; } = new(StringComparer.Ordinal);
}
