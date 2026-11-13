# Shared execution and documentation rules

## Task boundaries

Follow the effective instruction hierarchy and the user's authorized scope. Reuse existing session instructions and approvals; do not ask for the same authorization again. Read applicable rules and affected implementation before changing files. Keep unrelated refactoring and expansion out of the task.

Preserve access control, domain behavior, session behavior, and public contracts unless modification is authorized. Explain a discovered defect before expanding a narrower task, and continue independent work within scope.

Report conflicts between `common/`, project facts, and human-maintained requirements. An explicit, applicable, already-authorized project exception in `current/` may be followed within its scope. Clarify only unresolved disputed changes; do not rewrite standards to make implementation appear compliant.

Use bounded subagents for separable work when the runtime permits delegation. Keep concurrent write scopes disjoint. The coordinator must inspect, integrate, and verify results. Do not create another user-owned chat solely for internal delegation.

## Document ownership

`docs/` contains human-maintained documentation. AI may read, search, cite, and link it. Without explicit task-scoped authorization, AI must not create, edit, move, rename, delete, or replace its contents. Report stale material rather than silently repairing it.

`docs-ai/` contains AI-maintained standards, facts, decisions, and records. Reusable workflows belong in `.agents/skills/`; skill packaging does not bypass ownership or task authorization. Do not infer AI ownership from writing style.

Do not create documentation outside `docs-ai/` without explicit authorization. Root `AGENTS.md`, `SAF.json`, `SAF.config.json`, and `SAF.lock.json` are role-specific exceptions. SAF source maintenance may maintain its authorized README pair, bootstrap, scripts, release metadata, templates, tests, and CI; source-only material is not imported into consumers. `.agents/backup/` and `.agents/saf_log/` contain local operational data, not an alternative documentation area. Skill packages remain under `.agents/skills/`; do not treat the entire `.agents/` tree as disposable runtime data. An empty `.gitkeep` is a directory marker, not a requirement that the directory remain empty.

Write SAF instructions, AI-maintained project documents, templates, and records in English. This is a shared standard rather than a project-configurable language preference; preserve human-authored content and quoted identifiers as supplied. Store prose as `.md` and machine-readable documentation configuration as `.json`; operational logs use JSONL below. Keep toolchain-native formats. Active `AGENTS.md` and `common/` contain only effective policy. Keep dated decisions, approval history, uncertain project-specific guidance, and superseded material in `current/` with clear status.

## Project configuration

`SAF.config.json` is project-owned, schema-versioned data read through `AGENTS.md` and existing handlers/skills. It is not a second policy entry point, a release manifest, or observed installation/audit state. The neutral consumer seed contains only the supported `schemaVersion`; never copy source-project protected paths, verification/UI/commit preferences, or source defaults into another project.

Use `Resolve-SafConfig -Root <root>` from `scripts/saf-core.ps1` for shared defaults and strict validation. Unsupported schemas, removed/unknown fields, invalid types/enums, and unsafe relative paths are errors; do not silently drop fields, reset config, or ignore it. Read configuration before selecting work or changing protected paths. Existing explicit instructions, effective project constraints, and task authorization take precedence. Configuration supplies supported preferences and restrictions, never authorization to commit, change protected content, install dependencies, or bypass integrity checks.

| Optional field | Meaning and allowed values |
| --- | --- |
| `origin.repository`, `origin.ref` | Default source/ref for origin when explicit command values are absent; installation/bootstrap provenance remains observed state |
| `project.protectedPaths` | Additional exact repository-relative file paths or trailing-slash directory prefixes; no wildcards, absolute paths, or traversal. Existing directories require a trailing slash and existing files must not have one. Preserve them unless the current explicit task authorizes the affected change |
| `project.verification.computerUse` | `deny` or `inherit`, default `deny`; deny prohibits Computer Use testing unless an explicit user instruction grants an applicable exception. Inherit follows existing instructions and grants no tool permission |
| `project.ui.framework` | Nonempty design-guidance string, default `Material Design 3`; an Agent preference, not automatic adoption/installation of a component library |
| `project.commit.style` | Nonempty commit-style string, default `follow-history`; the Agent follows the repository's established commit pattern when this default is used |
| `project.commit.initialStyle` | Nonempty initial-style string, default `english-imperative`; use it when no established commit pattern applies, with English imperative subjects as the default |

Absent source/ref values and additional protected paths resolve through shared defaults. Explicit origin parameters override config defaults, then installation/bootstrap provenance supplies absent source values; a missing repository is an error and a still-missing ref defaults to `main`. UI and commit strings are Agent-consumed preferences, not shell commands. Do not store secrets, executable expressions, arbitrary module paths, or settings that disable required testing, commit approval, history preservation, English, the calendar-month SAF audit, root-script limits, ownership, hashing, or path/payload validation.

SAF write helpers used by bootstrap, origin, local maintenance, and state writers must check the target and every missing repository-relative ancestor that would be created against protected paths. Do not broaden an exact-file rule into a subtree rule for existing ancestors. Recheck the config snapshot before writes; Force does not bypass protection. Project-owned scripts launched by `debug.bat` are not filesystem-sandboxed by SAF; the Agent must assess their effects before invocation. Explicit authorization for a direct protected-file edit does not disable SAF helper checks or rewrite protection settings.

Read configuration bytes once to derive both the parsed policy and its raw-byte snapshot hash. SAF JSON readers require an object root before parser output can enumerate an array, then validate the parsed object/schema. Apply this to bootstrap metadata, config, state, release manifests, and neutral seeds; do not accept arrays, primitives, or null roots as objects. Keep the implementation compatible with Windows PowerShell 5.1.

Configuration is seed-only and never replaced by synchronization or Force. Preserve consumer bytes and include config in scoped recovery. A source must declare the supported handler/config-support contract in `SAF.json`; bootstrap rejects an incompatible source rather than load code that may ignore project restrictions. When a project has an unsupported config schema or removed fields, have the user migrate that config explicitly, then run the new trusted `scripts/init.ps1`. Do not use old controllers to reinterpret it, silently delete fields, or use Force as configuration-migration permission.

## Audit gate

Inspect `SAF.lock.json` and its `audit` section at task entry. The periodic audit is a full check of SAF framework integrity and required file/directory structure. It does not audit the host application's source, business behavior, architecture facts, package graph, or documentation accuracy. Do not scale this check with the application's file count. Perform this SAF structural audit when:

- The user requests a SAF audit or framework-structure initialization.
- The lock/audit state is missing, invalid, or uses an unsupported schema.
- `audit.auditedFrameworkVersion` is missing, null, or differs from `SAF.json.frameworkVersion`.
- `audit.lastCompletedAtUtc` is null, invalid, or later than current UTC time.
- Current UTC time is at least one calendar month after completion.
- Ordinary task work reveals an obviously missing, invalid, or empty required path/document.

Otherwise return `SKIP_AUDIT` and continue the reading routes. This is a workflow decision, not a script command or return from the task. It skips only the SAF structural audit. Do not recursively inspect every required path merely to decide whether a recent audit can be skipped. A separately requested project review follows its own task scope and does not become a monthly prerequisite.

Add one calendar month in UTC, preserving time of day and clamping the day to the target month's last valid day. Do not substitute 30 days. New policy takes effect immediately regardless of the timestamp. Handler bootstrap alone does not complete the SAF structural audit.

## Documentation contract

| Required path | Responsibility |
| --- | --- |
| `AGENTS.md` | Compact instruction router |
| `SAF.json` | Release version, handler compatibility, and payload manifest |
| `SAF.config.json` | Project-owned schema-versioned values and restrictions; seed-only |
| `SAF.lock.json` | Project-owned bootstrap/installation/audit state |
| `origin.bat`, `local.bat` | Remote management and local maintenance interfaces |
| `debug.bat` | Seeded project-owned operation dispatcher |
| `scripts/` | Handler implementations and project operations |
| `docs/`, `docs/.gitkeep` | Human documentation area and empty marker |
| `docs-ai/common/codedesign.md` | Shared code standards |
| `docs-ai/common/rules.md` | Shared workflow/documentation standards |
| `docs-ai/common/UIUX.md` | Shared UI integration standards |
| `.agents/skills/` | Project-owned skill packages |
| `docs-ai/current/1_architecture.md` | Key file/directory/project responsibilities and dependency relationships |
| `docs-ai/current/1_dependency.md` | Current dependency inventory |
| `docs-ai/current/1_changelogs/` | Append-only changes |
| `docs-ai/current/1_bugreports/` | Diagnosis and regression evidence |
| `docs-ai/current/1_archived/` | Superseded or abandoned material |

Keep the exact names/capitalization of SAF template artifacts in the table. Reserve `1_` for those framework-defined template files and directories. Ordinary project reports, proposals, and analyses belong directly under `current/` with descriptive names such as `configuration-review.md`, without a `1_` prefix. Required directories may use empty `.gitkeep` markers. Archive ignore does not require tracking archive content; retain/recreate the local directory.

### Architecture file

`1_architecture.md` describes significant files, directories, modules, and projects by their responsibilities and dependencies. Explain which entry points call which implementations, which projects reference which others, and how configuration, resources, and external boundaries relate. Use concise relationship tables or a small diagram when useful; a complete per-file directory tree is not required.

Base architectural claims on actual project evidence and distinguish unknowns from verified facts. Multiple projects do not establish microservices. Update the description when important responsibilities or dependency/runtime boundaries change; adding an incidental file does not require rebuilding an exhaustive inventory. Maintain project architecture during relevant project work or an explicitly requested architecture review. A SAF structural audit checks that this document exists in the prescribed location and has the expected format; it does not verify or populate its project-specific facts.

### Dependency file

`1_dependency.md` records every dependency actually used: direct and resolved transitive packages, SDKs/runtimes, local project references, services, assets, and build/test/development tools. Distinguish production, build, test, development, and optional use.

Record identity, kind, declared constraint versus resolved version, direct/transitive relationship, consumers, purpose, and declaration/resolution evidence. Record verified sources/licenses where applicable and mark unknowns. Describe service contracts rather than invent versions. Never store credentials or secret connection data.

Use manifests, central configuration, locks, references, and available resolution reports. Update actual dependencies when they change; proposals are not active dependencies and inventory entries do not authorize adoption.

Dependency content is maintained during relevant dependency work or an explicitly requested project review. Its presence and format belong to the SAF structural audit, but enumerating/resolving application dependencies or verifying this inventory is outside that audit. An initialized, explicitly unverified template does not prevent a structurally complete SAF audit.

### History and archives

Name change records `YYYY-MM-DD_HHmmss_<lowercase-slug>.md` using the current task user's time zone when specified, otherwise UTC. Record the zone and offset. This is not a project configuration setting; runtime logs/state remain UTC. Imported records with unknown time use `000000`, preserving original identifiers/provenance.

`1_changelogs/` is append-only: correct a record through a new linked entry. Bug reports distinguish symptoms, root cause, evidence, fix/mitigation, limitations, and regression verification; add dated findings or follow-ups rather than erase earlier diagnoses.

Archive superseded/abandoned designs in `1_archived/`, with status, reason, scope, and replacement when available. Archived material does not govern new work. Memory compression does not relocate changelogs or rewrite bug evidence. Explicit archive-export cleanup follows the separate local contract below.

### SAF state

`SAF.lock.json` uses its supported state schema and separate `bootstrap`, `installation`, and `audit` sections. Bootstrap describes deployed handlers; installation describes synchronized payload, exact source repository/ref/commit, manifest digest, normalized managed-file baselines, and synchronization time. They may differ after handler recovery. Either may be null before its operation succeeds; the source repository may have both null.

Audit contains `auditedFrameworkVersion`, `lastMissingMaterialAskedAtUtc`, and `lastCompletedAtUtc`. UTC timestamps use ISO 8601 ending in `Z`. Initialize audit values to null for a new consumer; preserve genuine prior evidence during migration. State is neither instructions nor authorization.

All state writers use `scripts/saf-core.ps1`'s common read/merge/atomic-write implementation and operation lock. Audit writers dot-source that helper and call `Set-SafState -Root <root> -Section audit -Value <audit-object>`; do not rewrite the entire JSON or discard bootstrap/installation. Preserve the latest applicable question time and write actual completion only after a successful full SAF structural audit. If the helper is missing/incompatible, report the issue rather than bypass coordination.

Bootstrap/synchronization never mark audit complete. Version changes make audit due by comparison without fabricating completion. Track the lock with installed framework files; logs/backups remain outside it and ignored.

## SAF structural audit

Use [audit-project-docs](../../.agents/skills/audit-project-docs/SKILL.md) to audit SAF only. Check the required files/directories, expected file kinds and basic formats, release/config/lock schema and version relationships, managed framework integrity, instruction/skill references, and required skill metadata. Validate project configuration through the shared resolver without reviewing project facts or treating consumer config as managed release content. Use the SAF manifest and required-path contract to bound inspection; do not enumerate the application's maintained files, read business source, resolve packages, or verify architecture/dependency documentation. The source repository may have null bootstrap/installation; do not invent a consumer installation.

Create authorized missing SAF structural directories, empty markers, and project-document seeds while preserving existing project facts and human content. An unverified architecture/dependency template can satisfy the structural contract; checking its existence/format does not establish its project facts. Do not populate those facts merely to pass the monthly check. Leave completion unchanged while required SAF structural checks or repairs remain unresolved. Write completion only for a successful SAF structural audit, not a project review or functional test. Report discovered framework customizations/conflicts without silently replacing them or using Force.

## Framework synchronization

Use existing Git, Windows CMD, Windows PowerShell 5.1, or already available PowerShell 7. Do not install an application, global CLI, package, service, or runtime, modify PATH/global Git configuration, or permanently change execution policy. Windows BAT interfaces do not imply cross-platform entry points.

`SAF.json` defines a stable `major.minor.patch` release, handler contract, and exact source/target/mode/normalized SHA-256 inventory. SAF source maintainers bump the release when the distributed payload/manifest changes. Consumer-local customization preserves the installed release identity and conflict baselines; it does not authorize preparing a framework release or rewriting installation evidence. The manifest does not hash itself; installation state records its digest and source commit. Source-only documentation commits are not automatically framework payload updates.

Source-only `scripts/init.ps1` provides remote bootstrap/recovery from a trusted configuration-aware source. Capture the caller's filesystem directory and require the target Git root; do not derive an `irm | iex` target from script-file variables. Resolve one commit, verify configuration support and validate all handlers before writes, back up replacements, preserve project config/facts and existing audit/installation sections, and deploy both BAT handlers and required PowerShell helpers. Initialize missing config only from the supported neutral seed; do not replace existing config. Bootstrap does not silently synchronize full payload or exit the caller's shell. Payload hashes establish consistency, not independent authentication of bootstrap code already executed.

Daily `origin.bat` uses its deployed local implementation to read one validated remote snapshot. Do not execute a second independently moving remote controller or fall back to unverified code. Incompatible handler/schema results require bootstrap refresh before payload writes. Recover through trusted init rather than delete state. SAF does not promise historical pre-SAF installer handoff/rollback support.

| Command | Contract |
| --- | --- |
| `origin.bat -check` | Report validated availability, compatibility, and conflicts. No framework/config/manifest/lock changes or runtime migration; only `.agents/saf_log/` command logging may write locally. |
| `origin.bat -update` | Apply the selected release or report current/no-op. Stop on managed conflicts. |
| `origin.bat -sync` | Check, then update when available, using one resolved commit/change plan. |
| `origin.bat -update -force` | Back up/replace conflicting managed files; may allow requested downgrade. |
| `origin.bat` | Select sync automatically, then wait after completion to show results. |

Explicit action parameters have no initial confirmation or final wait. Reject conflicting actions and isolated `-force`; Force applies only to Update. Exit codes: `0` successful/current, `1` error, `2` update available from Check, `3` conflict, `4` bootstrap refresh required. Version-only requests do not authorize update/Force. Do not check the network during unrelated tasks.

Read regular Git blobs from one resolved commit and validate the entire payload before changes. Reject traversal, unsafe linked paths, duplicates/reserved targets, invalid modes, and hash errors. Normalize CRLF to LF and remove an initial UTF-8 BOM for text hashes. Manage only permitted manifest paths and prior owned paths. Never alter the target Git origin, index, branch, or commits.

Detect committed/uncommitted customization against imported baselines. Force never bypasses path/payload checks or project ownership. Seed missing project facts/directories/debug entry points without overwriting current documents or nonempty project customizations. Preserve user-created skills and unrelated files. Update state after successful synchronization; retain recovery evidence and report partial failures accurately.

Origin recovery snapshots must include an existing `SAF.config.json` even when the seed is not a planned write. Include its exact bytes and hash rather than infer recovery completeness from preservation of the live file.

### Transaction recovery

Capture each before-image and raw-byte hash from one read. Before each mutation, recheck the expected snapshot and protections, including after any parent-directory creation. Use fixed create/replace behavior from the expected existence state: creation must not replace a newly appeared file, and replacement must not recreate an externally removed target. Deletion does not create parent directories.

Record the atomic mutation's commit receipt, including a commit followed by an exception. Recover in reverse order only committed paths whose current raw-byte state still matches this operation's expected result and differs from its before-image. Preserve other content and report manual recovery. Recovery must respect both the original and current protection sets, revalidate paths, and use non-overwriting creation when restoring a deleted file. Only remove recorded created parents if still empty and permitted. Hash checks and SAF leases do not guarantee isolation from arbitrary external edits or power loss; report incomplete recovery rather than claim a full rollback.

For legacy migration, preserve old baselines and real audit evidence before conversion. Move nonempty `debug-handler.bat` to `debug.bat` only when the target is absent; differing existing files conflict. Retire only verified old owned artifacts after new state/layout succeeds, retaining recovery copies. Historical records keep original terms.

## Local maintenance

`local.bat` performs offline mechanical operations. Audit/compression are Agent skills, not local commands; do not add model dispatchers, engine adapters, or task-request files. Skill discovery remains `.agents/skills/`; runtime data uses only `.agents/backup/`, `.agents/saf_log/`, and `.agents/operation.lock`.

Bootstrap and non-Check origin/local writes preserve and migrate legacy `.agent/` runtime data under the shared migration contract. Verify copied bytes before deleting old sources; identical destination bytes may be reused. A differing backup collision stops with both copies retained, while differing old logs receive a content-hash-suffixed path under `.agents/saf_log/legacy/`. Coordinate against old/new operation locks rather than discard an active legacy lease. Report incomplete/conflicting migration and preserve evidence. Check never migrates. Exclude old/new runtime from recursive backups while retaining skill packages.

No-argument local invocation shows a menu: Backup first/default, archive export/cleanup, archive ignore, Exit. Empty Enter chooses Backup, invalid input repeats, EOF cancels, and a final wait shows results. Explicit actions run without waits and reject incompatible flags.

### Backup

`local.bat -backup` creates `.agents/backup/<UTC-time>-<unique-id>/` with exact scoped bytes, inventory, and hashes of SAF handlers/state/config, AI project documents, and skills. State its SAF recovery scope; it is not a full source backup. Exclude previous backups, logs, operation locks, legacy runtime, Git internals, temporary/build caches, and unrelated source. Automatic backups use the same conventions and record the triggering action. An incomplete snapshot is not a successful recovery point.

### Archive export and cleanup

`local.bat -clean-arquived` exports only `docs-ai/current/1_archived/` to a unique ZIP in the user's real Windows Downloads folder obtained from the Windows User Shell Folders Downloads GUID. Do not assume a literal profile directory. Reject linked/traversing paths or output inside source.

Include hidden scoped files. Reopen the ZIP and verify relative paths, entry counts, and content hashes before deleting anything. Recheck source files and delete only unchanged exported content; preserve/report new or modified files. Retain/recreate the archive directory and empty marker. Export/verification failure preserves source; partial cleanup reports remaining content and recovery ZIP. A local ZIP is not a permanent external-retention guarantee.

Cleaning tracked archives creates local Git deletions; later user commits may remove them from a remote branch. SAF does not stage, commit, or push. This differs from ignore-only behavior.

### Archive ignore

`local.bat -no-arquived` idempotently adds root-scoped archive-child ignore rules while preserving `1_archived/.gitkeep` and existing `.gitignore` content. It does not modify index, commit, push, or remove remote files. Already tracked archives remain tracked; report that limitation. Never automatically run `git rm --cached`.

### Command logs

Append logs under `.agents/saf_log/<UTC-date>.jsonl`, excluded from synchronization and version control. Record operation ID, UTC start/completion times, action, result/exit code, affected relative paths, relevant versions/commits, and backup/export references. Exclude secret arguments, credential-bearing URLs, tokens, and complete file contents. Logs are operational evidence, not project change history. Show log failures without obscuring the real operation result. Task-specific record naming does not change operational UTC timestamps.

## Memory compression

Use [compress-saf](../../.agents/skills/compress-saf/SKILL.md) for authorized semantic consolidation of `docs-ai/current/`. Make a recovery snapshot first. Remove repetition and archive superseded active reports only after retaining effective facts, constraints, decisions, unresolved issues, compatibility, provenance, verification status, and unfinished work in accessible current material.

Preserve required architecture relationships and dependency evidence. Do not rewrite/delete/relocate append-only changelogs or bug evidence. Do not modify human `docs/`, shared policies, source, dependency declarations, installation state, or audit completion as a side effect. Archives identify status/reason/replacement. Update active links and report consolidation/archiving. Semantic compression is not archive ZIP cleanup and does not complete a SAF structural audit.

## External dependencies

Do not add, replace, or upgrade external dependencies without explicit task authorization, including packages, SDKs, services, fonts/icons, and build/runtime/deployment components. An unrelated feature is not blanket approval. Before seeking missing approval, prepare the need, alternatives, compatibility, impact, and affected declarations/inventory; do not install dependencies to prepare that explanation.

Apply approved changes through existing dependency management, verify affected behavior/builds as authorized, and update `1_dependency.md`. Inventory entries do not authorize adoption.

## Project scripts and entry points

Implement operations under root `scripts/`. AI-created root scripts are limited to exactly `origin.bat`, `local.bat`, and `debug.bat`, including in the SAF source repository. The source-only bootstrap is `scripts/init.ps1`; there is no root PowerShell-script exception.

Project debugging, data resets, and publication use project-owned `debug.bat`. The seed offers Debug, Clear database, Publish tag, Publish package, and Exit; projects may adapt authorized operations without adopting a mandatory ecosystem. Dispatch under `scripts/` and report missing implementations. Preserve nonempty project customizations, including under Force.

The debug seed shows a menu, defaults to Debug on empty Enter, repeats invalid input, cancels EOF, and waits before exit while retaining the operation code. Explicit noninteractive selection may skip waits. Isolate child BAT execution so its `exit` cannot close the parent; restore working directory and use `exit /b`. Origin/local retain their own contracts above.

## Implementation and verification

Use [codedesign.md](codedesign.md) and [UIUX.md](UIUX.md) as applicable. Running tests appropriate to implementation changes is required; it is not a configurable opt-out or a reason to ask for test authorization again. Reuse the task's authorization and run relevant automated tests and focused regressions before delivery. Read-only/documentation work uses appropriate scoped checks. An explicit user limit such as static-only verification takes precedence; unavailable tests, dependencies, or environments must be reported with their practical limits, never presented as a pass.

Computer Use project testing defaults to `deny`. Read effective `project.verification.computerUse`: `inherit` follows applicable instructions/tool permissions, and a user's explicit applicable exception takes precedence over the configured default. Neither value grants tool permission or allows skipped required tests. Keep verification proportional to risk and provide recommended manual steps for behavior needing human acceptance.

Distinguish passed, failed, and unperformed checks with reasons. Structural inspection does not prove runtime behavior. Follow task-specific testing limits, including static-only verification when requested.

## Delivery and records

Synchronize durable facts with actual changes. Record material implementation/decisions in `1_changelogs/`, investigations in `1_bugreports/`, and superseded designs in `1_archived/`. Do not create changelogs for read-only analysis with no lasting change unless requested or needed to preserve an important finding.

Distinguish implemented facts, verified results, assumptions, proposals, and outstanding work. Keep records concise/linked. At completion report affected modules/files, actual verification, unresolved work/reasons, limitations, and recommended manual checks.

Before creating a Git commit, ask for approval unless the user has already explicitly authorized the applicable commit; do not repeat an answered request or treat task completion as authorization. After implementation is complete, ask whether the user wants a commit unless already authorized or declined. This approval requirement cannot be disabled through configuration.

Do not delete or rewrite existing commits. Make authorized changes through new commits, preserving prior history. For a new commit, apply `project.commit.style`; the default `follow-history` uses the repository's established recent commit pattern. When no usable pattern exists, apply `project.commit.initialStyle`, defaulting to English imperative subjects. Configured style strings change presentation only; they never authorize a commit or history rewrite.
