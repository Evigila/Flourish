# Balance white, silver and beige in the light palette

Recorded 2026-11-21 in America/Sao_Paulo (UTC-03:00).

## Changes

- White is the default light page canvas and the standard surface. The header, primary navigation rail and its label preview retain silver. Display boards retain their separate silver tone.
- NavigationSurface and ApplicationShell secondary navigation use the new warm-neutral beige role in Light mode. Standard popup backgrounds remain white and their hover and press states use beige. The deepest selected navigation state retains its brown accent. Focus, link and status semantics retain their existing roles.
- Dark and System-dark keep their prior colors, including distinct secondary backgrounds in the two shells. Host-supplied primary and accent seeds and their explicit chrome conventions remain available.

## Verification and delivery

- Production build passed without errors or warnings. All six packages in local preview 1.4.9-preview.9 passed metadata, dependency and asset verification, including the pinned Noto set.
- Palette checks cover both default modes, contrasts and exact press-to-preview channel ratios. Automated browser checks compare white content and popup paint, silver header and navigation, beige secondary navigation and interaction paint, brown selection, and the original Dark and System-dark results.
- The source build remains based on 1.4.8. Local preview carries the appearance changes for Colligere validation.
