using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Threading;
using ArkheideSystem.Flourish.Abstract;
using ArkheideSystem.Flourish.Controls;
using ArkheideSystem.Flourish.Services;
using WpfContextMenu = System.Windows.Controls.ContextMenu;
using WpfControl = System.Windows.Controls.Control;
using WpfMenuItem = System.Windows.Controls.MenuItem;

namespace ArkheideSystem.Flourish.Views.Windows;

internal sealed class ProjectSelectorController : IDisposable
{
    private readonly FlourishTitlebar titlebar;
    private readonly FlourishComboBox selector;
    private readonly ProjectService projectService;
    private readonly IProjectBehavior projectBehavior;
    private readonly FlourishLocalizationService localizationService;
    private readonly NotificationService notificationService;
    private readonly Dispatcher dispatcher;
    private readonly CancellationTokenSource lifetimeCancellation = new();
    private readonly Dictionary<string, FlourishComboBoxItem> projectItemsById = new(
        StringComparer.Ordinal
    );
    private FlourishTitleBarState titleState = null!;
    private FlourishComboBoxItem? applicationTitleItem;
    private FlourishComboBoxItem? projectPlaceholderItem;
    private FlourishComboBoxItem? newProjectItem;
    private long appliedProjectVersion;
    private bool suppressSelectionChanged;
    private bool isProjectBehaviorPending;
    private bool isInitialized;
    private volatile bool isDisposed;

    internal ProjectSelectorController(
        FlourishTitlebar titlebar,
        ProjectService projectService,
        IProjectBehavior projectBehavior,
        FlourishLocalizationService localizationService,
        NotificationService notificationService
    )
    {
        this.titlebar = titlebar ?? throw new ArgumentNullException(nameof(titlebar));
        selector = titlebar.TitleSelector;
        this.projectService =
            projectService ?? throw new ArgumentNullException(nameof(projectService));
        this.projectBehavior =
            projectBehavior ?? throw new ArgumentNullException(nameof(projectBehavior));
        this.localizationService =
            localizationService
            ?? throw new ArgumentNullException(nameof(localizationService));
        this.notificationService =
            notificationService ?? throw new ArgumentNullException(nameof(notificationService));
        dispatcher = selector.Dispatcher;
    }

    internal event EventHandler? Opening;

    internal event EventHandler<FlourishProjectsChangedEventArgs>? Changed;

    internal FlourishProjectSnapshot Current => projectService.Current;

    internal bool CanSave =>
        !isDisposed
        && projectService.Current.IsMultiProjectEnabled
        && projectService.Current.ActiveProject is not null;

    internal void Init(FlourishTitleBarState initialTitleState)
    {
        ArgumentNullException.ThrowIfNull(initialTitleState);
        ObjectDisposedException.ThrowIf(isDisposed, this);
        if (isInitialized)
        {
            return;
        }

        isInitialized = true;
        titleState = initialTitleState;
        appliedProjectVersion = projectService.Current.Version;
        titlebar.TitleSelectionChanged += Titlebar_TitleSelectionChanged;
        titlebar.TitleDropDownOpened += Titlebar_TitleDropDownOpened;
        projectService.Changed += ProjectService_Changed;
        localizationService.Changed += LocalizationService_Changed;
        Refresh();
    }

    internal void SetTitleState(FlourishTitleBarState state)
    {
        ArgumentNullException.ThrowIfNull(state);
        if (isDisposed)
        {
            return;
        }

        var selectorChanged =
            !StringComparer.Ordinal.Equals(titleState.ApplicationTitle, state.ApplicationTitle)
            || !StringComparer.Ordinal.Equals(
                titleState.UnnamedProjectPlaceholder,
                state.UnnamedProjectPlaceholder
            );
        titleState = state;
        if (selectorChanged)
        {
            Refresh();
        }
    }

    internal string GetDisplayedTitle(
        FlourishTitleBarState? state = null,
        FlourishProjectSnapshot? projectState = null
    )
    {
        state ??= titleState;
        projectState ??= projectService.Current;
        return projectState.IsMultiProjectEnabled
            ? projectState.ActiveProject is { } activeProject
                ? GetProjectDisplayTitle(activeProject, state)
                : state.UnnamedProjectPlaceholder
            : state.ApplicationTitle;
    }

    internal async ValueTask<CommandResult> SaveAsync(
        CommandContext context,
        CancellationToken cancellationToken
    )
    {
        if (isDisposed)
        {
            return CommandResult.Canceled;
        }

        if (!projectService.Current.IsMultiProjectEnabled)
        {
            return CommandResult.NotHandled;
        }

        using var linkedCancellation = CancellationTokenSource.CreateLinkedTokenSource(
            cancellationToken,
            lifetimeCancellation.Token
        );
        try
        {
            return await projectBehavior.SaveActiveProjectAsync(linkedCancellation.Token)
                ? CommandResult.Handled
                : CommandResult.NotHandled;
        }
        catch (OperationCanceledException) when (linkedCancellation.IsCancellationRequested)
        {
            return CommandResult.Canceled;
        }
        catch (Exception error)
        {
            ShowFailure("save", "Project save failed", error);
            return CommandResult.Failed(error);
        }
    }

    internal async ValueTask<WindowCloseDecision> CheckCloseAsync(
        CancellationToken cancellationToken
    )
    {
        if (isDisposed)
        {
            return WindowCloseDecision.Cancel;
        }

        if (!projectService.Current.IsMultiProjectEnabled)
        {
            return WindowCloseDecision.Allow;
        }

        using var linkedCancellation = CancellationTokenSource.CreateLinkedTokenSource(
            cancellationToken,
            lifetimeCancellation.Token
        );
        try
        {
            return await projectBehavior.CanCloseAsync(linkedCancellation.Token)
                ? WindowCloseDecision.Allow
                : WindowCloseDecision.Cancel;
        }
        catch (OperationCanceledException) when (lifetimeCancellation.IsCancellationRequested)
        {
            return WindowCloseDecision.Cancel;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception error)
        {
            ShowFailure("close", "Project close check failed", error);
            return WindowCloseDecision.Cancel;
        }
    }

    public void Dispose()
    {
        if (isDisposed)
        {
            return;
        }

        isDisposed = true;
        lifetimeCancellation.Cancel();
        titlebar.TitleSelectionChanged -= Titlebar_TitleSelectionChanged;
        titlebar.TitleDropDownOpened -= Titlebar_TitleDropDownOpened;
        projectService.Changed -= ProjectService_Changed;
        localizationService.Changed -= LocalizationService_Changed;
        foreach (var item in projectItemsById.Values)
        {
            RemoveDeleteHandler(item);
        }

        lifetimeCancellation.Dispose();
    }

    private void Refresh(
        FlourishProjectSnapshot? projectState = null
    )
    {
        if (isDisposed)
        {
            return;
        }

        projectState ??= projectService.Current;
        var hadKeyboardFocus = selector.IsKeyboardFocusWithin;
        var wasDropDownOpen = selector.IsDropDownOpen;
        BuildItems(projectState);
        if (!hadKeyboardFocus && !wasDropDownOpen)
        {
            return;
        }

        _ = dispatcher.BeginInvoke(
            DispatcherPriority.Input,
            new Action(() =>
            {
                if (isDisposed || selector.Visibility != Visibility.Visible)
                {
                    return;
                }

                if (hadKeyboardFocus || wasDropDownOpen)
                {
                    selector.Focus();
                }

                if (wasDropDownOpen)
                {
                    selector.IsDropDownOpen = true;
                }
            })
        );
    }

    private void BuildItems(FlourishProjectSnapshot projectState)
    {
        FlourishComboBoxItem? selectedItem = null;
        var desiredItems = new List<UIElement>(projectState.Projects.Count + 2);
        suppressSelectionChanged = true;
        try
        {
            var activeIds = projectState.Projects
                .Select(project => project.Id)
                .ToHashSet(StringComparer.Ordinal);
            foreach (
                var removedId in projectItemsById.Keys
                    .Where(id => !activeIds.Contains(id))
                    .ToArray()
            )
            {
                RemoveDeleteHandler(projectItemsById[removedId]);
                projectItemsById.Remove(removedId);
            }

            if (projectState.IsMultiProjectEnabled)
            {
                foreach (var project in projectState.Projects)
                {
                    if (!projectItemsById.TryGetValue(project.Id, out var item))
                    {
                        item = CreateProjectItem(project);
                        projectItemsById.Add(project.Id, item);
                    }
                    else
                    {
                        UpdateProjectItem(item, project);
                    }

                    desiredItems.Add(item);
                    if (
                        StringComparer.Ordinal.Equals(
                            projectState.ActiveProject?.Id,
                            project.Id
                        )
                    )
                    {
                        selectedItem = item;
                    }
                }

                if (selectedItem is null)
                {
                    selectedItem = projectPlaceholderItem ??= CreateProjectPlaceholderItem();
                    selectedItem.Content = titleState.UnnamedProjectPlaceholder;
                    desiredItems.Add(selectedItem);
                }

                newProjectItem ??= CreateNewProjectItem();
                var newProjectLabel = localizationService.Get(
                    FlourishLocaleKeys.TitleBarNewProject
                );
                newProjectItem.Content = newProjectLabel;
                AutomationProperties.SetName(newProjectItem, newProjectLabel);
                desiredItems.Add(newProjectItem);
            }
            else
            {
                selectedItem = applicationTitleItem ??= CreateApplicationTitleItem();
                selectedItem.Content = titleState.ApplicationTitle;
                AutomationProperties.SetName(selectedItem, titleState.ApplicationTitle);
                desiredItems.Add(selectedItem);
            }

            SynchronizeItems(selector, desiredItems);
            selector.SelectedItem = selectedItem;
        }
        finally
        {
            suppressSelectionChanged = false;
        }

        AutomationProperties.SetName(selector, GetDisplayedTitle(titleState, projectState));
    }

    private void UpdateProjectItem(FlourishComboBoxItem item, FlourishProject project)
    {
        var displayTitle = GetProjectDisplayTitle(project, titleState);
        if (!StringComparer.Ordinal.Equals(item.Content as string, displayTitle))
        {
            item.Content = displayTitle;
            AutomationProperties.SetName(item, displayTitle);
        }

        if (
            item.Tag is not ProjectMenuItemTag tag
            || tag.Kind != ProjectMenuItemKind.Project
            || !StringComparer.Ordinal.Equals(tag.ProjectId, project.Id)
        )
        {
            item.Tag = new ProjectMenuItemTag(ProjectMenuItemKind.Project, project.Id);
        }

        item.ToolTip = project.StoragePath;
        if (
            item.ContextMenu?.Items.Count > 0
            && item.ContextMenu.Items[0] is WpfMenuItem deleteItem
        )
        {
            deleteItem.Header = localizationService.Get(FlourishLocaleKeys.ProjectDelete);
            deleteItem.Tag = project.Id;
        }
    }

    private FlourishComboBoxItem CreateApplicationTitleItem()
    {
        var item = new FlourishComboBoxItem
        {
            Content = titleState.ApplicationTitle,
            Tag = new ProjectMenuItemTag(ProjectMenuItemKind.Application),
        };
        ConfigureDropDownItem(item);
        AutomationProperties.SetName(item, titleState.ApplicationTitle);
        return item;
    }

    private FlourishComboBoxItem CreateProjectItem(FlourishProject project)
    {
        var displayTitle = GetProjectDisplayTitle(project, titleState);
        var item = new FlourishComboBoxItem
        {
            Content = displayTitle,
            Tag = new ProjectMenuItemTag(ProjectMenuItemKind.Project, project.Id),
            ToolTip = project.StoragePath,
        };
        ConfigureDropDownItem(item);

        var deleteItem = new WpfMenuItem
        {
            Header = localizationService.Get(FlourishLocaleKeys.ProjectDelete),
            Tag = project.Id,
        };
        deleteItem.SetResourceReference(
            WpfControl.FontSizeProperty,
            "FlourishFontSizeStandard"
        );
        deleteItem.Click += ProjectDeleteMenuItem_Click;
        item.ContextMenu = new WpfContextMenu { Items = { deleteItem } };
        AutomationProperties.SetName(item, displayTitle);
        return item;
    }

    private FlourishComboBoxItem CreateProjectPlaceholderItem()
    {
        var item = new FlourishComboBoxItem
        {
            Content = titleState.UnnamedProjectPlaceholder,
            IsEnabled = false,
            Tag = new ProjectMenuItemTag(ProjectMenuItemKind.Placeholder),
        };
        ConfigureDropDownItem(item);
        return item;
    }

    private FlourishComboBoxItem CreateNewProjectItem()
    {
        var label = localizationService.Get(FlourishLocaleKeys.TitleBarNewProject);
        var item = new FlourishComboBoxItem
        {
            Content = label,
            Tag = new ProjectMenuItemTag(ProjectMenuItemKind.NewProject),
        };
        ConfigureDropDownItem(item);
        AutomationProperties.SetName(item, label);
        return item;
    }

    private static void ConfigureDropDownItem(FlourishComboBoxItem item)
    {
        item.SetResourceReference(WpfControl.FontSizeProperty, "FlourishFontSizeStandard");
        item.FontWeight = FontWeights.Normal;
    }

    private async void Titlebar_TitleSelectionChanged(
        object sender,
        SelectionChangedEventArgs e
    )
    {
        if (
            suppressSelectionChanged
            || selector.SelectedItem is not FlourishComboBoxItem selectedItem
            || selectedItem.Tag is not ProjectMenuItemTag selection
        )
        {
            return;
        }

        switch (selection.Kind)
        {
            case ProjectMenuItemKind.Project when selection.ProjectId is { } projectId:
                if (
                    StringComparer.Ordinal.Equals(
                        projectService.Current.ActiveProject?.Id,
                        projectId
                    )
                )
                {
                    return;
                }

                selector.IsDropDownOpen = false;
                await ExecuteBehaviorAsync(
                    "activate",
                    "Project activation failed",
                    token => projectBehavior.ActivateProjectAsync(projectId, token)
                );
                break;
            case ProjectMenuItemKind.NewProject:
                selector.IsDropDownOpen = false;
                await ExecuteBehaviorAsync(
                    "create",
                    "Project creation failed",
                    projectBehavior.CreateProjectAsync
                );
                break;
        }
    }

    private async void ProjectDeleteMenuItem_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not WpfMenuItem { Tag: string projectId })
        {
            return;
        }

        await ExecuteBehaviorAsync(
            "delete",
            "Project deletion failed",
            token => projectBehavior.DeleteProjectAsync(projectId, token)
        );
    }

    private async Task<bool> ExecuteBehaviorAsync(
        string operationId,
        string failureTitle,
        Func<CancellationToken, ValueTask<bool>> operation
    )
    {
        if (isProjectBehaviorPending || isDisposed)
        {
            return false;
        }

        isProjectBehaviorPending = true;
        selector.IsEnabled = false;
        try
        {
            return await operation(lifetimeCancellation.Token);
        }
        catch (OperationCanceledException) when (lifetimeCancellation.IsCancellationRequested)
        {
            return false;
        }
        catch (Exception error)
        {
            ShowFailure(operationId, failureTitle, error);
            return false;
        }
        finally
        {
            isProjectBehaviorPending = false;
            if (!isDisposed)
            {
                selector.IsEnabled = true;
            }
        }
    }

    private void ShowFailure(string operationId, string failureTitle, Exception error)
    {
        DispatchIfActive(() =>
        {
            notificationService.Upsert(
                new FlourishNotification(
                    $"flourish.project.{operationId}.error",
                    failureTitle,
                    error.Message,
                    FlourishNotificationSeverity.Error,
                    Duration: TimeSpan.FromSeconds(8)
                )
            );
        });
    }

    private void ProjectService_Changed(
        object? sender,
        FlourishProjectsChangedEventArgs e
    )
    {
        DispatchIfActive(() =>
        {
            if (e.Current.Version <= appliedProjectVersion)
            {
                return;
            }

            appliedProjectVersion = e.Current.Version;
            Refresh(e.Current);
            Changed?.Invoke(this, e);
        });
    }

    private void LocalizationService_Changed(
        object? sender,
        FlourishLocalizationChangedEventArgs e
    )
    {
        DispatchIfActive(() => Refresh());
    }

    private void Titlebar_TitleDropDownOpened(object? sender, EventArgs e) =>
        Opening?.Invoke(this, EventArgs.Empty);

    private void DispatchIfActive(Action action)
    {
        void ExecuteIfActive()
        {
            if (
                isDisposed
                || dispatcher.HasShutdownStarted
                || dispatcher.HasShutdownFinished
            )
            {
                return;
            }

            action();
        }

        if (dispatcher.CheckAccess())
        {
            ExecuteIfActive();
            return;
        }

        if (
            isDisposed
            || dispatcher.HasShutdownStarted
            || dispatcher.HasShutdownFinished
        )
        {
            return;
        }

        _ = dispatcher.BeginInvoke(
            DispatcherPriority.DataBind,
            new Action(ExecuteIfActive)
        );
    }

    private static string GetProjectDisplayTitle(
        FlourishProject project,
        FlourishTitleBarState titleState
    ) =>
        project.StoragePath is null
            ? titleState.UnnamedProjectPlaceholder
            : project.Name;

    private void RemoveDeleteHandler(FlourishComboBoxItem item)
    {
        if (
            item.ContextMenu?.Items.Count > 0
            && item.ContextMenu.Items[0] is WpfMenuItem deleteItem
        )
        {
            deleteItem.Click -= ProjectDeleteMenuItem_Click;
        }
    }

    private static void SynchronizeItems(
        ItemsControl itemsControl,
        IReadOnlyList<UIElement> desiredItems
    )
    {
        for (var index = 0; index < desiredItems.Count; index++)
        {
            var desired = desiredItems[index];
            if (
                index < itemsControl.Items.Count
                && ReferenceEquals(itemsControl.Items[index], desired)
            )
            {
                continue;
            }

            var existingIndex = itemsControl.Items.IndexOf(desired);
            if (existingIndex >= 0)
            {
                itemsControl.Items.RemoveAt(existingIndex);
            }

            itemsControl.Items.Insert(index, desired);
        }

        while (itemsControl.Items.Count > desiredItems.Count)
        {
            itemsControl.Items.RemoveAt(itemsControl.Items.Count - 1);
        }
    }

    private enum ProjectMenuItemKind
    {
        Application,
        Project,
        Placeholder,
        NewProject,
    }

    private sealed record ProjectMenuItemTag(
        ProjectMenuItemKind Kind,
        string? ProjectId = null
    );
}
