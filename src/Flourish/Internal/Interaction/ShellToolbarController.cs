using System.Windows;
using System.Windows.Threading;
using ArkheideSystem.Flourish.Abstract;
using ArkheideSystem.Flourish.Controls;
using ArkheideSystem.Flourish.Services;
using ArkheideSystem.Flourish.Views.Windows;
using Button = ArkheideSystem.Flourish.Controls.Button;

namespace ArkheideSystem.Flourish.Internal.Interaction;

internal sealed class ShellToolbarController : IDisposable
{
    private readonly FlourishToolbar view;
    private readonly FlourishToolbarService service;
    private readonly ICommandRegistry commandRegistry;
    private readonly ICommandDispatcher commandDispatcher;
    private readonly ToolbarCommandButtonIndex commandButtons;
    private readonly Dictionary<Type, IReadOnlyList<Button>> buttonsByPageType = [];
    private IReadOnlyList<Button>? defaultButtons;
    private Type? activePageType;
    private Type? renderedPageType;
    private bool isDefaultToolbarActive;
    private bool lastEnabled;
    private long appliedVersion;
    private bool isInitialized;
    private bool isDisposed;

    internal ShellToolbarController(
        FlourishToolbar view,
        FlourishToolbarService service,
        ICommandRegistry commandRegistry,
        ICommandDispatcher commandDispatcher
    )
    {
        this.view = view ?? throw new ArgumentNullException(nameof(view));
        this.service = service ?? throw new ArgumentNullException(nameof(service));
        this.commandRegistry =
            commandRegistry ?? throw new ArgumentNullException(nameof(commandRegistry));
        this.commandDispatcher =
            commandDispatcher ?? throw new ArgumentNullException(nameof(commandDispatcher));
        commandButtons = new ToolbarCommandButtonIndex(commandDispatcher);
    }

    internal void Init(Type? initialPageType = null)
    {
        ObjectDisposedException.ThrowIf(isDisposed, this);
        if (isInitialized)
        {
            return;
        }

        isInitialized = true;
        service.Changed += Service_Changed;
        commandRegistry.Changed += CommandRegistry_Changed;
        commandRegistry.CanExecuteChanged += CommandRegistry_CanExecuteChanged;

        var snapshot = service.Current;
        lastEnabled = snapshot.IsEnabled;
        appliedVersion = snapshot.Version;
        Build(initialPageType, force: true);
    }

    internal void SetPage(Type? pageType)
    {
        EnsureInitialized();
        Build(pageType);
    }

    internal void RefreshVisibility()
    {
        EnsureInitialized();
        view.UpdateVisibility(service.Current.IsEnabled);
    }

    public void Dispose()
    {
        if (isDisposed)
        {
            return;
        }

        isDisposed = true;
        if (isInitialized)
        {
            service.Changed -= Service_Changed;
            commandRegistry.Changed -= CommandRegistry_Changed;
            commandRegistry.CanExecuteChanged -= CommandRegistry_CanExecuteChanged;
        }

        ClearButtonCache();
        view.Items.Children.Clear();
        view.UpdateVisibility(isEnabled: false);
    }

    private void Build(Type? pageType, bool force = false)
    {
        activePageType = pageType;
        if (!service.Current.IsEnabled)
        {
            view.Items.Children.Clear();
            view.UpdateVisibility(isEnabled: false);
            renderedPageType = null;
            isDefaultToolbarActive = false;
            return;
        }

        if (
            !force
            && renderedPageType == pageType
            && isDefaultToolbarActive == (pageType is null)
        )
        {
            return;
        }

        view.Items.Children.Clear();
        foreach (var button in GetButtons(pageType))
        {
            view.Items.Children.Add(button);
        }

        view.UpdateVisibility(isEnabled: true);
        renderedPageType = pageType;
        isDefaultToolbarActive = pageType is null;
    }

    private IReadOnlyList<Button> GetButtons(Type? pageType)
    {
        if (pageType is null)
        {
            return defaultButtons ??= CreateButtons(
                service.GetToolbarItems(),
                showIconOnly: false
            );
        }

        if (!buttonsByPageType.TryGetValue(pageType, out var buttons))
        {
            buttons = CreateButtons(
                service.GetToolbarItems(pageType),
                service.ShouldShowIconOnly(pageType)
            );
            buttonsByPageType[pageType] = buttons;
        }

        return buttons;
    }

    private IReadOnlyList<Button> CreateButtons(
        IReadOnlyList<FlourishToolbarItem> items,
        bool showIconOnly
    )
    {
        var buttons = new List<Button>(items.Count);
        foreach (var item in items)
        {
            if (!item.IsVisible)
            {
                continue;
            }

            var hasIcon = !string.IsNullOrWhiteSpace(item.IconGlyph);
            var useIconOnly = showIconOnly && hasIcon;
            Button button = hasIcon
                ? new Button { Icon = item.IconGlyph }
                : new Button();
            button.Content = useIconOnly ? null : item.DisplayName;
            button.Margin =
                buttons.Count > 0 ? new Thickness(2, 0, 0, 0) : new Thickness();
            button.ToolTip = item.DisplayName;
            button.Variant = ButtonVariant.Text;
            button.Tag = item;
            button.Width = useIconOnly ? 30 : double.NaN;
            button.Height = 28;
            button.MinWidth = useIconOnly ? 0 : 28;
            button.MinHeight = 0;
            button.Padding = new Thickness(7, 0, 7, 0);
            commandButtons.Track(button, item);
            button.Click += Button_Click;
            buttons.Add(button);
        }

        return buttons;
    }

    private async void Button_Click(object sender, RoutedEventArgs e)
    {
        if (
            sender is Button
            {
                Tag: FlourishToolbarItem { CommandKey: string commandKey },
            }
            && !string.IsNullOrWhiteSpace(commandKey)
        )
        {
            await commandDispatcher.ExecuteAsync(commandKey, source: CommandSource.Toolbar);
        }
    }

    private void Service_Changed(object? sender, FlourishToolbarChangedEventArgs e)
    {
        Dispatch(() =>
        {
            if (e.Current.Version <= appliedVersion)
            {
                return;
            }

            appliedVersion = e.Current.Version;
            var enabledChanged = lastEnabled != e.Current.IsEnabled;
            lastEnabled = e.Current.IsEnabled;
            if (enabledChanged)
            {
                ClearButtonCache();
                Build(activePageType, force: true);
                return;
            }

            InvalidateButtonCache(e.PageType, e.Current);
            if (
                e.PageType is null
                    ? activePageType is null
                        || !e.Current.Pages.ContainsKey(activePageType)
                    : e.PageType == activePageType
            )
            {
                Build(activePageType, force: true);
            }
        });
    }

    private void CommandRegistry_Changed(object? sender, CommandRegistryChangedEventArgs e)
    {
        Dispatch(() => commandButtons.Refresh(e.CommandKey));
    }

    private void CommandRegistry_CanExecuteChanged(
        object? sender,
        CommandCanExecuteChangedEventArgs e
    )
    {
        Dispatch(() => commandButtons.Refresh(e.CommandKey));
    }

    private void Dispatch(Action action)
    {
        void ExecuteIfActive()
        {
            if (
                isDisposed
                || view.Dispatcher.HasShutdownStarted
                || view.Dispatcher.HasShutdownFinished
            )
            {
                return;
            }

            action();
        }

        if (view.Dispatcher.CheckAccess())
        {
            ExecuteIfActive();
            return;
        }

        if (
            isDisposed
            || view.Dispatcher.HasShutdownStarted
            || view.Dispatcher.HasShutdownFinished
        )
        {
            return;
        }

        _ = view.Dispatcher.BeginInvoke(
            DispatcherPriority.DataBind,
            new Action(ExecuteIfActive)
        );
    }

    private void ClearButtonCache()
    {
        commandButtons.Clear();
        if (defaultButtons is not null)
        {
            DetachButtons(defaultButtons);
        }

        foreach (var buttons in buttonsByPageType.Values)
        {
            DetachButtons(buttons);
        }

        defaultButtons = null;
        buttonsByPageType.Clear();
        renderedPageType = null;
        isDefaultToolbarActive = false;
    }

    private void InvalidateButtonCache(
        Type? pageType,
        FlourishToolbarSnapshot snapshot
    )
    {
        if (pageType is not null)
        {
            if (buttonsByPageType.Remove(pageType, out var buttons))
            {
                ReleaseButtons(buttons);
            }

            return;
        }

        if (defaultButtons is not null)
        {
            ReleaseButtons(defaultButtons);
            defaultButtons = null;
        }

        foreach (
            var fallbackPageType in buttonsByPageType.Keys
                .Where(candidate => !snapshot.Pages.ContainsKey(candidate))
                .ToArray()
        )
        {
            var buttons = buttonsByPageType[fallbackPageType];
            buttonsByPageType.Remove(fallbackPageType);
            ReleaseButtons(buttons);
        }
    }

    private void ReleaseButtons(IReadOnlyList<Button> buttons)
    {
        commandButtons.Untrack(buttons);
        DetachButtons(buttons);
    }

    private void DetachButtons(IEnumerable<Button> buttons)
    {
        foreach (var button in buttons)
        {
            button.Click -= Button_Click;
        }
    }

    private void EnsureInitialized()
    {
        ObjectDisposedException.ThrowIf(isDisposed, this);
        if (!isInitialized)
        {
            throw new InvalidOperationException("The toolbar controller has not been initialized.");
        }
    }
}
