using ArkheideSystem.Flourish.Services;
using Moq;

namespace ArkheideSystem.Flourish.Test.Services;

public sealed class ServiceProviderPageFactoryTests
{
    [Fact]
    public void Create_WhenServiceIsRegistered_ReturnsServiceProviderInstance()
    {
        var instance = new Constructible();
        var serviceProvider = new Mock<IServiceProvider>(MockBehavior.Strict);
        serviceProvider
            .Setup(provider => provider.GetService(typeof(Constructible)))
            .Returns(instance);
        var sut = new ServiceProviderPageFactory(serviceProvider.Object);

        var result = sut.Create(typeof(Constructible));

        Assert.Same(instance, result);
    }

    [Fact]
    public void Create_WhenServiceIsMissing_UsesActivator()
    {
        var serviceProvider = new Mock<IServiceProvider>(MockBehavior.Strict);
        serviceProvider
            .Setup(provider => provider.GetService(typeof(Constructible)))
            .Returns((object?)null);
        var sut = new ServiceProviderPageFactory(serviceProvider.Object);

        var result = sut.Create(typeof(Constructible));

        Assert.IsType<Constructible>(result);
    }

    [Fact]
    public void Create_WhenActivatorFails_PropagatesException()
    {
        var serviceProvider = new Mock<IServiceProvider>(MockBehavior.Strict);
        serviceProvider
            .Setup(provider => provider.GetService(typeof(NotConstructible)))
            .Returns((object?)null);
        var sut = new ServiceProviderPageFactory(serviceProvider.Object);

        Assert.Throws<MissingMethodException>(() => sut.Create(typeof(NotConstructible)));
    }

    private sealed class Constructible
    {
        public Constructible() { }
    }

    private sealed class NotConstructible(string value)
    {
        public string Value { get; } = value;
    }
}
