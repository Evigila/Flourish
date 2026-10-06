import assert from "node:assert/strict";
import { readFile } from "node:fs/promises";
import test from "node:test";

const framework = new URL("../../src/Flourish.Blazor/Flourish.Blazor.Framework/", import.meta.url);

test("chart period and display controls share a wrapping baseline-aligned library toolbar", async () => {
  const source = await readFile(new URL("Components/LineChart.razor", framework), "utf8");
  const css = await readFile(new URL("wwwroot/line-chart.css", framework), "utf8");
  assert.match(source, /ControlsContent is not null \|\| ShowDisplayOptions/);
  assert.match(source, /f-line-chart-controls-content.*@ControlsContent/);
  assert.match(source, /ShowDisplayOptions\).*MultiSelectBox/);
  assert.match(css, /\.f-line-chart-controls\s*\{[^}]*display:flex;[^}]*flex-wrap:wrap;[^}]*align-items:flex-end;/);
  assert.match(css, /\.f-line-chart-controls-content\s*\{[^}]*min-inline-size:0;/);
  assert.doesNotMatch(css, /\.f-line-chart-controls-content\s*\{[^}]*margin-inline-end:auto;/);
  assert.match(css, /\.f-line-chart-heading-controls\s*\{[^}]*margin-block-end:0;/);
  assert.match(css, /\.f-line-chart > \.f-section > \.f-section-heading > \.f-heading-actions\s*\{[^}]*margin-inline-start:auto;/);
  assert.match(source, /<Section Id="@Id" Title="@Heading" Actions="@\(ControlsContent is not null \|\| ShowDisplayOptions \? Controls\(true\) : null\)" ChildContent="@Plot"/);
  assert.match(source, /MultiSelectBox[^\n]*Icon="remove_red_eye"[^\n]*TriggerVariant="ButtonVariant.Secondary"/);
});

test("inline input takes remaining width without forcing its natural-sized action onto another row", async () => {
  const css = await readFile(new URL("wwwroot/framework.css", framework), "utf8");
  assert.match(css, /\.f-inline-actions > \.f-input\s*\{[^}]*flex:1 1 0;[^}]*width:auto;[^}]*min-width:0;/);
  assert.match(css, /\.f-field-control > \.f-inline-actions\s*\{[^}]*margin-block-start:0;/);
});
