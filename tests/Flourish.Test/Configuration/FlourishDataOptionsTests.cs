using System.IO;
using ArkheideSystem.Flourish.Configuration;

namespace ArkheideSystem.Flourish.Test.Configuration;

public sealed class FlourishDataOptionsTests
{
    [Fact]
    public void GetRequiredAppPreferenceDataPath_WithExplicitPath_ReturnsExplicitPath()
    {
        var options = new FlourishDataOptions
        {
            AppPreferenceDataPath = @"C:\Portable\Flourish",
        };

        var path = options.GetRequiredAppPreferenceDataPath(new FlourishShellOptions());

        Assert.Equal(@"C:\Portable\Flourish", path);
    }

    [Fact]
    public void GetRequiredAppPreferenceDataPath_WithoutExplicitPath_CombinesApplicationDataParts()
    {
        var options = new FlourishDataOptions
        {
            AppName = "Flourish Gallery",
            CompanyName = "Arkheide System",
        };

        var path = options.GetRequiredAppPreferenceDataPath(new FlourishShellOptions());

        Assert.Equal(
            Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "Arkheide System",
                "Flourish Gallery"
            ),
            path
        );
    }

    [Fact]
    public void GetRequiredAppName_WithExplicitName_ReturnsExplicitName()
    {
        var options = new FlourishDataOptions { AppName = "Flourish Gallery" };
        var shellOptions = new FlourishShellOptions { Title = "Window title" };

        var appName = options.GetRequiredAppName(shellOptions);

        Assert.Equal("Flourish Gallery", appName);
    }

    [Fact]
    public void GetRequiredAppName_WithoutExplicitName_UsesShellTitle()
    {
        var options = new FlourishDataOptions();
        var shellOptions = new FlourishShellOptions { Title = "Window title" };

        var appName = options.GetRequiredAppName(shellOptions);

        Assert.Equal("Window title", appName);
    }

    [Fact]
    public void GetRequiredAppName_WithoutConfiguredName_ThrowsHelpfulException()
    {
        var options = new FlourishDataOptions();

        var exception = Assert.Throws<InvalidOperationException>(() =>
            options.GetRequiredAppName(new FlourishShellOptions())
        );

        Assert.Contains("SetAppName", exception.Message);
    }

    [Fact]
    public void GetRequiredCompanyName_WithoutConfiguredCompany_ThrowsHelpfulException()
    {
        var options = new FlourishDataOptions();

        var exception = Assert.Throws<InvalidOperationException>(
            options.GetRequiredCompanyName
        );

        Assert.Contains("SetAppCompany", exception.Message);
    }
}
