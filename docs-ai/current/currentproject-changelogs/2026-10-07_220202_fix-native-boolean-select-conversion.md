# Fix native Boolean selection conversion

Date: 2026-10-07, America/Sao_Paulo.

SelectBox and StandaloneSelectBox share one internal scalar converter rather than casting native strings through BindConverter's Boolean path. Boolean values format as lowercase true/false, and undeclared options as explicit empty strings. Nullable state, InputBase parsing validation, disabled behavior, culture, native names and non-Boolean scalar conversion remain intact. No public API or CSS changes are introduced.

Both Gallery samples now demonstrate three states with localized resources. Seven new real control event/rendering groups cover Boolean and adjacent scalar cases. The Blazor Release solution is warning/error-free; all 415 library checks, 7,266 Gallery checks including 124 post-event checks, 12 bridge checks and 21,538 catalog checks pass.

The authorized six-package `1.1.4-preview.fields.2` local set retains Field.Actions and passes the package verifier. Colligere consumes it through NuGet and its formerly failing company tests now pass. Stable 1.1.3 and `.1` remain unchanged. The [selection report](../bugfix-reports/2026-10-07_boolean-select-native-conversion.md) records evidence and manual acceptance. No WPF packaging, public upload, tag, commit or push occurred.
