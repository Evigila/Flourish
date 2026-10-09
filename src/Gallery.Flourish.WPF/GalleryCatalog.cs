using System.Reflection;
using System.Windows;
using ArkheideSystem.Flourish.WPF.Abstract;
using ArkheideSystem.Flourish.WPF;
using F = ArkheideSystem.Flourish.WPF.Controls;

namespace ArkheideSystem.Gallery.Flourish.WPF;

public sealed record GalleryEntry(Type ComponentType, string Category)
{
    public string Name => ComponentType.Name;
    public string Route => "/controls/" + Category + "/" + Name;
    public ComponentUsageInfo Usage => ComponentUsageCatalog.For(ComponentType);
    public object Create(Window owner, ITextProvider texts) => Samples.Create(ComponentType, owner, texts);
}

/// <summary>The executable native samples follow the same semantic groupings as Blazor.</summary>
public static class GalleryCatalog
{
    public static IReadOnlyList<string> Categories { get; } =
        ["actions", "inputs", "data", "overlays", "feedback", "progress", "content", "layout", "presentation"];
    public static IReadOnlyList<GalleryEntry> Entries { get; } = typeof(F.Button).Assembly.GetExportedTypes()
        .Where(type => type.Namespace == typeof(F.Button).Namespace && !type.IsAbstract
            && (typeof(FrameworkElement).IsAssignableFrom(type) || type.Name == "Dialog"))
        .OrderBy(type => type.Name, StringComparer.Ordinal)
        .Select(type => new GalleryEntry(type, Category(type.Name))).ToArray();

    public static string Category(string name) => name switch
    {
        "Button" or "SplitButton" or "ActionMenu" or "UniformGridButton" or "InlineActions" or "ExpansionIndicator" => "actions",
        "TextBox" or "NumberBox" or "DateBox" or "CheckBox" or "SelectBox" or "ToggleSwitch"
            or "SearchBox" or "ReferenceDropdown" or "SearchAutocomplete" or "MaskedInput" or "MultiSelectBox"
            or "FilePicker" or "Field" or "ValidationMessages" or "FormLayout" or "FormGroup" or "FormActions" or "ToggleSection" => "inputs",
        "DataTable" or "DataSearch" or "DataPager" or "ListView" or "EditingGrid" or "LineChart" => "data",
        "Dialog" or "DropdownSurface" => "overlays",
        "Notice" or "NoticeTrigger" or "EmptyState" or "LoadingState" or "InteractionBoundary" => "feedback",
        "ProgressBar" or "ProgressRing" => "progress",
        "ApplicationShell" or "ApplicationLayout" or "ContentSurface" or "NavigationSurface" or "PageBody" or "PageHeading"
            or "ContentContainer" or "ShellHeader" or "PrimaryNavigationItem" or "SecondaryNavigationItem"
            or "ServiceMenu" or "SectionNavigator" or "BackToTop" or "NavigationGuard" => "layout",
        "PresentationBand" or "PresentationHero" or "PresentationFooter" or "LogoDisplayer" or "OfferStage"
            or "OfferCard" or "AccessPanel" or "AccessFormSurface" or "AccessActions" or "AccessBrand"
            or "NavigationChoices" or "AttributionFooter" => "presentation",
        _ => "content"
    };
}
