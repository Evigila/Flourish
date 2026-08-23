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

    private readonly INavigationPanelService panel;
    private readonly INavigationMenuService menu;
    private readonly INavigationRouteRegistry routes;
    private readonly INavigationService navigation;
    private readonly IPageCacheService cache;
    private bool isRefreshing;

    public NavigationRuntimePage(
        INavigationPanelService panel,
        INavigationMenuService menu,
        INavigationRouteRegistry routes,
        INavigationService navigation,
        IPageCacheService cache
    )
    {
        this.panel = panel;
        this.menu = menu;
        this.routes = routes;
        this.navigation = navigation;
        this.cache = cache;
        InitializeComponent();

        DirectionBox.ItemsSource = Enum.GetValues<NavigationPanelDirection>();
        Loaded += Page_Loaded;
        Unloaded += Page_Unloaded;
        RefreshState();
    }

    private void Page_Loaded(object sender, RoutedEventArgs e)
    {
        panel.Changed -= RuntimeState_Changed;
        menu.Changed -= RuntimeState_Changed;
        routes.Changed -= RuntimeState_Changed;
        cache.Changed -= RuntimeState_Changed;
        panel.Changed += RuntimeState_Changed;
        menu.Changed += RuntimeState_Changed;
        routes.Changed += RuntimeState_Changed;
        cache.Changed += RuntimeState_Changed;
        RefreshState();
    }

    private void Page_Unloaded(object sender, RoutedEventArgs e)
    {
        panel.Changed -= RuntimeState_Changed;
        menu.Changed -= RuntimeState_Changed;
        routes.Changed -= RuntimeState_Changed;
        cache.Changed -= RuntimeState_Changed;
    }

    private void TogglePanel_Click(object sender, RoutedEventArgs e)
    {
        panel.Toggle();
        PanelOutput.WriteLine(
            Localizer.Parse(
                Key.Runtime_NavigationPanel0_92C7D51F,
                Localizer.Parse(
                    panel.Current.IsOpen
                        ? Key.Runtime_Opened_50236627
                        : Key.Runtime_Closed_C3EEFB58
                )
            )
        );
    }

    private void TogglePanelEnabled_Click(object sender, RoutedEventArgs e)
    {
        panel.SetEnabled(!panel.Current.IsEnabled);
        PanelOutput.WriteLine(
            Localizer.Parse(
                Key.Runtime_NavigationPanel0_92C7D51F,
                Localizer.Parse(
                    panel.Current.IsEnabled
                        ? Key.Runtime_Enabled_FB9CF756
                        : Key.Runtime_Disabled_17EB3C01
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
            panel.SetDirection(direction);
            PanelOutput.WriteLine(
                Localizer.Parse(Key.Runtime_NavigationPanelMovedTo0_39B53359, direction)
            );
        }
    }

    private void ApplyWidths_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            panel.SetPanelWidth(
                Parse(OpenWidthBox.Text),
                Parse(ClosedWidthBox.Text),
                Parse(MaxWidthBox.Text),
                Parse(MinWidthBox.Text)
            );
            var state = panel.Current;
            PanelOutput.WriteLine(
                Localizer.Parse(
                    Key.Runtime_PanelWidthsSetToClosed00Open10Range2030_7AF1DFF9,
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
                Localizer.Parse(Key.Dynamic_Error0_43F78154, error.Message)
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
            routes.Set(
                new FlourishNavigationRoute(
                    RuntimeRouteKey,
                    typeof(RuntimeRoutePage),
                    FlourishPageCacheMode.Enabled,
                    static provider => new RuntimeRoutePage(
                        provider.GetRequiredService<INavigationService>()
                    )
                )
            );

            var hasGroup = menu.Current.Groups.Any(group => group.Id == RuntimeGroupId);
            menu.Set(editor =>
            {
                if (!hasGroup)
                {
                    editor.AppendGroup(
                        RuntimeGroupId,
                        Localizer.Parse(Key.Runtime_AddedAtRuntime_82975386)
                    );
                }

                editor.SetItem(
                    RuntimeGroupId,
                    FlourishNavigationMenuItem.Page(
                        RuntimeItemId,
                        RuntimeRouteKey,
                        Localizer.Parse(Key.Runtime_RuntimeRouteInstance_9BC2A49C),
                        "\uE8A7"
                    )
                );
            });
            RouteOutput.WriteLine(
                Localizer.Parse(Key.Runtime_InstalledTheDemoRouteAndNavigationItem_2DE1D70D)
            );
        }
        catch (Exception error)
        {
            RouteOutput.WriteLine(
                Localizer.Parse(Key.Dynamic_Error0_43F78154, error.Message)
            );
        }
    }

    private void NavigateRoute_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            navigation.Navigate(RuntimeRouteKey, DateTimeOffset.Now);
            RouteOutput.WriteLine(
                Localizer.Parse(Key.Runtime_NavigatedTo0_27A49119, RuntimeRouteKey)
            );
        }
        catch (Exception error)
        {
            RouteOutput.WriteLine(
                Localizer.Parse(Key.Dynamic_Error0_43F78154, error.Message)
            );
        }
    }

    private void ToggleMenuItem_Click(object sender, RoutedEventArgs e)
    {
        var item = menu
            .Current.Groups.SelectMany(group => group.Items)
            .FirstOrDefault(candidate => candidate.Id == RuntimeItemId);
        if (item is null)
        {
            RouteOutput.WriteLine(
                Localizer.Parse(Key.Runtime_InstallTheDemoRouteFirst_54C0B4AA)
            );
            return;
        }

        menu.Set(editor => editor.SetItemEnabled(RuntimeItemId, !item.IsEnabled));
        RouteOutput.WriteLine(
            Localizer.Parse(
                Key.Runtime_DemoNavigationItem0_4FBC3954,
                Localizer.Parse(
                    !item.IsEnabled
                        ? Key.Runtime_Enabled_FB9CF756
                        : Key.Runtime_Disabled_17EB3C01
                )
            )
        );
    }

    private void RemoveRoute_Click(object sender, RoutedEventArgs e)
    {
        menu.Set(editor =>
        {
            editor.RemoveItem(RuntimeItemId);
            if (menu.Current.Groups.Any(group => group.Id == RuntimeGroupId))
            {
                editor.RemoveGroup(RuntimeGroupId);
            }
        });
        var removed = routes.Remove(RuntimeRouteKey);
        RouteOutput.WriteLine(
            removed
                ? Localizer.Parse(Key.Runtime_RemovedTheDemoRouteAndNavigationItem_932415B1)
                : Localizer.Parse(Key.Runtime_TheDemoRouteWasAlreadyAbsent_0556D625)
        );
    }

    private void EnableCache_Click(object sender, RoutedEventArgs e) =>
        SetCacheMode(FlourishPageCacheMode.Enabled);

    private void DisableCache_Click(object sender, RoutedEventArgs e) =>
        SetCacheMode(FlourishPageCacheMode.Disabled);

    private void EvictCache_Click(object sender, RoutedEventArgs e)
    {
        CacheOutput.WriteLine(
            cache.Evict(typeof(RuntimeRoutePage))
                ? Localizer.Parse(Key.Runtime_EvictedTheCachedDemoPageInstance_2957A414)
                : Localizer.Parse(Key.Runtime_NoCachedDemoPageInstanceWasPresent_34354CBF)
        );
    }

    private void ClearCache_Click(object sender, RoutedEventArgs e)
    {
        cache.Clear();
        CacheOutput.WriteLine(
            Localizer.Parse(Key.Runtime_ClearedAllCachedPageInstances_7839F7BC)
        );
    }

    private void SetCacheMode(FlourishPageCacheMode mode)
    {
        try
        {
            if (routes.Get(RuntimeRouteKey) is null)
            {
                InstallRoute_Click(this, new RoutedEventArgs());
            }

            routes.SetCacheMode(RuntimeRouteKey, mode);
            cache.SetCacheMode(typeof(RuntimeRoutePage), mode);
            CacheOutput.WriteLine(
                Localizer.Parse(Key.Runtime_DemoPageCacheModeSetTo0_1348FE55, mode)
            );
        }
        catch (Exception error)
        {
            CacheOutput.WriteLine(
                Localizer.Parse(Key.Dynamic_Error0_43F78154, error.Message)
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
            var panelState = panel.Current;
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
