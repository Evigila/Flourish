using System.Reflection;
using ArkheideSystem.Flourish.Blazor;
using ArkheideSystem.Flourish.Blazor.Components;
using ArkheideSystem.Flourish.Blazor.Hosting;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;

internal static class DialogResultChecks
{
    internal static void Register(List<(string Name, Func<Task> Run)> tests)
    {
        tests.Add(("form grouping preserves native disable boundaries and production image and footer content", async () =>
        {
            using var services = Services(); using var scope = services.CreateScope();
            await using var renderer = new HtmlRenderer(scope.ServiceProvider, scope.ServiceProvider.GetRequiredService<ILoggerFactory>());
            var form = await renderer.Dispatcher.InvokeAsync(async () => (await renderer.RenderComponentAsync<FormGroup>(ParameterView.FromDictionary(new Dictionary<string, object?>
            {
                [nameof(FormGroup.Disabled)] = true, [nameof(FormGroup.Title)] = "Group <title>",
                [nameof(FormGroup.ChildContent)] = (RenderFragment)(builder => builder.AddContent(0, "Native business fields"))
            }))).ToHtmlString());
            Require(form.Contains("<fieldset disabled", StringComparison.Ordinal) && form.Contains("<legend>Group &lt;title&gt;</legend>", StringComparison.Ordinal)
                && form.Contains("f-form-layout", StringComparison.Ordinal), "Grouping changed its disable protocol or cloned the field layout.");
            var image = await renderer.Dispatcher.InvokeAsync(async () => (await renderer.RenderComponentAsync<ImagePreview>(ParameterView.FromDictionary(new Dictionary<string, object?>
            { [nameof(ImagePreview.Url)] = "/business-image", [nameof(ImagePreview.Alt)] = "Image <label>" }))).ToHtmlString());
            Require(image.Contains("f-image-preview", StringComparison.Ordinal) && image.Contains("alt=\"Image &lt;label&gt;\"", StringComparison.Ordinal), "Image preview lost encoded semantics.");
            var footer = await renderer.Dispatcher.InvokeAsync(async () => (await renderer.RenderComponentAsync<AttributionFooter>(ParameterView.FromDictionary(new Dictionary<string, object?>
            { [nameof(AttributionFooter.Text)] = "ARKHEIDE SYSTEM <attribution>" }))).ToHtmlString());
            Require(footer.Contains("f-attribution-footer", StringComparison.Ordinal) && footer.Contains("ARKHEIDE SYSTEM &lt;attribution&gt;", StringComparison.Ordinal), "Attribution changed business ownership or text encoding.");
        }));
        tests.Add(("a Dialog awaits the actual standard button result and encodes host content", async () =>
        {
            await using var f = new DialogFixture(); Dialog? dialog = null; Task<object?>? result = null;
            var output = await f.Render<Dialog>(new()
            {
                ["Title"] = "Confirm <action>", ["ChildContent"] = (RenderFragment)(builder => builder.AddContent(0, "Discard <unsafe> & changes?")),
                ["Actions"] = (RenderFragment)(builder =>
                {
                    builder.OpenComponent<Button>(0); builder.AddAttribute(1, "Text", "Cancel");
                    builder.AddAttribute(2, "OnClick", EventCallback.Factory.Create<MouseEventArgs>(f, () => dialog!.CloseAsync())); builder.CloseComponent();
                    builder.OpenComponent<Button>(3); builder.AddAttribute(4, "Text", "Accept");
                    builder.AddAttribute(5, "OnClick", EventCallback.Factory.Create<MouseEventArgs>(f, () => dialog!.CloseAsync(true))); builder.CloseComponent();
                })
            });
            dialog = f.Component<Dialog>();
            await f.Dispatch(() => { result = dialog.ShowAsync(); return Task.CompletedTask; });
            var html = await f.Read(output);
            Require(dialog.HasPendingResult && !result!.IsCompleted, "The generic dialog did not await a user decision.");
            Require(html.Contains("f-dialog") && html.Contains("f-button") && !html.Contains("f-uniform-grid"), "Confirmation did not compose generic Dialog and Button.");
            Require(html.Contains("Discard &lt;unsafe&gt; &amp; changes?") && !html.Contains("<unsafe>"), "Host text became markup.");
            await f.Dispatch(() => ClickAsync(f.Components.OfType<Button>().Single(button => button.Text == "Accept")));
            Require(Equals(await result!.WaitAsync(TimeSpan.FromSeconds(3)), true) && !dialog.HasPendingResult, "The actual accept button lost its result.");
            await f.Dispatch(() => { result = dialog.ShowAsync(); return Task.CompletedTask; });
            await f.Dispatch(() => ClickAsync(f.Components.OfType<Button>().Last(button => button.Text == "Cancel")));
            Require(await result!.WaitAsync(TimeSpan.FromSeconds(3)) is null, "Cancel accepted the pending action.");
        }));
        tests.Add(("awaitable dialogs reject concurrent controlled and browser-owned opening", async () =>
        {
            await using var f = new DialogFixture(); await f.Render<Dialog>(new() { ["Title"] = "Result" });
            var dialog = f.Component<Dialog>(); Task<object?>? result = null;
            await f.Dispatch(() => { result = dialog.ShowAsync(); return Task.CompletedTask; });
            await f.Dispatch(() => RejectAsync<InvalidOperationException>(() => dialog.ShowAsync()));
            await f.Dispatch(() => dialog.CloseAsync(false));
            Require(Equals(await result!, false), "An explicit false result was replaced by cancellation.");
            await using var controlled = new DialogFixture();
            await controlled.Render<Dialog>(new() { ["Title"] = "Controlled", ["IsOpen"] = true });
            await controlled.Dispatch(() => RejectAsync<InvalidOperationException>(() => controlled.Component<Dialog>().ShowAsync()));
            await using var browser = new DialogFixture();
            await browser.Render<Dialog>(new() { ["Title"] = "Browser", ["BrowserControlled"] = true });
            await browser.Dispatch(() => RejectAsync<InvalidOperationException>(() => browser.Component<Dialog>().ShowAsync()));
        }));
        tests.Add(("busy and vetoed result dialogs preserve pending completion until an allowed close", async () =>
        {
            await using var f = new DialogFixture(); var veto = true; var closes = 0;
            await f.Render<Dialog>(new()
            {
                ["Title"] = "Guarded", ["CanClose"] = (Func<Task<bool>>)(() => Task.FromResult(!veto)),
                ["OnClose"] = EventCallback.Factory.Create(f, () => closes++)
            });
            var dialog = f.Component<Dialog>(); Task<object?>? result = null;
            await f.Dispatch(() => { result = dialog.ShowAsync(); return Task.CompletedTask; });
            await f.Dispatch(() => dialog.CloseAsync("accepted"));
            Require(!result!.IsCompleted && dialog.HasPendingResult && closes == 0, "A veto settled the awaited result.");
            await f.Dispatch(() => dialog.SetParametersAsync(ParameterView.FromDictionary(new Dictionary<string, object?> { ["Busy"] = true })));
            veto = false; await f.Dispatch(() => dialog.RequestCloseAsync());
            Require(!result.IsCompleted && closes == 0, "A busy close accepted or canceled the pending action.");
            await f.Dispatch(() => dialog.SetParametersAsync(ParameterView.FromDictionary(new Dictionary<string, object?> { ["Busy"] = false })));
            await f.Dispatch(() => dialog.CloseAsync("accepted"));
            Require(Equals(await result, "accepted") && closes == 1, "An allowed close did not publish exactly one host result.");
        }));
        tests.Add(("dialog cancellation disposal and close requests resolve null without affecting another dialog", async () =>
        {
            await using var first = new DialogFixture(); await using var second = new DialogFixture();
            await first.Render<Dialog>(new() { ["Title"] = "First" }); await second.Render<Dialog>(new() { ["Title"] = "Second" });
            var a = first.Component<Dialog>(); var b = second.Component<Dialog>(); Task<object?>? ar = null; Task<object?>? br = null;
            using var cancellation = new CancellationTokenSource();
            await first.Dispatch(() => { ar = a.ShowAsync(cancellation.Token); return Task.CompletedTask; });
            await second.Dispatch(() => { br = b.ShowAsync(); return Task.CompletedTask; });
            cancellation.Cancel();
            Require(await ar!.WaitAsync(TimeSpan.FromSeconds(3)) is null && !br!.IsCompleted, "Cancellation leaked to another dialog or approved an action.");
            await second.Dispatch(() => b.RequestCloseAsync());
            Require(await br!.WaitAsync(TimeSpan.FromSeconds(3)) is null, "The X or Escape close path did not cancel.");
            await first.Dispatch(() => { ar = a.ShowAsync(); return Task.CompletedTask; });
            await first.Dispatch(async () => { await a.DisposeAsync(); await a.DisposeAsync(); });
            Require(await ar!.WaitAsync(TimeSpan.FromSeconds(3)) is null && !a.HasPendingResult, "Disposal retained or accepted a pending result.");
            using var alreadyCanceled = new CancellationTokenSource(); alreadyCanceled.Cancel();
            await second.Dispatch(() => { br = b.ShowAsync(alreadyCanceled.Token); return Task.CompletedTask; });
            Require(await br!.WaitAsync(TimeSpan.FromSeconds(3)) is null && !b.HasPendingResult, "An already canceled opening retained a pending result.");
        }));
        tests.Add(("non-dismissable result dialogs accept explicit buttons while stale veto results cannot settle a newer request", async () =>
        {
            await using var f = new DialogFixture(); await f.Render<Dialog>(new() { ["Title"] = "Explicit result", ["Dismissible"] = false });
            var dialog = f.Component<Dialog>(); Task<object?>? result = null;
            await f.Dispatch(() => { result = dialog.ShowAsync(); return Task.CompletedTask; });
            await f.Dispatch(() => dialog.RequestCloseAsync());
            Require(!result!.IsCompleted, "A non-dismissable request was canceled by X or Escape.");
            await f.Dispatch(() => dialog.CloseAsync("explicit"));
            Require(Equals(await result, "explicit"), "Disabling dismissal prevented the actual action result.");
            var gate = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            await f.Dispatch(() => dialog.SetParametersAsync(ParameterView.FromDictionary(new Dictionary<string, object?> { ["CanClose"] = (Func<Task<bool>>)(() => gate.Task) })));
            using var cancellation = new CancellationTokenSource(); Task? oldClose = null;
            await f.Dispatch(() => { result = dialog.ShowAsync(cancellation.Token); oldClose = dialog.CloseAsync("old"); return Task.CompletedTask; });
            cancellation.Cancel();
            Require(await result!.WaitAsync(TimeSpan.FromSeconds(3)) is null, "Cancellation during a close veto accepted its old result.");
            gate.SetResult(true); await oldClose!.WaitAsync(TimeSpan.FromSeconds(3));
            await f.Dispatch(() => dialog.SetParametersAsync(ParameterView.FromDictionary(new Dictionary<string, object?> { ["CanClose"] = null })));
            await f.Dispatch(() => { result = dialog.ShowAsync(); return Task.CompletedTask; });
            Require(!result!.IsCompleted, "A completed stale veto settled the next result request.");
            await f.Dispatch(() => dialog.CloseAsync("new")); Require(Equals(await result, "new"), "The next request retained a stale result.");
        }));
        tests.Add(("copy text is encoded production semantic content without a host selection skin", async () =>
        {
            using var services = Services(); using var scope = services.CreateScope();
            await using var renderer = new HtmlRenderer(scope.ServiceProvider, scope.ServiceProvider.GetRequiredService<ILoggerFactory>());
            var html = await renderer.Dispatcher.InvokeAsync(async () => (await renderer.RenderComponentAsync<CopyText>(ParameterView.FromDictionary(new Dictionary<string, object?> { [nameof(CopyText.Value)] = "token<&>" }))).ToHtmlString());
            Require(html == "<code class=\"f-copy-text\">token&lt;&amp;&gt;</code>", "CopyText recreated a host renderer or lost encoding.");
        }));
    }
    private static Task ClickAsync(Button button) => (Task)typeof(Button).GetMethod("ClickAsync", BindingFlags.Instance | BindingFlags.NonPublic)!.Invoke(button, [new MouseEventArgs()])!;
    private static async Task RejectAsync<T>(Func<Task> action) where T : Exception
    { try { await action(); } catch (T) { return; } throw new InvalidOperationException($"Expected {typeof(T).Name}."); }
    private static ServiceProvider Services()
    {
        var services = new ServiceCollection(); services.AddLogging(); services.AddFlourishFramework();
        services.AddSingleton<NavigationManager>(new TestNavigation()); services.AddSingleton<IJSRuntime>(new FakeJs());
        services.AddScoped<CaptureActivator>(); services.AddScoped<IComponentActivator>(provider => provider.GetRequiredService<CaptureActivator>());
        return services.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true });
    }
    private static void Reject<T>(Action action) where T : Exception
    { try { action(); } catch (T) { return; } throw new InvalidOperationException($"Expected {typeof(T).Name}."); }
    private static void Require(bool value, string message) { if (!value) throw new InvalidOperationException(message); }
    private sealed class CaptureActivator : IComponentActivator
    {
        internal List<IComponent> Components { get; } = [];
        public IComponent CreateInstance(Type type) { var component = (IComponent)Activator.CreateInstance(type)!; Components.Add(component); return component; }
    }
}
