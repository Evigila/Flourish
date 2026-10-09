import fs from 'node:fs/promises';
import assert from 'node:assert/strict';

let checks = 0;
const check = async (name, action) => { await action(); checks++; process.stdout.write(`PASS ${name}\n`); };
class Style {
    values = new Map();
    getPropertyValue(name) { return this.values.get(name) ?? ''; }
    getPropertyPriority() { return ''; }
    setProperty(name,value) { this.values.set(name,value); }
    removeProperty(name) { this.values.delete(name); }
}
class Node {
    attrs = new Map(); events = new Map(); children = []; style = new Style(); parent = null;
    isConnected = true; disabled = false; popoverOpen = false; shows = 0; hides = 0;
    constructor(tag = 'button') { this.tag = tag; }
    append(...nodes) { this.children.push(...nodes); nodes.forEach(node => node.parent = this); }
    addEventListener(name,handler) { const handlers = this.events.get(name) ?? []; handlers.push(handler); this.events.set(name,handlers); }
    removeEventListener(name,handler) { this.events.set(name,(this.events.get(name) ?? []).filter(value => value !== handler)); }
    emit(name,event = {}) { for (const handler of [...(this.events.get(name) ?? [])]) handler(event); }
    setAttribute(name,value) { this.attrs.set(name,String(value)); }
    getAttribute(name) { return this.attrs.get(name) ?? null; }
    removeAttribute(name) { this.attrs.delete(name); }
    matches(selector) { return selector === ':popover-open' && this.popoverOpen; }
    contains(node) { return node === this || this.children.some(child => child.contains(node)); }
    focus() { const previous = document.activeElement; if (previous === this) return; document.activeElement = this; previous?.emit('blur'); this.emit('focus'); }
    getBoundingClientRect() { return {left:700,right:748,top:10,bottom:58,width:this.tag === 'aside' ? 280 : 48,height:this.tag === 'aside' ? 220 : 48}; }
    getClientRects() { return [this.getBoundingClientRect()]; }
    showPopover() { assert.equal(this.popoverOpen,false); this.popoverOpen = true; this.shows++; queueMicrotask(() => this.emit('toggle',{newState:'open'})); }
    hidePopover() { if (!this.popoverOpen) return; this.popoverOpen = false; this.hides++; queueMicrotask(() => this.emit('toggle',{newState:'closed'})); }
}
globalThis.document = new Node('document'); document.documentElement = new Node('html'); document.documentElement.clientWidth = 800;
document.body = new Node('body'); document.activeElement = document.body;
globalThis.window = new Node('window'); window.innerHeight = 600;
const observers = [];
globalThis.MutationObserver = class { constructor(callback) { this.callback = callback; observers.push(this); } observe() {} disconnect() { this.disconnected = true; } };
globalThis.getComputedStyle = node => ({length:1,0:'--f-primary',maxHeight:node.style.getPropertyValue('max-height') || 'none',getPropertyValue:name => node.style.getPropertyValue(name)});
const root = new URL('../../src/Flourish.Blazor/Flourish.Blazor.Framework/wwwroot/',import.meta.url);
const dependency = 'data:text/javascript;base64,' + Buffer.from(await fs.readFile(new URL('primitives/interaction-origin.js',root),'utf8')).toString('base64');
const source = await fs.readFile(new URL('controls.js',root),'utf8');
const api = await import('data:text/javascript;base64,' + Buffer.from(source.replace(/(["'])\.\/primitives\/interaction-origin\.js\1/,JSON.stringify(dependency))).toString('base64'));
const trigger = new Node(), preview = new Node('aside'), board = new Node('section'), action = new Node();
preview.id = 'tutorial-preview'; board.append(action);
trigger.style.setProperty('--f-primary','#153A32');
const calls = [];
const reference = {invokeMethodAsync: async name => calls.push(name)};
const settle = () => new Promise(resolve => queueMicrotask(resolve));
const delay = () => new Promise(resolve => setTimeout(resolve,110));

await check('Repeated synchronization has one controller and closed popovers occupy no layout', () => {
    api.synchronizeTutorialBoard(trigger,preview,board,false,reference);
    api.synchronizeTutorialBoard(trigger,preview,board,false,reference);
    assert.equal(trigger.events.get('pointerenter').length,1); assert.equal(board.events.get('toggle').length,1);
    assert.equal(preview.popoverOpen,false); assert.equal(board.popoverOpen,false); assert.equal(trigger.getAttribute('aria-expanded'),'false');
});
await check('Hover opens a passive preview without moving focus and shares scoped palette/positioning', async () => {
    const before = document.activeElement; trigger.emit('pointerenter'); await settle();
    assert.equal(preview.popoverOpen,true); assert.equal(document.activeElement,before);
    assert.equal(trigger.getAttribute('aria-describedby'),'tutorial-preview');
    assert.equal(preview.style.getPropertyValue('--f-primary'),'#153A32');
    assert.equal(preview.style.left,'468px'); assert.equal(preview.style.top,'58px');
});
await check('Crossing to the preview keeps it open and leaving both owned regions dismisses it', async () => {
    trigger.emit('pointerleave'); preview.emit('pointerenter'); await delay(); assert.equal(preview.popoverOpen,true);
    preview.emit('pointerleave'); await delay(); assert.equal(preview.popoverOpen,false); assert.equal(trigger.getAttribute('aria-describedby'),null);
});
await check('Keyboard focus previews progress and Escape dismisses only that preview', async () => {
    trigger.focus(); await settle(); assert.equal(preview.popoverOpen,true);
    const event = {key:'Escape',preventDefault(){this.prevented = true;}}; trigger.emit('keydown',event);
    assert.equal(event.prevented,true); assert.equal(preview.popoverOpen,false); assert.equal(calls.length,0);
});
await check('Expanded tutorial is a native nonmodal top-layer board without a focus trap or inert background', async () => {
    api.synchronizeTutorialBoard(trigger,preview,board,true,reference); await settle();
    assert.equal(board.popoverOpen,true); assert.equal(preview.popoverOpen,false); assert.equal(document.activeElement,board);
    assert.equal(board.style.getPropertyValue('--f-primary'),'#153A32'); assert.equal(trigger.getAttribute('aria-expanded'),'true');
    assert.equal(document.events.get('keydown')?.length ?? 0,0); assert.equal(board.events.get('keydown')?.length ?? 0,0);
    assert.equal(document.body.getAttribute('inert'),null); assert.equal(document.body.style.getPropertyValue('overflow'),'');
    const outside = new Node(); outside.focus(); assert.equal(document.activeElement,outside);
});
await check('Native outside light-dismiss publishes close once and never steals outside target focus', async () => {
    const outside = document.activeElement; board.hidePopover(); await settle(); await settle();
    assert.deepEqual(calls,['RequestCloseAsync']); assert.equal(document.activeElement,outside); assert.equal(trigger.getAttribute('aria-expanded'),'false');
    api.synchronizeTutorialBoard(trigger,preview,board,false,reference); await settle(); assert.equal(board.popoverOpen,false);
});
await check('Native Escape dismissal restores the invoker when focus belonged to the board', async () => {
    api.synchronizeTutorialBoard(trigger,preview,board,true,reference); await settle(); action.focus();
    board.hidePopover(); await settle(); await settle();
    assert.equal(document.activeElement,trigger); assert.deepEqual(calls,['RequestCloseAsync','RequestCloseAsync']);
    api.synchronizeTutorialBoard(trigger,preview,board,false,reference);
});
await check('Host close hides the native board without dispatching a second bound close event', async () => {
    api.synchronizeTutorialBoard(trigger,preview,board,true,reference); await settle(); action.focus();
    api.synchronizeTutorialBoard(trigger,preview,board,false,reference); await settle(); await settle();
    assert.equal(board.popoverOpen,false); assert.equal(document.activeElement,trigger); assert.equal(calls.length,2);
});
await check('Preview refresh follows the trigger theme instead of retaining the previous scope', async () => {
    trigger.emit('pointerenter'); await settle(); trigger.style.setProperty('--f-primary','#BBD7C9');
    api.synchronizeTutorialBoard(trigger,preview,board,false,reference);
    assert.equal(preview.style.getPropertyValue('--f-primary'),'#BBD7C9');
});
await check('Disposal and removed DOM release listeners, timers, native popovers and palette overrides', async () => {
    api.synchronizeTutorialBoard(trigger,preview,board,true,reference); await settle();
    const observer = observers.at(-1); board.isConnected = false; observer.callback(); await settle();
    api.detachTutorialBoard(trigger,preview,board);
    assert.equal(observer.disconnected,true); assert.equal(board.popoverOpen,false); assert.equal(preview.popoverOpen,false);
    assert.equal(trigger.events.get('pointerenter').length,0); assert.equal(board.events.get('toggle').length,0);
    assert.equal(preview.style.getPropertyValue('--f-primary'),''); assert.equal(board.style.getPropertyValue('--f-primary'),'');
    assert.equal(window.events.get('resize').length,0); assert.equal(document.events.get('scroll').length,0); assert.equal(calls.length,2);
});
await check('Missing native Popover API is an explicit requirement, not a compatibility renderer', () => {
    const unsupported = new Node('section'); unsupported.showPopover = undefined;
    assert.throws(() => api.synchronizeTutorialBoard(new Node(),new Node('aside'),unsupported,true,reference),/native Popover API/);
});
await check('Tutorial geometry and skin are bundled separately and remain nonmodal at the CSS boundary', async () => {
    const framework = await fs.readFile(new URL('framework.css',root),'utf8');
    const designRoot = new URL('../../src/Flourish.Blazor/Flourish.Blazor.Design/wwwroot/',import.meta.url);
    const design = await fs.readFile(new URL('design.css',designRoot),'utf8');
    const geometry = await fs.readFile(new URL('tutorial-board.css',root),'utf8');
    const skin = await fs.readFile(new URL('tutorial-board.css',designRoot),'utf8');
    assert.match(framework,/tutorial-board\.css/); assert.match(design,/tutorial-board\.css/);
    assert.match(geometry,/\.f-tutorial-board::backdrop\s*\{[^}]*pointer-events:none[^}]*background:transparent/);
    assert.match(geometry,/grid-template-columns:minmax\(0,1fr\) minmax\(0,auto\) minmax\(0,1fr\)/);
    assert.match(skin,/font-size:var\(--f-type-h1,34px\)/); assert.match(skin,/\.f-tutorial-primary\s*\{[^}]*var\(--f-primary-ink\)[^}]*var\(--f-primary\)/);
});
process.stdout.write(`${checks}/${checks} TutorialBoard current-controller DOM checks passed.\n`);
