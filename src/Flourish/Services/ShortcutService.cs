using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Input;
using ArkheideSystem.Flourish.Abstract;

namespace ArkheideSystem.Flourish.Services;

internal sealed class ShortcutService(ICommandDispatcher commandDispatcher) : IShortcutService
{
    private readonly object gate = new();
    private readonly ICommandDispatcher commandDispatcher =
        commandDispatcher ?? throw new ArgumentNullException(nameof(commandDispatcher));
    private readonly List<ShortcutEntry> entries = [];
    private long nextSequence;
    private long version;

    public IReadOnlyList<ShortcutRegistrationInfo> Registrations
    {
        get
        {
            lock (gate)
            {
                return CreateSnapshotLocked();
            }
        }
    }

    public event EventHandler<ShortcutRegistryChangedEventArgs>? Changed;

    public IShortcutRegistration Register(
        KeyGesture gesture,
        string commandKey,
        object? parameter = null,
        ShortcutRegistrationOptions? options = null
    )
    {
        ValidateGesture(gesture);
        ValidateCommandKey(commandKey);
        options ??= new ShortcutRegistrationOptions();
        ValidateOptions(options);

        var scopeKey = NormalizeScopeKey(options.Scope, options.ScopeKey);
        var storedGesture = CloneGesture(gesture);
        ShortcutEntry entry;
        ShortcutRegistryChangedEventArgs changed;
        lock (gate)
        {
            var conflicts = entries
                .Where(candidate =>
                    candidate.IsRegistered
                    && GestureEquals(candidate.Gesture, storedGesture)
                    && candidate.Scope == options.Scope
                    && string.Equals(candidate.ScopeKey, scopeKey, StringComparison.Ordinal)
                )
                .ToArray();

            if (
                conflicts.Length > 0
                && options.ConflictPolicy == ShortcutConflictPolicy.Reject
            )
            {
                throw new InvalidOperationException(
                    $"Shortcut '{FormatGesture(storedGesture)}' is already registered in the {options.Scope} scope."
                );
            }

            var changeKind = ShortcutRegistryChangeKind.Registered;
            if (
                conflicts.Length > 0
                && options.ConflictPolicy == ShortcutConflictPolicy.Replace
            )
            {
                foreach (var conflict in conflicts)
                {
                    conflict.TryDeactivate();
                    entries.Remove(conflict);
                }

                changeKind = ShortcutRegistryChangeKind.Replaced;
            }

            entry = new ShortcutEntry(
                storedGesture,
                commandKey,
                parameter,
                options.Scope,
                scopeKey,
                options.Priority,
                options.AllowWhenTextInputFocused,
                nextSequence++
            );
            entries.Add(entry);
            changed = CreateChangedEventArgsLocked(changeKind, entry.CreateSnapshot());
        }

        RaiseChanged(changed);
        return new ShortcutRegistration(this, entry);
    }

    public bool TryResolve(
        KeyGesture gesture,
        ShortcutResolutionContext? context,
        out ShortcutRegistrationInfo? registration
    )
    {
        ValidateGesture(gesture);
        var entry = ResolveEntry(
            gesture.Key,
            gesture.Modifiers,
            context,
            isTextInputFocused: false
        );
        registration = entry?.CreateSnapshot();
        return registration is not null;
    }

    internal bool TryResolve(
        Key key,
        ModifierKeys modifiers,
        ShortcutResolutionContext? context,
        bool isTextInputFocused,
        out ShortcutRegistrationInfo? registration
    )
    {
        var entry = ResolveEntry(key, modifiers, context, isTextInputFocused);
        registration = entry?.CreateSnapshot();
        return registration is not null;
    }

    public ValueTask<CommandResult> ExecuteAsync(
        KeyGesture gesture,
        ShortcutResolutionContext? context = null,
        CancellationToken cancellationToken = default
    )
    {
        ValidateGesture(gesture);
        if (cancellationToken.IsCancellationRequested)
        {
            return ValueTask.FromResult(CommandResult.Canceled);
        }

        var entry = ResolveEntry(
            gesture.Key,
            gesture.Modifiers,
            context,
            isTextInputFocused: false
        );
        if (entry is null)
        {
            return ValueTask.FromResult(CommandResult.NotHandled);
        }

        return commandDispatcher.ExecuteAsync(
            entry.CommandKey,
            entry.Parameter,
            CommandSource.Shortcut,
            cancellationToken
        );
    }

    private ShortcutEntry? ResolveEntry(
        Key key,
        ModifierKeys modifiers,
        ShortcutResolutionContext? context,
        bool isTextInputFocused
    )
    {
        lock (gate)
        {
            return entries
                .Where(entry =>
                    entry.IsRegistered
                    && GestureEquals(entry.Gesture, key, modifiers)
                    && IsEligible(entry, context)
                    && (!isTextInputFocused || entry.AllowWhenTextInputFocused)
                )
                .OrderByDescending(entry => GetScopePrecedence(entry.Scope))
                .ThenByDescending(entry => GetScopeKeyPrecedence(entry, context))
                .ThenByDescending(entry => entry.Priority)
                .ThenBy(entry => entry.Sequence)
                .FirstOrDefault();
        }
    }

    private void Unregister(ShortcutEntry entry)
    {
        ShortcutRegistryChangedEventArgs? changed = null;
        lock (gate)
        {
            if (!entry.IsRegistered || !entries.Contains(entry))
            {
                return;
            }

            var removed = entry.CreateSnapshot();
            entry.TryDeactivate();
            entries.Remove(entry);
            changed = CreateChangedEventArgsLocked(
                ShortcutRegistryChangeKind.Unregistered,
                removed
            );
        }

        RaiseChanged(changed);
    }

    private ReadOnlyCollection<ShortcutRegistrationInfo> CreateSnapshotLocked()
    {
        return new ReadOnlyCollection<ShortcutRegistrationInfo>(
            entries
                .Where(entry => entry.IsRegistered)
                .OrderBy(entry => entry.Sequence)
                .Select(entry => entry.CreateSnapshot())
                .ToArray()
        );
    }

    private ShortcutRegistryChangedEventArgs CreateChangedEventArgsLocked(
        ShortcutRegistryChangeKind changeKind,
        ShortcutRegistrationInfo affectedShortcut
    )
    {
        version++;
        return new ShortcutRegistryChangedEventArgs(
            version,
            changeKind,
            affectedShortcut,
            CreateSnapshotLocked()
        );
    }

    private void RaiseChanged(ShortcutRegistryChangedEventArgs eventArgs)
    {
        var handlers = Changed;
        if (handlers is null)
        {
            return;
        }

        foreach (
            EventHandler<ShortcutRegistryChangedEventArgs> handler in handlers.GetInvocationList()
        )
        {
            try
            {
                handler(this, eventArgs);
            }
            catch (Exception error)
            {
                Debug.WriteLine($"Flourish shortcut registry event handler failed: {error}");
            }
        }
    }

    private static bool IsEligible(
        ShortcutEntry entry,
        ShortcutResolutionContext? context
    )
    {
        return entry.Scope switch
        {
            ShortcutScope.Application => true,
            ShortcutScope.Window =>
                context?.WindowKey is not null
                && (
                    entry.ScopeKey is null
                    || string.Equals(
                        entry.ScopeKey,
                        context.WindowKey,
                        StringComparison.Ordinal
                    )
                ),
            ShortcutScope.Page =>
                context?.PageKey is not null
                && (
                    entry.ScopeKey is null
                    || string.Equals(
                        entry.ScopeKey,
                        context.PageKey,
                        StringComparison.Ordinal
                    )
                ),
            _ => false,
        };
    }

    private static int GetScopePrecedence(ShortcutScope scope)
    {
        return scope switch
        {
            ShortcutScope.Page => 2,
            ShortcutScope.Window => 1,
            _ => 0,
        };
    }

    private static int GetScopeKeyPrecedence(
        ShortcutEntry entry,
        ShortcutResolutionContext? context
    )
    {
        if (entry.ScopeKey is null)
        {
            return 0;
        }

        return entry.Scope switch
        {
            ShortcutScope.Window
                when string.Equals(
                    entry.ScopeKey,
                    context?.WindowKey,
                    StringComparison.Ordinal
                ) => 1,
            ShortcutScope.Page
                when string.Equals(
                    entry.ScopeKey,
                    context?.PageKey,
                    StringComparison.Ordinal
                ) => 1,
            _ => 0,
        };
    }

    private static KeyGesture CloneGesture(KeyGesture gesture)
    {
        return new KeyGesture(gesture.Key, gesture.Modifiers, gesture.DisplayString);
    }

    private static bool GestureEquals(KeyGesture left, KeyGesture right)
    {
        return left.Key == right.Key && left.Modifiers == right.Modifiers;
    }

    private static bool GestureEquals(
        KeyGesture gesture,
        Key key,
        ModifierKeys modifiers
    )
    {
        return gesture.Key == key && gesture.Modifiers == modifiers;
    }

    private static string FormatGesture(KeyGesture gesture)
    {
        return string.IsNullOrWhiteSpace(gesture.DisplayString)
            ? gesture.GetDisplayStringForCulture(
                System.Globalization.CultureInfo.InvariantCulture
            )
            : gesture.DisplayString;
    }

    private static void ValidateGesture(KeyGesture gesture)
    {
        ArgumentNullException.ThrowIfNull(gesture);
        if (gesture.Key == Key.None)
        {
            throw new ArgumentException(
                "Shortcut gesture must include a non-empty key.",
                nameof(gesture)
            );
        }
    }

    private static void ValidateCommandKey(string commandKey)
    {
        if (string.IsNullOrWhiteSpace(commandKey))
        {
            throw new ArgumentException("Command key cannot be empty.", nameof(commandKey));
        }
    }

    private static void ValidateOptions(ShortcutRegistrationOptions options)
    {
        if (!Enum.IsDefined(options.Scope))
        {
            throw new ArgumentOutOfRangeException(
                nameof(options),
                options.Scope,
                "The shortcut scope is not defined."
            );
        }

        if (!Enum.IsDefined(options.ConflictPolicy))
        {
            throw new ArgumentOutOfRangeException(
                nameof(options),
                options.ConflictPolicy,
                "The shortcut conflict policy is not defined."
            );
        }

        if (
            options.Scope == ShortcutScope.Application
            && !string.IsNullOrWhiteSpace(options.ScopeKey)
        )
        {
            throw new ArgumentException(
                "Application-scoped shortcuts cannot specify a scope key.",
                nameof(options)
            );
        }
    }

    private static string? NormalizeScopeKey(ShortcutScope scope, string? scopeKey)
    {
        return scope == ShortcutScope.Application || string.IsNullOrWhiteSpace(scopeKey)
            ? null
            : scopeKey;
    }

    private sealed class ShortcutEntry(
        KeyGesture gesture,
        string commandKey,
        object? parameter,
        ShortcutScope scope,
        string? scopeKey,
        int priority,
        bool allowWhenTextInputFocused,
        long sequence
    )
    {
        private int isRegistered = 1;

        public Guid Id { get; } = Guid.NewGuid();

        public KeyGesture Gesture { get; } = gesture;

        public string CommandKey { get; } = commandKey;

        public object? Parameter { get; } = parameter;

        public ShortcutScope Scope { get; } = scope;

        public string? ScopeKey { get; } = scopeKey;

        public int Priority { get; } = priority;

        public bool AllowWhenTextInputFocused { get; } = allowWhenTextInputFocused;

        public long Sequence { get; } = sequence;

        public bool IsRegistered => Volatile.Read(ref isRegistered) != 0;

        public bool TryDeactivate()
        {
            return Interlocked.Exchange(ref isRegistered, 0) != 0;
        }

        public ShortcutRegistrationInfo CreateSnapshot()
        {
            return new ShortcutRegistrationInfo(
                Id,
                Gesture,
                CommandKey,
                Parameter,
                Scope,
                ScopeKey,
                Priority,
                AllowWhenTextInputFocused
            );
        }
    }

    private sealed class ShortcutRegistration(
        ShortcutService owner,
        ShortcutEntry entry
    ) : IShortcutRegistration
    {
        public Guid Id => entry.Id;

        public bool IsRegistered => entry.IsRegistered;

        public void Dispose()
        {
            owner.Unregister(entry);
        }
    }
}
