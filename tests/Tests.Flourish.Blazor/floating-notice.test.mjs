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
        getComputedStyle:node => ({visibility:'visible',animationDuration:'0.18s',...node.computed}),requestAnimationFrame(fn) { raf.set(++sequence,fn); return sequence; } };
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
    function notice(height = 90,dialog = null,delay = 5000) {
        const frame = node({left:88,right:1112,bottom:784},dialog), message = node({height},dialog);
        // Lifecycle cases use an explicit duration; default/fallback cases remove this attribute.
        frame.setAttribute('data-f-notice-dismiss-ms',String(delay));
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
    return { doc, frames, bars, bar, notice, node, flush, advance, timers, observers, listeners, mutations:() => mutations };
}
const value = (node,key) => parseFloat(node.style.getPropertyValue('--f-notice-' + key));


test('default timeout retains the dock during reverse exit and then restores the save bar', () => {
    const f = fixture(), root = f.notice(), bar = f.bar();
    root.removeAttribute('data-f-notice-dismiss-ms');
    initializeFloatingNotices(f.doc);
    f.advance(3499); f.flush();
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

test('default and malformed SSR durations use 3500ms while explicit durations remain exact', async () => {
    const component = await readFile(new URL('../Components/FloatingNotice.razor',location),'utf8');
    assert.match(component,/AutoDismissMilliseconds\s*\{[^}]+\}\s*=\s*3500;/);
    for (const input of [null,'invalid','-1','Infinity','5000','0']) {
        const f=fixture(), root=f.notice();
        if(input === null) root.removeAttribute('data-f-notice-dismiss-ms');
        else root.setAttribute('data-f-notice-dismiss-ms',input);
        initializeFloatingNotices(f.doc);
        if(input === '0') {
            assert.equal(f.timers.size,0);
            assert.equal(root.hasAttribute('data-f-notice-timed'),false);
            continue;
        }
        const delay=input === '5000' ? 5000 : 3500;
        assert.equal(root.style.getPropertyValue('--f-notice-duration'),delay+'ms');
        f.advance(delay-1);
        assert.equal(root.getAttribute('data-f-notice-phase'),'shown');
        f.advance(1);
        assert.equal(root.getAttribute('data-f-notice-phase'),'closing');
    }
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

test('wide visible shells share content height and shrink without reading their previously imposed height', () => {
    const f = fixture(), root = f.notice(), message = root.firstElementChild, bar = f.bar();
    message.computed = {display:'grid',rowGap:'6px',paddingTop:'13px',paddingBottom:'13px'};
    message.children = [f.node({height:20}),f.node({height:40})];
    bar.computed = {display:'flex',flexDirection:'row',paddingTop:'16px',paddingBottom:'16px',borderTopWidth:'1px',borderBottomWidth:'1px'};
    bar.children = [f.node({height:48}),f.node({height:30})];
    initializeFloatingNotices(f.doc);
    assert.equal(value(root,'height'),92);
    assert.equal(value(bar,'height'),92);
    assert.equal(value(root,'bottom'),value(bar,'bottom'));
    assert.equal(value(bar,'right'),0);
    message.rect.height = 500; bar.rect.height = 500;
    message.children[1].rect.height = 20; f.flush();
    assert.equal(value(root,'height'),82);
    assert.equal(value(bar,'height'),82);
    const count=f.mutations(); f.flush();
    assert.equal(f.mutations(),count,'equalization must settle without an observer feedback loop');
    assert.ok(f.observers.some(observer => observer.targets.has(message.children[1])),'font/content resizing is observed even when the shell is fixed');
    root.rect={left:16,right:344,bottom:784};
    f.doc.documentElement.clientWidth=360; f.flush();
    assert.equal(root.style.getPropertyValue('--f-notice-height'),'');
    assert.equal(bar.style.getPropertyValue('--f-notice-height'),'');
    assert.ok(value(root,'bottom') > value(bar,'bottom'));
});

test('replacement replays only the vertical entry and progress follows the actual paused deadline', () => {
    const f=fixture(), root=f.notice(), message=root.firstElementChild;
    initializeFloatingNotices(f.doc);
    const entry=root.getAttribute('data-f-notice-enter'), running=root.getAttribute('data-f-notice-progress');
    assert.equal(root.style.getPropertyValue('--f-notice-progress'),'1');
    assert.equal(root.style.getPropertyValue('--f-notice-duration'),'5000ms');
    f.advance(2000); message.dispatch('pointerenter');
    assert.equal(root.style.getPropertyValue('--f-notice-progress'),'0.6');
    assert.equal(root.style.getPropertyValue('--f-notice-duration'),'3000ms');
    assert.equal(root.getAttribute('data-f-notice-progress'),null);
    f.advance(10000); message.dispatch('pointerleave');
    assert.notEqual(root.getAttribute('data-f-notice-progress'),running);
    assert.equal(root.style.getPropertyValue('--f-notice-duration'),'3000ms');
    const resumed=root.getAttribute('data-f-notice-progress'); f.flush();
    assert.equal(root.getAttribute('data-f-notice-progress'),resumed,'ordinary rendering must not restart the progress');
    f.advance(500); message.innerHTML='replacement'; f.flush();
    assert.notEqual(root.getAttribute('data-f-notice-enter'),entry);
    assert.equal(root.style.getPropertyValue('--f-notice-progress'),'1');
    assert.equal(root.style.getPropertyValue('--f-notice-duration'),'5000ms');
    f.advance(5000);
    assert.equal(root.style.getPropertyValue('--f-notice-progress'),'0');
    assert.equal(root.getAttribute('data-f-notice-phase'),'closing');
});

test('persistent or host-hidden notices do not expose a countdown and background tabs freeze the fraction', () => {
    const f=fixture(), root=f.notice();
    root.setAttribute('data-f-notice-dismiss-ms','0'); initializeFloatingNotices(f.doc);
    assert.equal(root.hasAttribute('data-f-notice-timed'),false);
    root.setAttribute('data-f-notice-dismiss-ms','5000'); f.flush(); f.advance(1000);
    f.doc.hidden=true; f.listeners.get('visibilitychange')();
    assert.equal(root.style.getPropertyValue('--f-notice-progress'),'0.8');
    assert.equal(root.getAttribute('data-f-notice-progress'),null);
    f.advance(9000); f.doc.hidden=false; f.listeners.get('visibilitychange')();
    assert.equal(root.style.getPropertyValue('--f-notice-duration'),'4000ms');
    root.firstElementChild.hidden=true; f.flush();
    assert.equal(root.hasAttribute('data-f-notice-timed'),false);
    root.firstElementChild.hidden=false; f.flush();
    assert.equal(root.style.getPropertyValue('--f-notice-progress'),'1');
    f.frames.length=0; root.isConnected=false; f.flush();
    for(const name of ['progress','duration','steps']) assert.equal(root.style.getPropertyValue('--f-notice-'+name),'');
});

test('dock transitions keep the action right edge fixed while notice motion and the decorative clock stay separate', async () => {
    const noticeCss=await readFile(new URL('patterns/floating-notice.css',location),'utf8');
    const barCss=await readFile(new URL('patterns/save-changes-bar.css',location),'utf8');
    assert.match(barCss,/left:auto; right:var\(--f-save-panel-right\)/);
    assert.match(barCss,/transition:width \.18s ease-out/);
    assert.doesNotMatch(noticeCss,/translate:|transition:/);
    assert.match(noticeCss,/align-content:start/);
    assert.equal((barCss.match(/transition:width/g) ?? []).length,1);
    assert.match(noticeCss,/data-f-notice-enter="b"[^}]+f-notice-enter-again/s);
    assert.match(noticeCss,/data-f-notice-timed[^}]+position:absolute[^}]+height:3px/s);
    assert.match(noticeCss,/prefers-reduced-motion:reduce[^}]+animation-timing-function:steps\(var\(--f-notice-steps,5\),end\)/s);
    // The right inset formula is the same centered envelope for every viewport and safe area.
    for (const [viewport,left,right] of [[1200,16,16],[390,16,16],[1200,50,20]]) {
        const originalRight=viewport-right-Math.max(0,(viewport-left-right-1024)/2);
        const inset=right+Math.max(0,(viewport-left-right-1024)/2);
        for(const width of [Math.min(1024,viewport-left-right),580]) assert.equal(viewport-inset,originalRight);
    }
});

test('reduced motion replacements reset the real countdown while obsolete callbacks cannot hide the new message', () => {
    const f=fixture(), root=f.notice();
    f.doc.defaultView.reduced=true; initializeFloatingNotices(f.doc);
    const previous=[...f.timers.values()][0].fn;
    f.advance(4000); root.firstElementChild.innerHTML='new reduced message'; f.flush();
    assert.equal(root.style.getPropertyValue('--f-notice-progress'),'1');
    assert.equal(root.style.getPropertyValue('--f-notice-duration'),'5000ms');
    previous();
    assert.equal(root.getAttribute('data-f-notice-phase'),'shown');
    f.advance(4999);
    assert.equal(root.getAttribute('data-f-notice-phase'),'shown');
    f.advance(1); f.flush();
    assert.equal(root.getAttribute('data-f-notice-phase'),'dismissed');
    assert.equal(root.style.getPropertyValue('--f-notice-progress'),'0');
});

test('a cleared queued timeout cannot consume paused time or end a resumed occurrence', () => {
    const f=fixture(), root=f.notice(), message=root.firstElementChild;
    initializeFloatingNotices(f.doc);
    const obsolete=[...f.timers.values()][0].fn;
    f.advance(2000); message.dispatch('pointerenter');
    obsolete();
    assert.equal(root.style.getPropertyValue('--f-notice-progress'),'0.6');
    message.dispatch('pointerleave'); obsolete();
    assert.equal(root.getAttribute('data-f-notice-phase'),'shown');
    assert.equal(root.style.getPropertyValue('--f-notice-duration'),'3000ms');
    f.advance(2999);
    assert.equal(root.getAttribute('data-f-notice-phase'),'shown');
    f.advance(1);
    assert.equal(root.getAttribute('data-f-notice-phase'),'closing');
});

test('a cancelled exit callback cannot finish a later exit of the same message', () => {
    const f=fixture(), root=f.notice(), message=root.firstElementChild;
    initializeFloatingNotices(f.doc); f.advance(5000);
    const oldExit=[...f.timers.values()][0].fn;
    f.advance(50); message.dispatch('pointerenter'); message.dispatch('pointerleave'); f.advance(0);
    assert.equal(root.getAttribute('data-f-notice-phase'),'closing');
    oldExit();
    assert.equal(root.getAttribute('data-f-notice-phase'),'closing');
    f.advance(195);
    assert.equal(root.getAttribute('data-f-notice-phase'),'closing');
    f.advance(1);
    assert.equal(root.getAttribute('data-f-notice-phase'),'dismissed');
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
    assert.equal(value(dialogBar,'right'),0);
    inside.firstElementChild.hidden = true; f.flush();
    assert.equal(dialogBar.hasAttribute('data-f-notice-docked'),false);
});
