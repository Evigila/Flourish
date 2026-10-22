# Bounded operation pages in portal preview four

Recorded 2026-10-20 21:24:04, America/Sao_Paulo (UTC-03:00). Local source version: 1.4.3-preview.portals.4; earlier .1/.2/.3 artifacts and append-only records remain intact. Public 1.4.2 is unchanged.

The previous service composition used a centered PresentationBand around PageBody. Different active-method heights changed the centered group's upper edge; PageBody added document bottom spacing, and direct Section added 44–84px of page-section padding inside an already padded details card. Those behaviors are valid for their document/presentation scenarios but do not provide a stable operation-page frame.

AccessLayout now fills an existing bounded ContentSurface stage. Brand and stable return links sit above the remaining-height body. Wide selects the established 960px instead of 560px; Dotted consumes existing semantic roles. It introduces no second main, authentication state or GET/POST transport. NavigationChoices.FillHeight defaults false, preserving natural existing layouts. When enabled, the selected panel grows naturally to the body-height limit and then scrolls its complete content; hidden panels reserve no size, the sidebar stays top-aligned and narrow screens use a compact navigation row. Direct panel Sections suppress page-section top spacing even after an error Notice.

LogoDisplayer.Compact defaults false. Its opt-in horizontal image/name/context layout uses 48px, 32px and 18px respectively and scales down for short viewports while preserving configured identity and semantic headings. Ordinary artistic identity is unchanged. Brand action rows retain standard controls and their requested alignment. Narrow hosts with floating preferences reserve 80px above identity. At <=360px visual height, a scrollable brand and operation-body fallback keep controls reachable; this exceptional case deliberately permits the method navigation to move within that body instead of clipping submit actions.

The Gallery adds an actual AccessLayout sample, registers its public API/usage and trilingual descriptions, and uses the bounded composition on the native access-method route. Its 540px wrapper is only a catalog preview host. The same two native account/organization choices and fictitious demo behavior remain. Gallery release notes group the increment under 1.4.3-preview. No external dependency changed.

Actual verification by this bounded task: node --test tests/Tests.Flourish.Blazor/access-layout.test.mjs tests/Tests.Flourish.Blazor/palette-checks.mjs passed all 33 checks, comprising 7 new layout checks and the existing 26 palette checks. The new script is registered in the existing ReleaseSettings JavaScript list. Three actual renderer regressions were added for stable Brand/native POST boundaries, Compact identity semantics and defaults, and FillHeight opt-in/native form/current/hidden-panel behavior across all layouts. Their execution, final build/pack/Gallery and consumer restore belong to the coordinating task and are not claimed here. The earlier .3 results certify only that earlier candidate. Audit state remains unchanged.

Recommended manual acceptance after the coordinated .4 restore:
1. At 1366x768 and 1179x687, switch both native method links repeatedly; verify the brand and method navigation keep the same top position and only the selected card changes content height.
2. At narrow width and at browser zoom, inspect the compact method header, preference controls, wrapping labels and complete keyboard access. Scroll inside a long details panel to reach its submit button.
3. Trigger an authentication error; verify the Notice does not restore a large gap before the form heading or move the brand/navigation.
4. With a viewport height <=360px or the soft keyboard open, use the fallback scroll region to reach every field, action and return link without clipping.
5. Compare ordinary Account identity with its previous appearance; Compact must apply only where explicitly requested. Check Light/Dark readability and standard selected Primary hover/focus/press states.

No Computer Use, cloud/database operation, normal AppHost restart, commit, push or public publication was performed by this bounded task.
