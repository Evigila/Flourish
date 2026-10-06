import assert from "node:assert/strict";
import { access, readdir, readFile } from "node:fs/promises";
import { dirname, extname, join, relative, resolve } from "node:path";
import test from "node:test";
import { fileURLToPath } from "node:url";
import "./page-body-actions.test.mjs";

const testDirectory = dirname(fileURLToPath(import.meta.url));
const repositoryRoot = resolve(testDirectory, "../..");
const blazorRoot = join(repositoryRoot, "src/Flourish.Blazor");
const designCssRoot = join(blazorRoot, "Flourish.Blazor.Design/wwwroot");
const foundationPath = join(designCssRoot, "foundation.css");
const aliasesPath = join(designCssRoot, "theme-aliases.css");

test("presentation layout separates full-width backgrounds from centered content and guards hidden offers behind enhancement", async () => {
  const frameworkRoot = join(blazorRoot, "Flourish.Blazor.Framework/wwwroot");
  const css = withoutComments(await readFile(join(frameworkRoot, "presentation/layout.css"), "utf8"));
  const container = blockFor(css, ".f-content-container");
  assert.equal(property(container, "width"), "100%");
  assert.equal(property(container, "max-width"), "var(--f-content-width,1180px)");
  assert.equal(property(container, "margin-inline"), "auto");
  assert.equal(property(blockFor(css, ".f-presentation-band"), "width"), "100%");
  const document = blockFor(css, ".content-surface.f-document-surface");
  assert.equal(property(document, "height"), "auto");
  assert.equal(property(document, "overflow"), "visible");
  assert.equal(property(blockFor(css, ".content-surface.f-document-surface > .content-stage"), "overflow"), "visible");
  assert.equal(property(blockFor(css, ".f-presentation-footer"), "flex"), "0 0 auto");
  assert.equal(property(blockFor(css, ".f-access-panel"), "width"), "min(100%,560px)");
  assert.equal(property(blockFor(css, ".f-access-panel-wide"), "width"), "min(100%,960px)");
  assert.equal(property(blockFor(css, ".f-access-form-surface"), "display"), "grid");
  assert.equal(property(blockFor(css, ".f-access-form-surface > form"), "display"), "grid");
  assert.match(css, /@media\(min-width:1100px\) and \(prefers-reduced-motion:no-preference\)/);
  for (const rule of rules(css)) {
    if (/\.f-offer-(?:details|card\s+h3)\b/.test(rule.selector)
      && (property(rule.body, "opacity") === "0" || property(rule.body, "visibility") === "hidden")) {
      assert.ok(selectorArms(rule.selector).every(selector => selector.includes("[data-offer-ready]")),
        `Static SSR offer text became hidden without an attached controller: ${rule.selector}`);
    }
  }
  const compressed = blockFor(css, ".f-offer-stage[data-offer-ready]");
  assert.equal(property(compressed, "grid-template-columns"), "repeat(var(--f-offer-columns),minmax(0,1fr))");
  const baseDetails = blockFor(css, ".f-offer-details");
  assert.equal(property(baseDetails, "visibility"), undefined);
  assert.equal(property(baseDetails, "opacity"), undefined);
  assert.equal(property(blockFor(css, ".f-offer-controls"), "display"), "none");
  const rotationControls = rules(css).filter(rule => rule.selector.includes(".f-offer-controls") && property(rule.body, "display") !== "none");
  assert.equal(rotationControls.length, 1);
  assert.match(rotationControls[0].selector, /\.f-offer-presentation:has\(\.f-offer-stage\[data-offer-ready\]\) > \.f-offer-controls/);
  assert.equal(property(rotationControls[0].body, "display"), "flex");
});

test("presentation and access scenes consume existing theme roles without reskinning buttons or inputs", async () => {
  const foundation = withoutComments(await readFile(foundationPath, "utf8"));
  const knownTokens = new Set([...foundation.matchAll(/(--f-[\w-]+)\s*:/g)].map(match => match[1]));
  for (const path of [join(designCssRoot, "presentation.css"), join(designCssRoot, "primitives/AccessBrand.css")]) {
    const css = withoutComments(await readFile(path, "utf8"));
    assert.doesNotMatch(css, /#[0-9a-f]{3,8}\b|\b(?:rgba?|hsla?|color-mix)\s*\(|f-theme-|data-theme|font-family\s*:/i,
      `${relative(repositoryRoot, path)} introduced a local theme, color literal or separate font family.`);
    for (const token of css.matchAll(/var\((--f-[\w-]+)/g))
      assert.ok(knownTokens.has(token[1]), `${relative(repositoryRoot, path)} uses an undefined palette or typography token ${token[1]}.`);
    for (const rule of rules(css)) {
      assert.doesNotMatch(rule.selector, /\.f-button(?:-[\w-]+)?\b|\b(?:input|select|textarea|button)\b/,
        `${relative(repositoryRoot, path)} reskins a standard action or input: ${rule.selector}`);
    }
  }
  const presentation = withoutComments(await readFile(join(designCssRoot, "presentation.css"), "utf8"));
  assert.equal(property(blockFor(presentation, ".f-presentation-primary"), "color"), "var(--f-primary-ink)");
  assert.equal(property(blockFor(presentation, ".f-presentation-primary"), "background-color"), "var(--f-primary)");
  assert.equal(property(blockFor(presentation, ".f-presentation-band-heading :is(h1,h2,h3,h4,h5,h6)"), "font-size"), "var(--f-type-h2,28px)");
  assert.equal(property(blockFor(presentation, ".f-presentation-hero-content > h1"), "font-size"), "clamp(72px,11vw,128px)");
  assert.equal(property(blockFor(presentation, ".f-presentation-hero-content > .f-presentation-hero-description"), "font-size"), "var(--f-type-body,17px)");
  assert.equal(property(blockFor(presentation, ".f-presentation-footer"), "color"), "var(--f-primary-ink)");
  assert.equal(property(blockFor(presentation, ".f-presentation-footer"), "background"), "var(--f-primary)");
});

test("shell chrome only adjusts available Quiet buttons, preserving explicit variants", async () => {
  const css = withoutComments(await readFile(join(designCssRoot, "static-surfaces.css"), "utf8"));
  const selectors = [...css.matchAll(/([^{}]+)\{[^{}]*\}/g)]
    .map(match => match[1].trim())
    .filter(selector => selector.includes(".shell-header") && selector.includes(".f-button"));
  assert.equal(selectors.length, 3);
  for (const selector of selectors) {
    assert.match(selector, /\.f-button-quiet\b/);
    assert.match(selector, /:not\(:disabled\)/);
    assert.match(selector, /:not\(\[aria-disabled=true\]\)/);
    assert.doesNotMatch(selector, /\.f-button(?:\s|:|$)/);
  }
  const controls = withoutComments(await readFile(join(designCssRoot, "controls.css"), "utf8"));
  assert.match(controls, /\.f-button-elevated\{[^}]*color:var\(--f-text\);background:var\(--f-surface\);box-shadow:var\(--f-shadow-control\)/);
});

test("read-only ListView reuses the table surface and exposes complete wrapped values", async () => {
  const design = withoutComments(await readFile(join(designCssRoot, "data.css"), "utf8"));
  const frameworkRoot = join(blazorRoot, "Flourish.Blazor.Framework");
  const framework = withoutComments(await readFile(join(frameworkRoot, "wwwroot/framework.css"), "utf8"));
  const component = await readFile(join(frameworkRoot, "Components/ListView.razor"), "utf8");
  assert.match(component, /class="f-data f-list-view/);
  assert.match(component, /class="f-data-scroll" role="region" tabindex="0"/);
  assert.match(component, /class="f-data-table"/);
  assert.match(component, /TableData<TItem>\.Display/);
  assert.doesNotMatch(component, /IJSRuntime|OnAfterRender|@onclick|data-f-table|f-data-resize|f-data-toolbar|f-data-actions|f-data-pager/);
  const surface = blockFor(design, ".f-data-scroll");
  assert.equal(property(surface, "border"), "1px solid var(--f-border)");
  assert.equal(property(surface, "border-radius"), "16px");
  assert.equal(property(surface, "background"), "var(--f-surface)");
  const cells = blockFor(design, ".f-data-table th, .f-data-table td");
  assert.equal(property(cells, "height"), "76px");
  const text = blockFor(design, ".f-list-view .f-data-table .f-data-cell");
  assert.equal(property(text, "white-space"), "normal");
  assert.equal(property(text, "overflow"), "visible");
  assert.equal(property(text, "padding-block"), "12px");
  assert.equal(property(blockFor(framework, ".f-list-view .f-data-cell"), "white-space"), "normal");
  assert.equal(property(blockFor(design, ".f-list-view .f-data-table tbody th"), "background"), "var(--f-surface)");
  assert.equal(property(blockFor(design, ".f-data-table tbody tr:last-child :is(th, td)"), "border-bottom"), "0");
  assert.doesNotMatch(design, /\.f-list-view[^{}]*(?:hover|active)[^{}]*\{/);
});

test("unavailable Elevated actions retain their surface and elevation without acquiring hover or press paint", async () => {
  const css = await readFile(join(designCssRoot, "controls.css"), "utf8");
  const styles = rules(css);
  const element = (variant, nativeDisabled, ariaDisabled, hover = false, active = false) => ({
    tag: nativeDisabled ? "button" : "a",
    classes: new Set(["f-button", `f-button-${variant}`]),
    attributes: new Map(ariaDisabled ? [["aria-disabled", "true"]] : []),
    states: new Set([...(nativeDisabled ? ["disabled"] : []), ...(hover ? ["hover"] : []), ...(active ? ["active"] : [])]),
  });
  for (const [nativeDisabled, ariaDisabled] of [[true, false], [false, true]]) {
    for (const hover of [false, true]) {
      for (const active of [false, true]) {
        const unavailable = element("elevated", nativeDisabled, ariaDisabled, hover, active);
        assert.equal(buttonCascade(styles, unavailable, "background"), "var(--f-surface)");
        assert.equal(buttonCascade(styles, unavailable, "box-shadow"), "var(--f-shadow-control)");
        assert.equal(buttonCascade(styles, unavailable, "color"), "var(--f-disabled)");
        assert.equal(buttonCascade(styles, unavailable, "border-color"), "var(--f-border)");
        assert.equal(buttonCascade(styles, unavailable, "cursor"), "not-allowed");
        for (const variant of ["primary", "secondary", "quiet", "danger", "underline"]) {
          const other = element(variant, nativeDisabled, ariaDisabled, hover, active);
          assert.equal(buttonCascade(styles, other, "background"), "transparent", `${variant} changed its disabled background.`);
          assert.equal(buttonCascade(styles, other, "box-shadow"), "none", `${variant} gained disabled elevation.`);
          assert.equal(buttonCascade(styles, other, "color"), "var(--f-disabled)");
          assert.equal(buttonCascade(styles, other, "cursor"), "not-allowed");
        }
      }
    }
  }
  for (const [hover, active, expected] of [[false, false, "var(--f-surface)"], [true, false, "var(--f-target-preview)"],
    [false, true, "var(--f-target-click)"], [true, true, "var(--f-target-click)"]]) {
    const available = element("elevated", false, false, hover, active);
    assert.equal(buttonCascade(styles, available, "background"), expected);
    assert.equal(buttonCascade(styles, available, "box-shadow"), "var(--f-shadow-control)");
    assert.equal(buttonCascade(styles, available, "color"), "var(--f-text)");
    assert.equal(buttonCascade(styles, available, "cursor"), "pointer");
  }
});

test("UniformGrid fills rectangular rows while centered square grids retain their maximum side", async () => {
  const css = withoutComments(await readFile(join(blazorRoot, "Flourish.Blazor.Framework/wwwroot/uniform-grid.css"), "utf8"));
  const centered = blockFor(css, ".f-uniform-grid-centered");
  assert.equal(centered.trim(), "margin-inline:auto;");
  const grid = blockFor(css, ".f-uniform-grid");
  assert.equal(property(grid, "width"), "100%");
  assert.equal(property(grid, "max-width"), "100%");
  assert.equal(property(grid, "--f-grid-cell-width"), "1fr");
  assert.equal(property(grid, "grid-template-columns"),
    "repeat(auto-fit,minmax(min(100%,calc(2 * var(--f-grid-cell-max-height,260px))),1fr))");
  const square = blockFor(css, ".f-uniform-grid-square");
  assert.equal(property(square, "width"), "fit-content");
  assert.equal(property(square, "--f-grid-cell-width"), "var(--f-grid-cell-max,280px)");
  assert.equal(property(square, "grid-template-columns"), "repeat(auto-fit,minmax(0,var(--f-grid-cell-width)))");
  assert.equal(property(blockFor(css, ".f-uniform-grid-square > *"), "aspect-ratio"), "1");
  assert.equal(property(blockFor(css, ".f-uniform-grid-square > *"), "max-width"), "var(--f-grid-cell-max,280px)");
  assert.equal(property(blockFor(css, ".f-uniform-grid-square > *"), "max-height"), "var(--f-grid-cell-max,280px)");
});

test("UniformGrid explicit rows columns and narrow overrides share uncapped rectangle tracks and capped square tracks", async () => {
  const css = withoutComments(await readFile(join(blazorRoot, "Flourish.Blazor.Framework/wwwroot/uniform-grid.css"), "utf8"));
  assert.equal(property(blockFor(css, ".f-uniform-grid-columns"), "grid-template-columns"),
    "repeat(var(--f-grid-columns),minmax(0,var(--f-grid-cell-width)))");
  assert.equal(property(blockFor(css, ".f-uniform-grid-rows"), "grid-template-rows"),
    "repeat(var(--f-grid-rows),minmax(0,1fr))");
  const rows = blockFor(css, ".f-uniform-grid-rows:not(.f-uniform-grid-columns)");
  assert.equal(property(rows, "grid-template-columns"), "none");
  assert.equal(property(rows, "grid-auto-flow"), "column");
  assert.equal(property(rows, "grid-auto-columns"), "minmax(0,var(--f-grid-cell-width))");
  const narrow = blockFor(css, ".f-uniform-grid.f-uniform-grid-narrow-columns");
  assert.equal(property(narrow, "grid-template-columns"),
    "repeat(var(--f-grid-narrow-columns),minmax(0,var(--f-grid-cell-width)))");
  assert.equal(property(narrow, "grid-auto-flow"), "row");
  assert.equal(property(narrow, "grid-auto-columns"), "auto");
  const rectangle = blockFor(css, ".f-uniform-grid-rectangle > .f-uniform-cell");
  assert.equal(property(rectangle, "max-height"), "var(--f-grid-cell-max-height,260px)");
  assert.equal(property(rectangle, "overflow"), "auto");
  assert.equal(property(rectangle, "max-width"), undefined);
  assert.equal(property(blockFor(css, ".f-uniform-grid-rectangle > *"), "aspect-ratio"), "2/1");
});

const roles = [
  "primary",
  "accent",
  "canvas",
  "surface",
  "text",
  "muted",
  "display-board",
  "preview-light",
  "preview-dark",
  "border",
  "danger",
  "click-light",
  "click-dark",
  "info-text",
  "warning-background",
  "preview-danger",
  "click-danger",
];

const light = {
  "--f-primary": "var(--f-primary-light)",
  "--f-accent": "var(--f-accent-light)",
  "--f-canvas": "#F3F5F5",
  "--f-surface": "var(--f-surface-light)",
  "--f-text": "#112924",
  "--f-muted": "#4F645D",
  "--f-display-board": "#E5E8EB",
  "--f-preview-light": "#C9DFDA",
  "--f-preview-dark": "#2F5049",
  "--f-border": "#9FAEA9",
  "--f-danger": "#9D322D",
  "--f-preview-danger": "#7E2824",
  "--f-click-danger": "#712420",
  "--f-click-light": "#B5C9C4",
  "--f-click-dark": "#2A4842",
  "--f-info-text": "#1565C0",
  "--f-warning-background": "#F2A33A",
};

const dark = {
  "--f-primary": "var(--f-primary-dark)",
  "--f-accent": "var(--f-accent-dark)",
  "--f-canvas": "#18231D",
  "--f-surface": "#263A30",
  "--f-text": "#E7EFEA",
  "--f-muted": "#B8C9BF",
  "--f-display-board": "#31483B",
  "--f-preview-light": "#D5E5DE",
  "--f-preview-dark": "#395A48",
  "--f-border": "#526F60",
  "--f-danger": "#FFB4AB",
  "--f-preview-danger": "#8C3430",
  "--f-click-danger": "#7E2F2B",
  "--f-click-light": "#C0CEC8",
  "--f-click-dark": "#335141",
  "--f-info-text": "#8CB8FF",
  "--f-warning-background": "#E5A047",
};

const approvedLiterals = new Set([
  "#153A32", "#16745F", "#F3F5F5", "#FFFFFF", "#112924",
  "#4F645D", "#E5E8EB", "#C9DFDA", "#2F5049", "#9FAEA9",
  "#BBD7C9", "#75CBB2", "#18231D", "#263A30", "#E7EFEA",
  "#B8C9BF", "#31483B", "#D5E5DE", "#395A48", "#526F60",
  "#9D322D", "#FFB4AB", "#B5C9C4", "#C0CEC8", "#2A4842", "#335141",
  "#1565C0", "#8CB8FF", "#F2A33A", "#E5A047",
  "#7E2824", "#712420", "#8C3430", "#7E2F2B",
]);

async function filesUnder(root, extensions) {
  const result = [];
  for (const entry of await readdir(root, { withFileTypes: true })) {
    if (entry.isDirectory() && ["bin", "obj"].includes(entry.name)) continue;
    const path = join(root, entry.name);
    if (entry.isDirectory()) result.push(...await filesUnder(path, extensions));
    else if (extensions.has(extname(entry.name).toLowerCase())) result.push(path);
  }
  return result;
}

function withoutComments(css) {
  return css.replace(/\/\*[\s\S]*?\*\//g, "");
}

function selectorArms(selector) {
  const arms = [];
  let start = 0, depth = 0, quote = null;
  for (let index = 0; index < selector.length; index++) {
    const character = selector[index];
    if (quote) {
      if (character === "\\") index++;
      else if (character === quote) quote = null;
    } else if (character === '"' || character === "'") quote = character;
    else if (character === "(" || character === "[") depth++;
    else if (character === ")" || character === "]") depth--;
    else if (character === "," && depth === 0) {
      arms.push(selector.slice(start, index).trim());
      start = index + 1;
    }
  }
  arms.push(selector.slice(start).trim());
  return arms;
}

function blockFor(css, selector) {
  const escaped = selector.replace(/[.*+?^${}()|[\]\\]/g, "\\$&");
  const match = new RegExp(`(?:^|\\n)\\s*${escaped}\\s*\\{`, "m").exec(css);
  assert.ok(match, `Missing CSS block for ${selector}.`);
  const open = css.indexOf("{", match.index);
  let depth = 0;
  for (let index = open; index < css.length; index += 1) {
    if (css[index] === "{") depth += 1;
    if (css[index] === "}" && --depth === 0) return css.slice(open + 1, index);
  }
  assert.fail(`Unclosed CSS block for ${selector}.`);
}

function declarations(block) {
  return new Map([...block.matchAll(/(--[\w-]+)\s*:\s*([^;}]+)/g)].map(match => [match[1], match[2].trim()]));
}

function property(block, name) {
  const escaped = name.replace(/[.*+?^${}()|[\]\\]/g, "\\$&");
  return new RegExp(`(?:^|;)\\s*${escaped}\\s*:\\s*([^;}]+)`, "i").exec(block)?.[1].trim();
}

function assertMode(actual, expected, name) {
  for (const [property, value] of Object.entries(expected)) {
    assert.equal(actual.get(property), value, `${name} assigns ${property} outside its approved role value.`);
  }
  assert.equal(actual.get("--f-primary-ink"), "var(--f-surface)", `${name} primary ink must use Surface.`);
  assert.equal(actual.get("--f-accent-ink"), "var(--f-surface)", `${name} accent ink must use Surface.`);
}

function cssFunctions(css, names) {
  const result = [];
  const matcher = new RegExp(`\\b(${names.join("|")})\\(`, "gi");
  for (const match of css.matchAll(matcher)) {
    let depth = 0;
    let end = match.index;
    for (; end < css.length; end += 1) {
      if (css[end] === "(") depth += 1;
      else if (css[end] === ")" && --depth === 0) break;
    }
    result.push(css.slice(match.index, end + 1));
  }
  return result;
}

function rules(css) {
  return [...withoutComments(css).matchAll(/([^{}]+)\{([^{}]*)\}/g)]
    .map(match => ({ selector: match[1].trim(), body: match[2].trim(), declarations: declarations(match[2]) }));
}

function matchingRule(css, description, predicate) {
  const matches = rules(css).filter(predicate);
  assert.equal(matches.length, 1, `${description}: expected one rule, found ${matches.length}.`);
  return matches[0];
}

// This bounded cascade fixture models standalone Button compound selectors, not a browser DOM.
// It resolves matching, :not argument specificity, !important and source order so a disabled
// variant rule with insufficient specificity fails instead of passing a string-presence check.
function buttonCompound(selector, element) {
  let matches = true, offset = 0;
  const specificity = [0, 0, 0];
  while (offset < selector.length) {
    const token = /^(?:(:not\(([^()]*)\))|(\[([^\]=]+)(?:=(["']?)([^\]"']+)\5)?\])|(\.([\w-]+))|(:([\w-]+))|([a-z][\w-]*))/.exec(selector.slice(offset));
    if (!token) return null; // Descendant/combinator rules cannot match this standalone fixture.
    if (token[1]) {
      const negated = buttonCompound(token[2], element);
      assert.ok(negated, `Unsupported Button :not argument ${token[2]}.`);
      for (let index = 0; index < specificity.length; index++) specificity[index] += negated.specificity[index];
      matches &&= !negated.matches;
    } else if (token[3]) {
      specificity[1]++;
      matches &&= element.attributes.has(token[4]) && (token[6] === undefined || element.attributes.get(token[4]) === token[6]);
    } else if (token[7]) {
      specificity[1]++;
      matches &&= element.classes.has(token[8]);
    } else if (token[9]) {
      specificity[1]++;
      matches &&= element.states.has(token[10]);
    } else {
      specificity[2]++;
      matches &&= element.tag === token[11];
    }
    offset += token[0].length;
  }
  return { matches, specificity };
}

function buttonCascade(styles, element, name) {
  let winner;
  for (const rule of styles) {
    const raw = property(rule.body, name);
    if (raw === undefined) continue;
    const important = /!important\s*$/.test(raw) ? 1 : 0;
    for (const arm of selectorArms(rule.selector)) {
      const compound = buttonCompound(arm, element);
      if (!compound?.matches) continue;
      const specificityOrder = winner ? compound.specificity.map((value, index) => value - winner.specificity[index]).find(value => value !== 0) ?? 0 : 0;
      if (winner && (important < winner.important || important === winner.important && specificityOrder < 0)) continue;
      winner = { value: raw.replace(/\s*!important\s*$/, ""), important, specificity: compound.specificity };
    }
  }
  return winner?.value;
}

test("foundation exposes the seventeen-role palette to documents and Flourish surfaces", async () => {
  const css = withoutComments(await readFile(foundationPath, "utf8"));
  const baseBlock = blockFor(css, ":root, .f-root");
  const rootElementBlock = blockFor(css, ".f-root");
  const base = declarations(baseBlock);
  const darkMode = declarations(blockFor(css, ".f-root.f-theme-dark"));
  const systemMode = declarations(blockFor(css, ".f-root.f-theme-system"));

  assertMode(base, light, "Light mode");
  assertMode(darkMode, dark, "Dark mode");
  assertMode(systemMode, dark, "System dark mode");
  assert.equal(base.get("--f-primary-light"), "#153A32");
  assert.equal(base.get("--f-accent-light"), "#16745F");
  assert.equal(base.get("--f-primary-dark"), "#BBD7C9");
  assert.equal(base.get("--f-accent-dark"), "#75CBB2");
  assert.equal(base.get("--f-surface-light"), "#FFFFFF");

  for (const fragment of baseBlock.split(";")) {
    const separator = fragment.indexOf(":");
    if (separator >= 0) assert.ok(fragment.slice(0, separator).trim().startsWith("--"), "The shared token selector also applies element styling.");
  }
  assert.match(rootElementBlock, /(?:^|;)\s*font-family\s*:/);
  assert.match(rootElementBlock, /(?:^|;)\s*color\s*:/);
  assert.match(rootElementBlock, /(?:^|;)\s*background\s*:/);

  assert.equal(base.get("--f-surface-preview"), "var(--f-preview-light)");
  assert.equal(base.get("--f-target-preview"), "var(--f-surface-preview)");
  assert.equal(base.get("--f-primary-preview"), "var(--f-preview-dark)");
  assert.equal(base.get("--f-surface-click"), "var(--f-click-light)");
  assert.equal(base.get("--f-target-click"), "var(--f-surface-click)");
  assert.equal(base.get("--f-primary-click"), "var(--f-click-dark)");
  assert.equal(base.get("--f-accent-click"), "var(--f-click-dark)");
  assert.equal(base.get("--f-navigation-children-surface"), "var(--f-surface)");
  for (const [name, mode] of [["Dark mode", darkMode], ["System dark mode", systemMode]]) {
    assert.equal(mode.get("--f-surface-preview"), "var(--f-preview-dark)", `${name} does not use its surface preview role.`);
    assert.equal(mode.get("--f-target-preview"), "var(--f-surface-preview)", `${name} bypasses the surface preview alias.`);
    assert.equal(mode.get("--f-primary-preview"), "var(--f-preview-light)", `${name} does not invert preview over Primary.`);
    assert.equal(mode.get("--f-surface-click"), "var(--f-click-dark)", `${name} does not use the dark surface click role.`);
    assert.equal(mode.get("--f-target-click"), "var(--f-surface-click)", `${name} bypasses the surface click alias.`);
    assert.equal(mode.get("--f-primary-click"), "var(--f-click-light)", `${name} does not invert click over Primary.`);
    assert.equal(mode.get("--f-accent-click"), "var(--f-click-light)", `${name} does not invert click over Accent.`);
    assert.equal(mode.get("--f-navigation-children-surface"), "var(--f-surface)", `${name} gives nested navigation a private paint.`);
    assert.equal(mode.get("--f-chrome"), "var(--f-canvas)", `${name} chrome must use Canvas.`);
    assert.equal(mode.get("--f-chrome-ink"), "var(--f-text)", `${name} chrome ink must use Text.`);
  }
  assert.equal(base.get("--f-chrome"), "var(--f-primary)");
  assert.equal(base.get("--f-chrome-ink"), "var(--f-surface)");
});

test("click roles are exact ninety-percent preview channels", async () => {
  const css = withoutComments(await readFile(foundationPath, "utf8"));
  const modes = [declarations(blockFor(css, ":root, .f-root")), declarations(blockFor(css, ".f-root.f-theme-dark"))];
  const darken = hex => `#${[1, 3, 5].map(offset => Math.round(Number.parseInt(hex.slice(offset, offset + 2), 16) * .9)
    .toString(16).padStart(2, "0")).join("")}`.toUpperCase();
  for (const [index, mode] of modes.entries()) {
    assert.equal(mode.get("--f-click-light"), darken(mode.get("--f-preview-light")), `${index ? "Dark" : "Light"} click-light is not preview-light × 0.9.`);
    assert.equal(mode.get("--f-click-dark"), darken(mode.get("--f-preview-dark")), `${index ? "Dark" : "Light"} click-dark is not preview-dark × 0.9.`);
    assert.equal(mode.get("--f-click-danger"), darken(mode.get("--f-preview-danger")), `${index ? "Dark" : "Light"} click-danger is not preview-danger × 0.9.`);
  }
});

test("Design CSS derives paint from roles without private palette literals", async () => {
  const cssFiles = await filesUnder(designCssRoot, new Set([".css"]));
  const violations = [];
  for (const path of cssFiles) {
    const css = withoutComments(await readFile(path, "utf8"));
    const label = relative(repositoryRoot, path).replaceAll("\\", "/");
    const literals = [...css.matchAll(/#[0-9a-f]{3,8}\b/gi)].map(match => match[0].toUpperCase());
    if (path !== foundationPath && literals.length > 0) violations.push(`${label}: ${[...new Set(literals)].join(", ")}`);
    if (path === foundationPath) {
      for (const literal of literals) if (!approvedLiterals.has(literal)) violations.push(`${label}: unapproved ${literal}`);
    }
    for (const expression of cssFunctions(css, ["rgb", "rgba", "hsl", "hsla"])) {
      const primaryAlpha = /^rgb\(\s*from\s+var\(\s*--f-primary\s*\)\s+r\s+g\s+b\s*\/\s*(?:0?\.\d+|\d+(?:\.\d+)?%)\s*\)$/i.test(expression);
      const progressHighlight = path === join(designCssRoot, "controls.css")
        && /^rgb\(from var\(--f-surface-light\) r g b \/ 47%\)$/.test(expression)
        && rules(css).some(rule => rule.selector === ".f-progress-fill::after"
          && property(rule.body, "background")?.includes(expression));
      if (!primaryAlpha && !progressHighlight) {
        violations.push(`${label}: ${expression}`);
      }
    }
    if (/\bcolor-mix\s*\(/i.test(css)) violations.push(`${label}: color-mix()`);
    if (/\bbrightness\s*\(/i.test(css)) violations.push(`${label}: brightness()`);
    for (const match of css.matchAll(/:\s*(white|black|red|blue|green)\b/gi)) violations.push(`${label}: named ${match[1]}`);
  }
  assert.deepEqual(violations, [], `Palette bypasses:\n${violations.join("\n")}`);
});

test("palette roles have one source and retired token aliases are absent", async () => {
  const cssFiles = await filesUnder(designCssRoot, new Set([".css"]));
  const frameworkFiles = await filesUnder(join(blazorRoot, "Flourish.Blazor.Framework/wwwroot"), new Set([".css"]));
  const allDesign = (await Promise.all(cssFiles.map(path => readFile(path, "utf8")))).map(withoutComments).join("\n");
  await assert.rejects(access(aliasesPath), { code: "ENOENT" }, "The retired theme alias stylesheet still exists.");
  assert.doesNotMatch(await readFile(join(designCssRoot, "design.css"), "utf8"), /theme-aliases\.css/);
  const retired = [
    "canvas", "surface", "surface-alternate", "surface-raised", "secondary-nav", "ink", "ink-soft", "ink-muted",
    "muted", "disabled", "line", "line-strong", "row-hover", "row-active", "accent", "accent-strong",
    "accent-fill-hover", "accent-soft", "success", "danger", "focus", "chrome", "chrome-ink", "chrome-accent",
    "chrome-hover", "chrome-click", "chrome-line", "radius-small", "radius-medium", "radius-large",
    "control-radius", "checkbox-radius", "shadow-card", "control-height", "control-height-compact", "checkbox-size",
    "page-gutter", "field-label-gap", "field-label-weight", "surface-theme-primary", "surface-theme-accent",
    "surface-theme-primary-ink", "surface-theme-accent-ink", "surface-theme-accent-text", "surface-theme-focus",
    "topbar-height", "primary-rail-width", "secondary-rail-width", "surface-content-width", "surface-page-gutter",
    "surface-content-gutter", "surface-context", "surface-accent-context", "notice-text", "notice-background",
    "notice-foreground", "notice-error-background", "notice-subtle-background", "notice-subtle-foreground",
  ];
  const retiredToken = new RegExp(`--(?:${retired.join("|")})(?![\\w-])`);
  for (const path of [...cssFiles, ...frameworkFiles]) {
    const css = withoutComments(await readFile(path, "utf8"));
    assert.doesNotMatch(css, retiredToken, `${relative(repositoryRoot, path)} restores a retired token contract.`);
  }
  for (const role of roles) {
    const token = `--f-${role}`;
    assert.match(allDesign, new RegExp(`var\\(\\s*${token.replaceAll("-", "\\-")}\\b`), `${token} has no consumer.`);
    for (const path of cssFiles.filter(path => path !== foundationPath)) {
      const css = withoutComments(await readFile(path, "utf8"));
      assert.doesNotMatch(css, new RegExp(`${token.replaceAll("-", "\\-")}\\s*:`), `${token} is redefined by ${relative(repositoryRoot, path)}.`);
    }
  }
  assert.doesNotMatch(allDesign, /--f-border-strong\s*:|var\(\s*--f-border-strong\b/);
  assert.doesNotMatch(allDesign, /--f-focus\s*:|var\(\s*--f-focus\s*[,)]/);
});

test("current surface dimensions and local popup paint use canonical scoped roles", async () => {
  for (const name of ["navigation-surface", "content-surface"]) {
    const source = withoutComments(await readFile(join(designCssRoot, `patterns/${name}.css`), "utf8"));
    assert.equal(property(blockFor(source, `.${name}`), "color-scheme"), "inherit");
    const dimensionRules = rules(source).filter(rule => rule.selector === `.${name}` && rule.declarations.has("--f-content-width"));
    assert.equal(dimensionRules.length, 1, `${name} defines competing standard geometry.`);
    assert.equal(dimensionRules[0].declarations.get("--f-content-width"), "1180px");
    assert.equal(dimensionRules[0].declarations.get("--f-page-gutter"), "24px");
  }
  const framework = withoutComments(await readFile(join(blazorRoot, "Flourish.Blazor.Framework/wwwroot/layout.css"), "utf8"));
  assert.equal(declarations(blockFor(framework, ":root")).get("--f-control-height"), "48px");
  const navigation = await readFile(join(designCssRoot, "patterns/navigation-surface.css"), "utf8");
  assert.equal(property(blockFor(navigation, ".navigation-surface .valid.modified"), "border-color"), "var(--f-accent) !important");
  for (const [file, selector] of [["primitives/SystemNavigationMenu.css", ".system-menu-view"], ["controls.css", ".f-menu-panel"]]) {
    const local = declarations(blockFor(await readFile(join(designCssRoot, file), "utf8"), selector));
    assert.equal(local.get("--f-target-preview"), "var(--f-surface-preview)", `${selector} lost its surface-specific hover role.`);
    assert.equal(local.get("--f-target-click"), "var(--f-surface-click)", `${selector} lost its surface-specific press role.`);
  }
});

test("hover, press and persistent selections keep distinct cascade outcomes", async () => {
  const paths = Object.fromEntries(["foundation", "controls", "dropdown", "data", "uniform-grid", "split-button"]
    .map(name => [name, join(designCssRoot, `${name}.css`)]));
  const source = Object.fromEntries(await Promise.all(Object.entries(paths)
    .map(async ([name, path]) => [name, await readFile(path, "utf8")])));
  const allRules = Object.values(source).flatMap(rules);
  const one = (file, description, predicate) => matchingRule(source[file], description, predicate);
  const hasSelector = (rule, fragment) => rule.selector.includes(fragment);

  const groupedPreview = allRules.filter(rule => rule.selector.includes(":is(:hover,:active)")
    && /background\s*:\s*var\(--f-(?:target|primary|surface)-preview\b/.test(rule.body));
  assert.deepEqual(groupedPreview.map(rule => rule.selector), [], "A grouped hover/active selector makes press indistinguishable from hover.");

  const primaryHover = one("controls", "Primary button hover", rule => hasSelector(rule, ".f-button-primary:hover") && !rule.selector.includes(":active"));
  const primaryPress = one("controls", "Primary button press", rule => hasSelector(rule, ".f-button-primary:active"));
  assert.equal(property(primaryHover.body, "background"), "var(--f-primary-preview)");
  assert.equal(property(primaryPress.body, "background"), "var(--f-primary-click)");
  const controlRules = rules(source.controls);
  assert.ok(controlRules.findIndex(rule => rule.selector === primaryPress.selector) > controlRules.findIndex(rule => rule.selector === primaryHover.selector),
    "Primary press does not win the equal-specificity cascade.");

  for (const variant of ["secondary", "quiet", "elevated"]) {
    // General variants are distinct from the available Quiet action's Primary-surface context.
    const hover = one("controls", `${variant} button hover`, rule => hasSelector(rule, `.f-button-${variant}:hover`) && !rule.selector.includes(":active") && !rule.selector.includes(".f-access-form-primary"));
    const press = one("controls", `${variant} button press`, rule => hasSelector(rule, `.f-button-${variant}:active`) && !rule.selector.includes(".f-access-form-primary"));
    assert.equal(property(hover.body, "background"), "var(--f-target-preview)");
    assert.equal(property(press.body, "background"), "var(--f-target-click)");
  }

  const danger = one("controls", "Danger button", rule => rule.selector.trim() === ".f-button-danger");
  const dangerHover = one("controls", "Danger button hover", rule => hasSelector(rule, ".f-button-danger:hover") && !rule.selector.includes(":active"));
  const dangerPress = one("controls", "Danger button press", rule => hasSelector(rule, ".f-button-danger:active"));
  assert.equal(property(danger.body, "background"), "var(--f-danger)");
  assert.equal(property(danger.body, "color"), "var(--f-surface)");
  assert.equal(property(dangerHover.body, "background"), "var(--f-preview-danger)");
  assert.equal(property(dangerHover.body, "color"), "var(--f-surface-light)");
  assert.equal(property(dangerPress.body, "background"), "var(--f-click-danger)");
  assert.equal(property(dangerPress.body, "color"), "var(--f-surface-light)");

  const primarySelected = one("foundation", "Primary navigation selection", rule => rule.selector.trim() === ".f-primary-item.is-selected");
  const primarySelectedPress = one("foundation", "Primary navigation selected press", rule => rule.selector.includes(".f-primary-item") && rule.selector.includes(".is-selected") && rule.selector.includes(":active"));
  const primaryNavigationHover = one("foundation", "Primary navigation hover", rule => rule.selector.includes(".f-primary-item") && rule.selector.includes(":hover") && !rule.selector.includes(".is-selected"));
  const primaryNavigationPress = one("foundation", "Primary navigation press", rule => rule.selector.includes(".f-primary-item") && rule.selector.includes(":active") && !rule.selector.includes(".is-selected"));
  assert.equal(property(primarySelected.body, "background"), "var(--f-accent)");
  assert.equal(property(primarySelected.body, "color"), "var(--f-surface)!important");
  assert.equal(property(primarySelectedPress.body, "background"), "var(--f-accent-click)");
  assert.equal(property(primarySelectedPress.body, "color"), "var(--f-surface)!important");
  assert.equal(property(primaryNavigationHover.body, "background"), "var(--f-preview-dark)");
  assert.equal(property(primaryNavigationPress.body, "background"), "var(--f-click-dark)");
  const foundationRules = rules(source.foundation);
  assert.ok(foundationRules.findIndex(rule => rule.selector === primarySelected.selector) > foundationRules.findIndex(rule => rule.selector === primaryNavigationHover.selector),
    "Primary selection does not win over its generic hover rule.");
  assert.ok(foundationRules.findIndex(rule => rule.selector === primarySelectedPress.selector) > foundationRules.findIndex(rule => rule.selector === primaryNavigationPress.selector),
    "Primary selected press does not win over the generic chrome press rule.");

  const secondarySelected = one("foundation", "Secondary navigation selection", rule => rule.selector.trim() === ".f-secondary-item.is-selected");
  const secondarySelectedPress = one("foundation", "Secondary navigation selected press", rule => rule.selector.includes(".f-secondary-item") && rule.selector.includes(".is-selected") && rule.selector.includes(":active"));
  const secondaryHover = one("foundation", "Secondary navigation hover", rule => rule.selector.includes(".f-secondary-item") && rule.selector.includes(":hover") && !rule.selector.includes(".is-selected"));
  const secondaryPress = one("foundation", "Secondary navigation press", rule => rule.selector.includes(".f-secondary-item") && rule.selector.includes(":active") && !rule.selector.includes(".is-selected"));
  assert.equal(property(secondarySelected.body, "background"), "var(--f-primary)");
  assert.equal(property(secondarySelected.body, "color"), "var(--f-surface)!important");
  assert.equal(property(secondarySelectedPress.body, "background"), "var(--f-primary-click)");
  assert.equal(property(secondarySelectedPress.body, "color"), "var(--f-surface)!important");
  assert.equal(property(secondaryHover.body, "background"), "var(--f-target-preview)");
  assert.equal(property(secondaryPress.body, "background"), "var(--f-target-click)");
  assert.ok(foundationRules.findIndex(rule => rule.selector === secondarySelected.selector) > foundationRules.findIndex(rule => rule.selector === secondaryHover.selector),
    "Secondary selection does not win over its generic hover rule.");
  assert.ok(foundationRules.findIndex(rule => rule.selector === secondarySelectedPress.selector) > foundationRules.findIndex(rule => rule.selector === secondaryPress.selector),
    "Secondary selected press does not win over the generic surface press rule.");

  const chromeMenuHover = one("foundation", "Title-bar menu hover", rule => rule.selector.includes(".f-action-menu:hover") && rule.selector.includes(".f-menu-trigger-label"));
  const chromeMenuPress = one("foundation", "Title-bar menu press", rule => rule.selector.includes(".f-menu-trigger-label:active"));
  assert.equal(property(chromeMenuPress.body, "background"), "var(--f-click-dark)");
  const hoverArm = chromeMenuHover.selector.split(",").find(selector => selector.includes(".f-action-menu:hover"));
  const guardedHover = hoverArm?.includes(".f-menu-trigger-label:not(:active)");
  const strongerPress = foundationRules.some(rule => rule.selector.includes(".f-action-menu:hover")
    && rule.selector.includes(".f-menu-trigger-label:active") && property(rule.body, "background") === "var(--f-click-dark)");
  assert.ok(guardedHover || strongerPress, "The higher-specificity menu-parent hover selector masks the trigger's deep-click color.");

  const dropdownSelected = one("dropdown", "Dropdown selection", rule => rule.selector.trim() === ".f-dropdown-panel .f-dropdown-item.is-selected");
  const dropdownHover = one("dropdown", "Dropdown hover", rule => rule.selector.includes(".f-dropdown-item:hover") && !rule.selector.includes(".is-selected"));
  const dropdownPress = one("dropdown", "Dropdown press", rule => rule.selector.includes(".f-dropdown-item:active") && !rule.selector.includes(".is-selected"));
  assert.equal(property(dropdownSelected.body, "color"), "var(--f-text)");
  assert.equal(property(dropdownSelected.body, "background"), "transparent");
  assert.equal(property(dropdownHover.body, "background"), "var(--f-target-preview)");
  assert.equal(property(dropdownPress.body, "background"), "var(--f-target-click)");
  const filledMultiSelectOptionRows = rules(await readFile(join(designCssRoot,"multi-select-box.css"),"utf8")).filter(rule => rule.selector.includes(".f-multi-select-panel")
    && !rule.selector.includes("input") && ![undefined, "transparent"].includes(property(rule.body, "background")));
  assert.deepEqual(filledMultiSelectOptionRows.map(rule => rule.selector), [], "Display-option rows gained a persistent fill.");

  const dangerGrid = one("uniform-grid", "Danger uniform grid", rule => rule.selector.includes(".f-uniform-grid-variant-danger"));
  assert.equal(dangerGrid.declarations.get("--f-uniform-cell-background"), "var(--f-danger)");
  assert.equal(dangerGrid.declarations.get("--f-uniform-cell-foreground"), "var(--f-surface)");
  assert.equal(dangerGrid.declarations.get("--f-uniform-cell-hover"), "var(--f-preview-danger)");
  assert.equal(dangerGrid.declarations.get("--f-uniform-cell-click"), "var(--f-click-danger)");
  assert.equal(dangerGrid.declarations.get("--f-uniform-cell-interaction-foreground"), "var(--f-surface-light)");
  const gridHover = one("uniform-grid", "Uniform-grid hover", rule => rule.selector.includes(".f-uniform-grid-button:hover"));
  const gridPress = one("uniform-grid", "Uniform-grid press", rule => rule.selector.includes(".f-uniform-grid-button:active"));
  assert.equal(property(gridHover.body, "background"), "var(--f-uniform-cell-hover,var(--f-target-preview))");
  assert.equal(property(gridPress.body, "background"), "var(--f-uniform-cell-click,var(--f-target-click))");
});

test("Notice has one semantic foreground and background contract without retired status aliases", async () => {
  const source = await readFile(join(designCssRoot, "controls.css"), "utf8");
  const mappings = [
    ["information", "info-text", "surface"],
    ["success", "surface", "primary"],
    ["warning", "warning-ink", "warning-background"],
    ["error", "surface", "danger"],
    ["subtle", "muted", "display-board"],
  ];
  for (const [severity, foreground, background] of mappings) {
    const semantic = matchingRule(source, `${severity} semantic mapping`, rule => rule.selector === `.f-notice-${severity}`);
    assert.equal(semantic.declarations.get("--f-notice-text"), `var(--f-${foreground})`);
    assert.equal(semantic.declarations.get("--f-notice-background"), `var(--f-${background})`);
  }
  const notice = matchingRule(source, "Notice base", rule => rule.selector === ".f-notice");
  assert.equal(property(notice.body, "color"), "var(--f-notice-text,var(--f-info-text))");
  assert.equal(property(notice.body, "background"), "var(--f-notice-background,var(--f-surface))");
  assert.ok(!notice.declarations.has("--f-notice-text") && !notice.declarations.has("--f-notice-background"),
    "The Notice base overrides its explicit severity mapping.");
  const files = await filesUnder(designCssRoot, new Set([".css"]));
  for (const path of files) {
    const css = await readFile(path, "utf8");
    for (const rule of rules(css)) {
      for (const selector of selectorArms(rule.selector))
        assert.doesNotMatch(selector, /^\.notice-(?:info|success|warning|error|subtle)(?![\w-])/,
          `${relative(repositoryRoot, path)} keeps a retired StatusNotice selector.`);
    }
    if (path === join(designCssRoot, "controls.css")) continue;
    for (const rule of rules(css)) {
      if (/\.f-notice-(?:information|success|warning|error|subtle)\b/.test(rule.selector)) {
        assert.ok(!property(rule.body, "color") && !property(rule.body, "background")
          && !rule.declarations.has("--f-notice-text") && !rule.declarations.has("--f-notice-background"),
          `${relative(repositoryRoot, path)} overrides the shared semantic Notice colors.`);
      }
    }
  }
});

test("components do not restore per-subtree themes", async () => {
  const sourceFiles = await filesUnder(blazorRoot, new Set([".cs", ".razor", ".css"]));
  const violations = [];
  for (const path of sourceFiles) {
    const source = await readFile(path, "utf8");
    if (/ThemeScope|LocalTheme|local-theme|data-theme/i.test(source)) violations.push(relative(repositoryRoot, path).replaceAll("\\", "/"));
  }
  assert.deepEqual(violations, [], `Local theme mechanisms remain:\n${violations.join("\n")}`);
});

test("page titles use 50px expanded and reserve 38px for actual compact headings", async () => {
  const foundation = await readFile(foundationPath, "utf8");
  const tokens = declarations(blockFor(withoutComments(foundation), ":root, .f-root"));
  assert.equal(tokens.get("--f-type-page"), "50px");
  assert.equal(tokens.get("--f-type-page-compact"), "38px");
  const paths = await filesUnder(designCssRoot, new Set([".css"]));
  const compactFiles = new Set();
  for (const path of paths) {
    for (const rule of rules(await readFile(path, "utf8"))) {
      const size = property(rule.body, "font-size");
      if (!size) continue;
      const selectors = selectorArms(rule.selector);
      const compact = selector => /\[data-(?:compact-heading|heading-mode\s*=\s*["']?compact["']?)\]/.test(selector);
      const isTitle = selector => /\.(?:f-)?page-heading\b/.test(selector) && /\bh1\s*$/.test(selector);
      const normalized = size.replaceAll(/\s/g, "");
      if (selectors.every(selector => isTitle(selector) && compact(selector))) {
        assert.match(normalized, /^var\(--f-type-page-compact(?:,38px)?\)$/, `${relative(repositoryRoot, path)} compact title size`);
        compactFiles.add(relative(designCssRoot, path).replaceAll("\\", "/"));
      } else if (selectors.every(isTitle)) {
        assert.match(normalized, /^var\(--f-type-page(?:,50px)?\)$/, `${relative(repositoryRoot, path)} expanded title size`);
      }
      if (/38px|--f-type-page-compact/.test(size)) {
        assert.ok(selectors.every(selector => isTitle(selector) && compact(selector)),
          `38px escaped compact page titles: ${relative(repositoryRoot, path)} ${rule.selector}`);
      }
    }
  }
  assert.deepEqual([...compactFiles].sort(), ["foundation.css"]);
});

test("static SVG assets only embed the approved Primary and Surface paints", async () => {
  const galleryRoot = join(repositoryRoot, "src/Gallery.Flourish.Blazor");
  const svgFiles = [...await filesUnder(blazorRoot, new Set([".svg"])), ...await filesUnder(galleryRoot, new Set([".svg"]))];
  const allowed = new Set(["#FFFFFF", "#153A32"]);
  const violations = [];
  for (const path of svgFiles) {
    const source = withoutComments(await readFile(path, "utf8"));
    for (const match of source.matchAll(/#[0-9a-f]{3,8}\b/gi)) {
      const literal = match[0].toUpperCase();
      if (!allowed.has(literal)) violations.push(`${relative(repositoryRoot, path)}: ${literal}`);
    }
  }
  assert.deepEqual(violations, [], `Static SVG paint bypasses:\n${violations.join("\n")}`);
});

test("default reading, selected and preview foregrounds stay readable in both modes", async () => {
  const css = withoutComments(await readFile(foundationPath, "utf8"));
  const modes = [declarations(blockFor(css, ":root, .f-root")), declarations(blockFor(css, ".f-root.f-theme-dark"))];
  const seeds = declarations(blockFor(css, ":root, .f-root"));
  const luminance = hex => {
    const channels = [1, 3, 5].map(offset => Number.parseInt(hex.slice(offset, offset + 2), 16) / 255)
      .map(value => value <= .04045 ? value / 12.92 : ((value + .055) / 1.055) ** 2.4);
    return channels[0] * .2126 + channels[1] * .7152 + channels[2] * .0722;
  };
  for (const [index, mode] of modes.entries()) {
    const color = role => {
      let value = mode.get(`--f-${role}`) ?? seeds.get(`--f-${role}`);
      const visited = new Set();
      while (value?.startsWith("var(")) {
        const property = value.slice(4, -1);
        assert.ok(!visited.has(property), `Circular palette alias at ${property}.`);
        visited.add(property);
        value = mode.get(property) ?? seeds.get(property);
      }
      assert.match(value, /^#[0-9A-F]{6}$/i, `--f-${role} does not resolve to a palette color.`);
      return value;
    };
    const pairs = [["text", "canvas"], ["text", "surface"], ["text", "display-board"], ["muted", "canvas"],
      ["surface", "primary"], ["surface", "accent"], ["surface", "danger"], ["text", "surface-preview"],
      ["text", "surface-click"], ["surface", "primary-preview"], ["surface", "primary-click"],
      ["surface", "accent-click"], ["chrome-ink", "click-dark"],
      ["info-text", "surface"], ["warning-ink", "warning-background"],
      ["surface-light", "preview-danger"], ["surface-light", "click-danger"]];
    for (const [foreground, background] of pairs) {
      const values = [luminance(color(foreground)), luminance(color(background))];
      const ratio = (Math.max(...values) + .05) / (Math.min(...values) + .05);
      assert.ok(ratio >= 4.5, `${index ? "Dark" : "Light"} ${foreground}/${background}: ${ratio.toFixed(2)}:1`);
    }
  }
});
