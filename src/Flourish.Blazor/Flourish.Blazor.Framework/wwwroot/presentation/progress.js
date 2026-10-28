const instances = new WeakMap();
const interruptionEvents = ['wheel', 'touchstart', 'pointerdown', 'keydown'];
const bandSelector = '[data-presentation-band="true"]';
const footerSelector = 'footer, [data-presentation-footer], .f-presentation-footer';

function contentFor(root, contentId) {
    const surface = root.closest('.content-surface');
    if (!surface) return document.getElementById(contentId);
    // Scope the legacy default content ID to this surface, never another instance.
    return [...surface.children].find(child => child.classList.contains('content-stage') && child.id === contentId) ?? null;
}
function scrollOwner(content) {
    for (let node = content; node && node !== document.body && node !== document.documentElement; node = node.parentElement) {
        if (/^(auto|scroll|overlay)$/.test(getComputedStyle(node).overflowY)) return node;
    }
    return window;
}
function ancestors(content) {
    const nodes = [];
    for (let node = content; node && node !== document.body && node !== document.documentElement; node = node.parentElement) nodes.push(node);
    return nodes;
}
function viewport(state) {
    const visual = window.visualViewport;
    const left = visual?.offsetLeft ?? 0, top = visual?.offsetTop ?? 0;
    const bounds = { left, top, right:left + (visual?.width ?? window.innerWidth), bottom:top + (visual?.height ?? window.innerHeight) };
    // Only the actual ancestor chain can clip this surface; chart/table child scrollers do not own its viewport.
    for (const node of ancestors(state.content)) {
        const style = getComputedStyle(node);
        const clipsX = /^(auto|scroll|overlay|hidden|clip)$/.test(style.overflowX);
        const clipsY = /^(auto|scroll|overlay|hidden|clip)$/.test(style.overflowY);
        if (!clipsX && !clipsY) continue;
        const rect = node.getBoundingClientRect();
        const nodeTop = rect.top + node.clientTop, nodeLeft = rect.left + node.clientLeft;
        if (clipsX) { bounds.left = Math.max(bounds.left, nodeLeft); bounds.right = Math.min(bounds.right, nodeLeft + node.clientWidth); }
        if (clipsY) { bounds.top = Math.max(bounds.top, nodeTop); bounds.bottom = Math.min(bounds.bottom, nodeTop + node.clientHeight); }
    }
    return bounds;
}
function alignmentTop(state) {
    const bounds = viewport(state);
    let top = bounds.top;
    for (const header of state.headers) {
        if (!/^(sticky|fixed)$/.test(getComputedStyle(header).position)) continue;
        const rect = header.getBoundingClientRect();
        if (rect.left < bounds.right && rect.right > bounds.left && rect.top <= top + 1
            && rect.bottom > top && rect.bottom < bounds.bottom) top = rect.bottom;
    }
    return top;
}
function eligibleBands(state) {
    const ownMain = state.content.closest('main');
    return [...state.content.querySelectorAll(bandSelector)].filter(band => {
        if (band.closest('main') !== ownMain || band.closest(footerSelector)
            || band.closest('[hidden], dialog, [role="dialog"], [role="alertdialog"], details:not([open])')) return false;
        if (state.surface && band.closest('.content-surface') !== state.surface) return false;
        const parent = band.parentElement?.closest(bandSelector);
        if (parent && state.content.contains(parent)) return false;
        const style = getComputedStyle(band);
        return style.display !== 'none' && style.visibility !== 'hidden';
    });
}
function labelFor(band, index) {
    const ids = (band.getAttribute('aria-labelledby') ?? '').split(/\s+/).filter(Boolean);
    const headings = [...band.querySelectorAll('[id]')];
    const labelled = ids.map(id => headings.find(heading => heading.id === id)?.textContent.trim()).filter(Boolean).join(' ');
    return labelled || band.getAttribute('aria-label')?.trim()
        || band.querySelector('h1,h2,h3,h4,h5,h6')?.textContent.trim() || String(index + 1);
}
function releaseTargets(state, eligible = []) {
    for (const [band, id] of state.generatedIds) if (!eligible.includes(band)) {
        if (band.id === id) band.removeAttribute('id');
        state.generatedIds.delete(band);
    }
    for (const target of state.focusTargets) if (!state.content.contains(target)) {
        if (target.getAttribute('tabindex') === '-1') target.removeAttribute('tabindex');
        state.focusTargets.delete(target);
    }
}
function cancelNavigation(state) {
    const navigation = state.navigation;
    if (!navigation) return;
    state.navigation = null;
    clearTimeout(navigation.idleTimer);
    clearTimeout(navigation.deadlineTimer);
    interruptionEvents.forEach(type => document.removeEventListener(type, state.interrupt, true));
}
function scrollTop(state) { return state.owner === window ? window.scrollY : state.owner.scrollTop; }
function finishNavigation(state, navigation) {
    if (state.disposed || state.navigation !== navigation) return;
    cancelNavigation(state);
    if (!navigation.band.isConnected || !state.targets.some(target => target.band === navigation.band)) return;
    const difference = navigation.band.getBoundingClientRect().top - alignmentTop(state);
    // Correct a settled sticky header once, rather than maintaining a scroll lock.
    if (Math.abs(difference) > 1) state.owner.scrollTo({ top:Math.max(0, scrollTop(state) + difference), behavior:'auto' });
    update(state);
}
function scheduleAlignment(state) {
    if (!state.navigation || state.disposed) return;
    clearTimeout(state.navigation.idleTimer);
    const navigation = state.navigation;
    navigation.idleTimer = setTimeout(() => finishNavigation(state, navigation), 120);
}
function beginNavigation(state, band) {
    cancelNavigation(state);
    const navigation = state.navigation = { band, idleTimer:0, deadlineTimer:0 };
    interruptionEvents.forEach(type => document.addEventListener(type, state.interrupt, { capture:true, passive:true }));
    navigation.deadlineTimer = setTimeout(() => finishNavigation(state, navigation), 1000);
    scheduleAlignment(state);
    update(state);
}
function update(state) {
    if (state.disposed || !state.targets.length) { state.root.hidden = true; return; }
    const bounds = viewport(state), content = state.content.getBoundingClientRect();
    const left = Math.max(bounds.left, content.left), right = Math.min(bounds.right, content.right);
    const top = Math.max(alignmentTop(state), content.top);
    let bottom = Math.min(bounds.bottom, content.bottom);
    const visibleBands = state.targets.map(({ band }) => ({ band, rect:band.getBoundingClientRect() })).filter(({ rect }) =>
        rect.height > 0 && rect.bottom > top && rect.top < bottom && rect.left < right && rect.right > left);
    // Keep the floating links within actual bands, without adding a row after the footer.
    if (visibleBands.length) bottom = Math.min(bottom, Math.max(...visibleBands.map(({ rect }) => rect.bottom)));
    state.root.hidden = !visibleBands.length || right - left < 64 || bottom - top < 64;
    state.root.style.setProperty('--f-presentation-progress-width', Math.max(0, right - left - 24) + 'px');
    state.root.style.setProperty('--f-presentation-progress-left', (left + (right - left) / 2) + 'px');
    const height = Math.max(44, state.root.getBoundingClientRect().height);
    const navigationTop = Math.max(top + 8, bottom - height - 16);
    state.root.style.setProperty('--f-presentation-progress-top', navigationTop + 'px');
    state.root.hidden ||= bottom - top < height + 24;
    // The bottom links can be over a different tone than the band active at the viewport's top.
    const centerX = left + (right - left) / 2, centerY = navigationTop + height / 2;
    const backgroundBand = visibleBands.find(({ rect }) => rect.left <= centerX && rect.right > centerX
        && rect.top <= centerY && rect.bottom > centerY)?.band;
    if (backgroundBand) state.root.style.setProperty('--f-presentation-progress-foreground', getComputedStyle(backgroundBand).color);
    else state.root.style.removeProperty('--f-presentation-progress-foreground');
    // A new band becomes current once it fills roughly two thirds of the usable
    // viewport. Its top crosses the upper-third line; tall bands need not fit.
    const usableTop = alignmentTop(state);
    const boundary = usableTop + Math.max(0, bounds.bottom - usableTop) / 3;
    // Keep the chosen destination current while our own smooth scroll crosses intermediate bands.
    let active = state.targets.find(target => target.band === state.navigation?.band);
    if (!active) {
        active = state.targets[0];
        for (const target of state.targets) if (target.band.getBoundingClientRect().top <= boundary) active = target;
    }
    for (const target of state.targets) {
        if (target === active) target.link.setAttribute('aria-current', 'location');
        else target.link.removeAttribute('aria-current');
    }
}
function refresh(state) {
    if (state.disposed) return;
    if (!state.root.isConnected) { detach(state.root); return; }
    const content = contentFor(state.root, state.contentId);
    if (content !== state.content || !state.content.isConnected) {
        const { root, contentId } = state;
        detach(root);
        synchronize(root, contentId);
        return;
    }
    const owner = scrollOwner(state.content);
    if (state.owner !== owner) {
        cancelNavigation(state);
        state.owner?.removeEventListener('scroll', state.scroll);
        state.owner = owner;
        owner.addEventListener('scroll', state.scroll, { passive:true });
    }
    state.headers = state.surface ? [...state.surface.children].filter(node => node.classList.contains('f-document-surface-header')) : [];
    state.headers.push(...[...state.content.querySelectorAll('.f-page-heading')].filter(header =>
        header.closest('main') === state.content.closest('main') && (!state.surface || header.closest('.content-surface') === state.surface)));
    const bands = eligibleBands(state);
    releaseTargets(state, bands);
    const entries = bands.map((band, index) => {
        if (!band.id) {
            let id;
            do { id = state.root.id + '-band-' + ++state.sequence; } while (document.getElementById(id));
            band.id = id;
            state.generatedIds.set(band, id);
        }
        return { band, id:band.id, label:labelFor(band, index) };
    });
    if (entries.length !== state.targets.length || entries.some((entry, index) =>
        entry.band !== state.targets[index].band || entry.id !== state.targets[index].id || entry.label !== state.targets[index].label)) {
        const fragment = document.createDocumentFragment();
        state.targets = entries.map(entry => {
            const item = document.createElement('li'), link = document.createElement('a');
            link.className = 'f-presentation-progress-link f-tutorial-segment';
            link.setAttribute('href', '#' + encodeURIComponent(entry.id));
            link.setAttribute('aria-label', entry.label);
            item.append(link); fragment.append(item);
            return { ...entry, link };
        });
        state.root.querySelector('ol').replaceChildren(fragment);
    }
    if (state.navigation && !bands.includes(state.navigation.band)) cancelNavigation(state);
    const ancestry = ancestors(state.content);
    if (!state.ancestry || ancestry.length !== state.ancestry.length || ancestry.some((node, index) => node !== state.ancestry[index])) {
        state.geometryMutation.disconnect();
        ancestry.filter(node => node !== state.content && node !== state.surface).forEach(node =>
            state.geometryMutation.observe(node, { attributes:true, attributeFilter:['class','style','hidden'] }));
        state.ancestry = ancestry;
    }
    const observed = [...new Set([state.root, ...ancestry, ...state.headers, ...bands])];
    if (!state.observed || observed.length !== state.observed.length || observed.some((node, index) => node !== state.observed[index])) {
        state.resize.disconnect();
        observed.forEach(node => state.resize.observe(node));
        state.observed = observed;
    }
    update(state);
}
export function synchronize(root, contentId) {
    const content = contentFor(root, contentId);
    let state = instances.get(root);
    if (state && (state.content !== content || state.contentId !== contentId)) { detach(root); state = null; }
    if (!content) { root.hidden = true; return; }
    if (!state) {
        state = { root, content, contentId, surface:root.closest('.content-surface'), targets:[], headers:[],
            generatedIds:new Map(), focusTargets:new Set(), sequence:0, frame:0, disposed:false };
        state.schedule = () => {
            if (state.frame || state.disposed) return;
            state.frame = requestAnimationFrame(() => { state.frame = 0; refresh(state); scheduleAlignment(state); });
        };
        state.scroll = () => { update(state); scheduleAlignment(state); };
        state.interrupt = () => { cancelNavigation(state); update(state); };
        state.click = event => {
            const link = event.target.closest('.f-presentation-progress-link');
            const target = state.targets.find(entry => entry.link === link);
            if (!target || !root.contains(link) || event.button > 0 || event.ctrlKey || event.metaKey || event.shiftKey || event.altKey) return;
            event.preventDefault();
            const top = Math.max(0, scrollTop(state) + target.band.getBoundingClientRect().top - alignmentTop(state));
            beginNavigation(state, target.band);
            state.owner.scrollTo({ top, behavior:state.motion.matches ? 'auto' : 'smooth' });
            const heading = target.band.querySelector('h1,h2,h3,h4,h5,h6') ?? target.band;
            if (!heading.hasAttribute('tabindex')) { heading.setAttribute('tabindex', '-1'); state.focusTargets.add(heading); }
            heading.focus({ preventScroll:true });
        };
        state.motion = matchMedia('(prefers-reduced-motion: reduce)');
        state.motionChanged = () => {
            if (state.motion.matches && state.navigation) finishNavigation(state, state.navigation);
        };
        state.motion.addEventListener?.('change', state.motionChanged);
        root.addEventListener('click', state.click);
        window.addEventListener('resize', state.schedule);
        document.addEventListener('scroll', state.scroll, { capture:true, passive:true });
        window.visualViewport?.addEventListener('resize', state.schedule);
        window.visualViewport?.addEventListener('scroll', state.scroll);
        state.mutation = new MutationObserver(records => {
            if (records.some(record => !root.contains(record.target))) state.schedule();
        });
        state.mutation.observe(state.surface ?? content, { childList:true, subtree:true, characterData:true, attributes:true,
            attributeFilter:['id','hidden','class','style','aria-label','aria-labelledby','data-presentation-band','open'] });
        state.geometryMutation = new MutationObserver(state.schedule);
        state.removal = new MutationObserver(() => { if (!root.isConnected || !content.isConnected) state.schedule(); });
        state.removal.observe(document.body, { childList:true, subtree:true });
        state.resize = new ResizeObserver(state.schedule);
        instances.set(root, state);
    }
    refresh(state);
}
export function detach(root) {
    const state = instances.get(root);
    if (!state) return;
    state.disposed = true;
    cancelNavigation(state);
    if (state.frame) cancelAnimationFrame(state.frame);
    state.owner?.removeEventListener('scroll', state.scroll);
    root.removeEventListener('click', state.click);
    window.removeEventListener('resize', state.schedule);
    document.removeEventListener('scroll', state.scroll, true);
    window.visualViewport?.removeEventListener('resize', state.schedule);
    window.visualViewport?.removeEventListener('scroll', state.scroll);
    state.motion.removeEventListener?.('change', state.motionChanged);
    state.mutation.disconnect(); state.geometryMutation.disconnect(); state.removal.disconnect(); state.resize.disconnect();
    releaseTargets(state);
    state.focusTargets.forEach(target => { if (target.getAttribute('tabindex') === '-1') target.removeAttribute('tabindex'); });
    state.focusTargets.clear();
    root.querySelector('ol')?.replaceChildren();
    for (const property of ['left','top','width','foreground']) root.style.removeProperty('--f-presentation-progress-' + property);
    root.hidden = true;
    instances.delete(root);
}

// Static SSR surfaces share the same controller as interactive component interop.
// The framework browser entry calls this on initial and enhanced page loads.
export function initializePresentationProgress(scope = document) {
    const roots = [...scope.querySelectorAll('.f-presentation-progress')];
    if (scope.matches?.('.f-presentation-progress')) roots.unshift(scope);
    roots.forEach(root => synchronize(root, root.getAttribute('data-content-id') ?? ''));
}
