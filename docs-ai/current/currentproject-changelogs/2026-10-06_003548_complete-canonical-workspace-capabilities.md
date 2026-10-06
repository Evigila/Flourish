# Canonical Workspace capabilities and production ownership

The canonical Components.DataTable now supports the actual Registry/Pool/Worklist consumers without a host renderer or another table family. Shared DataSearch, BackToTop and LineChart are production entries, and existing EditingGrid supports composite template cells. See [the ownership/capability report](../bugfix-reports/2026-10-06_canonical-workspace-capabilities.md) and [API organization](../component-api-organization.md). This supersedes the earlier Account-only pending-consumer limitation; it does not claim every compatibility API is merged.

## Changes

TableColumn appends optional default visibility, card role, reordering, independent typed sort/search and template metadata after its original constructor arguments. DataTable retains one typed renderer and adds purpose-aware retained editing, business template/bulk slots, controlled remote search, limited/reset state, native guarded opening and explicit display/sort preferences. Local algorithms are available through the shared TableData helper; consumers do not copy sorting/filtering/paging into their own component contracts.

Registry keeps ordinary browse/display tools. Pool/Worklist keep all supplied keyed editor rows and hide off-page rows so pagination does not destroy drafts. Bulk busy/readonly restrictions, selection and opening availability are explicit. RowOpenHref is a real GET, never a fictitious business callback; native POST uses the existing standard menu slot and unique eligible submitter with strict incompatible-parameter rejection.

Progressive is the same canonical table enhanced from already-visible authorized SSR text. It rejects hidden defaults, separate search/sort payloads and editing, and embeds no extra business-record JSON. Existing native actions, no-JavaScript readability, search/sort/paging, List/Cards, column display/width and disposal are covered in the shared controller, not a third renderer.

Two-way column drag now resolves the original destination index before removal and preserves fixed slots; the progressive controller follows the same movable-slot shift, not swap semantics. Manual widths immediately update ARIA, remain finite/clamped and are cached for declared hidden columns or Cards without a current col element. Reset survives a later return to Table. Unknown keys and NaN/Infinity fail closed without future-key cache pollution.

Components.DataSearch composes real Field and StandaloneSelectBox/TextBox with a same-row business filter slot. It validates keys, disabled input/callbacks and maximum length; input/change propagation cannot dirty a containing business form. Primitives.DataSearch is now a thin adapter to this renderer. Primitives.DataTable still has its independent compatibility implementation and contract; that remaining debt is explicitly stated and Colligere no longer calls it.

NavigationSurface provides one active SectionNavigator over its actual scroll track. BackToTop uses an Elevated icon Button, shared region-scroll lifecycle, threshold/focus/reduced-motion behavior and native fragment fallback. The old raw return-to-top paint is removed from both shell paths. LineChart owns theme-aware geometry, stable labels/decimal values, independent-scale validation, exact-value accessibility and empty states.

EditingGrid gains GridEditorKind.Template and CellTemplate/GridCellContext within its existing cell shell and keyboard/clipboard controller. Composite tag selection retains complete cell values, readonly/disabled boundaries and errors; an absent required template fails closed. It is still a specialized spreadsheet rather than a new DataTable mode.

Gallery registers all 101 controls, the three new production entries and extended DataTable/EditingGrid scenarios. Classification and parameter descriptions expose capability differences rather than relying on a Primitives namespace ban. Supported masks, references, identity, multiple selection and spreadsheets remain valid production scenario entries.

## Verification

- Final complete scripts/Test-Release.ps1: exit zero; Release build zero warnings/errors; Core 367, Blazor 301/301 and Culture bridge 12 passed. Evidence: artifacts/workspace-controls-release-accepted.log.
- All ten configured JavaScript files pass, with 53 Node-runner checks and 55/55 controls-DOM, 25/25 directory/top, 8/8 row-menu and five interaction-origin checks. CSS bundle 21 and SDK integration 194 pass.
- Catalog/Gallery contract audit: 101 components, 799 parameter rows, 355 documented defaults. These figures establish coverage, not equivalence of independent compatibility implementations.
- Six local 1.1.0 candidate packages verify dependencies/assets; FrameworkOnly, MetaNative, MetaDesign and MetaCulture isolated NuGet-only consumers pass 95 checks. Evidence: artifacts/package-consumers/a9403fa087014a38982a95724d23def8. Existing package README advisories are not compilation failures.
- Final Framework archive SHA256: B2F26FDD5DD19B504A7971261553F2993218F430E21BC2254C753F1D495768B2. Design: 7CBD5A6A4C183081E247DAC0FAA4D0B46CD693A567C3938E17D424A973A1A845.
- Colligere restores actual candidates into artifacts/nuget/d086142b2ac8524d2a58c55e60bfac4e and verifies restored archive SHA256 before Release build; no source-project shortcut or cache purge is used. All 25 maintained record-table instances call the canonical entry directly. Generic host wrappers/renderers and copied input behavior are removed; native protocol/business/image/circuit scripts remain host-owned.
- Colligere full Release build has zero warnings/errors. Full final tests: Web 1664 passed, one pre-existing order wire failure, 85 database skips; Core 308 passed, one pre-existing snapshot failure; Application 221 passed; Provisioner 51 passed, two database skips. Total 2244 passed, two failed, 87 skipped. UI/consumer regressions pass; the application suite is not entirely green. Original order model/schema assertions remain unchanged.
- All eight host JS files pass against restored package assets. Twelve HTTP checks verify actual Gallery examples and served Framework/Design/data/directory/top/input resources; host evidence: Colligere/artifacts/workspace-controls-gallery-http.log. The task-owned temporary Gallery is stopped.
- No Computer Use, Debug build, user-instance restart, live database migration/workflow, external dependency addition, business/security-rule change, commit, push, public NuGet upload or deployment. Essential and desktop source are unchanged for this task; previously uncommitted display/access changes are preserved.

## Manual acceptance and remaining scope

Use Gallery and the restarted host in Light/Dark, narrow widths and 200 percent zoom. Test both drag directions, fixed columns, width/reset while hidden or in Cards, independent named states, local and remote search, retained off-page editors, busy/readonly guards and native GET/POST opening once only. Check active section tracking, threshold/focus/centered top icon, delayed import, navigation/reconnect/disposal and reduced motion.

Test chart quantity/currency, empty series selection and screen-reader exact values. Exercise spreadsheet keyboard/copy/paste/composite tags and validation without changing host transactions. Verify static/native form focus and masks use one ImportMap-resolved library input module, not copied listeners.

Physical geometry and real authenticated/database workflows require user acceptance. Complete independent compatibility table/dialog/identity API convergence remains a later library migration, not a justification for consumers to retain clones.
