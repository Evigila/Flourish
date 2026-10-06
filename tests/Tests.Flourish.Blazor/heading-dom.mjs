import assert from 'node:assert/strict';
import { readFile } from 'node:fs/promises';
import test from 'node:test';

const source = await readFile(new URL('../../src/Flourish.Blazor/Flourish.Blazor.Framework/wwwroot/shell.js', import.meta.url), 'utf8');
const { updateCompactHeading, resetCompactHeading, attach, detach } = await import(`data:text/javascript;base64,${Buffer.from(source).toString('base64')}`);

function fixture(range = 110, shrink = 102) {
    const attrs = new Set(), events = [];
    let top = 0, changes = 0;
    const root = {
        hasAttribute: name => attrs.has(name),
        setAttribute(name) { if (!attrs.has(name)) { attrs.add(name); changes++; events.push('layout'); } },
        removeAttribute(name) { if (attrs.delete(name)) { changes++; events.push('layout'); } }
    };
    const content = {
        clientHeight:600, clientWidth:1000, range, shrink,
        get scrollHeight() { return this.clientHeight + Math.max(0, this.range - (attrs.size ? this.shrink : 0)); },
        get scrollTop() { top = Math.min(top, this.scrollHeight - this.clientHeight); return top; },
        set scrollTop(value) { top = Math.max(0, Math.min(value, this.scrollHeight - this.clientHeight)); events.push('scroll'); }
    };
    const settle = () => {
        let frames = 0;
        while (events.length) {
            events.shift();
            updateCompactHeading(root, content);
            assert.ok(++frames < 12, 'Heading feedback did not settle.');
        }
    };
    return { root, content, settle, compact:() => attrs.size > 0, changes:() => changes };
}

test('short Home-like content restores scroll and settles without a spacer or repeated compaction', () => {
    const f = fixture(); f.content.scrollTop = 100; f.settle();
    assert.equal(f.compact(), false);
    assert.equal(f.content.scrollTop, 100);
    assert.equal(f.content.scrollHeight, 710);
    assert.equal(f.changes(), 2);
    for (let i = 0; i < 20; i++) updateCompactHeading(f.root, f.content);
    assert.equal(f.changes(), 2);
});

test('usable compact range preserves hysteresis and expands for a real scroll to top', () => {
    const f = fixture(110, 70); f.content.scrollTop = 100; f.settle();
    assert.equal(f.compact(), true); assert.equal(f.content.scrollTop, 40);
    f.content.scrollTop = 24; f.settle(); assert.equal(f.compact(), true);
    f.content.scrollTop = 23; f.settle(); assert.equal(f.compact(), false);
    assert.equal(f.changes(), 2);
});

test('long content keeps the original collapse threshold and does not oscillate near it', () => {
    const f = fixture(900); f.content.scrollTop = 96; f.settle(); assert.equal(f.compact(), false);
    f.content.scrollTop = 97; f.settle(); assert.equal(f.compact(), true);
    assert.equal(f.content.scrollTop, 97);
    f.content.scrollTop = 0; f.settle(); assert.equal(f.compact(), false);
});

test('content growth and viewport changes can retry a previously unsuitable geometry', () => {
    const f = fixture(); f.content.scrollTop = 100; f.settle();
    f.content.range = 400; updateCompactHeading(f.root, f.content); f.settle(); assert.equal(f.compact(), true);
    resetCompactHeading(f.root); f.content.range = 110; f.content.scrollTop = 100; f.settle(); assert.equal(f.compact(), false);
    f.content.range = 300; f.content.clientHeight = 400;
    updateCompactHeading(f.root, f.content); f.settle(); assert.equal(f.compact(), true);
});

test('document replacement and navigation reset clear the blocked geometry', () => {
    const f = fixture(); f.content.scrollTop = 100; f.settle();
    resetCompactHeading(f.root); f.content.scrollTop = 0; f.settle();
    assert.equal(f.compact(), false);
    f.content.range = 500; f.content.scrollTop = 100; f.settle(); assert.equal(f.compact(), true);
    resetCompactHeading(f.root); f.content.scrollTop = 0; f.settle(); assert.equal(f.compact(), false);
});

test('shell measurement requires and owns the native ResizeObserver lifecycle', () => {
    const previous = { window: globalThis.window, ResizeObserver: globalThis.ResizeObserver };
    const observers = [], properties = new Map();
    const titlebar = { height: 68, getBoundingClientRect() { return { height: this.height }; } };
    const primary = { width: 80, getBoundingClientRect() { return { width: this.width }; } };
    const root = { attrs: new Set(), dataset: {}, style: { setProperty: (key, value) => properties.set(key, value) },
        querySelector: selector => selector === '.f-titlebar' ? titlebar : selector === '.f-primary-navigation' ? primary : null,
        querySelectorAll: () => [], addEventListener() {}, removeEventListener() {},
        hasAttribute(name) { return this.attrs.has(name); }, removeAttribute(name) { this.attrs.delete(name); } };
    const content = { scrollTop: 0, addEventListener() {}, removeEventListener() {} };
    globalThis.window = { addEventListener() {}, removeEventListener() {} };
    globalThis.ResizeObserver = class {
        constructor(callback) { this.callback = callback; this.observed = []; observers.push(this); }
        observe(node) { this.observed.push(node); }
        disconnect() { this.disconnected = true; }
    };
    try {
        attach(root, content);
        assert.deepEqual(observers[0].observed, [titlebar, primary]);
        assert.equal(properties.get('--f-shell-titlebar-size'), '68px');
        titlebar.height = 96; observers[0].callback();
        assert.equal(properties.get('--f-shell-titlebar-size'), '96px');
        detach(root);
        assert.equal(observers[0].disconnected, true);
        delete globalThis.ResizeObserver;
        assert.throws(() => attach(root, content), ReferenceError,
            'An unsupported browser must fail instead of silently disabling live shell measurement.');
    } finally {
        globalThis.window = previous.window;
        if (previous.ResizeObserver === undefined) delete globalThis.ResizeObserver;
        else globalThis.ResizeObserver = previous.ResizeObserver;
    }
});

test('production interaction sources do not select alternative layouts when required modern APIs are absent', async () => {
    for (const name of ['shell.js', 'primitives/primary-navigation-item.js', 'patterns/surfaces.js',
        'section-navigator.js', 'primitives/editing-grid-columns.js', 'primitives/editing-grid.js',
        'presentation/offers.js', 'data.js']) {
        const text = await readFile(new URL('../../src/Flourish.Blazor/Flourish.Blazor.Framework/wwwroot/' + name, import.meta.url), 'utf8');
        assert.doesNotMatch(text, /typeof[^;\n]*(?:ResizeObserver|MutationObserver|requestAnimationFrame|showPopover|hidePopover)/, name);
        assert.doesNotMatch(text, /if\s*\(!window\?\.matchMedia\)/, name);
        assert.doesNotMatch(text, /runtime without observers|\.f-page-heading, \.page-heading/, name);
    }
});
