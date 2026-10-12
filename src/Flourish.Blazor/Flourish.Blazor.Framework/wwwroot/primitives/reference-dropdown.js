export function containsFocus(element) {
    return element?.contains(document.activeElement) ?? false;
}

const popups = new WeakMap();
/** Outside pointer interactions dismiss circuit-owned selection popups, including nonfocusable targets. */
export function attachPopupDismissal(root, reference) {
    if (!root?.isConnected) return;
    const existing = popups.get(root);
    if (existing) { existing.reference = reference; return; }
    const state = {reference,pending:false};
    const outside = async event => {
        if (root.contains(event.target) || root.getAttribute('data-f-popup-strength') === 'strong'
            || root.getAttribute('aria-busy') === 'true' || state.pending) return;
        state.pending = true;
        try { await state.reference.invokeMethodAsync('RequestDismissAsync'); }
        catch { /* Disposed/reconnecting circuits do not receive a second owner. */ }
        finally { state.pending = false; }
    };
    const observer = new MutationObserver(() => { if (!root.isConnected) detachPopupDismissal(root); });
    observer.observe(document.documentElement,{childList:true,subtree:true});
    document.addEventListener('pointerdown',outside,true);
    state.cleanup = () => { observer.disconnect(); document.removeEventListener('pointerdown',outside,true); };
    popups.set(root,state);
}
export function detachPopupDismissal(root) { popups.get(root)?.cleanup(); popups.delete(root); }
