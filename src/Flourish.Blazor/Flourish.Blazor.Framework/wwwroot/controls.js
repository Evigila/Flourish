// Scoped DOM lifetimes adapted from the source tooltip, menu, modal and input behaviors.
import { rememberInvoker, resolveInvoker } from './primitives/interaction-origin.js';
const menus = new WeakMap();
const dialogs = new WeakMap();
const inputs = new WeakMap();
const accessForms = new WeakMap();
const tutorialBoards = new WeakMap();
let activeMenu = null;
const focusableSelector = 'a[href],button:not([disabled]),input:not([disabled]),select:not([disabled]),textarea:not([disabled]),[tabindex]:not([tabindex="-1"])';
const visible = element => element.getClientRects().length > 0 && !element.closest('[hidden],[inert]');

export function focusFirstInvalid(root) {
    const form = root?.closest('form') ?? root;
    const target = [...(form?.querySelectorAll('input.invalid,select.invalid,textarea.invalid,[aria-invalid="true"]') ?? [])].find(element => visible(element) && !element.disabled && element.matches(focusableSelector));
    if (!target) return false;
    target.scrollIntoView({block:'center',behavior:'auto'}); target.focus({preventScroll:true}); return true;
}

export function attachTextSelection(input) {
    if (!input || inputs.has(input)) return;
    let frame = null;
    const focus = () => {
        if (!(input instanceof HTMLInputElement) || !['text','email','tel','url','number'].includes(input.type)
            || input.disabled || input.readOnly || !input.value || input.dataset.inputMask !== undefined
            || input.dataset.preserveSelection === 'true') return;
        frame = requestAnimationFrame(() => { if (document.activeElement === input) input.select(); });
    };
    input.addEventListener('focus', focus);
    inputs.set(input, () => { cancelAnimationFrame(frame); input.removeEventListener('focus', focus); });
}
export function detachTextSelection(input) { inputs.get(input)?.(); inputs.delete(input); }

/** Match the visible input-to-input gap before actions while retaining visible field labels. */
export function attachAccessFormSpacing(root) {
    if (!root?.isConnected || accessForms.has(root)) return;
    const managed = new Map();
    let observed = [];
    const resize = new ResizeObserver(update);
    const mutation = new MutationObserver(update);
    function update() {
        if (!root.isConnected) { detachAccessFormSpacing(root); return; }
        const targets = new Map();
        const labels = new Set();
        const owns = element => element.closest('.f-access-form-surface') === root;
        function reserve(action, field) {
            const label = field?.querySelector(':scope > .f-field-label');
            if (!label || !owns(field)) return;
            labels.add(label);
            const height = label.getClientRects().length ? label.getBoundingClientRect().height : 0;
            const gap = Number.parseFloat(getComputedStyle(field).rowGap) || 0;
            targets.set(action, `${height > 0 ? height + gap : 0}px`);
        }
        for (const layout of root.querySelectorAll('.f-form-layout')) {
            if (!owns(layout)) continue;
            for (const action of layout.children)
                if (action.matches('.f-button,.f-inline-actions') && action.previousElementSibling?.matches('.f-field'))
                    reserve(action, action.previousElementSibling);
        }
        for (const actions of root.querySelectorAll(':scope > .f-access-form-actions')) {
            const preceding = [...root.querySelectorAll('.f-field')].filter(field => owns(field) && visible(field)
                && (field.compareDocumentPosition(actions) & Node.DOCUMENT_POSITION_FOLLOWING));
            reserve(actions, preceding.at(-1));
        }
        for (const [action, original] of managed) {
            if (targets.has(action)) continue;
            if (original) action.style.setProperty('--f-access-action-label-gap', original);
            else action.style.removeProperty('--f-access-action-label-gap');
            managed.delete(action);
        }
        for (const [action, value] of targets) {
            if (!managed.has(action)) managed.set(action, action.style.getPropertyValue('--f-access-action-label-gap'));
            if (action.style.getPropertyValue('--f-access-action-label-gap') !== value)
                action.style.setProperty('--f-access-action-label-gap', value);
        }
        const nextObserved = [root, ...labels];
        if (nextObserved.length !== observed.length || nextObserved.some((node, index) => node !== observed[index])) {
            resize.disconnect(); nextObserved.forEach(node => resize.observe(node)); observed = nextObserved;
        }
    }
    accessForms.set(root, () => {
        resize.disconnect(); mutation.disconnect();
        for (const [action, original] of managed)
            if (original) action.style.setProperty('--f-access-action-label-gap', original);
            else action.style.removeProperty('--f-access-action-label-gap');
    });
    mutation.observe(root, { childList:true, subtree:true, characterData:true, attributes:true, attributeFilter:['hidden','class'] });
    update();
}
export function detachAccessFormSpacing(root) { accessForms.get(root)?.(); accessForms.delete(root); }

function menuUnavailable(element) {
    return !element || element.disabled || element.matches(':disabled')
        || element.closest('[disabled],[aria-disabled="true"],[hidden],[inert]') !== null;
}
function menuItems(state) { return [...state.panel.querySelectorAll(state.itemSelector)].filter(element => visible(element) && !menuUnavailable(element)); }
function copyTheme(trigger, panel) {
    const style = getComputedStyle(trigger);
    // Preserve the trigger's scoped surface roles on native top-layer popups.
    const names = new Set(['font-family','font-size','font-weight','line-height','color-scheme']);
    for (let index = 0; index < style.length; index++) {
        const name = style[index];
        if (name?.startsWith('--f-')) names.add(name);
    }
    const saved = [];
    for (const name of names) {
        // A popup is a surface even when its trigger lives on dark chrome.
        const value = ['--f-target-preview','--f-row-hover'].includes(name)
            ? style.getPropertyValue('--f-surface-preview') || style.getPropertyValue(name)
            : ['--f-target-click','--f-row-active'].includes(name)
            ? style.getPropertyValue('--f-surface-click') || style.getPropertyValue(name)
            : style.getPropertyValue(name);
        if (!value) continue;
        saved.push([name,panel.style.getPropertyValue(name),panel.style.getPropertyPriority(name)]);
        panel.style.setProperty(name,value);
    }
    return () => saved.forEach(([name,value,priority]) => value ? panel.style.setProperty(name,value,priority) : panel.style.removeProperty(name));
}
function positionMenu(state) {
    if (!state.open) return;
    // Hover regions can fill an entire top-bar slot; both modes anchor to the actual control.
    const trigger = state.trigger.getBoundingClientRect();
    const panel = state.panel;
    if (state.maximumHeight) panel.style.setProperty('max-height',state.maximumHeight,state.maximumHeightPriority);
    else panel.style.removeProperty('max-height');
    const maximumHeight = getComputedStyle(panel).maxHeight;
    const menu = state.panel.getBoundingClientRect();
    const width = document.documentElement.clientWidth;
    const height = window.innerHeight;
    panel.style.left = `${Math.max(12, Math.min(trigger.right - menu.width, width - menu.width - 12))}px`;
    // Hover surfaces meet their trigger region so crossing into the menu has no dead gap.
    const gap = state.openOnHover ? 0 : 6;
    const below = Math.max(0,height - trigger.bottom - gap - 12);
    const above = Math.max(0,trigger.top - gap - 12);
    const openAbove = menu.height > below && above > below;
    const available = openAbove ? above : below;
    // A long panel scrolls on its chosen side instead of overlapping the control or leaving the viewport.
    panel.style.setProperty('max-height',maximumHeight && maximumHeight !== 'none'
        ? `min(${available}px, ${maximumHeight})` : `${available}px`);
    const panelHeight = Math.min(panel.getBoundingClientRect().height,available);
    const top = openAbove ? trigger.top - panelHeight - gap : trigger.bottom + gap;
    panel.style.top = `${Math.max(12,Math.min(top,height - panelHeight - 12))}px`;
}

/** One nonmodal tutorial entry: native top-layer board plus a passive hover/focus overview. */
export function synchronizeTutorialBoard(trigger, preview, board, open, reference) {
    if (!trigger?.isConnected || !preview?.isConnected || !board?.isConnected) return;
    if (typeof board.showPopover !== 'function' || typeof preview.showPopover !== 'function')
        throw new Error('TutorialBoard requires the native Popover API.');
    let state = tutorialBoards.get(board);
    if (!state) {
        state = {trigger,preview,board,reference,requestedOpen:false,wasOpen:false,hover:false,focus:false,timer:null,
            panel:preview,open:false,openOnHover:true,maximumHeight:preview.style.getPropertyValue('max-height'),
            maximumHeightPriority:preview.style.getPropertyPriority('max-height')};
        tutorialBoards.set(board,state);
        const hidePreview = () => {
            clearTimeout(state.timer); state.timer = null;
            if (preview.matches(':popover-open')) preview.hidePopover();
            state.open = false; trigger.removeAttribute('aria-describedby');
            state.restorePreviewTheme?.(); state.restorePreviewTheme = null;
        };
        const showPreview = () => {
            clearTimeout(state.timer); state.timer = null;
            if (trigger.disabled || state.requestedOpen || board.matches(':popover-open')) return;
            state.restorePreviewTheme?.(); state.restorePreviewTheme = copyTheme(trigger,preview);
            if (!preview.matches(':popover-open')) preview.showPopover();
            state.open = true; trigger.setAttribute('aria-describedby',preview.id); positionMenu(state);
        };
        const scheduleHide = () => {
            clearTimeout(state.timer);
            state.timer = setTimeout(() => { if (!state.hover && !state.focus) hidePreview(); },90);
        };
        const enter = () => { state.hover = true; showPreview(); };
        const leave = () => { state.hover = false; scheduleHide(); };
        const focus = () => { state.focus = true; showPreview(); };
        const blur = () => { state.focus = false; scheduleHide(); };
        const previewEscape = event => {
            if (event.key === 'Escape' && state.open) { hidePreview(); event.preventDefault(); }
        };
        const toggle = event => {
            if (event.newState === 'open') { state.wasOpen = true; hidePreview(); return; }
            if (!state.wasOpen) return;
            state.wasOpen = false; trigger.setAttribute('aria-expanded','false');
            state.restoreBoardTheme?.(); state.restoreBoardTheme = null;
            // Restore only focus owned by this board, never steal an outside-click target's focus.
            if (board.contains(document.activeElement) && trigger.isConnected) trigger.focus({preventScroll:true});
            if (state.requestedOpen) {
                state.requestedOpen = false;
                Promise.resolve(state.reference?.invokeMethodAsync('RequestCloseAsync')).catch(() => {});
            }
        };
        const reposition = () => { if (state.open) positionMenu(state); };
        const observer = new MutationObserver(() => {
            if (!trigger.isConnected || !preview.isConnected || !board.isConnected) detachTutorialBoard(trigger,preview,board);
        });
        trigger.addEventListener('pointerenter',enter); trigger.addEventListener('pointerleave',leave);
        preview.addEventListener('pointerenter',enter); preview.addEventListener('pointerleave',leave);
        trigger.addEventListener('focus',focus); trigger.addEventListener('blur',blur); trigger.addEventListener('keydown',previewEscape);
        board.addEventListener('toggle',toggle);
        window.addEventListener('resize',reposition); document.addEventListener('scroll',reposition,true);
        observer.observe(document.documentElement,{childList:true,subtree:true});
        state.hidePreview = hidePreview;
        state.cleanup = () => {
            observer.disconnect(); hidePreview();
            trigger.removeEventListener('pointerenter',enter); trigger.removeEventListener('pointerleave',leave);
            preview.removeEventListener('pointerenter',enter); preview.removeEventListener('pointerleave',leave);
            trigger.removeEventListener('focus',focus); trigger.removeEventListener('blur',blur); trigger.removeEventListener('keydown',previewEscape);
            board.removeEventListener('toggle',toggle); window.removeEventListener('resize',reposition); document.removeEventListener('scroll',reposition,true);
        };
    }
    state.reference = reference; state.requestedOpen = open;
    trigger.setAttribute('aria-expanded',open ? 'true' : 'false');
    if (open) {
        state.hidePreview();
        state.restoreBoardTheme?.(); state.restoreBoardTheme = copyTheme(trigger,board);
        if (!board.matches(':popover-open')) {
            board.showPopover(); state.wasOpen = true;
            // A modeless board has initial focus but neither a trap nor an inert/scroll-locked background.
            board.focus({preventScroll:true});
        }
    } else if (board.matches(':popover-open')) {
        if (board.contains(document.activeElement) && trigger.isConnected) trigger.focus({preventScroll:true});
        board.hidePopover(); state.wasOpen = false;
        state.restoreBoardTheme?.(); state.restoreBoardTheme = null;
    } else if (state.open) {
        state.restorePreviewTheme?.(); state.restorePreviewTheme = copyTheme(trigger,preview); positionMenu(state);
    }
}
export function detachTutorialBoard(trigger,preview,board) {
    const state = tutorialBoards.get(board);
    if (!state) return;
    tutorialBoards.delete(board); state.requestedOpen = false; state.cleanup();
    if (board.matches(':popover-open')) board.hidePopover();
    state.restoreBoardTheme?.(); trigger.setAttribute('aria-expanded','false');
}

export function attachMenu(trigger, panel, openOnHover=false) {
    if (!trigger || !panel) return;
    const existing = menus.get(panel);
    if (existing) {
        if (existing.openOnHover !== openOnHover || menuUnavailable(trigger)) closeMenu(trigger,panel,false);
        existing.openOnHover = openOnHover;
        trigger.setAttribute('aria-expanded',existing.open ? 'true' : 'false');
        if (existing.open) {
            existing.restoreTheme?.(); existing.restoreTheme = copyTheme(trigger,panel);
            positionMenu(existing);
        }
        return;
    }
    const state = { trigger, panel, hoverRoot:trigger.closest('.f-action-menu') ?? trigger, openOnHover,
        itemSelector:'[role="menuitem"]:not([disabled])', disclosure:null, dismissOnAction:true,
        open:false, restoreTheme:null, maximumHeight:'', maximumHeightPriority:'', cleanup:[] };
    const listen = (target, type, callback, options) => {
        target.addEventListener(type, callback, options);
        state.cleanup.push(() => target.removeEventListener(type, callback, options));
    };
    listen(trigger, 'keydown', event => {
        if (!['ArrowDown','ArrowUp'].includes(event.key) || menuUnavailable(trigger)) return;
        event.preventDefault();
        event.stopPropagation();
        if (!state.open) openMenu(state,true);
        const items = menuItems(state); (event.key === 'ArrowUp' ? items.at(-1) : items[0])?.focus();
    });
    listen(state.hoverRoot, 'pointerenter', event => {
        if (state.openOnHover && event.pointerType !== 'touch' && !menuUnavailable(trigger)) openMenu(state,false);
    });
    const leave = event => {
        if (!state.openOnHover || !state.open || event.pointerType === 'touch') return;
        if (state.hoverRoot.contains(event.relatedTarget) || panel.contains(event.relatedTarget)) return;
        closeMenu(trigger,panel,panel.contains(document.activeElement));
    };
    listen(state.hoverRoot, 'pointerleave', leave);
    listen(panel, 'pointerleave', leave);
    listen(panel, 'click', event => {
        const action = event.target?.closest?.('a,button,[role="menuitem"]');
        if (!action || !panel.contains(action)) return;
        if (menuUnavailable(action)) {
            event.preventDefault(); event.stopImmediatePropagation(); return;
        }
        if (!state.open || !state.dismissOnAction) return;
        // Native transport and callback dispatch complete before the shared menu dismisses.
        rememberInvoker(trigger);
        queueMicrotask(() => { if (state.open) closeMenu(trigger,panel,false); });
    }, true);
    listen(document, 'pointerdown', event => {
        if (state.open && !panel.contains(event.target) && !trigger.contains(event.target)) closeMenu(trigger,panel,false);
    }, true);
    listen(document, 'focusin', event => {
        if (state.open && !panel.contains(event.target) && !trigger.contains(event.target)) closeMenu(trigger,panel,false);
    });
    listen(document, 'keydown', event => {
        if (!state.open || event.defaultPrevented) return;
        if (event.key === 'Escape') { event.preventDefault(); event.stopPropagation(); closeMenu(trigger,panel,true); return; }
        if (!['ArrowDown','ArrowUp','Home','End'].includes(event.key)) return;
        const items = menuItems(state); if (!items.length) return;
        event.preventDefault();
        const index = items.indexOf(document.activeElement);
        const next = event.key === 'Home' ? 0 : event.key === 'End' ? items.length - 1
            : event.key === 'ArrowDown' ? (index + 1 + items.length) % items.length : (index - 1 + items.length) % items.length;
        items[next].focus();
    });
    listen(window,'resize',()=>positionMenu(state));
    listen(document,'scroll',()=>positionMenu(state),true);
    const observer = new MutationObserver(() => { if (!trigger.isConnected) detachMenu(trigger,panel); });
    observer.observe(document.body,{childList:true,subtree:true});
    state.cleanup.push(()=>observer.disconnect());
    menus.set(panel,state);
}
export function attachDisclosureMenu(disclosure, trigger, panel, dismissOnAction=false) {
    if (!disclosure || !trigger || !panel) return;
    attachMenu(trigger,panel);
    const state = menus.get(panel);
    state.dismissOnAction = dismissOnAction;
    state.itemSelector = dismissOnAction ? '[role="menuitem"]:not([disabled])' : focusableSelector;
    if (!state.disclosure) {
        state.disclosure = disclosure;
        const synchronize = () => disclosure.open ? openMenu(state,false) : closeMenu(trigger,panel,false);
        disclosure.addEventListener('toggle',synchronize);
        state.cleanup.push(()=>disclosure.removeEventListener('toggle',synchronize));
    }
    // Native summary activation owns the open state; selection options remain open, commands dismiss after dispatch.
    if (disclosure.open && !state.open) openMenu(state,false);
    else if (!disclosure.open && state.open) closeMenu(trigger,panel,false);
}
function openMenu(state, focusMenu) {
    if (state.open || menuUnavailable(state.trigger)) return;
    const {trigger,panel} = state;
    if (typeof panel.showPopover !== 'function') throw new TypeError('Native Popover API is required.');
    if (activeMenu) closeMenu(activeMenu.trigger,activeMenu.panel,false);
    state.restoreTheme = copyTheme(trigger,panel);
    state.maximumHeight = panel.style.getPropertyValue('max-height');
    state.maximumHeightPriority = panel.style.getPropertyPriority('max-height');
    state.open = true;
    activeMenu = state;
    if (state.disclosure) state.disclosure.open = true;
    panel.setAttribute('data-f-open','');
    panel.showPopover();
    trigger.setAttribute('aria-expanded','true');
    positionMenu(state);
    if (focusMenu) menuItems(state)[0]?.focus({preventScroll:true});
}
export function toggleMenu(trigger, panel, focusMenu=true) {
    if (!menus.has(panel)) attachMenu(trigger,panel);
    const state = menus.get(panel); if (!state || menuUnavailable(trigger)) return;
    if (state.open) {
        // Pointer clicks on an already hovered top-bar trigger should not flash the menu closed.
        if (state.openOnHover && !focusMenu) return;
        closeMenu(trigger,panel,true); return;
    }
    openMenu(state,focusMenu);
}
export function closeMenu(trigger,panel,restoreFocus=false) {
    const state = menus.get(panel); if (!state) return;
    const wasOpen = state.open;
    if (state.open && panel.isConnected && panel.matches(':popover-open')) panel.hidePopover();
    panel.removeAttribute('data-f-open');
    state.open = false;
    if (state.disclosure) state.disclosure.open = false;
    trigger.setAttribute('aria-expanded','false');
    state.restoreTheme?.(); state.restoreTheme = null;
    if (wasOpen) {
        if (state.maximumHeight) panel.style.setProperty('max-height',state.maximumHeight,state.maximumHeightPriority);
        else panel.style.removeProperty('max-height');
    }
    if (activeMenu === state) activeMenu = null;
    if (restoreFocus && trigger.isConnected) trigger.focus({preventScroll:true});
}
export function detachMenu(trigger,panel) {
    const state = menus.get(panel); if (!state) return;
    closeMenu(trigger,panel,false); state.cleanup.forEach(remove=>remove()); menus.delete(panel);
}

function restoreDialogFocus(state) {
    const opener = (state.opener?.isConnected ? state.opener
        : document.querySelector(`[aria-controls="${CSS.escape(state.dialog.id)}"]`))
        ?? document.querySelector(`[data-f-dialog-return-focus="${CSS.escape(state.dialog.id)}"]`);
    if (opener && !opener.closest('[inert]') && !opener.disabled) opener.focus({preventScroll:true});
    state.opener = null;
}
function closeDialog(state, restore=true) {
    if (state.dialog.open) {
        state.dialog.close();
    }
    if (restore) restoreDialogFocus(state);
}
export function synchronizeDialog(dialog,isOpen,reference = null) {
    if (!dialog?.isConnected) return;
    if (isOpen && typeof dialog.showModal !== 'function') throw new TypeError('Native Dialog API is required.');
    let state = dialogs.get(dialog);
    if (!state) {
        state = { dialog,reference,pending:false,opener:null,cleanup:[] };
        const cancel = async event => {
            event.preventDefault();
            if (dialog.dataset.fDialogDismissible === 'false' || dialog.getAttribute('aria-busy') === 'true' || state.pending) return;
            state.pending = true;
            try { if (state.reference) await state.reference.invokeMethodAsync('RequestCloseAsync'); else closeDialog(state); }
            catch { /* A reconnect must retain the open form. */ }
            finally { state.pending = false; }
        };
        const keys = event => {
            if (!dialog.open) return;
            if (event.key !== 'Tab') return;
            const items = [...dialog.querySelectorAll(focusableSelector)].filter(visible);
            const first = items[0], last = items.at(-1);
            if (!first) { event.preventDefault(); dialog.focus(); return; }
            if (event.shiftKey && (document.activeElement === first || document.activeElement === dialog)) { event.preventDefault(); last.focus(); }
            else if (!event.shiftKey && (document.activeElement === last || !dialog.contains(document.activeElement))) { event.preventDefault(); first.focus(); }
        };
        const close = event => {
            const control = event.target?.closest?.('[data-f-dialog-close]');
            if (dialog.dataset.fDialogBrowserControlled !== 'true' || !control || control.closest('dialog') !== dialog) return;
            if (dialog.dataset.fDialogDismissible === 'false' || dialog.getAttribute('aria-busy') === 'true') return;
            event.preventDefault(); closeDialog(state);
        };
        dialog.addEventListener('click',close);
        state.cleanup.push(()=>dialog.removeEventListener('click',close));
        dialog.addEventListener('cancel',cancel); dialog.addEventListener('keydown',keys);
        state.cleanup.push(()=>dialog.removeEventListener('cancel',cancel),()=>dialog.removeEventListener('keydown',keys));
        const observer = new MutationObserver(()=>{ if (!dialog.isConnected) detachDialog(dialog); });
        observer.observe(document.body,{childList:true,subtree:true}); state.cleanup.push(()=>observer.disconnect());
        dialogs.set(dialog,state);
    }
    state.reference = reference;
    if (isOpen && !dialog.open) {
        if (activeMenu) closeMenu(activeMenu.trigger,activeMenu.panel,false);
        state.opener = resolveInvoker(document.activeElement);
        dialog.showModal();
        dialog.querySelector('[data-f-dialog-body]')?.scrollTo(0,0);
        const requested = [...dialog.querySelectorAll('[autofocus]')].find(element => visible(element) && !element.disabled);
        const target = requested ?? [...dialog.querySelectorAll(focusableSelector)].find(visible);
        if (target) target.focus({preventScroll:true}); else { dialog.tabIndex = -1; dialog.focus({preventScroll:true}); }
    } else if (!isOpen && dialog.open) closeDialog(state);
}
/** Selects a keyed view and its declared actions without introducing a second modal controller. */
export function setDialogView(dialog,key) {
    if (!dialog?.isConnected) return false;
    const owned = element => element.closest('dialog') === dialog;
    const views = [...dialog.querySelectorAll('[data-f-dialog-view]')].filter(owned);
    if (!views.some(view => view.dataset.fDialogView === key)) return false;
    for (const view of views) view.hidden = view.dataset.fDialogView !== key;
    for (const action of [...dialog.querySelectorAll('[data-f-dialog-views]')].filter(owned))
        action.hidden = !action.dataset.fDialogViews.split(/\s+/).includes(key);
    dialog.dataset.fDialogActiveView = key;
    const actions = dialog.querySelector('.f-dialog-actions');
    if (actions && owned(actions)) {
        actions.hidden = false;
        const declared = [...actions.querySelectorAll('[data-f-dialog-views]')].filter(owned);
        if (declared.length && declared.every(element => element.hidden))
            actions.hidden = ![...actions.querySelectorAll('button,a[href],input,select,textarea')].some(element => !element.closest('[hidden]'));
    }
    if (dialog.open && document.activeElement && dialog.contains(document.activeElement) && !visible(document.activeElement)) {
        const target = [...dialog.querySelectorAll(focusableSelector)].find(visible);
        if (target) target.focus({preventScroll:true}); else { dialog.tabIndex = -1; dialog.focus({preventScroll:true}); }
    }
    return true;
}
export function detachDialog(dialog) {
    const state = dialogs.get(dialog); if (!state) return;
    closeDialog(state); state.cleanup.forEach(remove=>remove()); dialogs.delete(dialog);
}
