using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ArkheideSystem.Flourish.Abstract;
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
        PanelOutput.WriteLine($"Navigation panel {(panel.Current.IsOpen ? "opened" : "closed")}.");
    }

    private void TogglePanelEnabled_Click(object sender, RoutedEventArgs e)
    {
        panel.SetEnabled(!panel.Current.IsEnabled);
        PanelOutput.WriteLine($"Navigation panel {(panel.Current.IsEnabled ? "enabled" : "disabled")}.");
    }

    private void DirectionBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (!isRefreshing
            && IsLoaded
            && DirectionBox.SelectedItem is NavigationPanelDirection direction)
        {
            panel.SetDirection(direction);
            PanelOutput.WriteLine($"Navigation panel moved to {direction}.");
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
                $"Panel widths set to closed {state.ClosedWidth:0}, open {state.OpenWidth:0}, range {state.MinWidth:0}-{state.MaxWidth:0}."
            );
        }
        catch (Exception error)
        {
            PanelOutput.WriteLine($"Error: {error.Message}");
        }
    }

    private void WidthBox_LostFocus(object sender, RoutedEventArgs e) => CommitWidths();

    private void WidthBox_KeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key != Key.Enter)
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
            routes.Upsert(
                new FlourishNavigationRoute(
                    RuntimeRouteKey,
                    typeof(RuntimeRoutePage),
                    FlourishPageCacheMode.Enabled,
                    static provider =>
                        new RuntimeRoutePage(
                            provider.GetRequiredService<INavigationService>()
                        )
                )
            );

            var hasGroup = menu.Current.Groups.Any(group => group.Id == RuntimeGroupId);
            menu.Update(editor =>
            {
                if (!hasGroup)
                {
                    editor.AddGroup(RuntimeGroupId, "Added at runtime");
                }

                editor.UpsertItem(
                    RuntimeGroupId,
                    FlourishNavigationMenuItem.Page(
                        RuntimeItemId,
                        RuntimeRouteKey,
                        "Runtime route instance",
                        "\uE8A7"
                    )
                );
            });
            RouteOutput.WriteLine("Installed the demo route and navigation item.");
        }
        catch (Exception error)
        {
            RouteOutput.WriteLine($"Error: {error.Message}");
        }
    }

    private void NavigateRoute_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            navigation.Navigate(RuntimeRouteKey, DateTimeOffset.Now);
            RouteOutput.WriteLine($"Navigated to '{RuntimeRouteKey}'.");
        }
        catch (Exception error)
        {
            RouteOutput.WriteLine($"Error: {error.Message}");
        }
    }

    private void ToggleMenuItem_Click(object sender, RoutedEventArgs e)
    {
        var item = menu.Current.Groups.SelectMany(group => group.Items)
            .FirstOrDefault(candidate => candidate.Id == RuntimeItemId);
        if (item is null)
        {
            RouteOutput.WriteLine("Install the demo route first.");
            return;
        }

        menu.Update(editor => editor.SetItemEnabled(RuntimeItemId, !item.IsEnabled));
        RouteOutput.WriteLine($"Demo navigation item {(!item.IsEnabled ? "enabled" : "disabled")}.");
    }

    private void RemoveRoute_Click(object sender, RoutedEventArgs e)
    {
        menu.Update(editor =>
        {
            editor.RemoveItem(RuntimeItemId);
            if (menu.Current.Groups.Any(group => group.Id == RuntimeGroupId))
            {
                editor.RemoveGroup(RuntimeGroupId);
            }
        });
        var removed = routes.Remove(RuntimeRouteKey);
        RouteOutput.WriteLine(
            removed ? "Removed the demo route and navigation item." : "The demo route was already absent."
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
                ? "Evicted the cached demo page instance."
                : "No cached demo page instance was present."
        );
    }

    private void ClearCache_Click(object sender, RoutedEventArgs e)
    {
        cache.Clear();
        CacheOutput.WriteLine("Cleared all cached page instances.");
    }

    private void SetCacheMode(FlourishPageCacheMode mode)
    {
        try
        {
            if (!routes.Contains(RuntimeRouteKey))
            {
                InstallRoute_Click(this, new RoutedEventArgs());
            }

            routes.SetCacheMode(RuntimeRouteKey, mode);
            cache.SetCacheMode(typeof(RuntimeRoutePage), mode);
            CacheOutput.WriteLine($"Demo page cache mode set to {mode}.");
        }
        catch (Exception error)
        {
            CacheOutput.WriteLine($"Error: {error.Message}");
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
