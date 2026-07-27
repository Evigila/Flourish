using System.Windows.Controls;

namespace ArkheideSystem.Flourish.Internal.Navigation;

internal interface INavigationPageProvider
{
    Page GetPage(Type sourcePageType);
}
