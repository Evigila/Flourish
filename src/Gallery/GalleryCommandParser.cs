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

        commands.Register("demo.hello", () => ShowCommandOutput("Hello"));
        commands.Register("demo.world", () => ShowCommandOutput("World"));
        commands.Register(
            "demo.background",
            () =>
                backgroundTasks.QueueTask(
                    new FlourishBackgroundTaskMetadata(
                        localization.Get("Gallery background task"),
                        localization.Get(
                            "A cancellable ten-second task that reports progress."
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
        commands.Register("tree.button1", () => ShowCommandOutput("Button1"));
        commands.Register("tree.button2", () => ShowCommandOutput("Button2"));
        commands.Register("app.about", () => ShowCommandOutput("About"));
        commands.Register(
            "titlebar.trace",
            () => ShowCommandOutput("Titlebar command invoked")
        );
        commands.Register(
            "footer.trace",
            () => ShowCommandOutput("Footer command invoked")
        );
        commands.Register(
            "home.open",
            () =>
                messages.Show(
                    localization.Get("Hello, World!"),
                    localization.Get("Gallery"),
                    MessageBoxButton.OK,
                    MessageBoxImage.Information
                )
        );
        commands.Register("home.save", static () => { });
        commands.Register("gallery.open", static () => { });
        commands.Register("gallery.save", static () => { });
        commands.Register("gallery.import", static () => { });
    }

    private void ShowCommandOutput(string text)
    {
        messages.Show(
            localization.Get(text),
            localization.Get("Gallery"),
            MessageBoxButton.OK,
            MessageBoxImage.Information
        );
    }
}
