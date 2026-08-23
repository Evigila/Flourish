using System.Windows;

namespace ArkheideSystem.Gallery;

internal sealed class GalleryCommandParser(
    IMessageService messages,
    IBackgroundTaskService backgroundTasks
) : ICommandParser
{
    public void RegisterCommands(ICommandRegistrar commands)
    {
        ArgumentNullException.ThrowIfNull(commands);
        ArgumentNullException.ThrowIfNull(messages);
        ArgumentNullException.ThrowIfNull(backgroundTasks);

        commands.Register(
            GalleryCommandKeys.DemoHello,
            () => ShowCommandOutput(Key.Runtime_Hello_185F8DB3)
        );
        commands.Register(
            GalleryCommandKeys.DemoWorld,
            () => ShowCommandOutput(Key.Runtime_World_78AE647D)
        );
        commands.Register(
            GalleryCommandKeys.DemoBackground,
            () =>
                backgroundTasks.QueueTask(
                    new FlourishBackgroundTaskMetadata(
                        Localizer.Parse(Key.Runtime_GalleryBackgroundTask_26C68541),
                        Localizer.Parse(
                            Key.Runtime_ACancellableTenSecondTaskThatReportsProgress_C83A0037
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
            GalleryCommandKeys.TreeButton1,
            () => ShowCommandOutput(Key.Runtime_Button1_BDA4837E)
        );
        commands.Register(
            GalleryCommandKeys.TreeButton2,
            () => ShowCommandOutput(Key.Runtime_Button2_9EF26615)
        );
        commands.Register(
            GalleryCommandKeys.AppAbout,
            () => ShowCommandOutput(Key.Application_About_4EFCA0D1)
        );
        commands.Register(
            GalleryCommandKeys.TitleBarTrace,
            () => ShowCommandOutput(Key.Runtime_TitlebarCommandInvoked_5B658D8B)
        );
        commands.Register(
            GalleryCommandKeys.FooterTrace,
            () => ShowCommandOutput(Key.Runtime_FooterCommandInvoked_750C5860)
        );
        commands.Register(
            GalleryCommandKeys.HomeOpen,
            () =>
                messages.Show(
                    Localizer.Parse(Key.Runtime_HelloWorld_DFFD6021),
                    Localizer.Parse(Key.Runtime_Gallery_352CFC74),
                    MessageBoxButton.OK,
                    MessageBoxImage.Information
                )
        );
        commands.Register(GalleryCommandKeys.HomeSave, static () => { });
        commands.Register(GalleryCommandKeys.GalleryOpen, static () => { });
        commands.Register(GalleryCommandKeys.GallerySave, static () => { });
        commands.Register(GalleryCommandKeys.GalleryImport, static () => { });
    }

    private void ShowCommandOutput(string resourceKey)
    {
        messages.Show(
            Localizer.Parse(resourceKey),
            Localizer.Parse(Key.Runtime_Gallery_352CFC74),
            MessageBoxButton.OK,
            MessageBoxImage.Information
        );
    }
}
