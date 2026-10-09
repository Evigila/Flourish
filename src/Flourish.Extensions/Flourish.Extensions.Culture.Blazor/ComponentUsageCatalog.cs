using System.Collections.Frozen;
using ArkheideSystem.Flourish.Blazor.Abstract;
using ArkheideSystem.Flourish.Blazor.Components;

namespace ArkheideSystem.Flourish.Extensions.Culture.Blazor;

/// <summary>Explicit usage inventory for the optional Culture extension's exported controls.</summary>
public static class ComponentUsageCatalog
{
    /// <summary>Gets the reviewed production entries.</summary>
    public static IReadOnlyDictionary<Type, ComponentUsageInfo> Entries { get; } = new[]
    {
        new ComponentUsageInfo(typeof(LanguagePicker), ComponentUseKind.Scenario,
            new TextReference("Culture", "PickerScenario"), new TextReference("Culture", "PickerGuidance"))
    }.ToFrozenDictionary(entry => entry.ComponentType);

    /// <summary>Gets the reviewed usage entry for an exported control.</summary>
    public static ComponentUsageInfo For(Type componentType)
    {
        ArgumentNullException.ThrowIfNull(componentType);
        return Entries.TryGetValue(componentType, out var entry) ? entry
            : throw new KeyNotFoundException($"No reviewed component usage entry exists for {componentType.FullName}.");
    }
}
