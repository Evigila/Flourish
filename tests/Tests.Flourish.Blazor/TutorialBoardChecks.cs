using ArkheideSystem.Flourish.Blazor;
using ArkheideSystem.Flourish.Blazor.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.RenderTree;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;

internal static class TutorialBoardChecks
{
    private static readonly TutorialStep[] Steps =
    [new("company", "Company <one>", "Add the company.", "Register", Completed: true),
     new("products", "Define products", "Shared definitions.", "Define"),
     new("stock", "Record stock", "Select an establishment.", "Record")];

    internal static void Register(List<(string Name, Func<Task> Run)> tests)
    {
        tests.Add(("tutorial has one Abstract state contract and a nonmodal primary dotted production entry", async () =>
        {
            Require(typeof(TutorialStep).Assembly != typeof(TutorialBoard).Assembly, "State depends on a renderer.");
            Require(ComponentUsageCatalog.For(typeof(TutorialBoard)).Kind == ComponentUseKind.Scenario, "Tutorial is not a production scenario.");
            await WithBoard(new(), async (_, html, _) =>
            {
                var value = html();
                Require(value.Contains("f-tutorial-primary f-tutorial-dotted", StringComparison.Ordinal)
                    && value.Contains("popover=\"auto\" role=\"dialog\" aria-modal=\"false\"", StringComparison.Ordinal), "The board is modal or lost default roles.");
                Require(value.Contains("<h1", StringComparison.Ordinal) && value.Contains(">Define products</h1>", StringComparison.Ordinal), "The initial step is not the first incomplete H1.");
                Require(value.Contains("Company &lt;one&gt;", StringComparison.Ordinal) && !value.Contains("Company <one>", StringComparison.Ordinal), "Tutorial data bypassed HTML encoding.");
                Require(value.Contains("1 of 3 completed", StringComparison.Ordinal) && value.Contains("role=\"tooltip\"", StringComparison.Ordinal), "Passive progress overview is missing.");
                Require(!value.Contains("<dialog", StringComparison.Ordinal) && !value.Contains("role=\"menu\"", StringComparison.Ordinal), "Tutorial disguises a modal or command menu.");
                await Task.CompletedTask;
            });
        }));
        tests.Add(("empty tutorial remains closed and its compact entry is disabled without an invalid percentage", async () =>
        {
            await WithBoard(new() { [nameof(TutorialBoard.Steps)] = Array.Empty<TutorialStep>() }, async (board, html, _) =>
            {
                Require(html().Contains("aria-valuenow=\"0\"", StringComparison.Ordinal)
                    && !html().Contains("NaN", StringComparison.Ordinal) && !html().Contains("<h1", StringComparison.Ordinal), "Empty tutorial creates invalid state.");
                Require(board.NativeDisabled("f-tutorial-trigger"), "Empty tutorial entry remains clickable.");
                await board.ClickNativeAsync("f-tutorial-trigger");
                Require(!board.IsOpen, "An empty tutorial opened.");
            });
        }));
        tests.Add(("tutorial trigger and close publish actual bound open state without completion or skip", async () =>
        {
            var state = new List<bool>(); var skips = 0;
            await WithBoard(new()
            {
                [nameof(TutorialBoard.IsOpenChanged)] = EventCallback.Factory.Create<bool>(new object(), (bool value) => state.Add(value)),
                [nameof(TutorialBoard.OnSkip)] = EventCallback.Factory.Create(new object(), () => skips++)
            }, async (board, html, _) =>
            {
                await board.ClickNativeAsync("f-tutorial-trigger"); await board.ClickNativeAsync("f-tutorial-trigger");
                Require(board.IsOpen && state.SequenceEqual([true]), "Opening duplicated a state event.");
                Require(html().Contains("aria-expanded=\"true\"", StringComparison.Ordinal), "Trigger does not announce open state.");
                await board.ClickButtonAsync(icon: "close"); await board.RequestCloseAsync();
                Require(!board.IsOpen && state.SequenceEqual([true, false]) && skips == 0, "Closing performs skip or duplicates an event.");
                Require(Steps.Count(step => step.Completed) == 1, "Closing mutated host completion.");
            });
        }));
        tests.Add(("actual segmented navigation only selects a milestone and preserves verified completion", async () =>
        {
            string? selected = null;
            await WithBoard(new() { [nameof(TutorialBoard.ActiveStepKeyChanged)] = EventCallback.Factory.Create<string>(new object(), (string key) => selected = key) },
                async (board, html, _) =>
                {
                    await board.ClickNativeAsync("f-tutorial-segment", 2);
                    Require(selected == "stock" && board.ActiveStepKey == "stock" && html().Contains(">Record stock</h1>", StringComparison.Ordinal), "Segment did not invoke the actual selection event.");
                    Require(html().Contains("1 of 3 completed", StringComparison.Ordinal), "Selecting a milestone falsely completed it.");
                    await board.UpdateStepsAsync(Steps.Select(step => step with { Completed = true }).ToArray());
                    Require(html().Contains("3 of 3 completed", StringComparison.Ordinal) && html().Contains("aria-valuenow=\"100\"", StringComparison.Ordinal), "Host-verified state did not update both projections.");
                });
        }));
        tests.Add(("tutorial action prevents concurrent operations and receives the actual selected milestone", async () =>
        {
            var release = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously); var calls = 0; TutorialStep? received = null;
            await WithBoard(new() { [nameof(TutorialBoard.OnAction)] = EventCallback.Factory.Create<TutorialStep>(new object(), async (TutorialStep step) => { calls++; received = step; await release.Task; }) },
                async (board, html, _) =>
                {
                    var first = board.ClickButtonAsync(text: "Define");
                    Require(calls == 1 && received?.Key == "products", "Action did not receive the selected milestone.");
                    await board.ClickButtonAsync(text: "Define"); await board.ClickNativeAsync("f-tutorial-segment", 2);
                    Require(calls == 1 && board.ActiveStepKey is null, "An in-flight operation permitted a duplicate action or selection.");
                    release.SetResult(); await first;
                    Require(html().Contains("1 of 3 completed", StringComparison.Ordinal), "Action inferred completion.");
                });
        }));
        tests.Add(("tutorial navigation never also dispatches a host side effect", async () =>
        {
            var calls = 0;
            await WithBoard(new()
            {
                [nameof(TutorialBoard.Steps)] = new[] { new TutorialStep("link", "A linked step", "Navigate.", "Open", ActionHref: "/target") },
                [nameof(TutorialBoard.OnAction)] = EventCallback.Factory.Create<TutorialStep>(new object(), (TutorialStep _) => calls++)
            }, async (board, html, _) =>
            {
                Require(html().Contains("href=\"/target\"", StringComparison.Ordinal), "Shortcut lost standard Button navigation.");
                await board.ClickButtonAsync(text: "Open"); Require(calls == 0, "Navigation also invoked OnAction.");
            });
        }));
        tests.Add(("tutorial skip is only a host request and busy or disabled steps prevent shortcuts", async () =>
        {
            var skips = 0; var actions = 0;
            await WithBoard(new()
            {
                [nameof(TutorialBoard.IsOpen)] = true,
                [nameof(TutorialBoard.Steps)] = new[] { new TutorialStep("disabled", "Locked step", "Wait.", "Act", Disabled: true) },
                [nameof(TutorialBoard.OnAction)] = EventCallback.Factory.Create<TutorialStep>(new object(), (TutorialStep _) => actions++),
                [nameof(TutorialBoard.OnSkip)] = EventCallback.Factory.Create(new object(), () => skips++)
            }, async (board, html, _) =>
            {
                await board.ClickButtonAsync(text: "Act"); Require(actions == 0, "Disabled milestone dispatched a shortcut.");
                await board.ClickButtonAsync(text: "Skip tutorial");
                Require(skips == 1 && board.IsOpen && html().Contains("0 of 1 completed", StringComparison.Ordinal), "Skip pretended to persist, close, or complete the tutorial.");
                await board.SetBusyAsync(true); await board.ClickButtonAsync(text: "Skip tutorial");
                Require(skips == 1, "Busy tutorial dispatched another skip.");
            });
        }));
        tests.Add(("tutorial rejects duplicate keys, unknown selections, invalid tone and an open empty board", () =>
        {
            var probe = new BoardProbe();
            Reject<ArgumentException>(() => probe.Validate([Steps[0], Steps[0]]));
            Reject<ArgumentException>(() => probe.Validate([Steps[0]], "unknown"));
            Reject<ArgumentException>(() => probe.Validate([Steps[0] with { Key = " " }]));
            Reject<ArgumentOutOfRangeException>(() => probe.Validate(Steps, tone: (PresentationTone)999));
            Reject<InvalidOperationException>(() => probe.Validate([], open: true));
            return Task.CompletedTask;
        }));
        tests.Add(("tutorial uses the shared controls asset and detaches every browser ownership on disposal", async () =>
        {
            var js = new JsProbe();
            await WithBoard(new(), async (board, _, activeJs) =>
            {
                Require(activeJs.Imports == 0, "Static render eagerly started browser interop.");
                await board.AttachAsync(); await board.AttachAsync();
                Require(activeJs.Imports == 1 && activeJs.Path == "./_content/Arkheide.Flourish.Blazor.Framework/controls.js"
                    && activeJs.Module.Calls.Count(name => name == "synchronizeTutorialBoard") == 2, "Tutorial installed an independent browser asset or imported twice.");
            }, js);
            Require(js.Module.Calls.Count(name => name == "detachTutorialBoard") == 1 && js.Module.Disposed, "Disposal retained the browser controller.");
        }));
        tests.Add(("compact ProgressRing hides only its visible number while retaining determinate semantics", async () =>
        {
            using var provider = Services(new Capture(), new JsProbe());
            await using var renderer = new HtmlRenderer(provider, provider.GetRequiredService<ILoggerFactory>());
            await renderer.Dispatcher.InvokeAsync(async () =>
            {
                var output = await renderer.RenderComponentAsync<ProgressRing>(ParameterView.FromDictionary(new Dictionary<string, object?>
                    { [nameof(ProgressRing.Value)] = 50d, [nameof(ProgressRing.Compact)] = true, [nameof(ProgressRing.ShowValue)] = false }));
                var html = output.ToHtmlString();
                Require(html.Contains("f-progress-ring-compact", StringComparison.Ordinal) && html.Contains("aria-valuenow=\"50\"", StringComparison.Ordinal)
                    && html.Contains("stroke-dasharray=\"50 100\"", StringComparison.Ordinal) && !html.Contains("50%", StringComparison.Ordinal), "Compact variant changed the shared progress contract.");
            });
        }));
    }

    private static async Task WithBoard(Dictionary<string, object?> parameters, Func<BoardProbe, Func<string>, JsProbe, Task> verify, JsProbe? runtime = null)
    {
        runtime ??= new JsProbe(); var capture = new Capture();
        parameters.TryAdd(nameof(TutorialBoard.Steps), Steps);
        using var provider = Services(capture, runtime);
        await using var renderer = new HtmlRenderer(provider, provider.GetRequiredService<ILoggerFactory>());
        await renderer.Dispatcher.InvokeAsync(async () =>
        {
            var output = await renderer.RenderComponentAsync<BoardProbe>(ParameterView.FromDictionary(parameters));
            await verify(capture.Board!, output.ToHtmlString, runtime);
        });
    }
    private static ServiceProvider Services(Capture capture, JsProbe js)
    {
        var services = new ServiceCollection(); services.AddLogging(); services.AddFlourishFramework();
        services.AddSingleton<NavigationManager>(new NavigationProbe()); services.AddSingleton<IJSRuntime>(js); services.AddSingleton<IComponentActivator>(capture);
        return services.BuildServiceProvider();
    }
    private static void Require(bool condition, string message) { if (!condition) throw new InvalidOperationException(message); }
    private static void Reject<T>(Action action) where T : Exception { try { action(); } catch (T) { return; } throw new InvalidOperationException($"Expected {typeof(T).Name}."); }
    private sealed class NavigationProbe : NavigationManager { public NavigationProbe() => Initialize("https://example.test/", "https://example.test/"); protected override void NavigateToCore(string uri, bool forceLoad) { } }
    private sealed class Capture : IComponentActivator
    {
        internal BoardProbe? Board;
        public IComponent CreateInstance(Type type) { var component = (IComponent)Activator.CreateInstance(type)!; if (component is BoardProbe board) Board = board; return component; }
    }
    private sealed class BoardProbe : TutorialBoard
    {
        public BoardProbe() { }
#pragma warning disable BL0005, BL0006
        internal Task AttachAsync() => base.OnAfterRenderAsync(false);
        internal Task UpdateStepsAsync(IReadOnlyList<TutorialStep> steps) { Steps = steps; base.OnParametersSet(); return InvokeAsync(StateHasChanged); }
        internal Task SetBusyAsync(bool busy) { Busy = busy; return InvokeAsync(StateHasChanged); }
        internal void Validate(IReadOnlyList<TutorialStep> steps, string? active = null, bool open = false, PresentationTone tone = PresentationTone.Primary)
        { Steps = steps; ActiveStepKey = active; IsOpen = open; Tone = tone; base.OnParametersSet(); }
        internal bool NativeDisabled(string cssClass) => NativeAttributes(cssClass, 0).Any(frame => frame.AttributeName == "disabled" && frame.AttributeValue is true);
        internal Task ClickNativeAsync(string cssClass, int occurrence = 0) => Invoke(NativeAttributes(cssClass, occurrence).Single(frame => frame.AttributeName == "onclick").AttributeValue);
        private RenderTreeFrame[] NativeAttributes(string cssClass, int occurrence)
        {
            using var builder = new RenderTreeBuilder(); base.BuildRenderTree(builder); var frames = builder.GetFrames();
            for (var index = 0; index < frames.Count; index++)
            {
                if (frames.Array[index].FrameType != RenderTreeFrameType.Element || frames.Array[index].ElementName != "button") continue;
                var attributes = frames.Array.Skip(index + 1).TakeWhile(frame => frame.FrameType == RenderTreeFrameType.Attribute).ToArray();
                if (!attributes.Any(frame => frame.AttributeName == "class" && frame.AttributeValue?.ToString()?.Split(' ').Contains(cssClass) == true)) continue;
                if (occurrence-- == 0) return attributes;
            }
            throw new InvalidOperationException("Missing native tutorial button: " + cssClass);
        }
        internal Task ClickButtonAsync(string? text = null, string? icon = null)
        {
            using var builder = new RenderTreeBuilder(); base.BuildRenderTree(builder); var frames = builder.GetFrames();
            for (var index = 0; index < frames.Count; index++)
            {
                if (frames.Array[index].FrameType != RenderTreeFrameType.Component || frames.Array[index].ComponentType != typeof(Button)) continue;
                var attributes = frames.Array.Skip(index + 1).TakeWhile(frame => frame.FrameType == RenderTreeFrameType.Attribute).ToArray();
                if (text is not null && !attributes.Any(frame => frame.AttributeName == nameof(Button.Text) && frame.AttributeValue?.ToString() == text)) continue;
                if (icon is not null && !attributes.Any(frame => frame.AttributeName == nameof(Button.Icon) && frame.AttributeValue?.ToString() == icon)) continue;
                return Invoke(attributes.Single(frame => frame.AttributeName == nameof(Button.OnClick)).AttributeValue);
            }
            throw new InvalidOperationException("Missing standard tutorial action.");
        }
        private Task Invoke(object? callback) => callback switch
        {
            EventCallback<MouseEventArgs> typed => typed.InvokeAsync(new MouseEventArgs()), EventCallback untyped => untyped.InvokeAsync(new MouseEventArgs()),
            MulticastDelegate action => ((IHandleEvent)this).HandleEventAsync(new EventCallbackWorkItem(action), new MouseEventArgs()),
            _ => throw new InvalidOperationException("No invocable rendered callback.")
        };
#pragma warning restore BL0005, BL0006
    }
    private sealed class JsProbe : IJSRuntime
    {
        internal int Imports; internal string? Path; internal ModuleProbe Module = new();
        public ValueTask<T> InvokeAsync<T>(string name, object?[]? args) => InvokeAsync<T>(name, default, args);
        public ValueTask<T> InvokeAsync<T>(string name, CancellationToken token, object?[]? args)
        { Require(name == "import", "Unexpected JS call."); Imports++; Path = args![0]?.ToString(); return ValueTask.FromResult((T)(object)Module); }
    }
    private sealed class ModuleProbe : IJSObjectReference
    {
        internal List<string> Calls = []; internal bool Disposed;
        public ValueTask<T> InvokeAsync<T>(string name, object?[]? args) => InvokeAsync<T>(name, default, args);
        public ValueTask<T> InvokeAsync<T>(string name, CancellationToken token, object?[]? args) { Calls.Add(name); return ValueTask.FromResult(default(T)!); }
        public ValueTask DisposeAsync() { Disposed = true; return ValueTask.CompletedTask; }
    }
}
