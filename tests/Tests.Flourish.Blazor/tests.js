import test from 'node:test';
import assert from 'node:assert/strict';
import { readFile } from 'node:fs/promises';

const documentListeners = new Map();
globalThis.document = {
    readyState: 'complete', documentElement: {}, querySelectorAll: () => [],
    createElement: () => ({ getContext: () => ({ font: '', measureText: text => ({ width: text.length * 8 }) }) }),
    addEventListener: (name, handler) => { const handlers = documentListeners.get(name) ?? new Set(); handlers.add(handler); documentListeners.set(name, handlers); },
    removeEventListener: (name, handler) => documentListeners.get(name)?.delete(handler)
};
globalThis.MutationObserver = class {
    constructor(callback) { this.callback = callback; }
    observe(node) { assert.equal(node, document.documentElement); }
    disconnect() {}
};
globalThis.getComputedStyle = node => ({ fontWeight: '400', fontSize: '17px', fontFamily: 'Segoe UI', paddingLeft: node.tagName ? '16px' : '0px', paddingRight: node.tagName ? '16px' : '0px' });
const controlsSource = await readFile(new URL('../../src/Flourish.Blazor/Flourish.Blazor.Framework/wwwroot/controls.js', import.meta.url), 'utf8');
const originSource = await readFile(new URL('../../src/Flourish.Blazor/Flourish.Blazor.Framework/wwwroot/primitives/interaction-origin.js', import.meta.url), 'utf8');
const originUrl = `data:text/javascript;base64,${Buffer.from(originSource).toString('base64')}`;
const controlsUrl = `data:text/javascript;base64,${Buffer.from(controlsSource.replace(/(["'])\.\/primitives\/interaction-origin\.js\1/, JSON.stringify(originUrl))).toString('base64')}`;
const displaySource = await readFile(new URL('../../src/Flourish.Blazor/Flourish.Blazor.Framework/wwwroot/multi-select-box.js', import.meta.url), 'utf8');
const displayUrl = `data:text/javascript;base64,${Buffer.from(displaySource.replace("'./controls.js'", JSON.stringify(controlsUrl))).toString('base64')}`;
const source = (await readFile(new URL('../../src/Flourish.Blazor/Flourish.Blazor.Framework/wwwroot/data.js', import.meta.url), 'utf8'))
    .replace("'./multi-select-box.js'", JSON.stringify(displayUrl));
const { synchronize, detach } = await import(`data:text/javascript;base64,${Buffer.from(source).toString('base64')}`);

function table(text = 'Alpha', withActions = true, lastDataColumn = true, bulk = false) {
    const col = { dataset: { fColumn: 'name', ...(lastDataColumn ? { fLastColumn: 'true' } : {}) }, style: {} };
    const button = {
        dataset: { fResize: 'name' }, attributes: new Map([['aria-valuenow', '72']]), capture: null,
        getAttribute(name) { return this.attributes.get(name); },
        setAttribute(name, value) { this.attributes.set(name, value); },
        closest(selector) { return selector === '[data-f-resize]' ? this : null; },
        getBoundingClientRect() { return { width: 8 }; }, focus() {}, setPointerCapture(id) { this.capture = id; },
        hasPointerCapture(id) { return this.capture === id; }, releasePointerCapture() { this.capture = null; }
    };
    const cell = (tagName, textContent) => ({ tagName, dataset: { fColumn: 'name' }, querySelector: selector => selector === '[data-f-resize]' ? button : ({ textContent }) });
    const cells = [cell('TH', 'Name'), cell('TD', text)];
    if (bulk) cells.push({ ...cell('TD', 'Selecionar Ativo (publicar / reativar) Arquivado Aplicar'.repeat(20)),
        closest: selector => selector === '.f-data-bulk-row' ? {} : null });
    return {
        col, button, style: {}, attributes: new Map(),
        querySelectorAll(selector) {
            if (selector === 'col[data-f-column]') return [col];
            if (selector === 'th[data-f-column],td[data-f-column]') return cells;
            if (selector === '[data-f-resize]') return [button];
            return [];
        },
        querySelector(selector) {
            if (selector === 'th.f-data-actions' && withActions) return { getBoundingClientRect: () => ({ width: 80 }) };
            if (selector === 'th.f-data-selection' && bulk) return { getBoundingClientRect: () => ({ width: 64 }) };
            return null;
        },
        setAttribute(name, value) { this.attributes.set(name, value); }
    };
}
function root(content = table()) {
    const handlers = new Map(); const classes = new Set();
    return {
        isConnected: true, table: content,
        classList: { add: key => classes.add(key), remove: key => classes.delete(key) },
        querySelector(selector) { return selector === '[data-f-table]' ? this.table : null; },
        querySelectorAll() { return []; },
        addEventListener(name, handler) { handlers.set(name, handler); },
        removeEventListener(name, handler) { if (handlers.get(name) === handler) handlers.delete(name); },
        dispatch(name, event) { handlers.get(name)?.(event); },
        handlerCount: () => handlers.size
    };
}
function key(root, name, shiftKey = false) {
    let prevented = false;
    root.dispatch('keydown', { target: root.table.button, key: name, shiftKey, preventDefault: () => prevented = true, stopPropagation() {} });
    return prevented;
}
function pointer(root, name, clientX, pointerId = 1) {
    root.dispatch(name, { target: root.table.button, button: 0, clientX, pointerId, preventDefault() {}, stopPropagation() {} });
}

test('natural column widths cap ordinary columns while the last data column ignores spacer and actions', () => {
    const short = root(); synchronize(short, 'short');
    assert.equal(short.table.col.style.width, '80px');
    assert.equal(short.table.style.minWidth, '160px');
    const capped = root(table('A'.repeat(200), false, false)); synchronize(capped, 'capped');
    assert.equal(capped.table.col.style.width, '320px');
    const last = root(table('A'.repeat(200), true)); synchronize(last, 'last');
    assert.equal(last.table.col.style.width, '1640px');
    assert.equal(last.table.style.minWidth, '1720px');
    detach('short'); detach('capped'); detach('last');
});
test('bulk editor option text never expands the last record column and selection width is reserved', () => {
    const view = root(table('Ativo')); synchronize(view, 'bulk-width');
    assert.equal(view.table.col.style.width, '80px');
    view.table = table('Ativo', false, true, true); synchronize(view, 'bulk-width');
    assert.equal(view.table.col.style.width, '80px');
    assert.equal(view.table.style.minWidth, '144px');
    key(view, 'ArrowRight', true);
    assert.equal(view.table.col.style.width, '130px');
    view.table = table('Ativo', false, true, true); synchronize(view, 'bulk-width');
    assert.equal(view.table.col.style.width, '130px');
    key(view, 'Home'); assert.equal(view.table.col.style.width, '80px');
    view.table = table('Ativo'); synchronize(view, 'bulk-width');
    assert.equal(view.table.col.style.width, '80px');
    detach('bulk-width');
});
test('bulk editor text cannot affect capped ordinary columns either', () => {
    const view = root(table('Alpha', true, false, true)); synchronize(view, 'bulk-ordinary');
    assert.equal(view.table.col.style.width, '80px');
    assert.equal(view.table.style.minWidth, '224px');
    detach('bulk-ordinary');
});
test('keyboard resize supports 10/50px steps, minimum bounds and Home natural reset', () => {
    const view = root(); synchronize(view, 'keys');
    assert.equal(key(view, 'ArrowRight'), true);
    assert.equal(view.table.col.style.width, '90px');
    key(view, 'ArrowRight', true); assert.equal(view.table.col.style.width, '140px');
    key(view, 'ArrowLeft', true); key(view, 'ArrowLeft', true); assert.equal(view.table.col.style.width, '72px');
    key(view, 'Home'); assert.equal(view.table.col.style.width, '80px');
    assert.equal(view.table.button.getAttribute('aria-valuetext'), '80 px');
    detach('keys');
});
test('manual widths survive cards/table replacement and remain isolated by component', () => {
    const first = root(); synchronize(first, 'first'); key(first, 'ArrowRight', true);
    first.table = null; synchronize(first, 'first');
    first.table = table('A'.repeat(200)); synchronize(first, 'first');
    assert.equal(first.table.col.style.width, '130px');
    const second = root(); synchronize(second, 'second'); assert.equal(second.table.col.style.width, '80px');
    detach('first'); detach('second');
});
test('Escape and pointer cancel restore the previous manual width', () => {
    const view = root(); synchronize(view, 'cancel'); key(view, 'ArrowRight');
    pointer(view, 'pointerdown', 100); pointer(view, 'pointermove', 200); assert.equal(view.table.col.style.width, '190px');
    key(view, 'Escape'); assert.equal(view.table.col.style.width, '90px');
    pointer(view, 'pointerdown', 100); pointer(view, 'pointermove', 300); pointer(view, 'pointercancel', 300);
    assert.equal(view.table.col.style.width, '90px'); detach('cancel');
});
test('extreme pointer values are bounded without poisoning CSS or accessible values', () => {
    const view = root(); synchronize(view, 'bounds');
    pointer(view, 'pointerdown', 0); pointer(view, 'pointermove', 1e9);
    assert.equal(view.table.col.style.width, '100000px'); assert.equal(view.table.button.getAttribute('aria-valuenow'), '100000');
    pointer(view, 'pointerup', 1e9); detach('bounds');
});
test('page Enter prevents parent form submission and uses the same change callback', () => {
    const view = root(); synchronize(view, 'page'); let prevented = false; let changes = 0;
    const page = { closest: selector => selector === '[data-f-page]' ? page : null, dispatchEvent: event => { assert.equal(event.type, 'change'); changes++; } };
    view.dispatch('keydown', { target: page, key: 'Enter', preventDefault: () => prevented = true });
    assert.equal(prevented, true); assert.equal(changes, 1);
    detach('page'); assert.equal(view.handlerCount(), 0);
    assert.ok([...documentListeners.values()].every(listeners => listeners.size === 0));
});
