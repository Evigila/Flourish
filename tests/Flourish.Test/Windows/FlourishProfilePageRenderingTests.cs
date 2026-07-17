using System.IO;
using System.Xml.Linq;

namespace ArkheideSystem.Flourish.Test.Windows;

public sealed class FlourishProfilePageRenderingTests
{
    private const string XamlNamespace =
        "http://schemas.microsoft.com/winfx/2006/xaml";
    private const string FlourishControlsNamespace =
        "clr-namespace:ArkheideSystem.Flourish.Controls";
    private static readonly string ProfileXamlPath = Path.Combine(
        FindRepositoryRoot(),
        "src",
        "Flourish",
        "Views",
        "Page",
        "ProfilePage.xaml"
    );
    private static readonly string ProfileCodePath = Path.ChangeExtension(
        ProfileXamlPath,
        ".xaml.cs"
    );

    [Fact]
    public void UploadImageButton_UsesSharedButtonWithoutASelectionPreview()
    {
        var document = XDocument.Load(ProfileXamlPath);
        var uploadButton = FindNamedElement(document, "UploadImageButton");

        Assert.Equal("Button", uploadButton.Name.LocalName);
        Assert.Equal(FlourishControlsNamespace, uploadButton.Name.NamespaceName);
        Assert.Equal("Outlined", (string?)uploadButton.Attribute("Variant"));
        Assert.DoesNotContain(
            document.Descendants(),
            element =>
                (string?)element.Attribute(XName.Get("Name", XamlNamespace))
                is "SelectedImageContent"
                    or "SelectedImagePreview"
                    or "ImageSelectedText"
        );
    }

    [Fact]
    public void UploadImageHandler_KeepsFileSelectionWithoutPreviewButtonState()
    {
        var source = File.ReadAllText(ProfileCodePath);

        Assert.Contains("new OpenFileDialog", source, StringComparison.Ordinal);
        Assert.Contains("dialog.ShowDialog(Window.GetWindow(this))", source);
        Assert.Contains("selectedImagePath = dialog.FileName;", source);
        Assert.DoesNotContain("UpdateSelectedImageButton", source);
    }

    private static XElement FindNamedElement(XDocument document, string name)
    {
        return document
            .Descendants()
            .Single(element =>
                (string?)element.Attribute(XName.Get("Name", XamlNamespace)) == name
            );
    }

    private static string FindRepositoryRoot()
    {
        for (
            var directory = new DirectoryInfo(AppContext.BaseDirectory);
            directory is not null;
            directory = directory.Parent
        )
        {
            if (
                File.Exists(Path.Combine(directory.FullName, "Flourish.slnx"))
                && Directory.Exists(Path.Combine(directory.FullName, "src", "Flourish"))
            )
            {
                return directory.FullName;
            }
        }

        throw new DirectoryNotFoundException(
            $"Could not locate the Flourish repository above {AppContext.BaseDirectory}."
        );
    }
}
