namespace ArkheideSystem.Gallery.Flourish.Blazor.Models;

/// <summary>Groups Gallery component documentation by use without changing library metadata.</summary>
public static class CatalogSections
{
    public static IReadOnlyDictionary<string, string> Titles { get; } = new Dictionary<string, string>
    {
        ["actions"] = "按钮与菜单",
        ["inputs"] = "输入与选择",
        ["data"] = "数据与列表",
        ["overlays"] = "对话框与弹层",
        ["feedback"] = "状态反馈",
        ["progress"] = "进度",
        ["content"] = "内容与组合",
        ["layout"] = "外壳与布局"
    };

    public static string HrefFor(string category) => category == "actions" ? "/controls" : $"/controls/{category}";

    public static IReadOnlyList<ComponentEntry> EntriesFor(string category) => ComponentCatalog.Groups.SelectMany(group => group.Entries)
        .Where(entry => CategoryOf(entry) == category).ToArray();

    public static string ComponentHref(ComponentEntry entry) => $"/controls/{CategoryOf(entry)}/{SampleCatalog.For(entry).Key}";

    public static string CategoryOf(ComponentEntry entry) => entry.Name switch
    {
        "Button" or "UniformGridButton" or "SplitButton" or "ExpansionIndicator" or "ActionMenu" => "actions",
        "TextBox" or "NumberBox<TValue>" or "SelectBox<TValue>" or "MultiSelectBox" or "CheckBox" or "ToggleSwitch"
            or "DateBox<TValue>" or "FilePicker" or "StandaloneTextBox"
            or "StandaloneSelectBox<TValue>" or "StandaloneCheckBox" or "AccessFormSurface" or "AccessActions"
            or "SearchBox" or "Field" or "ValidationMessages" or "FormLayout" or "FormActions" or "FormGroup"
            or "Primitives.MaskedInput" or "Primitives.StandaloneMaskedInput"
            or "Primitives.SearchAutocomplete<TItem>"
            or "Primitives.ReferenceDropdown<TValue>"
            => "inputs",
        "DataTable<TItem>" or "DataSearch<TItem>" or "ListView<TItem>" or "LineChart"
            or "Primitives.DataPager" or "Primitives.EditingGrid" => "data",
        "Dialog" or "DropdownSurface" => "overlays",
        "Notice" or "EmptyState" or "LoadingState" or "Primitives.NoticeTrigger" => "feedback",
        "ProgressBar" or "ProgressRing" => "progress",
        "ApplicationLayout" or "ApplicationShell" or "PageBody" or "PageHeading" or "SectionNavigator" or "BackToTop"
            or "Patterns.ContentSurface" or "Patterns.NavigationSurface"
            or "ContentContainer" or "PresentationBand" or "PresentationHero" or "PresentationFooter" or "LogoDisplayer" or "AccessPanel" or "NavigationChoices"
            or "Primitives.AccessBrand" or "Primitives.ShellHeader"
            or "Primitives.ServiceMenu" or "Primitives.PrimaryNavigationItem" or "Primitives.SecondaryNavigationItem"
            or "Primitives.NavigationGuard" or "Primitives.InteractionBoundary" or "AttributionFooter" => "layout",
        _ => "content"
    };
}
