# Production banners and automatic project footer

The homepage follow-up standardizes existing PresentationBand, PresentationHero and PresentationFooter rather than introducing another component family. No package, font, dependency, desktop control or business behavior was added.

## Component changes

- PresentationBand defaults to MinHeight=450 and Dotted=false. Zero permits natural height; negative values are rejected. It owns the full-width page-region background and inner centered ContentContainer. Long or narrow content may grow beyond the minimum without clipping.
- PresentationHero delegates those contracts and Tone to Band. Its artistic title/subtitle retain their size roles; a 40px grid gap and additional action spacing replace margins that the foundation reset could erase. Direct-child selectors do not restyle nested consumer controls.
- Canvas, Surface and Primary backgrounds use their existing colors directly. Background-color preserves the shared 18px dotted grid; Primary dots use primary-preview. Different adjacent tones are recommended, not automatically assigned from sibling DOM.
- Header inline actions no longer inherit the ordinary content group's 16px top margin. Buttons retain their actual variants, including Elevated.
- Footer title and decorative watermark default to configured ProjectName, with scoped TextReference refresh and disposal. Explicit BrandName/Watermark overrides remain compatible. Copyright is separately supplied by the host; the library invents neither an owner nor a year.
- Usage catalog, reflected parameter meanings and Band/Footer Gallery examples describe the production defaults and compatibility boundaries. DisplayBoard retains its separate preview/copy behavior and default dots.

## Verification

Complete release preparation passed with zero build warnings/errors: Core 367, Blazor 234, Culture bridge 12, all ten JavaScript files, 21 CSS bundle checks and 194 SDK asset checks. Six candidates and four fresh NuGet-only consumers passed 95 checks. Evidence is artifacts/package-consumers/5871b0bae0ab481b916ceb6832a8bb25. The catalog is 97 components, 712 rows and 315 documented defaults.

Seven strict Gallery HTTP routes passed, including product/pricing/access/accounts and /controls/layout/presentationbandsample, presentationherosample and presentationfootersample. Dot opt-in and configured Gallery identity were checked separately. An initially guessed /guide route was not a valid Gallery route; final checks use the actual catalog paths with terminating error handling. The owned Gallery process was stopped.

The first preparation rejected a derived dot color. Final CSS consumes existing roles without weakening palette checks. Source review also caught background-image reset and nested-content selector leakage; the final regression covers both. See [the defect report](../bugfix-reports/2026-10-05_presentation-spacing-and-dot-cascade.md).

This follow-up remains uncommitted and unpublished, separate from the earlier library audit and Colligere baseline commit d4c5d9a. No Computer Use, push, tag, NuGet upload or deployment occurred. Existing advanced table/API convergence debt remains unchanged.

## Manual acceptance

Check header Elevated action centers and keyboard focus in Light/Dark/System modes. Inspect Hero title/subtitle/action separation at full width, 760px, 320px and 200% zoom. Check all three tones with dots off/on, adjacent alternating tones and naturally growing long text. Change configured literal/localized ProjectName and confirm footer title/watermark follow it without replacing host copyright. Full-width banners must be placed in a full-width page region, not used to escape a constrained business shell.
