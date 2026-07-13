using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using ArkheideSystem.Flourish.Abstract;
using ArkheideSystem.Flourish.Controls;
using ArkheideSystem.Flourish.Internal.Configuration;
using ArkheideSystem.Flourish.Services;
using ArkheideSystem.Flourish.Themes;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Moq;

namespace ArkheideSystem.Flourish.Test.Services;

public sealed class RuntimeAppearanceServiceTests
{
    [Fact]
    public void FontService_UpdatesSettingsAndRaisesChanged()
    {
        var options = new FlourishShellOptions();
        IFontService sut = new FontService(options);
        FlourishFontChangedEventArgs? change = null;
        sut.Changed += (_, args) => change = args;

        sut.SetFont("Arial", 16);
        sut.SetIconFontFamily("Segoe Fluent Icons");

        Assert.Equal("Arial", sut.FontFamily);
        Assert.Equal(16, sut.FontSize);
        Assert.Equal("Segoe Fluent Icons", sut.IconFontFamily);
        Assert.NotNull(change);
        Assert.Equal("Segoe Fluent Icons", change.IconFontFamily);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    public void FontService_InvalidSize_Throws(double size)
    {
        IFontService sut = new FontService(new FlourishShellOptions());

        Assert.Throws<ArgumentOutOfRangeException>(() => sut.SetFontSize(size));
    }

    [Fact]
    public void FontService_PageOverrideSnapshotsAreImmutableAndChangesAreIdempotent()
    {
        IFontService sut = new FontService(new FlourishShellOptions());
        var changes = 0;
        sut.Changed += (_, _) => changes++;

        sut.SetOverrideFont<RuntimeFontPage>("Consolas");
        var firstSnapshot = sut.PageOverrides;
        sut.SetOverrideFont<RuntimeFontPage>("Consolas");

        Assert.Equal(1, changes);
        var pageOverride = Assert.Single(firstSnapshot);
        Assert.Equal(typeof(RuntimeFontPage), pageOverride.Key);
        Assert.Equal(new FlourishPageFontOverride("Consolas", null), pageOverride.Value);
        Assert.Throws<NotSupportedException>(() =>
            ((IDictionary<Type, FlourishPageFontOverride>)firstSnapshot).Add(
                typeof(SecondRuntimeFontPage),
                new FlourishPageFontOverride("Arial", 15)
            )
        );

        sut.SetOverrideFont(typeof(RuntimeFontPage), "Arial", 16);
        Assert.Equal(2, changes);
        Assert.Equal(16, sut.PageOverrides[typeof(RuntimeFontPage)].FontSize);
        Assert.True(sut.ClearOverrideFont<RuntimeFontPage>());
        Assert.False(sut.ClearOverrideFont(typeof(RuntimeFontPage)));
        Assert.Equal(3, changes);
        Assert.Empty(sut.PageOverrides);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void FontService_PageOverrideRejectsMissingFamily(string? fontFamily)
    {
        IFontService sut = new FontService(new FlourishShellOptions());

        Assert.Throws<ArgumentException>(() =>
            sut.SetOverrideFont<RuntimeFontPage>(fontFamily!)
        );
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(double.NaN)]
    [InlineData(double.PositiveInfinity)]
    public void FontService_PageOverrideRejectsInvalidSize(double fontSize)
    {
        IFontService sut = new FontService(new FlourishShellOptions());

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            sut.SetOverrideFont<RuntimeFontPage>("Arial", fontSize)
        );
    }

    [Fact]
    public void FontService_PageOverrideRejectsInvalidRuntimePageType()
    {
        IFontService sut = new FontService(new FlourishShellOptions());

        Assert.Throws<ArgumentNullException>(() =>
            sut.SetOverrideFont(null!, "Arial")
        );
        Assert.Throws<ArgumentException>(() =>
            sut.SetOverrideFont(typeof(string), "Arial")
        );
        Assert.Throws<ArgumentException>(() =>
            sut.SetOverrideFont(typeof(AbstractRuntimeFontPage), "Arial")
        );
    }

    [Fact]
    public void ToolTipService_EnablesConfiguresAndDisablesAtRuntime()
    {
        var options = new FlourishShellOptions();
        IToolTipService sut = new FlourishToolTipService(options);
        var changes = new List<FlourishToolTipChangedEventArgs>();
        sut.Changed += (_, args) => changes.Add(args);

        sut.Configure(450, 8);
        sut.SetEnabled(false);

        Assert.False(sut.Current.IsEnabled);
        Assert.Equal(450, sut.Current.InitialShowDelayMilliseconds);
        Assert.Equal(8, sut.Current.SpawnableMargin);
        Assert.Equal(2, changes.Count);
        Assert.True(changes[0].Current.IsEnabled);
        Assert.False(changes[1].Current.IsEnabled);
    }

    [Theory]
    [InlineData(-1, 5)]
    [InlineData(0, -1)]
    [InlineData(0, double.NaN)]
    [InlineData(0, double.PositiveInfinity)]
    public void ToolTipService_InvalidSettings_Throw(int delay, double margin)
    {
        IToolTipService sut = new FlourishToolTipService(new FlourishShellOptions());

        Assert.Throws<ArgumentOutOfRangeException>(() => sut.Configure(delay, margin));
    }

    [Fact]
    public void MotionService_UpdatesIndependentRuntimeSettings()
    {
        IMotionService sut = new FlourishMotionService(new FlourishShellOptions());
        var changes = new List<FlourishMotionChangedEventArgs>();
        sut.Changed += (_, args) => changes.Add(args);

        sut.SetEnabled(true);
        sut.SetPageTransition(FlourishPageTransition.Fade, TimeSpan.FromMilliseconds(250));
        sut.SetNavigationPanelTransition(
            FlourishNavigationPanelTransition.None,
            TimeSpan.FromMilliseconds(100)
        );
        sut.SetHoverReveal(true, TimeSpan.FromMilliseconds(90));
        sut.SetRespectSystemReducedMotion(false);

        Assert.True(sut.Current.IsEnabled);
        Assert.Equal(FlourishPageTransition.Fade, sut.Current.PageTransition);
        Assert.Equal(TimeSpan.FromMilliseconds(250), sut.Current.PageTransitionDuration);
        Assert.Equal(
            FlourishNavigationPanelTransition.None,
            sut.Current.NavigationPanelTransition
        );
        Assert.True(sut.Current.IsHoverRevealEnabled);
        Assert.False(sut.Current.RespectSystemReducedMotion);
        Assert.True(sut.CanAnimate);
        Assert.Equal(5, changes.Count);
    }

    [Fact]
    public void MotionService_InvalidTransitionValues_Throw()
    {
        IMotionService sut = new FlourishMotionService(new FlourishShellOptions());

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            sut.SetPageTransition(
                (FlourishPageTransition)int.MaxValue,
                TimeSpan.FromMilliseconds(1)
            )
        );
        Assert.Throws<ArgumentOutOfRangeException>(() =>
            sut.SetHoverReveal(true, TimeSpan.Zero)
        );
    }

    [Fact]
    public void MaterialEffectService_TracksRequestedRuntimeStateWithoutOwner()
    {
        IMaterialEffectService sut = new MaterialEffectService();
        var changes = new List<FlourishMaterialEffectChangedEventArgs>();
        sut.Changed += (_, args) => changes.Add(args);

        sut.SetEffect(MaterialEffect.Mica);
        sut.SetDarkMode(true);

        Assert.Equal(MaterialEffect.Mica, sut.CurrentEffect);
        Assert.False(sut.IsApplied);
        Assert.True(sut.IsDarkMode);
        Assert.Equal(2, changes.Count);
        Assert.Equal(sut.IsSupported(MaterialEffect.Mica), changes[0].IsSupported);
    }

    [Fact]
    public async Task ThemeService_SetThemeActivatesRuntimeThemeBeforePersistenceCompletes()
    {
        using var directory = new TemporaryDirectory();
        var configuration = new ConfigurationBuilder()
            .SetBasePath(directory.Path)
            .Add(
                new FlourishAppSettingsConfigurationSource
                {
                    Path = "appsettings.json",
                    Optional = true,
                    ReloadOnChange = false,
                    WatchForChanges = false,
                }
            )
            .Build();
        var environment = new Mock<IHostEnvironment>();
        environment.SetupGet(value => value.ContentRootPath).Returns(directory.Path);
        using var preferences = new AppPreferenceService(configuration, environment.Object);
        var options = new FlourishShellOptions();
        IThemeService sut = new ThemeService(options, preferences);
        FlourishThemeChangedEventArgs? change = null;
        sut.ThemeChanged += (_, args) => change = args;
        using var updateEntered = new ManualResetEventSlim();
        using var releaseUpdate = new ManualResetEventSlim();
        var blockingUpdate = preferences
            .UpdateAsync(editor =>
            {
                updateEntered.Set();
                releaseUpdate.Wait();
                editor.Set("Test:Blocker", "completed");
            })
            .AsTask();
        try
        {
            Assert.True(updateEntered.Wait(TimeSpan.FromSeconds(5)));
            sut.SetTheme(FlourishTheme.Dark);

            Assert.Equal(FlourishTheme.Dark, sut.CurrentTheme);
            Assert.Equal(FlourishTheme.Dark, sut.EffectiveTheme);
            Assert.True(sut.IsDark);
            Assert.True(options.IsThemeEnabled);
            Assert.NotNull(change);
            Assert.Equal(FlourishTheme.Dark, change.RequestedTheme);
            Assert.False(blockingUpdate.IsCompleted);
        }
        finally
        {
            releaseUpdate.Set();
        }

        await blockingUpdate;
        await preferences.FlushThemeSavesAsync();
        Assert.Equal(FlourishTheme.Dark, preferences.ReadTheme());
    }

    [Fact]
    public async Task ThemeService_ThemeChangedCanSynchronouslyPersistAnotherSetting()
    {
        using var directory = new TemporaryDirectory();
        var configuration = new ConfigurationBuilder()
            .SetBasePath(directory.Path)
            .Add(
                new FlourishAppSettingsConfigurationSource
                {
                    Path = "appsettings.json",
                    Optional = true,
                    ReloadOnChange = false,
                    WatchForChanges = false,
                }
            )
            .Build();
        var environment = new Mock<IHostEnvironment>();
        environment.SetupGet(value => value.ContentRootPath).Returns(directory.Path);
        using var preferences = new AppPreferenceService(configuration, environment.Object);
        IThemeService sut = new ThemeService(new FlourishShellOptions(), preferences);
        sut.ThemeChanged += (_, _) =>
            preferences
                .SetAsync("Feature:FromThemeChanged", true)
                .AsTask()
                .WaitAsync(TimeSpan.FromSeconds(1))
                .GetAwaiter()
                .GetResult();

        await Task.Run(() => sut.SetTheme(FlourishTheme.Dark))
            .WaitAsync(TimeSpan.FromSeconds(5));
        await preferences
            .FlushThemeSavesAsync()
            .AsTask()
            .WaitAsync(TimeSpan.FromSeconds(5));

        Assert.Equal("True", configuration["Feature:FromThemeChanged"]);
        Assert.Equal(FlourishTheme.Dark, preferences.ReadTheme());
    }

    [Fact]
    public void ThemeService_PaletteSwitchRemainsInsideTheGenericThemeRoot()
    {
        RunInSta(() =>
        {
            const string paletteHostSource =
                "/Flourish;component/Themes/Colors/Colors.xaml";
            const string lightSource =
                "/Flourish;component/Themes/Colors/Colors.Light.xaml";
            const string darkSource =
                "/Flourish;component/Themes/Colors/Colors.Dark.xaml";
            _ = Application.LoadComponent(
                new Uri("/Flourish;component/Themes/Generic.xaml", UriKind.Relative)
            );
            var resources = new ResourceDictionary();
            resources.MergedDictionaries.Add(new FlourishThemeResources());

            ThemeService.ApplyThemePalette(resources, FlourishTheme.Light);
            var paletteHost = Assert.IsType<ResourceDictionary>(
                FindDictionary(resources, lightSource)
            );
            var lightPalette = LoadDictionary(lightSource);
            var darkPalette = LoadDictionary(darkSource);
            var visualRoot = new Grid
            {
                Resources = resources,
            };
            var card = new FlourishCard
            {
                Style = Assert.IsType<Style>(resources[typeof(FlourishCard)]),
            };
            visualRoot.Children.Add(card);
            card.ApplyTemplate();

            AssertPaletteColor(paletteHost, lightPalette, "AppBackgroundBrush");
            AssertPaletteColor(
                paletteHost,
                lightPalette,
                "FlourishNeutralForeground1Brush"
            );
            AssertPaletteColor(paletteHost, lightPalette, "FlourishCardBackgroundBrush");
            AssertBrushColor(card.Background, lightPalette, "FlourishCardBackgroundBrush");
            AssertBrushColor(card.Foreground, lightPalette, "FlourishNeutralForeground1Brush");
            AssertNoTopLevelPalette(resources, lightSource, darkSource);

            ThemeService.ApplyThemePalette(resources, FlourishTheme.Dark);
            Assert.Same(paletteHost, FindDictionary(resources, darkSource));
            Assert.Empty(paletteHost.MergedDictionaries);
            AssertPaletteColor(paletteHost, darkPalette, "AppBackgroundBrush");
            AssertPaletteColor(
                paletteHost,
                darkPalette,
                "FlourishNeutralForeground1Brush"
            );
            AssertPaletteColor(paletteHost, darkPalette, "FlourishCardBackgroundBrush");
            AssertBrushColor(card.Background, darkPalette, "FlourishCardBackgroundBrush");
            AssertBrushColor(card.Foreground, darkPalette, "FlourishNeutralForeground1Brush");
            AssertNoTopLevelPalette(resources, lightSource, darkSource);

            ThemeService.ApplyThemePalette(resources, FlourishTheme.Light);
            Assert.Same(paletteHost, FindDictionary(resources, lightSource));
            Assert.Empty(paletteHost.MergedDictionaries);
            AssertPaletteColor(paletteHost, lightPalette, "AppBackgroundBrush");
            AssertPaletteColor(
                paletteHost,
                lightPalette,
                "FlourishNeutralForeground1Brush"
            );
            AssertPaletteColor(paletteHost, lightPalette, "FlourishCardBackgroundBrush");
            AssertBrushColor(card.Background, lightPalette, "FlourishCardBackgroundBrush");
            AssertBrushColor(card.Foreground, lightPalette, "FlourishNeutralForeground1Brush");
            AssertNoTopLevelPalette(resources, lightSource, darkSource);

            Assert.Single(resources.MergedDictionaries);
            Assert.IsType<FlourishThemeResources>(resources.MergedDictionaries[0]);
            Assert.Null(FindDictionary(resources, paletteHostSource));
        });
    }

    [Fact]
    public void ThemeService_PaletteSwitchPreservesNestedThemeAndWrapperOverrides()
    {
        RunInSta(() =>
        {
            const string lightSource =
                "/Flourish;component/Themes/Colors/Colors.Light.xaml";
            const string darkSource =
                "/Flourish;component/Themes/Colors/Colors.Dark.xaml";
            const string customToken = "FlourishBrandForegroundBrush";
            var resources = new ResourceDictionary();
            var wrapper = new ResourceDictionary();
            var theme = new FlourishThemeResources();
            var customBrush = new SolidColorBrush(Color.FromRgb(0x12, 0x34, 0x56));
            wrapper.MergedDictionaries.Add(theme);
            wrapper[customToken] = customBrush;
            resources.MergedDictionaries.Add(wrapper);
            var lightPalette = LoadDictionary(lightSource);
            var darkPalette = LoadDictionary(darkSource);

            FlourishThemeResources.EnsureMerged(resources);
            ThemeService.ApplyThemePalette(resources, FlourishTheme.Light);
            var paletteHost = Assert.IsType<ResourceDictionary>(
                FindDictionary(resources, lightSource)
            );

            Assert.Same(wrapper, Assert.Single(resources.MergedDictionaries));
            Assert.Same(theme, Assert.Single(wrapper.MergedDictionaries));
            Assert.Same(theme, FlourishThemeResources.FindThemeRoot(resources));
            Assert.Same(customBrush, resources[customToken]);
            AssertBrushColor(
                Assert.IsAssignableFrom<Brush>(resources["AppBackgroundBrush"]),
                lightPalette,
                "AppBackgroundBrush"
            );

            ThemeService.ApplyThemePalette(resources, FlourishTheme.Dark);

            Assert.Same(paletteHost, FindDictionary(resources, darkSource));
            Assert.Same(theme, FlourishThemeResources.FindThemeRoot(resources));
            Assert.Same(customBrush, resources[customToken]);
            AssertBrushColor(
                Assert.IsAssignableFrom<Brush>(resources["AppBackgroundBrush"]),
                darkPalette,
                "AppBackgroundBrush"
            );

            ThemeService.ApplyThemePalette(resources, FlourishTheme.Light);

            Assert.Same(paletteHost, FindDictionary(resources, lightSource));
            Assert.Same(theme, FlourishThemeResources.FindThemeRoot(resources));
            Assert.Same(customBrush, resources[customToken]);
            AssertBrushColor(
                Assert.IsAssignableFrom<Brush>(resources["AppBackgroundBrush"]),
                lightPalette,
                "AppBackgroundBrush"
            );
            Assert.Same(wrapper, Assert.Single(resources.MergedDictionaries));
            Assert.Same(theme, Assert.Single(wrapper.MergedDictionaries));
        });
    }

    private static ResourceDictionary LoadDictionary(string source)
    {
        return new ResourceDictionary
        {
            Source = new Uri(source, UriKind.Relative),
        };
    }

    private static void AssertPaletteColor(
        ResourceDictionary actual,
        ResourceDictionary expected,
        string key
    )
    {
        var actualBrush = Assert.IsType<SolidColorBrush>(actual[key]);
        var expectedBrush = Assert.IsType<SolidColorBrush>(expected[key]);
        Assert.Equal(expectedBrush.Color, actualBrush.Color);
    }

    private static void AssertBrushColor(
        Brush actual,
        ResourceDictionary expected,
        string key
    )
    {
        var actualBrush = Assert.IsType<SolidColorBrush>(actual);
        var expectedBrush = Assert.IsType<SolidColorBrush>(expected[key]);
        Assert.Equal(expectedBrush.Color, actualBrush.Color);
    }

    private static ResourceDictionary? FindDictionary(
        ResourceDictionary dictionary,
        string source
    )
    {
        if (dictionary.Source?.OriginalString == source)
        {
            return dictionary;
        }

        foreach (var mergedDictionary in dictionary.MergedDictionaries)
        {
            var result = FindDictionary(mergedDictionary, source);
            if (result is not null)
            {
                return result;
            }
        }

        return null;
    }

    private static void AssertNoTopLevelPalette(
        ResourceDictionary resources,
        params string[] paletteSources
    )
    {
        Assert.DoesNotContain(
            resources.MergedDictionaries,
            dictionary => paletteSources.Contains(dictionary.Source?.OriginalString)
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

    private sealed class TemporaryDirectory : IDisposable
    {
        public TemporaryDirectory()
        {
            Path = System.IO.Path.Combine(
                System.IO.Path.GetTempPath(),
                "Flourish.Test",
                Guid.NewGuid().ToString("N")
            );
            Directory.CreateDirectory(Path);
        }

        public string Path { get; }

        public void Dispose()
        {
            if (Directory.Exists(Path))
            {
                Directory.Delete(Path, recursive: true);
            }
        }
    }

    private sealed class RuntimeFontPage : System.Windows.Controls.Page { }

    private sealed class SecondRuntimeFontPage : System.Windows.Controls.Page { }

    private abstract class AbstractRuntimeFontPage : System.Windows.Controls.Page { }
}
