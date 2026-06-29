using AcksheedSys.Flourish.Abstract;
using AcksheedSys.Flourish.Models;

namespace AcksheedSys.Flourish.Services;

internal sealed class FlourishToolbarService(FlourishShellOptions options) : IFlourishToolbarService
{
    public IReadOnlyList<FlourishToolbarItem> GetToolbarItems(Type? pageType = null)
    {
        if (
            options.IsDynamicToolbarEnabled
            && pageType is not null
            && options.DynamicToolbarItems.TryGetValue(pageType, out var dynamicItems)
        )
        {
            return dynamicItems;
        }

        return options.ToolbarItems;
    }
}
