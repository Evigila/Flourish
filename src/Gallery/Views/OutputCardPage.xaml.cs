using System.Windows;
using System.Windows.Controls;
using ArkheideSystem.Gallery.Models;

namespace ArkheideSystem.Gallery.Views;

public partial class OutputCardPage : Page
{
    private const int BurstMessageCount = 24;
    private int messageSequence;

    public OutputCardPage()
    {
        InitializeComponent();
        OutputCardMemberGrid.ItemsSource = new ControlMemberRow[]
        {
            new("Output", "Gets the complete append-only output text."),
            new("WriteLine", "Appends one line and scrolls the viewport to the latest output."),
            new("Clear", "Removes the complete output history."),
        };
        HistoryOutput.WriteLine("OutputCard is ready.");
        HistoryOutput.WriteLine("Each action appends a line instead of replacing history.");
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
