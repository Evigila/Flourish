namespace ArkheideSystem.Flourish.Internal.Navigation;

internal interface IPageFactory
{
    object? Create(Type sourcePageType);
}
