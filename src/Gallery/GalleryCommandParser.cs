using System.Windows;
using ArkheideSystem.Flourish.Abstract;
using ArkheideSystem.Gallery.Localization;

namespace ArkheideSystem.Gallery;

internal sealed class GalleryCommandParser(
    IMessageService messages,
    IBackgroundTaskService backgroundTasks,
    IGalleryLocalization localization
) : ICommandParser
{
    public void RegisterCommands(ICommandRegistrar commands)
    {
        ArgumentNullException.ThrowIfNull(commands);
        ArgumentNullException.ThrowIfNull(messages);
        ArgumentNullException.ThrowIfNull(backgroundTasks);

        commands.Register(
            "demo.hello",
            () => ShowCommandOutput(GalleryLocaleKeys.RuntimeHello_185F8DB3)
        );
        commands.Register(
            "demo.world",
            () => ShowCommandOutput(GalleryLocaleKeys.RuntimeWorld_78AE647D)
        );
        commands.Register(
            "demo.background",
            () =>
                backgroundTasks.QueueTask(
                    new FlourishBackgroundTaskMetadata(
                        localization.Get(GalleryLocaleKeys.RuntimeGalleryBackgroundTask_26C68541),
                        localization.Get(
                            GalleryLocaleKeys.RuntimeACancellableTenSecondTaskThatReportsProgress_C83A0037
                        ),
                        "\uE895"
                    ),
                    async context =>
                    {
                        for (var step = 1; step <= 40; step++)
                        {
                            await Task.Delay(250, context.CancellationToken);
                            context.ReportProgress(step / 40d);
                        }
                    }
                )
        );
        commands.Register(
            "tree.button1",
            () => ShowCommandOutput(GalleryLocaleKeys.RuntimeButton1_BDA4837E)
        );
        commands.Register(
            "tree.button2",
            () => ShowCommandOutput(GalleryLocaleKeys.RuntimeButton2_9EF26615)
        );
        commands.Register(
            "app.about",
            () => ShowCommandOutput(GalleryLocaleKeys.ApplicationAbout_4EFCA0D1)
        );
        commands.Register(
            "titlebar.trace",
            () => ShowCommandOutput(GalleryLocaleKeys.RuntimeTitlebarCommandInvoked_5B658D8B)
        );
        commands.Register(
            "footer.trace",
            () => ShowCommandOutput(GalleryLocaleKeys.RuntimeFooterCommandInvoked_750C5860)
        );
        commands.Register(
            "home.open",
            () =>
                messages.Show(
                    localization.Get(GalleryLocaleKeys.RuntimeHelloWorld_DFFD6021),
                    localization.Get(GalleryLocaleKeys.RuntimeGallery_352CFC74),
                    MessageBoxButton.OK,
                    MessageBoxImage.Information
                )
        );
        commands.Register("home.save", static () => { });
        commands.Register("gallery.open", static () => { });
        commands.Register("gallery.save", static () => { });
        commands.Register("gallery.import", static () => { });
    }

    private void ShowCommandOutput(string resourceKey)
    {
        messages.Show(
            localization.Get(resourceKey),
            localization.Get(GalleryLocaleKeys.RuntimeGallery_352CFC74),
            MessageBoxButton.OK,
            MessageBoxImage.Information
        );
    }
}
