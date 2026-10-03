export function containsFocus(element) {
    return element?.contains(document.activeElement) ?? false;
}
