const headers = new Map();
const surfaceSelector = '.content-surface,.navigation-surface';
const pixels = value => Number.parseFloat(value) || 0;
const clamp = (value, minimum, maximum) => Math.max(minimum, Math.min(maximum, value));

// Geometry is taken from the actual page, including navigation rails and its
// scrollbar. The wings never change the page's selected container width.
export function resolveHeaderLayout(header, content, startWidth, endWidth, direction = 'ltr', centerWidth = 0) {
    if (!(header.width > 0) || !(content.right > content.left)) return null;
    const left = clamp(content.left - header.left, 0, header.width);
    const right = clamp(header.right - content.right, 0, header.width - left);
    const start = direction === 'rtl' ? right : left;
    const end = direction === 'rtl' ? left : right;
    // A wide business page can leave almost no outside gutter. Only the menu's
    // intrinsic controls, not that page-wide center track, must fit beside wings.
    if (startWidth + centerWidth + endWidth > header.width + 0.5)
        return { start, end, mode:'stacked' };
    let fittedStart = Math.max(start, startWidth), fittedEnd = Math.max(end, endWidth);
    const excess = fittedStart + centerWidth + fittedEnd - header.width;
    if (excess > 0) {
        const startSpace = fittedStart - startWidth, endSpace = fittedEnd - endWidth;
        const space = startSpace + endSpace;
        if (space > 0) {
            fittedStart -= excess * startSpace / space;
            fittedEnd -= excess * endSpace / space;
        }
    }
    const exact = Math.abs(fittedStart - start) < 0.5 && Math.abs(fittedEnd - end) < 0.5;
    return { start:fittedStart, end:fittedEnd, mode:exact ? 'aligned' : 'fitted' };
}

function stageFor(header) {
    const surface = header.closest(surfaceSelector);
    if (!surface) return null;
    const stage = [...surface.querySelectorAll('.content-stage,.application-stage')]
        .find(node => node.closest(surfaceSelector) === surface);
    return stage && !stage.contains(header) ? { surface, stage } : null;
}
function targetFor(stage, surface) {
    // The outer page owns the boundary; a nested form or card cannot resize chrome.
    const own = selector => [...stage.querySelectorAll(selector)]
        .find(node => node.closest(surfaceSelector) === surface && node.getBoundingClientRect().width > 0);
    return own('.f-page-body') ?? own('.f-content-container') ?? stage;
}
function contentBounds(target) {
    const bounds = target.getBoundingClientRect();
    const style = getComputedStyle(target);
    let left = bounds.left + pixels(style.borderLeftWidth) + pixels(style.paddingLeft);
    let right = bounds.right - pixels(style.borderRightWidth) - pixels(style.paddingRight);
    if (target.classList.contains('f-page-body')) {
        const child = [...target.children].find(node => node.getBoundingClientRect().width > 0);
        if (child) {
            const childStyle = getComputedStyle(child);
            if (child.classList.contains('f-page-heading')) {
                const heading = child.getBoundingClientRect();
                left = heading.left + pixels(childStyle.borderLeftWidth) + pixels(childStyle.paddingLeft);
                right = heading.right - pixels(childStyle.borderRightWidth) - pixels(childStyle.paddingRight);
            } else {
                left += pixels(childStyle.marginLeft);
                right -= pixels(childStyle.marginRight);
            }
        }
    }
    return { left, right };
}
function intrinsicWidth(element, parentStyle) {
    if (!element) return 0;
    const style = getComputedStyle(element);
    const children = [...element.children].filter(node => {
        const position = getComputedStyle(node).position;
        return !node.matches('.f-menu-panel,[data-f-popup-panel]') && !['absolute','fixed'].includes(position)
            && node.getBoundingClientRect().width > 0;
    });
    // Ignore stretched layout wrappers, but retain a real control's own box.
    // An icon-only Button is wider than its glyph even with zero padding.
    const control = element.matches('button,a,input,select,textarea,summary,.f-button,.f-menu-trigger,.service-menu-trigger,.shell-header-identity');
    const width = !children.length || control ? element.getBoundingClientRect().width : 0;
    // Keep nonshrinking flex constraints even when a previous narrow track has
    // constrained the measured box. Read CSS rather than duplicating target sizes.
    const basis = style.flexShrink === '0' && parentStyle?.display.includes('flex')
        && !parentStyle.flexDirection.startsWith('column') && style.flexBasis?.endsWith('px') ? pixels(style.flexBasis) : 0;
    const minimum = Math.max(pixels(style.minWidth), basis);
    if (!children.length) return Math.max(width, minimum, pixels(element.scrollWidth));
    const sizes = children.map(node => {
        const childStyle = getComputedStyle(node);
        return intrinsicWidth(node, style) + pixels(childStyle.marginLeft) + pixels(childStyle.marginRight);
    });
    const row = style.display.includes('flex') ? !style.flexDirection.startsWith('column') : style.display.startsWith('inline');
    const content = row ? sizes.reduce((total, size) => total + size, 0)
        + Math.max(0, sizes.length - 1) * pixels(style.columnGap) : Math.max(...sizes);
    return Math.max(width, minimum, content + pixels(style.paddingLeft) + pixels(style.paddingRight)
        + pixels(style.borderLeftWidth) + pixels(style.borderRightWidth));
}
function setProperty(header, name, value) {
    const text = `${Math.round(value * 100) / 100}px`;
    if (header.style.getPropertyValue(name) !== text) header.style.setProperty(name, text);
}
function clearFooterLayout(footer) {
    footer.removeAttribute('data-presentation-footer-layout');
    footer.style.removeProperty('--f-presentation-footer-content-start');
    footer.style.removeProperty('--f-presentation-footer-content-end');
}
function update(state) {
    state.frame = 0;
    const { header } = state;
    if (!header.isConnected) { disposeShellHeader(header); return; }
    const context = stageFor(header);
    if (!context) { disposeShellHeader(header); return; }
    const target = targetFor(context.stage, context.surface);
    const footers = new Set([...context.surface.querySelectorAll('.f-presentation-footer')]
        .filter(node => node.closest(surfaceSelector) === context.surface && !context.stage.contains(node)));
    for (const footer of state.footers) if (!footers.has(footer)) {
        clearFooterLayout(footer);
        state.footerStyles.delete(footer);
    }
    state.footers = footers;
    const start = header.querySelector(':scope > .shell-header-start');
    const center = header.querySelector(':scope > .shell-header-center');
    const end = header.querySelector(':scope > .shell-header-actions');
    const controlsFor = container => container ? [...container.querySelectorAll('.f-button,button,select,.service-menu-trigger,.shell-header-identity strong')]
        .filter(node => !node.closest('.f-menu-panel,[data-f-popup-panel],.f-dialog')) : [];
    const observed = new Set([header, context.stage, target, start, center, end, ...controlsFor(center), ...controlsFor(end), ...footers,
        ...header.querySelectorAll(':scope > .shell-header-start .shell-header-title > *')].filter(Boolean));
    for (const node of state.observed) if (!observed.has(node)) state.resize?.unobserve(node);
    for (const node of observed) if (!state.observed.has(node)) state.resize?.observe(node);
    state.observed = observed;
    if (state.surface !== context.surface) {
        state.mutation?.disconnect();
        state.surface = context.surface;
        state.mutation?.observe(context.surface, { attributes:true, attributeFilter:['class','style','hidden','data-f-native-select-ready'], childList:true, subtree:true });
    }
    const bounds = contentBounds(target);
    const layout = resolveHeaderLayout(header.getBoundingClientRect(), bounds,
        intrinsicWidth(start), intrinsicWidth(end), getComputedStyle(header).direction, intrinsicWidth(center));
    if (!layout) return;
    setProperty(header, '--f-shell-content-start', layout.start);
    setProperty(header, '--f-shell-content-end', layout.end);
    if (header.getAttribute('data-shell-header-layout') !== layout.mode)
        header.setAttribute('data-shell-header-layout', layout.mode);
    // Foreground footer copy shares the same measured edges; the decorative
    // wordmark remains outside its independently padded full-width container.
    for (const footer of footers) {
        const insets = resolveHeaderLayout(footer.getBoundingClientRect(), bounds, 0, 0, getComputedStyle(footer).direction);
        if (!insets) continue;
        setProperty(footer, '--f-presentation-footer-content-start', insets.start);
        setProperty(footer, '--f-presentation-footer-content-end', insets.end);
        footer.setAttribute('data-presentation-footer-layout', 'aligned');
        state.footerStyles.set(footer, footer.getAttribute('style'));
    }
}
function schedule(state) {
    if (!state.frame) state.frame = requestAnimationFrame(() => update(state));
}

// Called by the existing static/interactive document initializer. All observer
// writes are idempotent; our own header style writes do not schedule another pass.
export function initializeShellHeaders(scope = document) {
    for (const [header] of headers) if (!header.isConnected) disposeShellHeader(header);
    const found = [...scope.querySelectorAll('.shell-header')];
    if (scope.matches?.('.shell-header')) found.unshift(scope);
    for (const header of found) {
        let state = headers.get(header);
        if (!state) {
            state = { header, observed:new Set(), footers:new Set(), footerStyles:new Map(), frame:0, surface:null };
            state.resize = new ResizeObserver(() => schedule(state));
            state.mutation = new MutationObserver(records => {
                if (records.some(record => record.target !== header && !(record.attributeName === 'style'
                    && state.footers.has(record.target)
                    && state.footerStyles.get(record.target) === record.target.getAttribute('style')))) schedule(state);
            });
            state.onResize = () => schedule(state);
            window.addEventListener('resize', state.onResize);
            headers.set(header, state);
        }
        schedule(state);
    }
}
export function disposeShellHeader(header) {
    const state = headers.get(header);
    if (!state) return;
    if (state.frame) cancelAnimationFrame(state.frame);
    state.resize?.disconnect();
    state.mutation?.disconnect();
    for (const footer of state.footers) clearFooterLayout(footer);
    window.removeEventListener('resize', state.onResize);
    header.removeAttribute('data-shell-header-layout');
    header.style.removeProperty('--f-shell-content-start');
    header.style.removeProperty('--f-shell-content-end');
    headers.delete(header);
}
