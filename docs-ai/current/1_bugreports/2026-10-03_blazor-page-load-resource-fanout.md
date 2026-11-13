# Gallery page-load resource fan-out

Status: diagnosed from source, HTTP probes and the user's Visual Studio console capture. Application code is unchanged in this investigation; mitigation is proposed below. The earlier functional acceptance does not certify page-load performance.

## Reproduction and evidence

The user runs Gallery.Blazor from Visual Studio at http://localhost:5188/ in Development, reports tens-of-seconds refresh waits and opens the root page once without further interaction. Startup emits six Information records. The supplied capture is 117812 bytes and contains 1190 lines.

Captured text: C:/Users/Evigila/.codex/attachments/b460b86c-f935-4fba-97e4-e52c355b7dc8/已粘贴的文本.txt. Machine-readable statistics and HTTP probes are preserved under C:/Users/Evigila/.codex/visualizations/2026/10/03/01a0ff44-995d-7090-af88-8835158b1571/ in gallery-vs-console-analysis-2026-10-03.json, gallery-vs-http-performance-2026-10-03.json and gallery-http-performance-2026-10-03.json.

| Observation | Result |
| --- | --- |
| Started/completed requests | 120 / 119, all paths distinct |
| CSS requests | 110, all status 200 |
| CSS response lengths | 377948 bytes in aggregate |
| CSS request median / P95 / maximum | 529.5313 / 930.0849 / 1282.6880 ms |
| CSS requests at least 500 / 1000 ms | 61 / 3 |
| Root HTML server request | 223.8245 ms |
| Blazor initializers / negotiation | 3.3968 / 6.2604 ms |
| Completed statuses | 117 status 200, one 304, one 404 |
| Log records | 594 Information, one Warning; no Error/Critical |

The unmatched started request is the retained /_blazor connection. The sole 404 is the browser's favicon.ico request. The only warning is HTTPS redirection failing to infer an HTTPS port for the HTTP launch profile; the capture shows no redirect loop. The MVC action-descriptor startup Information record is not a failed MVC request in this Razor-only host. Browser Refresh and Browser Link are Visual Studio development requests.

No repeated path, business API request, or repeated root navigation is present in this capture. The volume is automatic resource loading after one page open, not evidence of user interaction or a polling loop.

## Confirmed resource-delivery cause

Gallery App.razor lines 7-8 link Framework/framework.css and Design/design.css globally. Framework imports behavior, colligere and native entries; Design imports foundation, controls, data, primitives, colligere and overrides. Those entries import individual component/layout/business-page files.

Recursive source review confirms 35 Framework and 75 Design CSS URLs, 374318 source bytes, two import hops/three discovery layers, no missing imports and no cycles. Forty-eight page-specific CSS URLs and ten layout CSS URLs are loaded even on the ordinary overview page. Framework and Design partition behavior and skin; these are distinct assets rather than identical duplicate downloads.

All 110 CSS requests finish before /_blazor/initializers appears in the console capture. This demonstrates that this run's interactivity startup follows the entire stylesheet load chain. Runtime @import makes the browser discover downstream assets only after parent stylesheets arrive. HTTP/1.1 and many separate small requests amplify scheduling and development-host overhead.

The console reports 58.31067 seconds of CSS request durations summed across parallel requests. This is aggregate occupied request time, not a measured 58-second page load. The capture has no common wall-clock timestamps or browser waterfall, so it cannot establish exact end-to-end duration or how many seconds are specifically spent writing debugger/console output.

## Confirmed logging cause and development amplification

Gallery has no appsettings.json or appsettings.Development.json. The default logging threshold is Information. The capture contains 240 Hosting.Diagnostics, 235 EndpointMiddleware and 113 StaticAssetsInvoker records: 588 request-related Information records in addition to six startup records. A normal static endpoint logs request start, endpoint start, file send, endpoint completion and request completion. The library's component code does not emit these records.

The active Development instance returns Cache-Control: no-cache for literal CSS paths. All 110 captured CSS responses include content, while blazor.web.js uses a 304. Gallery links literal asset paths; the 108 imported descendants also use literal paths. Merely fingerprinting the two entry links would not fingerprint that descendant graph.

The built manifest has matching raw source lengths and 85772 total precompressed gzip bytes for the 110 CSS files. The captured lengths total 377948 bytes, exactly source size plus 33 bytes per file. An explicit default-instance gzip probe returned Framework CSS as 9257 wire bytes with an unchanged ETag conditional request still returning 200; the manifest's precompressed form is 2233 bytes. A temporary separate instance using the same built application with ReloadStaticAssetsAtRuntime=false returned 2233 bytes and 304. This establishes development runtime asset reprocessing as an additional transfer/cache cost. It does not establish that disabling Hot Reload is the correct permanent product change, or identify an internal SDK defect without further source-level evidence.

Before reading the user's capture, direct sequential HTTP probes to the active Visual Studio instance measured root HTML at 47.01 ms, Framework CSS at 6.02 ms and Design CSS at 1.41 ms. Prior isolated warm HTML probes were approximately 0.7-1.1 ms. This contrast with the user's bulk resource request timings supports debugger/logging/concurrency amplification as a candidate, but does not quantify each contribution separately.

## Component review boundary

The default overview has no remote load, table, timer or continuous StateHasChanged path. SSR does not run OnAfterRenderAsync. No default-page infinite rerender was found. Its interactive startup initializes the shell and action-menu modules once per component lifetime, with existing binding guards.

WorkspaceDataTable separately invokes connect after every render, and its JS remeasures cells even when already bound. Closed sheets also initialize their modules/controllers. These are confirmed avoidable work in other views and deserve targeted optimization, but do not explain this root-page capture.

## Proposed mitigation order

1. Add ordinary host logging configuration: Default=Information, Microsoft.AspNetCore=Warning, retaining startup messages and Warning/Error. Do not apply a global host logging policy inside AddFlourish. This controls noise; it is not a substitute for fixing resource delivery.
2. Preserve modular CSS source but flatten ordered local imports at build/pack time into separate Framework and Design entry artifacts. Preserve cascade order, relative URLs, conditional imports and optional Design independence. Do not introduce an external bundler without approval.
3. Use versioned/fingerprinted built assets and verify real publish-mode compression, ETag and fresh-cache behavior. Keep normal development editing behavior; compare a temporary non-hot-reload run rather than permanently suppressing it without acceptance.
4. Separate retained Colligere-specific page skins into an explicit compatibility/route opt-in, so a third-party overview does not load every product page's selectors. Maintain current Colligere scope identities and appearance during this change.
5. Initialize closed floating controllers on demand and gate table measurement by relevant data/layout revisions. Confirm with targeted counts before attributing a performance win.
6. Clean up the HTTP-only HTTPS warning and provide an explicit existing-icon favicon. These are minor log/extra-request corrections, not the primary delay.

## Manual acceptance after mitigation

- Run the same root URL in VS F5 and without the debugger. Capture cold and warm refresh at equal viewport/browser settings, including Network waterfall, document response, final stylesheet response and time until the first control responds.
- Confirm the root requests compiled stylesheet entries rather than 110 imported CSS files; inspect the request count before claiming improvement. Compare CSS transfer bytes and unchanged-resource caching.
- Keep Warning/Error visible and preserve startup Information; request-by-request Information should require an explicit diagnostic setting.
- Compare native Framework-only and Design consumers, Gallery overview/Workspace/forms and actual Colligere pages. Cascade, focused controls, layout, masks and theme roles must remain equivalent.
- Repeat static-asset consumption from independently packed NuGet packages, including a fresh version deployment that must not reuse stale CSS.

No Computer Use, domain operation, new package/font, application code edit or service restart of the user's VS host was performed. The temporary diagnostic instance is no longer listening; it did not replace the user's service.

## Primary documentation

- [Microsoft static asset development/cache/compression guidance](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/static-files?view=aspnetcore-10.0).
- [Microsoft logging defaults and category configuration](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/logging/?view=aspnetcore-10.0).
- [Blazor prerender/interactivity lifecycle](https://learn.microsoft.com/en-us/aspnet/core/blazor/components/prerender?view=aspnetcore-10.0).

## Subsequent user clarification and repair

The user explicitly rejected mitigation item 4's consumer compatibility/route opt-in on 2026-10-03. The library must be universal and know no consuming product. That proposal is superseded: business page/layout CSS and business scripts return to the host; only generic components, layouts, tokens and behavior remain in the library. The original request/log measurements above remain historical evidence. Implementation and subsequent verification are described in [the generic boundary and loading repair](2026-10-03_generic-blazor-assets-and-page-load.md).
