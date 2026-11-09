const drafts = new WeakMap();
const maxSourceBytes = 12 * 1024 * 1024;
const maxSide = 8192;
const maxPixels = 24_000_000;
const maxJpegBytes = 256 * 1024;
const jpegQuality = 0.75;

function pngDimensions(bytes) {
    const signature = [137, 80, 78, 71, 13, 10, 26, 10];
    if (bytes.length < 24 || !signature.every((value, index) => bytes[index] === value)) return null;
    const view = new DataView(bytes.buffer, bytes.byteOffset, bytes.byteLength);
    if (view.getUint32(8) !== 13 || view.getUint32(12) !== 0x49484452) return null;
    return [view.getUint32(16), view.getUint32(20)];
}

function jpegDimensions(bytes) {
    if (bytes[0] !== 0xff || bytes[1] !== 0xd8) return null;
    let offset = 2;
    while (offset < bytes.length - 3 && bytes[offset++] === 0xff) {
        while (bytes[offset] === 0xff) offset++;
        const marker = bytes[offset++];
        if (marker === 0 || marker === 0xda || marker === 0xd9) break;
        if (marker === 1 || (marker >= 0xd0 && marker <= 0xd7)) continue;
        const length = (bytes[offset] << 8) | bytes[offset + 1];
        if (length < 2 || offset + length > bytes.length) break;
        if (marker >= 0xc0 && marker <= 0xcf && ![0xc4, 0xc8, 0xcc].includes(marker)) {
            if (length < 8) return null;
            return [(bytes[offset + 5] << 8) | bytes[offset + 6], (bytes[offset + 3] << 8) | bytes[offset + 4]];
        }
        offset += length;
    }
    return null;
}

function webpDimensions(bytes) {
    if (bytes.length < 20) return null;
    const view = new DataView(bytes.buffer, bytes.byteOffset, bytes.byteLength);
    if (view.getUint32(0) !== 0x52494646 || view.getUint32(8) !== 0x57454250) return null;
    const end = view.getUint32(4, true) + 8;
    if (end !== bytes.length) return null;
    let canvasSize;
    for (let offset = 12; offset + 8 <= end;) {
        const kind = view.getUint32(offset);
        const length = view.getUint32(offset + 4, true);
        const data = offset + 8;
        offset = data + length + (length & 1);
        if (offset > end) return null;
        if (kind === 0x56503858) {
            if (length !== 10 || (bytes[data] & 2) !== 0) return null;
            const read24 = index => bytes[index] | (bytes[index + 1] << 8) | (bytes[index + 2] << 16);
            canvasSize = [read24(data + 4) + 1, read24(data + 7) + 1];
            continue;
        }

        let size;
        if (kind === 0x56503820 && length >= 10 && bytes[data + 3] === 0x9d && bytes[data + 4] === 1 && bytes[data + 5] === 0x2a) {
            size = [view.getUint16(data + 6, true) & 0x3fff, view.getUint16(data + 8, true) & 0x3fff];
        } else if (kind === 0x5650384c && length >= 5 && bytes[data] === 0x2f) {
            const header = view.getUint32(data + 1, true);
            size = [(header & 0x3fff) + 1, ((header >>> 14) & 0x3fff) + 1];
        }
        if (size) return canvasSize && (canvasSize[0] !== size[0] || canvasSize[1] !== size[1]) ? null : size;
    }
    return null;
}

function oversized(width, height) {
    return width > maxSide || height > maxSide || width * height > maxPixels;
}

function current(owner, draft) {
    return owner.isConnected && drafts.get(owner) === draft;
}

export function release(owner, version) {
    const draft = drafts.get(owner);
    if (!draft || (version !== undefined && draft.version !== version)) return;
    drafts.delete(owner);
    draft.listeners?.abort();
    if (draft.canvas) draft.canvas.width = draft.canvas.width;
    draft.image?.close();
}

export async function load(owner, picker, version) {
    const file = picker.files?.[0];
    picker.value = "";
    release(owner);
    if (!file || !owner.isConnected) return "empty";
    if (file.size > maxSourceBytes) return "too-large";
    if (file.size === 0) return "invalid";

    const draft = { version };
    drafts.set(owner, draft);
    let failure = "invalid";
    try {
        const bytes = new Uint8Array(await file.arrayBuffer());
        if (!current(owner, draft)) return "stale";
        const size = pngDimensions(bytes) ?? jpegDimensions(bytes) ?? webpDimensions(bytes);
        if (!size || !size[0] || !size[1]) return failure;
        if (oversized(...size)) return failure = "too-large";

        const image = await createImageBitmap(file, { imageOrientation: "from-image" });
        if (!current(owner, draft)) {
            image.close();
            return "stale";
        }
        if (oversized(image.width, image.height) || !image.width || !image.height) {
            image.close();
            return failure = "too-large";
        }

        draft.image = image;
        return "ready";
    } catch {
        return failure;
    } finally {
        if (!draft.image) release(owner, version);
    }
}

function clamp(value, minimum, maximum) {
    return Math.max(minimum, Math.min(maximum, value));
}

function readRange(input, fallback, min, max) {
    return Number.isFinite(input.valueAsNumber) ? clamp(input.valueAsNumber, min, max) : fallback;
}

function crop(draft) {
    const zoom = readRange(draft.zoom, 100, 100, 300);
    const side = Math.min(draft.image.width, draft.image.height) * 100 / zoom;
    const horizontal = (readRange(draft.horizontal, 0, -100, 100) + 100) / 200;
    const vertical = (readRange(draft.vertical, 0, -100, 100) + 100) / 200;
    return {
        side,
        left: (draft.image.width - side) * horizontal,
        top: (draft.image.height - side) * vertical
    };
}

function draw(context, image, area, size) {
    context.fillStyle = "#fff";
    context.fillRect(0, 0, size, size);
    context.drawImage(image, area.left, area.top, area.side, area.side, 0, 0, size, size);
}

export function attach(owner, editor, version) {
    const draft = drafts.get(owner);
    if (!draft?.image || draft.version !== version || !current(owner, draft)) return false;
    const canvas = editor.querySelector("canvas");
    const zoom = editor.querySelector('[data-avatar-range="zoom"]');
    const horizontal = editor.querySelector('[data-avatar-range="horizontal"]');
    const vertical = editor.querySelector('[data-avatar-range="vertical"]');
    const context = canvas?.getContext("2d", { alpha: false });
    if (!context || !zoom || !horizontal || !vertical) return false;

    draft.listeners?.abort();
    draft.listeners = new AbortController();
    Object.assign(draft, { canvas, zoom, horizontal, vertical });
    zoom.value = "100";
    horizontal.value = vertical.value = "0";

    const options = { signal: draft.listeners.signal };
    const redraw = () => draw(context, draft.image, crop(draft), canvas.width);
    for (const control of [zoom, horizontal, vertical]) control.addEventListener("input", redraw, options);

    let drag;
    canvas.addEventListener("pointerdown", event => {
        if (event.button !== 0 || horizontal.disabled || drag) return;
        const area = crop(draft);
        drag = { id: event.pointerId, x: event.clientX, y: event.clientY, area };
        canvas.setPointerCapture(event.pointerId);
    }, options);
    canvas.addEventListener("pointermove", event => {
        if (drag?.id !== event.pointerId || horizontal.disabled || !canvas.clientWidth) return;
        const { area } = drag;
        const scale = area.side / canvas.clientWidth;
        const remainingX = draft.image.width - area.side;
        const remainingY = draft.image.height - area.side;
        if (remainingX > 0) horizontal.value = String(Math.round(clamp((area.left - (event.clientX - drag.x) * scale) * 200 / remainingX - 100, -100, 100)));
        if (remainingY > 0) vertical.value = String(Math.round(clamp((area.top - (event.clientY - drag.y) * scale) * 200 / remainingY - 100, -100, 100)));
        redraw();
    }, options);
    for (const name of ["pointerup", "pointercancel", "lostpointercapture"]) {
        canvas.addEventListener(name, event => {
            if (drag?.id === event.pointerId) drag = undefined;
        }, options);
    }

    redraw();
    return true;
}

export async function encode(owner, version) {
    const draft = drafts.get(owner);
    if (!draft?.canvas || draft.version !== version || !current(owner, draft)) throw new Error("The crop is unavailable.");
    const area = crop(draft);
    const canvas = owner.ownerDocument.createElement("canvas");
    try {
        for (let side = Math.max(1, Math.min(512, Math.floor(area.side)));;) {
            canvas.width = canvas.height = side;
            const context = canvas.getContext("2d", { alpha: false });
            if (!context) throw new Error("Canvas is unavailable.");
            draw(context, draft.image, area, side);
            const blob = await new Promise(resolve => canvas.toBlob(resolve, "image/jpeg", jpegQuality));
            if (!current(owner, draft)) throw new Error("The crop was closed.");
            if (blob?.type !== "image/jpeg" || blob.size === 0) throw new Error("JPEG encoding failed.");
            if (blob.size <= maxJpegBytes) return DotNet.createJSStreamReference(blob);
            if (side === 1) throw new Error("The compressed photo is too large.");
            side = Math.max(1, Math.floor(side * 0.75));
        }
    } finally {
        canvas.width = canvas.height = 0;
    }
}
