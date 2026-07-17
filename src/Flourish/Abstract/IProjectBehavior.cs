namespace ArkheideSystem.Flourish.Abstract;

/// <summary>
/// Coordinates the user-facing project lifecycle used by the Flourish shell.
/// </summary>
/// <remarks>
/// Applications can replace this service through dependency injection when project creation,
/// persistence, activation, deletion, or close behavior is owned by the application.
/// </remarks>
public interface IProjectBehavior
{
    /// <summary>Creates and activates a project.</summary>
    /// <param name="cancellationToken">A token that cancels the operation.</param>
    /// <returns>
    /// <see langword="true" /> when the project was created; otherwise,
    /// <see langword="false" /> when the operation was canceled.
    /// </returns>
    ValueTask<bool> CreateProjectAsync(CancellationToken cancellationToken = default);

    /// <summary>Saves the active project when it requires framework-managed persistence.</summary>
    /// <param name="cancellationToken">A token that cancels the operation.</param>
    /// <returns>
    /// <see langword="true" /> when saving completed or no framework-managed save was required;
    /// otherwise, <see langword="false" /> when the operation was canceled or no project is active.
    /// </returns>
    ValueTask<bool> SaveActiveProjectAsync(CancellationToken cancellationToken = default);

    /// <summary>Activates a project after resolving any unsaved active project.</summary>
    /// <param name="projectId">The case-sensitive project ID to activate.</param>
    /// <param name="cancellationToken">A token that cancels the operation.</param>
    /// <returns>
    /// <see langword="true" /> when the project was activated; otherwise,
    /// <see langword="false" /> when the operation was canceled or the project no longer exists.
    /// </returns>
    ValueTask<bool> ActivateProjectAsync(
        string projectId,
        CancellationToken cancellationToken = default
    );

    /// <summary>Deletes a project after confirmation.</summary>
    /// <param name="projectId">The case-sensitive project ID to delete.</param>
    /// <param name="cancellationToken">A token that cancels the operation.</param>
    /// <returns>
    /// <see langword="true" /> when the project was deleted; otherwise,
    /// <see langword="false" /> when the operation was canceled or the project no longer exists.
    /// </returns>
    ValueTask<bool> DeleteProjectAsync(
        string projectId,
        CancellationToken cancellationToken = default
    );

    /// <summary>Determines whether the active project allows the shell to close.</summary>
    /// <param name="cancellationToken">A token that cancels the operation.</param>
    /// <returns>
    /// <see langword="true" /> when closing may continue; otherwise,
    /// <see langword="false" /> when the user canceled the required save.
    /// </returns>
    ValueTask<bool> CanCloseAsync(CancellationToken cancellationToken = default);
}
