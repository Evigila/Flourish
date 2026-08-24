namespace ArkheideSystem.Flourish.Abstract;

/// <summary>
/// Describes a runtime localization change.
/// </summary>
public sealed class FlourishLocalizationChangedEventArgs(
    FlourishLocalizationChangeKind kind,
    string previousLocale,
    string currentLocale,
    IReadOnlyList<string> affectedLocales,
    FlourishCultureRegistration? registration
) : EventArgs
{
    /// <summary>
    /// Gets the reason for the change.
    /// </summary>
    public FlourishLocalizationChangeKind Kind { get; } = kind;

    /// <summary>
    /// Gets the selected locale before the change.
    /// </summary>
    public string PreviousLocale { get; } = previousLocale;

    /// <summary>
    /// Gets the selected locale after the change.
    /// </summary>
    public string CurrentLocale { get; } = currentLocale;

    /// <summary>
    /// Gets the locales whose values were affected.
    /// </summary>
    public IReadOnlyList<string> AffectedLocales { get; } =
        Array.AsReadOnly(affectedLocales.ToArray());

    /// <summary>
    /// Gets the affected file registration, when the change originated from a file.
    /// </summary>
    public FlourishCultureRegistration? Registration { get; } = registration;
}
