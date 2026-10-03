using Microsoft.JSInterop;

namespace ArkheideSystem.Flourish.Blazor.Components.Primitives;

/// <summary>Host operations invoked by the grid's keyboard and clipboard interaction bridge.</summary>
public sealed class GridInteractions
{
    public Func<int, int, Task>? Begin { get; init; }
    public Func<Task>? End { get; init; }
    public Func<Task>? Cancel { get; init; }
    public Func<Task>? Undo { get; init; }
    public Func<Task>? Redo { get; init; }
    public Func<Task>? Save { get; init; }
    public Func<Task<bool>>? LoadMore { get; init; }
    public Func<IJSStreamReference, Task<bool>>? ApplyCells { get; init; }
    public Func<int, int, IJSStreamReference, Task>? Paste { get; init; }
    public Func<string, Task>? ReportError { get; init; }
}
