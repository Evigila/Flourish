# Unify Notice status colors and elevation

Recorded: 2026-10-25 22:40:53 America/Sao_Paulo (UTC-03:00).

Blazor Notice now uses one status palette for all callers. The former Subtle gray skin and unused subtle paint tokens were removed; the parameter/class retains its existing announcement-priority behavior. Title and body use `--f-body-size`, title weight 700 and body weight 400; title owns a full grid row. The whole Notice uses existing `--f-shadow-control`.

| Type | Background | Title | Body |
| --- | --- | --- | --- |
| Information | `--f-surface-light` / #FFFFFF | `--f-info-text-light` / #1565C0 | `--f-text-light` / #112924 |
| Warning | `--f-warning-strong` / #B45309 | `--f-surface-light` / #FFFFFF | `--f-surface-light` / #FFFFFF |
| Success | `--f-success-background` / #16745F | `--f-surface-light` / #FFFFFF | `--f-surface-light` / #FFFFFF |
| Error | `--f-danger-light` / #9D322D | `--f-surface-light` / #FFFFFF | `--f-surface-light` / #FFFFFF |

White, blue, reading ink and red reuse existing palette values through stable light tokens. Other components retain their light/dark values. Success reuses the existing default green as an independent semantic role so blue host branding cannot recolor success. Existing pale warning orange did not provide readable white text; the new strong orange role meets 4.5:1 contrast. All Notice palettes remain stable under explicit/system dark modes and host brand overrides.

Root is `1.4.3-preview.portals.17`. Six local packages were verified; source, packed CSS and five consumer cache archives match. No external package identity, SDK, runtime, font or service was added. No public publication or app restart was performed.

Verification: Framework 474/474; Node palette suite 33/33, including 64 theme/brand/severity/Subtle combinations, typography, shadow and title/body contrast >=4.5; consuming Colligere Release all 17 projects 0 warnings/errors; consumer color tables 27/27; full Web 2601 passed, 187 environment-dependent skips, 0 failures. An initial consumer run exposed five old literal-only color-parser failures; the parser now resolves aliases and the tests verify actual packed Notice paints. Both repositories pass `git diff --check`.

Evidence: `../Colligere/artifacts/notice-colors-20261025/` contains build/Node/TRX reports and frozen dependency/package/cache/resource hashes. No Computer Use was performed. Manual acceptance: restart the consumer, compare registration/onboarding information Notices, inspect all four statuses under light/dark and blue branding, verify shared body size, title row and external shadow.
