import { resolveInvoker } from "./interaction-origin.js";

const sheets = new WeakMap();

export function synchronize(dialog, isOpen, reference) {
    let state = sheets.get(dialog);
    if (!state) {
        state = { reference, pending: false, opener: null };
        const cancel = async (event) => {
            // Prevent native dismissal before the asynchronous server-side busy check.
            event.preventDefault();
            if (dialog.getAttribute("aria-busy") === "true" || state.pending) return;

            state.pending = true;
            try {
                await state.reference.invokeMethodAsync("RequestCloseAsync");
            } catch {
                // Leave the form intact if the circuit is reconnecting.
            } finally {
                state.pending = false;
            }
        };
        dialog.addEventListener("cancel", cancel);

        // Blazor may remove the dialog before .NET disposal can call into the DOM.
        const observer = new MutationObserver(() => {
            if (!dialog.isConnected) {
                dispose(dialog);
            }
        });
        observer.observe(document.body, { childList: true, subtree: true });
        state.dispose = () => {
            dialog.removeEventListener("cancel", cancel);
            observer.disconnect();
        };
        sheets.set(dialog, state);
    }

    state.reference = reference;
    if (isOpen && !dialog.open) {
        state.opener = resolveInvoker(document.activeElement);
        dialog.showModal();
        dialog.querySelector("[data-sheet-scroll]")?.scrollTo(0, 0);
    } else if (!isOpen && dialog.open) {
        dialog.close();
        const opener = (state.opener?.isConnected
            ? state.opener
            : document.querySelector(`[aria-controls="${CSS.escape(dialog.id)}"]`))
            ?? document.querySelector(`[data-sheet-return-focus="${CSS.escape(dialog.id)}"]`);
        opener?.focus({ preventScroll: true });
    }
}

export function dispose(dialog) {
    sheets.get(dialog)?.dispose();
    sheets.delete(dialog);
}
