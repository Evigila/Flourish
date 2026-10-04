// Copy only after an explicit user action; preserve the page's focus and selection.
export async function copyText(text) {
    try {
        if (navigator.clipboard?.writeText) {
            await navigator.clipboard.writeText(text);
            return true;
        }
    } catch { /* Some embedded browsers deny Clipboard API permission. */ }

    const active = document.activeElement;
    const inputSelection = active && typeof active.selectionStart === 'number'
        ? [active.selectionStart,active.selectionEnd,active.selectionDirection] : null;
    const selection = window.getSelection();
    const ranges = selection ? Array.from({length:selection.rangeCount},(_,index)=>selection.getRangeAt(index).cloneRange()) : [];
    const textarea = document.createElement('textarea');
    textarea.value = text;
    textarea.setAttribute('readonly','');
    textarea.setAttribute('aria-hidden','true');
    // A fixed, contained fallback never contributes to document scroll dimensions.
    Object.assign(textarea.style,{position:'fixed',top:'0',left:'0',width:'1px',height:'1px',padding:'0',border:'0',opacity:'0'});
    try {
        document.body.append(textarea);
        textarea.focus({preventScroll:true});
        textarea.select();
        return document.execCommand('copy');
    } catch { return false; }
    finally {
        try { textarea.remove(); } catch { /* The host may already have removed it. */ }
        if (active?.isConnected) {
            try { active.focus({preventScroll:true}); } catch { /* The opener may become unavailable. */ }
            try { if (inputSelection) active.setSelectionRange(...inputSelection); } catch { /* Its input type may have changed. */ }
        }
        if (selection) {
            try { selection.removeAllRanges(); } catch { /* The document may be changing. */ }
            for (const range of ranges) {
                try { selection.addRange(range); } catch { /* A saved range may now be detached. */ }
            }
        }
    }
}
