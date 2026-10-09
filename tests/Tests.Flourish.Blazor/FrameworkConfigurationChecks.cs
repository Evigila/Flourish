using ArkheideSystem.Flourish.Abstract;
using ArkheideSystem.Flourish.Blazor;
using ArkheideSystem.Flourish.Blazor.Abstract;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

internal static class FrameworkConfigurationChecks
{
    internal static void Register(List<(string Name, Func<Task> Run)> tests)
    {
        tests.Add(("framework loads file defaults below host overrides and passes them to optional integrations", () =>
        {
            WithSettings(configuration =>
            {
                configuration.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["Flourish:Appearance:Accent"] = "#123456",
                    ["Business:Connection"] = "host-owned"
                });
                var services = new ServiceCollection();
                IConfiguration? received = null;
                services.AddFlourishFramework(configuration, framework => framework.ConfigureServices((_, settings) => received = settings));
                Require(received!["Flourish:Localization:DefaultCulture"] == "pt-BR", "An integration lost the file defaults.");
                Require(received["Flourish:Appearance:Accent"] == "#123456", "File defaults overrode host settings.");
                Require(configuration["Business:Connection"] == "host-owned", "Framework loading changed business settings.");
            });
            return Task.CompletedTask;
        }));
        tests.Add(("design uses shared framework defaults while explicit API choices take precedence", () =>
        {
            WithSettings(configuration =>
            {
                var services = new ServiceCollection();
                services.AddFlourishFramework(configuration);
                services.AddFlourishDesign(configuration, appearance => appearance.SetTheme(ApplicationTheme.Light));
                using var provider = services.BuildServiceProvider();
                using var scope = provider.CreateScope();
                var appearance = scope.ServiceProvider.GetRequiredService<IAppearanceService>().Current;
                Require(appearance.Primary == "#FFFFFF" && appearance.Accent == "#ABCDEF", "Design did not receive file colors.");
                Require(appearance.FontFamily == "system-ui" && appearance.Theme == ApplicationTheme.Light,
                    "Explicit choices or file typography lost precedence.");
            });
            return Task.CompletedTask;
        }));
        tests.Add(("generic integrations run once in order after shell configuration is frozen", () =>
        {
            var services = new ServiceCollection();
            var calls = new List<int>();
            IFrameworkBuilder? retained = null;
            services.AddFlourishFramework(framework =>
            {
                retained = framework;
                framework.ConfigureServices((collection, _) =>
                {
                    try { retained.ConfigureLayout(_ => { }); }
                    catch (InvalidOperationException) { calls.Add(1); }
                    collection.AddSingleton(new RegistrationMarker());
                }).ConfigureServices((collection, _) =>
                {
                    Require(collection.Any(item => item.ServiceType == typeof(RegistrationMarker)), "Integrations ran out of order.");
                    calls.Add(2);
                });
            });
            Require(calls.SequenceEqual([1, 2]), "An integration was duplicated or mutated completed shell configuration.");
            return Task.CompletedTask;
        }));
    }

    private static void WithSettings(Action<ConfigurationManager> test)
    {
        var directory = Path.Combine(Path.GetTempPath(), "framework-settings-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        var path = Path.Combine(directory, "appsettings.Flourish.json");
        try
        {
            File.WriteAllText(path, """
                { "Flourish": { "Localization": { "DefaultCulture": "pt-BR" },
                  "Appearance": { "Primary": "#FFFFFF", "Accent": "#ABCDEF", "Theme": "Dark", "FontFamily": "system-ui" } } }
                """);
            using var configuration = new ConfigurationManager();
            configuration.SetBasePath(directory);
            test(configuration);
        }
        finally
        {
            File.Delete(path);
            Directory.Delete(directory);
        }
    }

    private sealed class RegistrationMarker;
    private static void Require(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }
}
