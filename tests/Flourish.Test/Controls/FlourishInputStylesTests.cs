using System.Diagnostics;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using ArkheideSystem.Flourish.Controls;
using CustomScrollViewer = ArkheideSystem.Flourish.Controls.ScrollViewer;

namespace ArkheideSystem.Flourish.Test.Controls;

public sealed class FlourishInputStylesTests
{
    private const string GenericThemeSource =
        "/Flourish;component/Themes/Generic.xaml";

    [Fact]
    public void ContentBodyMargin_UsesOneSharedThirtyTwoPixelGutter()
    {
        RunInSta(() =>
        {
            var resources = LoadResourceDictionary(
                "/Flourish;component/Themes/Layout.xaml"
            );

            Assert.Equal(
                new Thickness(32, 0, 32, 0),
                resources["FlourishContentBodyMargin"]
            );
        });
    }

    [Fact]
    public void ComboBoxItem_DefaultStyleUsesStableAlignmentWithoutAncestorBindings()
    {
        RunInSta(() =>
        {
            var item = new FlourishComboBoxItem { Content = "Choice" };
            var comboBox = new FlourishComboBox { Items = { item } };
            var window = CreateWindow(comboBox);

            try
            {
                window.Show();
                comboBox.IsDropDownOpen = true;
                window.UpdateLayout();

                Assert.Equal(HorizontalAlignment.Left, item.HorizontalContentAlignment);
                Assert.Equal(VerticalAlignment.Center, item.VerticalContentAlignment);
                Assert.False(
                    BindingOperations.IsDataBound(
                        item,
                        Control.HorizontalContentAlignmentProperty
                    )
                );
                Assert.False(
                    BindingOperations.IsDataBound(
                        item,
                        Control.VerticalContentAlignmentProperty
                    )
                );
            }
            finally
            {
                comboBox.IsDropDownOpen = false;
                window.Close();
            }
        });
    }

    [Fact]
    public void ScrollViewer_DefaultStyleStretchesPageContentAcrossTheViewport()
    {
        RunInSta(() =>
        {
            var content = new Border();
            var scrollViewer = new CustomScrollViewer
            {
                Width = 320,
                Height = 120,
                Content = content,
            };
            var window = CreateWindow(scrollViewer);

            try
            {
                window.Show();
                window.UpdateLayout();

                Assert.Equal(
                    HorizontalAlignment.Stretch,
                    scrollViewer.HorizontalContentAlignment
                );
                Assert.Equal(
                    VerticalAlignment.Stretch,
                    scrollViewer.VerticalContentAlignment
                );
                Assert.InRange(content.ActualWidth, 319, 320);
            }
            finally
            {
                window.Close();
            }
        });
    }

    [Fact]
    public void ScrollViewer_DefaultStyleUsesRenderTransformAndSlenderRoundedThumb()
    {
        RunInSta(() =>
        {
            var scrollViewer = new CustomScrollViewer
            {
                Width = 320,
                Height = 120,
                Content = new Border { Height = 600 },
            };
            var window = CreateWindow(scrollViewer);

            try
            {
                window.Show();
                window.UpdateLayout();

                var presenter = Assert.IsType<ScrollContentPresenter>(
                    scrollViewer.Template.FindName(
                        "PART_ScrollContentPresenter",
                        scrollViewer
                    )
                );
                var scrollBar = Assert.IsType<FlourishScrollBar>(
                    scrollViewer.Template.FindName("PART_VerticalScrollBar", scrollViewer)
                );
                scrollBar.ApplyTemplate();
                var track = Assert.IsType<Track>(
                    scrollBar.Template.FindName("PART_Track", scrollBar)
                );
                var thumbChrome = Assert.IsType<Border>(
                    track.Thumb.Template.FindName("ThumbChrome", track.Thumb)
                );

                var transform = Assert.IsType<TranslateTransform>(
                    presenter.RenderTransform
                );
                Assert.False(transform.IsFrozen);
                Assert.Equal(7, scrollBar.ActualWidth);
                Assert.Equal(new Thickness(2), thumbChrome.Margin);
                Assert.True(thumbChrome.CornerRadius.TopLeft > 0);
            }
            finally
            {
                window.Close();
            }
        });
    }

    [Fact]
    public void ScrollViewer_MouseWheelUsesMutableTransformAndUnloadsSafely()
    {
        RunInSta(() =>
        {
            var scrollViewer = new CustomScrollViewer
            {
                Width = 320,
                Height = 120,
                Content = new Border { Height = 600 },
            };
            var window = CreateWindow(scrollViewer);

            try
            {
                window.Show();
                window.UpdateLayout();

                var presenter = Assert.IsType<ScrollContentPresenter>(
                    scrollViewer.Template.FindName(
                        "PART_ScrollContentPresenter",
                        scrollViewer
                    )
                );
                var transform = Assert.IsType<TranslateTransform>(
                    presenter.RenderTransform
                );
                var wheel = new MouseWheelEventArgs(
                    Mouse.PrimaryDevice,
                    Environment.TickCount,
                    -Mouse.MouseWheelDeltaForOneLine
                )
                {
                    RoutedEvent = Mouse.PreviewMouseWheelEvent,
                    Source = scrollViewer,
                };

                scrollViewer.RaiseEvent(wheel);

                Assert.True(wheel.Handled);
                PumpDispatcherUntil(
                    () =>
                        Math.Abs(transform.Y) > 0.001
                        || scrollViewer.VerticalOffset > 0,
                    TimeSpan.FromSeconds(1)
                );
                Assert.False(transform.IsFrozen);

                window.Content = null;
                PumpDispatcher();

                Assert.Equal(0, transform.Y);

                var unloadedWheel = new MouseWheelEventArgs(
                    Mouse.PrimaryDevice,
                    Environment.TickCount,
                    -Mouse.MouseWheelDeltaForOneLine
                )
                {
                    RoutedEvent = Mouse.PreviewMouseWheelEvent,
                    Source = scrollViewer,
                };
                scrollViewer.RaiseEvent(unloadedWheel);

                Assert.False(unloadedWheel.Handled);
            }
            finally
            {
                window.Close();
            }
        });
    }

    [Fact]
    public void ScrollViewer_TemplateCreatesIndependentMutableTransforms()
    {
        RunInSta(() =>
        {
            var first = new CustomScrollViewer
            {
                Height = 80,
                Content = new Border { Height = 240 },
            };
            var second = new CustomScrollViewer
            {
                Height = 80,
                Content = new Border { Height = 240 },
            };
            var panel = new StackPanel { Children = { first, second } };
            var window = CreateWindow(panel);

            try
            {
                window.Show();
                window.UpdateLayout();

                var firstPresenter = Assert.IsType<ScrollContentPresenter>(
                    first.Template.FindName("PART_ScrollContentPresenter", first)
                );
                var secondPresenter = Assert.IsType<ScrollContentPresenter>(
                    second.Template.FindName("PART_ScrollContentPresenter", second)
                );
                var firstTransform = Assert.IsType<TranslateTransform>(
                    firstPresenter.RenderTransform
                );
                var secondTransform = Assert.IsType<TranslateTransform>(
                    secondPresenter.RenderTransform
                );

                Assert.NotSame(firstTransform, secondTransform);
                Assert.False(firstTransform.IsFrozen);
                Assert.False(secondTransform.IsFrozen);
            }
            finally
            {
                window.Close();
            }
        });
    }

    [Fact]
    public void ScrollViewer_MouseWheelAnimationCompletesAndStopsRendering()
    {
        RunInSta(() =>
        {
            var scrollViewer = new CustomScrollViewer
            {
                Width = 320,
                Height = 120,
                Content = new Border { Height = 600 },
            };
            var window = CreateWindow(scrollViewer);

            try
            {
                window.Show();
                window.UpdateLayout();

                var presenter = Assert.IsType<ScrollContentPresenter>(
                    scrollViewer.Template.FindName(
                        "PART_ScrollContentPresenter",
                        scrollViewer
                    )
                );
                var transform = Assert.IsType<TranslateTransform>(
                    presenter.RenderTransform
                );
                var wheel = new MouseWheelEventArgs(
                    Mouse.PrimaryDevice,
                    Environment.TickCount,
                    -Mouse.MouseWheelDeltaForOneLine
                )
                {
                    RoutedEvent = Mouse.PreviewMouseWheelEvent,
                    Source = scrollViewer,
                };

                scrollViewer.RaiseEvent(wheel);

                Assert.True(wheel.Handled);
                PumpDispatcherUntil(
                    () =>
                        scrollViewer.VerticalOffset > 0
                        && !GetIsRendering(scrollViewer),
                    TimeSpan.FromSeconds(2)
                );

                Assert.Equal(0, transform.Y);
            }
            finally
            {
                window.Close();
            }
        });
    }

    private static Window CreateWindow(UIElement content)
    {
        var window = new Window
        {
            Width = 320,
            Height = 240,
            Left = -10000,
            Top = -10000,
            ShowActivated = false,
            ShowInTaskbar = false,
            Content = content,
        };
        window.Resources.MergedDictionaries.Add(
            LoadResourceDictionary(GenericThemeSource)
        );
        return window;
    }

    private static ResourceDictionary LoadResourceDictionary(string source)
    {
        return Assert.IsType<ResourceDictionary>(
            Application.LoadComponent(new Uri(source, UriKind.Relative))
        );
    }

    private static void PumpDispatcher()
    {
        var frame = new DispatcherFrame();
        Dispatcher.CurrentDispatcher.BeginInvoke(
            DispatcherPriority.Background,
            new Action(() => frame.Continue = false)
        );
        Dispatcher.PushFrame(frame);
    }

    private static void PumpDispatcherUntil(Func<bool> condition, TimeSpan timeout)
    {
        if (condition())
        {
            return;
        }

        var stopwatch = Stopwatch.StartNew();
        var frame = new DispatcherFrame();
        var timer = new DispatcherTimer(DispatcherPriority.Background)
        {
            Interval = TimeSpan.FromMilliseconds(10),
        };
        timer.Tick += (_, _) =>
        {
            if (!condition() && stopwatch.Elapsed < timeout)
            {
                return;
            }

            timer.Stop();
            frame.Continue = false;
        };
        timer.Start();
        Dispatcher.PushFrame(frame);

        Assert.True(condition(), $"Condition was not met within {timeout}.");
    }

    private static bool GetIsRendering(CustomScrollViewer scrollViewer)
    {
        var field = typeof(CustomScrollViewer).GetField(
            "_isRendering",
            BindingFlags.Instance | BindingFlags.NonPublic
        );
        return Assert.IsType<bool>(field?.GetValue(scrollViewer));
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
}
