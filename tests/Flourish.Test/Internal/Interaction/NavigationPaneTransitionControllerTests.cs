using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
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
            AssertClip(direction, 420, committedWidth, clip.Rect);
            AssertClose(1, fixture.ContentScale.ScaleX);
            AssertClose(0, fixture.ContentTranslation.X);

            clock.SeekAlignedToLastTick(Duration / 2, TimeSeekOrigin.BeginTime);

            var midpointWidth = (committedWidth + targetWidth) / 2;
            var targetScale =
                (fixture.Content.ActualWidth + committedWidth - targetWidth)
                / fixture.Content.ActualWidth;
            var targetTranslation = direction == NavigationPanelDirection.Left
                ? targetWidth - committedWidth
                : 0;

            AssertClip(direction, 420, midpointWidth, clip.Rect);
            AssertClose((1 + targetScale) / 2, fixture.ContentScale.ScaleX);
            AssertClose(targetTranslation / 2, fixture.ContentTranslation.X);
            Assert.False(IsWidthAnimated(fixture.PaneColumn));
            Assert.Equal(0, completionCount);
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
            AssertClose(1, fixture.ContentScale.ScaleX);
            AssertClose(0, fixture.ContentTranslation.X);
            Assert.False(animatedClip.HasAnimatedProperties);
            Assert.False(fixture.ContentScale.HasAnimatedProperties);
            Assert.False(fixture.ContentTranslation.HasAnimatedProperties);
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
            var scaleBeforeReverse = fixture.ContentScale.ScaleX;
            var translationBeforeReverse = fixture.ContentTranslation.X;

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
            AssertClose(scaleBeforeReverse, fixture.ContentScale.ScaleX);
            AssertClose(translationBeforeReverse, fixture.ContentTranslation.X);
            Assert.Equal(0, firstCompletionCount);
            Assert.Equal(0, reverseCompletionCount);
            Assert.False(IsWidthAnimated(fixture.PaneColumn));

            reverseClock.SeekAlignedToLastTick(Duration / 2, TimeSeekOrigin.BeginTime);

            Assert.Equal(0, firstCompletionCount);
            Assert.Equal(1, reverseCompletionCount);
            Assert.False(sut.IsActive);
            Assert.Null(fixture.Pane.Clip);
            AssertClose(1, fixture.ContentScale.ScaleX);
            AssertClose(0, fixture.ContentTranslation.X);
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
            AssertClose(1, fixture.ContentScale.ScaleX);
            AssertClose(0, fixture.ContentTranslation.X);
            Assert.False(animatedClip.HasAnimatedProperties);
            Assert.False(fixture.ContentScale.HasAnimatedProperties);
            Assert.False(fixture.ContentTranslation.HasAnimatedProperties);
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
            AssertClose(1, fixture.ContentScale.ScaleX);
            AssertClose(0, fixture.ContentTranslation.X);
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

    private static void AssertClose(double expected, double actual)
    {
        Assert.InRange(actual, expected - 0.001, expected + 0.001);
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
    }

    private sealed class TransitionFixture
    {
        private static readonly Size LayoutSize = new(800, 480);

        private TransitionFixture(
            Grid workArea,
            Border pane,
            CountingElement content,
            ColumnDefinition paneColumn,
            ScaleTransform contentScale,
            TranslateTransform contentTranslation,
            NavigationPanelDirection direction,
            int originalPaneColumn
        )
        {
            WorkArea = workArea;
            Pane = pane;
            Content = content;
            PaneColumn = paneColumn;
            ContentScale = contentScale;
            ContentTranslation = contentTranslation;
            Direction = direction;
            OriginalPaneColumn = originalPaneColumn;
        }

        internal Grid WorkArea { get; }

        internal Border Pane { get; }

        internal CountingElement Content { get; }

        internal ColumnDefinition PaneColumn { get; }

        internal ScaleTransform ContentScale { get; }

        internal TranslateTransform ContentTranslation { get; }

        internal NavigationPanelDirection Direction { get; }

        internal int OriginalPaneColumn { get; }

        internal NavigationPaneTransitionTarget Target =>
            new(
                WorkArea,
                Pane,
                Content,
                ContentScale,
                ContentTranslation,
                Direction
            );

        internal static TransitionFixture Create(
            NavigationPanelDirection direction,
            double paneWidth
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

            var contentScale = new ScaleTransform(1, 1);
            var contentTranslation = new TranslateTransform();
            var transforms = new TransformGroup();
            transforms.Children.Add(contentScale);
            transforms.Children.Add(contentTranslation);
            var content = new CountingElement { RenderTransform = transforms };
            Grid.SetColumn(content, paneColumnIndex == 0 ? 1 : 0);

            workArea.Children.Add(pane);
            workArea.Children.Add(content);

            var fixture = new TransitionFixture(
                workArea,
                pane,
                content,
                paneColumn,
                contentScale,
                contentTranslation,
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
