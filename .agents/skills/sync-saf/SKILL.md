---
name: sync-saf
description: Initialize, check, or update SAF in an existing Git repository when the user requests framework synchronization or a version check. Does not perform SAF structural audits.
---

# Synchronize SAF

Read the target's `AGENTS.md`, [configuration contract](../../../docs-ai/common/rules.md#project-configuration), [synchronization rules](../../../docs-ai/common/rules.md#framework-synchronization), and `SAF.json`/`SAF.lock.json` when present. Resolve `SAF.config.json` through `Resolve-SafConfig -Root <root>` in `scripts/saf-core.ps1`; preserve project-owned bytes and configured protections. Configuration narrows scope and never supplies authorization. Separate configuration, bootstrap provenance, payload installation, and audit evidence.

- For a version-only request, run `origin.bat -check`. Exit `0` means current, `2` update available, `3` local conflict, `4` bootstrap refresh required, and `1` error. Check may append `.agents/saf_log/` only; it does not update framework/config/manifest/lock files or migrate legacy runtime data. Report the result without applying an update.
- For an authorized update, run `origin.bat -sync` to check then update from the same resolved snapshot, or `origin.bat -update` for the requested selected release. Explicit actions do not wait for input. Reuse existing authorization.
- Apply a requested source/ref through supported `-Repository`/`-Ref` options. Explicit values override config defaults, then installation/bootstrap provenance supplies absent values; no repository is an error, while an absent ref falls back to `main`. Preserve selected source in installation state without changing the consumer Git remote or rewriting config.
- Inspect conflicts before proceeding. Use `origin.bat -update -force` only when replacement of managed customizations is authorized. Isolated Force is invalid; seeds and project-owned files remain protected.
- If config uses an unsupported schema or removed fields, have the user migrate it explicitly before running the new trusted `scripts/init.ps1`; do not drop fields or overwrite config automatically. For missing/incompatible handlers, use a trusted source declaring the supported config-support/handler contract. Do not silently use an older controller that ignores restrictions. Bootstrap refreshes handlers with backups and leaves payload sync separate. Do not delete consumer state/config, hand-edit baselines, or claim an unpublished remote URL works.

Verify actual differences, installation version/source commit/manifest digest/baselines, and backup references. Confirm project facts, human documentation, user-added skills, debug customization, Git index, and Git origin were preserved. Bootstrap/synchronization does not complete a SAF structural audit; a version change makes that separate framework/required-structure check due under the shared gate, without requiring a business-code or project-fact review.

Report changed paths, actual verification, conflicts/recovery, and remaining work. Run tests/checks appropriate to the affected framework operation unless an explicit user limit takes precedence; report unavailable checks without claiming success. Computer Use defaults to deny, follows its supported config preference, and permits applicable explicit user exceptions. Commit approval is fixed: ask unless already explicitly authorized, preserve existing commit history, and use configured commit styles only for authorized new messages. Do not install software or refresh SAF structural audit completion as part of synchronization.
