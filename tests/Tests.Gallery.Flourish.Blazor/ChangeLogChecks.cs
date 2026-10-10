using System.Net;
using System.Reflection;
using System.Text.RegularExpressions;
using ArkheideSystem.Essential.Culture.Blazor;
using ArkheideSystem.Gallery.Flourish.Blazor.Components.Pages;
using ArkheideSystem.Gallery.Flourish.Blazor.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace ArkheideSystem.Tests.Gallery.Flourish.Blazor;

internal static class ChangeLogChecks
{
    public static async Task<int> Run(ServiceProvider provider)
    {
        await using var scope = provider.CreateAsyncScope();
        var language = scope.ServiceProvider.GetRequiredService<ILocalizationService>();
        await using var renderer = new HtmlRenderer(scope.ServiceProvider, provider.GetRequiredService<ILoggerFactory>());
        ChangeLog? page = null;
        var captures = 0;
        var checks = 0;
        void Check(bool pass, string message)
        {
            if (!pass) throw new InvalidOperationException(message);
            checks++;
        }
        var root = await renderer.Dispatcher.InvokeAsync(() => renderer.RenderComponentAsync<ChangeLogHarness>(
            ParameterView.FromDictionary(new Dictionary<string, object?>
            {
                [nameof(ChangeLogHarness.Capture)] = (Action<ChangeLog>)(instance => { page = instance; captures++; })
            })));
        var initialHtml = await renderer.Dispatcher.InvokeAsync(root.ToHtmlString);
        var preview = ChangeLogCatalog.Releases[0].Version;
        Check(preview.EndsWith("-preview", StringComparison.Ordinal) && initialHtml.Contains("<h2 id=\"changelog-changes-title\">" + preview + "</h2>", StringComparison.Ordinal), "ChangeLog must open the next preview.");
        Check(page is not null, "The actual ChangeLog page was not captured.");
        var select = typeof(ChangeLog).GetMethod("SelectVersion", BindingFlags.Instance | BindingFlags.NonPublic)!;
        foreach (var release in ChangeLogCatalog.Releases)
        {
            var callback = EventCallback.Factory.Create(page!, (Action)(() => select.Invoke(page, [release.Version])));
            await renderer.Dispatcher.InvokeAsync(() => callback.InvokeAsync());
            foreach (var culture in new[] { "en-US", "zh-CN", "pt-BR", "en-US" })
            {
                await renderer.Dispatcher.InvokeAsync(() => language.SetCulture(culture));
                var html = WebUtility.HtmlDecode(await renderer.Dispatcher.InvokeAsync(root.ToHtmlString));
                Check(html.Contains("<h2 id=\"changelog-changes-title\">" + release.Version + "</h2>", StringComparison.Ordinal), "Language switching reset the selected release.");
                Check(html.IndexOf("<select", StringComparison.Ordinal) < html.IndexOf("<h2", StringComparison.Ordinal), "Version selector must precede the first subtitle.");
                Check(html.Contains("for=\"changelog-version\"", StringComparison.Ordinal) && html.Contains("id=\"changelog-version\"", StringComparison.Ordinal), "Version selector lost its accessible label.");
                Check(!html.Contains("value=\"1.1.0\"", StringComparison.Ordinal), "The initial tag must not have a ChangeLog option.");
                Check(Regex.Matches(html, "<option\\b").Count == ChangeLogCatalog.Releases.Count, "Version options are missing or duplicated.");
                Check(Regex.Matches(html, "<li>").Count == release.ChangeKeys.Count, "ChangeLog did not replace the previous release's notes.");
                if (release.ChangeKeys.Count == 0)
                    Check(html.Contains(language.Parse("Key.ChangeLog_NoChanges"), StringComparison.Ordinal), "The empty preview did not display its translated no-changes message.");
                foreach (var key in release.ChangeKeys)
                    Check(html.Contains(language.Parse(key), StringComparison.Ordinal), release.Version + ": missing translated release note " + key);
                Check(!Regex.IsMatch(html, @">[^<]*\bKey\.[A-Za-z0-9_]+"), "ChangeLog leaked a localization key.");
            }
        }
        Check(captures == 1, "Selecting a version or language remounted the ChangeLog page.");
        Console.WriteLine($"{checks} ChangeLog version selection, content replacement and retained-language checks passed.");
        return checks;
    }
}

internal sealed class ChangeLogHarness : ComponentBase
{
    [Parameter, EditorRequired] public Action<ChangeLog> Capture { get; set; } = default!;
    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenComponent<ChangeLog>(0);
        builder.AddComponentReferenceCapture(1, component => Capture((ChangeLog)component));
        builder.CloseComponent();
    }
}
