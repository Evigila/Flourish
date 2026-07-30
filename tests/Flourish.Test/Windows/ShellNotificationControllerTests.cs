using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using ArkheideSystem.Flourish.Abstract;
using ArkheideSystem.Flourish.Services;
using ArkheideSystem.Flourish.Views.Windows;
using Microsoft.Extensions.Logging.Abstractions;
using Button = ArkheideSystem.Flourish.Controls.Button;

namespace ArkheideSystem.Flourish.Test.Windows;

public sealed class ShellNotificationControllerTests
{
    [Fact]
    public void Changes_ShowNewestFiveReuseViewsAndStopAfterDispose()
    {
        StaTest.Run(() =>
        {
            using var service = new NotificationService(
                NullLogger<NotificationService>.Instance
            );
            var host = new FlourishNotificationHost();
            using var controller = new ShellNotificationController(
                host,
                service,
                new RecordingCommandDispatcher()
            );
            for (var index = 0; index < 6; index++)
            {
                service.Show(
                    new FlourishNotification(
                        $"id-{index}",
                        $"Title {index}",
                        $"Message {index}"
                    )
                );
            }

            DispatcherTest.DrainApplicationIdle();

            Assert.Equal(5, host.Items.Children.Count);
            Assert.Equal(
                ["Title 5", "Title 4", "Title 3", "Title 2", "Title 1"],
                host.Items.Children
                    .Cast<UIElement>()
                    .Select(item =>
                        AutomationProperties.GetName(item).Split(':', 2)[0]
                    )
            );
            var newestView = host.Items.Children[0];

            service.Upsert(
                new FlourishNotification("id-5", "Updated", "Updated message")
            );
            DispatcherTest.DrainApplicationIdle();

            Assert.Same(newestView, host.Items.Children[0]);
            Assert.Equal(
                "Updated: Updated message",
                AutomationProperties.GetName(host.Items.Children[0])
            );

            controller.Dispose();
            service.Upsert(new FlourishNotification("after-close", "Late", "Ignored"));
            DispatcherTest.DrainApplicationIdle();

            Assert.Equal(5, host.Items.Children.Count);
            Assert.Same(newestView, host.Items.Children[0]);
        });
    }

    [Fact]
    public void ActionAndDismissButtons_DelegateToRuntimeServices()
    {
        StaTest.Run(() =>
        {
            using var service = new NotificationService(
                NullLogger<NotificationService>.Instance
            );
            var dispatcher = new RecordingCommandDispatcher();
            var host = new FlourishNotificationHost();
            using var controller = new ShellNotificationController(
                host,
                service,
                dispatcher
            );
            service.Show(
                new FlourishNotification(
                    "action",
                    "Action",
                    "Run it",
                    CommandKey: "notification.run"
                )
            );
            DispatcherTest.DrainApplicationIdle();
            var surface = Assert.IsType<Border>(Assert.Single(host.Items.Children));
            var layout = Assert.IsType<Grid>(surface.Child);
            var content = Assert.IsType<StackPanel>(
                layout.Children.Cast<UIElement>().Single(element => Grid.GetColumn(element) == 1)
            );
            var action = Assert.IsType<Button>(content.Children[2]);
            var dismiss = Assert.IsType<Button>(
                layout.Children.Cast<UIElement>().Single(element => Grid.GetColumn(element) == 2)
            );

            action.RaiseEvent(new RoutedEventArgs(System.Windows.Controls.Button.ClickEvent));

            Assert.Equal("notification.run", dispatcher.CommandKey);
            Assert.Equal(CommandSource.Notification, dispatcher.Source);
            Assert.IsType<FlourishNotification>(dispatcher.Parameter);

            dismiss.RaiseEvent(new RoutedEventArgs(System.Windows.Controls.Button.ClickEvent));
            DispatcherTest.DrainApplicationIdle();

            Assert.Empty(service.ActiveNotifications);
            Assert.Empty(host.Items.Children);
        });
    }

    private sealed class RecordingCommandDispatcher : ICommandDispatcher
    {
        internal string? CommandKey { get; private set; }

        internal object? Parameter { get; private set; }

        internal CommandSource Source { get; private set; }

        public bool CanExecute(
            string commandKey,
            object? parameter = null,
            CommandSource source = CommandSource.Application
        ) => true;

        public ValueTask<CommandResult> ExecuteAsync(
            string commandKey,
            object? parameter = null,
            CommandSource source = CommandSource.Application,
            CancellationToken cancellationToken = default
        )
        {
            CommandKey = commandKey;
            Parameter = parameter;
            Source = source;
            return ValueTask.FromResult(CommandResult.Handled);
        }
    }
}
