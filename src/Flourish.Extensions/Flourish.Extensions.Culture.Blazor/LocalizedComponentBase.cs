using Microsoft.AspNetCore.Components;

namespace ArkheideSystem.Flourish.Extensions.Culture.Blazor;

/// <summary>Refreshes a component when the current browser's culture selection changes.</summary>
public abstract class LocalizedComponentBase : ComponentBase, IDisposable
{
    private CultureSession? subscribed;

    /// <summary>Gets the scoped text and personal culture selection service.</summary>
    [Inject] protected CultureSession Localization { get; set; } = default!;

    /// <inheritdoc />
    protected override void OnInitialized()
    {
        base.OnInitialized();
        subscribed = Localization;
        subscribed.Changed += CultureChanged;
    }

    private void CultureChanged(object? sender, EventArgs arguments)
    {
        if (subscribed is not null) _ = InvokeAsync(() =>
        {
            if (subscribed is not null) StateHasChanged();
        });
    }

    /// <summary>Releases the subscription acquired during component initialization.</summary>
    public virtual void Dispose()
    {
        if (subscribed is null) return;
        subscribed.Changed -= CultureChanged;
        subscribed = null;
        GC.SuppressFinalize(this);
    }
}
