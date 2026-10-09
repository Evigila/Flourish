using System.Text.RegularExpressions;
using ArkheideSystem.Flourish.Blazor;
using ArkheideSystem.Flourish.Blazor.Abstract;
using ArkheideSystem.Flourish.Blazor.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;

internal static class InlineActionsChecks
{
    internal static void Register(List<(string Name, Func<Task> Run)> tests)
    {
        tests.Add(("inline actions default to the end and retain explicit logical alignments", async () =>
        {
            Require(new InlineActions().Alignment == HorizontalAlignment.End, "Compact actions do not default to the trailing edge.");
            foreach (var alignment in Enum.GetValues<HorizontalAlignment>())
            {
                var html = await Render(new() { [nameof(InlineActions.Alignment)] = alignment });
                Require(html.Contains($"data-alignment=\"{alignment.ToString().ToLowerInvariant()}\""), "A valid alignment did not reach the real renderer.");
                Require(html.Contains("data-alignment-explicit=\"true\""), "An explicit alignment cannot be distinguished from contextual defaults.");
                Require(html.Contains("class=\"f-inline-actions\"") && html.Contains("Action &lt;safe&gt;"), "Alignment changed the single action renderer or encoded content.");
                Require(!html.Contains("style="), "Alignment leaked geometry into host inline styles.");
            }
            var defaults = await Render(new());
            Require(defaults.Contains("data-alignment=\"end\"") && !defaults.Contains("data-alignment-explicit="),
                "An omitted alignment must retain the page default and allow its preview board to center the row.");
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
        tests.Add(("real page action rows keep direct and nested structure in every page width and centered container", async () =>
        {
            RenderFragment actions(HorizontalAlignment alignment) => builder =>
            {
                builder.OpenComponent<InlineActions>(0);
                builder.AddAttribute(1, nameof(InlineActions.Alignment), alignment);
                builder.AddAttribute(2, nameof(InlineActions.ChildContent), (RenderFragment)(buttons =>
                {
                    buttons.OpenComponent<Button>(0);
                    buttons.AddAttribute(1, nameof(Button.Text), "Save <safe>");
                    buttons.CloseComponent();
                }));
                builder.CloseComponent();
            };
            RenderFragment content = builder =>
            {
                foreach (var alignment in Enum.GetValues<HorizontalAlignment>()) builder.AddContent(0, actions(alignment));
                builder.OpenComponent<Section>(1);
                builder.AddAttribute(2, nameof(Section.ChildContent), actions(HorizontalAlignment.End));
                builder.CloseComponent();
            };
            foreach (var (fluid, fullWidth, mode) in new[] { (false, false, "centered"), (true, false, "fluid"), (false, true, "full") })
            foreach (var container in Enum.GetValues<CenteredContainer>())
            {
                var html = await RenderPageBody(new()
                {
                    [nameof(PageBody.Fluid)] = fluid,
                    [nameof(PageBody.FullWidth)] = fullWidth,
                    [nameof(PageBody.CenteredContainer)] = container,
                    [nameof(PageBody.ChildContent)] = content
                });
                Require(html.StartsWith($"<div class=\"f-page-body f-page-{mode}", StringComparison.Ordinal), "The actual page width renderer was bypassed.");
                Require(html.Contains("f-page-centered-expanded", StringComparison.Ordinal) == (mode == "centered" && container == CenteredContainer.Expanded), "Centered geometry escaped its page width mode.");
                var direct = html[..html.IndexOf("<section", StringComparison.Ordinal)];
                Require(Regex.Matches(direct, "class=\"f-inline-actions\"").Count == 3, "A direct page action row gained a wrapper or disappeared.");
                foreach (var alignment in Enum.GetValues<HorizontalAlignment>())
                    Require(Regex.Matches(direct, $"data-alignment=\"{alignment.ToString().ToLowerInvariant()}\"").Count == 1, "The page lost an explicit action alignment.");
                Require(Regex.IsMatch(html, "<section[^>]*>\\s*<div class=\"f-inline-actions\"[^>]* data-alignment=\"end\">"), "Nested actions no longer use the same production renderer.");
                Require(Regex.Matches(html, "<button").Count == 4 && html.Contains("Save &lt;safe&gt;", StringComparison.Ordinal), "The production Button or encoded action label was lost.");
                Require(!html.Contains("style=", StringComparison.Ordinal), "The composition introduces host geometry.");
            }
        }));
        tests.Add(("Dialog owns end alignment for nested action rows and native forms without changing ordinary rows", () =>
        {
            var css = ReadSource("src/Flourish.Blazor/Flourish.Blazor.Framework/wwwroot/framework.css");
            var nested = Regex.Match(css, @"\.f-dialog-actions \.f-inline-actions\s*\{([^}]*)\}");
            Require(nested.Success && nested.Groups[1].Value.Contains("justify-content:flex-end") && nested.Groups[1].Value.Contains("margin-block:0"), "Nested actions defeat the Dialog footer alignment or spacing.");
            Require(nested.Index > css.IndexOf(".f-inline-actions[data-alignment=\"center\"]", StringComparison.Ordinal), "An equal-specificity centered row rule wins over the Dialog context.");
            var nativeForm = Regex.Match(css, @"\.f-dialog-actions > form\s*\{([^}]*)\}").Groups[1].Value;
            Require(nativeForm.Contains("display:flex") && nativeForm.Contains("flex-wrap:wrap") && nativeForm.Contains("justify-content:flex-end") && nativeForm.Contains("max-width:100%"), "Native action forms lose bounded end alignment.");
            var sample = ReadSource("src/Gallery.Flourish.Blazor/Components/Samples/Data/DialogSample.razor");
            Require(Regex.IsMatch(sample, @"<Actions>[\s\S]*<InlineActions>"), "Gallery does not exercise the real Dialog with the default reusable action row.");
            return Task.CompletedTask;
        }));
        tests.Add(("inline action CSS owns alignment and boards preserve available-width action rows", () =>
        {
            var css = ReadSource("src/Flourish.Blazor/Flourish.Blazor.Framework/wwwroot/framework.css");
            var defaults = Regex.Match(css, @"\.f-inline-actions\s*\{([^}]*)\}").Groups[1].Value;
            Require(defaults.Contains("justify-content:flex-end") && defaults.Contains("flex-wrap:wrap"), "Compact rows do not default to wrapping at the trailing edge.");
            foreach (var pair in new[] { ("start", "flex-start"), ("center", "center"), ("end", "flex-end") })
            {
                var selector = $".f-inline-actions[data-alignment=\"{pair.Item1}\"]";
                var match = Regex.Match(css, Regex.Escape(selector) + @"\s*\{([^}]*)\}");
                Require(match.Success && match.Groups[1].Value.Contains("justify-content:" + pair.Item2), "Framework CSS does not implement " + pair.Item1);
            }
            var width = Regex.Match(css, @"\.f-inline-actions\[data-alignment\]\s*\{([^}]*)\}").Groups[1].Value;
            Require(width.Contains("width:100%") && width.Contains("min-width:0") && width.Contains("box-sizing:border-box"), "Alignment has no bounded available-width track.");
            var board = ReadSource("src/Flourish.Blazor/Flourish.Blazor.Framework/wwwroot/display-board.css");
            const string centeredRow = ".f-display-board-centered > .f-display-board-viewport > .f-display-board-content:not(:has(> .f-code-block)) > .f-inline-actions:not([data-alignment-explicit])";
            Require(board.Contains(centeredRow + " { justify-content:center; }"), "Centered preview boards do not center their default direct action rows.");
            Require(!board.Contains(".f-display-board-centered .f-inline-actions"), "Preview centering leaks into nested business containers.");
            var boardRow = Regex.Match(board, @"\.f-display-board-content > \.f-inline-actions\s*\{([^}]*)\}").Groups[1].Value;
            Require(boardRow.Contains("width:100%") && boardRow.Contains("box-sizing:border-box"), "A centered board shrink-wraps rows and prevents trailing-edge alignment.");
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

    private static async Task<string> RenderPageBody(Dictionary<string, object?> parameters)
    {
        var services = new ServiceCollection().AddLogging();
        services.AddSingleton<NavigationManager>(new TestNavigation());
        services.AddSingleton<IJSRuntime>(new FakeJs());
        services.AddFlourishFramework();
        using var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();
        await using var renderer = new HtmlRenderer(scope.ServiceProvider, provider.GetRequiredService<ILoggerFactory>());
        return await renderer.Dispatcher.InvokeAsync(async () =>
            (await renderer.RenderComponentAsync<PageBody>(ParameterView.FromDictionary(parameters))).ToHtmlString());
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
