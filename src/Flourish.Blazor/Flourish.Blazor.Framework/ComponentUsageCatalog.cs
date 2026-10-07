using System.Collections.Frozen;
using ArkheideSystem.Flourish.Blazor.Abstract;
using Patterns = ArkheideSystem.Flourish.Blazor.Components.Patterns;
using Primitives = ArkheideSystem.Flourish.Blazor.Components.Primitives;

namespace ArkheideSystem.Flourish.Blazor.Components;

/// <summary>Distinguishes general controls, explicit production scenarios and construction helpers.</summary>
public enum ComponentUseKind
{
    /// <summary>A general-purpose production control or layout component.</summary>
    General,
    /// <summary>A production entry point for the explicitly described scenario.</summary>
    Scenario,
    /// <summary>A construction helper whose owner must provide the missing interaction and semantics.</summary>
    BuildingBlock
}

/// <summary>Source-owned usage guidance; this describes actual capabilities, not a second visual specification.</summary>
public sealed record ComponentUsageInfo(
    Type ComponentType,
    ComponentUseKind Kind,
    TextReference Scenario,
    TextReference Guidance,
    Type? PreferredEntry = null);

/// <summary>Explicit usage inventory of exported Framework production entries and construction helpers.</summary>
public static class ComponentUsageCatalog
{
    public static IReadOnlyDictionary<Type, ComponentUsageInfo> Entries { get; } = CreateEntries();

    /// <summary>Looks up the reviewed entry, treating closed generic components as their generic definition.</summary>
    public static ComponentUsageInfo For(Type componentType)
    {
        ArgumentNullException.ThrowIfNull(componentType);
        var definition = componentType.IsGenericType ? componentType.GetGenericTypeDefinition() : componentType;
        return Entries.TryGetValue(definition, out var usage)
            ? usage
            : throw new KeyNotFoundException($"No reviewed component usage entry exists for {componentType.FullName}.");
    }

    private static IReadOnlyDictionary<Type, ComponentUsageInfo> CreateEntries()
    {
        // Deliberately explicit: adding an exported component must also choose its scope and guidance.
        ComponentUsageInfo[] entries =
        [
            General<ActionMenu>("Command and record action menus", "Production use: supply MenuAction through Actions, or standard Button/native form actions through ChildContent; the two are exclusive. One core owns availability, keyboard navigation, closing and async dialog callers. OpenOnHover applies only to Actions. Use ServiceMenu for service links."),
            Scenario<ApplicationLayout>("Business route layout", "Production use: set as the Router default layout to consume Program configuration and load Framework assets. Disable OwnsDocument when embedding examples; compose presentation components for presentation pages."),
            Scenario<ApplicationShell>("Business application shell", "Production use: top bar, two navigation levels, content scrolling and section navigation. Usually created by ApplicationLayout. Use presentation components for full-width banner pages."),
            General<Button>("Actions and navigation", "Production use: Variant selects appearance; Disabled and Busy select state. Href navigates and OnClick invokes actions. Icon-only buttons require an accessible name. Description and TrailingText opt into one full-width structured button with a stacked primary label and passive trailing text; Text is required and ChildContent is not accepted in this mode."),
            General<Card>("Content cards", "Production use: organize ordinary content. Use the dedicated width container or presentation band for page geometry."),
            General<CopyText>("Selectable original text", "Production use: display encoded Value with standard code typography. No execution or clipboard side effect; use DisplayBoard for a complete copy action."),
            General<FormGroup>("Grouped native fields", "Production use: native fieldset and legend combined with FormLayout. Disabled disables the group; the host retains validation and submission."),
            General<InlineActions>("Compact standard action rows", "Production use: wrapping rows of standard Button or actual controls at a native protocol boundary. Alignment chooses Start, Center (default), or End in the available width. Field inputs take remaining width and buttons keep natural width. Owns spacing and layout only; use UniformGridButton for finite wizard choices."),
            Scenario<ImagePreview>("Read-only images and captions", "Production use: standard figure, img and figcaption. The host supplies Url and Alt; Actions uses standard controls. Upload, authentication and image processing remain host responsibilities."),
            Scenario<AttributionFooter>("Compact attribution", "Production use: standard footer and small for host-supplied copyright or attribution. Use PresentationFooter for marketing wordmarks."),
            Scenario<CheckBox>("Model-bound boolean input", "Production use: @bind-Value supplies ValueExpression and supports EditForm validation. Use StandaloneCheckBox for native POST or input without a validation model."),
            Scenario<CodeBlock>("Code and text display", "Production use: encode and display text without execution. May be composed with DisplayBoard; use ordinary content containers for business prose."),
            Scenario(typeof(DataTable<>), "Record browsing and business tables", "Production use: shared TableColumn and TableSearchRequest; local/remote search, paging, culture-aware formatting, Table/Cards, column visibility/order/width, scoped sort preferences, native row actions, Registry/Pool/Worklist and explicit bulk editing. Progressive enhances complete authorized visible SSR text; hidden defaults, independent search/sort values and editing are unsupported in that mode."),
            Scenario(typeof(DataSearch<>), "Controlled column search", "Production use: share TableColumn and TableSearchRequest with DataTable and standard fields/select/text input. Defaults to the first searchable column; IncludeAllColumns enables a null key for all columns. The host owns queries/results; ChildContent supplies inline filters."),
            Scenario(typeof(DateBox<>), "Model-bound date input", "Production use: InputDate parsing, ValueExpression and EditForm validation. Use StandaloneTextBox Type=date for native date POST."),
            General<Dialog>("Generic modal and content views", "Production use: controlled IsOpen or ShowAsync awaiting a result; CloseAsync returns it, and close/cancel/disposal returns null. Shared Busy, CanClose and focus behavior; Centered/BottomSheet presentations. BrowserControlled prerenders views for the standard browser module. Views/View are keyed content; confirmation/reconnection business state stays with the host."),
            General<Disclosure>("Native expandable content", "Production use: details/summary disclosure. Title names the trigger; expansion does not change business boolean fields."),
            Scenario<DisplayBoard>("Control previews and copyable content", "Production use: previews, dotted backgrounds and copy feedback. Use the appropriate presentation/form/page composition for complete scenarios."),
            BuildingBlock<DropdownSurface>("Native dropdown surface", "Building block: details/summary and panel only. Does not own selected values, list keyboard behavior, outside-click dismissal or command models. Use SelectBox, ReferenceDropdown or MultiSelectBox for selection and ActionMenu for commands."),
            General<EmptyState>("Actual empty results and unused page areas", "Production use: Standard explains an empty result and the next action; Watermark presents title-only passive content centered in an unused page area. Use the appropriate states for loading, failure or missing permissions."),
            BuildingBlock<ExpansionIndicator>("Decorative expansion indicator", "Building block: an aria-hidden triangle with no interaction. Button, SplitButton or the selector owns its name, aria-expanded/aria-controls and events."),
            General<Field>("Field labels and validation", "Production use: associate Label, Id, required state, errors and input. Submission, authorization and persistence remain host responsibilities."),
            Scenario<ValidationMessages>("Hidden and cross-field validation messages", "Production use: the same error renderer as Field. For binds an EditContext field and Error supplies explicit errors. Use Field.For for ordinary inputs; hidden tokens and cross-field errors can use this entry independently. Business validation, translation, submission and authorization remain with the host."),
            Scenario<NavigationChoices>("Native GET method selection", "Production use: NavigationChoiceItem items and a route-derived ActiveKey. Compact selects standard Button links with Variant/ActiveVariant; the default is a grid. The library owns same-site GET links and retained panels, with aria-current on the active link. The host supplies forms and retains authentication/POST/authorization. Disabled is not authorization."),
            Scenario<FilePicker>("Interactive file selection", "Production use: receive selected files through InputFileChangeEventArgs. File type, size, authorization and server validation belong to the host; Accept is not a security boundary."),
            Scenario<FormActions>("Business form actions", "Production use: equal-width save, cancel and bulk action areas. Use AccessActions or standard Button for compact presentation/login action groups."),
            General<FormLayout>("Field layout", "Production use: field columns, input behavior and focus on invalid fields. The host retains EditForm or native GET/POST protocol boundaries."),
            General<Icon>("Icon rendering", "Production use: shared self-hosted icons with inherited control color. Decorative icons do not provide independent action semantics."),
            General<IdentityCard>("Identity and record summaries", "Production use: Title is the native summary heading, with HeadingLevel defaulting to 2. Use HeadingLevel=1 for a prominent account name and CopyText for its identifier. SideContent supplies supporting facts. Compose the dedicated hero or form controls for product presentation and editing."),
            Scenario(typeof(ListView<>), "Static read-only tables", "Production use: display all TableColumn data in input order, with optional row headings and read-only templates. No search, sorting, paging, selection, row actions or JavaScript. Use DataTable for interactive record management."),
            General<LoadingState>("Actual loading feedback", "Production use: status announcements and loading indicators. Keep failures, empty results and permission problems visible as their own states."),
            General<Notice>("Status and error notifications", "Production use: NotificationSeverity, Title, Subtle and announcement control. Errors requiring immediate attention must be directly visible, not available only on hover."),
            Scenario(typeof(NumberBox<>), "Model-bound numeric input", "Production use: type parsing, ValueExpression and optional EditForm validation. Use StandaloneTextBox Type=number for native POST or raw numeric text."),
            General<PageBody>("Business page content width", "Production use: controlled content width and gutters; direct child sections follow the page layout. FillHeight fills a fixed shell stage, retains the direct PageHeading and gives the remaining space to content such as EditingGrid. Use Compact on the heading for immediate collapse. Use PresentationBand/ContentContainer for full-width backgrounds with bounded content."),
            Scenario<PageHeading>("Business page and record titles", "Production use: collapsible heading, parent link and actions. Use PresentationHero for artistic presentation titles."),
            General<ProgressBar>("Task progress", "Production use: determinate progress from 0 to 100, or null for indeterminate progress. Status text describes the actual task stage."),
            General<ProgressRing>("Compact task progress", "Production use: determinate or indeterminate progress in compact areas. Button busy state remains a separate action concern."),
            General<SearchBox>("Query text", "Production use: debounced, cancellable query callbacks. Use SearchAutocomplete for choosing suggestions and DataSearch for column-based queries."),
            General<Section>("Business content sections", "Production use: standard heading, section content and section actions. Use presentation components for full-width backgrounds and artistic layouts."),
            Scenario<SectionNavigator>("In-page business content navigation", "Production use: headings within the specified ContentId or explicit Items. Applies to the business content scroll area; presentation pages use their appropriate navigation composition."),
            Scenario<BackToTop>("Return to the top of content", "Production use: standard Elevated icon button for the specified ContentId scroll area. Retains static fragment navigation, reduced-motion support and target focus. Does not change authorization or business navigation."),
            Scenario<LineChart>("Read-only multi-series line charts", "Production use: provide labels and numeric series for standard themed rendering. ControlsContent filters share one responsive toolbar with the built-in display controller. Independent scales are explicit and expose maxima; precise values are available in an accessible table. The chart owns visibility/order, not business calculations or data mutations."),
            General<MultiSelectBox>("General multi-selection with optional ordering", "Production use: business options, table columns and chart series share one selector. One component owns stable keys, complete selection/order snapshots, limits, fixed/disabled items, search and async creation. Ordering is explicitly enabled; business persistence and display-state ownership remain outside."),
            Scenario(typeof(SelectBox<>), "Model-bound single selection", "Production use: SelectOption, ValueExpression and optional EditForm validation. Use StandaloneSelectBox for native POST and ReferenceDropdown for searchable references with creation."),
            General<SplitButton>("Two-action entry", "Production use: independent main and secondary actions. MenuContent uses native details and works with static SSR; the secondary action does not submit. Without a slot, callbacks and controlled expansion remain. FullWidth/Width changes total width; the secondary button stays 48px square. PrimaryDisabled locks only the main action; Busy/Disabled locks both."),
            Scenario<StandaloneCheckBox>("Boolean input for native POST or unvalidated models", "Production use: Value/ValueChanged, native attributes and submitted values. Does not register EditContext validation; use CheckBox for model validation."),
            Scenario(typeof(StandaloneSelectBox<>), "Single selection for native POST or unvalidated models", "Production use: options, Value/ValueChanged and native attributes. Does not register EditContext validation; use SelectBox for validated model selection."),
            Scenario<StandaloneTextBox>("Text input for native POST or unvalidated models", "Production use: native name/form/autocomplete attributes and Value/ValueChanged. The server owns validation; use TextBox for EditContext validation."),
            Scenario<TextBox>("Model-bound text input", "Production use: ValueExpression, typed input and optional EditForm validation. Use StandaloneTextBox for native POST or input without a validation model."),
            General<ToggleSection>("Content controlled by a boolean state", "Production use: shared boolean binding with ToggleSwitch; hidden content stays mounted. Native disclosure controls expansion without representing a business switch value."),
            General<ToggleSwitch>("Boolean switches", "Production use: Value/ValueChanged, status labels and a controlled region. Appearance changes do not create a separate switch-state API."),
            Scenario<UniformGrid>("Uniform display cells", "Production use: UniformGridItem/UniformGridButton shapes, size limits, automatic columns and variants. Optional CellHeight fixes rectangular row height independently of responsive width and overrides MaxCellHeight. Square shape does not accept a fixed height. The library owns uniform cell layout and interaction."),
            Scenario<UniformGridButton>("Interactive grid cells", "Production use: finite choices for form actions and multi-step wizards. Keep PageHeading; choice areas contain the grid and button labels without extra step headings or selectors. Rectangles fill rows and divide columns equally; squares retain equal width/height limits. Inherits grid layout/appearance and Button behavior; use standard buttons for navigation bars and login forms."),
            Scenario<UniformGridItem>("Read-only grid cells", "Production use: shared content structure and appearance with UniformGridButton, without Href or OnClick. Use a real button when actions are required."),

            Scenario<Patterns.ContentSurface>("Custom content hosts", "Production use: top bar and content composition; DocumentFlow/Footer support document-flow presentation pages. Explicitly choose business-scroll or full-width document layouts without overriding the other mode with host CSS."),
            Scenario<Patterns.NavigationSurface>("Custom business navigation hosts", "Production use: two navigation levels and one business scroll area. ConfigureLayout.SetContentWidth supplies the centered reference width; CenteredContentGutterScale scales each centered PageBody side gutter for this shell only (0 through 1, default 1). Standard applications usually use ApplicationLayout; presentation pages use a document-flow content host."),

            Scenario<Primitives.AccessBrand>("Access-entry branding", "Production use: identity and context for sign-in, registration and access pages. TitleId must be unique within the page; use presentation controls for marketing titles."),
            Scenario<Primitives.DataPager>("Paging of controlled loaded results", "Production use: the host manages CurrentPage/PageSize and handles PageChanged. IsLimited means loaded results only; it neither promises a server total nor queries data."),
            Scenario<Primitives.EditingGrid>("Spreadsheet editing", "Production use: GridColumn/GridRow/GridCell, keyboard, clipboard and host editing commands. Editing is a distinct scenario from a read-only list or ordinary DataTable. The host retains validation, saving and authorization."),
            Scenario<Primitives.InteractionBoundary>("Temporary interaction lock", "Production use: the host supplies the actual reason and inert suspends editing. This does not authenticate or authorize users and cannot replace server validation."),
            Scenario<Primitives.MaskedInput>("Model-bound masked input", "Production use: InputBase, ValueExpression and shared mask formatting. Use StandaloneMaskedInput for native POST or no validation model; masking has behavior beyond TextBox appearance."),
            Scenario<Primitives.NavigationGuard>("Leaving an unsaved draft", "Production use: HasUnsavedChanges reflects actual draft state and protects interactive navigation/browser exit. It is not a security or persistence guarantee; pure SSR has no interactive protection."),
            Scenario<Primitives.NoticeTrigger>("Secondary status details", "Production use: status icons with details readable on focus/hover. Supplementary information only; use Notice for important warnings or errors that must be immediately visible."),
            Scenario<Primitives.PrimaryNavigationItem>("Primary business-shell navigation", "Production use: icon navigation rail, current state and unavailable explanation. Configure ordinary navigation through Program; use the appropriate controls for presentation links and actions."),
            Scenario(typeof(Primitives.ReferenceDropdown<>), "Searchable single references", "Production use: value-type identifiers, nullable selection, unknown labels and optional creation. Retains single-reference semantics rather than plain SelectBox appearance or a multi-value collection API."),
            Scenario(typeof(Primitives.SearchAutocomplete<>), "Search suggestion selection", "Production use: query text, suggestion keys, keyboard active item and ItemSelected. The host provides remote suggestions. It has selection semantics beyond SearchBox and does not bind a reference identifier."),
            Scenario<Primitives.SecondaryNavigationItem>("Secondary business-shell navigation", "Production use: route matching and the current-page link. Configure ordinary navigation through Program; use content links or presentation actions for those scenarios."),
            Scenario<Primitives.ServiceMenu>("Top-level service navigation", "Production use: ServiceLink destinations. Use ActionMenu for commands; user authorization and session switching belong to the host."),
            Scenario<Primitives.ShellHeader>("Brand and identity header", "Production use: branding, services, identity and action slots. May be the top bar of an explicit page host; it does not own the entire page's scrolling or navigation."),
            Scenario<Primitives.StandaloneMaskedInput>("Masked input for native POST or unvalidated models", "Production use: Value/ValueChanged and native attributes. Does not register EditContext validation; use MaskedInput for model validation."),

            General<ContentContainer>("Centered bounded content", "Production use: independently control content width and horizontal centering on business or presentation pages. PresentationBand owns the full-width background."),
            Scenario<PresentationBand>("Full-width presentation bands", "Production use: default minimum height 800px with natural growth. Separate full-width background and internal ContentContainer. Dotted defaults to false; Tone uses standard color roles, with alternating neighboring bands recommended. Do not override widths with consumer CSS."),
            Scenario<PresentationHero>("Artistic product headings", "Production use: artistic heading, subtitle and standard actions for presentation homepages. Use PageHeading for business records and management pages."),
            Scenario<LogoDisplayer>("Project identity on access/presentation pages", "Production use: Logo above the artistic project name; reads ConfigureProject by default. Names support scoped localization and instance overrides. HeadingLevel selects actual heading semantics. Branding is distinct from authentication behavior and shell-header identity."),
            Scenario<PresentationFooter>("Presentation-site brand footer", "Production use: full-width footer, centered content and decorative watermark. Heading/watermark follow the configured localized project name unless ProjectName supplies an instance override. Copyright is optional. Business navigation/account panels use their own composition."),
            Scenario<OfferStage>("Product offer presentation", "Production use: responsive offer stage and selection presentation. The host supplies prices, licensing capabilities and purchase actions, and owns actual orders/authorization."),
            Scenario<OfferCard>("Product offer cards", "Production use: offer names, descriptions and standard actions, composed with OfferStage. Use IdentityCard/Card for business entity summaries."),
            Scenario<AccessPanel>("Access-entry content area", "Production use: centered access panel inside an existing page host without creating a second main. Compose with AccessFormSurface/AccessActions; authentication remains host-owned."),
            Scenario<AccessFormSurface>("Access-entry form content", "Production use: standard form surface for sign-in, registration and recovery, with explicit Surface/Primary/Canvas Tone. Organizes content without owning authentication or changing native GET/POST protocols."),
            Scenario<AccessActions>("Compact access/presentation action groups", "Production use: arrange standard Button controls for login and presentation actions. Use FormActions for equal-width business form actions.")
        ];
        return entries.ToFrozenDictionary(entry => entry.ComponentType);
    }

    private static ComponentUsageInfo General<TComponent>(string scenario, string guidance, Type? preferredEntry = null)
        => new(typeof(TComponent), ComponentUseKind.General, UsageText(typeof(TComponent), "Scenario", scenario), UsageText(typeof(TComponent), "Guidance", guidance), preferredEntry);

    private static ComponentUsageInfo Scenario<TComponent>(string scenario, string guidance)
        => Scenario(typeof(TComponent), scenario, guidance);

    private static ComponentUsageInfo Scenario(Type componentType, string scenario, string guidance)
        => new(componentType, ComponentUseKind.Scenario, UsageText(componentType, "Scenario", scenario), UsageText(componentType, "Guidance", guidance));

    private static ComponentUsageInfo BuildingBlock<TComponent>(string scenario, string guidance, Type? preferredEntry = null)
        => new(typeof(TComponent), ComponentUseKind.BuildingBlock, UsageText(typeof(TComponent), "Scenario", scenario), UsageText(typeof(TComponent), "Guidance", guidance), preferredEntry);

    private static TextReference UsageText(Type componentType, string part, string fallback)
    {
        var name = componentType.Name.Split((char)96)[0];
        if (componentType.Namespace?.EndsWith(".Primitives", StringComparison.Ordinal) == true) name = "Primitives_" + name;
        else if (componentType.Namespace?.EndsWith(".Patterns", StringComparison.Ordinal) == true) name = "Patterns_" + name;
        return new("Flourish", $"Usage_{name}_{part}", fallback);
    }
}
