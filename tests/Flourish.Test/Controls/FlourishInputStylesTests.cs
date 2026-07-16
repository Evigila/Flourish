using System.Diagnostics;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Markup;
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
            var content = new Border { Height = 600 };
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

                var transform = GetSmoothTransform(scrollViewer);
                Assert.Equal(Matrix.Identity, presenter.RenderTransform.Value);
                Assert.Equal(Matrix.Identity, content.RenderTransform.Value);
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
            var content = new Border { Height = 600 };
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

                var presenter = Assert.IsType<ScrollContentPresenter>(
                    scrollViewer.Template.FindName(
                        "PART_ScrollContentPresenter",
                        scrollViewer
                    )
                );
                var transform = GetSmoothTransform(scrollViewer);
                Assert.Equal(Matrix.Identity, presenter.RenderTransform.Value);
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
            var firstContent = new Border { Height = 240 };
            var secondContent = new Border { Height = 240 };
            var first = new CustomScrollViewer
            {
                Height = 80,
                Content = firstContent,
            };
            var second = new CustomScrollViewer
            {
                Height = 80,
                Content = secondContent,
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
                var firstTransform = GetSmoothTransform(first);
                var secondTransform = GetSmoothTransform(second);

                Assert.NotSame(firstTransform, secondTransform);
                Assert.False(firstTransform.IsFrozen);
                Assert.False(secondTransform.IsFrozen);
                Assert.Equal(Matrix.Identity, firstPresenter.RenderTransform.Value);
                Assert.Equal(Matrix.Identity, secondPresenter.RenderTransform.Value);
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
            var content = new Border { Height = 600 };
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

                var presenter = Assert.IsType<ScrollContentPresenter>(
                    scrollViewer.Template.FindName(
                        "PART_ScrollContentPresenter",
                        scrollViewer
                    )
                );
                var transform = GetSmoothTransform(scrollViewer);
                Assert.Equal(Matrix.Identity, presenter.RenderTransform.Value);
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

    [Fact]
    public void ScrollViewer_SmoothTransformPreservesAndRestoresContentTransform()
    {
        RunInSta(() =>
        {
            var originalTransform = new ScaleTransform(1.05, 0.95);
            var content = new Border
            {
                Height = 600,
                RenderTransform = originalTransform,
            };
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

                var smoothTransform = GetSmoothTransform(scrollViewer);
                Assert.Same(originalTransform, content.RenderTransform);
                Assert.NotSame(originalTransform, smoothTransform);

                window.Content = null;
                PumpDispatcher();

                Assert.Same(originalTransform, content.RenderTransform);
            }
            finally
            {
                window.Close();
            }
        });
    }

    [Fact]
    public void ScrollViewer_SwitchingToLogicalScrollingStopsPhysicalAnimation()
    {
        RunInSta(() =>
        {
            var content = new Border { Height = 600 };
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

                var transform = GetSmoothTransform(scrollViewer);
                Assert.True(
                    RaisePreviewMouseWheel(
                        scrollViewer,
                        -Mouse.MouseWheelDeltaForOneLine
                    ).Handled
                );
                PumpDispatcherUntil(
                    () => Math.Abs(transform.Y) > 0.001,
                    TimeSpan.FromSeconds(1)
                );

                scrollViewer.CanContentScroll = true;
                window.UpdateLayout();
                PumpDispatcher();

                Assert.False(GetIsRendering(scrollViewer));
                Assert.Equal(0, transform.Y);
                Assert.Null(
                    scrollViewer.Template.FindName(
                        "PART_SmoothScrollContentHost",
                        scrollViewer
                    )
                );
                var presenter = Assert.IsType<ScrollContentPresenter>(
                    scrollViewer.Template.FindName(
                        "PART_ScrollContentPresenter",
                        scrollViewer
                    )
                );
                Assert.Equal(Matrix.Identity, presenter.RenderTransform.Value);
                Assert.Same(content, presenter.Content);

                scrollViewer.CanContentScroll = false;
                window.UpdateLayout();
                PumpDispatcher();

                var replacementHost = Assert.IsType<ContentPresenter>(
                    scrollViewer.Template.FindName(
                        "PART_SmoothScrollContentHost",
                        scrollViewer
                    )
                );
                Assert.Same(content, replacementHost.Content);
                Assert.Equal(
                    0,
                    Assert.IsType<TranslateTransform>(
                        replacementHost.RenderTransform
                    ).Y
                );
            }
            finally
            {
                window.Close();
            }
        });
    }

    [Fact]
    public void ScrollViewer_PhysicalTemplateSupportsDataTemplatedContent()
    {
        RunInSta(() =>
        {
            var contentTemplate = Assert.IsType<DataTemplate>(
                XamlReader.Parse(
                    """
                    <DataTemplate xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation">
                      <Border Height="600" Background="Red" />
                    </DataTemplate>
                    """
                )
            );
            var scrollViewer = new CustomScrollViewer
            {
                Width = 320,
                Height = 120,
                Content = "templated content",
                ContentTemplate = contentTemplate,
            };
            var window = CreateWindow(scrollViewer);

            try
            {
                window.Show();
                window.UpdateLayout();

                var host = Assert.IsType<ContentPresenter>(
                    scrollViewer.Template.FindName(
                        "PART_SmoothScrollContentHost",
                        scrollViewer
                    )
                );
                Assert.Equal("templated content", host.Content);
                Assert.Same(contentTemplate, host.ContentTemplate);
                Assert.InRange(scrollViewer.ScrollableHeight, 479, 481);
            }
            finally
            {
                window.Close();
            }
        });
    }

    [Fact]
    public void ScrollViewer_SmoothScrollingKeepsBothViewportEdgesCovered()
    {
        RunInSta(() =>
        {
            var content = new Border
            {
                Height = 600,
                Background = Brushes.Red,
            };
            var scrollViewer = new CustomScrollViewer
            {
                Width = 320,
                Height = 120,
                Background = Brushes.Blue,
                Content = content,
            };
            var window = CreateWindow(scrollViewer);

            try
            {
                window.Show();
                window.UpdateLayout();
                scrollViewer.ScrollToVerticalOffset(240);
                window.UpdateLayout();
                PumpDispatcher();

                var transform = GetSmoothTransform(scrollViewer);
                Assert.True(
                    RaisePreviewMouseWheel(
                        scrollViewer,
                        Mouse.MouseWheelDeltaForOneLine
                    ).Handled
                );
                PumpDispatcherUntil(
                    () => transform.Y > 2,
                    TimeSpan.FromSeconds(1)
                );

                var viewportCenterX =
                    (int)Math.Floor(scrollViewer.ActualWidth / 2d);
                Assert.True(
                    IsVisualDescendantOrSelf(
                        content,
                        scrollViewer.InputHitTest(new Point(viewportCenterX, 1))
                    )
                );

                PumpDispatcherUntil(
                    () => !GetIsRendering(scrollViewer),
                    TimeSpan.FromSeconds(2)
                );
                transform.Y = -12;

                Assert.True(
                    IsVisualDescendantOrSelf(
                        content,
                        scrollViewer.InputHitTest(
                            new Point(
                                viewportCenterX,
                                Math.Floor(scrollViewer.ActualHeight) - 2
                            )
                        )
                    )
                );
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

    private static TranslateTransform GetSmoothTransform(
        CustomScrollViewer scrollViewer
    )
    {
        var host = Assert.IsType<ContentPresenter>(
            scrollViewer.Template.FindName(
                "PART_SmoothScrollContentHost",
                scrollViewer
            )
        );
        return Assert.IsType<TranslateTransform>(host.RenderTransform);
    }

    private static MouseWheelEventArgs RaisePreviewMouseWheel(
        UIElement source,
        int delta
    )
    {
        var wheel = new MouseWheelEventArgs(
            Mouse.PrimaryDevice,
            Environment.TickCount,
            delta
        )
        {
            RoutedEvent = Mouse.PreviewMouseWheelEvent,
            Source = source,
        };
        source.RaiseEvent(wheel);
        return wheel;
    }

    private static bool IsVisualDescendantOrSelf(
        DependencyObject ancestor,
        IInputElement? inputElement
    )
    {
        var current = inputElement as DependencyObject;
        while (current is not null)
        {
            if (ReferenceEquals(current, ancestor))
            {
                return true;
            }

            current = VisualTreeHelper.GetParent(current);
        }

        return false;
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
