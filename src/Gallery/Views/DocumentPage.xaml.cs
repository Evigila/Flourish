using System.Windows.Controls;
using ArkheideSystem.Gallery.Models;

namespace ArkheideSystem.Gallery.Views;

public partial class DocumentPage : Page
{
    public IReadOnlyList<ControlMemberRow> Properties { get; } =
    [
        new("Items", Key.Controls_ContainsParagraphElementsInReadingOrder_D7EBE677),
        new(
            "ItemsSource",
            Key.Controls_BindsAnApplicationOwnedParagraphCollectionWhenNeeded_39E7CC3C
        ),
        new(
            "Margin",
            Key.Controls_AddsTheStandardSeparationFromChunkTitleAndContentCopy_F2EFD46C
        ),
    ];

    public DocumentPage()
    {
        InitializeComponent();
    }
}
