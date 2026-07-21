using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;
using System.Xml.Linq;
using ArkheideSystem.Flourish.Abstract;
using ArkheideSystem.Flourish.Controls;
using ArkheideSystem.Flourish.Internal.Configuration;
using ArkheideSystem.Flourish.Services;
using ArkheideSystem.Flourish.Themes;
using WpfControl = System.Windows.Controls.Control;

namespace ArkheideSystem.Flourish.Test.Services;

public sealed class FontServicePropagationTests
{
    private const string TextFamilyKey = "FlourishFontFamily";
    private const string IconFamilyKey = "FlourishIconFontFamily";

    private static readonly string[] FontSizeAndLineHeightKeys =
    [
        "FlourishFontSizeSmall",
        "FlourishFontSizeStandard",
        "FlourishFontSizeIcon",
        "FlourishFontSizeLarge",
        "FlourishFontSizeExtraLarge",
        "FlourishFontSizeHeaderSize",
        "FlourishLineHeightSmall",
        "FlourishLineHeightStandard",
        "FlourishLineHeightIcon",
        "FlourishLineHeightLarge",
        "FlourishLineHeightExtraLarge",
        "FlourishLineHeightHeaderSize",
    ];

    private static readonly string[] ScaleKeys = FontSizeAndLineHeightKeys;

    private static readonly string[] AllKeys =
    [
        TextFamilyKey,
        IconFamilyKey,
        .. ScaleKeys,
    ];

    [Fact]
    public void Attach_PopulatesOnlyTheFourteenTypographyKeysAndSameScopeReattachIsStable()
    {
        RunInSta(() =>
        {
            var resources = new ResourceDictionary();
            var sut = new FontService(new FlourishShellOptions());

            sut.Attach(Dispatcher.CurrentDispatcher, resources);
            var before = CaptureResources(resources);

            sut.Attach(Dispatcher.CurrentDispatcher, resources);
            var after = CaptureResources(resources);

            Assert.Equal(14, resources.Count);
            Assert.Equal(AllKeys.Order(), resources.Keys.Cast<string>().Order());
            Assert.All(AllKeys, key => Assert.Same(before[key], after[key]));
            Assert.Equal(12d, resources["FlourishFontSizeSmall"]);
            Assert.Equal(14d, resources["FlourishFontSizeStandard"]);
            Assert.Equal(16d, resources["FlourishFontSizeIcon"]);
            Assert.Equal(16d, resources["FlourishFontSizeLarge"]);
            Assert.Equal(24d, resources["FlourishFontSizeExtraLarge"]);
            Assert.Equal(32d, resources["FlourishFontSizeHeaderSize"]);
            Assert.Equal(14d, resources["FlourishLineHeightSmall"]);
            Assert.Equal(16d, resources["FlourishLineHeightStandard"]);
            Assert.Equal(16d, resources["FlourishLineHeightIcon"]);
            Assert.Equal(20d, resources["FlourishLineHeightLarge"]);
            Assert.Equal(29d, resources["FlourishLineHeightExtraLarge"]);
            Assert.Equal(37d, resources["FlourishLineHeightHeaderSize"]);
        });
    }

    [Fact]
    public void SetFont_WithFamilyOnlyChange_ReplacesOnlyTheTextFamilyResource()
    {
        RunInSta(() =>
        {
            var (sut, resources) = CreateAttachedService();
            var before = CaptureResources(resources);

            sut.SetFont(
                "Arial",
                sut.SmallFontSize,
                sut.StandardFontSize,
                sut.IconFontSize,
                sut.LargeFontSize,
                sut.ExtraLargeFontSize,
                sut.HeaderSizeFontSize
            );

            AssertOnlyResourcesChanged(before, resources, TextFamilyKey);
            Assert.Equal(
                "Arial",
                Assert.IsType<FontFamily>(resources[TextFamilyKey]).Source
            );
        });
    }

    [Fact]
    public void SetFont_WithScaleOnlyChangeReplacesAllExplicitSizeResources()
    {
        RunInSta(() =>
        {
            var (sut, resources) = CreateAttachedService();
            var before = CaptureResources(resources);

            sut.SetFont("Segoe UI", 14, 18, 20, 22, 25, 31);

            AssertOnlyResourcesChanged(before, resources, FontSizeAndLineHeightKeys);
            Assert.Equal(14d, resources["FlourishFontSizeSmall"]);
            Assert.Equal(18d, resources["FlourishFontSizeStandard"]);
            Assert.Equal(20d, resources["FlourishFontSizeIcon"]);
            Assert.Equal(22d, resources["FlourishFontSizeLarge"]);
            Assert.Equal(25d, resources["FlourishFontSizeExtraLarge"]);
            Assert.Equal(31d, resources["FlourishFontSizeHeaderSize"]);
            Assert.Equal(16d, resources["FlourishLineHeightSmall"]);
            Assert.Equal(20d, resources["FlourishLineHeightStandard"]);
            Assert.Equal(20d, resources["FlourishLineHeightIcon"]);
            Assert.Equal(26d, resources["FlourishLineHeightLarge"]);
            Assert.Equal(30d, resources["FlourishLineHeightExtraLarge"]);
            Assert.Equal(36d, resources["FlourishLineHeightHeaderSize"]);
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

            sut.SetFont("Arial", 15, 18, 20, 21, 24, 30);

            AssertOnlyResourcesChanged(
                before,
                resources,
                [TextFamilyKey, .. ScaleKeys]
            );
            Assert.Same(before[IconFamilyKey], resources[IconFamilyKey]);
            Assert.Equal(15d, resources["FlourishFontSizeSmall"]);
            Assert.Equal(18d, resources["FlourishFontSizeStandard"]);
            Assert.Equal(20d, resources["FlourishFontSizeIcon"]);
            Assert.Equal(21d, resources["FlourishFontSizeLarge"]);
            Assert.Equal(24d, resources["FlourishFontSizeExtraLarge"]);
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

            sut.SetFont(
                sut.FontFamily,
                sut.SmallFontSize,
                sut.StandardFontSize,
                sut.IconFontSize,
                sut.LargeFontSize,
                sut.ExtraLargeFontSize,
                sut.HeaderSizeFontSize
            );
            sut.SetIconFontFamily(sut.IconFontFamily);

            Assert.Empty(events);

            sut.SetOverrideFont<TestPage>("Arial", 15, 18, 20, 21, 24, 30);
            sut.SetOverrideFont<TestPage>("Arial", 15, 18, 20, 21, 24, 30);
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

            sut.SetFont(
                "Arial",
                sut.SmallFontSize,
                sut.StandardFontSize,
                sut.IconFontSize,
                sut.LargeFontSize,
                sut.ExtraLargeFontSize,
                sut.HeaderSizeFontSize
            );
            sut.SetFont("Times New Roman", 14, 19, 21, 24, 28, 34);
            sut.SetIconFontFamily("Arial");
            sut.SetOverrideFont<TestPage>("Arial", 15, 18, 20, 21, 24, 30);
            sut.ClearOverrideFont<TestPage>();

            Assert.Equal(
                [
                    FlourishFontChangeKind.GlobalText,
                    FlourishFontChangeKind.GlobalText,
                    FlourishFontChangeKind.Icon,
                    FlourishFontChangeKind.PageOverride,
                    FlourishFontChangeKind.PageOverride,
                ],
                events.Select(args => args.ChangeKind)
            );
            Assert.All(events.Take(3), args => Assert.Null(args.AffectedPageType));
            Assert.All(
                events.Skip(3),
                args => Assert.Equal(typeof(TestPage), args.AffectedPageType)
            );
            Assert.Equal(14d, events[1].SmallFontSize);
            Assert.Equal(19d, events[1].StandardFontSize);
            Assert.Equal(21d, events[1].IconFontSize);
            Assert.Equal(24d, events[1].LargeFontSize);
            Assert.Equal(28d, events[1].ExtraLargeFontSize);
            Assert.Equal(34d, events[1].HeaderSizeFontSize);
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
                FontSizeSmall = 11,
                FontSizeStandard = 13,
                FontSizeIcon = 15,
                FontSizeLarge = 17,
                FontSizeExtraLarge = 19,
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
                () => sut.SetFont("Arial", 12, 16, 18, 20, 23, 29),
                () => sut.SetIconFontFamily("Arial"),
                () => sut.SetOverrideFont<TestPage>("Arial", 14, 18, 20, 22, 26, 32),
                () => sut.SetFont("Consolas", 15, 19, 21, 24, 29, 35),
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
            Assert.Equal(15d, options.FontSizeSmall);
            Assert.Equal(19d, options.FontSizeStandard);
            Assert.Equal(21d, options.FontSizeIcon);
            Assert.Equal(24d, options.FontSizeLarge);
            Assert.Equal(29d, options.FontSizeExtraLarge);
            Assert.Equal(35d, options.FontSizeHeaderSize);
            Assert.Equal(options.FontFamily, sut.FontFamily);
            Assert.Equal(options.IconFontFamily, sut.IconFontFamily);
            Assert.Equal(options.FontSizeSmall, sut.SmallFontSize);
            Assert.Equal(options.FontSizeStandard, sut.StandardFontSize);
            Assert.Equal(options.FontSizeIcon, sut.IconFontSize);
            Assert.Equal(options.FontSizeLarge, sut.LargeFontSize);
            Assert.Equal(options.FontSizeExtraLarge, sut.ExtraLargeFontSize);
            Assert.Equal(options.FontSizeHeaderSize, sut.HeaderSizeFontSize);
            Assert.Equal(last.FontFamily, options.FontFamily);
            Assert.Equal(last.IconFontFamily, options.IconFontFamily);
            Assert.Equal(last.SmallFontSize, options.FontSizeSmall);
            Assert.Equal(last.StandardFontSize, options.FontSizeStandard);
            Assert.Equal(last.IconFontSize, options.FontSizeIcon);
            Assert.Equal(last.LargeFontSize, options.FontSizeLarge);
            Assert.Equal(last.ExtraLargeFontSize, options.FontSizeExtraLarge);
            Assert.Equal(last.HeaderSizeFontSize, options.FontSizeHeaderSize);
            Assert.Equal(
                options.FontFamily,
                Assert.IsType<FontFamily>(resources[TextFamilyKey]).Source
            );
            Assert.Equal(
                options.IconFontFamily,
                Assert.IsType<FontFamily>(resources[IconFamilyKey]).Source
            );
            Assert.Equal(options.FontSizeSmall, resources["FlourishFontSizeSmall"]);
            Assert.Equal(options.FontSizeStandard, resources["FlourishFontSizeStandard"]);
            Assert.Equal(options.FontSizeIcon, resources["FlourishFontSizeIcon"]);
            Assert.Equal(24d, resources["FlourishFontSizeLarge"]);
            Assert.Equal(29d, resources["FlourishFontSizeExtraLarge"]);
            Assert.Equal(35d, resources["FlourishFontSizeHeaderSize"]);
            Assert.Equal(
                new FlourishPageFontOverride("Arial", 14, 18, 20, 22, 26, 32),
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
                FontSizeSmall = 11,
                FontSizeStandard = 13,
                FontSizeIcon = 15,
                FontSizeLarge = 17,
                FontSizeExtraLarge = 19,
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
                sut.SetFont("Arial", 14, 18, 20, 22, 25, 31);
            });
            detachedSetter.GetAwaiter().GetResult();

            var detachedEvent = Assert.Single(events);
            Assert.Equal(detachedSetterThreadId, detachedEvent.ThreadId);
            Assert.NotEqual(dispatcherThreadId, detachedEvent.ThreadId);
            Assert.Equal(0, detachedEvent.ResourceCount);
            Assert.Empty(resources);
            Assert.Equal("Arial", sut.FontFamily);
            Assert.Equal(14d, sut.SmallFontSize);
            Assert.Equal(18d, sut.StandardFontSize);
            Assert.Equal(20d, sut.IconFontSize);
            Assert.Equal(22d, sut.LargeFontSize);
            Assert.Equal(25d, sut.ExtraLargeFontSize);

            PumpDispatcherUntil(dispatcher, attachTask);

            Assert.Equal(
                "Arial",
                Assert.IsType<FontFamily>(resources[TextFamilyKey]).Source
            );
            Assert.Equal(14d, resources["FlourishFontSizeSmall"]);
            Assert.Equal(18d, resources["FlourishFontSizeStandard"]);
            Assert.Equal(20d, resources["FlourishFontSizeIcon"]);
            Assert.Equal(22d, resources["FlourishFontSizeLarge"]);
            Assert.Equal(25d, resources["FlourishFontSizeExtraLarge"]);
            Assert.Equal(sut.FontFamily, detachedEvent.Args.FontFamily);
            Assert.Equal(sut.SmallFontSize, detachedEvent.Args.SmallFontSize);
            Assert.Equal(sut.StandardFontSize, detachedEvent.Args.StandardFontSize);
            Assert.Equal(sut.IconFontSize, detachedEvent.Args.IconFontSize);
            Assert.Equal(sut.LargeFontSize, detachedEvent.Args.LargeFontSize);
            Assert.Equal(sut.ExtraLargeFontSize, detachedEvent.Args.ExtraLargeFontSize);
            Assert.Equal(sut.HeaderSizeFontSize, detachedEvent.Args.HeaderSizeFontSize);

            using var attachedMutationPosted = new ManualResetEventSlim();
            dispatcher.Hooks.OperationPosted += OnAttachedMutationPosted;
            var attachedSetter = Task.Run(() =>
                sut.SetFont("Consolas", 15, 19, 21, 24, 28, 34)
            );
            Assert.True(
                attachedMutationPosted.Wait(TimeSpan.FromSeconds(5)),
                "The attached mutation did not queue on the target dispatcher."
            );
            dispatcher.Hooks.OperationPosted -= OnAttachedMutationPosted;

            Assert.Equal("Arial", sut.FontFamily);
            Assert.Equal(14d, sut.SmallFontSize);
            Assert.Equal(18d, sut.StandardFontSize);
            Assert.Equal(20d, sut.IconFontSize);
            Assert.Equal(22d, sut.LargeFontSize);
            Assert.Equal(25d, sut.ExtraLargeFontSize);
            Assert.Equal(
                "Arial",
                Assert.IsType<FontFamily>(resources[TextFamilyKey]).Source
            );
            Assert.Equal(18d, resources["FlourishFontSizeStandard"]);

            PumpDispatcherUntil(dispatcher, attachedSetter);

            Assert.Equal(2, events.Count);
            var attachedEvent = events[^1];
            Assert.Equal(dispatcherThreadId, attachedEvent.ThreadId);
            Assert.Equal("Consolas", sut.FontFamily);
            Assert.Equal(15d, sut.SmallFontSize);
            Assert.Equal(19d, sut.StandardFontSize);
            Assert.Equal(21d, sut.IconFontSize);
            Assert.Equal(24d, sut.LargeFontSize);
            Assert.Equal(28d, sut.ExtraLargeFontSize);
            Assert.Equal(
                "Consolas",
                Assert.IsType<FontFamily>(resources[TextFamilyKey]).Source
            );
            Assert.Equal(19d, resources["FlourishFontSizeStandard"]);
            Assert.Equal(sut.FontFamily, attachedEvent.Args.FontFamily);
            Assert.Equal(sut.SmallFontSize, attachedEvent.Args.SmallFontSize);
            Assert.Equal(sut.StandardFontSize, attachedEvent.Args.StandardFontSize);
            Assert.Equal(sut.IconFontSize, attachedEvent.Args.IconFontSize);
            Assert.Equal(sut.LargeFontSize, attachedEvent.Args.LargeFontSize);
            Assert.Equal(sut.ExtraLargeFontSize, attachedEvent.Args.ExtraLargeFontSize);
            Assert.Equal(sut.HeaderSizeFontSize, attachedEvent.Args.HeaderSizeFontSize);

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
                "FlourishFontSizeStandard"
            );
            var text = new TextBlock();
            window.Content = text;
            window.Measure(new Size(400, 300));
            window.Arrange(new Rect(0, 0, 400, 300));

            Assert.Equal(sut.FontFamily, window.FontFamily.Source);
            Assert.Equal(sut.FontFamily, text.FontFamily.Source);
            Assert.Equal(sut.StandardFontSize, window.FontSize);
            Assert.Equal(sut.StandardFontSize, text.FontSize);
            Assert.DoesNotContain(
                TextFamilyKey,
                window.Resources.Keys.Cast<object>()
            );
            Assert.DoesNotContain(
                "FlourishFontSizeStandard",
                window.Resources.Keys.Cast<object>()
            );

            sut.SetFont("Arial", 14, 18, 20, 22, 25, 31);

            Assert.Equal("Arial", window.FontFamily.Source);
            Assert.Equal("Arial", text.FontFamily.Source);
            Assert.Equal(18d, window.FontSize);
            Assert.Equal(18d, text.FontSize);
            Assert.Equal(14d, window.FindResource("FlourishFontSizeSmall"));
            Assert.Equal(20d, window.FindResource("FlourishFontSizeIcon"));
            Assert.Equal(22d, window.FindResource("FlourishFontSizeLarge"));
            Assert.Equal(25d, window.FindResource("FlourishFontSizeExtraLarge"));
        });
    }

    [Fact]
    public void AttachedGlobalScaleUpdatesExistingChunkAndHeroTitleHostsAtDistinctTiers()
    {
        RunInSta(() =>
        {
            var resources = new ResourceDictionary();
            resources.MergedDictionaries.Add(new FlourishThemeResources());
            var sut = new FontService(new FlourishShellOptions());
            sut.Attach(Dispatcher.CurrentDispatcher, resources);

            WpfControl[] controls =
            [
                new Chunk { Title = "Section" },
                new ChunkHero { Title = "Hero" },
            ];
            var panel = new StackPanel();
            foreach (var control in controls)
            {
                panel.Children.Add(control);
            }

            var window = new Window { Content = panel };
            window.Resources.MergedDictionaries.Add(resources);
            try
            {
                window.Show();
                window.UpdateLayout();

                var titles = controls
                    .Select(control =>
                    {
                        control.ApplyTemplate();
                        return Assert.IsType<FlourishTextBlock>(
                            control.Template.FindName("TitleHost", control)
                        );
                    })
                    .ToArray();

                Assert.Equal(24d, titles[0].FontSize);
                Assert.Equal(32d, titles[1].FontSize);
                Assert.All(titles, title => Assert.Equal(FontWeights.Bold, title.FontWeight));

                sut.SetFont("Segoe UI", 11, 15, 17, 23, 29, 35);

                Assert.Equal(29d, titles[0].FontSize);
                Assert.Equal(35d, titles[1].FontSize);
                Assert.All(titles, title => Assert.Equal(FontWeights.Bold, title.FontWeight));
            }
            finally
            {
                window.Close();
            }
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
                "FlourishFontSizeIcon"
            );
            window.Content = icon;
            window.Measure(new Size(400, 300));
            window.Arrange(new Rect(0, 0, 400, 300));
            var originalIcon = icon;

            sut.SetIconFontFamily("Arial");
            sut.SetFont(
                sut.FontFamily,
                sut.SmallFontSize,
                sut.StandardFontSize,
                18,
                sut.LargeFontSize,
                sut.ExtraLargeFontSize,
                sut.HeaderSizeFontSize
            );

            Assert.Same(originalIcon, window.Content);
            Assert.Equal("Arial", icon.FontFamily.Source);
            Assert.Equal(18d, icon.FontSize);
            Assert.Equal("Segoe UI", window.FontFamily.Source);
            Assert.DoesNotContain(
                IconFamilyKey,
                window.Resources.Keys.Cast<object>()
            );
            Assert.DoesNotContain(
                "FlourishFontSizeIcon",
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

            sut.SetFont("Arial", 14, 18, 20, 22, 25, 31);

            Assert.Equal("Arial", page.FontFamily.Source);
            Assert.Equal(18d, page.FontSize);
            Assert.False(sut.ApplyToPage(page));
        });
    }

    [Fact]
    public void FamilyOnlyPageOverrideKeepsItsFamilyAndFollowsGlobalScaleWithoutReapply()
    {
        RunInSta(() =>
        {
            var (sut, resources) = CreateAttachedService();
            var page = CreatePageWithResources(resources);
            sut.SetOverrideFont<TestPage>("Arial", null, null, null, null, null, null);

            Assert.True(sut.ApplyToPage(page));
            Assert.Equal("Arial", page.FontFamily.Source);
            Assert.Equal(13d, page.FontSize);

            sut.SetFont("Times New Roman", 14, 18, 20, 22, 26, 32);

            Assert.Equal("Arial", page.FontFamily.Source);
            Assert.Equal(18d, page.FontSize);
            Assert.Equal(14d, page.FindResource("FlourishFontSizeSmall"));
            Assert.Equal(20d, page.FindResource("FlourishFontSizeIcon"));
            Assert.Equal(22d, page.FindResource("FlourishFontSizeLarge"));
            Assert.Equal(26d, page.FindResource("FlourishFontSizeExtraLarge"));
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
            sut.SetOverrideFont<TestPage>("Arial", 14, 18, 20, 22, 26, 32);
            Assert.True(sut.ApplyToPage(page));

            sut.SetFont("Times New Roman", 17, 22, 24, 27, 31, 37);
            sut.SetIconFontFamily("Arial");

            Assert.Equal("Arial", page.FontFamily.Source);
            Assert.Equal(18d, page.FontSize);
            Assert.Equal(14d, page.Resources["FlourishFontSizeSmall"]);
            Assert.Equal(18d, page.Resources["FlourishFontSizeStandard"]);
            Assert.Equal(20d, page.Resources["FlourishFontSizeIcon"]);
            Assert.Equal(22d, page.Resources["FlourishFontSizeLarge"]);
            Assert.Equal(26d, page.Resources["FlourishFontSizeExtraLarge"]);
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
            page.Resources["FlourishFontSizeSmall"] = 12d;
            page.Resources["FlourishFontSizeStandard"] = 15d;
            page.Resources["FlourishFontSizeIcon"] = 17d;
            page.Resources["FlourishFontSizeLarge"] = 19d;
            page.Resources["FlourishFontSizeExtraLarge"] = 22d;
            sut.SetOverrideFont<TestPage>("Arial", 14, 18, 20, 22, 25, 31);
            Assert.True(sut.ApplyToPage(page));

            Assert.True(sut.ClearOverrideFont<TestPage>());
            Assert.True(sut.ApplyToPage(page));

            Assert.Same(originalFamily, page.Resources[TextFamilyKey]);
            Assert.Equal("Consolas", page.FontFamily.Source);
            Assert.Equal(15d, page.FontSize);
            Assert.Equal(12d, page.Resources["FlourishFontSizeSmall"]);
            Assert.Equal(17d, page.Resources["FlourishFontSizeIcon"]);
            Assert.Equal(19d, page.Resources["FlourishFontSizeLarge"]);
            Assert.Equal(22d, page.Resources["FlourishFontSizeExtraLarge"]);
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
        var statusItemSource = File.ReadAllText(
            Path.Combine(
                flourishRoot,
                "Internal",
                "Interaction",
                "StatusItemViewCache.cs"
            )
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
        Assert.DoesNotContain("new FontFamily", statusItemSource, StringComparison.Ordinal);
        Assert.Contains(
            "textBlock.SetResourceReference(TextBlock.FontFamilyProperty, \"FlourishIconFontFamily\")",
            shellSource,
            StringComparison.Ordinal
        );
        Assert.Contains(
            "\"FlourishIconFontFamily\"",
            statusItemSource,
            StringComparison.Ordinal
        );
        Assert.Equal(5, CountOccurrences(shellSource, "BindIconTypography(icon"));

        var root = Assert.IsType<XElement>(shellXaml.Root);
        Assert.Equal(
            "{DynamicResource FlourishFontFamily}",
            (string?)root.Attribute("FontFamily")
        );
        Assert.Equal(
            "{DynamicResource FlourishFontSizeStandard}",
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
            "e.ChangeKind == FlourishFontChangeKind.Icon",
            handler,
            StringComparison.Ordinal
        );
        Assert.Contains("affectedPageType is null", handler, StringComparison.Ordinal);
        Assert.Contains("== affectedPageType", handler, StringComparison.Ordinal);
        Assert.Equal(2, CountOccurrences(handler, "fontService.ApplyToPage("));
    }

    private static (FontService Service, ResourceDictionary Resources) CreateAttachedService()
    {
        var options = new FlourishShellOptions();
        options.FontFamily = "Segoe UI";
        options.IconFontFamily = "Segoe MDL2 Assets";
        options.FontSizeSmall = 11;
        options.FontSizeStandard = 13;
        options.FontSizeIcon = 15;
        options.FontSizeLarge = 17;
        options.FontSizeExtraLarge = 19;
        options.FontSizeHeaderSize = 25;
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
