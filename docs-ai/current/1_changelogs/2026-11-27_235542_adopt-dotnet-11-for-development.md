# Adopt SDK and application runtime .NET 11

Recorded on 2026-11-27 at 23:55:42 America/Sao_Paulo (UTC−03:00).

SDK 11.0.100 builds the solution and the release workflow. Gallery applications, the Framework-only diagnostic host, WinUI source and automated test projects run on .NET 11. Core, the published Blazor package set and the WPF library projects retain .NET 10 consumer targets. CI provisions both SDK versions so its package-consumer fixtures can run on .NET 10. Package identities and the Essential Culture dependency remain unchanged.

The Gallery uses asynchronous palette validation, dispatches grid changes through event callbacks and handles JS disconnection during its reconnection example. Test-specific JS analyzer exceptions cover in-process fakes.

## Verification

SDK 10.0.404 built the original Blazor solution. SDK 11.0.100 built all 22 updated projects with warnings treated as errors. Both builds passed with zero warnings and errors. The Blazor Framework, Gallery and Culture executable suites passed 10,085 checks. Core and WPF passed 536 xUnit tests. The browser-independent JavaScript runs passed 353 Node checks and 29 menu DOM checks.

All six local package artifacts passed metadata, dependency, font and static asset verification. Consumers restored those artifacts into a fresh cache and published the expected CSS and JavaScript for both `net10.0` and `net11.0`. Native desktop startup and real browser interactions remain manual acceptance.

## Release candidate

The `1.4.12-release` candidate uses the suffixed public channel. The annotated tag and the Gallery history use the exact package version. NuGet indexes the version as a prerelease. Release preparation passed with zero build warnings and errors, including verification of all six package archives, the .NET 10 consumer harness, and an independent .NET 11 package-only publish. See the [release and integration procedure](../nuget-release-integration.md).
