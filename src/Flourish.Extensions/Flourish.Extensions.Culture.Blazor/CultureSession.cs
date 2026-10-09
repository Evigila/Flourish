using System.Globalization;
using ArkheideSystem.Essential.Culture.Blazor;
using ArkheideSystem.Flourish.Blazor.Abstract;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace ArkheideSystem.Flourish.Extensions.Culture.Blazor;

/// <summary>Resolves scoped text and persists UI and formatting selections for the current browser.</summary>
public sealed class CultureSession : ITextProvider, IAsyncDisposable
{
    private readonly ILocalizationService localization;
    private readonly IJSRuntime javascript;
    private readonly NavigationManager navigation;
    private readonly CultureOptions options;
    private readonly SemaphoreSlim gate = new(1, 1);
    private IJSObjectReference? module;
    private bool disposed;

    internal CultureSession(ILocalizationService localization, IJSRuntime javascript, NavigationManager navigation, CultureOptions options)
    {
        this.localization = localization;
        this.javascript = javascript;
        this.navigation = navigation;
        this.options = options;
    }

    /// <summary>Gets the current UI culture.</summary>
    public string Culture => localization.Culture;
    /// <summary>Gets the current culture used by composite formatting.</summary>
    public CultureInfo FormatCulture => localization.FormatCulture;
    /// <summary>Gets the configured UI and formatting choices.</summary>
    public IReadOnlyList<string> AvailableCultures => options.SupportedCultures;
    /// <summary>Occurs when this scope's selection changes.</summary>
    public event EventHandler? Changed
    {
        add => localization.Changed += value;
        remove => localization.Changed -= value;
    }

    /// <summary>Resolves and optionally formats a token from the application's default catalog.</summary>
    public string Parse(string token, params object?[] arguments) => localization.Parse(token, arguments);
    /// <summary>Attempts to resolve a token from the application's default catalog.</summary>
    public bool TryParse(string token, out string value) => localization.TryParse(token, out value);
    /// <summary>Attempts to resolve and format a token from the application's default catalog.</summary>
    public bool TryParse(string token, object?[] arguments, out string value) => localization.TryParse(token, arguments, out value);

    string ITextProvider.Get(TextReference text, params object?[] arguments)
    {
        ArgumentNullException.ThrowIfNull(text);
        ArgumentNullException.ThrowIfNull(arguments);
        if (localization.TryParseFrom(text.CatalogId, text.Token, arguments, out var value)) return value;
        var fallback = text.FallbackText ?? text.Token;
        return arguments.Length == 0 ? fallback : string.Format(FormatCulture, fallback, arguments);
    }

    /// <summary>
    /// Persists a supported UI/formatting pair, then applies it to this scope. An omitted format follows the UI culture.
    /// Returns false for browser storage, cancellation or connection failures without applying the selection.
    /// </summary>
    public Task<bool> SelectAsync(string culture, string? formatCulture = null)
    {
        var ui = RequireSupported(culture);
        var format = RequireSupported(formatCulture ?? culture);
        return ApplySelectionAsync(ui, format);
    }

    /// <summary>Persists a formatting culture while retaining the latest UI selection after any queued transaction.</summary>
    public Task<bool> SelectFormatAsync(string formatCulture) => ApplySelectionAsync(null, RequireSupported(formatCulture));

    private async Task<bool> ApplySelectionAsync(string? ui, string format)
    {
        await gate.WaitAsync();
        try
        {
            ObjectDisposedException.ThrowIf(disposed, this);
            ui ??= Culture;
            try
            {
                module ??= await javascript.InvokeAsync<IJSObjectReference>("import", "./_content/Arkheide.Flourish.Extensions.Culture.Blazor/browser-preferences.js");
                await module.InvokeVoidAsync("saveCulture", ui, format, new Uri(navigation.BaseUri).AbsolutePath, options.RetentionDays * 86400);
            }
            catch (Exception error) when (error is JSException or JSDisconnectedException or OperationCanceledException)
            {
                return false;
            }
            localization.SetCulture(ui, format);
            return true;
        }
        finally { gate.Release(); }
    }

    private string RequireSupported(string culture)
    {
        var name = CultureBuilder.Normalize(culture);
        return options.SupportedCultures.Contains(name, StringComparer.OrdinalIgnoreCase) ? name
            : throw new ArgumentException($"Culture '{name}' is not included in the supported cultures.", nameof(culture));
    }

    /// <summary>Releases the optional browser module after queued selection transactions finish.</summary>
    public async ValueTask DisposeAsync()
    {
        await gate.WaitAsync();
        try
        {
            if (disposed) return;
            disposed = true;
            if (module is not null)
            {
                try { await module.DisposeAsync(); }
                catch (Exception error) when (error is JSDisconnectedException or OperationCanceledException) { }
            }
        }
        finally { gate.Release(); }
    }
}
