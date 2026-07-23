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
            new("Content", "Supplies text or custom label content."),
            new("Target", "Identifies the control that receives focus through the access key."),
            new("Padding", "Controls space around the label content."),
            new("HorizontalContentAlignment", "Aligns content within the label bounds."),
            new("IsEnabled", "Reflects whether the associated input is available."),
            new("ToolTip", "Supplies optional supporting guidance."),
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
