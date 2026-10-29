import assert from 'node:assert/strict';
import { readFile } from 'node:fs/promises';
import test from 'node:test';

const source = await readFile(new URL('../../src/Flourish.Blazor/Flourish.Blazor.Framework/wwwroot/patterns/save-changes-bar.js', import.meta.url), 'utf8');
const api = await import(`data:text/javascript;base64,${Buffer.from(source).toString('base64')}`);

class Element {
    constructor(tag = 'div', attributes = {}) {
        this.nodeType = 1; this.localName = tag; this.attributes = new Map(Object.entries(attributes));
        this.children = []; this.events = new Map(); this.writes = 0; this.height = 80; this.bottom = '16px';
        const values = new Map(), priorities = new Map();
        this.style = { getPropertyValue:name => values.get(name) ?? '', getPropertyPriority:name => priorities.get(name) ?? '',
            setProperty:(name,value,priority = '') => { values.set(name,String(value)); priorities.set(name,priority); },
            removeProperty:name => { values.delete(name); priorities.delete(name); } };
    }
    get isConnected() { return this === this.ownerDocument?.documentElement || !!this.parentElement?.isConnected; }
    get form() {
        const id = this.getAttribute('form');
        return id ? this.ownerDocument.querySelectorAll('form').find(form => form.getAttribute('id') === id) : this.closest('form');
    }
    getAttribute(name) { return this.attributes.get(name) ?? null; }
    hasAttribute(name) { return this.attributes.has(name); }
    setAttribute(name, value) { this.writes++; this.attributes.set(name, String(value)); }
    removeAttribute(name) { this.writes++; this.attributes.delete(name); }
    matches(selector) {
        return selector.split(',').some(part => {
            if (part.startsWith('.')) return (this.getAttribute('class') ?? '').split(' ').includes(part.slice(1));
            const match = /^([a-z]+)?(?:\[([\w-]+)(?:=([^\]]+))?\])?$/.exec(part);
            return !!match && (!match[1] || match[1] === this.localName)
                && (!match[2] || this.hasAttribute(match[2]) && (match[3] === undefined || this.getAttribute(match[2]) === match[3]));
        });
    }
    closest(selector) { return this.matches(selector) ? this : this.parentElement?.closest(selector) ?? null; }
    contains(element) { return element === this || this.children.some(child => child.contains(element)); }
    append(...elements) {
        for (const element of elements) {
            element.remove(); element.parentElement = this; element.adopt(this.ownerDocument); this.children.push(element);
        }
        return this;
    }
    adopt(document) { this.ownerDocument = document; for (const child of this.children) child.adopt(document); }
    remove() { if (this.parentElement) this.parentElement.children = this.parentElement.children.filter(child => child !== this); this.parentElement = null; }
    querySelectorAll(selector) { return this.children.flatMap(child => [...(child.matches(selector) ? [child] : []), ...child.querySelectorAll(selector)]); }
    querySelector(selector) { return this.querySelectorAll(selector)[0] ?? null; }
    getBoundingClientRect() { this.reads = (this.reads ?? 0) + 1; return { height:this.height }; }
    addEventListener(type, handler, capture) { const listeners = this.events.get(type) ?? []; listeners.push({ handler,capture }); this.events.set(type,listeners); }
    removeEventListener(type, handler, capture) { this.events.set(type,(this.events.get(type) ?? []).filter(listener => listener.handler !== handler || listener.capture !== capture)); }
    emit(type, target = this, extra = {}) {
        const event = { type,target,defaultPrevented:false,stopped:false,returnValue:undefined,
            preventDefault() { this.defaultPrevented = true; }, stopImmediatePropagation() { this.stopped = true; }, ...extra };
        for (const { handler } of [...this.events.get(type) ?? []]) { handler(event); if (event.stopped) break; }
        return event;
    }
}

function fixture() {
    const previous = new Map(['MutationObserver','ResizeObserver'].map(name => [name,globalThis[name]])), observers = [], sizes = [];
    globalThis.MutationObserver = class {
        constructor(callback) { this.callback = callback; this.disconnected = false; observers.push(this); }
        observe(target,options) { this.target = target; this.options = options; }
        disconnect() { this.disconnected = true; }
    };
    globalThis.ResizeObserver = class {
        constructor(callback) { this.callback = callback; this.observed = new Set(); sizes.push(this); }
        observe(target) { this.observed.add(target); }
        unobserve(target) { this.observed.delete(target); }
        disconnect() { this.observed.clear(); this.disconnected = true; }
    };
    const document = new Element('document'), window = new Element('window');
    document.defaultView = window; window.location = new URL('https://portal.test/editor/?id=1#current');
    document.baseURI = window.location.href; window.getComputedStyle = element => ({ bottom:element.bottom });
    document.documentElement = new Element('html'); document.documentElement.adopt(document);
    document.children = [document.documentElement];
    const body = new Element('body'); document.documentElement.append(body);
    const bar = new Element('section',{ 'data-f-save-changes-bar':'' }), panel = new Element('div',{ class:'f-save-changes-panel' });
    const save = new Element('button',{ type:'button' }), discard = new Element('button',{ type:'button' });
    const link = new Element('a',{ href:'/another/' }), label = new Element('span'); link.append(label);
    panel.append(save,discard); bar.append(panel); body.append(link,bar);
    return { document,window,body,bar,panel,save,discard,link,label,observers,sizes,
        activate(active = true) { api.initializeSaveChangesBar(bar,active); },
        mutate() { for (const observer of observers) if (!observer.disconnected) observer.callback([]); },
        cleanup() {
            for (const root of body.querySelectorAll('[data-f-save-changes-bar]')) api.disposeSaveChangesBar(root);
            api.disposeSaveChangesBar(bar);
            for (const [name,value] of previous) { if (value === undefined) delete globalThis[name]; else globalThis[name] = value; }
        } };
}

test('dirty/busy updates block links in capture without interfering with save/discard or typing', () => {
    const f = fixture();
    try {
        const input = new Element('input'), form = new Element('form'), submit = new Element('button'); form.append(input,submit); f.body.append(form);
        f.activate(false); assert.equal(f.link.hasAttribute('data-f-edit-blocked'),false);
        api.updateSaveChangesBar(f.bar,true);
        assert.equal(f.link.getAttribute('aria-disabled'),'true'); assert.equal(f.link.getAttribute('tabindex'),'-1');
        for (const type of ['click','auxclick']) {
            const event = f.document.emit(type,f.label); assert.equal(event.defaultPrevented,true); assert.equal(event.stopped,true);
        }
        for (const key of ['Enter',' ','Spacebar']) assert.equal(f.document.emit('keydown',f.link,{ key }).defaultPrevented,true);
        for (const target of [f.save,f.discard,input,submit]) assert.equal(f.document.emit('click',target).defaultPrevented,false);
        assert.equal(f.document.emit('keydown',input,{ key:' ' }).defaultPrevented,false);
        assert.equal(f.document.emit('keydown',input,{ key:'Enter' }).defaultPrevented,false);
        assert.equal(f.document.emit('keydown',f.link,{ key:'Escape' }).defaultPrevented,false);
        assert.equal(f.document.emit('submit',form,{ submitter:submit }).defaultPrevented,false);
        assert.ok(f.document.events.get('click').every(listener => listener.capture === true));
        api.updateSaveChangesBar(f.bar,false); assert.equal(f.link.hasAttribute('aria-disabled'),false);
        assert.equal(f.document.emit('click',f.label).defaultPrevented,false);
    } finally { f.cleanup(); }
});

for (const [href,allowed] of [['#next',true],['/editor/?id=1#next',true],['https://portal.test/editor/?id=1#next',true],
    ['#current',false],['#',false],['',false],['/editor/?id=1',false],['/editor/?id=2#next',false],
    ['/elsewhere/#next',false],['https://other.test/editor/?id=1#next',false],['javascript:history.back()',false]])
test(`only a changed nonempty fragment on the same path and query remains navigable: ${href || '(empty href)'}`, () => {
    const f = fixture();
    try {
        f.link.setAttribute('href',href); f.activate();
        assert.equal(f.document.emit('click',f.link).defaultPrevented,!allowed);
        assert.equal(f.link.hasAttribute('data-f-edit-blocked'),!allowed);
    } finally { f.cleanup(); }
});

test('opt-in interruption forms/buttons block native POST including controls associated by form id', () => {
    const f = fixture();
    try {
        const logout = new Element('form',{ id:'logout',method:'post','data-f-edit-interruption':'' });
        const button = new Element('button'), icon = new Element('span'), text = new Element('input'), textarea = new Element('textarea');
        button.append(icon); logout.append(button,text,textarea);
        const external = new Element('button',{ form:'logout' }), refresh = new Element('button',{ type:'button','data-f-edit-interruption':'' });
        const inputButton = new Element('input',{ type:'button','data-f-edit-interruption':'' });
        const normal = new Element('button',{ type:'button' }); f.body.append(logout,external,refresh,inputButton,normal); f.activate();
        for (const target of [button,external,refresh,inputButton]) {
            assert.equal(target.getAttribute('aria-disabled'),'true'); assert.equal(target.getAttribute('tabindex'),'-1');
            assert.equal(f.document.emit('click',target).defaultPrevented,true);
            assert.equal(f.document.emit('keydown',target,{ key:'Enter' }).defaultPrevented,true);
            assert.equal(f.document.emit('keydown',target,{ key:' ' }).defaultPrevented,true);
        }
        assert.equal(f.document.emit('click',icon).defaultPrevented,true);
        assert.equal(f.document.emit('submit',logout,{ submitter:button }).defaultPrevented,true);
        assert.equal(f.document.emit('submit',logout).defaultPrevented,true);
        assert.equal(f.document.emit('keydown',text,{ key:'Enter' }).defaultPrevented,true);
        assert.equal(f.document.emit('keydown',text,{ key:' ' }).defaultPrevented,false);
        assert.equal(f.document.emit('keydown',textarea,{ key:'Enter' }).defaultPrevented,false);
        assert.equal(f.document.emit('click',normal).defaultPrevented,false);
        assert.equal(text.hasAttribute('data-f-edit-blocked'),false); assert.equal(text.hasAttribute('disabled'),false);
        const editor = new Element('form'), optIn = new Element('button',{ 'data-f-edit-interruption':'' }); editor.append(optIn); f.body.append(editor);
        assert.equal(f.document.emit('submit',editor,{ submitter:optIn }).defaultPrevented,true);
    } finally { f.cleanup(); }
});

test('the registered bar subtree remains available even when its own action/form has interruption metadata', () => {
    const f = fixture();
    try {
        f.save.setAttribute('data-f-edit-interruption','');
        const link = new Element('a',{ href:'/save-and-leave/' }), form = new Element('form',{ 'data-f-edit-interruption':'' });
        form.append(f.save); f.panel.append(link,form); f.activate();
        assert.equal(f.document.emit('click',link).defaultPrevented,false);
        assert.equal(f.document.emit('click',f.save).defaultPrevented,false);
        assert.equal(f.document.emit('submit',form,{ submitter:f.save }).defaultPrevented,false);
        assert.equal(link.hasAttribute('aria-disabled'),false); assert.equal(f.save.hasAttribute('aria-disabled'),false);
    } finally { f.cleanup(); }
});

test('restoration preserves authored aria/tabindex/native disabled and newer host values without mutation loops', () => {
    const f = fixture();
    try {
        f.link.setAttribute('aria-disabled','false'); f.link.setAttribute('tabindex','3');
        const button = new Element('button',{ disabled:'disabled','aria-disabled':'true',tabindex:'7','data-f-edit-interruption':'' }); f.body.append(button); f.activate();
        const writes = f.link.writes + button.writes, reads = f.panel.reads;
        f.mutate(); f.mutate(); assert.equal(f.link.writes + button.writes,writes); assert.equal(f.panel.reads,reads,'unrelated mutations must not measure layout');
        assert.equal(button.getAttribute('disabled'),'disabled');
        f.link.setAttribute('tabindex','8'); f.link.setAttribute('aria-disabled','mixed'); f.mutate();
        assert.equal(f.link.getAttribute('tabindex'),'-1'); assert.equal(f.link.getAttribute('aria-disabled'),'true');
        button.setAttribute('disabled','changed-by-host'); api.disposeSaveChangesBar(f.bar);
        assert.equal(f.link.getAttribute('tabindex'),'8'); assert.equal(f.link.getAttribute('aria-disabled'),'mixed');
        assert.equal(button.getAttribute('disabled'),'changed-by-host'); assert.equal(button.getAttribute('aria-disabled'),'true');
        assert.equal(button.getAttribute('tabindex'),'7'); assert.equal(button.hasAttribute('data-f-edit-blocked'),false);
        assert.ok(f.observers[0].disconnected); assert.ok(f.sizes[0].disconnected);
        for (const listeners of f.document.events.values()) assert.equal(listeners.length,0);
    } finally { f.cleanup(); }
});

test('teardown does not overwrite an owner change that arrived before the mutation observer', () => {
    const f = fixture();
    try {
        f.activate(); f.link.setAttribute('aria-disabled','false'); f.link.setAttribute('tabindex','2'); api.disposeSaveChangesBar(f.bar);
        assert.equal(f.link.getAttribute('aria-disabled'),'false'); assert.equal(f.link.getAttribute('tabindex'),'2');
    } finally { f.cleanup(); }
});

for (const observe of [false,true]) test(`host link-button disable remains authoritative when its aria/tabindex equal the guard values (${observe ? 'observed' : 'before observer'})`, () => {
    const f = fixture();
    try {
        f.link.setAttribute('aria-disabled','false'); f.link.setAttribute('tabindex','4'); f.activate();
        f.link.removeAttribute('href'); f.link.setAttribute('role','link');
        f.link.setAttribute('aria-disabled','true'); f.link.setAttribute('tabindex','-1');
        if (observe) f.mutate();
        api.disposeSaveChangesBar(f.bar);
        assert.equal(f.link.getAttribute('aria-disabled'),'true'); assert.equal(f.link.getAttribute('tabindex'),'-1');
        assert.equal(f.link.getAttribute('role'),'link'); assert.equal(f.link.hasAttribute('href'),false);
        assert.equal(f.link.hasAttribute('data-f-edit-blocked'),false);
    } finally { f.cleanup(); }
});

test('host native-button disable also keeps its final aria/tabindex when an interruption marker is removed', () => {
    const f = fixture();
    try {
        const button = new Element('button',{ type:'button','data-f-edit-interruption':'' }); f.body.append(button); f.activate();
        button.setAttribute('disabled',''); button.setAttribute('aria-disabled','true'); button.setAttribute('tabindex','-1');
        button.removeAttribute('data-f-edit-interruption'); f.mutate(); api.disposeSaveChangesBar(f.bar);
        assert.equal(button.hasAttribute('disabled'),true); assert.equal(button.getAttribute('aria-disabled'),'true');
        assert.equal(button.getAttribute('tabindex'),'-1'); assert.equal(button.hasAttribute('data-f-edit-blocked'),false);
    } finally { f.cleanup(); }
});

test('allowed fragment navigation updates accessible states against the actual new location', () => {
    const f = fixture();
    try {
        f.link.setAttribute('href','#next'); const previous = new Element('a',{ href:'#current' }); f.body.append(previous); f.activate();
        assert.equal(f.link.hasAttribute('aria-disabled'),false); assert.equal(previous.getAttribute('aria-disabled'),'true');
        f.window.location.hash = '#next'; f.document.baseURI = f.window.location.href; f.window.emit('hashchange');
        assert.equal(f.link.getAttribute('aria-disabled'),'true'); assert.equal(previous.hasAttribute('aria-disabled'),false);
        assert.equal(f.document.emit('click',previous).defaultPrevented,false);
    } finally { f.cleanup(); }
});

test('multiple guards aggregate per document and disposing/cleaning one cannot release another', () => {
    const f = fixture();
    try {
        const second = new Element('section',{ 'data-f-save-changes-bar':'' }), secondSave = new Element('button'); second.append(secondSave); f.body.append(second);
        f.activate(); api.initializeSaveChangesBar(f.bar,true); api.initializeSaveChangesBar(second,true);
        assert.equal(f.document.events.get('click').length,1); assert.equal(f.observers.length,1);
        api.updateSaveChangesBar(f.bar,false); assert.equal(f.document.emit('click',f.link).defaultPrevented,true);
        api.disposeSaveChangesBar(f.bar); assert.equal(f.document.emit('click',f.link).defaultPrevented,true);
        assert.equal(f.document.emit('click',secondSave).defaultPrevented,false);
        api.disposeSaveChangesBar(second); assert.equal(f.document.emit('click',f.link).defaultPrevented,false);
        assert.equal(f.link.hasAttribute('tabindex'),false); assert.equal(f.window.events.get('beforeunload').length,0);
        api.disposeSaveChangesBar(second);
    } finally { f.cleanup(); }
});

test('new/replaced navigation, marker changes and removed bars reconcile without server calls', () => {
    const f = fixture();
    try {
        f.activate(); const replacement = new Element('a',{ href:'/new/' }); f.link.remove(); f.body.append(replacement); f.mutate();
        assert.equal(f.link.hasAttribute('aria-disabled'),false); assert.equal(replacement.getAttribute('aria-disabled'),'true');
        replacement.setAttribute('href','#new-section'); f.mutate(); assert.equal(replacement.hasAttribute('aria-disabled'),false);
        replacement.setAttribute('href','/other/'); assert.equal(f.document.emit('click',replacement).defaultPrevented,true,'capture uses live href before observers');
        const action = new Element('button',{ type:'button' }); f.body.append(action); f.mutate(); assert.equal(action.hasAttribute('aria-disabled'),false);
        action.setAttribute('data-f-edit-interruption',''); f.mutate(); assert.equal(action.getAttribute('aria-disabled'),'true');
        action.removeAttribute('data-f-edit-interruption'); f.mutate(); assert.equal(action.hasAttribute('aria-disabled'),false);
        f.bar.remove(); f.mutate(); assert.equal(replacement.hasAttribute('aria-disabled'),false); assert.equal(f.document.events.get('click').length,0);
        assert.ok(f.observers[0].disconnected);
    } finally { f.cleanup(); }
});

test('browser refresh/close use only native beforeunload confirmation while any guard remains active', () => {
    const f = fixture();
    try {
        f.activate(); const dirty = f.window.emit('beforeunload'); assert.equal(dirty.defaultPrevented,true); assert.equal(dirty.returnValue,'');
        api.updateSaveChangesBar(f.bar,false); const clean = f.window.emit('beforeunload'); assert.equal(clean.defaultPrevented,false); assert.equal(clean.returnValue,undefined);
        api.updateSaveChangesBar(f.bar,true); f.bar.remove(); const removed = f.window.emit('beforeunload');
        assert.equal(removed.defaultPrevented,false); assert.equal(f.link.hasAttribute('aria-disabled'),false);
    } finally { f.cleanup(); }
});

test('actual wrapped panel height and computed safe-area bottom reserve local clearance and restore author priority', () => {
    const f = fixture();
    try {
        const property = '--f-save-changes-clearance'; f.bar.style.setProperty(property,'9rem','important'); f.activate();
        assert.equal(f.bar.style.getPropertyValue(property),'112px'); assert.equal(f.bar.style.getPropertyPriority(property),'');
        assert.ok(f.sizes[0].observed.has(f.panel));
        f.panel.height = 154.2; f.panel.bottom = '27px'; f.sizes[0].callback(); assert.equal(f.bar.style.getPropertyValue(property),'198px');
        f.panel.bottom = '35px'; f.window.emit('resize'); assert.equal(f.bar.style.getPropertyValue(property),'206px');
        const replacement = new Element('div',{ class:'f-save-changes-panel' }); replacement.height = 100; f.panel.remove(); f.bar.append(replacement); f.mutate();
        assert.equal(f.sizes[0].observed.has(f.panel),false); assert.equal(f.sizes[0].observed.has(replacement),true);
        assert.equal(f.bar.style.getPropertyValue(property),'132px');
        api.disposeSaveChangesBar(f.bar); f.sizes[0].callback();
        assert.equal(f.bar.style.getPropertyValue(property),'9rem'); assert.equal(f.bar.style.getPropertyPriority(property),'important');
        assert.equal(f.body.style.getPropertyValue(property),'');
    } finally { f.cleanup(); }
});
