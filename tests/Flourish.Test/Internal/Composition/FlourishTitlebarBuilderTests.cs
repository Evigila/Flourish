using System.Windows.Controls;
using ArkheideSystem.Flourish.Abstract;
using ArkheideSystem.Flourish.Internal.Configuration;
using ArkheideSystem.Flourish.Internal.Composition;

namespace ArkheideSystem.Flourish.Test.Internal.Composition;

public sealed class FlourishTitlebarBuilderTests
{
    [Fact]
    public void InitProfilePage_WithPageType_UpdatesOptionsAndReturnsBuilder()
    {
        var options = new FlourishShellOptions();
        var sut = new FlourishTitlebarBuilder(options);

        var result = sut.InitProfilePage<TestProfilePage>();

        Assert.Same(sut, result);
        Assert.Equal(typeof(TestProfilePage), options.Profile.PageType);
    }

    [Fact]
    public void Options_ExposeDefaultValuesButElementsRemainDisabledUntilDefaultsAreApplied()
    {
        var options = new FlourishShellOptions();

        Assert.False(options.IsTitlebarSearchEnabled);
        Assert.False(options.IsBreadcrumbEnabled);
        Assert.False(options.IsTitlebarNavigationToggleEnabled);
        Assert.False(options.IsTitlebarLogoEnabled);
        Assert.False(options.IsTitlebarTitleEnabled);
        Assert.Equal("MyApp", options.ApplicationTitle);
        Assert.Equal("MyApp", options.ApplicationSubtitle);
        Assert.Equal("Unnamed project", options.UnnamedProjectPlaceholder);
        Assert.True(options.ShowApplicationTitleInLogoFlyout);
        Assert.True(options.ShowApplicationSubtitleInLogoFlyout);
        Assert.False(options.ShowProjectTitleInLogoFlyout);
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

        Assert.Same(sut, sut.UseBreadcrumb(option: BreadcrumbShowOption.Always));
        Assert.Same(sut, sut.UseNavigationToggle());
        Assert.Same(
            sut,
            sut.UseLogo(
                logoPath: "Assets/logo.png",
                showApplicationTitle: false,
                showApplicationSubTitle: false,
                showProjectTitle: true
            )
        );
        Assert.Same(sut, sut.InitApplicationTitle("Foobar"));
        Assert.Same(sut, sut.InitApplicationSubTitle("Workspace"));
        Assert.Same(sut, sut.InitUnnamedProjectPlaceholder("Untitled workspace"));
        Assert.Same(sut, sut.UseProfile(nameOrder: NameOrder.LastFirst));
        Assert.Same(sut, sut.UseThemeToggle(mode: FlourishTheme.Dark));

        Assert.True(options.IsBreadcrumbEnabled);
        Assert.Equal(BreadcrumbShowOption.Always, options.BreadcrumbShowOption);
        Assert.True(options.IsTitlebarNavigationToggleEnabled);
        Assert.True(options.IsTitlebarLogoEnabled);
        Assert.Equal("Assets/logo.png", options.LogoPath);
        Assert.False(options.ShowApplicationTitleInLogoFlyout);
        Assert.False(options.ShowApplicationSubtitleInLogoFlyout);
        Assert.True(options.ShowProjectTitleInLogoFlyout);
        Assert.True(options.IsTitlebarTitleEnabled);
        Assert.Equal("Foobar", options.ApplicationTitle);
        Assert.Equal("Workspace", options.ApplicationSubtitle);
        Assert.Equal("Untitled workspace", options.UnnamedProjectPlaceholder);
        Assert.True(options.IsProfileEnabled);
        Assert.True(options.IsTitlebarProfileEnabled);
        Assert.Equal(NameOrder.LastFirst, options.Profile.NameOrder);
        Assert.True(options.IsThemeEnabled);
        Assert.True(options.IsTitlebarThemeToggleEnabled);
        Assert.Equal(FlourishTheme.Dark, options.DefaultTheme);
    }

    [Fact]
    public void PreferenceAwareMethods_ControlThemeAndNameOrderPolicies()
    {
        var options = new FlourishShellOptions();
        var sut = new FlourishTitlebarBuilder(options);

        sut.UseProfile(true, NameOrder.LastFirst, true)
            .UseThemeToggle(true, FlourishTheme.Dark, false);

        Assert.True(options.UsePersistedNameOrder);
        Assert.False(options.UsePersistedTheme);
    }

    [Fact]
    public void SetSearch_ConfiguresSearchAndForwardsServicesAndText()
    {
        var options = new FlourishShellOptions();
        var sut = new FlourishTitlebarBuilder(options);
        IServiceProvider? receivedServices = null;
        string? receivedText = null;
        var serviceProvider = new EmptyServiceProvider();

        var result = sut.UseSearch(
            placeholder: "Search pages",
            handler: (services, text) =>
            {
                receivedServices = services;
                receivedText = text;
            }
        );
        options.TitlebarSearchTextChanged!(serviceProvider, "flourish");

        Assert.Same(sut, result);
        Assert.True(options.IsTitlebarSearchEnabled);
        Assert.Equal("Search pages", options.SearchPlaceholder);
        Assert.Same(serviceProvider, receivedServices);
        Assert.Equal("flourish", receivedText);
    }

    [Fact]
    public void SetLogo_WithAbsolutePackUri_ConfiguresLogo()
    {
        var options = new FlourishShellOptions();
        var sut = new FlourishTitlebarBuilder(options);

        var result = sut.UseLogo(
            logoPath: "pack://application:,,,/Flourish;component/Assets/favicon.ico"
        );

        Assert.Same(sut, result);
        Assert.True(options.IsTitlebarLogoEnabled);
        Assert.Equal(
            "pack://application:,,,/Flourish;component/Assets/favicon.ico",
            options.LogoPath
        );
    }

    [Fact]
    public void SetLogo_WithoutPath_EnablesBuiltInLogo()
    {
        var options = new FlourishShellOptions();
        var sut = new FlourishTitlebarBuilder(options);

        var result = sut.UseLogo();

        Assert.Same(sut, result);
        Assert.True(options.IsTitlebarLogoEnabled);
        Assert.Null(options.LogoPath);
        Assert.True(options.ShowApplicationTitleInLogoFlyout);
        Assert.True(options.ShowApplicationSubtitleInLogoFlyout);
        Assert.False(options.ShowProjectTitleInLogoFlyout);
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

        var result = sut.UseSearch(placeholder: "Search", handler: handler);
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
            Assert
                .Throws<ArgumentException>(() => sut.InitApplicationTitle(value!))
                .ParamName
        );
        Assert.Equal(
            "subTitle",
            Assert
                .Throws<ArgumentException>(() => sut.InitApplicationSubTitle(value!))
                .ParamName
        );
        Assert.Equal(
            "placeholder",
            Assert
                .Throws<ArgumentException>(() =>
                    sut.InitUnnamedProjectPlaceholder(value!)
                )
                .ParamName
        );
        Assert.Equal(
            "placeholder",
            Assert
                .Throws<ArgumentException>(() =>
                    sut.UseSearch(placeholder: value!, handler: (_, _) => { })
                )
                .ParamName
        );
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void SetLogo_WithBlankPath_ThrowsArgumentException(string value)
    {
        var sut = new FlourishTitlebarBuilder(new FlourishShellOptions());

        Assert.Equal(
            "logoPath",
            Assert
                .Throws<ArgumentException>(() => sut.UseLogo(logoPath: value))
                .ParamName
        );
    }

    [Fact]
    public void UseSearch_WithoutHandler_StillEnablesRuntimeSearchSurface()
    {
        var options = new FlourishShellOptions();
        var sut = new FlourishTitlebarBuilder(options);

        Assert.Same(sut, sut.UseSearch(placeholder: "Search"));
        Assert.True(options.IsTitlebarSearchEnabled);
        Assert.Null(options.TitlebarSearchTextChanged);
    }

    [Fact]
    public void EnumMethods_WithUndefinedValues_ThrowArgumentOutOfRangeException()
    {
        var sut = new FlourishTitlebarBuilder(new FlourishShellOptions());

        Assert.Equal(
            "option",
            Assert
                .Throws<ArgumentOutOfRangeException>(() =>
                    sut.UseBreadcrumb(option: (BreadcrumbShowOption)int.MaxValue)
                )
                .ParamName
        );
        Assert.Equal(
            "nameOrder",
            Assert
                .Throws<ArgumentOutOfRangeException>(() =>
                    sut.UseProfile(nameOrder: (NameOrder)int.MaxValue)
                )
                .ParamName
        );
        Assert.Equal(
            "mode",
            Assert
                .Throws<ArgumentOutOfRangeException>(() =>
                    sut.UseThemeToggle(mode: (FlourishTheme)int.MaxValue)
                )
                .ParamName
        );
    }

    private sealed class EmptyServiceProvider : IServiceProvider
    {
        public object? GetService(Type serviceType) => null;
    }

    private sealed class TestProfilePage : Page { }
}
