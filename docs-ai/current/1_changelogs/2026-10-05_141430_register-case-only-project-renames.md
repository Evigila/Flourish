# Register case-only project renames in Git

This supplements 2026-10-05_141100_verify-all-project-naming.md. Windows case-insensitive filesystem moves alone did not register the intended path spelling in Git, so the coordinating task authorized exact Git rename registration through verified within-repository temporary paths.

The initial Git index was empty and had no user-staged overlap. Only these six tracked renames are now staged; no other content was added or committed:

- src/Flourish.WinUI3/Flourish.WINUI3.slnx to src/Flourish.WinUI3/Flourish.WinUI3.slnx.
- src/Gallery.Flourish.WINUI3/App.xaml to src/Gallery.Flourish.WinUI3/App.xaml.
- src/Gallery.Flourish.WINUI3/App.xaml.cs to src/Gallery.Flourish.WinUI3/App.xaml.cs.
- src/Gallery.Flourish.WINUI3/Gallery.Flourish.WINUI3.csproj to src/Gallery.Flourish.WinUI3/Gallery.Flourish.WinUI3.csproj.
- src/Gallery.Flourish.WINUI3/MainWindow.xaml to src/Gallery.Flourish.WinUI3/MainWindow.xaml.
- src/Gallery.Flourish.WINUI3/MainWindow.xaml.cs to src/Gallery.Flourish.WinUI3/MainWindow.xaml.cs.

Git reports all six as R100, preserving original staged file contents. The platform solution's new Gallery reference remains an ordinary unstaged edit, as do the root solution and AI records. git diff --check and git diff --cached --check pass. Explicit assemblies/public namespaces and the prepared six-package identities remain unchanged; no Git add -A, commit, push or publication occurred.
