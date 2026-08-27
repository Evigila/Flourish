using ArkheideSystem.Flourish.Abstract;
namespace ArkheideSystem.Flourish.Navigation;

internal sealed record NavigationStackEntry(string NavigationKey, object? Parameter);
