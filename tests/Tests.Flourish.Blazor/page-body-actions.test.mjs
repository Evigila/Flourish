import assert from "node:assert/strict";
import { readFile } from "node:fs/promises";
import test from "node:test";

const root = new URL("../../", import.meta.url);
const read = async path => (await readFile(new URL(path, root), "utf8")).replace(/\/\*[\s\S]*?\*\//g, "");
const designRoot = "src/Flourish.Blazor/Flourish.Blazor.Design/wwwroot/";
const frameworkRoot = "src/Flourish.Blazor/Flourish.Blazor.Framework/wwwroot/";

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
