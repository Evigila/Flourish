using System.Collections.Generic;
using ArkheideSystem.Flourish.Configuration;
using ArkheideSystem.Flourish.Hosting;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace ArkheideSystem.Tests.Flourish.WPF.Hosting;

public sealed class PreferenceLoaderFontTests
{
    [Fact]
    public void Apply_StandardIconValueRestoresTheFinalFontContract()
    {
        var options = new ApplicationOptions();

        PreferenceLoader.Apply(
            CreateConfiguration(standardIcon: "18", obsoleteIcon: null),
            new ApplicationDataOptions { UsePersistedLocale = false },
            options
        );

        Assert.Equal(18d, options.Appearance.FontSizeIcon);
    }

    [Fact]
    public void Apply_ObsoleteIconValueDoesNotRestoreAnUnpublishedContract()
    {
        var options = new ApplicationOptions();

        PreferenceLoader.Apply(
            CreateConfiguration(standardIcon: null, obsoleteIcon: "17"),
            new ApplicationDataOptions { UsePersistedLocale = false },
            options
        );

        Assert.Equal(14d, options.Appearance.FontSizeIcon);
    }

    private static IConfiguration CreateConfiguration(string? standardIcon, string? obsoleteIcon)
    {
        var prefix = PreferenceConfigurationKeys.Font;
        var values = new Dictionary<string, string?>
        {
            [$"{prefix}:Family"] = "Segoe UI",
            [$"{prefix}:IconFamily"] = "Segoe MDL2 Assets",
            [$"{prefix}:Small"] = "11",
            [$"{prefix}:Standard"] = "13",
            [$"{prefix}:Large"] = "14",
            [$"{prefix}:ExtraLarge"] = "18",
            [$"{prefix}:Header"] = "25",
        };
        if (standardIcon is not null)
        {
            values[$"{prefix}:StandardIcon"] = standardIcon;
        }
        if (obsoleteIcon is not null)
        {
            values[$"{prefix}:Icon"] = obsoleteIcon;
        }

        return new ConfigurationBuilder().AddInMemoryCollection(values).Build();
    }
}
