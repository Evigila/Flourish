using System.Windows.Controls;
using ArkheideSystem.Gallery.Models;

namespace ArkheideSystem.Gallery.Views;

public partial class TextBoxPage : Page
{
    public TextBoxPage()
    {
        InitializeComponent();
        MemberGrid.ItemsSource = new ControlMemberRow[]
        {
            new("Text", "Gets or sets the editable text value."),
            new("IsReadOnly", "Prevents edits while preserving selection and copying."),
            new("AcceptsReturn", "Allows the Enter key to insert a new line."),
            new("TextWrapping", "Wraps long text within the available width."),
            new("MaxLength", "Limits the number of accepted characters."),
            new("TextChanged", "Reports edits made by the user or application."),
        };
    }

    public string UsageCode { get; } =
        """
        <flourish:FlourishTextBox
          Text="{Binding DisplayName, Mode=TwoWay, UpdateSourceTrigger=PropertyChanged}"
          MaxLength="80" />

        private void SetName(string value)
        {
            NameBox.Text = value;
            NameBox.SelectAll();
            NameBox.Focus();
        }
        """;
}
