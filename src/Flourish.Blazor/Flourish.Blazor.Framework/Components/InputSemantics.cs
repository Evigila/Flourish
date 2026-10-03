using Microsoft.AspNetCore.Components.Forms;

namespace ArkheideSystem.Flourish.Blazor.Components;

internal sealed record FieldContext(string Id, bool Required, bool HasErrors)
{
    public string? Description(IReadOnlyDictionary<string, object>? attributes)
    {
        var existing = attributes is not null && attributes.TryGetValue("aria-describedby", out var value) ? value?.ToString() : null;
        return HasErrors ? string.IsNullOrWhiteSpace(existing) ? Id + "-error" : existing + " " + Id + "-error" : existing;
    }
}

internal static class InputSemantics
{
    internal static string? Invalid(EditContext? context, FieldIdentifier identifier, FieldContext? field, IReadOnlyDictionary<string, object>? attributes)
        => field?.HasErrors == true || context?.GetValidationMessages(identifier).Any() == true ? "true"
            : attributes is not null && attributes.TryGetValue("aria-invalid", out var value) ? value?.ToString() : null;

    internal static bool Required(FieldContext? field, IReadOnlyDictionary<string, object>? attributes)
        => field?.Required == true || attributes is not null && attributes.TryGetValue("required", out var value)
            && value is not null && !string.Equals(value.ToString(), "false", StringComparison.OrdinalIgnoreCase);

    internal static string? Description(FieldContext? field, IReadOnlyDictionary<string, object>? attributes)
        => field?.Description(attributes)
            ?? (attributes is not null && attributes.TryGetValue("aria-describedby", out var value) ? value?.ToString() : null);
}
