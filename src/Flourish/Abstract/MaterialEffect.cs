namespace ArkheideSystem.Flourish.Abstract;

/// <summary>
/// Specifies the system material effect applied to the Flourish shell window.
/// </summary>
/// <example>
/// <code><![CDATA[
/// builder.ConfigShell(shell =>
///     shell.UseMaterialEffect(enabled: true, effect: MaterialEffect.Auto));
/// ]]></code>
/// </example>
public enum MaterialEffect
{
    /// <summary>
    /// Does not apply a system material effect.
    /// </summary>
    /// <example>
    /// <code><![CDATA[
    /// builder.ConfigShell(shell => shell.UseMaterialEffect(enabled: false));
    /// ]]></code>
    /// </example>
    None = 0,

    /// <summary>
    /// Applies the Windows Mica material effect when supported.
    /// </summary>
    /// <example>
    /// <code><![CDATA[
    /// builder.ConfigShell(shell =>
    ///     shell.UseMaterialEffect(enabled: true, effect: MaterialEffect.Mica));
    /// ]]></code>
    /// </example>
    Mica = 1,

    /// <summary>
    /// Applies Desktop Acrylic when supported. Windows 11 uses the system backdrop API;
    /// Windows 10 uses the compatible AccentPolicy backend.
    /// </summary>
    Acrylic = 2,

    /// <summary>
    /// Applies the alternate Mica material intended for tabbed or strongly layered windows.
    /// </summary>
    MicaAlt = 3,

    /// <summary>
    /// Selects the platform default: Mica on Windows 11, Acrylic on Windows 10, and no
    /// material on unsupported operating systems.
    /// </summary>
    Auto = 4,
}
