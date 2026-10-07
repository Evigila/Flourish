# Centered gutters Dialog actions and editing corners

The local `1.1.3-preview.workspace.1` candidate corrects three library layout contracts. Public 1.1.2 is unchanged.

## Causes and corrections

NavigationSurface previously ignored ConfigureLayout.SetContentWidth and had no proportional gutter configuration. Its CenteredContentGutterScale now accepts finite values from 0 to 1, defaults to 1 and emits an invariant-culture, instance-scoped value. Design computes each side as `max(minimum gutter, (stage width - configured width) / 2) * scale`; heading and content use the same result. Framework-only centered layout also scales its available-width cap. Fluid and FullWidth are unchanged. The NavigationSurface Gallery uses PageBody and a localized half-gutter switch.

Dialog's footer already aligned right, but a full-width InlineActions applied its centered default inside it. The Dialog action context now owns end alignment for nested action rows and native forms. Ordinary InlineActions elsewhere keeps its Start/Center/End contract; Dialog lifecycle, results and POST transport are unchanged. Gallery and actual-component checks include nested rows and native submit forms.

EditingGrid's Design viewport rounded and clipped the table header. Its border radius is now zero; borders, dirty/error/selection cues and DataTable remain unchanged.

## Verification

- Core/Blazor six-library graph, Blazor checks and Gallery Release builds: zero warnings/errors.
- Blazor: 401/401 checks. Existing JavaScript controller suite: 99 passed. CSS SDK integration: 194 checks. Culture catalog integrity: 21,450 checks.
- Six candidate packages pass package-set, dependency and asset verification. Colligere restored all six from its local candidate feed; related tests are 273 passed and 24 environment skips. Its complete Web run retains the same three unrelated failures as its prior baseline.

Automated rendering/source checks are not physical browser geometry acceptance. Manually inspect wide/narrow centered pages, heading/content alignment, unchanged fluid/full-width pages, ordinary action rows, Centered/BottomSheet Dialogs with wrapped/native actions and EditingGrid scrolling/frozen cells. No Computer Use, public publication or running Aspire interruption occurred.
