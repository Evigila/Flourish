using System.Collections.Generic;

using ArkheideSystem.Flourish.Abstract;

namespace ArkheideSystem.Flourish.Shell.Regions;

internal sealed class ShellRegionOptions
{
    public List<ShellRegionRegistration> RegionContents { get; } = [];
}
