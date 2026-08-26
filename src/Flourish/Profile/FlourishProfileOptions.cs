using System;

using ArkheideSystem.Flourish.Abstract;
namespace ArkheideSystem.Flourish.Profile;

internal sealed class FlourishProfileOptions
{
    public bool IsProfileEnabled { get; set; }

    public bool UsePersistedNameOrder { get; set; } = true;

    public string DefaultFirstName { get; set; } = string.Empty;

    public string DefaultLastName { get; set; } = string.Empty;

    public NameOrder NameOrder { get; set; } = NameOrder.FirstLast;

    public string? DefaultImagePath { get; set; }

    public Type PageType { get; set; } = null!;
}
