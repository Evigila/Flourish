using System.Windows.Controls;

namespace AcksheedSys.Flourish.Abstract;

public interface IFlourishDynamicToolbarBuilder
{
    IFlourishDynamicToolbarBuilder CreateToolbarItems(Type pageType, params FlourishToolbarItem[] items);

    IFlourishDynamicToolbarBuilder CreateToolbarItems(
        Type pageType,
        bool icon,
        params FlourishToolbarItem[] items
    );

    IFlourishDynamicToolbarBuilder CreateToolbarItems<TPage>(params FlourishToolbarItem[] items)
        where TPage : Page;

    IFlourishDynamicToolbarBuilder CreateToolbarItems<TPage>(
        bool icon,
        params FlourishToolbarItem[] items
    )
        where TPage : Page;
}
