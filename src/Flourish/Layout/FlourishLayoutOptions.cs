namespace ArkheideSystem.Flourish.Layout;

internal sealed class FlourishLayoutOptions
{
    public bool IsSmoothScrollingEnabled { get; set; } = true;
    public bool IsCenterContentEnabled { get; set; }
    public double CenterContentWidth { get; set; }
    public bool UsePersistedSmoothScroll { get; set; } = true;
    public bool UsePersistedContentLayout { get; set; } = true;
}
