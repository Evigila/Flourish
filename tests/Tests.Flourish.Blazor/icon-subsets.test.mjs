import assert from 'node:assert/strict';
import { readFile } from 'node:fs/promises';
import { createHash } from 'node:crypto';
import { inflateSync } from 'node:zlib';
import test from 'node:test';

const root = new URL('../../', import.meta.url);
const assets = new URL('src/Flourish.Blazor/Flourish.Blazor.Framework/wwwroot/icons/material/', root);
const load = path => readFile(new URL(path, root));
const common = await readFile(new URL('MaterialSymbolsOutlined.common.woff', assets));
const original = await load('src/Flourish.WPF/Flourish.WPF.Framework/Icons/MaterialSymbolsOutlined.ttf');
const fullWoff = await readFile(new URL('MaterialSymbolsOutlined.woff2', assets));
const css = await readFile(new URL('subsets.css', assets), 'utf8');
const names = JSON.parse(await readFile(new URL('common-icons.json', assets), 'utf8')).names;
const provenance = JSON.parse(await readFile(new URL('source.json', assets), 'utf8'));
const mapping = new Map((await load('src/Flourish.Blazor/Flourish.Blazor.Framework/Icons/MaterialSymbolsOutlined.codepoints'))
    .toString().trim().split(/\r?\n/).map(line => { const [name, point] = line.split(' '); return [name, Number.parseInt(point, 16)]; }));
const hash = value => createHash('sha256').update(value).digest('hex');

function tableChecksum(data) {
    const padded = Buffer.alloc(Math.ceil(data.length / 4) * 4);
    data.copy(padded);
    let result = 0;
    for (let offset = 0; offset < padded.length; offset += 4) result = (result + padded.readUInt32BE(offset)) >>> 0;
    return result;
}
function decodeWoff(font) {
    assert.equal(font.toString('ascii', 0, 4), 'wOFF');
    assert.equal(font.readUInt32BE(8), font.length);
    const count = font.readUInt16BE(12), result = new Map();
    let sfntSize = 12 + 16 * count;
    for (let index = 0; index < count; index++) {
        const row = 44 + index * 20, tag = font.toString('ascii', row, row + 4);
        const offset = font.readUInt32BE(row + 4), packed = font.readUInt32BE(row + 8), length = font.readUInt32BE(row + 12);
        assert.ok(offset + packed <= font.length);
        const bytes = font.subarray(offset, offset + packed), data = packed < length ? inflateSync(bytes) : Buffer.from(bytes);
        assert.equal(data.length, length, tag);
        const checked = Buffer.from(data);
        if (tag === 'head') checked.fill(0, 8, 12);
        assert.equal(tableChecksum(checked), font.readUInt32BE(row + 16), tag);
        result.set(tag, data); sfntSize += Math.ceil(length / 4) * 4;
    }
    assert.equal(sfntSize, font.readUInt32BE(16));
    return result;
}
function sfntTables(font) {
    const result = new Map();
    for (let index = 0; index < font.readUInt16BE(4); index++) {
        const row = 12 + index * 16, tag = font.toString('ascii', row, row + 4);
        const offset = font.readUInt32BE(row + 8), length = font.readUInt32BE(row + 12);
        result.set(tag, font.subarray(offset, offset + length));
    }
    return result;
}
function glyphFor(cmap, point) {
    for (let index = 0; index < cmap.readUInt16BE(2); index++) {
        const offset = cmap.readUInt32BE(8 + index * 8), format = cmap.readUInt16BE(offset);
        if (format === 12) {
            for (let group = 0; group < cmap.readUInt32BE(offset + 12); group++) {
                const row = offset + 16 + group * 12, start = cmap.readUInt32BE(row), end = cmap.readUInt32BE(row + 4);
                if (point >= start && point <= end) return cmap.readUInt32BE(row + 8) + point - start;
            }
        } else if (format === 4 && point <= 0xFFFF) {
            const count = cmap.readUInt16BE(offset + 6) / 2;
            for (let segment = 0; segment < count; segment++) {
                const end = cmap.readUInt16BE(offset + 14 + segment * 2);
                const start = cmap.readUInt16BE(offset + 16 + count * 2 + segment * 2);
                if (point < start || point > end) continue;
                const delta = cmap.readInt16BE(offset + 16 + count * 4 + segment * 2);
                const position = offset + 16 + count * 6 + segment * 2, range = cmap.readUInt16BE(position);
                if (!range) return (point + delta) & 0xFFFF;
                const glyph = cmap.readUInt16BE(position + range + 2 * (point - start));
                return glyph ? (glyph + delta) & 0xFFFF : 0;
            }
        }
    }
    return 0;
}
function ranges(text) {
    return [...text.matchAll(/U\+([0-9A-F]+)(?:-([0-9A-F]+))?/g)].map(match => [Number.parseInt(match[1], 16), Number.parseInt(match[2] ?? match[1], 16)]);
}
function variationBlocks(data) {
    const count = data.readUInt16BE(12), long = data.readUInt16BE(14) & 1, start = data.readUInt32BE(16);
    const offset = index => long ? data.readUInt32BE(20 + 4 * index) : data.readUInt16BE(20 + 2 * index) * 2;
    return Array.from({ length:count }, (_, index) => data.subarray(start + offset(index), start + offset(index + 1)));
}
const subset = decodeWoff(common), source = sfntTables(original);
const faces = [...css.matchAll(/@font-face\s*\{([^}]+)\}/g)].map(match => match[1]);

test('common and full font ranges partition Unicode so common glyphs never select the full download', () => {
    assert.equal(faces.length, 2);
    assert.match(faces[0], /MaterialSymbolsOutlined\.common\.woff/);
    assert.match(faces[1], /MaterialSymbolsOutlined\.woff2/);
    for (const face of faces) {
        assert.match(face, /font-family:'Material Symbols Outlined'/);
        assert.match(face, /font-weight:100 700/);
    }
    const commonRanges = ranges(faces[0]), all = [...commonRanges, ...ranges(faces[1])].sort((a, b) => a[0] - b[0]);
    assert.equal(all[0][0], 0);
    assert.equal(all.at(-1)[1], 0x10FFFF);
    for (let index = 1; index < all.length; index++) assert.equal(all[index - 1][1] + 1, all[index][0], 'overlap or coverage hole');
    for (const name of names) assert.equal(commonRanges.filter(([a, b]) => mapping.get(name) >= a && mapping.get(name) <= b).length, 1, name);
    for (const point of mapping.values()) assert.equal(all.filter(([a, b]) => point >= a && point <= b).length, 1);
    assert.ok([...mapping.values()].some(point => !commonRanges.some(([a, b]) => point >= a && point <= b)), 'dynamic uncommon names must retain the full fallback');
    assert.ok(names.includes('help_outline'), 'unknown-name fallback must remain common');
});

test('common WOFF retains original glyph IDs outlines and every retained variable delta block', () => {
    const originalVariations = variationBlocks(source.get('gvar')), retainedVariations = variationBlocks(subset.get('gvar'));
    assert.equal(retainedVariations.length, originalVariations.length);
    for (const tag of ['fvar', 'avar', 'STAT', 'HVAR']) assert.deepEqual(subset.get(tag), source.get(tag), tag);
    const originalLoca = source.get('loca'), subsetLoca = subset.get('loca');
    const originalLong = source.get('head').readInt16BE(50) === 1, subsetLong = subset.get('head').readInt16BE(50) === 1;
    const position = (data, index, long) => long ? data.readUInt32BE(index * 4) : data.readUInt16BE(index * 2) * 2;
    for (const name of names) {
        const glyph = glyphFor(subset.get('cmap'), mapping.get(name));
        assert.ok(glyph > 0, name);
        assert.equal(glyph, glyphFor(source.get('cmap'), mapping.get(name)), name);
        const a = position(originalLoca, glyph, originalLong), b = position(originalLoca, glyph + 1, originalLong);
        const c = position(subsetLoca, glyph, subsetLong), d = position(subsetLoca, glyph + 1, subsetLong);
        assert.deepEqual(subset.get('glyf').subarray(c, d), source.get('glyf').subarray(a, b), name);
        assert.deepEqual(retainedVariations[glyph], originalVariations[glyph], name + ' all variation axes');
    }
});

test('common font stays small and licensed while the original full dynamic font remains byte-identical', async () => {
    assert.ok(common.length < 160_000);
    assert.equal(common.length, provenance.commonSubset.commonBytes);
    assert.equal(hash(common), provenance.commonSubset.sha256);
    assert.equal(fullWoff.length, 4_003_088);
    assert.equal(hash(fullWoff), '96c9913f4d415adeefcc3fa13f4f75912648f6f426be8b50e5e20c035b17cdf3');
    assert.match(await readFile(new URL('LICENSE.txt', assets), 'utf8'), /Apache License/);
    const entry = (await load('src/Flourish.Blazor/Flourish.Blazor.Framework/wwwroot/framework.css')).toString();
    assert.match(entry, /@import url\('\.\/icons\/material\/subsets\.css'\)/);
    assert.doesNotMatch(entry, /src:url\('\.\/icons\/material\/MaterialSymbolsOutlined\.woff2'\)/, 'an unconditional face would reinstate the full first-load download');
});
