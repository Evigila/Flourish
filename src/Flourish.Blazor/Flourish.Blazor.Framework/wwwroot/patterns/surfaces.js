import { updateCompactHeading, resetCompactHeading } from '../shell.js';

const surfaces = new WeakMap();
const topControls = new WeakMap();
export function synchronize(root, stage, top) {
    let state = surfaces.get(root);
    if (state?.stage !== stage) {
        dispose(root);
        state = {stage, top, observer: null};
        state.update = () => {
            updateCompactHeading(root, stage);
            if (state.top) state.top.hidden = stage.scrollTop < 280;
        };
        stage.addEventListener('scroll', state.update, {passive:true});
        state.observer = new MutationObserver(() => { if (!root.isConnected) dispose(root); });
        state.observer.observe(document.body, {childList:true, subtree:true});
        surfaces.set(root, state);
    }
    state.top = top;
    state.update();
}
export function scrollToTop(stage) { stage.scrollTo({top:0,behavior:'auto'}); }
export function synchronizeBackToTop(root, contentId, threshold) {
    const content = document.getElementById(contentId);
    let state = topControls.get(root);
    if (state?.content !== content) { disposeBackToTop(root); state = null; }
    if (!content) { root.hidden = true; return; }
    if (!state) {
        state = { content, threshold };
        state.update = () => {
            if (!root.isConnected || !content.isConnected) { disposeBackToTop(root); return; }
            const bounds = content.getBoundingClientRect();
            const left = Math.max(0, bounds.left), right = Math.min(window.innerWidth, bounds.right);
            const top = Math.max(0, bounds.top), bottom = Math.min(window.innerHeight, bounds.bottom);
            root.hidden = content.scrollTop < state.threshold || right - left < 64 || bottom - top < 64;
            root.style.setProperty('--f-back-to-top-left', `${Math.max(left + 8, right - 72)}px`);
            root.style.setProperty('--f-back-to-top-top', `${Math.max(top + 8, bottom - 72)}px`);
            root.setAttribute('data-enhanced', '');
        };
        state.click = event => {
            const link = event.target.closest('a');
            if (!link || !root.contains(link) || event.button > 0 || event.ctrlKey || event.metaKey || event.shiftKey || event.altKey) return;
            event.preventDefault();
            content.scrollTo({ top:0, behavior:matchMedia('(prefers-reduced-motion: reduce)').matches ? 'auto' : 'smooth' });
            // The owning shells already supply tabindex=-1; standalone regions opt in explicitly.
            if (content.hasAttribute('tabindex')) content.focus({ preventScroll:true });
        };
        content.addEventListener('scroll', state.update, { passive:true });
        root.addEventListener('click', state.click);
        window.addEventListener('resize', state.update);
        document.addEventListener('scroll', state.update, { capture:true, passive:true });
        state.mutation = new MutationObserver(state.update);
        state.mutation.observe(document.body, { childList:true, subtree:true });
        if (typeof ResizeObserver !== 'undefined') { state.resize = new ResizeObserver(state.update); state.resize.observe(content); }
        topControls.set(root, state);
    }
    state.threshold = threshold;
    state.update();
}
export function disposeBackToTop(root) {
    const state = topControls.get(root);
    if (!state) return;
    state.content.removeEventListener('scroll', state.update);
    root.removeEventListener('click', state.click);
    window.removeEventListener('resize', state.update);
    document.removeEventListener('scroll', state.update, true);
    state.mutation.disconnect();
    state.resize?.disconnect();
    root.removeAttribute('data-enhanced');
    root.style.removeProperty('--f-back-to-top-left');
    root.style.removeProperty('--f-back-to-top-top');
    root.hidden = false;
    topControls.delete(root);
}
export function dispose(root) {
    const state = surfaces.get(root);
    if (!state) return;
    state.stage.removeEventListener('scroll', state.update);
    state.observer?.disconnect();
    resetCompactHeading(root);
    surfaces.delete(root);
}
