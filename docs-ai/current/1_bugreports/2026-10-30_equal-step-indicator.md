# Equal process-step indicator

Status: implemented as local `1.4.5-preview.3`; no public release.

## Requirement and ownership

An order requires a read-only current-state display that spans its content container and divides all supplied steps equally. Existing TutorialBoard and navigation controls own different interactions and cannot imply this behavior. `StepIndicator` now owns this presentation; the host continues to own its actual business states, transitions, cancellation and labels.

Public API: `IReadOnlyList<StepIndicatorItem> Items`, required accessible `Label`, optional `CurrentKey` and `Id`. The public contract record is in Blazor.Abstract and the renderer remains in Blazor.Framework. Null CurrentKey highlights nothing. Empty Items render nothing. Null/malformed items, duplicate or empty keys, empty item labels, missing accessible label and unknown current keys fail parameter validation. Key comparisons are ordinal. No click callback, navigation target or inferred completed state exists.

The semantic ordered list marks only the current item with `aria-current="step"`. Framework supplies equal CSS grid tracks, wrapping, native system-color fallback and responsive padding. Optional Design supplies its existing accent/ink pair, surface and border roles. Gallery has an executable three-language example and parameter/use guidance.

## Diagnostic findings

Actual CSS measurements initially found that fixed 12px inline padding widened 17 segments inside a 300px container. Inline padding now shrinks to a fraction of the segment width while preserving the ordinary 12px maximum. Real computed geometry verifies 1, 3, 5 and 17 steps at 300px and 997px, including long unbroken captions, equal widths and heights, container width, no horizontal overflow and noninteractive markup.

A fresh complete build also exposed an inherited test-only omission from the preceding CollectionView preview: SplitButtonChecks rejected CollectionView's SSR display selector as unknown. The bounded test now explicitly recognizes that its unrelated split/action fixtures are not counted collections. Production CollectionView behavior was not changed. Earlier preview.2 verification history is retained, not rewritten.

The first package-consumer invocation supplied a relative feed path. NuGet resolved that path under the generated fixture and failed NU1301 before compilation. The final invocation used the absolute frozen feed and passed; both logs remain available. No archive was overwritten to address either diagnostic.

## Verification and provenance

- Twelve-project Blazor Release build: zero warnings/errors.
- Framework: 512/512; Gallery: 8939; catalog: 86 components, 802 parameter rows, 378 documented defaults.
- Node regressions excluding separately executed action/geometry: 289/289. Action controller and installed-headless-browser geometry: 43/43, zero skipped. This is automated testing; no Computer Use was used.
- Culture integrity: 25568; changelog: 197; CSS bundle: 21; CSS SDK integration: 194.
- Six local packages: identities, internal version edges, expected assets and 151 fresh-cache consumer checks passed. All four consumer modes explicitly compile and render the new public contract. The 33 external package/version pairs remain equal to preview.2.
- Full maintained tree: 727 files / 121 directories. Full audit state remains unchanged.

The immutable feed is `artifacts/step-indicator-20261030/nuget`. `provenance.json`, `package-sha256.json`, `source-files/` and `source.patch` in its parent identify baseline `7c6f4e2f939ba73e4d67ee532b38256c306dc4be`, 25 changed source/test/build files and all six archives. Source `Directory.Build.props` stays at 1.4.4; explicit CLI overrides produce 1.4.5-preview.3. Framework and Design packaged CSS match their actual Release bundles byte for byte. Preview.1 and preview.2 archives remain available. No external dependency was installed/upgraded, no host project was edited, and no Git commit/push/tag/public publication occurred.

## Manual acceptance checklist

1. Open Gallery's StepIndicator sample in English, Chinese and Portuguese. Confirm all visible labels and guide text switch language.
2. Observe a three-step process at its middle and final states. Only the supplied current segment is highlighted; earlier segments remain neutral.
3. Observe the example with no current state. No segment is highlighted, and assistive technology can still read the list and its accessible label.
4. Resize a host container and try three/five steps with long localized captions. Segments remain equal-width, full-width and wrap naturally without navigation behavior.
5. In a host with cancellation, supply `CurrentKey=null` and an appropriate host-owned accessible label or Notice. Confirm no ordinary process step is falsely announced as current.

Human visual/assistive-technology acceptance is not claimed as completed by the automated checks.
