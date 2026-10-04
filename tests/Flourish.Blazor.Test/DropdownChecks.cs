using System.Net;
using System.Text.RegularExpressions;
using ArkheideSystem.Flourish.Blazor;
using ArkheideSystem.Flourish.Blazor.Components.Primitives;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.RenderTree;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;

internal static class DropdownChecks
{
    internal static void Register(List<(string Name, Func<Task> Run)> tests)
    {
        tests.Add(("reference dropdown renders explicit collapsed and expanded states through its actual trigger", async () =>
        {
            await WithDropdown(Parameters(), async (dropdown, html) =>
            {
                Require(Attribute(Trigger(html()), "aria-expanded") == "false", "A closed trigger does not announce an explicit collapsed state.");
                Require(!html().Contains("role=\"listbox\"", StringComparison.Ordinal), "A closed popup remains exposed as a listbox.");
                await dropdown.ClickTriggerAsync();
                var opened = html();
                Require(Attribute(Trigger(opened), "aria-expanded") == "true", "Opening rendered a minimized or missing expanded attribute.");
                var controls = Attribute(Trigger(opened), "aria-controls");
                Require(controls.Length > 0 && Regex.IsMatch(opened, $@"<div\b[^>]*\bid=""{Regex.Escape(controls)}""[^>]*\brole=""listbox"""), "The expanded trigger does not identify its actual listbox.");
                await dropdown.ClickTriggerAsync();
                Require(Attribute(Trigger(html()), "aria-expanded") == "false" && !html().Contains("role=\"listbox\"", StringComparison.Ordinal), "Closing retained an expanded state or stale popup.");
            });
        }));
        tests.Add(("reference dropdown announces only the optional empty choice when no item is selected", async () =>
        {
            await WithDropdown(Parameters(), async (dropdown, html) =>
            {
                await dropdown.ClickTriggerAsync();
                var options = Options(html());
                Require(options.Count == 3, "The empty choice or one supplied option was lost.");
                Require(options.Select(option => Attribute(option.Tag, "aria-selected")).SequenceEqual(["true", "false", "false"]), "An unselected option emits a minimized or missing selected attribute.");
                Require(options[0].Text == "None", "The selected empty choice lost its consumer label.");
            });
        }));
        tests.Add(("reference dropdown identifies the selected typed item and keeps other options explicitly unselected", async () =>
        {
            var parameters = Parameters();
            parameters[nameof(ReferenceDropdown<Guid>.SelectedId)] = FirstId;
            await WithDropdown(parameters, async (dropdown, html) =>
            {
                await dropdown.ClickTriggerAsync();
                var rendered = html();
                var options = Options(rendered);
                Require(options.Count == 3 && options.Select(option => Attribute(option.Tag, "aria-selected")).SequenceEqual(["false", "true", "false"]), "Selection differs between the empty, selected and remaining options.");
                Require(options[1].Text == "<Alpha & selected>" && rendered.Contains("&lt;Alpha &amp; selected&gt;", StringComparison.Ordinal), "Fixing selection changed the consumer text or its HTML encoding.");
            });
        }));
        tests.Add(("disabled reference dropdown retains collapsed semantics when its trigger handler is invoked", async () =>
        {
            var parameters = Parameters();
            parameters[nameof(ReferenceDropdown<Guid>.Disabled)] = true;
            await WithDropdown(parameters, async (dropdown, html) =>
            {
                Require(Regex.IsMatch(Trigger(html()), @"\bdisabled(?:\s|=|>)"), "The disabled trigger lost its native disabled attribute.");
                await dropdown.ClickTriggerAsync();
                Require(Attribute(Trigger(html()), "aria-expanded") == "false" && !html().Contains("role=\"listbox\"", StringComparison.Ordinal), "A disabled trigger opened or contradicted its collapsed state.");
            });
        }));
    }

    private static readonly Guid FirstId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    private static readonly Guid SecondId = Guid.Parse("22222222-2222-2222-2222-222222222222");
    private static Dictionary<string, object?> Parameters() => new()
    {
        [nameof(ReferenceDropdown<Guid>.Id)] = "reference-test",
        [nameof(ReferenceDropdown<Guid>.LabelledBy)] = "reference-label",
        [nameof(ReferenceDropdown<Guid>.Items)] = new ReferenceItem<Guid>[] { new(FirstId, "<Alpha & selected>"), new(SecondId, "Beta") },
        [nameof(ReferenceDropdown<Guid>.EmptySelectionText)] = "None",
        [nameof(ReferenceDropdown<Guid>.AllowNone)] = true
    };

    private static async Task WithDropdown(Dictionary<string, object?> parameters, Func<DropdownProbe, Func<string>, Task> verify)
    {
        var activator = new CapturingActivator();
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton<IJSRuntime>(new FakeJs());
        services.AddSingleton<IComponentActivator>(activator);
        services.AddFlourishFramework();
        using var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();
        await using var renderer = new HtmlRenderer(scope.ServiceProvider, provider.GetRequiredService<ILoggerFactory>());
        await renderer.Dispatcher.InvokeAsync(async () =>
        {
            var output = await renderer.RenderComponentAsync<DropdownProbe>(ParameterView.FromDictionary(parameters));
            await verify(activator.Dropdown ?? throw new InvalidOperationException("The renderer did not construct the dropdown."), output.ToHtmlString);
        });
    }

    private static string Trigger(string html)
    {
        var match = Regex.Match(html, @"<button\b[^>]*\bid=""reference-test-trigger""[^>]*>");
        return match.Success ? match.Value : throw new InvalidOperationException("The reference trigger was not rendered.");
    }

    private static List<(string Tag, string Text)> Options(string html) =>
        Regex.Matches(html, @"(<button\b[^>]*\brole=""option""[^>]*>)([\s\S]*?)</button>")
            .Select(match => (match.Groups[1].Value, WebUtility.HtmlDecode(match.Groups[2].Value))).ToList();

    private static string Attribute(string tag, string name)
    {
        var match = Regex.Match(tag, $@"\b{Regex.Escape(name)}=""([^""]*)""");
        return match.Success ? WebUtility.HtmlDecode(match.Groups[1].Value) : "";
    }

    private static void Require(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }

    private sealed class CapturingActivator : IComponentActivator
    {
        internal DropdownProbe? Dropdown { get; private set; }
        public IComponent CreateInstance(Type componentType)
        {
            var component = (IComponent)Activator.CreateInstance(componentType)!;
            if (component is DropdownProbe dropdown) Dropdown = dropdown;
            return component;
        }
    }

    private sealed class DropdownProbe : ReferenceDropdown<Guid>
    {
        public DropdownProbe() { }

        // Replay the emitted onclick callback: no private state is set and no test-only UI API is added.
#pragma warning disable BL0006
        internal Task ClickTriggerAsync()
        {
            using var outer = new RenderTreeBuilder();
            base.BuildRenderTree(outer);
            var outerFrames = outer.GetFrames();
            RenderFragment? content = null;
            for (var index = 0; index < outerFrames.Count; index++)
            {
                var frame = outerFrames.Array[index];
                if (frame.FrameType == RenderTreeFrameType.Attribute && frame.AttributeName == "ChildContent")
                    content = frame.AttributeValue as RenderFragment;
            }
            if (content is null) throw new InvalidOperationException("The reference dropdown lost its rendered content.");
            using var body = new RenderTreeBuilder();
            content(body);
            var bodyFrames = body.GetFrames();
            for (var index = 0; index < bodyFrames.Count; index++)
            {
                var frame = bodyFrames.Array[index];
                if (frame.FrameType != RenderTreeFrameType.Attribute || frame.AttributeName != "onclick") continue;
                return frame.AttributeValue switch
                {
                    EventCallback<MouseEventArgs> callback => callback.InvokeAsync(new MouseEventArgs()),
                    EventCallback callback => callback.InvokeAsync(new MouseEventArgs()),
                    MulticastDelegate callback => ((IHandleEvent)this).HandleEventAsync(new EventCallbackWorkItem(callback), new MouseEventArgs()),
                    _ => throw new InvalidOperationException("The rendered trigger has no invocable event callback.")
                };
            }
            throw new InvalidOperationException("The rendered reference trigger has no onclick handler.");
        }
#pragma warning restore BL0006
    }
}
