using System.Windows;
using System.Windows.Controls;
using ArkheideSystem.Gallery.Localization;
using ArkheideSystem.Gallery.Models;

namespace ArkheideSystem.Gallery.Views;

public partial class OutputCardPage : Page
{
    private const int BurstMessageCount = 24;
    private readonly IGalleryLocalization localization;
    private int messageSequence;

    public OutputCardPage(IGalleryLocalization localization)
    {
        this.localization = localization;
        InitializeComponent();
        OutputCardMemberGrid.ItemsSource = new ControlMemberRow[]
        {
            new("Output", "Gets the complete append-only output text."),
            new("WriteLine", "Appends one line and scrolls the viewport to the latest output."),
            new("Clear", "Removes the complete output history."),
        };
        HistoryOutput.WriteLine(localization.Get("OutputCard is ready."));
        HistoryOutput.WriteLine(
            localization.Get("Each action appends a line instead of replacing history.")
        );
    }

    private void AppendMessage_Click(object sender, RoutedEventArgs e) =>
        WriteMessage(localization.Get("The sample operation completed."));

    private void AppendBurst_Click(object sender, RoutedEventArgs e)
    {
        for (var index = 1; index <= BurstMessageCount; index++)
        {
            WriteMessage(
                localization.Format("Burst entry {0} of {1}.", index, BurstMessageCount)
            );
        }
    }

    private void InspectOutput_Click(object sender, RoutedEventArgs e)
    {
        var characterCount = HistoryOutput.Output.Length;
        WriteMessage(
            localization.Format(
                "The history contained {0} characters before this summary.",
                characterCount
            )
        );
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
            localization.Format(
                "[{0:HH:mm:ss}] Message {1}: {2}",
                DateTimeOffset.Now,
                messageSequence,
                message
            )
        );
    }
}
