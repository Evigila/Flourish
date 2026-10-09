using System.Collections.Frozen;
using ArkheideSystem.Flourish.WPF.Abstract;
using F = ArkheideSystem.Flourish.WPF.Controls;

namespace ArkheideSystem.Flourish.WPF;

public sealed record ComponentUsageInfo(Type ComponentType, ComponentUseKind Kind, string Scenario, string Guidance);

/// <summary>Explicit reviewed native entry inventory; new controls must also provide executable Gallery samples.</summary>
public static class ComponentUsageCatalog
{
    public static IReadOnlyDictionary<Type, ComponentUsageInfo> Entries { get; } = Create();
    public static ComponentUsageInfo For(Type type) => Entries.TryGetValue(type, out var entry)
        ? entry : throw new KeyNotFoundException($"No reviewed component entry exists for {type.FullName}.");
    private static IReadOnlyDictionary<Type, ComponentUsageInfo> Create()
    {
        var entries = new Dictionary<Type, ComponentUsageInfo>();
        void Add(ComponentUseKind kind, string scenario, string guidance, params Type[] types)
        { foreach (var type in types) entries.Add(type, new(type, kind, scenario, guidance)); }
        Add(ComponentUseKind.General, "Actions", "Use native commands/events. Busy and Disabled suppress activation; variants affect paint. Structured labels remain one action.",
            typeof(F.Button), typeof(F.SplitButton), typeof(F.ActionMenu), typeof(F.InlineActions));
        Add(ComponentUseKind.General, "Native input and selection", "Use dependency-property binding and Field validation. Queries, reference creation and file processing remain consumer responsibilities.",
            typeof(F.TextBox), typeof(F.NumberBox), typeof(F.DateBox), typeof(F.CheckBox), typeof(F.SelectBox), typeof(F.ToggleSwitch),
            typeof(F.SearchBox), typeof(F.SearchAutocomplete), typeof(F.ReferenceDropdown), typeof(F.MaskedInput), typeof(F.FilePicker));
        Add(ComponentUseKind.General, "Complete keyed selection", "One MultiSelectBox owns business choices and table/chart display. Accept complete stable-key selection/order snapshots; fixed choices retain position and selection.", typeof(F.MultiSelectBox));
        Add(ComponentUseKind.General, "Forms and shared validation", "Field labels native inputs. ValidationMessages is the shared renderer for binding, cross-field and hidden-field errors. Consumers own submission and authorization.",
            typeof(F.Field), typeof(F.ValidationMessages), typeof(F.FormGroup), typeof(F.FormLayout), typeof(F.FormActions), typeof(F.ToggleSection));
        Add(ComponentUseKind.General, "Generic modal and disclosure", "Dialog owns one native modal lifecycle and awaited results. BottomSheet is a presentation value; confirmation uses ordinary Button. Disclosure expansion is separate from ToggleSection's business value.", typeof(F.Dialog), typeof(F.Disclosure));
        Add(ComponentUseKind.General, "Status and progress", "Use explicit semantic states. Progress is read-only; native reduced-motion/high-contrast policy remains library-owned.",
            typeof(F.Notice), typeof(F.NoticeTrigger), typeof(F.EmptyState), typeof(F.LoadingState), typeof(F.InteractionBoundary), typeof(F.ProgressBar), typeof(F.ProgressRing));
        Add(ComponentUseKind.General, "Content and bounded layout", "Library-owned composition supplies page, section, card and uniform-grid geometry. Use Card for identity and record summaries. Consumers supply business data and real actions.",
            typeof(F.Card), typeof(F.PageBody), typeof(F.PageHeading), typeof(F.Section), typeof(F.ContentContainer), typeof(F.UniformGrid), typeof(F.UniformGridItem), typeof(F.UniformGridButton));
        Add(ComponentUseKind.General, "Original text and artwork", "Text is selectable and never executed. Icon uses the same official Material Symbols names/artwork as Blazor. DisplayBoard owns explicit copy feedback.",
            typeof(F.CopyText), typeof(F.CodeBlock), typeof(F.Icon), typeof(F.DisplayBoard));
        Add(ComponentUseKind.Scenario, "Business application shell", "Configure project/top-bar/navigation through FrameworkBuilder. ApplicationShell owns native content scrolling and hierarchical selection; Window and business routes remain consumers' responsibilities.",
            typeof(F.ApplicationShell), typeof(F.ShellHeader), typeof(F.PrimaryNavigationItem), typeof(F.SecondaryNavigationItem), typeof(F.ServiceMenu), typeof(F.ContentSurface), typeof(F.NavigationSurface), typeof(F.NavigationGuard), typeof(F.SectionNavigator), typeof(F.BackToTop));
        Add(ComponentUseKind.Scenario, "Interactive records", "DataTable is the only interactive record-list controller: header sorting, local/remote search, paging, Table/Cards, display ordering, widths, retained row/cell and explicit bulk-edit callbacks. Native bindings replace browser SSR transport.", typeof(F.DataTable));
        Add(ComponentUseKind.Scenario, "Controlled table operations", "Share TableColumn/TableSearchRequest with DataTable. Native DataTemplate retains cell visual lifetime. ListView remains read-only; EditingGrid is a distinct spreadsheet scenario.",
            typeof(F.DataSearch), typeof(F.DataPager), typeof(F.ListView), typeof(F.EditingGrid));
        Add(ComponentUseKind.Scenario, "Native line chart", "Draw native paths with host-supplied series and labels. Series display uses the general MultiSelectBox; scales, text and selected members remain explicit.", typeof(F.LineChart));
        Add(ComponentUseKind.Scenario, "Image presentation", "Compose host-provided image sources, accessible captions and attribution without business processing.",
            typeof(F.ImagePreview), typeof(F.AttributionFooter), typeof(F.LogoDisplayer));
        Add(ComponentUseKind.Scenario, "Presentation and access", "PresentationBand owns full-width paint, ContentContainer bounded width and Hero split content. Offers, access state and navigation targets remain host data; OfferStage owns hover/focus/rotation.",
            typeof(F.PresentationBand), typeof(F.PresentationHero), typeof(F.PresentationFooter), typeof(F.OfferStage), typeof(F.OfferCard), typeof(F.AccessBrand), typeof(F.AccessPanel), typeof(F.AccessFormSurface), typeof(F.AccessActions), typeof(F.NavigationChoices));
        Add(ComponentUseKind.BuildingBlock, "Construction only", "The owner supplies interaction and selection semantics. Prefer the general selectors, ActionMenu or Dialog for a complete production entry.", typeof(F.DropdownSurface), typeof(F.ExpansionIndicator));
        return entries.ToFrozenDictionary();
    }
}
