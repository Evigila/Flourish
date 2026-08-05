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
    private IReadOnlyDictionary<string, string>? groupTitleKeys;
    private IReadOnlyDictionary<string, string>? navigationLabelKeys;
    private IReadOnlyDictionary<string, string>? defaultToolbarLabelKeys;
    private IReadOnlyDictionary<(Type PageType, string ItemId), string>? pageToolbarLabelKeys;
    private bool isStarted;

    public void Start()
    {
        if (isStarted)
        {
            return;
        }

        isStarted = true;
        CaptureResourceKeys();
        localization.Changed += Localization_Changed;
        Apply();
    }

    public void Dispose()
    {
        localization.Changed -= Localization_Changed;
    }

    private void CaptureResourceKeys()
    {
        var menu = navigationMenu.Current;
        groupTitleKeys = menu
            .Groups.Where(group => group.Title is not null)
            .ToDictionary(group => group.Id, group => group.Title!, StringComparer.Ordinal);
        navigationLabelKeys = menu
            .Groups.SelectMany(group => group.Items)
            .Concat(menu.FixedItems)
            .ToDictionary(item => item.Id, item => item.Label, StringComparer.Ordinal);

        var toolbarState = toolbar.Current;
        defaultToolbarLabelKeys = toolbarState.DefaultItems.ToDictionary(
            item => item.Id,
            item => item.DisplayName,
            StringComparer.Ordinal
        );
        pageToolbarLabelKeys = toolbarState
            .Pages.SelectMany(page =>
                page.Value.Items.Select(item => (Key: (page.Key, item.Id), item.DisplayName))
            )
            .ToDictionary(pair => pair.Key, pair => pair.DisplayName);
    }

    private void Localization_Changed(object? sender, EventArgs e) => Apply();

    private void Apply()
    {
        if (
            groupTitleKeys is null
            || navigationLabelKeys is null
            || defaultToolbarLabelKeys is null
            || pageToolbarLabelKeys is null
        )
        {
            return;
        }

        navigationMenu.Set(editor =>
        {
            foreach (var (id, resourceKey) in groupTitleKeys)
            {
                editor.SetGroupTitle(id, localization.Get(resourceKey));
            }

            foreach (var (id, resourceKey) in navigationLabelKeys)
            {
                editor.SetItem(id, item => item with { Label = localization.Get(resourceKey) });
            }
        });

        titleBar.SetApplicationSubTitle(
            localization.Get(GalleryLocaleKeys.ApplicationComponentReference_661E6097)
        );
        titleBar.SetUnnamedProjectPlaceholder(
            localization.Get(GalleryLocaleKeys.ApplicationUntitledProject_1B5A65C3)
        );
        search.SetPlaceholder(
            localization.Get(GalleryLocaleKeys.ApplicationTypeHereToSearch_85717255)
        );

        var toolbarState = toolbar.Current;
        if (toolbarState.DefaultItems.Count > 0)
        {
            toolbar.SetDefault(
                toolbarState.DefaultItems.Select(item =>
                    defaultToolbarLabelKeys.TryGetValue(item.Id, out var resourceKey)
                        ? item with
                        {
                            DisplayName = localization.Get(resourceKey),
                        }
                        : item
                )
            );
        }

        foreach (var (pageType, page) in toolbarState.Pages)
        {
            toolbar.Set(
                pageType,
                page.Items.Select(item =>
                    pageToolbarLabelKeys.TryGetValue((pageType, item.Id), out var resourceKey)
                        ? item with
                        {
                            DisplayName = localization.Get(resourceKey),
                        }
                        : item
                ),
                page.IconOnly
            );
        }
    }
}
