import assert from 'node:assert/strict';
import { readFile } from 'node:fs/promises';
import test from 'node:test';

const css = (await readFile(new URL('../../src/Flourish.Blazor/Flourish.Blazor.Design/wwwroot/controls.css', import.meta.url), 'utf8'))
    .replace(/\/\*[\s\S]*?\*\//g, '');
function rule(selector, source = css) {
    const found = [...source.matchAll(/([^{}]+)\{([^{}]*)\}/g)].find(match => match[1].trim() === selector);
    assert.ok(found, 'Missing progress rule: ' + selector);
    return new Map(found[2].split(';').map(entry => {
        const delimiter = entry.indexOf(':');
        return [entry.slice(0, delimiter).trim(), entry.slice(delimiter + 1).trim()];
    }));
}
function keyframes(name) {
    const offset = css.indexOf('@keyframes ' + name + '{');
    assert.ok(offset >= 0, 'Missing animation: ' + name);
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
    assert.doesNotMatch(search, /\btransform\s*:/);
    assert.equal(rule('.f-progress-fill').get('transform'), undefined);
    assert.equal(fill.get('inset-inline-start'), '0');
    const end = rule('to', search).get('inset-inline-start');
    assert.equal(percent(fill.get('width')) + percent(end), 100,
        'The moving segment must reach the trailing edge without overflowing the rounded track.');
    assert.equal(rule('.f-progress-track').get('overflow'), 'hidden');
    assert.equal(rule('.f-progress-fill').get('border-radius'), 'inherit');
});

test('determinate progress retains solid fill and real width transitions without a shimmer layer', () => {
    const fill = rule('.f-progress-fill');
    assert.match(fill.get('transition'), /^width\s/);
    assert.equal(fill.get('background'), 'var(--f-primary)');
    assert.doesNotMatch(fill.get('background'), /gradient/);
    assert.doesNotMatch(css, /\.f-progress-fill::after|f-progress-flow/,
        'No common highlight pseudo-element or shimmer keyframes may affect either progress mode.');
    assert.equal(rule('.f-progress-track').get('height'), '40px',
        'The standard large progress bar remains separate from heading progress.');
});

test('reduced motion resets the moving segment to a static full-width visible indicator', () => {
    const reduced = rule('.f-progress-indeterminate .f-progress-fill', css.slice(css.indexOf('@media(prefers-reduced-motion:reduce){.f-progress-fill')));
    assert.equal(reduced.get('animation'), 'none');
    assert.equal(reduced.get('width'), '100%');
    assert.equal(reduced.get('inset-inline-start'), '0');
    assert.equal(reduced.get('opacity'), '.35');
});

test('single-color indeterminate progress keeps its original search animation without another animated layer', () => {
    const fill = rule('.f-progress-indeterminate .f-progress-fill');
    assert.match(fill.get('animation'), /^f-progress-search 1\.8s ease-in-out infinite alternate$/);
    assert.equal(rule('.f-progress-fill').get('background'), 'var(--f-primary)');
    assert.doesNotMatch(fill.get('background') ?? '', /gradient/);
    assert.doesNotMatch(css, /\.f-progress-fill::after|f-progress-flow/);
});