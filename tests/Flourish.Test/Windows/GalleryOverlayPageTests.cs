using System.IO;
using System.Xml.Linq;

namespace ArkheideSystem.Flourish.Test.Windows;

public sealed class GalleryOverlayPageTests
{
    private const string XamlNamespace = "http://schemas.microsoft.com/winfx/2006/xaml";
    private static readonly string RepositoryRoot = FindRepositoryRoot();
    private static readonly string OverlayPageXamlPath = Path.Combine(
        RepositoryRoot,
        "src",
        "Gallery",
        "Views",
        "OverlayPage.xaml"
    );
    private static readonly string OverlayPageCode = File.ReadAllText(
        Path.ChangeExtension(OverlayPageXamlPath, ".xaml.cs")
    );

    [Fact]
    public void Examples_UseInteractiveTriggersAndRealPopupHostedOverlays()
    {
        var document = XDocument.Load(OverlayPageXamlPath);
        var temporaryTrigger = FindNamedElement(document, "TemporaryTrigger");
        var strongTrigger = FindNamedElement(document, "StrongTrigger");
        var temporaryPopup = FindNamedElement(document, "TemporaryPopup");
        var strongPopup = FindNamedElement(document, "StrongPopup");
        var temporaryOverlay = FindNamedElement(document, "TemporaryOverlay");
        var strongOverlay = FindNamedElement(document, "StrongOverlay");

        Assert.Equal("Button", temporaryTrigger.Name.LocalName);
        Assert.Equal("CardButton", strongTrigger.Name.LocalName);
        Assert.DoesNotContain(
            new[] { temporaryTrigger, strongTrigger },
            element => element.Name.LocalName == "Card"
        );
        AssertPopupContract(temporaryPopup, "True");
        AssertPopupContract(strongPopup, "False");
        Assert.Equal("Temporary", (string?)temporaryOverlay.Attribute("Variant"));
        Assert.Equal("Strong", (string?)strongOverlay.Attribute("Variant"));
        Assert.Equal(
            "Overlay_DismissRequested",
            (string?)temporaryOverlay.Attribute("DismissRequested")
        );
        Assert.Equal(
            "Overlay_DismissRequested",
            (string?)strongOverlay.Attribute("DismissRequested")
        );
    }

    [Fact]
    public void CodeBehind_ConnectsAnchorsDismissalKeyboardAndPageLifetime()
    {
        Assert.Contains("TemporaryPopup.PlacementTarget = TemporaryTrigger;", OverlayPageCode);
        Assert.Contains("TemporaryOverlay.PlacementTarget = TemporaryTrigger;", OverlayPageCode);
        Assert.Contains("StrongPopup.PlacementTarget = StrongTrigger;", OverlayPageCode);
        Assert.Contains("StrongOverlay.PlacementTarget = StrongTrigger;", OverlayPageCode);
        Assert.Contains("TemporaryPopup.IsOpen = true;", OverlayPageCode);
        Assert.Contains("StrongPopup.IsOpen = true;", OverlayPageCode);
        Assert.Contains("Overlay.DismissRequestedEvent", OverlayPageCode);
        Assert.Contains("e.Key != Key.Escape", OverlayPageCode);
        Assert.Contains("private void Page_Unloaded", OverlayPageCode);
        Assert.Contains("TemporaryPopup.IsOpen = false;", OverlayPageCode);
        Assert.Contains("StrongPopup.IsOpen = false;", OverlayPageCode);
    }

    private static void AssertPopupContract(XElement popup, string staysOpen)
    {
        Assert.Equal("True", (string?)popup.Attribute("AllowsTransparency"));
        Assert.Equal("Bottom", (string?)popup.Attribute("Placement"));
        Assert.Equal("Fade", (string?)popup.Attribute("PopupAnimation"));
        Assert.Equal(staysOpen, (string?)popup.Attribute("StaysOpen"));
    }

    private static XElement FindNamedElement(XDocument document, string name)
    {
        return Assert.Single(
            document.Descendants(),
            element => (string?)element.Attribute(XName.Get("Name", XamlNamespace)) == name
        );
    }

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            if (
                Directory.Exists(Path.Combine(directory.FullName, "src", "Flourish"))
                && Directory.Exists(Path.Combine(directory.FullName, "src", "Gallery"))
            )
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException("Could not locate the repository root.");
    }
}
