import { suppressPrimaryNavigationTooltip } from '../primitives/primary-navigation-item.js';

const previews = new Map();
const marker = '[data-f-primary-preview]';
const itemSelector = '.primary-nav-item,.f-primary-item';
const ownedProperties = ['top','height','left','right','available','target-width','target-height','inline-inset','padding-top','padding-bottom'];
const itemProperties = ['row-height','icon-inline-inset','icon-block-inset'];
let active = null, removalObserver;
const px = value => `${Math.round(value * 100) / 100}px`;
const set = (element, name, value) => element.style.setProperty(`--f-primary-preview-${name}`, value);

function restoreFocus(previous) {
    // Manual popovers do not request auto/hint focus restoration. Preserve an
    // existing focus only if the rendering transition itself cleared it to body.
    if (previous?.isConnected && document.activeElement === document.body && previous !== document.body)
        previous.focus({ preventScroll:true });
}
function scrollPositions(rail) {
    return [rail, ...rail.querySelectorAll('.f-primary-navigation-main')].map(element => [element, element.scrollTop]);
}
function restoreScroll(positions) { for (const [element, top] of positions) element.scrollTop = top; }
function close(state) {
    if (!state.open) return;
    state.open = false;
    const focused = document.activeElement, positions = scrollPositions(state.rail);
    for (const position of positions) if (state.presented && position[0] === state.scroller) position[1] = Math.max(0, position[1] - (state.clip ?? 0));
    try { if (state.rail.matches(':popover-open')) state.rail.hidePopover(); } catch { /* Removed DOM is restored below. */ }
    state.rail.removeAttribute('popover');
    state.rail.removeAttribute('data-f-primary-preview-open');
    state.rail.removeAttribute('data-f-primary-preview-direction');
    for (const name of ownedProperties) state.rail.style.removeProperty(`--f-primary-preview-${name}`);
    for (const item of state.items ?? [])
        for (const name of itemProperties) item.element.style.removeProperty(`--f-primary-preview-${name}`);
    state.items = []; state.nodes = [];
    state.slot.removeAttribute('data-f-primary-preview-active');
    state.slot.style.removeProperty('--f-primary-preview-reserved-width');
    restoreScroll(positions);
    state.presented = false; state.clip = 0;
    restoreFocus(focused);
    if (active === state) active = null;
}
function position(state) {
    const anchor = state.slot.getBoundingClientRect(), owner = state.host.getBoundingClientRect();
    const width = document.documentElement.clientWidth, height = window.innerHeight;
    const left = Math.max(0, owner.left), right = Math.min(width, owner.right);
    const top = Math.max(0, anchor.top, owner.top), bottom = Math.min(height, anchor.bottom, owner.bottom);
    const rtl = getComputedStyle(state.rail).direction === 'rtl';
    const edge = rtl ? Math.min(right, anchor.right) : Math.max(left, anchor.left);
    const available = rtl ? edge - left : right - edge;
    if (!(right > left) || !(bottom > top) || !(available > 0)) return false;
    const clip = Math.max(0, top - anchor.top);
    if (state.presented) state.scroller.scrollTop = Math.max(0, state.scroller.scrollTop + clip - state.clip);
    state.clip = clip;
    set(state.rail, 'top', px(top));
    set(state.rail, 'height', px(bottom - top));
    set(state.rail, 'left', rtl ? 'auto' : px(edge));
    set(state.rail, 'right', rtl ? px(width - edge) : 'auto');
    set(state.rail, 'available', px(available));
    state.rail.setAttribute('data-f-primary-preview-direction', rtl ? 'rtl' : 'ltr');
    return true;
}
function open(state) {
    if (state.open || state.dismissed || state.disposed || !state.rail.isConnected || !state.rail.matches(marker)
        || typeof state.rail.showPopover !== 'function') return;
    const nodes = [...state.rail.querySelectorAll(itemSelector)];
    const items = nodes.filter(item => {
        const box = item.getBoundingClientRect();
        return box.width > 0 && box.height > 0;
    });
    const first = items[0];
    if (!first) return;
    if (active && active !== state) close(active);
    const rail = state.rail.getBoundingClientRect(), target = first.getBoundingClientRect();
    if (!(rail.width > 0) || !(target.width > 0)) return;
    const focused = document.activeElement, positions = scrollPositions(state.rail);
    state.scroller = state.rail.querySelector('.f-primary-navigation-main') ?? state.rail;
    const style = getComputedStyle(state.rail), rtl = style.direction === 'rtl';
    const length = name => Number.parseFloat(style[name]) || 0;
    // Adding popover introduces UA padding and borders. Freeze the normal-flow
    // block inset, including its old border, before neutralizing those defaults.
    set(state.rail, 'padding-top', px(length('paddingTop') + length('borderTopWidth')));
    set(state.rail, 'padding-bottom', px(length('paddingBottom') + length('borderBottomWidth')));
    state.items = items.map(element => {
        const box = element.getBoundingClientRect(), icon = element.querySelector('.primary-nav-symbol,.f-icon');
        set(element, 'row-height', px(box.height));
        return { element, iconElement:icon, icon:icon?.getBoundingClientRect() };
    });
    state.nodes = nodes;
    set(state.slot, 'reserved-width', px(rail.width));
    state.slot.setAttribute('data-f-primary-preview-active', '');
    set(state.rail, 'target-width', px(target.width));
    set(state.rail, 'target-height', px(target.height));
    set(state.rail, 'inline-inset', px(Math.max(0, rtl ? rail.right - target.right : target.left - rail.left)));
    state.open = true;
    if (!position(state)) { close(state); return; }
    state.rail.setAttribute('data-f-primary-preview-open', '');
    state.rail.setAttribute('popover', 'manual');
    try { state.rail.showPopover(); }
    catch { close(state); return; }
    restoreScroll(positions);
    state.scroller.scrollTop += state.clip;
    state.presented = true;
    const expanded = first.getBoundingClientRect();
    const inset = Number.parseFloat(state.rail.style.getPropertyValue('--f-primary-preview-inline-inset')) || 0;
    // Inline scrollbars consume space inside the live scroll owner too; do not
    // count that gutter again as padding when expanding the same rail.
    const adjustment = rtl ? expanded.right - target.right : target.left - expanded.left;
    set(state.rail, 'inline-inset', px(Math.max(0, inset + adjustment)));
    // A command button's padding, a rail border or an RTL scrollbar can put
    // its real icon away from border-box/2. Align each existing icon to its
    // measured collapsed position using the actual expanded row origin.
    for (const item of state.items) {
        if (!item.icon) continue;
        const box = item.element.getBoundingClientRect();
        set(item.element, 'icon-inline-inset', px(rtl ? box.right - item.icon.right : item.icon.left - box.left));
        set(item.element, 'icon-block-inset', px(item.icon.top - box.top));
    }
    restoreFocus(focused);
    active = state;
    suppressPrimaryNavigationTooltip(state.rail);
}
function dispose(rail) {
    const state = previews.get(rail);
    if (!state) return;
    state.disposed = true;
    close(state);
    if (state.frame) cancelAnimationFrame(state.frame);
    state.resize.disconnect();
    for (const remove of state.removers) remove();
    previews.delete(rail);
    if (!previews.size) { removalObserver?.disconnect(); removalObserver = null; }
}
function prune(mutations = []) {
    for (const [rail, state] of previews) {
        if (!rail.isConnected || !rail.matches(marker) || !state.slot.contains(rail)) dispose(rail);
        else if (state.open) {
            const nodes = [...rail.querySelectorAll(itemSelector)];
            // A rerender can replace an anchor with a disabled span. Reopen
            // from its real collapsed geometry on the next interaction.
            if (nodes.length !== state.nodes.length || nodes.some((item, index) => state.nodes[index] !== item)
                || state.items.some(item => item.iconElement !== item.element.querySelector('.primary-nav-symbol,.f-icon'))
                || mutations.some(change => change.attributeName === 'hidden' && (rail.contains(change.target) || change.target?.contains?.(rail)))) close(state);
        }
    }
}
export function initializePrimaryNavigationPreviews(scope = document) {
    prune();
    const rails = [...scope.querySelectorAll(marker)];
    if (scope.matches?.(marker)) rails.unshift(scope);
    for (const rail of rails) {
        if (previews.has(rail)) continue;
        const slot = rail.closest('[data-f-primary-preview-slot]'), host = rail.closest('.navigation-surface,.f-shell');
        if (!slot || !host) continue;
        const state = { rail, slot, host, open:false, hovered:false, keyboard:false, dismissed:false, frame:0, removers:[] };
        const listen = (target, name, handler, options) => {
            target.addEventListener(name, handler, options);
            state.removers.push(() => target.removeEventListener(name, handler, options));
        };
        const schedule = () => {
            if (state.open && !state.frame) state.frame = requestAnimationFrame(() => {
                state.frame = 0;
                if (!position(state)) close(state);
            });
        };
        listen(rail, 'pointerenter', event => {
            if (event.pointerType !== 'mouse') return;
            state.hovered = true;
            open(state);
        });
        listen(rail, 'pointerleave', event => {
            if (rail.contains(event.relatedTarget)) return;
            state.hovered = false; state.dismissed = false;
            if (!state.keyboard) close(state);
        });
        listen(document, 'pointerdown', event => {
            state.keyboard = false;
            if (!rail.contains(event.target)) close(state);
        }, true);
        listen(document, 'keydown', event => {
            if (event.key === 'Escape' && state.open) {
                const ownsKeyboard = rail.contains(document.activeElement);
                state.dismissed = true; state.keyboard = false;
                close(state);
                if (ownsKeyboard) { event.preventDefault(); event.stopPropagation(); }
            } else if (event.key === 'Tab' && !event.altKey && !event.ctrlKey && !event.metaKey) {
                state.dismissed = false;
            }
        }, true);
        listen(rail, 'focusin', event => {
            if (!event.target.closest(itemSelector)?.matches(':focus-visible')) return;
            state.keyboard = true;
            open(state);
        });
        listen(rail, 'focusout', () => queueMicrotask(() => {
            if (!rail.contains(document.activeElement)) { state.keyboard = false; if (!state.hovered) close(state); }
        }));
        // Viewport changes release the frozen narrow reservation before CSS
        // breakpoints choose a different rail size. The next interaction reopens.
        const release = () => {
            state.dismissed = true; state.hovered = state.keyboard = false;
            close(state);
            state.dismissed = false;
        };
        listen(window, 'resize', release);
        listen(window, 'blur', release);
        listen(document, 'scroll', event => { if (!rail.contains(event.target)) schedule(); }, true);
        state.resize = new ResizeObserver(schedule);
        state.resize.observe(slot); state.resize.observe(host);
        previews.set(rail, state);
    }
    if (previews.size && !removalObserver) {
        removalObserver = new MutationObserver(prune);
        removalObserver.observe(document.documentElement, { childList:true, subtree:true, attributes:true, attributeFilter:['data-f-primary-preview','hidden'] });
    }
}
export function disposePrimaryNavigationPreviews(scope) {
    for (const [rail, state] of previews) if (scope === state.host || scope === rail || scope.contains(rail)) dispose(rail);
}
