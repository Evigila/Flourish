import assert from 'node:assert/strict';
import { readFileSync } from 'node:fs';
import vm from 'node:vm';
import { test } from 'node:test';

const source = readFileSync(new URL('../../src/Flourish.Extensions/Flourish.Extensions.Culture.Blazor/wwwroot/browser-preferences.js', import.meta.url), 'utf8');
function browser(protocol = 'https:', blocked = false) {
    const cookies = new Map();
    const writes = [];
    const document = {};
    Object.defineProperty(document, 'cookie', {
        get: () => [...cookies].map(([key, value]) => `${key}=${value}`).join('; '),
        set: value => {
            writes.push(value);
            if (blocked) return;
            const pair = value.split(';')[0];
            const equals = pair.indexOf('=');
            cookies.set(pair.slice(0, equals), pair.slice(equals + 1));
        }
    });
    const context = { document, location: { protocol } };
    vm.createContext(context);
    vm.runInContext(source.replace('export function', 'function') + '\nthis.saveCulture = saveCulture;', context);
    return { ...context, cookies, writes };
}

test('saves UI and format cultures in one persistent request cookie with application path and HTTPS protection', () => {
    const current = browser();
    current.cookies.set('business', 'keep');
    current.saveCulture('zh-CN', 'pt-BR', '/gallery/', 365 * 86400);
    assert.equal(decodeURIComponent(current.cookies.get('.AspNetCore.Culture')), 'c=pt-BR|uic=zh-CN');
    assert.equal(current.cookies.get('business'), 'keep');
    assert.match(current.writes[0], /; path=\/gallery\/; max-age=31536000; samesite=lax; secure$/);
});
test('each browser keeps its own latest selection and loopback HTTP remains usable', () => {
    const first = browser('http:');
    const second = browser();
    first.saveCulture('en-US', 'en-US', '/', 86400);
    second.saveCulture('zh-CN', 'pt-BR', '/', 86400);
    first.saveCulture('pt-BR', 'pt-BR', '/', 86400);
    assert.equal(decodeURIComponent(first.cookies.get('.AspNetCore.Culture')), 'c=pt-BR|uic=pt-BR');
    assert.equal(decodeURIComponent(second.cookies.get('.AspNetCore.Culture')), 'c=pt-BR|uic=zh-CN');
    assert.equal(first.writes.some(value => value.includes('; secure')), false);
});
test('blocked persistence reports failure instead of pretending the choice was saved', () => {
    const current = browser('https:', true);
    assert.throws(() => current.saveCulture('en-US', 'pt-BR', '/', 86400), /could not save/);
});
test('invalid cookie arguments never write a value', () => {
    const current = browser();
    for (const args of [['en-US|uic=zh-CN', 'en-US', '/', 86400], ['en-US', '', '/', 86400],
        ['en-US', 'en-US', '/; secure', 86400], ['en-US', 'en-US', '/', 0], ['en-US', 'en-US', '/', 1.5]])
        assert.throws(() => current.saveCulture(...args), /Invalid/);
    assert.equal(current.writes.length, 0);
});
