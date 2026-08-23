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
            new("Content", Key.Controls_SuppliesTextOrCustomLabelContent_2C033448),
            new(
                "Target",
                Key.Controls_IdentifiesTheControlThatReceivesFocusThroughTheAccessKey_808886ED
            ),
            new("Padding", Key.Controls_ControlsSpaceAroundTheLabelContent_EC85A9CD),
            new(
                "HorizontalContentAlignment",
                Key.Controls_AlignsContentWithinTheLabelBounds_8CD9E783
            ),
            new(
                "IsEnabled",
                Key.Controls_ReflectsWhetherTheAssociatedInputIsAvailable_B36D0858
            ),
            new("ToolTip", Key.Controls_SuppliesOptionalSupportingGuidance_77EF85E6),
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
