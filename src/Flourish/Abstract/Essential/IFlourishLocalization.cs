namespace ArkheideSystem.Flourish.Abstract.Essential;

/// <summary>
/// Resolves localized text and manages runtime locale sources.
/// </summary>
public interface IFlourishLocalization
{
    /// <summary>
    /// Gets the currently selected normalized locale.
    /// </summary>
    string CurrentLocale { get; }

    /// <summary>
    /// Gets the built-in and currently registered locales.
    /// </summary>
    IReadOnlyList<string> AvailableLocales { get; }

    /// <summary>
    /// Raised when the locale or one of its registered sources changes.
    /// </summary>
    /// <remarks>The event is raised synchronously on the thread performing the change.</remarks>
    event EventHandler<FlourishLocalizationChangedEventArgs>? Changed;

    /// <summary>
    /// Resolves a localized value, falling back to English and then the key itself.
    /// </summary>
    string Get(string key);

    /// <summary>
    /// Resolves and formats a localized value using the current culture.
    /// </summary>
    string Format(string key, params object?[] arguments);

    /// <summary>
    /// Changes the currently selected locale.
    /// </summary>
    void SetLocale(string locale);

    /// <summary>
    /// Loads and registers a lang_&lt;locale&gt;.json file.
    /// </summary>
    FlourishLocaleRegistration RegisterFile(string path);

    /// <summary>
    /// Reloads a previously registered locale file from disk.
    /// </summary>
    void ReloadFile(FlourishLocaleRegistration registration);

    /// <summary>
    /// Removes a previously registered locale file.
    /// </summary>
    bool Unregister(FlourishLocaleRegistration registration);
}
