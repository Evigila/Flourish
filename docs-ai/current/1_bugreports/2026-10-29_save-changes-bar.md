# Persistent save/discard bar

Recorded 2026-10-29 11:37:08 America/Sao_Paulo (UTC-03:00). Status: source, automated checks and local package verification complete; physical browser acceptance remains manual.

## Requirement and cause

Editable pages needed a persistent bottom notice with Save/Discard and navigation blocked until the draft was settled. The existing NavigationGuard intentionally offers a discard-and-leave confirmation, so reusing it would grant the wrong behavior. This is a new reusable library scenario, not a reproduced browser data-loss claim or a replacement for server validation.

## Implementation

SaveChangesBar exposes HasChanges, Busy, OnSave, OnDiscard and optional Message/SaveText/DiscardText. It composes the standard Primary and Quiet Buttons, a polite live status and three-language defaults. The page owns its saved snapshot, validation, save and restoration; there is no automatic dismissal or confirmation shortcut. Internal callback execution keeps navigation blocked even if the host clears HasChanges early, and finally releases busy presentation after success or failure without discarding the draft.

An active managed location-changing handler blocks routes, query changes and same-URL refresh; only a new nonempty fragment in the same document remains allowed. The scoped browser module captures native links and explicitly marked data-f-edit-interruption actions, aggregates multiple roots and restores owned disabled attributes/styles on release. Browser close/reload only uses its native beforeunload prompt and remains subject to browser policy. Existing NavigationGuard/Dialog behavior is unchanged.

Framework owns responsive fixed placement, safe-area offsets, scrolling and measured bottom clearance. Design owns normal semantic paint. ResizeObserver measures the actual wrapped panel; only the root's own custom property is changed, with previous state restored on disposal. No host skin, body-padding overwrite, focus steal, new animation or external dependency is added. Place the component last inside the editable page.

The public catalog and Gallery contain one reviewed scenario/sample. The complete maintained tree is 721 files/121 directories, adding exactly the Razor component, two CSS partials, browser module, Gallery sample and C#/Node checks.

## Verification and delivery

- Twelve-project Release: zero warnings/errors.
- Framework 494/494; Gallery 8750/8750, including 474 post-event and 443 changelog checks; Culture bridge 27/27.
- Focused save-bar Node 24/24; CSS bundler 21/21. Managed checks cover clean/busy state, callbacks and retry after failure, fragment/query/refresh boundaries, lifecycle and disposal during import; browser mocks cover multiple roots, marked actions, native links, attribute/clearance restoration and native exit prompting.
- Six .15 archives in artifacts/save-changes-bar-20261029/nuget; 14 unique delivered entries (13 static resources and one Framework DLL) match source/Release bytes. Twelve graphs retain 33 external package/version pairs. See [current hashes](../1_dependency.md#current-15-archive-and-asset-evidence) and the folder's build-final.log, framework-tests.log, gallery-tests.log, culture-tests.log, node-tests.log, css-bundle-tests.log and package-delivery.json.

The local candidate is 1.4.4-preview.shell.20261027.15; .14 remains historical. No Computer Use, physical-browser measurement, public publication, commit or push is claimed. Interactive lifecycle/browser enhancement is required for navigation protection; this is not authorization or a persistence guarantee. Unmarked business-specific nonnavigation actions remain the host's responsibility.

## Manual acceptance

1. Edit then restore a field; verify the bar appears only for actual changes and disappears after the page confirms save/discard.
2. While dirty or saving, try primary/secondary links, query changes, same-page refresh and marked interruption actions. Verify they stay blocked while a distinct same-page anchor works. Test save failure/retry and discard restoration.
3. Repeat on narrow screens, zoom, long Portuguese/Chinese labels and safe-area devices; confirm the final field remains visible and both actions are keyboard-accessible without focus being moved automatically.
4. Verify multiple active bars do not release one another, navigation becomes available when all are clean, and browser close/reload gives the browser's native prompt where supported.
