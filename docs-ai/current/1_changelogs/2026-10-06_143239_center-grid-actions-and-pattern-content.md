# Center grid actions and pattern content

Local timestamp: 2026-10-06 14:32:39, America/Sao_Paulo.

## Symptoms and causes

The /records/sample email action was left-aligned under its label. UniformGridItem uses align-items:center, but Design Button explicitly sets align-self:start, overriding its parent's alignment.

The /patterns composition example retained PageBody but omitted Fluid. Application LayoutOptions defaults Fluid=true and Gallery does not override it. The page expanded with a fluid gutter on wide screens. This is not evidence of a deleted ContentContainer; an additional wrapper is unnecessary.

## Changes

Framework wwwroot/uniform-grid.css centers direct Button children of passive UniformGridItem, including the icon-copy slot. Selector specificity exceeds Button defaults with either asset order. Direct-child targeting preserves nested menus and interactive UniformGridButton geometry. No Gallery skin, inline override or new API was added.

Gallery SurfacePatterns.razor sets PageBody Fluid=false. Heading, status and table use the existing centered gutter. Global defaults and row/menu/form/bottom-Dialog behavior remain unchanged. The active API guide records the composition; earlier uncommitted work and history are preserved.

## Verification

Isolated Gallery Release compilation has zero warnings/errors (artifacts/gallery-alignment-build.log). The existing Blazor suite passes 356/356 (gallery-alignment-tests.log); catalogue remains 80 components, 683 rows and 307 defaults. Compiled bundled Framework CSS contains the fix.

The existing local Framework 1.1.0 candidate was rebuilt without dependency/version changes. All six archives pass package graph/dependency/asset checks (gallery-alignment-pack.log and gallery-alignment-package-set.log); the other five were not rebuilt here. Actual packaged staticwebassets/framework.css contains the new rule. Repository diff check passes with normal newline settings.

No physical browser geometry, Computer Use, application termination, dependency change, public publication, deployment, Git commit/push or business/data change occurred.

## Manual acceptance

1. Restart Gallery, then inspect /records/sample: the email action is centered under its label and retains its mailto destination.
2. Inspect passive grid actions with/without icons, at wide/narrow widths. Nested menus and ordinary buttons elsewhere retain their own alignment.
3. Inspect /patterns: heading, status and table share a centered width on wide screens, with narrow gutters and no horizontal overflow.
4. Exercise search, record actions, creation and bottom Dialog opening/closing for unchanged behavior.
