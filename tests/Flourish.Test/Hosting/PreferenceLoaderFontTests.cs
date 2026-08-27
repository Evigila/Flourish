using System.Collections.Generic;
using ArkheideSystem.Flourish.Configuration;
using ArkheideSystem.Flourish.Hosting;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace ArkheideSystem.Flourish.Test.Hosting;

public sealed class PreferenceLoaderFontTests
{
    [Theory]
    [InlineData("22", 14d)]
    [InlineData("17", 17d)]
    public void Apply_LegacyIconValueMigratesOnlyTheFormerDefault(
        string persistedIcon,
        double expectedStandardIcon
    )
    {
        var options = new ApplicationOptions();

        PreferenceLoader.Apply(
            CreateConfiguration(persistedIcon, standardIcon: null),
            new ApplicationDataOptions { UsePersistedLocale = false },
            options
        );

        Assert.Equal(expectedStandardIcon, options.Appearance.FontSizeIcon);
    }

    [Fact]
    public void Apply_StandardIconValueTakesPriorityOverTheCompatibilityValue()
    {
        var options = new ApplicationOptions();

        PreferenceLoader.Apply(
            CreateConfiguration(persistedIcon: "22", standardIcon: "18"),
            new ApplicationDataOptions { UsePersistedLocale = false },
            options
        );

        Assert.Equal(18d, options.Appearance.FontSizeIcon);
    }

    private static IConfiguration CreateConfiguration(string persistedIcon, string? standardIcon)
    {
        var prefix = PreferenceConfigurationKeys.Font;
        var values = new Dictionary<string, string?>
        {
            [$"{prefix}:Family"] = "Segoe UI",
            [$"{prefix}:IconFamily"] = "Segoe MDL2 Assets",
            [$"{prefix}:Small"] = "11",
            [$"{prefix}:Standard"] = "13",
            [$"{prefix}:Icon"] = persistedIcon,
            [$"{prefix}:Large"] = "14",
            [$"{prefix}:ExtraLarge"] = "18",
            [$"{prefix}:Header"] = "25",
        };
        if (standardIcon is not null)
        {
            values[$"{prefix}:StandardIcon"] = standardIcon;
        }

        return new ConfigurationBuilder().AddInMemoryCollection(values).Build();
    }
}
