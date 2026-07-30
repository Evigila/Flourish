using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Threading;
using ArkheideSystem.Flourish.Abstract;
using ArkheideSystem.Flourish.Controls;
using ArkheideSystem.Flourish.Services;
using Button = ArkheideSystem.Flourish.Controls.Button;
using TextBlock = ArkheideSystem.Flourish.Controls.FlourishTextBlock;
using WpfPanel = System.Windows.Controls.Panel;

namespace ArkheideSystem.Flourish.Views.Windows;

internal sealed class ShellNotificationController : IDisposable
{
    private readonly Lock refreshGate = new();
    private readonly FlourishNotificationHost host;
    private readonly NotificationService notificationService;
    private readonly ICommandDispatcher commandDispatcher;
    private readonly Dispatcher dispatcher;
    private readonly Dictionary<string, NotificationItemView> viewsById = new(
        StringComparer.Ordinal
    );
    private IReadOnlyList<FlourishNotificationInfo> pendingNotifications = [];
    private long pendingVersion;
    private long appliedVersion;
    private bool refreshPending;
    private bool isDisposed;

    internal ShellNotificationController(
        FlourishNotificationHost host,
        NotificationService notificationService,
        ICommandDispatcher commandDispatcher
    )
    {
        this.host = host ?? throw new ArgumentNullException(nameof(host));
        this.notificationService =
            notificationService ?? throw new ArgumentNullException(nameof(notificationService));
        this.commandDispatcher =
            commandDispatcher ?? throw new ArgumentNullException(nameof(commandDispatcher));
        dispatcher = host.Dispatcher;
        appliedVersion = notificationService.CurrentVersion;
        BuildNotifications(notificationService.ActiveNotifications);
        notificationService.NotificationsChanged += NotificationService_NotificationsChanged;
    }

    public void Dispose()
    {
        lock (refreshGate)
        {
            if (isDisposed)
            {
                return;
            }

            isDisposed = true;
            refreshPending = false;
            pendingNotifications = [];
        }

        notificationService.NotificationsChanged -= NotificationService_NotificationsChanged;
        foreach (var view in viewsById.Values)
        {
            view.Action.Click -= NotificationAction_Click;
            view.Dismiss.Click -= NotificationDismiss_Click;
        }

        viewsById.Clear();
    }

    private void NotificationService_NotificationsChanged(
        object? sender,
        FlourishNotificationsChangedEventArgs e
    )
    {
        lock (refreshGate)
        {
            if (
                isDisposed
                || e.Version <= appliedVersion
                || e.Version <= pendingVersion
            )
            {
                return;
            }

            pendingNotifications = e.Notifications;
            pendingVersion = e.Version;
            if (refreshPending)
            {
                return;
            }

            refreshPending = true;
        }

        if (dispatcher.HasShutdownStarted || dispatcher.HasShutdownFinished)
        {
            return;
        }

        _ = dispatcher.BeginInvoke(
            DispatcherPriority.DataBind,
            new Action(FlushPendingNotifications)
        );
    }

    private void FlushPendingNotifications()
    {
        IReadOnlyList<FlourishNotificationInfo> notifications;
        lock (refreshGate)
        {
            if (isDisposed)
            {
                return;
            }

            notifications = pendingNotifications;
            var version = pendingVersion;
            refreshPending = false;
            if (version <= appliedVersion)
            {
                return;
            }

            appliedVersion = version;
        }

        BuildNotifications(notifications);
    }

    private void BuildNotifications(IReadOnlyList<FlourishNotificationInfo> notifications)
    {
        var activeIds = notifications
            .Select(info => info.Notification.Id)
            .ToHashSet(StringComparer.Ordinal);
        foreach (
            var removedId in viewsById.Keys
                .Where(id => !activeIds.Contains(id))
                .ToArray()
        )
        {
            if (viewsById.Remove(removedId, out var removed))
            {
                removed.Action.Click -= NotificationAction_Click;
                removed.Dismiss.Click -= NotificationDismiss_Click;
            }
        }

        var visibleNotifications = notifications.Reverse().Take(5).ToArray();
        var desiredViews = new List<UIElement>(visibleNotifications.Length);
        foreach (var info in visibleNotifications)
        {
            var view = GetOrCreateNotificationView(info.Notification.Id);
            UpdateNotificationView(view, info);
            desiredViews.Add(view.Container);
        }

        SynchronizePanelChildren(host.Items, desiredViews);
        host.UpdateVisibility();
    }

    private NotificationItemView GetOrCreateNotificationView(string id)
    {
        if (viewsById.TryGetValue(id, out var cached))
        {
            return cached;
        }

        var layout = new Grid();
        layout.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
        layout.ColumnDefinitions.Add(
            new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) }
        );
        layout.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

        var icon = new TextBlock
        {
            Width = 24,
            Margin = new Thickness(0, 2, 10, 0),
            VerticalAlignment = VerticalAlignment.Top,
        };
        BindIconTypography(icon, "FlourishFontSizeIcon");
        icon.SetResourceReference(TextBlock.ForegroundProperty, "FlourishPrimaryForegroundBrush");
        layout.Children.Add(icon);

        var title = new FlourishTextBlock { Role = FlourishTextRole.CardTitle };
        var message = new FlourishTextBlock
        {
            Margin = new Thickness(0, 4, 0, 0),
            Role = FlourishTextRole.Description,
        };
        var action = new Button
        {
            Margin = new Thickness(0, 8, 0, 0),
            HorizontalAlignment = System.Windows.HorizontalAlignment.Left,
            Height = 28,
            MinWidth = 28,
            MinHeight = 0,
            Padding = new Thickness(7, 0, 7, 0),
            Content = "Run action",
            Variant = ButtonVariant.Text,
        };
        action.Click += NotificationAction_Click;
        var content = new StackPanel { Children = { title, message, action } };
        Grid.SetColumn(content, 1);
        layout.Children.Add(content);

        var dismiss = new Button
        {
            Width = 28,
            Height = 28,
            MinWidth = 0,
            MinHeight = 0,
            Padding = new Thickness(),
            Margin = new Thickness(8, 0, 0, 0),
            Icon = CreateIconContent("\uE711"),
            Tag = id,
            Variant = ButtonVariant.Text,
            ToolTip = "Dismiss",
        };
        dismiss.Click += NotificationDismiss_Click;
        Grid.SetColumn(dismiss, 2);
        layout.Children.Add(dismiss);

        var surface = new Border
        {
            Padding = new Thickness(14, 12, 10, 12),
            Margin = new Thickness(0, 0, 0, 14),
            Child = layout,
        };
        surface.SetResourceReference(Border.BackgroundProperty, "FlourishCardBackgroundBrush");
        surface.SetResourceReference(Border.BorderBrushProperty, "FlourishNeutralStroke1Brush");
        surface.SetResourceReference(
            Border.BorderThicknessProperty,
            "FlourishControlBorderThickness"
        );
        surface.SetResourceReference(Border.CornerRadiusProperty, "FlourishOverlayCornerRadius");

        var view = new NotificationItemView(surface, icon, title, message, action, dismiss);
        viewsById.Add(id, view);
        return view;
    }

    private static void UpdateNotificationView(
        NotificationItemView view,
        FlourishNotificationInfo info
    )
    {
        if (view.Version == info.Version)
        {
            return;
        }

        var definition = info.Notification;
        view.Icon.Text = definition.IconGlyph ?? GetNotificationGlyph(definition.Severity);
        view.Title.Text = definition.Title;
        view.Message.Text = definition.Message;
        view.Action.Tag = info;
        view.Action.Visibility = string.IsNullOrWhiteSpace(definition.CommandKey)
            ? Visibility.Collapsed
            : Visibility.Visible;
        view.Dismiss.Tag = definition.Id;
        AutomationProperties.SetName(
            view.Container,
            $"{definition.Title}: {definition.Message}"
        );
        view.Version = info.Version;
    }

    private async void NotificationAction_Click(object sender, RoutedEventArgs e)
    {
        if (
            isDisposed
            || sender is not Button { Tag: FlourishNotificationInfo info }
            || string.IsNullOrWhiteSpace(info.Notification.CommandKey)
        )
        {
            return;
        }

        await commandDispatcher.ExecuteAsync(
            info.Notification.CommandKey,
            info.Notification,
            CommandSource.Notification
        );
    }

    private void NotificationDismiss_Click(object sender, RoutedEventArgs e)
    {
        if (!isDisposed && sender is Button { Tag: string id })
        {
            notificationService.Dismiss(id);
        }
    }

    private static string GetNotificationGlyph(FlourishNotificationSeverity severity) =>
        severity switch
        {
            FlourishNotificationSeverity.Success => "\uE930",
            FlourishNotificationSeverity.Warning => "\uE7BA",
            FlourishNotificationSeverity.Error => "\uEA39",
            _ => "\uE946",
        };

    private static TextBlock CreateIconContent(string iconGlyph)
    {
        var icon = new TextBlock
        {
            VerticalAlignment = VerticalAlignment.Center,
            Role = FlourishTextRole.Icon,
            Text = iconGlyph,
        };
        BindIconTypography(icon, "FlourishFontSizeIcon");
        return icon;
    }

    private static void BindIconTypography(TextBlock textBlock, string sizeResourceKey)
    {
        textBlock.SetResourceReference(
            System.Windows.Controls.Control.FontFamilyProperty,
            "FlourishIconFontFamily"
        );
        textBlock.SetResourceReference(
            System.Windows.Controls.Control.FontSizeProperty,
            sizeResourceKey
        );
        textBlock.SetResourceReference(TextBlock.LineHeightProperty, sizeResourceKey);
    }

    private static void SynchronizePanelChildren(
        WpfPanel panel,
        IReadOnlyList<UIElement> desiredChildren
    )
    {
        for (var index = 0; index < desiredChildren.Count; index++)
        {
            var element = desiredChildren[index];
            if (index < panel.Children.Count && ReferenceEquals(panel.Children[index], element))
            {
                continue;
            }

            var existingIndex = panel.Children.IndexOf(element);
            if (existingIndex >= 0)
            {
                panel.Children.RemoveAt(existingIndex);
            }

            panel.Children.Insert(index, element);
        }

        while (panel.Children.Count > desiredChildren.Count)
        {
            panel.Children.RemoveAt(panel.Children.Count - 1);
        }
    }

    private sealed class NotificationItemView(
        Border container,
        TextBlock icon,
        TextBlock title,
        TextBlock message,
        Button action,
        Button dismiss
    )
    {
        internal Border Container { get; } = container;

        internal TextBlock Icon { get; } = icon;

        internal TextBlock Title { get; } = title;

        internal TextBlock Message { get; } = message;

        internal Button Action { get; } = action;

        internal Button Dismiss { get; } = dismiss;

        internal long Version { get; set; } = -1;
    }
}
