using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Threading;
using ArkheideSystem.Flourish.Controls;
using ArkheideSystem.Flourish.Internal.Interaction;

namespace ArkheideSystem.Flourish.Test.Controls;

public sealed class HoverRevealVisualTests
{
    private const string GenericThemeSource =
        "/Flourish;component/Themes/Generic.xaml";

    [Fact]
    public void Animator_ResetAndStaticRevealPreserveExplicitVisualStates()
    {
        RunInSta(() =>
        {
            const string templateXaml =
                """
                <ControlTemplate
                  xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                  xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
                  TargetType="{x:Type Button}"
                >
                  <Border x:Name="HoverChrome" Opacity="0">
                    <Border.RenderTransform>
                      <ScaleTransform x:Name="HoverRevealScale" ScaleX="0" ScaleY="0" />
                    </Border.RenderTransform>
                  </Border>
                  <ControlTemplate.Triggers>
                    <Trigger Property="Tag" Value="Pressed">
                      <Setter TargetName="HoverChrome" Property="Opacity" Value="1" />
                    </Trigger>
                  </ControlTemplate.Triggers>
                </ControlTemplate>
                """;
            var button = new Button
            {
                Template = Assert.IsType<ControlTemplate>(XamlReader.Parse(templateXaml)),
            };
            var window = CreateWindow(new ResourceDictionary(), button);

            try
            {
                window.Show();
                window.UpdateLayout();
                button.ApplyTemplate();
                var hoverChrome = AssertTemplatePart<Border>(button, "HoverChrome");
                var revealScale = AssertTemplatePart<ScaleTransform>(
                    button,
                    "HoverRevealScale"
                );

                HoverRevealAnimator.Reset(button);

                Assert.Equal(0, hoverChrome.Opacity);
                Assert.Equal(0, revealScale.ScaleX);
                Assert.Equal(0, revealScale.ScaleY);

                HoverRevealAnimator.Show(button);
                FlushDispatcher();

                Assert.Equal(1, hoverChrome.Opacity);
                Assert.Equal(1, revealScale.ScaleX);
                Assert.Equal(1, revealScale.ScaleY);

                HoverRevealAnimator.Begin(button, TimeSpan.Zero);
                FlushDispatcher();

                Assert.Equal(1, hoverChrome.Opacity);
                Assert.Equal(1, revealScale.ScaleX);
                Assert.Equal(1, revealScale.ScaleY);

                button.Tag = "Pressed";
                HoverRevealAnimator.Reset(button);
                FlushDispatcher();

                Assert.Equal(1, hoverChrome.Opacity);
                Assert.Equal(0, revealScale.ScaleX);
                Assert.Equal(0, revealScale.ScaleY);

                button.ClearValue(FrameworkElement.TagProperty);
                FlushDispatcher();

                Assert.Equal(0, hoverChrome.Opacity);
            }
            finally
            {
                window.Close();
            }
        });
    }

    [Fact]
    public void PointerPressAndRelease_DoNotResetOrReplayTheReveal()
    {
        RunInSta(() =>
        {
            var button = new FlourishButton { Content = "Press" };
            var window = CreateWindow(LoadResourceDictionary(), button);

            try
            {
                window.Show();
                window.UpdateLayout();
                button.ApplyTemplate();
                var hoverChrome = AssertTemplatePart<Border>(button, "HoverChrome");
                var revealScale = AssertTemplatePart<ScaleTransform>(
                    button,
                    "HoverRevealScale"
                );

                HoverRevealAnimator.Show(button);
                FlushDispatcher();

                Assert.Equal(1, hoverChrome.Opacity);
                Assert.Equal(1, revealScale.ScaleX);
                Assert.Equal(1, revealScale.ScaleY);

                RaiseMouseButtonEvent(button, Mouse.PreviewMouseDownEvent);
                RaiseMouseButtonEvent(button, Mouse.PreviewMouseUpEvent);
                FlushDispatcher();

                Assert.Equal(1, hoverChrome.Opacity);
                Assert.Equal(1, revealScale.ScaleX);
                Assert.Equal(1, revealScale.ScaleY);

                RaiseMouseEvent(button, Mouse.MouseLeaveEvent);
                FlushDispatcher();

                Assert.Equal(0, hoverChrome.Opacity);
                Assert.Equal(0, revealScale.ScaleX);
                Assert.Equal(0, revealScale.ScaleY);
            }
            finally
            {
                window.Close();
            }
        });
    }

    [Fact]
    public void DisabledBehavior_ClearsClocksAndDoesNotAnimatePointerEvents()
    {
        RunInSta(() =>
        {
            var button = new FlourishButton { Content = "Disabled motion" };
            HoverReveal.SetAnimationDuration(button, TimeSpan.FromMinutes(1));
            var window = CreateWindow(LoadResourceDictionary(), button);

            try
            {
                window.Show();
                window.UpdateLayout();
                var parts = HoverRevealAnimator.ResolveTemplateParts(button);

                RaiseMouseEvent(button, Mouse.MouseEnterEvent);
                Assert.True(parts.HoverChrome!.HasAnimatedProperties);
                Assert.True(parts.HoverRevealScale!.HasAnimatedProperties);

                HoverReveal.SetIsEnabled(button, false);

                Assert.False(parts.HoverChrome.HasAnimatedProperties);
                Assert.False(parts.HoverRevealScale.HasAnimatedProperties);
                Assert.Null(HoverRevealAnimator.TryGetTemplateParts(button));

                RaiseMouseEvent(button, Mouse.MouseEnterEvent);
                RaiseMouseButtonEvent(button, Mouse.PreviewMouseDownEvent);
                RaiseMouseButtonEvent(button, Mouse.PreviewMouseUpEvent);
                RaiseMouseEvent(button, Mouse.MouseLeaveEvent);

                Assert.False(parts.HoverChrome.HasAnimatedProperties);
                Assert.False(parts.HoverRevealScale.HasAnimatedProperties);
                Assert.Null(HoverRevealAnimator.TryGetTemplateParts(button));
            }
            finally
            {
                window.Close();
            }
        });
    }

    [Fact]
    public void DisabledControl_DoesNotResolvePartsOrStartAnimationClocks()
    {
        RunInSta(() =>
        {
            var button = new FlourishButton { Content = "Disabled control" };
            HoverReveal.SetAnimationDuration(button, TimeSpan.FromMinutes(1));
            var window = CreateWindow(LoadResourceDictionary(), button);

            try
            {
                window.Show();
                window.UpdateLayout();
                RaiseMouseEvent(button, Mouse.MouseEnterEvent);
                var active = HoverRevealAnimator.ResolveTemplateParts(button);
                Assert.True(active.HasAnimationClocks);

                button.IsEnabled = false;

                Assert.False(active.HoverChrome!.HasAnimatedProperties);
                Assert.False(active.HoverRevealScale!.HasAnimatedProperties);
                Assert.Null(HoverRevealAnimator.TryGetTemplateParts(button));

                RaiseMouseEvent(button, Mouse.MouseEnterEvent);
                Assert.Null(HoverRevealAnimator.TryGetTemplateParts(button));

                button.IsEnabled = true;
                RaiseMouseEvent(button, Mouse.MouseEnterEvent);
                Assert.True(
                    HoverRevealAnimator.ResolveTemplateParts(button).HasAnimationClocks
                );
            }
            finally
            {
                window.Close();
            }
        });
    }

    [Fact]
    public void RuntimeMotionResource_UpdatesParticipantsWithoutAWindowPolicyValue()
    {
        RunInSta(() =>
        {
            var button = new FlourishButton { Content = "Runtime policy" };
            var unrelatedElement = new Border();
            var panel = new StackPanel
            {
                Children = { button, unrelatedElement },
            };
            var window = CreateWindow(LoadResourceDictionary(), panel);

            try
            {
                window.Show();
                window.UpdateLayout();

                Assert.Same(
                    DependencyProperty.UnsetValue,
                    window.ReadLocalValue(HoverReveal.IsEnabledProperty)
                );
                Assert.True(HoverReveal.GetIsMotionEnabled(button));
                Assert.True(HoverReveal.GetIsEnabled(button));

                window.Resources["FlourishHoverRevealEnabled"] = false;
                FlushDispatcher();

                Assert.False(HoverReveal.GetIsMotionEnabled(button));
                Assert.False(HoverReveal.GetIsEnabled(button));
                Assert.True(HoverReveal.GetIsMotionEnabled(unrelatedElement));
                Assert.True(HoverReveal.GetIsEnabled(unrelatedElement));

                RaiseMouseEvent(button, Mouse.MouseEnterEvent);
                Assert.Null(HoverRevealAnimator.TryGetTemplateParts(button));

                window.Resources["FlourishHoverRevealEnabled"] = true;
                FlushDispatcher();
                RaiseMouseEvent(button, Mouse.MouseEnterEvent);

                Assert.True(HoverReveal.GetIsMotionEnabled(button));
                Assert.True(HoverReveal.GetIsEnabled(button));
                Assert.True(
                    HoverRevealAnimator.ResolveTemplateParts(button).HasAnimationClocks
                );
            }
            finally
            {
                window.Close();
            }
        });
    }

    [Fact]
    public void ResourceOnlyMotionDisable_PreservesTheCompatibilityFallback()
    {
        RunInSta(() =>
        {
            var resources = new ResourceDictionary
            {
                ["FlourishHoverRevealEnabled"] = false,
            };
            var button = new Button { Template = CreateHoverTemplate() };
            HoverReveal.SetIsParticipant(button, true);
            var window = CreateWindow(resources, button);

            try
            {
                window.Show();
                window.UpdateLayout();
                var parts = HoverRevealAnimator.ResolveTemplateParts(button);

                RaiseMouseEvent(button, Mouse.MouseEnterEvent);
                FlushDispatcher();

                Assert.True(parts.HoverChrome!.HasAnimatedProperties);
                Assert.True(parts.HoverRevealScale!.HasAnimatedProperties);
                Assert.Equal(1, parts.HoverChrome.Opacity);
                Assert.Equal(1, parts.HoverRevealScale.ScaleX);
                Assert.Equal(1, parts.HoverRevealScale.ScaleY);
            }
            finally
            {
                window.Close();
            }
        });
    }

    [Fact]
    public void CustomTemplate_DisabledMotionPreservesTheBehaviorFallback()
    {
        RunInSta(() =>
        {
            var button = new Button { Template = CreateHoverTemplate() };
            HoverReveal.SetIsParticipant(button, true);
            HoverReveal.SetIsEnabled(button, false);
            var window = CreateWindow(new ResourceDictionary(), button);

            try
            {
                window.Show();
                window.UpdateLayout();

                Assert.Null(HoverRevealAnimator.TryGetTemplateParts(button));
                RaiseMouseEvent(button, Mouse.MouseEnterEvent);
                FlushDispatcher();

                var parts = HoverRevealAnimator.ResolveTemplateParts(button);
                Assert.True(parts.HoverChrome!.HasAnimatedProperties);
                Assert.True(parts.HoverRevealScale!.HasAnimatedProperties);
                Assert.Equal(1, parts.HoverChrome.Opacity);
                Assert.Equal(1, parts.HoverRevealScale.ScaleX);
                Assert.Equal(1, parts.HoverRevealScale.ScaleY);
            }
            finally
            {
                window.Close();
            }
        });
    }

    [Fact]
    public void FlourishControl_LocalTemplateOverrideUsesTheCompatibilityContract()
    {
        RunInSta(() =>
        {
            var button = new FlourishButton
            {
                Content = "Custom Flourish template",
                Template = CreateFlourishButtonHoverTemplate(),
            };
            HoverReveal.SetIsEnabled(button, false);
            var window = CreateWindow(LoadResourceDictionary(), button);

            try
            {
                window.Show();
                window.UpdateLayout();

                Assert.True(HoverReveal.GetTemplateHandlesInteraction(button));
                Assert.Null(HoverRevealAnimator.TryGetTemplateParts(button));

                RaiseMouseEvent(button, Mouse.MouseEnterEvent);
                var compatibilityParts = HoverRevealAnimator.ResolveTemplateParts(button);

                Assert.True(compatibilityParts.HasAnimationClocks);

                HoverReveal.SetTemplateHandlesInteraction(button, true);

                Assert.Null(HoverRevealAnimator.TryGetTemplateParts(button));
                RaiseMouseEvent(button, Mouse.MouseEnterEvent);
                Assert.Null(HoverRevealAnimator.TryGetTemplateParts(button));
            }
            finally
            {
                window.Close();
            }
        });
    }

    [Fact]
    public void Animator_ReusesCachedPartsUntilTheTemplateChanges()
    {
        RunInSta(() =>
        {
            var button = new Button { Template = CreateHoverTemplate() };
            var window = CreateWindow(new ResourceDictionary(), button);

            try
            {
                window.Show();
                window.UpdateLayout();

                var first = HoverRevealAnimator.ResolveTemplateParts(button);
                var second = HoverRevealAnimator.ResolveTemplateParts(button);

                Assert.Same(first, second);
                Assert.True(first.HasParts);

                button.Template = CreateHoverTemplate();
                button.ApplyTemplate();
                window.UpdateLayout();
                var replacement = HoverRevealAnimator.ResolveTemplateParts(button);

                Assert.NotSame(first, replacement);
                Assert.NotSame(first.HoverChrome, replacement.HoverChrome);
                Assert.NotSame(first.HoverRevealScale, replacement.HoverRevealScale);

                HoverRevealAnimator.Show(button);
                FlushDispatcher();

                Assert.False(first.HoverChrome!.HasAnimatedProperties);
                Assert.False(first.HoverRevealScale!.HasAnimatedProperties);
                Assert.True(replacement.HoverChrome!.HasAnimatedProperties);
                Assert.True(replacement.HoverRevealScale!.HasAnimatedProperties);
            }
            finally
            {
                window.Close();
            }
        });
    }

    [Fact]
    public void Participant_DoesNotResolveTemplatePartsUntilTheFirstHover()
    {
        RunInSta(() =>
        {
            var button = new FlourishButton { Content = "Lazy hover" };
            var window = CreateWindow(LoadResourceDictionary(), button);

            try
            {
                window.Show();
                window.UpdateLayout();

                Assert.Null(HoverRevealAnimator.TryGetTemplateParts(button));

                RaiseMouseEvent(button, Mouse.MouseEnterEvent);

                var parts = HoverRevealAnimator.TryGetTemplateParts(button);
                Assert.NotNull(parts);
                Assert.True(parts.HasAnimationClocks);
            }
            finally
            {
                window.Close();
            }
        });
    }

    [Fact]
    public void Animator_InvalidatesTheCacheWhenTheSameTemplateIsReapplied()
    {
        RunInSta(() =>
        {
            var template = CreateHoverTemplate();
            var button = new Button { Template = template };
            var window = CreateWindow(new ResourceDictionary(), button);

            try
            {
                window.Show();
                window.UpdateLayout();
                var first = HoverRevealAnimator.ResolveTemplateParts(button);

                button.Template = null;
                window.UpdateLayout();
                button.Template = template;
                button.ApplyTemplate();
                window.UpdateLayout();
                var reapplied = HoverRevealAnimator.ResolveTemplateParts(button);

                Assert.NotSame(first, reapplied);
                Assert.NotSame(first.HoverChrome, reapplied.HoverChrome);
                Assert.NotSame(first.HoverRevealScale, reapplied.HoverRevealScale);
            }
            finally
            {
                window.Close();
            }
        });
    }

    [Fact]
    public void Animator_NegativeCacheIsReplacedWhenAValidTemplateIsApplied()
    {
        RunInSta(() =>
        {
            var button = new Button { Template = CreateHoverTemplate(includeParts: false) };
            var window = CreateWindow(new ResourceDictionary(), button);

            try
            {
                window.Show();
                window.UpdateLayout();
                var missing = HoverRevealAnimator.ResolveTemplateParts(button);

                Assert.False(missing.HasParts);
                Assert.Same(missing, HoverRevealAnimator.ResolveTemplateParts(button));

                button.Template = CreateHoverTemplate();
                button.ApplyTemplate();
                window.UpdateLayout();
                var resolved = HoverRevealAnimator.ResolveTemplateParts(button);

                Assert.NotSame(missing, resolved);
                Assert.True(resolved.HasParts);
            }
            finally
            {
                window.Close();
            }
        });
    }

    [Fact]
    public void ParticipantLifecycle_DetachesPointerWorkWhileUnloadedOrDisabled()
    {
        RunInSta(() =>
        {
            var button = new Button { Template = CreateHoverTemplate() };
            HoverReveal.SetIsParticipant(button, true);
            var panel = new StackPanel();
            var window = CreateWindow(new ResourceDictionary(), panel);

            RaiseMouseEvent(button, Mouse.MouseEnterEvent);
            Assert.Null(HoverRevealAnimator.TryGetTemplateParts(button));

            try
            {
                window.Show();
                window.UpdateLayout();

                for (var cycle = 0; cycle < 3; cycle++)
                {
                    panel.Children.Add(button);
                    window.UpdateLayout();
                    FlushDispatcher();
                    RaiseMouseEvent(button, Mouse.MouseEnterEvent);
                    var loaded = HoverRevealAnimator.ResolveTemplateParts(button);
                    Assert.True(loaded.HasAnimationClocks);

                    panel.Children.Remove(button);
                    FlushDispatcher();
                    Assert.False(button.IsLoaded);
                    Assert.False(loaded.HoverChrome!.HasAnimatedProperties);
                    Assert.False(loaded.HoverRevealScale!.HasAnimatedProperties);
                    Assert.Null(HoverRevealAnimator.TryGetTemplateParts(button));

                    RaiseMouseEvent(button, Mouse.MouseEnterEvent);
                    Assert.Null(HoverRevealAnimator.TryGetTemplateParts(button));
                }

                panel.Children.Add(button);
                window.UpdateLayout();
                FlushDispatcher();

                HoverReveal.SetIsParticipant(button, false);
                Assert.Null(HoverRevealAnimator.TryGetTemplateParts(button));
                RaiseMouseEvent(button, Mouse.MouseEnterEvent);
                Assert.Null(HoverRevealAnimator.TryGetTemplateParts(button));
            }
            finally
            {
                window.Close();
            }
        });
    }

    [Fact]
    public void NavigationItem_HoverRevealStartsHiddenAndResetsAfterPointerLeaves()
    {
        RunInSta(() =>
        {
            var resources = LoadResourceDictionary();
            var item = new FlourishListBoxItem
            {
                Content = "Navigation item",
                DataContext = new NavigationItemState(),
            };
            HoverReveal.SetAnimationDuration(item, TimeSpan.Zero);

            var listBox = new FlourishListBox
            {
                Appearance = FlourishListBoxAppearance.Navigation,
                Items = { item },
            };
            var window = CreateWindow(resources, listBox);

            try
            {
                window.Show();
                window.UpdateLayout();
                item.ApplyTemplate();

                var hoverChrome = AssertTemplatePart<Border>(item, "HoverChrome");
                var revealScale = AssertTemplatePart<ScaleTransform>(
                    item,
                    "HoverRevealScale"
                );

                Assert.Equal(0, hoverChrome.Opacity);
                Assert.Equal(0, revealScale.ScaleX);
                Assert.Equal(0, revealScale.ScaleY);

                RaiseMouseEvent(item, Mouse.MouseEnterEvent);
                FlushDispatcher();

                Assert.Equal(1, hoverChrome.Opacity);
                Assert.Equal(1, revealScale.ScaleX);
                Assert.Equal(1, revealScale.ScaleY);

                RaiseMouseEvent(item, Mouse.MouseLeaveEvent);
                FlushDispatcher();

                Assert.Equal(0, hoverChrome.Opacity);
                Assert.Equal(0, revealScale.ScaleX);
                Assert.Equal(0, revealScale.ScaleY);
            }
            finally
            {
                window.Close();
            }
        });
    }

    [Fact]
    public void ButtonTemplates_UseKeyboardFocusVisualWithoutTemplateFocusChrome()
    {
        RunInSta(() =>
        {
            var resources = LoadResourceDictionary();
            FlourishButton[] buttons =
            [
                new() { Content = "Standard" },
                new()
                {
                    Content = "Primary",
                    Appearance = FlourishButtonAppearance.Primary,
                },
                new()
                {
                    Content = "Caption",
                    Appearance = FlourishButtonAppearance.Subtle,
                    Variant = FlourishButtonVariant.WindowCaption,
                },
            ];
            var panel = new StackPanel();
            foreach (var button in buttons)
            {
                panel.Children.Add(button);
            }

            var window = CreateWindow(resources, panel);
            try
            {
                window.Show();
                window.UpdateLayout();

                foreach (var button in buttons)
                {
                    button.ApplyTemplate();
                    Assert.NotNull(button.FocusVisualStyle);
                    Assert.Null(button.Template.FindName("FocusChrome", button));
                }
            }
            finally
            {
                window.Close();
            }
        });
    }

    private static ResourceDictionary LoadResourceDictionary()
    {
        return Assert.IsType<ResourceDictionary>(
            Application.LoadComponent(new Uri(GenericThemeSource, UriKind.Relative))
        );
    }

    private static Window CreateWindow(ResourceDictionary resources, UIElement content)
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
        window.Resources.MergedDictionaries.Add(resources);
        return window;
    }

    private static T AssertTemplatePart<T>(Control control, string partName)
        where T : class
    {
        var part = control.Template.FindName(partName, control);
        return Assert.IsType<T>(part);
    }

    private static ControlTemplate CreateHoverTemplate(bool includeParts = true)
    {
        var templateXaml = includeParts
            ? """
              <ControlTemplate
                xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
                TargetType="{x:Type Button}"
              >
                <Border x:Name="HoverChrome" Opacity="0">
                  <Border.RenderTransform>
                    <ScaleTransform x:Name="HoverRevealScale" ScaleX="0" ScaleY="0" />
                  </Border.RenderTransform>
                </Border>
              </ControlTemplate>
              """
            : """
              <ControlTemplate
                xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
                xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
                TargetType="{x:Type Button}"
              >
                <Border />
              </ControlTemplate>
              """;
        return Assert.IsType<ControlTemplate>(XamlReader.Parse(templateXaml));
    }

    private static ControlTemplate CreateFlourishButtonHoverTemplate()
    {
        const string templateXaml =
            """
            <ControlTemplate
              xmlns="http://schemas.microsoft.com/winfx/2006/xaml/presentation"
              xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
              xmlns:controls="clr-namespace:ArkheideSystem.Flourish.Controls;assembly=Flourish"
              TargetType="{x:Type controls:FlourishButton}"
            >
              <Border x:Name="HoverChrome" Opacity="0">
                <Border.RenderTransform>
                  <ScaleTransform x:Name="HoverRevealScale" ScaleX="0" ScaleY="0" />
                </Border.RenderTransform>
                <ContentPresenter />
              </Border>
            </ControlTemplate>
            """;
        return Assert.IsType<ControlTemplate>(XamlReader.Parse(templateXaml));
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
            new MouseButtonEventArgs(
                Mouse.PrimaryDevice,
                Environment.TickCount,
                MouseButton.Left
            )
            {
                RoutedEvent = routedEvent,
            }
        );
    }

    private static void FlushDispatcher()
    {
        Dispatcher.CurrentDispatcher.Invoke(
            DispatcherPriority.Render,
            new Action(() => { })
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

    private sealed class NavigationItemState
    {
        public string Label => "Navigation item";

        public bool IsEnabled => true;

        public bool IsVisible => true;

        public bool IsGroupHeader => false;

        public bool IsCommandItem => false;
    }
}
