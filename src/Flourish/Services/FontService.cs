using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using ArkheideSystem.Flourish.Abstract;
using ArkheideSystem.Flourish.Internal.Configuration;
using Application = System.Windows.Application;
using FontFamily = System.Windows.Media.FontFamily;
using WpfControl = System.Windows.Controls.Control;

namespace ArkheideSystem.Flourish.Services;

internal sealed class FontService : IFontService
{
    private readonly object gate = new();
    private readonly FlourishShellOptions options;
    private readonly ConditionalWeakTable<Page, PageFontResourceState> pageFontStates = new();
    private Dispatcher? applicationDispatcher;
    private ResourceDictionary? applicationResources;
    private ResourceDictionary? appliedResources;
    private FontSnapshot? appliedSnapshot;

    private static readonly string[] PageFontResourceKeys =
    [
        "FlourishFontFamily",
        "FlourishFontSizeSmall",
        "FlourishFontSizeCaption",
        "FlourishFontSizeDescription",
        "FlourishFontSizeBase",
        "FlourishFontSizeSubtitle",
        "FlourishFontSizeSectionTitle",
        "FlourishFontSizePageTitle",
        "FlourishFontSizeTitle",
        "FlourishFontSizeTitlebarIcon",
        "FlourishFontSizeNavigationIcon",
        "FlourishFontSizeWindowButtonIcon",
        "FlourishLineHeightBody",
        "FlourishLineHeightDescription",
        "FlourishLineHeightSubtitle",
    ];

    public FontService(FlourishShellOptions options)
    {
        this.options = options ?? throw new ArgumentNullException(nameof(options));
    }

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

    internal void Attach(Application application)
    {
        ArgumentNullException.ThrowIfNull(application);
        Attach(application.Dispatcher, application.Resources);
    }

    internal void Attach(Dispatcher dispatcher, ResourceDictionary resources)
    {
        ArgumentNullException.ThrowIfNull(dispatcher);
        ArgumentNullException.ThrowIfNull(resources);

        void AttachCore()
        {
            FontSnapshot snapshot;
            lock (gate)
            {
                applicationDispatcher = dispatcher;
                applicationResources = resources;
                snapshot = CaptureSnapshot();
            }

            if (ReferenceEquals(appliedResources, resources) && appliedSnapshot == snapshot)
            {
                return;
            }

            ApplyResources(resources, snapshot, FontResourceChange.All);
            appliedResources = resources;
            appliedSnapshot = snapshot;
        }

        if (dispatcher.CheckAccess())
        {
            AttachCore();
        }
        else
        {
            dispatcher.Invoke(AttachCore);
        }
    }

    public void SetFont(string fontFamily, double fontSize)
    {
        fontFamily = ValidateNotBlank(fontFamily, nameof(fontFamily));
        ValidatePositiveFinite(fontSize, nameof(fontSize));
        ExecuteMutation(() => SetFontCore(fontFamily, fontSize));
    }

    private FontMutation? SetFontCore(string fontFamily, double fontSize)
    {
        var change = FontResourceChange.None;
        if (options.FontFamily != fontFamily)
        {
            options.FontFamily = fontFamily;
            change |= FontResourceChange.TextFamily;
        }

        if (options.FontSize != fontSize)
        {
            options.FontSize = fontSize;
            change |= FontResourceChange.Sizes;
        }

        return change == FontResourceChange.None
            ? null
            : new FontMutation(
                CaptureSnapshot(),
                change,
                FlourishFontChangeKind.GlobalText,
                null
            );
    }

    public void SetFontFamily(string fontFamily)
    {
        fontFamily = ValidateNotBlank(fontFamily, nameof(fontFamily));
        ExecuteMutation(() => SetFontFamilyCore(fontFamily));
    }

    private FontMutation? SetFontFamilyCore(string fontFamily)
    {
        if (options.FontFamily == fontFamily)
        {
            return null;
        }

        options.FontFamily = fontFamily;
        return new FontMutation(
            CaptureSnapshot(),
            FontResourceChange.TextFamily,
            FlourishFontChangeKind.GlobalText,
            null
        );
    }

    public void SetFontSize(double fontSize)
    {
        ValidatePositiveFinite(fontSize, nameof(fontSize));
        ExecuteMutation(() => SetFontSizeCore(fontSize));
    }

    private FontMutation? SetFontSizeCore(double fontSize)
    {
        if (options.FontSize == fontSize)
        {
            return null;
        }

        options.FontSize = fontSize;
        return new FontMutation(
            CaptureSnapshot(),
            FontResourceChange.Sizes,
            FlourishFontChangeKind.GlobalText,
            null
        );
    }

    public void SetIconFontFamily(string fontFamily)
    {
        fontFamily = ValidateNotBlank(fontFamily, nameof(fontFamily));
        ExecuteMutation(() => SetIconFontFamilyCore(fontFamily));
    }

    private FontMutation? SetIconFontFamilyCore(string fontFamily)
    {
        if (options.IconFontFamily == fontFamily)
        {
            return null;
        }

        options.IconFontFamily = fontFamily;
        return new FontMutation(
            CaptureSnapshot(),
            FontResourceChange.IconFamily,
            FlourishFontChangeKind.Icon,
            null
        );
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
        ExecuteMutation(() => SetOverrideFontCore(pageType, pageOverride));
    }

    private FontMutation? SetOverrideFontCore(
        Type pageType,
        FlourishPageFontOverride pageOverride
    )
    {
        if (
            options.PageFontOverridesByPageType.TryGetValue(pageType, out var current)
            && current == pageOverride
        )
        {
            return null;
        }

        options.PageFontOverridesByPageType[pageType] = pageOverride;
        return new FontMutation(
            CaptureSnapshot(),
            FontResourceChange.None,
            FlourishFontChangeKind.PageOverride,
            pageType
        );
    }

    public bool ClearOverrideFont<TPage>() where TPage : Page
    {
        return ClearOverrideFont(typeof(TPage));
    }

    public bool ClearOverrideFont(Type pageType)
    {
        ValidatePageType(pageType, nameof(pageType));
        var removed = false;
        ExecuteMutation(() =>
        {
            if (!options.PageFontOverridesByPageType.Remove(pageType))
            {
                return null;
            }

            removed = true;
            return new FontMutation(
                CaptureSnapshot(),
                FontResourceChange.None,
                FlourishFontChangeKind.PageOverride,
                pageType
            );
        });

        return removed;
    }

    internal bool ApplyToPage(Page page, Type? configuredPageType = null)
    {
        ArgumentNullException.ThrowIfNull(page);
        var pageType = configuredPageType ?? page.GetType();
        FlourishPageFontOverride? pageOverride;
        lock (gate)
        {
            options.PageFontOverridesByPageType.TryGetValue(pageType, out pageOverride);
        }

        var signature = new PageFontSignature(
            pageType,
            pageOverride?.FontFamily,
            pageOverride?.FontSize
        );
        var state = pageFontStates.GetValue(page, static _ => new PageFontResourceState());
        if (state.AppliedSignature == signature)
        {
            return false;
        }

        if (pageOverride is null)
        {
            RestorePageResources(page, state);
            page.SetResourceReference(WpfControl.FontFamilyProperty, "FlourishFontFamily");
            page.SetResourceReference(WpfControl.FontSizeProperty, "FlourishFontSizeBase");
            state.AppliedSignature = signature;
            return true;
        }

        CapturePageResources(page, state);
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

        state.AppliedSignature = signature;
        return true;
    }

    private void ExecuteMutation(Func<FontMutation?> mutationFactory)
    {
        Dispatcher? dispatcher;
        FontMutation? detachedMutation;
        lock (gate)
        {
            dispatcher = applicationDispatcher;
            detachedMutation = dispatcher is null ? mutationFactory() : null;
        }

        if (dispatcher is null)
        {
            PublishMutation(detachedMutation, null);
            return;
        }

        void ExecuteOnDispatcher()
        {
            FontMutation? mutation;
            ResourceDictionary? resources;
            lock (gate)
            {
                mutation = mutationFactory();
                resources = applicationResources;
            }

            PublishMutation(mutation, resources);
        }

        if (dispatcher.CheckAccess())
        {
            ExecuteOnDispatcher();
        }
        else
        {
            dispatcher.Invoke(ExecuteOnDispatcher);
        }
    }

    private void PublishMutation(
        FontMutation? mutation,
        ResourceDictionary? resources
    )
    {
        if (mutation is not { } value)
        {
            return;
        }

        if (resources is not null && value.ResourceChange != FontResourceChange.None)
        {
            ApplyResources(resources, value.Snapshot, value.ResourceChange);
            appliedResources = resources;
            appliedSnapshot = value.Snapshot;
        }

        RaiseChanged(value.Snapshot, value.ChangeKind, value.AffectedPageType);
    }

    private void RaiseChanged(
        FontSnapshot snapshot,
        FlourishFontChangeKind changeKind,
        Type? affectedPageType
    )
    {
        Changed?.Invoke(
            this,
            new FlourishFontChangedEventArgs(
                snapshot.FontFamily,
                snapshot.IconFontFamily,
                snapshot.FontSize,
                changeKind,
                affectedPageType
            )
        );
    }

    private FontSnapshot CaptureSnapshot()
    {
        return new FontSnapshot(
            options.FontFamily,
            options.IconFontFamily,
            options.FontSize
        );
    }

    private static void ApplyResources(
        ResourceDictionary resources,
        FontSnapshot snapshot,
        FontResourceChange change
    )
    {
        if ((change & FontResourceChange.TextFamily) != 0)
        {
            SetResourceIfChanged(
                resources,
                "FlourishFontFamily",
                new FontFamily(snapshot.FontFamily)
            );
        }

        if ((change & FontResourceChange.IconFamily) != 0)
        {
            SetResourceIfChanged(
                resources,
                "FlourishIconFontFamily",
                new FontFamily(snapshot.IconFontFamily)
            );
        }

        if ((change & FontResourceChange.Sizes) != 0)
        {
            SetPageFontSizeResources(resources, snapshot.FontSize);
        }
    }

    private static void SetResourceIfChanged(
        ResourceDictionary resources,
        string key,
        object value
    )
    {
        if (
            TryGetDirectResource(resources, key, out var current)
            && Equals(current, value)
        )
        {
            return;
        }

        resources[key] = value;
    }

    private static double ClampFontSize(double size)
    {
        return Math.Max(1, size);
    }

    private static void CapturePageResources(Page page, PageFontResourceState state)
    {
        if (state.HasCapturedResources)
        {
            return;
        }

        foreach (var key in PageFontResourceKeys)
        {
            if (TryGetDirectResource(page.Resources, key, out var value))
            {
                state.OriginalResources[key] = value;
            }
            else
            {
                state.MissingResourceKeys.Add(key);
            }
        }

        state.HasCapturedResources = true;
    }

    private static bool TryGetDirectResource(
        ResourceDictionary resources,
        string key,
        out object? value
    )
    {
        if (!resources.Keys.Cast<object>().Contains(key))
        {
            value = null;
            return false;
        }

        value = resources[key];
        return true;
    }

    private static void RestorePageResources(Page page, PageFontResourceState state)
    {
        if (!state.HasCapturedResources)
        {
            return;
        }

        foreach (var key in PageFontResourceKeys)
        {
            RestorePageResource(page, state, key);
        }

        state.OriginalResources.Clear();
        state.MissingResourceKeys.Clear();
        state.HasCapturedResources = false;
    }

    private static void RestorePageFontSizeResources(
        Page page,
        PageFontResourceState state
    )
    {
        if (!state.HasCapturedResources)
        {
            return;
        }

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
        SetResourceIfChanged(resources, "FlourishFontSizeSmall", ClampFontSize(baseSize - 4));
        SetResourceIfChanged(resources, "FlourishFontSizeCaption", ClampFontSize(baseSize - 2));
        SetResourceIfChanged(
            resources,
            "FlourishFontSizeDescription",
            ClampFontSize(baseSize - 1)
        );
        SetResourceIfChanged(resources, "FlourishFontSizeBase", baseSize);
        SetResourceIfChanged(resources, "FlourishFontSizeSubtitle", ClampFontSize(baseSize + 2));
        SetResourceIfChanged(
            resources,
            "FlourishFontSizeSectionTitle",
            ClampFontSize(baseSize + 6)
        );
        SetResourceIfChanged(
            resources,
            "FlourishFontSizePageTitle",
            ClampFontSize(baseSize + 18)
        );
        SetResourceIfChanged(resources, "FlourishFontSizeTitle", baseSize);
        SetResourceIfChanged(
            resources,
            "FlourishFontSizeTitlebarIcon",
            ClampFontSize(baseSize + 1)
        );
        SetResourceIfChanged(
            resources,
            "FlourishFontSizeNavigationIcon",
            ClampFontSize(baseSize + 1)
        );
        SetResourceIfChanged(
            resources,
            "FlourishFontSizeWindowButtonIcon",
            ClampFontSize(baseSize - 2)
        );
        SetResourceIfChanged(resources, "FlourishLineHeightBody", ClampFontSize(baseSize + 7));
        SetResourceIfChanged(
            resources,
            "FlourishLineHeightDescription",
            ClampFontSize(baseSize + 4)
        );
        SetResourceIfChanged(
            resources,
            "FlourishLineHeightSubtitle",
            ClampFontSize(baseSize + 10)
        );
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
        if (!double.IsFinite(value) || value <= 0)
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

    [Flags]
    private enum FontResourceChange
    {
        None = 0,
        TextFamily = 1,
        IconFamily = 2,
        Sizes = 4,
        All = TextFamily | IconFamily | Sizes,
    }

    private readonly record struct FontSnapshot(
        string FontFamily,
        string IconFontFamily,
        double FontSize
    );

    private readonly record struct FontMutation(
        FontSnapshot Snapshot,
        FontResourceChange ResourceChange,
        FlourishFontChangeKind ChangeKind,
        Type? AffectedPageType
    );

    private sealed record PageFontSignature(
        Type ConfiguredPageType,
        string? FontFamily,
        double? FontSize
    );

    private sealed class PageFontResourceState
    {
        internal Dictionary<string, object?> OriginalResources { get; } = [];

        internal HashSet<string> MissingResourceKeys { get; } = [];

        internal bool HasCapturedResources { get; set; }

        internal PageFontSignature? AppliedSignature { get; set; }
    }
}
