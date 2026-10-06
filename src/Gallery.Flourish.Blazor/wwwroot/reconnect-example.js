import { synchronizeDialog, setDialogView } from './_content/Arkheide.Flourish.Blazor.Framework/controls.js';

const bindings = new WeakMap();

// This example owns only fictional protocol transitions. Dialog owns modality and focus.
export function attach(root) {
    detach(root);
    const dialog = root.querySelector('dialog');
    if (!dialog) return;
    const click = event => {
        const action = event.target.closest('[data-gallery-reconnect-view], [data-gallery-reconnect-close]');
        if (!action || !root.contains(action) || action.disabled) return;
        if (action.hasAttribute('data-gallery-reconnect-close')) {
            synchronizeDialog(dialog, false, null);
            return;
        }
        setDialogView(dialog, action.dataset.galleryReconnectView);
        synchronizeDialog(dialog, true, null);
    };
    root.addEventListener('click', click);
    bindings.set(root, click);
}

export function detach(root) {
    const click = bindings.get(root);
    if (click) root.removeEventListener('click', click);
    bindings.delete(root);
}
