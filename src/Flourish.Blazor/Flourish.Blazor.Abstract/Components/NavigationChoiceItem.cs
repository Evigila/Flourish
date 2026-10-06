namespace ArkheideSystem.Flourish.Blazor.Components;

/// <summary>A native same-site navigation choice and its host-owned content identity.</summary>
/// <param name="Key">A unique stable identifier, independent of the translated label.</param>
/// <param name="Label">The visible and accessible choice name.</param>
/// <param name="Href">The root-relative GET destination selected by the host.</param>
/// <param name="Disabled">Whether navigation to this choice is unavailable.</param>
public sealed record NavigationChoiceItem(string Key, string Label, string Href, bool Disabled = false);
