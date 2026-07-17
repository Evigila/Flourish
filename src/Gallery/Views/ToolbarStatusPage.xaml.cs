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
    }

    private void Page_Unloaded(object sender, RoutedEventArgs e)
    {
        commandRegistration?.Dispose();
        commandRegistration = null;
    }

    private void AddToolbarItem_Click(object sender, RoutedEventArgs e)
    {
        Execute(
            () =>
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
            },
            ToolbarOutput,
            "Added or updated the two runtime toolbar actions."
        );
    }

    private void ToggleToolbarItemEnabled_Click(object sender, RoutedEventArgs e)
    {
        var item = GetToolbarItem();
        if (item is not null)
        {
            var enabled = !item.IsEnabled;
            Execute(
                () => toolbar.SetItemEnabled(ToolbarItemId, enabled, typeof(ToolbarStatusPage)),
                ToolbarOutput,
                $"Runtime toolbar action {(enabled ? "enabled" : "disabled")}."
            );
        }
        else
        {
            ToolbarOutput.WriteLine("Add the runtime toolbar action first.");
        }
    }

    private void ToggleToolbarItemVisible_Click(object sender, RoutedEventArgs e)
    {
        var item = GetToolbarItem();
        if (item is not null)
        {
            var visible = !item.IsVisible;
            Execute(
                () => toolbar.SetItemVisible(ToolbarItemId, visible, typeof(ToolbarStatusPage)),
                ToolbarOutput,
                $"Runtime toolbar action {(visible ? "shown" : "hidden")}."
            );
        }
        else
        {
            ToolbarOutput.WriteLine("Add the runtime toolbar action first.");
        }
    }

    private void ToggleIconOnly_Click(object sender, RoutedEventArgs e)
    {
        var page = toolbar.Current.Pages.GetValueOrDefault(typeof(ToolbarStatusPage));
        if (page is not null)
        {
            var iconOnly = !page.IconOnly;
            Execute(
                () => toolbar.SetIconOnly(typeof(ToolbarStatusPage), iconOnly),
                ToolbarOutput,
                $"Toolbar presentation set to {(iconOnly ? "icon only" : "icon and text")}."
            );
        }
        else
        {
            ToolbarOutput.WriteLine("Add the runtime toolbar action first.");
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
            var targetIndex = currentIndex == 0 ? items.Count - 1 : 0;
            Execute(
                () => toolbar.Move(
                    ToolbarItemId,
                    targetIndex,
                    typeof(ToolbarStatusPage)
                ),
                ToolbarOutput,
                $"Moved the runtime toolbar action to index {targetIndex}."
            );
        }
        else
        {
            ToolbarOutput.WriteLine("Add the runtime toolbar action first.");
        }
    }

    private void RemoveToolbarItem_Click(object sender, RoutedEventArgs e)
    {
        Execute(
            () =>
            {
                toolbar.Remove(ToolbarItemId, typeof(ToolbarStatusPage));
                toolbar.Remove(CompanionToolbarItemId, typeof(ToolbarStatusPage));
            },
            ToolbarOutput,
            "Removed the runtime toolbar actions."
        );
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
            ToolbarOutput.WriteLine(message);
        }
        else
        {
            Dispatcher.Invoke(() => ToolbarOutput.WriteLine(message));
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
            StatusOutput,
            "Added or updated the persistent status item."
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
            StatusOutput,
            "Displayed the timed status item for four seconds."
        );

    private void ToggleStatusVisible_Click(object sender, RoutedEventArgs e)
    {
        var item = status.Current.Items.FirstOrDefault(candidate => candidate.Id == StatusItemId);
        if (item is not null)
        {
            var visible = !item.IsVisible;
            Execute(
                () => status.SetItemVisible(StatusItemId, visible),
                StatusOutput,
                $"Persistent status item {(visible ? "shown" : "hidden")}."
            );
        }
        else
        {
            StatusOutput.WriteLine("Add the persistent status item first.");
        }
    }

    private void RemoveStatus_Click(object sender, RoutedEventArgs e) =>
        Execute(
            () => status.Remove(StatusItemId),
            StatusOutput,
            "Removed the persistent status item."
        );

    private void MoveStatus_Click(object sender, RoutedEventArgs e)
    {
        var items = status.Current.Items;
        var currentIndex = items.Select((item, index) => (item, index))
            .FirstOrDefault(pair => pair.item.Id == StatusItemId)
            .index;
        if (items.Any(item => item.Id == StatusItemId))
        {
            var targetIndex = currentIndex == 0 ? items.Count - 1 : 0;
            Execute(
                () => status.Move(StatusItemId, targetIndex),
                StatusOutput,
                $"Moved the persistent status item to index {targetIndex}."
            );
        }
        else
        {
            StatusOutput.WriteLine("Add the persistent status item first.");
        }
    }

    private void ToggleLan_Click(object sender, RoutedEventArgs e)
    {
        var enabled = !status.Current.IsLanStatusEnabled;
        Execute(
            () => status.SetLanStatusEnabled(enabled),
            StatusOutput,
            $"LAN indicator {(enabled ? "enabled" : "disabled")}."
        );
    }

    private void TogglePower_Click(object sender, RoutedEventArgs e)
    {
        var enabled = !status.Current.IsPowerStatusEnabled;
        Execute(
            () => status.SetPowerStatusEnabled(enabled),
            StatusOutput,
            $"Power indicator {(enabled ? "enabled" : "disabled")}."
        );
    }

    private void ToggleStatusBar_Click(object sender, RoutedEventArgs e)
    {
        var enabled = !status.Current.IsEnabled;
        Execute(
            () => status.SetEnabled(enabled),
            StatusOutput,
            $"Status bar {(enabled ? "enabled" : "disabled")}."
        );
    }

    private void AddRegion_Click(object sender, RoutedEventArgs e)
    {
        Execute(
            () => regions.Upsert(
                RegionId,
                FlourishRegion.ContentHeader,
                static _ => CreateRegionContent(),
                order: 50
            ),
            RegionOutput,
            "Added or updated the ContentHeader region at order 50."
        );
    }

    private void ToggleRegion_Click(object sender, RoutedEventArgs e)
    {
        var entry = regions.Current.Entries.FirstOrDefault(candidate => candidate.Id == RegionId);
        if (entry is not null)
        {
            var enabled = !entry.IsEnabled;
            Execute(
                () => regions.SetEnabled(RegionId, enabled),
                RegionOutput,
                $"ContentHeader region {(enabled ? "enabled" : "disabled")}."
            );
        }
        else
        {
            RegionOutput.WriteLine("Add the ContentHeader region first.");
        }
    }

    private void RemoveRegion_Click(object sender, RoutedEventArgs e) =>
        Execute(
            () => regions.Remove(RegionId),
            RegionOutput,
            "Removed the ContentHeader region."
        );

    private void ReorderRegion_Click(object sender, RoutedEventArgs e)
    {
        var entry = regions.Current.Entries.FirstOrDefault(candidate => candidate.Id == RegionId);
        if (entry is not null)
        {
            var order = entry.Order >= 90 ? 10 : 90;
            Execute(
                () => regions.SetOrder(RegionId, order),
                RegionOutput,
                $"Moved the ContentHeader region to order {order}."
            );
        }
        else
        {
            RegionOutput.WriteLine("Add the ContentHeader region first.");
        }
    }

    private static FrameworkElement CreateRegionContent()
    {
        var text = new FlourishTextBlock
        {
            Text = $"ContentHeader registered at {DateTimeOffset.Now:HH:mm:ss}",
            VerticalAlignment = VerticalAlignment.Center,
        };
        text.SetResourceReference(
            FlourishTextBlock.ForegroundProperty,
            "FlourishAccentForegroundBrush"
        );

        var border = new Border
        {
            Margin = new Thickness(8, 4, 8, 4),
            Padding = new Thickness(12, 7, 12, 7),
            Child = text,
        };
        border.SetResourceReference(Border.BackgroundProperty, "FlourishAccentSurfaceBrush");
        border.SetResourceReference(Border.BorderBrushProperty, "FlourishSurfaceStrokeBrush");
        border.SetResourceReference(
            Border.BorderThicknessProperty,
            "FlourishSurfaceBorderThickness"
        );
        border.SetResourceReference(Border.CornerRadiusProperty, "FlourishSurfaceCornerRadius");
        return border;
    }

    private FlourishToolbarItem? GetToolbarItem() =>
        toolbar.Current.Pages.GetValueOrDefault(typeof(ToolbarStatusPage))?.Items
            .FirstOrDefault(item => item.Id == ToolbarItemId);

    private void Execute(Action action, OutputCard output, string successMessage)
    {
        try
        {
            action();
            output.WriteLine(successMessage);
        }
        catch (Exception error)
        {
            output.WriteLine($"Error: {error.Message}");
        }
    }
}
