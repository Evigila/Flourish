using ArkheideSystem.Flourish.Abstract;
namespace ArkheideSystem.Flourish.Navigation;

internal sealed record FlourishPageStackEntry(string NavigationKey, object? Parameter);
