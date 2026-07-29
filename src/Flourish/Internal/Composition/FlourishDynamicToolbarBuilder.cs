using System.Windows.Controls;
using ArkheideSystem.Flourish.Abstract;
using ArkheideSystem.Flourish.Internal.Configuration;

namespace ArkheideSystem.Flourish.Internal.Composition;

internal sealed class FlourishDynamicToolbarBuilder(FlourishShellOptions options)
    : FlourishBuilderMutationGuard,
        IFlourishDynamicToolbarBuilder
{
    public IFlourishDynamicToolbarBuilder InitToolbarItems<TPage>(
        bool iconOnly,
        params FlourishToolbarItem[] items
    )
        where TPage : Page
    {
        ThrowIfFrozen();
        ArgumentNullException.ThrowIfNull(items);
        if (items.Any(item => item is null))
        {
            throw new ArgumentException("Toolbar items cannot contain null.", nameof(items));
        }

        options.DynamicToolbarItems[typeof(TPage)] = items.ToArray();
        options.DynamicToolbarIconModes[typeof(TPage)] = iconOnly;
        return this;
    }
}
