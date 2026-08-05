using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ArkheideSystem.Flourish.Abstract;
using ArkheideSystem.Gallery.Localization;

namespace ArkheideSystem.Gallery.Views;

public partial class ProjectRuntimePage : Page
{
    private readonly IProjectService projects;
    private readonly IProjectBehavior projectBehavior;
    private readonly ITitleBarService titleBar;
    private readonly IGalleryLocalization localization;
    private bool isRefreshing;

    public ProjectRuntimePage(
        IProjectService projects,
        IProjectBehavior projectBehavior,
        ITitleBarService titleBar,
        IGalleryLocalization localization
    )
    {
        this.projects = projects;
        this.projectBehavior = projectBehavior;
        this.titleBar = titleBar;
        this.localization = localization;
        InitializeComponent();

        Loaded += Page_Loaded;
        Unloaded += Page_Unloaded;
        RefreshState();
    }

    private void Page_Loaded(object sender, RoutedEventArgs e)
    {
        Page_Unloaded(sender, e);
        projects.Changed += Projects_Changed;
        projects.NewProjectRequested += Projects_NewProjectRequested;
        projects.ProjectActivationRequested += Projects_ProjectActivationRequested;
        titleBar.Changed += TitleBar_Changed;
        RefreshState();
    }

    private void Page_Unloaded(object sender, RoutedEventArgs e)
    {
        projects.Changed -= Projects_Changed;
        projects.NewProjectRequested -= Projects_NewProjectRequested;
        projects.ProjectActivationRequested -= Projects_ProjectActivationRequested;
        titleBar.Changed -= TitleBar_Changed;
    }

    private void Projects_Changed(object? sender, FlourishProjectsChangedEventArgs e) =>
        Dispatcher.BeginInvoke(RefreshState);

    private void TitleBar_Changed(object? sender, FlourishTitleBarChangedEventArgs e) =>
        Dispatcher.BeginInvoke(RefreshState);

    private void AppendProject_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var project = ReadProjectInput();
            projects.AppendProject(project);
            CollectionOutput.WriteLine(
                localization.Format(GalleryLocaleKeys.RuntimeAddedProject0_BC8EDEEB, project.Id)
            );
        }
        catch (Exception error)
        {
            CollectionOutput.WriteLine(
                localization.Format(GalleryLocaleKeys.DynamicError0_43F78154, error.Message)
            );
        }

        RefreshState();
    }

    private void SetProject_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var project = ReadProjectInput();
            projects.SetProject(project);
            CollectionOutput.WriteLine(
                localization.Format(
                    GalleryLocaleKeys.RuntimeAddedOrReplacedProject0_652E70C6,
                    project.Id
                )
            );
        }
        catch (Exception error)
        {
            CollectionOutput.WriteLine(
                localization.Format(GalleryLocaleKeys.DynamicError0_43F78154, error.Message)
            );
        }

        RefreshState();
    }

    private void FindProject_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (projects.GetProject(ProjectIdBox.Text) is { } project)
            {
                CollectionOutput.WriteLine(
                    localization.Format(
                        GalleryLocaleKeys.RuntimeFound01At2_56F0265D,
                        project.Name,
                        project.Id,
                        project.StoragePath
                            ?? localization.Get(GalleryLocaleKeys.RuntimeNoStoragePath_59132F06)
                    )
                );
            }
            else
            {
                CollectionOutput.WriteLine(
                    localization.Format(
                        GalleryLocaleKeys.RuntimeProject0WasNotFound_F552F4FC,
                        ProjectIdBox.Text.Trim()
                    )
                );
            }
        }
        catch (Exception error)
        {
            CollectionOutput.WriteLine(
                localization.Format(GalleryLocaleKeys.DynamicError0_43F78154, error.Message)
            );
        }

        RefreshState();
    }

    private async void ActiveProjectBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (isRefreshing || ActiveProjectBox.SelectedItem is not FlourishProject project)
        {
            return;
        }

        PopulateProjectInput(project);
        try
        {
            var activated = await projectBehavior.ActivateProjectAsync(project.Id);
            ActiveProjectOutput.WriteLine(
                activated
                    ? localization.Format(
                        GalleryLocaleKeys.RuntimeActivatedProject0_A141BFAE,
                        project.Id
                    )
                    : localization.Format(
                        GalleryLocaleKeys.RuntimeActivationOfProject0WasCanceled_D2FBA00D,
                        project.Id
                    )
            );
        }
        catch (Exception error)
        {
            ActiveProjectOutput.WriteLine(
                localization.Format(GalleryLocaleKeys.DynamicError0_43F78154, error.Message)
            );
        }

        RefreshState();
    }

    private void UpdateMetadata_Click(object sender, RoutedEventArgs e)
    {
        if (ActiveProjectBox.SelectedItem is not FlourishProject project)
        {
            ActiveProjectOutput.WriteLine(
                localization.Get(
                    GalleryLocaleKeys.RuntimeSelectAProjectBeforeUpdatingItsMetadata_5DB9165E
                )
            );
            RefreshState();
            return;
        }

        try
        {
            projects.SetProjectMetadata(
                project.Id,
                ProjectNameBox.Text,
                ReadExistingStoragePath(StoragePathBox.Text)
            );
            ActiveProjectOutput.WriteLine(
                localization.Format(
                    GalleryLocaleKeys.RuntimeUpdatedMetadataForProject0_2AED78D9,
                    project.Id
                )
            );
        }
        catch (Exception error)
        {
            ActiveProjectOutput.WriteLine(
                localization.Format(GalleryLocaleKeys.DynamicError0_43F78154, error.Message)
            );
        }

        RefreshState();
    }

    private void ClearActiveProject_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            projects.SetActiveProject(null);
            ActiveProjectOutput.WriteLine(
                localization.Get(GalleryLocaleKeys.RuntimeClearedTheActiveProject_CD1CD5F9)
            );
        }
        catch (Exception error)
        {
            ActiveProjectOutput.WriteLine(
                localization.Format(GalleryLocaleKeys.DynamicError0_43F78154, error.Message)
            );
        }

        RefreshState();
    }

    private async void RemoveProject_Click(object sender, RoutedEventArgs e)
    {
        if (ActiveProjectBox.SelectedItem is not FlourishProject project)
        {
            ActiveProjectOutput.WriteLine(
                localization.Get(GalleryLocaleKeys.RuntimeSelectAProjectBeforeDeletingIt_2E30E35D)
            );
            RefreshState();
            return;
        }

        try
        {
            var deleted = await projectBehavior.DeleteProjectAsync(project.Id);
            ActiveProjectOutput.WriteLine(
                deleted
                    ? localization.Format(
                        GalleryLocaleKeys.RuntimeDeletedProject0_0AABCF44,
                        project.Id
                    )
                    : localization.Format(
                        GalleryLocaleKeys.RuntimeDeletionOfProject0WasCanceled_EC73BFA7,
                        project.Id
                    )
            );
        }
        catch (Exception error)
        {
            ActiveProjectOutput.WriteLine(
                localization.Format(GalleryLocaleKeys.DynamicError0_43F78154, error.Message)
            );
        }

        RefreshState();
    }

    private void MultiProjectEnabledBox_Changed(object sender, RoutedEventArgs e)
    {
        if (CanApplyImmediately)
        {
            try
            {
                projects.SetMultiProjectEnabled(MultiProjectEnabledBox.IsChecked == true);
                RequestOutput.WriteLine(
                    MultiProjectEnabledBox.IsChecked == true
                        ? localization.Get(
                            GalleryLocaleKeys.RuntimeEnabledTheProjectAwareTitleSelector_9113E219
                        )
                        : localization.Get(
                            GalleryLocaleKeys.RuntimeDisabledProjectAwareTitleDisplayProjectMetadataRemainsRegistered_EF99C5FD
                        )
                );
            }
            catch (Exception error)
            {
                RequestOutput.WriteLine(
                    localization.Format(GalleryLocaleKeys.DynamicError0_43F78154, error.Message)
                );
            }

            RefreshState();
        }
    }

    private void UnnamedProjectPlaceholderBox_LostFocus(object sender, RoutedEventArgs e) =>
        CommitUnnamedProjectPlaceholder();

    private void UnnamedProjectPlaceholderBox_KeyDown(object sender, KeyEventArgs e) =>
        CommitOnEnter(e, CommitUnnamedProjectPlaceholder);

    private void CommitUnnamedProjectPlaceholder()
    {
        if (!CanApplyImmediately)
        {
            return;
        }

        try
        {
            titleBar.SetUnnamedProjectPlaceholder(UnnamedProjectPlaceholderBox.Text);
            RequestOutput.WriteLine(
                localization.Format(
                    GalleryLocaleKeys.RuntimeUpdatedTheUnnamedProjectTitleTo0_DEA44997,
                    titleBar.Current.UnnamedProjectPlaceholder
                )
            );
        }
        catch (Exception error)
        {
            RequestOutput.WriteLine(
                localization.Format(GalleryLocaleKeys.DynamicError0_43F78154, error.Message)
            );
        }

        RefreshState();
    }

    private void Projects_NewProjectRequested(
        object? sender,
        FlourishNewProjectRequestedEventArgs e
    )
    {
        Dispatcher.BeginInvoke(() =>
        {
            RequestOutput.WriteLine(
                localization.Get(
                    GalleryLocaleKeys.RuntimeObservedANewProjectRequestFromTheTitleSelector_55EB08E8
                )
            );
            RefreshState();
        });
    }

    private void Projects_ProjectActivationRequested(
        object? sender,
        FlourishProjectActivationRequestedEventArgs e
    )
    {
        Dispatcher.BeginInvoke(() =>
        {
            RequestOutput.WriteLine(
                localization.Format(
                    GalleryLocaleKeys.RuntimeObservedAnActivationRequestFor01_21C02866,
                    e.Project.Name,
                    e.Project.Id
                )
            );
            RefreshState();
        });
    }

    private async void CreateProject_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var created = await projectBehavior.CreateProjectAsync();
            RequestOutput.WriteLine(
                created
                    ? localization.Get(GalleryLocaleKeys.RuntimeCreatedAPersistedProject_959B70B9)
                    : localization.Get(GalleryLocaleKeys.RuntimeProjectCreationWasCanceled_5BE63576)
            );
        }
        catch (Exception error)
        {
            RequestOutput.WriteLine(
                localization.Format(GalleryLocaleKeys.DynamicError0_43F78154, error.Message)
            );
        }

        RefreshState();
    }

    private async void SaveActiveProject_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var saved = await projectBehavior.SaveActiveProjectAsync();
            RequestOutput.WriteLine(
                saved
                    ? localization.Get(GalleryLocaleKeys.RuntimeSavedTheActiveProject_1253EB9E)
                    : localization.Get(GalleryLocaleKeys.RuntimeProjectSaveWasCanceled_409777D2)
            );
        }
        catch (Exception error)
        {
            RequestOutput.WriteLine(
                localization.Format(GalleryLocaleKeys.DynamicError0_43F78154, error.Message)
            );
        }

        RefreshState();
    }

    private FlourishProject ReadProjectInput() =>
        new(ProjectIdBox.Text, ProjectNameBox.Text, ReadExistingStoragePath(StoragePathBox.Text));

    private void PopulateProjectInput(FlourishProject project)
    {
        ProjectIdBox.Text = project.Id;
        ProjectNameBox.Text = project.Name;
        StoragePathBox.Text = project.StoragePath ?? string.Empty;
    }

    private void RefreshState()
    {
        isRefreshing = true;
        try
        {
            var current = projects.Current;
            ActiveProjectBox.ItemsSource = current.Projects;
            ActiveProjectBox.SelectedItem = current.ActiveProject;
            MultiProjectEnabledBox.IsChecked = current.IsMultiProjectEnabled;
            UnnamedProjectPlaceholderBox.Text = titleBar.Current.UnnamedProjectPlaceholder;
            ProjectCollectionControls.IsEnabled = current.IsMultiProjectEnabled;
            ActiveProjectControls.IsEnabled = current.IsMultiProjectEnabled;
            MultiProjectBehaviorControls.IsEnabled = current.IsMultiProjectEnabled;
        }
        finally
        {
            isRefreshing = false;
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

    private static string ReadExistingStoragePath(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidOperationException(
                "Select an existing local project file before registering the project."
            );
        }

        var storagePath = Path.GetFullPath(value.Trim());
        if (!File.Exists(storagePath))
        {
            throw new FileNotFoundException(
                "The selected local project file does not exist.",
                storagePath
            );
        }

        return storagePath;
    }
}
