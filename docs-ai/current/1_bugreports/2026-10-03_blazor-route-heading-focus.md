# Route heading focus outline

## Symptoms

After initial Gallery hydration, the entire overview H1 looked like a selected field. The same rectangle appeared after route navigation. An installed Edge sample reproduced document.activeElement.tagName === "H1" and a solid 3px outline.

## Cause and evidence

Gallery uses the native Blazor FocusOnNavigate component with Selector="h1". The library foundation applied its generic focus-visible rule to that programmatic target. The source Colligere app already excludes h1[tabindex="-1"]:focus; that exception was omitted from the extraction. The source project design explicitly preserves heading announcement without an input-like rectangle.

## Mitigation

Scope the exception to .f-root h1[tabindex="-1"]:focus. Retain FocusOnNavigate and the normal focus-visible rules for interactive controls. Do not blur the heading or disable outlines globally.

## Regression evidence

Real installed Edge: initial overview H1 retains focus with outline-style none; subsequent navigation retains the exception. Tab reaches a native field with a solid 3px outline. Dirty-form confirmation, menu top layer, compact-heading hysteresis and narrow navigation checks also pass. Debug builds pass without warnings/errors; 34 C# and 21 DOM checks pass.

## Limits

These samples do not establish screen-reader announcement, forced-colors rendering, OS DPI/zoom or other browsers. They remain explicit manual acceptance tasks. The fix affects library presentation and Gallery; it does not migrate the Colligere runtime.
