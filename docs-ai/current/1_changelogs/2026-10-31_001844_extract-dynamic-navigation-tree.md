# Extract shared dynamic navigation

Recorded: 2026-10-31 00:18:44 America/Sao_Paulo (UTC-03:00).

Extracted ApplicationShell's secondary/child renderer into public NavigationTree using the existing NavigationItem contract. Shell and dynamic NavigationSurface hosts share native lists, independent SplitButton disclosures and one internal route-selection helper. The tree preserves original links, supports absolute-root organization selection, and Shell explicitly retains its original base-relative path contract.

Hierarchical semantic keys retain manual collapse across fresh item records and support existing parent/landing-child shared destinations. Current-node identity/generation checks reject stale callbacks after replacement/removal, and disabled ancestry removes descendant targets. A library-owned Surface adaptation retains a vertical narrow-screen hierarchy with existing shared rounded child backgrounds. No host clone or global per-user ApplicationOptions mutation is introduced.

Gallery adds the executable three-language guide, parameter meanings and actual Blazor Framework navigation guidance. The initial whole-tree unique-Href check exposed an established landing-child contract and was corrected without weakening the original regression. Final Release build has zero warnings/errors; Framework522, Gallery9023, fresh consumers155, other Node289, controller/actual-CSS geometry44 with zero skips, Culture25787, changelog200, CSS bundle21 and CSS SDK194 passed.

All six packages are frozen locally as `1.4.5-preview.4` in `artifacts/navigation-tree-20261031/nuget`. Provenance records 33 changed source/test/build hashes, six archive hashes, the unchanged 33 external package/version pairs and eighteen retained older preview archives. Source VersionPrefix remains 1.4.4. Complete tree: 731 files / 121 directories. No Computer Use, Colligere edit, external dependency upgrade, Git action or public publication occurred. See [the implementation and acceptance record](../1_bugreports/2026-10-31_shared-dynamic-navigation-tree.md).
