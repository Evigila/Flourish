namespace ArkheideSystem.Flourish.Abstract;

/// <summary>
/// Provides runtime navigation services for registered Flourish pages.
/// </summary>
/// <example>
/// <code><![CDATA[
/// public sealed class HomeViewModel(INavigationService navigation)
/// {
///     public void OpenSettings() => navigation.Navigate("Settings");
/// }
/// ]]></code>
/// </example>
public interface INavigationService
{
    /// <summary>Occurs after the current route or available navigation history changes.</summary>
    event EventHandler<FlourishNavigationStateChangedEventArgs>? StateChanged;

    /// <summary>
    /// Occurs after Flourish navigates to a registered page.
    /// </summary>
    /// <example>
    /// <code><![CDATA[
    /// navigation.Navigated += (_, args) =>
    /// {
    ///     Console.WriteLine(args.SourcePageType.Name);
    /// };
    /// ]]></code>
    /// </example>
    event EventHandler<FlourishNavigatedEventArgs>? Navigated;

    /// <summary>
    /// Gets a value indicating whether backward navigation is available.
    /// </summary>
    /// <example>
    /// <code><![CDATA[
    /// if (navigation.CanGoBack)
    /// {
    ///     navigation.GoBack();
    /// }
    /// ]]></code>
    /// </example>
    bool CanGoBack { get; }

    /// <summary>
    /// Gets a value indicating whether forward navigation is available.
    /// </summary>
    /// <example>
    /// <code><![CDATA[
    /// if (navigation.CanGoForward)
    /// {
    ///     navigation.GoForward();
    /// }
    /// ]]></code>
    /// </example>
    bool CanGoForward { get; }

    /// <summary>
    /// Gets the registered source page type currently displayed in the content frame.
    /// </summary>
    /// <example>
    /// <code><![CDATA[
    /// Type? currentPageType = navigation.CurrentSourcePageType;
    /// ]]></code>
    /// </example>
    Type? CurrentSourcePageType { get; }

    /// <summary>
    /// Gets the navigation key currently displayed in the content frame.
    /// </summary>
    /// <example>
    /// <code><![CDATA[
    /// string? currentRoute = navigation.CurrentNavigationKey;
    /// ]]></code>
    /// </example>
    string? CurrentNavigationKey { get; }

    /// <summary>Gets the parameter supplied by the most recent successful navigation.</summary>
    object? CurrentParameter { get; }

    /// <summary>Gets the case-sensitive keys of all currently registered runtime routes.</summary>
    IReadOnlyCollection<string> Routes { get; }

    /// <summary>Gets whether a route is currently registered.</summary>
    /// <param name="navigationKey">The case-sensitive route key.</param>
    /// <returns><see langword="true" /> when the route is registered.</returns>
    bool CanNavigate(string navigationKey);

    /// <summary>
    /// Navigates to a registered page by navigation key.
    /// </summary>
    /// <param name="navigationKey">The case-sensitive registered navigation key.</param>
    /// <param name="parameter">An optional parameter passed to the destination page.</param>
    /// <param name="addToBackStack">A value indicating whether the current page should be added to the back stack.</param>
    /// <returns><see langword="true" /> if navigation succeeded; otherwise, <see langword="false" />.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the service is not initialized or <paramref name="navigationKey" /> is not registered.</exception>
    /// <example>
    /// <code><![CDATA[
    /// navigation.Navigate("Settings");
    /// ]]></code>
    /// </example>
    bool Navigate(string navigationKey, object? parameter = null, bool addToBackStack = true);

    /// <summary>Navigates to the route registered for a page type.</summary>
    /// <typeparam name="TPage">The registered WPF page type.</typeparam>
    /// <param name="parameter">An optional parameter passed to the destination page.</param>
    /// <param name="addToBackStack">Whether to add the current route to history.</param>
    /// <returns><see langword="true" /> when navigation succeeds.</returns>
    bool Navigate<TPage>(object? parameter = null, bool addToBackStack = true)
        where TPage : System.Windows.Controls.Page;

    /// <summary>
    /// Navigates to a route and marshals the operation to the initialized Frame dispatcher.
    /// </summary>
    /// <param name="navigationKey">The case-sensitive route key.</param>
    /// <param name="parameter">An optional parameter passed to the destination page.</param>
    /// <param name="addToBackStack">Whether to add the current route to history.</param>
    /// <param name="cancellationToken">A token that cancels waiting for dispatcher execution.</param>
    /// <returns>A task containing whether navigation succeeded.</returns>
    Task<bool> NavigateAsync(
        string navigationKey,
        object? parameter = null,
        bool addToBackStack = true,
        CancellationToken cancellationToken = default
    );

    /// <summary>
    /// Navigates to the previous page in the back stack.
    /// </summary>
    /// <returns><see langword="true" /> if backward navigation succeeded; otherwise, <see langword="false" />.</returns>
    /// <example>
    /// <code><![CDATA[
    /// var moved = navigation.GoBack();
    /// ]]></code>
    /// </example>
    bool GoBack();

    /// <summary>
    /// Navigates to the next page in the forward stack.
    /// </summary>
    /// <returns><see langword="true" /> if forward navigation succeeded; otherwise, <see langword="false" />.</returns>
    /// <example>
    /// <code><![CDATA[
    /// var moved = navigation.GoForward();
    /// ]]></code>
    /// </example>
    bool GoForward();

    /// <summary>
    /// Clears the navigation back stack.
    /// </summary>
    /// <example>
    /// <code><![CDATA[
    /// navigation.ClearBackStack();
    /// ]]></code>
    /// </example>
    void ClearBackStack();

    /// <summary>Clears the navigation forward stack.</summary>
    void ClearForwardStack();

    /// <summary>Clears both navigation history stacks.</summary>
    void ClearHistory();
}
