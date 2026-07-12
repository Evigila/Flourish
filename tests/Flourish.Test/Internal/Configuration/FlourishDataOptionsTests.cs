using ArkheideSystem.Flourish.Internal.Configuration;

namespace ArkheideSystem.Flourish.Test.Internal.Configuration;

public sealed class FlourishDataOptionsTests
{
    [Fact]
    public void Defaults_UseEnglishLocaleWithoutCustomFiles()
    {
        var options = new FlourishDataOptions();

        Assert.Equal("EN", options.Locale);
        Assert.Empty(options.LocalePaths);
    }
}
