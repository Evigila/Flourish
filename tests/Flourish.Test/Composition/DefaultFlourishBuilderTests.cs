using System.Windows.Controls;
using ArkheideSystem.Flourish.Abstract;
using ArkheideSystem.Flourish.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace ArkheideSystem.Flourish.Test.Composition;

public sealed class DefaultFlourishBuilderTests
{
    [Fact]
    public void ConfigureMethods_WithNullCallbacks_ThrowArgumentNullExceptionImmediately()
    {
        var builder = FlourishBuilder.CreateDefaultBuilder([]);

        Assert.Throws<ArgumentNullException>(() => builder.ConfigureData(null!));
        Assert.Throws<ArgumentNullException>(() => builder.ConfigureServices(null!));
        Assert.Throws<ArgumentNullException>(() => builder.ConfigureShell(null!));
        Assert.Throws<ArgumentNullException>(() => builder.ConfigureTitleBar(null!));
        Assert.Throws<ArgumentNullException>(() => builder.ConfigureNavigation(null!));
        Assert.Throws<ArgumentNullException>(() => builder.ConfigureCustomHandler(null!));
        Assert.Throws<ArgumentNullException>(() => builder.ConfigureDynamicToolbar(null!));
        Assert.Throws<ArgumentNullException>(() => builder.ConfigureMotion(null!));
        Assert.Throws<ArgumentNullException>(() => builder.ConfigureWindow(null!));
        Assert.Throws<ArgumentNullException>(() => builder.ConfigureStatusBar(null!));
    }

    [Fact]
    public void Build_AppliesConfigurationCallbacksAndPreservesRegistrationOrder()
    {
        var marker = new object();
        var builder = FlourishBuilder
            .CreateDefaultBuilder([])
            .ConfigureData(data => data.SetAppPreferenceDataPath(@"C:\Flourish.Test"))
            .ConfigureServices((_, services) => services.AddSingleton(marker))
            .ConfigureShell(shell =>
                shell
                    .UseNavigation()
                    .UseTips(350)
                    .UseGlobalFont("Arial", 15)
                    .UseMaterialEffect(MaterialEffect.None)
                    .UseStatusBar()
            )
            .ConfigureShell(shell => shell.UseStatusBar(enabled: false))
            .ConfigureTitleBar(titlebar =>
                titlebar
                    .SetTitle("Test Shell")
                    .SetProfile(NameOrder.LastFirst)
                    .SetThemeToggle(FlourishTheme.Dark)
            )
            .ConfigureNavigation(navigation => navigation.SetTitle("Navigation"))
            .ConfigureCustomHandler(custom =>
                custom.Add(FlourishRegion.TitlebarStart, _ => null!)
            )
            .ConfigureDynamicToolbar(toolbar =>
                toolbar.CreateToolbarItems<TestPage>(
                    new FlourishToolbarItem("Refresh", "R", "test.refresh")
                )
            )
            .ConfigureMotion(motion => motion.RespectSystemReducedMotion(enabled: false))
            .ConfigureWindow(window => window.UseTopmost())
            .ConfigureStatusBar(statusBar => statusBar.SetStatusText("Ready"));

        using var flourish = builder.Build();
        var options = flourish.GetRequiredService<FlourishShellOptions>();
        var dataOptions = flourish.GetRequiredService<FlourishDataOptions>();

        Assert.Same(marker, flourish.GetRequiredService<object>());
        Assert.Equal(@"C:\Flourish.Test", dataOptions.AppPreferenceDataPath);
        Assert.True(options.IsNavigationPanelEnabled);
        Assert.False(options.IsStatusBarEnabled);
        Assert.Equal("Test Shell", options.Title);
        Assert.True(options.IsTitlebarTitleEnabled);
        Assert.True(options.IsProfileEnabled);
        Assert.True(options.IsTitlebarProfileEnabled);
        Assert.Equal(NameOrder.LastFirst, options.Profile.NameOrder);
        Assert.True(options.IsThemeEnabled);
        Assert.True(options.IsTitlebarThemeToggleEnabled);
        Assert.Equal("Navigation", options.PaneTitle);
        Assert.Single(options.RegionContents);
        Assert.Single(options.DynamicToolbarItems[typeof(TestPage)]);
        Assert.Equal(350, options.Tips.InitialShowDelayMilliseconds);
        Assert.False(options.Motion.RespectSystemReducedMotion);
        Assert.True(options.WindowTopmost);
        Assert.Equal("Arial", options.FontFamily);
        Assert.Equal(15, options.FontSize);
        Assert.Equal(MaterialEffect.None, options.MaterialEffect);
        Assert.False(options.IsMaterialEffectEnabled);
        Assert.Equal(FlourishTheme.Dark, options.DefaultTheme);
        Assert.Equal("Ready", options.StatusText);
    }

    private sealed class TestPage : Page { }
}
