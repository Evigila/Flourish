using CKey = Arkheide.Essential.Culture.Key;
using Localizer = Arkheide.Essential.Culture.Localizer;
using ArkheideSystem.Flourish.Abstract;
using ArkheideSystem.Flourish.Abstract.Essential;
using ArkheideSystem.Flourish.Abstract.Runtime;
using System.Windows;
using System.Windows.Controls;
using ArkheideSystem.Flourish.Controls;

namespace ArkheideSystem.Gallery.Views;

public partial class ToolbarStatusPage : Page
{
    private const string ToolbarItemId = "gallery.runtime.toolbar";
    private const string CompanionToolbarItemId = "gallery.runtime.toolbar.companion";
    private const string ToolbarCommandKey = "cmd_gallery_runtime_toolbar_execute";
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
                toolbar.SetItem(
                    new FlourishToolbarItem(
                        Localizer.Parse(CKey.Runtime_RunLiveCommand_7352E4E7),
                        "\uE768",
                        ToolbarCommandKey
                    )
                    {
                        Id = ToolbarItemId,
                    },
                    typeof(ToolbarStatusPage)
                );
                toolbar.SetItem(
                    new FlourishToolbarItem(
                        Localizer.Parse(CKey.Runtime_Companion_1DADB328),
                        "\uE8EF",
                        ToolbarCommandKey
                    )
                    {
                        Id = CompanionToolbarItemId,
                    },
                    typeof(ToolbarStatusPage)
                );
            },
            ToolbarOutput,
            Localizer.Parse(
                CKey.Runtime_AddedOrUpdatedTheTwoRuntimeToolbarActions_063BFB6C
            )
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
                Localizer.Parse(
                    CKey.Runtime_RuntimeToolbarAction0_ABFE414D,
                    Localizer.Parse(
                        enabled
                            ? CKey.Runtime_Enabled_FB9CF756
                            : CKey.Runtime_Disabled_17EB3C01
                    )
                )
            );
        }
        else
        {
            ToolbarOutput.WriteLine(
                Localizer.Parse(CKey.Runtime_AddTheRuntimeToolbarActionFirst_C243FC0B)
            );
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
                Localizer.Parse(
                    CKey.Runtime_RuntimeToolbarAction0_ABFE414D,
                    Localizer.Parse(
                        visible ? CKey.Runtime_Shown_BAAF5362 : CKey.Runtime_Hidden_E564B408
                    )
                )
            );
        }
        else
        {
            ToolbarOutput.WriteLine(
                Localizer.Parse(CKey.Runtime_AddTheRuntimeToolbarActionFirst_C243FC0B)
            );
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
                Localizer.Parse(
                    CKey.Runtime_ToolbarPresentationSetTo0_D717A96F,
                    Localizer.Parse(
                        iconOnly
                            ? CKey.Runtime_IconOnly_3B3B44D5
                            : CKey.Runtime_IconAndText_5C472518
                    )
                )
            );
        }
        else
        {
            ToolbarOutput.WriteLine(
                Localizer.Parse(CKey.Runtime_AddTheRuntimeToolbarActionFirst_C243FC0B)
            );
        }
    }

    private void MoveToolbarItem_Click(object sender, RoutedEventArgs e)
    {
        var items = toolbar.Current.Pages.GetValueOrDefault(typeof(ToolbarStatusPage))?.Items;
        if (items is not null && items.Any(item => item.Id == ToolbarItemId))
        {
            var currentIndex = items
                .Select((item, index) => (item, index))
                .First(pair => pair.item.Id == ToolbarItemId)
                .index;
            var targetIndex = currentIndex == 0 ? items.Count - 1 : 0;
            Execute(
                () => toolbar.SetOrder(ToolbarItemId, targetIndex, typeof(ToolbarStatusPage)),
                ToolbarOutput,
                Localizer.Parse(
                    CKey.Runtime_MovedTheRuntimeToolbarActionToIndex0_375DFF7C,
                    targetIndex
                )
            );
        }
        else
        {
            ToolbarOutput.WriteLine(
                Localizer.Parse(CKey.Runtime_AddTheRuntimeToolbarActionFirst_C243FC0B)
            );
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
            Localizer.Parse(CKey.Runtime_RemovedTheRuntimeToolbarActions_DC2790AE)
        );
    }

    private ValueTask<CommandResult> ExecuteToolbarCommandAsync(
        CommandContext context,
        CancellationToken cancellationToken
    )
    {
        cancellationToken.ThrowIfCancellationRequested();
        var message = Localizer.Parse(
            CKey.Runtime_ExecutedAt0HHMmSsFffFrom1_C1532414,
            DateTimeOffset.Now,
            context.Source
        );
        if (Dispatcher.CheckAccess())
        {
            ToolbarOutput.WriteLine(message);
        }
        else
        {
            Dispatcher.Invoke(() => ToolbarOutput.WriteLine(message));
        }

        status.SetItem(new FlourishStatusItem(StatusItemId, message, "\uE930"));
        return ValueTask.FromResult(CommandResult.HandledWith(message));
    }

    private void UpsertStatus_Click(object sender, RoutedEventArgs e) =>
        Execute(
            () =>
            {
                status.SetEnabled(true);
                status.SetItem(new FlourishStatusItem(StatusItemId, StatusTextBox.Text, "\uE946"));
            },
            StatusOutput,
            Localizer.Parse(CKey.Runtime_AddedOrUpdatedThePersistentStatusItem_D8C174F7)
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
            Localizer.Parse(
                CKey.Runtime_DisplayedTheTimedStatusItemForFourSeconds_C5514AE4
            )
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
                Localizer.Parse(
                    CKey.Runtime_PersistentStatusItem0_66B63E53,
                    Localizer.Parse(
                        visible ? CKey.Runtime_Shown_BAAF5362 : CKey.Runtime_Hidden_E564B408
                    )
                )
            );
        }
        else
        {
            StatusOutput.WriteLine(
                Localizer.Parse(CKey.Runtime_AddThePersistentStatusItemFirst_ED79CDE9)
            );
        }
    }

    private void RemoveStatus_Click(object sender, RoutedEventArgs e) =>
        Execute(
            () => status.Remove(StatusItemId),
            StatusOutput,
            Localizer.Parse(CKey.Runtime_RemovedThePersistentStatusItem_02776B4A)
        );

    private void MoveStatus_Click(object sender, RoutedEventArgs e)
    {
        var items = status.Current.Items;
        var currentIndex = items
            .Select((item, index) => (item, index))
            .FirstOrDefault(pair => pair.item.Id == StatusItemId)
            .index;
        if (items.Any(item => item.Id == StatusItemId))
        {
            var targetIndex = currentIndex == 0 ? items.Count - 1 : 0;
            Execute(
                () => status.SetOrder(StatusItemId, targetIndex),
                StatusOutput,
                Localizer.Parse(
                    CKey.Runtime_MovedThePersistentStatusItemToIndex0_333774DF,
                    targetIndex
                )
            );
        }
        else
        {
            StatusOutput.WriteLine(
                Localizer.Parse(CKey.Runtime_AddThePersistentStatusItemFirst_ED79CDE9)
            );
        }
    }

    private void ToggleLan_Click(object sender, RoutedEventArgs e)
    {
        var enabled = !status.Current.IsLanStatusEnabled;
        Execute(
            () => status.SetLanStatusEnabled(enabled),
            StatusOutput,
            Localizer.Parse(
                CKey.Runtime_LANIndicator0_C793788E,
                Localizer.Parse(
                    enabled ? CKey.Runtime_Enabled_FB9CF756 : CKey.Runtime_Disabled_17EB3C01
                )
            )
        );
    }

    private void TogglePower_Click(object sender, RoutedEventArgs e)
    {
        var enabled = !status.Current.IsPowerStatusEnabled;
        Execute(
            () => status.SetPowerStatusEnabled(enabled),
            StatusOutput,
            Localizer.Parse(
                CKey.Runtime_PowerIndicator0_8A0236F4,
                Localizer.Parse(
                    enabled ? CKey.Runtime_Enabled_FB9CF756 : CKey.Runtime_Disabled_17EB3C01
                )
            )
        );
    }

    private void ToggleStatusBar_Click(object sender, RoutedEventArgs e)
    {
        var enabled = !status.Current.IsEnabled;
        Execute(
            () => status.SetEnabled(enabled),
            StatusOutput,
            Localizer.Parse(
                CKey.Runtime_StatusBar0_EB094B42,
                Localizer.Parse(
                    enabled ? CKey.Runtime_Enabled_FB9CF756 : CKey.Runtime_Disabled_17EB3C01
                )
            )
        );
    }

    private void AddRegion_Click(object sender, RoutedEventArgs e)
    {
        Execute(
            () =>
                regions.Set(
                    RegionId,
                    FlourishRegion.ContentHeader,
                    _ => CreateRegionContent(),
                    order: 50
                ),
            RegionOutput,
            Localizer.Parse(
                CKey.Runtime_AddedOrUpdatedTheContentHeaderRegionAtOrder50_57F220FA
            )
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
                Localizer.Parse(
                    CKey.Runtime_ContentHeaderRegion0_CE212BA6,
                    Localizer.Parse(
                        enabled
                            ? CKey.Runtime_Enabled_FB9CF756
                            : CKey.Runtime_Disabled_17EB3C01
                    )
                )
            );
        }
        else
        {
            RegionOutput.WriteLine(
                Localizer.Parse(CKey.Runtime_AddTheContentHeaderRegionFirst_AF6B9E4C)
            );
        }
    }

    private void RemoveRegion_Click(object sender, RoutedEventArgs e) =>
        Execute(
            () => regions.Remove(RegionId),
            RegionOutput,
            Localizer.Parse(CKey.Runtime_RemovedTheContentHeaderRegion_03EAF38D)
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
                Localizer.Parse(
                    CKey.Runtime_MovedTheContentHeaderRegionToOrder0_C3C9AC0D,
                    order
                )
            );
        }
        else
        {
            RegionOutput.WriteLine(
                Localizer.Parse(CKey.Runtime_AddTheContentHeaderRegionFirst_AF6B9E4C)
            );
        }
    }

    private FrameworkElement CreateRegionContent()
    {
        var text = new FlourishTextBlock
        {
            Text = Localizer.Parse(
                CKey.Runtime_ContentHeaderRegisteredAt0HHMmSs_52D12698,
                DateTimeOffset.Now
            ),
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
        toolbar
            .Current.Pages.GetValueOrDefault(typeof(ToolbarStatusPage))
            ?.Items.FirstOrDefault(item => item.Id == ToolbarItemId);

    private void Execute(Action action, OutputCard output, string successMessage)
    {
        try
        {
            action();
            output.WriteLine(successMessage);
        }
        catch (Exception error)
        {
            output.WriteLine(
                Localizer.Parse(CKey.Dynamic_Error0_43F78154, error.Message)
            );
        }
    }
}
