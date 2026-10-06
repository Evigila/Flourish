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
                Check(info.Scenario.CatalogId == "Flourish" && info.Guidance.CatalogId == "Flourish" && !string.IsNullOrWhiteSpace(info.Scenario.Token) && !string.IsNullOrWhiteSpace(info.Guidance.FallbackText),
                    $"Missing scenario or guidance for {pair.Key.FullName}.");
                if (info.Kind is ComponentUseKind.General or ComponentUseKind.Scenario)
                    Check(info.Guidance.FallbackText!.StartsWith("Production use:", StringComparison.Ordinal), $"Supported usage must be explicit for {pair.Key.FullName}.");
            }
            return Task.CompletedTask;
        }));

        tests.Add(("usage lookup normalizes generics and refuses undocumented fallback classifications", () =>
        {
            Check(ReferenceEquals(ComponentUsageCatalog.For(typeof(DataTable<string>)), ComponentUsageCatalog.For(typeof(DataTable<>))),
                "Closed generic controls must use their reviewed generic definition.");
            Check(ComponentUsageCatalog.For(typeof(MultiSelectBox)).Kind == ComponentUseKind.General,
                "The shared multi-selection controller must be a general production control.");
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

        tests.Add(("composition guidance names current production entries without cycles", () =>
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

        tests.Add(("primitive guidance distinguishes production scenarios from construction helpers", () =>
        {
            foreach (var type in new[]
            {
                typeof(Primitives.EditingGrid), typeof(Primitives.MaskedInput),
                typeof(Primitives.StandaloneMaskedInput),
                typeof(Primitives.ReferenceDropdown<>), typeof(Primitives.SearchAutocomplete<>),
                typeof(Primitives.DataPager), typeof(Primitives.NavigationGuard),
                typeof(Primitives.InteractionBoundary), typeof(Primitives.NoticeTrigger)
            })
                Check(ComponentUsageCatalog.For(type).Kind == ComponentUseKind.Scenario,
                    $"An independent production feature was mislabeled as an internal primitive: {type.Name}.");

            foreach (var type in new[] { typeof(ExpansionIndicator), typeof(DropdownSurface) })
                Check(ComponentUsageCatalog.For(type).Kind == ComponentUseKind.BuildingBlock, $"Incomplete helper lacks its construction-only warning: {type.Name}.");
            Check(ComponentUsageCatalog.For(typeof(DataTable<>)).Kind == ComponentUseKind.Scenario
                && ComponentUsageCatalog.For(typeof(DataSearch<>)).Kind == ComponentUseKind.Scenario,
                "Canonical record browsing and column search must be production scenarios.");
            Check(ComponentUsageCatalog.For(typeof(SectionNavigator)).Kind == ComponentUseKind.Scenario
                && ComponentUsageCatalog.For(typeof(UniformGrid)).Kind == ComponentUseKind.Scenario
                && ComponentUsageCatalog.For(typeof(Dialog)).Kind == ComponentUseKind.General,
                "The standard directory, grid and dialog variation must have explicit current usage.");
            return Task.CompletedTask;
        }));

        tests.Add(("retired public renderers and compatibility usage classifications do not exist", () =>
        {
            var assembly = typeof(Button).Assembly;
            const string prefix = "ArkheideSystem.Flourish.Blazor.Components.Primitives.";
            foreach (var name in new[]
            {
                "AppIcon", "BottomSheet", "DataTable`1", "DataSearch`1", "DisclosureSection",
                "FilledIdentityCard", "FormActionBar", "FormFields", "FormSurface", "Glyph",
                "PageContent", "PageContents", "PageHeading", "PageLoading", "RecordPageHeading",
                "RowActionMenu", "StatusNotice", "ToggleSection", "ToggleSwitch", "UniformGrid", "MultiSelectDropdown`2", "SelectionDropdownSurface", "ToggleIndicator", "AccessSurface"
            })
                Check(assembly.GetType(prefix + name) is null, $"Retired public renderer remains available: {name}.");
            foreach (var name in new[] { "DisplayOptions", "DisplayOption", "DisplayOptionsChange", "ConfirmationHost", "ReconnectDialog", "BottomSheet", "StaticDialog", "StandaloneNumberBox" })
                Check(assembly.GetType("ArkheideSystem.Flourish.Blazor.Components." + name) is null, "Retired display contract remains available: " + name);
            Check(assembly.GetType("ArkheideSystem.Flourish.Blazor.Components.Patterns.RecordListPage") is null, "Retired record-page wrapper remains available.");
            Check(assembly.GetType("ArkheideSystem.Flourish.Blazor.Hosting.ConfirmationService") is null, "Retired confirmation service remains available.");
            Check(assembly.GetType("ArkheideSystem.Flourish.Blazor.Components.TableSurface") is null,
                "Retired TableSurface renderer remains available.");
            Check(!Enum.GetNames<ComponentUseKind>().Contains("Compatibility", StringComparer.Ordinal),
                "The catalog must not advertise a compatibility component family.");
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
