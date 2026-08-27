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
        BuilderValidation.PositiveFinite(contentWidth, nameof(contentWidth));
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
}
