using System.Linq;

using System;
using System.Collections.Generic;
using System.Threading;

using ArkheideSystem.Flourish.Abstract;
using System.Collections.ObjectModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using Application = System.Windows.Application;
using FontFamily = System.Windows.Media.FontFamily;
using WpfControl = System.Windows.Controls.Control;

namespace ArkheideSystem.Flourish.Appearance;

internal sealed class FontService : IFontService
{
    private readonly Lock gate = new();
    private readonly AppearanceOptions options;
    private readonly ConditionalWeakTable<Page, PageFontResourceState> pageFontStates = new();
    private FontState current;
    private Dispatcher? applicationDispatcher;
    private ResourceDictionary? applicationResources;
    private ResourceDictionary? appliedResources;
    private FontSnapshot? appliedSnapshot;

    private static readonly string[] PageFontResourceKeys =
    [
        "FlourishFontFamily",
        "FlourishFontSizeSmall",
        "FlourishFontSizeStandard",
        "FlourishFontSizeStandardIcon",
        "FlourishFontSizeIcon",
        "FlourishFontSizeLarge",
        "FlourishFontSizeExtraLarge",
        "FlourishFontSizeHeaderSize",
        "FlourishLineHeightSmall",
        "FlourishLineHeightStandard",
        "FlourishLineHeightStandardIcon",
        "FlourishLineHeightIcon",
        "FlourishLineHeightLarge",
        "FlourishLineHeightExtraLarge",
        "FlourishLineHeightHeaderSize",
    ];

    public FontService(AppearanceOptions options)
    {
        this.options = options ?? throw new ArgumentNullException(nameof(options));
        current = CaptureState();
    }

    public FontState Current => Volatile.Read(ref current);

    public event EventHandler<FontChangedEventArgs>? Changed;

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

    public void SetFont(
        string fontFamily,
        double smallFontSize,
        double standardFontSize,
        double iconFontSize,
        double largeFontSize,
        double extraLargeFontSize,
        double headerSizeFontSize
    )
    {
        fontFamily = ValidateNotBlank(fontFamily, nameof(fontFamily));
        ValidateFontScale(
            smallFontSize,
            standardFontSize,
            iconFontSize,
            largeFontSize,
            extraLargeFontSize,
            headerSizeFontSize
        );
        ExecuteMutation(() =>
            SetFontCore(
                fontFamily,
                smallFontSize,
                standardFontSize,
                iconFontSize,
                largeFontSize,
                extraLargeFontSize,
                headerSizeFontSize
            )
        );
    }

    private FontMutation? SetFontCore(
        string fontFamily,
        double smallFontSize,
        double standardFontSize,
        double iconFontSize,
        double largeFontSize,
        double extraLargeFontSize,
        double headerSizeFontSize
    )
    {
        ValidatePageOverrideScales(
            smallFontSize,
            standardFontSize,
            iconFontSize,
            largeFontSize,
            extraLargeFontSize,
            headerSizeFontSize
        );

        var change = FontResourceChange.None;
        if (options.FontFamily != fontFamily)
        {
            options.FontFamily = fontFamily;
            change |= FontResourceChange.TextFamily;
        }

        if (
            options.FontSizeSmall != smallFontSize
            || options.FontSizeStandard != standardFontSize
            || options.FontSizeIcon != iconFontSize
            || options.FontSizeLarge != largeFontSize
            || options.FontSizeExtraLarge != extraLargeFontSize
            || options.FontSizeHeaderSize != headerSizeFontSize
        )
        {
            options.FontSizeSmall = smallFontSize;
            options.FontSizeStandard = standardFontSize;
            options.FontSizeIcon = iconFontSize;
            options.FontSizeLarge = largeFontSize;
            options.FontSizeExtraLarge = extraLargeFontSize;
            options.FontSizeHeaderSize = headerSizeFontSize;
            change |= FontResourceChange.Sizes;
        }

        return change == FontResourceChange.None
            ? null
            : new FontMutation(
                CaptureSnapshot(),
                CaptureState(),
                change,
                FontChangeKind.GlobalText,
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
            CaptureState(),
            FontResourceChange.IconFamily,
            FontChangeKind.Icon,
            null
        );
    }

    public void SetOverrideFont(
        Type pageType,
        string fontFamily,
        double? smallFontSize,
        double? standardFontSize,
        double? iconFontSize,
        double? largeFontSize,
        double? extraLargeFontSize,
        double? headerSizeFontSize
    )
    {
        ValidatePageType(pageType, nameof(pageType));
        fontFamily = ValidateNotBlank(fontFamily, nameof(fontFamily));
        ValidateNullableSize(smallFontSize, nameof(smallFontSize));
        ValidateNullableSize(standardFontSize, nameof(standardFontSize));
        ValidateNullableSize(iconFontSize, nameof(iconFontSize));
        ValidateNullableSize(largeFontSize, nameof(largeFontSize));
        ValidateNullableSize(extraLargeFontSize, nameof(extraLargeFontSize));
        ValidateNullableSize(headerSizeFontSize, nameof(headerSizeFontSize));

        var pageOverride = new PageFontOverride(
            fontFamily,
            smallFontSize,
            standardFontSize,
            iconFontSize,
            largeFontSize,
            extraLargeFontSize,
            headerSizeFontSize
        );
        ExecuteMutation(() => SetOverrideFontCore(pageType, pageOverride));
    }

    private FontMutation? SetOverrideFontCore(Type pageType, PageFontOverride pageOverride)
    {
        ValidateFontScale(
            pageOverride.SmallFontSize ?? options.FontSizeSmall,
            pageOverride.StandardFontSize ?? options.FontSizeStandard,
            pageOverride.IconFontSize ?? options.FontSizeIcon,
            pageOverride.LargeFontSize ?? options.FontSizeLarge,
            pageOverride.ExtraLargeFontSize ?? options.FontSizeExtraLarge,
            pageOverride.HeaderSizeFontSize ?? options.FontSizeHeaderSize
        );

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
            CaptureState(),
            FontResourceChange.None,
            FontChangeKind.PageOverride,
            pageType
        );
    }

    public bool RemoveOverrideFont(Type pageType)
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
                CaptureState(),
                FontResourceChange.None,
                FontChangeKind.PageOverride,
                pageType
            );
        });

        return removed;
    }

    internal bool ApplyToPage(Page page, Type? configuredPageType = null)
    {
        ArgumentNullException.ThrowIfNull(page);
        var pageType = configuredPageType ?? page.GetType();
        var fontState = Volatile.Read(ref current);
        fontState.PageOverrides.TryGetValue(pageType, out var pageOverride);
        var globalScale = new FontScale(
            fontState.SmallFontSize,
            fontState.StandardFontSize,
            fontState.IconFontSize,
            fontState.LargeFontSize,
            fontState.ExtraLargeFontSize,
            fontState.HeaderSizeFontSize
        );

        var hasLocalScale = pageOverride is not null && HasLocalScale(pageOverride);
        var effectiveScale = hasLocalScale
            ? ResolveScale(pageOverride!, globalScale)
            : (FontScale?)null;
        var signature = new PageFontSignature(
            pageType,
            pageOverride?.FontFamily,
            pageOverride?.SmallFontSize,
            pageOverride?.StandardFontSize,
            pageOverride?.IconFontSize,
            pageOverride?.LargeFontSize,
            pageOverride?.ExtraLargeFontSize,
            pageOverride?.HeaderSizeFontSize,
            effectiveScale?.Small,
            effectiveScale?.Standard,
            effectiveScale?.Icon,
            effectiveScale?.Large,
            effectiveScale?.ExtraLarge,
            effectiveScale?.HeaderSize
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
            page.SetResourceReference(WpfControl.FontSizeProperty, "FlourishFontSizeStandard");
            state.AppliedSignature = signature;
            return true;
        }

        CapturePageResources(page, state);
        var fontFamily = new FontFamily(pageOverride.FontFamily);
        page.FontFamily = fontFamily;
        page.Resources["FlourishFontFamily"] = fontFamily;
        if (effectiveScale is { } scale)
        {
            page.FontSize = scale.Standard;
            SetPageFontSizeResources(page.Resources, scale);
        }
        else
        {
            RestorePageFontSizeResources(page, state);
            page.SetResourceReference(WpfControl.FontSizeProperty, "FlourishFontSizeStandard");
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
            detachedMutation = dispatcher is null ? CaptureMutation(mutationFactory) : null;
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
                mutation = CaptureMutation(mutationFactory);
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

    private void PublishMutation(FontMutation? mutation, ResourceDictionary? resources)
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

        RaiseChanged(value.Current, value.ChangeKind, value.AffectedPageType);
    }

    private FontMutation? CaptureMutation(Func<FontMutation?> mutationFactory)
    {
        var mutation = mutationFactory();
        if (mutation is { } value)
        {
            Volatile.Write(ref current, value.Current);
        }

        return mutation;
    }

    private void RaiseChanged(
        FontState current,
        FontChangeKind changeKind,
        Type? affectedPageType
    )
    {
        Changed?.Invoke(
            this,
            new FontChangedEventArgs(current, changeKind, affectedPageType)
        );
    }

    private FontState CaptureState()
    {
        return new FontState(
            options.FontFamily,
            options.IconFontFamily,
            options.FontSizeSmall,
            options.FontSizeStandard,
            options.FontSizeIcon,
            options.FontSizeLarge,
            options.FontSizeExtraLarge,
            options.FontSizeHeaderSize,
            new ReadOnlyDictionary<Type, PageFontOverride>(
                new Dictionary<Type, PageFontOverride>(options.PageFontOverridesByPageType)
            )
        );
    }

    private FontSnapshot CaptureSnapshot()
    {
        return new FontSnapshot(options.FontFamily, options.IconFontFamily, CaptureScale());
    }

    private FontScale CaptureScale()
    {
        return new FontScale(
            options.FontSizeSmall,
            options.FontSizeStandard,
            options.FontSizeIcon,
            options.FontSizeLarge,
            options.FontSizeExtraLarge,
            options.FontSizeHeaderSize
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
            SetPageFontSizeResources(resources, snapshot.Scale);
        }
    }

    private static void SetResourceIfChanged(ResourceDictionary resources, string key, object value)
    {
        if (TryGetDirectResource(resources, key, out var current) && Equals(current, value))
        {
            return;
        }

        resources[key] = value;
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

    private static void RestorePageFontSizeResources(Page page, PageFontResourceState state)
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

    private static void RestorePageResource(Page page, PageFontResourceState state, string key)
    {
        if (state.MissingResourceKeys.Contains(key))
        {
            page.Resources.Remove(key);
            return;
        }

        page.Resources[key] = state.OriginalResources[key];
    }

    private static void SetPageFontSizeResources(ResourceDictionary resources, FontScale scale)
    {
        ValidateFontScale(
            scale.Small,
            scale.Standard,
            scale.Icon,
            scale.Large,
            scale.ExtraLarge,
            scale.HeaderSize
        );
        SetResourceIfChanged(resources, "FlourishFontSizeSmall", scale.Small);
        SetResourceIfChanged(resources, "FlourishFontSizeStandard", scale.Standard);
        SetResourceIfChanged(resources, "FlourishFontSizeStandardIcon", scale.Icon);
        SetResourceIfChanged(resources, "FlourishFontSizeIcon", scale.Icon);
        SetResourceIfChanged(resources, "FlourishFontSizeLarge", scale.Large);
        SetResourceIfChanged(resources, "FlourishFontSizeExtraLarge", scale.ExtraLarge);
        SetResourceIfChanged(resources, "FlourishFontSizeHeaderSize", scale.HeaderSize);
        SetResourceIfChanged(resources, "FlourishLineHeightSmall", scale.Small + 2);
        SetResourceIfChanged(resources, "FlourishLineHeightStandard", scale.Standard + 2);
        SetResourceIfChanged(resources, "FlourishLineHeightStandardIcon", scale.Icon);
        SetResourceIfChanged(resources, "FlourishLineHeightIcon", scale.Icon);
        SetResourceIfChanged(resources, "FlourishLineHeightLarge", scale.Large + 4);
        SetResourceIfChanged(resources, "FlourishLineHeightExtraLarge", scale.ExtraLarge + 5);
        SetResourceIfChanged(resources, "FlourishLineHeightHeaderSize", scale.HeaderSize + 5);
    }

    private static bool HasLocalScale(PageFontOverride pageOverride)
    {
        return pageOverride.SmallFontSize is not null
            || pageOverride.StandardFontSize is not null
            || pageOverride.IconFontSize is not null
            || pageOverride.LargeFontSize is not null
            || pageOverride.ExtraLargeFontSize is not null
            || pageOverride.HeaderSizeFontSize is not null;
    }

    private static FontScale ResolveScale(
        PageFontOverride pageOverride,
        FontScale globalScale
    )
    {
        var scale = new FontScale(
            pageOverride.SmallFontSize ?? globalScale.Small,
            pageOverride.StandardFontSize ?? globalScale.Standard,
            pageOverride.IconFontSize ?? globalScale.Icon,
            pageOverride.LargeFontSize ?? globalScale.Large,
            pageOverride.ExtraLargeFontSize ?? globalScale.ExtraLarge,
            pageOverride.HeaderSizeFontSize ?? globalScale.HeaderSize
        );
        ValidateFontScale(
            scale.Small,
            scale.Standard,
            scale.Icon,
            scale.Large,
            scale.ExtraLarge,
            scale.HeaderSize
        );
        return scale;
    }

    private static string ValidateNotBlank(string value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Value cannot be empty.", parameterName);
        }

        return value;
    }

    private static void ValidateNullableSize(double? value, string parameterName)
    {
        if (value is { } size)
        {
            ValidatePositiveFinite(size, parameterName);
        }
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

    private static void ValidateFontScale(
        double smallFontSize,
        double standardFontSize,
        double iconFontSize,
        double largeFontSize,
        double extraLargeFontSize,
        double headerSizeFontSize
    )
    {
        ValidatePositiveFinite(smallFontSize, nameof(smallFontSize));
        ValidatePositiveFinite(standardFontSize, nameof(standardFontSize));
        ValidatePositiveFinite(iconFontSize, nameof(iconFontSize));
        ValidatePositiveFinite(largeFontSize, nameof(largeFontSize));
        ValidatePositiveFinite(extraLargeFontSize, nameof(extraLargeFontSize));
        ValidatePositiveFinite(headerSizeFontSize, nameof(headerSizeFontSize));
    }

    private void ValidatePageOverrideScales(
        double smallFontSize,
        double standardFontSize,
        double iconFontSize,
        double largeFontSize,
        double extraLargeFontSize,
        double headerSizeFontSize
    )
    {
        var globalScale = new FontScale(
            smallFontSize,
            standardFontSize,
            iconFontSize,
            largeFontSize,
            extraLargeFontSize,
            headerSizeFontSize
        );
        foreach (var pageOverride in options.PageFontOverridesByPageType.Values)
        {
            ResolveScale(pageOverride, globalScale);
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

    private readonly record struct FontScale(
        double Small,
        double Standard,
        double Icon,
        double Large,
        double ExtraLarge,
        double HeaderSize
    );

    private readonly record struct FontSnapshot(
        string FontFamily,
        string IconFontFamily,
        FontScale Scale
    );

    private readonly record struct FontMutation(
        FontSnapshot Snapshot,
        FontState Current,
        FontResourceChange ResourceChange,
        FontChangeKind ChangeKind,
        Type? AffectedPageType
    );

    private sealed record PageFontSignature(
        Type ConfiguredPageType,
        string? FontFamily,
        double? SmallFontSize,
        double? StandardFontSize,
        double? IconFontSize,
        double? LargeFontSize,
        double? ExtraLargeFontSize,
        double? HeaderSizeFontSize,
        double? EffectiveSmallFontSize,
        double? EffectiveStandardFontSize,
        double? EffectiveIconFontSize,
        double? EffectiveLargeFontSize,
        double? EffectiveExtraLargeFontSize,
        double? EffectiveHeaderSizeFontSize
    );

    private sealed class PageFontResourceState
    {
        internal Dictionary<string, object?> OriginalResources { get; } = [];

        internal HashSet<string> MissingResourceKeys { get; } = [];

        internal bool HasCapturedResources { get; set; }

        internal PageFontSignature? AppliedSignature { get; set; }
    }
}
