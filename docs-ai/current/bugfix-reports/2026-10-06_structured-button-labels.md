# Structured labels in the ordinary Button

## Symptoms and cause

Colligere's account selector needs one action containing a left-aligned name/email stack and a right-aligned connected status. Public 1.1.1 Button supports Text/ChildContent but has no library-owned layout contract for this scene. A host skin or account-specific cloned control would violate UI ownership. The former separate Card and continue command were consumer composition defects; the structured label is a genuine general Button capability gap.

## Source repair

Add Description and TrailingText to ordinary Button, defaulting to empty. A nonempty value activates bounded full-width geometry owned by Framework: required Text above optional Description at logical start, optional passive TrailingText at logical end, wrapping for long labels and no nested interactive element. Design owns secondary/status typography. Structured mode rejects ChildContent and an empty primary label, keeping one explicit composition contract. Regular and icon-only buttons are unchanged.

Native button Type, navigation Href, callback handling, Disabled/Busy refusal, spinner/status and unavailable-link transport remain intact. The capability applies to ordinary actions and links, not exclusively account selection. Catalog, parameter meanings, defaults, three-language descriptions and an executable Button Gallery sample are updated together. No alternate renderer or compatibility API is introduced.

## Verification and limitations

Release Tests.Flourish.Blazor and Gallery.Flourish.Blazor builds passed with zero warnings/errors. Console checks passed 387/387. Five new checks cover ordinary/icon-only invariants, encoded passive spans inside a single button, available/disabled/busy button/link event transport, invalid mixed composition, logical bounded layout and real Gallery usage.

Physical browser acceptance remains user-operated. Verify light/dark variants, long name/email/status wrapping, narrow widths, keyboard/native submit, busy/disabled refusal and normal/icon-only button invariants. Publication, version assignment and consumer upgrade are deferred. Colligere's public 1.1.1 still uses a combined plain caption; exact structured layout requires a newly approved public package and explicit use of the new parameters. Earlier uncommitted source repairs remain untouched; no commit occurred.
