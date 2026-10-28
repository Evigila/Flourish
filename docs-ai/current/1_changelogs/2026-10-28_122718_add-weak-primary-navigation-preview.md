# Add weak same-tree primary navigation previews

Recorded 2026-10-28 12:27:18, America/Sao_Paulo (UTC-03:00).

Local **1.4.4-preview.shell.20261027.10** adds default-enabled PrimaryNavigationPreview to NavigationSurface and ApplicationShell. The same real rail temporarily enters a bounded manual native popover while a narrow slot preserves content/secondary layout. Mouse hover and keyboard focus-visible show Icon + Label; leave/Escape close, mouse-click focus cannot pin it, and touch retains ordinary icons. Existing GET links, commands, current/disabled state and scroll owners remain. DisabledMessage stays readable; normal duplicate tooltips are suppressed and replaced anchor/span triggers rebind safely.

Four maintained controller/CSS/regression files were added, for 714 files/121 directories. No component, project or external dependency was added. Gallery/API/localization explain the default/opt-out and provide a disabled-reason example. The .9 compact header, .8 Information24px/fitted menus and .7 complete/common fonts are preserved.

Final Release12 passed with zero warnings/errors; Framework **487/487**, Gallery **8,653/8,653**, focused Node **76/76** and CSS bundle **21/21** passed. Six packages and eleven delivered source/bundle/assembly resources passed identity/byte checks; header-layout.js and both fonts equal .9. The independent frozen feed is `artifacts/primary-preview-20261028/nuget`, without replacing earlier archives. No Computer Use, browser measurement, publication, deployment or commit occurred.

[Design, development-boundary corrections and manual acceptance](../1_bugreports/2026-10-28_weak-primary-navigation-preview.md) and [archive identities](../1_dependency.md#current-candidate-archive-and-asset-evidence) retain evidence. Consumer acceptance is coordinated separately.
