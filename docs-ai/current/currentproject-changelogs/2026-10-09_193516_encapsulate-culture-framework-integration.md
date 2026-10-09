# Encapsulate Culture framework integration

- Local date/time: 2026-10-09 19:35:16, America/Sao_Paulo.
- User request: reduce Gallery service startup to AddFlourishFramework, AddFlourishDesign and RecordStore; expose ConfigureCulture through the optional extension and encapsulate its non-Gallery responsibilities.
- Confirmed preference scope: appsettings.Flourish.json stores shared defaults; browser cookies store independent personal UI/format selections.
- Added Framework configuration loading, provider-neutral integration registration and automatic Interactive Server service setup for Web hosts. Design reads the shared appearance defaults through its configuration overload.
- Moved catalogs, Essential registration, request negotiation, persistent selection transactions, renderer subscriptions and LanguagePicker into the Culture extension. Added explicit usage metadata and complete Gallery/API/sample/navigation registration.
- Removed old split registration/writer/provider/host-selector APIs and assets without aliases. This supersedes the preceding same-day persistence records without editing their history.
- Updated Gallery to consume the extension project directly; preserved independent NuGet consumer verification. Extension now depends on Framework and uses the existing Razor SDK. External versions and package IDs remain unchanged.
- Added a concise 1.1.4-preview ChangeLog entry, updated active architecture/integration/API guides and recorded the diagnosis and manual checklist in [the report](../bugfix-reports/2026-10-09_culture-startup-encapsulation.md).
- Verification: zero compiler warnings/errors; Framework 436/436, Gallery 8,275, Culture 21, Node 4, catalog/boundary 22,320, ChangeLog 91, HTTP preference 8/culture 583/navigation 240, six package contracts and 136 isolated consumer checks passed.
- No Computer Use, Git commit, release version change, tag or publication. User-run browser acceptance remains listed in the report.
