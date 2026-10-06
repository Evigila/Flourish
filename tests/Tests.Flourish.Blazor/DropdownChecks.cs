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
            await WithDropdown(Parameters(), async (dropdown, html, focusCalls) =>
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
            await WithDropdown(Parameters(), async (dropdown, html, focusCalls) =>
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
            parameters[nameof(ReferenceDropdown<Guid>.Value)] = FirstId;
            await WithDropdown(parameters, async (dropdown, html, focusCalls) =>
            {
                await dropdown.ClickTriggerAsync();
                var rendered = html();
                var options = Options(rendered);
                Require(options.Count == 3 && options.Select(option => Attribute(option.Tag, "aria-selected")).SequenceEqual(["false", "true", "false"]), "Selection differs between the empty, selected and remaining options.");
                Require(options[1].Text == "<Alpha & selected>" && rendered.Contains("&lt;Alpha &amp; selected&gt;", StringComparison.Ordinal), "Fixing selection changed the consumer text or its HTML encoding.");
            });
        }));
        tests.Add(("reference dropdown emits nullable ValueChanged through actual selection and clear actions", async () =>
        {
            var changes = new List<Guid?>();
            var parameters = Parameters();
            parameters[nameof(ReferenceDropdown<Guid>.Value)] = FirstId;
            parameters[nameof(ReferenceDropdown<Guid>.ValueChanged)] = EventCallback.Factory.Create<Guid?>(changes, value => changes.Add(value));
            await WithDropdown(parameters, async (dropdown, html, focusCalls) =>
            {
                await dropdown.ClickTriggerAsync();
                await dropdown.ClickOptionAsync(2);
                Require(changes.SequenceEqual(new Guid?[] { SecondId }), "Selecting a real option did not emit its typed ValueChanged value.");
                Require(Attribute(Trigger(html()), "aria-expanded") == "false", "Accepted selection did not close the actual popup.");
                Require(dropdown.Value == FirstId, "The dropdown mutated its controlled Value instead of notifying the host.");
                Require(focusCalls.Count == 1 && focusCalls[0].Id == "reference-test-trigger", "Selection did not restore focus through the captured current browser trigger.");
                await dropdown.ClickTriggerAsync();
                await dropdown.ClickOptionAsync(0);
                Require(changes.SequenceEqual(new Guid?[] { SecondId, null }), "The clear choice did not emit nullable ValueChanged(null).");
                Require(Attribute(Trigger(html()), "aria-expanded") == "false", "Clearing did not close the actual popup.");
                Require(focusCalls.Count == 2 && focusCalls[1].Id == "reference-test-trigger", "Clearing did not restore focus through the captured current browser trigger.");
            });
        }));
        tests.Add(("filtered dropdown reserves all label widths without exposing hidden candidates", async () =>
        {
            await WithDropdown(Parameters(), async (dropdown, html, focusCalls) =>
            {
                await dropdown.ClickTriggerAsync();
                await dropdown.ChangeSearchAsync("Alpha");
                var rendered = html();
                var metrics = Regex.Match(rendered, @"<div\b[^>]*class=""selection-panel-width""[^>]*aria-hidden=""true""[^>]*>([\s\S]*?)</div>");
                Require(metrics.Success && metrics.Groups[1].Value.Contains("Beta", StringComparison.Ordinal), "Filtering discarded the full-label width reservation.");
                Require(metrics.Groups[1].Value.Contains("&lt;Alpha &amp; selected&gt;", StringComparison.Ordinal), "Width measurement bypassed normal label encoding.");
                Require(!Regex.IsMatch(metrics.Groups[1].Value, @"<(button|input)\b"), "Hidden width reservation introduced an interactive candidate.");
                var options = Options(rendered);
                Require(options.Count == 2 && options.All(option => option.Text != "Beta"), "Width-only labels leaked into real selectable results.");
            });
        }));
        tests.Add(("disabled reference dropdown retains collapsed semantics when its trigger handler is invoked", async () =>
        {
            var parameters = Parameters();
            parameters[nameof(ReferenceDropdown<Guid>.Disabled)] = true;
            await WithDropdown(parameters, async (dropdown, html, focusCalls) =>
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

    private static async Task WithDropdown(Dictionary<string, object?> parameters, Func<DropdownProbe, Func<string>, IReadOnlyList<ElementReference>, Task> verify)
    {
        var activator = new CapturingActivator();
        var javascript = new FocusRuntime();
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton<IJSRuntime>(javascript);
        services.AddSingleton<IComponentActivator>(activator);
        services.AddFlourishFramework();
        using var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();
        await using var renderer = new HtmlRenderer(scope.ServiceProvider, provider.GetRequiredService<ILoggerFactory>());
        await renderer.Dispatcher.InvokeAsync(async () =>
        {
            var output = await renderer.RenderComponentAsync<DropdownProbe>(ParameterView.FromDictionary(parameters));
            var dropdown = activator.Dropdown ?? throw new InvalidOperationException("The renderer did not construct the dropdown.");
            dropdown.ConfigureBrowserReferences(new WebElementReferenceContext(javascript));
            await verify(dropdown, output.ToHtmlString, javascript.FocusCalls);
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

    private sealed class FocusRuntime : IJSRuntime
    {
        internal List<ElementReference> FocusCalls { get; } = [];
        public ValueTask<TValue> InvokeAsync<TValue>(string identifier, object?[]? args)
            => InvokeAsync<TValue>(identifier, CancellationToken.None, args);
        public ValueTask<TValue> InvokeAsync<TValue>(string identifier, CancellationToken cancellationToken, object?[]? args)
        {
            Require(identifier == "Blazor._internal.domWrapper.focus", $"Unexpected browser operation: {identifier}.");
            Require(args is { Length: 2 } && args[0] is ElementReference && args[1] is false,
                "FocusAsync did not use the current ElementReference and explicit preventScroll protocol.");
            FocusCalls.Add((ElementReference)args![0]!);
            return ValueTask.FromResult(default(TValue)!);
        }
    }

    private sealed class DropdownProbe : ReferenceDropdown<Guid>
    {
        public DropdownProbe() { }
        private ElementReferenceContext? BrowserContext;
        internal void ConfigureBrowserReferences(ElementReferenceContext context) => BrowserContext = context;

        // SSR does not capture browser references. Replay actual captures with the current web context,
        // then dispatch emitted handlers without setting interaction state or changing production APIs.
#pragma warning disable BL0006
        internal Task ClickTriggerAsync() => DispatchAsync("onclick", new MouseEventArgs());
        internal Task ClickOptionAsync(int index) => DispatchAsync("onclick", new MouseEventArgs(), index + 1);
        internal Task ChangeSearchAsync(string query) => DispatchAsync("oninput", new ChangeEventArgs { Value = query });
        private Task DispatchAsync(string name, object args, int occurrence = 0)
        {
            using var body = new RenderTreeBuilder();
            base.BuildRenderTree(body);
            var bodyFrames = body.GetFrames();
            CaptureBrowserReferences(bodyFrames);
            for (var index = 0; index < bodyFrames.Count; index++)
            {
                var frame = bodyFrames.Array[index];
                if (frame.FrameType != RenderTreeFrameType.Attribute || frame.AttributeName != name) continue;
                if (occurrence-- > 0) continue;
                return frame.AttributeValue switch
                {
                    EventCallback<MouseEventArgs> callback => callback.InvokeAsync((MouseEventArgs)args),
                    EventCallback<ChangeEventArgs> callback => callback.InvokeAsync((ChangeEventArgs)args),
                    EventCallback callback => callback.InvokeAsync(args),
                    MulticastDelegate callback => ((IHandleEvent)this).HandleEventAsync(new EventCallbackWorkItem(callback), args),
                    _ => throw new InvalidOperationException("The rendered trigger has no invocable event callback.")
                };
            }
            throw new InvalidOperationException("The rendered reference trigger has no onclick handler.");
        }

        private void CaptureBrowserReferences(ArrayRange<RenderTreeFrame> frames)
        {
            var context = BrowserContext ?? throw new InvalidOperationException("The current browser reference context was not configured.");
            var ancestors = new Stack<int>();
            for (var index = 0; index < frames.Count; index++)
            {
                while (ancestors.TryPeek(out var ancestor) && index >= ancestor + frames.Array[ancestor].ElementSubtreeLength) ancestors.Pop();
                var frame = frames.Array[index];
                if (frame.FrameType == RenderTreeFrameType.Element) ancestors.Push(index);
                else if (frame.FrameType == RenderTreeFrameType.ElementReferenceCapture)
                {
                    Require(ancestors.Count > 0, "A browser reference capture lost its actual rendered element.");
                    string? elementId = null;
                    for (var attributeIndex = ancestors.Peek() + 1; attributeIndex < frames.Count; attributeIndex++)
                    {
                        var attribute = frames.Array[attributeIndex];
                        if (attribute.FrameType != RenderTreeFrameType.Attribute) break;
                        if (attribute.AttributeName == "id") elementId = attribute.AttributeValue?.ToString();
                    }
                    frame.ElementReferenceCaptureAction(new ElementReference(elementId ?? $"reference-test-element-{index}", context));
                }
            }
        }
#pragma warning restore BL0006
    }
}
