using System;
using System.Linq;

using ArkheideSystem.Flourish.Abstract;
using System.Windows.Controls;
using ArkheideSystem.Flourish.Configuration;

namespace ArkheideSystem.Flourish.Shell.Toolbar;

internal sealed class ToolbarBuilder(FlourishToolbarOptions options)
    : FlourishBuilderMutationGuard,
        IToolbarBuilder
{
    public IToolbarBuilder SetEnabled(bool enabled = true)
    {
        ThrowIfFrozen();
        options.IsDynamicToolbarEnabled = enabled;
        return this;
    }

    public IToolbarBuilder Set<TPage>(
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
