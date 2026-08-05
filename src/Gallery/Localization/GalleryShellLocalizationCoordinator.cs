using ArkheideSystem.Flourish.Abstract.Runtime;

namespace ArkheideSystem.Gallery.Localization;

/// <summary>
/// Re-publishes application-owned shell labels when the selected locale changes.
/// Navigation identities, routes, and command keys remain unchanged.
/// </summary>
public sealed class GalleryShellLocalizationCoordinator(
    IGalleryLocalization localization,
    INavigationMenuService navigationMenu,
    ITitleBarService titleBar,
    ITitleBarSearchService search,
    IToolbarService toolbar
) : IDisposable
{
    private IReadOnlyDictionary<string, string>? groupTitles;
    private IReadOnlyDictionary<string, string>? navigationLabels;
    private IReadOnlyDictionary<string, string>? defaultToolbarLabels;
    private IReadOnlyDictionary<(Type PageType, string ItemId), string>? pageToolbarLabels;
    private bool isStarted;

    public void Start()
    {
        if (isStarted)
        {
            return;
        }

        isStarted = true;
        CaptureSources();
        localization.Changed += Localization_Changed;
        Apply();
    }

    public void Dispose()
    {
        localization.Changed -= Localization_Changed;
    }

    private void CaptureSources()
    {
        var menu = navigationMenu.Current;
        groupTitles = menu.Groups
            .Where(group => group.Title is not null)
            .ToDictionary(group => group.Id, group => group.Title!, StringComparer.Ordinal);
        navigationLabels = menu.Groups
            .SelectMany(group => group.Items)
            .Concat(menu.FixedItems)
            .ToDictionary(item => item.Id, item => item.Label, StringComparer.Ordinal);

        var toolbarState = toolbar.Current;
        defaultToolbarLabels = toolbarState.DefaultItems.ToDictionary(
            item => item.Id,
            item => item.DisplayName,
            StringComparer.Ordinal
        );
        pageToolbarLabels = toolbarState.Pages
            .SelectMany(page => page.Value.Items.Select(item =>
                (Key: (page.Key, item.Id), item.DisplayName)
            ))
            .ToDictionary(pair => pair.Key, pair => pair.DisplayName);
    }

    private void Localization_Changed(object? sender, EventArgs e) => Apply();

    private void Apply()
    {
        if (
            groupTitles is null
            || navigationLabels is null
            || defaultToolbarLabels is null
            || pageToolbarLabels is null
        )
        {
            return;
        }

        navigationMenu.Set(editor =>
        {
            foreach (var (id, source) in groupTitles)
            {
                editor.SetGroupTitle(id, localization.Get(source));
            }

            foreach (var (id, source) in navigationLabels)
            {
                editor.SetItem(id, item => item with { Label = localization.Get(source) });
            }
        });

        titleBar.SetApplicationSubTitle(localization.Get("Component reference"));
        titleBar.SetUnnamedProjectPlaceholder(localization.Get("Untitled project"));
        search.SetPlaceholder(localization.Get("Type here to search"));

        var toolbarState = toolbar.Current;
        if (toolbarState.DefaultItems.Count > 0)
        {
            toolbar.SetDefault(
                toolbarState.DefaultItems.Select(item =>
                    defaultToolbarLabels.TryGetValue(item.Id, out var source)
                        ? item with { DisplayName = localization.Get(source) }
                        : item
                )
            );
        }

        foreach (var (pageType, page) in toolbarState.Pages)
        {
            toolbar.Set(
                pageType,
                page.Items.Select(item =>
                    pageToolbarLabels.TryGetValue((pageType, item.Id), out var source)
                        ? item with { DisplayName = localization.Get(source) }
                        : item
                ),
                page.IconOnly
            );
        }
    }
}
