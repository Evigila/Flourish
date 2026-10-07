import assert from 'node:assert/strict';
import { readFile } from 'node:fs/promises';
import test from 'node:test';

const framework = new URL('../../src/Flourish.Blazor/Flourish.Blazor.Framework/', import.meta.url);
const source = path => readFile(new URL(path, framework), 'utf8');
const moduleUrl = text => `data:text/javascript;base64,${Buffer.from(text).toString('base64')}`;
const shellUrl = moduleUrl(await source('wwwroot/shell.js'));
const shell = await import(shellUrl);
const surfaces = await import(moduleUrl((await source('wwwroot/patterns/surfaces.js'))
    .replace("from '../shell.js'", `from '${shellUrl}'`)));

// Execute the production controllers with simulated DOM geometry. These are
// lifecycle and CSS contract tests, not browser/computed-style acceptance.
function environment() {
    const names = ['window', 'document', 'MutationObserver', 'ResizeObserver',
        'requestAnimationFrame', 'cancelAnimationFrame', 'matchMedia'];
    const previous = new Map(names.map(name => [name, Object.getOwnPropertyDescriptor(globalThis, name)]));
    const frames = new Map(), mutations = [], resizes = [];
    let nextFrame = 0;
    globalThis.window = { addEventListener() {}, removeEventListener() {} };
    globalThis.document = { body: {}, activeElement: null };
    globalThis.matchMedia = () => ({ matches: false });
    globalThis.requestAnimationFrame = callback => { frames.set(++nextFrame, callback); return nextFrame; };
    globalThis.cancelAnimationFrame = id => frames.delete(id);
    for (const [name, instances] of [['MutationObserver', mutations], ['ResizeObserver', resizes]]) {
        globalThis[name] = class {
            constructor(callback) { this.callback = callback; this.nodes = []; instances.push(this); }
            observe(node) { this.nodes.push(node); }
            disconnect() { this.disconnected = true; }
        };
    }
    return {
        mutations, resizes, frames,
        flush() {
            let count = 0;
            while (frames.size) {
                const callbacks = [...frames.values()]; frames.clear();
                for (const callback of callbacks) callback();
                assert.ok(++count < 12, 'Animation lifecycle did not settle.');
            }
        },
        restore() {
            for (const [name, descriptor] of previous) {
                if (descriptor) Object.defineProperty(globalThis, name, descriptor);
                else delete globalThis[name];
            }
        }
    };
}

function fixture(kind, range = 900, shrink = 102) {
    const attributes = new Set();
    let changes = 0;
    const root = {
        className: kind, isConnected: true, dataset: {},
        style: { setProperty() {} },
        querySelector: () => null, querySelectorAll: () => [],
        addEventListener() {}, removeEventListener() {},
        hasAttribute: name => attributes.has(name),
        setAttribute(name) { if (!attributes.has(name)) { attributes.add(name); changes++; } },
        removeAttribute(name) { if (attributes.delete(name)) changes++; }
    };
    const makeStage = () => {
        let top = 0;
        const handlers = new Set();
        return {
            range, clientHeight: 600, clientWidth: 1000, inert: false,
            get scrollHeight() { return this.clientHeight + Math.max(0, this.range - (attributes.has('data-compact-heading') ? shrink : 0)); },
            get scrollTop() { return top = Math.min(top, this.scrollHeight - this.clientHeight); },
            set scrollTop(value) { top = Math.max(0, Math.min(value, this.scrollHeight - this.clientHeight)); },
            addEventListener(name, handler, options) { assert.equal(name, 'scroll'); assert.equal(options.passive, true); handlers.add(handler); },
            removeEventListener(name, handler) { assert.equal(name, 'scroll'); handlers.delete(handler); },
            emitScroll() { for (const handler of handlers) handler(); },
            listenerCount: () => handlers.size
        };
    };
    return { root, stage: makeStage(), makeStage, compact: () => attributes.has('data-compact-heading'), changes: () => changes };
}

const kinds = ['f-shell', 'navigation-surface', 'content-surface'];
function bind(kind, root, stage) {
    if (kind === 'f-shell') shell.attach(root, stage);
    else surfaces.synchronize(root, stage);
}
function unbind(kind, root) {
    if (kind === 'f-shell') shell.detach(root);
    else surfaces.dispose(root);
}

for (const kind of kinds) {
    test(`${kind}: production scroll controller preserves collapse/expand hysteresis`, () => {
        const env = environment(), f = fixture(kind);
        try {
            bind(kind, f.root, f.stage);
            assert.equal(f.stage.listenerCount(), 1);
            for (const [top, compact] of [[96, false], [97, true], [24, true], [23, false]]) {
                f.stage.scrollTop = top; f.stage.emitScroll(); env.flush();
                assert.equal(f.compact(), compact, `scrollTop=${top}`);
            }
        } finally { unbind(kind, f.root); env.restore(); }
    });

    test(`${kind}: production short-page guard restores position and avoids repeated compaction`, () => {
        const env = environment(), f = fixture(kind, 110);
        try {
            bind(kind, f.root, f.stage);
            f.stage.scrollTop = 100; f.stage.emitScroll(); env.flush();
            assert.equal(f.compact(), false);
            assert.equal(f.stage.scrollTop, 100);
            assert.equal(f.changes(), 2);
            for (let index = 0; index < 10; index++) { f.stage.emitScroll(); env.flush(); }
            assert.equal(f.changes(), 2);
            f.stage.range = 500; f.stage.emitScroll(); env.flush();
            assert.equal(f.compact(), true, 'Changed geometry permits a new compact attempt.');
        } finally { unbind(kind, f.root); env.restore(); }
    });

    test(`${kind}: document-stage replacement removes the old listener and resets compact state`, () => {
        const env = environment(), f = fixture(kind);
        try {
            bind(kind, f.root, f.stage);
            f.stage.scrollTop = 120; f.stage.emitScroll(); env.flush();
            assert.equal(f.compact(), true);
            const next = f.makeStage(); bind(kind, f.root, next);
            assert.equal(f.stage.listenerCount(), 0);
            assert.equal(next.listenerCount(), 1);
            assert.equal(f.compact(), false);
            f.stage.scrollTop = 180; f.stage.emitScroll(); env.flush();
            assert.equal(f.compact(), false, 'Retired stage cannot mutate the new document.');
            next.scrollTop = 120; next.emitScroll(); env.flush(); assert.equal(f.compact(), true);
            unbind(kind, f.root); assert.equal(next.listenerCount(), 0); assert.equal(f.compact(), false);
        } finally { unbind(kind, f.root); env.restore(); }
    });

    test(`${kind}: controller disposal releases listeners, observers and queued work`, () => {
        const env = environment(), f = fixture(kind);
        try {
            bind(kind, f.root, f.stage);
            f.stage.scrollTop = 120; f.stage.emitScroll(); env.flush();
            if (kind === 'f-shell') {
                f.stage.emitScroll(); assert.equal(env.frames.size, 1);
                unbind(kind, f.root);
                assert.equal(env.frames.size, 0);
                assert.ok(env.resizes.every(observer => observer.disconnected));
            } else {
                bind(kind, f.root, f.stage);
                assert.equal(env.mutations.length, 1, 'Synchronizing the same stage must reuse its observer.');
                f.root.isConnected = false; env.mutations[0].callback();
                assert.equal(env.mutations[0].disconnected, true);
            }
            assert.equal(f.stage.listenerCount(), 0); assert.equal(f.compact(), false);
            f.stage.emitScroll(); env.flush(); assert.equal(f.compact(), false);
        } finally { unbind(kind, f.root); env.restore(); }
    });
}

function rule(css, selector) {
    const found = [...css.replace(/\/\*[\s\S]*?\*\//g, '').matchAll(/([^{}]+)\{([^{}]*)\}/g)]
        .find(match => match[1].trim() === selector);
    assert.ok(found, `Missing CSS contract: ${selector}`);
    return found[2];
}
function property(body, name) {
    return body.split(';').map(value => value.trim()).find(value => value.startsWith(`${name}:`))?.slice(name.length + 1).trim();
}

test('all three scroll roots share complete compact heading structure and explicit Compact mode', async () => {
    const css = await source('wwwroot/framework.css');
    const compact = ':is(:is(.f-shell,.content-surface,.navigation-surface)[data-compact-heading] .f-page-heading,.f-page-heading[data-heading-mode=compact])';
    assert.equal(property(rule(css, compact), 'min-height'), '72px');
    assert.equal(property(rule(css, `${compact} .f-heading-title`), 'flex-direction'), 'row');
    assert.equal(property(rule(css, `${compact} h1`), 'flex'), '1');
    const back = rule(css, `${compact} .f-parent-link`);
    for (const name of ['width', 'height', 'min-height']) assert.equal(property(back, name), '48px');
    assert.equal(property(back, 'flex'), '0 0 48px');
    assert.equal(property(rule(css, `${compact} .f-parent-link .f-button-text`), 'display'), 'none');
    assert.doesNotMatch(css, /\.f-shell\[data-compact-heading\] \.f-page-heading/);
});

test('field-owned reference dropdown fills its field while standalone selection retains its 240px contract', async () => {
    const css = await source('wwwroot/primitives/behavior.css');
    const standalone = rule(css, '.selection-dropdown-surface');
    assert.equal(property(standalone, 'width'), 'var(--f-selection-trigger-width, 240px)');
    assert.equal(property(standalone, 'max-width'), '100%');
    assert.equal(property(rule(css, '.f-field-control > .selection-dropdown-surface'), 'width'), '100%');
    assert.ok(css.indexOf('.f-field-control > .selection-dropdown-surface') > css.indexOf('.selection-dropdown-surface {'));
    assert.equal(property(rule(css, '.selection-dropdown-surface .selection-trigger'), 'width'), '100%');
    assert.doesNotMatch(rule(css, '.f-field-control > .selection-dropdown-surface'), /!important|position:|overflow:/);
});

test('full-height PageBody preserves a heading row and gives the spreadsheet only the remaining shell stage', async () => {
    const css = await source('wwwroot/framework.css');
    const stage = ':is(.f-content-scroll,.application-stage,.content-stage):has(> .f-page-fill-height)';
    assert.equal(property(rule(css, stage), 'overflow'), 'hidden');
    const body = rule(css, '.f-page-body.f-page-fill-height');
    for (const name of ['height', 'max-height']) assert.equal(property(body, name), '100%');
    assert.equal(property(body, 'min-height'), '0');
    assert.equal(property(body, 'box-sizing'), 'border-box');
    assert.equal(property(body, 'padding-block'), '0');
    assert.equal(property(body, 'flex-direction'), 'column');
    assert.equal(property(body, 'overflow'), 'hidden');
    assert.equal(property(rule(css, '.f-page-fill-height > .f-page-heading'), 'flex'), '0 0 auto');
    assert.equal(property(rule(css, '.f-page-fill-height > :not(.f-page-heading)'), 'flex'), '1 1 0');
    assert.equal(property(rule(css, '.f-page-fill-height > .spreadsheet-scroll'), 'max-height'), 'none');
    assert.equal(property(rule(await source('wwwroot/primitives/EditingGrid.css'), '.spreadsheet-scroll'), 'max-height'), '65vh',
        'Standalone grids retain bounded scrolling; only fixed-stage pages fill remaining height.');
});

test('start-aligned DisplayBoard stretches direct form containers without changing centered control alignment', async () => {
    const css = await source('wwwroot/display-board.css');
    const path = '.f-display-board-start > .f-display-board-viewport > .f-display-board-content';
    assert.equal(property(rule(css, path), 'align-items'), 'flex-start');
    assert.equal(property(rule(css, `${path} > :is(.f-section,.f-form-layout,.f-form-group)`), 'width'), '100%');
    const centered = '.f-display-board-centered > .f-display-board-viewport > .f-display-board-content:not(:has(> .f-code-block))';
    assert.equal(property(rule(css, centered), 'align-items'), 'center');
    assert.equal(property(rule(css, `${centered} > *`), 'align-self'), 'center');
});
