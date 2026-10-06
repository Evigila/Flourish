using System.Net;
using System.Reflection;
using System.Text.RegularExpressions;
using ArkheideSystem.Essential.Culture;
using ArkheideSystem.Essential.Culture.Blazor;
using ArkheideSystem.Flourish.Blazor.Abstract;
using ArkheideSystem.Flourish.Blazor;
using ArkheideSystem.Flourish.Blazor.Components;
using ArkheideSystem.Gallery.Flourish.Blazor.Components.Pages;
using ArkheideSystem.Gallery.Flourish.Blazor.Models;
using ArkheideSystem.Gallery.Flourish.Blazor.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using ILocalizationService = ArkheideSystem.Essential.Culture.Blazor.ILocalizationService;

namespace ArkheideSystem.Tests.Gallery.Flourish.Blazor;

internal static class Program
{
    private static readonly string[] Cultures = ["en-US", "zh-CN", "pt-BR", "en-US"];
    private static int Checks;

    private static async Task Main()
    {
        using var galleryStream = typeof(Framework).Assembly.GetManifestResourceStream("Gallery.Texts.json")!;
        using var frameworkStream = typeof(PageHeading).Assembly.GetManifestResourceStream("Flourish.Blazor.Texts.json")!;
        var galleryCatalog = LocalizationCatalog.Load(galleryStream);
        var frameworkCatalog = LocalizationCatalog.Load(frameworkStream);
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton<NavigationManager>(new TestNavigation());
        services.AddSingleton<IJSRuntime>(new NoJs());
        services.AddScoped<RecordStore>();
        services.AddCultureBlazor(builder => builder.AddCatalog("Gallery", galleryCatalog).AddCatalog("Flourish", frameworkCatalog)
            .SetDefaultCatalog("Gallery").SetDefaultCulture("en-US").AddSupportedCultures("en-US", "zh-CN", "pt-BR"));
        services.AddFlourishCulture();
        services.AddFlourishFramework(builder => builder.ConfigureTopBar(_ => { }));
        services.AddFlourishDesign();
        using var provider = services.BuildServiceProvider();

        var scenarios = new List<PageCase>
        {
            new(typeof(Framework), "Key.Nav_GetStarted", ["Key.Nav_GetStarted"], ["Key.Framework_StartFramework", "Key.Framework_StartDesign"]),
            new(typeof(Framework), "Key.Nav_TopBar", ["Key.Nav_TopBar"], ["Key.Framework_ProjectBrand", "Key.Framework_Injection"], new() { ["Topic"] = "topbar" }),
            new(typeof(Framework), "Key.Nav_Navigation", ["Key.Nav_Navigation"], ["Key.Framework_AddNav", "Key.Framework_ExpansionLifetime"], new() { ["Topic"] = "navigation" }),
            new(typeof(Framework), "Key.Nav_Commands", ["Key.Nav_Commands"], ["Key.Framework_RegisterCommands"], new() { ["Topic"] = "commands" }),
            new(typeof(Framework), "Key.Nav_PageLayout", ["Key.Nav_PageLayout"], ["Key.Framework_PageComposition"], new() { ["Topic"] = "layout" }),
            new(typeof(Framework), "Key.Nav_Interactions", ["Key.Nav_Interactions"], ["Key.Framework_InputDescription"], new() { ["Topic"] = "interactions" }),
            new(typeof(Examples), "Key.Nav_ExampleIndex", ["Key.Examples_CompletePages"], ["Key.Examples_SessionData", "Key.Examples_FormDescription"]),
            new(typeof(Examples), "Key.Nav_FormPage", ["Key.Nav_FormPage"], ["Key.Examples_FormValidation"], new() { ["Topic"] = "form" }),
            new(typeof(Examples), "Key.Nav_Enums", ["Key.Nav_Enums"], ["Key.Examples_TaskStatus", "Key.Examples_Planned"], new() { ["Topic"] = "enum" }),
            new(typeof(Examples), "Key.Nav_Search", ["Key.Nav_Search"], ["Key.Examples_SearchEvents", "Key.Examples_SearchForm"], new() { ["Topic"] = "search" }),
            new(typeof(Controls), "Key.Nav_Actions", [], []),
            new(typeof(Appearance), "Key.Nav_Colors", ["Key.Nav_Theme", "Key.Nav_Colors"], ["Key.Appearance_ColorDescription", "Key.Appearance_Primary"]),
            new(typeof(Appearance), "Key.Nav_Typography", ["Key.Nav_Typography"], ["Key.Appearance_FontDescription"], new() { ["Topic"] = "typography" }),
            new(typeof(Appearance), "Key.Nav_Spacing", ["Key.Nav_Spacing"], [], new() { ["Topic"] = "spacing" }),
            new(typeof(Appearance), "Key.Nav_Shape", ["Key.Nav_Shape"], ["Key.Appearance_ControlShadow"], new() { ["Topic"] = "shape" }),
            new(typeof(Appearance), "Key.Nav_Dimensions", ["Key.Nav_Dimensions"], ["Key.Appearance_ContentWidth"], new() { ["Topic"] = "dimensions" }),
            new(typeof(Forms), "Key.Page_RecordCreate", ["Key.Forms_Basic"], ["Key.Common_Category", "Key.Common_Save"]),
            new(typeof(Records), "Key.Nav_Records", [], ["Key.Common_Contact", "Key.Common_Updated", "Key.Records_Label"]),
            new(typeof(RecordDetails), "演示组织", ["Key.Records_ContactInformation"], ["Key.Common_Contact", "Key.Common_Email"], new() { ["Id"] = "sample" }),
            new(typeof(SurfacePatterns), "Key.Page_Products", ["Key.Patterns_Catalog"], ["Key.Patterns_Description", "Key.Patterns_Price"]),
            new(typeof(Icons), "Key.Nav_Icons", ["Key.Icons_IntroductionTitle", "Key.Icons_Usage"], ["Key.Icons_FontDescription", "Key.Icons_Search"]),
            new(typeof(Localization), "Key.Nav_Localization", ["Key.Localization_IntroTitle", "Key.Localization_Sample"], ["Key.Localization_Introduction"]),
            new(typeof(ReconnectExample), "Key.Page_Reconnect", ["Key.Reconnect_DescriptionTitle", "Key.Reconnect_StatesTitle"], ["Key.Reconnect_Description"]),
            new(typeof(ShellExample), "Key.Page_Shell", ["Key.Shell_Heading"], ["Key.Shell_Description", "Key.Shell_ReturnGuide"]),
            new(typeof(NotFound), "Key.Page_NotFound", [], ["Key.NotFound_Description", "Key.NotFound_Path"]),
            new(typeof(Error), "Key.Page_Error", [], ["Key.Error_Request"]),
            new(typeof(WizardExample), "Key.Nav_DisplayWizard", [], ["Key.Wizard_Personal", "Key.Wizard_Workspace"])
        };
        foreach (var scenario in scenarios)
            await CheckPage(provider, scenario);

        var entries = ComponentCatalog.Groups.SelectMany(group => group.Entries).ToArray();
        Check(entries.Length == 80, "Expected all 80 current component guides; update this guard with an intentional inventory change.");
        foreach (var entry in entries)
        {
            var sample = SampleCatalog.For(entry);
            await CheckPage(provider, new(typeof(Controls), entry.Name,
                ["Key.Guide_Intro", "Key.Guide_ControlExample", "Key.Guide_Api", "Key.Guide_ScenarioExample", "Key.Guide_ScenarioCode"],
                [entry.PurposeKey, entry.VariantsKey], new() { ["Category"] = CatalogSections.CategoryOf(entry), ["ComponentKey"] = sample.Key }, entry));
        }
        await CheckStoredValidation(provider);
        Checks += await DynamicSampleChecks.Run(provider);
        Console.WriteLine($"{Checks} actual Gallery heading, content, guide, sample, validation and disposal checks passed.");
    }

    private static async Task CheckPage(ServiceProvider provider, PageCase scenario)
    {
        using var scope = provider.CreateScope();
        var language = scope.ServiceProvider.GetRequiredService<ILocalizationService>();
        var texts = scope.ServiceProvider.GetRequiredService<ITextProvider>();
        var renderer = new HtmlRenderer(scope.ServiceProvider, provider.GetRequiredService<ILoggerFactory>());
        var initial = Subscriptions(language);
        language.SetCulture("en-US");
        var root = await renderer.Dispatcher.InvokeAsync(() => renderer.RenderComponentAsync(scenario.Component,
            ParameterView.FromDictionary(scenario.Parameters ?? new Dictionary<string, object?>())));
        Check(Subscriptions(language) > initial, scenario.Component.Name + ": no language event subscriptions.");
        foreach (var culture in Cultures)
        {
            await renderer.Dispatcher.InvokeAsync(() => language.SetCulture(culture));
            var html = await renderer.Dispatcher.InvokeAsync(root.ToHtmlString);
            var context = scenario.Component.Name + " " + (scenario.Entry?.Name ?? "") + " " + culture;
            var expected = scenario.Heading.StartsWith("Key.", StringComparison.Ordinal) ? language.Parse(scenario.Heading) : scenario.Heading;
            Check(Heading(html) == expected, context + ": expected h1 '" + expected + "', got '" + Heading(html) + "'.");
            foreach (var key in scenario.Headings)
                Check(Headings(html, "h2").Contains(Normalize(language.Parse(key))), context + ": missing translated H2 " + key);
            foreach (var key in scenario.BodyKeys)
                CheckParagraphs(html, language.Parse(key), context + ": missing translated text " + key);
            Check(!Regex.IsMatch(OutsideCode(html), @"\bKey\.[A-Za-z0-9_]+"), context + ": unresolved localization token outside a code sample.");
            if (scenario.Component == typeof(ShellExample))
                Check(WebUtility.HtmlDecode(html).Contains("aria-label=\"" + language.Parse("Key.Shell_Refresh") + "\"", StringComparison.Ordinal), context + ": refresh accessibility caption was not translated.");
            if (scenario.Entry is { } entry)
            {
                Check(WebUtility.HtmlDecode(html).Contains("data-component=\"" + entry.Name + "\"", StringComparison.Ordinal), context + ": the actual guide was not rendered.");
                CheckParagraphs(html, texts.Get(entry.Usage.Scenario), context + ": missing library usage scenario.");
                CheckParagraphs(html, texts.Get(entry.Usage.Guidance), context + ": missing library usage guidance.");
                // The actual API table renders its first page of ten parameter rows.
                foreach (var parameter in entry.ApiParameters.Take(10))
                {
                    var description = parameter.DescriptionPrefixKey is { } prefix
                        ? language.Parse(prefix, parameter.DescriptionOwner) + " " + language.Parse(parameter.DescriptionKey)
                        : language.Parse(parameter.DescriptionKey);
                    CheckParagraphs(html, description, context + ": missing actual parameter description " + parameter.Name);
                }
                if (SampleCatalog.For(entry).DescriptionKey is { } descriptionKey)
                    CheckParagraphs(html, language.Parse(descriptionKey), context + ": missing executable scenario description.");
            }
        }
        await renderer.DisposeAsync();
        Check(Subscriptions(language) == initial, scenario.Component.Name + ": disposed renderer retained language subscribers.");
        language.SetCulture("pt-BR");
        Check(Subscriptions(language) == initial, scenario.Component.Name + ": disposed page reattached language subscribers.");
        Console.WriteLine("PASS " + scenario.Component.Name + " " + (scenario.Entry?.Name ?? "page") + " three cultures and same-page refresh/disposal");
    }

    private static async Task CheckStoredValidation(ServiceProvider provider)
    {
        using var scope = provider.CreateScope();
        var language = scope.ServiceProvider.GetRequiredService<ILocalizationService>();
        var model = new RecordDraft();
        var context = new EditContext(model);
        var renderer = new HtmlRenderer(scope.ServiceProvider, provider.GetRequiredService<ILoggerFactory>());
        var initial = Subscriptions(language);
        language.SetCulture("en-US");
        var root = await renderer.Dispatcher.InvokeAsync(() => renderer.RenderComponentAsync<LocalizedValidationHarness>(
            ParameterView.FromDictionary(new Dictionary<string, object?> { [nameof(LocalizedValidationHarness.Context)] = context })));
        await renderer.Dispatcher.InvokeAsync(() => Check(!context.Validate(), "Real RecordDraft annotation validation should fail for an empty draft."));
        var storedMessages = context.GetValidationMessages().ToArray();
        Check(storedMessages.Contains("Key.Validation_NameRequired") && storedMessages.Contains("Key.Validation_EmailRequired"), "Real annotations did not store stable localization tokens.");
        foreach (var culture in Cultures)
        {
            await renderer.Dispatcher.InvokeAsync(() => language.SetCulture(culture));
            var html = await renderer.Dispatcher.InvokeAsync(root.ToHtmlString);
            Check(Text(html).Contains(Normalize(language.Parse("Key.Validation_NameRequired")), StringComparison.Ordinal), culture + ": Field did not translate an already stored name error.");
            Check(Text(html).Contains(Normalize(language.Parse("Key.Validation_EmailRequired")), StringComparison.Ordinal), culture + ": standalone ValidationMessages did not translate an already stored email error.");
            Check(!Regex.IsMatch(OutsideCode(html), @"\bKey\.Validation_[A-Za-z0-9_]+"), culture + ": validation token leaked to rendered errors.");
            Check(context.GetValidationMessages().SequenceEqual(storedMessages), culture + ": changing language changed validation state instead of rendering existing messages.");
        }
        await renderer.DisposeAsync();
        Check(Subscriptions(language) == initial, "Validation harness retained language subscriptions after disposal.");
        context.NotifyValidationStateChanged();
        Check(Subscriptions(language) == initial, "Validation notification after disposal reattached language subscribers.");
        Console.WriteLine("PASS already stored annotation errors through real Field and ValidationMessages across three cultures");
    }

    private static int Subscriptions(ILocalizationService service) =>
        (service.GetType().GetField("Changed", BindingFlags.Instance | BindingFlags.NonPublic)?.GetValue(service) as Delegate)?.GetInvocationList().Length ?? 0;
    private static void Check(bool pass, string description)
    {
        if (!pass) throw new InvalidOperationException(description);
        Checks++;
    }
    private static string OutsideCode(string html) => Regex.Replace(html, @"<(pre|script)\b[^>]*>.*?</\1>", "", RegexOptions.Singleline | RegexOptions.IgnoreCase);
    private static string Normalize(string text) => Regex.Replace(text, @"\s+", " ").Trim();
    private static string Text(string html) => Normalize(WebUtility.HtmlDecode(Regex.Replace(OutsideCode(html), "<[^>]*>", " ")));
    private static string Heading(string html) => Headings(html, "h1").FirstOrDefault() ?? "";
    private static IEnumerable<string> Headings(string html, string tag) => Regex.Matches(html, "<" + tag + @"\b[^>]*>(.*?)</" + tag + ">", RegexOptions.Singleline)
        .Select(match => Normalize(WebUtility.HtmlDecode(Regex.Replace(match.Groups[1].Value, "<[^>]*>", ""))));
    private static void CheckParagraphs(string html, string expected, string description)
    {
        var actual = Text(html);
        foreach (var paragraph in expected.Split('\n', StringSplitOptions.RemoveEmptyEntries))
            Check(actual.Contains(Normalize(paragraph), StringComparison.Ordinal), description + ": " + paragraph);
    }
    private sealed record PageCase(Type Component, string Heading, string[] Headings, string[] BodyKeys,
        Dictionary<string, object?>? Parameters = null, ComponentEntry? Entry = null);
}

internal sealed class TestNavigation : NavigationManager
{
    public TestNavigation() { Initialize("http://localhost/", "http://localhost/"); }
    protected override void NavigateToCore(string uri, bool forceLoad) => Uri = ToAbsoluteUri(uri).AbsoluteUri;
}
internal sealed class NoJs : IJSRuntime
{
    public ValueTask<TValue> InvokeAsync<TValue>(string identifier, object?[]? args) => ValueTask.FromResult(default(TValue)!);
    public ValueTask<TValue> InvokeAsync<TValue>(string identifier, CancellationToken cancellationToken, object?[]? args) => ValueTask.FromResult(default(TValue)!);
}
