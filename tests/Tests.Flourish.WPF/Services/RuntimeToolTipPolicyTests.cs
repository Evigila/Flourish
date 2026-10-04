using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using ArkheideSystem.Flourish.Abstract;
using ArkheideSystem.Flourish.ToolTips;
using ArkheideSystem.Tests.Flourish.WPF.Infrastructure;

using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using System.Xml.Linq;
using ArkheideSystem.Flourish.Controls;
using FlourishButton = ArkheideSystem.Flourish.Controls.Button;
using WpfButton = System.Windows.Controls.Button;
using CustomToolTip = ArkheideSystem.Flourish.Controls.ToolTip;
using RuntimeToolTipService = ArkheideSystem.Flourish.ToolTips.ToolTipService;
using ToolTip = System.Windows.Controls.ToolTip;
using ToolTipService = System.Windows.Controls.ToolTipService;

namespace ArkheideSystem.Tests.Flourish.WPF.Services;

public sealed class RuntimeToolTipPolicyTests
{
    private const string DelayKey = "ToolTipInitialShowDelay";
    private const string MarginKey = "ToolTipSpawnableMargin";
    private const string EnabledKey = "ToolTipsEnabled";
    private const string GenericThemeSource = "/Flourish.WPF;component/Themes/Generic.xaml";

    [Fact]
    public void Attach_PublishesOnlyApplicationPolicyResourcesAndSameScopeIsStable()
    {
        StaTest.Run(() =>
        {
            var options = new ToolTipOptions { IsTipsEnabled = true };
            options.InitialShowDelayMilliseconds = 240;
            options.SpawnableMargin = 6;
            var resources = new ResourceDictionary();
            var sut = new RuntimeToolTipService(options);

            sut.Attach(Dispatcher.CurrentDispatcher, resources);
            var delay = resources[DelayKey];
            var margin = resources[MarginKey];

            sut.Attach(Dispatcher.CurrentDispatcher, resources);

            Assert.Equal(3, resources.Count);
            Assert.Same(delay, resources[DelayKey]);
            Assert.Same(margin, resources[MarginKey]);
            Assert.True(Assert.IsType<bool>(resources[EnabledKey]));
            Assert.Equal(240, delay);
            Assert.Equal(6d, margin);
        });
    }

    [Fact]
    public void ExistingButtonsInTwoWindowsFollowTheSharedDynamicDelayResource()
    {
        StaTest.Run(() =>
        {
            var options = new ToolTipOptions { IsTipsEnabled = true };
            options.InitialShowDelayMilliseconds = 200;
            var resources = new ResourceDictionary();
            var sut = new RuntimeToolTipService(options);
            sut.Attach(Dispatcher.CurrentDispatcher, resources);
            var firstWindow = new Window();
            var secondWindow = new Window();
            firstWindow.Resources.MergedDictionaries.Add(resources);
            secondWindow.Resources.MergedDictionaries.Add(resources);
            var firstButton = CreateDynamicDelayButton();
            var secondButton = CreateDynamicDelayButton();
            firstWindow.Content = firstButton;
            secondWindow.Content = secondButton;

            Assert.Equal(200, ToolTipService.GetInitialShowDelay(firstButton));
            Assert.Equal(200, ToolTipService.GetInitialShowDelay(secondButton));

            sut.SetSettings(450, 8);

            Assert.Equal(450, ToolTipService.GetInitialShowDelay(firstButton));
            Assert.Equal(450, ToolTipService.GetInitialShowDelay(secondButton));
            Assert.Equal(8d, firstButton.TryFindResource(MarginKey));
            Assert.Equal(8d, secondButton.TryFindResource(MarginKey));
            Assert.DoesNotContain(DelayKey, firstWindow.Resources.Keys.Cast<object>());
            Assert.DoesNotContain(DelayKey, secondWindow.Resources.Keys.Cast<object>());

            sut.SetEnabled(false);

            Assert.Equal(int.MaxValue, ToolTipService.GetInitialShowDelay(firstButton));
            Assert.Equal(int.MaxValue, ToolTipService.GetInitialShowDelay(secondButton));
            Assert.Equal(0d, firstButton.TryFindResource(MarginKey));
            Assert.Equal(0d, secondButton.TryFindResource(MarginKey));
        });
    }

    [Fact]
    public void EnabledPolicy_WrapsOnlyFlourishOwnersAndRuntimeDisableRestoresNativeDefaults()
    {
        StaTest.Run(() =>
        {
            var options = new ToolTipOptions { IsTipsEnabled = true };
            options.InitialShowDelayMilliseconds = 240;
            options.SpawnableMargin = 6;
            var policyResources = new ResourceDictionary();
            var sut = new RuntimeToolTipService(options);
            sut.Attach(Dispatcher.CurrentDispatcher, policyResources);

            var rawContent = new object();
            var explicitNativeToolTip = new ToolTip { Content = "Explicit native" };
            var flourishOwner = new FlourishButton { ToolTip = rawContent };
            var explicitOwner = new FlourishButton { ToolTip = explicitNativeToolTip };
            var nativeOwner = new WpfButton { ToolTip = "Native owner" };
            var panel = new StackPanel { Children = { flourishOwner, explicitOwner, nativeOwner } };
            var window = CreatePolicyWindow(panel, policyResources);
            var nativeDefaultDelay = ToolTipService.GetInitialShowDelay(new WpfButton());

            try
            {
                window.Show();
                window.UpdateLayout();

                var wrapped = Assert.IsType<CustomToolTip>(flourishOwner.ToolTip);
                Assert.Same(rawContent, wrapped.Content);
                Assert.Same(explicitNativeToolTip, explicitOwner.ToolTip);
                Assert.Equal("Native owner", nativeOwner.ToolTip);
                Assert.True(ToolTipPolicy.GetIsEnabled(flourishOwner));
                Assert.False(ToolTipPolicy.GetIsEnabled(nativeOwner));
                Assert.Equal(240, ToolTipService.GetInitialShowDelay(flourishOwner));
                Assert.Equal(nativeDefaultDelay, ToolTipService.GetInitialShowDelay(nativeOwner));

                sut.SetEnabled(false);

                Assert.Same(rawContent, flourishOwner.ToolTip);
                Assert.Same(explicitNativeToolTip, explicitOwner.ToolTip);
                Assert.Equal("Native owner", nativeOwner.ToolTip);
                Assert.False(ToolTipPolicy.GetIsEnabled(flourishOwner));
                Assert.Equal(nativeDefaultDelay, ToolTipService.GetInitialShowDelay(flourishOwner));
                Assert.Equal(nativeDefaultDelay, ToolTipService.GetInitialShowDelay(nativeOwner));

                sut.SetEnabled(true);

                var rewrapped = Assert.IsType<CustomToolTip>(flourishOwner.ToolTip);
                Assert.Same(rawContent, rewrapped.Content);
                Assert.Same(explicitNativeToolTip, explicitOwner.ToolTip);
                Assert.Equal("Native owner", nativeOwner.ToolTip);
                Assert.True(ToolTipPolicy.GetIsEnabled(flourishOwner));
                Assert.Equal(240, ToolTipService.GetInitialShowDelay(flourishOwner));
                Assert.Equal(nativeDefaultDelay, ToolTipService.GetInitialShowDelay(nativeOwner));
            }
            finally
            {
                window.Close();
            }
        });
    }

    [Fact]
    public void UnconfiguredPolicy_LeavesFlourishAndNativeOwnersOnNativeTooltipBehavior()
    {
        StaTest.Run(() =>
        {
            var rawContent = new object();
            var flourishOwner = new FlourishButton { ToolTip = rawContent };
            var nativeOwner = new WpfButton { ToolTip = "Native owner" };
            var panel = new StackPanel { Children = { flourishOwner, nativeOwner } };
            var window = CreatePolicyWindow(panel);
            var nativeDefaultDelay = ToolTipService.GetInitialShowDelay(new WpfButton());

            try
            {
                window.Show();
                window.UpdateLayout();

                Assert.Same(rawContent, flourishOwner.ToolTip);
                Assert.Equal("Native owner", nativeOwner.ToolTip);
                Assert.False(ToolTipPolicy.GetIsEnabled(flourishOwner));
                Assert.False(ToolTipPolicy.GetIsEnabled(nativeOwner));
                Assert.Equal(nativeDefaultDelay, ToolTipService.GetInitialShowDelay(flourishOwner));
                Assert.Equal(nativeDefaultDelay, ToolTipService.GetInitialShowDelay(nativeOwner));
            }
            finally
            {
                window.Close();
            }
        });
    }

    [Fact]
    public void BackgroundMutationUpdatesResourcesBeforeChangedOnTheAttachedDispatcher()
    {
        StaTest.Run(() =>
        {
            var dispatcher = Dispatcher.CurrentDispatcher;
            var dispatcherThreadId = Environment.CurrentManagedThreadId;
            var options = new ToolTipOptions { IsTipsEnabled = true };
            options.InitialShowDelayMilliseconds = 200;
            options.SpawnableMargin = 5;
            var resources = new ResourceDictionary();
            var sut = new RuntimeToolTipService(options);
            sut.Attach(dispatcher, resources);
            var events =
                new List<(
                    int ThreadId,
                    int Delay,
                    double Margin,
                    StateTransitionEventArgs<ToolTipSettings> Args
                )>();
            sut.Changed += (_, args) =>
                events.Add(
                    (
                        Environment.CurrentManagedThreadId,
                        (int)resources[DelayKey],
                        (double)resources[MarginKey],
                        args
                    )
                );

            using var operationPosted = new ManualResetEventSlim();
            dispatcher.Hooks.OperationPosted += OnOperationPosted;
            var mutation = Task.Run(() => sut.SetSettings(480, 9));
            Assert.True(
                operationPosted.Wait(TimeSpan.FromSeconds(5)),
                "The tooltip mutation did not post to the attached dispatcher."
            );
            dispatcher.Hooks.OperationPosted -= OnOperationPosted;

            Assert.Equal(200, resources[DelayKey]);
            Assert.Equal(5d, resources[MarginKey]);
            Assert.Equal(200, sut.Current.InitialShowDelayMilliseconds);

            DispatcherTest.Wait(dispatcher, mutation);

            var changed = Assert.Single(events);
            Assert.Equal(dispatcherThreadId, changed.ThreadId);
            Assert.Equal(480, changed.Delay);
            Assert.Equal(9d, changed.Margin);
            Assert.Equal(changed.Args.Current, sut.Current);
            Assert.Equal(changed.Args.Current.InitialShowDelayMilliseconds, resources[DelayKey]);
            Assert.Equal(changed.Args.Current.SpawnableMargin, resources[MarginKey]);

            void OnOperationPosted(object? sender, DispatcherHookEventArgs e)
            {
                operationPosted.Set();
            }
        });
    }

    [Fact]
    public void EquivalentRuntimeMutationsRaiseNoEventsOrResourceReplacements()
    {
        StaTest.Run(() =>
        {
            var options = new ToolTipOptions { IsTipsEnabled = true };
            options.InitialShowDelayMilliseconds = 200;
            options.SpawnableMargin = 5;
            var resources = new ResourceDictionary();
            var sut = new RuntimeToolTipService(options);
            sut.Attach(Dispatcher.CurrentDispatcher, resources);
            var delay = resources[DelayKey];
            var margin = resources[MarginKey];
            var eventCount = 0;
            sut.Changed += (_, _) => eventCount++;

            sut.SetEnabled(true);
            sut.SetSettings(200, 5);

            Assert.Equal(0, eventCount);
            Assert.Same(delay, resources[DelayKey]);
            Assert.Same(margin, resources[MarginKey]);
        });
    }

    [Fact]
    public void SourceContractsUseOneApplicationPolicyAndNoButtonLocalDelay()
    {
        var flourishRoot = Path.Combine(TestPaths.RepositoryRoot, "src", "Flourish.WPF");
        var serviceSource = File.ReadAllText(
            Path.Combine(flourishRoot, "ToolTips", "ToolTipService.cs")
        );
        var shellSource = File.ReadAllText(
            Path.Combine(flourishRoot, "Views", "Windows", "ShellWindow.xaml.cs")
        );
        var runtimeSource = File.ReadAllText(
            Path.Combine(flourishRoot, "Hosting", "HostedApplicationRuntime.cs")
        );
        var buttonXaml = XDocument.Load(Path.Combine(flourishRoot, "Controls", "Button.xaml"));

        Assert.DoesNotContain("Window? owner", serviceSource, StringComparison.Ordinal);
        Assert.DoesNotContain("window.Resources", serviceSource, StringComparison.Ordinal);
        Assert.DoesNotContain("Application.Current", serviceSource, StringComparison.Ordinal);
        Assert.DoesNotContain("toolTipService.Attach", shellSource, StringComparison.Ordinal);
        Assert.DoesNotContain("ApplyToolTipResources", shellSource, StringComparison.Ordinal);
        Assert.DoesNotContain(
            "ToolTipService.SetInitialShowDelay",
            shellSource,
            StringComparison.Ordinal
        );
        var toolTipAttachIndex = runtimeSource.IndexOf(
            "GetRequiredService<ToolTipService>().Attach(application)",
            StringComparison.Ordinal
        );
        var shellResolveIndex = runtimeSource.IndexOf(
            "GetRequiredService<ShellWindow>()",
            StringComparison.Ordinal
        );
        Assert.True(toolTipAttachIndex >= 0);
        Assert.True(shellResolveIndex > toolTipAttachIndex);

        Assert.Contains(
            buttonXaml.Descendants().Where(element => element.Name.LocalName == "Setter"),
            element =>
                (string?)element.Attribute("Property") == "ToolTipService.InitialShowDelay"
                && (string?)element.Attribute("Value")
                    == "{DynamicResource ToolTipInitialShowDelay}"
        );
    }

    private static WpfButton CreateDynamicDelayButton()
    {
        var button = new WpfButton();
        button.SetResourceReference(ToolTipService.InitialShowDelayProperty, DelayKey);
        return button;
    }

    private static Window CreatePolicyWindow(
        UIElement content,
        ResourceDictionary? policyResources = null
    )
    {
        var window = new Window
        {
            Width = 480,
            Height = 320,
            Left = -10000,
            Top = -10000,
            ShowActivated = false,
            ShowInTaskbar = false,
            Content = content,
        };
        window.Resources.MergedDictionaries.Add(
            Assert.IsType<ResourceDictionary>(
                Application.LoadComponent(new Uri(GenericThemeSource, UriKind.Relative))
            )
        );
        if (policyResources is not null)
        {
            window.Resources.MergedDictionaries.Add(policyResources);
        }

        return window;
    }
}
