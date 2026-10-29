// Scoped DOM lifetimes adapted from the source tooltip, menu, modal and input behaviors.
import { rememberInvoker, resolveInvoker } from './primitives/interaction-origin.js';
const menus = new WeakMap();
const dialogs = new WeakMap();
const inputs = new WeakMap();
const accessForms = new WeakMap();
const tutorialBoards = new WeakMap();
let activeMenu = null;
const weakPopup = panel => panel.getAttribute('data-f-popup-strength') !== 'strong';
const focusableSelector = 'a[href],button:not([disabled]),input:not([disabled]),select:not([disabled]),textarea:not([disabled]),[tabindex]:not([tabindex="-1"])';
const visible = element => element.getClientRects().length > 0 && !element.closest('[hidden],[inert]');

export function focusFirstInvalid(root) {
    const form = root?.closest('form') ?? root;
    const target = [...(form?.querySelectorAll('input.invalid,select.invalid,textarea.invalid,[aria-invalid="true"]') ?? [])].find(element => visible(element) && !element.disabled && element.matches(focusableSelector));
    if (!target) return false;
    target.scrollIntoView({block:'center',behavior:'auto'}); target.focus({preventScroll:true}); return true;
}

export function attachTextSelection(input) {
    if (!input || inputs.has(input)) return;
    let frame = null;
    const focus = () => {
        if (!(input instanceof HTMLInputElement) || !['text','email','tel','url','number'].includes(input.type)
            || input.disabled || input.readOnly || !input.value || input.dataset.inputMask !== undefined
            || input.dataset.preserveSelection === 'true') return;
        frame = requestAnimationFrame(() => { if (document.activeElement === input) input.select(); });
    };
    input.addEventListener('focus', focus);
    inputs.set(input, () => { cancelAnimationFrame(frame); input.removeEventListener('focus', focus); });
}
export function detachTextSelection(input) { inputs.get(input)?.(); inputs.delete(input); }

/** Match the visible input-to-input gap before actions while retaining visible field labels. */
export function attachAccessFormSpacing(root) {
    if (!root?.isConnected || accessForms.has(root)) return;
    const managed = new Map();
    let observed = [];
    const resize = new ResizeObserver(update);
    const mutation = new MutationObserver(update);
    function update() {
        if (!root.isConnected) { detachAccessFormSpacing(root); return; }
        const targets = new Map();
        const labels = new Set();
        const owns = element => element.closest('.f-access-form-surface') === root;
        function reserve(action, field) {
            const label = field?.querySelector(':scope > .f-field-label');
            if (!label || !owns(field)) return;
            labels.add(label);
            const height = label.getClientRects().length ? label.getBoundingClientRect().height : 0;
            const gap = Number.parseFloat(getComputedStyle(field).rowGap) || 0;
            targets.set(action, `${height > 0 ? height + gap : 0}px`);
        }
        for (const layout of root.querySelectorAll('.f-form-layout')) {
            if (!owns(layout)) continue;
            for (const action of layout.children)
                if (action.matches('.f-button,.f-inline-actions') && action.previousElementSibling?.matches('.f-field'))
                    reserve(action, action.previousElementSibling);
        }
        for (const actions of root.querySelectorAll(':scope > .f-access-form-actions')) {
            const preceding = [...root.querySelectorAll('.f-field')].filter(field => owns(field) && visible(field)
                && (field.compareDocumentPosition(actions) & Node.DOCUMENT_POSITION_FOLLOWING));
            reserve(actions, preceding.at(-1));
        }
        for (const [action, original] of managed) {
            if (targets.has(action)) continue;
            if (original) action.style.setProperty('--f-access-action-label-gap', original);
            else action.style.removeProperty('--f-access-action-label-gap');
            managed.delete(action);
        }
        for (const [action, value] of targets) {
            if (!managed.has(action)) managed.set(action, action.style.getPropertyValue('--f-access-action-label-gap'));
            if (action.style.getPropertyValue('--f-access-action-label-gap') !== value)
                action.style.setProperty('--f-access-action-label-gap', value);
        }
        const nextObserved = [root, ...labels];
        if (nextObserved.length !== observed.length || nextObserved.some((node, index) => node !== observed[index])) {
            resize.disconnect(); nextObserved.forEach(node => resize.observe(node)); observed = nextObserved;
        }
    }
    accessForms.set(root, () => {
        resize.disconnect(); mutation.disconnect();
        for (const [action, original] of managed)
            if (original) action.style.setProperty('--f-access-action-label-gap', original);
            else action.style.removeProperty('--f-access-action-label-gap');
    });
    mutation.observe(root, { childList:true, subtree:true, characterData:true, attributes:true, attributeFilter:['hidden','class'] });
    update();
}
export function detachAccessFormSpacing(root) { accessForms.get(root)?.(); accessForms.delete(root); }

function menuUnavailable(element) {
    return !element || element.disabled || element.matches(':disabled')
        || element.closest('[disabled],[aria-disabled="true"],[hidden],[inert]') !== null;
}
function menuItems(state) { return [...state.panel.querySelectorAll(state.itemSelector)].filter(element => visible(element) && !menuUnavailable(element)); }
const popupTriggerWidthProperty = '--f-popup-trigger-width';
function setPopupTriggerWidth(element, width, maximumWidth = 'none') {
    if (!Number.isFinite(width) || width < 0) return;
    const viewport = (element.ownerDocument ?? document).documentElement.clientWidth;
    width = Math.min(width,Math.max(0,viewport - 24));
    // Preserve an existing explicit maximum as well as the CSS viewport bound.
    // Only a minimum changes; max-content keeps the live longest-option width.
    const value = maximumWidth && maximumWidth !== 'none' ? `min(${width}px, ${maximumWidth})` : `${width}px`;
    if (element.style.getPropertyValue(popupTriggerWidthProperty) !== value)
        element.style.setProperty(popupTriggerWidthProperty, value);
}
function copyTheme(trigger, panel) {
    const style = getComputedStyle(trigger);
    const chrome = trigger.closest?.('.shell-header,.f-titlebar');
    // A header can invert only its button's foreground/surface. Keep scoped
    // brands/fonts from the trigger; use the sibling popup's own surface roles
    // only for that chrome boundary. Detached previews keep their source theme.
    const roleSource = chrome?.contains(panel) ? panel : chrome ? trigger.parentElement : null;
    const surface = roleSource ? getComputedStyle(roleSource) : null;
    // Preserve the trigger's scoped surface roles on native top-layer popups.
    const names = new Set(['font-family','font-size','font-weight','line-height','color-scheme']);
    for (let index = 0; index < style.length; index++) {
        const name = style[index];
        if (name?.startsWith('--f-') && !['--f-popup-align',popupTriggerWidthProperty].includes(name)) names.add(name);
    }
    const saved = [];
    for (const name of names) {
        // A popup is a surface even when its trigger lives on dark chrome.
        const own = ['--f-text','--f-surface','--f-accent-text'].includes(name) ? surface?.getPropertyValue(name) : '';
        const value = ['--f-target-preview','--f-row-hover'].includes(name)
            ? style.getPropertyValue('--f-surface-preview') || style.getPropertyValue(name)
            : ['--f-target-click','--f-row-active'].includes(name)
            ? style.getPropertyValue('--f-surface-click') || style.getPropertyValue(name)
            : own || style.getPropertyValue(name);
        if (!value) continue;
        saved.push([name,panel.style.getPropertyValue(name),panel.style.getPropertyPriority(name)]);
        panel.style.setProperty(name,value);
    }
    return () => saved.forEach(([name,value,priority]) => value ? panel.style.setProperty(name,value,priority) : panel.style.removeProperty(name));
}
export function positionPopup(state) {
    if (!state.open) return;
    // Hover regions can fill an entire top-bar slot; both modes anchor to the actual control.
    const trigger = state.trigger.getBoundingClientRect();
    const panel = state.panel;
    if (state.maximumHeight) panel.style.setProperty('max-height',state.maximumHeight,state.maximumHeightPriority);
    else panel.style.removeProperty('max-height');
    const style = getComputedStyle(panel), maximumHeight = style.maxHeight;
    if (panel.matches('.f-menu-panel,.selection-panel,.search-autocomplete-options'))
        setPopupTriggerWidth(panel, trigger.width, style.maxWidth);
    const width = document.documentElement.clientWidth;
    const alignment = popupAlignment(trigger, width);
    panel.setAttribute('data-f-popup-align',alignment);
    const menu = panel.getBoundingClientRect();
    const height = window.innerHeight;
    const edge = alignment === 'left' ? trigger.left : trigger.right - menu.width;
    panel.style.left = `${Math.max(12, Math.min(edge, width - menu.width - 12))}px`;
    // Hover surfaces meet their trigger region so crossing into the menu has no dead gap.
    const gap = state.openOnHover ? 0 : 6;
    const below = Math.max(0,height - trigger.bottom - gap - 12);
    const above = Math.max(0,trigger.top - gap - 12);
    const openAbove = menu.height > below && above > below;
    const available = openAbove ? above : below;
    // A long panel scrolls on its chosen side instead of overlapping the control or leaving the viewport.
    panel.style.setProperty('max-height',maximumHeight && maximumHeight !== 'none'
        ? `min(${available}px, ${maximumHeight})` : `${available}px`,state.maximumHeightPriority ?? '');
    const panelHeight = Math.min(panel.getBoundingClientRect().height,available);
    const top = openAbove ? trigger.top - panelHeight - gap : trigger.bottom + gap;
    panel.style.top = `${Math.max(12,Math.min(top,height - panelHeight - 12))}px`;
}

function positionMenu(state) { positionPopup(state); }
function popupAlignment(trigger, viewportWidth) { return (trigger.left + trigger.right) / 2 <= viewportWidth / 2 ? 'left' : 'right'; }

const nativePickerDocuments = new WeakMap();
const measuredNativePickers = new WeakSet();
const pickerMeasureProperties = [
    'font-family','font-size','font-style','font-weight','font-stretch','font-kerning','font-feature-settings',
    'font-variation-settings','line-height','letter-spacing','word-spacing','text-transform','text-indent','white-space',
    'box-sizing','padding-top','padding-right','padding-bottom','padding-left',
    'border-top-width','border-right-width','border-bottom-width','border-left-width',
    'border-top-style','border-right-style','border-bottom-style','border-left-style',
    'column-gap','row-gap','grid-template-columns','align-items'
];
function copyPickerMetrics(source, target, properties = pickerMeasureProperties) {
    for (const name of properties) {
        const value = source.getPropertyValue(name);
        if (value) target.style.setProperty(name,value);
    }
}
function measureNativePicker(select) {
    const owner = select.ownerDocument, view = owner.defaultView;
    if (!(view?.CSS ?? globalThis.CSS)?.supports('appearance','base-select') || select.matches(':open')) return;
    const styles = (node, pseudo) => view.getComputedStyle(node,pseudo);
    const probe = owner.createElement('div');
    probe.setAttribute('aria-hidden','true'); probe.inert = true;
    copyPickerMetrics(styles(select,'::picker(select)'),probe);
    Object.assign(probe.style,{position:'fixed',left:'-100000px',top:'0',display:'block',width:'max-content',
        minWidth:'0',maxWidth:'none',minInlineSize:'0',maxInlineSize:'none',margin:'0',height:'auto',maxHeight:'none',overflow:'visible',visibility:'hidden',pointerEvents:'none'});
    function cloneContent(node) {
        const clone = node.cloneNode(false);
        if (node.nodeType === 1) {
            clone.removeAttribute('id');
            // Retain actual rich-option content metrics even outside its themed ancestor tree.
            copyPickerMetrics(styles(node),clone,[...pickerMeasureProperties,'display','width','height',
                'min-width','max-width','margin-top','margin-right','margin-bottom','margin-left','flex','gap']);
        }
        for (const child of node.childNodes) clone.appendChild(cloneContent(child));
        return clone;
    }
    function appendOptions(source, target) {
        for (const child of source.children) {
            if (!['OPTION','OPTGROUP'].includes(child.tagName)) continue;
            const computed = styles(child);
            if (computed.display === 'none' || child.hidden) continue;
            const row = owner.createElement('div');
            copyPickerMetrics(computed,row);
            if (child.tagName === 'OPTGROUP') {
                const heading = owner.createElement('div');
                copyPickerMetrics(styles(child,'::before'),heading);
                heading.textContent = child.label;
                row.appendChild(heading); appendOptions(child,row);
            } else {
                row.style.display = 'grid';
                const marker = owner.createElement('span'), label = owner.createElement('span');
                copyPickerMetrics(styles(child,'::checkmark'),marker,[...pickerMeasureProperties,'width','height']);
                row.style.gridTemplateColumns = styles(child,'::checkmark').getPropertyValue('width') + ' max-content';
                marker.style.gridColumn = '1'; marker.style.gridRow = '1';
                label.style.gridColumn = '2'; label.style.gridRow = '1'; label.style.minWidth = '0';
                if (child.getAttribute('label')) label.textContent = child.label;
                else for (const content of child.childNodes) label.appendChild(cloneContent(content));
                row.appendChild(marker); row.appendChild(label);
            }
            target.appendChild(row);
        }
    }
    appendOptions(select,probe);
    if (!probe.childElementCount) return;
    // Measure a private inert copy before the native opening default, never the visible picker.
    try {
        (owner.body ?? owner.documentElement).appendChild(probe);
        const width = Math.ceil(probe.getBoundingClientRect().width);
        if (Number.isFinite(width) && width > 0) {
            const value = `${width}px`;
            if (select.style.getPropertyValue('--f-picker-inline-size') !== value)
                select.style.setProperty('--f-picker-inline-size',value);
        }
    } finally { probe.remove(); }
}
/** Select keeps its native picker; settle its intrinsic width and edge before the opening default action. */
export function initializeNativePickers(root = document) {
    const owner = root.ownerDocument ?? root;
    let state = nativePickerDocuments.get(owner);
    const update = (select, opening = false) => {
        if (select?.tagName !== 'SELECT' || !select.isConnected || !select.getClientRects().length) return;
        const bounds = select.getBoundingClientRect();
        const alignment = popupAlignment(bounds,owner.documentElement.clientWidth);
        if (select.getAttribute('data-f-popup-align') !== alignment) select.setAttribute('data-f-popup-align',alignment);
        const view = owner.defaultView ?? window;
        if ((view.CSS ?? globalThis.CSS)?.supports('appearance','base-select'))
            setPopupTriggerWidth(select, bounds.width, view.getComputedStyle(select,'::picker(select)').maxWidth);
        if (state.resize && !state.observed.has(select)) { state.observed.add(select); state.resize.observe(select); }
        if (opening || !measuredNativePickers.has(select)) {
            // A removed measurement probe may trigger the host's DOM discovery observer again.
            measuredNativePickers.add(select); measureNativePicker(select);
        }
    };
    const synchronize = target => {
        if (target.tagName === 'SELECT' && target.matches('.f-select')) update(target);
        for (const select of target.querySelectorAll('select.f-select')) update(select);
    };
    if (!state) {
        state = { observed:new Set(), resize:null };
        if (typeof ResizeObserver === 'function') state.resize = new ResizeObserver(entries => {
            for (const entry of entries) {
                if (!entry.target.isConnected) { state.resize.unobserve(entry.target); state.observed.delete(entry.target); }
                else update(entry.target);
            }
        });
        nativePickerDocuments.set(owner,state);
        const beforeOpen = event => update(event.target?.closest?.('select.f-select'),true);
        for (const type of ['pointerdown','focusin','keydown']) owner.addEventListener(type,beforeOpen,true);
        (owner.defaultView ?? window).addEventListener('resize',()=>synchronize(owner));
        owner.addEventListener('scroll',()=>synchronize(owner),true);
    }
    for (const select of state.observed) if (!select.isConnected) { state.resize.unobserve(select); state.observed.delete(select); }
    synchronize(root);
}

const nativeSelectPopups = new WeakMap();
/** Opt-in hover selection uses the shared menu; the original select still owns form and change semantics. */
export function initializeNativeSelectPopups(root = document) {
    const owners = [...(root.matches?.('[data-f-native-select-popup]') ? [root] : []),
        ...root.querySelectorAll('[data-f-native-select-popup]')];
    for (const owner of owners) {
        const select = owner.querySelector(':scope > select');
        const menu = owner.querySelector('.f-native-select-actions > .f-action-menu');
        const trigger = menu?.querySelector(':scope > summary');
        const panel = menu?.querySelector(':scope > [data-f-popup-panel]');
        const options = panel?.querySelector('[data-f-native-select-options]');
        if (!select || select.multiple || Number(select.size) > 1 || !trigger || !panel || !options || typeof panel.showPopover !== 'function') continue;
        const existing = nativeSelectPopups.get(owner);
        if (existing && (existing.select !== select || existing.trigger !== trigger || existing.panel !== panel)) detachNativeSelectPopup(owner);
        else if (existing) { existing.refresh(); continue; }
        const cleanup = [], view = select.ownerDocument.defaultView;
        const on = (target, type, callback, capture = false) => {
            target.addEventListener(type,callback,capture);
            cleanup.push(()=>target.removeEventListener(type,callback,capture));
        };
        const labels = () => [...(select.labels ?? [])];
        const caption = trigger.querySelector('[data-f-native-select-caption]');
        let search = '', searchedAt = 0, previousSignature = null;
        const disabled = option => select.disabled || select.matches(':disabled') || option.disabled || option.parentElement?.disabled;
        const refresh = () => {
            if (!owner.isConnected || !select.isConnected) { detachNativeSelectPopup(owner); return; }
            const label = select.getAttribute('aria-label') || labels().map(item=>item.textContent.trim()).join(' ');
            const signature = JSON.stringify([select.disabled,select.matches(':disabled'),select.value,label,
                select.getAttribute('aria-labelledby'),select.getAttribute('aria-describedby'),select.getAttribute('aria-invalid'),
                [...select.options].map(option=>[option.value,option.label,option.selected,!!disabled(option),option.hidden,option.parentElement?.hidden])]);
            if (signature === previousSignature) return;
            previousSignature = signature;
            const oldFocus = options.contains(select.ownerDocument.activeElement)
                ? select.ownerDocument.activeElement.getAttribute('data-f-native-select-value') : null;
            const fragment = select.ownerDocument.createDocumentFragment();
            for (const option of select.options) {
                if (option.hidden || option.parentElement?.hidden) continue;
                const choice = select.ownerDocument.createElement('button');
                choice.type = 'button'; choice.className = 'f-native-select-option f-dropdown-item';
                choice.setAttribute('role','menuitemradio'); choice.setAttribute('aria-checked',String(option.selected));
                choice.setAttribute('data-f-native-select-value',option.value);
                choice.setAttribute('data-f-native-select-text',option.label);
                choice.disabled = !!disabled(option);
                const marker = select.ownerDocument.createElement('span');
                marker.className = 'f-select-checkmark'; marker.setAttribute('aria-hidden','true'); marker.textContent = option.selected ? '✓' : '';
                const text = select.ownerDocument.createElement('span'); text.textContent = option.label;
                choice.append(marker,text); fragment.appendChild(choice);
            }
            options.replaceChildren(fragment);
            if (caption) caption.textContent = select.selectedOptions[0]?.label ?? '';
            if (label) { trigger.setAttribute('aria-label',label); panel.setAttribute('aria-label',label); }
            const labelledBy = select.getAttribute('aria-labelledby'), description = select.getAttribute('aria-describedby');
            for (const [name,value] of [['aria-labelledby',labelledBy],['aria-describedby',description],['aria-invalid',select.getAttribute('aria-invalid')]])
                value ? trigger.setAttribute(name,value) : trigger.removeAttribute(name);
            trigger.setAttribute('tabindex','0');
            trigger.setAttribute('aria-disabled',String(select.disabled || select.matches(':disabled')));
            if (select.disabled || select.matches(':disabled')) closeMenu(trigger,panel,false);
            if (oldFocus !== null) [...options.children].find(item=>item.getAttribute('data-f-native-select-value') === oldFocus)?.focus({preventScroll:true});
        };
        const initial = select.getAttribute('value');
        if (initial !== null && [...select.options].some(option=>option.value === initial)) select.value = initial;
        on(options,'click',event=>{
            const choice = event.target.closest?.('button[data-f-native-select-value]');
            if (!choice || !options.contains(choice) || menuUnavailable(select) || choice.disabled) return;
            event.preventDefault();
            const value = choice.getAttribute('data-f-native-select-value');
            const option = [...select.options].find(item=>item.value === value && !disabled(item));
            if (!option) return;
            rememberInvoker(trigger);
            const changed = select.value !== value;
            select.value = value;
            closeMenu(trigger,panel,true);
            refresh();
            if (changed) select.dispatchEvent(new (view?.Event ?? Event)('change',{bubbles:true}));
        });
        on(select,'change',refresh);
        on(select,'invalid',event=>{
            event.preventDefault(); trigger.setAttribute('aria-invalid','true');
            trigger.focus({preventScroll:true});
        });
        for (const label of labels()) on(label,'click',event=>{
            if (!owner.hasAttribute('data-f-native-select-ready') || menuUnavailable(select)) return;
            event.preventDefault(); toggleMenu(trigger,panel,true);
        });
        on(menu,'keydown',event=>{
            if (event.key.length !== 1 || event.ctrlKey || event.altKey || event.metaKey || event.key === ' ' || menuUnavailable(select)) return;
            const now = Date.now(); search = now - searchedAt < 700 ? search + event.key : event.key; searchedAt = now;
            const text = search.toLocaleLowerCase(select.ownerDocument.documentElement.lang || undefined);
            const choices = [...options.children].filter(item=>!item.disabled);
            const match = choices.find(item=>item.getAttribute('data-f-native-select-text').toLocaleLowerCase(select.ownerDocument.documentElement.lang || undefined).startsWith(text));
            if (match) { event.preventDefault(); if (!menus.get(panel)?.open) toggleMenu(trigger,panel,false); match.focus({preventScroll:true}); }
        });
        if (select.form) on(select.form,'reset',()=>queueMicrotask(refresh));
        const observer = new MutationObserver(refresh);
        observer.observe(select,{childList:true,subtree:true,characterData:true,attributes:true,
            attributeFilter:['disabled','selected','value','label','hidden','aria-label','aria-labelledby','aria-describedby','aria-invalid']});
        cleanup.push(()=>observer.disconnect());
        nativeSelectPopups.set(owner,{select,trigger,panel,refresh,cleanup});
        menus.get(panel)?.cleanup.push(()=>detachNativeSelectPopup(owner));
        refresh(); owner.setAttribute('data-f-native-select-ready','true');
    }
}
export function detachNativeSelectPopup(owner) {
    const state = nativeSelectPopups.get(owner); if (!state) return;
    state.cleanup.forEach(remove=>remove()); nativeSelectPopups.delete(owner);
    owner.removeAttribute('data-f-native-select-ready');
}

/** One nonmodal tutorial entry: native top-layer board plus a passive hover/focus overview. */
export function synchronizeTutorialBoard(trigger, preview, board, open, reference) {
    if (!trigger?.isConnected || !preview?.isConnected || !board?.isConnected) return;
    if (typeof board.showPopover !== 'function' || typeof preview.showPopover !== 'function')
        throw new Error('TutorialBoard requires the native Popover API.');
    let state = tutorialBoards.get(board);
    if (!state) {
        state = {trigger,preview,board,reference,requestedOpen:false,wasOpen:false,hover:false,focus:false,timer:null,
            panel:preview,open:false,openOnHover:true,maximumHeight:preview.style.getPropertyValue('max-height'),
            maximumHeightPriority:preview.style.getPropertyPriority('max-height')};
        tutorialBoards.set(board,state);
        const hidePreview = () => {
            clearTimeout(state.timer); state.timer = null;
            if (preview.matches(':popover-open')) preview.hidePopover();
            state.open = false; trigger.removeAttribute('aria-describedby');
            state.restorePreviewTheme?.(); state.restorePreviewTheme = null;
        };
        const showPreview = () => {
            clearTimeout(state.timer); state.timer = null;
            if (trigger.disabled || state.requestedOpen || board.matches(':popover-open')) return;
            state.restorePreviewTheme?.(); state.restorePreviewTheme = copyTheme(trigger,preview);
            if (!preview.matches(':popover-open')) preview.showPopover();
            state.open = true; trigger.setAttribute('aria-describedby',preview.id); positionMenu(state);
        };
        const scheduleHide = () => {
            clearTimeout(state.timer);
            state.timer = setTimeout(() => { if (!state.hover && !state.focus) hidePreview(); },90);
        };
        const enter = () => { state.hover = true; showPreview(); };
        const leave = () => { state.hover = false; scheduleHide(); };
        const focus = () => { state.focus = true; showPreview(); };
        const blur = () => { state.focus = false; scheduleHide(); };
        const previewEscape = event => {
            if (event.key === 'Escape' && state.open) { hidePreview(); event.preventDefault(); }
        };
        const toggle = event => {
            if (event.newState === 'closed' && state.requestedOpen && !weakPopup(board)) {
                board.showPopover(); return;
            }
            if (event.newState === 'open') { state.wasOpen = true; hidePreview(); return; }
            if (!state.wasOpen) return;
            state.wasOpen = false; trigger.setAttribute('aria-expanded','false');
            state.restoreBoardTheme?.(); state.restoreBoardTheme = null;
            // Restore only focus owned by this board, never steal an outside-click target's focus.
            if (board.contains(document.activeElement) && trigger.isConnected) trigger.focus({preventScroll:true});
            if (state.requestedOpen) {
                state.requestedOpen = false;
                Promise.resolve(state.reference?.invokeMethodAsync('RequestCloseAsync')).catch(() => {});
            }
        };
        const reposition = () => { if (state.open) positionMenu(state); };
        const observer = new MutationObserver(() => {
            if (!trigger.isConnected || !preview.isConnected || !board.isConnected) detachTutorialBoard(trigger,preview,board);
        });
        trigger.addEventListener('pointerenter',enter); trigger.addEventListener('pointerleave',leave);
        preview.addEventListener('pointerenter',enter); preview.addEventListener('pointerleave',leave);
        trigger.addEventListener('focus',focus); trigger.addEventListener('blur',blur); trigger.addEventListener('keydown',previewEscape);
        board.addEventListener('toggle',toggle);
        window.addEventListener('resize',reposition); document.addEventListener('scroll',reposition,true);
        observer.observe(document.documentElement,{childList:true,subtree:true});
        state.hidePreview = hidePreview;
        state.cleanup = () => {
            observer.disconnect(); hidePreview();
            trigger.removeEventListener('pointerenter',enter); trigger.removeEventListener('pointerleave',leave);
            preview.removeEventListener('pointerenter',enter); preview.removeEventListener('pointerleave',leave);
            trigger.removeEventListener('focus',focus); trigger.removeEventListener('blur',blur); trigger.removeEventListener('keydown',previewEscape);
            board.removeEventListener('toggle',toggle); window.removeEventListener('resize',reposition); document.removeEventListener('scroll',reposition,true);
        };
    }
    state.reference = reference; state.requestedOpen = open;
    trigger.setAttribute('aria-expanded',open ? 'true' : 'false');
    if (open) {
        state.hidePreview();
        state.restoreBoardTheme?.(); state.restoreBoardTheme = copyTheme(trigger,board);
        if (!board.matches(':popover-open')) {
            board.showPopover(); state.wasOpen = true;
            // A modeless board has initial focus but neither a trap nor an inert/scroll-locked background.
            board.focus({preventScroll:true});
        }
    } else if (board.matches(':popover-open')) {
        if (board.contains(document.activeElement) && trigger.isConnected) trigger.focus({preventScroll:true});
        board.hidePopover(); state.wasOpen = false;
        state.restoreBoardTheme?.(); state.restoreBoardTheme = null;
    } else if (state.open) {
        state.restorePreviewTheme?.(); state.restorePreviewTheme = copyTheme(trigger,preview); positionMenu(state);
    }
}
export function detachTutorialBoard(trigger,preview,board) {
    const state = tutorialBoards.get(board);
    if (!state) return;
    tutorialBoards.delete(board); state.requestedOpen = false; state.cleanup();
    if (board.matches(':popover-open')) board.hidePopover();
    state.restoreBoardTheme?.(); trigger.setAttribute('aria-expanded','false');
}

export function attachMenu(trigger, panel, openOnHover=false) {
    if (!trigger || !panel) return;
    const existing = menus.get(panel);
    if (existing) {
        if (existing.openOnHover !== openOnHover || menuUnavailable(trigger)) closeMenu(trigger,panel,false);
        existing.openOnHover = openOnHover;
        trigger.setAttribute('aria-expanded',existing.open ? 'true' : 'false');
        if (existing.open) {
            existing.restoreTheme?.(); existing.restoreTheme = copyTheme(trigger,panel);
            positionMenu(existing);
        }
        return;
    }
    const state = { trigger, panel, hoverRoot:trigger.closest('[data-f-popup]') ?? trigger.closest('.f-action-menu') ?? trigger, openOnHover,
        itemSelector:'[role="menuitem"]:not([disabled])', disclosure:null, dismissOnAction:true,
        open:false, restoreTheme:null, maximumHeight:'', maximumHeightPriority:'', cleanup:[] };
    const listen = (target, type, callback, options) => {
        target.addEventListener(type, callback, options);
        state.cleanup.push(() => target.removeEventListener(type, callback, options));
    };
    listen(trigger, 'keydown', event => {
        if (!['ArrowDown','ArrowUp'].includes(event.key) || menuUnavailable(trigger)) return;
        event.preventDefault();
        event.stopPropagation();
        if (!state.open) openMenu(state,true);
        const items = menuItems(state); (event.key === 'ArrowUp' ? items.at(-1) : items[0])?.focus();
    });
    listen(state.hoverRoot, 'pointerenter', event => {
        if (state.openOnHover && event.pointerType !== 'touch' && !menuUnavailable(trigger)) openMenu(state,false);
    });
    const leave = event => {
        if (!weakPopup(panel) || !state.openOnHover || !state.open || event.pointerType === 'touch') return;
        if (state.hoverRoot.contains(event.relatedTarget) || panel.contains(event.relatedTarget)) return;
        closeMenu(trigger,panel,panel.contains(document.activeElement));
    };
    listen(state.hoverRoot, 'pointerleave', leave);
    listen(panel, 'pointerleave', leave);
    listen(panel, 'click', event => {
        const action = event.target?.closest?.('a,button,[role="menuitem"]');
        if (!action || !panel.contains(action)) return;
        if (menuUnavailable(action)) {
            event.preventDefault(); event.stopImmediatePropagation(); return;
        }
        if (!state.open || (!state.dismissOnAction && action.getAttribute('data-f-popup-close') === null)) return;
        // Native transport and callback dispatch complete before the shared menu dismisses.
        rememberInvoker(trigger);
        queueMicrotask(() => { if (state.open) closeMenu(trigger,panel,action.getAttribute('data-f-popup-close') !== null); });
    }, true);
    listen(document, 'pointerdown', event => {
        if (weakPopup(panel) && state.open && !panel.contains(event.target) && !trigger.contains(event.target)) closeMenu(trigger,panel,false);
    }, true);
    listen(document, 'focusin', event => {
        if (weakPopup(panel) && state.open && !panel.contains(event.target) && !trigger.contains(event.target)) closeMenu(trigger,panel,false);
    });
    listen(document, 'keydown', event => {
        if (!state.open || event.defaultPrevented) return;
        if (event.key === 'Escape') {
            if (weakPopup(panel)) { event.preventDefault(); event.stopPropagation(); closeMenu(trigger,panel,true); }
            return;
        }
        if (!panel.contains(document.activeElement) && document.activeElement !== trigger) return;
        if (!['ArrowDown','ArrowUp','Home','End'].includes(event.key)) return;
        const items = menuItems(state); if (!items.length) return;
        event.preventDefault();
        const index = items.indexOf(document.activeElement);
        const next = event.key === 'Home' ? 0 : event.key === 'End' ? items.length - 1
            : event.key === 'ArrowDown' ? (index + 1 + items.length) % items.length : (index - 1 + items.length) % items.length;
        items[next].focus();
    });
    listen(window,'resize',()=>positionMenu(state));
    listen(document,'scroll',()=>positionMenu(state),true);
    if (typeof ResizeObserver === 'function') {
        const resize = new ResizeObserver(() => positionMenu(state));
        resize.observe(trigger);
        state.cleanup.push(()=>resize.disconnect());
    }
    const observer = new MutationObserver(() => {
        if (!trigger.isConnected) detachMenu(trigger,panel);
        else if (state.open) positionMenu(state);
    });
    observer.observe(document.body,{childList:true,subtree:true,characterData:true});
    state.cleanup.push(()=>observer.disconnect());
    menus.set(panel,state);
}
export function attachDisclosureMenu(disclosure, trigger, panel, dismissOnAction=false, openOnHover=false) {
    if (!disclosure || !trigger || !panel) return;
    panel.setAttribute('data-f-popup-strength',disclosure.getAttribute('data-f-popup-strength') ?? 'weak');
    attachMenu(trigger,panel,openOnHover);
    const state = menus.get(panel);
    state.dismissOnAction = dismissOnAction;
    state.itemSelector = dismissOnAction ? 'a[href],button:not([disabled]),[role="menuitem"]:not([disabled])' : focusableSelector;
    if (!state.disclosure) {
        state.disclosure = disclosure;
        const synchronize = () => {
            if (!disclosure.open && state.open && !weakPopup(panel)) { disclosure.open = true; return; }
            disclosure.open ? openMenu(state,false) : closeMenu(trigger,panel,false);
        };
        disclosure.addEventListener('toggle',synchronize);
        state.cleanup.push(()=>disclosure.removeEventListener('toggle',synchronize));
        const activate = event => {
            if (state.open && !weakPopup(panel)) { event.preventDefault(); return; }
            if (!state.openOnHover) return;
            // Enhanced hover menus use the same pointer/keyboard activation policy as generated actions.
            // Prevent summary's default toggle from closing an already hovered menu.
            event.preventDefault();
            toggleMenu(trigger,panel,event.detail === 0);
        };
        trigger.addEventListener('click',activate);
        state.cleanup.push(()=>trigger.removeEventListener('click',activate));
    }
    // Synchronize native disclosure activation; selection options remain open, commands dismiss after dispatch.
    if (disclosure.open && !state.open) openMenu(state,false);
    else if (!disclosure.open && state.open) closeMenu(trigger,panel,false);
}
function openMenu(state, focusMenu) {
    if (state.open || menuUnavailable(state.trigger)) return;
    const {trigger,panel} = state;
    if (typeof panel.showPopover !== 'function') throw new TypeError('Native Popover API is required.');
    if (activeMenu && weakPopup(activeMenu.panel)) closeMenu(activeMenu.trigger,activeMenu.panel,false);
    state.restoreTheme = copyTheme(trigger,panel);
    const geometry = ['position','inset','margin','right','bottom','min-width',popupTriggerWidthProperty];
    const savedGeometry = geometry.map(name => [name,panel.style.getPropertyValue(name),panel.style.getPropertyPriority(name)]);
    state.restoreGeometry = () => savedGeometry.forEach(([name,value,priority]) => value ? panel.style.setProperty(name,value,priority) : panel.style.removeProperty(name));
    panel.style.setProperty('position','fixed'); panel.style.setProperty('inset','auto'); panel.style.setProperty('margin','0');
    panel.style.setProperty('right','auto'); panel.style.setProperty('bottom','auto');
    state.maximumHeight = panel.style.getPropertyValue('max-height');
    state.maximumHeightPriority = panel.style.getPropertyPriority('max-height');
    state.open = true;
    activeMenu = state;
    if (state.disclosure) state.disclosure.open = true;
    panel.setAttribute('data-f-open','');
    const visibility = panel.style.getPropertyValue('visibility'), priority = panel.style.getPropertyPriority('visibility');
    panel.style.setProperty('visibility','hidden');
    try {
        panel.showPopover();
        trigger.setAttribute('aria-expanded','true');
        positionMenu(state);
    } finally {
        if (visibility) panel.style.setProperty('visibility',visibility,priority);
        else panel.style.removeProperty('visibility');
    }
    if (focusMenu) menuItems(state)[0]?.focus({preventScroll:true});
}
export function toggleMenu(trigger, panel, focusMenu=true) {
    if (!menus.has(panel)) attachMenu(trigger,panel);
    const state = menus.get(panel); if (!state || menuUnavailable(trigger)) return;
    if (state.open) {
        if (!weakPopup(panel)) return;
        // Pointer clicks on an already hovered top-bar trigger should not flash the menu closed.
        if (state.openOnHover && !focusMenu) return;
        closeMenu(trigger,panel,true); return;
    }
    openMenu(state,focusMenu);
}
export function closeMenu(trigger,panel,restoreFocus=false) {
    const state = menus.get(panel); if (!state) return;
    const wasOpen = state.open;
    if (state.open && panel.isConnected && panel.matches(':popover-open')) panel.hidePopover();
    panel.removeAttribute('data-f-open');
    state.open = false;
    if (state.disclosure) state.disclosure.open = false;
    trigger.setAttribute('aria-expanded','false');
    state.restoreTheme?.(); state.restoreTheme = null;
    state.restoreGeometry?.(); state.restoreGeometry = null;
    if (wasOpen) {
        if (state.maximumHeight) panel.style.setProperty('max-height',state.maximumHeight,state.maximumHeightPriority);
        else panel.style.removeProperty('max-height');
    }
    if (activeMenu === state) activeMenu = null;
    if (restoreFocus && trigger.isConnected) {
        state.suppressFocus = true;
        trigger.focus({preventScroll:true});
        queueMicrotask(() => { state.suppressFocus = false; });
    }
}
export function detachMenu(trigger,panel) {
    const state = menus.get(panel); if (!state) return;
    closeMenu(trigger,panel,false); state.cleanup.forEach(remove=>remove()); menus.delete(panel);
}


/** Enhances static/native popup content using the same controller as interactive components. */
export function initializePopups(root = document) {
    initializeNativePickers(root);
    for (const owner of root.querySelectorAll('[data-f-popup="menu"]')) {
        if (!owner.isConnected) continue;
        const trigger = owner.querySelector(':scope > summary, :scope > button');
        const panel = owner.querySelector(':scope > [data-f-popup-panel]');
        if (!trigger || !panel) continue;
        panel.setAttribute('popover','manual');
        panel.setAttribute('data-f-popup-strength',owner.getAttribute('data-f-popup-strength') ?? 'weak');
        owner.setAttribute('data-f-enhanced','true');
        const hover = owner.getAttribute('data-f-popup-hover') === 'true';
        const dismiss = owner.getAttribute('data-f-popup-dismiss-action') === 'true';
        if (owner.tagName === 'DETAILS') attachDisclosureMenu(owner,trigger,panel,dismiss,hover);
        else {
            const installed = menus.has(panel);
            attachMenu(trigger,panel,hover);
            const state = menus.get(panel);
            state.dismissOnAction = dismiss;
            state.openOnFocus = owner.getAttribute('data-f-popup-focus') === 'true';
            state.itemSelector = dismiss ? 'a[href],button:not([disabled]),[role="menuitem"]:not([disabled])' : focusableSelector;
            if (!installed) {
                const activate = event => toggleMenu(trigger,panel,event.detail === 0);
                const focus = () => { if (state.openOnFocus && !state.suppressFocus) openMenu(state,false); };
                trigger.addEventListener('click',activate); trigger.addEventListener('focus',focus);
                state.cleanup.push(()=>trigger.removeEventListener('click',activate),()=>trigger.removeEventListener('focus',focus));
            }
        }
    }
    initializeNativeSelectPopups(root);
}

function restoreDialogFocus(state) {
    const opener = (state.opener?.isConnected ? state.opener
        : document.querySelector(`[aria-controls="${CSS.escape(state.dialog.id)}"]`))
        ?? document.querySelector(`[data-f-dialog-return-focus="${CSS.escape(state.dialog.id)}"]`);
    if (opener && !opener.closest('[inert]') && !opener.disabled) opener.focus({preventScroll:true});
    state.opener = null;
}
function closeDialog(state, restore=true) {
    if (state.dialog.open) {
        state.dialog.close();
    }
    if (restore) restoreDialogFocus(state);
}
export function synchronizeDialog(dialog,isOpen,reference = null) {
    if (!dialog?.isConnected) return;
    if (isOpen && typeof dialog.showModal !== 'function') throw new TypeError('Native Dialog API is required.');
    let state = dialogs.get(dialog);
    if (!state) {
        state = { dialog,reference,pending:false,opener:null,cleanup:[] };
        const cancel = async event => {
            event.preventDefault();
            if (!weakPopup(dialog) || dialog.dataset.fDialogDismissible === 'false' || dialog.getAttribute('aria-busy') === 'true' || state.pending) return;
            state.pending = true;
            try { if (state.reference) await state.reference.invokeMethodAsync('RequestDismissAsync'); else closeDialog(state); }
            catch { /* A reconnect must retain the open form. */ }
            finally { state.pending = false; }
        };
        const keys = event => {
            if (!dialog.open) return;
            if (event.key !== 'Tab') return;
            const items = [...dialog.querySelectorAll(focusableSelector)].filter(visible);
            const first = items[0], last = items.at(-1);
            if (!first) { event.preventDefault(); dialog.focus(); return; }
            if (event.shiftKey && (document.activeElement === first || document.activeElement === dialog)) { event.preventDefault(); last.focus(); }
            else if (!event.shiftKey && (document.activeElement === last || !dialog.contains(document.activeElement))) { event.preventDefault(); first.focus(); }
        };
        const close = event => {
            const control = event.target?.closest?.('[data-f-dialog-close]');
            if (dialog.dataset.fDialogBrowserControlled !== 'true' || !control || control.closest('dialog') !== dialog) return;
            if (dialog.dataset.fDialogDismissible === 'false' || dialog.getAttribute('aria-busy') === 'true') return;
            event.preventDefault(); closeDialog(state);
        };
        const backdrop = event => {
            if (event.target !== dialog || !dialog.open) return;
            const bounds = dialog.getBoundingClientRect();
            if (event.clientX >= bounds.left && event.clientX <= bounds.right && event.clientY >= bounds.top && event.clientY <= bounds.bottom) return;
            void cancel(event);
        };
        dialog.addEventListener('click',backdrop);
        state.cleanup.push(()=>dialog.removeEventListener('click',backdrop));
        dialog.addEventListener('click',close);
        state.cleanup.push(()=>dialog.removeEventListener('click',close));
        dialog.addEventListener('cancel',cancel); dialog.addEventListener('keydown',keys);
        state.cleanup.push(()=>dialog.removeEventListener('cancel',cancel),()=>dialog.removeEventListener('keydown',keys));
        const observer = new MutationObserver(()=>{ if (!dialog.isConnected) detachDialog(dialog); });
        observer.observe(document.body,{childList:true,subtree:true}); state.cleanup.push(()=>observer.disconnect());
        dialogs.set(dialog,state);
    }
    state.reference = reference;
    if (isOpen && !dialog.open) {
        if (activeMenu && weakPopup(activeMenu.panel)) closeMenu(activeMenu.trigger,activeMenu.panel,false);
        state.opener = resolveInvoker(document.activeElement);
        dialog.showModal();
        dialog.querySelector('[data-f-dialog-body]')?.scrollTo(0,0);
        const requested = [...dialog.querySelectorAll('[autofocus]')].find(element => visible(element) && !element.disabled);
        const target = requested ?? [...dialog.querySelectorAll(focusableSelector)].find(visible);
        if (target) target.focus({preventScroll:true}); else { dialog.tabIndex = -1; dialog.focus({preventScroll:true}); }
    } else if (!isOpen && dialog.open) closeDialog(state);
}
/** Selects a keyed view and its declared actions without introducing a second modal controller. */
export function setDialogView(dialog,key) {
    if (!dialog?.isConnected) return false;
    const owned = element => element.closest('dialog') === dialog;
    const views = [...dialog.querySelectorAll('[data-f-dialog-view]')].filter(owned);
    if (!views.some(view => view.dataset.fDialogView === key)) return false;
    for (const view of views) view.hidden = view.dataset.fDialogView !== key;
    for (const action of [...dialog.querySelectorAll('[data-f-dialog-views]')].filter(owned))
        action.hidden = !action.dataset.fDialogViews.split(/\s+/).includes(key);
    dialog.dataset.fDialogActiveView = key;
    const actions = dialog.querySelector('.f-dialog-actions');
    if (actions && owned(actions)) {
        actions.hidden = false;
        const declared = [...actions.querySelectorAll('[data-f-dialog-views]')].filter(owned);
        if (declared.length && declared.every(element => element.hidden))
            actions.hidden = ![...actions.querySelectorAll('button,a[href],input,select,textarea')].some(element => !element.closest('[hidden]'));
    }
    if (dialog.open && document.activeElement && dialog.contains(document.activeElement) && !visible(document.activeElement)) {
        const target = [...dialog.querySelectorAll(focusableSelector)].find(visible);
        if (target) target.focus({preventScroll:true}); else { dialog.tabIndex = -1; dialog.focus({preventScroll:true}); }
    }
    return true;
}
export function detachDialog(dialog) {
    const state = dialogs.get(dialog); if (!state) return;
    closeDialog(state); state.cleanup.forEach(remove=>remove()); dialogs.delete(dialog);
}
