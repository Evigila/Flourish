import { updateCompactHeading, resetCompactHeading } from '../shell.js';

const surfaces = new WeakMap();
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
export function dispose(root) {
    const state = surfaces.get(root);
    if (!state) return;
    state.stage.removeEventListener('scroll', state.update);
    state.observer?.disconnect();
    resetCompactHeading(root);
    surfaces.delete(root);
}
