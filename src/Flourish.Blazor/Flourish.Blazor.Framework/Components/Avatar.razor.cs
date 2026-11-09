using System.Globalization;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace ArkheideSystem.Flourish.Blazor.Components;

public partial class Avatar : IAsyncDisposable
{
    private const int MaxOutputBytes = 256 * 1024;
    private const string EditorModulePath = "./_content/Arkheide.Flourish.Blazor.Framework/avatar-editor.js";

    [Inject] private IJSRuntime JS { get; set; } = null!;

    [Parameter] public string? Url { get; set; }
    [Parameter] public string Name { get; set; } = string.Empty;
    [Parameter] public string? Description { get; set; }
    [Parameter] public string? HelpText { get; set; }
    [Parameter] public int Size { get; set; } = 64;
    [Parameter] public bool Decorative { get; set; }
    [Parameter] public bool Disabled { get; set; }
    /// <summary>Enables editing. Return true after saving the JPEG data URL or false to keep the crop open for retry.</summary>
    [Parameter] public Func<string, Task<bool>>? SaveAsync { get; set; }
    [Parameter] public string? Class { get; set; }
    [Parameter(CaptureUnmatchedValues = true)] public IReadOnlyDictionary<string, object>? AdditionalAttributes { get; set; }

    private readonly string ControlId = "f-avatar-" + Guid.NewGuid().ToString("N");
    private readonly CancellationTokenSource Lifetime = new();
    private ElementReference Owner;
    private ElementReference Picker;
    private ElementReference Editor;
    private IJSObjectReference? Module;
    private string? PreviousUrl;
    private string? ImageUrl;
    private string? FailedUrl;
    private string? PickerError;
    private string? CropError;
    private int CropVersion;
    private bool Loading;
    private bool CropOpen;
    private bool AttachCrop;
    private bool CropReady;
    private bool Saving;
    private bool Disposed;
    private bool HelpExpanded;
    private bool HelpDismissed;

    private bool HasHelp => !string.IsNullOrWhiteSpace(HelpText);
    private string HelpId => ControlId + "-help";

    private void ToggleHelp()
    {
        HelpExpanded = !HelpExpanded;
        HelpDismissed = !HelpExpanded;
    }

    private void ResetHelp()
    {
        HelpExpanded = false;
        HelpDismissed = false;
    }

    private void HandleHelpKey(Microsoft.AspNetCore.Components.Web.KeyboardEventArgs args)
    {
        if (args.Key != "Escape") return;
        HelpExpanded = false;
        HelpDismissed = true;
    }

    private string AvatarStyle => FormattableString.Invariant($"--f-avatar-size:{Size}px");
    private string AccessibleName => string.IsNullOrWhiteSpace(Name) ? Ui("Avatar_Account", "Account") : Name.Trim();

    private string Initials
    {
        get
        {
            var words = (Name ?? string.Empty).Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
            if (words.Length == 0) return string.Empty;
            var first = StringInfo.GetNextTextElement(words[0]);
            return (words.Length > 1 ? first + StringInfo.GetNextTextElement(words[^1]) : first).ToUpperInvariant();
        }
    }

    protected override void OnParametersSet()
    {
        base.OnParametersSet();
        if (Size is < 16 or > 512)
            throw new ArgumentOutOfRangeException(nameof(Size), "Avatar size must be between 16 and 512 pixels.");
        if (string.Equals(PreviousUrl, Url, StringComparison.Ordinal)) return;
        PreviousUrl = Url;
        ImageUrl = SafeImageUrl(Url);
        FailedUrl = null;
    }

    private void ImageFailed(string source)
    {
        if (string.Equals(source, ImageUrl, StringComparison.Ordinal)) FailedUrl = source;
    }

    private async Task ChoosePhotoAsync()
    {
        if (Disposed || Disabled || SaveAsync is null || Loading || CropOpen || Saving) return;
        Loading = true;
        PickerError = null;
        var version = ++CropVersion;
        try
        {
            var module = Module ?? await JS.InvokeAsync<IJSObjectReference>("import", Lifetime.Token, EditorModulePath);
            if (Disposed)
            {
                await module.DisposeAsync();
                return;
            }

            Module = module;
            var result = await module.InvokeAsync<string>("load", Lifetime.Token, Owner, Picker, version);
            if (Disposed) return;
            if (result == "ready" && !Disabled && SaveAsync is not null)
            {
                CropError = null;
                AttachCrop = true;
                CropOpen = true;
                return;
            }

            await ReleaseCropAsync(version);
            PickerError = result switch
            {
                "too-large" => "size",
                "invalid" => "image",
                _ => null
            };
        }
        catch (Exception error) when (error is JSException or OperationCanceledException)
        {
            await ReleaseCropAsync(version);
            if (!Disposed) PickerError = "image";
        }
        finally
        {
            Loading = false;
        }
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (!CropOpen || !AttachCrop || Module is not { } module || Disposed) return;
        AttachCrop = false;
        var version = CropVersion;
        var ready = false;
        try
        {
            ready = await module.InvokeAsync<bool>("attach", Lifetime.Token, Owner, Editor, version);
        }
        catch (Exception error) when (error is JSException or OperationCanceledException) { }

        if (Disposed || !CropOpen || version != CropVersion) return;
        CropReady = ready;
        if (!ready) CropError = "crop";
        StateHasChanged();
    }

    private async Task SaveCropAsync()
    {
        if (Disposed || Disabled || Saving || !CropReady || !CropOpen || Module is not { } module || SaveAsync is not { } save)
            return;
        Saving = true;
        CropError = null;
        try
        {
            string? photo;
            try
            {
                photo = await EncodeCropAsync(module);
            }
            catch (Exception error) when (error is JSException or OperationCanceledException or IOException or InvalidOperationException)
            {
                photo = null;
            }

            if (Disposed) return;
            if (photo is null)
            {
                CropError = "crop";
                return;
            }

            if (Disabled || SaveAsync is null) return;
            if (await save(photo))
            {
                if (!Disposed) await CloseCropAsync();
            }
            else if (!Disposed) CropError = "save";
        }
        finally
        {
            Saving = false;
        }
    }

    private async Task<string?> EncodeCropAsync(IJSObjectReference module)
    {
        await using var jpeg = await module.InvokeAsync<IJSStreamReference>("encode", Lifetime.Token, Owner, CropVersion);
        if (jpeg.Length is <= 0 or > MaxOutputBytes) return null;
        await using var stream = await jpeg.OpenReadStreamAsync(MaxOutputBytes, Lifetime.Token);
        var bytes = new byte[(int)jpeg.Length];
        await stream.ReadExactlyAsync(bytes, Lifetime.Token);
        if (bytes.Length < 4 || bytes[0] != 0xff || bytes[1] != 0xd8 || bytes[^2] != 0xff || bytes[^1] != 0xd9)
            return null;
        return "data:image/jpeg;base64," + Convert.ToBase64String(bytes);
    }

    private Task CropVisibilityChangedAsync(bool open) => open ? Task.CompletedTask : CloseCropAsync();

    private async Task CloseCropAsync()
    {
        CropOpen = false;
        CropReady = false;
        AttachCrop = false;
        CropError = null;
        await ReleaseCropAsync(CropVersion);
    }

    private async Task ReleaseCropAsync(int version)
    {
        if (Module is not { } module) return;
        try { await module.InvokeVoidAsync("release", Owner, version); }
        catch (Exception error) when (error is JSException or OperationCanceledException or ObjectDisposedException) { }
    }

    private string ErrorText(string error) => error switch
    {
        "size" => Ui("Avatar_TooLarge", "Choose a photo up to 12 MiB, 8,192 pixels per side, and 24 megapixels."),
        "crop" => Ui("Avatar_CropFailed", "The photo could not be cropped. Try again or choose a different photo."),
        "save" => Ui("Avatar_SaveFailed", "The photo could not be saved. Try again."),
        _ => Ui("Avatar_InvalidImage", "Choose a valid JPEG, PNG, or WebP photo.")
    };

    private static string? SafeImageUrl(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return null;
        value = value.Trim();
        if (value.Any(char.IsControl) || value.Contains('\\')) return null;
        if (value.StartsWith("data:", StringComparison.OrdinalIgnoreCase))
            return new[] { "data:image/png;base64,", "data:image/jpeg;base64,", "data:image/gif;base64,", "data:image/webp;base64," }
                .Any(prefix => value.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)) ? value : null;
        if (!Uri.TryCreate(value, UriKind.RelativeOrAbsolute, out var uri)) return null;
        return !uri.IsAbsoluteUri || uri.Scheme is "https" or "http" ? value : null;
    }

    public async ValueTask DisposeAsync()
    {
        if (Disposed) return;
        Disposed = true;
        Lifetime.Cancel();
        base.Dispose();
        await ReleaseCropAsync(CropVersion);
        if (Module is { } module)
        {
            try { await module.DisposeAsync(); }
            catch (Exception error) when (error is JSException or OperationCanceledException) { }
        }

        Lifetime.Dispose();
    }
}
