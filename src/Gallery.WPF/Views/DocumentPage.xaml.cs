using System.Collections.Generic;

using CKey = ArkheideSystem.Essential.Culture.Key;
using System.Windows.Controls;
using ArkheideSystem.Gallery.WPF.Models;

namespace ArkheideSystem.Gallery.WPF.Views;

public partial class DocumentPage : Page
{
    public IReadOnlyList<MemberRow> Properties { get; } =
    [
        new("Items", CKey.Controls_ContainsParagraphElementsInReadingOrder_D7EBE677),
        new(
            "ItemsSource",
            CKey.Controls_BindsAnApplicationOwnedParagraphCollectionWhenNeeded_39E7CC3C
        ),
        new(
            "Margin",
            CKey.Controls_AddsTheStandardSeparationFromChunkTitleAndContentCopy_F2EFD46C
        ),
    ];

    public DocumentPage()
    {
        InitializeComponent();
    }
}
