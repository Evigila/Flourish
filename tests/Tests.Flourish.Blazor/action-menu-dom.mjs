import fs from 'node:fs/promises';
import assert from 'node:assert/strict';
import { execFileSync, spawn } from 'node:child_process';
import os from 'node:os';
import path from 'node:path';
import { pathToFileURL } from 'node:url';


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
    get tagName() { return this.tag.toUpperCase(); }
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
            if (part === '.f-action-menu') return this.getAttribute('class')?.split(' ').includes('f-action-menu') ?? false;
            if (part === 'summary') return this.tag === 'summary';
            const attribute = part.match(/^\[([\w-]+)(?:="([^"]*)")?\]$/);
            if (attribute && attribute[1].startsWith('data-f-')) return attribute[2] === undefined ? this.attrs.has(attribute[1]) : this.getAttribute(attribute[1]) === attribute[2];
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
    querySelectorAll(selector) {
        if (selector.includes(':scope >')) return this.children.filter(node => node.matches(selector.replaceAll(':scope >','')));
        return this.children.flatMap(node => [...(node.matches(selector) ? [node] : []), ...node.querySelectorAll(selector)]); }
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
globalThis.getComputedStyle = node => ({ length: 0, minWidth: node.style.getPropertyValue('min-width') || '0px', maxHeight: node.style.getPropertyValue('max-height') || 'none', getPropertyValue: name => node.style.getPropertyValue(name) });
globalThis.CSS = { escape: value => value };
const sourcePath = new URL('../../src/Flourish.Blazor/Flourish.Blazor.Framework/wwwroot/', import.meta.url);
const dependency = await fs.readFile(new URL('primitives/interaction-origin.js', sourcePath), 'utf8');
const dependencyUrl = 'data:text/javascript;base64,' + Buffer.from(dependency).toString('base64');
const source = await fs.readFile(new URL('controls.js', sourcePath), 'utf8');
const controlsUrl = 'data:text/javascript;base64,' + Buffer.from(source.replace(/(["'])\.\/primitives\/interaction-origin\.js\1/, JSON.stringify(dependencyUrl))).toString('base64');
const api = await import(controlsUrl);
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
disclosure.setAttribute('class', 'f-action-menu');
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
await check('Native hover opens the same disclosure and top layer without moving focus', () => {
    const outside = new Node(); outside.focus();
    api.attachDisclosureMenu(disclosure,summary,nativePanel,true,true);
    disclosure.emit('pointerenter', { pointerType: 'mouse' });
    assert.equal(disclosure.open, true); assert.equal(nativePanel.popoverOpen, true);
    assert.equal(summary.getAttribute('aria-expanded'), 'true'); assert.equal(document.activeElement, outside);
    assert.equal(nativePanel.style.top, '136px');
    // The browser subsequently emits a toggle for details.open; it must not close or focus the panel.
    disclosure.emit('toggle', {}); assert.equal(nativePanel.popoverOpen, true); assert.equal(document.activeElement, outside);
});
await check('Native hover stays open across both regions and pointer clicks do not toggle it shut', () => {
    disclosure.emit('pointerleave', { pointerType: 'mouse', relatedTarget: nativeAction });
    nativePanel.emit('pointerleave', { pointerType: 'mouse', relatedTarget: summary });
    const click = event(summary); click.detail = 1; summary.emit('click', click);
    assert.equal(click.defaultPrevented, true); assert.equal(disclosure.open, true); assert.equal(nativePanel.popoverOpen, true);
});
await check('Native hover closes on leave and restores only focus owned by the menu', () => {
    const outside = document.activeElement;
    nativePanel.emit('pointerleave', { pointerType: 'mouse', relatedTarget: null });
    assert.equal(disclosure.open, false); assert.equal(nativePanel.popoverOpen, false); assert.equal(document.activeElement, outside);
    disclosure.emit('pointerenter', { pointerType: 'mouse' }); nativeAction.focus();
    disclosure.emit('pointerleave', { pointerType: 'mouse', relatedTarget: new Node() });
    assert.equal(disclosure.open, false); assert.equal(document.activeElement, summary);
});
await check('Native touch ignores hover and keyboard activation focuses the enabled menu action', () => {
    disclosure.emit('pointerenter', { pointerType: 'touch' }); assert.equal(disclosure.open, false);
    const click = event(summary); click.detail = 0; summary.emit('click', click);
    assert.equal(click.defaultPrevented, true); assert.equal(disclosure.open, true); assert.equal(document.activeElement, nativeAction);
    document.emit('keydown', event(nativeAction, 'Escape')); assert.equal(disclosure.open, false); assert.equal(document.activeElement, summary);
    click.detail = 1; summary.emit('click', click); assert.equal(disclosure.open, true);
    api.closeMenu(summary,nativePanel,false);
});
await check('Native hover respects aria-disabled and inert availability', () => {
    summary.setAttribute('aria-disabled','true'); disclosure.emit('pointerenter', { pointerType: 'mouse' });
    assert.equal(disclosure.open, false); summary.removeAttribute('aria-disabled');
    disclosure.setAttribute('inert',''); disclosure.emit('pointerenter', { pointerType: 'mouse' });
    assert.equal(disclosure.open, false); disclosure.removeAttribute('inert');
});
await check('Native hover refresh switches back to disclosure clicks and teardown releases all listeners', () => {
    disclosure.emit('pointerenter', { pointerType: 'mouse' }); assert.equal(disclosure.open, true);
    api.attachDisclosureMenu(disclosure,summary,nativePanel,true,false);
    assert.equal(disclosure.open, false); assert.equal(nativePanel.popoverOpen, false);
    disclosure.emit('pointerenter', { pointerType: 'mouse' }); assert.equal(disclosure.open, false);
    const click = event(summary); click.detail = 1; summary.emit('click',click); assert.equal(click.defaultPrevented,false);
    disclosure.open = true; disclosure.emit('toggle',{}); assert.equal(nativePanel.popoverOpen,true);
    api.attachDisclosureMenu(disclosure,summary,nativePanel,true,false);
    assert.equal(disclosure.events.get('toggle').length,1); assert.equal(summary.events.get('click').length,1);
    api.detachMenu(summary,nativePanel); api.detachMenu(summary,nativePanel);
    for (const [node,type] of [[disclosure,'toggle'],[disclosure,'pointerenter'],[disclosure,'pointerleave'],[summary,'click'],[summary,'keydown'],[nativePanel,'pointerleave']])
        assert.equal(node.events.get(type).length,0,`${node.tag} ${type}`);
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

await check('Strong menus reject every implicit dismissal and preserve selection dispatch', async () => {
    const owner = new Node('details'), control = new Node('summary'), popup = new Node('div'), choice = new Node();
    owner.setAttribute('class','f-action-menu'); owner.setAttribute('data-f-popup-strength','strong');
    popup.setAttribute('role','menu'); choice.setAttribute('role','menuitem'); popup.append(choice); owner.append(control,popup);
    api.attachDisclosureMenu(owner,control,popup,true,true); owner.emit('pointerenter',{pointerType:'mouse'});
    const outside = new Node(); document.emit('pointerdown',event(outside)); document.emit('focusin',event(outside));
    document.emit('keydown',event(choice,'Escape')); owner.emit('pointerleave',{pointerType:'mouse',relatedTarget:outside});
    const click = event(control); click.detail = 1; control.emit('click',click);
    assert.equal(click.defaultPrevented,true); assert.equal(popup.popoverOpen,true);
    owner.open = false; owner.emit('toggle',{}); assert.equal(owner.open,true);
    const other = new Node(), otherPopup = new Node('div'); api.attachMenu(other,otherPopup); api.toggleMenu(other,otherPopup);
    assert.equal(popup.popoverOpen,true); api.detachMenu(other,otherPopup);
    let selected = 0; choice.addEventListener('click',()=>selected++); dispatchClick(choice); await Promise.resolve();
    assert.equal(selected,1); assert.equal(popup.popoverOpen,false); api.detachMenu(control,popup);
});
await check('Strong multiple selection remains open until the explicit completion control', async () => {
    const owner = new Node('details'), control = new Node('summary'), popup = new Node('div'), choice = new Node(), complete = new Node();
    owner.setAttribute('data-f-popup-strength','strong'); complete.setAttribute('data-f-popup-close','');
    popup.append(choice,complete); owner.append(control,popup);
    api.attachDisclosureMenu(owner,control,popup,false); owner.open = true; owner.emit('toggle',{});
    dispatchClick(choice); await Promise.resolve(); assert.equal(popup.popoverOpen,true);
    dispatchClick(complete); await Promise.resolve(); assert.equal(popup.popoverOpen,false); api.detachMenu(control,popup);
});
await check('The automatic SSR asset enhances a native menu below its real trigger and installs one outside-dismiss owner', async () => {
    const owner = new Node('details'), control = new Node('summary'), popup = new Node('div'), choice = new Node();
    owner.setAttribute('data-f-popup','menu'); owner.setAttribute('data-f-popup-dismiss-action','true');
    popup.setAttribute('data-f-popup-panel',''); choice.setAttribute('role','menuitem'); popup.append(choice); owner.append(control,popup);
    document.append(document.body); document.body.append(owner); document.readyState = 'complete';
    const entry = await fs.readFile(new URL('primitives/input-behaviors.js',sourcePath),'utf8');
    const progress = await fs.readFile(new URL('presentation/progress.js',sourcePath),'utf8');
    const progressUrl = 'data:text/javascript;base64,' + Buffer.from(progress).toString('base64');
    await import('data:text/javascript;base64,' + Buffer.from(entry.replace("'../controls.js'",JSON.stringify(controlsUrl)).replace("'../presentation/progress.js'",JSON.stringify(progressUrl))).toString('base64'));
    assert.equal(owner.getAttribute('data-f-enhanced'),'true'); assert.equal(popup.getAttribute('popover'),'manual');
    owner.open = true; owner.emit('toggle',{});
    assert.equal(popup.popoverOpen,true); assert.equal(popup.style.getPropertyValue('position'),'fixed');
    assert.equal(popup.style.top,'142px'); assert.equal(control.getAttribute('aria-expanded'),'true');
    api.initializePopups(document); assert.equal(owner.events.get('toggle').length,1);
    document.emit('pointerdown',event(new Node())); assert.equal(owner.open,false); assert.equal(popup.popoverOpen,false);
    api.detachMenu(control,popup); owner.isConnected = false;
});
await check('Circuit selection pointer dismissal honors strength and removes its actual subscription', async () => {
    const code = await fs.readFile(new URL('primitives/reference-dropdown.js',sourcePath),'utf8');
    const selection = await import('data:text/javascript;base64,' + Buffer.from(code).toString('base64'));
    const root = new Node('div'), outside = new Node(); let calls = 0;
    const reference = {invokeMethodAsync:async name => { assert.equal(name,'RequestDismissAsync'); calls++; }};
    selection.attachPopupDismissal(root,reference); selection.attachPopupDismissal(root,reference);
    root.setAttribute('data-f-popup-strength','strong'); document.emit('pointerdown',event(outside)); await Promise.resolve(); assert.equal(calls,0);
    root.setAttribute('data-f-popup-strength','weak'); document.emit('pointerdown',event(outside)); await Promise.resolve(); assert.equal(calls,1);
    document.emit('pointerdown',event(root)); await Promise.resolve(); assert.equal(calls,1);
    selection.detachPopupDismissal(root); document.emit('pointerdown',event(outside)); await Promise.resolve(); assert.equal(calls,1);
});

await check('Native focus-open notices and service menus preserve keyboard entry without reopening after Escape', async () => {
    const owner=new Node('div'), control=new Node(), popup=new Node('div'), close=new Node();
    owner.setAttribute('data-f-popup','menu');owner.setAttribute('data-f-popup-focus','true');
    popup.setAttribute('data-f-popup-panel','');close.setAttribute('data-f-popup-close','');
    owner.append(control,popup);popup.append(close);document.body.append(owner);api.initializePopups(document);
    control.emit('focus',{});assert.equal(popup.popoverOpen,true);
    document.emit('keydown',event(control,'Escape'));control.emit('focus',{});
    assert.equal(popup.popoverOpen,false);await Promise.resolve();
    owner.setAttribute('data-f-popup-strength','strong');api.initializePopups(document);control.emit('focus',{});
    assert.equal(popup.popoverOpen,true);dispatchClick(close);await Promise.resolve();assert.equal(popup.popoverOpen,false);
    api.detachMenu(control,popup);assert.equal(control.events.get('focus').length,0);
});
let browserChecksSkipped = 0;
const browserPaths = [process.env.FLOURISH_HEADLESS_BROWSER,
    'C:/Program Files (x86)/Microsoft/Edge/Application/msedge.exe',
    'C:/Program Files/Microsoft/Edge/Application/msedge.exe',
    'C:/Program Files/Google/Chrome/Application/chrome.exe',
    '/usr/bin/chromium', '/usr/bin/chromium-browser', '/usr/bin/google-chrome',
    '/Applications/Google Chrome.app/Contents/MacOS/Google Chrome'].filter(Boolean);
let browserPath;
for (const candidate of browserPaths) {
    try { if ((await fs.stat(candidate)).isFile()) { browserPath = candidate; break; } }
    catch (error) { if (error.code !== 'ENOENT') throw error; }
}
if (browserPath) {
    async function expandCss(url) {
        const css = await fs.readFile(url, 'utf8');
        const imports = [...css.matchAll(/@import\s+(?:url\(\s*)?["']([^"']+)["']\s*\)?\s*;/g)];
        let expanded = '', cursor = 0;
        for (const match of imports) {
            expanded += css.slice(cursor, match.index) + await expandCss(new URL(match[1], url));
            cursor = match.index + match[0].length;
        }
        return expanded + css.slice(cursor);
    }
    const frameworkCss = await expandCss(new URL('framework.css', sourcePath));
    const designCss = await expandCss(new URL('../../Flourish.Blazor.Design/wwwroot/design.css', sourcePath));
    const css = frameworkCss + designCss;
    const controller = dependency.replace(/^export /gm, '')
        + source.replace(/^import[^\r\n]+[\r\n]+/gm, '').replace(/^export /gm, '');
    const progressSource = await fs.readFile(new URL('presentation/progress.js', sourcePath), 'utf8');
    const progressController = `const progressApi=(()=>{${progressSource.replace(/^export /gm, '')};return {initializePresentationProgress};})();`;
    async function browserFixture(markup, measure, { width=1280, height=900, style='' } = {}) {
        const fixtureRoot = await fs.mkdtemp(path.join(os.tmpdir(), 'flourish-native-popup-'));
        try {
            const fixture = `<!doctype html><html><head><style>${css}${style}</style></head><body class="f-root f-theme-light">${markup}
                <pre id="measurements" hidden></pre><script>${controller}${progressController}
                const wait=()=>new Promise(resolve=>setTimeout(resolve,50));
                (async()=>{try {const measurements=await (async()=>{${measure}})();document.querySelector('#measurements').textContent=JSON.stringify(measurements);}
                catch(error) {document.querySelector('#measurements').textContent=JSON.stringify({error:String(error)});}})();</script></body></html>`;
            const fixturePath = path.join(fixtureRoot, 'fixture.html');
            await fs.writeFile(fixturePath, fixture);
            const dom = execFileSync(browserPath, ['--headless', '--disable-gpu', '--no-sandbox', '--no-first-run',
                '--no-default-browser-check', '--disable-extensions', '--virtual-time-budget=3000', '--dump-dom',
                `--window-size=${width},${height}`, '--user-data-dir=' + path.join(fixtureRoot, 'profile'), pathToFileURL(fixturePath).href],
                { encoding:'utf8', stdio:['ignore','pipe','ignore'], timeout:20000, maxBuffer:8 * 1024 * 1024, windowsHide:true });
            const match = dom.match(/<pre id="measurements"[^>]*>([\s\S]*?)<\/pre>/);
            assert.ok(match?.[1], 'The headless browser did not produce popup measurements.');
            const measurements = JSON.parse(match[1].replaceAll('&quot;', '"').replaceAll('&lt;', '<').replaceAll('&gt;', '>').replaceAll('&amp;', '&'));
            assert.equal(measurements.error, undefined, measurements.error);
            return measurements;
        } finally {
            assert.equal(path.dirname(path.resolve(fixtureRoot)), path.resolve(os.tmpdir()));
            assert.match(path.basename(fixtureRoot), /^flourish-native-popup-/);
            await fs.rm(fixtureRoot, { recursive:true, force:true, maxRetries:3, retryDelay:200 });
        }
    }
    // A trusted Runtime gesture opens the real native picker; the select continues to own all input.
    async function browserSessionFixture(markup, verify, { width=1280, height=900, styles=css, style='' } = {}) {
        const fixtureRoot = await fs.mkdtemp(path.join(os.tmpdir(), 'flourish-native-select-'));
        const profile = path.join(fixtureRoot, 'profile');
        let child, socket;
        const pending = new Map();
        try {
            const fixturePath = path.join(fixtureRoot, 'fixture.html');
            await fs.writeFile(fixturePath, `<!doctype html><html><head><style>${styles}${style}</style></head><body class="f-root f-theme-light">${markup}</body></html>`);
            let launchError;
            child = spawn(browserPath, ['--headless', '--disable-gpu', '--no-sandbox', '--no-first-run', '--no-default-browser-check',
                '--disable-extensions', '--remote-debugging-port=0', `--window-size=${width},${height}`, '--user-data-dir=' + profile, 'about:blank'],
                { stdio:'ignore', windowsHide:true });
            child.on('error', error => { launchError = error; });
            let endpoint;
            const deadline = Date.now() + 10000;
            while (!endpoint && Date.now() < deadline) {
                if (launchError) throw launchError;
                try {
                    const [port, route] = (await fs.readFile(path.join(profile, 'DevToolsActivePort'), 'utf8')).trim().split(/\r?\n/);
                    if (port && route) endpoint = `ws://127.0.0.1:${port}${route}`;
                } catch (error) { if (error.code !== 'ENOENT') throw error; }
                if (!endpoint) await new Promise(resolve => setTimeout(resolve, 50));
            }
            assert.ok(endpoint, 'The installed browser did not expose its isolated native-select test session.');
            socket = new WebSocket(endpoint);
            await new Promise((resolve, reject) => {
                const timer = setTimeout(() => reject(new Error('Native-select browser connection timed out.')), 5000);
                socket.addEventListener('open', () => { clearTimeout(timer); resolve(); }, { once:true });
                socket.addEventListener('error', () => { clearTimeout(timer); reject(new Error('Native-select browser connection failed.')); }, { once:true });
            });
            let nextId = 0;
            socket.addEventListener('message', event => {
                const message = JSON.parse(event.data);
                const request = pending.get(message.id);
                if (!request) return;
                pending.delete(message.id); clearTimeout(request.timer);
                if (message.error) request.reject(new Error(message.error.message));
                else request.resolve(message.result);
            });
            const command = (method, params={}, sessionId) => new Promise((resolve, reject) => {
                const id = ++nextId;
                const timer = setTimeout(() => { pending.delete(id); reject(new Error(`${method} timed out.`)); }, 5000);
                pending.set(id, { resolve, reject, timer });
                socket.send(JSON.stringify({ id, method, params, ...(sessionId ? {sessionId} : {}) }));
            });
            const { targetId } = await command('Target.createTarget', { url:'about:blank' });
            const { sessionId } = await command('Target.attachToTarget', { targetId, flatten:true });
            const send = (method, params={}) => command(method, params, sessionId);
            const evaluate = async (expression, userGesture=false) => {
                const result = await send('Runtime.evaluate', { expression, awaitPromise:true, returnByValue:true, userGesture });
                assert.equal(result.exceptionDetails, undefined, result.exceptionDetails?.text);
                return result.result.value;
            };
            await send('Page.navigate', {url:pathToFileURL(fixturePath).href});
            const readyDeadline = Date.now() + 5000;
            while (await evaluate(`document.readyState === 'complete' && location.href === ${JSON.stringify(pathToFileURL(fixturePath).href)}`) !== true) {
                assert.ok(Date.now() < readyDeadline, 'Native select fixture did not finish loading.');
                await new Promise(resolve => setTimeout(resolve, 25));
            }
            return await verify({ evaluate, send });
        } finally {
            if (socket?.readyState === WebSocket.OPEN) {
                socket.send(JSON.stringify({ id:2147483647, method:'Browser.close' }));
                socket.close();
            }
            for (const request of pending.values()) { clearTimeout(request.timer); request.reject(new Error('Native-select fixture was released.')); }
            if (child && child.exitCode === null) {
                await Promise.race([new Promise(resolve => child.once('exit', resolve)), new Promise(resolve => setTimeout(resolve, 1000))]);
                if (child.exitCode === null) child.kill();
            }
            assert.equal(path.dirname(path.resolve(fixtureRoot)), path.resolve(os.tmpdir()));
            assert.match(path.basename(fixtureRoot), /^flourish-native-select-/);
            await fs.rm(fixtureRoot, { recursive:true, force:true, maxRetries:5, retryDelay:200 });
        }
    }
    function withoutBaseSelect(styles) {
        const pattern = /@supports\s*\(\s*appearance\s*:\s*base-select\s*\)\s*\{/g;
        let output = '', cursor = 0, match;
        while ((match = pattern.exec(styles))) {
            output += styles.slice(cursor, match.index);
            let index = pattern.lastIndex, depth = 1;
            while (index < styles.length && depth) {
                if (styles[index] === '{') depth++;
                else if (styles[index] === '}') depth--;
                index++;
            }
            assert.equal(depth, 0, 'The actual base-select support block is unbalanced.');
            cursor = index; pattern.lastIndex = index;
        }
        return output + styles.slice(cursor);
    }
    const language = `<span><details class="f-action-menu f-dropdown-surface" data-f-native-menu data-f-popup="menu" data-f-popup-dismiss-action="true" data-f-popup-strength="weak">
        <summary class="f-disclosure-button-trigger f-button f-button-quiet f-button-icon" aria-label="Language" aria-controls="popup" aria-haspopup="menu"><span aria-hidden="true">Lang</span></summary>
        <div id="popup" class="f-menu-panel f-dropdown-panel" role="menu" data-f-popup-panel><button type="button" class="f-button f-button-quiet" role="menuitem">Português</button><button type="button" class="f-button f-button-quiet" role="menuitem">English</button><button type="button" class="f-button f-button-quiet" role="menuitem">中文</button></div></details></span>`;
    const nativeLanguage = `<span data-culture-menu><select class="f-input f-select f-select-customized" aria-label="Language" data-culture-picker="true" data-current-culture="pt-BR">
        <button type="button" class="f-select-trigger f-button f-button-quiet f-button-icon"><span class="f-icon" aria-hidden="true" data-icon="language">&#xEA07;</span></button>
        <option value="pt-BR" selected>Português (Brasil)</option><option value="en-US">English (United States)</option><option value="zh-CN">中文（中国）</option></select><span data-culture-save-error hidden role="alert">Unable to save the language.</span></span>`;
    const preferenceActions = `<div class="f-inline-actions" data-alignment="end">${nativeLanguage}
        <form method="post" action="/preferences/theme" data-enhance="false"><input type="hidden" name="theme" value="dark"><button type="submit" class="f-button f-button-quiet f-button-icon" aria-label="Dark theme"><span aria-hidden="true">Theme</span></button></form></div>`;
    const preferences = `<div class="f-page-body f-page-full">${preferenceActions}</div>`;
    function loginBody(fieldCount=2) {
        const fields = Array.from({length:fieldCount}, (_, index) => `<div class="f-field"><label id="field-${index}-label" class="f-field-label" for="field-${index}">${index === 0 ? 'Email' : index === 1 ? 'Senha' : 'Field ' + (index + 1)}<span class="f-required" aria-label="Required">*</span></label><div class="f-field-control"><input id="field-${index}" class="f-input" name="${index === 0 ? 'email' : index === 1 ? 'password' : 'field-' + index}" type="${index === 0 ? 'email' : index === 1 ? 'password' : 'text'}" required autocomplete="${index === 0 ? 'email' : index === 1 ? 'current-password' : 'off'}"></div></div>`).join('');
        return `<article class="f-access-panel" aria-label="Entrar"><header class="f-logo-displayer"><img class="f-logo-displayer-logo" width="96" height="96" alt="Colligere" src="data:image/svg+xml,%3Csvg xmlns='http://www.w3.org/2000/svg'/%3E"><h1 id="login-title" class="f-logo-displayer-title"><span>Colligere</span><span class="f-logo-displayer-context">Account</span></h1></header>
            <div class="f-form-layout" style="--f-form-columns:1;"><form method="post" action="/auth/login" data-enhance="false"><input type="hidden" name="__RequestVerificationToken" value="fixture"><input type="hidden" name="returnUrl" value="/"><div class="f-access-form-surface"><div class="f-form-layout" style="--f-form-columns:1;">${fields}</div><div class="f-access-form-actions"><div class="f-split-button f-split-button-full-width"><button id="final-submit" type="submit" class="f-split-button-primary" aria-busy="false"><span class="f-split-button-label">Entrar</span></button><details class="f-dropdown-surface f-split-button-menu" data-f-popup="menu" data-f-popup-dismiss-action="true" data-f-popup-strength="weak"><summary class="f-menu-trigger f-split-button-secondary" aria-label="Ações de acesso" aria-controls="login-actions"><span class="f-expansion-indicator" data-expanded="false" aria-hidden="true"></span></summary><div class="f-menu-panel f-dropdown-panel" data-f-popup-panel><div id="login-actions" class="f-split-button-menu-content"><a class="f-button f-button-quiet" href="/forgot-password/">Esqueci minha senha</a><a class="f-button f-button-quiet" href="/signup/">Criar conta</a></div></div></details></div></div></div></form></div></article>`;
    }
    function band(content, id='band') {
        return `<section id="${id}" class="f-presentation-band f-presentation-surface f-presentation-fullheight" data-presentation-band="true" style="--f-band-min-height:0px;" aria-label="${id}"><div class="f-content-container">${content}</div></section>`;
    }
    function fixedSurface(header=preferences, fields=2, content=loginBody(fields)) {
        return `<div class="content-surface" id="surface"><a class="skip-link" href="#content">Skip</a>${header === null ? '' : `<div class="f-content-surface-header">${header}</div>`}<main class="content-stage" id="content" tabindex="-1">${band(content)}</main></div>`;
    }
    function floatingSurface(actions=preferenceActions, fields=2, content=loginBody(fields)) {
        return `<div class="content-surface ${actions === null ? '' : 'f-content-surface-has-floating-actions'}" id="surface"><a class="skip-link" href="#content">Skip</a>${actions === null ? '' : `<div class="f-content-surface-floating-actions">${actions}</div>`}<main class="content-stage" id="content" tabindex="-1">${band(content)}</main></div>`;
    }
    const surfaceMeasurement = `
        const root=document.querySelector('#surface'), main=document.querySelector('#content'), band=document.querySelector('#band'), header=root.querySelector('.f-content-surface-header'), floating=root.querySelector('.f-content-surface-floating-actions'), submit=document.querySelector('#final-submit');
        const measure=()=>({viewport:window.innerHeight,documentHeight:document.scrollingElement.scrollHeight,documentScroll:document.scrollingElement.scrollTop,rootHeight:root.getBoundingClientRect().height,rootScroll:root.scrollTop,headerHeight:header?.getBoundingClientRect().height??0,floatingHeight:floating?.getBoundingClientRect().height??0,floatingTop:floating?.getBoundingClientRect().top??null,floatingRight:floating?root.getBoundingClientRect().right-floating.getBoundingClientRect().right:null,floatingPosition:floating?getComputedStyle(floating).position:null,stageTop:main.getBoundingClientRect().top,stageBottom:main.getBoundingClientRect().bottom,stageHeight:main.clientHeight,stageScrollHeight:main.scrollHeight,stageScroll:main.scrollTop,bandTop:band.getBoundingClientRect().top,bandHeight:band.getBoundingClientRect().height,bandPadding:getComputedStyle(band).paddingTop,logoHeight:root.querySelector(".f-logo-displayer-logo")?.getBoundingClientRect().height??0,titleSize:root.querySelector(".f-logo-displayer-title")?getComputedStyle(root.querySelector(".f-logo-displayer-title")).fontSize:null,submitTop:submit.getBoundingClientRect().top,submitBottom:submit.getBoundingClientRect().bottom});
        await wait();`;
    function fixedViewport(measurements) {
        assert.equal(measurements.rootHeight, measurements.viewport);
        assert.equal(measurements.documentHeight, measurements.viewport);
        assert.equal(measurements.documentScroll, 0);
        assert.equal(measurements.rootScroll, 0);
        assert.equal(measurements.stageHeight, measurements.viewport - measurements.headerHeight);
    }
    function floatingViewport(measurements) {
        fixedViewport(measurements);
        assert.equal(measurements.headerHeight, 0);
        assert.equal(measurements.stageTop, 0);
        assert.equal(measurements.floatingHeight, 48);
        assert.equal(measurements.floatingPosition, 'absolute');
        assert.equal(measurements.floatingTop, 16);
        assert.equal(measurements.floatingRight, 16);
    }
    await check('Real browser keeps SSR button-menu geometry before and after enhancement without changing row-menu fallback', async () => {
        const measurements = await browserFixture(`<div id="header">${language}</div><div id="row"><details data-f-native-menu class="f-action-menu"><summary class="f-menu-trigger">•••</summary><div class="f-menu-panel" role="menu"><button role="menuitem">Open record</button></div></details></div>`, `
            const header=document.querySelector('#header'), owner=header.querySelector('details'), trigger=owner.querySelector('summary'), panel=document.querySelector('#popup'), row=document.querySelector('#row'), rowTrigger=row.querySelector('summary'), rowPanel=row.querySelector('.f-menu-panel');
            const measure=()=>({headerHeight:header.getBoundingClientRect().height,ownerHeight:owner.getBoundingClientRect().height,triggerBottom:trigger.getBoundingClientRect().bottom,panelTop:panel.getBoundingClientRect().top,panelHeight:panel.getBoundingClientRect().height,panelWidth:panel.getBoundingClientRect().width,viewportWidth:document.documentElement.clientWidth,position:getComputedStyle(panel).position,detailsOpen:owner.open,popoverOpen:panel.matches(':popover-open')});
            const initial=measure(), rowHeight=row.getBoundingClientRect().height, rowWidth=row.querySelector('details').getBoundingClientRect().width;
            rowTrigger.click(); await wait(); const nativeRow={before:rowHeight,after:row.getBoundingClientRect().height,widthBefore:rowWidth,widthAfter:row.querySelector('details').getBoundingClientRect().width,position:getComputedStyle(rowPanel).position}; rowTrigger.click(); await wait();
            trigger.click(); await wait(); const native=measure(); initializePopups(document); await wait(); const enhanced=measure();
            initializePopups(document); await wait(); const rescanned=measure(); document.body.dispatchEvent(new PointerEvent('pointerdown',{bubbles:true})); await wait(); const closed=measure();
            return {initial,native,nativeRow,enhanced,rescanned,closed};`, { style:'#header{display:flex;justify-content:flex-end;align-items:center;width:800px}' });
        for (const stage of ['native','enhanced','rescanned','closed']) {
            assert.equal(measurements[stage].headerHeight, measurements.initial.headerHeight, stage + ' changed the header height');
            assert.equal(measurements[stage].ownerHeight, measurements.initial.ownerHeight, stage + ' changed the trigger layout');
        }
        assert.equal(measurements.native.position, 'absolute');
        assert.equal(measurements.native.popoverOpen, false);
        assert.equal(measurements.native.panelTop, measurements.native.triggerBottom + 6);
        assert.equal(measurements.nativeRow.position, 'static');
        assert.ok(measurements.nativeRow.after > measurements.nativeRow.before || measurements.nativeRow.widthAfter > measurements.nativeRow.widthBefore, 'The generic row menu lost its unclipped native fallback.');
        for (const stage of ['enhanced','rescanned']) {
            assert.equal(measurements[stage].position, 'fixed');
            assert.equal(measurements[stage].popoverOpen, true);
            assert.equal(measurements[stage].panelTop, measurements[stage].triggerBottom + 6);
            assert.equal(measurements[stage].panelWidth, measurements.enhanced.panelWidth, 'Repeated enhancement changed the content-sized popup width.');
            assert.ok(measurements[stage].panelWidth < measurements[stage].viewportWidth / 2, 'The native percentage minimum stretched the popup to the viewport.');
        }
        assert.equal(measurements.closed.popoverOpen, false);
        assert.equal(measurements.closed.detailsOpen, false);
        assert.equal(measurements.closed.panelHeight, 0);
    });
    for (const [mode, styles] of [['Framework', frameworkCss], ['Design', css], ['Classic fallback', withoutBaseSelect(css)]]) {
        await check('Real browser ' + mode + ' keeps standard icon select input and native selection', async () => {
            const result = await browserSessionFixture(`<div style="width:340px">${nativeLanguage}<button id="after-select" type="button">After select</button></div>`, async ({evaluate,send}) => {
                const initial = await evaluate(`(()=>{const select=document.querySelector('select'), button=select.querySelector('button'); select.focus(); button.focus();
                    const style=getComputedStyle(select), icon=select.querySelector('[data-icon]');
                    window.changes=[];select.addEventListener('change',()=>window.changes.push(select.value));
                    return {width:select.getBoundingClientRect().width,height:select.getBoundingClientRect().height,appearance:style.appearance,
                        display:style.display,visibility:style.visibility,opacity:style.opacity,focusOnSelect:document.activeElement===select,
                        options:[...select.options].map(option=>({text:option.text,value:option.value,selected:option.selected,color:getComputedStyle(option).color})),
                        icon:icon.getAttribute('data-icon'),label:select.getAttribute('aria-label'),value:select.value};})()`);
                if (mode !== 'Classic fallback') {
                    const opened = await evaluate(`(async()=>{const select=document.querySelector('select');select.showPicker();await new Promise(resolve=>setTimeout(resolve,50));
                        const picker=getComputedStyle(select,'::picker(select)');return {open:select.matches(':open'),width:picker.width,maxWidth:picker.maxWidth,
                            color:picker.color,background:picker.backgroundColor,viewport:document.documentElement.clientWidth,
                            options:[...select.options].map(option=>({left:option.getBoundingClientRect().left,top:option.getBoundingClientRect().top,width:option.getBoundingClientRect().width,height:option.getBoundingClientRect().height,color:getComputedStyle(option).color}))};})()`, true);
                    const option=opened.options[1];
                    await send('Input.dispatchMouseEvent',{type:'mousePressed',x:option.left+option.width/2,y:option.top+option.height/2,button:'left',clickCount:1});
                    await send('Input.dispatchMouseEvent',{type:'mouseReleased',x:option.left+option.width/2,y:option.top+option.height/2,button:'left',clickCount:1});
                    const selected=await evaluate(`({value:document.querySelector('select').value,changes:window.changes,open:document.querySelector('select').matches(':open')})`);
                    return {initial,opened,selected};
                }
                await send('Input.dispatchKeyEvent',{type:'keyDown',key:'ArrowDown',code:'ArrowDown',windowsVirtualKeyCode:40});
                await send('Input.dispatchKeyEvent',{type:'keyUp',key:'ArrowDown',code:'ArrowDown',windowsVirtualKeyCode:40});
                return {initial,selected:await evaluate(`({value:document.querySelector('select').value,changes:window.changes})`)};
            }, { styles });
            assert.equal(result.initial.focusOnSelect,true,'The first native button added a second focus target.');
            assert.equal(result.initial.visibility,'visible');
            assert.equal(result.initial.opacity,'1');
            assert.notEqual(result.initial.display,'none');
            assert.equal(result.initial.label,'Language');
            assert.equal(result.initial.icon,'language');
            assert.deepEqual(result.initial.options.map(option=>option.value),['pt-BR','en-US','zh-CN']);
            assert.equal(result.initial.options.filter(option=>option.selected).length,1);
            assert.equal(result.initial.value,'pt-BR');
            assert.equal(result.selected.value,'en-US');
            assert.deepEqual(result.selected.changes,['en-US']);
            if (mode !== 'Classic fallback') {
                assert.equal(result.initial.appearance,'base-select');
                assert.equal(result.initial.width,48);
                assert.equal(result.initial.height,48);
                assert.equal(result.opened.open,true);
                assert.ok(Number.parseFloat(result.opened.width)<result.opened.viewport/2,'The native icon picker stretched to the viewport.');
                assert.ok(result.opened.options.every(option=>option.width>0&&option.height>0),'Native options did not enter the browser picker.');
                assert.notEqual(result.opened.color,result.opened.background,'The native picker lost its readable foreground.');
                assert.equal(result.selected.open,false,'A selected native option did not close the weak browser picker.');
            } else {
                assert.equal(result.initial.width,340,'Classic fallback text was clipped to an icon target.');
                assert.notEqual(result.initial.appearance,'base-select');
                assert.ok(result.initial.options.every(option=>option.text.length>0),'Classic fallback discarded option captions.');
            }
        });
    }
    await check('Real browser native Quiet select inherits ShellHeader foreground without recoloring its picker', async () => {
        const markup = `<header class="shell-header" style="--f-chrome:#153A61;--f-chrome-ink:#FFFFFF"><div class="shell-header-start"><a class="shell-header-brand" href="/" aria-label="Home"><span class="shell-header-title"><strong>Fixture</strong></span></a></div><div class="shell-header-actions"><div class="f-topbar-slot"><span style="--f-text:#000000;--f-surface:#FFFFFF;--f-accent-text:#000000">${nativeLanguage}</span></div><div class="f-topbar-slot"><button id="ordinary-quiet" type="button" class="f-button f-button-quiet f-button-icon" aria-label="Sign out"><span class="f-icon" aria-hidden="true">Exit</span></button></div></div></header>`;
        const result = await browserSessionFixture(markup, async ({evaluate}) => {
            const initial = await evaluate(`(()=>{const header=document.querySelector('.shell-header'),select=document.querySelector('select'),trigger=select.querySelector('button'),icon=trigger.querySelector('.f-icon'),ordinary=document.querySelector('#ordinary-quiet');
                return {headerColor:getComputedStyle(header).color,headerBackground:getComputedStyle(header).backgroundColor,ordinaryColor:getComputedStyle(ordinary).color,
                    selectColor:getComputedStyle(select).color,triggerColor:getComputedStyle(trigger).color,iconColor:getComputedStyle(icon).color,appearance:getComputedStyle(select).appearance};})()`);
            const picker = await evaluate(`(async()=>{const select=document.querySelector('select');select.showPicker();await new Promise(resolve=>setTimeout(resolve,50));const style=getComputedStyle(select,'::picker(select)');
                return {open:select.matches(':open'),color:style.color,background:style.backgroundColor,optionColors:[...select.options].map(option=>getComputedStyle(option).color)};})()`,true);
            return {initial,picker};
        });
        process.stdout.write('MEASURE chrome native select colors ' + JSON.stringify(result) + '\n');
        assert.equal(result.initial.appearance,'base-select');
        assert.equal(result.initial.ordinaryColor,result.initial.headerColor);
        assert.equal(result.initial.iconColor,result.initial.ordinaryColor,'The native Quiet select icon lost the chrome foreground.');
        assert.equal(result.initial.triggerColor,result.initial.ordinaryColor);
        assert.equal(result.initial.selectColor,result.initial.ordinaryColor);
        assert.equal(result.picker.open,true);
        assert.equal(result.picker.color,'rgb(0, 0, 0)');
        assert.equal(result.picker.background,'rgb(255, 255, 255)');
        assert.ok(result.picker.optionColors.every(color=>color==='rgb(0, 0, 0)'));
    });
    await check('Real browser native login picker opens beneath floating preferences and closes without changing the viewport', async () => {
        const result = await browserSessionFixture(floatingSurface(), async ({evaluate,send}) => {
            const initial = await evaluate(`(async()=>{const wait=()=>new Promise(resolve=>setTimeout(resolve,50));${surfaceMeasurement}
                window.measureSurface=measure;window.changes=[];const select=floating.querySelector('select');
                select.addEventListener('change',()=>window.changes.push(select.value));return measure();})()`);
            const opened = await evaluate(`(async()=>{const select=document.querySelector('[data-culture-picker]');select.showPicker();await new Promise(resolve=>setTimeout(resolve,50));
                return {surface:window.measureSurface(),open:select.matches(':open'),triggerBottom:select.getBoundingClientRect().bottom,
                    options:[...select.options].map(option=>({left:option.getBoundingClientRect().left,top:option.getBoundingClientRect().top,width:option.getBoundingClientRect().width,height:option.getBoundingClientRect().height}))};})()`,true);
            await send('Input.dispatchMouseEvent',{type:'mousePressed',x:5,y:5,button:'left',clickCount:1});
            await send('Input.dispatchMouseEvent',{type:'mouseReleased',x:5,y:5,button:'left',clickCount:1});
            const closed = await evaluate(`(async()=>{await new Promise(resolve=>setTimeout(resolve,50));return {surface:window.measureSurface(),open:document.querySelector('[data-culture-picker]').matches(':open'),changes:window.changes};})()`);
            return {initial,opened,closed};
        });
        for (const measurement of [result.initial,result.opened.surface,result.closed.surface]) {
            floatingViewport(measurement);
            assert.equal(measurement.bandTop,0);
            assert.equal(measurement.bandHeight,measurement.viewport);
            assert.equal(measurement.stageScrollHeight,measurement.stageHeight);
        }
        assert.equal(result.opened.open,true,'The login fixture did not open the actual browser picker.');
        assert.equal(result.opened.options.length,3);
        assert.ok(result.opened.options.every(option=>option.width>0&&option.height>0));
        assert.ok(result.opened.options[0].top>=result.opened.triggerBottom,'The native language picker did not open beneath its trigger.');
        assert.equal(result.closed.open,false,'Outside input did not close the weak native language picker.');
        assert.deepEqual(result.closed.changes,[],'Dismissing the native language picker changed its value.');
        process.stdout.write('MEASURE native floating login picker ' + JSON.stringify({surface:result.opened.surface,triggerBottom:result.opened.triggerBottom,options:result.opened.options}) + '\n');
    });
    // Chromium window-size includes platform chrome; calibrate from its measured CSS viewport.
    let viewport768WindowHeight;
    await check('Real browser actual native login selector and floating preferences reserve no header or band space', async () => {
        const measurements = await browserFixture(floatingSurface(), surfaceMeasurement + `
            const initial=measure(), select=floating.querySelector('select'), trigger=select.querySelector(':scope > button');
            select.focus(); await wait(); const focused=measure(); initializePopups(document); await wait(); const enhanced=measure();
            const selector={tag:select.tagName,firstChild:select.firstElementChild.tagName,triggerClass:trigger.className,icon:trigger.querySelector('[data-icon]')?.getAttribute('data-icon'),label:select.getAttribute('aria-label'),options:[...select.options].map(option=>option.value),value:select.value,disabled:select.disabled,legacyMenu:!!floating.querySelector('details, [role="menu"]')};
            select.blur(); window.scrollTo(0,10000); await wait(); return {initial,focused,enhanced,selector,closed:measure()};`);
        for (const stage of ['initial','focused','enhanced','closed']) {
            floatingViewport(measurements[stage]);
            assert.equal(measurements[stage].bandTop, 0);
            assert.equal(measurements[stage].bandHeight, measurements[stage].viewport);
            assert.equal(measurements[stage].stageScrollHeight, measurements[stage].stageHeight);
        }
        assert.equal(measurements.selector.tag, 'SELECT');
        assert.equal(measurements.selector.firstChild, 'BUTTON');
        assert.equal(measurements.selector.triggerClass, 'f-select-trigger f-button f-button-quiet f-button-icon');
        assert.equal(measurements.selector.icon, 'language');
        assert.equal(measurements.selector.label, 'Language');
        assert.deepEqual(measurements.selector.options, ['pt-BR','en-US','zh-CN']);
        assert.equal(measurements.selector.value, 'pt-BR');
        assert.equal(measurements.selector.disabled, false);
        assert.equal(measurements.selector.legacyMenu, false);
        viewport768WindowHeight = 900 + 768 - measurements.initial.viewport;
        process.stdout.write('MEASURE desktop floating login ' + JSON.stringify(measurements.initial) + '\n');
    });
    await check('Real browser actual two-field floating login fits a 768px viewport without shrinking branding', async () => {
        const measurements = await browserFixture(floatingSurface(), surfaceMeasurement + 'return measure();', { height:viewport768WindowHeight });
        floatingViewport(measurements);
        assert.equal(measurements.viewport, 768);
        assert.equal(measurements.bandTop, 0);
        assert.equal(measurements.bandHeight, measurements.viewport);
        assert.equal(measurements.stageScrollHeight, measurements.stageHeight);
        assert.equal(measurements.bandPadding, '16px');
        assert.equal(measurements.logoHeight, 96);
        assert.equal(measurements.titleSize, '80px');
        process.stdout.write('MEASURE exact 768px actual floating login ' + JSON.stringify(measurements) + '\n');
    });
    await check('Real browser ordinary TitleBar still reserves its row for short fixed content at 768px', async () => {
        const measurements = await browserFixture(fixedSurface(preferences, 2, '<p>Short content</p><form method="post"><button id="final-submit" type="submit" class="f-button">Continue</button></form>'), surfaceMeasurement + 'return measure();', { height:viewport768WindowHeight });
        fixedViewport(measurements);
        assert.equal(measurements.viewport, 768);
        assert.equal(measurements.headerHeight, 48);
        assert.equal(measurements.floatingHeight, 0);
        assert.equal(measurements.bandHeight, 720);
        assert.equal(measurements.stageScrollHeight, 720);
        assert.equal(measurements.bandPadding, '48px');
    });
    for (const [name, header] of [['omitted fragment', null], ['empty fragment', '']]) {
        await check('Real browser fixed login reserves no blank row for ' + name, async () => {
            for (const surface of [fixedSurface, floatingSurface]) {
                const measurements = await browserFixture(surface(header), surfaceMeasurement + 'return measure();');
                fixedViewport(measurements);
                assert.equal(measurements.headerHeight, 0);
                assert.equal(measurements.floatingHeight, 0);
                assert.equal(measurements.stageTop, 0);
                assert.equal(measurements.bandHeight, measurements.viewport);
            }
        });
    }
    for (const [name, fields, height] of [['short viewport', 2, 420], ['long native form', 12, 900]]) {
        await check('Real browser floating login scrolls only its main to the final submit for ' + name, async () => {
            const measurements = await browserFixture(floatingSurface(preferenceActions, fields), surfaceMeasurement + `
                const initial=measure(); window.scrollTo(0,10000); main.scrollTop=main.scrollHeight; await wait(); return {initial,scrolled:measure()};`, { height });
            floatingViewport(measurements.initial); floatingViewport(measurements.scrolled);
            assert.ok(measurements.initial.bandHeight > measurements.initial.stageHeight);
            assert.ok(measurements.scrolled.stageScroll > 0);
            assert.ok(measurements.scrolled.submitTop >= measurements.scrolled.stageTop);
            assert.ok(measurements.scrolled.submitBottom <= measurements.scrolled.stageBottom);
            process.stdout.write('MEASURE ' + name + ' ' + JSON.stringify(measurements.scrolled) + '\n');
        });
    }
    await check('Real browser presentation uses floating tutorial segments without a footer tail row', async () => {
        const markup = `<div class="content-surface f-document-surface" id="surface"><a class="skip-link" href="#content">Skip</a><div class="f-document-surface-header">${preferences}</div><main class="content-stage" id="content" tabindex="-1"><div class="f-page-body f-page-full">${band('<p>First scene</p>', 'first').replace('f-presentation-surface', 'f-presentation-primary')}${band('<p>Second scene</p>', 'second')}</div></main>
            <div class="f-content-surface-footer" data-presentation-footer><footer class="f-presentation-footer"><span class="f-presentation-watermark" aria-hidden="true">Fixture</span><div class="f-content-container f-presentation-footer-content"><div class="f-presentation-footer-identity"><strong>Fixture</strong><span>Copyright</span></div><nav class="f-inline-actions"><a id="footer-price" class="f-button f-button-underline" href="/pricing/">Pricing</a></nav></div></footer></div>
            <nav id="viewport-progress" class="f-presentation-progress" data-content-id="content" aria-label="On this page" hidden><ol class="f-tutorial-segments"></ol></nav></div>`;
        const measurements = await browserFixture(markup, `
            const root=document.querySelector('#surface'),main=document.querySelector('#content'),footer=document.querySelector('footer'),progress=document.querySelector('.f-presentation-progress');
            const before=document.scrollingElement.scrollHeight;
            progressApi.initializePresentationProgress(document); await wait();
            const entries=[...progress.querySelectorAll('a')],links=entries.map(link=>link.getAttribute('href'));
            const bands=[...main.querySelectorAll('[data-presentation-band="true"]')].map(band=>({height:band.getBoundingClientRect().height,padding:getComputedStyle(band).paddingTop,id:band.id}));
            const segmentStyle=element=>{const actual=getComputedStyle(element),bar=getComputedStyle(element,'::after');return {width:actual.width,minHeight:actual.minHeight,barWidth:bar.width,barHeight:bar.height,barRadius:bar.borderRadius,opacity:bar.opacity,shadow:bar.boxShadow};};
            const reference=document.createElement('nav');reference.className='f-tutorial-segments';reference.style.cssText='position:fixed;left:-1000px;top:0';reference.style.color=getComputedStyle(progress).color;
            reference.innerHTML='<button type="button" class="f-tutorial-segment" aria-current="step" aria-label="First"><span class="f-sr-only">First</span></button><button type="button" class="f-tutorial-segment" aria-label="Second"><span class="f-sr-only">Second</span></button>';document.body.append(reference);
            const tutorial={active:segmentStyle(reference.children[0]),inactive:segmentStyle(reference.children[1]),gap:getComputedStyle(reference).gap};
            const presentation={active:segmentStyle(entries[0]),inactive:segmentStyle(entries[1]),gap:getComputedStyle(progress.querySelector('ol')).gap};reference.remove();
            const rect=progress.getBoundingClientRect(),initial={hidden:progress.hidden,position:getComputedStyle(progress).position,center:(rect.left+rect.right)/2,viewportCenter:document.documentElement.clientWidth/2,viewportWidth:window.innerWidth,clientWidth:document.documentElement.clientWidth,bottom:rect.bottom,viewport:window.innerHeight,foreground:getComputedStyle(progress).color,bandForeground:getComputedStyle(document.querySelector('#first')).color,background:getComputedStyle(progress).backgroundColor,border:getComputedStyle(progress).borderTopWidth,hasTitle:!!progress.querySelector('[title], [role="tooltip"], .f-section-label'),labels:entries.map(link=>link.getAttribute('aria-label'))};
            window.scrollTo(0,document.scrollingElement.scrollHeight); window.dispatchEvent(new Event('scroll')); await wait();
            const price=document.querySelector('#footer-price'),priceRect=price.getBoundingClientRect(),hit=document.elementFromPoint(priceRect.left+priceRect.width/2,priceRect.top+priceRect.height/2);let clicks=0;price.addEventListener('click',event=>{event.preventDefault();clicks++;});hit?.click();
            const tail={height:document.scrollingElement.scrollHeight,scroll:window.scrollY,footerBottom:footer.getBoundingClientRect().bottom,footerTop:footer.getBoundingClientRect().top,footerHeight:footer.getBoundingClientRect().height,footerInMain:main.contains(footer),footerBand:footer.hasAttribute('data-presentation-band'),stageOverflow:getComputedStyle(main).overflowY,progressInSurface:progress.closest('.content-surface')===root,hidden:progress.hidden,position:getComputedStyle(progress).position,foreground:getComputedStyle(progress).color,bandForeground:getComputedStyle(document.querySelector('#second')).color,progressBottom:progress.getBoundingClientRect().bottom,current:entries.findIndex(link=>link.getAttribute('aria-current')==='location'),footerHit:hit===price,clicks};
            // A naturally longer footer can occupy the viewport by itself, without manufacturing another band.
            for(let index=0;index<24;index++){const paragraph=document.createElement('p');paragraph.textContent='Footer information '+index;footer.querySelector('.f-presentation-footer-identity').append(paragraph);}
            await wait();window.scrollTo(0,document.scrollingElement.scrollHeight);window.dispatchEvent(new Event('scroll'));await wait();
            const footerOnly={hidden:progress.hidden,position:getComputedStyle(progress).position,height:document.scrollingElement.scrollHeight,scroll:window.scrollY,footerBottom:footer.getBoundingClientRect().bottom,mainBottom:main.getBoundingClientRect().bottom,current:entries.findIndex(link=>link.getAttribute('aria-current')==='location')};
            window.scrollTo(0,document.querySelector('#second').offsetTop);window.dispatchEvent(new Event('scroll'));await wait();entries[1].click();await new Promise(resolve=>setTimeout(resolve,1200));
            const restored={hidden:progress.hidden,position:getComputedStyle(progress).position,current:entries.findIndex(link=>link.getAttribute('aria-current')==='location'),bandTop:document.querySelector('#second').getBoundingClientRect().top,headerBottom:root.querySelector('.f-document-surface-header').getBoundingClientRect().bottom,sameLinks:entries.every((link,index)=>progress.querySelectorAll('a')[index]===link)};
            return {before,after:tail.height,bands,links,tutorial,presentation,initial,tail,footerOnly,restored};`);
        process.stdout.write('MEASURE presentation progress ' + JSON.stringify(measurements) + '\n');
        assert.equal(measurements.bands.length, 2);
        for (const band of measurements.bands) { assert.equal(band.height, measurements.initial.viewport); assert.equal(band.padding, '48px'); }
        assert.deepEqual(measurements.links, ['#first','#second']);
        assert.deepEqual(measurements.presentation, measurements.tutorial);
        assert.equal(measurements.presentation.active.width, '32px');
        assert.equal(measurements.presentation.active.minHeight, '44px');
        assert.equal(measurements.presentation.active.barHeight, '8px');
        assert.equal(measurements.presentation.gap, '8px');
        assert.equal(measurements.initial.hidden, false);
        assert.equal(measurements.initial.position, 'fixed');
        assert.ok(Math.abs(measurements.initial.center-measurements.initial.viewportCenter)<=1);
        assert.ok(measurements.initial.bottom<=measurements.initial.viewport);
        assert.equal(measurements.initial.foreground, measurements.initial.bandForeground);
        assert.equal(measurements.initial.background, 'rgba(0, 0, 0, 0)');
        assert.equal(measurements.initial.border, '0px');
        assert.equal(measurements.initial.hasTitle, false);
        assert.deepEqual(measurements.initial.labels, ['first','second']);
        assert.equal(measurements.before, measurements.after, 'Progress added a document-flow row after the footer');
        const tail=measurements.tail;
        assert.ok(tail.scroll>0); assert.ok(tail.footerHeight>0 && tail.footerHeight<measurements.initial.viewport);
        assert.ok(Math.abs(tail.height-(tail.scroll+tail.footerBottom))<=1, 'The document extends below its natural footer');
        assert.equal(tail.footerInMain, false); assert.equal(tail.footerBand, false); assert.equal(tail.stageOverflow, 'visible');
        assert.equal(tail.progressInSurface, true); assert.equal(tail.hidden, false); assert.equal(tail.position, 'fixed');
        assert.equal(tail.foreground, tail.bandForeground); assert.notEqual(tail.foreground, measurements.initial.foreground);
        assert.ok(tail.progressBottom<=tail.footerTop); assert.equal(tail.current, 1); assert.equal(tail.footerHit, true); assert.equal(tail.clicks, 1);
        assert.equal(measurements.footerOnly.hidden, true); assert.equal(measurements.footerOnly.position, 'fixed');
        assert.ok(measurements.footerOnly.mainBottom<=0); assert.equal(measurements.footerOnly.current, 1);
        assert.ok(Math.abs(measurements.footerOnly.height-(measurements.footerOnly.scroll+measurements.footerOnly.footerBottom))<=1);
        assert.equal(measurements.restored.hidden, false); assert.equal(measurements.restored.position, 'fixed'); assert.equal(measurements.restored.current, 1);
        assert.equal(measurements.restored.sameLinks, true); assert.ok(Math.abs(measurements.restored.bandTop-measurements.restored.headerBottom)<=1);
    });
} else {
    browserChecksSkipped = 14;
    process.stdout.write('SKIP real-browser SSR geometry: no installed Chromium browser found.\n');
}
process.stdout.write(`${checks}/${checks} ActionMenu current-controller DOM checks passed; ${browserChecksSkipped} real-browser checks skipped.\n`);