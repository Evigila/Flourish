import assert from "node:assert/strict";
import { readFile } from "node:fs/promises";
import { dirname, join, resolve } from "node:path";
import { fileURLToPath } from "node:url";
import test from "node:test";

const repository = resolve(dirname(fileURLToPath(import.meta.url)), "../..");
const frameworkRoot = join(repository, "src/Flourish.Blazor/Flourish.Blazor.Framework/wwwroot");
const designRoot = join(repository, "src/Flourish.Blazor/Flourish.Blazor.Design/wwwroot");
const [framework, design, presentation] = await Promise.all([
  importedStylesheet(join(frameworkRoot, "framework.css")),
  importedStylesheet(join(designRoot, "design.css")),
  readFile(join(designRoot, "presentation.css"), "utf8"),
]);
// Follow both production entry points, including all local imports in their
// declared order. Later imports and the entry's own rules must take part in the
// same cascade; testing selected files alone could miss a subsequent override.
const stylesheet = [...framework, ...design].flatMap(css => rules(css));

test("Primary access Quiet actions keep paired ink and distinct hover/press roles without recoloring unavailable actions", () => {
  for (const states of [[], ["hover"], ["focus-visible"], ["active"], ["hover", "active"]]) {
    const { surface } = accessFixture();
    for (const tag of ["button", "a"]) {
      const action = element(tag, ["f-button", "f-button-quiet"], surface, states, { "aria-disabled": "false" });
      assert.equal(cascade(action, "color"), "var(--f-primary-ink)", `${tag} available Quiet foreground: ${states}`);
      assert.equal(cascade(action, "background"), states.includes("active") ? "var(--f-primary-click)"
        : states.includes("hover") ? "var(--f-primary-preview)" : "transparent", `${tag} available Quiet background: ${states}`);
    }
    for (const availability of ["native", "aria"]) {
      const unavailable = element(availability === "native" ? "button" : "a", ["f-button", "f-button-quiet"], surface, states,
        availability === "native" ? { disabled: "" } : { "aria-disabled": "true" });
      assert.equal(cascade(unavailable, "color"), "var(--f-disabled)", `${availability} unavailable foreground: ${states}`);
      assert.equal(cascade(unavailable, "background"), "transparent", `${availability} unavailable background: ${states}`);
      assert.equal(cascade(unavailable, "box-shadow"), "none", `${availability} unavailable elevation: ${states}`);
      assert.equal(cascade(unavailable, "cursor"), "not-allowed", `${availability} unavailable cursor: ${states}`);
    }
  }
});

test("Primary access context preserves explicit Secondary Danger Elevated and Input skins", () => {
  const { surface, root } = accessFixture();
  const properties = ["color", "background", "border-color", "border-radius", "box-shadow", "cursor", "font-family"];
  for (const variant of ["secondary", "danger", "elevated"]) {
    for (const states of [[], ["hover"], ["focus-visible"], ["active"], ["hover", "active"]]) {
      for (const availability of ["available", "native", "aria"]) {
        const tag = availability === "aria" ? "a" : "button";
        const attributes = availability === "native" ? { disabled: "" } : { "aria-disabled": availability === "aria" ? "true" : "false" };
        const scoped = element(tag, ["f-button", `f-button-${variant}`], surface, states, attributes);
        const ordinary = element(tag, ["f-button", `f-button-${variant}`], root, states, attributes);
        assert.notEqual(cascade(ordinary, "color"), undefined, `${variant} fixture did not find its production foreground`);
        assert.notEqual(cascade(ordinary, "background"), undefined, `${variant} fixture did not find its production fill`);
        for (const property of properties)
          assert.equal(cascade(scoped, property), cascade(ordinary, property), `${variant}/${availability}/${states}: ${property} was reskinned`);
      }
    }
  }
  const field = element("div", ["f-field"], surface);
  const control = element("div", ["f-field-control"], field);
  for (const states of [[], ["focus-visible"], ["disabled"]]) {
    const attributes = states.includes("disabled") ? { disabled: "" } : {};
    const scoped = element("input", ["f-input"], control, states, attributes);
    const ordinary = element("input", ["f-input"], root, states, attributes);
    assert.notEqual(cascade(ordinary, "color"), undefined, "Input fixture did not find its production foreground");
    assert.notEqual(cascade(ordinary, "background"), undefined, "Input fixture did not find its production fill");
    for (const property of [...properties, "height", "padding"])
      assert.equal(cascade(scoped, property), cascade(ordinary, property), `Input/${states}: ${property} was reskinned`);
  }
});

test("Primary access Field and label both select paired ink instead of inheriting a muted Field parent", () => {
  const { surface, root } = accessFixture();
  const field = element("div", ["f-field"], surface);
  const label = element("label", ["f-field-label"], field);
  assert.equal(cascade(field, "color"), "var(--f-primary-ink)");
  assert.equal(cascade(label, "color"), "var(--f-primary-ink)");
  assert.equal(inherited(label, "color"), "var(--f-primary-ink)");
  const ordinaryField = element("div", ["f-field"], root);
  const ordinaryLabel = element("label", ["f-field-label"], ordinaryField);
  assert.equal(cascade(ordinaryField, "color"), "var(--f-ink-soft,var(--f-muted))");
  assert.equal(cascade(ordinaryLabel, "color"), "var(--f-ink-soft,var(--f-muted))");
});

test("Presentation scene CSS does not own per-scene button or input rendering rules", () => {
  for (const rule of rules(presentation))
    assert.doesNotMatch(rule.selector, /\.f-button(?:-[\w-]+)?\b|\b(?:input|select|textarea|button)\b/,
      `Presentation reskins a standard control: ${rule.selector}`);
});

test("The single Primary operation panel clears only its direct Section spacing and supplies full-width native actions", () => {
  const { surface, root } = accessFixture();
  const section = element("section", ["f-section"], surface);
  assert.equal(cascade(section, "margin"), "0");
  assert.equal(cascade(section, "padding-top"), "0");
  assert.equal(cascade(element("section", ["f-section"], root), "padding-top"), "clamp(44px,7vw,84px)");
  const choices = element("div", ["f-navigation-choices"], section);
  const panel = element("section", ["f-navigation-choice-panel"], choices);
  const nativeForm = element("form", [], panel);
  const formLayout = element("div", ["f-form-layout"], nativeForm);
  for (const parent of [section, panel, formLayout]) {
    const action = element("button", ["f-button", "f-button-secondary"], parent);
    assert.equal(cascade(action, "width"), "100%", "A standard/linked/POST action lost the operation area's width");
    assert.equal(cascade(action, "max-width"), "100%");
  }
  assert.equal(cascade(element("button", ["f-button", "f-button-secondary"], root), "width"), "max-content",
    "Operation geometry leaked into an ordinary Button");
});

test("Access fields and production action rows share one gap through real native form and retained panel topology", () => {
  const { surface } = accessFixture();
  const choices = element("div", ["f-navigation-choices"], surface);
  const panel = element("section", ["f-navigation-choice-panel"], choices);
  const nativeForm = element("form", [], panel);
  const formLayout = element("div", ["f-form-layout"], nativeForm);
  const inlineActions = element("div", ["f-inline-actions"], formLayout, [], { "data-alignment": "end" });
  const actionsSlot = element("div", ["f-access-form-actions"], surface);
  for (const node of [surface, panel, nativeForm, formLayout, actionsSlot])
    assert.equal(cascade(node, "gap"), "var(--f-access-form-gap)", `The ${node.tag} ${[...node.classes]} introduced a different form/action gap`);
  assert.equal(cascade(surface, "--f-access-form-gap"), "24px");
  assert.equal(cascade(actionsSlot, "margin-top"), "0", "The slot added a second gap above actions");
  assert.equal(cascade(inlineActions, "margin-top"), "0", "The form row added a second gap above actions");
  assert.equal(cascade(element("div", ["f-inline-actions"], actionsSlot), "margin-top"), "0");
  const hiddenPanel = element("section", ["f-navigation-choice-panel"], choices, [], { hidden: "" });
  assert.equal(cascade(hiddenPanel, "display"), "none", "Grid composition made a retained inactive panel visible");
});

test("Identity cards retain bounded width and start-aligned large name and bold identifier through production styles", () => {
  const root = element("div", ["f-root"]);
  const body = element("div", ["f-page-body"], root);
  const card = element("section", ["f-identity-card"], body);
  const heading = element("h1", ["f-identity-title"], card);
  const layout = element("div", ["f-identity-layout"], card);
  const content = element("div", ["f-identity-content"], layout);
  const identifier = element("code", ["f-copy-text"], content);
  for (const viewport of [1200, 760, 360]) {
    assert.equal(cascade(card, "width", viewport), "auto", "Explicit 100% width adds gutters outside the page");
    assert.equal(cascade(card, "max-width", viewport), "100%");
    assert.equal(cascade(card, "box-sizing", viewport), "border-box");
    assert.equal(cascade(card, "text-align", viewport), "start");
    assert.equal(cascade(heading, "font-size", viewport), "var(--f-type-page,50px)");
    assert.equal(cascade(heading, "overflow-wrap", viewport), "anywhere");
    assert.equal(cascade(heading, "text-align", viewport), "start");
    assert.equal(cascade(identifier, "font-weight", viewport), "740");
    assert.equal(inherited(identifier, "color"), "var(--f-primary-ink)");
    assert.notEqual(cascade(heading, "position", viewport), "sticky");
    assert.notEqual(cascade(heading, "background", viewport), "var(--f-canvas)");
  }
});

test("Split organization typography and operation geometry follow the real 860px and 560px cascades", () => {
  const { hero, layout: heroLayout, content, heading, word, side } = heroFixture();
  for (const [viewport, font, columns, gap] of [
    [1200, "clamp(64px,9.5vw,112px)", "minmax(0,1fr) minmax(360px,470px)", "clamp(54px,9vw,120px)"],
    [861, "clamp(64px,9.5vw,112px)", "minmax(0,1fr) minmax(360px,470px)", "clamp(54px,9vw,120px)"],
    [860, "clamp(58px,12vw,88px)", "minmax(0,1fr)", "48px"],
    [600, "clamp(58px,12vw,88px)", "minmax(0,1fr)", "48px"],
    [560, "clamp(52px,16vw,72px)", "minmax(0,1fr)", "48px"],
    [360, "clamp(52px,16vw,72px)", "minmax(0,1fr)", "48px"],
  ]) {
    assert.equal(cascade(heading, "font-size", viewport), font, `${viewport}px title size`);
    assert.equal(cascade(heroLayout, "grid-template-columns", viewport), columns, `${viewport}px layout columns`);
    assert.equal(cascade(heroLayout, "gap", viewport), gap, `${viewport}px column gap`);
    assert.equal(cascade(content, "justify-items", viewport), "end", `${viewport}px artistic heading alignment`);
    assert.equal(cascade(content, "text-align", viewport), "end", `${viewport}px artistic text alignment`);
    assert.equal(cascade(content, "min-width", viewport), "0", `${viewport}px shrinking content track`);
    assert.equal(cascade(heading, "max-width", viewport), "100%", `${viewport}px long-name heading boundary`);
    assert.equal(cascade(heading, "overflow-wrap", viewport), "anywhere", `${viewport}px unbroken organization name`);
    assert.equal(cascade(word, "min-width", viewport), "0");
    assert.equal(cascade(word, "overflow-wrap", viewport), "anywhere");
    assert.notEqual(inherited(word, "white-space"), "nowrap", "Unbroken long names were forbidden to wrap");
    assert.notEqual(cascade(heading, "width", viewport), "max-content", "Retired unbounded title width returned");
    assert.notEqual(cascade(heading, "max-width", viewport), "none", "Retired unbounded title maximum returned");
    assert.equal(cascade(side, "min-width", viewport), "0");
  }
  assert.equal(cascade(hero, "width"), "100%");
  assert.equal(cascade(word.parent, "display"), "grid");
  const container = heroLayout.parent;
  assert.equal(cascade(container, "width"), "100%");
  assert.equal(cascade(container, "max-width"), "var(--f-content-width,1180px)");
  assert.equal(cascade(container, "margin-inline"), "auto");
});

test("A Primary split hero's actual nested subtitle and description inherit its paired foreground", () => {
  const { content } = heroFixture("primary");
  for (const className of ["f-presentation-hero-subtitle", "f-presentation-hero-description"])
    assert.equal(inherited(element("p", [className], content), "color"), "var(--f-primary-ink)",
      `The nested ${className} did not use the actual Primary hero surface`);
});

function element(tag, classes = [], parent = null, states = [], attributes = {}) {
  return { tag, classes: new Set(classes), parent, states: new Set(states), attributes: new Map(Object.entries(attributes)) };
}
function accessFixture() {
  const root = element("div", ["f-root"]);
  const surface = element("div", ["f-access-form-surface", "f-access-form-primary"], root);
  return { root, surface };
}
function heroFixture(tone = "canvas") {
  const root = element("div", ["f-root"]);
  const hero = element("section", ["f-presentation-band", "f-presentation-hero", "f-presentation-hero-split", `f-presentation-${tone}`], root);
  const container = element("div", ["f-content-container"], hero);
  const layout = element("div", ["f-presentation-hero-layout"], container);
  const content = element("div", ["f-presentation-hero-content"], layout);
  const heading = element("h1", ["f-presentation-title-words"], content);
  return { hero, layout, content, heading, word: element("span", [], heading), side: element("div", ["f-presentation-hero-side"], layout) };
}

async function importedStylesheet(path, ancestry = []) {
  assert.ok(!ancestry.includes(path), `Cyclic stylesheet import: ${path}`);
  const css = (await readFile(path, "utf8")).replace(/\/\*[\s\S]*?\*\//g, "");
  const sources = [];
  let position = 0;
  for (const match of css.matchAll(/@import\s+([^;]+);/g)) {
    const import_ = /^url\((['"]?)([^)'"\s]+)\1\)$/.exec(match[1].trim())
      ?? /^(['"])([^'"]+)\1$/.exec(match[1].trim());
    assert.ok(import_?.[2].startsWith("./"), `The regression must not ignore an external or conditional import: ${match[0]}`);
    assert.equal(css.slice(position, match.index).trim(), "", `An import appeared after ordinary CSS: ${path}`);
    sources.push(...await importedStylesheet(resolve(dirname(path), import_[2]), [...ancestry, path]));
    position = match.index + match[0].length;
  }
  const body = css.slice(position);
  assert.doesNotMatch(body, /@import\b/, `Unsupported import syntax: ${path}`);
  sources.push(body);
  return sources;
}

// Bounded CSS evaluation, not a browser: match real declarations on the component's
// actual parent chain. It includes compound/descendant/child selectors, :not/:is,
// disabled attributes, !important, specificity, source order and width media queries.
function rules(source, media = []) {
  const css = source.replace(/\/\*[\s\S]*?\*\//g, "");
  const result = [];
  for (let offset = 0; offset < css.length;) {
    const open = css.indexOf("{", offset);
    if (open < 0) break;
    const selector = css.slice(offset, open).trim();
    let end = open + 1, depth = 1;
    while (end < css.length && depth) { if (css[end] === "{") depth++; else if (css[end] === "}") depth--; end++; }
    assert.equal(depth, 0, `Unbalanced CSS block: ${selector}`);
    const body = css.slice(open + 1, end - 1);
    if (selector.startsWith("@media")) result.push(...rules(body, [...media, selector]));
    else if (!selector.startsWith("@")) result.push({ selector, body, media, declarations: new Map([...body.matchAll(/([\w-]+)\s*:\s*([^;]+)(?:;|$)/g)].map(match => [match[1], match[2].trim()])) });
    offset = end;
  }
  return result;
}
function mediaMatches(media, viewport) {
  return media.every(query => {
    const minimum = /min-width\s*:\s*([\d.]+)px/.exec(query);
    const maximum = /max-width\s*:\s*([\d.]+)px/.exec(query);
    return (!minimum || viewport >= Number(minimum[1])) && (!maximum || viewport <= Number(maximum[1]));
  });
}
function arms(selector) {
  let depth = 0, start = 0;
  const result = [];
  for (let index = 0; index < selector.length; index++) {
    if (selector[index] === "(" || selector[index] === "[") depth++;
    else if (selector[index] === ")" || selector[index] === "]") depth--;
    else if (selector[index] === "," && depth === 0) { result.push(selector.slice(start, index).trim()); start = index + 1; }
  }
  result.push(selector.slice(start).trim());
  return result;
}
function specificityOrder(left, right) { return left.map((value, index) => value - right[index]).find(value => value !== 0) ?? 0; }
function compound(selector, node) {
  let matches = true, offset = 0;
  const specificity = [0, 0, 0];
  while (offset < selector.length) {
    const remaining = selector.slice(offset);
    const group = /^:(not|is|where)\(/.exec(remaining);
    if (group) {
      let end = group[0].length, depth = 1;
      while (end < remaining.length && depth) { if (remaining[end] === "(") depth++; else if (remaining[end] === ")") depth--; end++; }
      const arguments_ = arms(remaining.slice(group[0].length, end - 1)).map(value => compound(value, node));
      if (arguments_.some(value => !value)) return null;
      matches &&= group[1] === "not" ? !arguments_.some(value => value.matches) : arguments_.some(value => value.matches);
      if (group[1] !== "where") {
        const maximum = arguments_.map(value => value.specificity).sort(specificityOrder).at(-1);
        maximum.forEach((value, index) => specificity[index] += value);
      }
      offset += end;
      continue;
    }
    const token = /^(?:(\[([^\]=]+)(?:=(["']?)([^\]"']+)\3)?\])|(\.([\w-]+))|(#([\w-]+))|(:([\w-]+))|([a-z][\w-]*)|(\*))/.exec(remaining);
    if (!token) return null; // Pseudo-elements and sibling/tree-query selectors do not target this bounded fixture.
    if (token[1]) {
      specificity[1]++;
      matches &&= node.attributes.has(token[2]) && (token[4] === undefined || node.attributes.get(token[2]) === token[4]);
    } else if (token[5]) { specificity[1]++; matches &&= node.classes.has(token[6]); }
    else if (token[7]) { specificity[0]++; matches &&= node.attributes.get("id") === token[8]; }
    else if (token[9]) { specificity[1]++; matches &&= node.states.has(token[10]) || token[10] === "disabled" && node.attributes.has("disabled"); }
    else if (token[11]) { specificity[2]++; matches &&= node.tag === token[11]; }
    offset += token[0].length;
  }
  return { matches, specificity };
}
function selectorParts(selector) {
  const result = [];
  let current = "", depth = 0;
  const flush = () => { if (current) { result.push(current); current = ""; } };
  for (const character of selector) {
    if (character === "(" || character === "[") depth++;
    else if (character === ")" || character === "]") depth--;
    if (!depth && character === ">") { flush(); if (result.at(-1) === " ") result.pop(); result.push(">"); }
    else if (!depth && /\s/.test(character)) { flush(); if (result.length && result.at(-1) !== " " && result.at(-1) !== ">") result.push(" "); }
    else current += character;
  }
  flush(); if (result.at(-1) === " ") result.pop();
  return result;
}
function matching(selector, target) {
  const parts = selectorParts(selector);
  function visit(index, node) {
    if (!node) return null;
    const current = compound(parts[index], node);
    if (!current?.matches) return null;
    if (index === 0) return current.specificity;
    for (let parent = node.parent; parent; parent = parts[index - 1] === ">" ? null : parent.parent) {
      const prior = visit(index - 2, parent);
      if (prior) return current.specificity.map((value, position) => value + prior[position]);
    }
    return null;
  }
  return visit(parts.length - 1, target);
}
function cascade(target, property, viewport = 1200) {
  let winner;
  for (const rule of stylesheet) {
    const raw = rule.declarations.get(property);
    if (raw === undefined || !mediaMatches(rule.media, viewport)) continue;
    const important = /!important\s*$/.test(raw) ? 1 : 0;
    for (const selector of arms(rule.selector)) {
      const specificity = matching(selector, target);
      if (!specificity || winner && (important < winner.important || important === winner.important && specificityOrder(specificity, winner.specificity) < 0)) continue;
      winner = { value: raw.replace(/\s*!important\s*$/, ""), important, specificity };
    }
  }
  return winner?.value;
}
function inherited(target, property) {
  const value = cascade(target, property);
  return value === "inherit" || value === undefined ? target.parent ? inherited(target.parent, property) : undefined : value;
}
