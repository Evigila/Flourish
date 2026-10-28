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
    showPopover() { if (this.failOpen) throw new Error('unsupported'); this.popoverOpen = true; this.showCount = (this.showCount ?? 0) + 1; }
    hidePopover() { this.popoverOpen = false; this.hideCount = (this.hideCount ?? 0) + 1; if (this.contains(document.activeElement)) document.activeElement = document.body; }
    focus(options) { this.focusCalls.push(options); document.activeElement = this; }
}
function fixture(legacy = false, rtl = false, reducedMotion = true) {
    const keys = ['document','window','getComputedStyle','ResizeObserver','MutationObserver','requestAnimationFrame','cancelAnimationFrame','setTimeout','clearTimeout','matchMedia'];
    const previous = new Map(keys.map(key => [key, globalThis[key]]));
    const document = new Node(), window = new Node(), body = new Node();
    document.body = body; document.activeElement = body; document.documentElement = { clientWidth:1280 };
    window.innerHeight = 800;
    const observers = [], resizes = [], frames = new Map(), timers = new Map(), timerDelays = new Map(); let sequence = 0;
    const motion = new Node(); motion.matches = reducedMotion;
    Object.assign(globalThis, { document, window, matchMedia:() => motion,
        getComputedStyle:node => ({ direction:node.direction, getPropertyValue:property => property === '--f-primary-preview-duration' ? '180ms' : '' }),
        ResizeObserver:class { constructor(callback) { this.callback = callback; this.observed = new Set(); resizes.push(this); } observe(node) { this.observed.add(node); } unobserve(node) { this.observed.delete(node); } disconnect() { this.disconnected = true; } },
        MutationObserver:class { constructor(callback) { this.callback = callback; observers.push(this); } observe(node, options) { this.options = options; } disconnect() { this.disconnected = true; } },
        requestAnimationFrame:callback => { frames.set(++sequence, callback); return sequence; }, cancelAnimationFrame:id => frames.delete(id),
        setTimeout:(callback, delay) => { timers.set(++sequence, callback); timerDelays.set(sequence, delay); return sequence; },
        clearTimeout:id => { timers.delete(id); timerDelays.delete(id); } });
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
    return { document, window, host, slot, rail, first, last, main, fixed, observers, resizes, frames, timers, timerDelays, motion, flush,
        timerFlush() { const pending = [...timers.values()]; timers.clear(); timerDelays.clear(); pending.forEach(callback => callback()); },
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

function motionGeometry(f) {
    const railBox = { ...f.rail.rect }, rowBox = { ...f.first.rect }, rtl = f.rail.direction === 'rtl';
    const icon = new Node(['primary-nav-symbol']);
    const iconBox = { left:rtl ? rowBox.right - 39.5 : rowBox.left + 15.5, top:rowBox.top + 15.5, width:24, height:24 };
    iconBox.right = iconBox.left + 24; iconBox.bottom = iconBox.top + 24;
    f.first.children.unshift(icon); icon.parent = f.first;
    const number = (element, name) => Number.parseFloat(element.style.getPropertyValue('--f-primary-preview-' + name)) || 0;
    f.rail.layoutReads = 0;
    f.rail.getBoundingClientRect = () => {
        f.rail.layoutReads++;
        if (!f.rail.popoverOpen) return railBox;
        const cap = Math.min(288, number(f.rail, 'available'), number(f.rail, 'secondary-limit'));
        const phase = f.rail.getAttribute('data-f-primary-preview-phase');
        const target = phase === 'collapsed' ? railBox.width : number(f.rail,'expanded-width') || Math.min(320,cap);
        const width = Math.min(f.rail.usedWidth ?? target, cap), left = rtl ? railBox.right - width : railBox.left;
        return { ...railBox, left, right:left + width, width };
    };
    f.first.getBoundingClientRect = () => {
        if (!f.rail.popoverOpen) return rowBox;
        const rail = f.rail.getBoundingClientRect(), inset = number(f.rail, 'inline-inset');
        return { ...rowBox, left:rail.left + inset, right:rail.right - inset, width:rail.width - inset * 2 };
    };
    icon.getBoundingClientRect = () => {
        if (!f.rail.popoverOpen) return iconBox;
        const row = f.first.getBoundingClientRect(), inset = number(f.first, 'icon-inline-inset');
        const left = rtl ? row.right - inset - 24 : row.left + inset, top = row.top + number(f.first, 'icon-block-inset');
        return { left, right:left + 24, top, bottom:top + 24, width:24, height:24 };
    };
    return { icon, iconBox };
}
for (const rtl of [false,true]) test(`actual ${rtl ? 'RTL' : 'LTR'} secondary geometry keeps a quarter exposed without reading its labels`, () => {
    const f = fixture(false,rtl);
    try {
        const secondary = new Node(['secondary-rail'], { left:rtl ? 972 : 76, right:rtl ? 1204 : 308, width:232, top:68,bottom:768,height:700 });
        secondary.append(new Node(['long-label'], { width:9000,height:24 })); f.host.append(secondary);
        api.initializePrimaryNavigationPreviews(f.host); f.rail.emit('pointerenter');
        assert.equal(f.rail.style.getPropertyValue('--f-primary-preview-secondary-limit'),'250px');
        assert.ok(f.resizes[0].observed.has(secondary));
        assert.equal(f.slot.style.getPropertyValue('--f-primary-preview-reserved-width'),'76px');
    } finally { f.cleanup(); }
});
for (const rtl of [false,true]) test(`${rtl ? 'RTL' : 'LTR'} narrow embedded roots preserve a quarter of the secondary portion that is actually visible`, () => {
    const f = fixture(false,rtl);
    try {
        f.host.rect.left = rtl ? 1080 : 0; f.host.rect.right = rtl ? 1280 : 200; f.host.rect.width = 200;
        const secondary = new Node(['secondary-rail'], { left:rtl ? 972 : 76,right:rtl ? 1204 : 308,width:232,top:68,bottom:768,height:700 }); f.host.append(secondary);
        api.initializePrimaryNavigationPreviews(f.host); f.rail.emit('pointerenter');
        assert.equal(f.rail.style.getPropertyValue('--f-primary-preview-available'),'200px');
        assert.equal(f.rail.style.getPropertyValue('--f-primary-preview-secondary-limit'),'169px', '124 visible secondary pixels retain their 31px strip');
    } finally { f.cleanup(); }
});
test('secondary width and replacement refresh the cap during opening or open without changing host or slot size', () => {
    const f = fixture(false,false,false);
    try {
        const secondary = new Node(['secondary-rail'], { left:76,right:308,width:232,top:68,bottom:768,height:700 }); f.host.append(secondary);
        motionGeometry(f); api.initializePrimaryNavigationPreviews(f.host); f.rail.emit('pointerenter');
        assert.equal(f.rail.style.getPropertyValue('--f-primary-preview-secondary-limit'),'250px');
        secondary.rect.right = 176; secondary.rect.width = 100;
        f.resizes[0].callback(); f.flush();
        assert.equal(f.rail.style.getPropertyValue('--f-primary-preview-secondary-limit'),'151px');
        assert.equal(f.rail.getBoundingClientRect().width,151);
        const replacement = new Node(['secondary-rail'], { left:76,right:276,width:200,top:68,bottom:768,height:700 }); replacement.parent = f.host;
        f.host.children = f.host.children.map(node => node === secondary ? replacement : node);
        const rect = replacement.getBoundingClientRect; replacement.getBoundingClientRect = () => { throw new Error('mutation must defer layout to its frame'); };
        f.observers.find(observer => observer.options?.attributeFilter).callback();
        assert.equal(f.resizes[0].observed.has(secondary),false); assert.equal(f.resizes[0].observed.has(replacement),true);
        replacement.getBoundingClientRect = rect; f.flush();
        assert.equal(f.rail.style.getPropertyValue('--f-primary-preview-secondary-limit'),'226px');
    } finally { f.cleanup(); }
});
test('horizontal, missing and narrow-viewport secondary layouts use the ordinary viewport bound', () => {
    const f = fixture();
    try {
        const secondary = new Node(['secondary-rail'], { left:76,right:1280,width:1204,top:68,bottom:126,height:58 }); f.host.append(secondary);
        f.document.documentElement.clientWidth = 210;
        api.initializePrimaryNavigationPreviews(f.host); f.rail.emit('pointerenter');
        assert.equal(f.rail.style.getPropertyValue('--f-primary-preview-secondary-limit'),'210px');
        assert.equal(f.rail.style.getPropertyValue('--f-primary-preview-available'),'210px');
        f.rail.emit('pointerleave'); f.host.children = f.host.children.filter(node => node !== secondary);
        f.rail.emit('pointerenter'); assert.equal(f.rail.style.getPropertyValue('--f-primary-preview-secondary-limit'),'210px');
    } finally { f.cleanup(); }
});
for (const rtl of [false,true]) test(`${rtl ? 'RTL' : 'LTR'} width motion never scales or moves the actual icon and hides only after collapse`, () => {
    const f = fixture(false,rtl,false);
    try {
        const { icon, iconBox } = motionGeometry(f); f.main.scrollTop = 127;
        api.initializePrimaryNavigationPreviews(f.host); f.rail.emit('pointerenter');
        assert.equal(f.rail.getAttribute('data-f-primary-preview-phase'),'collapsed');
        assert.equal(f.rail.getBoundingClientRect().width,76); assert.equal(f.rail.popoverOpen,true);
        f.flush(); assert.equal(f.rail.getAttribute('data-f-primary-preview-phase'),'expanded');
        for (const width of [76,144,220,288]) { f.rail.usedWidth = width; assert.deepEqual(icon.getBoundingClientRect(),iconBox); }
        f.rail.emit('pointerleave'); assert.equal(f.rail.getAttribute('data-f-primary-preview-phase'),'collapsed');
        assert.equal(f.rail.popoverOpen,true); assert.equal(f.slot.hasAttribute('data-f-primary-preview-active'),true);
        assert.equal([...f.timerDelays.values()][0],230);
        for (const width of [288,190,110,76]) { f.rail.usedWidth = width; assert.deepEqual(icon.getBoundingClientRect(),iconBox); }
        f.rail.emit('transitionend',{ propertyName:'opacity',target:f.first.children[1] }); assert.equal(f.rail.popoverOpen,true);
        f.rail.emit('transitionend',{ propertyName:'inline-size' });
        assert.equal(f.rail.popoverOpen,false); assert.equal(f.main.scrollTop,127); assert.equal(f.rail.properties.size,0);
        assert.equal(f.timers.size,0); assert.deepEqual(icon.getBoundingClientRect(),iconBox);
    } finally { f.cleanup(); }
});
test('rapid reversal keeps the same top-layer node and rejects stale close timers and earlier expansion-end events', () => {
    const f = fixture(false,false,false);
    try {
        motionGeometry(f); api.initializePrimaryNavigationPreviews(f.host); f.rail.emit('pointerenter'); f.flush();
        f.rail.usedWidth = 190; f.rail.emit('pointerleave'); const stale = [...f.timers.values()][0];
        f.rail.emit('transitionend',{ propertyName:'inline-size' }); assert.equal(f.rail.popoverOpen,true,'an old expansion end is not a completed collapse');
        const reads = f.rail.layoutReads;
        f.rail.emit('pointerenter'); assert.equal(f.rail.getAttribute('data-f-primary-preview-phase'),'expanded');
        assert.equal(f.rail.layoutReads,reads,'reverse from current CSS width without recapturing expanded/normal DOM geometry');
        stale(); assert.equal(f.rail.popoverOpen,true); assert.equal(f.rail.showCount,1); assert.equal(f.rail.hideCount,undefined);
        assert.equal(f.timers.size,0);
        f.rail.emit('pointerleave'); f.rail.usedWidth = 76; f.timerFlush();
        assert.equal(f.rail.popoverOpen,false); assert.equal(f.rail.hideCount,1);
    } finally { f.cleanup(); }
});
test('pre-paint cancellation and later native closure cannot leave frames or a clickable stale popover', () => {
    const f = fixture(false,false,false);
    try {
        motionGeometry(f); api.initializePrimaryNavigationPreviews(f.host); f.rail.emit('pointerenter');
        const oldFrame = [...f.frames.values()][0];
        f.rail.emit('pointerleave'); assert.equal(f.rail.popoverOpen,false); assert.equal(f.frames.size,0);
        f.rail.emit('pointerenter'); const newFrameCount = f.frames.size;
        oldFrame(); assert.equal(f.frames.size,newFrameCount); f.flush();
        f.rail.popoverOpen = false; f.rail.emit('toggle',{ newState:'closed' });
        assert.equal(f.rail.hasAttribute('popover'),false); assert.equal(f.slot.properties.size,0); assert.equal(f.frames.size,0);
    } finally { f.cleanup(); }
});
test('reduced motion is immediate initially and immediately settles an active close when changed', () => {
    const f = fixture(false,false,false);
    try {
        motionGeometry(f); api.initializePrimaryNavigationPreviews(f.host); f.rail.emit('pointerenter'); f.flush();
        f.rail.emit('pointerleave'); assert.equal(f.timers.size,1);
        f.motion.matches = true; f.motion.emit('change');
        assert.equal(f.rail.popoverOpen,false); assert.equal(f.timers.size,0); assert.equal(f.frames.size,0);
        f.rail.emit('pointerenter'); assert.equal(f.rail.getAttribute('data-f-primary-preview-phase'),'expanded'); assert.equal(f.frames.size,0);
        f.rail.emit('pointerleave'); assert.equal(f.rail.popoverOpen,false); assert.equal(f.timers.size,0);
    } finally { f.cleanup(); }
});
for (const reason of ['resize','blur','opt-out','dispose']) test(`an animated close interrupted by ${reason} immediately releases geometry and every pending callback`, () => {
    const f = fixture(false,false,false);
    try {
        motionGeometry(f); api.initializePrimaryNavigationPreviews(f.host); f.rail.emit('pointerenter'); f.flush(); f.rail.emit('pointerleave');
        const stale = [...f.timers.values()][0];
        f.document.emit('scroll',{ target:f.document.body });
        if (reason === 'resize' || reason === 'blur') f.window.emit(reason);
        else if (reason === 'opt-out') { f.rail.removeAttribute('data-f-primary-preview'); f.observers.find(observer => observer.options?.attributeFilter).callback(); }
        else api.disposePrimaryNavigationPreviews(f.host);
        assert.equal(f.rail.popoverOpen,false); assert.equal(f.rail.properties.size,0); assert.equal(f.slot.properties.size,0);
        assert.equal(f.frames.size,0); assert.equal(f.timers.size,0);
        stale(); assert.equal(f.rail.properties.size,0); assert.equal(f.rail.hasAttribute('popover'),false);
    } finally { f.cleanup(); }
});
test('animated Escape preserves link focus after cleanup and never consumes an outside dialog key', () => {
    const f = fixture(false,false,false);
    try {
        motionGeometry(f); api.initializePrimaryNavigationPreviews(f.host); f.first.focusVisible = true; f.document.activeElement = f.first;
        f.rail.emit('focusin',{ target:f.first }); f.flush();
        const escape = f.document.emit('keydown',{ key:'Escape',target:f.first });
        assert.equal(escape.prevented,true); assert.equal(f.rail.popoverOpen,true); assert.equal(f.document.activeElement,f.first);
        f.rail.usedWidth = 76; f.timerFlush(); assert.equal(f.document.activeElement,f.first); assert.equal(f.rail.popoverOpen,false);
        f.rail.emit('pointerleave'); f.rail.emit('pointerenter'); f.flush();
        const dialog = new Node(); f.document.body.append(dialog); f.document.activeElement = dialog;
        const outside = f.document.emit('keydown',{ key:'Escape',target:dialog }); assert.equal(outside.prevented,false); assert.equal(outside.stopped,false);
        f.timerFlush(); assert.equal(f.document.activeElement,dialog); assert.equal(dialog.focusCalls.length,0);
    } finally { f.cleanup(); }
});
test('motion CSS preserves the narrow initial width, limits only the overlay and never introduces an icon scale', () => {
    assert.match(previewCss,/inline-size:var\(--f-primary-preview-reserved-width\)/);
    assert.match(previewCss,/min-inline-size:0;/);
    assert.match(previewCss,/transition:inline-size var\(--f-primary-preview-duration,180ms\)/);
    assert.match(previewCss,/@media \(prefers-reduced-motion:reduce\)/);
    assert.match(previewCss,/transition:none/);
    assert.doesNotMatch(previewCss,/scale(?:X|Y)?\(|transform:/);
});
