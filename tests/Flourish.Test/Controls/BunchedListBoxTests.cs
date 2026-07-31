using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using ArkheideSystem.Flourish.Controls;
using FlourishScrollViewer = ArkheideSystem.Flourish.Controls.ScrollViewer;

namespace ArkheideSystem.Flourish.Test.Controls;

public sealed class BunchedListBoxTests
{
    private const string GenericThemeSource = "/Flourish;component/Themes/Generic.xaml";

    [Fact]
    public void GeneratedAndExplicitItems_UseOnlyBunchedContainers()
    {
        StaTest.Run(() =>
        {
            var explicitContainer = new BunchedListBoxItem { Content = "Explicit" };
            var listBox = new ContainerProbeBunchedListBox();
            listBox.Resources.MergedDictionaries.Add(LoadGenericTheme());
            listBox.Items.Add("Generated");
            listBox.Items.Add(explicitContainer);
            using var fixture = BunchedListBoxFixture.Show(listBox);

            Assert.IsType<BunchedListBoxItem>(listBox.ItemContainerGenerator.ContainerFromIndex(0));
            Assert.Same(explicitContainer, listBox.ItemContainerGenerator.ContainerFromIndex(1));
            Assert.True(listBox.IsOwnContainer(explicitContainer));
            Assert.False(listBox.IsOwnContainer(new FlourishListBoxItem()));
        });
    }

    [Fact]
    public void NavigationPresentation_IsInheritedByGeneratedBunchedContainers()
    {
        StaTest.Run(() =>
        {
            var state = new NavigationItemState();
            var listBox = new BunchedListBox
            {
                Appearance = FlourishListBoxAppearance.Navigation,
                Items = { state },
            };
            using var fixture = BunchedListBoxFixture.Show(listBox);
            var container = Assert.IsType<BunchedListBoxItem>(
                listBox.ItemContainerGenerator.ContainerFromIndex(0)
            );

            Assert.False(container.IsEnabled);
            Assert.False(container.IsItemVisible);
            Assert.True(container.IsGroupHeader);
            Assert.True(container.IsCommandItem);
            Assert.Equal(state.Label, container.ToolTip);
        });
    }

    [Fact]
    public void Template_OwnsOneNonHitTestInteractionLayerBehindTheItemsViewport()
    {
        StaTest.Run(() =>
        {
            var listBox = new BunchedListBox { Items = { "First", "Second" } };
            using var fixture = BunchedListBoxFixture.Show(listBox);
            var viewport = fixture.Part<FrameworkElement>(
                BunchedListBox.InteractionViewportPartName
            );
            var layer = fixture.Part<Canvas>(BunchedListBox.IndicatorLayerPartName);
            var selection = fixture.Part<Border>(BunchedListBox.SelectionChromePartName);
            var hover = fixture.Part<Border>(BunchedListBox.HoverChromePartName);
            var pressed = fixture.Part<Border>(BunchedListBox.PressedChromePartName);
            var scrollViewer = fixture.Part<FlourishScrollViewer>(
                BunchedListBox.ScrollViewerPartName
            );

            Assert.False(layer.IsHitTestVisible);
            Assert.True(viewport.ClipToBounds);
            Assert.True(layer.ClipToBounds);
            Assert.Same(
                VisualTreeHelper.GetParent(layer),
                VisualTreeHelper.GetParent(scrollViewer)
            );
            Assert.True(Panel.GetZIndex(layer) < Panel.GetZIndex(scrollViewer));
            Assert.Equal(0, selection.Opacity);
            Assert.Equal(0, hover.Opacity);
            Assert.Equal(0, pressed.Opacity);
            Assert.False(selection.IsHitTestVisible);
            Assert.False(hover.IsHitTestVisible);
            Assert.False(pressed.IsHitTestVisible);
            Assert.NotNull(FindVisualDescendant<ItemsPresenter>(scrollViewer));
            Assert.True(VirtualizingPanel.GetIsVirtualizing(listBox));
            Assert.Equal(
                VirtualizationMode.Recycling,
                VirtualizingPanel.GetVirtualizationMode(listBox)
            );

            var item = Assert.IsType<BunchedListBoxItem>(
                listBox.ItemContainerGenerator.ContainerFromIndex(0)
            );
            item.ApplyTemplate();
            Assert.False(HoverReveal.GetIsParticipant(item));
            Assert.Null(item.Template.FindName("HoverChrome", item));
            Assert.Null(item.Template.FindName("HoverRevealScale", item));
        });
    }

    [Fact]
    public void PointerTarget_MovesOneHoverSurfaceBetweenContainersWithoutHidingIt()
    {
        StaTest.Run(() =>
        {
            var listBox = new BunchedListBox
            {
                Width = 280,
                Items =
                {
                    new BunchedListBoxItem { Content = "First", Height = 32 },
                    new BunchedListBoxItem { Content = "Second", Height = 44 },
                },
            };
            HoverReveal.SetAnimationDuration(listBox, TimeSpan.Zero);
            using var fixture = BunchedListBoxFixture.Show(listBox);
            var first = Assert.IsType<BunchedListBoxItem>(
                listBox.ItemContainerGenerator.ContainerFromIndex(0)
            );
            var second = Assert.IsType<BunchedListBoxItem>(
                listBox.ItemContainerGenerator.ContainerFromIndex(1)
            );
            var hover = fixture.Part<Border>(BunchedListBox.HoverChromePartName);
            var layer = fixture.Part<Canvas>(BunchedListBox.IndicatorLayerPartName);

            RaiseMouseEvent(first, Mouse.PreviewMouseMoveEvent);

            Assert.Same(first, listBox.InteractionController.HoverTarget);
            Assert.Equal(1, hover.Opacity);
            AssertRectClose(
                GetBounds(first, layer),
                listBox.InteractionController.CurrentHoverBounds
            );

            RaiseMouseEvent(second, Mouse.PreviewMouseMoveEvent);

            Assert.Same(second, listBox.InteractionController.HoverTarget);
            Assert.Same(hover, fixture.Part<Border>(BunchedListBox.HoverChromePartName));
            Assert.Equal(1, hover.Opacity);
            AssertRectClose(
                GetBounds(second, layer),
                listBox.InteractionController.CurrentHoverBounds
            );
        });
    }

    [Fact]
    public void PointerPress_UsesThePressedSurfaceAndReleaseRestoresHover()
    {
        StaTest.Run(() =>
        {
            var listBox = new BunchedListBox { Items = { "Item" } };
            HoverReveal.SetAnimationDuration(listBox, TimeSpan.Zero);
            using var fixture = BunchedListBoxFixture.Show(listBox);
            var item = Assert.IsType<BunchedListBoxItem>(
                listBox.ItemContainerGenerator.ContainerFromIndex(0)
            );
            var hover = fixture.Part<Border>(BunchedListBox.HoverChromePartName);
            var pressed = fixture.Part<Border>(BunchedListBox.PressedChromePartName);

            RaiseMouseEvent(item, Mouse.PreviewMouseMoveEvent);
            Assert.Equal(1, hover.Opacity);
            Assert.Equal(0, pressed.Opacity);

            RaiseMouseButtonEvent(item, Mouse.PreviewMouseDownEvent);
            Assert.Equal(0, hover.Opacity);
            Assert.Equal(1, pressed.Opacity);

            RaiseMouseButtonEvent(item, Mouse.PreviewMouseUpEvent);
            Assert.Equal(1, hover.Opacity);
            Assert.Equal(0, pressed.Opacity);
            Assert.Same(
                fixture.Window.FindResource("FlourishPressedRevealBrush"),
                pressed.Background
            );
        });
    }

    [Fact]
    public void SelectionModes_DrawSelectionOnlyInTheParentIndicatorLayer()
    {
        StaTest.Run(() =>
        {
            var listBox = new BunchedListBox { ItemsSource = new[] { "First", "Second", "Third" } };
            HoverReveal.SetAnimationDuration(listBox, TimeSpan.Zero);
            using var fixture = BunchedListBoxFixture.Show(listBox);
            var selection = fixture.Part<Border>(BunchedListBox.SelectionChromePartName);

            listBox.SelectedIndex = 0;
            FlushDispatcher();

            Assert.Equal(1, listBox.InteractionController.SelectionIndicatorCount);
            Assert.Equal(1, selection.Opacity);
            var first = Assert.IsType<BunchedListBoxItem>(
                listBox.ItemContainerGenerator.ContainerFromIndex(0)
            );
            Assert.True(
                first.Background is null || first.Background is SolidColorBrush { Color.A: 0 }
            );

            listBox.SelectionMode = SelectionMode.Multiple;
            listBox.SelectedItems.Clear();
            listBox.SelectedItems.Add(listBox.Items[0]);
            listBox.SelectedItems.Add(listBox.Items[2]);
            FlushDispatcher();

            Assert.Equal(0, selection.Opacity);
            Assert.Equal(2, listBox.InteractionController.SelectionIndicatorCount);
        });
    }

    [Fact]
    public void Virtualization_TracksOnlyRealizedContainersAndRecoversAtTheNewViewport()
    {
        StaTest.Run(() =>
        {
            var items = Enumerable.Range(0, 200).Select(index => $"Item {index}").ToArray();
            var listBox = new BunchedListBox
            {
                Width = 260,
                Height = 90,
                ItemsSource = items,
            };
            VirtualizingPanel.SetCacheLength(listBox, new VirtualizationCacheLength(0));
            HoverReveal.SetAnimationDuration(listBox, TimeSpan.Zero);
            using var fixture = BunchedListBoxFixture.Show(listBox, height: 150);

            listBox.SelectedIndex = 0;
            FlushDispatcher();
            var initiallyRealized = GetRealizedContainers(listBox);
            Assert.NotEmpty(initiallyRealized);
            Assert.True(initiallyRealized.Count < items.Length);
            Assert.Equal(1, listBox.InteractionController.SelectionIndicatorCount);

            listBox.ScrollIntoView(items[^1]);
            FlushDispatcher();
            fixture.Window.UpdateLayout();
            FlushDispatcher();

            Assert.Null(listBox.ItemContainerGenerator.ContainerFromIndex(0));
            Assert.NotNull(listBox.ItemContainerGenerator.ContainerFromIndex(items.Length - 1));
            Assert.Equal(0, listBox.InteractionController.SelectionIndicatorCount);

            listBox.SelectedIndex = items.Length - 1;
            FlushDispatcher();

            Assert.Equal(1, listBox.InteractionController.SelectionIndicatorCount);
            Assert.True(GetRealizedContainers(listBox).Count < items.Length);
        });
    }

    [Fact]
    public void Unload_ClearsTargetsSelectionsAndDetachesTheController()
    {
        StaTest.Run(() =>
        {
            var listBox = new BunchedListBox { Items = { "First", "Second" } };
            HoverReveal.SetAnimationDuration(listBox, TimeSpan.Zero);
            var fixture = BunchedListBoxFixture.Show(listBox);
            var first = Assert.IsType<BunchedListBoxItem>(
                listBox.ItemContainerGenerator.ContainerFromIndex(0)
            );
            RaiseMouseEvent(first, Mouse.PreviewMouseMoveEvent);
            listBox.SelectedIndex = 0;
            FlushDispatcher();
            RaiseMouseEvent(first, Mouse.PreviewMouseMoveEvent);

            Assert.True(listBox.InteractionController.IsAttached);
            Assert.NotNull(listBox.InteractionController.HoverTarget);
            Assert.Equal(1, listBox.InteractionController.SelectionIndicatorCount);

            fixture.Dispose();

            Assert.False(listBox.InteractionController.IsAttached);
            Assert.Null(listBox.InteractionController.HoverTarget);
            Assert.Equal(0, listBox.InteractionController.SelectionIndicatorCount);
        });
    }

    private static IReadOnlyList<BunchedListBoxItem> GetRealizedContainers(BunchedListBox listBox)
    {
        return Enumerable
            .Range(0, listBox.Items.Count)
            .Select(index => listBox.ItemContainerGenerator.ContainerFromIndex(index))
            .OfType<BunchedListBoxItem>()
            .ToArray();
    }

    private static Rect GetBounds(FrameworkElement element, Visual relativeTo)
    {
        return element
            .TransformToVisual(relativeTo)
            .TransformBounds(new Rect(new Point(), element.RenderSize));
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

    private static void RaiseMouseEvent(UIElement element, RoutedEvent routedEvent)
    {
        element.RaiseEvent(
            new MouseEventArgs(Mouse.PrimaryDevice, Environment.TickCount)
            {
                RoutedEvent = routedEvent,
            }
        );
    }

    private static void RaiseMouseButtonEvent(UIElement element, RoutedEvent routedEvent)
    {
        element.RaiseEvent(
            new MouseButtonEventArgs(Mouse.PrimaryDevice, Environment.TickCount, MouseButton.Left)
            {
                RoutedEvent = routedEvent,
            }
        );
    }

    private static void FlushDispatcher()
    {
        DispatcherTest.DrainApplicationIdle();
    }

    private static void AssertRectClose(Rect expected, Rect actual)
    {
        Assert.InRange(actual.X, expected.X - 0.001, expected.X + 0.001);
        Assert.InRange(actual.Y, expected.Y - 0.001, expected.Y + 0.001);
        Assert.InRange(actual.Width, expected.Width - 0.001, expected.Width + 0.001);
        Assert.InRange(actual.Height, expected.Height - 0.001, expected.Height + 0.001);
    }

    private sealed class BunchedListBoxFixture : IDisposable
    {
        private bool isDisposed;

        private BunchedListBoxFixture(BunchedListBox listBox, Window window)
        {
            ListBox = listBox;
            Window = window;
        }

        internal BunchedListBox ListBox { get; }

        internal Window Window { get; }

        internal static BunchedListBoxFixture Show(BunchedListBox listBox, double height = 280)
        {
            var window = new Window
            {
                Width = 360,
                Height = height,
                Left = -10000,
                Top = -10000,
                ShowActivated = false,
                ShowInTaskbar = false,
                Content = listBox,
            };
            window.Resources.MergedDictionaries.Add(
                Assert.IsType<ResourceDictionary>(
                    Application.LoadComponent(new Uri(GenericThemeSource, UriKind.Relative))
                )
            );
            window.Show();
            window.UpdateLayout();
            FlushDispatcher();
            window.UpdateLayout();
            listBox.ApplyTemplate();
            return new BunchedListBoxFixture(listBox, window);
        }

        internal T Part<T>(string name)
            where T : class
        {
            return Assert.IsAssignableFrom<T>(ListBox.Template.FindName(name, ListBox));
        }

        public void Dispose()
        {
            if (isDisposed)
            {
                return;
            }
            isDisposed = true;
            Window.Close();
            FlushDispatcher();
        }
    }

    private sealed class ContainerProbeBunchedListBox : BunchedListBox
    {
        internal bool IsOwnContainer(object item)
        {
            return IsItemItsOwnContainerOverride(item);
        }
    }

    private sealed class NavigationItemState
    {
        public string Label => "Navigation heading";

        public bool IsEnabled => false;

        public bool IsVisible => false;

        public bool IsGroupHeader => true;

        public bool IsCommandItem => true;
    }

    private static ResourceDictionary LoadGenericTheme()
    {
        return Assert.IsType<ResourceDictionary>(
            Application.LoadComponent(new Uri(GenericThemeSource, UriKind.Relative))
        );
    }
}
