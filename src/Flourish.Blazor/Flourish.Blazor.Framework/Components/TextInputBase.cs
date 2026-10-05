using ArkheideSystem.Flourish.Blazor.Abstract;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace ArkheideSystem.Flourish.Blazor.Components;

/// <summary>Provides scoped default validation text while retaining InputBase form behavior.</summary>
public abstract class TextInputBase<TValue> : InputBase<TValue>
{
    private ITextProvider? subscribed;
    private bool disposed;
    private HashSet<string> supplied = new(StringComparer.Ordinal);

    [Inject] protected ITextProvider Texts { get; set; } = null!;

    public override Task SetParametersAsync(ParameterView parameters)
    {
        supplied = new(StringComparer.Ordinal);
        foreach (var parameter in parameters) supplied.Add(parameter.Name);
        return base.SetParametersAsync(parameters);
    }

    protected string DefaultText(string parameter, string literal, string token, string fallback) =>
        supplied.Contains(parameter) ? literal : Texts.Get(new TextReference("Flourish", token, fallback));

    protected override void OnInitialized()
    {
        base.OnInitialized();
        subscribed = Texts;
        subscribed.Changed += HandleChanged;
    }

    private void HandleChanged(object? sender, EventArgs args)
    {
        if (disposed) return;
        var refresh = InvokeAsync(() => { if (!disposed) StateHasChanged(); });
        if (!refresh.IsCompletedSuccessfully) _ = ObserveAsync(refresh);
    }

    private async Task ObserveAsync(Task refresh)
    {
        try { await refresh; }
        catch (Exception error) { if (!disposed) await DispatchExceptionAsync(error); }
    }

    protected override void Dispose(bool disposing)
    {
        disposed = true;
        if (subscribed is not null) subscribed.Changed -= HandleChanged;
        subscribed = null;
        base.Dispose(disposing);
    }
}
