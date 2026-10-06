using ArkheideSystem.Flourish.Blazor.Components;
using Microsoft.AspNetCore.Components;
using Primitives = ArkheideSystem.Flourish.Blazor.Components.Primitives;

namespace ArkheideSystem.Tests.Flourish.Blazor;

internal static class ComponentInventoryChecks
{
    public static void Register(List<(string Name, Func<Task> Run)> tests)
    {
        tests.Add(("every exported Framework component has explicit source-owned usage guidance", () =>
        {
            var exported = typeof(Button).Assembly.GetExportedTypes()
                .Where(type => !type.IsAbstract && typeof(IComponent).IsAssignableFrom(type))
                .Select(Normalize).ToHashSet();
            var classified = ComponentUsageCatalog.Entries.Keys.ToHashSet();
            Check(exported.SetEquals(classified),
                $"Component inventory differs. Unclassified: {Names(exported.Except(classified))}; stale: {Names(classified.Except(exported))}.");
            foreach (var pair in ComponentUsageCatalog.Entries)
            {
                var info = pair.Value;
                Check(pair.Key == info.ComponentType && pair.Key == Normalize(pair.Key), "Registry keys must be normalized component definitions.");
                Check(Enum.IsDefined(info.Kind), $"Unknown usage kind for {pair.Key.FullName}.");
                Check(!string.IsNullOrWhiteSpace(info.Scenario) && !string.IsNullOrWhiteSpace(info.Guidance),
                    $"Missing scenario or guidance for {pair.Key.FullName}.");
                if (info.Kind is ComponentUseKind.General or ComponentUseKind.Scenario)
                    Check(info.Guidance.Contains("生产使用", StringComparison.Ordinal), $"Supported usage must be explicit for {pair.Key.FullName}.");
            }
            return Task.CompletedTask;
        }));

        tests.Add(("usage lookup normalizes generics and refuses undocumented fallback classifications", () =>
        {
            Check(ReferenceEquals(ComponentUsageCatalog.For(typeof(DataTable<string>)), ComponentUsageCatalog.For(typeof(DataTable<>))),
                "Closed generic controls must use their reviewed generic definition.");
            Check(ReferenceEquals(ComponentUsageCatalog.For(typeof(Primitives.MultiSelectDropdown<string, int>)),
                ComponentUsageCatalog.For(typeof(Primitives.MultiSelectDropdown<,>))), "Two-parameter generic normalization differs.");
            var unknownRejected = false;
            try { _ = ComponentUsageCatalog.For(typeof(string)); }
            catch (KeyNotFoundException) { unknownRejected = true; }
            Check(unknownRejected, "Unreviewed types must not silently receive a production classification.");
            var nullRejected = false;
            try { _ = ComponentUsageCatalog.For(null!); }
            catch (ArgumentNullException) { nullRejected = true; }
            Check(nullRejected, "Null component types must be rejected.");
            return Task.CompletedTask;
        }));

        tests.Add(("preferred entry points are current and have no replacement cycles", () =>
        {
            foreach (var info in ComponentUsageCatalog.Entries.Values)
            {
                var seen = new HashSet<Type> { info.ComponentType };
                var next = info.PreferredEntry;
                while (next is not null)
                {
                    Check(next == Normalize(next) && ComponentUsageCatalog.Entries.ContainsKey(next),
                        $"Unknown preferred entry point for {info.ComponentType.FullName}: {next.FullName}.");
                    Check(seen.Add(next), $"Preferred entry cycle for {info.ComponentType.FullName}.");
                    next = ComponentUsageCatalog.For(next).PreferredEntry;
                }
            }
            return Task.CompletedTask;
        }));

        tests.Add(("primitive guidance distinguishes supported features from incomplete helpers and migration debt", () =>
        {
            foreach (var type in new[]
            {
                typeof(Primitives.EditingGrid), typeof(Primitives.MaskedInput),
                typeof(Primitives.StandaloneMaskedInput), typeof(Primitives.MultiSelectDropdown<,>),
                typeof(Primitives.ReferenceDropdown<>), typeof(Primitives.SearchAutocomplete<>),
                typeof(Primitives.DataPager), typeof(Primitives.NavigationGuard),
                typeof(Primitives.InteractionBoundary), typeof(Primitives.NoticeTrigger), typeof(Primitives.RowActionMenu)
            })
                Check(ComponentUsageCatalog.For(type).Kind == ComponentUseKind.Scenario,
                    $"An independent production feature was mislabeled as an internal primitive: {type.Name}.");

            foreach (var type in new[] { typeof(Primitives.ToggleIndicator), typeof(Primitives.SelectionDropdownSurface), typeof(ExpansionIndicator) })
                Check(ComponentUsageCatalog.For(type).Kind == ComponentUseKind.BuildingBlock, $"Incomplete helper lacks its construction-only warning: {type.Name}.");
            var advanced = ComponentUsageCatalog.For(typeof(Primitives.DataTable<>));
            var search = ComponentUsageCatalog.For(typeof(Primitives.DataSearch<>));
            Check(search.Kind == ComponentUseKind.Compatibility && search.PreferredEntry == typeof(DataSearch<>)
                && search.Guidance.Contains("委托同一", StringComparison.Ordinal), "Legacy search must truthfully name its single canonical renderer.");
            Check(advanced.Kind == ComponentUseKind.Compatibility && advanced.PreferredEntry == typeof(DataTable<>)
                && advanced.Guidance.Contains("仍有独立实现", StringComparison.Ordinal), "Legacy table guidance must name the preferred entry without falsely claiming a shared implementation.");
            var outline = ComponentUsageCatalog.For(typeof(Primitives.PageContents));
            Check(outline.Kind == ComponentUseKind.Compatibility && outline.PreferredEntry == typeof(SectionNavigator)
                && outline.Guidance.Contains("不得用于业务页面", StringComparison.Ordinal), "Static directory cannot stand in for the active business section indicator.");
            var grid = ComponentUsageCatalog.For(typeof(Primitives.UniformGrid));
            Check(grid.Kind == ComponentUseKind.Compatibility && grid.PreferredEntry is null
                && grid.Guidance.Contains("不等价", StringComparison.Ordinal), "The fluid legacy grid must not be presented as a drop-in tile-grid alias.");
            var sheet = ComponentUsageCatalog.For(typeof(Primitives.BottomSheet));
            Check(sheet.Kind == ComponentUseKind.Compatibility && sheet.PreferredEntry is null
                && sheet.Guidance.Contains("OnClose", StringComparison.Ordinal), "The independent sheet close contract must not be hidden by a false replacement pointer.");
            return Task.CompletedTask;
        }));
    }

    private static Type Normalize(Type type) => type.IsGenericType ? type.GetGenericTypeDefinition() : type;
    private static string Names(IEnumerable<Type> types) => string.Join(", ", types.Select(type => type.FullName).Order(StringComparer.Ordinal));
    private static void Check(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }
}
