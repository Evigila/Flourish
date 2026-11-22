# Compact action grids and stack launcher entries

Recorded 2026-11-21 in America/Sao_Paulo (UTC-03:00).

## Changes

- Ordinary UniformGrid titles and action labels use the body role. Supplied icons remain visible above the label with a compact natural track. IconSupport can reserve a track for aligned entries. An ordinary cell without an icon uses its normal text layout. Information cells retain their separate label/value hierarchy.
- Default Rectangle cells use 70% of the previous height at the same width, with a 182px cap and independent horizontal wrapping threshold. The default Square cap is 196px per side. Standalone action tiles and FormActions minimums are 77px and 67.2px. Explicit caps, fixed heights and naturally growing content retain their sizing contracts.
- AppLauncher entries stack a decorative icon over a visible service label. Hosts can supply ServiceLink.Icon and older link lists use the Apps icon. Each entry remains one native link in the existing popup. The header trigger retains its nine-dot icon and accessible name.
- A missing Primary or Accent value in partial configuration now resolves through the current Design defaults.

## Verification and delivery

- Six local 1.4.9-preview.7 packages passed identity, dependency and asset verification, including 119 pinned Noto files and notices. The production build had zero warnings or errors.
- Automated browser measurements verified rectangle and square defaults, a constrained square, retained explicit and fixed dimensions, body-size and icon placement, long content-height labels and launcher stacking. The existing popup tests also exercised native links and keyboard behavior.
- Manual acceptance covers representative long translated labels, tile actions, and navigation through all configured service links. Source package version declarations remain based on 1.4.8.
