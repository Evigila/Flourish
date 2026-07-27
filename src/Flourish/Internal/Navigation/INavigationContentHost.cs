using System.Windows.Controls;

namespace ArkheideSystem.Flourish.Internal.Navigation;

internal interface INavigationContentHost
{
    bool Navigate(Page page);
}
