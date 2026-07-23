using System.Windows.Controls;
using ArkheideSystem.Gallery.Models;

namespace ArkheideSystem.Gallery.Views;

public partial class SearchBoxPage : Page
{
    public SearchBoxPage()
    {
        InitializeComponent();
        MemberGrid.ItemsSource = new ControlMemberRow[]
        {
            new("Placeholder", "Displays an in-control hint while the query is empty and unfocused."),
            new("Text", "Gets or sets the current search query."),
            new("IsReadOnly", "Prevents query edits while preserving selection."),
            new("MaxLength", "Limits the accepted query length."),
            new("TextChanged", "Reports each query update."),
            new("CommandBindings", "Connects keyboard gestures such as Enter to application search."),
        };
    }

    public string UsageCode { get; } =
        """
        <flourish:FlourishSearchBox
          x:Name="ControlSearch"
          Placeholder="Search controls"
          Text="{Binding Query, Mode=TwoWay, UpdateSourceTrigger=PropertyChanged}"
          KeyDown="ControlSearch_KeyDown" />

        private void ControlSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                SearchCommand.Execute(ControlSearch.Text);
        }
        """;
}
