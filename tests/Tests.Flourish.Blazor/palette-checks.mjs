import assert from "node:assert/strict";
import { readdir, readFile } from "node:fs/promises";
import { dirname, extname, join, relative, resolve } from "node:path";
import test from "node:test";
import { fileURLToPath } from "node:url";

const testDirectory = dirname(fileURLToPath(import.meta.url));
const repositoryRoot = resolve(testDirectory, "../..");
const blazorRoot = join(repositoryRoot, "src/Flourish.Blazor");
const designCssRoot = join(blazorRoot, "Flourish.Blazor.Design/wwwroot");
const foundationPath = join(designCssRoot, "foundation.css");
const aliasesPath = join(designCssRoot, "theme-aliases.css");

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

test("foundation exposes the fifteen-role palette to documents and Flourish surfaces", async () => {
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

test("palette roles have one source and aliases reference declared tokens", async () => {
  const cssFiles = await filesUnder(designCssRoot, new Set([".css"]));
  const foundation = withoutComments(await readFile(foundationPath, "utf8"));
  const aliases = withoutComments(await readFile(aliasesPath, "utf8"));
  const allDesign = (await Promise.all(cssFiles.map(path => readFile(path, "utf8")))).map(withoutComments).join("\n");
  const declaredFoundation = new Set([...foundation.matchAll(/(--f-[\w-]+)\s*:/g)].map(match => match[1]));
  const legacyRoles = declarations(aliases);
  assert.equal(legacyRoles.get("--accent-fill-hover"), "var(--f-primary-preview)");
  assert.equal(legacyRoles.get("--chrome-hover"), "var(--f-preview-dark)");
  assert.equal(legacyRoles.get("--line"), "var(--f-border)");
  assert.equal(legacyRoles.get("--focus"), "var(--f-accent)");

  for (const role of roles) {
    const property = `--f-${role}`;
    assert.match(allDesign, new RegExp(`var\\(\\s*${property.replaceAll("-", "\\-")}\\b`), `${property} has no consumer.`);
    for (const path of cssFiles.filter(path => path !== foundationPath)) {
      const css = withoutComments(await readFile(path, "utf8"));
      assert.doesNotMatch(css, new RegExp(`${property.replaceAll("-", "\\-")}\\s*:`), `${property} is redefined by ${relative(repositoryRoot, path)}.`);
    }
  }
  for (const reference of aliases.matchAll(/var\(\s*(--f-[\w-]+)/g)) {
    assert.ok(declaredFoundation.has(reference[1]), `theme-aliases.css references undeclared ${reference[1]}.`);
  }
  assert.doesNotMatch(allDesign, /--f-border-strong\s*:|var\(\s*--f-border-strong\b/);
  assert.doesNotMatch(allDesign, /--f-focus\s*:|var\(\s*--f-focus\s*[,)]/);
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
    const hover = one("controls", `${variant} button hover`, rule => hasSelector(rule, `.f-button-${variant}:hover`) && !rule.selector.includes(":active"));
    const press = one("controls", `${variant} button press`, rule => hasSelector(rule, `.f-button-${variant}:active`));
    assert.equal(property(hover.body, "background"), "var(--f-target-preview)");
    assert.equal(property(press.body, "background"), "var(--f-target-click)");
  }

  const danger = one("controls", "Danger button", rule => rule.selector.trim() === ".f-button-danger");
  const dangerHover = one("controls", "Danger button hover", rule => hasSelector(rule, ".f-button-danger:hover") && !rule.selector.includes(":active"));
  const dangerPress = one("controls", "Danger button press", rule => hasSelector(rule, ".f-button-danger:active"));
  assert.equal(property(danger.body, "background"), "var(--f-danger)");
  assert.equal(property(danger.body, "color"), "var(--f-surface)");
  assert.equal(property(dangerHover.body, "background"), "var(--f-primary-preview)");
  assert.equal(property(dangerHover.body, "color") ?? property(danger.body, "color"), "var(--f-surface)");
  assert.equal(property(dangerPress.body, "background"), "var(--f-primary-click)");
  assert.equal(property(dangerPress.body, "color") ?? property(danger.body, "color"), "var(--f-surface)");

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
  const filledDisplayOptionRows = rules(source.data).filter(rule => rule.selector.includes(".f-data-display-options")
    && !rule.selector.includes("input") && ![undefined, "transparent"].includes(property(rule.body, "background")));
  assert.deepEqual(filledDisplayOptionRows.map(rule => rule.selector), [], "Display-option rows gained a persistent fill.");

  const dangerGrid = one("uniform-grid", "Danger uniform grid", rule => rule.selector.includes(".f-uniform-grid-variant-danger"));
  assert.equal(dangerGrid.declarations.get("--f-uniform-cell-background"), "var(--f-danger)");
  assert.equal(dangerGrid.declarations.get("--f-uniform-cell-foreground"), "var(--f-surface)");
  assert.equal(dangerGrid.declarations.get("--f-uniform-cell-hover"), "var(--f-primary-preview)");
  assert.equal(dangerGrid.declarations.get("--f-uniform-cell-click"), "var(--f-primary-click)");
  const gridHover = one("uniform-grid", "Uniform-grid hover", rule => rule.selector.includes(".f-uniform-grid-button:hover"));
  const gridPress = one("uniform-grid", "Uniform-grid press", rule => rule.selector.includes(".f-uniform-grid-button:active"));
  assert.equal(property(gridHover.body, "background"), "var(--f-uniform-cell-hover,var(--f-target-preview))");
  assert.equal(property(gridPress.body, "background"), "var(--f-uniform-cell-click,var(--f-target-click))");
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
      const selectors = rule.selector.split(",").map(selector => selector.trim());
      const compact = selector => /\[data-(?:compact-heading|heading-mode\s*=\s*["']?compact["']?)\]/.test(selector);
      const isTitle = selector => /\.(?:f-)?page-heading\b/.test(selector) && /\bh1\s*$/.test(selector);
      const normalized = size.replaceAll(/\s/g, "");
      if (selectors.every(selector => isTitle(selector) && compact(selector))) {
        assert.equal(normalized, "var(--f-type-page-compact,38px)", `${relative(repositoryRoot, path)} compact title size`);
        compactFiles.add(relative(designCssRoot, path).replaceAll("\\", "/"));
      } else if (selectors.every(isTitle)) {
        assert.equal(normalized, "var(--f-type-page,50px)", `${relative(repositoryRoot, path)} expanded title size`);
      }
      if (/38px|--f-type-page-compact/.test(size)) {
        assert.ok(selectors.every(selector => isTitle(selector) && compact(selector)),
          `38px escaped compact page titles: ${relative(repositoryRoot, path)} ${rule.selector}`);
      }
    }
  }
  assert.deepEqual([...compactFiles].sort(), ["foundation.css", "layout.css", "patterns/content-surface.css", "patterns/navigation-surface.css"]);
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
      ["info-text", "surface"], ["warning-ink", "warning-background"]];
    for (const [foreground, background] of pairs) {
      const values = [luminance(color(foreground)), luminance(color(background))];
      const ratio = (Math.max(...values) + .05) / (Math.min(...values) + .05);
      assert.ok(ratio >= 4.5, `${index ? "Dark" : "Light"} ${foreground}/${background}: ${ratio.toFixed(2)}:1`);
    }
  }
});
