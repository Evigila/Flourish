import assert from "node:assert/strict";
import { readFile } from "node:fs/promises";
import test from "node:test";

const base = new URL("../../src/Flourish.Blazor/", import.meta.url);
const read = async path => (await readFile(new URL(path, base), "utf8")).replace(/\/\*[\s\S]*?\*\//g, "");
const framework = "Flourish.Blazor.Framework/";
const design = "Flourish.Blazor.Design/";
function rule(css, selector) {
  const found = [...css.matchAll(/([^{}]+)\{([^{}]*)\}/g)].find(match => match[1].trim() === selector);
  assert.ok(found, `Missing ${selector}`);
  return Object.fromEntries(found[2].split(";").map(value => value.trim()).filter(Boolean).map(value => {
    const colon = value.indexOf(":"); return [value.slice(0, colon).trim(), value.slice(colon + 1).trim()];
  }));
}

test("heading feedback follows observable sources in the same style update, including sibling dialogs", async () => {
  const css = await read(`${framework}wwwroot/page-heading-loading.css`);
  assert.match(css, /:has\(\s*:is\(\[data-f-loading="true"\],\[aria-busy="true"\]\)/);
  for (const scope of ["body", ".f-root", ".f-page-body", ".content-surface", ".navigation-surface", ".f-content-scroll"]) assert.ok(css.includes(scope));
  for (const hidden of ['[hidden] *', '[aria-hidden="true"] *', '[inert] *', 'dialog:not([open]) *', '[popover]:not(:popover-open):not([data-f-open]) *']) assert.ok(css.includes(hidden), `Inactive source guard ${hidden}`);
  assert.match(css, /\.f-page-heading\[data-heading-loading="true"\]\s*>\s*\.f-heading-loading,/);
  assert.doesNotMatch(css, /(?:transition|animation)-delay\s*:/);
  const component = await read(`${framework}Components/PageHeading.razor`);
  assert.match(component, /public bool Loading/);
  assert.doesNotMatch(component, /data-f-loading|aria-busy|f-progress-fill|IJSRuntime/);
});

test("heading progress is flush, independent of title width and absent from normal layout", async () => {
  const css = await read(`${framework}wwwroot/page-heading-loading.css`);
  const bar = rule(css, ".f-heading-loading");
  assert.equal(bar.display, "none");
  assert.equal(bar.position, "absolute");
  assert.equal(bar["inset-block-end"], "0");
  assert.equal(bar["block-size"], "4px");
  assert.equal(bar.margin, "0");
  assert.equal(bar.padding, "0");
  assert.equal(bar["pointer-events"], "none");
  assert.equal(bar["inset-inline"], "var(--f-content-gutter,var(--f-page-gutter,24px))");
  assert.match(rule(css, ".f-page-full > .f-page-heading > .f-heading-loading")["inset-inline"], /--f-content-width,1180px/);
  assert.match(css, /@media\(max-width:560px\)[\s\S]*?inset-inline:18px/);
  const layout = await read(`${framework}wwwroot/presentation/layout.css`);
  assert.equal(rule(layout, ".f-document-surface .f-page-heading").position, "relative");
});

test("heading animation remains separate from ordinary progress and respects reduced motion", async () => {
  const css = await read(`${framework}wwwroot/page-heading-loading.css`);
  assert.match(css, /@keyframes f-heading-loading-travel/);
  assert.match(css, /@media\(prefers-reduced-motion:reduce\)/);
  assert.match(css, /animation:none/);
  assert.doesNotMatch(css, /transform\s*:|\.f-progress\b/);
  const skin = await read(`${design}wwwroot/page-heading-loading.css`);
  assert.equal(rule(skin, ".f-heading-loading").color, "var(--f-primary)");
  assert.match(skin, /@media\(forced-colors:active\)/);
  for (const path of [`${framework}wwwroot/framework.css`, `${design}wwwroot/design.css`]) assert.match(await read(path), /@import url\('\.\/page-heading-loading\.css'\)/);
});
