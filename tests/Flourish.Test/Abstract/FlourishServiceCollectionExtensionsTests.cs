using System.Windows.Controls;
using ArkheideSystem.Flourish.Abstract;
using ArkheideSystem.Flourish.Internal.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ArkheideSystem.Flourish.Test.Abstract;

public sealed class FlourishServiceCollectionExtensionsTests
{
    [Fact]
    public void AddCommandParser_RegistersEachParserTypeOnce()
    {
        var services = new ServiceCollection();

        var result = services
            .AddCommandParser<FirstCommandParser>()
            .AddCommandParser<FirstCommandParser>()
            .AddCommandParser<SecondCommandParser>();

        Assert.Same(services, result);
        using var serviceProvider = services.BuildServiceProvider();
        var parsers = serviceProvider
            .GetServices<ICommandParser>()
            .ToArray();
        Assert.Collection(
            parsers,
            parser => Assert.IsType<FirstCommandParser>(parser),
            parser => Assert.IsType<SecondCommandParser>(parser)
        );
    }

    [Fact]
    public void AddNavigable_RegistersTransientPageAndMetadata()
    {
        var services = new ServiceCollection();

        var result = services.AddNavigable<TestPage>(
            "Test page",
            "T",
            FlourishPageCacheMode.Disabled
        );

        Assert.Same(services, result);
        var pageDescriptor = Assert.Single(
            services,
            descriptor => descriptor.ServiceType == typeof(TestPage)
        );
        Assert.Equal(ServiceLifetime.Transient, pageDescriptor.Lifetime);
        Assert.Equal(typeof(TestPage), pageDescriptor.ImplementationType);

        var registration = Assert.Single(GetState(services).NavigablePages);
        Assert.Equal("Test", registration.NavigationKey);
        Assert.Equal(typeof(TestPage), registration.PageType);
        Assert.Equal("Test page", registration.DisplayName);
        Assert.Equal("T", registration.IconGlyph);
        Assert.Equal(FlourishPageCacheMode.Disabled, registration.CacheMode);
    }

    [Fact]
    public void AddNavigable_WithoutPageSuffix_UsesCompleteClassName()
    {
        var services = new ServiceCollection();

        services.AddNavigable<Dashboard>("Dashboard", "D");

        var registration = Assert.Single(GetState(services).NavigablePages);
        Assert.Equal("Dashboard", registration.NavigationKey);
    }

    [Fact]
    public void AddNavigable_WithoutNavigationKey_UsesClassNameWithoutPageSuffix()
    {
        var services = new ServiceCollection();

        services.AddNavigable<TestPage>("Test page", "T");

        var registration = Assert.Single(GetState(services).NavigablePages);
        Assert.Equal("Test", registration.NavigationKey);
    }

    [Fact]
    public void AddNavigable_WithClassNamedPage_DoesNotGenerateEmptyKey()
    {
        var services = new ServiceCollection();

        services.AddNavigable<Pages.Page>("Page", "P");

        var registration = Assert.Single(GetState(services).NavigablePages);
        Assert.Equal("Page", registration.NavigationKey);
    }

    [Fact]
    public void AddNavigable_WithDifferentSuffixCasing_DoesNotRemoveSuffix()
    {
        var services = new ServiceCollection();

        services.AddNavigable<Homepage>("Homepage", "H");

        var registration = Assert.Single(GetState(services).NavigablePages);
        Assert.Equal("Homepage", registration.NavigationKey);
    }

    [Fact]
    public void AddNavigable_WithRepeatedPageSuffix_RemovesOnlyTrailingSuffix()
    {
        var services = new ServiceCollection();

        services.AddNavigable<ReportPagePage>("Report", "R");

        var registration = Assert.Single(GetState(services).NavigablePages);
        Assert.Equal("ReportPage", registration.NavigationKey);
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

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void AddNavigable_WithMissingDisplayName_ThrowsArgumentException(string? displayName)
    {
        var services = new ServiceCollection();

        var exception = Assert.Throws<ArgumentException>(() =>
            services.AddNavigable<TestPage>(displayName!, "T")
        );

        Assert.Equal("displayName", exception.ParamName);
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

    private sealed class Dashboard : Page { }

    private sealed class Homepage : Page { }

    private sealed class ReportPagePage : Page { }

    private sealed class FirstCommandParser : ICommandParser
    {
        public void RegisterCommands(ICommandRegistrar commands) { }
    }

    private sealed class SecondCommandParser : ICommandParser
    {
        public void RegisterCommands(ICommandRegistrar commands) { }
    }

    private static class Pages
    {
        internal sealed class Page : System.Windows.Controls.Page { }
    }
}
