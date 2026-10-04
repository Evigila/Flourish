const instances = new WeakMap();
const interruptionEvents = ['wheel', 'touchstart', 'pointerdown', 'keydown'];
const links = root => [...root.querySelectorAll('.f-section-link')];
const idFromLink = link => {
    try { return decodeURIComponent((link.getAttribute('href') ?? '').slice(1)); }
    catch { return ''; }
};

function discover(state) {
    const parentMain = state.content.closest('main');
    return [...state.content.querySelectorAll('h2')].filter(heading => {
        if (!heading.textContent.trim() || heading.closest('main') !== parentMain || heading.closest('[hidden]')
            || heading.closest('dialog, [role="dialog"], [role="alertdialog"]')) return false;
        const section = heading.closest('section');
        if (section && state.content.contains(section) && section.querySelector('h2') !== heading) return false;
        const ancestorSection = section?.parentElement?.closest('section');
        return !ancestorSection || !state.content.contains(ancestorSection);
    }).map(heading => {
        if (!heading.id) {
            let id;
            do { id = `${state.root.id}-heading-${++state.sequence}`; } while (document.getElementById(id));
            heading.id = id;
            state.generatedIds.set(heading, heading.id);
        }
        return { id: heading.id, title: heading.textContent.trim() };
    });
}

function headingOffset(state) {
    const heading = state.content.querySelector('.f-page-heading, .page-heading');
    return heading && getComputedStyle(heading).position === 'sticky' ? heading.getBoundingClientRect().height + 12 : 12;
}

function cancelNavigation(state) {
    const navigation = state.navigation;
    if (!navigation) return;
    state.navigation = null;
    clearTimeout(navigation.idleTimer);
    clearTimeout(navigation.deadlineTimer);
    interruptionEvents.forEach(type => document.removeEventListener(type, state.interrupt, true));
}

function finishNavigation(state, navigation) {
    if (state.navigation !== navigation || state.disposed) return;
    cancelNavigation(state);
    if (!navigation.heading.isConnected || !state.content.contains(navigation.heading)) return;
    const difference = navigation.heading.getBoundingClientRect().top - state.content.getBoundingClientRect().top - headingOffset(state);
    // One final correction uses the settled sticky-heading height; it cannot restart itself.
    if (Math.abs(difference) > 1) state.content.scrollTo({ top:Math.max(0, state.content.scrollTop + difference), behavior:'auto' });
    update(state);
}

function scheduleAlignment(state) {
    const navigation = state.navigation;
    if (!navigation || state.disposed) return;
    clearTimeout(navigation.idleTimer);
    navigation.idleTimer = setTimeout(() => finishNavigation(state, navigation), 120);
}

function beginNavigation(state, heading) {
    cancelNavigation(state);
    const navigation = state.navigation = { heading, idleTimer:0, deadlineTimer:0 };
    interruptionEvents.forEach(type => document.addEventListener(type, state.interrupt, { capture:true, passive:true }));
    // Browsers without scrollend still settle, and continuous events cannot postpone cleanup indefinitely.
    navigation.deadlineTimer = setTimeout(() => finishNavigation(state, navigation), 1000);
    scheduleAlignment(state);
}

function refresh(state) {
    if (state.disposed) return;
    if (!state.root.isConnected || !state.content.isConnected) { detach(state.root); return; }
    if (state.autoDiscover) {
        const entries = discover(state);
        const signature = JSON.stringify(entries);
        if (signature !== state.signature) {
            state.signature = signature;
            state.reference.invokeMethodAsync('UpdateSectionsAsync', entries).catch(() => {
                if (!state.root.isConnected) detach(state.root);
            });
        }
    }
    state.targets = links(state.root).map(link => ({ link, heading: document.getElementById(idFromLink(link)) }))
        .filter(item => item.heading && state.content.contains(item.heading));
    state.root.hidden = state.targets.length === 0;
    const pageHeading = state.content.querySelector('.f-page-heading, .page-heading');
    const observed = [state.content, ...(pageHeading ? [pageHeading] : []), ...state.targets.map(item => item.heading)];
    if (!state.observed || observed.length !== state.observed.length || observed.some((node, index) => node !== state.observed[index])) {
        state.resize?.disconnect();
        observed.forEach(node => state.resize?.observe(node));
        state.observed = observed;
    }
    for (const [heading, id] of state.generatedIds) if (!state.content.contains(heading)) {
        if (heading.id === id) heading.removeAttribute('id');
        state.generatedIds.delete(heading);
    }
    for (const heading of state.focusTargets ?? []) if (!state.content.contains(heading)) {
        heading.removeAttribute('tabindex');
        state.focusTargets.delete(heading);
    }
    if (state.navigation && !state.content.contains(state.navigation.heading)) cancelNavigation(state);
    update(state);
}

function position(state) {
    if (!state.targets.length) return;
    const bounds = state.content.getBoundingClientRect();
    state.root.hidden = bounds.bottom <= 0 || bounds.top >= window.innerHeight || bounds.right <= 0 || bounds.left >= window.innerWidth;
    const track = state.targets[0].heading.closest('section') ?? state.targets[0].heading;
    const gutter = Math.max(20, bounds.right - track.getBoundingClientRect().right);
    const top = Math.max(bounds.top, 0);
    const bottom = Math.min(bounds.bottom, window.innerHeight);
    state.root.style.setProperty('--f-section-nav-left', `${Math.max(bounds.left, bounds.right - gutter / 2 - 10)}px`);
    state.root.style.setProperty('--f-section-nav-top', `${top + Math.max(0, bottom - top) / 2}px`);
    state.root.style.setProperty('--f-section-nav-height', `${Math.max(24, bottom - top - 24)}px`);
    state.root.toggleAttribute('data-enhanced', true);
}

function update(state) {
    if (!state.targets.length || state.disposed) return;
    position(state);
    const boundary = state.content.getBoundingClientRect().top + headingOffset(state) + 1;
    let active = state.targets[0];
    for (const target of state.targets) if (target.heading.getBoundingClientRect().top <= boundary) active = target;
    const maximum = state.content.scrollHeight - state.content.clientHeight;
    if (maximum > 2 && state.content.scrollTop >= maximum - 2) active = state.targets.at(-1);
    for (const target of state.targets) {
        if (target === active) target.link.setAttribute('aria-current', 'location');
        else target.link.removeAttribute('aria-current');
    }
}

export function synchronize(root, contentId, autoDiscover, reference) {
    const content = document.getElementById(contentId);
    let state = instances.get(root);
    if (state && (state.content !== content || state.autoDiscover !== autoDiscover)) { detach(root); state = null; }
    if (!content) { root.hidden = true; return; }
    if (!state) {
        state = { root, content, autoDiscover, reference, generatedIds:new Map(), sequence:0, targets:[], frame:0, disposed:false };
        const schedule = () => {
            if (state.frame || state.disposed) return;
            state.frame = requestAnimationFrame(() => { state.frame = 0; refresh(state); scheduleAlignment(state); });
        };
        state.scroll = () => { update(state); scheduleAlignment(state); };
        state.position = () => position(state);
        state.interrupt = () => cancelNavigation(state);
        state.click = event => {
            const link = event.target.closest('.f-section-link');
            if (!link || !root.contains(link) || event.button > 0 || event.ctrlKey || event.metaKey || event.shiftKey || event.altKey) return;
            const heading = document.getElementById(idFromLink(link));
            if (!heading || !content.contains(heading)) return;
            event.preventDefault();
            const top = content.scrollTop + heading.getBoundingClientRect().top - content.getBoundingClientRect().top - headingOffset(state);
            beginNavigation(state, heading);
            content.scrollTo({ top:Math.max(0, top), behavior:matchMedia('(prefers-reduced-motion: reduce)').matches ? 'auto' : 'smooth' });
            if (!heading.hasAttribute('tabindex')) { heading.setAttribute('tabindex', '-1'); state.focusTargets ??= new Set(); state.focusTargets.add(heading); }
            heading.focus({ preventScroll:true });
        };
        state.dismiss = event => { if (event.key === 'Escape') root.setAttribute('data-tooltip-dismissed', ''); };
        state.showTooltip = event => {
            root.removeAttribute('data-tooltip-dismissed');
            const tooltip = event.target.closest('.f-section-link')?.querySelector('.f-section-label');
            if (!tooltip) return;
            const linkBounds = event.target.closest('.f-section-link').getBoundingClientRect();
            const left = linkBounds.left - 18, right = window.innerWidth - linkBounds.right - 18;
            const flip = left < 120 && right > left;
            tooltip.toggleAttribute('data-tooltip-right', flip);
            tooltip.style.setProperty('--f-section-tooltip-width', `${Math.max(32, Math.min(280, flip ? right : left))}px`);
            tooltip.style.setProperty('--f-section-tooltip-shift', '0px');
            const bounds = tooltip.getBoundingClientRect();
            const shift = Math.max(0, 8 - bounds.top) - Math.max(0, bounds.bottom - window.innerHeight + 8);
            tooltip.style.setProperty('--f-section-tooltip-shift', `${shift}px`);
        };
        content.addEventListener('scroll', state.scroll, { passive:true });
        root.addEventListener('click', state.click);
        root.addEventListener('keydown', state.dismiss);
        root.addEventListener('pointerover', state.showTooltip);
        root.addEventListener('focusin', state.showTooltip);
        window.addEventListener('resize', state.position);
        document.addEventListener('scroll', state.position, { capture:true, passive:true });
        state.mutation = new MutationObserver(schedule);
        state.mutation.observe(content, { childList:true, subtree:true, characterData:true, attributes:true, attributeFilter:['id','hidden'] });
        if (typeof ResizeObserver !== 'undefined') { state.resize = new ResizeObserver(schedule); }
        instances.set(root, state);
    }
    state.reference = reference;
    refresh(state);
}

export function detach(root) {
    const state = instances.get(root);
    if (!state) return;
    state.disposed = true;
    cancelNavigation(state);
    if (state.frame) cancelAnimationFrame(state.frame);
    state.mutation.disconnect();
    state.resize?.disconnect();
    state.content.removeEventListener('scroll', state.scroll);
    root.removeEventListener('click', state.click);
    root.removeEventListener('keydown', state.dismiss);
    root.removeEventListener('pointerover', state.showTooltip);
    root.removeEventListener('focusin', state.showTooltip);
    window.removeEventListener('resize', state.position);
    document.removeEventListener('scroll', state.position, true);
    state.generatedIds.forEach((id, heading) => { if (heading.id === id) heading.removeAttribute('id'); });
    state.focusTargets?.forEach(heading => heading.removeAttribute('tabindex'));
    root.removeAttribute('data-enhanced');
    root.removeAttribute('data-tooltip-dismissed');
    instances.delete(root);
}
