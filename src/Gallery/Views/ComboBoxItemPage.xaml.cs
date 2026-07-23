using System.Windows.Controls;
using ArkheideSystem.Gallery.Models;

namespace ArkheideSystem.Gallery.Views;

public partial class ComboBoxItemPage : Page
{
    public ComboBoxItemPage()
    {
        InitializeComponent();
        MemberGrid.ItemsSource = new ControlMemberRow[]
        {
            new("Content", "Supplies the visible option content."),
            new("IsSelected", "Gets or sets whether this option is selected."),
            new("IsHighlighted", "Reports whether keyboard or pointer navigation highlights the item."),
            new("IsEnabled", "Controls whether the option can be selected."),
            new("ToolTip", "Explains an option or why a disabled option is unavailable."),
            new("HoverReveal.IsEnabled", "Controls the shared pointer-reveal feedback."),
        };
    }

    public string UsageCode { get; } =
        """
        <flourish:FlourishComboBox SelectedIndex="0">
          <flourish:FlourishComboBoxItem Content="Stable channel" />
          <flourish:FlourishComboBoxItem
            Content="Preview channel"
            IsEnabled="False"
            ToolTip="Preview updates are not configured." />
        </flourish:FlourishComboBox>

        // Data-bound items receive FlourishComboBoxItem containers automatically.
        """;
}
