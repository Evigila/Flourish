namespace ArkheideSystem.Flourish.Abstract;

/// <summary>
/// Identifies one runtime culture-file registration.
/// </summary>
public sealed class FlourishCultureRegistration
{
    private IReadOnlyList<string> locales;

    internal FlourishCultureRegistration(Guid id, IReadOnlyList<string> locales, string filePath)
    {
        Id = id;
        this.locales = Array.AsReadOnly(locales.ToArray());
        FilePath = filePath;
    }

    /// <summary>
    /// Gets the stable registration identifier.
    /// </summary>
    public Guid Id { get; }

    /// <summary>
    /// Gets the registered locales in canonical form.
    /// </summary>
    public IReadOnlyList<string> Locales => locales;

    /// <summary>
    /// Gets the absolute culture-file path.
    /// </summary>
    public string FilePath { get; }

    internal void UpdateLocales(IReadOnlyList<string> values) =>
        locales = Array.AsReadOnly(values.ToArray());
}
