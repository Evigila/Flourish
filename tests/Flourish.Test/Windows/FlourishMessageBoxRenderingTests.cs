using System.IO;
using System.Xml.Linq;

namespace ArkheideSystem.Flourish.Test.Windows;

public sealed class FlourishMessageBoxRenderingTests
{
    private static readonly string MessageBoxXamlPath = Path.Combine(
        FindRepositoryRoot(),
        "src",
        "Flourish",
        "Views",
        "Windows",
        "FlourishMessageBoxWindow.xaml"
    );

    [Fact]
    public void Window_UsesOpaqueHwndAndNativeFrameShadow()
    {
        var document = XDocument.Load(MessageBoxXamlPath);
        var window = Assert.IsType<XElement>(document.Root);

        Assert.Equal("False", (string?)window.Attribute("AllowsTransparency"));
        Assert.NotEqual("Transparent", (string?)window.Attribute("Background"));
        Assert.Contains(
            window.Descendants(),
            element => element.Name.LocalName == "WindowChrome"
        );
        Assert.DoesNotContain(
            window.Descendants(),
            element => element.Attribute("Effect") is not null
        );
    }

    [Fact]
    public void Window_DoesNotReserveClientAreaForACompositedShadow()
    {
        var document = XDocument.Load(MessageBoxXamlPath);

        Assert.DoesNotContain(
            document.Descendants(),
            element => (string?)element.Attribute("Margin") == "30,28,30,46"
        );
        Assert.DoesNotContain(
            document.Descendants(),
            element => ((string?)element.Attribute("Effect"))?.Contains(
                "FlourishElevation",
                StringComparison.Ordinal
            ) == true
        );
    }

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            if (File.Exists(Path.Combine(directory.FullName, "Flourish.slnx")))
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException("Unable to locate the Flourish repository root.");
    }
}
