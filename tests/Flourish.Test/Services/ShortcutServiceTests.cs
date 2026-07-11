using System.Windows.Input;
using ArkheideSystem.Flourish.Abstract;
using ArkheideSystem.Flourish.Services;

namespace ArkheideSystem.Flourish.Test.Services;

public sealed class ShortcutServiceTests
{
    [Fact]
    public void Register_AndDispose_UpdatesImmutableSnapshotsAndChangedEvents()
    {
        var dispatcher = new RecordingCommandDispatcher();
        var sut = new ShortcutService(dispatcher);
        var changes = new List<ShortcutRegistryChangedEventArgs>();
        sut.Changed += (_, args) => changes.Add(args);

        var registration = sut.Register(
            Gesture(Key.S, ModifierKeys.Control),
            "editor.save",
            42
        );

        var snapshot = Assert.Single(sut.Registrations);
        Assert.Equal(registration.Id, snapshot.Id);
        Assert.Equal("editor.save", snapshot.CommandKey);
        Assert.Equal(42, snapshot.Parameter);
        Assert.False(snapshot.AllowWhenTextInputFocused);
        Assert.Equal(ShortcutRegistryChangeKind.Registered, changes[0].ChangeKind);
        registration.Dispose();
        registration.Dispose();
        Assert.False(registration.IsRegistered);
        Assert.Empty(sut.Registrations);
        Assert.Equal(2, changes.Count);
        Assert.Equal(ShortcutRegistryChangeKind.Unregistered, changes[1].ChangeKind);
        Assert.Empty(changes[1].Registrations);
    }

    [Fact]
    public void Register_IdenticalGestureAndScope_RejectsByDefault()
    {
        var sut = new ShortcutService(new RecordingCommandDispatcher());
        sut.Register(Gesture(Key.S, ModifierKeys.Control), "editor.save");

        var exception = Assert.Throws<InvalidOperationException>(() =>
            sut.Register(Gesture(Key.S, ModifierKeys.Control), "editor.saveAs")
        );

        Assert.Contains("Ctrl", exception.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Single(sut.Registrations);
    }

    [Fact]
    public void Register_WithReplacePolicy_InvalidatesConflictingLease()
    {
        var sut = new ShortcutService(new RecordingCommandDispatcher());
        var original = sut.Register(
            Gesture(Key.S, ModifierKeys.Control),
            "editor.save"
        );

        var replacement = sut.Register(
            Gesture(Key.S, ModifierKeys.Control),
            "editor.saveAs",
            options: new ShortcutRegistrationOptions
            {
                ConflictPolicy = ShortcutConflictPolicy.Replace,
            }
        );

        Assert.False(original.IsRegistered);
        Assert.True(replacement.IsRegistered);
        Assert.Equal("editor.saveAs", Assert.Single(sut.Registrations).CommandKey);
    }

    [Fact]
    public void TryResolve_AppendedConflicts_UsesHighestPriority()
    {
        var sut = new ShortcutService(new RecordingCommandDispatcher());
        sut.Register(
            Gesture(Key.S, ModifierKeys.Control),
            "editor.save",
            options: new ShortcutRegistrationOptions { Priority = 1 }
        );
        sut.Register(
            Gesture(Key.S, ModifierKeys.Control),
            "editor.saveAll",
            options: new ShortcutRegistrationOptions
            {
                ConflictPolicy = ShortcutConflictPolicy.Append,
                Priority = 10,
            }
        );

        var resolved = sut.TryResolve(
            Gesture(Key.S, ModifierKeys.Control),
            null,
            out var shortcut
        );

        Assert.True(resolved);
        Assert.NotNull(shortcut);
        Assert.Equal("editor.saveAll", shortcut.CommandKey);
    }

    [Fact]
    public void TryResolve_PageThenWindowScopesOverrideApplicationAndRequireMatchingContext()
    {
        var sut = new ShortcutService(new RecordingCommandDispatcher());
        var gesture = Gesture(Key.S, ModifierKeys.Control);
        sut.Register(gesture, "app.save");
        sut.Register(
            gesture,
            "window.save",
            options: new ShortcutRegistrationOptions
            {
                Scope = ShortcutScope.Window,
                ScopeKey = "Main",
            }
        );
        sut.Register(
            gesture,
            "page.save",
            options: new ShortcutRegistrationOptions
            {
                Scope = ShortcutScope.Page,
                ScopeKey = "Editor",
            }
        );

        Assert.True(
            sut.TryResolve(
                gesture,
                new ShortcutResolutionContext("Main", "Editor"),
                out var pageShortcut
            )
        );
        Assert.Equal("page.save", pageShortcut!.CommandKey);

        Assert.True(
            sut.TryResolve(
                gesture,
                new ShortcutResolutionContext("Main", "Home"),
                out var windowShortcut
            )
        );
        Assert.Equal("window.save", windowShortcut!.CommandKey);

        Assert.True(
            sut.TryResolve(
                gesture,
                new ShortcutResolutionContext("Secondary", "Home"),
                out var appShortcut
            )
        );
        Assert.Equal("app.save", appShortcut!.CommandKey);
    }

    [Fact]
    public void TryResolve_ExactScopeKeyOverridesWildcardWithinSameScope()
    {
        var sut = new ShortcutService(new RecordingCommandDispatcher());
        var gesture = Gesture(Key.S, ModifierKeys.Control);
        sut.Register(
            gesture,
            "page.wildcard",
            options: new ShortcutRegistrationOptions
            {
                Scope = ShortcutScope.Page,
                Priority = 100,
            }
        );
        sut.Register(
            gesture,
            "page.editor",
            options: new ShortcutRegistrationOptions
            {
                Scope = ShortcutScope.Page,
                ScopeKey = "Editor",
            }
        );

        Assert.True(
            sut.TryResolve(
                gesture,
                new ShortcutResolutionContext(pageKey: "Editor"),
                out var shortcut
            )
        );
        Assert.Equal("page.editor", shortcut!.CommandKey);
    }

    [Fact]
    public async Task ExecuteAsync_DispatchesResolvedCommandWithParameterAndShortcutSource()
    {
        var dispatcher = new RecordingCommandDispatcher();
        var sut = new ShortcutService(dispatcher);
        var parameter = new object();
        sut.Register(
            Gesture(Key.F5),
            "document.refresh",
            parameter
        );

        var result = await sut.ExecuteAsync(Gesture(Key.F5));

        Assert.True(result.IsHandled);
        var invocation = Assert.Single(dispatcher.Invocations);
        Assert.Equal("document.refresh", invocation.CommandKey);
        Assert.Same(parameter, invocation.Parameter);
        Assert.Equal(CommandSource.Shortcut, invocation.Source);
    }

    [Fact]
    public async Task ExecuteAsync_WhenGestureDoesNotResolve_ReturnsNotHandled()
    {
        var dispatcher = new RecordingCommandDispatcher();
        var sut = new ShortcutService(dispatcher);

        var result = await sut.ExecuteAsync(Gesture(Key.F5));

        Assert.Equal(CommandExecutionStatus.NotHandled, result.Status);
        Assert.Empty(dispatcher.Invocations);
    }

    [Fact]
    public void TryResolve_RawTextKey_DoesNotConstructOrResolveInvalidGesture()
    {
        var sut = new ShortcutService(new RecordingCommandDispatcher());

        var resolved = sut.TryResolve(
            Key.S,
            ModifierKeys.Shift,
            context: null,
            isTextInputFocused: true,
            out var registration
        );

        Assert.False(resolved);
        Assert.Null(registration);
    }

    [Fact]
    public void TryResolve_TextInputFocus_RequiresExplicitRegistrationOptIn()
    {
        var sut = new ShortcutService(new RecordingCommandDispatcher());
        sut.Register(Gesture(Key.C, ModifierKeys.Control), "editor.copy");
        sut.Register(
            Gesture(Key.S, ModifierKeys.Control),
            "document.save",
            options: new ShortcutRegistrationOptions
            {
                AllowWhenTextInputFocused = true,
            }
        );

        Assert.False(
            sut.TryResolve(
                Key.C,
                ModifierKeys.Control,
                context: null,
                isTextInputFocused: true,
                out _
            )
        );
        Assert.True(
            sut.TryResolve(
                Key.S,
                ModifierKeys.Control,
                context: null,
                isTextInputFocused: true,
                out var registration
            )
        );
        Assert.NotNull(registration);
        Assert.Equal("document.save", registration.CommandKey);
        Assert.True(registration.AllowWhenTextInputFocused);
    }

    private static KeyGesture Gesture(
        Key key,
        ModifierKeys modifiers = ModifierKeys.None
    )
    {
        return new KeyGesture(key, modifiers);
    }

    private sealed class RecordingCommandDispatcher : ICommandDispatcher
    {
        public List<CommandContext> Invocations { get; } = [];

        public bool CanExecute(
            string commandKey,
            object? parameter = null,
            CommandSource source = CommandSource.Application
        )
        {
            return true;
        }

        public ValueTask<CommandResult> ExecuteAsync(
            string commandKey,
            object? parameter = null,
            CommandSource source = CommandSource.Application,
            CancellationToken cancellationToken = default
        )
        {
            Invocations.Add(new CommandContext(commandKey, parameter, source));
            return ValueTask.FromResult(CommandResult.Handled);
        }
    }
}
