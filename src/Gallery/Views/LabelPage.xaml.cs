using CKey = ArkheideSystem.Essential.Culture.Key;
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
            new("Content", CKey.Controls_SuppliesTextOrCustomLabelContent_2C033448),
            new(
                "Target",
                CKey.Controls_IdentifiesTheControlThatReceivesFocusThroughTheAccessKey_808886ED
            ),
            new("Padding", CKey.Controls_ControlsSpaceAroundTheLabelContent_EC85A9CD),
            new(
                "HorizontalContentAlignment",
                CKey.Controls_AlignsContentWithinTheLabelBounds_8CD9E783
            ),
            new(
                "IsEnabled",
                CKey.Controls_ReflectsWhetherTheAssociatedInputIsAvailable_B36D0858
            ),
            new("ToolTip", CKey.Controls_SuppliesOptionalSupportingGuidance_77EF85E6),
        };
    }

    public string UsageCode { get; } =
        """
            <StackPanel>
              <flourish:Label
                Content="_Display name"
                Target="{Binding ElementName=DisplayNameBox}" />
              <flourish:TextBox
                x:Name="DisplayNameBox"
                Text="{Binding DisplayName, Mode=TwoWay}" />
            </StackPanel>

            // Alt+D moves keyboard focus to DisplayNameBox.
            """;
}
