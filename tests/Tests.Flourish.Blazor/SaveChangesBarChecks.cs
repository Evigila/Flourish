using System.Reflection;
using ArkheideSystem.Flourish.Blazor.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

internal static class SaveChangesBarChecks
{
    internal static void Register(List<(string Name, Func<Task> Run)> tests)
    {
        tests.Add(("save changes bar keeps clean markup empty and composes two actual encoded standard actions", async () =>
        {
            await using var f = new DialogFixture();
            var output = await f.Render<SaveChangesBar>(new() { ["Id"] = "draft-actions", ["Class"] = "host-context" });
            var bar = f.Component<SaveChangesBar>(); var html = await f.Read(output);
            Require(html.Contains("data-active=\"false\"") && !html.Contains("f-save-changes-panel") && !f.Components.OfType<Button>().Any(), "A clean page retained a visible bar or interactive actions.");
            await f.AfterRender(bar, true);
            Require(f.Javascript.Imports == 0, "A clean bar imported browser navigation guards.");
            await f.Dispatch(() => bar.SetParametersAsync(Parameters(new()
            {
                ["HasChanges"] = true, ["Message"] = "Draft <one> & two", ["SaveText"] = "Commit <draft>", ["DiscardText"] = "Restore & reset",
                ["OnSave"] = EventCallback.Factory.Create(new object(), () => { }), ["OnDiscard"] = EventCallback.Factory.Create(new object(), () => { })
            })));
            html = await f.Read(output);
            Require(html.Contains("Draft &lt;one&gt; &amp; two") && html.Contains("Commit &lt;draft&gt;") && html.Contains("Restore &amp; reset"), "Host text was replaced or rendered as executable markup.");
            Require(html.Contains("role=\"status\"") && html.Contains("aria-live=\"polite\"") && html.Contains("draft-actions"), "The persistent message lost its live status or host identity.");
            var buttons = f.Components.OfType<Button>().ToArray();
            Require(buttons.Length == 2 && buttons[0].Variant == ButtonVariant.Primary && buttons[1].Variant == ButtonVariant.Quiet, "The bar did not use the shared save/discard button variants.");
            Require(!f.Components.OfType<Dialog>().Any(), "Hard navigation blocking unexpectedly created a discard dialog.");
        }));

        tests.Add(("save changes bar blocks routes query refresh and unchanged fragments while retaining a new local fragment", async () =>
        {
            var navigation = new BarNavigation("http://localhost/edit#original"); await using var f = new DialogFixture(navigation: navigation);
            await f.Render<SaveChangesBar>(new() { ["HasChanges"] = true }); var bar = f.Component<SaveChangesBar>();
            foreach (var destination in new[] { "/other", "/edit?mode=other#next", "/edit", "/edit#", "/edit#original", "/Edit#next", "https://other.test/edit#next" })
                Require(!await navigation.RequestAsync(destination), "An unsaved draft allowed navigation to " + destination);
            Require(await navigation.RequestAsync("/edit#next"), "A distinct same-document fragment was blocked.");
            await f.Dispatch(() => bar.SetParametersAsync(Parameters(new() { ["HasChanges"] = false, ["Busy"] = true })));
            Require(!await navigation.RequestAsync("/other"), "A busy-only save allowed navigation.");
            Require(f.Components.OfType<Button>().All(button => button.Disabled), "Busy actions were not disabled.");
            await f.Dispatch(() => bar.SetParametersAsync(Parameters(new() { ["Busy"] = false })));
            Require(await navigation.RequestAsync("/other"), "A clean idle page retained its navigation lock.");
            await f.Dispatch(() => bar.SetParametersAsync(Parameters(new() { ["HasChanges"] = true })));
            await f.Dispatch(() => bar.DisposeAsync().AsTask());
            Require(await navigation.RequestAsync("/other"), "Disposal retained the navigation registration.");
        }));

        tests.Add(("save changes callbacks retain internal busy navigation lock after the host clears dirty state", async () =>
        {
            var navigation = new BarNavigation(); await using var f = new DialogFixture(navigation: navigation);
            var completion = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            var saves = 0; var discards = 0;
            await f.Render<SaveChangesBar>(new()
            {
                ["HasChanges"] = true, ["OnSave"] = EventCallback.Factory.Create(new object(), async () => { saves++; await completion.Task; }),
                ["OnDiscard"] = EventCallback.Factory.Create(new object(), () => discards++)
            });
            var bar = f.Component<SaveChangesBar>(); Task? pending = null;
            await f.Dispatch(() => { pending = Invoke(bar, "SaveAsync"); return Task.CompletedTask; });
            await f.Dispatch(() => bar.SetParametersAsync(Parameters(new() { ["HasChanges"] = false })));
            Require(!await navigation.RequestAsync("/other"), "Clearing dirty state during a callback released navigation early.");
            await f.Dispatch(() => Invoke(bar, "DiscardAsync")); await f.Dispatch(() => Invoke(bar, "SaveAsync"));
            Require(saves == 1 && discards == 0 && f.Components.OfType<Button>().All(button => button.Disabled), "A pending save allowed duplicate or discard actions.");
            completion.SetResult(); await pending!.WaitAsync(TimeSpan.FromSeconds(3));
            Require(await navigation.RequestAsync("/other"), "Completed clean state did not release navigation.");
        }));

        foreach (var action in new[] { "SaveAsync", "DiscardAsync" })
            tests.Add(("save changes " + action + " failure releases busy and permits retry without clearing the draft", async () =>
            {
                var navigation = new BarNavigation(); await using var f = new DialogFixture(navigation: navigation);
                var completion = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
                var calls = 0;
                var callback = EventCallback.Factory.Create(new object(), async () => { calls++; await completion.Task; throw new InvalidOperationException("Expected save bar failure"); });
                var callbackName = action == "SaveAsync" ? "OnSave" : "OnDiscard";
                await f.Render<SaveChangesBar>(new() { ["HasChanges"] = true, [callbackName] = callback });
                var bar = f.Component<SaveChangesBar>(); Task? pending = null;
                await f.Dispatch(() => { pending = Invoke(bar, action); return Task.CompletedTask; });
                await f.Dispatch(() => bar.SetParametersAsync(Parameters(new() { ["HasChanges"] = true })));
                Require(f.Components.OfType<Button>().All(button => button.Disabled), "A running callback was not presented as busy.");
                completion.SetResult(); Exception? failure = null;
                try { await pending!.WaitAsync(TimeSpan.FromSeconds(3)); } catch (InvalidOperationException error) { failure = error; }
                await f.Dispatch(() => Task.CompletedTask);
                Require(failure?.Message == "Expected save bar failure" && bar.HasChanges && !bar.Busy, "The callback failure was hidden or cleared the host draft.");
                var variant = action == "SaveAsync" ? ButtonVariant.Primary : ButtonVariant.Quiet;
                Require(!f.Components.OfType<Button>().Single(button => button.Variant == variant).Disabled, "A failed callback left stale busy parameters on the action.");
                Require(!await navigation.RequestAsync("/other"), "Failure released the unsaved navigation lock.");
                await f.Dispatch(() => bar.SetParametersAsync(Parameters(new() { [callbackName] = EventCallback.Factory.Create(new object(), () => calls++) })));
                await f.Dispatch(() => Invoke(bar, action));
                Require(calls == 2 && bar.HasChanges, "Retry was rejected or the component mutated draft ownership.");
            }));

        tests.Add(("save changes browser attachment follows active roots and releases after inactive and disposal", async () =>
        {
            await using var f = new DialogFixture();
            await f.Render<SaveChangesBar>(new() { ["HasChanges"] = true }); var bar = f.Component<SaveChangesBar>();
            await f.AfterRender(bar, true); await f.AfterRender(bar);
            Require(f.Javascript.Imports == 1 && f.Javascript.Module.Calls.Count(call => call.Name == "initializeSaveChangesBar") == 1
                && f.Javascript.Module.Calls.Any(call => call.Name == "updateSaveChangesBar"), "Rerender duplicated the instance guard or imported more modules.");
            await f.Dispatch(() => bar.SetParametersAsync(Parameters(new() { ["HasChanges"] = false })));
            await f.AfterRender(bar);
            Require(f.Javascript.Module.Calls.Count(call => call.Name == "disposeSaveChangesBar") == 1, "Inactive state retained native guards or measured clearance.");
            await f.Dispatch(() => bar.SetParametersAsync(Parameters(new() { ["HasChanges"] = true })));
            await f.AfterRender(bar);
            await f.Dispatch(() => bar.DisposeAsync().AsTask());
            Require(f.Javascript.Module.Calls.Count(call => call.Name == "initializeSaveChangesBar") == 2
                && f.Javascript.Module.Calls.Count(call => call.Name == "disposeSaveChangesBar") == 2, "Reactivation or disposal leaked a root registration.");
        }));

        tests.Add(("save changes disposal during browser import releases the late module without attaching a guard", async () =>
        {
            var javascript = new PendingJs(); var navigation = new BarNavigation();
            await using var f = new DialogFixture(navigation: navigation, javascript: javascript);
            await f.Render<SaveChangesBar>(new() { ["HasChanges"] = true }); var bar = f.Component<SaveChangesBar>();
            var rendering = f.AfterRender(bar, true);
            await javascript.Started.Task.WaitAsync(TimeSpan.FromSeconds(3));
            Task? disposing = null;
            await f.Dispatch(() => { disposing = bar.DisposeAsync().AsTask(); return Task.CompletedTask; });
            javascript.Completion.SetResult(javascript.Module);
            await Task.WhenAll(rendering, disposing!).WaitAsync(TimeSpan.FromSeconds(3));
            Require(javascript.Module.Disposals == 1 && javascript.Module.Calls == 0, "A late import attached listeners after disposal or leaked its reference.");
            Require(await navigation.RequestAsync("/other"), "Pending import disposal retained the managed navigation lock.");
        }));
    }

    private static ParameterView Parameters(Dictionary<string, object?> values) => ParameterView.FromDictionary(values);
    private static Task Invoke(SaveChangesBar component, string name) => (Task)typeof(SaveChangesBar).GetMethod(name, BindingFlags.Instance | BindingFlags.NonPublic)!.Invoke(component, null)!;
    private static void Require(bool value, string message) { if (!value) throw new InvalidOperationException(message); }
    private sealed class BarNavigation : NavigationManager
    {
        internal BarNavigation(string current = "http://localhost/edit") => Initialize("http://localhost/", current);
        internal Task<bool> RequestAsync(string target) => NotifyLocationChangingAsync(ToAbsoluteUri(target).AbsoluteUri, null, false).AsTask();
        protected override void SetNavigationLockState(bool value) { }
        protected override void NavigateToCore(string uri, bool forceLoad) { }
        protected override void NavigateToCore(string uri, NavigationOptions options) { }
    }
    private sealed class PendingJs : IJSRuntime
    {
        internal TaskCompletionSource<IJSObjectReference> Completion { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        internal TaskCompletionSource Started { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        internal PendingModule Module { get; } = new();
        public ValueTask<T> InvokeAsync<T>(string identifier, object?[]? args) => InvokeAsync<T>(identifier, CancellationToken.None, args);
        public async ValueTask<T> InvokeAsync<T>(string identifier, CancellationToken cancellationToken, object?[]? args)
        {
            if (identifier != "import") throw new InvalidOperationException("Unexpected browser call " + identifier);
            Started.TrySetResult();
            return (T)(object)await Completion.Task;
        }
    }
    private sealed class PendingModule : IJSObjectReference
    {
        internal int Disposals, Calls;
        public ValueTask<T> InvokeAsync<T>(string identifier, object?[]? args) => InvokeAsync<T>(identifier, CancellationToken.None, args);
        public ValueTask<T> InvokeAsync<T>(string identifier, CancellationToken cancellationToken, object?[]? args) { Calls++; return ValueTask.FromResult(default(T)!); }
        public ValueTask DisposeAsync() { Disposals++; return ValueTask.CompletedTask; }
    }
}
