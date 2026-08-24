namespace ArkheideSystem.Flourish.Abstract;

/// <summary>
/// Describes why localized values changed.
/// </summary>
public enum FlourishLocalizationChangeKind
{
    /// <summary>
    /// The selected locale changed.
    /// </summary>
    LocaleChanged,

    /// <summary>
    /// A culture file was registered.
    /// </summary>
    FileRegistered,

    /// <summary>
    /// A registered culture file was reloaded.
    /// </summary>
    FileReloaded,

    /// <summary>
    /// A culture-file registration was removed.
    /// </summary>
    FileUnregistered,
}
