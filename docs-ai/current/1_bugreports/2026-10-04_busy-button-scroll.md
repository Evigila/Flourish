# Busy button and temporary document overflow

## Symptom

The user reported two scrollbars at the page's right edge while Save draft was pending in the Button scenario; the additional scrollbar disappeared after the save.

## Source evidence and diagnosis

Button's Busy state previously inserted f-busy-text. Its hidden layout existed only in Design foundation.css and used absolute positioning, while the button had no positioning context. An absolute child in a deep scrolled demonstration could therefore belong to the positioned outer shell instead of its own action. The Gallery-only sample-board also used general overflow:auto, introducing another potential vertical scroll track. The conditional status node and the user's temporary symptom are consistent with this mechanism. No browser measurement was performed to certify the exact pixel-level cause.

## Mitigation

Busy text now uses Framework's f-sr-only semantics in both native and styled consumers. Functional CSS positions the button relatively, containing its hidden status. The old Design-only hidden rule and the icon-busy rule that suppressed the status from assistive technology were removed. Button keeps its busy/disabled callback guard. The reusable DisplayBoard has no general overflow:auto wrapper; code blocks and tables own their required content overflow. Clipboard fallback uses a fixed 1px textarea and always removes it, avoiding a new document overflow source.

## Verification and limits

The 78-check .NET suite verifies Busy/native-disabled semantics, callback suppression and standardized board output. Actual clipboard DOM tests cover fixed fallback sizing, cleanup and restoration. Builds and HTTP style delivery pass. These checks do not observe scrollbar pixels. Manual regression: scroll to Save draft in a long guide, activate it repeatedly, inspect the entire Busy interval at wide/narrow widths and 200% zoom, and repeat Framework-only. Only the shell's content scroll track should remain. See [manual acceptance](../blazor-manual-tests.md).
