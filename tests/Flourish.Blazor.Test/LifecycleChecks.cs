using ArkheideSystem.Flourish.Blazor;
using ArkheideSystem.Flourish.Blazor.Components.Primitives;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;

internal static class LifecycleChecks
{
    internal static void Register(List<(string Name, Func<Task> Run)> tests)
    {
        tests.Add(("unused sheets avoid mounting form contents and importing browser behavior", async () =>
        {
            await WithSheet(async (sheet, javascript, html) =>
            {
                Require(!html().Contains("Draft field", StringComparison.Ordinal), "An unopened sheet mounted its form content.");
                await sheet.InteractiveRenderAsync();
                await sheet.InteractiveRenderAsync();
                Require(javascript.Imports == 0 && javascript.Module.States.Count == 0, "A closed sheet initialized browser listeners.");
                await sheet.OpenAsync(true);
                Require(html().Contains("Draft field", StringComparison.Ordinal), "First opening did not render the form.");
                await sheet.InteractiveRenderAsync();
                await sheet.InteractiveRenderAsync();
                Require(javascript.Imports == 1 && javascript.Module.States.SequenceEqual([true]), "Opening imported repeatedly or repeated identical synchronization.");
                await sheet.OpenAsync(false);
                await sheet.InteractiveRenderAsync();
                Require(html().Contains("Draft field", StringComparison.Ordinal), "Closing discarded an already used form's component state.");
                await sheet.OpenAsync(true);
                await sheet.InteractiveRenderAsync();
                Require(javascript.Imports == 1 && javascript.Module.States.SequenceEqual([true, false, true]), "Close and reopen lost synchronization or created another bridge.");
            });
        }));
        tests.Add(("sheet disposal releases initialized listeners while unused sheets require no interop", async () =>
        {
            await WithSheet(async (sheet, javascript, _) =>
            {
                await sheet.DisposeAsync();
                await sheet.InteractiveRenderAsync();
                Require(javascript.Imports == 0 && javascript.Module.DisposeCalls == 0, "An unused disposed sheet touched JavaScript.");
            });
            await WithSheet(async (sheet, javascript, _) =>
            {
                await sheet.OpenAsync(true);
                await sheet.InteractiveRenderAsync();
                await sheet.DisposeAsync();
                Require(javascript.Module.DisposeCalls == 1 && javascript.Module.Released, "An initialized sheet retained DOM listeners or its module proxy.");
            });
        }));
    }

    private static async Task WithSheet(Func<SheetProbe, CountingJs, Func<string>, Task> verify)
    {
        var javascript = new CountingJs();
        var activator = new ProbeActivator();
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
            var output = await renderer.RenderComponentAsync<SheetProbe>(ParameterView.FromDictionary(new Dictionary<string, object?>
            {
                [nameof(BottomSheet.Id)] = "lazy-sheet", [nameof(BottomSheet.Title)] = "Generic form",
                [nameof(BottomSheet.ChildContent)] = (RenderFragment)(builder => builder.AddContent(0, "Draft field"))
            }));
            await verify(activator.Sheet ?? throw new InvalidOperationException("Sheet was not constructed."), javascript, output.ToHtmlString);
        });
    }

    private static void Require(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
    }
    private sealed class ProbeActivator : IComponentActivator
    {
        internal SheetProbe? Sheet { get; private set; }
        public IComponent CreateInstance(Type componentType)
        {
            var component = (IComponent)Activator.CreateInstance(componentType)!;
            if (component is SheetProbe sheet) Sheet = sheet;
            return component;
        }
    }
    private sealed class SheetProbe : BottomSheet
    {
        public SheetProbe() { }
        internal Task InteractiveRenderAsync() => base.OnAfterRenderAsync(false);
        internal Task OpenAsync(bool open)
        {
            IsOpen = open;
            base.OnParametersSet();
            StateHasChanged();
            return Task.CompletedTask;
        }
    }
    private sealed class CountingJs : IJSRuntime
    {
        internal int Imports { get; private set; }
        internal CountingModule Module { get; } = new();
        public ValueTask<TValue> InvokeAsync<TValue>(string identifier, object?[]? args) => InvokeAsync<TValue>(identifier, CancellationToken.None, args);
        public ValueTask<TValue> InvokeAsync<TValue>(string identifier, CancellationToken cancellationToken, object?[]? args)
        {
            if (identifier != "import") throw new InvalidOperationException("Unexpected JS entry: " + identifier);
            Imports++;
            return ValueTask.FromResult((TValue)(object)Module);
        }
    }
    private sealed class CountingModule : IJSObjectReference
    {
        internal List<bool> States { get; } = [];
        internal int DisposeCalls { get; private set; }
        internal bool Released { get; private set; }
        public ValueTask<TValue> InvokeAsync<TValue>(string identifier, object?[]? args) => InvokeAsync<TValue>(identifier, CancellationToken.None, args);
        public ValueTask<TValue> InvokeAsync<TValue>(string identifier, CancellationToken cancellationToken, object?[]? args)
        {
            if (identifier == "synchronize") States.Add((bool)args![1]!);
            else if (identifier == "dispose") DisposeCalls++;
            else throw new InvalidOperationException("Unexpected module call: " + identifier);
            return ValueTask.FromResult(default(TValue)!);
        }
        public ValueTask DisposeAsync() { Released = true; return ValueTask.CompletedTask; }
    }
}
