using System.Text.RegularExpressions;
using ArkheideSystem.Flourish.Blazor.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

internal static class InlineActionsChecks
{
    internal static void Register(List<(string Name, Func<Task> Run)> tests)
    {
        tests.Add(("inline actions render all logical alignments and preserve centered defaults", async () =>
        {
            Require(new InlineActions().Alignment == HorizontalAlignment.Center, "The centered default changed.");
            foreach (var alignment in Enum.GetValues<HorizontalAlignment>())
            {
                var html = await Render(new() { [nameof(InlineActions.Alignment)] = alignment });
                Require(html.Contains($"data-alignment=\"{alignment.ToString().ToLowerInvariant()}\""), "A valid alignment did not reach the real renderer.");
                Require(html.Contains("class=\"f-inline-actions\"") && html.Contains("Action &lt;safe&gt;"), "Alignment changed the single action renderer or encoded content.");
                Require(!html.Contains("style="), "Alignment leaked geometry into host inline styles.");
            }
            Require((await Render(new())).Contains("data-alignment=\"center\""), "An omitted alignment is not centered.");
        }));
        tests.Add(("inline actions reject unknown alignment enum values", async () =>
        {
            foreach (var value in new[] { -1, 3, int.MaxValue })
            {
                try { await Render(new() { [nameof(InlineActions.Alignment)] = (HorizontalAlignment)value }); }
                catch (ArgumentOutOfRangeException error) when (error.ParamName == nameof(InlineActions.Alignment)) { continue; }
                throw new InvalidOperationException("An unknown alignment reached rendering.");
            }
        }));
        tests.Add(("Dialog owns end alignment for nested action rows and native forms without changing ordinary rows", () =>
        {
            var css = ReadSource("src/Flourish.Blazor/Flourish.Blazor.Framework/wwwroot/framework.css");
            var nested = Regex.Match(css, @"\.f-dialog-actions \.f-inline-actions\s*\{([^}]*)\}");
            Require(nested.Success && nested.Groups[1].Value.Contains("justify-content:flex-end") && nested.Groups[1].Value.Contains("margin-block:0"), "Nested default-centered actions defeat the Dialog footer alignment.");
            Require(nested.Index > css.IndexOf(".f-inline-actions[data-alignment=\"center\"]", StringComparison.Ordinal), "An equal-specificity centered row rule wins over the Dialog context.");
            var nativeForm = Regex.Match(css, @"\.f-dialog-actions > form\s*\{([^}]*)\}").Groups[1].Value;
            Require(nativeForm.Contains("display:flex") && nativeForm.Contains("flex-wrap:wrap") && nativeForm.Contains("justify-content:flex-end") && nativeForm.Contains("max-width:100%"), "Native action forms lose bounded end alignment.");
            var sample = ReadSource("src/Gallery.Flourish.Blazor/Components/Samples/Data/DialogSample.razor");
            Require(Regex.IsMatch(sample, @"<Actions>[\s\S]*<InlineActions>"), "Gallery does not exercise the real Dialog with the default-centered reusable action row.");
            return Task.CompletedTask;
        }));
        tests.Add(("inline action CSS owns alignment and boards cannot override explicit choices", () =>
        {
            var css = ReadSource("src/Flourish.Blazor/Flourish.Blazor.Framework/wwwroot/framework.css");
            foreach (var pair in new[] { ("start", "flex-start"), ("center", "center"), ("end", "flex-end") })
            {
                var selector = $".f-inline-actions[data-alignment=\"{pair.Item1}\"]";
                var match = Regex.Match(css, Regex.Escape(selector) + @"\s*\{([^}]*)\}");
                Require(match.Success && match.Groups[1].Value.Contains("justify-content:" + pair.Item2), "Framework CSS does not implement " + pair.Item1);
            }
            var width = Regex.Match(css, @"\.f-inline-actions\[data-alignment\]\s*\{([^}]*)\}").Groups[1].Value;
            Require(width.Contains("width:100%") && width.Contains("min-width:0") && width.Contains("box-sizing:border-box"), "Alignment has no bounded available-width track.");
            var board = ReadSource("src/Flourish.Blazor/Flourish.Blazor.Framework/wwwroot/display-board.css");
            Require(board.Contains("> .f-inline-actions:not([data-alignment]) { justify-content:center; }"), "Board centering overrides the action-row alignment contract.");
            var sample = ReadSource("src/Gallery.Flourish.Blazor/Components/Samples/Content/InlineActionsSample.razor");
            Require(sample.Contains("Enum.GetValues<HorizontalAlignment>()") && sample.Contains("<InlineActions Alignment=\"alignment\">"), "Gallery lacks executable examples of every alignment.");
            return Task.CompletedTask;
        }));
    }

    private static async Task<string> Render(Dictionary<string, object?> parameters)
    {
        parameters[nameof(InlineActions.ChildContent)] = (RenderFragment)(builder => builder.AddContent(0, "Action <safe>"));
        using var services = new ServiceCollection().AddLogging().BuildServiceProvider();
        await using var renderer = new HtmlRenderer(services, services.GetRequiredService<ILoggerFactory>());
        return await renderer.Dispatcher.InvokeAsync(async () =>
            (await renderer.RenderComponentAsync<InlineActions>(ParameterView.FromDictionary(parameters))).ToHtmlString());
    }

    private static string ReadSource(string path)
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !Directory.Exists(Path.Combine(directory.FullName, "src", "Flourish.Blazor"))) directory = directory.Parent;
        return File.ReadAllText(Path.Combine(directory?.FullName ?? throw new InvalidOperationException("Repository root not found."), path));
    }

    private static void Require(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }
}
