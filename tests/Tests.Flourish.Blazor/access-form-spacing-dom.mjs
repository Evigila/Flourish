import assert from 'node:assert/strict';
import { readFile } from 'node:fs/promises';

let checks = 0;
const check = (name, run) => { run(); checks++; process.stdout.write(`PASS ${name}\n`); };
const checkAsync = async (name, run) => { await run(); checks++; process.stdout.write(`PASS ${name}\n`); };
class Element {
    static DOCUMENT_POSITION_FOLLOWING = 4;
    children = []; parentElement = null; isConnected = true; hidden = false; rowGap = '7px'; height = 26.35;
    values = new Map(); attributes = new Map();
    style = { getPropertyValue: key => this.values.get(key) ?? '', setProperty: (key, value) => this.values.set(key, value), removeProperty: key => this.values.delete(key) };
    constructor(classes = []) { this.classes = new Set(classes); }
    append(...children) { for (const child of children) { this.children.push(child); child.parentElement = this; } return this; }
    matches(selector) { return selector.split(',').some(arm => this.classes.has(arm.trim().slice(1))); }
    closest(selector) {
        for (let node = this; node; node = node.parentElement) {
            if (selector === '[hidden],[inert]' ? node.hidden : node.matches(selector)) return node;
        }
        return null;
    }
    get previousElementSibling() { return this.parentElement?.children[this.parentElement.children.indexOf(this) - 1] ?? null; }
    querySelectorAll(selector) {
        if (selector.startsWith(':scope > ')) return this.children.filter(child => child.matches(selector.slice(9)));
        return this.children.flatMap(child => [...(child.matches(selector) ? [child] : []), ...child.querySelectorAll(selector)]);
    }
    querySelector(selector) { return this.querySelectorAll(selector)[0] ?? null; }
    getClientRects() { return this.closest('[hidden],[inert]') ? [] : [{ height: this.height }]; }
    getBoundingClientRect() { return { height: this.height }; }
    compareDocumentPosition(other) {
        let root = this; while (root.parentElement) root = root.parentElement;
        const flatten = node => [node, ...node.children.flatMap(flatten)];
        const nodes = flatten(root);
        return nodes.indexOf(this) < nodes.indexOf(other) ? Element.DOCUMENT_POSITION_FOLLOWING : 0;
    }
}
const resized = [], mutated = [];
globalThis.Node = Element;
globalThis.ResizeObserver = class {
    observed = []; disconnects = 0;
    constructor(callback) { this.callback = callback; resized.push(this); }
    observe(node) { this.observed.push(node); }
    disconnect() { this.observed = []; this.disconnects++; }
};
globalThis.MutationObserver = class {
    constructor(callback) { this.callback = callback; mutated.push(this); }
    observe(target, options) { this.target = target; this.options = options; }
    disconnect() { this.disconnected = true; }
};
globalThis.getComputedStyle = node => ({ rowGap: node.rowGap });
const moduleRoot = '../../src/Flourish.Blazor/Flourish.Blazor.Framework/wwwroot/';
const controlsCode = await readFile(new URL(moduleRoot + 'controls.js', import.meta.url), 'utf8');
const originCode = await readFile(new URL(moduleRoot + 'primitives/interaction-origin.js', import.meta.url), 'utf8');
const url = code => 'data:text/javascript;base64,' + Buffer.from(code).toString('base64');
const controlsUrl = url(controlsCode.replace(/(["'])\.\/primitives\/interaction-origin\.js\1/, JSON.stringify(url(originCode))));
const { attachAccessFormSpacing, detachAccessFormSpacing } = await import(controlsUrl);
const space = node => Number.parseFloat(node.style.getPropertyValue('--f-access-action-label-gap'));
function field(height = 26.35) {
    const label = new Element(['f-field-label']); label.height = height;
    const input = new Element(['f-field-control']); input.height = 48;
    return { node: new Element(['f-field']).append(label, input), label, input };
}
function nativeFixture() {
    const root = new Element(['f-access-form-surface']);
    const password = field();
    const username = field();
    const submit = new Element(['f-button']);
    const layout = new Element(['f-form-layout']).append(username.node, password.node, submit);
    const form = new Element().append(layout); form.attributes = new Map([['method', 'post'], ['action', '/auth/workspace'], ['data-enhance', 'false'], ['token', 'unchanged']]);
    const panel = new Element(['f-navigation-choice-panel']).append(form);
    const guest = field(52.7);
    const guestSubmit = new Element(['f-button']);
    const guestForm = new Element().append(new Element(['f-form-layout']).append(guest.node, guestSubmit));
    guestForm.attributes = new Map([['method', 'post'], ['action', '/portal/guest-login'], ['token', 'retained']]);
    const guestPanel = new Element(['f-navigation-choice-panel']).append(guestForm); guestPanel.hidden = true;
    root.append(panel, guestPanel);
    return { root, username, password, submit, guest, guestSubmit, guestPanel, form, guestForm };
}

check('Native workspace fields and submit reserve the visible password label row rather than only equal component gaps', () => {
    const f = nativeFixture(); const protocol = [...f.form.attributes];
    attachAccessFormSpacing(f.root);
    assert.equal(space(f.submit), 26.35 + 7);
    assert.equal(24 + space(f.submit), 24 + f.password.label.height + Number.parseFloat(f.password.node.rowGap));
    assert.deepEqual([...f.form.attributes], protocol);
    const observer = resized.at(-1); const disconnects = observer.disconnects;
    observer.callback();
    assert.equal(observer.disconnects, disconnects, 'Unchanged observations caused recurring first-delivery ResizeObserver loops');
    const count = resized.length; attachAccessFormSpacing(f.root); assert.equal(resized.length, count);
    detachAccessFormSpacing(f.root);
    assert.equal(f.submit.style.getPropertyValue('--f-access-action-label-gap'), '');
    assert.equal(observer.observed.length, 0);
});
check('Visible wrapped labels and changed Field row gaps update submit spacing while hidden guest native forms remain retained', () => {
    const f = nativeFixture(); const protocol = [...f.guestForm.attributes];
    attachAccessFormSpacing(f.root);
    const resize = resized.at(-1), mutation = mutated.at(-1);
    f.password.label.height = 79.05; f.password.node.rowGap = '9px'; resize.callback();
    assert.equal(space(f.submit), 79.05 + 9);
    assert.equal(space(f.guestSubmit), 0);
    f.guestPanel.hidden = false; mutation.callback();
    assert.equal(space(f.guestSubmit), 52.7 + 7);
    assert.deepEqual([...f.guestForm.attributes], protocol);
    assert.ok(mutation.options.attributeFilter.includes('hidden') && mutation.options.characterData);
    detachAccessFormSpacing(f.root); assert.ok(mutation.disconnected);
});
check('Inline production action rows and the existing AccessFormSurface Actions slot use the same measured label spacing', () => {
    const root = new Element(['f-access-form-surface']), first = field(), last = field(52.7);
    const inline = new Element(['f-inline-actions']);
    const layout = new Element(['f-form-layout']).append(first.node, last.node, inline);
    const actions = new Element(['f-access-form-actions']);
    root.append(layout, actions);
    attachAccessFormSpacing(root);
    assert.equal(space(inline), 59.7); assert.equal(space(actions), 59.7);
    layout.children.pop(); inline.parentElement = null; mutated.at(-1).callback();
    assert.equal(inline.style.getPropertyValue('--f-access-action-label-gap'), '', 'An unmounted action retained library geometry');
    detachAccessFormSpacing(root);
});
await checkAsync('The native input entry discovers SSR forms, survives document replacement and disposes the one shared controller', async () => {
    const f = nativeFixture();
    globalThis.document = { readyState: 'complete', roots: [f.root], documentElement: {}, addEventListener() {}, querySelectorAll() { return this.roots; } };
    const previousCount = resized.length;
    const nativeCode = await readFile(new URL(moduleRoot + 'primitives/input-behaviors.js', import.meta.url), 'utf8');
    await import(url(nativeCode.replace(/(["'])\.\.\/controls\.js\1/, JSON.stringify(controlsUrl))));
    assert.equal(resized.length, previousCount + 1);
    assert.equal(space(f.submit), 33.35);
    const scan = mutated.find(observer => observer.target === document.documentElement);
    const rootObserver = mutated.findLast(observer => observer.target === f.root);
    f.root.isConnected = false; document.roots = []; scan.callback();
    assert.ok(rootObserver.disconnected); assert.equal(f.submit.style.getPropertyValue('--f-access-action-label-gap'), '');
    const replacement = nativeFixture(); document.roots = [replacement.root]; scan.callback();
    assert.equal(space(replacement.submit), 33.35);
    replacement.root.isConnected = false; document.roots = []; scan.callback();
});
process.stdout.write(`${checks}/${checks} access form spacing DOM checks passed.\n`);
