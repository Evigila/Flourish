using System.Net;
using System.Reflection;
using System.Text.RegularExpressions;
using ArkheideSystem.Flourish.Blazor;
using ArkheideSystem.Flourish.Blazor.Abstract;
using ArkheideSystem.Flourish.Blazor.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.Web.HtmlRendering;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.RenderTree;
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
        RegisterAsyncExecution(tests);
    }

    private static void RegisterAsyncExecution(List<(string Name, Func<Task> Run)> tests)
    {
        foreach (var link in new[] { false, true })
        {
            tests.Add(($"asynchronous {(link ? "link" : "button")} renders busy until completion and rejects repeated activation", async () =>
            {
                var completion = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
                var calls = 0;
                var callback = EventCallback.Factory.Create<MouseEventArgs>(new object(), async _ => { calls++; await completion.Task; });
                await WithInteractiveButton(link, callback, async (button, renderer, output) =>
                {
                    RequireExecutionState(await renderer.Dispatcher.InvokeAsync(output.ToHtmlString), link, false);
                    Task? pending = null;
                    try
                    {
                        await renderer.Dispatcher.InvokeAsync(() => { pending = button.ClickRenderedAsync(); });
                        Require(pending is { IsCompleted: false } && calls == 1, "The asynchronous callback was not awaited exactly once.");
                        RequireExecutionState(await renderer.Dispatcher.InvokeAsync(output.ToHtmlString), link, true);
                        Require(!button.Busy, "Automatic execution mutated the host's Busy parameter.");
                        await renderer.Dispatcher.InvokeAsync(button.ClickRenderedAsync);
                        Require(calls == 1, "An in-flight callback accepted repeated activation.");
                    }
                    finally
                    {
                        completion.TrySetResult();
                        if (pending is not null) await pending;
                    }
                    RequireExecutionState(await renderer.Dispatcher.InvokeAsync(output.ToHtmlString), link, false);
                    await renderer.Dispatcher.InvokeAsync(button.ClickRenderedAsync);
                    Require(calls == 2, "A completed callback did not allow a later activation.");
                });
            }));
            tests.Add(($"asynchronous {(link ? "link" : "button")} releases rendered busy and propagates callback failure", async () =>
            {
                var completion = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
                var failure = new InvalidOperationException("Expected action failure");
                var calls = 0;
                var callback = EventCallback.Factory.Create<MouseEventArgs>(new object(), async _ => { calls++; await completion.Task; throw failure; });
                await WithInteractiveButton(link, callback, async (button, renderer, output) =>
                {
                    Task? pending = null;
                    try
                    {
                        await renderer.Dispatcher.InvokeAsync(() => { pending = button.ClickRenderedAsync(); });
                        RequireExecutionState(await renderer.Dispatcher.InvokeAsync(output.ToHtmlString), link, true);
                        await renderer.Dispatcher.InvokeAsync(button.ClickRenderedAsync);
                        Require(calls == 1, "A failing in-flight action was invoked more than once.");
                    }
                    finally { completion.TrySetResult(); }
                    Exception? observed = null;
                    try { await pending!; }
                    catch (Exception error) { observed = error; }
                    Require(ReferenceEquals(observed, failure), "The callback failure was swallowed or replaced.");
                    RequireExecutionState(await renderer.Dispatcher.InvokeAsync(output.ToHtmlString), link, false);
                });
            }));
            tests.Add(($"external Busy keeps {(link ? "link" : "button")} unavailable after its asynchronous execution ends", async () =>
            {
                var completion = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
                var calls = 0;
                var callback = EventCallback.Factory.Create<MouseEventArgs>(new object(), async _ => { calls++; await completion.Task; });
                await WithInteractiveButton(link, callback, async (button, renderer, output) =>
                {
                    Task? pending = null;
                    try
                    {
                        await renderer.Dispatcher.InvokeAsync(() => { pending = button.ClickRenderedAsync(); });
                        await renderer.Dispatcher.InvokeAsync(() => button.SetParametersAsync(ParameterView.FromDictionary(new Dictionary<string, object?> { [nameof(Button.Busy)] = true, [nameof(Button.BusyLabel)] = "Preparing contact" })));
                    }
                    finally
                    {
                        completion.TrySetResult();
                        if (pending is not null) await pending;
                    }
                    RequireExecutionState(await renderer.Dispatcher.InvokeAsync(output.ToHtmlString), link, true);
                    await renderer.Dispatcher.InvokeAsync(button.ClickRenderedAsync);
                    Require(button.Busy && calls == 1, "Automatic completion cleared or ignored externally controlled Busy.");
                    await renderer.Dispatcher.InvokeAsync(() => button.SetParametersAsync(ParameterView.FromDictionary(new Dictionary<string, object?> { [nameof(Button.Busy)] = false, [nameof(Button.BusyLabel)] = "Preparing contact" })));
                    RequireExecutionState(await renderer.Dispatcher.InvokeAsync(output.ToHtmlString), link, false);
                });
            }));
        }
        tests.Add(("navigation without a callback remains available without automatic busy presentation", async () =>
        {
            await WithInteractiveButton(true, default, async (button, renderer, output) =>
            {
                RequireExecutionState(await renderer.Dispatcher.InvokeAsync(output.ToHtmlString), true, false);
                await renderer.Dispatcher.InvokeAsync(button.ClickRenderedAsync);
                RequireExecutionState(await renderer.Dispatcher.InvokeAsync(output.ToHtmlString), true, false);
                await renderer.Dispatcher.InvokeAsync(() => button.SetParametersAsync(ParameterView.FromDictionary(new Dictionary<string, object?> { [nameof(Button.Href)] = "#details" })));
                var tag = OpeningTag(await renderer.Dispatcher.InvokeAsync(output.ToHtmlString), "a");
                Require(Attribute(tag, "href") == "https://test.example/records#details", "Callback-free navigation changed fragment-link resolution.");
            });
        }));
        tests.Add(("synchronous callback failure releases busy and preserves its original exception", async () =>
        {
            var failure = new InvalidOperationException("Expected synchronous action failure");
            var callback = EventCallback.Factory.Create<MouseEventArgs>(new object(), (Action<MouseEventArgs>)(_ => throw failure));
            await WithInteractiveButton(false, callback, async (button, renderer, output) =>
            {
                Exception? observed = null;
                try { await renderer.Dispatcher.InvokeAsync(button.ClickRenderedAsync); }
                catch (Exception error) { observed = error; }
                Require(ReferenceEquals(observed, failure), "The synchronous callback failure was swallowed or replaced.");
                RequireExecutionState(await renderer.Dispatcher.InvokeAsync(output.ToHtmlString), false, false);
            });
        }));
    }

    private static async Task WithInteractiveButton(bool link, EventCallback<MouseEventArgs> callback, Func<ButtonProbe, HtmlRenderer, HtmlRootComponent, Task> exercise)
    {
        var capture = new ButtonActivator();
        var services = new ServiceCollection().AddLogging().AddFlourishFramework();
        services.AddSingleton<NavigationManager>(new TestNavigation());
        services.AddSingleton<IComponentActivator>(capture);
        using var provider = services.BuildServiceProvider();
        await using var renderer = new HtmlRenderer(provider, provider.GetRequiredService<ILoggerFactory>());
        var parameters = new Dictionary<string, object?>
        {
            [nameof(Button.Text)] = "Contact support", [nameof(Button.Type)] = "submit",
            [nameof(Button.BusyLabel)] = "Preparing contact", [nameof(Button.OnClick)] = callback
        };
        if (link) parameters[nameof(Button.Href)] = "/support";
        var output = await renderer.Dispatcher.InvokeAsync(() => renderer.RenderComponentAsync<ButtonProbe>(ParameterView.FromDictionary(parameters)));
        var button = capture.Button as ButtonProbe ?? throw new InvalidOperationException("Interactive button was not instantiated.");
        await exercise(button, renderer, output);
    }

    private static void RequireExecutionState(string html, bool link, bool busy)
    {
        var tag = OpeningTag(html, link ? "a" : "button");
        Require(Attribute(tag, "aria-busy") == (busy ? "true" : "false"), "The real rendered action did not announce its current execution state.");
        Require(html.Contains("f-spinner", StringComparison.Ordinal) == busy && html.Contains("Preparing contact", StringComparison.Ordinal) == busy,
            "Execution state disagrees with the rendered spinner or accessible status label.");
        if (link)
        {
            Require(Attribute(tag, "href") == (busy ? null : "/support") && Attribute(tag, "aria-disabled") == (busy ? "true" : "false"),
                "Execution state changed the navigation destination or unavailable link semantics.");
            Require(Attribute(tag, "tabindex") == (busy ? "-1" : null), "An executing link remained in the Tab sequence.");
        }
        else Require(Attribute(tag, "type") == "submit" && Regex.IsMatch(tag, @"\sdisabled(?:[\s=>])") == busy,
            "Execution state changed native submit type or disabled semantics.");
    }

    private sealed class ButtonProbe : Button
    {
        public ButtonProbe() { }
#pragma warning disable BL0006
        internal Task ClickRenderedAsync()
        {
            using var body = new RenderTreeBuilder();
            base.BuildRenderTree(body);
            var frames = body.GetFrames();
            for (var index = 0; index < frames.Count; index++)
            {
                var frame = frames.Array[index];
                if (frame.FrameType != RenderTreeFrameType.Attribute || frame.AttributeName != "onclick") continue;
                var args = new MouseEventArgs();
                return frame.AttributeValue switch
                {
                    EventCallback<MouseEventArgs> callback => callback.InvokeAsync(args),
                    EventCallback callback => callback.InvokeAsync(args),
                    MulticastDelegate callback => ((IHandleEvent)this).HandleEventAsync(new EventCallbackWorkItem(callback), args),
                    _ => throw new InvalidOperationException("The rendered button has no invocable click callback.")
                };
            }
            throw new InvalidOperationException("The rendered button has no onclick handler.");
        }
#pragma warning restore BL0006
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
