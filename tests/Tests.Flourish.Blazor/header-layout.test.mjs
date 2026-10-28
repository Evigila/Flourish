import assert from 'node:assert/strict';
import { readFile } from 'node:fs/promises';
import test from 'node:test';

const source = await readFile(new URL('../../src/Flourish.Blazor/Flourish.Blazor.Framework/wwwroot/patterns/header-layout.js', import.meta.url), 'utf8');
const { resolveHeaderLayout, initializeShellHeaders, disposeShellHeader } = await import(`data:text/javascript;base64,${Buffer.from(source).toString('base64')}`);

test('wide chrome preserves exact presentation content edges while both wings fit', () => {
    const header = { left:0, right:1920, width:1920 };
    const text = { left:394, right:1526 }; // 1180 container including its 24px padding.
    const layout = resolveHeaderLayout(header, text, 224, 280);
    assert.equal(layout.mode, 'aligned');
    assert.equal(header.left + layout.start, text.left);
    assert.equal(header.right - layout.end, text.right);
});

test('wide page tracks fit intrinsic menus beside wings instead of forcing a second row', () => {
    for (const text of [{ left:117, right:1249 }, { left:332, right:1342 }]) {
        const layout = resolveHeaderLayout({ left:0, right:1366, width:1366 }, text, 224, 280, 'ltr', 264);
        assert.equal(layout.mode, 'fitted');
        assert.ok(layout.start >= 224 && layout.end >= 280);
        assert.ok(1366 - layout.start - layout.end >= 264);
    }
});

test('only real control overflow stacks menus and generous alignment gutters can give space back', () => {
    const header = { left:0, right:1366, width:1366 }, content = { left:400, right:966 };
    const fitted = resolveHeaderLayout(header, content, 224, 280, 'ltr', 800);
    assert.equal(fitted.mode, 'fitted');
    assert.ok(fitted.start >= 224 && fitted.end >= 280);
    assert.ok(Math.abs(header.width - fitted.start - fitted.end - 800) < 0.001);
    const narrow = resolveHeaderLayout({ left:0, right:767, width:767 }, { left:24, right:743 }, 224, 280, 'ltr', 264);
    assert.equal(narrow.mode, 'stacked');
    assert.deepEqual([narrow.start, narrow.end], [24, 24], 'stacked navigation keeps actual page alignment');
    assert.equal(resolveHeaderLayout({ left:0, right:768, width:768 }, { left:24, right:744 }, 224, 280, 'ltr', 264).mode, 'fitted');
});

test('fitted and aligned CSS use the same first-row tracks without fixing header height', async () => {
    const css = await readFile(new URL('../../src/Flourish.Blazor/Flourish.Blazor.Framework/wwwroot/primitives/behavior.css', import.meta.url), 'utf8');
    const selector = '.shell-header:is([data-shell-header-layout="aligned"],[data-shell-header-layout="fitted"])';
    const rule = suffix => css.slice(css.indexOf(selector + suffix)).split('}')[0];
    assert.match(rule(' {'), /grid-template-columns:\s*var\(--f-shell-content-start\) minmax\(0, 1fr\) var\(--f-shell-content-end\)/);
    assert.match(rule(' > .shell-header-center {'), /grid-row:\s*1/);
    assert.match(rule(' > .shell-header-actions {'), /grid-column:\s*3/);
    assert.doesNotMatch(rule(' {'), /(?:^|;)\s*(?:height|max-height)\s*:/);
});

test('compact desktop and narrow brand reservations fit without changing the measured content track', async () => {
    const paint = await readFile(new URL('../../src/Flourish.Blazor/Flourish.Blazor.Design/wwwroot/primitives/ShellHeader.css', import.meta.url), 'utf8');
    const widths = [...paint.matchAll(/--f-shell-brand-min:\s*(\d+)px/g)].map(match => Number(match[1]));
    assert.deepEqual(widths, [82, 78]);
    const header = { left:0, right:1366, width:1366 }, content = { left:82, right:1284 };
    for (const width of widths) {
        const layout = resolveHeaderLayout(header, content, width, 82);
        assert.equal(layout.mode, 'aligned');
        assert.equal(layout.start, content.left);
        assert.equal(header.right - layout.end, content.right);
    }
    assert.equal(resolveHeaderLayout(header, { left:81, right:1285 }, widths[0], 81).mode, 'fitted');
    assert.equal(resolveHeaderLayout(header, { left:81, right:1285 }, widths[1], 81).mode, 'aligned');
});

test('a dynamic expanded page and changed navigation rails are reflected by actual content bounds', () => {
    const header = { left:40, right:2440, width:2400 };
    const standard = resolveHeaderLayout(header, { left:778, right:1958 }, 220, 260);
    const expanded = resolveHeaderLayout(header, { left:563, right:2173 }, 220, 260);
    assert.equal(standard.mode, 'aligned');
    assert.equal(expanded.mode, 'aligned');
    assert.ok(expanded.start < standard.start && expanded.end < standard.end);
    assert.equal(header.width - expanded.start - expanded.end, 1610);
    const collapsedRails = resolveHeaderLayout(header, { left:485, right:2173 }, 220, 260);
    assert.equal(collapsedRails.start, 445);
});

test('RTL mirrors inline wings while preserving physical content edges and invalid geometry fails closed', () => {
    const header = { left:20, right:1940, width:1920 };
    const layout = resolveHeaderLayout(header, { left:400, right:1500 }, 300, 200, 'rtl');
    assert.equal(layout.start, 440);
    assert.equal(layout.end, 380);
    assert.equal(layout.mode, 'aligned');
    const fitted = resolveHeaderLayout(header, { left:44, right:1916 }, 300, 200, 'rtl', 260);
    assert.deepEqual([fitted.start, fitted.end, fitted.mode], [300, 200, 'fitted']);
    assert.equal(resolveHeaderLayout({ left:0, right:0, width:0 }, { left:0, right:0 }, 1, 1), null);
});

test('removed headers release every subscription and pending animation frame', () => {
    let resizeDisconnected = 0, mutationDisconnected = 0, cancelled = 0, removed = 0;
    const previous = Object.fromEntries(['ResizeObserver', 'MutationObserver', 'window', 'requestAnimationFrame', 'cancelAnimationFrame']
        .map(key => [key, globalThis[key]]));
    globalThis.ResizeObserver = class { disconnect() { resizeDisconnected++; } };
    globalThis.MutationObserver = class { disconnect() { mutationDisconnected++; } };
    globalThis.window = { addEventListener() {}, removeEventListener() { removed++; } };
    globalThis.requestAnimationFrame = () => 17;
    globalThis.cancelAnimationFrame = () => cancelled++;
    const header = { isConnected:true, style:{ removeProperty() {} }, removeAttribute() {} };
    try {
        initializeShellHeaders({ querySelectorAll:() => [header] });
        header.isConnected = false;
        initializeShellHeaders({ querySelectorAll:() => [] });
        disposeShellHeader(header);
        assert.equal(resizeDisconnected, 1);
        assert.equal(mutationDisconnected, 1);
        assert.equal(cancelled, 1);
        assert.equal(removed, 1);
    } finally {
        for (const [key, value] of Object.entries(previous)) {
            if (value === undefined) delete globalThis[key]; else globalThis[key] = value;
        }
    }
});

test('footer foreground follows the same real content edges through resize routing RTL and disposal', () => {
    const keys = ['ResizeObserver', 'MutationObserver', 'window', 'requestAnimationFrame', 'cancelAnimationFrame', 'getComputedStyle'];
    const previous = Object.fromEntries(keys.map(key => [key, globalThis[key]]));
    const frames = new Map();
    let nextFrame = 0, mutation, resize, mutationSettings;
    const observed = new Set();
    function element(classes, rect, computed = {}) {
        const attributes = new Map(), properties = new Map();
        return {
            rect, isConnected:true, computed:{ direction:'ltr', display:'flex', flexDirection:'row', paddingLeft:'0px', paddingRight:'0px', ...computed }, children:[],
            classList:{ contains:name => classes.includes(name) },
            style:{
                getPropertyValue:name => properties.get(name) ?? '',
                setProperty:(name, value) => properties.set(name, value),
                removeProperty:name => properties.delete(name)
            },
            getAttribute:name => name === 'style' ? [...properties].map(([key, value]) => `${key}:${value}`).join(';') || null : attributes.get(name) ?? null,
            setAttribute:(name, value) => attributes.set(name, value),
            removeAttribute:name => attributes.delete(name),
            getBoundingClientRect() { return this.rect; },
            closest:selector => selector.includes('.f-menu-panel') ? null : surface,
            matches:selector => selector.split(',').some(part => classes.includes(part.slice(1)))
        };
    }
    const surface = {};
    const header = element(['shell-header'], { left:0, right:1920, width:1920 });
    const brandWing = element(['shell-header-start'], { left:0, right:442, width:442 }, { paddingLeft:'10px', paddingRight:'10px' });
    brandWing.children = [element(['shell-header-brand'], { left:10, right:210, width:200 })];
    const center = element(['shell-header-center'], { left:442, right:1478, width:1036 });
    const slot = element(['f-topbar-slot'], center.rect), group = element(['f-inline-actions'], center.rect, { columnGap:'12px' });
    const menus = [element(['f-button'], { left:442, right:562, width:120 }), element(['f-button'], { left:574, right:706, width:132 })];
    const panel = element(['f-menu-panel'], { left:0, right:9001, width:9001 });
    group.children = [...menus, panel];
    slot.children = [group];
    center.children = [slot];
    center.querySelectorAll = () => menus;
    const actions = element(['shell-header-actions'], { left:1602, right:1920, width:318 }, { paddingRight:'10px' });
    const identity = element(['shell-header-identity'], { left:1602, right:1802, width:200 }, { paddingLeft:'15px', paddingRight:'15px', columnGap:'10px' });
    identity.children = [element(['f-icon'], { width:26 }), element(['name'], { width:134 })];
    const actionSlot = element(['f-topbar-slot'], { width:108 }), actionGroup = element(['f-inline-actions'], { width:108 }, { columnGap:'12px' });
    actionGroup.setAttribute('data-alignment', 'end');
    const language = element(['f-native-select-popup'], { width:48 }, { display:'inline-block' });
    const languageActions = element(['f-native-select-actions'], { width:48 }, { display:'inline-block' });
    const languageMenu = element(['f-action-menu'], { width:48 }, { display:'inline-flex' });
    const iconButton = () => {
        const button = element(['f-button', 'f-button-icon'], { width:48 }, { flexShrink:'0', flexBasis:'48px', minWidth:'0px' });
        button.children = [element(['f-icon'], { width:26 })];
        return button;
    };
    const languageTrigger = iconButton(), logout = iconButton();
    languageMenu.children = [languageTrigger];
    languageActions.children = [languageMenu];
    language.children = [element(['select'], { width:0 }), languageActions];
    const postForm = element(['form'], { width:48 });
    postForm.setAttribute('method', 'post'); postForm.setAttribute('action', '/org/example/auth/organization-logout');
    const antiforgery = element(['hidden-antiforgery'], { width:0 });
    antiforgery.setAttribute('type', 'hidden'); antiforgery.setAttribute('name', '__RequestVerificationToken'); antiforgery.setAttribute('value', 'fixture-token');
    logout.setAttribute('type', 'submit');
    postForm.children = [antiforgery, logout];
    actionGroup.children = [language, element(['f-dialog'], { width:0 }), postForm];
    actionSlot.children = [actionGroup];
    actions.children = [identity, actionSlot];
    actions.querySelectorAll = () => [identity.children[1], languageTrigger, logout];
    const target = element(['f-content-container'], { left:370, right:1550, width:1180 }, { paddingLeft:'72px', paddingRight:'72px' });
    const stage = element(['content-stage'], { left:0, right:1920, width:1920 });
    const firstFooter = element(['f-presentation-footer'], { left:0, right:1880, width:1880 });
    let footers = [firstFooter];
    stage.contains = () => false;
    stage.querySelectorAll = selector => selector === '.f-content-container' ? [target] : [];
    surface.querySelectorAll = selector => selector === '.f-presentation-footer' ? footers : [stage];
    header.querySelector = selector => selector === ':scope > .shell-header-start' ? brandWing : selector === ':scope > .shell-header-center' ? center : actions;
    header.querySelectorAll = () => [];
    const flush = () => {
        const pending = [...frames.values()];
        frames.clear();
        for (const callback of pending) callback();
    };
    const physicalFooterEdges = footer => {
        const start = Number.parseFloat(footer.style.getPropertyValue('--f-presentation-footer-content-start'));
        const end = Number.parseFloat(footer.style.getPropertyValue('--f-presentation-footer-content-end'));
        return footer.style.getPropertyValue('direction') === 'rtl'
            ? [footer.rect.left + end, footer.rect.right - start]
            : [footer.rect.left + start, footer.rect.right - end];
    };
    globalThis.ResizeObserver = class {
        constructor(callback) { resize = callback; }
        observe(node) { observed.add(node); } unobserve(node) { observed.delete(node); } disconnect() { observed.clear(); }
    };
    globalThis.MutationObserver = class {
        constructor(callback) { mutation = callback; }
        observe(node, settings) { mutationSettings = settings; } disconnect() {}
    };
    globalThis.window = { addEventListener() {}, removeEventListener() {} };
    globalThis.requestAnimationFrame = callback => { frames.set(++nextFrame, callback); return nextFrame; };
    globalThis.cancelAnimationFrame = id => frames.delete(id);
    globalThis.getComputedStyle = node => ({ ...node.computed, direction:node.style.getPropertyValue('direction') || node.computed.direction });
    try {
        initializeShellHeaders({ querySelectorAll:() => [header] });
        flush();
        assert.deepEqual(physicalFooterEdges(firstFooter), [442, 1478]);
        assert.ok(observed.has(center) && menus.every(menu => observed.has(menu)), 'menu controls must be observed even when their containing slot stays page-wide');
        assert.ok(observed.has(identity.children[1]) && observed.has(languageTrigger) && observed.has(logout), 'right-side controls and identity text must refresh fit even when the constrained wing stays the same width');
        assert.ok(mutationSettings.attributeFilter.includes('data-f-native-select-ready'), 'native language enhancement must refresh the control set even without a class/style change');
        assert.equal(Number.parseFloat(header.style.getPropertyValue('--f-shell-content-start')), 442);
        assert.equal(1920 - Number.parseFloat(header.style.getPropertyValue('--f-shell-content-end')), 1478);
        mutation([{ target:firstFooter, attributeName:'style' }]);
        assert.equal(frames.size, 0, 'controller-owned footer styles must not feed another observer pass');

        firstFooter.style.setProperty('direction', 'rtl');
        mutation([{ target:firstFooter, attributeName:'style' }]);
        flush();
        assert.deepEqual(physicalFooterEdges(firstFooter), [442, 1478]);
        assert.equal(firstFooter.style.getPropertyValue('--f-presentation-footer-content-start'), '402px');

        target.rect = { left:510, right:1410, width:900 };
        target.computed.paddingLeft = target.computed.paddingRight = '24px';
        resize();
        flush();
        assert.deepEqual(physicalFooterEdges(firstFooter), [534, 1386]);
        const replacement = element(['f-presentation-footer'], { left:0, right:1920, width:1920 });
        footers = [replacement];
        mutation([{ target:surface, type:'childList' }]);
        flush();
        assert.equal(firstFooter.getAttribute('data-presentation-footer-layout'), null);
        assert.equal(firstFooter.style.getPropertyValue('--f-presentation-footer-content-start'), '');
        assert.equal(firstFooter.style.getPropertyValue('direction'), 'rtl', 'cleanup must retain caller-owned styles');
        assert.deepEqual(physicalFooterEdges(replacement), [534, 1386]);

        footers = [];
        mutation([{ target:surface, type:'childList' }]);
        flush();
        assert.equal(replacement.getAttribute('data-presentation-footer-layout'), null);
        footers = [replacement];
        mutation([{ target:surface, type:'childList' }]);
        flush();
        target.rect = { left:195, right:1614, width:1419 };
        resize();
        flush();
        assert.equal(header.getAttribute('data-shell-header-layout'), 'fitted', 'a 1px brand-gutter shortage should fit the menu instead of adding a row');
        target.computed.paddingLeft = target.computed.paddingRight = '25px';
        resize();
        flush();
        assert.equal(header.getAttribute('data-shell-header-layout'), 'aligned', 'the stretched 442px track must not replace the actual 220px branding requirement');
        target.rect = { left:0, right:1920, width:1920 };
        target.computed.paddingLeft = target.computed.paddingRight = '24px';
        resize();
        flush();
        assert.equal(header.getAttribute('data-shell-header-layout'), 'fitted', 'the page-wide InlineActions and open popup must not become intrinsic menu requirements');
        assert.equal(header.style.getPropertyValue('--f-shell-content-start'), '220px');
        assert.equal(header.style.getPropertyValue('--f-shell-content-end'), '318px', 'identity200 + language48 + gap12 + POST/logout48 + outer10 cannot be reduced to 26px glyphs');
        assert.deepEqual(physicalFooterEdges(replacement), [24, 1896], 'footer copy still follows the page rather than the clamped menu track');
        languageTrigger.rect.width = logout.rect.width = 26;
        resize();
        flush();
        assert.equal(header.style.getPropertyValue('--f-shell-content-end'), '318px', 'an already constrained control box must recover its nonshrinking CSS basis, avoiding the previous 44px shortfall');
        header.rect = { left:0, right:801, width:801 };
        resize();
        flush();
        assert.equal(header.getAttribute('data-shell-header-layout'), 'stacked', '220px brand + 264px menu + 318px business actions need 802px');
        header.rect = { left:0, right:802, width:802 };
        resize();
        flush();
        assert.equal(header.getAttribute('data-shell-header-layout'), 'fitted', 'intrinsic menu geometry must recover a first row without measuring the old stretched slot');
        assert.equal(postForm.getAttribute('method'), 'post');
        assert.equal(postForm.getAttribute('action'), '/org/example/auth/organization-logout');
        assert.equal(postForm.children[0], antiforgery); assert.equal(postForm.children[1], logout);
        assert.equal(antiforgery.getAttribute('type'), 'hidden');
        assert.equal(antiforgery.getAttribute('name'), '__RequestVerificationToken');
        assert.equal(antiforgery.getAttribute('value'), 'fixture-token');
        assert.equal(logout.getAttribute('type'), 'submit');
        menus[0].rect.width++;
        resize();
        flush();
        assert.equal(header.getAttribute('data-shell-header-layout'), 'stacked', 'a localized control width change must update the fit without a page-width change');
        menus[0].rect.width--;
        identity.children[1].rect.width++;
        resize(); flush();
        assert.equal(header.getAttribute('data-shell-header-layout'), 'stacked', 'identity font metrics are part of the right-side budget even while its parent box stays constrained');
        identity.children[1].rect.width--;
        resize(); flush();
        assert.equal(header.getAttribute('data-shell-header-layout'), 'fitted', 'a released right-side constraint must recover the same single-row contract');
        resize();
        disposeShellHeader(header);
        assert.equal(frames.size, 0);
        assert.equal(observed.size, 0);
        assert.equal(replacement.getAttribute('data-presentation-footer-layout'), null);
        assert.equal(replacement.style.getPropertyValue('--f-presentation-footer-content-end'), '');
    } finally {
        disposeShellHeader(header);
        for (const [key, value] of Object.entries(previous)) {
            if (value === undefined) delete globalThis[key]; else globalThis[key] = value;
        }
    }
});
