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

test('1366px presentation and fluid business layouts move navigation below wings without narrowing content', () => {
    for (const text of [{ left:117, right:1249 }, { left:332, right:1342 }]) {
        const layout = resolveHeaderLayout({ left:0, right:1366, width:1366 }, text, 224, 280);
        assert.equal(layout.mode, 'stacked');
        assert.equal(layout.start, text.left);
        assert.equal(1366 - layout.end, text.right);
    }
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
    assert.equal(resolveHeaderLayout(header, { left:81, right:1285 }, widths[0], 81).mode, 'stacked');
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
    let nextFrame = 0, mutation, resize;
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
            closest:() => surface,
            matches:selector => ['.shell-header', '.shell-header-start'].includes(selector) && classes.includes(selector.slice(1))
        };
    }
    const surface = {};
    const header = element(['shell-header'], { left:0, right:1920, width:1920 });
    const brandWing = element(['shell-header-start'], { left:0, right:442, width:442 }, { paddingLeft:'10px', paddingRight:'10px' });
    brandWing.children = [element(['shell-header-brand'], { left:10, right:210, width:200 })];
    const target = element(['f-content-container'], { left:370, right:1550, width:1180 }, { paddingLeft:'72px', paddingRight:'72px' });
    const stage = element(['content-stage'], { left:0, right:1920, width:1920 });
    const firstFooter = element(['f-presentation-footer'], { left:0, right:1880, width:1880 });
    let footers = [firstFooter];
    stage.contains = () => false;
    stage.querySelectorAll = selector => selector === '.f-content-container' ? [target] : [];
    surface.querySelectorAll = selector => selector === '.f-presentation-footer' ? footers : [stage];
    header.querySelector = selector => selector === ':scope > .shell-header-start' ? brandWing : null;
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
        observe() {} unobserve() {} disconnect() {}
    };
    globalThis.MutationObserver = class {
        constructor(callback) { mutation = callback; }
        observe() {} disconnect() {}
    };
    globalThis.window = { addEventListener() {}, removeEventListener() {} };
    globalThis.requestAnimationFrame = callback => { frames.set(++nextFrame, callback); return nextFrame; };
    globalThis.cancelAnimationFrame = id => frames.delete(id);
    globalThis.getComputedStyle = node => ({ ...node.computed, direction:node.style.getPropertyValue('direction') || node.computed.direction });
    try {
        initializeShellHeaders({ querySelectorAll:() => [header] });
        flush();
        assert.deepEqual(physicalFooterEdges(firstFooter), [442, 1478]);
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
        target.rect = { left:195, right:1725, width:1530 };
        resize();
        flush();
        assert.equal(header.getAttribute('data-shell-header-layout'), 'stacked', '200px branding plus 20px wing padding must not fit in a 219px wing');
        target.computed.paddingLeft = target.computed.paddingRight = '25px';
        resize();
        flush();
        assert.equal(header.getAttribute('data-shell-header-layout'), 'aligned', 'the stretched 442px track must not replace the actual 220px branding requirement');
        resize();
        disposeShellHeader(header);
        assert.equal(frames.size, 0);
        assert.equal(replacement.getAttribute('data-presentation-footer-layout'), null);
        assert.equal(replacement.style.getPropertyValue('--f-presentation-footer-content-end'), '');
    } finally {
        disposeShellHeader(header);
        for (const [key, value] of Object.entries(previous)) {
            if (value === undefined) delete globalThis[key]; else globalThis[key] = value;
        }
    }
});
