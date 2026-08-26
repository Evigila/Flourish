using System;
using System.Threading;
using System.Threading.Tasks;

using CKey = Arkheide.Essential.Culture.Key;
using Localizer = Arkheide.Essential.Culture.Localizer;
using InputKey = System.Windows.Input.Key;
using ArkheideSystem.Flourish.Abstract;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ArkheideSystem.Flourish.Controls;

namespace ArkheideSystem.Gallery.Views;

public partial class TitleBarRuntimePage : Page
{
    private readonly ITitleBarService titleBar;
    private IRegistration? searchSubscription;
    private bool isRefreshing;

    public TitleBarRuntimePage(ITitleBarService titleBar)
    {
        this.titleBar = titleBar;
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
        Localizer.Current.Changed += Localizer_Changed;
        searchSubscription = titleBar.SubscribeSearch(HandleSearchQueryAsync);
        RefreshState();
    }

    private void Page_Unloaded(object sender, RoutedEventArgs e)
    {
        titleBar.Changed -= TitleBar_Changed;
        Localizer.Current.Changed -= Localizer_Changed;
        searchSubscription?.Dispose();
        searchSubscription = null;
    }

    private void TitleBar_Changed(object? sender, FlourishStateChangedEventArgs<FlourishTitleBarState> e)
    {
        Dispatcher.BeginInvoke(RefreshState);
    }

    private void Localizer_Changed(object? sender, EventArgs e)
    {
        Dispatcher.BeginInvoke(RefreshSearchState);
    }

    private async ValueTask HandleSearchQueryAsync(
        FlourishTitleBarSearchQuery args,
        CancellationToken cancellationToken
    )
    {
        await Task.Delay(250, cancellationToken);
        await Dispatcher.InvokeAsync(() =>
        {
            SearchOutput.WriteLine(
                string.IsNullOrWhiteSpace(args.Text)
                    ? Localizer.Parse(
                        CKey.Runtime_Query0EmptyQuery_1782FB95,
                        args.Sequence
                    )
                    : Localizer.Parse(
                        CKey.Runtime_Query0SimulatedResultsFor1CompletedAt2T_DD07B40D,
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
                titleBar.SetApplicationIdentity(TitleBox.Text, NullIfWhiteSpace(SubtitleBox.Text)),
            IdentityOutput,
            Localizer.Parse(CKey.Runtime_ApplicationIdentityUpdated_965263E3)
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
                    current.ShowApplicationSubtitle,
                    current.ShowProjectTitle
                ),
            IdentityOutput,
            Localizer.Parse(CKey.Runtime_TitleBarLogoSettingsUpdated_791EEA02)
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
                Localizer.Parse(CKey.Runtime_UnnamedProjectPlaceholderUpdated_B0D701C1)
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
                () =>
                    titleBar.SetElementVisible(
                        element,
                        TitleBarElementVisibleBox.IsChecked == true
                    ),
                ElementOutput,
                Localizer.Parse(
                    CKey.Runtime_Text0VisibilitySetTo1_16423423,
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
                Localizer.Parse(
                    CKey.Runtime_BreadcrumbDisplayModeSetTo0_19EF937D,
                    mode
                )
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
            () => titleBar.SetSearchText(SearchTextBox.Text),
            SearchOutput,
            Localizer.Parse(
                CKey.Runtime_SearchTextSetTo0_37DE597D,
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
            titleBar.FocusSearch,
            SearchOutput,
            Localizer.Parse(CKey.Runtime_MovedFocusToTitleBarSearch_935CEC34)
        );
    }

    private void ClearSearch_Click(object sender, RoutedEventArgs e)
    {
        Execute(
            titleBar.ClearSearch,
            SearchOutput,
            Localizer.Parse(CKey.Runtime_ClearedTheTitleBarSearchQuery_36169020)
        );
    }

    private void ApplySearchPlaceholder_Click(object sender, RoutedEventArgs e)
    {
        Execute(
            () => titleBar.SetSearchPlaceholder(SearchPlaceholderBox.Text),
            SearchOutput,
            Localizer.Parse(
                CKey.Runtime_SearchPlaceholderSetTo0_F701246C,
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
        var visible = !titleBar.Current.IsSearchVisible;
        Execute(
            () => titleBar.SetSearchVisible(visible),
            SearchOutput,
            Localizer.Parse(
                CKey.Runtime_TitleBarSearch0_262A9ED5,
                Localizer.Parse(
                    visible ? CKey.Runtime_Shown_BAAF5362 : CKey.Runtime_Hidden_E564B408
                )
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
                Localizer.Parse(
                    CKey.Runtime_TitleBar0_7ACF611F,
                    Localizer.Parse(
                        enabled
                            ? CKey.Runtime_Enabled_FB9CF756
                            : CKey.Runtime_Disabled_17EB3C01
                    )
                )
            );
        }
    }

    private bool CanApplyImmediately => IsLoaded && !isRefreshing;

    private static void CommitOnEnter(KeyEventArgs e, Action commit)
    {
        if (e.Key != InputKey.Enter)
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
            output.WriteLine(
                Localizer.Parse(CKey.Dynamic_Error0_43F78154, error.Message)
            );
        }
    }

    private void RefreshState()
    {
        isRefreshing = true;
        try
        {
            var current = titleBar.Current;
            TitleBox.Text = current.ApplicationTitle;
            SubtitleBox.Text = current.ApplicationSubtitle;
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
        var current = titleBar.Current;
        SearchTextBox.Text = current.SearchText;
        SearchPlaceholderBox.Text = current.SearchPlaceholder;
        ToggleSearchVisibilityButton.Content = Localizer.Parse(
            current.IsSearchVisible
                ? CKey.Runtime_HideSearch_14BD5CB7
                : CKey.Runtime_ShowSearch_96369815
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
