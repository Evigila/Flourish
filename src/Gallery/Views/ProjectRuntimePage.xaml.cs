using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using ArkheideSystem.Flourish.Abstract;

namespace ArkheideSystem.Gallery.Views;

public partial class ProjectRuntimePage : Page
{
    private readonly IProjectService projects;
    private readonly ITitleBarService titleBar;
    private bool isRefreshing;
    private int untitledProjectSequence = 1;

    public ProjectRuntimePage(IProjectService projects, ITitleBarService titleBar)
    {
        this.projects = projects;
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

    private void Projects_Changed(object? sender, FlourishProjectsChangedEventArgs e) =>
        Dispatcher.BeginInvoke(RefreshState);

    private void TitleBar_Changed(object? sender, FlourishTitleBarChangedEventArgs e) =>
        Dispatcher.BeginInvoke(RefreshState);

    private void AddProject_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var project = ReadProjectInput();
            projects.AddProject(project);
            CollectionOutput.WriteLine($"Added project '{project.Id}'.");
        }
        catch (Exception error)
        {
            CollectionOutput.WriteLine($"Error: {error.Message}");
        }

        RefreshState();
    }

    private void UpsertProject_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var project = ReadProjectInput();
            projects.UpsertProject(project);
            CollectionOutput.WriteLine($"Added or replaced project '{project.Id}'.");
        }
        catch (Exception error)
        {
            CollectionOutput.WriteLine($"Error: {error.Message}");
        }

        RefreshState();
    }

    private void FindProject_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            if (
                projects.TryGetProject(ProjectIdBox.Text, out var project)
                && project is not null
            )
            {
                CollectionOutput.WriteLine(
                    $"Found '{project.Name}' [{project.Id}] at {project.StoragePath ?? "<no storage path>"}."
                );
            }
            else
            {
                CollectionOutput.WriteLine(
                    $"Project '{ProjectIdBox.Text.Trim()}' was not found."
                );
            }
        }
        catch (Exception error)
        {
            CollectionOutput.WriteLine($"Error: {error.Message}");
        }

        RefreshState();
    }

    private void ActiveProjectBox_SelectionChanged(
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
            projects.SetActiveProject(project.Id);
            ActiveProjectOutput.WriteLine($"Activated project '{project.Id}'.");
        }
        catch (Exception error)
        {
            ActiveProjectOutput.WriteLine($"Error: {error.Message}");
        }

        RefreshState();
    }

    private void UpdateMetadata_Click(object sender, RoutedEventArgs e)
    {
        if (ActiveProjectBox.SelectedItem is not FlourishProject project)
        {
            ActiveProjectOutput.WriteLine("Select a project before updating its metadata.");
            RefreshState();
            return;
        }

        try
        {
            projects.SetProjectMetadata(
                project.Id,
                ProjectNameBox.Text,
                NullIfWhiteSpace(StoragePathBox.Text)
            );
            ActiveProjectOutput.WriteLine($"Updated metadata for project '{project.Id}'.");
        }
        catch (Exception error)
        {
            ActiveProjectOutput.WriteLine($"Error: {error.Message}");
        }

        RefreshState();
    }

    private void ClearActiveProject_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            projects.SetActiveProject(null);
            ActiveProjectOutput.WriteLine("Cleared the active project.");
        }
        catch (Exception error)
        {
            ActiveProjectOutput.WriteLine($"Error: {error.Message}");
        }

        RefreshState();
    }

    private void RemoveProject_Click(object sender, RoutedEventArgs e)
    {
        if (ActiveProjectBox.SelectedItem is not FlourishProject project)
        {
            ActiveProjectOutput.WriteLine("Select a project before deleting it.");
            RefreshState();
            return;
        }

        try
        {
            ActiveProjectOutput.WriteLine(
                projects.RemoveProject(project.Id)
                    ? $"Deleted project '{project.Id}'."
                    : $"Project '{project.Id}' was not registered."
            );
        }
        catch (Exception error)
        {
            ActiveProjectOutput.WriteLine($"Error: {error.Message}");
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
                        ? "Enabled the project-aware title button."
                        : "Disabled project-aware title display; project metadata remains registered."
                );
            }
            catch (Exception error)
            {
                RequestOutput.WriteLine($"Error: {error.Message}");
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
                $"Updated the unnamed-project title to '{titleBar.Current.UnnamedProjectPlaceholder}'."
            );
        }
        catch (Exception error)
        {
            RequestOutput.WriteLine($"Error: {error.Message}");
        }

        RefreshState();
    }

    private void Projects_NewProjectRequested(
        object? sender,
        FlourishNewProjectRequestedEventArgs e
    )
    {
        if (HandleTitleBarRequestsBox.IsChecked != true)
        {
            RequestOutput.WriteLine("Observed a new-project request without handling it.");
            RefreshState();
            return;
        }

        try
        {
            var sequence = NextUntitledProjectSequence();
            var project = new FlourishProject(
                $"untitled-{sequence}",
                $"Untitled project {sequence}"
            );
            projects.AddProject(project);
            PopulateProjectInput(project);
            RequestOutput.WriteLine(
                $"Handled the title-bar request by adding '{project.Name}' [{project.Id}]."
            );
        }
        catch (Exception error)
        {
            RequestOutput.WriteLine($"Error: {error.Message}");
        }

        RefreshState();
    }

    private void Projects_ProjectActivationRequested(
        object? sender,
        FlourishProjectActivationRequestedEventArgs e
    )
    {
        if (HandleTitleBarRequestsBox.IsChecked != true)
        {
            RequestOutput.WriteLine(
                $"Observed an activation request for '{e.Project.Name}' without handling it."
            );
            RefreshState();
            return;
        }

        try
        {
            projects.SetActiveProject(e.Project.Id);
            PopulateProjectInput(e.Project);
            RequestOutput.WriteLine(
                $"Handled the title-bar request by activating '{e.Project.Name}' [{e.Project.Id}]."
            );
        }
        catch (Exception error)
        {
            RequestOutput.WriteLine($"Error: {error.Message}");
        }

        RefreshState();
    }

    private int NextUntitledProjectSequence()
    {
        while (
            projects.TryGetProject(
                $"untitled-{untitledProjectSequence}",
                out _
            )
        )
        {
            untitledProjectSequence++;
        }

        return untitledProjectSequence++;
    }

    private FlourishProject ReadProjectInput() =>
        new(
            ProjectIdBox.Text,
            ProjectNameBox.Text,
            NullIfWhiteSpace(StoragePathBox.Text)
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

    private static string? NullIfWhiteSpace(string value) =>
        string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
