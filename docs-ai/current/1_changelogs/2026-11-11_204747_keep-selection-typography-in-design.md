# Keep embedded selection typography in optional Design

2026-11-11 20:47:47 · America/Sao_Paulo · UTC−03:00.

The embedded selection marker introduced a visual font default into Framework: `800 17px/1 var(--f-font,"Segoe UI",sans-serif)`. This contradicted the native layer's skin-independent ownership and was caught by Colligere's delivered-package asset check. The marker's selected-state semantics and compact placement were correct; the missing producer regression concerned typography ownership.

Framework `wwwroot/selection-indicator.css` now inherits typography and keeps a unit line height for its centered glyph. The existing font declaration moved to Design `wwwroot/dropdown.css`, matching the native picker checkmark. The 28px trailing slot, unfilled and borderless marker, check visibility, disabled state and forced-color behavior retain their original owners and behavior. Loading Design preserves the prior marker font; no dependency, API, runtime source path or package version changed.

Verification: `node --test tests/Tests.Flourish.Blazor/native-select-trigger.test.mjs` passed **36/36**, zero failures or skips. Its new regression covers both native input and decorative span markers, inherited Framework typography, the absence of the skin's default family in the complete Framework CSS, and matching Design/native-picker fonts. Existing checks still cover selected/disabled states, geometry, unfilled paint, native semantics and standalone checkbox height.

The Release build of `src/Flourish.Blazor/Flourish.Blazor.Design/Flourish.Blazor.Design.csproj` completed with **zero warnings and errors**, rebuilding Framework and generating both CSS bundles through the existing `build/CssBundle.targets` process. Inspection of the generated `obj/Release/net10.0/bundled-css/` outputs confirmed neutral Framework typography and the unchanged Design marker font. `git diff --check` passed.

Manual acceptance: compare MultiSelectBox, ReferenceDropdown and native SelectBox markers with Design loaded; selected marks should keep their previous appearance and trailing alignment. In a Framework-only consumer with a custom inherited font, verify markers remain visible without installing the Design font stack. No Computer Use was performed. Colligere and the immutable preview.11 archives were not changed by this source fix; packaging, consumer adoption, commit and publication are separate follow-up operations.
