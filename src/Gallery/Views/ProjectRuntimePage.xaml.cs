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
            CollectionOutput.WriteLine(localization.Format("Added project '{0}'.", project.Id));
        }
        catch (Exception error)
        {
            CollectionOutput.WriteLine(localization.Format("Error: {0}", error.Message));
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
                localization.Format("Added or replaced project '{0}'.", project.Id)
            );
        }
        catch (Exception error)
        {
            CollectionOutput.WriteLine(localization.Format("Error: {0}", error.Message));
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
                        "Found '{0}' [{1}] at {2}.",
                        project.Name,
                        project.Id,
                        project.StoragePath ?? localization.Get("<no storage path>")
                    )
                );
            }
            else
            {
                CollectionOutput.WriteLine(
                    localization.Format(
                        "Project '{0}' was not found.",
                        ProjectIdBox.Text.Trim()
                    )
                );
            }
        }
        catch (Exception error)
        {
            CollectionOutput.WriteLine(localization.Format("Error: {0}", error.Message));
        }

        RefreshState();
    }

    private async void ActiveProjectBox_SelectionChanged(
        object sender,
        SelectionChangedEventArgs e
    )
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
                    ? localization.Format("Activated project '{0}'.", project.Id)
                    : localization.Format(
                        "Activation of project '{0}' was canceled.",
                        project.Id
                    )
            );
        }
        catch (Exception error)
        {
            ActiveProjectOutput.WriteLine(localization.Format("Error: {0}", error.Message));
        }

        RefreshState();
    }

    private void UpdateMetadata_Click(object sender, RoutedEventArgs e)
    {
        if (ActiveProjectBox.SelectedItem is not FlourishProject project)
        {
            ActiveProjectOutput.WriteLine(
                localization.Get("Select a project before updating its metadata.")
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
                localization.Format("Updated metadata for project '{0}'.", project.Id)
            );
        }
        catch (Exception error)
        {
            ActiveProjectOutput.WriteLine(localization.Format("Error: {0}", error.Message));
        }

        RefreshState();
    }

    private void ClearActiveProject_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            projects.SetActiveProject(null);
            ActiveProjectOutput.WriteLine(localization.Get("Cleared the active project."));
        }
        catch (Exception error)
        {
            ActiveProjectOutput.WriteLine(localization.Format("Error: {0}", error.Message));
        }

        RefreshState();
    }

    private async void RemoveProject_Click(object sender, RoutedEventArgs e)
    {
        if (ActiveProjectBox.SelectedItem is not FlourishProject project)
        {
            ActiveProjectOutput.WriteLine(
                localization.Get("Select a project before deleting it.")
            );
            RefreshState();
            return;
        }

        try
        {
            var deleted = await projectBehavior.DeleteProjectAsync(project.Id);
            ActiveProjectOutput.WriteLine(
                deleted
                    ? localization.Format("Deleted project '{0}'.", project.Id)
                    : localization.Format(
                        "Deletion of project '{0}' was canceled.",
                        project.Id
                    )
            );
        }
        catch (Exception error)
        {
            ActiveProjectOutput.WriteLine(localization.Format("Error: {0}", error.Message));
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
                        ? localization.Get("Enabled the project-aware title selector.")
                        : localization.Get(
                            "Disabled project-aware title display; project metadata remains registered."
                        )
                );
            }
            catch (Exception error)
            {
                RequestOutput.WriteLine(localization.Format("Error: {0}", error.Message));
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
                    "Updated the unnamed-project title to '{0}'.",
                    titleBar.Current.UnnamedProjectPlaceholder
                )
            );
        }
        catch (Exception error)
        {
            RequestOutput.WriteLine(localization.Format("Error: {0}", error.Message));
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
                localization.Get("Observed a new-project request from the title selector.")
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
                    "Observed an activation request for '{0}' [{1}].",
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
                    ? localization.Get("Created a persisted project.")
                    : localization.Get("Project creation was canceled.")
            );
        }
        catch (Exception error)
        {
            RequestOutput.WriteLine(localization.Format("Error: {0}", error.Message));
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
                    ? localization.Get("Saved the active project.")
                    : localization.Get("Project save was canceled.")
            );
        }
        catch (Exception error)
        {
            RequestOutput.WriteLine(localization.Format("Error: {0}", error.Message));
        }

        RefreshState();
    }

    private FlourishProject ReadProjectInput() =>
        new(
            ProjectIdBox.Text,
            ProjectNameBox.Text,
            ReadExistingStoragePath(StoragePathBox.Text)
        );

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
