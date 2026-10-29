import assert from 'node:assert/strict';
import { readFile } from 'node:fs/promises';
import test from 'node:test';

const framework = new URL('../../src/Flourish.Blazor/Flourish.Blazor.Framework/', import.meta.url);
const source = path => readFile(new URL(path, framework), 'utf8');
const moduleUrl = text => `data:text/javascript;base64,${Buffer.from(text + '\n//# sourceURL=popup-placement-module.js\n').toString('base64')}`;
const originUrl = moduleUrl(await source('wwwroot/primitives/interaction-origin.js'));
const controlsSource = await source('wwwroot/controls.js');
const controlsUrl = moduleUrl(controlsSource.replace(/(["'])\.\/primitives\/interaction-origin\.js\1/, JSON.stringify(originUrl)));
const api = await import(controlsUrl);
const referenceSource = await source('wwwroot/primitives/reference-dropdown.js');
const references = await import(moduleUrl(referenceSource.replace(/(["'])\.\.\/controls\.js\1/, JSON.stringify(controlsUrl))));

class Style {
    values = new Map(); priorities = new Map();
    getPropertyValue(name) { return this[name] ?? this.values.get(name) ?? ''; }
    getPropertyPriority(name) { return this.priorities.get(name) ?? ''; }
    setProperty(name, value, priority = '') { this.writeCount = (this.writeCount ?? 0) + 1; this.values.set(name, String(value)); this.priorities.set(name, priority); this[name] = String(value); }
    removeProperty(name) { this.values.delete(name); this.priorities.delete(name); delete this[name]; }
}
class Events {
    listeners = new Map();
    addEventListener(type, callback, options) {
        const list = this.listeners.get(type) ?? [];
        list.push({ callback, capture: options === true || options?.capture === true }); this.listeners.set(type, list);
    }
    removeEventListener(type, callback) { this.listeners.set(type, (this.listeners.get(type) ?? []).filter(item => item.callback !== callback)); }
    emit(type, target, afterDispatch) {
        const event = { type, target, key: 'ArrowDown', defaultPrevented: false };
        for (const capture of [true, false]) for (const item of [...(this.listeners.get(type) ?? [])]) if (item.capture === capture) item.callback(event);
        afterDispatch?.();
    }
}
class Element extends Events {
    constructor(tag = 'div', rect = {}) {
        super(); this.tagName = tag.toUpperCase(); this.localName = tag; this.nodeType = 1;
        this.rect = { left: 100, top: 100, width: 48, height: 48, ...rect };
        this.style = new Style(); this.dataset = {}; this.attributes = new Map(); this.isConnected = true;
        this.children = []; this.parentElement = null; this.cssMaxHeight = 'none'; this.measurements = 0;
    }
    setAttribute(name, value) {
        this.attributeWrites = (this.attributeWrites ?? 0) + 1;
        this.attributes.set(name, String(value));
        if (name.startsWith('data-')) this.dataset[name.slice(5).replace(/-([a-z])/g, (_, letter) => letter.toUpperCase())] = String(value);
    }
    get childNodes() { return this.nodes ?? this.children; }
    get childElementCount() { return this.children.length; }
    appendChild(node) {
        this.nodes ??= [...this.children]; this.nodes.push(node);
        if (node.nodeType === 1) this.children.push(node);
        node.parentElement = this; node.ownerDocument = this.ownerDocument;
        return node;
    }
    remove() {
        const parent = this.parentElement;
        if (parent) {
            parent.children = parent.children.filter(node => node !== this);
            if (parent.nodes) parent.nodes = parent.nodes.filter(node => node !== this);
        }
        this.parentElement = null;
    }
    cloneNode() {
        const clone = new Element(this.localName, this.rect);
        clone.attributes = new Map(this.attributes); return clone;
    }
    contains(node) { return node === this || this.children.some(child => child.contains(node)); }
    getAttribute(name) { return this.attributes.get(name) ?? null; }
    hasAttribute(name) { return this.attributes.has(name); }
    removeAttribute(name) { this.attributes.delete(name); }
    matches(selector) {
        if (selector === ':popover-open') return this.popoverOpen === true;
        if (selector.startsWith('.')) return selector.split(',').some(part =>
            part.trim().slice(1).split('.').every(name => (this.getAttribute('class') ?? '').split(/\s+/).includes(name)));
        return this.localName === 'select' && /select|\.f-select/.test(selector);
    }
    closest(selector) { return this.matches(selector) ? this : this.parentElement?.closest(selector) ?? null; }
    querySelectorAll(selector) { return this.children.flatMap(child => [...(child.matches(selector) ? [child] : []), ...child.querySelectorAll(selector)]); }
    showPopover() { this.showCount = (this.showCount ?? 0) + 1; this.visibilityAtShow = this.style.getPropertyValue('visibility'); this.popoverOpen = true; }
    hidePopover() { this.hideCount = (this.hideCount ?? 0) + 1; this.popoverOpen = false; }
    getClientRects() { return this.isConnected ? [this.getBoundingClientRect()] : []; }
    getBoundingClientRect() {
        this.measurements++;
        if (this.style.left === '-100000px') {
            this.ownerDocument.probeMeasurements ??= [];
            this.ownerDocument.probeMeasurements.push(this);
            this.ownerDocument.inspectProbe?.(this);
            return { width: this.ownerDocument.probeWidth ?? 200 };
        }
        const limits = (this.style.getPropertyValue('max-height') || this.cssMaxHeight).match(/[\d.]+(?=px)/g)?.map(Number) ?? [];
        const dropdown = this.matches('.f-menu-panel,.selection-panel,.search-autocomplete-options');
        const minimum = dropdown ? Math.min(...(this.style.getPropertyValue('--f-popup-trigger-width').match(/[\d.]+(?=px)/g)?.map(Number) ?? [0])) : 0;
        const width = Math.min(Math.max(this.rect.width, minimum), this.cssMaxWidth ?? Infinity), height = Math.min(this.rect.height, ...limits);
        return { ...this.rect, width, height, right: this.rect.left + width, bottom: this.rect.top + height };
    }
}
function environment() {
    const names = ['document', 'window', 'getComputedStyle', 'requestAnimationFrame', 'cancelAnimationFrame', 'MutationObserver', 'ResizeObserver', 'HTMLSelectElement', 'HTMLElement', 'CSS'];
    const saved = new Map(names.map(name => [name, Object.getOwnPropertyDescriptor(globalThis, name)]));
    const window = new Events(), document = new Element();
    document.nodeType = 9; document.ownerDocument = null; document.documentElement = { clientWidth: 800 }; document.body = new Element();
    document.defaultView = window; window.document = document; document.body.ownerDocument = document; window.innerWidth = 800; window.innerHeight = 600;
    document.createElement = tag => { assert.notEqual(tag, 'canvas', 'Native geometry must not measure text on canvas.'); return new Element(tag); };
    globalThis.window = window; globalThis.document = document;
    globalThis.HTMLSelectElement = Element; globalThis.HTMLElement = Element;
    globalThis.CSS = { supports: () => true, escape: value => value };
    globalThis.getComputedStyle = (node, pseudo) => {
        const theme = pseudo ? node.pseudoTheme?.[pseudo] ?? {} : node.computedTheme ?? {};
        const keys = [...new Set([...node.style.values.keys(), ...Object.keys(theme)])];
        return Object.assign({ display: theme.display ?? 'block', maxHeight: node.style.getPropertyValue('max-height') || node.cssMaxHeight,
            maxWidth: node.style.getPropertyValue('max-width') || theme['max-width'] || (node.cssMaxWidth === undefined ? 'none' : `${node.cssMaxWidth}px`),
            getPropertyValue: name => node.style.getPropertyValue(name) || theme[name] || '', length: keys.length }, keys);
    };
    window.getComputedStyle = globalThis.getComputedStyle;
    globalThis.requestAnimationFrame = () => { throw new Error('Popup placement must finish before the native default opening, without a later animation frame.'); };
    globalThis.cancelAnimationFrame = () => {};
    const mutations = [];
    globalThis.MutationObserver = class { constructor(callback) { this.callback = callback; mutations.push(this); } observe(target, options) { this.target = target; this.options = options; } disconnect() { this.disconnected = true; } };
    const resizes = [];
    globalThis.ResizeObserver = class {
        targets = new Set();
        constructor(callback) { this.callback = callback; resizes.push(this); }
        observe(target) { this.targets.add(target); }
        unobserve(target) { this.targets.delete(target); }
        disconnect() { this.targets.clear(); this.disconnected = true; }
        emit(target) { assert.ok(this.targets.has(target), 'Only observed targets may resize.'); this.callback([{ target }]); }
    };
    return {
        document, window, mutations, resizes,
        adopt(node, parent = document) { node.ownerDocument = document; node.parentElement = parent; parent.children.push(node); return node; },
        restore() { for (const [name, descriptor] of saved) if (descriptor) Object.defineProperty(globalThis, name, descriptor); else delete globalThis[name]; }
    };
}
function run(action) { const env = environment(); try { action(env); } finally { env.restore(); } }
function popup(triggerRect = {}, panelRect = {}, options = {}) {
    const trigger = new Element('button', triggerRect), panel = new Element('div', { width: 120, height: 120, ...panelRect });
    panel.setAttribute('class', 'f-menu-panel');
    return { trigger, panel, open: true, openOnHover: false, maximumHeight: '', maximumHeightPriority: '', ...options };
}
const left = state => Number.parseFloat(state.panel.style.getPropertyValue('left'));
const top = state => Number.parseFloat(state.panel.style.getPropertyValue('top'));

test('popup chooses physical left/right edges from the actual trigger center', () => run(() => {
    for (const [rect, expected] of [[{ left: 100 }, 100], [{ left: 650 }, 578]]) {
        const state = popup(rect); state.hoverRoot = new Element('div', { left: 0, width: 800 });
        api.positionPopup(state); assert.equal(left(state), expected);
        assert.equal(state.hoverRoot.measurements, 0, 'A full-width hover region must not become the geometric anchor.');
    }
}));
test('popup clamps horizontal placement to both viewport gutters', () => run(() => {
    for (const [rect, expected] of [[{ left: -20 }, 12], [{ left: 790 }, 668]]) {
        const state = popup(rect); api.positionPopup(state); assert.equal(left(state), expected);
    }
}));
test('popup fills the trigger while retaining longer natural content and an independently owned CSS width', () => run(() => {
    const narrow = popup({ left: 100, width: 300 }, { width: 80 }); api.positionPopup(narrow);
    assert.equal(left(narrow), 100); assert.equal(narrow.panel.getBoundingClientRect().width, 300);
    const long = popup({ left: 650 }, { width: 2000 }); long.panel.cssMaxWidth = 776; api.positionPopup(long);
    assert.equal(left(long), 12); assert.equal(long.panel.getBoundingClientRect().width, 776);
    for (const state of [narrow, long]) for (const property of ['width', 'min-width', 'inline-size', 'min-inline-size'])
        assert.equal(state.panel.style.getPropertyValue(property), '', `${property} must stay owned by intrinsic CSS.`);
}));
test('popup floor follows trigger and filtered natural content in both directions without retaining the prior width', () => run(() => {
    const state = popup({ width: 420 }, { width: 200 });
    api.positionPopup(state); assert.equal(state.panel.getBoundingClientRect().width, 420);
    state.trigger.rect.width = 80; api.positionPopup(state); assert.equal(state.panel.getBoundingClientRect().width, 200);
    state.panel.rect.width = 110; api.positionPopup(state); assert.equal(state.panel.getBoundingClientRect().width, 110);
    state.panel.rect.width = 310; api.positionPopup(state); assert.equal(state.panel.getBoundingClientRect().width, 310);
    state.trigger.rect.width = 360; api.positionPopup(state); assert.equal(state.panel.getBoundingClientRect().width, 360);
}));
test('trigger floor respects explicit maximum and the client viewport even when CSS vw includes a scrollbar', () => run(env => {
    env.window.innerWidth = 800; env.document.documentElement.clientWidth = 783;
    const state = popup({ left: 0, width: 900 }, { width: 80 });
    api.positionPopup(state); assert.equal(state.panel.getBoundingClientRect().width, 759); assert.equal(left(state), 12);
    state.panel.style.setProperty('max-width', '220px', 'important');
    api.positionPopup(state); assert.equal(state.panel.getBoundingClientRect().width, 220);
    assert.equal(state.panel.style.getPropertyValue('max-width'), '220px');
    assert.equal(state.panel.style.getPropertyPriority('max-width'), 'important');
}));
test('non-dropdown tutorial placement retains its authored intrinsic width', () => run(() => {
    const state = popup({ width: 420 }, { width: 80 }); state.panel.setAttribute('class', 'f-tutorial-preview');
    api.positionPopup(state); assert.equal(state.panel.getBoundingClientRect().width, 80);
    assert.equal(state.panel.style.getPropertyValue('--f-popup-trigger-width'), '');
}));
test('popup remains below when there is room and keeps hover transfer gap semantics', () => run(() => {
    const action = popup(); api.positionPopup(action); assert.equal(top(action), 154);
    const hover = popup({}, {}, { openOnHover: true }); api.positionPopup(hover); assert.equal(top(hover), 148);
}));
test('popup flips above and caps tall contents to the selected side', () => run(() => {
    const state = popup({ top: 520 }, { height: 900 }); api.positionPopup(state);
    assert.equal(state.panel.style.getPropertyValue('max-height'), '502px'); assert.equal(top(state), 12);
}));
test('popup preserves its supplied height cap and priority while applying available space', () => run(() => {
    const state = popup({ top: 520 }, { height: 400 }, { maximumHeight: '240px', maximumHeightPriority: 'important' });
    api.positionPopup(state);
    assert.equal(state.panel.style.getPropertyValue('max-height'), 'min(502px, 240px)');
    assert.equal(state.panel.style.getPropertyPriority('max-height'), 'important');
    assert.equal(top(state), 274);
}));
test('closed popup is not measured or repositioned', () => run(() => {
    const state = popup({}, {}, { open: false }); api.positionPopup(state);
    assert.equal(state.trigger.measurements, 0); assert.equal(state.panel.measurements, 0); assert.equal(left(state), NaN);
}));
test('native picker initial alignment and activation events are synchronous and use select geometry', () => run(env => {
    const select = env.adopt(new Element('select', { left: 100 }));
    const inert = env.adopt(new Element('button'), select);
    inert.getBoundingClientRect = () => { throw new Error('An inert child is not the native select anchor.'); };
    api.initializeNativePickers(env.document); assert.equal(select.getAttribute('data-f-popup-align'), 'left');
    for (const [event, offset, expected] of [['pointerdown', 650, 'right'], ['focusin', 100, 'left'], ['keydown', 650, 'right']]) {
        select.rect.left = offset;
        env.document.emit(event, inert, () => assert.equal(select.getAttribute('data-f-popup-align'), expected, `${event}: native default opening must see the new side.`));
        assert.ok((env.document.listeners.get(event) ?? []).some(listener => listener.capture), `${event} must run before native activation.`);
    }
    assert.equal(select.style.getPropertyValue('--f-popup-trigger-width'), '48px');
    assert.equal(select.style.getPropertyValue('--f-picker-inline-size'), '', 'A select without options has no natural option measurement.');
}));
test('native initialization installs document listeners once and covers inserted scoped selects', () => run(env => {
    const select = env.adopt(new Element('select', { left: 100 })); api.initializeNativePickers(env.document); api.initializeNativePickers(env.document);
    for (const type of ['pointerdown', 'focusin', 'keydown']) assert.equal(env.document.listeners.get(type)?.length, 1, type);
    const region = env.adopt(new Element()), inserted = env.adopt(new Element('select', { left: 650 }), region);
    api.initializeNativePickers(region); assert.equal(inserted.getAttribute('data-f-popup-align'), 'right');
    select.rect.left = 650; env.window.emit('resize', env.window); assert.equal(select.getAttribute('data-f-popup-align'), 'right');
    inserted.rect.left = 100; env.document.emit('scroll', env.document); assert.equal(inserted.getAttribute('data-f-popup-align'), 'left');
}));
test('picker and menu CSS retain intrinsic width without the UA anchor minimum or width animation', async () => {
    const design = new URL('../../src/Flourish.Blazor/Flourish.Blazor.Design/wwwroot/', import.meta.url);
    const css = [await source('wwwroot/form-inputs.css'), ...(await Promise.all(['controls.css', 'dropdown.css'].map(path => readFile(new URL(path, design), 'utf8'))))].join('\n').replace(/\/\*[\s\S]*?\*\//g, '');
    const blocks = [...css.matchAll(/([^{}]+)\{([^{}]*)\}/g)].filter(match => /\.f-select::picker\(select\)/.test(match[1]));
    assert.ok(blocks.some(match => /(?:width|inline-size)\s*:\s*var\(--f-picker-inline-size,\s*max-content\)/.test(match[2])), 'Picker consumes the pre-open intrinsic measurement with a CSS-only intrinsic default.');
    assert.ok(blocks.some(match => /min-inline-size\s*:\s*min\(var\(--f-popup-trigger-width,0px\),calc\(100vw - 24px\)\)/.test(match[2])), 'The explicit trigger floor remains bounded by the viewport.');
    assert.ok(blocks.some(match => /max-(?:width|inline-size)\s*:/.test(match[2])), 'Long option labels need a viewport width constraint.');
    for (const block of blocks) assert.doesNotMatch(block[2], /transition(?:-property)?\s*:[^;}]*(?:\bwidth\b|\binline-size\b|\ball\b)/);
    assert.doesNotMatch(controlsSource, /createElement\(['"]canvas['"]\)|measureText\(/);
});

async function runAsync(action) { const env = environment(); try { await action(env); } finally { env.restore(); } }
const listeners = (target, type) => target.listeners.get(type)?.length ?? 0;
const flushCallbacks = async () => { await Promise.resolve(); await Promise.resolve(); };
function selectionFixture(env, kind, withPanel = true) {
    const root = env.adopt(new Element(kind === 'reference' ? 'div' : 'span'));
    const trigger = env.adopt(new Element(kind === 'reference' ? 'button' : 'input'), root);
    trigger.value = 'selection-owner-value';
    if (kind === 'search') trigger.setAttribute('role', 'combobox');
    root.setAttribute('data-f-popup-strength', 'weak');
    root.currentPanel = null;
    root.querySelector = selector => selector.includes('.selection-panel') || selector.includes('.search-autocomplete-options') ? root.currentPanel : trigger;
    const calls = [];
    const reference = { invokeMethodAsync(method, ...args) { calls.push({ method, args }); return Promise.resolve(); } };
    function addPanel() {
        const panel = env.adopt(new Element(kind === 'reference' ? 'div' : 'span', { width: 160, height: 120 }), root);
        panel.setAttribute('class', kind === 'reference' ? 'selection-panel' : 'search-autocomplete-options');
        panel.style.setProperty('position', 'absolute'); panel.style.setProperty('left', '7px'); panel.style.setProperty('top', '11px');
        const option = env.adopt(new Element(kind === 'reference' ? 'button' : 'span'), panel);
        option.setAttribute('role', 'option'); option.setAttribute('aria-selected', 'true'); option.value = 'selected-key';
        root.currentPanel = panel; return { panel, option };
    }
    return { root, trigger, calls, reference, addPanel, ...(withPanel ? addPanel() : {}) };
}
for (const kind of ['reference', 'search']) {
    test(`${kind}: trigger resize refreshes the floor and replacement/detach restore authored sizing and priority`, () => run(env => {
        const f = selectionFixture(env, kind);
        f.panel.style.setProperty('--f-popup-trigger-width', '19px', 'important');
        f.panel.style.setProperty('max-width', '600px', 'important');
        f.trigger.rect.width = 420;
        try {
            references.attachPopupDismissal(f.root, f.reference);
            const resize = env.resizes[0];
            assert.equal(f.panel.getBoundingClientRect().width, 420);
            f.trigger.rect.width = 80; resize.emit(f.trigger);
            assert.equal(f.panel.getBoundingClientRect().width, 160, 'The reference sizer or live search content remains the natural width.');
            f.panel.rect.width = 230; env.mutations[0].callback();
            assert.equal(f.panel.getBoundingClientRect().width, 230);
            const old = f.panel, replacement = f.addPanel(); env.mutations[0].callback();
            assert.equal(old.style.getPropertyValue('--f-popup-trigger-width'), '19px');
            assert.equal(old.style.getPropertyPriority('--f-popup-trigger-width'), 'important');
            assert.equal(old.style.getPropertyValue('max-width'), '600px');
            assert.equal(old.style.getPropertyPriority('max-width'), 'important');
            assert.equal(replacement.panel.getBoundingClientRect().width, 160);
            assert.deepEqual(f.calls, [], 'Geometry changes must not dispatch selection or dismissal.');
            references.detachPopupDismissal(f.root);
            assert.equal(replacement.panel.style.getPropertyValue('--f-popup-trigger-width'), '');
            assert.equal(resize.disconnected, true); assert.equal(resize.targets.size, 0);
        } finally { references.detachPopupDismissal(f.root); }
    }));
    test(`${kind}: an actually rendered panel enters manual top layer before shared left/right/up/down placement`, () => run(env => {
        const f = selectionFixture(env, kind, false);
        try {
            references.attachPopupDismissal(f.root, f.reference);
            assert.equal(f.calls.length, 0, 'Attaching geometry must not request a selection or dismissal.');
            const { panel, option } = f.addPanel();
            env.mutations[0].callback();
            assert.equal(panel.getAttribute('popover'), 'manual'); assert.equal(panel.matches(':popover-open'), true);
            assert.equal(panel.visibilityAtShow, 'hidden', 'First top-layer measurement must be visually hidden.');
            assert.equal(panel.style.getPropertyValue('visibility'), '', 'Visibility must be restored in the same synchronous attachment.');
            assert.equal(panel.style.getPropertyValue('position'), 'fixed');
            assert.equal(left({ panel }), 100); assert.equal(top({ panel }), 154);
            f.trigger.rect.left = 650; env.window.emit('resize', env.window);
            assert.equal(panel.getAttribute('data-f-popup-align'), 'right'); assert.equal(left({ panel }), 538);
            f.trigger.rect.top = 520; env.document.emit('scroll', env.document);
            assert.equal(top({ panel }), 394); assert.equal(panel.style.getPropertyValue('max-height'), '502px');
            f.trigger.rect.left = 100; f.trigger.rect.top = 100; env.mutations[0].callback();
            assert.equal(panel.getAttribute('data-f-popup-align'), 'left'); assert.equal(left({ panel }), 100); assert.equal(top({ panel }), 154);
            assert.equal(panel.showCount, 1, 'Repositioning must not reopen or toggle the native top layer.');
            assert.equal(f.trigger.value, 'selection-owner-value'); assert.equal(option.value, 'selected-key'); assert.equal(option.getAttribute('aria-selected'), 'true');
            assert.equal(f.root.listeners.size + f.trigger.listeners.size + panel.listeners.size, 0, 'Geometry must not install an independent selection/input/keyboard controller.');
        } finally { references.detachPopupDismissal(f.root); }
    }));
    test(`${kind}: repeated attach updates the circuit reference without duplicate document listeners`, () => run(env => {
        const f = selectionFixture(env, kind), updatedCalls = [];
        try {
            references.attachPopupDismissal(f.root, f.reference);
            f.trigger.rect.left = 650;
            references.attachPopupDismissal(f.root, { invokeMethodAsync(method) { updatedCalls.push(method); return Promise.resolve(); } });
            references.attachPopupDismissal(f.root, { invokeMethodAsync(method) { updatedCalls.push(method); return Promise.resolve(); } });
            assert.equal(env.mutations.length, 1); assert.equal(f.panel.showCount, 1); assert.equal(left({ panel: f.panel }), 538);
            assert.equal(listeners(env.document, 'pointerdown'), 1); assert.equal(listeners(env.document, 'scroll'), 1); assert.equal(listeners(env.window, 'resize'), 1);
            const outside = env.adopt(new Element()); env.document.emit('pointerdown', outside);
            assert.deepEqual(updatedCalls, ['RequestDismissAsync']); assert.equal(f.calls.length, 0);
            assert.equal(f.panel.matches(':popover-open'), true, 'A circuit dismissal request must not itself mutate IsOpen or selection DOM.');
        } finally { references.detachPopupDismissal(f.root); }
    }));
    test(`${kind}: weak outside dismissal respects inside clicks, Strong, aria-busy and pending circuit calls`, () => runAsync(async env => {
        const f = selectionFixture(env, kind), outside = env.adopt(new Element()); let resolvePending;
        const invocations = [];
        const pendingReference = { invokeMethodAsync(method, ...args) { invocations.push({ method, args }); return new Promise(resolve => { resolvePending = resolve; }); } };
        try {
            references.attachPopupDismissal(f.root, pendingReference);
            env.document.emit('pointerdown', f.option); assert.equal(invocations.length, 0);
            f.root.setAttribute('data-f-popup-strength', 'strong'); env.document.emit('pointerdown', outside); assert.equal(invocations.length, 0);
            f.root.setAttribute('data-f-popup-strength', 'weak'); f.root.setAttribute('aria-busy', 'true');
            env.document.emit('pointerdown', outside); assert.equal(invocations.length, 0);
            f.root.setAttribute('aria-busy', 'false'); env.document.emit('pointerdown', outside); env.document.emit('pointerdown', outside);
            assert.deepEqual(invocations, [{ method: 'RequestDismissAsync', args: [] }], 'Pending dismissal must be serialized and routed only to its circuit owner.');
            assert.equal(f.panel.popoverOpen, true); assert.equal(f.option.getAttribute('aria-selected'), 'true'); assert.equal(f.trigger.value, 'selection-owner-value');
            resolvePending(); await flushCallbacks(); env.document.emit('pointerdown', outside); assert.equal(invocations.length, 2);
            resolvePending(); await flushCallbacks();
            references.attachPopupDismissal(f.root, { invokeMethodAsync() { return Promise.reject(new Error('circuit disposed')); } });
            env.document.emit('pointerdown', outside); await flushCallbacks();
            references.attachPopupDismissal(f.root, f.reference); env.document.emit('pointerdown', outside); await flushCallbacks();
            assert.deepEqual(f.calls, [{ method: 'RequestDismissAsync', args: [] }], 'A failed circuit request must release the pending guard for the next owner.');
        } finally { references.detachPopupDismissal(f.root); }
    }));
    test(`${kind}: replaced panels, explicit detach and removed roots release geometry, top layer and listeners`, () => run(env => {
        const f = selectionFixture(env, kind);
        references.attachPopupDismissal(f.root, f.reference);
        const original = f.panel;
        const replacement = f.addPanel(); env.mutations[0].callback();
        assert.equal(original.matches(':popover-open'), false); assert.equal(original.hideCount, 1);
        assert.equal(original.style.getPropertyValue('position'), 'absolute'); assert.equal(original.style.getPropertyValue('left'), '7px'); assert.equal(original.style.getPropertyValue('top'), '11px');
        assert.equal(replacement.panel.matches(':popover-open'), true); assert.equal(replacement.panel.showCount, 1);
        references.detachPopupDismissal(f.root); references.detachPopupDismissal(f.root);
        assert.equal(replacement.panel.matches(':popover-open'), false); assert.equal(replacement.panel.hideCount, 1);
        assert.equal(replacement.panel.style.getPropertyValue('position'), 'absolute');
        assert.equal(listeners(env.document, 'pointerdown') + listeners(env.document, 'scroll') + listeners(env.window, 'resize'), 0);
        assert.equal(env.mutations[0].disconnected, true);
        references.attachPopupDismissal(f.root, f.reference); assert.equal(env.mutations.length, 2);
        f.root.isConnected = false; replacement.panel.isConnected = false; replacement.panel.popoverOpen = false;
        env.mutations[1].callback();
        assert.equal(env.mutations[1].disconnected, true);
        assert.equal(listeners(env.document, 'pointerdown') + listeners(env.document, 'scroll') + listeners(env.window, 'resize'), 0);
        assert.equal(replacement.panel.style.getPropertyValue('position'), 'absolute');
        references.attachPopupDismissal(f.root, f.reference); assert.equal(env.mutations.length, 2, 'Removed roots must not acquire a new lifetime.');
    }));
}

test('shared menus resize without a window event and restore authored floor on close, reopen and detach', () => run(env => {
    const trigger = env.adopt(new Element('button', { width: 420 })), panel = env.adopt(new Element('div', { width: 160 }));
    panel.setAttribute('class', 'f-menu-panel');
    panel.style.setProperty('--f-popup-trigger-width', '19px', 'important');
    trigger.computedTheme = { '--f-popup-trigger-width': '999px' };
    try {
        api.attachMenu(trigger, panel); api.toggleMenu(trigger, panel, false);
        const resize = env.resizes[0];
        assert.equal(panel.getBoundingClientRect().width, 420);
        trigger.rect.width = 80; resize.emit(trigger); assert.equal(panel.getBoundingClientRect().width, 160);
        api.closeMenu(trigger, panel);
        assert.equal(panel.style.getPropertyValue('--f-popup-trigger-width'), '19px');
        assert.equal(panel.style.getPropertyPriority('--f-popup-trigger-width'), 'important');
        const writes = panel.style.writeCount; resize.emit(trigger);
        assert.equal(panel.style.writeCount, writes, 'Closed menus must not measure or rewrite the floor.');
        trigger.rect.width = 210; api.toggleMenu(trigger, panel, false); assert.equal(panel.getBoundingClientRect().width, 210);
        api.detachMenu(trigger, panel);
        assert.equal(panel.style.getPropertyValue('--f-popup-trigger-width'), '19px');
        assert.equal(panel.style.getPropertyPriority('--f-popup-trigger-width'), 'important');
        assert.equal(resize.disconnected, true);
    } finally { api.detachMenu(trigger, panel); }
}));

test('nested menu copies semantic theme roles while its own geometry overrides inherited popup direction', () => run(env => {
    for (const [triggerLeft, inherited, actual] of [[100, 'right', 'left'], [650, 'left', 'right']]) {
        const trigger = env.adopt(new Element('button', { left: triggerLeft })), panel = env.adopt(new Element('div', { width: 120, height: 120 }));
        trigger.computedTheme = { '--f-popup-align': inherited, '--f-accent': '#16745F', '--f-surface': '#FFFFFF',
            '--f-surface-preview': '#E5E8EB', '--f-target-preview': '#395A48', 'font-size': '17px' };
        panel.style.setProperty('--f-accent', '#old-instance');
        try {
            api.attachMenu(trigger, panel); api.toggleMenu(trigger, panel, false);
            assert.equal(panel.matches(':popover-open'), true); assert.equal(panel.getAttribute('data-f-popup-align'), actual);
            assert.equal(panel.style.getPropertyValue('--f-popup-align'), '', 'Parent popup direction must not become an inline theme override on the nested panel.');
            assert.equal(panel.style.getPropertyValue('--f-accent'), '#16745F'); assert.equal(panel.style.getPropertyValue('--f-surface'), '#FFFFFF');
            assert.equal(panel.style.getPropertyValue('--f-target-preview'), '#E5E8EB', 'Scoped semantic preview roles still need ordinary theme propagation.');
            assert.equal(panel.style.getPropertyValue('font-size'), '17px');
            assert.equal(left({ panel }), actual === 'left' ? 100 : 578);
        } finally { api.detachMenu(trigger, panel); }
        assert.equal(panel.style.getPropertyValue('--f-accent'), '#old-instance');
    }
}));


test('native pre-open synchronization does not rewrite an unchanged alignment or invalidate intrinsic sizing', () => run(env => {
    const select = env.adopt(new Element('select', { left: 100 }));
    api.initializeNativePickers(env.document);
    assert.equal(select.getAttribute('data-f-popup-align'), 'left');
    assert.equal(select.attributeWrites, 1);
    api.initializeNativePickers(env.document);
    for (const event of ['pointerdown', 'focusin', 'keydown']) env.document.emit(event, select);
    assert.equal(select.attributeWrites, 1, 'Opening events must not rewrite the same attribute while the native picker is being laid out.');
    select.rect.left = 650; env.document.emit('pointerdown', select);
    assert.equal(select.attributeWrites, 2); assert.equal(select.getAttribute('data-f-popup-align'), 'right');
    env.document.emit('focusin', select); env.document.emit('keydown', select);
    assert.equal(select.attributeWrites, 2);
}));

// These fixtures check synchronous lifecycle and copied metrics, not browser layout or physical first-frame painting.
class PickerText {
    nodeType = 3; childNodes = [];
    constructor(text) { this.textContent = text; }
    cloneNode() { return new PickerText(this.textContent); }
}
function measuredSelect(env, labels = ['English', 'Português (Brasil)', '简体中文']) {
    const select = env.adopt(new Element('select', { left: 650 }));
    select.value = 'selected-native-value';
    select.pseudoTheme = { '::picker(select)': { 'font-size': '17px', 'padding-left': '6px', 'padding-right': '6px',
        'border-left-width': '1px', 'border-right-width': '1px', 'border-left-style': 'solid', 'border-right-style': 'solid' } };
    for (const text of labels) {
        const option = env.adopt(new Element('option'), select);
        option.value = text; option.label = text;
        option.computedTheme = { 'font-family': 'Example Sans', 'font-size': '17px', 'letter-spacing': '.25px',
            'padding-left': '10px', 'padding-right': '10px', 'column-gap': '10px', 'grid-template-columns': '28px minmax(0px, 1fr)' };
        option.pseudoTheme = { '::checkmark': { 'box-sizing': 'border-box', 'width': '28px', 'height': '28px',
            'border-left-width': '1px', 'border-right-width': '1px', 'border-left-style': 'solid', 'border-right-style': 'solid' } };
        option.nodes = [new PickerText(text)];
    }
    return select;
}
test('native trigger resizing retains independent natural option width and never reuses a former wide floor', () => run(env => {
    const select = measuredSelect(env); select.rect.width = 420;
    api.initializeNativePickers(env.document);
    const resize = env.resizes[0];
    assert.equal(select.style.getPropertyValue('--f-popup-trigger-width'), '420px');
    assert.equal(select.style.getPropertyValue('--f-picker-inline-size'), '200px');
    select.rect.width = 80; resize.emit(select);
    assert.equal(select.style.getPropertyValue('--f-popup-trigger-width'), '80px');
    assert.equal(select.style.getPropertyValue('--f-picker-inline-size'), '200px');
    select.rect.width = 900; env.document.documentElement.clientWidth = 783; resize.emit(select);
    assert.equal(select.style.getPropertyValue('--f-popup-trigger-width'), '759px');
    select.pseudoTheme['::picker(select)']['max-width'] = '220px'; resize.emit(select);
    assert.equal(select.style.getPropertyValue('--f-popup-trigger-width'), 'min(759px, 220px)');
    env.document.probeWidth = 110; env.document.emit('pointerdown', select);
    assert.equal(select.style.getPropertyValue('--f-picker-inline-size'), '110px', 'Reopening remeasures natural options without the former trigger floor.');
    select.isConnected = false; resize.emit(select); assert.equal(resize.targets.has(select), false);
}));
test('native picker settles one rounded intrinsic width before the opening default and never leaves its probe mounted', () => run(env => {
    const select = measuredSelect(env); env.document.probeWidth = 243.125;
    env.document.inspectProbe = probe => {
        assert.equal(probe.inert, true); assert.equal(probe.getAttribute('aria-hidden'), 'true');
        assert.equal(probe.style.visibility, 'hidden'); assert.equal(probe.style.width, 'max-content');
        assert.equal(probe.style.getPropertyValue('padding-left'), '6px');
        assert.equal(probe.style.getPropertyValue('border-left-width'), '1px');
        assert.equal(probe.children.length, 3);
        const row = probe.children[1], [marker, label] = row.children;
        assert.equal(row.style.display, 'grid'); assert.equal(row.style.gridTemplateColumns, '28px max-content');
        assert.equal(row.style.getPropertyValue('letter-spacing'), '.25px'); assert.equal(row.style.getPropertyValue('column-gap'), '10px');
        assert.equal(marker.style.getPropertyValue('width'), '28px'); assert.equal(marker.style.getPropertyValue('height'), '28px');
        assert.equal(label.childNodes[0].textContent, 'Português (Brasil)');
        assert.equal(select.value, 'selected-native-value'); assert.equal(select.getAttribute('data-f-popup-align'), 'right');
    };
    api.initializeNativePickers(env.document);
    assert.equal(select.style.getPropertyValue('--f-picker-inline-size'), '244px');
    assert.equal(env.document.body.children.length, 0);
    env.document.probeWidth = 260.25;
    env.document.emit('pointerdown', select, () => assert.equal(select.style.getPropertyValue('--f-picker-inline-size'), '261px'));
    assert.equal(env.document.body.children.length, 0);
    assert.equal(select.children.length, 3); assert.equal(select.value, 'selected-native-value');
}));
test('native measurement respects changed choice sizes on every opening without changing selection', () => run(env => {
    const select = measuredSelect(env); api.initializeNativePickers(env.document);
    for (const size of ['28px', '36px', '48px']) {
        for (const option of select.children) {
            option.computedTheme['grid-template-columns'] = size + ' minmax(0px, 1fr)';
            option.pseudoTheme['::checkmark'].width = size; option.pseudoTheme['::checkmark'].height = size;
        }
        env.document.inspectProbe = probe => {
            for (const row of probe.children) {
                assert.equal(row.style.gridTemplateColumns, size + ' max-content');
                assert.equal(row.children[0].style.getPropertyValue('width'), size);
                assert.equal(row.children[0].style.getPropertyValue('height'), size);
            }
        };
        env.document.emit('pointerdown', select);
        assert.equal(select.value, 'selected-native-value'); assert.equal(select.children.length, 3);
        assert.equal(env.document.body.children.length, 0);
    }
}));

test('native discovery does not remeasure its own removed probes but every new opening remeasures changed labels and fonts', () => run(env => {
    const select = measuredSelect(env); api.initializeNativePickers(env.document);
    api.initializeNativePickers(env.document); api.initializeNativePickers(select);
    assert.equal(env.document.probeMeasurements.length, 1);
    const changed = select.children[0]; changed.nodes[0].textContent = 'A newly localized longer label'; changed.computedTheme['font-family'] = 'New Font';
    env.document.probeWidth = 321.5;
    env.document.emit('focusin', select);
    assert.equal(select.style.getPropertyValue('--f-picker-inline-size'), '322px');
    assert.equal(env.document.probeMeasurements.at(-1).children[0].style.getPropertyValue('font-family'), 'New Font');
    assert.equal(env.document.probeMeasurements.at(-1).children[0].children[1].childNodes[0].textContent, 'A newly localized longer label');
    const writes = select.style.writeCount;
    env.document.emit('keydown', select); assert.equal(select.style.writeCount, writes); assert.equal(select.attributeWrites, 1);
}));
test('rich option content and group labels retain actual metrics while IDs stay out of the inert probe', () => run(env => {
    const select = measuredSelect(env, ['Rich text']);
    const option = select.children[0], icon = new Element('svg'); icon.ownerDocument = env.document;
    icon.setAttribute('id', 'real-option-icon'); icon.computedTheme = { 'display': 'inline-block', 'width': '24px', 'height': '24px' };
    option.nodes.unshift(icon);
    const group = env.adopt(new Element('optgroup'), select); group.label = 'Group header';
    group.pseudoTheme = { '::before': { 'font-weight': '700' } };
    const nested = new Element('option'); nested.ownerDocument = env.document; nested.nodes = [new PickerText('Ignored child')];
    nested.setAttribute('label', 'Displayed option label'); nested.label = 'Displayed option label'; group.appendChild(nested);
    api.initializeNativePickers(env.document);
    const probe = env.document.probeMeasurements[0], clone = probe.children[0].children[1].childNodes[0];
    assert.equal(clone.localName, 'svg'); assert.equal(clone.hasAttribute('id'), false); assert.equal(icon.getAttribute('id'), 'real-option-icon');
    assert.equal(clone.style.getPropertyValue('width'), '24px');
    assert.equal(probe.children[1].children[0].textContent, 'Group header');
    assert.equal(probe.children[1].children[0].style.getPropertyValue('font-weight'), '700');
    assert.equal(probe.children[1].children[1].children[1].textContent, 'Displayed option label');
    assert.equal(env.document.body.children.length, 0);
}));
test('unsupported engines, hidden options and an already open picker keep their native behavior', () => run(env => {
    const select = measuredSelect(env); globalThis.CSS.supports = () => false;
    api.initializeNativePickers(env.document);
    assert.equal(select.style.getPropertyValue('--f-picker-inline-size'), ''); assert.equal(env.document.probeMeasurements, undefined);
    globalThis.CSS.supports = () => true;
    select.children[0].hidden = true; select.children[1].computedTheme.display = 'none';
    env.document.emit('pointerdown', select); assert.equal(env.document.probeMeasurements[0].children.length, 1);
    const originalMatches = select.matches.bind(select); select.matches = query => query === ':open' || originalMatches(query);
    const before = env.document.probeMeasurements.length, width = select.style.getPropertyValue('--f-picker-inline-size');
    env.document.probeWidth = 500; env.document.emit('keydown', select); env.document.emit('focusin', select);
    assert.equal(env.document.probeMeasurements.length, before); assert.equal(select.style.getPropertyValue('--f-picker-inline-size'), width);
}));
test('measurement cleanup survives an exception and the next opening can retry', () => run(env => {
    const select = measuredSelect(env);
    env.document.inspectProbe = () => { throw new Error('Controlled layout failure'); };
    assert.throws(() => api.initializeNativePickers(env.document), /Controlled layout failure/);
    assert.equal(env.document.body.children.length, 0); assert.equal(select.value, 'selected-native-value');
    delete env.document.inspectProbe; api.initializeNativePickers(env.document); env.document.emit('pointerdown', select);
    assert.equal(select.style.getPropertyValue('--f-picker-inline-size'), '200px'); assert.equal(env.document.body.children.length, 0);
}));
