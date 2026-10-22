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
        tests.Add(("compact navigation choices use standard buttons without changing native GET destinations", async () =>
        {
            var parameters = Parameters();
            parameters[nameof(NavigationChoices.Layout)] = NavigationChoicesLayout.Compact;
            await WithComponent(parameters, (_, activator, html) =>
            {
                var output = html();
                Require(output.Contains("class=\"f-navigation-choices-links\"", StringComparison.Ordinal)
                    && output.Contains("--f-choice-columns:3", StringComparison.Ordinal), "Compact choices did not expose the three-column library layout.");
                var buttons = activator.Instances.OfType<Button>().ToArray();
                Require(buttons.Length == 3 && !activator.Instances.OfType<UniformGridButton>().Any(), "Compact choices did not instantiate the standard Button renderer exclusively.");
                foreach (var key in new[] { "organization", "account", "pass" })
                {
                    var choice = OpeningTag(output, "a", "entry-" + key + "-choice");
                    var selected = key == "organization";
                    Require(choice.Contains("f-button ", StringComparison.Ordinal)
                        && choice.Contains(selected ? "f-button-secondary" : "f-button-quiet", StringComparison.Ordinal)
                        && !choice.Contains("f-uniform-grid-button", StringComparison.Ordinal), "Compact choice appearance bypassed standard button variants.");
                    Require(choice.Contains("href=\"/entry?method=" + key + "\"", StringComparison.Ordinal)
                        && choice.Contains("data-enhance-nav=\"false\"", StringComparison.Ordinal), "Compact mode changed a native GET destination or enhanced-navigation boundary.");
                    Require(choice.Contains("aria-current=\"page\"", StringComparison.Ordinal) == selected, "Compact mode selected the wrong route.");
                    Require(Regex.IsMatch(OpeningTag(output, "section", "entry-" + key + "-panel"), @"\shidden(?:\s|=|>)") == !selected, "Compact mode exposed an inactive native panel.");
                }
                Require(Regex.Matches(output, "<form method=\"post\"").Count == 3
                    && !Regex.IsMatch(output, "role=\"(?:tablist|tab|tabpanel)\"|aria-selected=|aria-controls="), "Compact mode dropped native forms or invented interactive ARIA tabs.");
                return Task.CompletedTask;
            });
        }));
        tests.Add(("compact navigation choices use selected production button variants without changing retained forms or destinations", async () =>
        {
            var parameters = Parameters();
            parameters[nameof(NavigationChoices.Layout)] = NavigationChoicesLayout.Compact;
            parameters[nameof(NavigationChoices.Variant)] = ButtonVariant.Elevated;
            parameters[nameof(NavigationChoices.ActiveVariant)] = ButtonVariant.Elevated;
            await WithComponent(parameters, async (component, activator, html) =>
            {
                var buttons = activator.Instances.OfType<Button>().ToArray();
                Require(buttons.Length == 3 && buttons.All(button => button.Variant == ButtonVariant.Elevated), "The selected compact variants bypassed real Button components.");
                var panels = Regex.Matches(html(), @"<section\b[^>]*>.*?</section>", RegexOptions.Singleline).Select(match => match.Value).ToArray();
                foreach (var key in new[] { "organization", "account", "pass" })
                {
                    var choice = OpeningTag(html(), "a", "entry-" + key + "-choice");
                    Require(choice.Contains("f-button-elevated", StringComparison.Ordinal)
                        && choice.Contains("href=\"/entry?method=" + key + "\"", StringComparison.Ordinal)
                        && choice.Contains("data-enhance-nav=\"false\"", StringComparison.Ordinal), "Elevated appearance changed native GET navigation.");
                    Require(choice.Contains("aria-current=\"page\"", StringComparison.Ordinal) == (key == "organization"), "Equal button variants lost the current destination.");
                }
                parameters[nameof(NavigationChoices.Variant)] = ButtonVariant.Quiet;
                parameters[nameof(NavigationChoices.ActiveVariant)] = ButtonVariant.Primary;
                await component.SetParametersAsync(ParameterView.FromDictionary(parameters));
                Require(activator.Instances.OfType<Button>().SequenceEqual(buttons), "Changing a standard variant recreated production choice buttons.");
                Require(buttons.Single(button => button.Href == "/entry?method=organization").Variant == ButtonVariant.Primary
                    && buttons.Where(button => button.Href != "/entry?method=organization").All(button => button.Variant == ButtonVariant.Quiet), "Current and ordinary variants did not update independently.");
                Require(Regex.Matches(html(), @"<section\b[^>]*>.*?</section>", RegexOptions.Singleline).Select(match => match.Value).SequenceEqual(panels), "Button variant changes rewrote host-owned native forms.");
            });
            foreach (var parameter in new[] { nameof(NavigationChoices.Variant), nameof(NavigationChoices.ActiveVariant) })
            {
                var invalid = Parameters();
                invalid[parameter] = (ButtonVariant)(-1);
                await Reject(invalid, parameter);
            }
        }));
        tests.Add(("compact navigation choices retain disabled panels while removing disabled GET activation", async () =>
        {
            var parameters = Parameters();
            parameters[nameof(NavigationChoices.Layout)] = NavigationChoicesLayout.Compact;
            parameters[nameof(NavigationChoices.Items)] = new NavigationChoiceItem[]
            {
                new("organization", "Organization", "/entry?method=organization"), new("account", "Account", "/entry?method=account", Disabled:true)
            };
            var html = await Render(parameters);
            var disabled = OpeningTag(html, "a", "entry-account-choice");
            Require(disabled.Contains("f-button-quiet", StringComparison.Ordinal)
                && !disabled.Contains("href=", StringComparison.Ordinal)
                && disabled.Contains("aria-disabled=\"true\"", StringComparison.Ordinal)
                && disabled.Contains("tabindex=\"-1\"", StringComparison.Ordinal), "A disabled compact choice retained navigation or bypassed standard disabled appearance.");
            Require(Regex.IsMatch(OpeningTag(html, "section", "entry-account-panel"), @"\shidden(?:\s|=|>)")
                && html.Contains("name=\"protocol-secret\" value=\"account\"", StringComparison.Ordinal), "The disabled compact choice discarded its host-owned form panel.");
            parameters[nameof(NavigationChoices.ActiveKey)] = "account";
            await Reject(parameters, nameof(NavigationChoices.ActiveKey));
        }));
        tests.Add(("compact navigation choices round trip variants without recreating route panels", async () =>
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
                Require(panels.Length == 3, "Every route panel must start mounted.");
                var originalPanels = Regex.Matches(html(), @"<section\b[^>]*>.*?</section>", RegexOptions.Singleline).Select(match => match.Value).ToArray();
                foreach (var compact in new[] { true, false, true })
                {
                    parameters[nameof(NavigationChoices.Layout)] = compact ? NavigationChoicesLayout.Compact : NavigationChoicesLayout.Grid;
                    await component.SetParametersAsync(ParameterView.FromDictionary(parameters));
                    Require(activator.Instances.OfType<RetainedPanel>().SequenceEqual(panels), "Changing Layout recreated host-owned route panel components.");
                    Require(Regex.Matches(html(), @"<section\b[^>]*>.*?</section>", RegexOptions.Singleline).Select(match => match.Value).SequenceEqual(originalPanels), "Changing Layout altered retained panel IDs, content or visibility.");
                    foreach (var key in new[] { "organization", "account", "pass" })
                        Require(OpeningTag(html(), "a", "entry-" + key + "-choice").Contains("href=\"/entry?method=" + key + "\"", StringComparison.Ordinal), "Changing Layout rewrote a GET destination.");
                }
                parameters[nameof(NavigationChoices.ActiveKey)] = "account";
                await component.SetParametersAsync(ParameterView.FromDictionary(parameters));
                Require(activator.Instances.OfType<RetainedPanel>().SequenceEqual(panels)
                    && !Regex.IsMatch(OpeningTag(html(), "section", "entry-account-panel"), @"\shidden(?:\s|=|>)")
                    && OpeningTag(html(), "a", "entry-account-choice").Contains("f-button-secondary", StringComparison.Ordinal), "Compact routing recreated panels or selected the wrong standard button.");
            });
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
                Require(OpeningTag(html(), "a", "entry-account-choice").Contains("f-uniform-variant-filled-elevated", StringComparison.Ordinal), "The new current link did not use FilledElevated.");
                foreach (var key in new[] { "organization", "pass" })
                    Require(OpeningTag(html(), "a", "entry-" + key + "-choice").Contains("f-uniform-variant-elevated", StringComparison.Ordinal), "An inactive choice did not use Elevated.");
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
        tests.Add(("sidebar navigation choices preserve native forms current identity and decorative icons", async () =>
        {
            var parameters = Parameters();
            parameters[nameof(NavigationChoices.Layout)] = NavigationChoicesLayout.Sidebar;
            parameters[nameof(NavigationChoices.Items)] = new NavigationChoiceItem[]
            {
                new("organization", "Organization", "/entry?method=organization", Icon:"apartment"),
                new("account", "Account", "/entry?method=account", Icon:"account_circle"),
                new("pass", "Unavailable", "/entry?method=pass", Disabled:true)
            };
            await WithComponent(parameters, async (component, activator, html) =>
            {
                var buttons = activator.Instances.OfType<Button>().ToArray();
                Require(buttons.Length == 3 && !activator.Instances.OfType<UniformGridButton>().Any(),
                    "Sidebar choices must use the real standard Button renderer.");
                Require(html().Contains("f-navigation-choices-sidebar", StringComparison.Ordinal)
                    && html().Contains("data-icon=\"apartment\"", StringComparison.Ordinal)
                    && html().Contains("data-icon=\"account_circle\"", StringComparison.Ordinal),
                    "Sidebar layout or optional library icons disappeared.");
                Require(Regex.Matches(html(), "data-icon=\"check\"").Count == 1
                    && Regex.Matches(html(), "aria-current=\"page\"").Count == 1,
                    "The current route needs exactly one decorative check and current-page marker.");
                foreach (var key in new[] { "organization", "account", "pass" })
                {
                    var panel = OpeningTag(html(), "section", "entry-" + key + "-panel");
                    Require(panel.Contains("aria-labelledby=\"entry-" + key + "-choice\"", StringComparison.Ordinal)
                        && Regex.IsMatch(panel, @"\shidden(?:\s|=|>)") == (key != "organization"),
                        "A sidebar panel lost its choice name or visibility.");
                }
                var disabled = OpeningTag(html(), "a", "entry-pass-choice");
                Require(!disabled.Contains("href=", StringComparison.Ordinal)
                    && disabled.Contains("aria-disabled=\"true\"", StringComparison.Ordinal)
                    && disabled.Contains("tabindex=\"-1\"", StringComparison.Ordinal),
                    "Disabled sidebar navigation remained actionable.");
                Require(Regex.Matches(html(), "<form method=\"post\"").Count == 3
                    && !Regex.IsMatch(html(), "role=\"(?:tablist|tab|tabpanel)\"|aria-selected=|aria-controls="),
                    "Sidebar layout replaced native host forms or invented interactive tabs.");
                parameters[nameof(NavigationChoices.ActiveKey)] = "account";
                await component.SetParametersAsync(ParameterView.FromDictionary(parameters));
                Require(activator.Instances.OfType<Button>().SequenceEqual(buttons),
                    "Changing the selected route recreated standard choice controls.");
                var accountLink = Regex.Match(html(), @"<a\b(?=[^>]*\bid=""entry-account-choice"")[^>]*>.*?</a>", RegexOptions.Singleline).Value;
                Require(accountLink.Contains("aria-current=\"page\"", StringComparison.Ordinal)
                    && accountLink.Contains("data-icon=\"check\"", StringComparison.Ordinal)
                    && accountLink.Contains("href=\"/entry?method=account\"", StringComparison.Ordinal)
                    && accountLink.Contains("data-enhance-nav=\"false\"", StringComparison.Ordinal),
                    "The selected sidebar item lost its current state or original GET destination.");
                foreach (var variant in new[] { ButtonVariant.Primary, ButtonVariant.Danger, ButtonVariant.Elevated })
                {
                    parameters[nameof(NavigationChoices.ActiveVariant)] = variant;
                    await component.SetParametersAsync(ParameterView.FromDictionary(parameters));
                    var currentButton = activator.Instances.OfType<Button>().Single(button => button.Href == "/entry?method=account");
                    Require(currentButton.Variant == variant
                        && OpeningTag(html(), "a", "entry-account-choice").Contains("f-button-" + variant.ToString().ToLowerInvariant(), StringComparison.Ordinal),
                        "Sidebar replaced the host-selected standard current Button variant.");
                    Require(buttons.Where(button => button.Href != "/entry?method=account").All(button => button.Variant == ButtonVariant.Quiet),
                        "Changing ActiveVariant changed inactive navigation variants.");
                }
                parameters[nameof(NavigationChoices.ShowActiveIndicator)] = false;
                await component.SetParametersAsync(ParameterView.FromDictionary(parameters));
                Require(!html().Contains("data-icon=\"check\"", StringComparison.Ordinal)
                    && html().Contains("data-icon=\"account_circle\"", StringComparison.Ordinal)
                    && Regex.Matches(html(), "aria-current=\"page\"").Count == 1
                    && Regex.Matches(html(), "<form method=\"post\"").Count == 3,
                    "Hiding the optional check changed icons, accessibility or retained native forms.");
            });
            parameters[nameof(NavigationChoices.Layout)] = (NavigationChoicesLayout)(-1);
            await Reject(parameters, nameof(NavigationChoices.Layout));
        }));
        tests.Add(("navigation choices keep passive contracts outside Framework and use one packaged style core", () =>
        {
            Require(typeof(NavigationChoiceItem).Assembly == typeof(ITablePreferences).Assembly, "The choice item contract depends on the renderer assembly.");
            var root = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "../../../../.."));
            var framework = File.ReadAllText(Path.Combine(root, "src/Flourish.Blazor/Flourish.Blazor.Framework/wwwroot/navigation-choices.css"));
            var design = File.ReadAllText(Path.Combine(root, "src/Flourish.Blazor/Flourish.Blazor.Design/wwwroot/navigation-choices.css"));
            Require(framework.Contains(".f-navigation-choice-panel[hidden] { display:none!important; }", StringComparison.Ordinal), "Design styles can accidentally reveal inactive native forms.");
            Require(design.Contains(".f-navigation-choices-nav > .f-uniform-grid { margin-block:0; }", StringComparison.Ordinal), "Native choice composition reintroduced host spacing rules.");
            Require(framework.Contains(".f-navigation-choices-links { display:grid; grid-template-columns:repeat(var(--f-choice-columns,3),minmax(0,1fr));", StringComparison.Ordinal)
                && framework.Contains("@media(max-width:560px) { .f-navigation-choices-links { grid-template-columns:minmax(0,1fr); } }", StringComparison.Ordinal), "Compact choices lost equal desktop columns or the narrow single-column layout.");
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
