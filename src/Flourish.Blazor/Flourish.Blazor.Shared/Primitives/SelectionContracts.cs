namespace ArkheideSystem.Flourish.Blazor.Components.Primitives;

public sealed record ServiceLink(string Label, string Href);
public sealed record ReferenceItem<TValue>(TValue Id, string Name) where TValue : struct;
public sealed record ChoiceItem(string Value, string Text);