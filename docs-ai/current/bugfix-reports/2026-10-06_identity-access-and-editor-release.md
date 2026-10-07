# Identity access and editor layout for Blazor 1.1.2

This release collects the previously deferred post-1.1.1 source repairs and the approved identity, access, short-form and spreadsheet improvements. Consumers use the same production controls; no host skins, alternate renderers or compatibility contracts are introduced. Essential remains 1.3.0 and the existing six-package Core/Blazor publication scope is unchanged.

## Causes and corrections

IdentityCard previously combined width 100% with PageBody's external content gutters, exceeding available width. Its own heading now supports HeadingLevel and remains start-aligned, with bounded wrapping and bold CopyText. Using Title with HeadingLevel 1 avoids placing a sticky PageHeading and its canvas surface inside an identity summary. Default HeadingLevel remains 2.

PageBody's ordinary document bottom spacing and Section's page-level inset were inappropriate when repeatedly nested in short checkout/redemption scenes. CompactSpacing is an explicit opt-in for short forms and identity summaries; it balances content spacing without changing ordinary page defaults. DisplayBoard owns its padding and clears the direct Section's extra page inset. InlineActions.Alignment controls end placement inside actual bounded layouts.

PageBody.FillHeight supplies the fixed-stage content contract. Compose a direct PageHeading with Compact true and a direct EditingGrid: the heading is compact immediately and the spreadsheet uses the remaining height rather than its normal 65vh cap. The existing grid editing, keyboard save, scrolling, cursor loading, selection and clipboard core remains unchanged. UniformGrid.CellHeight optionally fixes Rectangle cells at 100px or another positive finite height, independently of responsive width; Square rejects that option and default null preserves existing sizing.

Button.Description and TrailingText supply one structured native action: primary Text and secondary text stack at logical start, and a passive status aligns at logical end. This resolves the account-selector label gap without an account-specific renderer. Empty defaults retain ordinary/icon-only buttons; invalid mixed ChildContent is rejected.

PresentationFooter.ProjectName is a per-instance identity override shared by the title and watermark, without mutating global project configuration; omitted copyright produces no attribution. NavigationChoices.Variant and ActiveVariant customize its existing Compact Buttons while preserving native GET, aria-current and unavailable choices. AccessFormSurface's native form scanner uses the shared controls module to measure visible labels and field gaps, reserving the same spacing before actions. Wrapping labels, hidden panels, document replacement and disposal are covered; SSR has a single-label-line estimate until the native module initializes.

Previously deferred automatic compact headings, reference-field width, DisplayBoard form width, passive welcome watermark, bulk-action end placement and readable framed EditingGrid geometry are included in this release. Their historical reports remain unchanged; their former no-publication boundary is superseded by the user's explicit release authorization for this task.

## Verification and release boundary

Final complete preparation passed on 2026-10-06: Core 367, Blazor 396/396, bridge 12, Gallery 7,266 with 124 post-event checks, Node runner 99/99, CSS 21/194, launcher 95, catalogue 21,426 and isolated candidate consumers 129. Builds had zero warnings/errors. The successful consumer fixture is artifacts/package-consumers/c6a3001a3d99416b979529e2fcbd886e. The initial failed preparation and final successful log remain separate evidence.

Release preparation uses the existing script and fresh candidate package consumers. New Gallery parameters/defaults, three-language descriptions and actual examples accompany the APIs. The complete gate includes the newly added layout and native access-spacing JavaScript suites. An initial Gallery Razor namespace-alias compilation error was corrected by using actual component names; no release tag was created for that failed preparation. The final preparation log is artifacts/release-1.1.2-preparation-final.log. Publication and public-source consumption will be recorded separately after completion, not inferred from candidate packing.

## Cleanup and ownership

Twenty-six retired access, Gallery, heading and table verification output directories under Flourish/artifacts were deleted after validating their absolute repository-local paths and untracked generated status. They can be regenerated. Current release/package outputs, preparation logs, historical release evidence and the Gallery asset incident snapshot remain. Source tests are durable regressions, not temporary output; human DocFX material and Git history are unchanged.

## Manual acceptance

Check long identity names and IDs at narrow widths in light/dark modes, without card overflow or white title overlay. Check configured and per-instance organization footers, omitted copyright and unchanged global branding. Verify equal visible input/action spacing with short and wrapped labels, all access methods, native submit and page replacement. Check compact short-form spacing and end-aligned actions. Enter the full-height spreadsheet directly, resize it, edit cells, save by keyboard and scroll-load more records without losing drafts. Confirm 100px form UGB cells remain that height on narrow screens, while ordinary and Square grids retain their defaults.
