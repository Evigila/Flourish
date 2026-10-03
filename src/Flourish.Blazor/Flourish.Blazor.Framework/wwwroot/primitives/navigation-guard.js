let hasUnsavedChanges = false;
let externalNavigationApproved = false;
let listening = false;

function confirmExternalNavigation(event) {
    if (!hasUnsavedChanges) {
        return;
    }

    if (externalNavigationApproved) {
        externalNavigationApproved = false;
        return;
    }

    event.preventDefault();
    event.returnValue = "";
}

export function setUnsavedChanges(value) {
    hasUnsavedChanges = value === true;
    if (!listening) {
        window.addEventListener("beforeunload", confirmExternalNavigation);
        listening = true;
    }
}

export function allowNextExternalNavigation() {
    externalNavigationApproved = true;
}

export function dispose() {
    if (listening) {
        window.removeEventListener("beforeunload", confirmExternalNavigation);
    }

    hasUnsavedChanges = false;
    externalNavigationApproved = false;
    listening = false;
}
