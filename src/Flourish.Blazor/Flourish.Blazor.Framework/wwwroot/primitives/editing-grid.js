const bindings = new WeakMap();
const cellSelector = "td[data-row][data-column]";
const editorSelector = "input:not([type=checkbox]), textarea, select, button:not(:disabled)";

function findCell(target) {
    return target instanceof Element ? target.closest(cellSelector) : null;
}

function coordinate(cell) {
    return { row: Number(cell.dataset.row), column: Number(cell.dataset.column) };
}

function sameCell(first, second) {
    return first?.row === second?.row && first?.column === second?.column;
}

export function cellRange(first, last = first) {
    return {
        firstRow: Math.min(first.row, last.row), lastRow: Math.max(first.row, last.row),
        firstColumn: Math.min(first.column, last.column), lastColumn: Math.max(first.column, last.column)
    };
}

function contains(range, cell) {
    return cell.row >= range.firstRow && cell.row <= range.lastRow
        && cell.column >= range.firstColumn && cell.column <= range.lastColumn;
}

export function selectedCells(ranges) {
    const cells = new Map();
    for (const range of ranges) {
        for (let row = range.firstRow; row <= range.lastRow; row++) {
            for (let column = range.firstColumn; column <= range.lastColumn; column++) {
                cells.set(`${row}:${column}`, { row, column });
            }
        }
    }
    return [...cells.values()].sort((first, second) => first.row - second.row || first.column - second.column);
}

export function nextCell(key, row, column, shift, rows, columns) {
    if (key === "Enter") row += shift ? -1 : 1;
    else if (key === "ArrowUp") row--;
    else if (key === "ArrowDown") row++;
    else if (key === "ArrowLeft") column--;
    else if (key === "ArrowRight") column++;
    else if (key === "Home") column = 0;
    else if (key === "End") column = columns - 1;
    else if (key === "Tab") {
        column += shift ? -1 : 1;
        if (column >= columns) { column = 0; row++; }
        else if (column < 0) { column = columns - 1; row--; }
    } else return null;
    return row < 0 || row >= rows || column < 0 || column >= columns ? null : { row, column };
}

export function nextDataCell(key, row, column, rows, columns, readValue) {
    const current = { row, column };
    let destination = nextCell(key, row, column, false, rows, columns);
    if (!destination) return current;
    const populated = cell => String(readValue(cell.row, cell.column) ?? "").length > 0;
    if (populated(current) && populated(destination)) {
        // From inside a data region, stop at its last consecutive populated cell.
        for (let following = nextCell(key, destination.row, destination.column, false, rows, columns);
            following && populated(following);
            following = nextCell(key, destination.row, destination.column, false, rows, columns)) destination = following;
        return destination;
    }
    // Across a gap, stop at the first populated cell, or the loaded table's outer edge.
    while (!populated(destination)) {
        const following = nextCell(key, destination.row, destination.column, false, rows, columns);
        if (!following) break;
        destination = following;
    }
    return destination;
}

export function serializeCells(matrix) {
    return matrix.map(row => row.map(value => {
        const text = String(value ?? "");
        return /[\t\r\n"]/.test(text) ? `"${text.replaceAll('"', '""')}"` : text;
    }).join("\t")).join("\r\n");
}

async function reportStreamError(reference) {
    try { await reference.invokeMethodAsync("ReportPasteError", "Grid_EditFailed"); }
    catch { /* The disconnected page owns its reconnect message. */ }
}

export async function sendPaste(reference, row, column, text) {
    const blob = new Blob([text], { type: "text/plain;charset=utf-8" });
    if (blob.size > 2_000_000) {
        await reference.invokeMethodAsync("ReportPasteError", "Grid_PasteTooLarge");
        return;
    }
    const stream = DotNet.createJSStreamReference(blob);
    try {
        // Clipboard bytes use the existing chunked stream, rather than one large Hub message.
        await reference.invokeMethodAsync("PasteCells", row, column, stream);
    } catch {
        await reportStreamError(reference);
    } finally {
        DotNet.disposeJSObjectReference(stream);
    }
}

export async function sendEdits(reference, edits, mergeLastCellEdit = false) {
    const payload = mergeLastCellEdit ? { edits, mergeLastCellEdit: true } : edits;
    const blob = new Blob([JSON.stringify(payload)], { type: "application/json;charset=utf-8" });
    if (blob.size > 2_000_000) {
        await reference.invokeMethodAsync("ReportPasteError", "Grid_EditTooLarge");
        return false;
    }
    const stream = DotNet.createJSStreamReference(blob);
    try {
        return await reference.invokeMethodAsync("ApplyCellEdits", stream);
    } catch {
        await reportStreamError(reference);
        return false;
    } finally {
        DotNet.disposeJSObjectReference(stream);
    }
}

export function connect(root, reference) {
    if (!root || bindings.has(root)) return;
    const state = { active: null, anchor: null, ranges: [], editing: false, session: null, sessionBarrier: Promise.resolve(), dragging: false, dragEnd: null, pending: false, disposed: false, loadCount: -1, loading: false, loadFailed: false, openingLink: false };
    const getCell = cell => cell && root.querySelector(`td[data-row="${cell.row}"][data-column="${cell.column}"]`);
    const allCells = () => [...root.querySelectorAll(`tbody ${cellSelector}`)];
    const dimensions = (cells = allCells()) => {
        return {
            rows: cells.reduce((maximum, cell) => Math.max(maximum, Number(cell.dataset.row) + 1), 0),
            columns: cells.reduce((maximum, cell) => Math.max(maximum, Number(cell.dataset.column) + 1), 0)
        };
    };
    const owns = cell => !!cell && root.contains(cell);
    const editor = cell => cell?.querySelector(editorSelector);
    const writable = cell => cell && cell.dataset.readonly !== "true" && !editor(cell)?.disabled && !!editor(cell);
    const canEdit = () => root.dataset.editDisabled !== "true" && !state.pending;
    const valueOf = (cell, display = false) => {
        // Composite editors expose their complete value rather than a summary such as "3 etiquetas".
        if (cell?.dataset.cellValue !== undefined) return cell.dataset.cellValue;
        const control = editor(cell);
        if (display && control?.matches("select")) return control.selectedOptions?.[0]?.textContent ?? control.value;
        return control && "value" in control ? control.value
            : cell?.querySelector(".spreadsheet-cell-display")?.textContent ?? cell?.textContent?.trim() ?? "";
    };
    const scrollToCell = cell => {
        if (!cell) return;
        const bounds = root.getBoundingClientRect();
        const target = cell.getBoundingClientRect();
        const identityCell = root.querySelector("td.identity-column");
        const stickyWidth = identityCell && getComputedStyle(identityCell).position === "sticky"
            ? identityCell.getBoundingClientRect().width : 0;
        const stickyHeight = root.querySelector("thead")?.getBoundingClientRect().height ?? 0;
        if (target.right > bounds.right) root.scrollLeft += target.right - bounds.right + 16;
        else if (Number(cell.dataset.column) > 0 && target.left < bounds.left + stickyWidth) root.scrollLeft -= bounds.left + stickyWidth - target.left + 16;
        if (target.bottom > bounds.bottom) root.scrollTop += target.bottom - bounds.bottom + 16;
        else if (target.top < bounds.top + stickyHeight) root.scrollTop -= bounds.top + stickyHeight - target.top + 16;
    };
    const toggleClass = (cell, name, enabled) => {
        // Observe Blazor's class updates without writing unchanged classes and looping the observer.
        if (cell.classList.contains(name) !== enabled) cell.classList.toggle(name, enabled);
    };
    const paint = () => {
        const cells = allCells();
        if (cells.length === 0) { state.active = null; state.ranges = []; return; }
        if (!getCell(state.active)) {
            state.active = coordinate(cells[0]); state.anchor = state.active;
            state.ranges = [cellRange(state.active)]; state.editing = false;
        }
        if (root.dataset.editDisabled === "true" || !writable(getCell(state.active))) {
            state.editing = false;
            endSession();
        }
        const size = dimensions(cells);
        const table = root.querySelector("table");
        table?.setAttribute("role", "grid");
        table?.setAttribute("aria-multiselectable", "true");
        table?.setAttribute("aria-rowcount", String(size.rows + 1));
        table?.setAttribute("aria-colcount", String(size.columns));
        for (const cell of cells) {
            const position = coordinate(cell);
            const active = sameCell(position, state.active);
            const selected = state.ranges.some(range => contains(range, position));
            toggleClass(cell, "is-active-cell", active);
            toggleClass(cell, "is-selected-cell", selected);
            toggleClass(cell, "is-editing-cell", active && state.editing);
            cell.setAttribute("role", "gridcell");
            cell.setAttribute("aria-selected", String(selected));
            cell.setAttribute("aria-rowindex", String(position.row + 2));
            cell.setAttribute("aria-colindex", String(position.column + 1));
            cell.setAttribute("aria-readonly", String(!writable(cell) || root.dataset.editDisabled === "true"));
            cell.tabIndex = active && !state.editing ? 0 : -1;
            for (const control of cell.querySelectorAll("input, textarea, select, button, a[href]")) {
                control.tabIndex = active && state.editing ? 0 : -1;
            }
        }
        root.tabIndex = -1;
    };
    const focusActive = () => {
        const cell = getCell(state.active);
        cell?.focus({ preventScroll: true });
        scrollToCell(cell);
    };
    const notifySession = (method, ...args) => {
        if (state.disposed) return state.sessionBarrier;
        // Invoke immediately so Begin precedes native input, and End follows its blur/change events.
        let notification;
        try { notification = Promise.resolve(reference.invokeMethodAsync(method, ...args)).catch(() => reportStreamError(reference)); }
        catch { notification = reportStreamError(reference); }
        state.sessionBarrier = Promise.all([state.sessionBarrier, notification]).then(() => undefined);
        return state.sessionBarrier;
    };
    const endSession = (cancel = false, blur = true) => {
        const session = state.session;
        if (!session) return state.sessionBarrier;
        state.session = null;
        const control = editor(getCell(session.position));
        if (cancel && control && "value" in control) {
            control.value = session.originalValue;
            control.dispatchEvent(new Event("input", { bubbles: true }));
            control.dispatchEvent(new Event("change", { bubbles: true }));
        }
        if (blur) control?.blur?.();
        return notifySession(cancel ? "CancelCellEdit" : "EndCellEdit");
    };
    const select = (position, extend = false, add = false) => {
        endSession();
        state.editing = false;
        if (extend && state.anchor) {
            const range = cellRange(state.anchor, position);
            if (add) state.ranges = [...state.ranges.slice(0, -1), range];
            else state.ranges = [range];
        } else {
            state.anchor = position;
            state.ranges = add ? [...state.ranges, cellRange(position)] : [cellRange(position)];
        }
        state.active = position;
        paint();
    };
    const stopEditing = (cancel = false) => {
        endSession(cancel);
        state.editing = false; paint(); focusActive();
        return state.sessionBarrier;
    };
    const beginEditing = (replacement = null) => {
        const cell = getCell(state.active);
        if (!canEdit() || !writable(cell)) return false;
        const control = editor(cell);
        if (!state.session) {
            state.session = { position: { ...state.active }, originalValue: control.value ?? "" };
            notifySession("BeginCellEdit", state.active.row, state.active.column);
        }
        state.editing = true;
        paint();
        control.focus({ preventScroll: true });
        if (control.matches("input:not([type=checkbox]), textarea")) {
            control.select?.();
            if (replacement !== null) {
                control.value = replacement;
                control.dispatchEvent(new Event("input", { bubbles: true }));
                control.setSelectionRange?.(control.value.length, control.value.length);
            }
        }
        return true;
    };
    const invoke = async (method, ...args) => {
        if (!canEdit() || state.disposed) return;
        state.pending = true;
        root.setAttribute("aria-busy", "true");
        try {
            await state.sessionBarrier;
            if (state.disposed || root.dataset.editDisabled === "true") return false;
            return method === "ApplyCellEdits" ? await sendEdits(reference, args[0], args[1]) : await reference.invokeMethodAsync(method, ...args);
        }
        catch { if (!state.disposed) await reportStreamError(reference); return false; }
        finally {
            state.pending = false;
            if (!state.disposed) { root.removeAttribute("aria-busy"); paint(); }
        }
    };
    const apply = async (changes, mergeLastCellEdit = false) => {
        if (changes.length === 0 || !canEdit()) return;
        stopEditing();
        const applied = await invoke("ApplyCellEdits", changes, mergeLastCellEdit);
        if (applied === true && !state.disposed) stopEditing();
    };
    const selectionEdits = value => selectedCells(state.ranges)
        .filter(position => writable(getCell(position)))
        .map(position => ({ ...position, value }));
    const fill = direction => {
        const edits = new Map();
        for (const range of state.ranges) {
            for (let row = range.firstRow; row <= range.lastRow; row++) {
                for (let column = range.firstColumn; column <= range.lastColumn; column++) {
                    if ((direction === "down" && row === range.firstRow) || (direction === "right" && column === range.firstColumn)) continue;
                    const position = { row, column };
                    if (!writable(getCell(position))) continue;
                    const source = direction === "down" ? { row: range.firstRow, column } : { row, column: range.firstColumn };
                    edits.set(`${row}:${column}`, { ...position, value: valueOf(getCell(source)) });
                }
            }
        }
        void apply([...edits.values()]);
    };
    const navigate = event => {
        if (!["Tab", "Enter", "ArrowUp", "ArrowDown", "ArrowLeft", "ArrowRight", "Home", "End", "PageUp", "PageDown"].includes(event.key)) return;
        const size = dimensions();
        const selected = event.key === "Tab" || event.key === "Enter" ? selectedCells(state.ranges) : [];
        let destination;
        const withinSelection = (event.key === "Tab" || event.key === "Enter") && selected.length > 1;
        if (withinSelection) {
            const ordered = event.key === "Enter" ? selected.sort((first, second) => first.column - second.column || first.row - second.row) : selected;
            const current = ordered.findIndex(cell => sameCell(cell, state.active));
            destination = ordered[(current + (event.shiftKey ? -1 : 1) + ordered.length) % ordered.length];
        } else {
            destination = nextCell(event.key, state.active.row, state.active.column, event.shiftKey, size.rows, size.columns);
            if ((event.ctrlKey || event.metaKey) && event.key.startsWith("Arrow")) {
                destination = nextDataCell(event.key, state.active.row, state.active.column, size.rows, size.columns,
                    (row, column) => valueOf(getCell({ row, column })));
            }
            if ((event.ctrlKey || event.metaKey) && event.key === "Home") destination = { row: 0, column: 0 };
            if ((event.ctrlKey || event.metaKey) && event.key === "End") destination = { row: size.rows - 1, column: size.columns - 1 };
            if (event.key === "PageUp" || event.key === "PageDown") {
                const rowHeight = getCell(state.active)?.getBoundingClientRect().height || 1;
                const headerHeight = root.querySelector("thead")?.getBoundingClientRect().height ?? 0;
                const visibleRows = Math.max(1, Math.floor((root.clientHeight - headerHeight) / rowHeight));
                destination = {
                    row: Math.max(0, Math.min(size.rows - 1, state.active.row + (event.key === "PageDown" ? visibleRows : -visibleRows))),
                    column: state.active.column
                };
            }
        }
        if (!destination || !getCell(destination)) {
            if (event.key === "Enter" && state.editing) stopEditing();
            if (event.key !== "Tab") event.preventDefault();
            return;
        }
        event.preventDefault();
        endSession();
        state.editing = false;
        if (withinSelection) { state.active = destination; paint(); }
        else select(destination, event.shiftKey && event.key !== "Tab" && event.key !== "Enter");
        focusActive();
    };
    const openLink = cell => {
        state.openingLink = true;
        try { cell.querySelector("a[href]")?.click(); }
        finally { state.openingLink = false; }
    };
    const keydown = event => {
        const cell = findCell(event.target);
        if (!owns(cell) || event.isComposing || event.altKey) return;
        const command = event.ctrlKey || event.metaKey;
        const key = event.key.toLowerCase();
        if (command && key === "s") { event.preventDefault(); stopEditing(); void invoke("SaveFromKeyboard"); return; }
        if (command && !state.editing && (key === "z" || key === "y")) {
            event.preventDefault();
            stopEditing();
            void invoke(key === "y" || event.shiftKey ? "RedoEdits" : "UndoEdits");
            return;
        }
        if (command && event.key === "Enter") {
            event.preventDefault();
            void apply(selectionEdits(valueOf(getCell(state.active))), state.editing);
            return;
        }
        if (state.editing) {
            if (event.key === "Escape") {
                if (event.target.matches("select")) {
                    // Let a native dropdown cancel its pending option before moving focus away.
                    const position = { ...state.active };
                    endSession(true, false);
                    state.editing = false; paint();
                    requestAnimationFrame(() => { if (!state.disposed && !state.editing && sameCell(position, state.active)) focusActive(); });
                } else { event.preventDefault(); stopEditing(true); }
                return;
            }
            if (event.key === "Tab") { navigate(event); return; }
            // Native dropdowns, caret movement, checkbox shortcuts, and IME retain their own keys.
            if (event.target.matches("select, button, input[type=checkbox]")) return;
            if (event.key === "Enter" && !(event.target.matches("textarea") && event.shiftKey)) navigate(event);
            return;
        }
        if (command && key === "a") {
            event.preventDefault();
            const size = dimensions();
            state.ranges = [cellRange({ row: 0, column: 0 }, { row: size.rows - 1, column: size.columns - 1 })];
            paint();
            return;
        }
        if (command && (key === "d" || key === "r")) { event.preventDefault(); fill(key === "d" ? "down" : "right"); return; }
        if (event.key === "Delete" || event.key === "Backspace") { event.preventDefault(); void apply(selectionEdits("")); return; }
        if (event.key === "Escape") { event.preventDefault(); select(state.active); focusActive(); return; }
        if (event.key === "F2") { event.preventDefault(); beginEditing(); return; }
        if (event.key === "Enter" && selectedCells(state.ranges).length === 1 && cell.dataset.readonly === "true" && cell.querySelector("a[href]")) { event.preventDefault(); openLink(cell); return; }
        if (!command && (event.key.length === 1 || event.key === "Process" || event.key === "Unidentified")) {
            if (beginEditing(event.key.length === 1 ? event.key : null) && event.key.length === 1) event.preventDefault();
            return;
        }
        navigate(event);
    };
    const pointerdown = event => {
        const cell = findCell(event.target);
        if (!owns(cell) || event.button !== 0 || event.pointerType === "touch") return;
        const position = coordinate(cell);
        if (state.editing && sameCell(position, state.active)) return;
        event.preventDefault();
        select(position, event.shiftKey, event.ctrlKey || event.metaKey);
        state.dragging = true; state.dragEnd = position;
        focusActive();
    };
    const pointermove = event => {
        if (!state.dragging) return;
        const cell = findCell(event.target);
        if (!owns(cell)) return;
        const position = coordinate(cell);
        if (sameCell(position, state.dragEnd)) return;
        state.dragEnd = position;
        state.ranges = [...state.ranges.slice(0, -1), cellRange(state.anchor, position)];
        paint();
    };
    const pointerup = () => { state.dragging = false; };
    const click = event => {
        const cell = findCell(event.target);
        if (!owns(cell) || state.openingLink) return;
        if (state.editing && sameCell(coordinate(cell), state.active)) return;
        if (event.pointerType === "touch") { select(coordinate(cell)); focusActive(); }
        // The first click selects the cell; native controls open only in editing mode.
        event.preventDefault(); event.stopPropagation();
    };
    const dblclick = event => {
        const cell = findCell(event.target);
        if (!owns(cell)) return;
        event.preventDefault();
        if (!sameCell(coordinate(cell), state.active)) select(coordinate(cell));
        if (cell.dataset.readonly === "true") { openLink(cell); return; }
        beginEditing();
    };
    const copy = event => {
        if (state.editing || !owns(findCell(event.target)) || !event.clipboardData) return;
        const range = [...state.ranges].reverse().find(range => contains(range, state.active));
        if (!range) return;
        const matrix = [];
        for (let row = range.firstRow; row <= range.lastRow; row++) {
            const values = [];
            for (let column = range.firstColumn; column <= range.lastColumn; column++) values.push(valueOf(getCell({ row, column }), true));
            matrix.push(values);
        }
        event.preventDefault();
        event.clipboardData.setData("text/plain", serializeCells(matrix));
    };
    const paste = async event => {
        const cell = findCell(event.target);
        const text = event.clipboardData?.getData("text/plain");
        if (!owns(cell) || !canEdit() || !writable(cell) || text === undefined || text === "") return;
        const matrix = text.includes("\t") || text.includes("\n") || text.includes("\r");
        if (state.editing && (!matrix || (event.target.matches("textarea") && !text.includes("\t")))) return;
        event.preventDefault(); event.stopPropagation();
        // A scalar paste fills an existing selection. Rectangles keep the stream protocol.
        if (!matrix && selectedCells(state.ranges).length > 1) { await apply(selectionEdits(text)); return; }
        const position = { ...state.active };
        stopEditing();
        state.pending = true;
        root.setAttribute("aria-busy", "true");
        try {
            await state.sessionBarrier;
            if (!state.disposed && root.dataset.editDisabled !== "true") await sendPaste(reference, position.row, position.column, text);
        }
        catch { /* Error reporting can race page disposal. */ }
        finally {
            state.pending = false;
            if (!state.disposed) { root.removeAttribute("aria-busy"); stopEditing(); }
        }
    };
    const focusin = event => {
        const cell = findCell(event.target);
        if (owns(cell) && !sameCell(coordinate(cell), state.active)) select(coordinate(cell));
    };
    const focusout = event => {
        if (!state.session || !owns(findCell(event.target))) return;
        const destination = findCell(event.relatedTarget);
        if (owns(destination) && sameCell(coordinate(destination), state.session.position)) return;
        endSession(); state.editing = false; paint();
    };
    const scroll = async event => {
        if (state.loading || state.disposed || root.dataset.hasMore === "false") return;
        if (state.loadFailed && event?.type !== "scroll") return;
        if (root.scrollHeight - root.scrollTop - root.clientHeight > Math.max(200, root.clientHeight * 0.75)) return;
        const count = dimensions().rows;
        if (state.loadCount === count) return;
        state.loadCount = count; state.loading = true; state.loadFailed = false;
        try {
            if (await reference.invokeMethodAsync("LoadMoreRows") === false) { state.loadCount = -1; state.loadFailed = true; }
        } catch { state.loadCount = -1; state.loadFailed = true; /* The page reports loading failures and keeps loaded rows. */ }
        finally {
            state.loading = false;
            // A successful page may still leave a tall viewport empty. Failed loads need another user scroll.
            if (!state.disposed && !state.loadFailed) requestAnimationFrame(() => { if (!state.disposed) void scroll(); });
        }
    };
    const listeners = { keydown, pointerdown, pointermove, click, dblclick, copy, paste, focusin, focusout, scroll };
    for (const [name, handler] of Object.entries(listeners)) root.addEventListener(name, handler, name !== "scroll");
    root.ownerDocument.addEventListener("pointerup", pointerup);
    root.ownerDocument.addEventListener("pointercancel", pointerup);
    const observer = new MutationObserver(() => { paint(); void scroll(); });
    observer.observe(root, { childList: true, subtree: true, attributes: true, attributeFilter: ["class", "data-edit-disabled", "data-has-more", "disabled", "data-cell-value"] });
    const resizeObserver = new ResizeObserver(() => { void scroll(); });
    resizeObserver.observe(root);
    bindings.set(root, { listeners, pointerup, observer, resizeObserver, state });
    paint();
    void scroll();
}

export function disconnect(root) {
    const handlers = root && bindings.get(root);
    if (!handlers) return;
    handlers.state.disposed = true;
    handlers.observer.disconnect();
    handlers.resizeObserver.disconnect();
    for (const [name, handler] of Object.entries(handlers.listeners)) root.removeEventListener(name, handler, name !== "scroll");
    root.ownerDocument.removeEventListener("pointerup", handlers.pointerup);
    root.ownerDocument.removeEventListener("pointercancel", handlers.pointerup);
    bindings.delete(root);
}
