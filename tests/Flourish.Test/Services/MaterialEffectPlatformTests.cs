using ArkheideSystem.Flourish.Abstract;
using ArkheideSystem.Flourish.Services;

namespace ArkheideSystem.Flourish.Test.Services;

public sealed class MaterialEffectPlatformTests
{
    [Fact]
    public void MaterialEffect_PreservesExistingValuesAndAddsTheCompleteBackdropFamily()
    {
        Assert.Equal(0, (int)MaterialEffect.None);
        Assert.Equal(1, (int)MaterialEffect.Mica);
        Assert.Equal(2, (int)MaterialEffect.Acrylic);
        Assert.Equal(3, (int)MaterialEffect.MicaAlt);
        Assert.Equal(4, (int)MaterialEffect.Auto);
    }

    [Theory]
    [InlineData(6, 1, 7601, MaterialEffect.None)]
    [InlineData(10, 0, 16299, MaterialEffect.None)]
    [InlineData(10, 0, 17134, MaterialEffect.Acrylic)]
    [InlineData(10, 0, 19045, MaterialEffect.Acrylic)]
    [InlineData(10, 0, 22000, MaterialEffect.Mica)]
    [InlineData(10, 0, 22621, MaterialEffect.Mica)]
    public void Auto_ResolvesToThePlatformDefault(
        int major,
        int minor,
        int build,
        MaterialEffect expected
    )
    {
        var platform = MaterialEffectPlatform.FromWindowsVersion(
            new Version(major, minor, build)
        );

        Assert.Equal(expected, platform.DefaultEffect);
        Assert.Equal(expected, platform.Resolve(MaterialEffect.Auto));
        Assert.True(platform.IsSupported(MaterialEffect.Auto));
    }

    [Fact]
    public void Win10_UsesOnlyTheAccentAcrylicBackend()
    {
        var platform = MaterialEffectPlatform.FromWindowsVersion(
            new Version(10, 0, 19045)
        );

        Assert.Equal(
            MaterialEffectBackend.AccentAcrylic,
            platform.ResolveBackend(MaterialEffect.Acrylic)
        );
        Assert.Equal(
            MaterialEffectBackend.Unsupported,
            platform.ResolveBackend(MaterialEffect.Mica)
        );
        Assert.Equal(
            MaterialEffectBackend.Unsupported,
            platform.ResolveBackend(MaterialEffect.MicaAlt)
        );
    }

    [Fact]
    public void InitialWin11_UsesLegacyMicaAndAccentAcrylic()
    {
        var platform = MaterialEffectPlatform.FromWindowsVersion(
            new Version(10, 0, 22000)
        );

        Assert.Equal(
            MaterialEffectBackend.LegacyMica,
            platform.ResolveBackend(MaterialEffect.Mica)
        );
        Assert.Equal(
            MaterialEffectBackend.AccentAcrylic,
            platform.ResolveBackend(MaterialEffect.Acrylic)
        );
        Assert.Equal(
            MaterialEffectBackend.Unsupported,
            platform.ResolveBackend(MaterialEffect.MicaAlt)
        );
    }

    [Fact]
    public void CurrentWin11_UsesAllThreeSystemBackdropMappings()
    {
        var platform = MaterialEffectPlatform.FromWindowsVersion(
            new Version(10, 0, 22621)
        );

        Assert.Equal(
            MaterialEffectBackend.SystemMica,
            platform.ResolveBackend(MaterialEffect.Mica)
        );
        Assert.Equal(
            MaterialEffectBackend.SystemAcrylic,
            platform.ResolveBackend(MaterialEffect.Acrylic)
        );
        Assert.Equal(
            MaterialEffectBackend.SystemMicaAlt,
            platform.ResolveBackend(MaterialEffect.MicaAlt)
        );
    }

    [Fact]
    public void NonWindows_AutoFallsBackToNone()
    {
        var platform = default(MaterialEffectPlatform);

        Assert.Equal(MaterialEffect.None, platform.DefaultEffect);
        Assert.Equal(MaterialEffect.None, platform.Resolve(MaterialEffect.Auto));
        Assert.True(platform.IsSupported(MaterialEffect.Auto));
        Assert.False(platform.IsSupported(MaterialEffect.Acrylic));
        Assert.False(platform.IsSupported(MaterialEffect.Mica));
        Assert.False(platform.IsSupported(MaterialEffect.MicaAlt));
    }
}
