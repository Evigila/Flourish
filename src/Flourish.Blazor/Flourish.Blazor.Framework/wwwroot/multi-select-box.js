import { attachDisclosureMenu, closeMenu, detachMenu } from './controls.js';

const instances = new Map();
const rows = root => [...root.querySelectorAll('.f-multi-select-option')];
const input = row => row.querySelector('input[data-f-selection-key]');
const unavailable = (root,row) => root.dataset.fSelectionDisabled === 'true' || root.dataset.fCreating === 'true' || row?.dataset.fOptionDisabled === 'true';
const reorderable = (root,row) => !!row && row.isConnected && root.contains(row) && !unavailable(root,row) && root.dataset.fFiltering !== 'true' && root.dataset.fReorderEnabled !== 'false' && row.dataset.fCanReorder === 'true';
function clearDrag(state) { state.drag?.removeAttribute('data-f-dragging'); state.drag = null; }
function refresh(state) {
    const options = rows(state.root);
    const shown = options.filter(row => input(row)?.checked).length;
    const minimum = Number(state.root.dataset.fMinimumSelected) || 0;
    const maximum = Number(state.root.dataset.fMaximumSelections ?? Number.MAX_SAFE_INTEGER);
    for (const row of options) {
        const choice = input(row), enabled = reorderable(state.root,row);
        row.setAttribute('draggable',String(enabled)); row.setAttribute('tabindex',enabled ? '0' : '-1');
        row.setAttribute('aria-disabled',String(unavailable(state.root,row)));
        if (!choice) continue;
        choice.disabled = unavailable(state.root,row) || choice.dataset.fFixed === 'true' || (choice.checked && shown <= minimum) || (!choice.checked && shown >= maximum);
        choice.closest('label')?.classList.toggle('is-selected',choice.checked);
        state.previous.set(row.dataset.fSelectionKey,choice.checked);
    }
        const caption = state.trigger.querySelector('[data-f-selection-label]');
    if (caption && state.root.dataset.fSelectionCaption === undefined) {
        const selected = options.filter(row => input(row)?.checked);
        caption.textContent = selected.length === 0 ? state.root.dataset.fSelectionEmpty
            : selected.length === 1 ? selected[0].querySelector('label > span')?.textContent
            : `${selected.length.toLocaleString(document.documentElement.lang || undefined)} ${state.root.dataset.fSelectionCount}`;
    }
    if (state.root.dataset.fSelectionDisabled === 'true') closeMenu(state.trigger,state.panel,false);
    if (state.drag && !reorderable(state.root,state.drag)) clearDrag(state);
}
function publish(state) {
    const options = rows(state.root);
    const orderedKeys = options.map(row => row.dataset.fSelectionKey);
    const selectedKeys = options.filter(row => input(row)?.checked).map(row => row.dataset.fSelectionKey);
    refresh(state);
    if (state.proxy) {
        // The component validates the full snapshot before either owner can adopt it.
        state.proxy.invokeMethodAsync('ApplyAsync',orderedKeys,selectedKeys).catch(error => {
            if (instances.get(state.id) === state && state.root.isConnected) throw error;
        });
    } else state.root.dispatchEvent(new CustomEvent('flourish-selection-change',{bubbles:true,detail:{orderedKeys,selectedKeys}}));
}
function reorder(state,source,target) {
    if (source === target || !reorderable(state.root,source) || !reorderable(state.root,target)) return;
    const options = rows(state.root), movable = options.filter(row => reorderable(state.root,row));
    const destination = movable.indexOf(target); movable.splice(movable.indexOf(source),1); movable.splice(destination,0,source);
    const slots = options.map((row,index) => movable.includes(row) ? index : -1).filter(index => index >= 0);
    slots.forEach((slot,index) => { options[slot] = movable[index]; });
    for (const row of options) state.panel.append(row);
    publish(state); source.focus({preventScroll:true});
}
export function synchronize(root,id,proxy = null) {
    if (!root?.isConnected) return;
    let state = instances.get(id);
    if (state && state.root !== root) { detach(id); state = null; }
    if (!state) {
        const trigger = root.querySelector('summary'), panel = root.querySelector('.f-multi-select-panel');
        if (!trigger || !panel) return;
        state = {root,id,proxy,trigger,panel,drag:null,previous:new Map(),cleanup:[]};
        const on = (name,callback) => { root.addEventListener(name,callback,true); state.cleanup.push(()=>root.removeEventListener(name,callback,true)); };
        const option = target => { const row = target?.closest?.('.f-multi-select-option'); return row && root.contains(row) ? row : null; };
        on('change',event => {
            const row = option(event.target), choice = row && input(row);
            if (!choice || event.target !== choice) return;
            event.stopPropagation();
            if (unavailable(root,row) || choice.dataset.fFixed === 'true') { choice.checked = state.previous.get(row.dataset.fSelectionKey) ?? false; refresh(state); return; }
            const shown = rows(root).filter(candidate => input(candidate)?.checked).length;
            if (shown < (Number(root.dataset.fMinimumSelected) || 0) || shown > Number(root.dataset.fMaximumSelections ?? Number.MAX_SAFE_INTEGER)) { choice.checked = state.previous.get(row.dataset.fSelectionKey) ?? false; refresh(state); return; }
            publish(state);
        });
        on('dragstart',event => {
            clearDrag(state); const row = option(event.target);
            if (!reorderable(root,row) || !event.dataTransfer) { if (row) event.preventDefault(); return; }
            event.stopPropagation(); event.dataTransfer.effectAllowed = 'move';
            event.dataTransfer.setData('text/plain',row.dataset.fSelectionKey); state.drag = row; row.setAttribute('data-f-dragging','');
        });
        on('dragend',()=>clearDrag(state));
        on('dragover',event => {
            if (!state.drag || !reorderable(root,option(event.target))) return;
            event.preventDefault(); event.stopPropagation(); if (event.dataTransfer) event.dataTransfer.dropEffect = 'move';
        });
        on('drop',event => {
            const target = option(event.target);
            if (!state.drag || !reorderable(root,target)) return;
            event.preventDefault(); event.stopPropagation(); reorder(state,state.drag,target); clearDrag(state);
        });
        on('keydown',event => {
            if (event.key === 'Escape') { clearDrag(state); return; }
            const row = option(event.target);
            if (event.target !== row || !reorderable(root,row) || !['ArrowUp','ArrowDown'].includes(event.key)) return;
            event.preventDefault(); event.stopPropagation();
            const movable = rows(root).filter(candidate => reorderable(root,candidate));
            const target = movable[movable.indexOf(row) + (event.key === 'ArrowUp' ? -1 : 1)];
            if (target) reorder(state,row,target);
        });
        instances.set(id,state);
    }
    state.proxy = proxy;
    attachDisclosureMenu(root,state.trigger,state.panel);
    refresh(state);
}
export function setReordering(root,enabled) {
    root.dataset.fReorderEnabled = String(enabled);
    const state = instances.get(root.dataset.fSelectionInstance); if (state) refresh(state);
}
export function initializeStatic(root) {
    for (const display of root.querySelectorAll('[data-f-selection-static="true"]')) synchronize(display,display.dataset.fSelectionInstance);
}
export function detach(id) {
    const state = instances.get(id); if (!state) return;
    clearDrag(state); detachMenu(state.trigger,state.panel); state.cleanup.forEach(remove=>remove()); instances.delete(id);
}
