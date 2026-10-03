// Widths belong to a component instance, never to a translated header or a global table preference.
const instances = new Map();
const canvas = document.createElement('canvas');
const context = canvas.getContext('2d');
const minimum = 72;
const naturalMaximum = 260;
const manualMaximum = 100000;

function bounded(value) { return Math.max(minimum, Math.min(manualMaximum, Number.isFinite(value) ? value : minimum)); }
function columns(table) { return [...table.querySelectorAll('col[data-f-column]')]; }
function columnCells(table, key) { return [...table.querySelectorAll('th[data-f-column],td[data-f-column]')].filter(cell => cell.dataset.fColumn === key); }
function naturalWidth(table, key) {
    let width = minimum;
    for (const cell of columnCells(table, key)) {
        const text = cell.querySelector('.f-data-sort > span:first-child,.f-data-cell') ?? cell;
        const style = getComputedStyle(text);
        if (context) context.font = `${style.fontWeight} ${style.fontSize} ${style.fontFamily}`;
        const measured = context ? context.measureText(text.textContent.trim()).width : text.textContent.trim().length * 9;
        width = Math.max(width, measured + (cell.tagName === 'TH' ? 48 : 32));
    }
    return Math.min(naturalMaximum, Math.ceil(width));
}
function refreshMinimum(table) {
    const total = columns(table).reduce((sum, col) => sum + (parseFloat(col.style.width) || minimum), 0);
    table.style.minWidth = `${total + (table.querySelector('.f-data-action-column') ? 80 : 0)}px`;
}
function applyWidth(state, key, value, remember) {
    const table = state.root.querySelector('[data-f-table]');
    if (!table) return;
    const width = bounded(value);
    if (remember) state.widths.set(key, width);
    for (const col of columns(table).filter(col => col.dataset.fColumn === key)) col.style.width = `${width}px`;
    for (const button of table.querySelectorAll('[data-f-resize]')) {
        if (button.dataset.fResize !== key) continue;
        button.setAttribute('aria-valuenow', String(width));
        button.setAttribute('aria-valuetext', `${width} px`);
    }
    refreshMinimum(table);
}
function cancelDrag(state, restore) {
    const drag = state.drag;
    if (!drag) return;
    if (restore) {
        if (drag.previous === undefined) state.widths.delete(drag.key); else state.widths.set(drag.key, drag.previous);
        applyWidth(state, drag.key, drag.startWidth, false);
    }
    state.drag = null;
    state.root.classList.remove('f-data-resizing');
    if (drag.button.hasPointerCapture?.(drag.pointerId)) drag.button.releasePointerCapture(drag.pointerId);
}
function connect(root, instanceId) {
    const state = { root, widths: new Map(), drag: null, listeners: [] };
    const on = (target, name, handler, options) => {
        target.addEventListener(name, handler, options);
        state.listeners.push(() => target.removeEventListener(name, handler, options));
    };
    on(root, 'pointerdown', event => {
        const button = event.target.closest?.('[data-f-resize]');
        if (!button || event.button !== 0) return;
        event.preventDefault();
        event.stopPropagation();
        cancelDrag(state, false);
        const key = button.dataset.fResize;
        state.drag = { button, key, pointerId: event.pointerId, startX: event.clientX,
            startWidth: Number(button.getAttribute('aria-valuenow')), previous: state.widths.get(key) };
        root.classList.add('f-data-resizing');
        button.focus({ preventScroll: true });
        button.setPointerCapture(event.pointerId);
    });
    on(root, 'pointermove', event => {
        if (!state.drag || state.drag.pointerId !== event.pointerId) return;
        applyWidth(state, state.drag.key, state.drag.startWidth + event.clientX - state.drag.startX, true);
    });
    on(root, 'pointerup', event => { if (state.drag?.pointerId === event.pointerId) cancelDrag(state, false); });
    on(root, 'pointercancel', event => { if (state.drag?.pointerId === event.pointerId) cancelDrag(state, true); });
    on(root, 'lostpointercapture', event => { if (state.drag?.pointerId === event.pointerId) cancelDrag(state, true); });
    on(root, 'keydown', event => {
        if (event.key === 'Escape') {
            cancelDrag(state, true);
            const details = root.querySelector('.f-data-display[open]');
            if (details) { details.open = false; details.querySelector('summary')?.focus(); event.preventDefault(); }
        }
        const page = event.target.closest?.('[data-f-page]');
        if (page && event.key === 'Enter') {
            // Prevent a surrounding EditForm submit; dispatch the same page-change path as blur.
            event.preventDefault();
            page.dispatchEvent(new Event('change', { bubbles: true }));
            return;
        }
        const button = event.target.closest?.('[data-f-resize]');
        if (!button) return;
        const key = button.dataset.fResize;
        if (event.key === 'ArrowLeft' || event.key === 'ArrowRight') {
            event.preventDefault(); event.stopPropagation();
            const delta = (event.shiftKey ? 50 : 10) * (event.key === 'ArrowRight' ? 1 : -1);
            applyWidth(state, key, Number(button.getAttribute('aria-valuenow')) + delta, true);
        } else if (event.key === 'Home') {
            event.preventDefault(); event.stopPropagation();
            state.widths.delete(key);
            const table = root.querySelector('[data-f-table]');
            if (table) applyWidth(state, key, naturalWidth(table, key), false);
        }
    }, true);
    on(document, 'pointerdown', event => {
        const details = root.querySelector('.f-data-display[open]');
        if (details && !details.contains(event.target)) details.open = false;
    });
    instances.set(instanceId, state);
    return state;
}
export function synchronize(root, instanceId) {
    if (!root?.isConnected) return;
    let state = instances.get(instanceId);
    if (state && state.root !== root) { detach(instanceId); state = null; }
    state ??= connect(root, instanceId);
    const table = root.querySelector('[data-f-table]');
    if (!table) return;
    // Canvas measurement avoids a forced layout for every text cell.
    for (const col of columns(table)) {
        const key = col.dataset.fColumn;
        applyWidth(state, key, state.widths.get(key) ?? naturalWidth(table, key), false);
    }
    table.setAttribute('data-f-sized', '');
}
export function detach(instanceId) {
    const state = instances.get(instanceId);
    if (!state) return;
    cancelDrag(state, false);
    state.listeners.forEach(remove => remove());
    instances.delete(instanceId);
}
