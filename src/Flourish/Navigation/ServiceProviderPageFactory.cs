using System;

using ArkheideSystem.Flourish.Abstract;
namespace ArkheideSystem.Flourish.Navigation;

internal sealed class ServiceProviderPageFactory(IServiceProvider serviceProvider) : IPageFactory
{
    private readonly IServiceProvider serviceProvider =
        serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));

    public object? Create(Type sourcePageType)
    {
        ArgumentNullException.ThrowIfNull(sourcePageType);

        return serviceProvider.GetService(sourcePageType) ?? Activator.CreateInstance(sourcePageType);
    }
}
