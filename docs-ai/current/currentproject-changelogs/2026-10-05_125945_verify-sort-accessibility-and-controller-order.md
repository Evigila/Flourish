# Verify sort accessibility and controller order

The final local preparation passed after two additional host-contract findings. Boolean aria-pressed expressions omitted false states during SSR; sort and keyboard-reorder controls now emit explicit true/false strings. Actual NuGet framework rendering checks cover the complete state set, selection of a hidden sortable column and active keyboard reorder.

The advanced editor now appears directly beside sorting and before Display, restoring the consumer's documented controller order without changing editing callbacks, permissions or data behavior. Existing regression assertions were retained.

scripts/Test-Release.ps1 completed with exit code 0. The configured six 1.1.0 packages passed dependency/assembly/static-assets validation, and all four isolated NuGet-only consumers passed their combined 67 checks. Evidence: artifacts/package-consumers/dd0ad177e0c94552bd390cbd3c9564ae. The final Framework nupkg SHA256 is 3BD824F6AE2D767DF602478CC68CFE60782B8FCAD91DDDD83CF0DD9833F74FE1.

This is local release preparation, not an uploaded release. No commit, tag or push was performed. WPF remains excluded from the 1.1.0 release manifest, and the Culture bridge remains opt-in outside the dependency-only Blazor convenience package.
