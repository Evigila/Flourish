using System.Windows.Controls;

namespace AcksheedSys.Flourish.Abstract;

/// <summary>
/// Provides data for Flourish navigation events.
/// </summary>
/// <param name="sourcePageType">The registered page type that was navigated to.</param>
/// <param name="page">The page instance displayed in the content frame.</param>
/// <param name="parameter">The optional navigation parameter supplied by the caller.</param>
/// <example>
/// <code><![CDATA[
/// navigation.Navigated += (_, args) =>
/// {
///     Console.WriteLine(args.SourcePageType.FullName);
/// };
/// ]]></code>
/// </example>
public sealed class FlourishNavigatedEventArgs(Type sourcePageType, Page page, object? parameter)
    : EventArgs
{
    /// <summary>
    /// Gets the registered page type that was navigated to.
    /// </summary>
    /// <example>
    /// <code><![CDATA[
    /// var currentType = args.SourcePageType;
    /// ]]></code>
    /// </example>
    public Type SourcePageType { get; } = sourcePageType;

    /// <summary>
    /// Gets the page instance displayed in the content frame.
    /// </summary>
    /// <example>
    /// <code><![CDATA[
    /// Page currentPage = args.Page;
    /// ]]></code>
    /// </example>
    public Page Page { get; } = page;

    /// <summary>
    /// Gets the optional navigation parameter supplied by the caller.
    /// </summary>
    /// <example>
    /// <code><![CDATA[
    /// if (args.Parameter is string route)
    /// {
    ///     Console.WriteLine(route);
    /// }
    /// ]]></code>
    /// </example>
    public object? Parameter { get; } = parameter;
}
