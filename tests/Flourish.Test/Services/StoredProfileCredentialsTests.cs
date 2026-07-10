using ArkheideSystem.Flourish.Abstract;
using ArkheideSystem.Flourish.Services;

namespace ArkheideSystem.Flourish.Test.Services;

public sealed class StoredProfileCredentialsTests
{
    [Fact]
    public void TryGetName_WithLegacyFirstLastPayload_MigratesCombinedName()
    {
        var sut = new StoredProfileCredentials
        {
            UserName = "Mary Jane Watson",
            Password = "password",
            RememberLogin = true,
        };

        var succeeded = sut.TryGetName(NameOrder.FirstLast, out var name);

        Assert.True(succeeded);
        Assert.Equal("Mary Jane", name.FirstName);
        Assert.Equal("Watson", name.LastName);
    }

    [Fact]
    public void TryGetName_WithLegacyLastFirstPayload_PreservesDisplayOrder()
    {
        var sut = new StoredProfileCredentials
        {
            UserName = "Watson Mary Jane",
            Password = "password",
            RememberLogin = true,
        };

        var succeeded = sut.TryGetName(NameOrder.LastFirst, out var name);

        Assert.True(succeeded);
        Assert.Equal("Mary Jane", name.FirstName);
        Assert.Equal("Watson", name.LastName);
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
        Assert.Null(sut.UserName);
        Assert.True(sut.RememberLogin);
    }

    [Fact]
    public void TryGetName_WithFutureSchema_DoesNotInterpretPayload()
    {
        var sut = new StoredProfileCredentials
        {
            SchemaVersion = StoredProfileCredentials.CurrentSchemaVersion + 1,
            FirstName = "Future",
            Password = "password",
            RememberLogin = true,
        };

        Assert.True(sut.UsesFutureSchema);
        Assert.False(sut.TryGetName(NameOrder.FirstLast, out _));
    }

    [Fact]
    public void TryGetName_WithInvalidLegacyPayload_IsNotFutureSchema()
    {
        var sut = new StoredProfileCredentials
        {
            Password = "password",
            RememberLogin = true,
        };

        Assert.False(sut.UsesFutureSchema);
        Assert.False(sut.TryGetName(NameOrder.FirstLast, out _));
    }
}
