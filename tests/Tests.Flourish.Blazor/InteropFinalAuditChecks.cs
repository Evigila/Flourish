using System.Reflection;
using System.Text.RegularExpressions;
using ArkheideSystem.Flourish.Blazor;
using ArkheideSystem.Flourish.Blazor.Components;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using Primitives = ArkheideSystem.Flourish.Blazor.Components.Primitives;
using Patterns = ArkheideSystem.Flourish.Blazor.Components.Patterns;

internal static class InteropFinalAuditChecks
{
    internal static void Register(List<(string Name, Func<Task> Run)> tests)
    {
        tests.Add(("offer cards reserve registered IDs and semantic attributes before unmatched attributes", async () =>
        {
            await WithComponent<OfferCard>(new DeferredJs(), new()
            {
                [nameof(OfferCard.Id)] = "registered-offer", [nameof(OfferCard.Title)] = "Offer",
                [nameof(OfferCard.Class)] = "approved-card",
                [nameof(OfferCard.AdditionalAttributes)] = Conflicts()
            }, (_, _, html) =>
            {
                var tag = Regex.Match(html(), @"<article\b[^>]*>").Value;
                Require(Attribute(tag, "id") == "registered-offer", "Unmatched attributes changed the registered card ID.");
                Require(Attribute(tag, "class") == "f-offer-card approved-card", "Unmatched attributes replaced the required card classes.");
                Require(Attribute(tag, "role") == "listitem" && Attribute(tag, "tabindex") == "0", "Card list and focus semantics were overwritten.");
                Require(Attribute(tag, "data-test") == "kept", "An ordinary unmatched attribute was lost.");
                return Task.CompletedTask;
            });
        }));
        tests.Add(("offer stages reserve configured IDs and list semantics before unmatched attributes", async () =>
        {
            await WithComponent<StageProbe>(new DeferredJs(), new()
            {
                [nameof(OfferStage.Id)] = "configured-stage", [nameof(OfferStage.Label)] = "Offers",
                [nameof(OfferStage.Class)] = "approved-stage", [nameof(OfferStage.AutoRotate)] = false,
                [nameof(OfferStage.AdditionalAttributes)] = Conflicts()
            }, (_, _, html) =>
            {
                var tag = Regex.Match(html(), @"<div\b[^>]*>").Value;
                Require(Attribute(tag, "id") == "configured-stage", "Unmatched attributes changed the stage ID.");
                Require(Attribute(tag, "class") == "f-offer-stage approved-stage", "Unmatched attributes replaced required stage classes.");
                Require(Attribute(tag, "role") == "list" && Attribute(tag, "aria-label") == "Offers", "Stage list semantics were overwritten.");
                Require(Attribute(tag, "data-test") == "kept", "An ordinary stage unmatched attribute was lost.");
                return Task.CompletedTask;
            });
        }));
        foreach (var kind in Enum.GetValues<BridgeKind>())
        {
            tests.Add(($"{kind} releases a late import once without invoking disposed component behavior", async () =>
            {
                await WithBridge(kind, new DeferredJs { DelayImport = true }, async (component, javascript) =>
                {
                    var pending = InvokeBridge(kind, component);
                    await javascript.ImportStarted.Task.WaitAsync(TimeSpan.FromSeconds(5));
                    await DisposeBridge(component);
                    await DisposeBridge(component);
                    javascript.CompleteImport();
                    await pending;
                    Require(javascript.Module.Releases == 1 && javascript.Module.BehaviorCalls == 0,
                        "A late module was leaked, released twice, or invoked after component disposal.");
                    await InvokeBridge(kind, component);
                    Require(javascript.Imports == 1, "A disposed component imported another module.");
                    if (kind == BridgeKind.Reference) Require(IsReferenceOpen(component), "Late focus checking mutated a disposed dropdown.");
                    if (kind == BridgeKind.Stage) Require(!StageInitialized(component), "A late import initialized a disposed stage.");
                });
            }));
            tests.Add(($"{kind} handles synchronous imports and repeated teardown without reimporting", async () =>
            {
                await WithBridge(kind, new DeferredJs(), async (component, javascript) =>
                {
                    await InvokeBridge(kind, component);
                    Require(javascript.Imports == 1 && javascript.Module.BehaviorCalls == 1, "The synchronous bridge did not execute once.");
                    await DisposeBridge(component);
                    await DisposeBridge(component);
                    await InvokeBridge(kind, component);
                    Require(javascript.Module.Releases == 1 && javascript.Imports == 1 && javascript.Module.BehaviorCalls == 1,
                        "Repeated teardown or a disposed handler reused or leaked the bridge.");
                });
            }));
            tests.Add(($"{kind} shares concurrent imports and releases late completion exactly once", async () =>
            {
                await WithBridge(kind, new DeferredJs { DelayImport = true }, async (component, javascript) =>
                {
                    var first = InvokeBridge(kind, component);
                    await javascript.ImportStarted.Task.WaitAsync(TimeSpan.FromSeconds(5));
                    var second = InvokeBridge(kind, component);
                    await Task.Yield();
                    Require(javascript.Imports == 1, "Concurrent callbacks imported competing module owners.");
                    await DisposeBridge(component);
                    javascript.CompleteImport();
                    await Task.WhenAll(first, second);
                    Require(javascript.Module.Releases == 1 && javascript.Module.BehaviorCalls == 0,
                        "Concurrent import completion invoked a disposed bridge or released its proxy twice.");
                });
            }));
            tests.Add(($"{kind} tolerates a disconnected import without mutating disposed state", async () =>
            {
                await WithBridge(kind, new DeferredJs { DisconnectImport = true }, async (component, javascript) =>
                {
                    await InvokeBridge(kind, component);
                    await DisposeBridge(component);
                    await DisposeBridge(component);
                    Require(javascript.Module.Releases == 0 && javascript.Module.BehaviorCalls == 0, "A disconnected import fabricated an initialized bridge.");
                });
            }));
            foreach (var canceled in new[] { false, true })
            {
                tests.Add(($"{kind} releases once despite {(canceled ? "canceled" : "disconnected")} {(kind == BridgeKind.Reference ? "proxy teardown" : "DOM cleanup")}", async () =>
                {
                    await WithBridge(kind, new DeferredJs(), async (component, javascript) =>
                    {
                        await InvokeBridge(kind, component);
                        var failure = canceled ? (Exception)new TaskCanceledException() : new JSDisconnectedException("Closed circuit");
                        if (kind == BridgeKind.Reference) javascript.Module.FailRelease = failure;
                        else javascript.Module.FailCleanup = failure;
                        await DisposeBridge(component);
                        await DisposeBridge(component);
                        Require(javascript.Module.Releases == 1, "Failed DOM cleanup skipped proxy release or repeated it.");
                        if (kind != BridgeKind.Reference) Require(javascript.Module.CleanupCalls == 1, "DOM teardown was skipped or repeated.");
                    });
                }));
            }
            tests.Add(($"{kind} swallows disconnected proxy release exactly once", async () =>
            {
                await WithBridge(kind, new DeferredJs(), async (component, javascript) =>
                {
                    await InvokeBridge(kind, component);
                    javascript.Module.DisconnectRelease = true;
                    await DisposeBridge(component);
                    await DisposeBridge(component);
                    Require(javascript.Module.Releases == 1, "Disconnected proxy teardown was retried.");
                });
            }));
        }
        tests.Add(("reference dropdown ignores a focus result that arrives after disposal", async () =>
        {
            await WithBridge(BridgeKind.Reference, new DeferredJs(), async (component, javascript) =>
            {
                javascript.Module.DelayFocus = true;
                var pending = InvokeBridge(BridgeKind.Reference, component);
                await javascript.Module.FocusStarted.Task.WaitAsync(TimeSpan.FromSeconds(5));
                await DisposeBridge(component);
                javascript.Module.CompleteFocus();
                await pending;
                Require(IsReferenceOpen(component), "A completed focus query changed a disposed dropdown's state.");
                Require(javascript.Module.Releases == 1, "The focus race lost module ownership.");
            });
        }));
        tests.Add(("offer stage ignores synchronization completion after disposal", async () =>
        {
            await WithBridge(BridgeKind.Stage, new DeferredJs(), async (component, javascript) =>
            {
                javascript.Module.DelaySynchronization = true;
                var pending = InvokeBridge(BridgeKind.Stage, component);
                await javascript.Module.SynchronizationStarted.Task.WaitAsync(TimeSpan.FromSeconds(5));
                await DisposeBridge(component);
                javascript.Module.CompleteSynchronization();
                await pending;
                Require(!StageInitialized(component), "Completed synchronization initialized a disposed offer stage.");
                Require(javascript.Module.Releases == 1 && javascript.Module.CleanupCalls == 1, "Synchronization teardown lost module ownership.");
            });
        }));
        tests.Add(("document-flow content never imports fixed-stage interaction", async () =>
        {
            await WithComponent<ContentProbe>(new DeferredJs(), new() { [nameof(Patterns.ContentSurface.DocumentFlow)] = true }, async (component, javascript, _) =>
            {
                await component.InteractiveRenderAsync();
                await component.InteractiveRenderAsync();
                Require(javascript.Imports == 0, "A document-flow page initialized fixed-stage scrolling.");
            });
        }));
        tests.Add(("content switching to document flow rejects a late fixed-stage import", async () =>
        {
            await WithComponent<ContentProbe>(new DeferredJs { DelayImport = true }, new(), async (component, javascript, _) =>
            {
                var pending = component.InteractiveRenderAsync();
                await javascript.ImportStarted.Task.WaitAsync(TimeSpan.FromSeconds(5));
                await component.SetDocumentFlowAsync(true);
                await component.InteractiveRenderAsync();
                var oldModule = javascript.Module;
                javascript.CompleteImport();
                await pending;
                Require(oldModule.Releases == 1 && oldModule.BehaviorCalls == 0,
                    "A late fixed-stage module installed scrolling or leaked after switching document flow.");
                await component.SetDocumentFlowAsync(false);
                await component.InteractiveRenderAsync();
                Require(javascript.Imports == 2 && javascript.Module.BehaviorCalls == 1,
                    "Returning to fixed-stage layout did not obtain a fresh usable module.");
            });
        }));
        tests.Add(("content mode changes detach owned fixed-stage behavior once before reinitialization", async () =>
        {
            await WithComponent<ContentProbe>(new DeferredJs(), new(), async (component, javascript, _) =>
            {
                await component.InteractiveRenderAsync();
                var oldModule = javascript.Module;
                await component.SetDocumentFlowAsync(true);
                await component.InteractiveRenderAsync();
                await component.InteractiveRenderAsync();
                Require(oldModule.CleanupCalls == 1 && oldModule.Releases == 1, "Document flow retained or repeatedly disposed fixed-stage listeners.");
                await component.SetDocumentFlowAsync(false);
                await component.InteractiveRenderAsync();
                Require(javascript.Imports == 2 && javascript.Module.BehaviorCalls == 1, "Restoring fixed-stage mode reused a released module.");
                await component.DisposeAsync();
                Require(javascript.Module.Releases == 1 && oldModule.Releases == 1, "Mode changes confused module ownership.");
            });
        }));
    }

    private static Dictionary<string, object> Conflicts() => new()
    {
        ["id"] = "unregistered-id", ["class"] = "replacement", ["role"] = "button",
        ["tabindex"] = "-1", ["aria-label"] = "Replacement label", ["data-test"] = "kept"
    };
    private static string Attribute(string tag, string name) => Regex.Match(tag, $"\\b{Regex.Escape(name)}=\"([^\"]*)\"").Groups[1].Value;
    private static void Require(bool condition, string message) { if (!condition) throw new InvalidOperationException(message); }
    private enum BridgeKind { Stage, RowMenu, Reference, Content, Navigation, Table }

    private static Task WithBridge(BridgeKind kind, DeferredJs javascript, Func<IComponent, DeferredJs, Task> verify) => kind switch
    {
        BridgeKind.Stage => WithComponent<StageProbe>(javascript, new() { [nameof(OfferStage.AutoRotate)] = false }, (component, js, _) => verify(component, js)),
        BridgeKind.RowMenu => WithComponent<Primitives.RowActionMenu>(javascript, new()
        {
            [nameof(Primitives.RowActionMenu.ChildContent)] = (RenderFragment)(builder => builder.AddContent(0, "Action"))
        }, (component, js, _) => verify(component, js)),
        BridgeKind.Content => WithComponent<ContentProbe>(javascript, new(), (component, js, _) => verify(component, js)),
        BridgeKind.Navigation => WithComponent<NavigationProbe>(javascript, new(), (component, js, _) => verify(component, js)),
        BridgeKind.Table => WithComponent<TableProbe>(javascript, new(), (component, js, _) => verify(component, js)),
        _ => WithComponent<Primitives.ReferenceDropdown<int>>(javascript, new()
        {
            [nameof(Primitives.ReferenceDropdown<int>.Id)] = "reference", [nameof(Primitives.ReferenceDropdown<int>.LabelledBy)] = "reference-label"
        }, (component, js, _) => { SetReferenceOpen(component); return verify(component, js); })
    };

    private static Task InvokeBridge(BridgeKind kind, IComponent component) => kind switch
    {
        BridgeKind.Stage => ((StageProbe)component).InteractiveRenderAsync(),
        BridgeKind.RowMenu => InvokePrivate(typeof(Primitives.RowActionMenu), component, "ToggleAsync"),
        BridgeKind.Content => ((ContentProbe)component).InteractiveRenderAsync(),
        BridgeKind.Navigation => ((NavigationProbe)component).InteractiveRenderAsync(),
        BridgeKind.Table => ((TableProbe)component).InteractiveRenderAsync(),
        _ => InvokePrivate(typeof(Primitives.ReferenceDropdown<int>), component, "CloseIfFocusLeftAsync")
    };
    private static Task InvokePrivate(Type type, object component, string name) =>
        (Task)(type.GetMethod(name, BindingFlags.NonPublic | BindingFlags.Instance)!.Invoke(component, null)!);
    private static ValueTask DisposeBridge(IComponent component) => ((IAsyncDisposable)component).DisposeAsync();
    private static void SetReferenceOpen(object component) => typeof(Primitives.ReferenceDropdown<int>).GetField("IsOpen", BindingFlags.NonPublic | BindingFlags.Instance)!.SetValue(component, true);
    private static bool IsReferenceOpen(object component) => (bool)typeof(Primitives.ReferenceDropdown<int>).GetField("IsOpen", BindingFlags.NonPublic | BindingFlags.Instance)!.GetValue(component)!;
    private static bool StageInitialized(object component) => (bool)typeof(OfferStage).GetField("initialized", BindingFlags.NonPublic | BindingFlags.Instance)!.GetValue(component)!;

    private static async Task WithComponent<T>(DeferredJs javascript, Dictionary<string, object?> parameters, Func<T, DeferredJs, Func<string>, Task> verify) where T : class, IComponent
    {
        var activator = new ProbeActivator<T>();
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
            var output = await renderer.RenderComponentAsync<T>(ParameterView.FromDictionary(parameters));
            await verify(activator.Component ?? throw new InvalidOperationException("Audit component was not constructed."), javascript, output.ToHtmlString);
        });
    }
    private sealed class ProbeActivator<T> : IComponentActivator where T : class, IComponent
    {
        internal T? Component { get; private set; }
        public IComponent CreateInstance(Type componentType)
        {
            var component = (IComponent)Activator.CreateInstance(componentType)!;
            if (component is T target) Component = target;
            return component;
        }
    }
    private sealed class StageProbe : OfferStage
    {
        public StageProbe() { }
        internal Task InteractiveRenderAsync() => base.OnAfterRenderAsync(false);
    }
    private sealed class ContentProbe : Patterns.ContentSurface
    {
        public ContentProbe() { }
        internal Task InteractiveRenderAsync() => base.OnAfterRenderAsync(false);
        internal Task SetDocumentFlowAsync(bool documentFlow) => SetParametersAsync(ParameterView.FromDictionary(new Dictionary<string, object?>
        {
            [nameof(DocumentFlow)] = documentFlow
        }));
    }
    private sealed class NavigationProbe : Patterns.NavigationSurface
    {
        public NavigationProbe() { }
        internal Task InteractiveRenderAsync() => base.OnAfterRenderAsync(false);
    }
    private sealed record AuditRow(string Name);
    private sealed class TableProbe : DataTable<AuditRow>
    {
        public TableProbe() { }
        internal Task InteractiveRenderAsync() => base.OnAfterRenderAsync(false);
    }
    private sealed class DeferredJs : IJSRuntime
    {
        internal bool DelayImport { get; init; }
        internal bool DisconnectImport { get; init; }
        internal int Imports { get; private set; }
        internal CountingModule Module { get; private set; } = new();
        internal TaskCompletionSource ImportStarted { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        private readonly TaskCompletionSource<IJSObjectReference> pendingImport = new(TaskCreationOptions.RunContinuationsAsynchronously);
        internal void CompleteImport() => pendingImport.TrySetResult(Module);
        public ValueTask<TValue> InvokeAsync<TValue>(string identifier, object?[]? args) => InvokeAsync<TValue>(identifier, CancellationToken.None, args);
        public ValueTask<TValue> InvokeAsync<TValue>(string identifier, CancellationToken cancellationToken, object?[]? args)
        {
            if (identifier != "import") throw new InvalidOperationException("Unexpected audit JS entry: " + identifier);
            Imports++;
            if (Imports > 1) Module = new();
            ImportStarted.TrySetResult();
            if (DisconnectImport) return ValueTask.FromException<TValue>(new JSDisconnectedException("Closed circuit"));
            return DelayImport && Imports == 1 ? new ValueTask<TValue>(CompleteAsync<TValue>()) : ValueTask.FromResult((TValue)(object)Module);
        }
        private async Task<TValue> CompleteAsync<TValue>() => (TValue)(object)await pendingImport.Task;
    }
    private sealed class CountingModule : IJSObjectReference
    {
        internal int BehaviorCalls { get; private set; }
        internal int Releases { get; private set; }
        internal int CleanupCalls { get; private set; }
        internal Exception? FailCleanup { get; set; }
        internal Exception? FailRelease { get; set; }
        internal bool DisconnectRelease { get; set; }
        internal bool DelayFocus { get; set; }
        internal bool DelaySynchronization { get; set; }
        internal TaskCompletionSource FocusStarted { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        internal TaskCompletionSource SynchronizationStarted { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        private readonly TaskCompletionSource<bool> pendingFocus = new(TaskCreationOptions.RunContinuationsAsynchronously);
        private readonly TaskCompletionSource pendingSynchronization = new(TaskCreationOptions.RunContinuationsAsynchronously);
        internal void CompleteFocus() => pendingFocus.TrySetResult(false);
        internal void CompleteSynchronization() => pendingSynchronization.TrySetResult();
        public ValueTask<TValue> InvokeAsync<TValue>(string identifier, object?[]? args) => InvokeAsync<TValue>(identifier, CancellationToken.None, args);
        public ValueTask<TValue> InvokeAsync<TValue>(string identifier, CancellationToken cancellationToken, object?[]? args)
        {
            if (identifier is "dispose" or "detach")
            {
                CleanupCalls++;
                return FailCleanup is not null ? ValueTask.FromException<TValue>(FailCleanup) : ValueTask.FromResult(default(TValue)!);
            }
            BehaviorCalls++;
            if (identifier == "containsFocus")
            {
                FocusStarted.TrySetResult();
                return DelayFocus ? new ValueTask<TValue>(CompleteFocusAsync<TValue>()) : ValueTask.FromResult((TValue)(object)false);
            }
            if (identifier is not "synchronize" and not "toggle") throw new InvalidOperationException("Unexpected audit module call: " + identifier);
            if (identifier == "synchronize")
            {
                SynchronizationStarted.TrySetResult();
                if (DelaySynchronization) return new ValueTask<TValue>(CompleteSynchronizationAsync<TValue>());
            }
            return ValueTask.FromResult(default(TValue)!);
        }
        private async Task<TValue> CompleteFocusAsync<TValue>() => (TValue)(object)await pendingFocus.Task;
        private async Task<TValue> CompleteSynchronizationAsync<TValue>() { await pendingSynchronization.Task; return default!; }
        public ValueTask DisposeAsync()
        {
            Releases++;
            return FailRelease is not null ? ValueTask.FromException(FailRelease)
                : DisconnectRelease ? ValueTask.FromException(new JSDisconnectedException("Closed circuit")) : ValueTask.CompletedTask;
        }
    }
}
