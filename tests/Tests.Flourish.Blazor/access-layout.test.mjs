import assert from "node:assert/strict";
import { readFile } from "node:fs/promises";
import { dirname, resolve } from "node:path";
import { fileURLToPath } from "node:url";
import test from "node:test";

const root = resolve(dirname(fileURLToPath(import.meta.url)), "../..");
const base = "src/Flourish.Blazor/";
const read = path => readFile(resolve(root, base + path), "utf8");
const framework = await read("Flourish.Blazor.Framework/wwwroot/access-layout.css");
const design = await read("Flourish.Blazor.Design/wwwroot/access-layout.css");
const navigation = await read("Flourish.Blazor.Framework/wwwroot/navigation-choices.css");
const navigationDesign = await read("Flourish.Blazor.Design/wwwroot/navigation-choices.css");
const presentation = await read("Flourish.Blazor.Framework/wwwroot/presentation/layout.css");
const presentationDesign = await read("Flourish.Blazor.Design/wwwroot/presentation.css");
const rule = (css, selector) => {
    const start = css.indexOf(selector + " {");
    assert.ok(start >= 0, `Missing ${selector}`);
    return css.slice(css.indexOf("{", start) + 1, css.indexOf("}", start));
};
const property = (css, key) => new RegExp(`(?:^|;)\\s*${key}\\s*:\\s*([^;]+)`).exec(css)?.[1].trim();

test("bounded operation host uses available height without viewport recentering or document flow", async () => {
    const host = rule(framework, ".f-access-layout");
    assert.equal(property(host, "height"), "100%");
    assert.equal(property(host, "min-height"), "0");
    assert.equal(property(host, "overflow"), "hidden");
    assert.equal(property(host, "align-items"), undefined);
    assert.doesNotMatch(host, /100[ds]?vh|translate|align-content:center/);
    assert.equal(property(rule(framework, ".f-access-layout-content"), "grid-template-rows"), "auto minmax(0,1fr)");
    assert.equal(property(rule(framework, ".f-access-layout-body"), "min-height"), "0");
    assert.equal(property(rule(framework, ".f-access-layout-body"), "overflow"), "auto");
    for (const [layer, name] of [["Framework", "framework.css"], ["Design", "design.css"]])
        assert.equal((await read(`Flourish.Blazor.${layer}/wwwroot/${name}`)).split("@import url('./access-layout.css');").length - 1, 1);
});

test("height-filled details grow naturally up to the body limit and hidden alternatives reserve no height", () => {
    const host = rule(navigation, ".f-navigation-choices-fill-height");
    assert.equal(property(host, "height"), "100%");
    assert.equal(property(host, "min-height"), "0");
    assert.equal(property(host, "align-items"), "start");
    const panels = rule(navigation, ".f-navigation-choices-fill-height > .f-navigation-choices-nav,.f-navigation-choices-fill-height > .f-navigation-choice-panel");
    assert.equal(property(panels, "max-height"), "100%");
    assert.equal(property(panels, "overflow"), "auto");
    assert.equal(property(panels, "height"), undefined);
    assert.equal(property(rule(navigation, ".f-navigation-choice-panel[hidden]"), "display"), "none!important");
    assert.equal(property(rule(navigation, ".f-navigation-choices-sidebar.f-navigation-choices-fill-height"), "grid-template-rows"), "minmax(0,1fr)");
});

test("narrow height-filled choices reserve navigation above a separately bounded details row", () => {
    const narrow = navigation.slice(navigation.lastIndexOf("@media(max-width:760px)"));
    assert.equal(property(rule(narrow, ".f-navigation-choices-sidebar.f-navigation-choices-fill-height"), "grid-template-rows"), "auto minmax(0,1fr)");
    assert.equal(property(rule(narrow, ".f-navigation-choices-sidebar.f-navigation-choices-fill-height > .f-navigation-choices-nav"), "grid-row"), "1");
    assert.equal(property(rule(narrow, ".f-navigation-choices-sidebar.f-navigation-choices-fill-height > .f-navigation-choice-panel"), "grid-row"), "2");
    assert.equal(property(rule(narrow, ".f-navigation-choices-sidebar.f-navigation-choices-fill-height .f-navigation-choices-links"), "grid-template-columns"), "repeat(var(--f-choice-columns,2),minmax(0,1fr))");
});

test("panel Sections suppress page padding even when an error Notice precedes them", () => {
    assert.equal(property(rule(navigationDesign, ".f-navigation-choice-panel > .f-section"), "padding-top"), "0");
    assert.doesNotMatch(navigationDesign, /\.f-section:first-child/);
    assert.equal(property(rule(presentationDesign, ".f-access-form-surface > .f-section"), "padding-top"), "0");
});

test("compact identity is horizontal and preserves ordinary artistic branding", () => {
    assert.equal(property(rule(presentation, ".f-logo-displayer-compact"), "grid-template-columns"), "auto minmax(0,1fr)");
    assert.equal(property(rule(presentation, ".f-logo-displayer-compact > .f-logo-displayer-logo"), "width"), "var(--f-logo-size,48px)");
    assert.equal(property(rule(presentation, ".f-logo-displayer-compact > .f-logo-displayer-title"), "grid-column"), "2");
    assert.equal(property(rule(presentation, ".f-logo-displayer-compact > .f-logo-displayer-description"), "grid-column"), "1/-1");
    assert.equal(property(rule(presentation, ".f-logo-displayer-logo"), "width"), "var(--f-logo-size,96px)");
    const shortLayout = presentation.slice(presentation.lastIndexOf("@media(max-height:520px)"));
    const shortLogo = property(rule(shortLayout, ".f-logo-displayer-compact > .f-logo-displayer-logo"), "width");
    assert.equal(shortLogo, "var(--f-logo-size,32px)");
    for (const fallback of ["96px", "48px", "32px"]) {
        const size = `var(--f-logo-size,${fallback})`;
        const resolve = explicit => explicit ?? /,([^)]*)\)/.exec(size)[1];
        assert.equal(resolve(null), fallback);
        assert.equal(resolve("96px"), "96px", "Explicit LogoSize was reduced by a layout/short-height fallback.");
    }
    assert.equal(property(rule(presentationDesign, ".f-logo-displayer > .f-logo-displayer-title"), "font-size"), "clamp(56px,8vw,80px)");
    assert.equal(property(rule(presentationDesign, ".f-logo-displayer-context"), "font-size"), "var(--f-type-h2,28px)");
    assert.equal(property(rule(presentationDesign, ".f-logo-displayer-context"), "margin-top"), "12px");
    assert.equal(property(rule(presentationDesign, ".f-logo-displayer-compact"), "margin-bottom"), "0");
    for (const match of presentationDesign.matchAll(/([^{}]+)\{([^{}]*)\}/g)) {
        if (!match[1].includes(".f-logo-displayer-compact")) continue;
        assert.equal(property(match[2], "font-size"), undefined, "Horizontal identity must inherit the ordinary title/context typography even on short screens.");
    }
    const ordinary = rule(presentationDesign, ".f-logo-displayer > .f-logo-displayer-title");
    assert.equal(property(ordinary, "overflow-wrap"), "anywhere");
    assert.equal(property(rule(presentation, ".f-logo-displayer-title"), "min-width"), "0");
});

test("narrow floating preferences have clearance while stable action rows keep their standard controls", () => {
    assert.equal(property(rule(design, ".f-content-surface-has-floating-actions > .content-stage > .f-access-layout"), "padding-block-start"), "80px");
    const actions = rule(design, ".f-access-layout-brand > :is(.f-access-actions,.f-inline-actions)");
    assert.equal(property(actions, "margin-block"), "0");
    assert.equal(property(actions, "justify-self"), "stretch");
    assert.doesNotMatch(design, /\.f-button|#[0-9a-f]{3,8}/i);
});

test("extremely short viewports retain a reachable scroll area rather than clipping submission", () => {
    const short = framework.slice(framework.indexOf("@media(max-height:360px)"));
    assert.equal(property(rule(short, ".f-access-layout-content"), "grid-template-rows"), "minmax(0,auto) minmax(48px,1fr)");
    assert.equal(property(rule(short, ".f-access-layout-brand"), "overflow"), "auto");
    assert.equal(property(rule(short, ".f-access-layout-body:has(> .f-navigation-choices-fill-height)"), "overflow"), "auto");
    const body = rule(short, ".f-access-layout-body > .f-navigation-choices-fill-height");
    assert.equal(property(body, "height"), "auto");
    assert.equal(property(body, "max-height"), "none");
    assert.equal(property(body, "overflow"), "visible");
});


test("decorative band media is isolated from natural height and cannot intercept foreground actions", () => {
    const media = rule(presentation, ".f-presentation-band-media");
    assert.equal(property(media, "position"), "absolute");
    assert.equal(property(media, "inset"), "0");
    assert.equal(property(media, "pointer-events"), "none");
    assert.equal(property(media, "z-index"), "0");
    const image = rule(presentation, ".f-presentation-band-media > img");
    assert.equal(property(image, "width"), "100%");
    assert.equal(property(image, "height"), "100%");
    assert.equal(property(image, "object-fit"), "cover");
    const foreground = rule(presentation, ".f-presentation-band-media + .f-content-container");
    assert.equal(property(foreground, "position"), "relative");
    assert.equal(property(foreground, "z-index"), "1");
    assert.equal(property(rule(presentation, ".f-presentation-band:has(> .f-presentation-band-media)"), "isolation"), "isolate");
    const band = rule(presentation, ".f-presentation-band");
    assert.equal(property(band, "min-height"), "var(--f-band-min-height,0px)");
    assert.equal(property(band, "height"), undefined, "A decorative image must not impose a content-clipping fixed height.");
});

test("every presentation media Tone uses its existing role for a dense gradient and retains ordinary Dotted paint", async () => {
    for (const tone of ["canvas", "surface", "primary"]) {
        const overlay = rule(presentationDesign, `.f-presentation-${tone} > .f-presentation-band-media::after`);
        const gradient = property(overlay, "background");
        assert.match(gradient, /^linear-gradient\(90deg,/);
        assert.ok(gradient.includes(`var(--f-${tone}) 52%`), "The copy region must retain an opaque Tone backdrop.");
        assert.ok(gradient.includes("/ 92%) 76%") && gradient.includes("/ 20%) 100%"), "The desktop veil must reveal the image outside the dense copy region.");
        const narrow = presentationDesign.slice(presentationDesign.lastIndexOf("@media(max-width:760px)"));
        const narrowGradient = property(rule(narrow, `.f-presentation-${tone} > .f-presentation-band-media::after`), "background");
        assert.ok(narrowGradient.includes(`var(--f-${tone}) 52%`) && narrowGradient.includes("/ 92%) 76%") && narrowGradient.includes("/ 78%) 100%"), "Narrow copy must retain dense Tone coverage across the whole image.");
        assert.deepEqual([...new Set([...gradient.matchAll(/var\(--f-([^)]+)\)/g)].map(match => match[1]))], [tone]);
        assert.doesNotMatch(gradient, /#[0-9a-f]{3,8}|rgba?\(\s*\d/i, "A media variant introduced a competing color instead of the active role.");
        assert.equal(property(rule(presentationDesign, `.f-presentation-${tone}`), "background-color"), `var(--f-${tone})`, "Missing images must retain the ordinary Tone fallback.");
    }
    const shared = await read("Flourish.Blazor.Design/wwwroot/display-board.css");
    assert.match(rule(shared, ".f-display-board-dotted,.f-presentation-dotted"), /background-image:radial-gradient/);
    assert.match(rule(design, ".f-access-layout-dotted"), /background-image:radial-gradient/);
});

test("Gallery media examples use an existing local placeholder and describe the public contract in all three cultures", async () => {
    const gallery = resolve(root, "src/Gallery.Flourish.Blazor");
    const sample = await readFile(resolve(gallery, "Components/Samples/Display/PresentationHeroSample.razor"), "utf8");
    assert.match(sample, /BackgroundImageUrl="gallery\.svg"/);
    const asset = await readFile(resolve(gallery, "wwwroot/gallery.svg"), "utf8");
    assert.match(asset, /<svg\b/);
    const parameters = JSON.parse(await readFile(resolve(gallery, "Localization/Culture.Parameters.json"), "utf8"));
    const changes = JSON.parse(await readFile(resolve(gallery, "Localization/Culture.ChangeLog.json"), "utf8"));
    for (const culture of ["en-US", "zh-CN", "pt-BR"]) {
        assert.ok(parameters.Parameter_BackgroundImageUrl[culture].length > 30);
        assert.ok(changes.ChangeLog_PresentationMediaAndIdentityScale[culture].length > 30);
    }
});

test("context-primary identity shares the actual service row with its image without flattening the semantic heading", () => {
    const selector = ".f-logo-displayer-compact.f-logo-displayer-context-primary";
    assert.equal(property(rule(presentation, selector), "grid-template-rows"), "auto auto");
    const title = rule(presentation, selector + " > .f-logo-displayer-title");
    assert.equal(property(title, "grid-row"), "1/span 2");
    assert.equal(property(title, "grid-column"), "2");
    assert.equal(property(title, "grid-template-rows"), "subgrid");
    assert.equal(property(title, "align-self"), "stretch");
    assert.equal(property(title, "align-items"), "center");
    assert.equal(property(rule(presentation, ".f-logo-displayer-title"), "display"), "grid");
    assert.equal(property(rule(presentation, ".f-logo-displayer-context-primary > .f-logo-displayer-title"), "row-gap"), "8px");
    assert.equal(property(rule(presentation, selector + " .f-logo-displayer-project"), "grid-row"), "1");
    assert.equal(property(rule(presentation, selector + " .f-logo-displayer-context"), "grid-row"), "2");
    const image = rule(presentation, selector + " > .f-logo-displayer-logo");
    assert.equal(property(image, "grid-row"), "2", "The image must align with the service row, not the whole two-line identity.");
    assert.equal(property(image, "align-self"), "center");
    assert.equal(property(rule(presentation, selector + " > .f-logo-displayer-description"), "grid-row"), "3");
    const noImage = rule(presentation, selector + ":not(:has(> .f-logo-displayer-logo)) > .f-logo-displayer-title");
    assert.equal(property(noImage, "grid-column"), "1");
    for (const css of [presentation, presentationDesign]) {
        for (const match of css.matchAll(/([^{}]+)\{([^{}]*)\}/g)) {
            if (!match[1].includes(".f-logo-displayer-context-primary")) continue;
            assert.doesNotMatch(match[2], /(?:^|;)\s*(?:(?:height|max-height|top|bottom|transform):|overflow(?:-[xy])?:\s*(?:hidden|clip)|display:\s*contents)/,
                "Wrapped or short-screen service identity acquired clipping, a geometric offset or display:contents on its heading.");
        }
    }
});

test("primary context swaps emphasis explicitly while ordinary Account keeps its existing type scale", () => {
    const title = rule(presentationDesign, ".f-logo-displayer > .f-logo-displayer-title");
    assert.equal(property(title, "font-size"), "clamp(56px,8vw,80px)");
    assert.equal(property(title, "overflow-wrap"), "anywhere");
    const brand = rule(presentationDesign, ".f-logo-displayer-context-primary .f-logo-displayer-project");
    assert.equal(property(brand, "font-size"), "var(--f-type-h2,28px)");
    assert.equal(property(brand, "line-height"), "1.2");
    const context = rule(presentationDesign, ".f-logo-displayer-context-primary .f-logo-displayer-context");
    assert.equal(property(context, "font-size"), "inherit");
    assert.equal(property(context, "color"), "inherit");
    assert.equal(property(context, "letter-spacing"), "inherit");
    assert.equal(property(context, "margin-top"), "0");
    assert.equal(property(rule(presentationDesign, ".f-logo-displayer-context"), "font-size"), "var(--f-type-h2,28px)");
    assert.equal(property(rule(presentationDesign, ".f-logo-displayer-context"), "margin-top"), "12px");
});

test("Gallery exposes primary context identity and its opt-in contract in every supported culture", async () => {
    const gallery = resolve(root, "src/Gallery.Flourish.Blazor");
    const parameters = JSON.parse(await readFile(resolve(gallery, "Localization/Culture.Parameters.json"), "utf8"));
    const samples = JSON.parse(await readFile(resolve(gallery, "Localization/Culture.Samples.json"), "utf8"));
    const components = JSON.parse(await readFile(resolve(gallery, "Localization/Culture.Components.json"), "utf8"));
    for (const culture of ["en-US", "zh-CN", "pt-BR"]) {
        assert.ok(parameters.Parameter_ContextPrimary_LogoDisplayer[culture].length > 30);
        assert.ok(samples.Sample_UseContextAsPrimaryIdentity[culture].length > 5);
        assert.ok(components.Catalog_LogoDisplayer_Variants[culture].includes("ContextPrimary"));
    }
    const sample = await readFile(resolve(gallery, "Components/Samples/Display/LogoDisplayerSample.razor"), "utf8");
    assert.match(sample, /ContextPrimary="@ShowContextPrimary"/);
    assert.match(sample, /Sample_UseContextAsPrimaryIdentity/);
    for (const path of ["Components/Pages/AccessMethodExample.razor", "Components/Samples/Display/AccessLayoutSample.razor"]) {
        const example = await readFile(resolve(gallery, path), "utf8");
        assert.match(example, /<LogoDisplayer Compact="true" ContextPrimary="true" LogoSize="96"/);
    }
});
