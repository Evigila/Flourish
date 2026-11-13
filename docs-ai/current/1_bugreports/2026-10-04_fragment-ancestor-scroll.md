# Fragment navigation scrolls fixed shell children

## Symptoms

Activating UniformGridButton's scenario status fragment link moves the top bar/navigation upward and leaves empty space below the application. The link does not perform a real status operation. Earlier Busy containment, document ownership and current-document link fixes did not prevent this ancestor-scroll case.

## Cause and evidence

overflow:hidden suppresses a scrollbar but retains a programmatically scrollable container. Native navigation to a target inside the content area can also scroll its fixed outer shell ancestor. Against the previous compiled Gallery at 1440 by 900, clicking the status link preserved the current route and window/html/body scroll offsets at zero, while the header's viewport top changed from 0 to -68 and the content viewport top changed from 68 to 0. The outer shell was still position:fixed. Thus this symptom is distinct from root-route navigation and visible body scrolling.

An injected overflow:clip rule on the document-owning shell preserved the header at 0 and content viewport at 68 for the same link, while the inner content scrolled to its target. The shipped Framework stylesheet now applies that rule. The rule is limited to data-f-document-shell; embedded shells retain their existing host layout. Real section navigation, callbacks and link semantics remain intact.

## Mitigation

- Use overflow:clip on document-owning shells so fragments/scrollIntoView cannot scroll their chrome container.
- Remove inert status fragment actions from Button, UniformGrid and UniformGridButton scenarios; retain working save/reset actions and status text.
- Remove SplitButton's preview link to a target rendered only in scenario mode. Its actual export and disclosure examples remain.

## Regression checks and limitations

An isolated temporary Gallery and headless Edge verified 1440px, 800px and 320px layouts. Save/reset and a native fragment link inserted to a real status target left shell scrollTop, window scrollY and header viewport top at zero. Content scrolling remained functional. Both six-cell shapes had no extra track or cell overflow in the tested previews. No browser page errors appeared. The service/browser were closed afterward; the user's VS process was not stopped.

Final automated evidence: C:/Users/Evigila/AppData/Local/Temp/flourish-guide-acceptance-9a88820658e1464598291f9e7dfb2fd6.cjs and its matching flourish-control-http log files. Temporary evidence is local run material, not a durable dependency. Manual supported-browser, zoom and embedded-consumer acceptance remains in blazor-manual-tests.md. Historical reports remain unchanged; this report supplements their narrower diagnoses.
