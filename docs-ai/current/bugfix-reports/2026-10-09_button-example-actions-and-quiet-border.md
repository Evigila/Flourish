# Button example action coupling and Quiet pointer borders

## Symptoms and cause

Gallery's Button preview exposed enum names Primary/Secondary instead of the requested Filled/Outlined presentation names. Both Example account and Save draft invoked SaveAsync and read Saving, so either click activated both busy visuals and the same saved counter. Quiet inherited visible pointer borders from shared Secondary/Quiet hover and active rules; access and chrome rules independently repeated that border paint.

## Mitigation

The sample explicitly maps the six display labels without changing ButtonVariant identities. Example account now selects the fictitious account and shows a localized selected caption; it does not invoke or observe draft saving. Save draft keeps its own busy state and count; resetting drafts preserves account selection. Existing 24rem responsive bounds remain.

Design controls.css separates Secondary from Quiet pointer rules. Quiet retains a transparent border for stable geometry and preserves its background/foreground feedback. The access and chrome overrides no longer recolor that border. Keyboard focus-visible outlines, Outlined borders and existing unavailable-state paint remain.

## Evidence and regression checks

- Release Gallery test graph built with warnings as errors: zero warnings/errors. The generated design bundle contains the corrected Quiet rules.
- 7,713 real Gallery checks passed, including both text/icon Filled/Outlined labels, localized icon-only labels, independent account/draft events, a single busy action during saving, account selection during draft saving and retained/reset/language state.
- 41 focused Node tests passed across organization-access-presentation, palette-checks and split-menu-checks. Full import cascade checks cover ordinary/access/chrome surfaces, button/link elements, hover/active/focus states and retained Outlined borders.
- 22,160 catalog integrity checks and 58 ChangeLog checks passed. Preview notes include these changes; package version remains 1.1.3.

## Limitations and manual checks

No Computer Use or browser UI testing was performed. Verify the three preview rows, hover and press Quiet using mouse, then Tab to verify the focus outline. Check Outlined still has its border. Select Example account and verify no saved-count change; save a draft and verify only Save draft becomes busy. While saving, the account remains usable. Switch languages and reset the draft counter; retain account selection.
