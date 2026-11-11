import assert from "node:assert/strict";
import { readFile } from "node:fs/promises";
import test from "node:test";

const source = await readFile(new URL("../../src/Flourish.Blazor/Flourish.Blazor.Framework/wwwroot/avatar-editor.js", import.meta.url));
const { load, attach, reset, encode, release } = await import("data:text/javascript;base64," + source.toString("base64"));

test('Section avatars leave 36px before following content, including read-only portraits', async () => {
    const css = await readFile(new URL("../../src/Flourish.Blazor/Flourish.Blazor.Framework/wwwroot/avatar.css",import.meta.url),'utf8');
    assert.match(css,/\.f-section\s*>\s*\.f-avatar:not\(:last-child\)\s*\{\s*margin-block-end:36px;\s*\}/);
});

function png(width, height) {
    const bytes = Buffer.alloc(33);
    bytes.set([137, 80, 78, 71, 13, 10, 26, 10]);
    bytes.writeUInt32BE(13, 8);
    bytes.write("IHDR", 12);
    bytes.writeUInt32BE(width, 16);
    bytes.writeUInt32BE(height, 20);
    return new Blob([bytes], { type: "image/png" });
}

function jpeg(width, height) {
    return new Blob([Uint8Array.of(
        0xff, 0xd8, 0xff, 0xc0, 0, 11, 8,
        height >> 8, height & 255, width >> 8, width & 255,
        1, 1, 0x11, 0, 0xff, 0xd9
    )], { type: "image/jpeg" });
}

function webp(width, height) {
    const bytes = Buffer.alloc(26);
    bytes.write("RIFF", 0);
    bytes.writeUInt32LE(18, 4);
    bytes.write("WEBPVP8L", 8);
    bytes.writeUInt32LE(5, 16);
    bytes[20] = 0x2f;
    bytes.writeUInt32LE((width - 1) | ((height - 1) << 14), 21);
    return new Blob([bytes], { type: "image/webp" });
}

function bitmap(width, height) {
    return { width, height, closed: 0, close() { this.closed++; } };
}

class Control extends EventTarget {
    value = "0";
    disabled = false;
    get valueAsNumber() { return Number(this.value); }
}

class Canvas extends EventTarget {
    width = 256;
    height = 256;
    clientWidth = 256;
    draws = [];
    fills = [];
    encodes = [];
    constructor(sizes = [20]) { super(); this.sizes = sizes; }
    getContext() {
        return {
            fillRect: (...args) => this.fills.push(args),
            drawImage: (...args) => this.draws.push(args)
        };
    }
    toBlob(callback, type, quality) {
        this.encodes.push({ width: this.width, height: this.height, type, quality });
        callback(new Blob([new Uint8Array(this.sizes.shift())], { type }));
    }
    setPointerCapture(id) { this.capturedPointer = id; }
}

function fixture(file, output = new Canvas()) {
    const owner = { isConnected: true, ownerDocument: { createElement: () => output } };
    const picker = { files: [file], value: "photo" };
    const canvas = new Canvas();
    const axes = { zoom: new Control(), horizontal: new Control(), vertical: new Control() };
    const editor = {
        querySelector(selector) {
            return selector === "canvas" ? canvas : axes[selector.split('"')[1]];
        }
    };
    return { owner, picker, canvas, output, editor, ...axes };
}

test("native JPEG PNG and WebP enter one oriented browser decode", async () => {
    for (const file of [jpeg(600, 400), png(600, 400), webp(600, 400)]) {
        const image = bitmap(400, 600);
        globalThis.createImageBitmap = async (actual, options) => {
            assert.equal(actual, file);
            assert.deepEqual(options, { imageOrientation: "from-image" });
            return image;
        };
        const { owner, picker } = fixture(file);
        assert.equal(await load(owner, picker, 1), "ready");
        assert.equal(picker.value, "");
        release(owner, 1);
        assert.equal(image.closed, 1);
    }
});

test("rejects oversized bytes without reading or decoding and bounds dimensions before decode", async () => {
    let reads = 0;
    let decodes = 0;
    globalThis.createImageBitmap = async () => { decodes++; return bitmap(1, 1); };
    const file = { size: 12 * 1024 * 1024 + 1, arrayBuffer() { reads++; } };
    const { owner, picker } = fixture(file);
    assert.equal(await load(owner, picker, 1), "too-large");
    assert.equal(reads, 0);
    for (const large of [png(8193, 1), png(5000, 5000), jpeg(6000, 5000), webp(9000, 2000)]) {
        picker.files = [large];
        assert.equal(await load(owner, picker, 2), "too-large");
    }
    assert.equal(decodes, 0);
});

test("rejects SVG masquerading as PNG, malformed headers and zero dimensions", async () => {
    let decodes = 0;
    globalThis.createImageBitmap = async () => { decodes++; return bitmap(1, 1); };
    const fakeSvg = new Blob(['<svg xmlns="http://www.w3.org/2000/svg" onload="alert(1)"/>'], { type: "image/png" });
    const badJpeg = new Blob([Uint8Array.of(0xff, 0xd8, 0xff, 0xc0, 0xff, 0xff, 1, 2)], { type: "image/jpeg" });
    for (const file of [fakeSvg, badJpeg, png(0, 400)]) {
        const { owner, picker } = fixture(file);
        assert.equal(await load(owner, picker, 1), "invalid");
    }
    assert.equal(decodes, 0);
});

test("returns rejected or oversized decoded bitmaps to the browser", async () => {
    const { owner, picker } = fixture(png(900, 600));
    globalThis.createImageBitmap = async () => { throw new Error("corrupt image"); };
    assert.equal(await load(owner, picker, 1), "invalid");
    const large = bitmap(5000, 5000);
    globalThis.createImageBitmap = async () => large;
    assert.equal(await load(owner, picker, 2), "too-large");
    assert.equal(large.closed, 1);
});

test("a replaced or closed file cannot install its late decode or release the new one", async () => {
    const first = bitmap(800, 600);
    const next = bitmap(700, 600);
    const { owner, picker, editor } = fixture(png(800, 600));
    let finish;
    let started;
    const decoding = new Promise(resolve => { started = resolve; });
    globalThis.createImageBitmap = () => new Promise(resolve => {
        finish = resolve;
        started();
    });
    const pending = load(owner, picker, 1);
    await decoding;
    picker.files = [png(700, 600)];
    globalThis.createImageBitmap = async () => next;
    assert.equal(await load(owner, picker, 2), "ready");
    finish(first);
    assert.equal(await pending, "stale");
    assert.equal(first.closed, 1);
    release(owner, 1);
    assert.equal(attach(owner, editor, 2), true);
    assert.equal(next.closed, 0);
    release(owner, 2);
    assert.equal(next.closed, 1);
});

test("zoom and two axes redraw within the image and release detaches listeners", async () => {
    const image = bitmap(1200, 800);
    globalThis.createImageBitmap = async () => image;
    const { owner, picker, editor, canvas, zoom, horizontal, vertical } = fixture(png(1200, 800));
    assert.equal(await load(owner, picker, 1), "ready");
    assert.equal(attach(owner, editor, 1), true);
    assert.deepEqual(canvas.draws.at(-1), [image, 200, 0, 800, 800, 0, 0, 256, 256]);
    zoom.value = "200";
    horizontal.value = "100";
    vertical.value = "-100";
    horizontal.dispatchEvent(new Event("input"));
    assert.deepEqual(canvas.draws.at(-1), [image, 800, 0, 400, 400, 0, 0, 256, 256]);
    const draws = canvas.draws.length;
    release(owner, 1);
    vertical.dispatchEvent(new Event("input"));
    assert.equal(canvas.draws.length, draws);
    assert.equal(image.closed, 1);
});

test("pointer drag pans the circular preview and leaves keyboard ranges synchronized", async () => {
    const image = bitmap(1200, 800);
    globalThis.createImageBitmap = async () => image;
    const { owner, picker, editor, canvas, horizontal } = fixture(png(1200, 800));
    await load(owner, picker, 1);
    attach(owner, editor, 1);
    canvas.dispatchEvent(Object.assign(new Event("pointerdown"), { button: 0, pointerId: 3, clientX: 100, clientY: 100 }));
    canvas.dispatchEvent(Object.assign(new Event("pointermove"), { pointerId: 3, clientX: 164, clientY: 100 }));
    assert.equal(canvas.capturedPointer, 3);
    assert.equal(horizontal.value, "-100");
    assert.deepEqual(canvas.draws.at(-1), [image, 0, 0, 800, 800, 0, 0, 256, 256]);
    release(owner, 1);
});

test("encoding keeps JPEG quality .75, lowers dimensions to meet size budget and returns the JPEG Blob for Blazor streaming", async () => {
    const image = bitmap(1100, 850);
    globalThis.createImageBitmap = async () => image;
    const output = new Canvas([256 * 1024 + 1, 64 * 1024]);
    const { owner, picker, editor } = fixture(png(1100, 850), output);
    await load(owner, picker, 1);
    attach(owner, editor, 1);
    const result = await encode(owner, 1);
    assert.ok(result instanceof Blob);
    assert.equal(result.type, "image/jpeg");
    assert.equal(result.size, 64 * 1024);
    assert.deepEqual(output.encodes, [
        { width: 512, height: 512, type: "image/jpeg", quality: 0.75 },
        { width: 384, height: 384, type: "image/jpeg", quality: 0.75 }
    ]);
    assert.deepEqual(output.draws.at(-1), [image, 125, 0, 850, 850, 0, 0, 384, 384]);
    assert.equal(output.width, 0);
    release(owner, 1);
    await assert.rejects(encode(owner, 1), /crop is unavailable/);
});

test("a crop closed during canvas encoding does not expose a completed Blob", async () => {
    const image = bitmap(40, 30);
    globalThis.createImageBitmap = async () => image;
    const output = new Canvas();
    let finish;
    output.toBlob = callback => { finish = callback; };
    const { owner, picker, editor } = fixture(png(40, 30), output);
    await load(owner, picker, 1);
    attach(owner, editor, 1);
    const pending = encode(owner, 1);
    assert.equal(output.width, 30);
    release(owner, 1);
    finish(new Blob(["jpeg"], { type: "image/jpeg" }));
    await assert.rejects(pending, /crop was closed/);
    assert.equal(output.width, 0);
});

test("reset restores the original centered crop without decoding or saving again", async () => {
    const image = bitmap(600, 400);
    let decodes = 0;
    globalThis.createImageBitmap = async () => { decodes++; return image; };
    const state = fixture(png(600, 400));
    assert.equal(await load(state.owner, state.picker, 7), "ready");
    assert.equal(attach(state.owner, state.editor, 7), true);
    const initial = state.canvas.draws.at(-1);
    state.zoom.value = "240";
    state.horizontal.value = "80";
    state.vertical.value = "-60";
    state.zoom.dispatchEvent(new Event("input"));
    assert.notDeepEqual(state.canvas.draws.at(-1), initial);
    state.canvas.dispatchEvent(Object.assign(new Event("pointerdown"), { button: 0, pointerId: 1, clientX: 100, clientY: 100 }));
    assert.equal(reset(state.owner, 7), true);
    assert.equal(state.zoom.value, "100");
    assert.equal(state.horizontal.value, "0");
    assert.equal(state.vertical.value, "0");
    assert.deepEqual(state.canvas.draws.at(-1), initial);
    state.canvas.dispatchEvent(Object.assign(new Event("pointermove"), { pointerId: 1, clientX: 200, clientY: 200 }));
    assert.equal(state.horizontal.value, "0");
    assert.equal(decodes, 1);
    assert.equal(image.closed, 0);
    assert.equal(state.output.encodes.length, 0);
    release(state.owner);
});

test("reset cannot modify disabled, replaced, closed or disconnected crops", async () => {
    globalThis.createImageBitmap = async () => bitmap(600, 400);
    const state = fixture(png(600, 400));
    assert.equal(await load(state.owner, state.picker, 8), "ready");
    assert.equal(attach(state.owner, state.editor, 8), true);
    state.zoom.value = "200";
    state.zoom.disabled = true;
    assert.equal(reset(state.owner, 8), false);
    state.zoom.disabled = false;
    assert.equal(reset(state.owner, 7), false);
    state.owner.isConnected = false;
    assert.equal(reset(state.owner, 8), false);
    state.owner.isConnected = true;
    assert.equal(state.zoom.value, "200");
    release(state.owner, 8);
    assert.equal(reset(state.owner, 8), false);
});