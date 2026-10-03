# Extract a Blazor UI library and Workspace Gallery

## Request and decisions

The user abandoned the prior Figma effort and selected gradual extraction of Colligere's actual shared Web UI directly into the Flourish repository. The Web Gallery must follow Colligere/Workspace, while WPF contributes only the public API/internal implementation pattern. Existing WPF/WinUI source, root README modification and pre-existing skill deletions are preserved.

## Delivery

Added src/Flourish.Blazor (net10.0 Razor Class Library, package Arkheide.Flourish.Blazor 1.1.0), src/Gallery.Blazor (net10.0 Interactive Server Web App with prerender) and tests/Flourish.Blazor.Test (dependency-free executable contract/SSR checks plus Node built-in tests). Registered all three in Flourish.slnx. Only the installed ASP.NET shared framework and existing Core project are referenced; no direct external package, font or icon dependency is added.

The 32 public Razor components cover configured shell/layout/theme, titles/sections/cards, form layouts and action grids, InputBase text/select/check/numeric inputs, toggles/disclosures, debounced search, semantic feedback, known/unknown progress, row menus, guarded native dialogs/sheets and generic local data lists. AddFlourish/UseTitleBar/UseNavigation/ConfigureAppearance/ConfigureLayout expose startup composition without leaking internal options/builders. Runtime appearance is scoped. ApplicationShell accepts host-filtered runtime navigation and context title without taking ownership of access policy. JS lifetimes belong to component roots and clean up listeners/references.

Gallery contains Workspace-shaped overview, 33 explicitly labeled demonstration records, local search/sort/pagination/list-cards/visibility/resize, create/edit/detail/delete flows, model validation and dirty navigation confirmation, control interactions, theme palettes and genuine source examples. Its model store is circuit-scoped and in-memory. No Gallery CSS or JS copies the library's implementation. Unknown URLs retain HTTP404 with a complete shell.

Source-to-component correspondence, public API boundaries and adoption order are in ../blazor-extraction.md. Manual acceptance is in ../blazor-manual-tests.md. Package consumer instructions are in src/Flourish.Blazor/README.md.

## Verification

- Gallery and library builds: 0 warnings, 0 errors.
- Contract/data/DI/SSR runner: 32/32 passed. Includes numeric/date/culture stable/null-last sorting, column-scoped search, page bounds, scoped user state, frozen builders, arbitrary color contrast, HTML encoding, table status/page semantics, model-bound fields, dialog busy/dirty/concurrent close, runtime navigation/title and numeric/progress semantics.
- Node tests run actual data.js with DOM substitutes: 6/6 passed (width bounds, keyboard/pointer behavior, cancellation, replacement/instance isolation, pager Enter and detach).
- All three library JS modules passed syntax checks.
- Local NuGet packaging succeeds and includes assembly, XML/readme, static-web-asset build metadata and all six CSS/JS assets. Nothing was published.
- Local HTTP verifies eight primary routes (including missing-record empty state) with full shell, initial records with 20 rows and all six static assets. Subsequent complete-404 checks are reported in the task result.
- No Computer Use testing. SSR/DOM substitutes do not verify real visual rendering, browser focus/hover/touch or assistive technology; those remain in the manual checklist.

## Boundaries and follow-up

Colligere runtime consumers have not been replaced in this initial library delivery. Its effective shared UI contract and unrelated product changes remain intact. Custom masks, remote/autocomplete/multiple-selection controls, server/cursor pagination, advanced spreadsheet/bulk edit and durable preference storage remain future extraction work. Authentication, domain validation, query limits, persistence and authorization remain consumer-owned. No Core singleton UI services were moved into Web DI and no WPF typography was used as the Web default. Default Web fonts remain the existing Segoe UI/system stack; this is not a Noto download/migration.

Review in a real browser before adopting into Colligere. Then migrate leaf components/layouts/local lists/shell in batches, preserving each page's save/access/dirty-state behavior and deleting duplicate source only after its last consumer moves.