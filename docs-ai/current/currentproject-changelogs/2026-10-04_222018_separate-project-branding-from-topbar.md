# Separate project identity from top-bar presentation

Recorded using project local time: America/Sao_Paulo.

## Changes

- Add ConfigureProject(Action<IProjectBuilder>) to the Abstract public builder contracts. Project identity owns SetProjectName, SetLogo and SetFavicon. Name defaults to Application, Logo to the bundled white Material Symbols browse SVG, and dedicated Favicon to null. Favicon takes precedence for the browser tab; otherwise Logo is used. SetLogo() restores the built-in asset; SetFavicon(), null or empty clears the dedicated tab icon.
- Remove ITopBarBuilder.SetAppName/SetIcon as requested. Replace them with DisplayProjectName(bool display=true) and DisplayLogo(bool display=true). This intentionally changes those existing call sites. Keep the older explicitly obsolete ITitleBarBuilder.SetApplicationTitle compatibility path, forwarding to project identity. Freeze captured project/top-bar builders after registration and retain local-asset-path validation.
- ApplicationLayout outputs the resolved tab icon through its existing HeadContent beside the library styles. Gallery App no longer hardcodes a competing favicon. Embedded low-level ApplicationShell does not output head content. Display switches affect only the top-bar presentation, respect runtime brand/title slots and never change the configured favicon. Hiding both avoids an empty brand link.
- Gallery uses its white browse Logo at 0.8 scale: visible geometry decreases from 36px to 28.8px inside the existing 48px brand target. A separate browse SVG in Light Primary #153A32 serves browser tabs. The Framework default asset uses the existing licensed source at the established revision, with no new font, package or service.
- Change primary Framework/Controls/Foundations/Examples symbols to responsive_layout/crossword/shapes/explore, and the fixed theme action to routine. Home and other navigation items retain their icons. The provided explorer drawing corresponds to the bundled explore name. Home's related destination cells use the same new symbols.
- Design ActionMenu option text uses weight 400 at its existing 17px size. Preserve trigger emphasis, command semantics and popup interaction behavior.
- Migrate Gallery setup snippets and the Framework-only host; add six focused public-boundary/rendering checks; update current implementation/manual guides and the explanatory tree to 897 files/132 directories. No README, external dependency, Culture implementation or Colligere change.

## Verification

The normal fourteen-project root solution builds with zero warnings/errors. Blazor checks pass 111/111, including defaults, setter-order-independent favicon priority/reset, four display combinations, immutability, local paths/HTML encoding and removal of old top-bar setters. Catalog coverage remains 77 components, 546 rows and 253 defaults. Existing palette checks pass 10/10 and accept only approved SVG paints.

Headless installed Edge verifies the single Primary tab icon, white Logo with measured 28.8px geometry, default asset 36px, all six requested navigation symbols, Home destination icons, 17px/400 popup options, hover close, retained Framework/Design stylesheet links, 320px containment and the migrated top-bar guide. The Framework-only host supplies and serves its default browse tab icon without Design. Temporary hosts use dynamic ports and stop only their own processes; no Computer Use or VS host restart.

An initial cold browser run reported one resource 404 without its URL being captured. After adding failed-response and console-location instrumentation, four separate fresh browser contexts passed without failed responses or browser exceptions; the original isolated request could not be identified. Do not infer a source diagnosis from that unlocated response. Native browser tab appearance/cache, other engines and accessibility remain manual acceptance.

## Manual follow-up and save status

Restart Gallery and fully reload. Check the smaller white Logo, separate green tab icon and new navigation artwork. Hover the page menu and inspect ordinary-weight options. Test display switches independently, omit/clear Favicon to use Logo, and restore the default Logo through SetLogo(). Keep a normal HeadOutlet and avoid a competing host favicon link. The detailed checklist is in blazor-manual-tests.md.

No commit or remote push is performed for this new task before the user's completion-stage confirmation required by AGENTS.md.
