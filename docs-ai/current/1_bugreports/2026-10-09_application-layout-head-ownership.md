# ApplicationLayout guide navigation flicker: diagnosis

## Status and symptom

Analysis only, 2026-10-09. The user reports page flicker when entering or leaving the ApplicationLayout guide. No component or Gallery behavior was changed during this investigation. Visual browser reproduction remains unverified.

## Cause and source evidence

`ApplicationLayout.razor` registers `HeadContent` unconditionally. Its `OwnsDocument` parameter is passed only to `ApplicationShell`; setting it to `false` does not disable document-head resource ownership.

The Gallery route uses an outer ApplicationLayout. ComponentGuide renders its registered sample twice, once in the control example and once in the scenario example. ApplicationLayoutSample embeds a real ApplicationLayout with `OwnsDocument=false`. The guide therefore creates two additional providers for the same global head section.

Changing the active head provider replaces the section renderer and its contents, including the framework and design stylesheet links. Entering the guide changes ownership to the embedded samples; leaving it returns ownership to the outer layout. Replacing global stylesheet nodes can interrupt style application and cause reflow or repaint, even when their URLs are identical and their files are cached. This is the leading explanation for the reported two-way navigation flicker.

Relevant sources:

- `src/Flourish.Blazor/Flourish.Blazor.Framework/Components/ApplicationLayout.razor`: unconditional HeadContent and the limited OwnsDocument forwarding.
- `src/Gallery.Flourish.Blazor/Components/Routes.razor`: outer ApplicationLayout.
- `src/Gallery.Flourish.Blazor/Components/Catalog/ComponentGuide.razor`: two DynamicComponent instances.
- `src/Gallery.Flourish.Blazor/Components/Samples/Layout/ApplicationLayoutSample.razor`: embedded ApplicationLayout.

## Executable evidence

A temporary, dependency-free .NET 10 diagnostic executable used the existing compiled production assemblies, the actual ASP.NET Core HeadOutlet, and a custom Renderer that recorded render batches. The harness retained an outer ApplicationLayout and added or removed two keyed embedded ApplicationLayout instances. JavaScript calls were mocked; the probe measured render-tree ownership and replacement, not browser pixels or timing.

Observed head content renderers:

| Transition | New renderer | Disposed renderer | Assets recreated |
| --- | --- | --- | --- |
| Outer layout only | 12 | Initial empty renderer 4 | Browser icon, framework.css, design.css |
| Add first embedded layout | 21 | 12 | Same assets |
| Add second embedded layout | 30 | 21 | Same assets |
| Remove embedded layouts | 33 | 30 | Same assets |

Each new renderer emitted `PrependFrame` edits for its content. The probe built with zero warnings and zero errors and completed successfully. Its temporary files were removed after investigation.

## Other paths reviewed

- The sample's style rules are scoped to its generated container ID, including its 420 px shell height.
- Embedded shells do not receive the document-owner attribute used by viewport CSS. The outer shell remains the document owner.
- Shell JavaScript measures and disposes individual roots; it does not replace head stylesheets.
- Appearance registration supplies a stable design stylesheet URL. Sample mounting does not intentionally change the theme.
- Source CSS imports are flattened during asset generation; this diagnosis does not depend on runtime nested import requests.

## Proposed mitigation

Make global head registration conditional on document ownership. An ApplicationLayout with `OwnsDocument=false` should render its embedded shell without becoming a head-section provider. Keep the real executable ApplicationLayout sample and the existing outer resource owner.

This mitigation has not been implemented. Browser inspection is still needed to establish whether additional contributors remain after removing stylesheet replacement.

## Regression and manual checks

Add an executable regression with a real HeadOutlet, one outer ApplicationLayout, and two embedded instances. Entering, updating, and leaving the embedded examples must preserve the outer head renderer and stylesheet nodes.

Manual verification checklist:

1. Navigate repeatedly from another control guide into ApplicationLayout and back. Check both entry and exit for a blank frame, missing styles, or layout jumps.
2. Inspect head DOM mutations during these transitions. The existing framework.css and design.css nodes should retain their identity after the proposed fix.
3. Repeat with normal caching and browser cache disabled. Test light and dark appearances and narrow and wide viewport sizes.
4. Confirm both embedded examples still render and their local actions work. Confirm outer navigation, scrolling, and theme switching still work.
