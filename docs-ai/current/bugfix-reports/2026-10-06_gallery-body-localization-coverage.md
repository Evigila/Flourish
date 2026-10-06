# Gallery H2 and body localization coverage audit

## Status and symptoms

Open caller-adoption work, confirmed by source inspection on 2026-10-06. The user reported untranslated internal H2 headings and body text after the primary PageHeading repair. This audit explains the remaining scope; it does not claim to implement full Gallery prose localization.

## Confirmed cause and evidence

- Framework.razor:9–12 passes the literal Section title "接入应用" and renders literal Chinese paragraphs. Other topic headings follow the same pattern.
- Examples.razor:9–10 and Appearance.razor:10/21–22 likewise retain literal section titles and explanatory text.
- Section.razor:3 directly renders Title as an H2; its ChildContent is an opaque caller-supplied RenderFragment. It does not infer a catalog/key or translate arbitrary strings.
- Overview.razor:6/14–15 and Localization.razor:7–9 are existing positive examples: both heading and paragraphs resolve generated keys during render.
- ComponentGuide.razor retains literal section labels and renders Entry.Purpose, Variants, Usage.Scenario/Guidance and Sample.Description directly. Its explicit Chinese TableText and static API column captions override library-provided defaults.
- ComponentCatalog, ParameterMeaning, ComponentUsageCatalog and SampleFor/SampleCatalog contain natural-language descriptions as literal metadata. Some descriptions/parameter rows are captured in static collections; adding a language event alone would still display the same Chinese values.
- Individual sample controls also retain literal labels, options and status messages.

The primary-heading task deliberately repaired PageHeading/PageTitle, not all internal prose. Catalog integrity ensures that every existing key contains three languages; it does not detect user-facing literals which never request a key. LocalizedComponentBase rerenders on culture changes but does not automatically translate literals. The observed sources explain the symptom without requiring a new CSS, font, scope or Culture engine diagnosis.

## Recommended implementation boundary

Maintain H2 labels, natural-language paragraphs, actions, example statuses and human-readable API explanations through en-US/zh-CN/pt-BR resource keys. Store stable identifiers/keys in reusable metadata and resolve text in the current request/circuit while rendering. Do not resolve a user's language during static application initialization. Reuse existing keys where semantics match.

Keep component/parameter/enum/namespace/icon identifiers, code syntax, product brands and actual record data as identities/data. Preserve paragraph structure, line breaks, links and placeholders in the resource contract. Correct stale documentation against current source before translating it, rather than carrying obsolete claims into three languages.

No new generic control, automatic string translator or styling adjustment is necessary merely to translate Section.Title/ChildContent. Metadata ownership must remain explicit; a Gallery description and Framework-owned standard caption are not the same catalog responsibility.

## Verification and limitations

This request was analyzed through source inspection and a bounded read-only agent. No product source, build settings, dependencies, styles, fonts or generated build state were modified; no build or Computer Use test was run. Analysis does not claim live browser acceptance or completed H2/body translation.

After implementation, test actual H2/paragraph output in all three languages and changes within the same rendered page; cover descriptions from metadata as well as direct Razor text. Retain the compressed-resource checks added after the static-asset incident and isolate future verification with --artifacts-path. Manual review should include paragraph wrapping, links, long Portuguese headings and unchanged API/data identities.
