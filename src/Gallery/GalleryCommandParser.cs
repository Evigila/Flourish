using System.Windows;
using ArkheideSystem.Flourish.Abstract;

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

        commands.Register("demo.hello", () => ShowCommandOutput(messages, "Hello"));
        commands.Register("demo.world", () => ShowCommandOutput(messages, "World"));
        commands.Register(
            "demo.background",
            () =>
                backgroundTasks.AddTask(
                    new FlourishBackgroundTaskMetadata(
                        "Gallery background task",
                        "A cancellable ten-second task that reports progress.",
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
        commands.Register("tree.button1", () => ShowCommandOutput(messages, "Button1"));
        commands.Register("tree.button2", () => ShowCommandOutput(messages, "Button2"));
        commands.Register("app.about", () => ShowCommandOutput(messages, "关于"));
        commands.Register(
            "titlebar.trace",
            () => ShowCommandOutput(messages, "Titlebar command invoked")
        );
        commands.Register(
            "footer.trace",
            () => ShowCommandOutput(messages, "Footer command invoked")
        );
        commands.Register(
            "home.open",
            () =>
                messages.Show(
                    "Hello, World!",
                    "Gallery",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information
                )
        );
        commands.Register("home.save", static () => { });
        commands.Register("gallery.open", static () => { });
        commands.Register("gallery.save", static () => { });
        commands.Register("gallery.import", static () => { });
    }

    private static void ShowCommandOutput(IMessageService messages, string text)
    {
        messages.Show(text, "Gallery", MessageBoxButton.OK, MessageBoxImage.Information);
    }
}
