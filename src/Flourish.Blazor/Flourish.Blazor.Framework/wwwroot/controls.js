// Scoped DOM lifetimes adapted from the source tooltip, menu, modal and input behaviors.
const menus = new WeakMap();
const dialogs = new WeakMap();
const inputs = new WeakMap();
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

function menuItems(state) { return [...state.panel.querySelectorAll(state.itemSelector)].filter(visible); }
function copyTheme(trigger, panel) {
    const style = getComputedStyle(trigger);
    // Preserve inherited presentation across a DOM portal without depending on a visual package.
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
export function attachMenu(trigger, panel, openOnHover=false) {
    if (!trigger || !panel) return;
    const existing = menus.get(panel);
    if (existing) {
        if (existing.openOnHover !== openOnHover || trigger.disabled) closeMenu(trigger,panel,false);
        existing.openOnHover = openOnHover;
        trigger.setAttribute('aria-expanded',existing.open ? 'true' : 'false');
        if (existing.open) {
            existing.restoreTheme?.(); existing.restoreTheme = copyTheme(trigger,panel);
            positionMenu(existing);
        }
        return;
    }
    const state = { trigger, panel, hoverRoot:trigger.closest('.f-action-menu') ?? trigger, openOnHover,
        itemSelector:'[role="menuitem"]:not([disabled])', disclosure:null,
        open:false, placeholder:null, restoreTheme:null, maximumHeight:'', maximumHeightPriority:'', cleanup:[] };
    const listen = (target, type, callback, options) => {
        target.addEventListener(type, callback, options);
        state.cleanup.push(() => target.removeEventListener(type, callback, options));
    };
    listen(trigger, 'keydown', event => {
        if (!['ArrowDown','ArrowUp'].includes(event.key) || trigger.disabled) return;
        event.preventDefault();
        event.stopPropagation();
        if (!state.open) openMenu(state,true);
        const items = menuItems(state); (event.key === 'ArrowUp' ? items.at(-1) : items[0])?.focus();
    });
    listen(state.hoverRoot, 'pointerenter', event => {
        if (state.openOnHover && event.pointerType !== 'touch' && !trigger.disabled) openMenu(state,false);
    });
    const leave = event => {
        if (!state.openOnHover || !state.open || event.pointerType === 'touch') return;
        if (state.hoverRoot.contains(event.relatedTarget) || panel.contains(event.relatedTarget)) return;
        closeMenu(trigger,panel,panel.contains(document.activeElement));
    };
    listen(state.hoverRoot, 'pointerleave', leave);
    listen(panel, 'pointerleave', leave);
    listen(document, 'pointerdown', event => {
        if (state.open && !panel.contains(event.target) && !trigger.contains(event.target)) closeMenu(trigger,panel,false);
    }, true);
    listen(document, 'focusin', event => {
        if (state.open && !panel.contains(event.target) && !trigger.contains(event.target)) closeMenu(trigger,panel,false);
    });
    listen(document, 'keydown', event => {
        if (!state.open) return;
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
export function attachDisclosureMenu(disclosure, trigger, panel) {
    if (!disclosure || !trigger || !panel) return;
    attachMenu(trigger,panel);
    const state = menus.get(panel);
    if (!state.disclosure) {
        state.disclosure = disclosure;
        state.itemSelector = focusableSelector;
        const synchronize = () => disclosure.open ? openMenu(state,false) : closeMenu(trigger,panel,false);
        disclosure.addEventListener('toggle',synchronize);
        state.cleanup.push(()=>disclosure.removeEventListener('toggle',synchronize));
    }
    // Native summary activation owns the open state; option clicks never dismiss a multi-selection panel.
    if (disclosure.open && !state.open) openMenu(state,false);
    else if (!disclosure.open && state.open) closeMenu(trigger,panel,false);
}
function openMenu(state, focusMenu) {
    if (state.open || state.trigger.disabled) return;
    const {trigger,panel} = state;
    if (activeMenu) closeMenu(activeMenu.trigger,activeMenu.panel,false);
    state.restoreTheme = copyTheme(trigger,panel);
    state.maximumHeight = panel.style.getPropertyValue('max-height');
    state.maximumHeightPriority = panel.style.getPropertyPriority('max-height');
    state.open = true;
    activeMenu = state;
    if (state.disclosure) state.disclosure.open = true;
    panel.setAttribute('data-f-open','');
    if (typeof panel.showPopover === 'function') panel.showPopover();
    else {
        state.placeholder = document.createComment('menu return position');
        panel.before(state.placeholder); document.body.append(panel);
    }
    trigger.setAttribute('aria-expanded','true');
    positionMenu(state);
    if (focusMenu) menuItems(state)[0]?.focus({preventScroll:true});
}
export function toggleMenu(trigger, panel, focusMenu=true) {
    if (!menus.has(panel)) attachMenu(trigger,panel);
    const state = menus.get(panel); if (!state || trigger.disabled) return;
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
    if (state.open && typeof panel.hidePopover === 'function' && panel.isConnected && panel.matches(':popover-open')) panel.hidePopover();
    panel.removeAttribute('data-f-open');
    state.open = false;
    if (state.disclosure) state.disclosure.open = false;
    trigger.setAttribute('aria-expanded','false');
    if (state.placeholder?.isConnected) { state.placeholder.replaceWith(panel); state.placeholder = null; }
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
        if (typeof state.dialog.close === 'function') state.dialog.close(); else state.dialog.removeAttribute('open');
    }
    if (state.fallback) {
        state.fallback.remove(); state.fallback = null;
        state.inert.forEach(([element,previous])=>{ element.inert = previous; }); state.inert = [];
        state.dialog.classList.remove('f-dialog-fallback');
        if (state.placeholder?.isConnected) state.placeholder.replaceWith(state.dialog);
        state.placeholder = null;
        state.restoreTheme?.(); state.restoreTheme = null;
    }
    if (restore) restoreDialogFocus(state);
}
export function synchronizeDialog(dialog,isOpen,reference) {
    if (!dialog?.isConnected) return;
    let state = dialogs.get(dialog);
    if (!state) {
        state = { dialog,reference,pending:false,opener:null,fallback:null,placeholder:null,restoreTheme:null,inert:[],cleanup:[] };
        const cancel = async event => {
            event.preventDefault();
            if (dialog.getAttribute('aria-busy') === 'true' || state.pending) return;
            state.pending = true;
            try { await state.reference.invokeMethodAsync('RequestCloseAsync'); }
            catch { /* A reconnect must retain the open form. */ }
            finally { state.pending = false; }
        };
        const keys = event => {
            if (!dialog.open) return;
            if (event.key === 'Escape' && state.fallback) { cancel(event); return; }
            if (event.key !== 'Tab') return;
            const items = [...dialog.querySelectorAll(focusableSelector)].filter(visible);
            const first = items[0], last = items.at(-1);
            if (!first) { event.preventDefault(); dialog.focus(); return; }
            if (event.shiftKey && (document.activeElement === first || document.activeElement === dialog)) { event.preventDefault(); last.focus(); }
            else if (!event.shiftKey && (document.activeElement === last || !dialog.contains(document.activeElement))) { event.preventDefault(); first.focus(); }
        };
        dialog.addEventListener('cancel',cancel); dialog.addEventListener('keydown',keys);
        state.cleanup.push(()=>dialog.removeEventListener('cancel',cancel),()=>dialog.removeEventListener('keydown',keys));
        const observer = new MutationObserver(()=>{ if (!dialog.isConnected) detachDialog(dialog); });
        observer.observe(document.body,{childList:true,subtree:true}); state.cleanup.push(()=>observer.disconnect());
        dialogs.set(dialog,state);
    }
    state.reference = reference;
    if (isOpen && !dialog.open) {
        if (activeMenu) closeMenu(activeMenu.trigger,activeMenu.panel,false);
        state.opener = document.activeElement;
        if (typeof dialog.showModal === 'function') dialog.showModal();
        else {
            state.restoreTheme = copyTheme(dialog,dialog);
            state.placeholder = document.createComment('dialog return position'); dialog.before(state.placeholder);
            state.fallback = document.createElement('div'); state.fallback.className = 'f-modal-backdrop';
            for (const sibling of [...document.body.children]) { state.inert.push([sibling,sibling.inert]); sibling.inert = true; }
            document.body.append(state.fallback,dialog); dialog.classList.add('f-dialog-fallback'); dialog.setAttribute('open','');
        }
        dialog.querySelector('[data-f-dialog-body]')?.scrollTo(0,0);
        const requested = [...dialog.querySelectorAll('[autofocus]')].find(element => visible(element) && !element.disabled);
        const target = requested ?? [...dialog.querySelectorAll(focusableSelector)].find(visible);
        if (target) target.focus({preventScroll:true}); else { dialog.tabIndex = -1; dialog.focus({preventScroll:true}); }
    } else if (!isOpen && dialog.open) closeDialog(state);
}
export function detachDialog(dialog) {
    const state = dialogs.get(dialog); if (!state) return;
    closeDialog(state); state.cleanup.forEach(remove=>remove()); dialogs.delete(dialog);
}
