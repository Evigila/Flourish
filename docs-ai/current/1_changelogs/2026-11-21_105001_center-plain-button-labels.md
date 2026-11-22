# Center plain Button labels consistently

Recorded 2026-11-21 in America/Sao_Paulo (UTC-03:00).

## Changes

- Plain Button Text keeps its natural line box so localized or wrapped labels reserve the same height as ChildContent. Structured labels retain their established line treatment, including the structured dropdown override.
- A shared center default applies to plain native buttons and links. Menu items, sort headers and top-bar branding preserve their intentional alignment. Native select, text input and table text continue to use their component alignment.
- The updated layout does not use positional offsets. See the [diagnosis and before/after measurements](../1_bugreports/2026-11-21_105001_plain-button-text-centering.md).

## Verification and delivery

- The browser regression loads the vendored Noto fonts and checks identical wrapped geometry at 14px and 17px across native and anchor Buttons, Text and ChildContent. It also checks the menu, structured label, brand and sort contexts.
- All six preview 1.4.9-preview.10 packages passed metadata, dependency and asset verification. The production build had zero warnings and errors. Browser and DOM checks passed 52/52 with no skipped real-browser cases. Style checks passed 94/94.
- Manual acceptance covers short and wrapping captions, mixed scripts, placement beside an icon and the standard start-aligned selection and table contexts.
