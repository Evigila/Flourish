using System.Windows.Controls;
using ArkheideSystem.Flourish.Abstract;
using ArkheideSystem.Flourish.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ArkheideSystem.Flourish.Test.Abstract;

public sealed class FlourishServiceCollectionExtensionsTests
{
    [Fact]
    public void AddNavigable_RegistersTransientPageAndMetadata()
    {
        var services = new ServiceCollection();

        var result = services.AddNavigable<TestPage>(
            "Test page",
            "T",
            FlourishPageCacheMode.Disabled,
            "test"
        );

        Assert.Same(services, result);
        var pageDescriptor = Assert.Single(
            services,
            descriptor => descriptor.ServiceType == typeof(TestPage)
        );
        Assert.Equal(ServiceLifetime.Transient, pageDescriptor.Lifetime);
        Assert.Equal(typeof(TestPage), pageDescriptor.ImplementationType);

        var registration = Assert.Single(GetState(services).NavigablePages);
        Assert.Equal("test", registration.NavigationKey);
        Assert.Equal(typeof(TestPage), registration.PageType);
        Assert.Equal("Test page", registration.DisplayName);
        Assert.Equal("T", registration.IconGlyph);
        Assert.Equal(FlourishPageCacheMode.Disabled, registration.CacheMode);
    }

    [Fact]
    public void AddNavigable_WithoutNavigationKey_UsesPageFullName()
    {
        var services = new ServiceCollection();

        services.AddNavigable<TestPage>("Test page", "T");

        var registration = Assert.Single(GetState(services).NavigablePages);
        Assert.Equal(typeof(TestPage).FullName, registration.NavigationKey);
    }

    [Fact]
    public void AddNavigable_WithMultiplePages_ReusesRegistrationState()
    {
        var services = new ServiceCollection();

        services.AddNavigable<TestPage>("Test page", "T");
        services.AddNavigable<OtherPage>("Other page", "O");

        Assert.Equal(2, GetState(services).NavigablePages.Count);
        Assert.Single(
            services,
            descriptor => descriptor.ServiceType == typeof(FlourishServiceCollectionState)
        );
    }

    [Fact]
    public void AddNavigable_WithNonPageType_ThrowsArgumentException()
    {
        var services = new ServiceCollection();

        var exception = Assert.Throws<ArgumentException>(() =>
            services.AddNavigable(typeof(string), "Invalid", "I")
        );

        Assert.Equal("pageType", exception.ParamName);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void AddNavigable_WithMissingDisplayName_ThrowsArgumentException(string? displayName)
    {
        var services = new ServiceCollection();

        var exception = Assert.Throws<ArgumentException>(() =>
            services.AddNavigable(typeof(TestPage), displayName!, "T")
        );

        Assert.Equal("displayName", exception.ParamName);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void AddNavigable_WithBlankNavigationKey_ThrowsArgumentException(string navigationKey)
    {
        var services = new ServiceCollection();

        var exception = Assert.Throws<ArgumentException>(() =>
            services.AddNavigable(typeof(TestPage), "Test page", "T", navigationKey: navigationKey)
        );

        Assert.Equal("navigationKey", exception.ParamName);
    }

    private static FlourishServiceCollectionState GetState(ServiceCollection services)
    {
        var descriptor = Assert.Single(
            services,
            candidate => candidate.ServiceType == typeof(FlourishServiceCollectionState)
        );
        return Assert.IsType<FlourishServiceCollectionState>(descriptor.ImplementationInstance);
    }

    private sealed class TestPage : Page { }

    private sealed class OtherPage : Page { }
}
