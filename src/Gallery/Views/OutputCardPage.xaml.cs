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
            new("Output", GalleryLocaleKeys.ControlsGetsTheCompleteAppendOnlyOutputText_42EB24EB),
            new(
                "WriteLine",
                GalleryLocaleKeys.ControlsAppendsOneLineAndScrollsTheViewportToTheLatestOutput_7EFCD505
            ),
            new("Clear", GalleryLocaleKeys.ControlsRemovesTheCompleteOutputHistory_5CC4506C),
        };
        HistoryOutput.WriteLine(
            localization.Get(GalleryLocaleKeys.RuntimeOutputCardIsReady_D7FB9A68)
        );
        HistoryOutput.WriteLine(
            localization.Get(
                GalleryLocaleKeys.RuntimeEachActionAppendsALineInsteadOfReplacingHistory_3DF3CAE2
            )
        );
    }

    private void AppendMessage_Click(object sender, RoutedEventArgs e) =>
        WriteMessage(
            localization.Get(GalleryLocaleKeys.RuntimeTheSampleOperationCompleted_1BE5D5D5)
        );

    private void AppendBurst_Click(object sender, RoutedEventArgs e)
    {
        for (var index = 1; index <= BurstMessageCount; index++)
        {
            WriteMessage(
                localization.Format(
                    GalleryLocaleKeys.RuntimeBurstEntry0Of1_11AD7242,
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
            localization.Format(
                GalleryLocaleKeys.RuntimeTheHistoryContained0CharactersBeforeThisSummary_269B14EE,
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
                GalleryLocaleKeys.RuntimeText0HHMmSsMessage12_6A44E992,
                DateTimeOffset.Now,
                messageSequence,
                message
            )
        );
    }
}
