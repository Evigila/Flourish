using System.Windows;
using System.Security;
using System.Windows.Input;
using WpfControl = System.Windows.Controls.Control;
using WpfPasswordBox = System.Windows.Controls.PasswordBox;

namespace ArkheideSystem.Flourish.Controls;

/// <summary>A Flourish-styled password field that wraps WPF's sealed password editor.</summary>
[TemplatePart(Name = PartPasswordBox, Type = typeof(WpfPasswordBox))]
public class FlourishPasswordBox : WpfControl
{
    private const string PartPasswordBox = "PART_PasswordBox";
    private WpfPasswordBox? editor;
    private string password = string.Empty;
    private bool isSynchronizingEditor;

    /// <summary>Identifies the <see cref="PasswordChar" /> dependency property.</summary>
    public static readonly DependencyProperty PasswordCharProperty =
        WpfPasswordBox.PasswordCharProperty.AddOwner(typeof(FlourishPasswordBox));

    /// <summary>Identifies the <see cref="MaxLength" /> dependency property.</summary>
    public static readonly DependencyProperty MaxLengthProperty =
        WpfPasswordBox.MaxLengthProperty.AddOwner(typeof(FlourishPasswordBox));

    /// <summary>Identifies the routed event raised when the password changes.</summary>
    public static readonly RoutedEvent PasswordChangedEvent = EventManager.RegisterRoutedEvent(
        nameof(PasswordChanged),
        RoutingStrategy.Bubble,
        typeof(RoutedEventHandler),
        typeof(FlourishPasswordBox)
    );

    static FlourishPasswordBox()
    {
        DefaultStyleKeyProperty.OverrideMetadata(
            typeof(FlourishPasswordBox),
            new FrameworkPropertyMetadata(typeof(FlourishPasswordBox))
        );
        FocusableProperty.OverrideMetadata(
            typeof(FlourishPasswordBox),
            new FrameworkPropertyMetadata(true)
        );
    }

    /// <summary>Gets or sets the current password.</summary>
    /// <remarks>Like WPF's PasswordBox.Password, this is intentionally not a dependency property.</remarks>
    public string Password
    {
        get => editor?.Password ?? password;
        set
        {
            var nextPassword = value ?? string.Empty;
            if (Password == nextPassword)
            {
                return;
            }

            password = nextPassword;
            if (editor is not null)
            {
                editor.Password = password;
                return;
            }

            RaiseEvent(new RoutedEventArgs(PasswordChangedEvent, this));
        }
    }

    /// <summary>Gets the password as a secure string.</summary>
    public SecureString SecurePassword => editor?.SecurePassword ?? CreateSecurePassword(password);

    /// <summary>Gets or sets the masking character.</summary>
    public char PasswordChar
    {
        get => (char)GetValue(PasswordCharProperty);
        set => SetValue(PasswordCharProperty, value);
    }

    /// <summary>Gets or sets the maximum accepted password length.</summary>
    public int MaxLength
    {
        get => (int)GetValue(MaxLengthProperty);
        set => SetValue(MaxLengthProperty, value);
    }

    /// <summary>Occurs when the password changes.</summary>
    public event RoutedEventHandler PasswordChanged
    {
        add => AddHandler(PasswordChangedEvent, value);
        remove => RemoveHandler(PasswordChangedEvent, value);
    }

    /// <summary>Clears the password.</summary>
    public void Clear()
    {
        Password = string.Empty;
    }

    /// <summary>Selects the complete password in the inner editor.</summary>
    public void SelectAll()
    {
        editor?.SelectAll();
    }

    /// <summary>Moves keyboard focus to the inner password editor.</summary>
    public bool FocusEditor()
    {
        ApplyTemplate();
        return editor?.Focus() == true;
    }

    /// <inheritdoc />
    public override void OnApplyTemplate()
    {
        if (editor is not null)
        {
            editor.PasswordChanged -= Editor_PasswordChanged;
        }

        base.OnApplyTemplate();
        editor = GetTemplateChild(PartPasswordBox) as WpfPasswordBox;
        if (editor is null)
        {
            return;
        }

        editor.PasswordChanged += Editor_PasswordChanged;
        if (editor.Password != password)
        {
            isSynchronizingEditor = true;
            try
            {
                editor.Password = password;
            }
            finally
            {
                isSynchronizingEditor = false;
            }
        }
    }

    /// <inheritdoc />
    protected override void OnGotKeyboardFocus(KeyboardFocusChangedEventArgs e)
    {
        base.OnGotKeyboardFocus(e);
        if (ReferenceEquals(e.NewFocus, this))
        {
            FocusEditor();
        }
    }

    private void Editor_PasswordChanged(object sender, RoutedEventArgs e)
    {
        password = ((WpfPasswordBox)sender).Password;
        if (!isSynchronizingEditor)
        {
            RaiseEvent(new RoutedEventArgs(PasswordChangedEvent, this));
        }
    }

    private static SecureString CreateSecurePassword(string value)
    {
        var result = new SecureString();
        foreach (var character in value)
        {
            result.AppendChar(character);
        }

        result.MakeReadOnly();
        return result;
    }
}
