# Unified package release and current documentation

The current release target is Flourish 1.1.0 for all eight packages. Essential.Culture 1.3.0 is an external NuGet dependency of the two optional Culture bridges; Framework, Design, Abstract and Shared retain independent library boundaries.

## Changes

- Added nuget-release-integration.md with the exact six-package Essential prerequisite, eight-package Flourish order, real helper parameters, local-feed verification and user-confirmed tag flow.
- Documented the existing nuget environment, NUGET_USER profile-name secret and Trusted Publishing policy fields from each actual origin and .github/workflows/build.yml.
- Corrected active Culture/solution guides that still described automatic external source references, EssentialCultureRoot/UseLocalEssentialCulture, separate extension 1.0.0 versions and culture-v release tags. Existing dated evidence remains historical.
- Updated the sole currentproject-architecture.md directory map from the actual maintained files. It covers 948 files and 140 directories, including generic standalone/bound input APIs, field semantics, static surfaces, library text resources and release scripts.
- Kept public-facing docs, portable standards, existing history and all code/tests/workflows unchanged during this documentation task.

## Evidence and limits

The helper scripts, ReleaseSettings, package metadata and actual Git origins were inspected. The coordinating implementation task reports successful release checks and package verification. This documentation task did not rerun builds, pack packages, commit, create tags, push or publish.

Local artifacts and sibling feeds establish package-boundary verification only. They do not establish NuGet.org visibility or validate the user's external Trusted Publishing policies. Publish remains separately user-confirmed after reviewed changes are committed and pushed to a clean master matching origin/master.

## Acceptance

Check both Prepare runs, Essential's six public package versions before Flourish publication, each nuget environment and policy, a fresh NuGet-only consumer, required static assets and independent scoped culture changes. Use the new release guide for the precise commands and user-owned final publishing step.
