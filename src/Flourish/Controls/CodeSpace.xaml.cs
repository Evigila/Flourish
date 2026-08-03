using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Input;
using System.Windows.Threading;
using WpfClipboard = System.Windows.Clipboard;
using WpfControl = System.Windows.Controls.Control;
using WpfTextDataFormat = System.Windows.TextDataFormat;

namespace ArkheideSystem.Flourish.Controls;

/// <summary>
/// Presents code text at the Large monospaced size on a rounded surface and provides a built-in copy action.
/// </summary>
public class CodeSpace : WpfControl
{
    /// <summary>Expands a collapsed <see cref="CodeSpace" />.</summary>
    public static RoutedUICommand ExpandCommand { get; } =
        new("View code", nameof(ExpandCommand), typeof(CodeSpace));

    /// <summary>Collapses an expanded <see cref="CodeSpace" />.</summary>
    public static RoutedUICommand CollapseCommand { get; } =
        new("Collapse code", nameof(CollapseCommand), typeof(CodeSpace));

    internal static readonly TimeSpan CopyConfirmationDuration = TimeSpan.FromMilliseconds(1500);

    private static readonly DependencyPropertyKey IsCopyConfirmedPropertyKey =
        DependencyProperty.RegisterReadOnly(
            nameof(IsCopyConfirmed),
            typeof(bool),
            typeof(CodeSpace),
            new FrameworkPropertyMetadata(false)
        );

    /// <summary>Identifies the read-only <see cref="IsCopyConfirmed" /> dependency property.</summary>
    public static readonly DependencyProperty IsCopyConfirmedProperty =
        IsCopyConfirmedPropertyKey.DependencyProperty;

    /// <summary>Identifies the <see cref="CanCollapse" /> dependency property.</summary>
    public static readonly DependencyProperty CanCollapseProperty = DependencyProperty.Register(
        nameof(CanCollapse),
        typeof(bool),
        typeof(CodeSpace),
        new FrameworkPropertyMetadata(true, OnCanCollapseChanged)
    );

    /// <summary>Identifies the <see cref="IsExpanded" /> dependency property.</summary>
    public static readonly DependencyProperty IsExpandedProperty = DependencyProperty.Register(
        nameof(IsExpanded),
        typeof(bool),
        typeof(CodeSpace),
        new FrameworkPropertyMetadata(
            false,
            FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
            OnIsExpandedChanged
        )
    );

    /// <summary>Identifies the <see cref="Text" /> dependency property.</summary>
    public static readonly DependencyProperty TextProperty = DependencyProperty.Register(
        nameof(Text),
        typeof(string),
        typeof(CodeSpace),
        new FrameworkPropertyMetadata(string.Empty, OnTextChanged)
    );

    private readonly DispatcherTimer copyConfirmationTimer;

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
        CommandManager.RegisterClassCommandBinding(
            typeof(CodeSpace),
            new CommandBinding(ExpandCommand, ExecuteExpand, CanExecuteExpand)
        );
        CommandManager.RegisterClassCommandBinding(
            typeof(CodeSpace),
            new CommandBinding(CollapseCommand, ExecuteCollapse, CanExecuteCollapse)
        );
    }

    /// <summary>Initializes a new instance of the <see cref="CodeSpace" /> class.</summary>
    public CodeSpace()
    {
        copyConfirmationTimer = new DispatcherTimer(
            CopyConfirmationDuration,
            DispatcherPriority.Normal,
            OnCopyConfirmationElapsed,
            Dispatcher
        );
        Unloaded += OnUnloaded;
    }

    /// <summary>Gets or sets the code text displayed and copied by the control.</summary>
    public string Text
    {
        get => (string)GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    /// <summary>Gets whether the most recent copy operation is being acknowledged.</summary>
    public bool IsCopyConfirmed => (bool)GetValue(IsCopyConfirmedProperty);

    /// <summary>Gets or sets whether user interaction may collapse the expanded code.</summary>
    public bool CanCollapse
    {
        get => (bool)GetValue(CanCollapseProperty);
        set => SetValue(CanCollapseProperty, value);
    }

    /// <summary>Gets or sets whether the complete code presentation is expanded.</summary>
    public bool IsExpanded
    {
        get => (bool)GetValue(IsExpandedProperty);
        set => SetValue(IsExpandedProperty, value);
    }

    internal Action<string> ClipboardWriter { get; set; } = WriteClipboard;

    /// <inheritdoc />
    protected override AutomationPeer OnCreateAutomationPeer() => new CodeSpaceAutomationPeer(this);

    /// <inheritdoc />
    protected override void OnKeyDown(System.Windows.Input.KeyEventArgs e)
    {
        base.OnKeyDown(e);
        if (e.Handled || !ReferenceEquals(e.OriginalSource, this))
        {
            return;
        }

        if ((e.Key is Key.Enter or Key.Space) && (!IsExpanded || CanCollapse))
        {
            SetCurrentValue(IsExpandedProperty, !IsExpanded);
            e.Handled = true;
        }
    }

    /// <inheritdoc />
    protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
    {
        base.OnMouseLeftButtonUp(e);
        if (e.Handled || IsExpanded)
        {
            return;
        }

        Focus();
        SetCurrentValue(IsExpandedProperty, true);
        e.Handled = true;
    }

    private static void CanExecuteExpand(object sender, CanExecuteRoutedEventArgs e)
    {
        e.CanExecute = sender is CodeSpace { IsEnabled: true, IsExpanded: false };
        e.Handled = true;
    }

    private static void ExecuteExpand(object sender, ExecutedRoutedEventArgs e)
    {
        if (sender is CodeSpace codeSpace)
        {
            codeSpace.SetCurrentValue(IsExpandedProperty, true);
            codeSpace.Focus();
        }

        e.Handled = true;
    }

    private static void CanExecuteCollapse(object sender, CanExecuteRoutedEventArgs e)
    {
        e.CanExecute = sender is CodeSpace
        {
            IsEnabled: true,
            IsExpanded: true,
            CanCollapse: true,
        };
        e.Handled = true;
    }

    private static void ExecuteCollapse(object sender, ExecutedRoutedEventArgs e)
    {
        if (sender is CodeSpace { CanCollapse: true } codeSpace)
        {
            codeSpace.SetCurrentValue(IsExpandedProperty, false);
            codeSpace.Focus();
        }

        e.Handled = true;
    }

    private static void OnIsExpandedChanged(
        DependencyObject dependencyObject,
        DependencyPropertyChangedEventArgs e
    )
    {
        var codeSpace = (CodeSpace)dependencyObject;
        if (!(bool)e.NewValue)
        {
            codeSpace.ResetCopyConfirmation();
        }

        CommandManager.InvalidateRequerySuggested();
        if (UIElementAutomationPeer.FromElement(codeSpace) is CodeSpaceAutomationPeer peer)
        {
            peer.RaiseExpandCollapseStateChanged((bool)e.OldValue, (bool)e.NewValue);
        }
    }

    private static void OnCanCollapseChanged(
        DependencyObject dependencyObject,
        DependencyPropertyChangedEventArgs e
    )
    {
        CommandManager.InvalidateRequerySuggested();
    }

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

        codeSpace.ResetCopyConfirmation();
        try
        {
            codeSpace.ClipboardWriter(codeSpace.Text);
            codeSpace.ShowCopyConfirmation();
        }
        catch (ExternalException)
        {
            // The system clipboard can be temporarily locked by another desktop process.
        }

        e.Handled = true;
    }

    private void ShowCopyConfirmation()
    {
        copyConfirmationTimer.Stop();
        SetValue(IsCopyConfirmedPropertyKey, true);
        copyConfirmationTimer.Start();
    }

    private void OnCopyConfirmationElapsed(object? sender, EventArgs e)
    {
        copyConfirmationTimer.Stop();
        ResetCopyConfirmation();
    }

    private void OnUnloaded(object sender, RoutedEventArgs e)
    {
        ResetCopyConfirmation();
    }

    internal void ResetCopyConfirmation()
    {
        copyConfirmationTimer.Stop();
        SetValue(IsCopyConfirmedPropertyKey, false);
    }

    private static void WriteClipboard(string text)
    {
        WpfClipboard.SetText(text, WpfTextDataFormat.UnicodeText);
    }

    private sealed class CodeSpaceAutomationPeer(CodeSpace owner)
        : FrameworkElementAutomationPeer(owner), IExpandCollapseProvider
    {
        private CodeSpace CodeSpaceOwner => (CodeSpace)Owner;

        public ExpandCollapseState ExpandCollapseState =>
            CodeSpaceOwner.IsExpanded
                ? ExpandCollapseState.Expanded
                : ExpandCollapseState.Collapsed;

        public void Collapse()
        {
            EnsureEnabled();
            if (!CodeSpaceOwner.CanCollapse)
            {
                throw new InvalidOperationException(
                    "This CodeSpace does not allow user-initiated collapse."
                );
            }

            CodeSpaceOwner.SetCurrentValue(IsExpandedProperty, false);
        }

        public void Expand()
        {
            EnsureEnabled();
            CodeSpaceOwner.SetCurrentValue(IsExpandedProperty, true);
        }

        public override object? GetPattern(PatternInterface patternInterface) =>
            patternInterface == PatternInterface.ExpandCollapse
                ? this
                : base.GetPattern(patternInterface);

        protected override string GetClassNameCore() => nameof(CodeSpace);

        protected override string GetNameCore()
        {
            var name = base.GetNameCore();
            return string.IsNullOrWhiteSpace(name) ? "Code" : name;
        }

        protected override AutomationControlType GetAutomationControlTypeCore() =>
            AutomationControlType.Group;

        internal void RaiseExpandCollapseStateChanged(bool oldValue, bool newValue)
        {
            RaisePropertyChangedEvent(
                ExpandCollapsePatternIdentifiers.ExpandCollapseStateProperty,
                oldValue ? ExpandCollapseState.Expanded : ExpandCollapseState.Collapsed,
                newValue ? ExpandCollapseState.Expanded : ExpandCollapseState.Collapsed
            );
        }

        private void EnsureEnabled()
        {
            if (!CodeSpaceOwner.IsEnabled)
            {
                throw new ElementNotEnabledException();
            }
        }
    }
}
