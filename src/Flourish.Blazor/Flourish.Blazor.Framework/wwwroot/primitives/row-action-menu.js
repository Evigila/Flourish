import { rememberInvoker } from "./interaction-origin.js";

let openMenu = null;
let openTrigger = null;
let listenersInstalled = false;

export function toggle(trigger, menu) {
    installListeners();
    if (menu.matches(":popover-open")) {
        close(menu, trigger, false);
        return;
    }

    if (openMenu) close(openMenu, openTrigger, false);
    menu.showPopover();
    openMenu = menu;
    openTrigger = trigger;
    trigger.setAttribute("aria-expanded", "true");
    prepareItems(menu);
    positionMenu(trigger, menu);
}

export function dispose(trigger, menu) {
    if (openMenu === menu) close(menu, trigger, false);
}

function installListeners() {
    if (listenersInstalled) return;
    listenersInstalled = true;

    document.addEventListener("pointerdown", event => {
        if (!openMenu || openMenu.contains(event.target) || openTrigger?.contains(event.target)) return;
        close(openMenu, openTrigger, false);
    }, true);

    document.addEventListener("focusin", event => {
        if (!openMenu || openMenu.contains(event.target) || openTrigger?.contains(event.target)) return;
        close(openMenu, openTrigger, false);
    });

    document.addEventListener("click", event => {
        if (!openMenu || !openMenu.contains(event.target)) return;
        const action = event.target.closest("a, button, [role='menuitem']");
        if (action) {
            rememberInvoker(openTrigger);
            queueMicrotask(() => close(openMenu, openTrigger, false));
        }
    });

    document.addEventListener("keydown", event => {
        if (!openMenu) return;
        if (event.key === "Escape") {
            event.preventDefault();
            close(openMenu, openTrigger, true);
            return;
        }

        const items = menuItems(openMenu);
        if (items.length === 0 || !["ArrowDown", "ArrowUp", "Home", "End"].includes(event.key)) return;
        event.preventDefault();
        const activeIndex = items.indexOf(document.activeElement);
        const nextIndex = event.key === "Home" ? 0
            : event.key === "End" ? items.length - 1
                : event.key === "ArrowDown" ? (activeIndex + 1 + items.length) % items.length
                    : (activeIndex - 1 + items.length) % items.length;
        items[nextIndex].focus();
    });

    window.addEventListener("resize", repositionOpenMenu);
    document.addEventListener("scroll", repositionOpenMenu, true);
}

function prepareItems(menu) {
    for (const item of menu.querySelectorAll("a[href], button")) item.setAttribute("role", "menuitem");
}

function menuItems(menu) {
    return Array.from(menu.querySelectorAll("a[href], button:not([disabled]), [role='menuitem'][tabindex]"));
}

function repositionOpenMenu() {
    if (openMenu && openTrigger) positionMenu(openTrigger, openMenu);
}

function positionMenu(trigger, menu) {
    const triggerRect = trigger.getBoundingClientRect();
    const menuRect = menu.getBoundingClientRect();
    const gap = 6;
    const viewportPadding = 12;
    const left = Math.min(
        Math.max(viewportPadding, triggerRect.right - menuRect.width),
        window.innerWidth - menuRect.width - viewportPadding);
    const below = triggerRect.bottom + gap;
    const above = triggerRect.top - menuRect.height - gap;
    const top = below + menuRect.height <= window.innerHeight - viewportPadding
        ? below
        : Math.max(viewportPadding, above);
    menu.style.left = `${left}px`;
    menu.style.top = `${top}px`;
}

function close(menu, trigger, restoreFocus) {
    if (!menu || !trigger) return;
    if (menu.matches(":popover-open")) menu.hidePopover();
    trigger.setAttribute("aria-expanded", "false");
    if (openMenu === menu) {
        openMenu = null;
        openTrigger = null;
    }
    if (restoreFocus) trigger.focus();
}
