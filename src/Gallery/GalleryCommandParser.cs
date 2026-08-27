using System;
using System.Threading.Tasks;
using System.Windows;
using ArkheideSystem.Flourish.Abstract;
using CKey = ArkheideSystem.Essential.Culture.Key;
using Localizer = ArkheideSystem.Essential.Culture.Localizer;

namespace ArkheideSystem.Gallery;

internal sealed class GalleryCommandParser(
    IMessageService messages,
    IBackgroundTaskService backgroundTasks
) : ICommandParser
{
    internal const string DemoHello = "cmd_demo_hello";
    internal const string DemoBackground = "cmd_demo_background";

    public void RegisterCommands(ICommandRegistrar commands)
    {
        ArgumentNullException.ThrowIfNull(commands);
        ArgumentNullException.ThrowIfNull(messages);
        ArgumentNullException.ThrowIfNull(backgroundTasks);

        commands.Register(DemoHello, () => ShowCommandOutput(CKey.Runtime_Hello_185F8DB3));
        commands.Register(
            DemoBackground,
            () =>
                backgroundTasks.QueueTask(
                    new BackgroundTaskMetadata(
                        Localizer.Parse(CKey.Runtime_GalleryBackgroundTask_26C68541),
                        Localizer.Parse(
                            CKey.Runtime_ACancellableTenSecondTaskThatReportsProgress_C83A0037
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
    }

    private void ShowCommandOutput(string resourceKey)
    {
        messages.Show(
            Localizer.Parse(resourceKey),
            Localizer.Parse(CKey.Runtime_Gallery_352CFC74),
            MessageBoxButton.OK,
            MessageBoxImage.Information
        );
    }
}
