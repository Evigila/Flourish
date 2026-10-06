# Secondary label parity and chart heading

SecondaryNavigationItem uses the configured shell's shrinking f-navigation-label and full title, with common direct-item width constraints. Gallery and actual renderer tests compare both production entries. The previous selection-paint audit missed this structural difference; its history is retained.

LineChart Heading/Id composes the standard Section header with business ControlsContent and chart-owned icon-only Secondary display selection. The controls align right, preserve stable-key series state and wrap on narrow screens. Gallery and API guidance demonstrate the same composition without host rendering/state adapters.

The [report](../bugfix-reports/2026-10-06_secondary-label-parity-and-chart-heading.md) records 373 passing console contracts, catalog 80/695/311, clean isolated Gallery Release build, local Framework/Design candidates and passing navigation/chart/outline checks. The combined Node gate retains one access-style assertion failure outside these changed files. No external dependency, compatibility entry, publication, staging, commit or running-host interruption is introduced.
