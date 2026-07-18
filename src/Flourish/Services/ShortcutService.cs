using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows.Input;
using ArkheideSystem.Flourish.Abstract;

namespace ArkheideSystem.Flourish.Services;

internal sealed class ShortcutService(ICommandDispatcher commandDispatcher) : IShortcutService
{
    private readonly Lock gate = new();
    private readonly ICommandDispatcher commandDispatcher =
        commandDispatcher ?? throw new ArgumentNullException(nameof(commandDispatcher));
    private readonly List<ShortcutEntry> entries = [];
    private readonly Dictionary<ShortcutGestureKey, ShortcutGestureIndex> entriesByGesture = [];
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
            var gestureKey = new ShortcutGestureKey(
                storedGesture.Key,
                storedGesture.Modifiers
            );
            if (!entriesByGesture.TryGetValue(gestureKey, out var gestureIndex))
            {
                gestureIndex = new ShortcutGestureIndex();
                entriesByGesture.Add(gestureKey, gestureIndex);
            }

            var conflictGroup = gestureIndex.GetGroup(options.Scope, scopeKey);

            if (conflictGroup.Count > 0 && options.ConflictPolicy == ShortcutConflictPolicy.Reject)
            {
                throw new InvalidOperationException(
                    $"Shortcut '{FormatGesture(storedGesture)}' is already registered in the {options.Scope} scope."
                );
            }

            var changeKind = ShortcutRegistryChangeKind.Registered;
            if (
                conflictGroup.Count > 0
                && options.ConflictPolicy == ShortcutConflictPolicy.Replace
            )
            {
                foreach (var conflict in conflictGroup.ToArray())
                {
                    RemoveEntryLocked(
                        conflict,
                        gestureKey,
                        gestureIndex,
                        removeEmptyIndex: false
                    );
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
            gestureIndex.Add(entry);
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

    internal bool HasRegistrations(Key key, ModifierKeys modifiers)
    {
        lock (gate)
        {
            return entriesByGesture.ContainsKey(new ShortcutGestureKey(key, modifiers));
        }
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

        return DispatchAsync(entry.CommandKey, entry.Parameter, cancellationToken);
    }

    internal ValueTask<CommandResult> ExecuteResolvedAsync(
        ShortcutRegistrationInfo registration,
        CancellationToken cancellationToken = default
    )
    {
        ArgumentNullException.ThrowIfNull(registration);
        if (cancellationToken.IsCancellationRequested)
        {
            return ValueTask.FromResult(CommandResult.Canceled);
        }

        return DispatchAsync(registration.CommandKey, registration.Parameter, cancellationToken);
    }

    private ValueTask<CommandResult> DispatchAsync(
        string commandKey,
        object? parameter,
        CancellationToken cancellationToken
    )
    {
        return commandDispatcher.ExecuteAsync(
            commandKey,
            parameter,
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
            return entriesByGesture.TryGetValue(
                new ShortcutGestureKey(key, modifiers),
                out var gestureIndex
            )
                ? gestureIndex.Resolve(context, isTextInputFocused)
                : null;
        }
    }

    private void Unregister(ShortcutEntry entry)
    {
        ShortcutRegistryChangedEventArgs? changed = null;
        lock (gate)
        {
            var gestureKey = new ShortcutGestureKey(
                entry.Gesture.Key,
                entry.Gesture.Modifiers
            );
            if (
                !entry.IsRegistered
                || !entriesByGesture.TryGetValue(gestureKey, out var gestureIndex)
                || !RemoveEntryLocked(entry, gestureKey, gestureIndex)
            )
            {
                return;
            }

            var removed = entry.CreateSnapshot();
            changed = CreateChangedEventArgsLocked(
                ShortcutRegistryChangeKind.Unregistered,
                removed
            );
        }

        RaiseChanged(changed);
    }

    private bool RemoveEntryLocked(
        ShortcutEntry entry,
        ShortcutGestureKey gestureKey,
        ShortcutGestureIndex gestureIndex,
        bool removeEmptyIndex = true
    )
    {
        if (!entries.Remove(entry))
        {
            return false;
        }

        entry.TryDeactivate();
        gestureIndex.Remove(entry);
        if (removeEmptyIndex && gestureIndex.IsEmpty)
        {
            entriesByGesture.Remove(gestureKey);
        }

        return true;
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

    private static KeyGesture CloneGesture(KeyGesture gesture)
    {
        return new KeyGesture(gesture.Key, gesture.Modifiers, gesture.DisplayString);
    }

    private static string FormatGesture(KeyGesture gesture)
    {
        return string.IsNullOrWhiteSpace(gesture.DisplayString)
            ? gesture.GetDisplayStringForCulture(System.Globalization.CultureInfo.InvariantCulture)
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

    private readonly record struct ShortcutGestureKey(Key Key, ModifierKeys Modifiers);

    private sealed class ShortcutGestureIndex
    {
        private readonly List<ShortcutEntry> applicationEntries = [];
        private readonly List<ShortcutEntry> pageWildcardEntries = [];
        private readonly Dictionary<string, List<ShortcutEntry>> pageEntries = new(
            StringComparer.Ordinal
        );
        private readonly List<ShortcutEntry> windowWildcardEntries = [];
        private readonly Dictionary<string, List<ShortcutEntry>> windowEntries = new(
            StringComparer.Ordinal
        );

        public bool IsEmpty =>
            applicationEntries.Count == 0
            && pageWildcardEntries.Count == 0
            && pageEntries.Count == 0
            && windowWildcardEntries.Count == 0
            && windowEntries.Count == 0;

        public List<ShortcutEntry> GetGroup(ShortcutScope scope, string? scopeKey)
        {
            return scope switch
            {
                ShortcutScope.Application => applicationEntries,
                ShortcutScope.Page when scopeKey is null => pageWildcardEntries,
                ShortcutScope.Page => GetOrCreate(pageEntries, scopeKey),
                ShortcutScope.Window when scopeKey is null => windowWildcardEntries,
                ShortcutScope.Window => GetOrCreate(windowEntries, scopeKey),
                _ => throw new ArgumentOutOfRangeException(nameof(scope), scope, null),
            };
        }

        public void Add(ShortcutEntry entry)
        {
            var group = GetGroup(entry.Scope, entry.ScopeKey);
            var insertionIndex = group.BinarySearch(entry, ShortcutEntryComparer.Instance);
            group.Insert(insertionIndex < 0 ? ~insertionIndex : insertionIndex, entry);
        }

        public void Remove(ShortcutEntry entry)
        {
            var group = GetExistingGroup(entry.Scope, entry.ScopeKey);
            if (group is null || !group.Remove(entry) || group.Count > 0 || entry.ScopeKey is null)
            {
                return;
            }

            if (entry.Scope == ShortcutScope.Page)
            {
                pageEntries.Remove(entry.ScopeKey);
            }
            else if (entry.Scope == ShortcutScope.Window)
            {
                windowEntries.Remove(entry.ScopeKey);
            }
        }

        public ShortcutEntry? Resolve(
            ShortcutResolutionContext? context,
            bool isTextInputFocused
        )
        {
            if (context?.PageKey is { } pageKey)
            {
                if (
                    pageEntries.TryGetValue(pageKey, out var exactPageEntries)
                    && Resolve(exactPageEntries, isTextInputFocused) is { } pageEntry
                )
                {
                    return pageEntry;
                }

                if (Resolve(pageWildcardEntries, isTextInputFocused) is { } pageWildcard)
                {
                    return pageWildcard;
                }
            }

            if (context?.WindowKey is { } windowKey)
            {
                if (
                    windowEntries.TryGetValue(windowKey, out var exactWindowEntries)
                    && Resolve(exactWindowEntries, isTextInputFocused) is { } windowEntry
                )
                {
                    return windowEntry;
                }

                if (Resolve(windowWildcardEntries, isTextInputFocused) is { } windowWildcard)
                {
                    return windowWildcard;
                }
            }

            return Resolve(applicationEntries, isTextInputFocused);
        }

        private List<ShortcutEntry>? GetExistingGroup(ShortcutScope scope, string? scopeKey)
        {
            return scope switch
            {
                ShortcutScope.Application => applicationEntries,
                ShortcutScope.Page when scopeKey is null => pageWildcardEntries,
                ShortcutScope.Page => pageEntries.GetValueOrDefault(scopeKey),
                ShortcutScope.Window when scopeKey is null => windowWildcardEntries,
                ShortcutScope.Window => windowEntries.GetValueOrDefault(scopeKey),
                _ => null,
            };
        }

        private static List<ShortcutEntry> GetOrCreate(
            Dictionary<string, List<ShortcutEntry>> entriesByScopeKey,
            string scopeKey
        )
        {
            if (!entriesByScopeKey.TryGetValue(scopeKey, out var entries))
            {
                entries = [];
                entriesByScopeKey.Add(scopeKey, entries);
            }

            return entries;
        }

        private static ShortcutEntry? Resolve(
            List<ShortcutEntry> entries,
            bool isTextInputFocused
        )
        {
            foreach (var entry in entries)
            {
                if (
                    entry.IsRegistered
                    && (!isTextInputFocused || entry.AllowWhenTextInputFocused)
                )
                {
                    return entry;
                }
            }

            return null;
        }
    }

    private sealed class ShortcutEntryComparer : IComparer<ShortcutEntry>
    {
        public static ShortcutEntryComparer Instance { get; } = new();

        public int Compare(ShortcutEntry? left, ShortcutEntry? right)
        {
            if (ReferenceEquals(left, right))
            {
                return 0;
            }

            if (left is null)
            {
                return 1;
            }

            if (right is null)
            {
                return -1;
            }

            var priorityComparison = right.Priority.CompareTo(left.Priority);
            return priorityComparison != 0
                ? priorityComparison
                : left.Sequence.CompareTo(right.Sequence);
        }
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

    private sealed class ShortcutRegistration(ShortcutService owner, ShortcutEntry entry)
        : IShortcutRegistration
    {
        public Guid Id => entry.Id;

        public bool IsRegistered => entry.IsRegistered;

        public void Dispose()
        {
            owner.Unregister(entry);
        }
    }
}
