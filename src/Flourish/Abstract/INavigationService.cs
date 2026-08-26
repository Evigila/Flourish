using System;
using System.Threading;
using System.Threading.Tasks;

using System.Windows.Controls;

namespace ArkheideSystem.Flourish.Abstract;

/// <summary>Provides the complete runtime façade for Flourish navigation.</summary>
public interface INavigationService
{
    /// <summary>Gets the current navigation, route, menu, panel, and cache state atomically.</summary>
    FlourishNavigationState Current { get; }

    /// <summary>Occurs after any runtime navigation state changes.</summary>
    event EventHandler<FlourishStateChangedEventArgs<FlourishNavigationState>>? Changed;

    /// <summary>Occurs after Flourish navigates to a registered page.</summary>
    event EventHandler<FlourishNavigatedEventArgs>? Navigated;

    /// <summary>Returns whether a route with the specified navigation key is registered.</summary>
    bool CanNavigate(string navigationKey);

    /// <summary>Navigates to a registered route.</summary>
    bool Navigate(string navigationKey, object? parameter = null, bool addToBackStack = true);

    /// <summary>Navigates to a registered page type.</summary>
    bool Navigate<TPage>(object? parameter = null, bool addToBackStack = true)
        where TPage : Page;

    /// <summary>Navigates asynchronously to a registered route.</summary>
    Task<bool> NavigateAsync(
        string navigationKey,
        object? parameter = null,
        bool addToBackStack = true,
        CancellationToken cancellationToken = default
    );

    /// <summary>Navigates to the previous history entry.</summary>
    bool GoBack();

    /// <summary>Navigates to the next history entry.</summary>
    bool GoForward();

    /// <summary>Clears all entries behind the current page.</summary>
    void ClearBackStack();

    /// <summary>Clears all entries ahead of the current page.</summary>
    void ClearForwardStack();

    /// <summary>Clears both navigation history stacks.</summary>
    void ClearHistory();

    /// <summary>Adds or replaces a runtime page route.</summary>
    IRegistration AddNavigable<TPage>(
        string? navigationKey = null,
        FlourishPageCacheMode cacheMode = FlourishPageCacheMode.Enabled
    )
        where TPage : Page;

    /// <summary>Adds or replaces a runtime route definition.</summary>
    IRegistration SetNavigable(FlourishNavigationRoute route);

    /// <summary>Removes a runtime route by navigation key.</summary>
    bool RemoveNavigable(string navigationKey);

    /// <summary>Gets a runtime route by navigation key.</summary>
    FlourishNavigationRoute? GetNavigable(string navigationKey);

    /// <summary>Applies a runtime navigation-menu transaction.</summary>
    void SetMenu(Action<INavigationMenuEditor> update);

    /// <summary>Enables or disables the complete navigation surface.</summary>
    void SetEnabled(bool enabled);

    /// <summary>Sets the navigation panel direction.</summary>
    void SetDirection(NavigationPanelDirection direction);

    /// <summary>Sets the open, closed, maximum, and minimum panel widths.</summary>
    void SetPanelWidth(double openWidth, double closedWidth, double maxWidth, double minWidth);

    /// <summary>Opens the navigation panel.</summary>
    void Open(bool animate = true);

    /// <summary>Closes the navigation panel.</summary>
    void Close(bool animate = true);

    /// <summary>Toggles the navigation panel.</summary>
    void Toggle(bool animate = true);

    /// <summary>Changes the cache mode used by a page type and its route.</summary>
    void SetCacheMode(Type pageType, FlourishPageCacheMode cacheMode);

    /// <summary>Evicts a page instance from the navigation cache.</summary>
    bool Evict(Type pageType);

    /// <summary>Clears every cached page instance.</summary>
    void ClearCache();

    /// <summary>Returns whether a page instance is currently cached.</summary>
    bool IsCached(Type pageType);
}
