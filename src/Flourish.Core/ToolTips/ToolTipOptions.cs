namespace ArkheideSystem.Flourish.ToolTips;

internal sealed class ToolTipOptions
{
    public bool IsTipsEnabled { get; set; }

    public int InitialShowDelayMilliseconds { get; set; } = 200;

    public double SpawnableMargin { get; set; } = 5;
}
