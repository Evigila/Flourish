namespace ArkheideSystem.Flourish.Abstract;

/// <summary>
/// Identifies one runtime locale-file registration.
/// </summary>
public sealed class FlourishLocaleRegistration
{
    internal FlourishLocaleRegistration(Guid id, string locale, string filePath)
    {
        Id = id;
        Locale = locale;
        FilePath = filePath;
    }

    /// <summary>
    /// Gets the stable registration identifier.
    /// </summary>
    public Guid Id { get; }

    /// <summary>
    /// Gets the registered locale in canonical form.
    /// </summary>
    public string Locale { get; }

    /// <summary>
    /// Gets the absolute locale-file path.
    /// </summary>
    public string FilePath { get; }
}
