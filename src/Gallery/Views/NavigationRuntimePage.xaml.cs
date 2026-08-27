using System;
using System.Linq;

using CKey = ArkheideSystem.Essential.Culture.Key;
using Localizer = ArkheideSystem.Essential.Culture.Localizer;
using InputKey = System.Windows.Input.Key;
using ArkheideSystem.Flourish.Abstract;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Extensions.DependencyInjection;

namespace ArkheideSystem.Gallery.Views;

public partial class NavigationRuntimePage : Page
{
    private const string RuntimeGroupId = "runtime-gallery";
    private const string RuntimeItemId = "runtime-gallery.preview";
    private const string RuntimeRouteKey = "RuntimePreview";

    private readonly INavigationService navigation;
    private bool isRefreshing;

    public NavigationRuntimePage(
        INavigationService navigation
    )
    {
        this.navigation = navigation;
        InitializeComponent();

        DirectionBox.ItemsSource = Enum.GetValues<NavigationPanelDirection>();
        Loaded += Page_Loaded;
        Unloaded += Page_Unloaded;
        RefreshState();
    }

    private void Page_Loaded(object sender, RoutedEventArgs e)
    {
        navigation.Changed -= RuntimeState_Changed;
        navigation.Changed += RuntimeState_Changed;
        RefreshState();
    }

    private void Page_Unloaded(object sender, RoutedEventArgs e)
    {
        navigation.Changed -= RuntimeState_Changed;
    }

    private void TogglePanel_Click(object sender, RoutedEventArgs e)
    {
        navigation.Toggle();
        PanelOutput.WriteLine(
            Localizer.Parse(
                CKey.Runtime_NavigationPanel0_92C7D51F,
                Localizer.Parse(
                    navigation.Current.Panel.IsOpen
                        ? CKey.Runtime_Opened_50236627
                        : CKey.Runtime_Closed_C3EEFB58
                )
            )
        );
    }

    private void TogglePanelEnabled_Click(object sender, RoutedEventArgs e)
    {
        navigation.SetEnabled(!navigation.Current.Panel.IsEnabled);
        PanelOutput.WriteLine(
            Localizer.Parse(
                CKey.Runtime_NavigationPanel0_92C7D51F,
                Localizer.Parse(
                    navigation.Current.Panel.IsEnabled
                        ? CKey.Runtime_Enabled_FB9CF756
                        : CKey.Runtime_Disabled_17EB3C01
                )
            )
        );
    }

    private void DirectionBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (
            !isRefreshing
            && IsLoaded
            && DirectionBox.SelectedItem is NavigationPanelDirection direction
        )
        {
            navigation.SetDirection(direction);
            PanelOutput.WriteLine(
                Localizer.Parse(CKey.Runtime_NavigationPanelMovedTo0_39B53359, direction)
            );
        }
    }

    private void ApplyWidths_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            navigation.SetPanelWidth(
                Parse(OpenWidthBox.Text),
                Parse(ClosedWidthBox.Text),
                Parse(MaxWidthBox.Text),
                Parse(MinWidthBox.Text)
            );
            var state = navigation.Current.Panel;
            PanelOutput.WriteLine(
                Localizer.Parse(
                    CKey.Runtime_PanelWidthsSetToClosed00Open10Range2030_7AF1DFF9,
                    state.ClosedWidth,
                    state.OpenWidth,
                    state.MinWidth,
                    state.MaxWidth
                )
            );
        }
        catch (Exception error)
        {
            PanelOutput.WriteLine(
                Localizer.Parse(CKey.Dynamic_Error0_43F78154, error.Message)
            );
        }
    }

    private void WidthBox_LostFocus(object sender, RoutedEventArgs e) => CommitWidths();

    private void WidthBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key != InputKey.Enter)
        {
            return;
        }

        CommitWidths();
        e.Handled = true;
    }

    private void CommitWidths()
    {
        if (IsLoaded && !isRefreshing)
        {
            ApplyWidths_Click(this, new RoutedEventArgs());
        }
    }

    private void InstallRoute_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            navigation.SetNavigable(
                new NavigationRoute(
                    RuntimeRouteKey,
                    typeof(RuntimeRoutePage),
                    PageCacheMode.Enabled,
                    static provider => new RuntimeRoutePage(
                        provider.GetRequiredService<INavigationService>()
                    )
                )
            );

            var hasGroup = navigation.Current.Menu.Groups.Any(group => group.Id == RuntimeGroupId);
            navigation.SetMenu(editor =>
            {
                if (!hasGroup)
                {
                    editor.AddGroup(
                        RuntimeGroupId,
                        Localizer.Parse(CKey.Runtime_AddedAtRuntime_82975386)
                    );
                }

                editor.SetItem(
                    RuntimeGroupId,
                    NavigationMenuItem.Page(
                        RuntimeItemId,
                        RuntimeRouteKey,
                        Localizer.Parse(CKey.Runtime_RuntimeRouteInstance_9BC2A49C),
                        "\uE8A7"
                    )
                );
            });
            RouteOutput.WriteLine(
                Localizer.Parse(CKey.Runtime_InstalledTheDemoRouteAndNavigationItem_2DE1D70D)
            );
        }
        catch (Exception error)
        {
            RouteOutput.WriteLine(
                Localizer.Parse(CKey.Dynamic_Error0_43F78154, error.Message)
            );
        }
    }

    private void NavigateRoute_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            navigation.Navigate(RuntimeRouteKey, DateTimeOffset.Now);
            RouteOutput.WriteLine(
                Localizer.Parse(CKey.Runtime_NavigatedTo0_27A49119, RuntimeRouteKey)
            );
        }
        catch (Exception error)
        {
            RouteOutput.WriteLine(
                Localizer.Parse(CKey.Dynamic_Error0_43F78154, error.Message)
            );
        }
    }

    private void ToggleMenuItem_Click(object sender, RoutedEventArgs e)
    {
        var item = navigation
            .Current.Menu.Groups.SelectMany(group => group.Items)
            .FirstOrDefault(candidate => candidate.Id == RuntimeItemId);
        if (item is null)
        {
            RouteOutput.WriteLine(
                Localizer.Parse(CKey.Runtime_InstallTheDemoRouteFirst_54C0B4AA)
            );
            return;
        }

        navigation.SetMenu(editor => editor.SetItemEnabled(RuntimeItemId, !item.IsEnabled));
        RouteOutput.WriteLine(
            Localizer.Parse(
                CKey.Runtime_DemoNavigationItem0_4FBC3954,
                Localizer.Parse(
                    !item.IsEnabled
                        ? CKey.Runtime_Enabled_FB9CF756
                        : CKey.Runtime_Disabled_17EB3C01
                )
            )
        );
    }

    private void RemoveRoute_Click(object sender, RoutedEventArgs e)
    {
        navigation.SetMenu(editor =>
        {
            editor.RemoveItem(RuntimeItemId);
            if (navigation.Current.Menu.Groups.Any(group => group.Id == RuntimeGroupId))
            {
                editor.RemoveGroup(RuntimeGroupId);
            }
        });
        var removed = navigation.RemoveNavigable(RuntimeRouteKey);
        RouteOutput.WriteLine(
            removed
                ? Localizer.Parse(CKey.Runtime_RemovedTheDemoRouteAndNavigationItem_932415B1)
                : Localizer.Parse(CKey.Runtime_TheDemoRouteWasAlreadyAbsent_0556D625)
        );
    }

    private void EnableCache_Click(object sender, RoutedEventArgs e) =>
        SetCacheMode(PageCacheMode.Enabled);

    private void DisableCache_Click(object sender, RoutedEventArgs e) =>
        SetCacheMode(PageCacheMode.Disabled);

    private void EvictCache_Click(object sender, RoutedEventArgs e)
    {
        CacheOutput.WriteLine(
            navigation.Evict(typeof(RuntimeRoutePage))
                ? Localizer.Parse(CKey.Runtime_EvictedTheCachedDemoPageInstance_2957A414)
                : Localizer.Parse(CKey.Runtime_NoCachedDemoPageInstanceWasPresent_34354CBF)
        );
    }

    private void ClearCache_Click(object sender, RoutedEventArgs e)
    {
        navigation.ClearCache();
        CacheOutput.WriteLine(
            Localizer.Parse(CKey.Runtime_ClearedAllCachedPageInstances_7839F7BC)
        );
    }

    private void SetCacheMode(PageCacheMode mode)
    {
        try
        {
            if (navigation.GetNavigable(RuntimeRouteKey) is null)
            {
                InstallRoute_Click(this, new RoutedEventArgs());
            }

            navigation.SetCacheMode(typeof(RuntimeRoutePage), mode);
            CacheOutput.WriteLine(
                Localizer.Parse(CKey.Runtime_DemoPageCacheModeSetTo0_1348FE55, mode)
            );
        }
        catch (Exception error)
        {
            CacheOutput.WriteLine(
                Localizer.Parse(CKey.Dynamic_Error0_43F78154, error.Message)
            );
        }
    }

    private void RuntimeState_Changed(object? sender, EventArgs e) =>
        Dispatcher.BeginInvoke(RefreshState);

    private void RefreshState()
    {
        isRefreshing = true;
        try
        {
            var panelState = navigation.Current.Panel;
            DirectionBox.SelectedItem = panelState.Direction;
        }
        finally
        {
            isRefreshing = false;
        }
    }

    private static double Parse(string value) =>
        double.Parse(value, NumberStyles.Float, CultureInfo.InvariantCulture);
}
