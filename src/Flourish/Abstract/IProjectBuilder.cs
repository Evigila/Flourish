namespace ArkheideSystem.Flourish.Abstract;

/// <summary>Configures the optional multi-project shell behavior.</summary>
public interface IProjectBuilder
{
    /// <summary>Enables or disables project-aware shell behavior.</summary>
    IProjectBuilder SetMultiProjectEnabled(bool enabled = true);
}
