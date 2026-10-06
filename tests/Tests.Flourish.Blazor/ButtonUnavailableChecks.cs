using System.Net;
using System.Reflection;
using System.Text.RegularExpressions;
using ArkheideSystem.Flourish.Blazor;
using ArkheideSystem.Flourish.Blazor.Abstract;
using ArkheideSystem.Flourish.Blazor.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

internal static class ButtonUnavailableChecks
{
    internal static void Register(List<(string Name, Func<Task> Run)> tests)
    {
        foreach (var link in new[] { false, true })
        {
            tests.Add(($"Elevated {(link ? "link" : "submit")} preserves unavailable semantics and refuses activation", async () =>
            {
                foreach (var (disabled, busy) in new[] { (true, false), (false, true), (true, true) })
                {
                    var calls = 0;
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
                        var parameters = new Dictionary<string, object?>
                        {
                            [nameof(Button.Variant)] = ButtonVariant.Elevated,
                            [nameof(Button.Text)] = "Contact <support>",
                            [nameof(Button.Type)] = "submit",
                            [nameof(Button.Disabled)] = disabled,
                            [nameof(Button.Busy)] = busy,
                            [nameof(Button.BusyLabel)] = "Preparing contact",
                            [nameof(Button.OnClick)] = EventCallback.Factory.Create<MouseEventArgs>(new object(), (MouseEventArgs _) => calls++)
                        };
                        if (link)
                        {
                            parameters[nameof(Button.Href)] = "/support";
                            parameters[nameof(Button.AdditionalAttributes)] = new Dictionary<string, object>
                                { ["HREF"] = "/unsafe-override", ["tabindex"] = 0 };
                        }
                        var output = await renderer.RenderComponentAsync<Button>(ParameterView.FromDictionary(parameters));
                        var button = activator.Button ?? throw new InvalidOperationException("Button was not instantiated.");
                        var html = output.ToHtmlString();
                        var tag = OpeningTag(html, link ? "a" : "button");
                        Require(Attribute(tag, "class")?.Split(' ', StringSplitOptions.RemoveEmptyEntries).Contains("f-button-elevated") == true,
                            "Unavailable state lost the real Elevated variant identity.");
                        Require(Attribute(tag, "aria-busy") == (busy ? "true" : "false"), "Busy announcement changed.");
                        Require(html.Contains("Contact &lt;support&gt;", StringComparison.Ordinal), "Button text was not encoded.");
                        if (link)
                        {
                            Require(!Regex.IsMatch(tag, "\\bhref=", RegexOptions.IgnoreCase), "Unavailable Elevated link remained navigable through unmatched HREF.");
                            Require(Attribute(tag, "aria-disabled") == "true" && Attribute(tag, "tabindex") == "-1",
                                "Unavailable Elevated link remained enabled or in the Tab sequence.");
                        }
                        else Require(Attribute(tag, "type") == "submit" && Regex.IsMatch(tag, "\\sdisabled(?:[\\s=>])"),
                            "Unavailable Elevated submit lost native disabled semantics.");
                        if (busy) Require(html.Contains("Preparing contact", StringComparison.Ordinal) && html.Contains("f-spinner", StringComparison.Ordinal),
                            "Busy state lost the accessible label or standard spinner.");
                        await Click(button);
                        await Click(button);
                        Require(calls == 0, "Unavailable Elevated action invoked its callback.");
                        parameters[nameof(Button.Disabled)] = false;
                        parameters[nameof(Button.Busy)] = false;
                        await button.SetParametersAsync(ParameterView.FromDictionary(parameters));
                        var availableTag = OpeningTag(output.ToHtmlString(), link ? "a" : "button");
                        if (link) Require(Attribute(availableTag, "href") == "/support" && Attribute(availableTag, "aria-disabled") == "false",
                            "Re-enabled Elevated link did not restore only its actual destination.");
                        else Require(!Regex.IsMatch(availableTag, "\\sdisabled(?:[\\s=>])"), "Re-enabled Elevated submit remained disabled.");
                        await Click(button);
                        Require(calls == 1, "Re-enabled Elevated action failed to dispatch exactly once.");
                    });
                }
            }));
        }
    }

    private static string OpeningTag(string html, string name) => Regex.Matches(html, $"<{name}\\b[^>]*>").Single().Value;
    private static string? Attribute(string tag, string name)
    {
        var match = Regex.Match(tag, $"(?:^|\\s){Regex.Escape(name)}=\"([^\"]*)\"");
        return match.Success ? WebUtility.HtmlDecode(match.Groups[1].Value) : null;
    }
    private static Task Click(Button button) => (Task)(typeof(Button).GetMethod("ClickAsync", BindingFlags.Instance | BindingFlags.NonPublic)!
        .Invoke(button, [new MouseEventArgs()]) ?? throw new InvalidOperationException("Button callback returned no task."));
    private static void Require(bool condition, string message) { if (!condition) throw new InvalidOperationException(message); }
    private sealed class ButtonActivator : IComponentActivator
    {
        internal Button? Button { get; private set; }
        public IComponent CreateInstance(Type componentType)
        {
            var component = (IComponent)Activator.CreateInstance(componentType)!;
            if (component is Button button) Button = button;
            return component;
        }
    }
}
