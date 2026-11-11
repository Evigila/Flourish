# Refine shell menu placement and navigation icon defaults

Recorded 2026-11-10 11:59:00 America/Sao_Paulo (UTC-03).

ShellHeader now places product menus beside the brand by default. FollowContentWidth is an opt-in for consumers that need the menu aligned with the page content; responsive stacking and existing identity/action slots remain supported. Direct EndActions controls receive standard spacing from the shell, without requiring a host InlineActions wrapper. Header menu and action text use regular typography while the title stays bold.

Secondary and tertiary navigation leaves omit icons unless explicitly supplied. NavigationItem.Icon defaults to null; ISubNavigationBuilder adds iconless string and TextReference overloads while preserving existing nullable-icon signatures. Explicit icons, hierarchy disclosure, primary icons, expanded primary labels and navigation targets remain supported. No maintained source/test path or external dependency was added.

## Verification and local package

The Release build passed with zero warnings and errors. Verify-PackageSet passed all six 1.4.8-preview.10 package identities, dependencies and required assets. The Blazor test harness passed 539 checks. The header, native select and tutorial JavaScript suite passed 45 checks, including the header font and spacing assertions. Colligere restored the local preview and passed 130 focused Web tests.

The local feed is ../Colligere/artifacts/flourish-preview/1.4.8-preview.10 and source provenance is ../Colligere/docs-ai/current/flourish-local-preview-1.4.8-preview.10.json. Source VersionPrefix remains 1.4.7 with explicit preview build overrides. No public release, commit, tag or push is part of this task. Documentation inventory and shared API descriptions were synchronized.

## Manual acceptance

- On wide and narrow headers, verify menus sit beside the brand by default and wrap when required. With FollowContentWidth enabled, verify page alignment remains available.
- Check title emphasis, ordinary action/menu text and spacing between direct right-side controls, including language and sign-out controls.
- Inspect secondary/tertiary navigation with absent and explicit icons; verify primary icons, expanded primary captions, keyboard focus and nested navigation still work.
