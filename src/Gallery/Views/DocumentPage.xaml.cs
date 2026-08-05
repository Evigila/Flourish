using System.Windows.Controls;
using ArkheideSystem.Gallery.Models;

namespace ArkheideSystem.Gallery.Views;

public partial class DocumentPage : Page
{
    public IReadOnlyList<ControlMemberRow> Properties { get; } =
    [
        new("Items", GalleryLocaleKeys.ControlsContainsParagraphElementsInReadingOrder_D7EBE677),
        new(
            "ItemsSource",
            GalleryLocaleKeys.ControlsBindsAnApplicationOwnedParagraphCollectionWhenNeeded_39E7CC3C
        ),
        new(
            "Margin",
            GalleryLocaleKeys.ControlsAddsTheStandardSeparationFromChunkTitleAndContentCopy_F2EFD46C
        ),
    ];

    public DocumentPage()
    {
        InitializeComponent();
    }
}
