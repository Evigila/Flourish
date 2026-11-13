---
name: compress-saf
description: Consolidate AI project memory under docs-ai/current when the user requests semantic compression, removing repetition and archiving superseded active reports while retaining effective facts and unresolved work.
---

# Consolidate SAF project memory

Read `AGENTS.md` and the [configuration](../../../docs-ai/common/rules.md#project-configuration), [ownership](../../../docs-ai/common/rules.md#document-ownership), [history](../../../docs-ai/common/rules.md#history-and-archives), and [memory compression](../../../docs-ai/common/rules.md#memory-compression) rules. Resolve `SAF.config.json` with `Resolve-SafConfig -Root <root>` from `scripts/saf-core.ps1` and honor protected paths and the supported Computer Use preference. Config grants no authorization and cannot disable required testing, commit approval/history preservation, or the English/monthly-audit/root-script standards. Unsupported config requires explicit migration rather than silently dropping fields. This is an Agent workflow, not `local.bat` functionality or ZIP cleanup.

Identify active reports under `docs-ai/current/` and source evidence needed to distinguish current facts from superseded proposals. Read history only when necessary. Do not infer ownership from prose style. Preserve user constraints, decisions, unresolved issues, compatibility, provenance, verification status, and unfinished work. Maintain AI documents and records in English under the shared standard, preserving human content and quoted identifiers as supplied.

Before edits, create a recovery snapshot with `local.bat -backup` under `.agents/backup/` and inspect completion, including preservation of project config and skill files without recursively backing up runtime data. Respect static-only/no-side-effect limits: inspect and propose changes without executing backup/compression, and report it was not performed. Stop document writes if the required recovery snapshot fails.

Consolidate duplicate active prose without weakening the required architecture relationship description and dependency evidence. Preserve key file/directory/project responsibilities and dependencies; do not require a complete per-file tree. Archive an obsolete active report only after its replacement retains useful facts/provenance. Mark archived status, reason, scope, and replacement; repair active references. Keep append-only changelogs and bug evidence intact and in place. Apart from the backup command's documented local recovery/log/ignore maintenance, do not modify human `docs/`, `common/`, source/configuration, dependencies, or lock state as a side effect.

Record a concise dated change entry using the current task user's specified time zone, otherwise UTC, and link recovery/replacement/archive paths. Runtime logs stay UTC. Perform appropriate scoped checks of references, architecture relationships, dependency evidence, unresolved work, and the diff; explicit user verification limits still take precedence. Distinguish simplification from facts verified against implementation. Do not mark a SAF structural audit complete, rewrite config, or claim unperformed functional tests passed.

Report consolidation, archiving, unresolved matters, actual checks, recovery references, and skipped work. Do not delete archive contents or rewrite existing commits. Ask before a new commit unless already explicitly authorized, and apply the configured commit style only to its new message; pushing still requires applicable authorization.
