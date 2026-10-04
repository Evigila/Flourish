import assert from 'node:assert/strict';
import { readFile } from 'node:fs/promises';
import test from 'node:test';

const sourceUrl = new URL('../../src/Flourish.Blazor/Flourish.Blazor.Framework/wwwroot/clipboard.js', import.meta.url);
const source = await readFile(sourceUrl, 'utf8');
const { copyText } = await import(`data:text/javascript;base64,${Buffer.from(source).toString('base64')}`);
const exactCode = `const pi = 'π';\nconsole.log('你好 🚀');\r\n<&>`;

function installDom({ clipboard, execResult = true, execError, appendError, restoreFocusError, restoreSelectionError } = {}) {
    const calls = { clipboard: [], exec: [], restoredRanges: [], textarea: undefined };
    const originalRanges = [{ id: 'first' }, { id: 'second' }];
    const active = {
        isConnected: true,
        selectionStart: 2,
        selectionEnd: 8,
        selectionDirection: 'backward',
        focus(options) {
            calls.activeFocus = options;
            if (restoreFocusError) throw restoreFocusError;
            document.activeElement = active;
        },
        setSelectionRange(start, end, direction) {
            calls.inputSelection = [start, end, direction];
            if (restoreSelectionError) throw restoreSelectionError;
        }
    };
    const selection = {
        ranges: [...originalRanges],
        get rangeCount() { return this.ranges.length; },
        getRangeAt(index) {
            const range = this.ranges[index];
            return { cloneRange: () => ({ id: range.id }) };
        },
        removeAllRanges() {
            calls.selectionCleared = true;
            this.ranges = [];
        },
        addRange(range) {
            calls.restoredRanges.push(range.id);
            this.ranges.push(range);
        }
    };
    const body = {
        children: [],
        append(node) {
            if (appendError) throw appendError;
            this.children.push(node);
            node.isConnected = true;
        }
    };
    const documentMock = {
        activeElement: active,
        body,
        createElement(name) {
            assert.equal(name, 'textarea');
            const attributes = new Map();
            const textarea = {
                value: '',
                style: {},
                isConnected: false,
                setAttribute(name, value) { attributes.set(name, value); },
                getAttribute(name) { return attributes.get(name); },
                focus(options) { calls.textareaFocus = options; documentMock.activeElement = textarea; },
                select() { calls.textareaSelected = true; },
                remove() {
                    calls.textareaRemoved = true;
                    this.isConnected = false;
                    const index = body.children.indexOf(this);
                    if (index >= 0) body.children.splice(index, 1);
                }
            };
            calls.textarea = textarea;
            return textarea;
        },
        execCommand(command) {
            calls.exec.push(command);
            calls.copiedValue = calls.textarea?.value;
            if (execError) throw execError;
            return execResult;
        }
    };
    const clipboardValue = clipboard === undefined ? undefined : {
        async writeText(text) {
            calls.clipboard.push(text);
            return clipboard(text);
        }
    };
    Object.defineProperty(globalThis, 'navigator', { configurable: true, value: { clipboard: clipboardValue } });
    Object.defineProperty(globalThis, 'document', { configurable: true, value: documentMock });
    Object.defineProperty(globalThis, 'window', { configurable: true, value: { getSelection: () => selection } });
    return { calls, active, body, selection };
}

test('Clipboard API receives the exact multiline Unicode source and skips fallback DOM', async () => {
    const { calls, body } = installDom({ clipboard: async () => undefined });
    assert.equal(await copyText(exactCode), true);
    assert.deepEqual(calls.clipboard, [exactCode]);
    assert.equal(calls.textarea, undefined);
    assert.deepEqual(body.children, []);
});

test('a rejected Clipboard API uses a tiny fixed fallback and restores focus and selections', async () => {
    const { calls, body, selection } = installDom({ clipboard: async () => { throw new Error('NotAllowedError'); } });
    assert.equal(await copyText(exactCode), true);
    assert.deepEqual(calls.clipboard, [exactCode]);
    assert.deepEqual(calls.exec, ['copy']);
    assert.equal(calls.copiedValue, exactCode);
    assert.equal(calls.textarea?.getAttribute('readonly'), '');
    assert.equal(calls.textarea?.getAttribute('aria-hidden'), 'true');
    assert.deepEqual(
        Object.fromEntries(['position', 'top', 'left', 'width', 'height', 'padding', 'border', 'opacity'].map(key => [key, calls.textarea?.style[key]])),
        { position: 'fixed', top: '0', left: '0', width: '1px', height: '1px', padding: '0', border: '0', opacity: '0' }
    );
    assert.deepEqual(body.children, []);
    assert.equal(calls.textareaRemoved, true);
    assert.deepEqual(calls.textareaFocus, { preventScroll: true });
    assert.equal(calls.textareaSelected, true);
    assert.deepEqual(calls.activeFocus, { preventScroll: true });
    assert.deepEqual(calls.inputSelection, [2, 8, 'backward']);
    assert.equal(calls.selectionCleared, true);
    assert.deepEqual(calls.restoredRanges, ['first', 'second']);
    assert.deepEqual(selection.ranges.map(range => range.id), ['first', 'second']);
});

test('execCommand false is reported without leaking the fallback or losing selection', async () => {
    const { calls, body } = installDom({ execResult: false });
    assert.equal(await copyText(exactCode), false);
    assert.equal(calls.copiedValue, exactCode);
    assert.deepEqual(body.children, []);
    assert.equal(calls.textareaRemoved, true);
    assert.deepEqual(calls.inputSelection, [2, 8, 'backward']);
    assert.deepEqual(calls.restoredRanges, ['first', 'second']);
});

test('fallback append failure is converted to false without leaving a DOM node', async () => {
    const { body } = installDom({ appendError: new Error('detached document') });
    assert.equal(await copyText(exactCode), false);
    assert.deepEqual(body.children, []);
});

test('restoration failures do not change a successful copy and remaining restoration still runs', async () => {
    const { calls, body } = installDom({ restoreFocusError: new Error('focus denied'), restoreSelectionError: new Error('selection denied') });
    assert.equal(await copyText(exactCode), true);
    assert.deepEqual(body.children, []);
    assert.equal(calls.textareaRemoved, true);
    assert.equal(calls.selectionCleared, true);
    assert.deepEqual(calls.restoredRanges, ['first', 'second']);
});

test('execCommand exceptions return false and always remove the fallback', async () => {
    const { calls, body } = installDom({ execError: new Error('copy unavailable') });
    assert.equal(await copyText(exactCode), false);
    assert.deepEqual(body.children, []);
    assert.equal(calls.textareaRemoved, true);
});
