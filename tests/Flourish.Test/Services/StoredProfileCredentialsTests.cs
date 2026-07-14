using ArkheideSystem.Flourish.Services;

namespace ArkheideSystem.Flourish.Test.Services;

public sealed class StoredProfileCredentialsTests
{
    [Fact]
    public void TryGetName_WithCurrentSchema_NormalizesSeparateNameParts()
    {
        var sut = new StoredProfileCredentials
        {
            FirstName = "  Mary Jane ",
            LastName = " Watson  ",
            Password = "password",
            RememberLogin = true,
        };

        var succeeded = sut.TryGetName(out var name);

        Assert.True(succeeded);
        Assert.Equal("Mary Jane", name.FirstName);
        Assert.Equal("Watson", name.LastName);
    }

    [Fact]
    public void TryGetName_WithOnlyOneNamePart_Succeeds()
    {
        var sut = new StoredProfileCredentials
        {
            LastName = "Prince",
            Password = "password",
            RememberLogin = true,
        };

        var succeeded = sut.TryGetName(out var name);

        Assert.True(succeeded);
        Assert.Equal(string.Empty, name.FirstName);
        Assert.Equal("Prince", name.LastName);
    }

    [Fact]
    public void Create_WritesCurrentSchemaAndSeparateNameParts()
    {
        var sut = StoredProfileCredentials.Create(
            "Ada",
            "Lovelace",
            "password",
            "avatar.png",
            rememberLogin: true
        );

        Assert.Equal(StoredProfileCredentials.CurrentSchemaVersion, sut.SchemaVersion);
        Assert.Equal("Ada", sut.FirstName);
        Assert.Equal("Lovelace", sut.LastName);
        Assert.True(sut.IsSupportedSchema);
        Assert.True(sut.RememberLogin);
    }

    [Fact]
    public void TryGetName_WithUnsupportedSchema_DoesNotInterpretPayload()
    {
        var sut = new StoredProfileCredentials
        {
            SchemaVersion = StoredProfileCredentials.CurrentSchemaVersion + 1,
            FirstName = "Future",
            Password = "password",
            RememberLogin = true,
        };

        Assert.False(sut.IsSupportedSchema);
        Assert.False(sut.TryGetName(out _));
    }

    [Fact]
    public void TryGetName_WithEmptyNameParts_Fails()
    {
        var sut = new StoredProfileCredentials
        {
            Password = "password",
            RememberLogin = true,
        };

        Assert.True(sut.IsSupportedSchema);
        Assert.False(sut.TryGetName(out _));
    }
}
