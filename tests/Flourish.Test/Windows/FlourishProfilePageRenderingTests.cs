using System.IO;
using System.Windows.Controls;
using System.Windows.Data;
using System.Xml.Linq;
using ArkheideSystem.Flourish.Abstract;
using ArkheideSystem.Flourish.Internal.Configuration;
using ArkheideSystem.Flourish.Services;
using ArkheideSystem.Flourish.Views.Page;
using Moq;
using Shape = System.Windows.Shapes.Shape;
using WpfPath = System.Windows.Shapes.Path;

namespace ArkheideSystem.Flourish.Test.Windows;

public sealed class FlourishProfilePageRenderingTests
{
    private const string XamlNamespace =
        "http://schemas.microsoft.com/winfx/2006/xaml";
    private const string FlourishControlsNamespace =
        "clr-namespace:ArkheideSystem.Flourish.Controls";
    private static readonly string ProfileXamlPath = Path.Combine(
        TestPaths.RepositoryRoot,
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
        Assert.Contains(
            uploadButton.Elements(),
            element => element.Name.LocalName == "Button.Icon"
        );
        Assert.Equal(
            "{Binding Path=(TextElement.Foreground), RelativeSource={RelativeSource Self}}",
            (string?)uploadButton
                .Descendants()
                .Single(element => element.Name.LocalName == "Path")
                .Attribute("Stroke")
        );
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
    public void UploadImageIcon_ResolvesItsButtonSourceWhenThePageIsCreated()
    {
        StaTest.Run(() =>
        {
            var profileService = new Mock<IProfileService>();
            profileService
                .SetupGet(service => service.CurrentProfile)
                .Returns(new ProfileUser("User", "", NameOrder.FirstLast));
            profileService
                .SetupGet(service => service.LoginState)
                .Returns(ProfileLoginState.SignedOut);
            profileService.SetupGet(service => service.NameOrder).Returns(NameOrder.FirstLast);

            var page = new FlourishProfilePage(
                profileService.Object,
                new FlourishLocalizationService(new FlourishDataOptions())
            );
            var uploadButton = Assert.IsType<ArkheideSystem.Flourish.Controls.Button>(
                page.FindName("UploadImageButton")
            );
            var iconHost = Assert.IsType<Viewbox>(uploadButton.Icon);
            var icon = Assert.IsType<WpfPath>(iconHost.Child);
            var binding = Assert.IsType<BindingExpression>(
                BindingOperations.GetBindingExpression(icon, Shape.StrokeProperty)
            );

            Assert.Same(RelativeSource.Self, binding.ParentBinding.RelativeSource);
            Assert.Equal(BindingStatus.Active, binding.Status);
            Assert.False(binding.HasError);
        });
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

    [Fact]
    public void AvatarPreview_ReusesTheDecodedImageWhileNamesChange()
    {
        var source = File.ReadAllText(ProfileCodePath);

        Assert.Contains("profileImageCache.Get(profile.ImagePath)", source);
        Assert.Contains("profileImageCache.Set(profile.ImagePath, imageSource)", source);
        Assert.Contains("profileImageCache.Set(dialog.FileName, imageSource)", source);
        Assert.Equal(2, CountOccurrences(source, "ProfileImageLoader.Load("));
    }

    private static int CountOccurrences(string source, string value)
    {
        var count = 0;
        var startIndex = 0;
        while ((startIndex = source.IndexOf(value, startIndex, StringComparison.Ordinal)) >= 0)
        {
            count++;
            startIndex += value.Length;
        }

        return count;
    }

    private static XElement FindNamedElement(XDocument document, string name)
    {
        return document
            .Descendants()
            .Single(element =>
                (string?)element.Attribute(XName.Get("Name", XamlNamespace)) == name
            );
    }
}
