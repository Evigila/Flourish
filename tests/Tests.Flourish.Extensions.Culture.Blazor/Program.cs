using System.Globalization;
using System.Text;
using ArkheideSystem.Flourish.Blazor;
using ArkheideSystem.Flourish.Blazor.Abstract;
using ArkheideSystem.Flourish.Extensions.Culture.Blazor;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Localization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Microsoft.JSInterop;
using LocalizedComponentBase = ArkheideSystem.Flourish.Extensions.Culture.Blazor.LocalizedComponentBase;

namespace ArkheideSystem.Tests.Flourish.Extensions.Culture.Blazor;

internal static class Program
{
    private const string AppCatalog = """
        {
          "Shared": { "en-US": "Application", "zh-CN": "应用", "pt-BR": "Aplicação" },
          "Greeting": { "en-US": "Hello, {0}!", "zh-CN": "你好，{0}！", "pt-BR": "Olá, {0}!" },
          "Amount": { "en-US": "Amount: {0:N2}", "zh-CN": "金额：{0:N2}", "pt-BR": "Valor: {0:N2}" },
          "Pair": { "en-US": "{0} and {1}", "zh-CN": "{0}和{1}", "pt-BR": "{0} e {1}" }
        }
        """;

    private static async Task<int> Main()
    {
        var tests = new List<(string Name, Func<Task> Run)>
        {
            ("One framework entry owns culture registration and a scoped text session", Registration),
            ("Framework remains independent of the optional culture extension", FrameworkFallback),
            ("Culture registration rejects duplicates and freezes configuration", FrozenConfiguration),
            ("Application and library catalogs resolve through distinct identities", CatalogIdentity),
            ("Missing keys and argument errors preserve text contract behavior", TextContract),
            ("Independent scope and format choices leave ambient thread culture unchanged", IndependentScopes),
            ("Changed handlers can be removed", EventLifetime),
            ("Configuration supplies culture defaults and explicit overrides", ConfiguredDefaults),
            ("Invalid startup defaults, resources and retention fail explicitly", InvalidConfiguration),
            ("Extension exposes one production component with reviewed usage", ComponentInventory),
            ("Request negotiation restores cookie pairs before scoped initialization", RequestNegotiation)
        };
        BrowserPreferenceChecks.Register(tests);
        try
        {
            foreach (var test in tests)
            {
                await test.Run();
                Console.WriteLine($"PASS: {test.Name}");
            }
            Console.WriteLine($"Passed {tests.Count} culture extension checks.");
            return 0;
        }
        catch (Exception error)
        {
            Console.Error.WriteLine(error);
            return 1;
        }
    }

    internal static IServiceCollection Services(Action<CultureBuilder>? configure = null, string baseUri = "https://example.test/",
        IReadOnlyDictionary<string, string?>? configuration = null)
    {
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddScoped<JsProbe>();
        services.AddScoped<IJSRuntime>(provider => provider.GetRequiredService<JsProbe>());
        services.AddSingleton<NavigationManager>(new NavigationProbe(baseUri));
        var settings = new ConfigurationBuilder().AddInMemoryCollection(configuration ?? new Dictionary<string, string?>()).Build();
        services.AddFlourishFramework(settings, framework => framework.ConfigureCulture(culture =>
        {
            using var application = new MemoryStream(Encoding.UTF8.GetBytes(AppCatalog));
            culture.AddCatalog("App", application);
            configure?.Invoke(culture);
        }));
        return services;
    }

    internal static ServiceProvider Build(IServiceCollection services) =>
        services.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true, ValidateOnBuild = true });

    private static async Task Registration()
    {
        var services = Services();
        Equal(1, services.Count(service => service.ServiceType == typeof(ITextProvider)));
        Equal(ServiceLifetime.Scoped, services.Single(service => service.ServiceType == typeof(CultureSession)).Lifetime);
        await using var root = Build(services);
        Throws<InvalidOperationException>(() => root.GetRequiredService<CultureSession>());
        await using var scope = root.CreateAsyncScope();
        var first = scope.ServiceProvider.GetRequiredService<CultureSession>();
        Require(ReferenceEquals(first, scope.ServiceProvider.GetRequiredService<CultureSession>()), "One circuit created multiple sessions.");
        Require(ReferenceEquals(first, scope.ServiceProvider.GetRequiredService<ITextProvider>()), "Text uses a separate localization core.");
        Equal("Application", first.Parse("Shared"));
    }

    private static async Task FrameworkFallback()
    {
        var services = new ServiceCollection();
        services.AddFlourishFramework();
        await using var root = Build(services);
        await using var scope = root.CreateAsyncScope();
        var text = scope.ServiceProvider.GetRequiredService<ITextProvider>();
        Equal("Fallback", text.Get(new("Absent", "Token", "Fallback")));
        Equal("Token", text.Get(new("Absent", "Token")));
        Require(scope.ServiceProvider.GetService<CultureSession>() is null, "Framework registered its optional culture session.");
        Require(!typeof(ServiceCollectionExtensions).Assembly.GetReferencedAssemblies()
            .Any(reference => reference.Name!.Contains("Culture", StringComparison.Ordinal)), "Framework depends on Culture.");
    }

    private static Task FrozenConfiguration()
    {
        CultureBuilder? captured = null;
        _ = Services(culture => captured = culture);
        Throws<InvalidOperationException>(() => captured!.SetDefaultCulture("zh-CN"));
        Throws<InvalidOperationException>(() => captured!.SetDefaultCatalog("Flourish"));
        Throws<InvalidOperationException>(() => captured!.SetRetentionDays(42));
        Throws<InvalidOperationException>(() => captured!.AddSupportedCultures("pt-BR"));
        Throws<InvalidOperationException>(() => captured!.AddCatalog<LanguagePicker>("Later", "Culture.Texts.json"));
        var services = new ServiceCollection();
        Throws<InvalidOperationException>(() => services.AddFlourishFramework(framework => framework.ConfigureCulture().ConfigureCulture()));
        return Task.CompletedTask;
    }

    private static async Task CatalogIdentity()
    {
        await using var root = Build(Services(culture => culture.AddCatalog<LanguagePicker>("Another", "Culture.Texts.json")));
        await using var scope = root.CreateAsyncScope();
        var text = scope.ServiceProvider.GetRequiredService<ITextProvider>();
        Equal("Application", text.Get(new("App", "Key.Shared")));
        Equal("Getting started", text.Get(new("Flourish", "Tutorial_Label")));
        Equal("Language", text.Get(new("Culture", "Language")));
        Equal("Language", text.Get(new("Another", "Language")));
        Equal("Hello, Blazor!", text.Get(new("App", "Greeting"), "Blazor"));
        Require(Throws<KeyNotFoundException>(() => text.Get(new("Absent", "Shared"))).Message.Contains("Absent"), "Missing catalog omitted its identity.");
    }

    private static async Task TextContract()
    {
        await using var root = Build(Services());
        await using var scope = root.CreateAsyncScope();
        var text = scope.ServiceProvider.GetRequiredService<ITextProvider>();
        var session = scope.ServiceProvider.GetRequiredService<CultureSession>();
        Equal("Fallback", text.Get(new("App", "Missing", "Fallback")));
        Equal("Key.Missing", text.Get(new("App", "Key.Missing")));
        Equal("", text.Get(new("App", "Missing", "")));
        Require(session.TryParse("Shared", out var value) && value == "Application", "Default catalog lookup failed.");
        Require(!session.TryParse("Missing", [], out _), "A missing token appeared to exist.");
        Throws<ArgumentNullException>(() => text.Get(null!));
        Throws<ArgumentNullException>(() => text.Get(new("App", "Shared"), null!));
        Throws<FormatException>(() => session.Parse("Pair", "only one"));
        Throws<FormatException>(() => text.Get(new("App", "Missing", "{0} {1}"), "only one"));
    }

    private static async Task IndependentScopes()
    {
        var ui = CultureInfo.CurrentUICulture;
        var format = CultureInfo.CurrentCulture;
        await using var root = Build(Services());
        await using var first = root.CreateAsyncScope();
        await using var second = root.CreateAsyncScope();
        var left = first.ServiceProvider.GetRequiredService<CultureSession>();
        var right = second.ServiceProvider.GetRequiredService<CultureSession>();
        Require(await left.SelectAsync("zh-CN", "pt-BR"), "A supported pair failed to save.");
        Equal("zh-CN", left.Culture);
        Equal("en-US", right.Culture);
        Equal("pt-BR", left.FormatCulture.Name);
        Equal("金额：12.345,67", left.Parse("Amount", 12345.67m));
        Equal("Fallback: 12.345,67", ((ITextProvider)left).Get(new("App", "Missing", "Fallback: {0:N2}"), 12345.67m));
        Require(ReferenceEquals(ui, CultureInfo.CurrentUICulture) && ReferenceEquals(format, CultureInfo.CurrentCulture), "A browser choice changed ambient culture.");
        Equal(0, second.ServiceProvider.GetRequiredService<JsProbe>().Imports);
    }

    private static async Task EventLifetime()
    {
        await using var root = Build(Services());
        await using var scope = root.CreateAsyncScope();
        var session = scope.ServiceProvider.GetRequiredService<CultureSession>();
        var updates = 0;
        EventHandler handler = (_, _) => updates++;
        session.Changed += handler;
        Require(await session.SelectAsync("zh-CN"), "A supported choice failed.");
        Equal(1, updates);
        session.Changed -= handler;
        Require(await session.SelectAsync("en-US"), "A supported choice failed.");
        Equal(1, updates);
    }

    private static async Task ConfiguredDefaults()
    {
        var settings = new Dictionary<string, string?>
        {
            ["Flourish:Localization:DefaultCulture"] = "zh-CN",
            ["Flourish:Localization:DefaultFormatCulture"] = "pt-BR",
            ["Flourish:Localization:SupportedCultures:0"] = "zh-CN",
            ["Flourish:Localization:SupportedCultures:1"] = "pt-BR",
            ["Flourish:Preferences:RetentionDays"] = "42"
        };
        await using var root = Build(Services(configuration: settings));
        await using var scope = root.CreateAsyncScope();
        var session = scope.ServiceProvider.GetRequiredService<CultureSession>();
        Equal("zh-CN", session.Culture);
        Equal("pt-BR", session.FormatCulture.Name);
        Require(session.AvailableCultures.SequenceEqual(new[] { "zh-CN", "pt-BR" }), "Configured supported cultures were ignored.");
        Require(await session.SelectFormatAsync("zh-CN"), "Configured format choice failed.");
        Equal(42 * 86400, scope.ServiceProvider.GetRequiredService<JsProbe>().Module.Calls.Single().Arguments[3]);
        await using var overridden = Build(Services(culture => culture.SetDefaultCulture("en-US").AddSupportedCultures("en-US").SetRetentionDays(7), configuration: settings));
        await using var overriddenScope = overridden.CreateAsyncScope();
        Equal("en-US", overriddenScope.ServiceProvider.GetRequiredService<CultureSession>().Culture);
        var request = root.GetRequiredService<IOptions<RequestLocalizationOptions>>().Value;
        Equal("zh-CN", request.DefaultRequestCulture.UICulture.Name);
        Equal("pt-BR", request.DefaultRequestCulture.Culture.Name);
        Require(request.RequestCultureProviders.Select(provider => provider.GetType()).SequenceEqual(new[]
        { typeof(CookieRequestCultureProvider), typeof(AcceptLanguageHeaderRequestCultureProvider) }), "Request provider precedence changed.");
    }

    private static Task InvalidConfiguration()
    {
        foreach (var days in new[] { 0, -1, 3651, int.MaxValue }) Throws<ArgumentOutOfRangeException>(() => Services(culture => culture.SetRetentionDays(days)));
        Throws<ArgumentOutOfRangeException>(() => Services(configuration: new Dictionary<string, string?> { ["Flourish:Preferences:RetentionDays"] = "0" }));
        Throws<InvalidOperationException>(() => Services(culture => culture.SetDefaultCatalog("Absent")));
        Throws<InvalidOperationException>(() => Services(culture => culture.SetDefaultCulture("fr-FR")));
        Throws<InvalidOperationException>(() => Services(culture => culture.AddCatalog<LanguagePicker>("Missing", "Absent.json")));
        Throws<InvalidOperationException>(() => Services(culture => culture.AddCatalog<LanguagePicker>("Culture", "Culture.Texts.json")));
        Throws<ArgumentException>(() => Services(culture => culture.SetDefaultCulture("invalid_culture!")));
        return Task.CompletedTask;
    }

    private static Task ComponentInventory()
    {
        var assembly = typeof(CultureFrameworkExtensions).Assembly;
        var components = assembly.GetExportedTypes().Where(type => !type.IsAbstract && typeof(IComponent).IsAssignableFrom(type)).ToArray();
        Require(components.SequenceEqual(new[] { typeof(LanguagePicker) }), "Culture exposes multiple production selectors.");
        var usage = ArkheideSystem.Flourish.Extensions.Culture.Blazor.ComponentUsageCatalog.For(typeof(LanguagePicker));
        Equal("Culture", usage.Scenario.CatalogId);
        Equal("PickerGuidance", usage.Guidance.Token);
        Require(!assembly.GetExportedTypes().Any(type => type.Name is "BrowserPreferences" or "BrowserPreferenceOptions" or "CultureServiceCollectionExtensions"), "Retired APIs remain exported.");
        Require(typeof(LocalizedComponentBase).IsAbstract, "Lifecycle base became another production renderer.");
        return Task.CompletedTask;
    }

    private static async Task RequestNegotiation()
    {
        await using var root = Build(Services());
        var application = new ApplicationBuilder(root);
        var accessor = root.GetRequiredService<IHttpContextAccessor>();
        var filter = root.GetServices<IStartupFilter>().Single();
        string? ui = null, format = null, text = null;
        filter.Configure(app => app.Run(async context =>
        {
            await using var scope = root.CreateAsyncScope();
            accessor.HttpContext = context;
            var session = scope.ServiceProvider.GetRequiredService<CultureSession>();
            ui = session.Culture;
            format = session.FormatCulture.Name;
            text = session.Parse("Amount", 12345.67m);
        }))(application);
        var pipeline = application.Build();
        var request = new DefaultHttpContext();
        request.Request.Headers.AcceptLanguage = "en-US";
        request.Request.Headers.Cookie = CookieRequestCultureProvider.DefaultCookieName + "=" + Uri.EscapeDataString(
            CookieRequestCultureProvider.MakeCookieValue(new RequestCulture("pt-BR", "zh-CN")));
        await pipeline(request);
        Equal("zh-CN", ui);
        Equal("pt-BR", format);
        Equal("金额：12.345,67", text);
        var anotherBrowser = new DefaultHttpContext();
        anotherBrowser.Request.Headers.AcceptLanguage = "pt-BR";
        await pipeline(anotherBrowser);
        Equal("pt-BR", ui);
        Equal("pt-BR", format);
        var defaultBrowser = new DefaultHttpContext();
        defaultBrowser.Request.Headers.Cookie = CookieRequestCultureProvider.DefaultCookieName + "=invalid";
        await pipeline(defaultBrowser);
        Equal("en-US", ui);
        Equal("en-US", format);
        accessor.HttpContext = null;
    }

    internal static void Equal<T>(T expected, T actual) => Require(EqualityComparer<T>.Default.Equals(expected, actual), $"Expected '{expected}', received '{actual}'.");
    internal static void Require(bool condition, string message) { if (!condition) throw new InvalidOperationException(message); }
    internal static T Throws<T>(Action action) where T : Exception
    {
        try { action(); } catch (T error) { return error; }
        throw new InvalidOperationException($"Expected {typeof(T).Name}.");
    }
    internal static async Task<T> ThrowsAsync<T>(Func<Task> action) where T : Exception
    {
        try { await action(); } catch (T error) { return error; }
        throw new InvalidOperationException($"Expected {typeof(T).Name}.");
    }

    private sealed class NavigationProbe : NavigationManager
    {
        internal NavigationProbe(string baseUri) => Initialize(baseUri, new Uri(new Uri(baseUri), "appearance").AbsoluteUri);
        protected override void NavigateToCore(string uri, bool forceLoad) { }
    }
}

internal sealed class JsProbe : IJSRuntime
{
    public JsProbe() { }
    internal int Imports { get; private set; }
    internal List<string> ImportPaths { get; } = [];
    internal Exception? ImportFailure { get; set; }
    internal ModuleProbe Module { get; } = new();
    public ValueTask<TValue> InvokeAsync<TValue>(string identifier, object?[]? args) => InvokeAsync<TValue>(identifier, CancellationToken.None, args);
    public ValueTask<TValue> InvokeAsync<TValue>(string identifier, CancellationToken cancellationToken, object?[]? args)
    {
        if (identifier != "import") throw new InvalidOperationException("Unexpected JS entry: " + identifier);
        Imports++;
        ImportPaths.Add((string)args![0]!);
        return ImportFailure is { } failure ? ValueTask.FromException<TValue>(failure) : ValueTask.FromResult((TValue)(object)Module);
    }
}

internal sealed record Invocation(string Identifier, object?[] Arguments);
internal sealed class ModuleProbe : IJSObjectReference
{
    internal List<Invocation> Calls { get; } = [];
    internal TaskCompletionSource FirstWriteStarted { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
    internal TaskCompletionSource? WriteGate { get; set; }
    internal Exception? WriteFailure { get; set; }
    internal Exception? DisposeFailure { get; set; }
    internal int DisposeCalls { get; private set; }
    internal int MaxActiveWrites { get; private set; }
    private int activeWrites;
    public ValueTask<TValue> InvokeAsync<TValue>(string identifier, object?[]? args) => InvokeAsync<TValue>(identifier, CancellationToken.None, args);
    public async ValueTask<TValue> InvokeAsync<TValue>(string identifier, CancellationToken cancellationToken, object?[]? args)
    {
        if (identifier != "saveCulture") throw new InvalidOperationException("Unexpected preference entry: " + identifier);
        Calls.Add(new(identifier, args?.ToArray() ?? []));
        activeWrites++;
        MaxActiveWrites = Math.Max(MaxActiveWrites, activeWrites);
        FirstWriteStarted.TrySetResult();
        try
        {
            if (WriteGate is { } gate) await gate.Task;
            if (WriteFailure is { } failure) throw failure;
            return default!;
        }
        finally { activeWrites--; }
    }
    public ValueTask DisposeAsync()
    {
        DisposeCalls++;
        return DisposeFailure is { } failure ? ValueTask.FromException(failure) : ValueTask.CompletedTask;
    }
}
