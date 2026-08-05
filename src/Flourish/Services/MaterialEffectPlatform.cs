using ArkheideSystem.Flourish.Abstract;

namespace ArkheideSystem.Flourish.Services;

internal enum MaterialEffectBackend
{
    Unsupported = -1,
    None = 0,
    SystemMica,
    SystemAcrylic,
    SystemMicaAlt,
    LegacyMica,
    AccentAcrylic,
}

/// <summary>
/// Describes the native material capabilities available on one Windows version.
/// </summary>
internal readonly record struct MaterialEffectPlatform(
    bool IsWindows,
    int MajorVersion,
    int BuildNumber
)
{
    private const int Windows10AcrylicBuild = 17134;
    private const int Windows11Build = 22000;
    private const int Windows11SystemBackdropBuild = 22621;

    internal static MaterialEffectPlatform Current
    {
        get
        {
            if (!OperatingSystem.IsWindows())
            {
                return default;
            }

            var version = Environment.OSVersion.Version;
            return new MaterialEffectPlatform(true, version.Major, version.Build);
        }
    }

    internal static MaterialEffectPlatform FromWindowsVersion(Version version)
    {
        ArgumentNullException.ThrowIfNull(version);
        return new MaterialEffectPlatform(true, version.Major, version.Build);
    }

    internal bool IsWindows10 =>
        IsWindows && MajorVersion == 10 && BuildNumber < Windows11Build;

    internal bool IsWindows11OrLater =>
        IsWindows && MajorVersion == 10 && BuildNumber >= Windows11Build;

    internal bool SupportsSystemBackdrop =>
        IsWindows11OrLater && BuildNumber >= Windows11SystemBackdropBuild;

    internal bool SupportsLegacyMica =>
        IsWindows11OrLater && BuildNumber < Windows11SystemBackdropBuild;

    internal bool SupportsAccentAcrylic =>
        IsWindows
        && MajorVersion == 10
        && BuildNumber >= Windows10AcrylicBuild
        && !SupportsSystemBackdrop;

    internal MaterialEffect DefaultEffect => IsWindows11OrLater
        ? MaterialEffect.Mica
        : IsWindows10 && BuildNumber >= Windows10AcrylicBuild
            ? MaterialEffect.Acrylic
            : MaterialEffect.None;

    internal MaterialEffect Resolve(MaterialEffect effect)
    {
        Validate(effect);
        return effect == MaterialEffect.Auto ? DefaultEffect : effect;
    }

    internal bool IsSupported(MaterialEffect effect)
    {
        return ResolveBackend(effect) != MaterialEffectBackend.Unsupported;
    }

    internal MaterialEffectBackend ResolveBackend(MaterialEffect effect)
    {
        return Resolve(effect) switch
        {
            MaterialEffect.None => MaterialEffectBackend.None,
            MaterialEffect.Mica when SupportsSystemBackdrop =>
                MaterialEffectBackend.SystemMica,
            MaterialEffect.Mica when SupportsLegacyMica => MaterialEffectBackend.LegacyMica,
            MaterialEffect.Acrylic when SupportsSystemBackdrop =>
                MaterialEffectBackend.SystemAcrylic,
            MaterialEffect.Acrylic when SupportsAccentAcrylic =>
                MaterialEffectBackend.AccentAcrylic,
            MaterialEffect.MicaAlt when SupportsSystemBackdrop =>
                MaterialEffectBackend.SystemMicaAlt,
            _ => MaterialEffectBackend.Unsupported,
        };
    }

    internal string DescribeRequirement(MaterialEffect effect)
    {
        return effect switch
        {
            MaterialEffect.Mica => "Windows 11 build 22000 or later",
            MaterialEffect.MicaAlt => "Windows 11 build 22621 or later",
            MaterialEffect.Acrylic =>
                "Windows 10 version 1803 or later, including Windows 11",
            _ => "a supported Windows material platform",
        };
    }

    private static void Validate(MaterialEffect effect)
    {
        if (!Enum.IsDefined(effect))
        {
            throw new ArgumentOutOfRangeException(
                nameof(effect),
                effect,
                "Unknown material effect."
            );
        }
    }
}
