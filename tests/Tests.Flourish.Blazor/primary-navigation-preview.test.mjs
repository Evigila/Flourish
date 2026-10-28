import assert from 'node:assert/strict';
import { readFile } from 'node:fs/promises';
import test from 'node:test';
const framework = new URL('../../src/Flourish.Blazor/Flourish.Blazor.Framework/wwwroot/', import.meta.url);
const url = text => `data:text/javascript;base64,${Buffer.from(text).toString('base64')}`;
const tooltipUrl = url(await readFile(new URL('primitives/primary-navigation-item.js', framework), 'utf8'));
const tooltip = await import(tooltipUrl);
const source = (await readFile(new URL('patterns/primary-navigation-preview.js', framework), 'utf8'))
    .replace("from '../primitives/primary-navigation-item.js'", `from '${tooltipUrl}'`);
const api = await import(url(source));
const previewCss = await readFile(new URL('patterns/primary-navigation-preview.css', framework), 'utf8');
const rootBox = new Map([...previewCss.match(/\.f-primary-preview-slot > \[data-f-primary-preview-open\] \{([\s\S]*?)\n\}/)[1]
    .matchAll(/([a-z-]+)\s*:\s*([^;]+);/g)].map(match => [match[1], match[2].trim()]));

class Node {
    constructor(classes = [], rect = { left:0, right:76, top:68, bottom:668, width:76, height:600 }) {
        this.classes = new Set(classes); this.rect = rect; this.attributes = new Map(); this.children = [];
        this.events = new Map(); this.properties = new Map(); this.isConnected = true; this.scrollTop = 0;
        this.direction = 'ltr'; this.focusCalls = []; this.tag = 'div';
        this.style = { setProperty:(key, value) => this.properties.set(key, value),
            getPropertyValue:key => this.properties.get(key) ?? '', removeProperty:key => this.properties.delete(key) };
    }
    append(...children) { for (const child of children) { child.parent = this; this.children.push(child); } return this; }
    setAttribute(key, value) { this.attributes.set(key, value); }
    getAttribute(key) { return this.attributes.get(key) ?? null; }
    removeAttribute(key) { this.attributes.delete(key); }
    hasAttribute(key) { return this.attributes.has(key); }
    matches(selector) {
        return selector.split(',').some(part => {
            if (part === ':popover-open') return !!this.popoverOpen;
            if (part === ':focus-visible') return !!this.focusVisible;
            if (part.startsWith('.')) return this.classes.has(part.slice(1));
            if (part.startsWith('[')) return this.hasAttribute(part.slice(1, -1));
            return part === this.tag;
        });
    }
    closest(selector) { return this.matches(selector) ? this : this.parent?.closest(selector) ?? null; }
    contains(node) { return node === this || this.children.some(child => child.contains(node)); }
    querySelectorAll(selector) { return this.children.flatMap(child => [...(child.matches(selector) ? [child] : []), ...child.querySelectorAll(selector)]); }
    querySelector(selector) { return this.querySelectorAll(selector)[0] ?? null; }
    getBoundingClientRect() { return this.rect; }
    addEventListener(type, handler) { if (!this.events.has(type)) this.events.set(type, new Set()); this.events.get(type).add(handler); }
    removeEventListener(type, handler) { this.events.get(type)?.delete(handler); }
    emit(type, values = {}) {
        const event = { target:this, pointerType:'mouse', prevented:false, stopped:false,
            preventDefault() { this.prevented = true; }, stopPropagation() { this.stopped = true; }, ...values };
        for (const handler of [...this.events.get(type) ?? []]) handler(event);
        return event;
    }
    showPopover() { if (this.failOpen) throw new Error('unsupported'); this.popoverOpen = true; }
    hidePopover() { this.popoverOpen = false; if (this.contains(document.activeElement)) document.activeElement = document.body; }
    focus(options) { this.focusCalls.push(options); document.activeElement = this; }
}
function fixture(legacy = false, rtl = false) {
    const keys = ['document','window','getComputedStyle','ResizeObserver','MutationObserver','requestAnimationFrame','cancelAnimationFrame','setTimeout','clearTimeout'];
    const previous = new Map(keys.map(key => [key, globalThis[key]]));
    const document = new Node(), window = new Node(), body = new Node();
    document.body = body; document.activeElement = body; document.documentElement = { clientWidth:1280 };
    window.innerHeight = 800;
    const observers = [], resizes = [], frames = new Map(), timers = new Map(); let sequence = 0;
    Object.assign(globalThis, { document, window, getComputedStyle:node => ({ direction:node.direction }),
        ResizeObserver:class { constructor(callback) { this.callback = callback; this.observed = new Set(); resizes.push(this); } observe(node) { this.observed.add(node); } disconnect() { this.disconnected = true; } },
        MutationObserver:class { constructor(callback) { this.callback = callback; observers.push(this); } observe(node, options) { this.options = options; } disconnect() { this.disconnected = true; } },
        requestAnimationFrame:callback => { frames.set(++sequence, callback); return sequence; }, cancelAnimationFrame:id => frames.delete(id),
        setTimeout:callback => { timers.set(++sequence, callback); return sequence; }, clearTimeout:id => timers.delete(id) });
    const host = new Node([legacy ? 'f-shell' : 'navigation-surface'], { left:0, right:1280, top:0, bottom:800, width:1280, height:800 });
    const slot = new Node(['f-primary-preview-slot'], rtl ? { left:1204, right:1280, top:68, bottom:768, width:76, height:700 } : { left:0, right:76, top:68, bottom:768, width:76, height:700 });
    slot.setAttribute('data-f-primary-preview-slot', '');
    const rail = new Node([legacy ? 'f-primary-navigation' : 'primary-rail'], { ...slot.rect });
    rail.direction = rtl ? 'rtl' : 'ltr'; rail.setAttribute('data-f-primary-preview', '');
    const main = legacy ? new Node(['f-primary-navigation-main']) : rail;
    const fixed = legacy ? new Node(['f-primary-navigation-fixed']) : null;
    const first = new Node([legacy ? 'f-primary-item' : 'primary-nav-item'], { left:rtl ? 1214 : 10, right:rtl ? 1269 : 65, top:82, bottom:137, width:55, height:55 });
    first.tag = 'a'; first.setAttribute('href', '/same-real-link'); first.setAttribute('aria-label', 'Registry');
    const copy = new Node(['f-primary-preview-copy']); copy.setAttribute('aria-hidden', 'true'); first.append(copy);
    const last = new Node([legacy ? 'f-primary-item' : 'primary-nav-item']); last.tag = 'button';
    if (legacy) { rail.append(main, fixed); main.append(first); fixed.append(last); } else rail.append(first, last);
    slot.append(rail); host.append(slot); body.append(host);
    const flush = () => { const pending = [...frames.values()]; frames.clear(); pending.forEach(callback => callback()); };
    return { document, window, host, slot, rail, first, last, main, fixed, observers, resizes, frames, timers, flush,
        timerFlush() { const pending = [...timers.values()]; timers.clear(); pending.forEach(callback => callback()); },
        cleanup() { api.disposePrimaryNavigationPreviews(host); for (const [key, value] of previous) { if (value === undefined) delete globalThis[key]; else globalThis[key] = value; } } };
}
for (const legacy of [false, true]) test(`${legacy ? 'configured shell' : 'composed surface'} previews the same links and scroll owners above a stable narrow slot`, () => {
    const f = fixture(legacy);
    try {
        f.main.scrollTop = 137;
        const original = f.rail.children.slice(), href = f.first.getAttribute('href');
        api.initializePrimaryNavigationPreviews(f.host);
        api.initializePrimaryNavigationPreviews(f.host);
        assert.equal(f.rail.events.get('pointerenter').size, 1);
        f.rail.emit('pointerenter');
        assert.equal(f.rail.popoverOpen, true); assert.equal(f.rail.getAttribute('popover'), 'manual');
        assert.equal(f.slot.style.getPropertyValue('--f-primary-preview-reserved-width'), '76px');
        assert.equal(f.rail.style.getPropertyValue('--f-primary-preview-target-width'), '55px');
        assert.equal(f.main.scrollTop, 137); assert.deepEqual(f.rail.children, original);
        assert.equal(f.first.getAttribute('href'), href); assert.equal(f.document.activeElement, f.document.body);
        f.rail.emit('pointerleave', { relatedTarget:f.first });
        assert.equal(f.rail.popoverOpen, true, 'moving into the outer preview region is still inside the same rail');
        f.main.scrollTop = 231;
        f.document.emit('pointerdown', { target:f.first });
        f.document.activeElement = f.first;
        f.rail.emit('focusin', { target:f.first });
        f.rail.emit('pointerleave', { relatedTarget:null });
        assert.equal(f.rail.popoverOpen, false, 'mouse-click focus cannot pin the preview');
        assert.equal(f.document.activeElement, f.first, 'closing preserves the same real link focus');
        assert.equal(f.main.scrollTop, 231); assert.deepEqual(f.rail.children, original);
        assert.equal(f.slot.hasAttribute('data-f-primary-preview-active'), false);
        assert.equal(f.rail.hasAttribute('popover'), false);
        assert.equal(f.rail.style.getPropertyValue('--f-primary-preview-height'), '');
    } finally { f.cleanup(); }
});
test('touch never auto-expands while keyboard focus previews and Escape preserves the link without trapping Tab', async () => {
    const f = fixture();
    try {
        api.initializePrimaryNavigationPreviews(f.host);
        f.rail.emit('pointerenter', { pointerType:'touch' }); assert.equal(f.rail.popoverOpen, undefined);
        f.first.focusVisible = true; f.document.activeElement = f.first;
        f.rail.emit('focusin', { target:f.first }); assert.equal(f.rail.popoverOpen, true);
        const escape = f.document.emit('keydown', { key:'Escape', target:f.first });
        assert.equal(escape.prevented, true); assert.equal(escape.stopped, true); assert.equal(f.rail.popoverOpen, false);
        assert.equal(f.document.activeElement, f.first);
        f.rail.emit('focusin', { target:f.first }); assert.equal(f.rail.popoverOpen, false, 'Escape suppression remains until the next deliberate interaction');
        const tab = f.document.emit('keydown', { key:'Tab' }); assert.equal(tab.prevented, false);
        f.rail.emit('focusin', { target:f.first }); assert.equal(f.rail.popoverOpen, true);
        f.document.activeElement = f.document.body; f.rail.emit('focusout'); await Promise.resolve();
        assert.equal(f.rail.popoverOpen, false);
    } finally { f.cleanup(); }
});
test('hover-only Escape closes weak preview but reaches the outside editor or foreground dialog unchanged', () => {
    const f = fixture();
    try {
        api.initializePrimaryNavigationPreviews(f.host);
        f.rail.emit('pointerenter');
        const outside = new Node(); f.document.body.append(outside); f.document.activeElement = outside;
        const escape = f.document.emit('keydown', { key:'Escape', target:outside });
        assert.equal(f.rail.popoverOpen, false); assert.equal(escape.prevented, false); assert.equal(escape.stopped, false);
        assert.equal(f.document.activeElement, outside); assert.equal(outside.focusCalls.length, 0);
    } finally { f.cleanup(); }
});
test('RTL and clipped embedded owners bound the top-layer rail while preserving the visible scroll position', () => {
    const f = fixture(false, true);
    try {
        f.host.rect = { left:200, right:1280, top:-100, bottom:600, width:1080, height:700 };
        f.slot.rect.top = f.rail.rect.top = -40; f.slot.rect.bottom = f.rail.rect.bottom = 600;
        f.main.scrollTop = 90;
        api.initializePrimaryNavigationPreviews(f.host); f.rail.emit('pointerenter');
        assert.equal(f.rail.style.getPropertyValue('--f-primary-preview-left'), 'auto');
        assert.equal(f.rail.style.getPropertyValue('--f-primary-preview-right'), '0px');
        assert.equal(f.rail.style.getPropertyValue('--f-primary-preview-available'), '1080px');
        assert.equal(f.rail.style.getPropertyValue('--f-primary-preview-top'), '0px');
        assert.equal(f.rail.style.getPropertyValue('--f-primary-preview-height'), '600px');
        assert.equal(f.main.scrollTop, 130, 'the 40px viewport clip must not reveal earlier hidden rows');
        f.main.scrollTop += 20;
        f.rail.emit('pointerleave', { relatedTarget:null }); assert.equal(f.main.scrollTop, 110);
    } finally { f.cleanup(); }
});
test('resize, disabled opt-out, DOM removal and disposal release the reservation and every subscription/frame', () => {
    const f = fixture();
    try {
        api.initializePrimaryNavigationPreviews(f.host); f.rail.emit('pointerenter');
        f.window.emit('resize'); assert.equal(f.rail.popoverOpen, false);
        assert.equal(f.slot.hasAttribute('data-f-primary-preview-active'), false);
        f.rail.emit('pointerleave'); f.rail.emit('pointerenter'); assert.equal(f.rail.popoverOpen, true);
        f.document.emit('scroll', { target:f.document.body }); assert.equal(f.frames.size, 1);
        f.rail.removeAttribute('data-f-primary-preview');
        f.observers.find(observer => observer.options?.attributeFilter)?.callback();
        assert.equal(f.rail.popoverOpen, false); assert.equal(f.frames.size, 0);
        assert.equal(f.resizes[0].disconnected, true); assert.equal(f.rail.events.get('pointerenter').size, 0);
        f.rail.setAttribute('data-f-primary-preview', ''); api.initializePrimaryNavigationPreviews(f.host); f.rail.emit('pointerenter');
        f.rail.isConnected = false;
        f.observers.at(-1).callback();
        assert.equal(f.rail.hasAttribute('popover'), false); assert.equal(f.slot.properties.size, 0);
        assert.equal(f.rail.events.get('pointerenter').size, 0);
    } finally { f.cleanup(); }
});
test('ordinary tooltip is suppressed by preview but custom disabled reason survives and replaced triggers rebind once', () => {
    const f = fixture(); const container = new Node(['primary-navigation-entry']), tip = new Node(['primary-navigation-tooltip'], { left:0,right:160,top:0,bottom:30,width:160,height:30 });
    const old = f.first; f.rail.children = []; container.append(old, tip); f.rail.append(container);
    old.setAttribute('data-navigation-trigger',''); tip.setAttribute('data-navigation-tooltip','');
    try {
        api.initializePrimaryNavigationPreviews(f.host); tooltip.attach(container);
        f.rail.emit('pointerenter'); old.emit('focus'); f.timerFlush(); assert.equal(tip.popoverOpen, undefined);
        const disabled = new Node(['primary-nav-item']); disabled.setAttribute('data-navigation-trigger',''); disabled.setAttribute('aria-disabled','true');
        old.isConnected = false; container.children = []; container.append(disabled, tip);
        tooltip.attach(container); tooltip.attach(container);
        assert.equal(old.events.get('pointerenter').size, 0); assert.equal(disabled.events.get('pointerenter').size, 1);
        disabled.emit('focus'); f.timerFlush(); assert.equal(tip.popoverOpen, true, 'disabled custom descriptions remain available in the expanded rail');
    } finally { tooltip.detach(container); f.cleanup(); }
});
test('native top-layer failure leaves original navigation and scroll usable without owned styles', () => {
    const f = fixture();
    try {
        f.rail.failOpen = true; f.main.scrollTop = 77;
        f.slot.rect.top = f.rail.rect.top = -40;
        api.initializePrimaryNavigationPreviews(f.host); f.rail.emit('pointerenter');
        assert.equal(f.rail.hasAttribute('popover'), false); assert.equal(f.rail.hasAttribute('data-f-primary-preview-open'), false);
        assert.equal(f.main.scrollTop, 77); assert.equal(f.slot.properties.size, 0); assert.equal(f.first.getAttribute('href'), '/same-real-link');
    } finally { f.cleanup(); }
});
test('a hidden first entry does not prevent previewing subsequent visible links', () => {
    const f = fixture();
    try {
        const hidden = new Node(['primary-nav-item'], { width:0, height:0 });
        f.rail.children.unshift(hidden); hidden.parent = f.rail;
        api.initializePrimaryNavigationPreviews(f.host); f.rail.emit('pointerenter');
        assert.equal(f.rail.popoverOpen, true); assert.equal(f.rail.style.getPropertyValue('--f-primary-preview-target-width'), '55px');
    } finally { f.cleanup(); }
});
test('resize before first use and window blur do not swallow the next real rail entry', () => {
    const f = fixture();
    try {
        api.initializePrimaryNavigationPreviews(f.host);
        f.window.emit('resize'); f.rail.emit('pointerenter'); assert.equal(f.rail.popoverOpen, true);
        f.window.emit('blur'); assert.equal(f.rail.popoverOpen, false);
        f.rail.emit('pointerenter'); assert.equal(f.rail.popoverOpen, true);
    } finally { f.cleanup(); }
});

test('top-layer UA padding and borders are neutralized only while open, with captured block insets and per-item icon placement', () => {
    assert.equal(rootBox.get('box-sizing'), 'border-box');
    assert.equal(rootBox.get('border'), '0');
    assert.equal(rootBox.get('padding'), 'var(--f-primary-preview-padding-top,0px) 0 var(--f-primary-preview-padding-bottom,0px)');
    assert.match(previewCss, /margin-inline-start:var\(--f-primary-preview-icon-inline-inset,0px\)/);
    assert.match(previewCss, /margin-block-start:var\(--f-primary-preview-icon-block-inset,0px\)/);
    assert.match(previewCss, /height:var\(--f-primary-preview-row-height,var\(--f-primary-preview-target-height\)\)/);
});

for (const legacy of [false, true]) for (const rtl of [false, true]) for (const gutter of [0, 17])
test(`${legacy ? 'shell' : 'surface'} ${rtl ? 'RTL' : 'LTR'} icons retain real route, disabled and command coordinates with ${gutter}px inline scrollbar`, () => {
    const f = fixture(legacy, rtl);
    try {
        const disabled = new Node([legacy ? 'f-primary-item' : 'primary-nav-item']);
        disabled.setAttribute('aria-disabled', 'true');
        if (legacy) f.main.append(disabled);
        else { f.rail.children = [f.first, disabled, f.last]; disabled.parent = f.rail; }
        const rows = [f.first, disabled, f.last], heights = [55, 47, 48], widths = [55, 45, 51];
        const blockPadding = legacy ? 14 : 0, borderTop = 2, borderBottom = 3;
        const originalInline = 10 + gutter + (rtl && !legacy ? 1 : 0);
        const originalStart = rtl ? f.rail.rect.right - originalInline : f.rail.rect.left + originalInline;
        f.main.scrollTop = 91;
        globalThis.getComputedStyle = node => ({ direction:node.direction, paddingTop:`${blockPadding}px`, paddingBottom:`${blockPadding}px`,
            borderTopWidth:`${borderTop}px`, borderBottomWidth:`${borderBottom}px` });
        const number = (element, name) => Number.parseFloat(element.style.getPropertyValue(`--f-primary-preview-${name}`)) || 0;
        const resolve = value => value.startsWith('var(')
            ? Number.parseFloat(f.rail.style.getPropertyValue(value.slice(4, value.indexOf(',')))) || 0 : Number.parseFloat(value) || 0;
        const rootPadding = () => rootBox.has('padding') ? rootBox.get('padding').split(' ').map(resolve) : [4.25, 4.25, 4.25];
        let flow = 0;
        const originals = rows.map((row, index) => {
            const inset = (55 - widths[index]) / 2;
            const left = rtl ? originalStart - inset - widths[index] : originalStart + inset;
            const top = legacy && index === 2 ? f.rail.rect.bottom - blockPadding - borderBottom - heights[index]
                : f.rail.rect.top + blockPadding + borderTop + (legacy ? 0 : 14) - f.main.scrollTop + flow;
            flow += heights[index] + 8;
            row.rect = { left, right:left + widths[index], top, bottom:top + heights[index], width:widths[index], height:heights[index] };
            const icon = new Node([legacy ? 'f-icon' : 'primary-nav-symbol']);
            const iconWidth = [24, 20, 26][index], inlineInset = [15.5, 7.25, 11.75][index], blockInset = [15.5, 9, 12.25][index];
            const iconLeft = rtl ? row.rect.right - inlineInset - iconWidth : row.rect.left + inlineInset;
            icon.rect = { left:iconLeft, right:iconLeft + iconWidth, top:top + blockInset, bottom:top + blockInset + 24, width:iconWidth, height:24 };
            row.append(icon);
            return { row, icon, box:{ ...row.rect }, iconBox:{ ...icon.rect } };
        });
        const expandedRow = index => {
            const [paddingTop, , paddingBottom] = rootPadding();
            const inset = number(f.rail, 'inline-inset') + gutter;
            const left = rtl ? f.rail.rect.right - inset - 230 : f.rail.rect.left + inset;
            const preceding = rows.slice(0,index).reduce((sum,row) => sum + number(row,'row-height') + 8,0);
            const top = legacy && index === 2 ? f.rail.rect.bottom - paddingBottom - heights[index]
                : f.rail.rect.top + paddingTop + (legacy ? 0 : 14) - f.main.scrollTop + preceding;
            return { left, right:left + 230, top, bottom:top + heights[index], width:230, height:heights[index] };
        };
        for (const [index, item] of originals.entries()) {
            item.row.getBoundingClientRect = () => f.rail.popoverOpen ? expandedRow(index) : item.box;
            item.icon.getBoundingClientRect = () => {
                if (!f.rail.popoverOpen) return item.iconBox;
                const box = expandedRow(index), inset = number(item.row,'icon-inline-inset'), top = box.top + number(item.row,'icon-block-inset');
                const left = rtl ? box.right - inset - item.iconBox.width : box.left + inset;
                return { left, right:left + item.iconBox.width, top, bottom:top + item.iconBox.height, width:item.iconBox.width, height:item.iconBox.height };
            };
        }
        api.initializePrimaryNavigationPreviews(f.host); f.rail.emit('pointerenter');
        assert.equal(f.rail.popoverOpen, true);
        assert.equal(number(f.rail,'padding-top'), blockPadding + borderTop);
        assert.equal(number(f.rail,'padding-bottom'), blockPadding + borderBottom);
        for (const item of originals) {
            assert.deepEqual(item.icon.getBoundingClientRect(), item.iconBox);
            assert.equal(number(item.row,'row-height'), item.box.height);
        }
        assert.equal(originals[0].row.getBoundingClientRect().top, originals[0].box.top, 'row origin must not inherit native popover padding');
        f.rail.emit('pointerleave');
        assert.equal(f.main.scrollTop,91);
        for (const item of originals) assert.equal(item.row.properties.size,0);
        assert.equal(f.rail.properties.size,0);
    } finally { f.cleanup(); }
});

test('open rerenders compare all node identities without layout reads, keep existing hidden items, and release replaced trigger geometry', () => {
    const f = fixture();
    try {
        const hidden = new Node(['primary-nav-item'], { width:0,height:0 });
        hidden.parent = f.rail; f.rail.children.unshift(hidden);
        f.main.scrollTop = 77;
        api.initializePrimaryNavigationPreviews(f.host); f.rail.emit('pointerenter');
        const prune = f.observers.find(observer => observer.options?.attributeFilter).callback;
        const oldRect = f.first.getBoundingClientRect;
        f.first.getBoundingClientRect = () => { throw new Error('an unrelated mutation must not force layout'); };
        prune([{ type:'childList', target:f.first.children[0] }]);
        assert.equal(f.rail.popoverOpen,true);
        f.first.getBoundingClientRect = oldRect;
        const replaced = new Node(['primary-nav-item'], { ...f.first.rect }); replaced.setAttribute('aria-disabled','true'); replaced.parent = f.rail;
        f.rail.children = f.rail.children.map(item => item === f.first ? replaced : item);
        prune([{ type:'childList', target:f.rail }]);
        assert.equal(f.rail.popoverOpen,false); assert.equal(f.main.scrollTop,77);
        assert.equal(f.first.properties.size,0); assert.equal(f.last.properties.size,0);
        f.rail.emit('pointerleave'); f.rail.emit('pointerenter'); assert.equal(f.rail.popoverOpen,true);
        prune([{ type:'attributes', attributeName:'hidden', target:replaced }]);
        assert.equal(f.rail.popoverOpen,false);
    } finally { f.cleanup(); }
});
