const bindings = new WeakMap();
const minimumWidth = 72;
const maximumWidth = 100000;
const automaticMaximumWidth = 260;
const defaultWidth = automaticMaximumWidth;

function minimumWidthFor(table) {
    const configured = Number.parseFloat(table?.dataset?.columnMinimumWidth);
    return Number.isFinite(configured) ? Math.min(maximumWidth, Math.max(minimumWidth, configured)) : minimumWidth;
}

function columns(table) {
    return [...table.querySelectorAll("colgroup > col")];
}

function widthOf(column) {
    return Number.parseFloat(column.style.width) || defaultWidth;
}

export function synchronizeWidth(root) {
    const table = root?.querySelector("table[data-resizable-table]");
    if (table) table.style.width = `${columns(table).filter(column => !column.hidden && !("columnFill" in column.dataset))
        .reduce((total, column) => total + widthOf(column), 0)}px`;
}

export function canChangePage(root) {
    const table = root?.querySelector("table[data-resizable-table]");
    if (!table) return true;
    for (const row of table.querySelectorAll("tbody > tr")) {
        if (row.hidden) continue;
        for (const input of row.querySelectorAll("input, select, textarea")) {
            if (!input.willValidate || input.checkValidity()) continue;
            input.reportValidity();
            return false;
        }
    }
    return true;
}

function applyWidth(table, column, width) {
    column.style.width = `${width}px`;
    for (const handle of table.querySelectorAll("[data-column-resize]")) {
        if (handle.dataset.columnResize !== column.dataset.columnKey) continue;
        handle.setAttribute("aria-valuenow", String(width));
        handle.setAttribute("aria-valuetext", `${width} pixels`);
    }
}

// Measure unconstrained content in one batch, without changing the visible cells or their wrapping.
export function measure(root) {
    const result = {};
    const table = root?.querySelector("table[data-resizable-table]");
    if (!table) {
        const absent = bindings.get(root);
        if (absent) { absent.table = null; absent.measured = {}; absent.dirty = false; absent.font = ""; }
        return result;
    }
    const binding = bindings.get(root);
    const tableColumns = columns(table);
    const automaticColumns = new Map();
    tableColumns.forEach((column, index) => {
        const key = column.dataset.columnKey;
        if (!key) return;
        const manualWidth = binding?.manualWidths.get(key);
        if (manualWidth !== undefined) {
            column.dataset.columnWidthMode = "manual";
            applyWidth(table, column, manualWidth);
        } else if (column.dataset.columnWidthMode !== "manual" && !column.hidden) {
            automaticColumns.set(index, { column, key, width: minimumWidthFor(table) });
        }
    });
    if (automaticColumns.size) {
        const document = root.ownerDocument;
        const view = document.defaultView;
        const container = document.createElement("div");
        container.className = table.className ?? "";
        container.setAttribute("aria-hidden", "true");
        container.inert = true;
        Object.assign(container.style, { position: "fixed", left: "-100000px", top: "0",
            visibility: "hidden", pointerEvents: "none", width: "max-content", maxWidth: "none" });
        const probes = [];
        for (const row of table.querySelectorAll("thead > tr, tbody > tr:not(.bulk-edit-row)")) {
            if (row.hidden) continue;
            for (const cell of row.cells) {
                const target = automaticColumns.get(cell.cellIndex);
                if (!target || cell.hidden || cell.colSpan > 1) continue;
                const content = cell.querySelector(".data-column-label, .data-cell-content") ?? cell;
                const style = view.getComputedStyle(cell);
                const probe = document.createElement("div");
                Object.assign(probe.style, { width: "max-content", maxWidth: "none", font: style.font,
                    letterSpacing: style.letterSpacing, whiteSpace: "nowrap" });
                const clone = content.cloneNode(true);
                for (const [property, value] of Object.entries({ display: "inline-block", width: "max-content",
                    "min-width": "0", "max-width": "none", height: "auto", "max-height": "none",
                    "white-space": "nowrap", overflow: "visible", "text-overflow": "clip" }))
                    clone.style.setProperty(property, value, "important");
                probe.appendChild(clone);
                container.appendChild(probe);
                // Reserve room for a sort indicator before it appears, preserving header width through the cycle.
                const padding = (Number.parseFloat(style.paddingLeft) || 0) + (Number.parseFloat(style.paddingRight) || 0)
                    + (cell.querySelector(".data-column-sort, .cell-select-marker") ? 32 : 0)
                    + (Number.parseFloat(content.dataset?.measurementPadding) || 0);
                probes.push({ target, probe, padding });
            }
        }
        root.appendChild(container);
        try {
            for (const { target, probe, padding } of probes)
                target.width = Math.min(automaticMaximumWidth,
                    Math.max(target.width, Math.ceil(probe.getBoundingClientRect().width + padding)));
        } finally { container.remove(); }
        for (const { column, key, width } of automaticColumns.values()) {
            result[key] = width;
            applyWidth(table, column, width);
        }
    }
    synchronizeWidth(root);
    if (binding) {
        binding.table = table;
        binding.measured = result;
        binding.dirty = false;
        binding.font = currentFont(root, table);
        // Discard mutations emitted by our off-screen probes and width writes.
        consumeChanges(root, binding, binding.observer?.takeRecords() ?? []);
    }
    return result;
}

function affectsMeasurement(root, record) {
    const table = root.querySelector("table[data-resizable-table]");
    const target = record.target instanceof Element ? record.target : record.target?.parentElement;
    if (record.type === "attributes" && target === root) return true;
    if (table && (target === table || table.contains?.(target))) {
        // The measurer and resize behavior own these widths; observing them would create a loop.
        if (record.type === "attributes" && record.attributeName === "style"
            && (target === table || target?.matches?.("col"))) return false;
        return true;
    }
    if (record.type !== "childList") return false;
    return [...record.addedNodes, ...record.removedNodes].some(node => node instanceof Element
        && (node.matches?.("table[data-resizable-table]") || node.querySelector?.("table[data-resizable-table]")));
}

function consumeChanges(root, binding, records) {
    if (records.some(record => affectsMeasurement(root, record))) binding.dirty = true;
}

function restoreMeasuredWidths(root, binding) {
    const table = root.querySelector("table[data-resizable-table]");
    if (!table) return {};
    for (const column of columns(table)) {
        const key = column.dataset.columnKey;
        const manual = binding.manualWidths.get(key);
        if (manual !== undefined) {
            column.dataset.columnWidthMode = "manual";
            applyWidth(table, column, manual);
        } else if (column.dataset.columnWidthMode !== "manual" && binding.measured[key] !== undefined) {
            applyWidth(table, column, binding.measured[key]);
        }
    }
    synchronizeWidth(root);
    return binding.measured;
}

function currentFont(root, table) {
    if (!table) return "";
    const style = root.ownerDocument?.defaultView?.getComputedStyle(table);
    return style ? `${style.font}\u0000${style.letterSpacing}` : "";
}

function observeMeasurements(root, binding) {
    if (typeof MutationObserver !== "function") return;
    const view = root.ownerDocument.defaultView;
    let frame = null;
    const request = () => {
        binding.dirty = true;
        if (frame !== null || typeof view?.requestAnimationFrame !== "function") return;
        frame = view.requestAnimationFrame(() => {
            frame = null;
            if (!binding.active && root.isConnected !== false) measure(root);
        });
    };
    binding.observer = new MutationObserver(records => consumeChanges(root, binding, records));
    binding.observer.observe(root, { childList: true, subtree: true, characterData: true,
        attributes: true, attributeFilter: ["hidden", "class", "style", "src"] });
    const input = event => {
        if (event.target?.closest?.("table[data-resizable-table]")) request();
    };
    root.addEventListener("input", input);
    root.addEventListener("change", input);
    view?.addEventListener?.("resize", request);
    root.ownerDocument.fonts?.addEventListener?.("loadingdone", request);
    const Resize = view?.ResizeObserver;
    let width = root.clientWidth;
    const resize = typeof Resize === "function" ? new Resize(() => {
        if (root.clientWidth === width) return;
        width = root.clientWidth;
        request();
    }) : null;
    resize?.observe(root);
    binding.disposeMeasurements = () => {
        binding.observer.disconnect();
        resize?.disconnect();
        root.removeEventListener("input", input);
        root.removeEventListener("change", input);
        view?.removeEventListener?.("resize", request);
        root.ownerDocument.fonts?.removeEventListener?.("loadingdone", request);
        if (frame !== null) view.cancelAnimationFrame(frame);
    };
}

export function connect(root, onWidthChanged = null) {
    if (!root) return {};
    const previous = bindings.get(root);
    if (previous) {
        previous.onWidthChanged = onWidthChanged;
        if (previous.active) return {};
        // Native DOM observers preserve template edits, column visibility, page changes and new table nodes.
        // A runtime without observers retains the original conservative measurement behavior.
        if (!previous.observer) return measure(root);
        consumeChanges(root, previous, previous.observer.takeRecords());
        const table = root.querySelector("table[data-resizable-table]");
        if (previous.table !== table || previous.font !== currentFont(root, table)) previous.dirty = true;
        return previous.dirty ? measure(root) : restoreMeasuredWidths(root, previous);
    }
    const binding = { onWidthChanged, active: null, manualWidths: new Map(), measured: {}, dirty: true, table: null, font: "" };

    function findHandle(target) {
        const handle = target instanceof Element ? target.closest("[data-column-resize]") : null;
        return handle && root.contains(handle) ? handle : null;
    }

    function contextFor(handle) {
        const table = handle.closest("table[data-resizable-table]");
        if (!table) return null;
        const key = handle.dataset.columnResize;
        const column = columns(table).find(candidate => candidate.dataset.columnKey === key);
        return column ? { handle, table, column, key, width: widthOf(column) } : null;
    }

    function apply(context, width) {
        context.width = Math.min(maximumWidth, Math.max(minimumWidthFor(context.table), Math.round(width)));
        applyWidth(context.table, context.column, context.width);
        synchronizeWidth(root);
    }

    function notify(context, mode) {
        const callback = binding.onWidthChanged;
        return !callback ? undefined : typeof callback === "function"
            ? callback(context.key, context.width, mode)
            : mode === "auto" ? callback.invokeMethodAsync("ResetColumnWidth", context.key)
                : callback.invokeMethodAsync("SetColumnWidth", context.key, context.width);
    }

    function save(context, originalWidth) {
        if (context.width === originalWidth) return;
        const originalManual = binding.manualWidths.get(context.key);
        const originalMode = context.column.dataset.columnWidthMode;
        binding.manualWidths.set(context.key, context.width);
        context.column.dataset.columnWidthMode = "manual";
        const result = notify(context, "manual");
        // A disconnected view cannot retain a resize; reconnect renders the authoritative widths.
        Promise.resolve(result).catch(() => {
            if (context.column.isConnected && widthOf(context.column) === context.width) {
                if (originalManual === undefined) binding.manualWidths.delete(context.key);
                else binding.manualWidths.set(context.key, originalManual);
                context.column.dataset.columnWidthMode = originalMode ?? "auto";
                apply(context, originalWidth);
            }
        });
    }

    function reset(context) {
        const originalWidth = context.width;
        const originalManual = binding.manualWidths.get(context.key);
        const originalMode = context.column.dataset.columnWidthMode;
        binding.manualWidths.delete(context.key);
        context.column.dataset.columnWidthMode = "auto";
        context.width = measure(root)[context.key] ?? minimumWidthFor(context.table);
        Promise.resolve(notify(context, "auto")).catch(() => {
            if (context.column.isConnected && widthOf(context.column) === context.width) {
                if (originalManual !== undefined) binding.manualWidths.set(context.key, originalManual);
                context.column.dataset.columnWidthMode = originalMode ?? "auto";
                apply(context, originalWidth);
            }
        });
    }

    function finish(cancelled = false) {
        const active = binding.active;
        if (!active) return;
        binding.active = null;
        root.classList.remove("is-resizing-columns");
        active.handle.classList.remove("is-resizing");
        if (cancelled) apply(active, active.originalWidth);
        else save(active, active.originalWidth);
        if (active.handle.hasPointerCapture(active.pointerId))
            active.handle.releasePointerCapture(active.pointerId);
    }

    const pointerdown = event => {
        if (event.button !== 0 || binding.active) return;
        const handle = findHandle(event.target);
        const context = handle && contextFor(handle);
        if (!context) return;
        event.preventDefault();
        event.stopPropagation();
        handle.focus({ preventScroll: true });
        binding.active = { ...context, originalWidth: context.width, startX: event.clientX,
            scroll: tableScroll(context.table), pointerId: event.pointerId };
        root.classList.add("is-resizing-columns");
        handle.classList.add("is-resizing");
        handle.setPointerCapture(event.pointerId);
    };
    const pointermove = event => {
        const active = binding.active;
        if (!active || active.pointerId !== event.pointerId) return;
        if (!active.column.isConnected) { finish(true); return; }
        event.preventDefault();
        apply(active, active.originalWidth + event.clientX - active.startX
            + tableScroll(active.table) - active.scroll);
    };
    const pointerup = event => {
        if (binding.active?.pointerId === event.pointerId) finish();
    };
    const pointercancel = event => {
        if (binding.active?.pointerId === event.pointerId) finish(true);
    };
    const keydown = event => {
        const pageInput = event.target instanceof Element ? event.target.closest("[data-page-input]") : null;
        if (event.key === "Enter" && pageInput && root.contains(pageInput)) {
            event.preventDefault();
            return;
        }
        if (binding.active && event.key === "Escape") {
            event.preventDefault();
            event.stopPropagation();
            finish(true);
            return;
        }
        if (binding.active || event.altKey || event.ctrlKey || event.metaKey) return;
        const handle = findHandle(event.target);
        const context = handle && contextFor(handle);
        if (!context || !["ArrowLeft", "ArrowRight", "Home"].includes(event.key)) return;
        event.preventDefault();
        event.stopPropagation();
        if (event.key === "Home") { reset(context); return; }
        const originalWidth = context.width;
        const step = event.shiftKey ? 50 : 10;
        apply(context, originalWidth + (event.key === "ArrowLeft" ? -step : step));
        save(context, originalWidth);
    };
    const dblclick = event => {
        if (!findHandle(event.target)) return;
        event.preventDefault();
        event.stopPropagation();
    };
    const events = { pointerdown, pointermove, pointerup, pointercancel,
        lostpointercapture: pointercancel, keydown, dblclick };
    for (const [name, handler] of Object.entries(events)) root.addEventListener(name, handler);
    binding.dispose = () => {
        finish(true);
        binding.disposeMeasurements?.();
        for (const [name, handler] of Object.entries(events)) root.removeEventListener(name, handler);
    };
    bindings.set(root, binding);
    observeMeasurements(root, binding);
    return measure(root);
}

function tableScroll(table) {
    return table.parentElement.scrollLeft;
}

export function dispose(root) {
    bindings.get(root)?.dispose();
    bindings.delete(root);
}
