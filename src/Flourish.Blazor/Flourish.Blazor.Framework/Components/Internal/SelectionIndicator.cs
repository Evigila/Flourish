using Microsoft.AspNetCore.Components;

namespace ArkheideSystem.Flourish.Blazor.Components.Internal;

/// <summary>Compact embedded selection marker; its owning option retains selection and label semantics.</summary>
internal static class SelectionIndicator
{
    internal static RenderFragment Render(bool selected = false) => builder =>
    {
        builder.OpenElement(0, "span");
        builder.AddAttribute(1, "class", "f-selection-indicator");
        builder.AddAttribute(2, "aria-hidden", "true");
        builder.AddAttribute(3, "data-selected", selected ? "true" : null);
        builder.AddContent(4, "✓");
        builder.CloseElement();
    };

    internal static RenderFragment Input(bool selected, bool disabled, string key, bool fixedSelection) => builder =>
    {
        builder.OpenElement(0, "input");
        builder.AddAttribute(1, "class", "f-selection-indicator");
        builder.AddAttribute(2, "type", "checkbox");
        builder.AddAttribute(3, "checked", selected);
        builder.AddAttribute(4, "disabled", disabled);
        builder.AddAttribute(5, "data-f-selection-key", key);
        builder.AddAttribute(6, "data-f-fixed", fixedSelection ? "true" : null);
        builder.CloseElement();
    };
}
