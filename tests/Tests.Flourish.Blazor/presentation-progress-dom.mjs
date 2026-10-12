import assert from 'node:assert/strict';
import fs from 'node:fs/promises';

let checks = 0;
const check = (name, run) => { run(); checks++; console.log('PASS ' + name); };
class Events {
    events = new Map();
    addEventListener(name, callback) { const list = this.events.get(name) ?? []; if (!list.includes(callback)) list.push(callback); this.events.set(name, list); }
    removeEventListener(name, callback) { this.events.set(name, (this.events.get(name) ?? []).filter(value => value !== callback)); }
    emit(name, event = {}) { for (const callback of [...(this.events.get(name) ?? [])]) callback(event); }
    listeners(name) { return (this.events.get(name) ?? []).length; }
}
class Node extends Events {
    static all = [];
    constructor(tag, classes = []) {
        super(); this.tag = tag; this.attributes = new Map(); this.children = []; this.parentElement = null;
        this.style = { values:new Map(), setProperty(name, value) { this.values.set(name, value); }, removeProperty(name) { this.values.delete(name); } };
        this.className = classes.join(' '); this.classList = { contains:value => this.className.split(/\s+/).includes(value) };
        this.textContent = ''; this.docTop = 0; this.height = 200; this.left = 0; this.width = 1000;
        this.clientHeight = 400; this.clientWidth = 1000; this.clientTop = 0; this.clientLeft = 0;
        this.scrollTop = 0; this.isConnected = true; Node.all.push(this);
    }
    get id() { return this.getAttribute('id') ?? ''; }
    set id(value) { this.setAttribute('id', value); }
    setAttribute(name, value) { this.attributes.set(name, String(value)); }
    getAttribute(name) { return this.attributes.get(name) ?? null; }
    hasAttribute(name) { return this.attributes.has(name); }
    removeAttribute(name) { this.attributes.delete(name); }
    toggleAttribute(name, value) { if (value ?? !this.hasAttribute(name)) this.setAttribute(name, ''); else this.removeAttribute(name); }
    matches(selector) {
        return selector.split(',').some(raw => {
            const part = raw.trim();
            if (part === 'details:not([open])') return this.tag === 'details' && !this.hasAttribute('open');
            if (part.startsWith('.')) return this.classList.contains(part.slice(1));
            const attribute = /^\[([^=\]]+)(?:="([^"]*)")?\]$/.exec(part);
            if (attribute) return this.hasAttribute(attribute[1]) && (attribute[2] === undefined || this.getAttribute(attribute[1]) === attribute[2]);
            return this.tag === part;
        });
    }
    closest(selector) { for (let node = this; node; node = node.parentElement) if (node.matches(selector)) return node; return null; }
    querySelectorAll(selector) {
        const found = []; const visit = node => { for (const child of node.children) { if (child.matches(selector)) found.push(child); visit(child); } };
        visit(this); return found;
    }
    querySelector(selector) { return this.querySelectorAll(selector)[0] ?? null; }
    contains(node) { for (let value = node; value; value = value.parentElement) if (value === this) return true; return false; }
    append(...nodes) {
        for (const node of nodes) {
            if (node.tag === 'fragment') { this.append(...node.children); continue; }
            if (node.parentElement) node.parentElement.children = node.parentElement.children.filter(child => child !== node);
            node.parentElement = this; node.connected(true); this.children.push(node);
        }
    }
    connected(value) { this.isConnected = value; this.children.forEach(node => node.connected(value)); }
    remove() {
        if (this.parentElement) this.parentElement.children = this.parentElement.children.filter(node => node !== this);
        this.parentElement = null; this.connected(false);
    }
    replaceChildren(...nodes) { this.children.forEach(node => { node.parentElement = null; node.connected(false); }); this.children = []; this.append(...nodes); }
    getBoundingClientRect() {
        let shift = window.scrollY; for (let parent = this.parentElement; parent; parent = parent.parentElement) shift += parent.scrollTop;
        const top = this.position === 'sticky' ? Math.max(this.docTop - shift, this.stickyTop ?? 0) : this.docTop - shift;
        return { top, bottom:top + this.height, left:this.left, right:this.left + this.width, height:this.height, width:this.width };
    }
    scrollTo(options) { this.scrolled = options; this.scrollTop = options.top; this.emit('scroll'); }
    focus(options) { document.activeElement = this; this.focusOptions = options; }
}
globalThis.window = new Events(); window.innerHeight = 800; window.innerWidth = 1000; window.scrollY = 0;
window.scrollTo = options => { window.scrolled = options; window.scrollY = options.top; window.emit('scroll'); document.emit('scroll'); };
window.visualViewport = new Events(); Object.assign(window.visualViewport, { offsetTop:0, offsetLeft:0, height:800, width:1000 });
globalThis.document = new Events(); document.body = new Node('body'); document.documentElement = new Node('html');
document.querySelectorAll = selector => document.body.querySelectorAll(selector);
document.createElement = tag => new Node(tag);
document.createDocumentFragment = () => new Node('fragment');
document.getElementById = id => Node.all.find(node => node.isConnected && node.id === id) ?? null;
globalThis.getComputedStyle = node => ({ position:node.position ?? 'static', overflowX:node.overflowX ?? 'visible', overflowY:node.overflowY ?? 'visible', display:node.display ?? 'block', visibility:node.visibility ?? 'visible', color:node.color ?? 'rgb(0, 0, 0)' });
const motion = new Events(); motion.matches = false;
globalThis.matchMedia = () => motion;
const frames = new Map(), timers = new Map(); let sequence = 0, now = 0;
globalThis.requestAnimationFrame = callback => { const id = ++sequence; frames.set(id, callback); return id; };
globalThis.cancelAnimationFrame = id => frames.delete(id);
globalThis.setTimeout = (callback, delay) => { const id = ++sequence; timers.set(id, { callback, at:now + delay }); return id; };
globalThis.clearTimeout = id => timers.delete(id);
const flush = () => { let runs = 0; while (frames.size) { const batch = [...frames.values()]; frames.clear(); batch.forEach(callback => callback()); assert.ok(++runs < 30, 'Refresh did not settle'); } };
const advance = duration => { const end = now + duration; let runs = 0; while (true) { const next = [...timers].filter(([, timer]) => timer.at <= end).sort((a, b) => a[1].at - b[1].at)[0]; if (!next) break; timers.delete(next[0]); now = next[1].at; next[1].callback(); assert.ok(++runs < 30, 'Alignment did not settle'); } now = end; };
class Observer {
    static all = [];
    constructor(callback) { this.callback = callback; this.observed = []; this.disconnections = 0; Observer.all.push(this); }
    observe(node) { this.observed.push(node); }
    disconnect() { this.observed = []; this.disconnections++; }
    notify(target = document.body) { this.callback([{ target }]); }
}
globalThis.MutationObserver = Observer; globalThis.ResizeObserver = Observer;
const source = await fs.readFile(new URL('../../src/Flourish.Blazor/Flourish.Blazor.Framework/wwwroot/presentation/progress.js', import.meta.url), 'utf8');
const api = await import('data:text/javascript;base64,' + Buffer.from(source).toString('base64'));
let fixtureNumber = 0;
function band(title, top, { id, marked = true } = {}) {
    const section = new Node('section', ['f-presentation-band']); section.docTop = top; section.height = 800;
    if (marked) section.setAttribute('data-presentation-band', 'true');
    if (id) section.id = id;
    const heading = new Node('h2'); heading.textContent = title; heading.docTop = top; heading.height = 30; section.append(heading);
    return { section, heading };
}
function fixture({ count = 3, local = false, footer = true } = {}) {
    window.scrollY = 0;
    const surface = new Node('div', ['content-surface']); document.body.append(surface);
    const header = new Node('div', ['f-document-surface-header']); header.height = 64; header.position = 'sticky'; surface.append(header);
    const content = new Node('main', ['content-stage']); content.id = 'application-content'; content.docTop = 64; content.height = count * 800;
    if (local) { content.docTop = 100; content.height = 400; content.overflowY = 'auto'; header.stickyTop = 100; header.docTop = 100; }
    surface.append(content);
    const bands = Array.from({ length:count }, (_, index) => band('Band ' + (index + 1), content.docTop + index * 800));
    content.append(...bands.map(value => value.section));
    const ending = new Node('footer'); ending.docTop = 64 + count * 800; ending.height = 240;
    if (footer) surface.append(ending);
    const nav = new Node('nav', ['f-presentation-progress']); nav.id = 'progress-' + ++fixtureNumber; nav.height = 44; nav.setAttribute('data-content-id', content.id); nav.append(new Node('ol')); surface.append(nav);
    return { surface, header, content, bands, ending, nav, links:() => nav.querySelectorAll('.f-presentation-progress-link') };
}
const click = target => ({ target, button:0, prevented:false, preventDefault() { this.prevented = true; } });
const first = fixture(); first.bands[2].section.id = "authored-scene";
first.bands[0].section.color = 'rgb(255, 255, 255)'; first.bands[1].section.color = 'rgb(17, 41, 36)'; first.bands[2].section.color = 'rgb(40, 60, 80)';
const unmarked = band('Ordinary section', 300, { marked:false }); first.content.append(unmarked.section);
const nested = band('Nested sample', 1000); first.bands[1].section.append(nested.section);
const footerBand = band('Footer band', 2600); first.ending.append(footerBand.section);
const innerFooter = new Node('footer'); first.content.append(innerFooter); innerFooter.append(band('Inside main footer', 1600).section); innerFooter.height = 0;
const slot = new Node('div'); slot.setAttribute('data-presentation-footer', ''); slot.height = 0; slot.append(band('Footer slot without footer tag', 2800).section); first.content.append(slot);
const embedded = new Node('main'); first.content.append(embedded); embedded.append(band('Embedded main', 1000).section);
api.initializePresentationProgress(first.surface);
check('Static SSR bootstrap discovers only owned semantic bands, excluding every footer boundary and nested sample', () => {
    assert.equal(first.links().length, 3); assert.deepEqual(first.links().map(link => link.getAttribute('aria-label')), ['Band 1', 'Band 2', 'Band 3']);
    assert.equal(first.nav.hidden, false); assert.equal(footerBand.section.id, '');
    assert.ok(first.links().every(link => link.classList.contains('f-tutorial-segment') && !link.hasAttribute('title') && link.children.length === 0));
});
check('Transparent segments take the actual band foreground at their landing point, rather than the top active band', () => {
    assert.equal(first.nav.style.values.get('--f-presentation-progress-foreground'), first.bands[0].section.color);
    window.scrollY = 300; document.emit('scroll');
    assert.equal(first.links()[0].getAttribute('aria-current'), 'location');
    assert.equal(first.nav.style.values.get('--f-presentation-progress-foreground'), first.bands[1].section.color);
    window.scrollY = 0; document.emit('scroll');
});
check('Stable SSR and component synchronization reuse listeners, anchors and observers', () => {
    const links = first.links(), before = Observer.all.map(observer => observer.disconnections);
    api.initializePresentationProgress(first.surface); api.synchronize(first.nav, first.content.id);
    assert.deepEqual(first.links(), links); assert.equal(first.nav.listeners('click'), 1); assert.deepEqual(Observer.all.map(observer => observer.disconnections), before);
});
check('A segment click aligns its band below the sticky header and focuses actual content', () => {
    const event = click(first.links()[1]); first.nav.emit('click', event);
    assert.equal(event.prevented, true); assert.deepEqual(window.scrolled, { top:800, behavior:'smooth' });
    assert.equal(document.activeElement, first.bands[1].heading); assert.deepEqual(document.activeElement.focusOptions, { preventScroll:true });
});
check('A header resize during navigation receives one bounded final alignment', () => {
    first.header.height = 96; window.emit('resize'); flush(); advance(120);
    assert.deepEqual(window.scrolled, { top:768, behavior:'auto' }); assert.equal(timers.size, 0);
    assert.ok(first.links()[1].hasAttribute('aria-current')); first.header.height = 64;
});
check('Manual scroll follows the current band without changing document scroll position', () => {
    window.scrollY = 1600; document.emit('scroll');
    assert.equal(first.links()[2].getAttribute('aria-current'), 'location'); assert.equal(first.links().filter(link => link.hasAttribute('aria-current')).length, 1);
});
check('Visible last-band segments stay floating above the footer and target their band', () => {
    window.scrollY = 2200; document.emit('scroll');
    assert.equal(first.nav.hidden, false); assert.equal(first.nav.hasAttribute('data-footer-visible'), false);
    assert.equal(first.links()[2].getAttribute('aria-current'), 'location');
    assert.ok(Number.parseFloat(first.nav.style.values.get('--f-presentation-progress-top')) + first.nav.height <= first.ending.getBoundingClientRect().top);
    first.nav.emit('click', click(first.links()[2])); assert.equal(window.scrolled.top, 1600);
    advance(120);
});
check('A footer-only viewport hides navigation without clearing its final selection, then restores it', () => {
    const links = first.links(); window.scrollY = first.ending.docTop; document.emit('scroll');
    assert.equal(first.nav.hidden, true); assert.equal(first.nav.hasAttribute('data-footer-visible'), false);
    assert.equal(first.links()[2].getAttribute('aria-current'), 'location');
    window.scrollY = 2200; document.emit('scroll'); assert.equal(first.nav.hidden, false); assert.deepEqual(first.links(), links);
});
check('Reduced motion changes immediately; modified links keep native fragment semantics', () => {
    motion.matches = true; first.nav.emit('click', click(first.links()[0])); assert.equal(window.scrolled.behavior, 'auto'); advance(120);
    const event = { ...click(first.links()[1]), ctrlKey:true }; first.nav.emit('click', event); assert.equal(event.prevented, false);
    motion.matches = false;
});
check('Changing reduced-motion preference settles and clears an existing navigation', () => {
    first.nav.emit('click', click(first.links()[1])); motion.matches = true; motion.emit('change');
    assert.equal(timers.size, 0); motion.matches = false;
});
check('User wheel input cancels pending alignment instead of fighting a manual scroll', () => {
    first.nav.emit('click', click(first.links()[1])); document.emit('wheel'); window.scrollY = 900; advance(2000); assert.equal(window.scrollY, 900); assert.equal(timers.size, 0);
});
check('Visual viewport resize repositions the centered navigation', () => {
    window.scrollY = 0; window.visualViewport.height = 600; window.visualViewport.width = 840; window.visualViewport.offsetLeft = 40;
    window.visualViewport.emit('resize'); flush(); assert.equal(first.nav.style.values.get('--f-presentation-progress-left'), '460px');
    assert.equal(first.nav.style.values.get('--f-presentation-progress-top'), '540px');
    Object.assign(window.visualViewport, { height:800, width:1000, offsetLeft:0 });
});
const local = fixture({ local:true, footer:false });
api.synchronize(local.nav, local.content.id);
check('A contained scroll viewport scrolls only itself with its local header offset', () => {
    local.nav.emit('click', click(local.links()[1])); assert.deepEqual(local.content.scrolled, { top:736, behavior:'smooth' }); assert.equal(window.scrollY, 0); advance(120);
    assert.equal(local.links()[1].getAttribute('aria-current'), 'location');
});
check('Multiple surfaces with legacy identical main IDs own distinct band anchors and targets', () => {
    assert.notEqual(first.bands[0].section.id, local.bands[0].section.id);
    assert.ok(local.links().every(link => link.getAttribute('href').startsWith('#' + local.nav.id + '-band-')));
});
check('Changing a scroll owner releases the old listener and uses the replacement owner', () => {
    local.content.overflowY = 'visible'; local.content.scrollTop = 0; local.header.docTop = 0; local.header.stickyTop = 0; window.emit('resize'); flush(); assert.equal(local.content.listeners('scroll'), 0);
    local.nav.emit('click', click(local.links()[0])); assert.equal(window.scrolled.top, 36); advance(120);
});
const single = fixture({ count:1 }); api.initializePresentationProgress(single.surface);
check('A one-band homepage still gets one clickable segment and its footer never adds a segment', () => {
    assert.equal(single.links().length, 1); assert.equal(single.nav.hidden, false);
    window.scrollY = 300; document.emit('scroll'); assert.equal(single.nav.hidden, false); assert.equal(single.nav.hasAttribute('data-footer-visible'), false);
    single.nav.emit('click', click(single.links()[0])); assert.equal(window.scrolled.top, 0); advance(120);
});
check('A route stage replacement releases old generated IDs and binds the new owned content', () => {
    const old = single.content; old.remove();
    const replacement = new Node('main', ['content-stage']); replacement.id = old.id; replacement.docTop = 64; replacement.height = 800;
    const fresh = band('Replacement', 64); replacement.append(fresh.section); single.surface.children.unshift(replacement); replacement.parentElement = single.surface;
    Observer.all.forEach(observer => observer.notify(single.surface)); flush();
    assert.equal(single.links().length, 1); assert.equal(single.links()[0].getAttribute('aria-label'), 'Replacement'); assert.equal(single.bands[0].section.id, '');
});
check('Removal and detach clear every owned observer, listener, timer and generated attribute', () => {
    first.nav.emit('click', click(first.links()[1])); first.nav.remove(); Observer.all.forEach(observer => observer.notify()); flush();
    assert.equal(first.nav.listeners('click'), 0); assert.equal(first.bands[1].heading.hasAttribute('tabindex'), false); assert.equal(first.bands[0].section.id, ''); assert.equal(first.bands[2].section.id, 'authored-scene');
    api.detach(local.nav); api.detach(single.nav); flush(); advance(2000);
    assert.equal(document.listeners('scroll'), 0); assert.equal(window.listeners('resize'), 0); assert.equal(window.visualViewport.listeners('resize'), 0);
    assert.equal(motion.listeners('change'), 0); assert.ok(Observer.all.every(observer => observer.observed.length === 0));
    assert.equal(frames.size, 0); assert.equal(timers.size, 0);
    assert.equal(first.nav.style.values.has('--f-presentation-progress-foreground'), false);
});
// A local presentation example inside the actual shell's independent content scroller.
const nestedOuter = new Node('main', ['f-content-scroll']); nestedOuter.overflowY = 'auto';
nestedOuter.docTop = 128; nestedOuter.height = nestedOuter.clientHeight = 500; document.body.append(nestedOuter);
const bounded = fixture({ count:2, footer:false }); nestedOuter.append(bounded.surface); bounded.header.remove();
bounded.surface.overflowY = 'auto'; bounded.surface.docTop = 200; bounded.surface.height = bounded.surface.clientHeight = 420;
bounded.content.docTop = 200; bounded.content.height = 1600;
bounded.bands.forEach((entry, index) => { entry.section.docTop = 200 + index * 800; entry.heading.docTop = 200 + index * 800; });
api.synchronize(bounded.nav, bounded.content.id);
check('Nested scroll clipping uses the outer top boundary and hides an example cropped above it', () => {
    nestedOuter.scrollTop = 200; document.emit('scroll');
    bounded.nav.emit('click', click(bounded.links()[0]));
    assert.equal(bounded.surface.scrolled.top, 0); assert.equal(nestedOuter.scrollTop, 200);
    assert.equal(bounded.nav.hidden, false); assert.equal(bounded.nav.style.values.get('--f-presentation-progress-top'), '360px');
    advance(120);
    nestedOuter.scrollTop = 550; document.emit('scroll');
    assert.equal(bounded.nav.hidden, true, 'Segments escaped above their ancestor clip into shell chrome');
});
check('An outer bottom clip bounds segments, then a hidden example restores its owned links', () => {
    nestedOuter.scrollTop = 0; nestedOuter.height = nestedOuter.clientHeight = 300;
    window.emit('resize'); flush();
    assert.equal(bounded.nav.hidden, false); assert.equal(bounded.nav.style.values.get('--f-presentation-progress-top'), '368px');
    const links = bounded.links();
    nestedOuter.height = nestedOuter.clientHeight = 50; window.emit('resize'); flush(); assert.equal(bounded.nav.hidden, true);
    nestedOuter.height = nestedOuter.clientHeight = 500; window.emit('resize'); flush();
    assert.equal(bounded.nav.hidden, false); assert.deepEqual(bounded.links(), links);
    bounded.nav.emit('click', click(bounded.links()[1]));
    assert.equal(bounded.surface.scrolled.top, 800); assert.equal(nestedOuter.scrollTop, 0); advance(120);
});
check('The measured wrapped segment height and edge spacing must fit the clipped viewport', () => {
    nestedOuter.height = nestedOuter.clientHeight = 152; nestedOuter.scrollTop = 0; bounded.surface.scrollTop = 0;
    bounded.nav.height = 96; window.emit('resize'); flush();
    assert.equal(bounded.nav.hidden, true, 'Wrapped segments escaped their actual visible viewport');
    nestedOuter.height = nestedOuter.clientHeight = 240; window.emit('resize'); flush();
    assert.equal(bounded.nav.hidden, false);
    assert.ok(Number.parseFloat(bounded.nav.style.values.get('--f-presentation-progress-top')) + bounded.nav.height + 16 <= nestedOuter.getBoundingClientRect().bottom);
    bounded.nav.height = 44;
});
check('Ancestor-only hidden/clip overflow constrains geometry without observing chart child scrollers', () => {
    const chartScroller = new Node('div', ['f-line-chart-plot']); chartScroller.overflowY = 'auto'; chartScroller.docTop = 200; chartScroller.height = chartScroller.clientHeight = 30;
    bounded.bands[0].section.append(chartScroller);
    nestedOuter.overflowY = 'clip'; nestedOuter.height = nestedOuter.clientHeight = 160; nestedOuter.overflowX = 'hidden';
    nestedOuter.left = 300; nestedOuter.width = nestedOuter.clientWidth = 400;
    Observer.all.forEach(observer => observer.notify(nestedOuter)); flush();
    assert.equal(bounded.nav.hidden, false); assert.equal(bounded.nav.style.values.get('--f-presentation-progress-left'), '500px');
    assert.equal(bounded.nav.style.values.get('--f-presentation-progress-width'), '376px');
    assert.equal(bounded.nav.style.values.get('--f-presentation-progress-top'), '228px');
    assert.ok(Observer.all.some(observer => observer.observed.includes(nestedOuter)), 'Outer geometry changes lack an observer');
    assert.ok(Observer.all.every(observer => !observer.observed.includes(chartScroller)), 'A chart child took ownership of presentation geometry');
});
api.detach(bounded.nav); bounded.surface.remove(); nestedOuter.remove(); flush();
check('Nested ancestor geometry observers and scroll listeners are released on detach', () => {
    assert.equal(bounded.surface.listeners('scroll'), 0); assert.equal(nestedOuter.listeners('scroll'), 0);
    assert.ok(Observer.all.every(observer => observer.observed.length === 0));
    assert.equal(document.listeners('scroll'), 0); assert.equal(window.listeners('resize'), 0); assert.equal(timers.size, 0);
});
const empty = fixture({ count:0, footer:false }); api.initializePresentationProgress(empty.surface);
check('A surface without a presentation band stays hidden and does not invent a section', () => { assert.equal(empty.nav.hidden, true); assert.equal(empty.links().length, 0); });
api.detach(empty.nav);
const inlineFooter = fixture({ count:1, footer:false });
inlineFooter.content.height += inlineFooter.ending.height; inlineFooter.content.append(inlineFooter.ending);
api.initializePresentationProgress(inlineFooter.surface);
check('A footer inside main cannot keep segments visible after its actual band leaves the viewport', () => {
    assert.equal(inlineFooter.links().length, 1); assert.equal(inlineFooter.nav.hidden, false);
    window.scrollY = inlineFooter.ending.docTop; document.emit('scroll');
    assert.ok(inlineFooter.content.getBoundingClientRect().bottom > 0); assert.equal(inlineFooter.nav.hidden, true);
    window.scrollY = 0; document.emit('scroll'); assert.equal(inlineFooter.nav.hidden, false);
});
api.detach(inlineFooter.nav);
console.log(checks + ' presentation progress checks passed.');
