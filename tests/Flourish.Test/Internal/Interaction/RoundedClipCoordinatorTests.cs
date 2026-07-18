using System.Runtime.ExceptionServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using ArkheideSystem.Flourish.Internal.Interaction;

namespace ArkheideSystem.Flourish.Test.Internal.Interaction;

public sealed class RoundedClipCoordinatorTests
{
    [Fact]
    public void Create_UsesFrozenRectangleGeometryForUniformCorners()
    {
        RunInSta(() =>
        {
            var clip = Assert.IsType<RectangleGeometry>(
                RoundedClipGeometry.Create(new Size(100, 40), new CornerRadius(8))
            );

            Assert.True(clip.IsFrozen);
            Assert.Equal(new Rect(0, 0, 100, 40), clip.Rect);
            Assert.Equal(8, clip.RadiusX);
            Assert.Equal(8, clip.RadiusY);
        });
    }

    [Fact]
    public void Create_PreservesAsymmetricCornersAndScalesOversizedUniformRadius()
    {
        RunInSta(() =>
        {
            var asymmetric = Assert.IsType<StreamGeometry>(
                RoundedClipGeometry.Create(
                    new Size(100, 40),
                    new CornerRadius(2, 4, 6, 8)
                )
            );
            var constrained = Assert.IsType<RectangleGeometry>(
                RoundedClipGeometry.Create(new Size(20, 10), new CornerRadius(12))
            );

            Assert.True(asymmetric.IsFrozen);
            Assert.Equal(new Rect(0, 0, 100, 40), asymmetric.Bounds);
            Assert.False(asymmetric.FillContains(new Point(0, 0)));
            Assert.True(constrained.IsFrozen);
            Assert.Equal(5, constrained.RadiusX, precision: 6);
            Assert.Equal(5, constrained.RadiusY, precision: 6);
        });
    }

    [Fact]
    public void Coordinator_CoalescesSizeAndCornerChangesAndDetachesCleanly()
    {
        RunInSta(() =>
        {
            var clipHost = new Grid();
            var surface = new Border
            {
                Width = 100,
                Height = 50,
                CornerRadius = new CornerRadius(4),
                Child = clipHost,
            };
            var window = new Window
            {
                Width = 320,
                Height = 180,
                Left = -10000,
                Top = -10000,
                ShowActivated = false,
                ShowInTaskbar = false,
                Content = surface,
            };
            var coordinator = new RoundedClipCoordinator();

            try
            {
                window.Show();
                window.UpdateLayout();
                coordinator.Attach(clipHost, surface);
                var initialBuildCount = coordinator.GeometryBuildCount;

                surface.Width = 120;
                window.UpdateLayout();
                surface.Width = 140;
                window.UpdateLayout();

                Assert.Equal(initialBuildCount, coordinator.GeometryBuildCount);
                PumpDispatcher();
                Assert.Equal(initialBuildCount + 1, coordinator.GeometryBuildCount);
                var resized = Assert.IsType<RectangleGeometry>(clipHost.Clip);
                Assert.Equal(clipHost.RenderSize.Width, resized.Rect.Width, precision: 3);

                surface.CornerRadius = new CornerRadius(6);
                surface.CornerRadius = new CornerRadius(10);

                Assert.Equal(initialBuildCount + 1, coordinator.GeometryBuildCount);
                PumpDispatcher();
                Assert.Equal(initialBuildCount + 2, coordinator.GeometryBuildCount);
                Assert.Equal(
                    10,
                    Assert.IsType<RectangleGeometry>(clipHost.Clip).RadiusX
                );

                coordinator.Detach();
                surface.CornerRadius = new CornerRadius(12);
                PumpDispatcher();
                Assert.Equal(initialBuildCount + 2, coordinator.GeometryBuildCount);
            }
            finally
            {
                coordinator.Detach();
                window.Close();
            }
        });
    }

    private static void PumpDispatcher()
    {
        Dispatcher.CurrentDispatcher.Invoke(
            DispatcherPriority.ApplicationIdle,
            static () => { }
        );
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
            ExceptionDispatchInfo.Capture(error).Throw();
        }
    }
}
