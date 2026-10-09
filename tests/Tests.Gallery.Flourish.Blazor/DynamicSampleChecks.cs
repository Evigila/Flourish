using System.Globalization;
using System.Net;
using System.Reflection;
using System.Text.RegularExpressions;
using ArkheideSystem.Essential.Culture.Blazor;
using ArkheideSystem.Flourish.Blazor.Abstract;
using ArkheideSystem.Flourish.Blazor.Components;
using ArkheideSystem.Flourish.Blazor.Components.Primitives;
using ArkheideSystem.Flourish.Extensions.Culture.Blazor;
using ArkheideSystem.Gallery.Flourish.Blazor.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.Web.HtmlRendering;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ArkheideSystem.Tests.Gallery.Flourish.Blazor;

internal static class DynamicSampleChecks
{
    private static readonly string[] Cultures = ["en-US", "zh-CN", "pt-BR", "en-US"];
    private static int Checks;
    private const BindingFlags PrivateInstance = BindingFlags.Instance | BindingFlags.NonPublic;

    public static async Task<int> Run(ServiceProvider provider)
    {
        Checks = 0;
        await RunCulturePreferences(provider);
        await RunSample(provider, "Button", async (renderer, root, sample, language) =>
        {
            await Event(renderer, sample, "SaveAsync");
            Check(!Get<bool>(sample, "AccountSelected"), "Saving a draft selected the example account.");
            await Event(renderer, sample, "SelectAccount");
            Check(Get<int>(sample, "SavedCount") == 1 && !Get<bool>(sample, "Saving"), "Selecting the account changed draft count or busy state.");
            var save = Event(renderer, sample, "SaveAsync");
            var busyHtml = await renderer.Dispatcher.InvokeAsync(root.ToHtmlString);
            Check(Get<bool>(sample, "Saving"), "The draft save did not expose its busy state.");
            Check(Regex.Matches(busyHtml, "aria-busy=\"true\"").Count == 1, "Saving a draft made the independent account action busy.");
            await Event(renderer, sample, "SelectAccount");
            Check(Get<bool>(sample, "AccountSelected"), "The account action could not run while a draft was saving.");
            await save;
            await Languages(renderer, root, language, html =>
            {
                Contains(html, language.Parse("Key.Sample_AccountSelected", language.Parse("Key.Sample_StructuredButtonName")), "Account selection status did not change language.");
                Contains(html, language.Parse("Key.Sample_SavedCount", 2), "Draft save count did not change language.");
                Check(Get<bool>(sample, "AccountSelected") && Get<int>(sample, "SavedCount") == 2, "Language switching mixed account and draft state.");
            });
            await Event(renderer, sample, "Reset");
            Check(Get<int>(sample, "SavedCount") == 0 && Get<bool>(sample, "AccountSelected"), "Resetting the draft cleared the independent account selection.");
        });
        await RunSample(provider, "SplitButton", async (renderer, root, sample, language) =>
        {
            await Event(renderer, sample, "ChooseJson");
            await Event(renderer, sample, "Export");
            Set(sample, "Selected", true);
            await Event(renderer, sample, "Export");
            await Languages(renderer, root, language, html =>
            {
                Contains(html, language.Parse("Key.Sample_ExportCount", 2, "JSON"), "Stored export status did not change language.");
                Check(Get<int>(sample, "ExportCount") == 2 && Get<string>(sample, "Format") == "JSON" && Get<string>(sample, "LastFormat") == "JSON", "Language reset the chosen format or export history.");
                Check(Get<bool>(sample, "Selected"), "Language reset the SplitButton selection.");
                Contains(html, language.Parse("Key.Sample_ExportAction", "JSON"), "The selected export action did not retain JSON.");
            });
        });
        var actionMenus = new List<ActionMenu>();
        CheckBox? hoverCheckBox = null;
        await RunSample(provider, "ActionMenu", async (renderer, root, sample, language) =>
        {
            Check(actionMenus.Count == 2 && hoverCheckBox is not null, "The actual ActionMenu scenario did not render both action entries and its hover toggle.");
            Check(actionMenus.Count(menu => menu.ChildContent is not null) == 1 && actionMenus.Count(menu => menu.Actions.Count > 0) == 1,
                "The scenario did not exercise both native commands and generated actions.");
            Check(actionMenus.All(menu => !menu.OpenOnHover), "The hover scenario was enabled before its checkbox changed.");
            await Event(renderer, hoverCheckBox!, "Change", new ChangeEventArgs { Value = true });
            Check(Get<bool>(sample, "HoverMenu") && actionMenus.All(menu => menu.OpenOnHover),
                "The checkbox enabled hover only for one ActionMenu content contract.");
            await Event(renderer, sample, "EditDraft");
            await Languages(renderer, root, language, html =>
            {
                Check(actionMenus.Count == 2 && actionMenus.All(menu => menu.OpenOnHover), "Language switching remounted menus or reset native hover binding.");
                Contains(html, language.Parse("Key.Sample_LocalDemoDraftEdited"), "Native menu action status did not change language.");
                Check(Regex.Matches(html, "<details\\b").Count == 1 && !html.Contains("data-f-enhanced", StringComparison.Ordinal),
                    "The hover scenario replaced or duplicated its SSR native disclosure.");
            });
            await Event(renderer, hoverCheckBox!, "Change", new ChangeEventArgs { Value = false });
            Check(!Get<bool>(sample, "HoverMenu") && actionMenus.All(menu => !menu.OpenOnHover),
                "The checkbox did not restore click opening for both action entries.");
        }, component =>
        {
            if (component is ActionMenu menu) actionMenus.Add(menu);
            else if (component is CheckBox checkBox) hoverCheckBox = checkBox;
        });
        var pageBodies = new List<PageBody>();
        var layoutButtons = new List<Button>();
        SelectBox<string>? layoutSelect = null;
        await RunSample(provider, "PageBody", async (renderer, root, sample, language) =>
        {
            Check(layoutSelect is not null && pageBodies.Count == 2 && layoutButtons.Count >= 2,
                "The actual PageBody scenario did not render its layout selector, content body and spreadsheet body.");
            var selector = layoutSelect!;
            Check(selector.Options.Select(option => option.Value).SequenceEqual(new[] { "standard", "expanded", "fluid", "full" }),
                "The PageBody selector does not offer the two centered containers alongside fluid and full width.");
            Check(Get<string>(sample, "Mode") == "standard" && selector.Value == "standard",
                "The PageBody scenario does not start with the standard centered container.");
            Check(sample.GetType().GetField("CompactSpacing", PrivateInstance) is null,
                "The PageBody scenario retained the retired vertical compact-spacing state.");
            var contentBody = pageBodies[0];
            var spreadsheetBody = pageBodies[1];
            await Event(renderer, layoutButtons[0], "ClickAsync", new MouseEventArgs());
            Check(Get<int>(sample, "Count") == 1, "The actual PageBody increment button did not update its bound count.");
            foreach (var mode in new[] { "expanded", "fluid", "full", "standard", "expanded" })
            {
                await Event(renderer, selector, "Change", new ChangeEventArgs { Value = mode });
                await Languages(renderer, root, language, html =>
                {
                    Check(Get<string>(sample, "Mode") == mode && selector.Value == mode && ReferenceEquals(layoutSelect, selector),
                        "The actual selector event or language refresh lost the chosen PageBody mode.");
                    Check(pageBodies.Count == 2 && ReferenceEquals(pageBodies[0], contentBody) && ReferenceEquals(pageBodies[1], spreadsheetBody),
                        "Changing PageBody mode or language remounted its retained content bodies.");
                    Check(contentBody.Fluid == (mode == "fluid") && contentBody.FullWidth == (mode == "full")
                        && contentBody.CenteredContainer == (mode == "expanded" ? CenteredContainer.Expanded : CenteredContainer.Standard),
                        "The PageBody selector did not update the production width contract.");
                    var classes = Regex.Match(html, @"<div class=""(f-page-body[^""]*)""").Groups[1].Value
                        .Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    Check(classes.Contains(mode == "full" ? "f-page-full" : mode == "fluid" ? "f-page-fluid" : "f-page-centered")
                        && classes.Contains("f-page-centered-expanded") == (mode == "expanded"),
                        "Expanded centered width leaked into fluid/full width or failed to reach the rendered body.");
                    Check(spreadsheetBody.FullWidth && spreadsheetBody.FillHeight,
                        "Changing the main content width changed the independent full-stage spreadsheet body.");
                    Check(Get<int>(sample, "Count") == 1, "Changing content width or language reset the actual action count.");
                    Contains(html, language.Parse("Key.Sample_LocalOnlyActionCount", 1), "The retained PageBody action count did not change language.");
                    foreach (var (value, key) in new[] { ("standard", "Key.Sample_StandardCenteredContainer"), ("expanded", "Key.Sample_ExpandedCenteredContainer"),
                        ("fluid", "Key.Sample_Fluid"), ("full", "Key.Sample_FullWidth") })
                        Check(selector.Options.Single(option => option.Value == value).Text == language.Parse(key),
                            "The selected layout mode labels did not refresh in the existing SelectBox.");
                    Check(!html.Contains("f-page-compact-spacing", StringComparison.Ordinal),
                        "The PageBody scenario still renders the retired compact form-spacing class.");
                });
            }
            await Event(renderer, layoutButtons[1], "ClickAsync", new MouseEventArgs());
            Check(Get<int>(sample, "Count") == 0 && Get<string>(sample, "Mode") == "expanded",
                "The actual reset button reset the selected centered width alongside the action count.");
        }, component =>
        {
            if (component is PageBody body) pageBodies.Add(body);
            else if (component is SelectBox<string> select) layoutSelect = select;
            else if (component is Button button) layoutButtons.Add(button);
        });
        await RunSample(provider, "SearchBox", async (renderer, root, sample, language) =>
        {
            // The backing value is the actual @bind target; ApplySearch is the registered SearchChanged callback.
            Set(sample, "Query", "阅读");
            await Event(renderer, sample, "ApplySearch", "阅读");
            await Languages(renderer, root, language, html =>
            {
                var expected = string.Format(CultureInfo.GetCultureInfo(language.Culture), language.Parse("Key.Sample_ActivityMatches"), 1, "阅读");
                Contains(html, expected, "Search match status did not change language.");
                Check(Get<string>(sample, "Query") == "阅读" && Get<string>(sample, "AppliedQuery") == "阅读", "Language reset typed or applied search text.");
                Check(WebUtility.HtmlDecode(html).Contains("value=\"阅读\"", StringComparison.Ordinal), "The actual input lost its current query.");
                Check(Get<string[]>(sample, "Matches").Length == 1, "Language changed the matching business data.");
            });
        });
        await RunSample(provider, "Primitives.EditingGrid", async (renderer, root, sample, language) =>
        {
            await Event(renderer, sample, "ShowValidationError");
            var invalidRows = Get<IReadOnlyList<GridRow>>(sample, "Rows");
            var invalidRevision = Get<long>(sample, "Revision");
            await Languages(renderer, root, language, html =>
            {
                Contains(html, language.Parse("Key.Sample_EnterAnAmountGreaterThanOrEqualTo0"), "Stored cell validation error did not change language.");
                Check(ReferenceEquals(Get<IReadOnlyList<GridRow>>(sample, "Rows"), invalidRows), "Language replaced the draft rows.");
                Check(invalidRows[0].Cells[1].Value == "-1" && invalidRows[0].Cells[1].Error == "Key.Sample_EnterAnAmountGreaterThanOrEqualTo0", "Language changed the cell value or stored validation token.");
                Check(Count(sample, "History") == 1 && Count(sample, "RedoHistory") == 0 && Get<long>(sample, "Revision") == invalidRevision, "Language mutated grid history or edit revision.");
            });
            await Event(renderer, sample, "UndoAsync");
            await Languages(renderer, root, language, html =>
            {
                Contains(html, language.Parse("Key.Sample_DemoChangeUndone"), "Undo result did not change language.");
                Check(Get<IReadOnlyList<GridRow>>(sample, "Rows")[0].Cells[1].Value == "12.50", "Undo did not restore the actual prior value.");
                Check(Count(sample, "History") == 0 && Count(sample, "RedoHistory") == 1, "Language reset the undo/redo stacks.");
            });
            await Event(renderer, sample, "RedoAsync");
            await Event(renderer, sample, "SaveAsync");
            await Languages(renderer, root, language, html =>
            {
                Contains(html, language.Parse("Key.Sample_CorrectDemoValidationErrorsBeforeSaving"), "Rejected save status did not change language.");
                Contains(html, language.Parse("Key.Sample_EnterAnAmountGreaterThanOrEqualTo0"), "Redo lost the rendered validation error.");
                Check(Get<IReadOnlyList<GridRow>>(sample, "Rows")[0].Cells[1].Value == "-1", "Language or a rejected save changed the invalid draft.");
                Check(Count(sample, "History") == 1 && Count(sample, "RedoHistory") == 0, "Language or a rejected save changed retained editing history.");
            });
        });
        await RunSample(provider, "MultiSelectBox", async (renderer, root, sample, language) =>
        {
            var order = new[] { "reading", "photography", "discussion", "outdoors", "unavailable" };
            await Event(renderer, sample, "AcceptSelection", new MultiSelectChange(order, new HashSet<string> { "reading", "discussion" }));
            await Event(renderer, sample, "CreateTag", "User label");
            var tags = Get<IReadOnlyList<MultiSelectOption>>(sample, "Tags");
            await Languages(renderer, root, language, html =>
            {
                var labels = string.Join(", ", language.Parse("Key.Sample_Reading"), language.Parse("Key.Sample_Discussion"), "User label");
                var expected = string.Format(CultureInfo.GetCultureInfo(language.Culture), language.Parse("Key.Sample_SelectedTags"), labels);
                Contains(html, expected, "Selected tag status did not change language.");
                Check(ReferenceEquals(Get<IReadOnlyList<MultiSelectOption>>(sample, "Tags"), tags), "Language replaced the selected business options.");
                Check(tags.Where(option => option.Selected).Select(option => option.Key).SequenceEqual(new[] { "reading", "discussion", "created-1" }), "Language reset selected stable keys.");
                Check(tags.Last().Label == "User label" && Get<int>(sample, "NextTagId") == 1, "Language changed a user-created label or identifier.");
            });
        });
        Console.WriteLine($"{Checks} actual post-event sample translation and retained-state checks passed.");
        return Checks;
    }

    private static async Task RunCulturePreferences(ServiceProvider provider)
    {
        await using var scope = provider.CreateAsyncScope();
        var language = scope.ServiceProvider.GetRequiredService<ILocalizationService>();
        var session = scope.ServiceProvider.GetRequiredService<CultureSession>();
        var javascript = (NoJs)provider.GetRequiredService<Microsoft.JSInterop.IJSRuntime>();
        var subscriptions = Subscriptions(language);
        var pickers = new List<LanguagePicker>();
        var selectors = new Dictionary<LanguagePicker, SelectBox<string>>();
        var failureDialogs = new Dictionary<LanguagePicker, Dialog>();
        LanguagePicker? currentPicker = null;
        var services = new CaptureServices(scope.ServiceProvider, component =>
        {
            if (component is LanguagePicker picker) { pickers.Add(picker); currentPicker = picker; }
            if (component is SelectBox<string> selector && currentPicker is not null && !selectors.ContainsKey(currentPicker))
                selectors.Add(currentPicker, selector);
            if (component is Dialog dialog && currentPicker is not null && !failureDialogs.ContainsKey(currentPicker))
                failureDialogs.Add(currentPicker, dialog);
        });
        await using var renderer = new HtmlRenderer(services, provider.GetRequiredService<ILoggerFactory>());
        language.SetCulture("en-US");
        var pickerRoot = await renderer.Dispatcher.InvokeAsync(() => renderer.RenderComponentAsync<LanguagePicker>());
        Check(pickers.Count == 1 && selectors.ContainsKey(pickers[0]), "The real extension language picker and its production SelectBox were not captured.");
        var picker = pickers[0];
        var languageSelector = selectors[picker];
        Check(languageSelector.Options.Select(option => option.Value).SequenceEqual(session.AvailableCultures),
            "The extension picker does not use its configured supported culture list.");
        var count = javascript.CultureWrites.Count;
        Check(count == 0, "Prerendering wrote a preference cookie before a user selection.");
        await Select(languageSelector, "zh-CN");
        Check(language.Culture == "zh-CN" && language.FormatCulture.Name == "zh-CN", "The actual language picker did not apply its saved pair.");
        Check(javascript.CultureWrites.Count == count + 1
            && javascript.CultureWrites.Last().Take(2).SequenceEqual(new object?[] { "zh-CN", "zh-CN" }), "The language picker did not persist both UI and format choices.");
        var pageRoot = await renderer.Dispatcher.InvokeAsync(() => renderer.RenderComponentAsync<ArkheideSystem.Gallery.Flourish.Blazor.Components.Pages.Localization>());
        Check(pickers.Count(picker => picker.FormatCulture) == 1, "The real localization page did not compose the extension formatting picker.");
        var formatPicker = pickers.Single(picker => picker.FormatCulture);
        var formatSelector = selectors[formatPicker];
        await Select(formatSelector, "pt-BR");
        Check(language.Culture == "zh-CN" && language.FormatCulture.Name == "pt-BR", "The format control replaced the independent UI culture.");
        Check(javascript.CultureWrites.Last().Take(2).SequenceEqual(new object?[] { "zh-CN", "pt-BR" }), "The format control did not save the independent language/format pair.");
        javascript.CultureWriteGate = new(TaskCreationOptions.RunContinuationsAsynchronously);
        javascript.CultureWriteStarted = new(TaskCreationOptions.RunContinuationsAsynchronously);
        var pendingFormat = Select(formatSelector, "pt-BR");
        try
        {
            await javascript.CultureWriteStarted.Task.WaitAsync(TimeSpan.FromSeconds(3));
            await renderer.Dispatcher.InvokeAsync(() => language.SetCulture("en-US"));
        }
        finally { javascript.CultureWriteGate.TrySetResult(); }
        await pendingFormat;
        javascript.CultureWriteGate = null;
        javascript.CultureWriteStarted = null;
        Check(language.Culture == "zh-CN" && language.FormatCulture.Name == "pt-BR"
            && javascript.CultureWrites.Last().Take(2).SequenceEqual(new object?[] { "zh-CN", "pt-BR" }),
            "A language change during a pending format save made the visible selection differ from its saved pair.");
        javascript.RejectCultureWrites = true;
        try
        {
            await Select(languageSelector, "en-US");
            await Select(formatSelector, "en-US");
            Check(failureDialogs[picker].IsOpen && failureDialogs[formatPicker].IsOpen,
                "The failed language or formatting save did not open its actual standard Dialog.");
            var pickerHtml = await renderer.Dispatcher.InvokeAsync(pickerRoot.ToHtmlString);
            var pageHtml = await renderer.Dispatcher.InvokeAsync(pageRoot.ToHtmlString);
            var title = scope.ServiceProvider.GetRequiredService<ITextProvider>().Get(new("Culture", "SaveFailedTitle", "Preference not saved"));
            var message = scope.ServiceProvider.GetRequiredService<ITextProvider>().Get(new("Culture", "SaveFailed", "The preference could not be saved."));
            Contains(pickerHtml, title, "The language save failure did not render its extension-owned standard dialog.");
            Contains(pickerHtml, message, "The language save failure did not render its extension-owned warning message.");
            Contains(pageHtml, title, "The formatting save failure did not render its extension-owned standard dialog.");
            Contains(pageHtml, message, "The formatting save failure did not render its extension-owned warning message.");
            Check(pickerHtml.Contains("<dialog", StringComparison.Ordinal) && pickerHtml.Contains("f-notice", StringComparison.Ordinal)
                && pageHtml.Contains("<dialog", StringComparison.Ordinal) && pageHtml.Contains("f-notice", StringComparison.Ordinal),
                "A failed preference save did not reuse the standard Dialog and Notice controls.");
        }
        finally { javascript.RejectCultureWrites = false; }
        Check(language.Culture == "zh-CN" && language.FormatCulture.Name == "pt-BR", "A failed save changed the visible selection.");
        await Select(languageSelector, "en-US");
        Check(language.Culture == "en-US" && language.FormatCulture.Name == "en-US", "A later valid language choice could not replace both saved cultures.");
        Check(!failureDialogs[picker].IsOpen, "A later valid language choice did not dismiss its failure dialog.");
        await Select(formatSelector, "pt-BR");
        Check(language.Culture == "en-US" && language.FormatCulture.Name == "pt-BR", "A later valid format choice could not save its independent culture.");
        Check(!failureDialogs[formatPicker].IsOpen, "A later valid format choice did not dismiss its failure dialog.");
        await renderer.DisposeAsync();
        Check(Subscriptions(language) == subscriptions, "Preference controls retained localization subscriptions after disposal.");
        Console.WriteLine("PASS actual language and format controls persist before applying, retain separate cultures and report failed writes");

        Task Select(SelectBox<string> selector, string value) =>
            renderer.Dispatcher.InvokeAsync(() => selector.ValueChanged.InvokeAsync(value));
    }

    private static async Task RunSample(ServiceProvider provider, string name,
        Func<HtmlRenderer, HtmlRootComponent, IComponent, ILocalizationService, Task> exercise, Action<IComponent>? captureComponent = null)
    {
        await using var scope = provider.CreateAsyncScope();
        var language = scope.ServiceProvider.GetRequiredService<ILocalizationService>();
        _ = scope.ServiceProvider.GetRequiredService<CultureSession>();
        var initialSubscriptions = Subscriptions(language);
        var renderingServices = captureComponent is null ? scope.ServiceProvider : new CaptureServices(scope.ServiceProvider, captureComponent);
        await using var renderer = new HtmlRenderer(renderingServices, provider.GetRequiredService<ILoggerFactory>());
        var entry = ComponentCatalog.Groups.SelectMany(group => group.Entries).Single(item => item.Name == name);
        IComponent? sample = null;
        var captures = 0;
        language.SetCulture("en-US");
        var root = await renderer.Dispatcher.InvokeAsync(() => renderer.RenderComponentAsync<SampleCaptureHarness>(
            ParameterView.FromDictionary(new Dictionary<string, object?>
            {
                [nameof(SampleCaptureHarness.SampleType)] = SampleCatalog.For(entry).ComponentType,
                [nameof(SampleCaptureHarness.Capture)] = (Action<IComponent>)(instance => { sample = instance; captures++; })
            })));
        Check(sample is not null, name + ": failed to capture the actual registered sample.");
        await exercise(renderer, root, sample!, language);
        Check(captures == 1, name + ": language switching remounted the sample.");
        await renderer.DisposeAsync();
        Check(Subscriptions(language) == initialSubscriptions, name + ": disposed interactive sample retained language subscriptions.");
        language.SetCulture("pt-BR");
        Check(Subscriptions(language) == initialSubscriptions, name + ": language refresh reattached disposed sample subscriptions.");
        Console.WriteLine("PASS " + name + " post-event status, retained state and disposal across three cultures");
    }

    private static async Task Languages(HtmlRenderer renderer, HtmlRootComponent root, ILocalizationService language, Action<string> verify)
    {
        foreach (var culture in Cultures)
        {
            await renderer.Dispatcher.InvokeAsync(() => language.SetCulture(culture));
            var html = await renderer.Dispatcher.InvokeAsync(root.ToHtmlString);
            verify(html);
            Check(!Regex.IsMatch(html, @">[^<]*\bKey\.[A-Za-z0-9_]+"), culture + ": a post-event localization token leaked into sample text.");
        }
    }

    private static async Task Event(HtmlRenderer renderer, IComponent sample, string method, params object?[] arguments)
    {
        // Invoke the registered sample callback through ComponentBase's real event pipeline.
        var callback = EventCallback.Factory.Create(sample, (Func<Task>)(async () =>
        {
            var member = sample.GetType().GetMethod(method, PrivateInstance) ?? throw new InvalidOperationException("Sample callback not found: " + method);
            if (member.Invoke(sample, arguments) is Task task) await task;
        }));
        await renderer.Dispatcher.InvokeAsync(() => callback.InvokeAsync());
    }
    private static T Get<T>(IComponent sample, string member)
    {
        var type = sample.GetType();
        var value = type.GetField(member, PrivateInstance)?.GetValue(sample) ?? type.GetProperty(member, PrivateInstance)?.GetValue(sample);
        return value is T result ? result : throw new InvalidOperationException("Sample state member not found: " + member);
    }
    private static void Set(IComponent sample, string member, object value) =>
        (sample.GetType().GetField(member, PrivateInstance) ?? throw new InvalidOperationException("Sample binding member not found: " + member)).SetValue(sample, value);
    private static int Count(IComponent sample, string member)
    {
        var value = Get<object>(sample, member);
        return (int)value.GetType().GetProperty("Count")!.GetValue(value)!;
    }
    private static void Contains(string html, string expected, string description)
    {
        var text = Regex.Replace(WebUtility.HtmlDecode(Regex.Replace(html, "<[^>]*>", " ")), @"\s+", " ").Trim();
        Check(text.Contains(Regex.Replace(expected, @"\s+", " ").Trim(), StringComparison.Ordinal), description);
    }
    private static void Check(bool condition, string description)
    {
        if (!condition) throw new InvalidOperationException(description);
        Checks++;
    }
    private static int Subscriptions(ILocalizationService service) =>
        (service.GetType().GetField("Changed", PrivateInstance)?.GetValue(service) as Delegate)?.GetInvocationList().Length ?? 0;

    private sealed class CaptureServices(IServiceProvider services, Action<IComponent> capture) : IServiceProvider
    {
        private readonly IComponentActivator activator = new ComponentCaptureActivator(capture);
        public object? GetService(Type serviceType) => serviceType == typeof(IComponentActivator) ? activator : services.GetService(serviceType);
    }
    private sealed class ComponentCaptureActivator(Action<IComponent> capture) : IComponentActivator
    {
        public IComponent CreateInstance(Type componentType)
        {
            var component = (IComponent)Activator.CreateInstance(componentType)!;
            capture(component);
            return component;
        }
    }
}

internal sealed class SampleCaptureHarness : ComponentBase
{
    [Parameter, EditorRequired] public Type SampleType { get; set; } = default!;
    [Parameter, EditorRequired] public Action<IComponent> Capture { get; set; } = default!;
    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenComponent(0, SampleType);
        builder.AddAttribute(1, "Preview", false);
        builder.AddComponentReferenceCapture(2, component => Capture((IComponent)component));
        builder.CloseComponent();
    }
}
