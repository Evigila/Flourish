using System.Net;
using System.Text.RegularExpressions;
using ArkheideSystem.Flourish.Blazor;
using ArkheideSystem.Flourish.Blazor.Abstract;
using ArkheideSystem.Flourish.Blazor.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;

internal static class NavigationChoicesChecks
{
    internal static void Register(List<(string Name, Func<Task> Run)> tests)
    {
        tests.Add(("navigation choices render current native GET links rather than simulated ARIA tabs", async () =>
        {
            var html = await Render(Parameters());
            Require(html.Contains("class=\"f-navigation-choices-nav\"", StringComparison.Ordinal)
                && html.Contains("aria-label=\"Access method\"", StringComparison.Ordinal), "The production navigation landmark disappeared.");
            Require(Regex.Matches(html, "aria-current=\"page\"").Count == 1, "The renderer did not identify exactly one current GET destination.");
            foreach (var key in new[] { "organization", "account", "pass" })
            {
                var choice = OpeningTag(html, "a", "entry-" + key + "-choice");
                Require(choice.Contains("href=\"/entry?method=" + key + "\"", StringComparison.Ordinal)
                    && choice.Contains("data-enhance-nav=\"false\"", StringComparison.Ordinal), "Choice navigation lost its original GET address or document boundary.");
                Require(choice.Contains("f-uniform-grid-button", StringComparison.Ordinal), "The choice bypassed the actual UniformGridButton renderer.");
                Require(choice.Contains("aria-current=\"page\"", StringComparison.Ordinal) == (key == "organization"), "Current choice markup disagreed with ActiveKey.");
            }
            Require(!Regex.IsMatch(html, "role=\"(?:tablist|tab|tabpanel)\"|aria-selected=|aria-controls="), "Native GET links were falsely described as an interactive ARIA tab widget.");
        }));
        tests.Add(("navigation choices own retained panel structure and encode labels without editing native form content", async () =>
        {
            var parameters = Parameters();
            parameters[nameof(NavigationChoices.Items)] = new NavigationChoiceItem[]
            {
                new("organization", "Organization <script> & name", "/entry?method=organization&next=%2Fprivate"),
                new("account", "Account", "/entry?method=account"), new("pass", "Pass", "/entry?method=pass")
            };
            var html = await Render(parameters);
            Require(html.Contains("Organization &lt;script&gt; &amp; name", StringComparison.Ordinal)
                && !html.Contains("Organization <script>", StringComparison.Ordinal), "A host label became markup.");
            Require(WebUtility.HtmlDecode(html).Contains("href=\"/entry?method=organization&next=%2Fprivate\"", StringComparison.Ordinal), "Query transport was rewritten.");
            Require(Regex.Matches(html, "<form method=\"post\"").Count == 3, "Inactive native POST panels were discarded instead of retained.");
            foreach (var key in new[] { "organization", "account", "pass" })
            {
                var panel = OpeningTag(html, "section", "entry-" + key + "-panel");
                Require(panel.Contains("f-navigation-choice-panel", StringComparison.Ordinal), "The panel renderer escaped library ownership.");
                Require(Regex.IsMatch(panel, @"\shidden(?:\s|=|>)") == (key != "organization"), "Inactive panels remained interactive or the active panel was hidden.");
                Require(html.Contains("name=\"protocol-secret\" value=\"" + key + "\"", StringComparison.Ordinal), "The generic renderer changed a host-owned form value.");
            }
        }));
        tests.Add(("navigation choices preserve component identity when the host changes the active route key", async () =>
        {
            var parameters = Parameters();
            parameters[nameof(NavigationChoices.ChildContent)] = (RenderFragment<NavigationChoiceItem>)(choice => builder =>
            {
                builder.OpenComponent<RetainedPanel>(0);
                builder.AddAttribute(1, nameof(RetainedPanel.Key), choice.Key);
                builder.CloseComponent();
            });
            await WithComponent(parameters, async (component, activator, html) =>
            {
                var panels = activator.Instances.OfType<RetainedPanel>().ToArray();
                Require(panels.Length == 3, "Every supplied native panel must remain mounted.");
                parameters[nameof(NavigationChoices.ActiveKey)] = "account";
                await component.SetParametersAsync(ParameterView.FromDictionary(parameters));
                Require(activator.Instances.OfType<RetainedPanel>().SequenceEqual(panels), "Changing ActiveKey recreated hidden panel components.");
                Require(!Regex.IsMatch(OpeningTag(html(), "section", "entry-account-panel"), @"\shidden(?:\s|=|>)"), "A new current route did not select its panel.");
                Require(OpeningTag(html(), "a", "entry-account-choice").Contains("f-uniform-variant-filled", StringComparison.Ordinal), "The new current link did not use the standard selected appearance.");
            });
        }));
        tests.Add(("disabled navigation choices keep content but expose no selectable GET destination", async () =>
        {
            var parameters = Parameters();
            parameters[nameof(NavigationChoices.Items)] = new NavigationChoiceItem[]
            {
                new("organization", "Organization", "/entry?method=organization"), new("account", "Account", "/entry?method=account", Disabled:true)
            };
            var html = await Render(parameters);
            var disabled = OpeningTag(html, "a", "entry-account-choice");
            Require(!disabled.Contains("href=", StringComparison.Ordinal) && disabled.Contains("aria-disabled=\"true\"", StringComparison.Ordinal)
                && disabled.Contains("tabindex=\"-1\"", StringComparison.Ordinal), "A disabled choice remained navigable.");
            parameters[nameof(NavigationChoices.ActiveKey)] = "account";
            await Reject(parameters, nameof(NavigationChoices.ActiveKey));
        }));
        tests.Add(("navigation choices reject ambiguous keys unsafe destinations and incomplete host contracts", async () =>
        {
            foreach (var replacement in new Dictionary<string, object?>[]
            {
                new() { [nameof(NavigationChoices.Id)] = "not an id" },
                new() { [nameof(NavigationChoices.Label)] = " " },
                new() { [nameof(NavigationChoices.ActiveKey)] = "missing" },
                new() { [nameof(NavigationChoices.Items)] = Array.Empty<NavigationChoiceItem>() },
                new() { [nameof(NavigationChoices.ChildContent)] = null },
                new() { [nameof(NavigationChoices.Items)] = new NavigationChoiceItem[] { new("organization", "One", "/one"), new("organization", "Two", "/two") } },
                new() { [nameof(NavigationChoices.Items)] = new NavigationChoiceItem[] { new("organization", " ", "/one") } }
            })
            {
                var parameters = Parameters(); foreach (var (key, value) in replacement) parameters[key] = value;
                await Reject(parameters, replacement.Keys.First());
            }
            foreach (var href in new[] { "https://example.invalid/", "//example.invalid/", "javascript:alert(1)", "relative", "/bad\\path", "/bad path", "/bad\npath" })
            {
                var parameters = Parameters();
                parameters[nameof(NavigationChoices.Items)] = new NavigationChoiceItem[] { new("organization", "Organization", href) };
                await Reject(parameters, nameof(NavigationChoices.Items));
            }
        }));
        tests.Add(("navigation choices keep passive contracts outside Framework and use one packaged style core", () =>
        {
            Require(typeof(NavigationChoiceItem).Assembly == typeof(ITablePreferences).Assembly, "The choice item contract depends on the renderer assembly.");
            var root = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../.."));
            var framework = File.ReadAllText(Path.Combine(root, "src/Flourish.Blazor/Flourish.Blazor.Framework/wwwroot/navigation-choices.css"));
            var design = File.ReadAllText(Path.Combine(root, "src/Flourish.Blazor/Flourish.Blazor.Design/wwwroot/navigation-choices.css"));
            Require(framework.Contains(".f-navigation-choice-panel[hidden] { display:none!important; }", StringComparison.Ordinal), "Design styles can accidentally reveal inactive native forms.");
            Require(design.Contains(".f-navigation-choices-nav > .f-uniform-grid { margin-block:0; }", StringComparison.Ordinal), "Native choice composition reintroduced host spacing rules.");
            return Task.CompletedTask;
        }));
    }

    private static Dictionary<string, object?> Parameters() => new()
    {
        [nameof(NavigationChoices.Id)] = "entry", [nameof(NavigationChoices.Label)] = "Access method",
        [nameof(NavigationChoices.ActiveKey)] = "organization",
        [nameof(NavigationChoices.Items)] = new NavigationChoiceItem[]
        {
            new("organization", "Organization", "/entry?method=organization"), new("account", "Account", "/entry?method=account"), new("pass", "Pass", "/entry?method=pass")
        },
        [nameof(NavigationChoices.ChildContent)] = (RenderFragment<NavigationChoiceItem>)(choice => builder =>
        {
            builder.OpenElement(0, "form"); builder.AddAttribute(1, "method", "post"); builder.AddAttribute(2, "action", "/native-post/" + choice.Key);
            builder.OpenElement(3, "input"); builder.AddAttribute(4, "type", "hidden"); builder.AddAttribute(5, "name", "protocol-secret"); builder.AddAttribute(6, "value", choice.Key); builder.CloseElement();
            builder.CloseElement();
        })
    };
    private static string OpeningTag(string html, string tag, string id)
    {
        var match = Regex.Match(html, "<" + tag + "\\b(?=[^>]*\\bid=\"" + Regex.Escape(id) + "\")[^>]*>");
        Require(match.Success, "Missing production " + tag + " with id " + id);
        return match.Value;
    }
    private static async Task<string> Render(Dictionary<string, object?> parameters)
    {
        var result = string.Empty;
        await WithComponent(parameters, (_, _, html) => { result = html(); return Task.CompletedTask; });
        return result;
    }
    private static async Task WithComponent(Dictionary<string, object?> parameters, Func<NavigationChoices, CaptureActivator, Func<string>, Task> verify)
    {
        var activator = new CaptureActivator();
        var services = new ServiceCollection(); services.AddLogging(); services.AddFlourishFramework();
        services.AddSingleton<NavigationManager>(new TestNavigation()); services.AddSingleton<IJSRuntime, NoInterop>(); services.AddSingleton<IComponentActivator>(activator);
        using var provider = services.BuildServiceProvider();
        await using var renderer = new HtmlRenderer(provider, provider.GetRequiredService<ILoggerFactory>());
        await renderer.Dispatcher.InvokeAsync(async () =>
        {
            var output = await renderer.RenderComponentAsync<NavigationChoices>(ParameterView.FromDictionary(parameters));
            await verify(activator.Instances.OfType<NavigationChoices>().Single(), activator, output.ToHtmlString);
        });
    }
    private static async Task Reject(Dictionary<string, object?> parameters, string parameter)
    {
        try { await Render(parameters); }
        catch (ArgumentException exception) when (exception.ParamName == parameter) { return; }
        throw new InvalidOperationException("Malformed navigation choice parameter accepted: " + parameter);
    }
    private static void Require(bool condition, string message) { if (!condition) throw new InvalidOperationException(message); }
    private sealed class TestNavigation : NavigationManager { internal TestNavigation() => Initialize("https://example.test/", "https://example.test/entry"); }
    private sealed class NoInterop : IJSRuntime
    {
        public ValueTask<TValue> InvokeAsync<TValue>(string identifier, object?[]? args) => throw new InvalidOperationException("Native GET choices must not import JavaScript: " + identifier);
        public ValueTask<TValue> InvokeAsync<TValue>(string identifier, CancellationToken cancellationToken, object?[]? args) => throw new InvalidOperationException("Native GET choices must not import JavaScript: " + identifier);
    }
    private sealed class CaptureActivator : IComponentActivator
    {
        internal List<IComponent> Instances { get; } = [];
        public IComponent CreateInstance(Type type) { var component = (IComponent)Activator.CreateInstance(type)!; Instances.Add(component); return component; }
    }
    private sealed class RetainedPanel : ComponentBase
    {
        public RetainedPanel() { }
        [Parameter] public string Key { get; set; } = string.Empty;
        protected override void BuildRenderTree(RenderTreeBuilder builder) { builder.OpenElement(0, "p"); builder.AddContent(1, Key); builder.CloseElement(); }
    }
}
