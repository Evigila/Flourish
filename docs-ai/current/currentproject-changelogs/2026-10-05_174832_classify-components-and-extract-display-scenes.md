# Component classification and display extraction

The Primitives audit confirms a historical extraction namespace rather than an internal-only component layer. All public controls now have explicit use guidance, and display/access composition extracted from Colligere is implemented in Flourish without another shell, package or host visual exception. The [technical capability guide](../component-api-organization.md) records the complete merge decisions and remaining convergence work.

## Cause and implementation

- Initial inventory: 88 public Razor components, but only 78 Gallery registrations. The generic Components family and extracted Primitives family independently owned rendering, state and contracts; the existing hand-maintained catalog gate did not inspect every export. Source/history evidence identifies commits 1fcc922 and 1b5b3d0; the latter added the host-shaped family without completing a shared-core/facade migration.
- ComponentUsageCatalog explicitly registers all 97 current exports: General 22, Scenario 55, Compatibility 14 and BuildingBlock 6. Primitives contains 22 production scenarios, 14 compatibility entries and 2 helpers. No namespace-wide production ban or false replacement is introduced. Lookup normalizes generic definitions and rejects unreviewed types.
- Inventory tests compare actual exported IComponent types with both source-owned metadata and the full Gallery/sample inventory. Guidance, preferred-entry validity/cycles, parameters and defaults are checked. Gallery preserves full type-family names and separates compatibility/helper browsing while retaining old detail URLs.
- ContentContainer, PresentationBand/Hero/Footer, OfferStage/Card and AccessPanel/FormSurface/Actions add nine production entry points. PresentationTone is an Abstract contract. Framework owns layout/state/behavior; Design owns appearance using existing typography and palette roles. All new controls share Class/AdditionalAttributes and standard slot conventions.
- ContentSurface adds DocumentFlow/Footer to its current renderer. Document mode does not initialize the fixed-stage compact-heading behavior. Full-width background and inner centered content are independent layers; artistic title/footer geometry is now library-owned.
- OfferStage/Card keep all SSR content readable and add scoped native-link enhancement, dynamic column counts, independent controllers, focus/pointer/visibility pause, reduced-motion/mobile static mode and disposal/rebinding. Duplicate stage IDs fail during rendering. A standard Quiet pause/resume button has scoped en-US/zh-CN/pt-BR defaults, explicit override precedence and pressed-state feedback.
- Access surfaces compose existing controls rather than reskinning Button/input descendants. Panels have compact/wide variants; form surfaces do not generate forms. Colligere keeps authentication, native POST/EditForm, antiforgery, token/fragment processing and server validation.
- Gallery adds a Display pages secondary entry and product/pricing/access cases using fictitious data, plus executable samples for all nineteen newly registered controls. ListView is now correctly categorized and navigable. SurfacePatterns retains the advanced table's dedicated capabilities but uses canonical buttons, fields, notice and sheet where compatible.
- Root AGENTS and active technical documentation now require explicit public usage and capabilities before another renderer is added. No retired UI standard or archive was recreated; human docs and existing dirty changes remain protected.

The two DataTable APIs and other capability-sensitive compatibility APIs remain operational. This task implements their analysis, production guidance and inventory governance, not their destructive merger. A single typed data/state core must preserve the union of culture/paging, templates, bulk editing, preferences and guarded native transports before aliases can replace advanced consumers. ListView/EditingGrid and protocol-specific inputs are legitimate distinct scenarios. No extra NuGet is needed.

## Verification

- Blazor/Gallery Release build and complete Colligere Release build: zero warnings/errors.
- Flourish component checks: 166/166. Gallery audit: 97 components, 707 parameter rows and 311 documented defaults. Culture bridge: 12 passed.
- All ten configured JavaScript files passed: 53 Node-runner checks plus the retained 38 controls, 5 interaction-origin, 8 row-menu and 15 section-navigation mock checks. CSS bundle verification: 21 passed.
- Six configured Core/Blazor packages passed identity/dependency/static-asset verification. Four fresh NuGet-only consumers passed 95 SSR/registration/HTTP/asset checks. Final evidence: Flourish artifacts/package-consumers/7c7bc17721d342fda087b0b46793d2ff.
- Gallery passed 27 automatic HTTP SSR requests: four display routes, nineteen added guide routes, ListView and three support-directory routes. The owned ephemeral Gallery server was stopped; no browser/Computer Use was used.
- Colligere focused Web regressions: 220 passed, zero failed/skipped. Evidence: artifacts/test-results/presentation-standardization/presentation-final.trx. This includes Account, Standard and Workspace access/privacy/native-form checks; earlier unrelated full-suite business failures are not claimed fixed.
- Final local candidate SHA256: Abstract C5B74615246AD0C842EB6FEAB5366F2EC47A82895694B55DB089677EB71A152E; Framework ABEEFEB86CBD9A5F088EA2C8F6CC02D7360AF1B731A3BDB8FDA42B94C2C045D4; Design 2F026D4DEC7D1EA7A4338861F231B30687EA949316CB70EA4DFF2816116BF4D7. Colligere's artifacts/package-cache-presentation-final contains matching archives and is the final project.assets.json package root.

No commit, tag, push, public NuGet publication, deployment, data reset or external dependency change occurred. Essential and desktop code/package preparation were not modified. Full database/desktop suites and live authenticated browser acceptance were not rerun. Older candidate caches remain untouched; local repacking is allowed only while 1.1.0 is unpublished.

## Manual acceptance

1. Open /examples/display/product, /pricing and /access in both languages and Light/Dark/System modes. Check full-width bands, common centered edges, artwork and ordinary type/real Button variants at desktop, 760px and 320px.
2. Inspect production and /controls/support directories, both DataTable descriptions and all construction-helper warnings. Compare new guides/examples and confirm ListView belongs to data.
3. For wide offers, verify native anchors, focus/pointer pause, the explicit pause/resume button and independent stages. Reduced motion, narrow viewports and missing JavaScript must leave all offers readable; modifier-clicked links retain native behavior.
4. Check access panels, wide two-column forms, field/error identity and absence of nested main/form landmarks. Existing business shells, dialog/menu lifecycle and advanced tables remain unchanged.
5. Verify Colligere with its final isolated cache and existing test accounts before any publication decision.
