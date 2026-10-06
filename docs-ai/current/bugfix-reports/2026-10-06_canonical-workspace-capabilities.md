# Canonical table gaps and duplicated consumer UI ownership

Status: required Workspace production capabilities collected and consumed; compatibility debt and manual acceptance remain explicit. The [change record](../currentproject-changelogs/2026-10-06_003548_complete-canonical-workspace-capabilities.md) carries final package/build/test evidence.

## Symptoms and cause

The host used a generic table facade that selected the independently implemented Primitives.DataTable, while Gallery's primary entry was Components.DataTable. Different column/search/pager contracts and missing canonical templates, retained editors, native actions and remote query slots prevented a truthful full migration. Class names and an installed package were insufficient evidence of production adoption.

Static PageContents and raw return-to-top composition did not provide the active directory/scroll/focus behavior. The host still rendered its own chart, spreadsheet, static directory and copied input listeners. Scenario-specific controls such as identity cards and form action strips had also been substituted for metric/grid-button layouts.

The two library tables were not aliases. This correction completes the canonical capabilities required by the current consumer and removes its alternative path; it does not misrepresent the old compatibility renderer as already merged.

## Corrections and regression evidence

Keep one canonical TableColumn/TableSearchRequest/rendering pipeline with explicit retained editing, native/remote transport and business template slots. Progressive enhancement is restricted to visible authorized SSR values and existing native actions, without extra hidden payloads. EditingGrid's composite cell remains inside its production keyboard/clipboard shell. Shared DataSearch is used by canonical tables and the legacy search adapter.

Column drag had a real directional indexing defect: removing the source before resolving the destination changed insertion semantics. Both directions and fixed slots are tested. Manual width state also lagged accessible values and could be lost when the declared column was hidden or the view had no table col. C# state and shared JS now retain/reset valid widths immediately; finite bounds and unknown-key rejection prevent invalid cache/CSS/ARIA state. New mocked-DOM cases cover recovery from Cards and hidden columns.

BackToTop, active navigation and LineChart are public scenario entries with actual Gallery samples and lifecycle/value tests. Their geometry, roles, accessibility and teardown remain in the library, while hosts supply values and native business protocols. Component/API documentation classifies supported scenario entries separately from construction helpers and compatibility families.

The consumer's copied form-input-behaviors.js omitted the library's masked-input focus guard and capture-phase normalization. It is deleted. The host bootstrap now imports the authoritative module through the same ImportMap-resolved specifier as library input bindings, preserving its CSP nonce and avoiding duplicate module identities/listeners.

## Boundaries and limitations

No business schema, authorization, session, data store, native antiforgery, route or transaction ownership is transferred to Flourish. Hosts may compose production controls with business values and commands; they may not invent a generic renderer, cloned skin/controller or pass-through API. Framework-only, Design-enabled and Culture bridge configurations remain verified through real isolated NuGet consumers.

Primitives.DataTable remains independent compatibility debt. Its namespace also contains supported production scenarios, so indiscriminately forbidding all Primitives would remove valid masks, references, multiple selection and EditingGrid. Choose the production entry by capability and documented preferred API.

Final full release/package gate passes. Colligere's final suite retains two existing order-contract failures and 87 unconfigured database skips; an earlier random-ciphertext short-substring assertion also fluctuated without code changes and is recorded in its host report. Those issues were not concealed by changing business assertions or claiming every test passed.

## Manual regression

After the user's own restart, compare actual Gallery and host tables in both views/themes, narrow/zoomed layouts, column drag/resize/reset, retained edits and native opening. Exercise chart series, active directory, top-button focus/reduced motion, spreadsheet tags/clipboard and one input-module listener set after navigation/reconnect. No Computer Use or real authenticated/database visual workflow was performed.
