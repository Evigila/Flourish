import fs from 'node:fs/promises';
import assert from 'node:assert/strict';

let checks = 0;
const check = async (name, action) => { await action(); checks++; process.stdout.write(`PASS ${name}\n`); };
class Node {
    attrs = new Map(); events = new Map(); children = []; parent = null; style = {};
    disabled = false; isConnected = true; open = false;
    constructor(tag = 'button') { this.tag = tag; }
    setAttribute(name, value) { this.attrs.set(name, String(value)); }
    getAttribute(name) { return this.attrs.get(name) ?? null; }
    matches(selector) { return selector === ':popover-open' ? this.open : selector === ':disabled' && this.disabled; }
    contains(node) { return node === this || this.children.some(child => child.contains(node)); }
    closest(selector) {
        for (let node = this; node; node = node.parent) {
            if (selector === '[disabled], [aria-disabled="true"]') {
                if (node.disabled || node.attrs.has('disabled') || node.getAttribute('aria-disabled') === 'true') return node;
            } else if (['a', 'button'].includes(node.tag) || node.getAttribute('role') === 'menuitem') return node;
        }
        return null;
    }
    querySelectorAll(selector) {
        return this.children.filter(node => selector === 'a[href], button'
            ? node.tag === 'button' || node.tag === 'a' && node.attrs.has('href')
            : node.tag === 'button' || node.tag === 'a' && node.attrs.has('href')
                || node.getAttribute('role') === 'menuitem' && node.attrs.has('tabindex'));
    }
    addEventListener(type, handler, capture = false) {
        const events = this.events.get(type) ?? []; events.push({ handler, capture }); this.events.set(type, events);
    }
    emit(type, event) { for (const { handler } of this.events.get(type) ?? []) { handler(event); if (event.stopped) break; } }
    getBoundingClientRect() { return { left: 20, right: 80, top: 100, bottom: 136, width: 160, height: 180 }; }
    focus() { document.activeElement = this; }
    showPopover() { this.open = true; }
    hidePopover() { this.open = false; }
}
globalThis.document = new Node('document'); document.body = new Node('body'); document.documentElement = new Node('html');
globalThis.window = new Node('window'); window.innerWidth = 800; window.innerHeight = 600;
const sourcePath = new URL('../../src/Flourish.Blazor/Flourish.Blazor.Framework/wwwroot/primitives/', import.meta.url);
const dependency = await fs.readFile(new URL('interaction-origin.js', sourcePath), 'utf8');
const dependencyUrl = 'data:text/javascript;base64,' + Buffer.from(dependency).toString('base64');
const source = await fs.readFile(new URL('row-action-menu.js', sourcePath), 'utf8');
const api = await import('data:text/javascript;base64,' + Buffer.from(source.replace('"./interaction-origin.js"', JSON.stringify(dependencyUrl))).toString('base64'));
const trigger = new Node(), menu = new Node('div');
const first = new Node(), nativeDisabled = new Node(), ariaDisabled = new Node('a'), customDisabled = new Node('span'), last = new Node();
nativeDisabled.disabled = true; nativeDisabled.setAttribute('tabindex', '0');
ariaDisabled.setAttribute('href', '/should-not-navigate'); ariaDisabled.setAttribute('aria-disabled', 'true');
customDisabled.setAttribute('role', 'menuitem'); customDisabled.setAttribute('tabindex', '0'); customDisabled.setAttribute('aria-disabled', 'true');
menu.children = [first, nativeDisabled, ariaDisabled, customDisabled, last]; menu.children.forEach(node => node.parent = menu);
const event = (target, key) => ({ target, key, prevented: false, stopped: false,
    preventDefault() { this.prevented = true; }, stopImmediatePropagation() { this.stopped = true; } });
await check('Native and aria-disabled triggers cannot open', () => {
    trigger.disabled = true; api.toggle(trigger, menu); assert.equal(menu.open, false);
    trigger.disabled = false; trigger.setAttribute('aria-disabled', 'true'); api.toggle(trigger, menu); assert.equal(menu.open, false);
    trigger.attrs.delete('aria-disabled');
});
await check('Enabled trigger opens and disabled candidates are skipped by keyboard', () => {
    api.toggle(trigger, menu); assert.equal(menu.open, true); first.focus();
    document.emit('keydown', event(first, 'ArrowDown')); assert.equal(document.activeElement, last);
    document.emit('keydown', event(last, 'ArrowUp')); assert.equal(document.activeElement, first);
    document.emit('keydown', event(first, 'End')); assert.equal(document.activeElement, last);
    document.emit('keydown', event(last, 'Home')); assert.equal(document.activeElement, first);
});
await check('Disabled click capture cancels callback/navigation without closing menu', async () => {
    assert.equal(document.events.get('click')[0].capture, true);
    for (const disabled of [nativeDisabled, ariaDisabled, customDisabled]) {
        const click = event(disabled); let actions = 0;
        document.emit('click', click); if (!click.stopped) actions++;
        await Promise.resolve();
        assert.equal(click.prevented, true); assert.equal(actions, 0); assert.equal(menu.open, true);
    }
});
await check('Nested icon inside disabled action is also blocked', async () => {
    const icon = new Node('svg'); icon.parent = ariaDisabled; ariaDisabled.children.push(icon);
    const click = event(icon); document.emit('click', click); await Promise.resolve();
    assert.equal(click.prevented, true); assert.equal(click.stopped, true); assert.equal(menu.open, true);
});
await check('Enabled action callback remains available and queued close completes', async () => {
    const click = event(first); let actions = 0;
    document.emit('click', click); if (!click.stopped) actions++;
    assert.equal(actions, 1); assert.equal(click.prevented, false); assert.equal(menu.open, true);
    await Promise.resolve(); assert.equal(menu.open, false); assert.equal(trigger.getAttribute('aria-expanded'), 'false');
});
await check('Escape still closes and restores trigger focus', () => {
    api.toggle(trigger, menu); const escape = event(last, 'Escape'); document.emit('keydown', escape);
    assert.equal(escape.prevented, true); assert.equal(menu.open, false); assert.equal(document.activeElement, trigger);
});
await check('Outside press closes without stealing focus and dispose closes safely', () => {
    api.toggle(trigger, menu); const outside = new Node(); outside.focus();
    document.emit('pointerdown', event(outside)); assert.equal(menu.open, false); assert.equal(document.activeElement, outside);
    api.toggle(trigger, menu); api.dispose(trigger, menu); assert.equal(menu.open, false);
});
await check('Disabled items receive no hover/pressed/focus preview and have neutral paint', async () => {
    const css = await fs.readFile(new URL('../../src/Flourish.Blazor/Flourish.Blazor.Design/wwwroot/primitives/RowActionMenu.css', import.meta.url), 'utf8');
    for (const block of css.matchAll(/([^{}]+)\{([^{}]*)\}/g)) {
        if (!/background:\s*var\(--f-target-(?:preview|click)/.test(block[2])) continue;
        for (const selector of block[1].trim().split(',')) {
            assert.ok(selector.includes(':not([aria-disabled="true"])'), selector);
            if (!selector.includes(' a:')) assert.ok(selector.includes(':not(:disabled)'), selector);
        }
    }
    assert.match(css, /:is\(:disabled, \[aria-disabled="true"\]\)[\s\S]*?color:\s*var\(--f-muted\);[\s\S]*?background:\s*transparent;[\s\S]*?cursor:\s*not-allowed;/);
});
process.stdout.write(`${checks}/${checks} RowActionMenu mocked DOM checks passed.\n`);
