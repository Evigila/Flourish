using System.Windows;
using MessageBoxOptions = System.Windows.MessageBoxOptions;

namespace AcksheedSys.Flourish.Abstract;

/// <summary>
/// Shows Flourish-styled modal messages.
/// </summary>
/// <remarks>
/// Standard overloads mirror WPF <see cref="System.Windows.MessageBox"/> button and result enums. Custom
/// option overloads return the selected <see cref="FlourishMessageOption"/>, or <see langword="null"/>
/// when the dialog is dismissed without a cancel option.
/// </remarks>
/// <example>
/// <code><![CDATA[
/// public sealed class DemoViewModel(IMessageService messages)
/// {
///     public void Save()
///     {
///         messages.Show("Saved.", "Demo", MessageBoxButton.OK, MessageBoxImage.Information);
///     }
/// }
/// ]]></code>
/// </example>
public interface IMessageService
{
    /// <summary>
    /// Shows a modal message owned by the active application window.
    /// </summary>
    /// <param name="messageBoxText">The message text.</param>
    /// <param name="caption">The dialog caption.</param>
    /// <param name="button">The buttons to display.</param>
    /// <param name="icon">The icon to display.</param>
    /// <param name="defaultResult">The default result.</param>
    /// <param name="options">The message options.</param>
    /// <returns>The selected result.</returns>
    MessageBoxResult Show(
        string messageBoxText,
        string caption = "",
        MessageBoxButton button = MessageBoxButton.OK,
        MessageBoxImage icon = MessageBoxImage.None,
        MessageBoxResult defaultResult = MessageBoxResult.None,
        MessageBoxOptions options = MessageBoxOptions.None
    );

    /// <summary>
    /// Shows a modal message owned by a specific window.
    /// </summary>
    /// <param name="owner">The owner window.</param>
    /// <param name="messageBoxText">The message text.</param>
    /// <param name="caption">The dialog caption.</param>
    /// <param name="button">The buttons to display.</param>
    /// <param name="icon">The icon to display.</param>
    /// <param name="defaultResult">The default result.</param>
    /// <param name="options">The message options.</param>
    /// <returns>The selected result.</returns>
    MessageBoxResult Show(
        Window? owner,
        string messageBoxText,
        string caption = "",
        MessageBoxButton button = MessageBoxButton.OK,
        MessageBoxImage icon = MessageBoxImage.None,
        MessageBoxResult defaultResult = MessageBoxResult.None,
        MessageBoxOptions options = MessageBoxOptions.None
    );

    /// <summary>
    /// Shows a modal message with custom options owned by the active application window.
    /// </summary>
    /// <param name="messageBoxText">The message text.</param>
    /// <param name="caption">The dialog caption.</param>
    /// <param name="choices">The custom options to display, in visual order from left to right.</param>
    /// <param name="icon">The icon to display.</param>
    /// <param name="options">The message options.</param>
    /// <returns>
    /// The selected option, or <see langword="null"/> when the dialog is dismissed without a cancel option.
    /// </returns>
    FlourishMessageOption? Show(
        string messageBoxText,
        string caption,
        IReadOnlyList<FlourishMessageOption> choices,
        MessageBoxImage icon = MessageBoxImage.None,
        MessageBoxOptions options = MessageBoxOptions.None
    );

    /// <summary>
    /// Shows a modal message with custom options owned by a specific window.
    /// </summary>
    /// <param name="owner">The owner window.</param>
    /// <param name="messageBoxText">The message text.</param>
    /// <param name="caption">The dialog caption.</param>
    /// <param name="choices">The custom options to display, in visual order from left to right.</param>
    /// <param name="icon">The icon to display.</param>
    /// <param name="options">The message options.</param>
    /// <returns>
    /// The selected option, or <see langword="null"/> when the dialog is dismissed without a cancel option.
    /// </returns>
    FlourishMessageOption? Show(
        Window? owner,
        string messageBoxText,
        string caption,
        IReadOnlyList<FlourishMessageOption> choices,
        MessageBoxImage icon = MessageBoxImage.None,
        MessageBoxOptions options = MessageBoxOptions.None
    );
}
