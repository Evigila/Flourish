using Microsoft.AspNetCore.Components;

namespace ArkheideSystem.Flourish.Blazor.Components.Internal;

/// <summary>Shares the content structure of passive and interactive grid cells.</summary>
internal static class UniformGridContent
{
    internal static RenderFragment Create(string title, string icon, string text, RenderFragment? childContent, bool iconSupport = false) => builder =>
    {
        if (iconSupport)
        {
            builder.OpenElement(0, "span");
            builder.AddAttribute(1, "class", "f-uniform-cell-icon");
            if (!string.IsNullOrWhiteSpace(icon))
            {
                builder.OpenComponent<Icon>(2);
                builder.AddAttribute(3, nameof(Icon.Name), icon);
                builder.CloseComponent();
            }
            builder.CloseElement();
            builder.OpenElement(4, "span");
            builder.AddAttribute(5, "class", "f-uniform-cell-copy");
            builder.AddContent(6, Create(title, string.Empty, text, childContent));
            builder.CloseElement();
            return;
        }
        if (!string.IsNullOrWhiteSpace(title) || !string.IsNullOrWhiteSpace(icon))
        {
            builder.OpenElement(0, "span");
            builder.AddAttribute(1, "class", "f-uniform-cell-heading");
            if (!string.IsNullOrWhiteSpace(icon))
            {
                builder.OpenComponent<Icon>(2);
                builder.AddAttribute(3, nameof(Icon.Name), icon);
                builder.CloseComponent();
            }
            if (!string.IsNullOrWhiteSpace(title))
            {
                builder.OpenElement(4, "strong");
                builder.AddAttribute(5, "class", "f-uniform-cell-title");
                builder.AddContent(6, title);
                builder.CloseElement();
            }
            builder.CloseElement();
        }
        if (!string.IsNullOrWhiteSpace(text))
        {
            builder.OpenElement(7, "span");
            builder.AddAttribute(8, "class", "f-uniform-cell-text");
            builder.AddContent(9, text);
            builder.CloseElement();
        }
        builder.AddContent(10, childContent);
    };
}
