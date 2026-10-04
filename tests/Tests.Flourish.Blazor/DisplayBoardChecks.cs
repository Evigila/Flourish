using System.Diagnostics;
using System.Net;
using System.Text.RegularExpressions;
using ArkheideSystem.Flourish.Blazor;
using ArkheideSystem.Flourish.Blazor.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.RenderTree;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;

internal static class DisplayBoardChecks
{
    private const string Code = "const pi = 'π';\nConsole.WriteLine(\"你好 🚀\");\r\n<script>& value</script>";

    internal static void Register(List<(string Name, Func<Task> Run)> tests)
    {
        tests.Add(("generic display board defaults to a dotted centered surface without copy controls", async () =>
        {
            RenderFragment content = builder => builder.AddContent(0, "Preview <content> 你好");
            await WithBoard(new() { [nameof(DisplayBoard.ChildContent)] = content }, new TrackingJsRuntime(), (_, html) =>
            {
                var rendered = html();
                Require(rendered.Contains("f-display-board-dotted", StringComparison.Ordinal)
                    && rendered.Contains("f-display-board-centered", StringComparison.Ordinal)
                    && !rendered.Contains("f-display-board-start", StringComparison.Ordinal),
                    "Dotted or Centered no longer defaults to true.");
                Require(WebUtility.HtmlDecode(rendered).Contains("Preview <content> 你好", StringComparison.Ordinal)
                    && !rendered.Contains("<content>", StringComparison.Ordinal),
                    "Generic child content was lost or treated as markup.");
                Require(!rendered.Contains("f-display-board-copyable", StringComparison.Ordinal)
                    && !rendered.Contains("f-display-board-copy", StringComparison.Ordinal)
                    && !rendered.Contains("content_copy", StringComparison.Ordinal),
                    "A board without CopyText exposed copy controls.");
                return Task.CompletedTask;
            });
        }));

        tests.Add(("CodeBlock child content preserves exact code and CopyText enables an accessible elevated icon action", async () =>
        {
            RenderFragment content = builder =>
            {
                builder.OpenComponent<CodeBlock>(0);
                builder.AddAttribute(1, nameof(CodeBlock.Text), Code);
                builder.AddAttribute(2, nameof(CodeBlock.Language), "razor");
                builder.CloseComponent();
            };
            await WithBoard(new()
            {
                [nameof(DisplayBoard.ChildContent)] = content,
                [nameof(DisplayBoard.CopyText)] = Code
            }, new TrackingJsRuntime(), (_, html) =>
            {
                var rendered = html();
                var code = Regex.Match(rendered, @"<code>([\s\S]*?)</code>");
                Require(code.Success && WebUtility.HtmlDecode(code.Groups[1].Value) == Code, "CodeBlock changed a newline, Unicode character, or source-code symbol.");
                Require(!rendered.Contains("<script>", StringComparison.Ordinal)
                    && rendered.Contains("&lt;script&gt;&amp; value&lt;/script&gt;", StringComparison.Ordinal),
                    "CodeBlock escaped Razor's HTML encoding boundary.");
                var button = TagWithClass(rendered, "button", "f-display-board-copy");
                Require(button.Contains("f-button-elevated", StringComparison.Ordinal)
                    && button.Contains("f-button-icon", StringComparison.Ordinal)
                    && Attribute(button, "aria-label") == "Copy",
                    "CopyText did not expose an Elevated icon-only action with an accessible name.");
                Require(rendered.Contains("data-icon=\"content_copy\"", StringComparison.Ordinal), "The initial copy glyph is not content_copy.");
                return Task.CompletedTask;
            });
        }));

        tests.Add(("copy callback sends the exact full text, shows check feedback, and resets after 1600 ms", async () =>
        {
            var js = new TrackingJsRuntime();
            await WithBoard(new() { [nameof(DisplayBoard.CopyText)] = Code }, js, async (board, html) =>
            {
                await board.ClickCopyAsync();
                Require(js.ImportCount == 1 && js.ImportPath == "./_content/Arkheide.Flourish.Blazor.Framework/clipboard.js", "Copy did not lazily import the framework clipboard module.");
                Require(js.Module.CopyCalls == 1 && js.Module.LastText == Code, "Copy truncated or changed the supplied multiline Unicode text.");
                Require(html().Contains("data-icon=\"check\"", StringComparison.Ordinal)
                    && html().Contains("aria-label=\"Copied\"", StringComparison.Ordinal)
                    && html().Contains("role=\"status\">Copied", StringComparison.Ordinal),
                    "Successful copy did not expose check feedback and its status text.");
                await Task.Delay(1700);
                Require(html().Contains("data-icon=\"content_copy\"", StringComparison.Ordinal)
                    && html().Contains("aria-label=\"Copy\"", StringComparison.Ordinal)
                    && !html().Contains("role=\"status\">Copied", StringComparison.Ordinal),
                    "Copy feedback did not reset after its 1600 ms interval.");
            });
            Require(js.Module.Disposed, "Disposing the rendered board did not release its imported module.");
        }));

        tests.Add(("changing CopyText cancels stale feedback and disposal cancels the remaining timer", async () =>
        {
            var js = new TrackingJsRuntime();
            var disposal = Stopwatch.StartNew();
            await WithBoard(new() { [nameof(DisplayBoard.CopyText)] = "first" }, js, async (board, html) =>
            {
                await board.ClickCopyAsync();
                await Task.Delay(800);
                await board.SetCopyTextAsync("second\n你好");
                Require(html().Contains("data-icon=\"content_copy\"", StringComparison.Ordinal), "Changing CopyText retained feedback for obsolete content.");
                await board.ClickCopyAsync();
                await Task.Delay(900);
                Require(js.Module.LastText == "second\n你好" && html().Contains("data-icon=\"check\"", StringComparison.Ordinal),
                    "The canceled first timer reset feedback for the newer copied text.");
                await board.ClickCopyAsync();
                disposal.Restart();
            });
            disposal.Stop();
            Require(js.Module.Disposed, "Disposal did not release the clipboard module after canceling feedback.");
            Require(disposal.Elapsed < TimeSpan.FromMilliseconds(1000), "Disposal waited for the remaining feedback delay instead of canceling it.");
        }));
    }

    private static async Task WithBoard(
        Dictionary<string, object?> parameters,
        TrackingJsRuntime js,
        Func<DisplayBoardProbe, Func<string>, Task> verify)
    {
        var activator = new CapturingActivator();
        var services = new ServiceCollection();
        services.AddLogging();
        services.AddSingleton<NavigationManager>(new TestNavigation());
        services.AddSingleton<IJSRuntime>(js);
        services.AddSingleton<IComponentActivator>(activator);
        services.AddFlourishFramework();
        using var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();
        await using var renderer = new HtmlRenderer(scope.ServiceProvider, provider.GetRequiredService<ILoggerFactory>());
        await renderer.Dispatcher.InvokeAsync(async () =>
        {
            var output = await renderer.RenderComponentAsync<DisplayBoardProbe>(ParameterView.FromDictionary(parameters));
            await verify(activator.Board ?? throw new InvalidOperationException("The renderer did not construct the display board."), output.ToHtmlString);
        });
    }

    private static string TagWithClass(string html, string name, string className)
    {
        foreach (Match match in Regex.Matches(html, $@"<{name}\b[^>]*>"))
        {
            if (Attribute(match.Value, "class").Split(' ', StringSplitOptions.RemoveEmptyEntries).Contains(className)) return match.Value;
        }
        throw new InvalidOperationException($"Missing {className} element.");
    }

    private static string Attribute(string tag, string name)
    {
        var match = Regex.Match(tag, $"\\b{Regex.Escape(name)}=\"([^\"]*)\"");
        return match.Success ? WebUtility.HtmlDecode(match.Groups[1].Value) : string.Empty;
    }

    private static void Require(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }

    private sealed class CapturingActivator : IComponentActivator
    {
        internal DisplayBoardProbe? Board { get; private set; }
        public IComponent CreateInstance(Type componentType)
        {
            var component = (IComponent)Activator.CreateInstance(componentType)!;
            if (component is DisplayBoardProbe board) Board = board;
            return component;
        }
    }

    private sealed class DisplayBoardProbe : DisplayBoard
    {
        public DisplayBoardProbe() { }

#pragma warning disable BL0006
        internal Task ClickCopyAsync()
        {
            using var builder = new RenderTreeBuilder();
            base.BuildRenderTree(builder);
            var frames = builder.GetFrames();
            for (var index = 0; index < frames.Count; index++)
            {
                var frame = frames.Array[index];
                if (frame.FrameType != RenderTreeFrameType.Component || frame.ComponentType != typeof(Button)) continue;
                var end = index + frame.ComponentSubtreeLength;
                for (var attributeIndex = index + 1; attributeIndex < end; attributeIndex++)
                {
                    var attribute = frames.Array[attributeIndex];
                    if (attribute.FrameType != RenderTreeFrameType.Attribute || attribute.AttributeName != nameof(Button.OnClick)) continue;
                    var args = new MouseEventArgs();
                    return attribute.AttributeValue switch
                    {
                        EventCallback<MouseEventArgs> callback => callback.InvokeAsync(args),
                        EventCallback callback => callback.InvokeAsync(args),
                        MulticastDelegate callback => ((IHandleEvent)this).HandleEventAsync(new EventCallbackWorkItem(callback), args),
                        _ => throw new InvalidOperationException("The copy action has no invocable callback.")
                    };
                }
            }
            throw new InvalidOperationException("The copy action was not rendered.");
        }

        internal Task SetCopyTextAsync(string? text)
        {
            CopyText = text;
            base.OnParametersSet();
            return InvokeAsync(StateHasChanged);
        }
#pragma warning restore BL0006
    }

    private sealed class TrackingJsRuntime : IJSRuntime
    {
        internal TrackingJsModule Module { get; } = new();
        internal int ImportCount { get; private set; }
        internal string? ImportPath { get; private set; }

        public ValueTask<TValue> InvokeAsync<TValue>(string identifier, object?[]? args) => InvokeAsync<TValue>(identifier, default, args);
        public ValueTask<TValue> InvokeAsync<TValue>(string identifier, CancellationToken cancellationToken, object?[]? args)
        {
            if (identifier != "import") throw new InvalidOperationException($"Unexpected JS call: {identifier}.");
            ImportCount++;
            ImportPath = args?.SingleOrDefault()?.ToString();
            return ValueTask.FromResult((TValue)(object)Module);
        }
    }

    private sealed class TrackingJsModule : IJSObjectReference
    {
        internal int CopyCalls { get; private set; }
        internal string? LastText { get; private set; }
        internal bool Disposed { get; private set; }

        public ValueTask<TValue> InvokeAsync<TValue>(string identifier, object?[]? args) => InvokeAsync<TValue>(identifier, default, args);
        public ValueTask<TValue> InvokeAsync<TValue>(string identifier, CancellationToken cancellationToken, object?[]? args)
        {
            if (identifier != "copyText") throw new InvalidOperationException($"Unexpected module call: {identifier}.");
            CopyCalls++;
            LastText = args?.SingleOrDefault()?.ToString();
            return ValueTask.FromResult((TValue)(object)true);
        }

        public ValueTask DisposeAsync()
        {
            Disposed = true;
            return ValueTask.CompletedTask;
        }
    }
}
