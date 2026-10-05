using System.Net;
using System.Reflection;
using System.Text.RegularExpressions;
using ArkheideSystem.Flourish.Blazor;
using ArkheideSystem.Flourish.Blazor.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

internal static class SplitButtonChecks
{
    internal static void Register(List<(string Name, Func<Task> Run)> tests)
    {
        tests.Add(("split links share selection while disclosure expansion remains independent", async () =>
        {
            var parameters = new Dictionary<string, object?>
            {
                [nameof(SplitButton.Href)] = "/reports", [nameof(SplitButton.Text)] = "Reports",
                [nameof(SplitButton.Icon)] = "description", [nameof(SplitButton.Selected)] = true,
                [nameof(SplitButton.Expanded)] = false, [nameof(SplitButton.SecondaryControls)] = "report-options",
                [nameof(SplitButton.SecondaryLabel)] = "Expand report options",
                [nameof(SplitButton.PrimaryClass)] = "host-link", [nameof(SplitButton.SecondaryClass)] = "host-toggle",
                [nameof(SplitButton.PrimaryAttributes)] = new Dictionary<string, object> { ["title"] = "All reports", ["aria-current"] = "page" },
                [nameof(SplitButton.SecondaryAttributes)] = new Dictionary<string, object> { ["data-action"] = "report-options" }
            };
            await WithButton(parameters, async (button, html) =>
            {
                var primary = Tag(html(), "a", "f-split-button-primary");
                var secondary = Tag(html(), "button", "f-split-button-secondary");
                Require(Attribute(primary, "href") == "/reports", "The main link lost its destination.");
                Require(HasClass(primary, "is-selected") && HasClass(secondary, "is-selected"), "Selection differs between the two controls.");
                Require(HasClass(primary, "host-link") && HasClass(secondary, "host-toggle"), "Host class hooks were lost.");
                Require(Attribute(primary, "aria-current") == "page" && Attribute(primary, "title") == "All reports", "Main link attributes were lost.");
                Require(Attribute(secondary, "aria-expanded") == "false" && Attribute(secondary, "aria-controls") == "report-options", "The disclosure relationship was lost.");
                Require(Attribute(secondary, "aria-label") == "Expand report options" && Attribute(secondary, "data-action") == "report-options", "Secondary attributes or accessible name were lost.");
                Require(Attribute(secondary, "type") == "button" && Attribute(secondary, "aria-current") is null, "The secondary action became a submit or route action.");
                Require(Attribute(Tag(html(), "span", "f-expansion-indicator"), "data-expanded") == "false", "Collapsed splits lost the shared expansion marker.");
                parameters[nameof(SplitButton.Selected)] = false;
                parameters[nameof(SplitButton.Expanded)] = true;
                parameters[nameof(SplitButton.PrimaryAttributes)] = new Dictionary<string, object> { ["title"] = "All reports" };
                await button.SetParametersAsync(ParameterView.FromDictionary(parameters));
                Require(!HasClass(Tag(html(), "a", "f-split-button-primary"), "is-selected") && !HasClass(Tag(html(), "button", "f-split-button-secondary"), "is-selected"), "Expanding the secondary region selected the two controls.");
                Require(Attribute(Tag(html(), "button", "f-split-button-secondary"), "aria-expanded") == "true", "Controlled expansion did not update.");
                Require(Attribute(Tag(html(), "span", "f-expansion-indicator"), "data-expanded") == "true", "The shared expansion marker did not follow controlled expansion.");
            });
        }));
        tests.Add(("split actions keep submit and secondary-button semantics separate", async () =>
        {
            await WithButton(new Dictionary<string, object?>
            {
                [nameof(SplitButton.Type)] = "submit", [nameof(SplitButton.Text)] = "Save",
                [nameof(SplitButton.SecondaryLabel)] = "Other save actions",
                [nameof(SplitButton.SecondaryContent)] = (RenderFragment)(builder => builder.AddContent(0, "Options"))
            }, (_, html) =>
            {
                var primary = Tag(html(), "button", "f-split-button-primary");
                var secondary = Tag(html(), "button", "f-split-button-secondary");
                Require(Attribute(primary, "type") == "submit" && Attribute(secondary, "type") == "button", "Both halves submit the host form.");
                Require(Attribute(secondary, "aria-expanded") is null && Attribute(secondary, "aria-controls") is null, "A plain secondary action invents a disclosure region.");
                Require(html().Contains("Options", StringComparison.Ordinal), "Custom secondary content was lost.");
                Require(!html().Contains("f-secondary-item", StringComparison.Ordinal), "The generic split button acquired navigation-specific classes.");
                return Task.CompletedTask;
            });
        }));
        tests.Add(("split callbacks remain independent and disabled links cannot dispatch", async () =>
        {
            var primaryCalls = 0;
            var secondaryCalls = 0;
            var parameters = new Dictionary<string, object?>
            {
                [nameof(SplitButton.Href)] = "/reports", [nameof(SplitButton.Text)] = "Reports",
                [nameof(SplitButton.PrimaryAttributes)] = new Dictionary<string, object> { ["HREF"] = "/must-not-navigate" },
                [nameof(SplitButton.OnClick)] = EventCallback.Factory.Create<MouseEventArgs>(new object(), (MouseEventArgs _) => { primaryCalls++; }),
                [nameof(SplitButton.OnSecondaryClick)] = EventCallback.Factory.Create<MouseEventArgs>(new object(), (MouseEventArgs _) => { secondaryCalls++; })
            };
            await WithButton(parameters, async (button, html) =>
            {
                await Invoke(button, "ClickAsync");
                Require(primaryCalls == 1 && secondaryCalls == 0, "A main click also dispatched the secondary action.");
                await Invoke(button, "SecondaryClickAsync");
                Require(primaryCalls == 1 && secondaryCalls == 1, "A secondary click also dispatched the main action.");
                parameters[nameof(SplitButton.Disabled)] = true;
                await button.SetParametersAsync(ParameterView.FromDictionary(parameters));
                var primary = Tag(html(), "a", "f-split-button-primary");
                var secondary = Tag(html(), "button", "f-split-button-secondary");
                Require(Attribute(primary, "href") is null && Attribute(primary, "aria-disabled") == "true" && Attribute(primary, "tabindex") == "-1", "A disabled main link remains navigable or tabbable.");
                Require(Attribute(primary, "role") == "link" && !Regex.IsMatch(primary, "\\shref=", RegexOptions.IgnoreCase), "Passthrough attributes revived a disabled link.");
                Require(Regex.IsMatch(secondary, "\\sdisabled(?:[\\s=>])") && Attribute(secondary, "aria-disabled") == "true", "The secondary button remains enabled.");
                await Invoke(button, "ClickAsync");
                await Invoke(button, "SecondaryClickAsync");
                Require(primaryCalls == 1 && secondaryCalls == 1, "Disabled state still allows a callback to dispatch.");
            });
        }));
        tests.Add(("split whitespace destinations use native buttons without empty icon placeholders", async () =>
        {
            await WithButton(new Dictionary<string, object?>
            {
                [nameof(SplitButton.Href)] = "   ", [nameof(SplitButton.Icon)] = " \t ",
                [nameof(SplitButton.Text)] = "Save", [nameof(SplitButton.SecondaryIcon)] = string.Empty,
                [nameof(SplitButton.SecondaryContent)] = (RenderFragment)(builder => builder.AddContent(0, "Options"))
            }, (_, html) =>
            {
                Require(!Regex.IsMatch(html(), "<a\\b"), "A whitespace destination became an empty navigation link.");
                Require(Attribute(Tag(html(), "button", "f-split-button-primary"), "type") == "button", "The primary action lost its native button fallback.");
                Require(!html().Contains("data-icon=", StringComparison.Ordinal), "An empty icon became a question-mark glyph.");
                return Task.CompletedTask;
            });
        }));
    }

    private static async Task WithButton(Dictionary<string, object?> parameters, Func<SplitButton, Func<string>, Task> verify)
    {
        var activator = new ButtonActivator();
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddFlourishFramework();
        services.AddSingleton<NavigationManager>(new TestNavigation());
        services.AddSingleton<IComponentActivator>(activator);
        using var provider = services.BuildServiceProvider();
        await using var renderer = new HtmlRenderer(provider, provider.GetRequiredService<ILoggerFactory>());
        await renderer.Dispatcher.InvokeAsync(async () =>
        {
            var output = await renderer.RenderComponentAsync<SplitButton>(ParameterView.FromDictionary(parameters));
            await verify(activator.Button ?? throw new InvalidOperationException("Split button was not created."), output.ToHtmlString);
        });
    }

    private static Task Invoke(SplitButton button, string methodName)
    {
        var method = typeof(SplitButton).GetMethod(methodName, BindingFlags.Instance | BindingFlags.NonPublic)
            ?? throw new InvalidOperationException("Split button handler was not found.");
        return (Task)(method.Invoke(button, [new MouseEventArgs()]) ?? throw new InvalidOperationException("Split button handler returned no task."));
    }
    private static string Tag(string html, string name, string cssClass) => Regex.Matches(html, $"<{name}\\b[^>]*>")
        .Select(match => match.Value).Single(tag => HasClass(tag, cssClass));
    private static string? Attribute(string tag, string name)
    {
        var match = Regex.Match(tag, $"(?:^|\\s){Regex.Escape(name)}=\"([^\"]*)\"");
        return match.Success ? WebUtility.HtmlDecode(match.Groups[1].Value) : null;
    }
    private static bool HasClass(string tag, string cssClass) => Attribute(tag, "class")?.Split(' ', StringSplitOptions.RemoveEmptyEntries).Contains(cssClass) == true;
    private static void Require(bool condition, string message) { if (!condition) throw new InvalidOperationException(message); }
    private sealed class ButtonActivator : IComponentActivator
    {
        internal SplitButton? Button { get; private set; }
        public IComponent CreateInstance(Type componentType)
        {
            var component = (IComponent)Activator.CreateInstance(componentType)!;
            if (component is SplitButton button) Button = button;
            return component;
        }
    }
}
