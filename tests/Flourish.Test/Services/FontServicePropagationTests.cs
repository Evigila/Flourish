using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using System.Xml.Linq;
using ArkheideSystem.Flourish.Abstract;
using ArkheideSystem.Flourish.Internal.Configuration;
using ArkheideSystem.Flourish.Services;
using WpfControl = System.Windows.Controls.Control;

namespace ArkheideSystem.Flourish.Test.Services;

public sealed class FontServicePropagationTests
{
    private const string TextFamilyKey = "FlourishFontFamily";
    private const string IconFamilyKey = "FlourishIconFontFamily";

    private static readonly string[] SizeKeys =
    [
        "FlourishFontSizeSmall",
        "FlourishFontSizeCaption",
        "FlourishFontSizeDescription",
        "FlourishFontSizeBase",
        "FlourishFontSizeSubtitle",
        "FlourishFontSizeSectionTitle",
        "FlourishFontSizePageTitle",
        "FlourishFontSizeTitle",
        "FlourishFontSizeTitlebarIcon",
        "FlourishFontSizeNavigationIcon",
        "FlourishFontSizeWindowButtonIcon",
        "FlourishLineHeightBody",
        "FlourishLineHeightDescription",
        "FlourishLineHeightSubtitle",
    ];

    private static readonly string[] AllKeys =
    [
        TextFamilyKey,
        IconFamilyKey,
        .. SizeKeys,
    ];

    [Fact]
    public void Attach_PopulatesOnlyTheSixteenTypographyKeysAndSameScopeReattachIsStable()
    {
        RunInSta(() =>
        {
            var resources = new ResourceDictionary();
            var sut = new FontService(new FlourishShellOptions());

            sut.Attach(Dispatcher.CurrentDispatcher, resources);
            var before = CaptureResources(resources);

            sut.Attach(Dispatcher.CurrentDispatcher, resources);
            var after = CaptureResources(resources);

            Assert.Equal(16, resources.Count);
            Assert.Equal(AllKeys.Order(), resources.Keys.Cast<string>().Order());
            Assert.All(AllKeys, key => Assert.Same(before[key], after[key]));
        });
    }

    [Fact]
    public void SetFontFamily_ReplacesOnlyTheTextFamilyResource()
    {
        RunInSta(() =>
        {
            var (sut, resources) = CreateAttachedService();
            var before = CaptureResources(resources);

            sut.SetFontFamily("Arial");

            AssertOnlyResourcesChanged(before, resources, TextFamilyKey);
            Assert.Equal(
                "Arial",
                Assert.IsType<FontFamily>(resources[TextFamilyKey]).Source
            );
        });
    }

    [Fact]
    public void SetFontSize_ReplacesOnlyTheFourteenSizeAndLineHeightResources()
    {
        RunInSta(() =>
        {
            var (sut, resources) = CreateAttachedService();
            var before = CaptureResources(resources);

            sut.SetFontSize(18);

            AssertOnlyResourcesChanged(before, resources, SizeKeys);
            Assert.Equal(18d, resources["FlourishFontSizeBase"]);
            Assert.Equal(36d, resources["FlourishFontSizePageTitle"]);
        });
    }

    [Fact]
    public void SetIconFontFamily_ReplacesOnlyTheIconFamilyResource()
    {
        RunInSta(() =>
        {
            var (sut, resources) = CreateAttachedService();
            var before = CaptureResources(resources);

            sut.SetIconFontFamily("Arial");

            AssertOnlyResourcesChanged(before, resources, IconFamilyKey);
            Assert.Equal(
                "Arial",
                Assert.IsType<FontFamily>(resources[IconFamilyKey]).Source
            );
        });
    }

    [Fact]
    public void SetFont_ReplacesTextFamilyAndSizeResourcesButNotIconFamily()
    {
        RunInSta(() =>
        {
            var (sut, resources) = CreateAttachedService();
            var before = CaptureResources(resources);

            sut.SetFont("Arial", 18);

            AssertOnlyResourcesChanged(
                before,
                resources,
                [TextFamilyKey, .. SizeKeys]
            );
            Assert.Same(before[IconFamilyKey], resources[IconFamilyKey]);
        });
    }

    [Fact]
    public void EquivalentMutationsDoNotRaiseChanged()
    {
        RunInSta(() =>
        {
            var (sut, _) = CreateAttachedService();
            var events = new List<FlourishFontChangedEventArgs>();
            sut.Changed += (_, args) => events.Add(args);

            sut.SetFont(sut.FontFamily, sut.FontSize);
            sut.SetFontFamily(sut.FontFamily);
            sut.SetFontSize(sut.FontSize);
            sut.SetIconFontFamily(sut.IconFontFamily);

            Assert.Empty(events);

            sut.SetOverrideFont<TestPage>("Arial", 18);
            sut.SetOverrideFont<TestPage>("Arial", 18);
            Assert.Single(events);

            Assert.True(sut.ClearOverrideFont<TestPage>());
            Assert.False(sut.ClearOverrideFont<TestPage>());
            Assert.Equal(2, events.Count);
        });
    }

    [Fact]
    public void Changed_IdentifiesGlobalIconAndAffectedPageOverrideScopes()
    {
        RunInSta(() =>
        {
            var (sut, _) = CreateAttachedService();
            var events = new List<FlourishFontChangedEventArgs>();
            sut.Changed += (_, args) => events.Add(args);

            sut.SetFontFamily("Arial");
            sut.SetFontSize(18);
            sut.SetFont("Times New Roman", 19);
            sut.SetIconFontFamily("Arial");
            sut.SetOverrideFont<TestPage>("Arial", 18);
            sut.ClearOverrideFont<TestPage>();

            Assert.Equal(
                [
                    FlourishFontChangeKind.GlobalText,
                    FlourishFontChangeKind.GlobalText,
                    FlourishFontChangeKind.GlobalText,
                    FlourishFontChangeKind.Icon,
                    FlourishFontChangeKind.PageOverride,
                    FlourishFontChangeKind.PageOverride,
                ],
                events.Select(args => args.ChangeKind)
            );
            Assert.All(events.Take(4), args => Assert.Null(args.AffectedPageType));
            Assert.All(
                events.Skip(4),
                args => Assert.Equal(typeof(TestPage), args.AffectedPageType)
            );
        });
    }

    [Fact]
    public void BackgroundMutationsAreSerializedOnTheAttachedDispatcherWithConsistentSnapshots()
    {
        RunInSta(() =>
        {
            var dispatcher = Dispatcher.CurrentDispatcher;
            var dispatcherThreadId = Environment.CurrentManagedThreadId;
            var options = new FlourishShellOptions
            {
                FontFamily = "Segoe UI",
                IconFontFamily = "Segoe MDL2 Assets",
                FontSize = 13,
            };
            var resources = new ResourceDictionary();
            var sut = new FontService(options);
            sut.Attach(dispatcher, resources);
            var events = new List<(int ThreadId, FlourishFontChangedEventArgs Args)>();
            sut.Changed += (_, args) =>
                events.Add((Environment.CurrentManagedThreadId, args));

            var operationPosted = new ManualResetEventSlim();
            dispatcher.Hooks.OperationPosted += OnOperationPosted;
            var mutations = new Action[]
            {
                () => sut.SetFont("Arial", 16),
                () => sut.SetIconFontFamily("Arial"),
                () => sut.SetOverrideFont<TestPage>("Arial", 18),
                () => sut.SetFont("Consolas", 19),
            };
            var tasks = new List<Task>();
            try
            {
                foreach (var mutation in mutations)
                {
                    operationPosted.Reset();
                    tasks.Add(Task.Run(mutation));
                    Assert.True(
                        operationPosted.Wait(TimeSpan.FromSeconds(5)),
                        "The background mutation did not post to the attached dispatcher."
                    );
                }
            }
            finally
            {
                dispatcher.Hooks.OperationPosted -= OnOperationPosted;
            }

            var allMutations = Task.WhenAll(tasks);
            var frame = new DispatcherFrame();
            _ = allMutations.ContinueWith(
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
            allMutations.GetAwaiter().GetResult();

            Assert.Equal(4, events.Count);
            Assert.All(
                events,
                item => Assert.Equal(dispatcherThreadId, item.ThreadId)
            );
            Assert.Equal(
                [
                    FlourishFontChangeKind.GlobalText,
                    FlourishFontChangeKind.Icon,
                    FlourishFontChangeKind.PageOverride,
                    FlourishFontChangeKind.GlobalText,
                ],
                events.Select(item => item.Args.ChangeKind)
            );
            Assert.Equal(typeof(TestPage), events[2].Args.AffectedPageType);

            var last = events[^1].Args;
            Assert.Equal("Consolas", options.FontFamily);
            Assert.Equal("Arial", options.IconFontFamily);
            Assert.Equal(19d, options.FontSize);
            Assert.Equal(options.FontFamily, sut.FontFamily);
            Assert.Equal(options.IconFontFamily, sut.IconFontFamily);
            Assert.Equal(options.FontSize, sut.FontSize);
            Assert.Equal(last.FontFamily, options.FontFamily);
            Assert.Equal(last.IconFontFamily, options.IconFontFamily);
            Assert.Equal(last.FontSize, options.FontSize);
            Assert.Equal(
                options.FontFamily,
                Assert.IsType<FontFamily>(resources[TextFamilyKey]).Source
            );
            Assert.Equal(
                options.IconFontFamily,
                Assert.IsType<FontFamily>(resources[IconFamilyKey]).Source
            );
            Assert.Equal(options.FontSize, resources["FlourishFontSizeBase"]);
            Assert.Equal(
                new FlourishPageFontOverride("Arial", 18),
                options.PageFontOverridesByPageType[typeof(TestPage)]
            );

            void OnOperationPosted(object? sender, DispatcherHookEventArgs e)
            {
                operationPosted.Set();
            }
        });
    }

    [Fact]
    public void QueuedAttachAndDetachedSetterPublishConsistentStateAcrossTheDispatcherBoundary()
    {
        RunInSta(() =>
        {
            var dispatcher = Dispatcher.CurrentDispatcher;
            var dispatcherThreadId = Environment.CurrentManagedThreadId;
            var options = new FlourishShellOptions
            {
                FontFamily = "Segoe UI",
                IconFontFamily = "Segoe MDL2 Assets",
                FontSize = 13,
            };
            var resources = new ResourceDictionary();
            var sut = new FontService(options);
            var events = new List<(
                int ThreadId,
                int ResourceCount,
                FlourishFontChangedEventArgs Args
            )>();
            sut.Changed += (_, args) =>
                events.Add(
                    (Environment.CurrentManagedThreadId, resources.Count, args)
                );

            using var attachPosted = new ManualResetEventSlim();
            dispatcher.Hooks.OperationPosted += OnAttachPosted;
            var attachTask = Task.Run(() => sut.Attach(dispatcher, resources));
            Assert.True(
                attachPosted.Wait(TimeSpan.FromSeconds(5)),
                "Attach did not queue work on the target dispatcher."
            );
            dispatcher.Hooks.OperationPosted -= OnAttachPosted;

            var detachedSetterThreadId = 0;
            var detachedSetter = Task.Run(() =>
            {
                detachedSetterThreadId = Environment.CurrentManagedThreadId;
                sut.SetFont("Arial", 18);
            });
            detachedSetter.GetAwaiter().GetResult();

            var detachedEvent = Assert.Single(events);
            Assert.Equal(detachedSetterThreadId, detachedEvent.ThreadId);
            Assert.NotEqual(dispatcherThreadId, detachedEvent.ThreadId);
            Assert.Equal(0, detachedEvent.ResourceCount);
            Assert.Empty(resources);
            Assert.Equal("Arial", sut.FontFamily);
            Assert.Equal(18d, sut.FontSize);

            PumpDispatcherUntil(dispatcher, attachTask);

            Assert.Equal(
                "Arial",
                Assert.IsType<FontFamily>(resources[TextFamilyKey]).Source
            );
            Assert.Equal(18d, resources["FlourishFontSizeBase"]);
            Assert.Equal(sut.FontFamily, detachedEvent.Args.FontFamily);
            Assert.Equal(sut.FontSize, detachedEvent.Args.FontSize);

            using var attachedMutationPosted = new ManualResetEventSlim();
            dispatcher.Hooks.OperationPosted += OnAttachedMutationPosted;
            var attachedSetter = Task.Run(() => sut.SetFont("Consolas", 19));
            Assert.True(
                attachedMutationPosted.Wait(TimeSpan.FromSeconds(5)),
                "The attached mutation did not queue on the target dispatcher."
            );
            dispatcher.Hooks.OperationPosted -= OnAttachedMutationPosted;

            Assert.Equal("Arial", sut.FontFamily);
            Assert.Equal(18d, sut.FontSize);
            Assert.Equal(
                "Arial",
                Assert.IsType<FontFamily>(resources[TextFamilyKey]).Source
            );
            Assert.Equal(18d, resources["FlourishFontSizeBase"]);

            PumpDispatcherUntil(dispatcher, attachedSetter);

            Assert.Equal(2, events.Count);
            var attachedEvent = events[^1];
            Assert.Equal(dispatcherThreadId, attachedEvent.ThreadId);
            Assert.Equal("Consolas", sut.FontFamily);
            Assert.Equal(19d, sut.FontSize);
            Assert.Equal(
                "Consolas",
                Assert.IsType<FontFamily>(resources[TextFamilyKey]).Source
            );
            Assert.Equal(19d, resources["FlourishFontSizeBase"]);
            Assert.Equal(sut.FontFamily, attachedEvent.Args.FontFamily);
            Assert.Equal(sut.FontSize, attachedEvent.Args.FontSize);

            void OnAttachPosted(object? sender, DispatcherHookEventArgs e)
            {
                attachPosted.Set();
            }

            void OnAttachedMutationPosted(object? sender, DispatcherHookEventArgs e)
            {
                attachedMutationPosted.Set();
            }
        });
    }

    [Fact]
    public void RootDynamicResourcesUpdatePlainInheritedTextWithoutWindowResourceShadows()
    {
        RunInSta(() =>
        {
            var (sut, resources) = CreateAttachedService();
            var window = new Window();
            window.Resources.MergedDictionaries.Add(resources);
            window.SetResourceReference(WpfControl.FontFamilyProperty, TextFamilyKey);
            window.SetResourceReference(
                WpfControl.FontSizeProperty,
                "FlourishFontSizeBase"
            );
            var text = new TextBlock();
            window.Content = text;
            window.Measure(new Size(400, 300));
            window.Arrange(new Rect(0, 0, 400, 300));

            Assert.Equal(sut.FontFamily, window.FontFamily.Source);
            Assert.Equal(sut.FontFamily, text.FontFamily.Source);
            Assert.Equal(sut.FontSize, window.FontSize);
            Assert.Equal(sut.FontSize, text.FontSize);
            Assert.DoesNotContain(
                TextFamilyKey,
                window.Resources.Keys.Cast<object>()
            );
            Assert.DoesNotContain(
                "FlourishFontSizeBase",
                window.Resources.Keys.Cast<object>()
            );

            sut.SetFont("Arial", 18);

            Assert.Equal("Arial", window.FontFamily.Source);
            Assert.Equal("Arial", text.FontFamily.Source);
            Assert.Equal(18d, window.FontSize);
            Assert.Equal(18d, text.FontSize);
        });
    }

    [Fact]
    public void IconDynamicResourceUpdatesAnExistingTextBlockWithoutRecreation()
    {
        RunInSta(() =>
        {
            var (sut, resources) = CreateAttachedService();
            var window = new Window();
            window.Resources.MergedDictionaries.Add(resources);
            window.SetResourceReference(WpfControl.FontFamilyProperty, TextFamilyKey);
            var icon = new TextBlock { Text = "\uE10F" };
            icon.SetResourceReference(TextBlock.FontFamilyProperty, IconFamilyKey);
            icon.SetResourceReference(
                TextBlock.FontSizeProperty,
                "FlourishFontSizeTitlebarIcon"
            );
            window.Content = icon;
            window.Measure(new Size(400, 300));
            window.Arrange(new Rect(0, 0, 400, 300));
            var originalIcon = icon;

            sut.SetIconFontFamily("Arial");

            Assert.Same(originalIcon, window.Content);
            Assert.Equal("Arial", icon.FontFamily.Source);
            Assert.Equal(14d, icon.FontSize);
            Assert.Equal("Segoe UI", window.FontFamily.Source);
            Assert.DoesNotContain(
                IconFamilyKey,
                window.Resources.Keys.Cast<object>()
            );
        });
    }

    [Fact]
    public void ApplyToPage_IsIdempotentForTheSameEffectiveSignature()
    {
        RunInSta(() =>
        {
            var (sut, resources) = CreateAttachedService();
            var page = CreatePageWithResources(resources);

            Assert.True(sut.ApplyToPage(page));
            Assert.False(sut.ApplyToPage(page));

            sut.SetFont("Arial", 18);

            Assert.Equal("Arial", page.FontFamily.Source);
            Assert.Equal(18d, page.FontSize);
            Assert.False(sut.ApplyToPage(page));
        });
    }

    [Fact]
    public void FamilyOnlyPageOverrideKeepsItsFamilyAndFollowsGlobalSizeWithoutReapply()
    {
        RunInSta(() =>
        {
            var (sut, resources) = CreateAttachedService();
            var page = CreatePageWithResources(resources);
            sut.SetOverrideFont<TestPage>("Arial");

            Assert.True(sut.ApplyToPage(page));
            Assert.Equal("Arial", page.FontFamily.Source);
            Assert.Equal(13d, page.FontSize);

            sut.SetFont("Times New Roman", 18);

            Assert.Equal("Arial", page.FontFamily.Source);
            Assert.Equal(18d, page.FontSize);
            Assert.False(sut.ApplyToPage(page));
        });
    }

    [Fact]
    public void FixedPageOverrideRemainsStableAcrossGlobalTextAndIconChanges()
    {
        RunInSta(() =>
        {
            var (sut, resources) = CreateAttachedService();
            var page = CreatePageWithResources(resources);
            sut.SetOverrideFont<TestPage>("Arial", 18);
            Assert.True(sut.ApplyToPage(page));

            sut.SetFont("Times New Roman", 22);
            sut.SetIconFontFamily("Arial");

            Assert.Equal("Arial", page.FontFamily.Source);
            Assert.Equal(18d, page.FontSize);
            Assert.Equal(18d, page.Resources["FlourishFontSizeBase"]);
            Assert.Equal(
                "Arial",
                Assert.IsType<FontFamily>(page.FindResource(IconFamilyKey)).Source
            );
            Assert.False(sut.ApplyToPage(page));
        });
    }

    [Fact]
    public void ClearPageOverrideRestoresOriginalResourcesAndThenBecomesIdempotent()
    {
        RunInSta(() =>
        {
            var (sut, resources) = CreateAttachedService();
            var page = CreatePageWithResources(resources);
            var originalFamily = new FontFamily("Consolas");
            page.Resources[TextFamilyKey] = originalFamily;
            page.Resources["FlourishFontSizeBase"] = 15d;
            sut.SetOverrideFont<TestPage>("Arial", 18);
            Assert.True(sut.ApplyToPage(page));

            Assert.True(sut.ClearOverrideFont<TestPage>());
            Assert.True(sut.ApplyToPage(page));

            Assert.Same(originalFamily, page.Resources[TextFamilyKey]);
            Assert.Equal("Consolas", page.FontFamily.Source);
            Assert.Equal(15d, page.FontSize);
            Assert.False(sut.ApplyToPage(page));
        });
    }

    [Fact]
    public void SourceContractsUseOneApplicationScopeAndFilterPageOverrideRefreshes()
    {
        var flourishRoot = Path.Combine(FindRepositoryRoot(), "src", "Flourish");
        var fontSource = File.ReadAllText(
            Path.Combine(flourishRoot, "Services", "FontService.cs")
        );
        var shellSource = File.ReadAllText(
            Path.Combine(flourishRoot, "Views", "Windows", "FlourishShellWindow.xaml.cs")
        );
        var runtimeSource = File.ReadAllText(
            Path.Combine(flourishRoot, "Internal", "Composition", "FlourishRuntime.cs")
        );
        var shellXaml = XDocument.Load(
            Path.Combine(flourishRoot, "Views", "Windows", "FlourishShellWindow.xaml")
        );

        Assert.DoesNotContain("window.Resources", fontSource, StringComparison.Ordinal);
        Assert.DoesNotContain("window.FontFamily =", fontSource, StringComparison.Ordinal);
        Assert.DoesNotContain("window.FontSize =", fontSource, StringComparison.Ordinal);
        Assert.DoesNotContain("iconFontFamily", shellSource, StringComparison.Ordinal);
        Assert.DoesNotContain("new FontFamily", shellSource, StringComparison.Ordinal);
        Assert.Contains(
            "textBlock.SetResourceReference(TextBlock.FontFamilyProperty, \"FlourishIconFontFamily\")",
            shellSource,
            StringComparison.Ordinal
        );
        Assert.Equal(7, CountOccurrences(shellSource, "BindIconTypography(icon"));

        var root = Assert.IsType<XElement>(shellXaml.Root);
        Assert.Equal(
            "{DynamicResource FlourishFontFamily}",
            (string?)root.Attribute("FontFamily")
        );
        Assert.Equal(
            "{DynamicResource FlourishFontSizeBase}",
            (string?)root.Attribute("FontSize")
        );

        var fontAttachIndex = runtimeSource.IndexOf(
            "GetRequiredService<FontService>().Attach(application)",
            StringComparison.Ordinal
        );
        var shellResolveIndex = runtimeSource.IndexOf(
            "GetRequiredService<FlourishShellWindow>()",
            StringComparison.Ordinal
        );
        Assert.True(fontAttachIndex >= 0);
        Assert.True(shellResolveIndex > fontAttachIndex);

        var handlerStart = shellSource.IndexOf(
            "private void FontService_Changed",
            StringComparison.Ordinal
        );
        var handlerEnd = shellSource.IndexOf(
            "private void MotionService_Changed",
            handlerStart,
            StringComparison.Ordinal
        );
        Assert.True(handlerStart >= 0 && handlerEnd > handlerStart);
        var handler = shellSource[handlerStart..handlerEnd];
        Assert.Contains(
            "e.ChangeKind != FlourishFontChangeKind.PageOverride",
            handler,
            StringComparison.Ordinal
        );
        Assert.Contains("== affectedPageType", handler, StringComparison.Ordinal);
        Assert.DoesNotContain(
            "FlourishFontChangeKind.GlobalText",
            handler,
            StringComparison.Ordinal
        );
        Assert.DoesNotContain(
            "FlourishFontChangeKind.Icon",
            handler,
            StringComparison.Ordinal
        );
        Assert.Equal(2, CountOccurrences(handler, "fontService.ApplyToPage("));
    }

    private static (FontService Service, ResourceDictionary Resources) CreateAttachedService()
    {
        var options = new FlourishShellOptions();
        options.FontFamily = "Segoe UI";
        options.IconFontFamily = "Segoe MDL2 Assets";
        options.FontSize = 13;
        var resources = new ResourceDictionary();
        var service = new FontService(options);
        service.Attach(Dispatcher.CurrentDispatcher, resources);
        return (service, resources);
    }

    private static TestPage CreatePageWithResources(ResourceDictionary resources)
    {
        var page = new TestPage();
        page.Resources.MergedDictionaries.Add(resources);
        return page;
    }

    private static Dictionary<string, object> CaptureResources(
        ResourceDictionary resources
    )
    {
        return AllKeys.ToDictionary(key => key, key => resources[key]);
    }

    private static void AssertOnlyResourcesChanged(
        IReadOnlyDictionary<string, object> before,
        ResourceDictionary resources,
        params string[] changedKeys
    )
    {
        var expectedChanges = changedKeys.ToHashSet(StringComparer.Ordinal);
        foreach (var key in AllKeys)
        {
            if (expectedChanges.Contains(key))
            {
                Assert.NotSame(before[key], resources[key]);
            }
            else
            {
                Assert.Same(before[key], resources[key]);
            }
        }
    }

    private static int CountOccurrences(string source, string value)
    {
        var count = 0;
        for (
            var index = 0;
            (index = source.IndexOf(value, index, StringComparison.Ordinal)) >= 0;
            index += value.Length
        )
        {
            count++;
        }

        return count;
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

    private sealed class TestPage : Page { }
}
