using ArkheideSystem.Flourish.Configuration;
using ArkheideSystem.Flourish.Composition;

namespace ArkheideSystem.Flourish.Test.Composition;

public sealed class FlourishDataBuilderTests
{
    [Fact]
    public void ConfigurationMethods_WithValidValues_UpdateOptionsAndReturnBuilder()
    {
        var options = new FlourishDataOptions();
        var sut = new FlourishDataBuilder(options);

        Assert.Same(sut, sut.SetLocale(" EN "));
        Assert.Same(sut, sut.AddLocale(" Locales/lang_EN.json "));

        Assert.Equal("EN", options.Locale);
        Assert.Equal(["Locales/lang_EN.json"], options.LocalePaths);
    }

    [Theory]
    [InlineData("locale", null)]
    [InlineData("locale", "")]
    [InlineData("locale", "   ")]
    [InlineData("localePath", null)]
    [InlineData("localePath", "")]
    [InlineData("localePath", "   ")]
    public void ConfigurationMethods_WithBlankValue_ThrowArgumentException(
        string parameterName,
        string? value
    )
    {
        var options = new FlourishDataOptions();
        var sut = new FlourishDataBuilder(options);

        var exception = Assert.Throws<ArgumentException>(() =>
        {
            switch (parameterName)
            {
                case "locale":
                    sut.SetLocale(value!);
                    break;
                case "localePath":
                    sut.AddLocale(value!);
                    break;
            }
        });

        Assert.Equal(
            parameterName == "localePath" ? "path" : parameterName,
            exception.ParamName
        );
    }
}
