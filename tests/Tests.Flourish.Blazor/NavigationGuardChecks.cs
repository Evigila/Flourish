using System.Reflection;
using ArkheideSystem.Flourish.Blazor.Components;
using Guard = ArkheideSystem.Flourish.Blazor.Components.Primitives.NavigationGuard;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;

internal static class NavigationGuardChecks
{
    internal static void Register(List<(string Name, Func<Task> Run)> tests)
    {
        tests.Add(("navigation guard renders one actual result Dialog and standard buttons for cancel and confirm", async () =>
        {
            var navigation = new GuardNavigation(); await using var f = new DialogFixture(navigation: navigation);
            var output = await f.Render<Guard>(new() { ["HasUnsavedChanges"] = true, ["Message"] = "Discard <draft> & leave?" });
            await f.AfterRender(f.Component<Guard>(), true); Task<bool>? request = null;
            await f.Dispatch(() => { request = navigation.RequestAsync("/other"); return Task.CompletedTask; });
            var dialog = f.Component<Dialog>(); var html = await f.Read(output);
            Require(dialog.HasPendingResult && !request!.IsCompleted && html.Contains("Discard &lt;draft&gt; &amp; leave?"), "The navigation lifecycle did not await its own encoded generic dialog.");
            Require(f.Components.OfType<Dialog>().Count() == 1 && f.Components.OfType<Button>().Count() == 2, "The guard recreated a confirmation service renderer.");
            await f.Dispatch(() => ClickAsync(f.Components.OfType<Button>().Single(button => button.Variant == ButtonVariant.Quiet)));
            Require(!await request!.WaitAsync(TimeSpan.FromSeconds(3)) && !dialog.HasPendingResult, "Cancel allowed losing the unsaved draft.");
            await f.Dispatch(() => { request = navigation.RequestAsync("/other"); return Task.CompletedTask; });
            await f.Dispatch(() => ClickAsync(f.Components.OfType<Button>().Single(button => button.Variant == ButtonVariant.Primary)));
            Require(await request!.WaitAsync(TimeSpan.FromSeconds(3)), "The actual confirm action failed to allow navigation.");
        }));
        tests.Add(("navigation guard canceled navigation cannot approve a later request and preserves local and approved routes", async () =>
        {
            var navigation = new GuardNavigation(); await using var f = new DialogFixture(navigation: navigation);
            await f.Render<Guard>(new() { ["HasUnsavedChanges"] = true }); var guard = f.Component<Guard>(); var dialog = f.Component<Dialog>();
            await f.AfterRender(guard, true); Task<bool>? first = null; Task<bool>? replacement = null;
            await f.Dispatch(() => { first = navigation.RequestAsync("/abandoned"); return Task.CompletedTask; });
            Require(dialog.HasPendingResult, "The first navigation did not await confirmation.");
            await f.Dispatch(() => { replacement = navigation.RequestAsync("/edit#section"); return Task.CompletedTask; });
            Require(!await first!.WaitAsync(TimeSpan.FromSeconds(3)) && await replacement!.WaitAsync(TimeSpan.FromSeconds(3)), "Abandoned confirmation approved a replacement or blocked a local fragment.");
            await f.Dispatch(() => Task.CompletedTask);
            Require(!dialog.HasPendingResult, "Location cancellation left a reusable confirmation dialog pending.");
            await f.Dispatch(() => guard.NavigateToAsync("/saved", true));
            Require(await navigation.RequestAsync("/saved") && !dialog.HasPendingResult
                && f.Javascript.Module.Calls.Any(call => call.Name == "allowNextExternalNavigation"), "Explicit saved navigation lost its one approved native handoff.");
            await f.Dispatch(() => guard.SetParametersAsync(ParameterView.FromDictionary(new Dictionary<string, object?> { ["HasUnsavedChanges"] = false })));
            Require(await navigation.RequestAsync("/clean") && !dialog.HasPendingResult, "A clean form still requires a confirmation.");
            await f.Dispatch(() => guard.SetParametersAsync(ParameterView.FromDictionary(new Dictionary<string, object?> { ["HasUnsavedChanges"] = true })));
            await f.Dispatch(async () => await guard.DisposeAsync());
            Require(await navigation.RequestAsync("/after-disposal"), "Disposal retained the location handler.");
        }));
    }
    private static Task ClickAsync(Button button) => (Task)typeof(Button).GetMethod("ClickAsync", BindingFlags.Instance | BindingFlags.NonPublic)!.Invoke(button, [new MouseEventArgs()])!;
    private static void Require(bool value, string message) { if (!value) throw new InvalidOperationException(message); }
    private sealed class GuardNavigation : NavigationManager
    {
        internal GuardNavigation() => Initialize("http://localhost/", "http://localhost/edit");
        internal Task<bool> RequestAsync(string destination) => NotifyLocationChangingAsync(ToAbsoluteUri(destination).AbsoluteUri, null, false).AsTask();
        protected override void SetNavigationLockState(bool value) { }
        protected override void NavigateToCore(string uri, bool forceLoad) { }
        protected override void NavigateToCore(string uri, NavigationOptions options) { }
    }
}
