namespace ArkheideSystem.Flourish.Abstract.Essential;

/// <summary>
/// Edits the <c>Flourish</c> section in one in-memory appsettings transaction.
/// </summary>
/// <remarks>
/// Every path must start with <c>Flourish:</c> and identify a child value.
/// </remarks>
public interface IFlourishSettingsEditor
{
    /// <summary>
    /// Replaces the value at a colon-delimited path.
    /// </summary>
    void Set<T>(string path, T value);

    /// <summary>
    /// Removes the value at a colon-delimited path.
    /// </summary>
    bool Remove(string path);

    /// <summary>
    /// Recursively merges an object into the object at a colon-delimited path.
    /// </summary>
    void Merge<T>(string path, T value);

    /// <summary>
    /// Appends one value to the JSON array at a colon-delimited path.
    /// </summary>
    void Append<T>(string path, T value);
}
