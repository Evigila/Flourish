using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using System.Xml.Linq;
using ArkheideSystem.Flourish.Abstract;
using ArkheideSystem.Flourish.Internal.Configuration;
using ArkheideSystem.Flourish.Services;

namespace ArkheideSystem.Flourish.Test.Services;

public sealed class RuntimeToolTipPolicyTests
{
    private const string DelayKey = "FlourishToolTipInitialShowDelay";
    private const string MarginKey = "FlourishToolTipSpawnableMargin";

    [Fact]
    public void Attach_PublishesOnlyApplicationPolicyResourcesAndSameScopeIsStable()
    {
        RunInSta(() =>
        {
            var options = new FlourishShellOptions { IsTipsEnabled = true };
            options.Tips.InitialShowDelayMilliseconds = 240;
            options.Tips.SpawnableMargin = 6;
            var resources = new ResourceDictionary();
            var sut = new FlourishToolTipService(options);

            sut.Attach(Dispatcher.CurrentDispatcher, resources);
            var delay = resources[DelayKey];
            var margin = resources[MarginKey];

            sut.Attach(Dispatcher.CurrentDispatcher, resources);

            Assert.Equal(2, resources.Count);
            Assert.Same(delay, resources[DelayKey]);
            Assert.Same(margin, resources[MarginKey]);
            Assert.Equal(240, delay);
            Assert.Equal(6d, margin);
        });
    }

    [Fact]
    public void ExistingButtonsInTwoWindowsFollowTheSharedDynamicDelayResource()
    {
        RunInSta(() =>
        {
            var options = new FlourishShellOptions { IsTipsEnabled = true };
            options.Tips.InitialShowDelayMilliseconds = 200;
            var resources = new ResourceDictionary();
            var sut = new FlourishToolTipService(options);
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

            sut.Configure(450, 8);

            Assert.Equal(450, ToolTipService.GetInitialShowDelay(firstButton));
            Assert.Equal(450, ToolTipService.GetInitialShowDelay(secondButton));
            Assert.Equal(8d, firstButton.TryFindResource(MarginKey));
            Assert.Equal(8d, secondButton.TryFindResource(MarginKey));
            Assert.DoesNotContain(
                DelayKey,
                firstWindow.Resources.Keys.Cast<object>()
            );
            Assert.DoesNotContain(
                DelayKey,
                secondWindow.Resources.Keys.Cast<object>()
            );

            sut.SetEnabled(false);

            Assert.Equal(int.MaxValue, ToolTipService.GetInitialShowDelay(firstButton));
            Assert.Equal(int.MaxValue, ToolTipService.GetInitialShowDelay(secondButton));
            Assert.Equal(0d, firstButton.TryFindResource(MarginKey));
            Assert.Equal(0d, secondButton.TryFindResource(MarginKey));
        });
    }

    [Fact]
    public void BackgroundMutationUpdatesResourcesBeforeChangedOnTheAttachedDispatcher()
    {
        RunInSta(() =>
        {
            var dispatcher = Dispatcher.CurrentDispatcher;
            var dispatcherThreadId = Environment.CurrentManagedThreadId;
            var options = new FlourishShellOptions { IsTipsEnabled = true };
            options.Tips.InitialShowDelayMilliseconds = 200;
            options.Tips.SpawnableMargin = 5;
            var resources = new ResourceDictionary();
            var sut = new FlourishToolTipService(options);
            sut.Attach(dispatcher, resources);
            var events = new List<(
                int ThreadId,
                int Delay,
                double Margin,
                FlourishToolTipChangedEventArgs Args
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
            var mutation = Task.Run(() => sut.Configure(480, 9));
            Assert.True(
                operationPosted.Wait(TimeSpan.FromSeconds(5)),
                "The tooltip mutation did not post to the attached dispatcher."
            );
            dispatcher.Hooks.OperationPosted -= OnOperationPosted;

            Assert.Equal(200, resources[DelayKey]);
            Assert.Equal(5d, resources[MarginKey]);
            Assert.Equal(200, sut.Current.InitialShowDelayMilliseconds);

            PumpDispatcherUntil(dispatcher, mutation);

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
        RunInSta(() =>
        {
            var options = new FlourishShellOptions { IsTipsEnabled = true };
            options.Tips.InitialShowDelayMilliseconds = 200;
            options.Tips.SpawnableMargin = 5;
            var resources = new ResourceDictionary();
            var sut = new FlourishToolTipService(options);
            sut.Attach(Dispatcher.CurrentDispatcher, resources);
            var delay = resources[DelayKey];
            var margin = resources[MarginKey];
            var eventCount = 0;
            sut.Changed += (_, _) => eventCount++;

            sut.SetEnabled(true);
            sut.Configure(200, 5);

            Assert.Equal(0, eventCount);
            Assert.Same(delay, resources[DelayKey]);
            Assert.Same(margin, resources[MarginKey]);
        });
    }

    [Fact]
    public void SourceContractsUseOneApplicationPolicyAndNoButtonLocalDelay()
    {
        var flourishRoot = Path.Combine(FindRepositoryRoot(), "src", "Flourish");
        var serviceSource = File.ReadAllText(
            Path.Combine(flourishRoot, "Services", "FlourishToolTipService.cs")
        );
        var shellSource = File.ReadAllText(
            Path.Combine(flourishRoot, "Views", "Windows", "FlourishShellWindow.xaml.cs")
        );
        var featureSource = File.ReadAllText(
            Path.Combine(flourishRoot, "Services", "ShellFeatureService.cs")
        );
        var runtimeSource = File.ReadAllText(
            Path.Combine(flourishRoot, "Internal", "Composition", "FlourishRuntime.cs")
        );
        var buttonXaml = XDocument.Load(
            Path.Combine(flourishRoot, "Controls", "Button.xaml")
        );

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
        Assert.Contains(
            "toolTipService.SetEnabled(enabled)",
            featureSource,
            StringComparison.Ordinal
        );
        Assert.DoesNotContain(
            "options.IsTipsEnabled = enabled",
            featureSource,
            StringComparison.Ordinal
        );

        var toolTipAttachIndex = runtimeSource.IndexOf(
            "GetRequiredService<FlourishToolTipService>().Attach(application)",
            StringComparison.Ordinal
        );
        var shellResolveIndex = runtimeSource.IndexOf(
            "GetRequiredService<FlourishShellWindow>()",
            StringComparison.Ordinal
        );
        Assert.True(toolTipAttachIndex >= 0);
        Assert.True(shellResolveIndex > toolTipAttachIndex);

        Assert.Contains(
            buttonXaml.Descendants().Where(element => element.Name.LocalName == "Setter"),
            element =>
                (string?)element.Attribute("Property")
                    == "ToolTipService.InitialShowDelay"
                && (string?)element.Attribute("Value")
                    == "{DynamicResource FlourishToolTipInitialShowDelay}"
        );
    }

    private static Button CreateDynamicDelayButton()
    {
        var button = new Button();
        button.SetResourceReference(
            ToolTipService.InitialShowDelayProperty,
            DelayKey
        );
        return button;
    }

    private static void PumpDispatcherUntil(Dispatcher dispatcher, Task task)
    {
        var frame = new DispatcherFrame();
        _ = task.ContinueWith(
            _ =>
                dispatcher.BeginInvoke(
                    DispatcherPriority.Send,
                    new Action(() => frame.Continue = false)
                ),
            CancellationToken.None,
            TaskContinuationOptions.ExecuteSynchronously,
            TaskScheduler.Default
        );
        Dispatcher.PushFrame(frame);
        task.GetAwaiter().GetResult();
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
}
