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

test("Button content grows with vertical padding while short labels and icon-only buttons keep standard size", () => {
  const root = element("div", ["f-root"]);
  for (const tag of ["button", "a"]) for (const variant of ["primary", "secondary", "danger", "quiet", "underline", "elevated"])
  for (const structured of [false, true]) for (const state of ["available", "disabled", "busy"]) {
    const attributes = state === "disabled" ? tag === "button" ? { disabled: "" } : { "aria-disabled": "true" }
      : state === "busy" ? { "aria-busy": "true", "aria-disabled": "true" } : {};
    const action = element(tag, ["f-button", `f-button-${variant}`, ...(structured ? ["f-button-structured"] : [])], root, [], attributes);
    const [blockPadding] = cascade(action, "padding").split(/\s+/).map(parseFloat);
    const minimum = parseFloat(cascade(action, "min-height"));
    const fontSize = parseFloat(cascade(action, "font-size"));
    const lineHeight = parseFloat(cascade(action, "line-height"));
    assert.ok(blockPadding > 0, `${variant}/${structured}/${state}: content lost its vertical breathing room`);
    assert.ok([undefined, "auto"].includes(cascade(action, "height")) && [undefined, "none"].includes(cascade(action, "max-height")),
      "Ordinary Button must grow with content instead of imposing a fixed height");
    assert.equal(minimum, 48);
    assert.ok(fontSize * lineHeight + blockPadding * 2 + 2 <= minimum, "A short label exceeded the standard minimum");
    const icon = element("span", ["f-icon"], action);
    assert.ok(parseFloat(resolvedProperty(icon, "font-size")) + blockPadding * 2 + 2 <= minimum,
      "The standard inline icon unnecessarily increased a one-line button's height");
  }
  for (const tag of ["button", "a"]) {
    const iconOnly = element(tag, ["f-button", "f-button-primary", "f-button-icon"], root);
    assert.equal(cascade(iconOnly, "width"), "48px");
    assert.equal(cascade(iconOnly, "height"), "48px");
    assert.equal(cascade(iconOnly, "padding"), "0");
    const menuItem = element(tag, ["f-button", "f-menu-item"], element("div", ["f-menu-panel"], root));
    assert.equal(cascade(menuItem, "padding"), "8px 10px", "Menu items lost their own spacing");
  }
});

test("Native and generated ActionMenu triggers share hover press expanded and unavailable feedback", () => {
  for (const theme of ["light", "dark"]) {
    const root = element("div", ["f-root", `f-theme-${theme}`]);
    for (const native of [false, true]) for (const enhanced of [false, true]) {
      const container = element(native ? "details" : "span", ["f-action-menu"], root, [], native
        ? { "data-f-native-menu": "", ...(enhanced ? { "data-f-enhanced": "true" } : {}) } : {});
      for (const states of [[], ["hover"], ["active"], ["hover", "active"], ["focus-visible"]]) {
        const trigger = element(native ? "summary" : "button", ["f-menu-trigger"], container, states, { "aria-disabled": "false" });
        const label = `${theme}/${trigger.tag}/${enhanced}/${states}`;
        assert.equal(resolvedProperty(trigger, "background"), states.includes("active") ? resolvedProperty(root, "--f-target-click")
          : states.includes("hover") ? resolvedProperty(root, "--f-target-preview") : "transparent", `${label}: pointer fill`);
        if (states.includes("focus-visible")) assert.equal(cascade(trigger, "outline"), "3px solid var(--f-accent)", `${label}: keyboard focus`);
      }
      for (const disabled of [false, true]) {
        const attributes = { "aria-expanded": "true", "aria-disabled": String(disabled) };
        const trigger = element(native ? "summary" : "button", ["f-menu-trigger"], container, ["hover", "active"], attributes);
        assert.equal(resolvedProperty(trigger, "background"), disabled ? "transparent" : resolvedProperty(root, "--f-target-click"));
        if (disabled) { assert.equal(cascade(trigger, "opacity"), ".55"); assert.equal(cascade(trigger, "cursor"), "not-allowed"); }
      }
      const trigger = element(native ? "summary" : "button", ["f-menu-trigger"], container, [], { "aria-expanded": "true" });
      assert.equal(resolvedProperty(trigger, "background"), resolvedProperty(root, "--f-target-preview"));
    }
    const disclosure = element("details", ["f-action-menu", "f-dropdown-surface"], root, [], { "data-f-native-menu": "", open: "" });
    const trigger = element("summary", ["f-menu-trigger"], disclosure);
    assert.equal(resolvedProperty(trigger, "background"), resolvedProperty(root, "--f-target-preview"), "SSR details open paint");
  }
});

test("UniformGrid actions share only Danger Elevated and FilledElevated paint without interaction borders", () => {
  const uniformCss = design.find(css => css.includes(".f-uniform-grid-variant-filled-elevated"));
  assert.ok(uniformCss, "The production stylesheet did not import FilledElevated");
  assert.doesNotMatch(uniformCss, /\.f-uniform-(?:grid-)?variant-(?:filled|outlined)(?![\w-])/,
    "A retired UniformGrid variant selector remains");
  const variants = {
    elevated: { background: "--f-surface", ink: "--f-text", interactionInk: "--f-text", hover: "--f-target-preview", active: "--f-target-click", shadow: true },
    "filled-elevated": { background: "--f-primary", ink: "--f-primary-ink", interactionInk: "--f-primary-ink", hover: "--f-primary-preview", active: "--f-primary-click", shadow: true },
    danger: { background: "--f-danger", ink: "--f-surface", interactionInk: "--f-surface-light", hover: "--f-preview-danger", active: "--f-click-danger", shadow: false },
  };
  const interactions = [[], ["hover"], ["active"], ["hover", "active"], ["focus-visible"], ["hover", "focus-visible"], ["active", "focus-visible"]];
  for (const theme of ["light", "dark"]) {
    const root = element("div", ["f-root", `f-theme-${theme}`]);
    const form = element("div", ["f-form-actions"], root);
    const contexts = [{ parent: root, variant: null, expected: "elevated" }, { parent: form, variant: null, expected: "elevated" }];
    for (const variant of Object.keys(variants)) {
      const grid = element("div", ["f-uniform-grid", `f-uniform-grid-variant-${variant}`], root);
      contexts.push({ parent: root, variant, expected: variant }, { parent: form, variant, expected: variant }, { parent: grid, variant: null, expected: variant });
      // Local choice must beat inherited grid custom properties, including a
      // surface-colored secondary inside a FilledElevated primary group.
      const override = variant === "filled-elevated" ? "elevated" : "filled-elevated";
      contexts.push({ parent: grid, variant: override, expected: override });
      assert.equal(cascade(grid, "filter"), variants[variant].shadow ? "drop-shadow(var(--f-shadow-control))" : undefined,
        `${variant}: the grid lost the occupied-cell elevation`);
    }
    for (const { parent, variant, expected } of contexts) for (const tag of ["button", "a"]) {
      const appearances = [
        ...interactions.map(states => ({ states, attributes: { "aria-disabled": "false", "aria-busy": "false" }, unavailable: false })),
        ...["disabled", "busy"].map(availability => ({ states: ["hover", "active", ...(tag === "button" ? ["disabled"] : [])],
          attributes: { ...(tag === "button" ? { disabled: "" } : { "aria-disabled": "true" }), "aria-busy": String(availability === "busy") }, unavailable: true })),
      ];
      for (const { states, attributes, unavailable } of appearances) {
        // UniformGridButton delegates to the ordinary Primary Button. Its
        // production classes must not override the grid variant's actual paint.
        const action = element(tag, ["f-button", "f-button-primary", "f-uniform-cell", "f-uniform-grid-button",
          ...(variant ? [`f-uniform-variant-${variant}`] : [])], parent, states, attributes);
        const label = `${theme}/${tag}/${[...parent.classes]}/${variant ?? "inherited"}/${states}/${attributes["aria-busy"]}`;
        const contract = variants[expected];
        const interaction = !unavailable && (states.includes("active") ? "active" : states.includes("hover") ? "hover" : null);
        assert.equal(resolvedProperty(action, "background"), resolvedProperty(root, interaction ? contract[interaction] : contract.background), `${label}: fill`);
        assert.equal(resolvedProperty(action, "color"), resolvedProperty(root, interaction ? contract.interactionInk : contract.ink), `${label}: foreground`);
        assert.equal(resolvedProperty(action, "border"), "1px solid transparent", `${label}: border geometry`);
        assert.equal(resolvedProperty(action, "border-color") ?? "transparent", "transparent", `${label}: visible border`);
        assert.equal(resolvedProperty(action, "box-shadow"), !unavailable && interaction !== "active" && contract.shadow
          ? resolvedProperty(root, "--f-shadow-control") : "none", `${label}: elevation`);
        if (states.includes("focus-visible")) {
          assert.equal(cascade(action, "outline"), "3px solid var(--f-accent)", `${label}: keyboard focus`);
          assert.equal(cascade(action, "outline-offset"), parent.classes.has("f-uniform-grid") || parent.classes.has("f-form-actions") ? "-5px" : "3px", `${label}: focus placement`);
        }
        if (unavailable) {
          assert.equal(cascade(action, "opacity"), ".55", `${label}: unavailable feedback`);
          const spinner = element("span", ["f-spinner"], action);
          assert.equal(cascade(spinner, "border"), "2px solid currentColor", `${label}: busy spinner still follows action ink`);
          assert.equal(cascade(spinner, "position"), "absolute", `${label}: spinner changes cell geometry`);
        }
      }
    }
  }
});

test("FormActions keeps shared Elevated defaults and visible group elevation without divider borders or clipped focus", () => {
  for (const theme of ["light", "dark"]) {
    const root = element("div", ["f-root", `f-theme-${theme}`]);
    const actions = element("div", ["f-form-actions"], root);
    assert.equal(cascade(actions, "background"), "transparent", `${theme}: the container paints divider borders`);
    assert.equal(cascade(actions, "filter"), "drop-shadow(var(--f-shadow-control))", `${theme}: clipped child shadows lost group elevation`);
    assert.equal(cascade(actions, "overflow"), "hidden", `${theme}: group rounding changed`);
    assert.equal(cascade(actions, "border-radius"), "16px");
    assert.equal(cascade(actions, "gap"), "1px");
    assert.equal(cascade(actions, "display"), "grid");
    assert.equal(cascade(actions, "grid-template-columns"), "repeat(var(--f-action-columns,2),minmax(0,1fr))");
    assert.equal(cascade(actions, "grid-template-columns", 600), "repeat(var(--f-action-narrow-columns,2),minmax(0,1fr))");
    const standalone = element("button", ["f-button", "f-button-primary", "f-uniform-cell", "f-uniform-grid-button"], root);
    const defaultAction = element("button", [...standalone.classes], actions);
    for (const property of ["background", "color", "box-shadow", "border"])
      assert.equal(resolvedProperty(defaultAction, property), resolvedProperty(standalone, property), `${theme}: FormActions changes default UGB ${property}`);
    for (const variant of [null, "elevated", "filled-elevated", "danger"]) {
      const action = element("button", [...standalone.classes, ...(variant ? [`f-uniform-variant-${variant}`] : [])], actions, ["hover", "focus-visible"]);
      assert.equal(cascade(action, "min-height"), "96px", `${variant}: form height changed`);
      assert.equal(cascade(action, "max-width"), "none");
      assert.equal(cascade(action, "border-radius"), "0");
      assert.equal(cascade(action, "border-color"), "transparent", `${variant}: hover paints a border`);
      assert.equal(cascade(action, "outline"), "3px solid var(--f-accent)");
      assert.equal(cascade(action, "outline-offset"), "-5px", `${variant}: group clips the focus indicator`);
      assert.equal(cascade(action, "box-shadow"), "var(--f-uniform-cell-shadow,var(--f-shadow-control))");
      assert.equal(resolvedProperty(action, "box-shadow"), variant === "danger" ? "none" : resolvedProperty(root, "--f-shadow-control"));
    }
  }
});

test("Quiet buttons and links retain transparent borders through hover press and keyboard focus in every surface", () => {
  const root = element("div", ["f-root"]);
  const contexts = [
    { parent: root, ink: "var(--f-accent-text)", hoverInk: "var(--f-text)", preview: "var(--f-target-preview)", click: "var(--f-target-click)" },
    { parent: element("div", ["f-access-form-surface", "f-access-form-primary"], root), ink: "var(--f-primary-ink)", hoverInk: "var(--f-primary-ink)", preview: "var(--f-primary-preview)", click: "var(--f-primary-click)" },
    { parent: element("header", ["shell-header"], root), ink: "inherit", hoverInk: "inherit", preview: "var(--f-target-preview)", click: "var(--f-target-click)" },
  ];
  for (const { parent, ink, hoverInk, preview, click } of contexts) {
    for (const tag of ["button", "a"]) {
      for (const states of [[], ["hover"], ["active"], ["hover", "active"], ["focus-visible"], ["hover", "focus-visible"], ["active", "focus-visible"]]) {
        const action = element(tag, ["f-button", "f-button-quiet"], parent, states, { "aria-disabled": "false" });
        const label = `${tag}/${[...parent.classes]}/${states}`;
        assert.equal(cascade(action, "border"), "1px solid transparent", `${label}: Quiet border geometry changed`);
        assert.equal(cascade(action, "border-color") ?? "transparent", "transparent", `${label}: interaction painted a Quiet border`);
        assert.equal(cascade(action, "background"), states.includes("active") ? click : states.includes("hover") ? preview : "transparent",
          `${label}: border removal changed hover or pressed fill`);
        assert.equal(cascade(action, "color"), states.includes("active") || states.includes("hover") ? hoverInk : ink,
          `${label}: border removal changed foreground`);
        if (states.includes("focus-visible")) {
          assert.match(cascade(action, "outline"), /solid var\(--f-accent\)/, `${label}: keyboard focus lost its outline`);
          assert.equal(cascade(action, "outline-offset"), "3px", `${label}: keyboard outline moved onto the border`);
        }
      }
    }
  }
});

test("Outlined buttons keep their borders while Quiet pointer feedback loses them", () => {
  const root = element("div", ["f-root"]);
  for (const parent of [root, element("div", ["f-access-form-primary"], root), element("header", ["shell-header"], root)]) {
    for (const tag of ["button", "a"]) {
      for (const states of [[], ["hover"], ["active"], ["hover", "active"], ["focus-visible"]]) {
        const action = element(tag, ["f-button", "f-button-secondary"], parent, states, { "aria-disabled": "false" });
        assert.equal(cascade(action, "border-color"), "var(--f-border)", `${tag}/${[...parent.classes]}/${states}: Outlined lost its border`);
      }
    }
  }
});

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

test("Ordinary Cards compose account headings and selectable identifiers through the existing Card styles", () => {
  const root = element("div", ["f-root"]);
  const body = element("div", ["f-page-body"], root);
  const card = element("article", ["f-card"], body);
  const heading = element("h3", [], card);
  const identifier = element("code", ["f-copy-text"], card);
  for (const viewport of [1200, 760, 360]) {
    assert.equal(cascade(card, "padding", viewport), "24px");
    assert.equal(cascade(card, "min-width", viewport), "0");
    assert.equal(cascade(card, "overflow-wrap", viewport), "anywhere");
    assert.equal(cascade(card, "background", viewport), "var(--f-primary)");
    assert.equal(inherited(heading, "color"), "var(--f-primary-ink)");
    assert.equal(cascade(identifier, "user-select", viewport), "all");
    assert.equal(cascade(identifier, "overflow-wrap", viewport), "anywhere");
    assert.equal(inherited(identifier, "color"), "var(--f-primary-ink)");
    assert.notEqual(cascade(heading, "position", viewport), "sticky");
    assert.notEqual(cascade(heading, "background", viewport), "var(--f-canvas)");
  }
});

test("Retired IdentityCard has no separate geometry or paint in either production stylesheet entry", () => {
  for (const css of [...framework, ...design]) assert.doesNotMatch(css, /f-identity-/);
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

// Resolve this fixture's inherited custom-property chain in the same theme and
// parent context. Balanced var() arguments retain nested fallbacks and shadow
// functions; this is a palette/cascade regression, not browser layout emulation.
function resolvedProperty(target, property) {
  function resolveValue(value, depth = 0) {
    if (value === undefined) return undefined;
    assert.ok(depth < 20, `Cyclic CSS variable while resolving ${property}`);
    let output = "", position = 0;
    for (;;) {
      const start = value.indexOf("var(", position);
      if (start < 0) return output + value.slice(position);
      output += value.slice(position, start);
      let end = start + 4, nesting = 1;
      while (end < value.length && nesting) { if (value[end] === "(") nesting++; else if (value[end] === ")") nesting--; end++; }
      assert.equal(nesting, 0, `Unbalanced CSS variable ${value}`);
      const arguments_ = value.slice(start + 4, end - 1);
      const comma = arguments_.indexOf(",");
      const name = (comma < 0 ? arguments_ : arguments_.slice(0, comma)).trim();
      const fallback = comma < 0 ? undefined : arguments_.slice(comma + 1).trim();
      const replacement = resolveValue(inherited(target, name) ?? fallback, depth + 1);
      assert.notEqual(replacement, undefined, `Undefined CSS variable ${name}`);
      output += replacement;
      position = end;
    }
  }
  return resolveValue(inherited(target, property));
}
