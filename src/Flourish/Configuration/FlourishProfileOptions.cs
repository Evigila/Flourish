using ArkheideSystem.Flourish.Controls;

namespace ArkheideSystem.Flourish.Configuration;

internal sealed class FlourishProfileOptions
{
    public string DefaultUserName { get; set; } = "User";

    public string? DefaultImagePath { get; set; }

    public Type PageType { get; set; } = typeof(FlourishProfilePage);
}
