using ArkheideSystem.Flourish.Controls;
using ArkheideSystem.Flourish.Abstract;

namespace ArkheideSystem.Flourish.Configuration;

internal sealed class FlourishProfileOptions
{
    public string DefaultFirstName { get; set; } = string.Empty;

    public string DefaultLastName { get; set; } = string.Empty;

    public NameOrder NameOrder { get; set; } = NameOrder.FirstLast;

    public string? DefaultImagePath { get; set; }

    public Type PageType { get; set; } = typeof(FlourishProfilePage);
}
