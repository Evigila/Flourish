# Effective Flourish project constraints

These project-specific requirements complement SAF's shared standards. Read them for affected code, UI, package, script or delivery work. They preserve existing project constraints and confer no authority to add dependencies, publish packages, rewrite history or change protected content.

## Naming and public identities

Name types, members and files by responsibility rather than adding a product prefix solely for branding. Ordinary solution/project names use Product.Purpose; test projects use Tests.ProjectName, galleries use Gallery.ProjectName, and extensions use ProjectName.Extensions.Integration with corresponding test/gallery role prefixes. Project-owned namespaces begin with ArkheideSystem. or Arkheide., preferably ArkheideSystem.

Keep project/solution display and file names, namespaces/RootNamespace, AssemblyName, NuGet PackageId and public API identities distinct. Existing Arkheide.* package identities remain unchanged without a separately authorized migration. New or explicitly renamed projects follow these conventions; existing deviations require deliberate compatibility assessment. Third-party source retains its own naming and ownership.

## Visual and interaction ownership

Actual Flourish source, styles, component implementations and public contracts are the authority for visual and interaction behavior. Read those sources and existing Gallery/host usage before changing controls. Documentation is an integration/reference map, not a competing visual token system.

Library controls own generic rendering, geometry, paint, state, validation display, accessibility, keyboard/pointer behavior, popup lifecycle and disposal. Hosts own business/backend logic, authorization, domain/session behavior, persistence, application translation modules and approved semantic brand configuration. Hosts compose public controls and slots rather than clone controls, reskin them or implement a parallel generic UI layer. Native GET/POST handling may serve a host protocol boundary without replacing library controls.

Use the existing mechanisms and extension points. When capability is missing, identify its requirement and impact, continue independent supported work and obtain direction before introducing a custom replacement or expanding the task. Never present unsupported or skipped work as complete. WPF uses native rendering and binding, with the current Blazor controls/Gallery as its product reference; this does not authorize browser or assembly dependencies in native Framework.

## Project and package boundaries

Core remains UI/platform-neutral. Framework owns controls and neutral text contracts; optional Design and Culture adapters are separately registered. Neither Framework depends on its optional Design or Essential bridge. Preserve source and package-reference direction, required assets and existing package APIs.

Gallery is the maintained runnable example/API catalog and carries fictional business samples. It is not temporary test output. Maintain component usage/API reflection, localization and focused automated coverage with production entries; do not create compatibility aliases or move generic rendering into Gallery.

The six-package Core/Blazor release set is defined by ReleaseSettings. Native WPF/WinUI source is outside that publication set unless explicitly authorized. Local candidate builds, Git commits and source pushes do not authorize a version bump, tag or NuGet publication. Existing published package bytes and release tags must never be replaced.

## Operations and verification

Root launchers are SAF's origin/local and project-owned debug. Debug retains the Gallery/diagnostic launcher contract; production preparation/publication uses scripts/release.bat and its existing PowerShell implementations. Numbered discovery, the runnable allowlist, parameter validation, correct working directory and exit codes remain owned by the launcher. No operation may silently kill unrelated processes, install global tools or expand a missing desktop prerequisite.

Do not use Computer Use to test this repository. Use appropriate source/static/build/automated checks and supply manual browser/native-window acceptance steps. Preserve all maintained automated tests and necessary assets; generated builds, transient consumers, snapshots and one-off verification outputs are disposable.

Use bounded subagents for separable work when permitted; keep write scopes disjoint and integrate/verify their results. Update affected AI documents in the surrounding style, explain material changes and causes, and distinguish actual verification from historical evidence or remaining work. Keep append-only records and human ownership boundaries intact. The authorized DocFX removal does not select or authorize a replacement documentation system.
