using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.JSInterop;

namespace ArkheideSystem.Flourish.Blazor.Components.Internal;

/// <summary>Internal menu enhancement; the public input remains the native form/change carrier.</summary>
internal sealed class NativeSelectPopup : TextComponentBase, IAsyncDisposable
{
    [Inject] public IJSRuntime JS { get; set; } = default!;
    [Parameter] public RenderFragment? ChildContent { get; set; }
    [Parameter] public string? Label { get; set; }
    [Parameter] public string? Icon { get; set; }
    [Parameter] public ButtonVariant? TriggerVariant { get; set; }
    [Parameter] public PopupStrength Strength { get; set; }
    [Parameter] public bool Disabled { get; set; }
    [Parameter] public string? Style { get; set; }
    private ElementReference root;
    private IJSObjectReference? module;
    private Task<IJSObjectReference?>? moduleLoading;
    private bool disposed;

    public NativeSelectPopup() { }

    internal static RenderFragment Render(RenderFragment nativeControl, string? label, string? icon,
        ButtonVariant? variant, PopupStrength strength, bool disabled, string? style) => builder =>
    {
        builder.OpenComponent<NativeSelectPopup>(0);
        builder.AddAttribute(1, nameof(ChildContent), nativeControl);
        builder.AddAttribute(2, nameof(Label), label);
        builder.AddAttribute(3, nameof(Icon), icon);
        builder.AddAttribute(4, nameof(TriggerVariant), variant);
        builder.AddAttribute(5, nameof(Strength), strength);
        builder.AddAttribute(6, nameof(Disabled), disabled);
        builder.AddAttribute(7, nameof(Style), style);
        builder.CloseComponent();
    };

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenElement(0, "span");
        builder.AddAttribute(1, "class", "f-native-select-popup" + (TriggerVariant is not null || !string.IsNullOrWhiteSpace(Icon) ? " f-native-select-button" : ""));
        builder.AddAttribute(2, "style", Style);
        builder.AddAttribute(3, "data-f-native-select-popup", "");
        builder.AddElementReferenceCapture(4, value => root = value);
        builder.AddContent(5, ChildContent);
        builder.OpenElement(6, "span");
        builder.AddAttribute(7, "class", "f-native-select-actions");
        builder.OpenComponent<ActionMenu>(8);
        builder.AddAttribute(9, nameof(ActionMenu.Label), Label ?? Ui("Selection_NoneSelected", "None selected"));
        builder.AddAttribute(10, nameof(ActionMenu.Icon), Icon);
        builder.AddAttribute(11, nameof(ActionMenu.TriggerVariant), TriggerVariant);
        if (string.IsNullOrWhiteSpace(Icon)) builder.AddAttribute(12, nameof(ActionMenu.TriggerContent), (RenderFragment)(caption =>
        {
            caption.OpenElement(0, "span"); caption.AddAttribute(1, "data-f-native-select-caption", ""); caption.CloseElement();
        }));
        builder.AddAttribute(13, nameof(ActionMenu.OpenOnHover), true);
        builder.AddAttribute(14, nameof(ActionMenu.Strength), Strength);
        builder.AddAttribute(15, nameof(ActionMenu.Disabled), Disabled);
        builder.AddAttribute(16, nameof(ActionMenu.ChildContent), (RenderFragment)(options =>
        {
            options.OpenElement(0, "div"); options.AddAttribute(1, "data-f-native-select-options", ""); options.CloseElement();
        }));
        builder.CloseComponent();
        builder.CloseElement();
        builder.CloseElement();
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (disposed) return;
        try
        {
            var activeModule = await GetModuleAsync();
            if (!disposed && activeModule is not null) await activeModule.InvokeVoidAsync("initializePopups", root);
        }
        catch (Exception error) when (error is JSDisconnectedException or TaskCanceledException) { }
    }

    private async Task<IJSObjectReference?> GetModuleAsync()
    {
        if (module is not null) return module;
        var loading = moduleLoading ??= ImportModuleAsync();
        try { return await loading; }
        finally { if (ReferenceEquals(moduleLoading, loading)) moduleLoading = null; }
    }
    private async Task<IJSObjectReference?> ImportModuleAsync()
    {
        var imported = await JS.InvokeAsync<IJSObjectReference>("import", "./_content/Arkheide.Flourish.Blazor.Framework/controls.js");
        if (disposed) { await ReleaseModuleAsync(imported); return null; }
        module = imported;
        return imported;
    }
    public async ValueTask DisposeAsync()
    {
        if (disposed) return;
        disposed = true;
        base.Dispose();
        var activeModule = module;
        module = null;
        if (activeModule is null) return;
        try { await activeModule.InvokeVoidAsync("detachNativeSelectPopup", root); }
        catch (Exception error) when (error is JSDisconnectedException or TaskCanceledException) { }
        finally { await ReleaseModuleAsync(activeModule); }
    }
    private static async ValueTask ReleaseModuleAsync(IJSObjectReference activeModule)
    {
        try { await activeModule.DisposeAsync(); }
        catch (Exception error) when (error is JSDisconnectedException or TaskCanceledException) { }
    }
}
