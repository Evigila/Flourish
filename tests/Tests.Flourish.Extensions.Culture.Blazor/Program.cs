using System.Globalization;
using ArkheideSystem.Essential.Culture;
using ArkheideSystem.Essential.Culture.Blazor;
using ArkheideSystem.Flourish.Blazor.Abstract;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using LocalizationService = ArkheideSystem.Essential.Culture.Blazor.ILocalizationService;

namespace ArkheideSystem.Tests.Flourish.Extensions.Culture.Blazor;

internal static class Program
{
    private static readonly LocalizationCatalog AppCatalog = LocalizationCatalog.FromJson("""
        {
          "Shared": { "en-US": "Application", "zh-CN": "应用" },
          "Greeting": { "en-US": "Hello, {0}!", "zh-CN": "你好，{0}！" },
          "Amount": { "en-US": "Amount: {0:N2}", "zh-CN": "金额：{0:N2}" },
          "Pair": { "en-US": "{0} and {1}", "zh-CN": "{0}和{1}" }
        }
        """);
    private static readonly LocalizationCatalog LibraryCatalog = LocalizationCatalog.FromJson("""
        { "Shared": { "en-US": "Library", "zh-CN": "控件库" } }
        """);

    private static int Main()
    {
        (string Name, Action Run)[] cases =
        [
            ("Independent scopes keep separate languages", IndependentScopes),
            ("Catalog identity disambiguates equal tokens", CatalogIdentity),
            ("Missing keys use explicit fallback or token", MissingKeys),
            ("Formatting follows the scoped format culture", Formatting),
            ("Changed subscriptions can be removed", EventLifetime),
            ("Registration is idempotent and scoped", Registration),
            ("Framework default before bridge is replaced", DefaultBeforeBridge),
            ("Framework default after bridge cannot override it", DefaultAfterBridge),
            ("Unregistered catalogs fail explicitly", MissingCatalog),
            ("Formatting and argument errors remain visible", InvalidArguments),
            ("Provider is unavailable without Culture registration", MissingCultureRegistration),
            ("Bridge does not alter ambient thread culture", AmbientCulture)
        ];
        try
        {
            foreach (var test in cases)
            {
                test.Run();
                Console.WriteLine($"PASS: {test.Name}");
            }
            Console.WriteLine($"Passed {cases.Length} bridge checks.");
            return 0;
        }
        catch (Exception error)
        {
            Console.Error.WriteLine(error);
            return 1;
        }
    }

    private static ServiceCollection CreateServices()
    {
        var services = new ServiceCollection();
        services.AddCultureBlazor(builder => builder
            .AddCatalog("App", AppCatalog)
            .AddCatalog("Library", LibraryCatalog)
            .SetDefaultCatalog("App")
            .SetDefaultCulture("en-US")
            .AddSupportedCultures("en-US", "zh-CN")
            .InitializeWith(_ => new LocalizationSelection("en-US", "en-US")));
        return services;
    }

    private static ServiceProvider Build(IServiceCollection services) =>
        services.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true, ValidateOnBuild = true });

    private static void IndependentScopes()
    {
        using var root = Build(CreateServices().AddFlourishCulture());
        using var first = root.CreateScope();
        using var second = root.CreateScope();
        var left = first.ServiceProvider.GetRequiredService<ITextProvider>();
        var right = second.ServiceProvider.GetRequiredService<ITextProvider>();
        first.ServiceProvider.GetRequiredService<LocalizationService>().SetCulture("zh-CN");
        Equal("zh-CN", left.Culture);
        Equal("en-US", right.Culture);
        Equal("应用", left.Get(new TextReference("App", "Key.Shared")));
        Equal("Application", right.Get(new TextReference("App", "Key.Shared")));
        Require(!ReferenceEquals(left, right), "Two scopes shared a provider.");
    }

    private static void CatalogIdentity()
    {
        using var root = Build(CreateServices().AddFlourishCulture());
        using var scope = root.CreateScope();
        var text = scope.ServiceProvider.GetRequiredService<ITextProvider>();
        Equal("Application", text.Get(new TextReference("App", "Key.Shared")));
        Equal("Library", text.Get(new TextReference("Library", "Key.Shared")));
        Equal("Hello, Blazor!", text.Get(new TextReference("App", "Greeting"), "Blazor"));
    }

    private static void MissingKeys()
    {
        using var root = Build(CreateServices().AddFlourishCulture());
        using var scope = root.CreateScope();
        var text = scope.ServiceProvider.GetRequiredService<ITextProvider>();
        Equal("Fallback", text.Get(new TextReference("App", "Missing", "Fallback")));
        Equal("Key.Missing", text.Get(new TextReference("App", "Key.Missing")));
        Equal("", text.Get(new TextReference("App", "Missing", "")));
    }

    private static void Formatting()
    {
        using var root = Build(CreateServices().AddFlourishCulture());
        using var scope = root.CreateScope();
        var selection = scope.ServiceProvider.GetRequiredService<LocalizationService>();
        var text = scope.ServiceProvider.GetRequiredService<ITextProvider>();
        selection.SetCulture("zh-CN", "pt-BR");
        Equal("zh-CN", text.Culture);
        Equal("pt-BR", text.FormatCulture.Name);
        Require(ReferenceEquals(selection.FormatCulture, text.FormatCulture), "The adapter cloned format state.");
        Equal("金额：12.345,67", text.Get(new TextReference("App", "Amount"), 12345.67m));
        Equal("Fallback: 12.345,67", text.Get(new TextReference("App", "Missing", "Fallback: {0:N2}"), 12345.67m));
    }

    private static void EventLifetime()
    {
        using var root = Build(CreateServices().AddFlourishCulture());
        using var scope = root.CreateScope();
        var selection = scope.ServiceProvider.GetRequiredService<LocalizationService>();
        var text = scope.ServiceProvider.GetRequiredService<ITextProvider>();
        var updates = 0;
        EventHandler handler = (sender, _) =>
        {
            Require(ReferenceEquals(sender, selection), "Changed should forward the source directly.");
            updates++;
        };
        text.Changed += handler;
        selection.SetCulture("zh-CN");
        Equal(1, updates);
        text.Changed -= handler;
        selection.SetCulture("en-US");
        Equal(1, updates);
    }

    private static void Registration()
    {
        var services = CreateServices();
        Require(ReferenceEquals(services, services.AddFlourishCulture()), "Registration must remain chainable.");
        services.AddFlourishCulture();
        Equal(1, services.Count(service => service.ServiceType == typeof(ITextProvider)));
        Equal(ServiceLifetime.Scoped, services.Single(service => service.ServiceType == typeof(ITextProvider)).Lifetime);
        using var root = Build(services);
        Throws<InvalidOperationException>(() => root.GetRequiredService<ITextProvider>());
        using var scope = root.CreateScope();
        var first = scope.ServiceProvider.GetRequiredService<ITextProvider>();
        var second = scope.ServiceProvider.GetRequiredService<ITextProvider>();
        Require(ReferenceEquals(first, second), "Provider should be stable inside one scope.");
        var exports = typeof(CultureServiceCollectionExtensions).Assembly.GetExportedTypes();
        Equal(1, exports.Length);
        Equal(typeof(CultureServiceCollectionExtensions), exports[0]);
    }

    private static void DefaultBeforeBridge()
    {
        var services = CreateServices();
        services.TryAddScoped<ITextProvider, DefaultProvider>();
        services.AddFlourishCulture();
        VerifyBridge(services);
    }

    private static void DefaultAfterBridge()
    {
        var services = CreateServices();
        services.AddFlourishCulture();
        services.TryAddScoped<ITextProvider, DefaultProvider>();
        VerifyBridge(services);
    }

    private static void VerifyBridge(IServiceCollection services)
    {
        using var root = Build(services);
        using var scope = root.CreateScope();
        Equal(1, services.Count(service => service.ServiceType == typeof(ITextProvider)));
        Equal("Application", scope.ServiceProvider.GetRequiredService<ITextProvider>()
            .Get(new TextReference("App", "Shared")));
    }

    private static void MissingCatalog()
    {
        using var root = Build(CreateServices().AddFlourishCulture());
        using var scope = root.CreateScope();
        var text = scope.ServiceProvider.GetRequiredService<ITextProvider>();
        var error = Throws<KeyNotFoundException>(() => text.Get(new TextReference("Absent", "Shared", "Fallback")));
        Require(error.Message.Contains("Absent", StringComparison.Ordinal), "Missing catalog must identify its name.");
    }

    private static void InvalidArguments()
    {
        using var root = Build(CreateServices().AddFlourishCulture());
        using var scope = root.CreateScope();
        var text = scope.ServiceProvider.GetRequiredService<ITextProvider>();
        Throws<ArgumentNullException>(() => text.Get(null!));
        Throws<ArgumentNullException>(() => text.Get(new TextReference("App", "Shared"), null!));
        Throws<FormatException>(() => text.Get(new TextReference("App", "Pair"), "only one"));
        Throws<FormatException>(() => text.Get(new TextReference("App", "Missing", "{0} {1}"), "only one"));
    }

    private static void MissingCultureRegistration()
    {
        var services = new ServiceCollection();
        services.AddFlourishCulture();
        Throws<AggregateException>(() => Build(services));
        Require(!services.Any(service => service.ServiceType == typeof(LocalizationService)),
            "The bridge must not configure Culture.");
    }

    private static void AmbientCulture()
    {
        var ui = CultureInfo.CurrentUICulture;
        var format = CultureInfo.CurrentCulture;
        using var root = Build(CreateServices().AddFlourishCulture());
        using var scope = root.CreateScope();
        scope.ServiceProvider.GetRequiredService<LocalizationService>().SetCulture("zh-CN", "pt-BR");
        Require(ReferenceEquals(ui, CultureInfo.CurrentUICulture), "UI thread culture was changed.");
        Require(ReferenceEquals(format, CultureInfo.CurrentCulture), "Formatting thread culture was changed.");
    }

    private static void Equal<T>(T expected, T actual) =>
        Require(EqualityComparer<T>.Default.Equals(expected, actual), $"Expected '{expected}', received '{actual}'.");

    private static void Require(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }

    private static T Throws<T>(Action action) where T : Exception
    {
        try { action(); }
        catch (T error) { return error; }
        throw new InvalidOperationException($"Expected {typeof(T).Name}.");
    }

    private sealed class DefaultProvider : ITextProvider
    {
        public DefaultProvider() { }
        public string Culture => "en-US";
        public CultureInfo FormatCulture => CultureInfo.InvariantCulture;
        public event EventHandler? Changed { add { } remove { } }
        public string Get(TextReference text, params object?[] arguments) => text.FallbackText ?? text.Token;
    }
}
