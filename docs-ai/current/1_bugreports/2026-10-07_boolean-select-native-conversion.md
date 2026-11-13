# Native Boolean selection conversion

SelectBox and StandaloneSelectBox now handle native Boolean option strings through one internal converter. The user approved local-only `1.1.4-preview.fields.2` for Colligere, retaining Field.Actions from `.1`. Public 1.1.3, the stable VersionPrefix and unrelated native WPF work are unchanged.

## Failure and cause

Colligere's supplied Aspire log reports InvalidCastException in BindConverter.ConvertToNullableBoolCore, called by SelectBox.TryParseValueFromString during onchange. The unhandled renderer error terminates the circuit before company submission. A real restored-package company event test reproduces the exception against `.1`; assigning the model property directly, as earlier tests did, bypasses the faulty input path.

Both selection entries passed ChangeEventArgs.Value strings into a Boolean converter that expects Boolean values. Their option formatter also returned boxed Boolean values: HTML conditional-attribute handling can omit false, and ordinary ToString casing need not match literal lowercase options. The [.NET InputSelect source](https://raw.githubusercontent.com/dotnet/aspnetcore/v10.0.11/src/Components/Web/src/Forms/InputSelect.cs) explicitly special-cases Boolean string formatting. Its nullable formatting is not copied wholesale: Flourish retains null as an undeclared option, distinct from false.

## Shared correction

Components.Internal.SelectValueConversion owns scalar formatting and parsing for both controls. It emits strings `true`, `false` and empty for Boolean/nullable Boolean values, uses Boolean string parsing, and retains existing culture-aware BindConverter conversion for other scalar types. All null options explicitly format as empty strings; omitting value would make HTML submit the visible caption rather than an undeclared selection.

Bound controls keep InputBase field notifications, model retention and localized parsing validation. Standalone controls ignore malformed input without notifying the owner. Disabled, native attribute/name, ChildContent and Options contracts remain unchanged. No public API, alias, alternate renderer, host workaround, CSS rule or dependency is added. Gallery's bound and standalone examples expose three-state selections with complete en-US, zh-CN and pt-BR resources.

## Verification and package scope

The Blazor-only Release solution builds with zero warnings/errors. All 415 library checks pass, including seven new real onchange/rendering groups covering both entries, bool/bool?, empty/null/invalid values, recovery, disabled events, canonical value attributes, handwritten options and int?/enum?/enum/Guid/string regressions. Gallery passes 7,266 checks, including its existing 124 post-event language/state checks. The Culture bridge passes 12 checks, and catalog validation passes 21,538 checks.

Exactly six Core/Blazor `.2` packages are generated and verified in Colligere/artifacts/local-nuget/1.1.4-preview.fields.2. Package IDs, internal dependency versions and static assets pass Verify-PackageSet. Colligere restores every Flourish package from that feed, retains Essential 1.3.0, builds Release cleanly and passes the previously failing three company selection/submission cases. Its broader focused run has 179 passes and one unconfigured PostgreSQL integration skip. No public upload, tag, commit, push, WPF packaging or running-service restart occurred.

## User operated acceptance

Rebuild and restart the consumer before testing; old running assemblies do not change when package references are edited. Exercise both Gallery selectors through all three states and languages, checking that values survive language changes. Test company NF-e selection, required validation, save/cancel, native login and the retained Field.Actions layout. Automated render/event checks do not certify physical browser geometry or live database persistence.
