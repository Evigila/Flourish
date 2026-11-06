# Structured-menu title and description trimming

2026-11-05 · America/Sao_Paulo · UTC−03:00.

## Symptom and evidence

The Colligere user reported that two-line Portal text appeared above its left icon. Button structured labels and menu rows already use centered flex alignment and symmetric block padding. The host uses native Icon/Text/Description parameters; the served Design 1.4.5 bundle matched its restored package.

Design controls.css applies `text-box-trim:trim-both;text-box-edge:cap alphabetic` to `.f-button-text` in supporting browsers. The separate description span retains its natural line box. Setting both lines to 17px/1.55 in dropdown.css did not neutralize this difference. Unequal trimming is confirmed in source and can explain the reported visible-center discrepancy; no browser geometry was measured. Initial font-only estimates omitted this feature branch and are not a basis for fixed pixel offsets.

## Correction and scope

The existing `.f-dropdown-panel .f-button-structured.f-dropdown-item .f-button-text` rule now sets `text-box-trim:none`. Title and description retain natural line boxes while ordinary buttons keep their original trim behavior. No public parameter, icon font, menu padding, framework component or host override changed. Unsupported browsers ignore the added property.

The user explicitly authorized the producer fix and a local preview consumer, with no public NuGet update. Source VersionPrefix stays 1.4.5; build/pack overrides create immutable 1.4.6-preview.1 archives. The six packages passed existing metadata/dependency/asset validation and are retained in the Colligere ignored local feed. No tag, push or publication occurred.

## Verification and limitations

Release Blazor solution build: zero warnings/errors. Existing native-select-trigger cascade suite: 33 passed, zero failed/skipped, including menu title/description natural boxes and ordinary-title trim preservation. Pack emitted skip warnings for two non-packable solution projects and missing-README notices; all six intended archives were created and validated. Colligere restored the five consumed identities, verified cache hashes and bundle contents, built its full Release solution without warnings/errors, and passed 275 selected Web checks. Other 163 external package/version pairs were unchanged.

No Computer Use or browser visual test was performed. Manual acceptance after normal consumer restart must compare all three icons and Chinese/English/Portuguese title/description, wrapping, 100/125/150% zoom, narrow windows, menu keyboard/touch behavior and ordinary buttons. The consumer's existing public release gate still rejects development-only previews.
