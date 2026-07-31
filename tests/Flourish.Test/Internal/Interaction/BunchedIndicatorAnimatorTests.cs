using System.Windows;
using System.Windows.Controls;
using ArkheideSystem.Flourish.Internal.Interaction;

namespace ArkheideSystem.Flourish.Test.Internal.Interaction;

public sealed class BunchedIndicatorAnimatorTests
{
    private static readonly TimeSpan LongDuration = TimeSpan.FromMinutes(1);

    [Fact]
    public void Move_RetargetsOneVisibleIndicatorWithoutRestartingItsOpacity()
    {
        StaTest.Run(() =>
        {
            using var fixture = IndicatorFixture.Show();
            var indicator = fixture.Indicator;
            var animator = new BunchedIndicatorAnimator();
            var first = new Rect(4, 8, 120, 30);
            var second = new Rect(4, 44, 160, 38);

            animator.SetBounds(indicator, first);

            Assert.True(animator.Move(indicator, second, LongDuration, animate: true));

            Assert.Equal(1, indicator.Opacity);
            Assert.False(IsAnimated(indicator, UIElement.OpacityProperty));
            Assert.True(indicator.HasAnimatedProperties);
            Assert.Equal(second.Left, indicator.GetAnimationBaseValue(Canvas.LeftProperty));
            Assert.Equal(second.Top, indicator.GetAnimationBaseValue(Canvas.TopProperty));
            Assert.Equal(
                second.Width,
                indicator.GetAnimationBaseValue(FrameworkElement.WidthProperty)
            );
            Assert.Equal(
                second.Height,
                indicator.GetAnimationBaseValue(FrameworkElement.HeightProperty)
            );
        });
    }

    [Fact]
    public void Move_DuringAnActiveMoveContinuesFromTheCurrentAnimatedBounds()
    {
        StaTest.Run(() =>
        {
            using var fixture = IndicatorFixture.Show();
            var indicator = fixture.Indicator;
            var animator = new BunchedIndicatorAnimator();
            var first = new Rect(0, 0, 100, 30);
            var second = new Rect(200, 80, 140, 40);
            var third = new Rect(20, 160, 180, 48);

            animator.SetBounds(indicator, first);
            Assert.True(animator.Move(indicator, second, LongDuration, animate: true));
            DispatcherTest.DrainApplicationIdle();
            var beforeRetarget = animator.GetCurrentBounds(indicator);

            Assert.True(animator.Move(indicator, third, LongDuration, animate: true));
            DispatcherTest.DrainApplicationIdle();
            var afterRetarget = animator.GetCurrentBounds(indicator);

            AssertRectClose(beforeRetarget, afterRetarget, tolerance: 2);
            Assert.Equal(third.Left, indicator.GetAnimationBaseValue(Canvas.LeftProperty));
            Assert.Equal(third.Top, indicator.GetAnimationBaseValue(Canvas.TopProperty));
            Assert.Equal(
                third.Width,
                indicator.GetAnimationBaseValue(FrameworkElement.WidthProperty)
            );
            Assert.Equal(
                third.Height,
                indicator.GetAnimationBaseValue(FrameworkElement.HeightProperty)
            );
            Assert.Equal(1, indicator.Opacity);
            Assert.False(IsAnimated(indicator, UIElement.OpacityProperty));
        });
    }

    [Fact]
    public void Move_UsesStaticGeometryWhenMotionIsUnavailableOrTheIndicatorIsHidden()
    {
        StaTest.Run(() =>
        {
            var animator = new BunchedIndicatorAnimator();
            using var fixture = IndicatorFixture.Show();
            var indicator = fixture.Indicator;
            var first = new Rect(2, 3, 40, 20);
            var second = new Rect(8, 12, 60, 24);

            animator.SetBounds(indicator, first);
            Assert.False(animator.Move(indicator, second, LongDuration, animate: false));
            Assert.Equal(second, animator.GetCurrentBounds(indicator));
            Assert.False(indicator.HasAnimatedProperties);

            indicator.Opacity = 0;
            Assert.False(animator.Move(indicator, first, LongDuration, animate: true));
            Assert.Equal(first, animator.GetCurrentBounds(indicator));
            Assert.False(indicator.HasAnimatedProperties);
        });
    }

    [Fact]
    public void Stop_CommitsTheRequestedStateAndClearsEveryAnimationClock()
    {
        StaTest.Run(() =>
        {
            using var fixture = IndicatorFixture.Show();
            var indicator = fixture.Indicator;
            var animator = new BunchedIndicatorAnimator();
            animator.SetBounds(indicator, new Rect(0, 0, 100, 30));
            animator.Move(indicator, new Rect(50, 40, 120, 36), LongDuration, animate: true);
            animator.Hide(indicator, LongDuration, animate: true);

            Assert.True(indicator.HasAnimatedProperties);

            animator.Stop(indicator, 0);

            Assert.Equal(0, indicator.Opacity);
            Assert.False(indicator.HasAnimatedProperties);
            Assert.False(IsAnimated(indicator, UIElement.OpacityProperty));
            Assert.False(IsAnimated(indicator, Canvas.LeftProperty));
            Assert.False(IsAnimated(indicator, Canvas.TopProperty));
            Assert.False(IsAnimated(indicator, FrameworkElement.WidthProperty));
            Assert.False(IsAnimated(indicator, FrameworkElement.HeightProperty));
        });
    }

    [Fact]
    public void BoundsOperations_RejectInvalidGeometry()
    {
        StaTest.Run(() =>
        {
            var indicator = new Border();
            var animator = new BunchedIndicatorAnimator();

            Assert.Throws<ArgumentOutOfRangeException>(() =>
                animator.SetBounds(indicator, Rect.Empty)
            );
            Assert.Throws<ArgumentOutOfRangeException>(() =>
                animator.Move(
                    indicator,
                    new Rect(double.NaN, 0, 10, 10),
                    TimeSpan.Zero,
                    animate: false
                )
            );
        });
    }

    private static bool IsAnimated(DependencyObject owner, DependencyProperty property)
    {
        return DependencyPropertyHelper.GetValueSource(owner, property).IsAnimated;
    }

    private static void AssertRectClose(Rect expected, Rect actual, double tolerance)
    {
        Assert.InRange(actual.X, expected.X - tolerance, expected.X + tolerance);
        Assert.InRange(actual.Y, expected.Y - tolerance, expected.Y + tolerance);
        Assert.InRange(actual.Width, expected.Width - tolerance, expected.Width + tolerance);
        Assert.InRange(actual.Height, expected.Height - tolerance, expected.Height + tolerance);
    }

    private sealed class IndicatorFixture : IDisposable
    {
        private IndicatorFixture(Border indicator, Window window)
        {
            Indicator = indicator;
            Window = window;
        }

        internal Border Indicator { get; }

        private Window Window { get; }

        internal static IndicatorFixture Show()
        {
            var indicator = new Border { Opacity = 1 };
            var canvas = new Canvas { Children = { indicator } };
            var window = new Window
            {
                Width = 320,
                Height = 180,
                Left = -10000,
                Top = -10000,
                ShowActivated = false,
                ShowInTaskbar = false,
                Content = canvas,
            };
            window.Show();
            window.UpdateLayout();
            return new IndicatorFixture(indicator, window);
        }

        public void Dispose()
        {
            Window.Close();
        }
    }
}
