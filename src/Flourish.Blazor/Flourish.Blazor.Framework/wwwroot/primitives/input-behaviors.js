const selectableInputTypes = new Set(["text", "email", "tel", "url", "number"]);

document.addEventListener("focusin", event => {
    const input = event.target;
    if (!(input instanceof HTMLInputElement)
        || !selectableInputTypes.has(input.type)
        || input.disabled
        || input.readOnly
        || input.value.length === 0
        || input.dataset.preserveSelection === "true") return;

    requestAnimationFrame(() => {
        if (document.activeElement === input) input.select();
    });
});

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
});

document.addEventListener("dragstart", event => {
    const option = event.target instanceof Element
        ? event.target.closest("[data-column-drag-key][draggable='true']")
        : null;
    if (!option || !event.dataTransfer) return;
    event.dataTransfer.effectAllowed = "move";
    event.dataTransfer.setData("text/plain", option.dataset.columnDragKey ?? "");
});

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
