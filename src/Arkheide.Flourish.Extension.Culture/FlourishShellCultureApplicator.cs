using System.Windows;
using System.Windows.Threading;

namespace ArkheideSystem.Flourish.Extension.Culture;

/// <summary>Localizes stable Essential Culture tokens stored in Flourish shell state.</summary>
internal sealed class FlourishShellCultureApplicator(
    INavigationMenuService navigationMenu,
    ITitleBarService titleBar,
    ITitleBarSearchService search,
    IToolbarService toolbar,
    IStatusBarService statusBar
) : IDisposable
{
    private readonly Dictionary<string, string> groupTitleKeys = new(StringComparer.Ordinal);
    private readonly Dictionary<string, string> navigationLabelKeys = new(StringComparer.Ordinal);
    private readonly Dictionary<string, string> defaultToolbarLabelKeys = new(StringComparer.Ordinal);
    private readonly Dictionary<(Type PageType, string ItemId), string> pageToolbarLabelKeys = [];
    private readonly Dictionary<string, string> statusItemTextKeys = new(StringComparer.Ordinal);
    private Dispatcher? dispatcher;
    private string? applicationTitleKey;
    private string? applicationSubTitleKey;
    private string? unnamedProjectPlaceholderKey;
    private string? logoFallbackTextKey;
    private string? searchPlaceholderKey;
    private bool isStarted;
    private bool isApplying;

    internal void Start(Dispatcher dispatcher)
    {
        ArgumentNullException.ThrowIfNull(dispatcher);
        if (isStarted)
        {
            if (!ReferenceEquals(this.dispatcher, dispatcher))
            {
                throw new InvalidOperationException(
                    "Essential Culture is already connected to another dispatcher."
                );
            }

            return;
        }

        this.dispatcher = dispatcher;
        isStarted = true;
        Localizer.Current.Changed += Localizer_Changed;
        navigationMenu.Changed += ShellState_Changed;
        titleBar.Changed += ShellState_Changed;
        search.StateChanged += ShellState_Changed;
        toolbar.Changed += ShellState_Changed;
        statusBar.Changed += ShellState_Changed;
        try
        {
            RefreshOnDispatcher(captureTokens: true);
        }
        catch
        {
            Stop();
            throw;
        }
    }

    internal void Stop()
    {
        if (!isStarted)
        {
            return;
        }

        isStarted = false;
        dispatcher = null;
        Localizer.Current.Changed -= Localizer_Changed;
        navigationMenu.Changed -= ShellState_Changed;
        titleBar.Changed -= ShellState_Changed;
        search.StateChanged -= ShellState_Changed;
        toolbar.Changed -= ShellState_Changed;
        statusBar.Changed -= ShellState_Changed;
        ClearCapturedTokens();
    }

    public void Dispose() => Stop();

    private void Localizer_Changed(object? sender, EventArgs e) =>
        RefreshOnDispatcher(captureTokens: false);

    private void ShellState_Changed(object? sender, EventArgs e)
    {
        if (!isApplying)
        {
            RefreshOnDispatcher(captureTokens: true);
        }
    }

    private void RefreshOnDispatcher(bool captureTokens)
    {
        var targetDispatcher = Application.Current?.Dispatcher ?? dispatcher;
        if (
            targetDispatcher is null
            || targetDispatcher.HasShutdownStarted
            || targetDispatcher.HasShutdownFinished
        )
        {
            return;
        }

        void Refresh()
        {
            if (!isStarted)
            {
                return;
            }

            if (captureTokens)
            {
                CaptureTokens();
            }

            Apply();
        }

        if (targetDispatcher.CheckAccess())
        {
            Refresh();
        }
        else
        {
            _ = targetDispatcher.BeginInvoke(DispatcherPriority.DataBind, Refresh);
        }
    }

    private void CaptureTokens()
    {
        var menu = navigationMenu.Current;
        CaptureMap(
            groupTitleKeys,
            menu.Groups.Where(group => group.Title is not null).Select(group => (group.Id, group.Title!))
        );
        CaptureMap(
            navigationLabelKeys,
            menu
                .Groups.SelectMany(group => group.Items)
                .Concat(menu.FixedItems)
                .Select(item => (item.Id, item.Label))
        );

        var titleBarState = titleBar.Current;
        applicationTitleKey = CaptureToken(titleBarState.ApplicationTitle, applicationTitleKey);
        applicationSubTitleKey = CaptureToken(
            titleBarState.ApplicationSubTitle,
            applicationSubTitleKey
        );
        unnamedProjectPlaceholderKey = CaptureToken(
            titleBarState.UnnamedProjectPlaceholder,
            unnamedProjectPlaceholderKey
        );
        logoFallbackTextKey = CaptureToken(
            titleBarState.LogoFallbackText,
            logoFallbackTextKey
        );
        searchPlaceholderKey = CaptureToken(search.Current.Placeholder, searchPlaceholderKey);

        var toolbarState = toolbar.Current;
        CaptureMap(
            defaultToolbarLabelKeys,
            toolbarState.DefaultItems.Select(item => (item.Id, item.DisplayName))
        );
        CaptureMap(
            pageToolbarLabelKeys,
            toolbarState.Pages.SelectMany(page =>
                page.Value.Items.Select(item => ((page.Key, item.Id), item.DisplayName))
            )
        );

        CaptureMap(
            statusItemTextKeys,
            statusBar.Current.Items.Select(item => (item.Id, item.Text))
        );
    }

    private static void CaptureMap<TKey>(
        Dictionary<TKey, string> keys,
        IEnumerable<(TKey Key, string Value)> currentValues
    )
        where TKey : notnull
    {
        var currentKeys = new HashSet<TKey>();
        foreach (var (key, value) in currentValues)
        {
            currentKeys.Add(key);
            var captured = CaptureToken(value, keys.GetValueOrDefault(key));
            if (captured is null)
            {
                keys.Remove(key);
            }
            else
            {
                keys[key] = captured;
            }
        }

        foreach (var removed in keys.Keys.Where(key => !currentKeys.Contains(key)).ToArray())
        {
            keys.Remove(removed);
        }
    }

    private static string? CaptureToken(string? value, string? existing)
    {
        if (string.IsNullOrEmpty(value))
        {
            return null;
        }

        if (Localizer.Contains(value))
        {
            return value;
        }

        return existing is not null
            && string.Equals(value, Localizer.Parse(existing), StringComparison.Ordinal)
                ? existing
                : null;
    }

    private void Apply()
    {
        isApplying = true;
        try
        {
            ApplyNavigation();
            ApplyTitleBar();
            ApplyToolbar();
            ApplyStatusBar();
        }
        finally
        {
            isApplying = false;
        }
    }

    private void ApplyNavigation()
    {
        if (groupTitleKeys.Count == 0 && navigationLabelKeys.Count == 0)
        {
            return;
        }

        navigationMenu.Set(editor =>
        {
            foreach (var (id, resourceKey) in groupTitleKeys)
            {
                editor.SetGroupTitle(id, Localizer.Parse(resourceKey));
            }

            foreach (var (id, resourceKey) in navigationLabelKeys)
            {
                editor.SetItem(id, item => item with { Label = Localizer.Parse(resourceKey) });
            }
        });
    }

    private void ApplyTitleBar()
    {
        if (applicationTitleKey is not null)
        {
            titleBar.SetApplicationTitle(Localizer.Parse(applicationTitleKey));
        }

        if (applicationSubTitleKey is not null)
        {
            titleBar.SetApplicationSubTitle(Localizer.Parse(applicationSubTitleKey));
        }

        if (unnamedProjectPlaceholderKey is not null)
        {
            titleBar.SetUnnamedProjectPlaceholder(Localizer.Parse(unnamedProjectPlaceholderKey));
        }

        if (logoFallbackTextKey is not null)
        {
            var state = titleBar.Current;
            titleBar.SetLogo(
                state.LogoPath,
                Localizer.Parse(logoFallbackTextKey),
                state.ShowApplicationTitle,
                state.ShowApplicationSubTitle,
                state.ShowProjectTitle
            );
        }

        if (searchPlaceholderKey is not null)
        {
            search.SetPlaceholder(Localizer.Parse(searchPlaceholderKey));
        }
    }

    private void ApplyToolbar()
    {
        if (defaultToolbarLabelKeys.Count == 0 && pageToolbarLabelKeys.Count == 0)
        {
            return;
        }

        var toolbarState = toolbar.Current;
        if (defaultToolbarLabelKeys.Count > 0 && toolbarState.DefaultItems.Count > 0)
        {
            toolbar.SetDefault(
                toolbarState.DefaultItems.Select(item =>
                    defaultToolbarLabelKeys.TryGetValue(item.Id, out var resourceKey)
                        ? item with { DisplayName = Localizer.Parse(resourceKey) }
                        : item
                )
            );
        }

        foreach (var (pageType, page) in toolbarState.Pages)
        {
            if (!pageToolbarLabelKeys.Keys.Any(key => key.PageType == pageType))
            {
                continue;
            }

            toolbar.Set(
                pageType,
                page.Items.Select(item =>
                    pageToolbarLabelKeys.TryGetValue((pageType, item.Id), out var resourceKey)
                        ? item with { DisplayName = Localizer.Parse(resourceKey) }
                        : item
                ),
                page.IconOnly
            );
        }
    }

    private void ApplyStatusBar()
    {
        foreach (var (id, resourceKey) in statusItemTextKeys)
        {
            statusBar.SetItemText(id, Localizer.Parse(resourceKey));
        }
    }

    private void ClearCapturedTokens()
    {
        groupTitleKeys.Clear();
        navigationLabelKeys.Clear();
        defaultToolbarLabelKeys.Clear();
        pageToolbarLabelKeys.Clear();
        statusItemTextKeys.Clear();
        applicationTitleKey = null;
        applicationSubTitleKey = null;
        unnamedProjectPlaceholderKey = null;
        logoFallbackTextKey = null;
        searchPlaceholderKey = null;
    }
}
