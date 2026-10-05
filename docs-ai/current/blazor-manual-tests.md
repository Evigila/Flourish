# Blazor manual acceptance

This is the current checklist for Gallery, Framework-only consumption and approved host integration. It consolidates repeated earlier checklists; old expected layouts, palette counts and run totals are historical. Read the [implementation guide](blazor-extraction.md) and [seventeen-role color contract](blazor-color-roles.md) first. Record actual outcomes in a new append-only change record, including build revision, host mode, browser/version, viewport and unresolved failures. Unchecked items remain acceptance work.

## Prepare the intended consumer

- Rebuild and restart the selected host, then reload the document so Program configuration, CSS and dynamic modules match the current source. Existing Visual Studio processes do not reload all startup options automatically.
- Start Gallery with dotnet run --project src/Gallery.Flourish.Blazor (localhost:5188). Separately start dotnet run --project tests/Tests.Flourish.Blazor.Native (localhost:5189). Gallery loads Framework then Design; Native loads Framework only and registers no appearance service.
- Repeat relevant checks at wide width, 980px, 760px, 520px and 320px, with 200% zoom, long Chinese/Latin labels, keyboard-only use and Light/Dark/System. Include forced colors, reduced motion and at least one browser without customizable native selects.
- Clear console/network errors before each sequence. Repeated navigation must not produce failed assets, decoding errors, disposed callbacks or accumulating interaction listeners. A successful SSR/mock/HTTP check is not a completed visual or assistive-technology check.

## Solutions, public boundaries and packages

- Open Flourish.slnx in Visual Studio: Core is at root; platform groups contain libraries/Galleries; Tests has four projects; Solutions has three platform solutions. All 14 projects load without unavailable paths.
- Select each intended Gallery startup profile. Confirm Gallery.Flourish.WPF resources/localization and existing user settings, Gallery.Flourish.WINUI3's initial window and Tests.Flourish.Core/WPF discovery. Platform solutions use the renamed project paths.
- Native retains working forms, masks, menus, dialog/sheet, notices, progress and table with no Design asset. Add Design explicitly in a separate consumer and confirm behavior survives its skin.
- Pack/consume Framework alone from a local feed in a separate .NET 10 host, then add Design. Verify static assets and dependencies without sibling source paths, copied Gallery CSS or internal APIs. Framework has no Design dependency; the icon font includes license/provenance. Package configuration does not prove public publication.
- Qualify duplicate Components/Primitives names and check each actual API. Shared still contains both metadata and algorithms; the proposed contract migration is not already implemented.

## Navigation and Gallery documentation

- Open Home, Framework, Controls, Foundations and Examples through pointer, keyboard and back/forward. Home has no secondary rail; the other destinations render only their selected topic/category. Fixed theme/navigation actions stay at the bottom without the old horizontal separator.
- Home's four Rectangle destination cells and Framework's five passive interaction cells wrap automatically; Rows/Columns are unset. Verify links, complete titles/descriptions and no document horizontal overflow at narrow widths. The Foundations theme chooser intentionally uses three Rectangle columns and one row.
- Check the Gallery's fixed browse SVG in its 48px brand box: the 36px icon is clear, proportional and aligned with the rail. It remains smaller than the former 39px clock and needs no network font/icon request.
- Check all eight Controls categories and their Square destination grids. Each of the 77 catalog guides has a third-level entry and exactly five guide-level H2 areas: introduction, control examples, API, scenario examples and scenario code. Nested sample headings/modals must not become extra outer sections.
- Navigate from a leaf to its category/primary landing page, between leaves, to a deeper record route and on direct reload. Most-specific selection, ancestry and route-parameter reset remain coherent; /appearance redirects to /foundations.
- Activate a branch link and triangle separately. The link navigates; the triangle only expands/collapses. Both halves keep selection when the current branch is manually collapsed, and an unrelated expanded branch is not selected. Check aria-current on links and aria-expanded/controls on disclosures. Disabled ancestors block descendants.
- Check full accessible navigation labels despite single-line ellipsis. Third-level siblings share Surface in both themes. Expand narrow navigation, cycle Tab/Shift+Tab, press Escape, click outside and resize while open; focus/inert behavior returns correctly.
- Exercise top-bar hover menus without stealing focus; move into the popup, out of both regions and between menu items. Keyboard arrows/Home/End/Escape, touch activation, disabled/busy guards and click-driven standalone ActionMenu remain usable. Overflow menus/rails still scroll despite hidden scrollbars.
- Inspect guide API metadata: 546 rows and 253 documented defaults across 77 guides. Empty strings appear as double quotes; missing callbacks/templates remain blank. The name column cannot hide. Qualified UniformGrid parameters belong to the outer container, not a child's Shape API.
- Copy complete compiled scenarios for Button, UniformGridButton, an input, data view and layout into a separate host. Imports/models/callbacks agree with execution; Gallery-only registration/namespace lines are absent. Preview=true shows variants; default mode is the working scenario.

## Document scrolling and section navigation

- Scroll a long guide by wheel, touch, PageDown and focus. ApplicationLayout defaults OwnsDocument=true: chrome remains fixed, html/body does not scroll and one shell content track owns page scrolling. Activate real fragment links with path/query/previous-fragment present; only the intended content target moves.
- Activate Save draft repeatedly and copy source near the guide's bottom. Busy/idle/completion must not create an extra document scrollbar or move chrome. Code/wide tables may retain their necessary local overflow.
- Embed ApplicationLayout/ApplicationShell with OwnsDocument=false in a scrollable host. It stays bounded, permits the host's scroll and releases any full-page document lock after navigation/disposal.
- Scroll beyond 96px. Automatic compaction is allowed only when the collapsed page retains at least 24px of scroll range; otherwise the heading stays expanded without an added spacer. Expand below 24px. Repeat short Home content at 1440px/1000px (stable expanded) and 760px (stable compact when the remaining range allows it), wheel at the end, viewport resize and content changes. No repeated expand/collapse loop should occur, and eligibility must be retried after content/viewport changes.
- Expanded title is 50px; compact is 38px beside a 48px icon-only Underline return button, normally 72px high. Long titles/actions may increase height; descenders and accessible return names remain intact. Explicit Compact retains its host-controlled behavior.
- Tab through skip links/navigation/actions. Layout main landmarks remain focusable without an outline around the whole canvas; buttons, fields and grid/table controls keep visible keyboard focus.
- Verify gutter SectionNavigator dots do not narrow content, follow the content scroll and announce the current location. Hover/focus shows real heading text, Escape closes it and labels remain accessible.
- Activate sections with expanded/compact/wrapped sticky headings. Alignment settles below the actual heading; reduced motion is honored. Wheel/touch/pointer/keyboard interruption cancels pending correction; leaving must not cause a delayed scroll.
- Add/remove/rename sections and open nested samples/modals. Discovery excludes nested mains/deeper sections/dialog titles, supports direct H2 and explicit entries, and restores author IDs/tabindex on disposal without stale observers.

## Reading sizes, icons and colors

- Design uses 50/34/28/20/17px roles, with 38px only for compact titles. Art display is explicitly f-type-art and at least 42px. Check long labels, Chinese/Latin glyphs and descenders; Framework-only typography remains native.
- Design defaults Segoe UI; Gallery configures its approved Noto stack without downloading a new text font. Check fallback and zoom.
- Icon/AppIcon/Glyph use the same Material Symbols glyphs/currentColor. Test aliases, unknown-name help_outline, official-name search and icon-only accessible names. Defaults are 26px, primary navigation 24px, tools 26px and information/search 22px; targets/logo dimensions are separate.
- Verify all seventeen live palette roles in Light/Dark/System against the color document. Focus uses Accent; primary navigation on dark chrome uses Accent/Surface, ordinary selected entries Primary/Surface. Third-level panels and popup bodies use Surface.
- Hover, hold, release inside and release outside ordinary/light/dark/primary-filled targets. Persistent selection survives hover; press temporarily uses the matching fixed click role. Disabled/Busy controls do not execute or gain misleading active feedback.
- Danger buttons/grid cells/destructive menu options remain red during hover/press, with approved Danger preview/click and white Surface-light ink. Error/validation semantics remain intact. Popup choices under dark chrome restore ordinary surface preview/click and label Text.
- Configure custom Primary/Accent seeds and verify actual readability. Seeds remain exact in both modes; the host owns suitable contrast. No generated focus/foreground shade or ThemeScope is implied.

## Buttons, grids, boards and identity

- Compare Button's Filled, Outlined, Danger, Quiet, Underline and Elevated variants as text/icon/icon-only, including 48px geometry, 12px radius, 17px text, centered descenders and long labels. Disabled/Busy prevent duplicates; submit works with EditForm; unavailable Href/additional-attribute links do not navigate.
- Ordinary Button inside FormActions keeps its own shape. Samples recommend Filled for the primary action and Outlined alternatives without an implicit first-child default.
- Inspect Rectangle (preferred 2:1; 260px maximum height, preferred 520px width cap) and Square (1:1; 280px maximum side). Automatic occupied tracks wrap and shrink below caps. Test explicit Rows, Columns, both and NarrowColumns separately, including row-only distribution.
- Cells stay connected with 1px separators. There is no rectangular backdrop/shadow behind unoccupied cells in a partial last row. Outlined borders belong to cells; Elevated shadow follows occupied cells. Check per-cell variants, standalone corners, cap overrides and FormActions' separate full-width/96px geometry.
- Test IconSupport unset/true/false, parent setting/local override and nested automatic grids. Equal upper/lower regions remain. The icon aligns at the bottom of the upper region with 4px padding; title/copy begins at the top of the lower region with 4px padding, giving an 8px central gap. Check both shapes, passive/interactive cells, true without an icon, automatic support with an icon and explicit false continuous flow. Busy hides the icon without shifting geometry; long narrow lower content stays scrollable/reachable.
- UniformGridButton preview intentionally shows six desktop columns/two narrow columns in both shapes. Scenario save/reset updates real local state; no inert status/result action remains. Generic fragment-only Button/SplitButton links retain the current path/query.
- Check SplitButton's independent main/disclosure actions, shared selection, 3px gap and 3px inner corners. Disable both, then verify navigation uses the same contract.
- DisplayBoard defaults Dotted/Centered true for controls. Code composes false/false with CodeBlock. First code line starts upper-left; copy remains upper-right outside one edge-aligned viewport. Padding is 24px/16px narrow; consecutive boards/following content have 24px spacing without doubling.
- Scroll constrained boards in both axes. Supported custom scrollbars have 4px thumbs, transparent corners/tracks and no arrows; native fallback remains usable. Full-width layouts/forms/tables retain width while ordinary controls/action groups center.
- Copy multiline Chinese/text/quotes/angle brackets including leading/trailing whitespace. Success shows check for 1.6 seconds, repeated copy/data changes/disposal behave correctly and actual failure is honest. Clipboard JS loads on activation.
- Card/IdentityCard use Primary/PrimaryInk in both themes. Names wrap, grouped dt/dd pairs remain semantic, sidebar toggle and responsive facts work inside a centered board. FactList is removed; migrated callers use UniformGridItem/Button.

## Inputs, validation and dropdowns

- Submit empty required/malformed email examples. Real EditContext/explicit Field errors identify inputs and first-invalid focus; correcting clears them, valid local submit reports success and later editing clears stale success. No invented initial error or unauthorized business write appears.
- Check text/number/date/multiline/checkbox/select values under a decimal-comma locale. Raw native values remain valid, labels/error description IDs resolve and Busy retains entered data.
- Ordinary single-line select-on-focus remains usable. MaskedInput/StandaloneMaskedInput keep clicked caret, explicit selection/Ctrl+A, selected-text visibility, typing/paste/backspace and enhanced oninput bookkeeping.
- Design checkbox targets use the 48px control-height token, including table choices and legacy visible proxy; the invisible accessibility input stays invisible. Native mode remains usable.
- SelectBox, DataSearch, DataTable search/page-size selectors and EditingGrid keep native binding/keyboard semantics and shared dropdown appearance. Unsupported base-select engines retain platform popups; supported engines preserve right-edge alignment with vertical flip.
- ReferenceDropdown/MultiSelectDropdown trigger defaults 240px within its parent, independent popup intrinsic width capped at 480px. Long/short/new labels and filtering must not collapse reservation width. At 760px/below the popup caps/wraps to its field; host width overrides and full accessible selection text remain available.
- SearchAutocomplete demonstrates matching initial data, filtering, keyboard/pointer selection, empty/no-match/data-state changes and clearing without stale selected feedback. Search/create callbacks and remote persistence remain host-owned.
- Expand/collapse menu triangles with reduced motion. ExpansionIndicator stays decorative while the owner supplies focus/name/state; native details/select open states stay synchronized.
- Opt into NavigationGuard, edit, attempt a new destination/query/back/reload and choose keep/discard. Keep preserves the draft; discard follows the latest requested destination without duplicate confirmation. InteractionBoundary locks only its sample.

## Tables, grids and host data

- Check default size 10, 10/20/50/100/custom positive sizes, synchronized pagers, reset on size/search changes and accurate local loaded/filter counts. Gallery range text uses 项 and 共; empty/loading/error remain distinct real states.
- Search all/single columns, sort localized text and typed numbers/dates, retain stable equal keys/null-last and reapply sort after changes. A remote response page is not the complete remote dataset.
- Switch list/cards and visibility; fields/page agree. Test Primitives' own column order/preferences separately. Default preference storage resets with its documented scoped in-memory lifetime.
- Ordinary automatic columns cap at 320px; last visible data column stays uncapped independent of action/spacer columns. Hide/show it, resize manually beyond the cap and use Home to restore sizing by current role.
- Horizontal scrolling stays within the grid. Sticky row-action cells remain reachable, sort-title hover stays inside Quiet Button and menus avoid clipping. Standard menu targets are 48px; row/card action context uses 36px.
- Open display choices near viewport edges; above/below placement and scrolling remain visible. Checkbox changes keep it open, arrows skip disabled options, Escape returns focus and outside click respects its target. Repeat empty/populated table and rerenders.
- RowActionMenu disabled entries do not highlight/execute/navigate/close. Arrow/Home/End skip them; enabled actions update actual state and close. Verify popover support/fallback according to each component.
- In EditingGrid sample and an authorized host workspace, exercise Text/Decimal/Date/Multiline/Select/Masked/Link; raw/display mapping, masks/options/errors remain coherent. Primary/secondary errors are encoded and announced; ReadOnly/Disabled/EditDisabled prevent edits.
- Pointer/keyboard selection, begin/end/cancel, copy/paste rectangles, undo/redo/save/load-more and MeasurementRevision obey host contracts. Select popup Text/Surface stays readable when a selected editor uses inverted ink. Host permissions, parsing, drafts and transactions remain authoritative.
- Hide/show/navigate/dispose grids and reload rows. Stable keys, widths and intended edit lifetime survive without stale DotNet callbacks. Gallery's simple local scenario does not prove every remote transaction or multi-cell paste integration.

## Overlays, notices, progress and lifecycle

- Test each actual Components/Primitives overlay API: title/close, internal scroll, Tab containment, Escape/focus return, Busy refusal and Components CanClose veto. Opening from a row menu returns focus to a useful connected invoker after rerender/removal.
- Dialog/BottomSheet decorative dividers are absent; real outlines/fields retain theirs. Primitives.BottomSheet Actions aligns right within the track, wraps at narrow width and supports standard Buttons. First-open mounting, retained draft/save/Busy/close and unused-sheet deferred import remain correct.
- Notice/StatusNotice share Information Surface/Info text, Success Primary/Surface, Warning Warning background/Warning ink, Error Danger/Surface and Subtle Display board/Muted mappings in both themes. Dynamic results appear fully and retain real status/alert behavior; paragraph/Role/Announce APIs remain intact.
- NoticeTrigger uses the 17px/1em circle: cross, exclamation or check with accessible severity/description. Test pointer corridor, focus explanation, live severity changes, unique IDs and no severity-color replacement on hover/press.
- ProgressBar shows actual value/unknown/Stopped, solid Primary plus the approved white sheen in both modes; reduced motion stops the unknown block as well as the known animation. ProgressRing is 72px/68px with legible 100% and thick stroke; test 0/100/unknown/pause/resume/zoom/narrow containment.
- Check two independent circuits/tabs: appearance/commands/preferences/local demo state do not leak between users. Repeated navigation/disposal releases callbacks, probes, timers and observers.
- Extracted RowActionMenu uses native Popover directly; enhanced ActionMenu/overlays have their own fallback. Verify actual supported browsers, screen readers, forced colors, pointer timing and reduced motion instead of assuming equal fallback from similar names.

## Assets and external host acceptance

- Gallery requests two flattened library CSS assets without @import plus its host-scoped guide stylesheet; Native requests Framework only. Business/page skins stay in the external host, with no removed consumer-specific library entry requested.
- After a fresh Development rebuild/restart and full browser reload, open root/records/controls/patterns and use menus/inputs/dialogs. The import map and normal host output agree; shell, controls and surfaces modules and their dependencies load without stale fingerprint URLs. Dynamic modules decode correctly with gzip or identity for br-only requests; no Development build Brotli endpoint or ERR_CONTENT_DECODING_FAILED appears.
- Publish separately and verify stable/fingerprint CSS negotiate gzip/Brotli, correct decoded bytes and matching-ETag 304. Record browser cold/warm elapsed time separately from HTTP timing.
- In an authorized Colligere environment, check account/workspace/public host palettes and styles after library layers, existing navigation/forms/lists, session boundaries and EditingGrid adapters. Login/invitations/guest sign-out/CRUD stay within host behavior and approved test data; UI class names are not security policy identifiers.
- Runtime host-filtered menus, render slots, labels and table preference replacement remain usable. No generic remote query, universal Culture integration, business-rule migration or public-feed deployment follows from passing Gallery.
- Culture implementation is future work under the [integration plan](culture-web-integration.md); verify per-user lifetimes and runtime labels when that separately authorized work is implemented.

Automated commands and focused scripts remain in tests/Tests.Flourish.Blazor and build/. Select them for the changed boundary. Run counts and temporary artifact paths belong in change records/bug reports, not in this reusable acceptance checklist. Computer Use is not used to test this project.
