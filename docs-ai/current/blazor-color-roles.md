# Blazor application color roles

The user approved this Flourish-specific contract on 2026-10-04, including an independent dark palette. Later corrections restored Danger, added fixed click colors and corrected navigation and display-option colors. The latest notice instruction adds Info text and Warning background, bringing the palette to fifteen roles. It supersedes the earlier merge of information/warning paint into Primary. Historical change records remain unchanged.

This is an approved exception to the portable common standard's generated theme shades, separate focus color, full semantic notice palette and independent subtree themes for Flourish.Blazor. The portable standard and other platforms remain unchanged.

## Palette

These are the only default paint colors in Design and Gallery. Framework-only consumers keep native system colors. Transparent paint, currentColor and opacity effects do not introduce another color role.

| Role | Public token | Light | Dark |
|---|---|---|---|
| Primary | --f-primary | #153A32 | #BBD7C9 |
| Accent | --f-accent | #16745F | #75CBB2 |
| Canvas | --f-canvas | #F3F5F5 | #18231D |
| Surface | --f-surface | #FFFFFF | #263A30 |
| Text | --f-text | #112924 | #E7EFEA |
| Muted text | --f-muted | #4F645D | #B8C9BF |
| Display board | --f-display-board | #E5E8EB | #31483B |
| Light target preview | --f-preview-light | #C9DFDA | #D5E5DE |
| Dark target preview | --f-preview-dark | #2F5049 | #395A48 |
| Border | --f-border | #9FAEA9 | #526F60 |
| Danger | --f-danger | #9D322D | #FFB4AB |
| Light target click | --f-click-light | #B5C9C4 | #C0CEC8 |
| Dark target click | --f-click-dark | #2A4842 | #335141 |
| Info text | --f-info-text | #1565C0 | #8CB8FF |
| Warning background | --f-warning-background | #F2A33A | #E5A047 |

Foundation defines palette values once. Other CSS consumes roles or role aliases. Surface names its light-mode white value as --f-surface-light; the active light Surface and the user-requested progress highlight reuse that value. It is an existing mode value, not a sixteenth paint role. Each click color is the matching preview's RGB channels multiplied by 0.9 and rounded to the nearest integer. These approved hexadecimal values are fixed tokens; CSS does not generate them with runtime color-mix. Shadows, backdrops and the authorized white progress sheen may apply approved alpha effects.

Dark Primary is deliberately light so ordinary selected items can use Surface as their foreground. Dark-mode chrome uses Canvas and Text; light-mode chrome uses Primary and Surface. Primary navigation on dark chrome uses Accent with Surface for persistent selection in both modes. This avoids turning the entire dark title bar into a bright primary surface.

## Interaction and component mappings

- Keyboard focus uses Accent. There is no separate focus paint color.
- Hover uses the preview appropriate to the target's actual brightness. Ordinary light-mode surfaces use Light target preview; dark-mode surfaces and dark chrome use Dark target preview. A primary-filled control reverses this mapping in dark mode because dark Primary is light.
- A held pointer press or native button activation uses Light target click or Dark target click, independently of hover and persistent selection. Releasing restores the hovered, idle or persistent selected state. A selected item keeps its selection while merely hovered, but still shows click feedback while pressed. Background changes retain a readable foreground rather than introducing a new text color.
- Ordinary secondary/third-level navigation and other persistent selections use Primary with Surface foreground. Primary navigation on dark chrome uses Accent with Surface instead. A navigation link and its split disclosure action share the same persistent selection, independent of whether their child list is expanded. Third-level siblings share a Surface panel in both modes, rather than a preview-colored panel.
- Filled buttons and cells use Primary with Surface foreground. Danger buttons and grid cells use Danger with Surface foreground; error/validation and extracted danger aliases retain the Danger role. Default Danger is dark in Light mode and light in Dark mode, matching Primary's brightness direction, so both use the same target-preview/click direction. Outlined buttons and cells use Canvas with a Border outline. Elevated buttons and cells use Surface and the shared control shadow. Quiet controls retain a transparent idle background.
- Table headers use Border. Sort-title feedback stays inside the text button rather than filling a complete column-header cell.
- Popup panels use Surface and restore the surface preview/click mapping, including when nested beneath dark title-bar chrome. Display-option labels always use Text while selected, unselected or hovered. Their idle row remains transparent; hover supplies the surface preview and press supplies its click token. The checked checkbox uses Primary and a visible check, so selection is distinguishable without changing the label to an inverted foreground.
- Code boards and control previews share Display board. Dotted is optional; code uses Dotted=false and starts at the upper-left independently of ordinary content centering. Dot paint uses Border.
- Shadow and modal-backdrop effects use Primary with alpha. Their transparency is an effect, not a second black or gray palette. Progress fill remains solid Primary. The latest user instruction restores its moving 105-degree white highlight: transparent ends and 47% of --f-surface-light at the center, in both themes. This gradient is a clipped overlay effect, not a second progress-fill palette. No other private gradient or paint literal is authorized.
- Notice Information uses Surface with Info text. In Light mode this is white with blue reading text; Dark uses its existing Surface and lighter blue. Warning uses Warning background; --f-warning-ink aliases Text in Light and Surface in Dark, preserving readable ink on orange without adding another paint value. Success remains Primary/Surface and Error remains Danger/Surface. Subtle Notice is unchanged.
- NoticeTrigger is a 17px circular body-size indicator in Design and 1em natively. Error uses Danger/Surface with a cross; Warning uses Warning background/Warning ink with an exclamation; Success uses Primary/Surface with a check; Information and compatible Subtle use Surface/Info text with an exclamation. Glyphs preserve severity while hovering/pressing; focus adds the standard Accent outline. Accessible names and descriptions remain available without the former text badge.

## Removed and merged colors

| Earlier source or use | Current role |
|---|---|
| Separate focus and surface-theme focus shades | Accent |
| Boundary, strong boundary and line colors on navigation dividers, inputs, cards, tables, popup panels and split controls | Border |
| Independent rounded-hover grays and generated pale green mixes | Light target preview or Dark target preview; a press uses the matching fixed click token |
| Selected pale accent backgrounds and navigation | Primary with Surface foreground for ordinary selection; Accent with Surface for primary navigation on dark chrome |
| Historical red danger/error and validation paint | Danger, including extracted danger/error/invalid-state aliases |
| Earlier arbitrary orange and blue values | New approved Warning background and Info text tokens for Notice and NoticeTrigger; other legacy severity presentation remains unchanged |
| Earlier preview-colored third-level panels | Surface |
| Earlier inverted display-option label colors | Text in idle, selected and hovered states; selection remains on the checkbox |
| Alternate surface grays and raised surface mixes | Canvas, Surface or Display board according to purpose |
| Black popup/card shadows and black modal veils | Primary alpha effects |
| Gallery-only dark display surfaces | Display board |
| Custom scope palettes and ThemeScope sample | Removed; application-level Design configuration |

NotificationSeverity, error labels, ARIA alerts/status and destructive-action behavior remain intact. Danger supplies error/validation paint; Info text and Warning background supply the newly requested Notice and NoticeTrigger colors. Legacy extracted CSS aliases such as --line, --danger or --surface-theme-accent resolve to the standard roles; they are compatibility bridges rather than public additional colors.

## Configuration and compatibility

AddFlourishDesign, IAppearanceBuilder.SetColors/SetTheme and the scoped IAppearanceService remain supported. ApplicationLayout applies one application theme. ThemeScope and its independent Gallery guide are removed. Gallery now contains 78 component guides.

Primary and Accent remain configurable #RRGGBB seeds. The default light seeds select the dark defaults above when dark mode is active. A custom seed remains the exact configured value in both modes. Four mode seed variables allow CSS to select the active mode without an inline light color overriding dark mode. AppearanceState stores the configured seeds; the live foundation swatches show the active mode's roles.

No generated foreground, focus or hover shade is created for an arbitrary custom seed. The host must choose seeds that remain readable against Surface and the approved previews. AppearancePalette.Contrast remains a public diagnostic utility; it does not paint or generate another color. System mode uses the dark values only when prefers-color-scheme is dark. Font and reading-size rules remain unchanged.

The audit covers production Flourish.Blazor Framework/Design assets and Gallery. WPF/WinUI resources, historical change records, negative test inputs and Colligere's host-owned business palettes are outside this change. No dependency was added.
