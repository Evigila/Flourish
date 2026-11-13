# Navigation/menu interaction polish and Material Symbols

The user requested single-line secondary labels, one disclosure button per parent row, hidden menu scrollbars, brighter shared third-level panels, Google's newer Material Symbols, and inset hover triggers with weak top-bar popups. Changes are limited to Flourish; Colligere was read only as reference.

## Navigation and optional skin

ApplicationShell now renders an item with children as one native button containing the icon, label and decorative triangle. Every part toggles the same branch in both directions without navigating or closing the narrow-window rail. Leaf-only items remain route links. Labels truncate to one line while preserving full title and accessible text. Existing selected ancestry, direct reload, scoped expansion, disabled ancestors and the AddSubNav configure-lambda API remain intact. The mobile focus loop now includes the combined buttons.

Framework hides navigation/menu scrollbars while retaining scrolling. The rules cover the default shell, ActionMenu, NavigationSurface rails, ServiceMenu views and RowActionMenu panels. Design removes its conflicting thin-scrollbar declarations. Third-level siblings share one brighter rounded panel: near white in Light mode and lighter than the surrounding surface in Dark/dark System modes. This supersedes the darker-panel/separate-arrow presentation in earlier append-only records.

## Top-bar menus

Program-configured AddMenu entries automatically enable the new public ActionMenu.OpenOnHover parameter. Mouse entry opens without moving focus; the trigger region meets the popup without a pointer gap. Moving between them retains the popup, and leaving both closes it immediately. Keyboard/touch activation, arrow navigation, Escape, focus departure, outside interaction, disabled state and disposal remain supported. Trigger arrow events stop propagation so document navigation does not process the same key twice. Standalone ActionMenu defaults to click mode; Gallery demonstrates both modes and exposes a scenario switch.

Design constrains the top-bar hover/pressed background to a 48px rounded inset trigger. The compatible ServiceMenu receives the same inset styling and retains its own CSS hover/focus behavior. Gallery's framework explanations and category guidance now describe the unified interaction.

## Authorized icon migration

Framework replaces the classic Material Icons OTF/map with Google's Material Symbols Outlined variable WOFF2 and 4299-name map from revision 737e3324305806514d7909874fa1818ae1808232 of https://github.com/google/material-design-icons. Source hashes, axis ranges/defaults and the Apache-2.0 license ship with the font. Icon, AppIcon and Glyph share the new family; existing default sizes and compatibility aliases remain. No CDN, NuGet/npm dependency or Gallery-specific asset is introduced into the library.

The official complete font is 4003088 bytes, substantially larger than the classic font. A cold request transfers approximately 4MB. The pinned map and variable-font metadata were checked against the font's glyph coverage. Names unique to the classic set are not all supported by Symbols; unknown names retain help_outline fallback. Current Framework/Gallery references and compatibility aliases are covered. See blazor-extraction.md for ranges, hashes and compatibility limits.

## Verification and limits

- Gallery and console test builds: zero warnings and errors, using temporary output directories to avoid the user's running VS binaries.
- C# data/DI/SSR regression suite: 63/63 passed, including unified disclosure state, routing, disabled ancestry, top-bar hover configuration and the pinned 4299-name catalog.
- Mocked controls DOM suite: 23/23 passed, including weak hover dismissal, trigger-to-popup crossing, no focus theft, click/touch/keyboard fallback, aria refresh and listener cleanup.
- CSS bundle suite: 21/21 passed.
- An owned temporary Gallery host returned all 73 independent component guides and eight lightweight category directories. Its outer shell had eight combined branch buttons and 73 leaf links. Updated menu API, hover mode, CSS and JS were present in actual responses. The host was stopped.
- Font HTTP response: 200, font/woff2, exact byte length and SHA-256. The Development stable route uses Cache-Control=no-cache; an If-None-Match repeat returned 304 without retransmitting the font. This is cache protocol evidence, not a browser timing measurement. Evidence logs are in the local temporary directory under flourish-menu-http-a181ca6d879747a5b112bee2d03d4bfc.
- Local Framework NuGet package contains the new font, LICENSE.txt and source.json; the old font is absent. NuGet's missing-README advisory is expected under the user's README-removal policy. No package was published.
- git diff --check passed. Current architecture and manual acceptance documents were updated; historical records and the user's pending skill deletions were preserved.

No Computer Use or browser pixel/assistive-technology acceptance was performed. The updated blazor-manual-tests.md lists pointer, keyboard, touch, long-label, hidden-scrollbar, theme and cold/warm-font checks. No Git commit was created; confirmation remains the completion step required by AGENTS.md.
