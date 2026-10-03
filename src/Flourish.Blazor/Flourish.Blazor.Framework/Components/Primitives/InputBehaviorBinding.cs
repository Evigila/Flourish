using Microsoft.JSInterop;

namespace ArkheideSystem.Flourish.Blazor.Components.Primitives;

/// <summary>Loads document-level native input behavior once through the browser module cache.</summary>
internal sealed class InputBehaviorBinding(IJSRuntime javascript) : IAsyncDisposable
{
    private IJSObjectReference? module;
    private bool disposed;

    public async Task EnsureAsync()
    {
        if (disposed || module is not null) return;
        try
        {
            var imported = await javascript.InvokeAsync<IJSObjectReference>("import",
                "./_content/Arkheide.Flourish.Blazor.Framework/primitives/input-behaviors.js");
            if (disposed) { await imported.DisposeAsync(); return; }
            module = imported;
        }
        catch (Exception exception) when (exception is JSDisconnectedException or TaskCanceledException)
        {
            // The component can leave while its interactive rendering initializes.
        }
    }

    public async ValueTask DisposeAsync()
    {
        disposed = true;
        if (module is null) return;
        try { await module.DisposeAsync(); }
        catch (Exception exception) when (exception is JSDisconnectedException or TaskCanceledException) { }
    }
}