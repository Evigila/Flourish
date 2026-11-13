# Remove overlay separators and standardize notice indicators

## Request and causes

Dialog and both sheets carried header/footer/top-edge dividers rejected by the user. The primitive draft demo placed ordinary Buttons in the large filled FormActionBar, and Primitives.BottomSheet had no action footer. Notice Information/Warning shared Primary after the earlier color merge. NoticeTrigger used long rectangular severity badges, with a framework40px minimum height.

## Changes

Remove decorative overlay separators while retaining centered outer/control outlines and modal behavior. Add optional Actions to Primitives.BottomSheet, after its scrollable body, using the shared right-aligned flexible dialog row in Framework and gutter/safe-area spacing in Design. Replace the draft sample's FormActionBar with that slot and standard buttons; reflect Actions in the guide/API list.

Add Info text (#1565C0 Light / #8CB8FF Dark) and Warning background (#F2A33A Light / #E5A047 Dark) as centralized palette roles. Notice Information uses Surface/Info text, Warning orange and readable role-alias ink; Success/Error/Subtle are unchanged. Foundation swatches and palette audits now cover fifteen roles.

Replace NoticeTrigger text badges with17px circles in Design and1em native geometry. Use decorative SVG cross/exclamation/check; map red, orange, green and white/blue presentations through approved severity roles. Keep severity names, unique descriptions and hover/focus explanations; a transparent bridge permits pointer entry into the popup. No extra icon package, glyph typography size or business resource.

## Verification

Gallery and test builds have zero warnings/errors.102/102 .NET checks pass: new indicator naming/encoding/description checks and expanded sheet-lifecycle tests include the Actions slot. API audit:78 guides,543 rows,254 non-null defaults.27 control DOM checks,8 row-action checks and9 palette/style audits pass, including >=4.5:1 Info/Warning reading contrast in both themes.

Isolated installed headless Edge confirms divider widths0px in every requested overlay, normal close/Escape, right-aligned primitive Actions at desktop and320px, real local save with Busy lock and retained draft, exact Light/Dark Notice paint and alert semantics, five17px named circles, pointer/focus explanation and live severity updates. No browser errors occur. All78 guide routes retain their standard sections/default columns. A separate bounded agent's indicator CSS fixture also passes. Whitespace check passes; maintained source tree counts are unchanged.

## Acceptance and limits

Use blazor-manual-tests.md for overlay guards/focus, footer wrapping, descriptions, System/Dark themes, zoom/high contrast, screen readers and other browser engines. Dark information/error/success colors use the existing readable dark role values rather than hardcoded white. Temporary hosts use separate ports and preserve the user's VS process. No Computer Use, external dependency, Colligere change or Git commit.