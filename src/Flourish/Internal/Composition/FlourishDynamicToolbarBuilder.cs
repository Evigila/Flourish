using System.Windows.Controls;
using ArkheideSystem.Flourish.Abstract;
using ArkheideSystem.Flourish.Internal.Configuration;

namespace ArkheideSystem.Flourish.Internal.Composition;

internal sealed class FlourishDynamicToolbarBuilder(FlourishShellOptions options)
    : IFlourishDynamicToolbarBuilder
{
    public IFlourishDynamicToolbarBuilder CreateToolbarItems<TPage>(
        params FlourishToolbarItem[] items
    )
        where TPage : Page
    {
        return CreateToolbarItems<TPage>(true, items);
    }

    public IFlourishDynamicToolbarBuilder CreateToolbarItems<TPage>(
        bool icon,
        params FlourishToolbarItem[] items
    )
        where TPage : Page
    {
        ArgumentNullException.ThrowIfNull(items);
        if (items.Any(item => item is null))
        {
            throw new ArgumentException("Toolbar items cannot contain null.", nameof(items));
        }

        options.DynamicToolbarItems[typeof(TPage)] = items.ToArray();
        options.DynamicToolbarIconModes[typeof(TPage)] = icon;
        return this;
    }
}
