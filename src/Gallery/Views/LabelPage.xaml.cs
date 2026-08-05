using System.Windows.Controls;
using ArkheideSystem.Gallery.Models;

namespace ArkheideSystem.Gallery.Views;

public partial class LabelPage : Page
{
    public LabelPage()
    {
        InitializeComponent();
        MemberGrid.ItemsSource = new ControlMemberRow[]
        {
            new("Content", GalleryLocaleKeys.ControlsSuppliesTextOrCustomLabelContent_2C033448),
            new(
                "Target",
                GalleryLocaleKeys.ControlsIdentifiesTheControlThatReceivesFocusThroughTheAccessKey_808886ED
            ),
            new("Padding", GalleryLocaleKeys.ControlsControlsSpaceAroundTheLabelContent_EC85A9CD),
            new(
                "HorizontalContentAlignment",
                GalleryLocaleKeys.ControlsAlignsContentWithinTheLabelBounds_8CD9E783
            ),
            new(
                "IsEnabled",
                GalleryLocaleKeys.ControlsReflectsWhetherTheAssociatedInputIsAvailable_B36D0858
            ),
            new("ToolTip", GalleryLocaleKeys.ControlsSuppliesOptionalSupportingGuidance_77EF85E6),
        };
    }

    public string UsageCode { get; } =
        """
            <StackPanel>
              <flourish:FlourishLabel
                Content="_Display name"
                Target="{Binding ElementName=DisplayNameBox}" />
              <flourish:FlourishTextBox
                x:Name="DisplayNameBox"
                Text="{Binding DisplayName, Mode=TwoWay}" />
            </StackPanel>

            // Alt+D moves keyboard focus to DisplayNameBox.
            """;
}
