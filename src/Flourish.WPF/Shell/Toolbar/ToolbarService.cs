using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading;
using System.Windows.Controls;

using ArkheideSystem.Flourish.Abstract;

namespace ArkheideSystem.Flourish.Shell.Toolbar;

/// <summary>
/// Adapts the WPF page-type contract to the platform-neutral toolbar state service.
/// </summary>
internal sealed class ToolbarService : IToolbarService
{
    private readonly Lock mappingGate = new();
    private readonly Dictionary<Type, string> viewKeys = [];
    private readonly Dictionary<string, Type> pageTypes = new(StringComparer.Ordinal);
    private readonly HashSet<string> definedViews = new(StringComparer.Ordinal);
    private readonly IToolbarStateService state;
    private ToolbarSnapshot current;

    public ToolbarService(ToolbarOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        var stateOptions = new ToolbarStateOptions
        {
            IsEnabled = options.IsDynamicToolbarEnabled,
        };
        stateOptions.DefaultItems.AddRange(options.ToolbarItems);

        foreach ((Type pageType, IReadOnlyList<ToolbarItem> items) in options.DynamicToolbarItems)
        {
            string viewKey = RegisterPageType(pageType);
            stateOptions.ViewItems[viewKey] = items;
            stateOptions.ViewIconModes[viewKey] = options.DynamicToolbarIconModes.GetValueOrDefault(
                pageType,
                true
            );
            definedViews.Add(viewKey);
        }

        state = new ToolbarStateService(stateOptions);
        current = CreateSnapshot(state.Current);
        state.Changed += OnStateChanged;
    }

    public event EventHandler<ToolbarChangedEventArgs>? Changed;

    public ToolbarSnapshot Current => Volatile.Read(ref current);

    internal IToolbarStateService State => state;

    public void SetEnabled(bool enabled) => state.SetEnabled(enabled);

    public void SetDefault(IEnumerable<ToolbarItem> items) => state.SetDefault(items);

    public void Set(Type pageType, IEnumerable<ToolbarItem> items, bool iconOnly = true)
    {
        string viewKey = RegisterPageType(pageType);
        MarkDefined(viewKey);
        state.SetView(viewKey, items, iconOnly);
    }

    public void AddItem(ToolbarItem item, Type? pageType = null, int? index = null)
    {
        string? viewKey = RegisterOptionalPageType(pageType);
        MarkDefined(viewKey);
        state.AddItem(item, viewKey, index);
    }

    public void SetItem(ToolbarItem item, Type? pageType = null, int? index = null)
    {
        string? viewKey = RegisterOptionalPageType(pageType);
        MarkDefined(viewKey);
        state.SetItem(item, viewKey, index);
    }

    public bool Remove(string id, Type? pageType = null) =>
        state.Remove(id, RegisterOptionalPageType(pageType));

    public void RemoveAll(Type? pageType = null) =>
        state.RemoveAll(RegisterOptionalPageType(pageType));

    public void SetOrder(string id, int newIndex, Type? pageType = null) =>
        state.SetOrder(id, newIndex, RegisterOptionalPageType(pageType));

    public void SetItemEnabled(string id, bool enabled, Type? pageType = null) =>
        state.SetItemEnabled(id, enabled, RegisterOptionalPageType(pageType));

    public void SetItemVisible(string id, bool visible, Type? pageType = null) =>
        state.SetItemVisible(id, visible, RegisterOptionalPageType(pageType));

    public void SetIconOnly(Type pageType, bool iconOnly) =>
        state.SetIconOnly(RegisterPageType(pageType), iconOnly);

    internal IReadOnlyList<ToolbarItem> GetToolbarItems(Type? pageType = null) =>
        state.GetItems(RegisterOptionalPageType(pageType, validate: false));

    internal bool ShouldShowIconOnly(Type pageType) =>
        state.GetIconOnly(RegisterPageType(pageType, validate: false));

    private void OnStateChanged(object? sender, ToolbarStateChangedEventArgs args)
    {
        ToolbarSnapshot snapshot = CreateSnapshot(args.Current);
        Volatile.Write(ref current, snapshot);

        Changed?.Invoke(
            this,
            new ToolbarChangedEventArgs(
                snapshot,
                args.ChangeKind,
                ResolvePageType(args.ViewKey),
                args.ItemId
            )
        );
    }

    private ToolbarSnapshot CreateSnapshot(ToolbarStateSnapshot snapshot)
    {
        Dictionary<Type, PageToolbarSnapshot> pages;
        lock (mappingGate)
        {
            pages = snapshot.Views
                .Where(pair => definedViews.Contains(pair.Key) && pageTypes.ContainsKey(pair.Key))
                .ToDictionary(
                    pair => pageTypes[pair.Key],
                    pair => new PageToolbarSnapshot(
                        pageTypes[pair.Key],
                        pair.Value.IconOnly,
                        pair.Value.Items
                    )
                );
        }

        return new ToolbarSnapshot(
            snapshot.IsEnabled,
            snapshot.DefaultItems,
            new ReadOnlyDictionary<Type, PageToolbarSnapshot>(pages),
            snapshot.Version
        );
    }

    private string? RegisterOptionalPageType(Type? pageType, bool validate = true) =>
        pageType is null ? null : RegisterPageType(pageType, validate);

    private string RegisterPageType(Type pageType, bool validate = true)
    {
        ArgumentNullException.ThrowIfNull(pageType);
        if (validate && !typeof(Page).IsAssignableFrom(pageType))
        {
            throw new ArgumentException(
                $"{pageType.FullName} must derive from System.Windows.Controls.Page.",
                nameof(pageType)
            );
        }

        lock (mappingGate)
        {
            if (viewKeys.TryGetValue(pageType, out string? existing))
            {
                return existing;
            }

            string viewKey = CreateViewKey(pageType);
            if (pageTypes.TryGetValue(viewKey, out Type? mappedType) && mappedType != pageType)
            {
                throw new InvalidOperationException(
                    $"The stable toolbar view key '{viewKey}' is already mapped to "
                        + $"{mappedType.AssemblyQualifiedName}."
                );
            }

            viewKeys.Add(pageType, viewKey);
            pageTypes[viewKey] = pageType;
            return viewKey;
        }
    }

    private Type? ResolvePageType(string? viewKey)
    {
        if (viewKey is null)
        {
            return null;
        }

        lock (mappingGate)
        {
            return pageTypes.GetValueOrDefault(viewKey);
        }
    }

    private void MarkDefined(string? viewKey)
    {
        if (viewKey is null)
        {
            return;
        }

        lock (mappingGate)
        {
            definedViews.Add(viewKey);
        }
    }

    private static string CreateViewKey(Type pageType) =>
        $"page:{pageType.Assembly.GetName().Name}:{pageType.FullName ?? pageType.Name}";
}
