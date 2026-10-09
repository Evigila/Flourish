using System.Net;
using System.Reflection;
using System.Text.RegularExpressions;
using ArkheideSystem.Flourish.Blazor.Abstract;
using ArkheideSystem.Flourish.Blazor;
using ArkheideSystem.Flourish.Blazor.Components;
using ArkheideSystem.Flourish.Extensions.Culture.Blazor;
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
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton<NavigationManager>(new TestNavigation());
        services.AddSingleton<IJSRuntime>(new NoJs());
        services.AddScoped<RecordStore>();
        services.AddFlourishFramework(builder => builder
            .ConfigureCulture(culture => culture.AddCatalog<Framework>("Gallery", "Gallery.Texts.json")
                .SetDefaultCatalog("Gallery").SetDefaultCulture("en-US")
                .AddSupportedCultures("en-US", "zh-CN", "pt-BR"))
            .ConfigureTopBar(_ => { }));
        services.AddFlourishDesign();
        await using var provider = services.BuildServiceProvider();

        var scenarios = new List<PageCase>
        {
            new(typeof(Overview), "Key.Nav_Home", ["Key.Home_Start", "Key.Nav_ChangeLog", "Key.Home_Setup"], ["Key.Home_ChangeLogDescription"]),
            new(typeof(ChangeLog), "Key.Nav_ChangeLog", [], ChangeLogCatalog.Releases[0].ChangeKeys.ToArray()),
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
            new(typeof(Records), "Key.Nav_Records", ["Key.Records_Label"], ["Key.Common_Contact", "Key.Common_Updated", "Key.Records_Label"]),
            new(typeof(RecordDetails), "演示组织", ["Key.Records_ContactInformation"], ["Key.Common_Contact", "Key.Common_Email"], new() { ["Id"] = "sample" }),
            new(typeof(SurfacePatterns), "Key.Page_Products", ["Key.Patterns_Catalog"], ["Key.Patterns_Description", "Key.Patterns_Price"]),
            new(typeof(TutorialExample), "Key.Nav_Tutorial", ["Key.Sample_TutorialHost"], ["Key.Sample_TutorialDescription", "Key.Sample_TutorialCompany"]),
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
        Check(entries.Length == 81, "Expected all 81 current component guides; update this guard with an intentional inventory change.");
        Check(entries.Count(entry => entry.Name == nameof(LanguagePicker)) == 1, "Culture must register its single library-owned LanguagePicker guide.");
        Check(!entries.Any(entry => entry.Name == "IdentityCard"), "The retired IdentityCard must not retain a Gallery guide or sample.");
        Check(entries.Count(entry => entry.Name == "Card") == 1, "Account summaries must reuse the single Card guide.");
        var registeredSamples = typeof(SampleCatalog).Assembly.GetTypes().Select(type => type.GetCustomAttribute<SampleFor>()).Where(sample => sample is not null).ToArray();
        Check(registeredSamples.Count(sample => sample!.ComponentName == "Card") == 1 && !registeredSamples.Any(sample => sample!.ComponentName == "IdentityCard"),
            "Account summaries must reuse the single Card sample without a retired IdentityCard registration.");
        foreach (var entry in entries)
        {
            var sample = SampleCatalog.For(entry);
            await CheckPage(provider, new(typeof(Controls), entry.Name,
                ["Key.Guide_Intro", "Key.Guide_ControlExample", "Key.Guide_Api", "Key.Guide_ScenarioExample", "Key.Guide_ScenarioCode"],
                [entry.PurposeKey, entry.VariantsKey], new() { ["Category"] = CatalogSections.CategoryOf(entry), ["ComponentKey"] = sample.Key }, entry));
        }
        await CheckStoredValidation(provider);
        Checks += await DynamicSampleChecks.Run(provider);
        Checks += await ChangeLogChecks.Run(provider);
        Console.WriteLine($"{Checks} actual Gallery heading, content, guide, sample, validation and disposal checks passed.");
    }

    private static async Task CheckPage(ServiceProvider provider, PageCase scenario)
    {
        await using var scope = provider.CreateAsyncScope();
        var language = scope.ServiceProvider.GetRequiredService<ILocalizationService>();
        _ = scope.ServiceProvider.GetRequiredService<CultureSession>();
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
            if (scenario.Component == typeof(Appearance) && scenario.Heading == "Key.Nav_Colors")
            {
                var actions = Regex.Matches(html, @"<div class=""f-inline-actions"" data-alignment=""end"">(.*?)</div>", RegexOptions.Singleline)
                    .Select(match => match.Groups[1].Value).ToArray();
                Check(actions.Length == 2, context + ": palette and framework navigation must use the default standard action row.");
                var palette = actions.Single(row => row.Contains("<button", StringComparison.Ordinal));
                Check(Regex.Matches(palette, @"<button\b").Count == 2
                    && Regex.IsMatch(palette, @"<button\b[^>]*type=""submit""[^>]*class=""f-button f-button-primary")
                    && Regex.IsMatch(palette, @"<button\b[^>]*type=""button""[^>]*class=""f-button f-button-secondary"),
                    context + ": palette alignment changed the standard apply/reset controls or their form behavior.");
                ContainsTranslatedAction(palette, "Key.Appearance_Apply");
                ContainsTranslatedAction(palette, "Key.Appearance_Reset");
                var frameworkLink = actions.Single(row => row.Contains("<a", StringComparison.Ordinal));
                Check(Regex.IsMatch(frameworkLink, @"<a\b[^>]*href=""/framework""[^>]*class=""f-button f-button-secondary"),
                    context + ": framework integration navigation does not use the standard link Button.");
                ContainsTranslatedAction(frameworkLink, "Key.Appearance_FrameworkLink");
                Check(html.Contains("f-uniform-grid-rectangle", StringComparison.Ordinal)
                    && html.Contains("--f-grid-columns:3", StringComparison.Ordinal),
                    context + ": aligning compact actions changed the full-row three-choice theme group.");

                void ContainsTranslatedAction(string content, string key) =>
                    Check(Text(content).Contains(Normalize(language.Parse(key)), StringComparison.Ordinal), context + ": action lost its translated label " + key);
            }
            if (scenario.Component == typeof(RecordDetails))
            {
                var record = scope.ServiceProvider.GetRequiredService<RecordStore>().Find("sample")!;
                var card = Regex.Match(html, @"<article class=""f-card"">\s*<dl>(.*?)</dl>\s*</article>", RegexOptions.Singleline);
                Check(card.Success, context + ": record metadata did not reuse the ordinary Card with a definition list.");
                var pairs = Regex.Matches(card.Groups[1].Value, @"<dt>(.*?)</dt>\s*<dd>(.*?)</dd>", RegexOptions.Singleline)
                    .ToDictionary(match => Text(match.Groups[1].Value), match => Text(match.Groups[2].Value));
                foreach (var (key, value) in new[] { ("Key.Record_Name", record.Name), ("Key.Common_Code", record.Code),
                    ("Key.Common_Category", record.Category), ("Key.Common_Status", record.Status) })
                    Check(pairs.TryGetValue(Normalize(language.Parse(key)), out var actual) && actual == Normalize(value),
                        context + ": Card metadata lost the translated label or original value for " + key);
                Check(pairs.Count == 4 && !html.Contains("f-identity-", StringComparison.Ordinal), context + ": migrated record details retained retired identity markup or changed their fields.");
            }
            if (scenario.Entry is { } entry)
            {
                if (entry.Name == "PageBody")
                {
                    var headings = Regex.Matches(html, @"<h2\b[^>]*>(.*?)</h2>", RegexOptions.Singleline).ToArray();
                    var controlHeading = Array.FindIndex(headings, heading => Text(heading.Groups[1].Value) == Normalize(language.Parse("Key.Guide_ControlExample")));
                    Check(controlHeading >= 0 && controlHeading + 1 < headings.Length,
                        context + ": the real PageBody control example section is missing.");
                    var previewStart = headings[controlHeading].Index + headings[controlHeading].Length;
                    var preview = html[previewStart..headings[controlHeading + 1].Index];
                    var bodies = Regex.Matches(preview, @"<div class=""(f-page-body[^""]*)"">\s*<p>(.*?)</p>\s*</div>", RegexOptions.Singleline).ToArray();
                    Check(bodies.Length == 4, context + ": the actual guide must show standard/expanded centered, fluid and full-width previews.");
                    foreach (var (index, widthClass, labelKey) in new[]
                    {
                        (0, "f-page-centered", "Key.Sample_StandardCenteredContainer"),
                        (1, "f-page-centered", "Key.Sample_ExpandedCenteredContainer"),
                        (2, "f-page-fluid", "Key.Sample_FluidContentArea"),
                        (3, "f-page-full", "Key.Sample_FullWidthContentArea")
                    })
                    {
                        var classes = bodies[index].Groups[1].Value.Split(' ', StringSplitOptions.RemoveEmptyEntries);
                        Check(classes.Contains(widthClass) && classes.Contains("f-page-centered-expanded") == (index == 1),
                            context + ": the real PageBody preview width does not match its container label.");
                        Check(Text(bodies[index].Groups[2].Value) == Normalize(language.Parse(labelKey)),
                            context + ": the actual container preview label did not refresh in this culture.");
                    }
                    Check(entry.ApiParameters.Any(parameter => parameter.Name == nameof(PageBody.CenteredContainer) && parameter.DefaultValue == "CenteredContainer.Standard")
                        && !entry.ApiParameters.Any(parameter => parameter.Name == "CompactSpacing"),
                        context + ": the PageBody API does not expose the standard centered-container contract or retained compact spacing.");
                    Check(typeof(PageBody).GetProperty("CompactSpacing") is null
                        && typeof(ArkheideSystem.Flourish.Blazor.Components.Patterns.NavigationSurface).GetProperty("CenteredContentGutterScale") is null,
                        context + ": a retired compact-spacing or shell gutter-scale API remains exported.");
                    Check(!html.Contains("f-page-compact-spacing", StringComparison.Ordinal),
                        context + ": the real PageBody guide retains vertical compact-spacing markup.");
                }
                if (entry.Name == "Card")
                {
                    var summary = Regex.Match(html, @"<article class=""f-card"">\s*<h3>([^<]*)</h3>\s*<code class=""f-copy-text"">DEMO-001</code>\s*</article>", RegexOptions.Singleline);
                    Check(summary.Success && Text(summary.Groups[1].Value) == Normalize(language.Parse("Key.Sample_FictionalMember")),
                        context + ": the Card preview lost its translated account title and real CopyText identifier.");
                    Check(!html.Contains("f-identity-", StringComparison.Ordinal), context + ": the Card guide still renders retired IdentityCard markup.");
                }
                if (entry.Name == "Button")
                {
                    foreach (var label in new[] { "Filled", "Outlined", "Danger", "Quiet", "Underline", "Elevated" })
                    {
                        Check(Regex.Matches(html, "<span class=\"f-button-text\">" + label + "</span>").Count == 2, context + ": missing plain/icon variant label " + label);
                        Check(WebUtility.HtmlDecode(html).Contains("aria-label=\"" + language.Parse("Key.Sample_VariantAdd", label) + "\"", StringComparison.Ordinal), context + ": wrong icon-only variant label " + label);
                    }
                }
                if (entry.Name == "UniformGridButton")
                {
                    var actions = Regex.Matches(html, "<(?:button|a)\\b[^>]*class=\"[^\"]*f-uniform-grid-button[^\"]*\"[^>]*>").Select(match => match.Value).ToArray();
                    Check(actions.Length == 12, context + ": rectangular/square variant previews or primary/secondary scenario actions are missing.");
                    Check(actions.Count(action => action.Contains("f-uniform-variant-filled-elevated", StringComparison.Ordinal)) == 5, context + ": missing FilledElevated preview or primary action.");
                    Check(actions.Count(action => action.Contains("f-uniform-variant-elevated", StringComparison.Ordinal)) == 5, context + ": missing Elevated preview or secondary action.");
                    Check(actions.Count(action => action.Contains("f-uniform-variant-danger", StringComparison.Ordinal)) == 2, context + ": missing Danger previews.");
                    Check(!actions.Any(action => Regex.IsMatch(action, @"f-uniform-variant-(?:filled|outlined)(?=[\s""])")), context + ": a retired variant is rendered.");
                }
                if (entry.Name == "FormActions")
                {
                    var actions = Regex.Matches(html, "<(?:button|a)\\b[^>]*class=\"[^\"]*f-uniform-grid-button[^\"]*\"[^>]*>").Select(match => match.Value).ToArray();
                    Check(actions.Length == 11, context + ": missing FormActions pairing, default and Danger previews or scenario actions.");
                    Check(actions.Count(action => action.Contains("f-uniform-variant-filled-elevated", StringComparison.Ordinal)) == 4,
                        context + ": FormActions lost the recommended primary FilledElevated actions.");
                    Check(actions.Count(action => action.Contains("f-uniform-variant-elevated", StringComparison.Ordinal)) == 5,
                        context + ": FormActions lost explicit Elevated secondary actions.");
                    Check(actions.Count(action => action.Contains("f-uniform-variant-danger", StringComparison.Ordinal)) == 1,
                        context + ": FormActions does not demonstrate Danger.");
                    Check(actions.Count(action => !action.Contains("f-uniform-variant-", StringComparison.Ordinal)) == 1,
                        context + ": FormActions does not demonstrate the omitted-Variant default.");
                    Check(!actions.Any(action => Regex.IsMatch(action, @"f-uniform-variant-(?:filled|outlined)(?=[\s""])")),
                        context + ": FormActions still renders a retired UGB variant.");
                }
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
        await using var scope = provider.CreateAsyncScope();
        var language = scope.ServiceProvider.GetRequiredService<ILocalizationService>();
        _ = scope.ServiceProvider.GetRequiredService<CultureSession>();
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

    private static int Subscriptions(object service) =>
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
    public List<object?[]> CultureWrites { get; } = [];
    public bool RejectCultureWrites { get; set; }
    public TaskCompletionSource? CultureWriteGate { get; set; }
    public TaskCompletionSource? CultureWriteStarted { get; set; }
    public ValueTask<TValue> InvokeAsync<TValue>(string identifier, object?[]? args) =>
        ValueTask.FromResult(identifier == "import" ? (TValue)(object)new Module(this) : default!);
    public ValueTask<TValue> InvokeAsync<TValue>(string identifier, CancellationToken cancellationToken, object?[]? args) => InvokeAsync<TValue>(identifier, args);
    private sealed class Module(NoJs owner) : IJSObjectReference
    {
        public async ValueTask<TValue> InvokeAsync<TValue>(string identifier, object?[]? args)
        {
            if (identifier == "saveCulture")
            {
                if (owner.RejectCultureWrites) throw new JSException("Preference storage unavailable.");
                owner.CultureWrites.Add(args!);
                owner.CultureWriteStarted?.TrySetResult();
                if (owner.CultureWriteGate is { } gate) await gate.Task;
            }
            return default!;
        }
        public ValueTask<TValue> InvokeAsync<TValue>(string identifier, CancellationToken cancellationToken, object?[]? args) => InvokeAsync<TValue>(identifier, args);
        public ValueTask DisposeAsync() => ValueTask.CompletedTask;
    }
}
