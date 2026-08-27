using System.Collections.Generic;

using ArkheideSystem.Flourish.Abstract;
namespace ArkheideSystem.Flourish.Navigation;

internal sealed class NavigationGroupDefinition(int groupId, string? title)
{
    public int GroupId { get; } = groupId;

    public string? Title { get; } = title;

    public List<NavigationItemDefinition> Items { get; } = [];
}
