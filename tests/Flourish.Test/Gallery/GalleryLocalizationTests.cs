using System.IO;
using System.Text.Json;
using System.Text.RegularExpressions;
using ArkheideSystem.Flourish.Abstract.Essential;
using ArkheideSystem.Flourish.Test.Infrastructure;
using ArkheideSystem.Gallery.Localization;
using ArkheideSystem.Gallery.Models;
using Moq;

namespace ArkheideSystem.Flourish.Test.Gallery;

public sealed class GalleryLocalizationTests
{
    [Fact]
    public void ChineseCatalog_TranslatesStableKeysAndFormatsValues()
    {
        var localization = CreateFlourishLocalization("zh-CN");
        var sut = new GalleryLocalizationService(localization.Object);

        Assert.Equal("\u6982\u89c8", sut.Get(GalleryLocaleKeys.ApplicationOverview_D4B1EA57));
        Assert.Equal(
            "\u7ec4\u4ef6\u53c2\u8003",
            sut.Get(GalleryLocaleKeys.ApplicationComponentReference_661E6097)
        );
        Assert.Equal(
            "\u533a\u57df\u8bbe\u7f6e\u5df2\u5207\u6362\u4e3a zh-CN\u3002",
            sut.Format(GalleryLocaleKeys.DynamicLocaleChangedTo0_1C2A91ED, "zh-CN")
        );
    }

    [Fact]
    public void EnglishAndUnknownLocales_FallBackToEnglishAndExposeUnknownKeys()
    {
        var english = new GalleryLocalizationService(
            CreateFlourishLocalization("en-US").Object
        );
        var spanish = new GalleryLocalizationService(
            CreateFlourishLocalization("es-ES").Object
        );

        Assert.Equal("Overview", english.Get(GalleryLocaleKeys.ApplicationOverview_D4B1EA57));
        Assert.Equal("Overview", spanish.Get(GalleryLocaleKeys.ApplicationOverview_D4B1EA57));
        Assert.Equal("Gallery.Test.Missing", spanish.Get("Gallery.Test.Missing"));
    }

    [Fact]
    public void Apply_ResolvesAStableXamlKeyWhenTheLocaleChanges()
    {
        StaTest.Run(() =>
        {
            var locale = "zh-CN";
            var flourish = new Mock<IFlourishLocalization>();
            flourish.SetupGet(service => service.CurrentLocale).Returns(() => locale);
            var sut = new GalleryLocalizationService(flourish.Object);
            var text = new System.Windows.Controls.TextBlock
            {
                Text = GalleryLocaleKeys.ApplicationOverview_D4B1EA57,
            };

            sut.Apply(text);
            Assert.Equal("\u6982\u89c8", text.Text);

            locale = "en-US";
            sut.Apply(text);
            Assert.Equal("Overview", text.Text);
        });
    }

    [Fact]
    public void Apply_RefreshesDataBoundControlMemberDescriptionsFromStableKeys()
    {
        StaTest.Run(() =>
        {
            var flourish = CreateFlourishLocalization("zh-CN");
            var sut = new GalleryLocalizationService(flourish.Object);
            var member = new ControlMemberRow(
                "Variant",
                GalleryLocaleKeys.ControlsSelectsVisualEmphasisAndSemanticFeedback_00B96251
            );
            var grid = new System.Windows.Controls.DataGrid
            {
                ItemsSource = new[] { member },
            };

            sut.Apply(grid);

            Assert.Equal(
                "\u9009\u62e9\u89c6\u89c9\u5f3a\u8c03\u7a0b\u5ea6\u548c\u8bed\u4e49\u53cd\u9988\u3002",
                member.Description
            );
            Assert.Equal("Variant", member.Name);
        });
    }

    [Fact]
    public void GalleryCatalogs_MatchTheDeclaredKeySetAndPlaceholderContract()
    {
        var repository = FindRepositoryRoot();
        var catalogDirectory = Path.Combine(
            repository,
            "src",
            "Gallery",
            "Localization",
            "Catalogs"
        );
        var catalogNames = Directory
            .GetFiles(catalogDirectory, "*.json")
            .Select(path => Path.GetFileName(path)!)
            .OrderBy(name => name, StringComparer.Ordinal)
            .ToArray();

        Assert.Equal(new[] { "en-US.gallery.json", "zh-CN.gallery.json" }, catalogNames);

        var english = ReadCatalog(Path.Combine(catalogDirectory, "en-US.gallery.json"));
        var chinese = ReadCatalog(Path.Combine(catalogDirectory, "zh-CN.gallery.json"));
        var declaredKeys = GalleryLocaleKeys.All
            .OrderBy(key => key, StringComparer.Ordinal)
            .ToArray();

        Assert.Equal(declaredKeys.Length, declaredKeys.Distinct(StringComparer.Ordinal).Count());
        Assert.True(
            declaredKeys.Length >= 1400,
            $"Expected broad Gallery coverage; found {declaredKeys.Length} keys."
        );
        Assert.Equal(
            declaredKeys,
            english.Keys.OrderBy(key => key, StringComparer.Ordinal).ToArray()
        );
        Assert.Equal(
            declaredKeys,
            chinese.Keys.OrderBy(key => key, StringComparer.Ordinal).ToArray()
        );

        foreach (var key in declaredKeys)
        {
            Assert.StartsWith("Gallery.", key, StringComparison.Ordinal);
            Assert.False(string.IsNullOrWhiteSpace(english[key]));
            Assert.False(string.IsNullOrWhiteSpace(chinese[key]));
            Assert.DoesNotContain('\uFFFD', english[key]);
            Assert.DoesNotContain('\uFFFD', chinese[key]);
            Assert.Equal(
                GetPlaceholderIndexes(english[key]),
                GetPlaceholderIndexes(chinese[key])
            );
        }
    }

    [Fact]
    public void GalleryKeys_RemainOutsideFlourishAssetsAndXamlUsesDeclaredKeys()
    {
        var repository = FindRepositoryRoot();
        var declaredKeys = GalleryLocaleKeys.All.ToHashSet(StringComparer.Ordinal);
        var viewDirectory = Path.Combine(repository, "src", "Gallery", "Views");
        var xaml = string.Join(
            Environment.NewLine,
            Directory.GetFiles(viewDirectory, "*.xaml").Select(File.ReadAllText)
        );
        var xamlKeys = Regex
            .Matches(xaml, "(?<==\")Gallery\\.[A-Za-z0-9_.]+(?=\")")
            .Select(match => match.Value)
            .Distinct(StringComparer.Ordinal)
            .ToArray();

        Assert.NotEmpty(xamlKeys);
        Assert.All(xamlKeys, key => Assert.Contains(key, declaredKeys));

        var flourishAssetDirectory = Path.Combine(repository, "src", "Flourish", "Assets");
        foreach (var path in Directory.GetFiles(flourishAssetDirectory, "lang_*.json"))
        {
            Assert.DoesNotContain("Gallery.", File.ReadAllText(path), StringComparison.Ordinal);
        }

        var commandsPage = File.ReadAllText(Path.Combine(viewDirectory, "CommandsPage.xaml"));
        Assert.Contains("Text=\"gallery.runtime.greet\"", commandsPage, StringComparison.Ordinal);
        var appearancePage = File.ReadAllText(Path.Combine(viewDirectory, "AppearancePage.xaml"));
        Assert.Contains("theme.SetTheme", appearancePage, StringComparison.Ordinal);
    }

    [Fact]
    public void Program_RegistersClientLocalizationWithoutAddingGalleryCatalogsToFlourish()
    {
        var repository = FindRepositoryRoot();
        var program = File.ReadAllText(Path.Combine(repository, "src", "Gallery", "Program.cs"));
        var project = File.ReadAllText(
            Path.Combine(repository, "src", "Gallery", "Gallery.csproj")
        );

        Assert.Contains(
            "AddSingleton<GalleryLocalizationService>()",
            program,
            StringComparison.Ordinal
        );
        Assert.Contains("GalleryShellLocalizationCoordinator", program, StringComparison.Ordinal);
        Assert.Contains("Localization\\Catalogs\\*.json", project, StringComparison.Ordinal);
        Assert.DoesNotContain("data.AddLocaleFile", program, StringComparison.Ordinal);
    }

    [Fact]
    public void ShellCoordinator_LocalizesStableKeysWithoutChangingNavigationIdentity()
    {
        var localization = new Mock<IGalleryLocalization>();
        localization
            .Setup(service => service.Get(It.IsAny<string>()))
            .Returns((string key) => $"zh:{key}");

        var sourceItem = FlourishNavigationMenuItem.Page(
            "appearance-item",
            "Appearance",
            GalleryLocaleKeys.ApplicationAppearance_3907FA7F,
            "icon"
        );
        var navigation = new Mock<INavigationMenuService>();
        navigation
            .SetupGet(service => service.Current)
            .Returns(
                new FlourishNavigationMenuSnapshot(
                    [
                        new FlourishNavigationMenuGroup(
                            "group:1",
                            GalleryLocaleKeys.ApplicationConfiguration_B332C349,
                            [sourceItem]
                        ),
                    ],
                    [],
                    1
                )
            );
        var editor = new Mock<INavigationMenuEditor>();
        string? localizedGroup = null;
        FlourishNavigationMenuItem? localizedItem = null;
        editor
            .Setup(value => value.SetGroupTitle("group:1", It.IsAny<string>()))
            .Callback((string _, string? title) => localizedGroup = title);
        editor
            .Setup(value =>
                value.SetItem(
                    "appearance-item",
                    It.IsAny<
                        Func<FlourishNavigationMenuItem, FlourishNavigationMenuItem>
                    >()
                )
            )
            .Callback(
                (
                    string _,
                    Func<FlourishNavigationMenuItem, FlourishNavigationMenuItem> update
                ) => localizedItem = update(sourceItem)
            );
        navigation
            .Setup(service => service.Set(It.IsAny<Action<INavigationMenuEditor>>()))
            .Callback((Action<INavigationMenuEditor> update) => update(editor.Object));

        var titleBar = new Mock<ITitleBarService>();
        var search = new Mock<ITitleBarSearchService>();
        var toolbar = new Mock<IToolbarService>();
        toolbar
            .SetupGet(service => service.Current)
            .Returns(
                new FlourishToolbarSnapshot(
                    false,
                    [],
                    new Dictionary<Type, FlourishPageToolbarSnapshot>(),
                    1
                )
            );
        var sut = new GalleryShellLocalizationCoordinator(
            localization.Object,
            navigation.Object,
            titleBar.Object,
            search.Object,
            toolbar.Object
        );

        sut.Start();

        Assert.Equal(
            $"zh:{GalleryLocaleKeys.ApplicationConfiguration_B332C349}",
            localizedGroup
        );
        Assert.NotNull(localizedItem);
        Assert.Equal(
            $"zh:{GalleryLocaleKeys.ApplicationAppearance_3907FA7F}",
            localizedItem.Label
        );
        Assert.Equal(sourceItem.Id, localizedItem.Id);
        Assert.Equal(sourceItem.NavigationKey, localizedItem.NavigationKey);
        Assert.Equal(sourceItem.IconGlyph, localizedItem.IconGlyph);
        titleBar.Verify(service =>
            service.SetApplicationSubTitle(
                $"zh:{GalleryLocaleKeys.ApplicationComponentReference_661E6097}"
            )
        );
        titleBar.Verify(service =>
            service.SetUnnamedProjectPlaceholder(
                $"zh:{GalleryLocaleKeys.ApplicationUntitledProject_1B5A65C3}"
            )
        );
        search.Verify(service =>
            service.SetPlaceholder(
                $"zh:{GalleryLocaleKeys.ApplicationTypeHereToSearch_85717255}"
            )
        );
    }

    private static Dictionary<string, string> ReadCatalog(string path)
    {
        using var document = JsonDocument.Parse(File.ReadAllText(path));
        Assert.Equal(JsonValueKind.Object, document.RootElement.ValueKind);
        return document.RootElement
            .EnumerateObject()
            .ToDictionary(
                property => property.Name,
                property => Assert.IsType<string>(property.Value.GetString()),
                StringComparer.Ordinal
            );
    }

    private static Mock<IFlourishLocalization> CreateFlourishLocalization(string locale)
    {
        var localization = new Mock<IFlourishLocalization>();
        localization.SetupGet(service => service.CurrentLocale).Returns(locale);
        return localization;
    }

    private static string[] GetPlaceholderIndexes(string value)
    {
        return Regex
            .Matches(value, @"(?<!\{)\{(?<index>[0-9]+)(?:[^}]*)\}(?!\})")
            .Select(match => match.Groups["index"].Value)
            .OrderBy(index => index, StringComparer.Ordinal)
            .ToArray();
    }

    private static string FindRepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null)
        {
            if (
                Directory.Exists(Path.Combine(directory.FullName, "src", "Gallery"))
                && Directory.Exists(Path.Combine(directory.FullName, "src", "Flourish"))
            )
            {
                return directory.FullName;
            }

            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException("Repository root could not be located.");
    }
}
