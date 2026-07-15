namespace ArkheideSystem.Flourish.Controls;

/// <summary>
/// Describes the presenter's position relative to the opposing textual content.
/// </summary>
public enum PresenterPosition
{
    /// <summary>The presenter is placed to the left of the textual content.</summary>
    Left,

    /// <summary>The presenter is placed at the upper-left, opposite lower-right text.</summary>
    LeftTop,

    /// <summary>The presenter is placed at the lower-left, opposite upper-right text.</summary>
    LeftBottom,

    /// <summary>The presenter is placed above the textual content.</summary>
    Top,

    /// <summary>The presenter is placed below the textual content.</summary>
    Bottom,

    /// <summary>The presenter is placed to the right of the textual content.</summary>
    Right,

    /// <summary>The presenter is placed at the upper-right, opposite lower-left text.</summary>
    RightTop,

    /// <summary>The presenter is placed at the lower-right, opposite upper-left text.</summary>
    RightBottom,
}
