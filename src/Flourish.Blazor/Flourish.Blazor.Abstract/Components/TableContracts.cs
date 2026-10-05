using System.Globalization;

namespace ArkheideSystem.Flourish.Blazor.Components;

/// <summary>Presentation metadata; the caller retains business and authorization decisions.</summary>
public sealed record TableColumn<TItem>(
    string Key,
    string Header,
    Func<TItem, object?> Value,
    bool Sortable = true,
    bool Searchable = true,
    bool CanHide = true,
    Func<TItem, string?>? Format = null);

/// <summary>A read-only cell and its culture-formatted text, using the shared table column contract.</summary>
public sealed record TableCellContext<TItem>(TItem Item, TableColumn<TItem> Column, string Text);

/// <summary>A record action supplied by the consumer, without built-in routes or business rules.</summary>
public sealed record RowAction<TItem>(
    string Label,
    Func<TItem, Task> OnClick,
    Func<TItem, bool>? IsAvailable = null,
    bool Destructive = false,
    bool Disabled = false);

public enum TableView { Table, Cards }

public enum TableSortDirection { Default, Descending, Ascending }

/// <summary>Replace visible table captions without coupling the library to a product locale.</summary>
public sealed record TableText(
    string SearchBy = "Search by",
    string AllColumns = "All columns",
    string SearchContent = "Content",
    string List = "List",
    string Cards = "Cards",
    string Display = "Display",
    string Actions = "Actions",
    string Open = "Open",
    string Items = "Items",
    string Of = "of",
    string Page = "Page",
    string PreviousPage = "Previous page",
    string NextPage = "Next page",
    string Width = "Width",
    string SortDescending = "sort descending",
    string SortAscending = "sort ascending",
    string SortDefault = "restore default order",
    string ResizeHint = "Drag or use arrow keys; Shift changes 50 px; Home restores; Escape cancels.",
    string ItemsPerPage = "Items per page",
    string Total = "Total",
    string ItemRangeFormat = "{0} {1}-{2} / {3} {4}");
