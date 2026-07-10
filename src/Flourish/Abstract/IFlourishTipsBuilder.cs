namespace ArkheideSystem.Flourish.Abstract;

/// <summary>
/// Configures Flourish tooltips.
/// </summary>
/// <example>
/// <code><![CDATA[
/// builder
///     .ConfigureShell(shell => shell.UseTips())
///     .ConfigureTips(tips => tips.SetDelay(200).SetSpawnableMargin(5));
/// ]]></code>
/// </example>
public interface IFlourishTipsBuilder
{
    /// <summary>
    /// Sets how long the pointer must hover before a tooltip is shown.
    /// </summary>
    /// <param name="milliseconds">The tooltip delay in milliseconds.</param>
    /// <returns>The current builder for chained configuration.</returns>
    /// <example>
    /// <code><![CDATA[
    /// tips.SetDelay(200);
    /// ]]></code>
    /// </example>
    IFlourishTipsBuilder SetDelay(int milliseconds = 200);

    /// <summary>
    /// Sets the minimum distance between a tooltip and the shell window bounds.
    /// </summary>
    /// <param name="margin">The margin, in device-independent pixels.</param>
    /// <returns>The current builder for chained configuration.</returns>
    /// <example>
    /// <code><![CDATA[
    /// tips.SetSpawnableMargin(5);
    /// ]]></code>
    /// </example>
    IFlourishTipsBuilder SetSpawnableMargin(double margin = 5);
}
