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
    public void PointerRelease_RestoresStaticHoverWithoutReplayingTheReveal()
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
                </ControlTemplate>
                """;
            var button = new Button
            {
                Template = Assert.IsType<ControlTemplate>(XamlReader.Parse(templateXaml)),
            };
            HoverReveal.SetAnimationDuration(button, TimeSpan.FromMinutes(1));
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
                HoverReveal.RestoreAfterPointerRelease(button, isMouseOver: false);
                FlushDispatcher();

                Assert.Equal(0, hoverChrome.Opacity);
                Assert.Equal(0, revealScale.ScaleX);
                Assert.Equal(0, revealScale.ScaleY);

                HoverReveal.RestoreAfterPointerRelease(button, isMouseOver: true);
                FlushDispatcher();

                // A replay would still be near zero because the configured duration is one minute.
                Assert.Equal(1, hoverChrome.Opacity);
                Assert.Equal(1, revealScale.ScaleX);
                Assert.Equal(1, revealScale.ScaleY);

                HoverRevealAnimator.Reset(button);
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
    public void ButtonTemplates_DoNotRetainASeparateBlueFocusChrome()
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
                    Assert.Null(button.FocusVisualStyle);
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

    private static void RaiseMouseEvent(UIElement element, RoutedEvent routedEvent)
    {
        element.RaiseEvent(
            new MouseEventArgs(Mouse.PrimaryDevice, Environment.TickCount)
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
