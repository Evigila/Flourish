using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using ArkheideSystem.Flourish.Controls;
using FlourishDataGrid = ArkheideSystem.Flourish.Controls.DataGrid;
using FlourishScrollViewer = ArkheideSystem.Flourish.Controls.ScrollViewer;

namespace ArkheideSystem.Flourish.Test.Controls;

public sealed class FlourishDataGridTests
{
    private const string GenericThemeSource = "/Flourish;component/Themes/Generic.xaml";

    [Fact]
    public void DataGrid_PreservesNativeContractAndExposesReadOnlyCounts()
    {
        RunInSta(() =>
        {
            var grid = new FlourishDataGrid();

            Assert.IsAssignableFrom<System.Windows.Controls.DataGrid>(grid);
            Assert.True(FlourishDataGrid.RowCountProperty.ReadOnly);
            Assert.True(FlourishDataGrid.ColumnCountProperty.ReadOnly);
            Assert.False(FlourishDataGrid.FirstColumnForegroundProperty.ReadOnly);
            var firstColumnBrush = Assert.IsType<SolidColorBrush>(grid.FirstColumnForeground);
            Assert.Equal(Color.FromRgb(0x0F, 0x6C, 0xBD), firstColumnBrush.Color);
            Assert.True(grid.AutoGenerateColumns);
        });
    }

    [Fact]
    public void DataGrid_CountsTrackNativeItemsAndColumns()
    {
        RunInSta(() =>
        {
            var rows = new ObservableCollection<MemberRow>
            {
                new("Variant", "Selects the surface treatment."),
            };
            var grid = new FlourishDataGrid
            {
                AutoGenerateColumns = false,
                ItemsSource = rows,
            };

            grid.Columns.Add(new DataGridTextColumn { Header = "Property", Binding = new System.Windows.Data.Binding(nameof(MemberRow.Name)) });
            grid.Columns.Add(new DataGridTextColumn { Header = "Function", Binding = new System.Windows.Data.Binding(nameof(MemberRow.Description)) });

            Assert.Equal(1, grid.RowCount);
            Assert.Equal(2, grid.ColumnCount);

            rows.Add(new MemberRow("Title", "Sets the heading."));
            grid.Columns.RemoveAt(1);

            Assert.Equal(2, grid.RowCount);
            Assert.Equal(1, grid.ColumnCount);
        });
    }

    [Fact]
    public void DataGrid_AutoGeneratesColumnsAndUsesRegularTypography()
    {
        RunInSta(() =>
        {
            var grid = new FlourishDataGrid
            {
                Width = 480,
                ItemsSource = new[] { new MemberRow("Variant", "Selects the surface treatment.") },
            };
            var window = CreateWindow(grid);

            try
            {
                window.Show();
                window.UpdateLayout();
                grid.ApplyTemplate();
                window.UpdateLayout();

                Assert.Equal(2, grid.ColumnCount);
                Assert.Equal(FontWeights.Regular, grid.FontWeight);

                var header = FindVisualDescendant<DataGridColumnHeader>(grid);
                var cells = FindVisualDescendants<DataGridCell>(grid)
                    .OrderBy(cell => cell.Column.DisplayIndex)
                    .ToArray();
                Assert.NotNull(header);
                Assert.Equal(2, cells.Length);
                Assert.Equal(FontWeights.Regular, header.FontWeight);
                Assert.All(cells, cell => Assert.Equal(FontWeights.Regular, cell.FontWeight));
                Assert.Equal(grid.FirstColumnForeground.ToString(), cells[0].Foreground.ToString());
            }
            finally
            {
                window.Close();
            }
        });
    }

    [Fact]
    public void DataGrid_WheelBubblesToThePageWhenItsRowsDoNotNeedScrolling()
    {
        RunInSta(() =>
        {
            var grid = CreateGrid(3);
            var pageContent = new StackPanel();
            pageContent.Children.Add(grid);
            pageContent.Children.Add(new Border { Height = 480 });
            var pageScrollViewer = new FlourishScrollViewer
            {
                Width = 420,
                Height = 180,
                Content = pageContent,
                IsSmoothScrollingEnabled = false,
            };
            var window = CreateWindow(pageScrollViewer);

            try
            {
                window.Show();
                window.UpdateLayout();
                grid.ApplyTemplate();
                pageScrollViewer.ApplyTemplate();
                window.UpdateLayout();

                var gridScrollViewer = Assert.IsType<FlourishScrollViewer>(
                    grid.Template.FindName("DG_ScrollViewer", grid)
                );
                var cell = Assert.IsType<DataGridCell>(FindVisualDescendant<DataGridCell>(grid));

                Assert.Equal(0, gridScrollViewer.ScrollableHeight);
                Assert.True(pageScrollViewer.ScrollableHeight > 0);
                Assert.Equal(0, pageScrollViewer.VerticalOffset);

                var wheel = RaiseMouseWheel(cell, -Mouse.MouseWheelDeltaForOneLine);
                PumpDispatcher(window);

                Assert.True(wheel.Handled);
                Assert.True(pageScrollViewer.VerticalOffset > 0);
            }
            finally
            {
                window.Close();
            }
        });
    }

    [Fact]
    public void DataGrid_WheelScrollsRowsFirstAndThenBubblesAtTheBoundary()
    {
        RunInSta(() =>
        {
            var grid = CreateGrid(40);
            grid.Height = 140;
            var pageContent = new StackPanel();
            pageContent.Children.Add(grid);
            pageContent.Children.Add(new Border { Height = 480 });
            var pageScrollViewer = new FlourishScrollViewer
            {
                Width = 420,
                Height = 220,
                Content = pageContent,
                IsSmoothScrollingEnabled = false,
            };
            var window = CreateWindow(pageScrollViewer);

            try
            {
                window.Show();
                window.UpdateLayout();
                grid.ApplyTemplate();
                pageScrollViewer.ApplyTemplate();
                window.UpdateLayout();

                var gridScrollViewer = Assert.IsType<FlourishScrollViewer>(
                    grid.Template.FindName("DG_ScrollViewer", grid)
                );
                var cell = Assert.IsType<DataGridCell>(FindVisualDescendant<DataGridCell>(grid));

                Assert.True(gridScrollViewer.ScrollableHeight > 0);
                Assert.True(pageScrollViewer.ScrollableHeight > 0);

                var innerWheel = RaiseMouseWheel(cell, -Mouse.MouseWheelDeltaForOneLine);
                PumpDispatcher(window);

                Assert.True(innerWheel.Handled);
                Assert.True(gridScrollViewer.VerticalOffset > 0);
                Assert.Equal(0, pageScrollViewer.VerticalOffset);

                gridScrollViewer.ScrollToEnd();
                window.UpdateLayout();
                cell = Assert.IsType<DataGridCell>(FindVisualDescendant<DataGridCell>(grid));
                Assert.Equal(
                    gridScrollViewer.ScrollableHeight,
                    gridScrollViewer.VerticalOffset,
                    precision: 3
                );

                var boundaryWheel = RaiseMouseWheel(
                    cell,
                    -Mouse.MouseWheelDeltaForOneLine
                );
                PumpDispatcher(window);

                Assert.True(boundaryWheel.Handled);
                Assert.Equal(
                    gridScrollViewer.ScrollableHeight,
                    gridScrollViewer.VerticalOffset,
                    precision: 3
                );
                Assert.True(pageScrollViewer.VerticalOffset > 0);
            }
            finally
            {
                window.Close();
            }
        });
    }

    private static FlourishDataGrid CreateGrid(int rowCount)
    {
        var grid = new FlourishDataGrid
        {
            AutoGenerateColumns = false,
            CanUserAddRows = false,
            IsReadOnly = true,
            ItemsSource = Enumerable.Range(1, rowCount)
                .Select(index => new MemberRow($"Member {index}", $"Description {index}")),
        };
        grid.Columns.Add(
            new DataGridTextColumn
            {
                Header = "Property",
                Binding = new System.Windows.Data.Binding(nameof(MemberRow.Name)),
            }
        );
        grid.Columns.Add(
            new DataGridTextColumn
            {
                Header = "Function",
                Binding = new System.Windows.Data.Binding(nameof(MemberRow.Description)),
            }
        );
        return grid;
    }

    private static MouseWheelEventArgs RaiseMouseWheel(UIElement source, int delta)
    {
        var wheel = new MouseWheelEventArgs(
            Mouse.PrimaryDevice,
            Environment.TickCount,
            delta
        )
        {
            RoutedEvent = Mouse.MouseWheelEvent,
            Source = source,
        };
        source.RaiseEvent(wheel);
        return wheel;
    }

    private static void PumpDispatcher(Window window)
    {
        window.Dispatcher.Invoke(
            System.Windows.Threading.DispatcherPriority.ApplicationIdle,
            static () => { }
        );
        window.UpdateLayout();
    }

    private static Window CreateWindow(UIElement content)
    {
        var window = new Window
        {
            Width = 640,
            Height = 480,
            Left = -10000,
            Top = -10000,
            ShowActivated = false,
            ShowInTaskbar = false,
            Content = content,
        };
        window.Resources.MergedDictionaries.Add(
            Assert.IsType<ResourceDictionary>(Application.LoadComponent(new Uri(GenericThemeSource, UriKind.Relative)))
        );
        return window;
    }

    private static T? FindVisualDescendant<T>(DependencyObject root)
        where T : DependencyObject
    {
        for (var index = 0; index < VisualTreeHelper.GetChildrenCount(root); index++)
        {
            var child = VisualTreeHelper.GetChild(root, index);
            if (child is T match)
            {
                return match;
            }

            var descendant = FindVisualDescendant<T>(child);
            if (descendant is not null)
            {
                return descendant;
            }
        }

        return null;
    }

    private static IEnumerable<T> FindVisualDescendants<T>(DependencyObject root)
        where T : DependencyObject
    {
        for (var index = 0; index < VisualTreeHelper.GetChildrenCount(root); index++)
        {
            var child = VisualTreeHelper.GetChild(root, index);
            if (child is T match)
            {
                yield return match;
            }

            foreach (var descendant in FindVisualDescendants<T>(child))
            {
                yield return descendant;
            }
        }
    }

    private static void RunInSta(Action action)
    {
        Exception? error = null;
        var thread = new Thread(() =>
        {
            try
            {
                action();
            }
            catch (Exception exception)
            {
                error = exception;
            }
        });
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();

        if (error is not null)
        {
            System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(error).Throw();
        }
    }

    private sealed record MemberRow(string Name, string Description);
}
