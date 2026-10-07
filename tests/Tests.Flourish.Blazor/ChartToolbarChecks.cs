using System.Text.RegularExpressions;
using ArkheideSystem.Flourish.Blazor;
using ArkheideSystem.Flourish.Blazor.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;

internal static class ChartToolbarChecks
{
    internal static void Register(List<(string Name, Func<Task> Run)> tests)
    {
        tests.Add(("chart business controls and production display selection share one toolbar", async () =>
        {
            var parameters = Chart(); parameters[nameof(LineChart.ControlsContent)] = PeriodControl();
            var html = await Render<LineChart>(parameters);
            Require(Regex.Matches(html, "class=\"f-line-chart-controls\"").Count == 1, "Chart split the toolbar into multiple rows.");
            Require(html.Contains("f-line-chart-controls-content", StringComparison.Ordinal)
                && html.Contains("for=\"period-choice\"", StringComparison.Ordinal)
                && html.Contains("id=\"period-choice\"", StringComparison.Ordinal), "The real Field/select protocol was not rendered inside the chart.");
            Require(html.IndexOf("f-line-chart-controls-content", StringComparison.Ordinal) < html.IndexOf("f-multi-select-box", StringComparison.Ordinal), "Period did not precede the library-owned display entry.");
            Require(Regex.Matches(html, "class=\"f-multi-select-box\"").Count == 1, "Host content created a second display controller.");
            var trigger = Regex.Match(html, "<summary[^>]*>(.*?)</summary>", RegexOptions.Singleline).Value;
            Require(trigger.Contains("f-button-secondary", StringComparison.Ordinal) && trigger.Contains("remove_red_eye", StringComparison.Ordinal)
                && trigger.Contains("f-sr-only", StringComparison.Ordinal), "Chart display did not follow the record table's icon-only entry.");
            Require(html.Contains("data-f-selection-key=\"count\"", StringComparison.Ordinal) && html.Contains("<td>3", StringComparison.Ordinal), "Toolbar changed chart series or exact values.");
        }));
        tests.Add(("chart heading shares the standard section header with right-aligned business and display controls", async () =>
        {
            var parameters = Chart(); parameters[nameof(LineChart.Heading)] = "Weekly orders";
            parameters[nameof(LineChart.Id)] = "weekly-orders";
            parameters[nameof(LineChart.ControlsContent)] = PeriodControl();
            var html = await Render<LineChart>(parameters);
            var header = Regex.Match(html, "<header class=\"f-section-heading\">(.*?)</header>", RegexOptions.Singleline).Value;
            Require(header.Contains("<h2 id=\"weekly-orders-title\">Weekly orders</h2>", StringComparison.Ordinal), "Chart did not use the standard section heading.");
            Require(header.Contains("f-heading-actions", StringComparison.Ordinal) && header.Contains("period-choice", StringComparison.Ordinal)
                && header.Contains("f-multi-select-box", StringComparison.Ordinal), "Period and display were outside the heading row.");
            Require(Regex.Matches(html, "f-line-chart-controls-content").Count == 1 && Regex.Matches(html, "class=\"f-multi-select-box\"").Count == 1,
                "Heading duplicated controls or their state.");
            Require(html.Contains("aria-labelledby=\"weekly-orders-title\"", StringComparison.Ordinal) && html.Contains("<td>3", StringComparison.Ordinal), "Heading lost section discovery or exact values.");
        }));
        tests.Add(("chart toolbar content remains usable when display options are disabled", async () =>
        {
            var parameters = Chart(); parameters[nameof(LineChart.ShowDisplayOptions)] = false;
            parameters[nameof(LineChart.ControlsContent)] = PeriodControl();
            var html = await Render<LineChart>(parameters);
            Require(html.Contains("f-line-chart-controls-content", StringComparison.Ordinal) && html.Contains("period-choice", StringComparison.Ordinal), "Hiding display selection removed the business filter.");
            Require(!html.Contains("f-multi-select-box", StringComparison.Ordinal), "Disabled display entry still rendered.");
            parameters.Remove(nameof(LineChart.ControlsContent));
            html = await Render<LineChart>(parameters);
            Require(!html.Contains("f-line-chart-controls", StringComparison.Ordinal), "An empty toolbar left a phantom row.");
        }));
        tests.Add(("inline input actions render real field input and native submit in one control slot", async () =>
        {
            RenderFragment content = builder =>
            {
                builder.OpenComponent<InlineActions>(0);
                builder.AddAttribute(1, nameof(InlineActions.ChildContent), (RenderFragment)(row =>
                {
                    row.OpenComponent<StandaloneTextBox>(0);
                    row.AddAttribute(1, nameof(StandaloneTextBox.Value), "Draft");
                    row.CloseComponent();
                    row.OpenComponent<Button>(2);
                    row.AddAttribute(3, nameof(Button.Type), "submit");
                    row.AddAttribute(4, nameof(Button.Text), "Create");
                    row.CloseComponent();
                }));
                builder.CloseComponent();
            };
            var html = await Render<Field>(new() { [nameof(Field.Id)] = "create-name", [nameof(Field.Label)] = "Name", [nameof(Field.ChildContent)] = content });
            Require(Regex.IsMatch(html, "<div class=\"f-field-control\"><div class=\"f-inline-actions\" data-alignment=\"center\"><input"), "Input/actions were not in the same actual field slot.");
            Require(html.Contains("id=\"create-name\"", StringComparison.Ordinal) && html.Contains("type=\"submit\"", StringComparison.Ordinal), "Inline layout lost native input/submit semantics.");
        }));
    }

    private static Dictionary<string, object?> Chart() => new()
    {
        [nameof(LineChart.Title)] = "Weekly orders",
        [nameof(LineChart.Labels)] = new LineChart.PointLabel[] { new("mon", "Monday") },
        [nameof(LineChart.Series)] = new LineChart.DataSeries[] { new("count", "Count", [3]) }
    };

    private static RenderFragment PeriodControl() => builder =>
    {
        builder.OpenComponent<Field>(0);
        builder.AddAttribute(1, nameof(Field.Id), "period-choice");
        builder.AddAttribute(2, nameof(Field.Label), "Week");
        builder.AddAttribute(3, nameof(Field.ChildContent), (RenderFragment)(field =>
        {
            field.OpenComponent<StandaloneSelectBox<string>>(0);
            field.AddAttribute(1, nameof(StandaloneSelectBox<string>.Value), "current");
            field.AddAttribute(2, nameof(StandaloneSelectBox<string>.ChildContent), (RenderFragment)(options =>
            {
                options.OpenElement(0, "option"); options.AddAttribute(1, "value", "current"); options.AddContent(2, "Current week"); options.CloseElement();
            }));
            field.CloseComponent();
        }));
        builder.CloseComponent();
    };

    private static async Task<string> Render<T>(Dictionary<string, object?> parameters) where T : IComponent
    {
        var services = new ServiceCollection(); services.AddLogging(); services.AddFlourishFramework();
        services.AddSingleton<IJSRuntime>(new StaticJs()); services.AddSingleton<NavigationManager>(new StaticNavigation());
        using var provider = services.BuildServiceProvider();
        await using var renderer = new HtmlRenderer(provider, provider.GetRequiredService<ILoggerFactory>());
        return await renderer.Dispatcher.InvokeAsync(async () => (await renderer.RenderComponentAsync<T>(ParameterView.FromDictionary(parameters))).ToHtmlString());
    }

    private static void Require(bool condition, string message) { if (!condition) throw new InvalidOperationException(message); }
    private sealed class StaticNavigation : NavigationManager { internal StaticNavigation() => Initialize("https://example.test/", "https://example.test/dashboard/"); }
    private sealed class StaticJs : IJSRuntime
    {
        public ValueTask<TValue> InvokeAsync<TValue>(string identifier, object?[]? args) => throw new InvalidOperationException("Static render must not import controls.");
        public ValueTask<TValue> InvokeAsync<TValue>(string identifier, CancellationToken cancellationToken, object?[]? args) => InvokeAsync<TValue>(identifier, args);
    }
}
