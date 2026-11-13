# Card summary consolidation

## Request, overlap and evidence

The user confirmed that Card already covers IdentityCard's summary tasks and requested removing IdentityCard, with the reason recorded in the next preview ChangeLog. The two exported entries separately registered and styled ordinary summary content despite Card already providing Title, semantic ChildContent and Actions. Maintaining both entries duplicated the public card contract, Gallery navigation and usage guidance.

The correction is a user-authorized breaking consolidation of project-owned Blazor and native WPF APIs. It adds no compatibility alias, forwarding component, new Card parameter or independent identity renderer. Earlier release records remain evidence of the former API rather than current usage instructions.

## Correction and ownership

Blazor IdentityCard and its independent styles are removed. Its Gallery sample, navigation link, ComponentCatalog and ComponentUsageCatalog entries, special parameter mappings and dedicated translation keys are removed. Card remains the single ordinary content, identity and record summary entry. OfferCard retains its distinct staged-offer responsibilities and now directs business entity summaries to Card alone.

RecordDetails directly uses Card with the original semantic definition list and four business fields; the former identity-specific class is deleted. CardSample demonstrates the existing Title and CopyText composition with a real localized member title and DEMO-001 identifier. Card's existing H3 Title semantics are unchanged. IdentityCard's specialized HeadingLevel, Columns and SideContent entry points are retired rather than reproduced as Card extensions. Business values, edit/delete callbacks and confirmation behavior remain host-owned.

Native WPF likewise removes IdentityCard's type, style and catalogue entry. The existing Card example retains the name, department, status, identifier-copy and edit demonstrations through the current Card contract. Its catalogue now has 74 native entries, all with actual offscreen Gallery template checks.

Current architecture, API, convergence and native integration guides explicitly supersede the former IdentityCard recommendation. The three-language 1.1.4-preview entry states: "Removed IdentityCard: Card covers its summary capabilities; one card entry avoids duplication." The published 1.1.2 IdentityCard repair note and append-only release/bug history remain unchanged.

## Verification and limits

Release builds for Blazor, Gallery and WPF completed with zero warnings and errors. Blazor passed 429/429 checks, including a catalogue of 80 components, 720 parameter rows and 329 defaults. Gallery passed 7,772 checks, including 171 actual-event examples and 199 ChangeLog assertions. Complete-import style and PageBody-focused Node regressions passed 20/20 checks.

WPF CatalogTests passed 76/76 checks, including all 74 native Gallery entries rendered through actual offscreen templates and the Card identity Edit event. Culture integrity passed 22,038 checks across 1,544 Gallery and 286 Framework three-language keys. ChangeLog validation passed 70 checks, CSS bundle verification passed 21 checks, and git diff --check was clean.

No dependency, package version, Git tag or publication changed. No Computer Use or browser UI testing was performed. Automated markup, event, style and offscreen-template checks do not establish visible-browser/window layout or assistive-technology acceptance; these remain manual checks.

## Manual regression checklist

1. Open Gallery's Card guide and verify its identity example displays the localized member title and CopyText identifier. Confirm navigation and API lists contain Card and omit IdentityCard.
2. Open RecordDetails. Verify the summary displays all four original facts, its Edit button opens the existing form and its parent navigation returns to the record list.
3. Check Card and RecordDetails at narrow widths and in light/dark themes. Titles, long identifiers and definition-list values should remain readable without horizontal overflow.
4. Switch English, Chinese and Portuguese. Confirm Card guidance and identity example labels update without unresolved resource keys.
5. Open ChangeLog and select 1.1.4-preview. Verify the removal reason explicitly says Card covers the summary capability and one entry avoids duplication. Select 1.1.2 and verify its historical IdentityCard repair note remains.
6. Open the native WPF Card example. Check name, department, status and identifier copy; activate Edit and verify its existing status/event feedback. Confirm native navigation and API inventory omit IdentityCard.
