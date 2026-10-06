import assert from 'node:assert/strict';
import { readFile } from 'node:fs/promises';
import test from 'node:test';

const source = await readFile(new URL('../../src/Flourish.Blazor/Flourish.Blazor.Framework/wwwroot/presentation/offers.js', import.meta.url), 'utf8');
const { synchronize, dispose } = await import(`data:text/javascript;base64,${Buffer.from(source).toString('base64')}`);

class Events {
    listeners = new Map();
    addEventListener(name, handler) {
        if (!this.listeners.has(name)) this.listeners.set(name, new Set());
        this.listeners.get(name).add(handler);
    }
    removeEventListener(name, handler) { this.listeners.get(name)?.delete(handler); }
    emit(name, event = {}) { for (const handler of [...this.listeners.get(name) ?? []]) handler(event); }
    listenerCount() { return [...this.listeners.values()].reduce((sum, handlers) => sum + handlers.size, 0); }
}

class Element extends Events {
    constructor(document, id = '', classes = []) {
        super();
        this.ownerDocument = document;
        this.id = id;
        this.classList = { contains: value => classes.includes(value) };
        this.children = [];
        this.parent = null;
        this.dataset = {};
        this.isConnected = true;
        const values = new Map();
        this.style = {
            setProperty: (name, value) => values.set(name, value),
            removeProperty: name => values.delete(name),
            getPropertyValue: name => values.get(name) ?? ''
        };
    }
    contains(node) { return node === this || this.children.some(child => child.contains(node)); }
    closest(selector) {
        assert.equal(selector, '.f-offer-presentation');
        for (let node = this; node; node = node.parent)
            if (node.classList.contains('f-offer-presentation')) return node;
        return null;
    }
    removeAttribute(name) {
        if (name.startsWith('data-')) delete this.dataset[name.slice(5).replace(/-([a-z])/g, (_, letter) => letter.toUpperCase())];
    }
    append(...children) { for (const child of children) { child.parent = this; this.children.push(child); } }
    focus(options) {
        this.focusOptions = options;
        this.ownerDocument.activeElement = this;
        for (let node = this; node; node = node.parent) node.emit('focusin');
    }
    querySelectorAll(selector) {
        assert.equal(selector, 'a[href]');
        return this.children.filter(child => child.href !== undefined);
    }
}

function fixture({ matches = true, matchMedia = true, mutationObserver = true } = {}) {
    const document = new Events();
    Object.assign(document, {
        URL: 'https://product.test/pricing/?lang=pt', baseURI: 'https://product.test/pricing/?lang=pt',
        hidden: false, activeElement: null, documentElement: {}, elements: new Map(),
        getElementById(id) { return this.elements.get(id) ?? null; }
    });
    const timers = new Map(), microtasks = [], media = [], observers = [];
    let timerId = 0;
    const window = {
        setInterval(callback, duration) { const id = ++timerId; timers.set(id, { callback, duration }); return id; },
        clearInterval(id) { timers.delete(id); },
        queueMicrotask(callback) { microtasks.push(callback); }
    };
    if (matchMedia) window.matchMedia = query => {
        assert.equal(query, '(min-width: 1100px) and (prefers-reduced-motion: no-preference)');
        const result = new Events(); result.matches = matches; media.push(result); return result;
    };
    if (mutationObserver) window.MutationObserver = class {
        constructor(callback) { this.callback = callback; this.connected = false; observers.push(this); }
        observe(node, config) { assert.equal(node, document.documentElement); assert.equal(config.subtree, true); this.connected = true; }
        disconnect() { this.connected = false; }
    };
    document.defaultView = window;
    function card(id) { const result = new Element(document, id, ['f-offer-card']); document.elements.set(id, result); return result; }
    function stage(prefix = 'offer', count = 3) {
        const root = new Element(document, `${prefix}-stage`, ['f-offer-stage']);
        root.append(...Array.from({ length: count }, (_, index) => card(`${prefix}-${index}`)));
        return root;
    }
    function scope(id, hrefs) {
        const result = new Element(document, id);
        result.append(...hrefs.map(href => Object.assign(new Element(document), { href })));
        document.elements.set(id, result);
        return result;
    }
    return {
        document, window, timers, media, observers, card, stage, scope,
        active: root => root.children.findIndex(child => child.dataset.active === 'true'),
        tick: () => { for (const timer of [...timers.values()]) timer.callback(); },
        flush: () => { while (microtasks.length) microtasks.shift()(); },
        mutate: () => { for (const observer of [...observers]) if (observer.connected) observer.callback(); },
        changeMedia: (index, value) => { media[index].matches = value; media[index].emit('change'); }
    };
}

function click(link, overrides = {}) {
    const event = { defaultPrevented: false, button: 0, ctrlKey: false, metaKey: false, altKey: false, shiftKey: false,
        preventDefault() { this.defaultPrevented = true; }, ...overrides };
    link.emit('click', event);
    return event;
}

test('progressive enhancement starts only after attach and derives columns from the actual number of cards', () => {
    for (const count of [1, 2, 3, 5]) {
        const f = fixture(), root = f.stage('actual', count);
        assert.equal(root.dataset.offerReady, undefined);
        assert.ok(root.children.every(card => card.dataset.active === undefined));
        synchronize(root, null);
        assert.equal(root.dataset.offerReady, '');
        assert.equal(root.style.getPropertyValue('--f-offer-columns'), String(count + 1));
        assert.equal(f.active(root), 0);
        assert.equal(f.timers.size, count > 1 ? 1 : 0);
        if (count > 1) { f.tick(); assert.equal(f.active(root), 1); }
        dispose(root);
        assert.equal(root.dataset.offerReady, undefined);
        assert.ok(root.children.every(card => card.dataset.active === undefined));
    }
});

test('two stages keep independent active cards timers configuration and disposal', () => {
    const f = fixture(), left = f.stage('left', 2), right = f.stage('right', 4);
    synchronize(left, null, true, 1200);
    synchronize(right, null, false, 3000);
    assert.equal(left.style.getPropertyValue('--f-offer-columns'), '3');
    assert.equal(right.style.getPropertyValue('--f-offer-columns'), '5');
    assert.equal(f.timers.size, 1);
    f.tick(); assert.equal(f.active(left), 1); assert.equal(f.active(right), 0);
    right.children[3].emit('pointerenter');
    assert.equal(f.active(right), 3); assert.equal(f.active(left), 1);
    dispose(left);
    assert.equal(f.timers.size, 0);
    assert.equal(right.dataset.offerReady, '');
    right.children[1].emit('pointerdown'); assert.equal(f.active(right), 1);
    dispose(right); assert.equal(f.document.listenerCount(), 0);
});

test('narrow and reduced-motion modes are readable static layouts and release enhancement state', () => {
    const f = fixture({ matches: false }), root = f.stage();
    synchronize(root, null);
    assert.equal(root.dataset.offerReady, undefined);
    assert.equal(root.style.getPropertyValue('--f-offer-columns'), '');
    assert.equal(f.active(root), -1); assert.equal(f.timers.size, 0);
    f.changeMedia(0, true);
    assert.equal(root.dataset.offerReady, ''); assert.equal(f.timers.size, 1);
    f.tick(); assert.equal(f.active(root), 1);
    f.changeMedia(0, false);
    assert.equal(root.dataset.offerReady, undefined); assert.equal(f.timers.size, 0);
    assert.ok(root.children.every(card => card.dataset.active === 'false'));
    dispose(root);
    const noMedia = fixture({ matchMedia: false }), native = noMedia.stage();
    assert.throws(() => synchronize(native, null), TypeError, 'A missing native media API must fail instead of selecting an old browser layout.');
    assert.equal(native.dataset.offerReady, undefined);
    assert.ok(native.children.every(card => card.dataset.active === undefined));
    assert.equal(noMedia.timers.size, 0);
});

test('a missing required MutationObserver is not treated as a supported static browser mode', () => {
    const f = fixture({ mutationObserver: false }), root = f.stage();
    assert.throws(() => synchronize(root, null), TypeError);
    assert.equal(root.dataset.offerReady, undefined);
    assert.equal(f.timers.size, 0);
});

test('pointer focus and document visibility pause rotation while focus prevents unrelated card activation', () => {
    const f = fixture(), root = f.stage();
    synchronize(root, null);
    root.emit('pointerenter'); assert.equal(f.timers.size, 0);
    root.children[1].emit('pointerenter'); assert.equal(f.active(root), 1);
    root.emit('pointerleave'); assert.equal(f.timers.size, 1);
    const input = new Element(f.document); root.children[1].append(input); input.focus();
    assert.equal(f.timers.size, 0);
    root.children[2].emit('pointerenter'); root.children[0].emit('pointerdown');
    assert.equal(f.active(root), 1, 'Pointer activation must not hide the currently focused controls.');
    f.document.activeElement = null; root.emit('focusout'); f.flush();
    assert.equal(f.timers.size, 1);
    f.document.hidden = true; f.document.emit('visibilitychange'); assert.equal(f.timers.size, 0);
    f.document.hidden = false; f.document.emit('visibilitychange'); assert.equal(f.timers.size, 1);
    dispose(root);
});

test('the presentation frame pauses rotation while its sibling pause control is hovered or focused', () => {
    const f = fixture(), root = f.stage();
    const frame = new Element(f.document, '', ['f-offer-presentation']);
    const controls = new Element(f.document, '', ['f-offer-controls']);
    const pause = new Element(f.document, '', ['f-button', 'f-button-quiet']);
    controls.append(pause); frame.append(root, controls);
    synchronize(root, null);
    assert.equal(f.timers.size, 1);
    frame.emit('pointerenter'); assert.equal(f.timers.size, 0);
    frame.emit('pointerleave'); assert.equal(f.timers.size, 1);
    pause.focus(); assert.equal(f.timers.size, 0);
    f.tick(); assert.equal(f.active(root), 0);
    synchronize(root, null, false);
    f.document.activeElement = null; frame.emit('focusout'); f.flush();
    assert.equal(f.timers.size, 0, 'A paused stage resumed when focus left its pause control.');
    synchronize(root, null, true);
    assert.equal(f.timers.size, 1);
    pause.focus(); assert.equal(f.timers.size, 0);
    f.document.activeElement = null; frame.emit('focusout'); f.flush();
    assert.equal(f.timers.size, 1);
    f.tick(); assert.equal(f.active(root), 1);
    dispose(root);
    assert.equal(frame.listenerCount(), 0);
    assert.equal(controls.listenerCount(), 0);
    assert.equal(f.timers.size, 0);
});

test('only matching native fragment links in the chosen scope activate offers and modifiers remain untouched', () => {
    const f = fixture(), root = f.stage();
    const links = f.scope('prices', [
        '#offer-2', 'https://elsewhere.test/pricing/?lang=pt#offer-1',
        'https://product.test/other/?lang=pt#offer-1', 'https://product.test/pricing/?lang=en#offer-1',
        '#unknown', '#%invalid', '#offer-1'
    ]).children;
    const outside = f.scope('outside', ['#offer-1']).children[0];
    synchronize(root, 'prices');
    for (const link of [...links.slice(1, 6), outside]) {
        assert.equal(link.listenerCount(), 0);
        assert.equal(click(link).defaultPrevented, false);
    }
    for (const overrides of [{ ctrlKey: true }, { metaKey: true }, { shiftKey: true }, { altKey: true }, { button: 1 }, { defaultPrevented: true }]) {
        click(links[0], overrides); assert.equal(f.active(root), 0); assert.equal(f.document.activeElement, null);
    }
    const event = click(links[0]);
    assert.equal(event.defaultPrevented, false, 'Native hash navigation must never be prevented.');
    assert.equal(f.active(root), 2);
    assert.equal(f.document.activeElement, root.children[2]);
    assert.deepEqual(root.children[2].focusOptions, { preventScroll: true });
    assert.equal(f.timers.size, 0);
    click(links[6]); assert.equal(f.active(root), 1);
    dispose(root);
});

test('same configuration is idempotent while changed configuration releases and reconnects one controller', () => {
    const f = fixture(), root = f.stage();
    const link = f.scope('prices', ['#offer-1']).children[0];
    synchronize(root, 'prices', true, 2200);
    const timer = [...f.timers.keys()][0], observers = f.observers.length, listeners = link.listenerCount();
    synchronize(root, 'prices', true, 2200);
    assert.equal([...f.timers.keys()][0], timer);
    assert.equal(f.observers.length, observers); assert.equal(link.listenerCount(), listeners);
    root.children[2].emit('pointerdown'); assert.equal(f.active(root), 2);
    synchronize(root, 'prices', false, 3000);
    assert.equal(f.timers.size, 0); assert.equal(f.active(root), 2);
    assert.equal(f.observers.filter(observer => observer.connected).length, 1);
    assert.equal(link.listenerCount(), listeners);
    synchronize(root, 'prices', true, 3000);
    assert.equal(f.timers.size, 1); assert.equal([...f.timers.values()][0].duration, 3000);
    dispose(root); dispose(root);
    assert.equal(link.listenerCount(), 0); assert.equal(f.document.listenerCount(), 0);
    assert.equal(f.observers.filter(observer => observer.connected).length, 0);
});

test('dynamic card replacement and removal rebind actual targets and clean removed card state', () => {
    const f = fixture(), root = f.stage();
    synchronize(root, null);
    const removed = root.children[2];
    root.children[1].emit('pointerenter'); assert.equal(f.active(root), 1);
    root.children = [root.children[0], root.children[1]];
    f.mutate();
    assert.equal(root.style.getPropertyValue('--f-offer-columns'), '3');
    assert.equal(f.active(root), 1);
    assert.equal(removed.dataset.active, undefined); assert.equal(removed.listenerCount(), 0);
    root.append(f.card('new-target'));
    f.mutate();
    assert.equal(root.style.getPropertyValue('--f-offer-columns'), '4');
    root.children[2].emit('pointerdown'); assert.equal(f.active(root), 2);
    assert.equal(f.observers.filter(observer => observer.connected).length, 1);
    dispose(root);
});

test('replaced link nodes reconnect scoped actions without leaking listeners onto removed links', () => {
    const f = fixture(), root = f.stage(), scope = f.scope('prices', ['#offer-1']);
    synchronize(root, 'prices');
    const old = scope.children[0];
    const replacement = Object.assign(new Element(f.document), { href: '#offer-2' });
    scope.children = [replacement]; replacement.parent = scope;
    f.mutate();
    assert.equal(old.listenerCount(), 0);
    assert.equal(replacement.listenerCount(), 1);
    click(replacement); assert.equal(f.active(root), 2);
    dispose(root); assert.equal(replacement.listenerCount(), 0);
});

test('invalid target collections and disconnected stages keep fully static content', () => {
    for (const kind of ['empty', 'missing', 'duplicate', 'disconnected']) {
        const f = fixture(), root = f.stage();
        if (kind === 'empty') root.children = [];
        if (kind === 'missing') root.children[0].id = '';
        if (kind === 'duplicate') root.children[1].id = root.children[0].id;
        if (kind === 'disconnected') root.isConnected = false;
        synchronize(root, null);
        assert.equal(root.dataset.offerReady, undefined, kind);
        assert.ok(root.children.every(card => card.dataset.active === undefined), kind);
        assert.equal(f.timers.size, 0, kind);
    }
});

test('DOM removal disposes listeners timers media subscriptions and enhancement attributes', () => {
    const f = fixture(), root = f.stage();
    const link = f.scope('prices', ['#offer-2']).children[0];
    synchronize(root, 'prices');
    assert.equal(f.timers.size, 1);
    root.isConnected = false; f.mutate();
    assert.equal(f.timers.size, 0); assert.equal(link.listenerCount(), 0);
    assert.equal(f.document.listenerCount(), 0); assert.equal(f.media[0].listenerCount(), 0);
    assert.equal(root.dataset.offerReady, undefined);
    assert.ok(root.children.every(card => card.listenerCount() === 0 && card.dataset.active === undefined));
    f.changeMedia(0, true); click(link);
    assert.equal(root.dataset.offerReady, undefined);
    dispose(root);
});
