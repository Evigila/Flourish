namespace ArkheideSystem.Flourish.Blazor.Components;

/// <summary>Determines whether a popup accepts implicit dismissal.</summary>
public enum PopupStrength
{
    /// <summary>Outside interaction, Escape and repeated trigger activation may dismiss the popup.</summary>
    Weak,
    /// <summary>Only selection or an explicit completion/cancellation action dismisses the popup.</summary>
    Strong
}
