using System.Windows.Controls;
using System.Windows.Media;
using ArkheideSystem.Flourish.Abstract;
using ArkheideSystem.Flourish.Internal.Configuration;
using ArkheideSystem.Flourish.Services;

namespace ArkheideSystem.Flourish.Test.Services;

public sealed class FontServicePageTests
{
    [Fact]
    public void ApplyToPage_GlobalResourcesBridgeTheFrameInheritanceBoundary()
    {
        RunInSta(() =>
        {
            var service = new FontService(new FlourishShellOptions());
            var text = new TextBlock();
            var page = new FontPage { Content = text };
            page.Resources["FlourishFontFamily"] = new FontFamily("Arial");
            page.Resources["FlourishFontSizeBase"] = 17d;

            service.ApplyToPage(page);

            Assert.Equal("Arial", page.FontFamily.Source);
            Assert.Equal("Arial", text.FontFamily.Source);
            Assert.Equal(17d, page.FontSize);
            Assert.Equal(17d, text.FontSize);
        });
    }

    [Fact]
    public void ApplyToPage_OverrideUpdatesInheritedAndResourceBasedTextButPreservesExplicitFont()
    {
        RunInSta(() =>
        {
            var service = new FontService(new FlourishShellOptions());
            var inheritedText = new TextBlock();
            var resourceText = new TextBlock();
            resourceText.SetResourceReference(
                TextBlock.FontFamilyProperty,
                "FlourishFontFamily"
            );
            var explicitText = new TextBlock { FontFamily = new FontFamily("Consolas") };
            var panel = new StackPanel();
            panel.Children.Add(inheritedText);
            panel.Children.Add(resourceText);
            panel.Children.Add(explicitText);
            var page = new FontPage { Content = panel };

            service.SetOverrideFont<FontPage>("Arial", 18);
            service.ApplyToPage(page);

            Assert.Equal("Arial", page.FontFamily.Source);
            Assert.Equal("Arial", inheritedText.FontFamily.Source);
            Assert.Equal("Arial", resourceText.FontFamily.Source);
            Assert.Equal("Consolas", explicitText.FontFamily.Source);
            Assert.Equal(18d, page.FontSize);
            Assert.Equal(18d, page.Resources["FlourishFontSizeBase"]);
            Assert.Equal(19d, page.Resources["FlourishFontSizeSubtitle"]);
            Assert.Equal(22d, page.Resources["FlourishFontSizeSectionTitle"]);
            Assert.Equal(35d, page.Resources["FlourishFontSizePageTitle"]);
        });
    }

    [Fact]
    public void ApplyToPage_OverrideWithoutSizeFollowsGlobalAndRestoresOriginalResources()
    {
        RunInSta(() =>
        {
            var service = new FontService(new FlourishShellOptions());
            var page = new FontPage();
            var originalFamily = new FontFamily("Times New Roman");
            page.Resources["FlourishFontFamily"] = originalFamily;
            page.Resources["FlourishFontSizeBase"] = 15d;
            page.Resources["FlourishFontSizeCaption"] = 14d;

            service.SetOverrideFont<FontPage>("Arial", 20);
            service.ApplyToPage(page);
            Assert.Equal(20d, page.Resources["FlourishFontSizeBase"]);
            Assert.True(page.Resources.Contains("FlourishFontSizeTitle"));

            service.SetOverrideFont<FontPage>("Arial");
            service.ApplyToPage(page);
            Assert.Equal(15d, page.FontSize);
            Assert.Equal(15d, page.Resources["FlourishFontSizeBase"]);
            Assert.Equal(14d, page.Resources["FlourishFontSizeCaption"]);
            Assert.False(page.Resources.Contains("FlourishFontSizeTitle"));

            Assert.True(service.ClearOverrideFont<FontPage>());
            service.ApplyToPage(page);
            Assert.Same(originalFamily, page.Resources["FlourishFontFamily"]);
            Assert.Equal("Times New Roman", page.FontFamily.Source);
            Assert.Equal(15d, page.FontSize);
        });
    }

    [Fact]
    public void ApplyToPage_UsesConfiguredTypeForFactoryReturnedDerivedPage()
    {
        RunInSta(() =>
        {
            var service = new FontService(new FlourishShellOptions());
            var page = new DerivedFontPage();

            service.SetOverrideFont<FontPage>("Arial", 19);
            service.ApplyToPage(page, typeof(FontPage));

            Assert.Equal("Arial", page.FontFamily.Source);
            Assert.Equal(19d, page.FontSize);
        });
    }

    private static void RunInSta(Action action)
    {
        Exception? error = null;
        var thread = new Thread(() =>
        {
            try
            {
                action();
            }
            catch (Exception exception)
            {
                error = exception;
            }
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();

        if (error is not null)
        {
            System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(error).Throw();
        }
    }

    private class FontPage : Page { }

    private sealed class DerivedFontPage : FontPage { }
}
