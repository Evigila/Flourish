using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ArkheideSystem.Flourish.Abstract;
using ArkheideSystem.Flourish.Controls;
using ArkheideSystem.Gallery.Localization;

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
    private readonly IGalleryLocalization galleryLocalization;
    private ICommandRegistration? commandRegistration;

    public ToolbarStatusPage(
        IToolbarService toolbar,
        IStatusBarService status,
        IShellRegionService regions,
        ICommandRegistry commands,
        IGalleryLocalization galleryLocalization
    )
    {
        this.toolbar = toolbar;
        this.status = status;
        this.regions = regions;
        this.commands = commands;
        this.galleryLocalization = galleryLocalization;
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
                        galleryLocalization.Get(GalleryLocaleKeys.RuntimeRunLiveCommand_7352E4E7),
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
                        galleryLocalization.Get(GalleryLocaleKeys.RuntimeCompanion_1DADB328),
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
            galleryLocalization.Get(
                GalleryLocaleKeys.RuntimeAddedOrUpdatedTheTwoRuntimeToolbarActions_063BFB6C
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
                galleryLocalization.Format(
                    GalleryLocaleKeys.RuntimeRuntimeToolbarAction0_ABFE414D,
                    galleryLocalization.Get(
                        enabled
                            ? GalleryLocaleKeys.RuntimeEnabled_FB9CF756
                            : GalleryLocaleKeys.RuntimeDisabled_17EB3C01
                    )
                )
            );
        }
        else
        {
            ToolbarOutput.WriteLine(
                galleryLocalization.Get(
                    GalleryLocaleKeys.RuntimeAddTheRuntimeToolbarActionFirst_C243FC0B
                )
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
                galleryLocalization.Format(
                    GalleryLocaleKeys.RuntimeRuntimeToolbarAction0_ABFE414D,
                    galleryLocalization.Get(
                        visible
                            ? GalleryLocaleKeys.RuntimeShown_BAAF5362
                            : GalleryLocaleKeys.RuntimeHidden_E564B408
                    )
                )
            );
        }
        else
        {
            ToolbarOutput.WriteLine(
                galleryLocalization.Get(
                    GalleryLocaleKeys.RuntimeAddTheRuntimeToolbarActionFirst_C243FC0B
                )
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
                galleryLocalization.Format(
                    GalleryLocaleKeys.RuntimeToolbarPresentationSetTo0_D717A96F,
                    galleryLocalization.Get(
                        iconOnly
                            ? GalleryLocaleKeys.RuntimeIconOnly_3B3B44D5
                            : GalleryLocaleKeys.RuntimeIconAndText_5C472518
                    )
                )
            );
        }
        else
        {
            ToolbarOutput.WriteLine(
                galleryLocalization.Get(
                    GalleryLocaleKeys.RuntimeAddTheRuntimeToolbarActionFirst_C243FC0B
                )
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
                galleryLocalization.Format(
                    GalleryLocaleKeys.RuntimeMovedTheRuntimeToolbarActionToIndex0_375DFF7C,
                    targetIndex
                )
            );
        }
        else
        {
            ToolbarOutput.WriteLine(
                galleryLocalization.Get(
                    GalleryLocaleKeys.RuntimeAddTheRuntimeToolbarActionFirst_C243FC0B
                )
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
            galleryLocalization.Get(
                GalleryLocaleKeys.RuntimeRemovedTheRuntimeToolbarActions_DC2790AE
            )
        );
    }

    private ValueTask<CommandResult> ExecuteToolbarCommandAsync(
        CommandContext context,
        CancellationToken cancellationToken
    )
    {
        cancellationToken.ThrowIfCancellationRequested();
        var message = galleryLocalization.Format(
            GalleryLocaleKeys.RuntimeExecutedAt0HHMmSsFffFrom1_C1532414,
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
            galleryLocalization.Get(
                GalleryLocaleKeys.RuntimeAddedOrUpdatedThePersistentStatusItem_D8C174F7
            )
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
            galleryLocalization.Get(
                GalleryLocaleKeys.RuntimeDisplayedTheTimedStatusItemForFourSeconds_C5514AE4
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
                galleryLocalization.Format(
                    GalleryLocaleKeys.RuntimePersistentStatusItem0_66B63E53,
                    galleryLocalization.Get(
                        visible
                            ? GalleryLocaleKeys.RuntimeShown_BAAF5362
                            : GalleryLocaleKeys.RuntimeHidden_E564B408
                    )
                )
            );
        }
        else
        {
            StatusOutput.WriteLine(
                galleryLocalization.Get(
                    GalleryLocaleKeys.RuntimeAddThePersistentStatusItemFirst_ED79CDE9
                )
            );
        }
    }

    private void RemoveStatus_Click(object sender, RoutedEventArgs e) =>
        Execute(
            () => status.Remove(StatusItemId),
            StatusOutput,
            galleryLocalization.Get(
                GalleryLocaleKeys.RuntimeRemovedThePersistentStatusItem_02776B4A
            )
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
                galleryLocalization.Format(
                    GalleryLocaleKeys.RuntimeMovedThePersistentStatusItemToIndex0_333774DF,
                    targetIndex
                )
            );
        }
        else
        {
            StatusOutput.WriteLine(
                galleryLocalization.Get(
                    GalleryLocaleKeys.RuntimeAddThePersistentStatusItemFirst_ED79CDE9
                )
            );
        }
    }

    private void ToggleLan_Click(object sender, RoutedEventArgs e)
    {
        var enabled = !status.Current.IsLanStatusEnabled;
        Execute(
            () => status.SetLanStatusEnabled(enabled),
            StatusOutput,
            galleryLocalization.Format(
                GalleryLocaleKeys.RuntimeLANIndicator0_C793788E,
                galleryLocalization.Get(
                    enabled
                        ? GalleryLocaleKeys.RuntimeEnabled_FB9CF756
                        : GalleryLocaleKeys.RuntimeDisabled_17EB3C01
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
            galleryLocalization.Format(
                GalleryLocaleKeys.RuntimePowerIndicator0_8A0236F4,
                galleryLocalization.Get(
                    enabled
                        ? GalleryLocaleKeys.RuntimeEnabled_FB9CF756
                        : GalleryLocaleKeys.RuntimeDisabled_17EB3C01
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
            galleryLocalization.Format(
                GalleryLocaleKeys.RuntimeStatusBar0_EB094B42,
                galleryLocalization.Get(
                    enabled
                        ? GalleryLocaleKeys.RuntimeEnabled_FB9CF756
                        : GalleryLocaleKeys.RuntimeDisabled_17EB3C01
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
            galleryLocalization.Get(
                GalleryLocaleKeys.RuntimeAddedOrUpdatedTheContentHeaderRegionAtOrder50_57F220FA
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
                galleryLocalization.Format(
                    GalleryLocaleKeys.RuntimeContentHeaderRegion0_CE212BA6,
                    galleryLocalization.Get(
                        enabled
                            ? GalleryLocaleKeys.RuntimeEnabled_FB9CF756
                            : GalleryLocaleKeys.RuntimeDisabled_17EB3C01
                    )
                )
            );
        }
        else
        {
            RegionOutput.WriteLine(
                galleryLocalization.Get(
                    GalleryLocaleKeys.RuntimeAddTheContentHeaderRegionFirst_AF6B9E4C
                )
            );
        }
    }

    private void RemoveRegion_Click(object sender, RoutedEventArgs e) =>
        Execute(
            () => regions.Remove(RegionId),
            RegionOutput,
            galleryLocalization.Get(GalleryLocaleKeys.RuntimeRemovedTheContentHeaderRegion_03EAF38D)
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
                galleryLocalization.Format(
                    GalleryLocaleKeys.RuntimeMovedTheContentHeaderRegionToOrder0_C3C9AC0D,
                    order
                )
            );
        }
        else
        {
            RegionOutput.WriteLine(
                galleryLocalization.Get(
                    GalleryLocaleKeys.RuntimeAddTheContentHeaderRegionFirst_AF6B9E4C
                )
            );
        }
    }

    private FrameworkElement CreateRegionContent()
    {
        var text = new FlourishTextBlock
        {
            Text = galleryLocalization.Format(
                GalleryLocaleKeys.RuntimeContentHeaderRegisteredAt0HHMmSs_52D12698,
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
                galleryLocalization.Format(GalleryLocaleKeys.DynamicError0_43F78154, error.Message)
            );
        }
    }
}
