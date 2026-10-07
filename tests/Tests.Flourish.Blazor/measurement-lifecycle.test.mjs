import assert from "node:assert/strict";
import fs from "node:fs";
import vm from "node:vm";
import test from "node:test";

const source = fs.readFileSync(new URL("../../src/Flourish.Blazor/Flourish.Blazor.Framework/wwwroot/primitives/editing-grid-columns.js", import.meta.url), "utf8");
class Element {}
class Observer {
    static instances = [];
    constructor(callback) { this.callback = callback; this.records = []; this.disconnected = false; Observer.instances.push(this); }
    observe(root) { this.root = root; }
    takeRecords() { return this.records.splice(0); }
    disconnect() { this.disconnected = true; }
    record(record) { this.records.push(record); }
}
const runtime = vm.createContext({ Element, MutationObserver: Observer });
vm.runInContext(source.replace(/^export /gm, ""), runtime);
const api = vm.runInContext("({ connect, dispose, synchronizeWidth })", runtime);

class Resize {
    constructor(callback) { this.callback = callback; this.disconnected = false; }
    observe(root) { this.root = root; }
    disconnect() { this.disconnected = true; }
}

function fixture() {
    const listeners = new Map(), viewListeners = new Map(), fontListeners = new Map(), frames = new Map();
    let probes = 0, font = "400 17px Segoe UI", frameKey = 0;
    const columns = [new Element(), new Element()];
    columns.forEach((column, index) => Object.assign(column, { dataset: { columnKey: index === 0 ? "title" : "detail" }, style: { width: "260px" }, isConnected: true, matches: selector => selector === "col" }));
    const makeCells = widths => widths.map((width, cellIndex) => Object.assign(new Element(), {
        cellIndex, colSpan: 1, matches: () => false,
        computedStyle: { font, letterSpacing: "normal", paddingLeft: "20px", paddingRight: "20px" },
        querySelector: selector => selector === ".cell-select-marker" ? null : ({ cloneNode: () => ({ naturalWidth: width, style: { setProperty() {} } }) })
    }));
    const rows = [{ cells: makeCells([40, 44]) }, { cells: makeCells([60, 44]) }];
    const table = Object.assign(new Element(), {
        style: {}, parentElement: { scrollLeft: 0 }, matches: () => false,
        contains: target => rows.some(row => row.cells.includes(target)) || columns.includes(target),
        querySelectorAll: selector => selector === "colgroup > col" ? columns : selector === "[data-column-resize]" ? [handle] : rows
    });
    const handle = Object.assign(new Element(), {
        dataset: { columnResize: "title" }, pointer: null, classList: { add() {}, remove() {} },
        closest: selector => selector === "[data-column-resize]" ? handle : selector === "table[data-resizable-table]" ? table : null,
        setAttribute() {}, focus() {}, setPointerCapture(id) { this.pointer = id; }, hasPointerCapture(id) { return this.pointer === id; }, releasePointerCapture() { this.pointer = null; }
    });
    const view = {
        ResizeObserver: Resize,
        getComputedStyle: element => element === table ? { font, letterSpacing: "normal" } : element.computedStyle,
        addEventListener: (name, fn) => viewListeners.set(name, fn), removeEventListener: name => viewListeners.delete(name),
        requestAnimationFrame: fn => { const id = ++frameKey; frames.set(id, fn); return id; }, cancelAnimationFrame: id => frames.delete(id)
    };
    const root = Object.assign(new Element(), {
        isConnected: true, clientWidth: 800, querySelector: () => table, contains: node => node === handle,
        classList: { add() {}, remove() {} },
        addEventListener: (name, fn) => listeners.set(name, fn), removeEventListener: name => listeners.delete(name),
        ownerDocument: { defaultView: view, fonts: { addEventListener: (name, fn) => fontListeners.set(name, fn), removeEventListener: name => fontListeners.delete(name) },
            createElement: () => ({ style: {}, children: [], setAttribute() {}, remove() {}, appendChild(child) { this.children.push(child); }, getBoundingClientRect() { return { width: this.children.reduce((total, child) => total + child.naturalWidth, 0) }; } }) },
        appendChild() { probes++; }
    });
    const fire = (name, extra = {}) => listeners.get(name)?.({ target: handle, key: "ArrowRight", pointerId: 1, button: 0, clientX: 100, preventDefault() {}, stopPropagation() {}, ...extra });
    const flush = () => { const pending = [...frames.values()]; frames.clear(); pending.forEach(fn => fn()); };
    return { root, table, rows, columns, makeCells, handle, fire, flush, listeners, viewListeners, fontListeners,
        frames, probeCount: () => probes, changeFont: value => { font = value; }, observer: () => Observer.instances.at(-1) };
}

test("unchanged table rerenders reuse measured widths without cloning another hidden table", () => {
    const f = fixture();
    assert.deepEqual({ ...api.connect(f.root) }, { title: 100, detail: 84 });
    f.columns[0].style.width = "260px"; // Blazor may replay the previous render's inline width.
    for (let i = 0; i < 12; i++) api.connect(f.root);
    assert.equal(f.probeCount(), 1);
    assert.equal(f.columns[0].style.width, "100px");
    assert.equal(f.listeners.size, 9);
    api.dispose(f.root);
});

test("grid width ends at the final real column even when the viewport has unused space", () => {
    const f = fixture(); api.connect(f.root);
    assert.equal(f.table.style.width, "184px");
    assert.equal(f.columns[1].style.width, "84px");
    f.root.clientWidth = 1600;
    f.viewListeners.get("resize")(); f.flush();
    assert.equal(f.table.style.width, "184px");
    assert.equal(f.columns[1].style.width, "84px");
    f.columns[1].style.width = "125px";
    api.synchronizeWidth(f.root);
    assert.equal(f.table.style.width, "225px");
    f.columns[0].hidden = true;
    api.synchronizeWidth(f.root);
    assert.equal(f.table.style.width, "125px");
    assert.equal(f.columns.length, 2);
    api.dispose(f.root);
});

test("text, paging visibility and inherited font changes invalidate automatic sizing", () => {
    const f = fixture(); api.connect(f.root);
    f.rows[1].cells = f.makeCells([150, 44]);
    f.observer().record({ type: "characterData", target: { parentElement: f.rows[1].cells[0] } });
    assert.equal(api.connect(f.root).title, 190);
    assert.equal(f.probeCount(), 2);
    f.rows[1].hidden = true;
    f.observer().record({ type: "attributes", attributeName: "hidden", target: f.table });
    assert.equal(api.connect(f.root).title, 80);
    f.changeFont("400 19px Arial");
    api.connect(f.root);
    assert.equal(f.probeCount(), 4);
    api.dispose(f.root);
});

test("owned width mutations do not start measurement loops", () => {
    const f = fixture(); api.connect(f.root);
    f.observer().record({ type: "attributes", attributeName: "style", target: f.table });
    f.observer().record({ type: "attributes", attributeName: "style", target: f.columns[0] });
    api.connect(f.root);
    assert.equal(f.probeCount(), 1);
    api.dispose(f.root);
});

test("manual widths survive content and viewport changes and Home resumes automatic sizing", () => {
    const f = fixture(), updates = []; api.connect(f.root, (...args) => updates.push(args));
    f.fire("keydown", { key: "ArrowRight" });
    assert.equal(f.columns[0].style.width, "110px");
    f.rows[1].cells = f.makeCells([500, 140]);
    f.observer().record({ type: "characterData", target: { parentElement: f.rows[1].cells[1] } });
    api.connect(f.root, (...args) => updates.push(args));
    assert.equal(f.columns[0].style.width, "110px");
    f.viewListeners.get("resize")(); f.flush();
    assert.equal(f.columns[0].style.width, "110px");
    assert.equal(f.columns[1].style.width, "180px");
    f.fire("keydown", { key: "Home" });
    assert.equal(f.columns[0].style.width, "260px");
    assert.deepEqual(updates.at(-1), ["title", 260, "auto"]);
    api.dispose(f.root);
});

test("native editor and completed font loading changes schedule one measurement frame", () => {
    const f = fixture(); api.connect(f.root);
    const editor = { closest: () => f.table };
    f.fire("input", { target: editor }); f.fire("change", { target: editor }); f.fontListeners.get("loadingdone")();
    assert.equal(f.frames.size, 1);
    f.flush();
    assert.equal(f.probeCount(), 2);
    api.dispose(f.root);
});

test("route removal disposal releases observers, global listeners and queued measurement frames", () => {
    const f = fixture(); api.connect(f.root);
    const observer = f.observer();
    f.viewListeners.get("resize")();
    api.dispose(f.root);
    assert.equal(observer.disconnected, true);
    assert.equal(f.listeners.size, 0);
    assert.equal(f.viewListeners.size, 0);
    assert.equal(f.fontListeners.size, 0);
    assert.equal(f.frames.size, 0);
});
