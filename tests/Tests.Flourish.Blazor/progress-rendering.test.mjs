import assert from 'node:assert/strict';
import { readFile } from 'node:fs/promises';
import test from 'node:test';

const css = (await readFile(new URL('../../src/Flourish.Blazor/Flourish.Blazor.Design/wwwroot/controls.css', import.meta.url), 'utf8'))
    .replace(/\/\*[\s\S]*?\*\//g, '');
function rule(selector, source = css) {
    const found = [...source.matchAll(/([^{}]+)\{([^{}]*)\}/g)].find(match => match[1].trim() === selector);
    assert.ok(found, `Missing progress rule: ${selector}`);
    return new Map(found[2].split(';').map(entry => {
        const delimiter = entry.indexOf(':');
        return [entry.slice(0, delimiter).trim(), entry.slice(delimiter + 1).trim()];
    }));
}
function keyframes(name) {
    const offset = css.indexOf(`@keyframes ${name}{`);
    assert.ok(offset >= 0, `Missing animation: ${name}`);
    const start = css.indexOf('{', offset);
    let depth = 1, end = start + 1;
    while (depth > 0 && end < css.length) {
        if (css[end] === '{') depth++;
        else if (css[end] === '}') depth--;
        end++;
    }
    return css.slice(start + 1, end - 1);
}
const percent = value => {
    assert.match(value ?? '', /^\d+(?:\.\d+)?%$/);
    return Number.parseFloat(value);
};

test('indeterminate progress paints one bounded rounded segment without nested transformed clipping', () => {
    const fill = rule('.f-progress-indeterminate .f-progress-fill');
    const search = keyframes('f-progress-search');
    // The former two nested transform animations could leave fragmented clipped
    // compositor paint. Move this small indicator through layout instead.
    for (const body of [search, keyframes('f-progress-flow')])
        assert.doesNotMatch(body, /\btransform\s*:/);
    for (const selector of ['.f-progress-fill', '.f-progress-fill::after'])
        assert.equal(rule(selector).get('transform'), undefined);
    assert.equal(fill.get('inset-inline-start'), '0');
    const end = rule('to', search).get('inset-inline-start');
    assert.equal(percent(fill.get('width')) + percent(end), 100,
        'The moving segment must reach the trailing edge without overflowing the rounded track.');
    assert.equal(rule('.f-progress-track').get('overflow'), 'hidden');
    assert.equal(rule('.f-progress-fill').get('border-radius'), 'inherit');
});

test('determinate progress retains real width transitions while its stationary highlight moves only its paint', () => {
    assert.match(rule('.f-progress-fill').get('transition'), /^width\s/);
    const highlight = rule('.f-progress-fill::after');
    assert.equal(highlight.get('position'), 'absolute');
    assert.equal(highlight.get('inset'), '0');
    assert.equal(highlight.get('background-repeat'), 'no-repeat');
    assert.match(highlight.get('background-position'), /^-?\d+%\s+0$/);
    assert.match(rule('to', keyframes('f-progress-flow')).get('background-position'), /^-?\d+%\s+0$/);
    assert.equal(rule('.f-progress-track').get('height'), '40px',
        'This fix preserves the standard large progress bar rather than shrinking it into heading progress.');
});

test('reduced motion resets the moving segment to a static full-width visible indicator', () => {
    const reduced = rule('.f-progress-indeterminate .f-progress-fill', css.slice(css.indexOf('@media(prefers-reduced-motion:reduce){.f-progress-fill')));
    assert.equal(reduced.get('animation'), 'none');
    assert.equal(reduced.get('width'), '100%');
    assert.equal(reduced.get('inset-inline-start'), '0');
    assert.equal(reduced.get('opacity'), '.35');
});
