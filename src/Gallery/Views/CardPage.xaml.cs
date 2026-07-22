using System.Windows;
using System.Windows.Controls;
using ArkheideSystem.Gallery.Models;

namespace ArkheideSystem.Gallery.Views;

public partial class CardPage : Page
{
    private const int BurstMessageCount = 24;
    private int messageSequence;

    public CardPage()
    {
        InitializeComponent();
        CardMemberGrid.ItemsSource = new ControlMemberRow[]
        {
            new("Variant", "Chooses Standard, Tonal, Filled, or Elevated."),
            new("Title", "Sets the heading."),
            new("Content", "Sets the single block of supporting copy."),
            new("Icon", "Sets one optional icon glyph."),
            new("ContentHorizontalAlignment", "Aligns the title-and-copy group horizontally."),
            new("ContentVerticalAlignment", "Aligns the title-and-copy group vertically."),
        };
        HistoryOutput.WriteLine("OutputCard is ready.");
        HistoryOutput.WriteLine(
            "Each action appends a line instead of replacing the existing history."
        );
    }

    private void AppendMessage_Click(object sender, RoutedEventArgs e) =>
        WriteMessage("The sample operation completed.");

    private void AppendBurst_Click(object sender, RoutedEventArgs e)
    {
        for (var index = 1; index <= BurstMessageCount; index++)
        {
            WriteMessage($"Burst entry {index} of {BurstMessageCount}.");
        }
    }

    private void InspectOutput_Click(object sender, RoutedEventArgs e)
    {
        var characterCount = HistoryOutput.Output.Length;
        WriteMessage($"The history contained {characterCount} characters before this summary.");
    }

    private void ClearHistory_Click(object sender, RoutedEventArgs e)
    {
        HistoryOutput.Clear();
        messageSequence = 0;
    }

    private void WriteMessage(string message)
    {
        messageSequence++;
        HistoryOutput.WriteLine(
            $"[{DateTimeOffset.Now:HH:mm:ss}] Message {messageSequence}: {message}"
        );
    }
}
