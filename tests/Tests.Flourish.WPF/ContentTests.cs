using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using ArkheideSystem.Flourish.WPF;
using Xunit;
using F = ArkheideSystem.Flourish.WPF.Controls;

namespace ArkheideSystem.Tests.Flourish.WPF;

public class ContentTests
{
    [Fact]
    public void Framework_only_progress_uses_native_OS_motion_and_respects_inherited_reduction() => NativeTest.Run(async () =>
    {
        var progress = new F.ProgressBar { IsIndeterminate = true };
        var scope = new Grid(); FrameworkResources.Apply(scope); scope.Children.Add(progress);
        NativeTest.Layout(scope, 500, 200);
        using var connection = NativeTest.Connect(scope);
        await Dispatcher.Yield(DispatcherPriority.ApplicationIdle);
        Assert.Equal(!SystemParameters.ClientAreaAnimation, Motion.GetReduced(progress));
        var indicator = NativeTest.Part<Border>(progress, "PART_Indicator");
        Assert.Equal(SystemParameters.ClientAreaAnimation, indicator.HasAnimatedProperties);
        Motion.SetReduced(scope, true);
        await Dispatcher.Yield(DispatcherPriority.ApplicationIdle);
        Assert.True(Motion.GetReduced(progress));
        Assert.False(indicator.HasAnimatedProperties);
    });

    [Fact]
    public void Reduced_motion_removes_the_native_indeterminate_animation_clock() => NativeTest.Run(async () =>
    {
        var progress = new F.ProgressBar { IsIndeterminate = true };
        Motion.SetReduced(progress, false);
        var stage = NativeTest.Stage(progress, 500, 200);
        using var connection = NativeTest.Connect(stage);
        await Dispatcher.Yield(DispatcherPriority.ApplicationIdle);
        var indicator = NativeTest.Part<Border>(progress, "PART_Indicator");
        Assert.True(indicator.HasAnimatedProperties);
        Motion.SetReduced(progress, true);
        await Dispatcher.Yield(DispatcherPriority.ApplicationIdle);
        Assert.False(indicator.HasAnimatedProperties);
        Assert.Equal(.55, indicator.Opacity);
    });

    [Fact]
    public void Offer_stage_uses_two_active_tracks_retains_editors_and_stops_rotation_when_motion_is_reduced() => NativeTest.Run(async () =>
    {
        var editor = new F.TextBox { Text = "Retained offer draft" };
        var cards = new[] { new F.OfferCard { Id = "a", Title = "Alpha", Content = editor }, new F.OfferCard { Id = "b", Title = "Beta", Description = "Details" }, new F.OfferCard { Id = "c", Title = "Gamma" } };
        var offers = new F.OfferStage { Offers = cards, AutoRotate = false, RotationIntervalMilliseconds = 1000 };
        Motion.SetReduced(offers, false);
        var stage = NativeTest.Stage(offers, 1200, 700);
        using var connection = NativeTest.Connect(stage);
        await Dispatcher.Yield(DispatcherPriority.ApplicationIdle);
        NativeTest.Layout(stage, 1200, 700);
        Assert.True(offers.IsEnhanced); Assert.Equal(cards[1].ActualWidth * 2, cards[0].ActualWidth, 4);
        Assert.Equal(Visibility.Hidden, NativeTest.Part<Grid>(cards[1], "Details").Visibility);
        Assert.True(offers.Activate("b")); NativeTest.Layout(stage, 1200, 700);
        Assert.Same(editor, NativeTest.Descendants<F.TextBox>(cards[0]).Single()); Assert.Equal("Retained offer draft", editor.Text);
        offers.AutoRotate = true;
        await Task.Delay(1100);
        Assert.Equal("c", offers.ActiveId);
        Motion.SetReduced(offers, true); NativeTest.Layout(stage, 1200, 700);
        Assert.False(offers.IsEnhanced);
        Assert.All(cards, card => Assert.Equal(Visibility.Visible, NativeTest.Part<Grid>(card, "Details").Visibility));
        var active = offers.ActiveId; await Task.Delay(1100); Assert.Equal(active, offers.ActiveId);
    });

    [Fact]
    public void Leading_heading_sticks_without_changing_native_scroll_extent_or_layout_placeholder() => NativeTest.Run(async () =>
    {
        var heading = new F.PageHeading { Title = "Native page" };
        var content = new StackPanel(); content.Children.Add(heading);
        content.Children.Add(new F.PageBody { Content = new Border { Height = 1800 } });
        var scroll = new ScrollViewer { Content = content };
        scroll.SetResourceReference(FrameworkElement.StyleProperty, "Flourish.ScrollViewer");
        var stage = NativeTest.Stage(scroll, 1000, 600);
        using var connection = NativeTest.Connect(stage);
        await Dispatcher.Yield(DispatcherPriority.ApplicationIdle);
        Assert.True(heading.IsLoaded);
        var height = heading.DesiredSize.Height; var extent = scroll.ExtentHeight;
        Assert.Equal(174, height);
        scroll.ScrollToVerticalOffset(200); NativeTest.Layout(stage, 1000, 600);
        await Dispatcher.Yield(DispatcherPriority.ApplicationIdle);
        Assert.True(heading.EffectiveCompact);
        Assert.Equal(0, heading.TransformToAncestor(scroll).Transform(new Point()).Y, 4);
        Assert.Equal(height, heading.DesiredSize.Height); Assert.Equal(extent, scroll.ExtentHeight);
        scroll.ScrollToVerticalOffset(12); NativeTest.Layout(stage, 1000, 600);
        await Dispatcher.Yield(DispatcherPriority.ApplicationIdle);
        Assert.False(heading.EffectiveCompact);
        Assert.Equal(height, heading.DesiredSize.Height); Assert.Equal(extent, scroll.ExtentHeight);
    });

    [Fact]
    public void Heading_inside_a_section_remains_in_ordinary_document_flow() => NativeTest.Run(async () =>
    {
        var heading = new F.PageHeading { Title = "Nested example" };
        var content = new StackPanel(); content.Children.Add(new F.Section { Content = heading }); content.Children.Add(new Border { Height = 1800 });
        var scroll = new ScrollViewer { Content = new F.PageBody { Content = content } };
        scroll.SetResourceReference(FrameworkElement.StyleProperty, "Flourish.ScrollViewer");
        var stage = NativeTest.Stage(scroll, 1000, 600);
        using var connection = NativeTest.Connect(stage);
        await Dispatcher.Yield(DispatcherPriority.ApplicationIdle);
        scroll.ScrollToVerticalOffset(200); NativeTest.Layout(stage, 1000, 600);
        await Dispatcher.Yield(DispatcherPriority.ApplicationIdle);
        Assert.False(heading.EffectiveCompact);
        Assert.True(heading.RenderTransform.Value.IsIdentity);
    });
}
