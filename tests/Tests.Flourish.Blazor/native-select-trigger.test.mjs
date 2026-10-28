import assert from "node:assert/strict";
import { readFile } from "node:fs/promises";
import { dirname, resolve } from "node:path";
import { fileURLToPath } from "node:url";
import test from "node:test";
const root = resolve(dirname(fileURLToPath(import.meta.url)), "../..");
const design = "src/Flourish.Blazor/Flourish.Blazor.Design/wwwroot/";
const read = path => readFile(resolve(root, path), "utf8");
const strip = css => css.replace(/\/\*[\s\S]*?\*\//g, "");
function arms(text) {
  const result = []; let depth = 0, start = 0;
  for (let index = 0; index < text.length; index++) {
    if ("([".includes(text[index])) depth++;
    if (")]".includes(text[index])) depth--;
    if (text[index] === "," && !depth) { result.push(text.slice(start, index).trim()); start = index + 1; }
  }
  result.push(text.slice(start).trim()); return result;
}
const add = (left, right) => left.map((value, index) => value + right[index]);
const compare = (left, right) => left.map((value, index) => value - right[index]).find(value => value !== 0) ?? 0;
const maximum = values => values.reduce((left, right) => compare(left, right) >= 0 ? left : right, [0, 0, 0]);
function compound(selector, element) {
  let matches = true, specificity = [0, 0, 0];
  while (selector) {
    let match;
    if ((match = /^\.([\w-]+)/.exec(selector))) { specificity[1]++; matches &&= element.classes.has(match[1]); }
    else if ((match = /^\[([\w-]+)(?:\s*=\s*(?:"([^"]*)"|'([^']*)'|([^\]\s]+))\s*([is])?\s*)?\]/.exec(selector))) {
      const expected = match[2] ?? match[3] ?? match[4], actual = element.attributes.get(match[1]);
      const equal = expected === undefined || (match[5] === "i" ? actual?.toLowerCase() === expected.toLowerCase() : actual === expected);
      specificity[1]++; matches &&= element.attributes.has(match[1]) && equal;
    } else if ((match = /^:([\w-]+)\(/.exec(selector))) {
      let end = match[0].length, depth = 1;
      for (; end < selector.length && depth; end++) { if (selector[end] === "(") depth++; if (selector[end] === ")") depth--; }
      const argument = selector.slice(match[0].length, end - 1), name = match[1];
      if (["is", "not", "where"].includes(name)) {
        const choices = arms(argument).map(arm => complex(arm, element));
        specificity = add(specificity, name === "where" ? [0, 0, 0] : maximum(choices.map(choice => choice.specificity)));
        matches &&= name === "not" ? choices.every(choice => !choice.matches) : choices.some(choice => choice.matches);
      } else if (name === "has") {
        const childSelector = argument.replace(/^>\s*/, "");
        const choices = element.children.map(child => compound(childSelector, child));
        specificity = add(specificity, compound(childSelector, element).specificity);
        matches &&= choices.some(choice => choice.matches);
      } else return { matches: false, specificity };
      selector = selector.slice(end); continue;
    } else if ((match = /^:([\w-]+)/.exec(selector))) { specificity[1]++; matches &&= element.states.has(match[1]); }
    else if ((match = /^(\*|[a-z][\w-]*)/.exec(selector))) { if (match[1] !== "*") { specificity[2]++; matches &&= element.tag === match[1]; } }
    else return { matches: false, specificity };
    selector = selector.slice(match[0].length);
  }
  return { matches, specificity };
}
function complex(selector, element) {
  // Bounded CSS cascade fixture: actual class/tag/attribute selectors, child/ancestor
  // combinators and :is/:not/:has. It does not claim native browser rendering coverage.
  selector = selector.replace(/\s*>\s*/g, ">");
  let depth = 0, split = -1;
  for (let index = 0; index < selector.length; index++) {
    if ("([".includes(selector[index])) depth++;
    if (")]".includes(selector[index])) depth--;
    if (!depth && (selector[index] === ">" || selector[index] === " ")) split = index;
  }
  if (split < 0) return compound(selector, element);
  const right = compound(selector.slice(split + 1), element), left = selector.slice(0, split).trim();
  let ancestor = element.parent, leftResult;
  while (ancestor) {
    const current = complex(left, ancestor);
    leftResult ??= current;
    if (current.matches) { leftResult = current; break; }
    if (selector[split] === ">") break;
    ancestor = ancestor.parent;
  }
  // Specificity is independent of whether this element actually has that ancestor.
  leftResult ??= complex(left, { tag: "", classes: new Set(), attributes: new Map(), states: new Set(), children: [], parent: null });
  return { matches: right.matches && !!ancestor && leftResult.matches, specificity: add(leftResult.specificity, right.specificity) };
}
function rules(css) {
  return [...strip(css).matchAll(/([^{}]+)\{([^{}]*)\}/g)].map(match => ({
    selectors: arms(match[1].trim()), declarations: new Map(match[2].split(";").map(value => { const index = value.indexOf(":"); return [value.slice(0, index).trim(), value.slice(index + 1).trim()]; }))
  }));
}
function cascade(styles, element, property) {
  let winner;
  for (const rule of styles) for (const selector of rule.selectors) {
    const raw = rule.declarations.get(property); if (raw === undefined) continue;
    const match = complex(selector, element); if (!match.matches) continue;
    const important = /!important\s*$/.test(raw) ? 1 : 0;
    if (winner && (important < winner.important || important === winner.important && compare(match.specificity, winner.specificity) < 0)) continue;
    winner = { specificity: match.specificity, important, value: raw.replace(/\s*!important\s*$/, "") };
  }
  return winner?.value;
}
function element(tag, classes, parent, states = [], attributes = []) {
  const value = { tag, classes: new Set(classes), states: new Set(states), attributes: new Map(attributes), children: [], parent };
  parent?.children.push(value); return value;
}
const variants = ["primary", "secondary", "quiet", "danger", "underline", "elevated"];
const contexts = [null, "shell-header", "f-titlebar", "f-access-form-primary", "f-presentation-footer"];
const scenarios = [[], ["hover"], ["open"], ["active"], ["hover", "active"], ["focus-visible"], ["hover", "focus-visible"]];
const properties = ["color", "background", "border-color", "border-bottom-color", "box-shadow", "outline", "outline-offset", "--f-target-preview", "--f-target-click"];
const styles = rules((await Promise.all(["foundation.css", "controls.css", "static-surfaces.css", "form-inputs.css"].map(path => read(design + path)))).join("\n"));
for (const variant of variants) test(`native ${variant} trigger shares Button states while only its select owns interaction`, () => {
  for (const context of contexts) for (const states of scenarios) {
    const rootElement = element("div", ["f-root"], null);
    const parent = context ? element("div", [context], rootElement) : rootElement;
    const owner = element("select", ["f-input", "f-select", "f-select-customized"], parent, states);
    const native = element("button", ["f-select-trigger", "f-button", `f-button-${variant}`], owner);
    const ordinary = element("button", ["f-button", `f-button-${variant}`], parent, states.map(state => state === "open" ? "hover" : state));
    assert.equal(native.states.size, 0, "The fixture must leave the native child inert.");
    for (const property of properties) assert.equal(cascade(styles, native, property), cascade(styles, ordinary, property), `${context ?? "body"}/${states}/${property}`);
  }
});
test("native owner disabled and parent-only aria-disabled defeat every variant and chrome branch", () => {
  for (const context of contexts) for (const variant of variants) for (const unavailable of ["disabled", "aria-disabled"]) for (const state of [[], ["hover"], ["open"], ["active"], ["hover", "active"]]) {
    const rootElement = element("div", ["f-root"], null), parent = context ? element("div", [context], rootElement) : rootElement;
    const states = [...state, ...(unavailable === "disabled" ? ["disabled"] : [])], attributes = unavailable === "aria-disabled" ? [["aria-disabled", "true"]] : [];
    const owner = element("select", ["f-input", "f-select", "f-select-customized"], parent, states, attributes);
    const native = element("button", ["f-select-trigger", "f-button", `f-button-${variant}`], owner, unavailable === "disabled" ? ["disabled"] : []);
    const ordinary = element("button", ["f-button", `f-button-${variant}`], parent, states.map(value => value === "open" ? "hover" : value), attributes);
    assert.equal(cascade(styles, native, "color"), "var(--f-disabled)", `${context}/${variant}/${unavailable}/${state} must remain disabled.`);
    for (const property of properties.filter(value => !value.startsWith("--"))) assert.equal(cascade(styles, native, property), cascade(styles, ordinary, property), `${context}/${variant}/${unavailable}/${state}/${property}`);
  }
});
test("native picker and service menus share canonical surface and option roles without making selected options disabled", async () => {
  const css = await read(design + "dropdown.css"), styleRules = rules(css);
  const picker = styleRules.find(rule => rule.selectors.includes(".f-select::picker(select)"));
  const menu = styleRules.find(rule => rule.selectors.includes(".service-menu-view"));
  for (const property of ["padding", "border", "border-radius", "color", "background", "box-shadow", "--f-target-preview", "--f-target-click"]) assert.equal(picker.declarations.get(property), menu.declarations.get(property), property);
  const option = styleRules.find(rule => rule.selectors.includes(".f-select option") && rule.declarations.has("border-radius"));
  for (const [property, expected] of [["border-radius", "12px"], ["background", "transparent"], ["color", "var(--f-text)"]]) assert.equal(option.declarations.get(property), expected);
  assert.doesNotMatch(css, /option:checked[^{}]*\{[^{}]*(?:disabled|opacity\s*:|cursor\s*:\s*not-allowed)/);
  const navigation = await read(design + "patterns/navigation-surface.css");
  for (const rule of rules(navigation)) for (const selector of rule.selectors.filter(value => /select.*(?:::picker|option|optgroup)/.test(value))) assert.match(selector, /select:not\(\.f-select\)/);
});

async function expandCss(path) {
  const css = await readFile(path, "utf8");
  let result = "", offset = 0;
  for (const match of css.matchAll(/@import\s+(?:url\(['"]([^'"]+)['"]\)|['"]([^'"]+)['"])\s*;/g)) {
    result += css.slice(offset, match.index) + await expandCss(resolve(dirname(path), match[1] ?? match[2]));
    offset = match.index + match[0].length;
  }
  return result + css.slice(offset);
}
const frameworkCss = await expandCss(resolve(root, "src/Flourish.Blazor/Flourish.Blazor.Framework/wwwroot/framework.css"));
const designCss = await expandCss(resolve(root, design + "design.css"));
function popupFixture(kind, alignment) {
  const rootElement = element("div", ["f-root"], null);
  const attributes = alignment ? [["data-f-popup-align", alignment]] : [];
  let panel, item;
  if (kind === "reference") {
    const surface = element("div", ["selection-dropdown-surface"], rootElement);
    const owner = element("div", ["reference-dropdown"], surface);
    panel = element("div", ["selection-panel", "reference-panel"], owner, [], attributes);
    item = element("button", ["selection-option", "reference-option"], element("div", ["reference-options"], panel));
  } else if (kind === "search") {
    const owner = element("span", ["search-autocomplete"], rootElement);
    panel = element("span", ["search-autocomplete-options"], owner, [], attributes);
    item = element("span", ["search-autocomplete-option"], panel);
  } else {
    const owner = element("details", [kind === "split" ? "f-split-button-menu" : kind === "dropdown" ? "f-dropdown-surface" : kind === "service" ? "service-menu" : "f-action-menu"], rootElement);
    panel = element("div", ["f-menu-panel", ...(kind === "service" ? ["service-menu-view"] : ["f-dropdown-panel"]), ...(kind === "multi" ? ["f-multi-select-panel"] : [])], owner, [], attributes);
    if (kind === "service") item = element("a", [], element("nav", [], panel));
    else if (kind === "multi") item = element("label", ["f-dropdown-item"], element("div", ["f-multi-select-option"], panel));
    else if (kind === "split") item = element("a", ["f-button", "f-button-quiet"], element("div", ["f-split-button-menu-content"], panel));
    else item = element("button", kind === "action" ? ["f-menu-item", "f-dropdown-item"] : ["f-button", "f-button-quiet"], panel);
  }
  return { rootElement, panel, item };
}
test("unframed column menus keep closed, native-open and enhanced visibility gates without a shared surface", () => {
  for (const css of [frameworkCss, frameworkCss + "\n" + designCss]) {
    const allStyles = rules(css), { panel } = popupFixture("action", "right"), owner = panel.parent;
    owner.attributes.set("data-f-native-menu", "");
    panel.classes.add("f-menu-panel-unframed"); panel.classes.add("f-menu-panel-columns");
    assert.equal(cascade(allStyles, panel, "display"), "none", "A configured grid must remain closed.");
    assert.equal(cascade(allStyles, panel, "grid-template-columns"), "repeat(var(--f-menu-panel-columns),minmax(0,1fr))");
    assert.equal(cascade(allStyles, panel, "gap"), "12px");
    assert.equal(cascade(allStyles, panel, "max-width"), "calc(100vw - 24px)");
    owner.attributes.set("open", "");
    assert.equal(cascade(allStyles, panel, "display"), "grid", "Native SSR opening must show all configured columns.");
    owner.attributes.delete("open"); owner.attributes.set("data-f-enhanced", "true"); panel.attributes.set("popover", "manual");
    assert.equal(cascade(allStyles, panel, "display"), "none");
    panel.states.add("popover-open");
    assert.equal(cascade(allStyles, panel, "display"), "grid", "The interactive top layer retains grid layout.");
    panel.states.delete("popover-open"); panel.attributes.set("data-f-open", "");
    assert.equal(cascade(allStyles, panel, "display"), "grid");
    panel.attributes.set("hidden", "");
    assert.equal(cascade(allStyles, panel, "display"), "none", "Native hidden remains stronger than open grid state.");
  }
  const allStyles = rules(frameworkCss + "\n" + designCss), { panel } = popupFixture("action", "left");
  panel.classes.add("f-menu-panel-unframed");
  for (const [property, expected] of [["padding", "0"], ["border", "0"], ["border-radius", "0"], ["background", "transparent"], ["box-shadow", "none"], ["overflow", "auto"]])
    assert.equal(cascade(allStyles, panel, property), expected, property);
  const ordinary = popupFixture("action", "left").panel;
  assert.equal(cascade(allStyles, ordinary, "padding"), "6px");
  assert.equal(cascade(allStyles, ordinary, "background"), "var(--f-surface)");
  assert.equal(cascade(allStyles, ordinary, "border-radius"), "16px");
});

test("direct accessible menu tiles retain standalone size, centering, rounding and individual elevation", () => {
  const allStyles = rules(frameworkCss + "\n" + designCss), { panel } = popupFixture("action", "left");
  panel.classes.add("f-menu-panel-columns"); panel.classes.add("f-menu-panel-unframed");
  const tile = element("a", ["f-button", "f-button-primary", "f-menu-item", "f-dropdown-item", "f-uniform-cell", "f-uniform-grid-button", "f-uniform-variant-elevated"], panel, [], [["role", "menuitem"]]);
  for (const [property, expected] of [["min-height", "110px"], ["padding", "16px"], ["border-radius", "var(--f-radius-control,12px)"], ["box-shadow", "var(--f-uniform-cell-shadow,var(--f-shadow-control))"], ["text-align", "center"], ["justify-content", "center"], ["width", "100%"]])
    assert.equal(cascade(allStyles, tile, property), expected, property);
  assert.equal(cascade(allStyles, tile, "max-height"), undefined, "Long tile labels can grow instead of inheriting a grouped grid cap.");
  tile.states.add("focus-visible");
  assert.equal(cascade(allStyles, tile, "outline"), "3px solid var(--f-accent)");
  tile.attributes.set("aria-disabled", "true");
  assert.equal(cascade(allStyles, tile, "cursor"), "not-allowed");
});

test("information grid fields grow naturally beyond shape and fixed-height caps while ordinary action geometry stays intact", () => {
  for (const css of [frameworkCss, frameworkCss + "\n" + designCss]) for (const shape of ["rectangle", "square"]) for (const fixed of [false, true]) {
    if (shape === "square" && fixed) continue;
    const allStyles = rules(css), grid = element("div", ["f-uniform-grid", `f-uniform-grid-${shape}`, "f-uniform-grid-rows", ...(fixed ? ["f-uniform-grid-fixed-height"] : [])], null);
    const info = element("div", ["f-uniform-cell", "f-uniform-grid-item", "f-uniform-information"], grid);
    const action = element("button", ["f-uniform-cell", "f-uniform-grid-button"], grid);
    assert.equal(cascade(allStyles, grid, "grid-auto-rows"), "minmax(min-content,auto)");
    assert.equal(cascade(allStyles, grid, "grid-template-rows"), "repeat(var(--f-grid-rows),minmax(min-content,auto))");
    for (const [property, expected] of [["aspect-ratio", "auto"], ["height", "auto"], ["max-height", "none"], ["overflow", "visible"], ["text-align", "start"]])
      assert.equal(cascade(allStyles, info, property), expected, `${shape}/${fixed}/${property}`);
    assert.equal(cascade(allStyles, action, "aspect-ratio"), fixed ? "auto" : shape === "square" ? "1" : "2/1");
    assert.equal(cascade(allStyles, action, "max-height"), fixed ? "var(--f-grid-cell-height)" : shape === "square" ? "var(--f-grid-cell-max,280px)" : "var(--f-grid-cell-max-height,260px)");
  }
});

test("information labels use body type and selectable values use H1 size without changing dialog-body or ordinary grid typography", () => {
  const allStyles = rules(frameworkCss + "\n" + designCss), grid = element("div", ["f-uniform-grid"], null);
  const cell = element("div", ["f-uniform-cell", "f-uniform-information"], grid), pair = element("dl", ["f-uniform-information-pair"], cell);
  const label = element("dt", ["f-uniform-information-label"], pair), value = element("dd", ["f-uniform-information-value"], pair);
  const copy = element("code", ["f-copy-text"], value);
  assert.equal(cascade(allStyles, label, "font-size"), "var(--f-type-body,17px)");
  assert.equal(cascade(allStyles, label, "font-weight"), "400");
  assert.equal(cascade(allStyles, value, "font-size"), "var(--f-type-h1,34px)");
  assert.equal(cascade(allStyles, value, "overflow-wrap"), "anywhere");
  assert.equal(cascade(allStyles, copy, "font"), "inherit");
  const dialog = element("dialog", ["f-dialog"], null), header = element("header", ["f-dialog-header"], dialog);
  const title = element("h2", ["f-dialog-title-danger"], header), body = element("div", ["f-dialog-body"], dialog);
  assert.equal(cascade(allStyles, title, "color"), "var(--f-danger)");
  assert.equal(cascade(allStyles, body, "color"), undefined);
});

test("popup placement attributes do not define a competing text-alignment token", () => {
  assert.doesNotMatch(strip(frameworkCss + "\n" + designCss), /--f-popup-align/);
});

test("all anchored panels retain intrinsic width with one viewport bound in the complete optional Design cascade", () => {
  for (const css of [frameworkCss, frameworkCss + "\n" + designCss]) {
    const allStyles = rules(css);
    for (const kind of ["service", "action", "dropdown", "split", "multi", "reference", "search"]) for (const alignment of [null, "left", "right"]) {
      const { panel, item } = popupFixture(kind, alignment);
      for (const [property, expected] of [["width", "max-content"], ["min-width", "0"], ["max-width", "calc(100vw - 24px)"]])
        assert.equal(cascade(allStyles, panel, property), expected, `${kind}/${alignment}/panel ${property}`);
      for (const [property, expected] of [["width", kind === "multi" ? "auto" : "100%"], ["min-width", "0"], ["max-width", kind === "multi" ? "none" : "100%"], ["box-sizing", "border-box"], ["justify-self", "stretch"], ["align-self", "stretch"]])
        assert.equal(cascade(allStyles, item, property), expected, `${kind}/${alignment}/item ${property}`);
      for (const property of ["text-align", "justify-content"])
        assert.equal(cascade(allStyles, item, property), kind === "multi" && property === "justify-content" ? "stretch" : "left", `${kind}/${alignment}/${property}`);
    }
  }
});
test("popup completion Buttons share row alignment while ordinary page Buttons retain content width and centering", () => {
  const allStyles = rules(frameworkCss + "\n" + designCss);
  for (const kind of ["service", "action", "dropdown", "multi", "reference", "search"]) for (const alignment of ["left", "right"]) {
    const { rootElement, panel } = popupFixture(kind, alignment);
    const completion = element("button", ["f-button", "f-button-quiet"], panel);
    assert.equal(cascade(allStyles, completion, "width"), "100%", kind);
    for (const property of ["text-align", "justify-content"]) assert.equal(cascade(allStyles, completion, property), "left", kind);
    const ordinary = element("button", ["f-button", "f-button-quiet"], rootElement);
    assert.equal(cascade(allStyles, ordinary, "width"), "max-content");
    assert.equal(cascade(allStyles, ordinary, "justify-content"), "center");
  }
});

test("structured dropdown actions use two body-sized lines without enlarging ordinary Button descriptions", () => {
  const allStyles = rules(frameworkCss + "\n" + designCss), { rootElement, panel } = popupFixture("action", "left");
  const action = element("button", ["f-button", "f-button-quiet", "f-button-structured", "f-menu-item", "f-dropdown-item"], panel, [], [["role", "menuitem"]]);
  const label = element("span", ["f-button-label"], action), title = element("span", ["f-button-text"], label), description = element("span", ["f-button-description"], label);
  for (const [property, expected] of [["padding", "10px 16px"], ["min-height", "72px"], ["gap", "16px"], ["box-sizing", "border-box"]])
    assert.equal(cascade(allStyles, action, property), expected, property);
  for (const states of [[], ["hover"], ["focus-visible"], ["active"]]) {
    action.states = new Set(states);
    assert.equal(cascade(allStyles, action, "color"), "var(--f-text)", states.join("/"));
    for (const [line, weight] of [[title, "700"], [description, "400"]]) {
      assert.equal(cascade(allStyles, line, "font-size"), "17px");
      assert.equal(cascade(allStyles, line, "font-weight"), weight);
      assert.equal(cascade(allStyles, line, "line-height"), "1.55");
      assert.equal(cascade(allStyles, line, "color"), "inherit");
    }
  }
  const ordinary = element("button", ["f-button", "f-button-quiet", "f-button-structured"], rootElement);
  const ordinaryDescription = element("span", ["f-button-description"], element("span", ["f-button-label"], ordinary));
  assert.equal(cascade(allStyles, ordinaryDescription, "font-size"), ".875em");
  assert.equal(cascade(allStyles, ordinaryDescription, "font-weight"), "400");
});

test("hover language and native select choices retain body weight including the selected choice", () => {
  const allStyles = rules(frameworkCss + "\n" + designCss), { panel } = popupFixture("action", "left");
  for (const checked of [false, true]) {
    const choice = element("button", ["f-native-select-option", "f-dropdown-item"], panel, [], [["role", "menuitemradio"], ["aria-checked", String(checked)]]);
    assert.equal(cascade(allStyles, choice, "font-size"), "17px");
    assert.equal(cascade(allStyles, choice, "font-weight"), "400");
    assert.equal(cascade(allStyles, choice, "line-height"), "1.55");
    assert.equal(cascade(allStyles, choice, "padding"), "6px 10px");
    assert.equal(cascade(allStyles, choice, "min-height"), "40px");
    assert.equal(cascade(allStyles, choice, "gap"), "12px");
    const marker = element("span", ["f-select-checkmark"], choice);
    assert.equal(cascade(allStyles, marker, "background"), checked ? "var(--f-primary)" : "var(--f-surface)");
    const native = element("select", ["f-select"], null), option = element("option", [], native, checked ? ["checked"] : []);
    assert.equal(cascade(allStyles, option, "font-weight"), "400");
  }
});

test("header Quiet branding buttons disclosure summaries and native select triggers share feedback roles", () => {
  const allStyles = rules(frameworkCss + "\n" + designCss);
  for (const theme of ["f-theme-light", "f-theme-dark"]) for (const states of scenarios) for (const unavailable of [false, true]) {
    const rootElement = element("div", ["f-root", theme], null), header = element("header", ["shell-header"], rootElement);
    const attributes = [["aria-disabled", String(unavailable)]], ordinaryStates = states.map(state => state === "open" ? "hover" : state);
    const brand = element("a", ["shell-header-brand", "f-button", "f-button-quiet"], header, ordinaryStates, attributes);
    const ordinary = element("button", ["f-button", "f-button-quiet"], header, ordinaryStates, attributes);
    const disclosure = element("summary", ["f-disclosure-button-trigger", "f-button", "f-button-quiet"], element("details", ["f-action-menu"], header), ordinaryStates, attributes);
    const select = element("select", ["f-input", "f-select", "f-select-customized"], header, states, attributes);
    const trigger = element("button", ["f-select-trigger", "f-button", "f-button-quiet"], select, [], attributes);
    for (const actual of [ordinary, disclosure, trigger]) for (const property of ["color", "background", "outline", "outline-offset", "--f-target-preview", "--f-target-click", "--f-accent-text"])
      assert.equal(cascade(allStyles, actual, property), cascade(allStyles, brand, property), `${theme}/${states}/${unavailable}/${actual.tag}/${property}`);
    for (const property of ["--f-target-preview", "--f-target-click"])
      assert.equal(cascade(allStyles, select, property), cascade(allStyles, brand, property), `native carrier must not restore a different surface role: ${property}`);

    for (const excluded of [["f-menu-panel", "f-dropdown-panel"], ["f-dialog"]]) {
      const panel = element("div", excluded, header), action = element("button", ["f-button", "f-button-quiet"], panel, ["hover"]);
      assert.notEqual(cascade(allStyles, action, "--f-accent-text"), cascade(allStyles, brand, "--f-accent-text"), "header white ink must not enter panels or dialogs");
      assert.notEqual(cascade(allStyles, action, "--f-target-preview"), cascade(allStyles, brand, "--f-target-preview"), "header feedback roles must remain on actual chrome controls");
    }
  }
});
test("circuit-owned popover panels remain hidden until the browser opens their existing grid", () => {
  for (const css of [frameworkCss, frameworkCss + "\n" + designCss]) for (const kind of ["reference", "search"]) {
    const allStyles = rules(css), { panel } = popupFixture(kind, "right");
    panel.attributes.set("popover", "manual");
    assert.equal(cascade(allStyles, panel, "display"), "none", kind);
    panel.states.add("popover-open");
    assert.equal(cascade(allStyles, panel, "display"), "grid", kind);
  }
});
test("native options remain left aligned on either physical owner side without changing the trigger Button", () => {
  const allStyles = rules(frameworkCss + "\n" + designCss);
  for (const alignment of [null, "left", "right"]) {
    const owner = element("select", ["f-select", "f-select-customized"], null, [], alignment ? [["data-f-popup-align", alignment]] : []);
    const option = element("option", [], owner);
    assert.equal(cascade(allStyles, option, "width"), "auto");
    assert.equal(cascade(allStyles, option, "max-width"), "none");
    for (const property of ["text-align", "justify-content"]) assert.equal(cascade(allStyles, option, property), "left");
    const trigger = element("button", ["f-select-trigger", "f-button", "f-button-quiet"], owner);
    assert.equal(cascade(allStyles, trigger, "justify-content"), "center");
  }
});
test("popup search inputs fill the option width without imposing the browser's default 20-character minimum", () => {
  for (const css of [frameworkCss, frameworkCss + "\n" + designCss]) for (const kind of ["reference", "multi"]) {
    const allStyles = rules(css), { panel } = popupFixture(kind, "right");
    const search = element("input", kind === "reference" ? ["selection-search", "reference-search"] : ["f-input", "f-multi-select-search"], panel);
    for (const [property, expected] of [["width", "0"], ["min-width", "100%"], ["max-width", "100%"], ["box-sizing", "border-box"]])
      assert.equal(cascade(allStyles, search, property), expected, `${kind}/${property}`);
  }
  const allStyles = rules(frameworkCss + "\n" + designCss), owner = element("span", ["search-autocomplete"], null);
  const trigger = element("input", ["f-input"], owner);
  assert.equal(cascade(allStyles, trigger, "width"), "100%", "The autocomplete trigger remains sized by its form.");
});


test("native picker sizing uses the pre-opening measurement in Framework with optional Design paint", () => {
  for (const css of [frameworkCss, frameworkCss + "\n" + designCss]) {
    const picker = new Map(rules(css).filter(rule => rule.selectors.includes(".f-select::picker(select)")).flatMap(rule => [...rule.declarations]));
    for (const [property, expected] of [["box-sizing", "border-box"], ["width", "var(--f-picker-inline-size,max-content)"], ["min-width", "0"], ["max-width", "calc(100vw - 24px)"], ["min-inline-size", "0"], ["max-inline-size", "calc(100vw - 24px)"]])
      assert.equal(picker.get(property), expected, property);
  }
});

test("native options reserve the same square choice column for every label and selection", () => {
  for (const css of [frameworkCss, frameworkCss + "\n" + designCss]) for (const alignment of ["left", "right"]) {
    const allStyles = rules(css);
    const owner = element("select", ["f-select", "f-select-customized"], null, [], [["data-f-popup-align", alignment]]);
    const option = element("option", [], owner);
    for (const [property, expected] of [["width", "auto"], ["max-width", "none"], ["min-width", "0"], ["display", "grid"], ["grid-template-columns", "var(--f-choice-size,28px) minmax(0,1fr)"], ["column-gap", "10px"], ["white-space", "normal"], ["overflow-wrap", "anywhere"]])
      assert.equal(cascade(allStyles, option, property), expected, alignment + "/" + property);
    assert.equal(cascade(allStyles, option, "padding-left"), undefined, "The marker column replaces extra padding rather than adding a second reservation.");
    assert.equal(cascade(allStyles, option, "text-align"), "left");
    assert.equal(cascade(allStyles, option, "min-height"), "calc(var(--f-choice-size,28px) + 12px)");
    assert.equal(cascade(allStyles, option, "padding"), "6px 10px");
    const marker = new Map(allStyles.filter(rule => rule.selectors.includes(".f-select option::checkmark")).flatMap(rule => [...rule.declarations]));
    for (const [property, expected] of [["box-sizing", "border-box"], ["display", "grid"], ["grid-column", "1"], ["grid-row", "1"], ["place-content", "center"], ["width", "var(--f-choice-size,28px)"], ["height", "var(--f-choice-size,28px)"], ["padding", "0"], ["margin", "0"], ["text-align", "center"], ["visibility", "visible"], ["content", '""']])
      assert.equal(marker.get(property), expected, property);
    for (const property of ["position", "left", "top", "transform"]) assert.equal(marker.get(property), undefined, property + " must not remove the native marker from its grid column.");
    const selected = new Map(allStyles.filter(rule => rule.selectors.includes(".f-select option:checked::checkmark")).flatMap(rule => [...rule.declarations]));
    assert.equal(selected.get("content"), '"✓"');
    for (const property of ["width", "height", "display", "grid-column", "grid-row"]) assert.equal(selected.get(property), undefined, "Selecting a row must preserve " + property + ".");
  }
});

test("native choice markers reuse Checkbox geometry, paint and check glyph", () => {
  const allStyles = rules(frameworkCss + "\n" + designCss);
  const marker = new Map(allStyles.filter(rule => rule.selectors.includes(".f-select option::checkmark")).flatMap(rule => [...rule.declarations]));
  const selectedMarker = new Map(allStyles.filter(rule => rule.selectors.includes(".f-select option:checked::checkmark")).flatMap(rule => [...rule.declarations]));
  const checkbox = element("input", ["f-checkbox"], null);
  for (const property of ["box-sizing", "display", "width", "height", "padding", "margin", "place-content", "border", "border-radius", "color", "background"])
    assert.equal(marker.get(property), cascade(allStyles, checkbox, property), property);
  checkbox.states.add("checked");
  for (const property of ["background", "border-color"]) assert.equal(selectedMarker.get(property), cascade(allStyles, checkbox, property), property);
  const standardCheck = new Map(allStyles.filter(rule => rule.selectors.includes(".f-checkbox:checked::before")).flatMap(rule => [...rule.declarations]));
  assert.equal(selectedMarker.get("content"), standardCheck.get("content"));
  assert.equal(marker.get("font"), standardCheck.get("font"));
});

test("checkbox choice labels have separate marker and longest-label tracks in both physical alignments", () => {
  for (const css of [frameworkCss, frameworkCss + "\n" + designCss]) for (const kind of ["multi"]) for (const alignment of ["left", "right"]) {
    const allStyles = rules(css), { panel, item } = popupFixture(kind, alignment);
    const label = item;
    const checkbox = element("input", [], label, [], [["type", "checkbox"]]);
    const text = element("span", [], label);
    assert.equal(cascade(allStyles, label, "display"), "grid", kind);
    assert.equal(cascade(allStyles, label, "grid-template-columns"), "var(--f-choice-size,28px) minmax(0,1fr)");
    assert.equal(cascade(allStyles, label, "column-gap"), "10px");
    assert.equal(cascade(allStyles, label, "justify-content"), "stretch");
    assert.equal(cascade(allStyles, label, "width"), "auto");
    assert.equal(cascade(allStyles, checkbox, "grid-column"), "1");
    assert.equal(cascade(allStyles, checkbox, "grid-row"), "1");
    assert.equal(cascade(allStyles, checkbox, "justify-self"), "center");
    assert.equal(cascade(allStyles, text, "grid-column"), "2");
    assert.equal(cascade(allStyles, text, "grid-row"), "1");
    assert.equal(cascade(allStyles, text, "min-width"), "0");
    assert.equal(cascade(allStyles, text, "text-align"), "left");
    for (const selected of [false, true]) {
      checkbox.states = new Set(selected ? ["checked"] : []);
      assert.equal(cascade(allStyles, checkbox, "grid-column"), "1", kind + "/" + selected);
      assert.equal(cascade(allStyles, label, "grid-template-columns"), "var(--f-choice-size,28px) minmax(0,1fr)");
    }
  }
});

test("compact choice markers are independent of form heights while labelled checkboxes retain their hit area", () => {
  const allStyles = rules(frameworkCss + "\n" + designCss), surface = element("div", ["f-root"], null);
  assert.equal(cascade(allStyles, surface, "--f-choice-size"), "28px");
  assert.equal(cascade(allStyles, surface, "--f-choice-radius"), "9px");
  assert.equal(cascade(allStyles, surface, "--f-control-height"), "48px");
  const label = element("label", ["f-checkbox-label"], surface), checkbox = element("input", ["f-checkbox"], label);
  assert.equal(cascade(allStyles, label, "min-height"), "var(--f-control-height,48px)");
  for (const property of ["width", "height"]) assert.equal(cascade(allStyles, checkbox, property), "var(--f-choice-size,28px)");
  assert.equal(cascade(allStyles, checkbox, "border-radius"), "var(--f-choice-radius,9px)");
  const nativeLabel = element("label", ["form-check"], surface), nativeCheckbox = element("input", [], nativeLabel, [], [["type", "checkbox"]]);
  for (const property of ["width", "height"]) assert.equal(cascade(allStyles, nativeCheckbox, property), "var(--f-choice-size,28px)");
  assert.equal(cascade(allStyles, nativeCheckbox, "border-radius"), "var(--f-choice-radius,9px)");
  assert.equal(cascade(allStyles, nativeLabel, "min-height"), "var(--f-control-height)");
  const field = element("input", ["f-input"], surface), button = element("button", ["f-button", "f-button-primary"], surface);
  assert.equal(cascade(allStyles, field, "height"), "var(--f-control-height,48px)");
  assert.equal(cascade(allStyles, button, "min-height"), "48px");
  checkbox.states.add("disabled");
  assert.equal(cascade(allStyles, checkbox, "cursor"), "not-allowed");
  checkbox.states.add("focus-visible");
  assert.equal(cascade(allStyles, checkbox, "outline"), "3px solid var(--f-accent)");
  checkbox.attributes.set("required", ""); checkbox.attributes.set("aria-invalid", "true");
  assert.equal(cascade(allStyles, checkbox, "border-color"), "var(--f-danger)");
});

test("compact popup rows retain wrapping and intrinsic growth across text, checkbox and action choices", () => {
  const allStyles = rules(frameworkCss + "\n" + designCss);
  for (const kind of ["service", "action", "multi", "reference", "search"]) for (const alignment of ["left", "right"]) {
    const { item } = popupFixture(kind, alignment);
    assert.equal(cascade(allStyles, item, "min-height"), "40px", kind);
    assert.equal(cascade(allStyles, item, "padding"), "6px 10px", kind);
    assert.equal(cascade(allStyles, item, "border-radius"), "12px", kind);
    for (const property of ["height", "max-height"]) assert.equal(cascade(allStyles, item, property), undefined, kind + " must grow for multiline labels.");
    assert.notEqual(cascade(allStyles, item, "white-space"), "nowrap", kind);
    assert.equal(cascade(allStyles, item, "overflow-wrap"), "anywhere", kind);
  }
});

// Hidden is a native state, independently of the optional Design display paint.
// Use the same complete author cascade as the popup rows, not hidden-property mocks.
const ordinaryHiddenValues = ["", "hidden", "true", "false", "other", " until-found"];
const hiddenScopeClasses = ["f-root", "f-shell", "content-surface", "navigation-surface", "f-dialog", "f-menu-panel"];
function hiddenScope(className, parent = null, hiddenValue) {
  const attributes = [["open", ""]];
  if (className === "f-menu-panel") attributes.push(["popover", "manual"]);
  if (hiddenValue !== undefined) attributes.push(["hidden", hiddenValue]);
  return element("div", [className], parent, ["open", "popover-open"], attributes);
}

test("ordinary hidden hides each library surface itself and descendants after optional Design display paint", () => {
  for (const css of [frameworkCss, frameworkCss + "\n" + designCss]) {
    const allStyles = rules(css);
    for (const className of hiddenScopeClasses) for (const value of ordinaryHiddenValues) {
      const scope = hiddenScope(className, null, value);
      assert.equal(cascade(allStyles, scope, "display"), "none", className + "/root/" + JSON.stringify(value));
      const visibleScope = hiddenScope(className);
      // A top-layer popover retains DOM ancestry; its selected/open paint must not defeat hidden descendants.
      const popover = hiddenScope("f-menu-panel", visibleScope);
      for (const parent of [visibleScope, popover]) for (const control of ["f-notice", "f-loading", "f-button"]) {
        const child = element("div", [control], parent, [], [["hidden", value]]);
        assert.equal(cascade(allStyles, child, "display"), "none", className + "/" + control + "/" + JSON.stringify(value));
      }
    }
  }
});

test("standalone Notice and LoadingState honor every ordinary hidden value without a shell or theme root", () => {
  for (const css of [frameworkCss, frameworkCss + "\n" + designCss]) {
    const allStyles = rules(css);
    for (const control of ["f-notice", "f-loading"]) for (const value of ordinaryHiddenValues) {
      const target = element("div", [control], null, [], [["hidden", value]]);
      assert.equal(cascade(allStyles, target, "display"), "none", control + "/" + JSON.stringify(value));
    }
    const themed = rules(frameworkCss + "\n" + designCss);
    assert.equal(cascade(themed, element("div", ["f-notice"], null), "display"), "grid");
    assert.equal(cascade(themed, element("div", ["f-loading"], null), "display"), "flex");
  }
});

test("until-found remains outside the ordinary hidden guard including case-insensitive native keywords", () => {
  for (const css of [frameworkCss, frameworkCss + "\n" + designCss]) {
    const allStyles = rules(css);
    for (const value of ["until-found", "UNTIL-FOUND", "UnTiL-FoUnD"]) {
      for (const className of hiddenScopeClasses) {
        const scope = hiddenScope(className, null, value), visibleScope = hiddenScope(className);
        assert.equal(cascade(allStyles, scope, "display"), cascade(allStyles, visibleScope, "display"), className + "/root/" + value);
        for (const control of ["f-notice", "f-loading", "f-button"]) {
          const target = element("div", [control], visibleScope, [], [["hidden", value]]);
          const visible = element("div", [control], visibleScope);
          assert.equal(cascade(allStyles, target, "display"), cascade(allStyles, visible, "display"), className + "/" + control + "/" + value);
        }
      }
      for (const control of ["f-notice", "f-loading"]) {
        const target = element("div", [control], null, [], [["hidden", value]]);
        const visible = element("div", [control], null);
        assert.equal(cascade(allStyles, target, "display"), cascade(allStyles, visible, "display"), "standalone/" + control + "/" + value);
      }
    }
  }
});

test("native hidden correction preserves legacy toggle/dialog hiding and stays outside unrelated host content", () => {
  const allStyles = rules(frameworkCss + "\n" + designCss);
  for (const value of ordinaryHiddenValues) {
    const toggle = element("div", ["f-toggle-section-content"], null, [], [["hidden", value]]);
    assert.equal(cascade(allStyles, toggle, "display"), "none", "legacy toggle/" + JSON.stringify(value));
    const dialog = hiddenScope("f-dialog"), target = element("div", ["f-loading"], dialog, [], [["hidden", value]]);
    assert.equal(cascade(allStyles, target, "display"), "none", "legacy dialog/" + JSON.stringify(value));
    const host = element("div", ["host-only-content"], null, [], [["hidden", value]]);
    assert.equal(cascade(allStyles, host, "display"), undefined, "No new global HTML display policy.");
  }
});
