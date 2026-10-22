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
        const choices = arms(argument).map(arm => compound(arm, element));
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
  for (const [property, expected] of [["border-radius", "10px"], ["background", "transparent"], ["color", "var(--f-text)"]]) assert.equal(option.declarations.get(property), expected);
  assert.doesNotMatch(css, /option:checked[^{}]*\{[^{}]*(?:disabled|opacity\s*:|cursor\s*:\s*not-allowed)/);
  const navigation = await read(design + "patterns/navigation-surface.css");
  for (const rule of rules(navigation)) for (const selector of rule.selectors.filter(value => /select.*(?:::picker|option|optgroup)/.test(value))) assert.match(selector, /select:not\(\.f-select\)/);
});

async function expandCss(path) {
  const css = await readFile(path, "utf8");
  let result = "", offset = 0;
  for (const match of css.matchAll(/@import\s+url\(['"]([^'"]+)['"]\)\s*;/g)) {
    result += css.slice(offset, match.index) + await expandCss(resolve(dirname(path), match[1]));
    offset = match.index + match[0].length;
  }
  return result + css.slice(offset);
}
const frameworkCss = await expandCss(resolve(root, "src/Flourish.Blazor/Flourish.Blazor.Framework/wwwroot/framework.css"));
const designCss = await expandCss(resolve(root, design + "design.css"));
function popupAlignment(styles, target) {
  for (let node = target; node; node = node.parent) {
    const value = cascade(styles, node, "--f-popup-align");
    if (value !== undefined) return value;
  }
  return "left";
}
function popupValue(styles, target, property) {
  return cascade(styles, target, property)?.replace(/var\(--f-popup-align,\s*left\)/g, popupAlignment(styles, target));
}
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
        assert.equal(popupValue(allStyles, item, property), kind === "multi" && property === "justify-content" ? "stretch" : alignment ?? "left", `${kind}/${alignment}/${property}`);
    }
  }
});
test("popup completion Buttons share row alignment while ordinary page Buttons retain content width and centering", () => {
  const allStyles = rules(frameworkCss + "\n" + designCss);
  for (const kind of ["service", "action", "dropdown", "multi", "reference", "search"]) for (const alignment of ["left", "right"]) {
    const { rootElement, panel } = popupFixture(kind, alignment);
    const completion = element("button", ["f-button", "f-button-quiet"], panel);
    assert.equal(cascade(allStyles, completion, "width"), "100%", kind);
    for (const property of ["text-align", "justify-content"]) assert.equal(popupValue(allStyles, completion, property), alignment, kind);
    const ordinary = element("button", ["f-button", "f-button-quiet"], rootElement);
    assert.equal(cascade(allStyles, ordinary, "width"), "max-content");
    assert.equal(cascade(allStyles, ordinary, "justify-content"), "center");
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
test("native option alignment follows its physical owner side without changing the trigger Button", () => {
  const allStyles = rules(frameworkCss + "\n" + designCss);
  for (const alignment of [null, "left", "right"]) {
    const owner = element("select", ["f-select", "f-select-customized"], null, [], alignment ? [["data-f-popup-align", alignment]] : []);
    const option = element("option", [], owner);
    assert.equal(cascade(allStyles, option, "width"), "auto");
    assert.equal(cascade(allStyles, option, "max-width"), "none");
    for (const property of ["text-align", "justify-content"]) assert.equal(popupValue(allStyles, option, property), alignment ?? "left");
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


test("native intrinsic sizing excludes cyclic percentage rows and reserves a fixed checkmark column", () => {
  for (const css of [frameworkCss, frameworkCss + "\n" + designCss]) for (const alignment of ["left", "right"]) {
    const allStyles = rules(css);
    const owner = element("select", ["f-select", "f-select-customized"], null, [], [["data-f-popup-align", alignment]]);
    const option = element("option", [], owner);
    for (const [property, expected] of [["width", "auto"], ["max-width", "none"], ["min-width", "0"], ["position", "relative"], ["white-space", "normal"], ["overflow-wrap", "anywhere"]])
      assert.equal(cascade(allStyles, option, property), expected, alignment + "/" + property);
    assert.equal(cascade(allStyles, option, "padding-left"), "calc(10px + var(--f-control-height,48px) + 12px)");
    assert.equal(popupValue(allStyles, option, "text-align"), alignment);
    assert.equal(cascade(allStyles, option, "min-height"), "40px");
    assert.equal(cascade(allStyles, option, "padding"), "8px 10px");
    const marker = rules(css).filter(rule => rule.selectors.includes(".f-select option::checkmark"));
    assert.equal(marker.length, 1);
    for (const [property, expected] of [["position", "absolute"], ["left", "10px"], ["width", "var(--f-control-height,48px)"], ["margin", "0"], ["text-align", "center"]])
      assert.equal(marker[0].declarations.get(property), expected, property);
    // Selected visibility and label length cannot contribute to an out-of-flow marker's width.
    assert.equal(marker[0].declarations.get("content"), undefined, "Keep the native checkmark and selection semantics.");
  }
});

test("checkbox choice labels have separate marker and longest-label tracks in both physical alignments", () => {
  for (const css of [frameworkCss, frameworkCss + "\n" + designCss]) for (const kind of ["multi"]) for (const alignment of ["left", "right"]) {
    const allStyles = rules(css), { panel, item } = popupFixture(kind, alignment);
    const label = item;
    const checkbox = element("input", [], label, [], [["type", "checkbox"]]);
    const text = element("span", [], label);
    assert.equal(cascade(allStyles, label, "display"), "grid", kind);
    assert.equal(cascade(allStyles, label, "grid-template-columns"), "var(--f-control-height,48px) minmax(0,1fr)");
    assert.equal(cascade(allStyles, label, "column-gap"), "12px");
    assert.equal(cascade(allStyles, label, "justify-content"), "stretch");
    assert.equal(cascade(allStyles, label, "width"), "auto");
    assert.equal(cascade(allStyles, checkbox, "grid-column"), "1");
    assert.equal(cascade(allStyles, checkbox, "grid-row"), "1");
    assert.equal(cascade(allStyles, checkbox, "justify-self"), "center");
    assert.equal(cascade(allStyles, text, "grid-column"), "2");
    assert.equal(cascade(allStyles, text, "grid-row"), "1");
    assert.equal(cascade(allStyles, text, "min-width"), "0");
    assert.equal(popupValue(allStyles, text, "text-align"), alignment);
    for (const selected of [false, true]) {
      checkbox.states = new Set(selected ? ["checked"] : []);
      assert.equal(cascade(allStyles, checkbox, "grid-column"), "1", kind + "/" + selected);
      assert.equal(cascade(allStyles, label, "grid-template-columns"), "var(--f-control-height,48px) minmax(0,1fr)");
    }
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
