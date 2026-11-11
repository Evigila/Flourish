import { refreshAnchoredPopups } from '../controls.js';

const documents = new WeakMap();
const selector = '[data-page-heading]';
const overlaySelector = '[data-f-heading-overlay]';
const clipping = /^(auto|scroll|hidden|clip)$/;
const number = value => Number.parseFloat(value) || 0;

function pixels(node, name, value) {
    const key = '--f-heading-overlay-' + name, next = Math.round(value * 100) / 100 + 'px';
    if (node.style.getPropertyValue(key) !== next) node.style.setProperty(key,next);
}
function restoreControls(entry) {
    if (!entry) return;
    for (const control of entry.controls) {
        control.slot.removeAttribute('data-f-heading-reserved');
        control.anchor.removeAttribute('data-f-heading-fixed');
        for (const name of ['width','height']) control.slot.style.removeProperty('--f-heading-slot-'+name);
        for (const name of ['left','top','width']) control.anchor.style.removeProperty('--f-heading-control-'+name);
    }
    entry.pinned = false;
}
function syncControls(entry) {
    const slots = [...entry.heading.querySelectorAll('[data-f-heading-control-slot]')];
    for (const old of entry.controls) if (!slots.includes(old.slot)) restoreControls({controls:[old]});
    entry.controls = slots.map(slot => {
        const anchor = slot.querySelector('[data-f-heading-control-anchor]'), content = anchor?.firstElementChild;
        const old = entry.controls.find(control => control.slot === slot && control.content === content);
        if (old) return old;
        entry.dirty = true;
        return {slot,anchor,content,kind:slot.getAttribute('data-f-heading-control-slot')};
    }).filter(control => control.content);
}
function measureControls(entry) {
    restoreControls(entry);
    for (const control of entry.controls) {
        const rect = control.slot.getBoundingClientRect();
        control.width = rect.width; control.height = rect.height;
        control.available = control.content.getClientRects().length > 0 && rect.width > 0 && rect.height > 0;
    }
    entry.width = entry.heading.getBoundingClientRect().width;
    entry.dirty = false; entry.measured = true;
}
function assign(node,key,value) {
    const text = Math.round(value*100)/100+'px';
    if (node.style.getPropertyValue(key) === text) return false;
    node.style.setProperty(key,text);
    return true;
}
function pinControls(entry,state,bounds,left,right,paddingLeft,paddingRight,observed) {
    let moved = !entry.pinned;
    if (entry.dirty || !entry.measured) measureControls(entry);
    const innerWidth = Math.max(0,right-left-paddingLeft-paddingRight);
    const parent = entry.controls.find(control => control.kind === 'parent' && control.available);
    const actions = entry.controls.find(control => control.kind === 'actions' && control.available);
    const parentWidth = parent ? Math.min(48,Math.max(0,innerWidth)) : 0;
    const actionWidth = actions ? Math.min(actions.width,innerWidth) : 0;
    const stacked = actions && parentWidth+actionWidth+160+(parent ? 24 : 12) > innerWidth;
    if(stacked) { if(!entry.overlay.hasAttribute('data-f-heading-stacked')) entry.overlay.setAttribute('data-f-heading-stacked',''); }
    else entry.overlay.removeAttribute('data-f-heading-stacked');
    for(const kind of ['parent','actions']) {
        const control = kind === 'parent' ? parent : actions;
        const space = entry.overlay.querySelector('[data-f-heading-'+kind+'-space]');
        if(!space) continue;
        const hidden = !control;
        if(space.hidden !== hidden) space.hidden = hidden;
        if(!control) continue;
        const width = kind === 'parent' ? parentWidth : stacked ? innerWidth : actionWidth;
        assign(control.slot,'--f-heading-slot-width',control.width);
        assign(control.slot,'--f-heading-slot-height',control.height);
        if(!control.slot.hasAttribute('data-f-heading-reserved')) control.slot.setAttribute('data-f-heading-reserved','');
        moved = assign(control.anchor,'--f-heading-control-width',width) || moved;
        // Use an untransformed fixed origin. Only the decorative panel animates.
        if(!control.anchor.hasAttribute('data-f-heading-fixed')) {
            assign(control.anchor,'--f-heading-control-left',0);
            assign(control.anchor,'--f-heading-control-top',0);
            control.anchor.setAttribute('data-f-heading-fixed','');
        }
        const anchored = control.anchor.getBoundingClientRect();
        control.origin = {left:anchored.left-number(control.anchor.style.getPropertyValue('--f-heading-control-left')),
            top:anchored.top-number(control.anchor.style.getPropertyValue('--f-heading-control-top'))};
        assign(entry.overlay,'--f-heading-'+kind+'-width',width);
        assign(entry.overlay,'--f-heading-'+kind+'-height',control.content.getBoundingClientRect().height);
        observed.add(control.content); observed.add(control.slot);
    }
    // offsetLeft/Top read layout, not the decorative panel's in-flight transform.
    for(const control of [parent,actions].filter(Boolean)) {
        const space = entry.overlay.querySelector('[data-f-heading-'+control.kind+'-space]');
        moved = assign(control.anchor,'--f-heading-control-left',left+space.offsetLeft-control.origin.left) || moved;
        moved = assign(control.anchor,'--f-heading-control-top',bounds.top+space.offsetTop-control.origin.top) || moved;
    }
    entry.pinned = true;
    if(moved) refreshAnchoredPopups(entry.heading);
}
function release(overlay,entry) {
    const wasPinned = entry?.pinned;
    restoreControls(entry);
    if (wasPinned) refreshAnchoredPopups(entry.heading);
    overlay.removeAttribute('data-f-heading-stacked');
    for(const kind of ['parent','actions']) for(const size of ['width','height'])
        overlay.style.removeProperty('--f-heading-'+kind+'-'+size);
    overlay.removeAttribute('data-f-heading-visible');
    overlay.removeAttribute('data-f-heading-enhanced');
    for (const name of ['top','left','width','limit','padding-left','padding-right'])
        overlay.style.removeProperty('--f-heading-overlay-' + name);
}
function geometry(heading, state) {
    const { win, doc } = state, viewport = win.visualViewport;
    const bounds = {left:viewport?.offsetLeft ?? 0,top:viewport?.offsetTop ?? 0};
    bounds.right = bounds.left + (viewport?.width ?? doc.documentElement.clientWidth);
    bounds.bottom = bounds.top + (viewport?.height ?? win.innerHeight);
    let owner = doc, node = heading.parentElement;
    const ancestors = [];
    while (node && node !== doc.documentElement) {
        const style = win.getComputedStyle(node), x = clipping.test(style.overflowX), y = clipping.test(style.overflowY);
        ancestors.push(node);
        if (x || y) {
            const box = node.getBoundingClientRect();
            const left = box.left + (node.clientLeft || 0), top = box.top + (node.clientTop || 0);
            if (x) { bounds.left = Math.max(bounds.left,left); bounds.right = Math.min(bounds.right,left + node.clientWidth); }
            if (y) { bounds.top = Math.max(bounds.top,top); bounds.bottom = Math.min(bounds.bottom,top + node.clientHeight); }
        }
        if (owner === doc && /^(auto|scroll)$/.test(style.overflowY)) owner = node;
        node = node.parentElement;
    }
    return { bounds, owner, ancestors };
}
function update(state) {
    state.frame = 0;
    const current = new Set(), observed = new Set(), owners = new Set();
    for (const heading of state.doc.querySelectorAll(selector)) {
        const overlay = heading.querySelector(overlaySelector);
        if (!overlay) continue;
        current.add(overlay);
        let entry = state.entries.get(overlay);
        if(!entry) { entry={heading,overlay,controls:[],dirty:true,pinned:false}; state.entries.set(overlay,entry); }
        syncControls(entry);
        const previouslyPinned = entry.pinned;
        if(entry.pinned && (entry.dirty || entry.width !== heading.getBoundingClientRect().width)) measureControls(entry);
        if (heading.getAttribute('data-heading-mode') === 'compact' || heading.closest('.f-document-surface')
            || heading.closest('[hidden],[inert],dialog:not([open])') || !heading.getClientRects().length
            || state.win.getComputedStyle(heading).visibility !== 'visible') { release(overlay,entry); continue; }
        const { bounds, owner, ancestors } = geometry(heading,state);
        observed.add(heading);
        ancestors.forEach(node => observed.add(node));
        // A scroll region has one page identity; nested samples must not pile up fixed headings.
        if (owners.has(owner)) { release(overlay,entry); continue; }
        owners.add(owner);
        const box = heading.getBoundingClientRect(), page = heading.closest('.f-page-body');
        if (page) bounds.bottom = Math.min(bounds.bottom,page.getBoundingClientRect().bottom);
        const left = Math.max(bounds.left,box.left), right = Math.min(bounds.right,box.right);
        if (right <= left || bounds.bottom <= bounds.top || !heading.querySelector('h1')?.textContent.trim()) {
            release(overlay,entry); continue;
        }
        if (!overlay.hasAttribute('data-f-heading-enhanced')) overlay.setAttribute('data-f-heading-enhanced','');
        const origin = overlay.getBoundingClientRect(), style = state.win.getComputedStyle(heading);
        pixels(overlay,'left',left-origin.left);
        pixels(overlay,'top',bounds.top-origin.top);
        pixels(overlay,'width',right-left);
        pixels(overlay,'limit',bounds.bottom-bounds.top);
        pixels(overlay,'padding-left',Math.max(0,number(style.paddingLeft)-(left-box.left)));
        pixels(overlay,'padding-right',Math.max(0,number(style.paddingRight)-(box.right-right)));
        const focusedAbove = previouslyPinned && entry.controls.some(control => control.content.contains(state.doc.activeElement)
            && (control.slot.getBoundingClientRect().top < bounds.top || control.slot.getBoundingClientRect().bottom > bounds.bottom));
        if (box.bottom <= bounds.top || focusedAbove) {
            if (!overlay.hasAttribute('data-f-heading-visible')) overlay.setAttribute('data-f-heading-visible','');
            pinControls(entry,state,bounds,left,right,Math.max(0,number(style.paddingLeft)-(left-box.left)),
                Math.max(0,number(style.paddingRight)-(box.right-right)),observed);
        } else {
            overlay.removeAttribute('data-f-heading-visible');
            const wasPinned = previouslyPinned; restoreControls(entry);
            entry.dirty = true;
            if(wasPinned) refreshAnchoredPopups(heading);
        }
    }
    for (const overlay of state.overlays) if (!current.has(overlay)) { release(overlay,state.entries.get(overlay)); state.entries.delete(overlay); }
    for (const node of state.observed) if (!observed.has(node)) state.resize.unobserve(node);
    for (const node of observed) if (!state.observed.has(node)) state.resize.observe(node);
    state.overlays = current; state.observed = observed;
    if (!current.size) dispose(state);
}
function dispose(state) {
    state.disposed = true;
    if (state.frame) state.win.cancelAnimationFrame(state.frame);
    state.resize.disconnect(); state.mutation.disconnect();
    state.removers.forEach(remove => remove());
    state.overlays.forEach(overlay => release(overlay,state.entries.get(overlay)));
    state.entries.clear();
    documents.delete(state.doc);
}
export function initializePageHeadings(root = document) {
    const doc = root.ownerDocument ?? root;
    let state = documents.get(doc);
    if (state) { state.schedule(); return; }
    if (!doc.querySelector(overlaySelector)) return;
    const win = doc.defaultView;
    state = {doc,win,frame:0,overlays:new Set(),observed:new Set(),removers:[],entries:new Map()};
    state.schedule = () => { if (!state.disposed && !state.frame) state.frame = win.requestAnimationFrame(() => update(state)); };
    const listen = (target,name,options) => {
        target.addEventListener(name,state.schedule,options);
        state.removers.push(() => target.removeEventListener(name,state.schedule,options));
    };
    const changed = () => { state.entries.forEach(entry => entry.dirty=true); state.schedule(); };
    state.resize = new win.ResizeObserver(changed);
    state.mutation = new win.MutationObserver(changed);
    state.mutation.observe(doc.documentElement,{childList:true,subtree:true,characterData:true,attributes:true,
        attributeFilter:['hidden','inert','open','class','data-heading-mode']});
    listen(doc,'scroll',{capture:true,passive:true});
    listen(win,'resize',{passive:true});
    listen(doc,'enhancedload');
    listen(doc,'focusin');
    listen(doc,'focusout');
    listen(doc,'animationend');
    listen(doc,'transitionend');
    if (win.visualViewport) {
        listen(win.visualViewport,'resize',{passive:true});
        listen(win.visualViewport,'scroll',{passive:true});
    }
    documents.set(doc,state);
    update(state);
}
