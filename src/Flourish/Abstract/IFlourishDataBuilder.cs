namespace AckSS.Flourish.Abstract;

/// <summary>
/// Configures application-level data and preference storage used by Flourish.
/// </summary>
/// <example>
/// <code><![CDATA[
/// builder.ConfigureData(data =>
/// {
///     data.SetAppCompany("Acksheed System Team")
///         .SetAppName("Gallery");
/// });
/// ]]></code>
/// </example>
public interface IFlourishDataBuilder
{
    /// <summary>
    /// Sets the directory used to store Flourish application preference data.
    /// </summary>
    /// <param name="path">The preference data directory path.</param>
    /// <returns>The current builder for chained configuration.</returns>
    IFlourishDataBuilder SetAppPreferenceDataPath(string path);

    /// <summary>
    /// Sets the application name used for preference data paths.
    /// </summary>
    /// <param name="appName">The application name.</param>
    /// <returns>The current builder for chained configuration.</returns>
    /// <remarks>
    /// When this value is not set, Flourish uses the title configured by
    /// <see cref="IFlourishTitlebarBuilder.SetTitle(string)" />.
    /// </remarks>
    IFlourishDataBuilder SetAppName(string appName);

    /// <summary>
    /// Sets the company name used for preference data paths.
    /// </summary>
    /// <param name="companyName">The company name.</param>
    /// <returns>The current builder for chained configuration.</returns>
    IFlourishDataBuilder SetAppCompany(string companyName);
}
