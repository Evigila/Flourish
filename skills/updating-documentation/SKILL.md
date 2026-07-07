---
name: updating-documentation
description: Update Flourish repository documentation and DocFX outputs. Use when editing docs/en-us, docs/zh-cn, DocFX configuration, API XML comments, docfx-material templates, generated docs/_site output, or when building and previewing the English or Chinese documentation sites.
---

# Updating Documentation

## Repository Map

Use this skill in the Flourish repository root.

- `.config/dotnet-tools.json`: local .NET tool manifest. DocFX is pinned as `docfx` version `2.78.5`.
- `docs/docfx.en-us.json`: English DocFX config. Outputs to `docs/_site/en-us`.
- `docs/docfx.zh-cn.json`: Chinese DocFX config. Outputs to `docs/_site/zh-cn` and applies `docs/zh-cn/api-overwrites/**/*.md`.
- `docs/filterConfig.yml`: API filter. Only `AckSS.Flourish.Abstract` is public in generated API docs.
- `docs/en-us/`: English conceptual docs and TOCs.
- `docs/zh-cn/`: Chinese conceptual docs, TOCs, and API overwrite docs.
- `docs/material/material/public/main.css`: active shared custom theme CSS.
- `docs/templates/flourish/public/main.js`: active custom DocFX JS; currently adds the language switch.
- `docs/_site/`: generated output. It is ignored by git and is not the source of truth.

Do not treat `docs/material/material-classic` or `docs/material/docs` as the active Flourish docs unless a config file is changed to reference them.

## Editing Rules

- Edit source docs or templates, not `docs/_site`, unless the user explicitly asks to patch generated output only.
- If you change shared templates or assets such as `docs/material/material/public/main.css` or `docs/templates/flourish/public/main.js`, rebuild both English and Chinese sites before previewing.
- If you change only English conceptual docs, rebuild and preview English.
- If you change only Chinese conceptual docs or API overwrites, rebuild and preview Chinese.
- If you change public API XML comments or public types in `src/Flourish/Abstract`, rebuild both language sites because both configs regenerate API pages from the project metadata.
- Read Chinese Markdown with UTF-8 explicitly in PowerShell:

```powershell
Get-Content -Path docs/zh-cn/articles/<file>.md -Encoding utf8
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

Important: `_site` is git-ignored. A successful build may update `docs/_site/...` without changing `git status`. That is normal. Commit source changes such as docs, config, XML comments, template CSS, or template JS.

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

- English API example: `http://127.0.0.1:8098/api/AckSS.Flourish.Abstract.IMessageService.html`
- Chinese API example: `http://127.0.0.1:8097/api/AckSS.Flourish.Abstract.IMessageService.html`
- Conceptual pages: use `/articles/<page>.html`.

For visual or CSS fixes, verify computed styles in the actual page, not only source CSS. For example, after changing the navbar logo rule, check `#logo.getBoundingClientRect()` and `getComputedStyle(#logo)` on both language sites. The previous logo fix required both:

- source template rule in `docs/material/material/public/main.css`
- regenerated output in each language site, because `docs/_site/en-us/public/main.css` and `docs/_site/zh-cn/public/main.css` are separate generated files

After previewing, stop the DocFX serve process. If the exec session cannot receive Ctrl+C, locate the `dotnet` preview process by start time or port and stop it. Elevated permission may be needed because the server may have been started elevated.

## Validation Checklist

Before finishing a documentation task:

- Check the relevant source docs or template files changed, not only `_site`.
- Build every affected language site.
- For template/shared visual changes, build and preview both `en-us` and `zh-cn`.
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
