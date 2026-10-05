namespace ArkheideSystem.Flourish.Blazor.Components.Primitives;

public sealed record IdentityFact(
    string Label,
    string Value,
    bool IsPrimary = false,
    bool IsIdentifier = false,
    string? ValueId = null);
