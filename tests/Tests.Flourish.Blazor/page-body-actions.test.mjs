import assert from "node:assert/strict";
import { readFile } from "node:fs/promises";
import test from "node:test";

const root = new URL("../../", import.meta.url);
const read = async path => (await readFile(new URL(path, root), "utf8")).replace(/\/\*[\s\S]*?\*\//g, "");
const designRoot = "src/Flourish.Blazor/Flourish.Blazor.Design/wwwroot/";
const frameworkRoot = "src/Flourish.Blazor/Flourish.Blazor.Framework/wwwroot/";

test("centered container variants reset locally and change horizontal gutters without compact form spacing", async () => {
  const framework = await read(`${frameworkRoot}framework.css`);
  assert.equal(property(rule(framework, ".f-page-body"), "--f-centered-gutter-scale"), "1");
  const expanded = rule(framework, ".f-page-centered-expanded");
  assert.equal(Number(property(expanded, "--f-centered-gutter-scale")), 0.5);
  assert.doesNotMatch(expanded, /(?:padding|margin)(?:-[a-z]+)?\s*:/);
  assert.doesNotMatch(framework, /f-page-compact-spacing/);
  assert.ok(framework.indexOf(".f-page-centered-expanded") > framework.indexOf(".f-page-body {"));
  const foundation = await read(`${designRoot}foundation.css`);
  assert.equal(property(rule(foundation, ".f-section"), "padding-top"), "clamp(44px,7vw,84px)");
  assert.equal(property(rule(foundation, ".f-page-body"), "--f-page-gutter"), "24px");
  assert.doesNotMatch(foundation, /f-page-compact-spacing/);
  const board = await read(`${frameworkRoot}display-board.css`);
  assert.equal(property(rule(board, ".f-display-board-content > .f-section"), "padding-block"), "0");
  assert.equal(property(rule(board, ".f-display-board-content > :is(.f-root,.f-data,.f-form-actions,.f-card,.f-shell,.navigation-surface,.f-page-body)"), "width"), "100%");
});

function rule(css, selector) {
  const found = [...css.matchAll(/([^{}]+)\{([^{}]*)\}/g)].find(match => match[1].trim() === selector);
  assert.ok(found, `Missing rule ${selector}`);
  return found[2];
}

function property(body, name) {
  return body.split(";").map(value => value.trim()).find(value => value.startsWith(`${name}:`))?.slice(name.length + 1).trim();
}

test("PageBody direct full-row actions leave room for content gutters", async () => {
  const foundation = await read(`${designRoot}foundation.css`);
  const gutterSelector = ".f-page-body > :not(.f-page-heading)";
  const sizingSelector = ".f-page-body > .f-uniform-grid-rectangle,.f-page-body > .f-form-actions";
  assert.equal(property(rule(foundation, gutterSelector), "margin-inline"), "var(--f-content-gutter)");
  assert.equal(property(rule(foundation, sizingSelector), "width"), "auto");
  assert.ok(foundation.indexOf(sizingSelector) > foundation.indexOf(gutterSelector));
  assert.doesNotMatch(rule(foundation, sizingSelector), /overflow\s*:|max-width\s*:|!important/);
  const actions = await read(`${designRoot}controls.css`);
  assert.equal(property(rule(actions, ".f-form-actions"), "width"), "100%");
});

test("PageBody bounds direct aligned action rows without changing nested rows or alignment", async () => {
  const framework = await read(`${frameworkRoot}framework.css`);
  const defaults = rule(framework, ".f-inline-actions");
  assert.equal(property(defaults, "justify-content"), "flex-end");
  assert.equal(property(defaults, "flex-wrap"), "wrap");
  const baseSelector = ".f-inline-actions[data-alignment]";
  const pageSelector = ".f-page-body > .f-inline-actions[data-alignment]";
  const base = rule(framework, baseSelector);
  const direct = rule(framework, pageSelector);
  assert.equal(property(base, "width"), "100%");
  assert.equal(property(base, "min-width"), "0");
  assert.equal(property(base, "box-sizing"), "border-box");
  assert.equal(property(direct, "width"), "auto");
  assert.ok(framework.indexOf(pageSelector) > framework.indexOf(baseSelector));
  assert.doesNotMatch(direct, /overflow\s*:|max-width\s*:|margin-inline\s*:|justify-content\s*:|!important/);
  assert.doesNotMatch(framework, /\.f-page-body\s+\.f-inline-actions\[data-alignment\]\s*\{/);
  for (const [alignment, justification] of [["start", "flex-start"], ["center", "center"], ["end", "flex-end"]]) {
    assert.equal(property(rule(framework, `.f-inline-actions[data-alignment="${alignment}"]`), "justify-content"), justification);
  }
  assert.equal(property(rule(framework, ".f-dialog-actions .f-inline-actions"), "justify-content"), "flex-end");
  const growingInput = rule(framework, ".f-inline-actions > .f-input");
  assert.equal(property(growingInput, "flex"), "1 1 0");
  assert.equal(property(growingInput, "width"), "auto");
  assert.equal(property(growingInput, "min-width"), "0");
  const board = await read(`${frameworkRoot}display-board.css`);
  const boardRow = rule(board, ".f-display-board-content > .f-inline-actions");
  assert.equal(property(boardRow, "width"), "100%");
  assert.equal(property(boardRow, "box-sizing"), "border-box");
  const centeredRow = ".f-display-board-centered > .f-display-board-viewport > .f-display-board-content:not(:has(> .f-code-block)) > .f-inline-actions:not([data-alignment-explicit])";
  assert.equal(property(rule(board, centeredRow), "justify-content"), "center");
  assert.doesNotMatch(rule(board, centeredRow), /(?:width|flex|overflow)\s*:|!important/);
  assert.doesNotMatch(board, /\.f-display-board-centered\s+\.f-inline-actions/);
  const foundation = await read(`${designRoot}foundation.css`);
  assert.equal(property(rule(foundation, ".f-page-body > :not(.f-page-heading)"), "margin-inline"), "var(--f-content-gutter)");
  assert.equal(property(rule(foundation, ".f-page-full"), "--f-content-gutter"), "0px");
  assert.equal(property(rule(foundation, ".f-page-fluid"), "--f-content-gutter"), "max(var(--f-page-gutter),calc((100% - var(--f-content-width))/6))");
  assert.equal(property(rule(foundation, ".f-page-centered"), "--f-content-gutter"), "calc(max(var(--f-page-gutter),calc((100% - var(--f-content-width))/2))*var(--f-centered-gutter-scale,1))");
});

test("Gallery exposes both centered containers alongside fluid and full pages with actual action rows", async () => {
  const page = await read("src/Gallery.Flourish.Blazor/Components/Samples/Layout/PageBodySample.razor");
  assert.doesNotMatch(page, /CompactSpacing|Sample_CompactPageSpacing|<StandaloneCheckBox/);
  assert.match(page, /<SelectBox[^>]*Options="Modes"[^>]*@bind-Value="Mode"/);
  assert.match(page, /new\("standard", Localization\.Parse\(TextKey\.Sample_StandardCenteredContainer\)\)/);
  assert.match(page, /new\("expanded", Localization\.Parse\(TextKey\.Sample_ExpandedCenteredContainer\)\)/);
  assert.match(page, /private string Mode = "standard"/);
  assert.match(page, /<ArkheideSystem\.Flourish\.Blazor\.Components\.PageBody Fluid="false" CenteredContainer="CenteredContainer\.Expanded"/);
  assert.match(page, /<ArkheideSystem\.Flourish\.Blazor\.Components\.PageBody Fluid="@\(Mode == "fluid"\)" FullWidth="@\(Mode == "full"\)" CenteredContainer="@\(Mode == "expanded" \? CenteredContainer\.Expanded : CenteredContainer\.Standard\)">[\s\S]*?<\/ArkheideSystem\.Flourish\.Blazor\.Components\.Section>\s*@foreach \(var alignment in Enum\.GetValues<HorizontalAlignment>\(\)\)\s*\{\s*<InlineActions Alignment="alignment">/);
  assert.match(page, /<ArkheideSystem\.Flourish\.Blazor\.Components\.Section[^>]*>[\s\S]*?<InlineActions Alignment="HorizontalAlignment.End">/);
  const actions = await read("src/Gallery.Flourish.Blazor/Components/Samples/Content/InlineActionsSample.razor");
  assert.match(actions, /<PageBody Fluid="false">\s*@foreach \(var alignment in Enum\.GetValues<HorizontalAlignment>\(\)\)\s*\{\s*<InlineActions Alignment="alignment">/);
  assert.doesNotMatch(actions, /CompactSpacing/);
  assert.doesNotMatch(actions, /style=|Class=/);
});

test("nested rectangle grids and bounded squares retain distinct sizing without consumer overrides", async () => {
  const css = await read(`${frameworkRoot}uniform-grid.css`);
  assert.equal(property(rule(css, ".f-uniform-grid"), "width"), "100%");
  assert.equal(property(rule(css, ".f-uniform-grid"), "max-width"), "100%");
  assert.equal(property(rule(css, ".f-uniform-grid-square"), "width"), "fit-content");
  assert.equal(property(rule(css, ".f-uniform-grid-columns"), "grid-template-columns"), "repeat(var(--f-grid-columns),minmax(0,var(--f-grid-cell-width)))");
  const wizard = await read("src/Gallery.Flourish.Blazor/Components/Pages/WizardExample.razor");
  assert.match(wizard, /<PageBody>\s*<PageHeading Title=[^>]*\/>\s*@if[\s\S]*?<UniformGrid Columns="2" NarrowColumns="1"/);
  assert.doesNotMatch(wizard, /style=|Class=/);
});
