using System.Windows.Controls;

namespace ArkheideSystem.Flourish.Abstract.Builder;

/// <summary>Provides convenience operations for configuring dynamic toolbar items.</summary>
public static class FlourishDynamicToolbarBuilderExtensions
{
    /// <summary>Creates icon-only toolbar items for the specified page type.</summary>
    /// <typeparam name="TPage">The page type associated with the toolbar items.</typeparam>
    /// <param name="builder">The dynamic toolbar builder.</param>
    /// <param name="items">The toolbar items displayed for the page.</param>
    /// <returns>The current builder for chained configuration.</returns>
    public static IFlourishDynamicToolbarBuilder InitToolbarItems<TPage>(
        this IFlourishDynamicToolbarBuilder builder,
        params FlourishToolbarItem[] items
    )
        where TPage : Page
    {
        ArgumentNullException.ThrowIfNull(builder);
        return builder.InitToolbarItems<TPage>(iconOnly: true, items);
    }
}
