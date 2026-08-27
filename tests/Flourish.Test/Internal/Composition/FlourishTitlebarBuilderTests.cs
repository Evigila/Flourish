using System;
using Xunit;
using ArkheideSystem.Flourish.Abstract;
using ArkheideSystem.Flourish.Appearance;
using ArkheideSystem.Flourish.Profile;
using ArkheideSystem.Flourish.Projects;
using ArkheideSystem.Flourish.Shell.TitleBar;

using System.Windows.Controls;

namespace ArkheideSystem.Flourish.Test.Internal.Composition;

public sealed class TitleBarBuilderTests
{
    [Fact]
    public void SetProfilePage_WithPageType_UpdatesOptionsAndReturnsBuilder()
    {
        var fixture = new BuilderFixture();
        var options = fixture.TitleBar;
        var sut = fixture.Builder;

        var result = sut.SetProfilePage<TestProfilePage>();

        Assert.Same(sut, result);
        Assert.Equal(typeof(TestProfilePage), fixture.Profile.PageType);
    }

    [Fact]
    public void Options_ExposeDefaultValuesButElementsRemainDisabledUntilDefaultsAreApplied()
    {
        var fixture = new BuilderFixture();
        var options = fixture.TitleBar;

        Assert.False(options.IsTitlebarSearchEnabled);
        Assert.False(options.IsBreadcrumbEnabled);
        Assert.False(options.IsTitlebarNavigationToggleEnabled);
        Assert.False(options.IsTitlebarLogoEnabled);
        Assert.False(options.IsTitlebarTitleEnabled);
        Assert.Equal("MyApp", options.ApplicationTitle);
        Assert.Equal("MyApp", options.ApplicationSubtitle);
        Assert.Equal("Unnamed project", fixture.Projects.UnnamedProjectPlaceholder);
        Assert.True(options.ShowApplicationTitleInLogoFlyout);
        Assert.True(options.ShowApplicationSubtitleInLogoFlyout);
        Assert.False(options.ShowProjectTitleInLogoFlyout);
        Assert.False(fixture.Profile.IsProfileEnabled);
        Assert.False(options.IsTitlebarProfileEnabled);
        Assert.False(fixture.Appearance.IsThemeEnabled);
        Assert.False(options.IsTitlebarThemeToggleEnabled);
    }

    [Fact]
    public void ConfigurationMethods_UpdateValuesEnableElementsAndReturnBuilder()
    {
        var fixture = new BuilderFixture();
        var options = fixture.TitleBar;
        var sut = fixture.Builder;

        Assert.Same(sut, sut.SetBreadcrumbMode(option: BreadcrumbShowOption.Always));
        Assert.Same(sut, sut.SetNavigationToggle());
        Assert.Same(
            sut,
            sut.SetLogo(
                logoPath: "Assets/logo.png",
                showApplicationTitle: false,
                showApplicationSubtitle: false,
                showProjectTitle: true
            )
        );
        Assert.Same(sut, sut.SetApplicationTitle("Foobar"));
        Assert.Same(sut, sut.SetApplicationSubtitle("Workspace"));
        Assert.Same(sut, sut.SetUnnamedProjectPlaceholder("Untitled workspace"));
        Assert.Same(sut, sut.SetProfile(nameOrder: NameOrder.LastFirst));
        Assert.Same(sut, sut.SetThemeToggle(mode: ApplicationTheme.Dark));

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
        Assert.Equal("Untitled workspace", fixture.Projects.UnnamedProjectPlaceholder);
        Assert.True(fixture.Profile.IsProfileEnabled);
        Assert.True(options.IsTitlebarProfileEnabled);
        Assert.Equal(NameOrder.LastFirst, fixture.Profile.NameOrder);
        Assert.True(fixture.Appearance.IsThemeEnabled);
        Assert.True(options.IsTitlebarThemeToggleEnabled);
        Assert.Equal(ApplicationTheme.Dark, fixture.Appearance.DefaultTheme);
    }

    [Fact]
    public void PreferenceAwareMethods_ControlThemeAndNameOrderPolicies()
    {
        var fixture = new BuilderFixture();
        var options = fixture.TitleBar;
        var sut = fixture.Builder;

        sut.SetProfile(true, NameOrder.LastFirst, true)
            .SetThemeToggle(true, ApplicationTheme.Dark, false);

        Assert.True(fixture.Profile.UsePersistedNameOrder);
        Assert.False(fixture.Appearance.UsePersistedTheme);
    }

    [Fact]
    public void SetSearch_ConfiguresSearchAndForwardsServicesAndText()
    {
        var fixture = new BuilderFixture();
        var options = fixture.TitleBar;
        var sut = fixture.Builder;
        IServiceProvider? receivedServices = null;
        string? receivedText = null;
        var serviceProvider = new EmptyServiceProvider();

        var result = sut.SetSearch(
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
        var fixture = new BuilderFixture();
        var options = fixture.TitleBar;
        var sut = fixture.Builder;

        var result = sut.SetLogo(
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
        var fixture = new BuilderFixture();
        var options = fixture.TitleBar;
        var sut = fixture.Builder;

        var result = sut.SetLogo();

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
        var fixture = new BuilderFixture();
        var options = fixture.TitleBar;
        var sut = fixture.Builder;
        IServiceProvider? receivedServices = null;
        string? receivedText = null;
        Action<IServiceProvider, string> handler = (services, text) =>
        {
            receivedServices = services;
            receivedText = text;
        };
        var serviceProvider = new EmptyServiceProvider();

        var result = sut.SetSearch(placeholder: "Search", handler: handler);
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
        var sut = CreateBuilder();

        Assert.Equal(
            "title",
            Assert.Throws<ArgumentException>(() => sut.SetApplicationTitle(value!)).ParamName
        );
        Assert.Equal(
            "subtitle",
            Assert.Throws<ArgumentException>(() => sut.SetApplicationSubtitle(value!)).ParamName
        );
        Assert.Equal(
            "placeholder",
            Assert
                .Throws<ArgumentException>(() => sut.SetUnnamedProjectPlaceholder(value!))
                .ParamName
        );
        Assert.Equal(
            "placeholder",
            Assert
                .Throws<ArgumentException>(() =>
                    sut.SetSearch(placeholder: value!, handler: (_, _) => { })
                )
                .ParamName
        );
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void SetLogo_WithBlankPath_ThrowsArgumentException(string value)
    {
        var sut = CreateBuilder();

        Assert.Equal(
            "logoPath",
            Assert.Throws<ArgumentException>(() => sut.SetLogo(logoPath: value)).ParamName
        );
    }

    [Fact]
    public void SetSearch_WithoutHandler_StillEnablesRuntimeSearchSurface()
    {
        var fixture = new BuilderFixture();
        var options = fixture.TitleBar;
        var sut = fixture.Builder;

        Assert.Same(sut, sut.SetSearch(placeholder: "Search"));
        Assert.True(options.IsTitlebarSearchEnabled);
        Assert.Null(options.TitlebarSearchTextChanged);
    }

    [Fact]
    public void EnumMethods_WithUndefinedValues_ThrowArgumentOutOfRangeException()
    {
        var sut = CreateBuilder();

        Assert.Equal(
            "option",
            Assert
                .Throws<ArgumentOutOfRangeException>(() =>
                    sut.SetBreadcrumbMode(option: (BreadcrumbShowOption)int.MaxValue)
                )
                .ParamName
        );
        Assert.Equal(
            "nameOrder",
            Assert
                .Throws<ArgumentOutOfRangeException>(() =>
                    sut.SetProfile(nameOrder: (NameOrder)int.MaxValue)
                )
                .ParamName
        );
        Assert.Equal(
            "mode",
            Assert
                .Throws<ArgumentOutOfRangeException>(() =>
                    sut.SetThemeToggle(mode: (ApplicationTheme)int.MaxValue)
                )
                .ParamName
        );
    }

    private static TitleBarBuilder CreateBuilder() =>
        new(
            new TitleBarOptions(),
            new ProjectOptions(),
            new AppearanceOptions(),
            new ProfileOptions()
        );

    private sealed class BuilderFixture
    {
        internal TitleBarOptions TitleBar { get; } = new();
        internal ProjectOptions Projects { get; } = new();
        internal AppearanceOptions Appearance { get; } = new();
        internal ProfileOptions Profile { get; } = new();
        internal TitleBarBuilder Builder { get; }

        internal BuilderFixture()
        {
            Builder = new TitleBarBuilder(TitleBar, Projects, Appearance, Profile);
        }
    }

    private sealed class EmptyServiceProvider : IServiceProvider
    {
        public object? GetService(Type serviceType) => null;
    }

    private sealed class TestProfilePage : Page { }
}
