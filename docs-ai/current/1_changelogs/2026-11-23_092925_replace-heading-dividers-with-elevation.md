# Replace heading dividers with elevation

Recorded on 2026-11-23 at 09:29:25 America/Sao_Paulo (UTC−03:00).

ShellHeader and TitleBar use a shared downward shadow in place of the bottom border. Expanded PageHeading uses its existing spacing below the title. The floating compact heading and the explicitly compact sticky heading use the same shadow as the top bar. The floating heading reserves space for the shadow within the existing scroll-stage clip, including during its slide transition.

Focused style, header layout and heading controller checks passed. All six local 1.4.11-preview.2 packages passed metadata and asset verification.
