# Project instruction and inventory audit

Date: 2026-10-10, America/Sao_Paulo (UTC-03:00).

## Scope and symptoms

The user replaced the instruction router and requested a renewed project review. The installed framework is 1.0.0, with lock provenance commit `03ec2f6ebacb443585056b83c581f89a99c8ee36`. The working tree includes the previously verified Essential.Culture 1.4.0 changes and user-installed instruction files; the review preserves those changes.

The prior audit state used schema 3 and omitted auditedFrameworkVersion. The installed rules accept schema 2 and require the installed framework version, so the full documentation audit was mandatory. New architecture/dependency files were seed templates. Older active guides still pointed to the previous inventory and presented obsolete bridge APIs/versions and the former archive restriction.

## Causes and repairs

- Framework installation preserved consumer facts/audit state by design. Installing templates does not populate a verified project inventory or complete an audit. The coordinator reconstructed the actual tree and dependency inventory before updating state.
- New 1_architecture.md covers 678 maintained files and 121 non-root directories, 22 projects, five solutions and 33 local project references. Explicit temporary/generated exclusions are documented, including eleven physical folders with no maintained descendants. Files and directory nodes were independently parsed and compared against physical enumeration; no missing/extra nodes or duplicates remain. Reference paths resolve on Windows and the local project graph is acyclic.
- New 1_dependency.md records actual declarations/resolution and external resources/tooling. It distinguishes declared constraints from resolved observations and treats unverified licenses/versions as unknown. This review does not change dependencies or certify external publication.
- The current index and architecture/dependency reading routes now use the installed contract. Old architecture, dependency-policy, bridge and solution guides are retained under 1_archived with explicit historical status, reason, scope, replacement and original-byte digests. Older link entry points remain usable. Legacy append-only change and bug records remain at their original paths; new records use the named 1_ directories.

## Copyable Culture example defect

Localization.razor displayed a registration snippet without the Framework/Abstract namespaces or a TextKey alias. The generated type is Key; Gallery's own _Imports supplies its alias, but an independent consumer copying the snippet does not inherit it. The Razor scenario also relied on Gallery's imports for the bridge and key alias.

Only the displayed Registration and Scenario strings were corrected. The actual page component behavior and bridge runtime remain unchanged. An independent package-only Web fixture extracted ModuleConfiguration, Registration and Scenario directly from the page, without Gallery imports or extra global using declarations. It compiled with zero warnings/errors and deployed two generated modules matching their source. Existing Flourish candidate 1.1.3 and Essential Core/Blazor/Generator 1.4.0 were reused; no dependency was added or upgraded.

Evidence: `artifacts/localization-example-review-41241f20ed6941489d71393b0c37906e/verification.json`. The first sandbox restore could not obtain NuGet vulnerability data and reported NU1900; rerunning the same restore with approved network access succeeded. Vulnerability checking was not disabled.

## Verification

- All 20 required structural paths exist; the human docs marker is empty and both project-owned skills have valid manifests.
- All ten managed framework payload hashes and the installed manifest digest match the lock after text normalization.
- Independent tree comparison and active local-document link verification pass.
- A before/after SHA-256 snapshot verifies that 317 human-document and legacy history files are unchanged.
- Eighteen maintained PowerShell scripts parse without errors; this is syntax verification, not script execution.
- The actual copied Culture snippets compile independently with zero warnings/errors; `git diff --check` passes.

Artifacts under `artifacts/agents-audit/` retain verification reports. Previous solution/runtime tests in the [Essential module report](../bugfix-reports/2026-10-10_essential-modules-bridge.md) remain earlier-task evidence; they were not all rerun for this audit. Static code review found no substantiated new Culture module bridge regression.

## Existing limitations and outstanding ownership

- Human-maintained `docs/docfx.en-us.json` and `docs/docfx.zh-cn.json` still reference removed `src/Flourish.WPF/Flourish.WPF.csproj` and `src/Flourish.WPF/Assets/favicon.ico`. `docs/roadmap.md` also cites former WPF source paths. These are reported rather than edited under the human-document ownership boundary; this audit does not claim the DocFX workflow succeeds.
- `LICENSE.txt:3` retains `[year]` and `[fullname]` placeholders. The MIT body exists; the review does not invent the missing attribution.
- WinUI3 is a placeholder. Physical `Gallery.Flourish.WINUI3` casing differs from solution/launcher `Gallery.Flourish.WinUI3`; Windows resolves it, while case-sensitive checkouts may fail.
- Blazor theme/color selections still live in scoped memory and do not survive refresh. This is existing preference scope, not a new regression. UI/format culture choices use the browser Cookie.
- The earlier ApplicationLayout head-ownership/flicker investigation remains analysis only. Browser/native appearance and interaction require manual acceptance; no Computer Use was performed.

## Manual regression checklist

1. Open Localization in Gallery and copy its module project configuration, registration and Razor scenario into an independent application using the current candidate packages. Supply Nav_Home and Formatted_Sample translations, then build without Gallery imports.
2. Publish/run that application, switch UI and format cultures and refresh; verify translated text, formatting and browser preference retention.
3. Follow the new index/router links to architecture, dependencies, histories and archives. Confirm archived guides are explicitly historical.

No Git commit, tag, package publication or network framework synchronization is performed by this review.
