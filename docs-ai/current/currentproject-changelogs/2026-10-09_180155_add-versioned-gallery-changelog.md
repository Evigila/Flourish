# Versioned Gallery ChangeLog and bounded action examples

## Request and implementation

- Added `/changelog` as a primary navigation entry without secondary navigation. The labeled SelectBox follows PageHeading and precedes the first Section heading; the next preview opens by default.
- Added a ChangeLog section after Home's Start here grid and before setup, using the existing UniformGridButton link.
- Embedded Models/ChangeLog.json as the single ordered release inventory. ChangeLogCatalog loads it without runtime Git/network access. Stable versions 1.1.1, 1.1.2 and 1.1.3 describe the preceding tag ranges; 1.1.4-preview describes v1.1.3..280018d plus this task. The initial v1.1.0 tag is omitted.
- Added concise English, Chinese and Portuguese release notes to the existing Gallery Culture catalog. Sources include the CSS fixture, identity/access/editor, table/gutter/Dialog, WPF reconstruction, Field.Actions, Boolean select, PageBody action-width and TutorialBoard records. Preview content is unreleased; package VersionPrefix stays 1.1.3.
- Constrained both Button Example account instances to 24rem with max-width:100%. The SplitButton Complete login page example uses the same bounded container while retaining its FullWidth switch, primary link, menu and availability controls.
- Added build/Test-ChangeLog.ps1 to release checks and Publish-Helper, including SkipBuild. It rejects missing stable/tag notes, invalid/duplicate/unordered versions, missing translations and an absent/incorrect next preview. It only reads reachable local tags. Future releases promote reviewed preview content and add the next patch preview; an empty preview displays the no-changes caption. Editorial accuracy remains a review responsibility.
- Updated the architecture directory guide and existing release-maintenance guide. No external dependency, library contract, business rule, package version or human-maintained docs content changed.

## Verification

- Release build of Tests.Gallery.Flourish.Blazor and its project graph with warnings as errors: zero warnings/errors.
- 7,633 real Gallery rendering/language/lifecycle checks passed, including 179 ChangeLog selection/content-replacement/retained-language checks and 124 existing post-event sample checks.
- 240 local HTTP navigation checks and 568 three-language HTTP/static-asset checks passed on an independent Development-mode Gallery instance at localhost:5279; the original occupied localhost:5188 instance was untouched. The temporary verification host was stopped.
- 22,135 Culture catalog integrity checks and 55 real ChangeLog release checks passed. The bounded agent also verified ten release-guard success/failure fixtures in Windows PowerShell 5.1.
- git diff --check passed. No Computer Use, Git commit, tag creation or publication was performed.
- Initial sandbox builds could not reach NuGet audit data; the authorized build outside the sandbox succeeded without changing dependencies. Running an un-published build as Production did not expose its source static assets; Development mode correctly loaded the generated manifest, and all asset checks then passed.

## Manual checks

1. Open ChangeLog from primary navigation and from the new Home section. Confirm no secondary rail and the selector between H1 and the first H2.
2. Select 1.1.1, 1.1.2, 1.1.3 and 1.1.4-preview; confirm each replaces the prior list and 1.1.0 is absent. Change language while viewing a stable version; retain the selection and translate its notes.
3. At desktop and narrow viewport widths, check both Button Example account examples and SplitButton Complete login page. They should stay within the 24rem maximum and available viewport width; inspect Chinese, English and Portuguese labels.
4. Exercise Button save/busy/count/reset and SplitButton width toggle, primary login navigation, menu options, full disable and primary-only disable. Preserve their existing behavior.

## Remaining action

Visual viewport, keyboard interaction and button/menu interaction checks are supplied for the user; no browser UI testing was performed. Await user authorization before committing, following recent imperative English subjects such as `Add versioned Gallery changelog and bound action examples`.
