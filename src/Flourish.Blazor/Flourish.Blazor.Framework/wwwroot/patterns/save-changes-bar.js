const registrations = new Map();
const documents = new Map();
const interruption = '[data-f-edit-interruption]';
const links = 'a[href],area[href]';
const actions = 'a[href],area[href],button,input[type=button],input[type=reset],input[type=submit],input[type=image],[role=button],[role=link],summary';
const ownedAttributes = ['aria-disabled', 'tabindex', 'data-f-edit-blocked'];
const clearance = '--f-save-changes-clearance';

function withinBar(state, element) {
    return [...state.guards.keys()].some(root => root.contains(element));
}
function hashNavigation(state, link) {
    try {
        const current = new URL(state.document.defaultView.location.href);
        const destination = new URL(link.getAttribute('href'), state.document.baseURI);
        return destination.origin === current.origin && destination.pathname === current.pathname
            && destination.search === current.search && destination.hash.length > 1 && destination.hash !== current.hash;
    } catch { return false; }
}
function submitControl(element) {
    const type = (element.getAttribute('type') ?? '').toLowerCase();
    return element.localName === 'button' && !['button', 'reset'].includes(type)
        || element.localName === 'input' && ['submit', 'image'].includes(type);
}
function restore(state, element) {
    const saved = state.affected.get(element);
    if (!saved) return;
    // Disabled link buttons remove href while rendering the very same aria/
    // tabindex values as this guard. Their new state belongs to the host even
    // though an attribute-value comparison alone cannot distinguish the writer.
    const hostDisabled = !saved.disabled && element.hasAttribute('disabled')
        || saved.href !== null && !element.hasAttribute('href') && ['link','menuitem','button'].includes(element.getAttribute('role'));
    for (const [name, attribute] of saved.attributes) {
        if (hostDisabled && name !== 'data-f-edit-blocked') continue;
        // A host rerender may have already replaced our value. Never overwrite it.
        if (element.getAttribute(name) !== attribute.owned) continue;
        if (attribute.previous === null) element.removeAttribute(name);
        else element.setAttribute(name, attribute.previous);
    }
    state.affected.delete(element);
}
function mark(state, element) {
    let saved = state.affected.get(element);
    if (!saved) {
        saved = { attributes:new Map(), href:element.getAttribute('href'), disabled:element.hasAttribute('disabled') };
        state.affected.set(element, saved);
    }
    const attributes = new Map([['aria-disabled', 'true'], ['data-f-edit-blocked', '']]);
    if (element.matches(actions)) attributes.set('tabindex', '-1');
    for (const [name, value] of attributes) {
        const current = element.getAttribute(name), previous = saved.attributes.get(name);
        if (!previous) saved.attributes.set(name, { previous:current, owned:value });
        else if (current !== previous.owned) previous.previous = current;
        if (current !== value) element.setAttribute(name, value);
    }
}
function restoreClearance(guard) {
    if (guard.ownedClearance === null) return;
    const style = guard.root.style;
    if (style.getPropertyValue(clearance) === guard.ownedClearance && !style.getPropertyPriority(clearance)) {
        if (guard.previousClearance) style.setProperty(clearance, guard.previousClearance, guard.previousPriority);
        else style.removeProperty(clearance);
    }
    guard.ownedClearance = null;
}
function measure(guard) {
    if (!guard.active || !guard.root.isConnected || !guard.panel?.isConnected) return;
    const height = guard.panel.getBoundingClientRect().height;
    const bottom = Number.parseFloat(guard.state.document.defaultView.getComputedStyle(guard.panel).bottom);
    if (!Number.isFinite(height) || height <= 0) return;
    const value = `${Math.ceil(height + (Number.isFinite(bottom) ? Math.max(0, bottom) : 0) + 16)}px`;
    const style = guard.root.style, current = style.getPropertyValue(clearance), priority = style.getPropertyPriority(clearance);
    if (guard.ownedClearance === null || current !== guard.ownedClearance || priority) {
        guard.previousClearance = current; guard.previousPriority = priority;
    }
    if (current !== value || priority) style.setProperty(clearance, value);
    guard.ownedClearance = value;
}
function observePanel(guard) {
    const panel = guard.active ? guard.root.querySelector('.f-save-changes-panel') : null;
    if (guard.panel === panel) return;
    if (guard.panel) guard.resize.unobserve(guard.panel);
    guard.panel = panel;
    if (panel) { guard.resize.observe(panel); measure(guard); }
    else restoreClearance(guard);
}
function releaseGuard(guard) {
    guard.active = false;
    guard.resize.disconnect(); restoreClearance(guard);
    guard.panel = null;
    guard.state.guards.delete(guard.root); registrations.delete(guard.root);
}
function cleanup(state) {
    state.disposed = true; state.observer.disconnect();
    for (const remove of state.removers) remove();
    for (const element of [...state.affected.keys()]) restore(state, element);
    if (documents.get(state.document) === state) documents.delete(state.document);
}
function synchronize(state) {
    if (state.disposed) return;
    for (const guard of state.guards.values()) {
        if (!guard.root.isConnected) releaseGuard(guard);
        else observePanel(guard);
    }
    if (!state.guards.size) { cleanup(state); return; }
    const desired = new Set();
    if ([...state.guards.values()].some(guard => guard.active)) {
        for (const link of state.document.querySelectorAll(links))
            if (!withinBar(state, link) && !hashNavigation(state, link)) desired.add(link);
        const forms = new Set();
        for (const element of state.document.querySelectorAll(interruption)) {
            if (withinBar(state, element)) continue;
            desired.add(element);
            if (element.localName === 'form') forms.add(element);
        }
        for (const control of state.document.querySelectorAll('button,input'))
            if (submitControl(control) && forms.has(control.form) && !withinBar(state, control)) desired.add(control);
    }
    for (const element of [...state.affected.keys()]) if (!desired.has(element)) restore(state, element);
    for (const element of desired) mark(state, element);
}
function active(state) {
    if ([...state.guards.keys()].some(root => !root.isConnected)) synchronize(state);
    return [...state.guards.values()].some(guard => guard.active);
}
function elementTarget(event) {
    return event.target?.nodeType === 1 ? event.target : event.target?.parentElement;
}
function markedForm(element) {
    const form = element.form ?? element.closest('form');
    return form?.matches(interruption) ? form : null;
}
function blockedAction(state, element) {
    if (!element || withinBar(state, element)) return false;
    const link = element.closest(links);
    if (link && !withinBar(state, link) && !hashNavigation(state, link)) return true;
    const marked = element.closest(interruption);
    if (marked && marked.localName !== 'form') return true;
    const control = element.closest('button,input');
    return !!control && submitControl(control) && !!markedForm(control);
}
function stop(event) {
    event.preventDefault();
    if (event.stopImmediatePropagation) event.stopImmediatePropagation();
    else event.stopPropagation();
}
function createState(document) {
    const state = { document, guards:new Map(), affected:new Map(), removers:[], disposed:false };
    const listen = (target, name, handler) => {
        target.addEventListener(name, handler, true);
        state.removers.push(() => target.removeEventListener(name, handler, true));
    };
    const activate = event => { if (active(state) && blockedAction(state, elementTarget(event))) stop(event); };
    listen(document, 'click', activate);
    listen(document, 'auxclick', activate);
    listen(document, 'keydown', event => {
        if (!active(state) || !['Enter', ' ', 'Spacebar'].includes(event.key)) return;
        const element = elementTarget(event);
        // Spaces and multiline text remain data entry. Enter in an explicitly
        // marked form is its native submit gesture, not an edit.
        if (element?.isContentEditable || element?.localName === 'textarea') return;
        if (event.key !== 'Enter' && !element?.closest(actions)) return;
        if (blockedAction(state, element) || event.key === 'Enter' && element && !withinBar(state, element) && markedForm(element)) stop(event);
    });
    listen(document, 'submit', event => {
        if (!active(state)) return;
        const form = elementTarget(event);
        if (!form || withinBar(state, form) || event.submitter && withinBar(state, event.submitter)) return;
        if (form.matches(interruption) || event.submitter && blockedAction(state, event.submitter)) stop(event);
    });
    listen(document.defaultView, 'beforeunload', event => {
        if (!active(state)) return;
        // Browsers control the availability/text of this native confirmation.
        event.preventDefault(); event.returnValue = '';
    });
    listen(document.defaultView, 'resize', () => { for (const guard of state.guards.values()) measure(guard); });
    listen(document.defaultView, 'hashchange', () => synchronize(state));
    state.observer = new MutationObserver(() => synchronize(state));
    state.observer.observe(document.documentElement, { subtree:true, childList:true, attributes:true,
        attributeFilter:['href', 'type', 'role', 'form', 'id', 'data-f-edit-interruption', ...ownedAttributes] });
    documents.set(document, state);
    return state;
}
export function initializeSaveChangesBar(element, active = false) {
    if (!element?.ownerDocument || !element.isConnected) return;
    let guard = registrations.get(element);
    if (!guard) {
        const state = documents.get(element.ownerDocument) ?? createState(element.ownerDocument);
        guard = { state, root:element, active:!!active, panel:null, ownedClearance:null };
        guard.resize = new ResizeObserver(() => measure(guard));
        registrations.set(element, guard); state.guards.set(element, guard);
    }
    guard.active = !!active;
    synchronize(guard.state);
}
export function updateSaveChangesBar(element, active) { initializeSaveChangesBar(element, active); }
export function disposeSaveChangesBar(element) {
    const guard = registrations.get(element);
    if (!guard) return;
    releaseGuard(guard); synchronize(guard.state);
}
