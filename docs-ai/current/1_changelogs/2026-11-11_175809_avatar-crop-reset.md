# Restore the initial avatar crop

Recorded 2026-11-11 17:58:09 America/Sao_Paulo (UTC-03).

Avatar now includes a standard Quiet Reset action in its crop Dialog, translated as Reset, 还原 and Restaurar. It restores zoom to 100 and both position ranges to zero, redraws the original centered bitmap and ends any active pan. Reset keeps the crop Dialog open and performs no encode or host save. Its disabled/busy/ready checks and browser generation/disconnection checks prevent it from changing a saved, replaced or unavailable crop. A previous crop/save error clears after successful reset.

The shared Framework owns this behavior, so editable Avatar instances in any host receive the same control. No public component parameter or style contract changed. Colligere consumes six immutable local-only `1.4.8-preview.11` packages; the producer's declared stable prefix remains 1.4.7, and no remote package or Git publication occurred.

The avatar browser suite passed 12 checks, the full Blazor component executable passed 540 checks, and the culture catalog verifier passed 26,290 checks under PowerShell 7. The first catalog attempt under Windows PowerShell 5 lacked `Path.GetRelativePath`; the existing check itself was unchanged and passed in its supported modern runtime. Package inspection confirmed the preview version, matching Flourish dependency versions and the packaged `reset` export. NuGet pack emitted its existing missing-readme recommendations; there were no compiler warnings after the new test used the component parameter channel.

Manual verification: choose a photo, change all three ranges or drag the preview, then use Reset. Confirm the original centered crop returns while the Dialog remains open and the account photo is unchanged until Save. Confirm Cancel keeps the existing photo, Save persists the chosen crop, keyboard controls remain usable and Reset is unavailable during saving.
