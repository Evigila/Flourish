using System.IO;
using System.Windows;
using System.Windows.Controls;
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
    private static readonly TimeSpan Duration = TimeSpan.FromMilliseconds(200);
    private static readonly XNamespace Presentation =
        "http://schemas.microsoft.com/winfx/2006/xaml/presentation";
    private static readonly XNamespace Xaml =
        "http://schemas.microsoft.com/winfx/2006/xaml";
    private static readonly string RepositoryRoot = FindRepositoryRoot();

    [Theory]
    [InlineData(FlourishPageTransition.Fade)]
    [InlineData(FlourishPageTransition.EntranceFromBottom)]
    public void Transition_AnimatesOnlyTheTextFreeChrome(
        FlourishPageTransition transition
    )
    {
        RunInSta(() =>
        {
            var fixture = TransitionFixture.Create();
            var contentTransform = fixture.Content.RenderTransform;
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

            var clock = sut.ActiveClockController;
            Assert.NotNull(clock);
            clock!.SeekAlignedToLastTick(TimeSpan.Zero, TimeSeekOrigin.BeginTime);
            fixture.Layout();

            AssertContentPresentationUnchanged(fixture, contentTransform);
            AssertClose(1, fixture.Chrome.Opacity);
            AssertClose(1, fixture.ChromeScale.ScaleY);
            Assert.Equal(
                transition == FlourishPageTransition.Fade,
                IsAnimated(fixture.Chrome, UIElement.OpacityProperty)
            );
            Assert.Equal(
                transition == FlourishPageTransition.EntranceFromBottom,
                IsAnimated(fixture.ChromeScale, ScaleTransform.ScaleYProperty)
            );

            clock.SeekAlignedToLastTick(Duration / 2, TimeSeekOrigin.BeginTime);
            fixture.Layout();

            AssertContentPresentationUnchanged(fixture, contentTransform);
            if (transition == FlourishPageTransition.Fade)
            {
                AssertClose(0.5, fixture.Chrome.Opacity);
                AssertClose(1, fixture.ChromeScale.ScaleY);
            }
            else
            {
                AssertClose(1, fixture.Chrome.Opacity);
                AssertClose(0.5, fixture.ChromeScale.ScaleY);
            }

            Assert.Equal(0, fixture.Content.MeasureCount);
            Assert.Equal(0, fixture.Content.ArrangeCount);

            clock.SeekAlignedToLastTick(Duration, TimeSeekOrigin.BeginTime);

            Assert.Equal(1, completionCount);
            Assert.False(sut.IsActive);
            AssertChromeIdle(fixture);
            AssertContentPresentationUnchanged(fixture, contentTransform);
        });
    }

    [Fact]
    public void Cancel_DropsTheCallbackAndRestoresIdleChrome()
    {
        RunInSta(() =>
        {
            var fixture = TransitionFixture.Create();
            var contentTransform = fixture.Content.RenderTransform;
            var sut = new PageTransitionController();
            var completionCount = 0;

            Assert.True(
                sut.Start(
                    fixture.Target,
                    FlourishPageTransition.Fade,
                    Duration,
                    new LinearEase(),
                    () => completionCount++
                )
            );
            sut.ActiveClockController!.SeekAlignedToLastTick(
                Duration / 2,
                TimeSeekOrigin.BeginTime
            );

            sut.Cancel();

            Assert.Equal(0, completionCount);
            Assert.False(sut.IsActive);
            AssertChromeIdle(fixture);
            AssertContentPresentationUnchanged(fixture, contentTransform);
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

            Assert.NotNull(secondClock);
            Assert.NotSame(firstClock, secondClock);
            Assert.Equal(0, firstCompletionCount);
            Assert.Equal(0, secondCompletionCount);
            Assert.True(sut.IsActive);
            secondClock!.SeekAlignedToLastTick(Duration, TimeSeekOrigin.BeginTime);

            Assert.Equal(0, firstCompletionCount);
            Assert.Equal(1, secondCompletionCount);
            Assert.False(sut.IsActive);
            AssertChromeIdle(fixture);
        });
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
            var fixture = TransitionFixture.Create();
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
            AssertChromeIdle(fixture);
        });
    }

    [Theory]
    [InlineData(FlourishPageTransition.Fade)]
    [InlineData(FlourishPageTransition.EntranceFromBottom)]
    public void MotionService_StartsTheConfiguredChromeTransition(
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
            AssertClose(1, fixture.Content.Opacity);
            Assert.False(fixture.Content.HasAnimatedProperties);
            controller.Cancel();
            AssertChromeIdle(fixture);
        });
    }

    [Fact]
    public void ShellXaml_KeepsTheChromeBetweenThePageAndContentOverlay()
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
        var children = host.Elements().ToArray();
        var rootFrameIndex = Array.FindIndex(
            children,
            element => (string?)element.Attribute(Xaml + "Name") == "RootFrame"
        );
        var chromeIndex = Array.FindIndex(
            children,
            element => (string?)element.Attribute(Xaml + "Name") == "PageTransitionChrome"
        );
        var overlayIndex = Array.FindIndex(
            children,
            element =>
                (string?)element.Attribute(Xaml + "Name")
                == "ContentOverlayRegionHost"
        );
        var chrome = children[chromeIndex];
        var scale = Assert.Single(chrome.Descendants(Presentation + "ScaleTransform"));

        Assert.Equal("True", (string?)host.Attribute("ClipToBounds"));
        Assert.InRange(chromeIndex, rootFrameIndex + 1, overlayIndex - 1);
        Assert.Equal("False", (string?)chrome.Attribute("IsHitTestVisible"));
        Assert.Equal("0", (string?)chrome.Attribute("Opacity"));
        Assert.Equal("5", (string?)chrome.Attribute("Panel.ZIndex"));
        Assert.Equal("0.5,0", (string?)chrome.Attribute("RenderTransformOrigin"));
        Assert.Equal(
            "{DynamicResource FlourishPageTransitionBackgroundBrush}",
            (string?)chrome.Attribute("Background")
        );
        Assert.Equal("PageTransitionScaleTransform", (string?)scale.Attribute(Xaml + "Name"));
        Assert.Equal("1", (string?)scale.Attribute("ScaleX"));
        Assert.Equal("0", (string?)scale.Attribute("ScaleY"));
    }

    [Theory]
    [InlineData("Colors.Light.xaml", "#FFF5F7FA")]
    [InlineData("Colors.Dark.xaml", "#FF141414")]
    public void ThemePalette_ProvidesAnOpaqueTransitionBackground(
        string fileName,
        string expectedColor
    )
    {
        var document = XDocument.Load(
            Path.Combine(
                RepositoryRoot,
                "src",
                "Flourish",
                "Themes",
                "Colors",
                fileName
            )
        );
        var brush = Assert.Single(
            document.Descendants(Presentation + "SolidColorBrush"),
            element =>
                (string?)element.Attribute(Xaml + "Key")
                == "FlourishPageTransitionBackgroundBrush"
        );

        Assert.Equal(expectedColor, (string?)brush.Attribute("Color"));
        Assert.Equal("FF", expectedColor.Substring(1, 2));
    }

    [Fact]
    public void MotionService_DoesNotAnimateThePageVisual()
    {
        var source = File.ReadAllText(
            Path.Combine(
                RepositoryRoot,
                "src",
                "Flourish",
                "Services",
                "FlourishMotionService.cs"
            )
        );

        Assert.DoesNotContain("PageEntranceOffset", source, StringComparison.Ordinal);
        Assert.DoesNotContain("EnsureTranslateTransform", source, StringComparison.Ordinal);
        Assert.DoesNotContain(
            "AnimatePageEntrance(FrameworkElement",
            source,
            StringComparison.Ordinal
        );
    }

    private static XElement FindNamedElement(XDocument document, string name)
    {
        return Assert.Single(
            document.Descendants(),
            element => (string?)element.Attribute(Xaml + "Name") == name
        );
    }

    private static void AssertContentPresentationUnchanged(
        TransitionFixture fixture,
        Transform originalTransform
    )
    {
        AssertClose(1, fixture.Content.Opacity);
        Assert.Same(originalTransform, fixture.Content.RenderTransform);
        Assert.False(fixture.Content.HasAnimatedProperties);
        Assert.False(IsAnimated(fixture.Content, UIElement.OpacityProperty));
        Assert.False(
            IsAnimated(fixture.Content, UIElement.RenderTransformProperty)
        );
    }

    private static void AssertChromeIdle(TransitionFixture fixture)
    {
        AssertClose(0, fixture.Chrome.Opacity);
        AssertClose(0, fixture.ChromeScale.ScaleY);
        Assert.False(IsAnimated(fixture.Chrome, UIElement.OpacityProperty));
        Assert.False(IsAnimated(fixture.ChromeScale, ScaleTransform.ScaleYProperty));
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
            CountingElement content,
            Border chrome,
            ScaleTransform chromeScale
        )
        {
            Host = host;
            Content = content;
            Chrome = chrome;
            ChromeScale = chromeScale;
        }

        internal Grid Host { get; }

        internal CountingElement Content { get; }

        internal Border Chrome { get; }

        internal ScaleTransform ChromeScale { get; }

        internal PageTransitionTarget Target => new(Content, Chrome, ChromeScale);

        internal static TransitionFixture Create()
        {
            var host = new Grid { Width = 480, Height = 320 };
            var contentTransform = new TransformGroup();
            contentTransform.Children.Add(new TranslateTransform(3, 4));
            var content = new CountingElement { RenderTransform = contentTransform };
            var chromeScale = new ScaleTransform(1, 0);
            var chrome = new Border
            {
                IsHitTestVisible = false,
                Opacity = 0,
                RenderTransform = chromeScale,
                RenderTransformOrigin = new Point(0.5, 0),
            };
            host.Children.Add(content);
            host.Children.Add(chrome);
            return new TransitionFixture(host, content, chrome, chromeScale);
        }

        internal void Layout()
        {
            Host.Measure(new Size(480, 320));
            Host.Arrange(new Rect(0, 0, 480, 320));
            Host.UpdateLayout();
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
            return new Size(200, 120);
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            ArrangeCount++;
            return finalSize;
        }
    }
}
