using System;

using ArkheideSystem.Flourish.Abstract;
namespace ArkheideSystem.Flourish.Navigation;

internal interface IPageFactory
{
    object? Create(Type sourcePageType);
}
