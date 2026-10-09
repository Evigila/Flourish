# Local application startup

## Entry points and prerequisites

Run `start.bat` from the repository root on Windows and select a project. Enter selects the Blazor Gallery; `0` cancels. The batch file locates its own scripts directory, so calling it by an absolute path from another directory also works.

The launcher needs Windows PowerShell 5.1 or later and the existing .NET SDK selected by `global.json`: 10.0.400, with `latestPatch` roll-forward and no prerelease SDK selection. It does not use the Visual Studio UI or install SDKs, workloads or build tools. Normal restore may download the projects' already-declared NuGet dependencies. Windows is required for desktop entries. The Blazor workflow is verified through the CLI; the existing WinUI3 placeholder has additional Windows/XAML toolchain requirements and is not a certified desktop workflow.

```powershell
.\start.bat
.\start.bat -List
.\start.bat -Project blazor -NonInteractive
.\start.bat -Project Gallery.Flourish.WPF
.\start.bat -Project native
.\start.bat -Project winui -Platform x64
```

Outside Windows, use PowerShell 7 directly for Web projects: `./scripts/Start-Project.ps1 -Project blazor`. Desktop selections fail on non-Windows hosts. This portable path is source/plan covered, not a claim of live cross-platform acceptance.

## Runnable-project allowlist

| Selection | Project | Scope |
| --- | --- | --- |
| `1`, `blazor` | Gallery.Flourish.Blazor | Default interactive Gallery; existing HTTP profile at `http://localhost:5188`. |
| `2`, `wpf` | Gallery.Flourish.WPF | Reconstructed native WPF Gallery using the current Blazor control standard; see wpf-native-integration.md. |
| `3`, `winui` | Gallery.Flourish.WinUI3 | Existing unpackaged WinUI3 placeholder, not the Blazor release scope. |
| `4`, `native` | Tests.Flourish.Blazor.Native | Framework-only diagnostic Web host at `http://localhost:5189`, not a complete Gallery. |

Aliases, full project names and valid menu numbers are case-insensitive selections. Arbitrary paths, library projects and generated command strings are rejected. `-List` and `-Help` work without an SDK. Non-interactive use requires `-Project` rather than hanging on a prompt.

## Execution and failure boundaries

The launcher restores the selected project, builds `Release` with `--no-restore`, then runs the same build with `--no-build --no-restore`. An explicit `-Configuration Debug` is available, but task verification remains Release-only. Web entries retain their existing `http` profile and Development environment; build configuration and runtime environment are distinct. The caller's working directory is restored on exit.

`-Platform Auto/x86/x64/ARM64` applies only to WinUI3; Auto resolves the Windows machine architecture. Its Platform and runtime identifier are identical in all three steps. The launcher never changes project files or installs missing XAML/Windows tooling. A missing SDK or failed native command stops later steps and returns a nonzero exit code.

Ctrl+C stops the application launched in that console. Port conflicts are reported rather than resolved by killing another instance. The script does not open a browser, stop unrelated processes, clear shared caches, publish packages, create Git changes or invoke database workflows.

## Verification and manual acceptance

`scripts/Test-StartProject.ps1` uses command-plan fixtures and mock runners; it does not build projects or open windows. It covers validation, failure short-circuiting, process exit codes, SDK-free discovery, invocation from another directory and a real batch fixture with spaces in its repository path. The existing Release gate runs it alongside CSS checks. Run both supported Windows engines when changing the launcher:

```powershell
powershell.exe -NoProfile -ExecutionPolicy Bypass -File scripts/Test-StartProject.ps1
pwsh -NoProfile -File scripts/Test-StartProject.ps1
```

Manually verify the interactive menu, the selected browser URL, language/theme changes, Ctrl+C and occupied-port behavior. WPF/WinUI3 window rendering and installed desktop prerequisites require separate Windows acceptance; no Computer Use or desktop-window startup is part of automated verification.
