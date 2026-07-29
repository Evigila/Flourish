namespace ArkheideSystem.Flourish.Abstract;

/// <summary>
/// Reports the outcome of a Flourish settings transaction.
/// </summary>
public sealed record FlourishSettingsUpdateResult(
    string FilePath,
    bool Changed,
    bool ConfigurationReloaded
);
