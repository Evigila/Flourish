using System.IO;
using System.Windows.Controls;
using System.Windows.Input;
using ArkheideSystem.Flourish.Views.Windows;

namespace ArkheideSystem.Flourish.Test.Windows;

public sealed class FlourishShellWindowShortcutTests
{
    private static readonly string RepositoryRoot = TestPaths.RepositoryRoot;
    private static readonly string ShellCodePath = Path.Combine(
        RepositoryRoot,
        "src",
        "Flourish",
        "Views",
        "Windows",
        "FlourishShellWindow.xaml.cs"
    );

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
        StaTest.Run(() =>
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
        StaTest.Run(() =>
        {
            Assert.False(FlourishShellWindow.IsTextInputTarget(new Button()));
            Assert.False(
                FlourishShellWindow.IsTextInputTarget(new ComboBox { IsEditable = false })
            );
            Assert.False(FlourishShellWindow.IsTextInputTarget(null));
        });
    }

    [Fact]
    public void PreviewKeyDown_ResolvesOnceAndHandlesBeforeExecutingAcceptedSnapshot()
    {
        var source = File.ReadAllText(ShellCodePath);
        var start = source.IndexOf(
            "private async void ShellWindow_PreviewKeyDown(",
            StringComparison.Ordinal
        );
        var end = source.IndexOf(
            "internal static bool ShouldIgnoreShortcutInput(",
            start,
            StringComparison.Ordinal
        );
        Assert.True(start >= 0 && end > start);
        var handler = source[start..end];

        Assert.Equal(
            1,
            handler.Split("shortcutService.TryResolve(", StringSplitOptions.None).Length - 1
        );
        Assert.Contains(
            "await shortcutService.ExecuteResolvedAsync(registration);",
            handler,
            StringComparison.Ordinal
        );
        Assert.DoesNotContain(
            "shortcutService.ExecuteAsync(registration.Gesture",
            handler,
            StringComparison.Ordinal
        );

        var executeIndex = handler.IndexOf(
            "await shortcutService.ExecuteResolvedAsync(registration);",
            StringComparison.Ordinal
        );
        var resolveIndex = handler.LastIndexOf(
            "shortcutService.TryResolve(",
            executeIndex,
            StringComparison.Ordinal
        );
        var handledIndex = handler.LastIndexOf(
            "e.Handled = true;",
            executeIndex,
            StringComparison.Ordinal
        );
        Assert.True(resolveIndex >= 0 && resolveIndex < handledIndex);
        Assert.True(handledIndex < executeIndex);
    }
}
