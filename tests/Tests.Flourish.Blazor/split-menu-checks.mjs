import assert from "node:assert/strict";
import { readFile } from "node:fs/promises";
import { dirname, join, resolve } from "node:path";
import test from "node:test";
import { fileURLToPath } from "node:url";

const repositoryRoot = resolve(dirname(fileURLToPath(import.meta.url)), "../..");
const blazorRoot = join(repositoryRoot, "src/Flourish.Blazor");
const frameworkRoot = join(blazorRoot, "Flourish.Blazor.Framework/wwwroot");
const designRoot = join(blazorRoot, "Flourish.Blazor.Design/wwwroot");
const menuItemSelector = ".f-split-button-menu-content > .f-button";
const geometryProperties = new Set(["width", "min-width", "max-width", "justify-self", "align-self", "box-sizing"]);

async function expandCss(path) {
  const source = (await readFile(path, "utf8")).replace(/\/\*[\s\S]*?\*\//g, "");
  let result = "";
  let offset = 0;
  for (const match of source.matchAll(/@import\s+url\(['"]([^'"]+)['"]\)\s*;/g)) {
    result += source.slice(offset, match.index);
    result += await expandCss(resolve(dirname(path), match[1]));
    offset = match.index + match[0].length;
  }
  return result + source.slice(offset);
}

const frameworkCss = await expandCss(join(frameworkRoot, "framework.css"));
const designCss = await expandCss(join(designRoot, "design.css"));

// This is a deliberately narrow contract for standard direct Button items, not a browser
// or a pixel-layout simulator. Read the complete import cascade, resolve the relevant
// library selectors by specificity/importance/source order, and reject new menu rules
// that would need a contract review. Physical browser geometry remains manual acceptance.
function simpleButtonMatches(selector, state) {
  const classes = [...selector.matchAll(/\.([\w-]+)/g)].map(match => match[1]);
  if (!classes.includes("f-button") && !classes.includes("f-button-quiet")) return false;
  if (classes.some(name => !["f-button", "f-button-quiet"].includes(name))) return false;
  if (/[ >+~]/.test(selector)) return false;
  const condition = token => {
    if (token === ":hover") return !!state.hover;
    if (token === ":active") return !!state.active;
    if (token === ":focus-visible") return !!state.focus;
    if (token === ":disabled") return !!state.disabled;
    if (/^\[aria-disabled=(?:true|"true"|'true')\]$/.test(token)) return !!state.ariaDisabled;
    assert.fail(`Review the new Button geometry condition: ${token}`);
  };
  let matches = true;
  let remainder = selector.replace(/:not\(([^()]*)\)/g, (_, token) => {
    matches &&= !condition(token);
    return "";
  }).replace(/\.[\w-]+/g, "");
  remainder = remainder.replace(/:[\w-]+|\[[^\]]+\]/g, token => {
    matches &&= condition(token);
    return "";
  });
  assert.equal(remainder, "", `Review the new standard Button geometry selector: ${selector}`);
  return matches;
}

function itemGeometry(css, insideMenu, state = {}) {
  const winners = new Map();
  for (const rule of css.matchAll(/([^{}]+)\{([^{}]*)\}/g)) {
    const declarations = [...rule[2].matchAll(/(?:^|;)\s*([\w-]+)\s*:\s*([^;]+)/g)]
      .filter(match => geometryProperties.has(match[1]));
    if (!declarations.length) continue;
    for (const arm of rule[1].split(",")) {
      const selector = arm.trim().replace(/\s+/g, " ");
      let matches = false;
      let specificity = 0;
      if (selector === menuItemSelector) { matches = insideMenu; specificity = 2; }
      else if (selector === ".f-menu-panel *") { matches = insideMenu; specificity = 1; }
      else if (selector.includes(".f-split-button-menu-content") && selector.includes(".f-button")) {
        assert.fail(`Review the new split-menu geometry selector: ${selector}`);
      }
      else if (/^\.f-button(?:[.:[\s]|$|-quiet)/.test(selector)) {
        matches = simpleButtonMatches(selector, state);
        specificity = [...selector.matchAll(/\.[\w-]+|\[[^\]]+\]|:(?!not\()[\w-]+/g)].length;
      }
      if (!matches) continue;
      for (const declaration of declarations) {
        const value = declaration[2].trim();
        const important = /!important\s*$/.test(value) ? 1 : 0;
        const current = winners.get(declaration[1]);
        if (current && (important < current.important
          || important === current.important && specificity < current.specificity)) continue;
        winners.set(declaration[1], {
          value: value.replace(/\s*!important\s*$/, ""), important, specificity,
        });
      }
    }
  }
  return Object.fromEntries([...winners].map(([name, winner]) => [name, winner.value]));
}

test("native split menu Buttons and links fill one column with or without optional Design", () => {
  for (const css of [frameworkCss, frameworkCss + "\n" + designCss]) {
    for (const state of [{}, { hover: true }, { active: true }, { focus: true }, { disabled: true }, { ariaDisabled: true }]) {
      const geometry = itemGeometry(css, true, state);
      assert.equal(geometry.width, "100%", "A menu item's border still shrinks to its text.");
      assert.equal(geometry["justify-self"], "stretch");
      assert.equal(geometry["align-self"], "stretch");
      assert.equal(geometry["min-width"], "0");
      assert.equal(geometry["max-width"], "100%");
      assert.equal(geometry["box-sizing"], "border-box", "Borders/padding must stay inside the shared item width.");
    }
  }
});

test("full-width item geometry is structural, never a text, hover, focus or disabled special case", async () => {
  const layout = (await readFile(join(frameworkRoot, "split-button.css"), "utf8")).replace(/\/\*[\s\S]*?\*\//g, "");
  const menuRules = [...layout.matchAll(/([^{}]+)\{([^{}]*)\}/g)]
    .filter(rule => rule[1].includes("f-split-button-menu-content") && rule[1].includes("f-button"));
  assert.equal(menuRules.length, 1);
  assert.equal(menuRules[0][1].trim(), menuItemSelector);
  assert.doesNotMatch(menuRules[0][1], /hover|focus|disabled|nth-|data-|aria-/);
  assert.doesNotMatch(menuRules[0][2], /background|color|border:|font|height|!important/,
    "The layout repair must not introduce a second Button skin or fixed text height.");
  assert.match(designCss, /\.f-button-quiet:hover:not\(:disabled\):not\(\[aria-disabled=true\]\)\s*\{[^}]*background:var\(--f-target-preview\)/);
  assert.match(designCss, /\.f-button:focus-visible[^{}]*\{[^}]*outline:3px solid var\(--f-accent\)/);
});

test("normal content-width Buttons and the split's square triangle keep their separate geometry", async () => {
  const normal = itemGeometry(frameworkCss + "\n" + designCss, false);
  assert.equal(normal.width, "max-content");
  assert.equal(normal["justify-self"], "start");
  assert.equal(normal["align-self"], "start");
  const layout = await readFile(join(frameworkRoot, "split-button.css"), "utf8");
  assert.match(layout, /\.f-split-button:not\(\.f-secondary-row\) > :is\(\.f-split-button-secondary,\.f-split-button-menu\)\s*\{[^}]*flex:0 0 48px; width:48px; height:48px/);
  assert.match(layout, /\.f-split-button-menu-content\s*\{[^}]*display:grid/);
});

test("the regression detects the former text-width Design override if the scoped library rule is removed", () => {
  const withoutMenuRule = (frameworkCss + "\n" + designCss)
    .replace(/\.f-split-button-menu-content\s*>\s*\.f-button\s*\{[^}]*\}/g, "");
  const former = itemGeometry(withoutMenuRule, true);
  assert.equal(former.width, "max-content");
  assert.equal(former["justify-self"], "start");
  assert.notEqual(former.width, itemGeometry(frameworkCss + "\n" + designCss, true).width);
});

test("the narrow cascade honors state specificity, important declarations and later equal-specificity rules", () => {
  const base = `${menuItemSelector}{width:100%;justify-self:stretch}`;
  const hover = ".f-button:hover{width:max-content;justify-self:start}";
  assert.equal(itemGeometry(base + hover, true).width, "100%");
  assert.equal(itemGeometry(base + hover, true, { hover: true }).width, "max-content");
  assert.equal(itemGeometry(hover + base, true, { hover: true }).width, "100%");
  assert.equal(itemGeometry(base + ".f-button{width:max-content!important}", true).width, "max-content");
});
