# Verify language changes after real Gallery sample events

## Final result

The prose-localization repair now includes 124 additional actual post-event checks for SplitButton exports, SearchBox input/query/matches, EditingGrid errors and undo/redo/rejected-save history, and MultiSelectBox selection/new user labels. Total real Gallery render checks: 7,234, extending the earlier 7,110 result without changing product state contracts. All pass in en-US/zh-CN/pt-BR/en-US with retained component and data identity.

The full Blazor solution Release build passed with warnings as errors: zero warnings/errors. All six original resource manifest/bundle SHA-256 values remain unchanged after the final isolated build. The test-only Gallery server is stopped. ReleaseSettings now registers the real Gallery console checks and Culture access-key/catalog checks in existing quality gates; no launcher, resource or dependency configuration changed.

The earlier implementation record remains accurate for its recorded verification point and is supplemented by this final result. See [the updated regression report](../bugfix-reports/2026-10-06_gallery-prose-localization.md). No Git commit or publication was performed; manual visual acceptance remains user-run.
