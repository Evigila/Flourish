using ArkheideSystem.Flourish.Abstract;
using ArkheideSystem.Flourish.Configuration;
using ArkheideSystem.Flourish.Composition;

namespace ArkheideSystem.Flourish.Test.Composition;

public sealed class FlourishTitlebarBuilderTests
{
    [Fact]
    public void Elements_AreDisabledUntilConfigured()
    {
        var options = new FlourishShellOptions();

        Assert.False(options.IsTitlebarSearchEnabled);
        Assert.False(options.IsBreadcrumbEnabled);
        Assert.False(options.IsTitlebarNavigationToggleEnabled);
        Assert.False(options.IsTitlebarLogoEnabled);
        Assert.False(options.IsTitlebarTitleEnabled);
        Assert.False(options.IsTitlebarSubtitleEnabled);
        Assert.False(options.IsProfileEnabled);
        Assert.False(options.IsTitlebarProfileEnabled);
        Assert.False(options.IsThemeEnabled);
        Assert.False(options.IsTitlebarThemeToggleEnabled);
    }

    [Fact]
    public void ConfigurationMethods_UpdateValuesEnableElementsAndReturnBuilder()
    {
        var options = new FlourishShellOptions();
        var sut = new FlourishTitlebarBuilder(options);

        Assert.Same(sut, sut.SetBreadcrumbButton(BreadcrumbShowOption.Always));
        Assert.Same(sut, sut.SetNavToggle());
        Assert.Same(sut, sut.SetLogo("Assets/logo.png"));
        Assert.Same(sut, sut.SetTitle("Foobar"));
        Assert.Same(sut, sut.SetSubTitle("Workspace"));
        Assert.Same(sut, sut.SetProfile(NameOrder.LastFirst));
        Assert.Same(sut, sut.SetThemeToggle(FlourishTheme.Dark));

        Assert.True(options.IsBreadcrumbEnabled);
        Assert.Equal(BreadcrumbShowOption.Always, options.BreadcrumbShowOption);
        Assert.True(options.IsTitlebarNavigationToggleEnabled);
        Assert.True(options.IsTitlebarLogoEnabled);
        Assert.Equal("Assets/logo.png", options.LogoPath);
        Assert.True(options.IsTitlebarTitleEnabled);
        Assert.Equal("Foobar", options.Title);
        Assert.True(options.IsTitlebarSubtitleEnabled);
        Assert.Equal("Workspace", options.Subtitle);
        Assert.True(options.IsProfileEnabled);
        Assert.True(options.IsTitlebarProfileEnabled);
        Assert.Equal(NameOrder.LastFirst, options.Profile.NameOrder);
        Assert.True(options.IsThemeEnabled);
        Assert.True(options.IsTitlebarThemeToggleEnabled);
        Assert.Equal(FlourishTheme.Dark, options.DefaultTheme);
    }

    [Fact]
    public void SetSearch_WithTextCallback_ConfiguresSearchAndForwardsText()
    {
        var options = new FlourishShellOptions();
        var sut = new FlourishTitlebarBuilder(options);
        string? receivedText = null;

        var result = sut.SetSearch("Search pages", text => receivedText = text);
        options.TitlebarSearchTextChanged!(new EmptyServiceProvider(), "flourish");

        Assert.Same(sut, result);
        Assert.True(options.IsTitlebarSearchEnabled);
        Assert.Equal("Search pages", options.SearchPlaceholder);
        Assert.Equal("flourish", receivedText);
    }

    [Fact]
    public void SetLogo_WithAbsolutePackUri_ConfiguresLogo()
    {
        var options = new FlourishShellOptions();
        var sut = new FlourishTitlebarBuilder(options);

        var result = sut.SetLogo(
            "pack://application:,,,/Flourish;component/Assets/favicon.ico"
        );

        Assert.Same(sut, result);
        Assert.True(options.IsTitlebarLogoEnabled);
        Assert.Equal(
            "pack://application:,,,/Flourish;component/Assets/favicon.ico",
            options.LogoPath
        );
    }

    [Fact]
    public void SetSearch_WithServiceCallback_PreservesCallback()
    {
        var options = new FlourishShellOptions();
        var sut = new FlourishTitlebarBuilder(options);
        IServiceProvider? receivedServices = null;
        string? receivedText = null;
        Action<IServiceProvider, string> handler = (services, text) =>
        {
            receivedServices = services;
            receivedText = text;
        };
        var serviceProvider = new EmptyServiceProvider();

        var result = sut.SetSearch("Search", handler);
        options.TitlebarSearchTextChanged!(serviceProvider, "query");

        Assert.Same(sut, result);
        Assert.True(options.IsTitlebarSearchEnabled);
        Assert.Same(handler, options.TitlebarSearchTextChanged);
        Assert.Same(serviceProvider, receivedServices);
        Assert.Equal("query", receivedText);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void TextMethods_WithBlankValue_ThrowArgumentException(string? value)
    {
        var sut = new FlourishTitlebarBuilder(new FlourishShellOptions());

        Assert.Equal(
            "title",
            Assert.Throws<ArgumentException>(() => sut.SetTitle(value!)).ParamName
        );
        Assert.Equal(
            "subTitle",
            Assert.Throws<ArgumentException>(() => sut.SetSubTitle(value!)).ParamName
        );
        Assert.Equal(
            "logoPath",
            Assert.Throws<ArgumentException>(() => sut.SetLogo(value!)).ParamName
        );
        Assert.Equal(
            "placeholder",
            Assert
                .Throws<ArgumentException>(() => sut.SetSearch(value!, _ => { }))
                .ParamName
        );
    }

    [Fact]
    public void SetSearch_WithNullHandlers_ThrowsArgumentNullException()
    {
        var sut = new FlourishTitlebarBuilder(new FlourishShellOptions());

        Assert.Equal(
            "handler",
            Assert
                .Throws<ArgumentNullException>(() =>
                    sut.SetSearch("Search", (Action<string>)null!)
                )
                .ParamName
        );
        Assert.Equal(
            "handler",
            Assert
                .Throws<ArgumentNullException>(() =>
                    sut.SetSearch(
                        "Search",
                        (Action<IServiceProvider, string>)null!
                    )
                )
                .ParamName
        );
    }

    [Fact]
    public void EnumMethods_WithUndefinedValues_ThrowArgumentOutOfRangeException()
    {
        var sut = new FlourishTitlebarBuilder(new FlourishShellOptions());

        Assert.Equal(
            "option",
            Assert
                .Throws<ArgumentOutOfRangeException>(() =>
                    sut.SetBreadcrumbButton((BreadcrumbShowOption)int.MaxValue)
                )
                .ParamName
        );
        Assert.Equal(
            "nameOrder",
            Assert
                .Throws<ArgumentOutOfRangeException>(() =>
                    sut.SetProfile((NameOrder)int.MaxValue)
                )
                .ParamName
        );
        Assert.Equal(
            "mode",
            Assert
                .Throws<ArgumentOutOfRangeException>(() =>
                    sut.SetThemeToggle((FlourishTheme)int.MaxValue)
                )
                .ParamName
        );
    }

    private sealed class EmptyServiceProvider : IServiceProvider
    {
        public object? GetService(Type serviceType) => null;
    }
}
