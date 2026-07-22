using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using WpfClipboard = System.Windows.Clipboard;
using WpfControl = System.Windows.Controls.Control;
using WpfTextDataFormat = System.Windows.TextDataFormat;

namespace ArkheideSystem.Flourish.Controls;

/// <summary>
/// Presents code text at the Large monospaced size on a rounded surface and provides a built-in copy action.
/// </summary>
public class CodeSpace : WpfControl
{
    /// <summary>Identifies the <see cref="Text" /> dependency property.</summary>
    public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
        nameof(Text),
        typeof(string),
        typeof(CodeSpace),
        new FrameworkPropertyMetadata(string.Empty, OnTextChanged)
    );

    static CodeSpace()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(CodeSpace),
            new FrameworkPropertyMetadata(typeof(CodeSpace))
        );
        CommandManager.RegisterClassCommandBinding(
            typeof(CodeSpace),
            new CommandBinding(
                ApplicationCommands.Copy,
                ExecuteCopy,
                CanExecuteCopy
            )
        );
    }

    /// <summary>Gets or sets the code text displayed and copied by the control.</summary>
    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    internal Action<string> ClipboardWriter { get; set; } = WriteClipboard;

    private static void OnTextChanged(
        DependencyObject dependencyObject,
        DependencyPropertyChangedEventArgs e
    )
    {
        CommandManager.InvalidateRequerySuggested();
    }

    private static void CanExecuteCopy(object sender, CanExecuteRoutedEventArgs e)
    {
        e.CanExecute = sender is CodeSpace { Text.Length: > 0 };
        e.Handled = true;
    }

    private static void ExecuteCopy(object sender, ExecutedRoutedEventArgs e)
    {
        if (sender is not CodeSpace { Text.Length: > 0 } codeSpace)
        {
            return;
        }

        try
        {
            codeSpace.ClipboardWriter(codeSpace.Text);
        }
        catch (ExternalException)
        {
            // The system clipboard can be temporarily locked by another desktop process.
        }

        e.Handled = true;
    }

    private static void WriteClipboard(string text)
    {
        WpfClipboard.SetText(text, WpfTextDataFormat.UnicodeText);
    }
}
