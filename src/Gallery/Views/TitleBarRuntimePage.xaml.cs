using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ArkheideSystem.Flourish.Abstract;
using ArkheideSystem.Flourish.Controls;
using ArkheideSystem.Gallery.Localization;

namespace ArkheideSystem.Gallery.Views;

public partial class TitleBarRuntimePage : Page
{
    private readonly ITitleBarService titleBar;
    private readonly ITitleBarSearchService search;
    private readonly IGalleryLocalization galleryLocalization;
    private IDisposable? searchSubscription;
    private bool isRefreshing;

    public TitleBarRuntimePage(
        ITitleBarService titleBar,
        ITitleBarSearchService search,
        IGalleryLocalization galleryLocalization
    )
    {
        this.titleBar = titleBar;
        this.search = search;
        this.galleryLocalization = galleryLocalization;
        InitializeComponent();

        TitleBarElementBox.ItemsSource = new TitleBarElement[]
        {
            TitleBarElement.Search,
            TitleBarElement.Breadcrumb,
            TitleBarElement.NavigationToggle,
            TitleBarElement.Logo,
            TitleBarElement.Title,
            TitleBarElement.ThemeToggle,
            TitleBarElement.Profile,
        };
        BreadcrumbModeBox.ItemsSource = Enum.GetValues<BreadcrumbShowOption>();
        TitleBarElementBox.SelectedItem = TitleBarElement.Search;

        Loaded += Page_Loaded;
        Unloaded += Page_Unloaded;
        RefreshState();
    }

    private void Page_Loaded(object sender, RoutedEventArgs e)
    {
        Page_Unloaded(sender, e);
        titleBar.Changed += TitleBar_Changed;
        search.StateChanged += Search_StateChanged;
        galleryLocalization.Changed += GalleryLocalization_Changed;
        searchSubscription = search.Subscribe(HandleSearchQueryAsync);
        RefreshState();
    }

    private void Page_Unloaded(object sender, RoutedEventArgs e)
    {
        titleBar.Changed -= TitleBar_Changed;
        search.StateChanged -= Search_StateChanged;
        galleryLocalization.Changed -= GalleryLocalization_Changed;
        searchSubscription?.Dispose();
        searchSubscription = null;
    }

    private void TitleBar_Changed(object? sender, FlourishTitleBarChangedEventArgs e)
    {
        Dispatcher.BeginInvoke(RefreshState);
    }

    private void Search_StateChanged(object? sender, FlourishTitleBarSearchStateChangedEventArgs e)
    {
        Dispatcher.BeginInvoke(RefreshSearchState);
    }

    private void GalleryLocalization_Changed(object? sender, EventArgs e)
    {
        Dispatcher.BeginInvoke(RefreshSearchState);
    }

    private async ValueTask HandleSearchQueryAsync(
        FlourishTitleBarSearchChangedEventArgs args,
        CancellationToken cancellationToken
    )
    {
        await Task.Delay(250, cancellationToken);
        await Dispatcher.InvokeAsync(() =>
        {
            SearchOutput.WriteLine(
                string.IsNullOrWhiteSpace(args.Text)
                    ? galleryLocalization.Format(
                        "Query #{0}: empty query",
                        args.Sequence
                    )
                    : galleryLocalization.Format(
                        "Query #{0}: simulated results for \"{1}\" completed at {2:T}.",
                        args.Sequence,
                        args.Text,
                        DateTime.Now
                    )
            );
        });
    }

    private void ApplyIdentity_Click(object sender, RoutedEventArgs e)
    {
        Execute(
            () =>
                titleBar.SetApplicationIdentity(
                    TitleBox.Text,
                    NullIfWhiteSpace(SubtitleBox.Text)
                ),
            IdentityOutput,
            galleryLocalization.Get("Application identity updated.")
        );
    }

    private void IdentityBox_LostFocus(object sender, RoutedEventArgs e) => CommitIdentity();

    private void IdentityBox_KeyDown(object sender, KeyEventArgs e) =>
        CommitOnEnter(e, CommitIdentity);

    private void CommitIdentity()
    {
        if (CanApplyImmediately)
        {
            ApplyIdentity_Click(this, new RoutedEventArgs());
        }
    }

    private void ApplyLogo_Click(object sender, RoutedEventArgs e)
    {
        var current = titleBar.Current;
        Execute(
            () =>
                titleBar.SetLogo(
                    NullIfWhiteSpace(LogoPathBox.Text),
                    NullIfWhiteSpace(LogoFallbackBox.Text),
                    current.ShowApplicationTitle,
                    current.ShowApplicationSubTitle,
                    current.ShowProjectTitle
                ),
            IdentityOutput,
            galleryLocalization.Get("Title-bar logo settings updated.")
        );
    }

    private void LogoBox_LostFocus(object sender, RoutedEventArgs e) => CommitLogo();

    private void LogoBox_KeyDown(object sender, KeyEventArgs e) => CommitOnEnter(e, CommitLogo);

    private void CommitLogo()
    {
        if (CanApplyImmediately)
        {
            ApplyLogo_Click(this, new RoutedEventArgs());
        }
    }

    private void UnnamedProjectBox_LostFocus(object sender, RoutedEventArgs e) =>
        CommitUnnamedProjectPlaceholder();

    private void UnnamedProjectBox_KeyDown(object sender, KeyEventArgs e) =>
        CommitOnEnter(e, CommitUnnamedProjectPlaceholder);

    private void CommitUnnamedProjectPlaceholder()
    {
        if (CanApplyImmediately)
        {
            Execute(
                () => titleBar.SetUnnamedProjectPlaceholder(UnnamedProjectBox.Text),
                IdentityOutput,
                galleryLocalization.Get("Unnamed project placeholder updated.")
            );
        }
    }

    private void TitleBarElementBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        RefreshSelectedElementState();
    }

    private void ApplyElementVisibility_Click(object sender, RoutedEventArgs e)
    {
        if (TitleBarElementBox.SelectedItem is TitleBarElement element)
        {
            Execute(
                () => titleBar.SetElementVisible(element, TitleBarElementVisibleBox.IsChecked == true),
                ElementOutput,
                galleryLocalization.Format(
                    "{0} visibility set to {1}.",
                    element,
                    TitleBarElementVisibleBox.IsChecked == true
                )
            );
        }
    }

    private void TitleBarElementVisibleBox_Changed(object sender, RoutedEventArgs e)
    {
        if (CanApplyImmediately)
        {
            ApplyElementVisibility_Click(sender, new RoutedEventArgs());
        }
    }

    private void ApplyBreadcrumbMode_Click(object sender, RoutedEventArgs e)
    {
        if (BreadcrumbModeBox.SelectedItem is BreadcrumbShowOption mode)
        {
            Execute(
                () => titleBar.SetBreadcrumbMode(mode),
                ElementOutput,
                galleryLocalization.Format("Breadcrumb display mode set to {0}.", mode)
            );
        }
    }

    private void BreadcrumbModeBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (CanApplyImmediately)
        {
            ApplyBreadcrumbMode_Click(sender, new RoutedEventArgs());
        }
    }

    private void SetSearchText_Click(object sender, RoutedEventArgs e)
    {
        Execute(
            () => search.SetText(SearchTextBox.Text),
            SearchOutput,
            galleryLocalization.Format(
                "Search text set to \"{0}\".",
                SearchTextBox.Text
            )
        );
    }

    private void SearchTextBox_LostFocus(object sender, RoutedEventArgs e) => CommitSearchText();

    private void SearchTextBox_KeyDown(object sender, KeyEventArgs e) =>
        CommitOnEnter(e, CommitSearchText);

    private void CommitSearchText()
    {
        if (CanApplyImmediately)
        {
            SetSearchText_Click(this, new RoutedEventArgs());
        }
    }

    private void FocusSearch_Click(object sender, RoutedEventArgs e)
    {
        Execute(
            search.Focus,
            SearchOutput,
            galleryLocalization.Get("Moved focus to title-bar search.")
        );
    }

    private void ClearSearch_Click(object sender, RoutedEventArgs e)
    {
        Execute(
            search.Clear,
            SearchOutput,
            galleryLocalization.Get("Cleared the title-bar search query.")
        );
    }

    private void ApplySearchPlaceholder_Click(object sender, RoutedEventArgs e)
    {
        Execute(
            () => search.SetPlaceholder(SearchPlaceholderBox.Text),
            SearchOutput,
            galleryLocalization.Format(
                "Search placeholder set to \"{0}\".",
                SearchPlaceholderBox.Text
            )
        );
    }

    private void SearchPlaceholderBox_LostFocus(object sender, RoutedEventArgs e) =>
        CommitSearchPlaceholder();

    private void SearchPlaceholderBox_KeyDown(object sender, KeyEventArgs e) =>
        CommitOnEnter(e, CommitSearchPlaceholder);

    private void CommitSearchPlaceholder()
    {
        if (CanApplyImmediately)
        {
            ApplySearchPlaceholder_Click(this, new RoutedEventArgs());
        }
    }

    private void ToggleSearchVisibility_Click(object sender, RoutedEventArgs e)
    {
        var visible = !search.Current.IsVisible;
        Execute(
            () => search.SetVisible(visible),
            SearchOutput,
            galleryLocalization.Format(
                "Title-bar search {0}.",
                galleryLocalization.Get(visible ? "shown" : "hidden")
            )
        );
    }

    private void TitleBarEnabledBox_Changed(object sender, RoutedEventArgs e)
    {
        if (CanApplyImmediately)
        {
            var enabled = TitleBarEnabledBox.IsChecked == true;
            Execute(
                () => titleBar.SetEnabled(enabled),
                TitleBarAvailabilityOutput,
                galleryLocalization.Format(
                    "Title bar {0}.",
                    galleryLocalization.Get(enabled ? "enabled" : "disabled")
                )
            );
        }
    }

    private bool CanApplyImmediately => IsLoaded && !isRefreshing;

    private static void CommitOnEnter(KeyEventArgs e, Action commit)
    {
        if (e.Key != Key.Enter)
        {
            return;
        }

        commit();
        e.Handled = true;
    }

    private void Execute(Action action, OutputCard output, string successMessage)
    {
        try
        {
            action();
            output.WriteLine(successMessage);
            RefreshState();
        }
        catch (Exception error)
        {
            output.WriteLine(galleryLocalization.Format("Error: {0}", error.Message));
        }
    }

    private void RefreshState()
    {
        isRefreshing = true;
        try
        {
            var current = titleBar.Current;
            TitleBox.Text = current.ApplicationTitle;
            SubtitleBox.Text = current.ApplicationSubTitle;
            LogoPathBox.Text = current.LogoPath ?? string.Empty;
            LogoFallbackBox.Text = current.LogoFallbackText;
            UnnamedProjectBox.Text = current.UnnamedProjectPlaceholder;
            BreadcrumbModeBox.SelectedItem = current.BreadcrumbMode;
            TitleBarEnabledBox.IsChecked = current.IsEnabled;
            RefreshSelectedElementState();
            RefreshSearchState();
        }
        finally
        {
            isRefreshing = false;
        }
    }

    private void RefreshSearchState()
    {
        var current = search.Current;
        SearchTextBox.Text = current.Text;
        SearchPlaceholderBox.Text = current.Placeholder;
        ToggleSearchVisibilityButton.Content = galleryLocalization.Get(
            current.IsVisible ? "Hide search" : "Show search"
        );
    }

    private void RefreshSelectedElementState()
    {
        if (TitleBarElementBox.SelectedItem is TitleBarElement element)
        {
            var wasRefreshing = isRefreshing;
            isRefreshing = true;
            try
            {
                TitleBarElementVisibleBox.IsChecked = IsTitleBarElementVisible(
                    titleBar.Current,
                    element
                );
            }
            finally
            {
                isRefreshing = wasRefreshing;
            }
        }
    }

    private static bool IsTitleBarElementVisible(
        FlourishTitleBarState state,
        TitleBarElement element
    ) =>
        element switch
        {
            TitleBarElement.Search => state.IsSearchVisible,
            TitleBarElement.Breadcrumb => state.IsBreadcrumbVisible,
            TitleBarElement.NavigationToggle => state.IsNavigationToggleVisible,
            TitleBarElement.Logo => state.IsLogoVisible,
            TitleBarElement.Title => state.IsTitleVisible,
            TitleBarElement.ThemeToggle => state.IsThemeToggleVisible,
            TitleBarElement.Profile => state.IsProfileVisible,
            _ => false,
        };

    private static string? NullIfWhiteSpace(string value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
