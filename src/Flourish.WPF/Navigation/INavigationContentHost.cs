using ArkheideSystem.Flourish.Abstract;
using System.Windows.Controls;

namespace ArkheideSystem.Flourish.Navigation;

internal interface INavigationContentHost
{
    bool Navigate(Page page);
}
