# Counted collection values in table cells

2026-10-30, America/Sao_Paulo (UTC-03:00). Implemented and verified as local candidate 1.4.5-preview.2; no public release.

## Requirement and ownership

A business row can contain several products or attachments. Replacing the cell with a single label loses detail; a count must expose the complete collection through a weak hover popup. The user explicitly requested a reusable Flourish control. The library must not assume business identities, permission rules, attachment destinations or search backends.

## Implementation

`CollectionView<TItem>` is a production component with typed Items, required accessible Label, optional ItemText formatter and ItemTemplate, Id and Disabled. The summary displays only Items.Count; its accessible name includes the collection label and count. Empty and disabled triggers are inert and marked aria-disabled. Ordinary values are encoded; templates compose the existing controls and host-supplied links.

The component uses native details/summary before enhancement and the existing attachDisclosureMenu controller afterward. It is always Weak and opens on pointer hover, click or keyboard, without moving focus during hover. Its nonmodal dialog contains a semantic, keyboard-readable list. Existing controller behavior owns positioning, trigger/panel transitions, top-layer scrolling, Escape, outside/focus dismissal, touch activation, theme copying and cleanup. There is no separate JavaScript popup implementation. Framework CSS adds only collection layout/native-fallback rules; existing standard Button and popup surfaces own appearance.

`TableColumn.SearchValue` already separates indexed text from rendered cells. The collection component does not index its displayed count. A host should provide the full underlying product/attachment values through SearchValue for local search, and apply equivalent authorized server predicates before paging/limiting for remote search. A regression uses the real TableData filter to find the owning row by a second product hidden behind the count. Backend indexing, scope and the relevance of attachment metadata remain host responsibilities.

Gallery now has an executable three-language guide/sample covering values, zero, disabled and attachment links. The source-owned usage catalog and parameter descriptions explicitly describe indexing ownership. Catalog inventory is 85 components, 798 parameter rows and 376 documented defaults.

## Verification

Evidence is in `artifacts/collection-view-20261030/`.

- Twelve-project Blazor Release build: zero warnings/errors.
- Framework executable: 507/507 checks, including six new collection/rendering/search cases.
- Gallery: 8863 checks, with an intentional inventory guard updated from 84 to 85.
- Full Node release selection: 290 top-level tests. The controls DOM module independently passed 88 subchecks, including six new collection hover, keyboard/link navigation, touch, weak dismissal, disabled and cleanup checks against the actual shared module.
- Three-language catalog integrity: 25413 checks; changelog: 194 checks.
- Six local package identities, internal dependency edges and required assets verified; four isolated fresh-cache consumer variants passed 147 checks.

The first Gallery run stopped at its previous exact 84-component inventory guard. This was corrected to the actual intentional 85-component inventory, with an explicit CollectionView guide check; the full Gallery selection then passed. All six pack invocations completed, but an invalid trailing aggregate PowerShell redirection token made that wrapper exit nonzero afterward. No archive was rebuilt or overwritten: subsequent archive verification and fresh consumers passed. `pack-note.log` records that distinction.

The candidate keeps all preceding ReferenceDropdown.MaximumResults changes. Source root props remain 1.4.4; explicit Version/VersionPrefix/VersionSuffix overrides produce the unique 1.4.5-preview.2 archives. The prior preview.1 feed remains unchanged. `provenance.json`, source-files snapshots, source.patch and package-sha256.json preserve the source baseline, seventeen changed source/test hashes, all six immutable archive hashes and unchanged twelve-project/33-external-pair closure. No dependency, SDK or service was added; no WPF/WinUI source, commit, push, tag or public publication was changed.

## Manual acceptance

1. In Gallery, verify a count of two opens its complete list on hover and pointer movement into the panel does not close it. Moving outside closes the weak popup.
2. Use Enter/Space or ArrowDown, navigate supplied links, and press Escape. Confirm focus returns appropriately. Touch activation must work without simulated hover.
3. Verify zero and disabled examples cannot open. Resize/scroll near viewport edges; the popup must retain standard placement, scrolling and colors.
4. In the host order table, verify opening a count does not also open the row, and remote searching by any supplied product value finds every authorized matching order before paging. This server-side acceptance belongs to the host integration.

Computer Use and physical-browser acceptance were not performed. Automated DOM fixtures do not assert a real browser's geometry or assistive-technology behavior.
