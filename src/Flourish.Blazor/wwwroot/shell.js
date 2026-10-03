const instances = new WeakMap();

export function attach(root, content) {
    detach(root);
    const removers = [];
    const state = { content, removers, activeTip:null, frame:0, returnFocus:null };
    const listen = (target, name, handler, options) => {
        target.addEventListener(name, handler, options);
        removers.push(() => target.removeEventListener(name, handler, options));
    };
    const updateHeading = () => {
        state.frame = 0;
        const compact = root.hasAttribute('data-compact-heading');
        const next = compact ? content.scrollTop >= 24 : content.scrollTop > 96;
        if (compact !== next) root.toggleAttribute('data-compact-heading', next);
    };
    listen(content, 'scroll', () => {
        if (!state.frame) state.frame = requestAnimationFrame(updateHeading);
    }, {passive:true});
    for (const trigger of root.querySelectorAll('.f-primary-item')) {
        const tip = trigger.querySelector('.f-nav-tooltip');
        if (!tip) continue;
        tip.setAttribute('popover', 'manual');
        let timer, leaveTimer, hovered = false, focused = false, overTip = false, dismissed = false;
        const position = () => {
            const box = trigger.getBoundingClientRect();
            tip.style.left = `${Math.min(box.right + 8, Math.max(8, innerWidth - tip.offsetWidth - 8))}px`;
            tip.style.top = `${Math.max(8,Math.min(box.top+(box.height-tip.offsetHeight)/2,innerHeight-tip.offsetHeight-8))}px`;
        };
        const hide = () => {
            clearTimeout(timer); clearTimeout(leaveTimer);
            if (typeof tip.hidePopover === 'function' && tip.matches(':popover-open')) tip.hidePopover();
            tip.removeAttribute('data-open');
            if (state.activeTip?.tip === tip) state.activeTip = null;
        };
        const show = () => {
            if (dismissed || (!hovered && !focused && !overTip)) return;
            state.activeTip?.hide();
            tip.setAttribute('data-open','');
            if (typeof tip.showPopover === 'function') tip.showPopover();
            state.activeTip = { tip, hide, position };
            position();
        };
        const leave = () => {
            if (hovered || focused || overTip) return;
            dismissed = false; clearTimeout(timer); clearTimeout(leaveTimer); leaveTimer = setTimeout(hide, 120);
        };
        listen(trigger,'pointerenter', e => { if(e.pointerType==='touch')return; hovered=true; clearTimeout(leaveTimer); timer=setTimeout(show,150); });
        listen(trigger,'pointerleave', () => { hovered=false; leave(); });
        listen(trigger,'focus', () => { focused=true; clearTimeout(leaveTimer); show(); });
        listen(trigger,'blur', () => { focused=false; leave(); });
        listen(trigger,'click', () => { dismissed=true; hide(); });
        listen(trigger,'keydown', e => { if(e.key==='Escape') { dismissed=true; hide(); } });
        listen(tip,'pointerenter', () => { overTip=true; clearTimeout(leaveTimer); });
        listen(tip,'pointerleave', () => { overTip=false; leave(); });
        removers.push(hide);
    }
    listen(window,'resize', () => {
        state.activeTip?.position();
        synchronize(root, root.dataset.navigationOpen === 'true', false);
    });
    listen(root,'keydown', e => {
        if(e.key==='Escape') { state.activeTip?.hide(); root.querySelector('.f-navigation-backdrop')?.click(); }
        if(e.key==='Tab' && state.content.inert) {
            const targets = [...root.querySelectorAll('.f-titlebar > .f-icon-button, .f-secondary-item:not([aria-disabled=true]), .f-navigation-backdrop')].filter(item => item.getClientRects().length);
            const first = targets[0], last = targets.at(-1);
            if(e.shiftKey && (document.activeElement === first || !targets.includes(document.activeElement))) { e.preventDefault(); last?.focus(); }
            else if(!e.shiftKey && (document.activeElement === last || !targets.includes(document.activeElement))) { e.preventDefault(); first?.focus(); }
        }
    });
    instances.set(root, state);
    updateHeading();
}
export function synchronize(root, open, resetScroll) {
    const state=instances.get(root); if (!state) return;
    if (resetScroll) { state.content.scrollTop=0; root.removeAttribute('data-compact-heading'); }
    // Mobile expanded navigation is modal-like: move focus inside, block background and return focus.
    const expanded = open && matchMedia('(max-width:760px)').matches;
    state.content.inert=expanded;
    if(expanded && !state.returnFocus) {
        state.returnFocus=document.activeElement;
        root.querySelector('.f-secondary-item:not([aria-disabled=true])')?.focus();
    } else if(!expanded && state.returnFocus) {
        state.returnFocus?.focus(); state.returnFocus=null;
    }
}
export function detach(root) {
    const state = instances.get(root); if(!state) return;
    if(state.frame) cancelAnimationFrame(state.frame);
    state.removers.forEach(remove => remove()); state.content.inert=false;
    instances.delete(root);
}