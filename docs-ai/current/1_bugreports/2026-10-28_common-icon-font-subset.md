# Common variable icon font without removing dynamic icons

Recorded 2026-10-28, America/Sao_Paulo (UTC-03:00). Local candidate: **1.4.4-preview.shell.20261027.7**. No remote publication or tag is involved.

## Symptom and cause

The previous single Material Symbols face referenced a **4,003,088-byte** WOFF2, with `font-display:block`, even when a page needed only a handful of icons. An anonymous consuming portal contained six Icon occurrences using five distinct names. The library's Icon renderer already converts names to a single Unicode character through the complete bundled codepoint map, so the bottleneck was the all-or-nothing font asset rather than an ASCII ligature lookup.

## Implementation and compatibility

The same family now has two **disjoint `unicode-range` faces**. The common face is a **125,404-byte WOFF 1.0**, covering 78 names/76 codepoints. This reduces the font required by common-only pages by **96.87%**. Every other Unicode character remains eligible for the original full WOFF2 at its unchanged URL and bytes. The ranges cover all Unicode without overlap; a common glyph cannot select both faces. An uncommon dynamically selected icon can legitimately cause the complete file to load once, alongside the common file. Unknown names still resolve to the common `help_outline` glyph.

No component API, complete name map, Filled parameter, font style or supported variation axis was removed. Existing raw ASCII ligature consumers retain the full font path. Normal builds and hosts consume checked-in assets; no font converter or Python runtime is required in the application.

`scripts/Subset-MaterialIcons.py` regenerates the asset from the maintained original WOFF2 and `common-icons.json`. It uses installed Python 3.12 standard-library facilities, existing Windows DirectWrite, and Windows PowerShell/WPF. FontTools, Brotli and Node font-converter packages were unavailable; none was installed. Windows computes the outline/cmap subset without renumbering glyphs. The generator then removes unused glyph variation blocks, retaining each included glyph's original bytes and the original `fvar`, `avar`, `STAT` and `HVAR` tables. It keeps upstream font names/license strings and wraps the result using standard zlib WOFF compression. This follows the [OpenType glyph-variation layout](https://learn.microsoft.com/en-us/typography/opentype/spec/gvar) and [WOFF 1.0 container specification](https://www.w3.org/TR/WOFF/).

The Apache-2.0 license and source revision remain in the package. The complete original font, WPF resources and original codepoint map are unchanged. The existing native converter now accepts WOFF as well as WOFF2 so the generated container can be independently decoded and checked. Temporary generation files stay under the resolved workspace because WPF rejected the short 8.3 user-directory URI returned by this machine's TEMP configuration.

The existing CSS bundler inlines `subsets.css` and rebases both font URLs into `framework.css`. It does not create another runtime stylesheet request. Package expectations therefore require the common font and manifest, rather than the source-only CSS partial. No scrolling/observer code was changed: the earlier P3 layout concern needs a browser trace before a separate fix.

## Verification and delivery

- Independent regeneration produced the same SHA-256: `098865209c2ad1173ba59feb02fbb232cb69cb56155cdd263999d84f0a6c65c1`.
- Windows loaded both generated SFNTs; all 78 default and Filled outline comparisons passed. Its WOFF decoder round-tripped every table unchanged. Node independently checked original glyph IDs/outlines, variation blocks, checksums, manifest hashes, full Unicode partition and the untouched original font: **3/3** passed.
- Twelve-project producer Release build: **zero warnings/errors**. Existing Framework checks **485/485**, CSS bundle **21/21** and ChangeLog **177 checks** passed. No Computer Use or browser automation ran.
- Six local archives passed the existing package-set, dependency and required-asset verifier. Font files, manifest, license and metadata match the source bytes; generated CSS contains exactly the two ranged faces with no remaining imports. The first package-verifier run incorrectly expected standalone `subsets.css`; correcting that expectation did not change or replace any archive.
- The consuming project reports a final 17-project Release build with zero warnings/errors, relevant Web tests **264/264** with zero skips, and five exact cache archives matching the new feed. Running-browser font selection and timings remain separate acceptance.

The repository has no debug-handler entry. The existing `publish-helper.bat -Mode Prepare -SkipBuild` completed its ChangeLog checks and then rejected preview versions under the stable-release guard. Following its instruction, this candidate was explicitly restored/built/packed into **`artifacts/icon-subset-20261028/nuget`**, then checked using `scripts/Verify-PackageSet.ps1`. Earlier shell feeds and archives were not overwritten. No Git commit, remote package publication, SDK installation or deployment was performed by this library task.

Evidence is under `artifacts/icon-subset-20261028/`: generation/regeneration JSON, font/Framework/CSS logs, the build and pack logs, package hashes, `font-delivery.json` and the existing-entrypoint log. These generated files are excluded from the maintained tree. See [current dependency identities](../1_dependency.md#current-candidate-archive-and-asset-evidence).

## Maintenance and manual acceptance

Regenerate with an installed Python 3.10+ interpreter on Windows: `python scripts/Subset-MaterialIcons.py`; use `--verify` to independently regenerate and compare without editing maintained assets. The script requires existing PowerShell 7, Windows PowerShell/WPF and DirectWrite. It performs no package installation or network acquisition.

1. With browser cache disabled, open a portal using only common icons and inspect font requests: the common WOFF should load and the complete WOFF2 should not. Open the language and service menus and check ordinary/Filled icons.
2. Display a valid uncommon name, for example `10k`, using an existing Gallery icon sample. Confirm it renders and loads the original fallback; additional uncommon icons should reuse that resource.
3. Try an unknown name and confirm the normal help glyph renders. Check light/dark, zoom and supported browsers, including Filled selection for common icons.
4. Repeat with a warm cache. Record actual transferred bytes, font request initiators and icon visibility; the stored byte counts and range checks above are not measured LCP, CLS, INP or first-paint timings.
