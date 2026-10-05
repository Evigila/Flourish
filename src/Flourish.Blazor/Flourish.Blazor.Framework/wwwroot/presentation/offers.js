// Each stage owns its cards, matching native links, timer and progressive enhancement.
const stages = new WeakMap();
const mediaQuery = "(min-width: 1100px) and (prefers-reduced-motion: no-preference)";

export function synchronize(root, linkScopeId, autoRotate = true, interval = 2200) {
    if (!root || !root.isConnected) return;
    const cards = Array.from(root.children).filter(card => card.classList.contains("f-offer-card"));
    const document = root.ownerDocument;
    const scope = linkScopeId ? document.getElementById(linkScopeId) : null;
    const links = scope ? Array.from(scope.querySelectorAll("a[href]")) : [];
    const previous = stages.get(root);
    if (previous && previous.same(cards, links, linkScopeId, autoRotate, interval)) return;
    const activeId = previous?.activeId();
    dispose(root);
    // Duplicate or absent target IDs cannot be enhanced safely; leave every offer readable.
    if (!cards.length || cards.some(card => !card.id) || new Set(cards.map(card => card.id)).size !== cards.length) return;
    const window = document.defaultView;
    if (!window?.matchMedia) return;
    const media = window.matchMedia(mediaQuery);
    const interactionRoot = root.closest?.(".f-offer-presentation") ?? root;
    const removers = [];
    let timer;
    let index = Math.max(0, cards.findIndex(card => card.id === activeId));
    let pointerInside = false;
    let disposed = false;
    function listen(node, event, handler) {
        node.addEventListener(event, handler);
        removers.push(() => node.removeEventListener(event, handler));
    }
    function focusedCard() { return cards.find(card => card.contains(document.activeElement)); }
    function stop() { if (timer !== undefined) { window.clearInterval(timer); timer = undefined; } }
    function updateTimer() {
        stop();
        if (!disposed && media.matches && autoRotate && cards.length > 1 && !pointerInside && !interactionRoot.contains(document.activeElement) && !document.hidden)
            timer = window.setInterval(() => activate((index + 1) % cards.length), interval);
    }
    function paint() { cards.forEach((card, i) => card.dataset.active = String(media.matches && i === index)); }
    function activate(next, explicit = false) {
        const focused = focusedCard();
        if (!explicit && focused && focused !== cards[next]) return;
        index = next;
        paint();
    }
    function mode() {
        if (media.matches) {
            root.style.setProperty("--f-offer-columns", String(cards.length + 1));
            root.dataset.offerReady = "";
        } else {
            root.removeAttribute("data-offer-ready");
            root.style.removeProperty("--f-offer-columns");
        }
        paint();
        updateTimer();
    }
    cards.forEach((card, i) => {
        listen(card, "pointerenter", () => activate(i));
        listen(card, "pointerdown", () => activate(i));
        listen(card, "focusin", () => activate(i, true));
    });
    listen(interactionRoot, "pointerenter", () => { pointerInside = true; updateTimer(); });
    listen(interactionRoot, "pointerleave", () => { pointerInside = false; updateTimer(); });
    listen(interactionRoot, "focusin", () => updateTimer());
    listen(interactionRoot, "focusout", () => window.queueMicrotask(() => { if (!disposed) updateTimer(); }));
    listen(document, "visibilitychange", updateTimer);
    if (scope) {
        for (const link of links) {
            let target;
            try {
                const url = new URL(link.href, document.baseURI);
                const page = new URL(document.URL);
                if (url.origin !== page.origin || url.pathname !== page.pathname || url.search !== page.search) continue;
                target = decodeURIComponent(url.hash.slice(1));
            } catch { continue; }
            const next = cards.findIndex(card => card.id === target);
            if (next < 0) continue;
            listen(link, "click", event => {
                if (event.defaultPrevented || event.button !== 0 || event.ctrlKey || event.metaKey || event.altKey || event.shiftKey) return;
                activate(next, true);
                cards[next].focus({ preventScroll: true });
                // No preventDefault: native fragment navigation remains available.
            });
        }
    }
    listen(media, "change", mode);
    const observer = window.MutationObserver ? new window.MutationObserver(() => {
        if (!root.isConnected) dispose(root);
        else synchronize(root, linkScopeId, autoRotate, interval);
    }) : null;
    observer?.observe(document.documentElement, { childList: true, subtree: true });
    stages.set(root, {
        same: (next, nextLinks, scopeId, rotate, duration) => scopeId === linkScopeId && rotate === autoRotate && duration === interval && next.length === cards.length && next.every((card, i) => card === cards[i]) && nextLinks.length === links.length && nextLinks.every((link, i) => link === links[i]),
        activeId: () => cards[index]?.id,
        dispose: () => {
            disposed = true;
            stop();
            observer?.disconnect();
            removers.forEach(remove => remove());
            root.removeAttribute("data-offer-ready");
            root.style.removeProperty("--f-offer-columns");
            cards.forEach(card => card.removeAttribute("data-active"));
        }
    });
    mode();
}

export function dispose(root) {
    stages.get(root)?.dispose();
    stages.delete(root);
}
