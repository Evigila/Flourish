# Align dropdown and data table spacing

Recorded 2026-11-21 in America/Sao_Paulo (UTC-03:00).

## Changes

- A searchable DataTable uses the native StandaloneSelectBox for its field picker. The hover language selector used a 4px grid gap, while adjacent options in the customizable native picker touched. Both paths now use the same 4px gap. Margin is applied to adjacent native options, so the first option in a group adds no extra group inset. Browsers with a classic picker retain their native rendering.
- DataSearch had a 36px document margin before the table toolbar. The toolbar and footer already used 14px. Framework now applies the 14px gap to the direct table regions, including the search, and preserves 36px for DataSearch outside a table.
- Search filtering, selection events, hover activation and keyboard behavior use the existing controls.

## Verification and delivery

- Automated browser checks compare the native and enhanced 4px gaps, verify grouped option spacing and keyboard selection, and measure all three table gaps at wide and narrow viewport widths. The standalone search margin remains 36px.
- The production build completed with zero errors and warnings. All six packages in local preview 1.4.9-preview.8 passed package, dependency and asset verification. No source version promotion or publication occurred.
