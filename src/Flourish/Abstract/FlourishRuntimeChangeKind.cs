namespace ArkheideSystem.Flourish.Abstract;

/// <summary>
/// Describes the kind of mutation that caused a Flourish runtime surface to change.
/// </summary>
public enum FlourishRuntimeChangeKind
{
    /// <summary>The complete state was replaced.</summary>
    Reset,

    /// <summary>An entry was added.</summary>
    Added,

    /// <summary>An existing entry was updated.</summary>
    Updated,

    /// <summary>An entry was removed.</summary>
    Removed,

    /// <summary>One or more entries were reordered.</summary>
    Moved,
}
