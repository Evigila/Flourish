using ArkheideSystem.Flourish.Abstract;
using ArkheideSystem.Flourish.Composition;
using ArkheideSystem.Flourish.Configuration;
using System.Windows.Controls;

namespace ArkheideSystem.Flourish.Test.Abstract;

public sealed class ProfileUserTests
{
    [Theory]
    [InlineData(NameOrder.FirstLast, "Ada Lovelace", "AL")]
    [InlineData(NameOrder.LastFirst, "Lovelace Ada", "LA")]
    public void Constructor_WithSeparateNameParts_UsesConfiguredOrderForDisplayAndInitials(
        NameOrder nameOrder,
        string expectedDisplayName,
        string expectedInitials
    )
    {
        var sut = new ProfileUser("Ada", "Lovelace", nameOrder);

        Assert.Equal("Ada", sut.FirstName);
        Assert.Equal("Lovelace", sut.LastName);
        Assert.Equal(nameOrder, sut.NameOrder);
        Assert.Equal(expectedDisplayName, sut.DisplayName);
        Assert.Equal(expectedDisplayName, sut.UserName);
        Assert.Equal(expectedInitials, sut.Initials);
    }

    [Theory]
    [InlineData(NameOrder.FirstLast, "Prince", "")]
    [InlineData(NameOrder.LastFirst, "", "Prince")]
    public void Constructor_WithSingleName_DisplaysNameAndOneInitial(
        NameOrder nameOrder,
        string firstName,
        string lastName
    )
    {
        var sut = new ProfileUser(firstName, lastName, nameOrder);

        Assert.Equal("Prince", sut.DisplayName);
        Assert.Equal("P", sut.Initials);
    }

    [Fact]
    public void Constructor_WithUnicodeGrapheme_UsesWholeTextElementForInitial()
    {
        const string firstName = "e\u0301lodie";
        var sut = new ProfileUser(firstName, "\u738B", NameOrder.LastFirst);

        Assert.Equal($"\u738B {firstName}", sut.DisplayName);
        Assert.Equal("\u738BE\u0301", sut.Initials);
    }

    [Theory]
    [InlineData(null, null)]
    [InlineData("", "")]
    [InlineData("   ", "\t")]
    public void Constructor_WithNoName_ThrowsArgumentException(
        string? firstName,
        string? lastName
    )
    {
        Assert.Throws<ArgumentException>(() =>
            new ProfileUser(firstName!, lastName!, NameOrder.FirstLast)
        );
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void CompatibilityConstructor_WithBlankDisplayName_ThrowsArgumentException(
        string? userName
    )
    {
        Assert.Throws<ArgumentException>(() => new ProfileUser(userName!));
    }
}

public sealed class ProfileSignInRequestTests
{
    [Fact]
    public void CompatibilityConstructor_ParsesDisplayNameInFirstLastOrder()
    {
        var sut = new ProfileSignInRequest(
            "Ada Lovelace",
            "legacy-password",
            "avatar.png"
        );

        Assert.Equal("Ada", sut.FirstName);
        Assert.Equal("Lovelace", sut.LastName);
        Assert.Equal(NameOrder.FirstLast, sut.NameOrder);
        Assert.Equal("Ada Lovelace", sut.DisplayName);
        Assert.Equal("Ada Lovelace", sut.UserName);
        Assert.Equal("legacy-password", sut.Password);
        Assert.Equal("avatar.png", sut.ImagePath);
    }

    [Fact]
    public void SeparateNameConstructor_UsesRequestedNameOrder()
    {
        var sut = new ProfileSignInRequest(
            "Ada",
            "Lovelace",
            "new-password",
            NameOrder.LastFirst,
            "avatar.png"
        );

        Assert.Equal("Ada", sut.FirstName);
        Assert.Equal("Lovelace", sut.LastName);
        Assert.Equal(NameOrder.LastFirst, sut.NameOrder);
        Assert.Equal("Lovelace Ada", sut.DisplayName);
        Assert.Equal("Lovelace Ada", sut.UserName);
        Assert.Equal("new-password", sut.Password);
        Assert.Equal("avatar.png", sut.ImagePath);
    }

    [Fact]
    public void ToString_MasksPasswordForBothConstructors()
    {
        const string password = "unique-secret-value";
        ProfileSignInRequest[] requests =
        [
            new("Ada Lovelace", password),
            new("Ada", "Lovelace", password, NameOrder.FirstLast),
        ];

        Assert.All(
            requests,
            request =>
            {
                var result = request.ToString();
                Assert.Equal("ProfileSignInRequest { Password = *** }", result);
                Assert.DoesNotContain(password, result);
            }
        );
    }
}

public sealed class FlourishProfileBuilderTests
{
    [Fact]
    public void SetProfilePage_WithPageType_UpdatesOptionsAndReturnsBuilder()
    {
        var options = new FlourishProfileOptions();
        var sut = new FlourishProfileBuilder(options);

        var result = sut.SetProfilePage<TestProfilePage>();

        Assert.Same(sut, result);
        Assert.Equal(typeof(TestProfilePage), options.PageType);
    }

    [Fact]
    public void SetProfilePage_WithNonPageType_ThrowsArgumentException()
    {
        var options = new FlourishProfileOptions();
        var sut = new FlourishProfileBuilder(options);

        var exception = Assert.Throws<ArgumentException>(() =>
            sut.SetProfilePage(typeof(string))
        );

        Assert.Equal("pageType", exception.ParamName);
    }

    private sealed class TestProfilePage : Page { }
}
