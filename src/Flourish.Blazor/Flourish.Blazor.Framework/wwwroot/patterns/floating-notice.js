const documents = new WeakMap();
const frames = '[data-f-notice-frame]';
const panels = '[data-f-save-changes-bar][data-active="true"] > .f-save-changes-panel';
const gap = 8;
const property = name => '--f-notice-' + name;

function pixels(node, name, number) {
    const value = Math.round(number * 100) / 100 + 'px';
    if (node.style.getPropertyValue(property(name)) !== value) node.style.setProperty(property(name), value);
}
function release(node) {
    for (const name of ['left','width','bottom','limit','right','height']) node.style.removeProperty(property(name));
    node.removeAttribute('data-f-notice-docked');
}
function visible(node) {
    return node && !node.closest('[hidden],[inert],dialog:not([open])') && node.getClientRects().length > 0
        && node.ownerDocument.defaultView.getComputedStyle(node).visibility === 'visible';
}
function observe(state, next) {
    for (const node of state.observed) if (!next.has(node)) state.resize.unobserve(node);
    for (const node of next) if (!state.observed.has(node)) state.resize.observe(node);
    state.observed = next;
}
function own(state, node) {
    state.next.add(node);
    if (!node.hasAttribute('data-f-notice-docked')) node.setAttribute('data-f-notice-docked','');
}

// A separate fixed frame preserves Blazor's original DOM and the full unshrunk bar width.
// The actual message remains an ordinary Notice, including its hidden, ID and live attributes.
function naturalHeight(node) {
    const win = node.ownerDocument.defaultView, style = win.getComputedStyle(node);
    const number = value => Number.parseFloat(value) || 0;
    const children = [...(node.children ?? [])].filter(child => child.getClientRects().length);
    if (!children.length) return node.getBoundingClientRect().height;
    const heights = children.map(child => {
        const childStyle = win.getComputedStyle(child);
        return child.getBoundingClientRect().height + number(childStyle.marginTop) + number(childStyle.marginBottom);
    });
    const stacked = style.display !== 'flex' || style.flexDirection.startsWith('column');
    const content = stacked ? heights.reduce((sum,value) => sum + value,0) + number(style.rowGap) * (heights.length - 1) : Math.max(...heights);
    return content + number(style.paddingTop) + number(style.paddingBottom) + number(style.borderTopWidth) + number(style.borderBottomWidth);
}
function clearHeight(node) { node.style.removeProperty(property('height')); }
function arrange(state, dialog, group, view) {
    const frame = group.notices[0].parentElement.getBoundingClientRect();
    const body = dialog?.querySelector(':scope > .f-dialog-body');
    const bounds = (body ?? dialog)?.getBoundingClientRect();
    const left = Math.max(frame.left, view.left + gap, bounds ? bounds.left + gap : frame.left);
    const right = Math.min(frame.right, view.right - gap, bounds ? bounds.right - gap : frame.right);
    const bottom = Math.min(frame.bottom, view.bottom - gap, bounds ? bounds.bottom - gap : frame.bottom);
    const top = Math.max(view.top + gap, bounds ? bounds.top + gap : view.top);
    const width = Math.max(0,right - left), height = Math.max(0,bottom - top);
    const split = group.bars.length > 0 && width >= 720;
    const noticeWidth = split ? Math.floor((width - gap) * .42) : width;
    const barWidth = split ? width - gap - noticeWidth : width;
    const barLimit = Math.max(0,((split ? height : height * .6) - (group.bars.length - 1) * gap) / Math.max(1,group.bars.length));
    for (const bar of group.bars) {
        own(state,bar);
        pixels(bar,'width',barWidth);
        // Relative to the original fixed envelope, never to a transitioning panel.
        pixels(bar,'right',frame.right - right);
        pixels(bar,'limit',barLimit);
    }
    for (const notice of group.notices) {
        const root = notice.parentElement;
        own(state,root);
        pixels(root,'width',noticeWidth);
        pixels(root,'left',left - root.getBoundingClientRect().left);
    }
    // Direct content stays naturally sized inside the imposed outer height; measuring
    // the shell here would feed its previous equalized height back into the next pass.
    const barHeights = group.bars.map(naturalHeight), noticeHeights = group.notices.map(naturalHeight);
    const sharedHeight = split ? Math.min(height,Math.max(
        Math.max(...barHeights) * group.bars.length + (group.bars.length - 1) * gap,
        Math.max(...noticeHeights) * group.notices.length + (group.notices.length - 1) * gap)) : 0;
    let barStack = 0;
    for (let index = 0; index < group.bars.length; index++) {
        const bar = group.bars[index];
        const outerHeight = split ? Math.max(0,(sharedHeight - (group.bars.length - 1) * gap) / group.bars.length) : Math.min(barLimit,barHeights[index]);
        if (split) pixels(bar,'height',outerHeight); else clearHeight(bar);
        pixels(bar,'bottom',frame.bottom - bottom + barStack);
        barStack += outerHeight + gap;
    }
    let noticeStack = split ? 0 : barStack;
    const count = group.notices.length;
    const spacing = Math.min(gap,Math.max(0,height - noticeStack) / (count * 2));
    const limit = Math.max(0,(height - noticeStack - (count - 1) * spacing) / count);
    // DOM order remains reading order, with later results nearest the bottom.
    for (let index = count - 1; index >= 0; index--) {
        const root = group.notices[index].parentElement;
        const outerHeight = split ? Math.max(0,(sharedHeight - (count - 1) * spacing) / count) : Math.min(limit,noticeHeights[index]);
        if (split) pixels(root,'height',outerHeight); else clearHeight(root);
        pixels(root,'bottom',root.getBoundingClientRect().bottom - bottom + noticeStack);
        pixels(root,'limit',limit);
        noticeStack += outerHeight + spacing;
    }
}
function signature(entry) {
    const notice = entry.notice;
    return [notice.innerHTML,notice.className,notice.getAttribute('role'),entry.root.getAttribute('data-f-notice-key')].join('\u0000');
}
function dismissalDelay(root) {
    const value = Number(root.getAttribute('data-f-notice-dismiss-ms') ?? 3500);
    return Number.isFinite(value) && value >= 0 ? value : 3500;
}
function phase(entry, value) {
    entry.phase = value;
    if (entry.root.getAttribute('data-f-notice-phase') !== value) entry.root.setAttribute('data-f-notice-phase',value);
}
function cancelCountdown(entry) {
    if (entry.timer === null) return;
    entry.state.doc.defaultView.clearTimeout(entry.timer);
    entry.timer = null;
    entry.remaining = Math.max(0,entry.deadline - entry.state.doc.defaultView.performance.now());
}
function cancelExit(entry) {
    if (entry.exitTimer !== null) entry.state.doc.defaultView.clearTimeout(entry.exitTimer);
    entry.exitTimer = null;
}
function progress(entry, running = false) {
    const root = entry.root;
    const timed = entry.delay > 0 && entry.hostVisible;
    if (timed && !root.hasAttribute('data-f-notice-timed')) root.setAttribute('data-f-notice-timed','');
    else if (!timed) root.removeAttribute('data-f-notice-timed');
    const fraction = entry.delay > 0 ? Math.max(0,Math.min(1,entry.remaining / entry.delay)) : 0;
    for (const [name,value] of [['progress',String(fraction)],['duration',entry.remaining + 'ms'],['steps',String(Math.max(1,Math.ceil(entry.remaining / 1000)))]]) {
        const key = property(name);
        if (root.style.getPropertyValue(key) !== value) root.style.setProperty(key,value);
    }
    if (running) {
        entry.progressTurn = !entry.progressTurn;
        root.setAttribute('data-f-notice-progress',entry.progressTurn ? 'a' : 'b');
    } else root.removeAttribute('data-f-notice-progress');
}
function releaseNotice(entry) {
    cancelCountdown(entry);
    cancelExit(entry);
    for (const remove of entry.removers) remove();
    entry.root.removeAttribute('data-f-notice-phase');
    entry.root.removeAttribute('data-f-notice-enter');
    entry.root.removeAttribute('data-f-notice-progress');
    entry.root.removeAttribute('data-f-notice-timed');
    for (const name of ['progress','duration','steps']) entry.root.style.removeProperty(property(name));
}
function current(entry, generation) {
    if (!entry.root.isConnected || entry.state.entries.get(entry.root) !== entry) return false;
    // Reconcile actual DOM before accepting a delayed callback, including mutations
    // whose observer or animation-frame callback has not run yet.
    reconcileNotice(entry.state,entry.root);
    return entry.state.entries.get(entry.root) === entry && entry.generation === generation && entry.hostVisible && !entry.paused;
}
function finishExit(entry, generation, timer = entry.exitTimer) {
    if (entry.exitTimer !== timer || !current(entry,generation) || entry.exitTimer !== timer || entry.phase !== 'closing') return;
    entry.exitTimer = null;
    phase(entry,'dismissed');
    entry.state.schedule();
}
function beginExit(entry) {
    entry.remaining = 0;
    progress(entry);
    phase(entry,'closing');
    const win = entry.state.doc.defaultView;
    const duration = win.getComputedStyle(entry.notice).animationDuration ?? '0s';
    const milliseconds = Math.max(0,...duration.split(',').map(part => parseFloat(part) * (part.trim().endsWith('ms') ? 1 : 1000)));
    if (win.matchMedia?.('(prefers-reduced-motion: reduce)').matches || !milliseconds) {
        finishExit(entry,entry.generation);
        return;
    }
    const generation = entry.generation;
    // One frame of slack keeps the slot reserved through the final animation paint.
    const timer = win.setTimeout(() => finishExit(entry,generation,timer),milliseconds + 16);
    entry.exitTimer = timer;
}
function arm(entry) {
    if (entry.timer !== null || entry.paused || entry.phase !== 'shown' || entry.delay === 0) return;
    const win = entry.state.doc.defaultView, generation = entry.generation;
    entry.deadline = win.performance.now() + entry.remaining;
    progress(entry,true);
    const timer = win.setTimeout(() => {
        if (entry.timer !== timer) return;
        const accepted = current(entry,generation);
        if (entry.generation !== generation || entry.timer !== timer || !accepted || entry.phase !== 'shown') return;
        entry.timer = null;
        entry.remaining = 0;
        beginExit(entry);
    },entry.remaining);
    entry.timer = timer;
}
function createNotice(state, root, notice) {
    const entry = { state,root,notice,phase:null,generation:0,delay:null,signature:null,hostVisible:false,
        hovered:notice.matches(':hover'),focused:notice.contains(state.doc.activeElement),
        timer:null,exitTimer:null,remaining:0,removers:[] };
    const listen = (name,handler) => {
        notice.addEventListener(name,handler);
        entry.removers.push(() => notice.removeEventListener(name,handler));
    };
    const change = () => reconcileNotice(state,root);
    listen('pointerenter',() => { entry.hovered = true; change(); });
    listen('pointerleave',() => { entry.hovered = false; change(); });
    listen('focusin',() => { entry.focused = true; change(); });
    listen('focusout',event => { entry.focused = notice.contains(event.relatedTarget); change(); });
    state.entries.set(root,entry);
    return entry;
}
function reconcileNotice(state, root) {
    const notice = root.firstElementChild;
    let entry = state.entries.get(root);
    if (entry && entry.notice !== notice) { releaseNotice(entry); state.entries.delete(root); entry = null; }
    if (!notice) return null;
    entry ??= createNotice(state,root,notice);
    const nextSignature = signature(entry), delay = dismissalDelay(root);
    const hostVisible = visible(root) && !notice.hidden
        && state.doc.defaultView.getComputedStyle(notice).visibility === 'visible';
    const replay = entry.signature !== nextSignature || hostVisible && (!entry.hostVisible || entry.hiddenChanged);
    const restart = replay || entry.delay !== delay;
    entry.hiddenChanged = false;
    entry.signature = nextSignature;
    entry.delay = delay;
    entry.hostVisible = hostVisible;
    entry.paused = entry.hovered || entry.focused || state.doc.hidden;
    if (restart) {
        cancelCountdown(entry);
        cancelExit(entry);
        entry.generation++;
        entry.remaining = delay;
        phase(entry,hostVisible ? 'shown' : 'suspended');
        if (hostVisible && replay) {
            entry.enterTurn = !entry.enterTurn;
            root.setAttribute('data-f-notice-enter',entry.enterTurn ? 'a' : 'b');
        }
    }
    if (!hostVisible) {
        cancelCountdown(entry);
        cancelExit(entry);
        phase(entry,'suspended');
        progress(entry);
    } else if (entry.paused) {
        cancelCountdown(entry);
        cancelExit(entry);
        if (entry.phase === 'closing') phase(entry,'shown');
        progress(entry);
    } else {
        if (entry.delay === 0) progress(entry);
        arm(entry);
    }
    return entry;
}

function synchronize(state) {
    state.frame = 0;
    const doc = state.doc, win = doc.defaultView;
    const roots = [...doc.querySelectorAll(frames)];
    const groups = new Map(), observed = new Set();
    for (const root of roots) {
        const notice = root.firstElementChild;
        observed.add(root);
        if (notice) {
            observed.add(notice);
            for (const child of notice.children ?? []) observed.add(child);
        }
        const entry = reconcileNotice(state,root);
        if (!entry?.hostVisible || entry.phase === 'dismissed' || !visible(notice)) continue;
        const dialog = root.closest('dialog');
        if (!groups.has(dialog)) groups.set(dialog,{ notices:[], bars:[] });
        groups.get(dialog).notices.push(notice);
        if (dialog) {
            observed.add(dialog);
            const body = dialog.querySelector(':scope > .f-dialog-body');
            if (body) observed.add(body);
        }
    }
    for (const panel of doc.querySelectorAll(panels)) {
        const group = groups.get(panel.closest('dialog'));
        if (group && visible(panel)) {
            group.bars.push(panel); observed.add(panel);
            for (const child of panel.children ?? []) observed.add(child);
        }
    }
    const viewport = win.visualViewport;
    const left = viewport?.offsetLeft ?? 0, top = viewport?.offsetTop ?? 0;
    const view = { left,top,right:left + (viewport?.width ?? doc.documentElement.clientWidth),
        bottom:top + (viewport?.height ?? win.innerHeight) };
    state.next = new Set();
    for (const [dialog,group] of groups) arrange(state,dialog,group,view);
    for (const node of state.owned) if (!state.next.has(node)) release(node);
    state.owned = state.next;
    for (const [root,entry] of state.entries) if (!roots.includes(root)) {
        releaseNotice(entry);
        state.entries.delete(root);
    }
    observe(state,observed);
    if (!roots.length) {
        state.mutations.disconnect();
        state.resize.disconnect();
        for (const remove of state.removers) remove();
        documents.delete(doc);
    }
}
export function initializeFloatingNotices(scope) {
    const doc = scope?.ownerDocument ?? scope;
    if (!doc?.querySelector(frames)) return;
    if (documents.has(doc)) return;
    const win = doc.defaultView;
    const state = { doc,frame:0,observed:new Set(),owned:new Set(),entries:new Map(),removers:[] };
    function schedule() {
        if (!state.frame) state.frame = win.requestAnimationFrame(() => synchronize(state));
    }
    state.schedule = schedule;
    function listen(target,name,handler = schedule) {
        if (!target) return;
        target.addEventListener(name,handler,true);
        state.removers.push(() => target.removeEventListener(name,handler,true));
    }
    state.resize = new win.ResizeObserver(schedule);
    state.mutations = new win.MutationObserver(records => {
        for (const entry of state.entries.values())
            if (records.some(record => record.attributeName === 'hidden' && record.target.contains(entry.notice)))
                entry.hiddenChanged = true;
        schedule();
    });
    state.mutations.observe(doc.documentElement,{ subtree:true,childList:true,characterData:true,attributes:true,
        attributeFilter:['hidden','inert','open','class','style','data-active','role','data-f-notice-key','data-f-notice-dismiss-ms'] });
    listen(win,'resize');
    listen(doc,'scroll');
    listen(doc,'visibilitychange',() => {
        for (const root of state.entries.keys()) reconcileNotice(state,root);
        schedule();
    });
    // A bottom sheet's entrance transform temporarily changes the fixed containing block.
    listen(doc,'animationend');
    listen(doc,'transitionend');
    listen(win.visualViewport,'resize');
    listen(win.visualViewport,'scroll');
    documents.set(doc,state);
    synchronize(state);
}
