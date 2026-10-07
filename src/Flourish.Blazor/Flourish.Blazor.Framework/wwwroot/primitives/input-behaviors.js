import { attachAccessFormSpacing, detachAccessFormSpacing } from '../controls.js';

const selectableInputTypes = new Set(["text", "email", "tel", "url", "number"]);
const accessFormRoots = new Set();
function synchronizeAccessForms() {
    for (const root of accessFormRoots) {
        if (root.isConnected) continue;
        detachAccessFormSpacing(root); accessFormRoots.delete(root);
    }
    for (const root of document.querySelectorAll('.f-access-form-surface')) {
        if (accessFormRoots.has(root)) continue;
        attachAccessFormSpacing(root); accessFormRoots.add(root);
    }
}
if (document.readyState === 'loading') document.addEventListener('DOMContentLoaded', synchronizeAccessForms, { once:true });
else synchronizeAccessForms();
const accessFormObserver = new MutationObserver(synchronizeAccessForms);
accessFormObserver.observe(document.documentElement, { childList:true, subtree:true });

document.addEventListener("focusin", event => {
    const input = event.target;
    if (!(input instanceof HTMLInputElement)
        || !selectableInputTypes.has(input.type)
        || input.disabled
        || input.readOnly
        || input.value.length === 0
        || input.dataset.inputMask !== undefined
        || input.dataset.preserveSelection === "true") return;

    requestAnimationFrame(() => {
        if (document.activeElement === input) input.select();
    });
});

// Format before Blazor captures the event value, so its binding sees the same text as the DOM.
document.addEventListener("input", event => {
    const input = event.target;
    if (!(input instanceof HTMLInputElement) || !input.dataset.inputMask) return;

    const mask = input.dataset.inputMask;
    const selectionStart = input.selectionStart ?? input.value.length;
    const prefix = input.value.slice(0, selectionStart);
    const rawValue = normalizeMaskValue(mask, input.value);
    const rawPrefix = normalizeMaskValue(mask, prefix);
    const formattedValue = formatMaskValue(mask, rawValue);
    const formattedPrefix = formatMaskValue(mask, rawPrefix);

    input.value = formattedValue;
    input.setSelectionRange(formattedPrefix.length, formattedPrefix.length);
}, { capture: true });

function normalizeMaskValue(mask, value) {
    const candidates = Array.from(value).filter(character => /[0-9A-Za-z]/.test(character));
    const result = [];
    let candidateIndex = 0;

    for (const slot of Array.from(mask).filter(character => character === "0" || character === "A")) {
        while (candidateIndex < candidates.length) {
            const candidate = candidates[candidateIndex++];
            if (slot === "0" && !/[0-9]/.test(candidate)) continue;
            result.push(slot === "A" ? candidate.toUpperCase() : candidate);
            break;
        }
    }

    return result.join("");
}

function formatMaskValue(mask, value) {
    const normalized = normalizeMaskValue(mask, value);
    if (normalized.length === 0) return "";

    let formatted = "";
    let valueIndex = 0;
    for (const maskCharacter of mask) {
        if (maskCharacter === "0" || maskCharacter === "A") {
            if (valueIndex >= normalized.length) break;
            formatted += normalized[valueIndex++];
        } else if (valueIndex > 0) {
            formatted += maskCharacter;
        }
    }

    return formatted;
}
