namespace ArkheideSystem.Flourish.Abstract;

/// <summary>
/// Specifies the system material effect applied to the Flourish shell window.
/// </summary>
/// <example>
/// <code><![CDATA[
/// builder.ConfigureShell(shell =>
///     shell.UseMaterialEffect(enabled: true, effect: MaterialEffect.Mica));
/// ]]></code>
/// </example>
public enum MaterialEffect
{
    /// <summary>
    /// Does not apply a system material effect.
    /// </summary>
    /// <example>
    /// <code><![CDATA[
    /// builder.ConfigureShell(shell => shell.UseMaterialEffect(enabled: false));
    /// ]]></code>
    /// </example>
    None,

    /// <summary>
    /// Applies the Windows Mica material effect when supported.
    /// </summary>
    /// <example>
    /// <code><![CDATA[
    /// builder.ConfigureShell(shell =>
    ///     shell.UseMaterialEffect(enabled: true, effect: MaterialEffect.Mica));
    /// ]]></code>
    /// </example>
    Mica,
}
