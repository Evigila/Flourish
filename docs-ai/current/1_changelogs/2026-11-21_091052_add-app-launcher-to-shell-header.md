# Add an app launcher to ShellHeader

Recorded 2026-11-21 in America/Sao_Paulo (UTC-03:00).

## Changes

- ShellHeader accepts a ServiceLauncher fragment before the brand. The independent Services slot still follows the brand. Empty launcher content does not take up header space.
- AppLauncher renders a nine-dot button and a compact panel of host-supplied ServiceLink destinations. The glyph remains visible when only Framework is loaded. The shared popup controller handles activation, horizontal anchoring, viewport clipping, arrow keys, outside dismissal and Escape focus return. Service links use full document navigation for cross-service handoffs.
- The Gallery shows the launcher on its own and beside the existing ShellHeader menu. Its guide, sample and accessible action label are available in all three Gallery languages.

## Verification and delivery

- The Release production graph built with zero warnings or errors. All six 1.4.9-preview.4 packages passed identity, dependency and required asset checks. The packaged framework and design styles contain the launcher rules.
- Blazor rendering and API checks passed 543/543. The menu DOM suite passed 46/46, including Framework-only glyph visibility, click-only activation, keyboard selection, Escape focus return, native link dispatch and outside dismissal. The header layout suite passed 9/9. The Gallery passed 9405 checks and the Culture catalog passed 26413 checks.
- The preview is a local package override of the 1.4.8 source baseline. Manual acceptance covers the nine-dot icon, narrow layouts, keyboard focus, light and dark chrome, and real service navigation.
