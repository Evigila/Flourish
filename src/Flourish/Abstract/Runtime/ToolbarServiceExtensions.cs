using System.Windows.Controls;

namespace ArkheideSystem.Flourish.Abstract.Runtime;

/// <summary>Provides typed conveniences for runtime toolbar configuration.</summary>
public static class ToolbarServiceExtensions
{
    /// <summary>Sets the toolbar definition for a page type.</summary>
    public static void Set<TPage>(
        this IToolbarService service,
        IEnumerable<FlourishToolbarItem> items,
        bool iconOnly = true
    )
        where TPage : Page
    {
        ArgumentNullException.ThrowIfNull(service);
        service.Set(typeof(TPage), items, iconOnly);
    }
}
