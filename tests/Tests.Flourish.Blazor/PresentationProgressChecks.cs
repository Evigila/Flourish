using System.Text.RegularExpressions;
using ArkheideSystem.Flourish.Blazor;
using ArkheideSystem.Flourish.Blazor.Components;
using ArkheideSystem.Flourish.Blazor.Components.Patterns;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;

internal static class PresentationProgressChecks
{
    internal static void Register(List<(string Name, Func<Task> Run)> tests)
    {
        tests.Add(("presentation progress is opt-in and SSR exposes its scoped host without inventing sections", async () =>
        {
            RenderFragment bands = builder => {
                builder.OpenComponent<PresentationHero>(0); builder.AddAttribute(1, nameof(PresentationHero.Title), "First <scene>"); builder.CloseComponent();
                builder.OpenComponent<PresentationBand>(2); builder.AddAttribute(3, nameof(PresentationBand.Title), "Second"); builder.CloseComponent();
            };
            RenderFragment footer = builder => {
                builder.OpenComponent<PresentationFooter>(0);
                builder.AddAttribute(1, nameof(PresentationFooter.ProjectName), "Natural footer");
                builder.AddAttribute(2, nameof(PresentationFooter.Actions), (RenderFragment)(actions => {
                    actions.OpenComponent<PresentationBand>(0); actions.AddAttribute(1, nameof(PresentationBand.Title), "Footer-only content"); actions.CloseComponent();
                }));
                builder.CloseComponent();
            };
            var parameters = new Dictionary<string, object?> { ["DocumentFlow"] = true, ["ContentId"] = "home-content", ["ChildContent"] = bands, ["Footer"] = footer };
            var ordinary = await Render<ContentSurface>(parameters);
            Require(!ordinary.Contains("f-presentation-progress", StringComparison.Ordinal), "A business or single-band scene implicitly enabled progress.");
            parameters[nameof(ContentSurface.ShowPresentationProgress)] = true;
            var html = await Render<ContentSurface>(parameters);
            var navigation = Regex.Match(html, @"<nav\b[^>]*class=""f-presentation-progress[^""]*""[^>]*>");
            Require(navigation.Success && navigation.Value.Contains("data-content-id=\"home-content\"", StringComparison.Ordinal)
                && navigation.Value.Contains(" hidden", StringComparison.Ordinal), "Static SSR lost its bootstrap target or fabricated discovered segments.");
            Require(html.Contains("<ol class=\"f-tutorial-segments\"></ol>", StringComparison.Ordinal), "Presentation progress does not use the actual tutorial segment container.");
            Require(Regex.Matches(html, @"data-presentation-band=""true""").Count == 3, "Hero emits duplicate progress markers or a real band lost its marker.");
            Require(html.IndexOf("data-presentation-footer", StringComparison.Ordinal) > html.IndexOf("</main>", StringComparison.Ordinal)
                && html.IndexOf("f-presentation-progress", StringComparison.Ordinal) > html.IndexOf("Footer-only content", StringComparison.Ordinal), "Progress is inside the footer's operation area or its slot lacks a semantic exclusion boundary.");
            Require(!navigation.Value.Contains("title=", StringComparison.Ordinal) && !html.Contains("role=\"tooltip\"", StringComparison.Ordinal), "Presentation progress introduced hovering titles.");
        }));
        tests.Add(("progress identifiers are unique while explicit content identifiers and semantic opt-out stay intact", async () =>
        {
            var one = await Render<PresentationProgress>(new() { ["ContentId"] = "shared-id", ["Label"] = "Scenes <draft>" });
            var two = await Render<PresentationProgress>(new() { ["ContentId"] = "shared-id" });
            var id = static (string html) => Regex.Match(html, @"id=""([^""]+)""").Groups[1].Value;
            Require(id(one) != id(two), "Multiple progress surfaces reuse a navigator identifier.");
            Require(one.Contains("Scenes &lt;draft&gt;", StringComparison.Ordinal), "Host accessible text bypasses HTML encoding.");
            foreach (var invalid in new[] { "", "bad identifier" })
            {
                var rejected = false;
                try { await Render<PresentationProgress>(new() { ["ContentId"] = invalid }); }
                catch (ArgumentException) { rejected = true; }
                Require(rejected, "An invalid scoped content identifier reached JS.");
            }
            var band = await Render<PresentationBand>(new() { ["IncludeInProgress"] = false, ["FullHeight"] = false, ["MinHeight"] = 240 });
            var hero = await Render<PresentationHero>(new() { ["Title"] = "Optional hero", ["IncludeInProgress"] = false });
            Require(!band.Contains("data-presentation-band", StringComparison.Ordinal) && !hero.Contains("data-presentation-band", StringComparison.Ordinal), "Semantic opt-out is ignored by a band or its Hero wrapper.");
            Require(band.Contains("--f-band-min-height:240px;", StringComparison.Ordinal) && !band.Contains("f-presentation-fullheight", StringComparison.Ordinal), "Explicit pixel height and viewport opt-out lost compatibility.");
        }));
        tests.Add(("progress disposal rejects late imports and releases the module once", async () =>
        {
            var javascript = new DeferredJs { DelayImport = true };
            await WithProgress(javascript, async component => {
                var pending = component.InteractiveRenderAsync();
                await javascript.Started.Task.WaitAsync(TimeSpan.FromSeconds(5));
                await component.DisposeAsync();
                javascript.CompleteImport();
                await pending;
                Require(javascript.Module.Synchronizations == 0 && javascript.Module.Releases == 1, "A late import attached a disposed navigation or leaked its module.");
                await component.DisposeAsync();
                Require(javascript.Module.Releases == 1, "Repeated disposal released a module twice.");
            });
        }));
        tests.Add(("progress synchronization reuses one import and disposes DOM behavior exactly once", async () =>
        {
            var javascript = new DeferredJs();
            await WithProgress(javascript, async component => {
                await component.InteractiveRenderAsync(); await component.InteractiveRenderAsync();
                Require(javascript.Imports == 1 && javascript.Module.Synchronizations == 2, "A render replaced its owned module instead of idempotently synchronizing.");
                await component.DisposeAsync(); await component.InteractiveRenderAsync(); await component.DisposeAsync();
                Require(javascript.Module.Detaches == 1 && javascript.Module.Releases == 1 && javascript.Module.Synchronizations == 2, "Disposed navigation reattached or did not tear down exactly once.");
            });
        }));
    }
    private static async Task<string> Render<T>(Dictionary<string, object?> parameters) where T : IComponent
    {
        using var provider = new ServiceCollection().AddLogging().AddFlourishFramework().AddSingleton<IJSRuntime>(new NoJs()).BuildServiceProvider();
        await using var renderer = new HtmlRenderer(provider, provider.GetRequiredService<ILoggerFactory>());
        return await renderer.Dispatcher.InvokeAsync(async () => (await renderer.RenderComponentAsync<T>(ParameterView.FromDictionary(parameters))).ToHtmlString());
    }
    private static async Task WithProgress(DeferredJs javascript, Func<ProgressProbe, Task> verify)
    {
        var activator = new ProgressActivator();
        using var provider = new ServiceCollection().AddLogging().AddFlourishFramework()
            .AddSingleton<IJSRuntime>(javascript).AddSingleton<IComponentActivator>(activator).BuildServiceProvider();
        await using var renderer = new HtmlRenderer(provider, provider.GetRequiredService<ILoggerFactory>());
        await renderer.Dispatcher.InvokeAsync(async () => {
            await renderer.RenderComponentAsync<ProgressProbe>(ParameterView.FromDictionary(new Dictionary<string, object?> { ["ContentId"] = "scene-content" }));
            await verify(activator.Component ?? throw new InvalidOperationException("The actual progress component was not activated."));
        });
    }
    private sealed class ProgressProbe : PresentationProgress
    {
        public ProgressProbe() { }
        internal Task InteractiveRenderAsync() => base.OnAfterRenderAsync(false);
    }
    private sealed class ProgressActivator : IComponentActivator
    {
        internal ProgressProbe? Component { get; private set; }
        public IComponent CreateInstance(Type type)
        {
            var component = (IComponent)Activator.CreateInstance(type)!;
            if (component is ProgressProbe probe) Component = probe;
            return component;
        }
    }
    private sealed class NoJs : IJSRuntime
    {
        public ValueTask<TValue> InvokeAsync<TValue>(string identifier, object?[]? args) => throw new InvalidOperationException("SSR invoked JavaScript.");
        public ValueTask<TValue> InvokeAsync<TValue>(string identifier, CancellationToken cancellationToken, object?[]? args) => InvokeAsync<TValue>(identifier, args);
    }
    private sealed class DeferredJs : IJSRuntime
    {
        internal bool DelayImport { get; init; }
        internal int Imports { get; private set; }
        internal ModuleReference Module { get; } = new();
        internal TaskCompletionSource Started { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        private readonly TaskCompletionSource<IJSObjectReference> pending = new(TaskCreationOptions.RunContinuationsAsynchronously);
        internal void CompleteImport() => pending.TrySetResult(Module);
        public ValueTask<TValue> InvokeAsync<TValue>(string identifier, object?[]? args) => InvokeAsync<TValue>(identifier, CancellationToken.None, args);
        public ValueTask<TValue> InvokeAsync<TValue>(string identifier, CancellationToken cancellationToken, object?[]? args)
        {
            Require(identifier == "import" && args?[0]?.ToString()?.EndsWith("/presentation/progress.js", StringComparison.Ordinal) == true, "Progress imported an unrelated interaction module.");
            Imports++; Started.TrySetResult();
            return DelayImport ? new ValueTask<TValue>(CompleteAsync<TValue>()) : ValueTask.FromResult((TValue)(object)Module);
        }
        private async Task<TValue> CompleteAsync<TValue>() => (TValue)(object)await pending.Task;
    }
    private sealed class ModuleReference : IJSObjectReference
    {
        internal int Synchronizations { get; private set; }
        internal int Detaches { get; private set; }
        internal int Releases { get; private set; }
        public ValueTask<TValue> InvokeAsync<TValue>(string identifier, object?[]? args) => InvokeAsync<TValue>(identifier, CancellationToken.None, args);
        public ValueTask<TValue> InvokeAsync<TValue>(string identifier, CancellationToken cancellationToken, object?[]? args)
        {
            if (identifier == "synchronize") Synchronizations++;
            else if (identifier == "detach") Detaches++;
            else throw new InvalidOperationException("Unexpected progress method: " + identifier);
            return ValueTask.FromResult(default(TValue)!);
        }
        public ValueTask DisposeAsync() { Releases++; return ValueTask.CompletedTask; }
    }
    private static void Require(bool condition, string message) { if (!condition) throw new InvalidOperationException(message); }
}
