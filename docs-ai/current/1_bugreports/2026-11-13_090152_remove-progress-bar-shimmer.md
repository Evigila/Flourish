# Remove the standard progress bar shimmer

Recorded: 2026-11-13 09:01:52 -03:00 (America/Sao_Paulo)

Status: implemented in local preview **1.4.8-preview.14**.

## Symptom and cause

A white flashing highlight crossed the standard progress fill. Design applied a common f-progress-fill::after pseudo-element with a 47%-opacity light-surface gradient and the repeating f-progress-flow animation. That common layer affected both known and unknown progress; indeterminate bars additionally ran their independent moving-segment animation.

## Change

Remove the shared highlight layer and shimmer keyframes rather than overriding only unknown progress. The primary-color fill and determinate width transition remain. Unknown progress retains the original bounded 35% segment moving over the remaining 65% of the track at 1.8 seconds. Reduced-motion mode still becomes a static full-width visible indicator. The PageHeading thin indicator has its own f-heading-loading-travel animation and required no change. Component markup and accessibility semantics are unchanged.

## Verification and limits

The targeted suites passed **47/47**: four progress checks, three independent heading-loading checks and forty existing selected-dropdown checks. Producer Release built with zero warnings/errors. Six packages passed identity/dependency/required-asset verification; packaged CSS matches compiled bundle bytes. The Design package contains neither the former pseudo-element nor f-progress-flow, while search motion and preview.13 dropdown paint remain. The 583-input provenance has one runtime delta from preview.13, controls.css; the 32 external package/consumer entries are identical.

No browser automation, production deployment or public NuGet release was performed.

## Manual acceptance

1. Observe unknown progress: the single-color segment moves without a white flash.
2. Update a known percentage: the solid fill changes width and its numeric/status text remains correct.
3. Enable reduced motion and check the independent heading-loading indicator; both retain their existing accessible loading behavior.