namespace ArkheideSystem.Flourish.Abstract;

/// <summary>Configures Flourish-owned tooltip presentation.</summary>
public interface IToolTipBuilder
{
    /// <summary>Enables or disables Flourish tooltip presentation.</summary>
    IToolTipBuilder SetEnabled(bool enabled = true);

    /// <summary>Sets tooltip timing and placement margin.</summary>
    IToolTipBuilder SetSettings(int initialShowDelayMilliseconds = 200, double spawnableMargin = 5);
}
