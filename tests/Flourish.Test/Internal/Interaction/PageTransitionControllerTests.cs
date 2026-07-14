using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Xml.Linq;
using ArkheideSystem.Flourish.Abstract;
using ArkheideSystem.Flourish.Internal.Configuration;
using ArkheideSystem.Flourish.Internal.Interaction;
using ArkheideSystem.Flourish.Services;

namespace ArkheideSystem.Flourish.Test.Internal.Interaction;

public sealed class PageTransitionControllerTests
{
    private const double ConfiguredOpacity = 0.8;
    private static readonly TimeSpan Duration = TimeSpan.FromMilliseconds(200);
    private static readonly XNamespace Xaml =
        "http://schemas.microsoft.com/winfx/2006/xaml";
    private static readonly string RepositoryRoot = FindRepositoryRoot();

    [Theory]
    [InlineData(FlourishPageTransition.Fade)]
    [InlineData(FlourishPageTransition.EntranceFromBottom)]
    public void Transition_AnimatesCachedPresenterWithoutRelayout(
        FlourishPageTransition transition
    )
    {
        RunInSta(() =>
        {
            var fixture = TransitionFixture.Create();
            var sut = new PageTransitionController();
            var completionCount = 0;

            fixture.Layout();
            fixture.Content.ResetLayoutCounts();
            Assert.True(
                sut.Start(
                    fixture.Target,
                    transition,
                    Duration,
                    new LinearEase(),
                    () => completionCount++
                )
            );

            var transitionCache = AssertTransitionCache(fixture);
            Assert.NotSame(fixture.OriginalCacheMode, transitionCache);
            var translation = transition == FlourishPageTransition.EntranceFromBottom
                ? Assert.IsType<TranslateTransform>(fixture.Presenter.RenderTransform)
                : null;
            if (translation is null)
            {
                Assert.Same(
                    fixture.OriginalRenderTransformLocalValue,
                    fixture.Presenter.ReadLocalValue(UIElement.RenderTransformProperty)
                );
            }

            var clock = sut.ActiveClockController;
            Assert.NotNull(clock);
            clock!.SeekAlignedToLastTick(TimeSpan.Zero, TimeSeekOrigin.BeginTime);
            fixture.Layout();

            AssertClose(0, fixture.Presenter.Opacity);
            Assert.True(IsAnimated(fixture.Presenter, UIElement.OpacityProperty));
            if (translation is not null)
            {
                AssertClose(14, translation.Y);
                Assert.True(IsAnimated(translation, TranslateTransform.YProperty));
            }

            AssertContentPresentationUnchanged(fixture);

            clock.SeekAlignedToLastTick(Duration / 2, TimeSeekOrigin.BeginTime);
            fixture.Layout();

            AssertClose(fixture.OriginalOpacity / 2, fixture.Presenter.Opacity);
            if (translation is not null)
            {
                AssertClose(7, translation.Y);
            }

            Assert.Equal(0, fixture.Content.MeasureCount);
            Assert.Equal(0, fixture.Content.ArrangeCount);
            AssertContentPresentationUnchanged(fixture);

            clock.SeekAlignedToLastTick(Duration, TimeSeekOrigin.BeginTime);

            Assert.Equal(1, completionCount);
            Assert.False(sut.IsActive);
            Assert.Null(sut.ActiveClockController);
            AssertPresenterRestored(fixture);
            AssertContentPresentationUnchanged(fixture);
            if (translation is not null)
            {
                AssertClose(0, translation.Y);
                Assert.False(IsAnimated(translation, TranslateTransform.YProperty));
            }
        });
    }

    [Fact]
    public void Cancel_DropsTheCallbackAndRestoresOriginalPresentation()
    {
        RunInSta(() =>
        {
            var fixture = TransitionFixture.Create(
                withOriginalOpacity: true,
                withOriginalCache: true,
                withOriginalRenderTransform: true
            );
            var sut = new PageTransitionController();
            var completionCount = 0;

            Assert.True(
                sut.Start(
                    fixture.Target,
                    FlourishPageTransition.EntranceFromBottom,
                    Duration,
                    new LinearEase(),
                    () => completionCount++
                )
            );
            var translation = Assert.IsType<TranslateTransform>(
                fixture.Presenter.RenderTransform
            );
            sut.ActiveClockController!.SeekAlignedToLastTick(
                Duration / 2,
                TimeSeekOrigin.BeginTime
            );

            sut.Cancel();

            Assert.Equal(0, completionCount);
            Assert.False(sut.IsActive);
            Assert.Null(sut.ActiveClockController);
            AssertPresenterRestored(fixture);
            AssertClose(0, translation.Y);
            Assert.False(IsAnimated(translation, TranslateTransform.YProperty));
        });
    }

    [Fact]
    public void ConsecutiveNavigation_DropsTheStaleRunAndCompletesOnlyTheLatestRun()
    {
        RunInSta(() =>
        {
            var fixture = TransitionFixture.Create();
            var sut = new PageTransitionController();
            var firstCompletionCount = 0;
            var secondCompletionCount = 0;

            Assert.True(
                sut.Start(
                    fixture.Target,
                    FlourishPageTransition.Fade,
                    Duration,
                    new LinearEase(),
                    () => firstCompletionCount++
                )
            );
            var firstClock = sut.ActiveClockController;
            var firstCache = AssertTransitionCache(fixture);
            firstClock!.SeekAlignedToLastTick(Duration / 2, TimeSeekOrigin.BeginTime);

            Assert.True(
                sut.Start(
                    fixture.Target,
                    FlourishPageTransition.EntranceFromBottom,
                    Duration,
                    new LinearEase(),
                    () => secondCompletionCount++
                )
            );
            var secondClock = sut.ActiveClockController;
            var secondCache = AssertTransitionCache(fixture);

            Assert.NotNull(secondClock);
            Assert.NotSame(firstClock, secondClock);
            Assert.NotSame(firstCache, secondCache);
            Assert.Equal(0, firstCompletionCount);
            Assert.Equal(0, secondCompletionCount);
            Assert.True(sut.IsActive);
            secondClock!.SeekAlignedToLastTick(Duration, TimeSeekOrigin.BeginTime);

            Assert.Equal(0, firstCompletionCount);
            Assert.Equal(1, secondCompletionCount);
            Assert.False(sut.IsActive);
            AssertPresenterRestored(fixture);
        });
    }

    [Fact]
    public void InvalidTransition_CancelsTheActiveRunAndRestoresPresentation()
    {
        RunInSta(() =>
        {
            var fixture = TransitionFixture.Create(withOriginalCache: true);
            var sut = new PageTransitionController();
            var completionCount = 0;

            Assert.True(
                sut.Start(
                    fixture.Target,
                    FlourishPageTransition.EntranceFromBottom,
                    Duration,
                    new LinearEase(),
                    () => completionCount++
                )
            );

            Assert.False(
                sut.Start(
                    fixture.Target,
                    FlourishPageTransition.None,
                    Duration,
                    new LinearEase(),
                    () => completionCount++
                )
            );

            Assert.Equal(0, completionCount);
            Assert.False(sut.IsActive);
            AssertPresenterRestored(fixture);
        });
    }

    [Fact]
    public void Start_WhenInstallingTheTemporaryTransformThrows_RestoresPresentation()
    {
        RunInSta(() =>
        {
            var presenter = new ThrowOncePresenter();
            var fixture = TransitionFixture.Create(
                withOriginalCache: true,
                presenter: presenter
            );
            var sut = new PageTransitionController();
            var completionCount = 0;
            presenter.ThrowOnNextOpacityCoercion = true;

            Assert.Throws<InvalidOperationException>(() =>
                sut.Start(
                    fixture.Target,
                    FlourishPageTransition.EntranceFromBottom,
                    Duration,
                    new LinearEase(),
                    () => completionCount++
                )
            );

            Assert.Equal(0, completionCount);
            Assert.False(sut.IsActive);
            Assert.Null(sut.ActiveClockController);
            AssertPresenterRestored(fixture);
        });
    }

    [Fact]
    public void Completion_PreservesExistingBindings()
    {
        RunInSta(() =>
        {
            var source = new TransitionBindingSource
            {
                Opacity = 0.65,
                CacheMode = new BitmapCache(1.5),
                RenderTransform = new ScaleTransform(1.05, 0.95),
            };
            var presenter = new Grid();
            BindingOperations.SetBinding(
                presenter,
                UIElement.OpacityProperty,
                new Binding(nameof(TransitionBindingSource.Opacity)) { Source = source }
            );
            BindingOperations.SetBinding(
                presenter,
                UIElement.CacheModeProperty,
                new Binding(nameof(TransitionBindingSource.CacheMode)) { Source = source }
            );
            BindingOperations.SetBinding(
                presenter,
                UIElement.RenderTransformProperty,
                new Binding(nameof(TransitionBindingSource.RenderTransform)) { Source = source }
            );
            var fixture = TransitionFixture.Create(presenter: presenter);
            var sut = new PageTransitionController();

            Assert.True(
                sut.Start(
                    fixture.Target,
                    FlourishPageTransition.EntranceFromBottom,
                    Duration,
                    new LinearEase(),
                    static () => { }
                )
            );
            sut.ActiveClockController!.SeekAlignedToLastTick(
                Duration,
                TimeSeekOrigin.BeginTime
            );

            AssertPresenterRestored(fixture);
            Assert.True(BindingOperations.IsDataBound(presenter, UIElement.OpacityProperty));
            Assert.True(BindingOperations.IsDataBound(presenter, UIElement.CacheModeProperty));
            Assert.True(
                BindingOperations.IsDataBound(
                    presenter,
                    UIElement.RenderTransformProperty
                )
            );
            AssertClose(source.Opacity, presenter.Opacity);
            Assert.Same(source.CacheMode, presenter.CacheMode);
            Assert.Same(source.RenderTransform, presenter.RenderTransform);
        });
    }

    [Fact]
    public void ShellXaml_DefinesTheCachedPagePresenterContract()
    {
        var document = XDocument.Load(
            Path.Combine(
                RepositoryRoot,
                "src",
                "Flourish",
                "Views",
                "Windows",
                "FlourishShellWindow.xaml"
            )
        );
        var host = FindNamedElement(document, "ContentFrameHost");
        var presenter = FindNamedElement(document, "PageTransitionContentHost");
        var rootFrame = FindNamedElement(document, "RootFrame");
        var overlay = FindNamedElement(document, "ContentOverlayRegionHost");
        var hostChildren = host.Elements().ToArray();
        var presenterIndex = Array.IndexOf(hostChildren, presenter);
        var overlayIndex = Array.IndexOf(hostChildren, overlay);

        Assert.Equal("True", (string?)host.Attribute("ClipToBounds"));
        Assert.Null(host.Attribute("Background"));
        Assert.InRange(presenterIndex, 0, overlayIndex - 1);
        Assert.Contains(rootFrame, presenter.Descendants());
        Assert.Null(presenter.Attribute("Background"));
        Assert.Null(presenter.Attribute("RenderOptions.ClearTypeHint"));
        Assert.Null(presenter.Attribute("RenderTransform"));
        Assert.DoesNotContain(
            presenter.Elements(),
            element => element.Name.LocalName.EndsWith(".RenderTransform", StringComparison.Ordinal)
        );
        Assert.DoesNotContain(
            document.Descendants(),
            element => (string?)element.Attribute(Xaml + "Name") == "PageTransitionChrome"
        );
    }

    [Theory]
    [InlineData(false, FlourishPageTransition.Fade, false, true)]
    [InlineData(true, FlourishPageTransition.None, false, true)]
    [InlineData(true, FlourishPageTransition.Fade, true, false)]
    public void MotionService_NonAnimatedPoliciesCancelAnActiveTransition(
        bool enabled,
        FlourishPageTransition transition,
        bool respectReducedMotion,
        bool systemAnimationsEnabled
    )
    {
        RunInSta(() =>
        {
            var fixture = TransitionFixture.Create(withOriginalCache: true);
            var controller = new PageTransitionController();
            Assert.True(
                controller.Start(
                    fixture.Target,
                    FlourishPageTransition.EntranceFromBottom,
                    Duration,
                    new LinearEase(),
                    static () => { }
                )
            );

            var options = new FlourishShellOptions();
            options.Motion.IsEnabled = enabled;
            options.Motion.PageTransition = transition;
            options.Motion.RespectSystemReducedMotion = respectReducedMotion;
            var sut = new FlourishMotionService(options, () => systemAnimationsEnabled);

            sut.AnimatePageEntrance(controller, fixture.Target);

            Assert.False(controller.IsActive);
            Assert.Null(controller.ActiveClockController);
            AssertPresenterRestored(fixture);
        });
    }

    [Theory]
    [InlineData(FlourishPageTransition.Fade)]
    [InlineData(FlourishPageTransition.EntranceFromBottom)]
    public void MotionService_StartsTheConfiguredPresenterTransition(
        FlourishPageTransition transition
    )
    {
        RunInSta(() =>
        {
            var fixture = TransitionFixture.Create();
            var options = new FlourishShellOptions();
            options.Motion.IsEnabled = true;
            options.Motion.PageTransition = transition;
            options.Motion.PageTransitionDuration = Duration;
            options.Motion.RespectSystemReducedMotion = false;
            var sut = new FlourishMotionService(options, static () => false);
            var controller = new PageTransitionController();

            sut.AnimatePageEntrance(controller, fixture.Target);

            Assert.True(controller.IsActive);
            Assert.NotNull(controller.ActiveClockController);
            AssertTransitionCache(fixture);
            controller.Cancel();
            AssertPresenterRestored(fixture);
        });
    }

    private static BitmapCache AssertTransitionCache(TransitionFixture fixture)
    {
        var cache = Assert.IsType<BitmapCache>(fixture.Presenter.CacheMode);
        Assert.False(cache.EnableClearType);
        Assert.False(cache.SnapsToDevicePixels);
        return cache;
    }

    private static void AssertPresenterRestored(TransitionFixture fixture)
    {
        AssertClose(fixture.OriginalOpacity, fixture.Presenter.Opacity);
        Assert.Same(fixture.OriginalCacheMode, fixture.Presenter.CacheMode);
        Assert.Same(
            fixture.OriginalOpacityLocalValue,
            fixture.Presenter.ReadLocalValue(UIElement.OpacityProperty)
        );
        Assert.Same(
            fixture.OriginalCacheModeLocalValue,
            fixture.Presenter.ReadLocalValue(UIElement.CacheModeProperty)
        );
        Assert.Same(
            fixture.OriginalRenderTransformLocalValue,
            fixture.Presenter.ReadLocalValue(UIElement.RenderTransformProperty)
        );
        Assert.Equal(
            fixture.OriginalRenderTransformOrigin,
            fixture.Presenter.RenderTransformOrigin
        );
        Assert.False(IsAnimated(fixture.Presenter, UIElement.OpacityProperty));
    }

    private static XElement FindNamedElement(XDocument document, string name)
    {
        return Assert.Single(
            document.Descendants(),
            element => (string?)element.Attribute(Xaml + "Name") == name
        );
    }

    private static void AssertContentPresentationUnchanged(TransitionFixture fixture)
    {
        AssertClose(1, fixture.Content.Opacity);
        Assert.Same(fixture.ContentTransform, fixture.Content.RenderTransform);
        Assert.False(fixture.Content.HasAnimatedProperties);
        Assert.False(IsAnimated(fixture.Content, UIElement.OpacityProperty));
        Assert.False(IsAnimated(fixture.Content, UIElement.RenderTransformProperty));
    }

    private static bool IsAnimated(DependencyObject owner, DependencyProperty property)
    {
        return DependencyPropertyHelper.GetValueSource(owner, property).IsAnimated;
    }

    private static void AssertClose(double expected, double actual)
    {
        Assert.InRange(actual, expected - 0.0001, expected + 0.0001);
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

        throw new DirectoryNotFoundException(
            $"Could not locate the Flourish repository above {AppContext.BaseDirectory}."
        );
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

    private sealed class TransitionFixture
    {
        private TransitionFixture(
            Grid host,
            Grid presenter,
            CountingElement content,
            Transform contentTransform,
            double originalOpacity,
            object originalOpacityLocalValue,
            CacheMode? originalCacheMode,
            object originalCacheModeLocalValue,
            object originalRenderTransformLocalValue,
            Point originalRenderTransformOrigin
        )
        {
            Host = host;
            Presenter = presenter;
            Content = content;
            ContentTransform = contentTransform;
            OriginalOpacity = originalOpacity;
            OriginalOpacityLocalValue = originalOpacityLocalValue;
            OriginalCacheMode = originalCacheMode;
            OriginalCacheModeLocalValue = originalCacheModeLocalValue;
            OriginalRenderTransformLocalValue = originalRenderTransformLocalValue;
            OriginalRenderTransformOrigin = originalRenderTransformOrigin;
        }

        internal Grid Host { get; }

        internal Grid Presenter { get; }

        internal CountingElement Content { get; }

        internal Transform ContentTransform { get; }

        internal double OriginalOpacity { get; }

        internal object OriginalOpacityLocalValue { get; }

        internal CacheMode? OriginalCacheMode { get; }

        internal object OriginalCacheModeLocalValue { get; }

        internal object OriginalRenderTransformLocalValue { get; }

        internal Point OriginalRenderTransformOrigin { get; }

        internal PageTransitionTarget Target => new(Presenter);

        internal static TransitionFixture Create(
            bool withOriginalOpacity = false,
            bool withOriginalCache = false,
            bool withOriginalRenderTransform = false,
            Grid? presenter = null
        )
        {
            var host = new Grid { Width = 480, Height = 320 };
            var originalCache = withOriginalCache
                ? new BitmapCache(1.25)
                : null;
            presenter ??= new Grid();
            if (withOriginalOpacity)
            {
                presenter.Opacity = ConfiguredOpacity;
            }

            if (originalCache is not null)
            {
                presenter.CacheMode = originalCache;
            }

            if (withOriginalRenderTransform)
            {
                presenter.RenderTransform = new ScaleTransform(1.1, 0.9);
                presenter.RenderTransformOrigin = new Point(0.25, 0.75);
            }

            var contentTransform = new TransformGroup();
            contentTransform.Children.Add(new TranslateTransform(3, 4));
            var content = new CountingElement { RenderTransform = contentTransform };
            presenter.Children.Add(content);
            host.Children.Add(presenter);
            return new TransitionFixture(
                host,
                presenter,
                content,
                contentTransform,
                presenter.Opacity,
                presenter.ReadLocalValue(UIElement.OpacityProperty),
                presenter.CacheMode,
                presenter.ReadLocalValue(UIElement.CacheModeProperty),
                presenter.ReadLocalValue(UIElement.RenderTransformProperty),
                presenter.RenderTransformOrigin
            );
        }

        internal void Layout()
        {
            Host.Measure(new Size(480, 320));
            Host.Arrange(new Rect(0, 0, 480, 320));
            Host.UpdateLayout();
        }
    }

    private sealed class ThrowOncePresenter : Grid
    {
        static ThrowOncePresenter()
        {
            UIElement.OpacityProperty.OverrideMetadata(
                typeof(ThrowOncePresenter),
                new UIPropertyMetadata(1d, null, CoerceOpacity)
            );
        }

        internal bool ThrowOnNextOpacityCoercion { get; set; }

        private static object CoerceOpacity(DependencyObject dependencyObject, object baseValue)
        {
            var presenter = (ThrowOncePresenter)dependencyObject;
            if (presenter.ThrowOnNextOpacityCoercion)
            {
                presenter.ThrowOnNextOpacityCoercion = false;
                throw new InvalidOperationException("Test opacity-animation failure.");
            }

            return baseValue;
        }
    }

    private sealed class TransitionBindingSource
    {
        public double Opacity { get; init; }

        public CacheMode? CacheMode { get; init; }

        public Transform? RenderTransform { get; init; }
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
            return new Size(200, 120);
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            ArrangeCount++;
            return finalSize;
        }
    }
}
