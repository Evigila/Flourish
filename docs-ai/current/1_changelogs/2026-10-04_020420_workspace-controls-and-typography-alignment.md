# Workspace controls and typography alignment

The user requested another source review of Colligere Workspace, restoration of independent navigation and disclosure actions, title-area return links, distinct standard/grid buttons, a fixed typography scale, and five separate Gallery guide regions. This record supersedes the combined navigation-row and responsive typography presentation in earlier records without changing their history.

## Source evidence and implementation

Colligere ProductEditor.razor:17 and CustomerEditor.razor:20/24 use RecordPageHeading. Its reusable component places record-parent-link before h1 in record-heading-copy. Products.razor:14 and WorkspaceMembers.razor:18/55 use primary-action/secondary-action for ordinary page operations; ProductEditor uses FormActionBar for its large form cells. Those shared styles are now in Flourish's Design layout and primitive files. The current Workspace secondary rail has ordinary links rather than a third-level tree; Flourish's third-level interaction remains an explicit user-requested extension.

Navigation branches now have a route link and adjacent disclosure button. Both derive their selected class from the same active route ancestry, independently of expanded state. Their background and foreground colors agree, with adjoining corners forming one row. The link opens its own page; the arrow only expands/collapses, retains the narrow rail, and participates in keyboard focus. Collapsing a selected branch retains both selected surfaces; expanding an unrelated branch does not select it. Existing single-line truncation, hidden scrollbars, disabled ancestry and bright shared child panels remain.

Gallery Controls passes the return target to PageHeading instead of placing a separate link below it. Both default and primitive return links reserve a transparent bottom border and reveal it on hover, with no underline decoration.

The principal Button mismatch was the container override: FormActions styled every button as a large cell. Button now remains a content-width, 48px-high, 12px-radius standard action with Filled/Outlined/Danger/Quiet variants. Primary/Secondary enum values remain compatible aliases. The new UniformGridButton composes Button to share native submit, busy, disabled and callback behavior without another DOM element; its optional Design skin fills a form/grid cell, with a 96px minimum height and 28px text. FormActions only applies that skin to UniformGridButton. FormActionBar also preserves legacy raw button/link styling while excluding ordinary f-button controls. Actual form/grid examples use UniformGridButton; ordinary sample actions use f-inline-actions. Both buttons have independent executable guide pages.

## Authorized typography contract

The user explicitly replaces responsive reading sizes in Flourish Design/Gallery with page title 42px, semantic H1 34px, H2 28px, H3 20px and body 17px. Page titles stay 42px in narrow and compact layouts; layout adaptation changes spacing. The opt-in f-type-art class permits artistic display at 42px or above. Foundation exports matching --f-type-* tokens and f-type-* classes. These are approved Flourish-specific replacements for the older portable common values; common files and WPF sources were not rewritten. Framework-only consumers retain native appearance and inherit host typography.

Audited 258 Blazor/Gallery CSS, Razor and C# source files, including explicit font-size declarations, font shorthands, variable fallbacks and inline source. Remaining out-of-scale font-size declarations are graphical: Material Symbols' 26px fallback, a 22px column drag handle, and a 24px checkbox tick. Text declarations use the five approved sizes. Old responsive page/section ranges and intermediate 38/40/48px reading sizes were removed; ordinary H3 is now 20px. Gallery Foundations describes the current scale.

## Guide structure

All 74 guides render five Section/H2 regions: introduction, control examples, API, scenario and scenario source. The duplicate outer component-name H2 is gone. Three full-row dark-gray boards (#343a40) contain the centered control examples, scenario, and source block. Explicit local dark ThemeScope keeps controls readable independently of the surrounding page's theme. The source board has only its code block; extra guide-stage prose and 35 scenario introductory paragraphs were removed while functional labels, data, status and actionable scene content remain.

The API uses existing DataTable<ComponentParameter> with the same reflected metadata and four columns (name, type, EditorRequired declaration, description); the hand-built api-table was removed. A component with no public parameters shows the existing table's honest empty state. Scenario source still comes from the executing sample, removing only Gallery namespace/registration lines.

## Verification

- Gallery and console builds succeeded with zero warnings/errors, using temporary output directories.
- Console data/DI/SSR and interaction contracts: 66/66 passed, including branch selection/collapse and both button behaviors.
- Menu mocked DOM checks: 23/23; shell script checks: 6/6; CSS bundle checks: 21/21.
- Copied all 74 displayed scenario sources into a separate temporary Razor library referencing only Framework/Design. Compilation succeeded with zero warnings/errors. Fixture: local temporary directory flourish-scenario-copy-45d2c78965874722b6be67b4ba6d8298.
- An owned temporary Gallery host returned all 74 independent guides and eight lightweight categories. Each guide had the five requested H2 labels, three boards, a library API list or empty state, and a return link before its page H1. Its outer rail had eight independent branch links/arrows and 74 leaves; the selected parent link and arrow both carried selected state. The new grid-button page rendered its actual component.
- Updated CSS/JS assets returned successfully. Material Symbols retained its exact length/hash and repeat conditional requests returned 304. HTTP logs: local temporary directory flourish-design-http-bad93f1e442b4007a91a9083530bbfa9. The temporary host was stopped.
- The architecture map now includes the two new component/sample files: 861 maintained files, 132 directories and the root. Active extraction and manual acceptance documents were updated. Historical records and user-owned pending skill deletions were preserved.

No Computer Use or browser visual/assistive-technology acceptance was performed. Source, SSR and mocked interactions do not establish actual pointer appearance, border painting, centering or narrow-window usability. The current manual checklist covers these checks. No external dependency, business rule or Colligere source change was introduced, and no Git commit was created.