# Information grids and header consistency

Recorded 2026-10-27, America/Sao_Paulo (UTC-03:00). Current local candidate: **1.4.4-preview.shell.20261027.5**. This follows the earlier header/footer refinements and implements the consumer's revised read-only presentation requirement.

## Library changes

Structured dropdown rows use 10px vertical/16px horizontal padding, a 72px minimum height and 16px icon/copy gap. Language choices retain their previous compact body typography. ShellHeader's existing Quiet brand Button receives noninteractive outer wing padding of 8px/10px (8px horizontal on narrow screens), with border-box measurement and matching fallback wing minima. Its feedback remains the standard Button paint. Native select ownership now shares the header Quiet preview/click roles; menu and dialog surfaces remain excluded.

UniformGridItem adds opt-in Information=false. Information cells contain a single semantic dl/dt/dd pair: body label and H1-sized value, with encoded Text or a higher-priority ChildContent value. CopyText inherits the value scale. The information-only layout removes icon tracks, fixed aspect ratios and height clipping, and permits natural row growth without changing ordinary grid cells.

Card remains compatible but is explicitly Deprecated in ComponentUsageCatalog with UniformGrid as its preferred replacement. The new Card.razor.cs partial declaration supplies XML guidance and EditorBrowsable(Never). Gallery exposes localized deprecation guidance and information-grid examples. No public renderer/parameter was removed and no Obsolete-warning cascade is introduced.

Dialog adds DangerTitle=false. Enabling it changes only the existing title foreground to the semantic Danger role, preserving aria-labelledby, presentation and lifecycle. The consumer chooses ordinary Quiet for cancellation instead of changing global Secondary paint.

## Ownership and compatibility

Framework owns semantic markup, layout and lifecycle; Design owns spacing/type/color. The host supplies labels, values, navigation and the deletion copy. Information values use the actual H1 type token rather than injecting extra document h1 headings. Existing form/validation controls remain for editable confirmation. The sole new maintained source file is Card.razor.cs; there is no new dependency, service or public component type. Card's new enum classification is appended without renumbering earlier ComponentUseKind values.

## Verification

The 12-project producer and 17-project consumer Release builds complete with zero warnings/errors. Six immutable local .5 archives were packed and five consumed; package identities, cache origins, archive bytes and representative resources are recorded in the current dependency inventories. The only new maintained file is Card.razor.cs, taking the library tree to 705 files/121 directories.

Framework checks pass **484/484**, Gallery checks **8645/8645**, header geometry mocks **6/6**, and native-select/CSS cascade checks **31/31**. Eleven library and two consumer JS/MJS files pass syntax checks; modified JSON parses. Ten focused consumer classes pass **229/229**, with zero skipped cases. These are not computer-use tests or a claim that every solution test ran.

The first Framework run passed 483/484 because a pre-existing brand assertion accepted only quoted empty alt, while HtmlRenderer emitted the semantically equivalent minimized attribute. The corrected check still requires decorative semantics and safe encoded text; no production change was needed. The first consumer run passed 226/229; its stale Account branding expectation and duplicated host menu classes were corrected, and an overbroad assertion against all compatible historical Card usage was removed. New information composition remains free of Card while legacy compatibility is intentional.

The restarted consumer selects .5 in its Debug manifest, returns eight ready responses and serves six byte-matching HTTP resources. Four public portal responses contain the new management menu and Admin Center destination. Package/cache proof is in `artifacts/shell-20261027/dependency-provenance-5.json`; running evidence is under the consumer's ignored `artifacts/information-grid-20261027/`. Browser geometry/feedback and protected-page acceptance remain manual. No public package, remote tag or cloud deployment was performed.

## Manual acceptance

Check shared Quiet feedback and brand outer gaps in light/dark themes, structured menu spacing and unchanged language labels. Check information-only body/H1 hierarchy, one field per cell, copy text and natural growth under long values, narrow widths and zoom. Check Danger dialog title, accessible name, default non-Danger dialogs, Quiet cancellation and preserved busy/close behavior. Gallery should visibly discourage new Card usage while old rendering remains available. These are user-run visual checks; no Computer Use is used.

## 2026-10-27 compact-brand follow-up

This report retains .5 evidence. The .6 correction reduces the oversized brand control and restores the single-row header baseline; see [compact branding and header height](2026-10-27_compact-brand-button-and-header-height.md). Other .5 capabilities remain unchanged.
