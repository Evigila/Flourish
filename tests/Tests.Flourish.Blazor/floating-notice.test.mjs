import assert from 'node:assert/strict';
import test from 'node:test';
import { readFile } from 'node:fs/promises';

const location = new URL('../../src/Flourish.Blazor/Flourish.Blazor.Framework/wwwroot/', import.meta.url);
const code = await readFile(new URL('patterns/floating-notice.js',location),'utf8');
const { initializeFloatingNotices } = await import('data:text/javascript;base64,' + Buffer.from(code).toString('base64'));

function fixture() {
    const frames = [], bars = [], observers = [], raf = new Map(), listeners = new Map();
    let sequence = 0, mutations = 0, now = 0;
    const timers = new Map();
    const events = {
        addEventListener(name,fn) { listeners.set(name,fn); },
        removeEventListener(name) { listeners.delete(name); }
    };
    class Observer {
        constructor(callback) { this.callback = callback; this.targets = new Set(); observers.push(this); }
        observe(node) { this.targets.add(node); }
        unobserve(node) { this.targets.delete(node); }
        disconnect() { this.targets.clear(); this.closed = true; }
    }
    const view = { ...events,innerHeight:800,ResizeObserver:Observer,MutationObserver:Observer,
        reduced:false, performance:{now:() => now},
        matchMedia() { return {matches:this.reduced}; },
        setTimeout(fn,ms) { const id = ++sequence; timers.set(id,{fn,at:now+ms}); return id; },
        clearTimeout(id) { timers.delete(id); },
        getComputedStyle:() => ({visibility:'visible',animationDuration:'0.18s'}),requestAnimationFrame(fn) { raf.set(++sequence,fn); return sequence; } };
    const doc = { ...events,defaultView:view,documentElement:{ clientWidth:1200 },
        querySelector:() => frames[0],querySelectorAll:selector => selector === '[data-f-notice-frame]' ? frames : selector.includes('.f-save-changes-panel') ? bars : [] };
    function node(rect,dialog = null) {
        const values = new Map(), attributes = new Map(), handlers = new Map();
        return { ownerDocument:doc,rect,hidden:false,isConnected:true,innerHTML:'message',className:'f-notice',
            matches:() => false,contains(other) { return !!other && (this === other || this.firstElementChild === other); },
            addEventListener(name,handler) { handlers.set(name,handler); },
            removeEventListener(name) { handlers.delete(name); },
            dispatch(name,event = {}) { handlers.get(name)?.(event); },
            handlers,style:{
            getPropertyValue:key => values.get(key) ?? '',
            setProperty(key,value) { values.set(key,value); mutations++; },
            removeProperty(key) { if (values.delete(key)) mutations++; }
        },getBoundingClientRect() { return this.rect; },getClientRects() { return this.hidden ? [] : [this.rect]; },
            closest(selector) { return selector === 'dialog' ? dialog : this.hidden ? this : null; },
            getAttribute:name => attributes.get(name) ?? null,
            hasAttribute:name => attributes.has(name),setAttribute(name,value = '') { attributes.set(name,value); mutations++; },
            removeAttribute(name) { if (attributes.delete(name)) mutations++; } };
    }
    function notice(height = 90,dialog = null) {
        const frame = node({left:88,right:1112,bottom:784},dialog), message = node({height},dialog);
        frame.firstElementChild = message; message.parentElement = frame; frames.push(frame);
        return frame;
    }
    function bar(dialog = null) {
        const panel = node({height:100},dialog); panel.parentElement = node({},dialog); bars.push(panel);
        return panel;
    }
    function flush() {
        observers.find(observer => !observer.closed && observer.targets.has(doc.documentElement))?.callback([]);
        assert.ok(raf.size <= 1);
        for (const [id,fn] of raf) { raf.delete(id); fn(); }
    }
    function advance(ms) {
        const until = now + ms;
        for (let count = 0; count < 100; count++) {
            const next = [...timers].sort((a,b) => a[1].at-b[1].at)[0];
            if (!next || next[1].at > until) { now = until; return; }
            now = next[1].at; timers.delete(next[0]); next[1].fn();
        }
        assert.fail('timer callbacks did not settle');
    }
    return { doc, frames, bars, bar, notice, flush, advance, timers, observers, listeners, mutations:() => mutations };
}
const value = (node,key) => parseFloat(node.style.getPropertyValue('--f-notice-' + key));


test('default timeout retains the dock during reverse exit and then restores the save bar', () => {
    const f = fixture(), root = f.notice(), bar = f.bar();
    initializeFloatingNotices(f.doc);
    f.advance(4999); f.flush();
    assert.equal(root.getAttribute('data-f-notice-phase'),'shown');
    f.advance(1); f.flush();
    assert.equal(root.getAttribute('data-f-notice-phase'),'closing');
    assert.equal(bar.hasAttribute('data-f-notice-docked'),true);
    f.advance(195);
    assert.equal(root.getAttribute('data-f-notice-phase'),'closing');
    f.advance(1); f.flush();
    assert.equal(root.getAttribute('data-f-notice-phase'),'dismissed');
    assert.equal(root.firstElementChild.hidden,false,'host hidden remains host-owned');
    assert.equal(bar.hasAttribute('data-f-notice-docked'),false);
    f.flush(); f.advance(10000);
    assert.equal(root.getAttribute('data-f-notice-phase'),'dismissed');
});

test('hover, nested focus and background tabs suspend remaining visible time', () => {
    const f = fixture(), root = f.notice(), notice = root.firstElementChild;
    initializeFloatingNotices(f.doc);
    f.advance(2000);
    notice.dispatch('pointerenter');
    f.advance(8000); f.flush();
    notice.dispatch('focusin'); notice.dispatch('pointerleave');
    f.advance(10000);
    assert.equal(root.getAttribute('data-f-notice-phase'),'shown');
    notice.dispatch('focusout',{relatedTarget:null});
    f.advance(1000);
    f.doc.hidden = true; f.listeners.get('visibilitychange')();
    f.advance(10000); f.flush();
    f.doc.hidden = false; f.listeners.get('visibilitychange')();
    f.advance(1999);
    assert.equal(root.getAttribute('data-f-notice-phase'),'shown');
    f.advance(1);
    assert.equal(root.getAttribute('data-f-notice-phase'),'closing');
});

test('new content cancels old callbacks while equal rerenders preserve the deadline', () => {
    const f = fixture(), root = f.notice();
    initializeFloatingNotices(f.doc);
    const firstCallback = [...f.timers.values()][0].fn;
    f.advance(4000); f.flush();
    root.firstElementChild.innerHTML = 'message'; f.flush();
    f.advance(1000);
    assert.equal(root.getAttribute('data-f-notice-phase'),'closing');
    const previousExit = [...f.timers.values()][0].fn;
    root.firstElementChild.innerHTML = 'new result';
    previousExit(); f.flush();
    firstCallback();
    assert.equal(root.getAttribute('data-f-notice-phase'),'shown');
    f.advance(4999);
    assert.equal(root.getAttribute('data-f-notice-phase'),'shown');
    f.advance(1);
    assert.equal(root.getAttribute('data-f-notice-phase'),'closing');
});

test('persistent and reduced-motion messages preserve host visibility and allow an explicit new occurrence', () => {
    const f = fixture(), root = f.notice();
    root.setAttribute('data-f-notice-dismiss-ms','0');
    initializeFloatingNotices(f.doc);
    assert.equal(f.timers.size,0);
    f.advance(100000);
    root.setAttribute('data-f-notice-dismiss-ms','100');
    f.doc.defaultView.reduced = true; f.flush();
    f.advance(100); f.flush();
    assert.equal(root.getAttribute('data-f-notice-phase'),'dismissed');
    root.firstElementChild.hidden = true;
    root.setAttribute('data-f-notice-key','retry-2'); f.flush();
    assert.equal(root.getAttribute('data-f-notice-phase'),'suspended');
    assert.equal(f.timers.size,0);
    root.firstElementChild.hidden = false; f.flush();
    assert.equal(root.getAttribute('data-f-notice-phase'),'shown');
    f.advance(100); f.flush();
    root.setAttribute('data-f-notice-key','retry-3'); f.flush();
    assert.equal(root.getAttribute('data-f-notice-phase'),'shown');
});

test('unmount cancels all timers, handlers and observers without changing the host notice', () => {
    const f = fixture(), root = f.notice(), notice = root.firstElementChild;
    initializeFloatingNotices(f.doc);
    const callback = [...f.timers.values()][0].fn;
    f.frames.length = 0; root.isConnected = false; f.flush();
    assert.equal(f.timers.size,0);
    assert.equal(notice.handlers.size,0);
    assert.ok(f.observers.every(observer => observer.closed));
    callback();
    assert.equal(root.getAttribute('data-f-notice-phase'),null);
    assert.equal(notice.hidden,false);
});

test('hidden pulses reset the occurrence, and replaced children reject old callbacks', () => {
    const f = fixture(), root = f.notice();
    initializeFloatingNotices(f.doc);
    f.advance(4000);
    f.observers.find(observer => observer.targets.has(f.doc.documentElement)).callback([
        {attributeName:'hidden',target:root.firstElementChild}
    ]);
    f.flush(); f.advance(4999);
    assert.equal(root.getAttribute('data-f-notice-phase'),'shown');
    const oldCallback = [...f.timers.values()][0].fn, oldNotice = root.firstElementChild;
    const replacement = f.notice().firstElementChild;
    f.frames.pop(); root.firstElementChild = replacement; replacement.parentElement = root;
    oldCallback(); f.flush();
    assert.equal(oldNotice.handlers.size,0);
    f.advance(4999);
    assert.equal(root.getAttribute('data-f-notice-phase'),'shown');
    f.advance(1);
    assert.equal(root.getAttribute('data-f-notice-phase'),'closing');
    replacement.dispatch('pointerenter');
    f.advance(1000);
    assert.equal(root.getAttribute('data-f-notice-phase'),'shown');
    replacement.dispatch('pointerleave'); f.advance(0);
    assert.equal(root.getAttribute('data-f-notice-phase'),'closing');
    f.advance(196); f.flush();
    assert.equal(root.getAttribute('data-f-notice-phase'),'dismissed');
});

test('exit reverses the same entrance path only when motion is permitted', async () => {
    const css = await readFile(new URL('patterns/floating-notice.css',location),'utf8');
    assert.match(css, /@media\s*\(prefers-reduced-motion:\s*no-preference\)/);
    assert.match(css, /data-f-notice-phase="closing"[^}]+animation:\s*f-notice-exit\s+\.18s\s+ease-in\s+both/s);
    assert.match(css, /data-f-notice-phase="dismissed"[^}]+display:\s*none/s);
    assert.match(css, /@keyframes f-notice-enter[^}]+translateY\(\.5rem\)/s);
    assert.match(css, /@keyframes f-notice-exit\s*\{\s*from\s*\{\s*opacity:1;\s*transform:translateY\(0\);\s*\}\s*to\s*\{\s*opacity:0;\s*transform:translateY\(\.5rem\)/s);
});

test('static hidden feedback participates on reveal and releases the original bar on hide and removal', async () => {
    const entry = await readFile(new URL('primitives/input-behaviors.js',location),'utf8');
    const f = fixture(), bar = f.bar(), root = f.notice();
    root.firstElementChild.hidden = true;
    const url = source => 'data:text/javascript;base64,' + Buffer.from(source).toString('base64');
    const stubs = url('export const attachAccessFormSpacing=()=>{},detachAccessFormSpacing=()=>{},initializePopups=()=>{},initializePresentationProgress=()=>{},initializeShellHeaders=()=>{},initializePrimaryNavigationPreviews=()=>{}');
    const bootstrap = entry.replace(/from '[^']+';/g,'from "' + stubs + '";')
        .replace(/import\('\.\.\/patterns\/floating-notice\.js'\)/,'import("' + url(code) + '")');
    const savedDoc = globalThis.document, savedObserver = globalThis.MutationObserver;
    globalThis.document = f.doc;
    globalThis.MutationObserver = class { observe() {} };
    try {
        await import(url(bootstrap));
        await new Promise(resolve => setImmediate(resolve));
    } finally {
        globalThis.document = savedDoc;
        globalThis.MutationObserver = savedObserver;
    }
    initializeFloatingNotices(root);
    assert.equal(f.observers.length,2);
    assert.equal(bar.hasAttribute('data-f-notice-docked'),false);
    root.firstElementChild.hidden = false; f.flush();
    assert.equal(value(root,'width') + value(bar,'width') + 8,1024);
    assert.equal(value(root,'bottom'),0);
    assert.equal(value(bar,'bottom'),0);
    const count = f.mutations(); f.flush();
    assert.equal(f.mutations(),count,'a stable render must not create another mutation');
    root.firstElementChild.hidden = true; f.flush();
    assert.equal(bar.hasAttribute('data-f-notice-docked'),false);
    assert.equal(bar.style.getPropertyValue('--f-notice-width'),'');
    f.frames.length = 0; f.flush();
    assert.deepEqual([...f.listeners.keys()].sort(),['enhancedload','focusin','input'],'only the permanent input initializer remains');
    assert.ok(f.observers.every(item => item.closed));
});

test('a bar appearing later shares the measured width, and narrow messages stack above its actions', () => {
    const f = fixture(), first = f.notice(), second = f.notice(75);
    initializeFloatingNotices(f.doc);
    assert.equal(value(first,'width'),1024);
    assert.equal(value(first,'bottom'),83);
    const bar = f.bar(); f.flush();
    assert.ok(value(first,'width') < 512);
    for (const frame of f.frames) frame.rect = {left:16,right:344,bottom:784};
    f.doc.documentElement.clientWidth = 360; f.flush();
    assert.equal(value(bar,'width'),328); assert.equal(value(second,'width'),328);
    assert.equal(value(bar,'bottom'),0); assert.equal(value(second,'bottom'),108);
    assert.equal(value(first,'bottom'),191);
    assert.equal(bar.parentElement.style.getPropertyValue('--f-save-changes-clearance'),'','the existing bar owns its clearance');
    f.bars.length = 0; f.flush();
    assert.equal(value(second,'bottom'),0); assert.equal(value(first,'bottom'),83);
    assert.equal(bar.hasAttribute('data-f-notice-docked'),false);
});

test('standalone feedback tracks visual viewport offsets and resizes around a software keyboard', () => {
    const f = fixture(), root = f.notice(160);
    const viewport = {offsetLeft:100,offsetTop:40,width:380,height:280,
        addEventListener(name,handler) { this[name] = handler; }, removeEventListener() {}};
    f.doc.defaultView.visualViewport = viewport;
    initializeFloatingNotices(f.doc);
    assert.equal(value(root,'left'),20);
    assert.equal(value(root,'width'),364);
    assert.equal(value(root,'bottom'),472);
    assert.equal(value(root,'limit'),264);
    viewport.height = 200;
    viewport.resize(); f.flush();
    assert.equal(value(root,'bottom'),552);
    assert.equal(value(root,'limit'),184);
});

test('dialog feedback is bounded by the scrollable dialog body and only shares its own actions', () => {
    const f = fixture(), body = { getBoundingClientRect:() => ({left:280,right:920,top:250,bottom:570}) };
    const dialog = {querySelector:() => body};
    const pageBar = f.bar(), inside = f.notice(120,dialog), dialogBar = f.bar(dialog);
    initializeFloatingNotices(f.doc);
    assert.equal(pageBar.hasAttribute('data-f-notice-docked'),false);
    assert.equal(value(dialogBar,'width'),624);
    assert.equal(value(inside,'left'),200);
    assert.ok(value(inside,'bottom') >= 784 - 570 + 100);
    assert.ok(value(inside,'limit') <= 320 - 100);
    // A translated animation can temporarily make the dialog itself a fixed containing block.
    inside.rect = {left:296,right:904,bottom:550};
    f.listeners.get('animationend')(); f.flush();
    assert.equal(value(inside,'width'),608);
    assert.equal(value(dialogBar,'shift'),0);
    inside.firstElementChild.hidden = true; f.flush();
    assert.equal(dialogBar.hasAttribute('data-f-notice-docked'),false);
});
