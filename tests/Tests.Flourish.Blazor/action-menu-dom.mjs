import fs from 'node:fs/promises';
import assert from 'node:assert/strict';

let checks = 0;
const check = async (name, action) => { await action(); checks++; process.stdout.write(`PASS ${name}\n`); };
class Style {
    values = new Map();
    getPropertyValue(name) { return this.values.get(name) ?? ''; }
    getPropertyPriority() { return ''; }
    setProperty(name, value) { this.values.set(name, value); }
    removeProperty(name) { this.values.delete(name); }
}
class Node {
    attrs = new Map(); events = new Map(); children = []; parent = null; style = new Style();
    disabled = false; hidden = false; isConnected = true; open = false; popoverOpen = false;
    constructor(tag = 'button') { this.tag = tag; }
    append(...nodes) { this.children.push(...nodes); nodes.forEach(node => node.parent = this); }
    setAttribute(name, value) { this.attrs.set(name, String(value)); }
    getAttribute(name) { return this.attrs.get(name) ?? null; }
    removeAttribute(name) { this.attrs.delete(name); }
    matches(selector) {
        if (selector === ':popover-open') return this.popoverOpen;
        if (selector === ':disabled') return this.disabled || this.attrs.has('disabled');
        if (selector === '.f-action-menu') return this.getAttribute('class')?.split(' ').includes('f-action-menu') ?? false;
        return selector.split(',').some(part => {
            part = part.trim();
            if (part === 'a') return this.tag === 'a';
            if (part === 'button') return this.tag === 'button';
            if (part === 'a[href]') return this.tag === 'a' && this.attrs.has('href');
            if (part === '[hidden]') return this.hidden || this.attrs.has('hidden');
            if (part === '[inert]') return this.attrs.has('inert');
            if (part === '[disabled]') return this.disabled || this.attrs.has('disabled');
            if (part === '[aria-disabled="true"]') return this.getAttribute('aria-disabled') === 'true';
            if (part === '[role="menu"]') return this.getAttribute('role') === 'menu';
            if (part === '[role="menuitem"]') return this.getAttribute('role') === 'menuitem';
            if (part === '[role="menuitem"]:not([disabled])') return this.getAttribute('role') === 'menuitem' && !this.disabled && !this.attrs.has('disabled');
            if (part === '[tabindex]:not([tabindex="-1"])') return this.attrs.has('tabindex') && this.getAttribute('tabindex') !== '-1';
            if (part === '[autofocus]') return this.attrs.has('autofocus');
            return ['button','input','select','textarea'].some(tag => part === `${tag}:not([disabled])` && this.tag === tag && !this.disabled && !this.attrs.has('disabled'));
        });
    }
    contains(node) { return node === this || this.children.some(child => child.contains(node)); }
    closest(selector) { for (let node = this; node; node = node.parent) if (node.matches(selector)) return node; return null; }
    querySelectorAll(selector) { return this.children.flatMap(node => [...(node.matches(selector) ? [node] : []), ...node.querySelectorAll(selector)]); }
    querySelector(selector) { return this.querySelectorAll(selector)[0] ?? null; }
    addEventListener(type, handler, options = false) {
        const events = this.events.get(type) ?? []; events.push({ handler, capture: options === true || options?.capture === true }); this.events.set(type, events);
    }
    removeEventListener(type, handler) { this.events.set(type, (this.events.get(type) ?? []).filter(listener => listener.handler !== handler)); }
    emit(type, event) { for (const { handler } of [...(this.events.get(type) ?? [])]) { handler(event); if (event.immediateStopped) break; } }
    getClientRects() { return this.hidden ? [] : [this.getBoundingClientRect()]; }
    getBoundingClientRect() { return { left: 20, right: 80, top: 100, bottom: 136, width: this.tag === 'div' ? 160 : 60, height: this.tag === 'div' ? 180 : 36 }; }
    focus() { document.activeElement = this; }
    showPopover() { this.popoverOpen = true; }
    hidePopover() { this.popoverOpen = false; }
    showModal() { this.open = true; }
    close() { this.open = false; }
    scrollTo() {}
}
globalThis.document = new Node('document'); document.body = new Node('body'); document.documentElement = new Node('html'); document.documentElement.clientWidth = 800;
globalThis.window = new Node('window'); window.innerHeight = 600;
const observers = [];
globalThis.MutationObserver = class { constructor(callback) { this.callback = callback; observers.push(this); } observe() {} disconnect() { this.disconnected = true; } };
globalThis.getComputedStyle = node => ({ length: 0, maxHeight: node.style.getPropertyValue('max-height') || 'none', getPropertyValue: name => node.style.getPropertyValue(name) });
globalThis.CSS = { escape: value => value };
const sourcePath = new URL('../../src/Flourish.Blazor/Flourish.Blazor.Framework/wwwroot/', import.meta.url);
const dependency = await fs.readFile(new URL('primitives/interaction-origin.js', sourcePath), 'utf8');
const dependencyUrl = 'data:text/javascript;base64,' + Buffer.from(dependency).toString('base64');
const source = await fs.readFile(new URL('controls.js', sourcePath), 'utf8');
const api = await import('data:text/javascript;base64,' + Buffer.from(source.replace(/(["'])\.\/primitives\/interaction-origin\.js\1/, JSON.stringify(dependencyUrl))).toString('base64'));
const event = (target, key) => ({ target, key, defaultPrevented: false, propagationStopped: false, immediateStopped: false,
    preventDefault() { this.defaultPrevented = true; }, stopPropagation() { this.propagationStopped = true; },
    stopImmediatePropagation() { this.immediateStopped = true; this.propagationStopped = true; } });
function dispatchClick(target) {
    const click = event(target); const path = [];
    for (let node = target; node; node = node.parent) path.push(node);
    const invoke = (node, capture) => {
        for (const listener of [...(node.events.get('click') ?? [])]) {
            if (listener.capture !== capture) continue;
            listener.handler(click); if (click.immediateStopped) break;
        }
    };
    for (const node of [...path].reverse()) { invoke(node, true); if (click.propagationStopped) return click; }
    for (const node of path) { invoke(node, false); if (click.propagationStopped) break; }
    return click;
}
const trigger = new Node(), menu = new Node('div'); menu.setAttribute('role', 'menu');
const first = new Node(), nativeDisabled = new Node(), ariaDisabled = new Node('a'), customDisabled = new Node('span'), hidden = new Node(), inert = new Node(), last = new Node();
for (const item of [first,nativeDisabled,ariaDisabled,customDisabled,hidden,inert,last]) item.setAttribute('role', 'menuitem');
nativeDisabled.disabled = true; ariaDisabled.setAttribute('href', '/should-not-navigate'); ariaDisabled.setAttribute('aria-disabled', 'true');
customDisabled.setAttribute('tabindex', '0'); customDisabled.setAttribute('aria-disabled', 'true'); hidden.hidden = true; inert.setAttribute('inert', '');
menu.append(first,nativeDisabled,ariaDisabled,customDisabled,hidden,inert,last);
api.attachMenu(trigger, menu);
await check('Native and aria-disabled triggers cannot open the actual menu', () => {
    trigger.disabled = true; api.toggleMenu(trigger, menu); assert.equal(menu.popoverOpen, false);
    trigger.disabled = false; trigger.setAttribute('aria-disabled', 'true'); api.toggleMenu(trigger, menu); assert.equal(menu.popoverOpen, false);
    trigger.removeAttribute('aria-disabled');
});
await check('Keyboard navigation skips disabled links, hidden and inert items', () => {
    api.toggleMenu(trigger, menu); assert.equal(menu.popoverOpen, true); assert.equal(document.activeElement, first);
    document.emit('keydown', event(first, 'ArrowDown')); assert.equal(document.activeElement, last);
    document.emit('keydown', event(last, 'ArrowUp')); assert.equal(document.activeElement, first);
    document.emit('keydown', event(first, 'End')); assert.equal(document.activeElement, last);
    document.emit('keydown', event(last, 'Home')); assert.equal(document.activeElement, first);
});
await check('Disabled click capture prevents native callbacks and preserves the open menu', async () => {
    assert.equal(menu.events.get('click')[0].capture, true);
    for (const disabled of [nativeDisabled,ariaDisabled,customDisabled,hidden,inert]) {
        let actions = 0; const action = () => actions++; disabled.addEventListener('click', action);
        const click = dispatchClick(disabled); await Promise.resolve();
        assert.equal(click.defaultPrevented, true); assert.equal(actions, 0); assert.equal(menu.popoverOpen, true);
        disabled.removeEventListener('click', action);
    }
});
await check('Nested icons within disabled native actions cannot activate them', async () => {
    const icon = new Node('svg'); ariaDisabled.append(icon);
    const click = dispatchClick(icon); await Promise.resolve();
    assert.equal(click.defaultPrevented, true); assert.equal(click.immediateStopped, true); assert.equal(menu.popoverOpen, true);
});
await check('Enabled native action dispatch precedes the actual queued menu dismissal', async () => {
    let actions = 0; first.addEventListener('click', () => { actions++; assert.equal(menu.popoverOpen, true); });
    const click = dispatchClick(first);
    assert.equal(actions, 1); assert.equal(click.defaultPrevented, false); assert.equal(menu.popoverOpen, true);
    await Promise.resolve(); assert.equal(menu.popoverOpen, false); assert.equal(trigger.getAttribute('aria-expanded'), 'false');
});
await check('Escape closes the actual menu and restores its trigger', () => {
    api.toggleMenu(trigger, menu); const escape = event(last, 'Escape'); document.emit('keydown', escape);
    assert.equal(escape.defaultPrevented, true); assert.equal(menu.popoverOpen, false); assert.equal(document.activeElement, trigger);
});
await check('Outside press and idempotent teardown release the real menu listeners', () => {
    api.toggleMenu(trigger, menu); const outside = new Node(); outside.focus();
    document.emit('pointerdown', event(outside)); assert.equal(menu.popoverOpen, false); assert.equal(document.activeElement, outside);
    api.toggleMenu(trigger, menu); api.detachMenu(trigger, menu); api.detachMenu(trigger, menu);
    assert.equal(menu.popoverOpen, false); assert.equal(trigger.events.get('keydown').length, 0); assert.equal(menu.events.get('click').length, 0);
    assert.equal(observers[0].disconnected, true);
});
await check('Current menu CSS excludes disabled links from hover, pressed and focus preview paint', async () => {
    for (const name of ['controls.css','dropdown.css']) {
        const css = await fs.readFile(new URL(`../../src/Flourish.Blazor/Flourish.Blazor.Design/wwwroot/${name}`, import.meta.url), 'utf8');
        let guarded = 0;
        for (const block of css.matchAll(/([^{}]+)\{([^{}]*)\}/g)) {
            if (!/background:\s*var\(--f-(?:target-(?:preview|click)|preview-danger|click-danger)/.test(block[2])) continue;
            for (const selector of block[1].trim().split(/,(?![^()]*\))/)) {
                if (!selector.includes('.f-menu-item') && !selector.includes('.f-dropdown-item') && !selector.includes('.f-menu-danger')) continue;
                assert.ok(selector.includes(':not([aria-disabled="true"])'), selector); assert.ok(selector.includes(':not(:disabled)'), selector); guarded++;
            }
        }
        assert.ok(guarded >= 3, name); assert.match(css, /\.f-(?:menu|dropdown)-item\[aria-disabled="true"\][^{]*\{[^}]*cursor:\s*not-allowed/);
    }
});
const disclosure = new Node('details'), summary = new Node('summary'), nativePanel = new Node('div'), nativeAction = new Node();
nativePanel.setAttribute('role', 'menu'); nativeAction.setAttribute('role', 'menuitem'); nativePanel.append(nativeAction); disclosure.append(summary,nativePanel);
await check('Native ChildContent command policy dismisses the same disclosure after dispatch', async () => {
    api.attachDisclosureMenu(disclosure,summary,nativePanel,true); disclosure.open = true; disclosure.emit('toggle', {});
    let actions = 0; nativeAction.addEventListener('click', () => { actions++; assert.equal(disclosure.open, true); });
    dispatchClick(nativeAction); assert.equal(actions, 1); assert.equal(nativePanel.popoverOpen, true);
    await Promise.resolve(); assert.equal(nativePanel.popoverOpen, false); assert.equal(disclosure.open, false);
});
await check('Disclosure refresh updates command policy without duplicating listener owners', async () => {
    api.attachDisclosureMenu(disclosure,summary,nativePanel); api.attachDisclosureMenu(disclosure,summary,nativePanel);
    assert.equal(disclosure.events.get('toggle').length, 1); assert.equal(nativePanel.events.get('click').length, 1);
    disclosure.open = true; disclosure.emit('toggle', {}); dispatchClick(nativeAction); await Promise.resolve();
    assert.equal(nativePanel.popoverOpen, true); assert.equal(disclosure.open, true);
    api.attachDisclosureMenu(disclosure,summary,nativePanel,true); dispatchClick(nativeAction); await Promise.resolve();
    assert.equal(disclosure.open, false); api.detachMenu(summary,nativePanel);
    assert.equal(disclosure.events.get('toggle').length, 0); assert.equal(nativePanel.events.get('click').length, 0);
});
await check('A real menu action preserves its invoker across an asynchronously opened dialog', async () => {
    api.attachMenu(trigger,menu); api.toggleMenu(trigger,menu); first.focus(); dispatchClick(first); await Promise.resolve();
    assert.equal(menu.popoverOpen,false); const dialog = new Node('dialog'); dialog.id = 'async-confirmation';
    api.synchronizeDialog(dialog,true,{ invokeMethodAsync: async () => {} }); assert.equal(dialog.open,true);
    api.synchronizeDialog(dialog,false,{}); assert.equal(document.activeElement,trigger); api.detachDialog(dialog); api.detachMenu(trigger,menu);
});
await check('A queued action close does not dismiss another independently opened menu', async () => {
    const nextTrigger = new Node(), nextPanel = new Node('div'), nextAction = new Node(); nextAction.setAttribute('role','menuitem'); nextPanel.append(nextAction);
    api.attachMenu(trigger,menu); api.attachMenu(nextTrigger,nextPanel); api.toggleMenu(trigger,menu); dispatchClick(first);
    api.toggleMenu(nextTrigger,nextPanel); await Promise.resolve();
    assert.equal(menu.popoverOpen,false); assert.equal(nextPanel.popoverOpen,true);
    api.detachMenu(trigger,menu); api.detachMenu(nextTrigger,nextPanel);
});
await check('Removing the current trigger releases its actual observer and popup ownership', () => {
    const removed = new Node(), panel = new Node('div'); api.attachMenu(removed,panel); api.toggleMenu(removed,panel);
    const observer = observers.at(-1); removed.isConnected = false; observer.callback();
    assert.equal(panel.popoverOpen,false); assert.equal(observer.disconnected,true); assert.equal(removed.events.get('keydown').length,0);
});
process.stdout.write(`${checks}/${checks} ActionMenu current-controller DOM checks passed.\n`);
