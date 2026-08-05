using System.IO;
using System.Text.Json;
using System.Text.RegularExpressions;
using ArkheideSystem.Flourish.Abstract.Essential;
using ArkheideSystem.Gallery.Localization;
using ArkheideSystem.Gallery.Models;
using ArkheideSystem.Flourish.Test.Infrastructure;
using Moq;

namespace ArkheideSystem.Flourish.Test.Gallery;

public sealed class GalleryLocalizationTests
{
    [Fact]
    public void ChineseCatalog_TranslatesGalleryOwnedText()
    {
        var localization = CreateFlourishLocalization("zh-CN");
        var sut = new GalleryLocalizationService(localization.Object);

        Assert.Equal("概览", sut.Get("Overview"));
        Assert.Equal("组件参考", sut.Get("Component reference"));
        Assert.Equal("区域设置已切换为 zh-CN。", sut.Format("Locale changed to {0}.", "zh-CN"));
    }

    [Fact]
    public void EnglishAndUnknownCatalogs_FallBackToSourceText()
    {
        var english = CreateFlourishLocalization("en-US");
        var spanish = CreateFlourishLocalization("es-ES");

        Assert.Equal(
            "Overview",
            new GalleryLocalizationService(english.Object).Get("Overview")
        );
        Assert.Equal(
            "Overview",
            new GalleryLocalizationService(spanish.Object).Get("Overview")
        );
    }

    [Fact]
    public void Apply_RestoresTheOriginalLiteralWhenLocaleReturnsToEnglish()
    {
        StaTest.Run(() =>
        {
            var locale = "zh-CN";
            var flourish = new Mock<IFlourishLocalization>();
            flourish.SetupGet(service => service.CurrentLocale).Returns(() => locale);
            var sut = new GalleryLocalizationService(flourish.Object);
            var text = new System.Windows.Controls.TextBlock { Text = "Overview" };

            sut.Apply(text);
            Assert.Equal("概览", text.Text);

            locale = "en-US";
            sut.Apply(text);
            Assert.Equal("Overview", text.Text);
        });
    }

    [Fact]
    public void Apply_RefreshesDataBoundControlMemberDescriptions()
    {
        StaTest.Run(() =>
        {
            var flourish = CreateFlourishLocalization("zh-CN");
            var sut = new GalleryLocalizationService(flourish.Object);
            var member = new ControlMemberRow(
                "Variant",
                "Selects visual emphasis and semantic feedback."
            );
            var grid = new System.Windows.Controls.DataGrid
            {
                ItemsSource = new[] { member },
            };

            sut.Apply(grid);

            Assert.Equal("选择视觉强调程度和语义反馈。", member.Description);
            Assert.Equal("Variant", member.Name);
        });
    }

    [Fact]
    public void GalleryCatalogs_AreValidAndRemainOutsideFlourishAssets()
    {
        var repository = FindRepositoryRoot();
        var catalogDirectory = Path.Combine(
            repository,
            "src",
            "Gallery",
            "Localization",
            "Catalogs"
        );
        var catalogs = Directory.GetFiles(catalogDirectory, "*.json");
        Assert.NotEmpty(catalogs);
        var merged = new Dictionary<string, string>(StringComparer.Ordinal);

        foreach (var path in catalogs)
        {
            using var document = JsonDocument.Parse(File.ReadAllText(path));
            Assert.Equal(JsonValueKind.Object, document.RootElement.ValueKind);
            foreach (var property in document.RootElement.EnumerateObject())
            {
                Assert.False(string.IsNullOrWhiteSpace(property.Name));
                var localized = Assert.IsType<string>(property.Value.GetString());
                Assert.False(string.IsNullOrWhiteSpace(localized));
                Assert.DoesNotContain('\uFFFD', localized);
                Assert.Equal(
                    GetPlaceholderIndexes(property.Name),
                    GetPlaceholderIndexes(localized)
                );
                if (merged.TryGetValue(property.Name, out var existing))
                {
                    Assert.Equal(existing, localized);
                }
                else
                {
                    merged.Add(property.Name, localized);
                }
            }
        }

        Assert.True(merged.Count >= 1200, $"Expected broad Gallery coverage; found {merged.Count} entries.");

        var flourishChinese = File.ReadAllText(
            Path.Combine(repository, "src", "Flourish", "Assets", "lang_zh-CN.json")
        );
        Assert.DoesNotContain("Gallery.", flourishChinese, StringComparison.Ordinal);
    }

    [Fact]
    public void Program_RegistersClientLocalizationWithoutAddingGalleryLocaleFilesToFlourish()
    {
        var repository = FindRepositoryRoot();
        var program = File.ReadAllText(Path.Combine(repository, "src", "Gallery", "Program.cs"));
        var project = File.ReadAllText(
            Path.Combine(repository, "src", "Gallery", "Gallery.csproj")
        );

        Assert.Contains("AddSingleton<GalleryLocalizationService>()", program, StringComparison.Ordinal);
        Assert.Contains("GalleryShellLocalizationCoordinator", program, StringComparison.Ordinal);
        Assert.Contains("Localization\\Catalogs\\*.json", project, StringComparison.Ordinal);
        Assert.DoesNotContain("data.AddLocaleFile", program, StringComparison.Ordinal);
    }

    [Fact]
    public void ShellCoordinator_LocalizesLabelsWithoutChangingNavigationIdentity()
    {
        var localization = new Mock<IGalleryLocalization>();
        localization
            .Setup(service => service.Get(It.IsAny<string>()))
            .Returns((string source) => $"zh:{source}");

        var sourceItem = FlourishNavigationMenuItem.Page(
            "appearance-item",
            "Appearance",
            "Appearance",
            "icon"
        );
        var navigation = new Mock<INavigationMenuService>();
        navigation
            .SetupGet(service => service.Current)
            .Returns(
                new FlourishNavigationMenuSnapshot(
                    [new FlourishNavigationMenuGroup("group:1", "Configuration", [sourceItem])],
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

        Assert.Equal("zh:Configuration", localizedGroup);
        Assert.NotNull(localizedItem);
        Assert.Equal("zh:Appearance", localizedItem.Label);
        Assert.Equal(sourceItem.Id, localizedItem.Id);
        Assert.Equal(sourceItem.NavigationKey, localizedItem.NavigationKey);
        Assert.Equal(sourceItem.IconGlyph, localizedItem.IconGlyph);
        titleBar.Verify(service =>
            service.SetApplicationSubTitle("zh:Component reference")
        );
        titleBar.Verify(service => service.SetUnnamedProjectPlaceholder("zh:Untitled project"));
        search.Verify(service => service.SetPlaceholder("zh:Type here to search"));
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
            .Matches(value, "\\{(?<index>[0-9]+)(?:[^}]*)\\}")
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
