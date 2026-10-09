using System.Windows;
using ArkheideSystem.Flourish.WPF;
using ArkheideSystem.Flourish.WPF.Abstract;
using F = ArkheideSystem.Flourish.WPF.Controls;

namespace ArkheideSystem.Gallery.Flourish.WPF;

public static partial class Samples
{
    private static object Spreadsheet()
    {
        var records = RecordsSource();
        var status = Status("Edit cells. Ctrl+S asks the host to save; Ctrl+Z/Ctrl+Y ask the host to undo/redo.");
        var grid = new F.EditingGrid
        {
            Columns = new[]
            {
                new GridColumn("id", "ID", ReadOnly: true, BindingPath: nameof(SampleRecord.Id)),
                new GridColumn("name", "Name", Required: true, BindingPath: nameof(SampleRecord.Name), MaximumLength: 80),
                new GridColumn("department", "Department", GridEditorKind.Select, Options: [new("Design", "Design"), new("Engineering", "Engineering")], BindingPath: nameof(SampleRecord.Department)),
                new GridColumn("amount", "Amount", GridEditorKind.Decimal, BindingPath: nameof(SampleRecord.Amount), Validate: value => Convert.ToDouble(value) < 0 ? "Amount cannot be negative." : null)
            },
            ItemsSource = records,
            Height = 400
        };
        var undo = new Stack<(SampleRecord Item, string Column, object? Previous, object? Next)>();
        var redo = new Stack<(SampleRecord Item, string Column, object? Previous, object? Next)>();
        object? Read(SampleRecord row, string column) => column switch { "name" => row.Name, "department" => row.Department, "amount" => row.Amount, _ => row.Id };
        void Write(SampleRecord row, string column, object? value)
        {
            switch (column)
            {
                case "name": row.Name = Convert.ToString(value) ?? ""; break;
                case "department": row.Department = Convert.ToString(value) ?? ""; break;
                case "amount": row.Amount = Convert.ToDouble(value); break;
            }
        }
        void Accept(GridCellChange change)
        {
            var record = (SampleRecord)change.Item;
            undo.Push((record, change.ColumnKey, Read(record, change.ColumnKey), change.Value));
            redo.Clear();
            Write(record, change.ColumnKey, change.Value);
            status.Title = record.Name + " changed";
        }
        grid.CellChanged += (_, change) => { Accept(change); grid.Dispatcher.InvokeAsync(grid.Refresh); };
        grid.CellsChanged += (_, changes) => { foreach (var change in changes) Accept(change); grid.Refresh(); };
        grid.SaveRequested += (_, _) => status.Title = "Save requested for " + records.Count + " in-memory records";
        grid.UndoRequested += (_, _) =>
        {
            if (undo.TryPop(out var change)) { Write(change.Item, change.Column, change.Previous); redo.Push(change); grid.Refresh(); status.Title = "Undo accepted"; }
        };
        grid.RedoRequested += (_, _) =>
        {
            if (redo.TryPop(out var change)) { Write(change.Item, change.Column, change.Next); undo.Push(change); grid.Refresh(); status.Title = "Redo accepted"; }
        };
        grid.EditError += (_, error) => status.Title = error.Message;
        return Group(grid, status);
    }

    private static object Chart(ITextProvider texts)
    {
        var independent = new F.ToggleSwitch { Content = "Independent scales" };
        var chart = new F.LineChart
        {
            TextProvider = texts,
            Title = "Monthly activity",
            Heading = "Monthly activity",
            Labels = new[] { new ChartPointLabel("jan", "Jan"), new("feb", "Feb"), new("mar", "Mar"), new("apr", "Apr") },
            Series = new[] { new ChartSeries("visits", "Visits", [40, 55, 38, 75]), new("orders", "Orders", [4, 8, 6, 12]) },
            ControlsContent = independent,
            Height = 400
        };
        independent.Click += (_, _) => chart.IndependentScales = independent.IsChecked == true;
        return chart;
    }

    private static object Dialogs(Window owner)
    {
        var status = Status("Await a result from the shared Dialog lifecycle");
        var bottomSheet = new F.ToggleSwitch { Content = "Bottom sheet presentation" };
        var open = new F.Button { Content = "Open dialog" };
        open.Click += async (_, _) =>
        {
            using var dialog = new F.Dialog
            {
                Title = "Confirm operation",
                Presentation = bottomSheet.IsChecked == true ? DialogPresentation.BottomSheet : DialogPresentation.Centered,
                Views = new[] { new DialogView("confirm", new F.Notice { Title = "Run this sample operation?", Content = "The consumer owns operation state." }), new DialogView("busy", new F.LoadingState { Title = "Processing…" }) },
                View = "confirm"
            };
            var cancel = new F.Button { Content = "Cancel", Variant = ButtonVariant.Secondary };
            var confirm = new F.Button { Content = "Confirm" };
            cancel.Click += async (_, _) => await dialog.CloseAsync();
            confirm.Click += async (_, _) =>
            {
                dialog.Busy = true;
                confirm.Busy = true;
                cancel.Disabled = true;
                dialog.View = "busy";
                await Task.Delay(700);
                dialog.Busy = false;
                await dialog.CloseAsync("confirmed");
            };
            dialog.Actions = new F.InlineActions { Children = { cancel, confirm } };
            var result = await dialog.ShowAsync(owner);
            status.Title = result?.ToString() ?? "Cancelled";
        };
        return Group(bottomSheet, open, status);
    }

    private static object Shell()
    {
        var options = new FrameworkBuilder()
            .ConfigureProject(project => project.SetProjectName("Sample application"))
            .ConfigureNavigation(navigation =>
            {
                navigation.AddNav("Home", "home", "/", () => new F.PageBody { Content = new F.Section { Title = "Home", Content = "The native shell owns layout and navigation presentation." } });
                navigation.AddNav("Records", "table_chart", "/records", () => Records(),
                    secondary => secondary.AddSubNav("Read-only", "view_list", "/records/read-only", () => new F.ListView { Columns = Columns, ItemsSource = RecordsSource() }));
            }).Build();
        var shell = new F.ApplicationShell { Options = options, Height = 480 };
        shell.Loaded += async (_, _) => await shell.NavigateAsync("/");
        return shell;
    }
}
