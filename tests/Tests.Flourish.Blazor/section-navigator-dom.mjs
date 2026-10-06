import fs from 'node:fs/promises';
import assert from 'node:assert/strict';

const nodes = [], frames = new Map(), timers = new Map(); let frameId = 0, timerId = 0, now = 0, count = 0, reducedMotion = false;
const check = (name, action) => { action(); count++; process.stdout.write(`PASS ${name}\n`); };
class Node {
    constructor(tag = 'div', classes = []) { this.tag = tag; this.classes = new Set(classes); this.attrs = new Map(); this.children = []; this.parentElement = null; this.events = new Map(); this.isConnected = true; this.hidden = false; this.scrollTop = 0; this.scrollHeight = 1500; this.clientHeight = 500; this.textContent = ''; this.rect = { left:100, right:900, top:100, bottom:600, height:500 }; this.style = { values:new Map(), setProperty(name, value) { this.values.set(name, value); } }; nodes.push(this); }
    get id() { return this.attrs.get('id') ?? ''; }
    set id(value) { this.attrs.set('id', value); }
    append(...children) { children.forEach(child => { child.parentElement = this; this.children.push(child); }); }
    setAttribute(name, value) { this.attrs.set(name, String(value)); }
    getAttribute(name) { return this.attrs.get(name) ?? null; }
    hasAttribute(name) { return this.attrs.has(name); }
    removeAttribute(name) { this.attrs.delete(name); }
    toggleAttribute(name, force) { if (force ?? !this.hasAttribute(name)) this.setAttribute(name, ''); else this.removeAttribute(name); }
    addEventListener(name, callback) { this.events.set(name, [...this.events.get(name) ?? [], callback]); }
    removeEventListener(name, callback) { this.events.set(name, (this.events.get(name) ?? []).filter(value => value !== callback)); }
    emit(name, event = {}) { for (const callback of [...this.events.get(name) ?? []]) callback(event); }
    matches(selector) { if (selector.includes(',')) return selector.split(',').some(value => this.matches(value.trim())); const attribute = selector.match(/^\[([^\]=]+)(?:="([^"]*)")?\]$/); return selector.startsWith('.') ? this.classes.has(selector.slice(1)) : selector === '[hidden]' ? this.hidden || this.hasAttribute('hidden') : attribute ? attribute[2] === undefined ? this.hasAttribute(attribute[1]) : this.getAttribute(attribute[1]) === attribute[2] : this.tag === selector; }
    closest(selector) { for (let node = this; node; node = node.parentElement) if (node.matches(selector)) return node; return null; }
    contains(node) { for (let current = node; current; current = current.parentElement) if (current === this) return true; return false; }
    querySelectorAll(selector) { const found = []; const visit = node => { for (const child of node.children) { if (selector.split(',').some(value => child.matches(value.trim()))) found.push(child); visit(child); } }; visit(this); return found; }
    querySelector(selector) { return this.querySelectorAll(selector)[0] ?? null; }
    getBoundingClientRect() { const offset = this.tag === 'h2' ? this.closest('main')?.scrollTop ?? 0 : 0; return { ...this.rect, top:this.rect.top - offset, bottom:this.rect.bottom - offset }; }
    scrollTo(options) { this.scrolled = options; (this.scrollCalls ??= []).push(options); this.scrollTop = Math.min(options.top, this.scrollHeight - this.clientHeight); this.emit('scroll'); }
    focus(options) { document.activeElement = this; this.focusOptions = options; }
}
globalThis.document = new Node('document'); document.getElementById = id => nodes.find(node => node.isConnected && node.id === id) ?? null;
globalThis.window = new Node('window'); window.innerHeight = 800; window.innerWidth = 1000;
globalThis.getComputedStyle = node => ({ position:node.position ?? 'static' });
globalThis.matchMedia = () => ({ matches:reducedMotion });
globalThis.requestAnimationFrame = callback => { const id = ++frameId; frames.set(id, callback); return id; };
globalThis.cancelAnimationFrame = id => frames.delete(id);
globalThis.setTimeout = (callback, delay) => { const id = ++timerId; timers.set(id, { callback, at:now + delay }); return id; };
globalThis.clearTimeout = id => timers.delete(id);
const advance = milliseconds => { const end = now + milliseconds; let runs = 0; while (true) { const next = [...timers].filter(([, value]) => value.at <= end).sort((a, b) => a[1].at - b[1].at)[0]; if (!next) break; timers.delete(next[0]); now = next[1].at; next[1].callback(); assert.ok(++runs < 50, 'Navigation timers did not settle'); } now = end; };
class Observer { static all = []; constructor(callback) { this.callback = callback; this.observed = []; this.disconnections = 0; Observer.all.push(this); } observe(node) { this.observed.push(node); } disconnect() { this.observed = []; this.disconnections++; } notify() { this.callback(); } }
globalThis.MutationObserver = Observer; globalThis.ResizeObserver = Observer;
const flush = () => { let runs = 0; while (frames.size) { const batch = [...frames.values()]; frames.clear(); batch.forEach(callback => callback()); assert.ok(++runs < 20, 'Observer loop did not settle'); } };
const code = await fs.readFile(new URL('../../src/Flourish.Blazor/Flourish.Blazor.Framework/wwwroot/section-navigator.js', import.meta.url), 'utf8');
const api = await import('data:text/javascript;base64,' + Buffer.from(code).toString('base64'));
const main = new Node('main'); main.id = 'document-content';
const root = new Node('nav'); root.id = 'outline';
const section = (title, top, id) => { const container = new Node('section'); container.rect = { left:132, right:868, top, bottom:top + 200, height:200 }; const heading = new Node('h2'); heading.textContent = title; heading.rect = { left:132, right:868, top, bottom:top + 34, height:34 }; if (id) heading.id = id; container.append(heading); return { container, heading }; };
const first = section('Introduction', 160), second = section('Data', 460), third = section('Summary', 760, 'author-summary');
main.append(first.container, second.container, third.container);
const nested = section('Nested preview', 500); second.container.append(nested.container);
const nestedMain = new Node('main'); main.append(nestedMain); nestedMain.append(section('Embedded document', 650).container);
let bridgeCalls = 0, entries;
const reference = { async invokeMethodAsync(name, values) { assert.equal(name, 'UpdateSectionsAsync'); bridgeCalls++; entries = values; root.children.forEach(node => { node.isConnected = false; }); root.children = []; for (const entry of values) { const link = new Node('a', ['f-section-link']); link.setAttribute('href', '#' + encodeURIComponent(entry.id)); const label = new Node('span', ['f-section-label']); label.textContent = entry.title; link.append(label); root.append(link); } } };
api.synchronize(root, main.id, true, reference);
const outlineLinks = () => root.querySelectorAll('.f-section-link');
check('Discovery uses the host document outline without nested examples', () => { assert.deepEqual(entries.map(value => value.title), ['Introduction', 'Data', 'Summary']); assert.equal(new Set(entries.map(value => value.id)).size, 3); assert.equal(root.hidden, false); });
check('Stable synchronization avoids repeated discovery callbacks and observer churn', () => { const resize = Observer.all[1], before = resize.disconnections; api.synchronize(root, main.id, true, reference); resize.notify(); flush(); assert.equal(bridgeCalls, 1); assert.equal(resize.disconnections, before); });
check('Current section follows content scrolling rather than window position', () => { assert.equal(outlineLinks()[0].getAttribute('aria-current'), 'location'); main.scrollTop = 400; main.emit('scroll'); assert.equal(outlineLinks()[1].getAttribute('aria-current'), 'location'); assert.equal(outlineLinks().filter(link => link.hasAttribute('aria-current')).length, 1); assert.equal(root.style.values.get('--f-section-nav-left'), '874px'); });
const click = target => ({ target, button:0, prevented:false, preventDefault() { this.prevented = true; } });
check('Section activation scrolls only its host and focuses the actual heading', () => { const event = click(outlineLinks()[1]); root.emit('click', event); assert.equal(event.prevented, true); assert.deepEqual(main.scrolled, { top:348, behavior:'smooth' }); assert.equal(window.scrolled, undefined); assert.equal(document.activeElement, second.heading); assert.deepEqual(second.heading.focusOptions, { preventScroll:true }); });
check('Reduced motion uses immediate host scrolling and modified clicks retain native link behavior', () => { reducedMotion = true; root.emit('click', click(outlineLinks()[0])); assert.equal(main.scrolled.behavior, 'auto'); const modified = { ...click(outlineLinks()[1]), ctrlKey:true }; root.emit('click', modified); assert.equal(modified.prevented, false); reducedMotion = false; });
check('Tooltip positioning stays within the viewport and Escape dismisses it', () => { const link = outlineLinks()[0], label = link.querySelector('.f-section-label'); link.rect = { left:874, right:894, top:760, bottom:784, height:24 }; label.rect = { left:590, right:864, top:750, bottom:850, height:100 }; root.emit('pointerover', { target:link }); assert.equal(label.style.values.get('--f-section-tooltip-shift'), '-58px'); root.emit('keydown', { key:'Escape' }); assert.equal(root.hasAttribute('data-tooltip-dismissed'), true); root.emit('focusin', { target:link }); assert.equal(root.hasAttribute('data-tooltip-dismissed'), false); });
check('Route content replacement refreshes entries and releases generated targets', () => { first.heading.isConnected = false; first.container.isConnected = false; main.children = main.children.filter(node => node !== first.container); first.container.parentElement = null; const replacement = section('New section', 180); main.children.unshift(replacement.container); replacement.container.parentElement = main; Observer.all[0].notify(); flush(); assert.equal(bridgeCalls, 2); assert.equal(entries[0].title, 'New section'); assert.equal(first.heading.id, ''); assert.equal(root.events.get('click').length, 1); });
check('Offscreen local hosts hide indicators without affecting document scroll ownership', () => { main.rect = { ...main.rect, top:900, bottom:1400 }; document.emit('scroll'); assert.equal(root.hidden, true); main.rect = { ...main.rect, top:100, bottom:600 }; document.emit('scroll'); assert.equal(root.hidden, false); });
api.detach(root);
check('Disposal removes scroll and input listeners and preserves author identifiers', () => { assert.equal(main.events.get('scroll').length, 0); assert.equal(root.events.get('click').length, 0); assert.equal(document.events.get('scroll').length, 0); assert.equal(window.events.get('resize').length, 0); assert.equal(third.heading.id, 'author-summary'); assert.equal(second.heading.id, ''); assert.equal(second.heading.hasAttribute('tabindex'), false); assert.equal(Observer.all[0].observed.length, 0); assert.equal(Observer.all[1].observed.length, 0); });
const missing = new Node('nav'); api.synchronize(missing, 'missing-content', true, reference);
check('A missing host produces no stale indicator or event listeners', () => { assert.equal(missing.hidden, true); assert.equal(missing.events.size, 0); });

let fixtureSequence = 0;
function referenceFor(nav) {
    return { async invokeMethodAsync(name, values) { assert.equal(name, 'UpdateSectionsAsync'); nav.children.forEach(node => { node.isConnected = false; }); nav.children = []; for (const entry of values) { const link = new Node('a', ['f-section-link']); link.setAttribute('href', '#' + encodeURIComponent(entry.id)); nav.append(link); } } };
}
function fixture({ embedded = false, sticky = false } = {}) {
    const host = new Node('main'), outsideSection = new Node('section'); host.append(outsideSection);
    const content = new Node(embedded ? 'div' : 'main'); content.id = `regression-content-${++fixtureSequence}`; outsideSection.append(content);
    const nav = new Node('nav'); nav.id = `regression-outline-${fixtureSequence}`;
    const one = section('First section', 160), two = section('Second section', 460), three = section('Third section', 760);
    const pageHeading = new Node('header', ['f-page-heading']); pageHeading.position = sticky ? 'sticky' : 'static'; pageHeading.rect.height = 174;
    content.append(pageHeading, one.container, two.container, three.container);
    const deeper = section('Nested own preview', 500); two.container.append(deeper.container);
    api.synchronize(nav, content.id, true, referenceFor(nav));
    return { content, nav, one, two, three, pageHeading, links:() => nav.querySelectorAll('.f-section-link') };
}
check('An outer guide section does not suppress the embedded content own outline', () => { const local = fixture({ embedded:true }); assert.equal(local.links().length, 3); assert.equal(local.nav.hidden, false); api.detach(local.nav); });
check('Guide discovery includes only five owned section headings and excludes control and modal titles', () => {
    const local = fixture(); local.content.append(section('Scenario', 1060).container, section('Source', 1360).container);
    const controlTitle = new Node('h2'); controlTitle.textContent = 'Embedded non-section control'; local.one.container.append(controlTitle);
    for (const kind of ['closed', 'open', 'dialog-role', 'alertdialog-role']) {
        const modal = new Node(kind.includes('role') ? 'div' : 'dialog');
        if (kind === 'open') modal.setAttribute('open', '');
        if (kind.includes('role')) modal.setAttribute('role', kind === 'dialog-role' ? 'dialog' : 'alertdialog');
        const heading = new Node('h2'); heading.textContent = kind; modal.append(heading); local.content.append(modal);
    }
    api.synchronize(local.nav, local.content.id, true, referenceFor(local.nav)); assert.equal(local.links().length, 5);
    const directHeading = new Node('h2'); directHeading.textContent = 'Direct content heading'; local.content.append(directHeading);
    api.synchronize(local.nav, local.content.id, true, referenceFor(local.nav)); assert.equal(local.links().length, 6); api.detach(local.nav);
});
check('Sticky heading compaction settles with one content-only correction and updated current section', () => {
    const local = fixture({ sticky:true }); local.nav.emit('click', click(local.links()[1]));
    assert.deepEqual(local.content.scrolled, { top:174, behavior:'smooth' });
    local.pageHeading.rect.height = 104;
    advance(120);
    assert.deepEqual(local.content.scrolled, { top:244, behavior:'auto' });
    assert.equal(local.links()[1].getAttribute('aria-current'), 'location');
    assert.equal(local.content.scrollCalls.length, 2); assert.equal(timers.size, 0);
    advance(3000); assert.equal(local.content.scrollCalls.length, 2);
    for (const type of ['wheel', 'touchstart', 'pointerdown', 'keydown']) assert.equal(document.events.get(type).length, 0);
    api.detach(local.nav);
});
check('Every user interruption cancels delayed alignment without another scroll', () => {
    for (const type of ['wheel', 'touchstart', 'pointerdown', 'keydown']) {
        const local = fixture({ sticky:true }); local.nav.emit('click', click(local.links()[1]));
        local.pageHeading.rect.height = 104; document.emit(type, { key:'PageDown' }); advance(1500);
        assert.equal(local.content.scrollCalls.length, 1, type); assert.equal(timers.size, 0, type); api.detach(local.nav);
    }
});
check('Continuous scroll events have a bounded alignment deadline and disposal removes pending timers', () => {
    const local = fixture({ sticky:true }); local.nav.emit('click', click(local.links()[1])); local.pageHeading.rect.height = 104;
    for (let step = 0; step < 11; step++) { local.content.emit('scroll'); advance(90); }
    assert.equal(local.content.scrollCalls.length, 1); advance(10); assert.equal(local.content.scrollCalls.length, 2); assert.equal(timers.size, 0);
    local.nav.emit('click', click(local.links()[2])); assert.equal(timers.size, 2); const before = local.content.scrollCalls.length;
    api.detach(local.nav); advance(1500); assert.equal(local.content.scrollCalls.length, before); assert.equal(timers.size, 0);
    for (const type of ['wheel', 'touchstart', 'pointerdown', 'keydown']) assert.equal(document.events.get(type).length, 0);
});
// The top control shares the surface module but does not own the compact-title controller.
// Keep that unrelated import isolated while executing the real BackToTop functions below.
document.body = new Node('body');
let surfaceCode = await fs.readFile(new URL('../../src/Flourish.Blazor/Flourish.Blazor.Framework/wwwroot/patterns/surfaces.js', import.meta.url), 'utf8');
surfaceCode = surfaceCode.replace("import { updateCompactHeading, resetCompactHeading } from '../shell.js';", 'const updateCompactHeading=()=>{}; const resetCompactHeading=()=>{};');
const surfaceApi = await import('data:text/javascript;base64,' + Buffer.from(surfaceCode).toString('base64'));
const makeTop = () => { const control = new Node('span'), link = new Node('a'), icon = new Node('span'); link.append(icon); control.append(link); control.style.removeProperty = name => control.style.values.delete(name); return { control, link, icon }; };
const topRegion = new Node('main'); topRegion.id = 'top-report'; topRegion.setAttribute('tabindex', '-1');
const topAction = makeTop();
surfaceApi.synchronizeBackToTop(topAction.control, topRegion.id, 120);
check('BackToTop enhances the chosen region and hides before the exact configured threshold', () => {
    assert.equal(topAction.control.hidden, true); assert.equal(topAction.control.hasAttribute('data-enhanced'), true);
    topRegion.scrollTop=120; topRegion.emit('scroll'); assert.equal(topAction.control.hidden, false);
    assert.equal(topAction.control.style.values.get('--f-back-to-top-left'), '828px'); assert.equal(topAction.control.style.values.get('--f-back-to-top-top'), '528px');
});
check('Repeated BackToTop synchronization owns one listener and refreshes threshold without another controller', () => {
    surfaceApi.synchronizeBackToTop(topAction.control, topRegion.id, 200); assert.equal(topAction.control.hidden, true);
    assert.equal(topRegion.events.get('scroll').length, 1); assert.equal(topAction.control.events.get('click').length, 1);
    surfaceApi.synchronizeBackToTop(topAction.control, topRegion.id, 120);
});
check('BackToTop icon activation scrolls only the target region and returns focus without moving the page', () => {
    const activation=click(topAction.icon); topAction.control.emit('click', activation);
    assert.equal(activation.prevented, true); assert.deepEqual(topRegion.scrolled, { top:0, behavior:'smooth' });
    assert.equal(document.activeElement, topRegion); assert.deepEqual(topRegion.focusOptions, { preventScroll:true });
    assert.equal(window.scrolled, undefined); assert.equal(topAction.control.hidden, true);
});
check('BackToTop reduced motion uses direct scrolling and modified clicks remain genuine fragment links', () => {
    reducedMotion=true; topRegion.scrollTop=400; topAction.control.emit('click', click(topAction.link)); assert.equal(topRegion.scrolled.behavior, 'auto');
    const modified={ ...click(topAction.link), metaKey:true }; topAction.control.emit('click', modified); assert.equal(modified.prevented, false); reducedMotion=false;
});
check('BackToTop stays inside the scroll region on viewport or geometry changes', () => {
    topRegion.scrollTop=300; topRegion.rect={left:500,right:1300,top:400,bottom:1000,height:600}; window.emit('resize');
    assert.equal(topAction.control.style.values.get('--f-back-to-top-left'), '928px'); assert.equal(topAction.control.style.values.get('--f-back-to-top-top'), '728px');
    topRegion.rect={left:500,right:1300,top:900,bottom:1500,height:600}; document.emit('scroll'); assert.equal(topAction.control.hidden, true);
});
const newTopRegion=new Node('main'); newTopRegion.id='new-top-report'; newTopRegion.scrollTop=300;
check('BackToTop target replacement releases the old listeners and selects the replacement region', () => {
    surfaceApi.synchronizeBackToTop(topAction.control, newTopRegion.id, 120);
    assert.equal(topRegion.events.get('scroll').length, 0); assert.equal(newTopRegion.events.get('scroll').length, 1);
    topAction.control.emit('click', click(topAction.link)); assert.equal(newTopRegion.scrollTop, 0);
});
check('A missing BackToTop region has no stale actions or retained listeners', () => {
    surfaceApi.synchronizeBackToTop(topAction.control, 'missing-top-report', 120);
    assert.equal(topAction.control.hidden, true); assert.equal(newTopRegion.events.get('scroll').length, 0); assert.equal(topAction.control.events.get('click').length, 0);
});
check('BackToTop disposal restores its SSR fragment fallback and removes all enhancement state', () => {
    surfaceApi.synchronizeBackToTop(topAction.control, newTopRegion.id, 120); surfaceApi.disposeBackToTop(topAction.control); surfaceApi.disposeBackToTop(topAction.control);
    assert.equal(topAction.control.hasAttribute('data-enhanced'), false); assert.equal(topAction.control.hidden, false);
    assert.equal(topAction.control.style.values.has('--f-back-to-top-left'), false); assert.equal(newTopRegion.events.get('scroll').length, 0);
    assert.equal(window.events.get('resize').length, 0); assert.equal(document.events.get('scroll').length, 0);
});
check('A removed BackToTop node releases listeners through the existing removal observer', () => {
    const removed=makeTop(); surfaceApi.synchronizeBackToTop(removed.control, newTopRegion.id, 120); removed.control.isConnected=false;
    Observer.all.at(-2).notify(); assert.equal(newTopRegion.events.get('scroll').length, 0); assert.equal(removed.control.events.get('click').length, 0);
});
const topCss = await fs.readFile(new URL('../../src/Flourish.Blazor/Flourish.Blazor.Framework/wwwroot/back-to-top.css', import.meta.url), 'utf8');
check('BackToTop geometry centers the actual standard icon button without retaining the old 72px raw skin', () => {
    const css=topCss; assert.match(css, /\.f-back-to-top > \.f-button\s*\{[^}]*align-items:center;[^}]*justify-content:center;[^}]*inline-size:48px;[^}]*block-size:48px;/);
    assert.match(css, /\.f-back-to-top \.f-icon\s*\{[^}]*inline-size:1em;[^}]*block-size:1em;[^}]*line-height:1;/);
    assert.doesNotMatch(css, /72px|\.app-icon|background:/);
});
process.stdout.write(`${count}/${count} section navigator and top control checks passed\n`);
