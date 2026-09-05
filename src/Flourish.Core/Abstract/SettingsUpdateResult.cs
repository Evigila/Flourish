namespace ArkheideSystem.Flourish.Abstract;

/// <summary>
/// Reports the outcome of a settings transaction.
/// </summary>
public sealed record SettingsUpdateResult(
    string FilePath,
    bool Changed,
    bool ConfigurationReloaded
);
