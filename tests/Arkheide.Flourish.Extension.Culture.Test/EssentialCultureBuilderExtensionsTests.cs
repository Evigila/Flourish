namespace Arkheide.Flourish.Extension.Culture.Test;

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
        var builder = new Mock<IFlourishBuilder>();
        builder
            .Setup(value =>
                value.ConfigServices(It.IsAny<Action<HostBuilderContext, IServiceCollection>>())
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
            descriptor => descriptor.ServiceType == typeof(FlourishShellCultureApplicator)
        );
        Assert.Single(
            services,
            descriptor =>
                descriptor.ServiceType == typeof(IHostedService)
                && descriptor.ImplementationType == typeof(EssentialCultureHostedService)
        );
        Assert.DoesNotContain(
            services,
            descriptor => descriptor.ServiceType == typeof(IFlourishLocalization)
        );
    }

    [Fact]
    public void UseEssentialCulture_WhenBuilderIsNull_Throws()
    {
        IFlourishBuilder builder = null!;

        Assert.Throws<ArgumentNullException>(() => builder.UseEssentialCulture());
    }
}
