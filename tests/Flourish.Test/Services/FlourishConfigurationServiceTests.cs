using ArkheideSystem.Flourish.Services;
using Microsoft.Extensions.Configuration;

namespace ArkheideSystem.Flourish.Test.Services;

public sealed class FlourishConfigurationServiceTests
{
    [Fact]
    public void CurrentAndTypedReads_UseEffectiveConfiguration()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(
                new Dictionary<string, string?>
                {
                    ["Feature:Enabled"] = "true",
                    ["Feature:Count"] = "3",
                }
            )
            .Build();
        using var sut = new FlourishConfigurationService(configuration);

        Assert.Equal("true", sut["Feature:Enabled"]);
        Assert.True(sut.Get<bool>("Feature:Enabled"));
        var section = sut.GetSection<FeatureOptions>("Feature");
        Assert.NotNull(section);
        Assert.True(section.Enabled);
        Assert.Equal(3, section.Count);
        Assert.Equal("3", sut.Current["Feature:Count"]);
        Assert.True(sut.Current.Version > 0);
    }

    [Fact]
    public void Reload_CapturesImmutableSnapshotAndRaisesChangedKeys()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["Feature:Value"] = "before" })
            .Build();
        using var sut = new FlourishConfigurationService(configuration);
        var original = sut.Current;
        FlourishConfigurationChangedEventArgs? change = null;
        sut.Changed += (_, args) => change = args;

        configuration["Feature:Value"] = "after";
        sut.Reload();

        Assert.NotNull(change);
        Assert.Equal("before", original["Feature:Value"]);
        Assert.Equal("after", change.Current["Feature:Value"]);
        Assert.Contains("Feature:Value", change.ChangedKeys);
        Assert.True(change.Current.Version > original.Version);
    }

    private sealed class FeatureOptions
    {
        public bool Enabled { get; set; }

        public int Count { get; set; }
    }
}
