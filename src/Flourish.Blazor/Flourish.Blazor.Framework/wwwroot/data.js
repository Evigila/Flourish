// Widths belong to a component instance, never to a translated header or a global table preference.
import { initializeStatic, setReordering, detach as detachSelection } from './multi-select-box.js';

const instances = new Map();
const canvas = document.createElement('canvas');
const context = canvas.getContext('2d');
const minimum = 72;
const naturalMaximum = 320;
const manualMaximum = 100000;

function bounded(value) { return Math.max(minimum, Math.min(manualMaximum, Number.isFinite(value) ? value : minimum)); }
function columns(table) { return [...table.querySelectorAll('col[data-f-column]')].filter(column => !column.hidden); }
function columnCells(table, key) { return [...table.querySelectorAll('th[data-f-column],td[data-f-column]')].filter(cell => cell.dataset.fColumn === key); }
function naturalWidth(table, key, maximum = naturalMaximum) {
    let width = minimum;
    const pixels = value => Number.parseFloat(value) || 0;
    const insets = style => pixels(style.paddingLeft) + pixels(style.paddingRight)
        + pixels(style.borderLeftWidth) + pixels(style.borderRightWidth);
    for (const cell of columnCells(table, key)) {
        const text = cell.querySelector('.f-data-sort-text,.f-data-cell') ?? cell;
        const style = getComputedStyle(text);
        if (context) context.font = [style.fontWeight,style.fontSize,style.fontFamily].join(' ');
        const value = text.textContent.trim();
        const measured = context ? context.measureText(value).width : value.length * 9;
        const container = text.closest?.('.f-data-sort-label') ?? text;
        const interactive = text.closest?.('.f-data-sort');
        const containerStyle = getComputedStyle(container);
        const siblings = [...(container.children ?? [])].filter(child => child !== text);
        const otherWidth = siblings.reduce((sum,child) => sum + (child.getBoundingClientRect?.().width ?? 0), 0);
        const gap = siblings.length * pixels(containerStyle.columnGap);
        const resize = cell.querySelector('[data-f-resize]')?.getBoundingClientRect?.().width ?? 0;
        const spacing = Math.max(0,value.length - 1) * pixels(style.letterSpacing);
        const interactiveInsets = interactive && interactive !== container ? insets(getComputedStyle(interactive)) : 0;
        width = Math.max(width, measured + spacing + insets(getComputedStyle(cell)) + insets(containerStyle) + interactiveInsets + otherWidth + gap + resize);
    }
    return Math.min(maximum, Math.ceil(width));
}
function automaticWidth(table, col) {
    const maximum = col.dataset.fLastColumn === 'true' ? manualMaximum : naturalMaximum;
    return naturalWidth(table, col.dataset.fColumn, maximum);
}
function refreshMinimum(table) {
    const total = columns(table).reduce((sum, col) => sum + (parseFloat(col.style.width) || minimum), 0);
    const action = table.querySelector('th.f-data-actions');
    const actionWidth = action?.getBoundingClientRect?.().width ?? 0;
    table.style.minWidth = (total + actionWidth) + 'px';
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
    on(root, 'dblclick', event => {
        if (event.target.closest?.('.f-data-actions,.f-data-card-actions,a,button,input,select,textarea,summary')) return;
        const row = event.target.closest?.('.f-data-openable[data-f-native-open="true"]');
        if (!row || !root.contains(row)) return;
        // A template owns the complete transport. Invalid or ambiguous templates fail closed,
        // rather than falling through to a delayed callback that could open an arbitrary action.
        event.stopPropagation();
        const entries = row.querySelectorAll('[data-record-open]');
        if (entries.length !== 1) return;
        const entry = entries[0];
        if (entry.closest?.('[inert]') || entry.getAttribute('aria-disabled') === 'true'
            || entry.getAttribute('aria-busy') === 'true') return;
        // Activate only the declared opening transport, never an arbitrary first action.
        if (entry.tagName === 'A' && entry.getAttribute('href')) {
            entry.click();
        } else if (entry.tagName === 'FORM') {
            const submitters = entry.querySelectorAll('button[type="submit"],button:not([type]),input[type="submit"],input[type="image"]');
            if (submitters.length !== 1) return;
            const submitter = submitters[0];
            if (submitter.disabled || submitter.matches?.(':disabled') || submitter.closest?.('[inert]')
                || submitter.getAttribute('aria-disabled') === 'true' || submitter.getAttribute('aria-busy') === 'true') return;
            entry.requestSubmit(submitter);
        }
    }, true);
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
        if (event.key === 'Escape') cancelDrag(state, true);
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
            if (table) {
                const col = columns(table).find(candidate => candidate.dataset.fColumn === key);
                if (col) applyWidth(state, key, automaticWidth(table, col), false);
            }
        }
    }, true);
    instances.set(instanceId, state);
    return state;
}
export function synchronize(root, instanceId) {
    if (!root?.isConnected) return;
    let state = instances.get(instanceId);
    if (state && state.root !== root) { detach(instanceId); state = null; }
    state ??= connect(root, instanceId);
    initializeStatic(root);
    connectDirectory(state);
    const table = root.querySelector('[data-f-table]');
    if (!table) return;
    // Canvas measurement avoids a forced layout for every text cell.
    for (const col of columns(table)) {
        const key = col.dataset.fColumn;
        applyWidth(state, key, state.widths.get(key) ?? automaticWidth(table, col), false);
    }
    table.setAttribute('data-f-sized', '');
}
export function detach(instanceId) {
    const state = instances.get(instanceId);
    if (!state) return;
    cancelDrag(state, false);
    for (const display of state.root.querySelectorAll('[data-f-selection-static="true"]')) detachSelection(display.dataset.fSelectionInstance);
    state.directory?.empty?.remove();
    state.listeners.forEach(remove => remove());
    instances.delete(instanceId);
}

// Retained editor rows must not leave the visible page while native inputs are invalid.
export function canChangePage(root) {
    const table = root?.querySelector('[data-f-table]');
    if (!table) return true;
    for (const row of table.querySelectorAll('tbody > tr')) {
        if (row.hidden) continue;
        for (const input of row.querySelectorAll('input,select,textarea')) {
            if (!input.willValidate || input.checkValidity()) continue;
            input.reportValidity();
            return false;
        }
    }
    return true;
}

export function setColumnWidth(root, instanceId, key, width) {
    const state = instances.get(instanceId);
    if (!state || state.root !== root) return;
    const table = root.querySelector('[data-f-table]');
    const col = table && columns(table).find(candidate => candidate.dataset.fColumn === key);
    const declared = col || [...root.querySelectorAll('input[data-f-selection-key]')].some(choice => choice.dataset.fSelectionKey === key);
    if (!declared) return;
    if (width === null) {
        state.widths.delete(key);
        if (col) applyWidth(state, key, automaticWidth(table, col), false);
    } else if (Number.isFinite(width)) {
        state.widths.set(key, bounded(width));
        if (col) applyWidth(state, key, width, false);
    }
}

// This directory mode reads only text already rendered in visible cells. It never
// receives host records, hidden fields, independent search values or sort keys.
export function processDirectory(rows, query, columnKey, sortKey, descending, culture, searchKeys = null) {
    const normalize = value => String(value ?? '').normalize('NFD').replace(/\p{M}/gu, '').toLocaleLowerCase(culture || undefined);
    const term = normalize(query).trim();
    const matching = rows.filter(row => !term || Object.entries(row.cells)
        .some(([key, value]) => (!searchKeys || searchKeys.includes(key)) && (!columnKey || key === columnKey) && normalize(value).includes(term)));
    if (!sortKey) return matching;
    const compare = new Intl.Collator(culture || undefined, { sensitivity:'base' });
    return matching.slice().sort((left,right) => {
        const order = compare.compare(left.cells[sortKey] ?? '', right.cells[sortKey] ?? '') * (descending ? -1 : 1);
        return order || left.index - right.index;
    });
}

function updateDirectory(state) {
    const directory = state.directory;
    if (!directory) return;
    const matching = processDirectory(directory.rows, directory.query, directory.columnKey,
        directory.sortKey, directory.descending, state.root.dataset.fCulture, directory.searchKeys);
    const count = matching.length;
    const pageCount = Math.max(1, Math.ceil(count / directory.pageSize));
    directory.page = Math.max(1, Math.min(pageCount, directory.page));
    const first = count ? (directory.page - 1) * directory.pageSize : 0;
    const visible = new Set(matching.slice(first, first + directory.pageSize).map(row => row.node));
    for (const row of directory.rows) row.node.hidden = !visible.has(row.node);
    for (const row of matching) directory.body.append(row.node);
    for (const input of state.root.querySelectorAll('[data-f-page]')) {
        input.value = directory.page; input.max = pageCount;
    }
    for (const select of state.root.querySelectorAll('[data-f-page-size]')) select.value = directory.pageSize;
    for (const button of state.root.querySelectorAll('[data-f-page-previous]')) button.disabled = directory.page <= 1;
    for (const button of state.root.querySelectorAll('[data-f-page-next]')) button.disabled = directory.page >= pageCount;
    for (const total of state.root.querySelectorAll('[data-f-page-prefix]')) total.textContent = `${total.dataset.fPagePrefix} ${pageCount}`;
    for (const counter of state.root.querySelectorAll('.f-data-count')) {
        const values = [state.root.dataset.fItemsLabel, count ? first + 1 : 0,
            Math.min(first + directory.pageSize, count), state.root.dataset.fTotalLabel, count];
        const label = counter.querySelector('span');
        if (label) label.textContent = (state.root.dataset.fRangeFormat || '{1}-{2} / {3} {4}')
            .replace(/\{([0-4])\}/g, (_,index) => String(values[Number(index)]));
    }
    for (const header of directory.table.querySelectorAll('th[data-f-column]')) {
        const selected = header.dataset.fColumn === directory.sortKey;
        header.setAttribute('aria-sort', !selected ? 'none' : directory.descending ? 'descending' : 'ascending');
        const mark = header.querySelector('.f-data-sort-mark');
        if (mark) mark.remove();
        if (selected) {
            const arrow = document.createElement('span'); arrow.className = 'f-data-sort-mark';
            arrow.setAttribute('aria-hidden','true'); arrow.textContent = directory.descending ? '▼' : '▲';
            header.querySelector('.f-data-sort-label')?.append(arrow);
        }
    }
    if (directory.empty) directory.empty.hidden = count !== 0;
}

function connectDirectory(state) {
    if (state.directory || state.root.dataset?.fProgressive !== 'true') return;
    const table = state.root.querySelector('[data-f-table]');
    const body = table?.querySelector('tbody');
    if (!body) return;
    const rows = [...body.querySelectorAll('tr')].map((node,index) => ({ node,index,
        cells:Object.fromEntries([...node.querySelectorAll('td[data-f-column]')]
            .map(cell => [cell.dataset.fColumn, cell.querySelector('.f-data-cell')?.textContent ?? ''])) }));
    const searchKeys = [...state.root.querySelectorAll('[data-f-search-column] option')].map(option => option.value).filter(Boolean);
    state.directory = { table,body,rows,query:'',columnKey:'',sortKey:null,descending:false,searchKeys,
        page:1,pageSize:Math.max(1,Number(state.root.dataset.fInitialPageSize) || 10),empty:null };
    const empty = document.createElement('div'); empty.className = 'f-data-empty'; empty.setAttribute('role','status');
    empty.textContent = state.root.dataset.fEmptyMessage || ''; empty.hidden = true;
    table.closest('.f-data-scroll')?.after(empty); state.directory.empty = empty;
    const on = (name, handler) => {
        state.root.addEventListener(name, handler, true);
        state.listeners.push(() => state.root.removeEventListener(name, handler, true));
    };
    on('input', event => {
        if (!event.target.matches?.('[data-f-search-query]')) return;
        event.stopPropagation(); state.directory.query = event.target.value; state.directory.page = 1; updateDirectory(state);
    });
    on('change', event => {
        const target = event.target;
        if (target.matches?.('[data-f-search-column]')) { state.directory.columnKey = target.value; state.directory.page = 1; }
        else if (target.matches?.('[data-f-page]')) state.directory.page = Math.max(1,Number(target.value) || 1);
        else if (target.matches?.('[data-f-page-size]')) { state.directory.pageSize = Math.max(1,Number(target.value) || 10); state.directory.page = 1; }
        else return;
        event.stopPropagation(); updateDirectory(state);
    });
    on('click', event => {
        const target = event.target.closest?.('[data-f-page-previous],[data-f-page-next],[data-f-sort-key],[data-f-view]');
        if (!target || target.disabled) return;
        event.preventDefault(); event.stopPropagation();
        if (target.hasAttribute('data-f-page-previous')) state.directory.page--;
        else if (target.hasAttribute('data-f-page-next')) state.directory.page++;
        else if (target.dataset.fSortKey) {
            const key = target.dataset.fSortKey;
            if (state.directory.sortKey !== key) { state.directory.sortKey = key; state.directory.descending = true; }
            else if (state.directory.descending) state.directory.descending = false;
            else state.directory.sortKey = null;
            state.directory.page = 1;
        } else if (target.dataset.fView) {
            const cards = target.dataset.fView === 'cards';
            state.root.classList.toggle('f-data-progressive-cards', cards);
            for (const display of state.root.querySelectorAll('[data-f-selection-static="true"]')) setReordering(display,!cards);
            for (const button of state.root.querySelectorAll('[data-f-view]')) {
                const selected = button === target;
                button.classList.toggle('f-data-view-selected', selected); button.setAttribute('aria-pressed', String(selected));
                button.querySelector('span')?.remove();
                if (selected) { const label = document.createElement('span'); label.textContent = button.getAttribute('aria-label'); button.append(label); }
            }
        }
        updateDirectory(state);
    });
    on('flourish-selection-change', event => {
        if (event.target !== state.root.querySelector('[data-f-table-selection="true"]')) return;
        const {orderedKeys,selectedKeys} = event.detail ?? {};
        if (!Array.isArray(orderedKeys) || !Array.isArray(selectedKeys)) return;
        event.stopPropagation();
        const visible = new Set(selectedKeys), lastKey = orderedKeys.filter(key=>visible.has(key)).at(-1);
        for (const cell of table.querySelectorAll('[data-f-column]')) {
            cell.hidden = !visible.has(cell.dataset.fColumn);
            if (cell.dataset.fColumn === lastKey) cell.dataset.fLastColumn = 'true';
            else delete cell.dataset.fLastColumn;
        }
        const groups = [table.querySelector('colgroup'),...table.querySelectorAll('tr')];
        for (const group of groups.filter(Boolean)) {
            const spacer = group.querySelector('.f-data-spacer');
            for (const key of orderedKeys) {
                const cell = [...group.children].find(child=>child.dataset?.fColumn === key);
                if (cell) group.insertBefore(cell,spacer);
            }
        }
        synchronize(state.root,state.root.dataset.fInstance);
        updateDirectory(state);
    });
    state.root.dataset.fDirectoryEnhanced = 'true';
    updateDirectory(state);
}

export function initializeDirectories() {
    for (const root of document.querySelectorAll('[data-f-progressive="true"][data-f-instance]'))
        if (!instances.has(root.dataset.fInstance)) synchronize(root, root.dataset.fInstance);
    for (const [id,state] of instances) if (state.directory && !state.root.isConnected) detach(id);
}

if (document.readyState === 'loading') document.addEventListener('DOMContentLoaded', initializeDirectories, { once:true });
else initializeDirectories();
const directoryObserver = new MutationObserver(initializeDirectories);
directoryObserver.observe(document.documentElement, { childList:true, subtree:true });
