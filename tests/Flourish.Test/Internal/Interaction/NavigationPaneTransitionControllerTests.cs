using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using ArkheideSystem.Flourish.Abstract;
using ArkheideSystem.Flourish.Internal.Configuration;
using ArkheideSystem.Flourish.Internal.Interaction;
using ArkheideSystem.Flourish.Services;

namespace ArkheideSystem.Flourish.Test.Internal.Interaction;

public sealed class NavigationPaneTransitionControllerTests
{
    private static readonly TimeSpan Duration = TimeSpan.FromMilliseconds(200);

    public static TheoryData<NavigationPanelDirection, double, double> GeometryCases =>
        new()
        {
            { NavigationPanelDirection.Left, 48, 220 },
            { NavigationPanelDirection.Left, 220, 48 },
            { NavigationPanelDirection.Right, 48, 220 },
            { NavigationPanelDirection.Right, 220, 48 },
        };

    public static TheoryData<NavigationPanelDirection, double, double, double>
        CenteredGeometryCases =>
        new()
        {
            { NavigationPanelDirection.Left, 220, 48, 1600 },
            { NavigationPanelDirection.Left, 48, 220, 1600 },
            { NavigationPanelDirection.Right, 220, 48, 1600 },
            { NavigationPanelDirection.Right, 48, 220, 1600 },
            { NavigationPanelDirection.Left, 220, 48, 700 },
            { NavigationPanelDirection.Left, 48, 220, 700 },
            { NavigationPanelDirection.Right, 220, 48, 700 },
            { NavigationPanelDirection.Right, 48, 220, 700 },
        };

    [Theory]
    [MemberData(nameof(GeometryCases))]
    public void Start_UsesRenderOnlyGeometryForLeftAndRightOpenClose(
        NavigationPanelDirection direction,
        double committedWidth,
        double targetWidth
    )
    {
        RunInSta(() =>
        {
            var fixture = TransitionFixture.Create(direction, committedWidth);
            var sut = new NavigationPaneTransitionController();
            var completionCount = 0;

            Assert.True(
                sut.Start(
                    fixture.Target,
                    committedWidth,
                    targetWidth,
                    maximumPaneWidth: 420,
                    referenceDistance: 172,
                    Duration,
                    new LinearEase(),
                    () => completionCount++
                )
            );

            var clock = sut.ActiveClockController;
            Assert.NotNull(clock);
            clock!.SeekAlignedToLastTick(TimeSpan.Zero, TimeSeekOrigin.BeginTime);

            Assert.False(IsWidthAnimated(fixture.PaneColumn));
            Assert.Equal(0, Grid.GetColumn(fixture.Pane));
            Assert.Equal(2, Grid.GetColumnSpan(fixture.Pane));
            Assert.Equal(420, fixture.Pane.Width);
            Assert.Equal(
                direction == NavigationPanelDirection.Left
                    ? HorizontalAlignment.Left
                    : HorizontalAlignment.Right,
                fixture.Pane.HorizontalAlignment
            );

            var clip = Assert.IsType<RectangleGeometry>(fixture.Pane.Clip);
            var transitionTransform = Assert.IsType<TransformGroup>(
                fixture.Content.RenderTransform
            );
            var contentScale = Assert.IsType<ScaleTransform>(
                transitionTransform.Children[0]
            );
            var contentTranslation = Assert.IsType<TranslateTransform>(
                transitionTransform.Children[1]
            );
            AssertClip(direction, 420, committedWidth, clip.Rect);
            Assert.NotSame(fixture.OriginalContentTransform, transitionTransform);
            Assert.Equal(new Point(), fixture.Content.RenderTransformOrigin);
            AssertClose(1, contentScale.ScaleX);
            AssertClose(0, contentTranslation.X);

            clock.SeekAlignedToLastTick(Duration / 2, TimeSeekOrigin.BeginTime);

            var midpointWidth = (committedWidth + targetWidth) / 2;
            var targetScale =
                (fixture.Content.ActualWidth + committedWidth - targetWidth)
                / fixture.Content.ActualWidth;
            var targetTranslation = direction == NavigationPanelDirection.Left
                ? targetWidth - committedWidth
                : 0;

            AssertClip(direction, 420, midpointWidth, clip.Rect);
            AssertClose((1 + targetScale) / 2, contentScale.ScaleX);
            AssertClose(targetTranslation / 2, contentTranslation.X);
            Assert.False(IsWidthAnimated(fixture.PaneColumn));
            Assert.Equal(0, completionCount);
        });
    }

    [Theory]
    [MemberData(nameof(CenteredGeometryCases))]
    public void Start_WithCenteredContent_KeepsTransformedWidthWithinTheMaximum(
        NavigationPanelDirection direction,
        double committedWidth,
        double targetWidth,
        double workWidth
    )
    {
        RunInSta(() =>
        {
            var fixture = CenteredTransitionFixture.Create(
                direction,
                committedWidth,
                workWidth
            );
            var sut = new NavigationPaneTransitionController();
            var initialBounds = fixture.GetCenteredBounds();
            AssertClose(initialBounds.Width, fixture.GetScrollableCenteredBounds().Width);
            var initialTextWidth = fixture.CenteredText.ActualWidth;
            var initialTextHeight = fixture.CenteredText.ActualHeight;
            var targetBounds = fixture.GetExpectedCenteredBounds(targetWidth);
            var completionCount = 0;

            Assert.True(
                sut.Start(
                    fixture.Target,
                    committedWidth,
                    targetWidth,
                    maximumPaneWidth: 420,
                    referenceDistance: 172,
                    Duration,
                    new LinearEase(),
                    () =>
                    {
                        completionCount++;
                        fixture.SetPaneWidth(targetWidth);
                        fixture.Layout();
                    }
                )
            );
            var clock = Assert.IsAssignableFrom<ClockController>(
                sut.ActiveClockController
            );

            foreach (var progress in new[] { 0d, 0.25, 0.5, 0.75, 0.99 })
            {
                clock.SeekAlignedToLastTick(
                    TimeSpan.FromTicks((long)(Duration.Ticks * progress)),
                    TimeSeekOrigin.BeginTime
                );
                fixture.Layout();
                var bounds = fixture.GetCenteredBounds();
                var scrollableBounds = fixture.GetScrollableCenteredBounds();

                AssertClose(
                    Lerp(initialBounds.Width, targetBounds.Width, progress),
                    bounds.Width
                );
                AssertClose(bounds.Width, scrollableBounds.Width);
                AssertClose(
                    Lerp(GetCenterX(initialBounds), GetCenterX(targetBounds), progress),
                    GetCenterX(bounds)
                );
                AssertClose(GetCenterX(bounds), GetCenterX(scrollableBounds));
                Assert.True(
                    bounds.Width <= CenteredTransitionFixture.ContentMaximumWidth + 0.001,
                    $"Centered content rendered at {bounds.Width} DIP."
                );
                if (workWidth == 1600)
                {
                    AssertClose(
                        fixture.CenteredText.ActualWidth,
                        fixture.GetCenteredTextBounds().Width
                    );
                    AssertClose(
                        fixture.ScrollableCenteredText.ActualWidth,
                        fixture.GetScrollableCenteredTextBounds().Width
                    );
                    AssertClose(initialTextWidth, fixture.CenteredText.ActualWidth);
                    AssertClose(initialTextHeight, fixture.CenteredText.ActualHeight);
                    AssertClose(
                        24,
                        fixture.GetCenteredTextBounds().Left - bounds.Left
                    );
                }
                Assert.False(IsWidthAnimated(fixture.PaneColumn));
            }

            clock.SeekAlignedToLastTick(Duration, TimeSeekOrigin.BeginTime);

            var completedBounds = fixture.GetCenteredBounds();
            Assert.Equal(1, completionCount);
            AssertClose(targetBounds.Width, completedBounds.Width);
            AssertClose(GetCenterX(targetBounds), GetCenterX(completedBounds));
            Assert.Same(
                DependencyProperty.UnsetValue,
                fixture.Centered.ReadLocalValue(UIElement.RenderTransformProperty)
            );
            Assert.Same(
                DependencyProperty.UnsetValue,
                fixture.Centered.ReadLocalValue(UIElement.RenderTransformOriginProperty)
            );
            Assert.Same(
                DependencyProperty.UnsetValue,
                fixture.ScrollableCentered.ReadLocalValue(UIElement.RenderTransformProperty)
            );
            Assert.Same(
                DependencyProperty.UnsetValue,
                fixture.ScrollableCentered.ReadLocalValue(
                    UIElement.RenderTransformOriginProperty
                )
            );
        });
    }

    [Theory]
    [MemberData(nameof(GeometryCases))]
    public void Start_WithCappedScrollableText_KeepsTextUnscaledAndCentersOnViewport(
        NavigationPanelDirection direction,
        double committedWidth,
        double targetWidth
    )
    {
        RunInSta(() =>
        {
            const double workWidth = 1000;
            var workArea = new Grid { Width = workWidth, Height = 400 };
            var paneColumn = new ColumnDefinition
            {
                Width = new GridLength(committedWidth),
            };
            var contentColumn = new ColumnDefinition
            {
                Width = new GridLength(1, GridUnitType.Star),
            };
            var paneColumnIndex = direction == NavigationPanelDirection.Left ? 0 : 1;
            if (paneColumnIndex == 0)
            {
                workArea.ColumnDefinitions.Add(paneColumn);
                workArea.ColumnDefinitions.Add(contentColumn);
            }
            else
            {
                workArea.ColumnDefinitions.Add(contentColumn);
                workArea.ColumnDefinitions.Add(paneColumn);
            }

            var pane = new Border();
            Grid.SetColumn(pane, paneColumnIndex);
            var text = new TextBlock
            {
                Text = "Scrollable centered text keeps its natural metrics while navigation changes.",
                TextWrapping = TextWrapping.Wrap,
            };
            var centered = new Border
            {
                Child = text,
                Height = 900,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                Margin = new Thickness(32, 0, 32, 0),
                MaxWidth = CenteredTransitionFixture.ContentMaximumWidth,
                Padding = new Thickness(24, 0, 24, 0),
                VerticalAlignment = VerticalAlignment.Top,
            };
            var scrollViewer = new ScrollViewer
            {
                Content = centered,
                HorizontalContentAlignment = HorizontalAlignment.Stretch,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
                VerticalScrollBarVisibility = ScrollBarVisibility.Visible,
            };
            var content = new Grid();
            content.Children.Add(scrollViewer);
            Grid.SetColumn(content, paneColumnIndex == 0 ? 1 : 0);
            workArea.Children.Add(pane);
            workArea.Children.Add(content);

            var window = new Window
            {
                Content = workArea,
                Left = -10_000,
                ShowActivated = false,
                ShowInTaskbar = false,
                SizeToContent = SizeToContent.WidthAndHeight,
                Top = -10_000,
                WindowStartupLocation = WindowStartupLocation.Manual,
                WindowStyle = WindowStyle.None,
            };
            window.Show();

            try
            {
                PumpRender();
                var sut = new NavigationPaneTransitionController();
                var initialBounds = GetTransformedBounds(centered, workArea);
                var initialTextWidth = text.ActualWidth;
                var initialTextHeight = text.ActualHeight;
                var paneWidthDelta = targetWidth - committedWidth;
                var expectedTargetCenter = GetCenterX(initialBounds)
                    + (direction == NavigationPanelDirection.Left
                        ? paneWidthDelta / 2
                        : -paneWidthDelta / 2);

                Assert.True(
                    sut.Start(
                        new NavigationPaneTransitionTarget(
                            workArea,
                            pane,
                            content,
                            direction,
                            [centered]
                        ),
                        committedWidth,
                        targetWidth,
                        maximumPaneWidth: 420,
                        referenceDistance: 172,
                        Duration,
                        new LinearEase(),
                        () =>
                        {
                            paneColumn.Width = new GridLength(targetWidth);
                            workArea.UpdateLayout();
                        }
                    )
                );
                var clock = Assert.IsAssignableFrom<ClockController>(
                    sut.ActiveClockController
                );
                clock.Pause();

                foreach (var progress in new[] { 0d, 0.25, 0.5, 0.75, 0.999 })
                {
                    clock.SeekAlignedToLastTick(
                        TimeSpan.FromTicks((long)(Duration.Ticks * progress)),
                        TimeSeekOrigin.BeginTime
                    );
                    PumpRender();
                    var bounds = GetTransformedBounds(centered, workArea);
                    var textBounds = GetTransformedBounds(text, workArea);

                    AssertClose(
                        Lerp(GetCenterX(initialBounds), expectedTargetCenter, progress),
                        GetCenterX(bounds)
                    );
                    AssertClose(
                        CenteredTransitionFixture.ContentMaximumWidth,
                        bounds.Width
                    );
                    AssertClose(text.ActualWidth, textBounds.Width);
                    AssertClose(initialTextWidth, text.ActualWidth);
                    AssertClose(initialTextHeight, text.ActualHeight);
                    AssertClose(24, textBounds.Left - bounds.Left);
                    Assert.False(
                        DependencyPropertyHelper
                            .GetValueSource(
                                centered,
                                FrameworkElement.MaxWidthProperty
                            )
                            .IsAnimated
                    );
                }

                clock.SeekAlignedToLastTick(Duration, TimeSeekOrigin.BeginTime);
                PumpRender();
                var finalBounds = GetTransformedBounds(centered, workArea);
                var finalTextBounds = GetTransformedBounds(text, workArea);

                AssertClose(expectedTargetCenter, GetCenterX(finalBounds));
                AssertClose(initialBounds.Width, finalBounds.Width);
                AssertClose(text.ActualWidth, finalTextBounds.Width);
                AssertClose(initialTextWidth, text.ActualWidth);
                AssertClose(initialTextHeight, text.ActualHeight);
                AssertClose(24, finalTextBounds.Left - finalBounds.Left);
                Assert.Same(
                    DependencyProperty.UnsetValue,
                    centered.ReadLocalValue(UIElement.RenderTransformProperty)
                );
            }
            finally
            {
                window.Close();
            }
        });
    }

    [Theory]
    [InlineData(NavigationPanelDirection.Left)]
    [InlineData(NavigationPanelDirection.Right)]
    public void Reverse_WithCenteredContent_ContinuesFromCurrentTransformedBounds(
        NavigationPanelDirection direction
    )
    {
        RunInSta(() =>
        {
            var fixture = CenteredTransitionFixture.Create(direction, 220, 1600);
            var sut = new NavigationPaneTransitionController();
            var firstCompletionCount = 0;
            var reverseCompletionCount = 0;

            Assert.True(
                sut.Start(
                    fixture.Target,
                    committedWidth: 220,
                    targetWidth: 48,
                    maximumPaneWidth: 420,
                    referenceDistance: 172,
                    Duration,
                    new LinearEase(),
                    () => firstCompletionCount++
                )
            );
            var firstClock = Assert.IsAssignableFrom<ClockController>(
                sut.ActiveClockController
            );
            firstClock.SeekAlignedToLastTick(Duration / 2, TimeSeekOrigin.BeginTime);
            fixture.Layout();
            var boundsBeforeReverse = fixture.GetCenteredBounds();
            var textBoundsBeforeReverse = fixture.GetCenteredTextBounds();

            Assert.True(
                sut.Start(
                    fixture.Target,
                    committedWidth: 220,
                    targetWidth: 220,
                    maximumPaneWidth: 420,
                    referenceDistance: 172,
                    Duration,
                    new LinearEase(),
                    () => reverseCompletionCount++
                )
            );
            var reverseClock = Assert.IsAssignableFrom<ClockController>(
                sut.ActiveClockController
            );
            reverseClock.SeekAlignedToLastTick(TimeSpan.Zero, TimeSeekOrigin.BeginTime);
            fixture.Layout();
            var boundsAfterReverse = fixture.GetCenteredBounds();
            var textBoundsAfterReverse = fixture.GetCenteredTextBounds();

            AssertClose(boundsBeforeReverse.Width, boundsAfterReverse.Width);
            AssertClose(GetCenterX(boundsBeforeReverse), GetCenterX(boundsAfterReverse));
            AssertClose(textBoundsBeforeReverse.Width, textBoundsAfterReverse.Width);
            AssertClose(
                GetCenterX(textBoundsBeforeReverse),
                GetCenterX(textBoundsAfterReverse)
            );
            AssertClose(fixture.CenteredText.ActualWidth, textBoundsAfterReverse.Width);
            Assert.True(
                boundsAfterReverse.Width
                    <= CenteredTransitionFixture.ContentMaximumWidth + 0.001
            );
            Assert.Equal(0, firstCompletionCount);
            Assert.Equal(0, reverseCompletionCount);

            reverseClock.SeekAlignedToLastTick(Duration, TimeSeekOrigin.BeginTime);

            Assert.Equal(0, firstCompletionCount);
            Assert.Equal(1, reverseCompletionCount);
            Assert.False(sut.IsActive);
        });
    }

    [Fact]
    public void Cancel_WithCappedCenteredContent_RestoresTextAndTransformState()
    {
        RunInSta(() =>
        {
            var fixture = CenteredTransitionFixture.Create(
                NavigationPanelDirection.Right,
                paneWidth: 48,
                workWidth: 1600
            );
            var initialBounds = fixture.GetCenteredBounds();
            var initialTextBounds = fixture.GetCenteredTextBounds();
            var sut = new NavigationPaneTransitionController();
            var completionCount = 0;

            Assert.True(
                sut.Start(
                    fixture.Target,
                    committedWidth: 48,
                    targetWidth: 220,
                    maximumPaneWidth: 420,
                    referenceDistance: 172,
                    Duration,
                    new LinearEase(),
                    () => completionCount++
                )
            );
            var clock = Assert.IsAssignableFrom<ClockController>(
                sut.ActiveClockController
            );
            clock.SeekAlignedToLastTick(Duration / 2, TimeSeekOrigin.BeginTime);
            fixture.Layout();

            sut.Cancel();
            fixture.Layout();

            var restoredBounds = fixture.GetCenteredBounds();
            var restoredTextBounds = fixture.GetCenteredTextBounds();
            AssertClose(initialBounds.Width, restoredBounds.Width);
            AssertClose(GetCenterX(initialBounds), GetCenterX(restoredBounds));
            AssertClose(initialTextBounds.Width, restoredTextBounds.Width);
            AssertClose(
                GetCenterX(initialTextBounds),
                GetCenterX(restoredTextBounds)
            );
            Assert.Same(
                DependencyProperty.UnsetValue,
                fixture.Centered.ReadLocalValue(UIElement.RenderTransformProperty)
            );
            Assert.Same(
                DependencyProperty.UnsetValue,
                fixture.Centered.ReadLocalValue(
                    UIElement.RenderTransformOriginProperty
                )
            );
            Assert.False(
                DependencyPropertyHelper
                    .GetValueSource(
                        fixture.Centered,
                        FrameworkElement.MaxWidthProperty
                    )
                    .IsAnimated
            );
            Assert.Equal(0, completionCount);
            Assert.False(sut.IsActive);
        });
    }

    [Fact]
    public void Completion_InvokesCallbackOnceAndRestoresPresentationState()
    {
        RunInSta(() =>
        {
            var fixture = TransitionFixture.Create(NavigationPanelDirection.Left, 48);
            var sut = new NavigationPaneTransitionController();
            var completionCount = 0;

            Assert.True(
                sut.Start(
                    fixture.Target,
                    committedWidth: 48,
                    targetWidth: 220,
                    maximumPaneWidth: 420,
                    referenceDistance: 172,
                    Duration,
                    new LinearEase(),
                    () => completionCount++
                )
            );
            var animatedClip = Assert.IsType<RectangleGeometry>(fixture.Pane.Clip);
            var animatedTransform = Assert.IsType<TransformGroup>(
                fixture.Content.RenderTransform
            );
            var animatedScale = Assert.IsType<ScaleTransform>(
                animatedTransform.Children[0]
            );
            var animatedTranslation = Assert.IsType<TranslateTransform>(
                animatedTransform.Children[1]
            );
            var clock = sut.ActiveClockController;
            Assert.NotNull(clock);

            clock!.SeekAlignedToLastTick(Duration, TimeSeekOrigin.BeginTime);

            Assert.Equal(1, completionCount);
            Assert.False(sut.IsActive);
            Assert.Null(sut.ActiveClockController);
            Assert.Null(fixture.Pane.Clip);
            Assert.True(double.IsNaN(fixture.Pane.Width));
            Assert.Equal(HorizontalAlignment.Stretch, fixture.Pane.HorizontalAlignment);
            Assert.Equal(fixture.OriginalPaneColumn, Grid.GetColumn(fixture.Pane));
            Assert.Equal(1, Grid.GetColumnSpan(fixture.Pane));
            AssertContentPresentationRestored(fixture);
            Assert.Same(
                DependencyProperty.UnsetValue,
                fixture.Content.ReadLocalValue(UIElement.RenderTransformProperty)
            );
            Assert.Same(
                DependencyProperty.UnsetValue,
                fixture.Content.ReadLocalValue(UIElement.RenderTransformOriginProperty)
            );
            Assert.False(animatedClip.HasAnimatedProperties);
            Assert.False(animatedScale.HasAnimatedProperties);
            Assert.False(animatedTranslation.HasAnimatedProperties);
            Assert.False(IsWidthAnimated(fixture.PaneColumn));

            sut.Cancel();
            Assert.Equal(1, completionCount);
        });
    }

    [Fact]
    public void Reverse_ContinuesFromCurrentWidthAndDoesNotRunSupersededCallback()
    {
        RunInSta(() =>
        {
            var fixture = TransitionFixture.Create(NavigationPanelDirection.Left, 48);
            var sut = new NavigationPaneTransitionController();
            var firstCompletionCount = 0;
            var reverseCompletionCount = 0;

            Assert.True(
                sut.Start(
                    fixture.Target,
                    committedWidth: 48,
                    targetWidth: 220,
                    maximumPaneWidth: 420,
                    referenceDistance: 172,
                    Duration,
                    new LinearEase(),
                    () => firstCompletionCount++
                )
            );
            var firstClock = sut.ActiveClockController;
            Assert.NotNull(firstClock);
            firstClock!.SeekAlignedToLastTick(Duration / 2, TimeSeekOrigin.BeginTime);
            var widthBeforeReverse = Assert.IsType<double>(sut.CurrentVisualWidth);
            var transitionTransform = Assert.IsType<TransformGroup>(
                fixture.Content.RenderTransform
            );
            var contentScale = Assert.IsType<ScaleTransform>(
                transitionTransform.Children[0]
            );
            var contentTranslation = Assert.IsType<TranslateTransform>(
                transitionTransform.Children[1]
            );
            var scaleBeforeReverse = contentScale.ScaleX;
            var translationBeforeReverse = contentTranslation.X;

            Assert.True(
                sut.Start(
                    fixture.Target,
                    committedWidth: 48,
                    targetWidth: 48,
                    maximumPaneWidth: 420,
                    referenceDistance: 172,
                    Duration,
                    new LinearEase(),
                    () => reverseCompletionCount++
                )
            );
            var reverseClock = sut.ActiveClockController;
            Assert.NotNull(reverseClock);
            reverseClock!.SeekAlignedToLastTick(TimeSpan.Zero, TimeSeekOrigin.BeginTime);

            AssertClose(widthBeforeReverse, sut.CurrentVisualWidth!.Value);
            Assert.Same(transitionTransform, fixture.Content.RenderTransform);
            AssertClose(scaleBeforeReverse, contentScale.ScaleX);
            AssertClose(translationBeforeReverse, contentTranslation.X);
            Assert.Equal(0, firstCompletionCount);
            Assert.Equal(0, reverseCompletionCount);
            Assert.False(IsWidthAnimated(fixture.PaneColumn));

            reverseClock.SeekAlignedToLastTick(Duration / 2, TimeSeekOrigin.BeginTime);

            Assert.Equal(0, firstCompletionCount);
            Assert.Equal(1, reverseCompletionCount);
            Assert.False(sut.IsActive);
            Assert.Null(fixture.Pane.Clip);
            AssertContentPresentationRestored(fixture);
            Assert.False(IsWidthAnimated(fixture.PaneColumn));
        });
    }

    [Fact]
    public void MotionService_ReverseToCommittedWidthKeepsTheControllerActiveUntilItReturns()
    {
        RunInSta(() =>
        {
            var fixture = TransitionFixture.Create(NavigationPanelDirection.Left, 48);
            var controller = new NavigationPaneTransitionController();
            var options = new FlourishShellOptions();
            options.Motion.IsEnabled = true;
            options.Motion.RespectSystemReducedMotion = false;
            options.Motion.NavigationPanelTransition =
                FlourishNavigationPanelTransition.Resize;
            options.Motion.NavigationPanelTransitionDuration = Duration;
            var sut = new FlourishMotionService(options, static () => true);
            var openingCompletionCount = 0;
            var reverseCompletionCount = 0;

            sut.AnimateNavigationPane(
                controller,
                fixture.Target,
                committedWidth: 48,
                targetWidth: 220,
                maximumPaneWidth: 420,
                referenceDistance: 172,
                () => openingCompletionCount++
            );
            var openingClock = controller.ActiveClockController;
            Assert.NotNull(openingClock);
            openingClock!.SeekAlignedToLastTick(Duration / 2, TimeSeekOrigin.BeginTime);
            var widthBeforeReverse = controller.CurrentVisualWidth;
            Assert.NotNull(widthBeforeReverse);

            sut.AnimateNavigationPane(
                controller,
                fixture.Target,
                committedWidth: 48,
                targetWidth: 48,
                maximumPaneWidth: 420,
                referenceDistance: 172,
                () => reverseCompletionCount++
            );

            Assert.True(controller.IsActive);
            Assert.Equal(0, openingCompletionCount);
            Assert.Equal(0, reverseCompletionCount);
            var reverseClock = controller.ActiveClockController;
            Assert.NotNull(reverseClock);
            reverseClock!.SeekAlignedToLastTick(TimeSpan.Zero, TimeSeekOrigin.BeginTime);
            AssertClose(widthBeforeReverse!.Value, controller.CurrentVisualWidth!.Value);

            reverseClock.SeekAlignedToLastTick(Duration, TimeSeekOrigin.BeginTime);

            Assert.False(controller.IsActive);
            Assert.Equal(0, openingCompletionCount);
            Assert.Equal(1, reverseCompletionCount);
            Assert.Null(fixture.Pane.Clip);
            AssertContentPresentationRestored(fixture);
            Assert.False(IsWidthAnimated(fixture.PaneColumn));
        });
    }

    [Fact]
    public void Cancel_DropsCallbackAndRestoresPresentationState()
    {
        RunInSta(() =>
        {
            var fixture = TransitionFixture.Create(NavigationPanelDirection.Right, 220);
            var sut = new NavigationPaneTransitionController();
            var completionCount = 0;

            Assert.True(
                sut.Start(
                    fixture.Target,
                    committedWidth: 220,
                    targetWidth: 48,
                    maximumPaneWidth: 420,
                    referenceDistance: 172,
                    Duration,
                    new LinearEase(),
                    () => completionCount++
                )
            );
            var animatedClip = Assert.IsType<RectangleGeometry>(fixture.Pane.Clip);
            var animatedTransform = Assert.IsType<TransformGroup>(
                fixture.Content.RenderTransform
            );
            var animatedScale = Assert.IsType<ScaleTransform>(
                animatedTransform.Children[0]
            );
            var animatedTranslation = Assert.IsType<TranslateTransform>(
                animatedTransform.Children[1]
            );
            var clock = sut.ActiveClockController;
            Assert.NotNull(clock);
            clock!.SeekAlignedToLastTick(Duration / 2, TimeSeekOrigin.BeginTime);

            sut.Cancel();

            Assert.Equal(0, completionCount);
            Assert.False(sut.IsActive);
            Assert.Null(fixture.Pane.Clip);
            Assert.True(double.IsNaN(fixture.Pane.Width));
            Assert.Equal(HorizontalAlignment.Stretch, fixture.Pane.HorizontalAlignment);
            Assert.Equal(fixture.OriginalPaneColumn, Grid.GetColumn(fixture.Pane));
            Assert.Equal(1, Grid.GetColumnSpan(fixture.Pane));
            AssertContentPresentationRestored(fixture);
            Assert.Same(
                DependencyProperty.UnsetValue,
                fixture.Content.ReadLocalValue(UIElement.RenderTransformProperty)
            );
            Assert.Same(
                DependencyProperty.UnsetValue,
                fixture.Content.ReadLocalValue(UIElement.RenderTransformOriginProperty)
            );
            Assert.False(animatedClip.HasAnimatedProperties);
            Assert.False(animatedScale.HasAnimatedProperties);
            Assert.False(animatedTranslation.HasAnimatedProperties);
            Assert.False(IsWidthAnimated(fixture.PaneColumn));
        });
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void End_RestoresNonDefaultContentTransformAndOrigin(bool complete)
    {
        RunInSta(() =>
        {
            var originalTransform = new RotateTransform(7);
            var originalOrigin = new Point(0.25, 0.75);
            var fixture = TransitionFixture.Create(
                NavigationPanelDirection.Left,
                48,
                originalTransform,
                originalOrigin
            );
            var sut = new NavigationPaneTransitionController();
            var completionCount = 0;

            Assert.True(
                sut.Start(
                    fixture.Target,
                    committedWidth: 48,
                    targetWidth: 220,
                    maximumPaneWidth: 420,
                    referenceDistance: 172,
                    Duration,
                    new LinearEase(),
                    () => completionCount++
                )
            );
            var temporaryTransform = Assert.IsType<TransformGroup>(
                fixture.Content.RenderTransform
            );
            Assert.NotSame(originalTransform, temporaryTransform);
            Assert.Equal(new Point(), fixture.Content.RenderTransformOrigin);

            if (complete)
            {
                var clock = sut.ActiveClockController;
                Assert.NotNull(clock);
                clock!.SeekAlignedToLastTick(Duration, TimeSeekOrigin.BeginTime);
            }
            else
            {
                sut.Cancel();
            }

            Assert.Equal(complete ? 1 : 0, completionCount);
            Assert.False(sut.IsActive);
            AssertContentPresentationRestored(fixture);
            Assert.Same(
                originalTransform,
                fixture.Content.ReadLocalValue(UIElement.RenderTransformProperty)
            );
            Assert.Equal(
                originalOrigin,
                fixture.Content.ReadLocalValue(UIElement.RenderTransformOriginProperty)
            );
            Assert.Null(fixture.Pane.Clip);
            Assert.False(IsWidthAnimated(fixture.PaneColumn));
        });
    }

    [Fact]
    public void Start_WhenTemporaryPresentationInstallationFails_RestoresPresentationState()
    {
        RunInSta(() =>
        {
            var originalTransform = new SkewTransform(4, 2);
            var originalOrigin = new Point(0.4, 0.6);
            var fixture = TransitionFixture.Create(
                NavigationPanelDirection.Right,
                220,
                originalTransform,
                originalOrigin
            );
            var sut = new NavigationPaneTransitionController();
            var completionCount = 0;
            fixture.Content.ThrowOnNextRenderTransformOriginChange = true;

            Assert.Throws<InvalidOperationException>(() =>
                sut.Start(
                    fixture.Target,
                    committedWidth: 220,
                    targetWidth: 48,
                    maximumPaneWidth: 420,
                    referenceDistance: 172,
                    Duration,
                    new LinearEase(),
                    () => completionCount++
                )
            );

            Assert.Equal(0, completionCount);
            Assert.False(sut.IsActive);
            Assert.Null(sut.ActiveClockController);
            AssertContentPresentationRestored(fixture);
            Assert.Null(fixture.Pane.Clip);
            Assert.True(double.IsNaN(fixture.Pane.Width));
            Assert.Equal(HorizontalAlignment.Stretch, fixture.Pane.HorizontalAlignment);
            Assert.Equal(fixture.OriginalPaneColumn, Grid.GetColumn(fixture.Pane));
            Assert.Equal(1, Grid.GetColumnSpan(fixture.Pane));
            Assert.False(IsWidthAnimated(fixture.PaneColumn));
        });
    }

    [Theory]
    [InlineData(false, FlourishNavigationPanelTransition.Resize, false, true)]
    [InlineData(true, FlourishNavigationPanelTransition.None, false, true)]
    [InlineData(true, FlourishNavigationPanelTransition.Resize, true, false)]
    public void MotionService_NonAnimatedPoliciesCompleteImmediatelyWithoutAnActiveClock(
        bool enabled,
        FlourishNavigationPanelTransition transition,
        bool respectReducedMotion,
        bool systemAnimationsEnabled
    )
    {
        RunInSta(() =>
        {
            var fixture = TransitionFixture.Create(NavigationPanelDirection.Left, 48);
            var controller = new NavigationPaneTransitionController();
            var options = new FlourishShellOptions();
            options.Motion.IsEnabled = enabled;
            options.Motion.NavigationPanelTransition = transition;
            options.Motion.RespectSystemReducedMotion = respectReducedMotion;
            var sut = new FlourishMotionService(options, () => systemAnimationsEnabled);
            var completionCount = 0;

            sut.AnimateNavigationPane(
                controller,
                fixture.Target,
                committedWidth: 48,
                targetWidth: 220,
                maximumPaneWidth: 420,
                referenceDistance: 172,
                () => completionCount++
            );

            Assert.Equal(1, completionCount);
            Assert.False(controller.IsActive);
            Assert.Null(controller.ActiveClockController);
            Assert.Null(fixture.Pane.Clip);
            AssertContentPresentationRestored(fixture);
            Assert.False(IsWidthAnimated(fixture.PaneColumn));
        });
    }

    [Fact]
    public void SeekingRenderClocksDoesNotRemeasureOrRearrangeContent()
    {
        RunInSta(() =>
        {
            var fixture = TransitionFixture.Create(NavigationPanelDirection.Left, 48);
            var sut = new NavigationPaneTransitionController();

            Assert.True(
                sut.Start(
                    fixture.Target,
                    committedWidth: 48,
                    targetWidth: 220,
                    maximumPaneWidth: 420,
                    referenceDistance: 172,
                    Duration,
                    new LinearEase(),
                    static () => { }
                )
            );
            fixture.Layout();
            fixture.Content.ResetLayoutCounts();
            var clock = sut.ActiveClockController;
            Assert.NotNull(clock);

            clock!.SeekAlignedToLastTick(Duration / 4, TimeSeekOrigin.BeginTime);
            fixture.Layout();
            clock.SeekAlignedToLastTick(Duration / 2, TimeSeekOrigin.BeginTime);
            fixture.Layout();
            clock.SeekAlignedToLastTick(Duration * 3 / 4, TimeSeekOrigin.BeginTime);
            fixture.Layout();

            Assert.Equal(0, fixture.Content.MeasureCount);
            Assert.Equal(0, fixture.Content.ArrangeCount);
            Assert.False(IsWidthAnimated(fixture.PaneColumn));
        });
    }

    [Fact]
    public void SeekingCappedCenteredContentClocks_UsesTransformCompensationWithoutLayout()
    {
        RunInSta(() =>
        {
            var fixture = CenteredTransitionFixture.Create(
                NavigationPanelDirection.Left,
                paneWidth: 220,
                workWidth: 1600
            );
            var sut = new NavigationPaneTransitionController();

            Assert.True(
                sut.Start(
                    fixture.Target,
                    committedWidth: 220,
                    targetWidth: 48,
                    maximumPaneWidth: 420,
                    referenceDistance: 172,
                    Duration,
                    new LinearEase(),
                    static () => { }
                )
            );
            fixture.Layout();
            fixture.Content.ResetLayoutCounts();
            var clock = Assert.IsAssignableFrom<ClockController>(
                sut.ActiveClockController
            );

            clock.SeekAlignedToLastTick(Duration / 4, TimeSeekOrigin.BeginTime);
            fixture.Layout();
            clock.SeekAlignedToLastTick(Duration / 2, TimeSeekOrigin.BeginTime);
            fixture.Layout();
            clock.SeekAlignedToLastTick(Duration * 3 / 4, TimeSeekOrigin.BeginTime);
            fixture.Layout();

            Assert.False(
                DependencyPropertyHelper
                    .GetValueSource(
                        fixture.Centered,
                        FrameworkElement.MaxWidthProperty
                    )
                    .IsAnimated
            );
            Assert.False(
                DependencyPropertyHelper
                    .GetValueSource(
                        fixture.ScrollableCentered,
                        FrameworkElement.MaxWidthProperty
                    )
                    .IsAnimated
            );
            Assert.True(IsCounterScaleAnimated(fixture.Centered));
            Assert.True(IsCounterScaleAnimated(fixture.ScrollableCentered));
            Assert.Equal(0, fixture.Content.MeasureCount);
            Assert.Equal(0, fixture.Content.ArrangeCount);
            Assert.False(IsWidthAnimated(fixture.PaneColumn));
        });
    }

    [Fact]
    public void SeekingCrossThresholdCenteredContentClocks_UsesWidthCompensation()
    {
        RunInSta(() =>
        {
            var fixture = CenteredTransitionFixture.Create(
                NavigationPanelDirection.Left,
                paneWidth: 220,
                workWidth: 700
            );
            var sut = new NavigationPaneTransitionController();

            Assert.True(
                sut.Start(
                    fixture.Target,
                    committedWidth: 220,
                    targetWidth: 48,
                    maximumPaneWidth: 420,
                    referenceDistance: 172,
                    Duration,
                    new LinearEase(),
                    static () => { }
                )
            );
            var clock = Assert.IsAssignableFrom<ClockController>(
                sut.ActiveClockController
            );
            clock.SeekAlignedToLastTick(Duration / 2, TimeSeekOrigin.BeginTime);
            fixture.Layout();

            Assert.True(
                DependencyPropertyHelper
                    .GetValueSource(
                        fixture.Centered,
                        FrameworkElement.MaxWidthProperty
                    )
                    .IsAnimated
            );
            Assert.True(
                DependencyPropertyHelper
                    .GetValueSource(
                        fixture.ScrollableCentered,
                        FrameworkElement.MaxWidthProperty
                    )
                    .IsAnimated
            );
            Assert.Same(
                DependencyProperty.UnsetValue,
                fixture.Centered.ReadLocalValue(UIElement.RenderTransformProperty)
            );
            Assert.False(IsWidthAnimated(fixture.PaneColumn));
        });
    }

    [Fact]
    public void ProductionSource_DoesNotAnimateColumnDefinitionWidth()
    {
        var flourishRoot = Path.Combine(FindRepositoryRoot(), "src", "Flourish");
        var source = string.Join(
            Environment.NewLine,
            Directory.EnumerateFiles(flourishRoot, "*.cs", SearchOption.AllDirectories)
                .Select(File.ReadAllText)
        );

        Assert.DoesNotContain("GridLengthAnimation", source, StringComparison.Ordinal);
        Assert.DoesNotMatch(
            new Regex(
                @"(?:BeginAnimation|ApplyAnimationClock)\s*\(\s*ColumnDefinition\.WidthProperty",
                RegexOptions.CultureInvariant
            ),
            source
        );
    }

    private static void AssertClip(
        NavigationPanelDirection direction,
        double presentationWidth,
        double visibleWidth,
        Rect actual
    )
    {
        var expectedX = direction == NavigationPanelDirection.Right
            ? presentationWidth - visibleWidth
            : 0;
        AssertClose(expectedX, actual.X);
        AssertClose(visibleWidth, actual.Width);
        AssertClose(480, actual.Height);
    }

    private static bool IsWidthAnimated(ColumnDefinition column)
    {
        return DependencyPropertyHelper
            .GetValueSource(column, ColumnDefinition.WidthProperty)
            .IsAnimated;
    }

    private static bool IsCounterScaleAnimated(FrameworkElement element)
    {
        var transform = Assert.IsType<TransformGroup>(element.RenderTransform);
        var scale = Assert.IsType<ScaleTransform>(transform.Children[0]);
        return DependencyPropertyHelper
            .GetValueSource(scale, ScaleTransform.ScaleXProperty)
            .IsAnimated;
    }

    private static void AssertClose(double expected, double actual)
    {
        Assert.InRange(actual, expected - 0.001, expected + 0.001);
    }

    private static double GetCenterX(Rect bounds)
    {
        return bounds.Left + (bounds.Width / 2);
    }

    private static double Lerp(double from, double to, double progress)
    {
        return from + ((to - from) * progress);
    }

    private static Rect GetTransformedBounds(
        FrameworkElement element,
        Visual ancestor
    )
    {
        return element
            .TransformToAncestor(ancestor)
            .TransformBounds(new Rect(new Point(), element.RenderSize));
    }

    private static void PumpRender()
    {
        Dispatcher.CurrentDispatcher.Invoke(
            DispatcherPriority.Render,
            new Action(() => { })
        );
    }

    private static void AssertContentPresentationRestored(TransitionFixture fixture)
    {
        Assert.Same(fixture.OriginalContentTransform, fixture.Content.RenderTransform);
        Assert.Equal(
            fixture.OriginalContentTransformOrigin,
            fixture.Content.RenderTransformOrigin
        );
        Assert.Equal(
            fixture.OriginalContentTransformLocalValue,
            fixture.Content.ReadLocalValue(UIElement.RenderTransformProperty)
        );
        Assert.Equal(
            fixture.OriginalContentTransformOriginLocalValue,
            fixture.Content.ReadLocalValue(UIElement.RenderTransformOriginProperty)
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
            System.Runtime.ExceptionServices.ExceptionDispatchInfo.Capture(error).Throw();
        }
    }

    private static string FindRepositoryRoot()
    {
        for (
            var directory = new DirectoryInfo(AppContext.BaseDirectory);
            directory is not null;
            directory = directory.Parent
        )
        {
            if (
                File.Exists(Path.Combine(directory.FullName, "Flourish.slnx"))
                && Directory.Exists(Path.Combine(directory.FullName, "src", "Flourish"))
            )
            {
                return directory.FullName;
            }
        }

        throw new DirectoryNotFoundException("Could not locate the Flourish repository root.");
    }

    private sealed class LinearEase : EasingFunctionBase
    {
        protected override double EaseInCore(double normalizedTime)
        {
            return normalizedTime;
        }

        protected override Freezable CreateInstanceCore()
        {
            return new LinearEase();
        }
    }

    private sealed class CountingElement : FrameworkElement
    {
        internal int MeasureCount { get; private set; }

        internal int ArrangeCount { get; private set; }

        internal bool ThrowOnNextRenderTransformOriginChange { get; set; }

        internal void ResetLayoutCounts()
        {
            MeasureCount = 0;
            ArrangeCount = 0;
        }

        protected override Size MeasureOverride(Size availableSize)
        {
            MeasureCount++;
            return new Size(
                double.IsFinite(availableSize.Width) ? availableSize.Width : 0,
                double.IsFinite(availableSize.Height) ? availableSize.Height : 0
            );
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            ArrangeCount++;
            return finalSize;
        }

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);
            if (
                e.Property != RenderTransformOriginProperty
                || !ThrowOnNextRenderTransformOriginChange
            )
            {
                return;
            }

            ThrowOnNextRenderTransformOriginChange = false;
            throw new InvalidOperationException(
                "Injected render-transform-origin assignment failure."
            );
        }
    }

    private sealed class CountingGrid : Grid
    {
        internal int MeasureCount { get; private set; }

        internal int ArrangeCount { get; private set; }

        internal void ResetLayoutCounts()
        {
            MeasureCount = 0;
            ArrangeCount = 0;
        }

        protected override Size MeasureOverride(Size constraint)
        {
            MeasureCount++;
            return base.MeasureOverride(constraint);
        }

        protected override Size ArrangeOverride(Size arrangeSize)
        {
            ArrangeCount++;
            return base.ArrangeOverride(arrangeSize);
        }
    }

    private sealed class CenteredTransitionFixture
    {
        internal const double ContentMaximumWidth = 480;
        private const double ContentMargin = 32;
        private readonly Size layoutSize;

        private CenteredTransitionFixture(
            Grid workArea,
            Border pane,
            CountingGrid content,
            Border centered,
            Border scrollableCentered,
            TextBlock centeredText,
            TextBlock scrollableCenteredText,
            ColumnDefinition paneColumn,
            NavigationPanelDirection direction,
            Size layoutSize
        )
        {
            WorkArea = workArea;
            Pane = pane;
            Content = content;
            Centered = centered;
            ScrollableCentered = scrollableCentered;
            CenteredText = centeredText;
            ScrollableCenteredText = scrollableCenteredText;
            PaneColumn = paneColumn;
            Direction = direction;
            this.layoutSize = layoutSize;
        }

        internal Grid WorkArea { get; }

        internal Border Pane { get; }

        internal CountingGrid Content { get; }

        internal Border Centered { get; }

        internal Border ScrollableCentered { get; }

        internal TextBlock CenteredText { get; }

        internal TextBlock ScrollableCenteredText { get; }

        internal ColumnDefinition PaneColumn { get; }

        internal NavigationPanelDirection Direction { get; }

        internal NavigationPaneTransitionTarget Target =>
            new(WorkArea, Pane, Content, Direction, [Centered, ScrollableCentered]);

        internal static CenteredTransitionFixture Create(
            NavigationPanelDirection direction,
            double paneWidth,
            double workWidth
        )
        {
            var layoutSize = new Size(workWidth, 480);
            var workArea = new Grid { Width = workWidth, Height = layoutSize.Height };
            var paneColumn = new ColumnDefinition { Width = new GridLength(paneWidth) };
            var contentColumn = new ColumnDefinition
            {
                Width = new GridLength(1, GridUnitType.Star),
            };
            var paneColumnIndex = direction == NavigationPanelDirection.Left ? 0 : 1;
            if (paneColumnIndex == 0)
            {
                workArea.ColumnDefinitions.Add(paneColumn);
                workArea.ColumnDefinitions.Add(contentColumn);
            }
            else
            {
                workArea.ColumnDefinitions.Add(contentColumn);
                workArea.ColumnDefinitions.Add(paneColumn);
            }

            var pane = new Border();
            Grid.SetColumn(pane, paneColumnIndex);

            var centeredText = CreateTextBlock();
            var centered = new Border
            {
                Child = centeredText,
                Height = 120,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                Margin = new Thickness(ContentMargin, 0, ContentMargin, 0),
                MaxWidth = ContentMaximumWidth,
                Padding = new Thickness(24, 0, 24, 0),
                VerticalAlignment = VerticalAlignment.Top,
            };
            var scrollableCenteredText = CreateTextBlock();
            var scrollableCentered = new Border
            {
                Child = scrollableCenteredText,
                Height = 120,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                Margin = new Thickness(ContentMargin, 0, ContentMargin, 0),
                MaxWidth = ContentMaximumWidth,
                Padding = new Thickness(24, 0, 24, 0),
                VerticalAlignment = VerticalAlignment.Top,
            };
            var scrollViewer = new ScrollViewer
            {
                Content = scrollableCentered,
                HorizontalContentAlignment = HorizontalAlignment.Stretch,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
                VerticalScrollBarVisibility = ScrollBarVisibility.Disabled,
            };
            var content = new CountingGrid();
            content.RowDefinitions.Add(new RowDefinition { Height = new GridLength(140) });
            content.RowDefinitions.Add(new RowDefinition { Height = new GridLength(140) });
            content.Children.Add(centered);
            Grid.SetRow(scrollViewer, 1);
            content.Children.Add(scrollViewer);
            Grid.SetColumn(content, paneColumnIndex == 0 ? 1 : 0);

            workArea.Children.Add(pane);
            workArea.Children.Add(content);

            var fixture = new CenteredTransitionFixture(
                workArea,
                pane,
                content,
                centered,
                scrollableCentered,
                centeredText,
                scrollableCenteredText,
                paneColumn,
                direction,
                layoutSize
            );
            fixture.Layout();
            return fixture;
        }

        internal Rect GetCenteredBounds()
        {
            return Centered
                .TransformToAncestor(WorkArea)
                .TransformBounds(new Rect(new Point(), Centered.RenderSize));
        }

        internal Rect GetScrollableCenteredBounds()
        {
            return ScrollableCentered
                .TransformToAncestor(WorkArea)
                .TransformBounds(new Rect(new Point(), ScrollableCentered.RenderSize));
        }

        internal Rect GetCenteredTextBounds()
        {
            return CenteredText
                .TransformToAncestor(WorkArea)
                .TransformBounds(new Rect(new Point(), CenteredText.RenderSize));
        }

        internal Rect GetScrollableCenteredTextBounds()
        {
            return ScrollableCenteredText
                .TransformToAncestor(WorkArea)
                .TransformBounds(
                    new Rect(new Point(), ScrollableCenteredText.RenderSize)
                );
        }

        internal Rect GetExpectedCenteredBounds(double paneWidth)
        {
            var contentWidth = layoutSize.Width - paneWidth;
            var centeredWidth = Math.Min(
                ContentMaximumWidth,
                Math.Max(0, contentWidth - (ContentMargin * 2))
            );
            var contentLeft = Direction == NavigationPanelDirection.Left ? paneWidth : 0;
            return new Rect(
                contentLeft + ((contentWidth - centeredWidth) / 2),
                0,
                centeredWidth,
                Centered.ActualHeight
            );
        }

        internal void SetPaneWidth(double paneWidth)
        {
            PaneColumn.Width = new GridLength(paneWidth);
        }

        internal void Layout()
        {
            WorkArea.Measure(layoutSize);
            WorkArea.Arrange(new Rect(layoutSize));
            WorkArea.UpdateLayout();
        }

        private static TextBlock CreateTextBlock()
        {
            return new TextBlock
            {
                Text = "Centered navigation content keeps stable text metrics during the transition.",
                TextWrapping = TextWrapping.Wrap,
            };
        }
    }

    private sealed class TransitionFixture
    {
        private static readonly Size LayoutSize = new(800, 480);

        private TransitionFixture(
            Grid workArea,
            Border pane,
            CountingElement content,
            ColumnDefinition paneColumn,
            NavigationPanelDirection direction,
            int originalPaneColumn
        )
        {
            WorkArea = workArea;
            Pane = pane;
            Content = content;
            PaneColumn = paneColumn;
            Direction = direction;
            OriginalPaneColumn = originalPaneColumn;
            OriginalContentTransform = content.RenderTransform;
            OriginalContentTransformOrigin = content.RenderTransformOrigin;
            OriginalContentTransformLocalValue = content.ReadLocalValue(
                UIElement.RenderTransformProperty
            );
            OriginalContentTransformOriginLocalValue = content.ReadLocalValue(
                UIElement.RenderTransformOriginProperty
            );
        }

        internal Grid WorkArea { get; }

        internal Border Pane { get; }

        internal CountingElement Content { get; }

        internal ColumnDefinition PaneColumn { get; }

        internal NavigationPanelDirection Direction { get; }

        internal int OriginalPaneColumn { get; }

        internal Transform OriginalContentTransform { get; }

        internal Point OriginalContentTransformOrigin { get; }

        internal object OriginalContentTransformLocalValue { get; }

        internal object OriginalContentTransformOriginLocalValue { get; }

        internal NavigationPaneTransitionTarget Target =>
            new(
                WorkArea,
                Pane,
                Content,
                Direction
            );

        internal static TransitionFixture Create(
            NavigationPanelDirection direction,
            double paneWidth,
            Transform? originalContentTransform = null,
            Point? originalContentTransformOrigin = null
        )
        {
            var workArea = new Grid { Width = LayoutSize.Width, Height = LayoutSize.Height };
            var paneColumn = new ColumnDefinition { Width = new GridLength(paneWidth) };
            var contentColumn = new ColumnDefinition
            {
                Width = new GridLength(1, GridUnitType.Star),
            };
            var paneColumnIndex = direction == NavigationPanelDirection.Left ? 0 : 1;
            if (paneColumnIndex == 0)
            {
                workArea.ColumnDefinitions.Add(paneColumn);
                workArea.ColumnDefinitions.Add(contentColumn);
            }
            else
            {
                workArea.ColumnDefinitions.Add(contentColumn);
                workArea.ColumnDefinitions.Add(paneColumn);
            }

            var pane = new Border();
            Grid.SetColumn(pane, paneColumnIndex);

            var content = new CountingElement();
            if (originalContentTransform is not null)
            {
                content.RenderTransform = originalContentTransform;
            }
            if (originalContentTransformOrigin is { } transformOrigin)
            {
                content.RenderTransformOrigin = transformOrigin;
            }
            Grid.SetColumn(content, paneColumnIndex == 0 ? 1 : 0);

            workArea.Children.Add(pane);
            workArea.Children.Add(content);

            var fixture = new TransitionFixture(
                workArea,
                pane,
                content,
                paneColumn,
                direction,
                paneColumnIndex
            );
            fixture.Layout();
            return fixture;
        }

        internal void Layout()
        {
            WorkArea.Measure(LayoutSize);
            WorkArea.Arrange(new Rect(LayoutSize));
        }
    }
}
