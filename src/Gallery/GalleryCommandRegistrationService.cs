using System.Windows;
using ArkheideSystem.Flourish.Abstract;

namespace ArkheideSystem.Gallery;

internal sealed class GalleryCommandRegistrationService : IDisposable
{
    private readonly List<ICommandRegistration> registrations = [];

    public GalleryCommandRegistrationService(
        ICommandRegistry commandRegistry,
        IMessageService messages,
        IBackgroundTaskService backgroundTasks
    )
    {
        ArgumentNullException.ThrowIfNull(commandRegistry);
        ArgumentNullException.ThrowIfNull(messages);
        ArgumentNullException.ThrowIfNull(backgroundTasks);

        try
        {
            Register(
                commandRegistry,
                "demo.hello",
                () => ShowCommandOutput(messages, "Hello")
            );
            Register(
                commandRegistry,
                "demo.world",
                () => ShowCommandOutput(messages, "World")
            );
            Register(
                commandRegistry,
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
            Register(
                commandRegistry,
                "tree.button1",
                () => ShowCommandOutput(messages, "Button1")
            );
            Register(
                commandRegistry,
                "tree.button2",
                () => ShowCommandOutput(messages, "Button2")
            );
            Register(
                commandRegistry,
                "app.about",
                () => ShowCommandOutput(messages, "关于")
            );
            Register(
                commandRegistry,
                "titlebar.trace",
                () => ShowCommandOutput(messages, "Titlebar command invoked")
            );
            Register(
                commandRegistry,
                "footer.trace",
                () => ShowCommandOutput(messages, "Footer command invoked")
            );
            Register(
                commandRegistry,
                "home.open",
                () =>
                    messages.Show(
                        "Hello, World!",
                        "Gallery",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information
                    )
            );
            Register(commandRegistry, "home.save", static () => { });
            Register(commandRegistry, "gallery.open", static () => { });
            Register(commandRegistry, "gallery.save", static () => { });
            Register(commandRegistry, "gallery.import", static () => { });
        }
        catch
        {
            Dispose();
            throw;
        }
    }

    public void Dispose()
    {
        for (var index = registrations.Count - 1; index >= 0; index--)
        {
            registrations[index].Dispose();
        }

        registrations.Clear();
    }

    private void Register(
        ICommandRegistry commandRegistry,
        string commandKey,
        Action execute
    )
    {
        registrations.Add(
            commandRegistry.Register(
                commandKey,
                (_, _) =>
                {
                    execute();
                    return ValueTask.FromResult(CommandResult.Handled);
                }
            )
        );
    }

    private static void ShowCommandOutput(IMessageService messages, string text)
    {
        messages.Show(text, "Gallery", MessageBoxButton.OK, MessageBoxImage.Information);
    }
}
