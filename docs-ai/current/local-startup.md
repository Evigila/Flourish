# Local application startup

## Entry points and prerequisites

Run debug.bat from the repository root on Windows. It preserves the former start launcher contract: select a runnable project, Enter chooses the Blazor Gallery and 0 cancels. The launcher resolves its own scripts directory and restores the caller's location.

Use the SDK declared by global.json: 10.0.400, latestPatch roll-forward, no prerelease SDK selection. Existing .NET/Windows tooling and normal restore access are prerequisites; the launcher does not install SDKs, workloads or Visual Studio. Normal restore may download already declared packages. Desktop choices require Windows and their declared toolchain; WinUI is a placeholder, not a certified desktop workflow.

    .\debug.bat
    .\debug.bat -List
    .\debug.bat -Project blazor -NonInteractive
    .\debug.bat -Project Gallery.Flourish.WPF
    .\debug.bat -Project native
    .\debug.bat -Project winui -Platform x64

PowerShell 7 may invoke scripts/Start-Project.ps1 for Web projects outside Windows; this is source/plan covered rather than new cross-platform acceptance. Non-Windows desktop selection fails.

## Runnable allowlist

| Choice | Project | Scope |
| --- | --- | --- |
| 1 / blazor | Gallery.Flourish.Blazor | Default Interactive Server Gallery, existing HTTP profile localhost:5188 |
| 2 / wpf | Gallery.Flourish.WPF | Native Gallery using current production controls, optional Design/Culture |
| 3 / winui | Gallery.Flourish.WinUI3 | Existing unpackaged empty-window placeholder |
| 4 / native | Tests.Flourish.Blazor.Native | Maintained Framework-only diagnostic host at localhost:5189, not a complete Gallery |

Aliases, full names and menu numbers are case-insensitive. Arbitrary paths, library projects and generated commands are rejected. List/Help work without the SDK; unattended operation requires Project rather than hanging for a prompt.

## Execution and failures

The launcher restores the selected project, builds Release with no restore, then runs that build without build/restore. Configuration Debug is explicit; runtime Development and build configuration are distinct. Web entries retain the HTTP launch profile.

Platform Auto/x86/x64/ARM64 applies to WinUI; Auto uses Windows machine architecture and preserves the same platform/RID across all stages. Missing SDK/native tooling or a failed command stops later stages with a nonzero exit. Ctrl+C stops the console-owned application; occupied ports do not trigger unrelated process termination. No browser is opened automatically.

Release preparation and explicitly authorized stable tag publication use scripts/release.bat. SAF check/sync/backup use origin/local. None of these are aliases for starting a Gallery, resetting consumer data or publishing a NuGet version.

## Verification and manual acceptance

Test-StartProject uses command-plan fixtures, mock runners and a real isolated batch fixture with a spaced path. It remains maintained automation and does not open windows. Current test entry names should follow the new Debug BAT while implementation filenames remain Start-Project/Start-Common.

After an intended normal restore/build/start, manually verify the menu, selected URL, culture/theme, Ctrl+C and occupied-port response. WPF/WinUI rendering, OS dialogs and desktop tooling require separate Windows acceptance. No Computer Use testing is permitted. Cleaned bin/obj, package consumers and verification output are recreated when their relevant build/tests next run; their removal does not upgrade dependencies or verify a new runtime state.
