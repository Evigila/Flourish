import { positionPopup } from '../controls.js';

export function containsFocus(element) {
    return element?.contains(document.activeElement) ?? false;
}

const popups = new WeakMap();
/** The circuit owns selection and dismissal; only anchored geometry uses the shared popup placement. */
export function attachPopupDismissal(root, reference) {
    if (!root?.isConnected) return;
    const existing = popups.get(root);
    if (existing) { existing.reference = reference; existing.update(); return; }
    const state = {reference,pending:false,panel:null,placement:null,restore:null};
    const releasePanel = () => {
        if (state.placement) state.resize?.unobserve(state.placement.trigger);
        if (state.panel?.isConnected && state.panel.matches(':popover-open')) state.panel.hidePopover();
        state.restore?.(); state.restore = null; state.panel = null; state.placement = null;
    };
    const update = () => {
        if (!root.isConnected) { detachPopupDismissal(root); return; }
        const panel = root.querySelector(':scope > .selection-panel, :scope > .search-autocomplete-options');
        const trigger = root.querySelector(':scope > button, :scope > input[role="combobox"]');
        if (state.panel !== panel || state.placement?.trigger !== trigger) {
            releasePanel();
            if (!panel || !trigger) return;
            const names = ['position','inset','margin','right','bottom','left','top','max-height','--f-popup-trigger-width'];
            const saved = names.map(name => [name,panel.style.getPropertyValue(name),panel.style.getPropertyPriority(name)]);
            state.restore = () => saved.forEach(([name,value,priority]) => value ? panel.style.setProperty(name,value,priority) : panel.style.removeProperty(name));
            state.panel = panel;
            state.placement = {trigger,panel,open:true,openOnHover:false,
                maximumHeight:panel.style.getPropertyValue('max-height'),maximumHeightPriority:panel.style.getPropertyPriority('max-height')};
            state.resize?.observe(trigger);
            panel.setAttribute('popover','manual');
            panel.style.setProperty('position','fixed'); panel.style.setProperty('inset','auto'); panel.style.setProperty('margin','0');
            panel.style.setProperty('right','auto'); panel.style.setProperty('bottom','auto');
            const visibility = panel.style.getPropertyValue('visibility'), priority = panel.style.getPropertyPriority('visibility');
            panel.style.setProperty('visibility','hidden');
            try {
                panel.showPopover();
                positionPopup(state.placement);
            } finally {
                if (visibility) panel.style.setProperty('visibility',visibility,priority);
                else panel.style.removeProperty('visibility');
            }
        } else if (state.placement) positionPopup(state.placement);
    };
    state.update = update;
    const outside = async event => {
        if (root.contains(event.target) || root.getAttribute('data-f-popup-strength') === 'strong'
            || root.getAttribute('aria-busy') === 'true' || state.pending) return;
        state.pending = true;
        try { await state.reference.invokeMethodAsync('RequestDismissAsync'); }
        catch { /* Disposed/reconnecting circuits do not receive a second owner. */ }
        finally { state.pending = false; }
    };
    const observer = new MutationObserver(update);
    observer.observe(document.documentElement,{childList:true,subtree:true,characterData:true});
    state.resize = typeof ResizeObserver === 'function' ? new ResizeObserver(update) : null;
    document.addEventListener('pointerdown',outside,true);
    document.addEventListener('scroll',update,true);
    window.addEventListener('resize',update);
    state.cleanup = () => {
        observer.disconnect(); document.removeEventListener('pointerdown',outside,true);
        state.resize?.disconnect();
        document.removeEventListener('scroll',update,true); window.removeEventListener('resize',update);
        releasePanel();
    };
    popups.set(root,state);
    update();
}
export function detachPopupDismissal(root) { popups.get(root)?.cleanup(); popups.delete(root); }
