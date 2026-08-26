namespace ArkheideSystem.Flourish.Abstract;

/// <summary>Configures shell content layout and scrolling defaults.</summary>
public interface ILayoutBuilder
{
    /// <summary>Enables or disables centered, width-limited page content.</summary>
    ILayoutBuilder SetCenterContent(
        bool enabled = true,
        double contentWidth = 1200,
        bool usePersistedPreference = true
    );

    /// <summary>Enables or disables smooth scrolling for built-in viewports.</summary>
    ILayoutBuilder SetSmoothScrollingEnabled(
        bool enabled = true,
        bool usePersistedPreference = true
    );
}
