namespace ArkheideSystem.Flourish.Blazor.Components;

/// <summary>A stable choice with controlled selection and optional ordering constraints.</summary>
public sealed record MultiSelectOption(string Key, string Label, bool Selected = false, bool CanDeselect = true,
    bool CanReorder = true, bool Disabled = false);

/// <summary>The complete validated choice order and selected keys.</summary>
public sealed record MultiSelectChange(IReadOnlyList<string> OrderedKeys, IReadOnlySet<string> SelectedKeys);