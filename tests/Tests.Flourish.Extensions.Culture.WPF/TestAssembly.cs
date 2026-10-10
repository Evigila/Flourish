using System.IO;
using System.Runtime.CompilerServices;
using ArkheideSystem.Essential.Culture;
using ArkheideSystem.Tests.Flourish.Extensions.Culture.WPF.Texts;
using Xunit;

[assembly: CollectionBehavior(DisableTestParallelization = true)]

namespace ArkheideSystem.Tests.Flourish.Extensions.Culture.WPF;

internal static class TestAssembly
{
    [ModuleInitializer]
    internal static void ConfigureDesktopCatalog()
    {
        // Configure before any test/provider subscription can start the process-wide facade.
        var paths = CultureResources.Files.Select(path => Path.Combine(AppContext.BaseDirectory, path));
        Localizer.Configure(LocalizationCatalog.FromFiles(paths, CultureResources.FallbackCulture,
            new CatalogLoadOptions(enabledCultures: ["en-US", "zh-CN", "pt-BR"])), "en-US");
    }
}
