# Complete reference catalogs were truncated after fifty options

Date: 2026-10-30, America/Sao_Paulo (UTC−03:00). Status: implemented and locally verified; not publicly released.

## Symptoms and cause

Colligere's timezone picker supplied a large catalog to `ReferenceDropdown<TValue>`. The actual component filtered names and then used an unconditional `.Take(50)`. With no search text, later candidates were absent from the selectable list and could not be reached by scrolling. Hidden width-measurement labels did contain the full catalog, but those labels are not options. Timezone naming/catalog problems in the host are separate from this library limit.

## Fix

The user explicitly authorized a configurable result bound and the consuming package update, then requested a local preview rather than publication. `MaximumResults` is nullable, defaults to 50, and accepts positive limits or null for all matches. Parameter validation rejects zero/negative limits. The existing filtering still happens before limiting. No styles, JavaScript, default bound, selection/creation semantics, focus behavior or disabled guards were changed. Gallery parameter descriptions cover all three languages and its 1.4.5-preview aggregate records the change.

The clean sibling checkout was safely fast-forwarded from `37a42ae` to `origin/master` `7c6f4e2` before edits. Candidate **1.4.5-preview.1** was built with explicit version overrides into an independent artifact directory; stable source props and older feeds remain intact. The documented explicit local-preview build route was used because the stable `publish-helper` preparation/publication path rejects prereleases. No commit, push, tag or public upload was performed.

## Verification

Evidence: `artifacts/reference-dropdown-20261030/`.

- Twelve-project Blazor Release solution build: zero warnings/errors (`build.log`).
- Framework: **501/501**, including five new actual-rendered-handler checks for default 50, filtering beyond the first 50, explicit positive limits, full-list selection/filtering/disabled behavior and invalid parameter rejection (`framework-standard-output-tests.log`).
- Gallery: **8779** checks, including localized API metadata and changelog (`gallery-tests.log`).
- Six-package archive/internal-edge/static-asset verification passed (`package-verification.log`).
- Four independent fresh-cache package consumer variants: **147** checks (`package-consumers.log`).
- Existing changelog validation **191** and localization integrity **25271** checks passed (`changelog-tests.log`, `culture-tests.log`).
- `git diff --check` passed; no Computer Use or manual visual claim.

An additional Framework run from the deeper isolated `--artifacts-path` output failed thirteen pre-existing CSS-file assertions because their fixed ancestor count resolves to `artifacts/src`. All new dropdown checks passed there. The same candidate was then rebuilt/run through the existing conventional test output path, where all 501 passed. Production code and the unrelated test path assumptions were not changed to mask that limitation. The isolated solution build, Gallery and packaged consumers passed independently.

`provenance.json`, `source.patch` and `package-sha256.json` preserve baseline, source and archive evidence. The feed contains only the unique candidate and is frozen after hashing. No cloud deployment or consumer runtime restart is asserted.

## Manual checks

1. In a host supplying more than fifty references, open a selector with `MaximumResults=null`; scroll to and select the final item without searching.
2. Search for a late item and confirm filtering still finds it; clear the query and confirm the complete list returns.
3. Confirm selection closes the popup, emits the original typed identifier and restores focus to the trigger.
4. Leave another selector at its default and confirm it retains the fifty-result bound.
5. Confirm a disabled selector cannot open or change its controlled value.

