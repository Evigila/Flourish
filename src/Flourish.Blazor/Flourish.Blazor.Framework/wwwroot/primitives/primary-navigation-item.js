const entries = new WeakMap();
let active = null;

export function attach(container) {
    if (entries.has(container)) return;
    const trigger = container.querySelector('[data-navigation-trigger]');
    const tooltip = container.querySelector('[data-navigation-tooltip]');
    if (!trigger || !tooltip) return;

    const state = { triggerHover: false, tooltipHover: false, focused: false, dismissed: false,
        open: false, showTimer: null, hideTimer: null, disposed: false };
    const listeners = [];
    const listen = (target, name, handler, options) => {
        target.addEventListener(name, handler, options);
        listeners.push(() => target.removeEventListener(name, handler, options));
    };
    const clearTimers = () => {
        clearTimeout(state.showTimer);
        clearTimeout(state.hideTimer);
        state.showTimer = state.hideTimer = null;
    };
    const interacting = () => state.triggerHover || state.tooltipHover || state.focused;
    const position = () => {
        if (!state.open) return;
        const anchor = trigger.getBoundingClientRect();
        const viewportWidth = document.documentElement.clientWidth;
        const viewportHeight = window.innerHeight;
        const left = Math.max(8, Math.min(anchor.right + 8, viewportWidth - 24));
        tooltip.style.left = `${left}px`;
        tooltip.style.maxWidth = `${Math.max(16, Math.min(352, viewportWidth - left - 8))}px`;
        const height = tooltip.getBoundingClientRect().height;
        const top = Math.max(8, Math.min(anchor.top + (anchor.height - height) / 2, viewportHeight - height - 8));
        tooltip.style.top = `${top}px`;
    };
    const hide = () => {
        clearTimers();
        if (state.open && tooltip.matches(':popover-open')) {
            tooltip.hidePopover();
        }
        tooltip.removeAttribute('data-open');
        state.open = false;
        state.tooltipHover = false;
        if (active === state) active = null;
    };
    const show = () => {
        state.showTimer = null;
        if (state.disposed || state.dismissed || !interacting() || !container.isConnected) return;
        if (active && active !== state) active.hide();
        active = state;
        state.open = true;
        tooltip.setAttribute('data-open', '');
        // The native top layer escapes neighbouring stacking contexts and overflow clipping.
        tooltip.showPopover();
        position();
    };
    const requestShow = (delay) => {
        clearTimeout(state.hideTimer);
        state.hideTimer = null;
        if (state.open || state.dismissed || state.showTimer !== null) return;
        state.showTimer = setTimeout(show, delay);
    };
    const leave = () => {
        if (interacting()) return;
        state.dismissed = false;
        clearTimeout(state.showTimer);
        state.showTimer = null;
        // Allow the pointer to cross the small gap to the hoverable description.
        clearTimeout(state.hideTimer);
        state.hideTimer = setTimeout(hide, 120);
    };
    const dismiss = () => {
        state.dismissed = true;
        hide();
    };

    state.hide = hide;
    listen(trigger, 'pointerenter', event => {
        if (event.pointerType === 'touch') return;
        state.triggerHover = true;
        requestShow(150);
    });
    listen(trigger, 'pointerleave', () => { state.triggerHover = false; leave(); });
    listen(trigger, 'focus', () => { state.focused = true; requestShow(0); });
    listen(trigger, 'blur', () => { state.focused = false; leave(); });
    listen(trigger, 'click', dismiss);
    listen(tooltip, 'pointerenter', () => { state.tooltipHover = true; requestShow(0); });
    listen(tooltip, 'pointerleave', () => { state.tooltipHover = false; leave(); });
    listen(document, 'keydown', event => {
        if (event.key === 'Escape' && (state.open || state.showTimer !== null)) dismiss();
    });
    listen(window, 'blur', dismiss);
    listen(window, 'resize', position);
    listen(document, 'scroll', position, true);

    const observer = new MutationObserver(() => {
        if (!container.isConnected) detach(container);
    });
    observer.observe(document.body, { childList: true, subtree: true });
    state.dispose = () => {
        state.disposed = true;
        hide();
        listeners.forEach(remove => remove());
        observer.disconnect();
    };
    entries.set(container, state);
    if (document.activeElement === trigger) {
        state.focused = true;
        requestShow(0);
    }
}

export function detach(container) {
    entries.get(container)?.dispose();
    entries.delete(container);
}
