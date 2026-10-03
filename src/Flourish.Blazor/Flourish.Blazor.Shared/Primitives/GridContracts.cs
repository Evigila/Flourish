namespace ArkheideSystem.Flourish.Blazor.Components.Primitives;

public enum GridEditorKind { Text, Decimal, Date, Multiline, Select, Masked, Link }

public sealed record GridOption(string Value, string Label);
public sealed record GridColumn(string Key, string Label, bool Required = false, bool Identity = false);
public sealed record GridRow(object Key, IReadOnlyList<GridCell> Cells, bool Dirty = false, string? Label = null);
public sealed record GridCellChange(int Row, int Column, string? Value);

/// <summary>One cell's presentation; the host retains parsing, permission checks and persistence.</summary>
public sealed record GridCell(
    string? Value,
    string Display,
    GridEditorKind Editor = GridEditorKind.Text,
    IReadOnlyList<GridOption>? Options = null,
    bool AllowEmpty = true,
    string EmptyText = "",
    string? Mask = null,
    int? MaximumLength = null,
    bool Disabled = false,
    bool ReadOnly = false,
    bool ReadOnlyAppearance = false,
    bool Required = false,
    bool Changed = false,
    string? Error = null,
    string? SecondaryError = null,
    string? LinkHref = null,
    string? LinkLabel = null,
    string? Hint = null,
    string? AriaLabel = null);

public sealed record GridText(
    string ColumnWidth = "Largura de {0}",
    string Pixels = "{0} pixels",
    string ResizeHint = "Arraste para ajustar a largura; Home restaura a largura automática");
