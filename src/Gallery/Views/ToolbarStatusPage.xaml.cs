using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ArkheideSystem.Flourish.Abstract;
using ArkheideSystem.Flourish.Controls;

namespace ArkheideSystem.Gallery.Views;

public partial class ToolbarStatusPage : Page
{
    private const string ToolbarItemId = "gallery.runtime.toolbar";
    private const string CompanionToolbarItemId = "gallery.runtime.toolbar.companion";
    private const string ToolbarCommandKey = "gallery.runtime.toolbar.execute";
    private const string StatusItemId = "gallery.runtime.status";
    private const string RegionId = "gallery.runtime.content-header";

    private readonly IToolbarService toolbar;
    private readonly IStatusBarService status;
    private readonly IShellRegionService regions;
    private readonly ICommandRegistry commands;
    private ICommandRegistration? commandRegistration;

    public ToolbarStatusPage(
        IToolbarService toolbar,
        IStatusBarService status,
        IShellRegionService regions,
        ICommandRegistry commands
    )
    {
        this.toolbar = toolbar;
        this.status = status;
        this.regions = regions;
        this.commands = commands;
        InitializeComponent();

        Loaded += Page_Loaded;
        Unloaded += Page_Unloaded;
        RefreshState();
    }

    private void Page_Loaded(object sender, RoutedEventArgs e)
    {
        commandRegistration ??= commands.Register(
            ToolbarCommandKey,
            ExecuteToolbarCommandAsync,
            options: new CommandRegistrationOptions
            {
                DuplicatePolicy = CommandDuplicatePolicy.Replace,
            }
        );
        toolbar.Changed -= RuntimeState_Changed;
        status.Changed -= RuntimeState_Changed;
        regions.Changed -= RuntimeState_Changed;
        toolbar.Changed += RuntimeState_Changed;
        status.Changed += RuntimeState_Changed;
        regions.Changed += RuntimeState_Changed;
        RefreshState();
    }

    private void Page_Unloaded(object sender, RoutedEventArgs e)
    {
        toolbar.Changed -= RuntimeState_Changed;
        status.Changed -= RuntimeState_Changed;
        regions.Changed -= RuntimeState_Changed;
        commandRegistration?.Dispose();
        commandRegistration = null;
    }

    private void AddToolbarItem_Click(object sender, RoutedEventArgs e)
    {
        toolbar.SetEnabled(true);
        toolbar.Upsert(
            new FlourishToolbarItem("Run live command", "\uE768", ToolbarCommandKey)
            {
                Id = ToolbarItemId,
            },
            typeof(ToolbarStatusPage)
        );
        toolbar.Upsert(
            new FlourishToolbarItem("Companion", "\uE8EF", ToolbarCommandKey)
            {
                Id = CompanionToolbarItemId,
            },
            typeof(ToolbarStatusPage)
        );
    }

    private void ToggleToolbarItemEnabled_Click(object sender, RoutedEventArgs e)
    {
        var item = GetToolbarItem();
        if (item is not null)
        {
            toolbar.SetItemEnabled(ToolbarItemId, !item.IsEnabled, typeof(ToolbarStatusPage));
        }
    }

    private void ToggleToolbarItemVisible_Click(object sender, RoutedEventArgs e)
    {
        var item = GetToolbarItem();
        if (item is not null)
        {
            toolbar.SetItemVisible(ToolbarItemId, !item.IsVisible, typeof(ToolbarStatusPage));
        }
    }

    private void ToggleIconOnly_Click(object sender, RoutedEventArgs e)
    {
        var page = toolbar.Current.Pages.GetValueOrDefault(typeof(ToolbarStatusPage));
        if (page is not null)
        {
            toolbar.SetIconOnly(typeof(ToolbarStatusPage), !page.IconOnly);
        }
    }

    private void MoveToolbarItem_Click(object sender, RoutedEventArgs e)
    {
        var items = toolbar.Current.Pages.GetValueOrDefault(typeof(ToolbarStatusPage))?.Items;
        if (items is not null && items.Any(item => item.Id == ToolbarItemId))
        {
            var currentIndex = items.Select((item, index) => (item, index))
                .First(pair => pair.item.Id == ToolbarItemId)
                .index;
            toolbar.Move(
                ToolbarItemId,
                currentIndex == 0 ? items.Count - 1 : 0,
                typeof(ToolbarStatusPage)
            );
        }
    }

    private void RemoveToolbarItem_Click(object sender, RoutedEventArgs e)
    {
        toolbar.Remove(ToolbarItemId, typeof(ToolbarStatusPage));
        toolbar.Remove(CompanionToolbarItemId, typeof(ToolbarStatusPage));
    }

    private ValueTask<CommandResult> ExecuteToolbarCommandAsync(
        CommandContext context,
        CancellationToken cancellationToken
    )
    {
        cancellationToken.ThrowIfCancellationRequested();
        var message = $"Executed at {DateTimeOffset.Now:HH:mm:ss.fff} from {context.Source}.";
        if (Dispatcher.CheckAccess())
        {
            CommandLogText.Text = message;
        }
        else
        {
            Dispatcher.Invoke(() => CommandLogText.Text = message);
        }

        status.Upsert(new FlourishStatusItem(StatusItemId, message, "\uE930"));
        return ValueTask.FromResult(CommandResult.HandledWith(message));
    }

    private void UpsertStatus_Click(object sender, RoutedEventArgs e) =>
        Execute(
            () =>
            {
                status.SetEnabled(true);
                status.Upsert(
                    new FlourishStatusItem(StatusItemId, StatusTextBox.Text, "\uE946")
                );
            },
            StatusStateText
        );

    private void ShowTimedStatus_Click(object sender, RoutedEventArgs e) =>
        Execute(
            () =>
            {
                status.SetEnabled(true);
                status.Show(
                    "gallery.runtime.timed",
                    $"{StatusTextBox.Text} ({DateTimeOffset.Now:HH:mm:ss})",
                    "\uE823",
                    TimeSpan.FromSeconds(4)
                );
            },
            StatusStateText
        );

    private void ToggleStatusVisible_Click(object sender, RoutedEventArgs e)
    {
        var item = status.Current.Items.FirstOrDefault(candidate => candidate.Id == StatusItemId);
        if (item is not null)
        {
            status.SetItemVisible(StatusItemId, !item.IsVisible);
        }
    }

    private void RemoveStatus_Click(object sender, RoutedEventArgs e) => status.Remove(StatusItemId);

    private void MoveStatus_Click(object sender, RoutedEventArgs e)
    {
        var items = status.Current.Items;
        var currentIndex = items.Select((item, index) => (item, index))
            .FirstOrDefault(pair => pair.item.Id == StatusItemId)
            .index;
        if (items.Any(item => item.Id == StatusItemId))
        {
            status.Move(StatusItemId, currentIndex == 0 ? items.Count - 1 : 0);
        }
    }

    private void ToggleLan_Click(object sender, RoutedEventArgs e) =>
        status.SetLanStatusEnabled(!status.Current.IsLanStatusEnabled);

    private void TogglePower_Click(object sender, RoutedEventArgs e) =>
        status.SetPowerStatusEnabled(!status.Current.IsPowerStatusEnabled);

    private void ToggleStatusBar_Click(object sender, RoutedEventArgs e) =>
        status.SetEnabled(!status.Current.IsEnabled);

    private void AddRegion_Click(object sender, RoutedEventArgs e)
    {
        regions.Upsert(
            RegionId,
            FlourishRegion.ContentHeader,
            static _ => CreateRegionContent(),
            order: 50
        );
    }

    private void ToggleRegion_Click(object sender, RoutedEventArgs e)
    {
        var entry = regions.Current.Entries.FirstOrDefault(candidate => candidate.Id == RegionId);
        if (entry is not null)
        {
            regions.SetEnabled(RegionId, !entry.IsEnabled);
        }
    }

    private void RemoveRegion_Click(object sender, RoutedEventArgs e) => regions.Remove(RegionId);

    private void ReorderRegion_Click(object sender, RoutedEventArgs e)
    {
        var entry = regions.Current.Entries.FirstOrDefault(candidate => candidate.Id == RegionId);
        if (entry is not null)
        {
            regions.SetOrder(RegionId, entry.Order >= 90 ? 10 : 90);
        }
    }

    private static FrameworkElement CreateRegionContent()
    {
        var text = new FlourishTextBlock
        {
            Text = $"ContentHeader registered at {DateTimeOffset.Now:HH:mm:ss}",
            VerticalAlignment = VerticalAlignment.Center,
        };
        text.SetResourceReference(FlourishTextBlock.ForegroundProperty, "FlourishBrandForegroundBrush");

        var border = new Border
        {
            Margin = new Thickness(8, 4, 8, 4),
            Padding = new Thickness(12, 7, 12, 7),
            CornerRadius = new CornerRadius(7),
            Child = text,
        };
        border.SetResourceReference(Border.BackgroundProperty, "FlourishAccentSurfaceBrush");
        border.SetResourceReference(Border.BorderBrushProperty, "FlourishCardStrokeBrush");
        border.BorderThickness = new Thickness(1);
        return border;
    }

    private FlourishToolbarItem? GetToolbarItem() =>
        toolbar.Current.Pages.GetValueOrDefault(typeof(ToolbarStatusPage))?.Items
            .FirstOrDefault(item => item.Id == ToolbarItemId);

    private void RuntimeState_Changed(object? sender, EventArgs e) =>
        Dispatcher.BeginInvoke(RefreshState);

    private void Execute(Action action, FlourishTextBlock statusText)
    {
        try
        {
            action();
            RefreshState();
        }
        catch (Exception error)
        {
            statusText.Text = error.Message;
        }
    }

    private void RefreshState()
    {
        var pageToolbar = toolbar.Current.Pages.GetValueOrDefault(typeof(ToolbarStatusPage));
        var item = pageToolbar?.Items.FirstOrDefault(candidate => candidate.Id == ToolbarItemId);
        ToolbarStateText.Text = item is null
            ? "No runtime item is installed for this page."
            : $"Toolbar enabled: {toolbar.Current.IsEnabled}  |  Item enabled: {item.IsEnabled}  |  Visible: {item.IsVisible}  |  Icon only: {pageToolbar!.IconOnly}";

        var statusItem = status.Current.Items.FirstOrDefault(candidate => candidate.Id == StatusItemId);
        StatusStateText.Text =
            $"Bar enabled: {status.Current.IsEnabled}  |  LAN: {status.Current.IsLanStatusEnabled}  |  Power: {status.Current.IsPowerStatusEnabled}  |  Demo item: {(statusItem is null ? "missing" : statusItem.IsVisible ? "visible" : "hidden")}";

        var region = regions.Current.Entries.FirstOrDefault(candidate => candidate.Id == RegionId);
        RegionStateText.Text = region is null
            ? "No runtime ContentHeader registration."
            : $"Region: {region.Region}  |  Enabled: {region.IsEnabled}  |  Order: {region.Order}";
    }
}
