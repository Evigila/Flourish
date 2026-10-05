using System;
using System.Collections.Generic;
using System.Linq;

using ArkheideSystem.Flourish.Abstract;
using ArkheideSystem.Flourish.Extensions.Culture.WPF;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Moq;
using Xunit;

namespace ArkheideSystem.Tests.Flourish.Extensions.Culture.WPF;

public sealed class EssentialCultureBuilderExtensionsTests
{
    [Fact]
    public void Assembly_ExportsOnlyTheBuilderExtensionSurface()
    {
        var exportedTypes = typeof(EssentialCultureBuilderExtensions)
            .Assembly.GetExportedTypes()
            .OrderBy(type => type.FullName, StringComparer.Ordinal)
            .ToArray();

        Assert.Equal([typeof(EssentialCultureBuilderExtensions)], exportedTypes);
        var method = Assert.Single(
            typeof(EssentialCultureBuilderExtensions).GetMethods(
                System.Reflection.BindingFlags.Public
                    | System.Reflection.BindingFlags.Static
                    | System.Reflection.BindingFlags.DeclaredOnly
            )
        );
        Assert.Equal("UseEssentialCulture", method.Name);
        Assert.False(method.IsGenericMethod);
    }

    [Fact]
    public void UseEssentialCulture_RegistersOneIntegrationAndRemainsChainable()
    {
        var callbacks = new List<Action<HostBuilderContext, IServiceCollection>>();
        var builder = new Mock<IApplicationBuilder>();
        builder
            .Setup(value =>
                value.ConfigureServices(It.IsAny<Action<HostBuilderContext, IServiceCollection>>())
            )
            .Callback(
                (Action<HostBuilderContext, IServiceCollection> callback) => callbacks.Add(callback)
            )
            .Returns(builder.Object);

        var result = builder.Object.UseEssentialCulture().UseEssentialCulture();
        var services = new ServiceCollection();
        foreach (var callback in callbacks)
        {
            callback(null!, services);
        }

        Assert.Same(builder.Object, result);
        Assert.Equal(2, callbacks.Count);
        Assert.Single(
            services,
            descriptor => descriptor.ServiceType == typeof(ShellCultureApplicator)
        );
        Assert.Single(
            services,
            descriptor =>
                descriptor.ServiceType == typeof(IHostedService)
                && descriptor.ImplementationType == typeof(EssentialCultureHostedService)
        );
        Assert.DoesNotContain(
            services,
            descriptor => descriptor.ServiceType == typeof(ILocalizationService)
        );
    }

    [Fact]
    public void UseEssentialCulture_WhenBuilderIsNull_Throws()
    {
        IApplicationBuilder builder = null!;

        Assert.Throws<ArgumentNullException>(() => builder.UseEssentialCulture());
    }
}
