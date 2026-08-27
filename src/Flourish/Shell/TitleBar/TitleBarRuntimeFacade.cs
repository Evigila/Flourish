using System;
using System.Threading;
using System.Threading.Tasks;

using ArkheideSystem.Flourish.Abstract;
namespace ArkheideSystem.Flourish.Shell.TitleBar;

internal sealed class TitleBarRuntimeFacade : ITitleBarService, IDisposable
{
    private readonly Lock gate = new();
    private readonly TitleBarService titleBar;
    private readonly TitleBarSearchService search;
    private TitleBarState current;
    private long version;
    private bool isDisposed;

    public TitleBarRuntimeFacade(TitleBarService titleBar, TitleBarSearchService search)
    {
        this.titleBar = titleBar ?? throw new ArgumentNullException(nameof(titleBar));
        this.search = search ?? throw new ArgumentNullException(nameof(search));
        current = CaptureCurrent();
        titleBar.Changed += Source_Changed;
        search.Changed += Source_Changed;
    }

    public TitleBarState Current
    {
        get
        {
            ObjectDisposedException.ThrowIf(Volatile.Read(ref isDisposed), this);
            return Volatile.Read(ref current);
        }
    }

    public event EventHandler<StateChangedEventArgs<TitleBarState>>? Changed;

    public void SetEnabled(bool enabled) => titleBar.SetEnabled(enabled);
    public void SetApplicationTitle(string title) => titleBar.SetApplicationTitle(title);
    public void SetApplicationSubtitle(string? subtitle) => titleBar.SetApplicationSubtitle(subtitle);
    public void SetApplicationIdentity(string title, string? subtitle = null) =>
        titleBar.SetApplicationIdentity(title, subtitle);
    public void SetUnnamedProjectPlaceholder(string placeholder) =>
        titleBar.SetUnnamedProjectPlaceholder(placeholder);
    public void SetLogo(
        string? logoPath,
        string? fallbackText = null,
        bool showApplicationTitle = true,
        bool showApplicationSubtitle = true,
        bool showProjectTitle = false
    ) => titleBar.SetLogo(
        logoPath,
        fallbackText,
        showApplicationTitle,
        showApplicationSubtitle,
        showProjectTitle
    );
    public void SetSearchPlaceholder(string placeholder) => search.SetPlaceholder(placeholder);
    public void SetSearchVisible(bool visible) => search.SetVisible(visible);
    public void SetSearchText(string text) => search.SetText(text);
    public void ClearSearch() => search.Clear();
    public void FocusSearch() => search.Focus();
    public IRegistration SubscribeSearch(
        Func<TitleBarSearchQuery, CancellationToken, ValueTask> handler
    ) => search.Subscribe(handler);
    public void SetElementVisible(TitleBarElement element, bool visible)
    {
        if (element == TitleBarElement.Search)
        {
            search.SetVisible(visible);
            return;
        }

        titleBar.SetElementVisible(element, visible);
    }
    public void SetBreadcrumbMode(BreadcrumbShowOption mode) => titleBar.SetBreadcrumbMode(mode);

    public void Dispose()
    {
        lock (gate)
        {
            if (isDisposed)
            {
                return;
            }

            Volatile.Write(ref isDisposed, true);
        }

        titleBar.Changed -= Source_Changed;
        search.Changed -= Source_Changed;
    }

    private void Source_Changed(object? sender, EventArgs args)
    {
        TitleBarState snapshot;
        lock (gate)
        {
            if (isDisposed)
            {
                return;
            }

            version++;
            snapshot = CaptureCurrent();
            Volatile.Write(ref current, snapshot);
        }

        Changed?.Invoke(this, new StateChangedEventArgs<TitleBarState>(snapshot));
    }

    private TitleBarState CaptureCurrent()
    {
        var titleBarState = titleBar.Current;
        var searchState = search.Current;
        return titleBarState with
        {
            SearchPlaceholder = searchState.Placeholder,
            IsSearchVisible = searchState.IsVisible,
            SearchText = searchState.Text,
            IsSearchFocusRequested = searchState.FocusRequested,
            Version = version,
        };
    }
}
