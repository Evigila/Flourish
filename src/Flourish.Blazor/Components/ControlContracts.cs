using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.AspNetCore.Components.Forms;
using System.Linq;

namespace ArkheideSystem.Flourish.Blazor.Components;

/// <summary>The semantic appearance of an action.</summary>
public enum ButtonVariant { Primary, Secondary, Danger, Quiet }

/// <summary>The geometry of a modal surface.</summary>
public enum DialogPresentation { Centered, BottomSheet }

/// <summary>An action supplied by the host; the component never owns business behavior.</summary>
public sealed record MenuAction(string Text, Func<Task> OnClick, bool Disabled = false, bool Destructive = false);

/// <summary>A typed native select option.</summary>
public sealed record SelectOption<TValue>(TValue Value, string Text, bool Disabled = false);

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
