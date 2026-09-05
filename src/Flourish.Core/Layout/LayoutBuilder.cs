using System;

using ArkheideSystem.Flourish.Abstract;
using ArkheideSystem.Flourish.Configuration;

namespace ArkheideSystem.Flourish.Layout;

internal sealed class LayoutBuilder(LayoutOptions options)
    : BuilderMutationGuard,
        ILayoutBuilder
{
    public ILayoutBuilder SetCenterContent(
        bool enabled = true,
        double contentWidth = 1200,
        bool usePersistedPreference = true
    )
    {
        ThrowIfFrozen();
        ValidateContentWidth(contentWidth);
        options.IsCenterContentEnabled = enabled;
        options.CenterContentWidth = contentWidth;
        options.UsePersistedContentLayout = usePersistedPreference;
        return this;
    }

    public ILayoutBuilder SetSmoothScrollingEnabled(
        bool enabled = true,
        bool usePersistedPreference = true
    )
    {
        ThrowIfFrozen();
        options.IsSmoothScrollingEnabled = enabled;
        options.UsePersistedSmoothScroll = usePersistedPreference;
        return this;
    }

    private static void ValidateContentWidth(double contentWidth)
    {
        if (!double.IsFinite(contentWidth) || contentWidth <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(contentWidth),
                contentWidth,
                "Value must be greater than 0."
            );
        }
    }
}
