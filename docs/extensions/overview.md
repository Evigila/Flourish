# Flourish extensions

Integration packages live in the Flourish.Extensions solution. Each extension connects otherwise
independent packages through their public APIs and consumes those packages through NuGet.

Current package:

- `Flourish.Extensions.Culture.WPF` connects `Arkheide.Flourish.WPF` with
  `Essential.Culture` through `UseEssentialCulture()`.
