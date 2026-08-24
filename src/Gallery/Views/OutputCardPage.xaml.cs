using CKey = Arkheide.Essential.Culture.Key;
using Localizer = Arkheide.Essential.Culture.Localizer;
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
            new("Output", CKey.Controls_GetsTheCompleteAppendOnlyOutputText_42EB24EB),
            new(
                "WriteLine",
                CKey.Controls_AppendsOneLineAndScrollsTheViewportToTheLatestOutput_7EFCD505
            ),
            new("Clear", CKey.Controls_RemovesTheCompleteOutputHistory_5CC4506C),
        };
        HistoryOutput.WriteLine(Localizer.Parse(CKey.Runtime_OutputCardIsReady_D7FB9A68));
        HistoryOutput.WriteLine(
            Localizer.Parse(
                CKey.Runtime_EachActionAppendsALineInsteadOfReplacingHistory_3DF3CAE2
            )
        );
    }

    private void AppendMessage_Click(object sender, RoutedEventArgs e) =>
        WriteMessage(Localizer.Parse(CKey.Runtime_TheSampleOperationCompleted_1BE5D5D5));

    private void AppendBurst_Click(object sender, RoutedEventArgs e)
    {
        for (var index = 1; index <= BurstMessageCount; index++)
        {
            WriteMessage(
                Localizer.Parse(
                    CKey.Runtime_BurstEntry0Of1_11AD7242,
                    index,
                    BurstMessageCount
                )
            );
        }
    }

    private void InspectOutput_Click(object sender, RoutedEventArgs e)
    {
        var characterCount = HistoryOutput.Output.Length;
        WriteMessage(
            Localizer.Parse(
                CKey.Runtime_TheHistoryContained0CharactersBeforeThisSummary_269B14EE,
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
            Localizer.Parse(
                CKey.Runtime_Text0HHMmSsMessage12_6A44E992,
                DateTimeOffset.Now,
                messageSequence,
                message
            )
        );
    }
}
