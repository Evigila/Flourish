# Converge dialogs and simplify Gallery

Local timestamp: 2026-10-06 14:18:57, America/Sao_Paulo.

## Decision and changes

The user requested removing specialized ConfirmationHost in favor of Dialog with ordinary Button, reviewing ReconnectDialog and overlapping controls, and reducing descriptions inside sample backgrounds. The [generic control audit](../generic-control-convergence.md) records the complete review scope, nine deleted entries, genuine remaining convergence candidates and distinct responsibilities.

ConfirmationHost/ConfirmationService/DI and shell mounts are deleted. NavigationGuard and real Colligere callers directly use Dialog.ShowAsync and CloseAsync results. Explicit acceptance alone advances business actions; busy/veto, cancellation, disposal, pending-message and concurrent-request policies are preserved. EditingGrid's unused browser-confirm method was also deleted.

ReconnectDialog and StaticDialog are removed after adding browser-controlled pre-rendered Dialog, keyed DialogView content, generic view/action visibility and the shared native modality/focus helpers. Host IDs, circuit retry/resume/reload and visibility protocols stay outside the library. Dialog owns the entire generic modal lifecycle; no reconnect stylesheet or alternative controller remains.

BottomSheet becomes DialogPresentation.BottomSheet. StandaloneNumberBox becomes StandaloneTextBox Type=number with native constraints. RecordListPage becomes direct PageBody/PageHeading/Section. AccessSurface becomes ContentSurface DocumentFlow plus PresentationBand FullHeight. SelectionDropdownSurface is private ReferenceDropdown markup; unused ToggleIndicator is removed. Related styles/imports, catalogue entries, Gallery guides/samples and tests are migrated or deleted.

Public component coverage changes from 89 exports / 723 parameter rows / 316 defaults to 80 / 683 / 307. Existing semantic Scenario entries are not mechanically reclassified. Shared Reconnect_* resources remain after automatic approval review rejected an extra blanket key deletion without evidence that all external protocol consumers were absent; the retired renderer and service remain deleted. This safer alternative does not block the requested convergence.

Thirty-four samples use SampleFor.Description for technical explanation outside DisplayBoard. ComponentGuide preserves paragraph breaks; boards retain actual controls, fictional scene data, short necessary hints and status/validation. The independent /examples/overlays/reconnect page uses the actual generic Dialog with fictional state transitions.

AGENTS.md and the current architecture/API guide record the new general-control direction and explicit supersession of the previous host/service recommendations. Earlier append-only records are unchanged.

## Final verification

- Blazor C# suite: 356/356. Actual Dialog result/view fixtures cover acceptance, cancellation, busy/veto, concurrency, disposal, SSR and encoded/localized content. Evidence: the retained temporary flourish-generic-controls-csharp.log.
- controls.js DOM suite: 75/75, including nested close ownership, keyed action visibility, empty wrapped footer and focus repair. Evidence: the retained temporary flourish-generic-controls-dom.log.
- All eleven configured JavaScript groups pass: controls 75, palette 18, action-menu 13, interaction-origin 8, clipboard 5, split-menu 5, section-navigator 25, offers 12, heading 7, measurement 6 and data 6. The last data gate is artifacts/dialog-convergence-data-js.log.
- CSS bundle 21/21 and SDK assets 194/194: artifacts/dialog-convergence-css-bundle.log and dialog-convergence-css-assets.log.
- Isolated Gallery Release and native test application builds: zero warnings/errors. Evidence: artifacts/dialog-convergence-build.log and dialog-convergence-native-build.log.
- Six existing local 1.1.0 candidates packed in dependency order and verified. Evidence: artifacts/dialog-convergence-pack-*.log and dialog-convergence-package-set.log.
- Four isolated NuGet-only consumer modes pass 99 package graph, registration, SSR and published-asset checks. Evidence: artifacts/dialog-convergence-package-consumers.log and artifacts/package-consumers/12eecf02427f446c82ecca9be43685a1.
- Colligere independently restores the final six archives with matching SHA identities, builds without warnings/errors and passes 371 focused C# plus four reconnect protocol cases. Its own repository records the scope and logs.

Core and Culture bridge source were unchanged in this follow-up; their earlier green checks were not rerun or represented as fresh results. No physical browser geometry, authenticated database workflow or complete Colligere suite is claimed.

## Manual acceptance

1. Restart Gallery and Colligere using their existing launchers to select current local candidate bytes. No running application was terminated.
2. Verify confirmation Cancel, close/X/Escape and explicit acceptance, including deletion, discard and unsaved navigation. Busy/veto must preserve the pending operation; concurrent requests must not replace its message.
3. Check centered and BottomSheet presentation, retained inputs and focus returning to the trigger, including keyboard menus.
4. Test disconnect/retry/countdown, failed/rejected sessions, paused/resume and document visibility in Colligere. Protocol IDs/actions and native reload behavior must remain.
5. Inspect reduced Gallery copy and paragraph breaks outside boards, numeric constraints, full-height login/account documents, light/dark and narrow widths.

No new dependency, public publication, deployment, database mutation, Git commit/push or Computer Use occurred. Previous uncommitted work remains intact.
