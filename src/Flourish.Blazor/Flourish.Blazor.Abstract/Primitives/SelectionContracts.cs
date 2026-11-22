namespace ArkheideSystem.Flourish.Blazor.Components.Primitives;

public sealed record ServiceLink(string Label, string Href)
{
    /// <summary>Optional launcher icon. Text menu entries continue to use only the label.</summary>
    public string? Icon { get; init; }
}
public sealed record ReferenceItem<TValue>(TValue Id, string Name) where TValue : struct;
public sealed record ChoiceItem(string Value, string Text);
