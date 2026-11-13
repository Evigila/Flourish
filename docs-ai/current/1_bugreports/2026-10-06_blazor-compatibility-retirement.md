# Blazor compatibility retirement and control-ownership audit

Date: 2026-10-06, America/Sao_Paulo.

## Symptoms and scope

Colligere's login menu painted different hover widths for differently sized labels. Record pages still exposed a forbidden sorting toolbar and an incorrect Display interaction despite referencing Flourish packages. The user authorized a pre-production breaking refactor: no old public aliases, adapters, duplicate renderers or legacy UI/browser paths. Desktop projects, persisted business data and authentication protocols are outside this Blazor implementation boundary.

## Causes and evidence

The split menu uses the real production SplitButton, DropdownSurface and Button, not a host clone. Generic optional Design Button width was content-sized; the native menu had no stronger scoped rule stretching each child to its common column. The library now owns that structural rule for commands and links, independent of text length and hover/focus state.

The Framework previously exported independent Components and Primitives table families with different column/request contracts. Inventory classification as Compatibility documented the overlap but did not remove it. Consumer migration likewise retained old visual wrappers/hooks and explicitly enabled sorting controls. Package installation was therefore not sufficient evidence of a single production renderer.

A retained Colligere Debug instance started at approximately 00:56 on 2026-10-06. Its actual HTTP Framework bundle returned status 200 and SHA256 2D28DD380098275E4EFAF60F6709457B59717A97B710885C1EF68612D2D7FBB3, without the new menu-width rule. Its manifest points to the older d086142b2ac8524d2a58c55e60bfac4e package cache. This is a second, independently verified reason rebuilt source cannot be accepted through that running process. It does not excuse the actual source/API defects above.

## Implementation

- Delete the independent Primitives table/search implementation and DataColumn/DataSearchRequest/filter/sorter contracts; keep Components.DataTable/TableColumn/TableSearchRequest/TableData as the only interactive-record pipeline.
- Delete ShowSortControls and its toolbar/query paths. Header sorting remains current behavior. Display uses whole-item native dragging and keyboard ordering, with fixed slots, busy state and Cards restrictions in the canonical controller.
- Remove retired form/title/notice/switch/identity/icon renderers and Gallery entries, old shell registration/builders/NavigationGroups adapters, enum/parameter/icon aliases, ThemePalette and theme variable bridges. Public component usage is General, Scenario or BuildingBlock only.
- Dialog owns lazy first mounting, retained content, native top-layer close/focus lifecycle and asynchronous row-action invoker restoration. BottomSheet is its presentation variant. Remove the second bottom-sheet controller and obsolete browser portal/backdrop paths.
- Require current native/observer/media APIs for the applicable features; preserve current responsive/reduced-motion behavior, SSR and asynchronous disposal. Clipboard has one navigator.clipboard path, without execCommand.
- ReferenceDropdown uses Value/ValueChanged. ShellHeader and layout Patterns use Class. Update actual callers, Gallery, metadata and event/lifecycle tests, not compatibility facades.
- Add production ConfirmationHost/ConfirmationService, FormGroup, InlineActions, ImagePreview, AttributionFooter, CopyText and ReconnectDialog. Reconnect owns native protocol-state presentation; the host retains connection transport only. Nine reconnect defaults support English, Chinese and Portuguese.
- Move the configured shell example into /examples/shell rather than nesting a second document shell. Reconnect's Gallery preview does not mount a duplicate fixed protocol ID.
- Strengthen both repositories' AGENTS.md rules and replace active compatibility guidance. Historical records remain unchanged.

The retired tracked source/styles/samples are recoverable through Git. No user data, database migration history, secrets or running user processes were deleted.

## Verification

Complete scripts/Test-Release.ps1 passed. Evidence: artifacts/breaking-ui-release-2026-10-06_024227.log.

- Blazor/Gallery Release solution: zero warnings/errors with TreatWarningsAsErrors.
- Core: 367 passed; Blazor executable regressions: 325/325; optional Culture bridge: passed.
- Export/catalog/Gallery audit: 88 controls, 707 parameter rows, 309 documented defaults.
- Eleven configured JavaScript files: 62 Node-runner checks passed; their embedded suites include 57 DOM, 8 interaction-origin, 8 row-action and 25 section/return-top checks. These counts are not additive independent test totals.
- CSS bundle: 21; CSS SDK integration: 194.
- Launcher: 95 checks under the release gate and separately under Windows PowerShell 5.1; neither check starts a project.
- Six local 1.1.0 package candidates and required dependency/assets verified. Four isolated real NuGet consumers pass 95 checks, including Framework-only, meta without Design activation, Design and Culture activation.
- Framework DLL SHA256: E259F219897421EBE72D8DF3CB54464AF7BED01236367E2D67AA9418677F6D3D.
- Design DLL SHA256: 7A2069D79FA2C7A5456DE4FF59C29DB37A81996D40D0F89CD514CE5DA83D1D43.

The first full gate caught an incomplete browser-reference fixture for a new dropdown event test. The fixture now replays actual element-reference capture with WebElementReferenceContext and strictly verifies focus; no exception was ignored and production callback behavior was not weakened. The final complete gate passed afterward.

## Limitations and manual acceptance

No Computer Use, public NuGet upload, Git commit/push, deployment or desktop release was performed. Browser geometry and authenticated application workflows remain manual. Colligere's new-package build/test evidence belongs to its separate report. Its retained Debug instance must be restarted by the user before accepting these candidate bytes.

Check different-length menu hover/focus widths and the square triangle; header-only sorting; whole-item Display drag and keyboard order; List/Cards/width/preference behavior; draft retention and cancel/busy confirmation; fixed headers and active directory/return-top; scoped light/dark and language changes; Gallery shell/login/account cases; and reconnect/retry/pause/resume state presentation.

Stored-order legacy decoding and fiscal legacy-review workflows are nonvisual business contracts. Their removal requires an explicit decision about existing data semantics; do not treat database history or legitimate domain validation as a UI adapter.

## Final convergence and accepted gate, 03:26 local

The earlier verification above predates this final follow-up. Further audit identified a deleted icon alias still used as the default, a missing primary-navigation size selector, dead table/menu CSS arms, the now-redundant RowActionMenu and Surface's old arbitrary-button top controller. These are now removed or corrected in the production library, not patched in Colligere.

ActionMenu exclusively supports generated commands and native ChildContent. One controller rejects disabled/aria-disabled/hidden/inert actions, preserves keyboard/focus and remembered asynchronous invokers, and dismisses commands after dispatch while keeping multi-selection disclosures open. Current circuit cancellation/disconnection and late import/teardown tests exercise actual ActionMenu; arbitrary errors still propagate. Menu disabled links no longer paint enabled hover/press states. No RowActionMenu forwarding wrapper, controller, style or Gallery entry remains.

Surface synchronization now takes root/stage only; BackToTop owns its own current lifecycle. Native observer/media requirements and reduced-motion behavior remain intact. Icon defaults use description, primary navigation shares 24px, and ordinary icons retain 26px. Remove IE-only -ms-overflow-style and all dead table/menu selector adapters. The whole-style negative regression caught a remaining danger-action selector and prevented acceptance until cleanup was complete.

Complete scripts/Test-Release.ps1 passed again. Final evidence: artifacts/breaking-ui-release-final-2026-10-06_032000.log. Core 367; Blazor 330/330; Culture bridge 12; 87 exported controls/705 parameter rows/308 defaults; eleven configured JavaScript files with 62 Node-runner checks and embedded 13 ActionMenu/57 DOM cases; CSS 21/194; launcher 95; six 1.1.0 local candidates and four isolated NuGet consumers/95 checks. Release builds remain zero-warning/error. Embedded suite counts are not additive independent totals.

Final Framework SHA256 is 8396860710ED412BD1028AAB703B11FA39EBF0301D9A7CA54B90708BA4FBD601; Design is 6A61CD8C7564217589925646EEEABE7A4438BF0C30FFD1D2C771BE4C56FE2A2C. Both match the restored NuGet cache and Colligere Release output byte-for-byte. Candidate Framework CSS bundle SHA256 is 41D663089A9DF2C1C1B2511FA6FCB6A351C04F269B9DE8C3539587CB715AC3C6. The final isolated consumer evidence is artifacts/package-consumers/01f49d4176ca4ea29c7141ed9939a717.

Seventy tracked retired source/style/sample/test files are deleted or renamed and recoverable through Git; no user data or running process was deleted. git diff --check passes. Final consumer verification is recorded separately: all UI and eight package-asset JS suites pass; the full Colligere suite still has its two previously recorded nonvisual order-contract failures. No commit, public upload or push occurred.
