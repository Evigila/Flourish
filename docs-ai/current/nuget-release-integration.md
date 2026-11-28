# Package release and integration

## Source, packages and scope

Directory.Build.props declares Flourish 1.4.12-release and EssentialCultureVersion 1.4.0. The .NET 11 SDK builds the release, and the public Blazor packages continue to target .NET 10 for consumption by .NET 10 and .NET 11 applications. NuGet classifies suffixes, including `-release`, as prerelease versions. Earlier releases and their candidate hashes and consumer evidence remain in their original release records.

ReleaseSettings defines six ordered IDs: Arkheide.Flourish.Core, Blazor.Abstract, Blazor.Framework, Blazor.Design, Extensions.Culture.Blazor and the dependency-only Blazor aggregate. Verification checks identities, exact local dependencies, Essential versions, architecture-neutral managed assemblies and required static/icon/license resources. WPF and WinUI are excluded. Do not infer package API ownership from the convenience aggregate.

Build/tag/version/public API identities are separate. Existing version bytes and tags must not be overwritten. A source push does not create permission to change VersionPrefix, mint a release tag or publish packages; those need their applicable explicit scope.

## Operator entry and immutable publication

Use scripts/release.bat. Default Prepare runs the existing changelog, restore/build/test/pack and package-consumer checks. Explicit Mode Publish requires clean master, exact remote/source agreement, a canonical suffixed version, a fresh tag, the approved release notes file and confirmation of the exact tag. SkipBuild verifies existing package output.

Publish fetches the relevant branch and tags, requires HEAD to equal origin/master, and creates the annotated tag with the approved title and notes. NuGet publication uses the GitHub nuget environment and Trusted Publishing. Existing package versions and tags are immutable.

The build workflow runs on master pushes/pull requests/dispatch and version tags; its publish job is restricted to the version-tag event. NuGet login uses scoped OIDC permission. Repository review, environment protection, action refs and the publish allowlist remain independent controls. DocFX/Pages workflow and tooling are removed; no replacement site is deployed.

## Verification pipeline

Test-Release stages an isolated restore/cache, builds production packages and verifies required entries, then validates the source-based Gallery graph and Essential package identities. It runs the selected solution tests, maintained console/JavaScript/script/CSS/catalog/changelog checks and independent package-only consumers. Verify-PackageSet separately validates an already prepared/downloaded package set.

A candidate feed must have immutable reviewed versions/bytes and source provenance. Explicit EssentialPackageDirectory is only for intentionally supplied candidates; normal dependencies restore through the established NuGet.org path. Do not edit cached packages or trust a prior test count for different source. Keep required source assets and licenses even while deleting generated output.

Framework-only, Design-enabled, aggregate and optional Culture compositions have distinct consumer obligations; verify the modes actually supported by the gate. Native WPF has its own focused build/tests/consumer harness and does not enter publication merely because its source exists.

## Evidence and acceptance limits

Generated package/feed/consumer/HTTP/render/verification folders are disposable after their relevant durable metadata is recorded. Historical successes identify their original commit/source/archive graph only. Cleanup does not run a new package upload, restore every desktop graph, certify WASM/Interactive Auto, accept visible geometry or prove a live consumer serves the current revision.

Read [dependencies](1_dependency.md) for exact declarations/captures and [startup](local-startup.md) for runnable examples. Before a separately authorized release, run the complete appropriate gate on the intended source, inspect package identities/assets and actual workflow artifacts, then manually check the supported browser/native surfaces and representative independent consumers. Unknown license attribution, runner/toolchain facts and unpublished native support remain explicit limitations.
