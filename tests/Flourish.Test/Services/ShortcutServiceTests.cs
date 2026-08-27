using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using ArkheideSystem.Flourish.Abstract;
using ArkheideSystem.Flourish.Commands;

using System.Windows.Input;

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
            "cmd_editor_save",
            42
        );

        var snapshot = Assert.Single(sut.Current);
        Assert.NotEqual(Guid.Empty, snapshot.Id);
        Assert.Equal("cmd_editor_save", snapshot.CommandKey);
        Assert.Equal(42, snapshot.Parameter);
        Assert.False(snapshot.AllowWhenTextInputFocused);
        Assert.Equal(CollectionChangeKind.Added, changes[0].ChangeKind);
        registration.Dispose();
        registration.Dispose();
        Assert.False(registration.IsRegistered);
        Assert.Empty(sut.Current);
        Assert.Equal(2, changes.Count);
        Assert.Equal(CollectionChangeKind.Removed, changes[1].ChangeKind);
        Assert.Empty(changes[1].Current);
    }

    [Fact]
    public void Register_IdenticalGestureAndScope_RejectsByDefault()
    {
        var sut = new ShortcutService(new RecordingCommandDispatcher());
        sut.Register(Gesture(Key.S, ModifierKeys.Control), "cmd_editor_save");

        var exception = Assert.Throws<InvalidOperationException>(() =>
            sut.Register(Gesture(Key.S, ModifierKeys.Control), "cmd_editor_save_as")
        );

        Assert.Contains("Ctrl", exception.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Single(sut.Current);
    }

    [Fact]
    public void Register_WithReplacePolicy_InvalidatesConflictingLease()
    {
        var sut = new ShortcutService(new RecordingCommandDispatcher());
        var original = sut.Register(Gesture(Key.S, ModifierKeys.Control), "cmd_editor_save");

        var replacement = sut.Register(
            Gesture(Key.S, ModifierKeys.Control),
            "cmd_editor_save_as",
            options: new ShortcutRegistrationOptions
            {
                ConflictPolicy = ShortcutConflictPolicy.Replace,
            }
        );

        Assert.False(original.IsRegistered);
        Assert.True(replacement.IsRegistered);
        Assert.Equal("cmd_editor_save_as", Assert.Single(sut.Current).CommandKey);
        Assert.True(
            sut.TryResolve(Gesture(Key.S, ModifierKeys.Control), context: null, out var resolved)
        );
        Assert.Equal("cmd_editor_save_as", resolved!.CommandKey);
    }

    [Fact]
    public void TryResolve_AppendedConflicts_UsesHighestPriority()
    {
        var sut = new ShortcutService(new RecordingCommandDispatcher());
        sut.Register(
            Gesture(Key.S, ModifierKeys.Control),
            "cmd_editor_save",
            options: new ShortcutRegistrationOptions { Priority = 1 }
        );
        sut.Register(
            Gesture(Key.S, ModifierKeys.Control),
            "cmd_editor_save_all",
            options: new ShortcutRegistrationOptions
            {
                ConflictPolicy = ShortcutConflictPolicy.Append,
                Priority = 10,
            }
        );

        var resolved = sut.TryResolve(Gesture(Key.S, ModifierKeys.Control), null, out var shortcut);

        Assert.True(resolved);
        Assert.NotNull(shortcut);
        Assert.Equal("cmd_editor_save_all", shortcut.CommandKey);
    }

    [Fact]
    public void TryResolve_PresortedCandidatesUsePriorityThenRegistrationOrder()
    {
        var sut = new ShortcutService(new RecordingCommandDispatcher());
        var gesture = Gesture(Key.S, ModifierKeys.Control);
        sut.Register(
            gesture,
            "cmd_editor_low_priority",
            options: new ShortcutRegistrationOptions { Priority = 1 }
        );
        var firstHighPriority = sut.Register(
            gesture,
            "cmd_editor_first_high_priority",
            options: new ShortcutRegistrationOptions
            {
                ConflictPolicy = ShortcutConflictPolicy.Append,
                Priority = 10,
            }
        );
        sut.Register(
            gesture,
            "cmd_editor_second_high_priority",
            options: new ShortcutRegistrationOptions
            {
                ConflictPolicy = ShortcutConflictPolicy.Append,
                Priority = 10,
            }
        );

        Assert.True(sut.TryResolve(gesture, context: null, out var firstWinner));
        Assert.Equal("cmd_editor_first_high_priority", firstWinner!.CommandKey);

        firstHighPriority.Dispose();

        Assert.True(sut.TryResolve(gesture, context: null, out var secondWinner));
        Assert.Equal("cmd_editor_second_high_priority", secondWinner!.CommandKey);
    }

    [Fact]
    public void TryResolve_GestureIndexIgnoresUnrelatedGesturesAndNonMatchingScopes()
    {
        var sut = new ShortcutService(new RecordingCommandDispatcher());
        foreach (var key in new[] { Key.A, Key.B, Key.C, Key.D, Key.E, Key.F, Key.G })
        {
            sut.Register(Gesture(key, ModifierKeys.Control), $"cmd_unrelated_{key}");
        }

        var targetGesture = Gesture(Key.S, ModifierKeys.Control);
        sut.Register(targetGesture, "cmd_editor_save");
        sut.Register(
            targetGesture,
            "cmd_other_page_save",
            options: new ShortcutRegistrationOptions
            {
                Scope = ShortcutScope.Page,
                ScopeKey = "Other",
                Priority = 100,
            }
        );

        Assert.True(
            sut.TryResolve(
                targetGesture,
                new ShortcutResolutionContext(pageKey: "Editor"),
                out var shortcut
            )
        );
        Assert.Equal("cmd_editor_save", shortcut!.CommandKey);
    }

    [Fact]
    public void TryResolve_PageThenWindowScopesOverrideApplicationAndRequireMatchingContext()
    {
        var sut = new ShortcutService(new RecordingCommandDispatcher());
        var gesture = Gesture(Key.S, ModifierKeys.Control);
        sut.Register(gesture, "cmd_app_save");
        sut.Register(
            gesture,
            "cmd_window_save",
            options: new ShortcutRegistrationOptions
            {
                Scope = ShortcutScope.Window,
                ScopeKey = "Main",
            }
        );
        sut.Register(
            gesture,
            "cmd_page_save",
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
        Assert.Equal("cmd_page_save", pageShortcut!.CommandKey);

        Assert.True(
            sut.TryResolve(
                gesture,
                new ShortcutResolutionContext("Main", "Home"),
                out var windowShortcut
            )
        );
        Assert.Equal("cmd_window_save", windowShortcut!.CommandKey);

        Assert.True(
            sut.TryResolve(
                gesture,
                new ShortcutResolutionContext("Secondary", "Home"),
                out var appShortcut
            )
        );
        Assert.Equal("cmd_app_save", appShortcut!.CommandKey);
    }

    [Fact]
    public void TryResolve_ExactScopeKeyOverridesWildcardWithinSameScope()
    {
        var sut = new ShortcutService(new RecordingCommandDispatcher());
        var gesture = Gesture(Key.S, ModifierKeys.Control);
        sut.Register(
            gesture,
            "cmd_page_wildcard",
            options: new ShortcutRegistrationOptions { Scope = ShortcutScope.Page, Priority = 100 }
        );
        sut.Register(
            gesture,
            "cmd_page_editor",
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
        Assert.Equal("cmd_page_editor", shortcut!.CommandKey);
    }

    [Fact]
    public void TryResolve_TextInputFallsBackFromExactScopeToEligibleWildcard()
    {
        var sut = new ShortcutService(new RecordingCommandDispatcher());
        var gesture = Gesture(Key.S, ModifierKeys.Control);
        sut.Register(
            gesture,
            "cmd_page_wildcard",
            options: new ShortcutRegistrationOptions
            {
                Scope = ShortcutScope.Page,
                AllowWhenTextInputFocused = true,
            }
        );
        sut.Register(
            gesture,
            "cmd_page_editor",
            options: new ShortcutRegistrationOptions
            {
                Scope = ShortcutScope.Page,
                ScopeKey = "Editor",
                Priority = 100,
            }
        );

        Assert.True(
            sut.TryResolve(
                Key.S,
                ModifierKeys.Control,
                new ShortcutResolutionContext(pageKey: "Editor"),
                isTextInputFocused: true,
                out var shortcut
            )
        );
        Assert.Equal("cmd_page_wildcard", shortcut!.CommandKey);
    }

    [Fact]
    public async Task ExecuteAsync_DispatchesResolvedCommandWithParameterAndShortcutSource()
    {
        var dispatcher = new RecordingCommandDispatcher();
        var sut = new ShortcutService(dispatcher);
        var parameter = new object();
        sut.Register(Gesture(Key.F5), "cmd_document_refresh", parameter);

        var result = await sut.ExecuteAsync(Gesture(Key.F5));

        Assert.True(result.IsHandled);
        var invocation = Assert.Single(dispatcher.Invocations);
        Assert.Equal("cmd_document_refresh", invocation.CommandKey);
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
        sut.Register(Gesture(Key.C, ModifierKeys.Control), "cmd_editor_copy");
        sut.Register(
            Gesture(Key.S, ModifierKeys.Control),
            "cmd_document_save",
            options: new ShortcutRegistrationOptions { AllowWhenTextInputFocused = true }
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
        Assert.Equal("cmd_document_save", registration.CommandKey);
        Assert.True(registration.AllowWhenTextInputFocused);
    }

    [Fact]
    public async Task ExecuteResolvedAsync_TextInputWinnerDispatchesWithoutResolvingAgain()
    {
        var dispatcher = new RecordingCommandDispatcher();
        var sut = new ShortcutService(dispatcher);
        var gesture = Gesture(Key.S, ModifierKeys.Control);
        sut.Register(
            gesture,
            "cmd_editor_format",
            options: new ShortcutRegistrationOptions { Priority = 100 }
        );
        var parameter = new object();
        sut.Register(
            gesture,
            "cmd_document_save",
            parameter,
            new ShortcutRegistrationOptions
            {
                ConflictPolicy = ShortcutConflictPolicy.Append,
                AllowWhenTextInputFocused = true,
            }
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

        var result = await sut.ExecuteResolvedAsync(registration!);

        Assert.True(result.IsHandled);
        var invocation = Assert.Single(dispatcher.Invocations);
        Assert.Equal("cmd_document_save", invocation.CommandKey);
        Assert.Same(parameter, invocation.Parameter);
        Assert.Equal(CommandSource.Shortcut, invocation.Source);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task ExecuteResolvedAsync_AfterRemovalOrReplacement_UsesAcceptedSnapshot(
        bool replace
    )
    {
        var dispatcher = new RecordingCommandDispatcher();
        var sut = new ShortcutService(dispatcher);
        var gesture = Gesture(Key.F5);
        var parameter = new object();
        var original = sut.Register(gesture, "cmd_document_refresh", parameter);
        Assert.True(sut.TryResolve(gesture, context: null, out var accepted));

        if (replace)
        {
            sut.Register(
                gesture,
                "cmd_document_reload",
                options: new ShortcutRegistrationOptions
                {
                    ConflictPolicy = ShortcutConflictPolicy.Replace,
                }
            );
        }
        else
        {
            original.Dispose();
        }

        Assert.False(original.IsRegistered);
        var result = await sut.ExecuteResolvedAsync(accepted!);

        Assert.True(result.IsHandled);
        var invocation = Assert.Single(dispatcher.Invocations);
        Assert.Equal("cmd_document_refresh", invocation.CommandKey);
        Assert.Same(parameter, invocation.Parameter);
        Assert.Equal(CommandSource.Shortcut, invocation.Source);
    }

    [Fact]
    public async Task ExecuteResolvedAsync_WithPreCanceledToken_DoesNotDispatch()
    {
        var dispatcher = new RecordingCommandDispatcher();
        var sut = new ShortcutService(dispatcher);
        var gesture = Gesture(Key.F5);
        sut.Register(gesture, "cmd_document_refresh");
        Assert.True(sut.TryResolve(gesture, context: null, out var registration));
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        var result = await sut.ExecuteResolvedAsync(registration!, cancellation.Token);

        Assert.Equal(CommandExecutionStatus.Canceled, result.Status);
        Assert.Empty(dispatcher.Invocations);
    }

    private static KeyGesture Gesture(Key key, ModifierKeys modifiers = ModifierKeys.None)
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
