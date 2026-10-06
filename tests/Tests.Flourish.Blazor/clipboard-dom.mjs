import assert from 'node:assert/strict';
import { readFile } from 'node:fs/promises';
import test from 'node:test';

const source = await readFile(new URL('../../src/Flourish.Blazor/Flourish.Blazor.Framework/wwwroot/clipboard.js', import.meta.url), 'utf8');
const { copyText } = await import(`data:text/javascript;base64,${Buffer.from(source).toString('base64')}`);
const exactCode = `const pi = 'π';\nconsole.log('你好 🚀');\r\n<&>`;

function install(clipboard) {
    const writes = [];
    Object.defineProperty(globalThis, 'navigator', { configurable: true, value: {
        clipboard: clipboard === undefined ? undefined : { writeText: async text => { writes.push(text); return clipboard(text); } }
    } });
    Object.defineProperty(globalThis, 'document', { configurable: true, get() { throw new Error('Clipboard must not create compatibility DOM.'); } });
    return writes;
}

test('current Clipboard API receives exact multiline Unicode content without DOM or focus changes', async () => {
    const writes = install(async () => {});
    assert.equal(await copyText(exactCode), true);
    assert.deepEqual(writes, [exactCode]);
});
test('permission denial is reported as failure without execCommand or a hidden textarea', async () => {
    const writes = install(async () => { throw new Error('NotAllowedError'); });
    assert.equal(await copyText(exactCode), false);
    assert.deepEqual(writes, [exactCode]);
});
test('missing current Clipboard API is not replaced by a legacy browser path', async () => {
    install();
    assert.equal(await copyText(exactCode), false);
});
test('empty text is copied verbatim through the same current API', async () => {
    const writes = install(async () => {});
    assert.equal(await copyText(''), true);
    assert.deepEqual(writes, ['']);
});
test('source exposes no compatibility command or copy DOM', () => {
    assert.doesNotMatch(source, /execCommand|createElement|textarea|selectionStart/);
});
