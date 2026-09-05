using System;

using CKey = ArkheideSystem.Essential.Culture.Key;
using Localizer = ArkheideSystem.Essential.Culture.Localizer;
using InputKey = System.Windows.Input.Key;
using ArkheideSystem.Flourish.Abstract;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace ArkheideSystem.Gallery.WPF.Views;

public partial class ProjectRuntimePage : Page
{
    private readonly IProjectService projects;
    private readonly IProjectBehavior projectBehavior;
    private readonly ITitleBarService titleBar;
    private bool isRefreshing;

    public ProjectRuntimePage(
        IProjectService projects,
        IProjectBehavior projectBehavior,
        ITitleBarService titleBar
    )
    {
        this.projects = projects;
        this.projectBehavior = projectBehavior;
        this.titleBar = titleBar;
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

    private void Projects_Changed(object? sender, ProjectCatalogChangedEventArgs e) =>
        Dispatcher.BeginInvoke(RefreshState);

    private void TitleBar_Changed(object? sender, StateChangedEventArgs<TitleBarState> e) =>
        Dispatcher.BeginInvoke(RefreshState);

    private void AddProject_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var project = ReadProjectInput();
            projects.AddProject(project);
            CollectionOutput.WriteLine(
                Localizer.Parse(CKey.Runtime_AddedProject0_BC8EDEEB, project.Id)
            );
        }
        catch (Exception error)
        {
            CollectionOutput.WriteLine(
                Localizer.Parse(CKey.Dynamic_Error0_43F78154, error.Message)
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
                Localizer.Parse(CKey.Runtime_AddedOrReplacedProject0_652E70C6, project.Id)
            );
        }
        catch (Exception error)
        {
            CollectionOutput.WriteLine(
                Localizer.Parse(CKey.Dynamic_Error0_43F78154, error.Message)
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
                    Localizer.Parse(
                        CKey.Runtime_Found01At2_56F0265D,
                        project.Name,
                        project.Id,
                        project.StoragePath
                            ?? Localizer.Parse(CKey.Runtime_NoStoragePath_59132F06)
                    )
                );
            }
            else
            {
                CollectionOutput.WriteLine(
                    Localizer.Parse(
                        CKey.Runtime_Project0WasNotFound_F552F4FC,
                        ProjectIdBox.Text.Trim()
                    )
                );
            }
        }
        catch (Exception error)
        {
            CollectionOutput.WriteLine(
                Localizer.Parse(CKey.Dynamic_Error0_43F78154, error.Message)
            );
        }

        RefreshState();
    }

    private async void ActiveProjectBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (isRefreshing || ActiveProjectBox.SelectedItem is not ProjectDescriptor project)
        {
            return;
        }

        PopulateProjectInput(project);
        try
        {
            var activated = await projectBehavior.ActivateProjectAsync(project.Id);
            ActiveProjectOutput.WriteLine(
                activated
                    ? Localizer.Parse(CKey.Runtime_ActivatedProject0_A141BFAE, project.Id)
                    : Localizer.Parse(
                        CKey.Runtime_ActivationOfProject0WasCanceled_D2FBA00D,
                        project.Id
                    )
            );
        }
        catch (Exception error)
        {
            ActiveProjectOutput.WriteLine(
                Localizer.Parse(CKey.Dynamic_Error0_43F78154, error.Message)
            );
        }

        RefreshState();
    }

    private void UpdateMetadata_Click(object sender, RoutedEventArgs e)
    {
        if (ActiveProjectBox.SelectedItem is not ProjectDescriptor project)
        {
            ActiveProjectOutput.WriteLine(
                Localizer.Parse(CKey.Runtime_SelectAProjectBeforeUpdatingItsMetadata_5DB9165E)
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
                Localizer.Parse(CKey.Runtime_UpdatedMetadataForProject0_2AED78D9, project.Id)
            );
        }
        catch (Exception error)
        {
            ActiveProjectOutput.WriteLine(
                Localizer.Parse(CKey.Dynamic_Error0_43F78154, error.Message)
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
                Localizer.Parse(CKey.Runtime_ClearedTheActiveProject_CD1CD5F9)
            );
        }
        catch (Exception error)
        {
            ActiveProjectOutput.WriteLine(
                Localizer.Parse(CKey.Dynamic_Error0_43F78154, error.Message)
            );
        }

        RefreshState();
    }

    private async void RemoveProject_Click(object sender, RoutedEventArgs e)
    {
        if (ActiveProjectBox.SelectedItem is not ProjectDescriptor project)
        {
            ActiveProjectOutput.WriteLine(
                Localizer.Parse(CKey.Runtime_SelectAProjectBeforeDeletingIt_2E30E35D)
            );
            RefreshState();
            return;
        }

        try
        {
            var deleted = await projectBehavior.DeleteProjectAsync(project.Id);
            ActiveProjectOutput.WriteLine(
                deleted
                    ? Localizer.Parse(CKey.Runtime_DeletedProject0_0AABCF44, project.Id)
                    : Localizer.Parse(
                        CKey.Runtime_DeletionOfProject0WasCanceled_EC73BFA7,
                        project.Id
                    )
            );
        }
        catch (Exception error)
        {
            ActiveProjectOutput.WriteLine(
                Localizer.Parse(CKey.Dynamic_Error0_43F78154, error.Message)
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
                        ? Localizer.Parse(
                            CKey.Runtime_EnabledTheProjectAwareTitleSelector_9113E219
                        )
                        : Localizer.Parse(
                            CKey.Runtime_DisabledProjectAwareTitleDisplayProjectMetadataRemainsRegistered_EF99C5FD
                        )
                );
            }
            catch (Exception error)
            {
                RequestOutput.WriteLine(
                    Localizer.Parse(CKey.Dynamic_Error0_43F78154, error.Message)
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
                Localizer.Parse(
                    CKey.Runtime_UpdatedTheUnnamedProjectTitleTo0_DEA44997,
                    titleBar.Current.UnnamedProjectPlaceholder
                )
            );
        }
        catch (Exception error)
        {
            RequestOutput.WriteLine(
                Localizer.Parse(CKey.Dynamic_Error0_43F78154, error.Message)
            );
        }

        RefreshState();
    }

    private void Projects_NewProjectRequested(
        object? sender,
        ProjectCreationRequestedEventArgs e
    )
    {
        Dispatcher.BeginInvoke(() =>
        {
            RequestOutput.WriteLine(
                Localizer.Parse(
                    CKey.Runtime_ObservedANewProjectRequestFromTheTitleSelector_55EB08E8
                )
            );
            RefreshState();
        });
    }

    private void Projects_ProjectActivationRequested(
        object? sender,
        ProjectActivationRequestedEventArgs e
    )
    {
        Dispatcher.BeginInvoke(() =>
        {
            RequestOutput.WriteLine(
                Localizer.Parse(
                    CKey.Runtime_ObservedAnActivationRequestFor01_21C02866,
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
                    ? Localizer.Parse(CKey.Runtime_CreatedAPersistedProject_959B70B9)
                    : Localizer.Parse(CKey.Runtime_ProjectCreationWasCanceled_5BE63576)
            );
        }
        catch (Exception error)
        {
            RequestOutput.WriteLine(
                Localizer.Parse(CKey.Dynamic_Error0_43F78154, error.Message)
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
                    ? Localizer.Parse(CKey.Runtime_SavedTheActiveProject_1253EB9E)
                    : Localizer.Parse(CKey.Runtime_ProjectSaveWasCanceled_409777D2)
            );
        }
        catch (Exception error)
        {
            RequestOutput.WriteLine(
                Localizer.Parse(CKey.Dynamic_Error0_43F78154, error.Message)
            );
        }

        RefreshState();
    }

    private ProjectDescriptor ReadProjectInput() =>
        new(ProjectIdBox.Text, ProjectNameBox.Text, ReadExistingStoragePath(StoragePathBox.Text));

    private void PopulateProjectInput(ProjectDescriptor project)
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
        if (e.Key != InputKey.Enter)
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
            throw new InvalidOperationException("Select a project file.");
        }

        var storagePath = Path.GetFullPath(value.Trim());
        if (!File.Exists(storagePath))
        {
            throw new FileNotFoundException(
                "Project file not found.",
                storagePath
            );
        }

        return storagePath;
    }
}
