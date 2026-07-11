using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using ArkheideSystem.Flourish.Abstract;
using ArkheideSystem.Flourish.Configuration;
using Application = System.Windows.Application;
using FontFamily = System.Windows.Media.FontFamily;
using Window = System.Windows.Window;
using WpfControl = System.Windows.Controls.Control;

namespace ArkheideSystem.Flourish.Services;

internal sealed class FontService(FlourishShellOptions options) : IFontService
{
    private readonly object gate = new();
    private readonly ConditionalWeakTable<Page, PageFontResourceState> pageFontStates = new();
    private Window? owner;

    private static readonly string[] PageFontResourceKeys =
    [
        "FlourishFontFamily",
        "FlourishFontSizeSmall",
        "FlourishFontSizeCaption",
        "FlourishFontSizeBase",
        "FlourishFontSizeTitle",
        "FlourishFontSizeTitlebarIcon",
        "FlourishFontSizeNavigationIcon",
        "FlourishFontSizeWindowButtonIcon",
    ];

    public string FontFamily
    {
        get
        {
            lock (gate)
            {
                return options.FontFamily;
            }
        }
    }

    public string IconFontFamily
    {
        get
        {
            lock (gate)
            {
                return options.IconFontFamily;
            }
        }
    }

    public double FontSize
    {
        get
        {
            lock (gate)
            {
                return options.FontSize;
            }
        }
    }

    public IReadOnlyDictionary<Type, FlourishPageFontOverride> PageOverrides
    {
        get
        {
            lock (gate)
            {
                return new ReadOnlyDictionary<Type, FlourishPageFontOverride>(
                    new Dictionary<Type, FlourishPageFontOverride>(
                        options.PageFontOverridesByPageType
                    )
                );
            }
        }
    }

    public event EventHandler<FlourishFontChangedEventArgs>? Changed;

    public void Apply(Window window)
    {
        owner = window;
        ApplyCore(window);
    }

    public void SetFont(string fontFamily, double fontSize)
    {
        fontFamily = ValidateNotBlank(fontFamily, nameof(fontFamily));
        ValidatePositiveFinite(fontSize, nameof(fontSize));
        lock (gate)
        {
            if (options.FontFamily == fontFamily && options.FontSize == fontSize)
            {
                return;
            }

            options.FontFamily = fontFamily;
            options.FontSize = fontSize;
        }

        ApplyAndNotify();
    }

    public void SetFontFamily(string fontFamily)
    {
        fontFamily = ValidateNotBlank(fontFamily, nameof(fontFamily));
        lock (gate)
        {
            if (options.FontFamily == fontFamily)
            {
                return;
            }

            options.FontFamily = fontFamily;
        }

        ApplyAndNotify();
    }

    public void SetFontSize(double fontSize)
    {
        ValidatePositiveFinite(fontSize, nameof(fontSize));
        lock (gate)
        {
            if (options.FontSize == fontSize)
            {
                return;
            }

            options.FontSize = fontSize;
        }

        ApplyAndNotify();
    }

    public void SetIconFontFamily(string fontFamily)
    {
        fontFamily = ValidateNotBlank(fontFamily, nameof(fontFamily));
        lock (gate)
        {
            if (options.IconFontFamily == fontFamily)
            {
                return;
            }

            options.IconFontFamily = fontFamily;
        }

        ApplyAndNotify();
    }

    public void SetOverrideFont<TPage>(string fontFamily, double? fontSize = null)
        where TPage : Page
    {
        SetOverrideFont(typeof(TPage), fontFamily, fontSize);
    }

    public void SetOverrideFont(
        Type pageType,
        string fontFamily,
        double? fontSize = null
    )
    {
        ValidatePageType(pageType, nameof(pageType));
        fontFamily = ValidateNotBlank(fontFamily, nameof(fontFamily));
        if (fontSize is { } size)
        {
            ValidatePositiveFinite(size, nameof(fontSize));
        }

        var pageOverride = new FlourishPageFontOverride(fontFamily, fontSize);
        lock (gate)
        {
            if (
                options.PageFontOverridesByPageType.TryGetValue(pageType, out var current)
                && current.FontFamily == pageOverride.FontFamily
                && current.FontSize == pageOverride.FontSize
            )
            {
                return;
            }

            options.PageFontOverridesByPageType[pageType] = pageOverride;
        }

        RaiseChangedOnOwnerDispatcher();
    }

    public bool ClearOverrideFont<TPage>() where TPage : Page
    {
        return ClearOverrideFont(typeof(TPage));
    }

    public bool ClearOverrideFont(Type pageType)
    {
        ValidatePageType(pageType, nameof(pageType));
        lock (gate)
        {
            if (!options.PageFontOverridesByPageType.Remove(pageType))
            {
                return false;
            }
        }

        RaiseChangedOnOwnerDispatcher();
        return true;
    }

    internal void ApplyToPage(Page page, Type? configuredPageType = null)
    {
        ArgumentNullException.ThrowIfNull(page);
        var pageType = configuredPageType ?? page.GetType();
        FlourishPageFontOverride? pageOverride;
        lock (gate)
        {
            options.PageFontOverridesByPageType.TryGetValue(pageType, out pageOverride);
        }

        if (pageOverride is null)
        {
            RestorePageResources(page);
            page.SetResourceReference(WpfControl.FontFamilyProperty, "FlourishFontFamily");
            page.SetResourceReference(WpfControl.FontSizeProperty, "FlourishFontSizeBase");
            return;
        }

        var state = pageFontStates.GetValue(page, CapturePageResources);
        var fontFamily = new FontFamily(pageOverride.FontFamily);
        page.FontFamily = fontFamily;
        page.Resources["FlourishFontFamily"] = fontFamily;
        if (pageOverride.FontSize is { } fontSize)
        {
            page.FontSize = fontSize;
            SetPageFontSizeResources(page.Resources, fontSize);
        }
        else
        {
            RestorePageFontSizeResources(page, state);
            page.SetResourceReference(WpfControl.FontSizeProperty, "FlourishFontSizeBase");
        }
    }

    private void ApplyAndNotify()
    {
        var attachedOwner = owner;
        if (attachedOwner is not null)
        {
            if (attachedOwner.CheckAccess())
            {
                ApplyCore(attachedOwner);
                RaiseChanged();
            }
            else
            {
                attachedOwner.Dispatcher.Invoke(() =>
                {
                    ApplyCore(attachedOwner);
                    RaiseChanged();
                });
            }

            return;
        }

        RaiseChanged();
    }

    private void RaiseChanged()
    {
        Changed?.Invoke(
            this,
            new FlourishFontChangedEventArgs(FontFamily, IconFontFamily, FontSize)
        );
    }

    private void RaiseChangedOnOwnerDispatcher()
    {
        var attachedOwner = owner;
        if (attachedOwner is null || attachedOwner.CheckAccess())
        {
            RaiseChanged();
            return;
        }

        attachedOwner.Dispatcher.Invoke(RaiseChanged);
    }

    private void ApplyCore(Window window)
    {
        string textFontFamily;
        string iconFontFamilyName;
        double baseSize;
        lock (gate)
        {
            textFontFamily = options.FontFamily;
            iconFontFamilyName = options.IconFontFamily;
            baseSize = options.FontSize;
        }

        var fontFamily = new FontFamily(textFontFamily);
        var iconFontFamily = new FontFamily(iconFontFamilyName);

        window.FontFamily = fontFamily;
        window.FontSize = baseSize;

        SetResource(window, "FlourishFontFamily", fontFamily);
        SetResource(window, "FlourishIconFontFamily", iconFontFamily);
        SetResource(window, "FlourishFontSizeSmall", ClampFontSize(baseSize - 3));
        SetResource(window, "FlourishFontSizeCaption", ClampFontSize(baseSize - 1));
        SetResource(window, "FlourishFontSizeBase", baseSize);
        SetResource(window, "FlourishFontSizeTitle", baseSize);
        SetResource(window, "FlourishFontSizeTitlebarIcon", ClampFontSize(baseSize + 1));
        SetResource(window, "FlourishFontSizeNavigationIcon", ClampFontSize(baseSize + 1));
        SetResource(window, "FlourishFontSizeWindowButtonIcon", ClampFontSize(baseSize - 2));
    }

    private static void SetResource(Window window, string key, object value)
    {
        window.Resources[key] = value;
        Application.Current?.Resources[key] = value;
    }

    private static double ClampFontSize(double size)
    {
        return Math.Max(1, size);
    }

    private static PageFontResourceState CapturePageResources(Page page)
    {
        var state = new PageFontResourceState();
        foreach (var key in PageFontResourceKeys)
        {
            if (page.Resources.Contains(key))
            {
                state.OriginalResources[key] = page.Resources[key];
            }
            else
            {
                state.MissingResourceKeys.Add(key);
            }
        }

        return state;
    }

    private void RestorePageResources(Page page)
    {
        if (!pageFontStates.TryGetValue(page, out var state))
        {
            return;
        }

        foreach (var key in PageFontResourceKeys)
        {
            RestorePageResource(page, state, key);
        }

        pageFontStates.Remove(page);
    }

    private static void RestorePageFontSizeResources(
        Page page,
        PageFontResourceState state
    )
    {
        foreach (var key in PageFontResourceKeys.Skip(1))
        {
            RestorePageResource(page, state, key);
        }
    }

    private static void RestorePageResource(
        Page page,
        PageFontResourceState state,
        string key
    )
    {
        if (state.MissingResourceKeys.Contains(key))
        {
            page.Resources.Remove(key);
            return;
        }

        page.Resources[key] = state.OriginalResources[key];
    }

    private static void SetPageFontSizeResources(
        ResourceDictionary resources,
        double baseSize
    )
    {
        resources["FlourishFontSizeSmall"] = ClampFontSize(baseSize - 3);
        resources["FlourishFontSizeCaption"] = ClampFontSize(baseSize - 1);
        resources["FlourishFontSizeBase"] = baseSize;
        resources["FlourishFontSizeTitle"] = baseSize;
        resources["FlourishFontSizeTitlebarIcon"] = ClampFontSize(baseSize + 1);
        resources["FlourishFontSizeNavigationIcon"] = ClampFontSize(baseSize + 1);
        resources["FlourishFontSizeWindowButtonIcon"] = ClampFontSize(baseSize - 2);
    }

    private static string ValidateNotBlank(string value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Value cannot be empty.", parameterName);
        }

        return value;
    }

    private static void ValidatePositiveFinite(double value, string parameterName)
    {
        if (double.IsNaN(value) || double.IsInfinity(value) || value <= 0)
        {
            throw new ArgumentOutOfRangeException(
                parameterName,
                value,
                "Value must be a positive finite number."
            );
        }
    }

    private static void ValidatePageType(Type pageType, string parameterName)
    {
        ArgumentNullException.ThrowIfNull(pageType, parameterName);
        if (
            !typeof(Page).IsAssignableFrom(pageType)
            || pageType.IsAbstract
            || pageType.ContainsGenericParameters
        )
        {
            throw new ArgumentException(
                $"{pageType.FullName} must be a closed, concrete System.Windows.Controls.Page type.",
                parameterName
            );
        }
    }

    private sealed class PageFontResourceState
    {
        public Dictionary<string, object?> OriginalResources { get; } = [];

        public HashSet<string> MissingResourceKeys { get; } = [];
    }
}
