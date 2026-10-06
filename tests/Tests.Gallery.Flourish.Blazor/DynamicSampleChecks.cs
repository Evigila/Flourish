using System.Globalization;
using System.Net;
using System.Reflection;
using System.Text.RegularExpressions;
using ArkheideSystem.Essential.Culture.Blazor;
using ArkheideSystem.Flourish.Blazor.Components;
using ArkheideSystem.Flourish.Blazor.Components.Primitives;
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

    private static async Task RunSample(ServiceProvider provider, string name,
        Func<HtmlRenderer, HtmlRootComponent, IComponent, ILocalizationService, Task> exercise)
    {
        using var scope = provider.CreateScope();
        var language = scope.ServiceProvider.GetRequiredService<ILocalizationService>();
        await using var renderer = new HtmlRenderer(scope.ServiceProvider, provider.GetRequiredService<ILoggerFactory>());
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
        Console.WriteLine("PASS " + name + " post-event status and retained state across three cultures");
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
