# Restore the single pagination range

The consumer's preserved pagination regressions detected duplicated range text and aria-live announcements after both table pagers defaulted to showing a range. The prior host contract had two page-button groups but only one top range. The Framework bottom Pager now explicitly uses ShowRange=false; no paging callbacks, preferences or row selection behavior changed.

A new actual SSR regression checks two data-page-controls groups and only one data-page-range with aria-live=polite. The complete local preparation passed: Core 367, Blazor 135/135, Culture bridge 12, DOM/browser 38 plus existing interaction/mock suites, CSS bundle 21 and SDK asset 194 checks. All six configured 1.1.0 packages passed verification and all four isolated NuGet-only consumers passed 67 combined checks.

Final consumer evidence: artifacts/package-consumers/aae97a0da9ff43ab82710e083fe5c955. Framework nupkg SHA256: 77C780A168A8EA21DA3C4D306408285127324CEFFA26A70DE557AC5BEDB82DBA. Earlier same-version candidates remain unpublished preparation evidence, not released packages. Downstream consumers must restore this candidate in a fresh isolated cache.

No commit, release tag, push or NuGet upload was performed.
