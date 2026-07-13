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
    public void ThemeService_StyleOverridesPopulateApplicationScopeSemanticResources()
    {
        RunInSta(() =>
        {
            var colors = new FlourishThemeColors(
                Color.FromRgb(0x12, 0x34, 0x56),
                Color.FromRgb(0xE8, 0xC5, 0x47),
                Color.FromRgb(0x7D, 0x4C, 0xDB)
            );
            var options = new FlourishShellOptions
            {
                ThemeColors = colors,
                CornerRadius = 5,
            };
            var resources = new ResourceDictionary();

            ThemeService.ApplyStyleOverrides(resources, options);

            Assert.Equal(colors.Primary, Assert.IsType<Color>(resources["FlourishPrimaryColor"]));
            Assert.Equal(
                colors.Secondary,
                Assert.IsType<Color>(resources["FlourishSecondaryColor"])
            );
            Assert.Equal(colors.Accent, Assert.IsType<Color>(resources["FlourishAccentColor"]));
            AssertDirectBrushColor(resources, "FlourishPrimaryBrush", colors.Primary);
            AssertDirectBrushColor(resources, "FlourishPrimaryForegroundBrush", colors.Primary);
            AssertDirectBrushColor(resources, "FlourishPrimaryBackgroundBrush", colors.Primary);
            AssertDirectBrushColor(resources, "FlourishSecondaryBrush", colors.Secondary);
            Assert.NotEqual(
                colors.Secondary,
                Assert.IsType<SolidColorBrush>(
                    resources["FlourishSecondaryForegroundBrush"]
                ).Color
            );
            AssertDirectBrushColor(resources, "FlourishAccentBrush", colors.Accent);
            AssertDirectBrushColor(resources, "FlourishAccentForegroundBrush", colors.Accent);
            AssertDirectBrushColor(resources, "FlourishBrandForegroundBrush", colors.Primary);
            AssertDirectBrushColor(resources, "FlourishBrandBackgroundBrush", colors.Primary);
            AssertDirectBrushColor(resources, "FlourishControlStrokeFocusBrush", colors.Accent);
            AssertDirectBrushColor(resources, "FlourishForegroundOnPrimaryBrush", Colors.White);
            AssertDirectBrushColor(resources, "FlourishForegroundOnSecondaryBrush", Colors.Black);

            var primaryHover = Assert.IsType<SolidColorBrush>(
                resources["FlourishPrimaryHoverBrush"]
            );
            var primaryPressed = Assert.IsType<SolidColorBrush>(
                resources["FlourishPrimaryPressedBrush"]
            );
            var primarySurface = Assert.IsType<SolidColorBrush>(
                resources["FlourishPrimarySurfaceBrush"]
            );
            Assert.NotEqual(colors.Primary, primaryHover.Color);
            Assert.NotEqual(primaryHover.Color, primaryPressed.Color);
            Assert.Equal(0x24, primarySurface.Color.A);
            AssertDirectBrushColor(
                resources,
                "FlourishBrandBackgroundHoverBrush",
                primaryHover.Color
            );
            AssertDirectBrushColor(
                resources,
                "FlourishBrandBackgroundPressedBrush",
                primaryPressed.Color
            );

            foreach (
                var key in new[]
                {
                    "FlourishControlCornerRadius",
                    "FlourishSurfaceCornerRadius",
                    "FlourishOverlayCornerRadius",
                    "FlourishDialogCornerRadius",
                }
            )
            {
                Assert.Equal(new CornerRadius(5), Assert.IsType<CornerRadius>(resources[key]));
            }
            Assert.Equal(
                new CornerRadius(0, 0, 5, 5),
                Assert.IsType<CornerRadius>(
                    resources["FlourishDialogFooterCornerRadius"]
                )
            );
        });
    }

    [Fact]
    public void ThemeService_StyleOverridesDeriveReadableForegroundsForEachTheme()
    {
        RunInSta(() =>
        {
            var colors = new FlourishThemeColors(
                Color.FromRgb(0x0F, 0x6C, 0xBD),
                Color.FromRgb(0x5C, 0x2E, 0x91),
                Color.FromRgb(0xD8, 0x3B, 0x01)
            );
            var options = new FlourishShellOptions { ThemeColors = colors };
            var light = new ResourceDictionary();
            var dark = new ResourceDictionary();

            ThemeService.ApplyStyleOverrides(light, options, FlourishTheme.Light);
            ThemeService.ApplyStyleOverrides(dark, options, FlourishTheme.Dark);

            var lightHoverReveal = Assert.IsType<SolidColorBrush>(
                light["FlourishHoverRevealBrush"]
            );
            var lightPressedReveal = Assert.IsType<SolidColorBrush>(
                light["FlourishPressedRevealBrush"]
            );
            var darkHoverReveal = Assert.IsType<SolidColorBrush>(
                dark["FlourishHoverRevealBrush"]
            );
            var darkPressedReveal = Assert.IsType<SolidColorBrush>(
                dark["FlourishPressedRevealBrush"]
            );

            Assert.Equal(0x59, lightHoverReveal.Color.A);
            Assert.Equal(0x66, lightPressedReveal.Color.A);
            Assert.Equal(0x66, darkHoverReveal.Color.A);
            Assert.Equal(0x73, darkPressedReveal.Color.A);
            AssertDirectBrushColor(
                light,
                "FlourishControlSelectedHoverBrush",
                lightHoverReveal.Color
            );
            AssertDirectBrushColor(
                dark,
                "FlourishControlSelectedHoverBrush",
                darkHoverReveal.Color
            );

            AssertDirectBrushColor(light, "FlourishPrimaryForegroundBrush", colors.Primary);
            Assert.NotEqual(
                colors.Primary,
                Assert.IsType<SolidColorBrush>(
                    dark["FlourishPrimaryForegroundBrush"]
                ).Color
            );
            AssertDirectBrushColor(dark, "FlourishPrimaryBackgroundBrush", colors.Primary);
            Assert.NotEqual(
                Assert.IsType<SolidColorBrush>(light["FlourishPrimaryHoverBrush"]).Color,
                Assert.IsType<SolidColorBrush>(dark["FlourishPrimaryHoverBrush"]).Color
            );
            AssertDirectBrushColor(
                dark,
                "FlourishControlStrokeFocusBrush",
                Assert.IsType<SolidColorBrush>(dark["FlourishAccentForegroundBrush"]).Color
            );
        });
    }

    [Fact]
    public void ThemeService_CustomRevealIntensityPreservesInteractionContrast()
    {
        RunInSta(() =>
        {
            var primaryColors = new[]
            {
                Colors.White,
                Colors.Black,
                Color.FromRgb(0x80, 0x80, 0x80),
                Color.FromRgb(0xFF, 0xD8, 0x00),
                Color.FromRgb(0xD1, 0x34, 0x38),
                Color.FromRgb(0xE4, 0xF3, 0xB6),
            };

            foreach (var theme in new[] { FlourishTheme.Light, FlourishTheme.Dark })
            {
                var isDark = theme == FlourishTheme.Dark;
                var neutralForeground = isDark
                    ? Color.FromRgb(0xF8, 0xF8, 0xFA)
                    : Color.FromRgb(0x1B, 0x1B, 0x1F);
                var neutralBackground = isDark
                    ? Color.FromRgb(0x14, 0x14, 0x14)
                    : Colors.White;
                var controlBackground = isDark
                    ? Color.FromRgb(0x29, 0x29, 0x29)
                    : Colors.White;
                var cardLayer = isDark
                    ? Color.FromArgb(0xF0, 0x2B, 0x2D, 0x31)
                    : Color.FromArgb(0xFA, 0xFF, 0xFF, 0xFF);
                var interactionBackgrounds = isDark
                    ? new[]
                    {
                        neutralBackground,
                        controlBackground,
                        Composite(cardLayer, neutralBackground),
                        Composite(cardLayer, controlBackground),
                    }
                    : new[] { Colors.White };

                foreach (var primary in primaryColors)
                {
                    var resources = new ResourceDictionary();
                    var options = new FlourishShellOptions
                    {
                        ThemeColors = new FlourishThemeColors(
                            primary,
                            Color.FromRgb(0x5C, 0x2E, 0x91),
                            Color.FromRgb(0xD8, 0x3B, 0x01)
                        ),
                    };

                    ThemeService.ApplyStyleOverrides(resources, options, theme);

                    var hoverReveal = Assert.IsType<SolidColorBrush>(
                        resources["FlourishHoverRevealBrush"]
                    ).Color;
                    var pressedReveal = Assert.IsType<SolidColorBrush>(
                        resources["FlourishPressedRevealBrush"]
                    ).Color;
                    foreach (var background in interactionBackgrounds)
                    {
                        AssertOverlayContrast(neutralForeground, hoverReveal, background);
                        AssertOverlayContrast(neutralForeground, pressedReveal, background);
                    }

                    var selectedBackground = Assert.IsType<SolidColorBrush>(
                        resources["FlourishControlSelectedBrush"]
                    ).Color;
                    var selectedForeground = Assert.IsType<SolidColorBrush>(
                        resources["FlourishControlSelectedForegroundBrush"]
                    ).Color;
                    var selectedHover = Assert.IsType<SolidColorBrush>(
                        resources["FlourishControlSelectedHoverBrush"]
                    ).Color;
                    AssertOverlayContrast(
                        selectedForeground,
                        selectedHover,
                        selectedBackground
                    );

                    if (isDark && primary == Colors.White)
                    {
                        Assert.True(hoverReveal.A < 0x66);
                    }
                }
            }
        });
    }

    [Fact]
    public void ThemeService_StyleOverridesWithoutExplicitConfigurationPreserveHostResources()
    {
        RunInSta(() =>
        {
            var resources = new ResourceDictionary();
            var hostBrush = new SolidColorBrush(Color.FromRgb(0x12, 0x34, 0x56));
            resources["FlourishPrimaryBrush"] = hostBrush;

            ThemeService.ApplyStyleOverrides(resources, new FlourishShellOptions());

            Assert.Same(hostBrush, resources["FlourishPrimaryBrush"]);
            Assert.DoesNotContain(
                resources.Keys.Cast<object>(),
                key => Equals(key, "FlourishControlCornerRadius")
            );
        });
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

    private static void AssertDirectBrushColor(
        ResourceDictionary resources,
        string key,
        Color expected
    )
    {
        var brush = Assert.IsType<SolidColorBrush>(resources[key]);
        Assert.Equal(expected, brush.Color);
    }

    private static void AssertOverlayContrast(
        Color foreground,
        Color overlay,
        Color background
    )
    {
        var composited = Composite(overlay, background);
        var contrast = GetContrastRatio(foreground, composited);
        Assert.True(
            contrast >= 4.5,
            $"Expected at least 4.5:1 contrast, but found {contrast:F2}:1 for "
                + $"foreground {foreground}, overlay {overlay}, and background {background}."
        );
    }

    private static Color Composite(Color foreground, Color background)
    {
        var alpha = foreground.A / 255d;
        return Color.FromRgb(
            (byte)Math.Round((foreground.R * alpha) + (background.R * (1 - alpha))),
            (byte)Math.Round((foreground.G * alpha) + (background.G * (1 - alpha))),
            (byte)Math.Round((foreground.B * alpha) + (background.B * (1 - alpha)))
        );
    }

    private static double GetContrastRatio(Color first, Color second)
    {
        var lighter = Math.Max(GetRelativeLuminance(first), GetRelativeLuminance(second));
        var darker = Math.Min(GetRelativeLuminance(first), GetRelativeLuminance(second));
        return (lighter + 0.05) / (darker + 0.05);
    }

    private static double GetRelativeLuminance(Color color)
    {
        return (0.2126 * Linearize(color.R))
            + (0.7152 * Linearize(color.G))
            + (0.0722 * Linearize(color.B));
    }

    private static double Linearize(byte channel)
    {
        var value = channel / 255d;
        return value <= 0.04045
            ? value / 12.92
            : Math.Pow((value + 0.055) / 1.055, 2.4);
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
