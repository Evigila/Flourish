# Blazor application color roles

This is the user-approved current palette and interaction contract for Flourish.Blazor Design and Gallery. It contains seventeen roles with independent Light/Dark values. Framework-only consumers retain native system colors.

This explicit project exception supersedes the portable standard's generated custom-theme shades, separate focus paint and full semantic-notice palette for Blazor. Independent subtree themes are removed. Common standards and other platforms remain unchanged; earlier contracts and run evidence stay in append-only history.

## Fixed palette

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
| Danger target preview | --f-preview-danger | #7E2824 | #8C3430 |
| Danger target click | --f-click-danger | #712420 | #7E2F2B |
| Light target click | --f-click-light | #B5C9C4 | #C0CEC8 |
| Dark target click | --f-click-dark | #2A4842 | #335141 |
| Info text | --f-info-text | #1565C0 | #8CB8FF |
| Warning background | --f-warning-background | #F2A33A | #E5A047 |

foundation.css defines literal palette values once; other styles consume roles/aliases. Transparent paint, currentColor, opacity and approved alpha effects do not create another paint role. --f-surface-light names the existing white Light Surface value for reuse; it is not an eighteenth role. Fixed click values equal the corresponding preview RGB channels multiplied by 0.9 and rounded, without runtime color-mix.

Dark Primary is intentionally light. Light chrome uses Primary/Surface; dark chrome uses Canvas/Text. Ordinary Dark selections can use Primary with Surface ink while primary navigation on dark chrome uses Accent/Surface.

## Hover, press, selection and focus

- Keyboard focus uses Accent; no independent focus paint exists.
- Hover selects the preview for the target's actual brightness: ordinary Light surfaces use Light preview; Dark surfaces/dark chrome use Dark preview. Primary-filled controls reverse that mapping in Dark because Dark Primary is light.
- Held press/native activation uses the corresponding fixed click token. Release restores hover/idle/persistent selection. Selection survives hover and still shows press feedback; foreground stays readable.
- Ordinary secondary/third-level and other persistent selections use Primary/Surface. Primary navigation on dark chrome uses Accent/Surface in both modes. A branch link/disclosure share selection independently of expansion.
- Third-level panels and popups use Surface. A popup nested under chrome resets ordinary surface preview/click roles. Display-choice labels always use Text; idle rows remain transparent and the Primary-filled checkbox/check conveys selection.
- Danger Filled buttons/grid cells and destructive menu choices retain red on interaction: Danger preview/click plus the existing white --f-surface-light ink. Idle Danger and error/validation paint retain their normal role mappings.

## Component mappings

| Component or purpose | Paint |
|---|---|
| Filled Button / grid cell | Primary / Surface |
| Outlined Button / grid cell | Canvas with Border |
| Elevated Button / grid cell | Surface with shared approved shadow |
| Quiet control idle | Transparent |
| Danger Button / cell | Danger / Surface; deep red preview/click with Surface-light interaction ink |
| Table header | Border; sortable text feedback stays on its inner button |
| Popup / native enhanced select | Surface with Text and surface preview/click roles |
| Code/control DisplayBoard | Display board; optional dots use Border |
| Card / IdentityCard | Primary / PrimaryInk alias |
| Dividers, outlines and legacy line aliases | Border |
| Error / validation / legacy danger aliases | Danger |
| Shadow / modal backdrop | Primary with approved alpha |

UniformGrid has no enclosing rectangular fill/shadow behind missing cells; occupied cells own these variant mappings. Dotted is optional; code remains upper-left regardless of ordinary content centering.

Notice and Primitives.StatusNotice share one Design semantic mapping:

| Severity | Background / foreground |
|---|---|
| Information | Surface / Info text |
| Success | Primary / Surface |
| Warning | Warning background / Warning ink |
| Error | Danger / Surface |
| Subtle | Display board / Muted |

--f-warning-ink aliases Text in Light and Surface in Dark, adding no paint value. StatusNotice retains paragraph markup and its independent Role/Announce API. Announcement, validation and destructive-action behavior remain semantic; color alone never supplies meaning.

NoticeTrigger uses a 17px circle in Design (1em native). Error uses a cross and Danger/Surface, Warning an exclamation and Warning background/ink, Success a check and Primary/Surface, Information and compatible Subtle an exclamation and Surface/Info text. Hover/press preserve severity; focus adds Accent. Actual accessible names/descriptions remain available.

Progress fill is solid Primary. Its approved moving 105-degree sheen has transparent ends at 20%/80% and 47% --f-surface-light white at 50% in both themes. It is a clipped alpha overlay, not a second progress-fill palette. No unrelated private gradient is authorized. Reduced-motion behavior is documented in the implementation/manual guides.

## Configuration and compatibility

AddFlourishDesign, IAppearanceBuilder.SetColors/SetTheme and scoped IAppearanceService remain supported. ApplicationLayout applies one theme; ThemeScope and its independent guide are removed.

Primary/Accent remain configurable #RRGGBB seeds. Default Light seeds select the approved Dark defaults in Dark mode; custom seeds remain exact in both modes. Four mode seed variables let CSS choose active colors without an inline Light value overriding Dark. AppearanceState stores configured seeds and Foundations swatches show active roles. System uses Dark only under prefers-color-scheme:dark.

Custom seeds do not generate foreground/focus/hover shades; hosts choose readable seeds against Surface/previews. AppearancePalette.Contrast is a public diagnostic utility, not another paint source. Font/reading scales are separate approved mechanisms in the [implementation guide](blazor-extraction.md).

Legacy extracted aliases such as --line, --danger and --surface-theme-accent map to these roles for compatibility. Historical arbitrary grays/mixes, separate focus shades, black shadows and preview-filled child panels do not add active colors. WPF/WinUI resources, negative test inputs, historical reports and external hosts' approved business palettes are outside this contract.

Use [manual acceptance](blazor-manual-tests.md#reading-sizes-icons-and-colors) for real hover/press/release, custom seeds, popup layering and supported-browser checks. Current catalog totals are 77 guides, 546 API rows and 253 documented defaults.
