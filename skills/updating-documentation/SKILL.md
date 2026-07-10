---
name: updating-documentation
description: Update Flourish conceptual articles, API documentation, navigation, DocFX configuration, templates, and generated previews. Use when editing docs/en-us, docs/zh-cn, public API XML comments, DocFX inputs, or the English and Chinese documentation sites, including work that must preserve feature-oriented information architecture, neutral public-documentation style, and bilingual parity.
---

# Updating Documentation

## Repository Map

Use this skill in the Flourish repository root.

- `.config/dotnet-tools.json`: local .NET tool manifest. DocFX is pinned as `docfx` version `2.78.5`.
- `docs/docfx.en-us.json`: English DocFX config. Outputs to `docs/_site/en-us`.
- `docs/docfx.zh-cn.json`: Chinese DocFX config. Outputs to `docs/_site/zh-cn` and applies `docs/zh-cn/api-overwrites/**/*.md`.
- `docs/filterConfig.yml`: API filter. Only `ArkheideSystem.Flourish.Abstract` is public in generated API docs.
- `docs/en-us/`: English conceptual docs and TOCs.
- `docs/zh-cn/`: Chinese conceptual docs, TOCs, and API overwrite docs.
- `docs/material/material/public/main.css`: active shared custom theme CSS.
- `docs/templates/flourish/public/main.js`: active custom DocFX JS that adds the language switch.
- `docs/_site/`: generated output. It is ignored by git and is not the source of truth.

Do not treat `docs/material/material-classic` or `docs/material/docs` as the active Flourish docs unless a config file is changed to reference them.

## Conceptual Documentation Model

Organize conceptual documentation by user-visible feature or task, not by builder method.

- Give each capability one canonical conceptual article. Document its `Configure...` method, enabling switch, workflow, observable behavior, and constraints together in that article.
- Use feature names such as **Navigation**, **Profile**, **Motion**, or **Application data** for article titles and TOC entries. Do not create a **Configuration APIs** category or use `Configure...` as the conceptual page title.
- A canonical feature article may retain a `configure-*.md` filename to preserve a stable URL. Visible titles and TOC labels must remain feature-oriented; rename the file only when the task includes a redirect or link-migration plan.
- Keep a type-named overview such as **IFlourishBuilder** only when the type itself is the concept being taught. It may summarize and link to feature articles, but must not duplicate their tutorials.
- When a feature overview and a `Configure...` page both exist, merge the useful details into the feature overview, remove the duplicate page, and update every TOC entry and cross-link.
- Keep overview pages concise. For example, **Shell configuration** explains `ConfigureShell` and links to feature articles; it does not repeat the complete title bar, navigation, toolbar, or motion tutorials.
- Put exhaustive signatures, parameters, overloads, and member lists in the generated API reference. Use conceptual articles to teach when and how to use the feature.
- Link to the canonical feature article when another article needs context. Repeat only the minimum information required to complete the local task.

Before changing the structure, inventory both language TOCs and search all conceptual articles for `Configure...` references. Define the canonical article for every affected method, then apply the same mapping to English and Chinese.

## Public Documentation Style

Write from the perspective of a library user learning stable public behavior.

- Lead with what the feature provides, when it is useful, and any prerequisite. Introduce the configuration method inside that explanation.
- Prefer objective cause-and-effect wording. State the observable behavior first, then explain the user-facing benefit when it matters.
- Describe the current public contract without release history. Avoid phrases such as “kept for source compatibility,” “the old API,” “version 1,” “previously,” or “currently supports.” Put migration history in an explicitly requested migration guide or changelog instead.
- Avoid implementation diaries and sample-project state. Do not document exact values derived from Gallery configuration, internal schema revisions, source-file decisions, or one build's generated identifiers.
- Include internal details only when they form a useful, stable user-facing contract. Prefer “credentials are protected for the current Windows user” over a list of internal fields, hash inputs, and sample-specific secret IDs.
- Avoid pixel measurements, control-style inventory, and exact UI microcopy unless consumers must design against them.
- Use neutral example identifiers such as `Foobar`, `Foo Bar`, `Example Company`, `HomePage`, `ReportsPage`, and `AppCommandParser`. Do not use real people, real organizations, or configuration copied from the Gallery project.
- Keep recommendations evidence-based. Explain the relevant constraint instead of using subjective comparisons such as “marketing-style” or project-specific judgments.
- Use direct, instructional language and present tense. Do not address repository maintainers or narrate the editing process inside an article.

For example, prefer this pattern:

> Profile does not depend on window focus, so opening the native file picker does not dismiss it. Click outside the card or press Esc to close it.

Avoid a chronological account of which window gains focus, what the sample application does next, or why the implementation changed.

## Conceptual Article Shape

Use the smallest structure that explains the feature clearly:

1. YAML front matter with a feature-oriented `title` and one-sentence `description`.
2. An H1 that matches the feature title.
3. A short overview that states purpose and prerequisites.
4. A minimal, generic example that includes required enabling switches.
5. Task- or behavior-oriented sections for the concepts users must understand.
6. A short **Related features** section when cross-links add value.

Do not force a section when the article is too small to need it. Keep English and Chinese articles structurally and semantically aligned; translate for clarity instead of preserving awkward sentence order. Require the same canonical href order, prerequisites, public behaviors, constraints, and example outcome in both languages, while allowing section boundaries and sentence order to differ.

## Editing Rules

- Edit source docs or templates, not `docs/_site`, unless the user explicitly asks to patch generated output only.
- Verify behavior against public interfaces, implementations, and tests before documenting it. Treat Gallery as an implementation example, not the documentation source of truth.
- Update English and Chinese versions together when a conceptual feature, structure, example, or shared public behavior changes.
- If conceptual wording also appears in public XML comments or `docs/zh-cn/api-overwrites`, update those sources so generated API pages do not preserve conflicting style or stale behavior.
- If you change shared templates or assets such as `docs/material/material/public/main.css` or `docs/templates/flourish/public/main.js`, rebuild both English and Chinese sites before previewing.
- If you change only English conceptual docs, rebuild and preview English.
- If you change only Chinese conceptual docs or API overwrites, rebuild and preview Chinese.
- If you change public API XML comments or public types in `src/Flourish/Abstract`, rebuild both language sites because both configs regenerate API pages from the project metadata.
- Read Markdown, YAML, and skill files with UTF-8 explicitly in PowerShell, especially Chinese content:

```powershell
Get-Content -Path <file> -Encoding utf8
```

Console output may look garbled if the encoding is omitted. Do not rewrite whole Chinese files just because terminal output looks wrong; verify with `-Encoding utf8` first.

## DocFX Build Workflow

Run commands from the repository root.

Restore the local DocFX tool when needed:

```powershell
dotnet tool restore
```

Build English:

```powershell
dotnet tool run docfx docs\docfx.en-us.json
```

Build Chinese:

```powershell
dotnet tool run docfx docs\docfx.zh-cn.json
```

Expected successful output includes `Build succeeded`, `0 warning(s)`, and `0 error(s)`.

Important: `_site` is git-ignored. A successful build may update `docs/_site/...` without changing `git status`. That is normal. Source docs, config, XML comments, template CSS, and template JS remain the reviewable changes; do not infer permission to commit them.

## Sandbox And Elevation Notes

This repository often needs access to user-level NuGet and tool caches.

Common sandbox failures:

- `dotnet tool restore` can fail with `Access to the path '%APPDATA%\NuGet\NuGet.Config' is denied`.
- `dotnet tool run docfx ...` can fail because the restored local tool is invisible inside the sandbox or because DocFX needs the user NuGet package cache.
- `dotnet build .\Flourish.slnx` can fail with the same `NuGet.Config` access error.

When those commands are required and fail for those reasons, rerun the same command with elevated permissions and a narrow prefix rule:

- Tool restore: prefix `["dotnet", "tool", "restore"]`
- DocFX build or serve: prefix `["dotnet", "tool", "run", "docfx"]`
- Project build: prefix `["dotnet", "build"]`

Do not work around these failures by editing generated output or bypassing NuGet/tool restoration.

## Preview Workflow

Do not preview DocFX pages through `file://` when using browser automation. Browser policy may block local file URLs. Serve the generated site over localhost instead.

Serve English:

```powershell
dotnet tool run docfx serve docs\_site\en-us --hostname 127.0.0.1 --port 8098
```

Serve Chinese:

```powershell
dotnet tool run docfx serve docs\_site\zh-cn --hostname 127.0.0.1 --port 8097
```

If a port is busy, choose another local port and mention it in the final response.

Open representative pages in the browser:

- English API example: `http://127.0.0.1:8098/api/ArkheideSystem.Flourish.Abstract.IMessageService.html`
- Chinese API example: `http://127.0.0.1:8097/api/ArkheideSystem.Flourish.Abstract.IMessageService.html`
- Conceptual pages: use `/articles/<page>.html`.

For visual or CSS fixes, verify computed styles in the actual page, not only source CSS. For example, after changing the navbar logo rule, check `#logo.getBoundingClientRect()` and `getComputedStyle(#logo)` on both language sites. The previous logo fix required both:

- source template rule in `docs/material/material/public/main.css`
- regenerated output in each language site, because `docs/_site/en-us/public/main.css` and `docs/_site/zh-cn/public/main.css` are separate generated files

After previewing, stop the DocFX serve process. If the exec session cannot receive Ctrl+C, locate the `dotnet` preview process by start time or port and stop it. Elevated permission may be needed because the server may have been started elevated.

## Validation Checklist

Before finishing a documentation task:

- Check the relevant source docs or template files changed, not only `_site`.
- Confirm every affected `Configure...` method has one canonical feature article and no duplicate configuration page.
- Confirm conceptual TOCs use feature names and contain no **Configuration APIs** grouping.
- Search for links to removed or renamed pages and for stale sample-specific or historical wording.
- Compare the English and Chinese TOCs, titles, examples, and feature coverage for semantic parity.
- For a mutation task, build every affected language site. For a read-only audit, do not build or preview unless the user asks for runtime verification or the audit requires it.
- For template/shared visual changes, build and preview both `en-us` and `zh-cn`.
- For conceptual navigation changes, preview a representative article and the sidebar in both languages.
- Confirm DocFX reports `0 warning(s)` and `0 error(s)`.
- Use localhost preview rather than `file://`.
- For visual fixes, collect a browser-level measurement or screenshot.
- Stop any preview server.
- Report any command that required elevation and why.
- Mention if DocFX build was intentionally skipped, and why.

## Common Pitfalls

- Rebuilding only one language after a shared template change leaves the other language with stale generated CSS.
- Looking only at `docs/material/...` does not prove the served site is fixed; check `docs/_site/<lang>/public/main.css` after building.
- `docs/_site` is ignored, so `git status` can be clean even when generated preview output changed.
- Chinese output can look corrupted in the terminal unless read with UTF-8.
- The Chinese config includes `default(zh-cn)` and API overwrites; English does not.
- The active theme is `material/material`, not `material-classic`.
