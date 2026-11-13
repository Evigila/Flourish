# Native host-driven tutorial board

## Request, gap and evidence

The consumer needs one nonmodal tutorial panel with Primary dotted treatment, H1 copy, segmented milestone progress and a shortcut action. A compact progress entry beside the user must preview progress on hover/focus and open the panel on activation. Completion, eligibility and durable skip decisions belong to business data, not browser clicks.

The source audit found no tutorial entry. Dialog explicitly uses native showModal(), aria-modal=true and a Tab trap; it cannot be renamed or styled into a nonmodal feature. ActionMenu has command semantics and its OpenOnHover accepts generated actions only, not a passive structured progress report. DropdownSurface is a construction helper without this lifecycle. ProgressRing already supplies the correct SVG/accessible progress contract but lacks compact chrome geometry. These are distinct missing capabilities, not permission to create a consumer popup or clone a modal.

## Implementation and boundaries

Components.TutorialBoard is the single new production milestone scenario; TutorialStep resides in Abstract. It owns a compact standard ProgressRing trigger, a passive manual popover overview and an auto popover board. Framework supplies geometry/native visibility; Design supplies the existing color-role skin, dotted pattern and typography. Both CSS entrypoints import their new stylesheet. controls.js owns the browser lifetime and shares existing theme copying/anchored positioning; it does not call showModal(), install a Tab trap, make the background inert or lock scrolling.

Hover/focus never moves focus. Board activation initially focuses the modeless region. Native light dismissal publishes a single close event and preserves an outside target's focus; focus owned by the board returns to its invoker. Explicit host closure does not publish a second native-close event. DOM removal/disposal release observers, listeners, timers, popovers and temporary palette overrides. Missing native Popover support fails explicitly rather than activating a compatibility renderer.

Steps supplies stable unique keys and encoded content with host-verified Completed/Disabled flags. Selection changes only ActiveStepKey; a disabled milestone remains inspectable but cannot dispatch its shortcut. The completed count and ring derive from the supplied snapshot. A destination uses ordinary Button native navigation without OnAction; an operation dispatches OnAction once, with concurrent dispatch refused. OnSkip is a persistence request, not assumed persistence or completion; close is not skip. Tone defaults to existing PresentationTone.Primary, Dotted to true. Explicit text overrides remain literal while omitted defaults follow the scoped text provider.

ProgressRing adds Compact/ShowValue as explicit variants of its existing renderer. Default geometry/text remain unchanged. The compact ring preserves aria-valuenow even when the visual percentage is omitted. No dependencies, host skin, separate progress SVG, public generic-popup family, WPF change or legacy alias is introduced.

ComponentUsageCatalog, ComponentCatalog, ParameterMeaning, CatalogSections and complete English/Chinese/Portuguese catalogs register the current production entry. The executable TutorialBoardSample separates shortcuts from host verification. /examples/tutorial is a second-level Gallery example; /controls/overlays/tutorialboardsample is its reflected API/sample page. The intentional exported-component Gallery guard increases from 80 to 81.

## Verification status

The parent agent performs repository builds/tests serially. Its Release build reports zero warnings/errors, Framework passes 426/426 and Gallery passes 7,388 checks plus 124 post-event cases. The first Gallery pass stopped at the intentional 80-component count guard; the intentional 81-component inventory correction passed the rerun. No automated or static result is claimed as measured physical browser acceptance.

TutorialBoardChecks registers ten focused checks of actual rendered controls/events, state projections, native navigation versus side effects, no fabricated completion, disabled/busy refusal, invalid input and module disposal. tutorial-board-dom.mjs passes all twelve checks against the actual controls.js export for hover/focus/keyboard preview, native open/close synchronization, focus ownership, shared theme/positioning, repeated synchronization, teardown and asset/CSS boundaries. Gallery's full three-language render gate includes TutorialExample and the new executable sample. The tutorial controller is now registered in ReleaseSettings.JavaScriptTests instead of depending on an ad hoc invocation.

The complete configured seventeen-file JavaScript gate initially reported 107/108 Node TAP cases: palette-checks rejected two new literal black shadow values. Both tutorial surfaces now use the existing --f-shadow-popup role without a new token or relaxed guard. The full rerun passes 108/108 TAP cases with zero failures/skips; its six self-executing DOM suites additionally report 138 internal scenarios, including the tutorial twelve. These nested totals describe the same run and must not be misrepresented as independent repeated executions.

The remaining CheckScripts gates pass: 21 CSS bundle checks, 194 CSS SDK integration checks, 95 launcher checks and 21,906 culture integrity checks covering 1,530 Gallery keys and 288 Framework keys. The CSS scripts compile only isolated temporary SDK fixtures, not the Flourish repository. The first CSS SDK attempt was rejected by the sandbox's virtual temporary-directory NuGet access permissions; a narrowly authorized isolated real-Temp rerun passes 194/194. Launcher output contains mocked restore/build/run plans and explicitly reports that no project was built or started. No Aspire database or instance was touched. The ordinary sandbox Git diff check could not access Flourish's repository and remains for the parent agent's final check.

The final approved target is the separate local `1.1.4-preview.tutorial.3` candidate retaining prior Field/selection/PageBody repairs. Earlier `.1`/`.2` candidates are preserved rather than overwritten: `.2` includes the late attachment-lifetime guard, and `.3` includes the role-compliant shadow correction. Final package/consumer verification remains owned by the parent agent. Public 1.1.3 is unchanged and no public publication, source-project replacement, commit, push, Azure operation or live restart was performed by this bounded implementation.

## Manual acceptance

1. Open Gallery /examples/tutorial. Preview by mouse hover and keyboard focus; verify that the ring and list reflect the host's completed count, and focus stays on the trigger during preview.
2. Activate the trigger. Verify H1 copy, Primary dotted background, centered segments and right-bottom Elevated shortcut at wide/narrow widths in light/dark themes. The page must remain usable outside the board with no inert layer or scroll lock.
3. Dismiss by close, Escape and outside click. A clicked outside field keeps focus; keyboard dismissal returns owned focus to the trigger. Reopen without stale preview or duplicate state changes.
4. Select milestones and invoke shortcuts: neither completes them. Use the separate host verification action to update completion. Verify a disabled shortcut is unavailable and a long-running operation dispatches only once.
5. Verify native navigation shortcuts do not invoke the host action callback, skip invokes host persistence only, and simply closing never skips. Navigate away/back while a preview is open to check observer/timer cleanup.

Acceptance is user-operated; no Computer Use was used.
