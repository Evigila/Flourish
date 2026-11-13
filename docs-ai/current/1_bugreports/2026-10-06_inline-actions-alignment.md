# Explicit inline action alignment

## Cause and scope

Colligere's code-redemption scene needs ordinary actions below a field and aligned to the logical end. InlineActions previously exposed only ChildContent; its design centered actions, so a consumer could not request this without introducing forbidden host styling. This is a library capability gap, not a reason for another action renderer.

## Implementation

The existing InlineActions has one Alignment parameter using the shared passive HorizontalAlignment contract (Start, Center, End). Center remains the default. Unknown enum values are rejected. The component emits data-alignment; Framework owns a bounded full-width track and the corresponding justify-content rules. DisplayBoard does not override an explicitly aligned InlineActions. No second renderer, style string, alias or compatibility entry was introduced.

Gallery demonstrates all three values. The catalog, usage guidance, documented defaults and three localized parameter descriptions match the public source API. Three added checks exercise real rendering, invalid values, Framework/board ownership and executable examples. The existing chart-toolbar test now expects the component's standard alignment attribute while still checking the actual field/input/native-submit structure.

## Verification and release boundary

Release test and Gallery builds passed with zero warnings/errors. Console checks passed 376/376; surface/toolbar/page-body Node checks passed 19/19. Diff check passed. Computed browser geometry remains a manual acceptance check: test all alignments in a centered/non-centered board, standalone form, narrow screen and both themes; verify native submit, wrapping and disabled/busy behavior remain unchanged.

This is source-only preparation. No version increment, package publication or Colligere upgrade occurred. Current public 1.1.1 consumers cannot use Alignment yet. Colligere's redemption row is therefore below the input but pending end alignment until a future approved release. Running Aspire was not stopped or restarted; no Computer Use or Git commit occurred.
