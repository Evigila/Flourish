// The production clipboard contract requires the current browser Clipboard API.
export async function copyText(text) {
    try {
        await navigator.clipboard.writeText(text);
        return true;
    } catch { return false; }
}
