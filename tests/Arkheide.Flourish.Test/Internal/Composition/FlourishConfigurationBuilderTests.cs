using System.IO;
using ArkheideSystem.Flourish.Internal.Composition;
using Microsoft.Extensions.Configuration.Json;
using Microsoft.Extensions.Configuration.Memory;

namespace ArkheideSystem.Flourish.Test.Internal.Composition;

public sealed class FlourishConfigurationBuilderTests
{
    [Fact]
    public void UseConfigurationFile_RegistersResolvedJsonSource()
    {
        var sut = new FlourishConfigurationBuilder();

        var result = sut.UseConfigurationFile(
            "Data/appsettings.User.json",
            optional: false,
            reloadOnChange: false
        );

        Assert.Same(sut, result);
        var source = Assert.IsType<JsonConfigurationSource>(Assert.Single(sut.Sources));
        Assert.Equal(
            Path.GetFullPath("Data/appsettings.User.json", AppContext.BaseDirectory),
            source.FileProvider!.GetFileInfo(source.Path!).PhysicalPath
        );
        Assert.False(source.Optional);
        Assert.False(source.ReloadOnChange);
    }

    [Fact]
    public void RegistrationMethods_RejectInvalidArguments()
    {
        var sut = new FlourishConfigurationBuilder();
        using var directory = new TemporaryDirectory();

        Assert.Throws<ArgumentException>(() => sut.UseConfigurationFile(" "));
        Assert.Throws<ArgumentException>(() => sut.UseConfigurationFile("appsettings.User.txt"));
        Assert.Throws<ArgumentException>(() => sut.UseConfigurationFile(directory.Path));
        Assert.Throws<ArgumentNullException>(() => sut.AddConfigurationSource(null!));
    }

    [Fact]
    public void AddConfigurationSource_PreservesTheStandardSourceInstance()
    {
        var sut = new FlourishConfigurationBuilder();
        var source = new MemoryConfigurationSource();

        var result = sut.AddConfigurationSource(source);

        Assert.Same(sut, result);
        Assert.Same(source, Assert.Single(sut.Sources));
    }
}
