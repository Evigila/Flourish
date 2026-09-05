using System;

using ArkheideSystem.Flourish.Abstract;
using System.Windows.Controls;

namespace ArkheideSystem.Flourish.Navigation;

internal interface INavigationPageProvider
{
    Page GetPage(Type sourcePageType);
}
