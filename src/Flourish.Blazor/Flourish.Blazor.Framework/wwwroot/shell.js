const blockedLayouts = new WeakMap();
const expandAt = 24;
const collapseAfter = 96;

export function updateCompactHeading(root, content) {
    if (root.hasAttribute('data-compact-heading')) {
        if (content.scrollTop < expandAt) resetCompactHeading(root);
        return;
    }
    if (content.scrollTop <= collapseAfter) return;

    const layout = { content, height:content.scrollHeight, viewport:content.clientHeight, width:content.clientWidth };
    const blocked = blockedLayouts.get(root);
    if (blocked && Object.keys(layout).every(key => blocked[key] === layout[key])) return;

    const previousTop = content.scrollTop;
    root.setAttribute('data-compact-heading', '');
    // A short document can lose its entire scroll range when the sticky title shrinks.
    // Measure before painting and remember unsuitable geometry so the queued scroll
    // event cannot start another collapse/restore cycle. Do not add blank space.
    if (content.scrollHeight - content.clientHeight < expandAt) {
        root.removeAttribute('data-compact-heading');
        blockedLayouts.set(root, layout);
        content.scrollTop = previousTop;
    } else {
        blockedLayouts.delete(root);
    }
}

export function resetCompactHeading(root) {
    blockedLayouts.delete(root);
    root.removeAttribute('data-compact-heading');
}

const instances = new WeakMap();

export function attach(root, content) {
    detach(root);
    const removers = [];
    const state = { content, removers, activeTip:null, frame:0, returnFocus:null };
    const listen = (target, name, handler, options) => {
        target.addEventListener(name, handler, options);
        removers.push(() => target.removeEventListener(name, handler, options));
    };
    const measureLayout = () => {
        const titlebar = root.querySelector('.f-titlebar');
        const primary = root.querySelector('.f-primary-navigation');
        root.style.setProperty('--f-shell-titlebar-size', (titlebar?.getBoundingClientRect().height ?? 0) + 'px');
        root.style.setProperty('--f-shell-primary-size', (primary?.getBoundingClientRect().width ?? 0) + 'px');
    };
    measureLayout();
    const observer = new ResizeObserver(measureLayout);
    for (const element of [root.querySelector('.f-titlebar'),root.querySelector('.f-primary-navigation')]) {
        if (element) observer.observe(element);
    }
    removers.push(() => observer.disconnect());
    const updateHeading = () => {
        state.frame = 0;
        updateCompactHeading(root, content);
    };
    state.scheduleHeading = () => {
        if (!state.frame) state.frame = requestAnimationFrame(updateHeading);
    };
    listen(content, 'scroll', state.scheduleHeading, {passive:true});
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
            if (tip.matches(':popover-open')) tip.hidePopover();
            tip.removeAttribute('data-open');
            if (state.activeTip?.tip === tip) state.activeTip = null;
        };
        const show = () => {
            if (dismissed || (!hovered && !focused && !overTip)) return;
            state.activeTip?.hide();
            tip.setAttribute('data-open','');
            tip.showPopover();
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
        measureLayout();
        state.activeTip?.position();
        synchronize(root, root.dataset.navigationOpen === 'true', false);
    });
    listen(root,'keydown', e => {
        if(e.key==='Escape') { state.activeTip?.hide(); root.querySelector('.f-navigation-backdrop')?.click(); }
        if(e.key==='Tab' && state.content.inert) {
            const targets = [...root.querySelectorAll('.f-titlebar > .f-icon-button, .f-secondary-item:not([aria-disabled=true]), .f-secondary-toggle:not(:disabled), .f-navigation-backdrop')].filter(item => item.getClientRects().length);
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
    if (resetScroll) { resetCompactHeading(root); state.content.scrollTop=0; }
    else state.scheduleHeading();
    // Mobile expanded navigation is modal-like: move focus inside, block background and return focus.
    const expanded = open && matchMedia('(max-width:760px)').matches;
    state.content.inert=expanded;
    if(expanded && !state.returnFocus) {
        state.returnFocus=document.activeElement;
        [...root.querySelectorAll('.f-secondary-item:not([aria-disabled=true]), .f-secondary-toggle:not(:disabled)')].find(item => item.getClientRects().length)?.focus();
    } else if(!expanded && state.returnFocus) {
        state.returnFocus?.focus(); state.returnFocus=null;
    }
}
export function detach(root) {
    const state = instances.get(root); if(!state) return;
    if(state.frame) cancelAnimationFrame(state.frame);
    state.removers.forEach(remove => remove()); state.content.inert=false;
    resetCompactHeading(root);
    instances.delete(root);
}
