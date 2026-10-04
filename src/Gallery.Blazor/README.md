# Web Gallery

Run from the repository:

```powershell
dotnet run --project src/Gallery.Blazor
```

The default launch profile serves http://localhost:5188. The application demonstrates a generic workspace configured through Flourish.Blazor public APIs and library static assets. Pages include overview, local records, create/edit/detail, controls and appearance/source examples. Data and theme changes belong to a server circuit and reset when that circuit is replaced; there is no database or authentication demo.

See ../Flourish.Blazor/README.md for consumer setup and ../../docs-ai/current/blazor-manual-tests.md for browser acceptance. No separate Gallery CSS, JavaScript, UI package, downloaded font or icon font is required.
