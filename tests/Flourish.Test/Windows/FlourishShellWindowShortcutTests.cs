using System.Windows.Controls;
using System.Windows.Input;
using ArkheideSystem.Flourish.Views.Windows;

namespace ArkheideSystem.Flourish.Test.Windows;

public sealed class FlourishShellWindowShortcutTests
{
    [Theory]
    [InlineData(Key.None)]
    [InlineData(Key.System)]
    [InlineData(Key.LeftShift)]
    [InlineData(Key.ImeProcessed)]
    [InlineData(Key.ImeConvert)]
    [InlineData(Key.DbeHiragana)]
    [InlineData(Key.DeadCharProcessed)]
    public void ShouldIgnoreShortcutInput_NonShortcutAndCompositionKeys_ReturnTrue(Key key)
    {
        Assert.True(
            FlourishShellWindow.ShouldIgnoreShortcutInput(
                key,
                ModifierKeys.Control,
                isRightAltPressed: false
            )
        );
    }

    [Fact]
    public void ShouldIgnoreShortcutInput_AltGraph_ReturnsTrueButPhysicalControlAltDoesNot()
    {
        const ModifierKeys modifiers = ModifierKeys.Control | ModifierKeys.Alt;

        Assert.True(
            FlourishShellWindow.ShouldIgnoreShortcutInput(
                Key.E,
                modifiers,
                isRightAltPressed: true
            )
        );
        Assert.False(
            FlourishShellWindow.ShouldIgnoreShortcutInput(
                Key.E,
                modifiers,
                isRightAltPressed: false
            )
        );
    }

    [Theory]
    [InlineData(Key.S, ModifierKeys.None)]
    [InlineData(Key.S, ModifierKeys.Shift)]
    [InlineData(Key.G, ModifierKeys.Control | ModifierKeys.Shift)]
    [InlineData(Key.F5, ModifierKeys.None)]
    public void ShouldIgnoreShortcutInput_OrdinaryAndShortcutCandidateKeys_ReturnFalse(
        Key key,
        ModifierKeys modifiers
    )
    {
        Assert.False(
            FlourishShellWindow.ShouldIgnoreShortcutInput(
                key,
                modifiers,
                isRightAltPressed: false
            )
        );
    }

    [Fact]
    public void IsTextInputTarget_RecognizesCommonEditableControls()
    {
        RunInSta(() =>
        {
            Assert.True(FlourishShellWindow.IsTextInputTarget(new TextBox()));
            Assert.True(FlourishShellWindow.IsTextInputTarget(new RichTextBox()));
            Assert.True(FlourishShellWindow.IsTextInputTarget(new PasswordBox()));
            Assert.True(
                FlourishShellWindow.IsTextInputTarget(new ComboBox { IsEditable = true })
            );
        });
    }

    [Fact]
    public void IsTextInputTarget_RejectsNonEditableControls()
    {
        RunInSta(() =>
        {
            Assert.False(FlourishShellWindow.IsTextInputTarget(new Button()));
            Assert.False(
                FlourishShellWindow.IsTextInputTarget(new ComboBox { IsEditable = false })
            );
            Assert.False(FlourishShellWindow.IsTextInputTarget(null));
        });
    }

    private static void RunInSta(Action action)
    {
        Exception? error = null;
        var thread = new Thread(() =>
        {
            try
            {
                action();
            }
            catch (Exception exception)
            {
                error = exception;
            }
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();

        if (error is not null)
        {
            System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(error).Throw();
        }
    }
}
