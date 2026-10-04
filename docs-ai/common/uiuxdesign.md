# Portable UI and UX design standard

This is the shared visual and interaction contract for projects using this documentation system. Copy it with `AGENTS.md` when the same product-family style is wanted elsewhere. Values below are standards, not examples. `docs-ai/current/` supplies routes, business terms, specific brand assets, and user-approved exceptions; it must not silently contradict this file. Check implementation before claiming a rule is already met.

## Visual direction and typography

- Favor task-first screens: a lightly tinted canvas, high-contrast dark chrome, a flat white base surface, generous whitespace, thin dividers, and very few shadows. Use cards for genuine selection/comparison, login, identity, summary, or preview—not to fill space. Avoid zebra modules, stacked bordered panels, fabricated metrics, decorative illustrations, slogans, and section numbers.
- Use `Segoe UI` throughout. Ordinary text is exactly `17px`: body, navigation, labels, buttons, table headers/cells, auxiliary values, legends, tooltips, and parent links. Private-page H1 is responsive `42–68px` (`36–50px` in narrow windows); H2 is `28–40px`. A true product/public-portal display title may be `64–112px`. Actual metric display values and the designated primary identity value in a filled card or account overview may use a larger scale.
- Search and input labels use ordinary body weight `400`, owned by the shared field-label token; page/layout selectors must not make them bold. This includes checkbox captions and selectors in heading action areas. Actual section headings, primary identity values and action labels (including a file-upload action implemented with a label) retain their established hierarchy; they are not input captions. A required marker or validation message may retain its semantic emphasis.
- Do not introduce `13–20px` intermediate type sizes to suggest hierarchy; use weight, position, color, and spacing. Identifiers and code-like values still use Segoe UI, optionally with tabular numerals. Headings need line height that preserves descenders (`g/j/p/q/y`), including when wrapped or compacted.
- Long organization, workspace, and record names must wrap without arbitrary one-line ellipsis or a width cap that hides identity. An end-aligned name in a two-column brand layout may wrap at words, but must not cover the logo or actions; expose its complete accessible name.

## Dimension and layout tokens

Values are CSS pixels on the web and device-independent pixels at 100% scale on desktop.

| Token | Value |
|---|---|
| Top bar | `68px`; narrow adaptation `60px` |
| Primary icon rail | `76px`; narrow adaptation `58px` |
| Secondary text rail | `232px` where needed |
| Non-business centered content maximum | `1180px` |
| Business content baseline | `1180px`; above this available width, use one-third of the former centered side gap on each side |
| Responsive horizontal gutter | `24–72px` for non-business pages; at least `24px` for business pages, `20px` in narrow windows |
| Standard input/select/page button | `48px` high, `12px` radius |
| Dense row-action trigger | `36px` high |
| Labeled checkbox / dropdown option checkbox | `24 × 24px`, `6px` radius; each option owns its row |
| Full-page progress track | `40px` high, `20px` radius |
| Compact header progress ring | `52 × 52px` within a `58 × 58px` target |
| Small / panel / emphasized radius | `10 / 16 / 28px` |
| Primary / top-tool / information icon glyph | `20.3 / 23 / 19px` |
| Menu action spacing | `8px` |
| Vertical section rhythm | `44–84px` |

- Keep titles, notices, sections, forms, and grid outer edges on one centered content track. Public, identity, pricing, and other non-business pages retain the `1180px` maximum and their larger side whitespace. Internal business Workspace pages use a fluid centered track: once the available content area exceeds `1180px`, divide that excess into six parts and leave one part on each side, subject to the responsive minimum gutter. This reduces the former centered side gaps by two-thirds while preserving safe narrow-window spacing. Use whitespace and dividers, not repeated boxes, to express section boundaries.
- When a long business page needs an in-page contents list, keep a slim vertical dot navigation centered in the right content gutter without narrowing the content track. Each dot is a real section link with a visible focus state; hovering or focusing it reveals that section's actual heading in a small readable popup. The labels stay available to assistive technology when the popup is closed.
- A filled identity card is one solid chrome-colored surface with contrasting chrome-ink text, no outline or shadow, the emphasized `28px` radius, and responsive `24–38px` internal padding. Group labeled facts and their actual values inside it. Enlarge the single most important identity value, usually the person's name, to `clamp(28px, 3.4vw, 40px)` with `740` weight, `-.035em` letter spacing and `1.14` line height; keep its label and supporting values at `17px`. Let long names and identifiers wrap, and stack columns when space is limited. Do not add decorative explanatory text.
- An account overview without an identity card may show the actual username as its sole primary identity value beneath the page title, using the same `clamp(28px, 3.4vw, 40px)`, `740` weight, `-.035em` tracking and `1.14` line height. Do not enlarge a missing-name placeholder or add a decorative profile card solely to house that name.
- Standard controls in one row share height and vertical center. Do not repair a faulty parent with local transforms or padding. Never mix a `36px` dense action with a `48px` input in the same control row.
- Form input height rules must exclude checkboxes and radio buttons; a dropdown checkbox remains a rounded square at its own `24 × 24px` size. Selected state remains legible without depending on color alone.
- A full-page task progress indicator occupies the centered content track on its own row. Its `40px` thick bar has semicircular ends, a smooth advancing fill and subtle moving highlight; the actual percentage appears below it as a display value, followed by one short stage description in ordinary text. Unknown progress remains indeterminate instead of inventing `0%`. A related compact header indicator uses a rotating hollow ring, the actual percentage centered inside, and a hover/focus stage tooltip; it links back to the task. Stop continuous animation for reduced-motion users, and expose progress semantics and numeric values to assistive technology.
- Adapt to available window/container width, not physical monitor size. Use `980px`, `760px`, and `520px` as common breakpoints, and remain usable at `320px`. Stack forms and records without page-level horizontal overflow; only a focusable data grid may scroll sideways.
- A fixed shell keeps top bar/navigation visible while one content area scrolls. Avoid nested vertical page scrolls. If the page title compacts, enter near `96px` of content scroll and expand near `24px`, avoiding a wrapped-title oscillation. Honor reduced-motion settings.

## Color, theme, and icon roles

| Surface role | Default palette |
|---|---|
| Organization/business | Primary `#153A32`, accent `#16745F` |
| Account/identity | Primary `#153A61`, accent `#2A77C7` |
| Operator/control | Canvas `#F4F7FA`, chrome `#17384F`, accent `#246B9F` |
| Light base and popup surface | `#FFFFFF` |
| Complete dark-theme reference surfaces | `#272E34` and `#2E363D` |

- These are portable role palettes, not hardcoded product names or logo choices. Primary colors serve chrome and large inverted surfaces; accent colors serve main actions, selected states, native-control emphasis, and keyboard focus. A product-specific brand can choose among roles in `current/`.
- For arbitrary configured `#RRGGBB` themes, derive readable foreground, focus, pale mixed surface, and a distinct darker filled-button hover. Do not assume a custom primary is dark enough for white text or let focus fall back to an unrelated global color. A dark canvas alone is not dark-mode support: foreground, borders, controls, and popups must change together.
- Popup menus, select lists, autocomplete, and display controls use the base surface token. Functional SVG glyphs inherit `currentColor`; the parent control supplies default, hover, selected, and disabled colors. Center glyphs in a larger interactive target. Keep logos proportional and aligned with the rail axis. Brand files and exact navigation entries belong in `current/`; do not add an asset library without approval.

## Semantic notices

| Severity | Background / foreground | Default announcement |
|---|---|---|
| Error | `#9D322D` / `#FFFFFF` | `alert` |
| Warning | `#A65300` / `#FFFFFF` | `alert` when attention is required |
| Success | `#246C43` / `#FFFFFF` | `status` |
| Information | `#245F9E` / `#FFFFFF` | `status` |
| Subtle | `#EEF0F2` / `#4F5964` | polite `status` |

Complete dark-theme Subtle uses `#394148` / `#D8DDE2`. Classify by meaning, not URL key or HTTP code; partial completion must not masquerade as full success. Dynamic results display their full text immediately. Associate field errors with the field. Color alone cannot convey status. Do not create a notice just to exhibit a semantic color.

## Headings, copy, and interaction

- A page/section heading area contains the real title, true navigation, and actionable controls. Do not place decorative text next to a heading, an explanatory subtitle beneath it, or an ornamental eyebrow/breadcrumb above a static title. A real parent-list link above a dynamic record name is navigation and is allowed.
- Apply a deletion test: if a title, label, value, validation, status, or action already explains the point, remove extra copy entirely; do not hide it or relabel it `INFO`. Keep consequences, one-time secrets, permission state, operational facts, and required confirmations visible where needed. Put destructive consequences in the confirmation surface, not a permanent footer warning.
- Use visible labels; a placeholder is only for a necessary format example. Provide real loading, empty, success, error, disabled, and pending states. Empty-state copy should state what is absent and offer a genuine next action when possible. No fabricated zero, metric, image, or attribute may stand in for an unknown fact.
- An icon rail expresses top-level modules; a secondary text rail holds static subpages. Selected secondary navigation uses one rounded background and weight change, not an extra stripe. A surface without an organization context should not inherit a workspace rail.
- Place a persistent sign-out action on an authenticated page or shell at the upper-right edge of its shared title/action track. Use an icon-only control instead of embedding the account name in the button; its accessible name and hover/focus tip must say `Sair conta`. Keep its hit target aligned in height with neighboring page actions, show a visible keyboard focus, and preserve the existing sign-out scope and confirmation behavior. A dedicated sign-out confirmation form may retain a labeled submit action.
- Icon-only actions need accessible names and visible focus. A primary-nav tooltip appears after about `150ms` of hover and immediately on focus, closes on Escape, remains readable when hovered, is associated with `aria-describedby`, and shows only one at a time. A disabled entry may remain focusable for explanation but must not navigate.
- Controls need hover, pressed, focus, disabled, and keyboard states. Keep native select fallback usable where full customization is unsupported. Avoid hover-only discovery. Give modals and bottom sheets visible titles/close actions, keyboard focus containment/return, Escape behavior, internal scrolling, and narrow-window access; never discard a dirty form silently.
- Prefer a list/search page plus a separate creation page when management and nontrivial creation coexist. A very short same-page form is a documented exception. A failed write retains entered values. If a write succeeded but the subsequent list refresh failed, say so and offer a refresh retry, not a duplicate submission.

## Lists and data presentation

- A registry list uses one flat, unshadowed, fully rounded fine-border grid with internal header/row dividers. Its outer edge aligns with the content track; first/last cell padding supplies breathing room. Do not wrap the grid in another padded/bordered card.
- Where both List and Grid modes exist, use the same real data fields and visibility settings, with a clear selected state. Actions are not data properties. Put record operations in one `36px` end-of-record `•••` menu, not multiple horizontal row buttons; keep its popup keyboard-operable and free of scroll clipping. In a horizontally scrolling list, pin the action column to the visible right edge on its own opaque layer so data columns move beneath it while the `•••` trigger remains reachable.
- Sort localized text using the product locale, numbers/dates as typed values, nulls consistently last, and equal keys stably. Search/filter/refresh must reapply sort. Let long names and translations wrap; keep headers and cells aligned. A wide grid scrolls inside itself, not at the page level.
- Keep a horizontally scrolling list's scrollbar thin and visually inset from the rounded container corners. Hide native end-arrow buttons where the browser exposes them; retain pointer, touch, and keyboard horizontal scrolling rather than clipping data or moving overflow to the page.

## Verification

Inspect wide, `980px`, `760px`, `520px`, and `320px` layouts; wrapped names; zoom and descenders; keyboard/focus order; tooltip/menu/modal dismissal; arbitrary configured themes; semantic notices; real loading/error/empty states; and reduced motion. Review visual changes separately from authentication, routing, authorization, and data ownership. Record product-specific page maps and approved exceptions in `docs-ai/current/`; ask before changing this cross-project contract.
