// Preserve the invoker when a transient menu disappears before an async dialog opens.
let pendingInvoker = null;
let recordedAt = 0;

export function rememberInvoker(element) {
    pendingInvoker = element ? new WeakRef(element) : null;
    recordedAt = performance.now();
}

export function resolveInvoker(current) {
    const remembered = pendingInvoker?.deref();
    const recent = performance.now() - recordedAt < 5000;
    pendingInvoker = null;
    if (current && current !== document.body && current !== document.documentElement
        && current.isConnected && !current.closest('[role="menu"], [role="menuitem"]')) return current;
    return recent && remembered?.isConnected ? remembered : null;
}
