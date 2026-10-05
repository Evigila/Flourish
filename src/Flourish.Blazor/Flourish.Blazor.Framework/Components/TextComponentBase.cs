using ArkheideSystem.Flourish.Blazor.Abstract;
using Microsoft.AspNetCore.Components;

namespace ArkheideSystem.Flourish.Blazor.Components;

/// <summary>Dispatches scoped text changes to a renderer and preserves explicit text parameters.</summary>
public abstract class TextComponentBase : ComponentBase, IDisposable
{
    private ITextProvider? subscribed;
    private volatile bool disposed;
    private HashSet<string> supplied = new(StringComparer.Ordinal);

    [Inject] protected ITextProvider Texts { get; set; } = null!;

    public override Task SetParametersAsync(ParameterView parameters)
    {
        supplied = new(StringComparer.Ordinal);
        foreach (var parameter in parameters) supplied.Add(parameter.Name);
        return base.SetParametersAsync(parameters);
    }

    protected bool HasExplicitParameter(string name) => supplied.Contains(name);

    protected string DefaultText(string parameter, string literal, string token, string fallback) =>
        HasExplicitParameter(parameter) ? literal : Texts.Get(new TextReference("Flourish", token, fallback));

    protected override void OnInitialized()
    {
        base.OnInitialized();
        subscribed = Texts;
        subscribed.Changed += HandleChanged;
    }

    protected string Ui(string token, string fallback, params object?[] args) =>
        Texts.Get(new TextReference("Flourish", token, fallback), args);

    protected virtual void OnTextChanged() { }

    private void HandleChanged(object? sender, EventArgs args)
    {
        if (disposed) return;
        var refresh = InvokeAsync(() =>
        {
            if (disposed) return;
            OnTextChanged();
            StateHasChanged();
        });
        if (!refresh.IsCompletedSuccessfully) _ = ObserveAsync(refresh);
    }

    private async Task ObserveAsync(Task refresh)
    {
        try { await refresh; }
        catch (Exception error) { if (!disposed) await DispatchExceptionAsync(error); }
    }

    public virtual void Dispose()
    {
        if (disposed) return;
        disposed = true;
        if (subscribed is not null) subscribed.Changed -= HandleChanged;
        subscribed = null;
        GC.SuppressFinalize(this);
    }
}
