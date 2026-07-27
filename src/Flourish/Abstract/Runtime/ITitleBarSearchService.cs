namespace ArkheideSystem.Flourish.Abstract.Runtime;

/// <summary>
/// Controls the Flourish title bar search box and registers runtime query handlers.
/// </summary>
public interface ITitleBarSearchService
{
    /// <summary>
    /// Gets an immutable snapshot of the current search-box state.
    /// </summary>
    FlourishTitleBarSearchState Current { get; }

    /// <summary>
    /// Occurs synchronously after programmatic search-box state changes.
    /// </summary>
    event EventHandler<FlourishTitleBarSearchStateChangedEventArgs>? StateChanged;

    /// <summary>
    /// Occurs synchronously when the user changes the query in the title bar.
    /// </summary>
    event EventHandler<FlourishTitleBarSearchChangedEventArgs>? QueryChanged;

    /// <summary>
    /// Shows or hides the title bar search box.
    /// </summary>
    /// <param name="visible"><see langword="true"/> to show the box; otherwise, <see langword="false"/>.</param>
    void SetVisible(bool visible);

    /// <summary>
    /// Sets the placeholder displayed when the search query is empty.
    /// </summary>
    /// <param name="placeholder">The non-empty placeholder text.</param>
    /// <exception cref="ArgumentException"><paramref name="placeholder"/> is empty or whitespace.</exception>
    void SetPlaceholder(string placeholder);

    /// <summary>
    /// Sets the query text without invoking query subscriptions.
    /// </summary>
    /// <param name="text">The query text.</param>
    /// <exception cref="ArgumentNullException"><paramref name="text"/> is <see langword="null"/>.</exception>
    void SetText(string text);

    /// <summary>
    /// Clears the query text without invoking query subscriptions.
    /// </summary>
    void Clear();

    /// <summary>
    /// Requests keyboard focus for the title bar search box.
    /// </summary>
    void Focus();

    /// <summary>
    /// Registers an asynchronous handler for user-entered search queries.
    /// </summary>
    /// <param name="handler">The handler to invoke. Its cancellation token is canceled when a newer query arrives.</param>
    /// <returns>A subscription that removes the handler when disposed.</returns>
    /// <remarks>
    /// Handlers run in registration order. An exception from one handler is logged and does not prevent later
    /// handlers from receiving the same query. Disposing a subscription does not cancel an invocation already in progress.
    /// </remarks>
    /// <exception cref="ArgumentNullException"><paramref name="handler"/> is <see langword="null"/>.</exception>
    /// <exception cref="ObjectDisposedException">The search service has been disposed.</exception>
    IDisposable Subscribe(
        Func<FlourishTitleBarSearchChangedEventArgs, CancellationToken, ValueTask> handler
    );
}
