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

        Assert.Same(sut, sut.SetAppPreferenceDataPath("C:\\Flourish Data"));
        Assert.Same(sut, sut.SetAppName("Gallery"));
        Assert.Same(sut, sut.SetAppCompany("Arkheide"));
        Assert.Same(sut, sut.SetLocale(" EN "));
        Assert.Same(sut, sut.AddLocale(" Locales/lang_EN.json "));

        Assert.Equal("C:\\Flourish Data", options.AppPreferenceDataPath);
        Assert.Equal("Gallery", options.AppName);
        Assert.Equal("Arkheide", options.CompanyName);
        Assert.Equal("EN", options.Locale);
        Assert.Equal(["Locales/lang_EN.json"], options.LocalePaths);
        Assert.True(options.HasConfiguration);
    }

    [Fact]
    public void LocaleMethods_DoNotEnablePreferenceDataConfiguration()
    {
        var options = new FlourishDataOptions();
        var sut = new FlourishDataBuilder(options);

        sut.SetLocale("EN").AddLocale("Locales/lang_CN.json");

        Assert.Equal("EN", options.Locale);
        Assert.Equal(["Locales/lang_CN.json"], options.LocalePaths);
        Assert.False(options.HasConfiguration);
    }

    [Theory]
    [InlineData("path", null)]
    [InlineData("path", "")]
    [InlineData("path", "   ")]
    [InlineData("appName", null)]
    [InlineData("appName", "")]
    [InlineData("appName", "   ")]
    [InlineData("companyName", null)]
    [InlineData("companyName", "")]
    [InlineData("companyName", "   ")]
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
                case "path":
                    sut.SetAppPreferenceDataPath(value!);
                    break;
                case "appName":
                    sut.SetAppName(value!);
                    break;
                case "companyName":
                    sut.SetAppCompany(value!);
                    break;
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
        Assert.False(options.HasConfiguration);
    }
}
