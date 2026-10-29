# Project dependency inventory

## Current controlled-table candidate, 2026-10-29

Root source now declares **1.4.4-preview.shell.20261027.16**, based on `8999b26`. The six-package Core/Blazor manifest, aligned internal versions and Essential.Culture **1.4.0** are retained. No external package, SDK, browser module, font, service or storage dependency is added. The new `TableQueryRequest` public record changes Abstract; the existing DataTable changes Framework. Both assemblies therefore require the new package identity, while the preceding .15 feed remains immutable. The six locally produced .16 packages are in `artifacts/liquidity-directory-20261029/nuget`; they have not been publicly published.

The feature uses existing Blazor parameters and EventCallback. It delegates data retrieval, stable ordering, paging, scope and count consistency to the host; it does not create a browser-side data source or an unbounded cache. Server mode is opt-in, so existing local table consumers preserve their default behavior. Gallery metadata covers the new parameters in all three supported languages. Current catalog: **84 components / 791 parameter rows / 372 documented defaults**.

Coordinated producer verification passed: Release **0 warnings / 0 errors**, Framework **496/496**, Gallery **8755/8755**. Logs are `artifacts/liquidity-directory-20261029/build-final.log`, `framework-tests-final.log` and `gallery-tests.log`. The coordinator verified the six archives and restored Colligere against the new local feed. This proves build/package integration, not physical-browser behavior, a running app's cache provenance or production deployment. Contract details, the corrected initial test expectation and manual acceptance are in the [change record](1_changelogs/2026-10-29_192626_add-controlled-server-data-table.md).

The six-package identity/dependency/required-asset check is recorded in `artifacts/liquidity-directory-20261029/package-verification.log`. The coordinating Colligere report `artifacts/liquidity-directory-20261029/package-integrity.log` verifies that each changed assembly has identical bytes in the .16 archive, producer Release output and consumer package cache: Abstract SHA-256 `19262c1eb80c492d100fec9a79ccef97d3cda62fed46b56e19eb005ff8470a8c`; Framework SHA-256 `f25b08cd56034a269da1365f33ec89a1f82e59da02a66ef527bae9413346959b`.

The earlier 2026-10-29 [Gallery save-bar adoption](1_bugreports/2026-10-29_appearance-save-bar-adoption.md) composed the already frozen .15 control in the color settings page. It changed no framework source/package payload, SDK/runtime, external package, font, service, tool requirement or resolved graph. No .15 archive was rebuilt or republished for that application-only adoption.

## Previous .15 local shell candidate, 2026-10-29

The preceding root source declared **1.4.4-preview.shell.20261027.15** on committed baseline `aaac88c`, retaining the preceding .13/.14 work after stable release `37a42ae` (`v1.4.3` source `d4e8cd95c58e87646684903a0fdb2f9767994931`). The candidate retains the six-package Core/Blazor manifest: Arkheide.Flourish.Core, Arkheide.Flourish.Blazor.Abstract, Arkheide.Flourish.Blazor.Framework, Arkheide.Flourish.Blazor.Design, Arkheide.Flourish.Extensions.Culture.Blazor and Arkheide.Flourish.Blazor. Internal package edges use the same candidate version; Essential.Culture remains **1.4.0**. WPF/WinUI release scope is unchanged. No additional external NuGet/npm package, SDK installation, font/service or host-side control implementation is required.

The header/footer controller reuses existing browser ResizeObserver/MutationObserver, animation-frame, computed-style and CSS-grid APIs. The fitted header menu uses measured intrinsic controls while the owned footer foreground keeps the actual page content bounds; responsive minimum-height and bottom alignment remain Framework/Design responsibilities. The existing progress controller owns the header-aware upper-third activation line, immediate click destination and interruption lifecycle without a new dependency. Static SSR uses the existing browser entry; interactive surfaces share the same module. Hover selection reuses ActionMenu and its existing popup controller around a native select, implemented by the internal NativeSelectPopup helper rather than a new public dependency/component. Framework owns behavior and optional Design owns paint. The normal no-JavaScript header layout and native select remain usable; exact content alignment and opt-in hover behavior require the library browser modules.

The default-enabled primary preview uses the existing native manual Popover API, DOM pointer/focus/keyboard events, ResizeObserver/MutationObserver and animation frames. A reserved slot preserves layout while the same rail enters the top layer; there is no cloned fragment, extra browser library, package or external service. The same module is shared by static initialization, NavigationSurface and ApplicationShell lifecycle; normal ESM URL caching shares that module instance. Tooltip rebinding remains in the existing primary-navigation module. Framework/Design CSS partials are inlined into the existing bundles, not delivered as additional stylesheet requests. The controller is one new same-origin module request; both complete/common font assets remain unchanged. Candidate .12 narrows the preview, reserves a visible quarter of vertical secondary navigation and adds reversible local CSS motion, using the existing browser/CSS modules.

Information-mode UniformGrid items now have 24px internal padding instead of 16px, with natural height and unchanged H1/copy semantics; ordinary action/form layouts retain their prior spacing. This and Card compatibility metadata/Dialog.DangerTitle use existing component/rendering, semantic-token and .NET attribute facilities. Header inset and structured-row spacing changes remain in Framework/Design CSS; no new browser library, package or service is introduced.

The compact brand correction changes only existing native image attributes and Framework/Design geometry: 24px logo at every width, standard 48px Button target, an explicit growable 68px header minimum and 82px/78px fallback wing widths. The .9 intrinsic-width correction retains real control boxes, existing minimum widths and fixed nonshrinking row-flex bases while ignoring stretched layout tracks; no additional dependency or maintained file is introduced.

The current icon derivative adds no third-party package: common variable-font WOFF generation uses existing Windows DirectWrite/WPF and PowerShell plus installed Python **3.12.14 standard-library zlib/struct**. These are optional maintenance tools, not normal build/consumer dependencies. The original Apache-2.0 variable font is retained byte-identically, with a derived 125,404-byte common WOFF and explicit disjoint character ranges; both styles and all supported variation axes remain available. Font provenance is in the existing source.json, including the generator, manifest and common hash. No font CDN or external conversion service is contacted.

Candidate .13 uses the existing CSS custom-property/min/max sizing, trigger ResizeObserver and native option probe to make dropdown width at least the trigger border box. Natural content width, existing author/viewport maxima, filtering and input semantics remain. The native picker branch keeps its natural option cache separate from the trigger floor. This adds no external browser library/API requirement beyond the existing supported native/CSS path.

### Frozen .15 archive and asset evidence

Candidate **1.4.4-preview.shell.20261027.15** is frozen in `artifacts/save-changes-bar-20261029/nuget`. It adds the public SaveChangesBar scenario, localized/default API metadata and one executable Gallery sample, preserving the preceding .13/.14 changes. The six package IDs and aligned internal versions, Essential.Culture 1.4.0, twelve producer graphs and **33 external package/version pairs** remain unchanged. The catalog is now **84 components/785 parameter rows/368 defaults**. This adds no external NuGet/npm dependency, browser driver, SDK, font or service.

The browser enhancement is one new same-origin Framework module, imported only when its interactive component becomes active. It uses existing DOM events, MutationObserver/ResizeObserver, computed geometry and native beforeunload; managed navigation interception and EventCallback/SemaphoreSlim use the existing .NET/Blazor runtime. The Framework/Design CSS partials are inlined into the existing bundles, without extra stylesheet requests. Node coverage uses built-in modules. Page-owned validation and persistence remain host responsibilities; no new storage or backend service is required.

Release passed with zero warnings/errors. Framework **494/494**, Gallery **8750/8750** (including 474 post-event and 443 changelog checks), Culture bridge **27/27**, focused save-bar Node **24/24** and CSS bundler **21/21** passed. No Computer Use or browser visual measurement was performed. Package verification confirms **14 unique delivery entries: 13 static resources plus one Framework DLL**; this is not a count of HTTP requests or a consumer-runtime/cache verification. Exact archive hashes, byte lengths, source/Release comparison paths and the twelve graph paths are recorded in [package-delivery.json](../../artifacts/save-changes-bar-20261029/package-delivery.json). Earlier candidate hashes below are historical, not current .15 delivery.

| Candidate archive | Bytes | SHA-256 |
| --- | ---: | --- |
| Arkheide.Flourish.Blazor.1.4.4-preview.shell.20261027.15.nupkg | 2095 | `22f4bab636e2414692cd73633eb2cf83976417f40cb3140f5412fbe4d409b1f7` |
| Arkheide.Flourish.Blazor.Abstract.1.4.4-preview.shell.20261027.15.nupkg | 33496 | `18e513d7fbba5b0fe9f5ceb9d13b9f777deed9765d533f17c5f5fc1e132633c8` |
| Arkheide.Flourish.Blazor.Design.1.4.4-preview.shell.20261027.15.nupkg | 60594 | `b6b83c60c0bb592229c7c994feb6c3551daf9d1b538dc5e74b2d336c749fdafe` |
| Arkheide.Flourish.Blazor.Framework.1.4.4-preview.shell.20261027.15.nupkg | 4528817 | `cfb03fd7e1bfb7682d6c442bd627a431e9d7389da30c5640ce44535394e0bfc1` |
| Arkheide.Flourish.Core.1.4.4-preview.shell.20261027.15.nupkg | 142997 | `25a7733159896e68425976d8ea2a9c2b71ee904243ed61860f15e8aa7912040d` |
| Arkheide.Flourish.Extensions.Culture.Blazor.1.4.4-preview.shell.20261027.15.nupkg | 24010 | `3aebb4d564c342ad04e370e92aca96ab5e4b7ef4b79d2c91990cca62db4cea37` |

| Unique delivered entry | Bytes | SHA-256 |
| --- | ---: | --- |
| Design / `staticwebassets/design.css` | 136892 | `db0784355f76df449eb9ab8941f1a188b6307bafda4b3249516f7202b6e88604` |
| Framework / `lib/net10.0/Flourish.Blazor.Framework.dll` | 791040 | `a3017fb2b67bb449e4f1b78717fe0781b5e8ebb8d7b6c1240664d2dd5af4df46` |
| Framework / `staticwebassets/controls.js` | 49584 | `7f0711bb84cd83b641c373e35c7e95080e4a298890679dc4798d2cf3b403159f` |
| Framework / `staticwebassets/framework.css` | 102412 | `e8b6f58e56012223acd808d26a5748f7c0dc138b3420ba4bdf39370581314f3b` |
| Framework / `staticwebassets/icons/material/MaterialSymbolsOutlined.common.woff` | 125404 | `098865209c2ad1173ba59feb02fbb232cb69cb56155cdd263999d84f0a6c65c1` |
| Framework / `staticwebassets/icons/material/MaterialSymbolsOutlined.woff2` | 4003088 | `96c9913f4d415adeefcc3fa13f4f75912648f6f426be8b50e5e20c035b17cdf3` |
| Framework / `staticwebassets/patterns/header-layout.js` | 11050 | `2eac112ff5f06aad443df5073be724a69e87449bd3c9360d394db70d4659eb64` |
| Framework / `staticwebassets/patterns/primary-navigation-preview.js` | 17644 | `ed0e793d9d97788f515ed5880b00ef3e210be62e3ffb123f3ee700c89a59efcd` |
| Framework / `staticwebassets/patterns/save-changes-bar.js` | 10607 | `064c8fed6bbdfbbfd7e2f44acb85a906c635bf4ef6f37b49956350b9cc8406c6` |
| Framework / `staticwebassets/patterns/surfaces.js` | 4453 | `289218875cd68d9c36e17dfbd2df309f57814dc87d96314a558158a68a5ee3a7` |
| Framework / `staticwebassets/primitives/input-behaviors.js` | 3920 | `9a16206034d09ed972ef3ea8004e379fff487c97338a8b18a985eb8f1a5d6a76` |
| Framework / `staticwebassets/primitives/primary-navigation-item.js` | 5155 | `72040afbb84665c74056eb6ea003a0fc3e51341416a21c7eec4f7424113193f0` |
| Framework / `staticwebassets/primitives/reference-dropdown.js` | 4236 | `edc099bc383c6fa508a1455a972243e24444d958397a6a13615a162de4a5f1c2` |
| Framework / `staticwebassets/shell.js` | 7467 | `fbab6783433c57add981c092af3fd40f1fb4a22adaac229f873519a477a6717e` |

### Historical .14 archive and asset evidence

Candidate **1.4.4-preview.shell.20261027.14** is frozen in `artifacts/admin-account-and-navigation-20261029/nuget`. It adds only the primary-preview text-container stretch rule, making the existing ellipsis work for long captions. Public components, parameters, JavaScript, fonts and external dependencies remain unchanged. The six-package manifest, twelve graphs and 33 external package/version pairs are retained. All 13 delivered assets match source/Release bytes; .13 archive hashes are unchanged.

Release passed with zero warnings/errors. Framework 487/487, Gallery 8653/8653, CSS bundle 21/21 and focused preview Node checks 42/42 passed. No Computer Use or real-browser geometry checks were performed. The first CSS wrapper incorrectly propagated an expected negative-fixture exit code; the corrected script-success wrapper passed all 21 checks.

| Candidate archive | SHA-256 |
| --- | --- |
| Arkheide.Flourish.Blazor.1.4.4-preview.shell.20261027.14.nupkg | `bc8a9b8646cff92bc34a7da0ac25fdda084c6a7343c86ad42ee05c739a3759d7` |
| Arkheide.Flourish.Blazor.Abstract.1.4.4-preview.shell.20261027.14.nupkg | `5e7a41d707b7a1ed5a0923f82fd12605519fdc488ef501ce4eb237ddde493e92` |
| Arkheide.Flourish.Blazor.Design.1.4.4-preview.shell.20261027.14.nupkg | `1e336299f1c1c46a2ce438f7dff13f065add69e8ebac9b802cadd409a668f574` |
| Arkheide.Flourish.Blazor.Framework.1.4.4-preview.shell.20261027.14.nupkg | `8f31bef0657d2ef2660f4daf063624456a5aefa6f71601fe5d61b348cfdcb68d` |
| Arkheide.Flourish.Core.1.4.4-preview.shell.20261027.14.nupkg | `80bec2b9995e55b4590822024ede531e3cacbff3802f7b425f08283db3d407f6` |
| Arkheide.Flourish.Extensions.Culture.Blazor.1.4.4-preview.shell.20261027.14.nupkg | `3cd32474d7adcf3076a8f5af9fa7a625e42e18d84450257c99767bc6b8147550` |

| Delivered resource | Bytes | SHA-256 |
| --- | --- | --- |
| Framework / `staticwebassets/framework.css` | 101059 | `92e2a4ba7591944d2adda6552d36713d7f93aa1d58f8927a388e3ec8b367da4b` |
| Design / `staticwebassets/design.css` | 136624 | `e54bbb363c384773a39c0191dec146ba8a1293af33c8111623768fe03f3c6eaa` |
| Framework / `staticwebassets/patterns/primary-navigation-preview.js` | 17644 | `ed0e793d9d97788f515ed5880b00ef3e210be62e3ffb123f3ee700c89a59efcd` |
| Framework / `staticwebassets/primitives/primary-navigation-item.js` | 5155 | `72040afbb84665c74056eb6ea003a0fc3e51341416a21c7eec4f7424113193f0` |
| Framework / `staticwebassets/primitives/input-behaviors.js` | 3920 | `9a16206034d09ed972ef3ea8004e379fff487c97338a8b18a985eb8f1a5d6a76` |
| Framework / `staticwebassets/patterns/surfaces.js` | 4453 | `289218875cd68d9c36e17dfbd2df309f57814dc87d96314a558158a68a5ee3a7` |
| Framework / `staticwebassets/shell.js` | 7467 | `fbab6783433c57add981c092af3fd40f1fb4a22adaac229f873519a477a6717e` |
| Framework / `staticwebassets/patterns/header-layout.js` | 11050 | `2eac112ff5f06aad443df5073be724a69e87449bd3c9360d394db70d4659eb64` |
| Framework / `staticwebassets/icons/material/MaterialSymbolsOutlined.common.woff` | 125404 | `098865209c2ad1173ba59feb02fbb232cb69cb56155cdd263999d84f0a6c65c1` |
| Framework / `staticwebassets/icons/material/MaterialSymbolsOutlined.woff2` | 4003088 | `96c9913f4d415adeefcc3fa13f4f75912648f6f426be8b50e5e20c035b17cdf3` |
| Framework / `lib/net10.0/Flourish.Blazor.Framework.dll` | 777216 | `2bcbd523c971139c66dd13a4844da14371435f0613245296b941473dbd579fd9` |
| Framework / `staticwebassets/controls.js` | 49584 | `7f0711bb84cd83b641c373e35c7e95080e4a298890679dc4798d2cf3b403159f` |
| Framework / `staticwebassets/primitives/reference-dropdown.js` | 4236 | `edc099bc383c6fa508a1455a972243e24444d958397a6a13615a162de4a5f1c2` |

### Historical .13 candidate archive and asset evidence

Candidate **1.4.4-preview.shell.20261027.13**, based on `aaac88c`, is frozen in `artifacts/dropdown-width-and-admin-navigation-20261028/nuget`. Six package IDs and internal versions remain aligned; twelve producer graphs retain 33 external package/version pairs, Essential 1.4.0 and the existing SDK/runtime requirements. No maintained source file, public component/parameter or external dependency is added.

The final twelve-project Release build passed with zero warnings/errors; Framework 487/487, Gallery 8653/8653 and CSS bundle 21/21 passed. Scoped DOM/CSS checks passed 176/176: popup/native CSS 66, controls DOM 82, action-menu DOM-only 28. Fourteen physical-browser checks were intentionally not run; no Computer Use or browser geometry measurement was performed.

| Candidate archive | SHA-256 |
| --- | --- |
| Arkheide.Flourish.Blazor.1.4.4-preview.shell.20261027.13.nupkg | `fc9692559a62024625e66b1555c9f10d448d75020d0cf039cef37cba549e4dbd` |
| Arkheide.Flourish.Blazor.Abstract.1.4.4-preview.shell.20261027.13.nupkg | `e764636be9b0b4ae8cd7fcf7eb6dd47571c6e24c9335929b98dbb8e446ead6bc` |
| Arkheide.Flourish.Blazor.Design.1.4.4-preview.shell.20261027.13.nupkg | `d841354f9df6c68f76a34ac4e1db063452eaea644fa61960d55e39c18f237377` |
| Arkheide.Flourish.Blazor.Framework.1.4.4-preview.shell.20261027.13.nupkg | `d945dc89045ce6be3f44dff0e965ac09be5c27cd0f5d40eef292dce6517805f4` |
| Arkheide.Flourish.Core.1.4.4-preview.shell.20261027.13.nupkg | `f9dff612df509eb7740978caf55e70f16ac8cdb1e25f651f24d1ef22720b88f7` |
| Arkheide.Flourish.Extensions.Culture.Blazor.1.4.4-preview.shell.20261027.13.nupkg | `75512ec489165eed0cc1331a6db429537ad4b7a0c670ad283ad4b37ad24c84c0` |

| Delivered resource | Bytes | SHA-256 |
| --- | --- | --- |
| Framework / `staticwebassets/framework.css` | 101037 | `d6981c2b1836c9dc7717be3598dbeb559e1089c7ab85c1fb0ef538478002c14e` |
| Design / `staticwebassets/design.css` | 136624 | `e54bbb363c384773a39c0191dec146ba8a1293af33c8111623768fe03f3c6eaa` |
| Framework / `staticwebassets/patterns/primary-navigation-preview.js` | 17644 | `ed0e793d9d97788f515ed5880b00ef3e210be62e3ffb123f3ee700c89a59efcd` |
| Framework / `staticwebassets/primitives/primary-navigation-item.js` | 5155 | `72040afbb84665c74056eb6ea003a0fc3e51341416a21c7eec4f7424113193f0` |
| Framework / `staticwebassets/primitives/input-behaviors.js` | 3920 | `9a16206034d09ed972ef3ea8004e379fff487c97338a8b18a985eb8f1a5d6a76` |
| Framework / `staticwebassets/patterns/surfaces.js` | 4453 | `289218875cd68d9c36e17dfbd2df309f57814dc87d96314a558158a68a5ee3a7` |
| Framework / `staticwebassets/shell.js` | 7467 | `fbab6783433c57add981c092af3fd40f1fb4a22adaac229f873519a477a6717e` |
| Framework / `staticwebassets/patterns/header-layout.js` | 11050 | `2eac112ff5f06aad443df5073be724a69e87449bd3c9360d394db70d4659eb64` |
| Framework / `staticwebassets/icons/material/MaterialSymbolsOutlined.common.woff` | 125404 | `098865209c2ad1173ba59feb02fbb232cb69cb56155cdd263999d84f0a6c65c1` |
| Framework / `staticwebassets/icons/material/MaterialSymbolsOutlined.woff2` | 4003088 | `96c9913f4d415adeefcc3fa13f4f75912648f6f426be8b50e5e20c035b17cdf3` |
| Framework / `lib/net10.0/Flourish.Blazor.Framework.dll` | 777216 | `a580049599c515664ec3e35c7b80e3a3c68269b890c34b672d766219391493fb` |
| Framework / `staticwebassets/controls.js` | 49584 | `7f0711bb84cd83b641c373e35c7e95080e4a298890679dc4798d2cf3b403159f` |
| Framework / `staticwebassets/primitives/reference-dropdown.js` | 4236 | `edc099bc383c6fa508a1455a972243e24444d958397a6a13615a162de4a5f1c2` |

All thirteen packaged CSS/JS/font/DLL resources equal maintained source or actual Release outputs. This capture additionally checks controls.js/reference-dropdown.js, which are changed in this task. Header-layout.js, primary-preview JS, both fonts and previous .12 archives remain unchanged. Evidence is under `artifacts/dropdown-width-and-admin-navigation-20261028/`: build.log/build-final.log, component/Gallery/DOM/CSS logs, pack.log, package-verification.log, package-hashes.json and package-delivery.json. See [trigger-floor design, cleanup and manual acceptance](1_bugreports/2026-10-28_dropdown-trigger-width-floor.md). No public publication, deployment, cache clearing or old-feed replacement occurred.

## Historical release and preview records

### Historical shell candidate .12 archive and asset evidence

Candidate **1.4.4-preview.shell.20261027.12**, based on committed shell source `c1ae486`, is frozen in `artifacts/dashboard-preview-motion-20261028/nuget`. It retains six package IDs, twelve project graphs/33 external package-version pairs, Essential 1.4.0 and the existing SDK/runtime requirements. No external dependency or maintained source file/public parameter was added.

The preview now prefers 15–18rem, caps itself to retain a visible quarter of a vertical secondary rail and follows secondary size/node changes. The same popover animates width/caption opacity for 180ms with reversible closing, reduced-motion and forced-cleanup paths. Original icon anchors, narrow layout reservation, local-only hover, native links and header/font behavior remain.

Final-source twelve-project Release passed with zero warnings/errors. Framework 487/487, Gallery 8653/8653, Node 79/79 (preview 37/header 9/native CSS 33) and CSS bundle 21/21 passed. No Computer Use or physical browser measurement ran. Six archives pass the existing package-set verifier; pack retains existing informational readme notices.

| Candidate archive | SHA-256 |
| --- | --- |
| Arkheide.Flourish.Blazor.1.4.4-preview.shell.20261027.12.nupkg | `a936bb19730879484dbfe5c729c666924165361cc504d6e76976a486e9fbfdc9` |
| Arkheide.Flourish.Blazor.Abstract.1.4.4-preview.shell.20261027.12.nupkg | `5e5af149845521180592ddbd40ccad81a7e394c050e224e69901de0322c52a70` |
| Arkheide.Flourish.Blazor.Design.1.4.4-preview.shell.20261027.12.nupkg | `62b5814b6ff6971df576b332de996c0dfe2f3c9285dfe4d94032bccf9bb446c6` |
| Arkheide.Flourish.Blazor.Framework.1.4.4-preview.shell.20261027.12.nupkg | `486333debf44eb0dff5ce4cbfbe73cf5d76d22ca2fc7fc1b5d8e17383dd9ffb1` |
| Arkheide.Flourish.Core.1.4.4-preview.shell.20261027.12.nupkg | `8f9cf6815d0fba31197f5635fb8268a599937559e9268deb252264714489d618` |
| Arkheide.Flourish.Extensions.Culture.Blazor.1.4.4-preview.shell.20261027.12.nupkg | `449beedc64e66275170e33cbf2c00879172aa9516e164022b40f960024f53041` |

| Delivered resource | Bytes | SHA-256 |
| --- | --- | --- |
| Framework / `staticwebassets/framework.css` | 100472 | `c2f98d33e257947eecbd532d55f9c1320022d9141b081e5eb0e0bb5319435b07` |
| Design / `staticwebassets/design.css` | 136624 | `e54bbb363c384773a39c0191dec146ba8a1293af33c8111623768fe03f3c6eaa` |
| Framework / `staticwebassets/patterns/primary-navigation-preview.js` | 17644 | `ed0e793d9d97788f515ed5880b00ef3e210be62e3ffb123f3ee700c89a59efcd` |
| Framework / `staticwebassets/primitives/primary-navigation-item.js` | 5155 | `72040afbb84665c74056eb6ea003a0fc3e51341416a21c7eec4f7424113193f0` |
| Framework / `staticwebassets/primitives/input-behaviors.js` | 3920 | `9a16206034d09ed972ef3ea8004e379fff487c97338a8b18a985eb8f1a5d6a76` |
| Framework / `staticwebassets/patterns/surfaces.js` | 4453 | `289218875cd68d9c36e17dfbd2df309f57814dc87d96314a558158a68a5ee3a7` |
| Framework / `staticwebassets/shell.js` | 7467 | `fbab6783433c57add981c092af3fd40f1fb4a22adaac229f873519a477a6717e` |
| Framework / `staticwebassets/patterns/header-layout.js` | 11050 | `2eac112ff5f06aad443df5073be724a69e87449bd3c9360d394db70d4659eb64` |
| Framework / `staticwebassets/icons/material/MaterialSymbolsOutlined.common.woff` | 125404 | `098865209c2ad1173ba59feb02fbb232cb69cb56155cdd263999d84f0a6c65c1` |
| Framework / `staticwebassets/icons/material/MaterialSymbolsOutlined.woff2` | 4003088 | `96c9913f4d415adeefcc3fa13f4f75912648f6f426be8b50e5e20c035b17cdf3` |
| Framework / `lib/net10.0/Flourish.Blazor.Framework.dll` | 777216 | `d87406712f6ea48e8a6df6c31d159c59c41c9040e5adef91437a992dcc923ce2` |

All eleven packaged CSS/JS/font/DLL assets equal maintained source or actual Release outputs. Header-layout.js and both fonts match .11 byte-for-byte, and all previous .11 archives were rehashed unchanged. Evidence is under `artifacts/dashboard-preview-motion-20261028/`: build-frozen.log, build-final.log, framework-tests.log, gallery-tests.log, node-tests-final.log, css-bundle-tests.log, pack.log, package-verification.log, package-hashes.json and package-delivery.json. See [width, motion lifecycle and manual acceptance](1_bugreports/2026-10-28_primary-preview-width-and-motion.md). No public publication, deployment, cache clearing or old-feed replacement was performed.

### Historical shell candidate .11 archive and asset evidence

Candidate **1.4.4-preview.shell.20261027.11** is frozen in the independent `artifacts/header-preview-alignment-20261028/nuget` feed. Six archives pass the existing identity/dependency/required-asset verifier; twelve producer graphs retain 33 external package/version pairs, Essential 1.4.0 and the same SDK/runtime requirements. Five of these archives are consumed by Colligere; its cache and closure evidence belongs to that repository.

This follow-up fixes ShellHeader's actual data-alignment action-group sizing and both wrapping levels in measured fit modes; it retains stacked/no-script wrapping and observes right-wing content/enhancement changes. Primary preview neutralizes native popover box defaults, preserves block insets and per-row geometry, aligns each actual icon and handles DOM collection replacement without layout reads. It adds no maintained source file or public API; the source tree remains 714 files/121 directories.

Frozen-source 12-project Release passed with zero warnings/errors; Framework 487/487, Gallery 8653/8653, focused Node 62/62 (header 9, native CSS 33, preview 20) and CSS bundle 21/21 passed. No Computer Use or physical-browser measurements were performed. No old archive was overwritten, cache cleared, external dependency installed/upgraded, public package published, Git commit created or service deployed. Pack retains existing informational package-readme notices.

| Candidate archive | SHA-256 |
| --- | --- |
| Arkheide.Flourish.Blazor.1.4.4-preview.shell.20261027.11.nupkg | `deb66a1df423d6acd3b970021c2ff4f89c4e2b6f11076f419f9f37658de015da` |
| Arkheide.Flourish.Blazor.Abstract.1.4.4-preview.shell.20261027.11.nupkg | `50f2197409ab1c40f171bfbdd48d44e7572b9b5f9178738f42a73fb7fbe752e6` |
| Arkheide.Flourish.Blazor.Design.1.4.4-preview.shell.20261027.11.nupkg | `50e44423b96285b4f9e8a29ed0545286cbf4afa4161b98e06de0df63de703e18` |
| Arkheide.Flourish.Blazor.Framework.1.4.4-preview.shell.20261027.11.nupkg | `0df33857b1193a92b2f5f0fe497a5bdba2c94b102bdf3c4c04b6c66275272edf` |
| Arkheide.Flourish.Core.1.4.4-preview.shell.20261027.11.nupkg | `21155cffbb0bdfe98ffae20b2b3be2f56f97336925c00c05afccae84a71799ef` |
| Arkheide.Flourish.Extensions.Culture.Blazor.1.4.4-preview.shell.20261027.11.nupkg | `7d2cb410c333cff7c997db47fe2d78783305de0b1ef2d61e32dec9cf20f3fa67` |

| Delivered resource | Bytes | SHA-256 |
| --- | --- | --- |
| Framework / `staticwebassets/framework.css` | 99094 | `9f77ed7aae7d22d885be6db41aeb7dadd044bcac62ebfa493bf35d23e132ebeb` |
| Design / `staticwebassets/design.css` | 136543 | `6314fd2fc6a6fd077fc3fee101db2d745768e2c9445c9806d17df14ef346ab6c` |
| Framework / `staticwebassets/patterns/primary-navigation-preview.js` | 12049 | `72a1bcf52505db9791cc56005e949550c8ae54a6ad9a263fa99c65010a747ac0` |
| Framework / `staticwebassets/primitives/primary-navigation-item.js` | 5155 | `72040afbb84665c74056eb6ea003a0fc3e51341416a21c7eec4f7424113193f0` |
| Framework / `staticwebassets/primitives/input-behaviors.js` | 3920 | `9a16206034d09ed972ef3ea8004e379fff487c97338a8b18a985eb8f1a5d6a76` |
| Framework / `staticwebassets/patterns/surfaces.js` | 4453 | `289218875cd68d9c36e17dfbd2df309f57814dc87d96314a558158a68a5ee3a7` |
| Framework / `staticwebassets/shell.js` | 7467 | `fbab6783433c57add981c092af3fd40f1fb4a22adaac229f873519a477a6717e` |
| Framework / `staticwebassets/patterns/header-layout.js` | 11050 | `2eac112ff5f06aad443df5073be724a69e87449bd3c9360d394db70d4659eb64` |
| Framework / `staticwebassets/icons/material/MaterialSymbolsOutlined.common.woff` | 125404 | `098865209c2ad1173ba59feb02fbb232cb69cb56155cdd263999d84f0a6c65c1` |
| Framework / `staticwebassets/icons/material/MaterialSymbolsOutlined.woff2` | 4003088 | `96c9913f4d415adeefcc3fa13f4f75912648f6f426be8b50e5e20c035b17cdf3` |
| Framework / `lib/net10.0/Flourish.Blazor.Framework.dll` | 777216 | `b7060734ebad93b4823f4b1d9f2020ba796305785440f990f96a15808ab99078` |

All eleven packaged CSS/JS/font/assembly resources equal current maintained source or actual Release outputs. Both fonts match .10 byte-for-byte; the new header/preview corrections are present in the Release bundles and modules. Previous .10 archives were also rehashed unchanged. Evidence is under `artifacts/header-preview-alignment-20261028/`: build.log, build-frozen.log, framework-tests.log, gallery-tests.log, node-tests-final.log, css-bundle-tests.log, pack.log, package-verification.log, package-hashes.json and package-delivery.json. See [diagnosis, preserved contracts and manual acceptance](1_bugreports/2026-10-28_header-action-row-and-preview-box-alignment.md).

### Historical shell candidate .10 archive and asset evidence

The local **1.4.4-preview.shell.20261027.10** candidate is frozen in `artifacts/primary-preview-20261028/nuget`, preserving .9 and earlier feeds. The same six Core/Blazor nuspec identities/internal edges pass `scripts/Verify-PackageSet.ps1`; Essential remains **1.4.0**. Twelve producer assets graphs still resolve **33 external package/version pairs**. SDK **10.0.400**, web assets **10.0.11**, and existing native browser APIs remain; no external dependency, font or tool was installed/upgraded.

The final 12-project Release build passed with zero warnings/errors. Framework **487/487** (83 components, 775 parameter rows/363 documented defaults), Gallery **8,653/8,653**, focused Node **76/76** and CSS bundle **21/21** passed. Node includes the new preview controller **10/10**, complete CSS-cascade **33/33** and retained header **9/9** plus existing heading/surface/navigation checks; the section/top-control script reports 26 internal checks. No Computer Use, physical browser measurement or production behavior is claimed. Consumer restore/runtime acceptance is recorded separately by its owner.

The repo has no debug-handler, and Prepare remains stable-only. The authorized explicit preview build/pack route generated six new archives with existing informational missing-package-readme notices. The verifier now explicitly requires the new preview JS module; source CSS partials remain inlined. No archive was replaced; there was no commit, remote tag/publication or deployment.

| Candidate archive | SHA-256 |
| --- | --- |
| Arkheide.Flourish.Blazor.1.4.4-preview.shell.20261027.10.nupkg | `108bc265cedf6fbde1cc7513b141a89096e06aa022f85ce4f931b13d0e31fabc` |
| Arkheide.Flourish.Blazor.Abstract.1.4.4-preview.shell.20261027.10.nupkg | `f988947aa4bbbc7994d4c1f3c1630ee3246cae42ee6aa44354eb417d23d25329` |
| Arkheide.Flourish.Blazor.Design.1.4.4-preview.shell.20261027.10.nupkg | `a70bdabe614893775a4f62a04e2bd790202cafe3eb38237cd8873d58772fdd50` |
| Arkheide.Flourish.Blazor.Framework.1.4.4-preview.shell.20261027.10.nupkg | `ce3e1b4afd679fdf73c5760cc07aefe61aace43e1609d6b293656cd66e314fd6` |
| Arkheide.Flourish.Core.1.4.4-preview.shell.20261027.10.nupkg | `59f3adb93f8d0974fd06eeff7fadbaf1e773285380e17abcd0e43bb60b967d3c` |
| Arkheide.Flourish.Extensions.Culture.Blazor.1.4.4-preview.shell.20261027.10.nupkg | `7ba9e4a1cf424da91a6697f9b6497d0445854bbbb5f9378bff6e367be51b0244` |

| Delivered resource | Bytes | SHA-256 |
| --- | --- | --- |
| Framework / `staticwebassets/framework.css` | 97336 | `5e347d4c171e5bfb9c1bc63d3fe00f9396b8dbe2b31d6e5aecba72a0175725e0` |
| Design / `staticwebassets/design.css` | 136827 | `98f1ba0ed9abe7140c0c5699c6c23f788a4958842145a7c034f5ad447b7ecd3d` |
| Framework / `staticwebassets/patterns/primary-navigation-preview.js` | 9220 | `0767ad706cefd6e709d35e2ab8d8b60051e4b4200847e330b2017ad131ccd6dc` |
| Framework / `staticwebassets/primitives/primary-navigation-item.js` | 5155 | `72040afbb84665c74056eb6ea003a0fc3e51341416a21c7eec4f7424113193f0` |
| Framework / `staticwebassets/primitives/input-behaviors.js` | 3920 | `9a16206034d09ed972ef3ea8004e379fff487c97338a8b18a985eb8f1a5d6a76` |
| Framework / `staticwebassets/patterns/surfaces.js` | 4453 | `289218875cd68d9c36e17dfbd2df309f57814dc87d96314a558158a68a5ee3a7` |
| Framework / `staticwebassets/shell.js` | 7467 | `fbab6783433c57add981c092af3fd40f1fb4a22adaac229f873519a477a6717e` |
| Framework / `staticwebassets/patterns/header-layout.js` | 10939 | `dbabc63c91dd8a28778c2ffebb565c04c7444b9b5bce5844e89bfb9e4460c238` |
| Framework / `staticwebassets/icons/material/MaterialSymbolsOutlined.common.woff` | 125404 | `098865209c2ad1173ba59feb02fbb232cb69cb56155cdd263999d84f0a6c65c1` |
| Framework / `staticwebassets/icons/material/MaterialSymbolsOutlined.woff2` | 4003088 | `96c9913f4d415adeefcc3fa13f4f75912648f6f426be8b50e5e20c035b17cdf3` |
| Framework / `lib/net10.0/Flourish.Blazor.Framework.dll` | 777216 | `47fc5eab2e1f0b3d845834eab4b148cf52f045d279e0f58711fa7940fe6f4a08` |

All eleven delivered resources equal maintained source or actual Release bundle/assembly outputs. The .9 header-layout.js and both fonts additionally match their previous immutable archives byte for byte. Both preview CSS partials are in the existing bundles; the shared controller adds one normal same-origin ESM resource and uses no new third-party browser dependency. Evidence: `artifacts/primary-preview-20261028/{restore.log,build.log,build-final.log,build-frozen.log,node-tests-final.log,preview-tests.log,framework-tests.log,gallery-tests.log,css-bundle-tests.log,pack.log,package-verification.log,package-hashes.json,package-delivery.json}`. See [primary-navigation design, boundary fixes and manual checks](1_bugreports/2026-10-28_weak-primary-navigation-preview.md).

### Historical shell candidate .9 archive and asset evidence

The local **1.4.4-preview.shell.20261027.9** candidate is frozen in `artifacts/header-height-20261028/nuget`, following .8 and committed shell .7 source `feee3ea`. All six nuspec identities/internal dependency edges passed `scripts/Verify-PackageSet.ps1`; Essential remains **1.4.0**. Twelve producer assets graphs still contain **33 external package/version pairs**. The SDK remains **10.0.400**, with web assets package **10.0.11**. No package, SDK/runtime, font or tool was installed/upgraded.

The 12-project Release build passed with zero warnings/errors. Framework **485/485**, header mock-DOM **9/9**, complete native-select/CSS-cascade **32/32** and CSS bundle **21/21** passed. The full business-header mock reserves actual identity, enhanced native-language and POST/logout targets, including already-constrained 26px bounding boxes recovering their CSS 48px bases, a 318px action-wing requirement, and the 801/802px genuine fit boundary. Complete Framework/Design CSS independently confirms both NavigationSurface and ContentSurface targets and their ordinary 68px height budget; it retains wrapping instead of imposing height or clipping. This is source/mock evidence, not measured browser geometry. Consumer integration is recorded separately by its owner.

No debug-handler exists, and Prepare remains stable-only. The authorized explicit local preview build/pack route produced these six independent .9 archives; no .8 or older archive was overwritten. No commit, remote publication, tag or deployment occurred.

| Candidate archive | SHA-256 |
| --- | --- |
| Arkheide.Flourish.Blazor.1.4.4-preview.shell.20261027.9.nupkg | `d5382ae71c35e99de5e0c5447acbea7137daa7144c3a4fdc050563d9b1c5dd89` |
| Arkheide.Flourish.Blazor.Abstract.1.4.4-preview.shell.20261027.9.nupkg | `bb26f216e0c875f3081fdad5e07394b3c7b226824e1dcf61bf933e240c8746a8` |
| Arkheide.Flourish.Blazor.Design.1.4.4-preview.shell.20261027.9.nupkg | `30524d0892c645a35847f07ba841f2f5c1322aa79b7f12cd15e20e2a89061cb6` |
| Arkheide.Flourish.Blazor.Framework.1.4.4-preview.shell.20261027.9.nupkg | `65163fa5f285d575c3baf288c5efb3992228ff2309b81e62ea557d251aeb7589` |
| Arkheide.Flourish.Core.1.4.4-preview.shell.20261027.9.nupkg | `c1543ff85a35df7205b1a4cfcc697f8227f4e205c09fa1495306efd57665d888` |
| Arkheide.Flourish.Extensions.Culture.Blazor.1.4.4-preview.shell.20261027.9.nupkg | `ce7247217fa6292a47221e5e773651aa7b736386f5192930b7a6629283489115` |

| Delivered resource | Bytes | SHA-256 |
| --- | --- | --- |
| Framework / `staticwebassets/framework.css` | 94404 | `c890ae895db285f214bb6501f6a16fbb98f5672f5b7a862f7cadfbf95befee0f` |
| Design / `staticwebassets/design.css` | 135502 | `90b5c131e808498e0f58d6f219b3dc36c938b15977a62eafeb0041d7d65db089` |
| Framework / `staticwebassets/patterns/header-layout.js` | 10939 | `dbabc63c91dd8a28778c2ffebb565c04c7444b9b5bce5844e89bfb9e4460c238` |
| Framework / `staticwebassets/icons/material/MaterialSymbolsOutlined.common.woff` | 125404 | `098865209c2ad1173ba59feb02fbb232cb69cb56155cdd263999d84f0a6c65c1` |
| Framework / `staticwebassets/icons/material/MaterialSymbolsOutlined.woff2` | 4003088 | `96c9913f4d415adeefcc3fa13f4f75912648f6f426be8b50e5e20c035b17cdf3` |

The header module matches maintained source. Framework/Design CSS equal actual bundled Release output and the immutable .8 archives; both icon fonts also remain byte-identical to .8. Only header-layout.js changes among these browser assets. The .8 fitted-menu and 24px information-inset features remain, as do the .7 common/full font partition and dynamic-icon fallback. Evidence: `artifacts/header-height-20261028/{restore.log,build.log,header-css-tests.log,framework-tests.log,css-bundle-tests.log,pack.log,package-verification.log,package-hashes.json,package-delivery.json}`. See the [business-header width-budget report](1_bugreports/2026-10-28_business-header-control-width-budget.md).

### Historical shell candidate .8 archive and asset evidence

The local **1.4.4-preview.shell.20261027.8** candidate is frozen in `artifacts/header-information-20261028/nuget`, based on committed .7 source `feee3ea`. All six nuspec identities/internal dependency edges passed `scripts/Verify-PackageSet.ps1`; Essential remains **1.4.0**. Twelve producer assets graphs still contain **33 external package/version pairs**. The selected SDK remains **10.0.400** and its web assets package **10.0.11**; no package, SDK/runtime, font or tool was installed or upgraded for this refinement.

The 12-project Release build passed with zero warnings/errors. Existing Framework checks **485/485**, header mock-DOM **9/9**, native-select/full CSS-cascade **31/31** and CSS bundle **21/21** passed. The header checks cover page-wide stretched menu wrappers, both-wing fit, real 663/664px overflow boundaries, translated-control resizing, exact/fitted/stacked modes, RTL, footer page-edge preservation and observer cleanup. Information cascade checks retain H1/CopyText typography, natural height and action-cell geometry while increasing only information-cell padding. No browser or Computer Use validation ran. Consumer integration results are maintained by the consuming project; no .8 runtime acceptance is asserted here.

The repository has no debug-handler entry, and the existing Prepare route is stable-only. Following its documented preview direction, .8 was explicitly built and packed into its independent local feed, then passed the existing package verifier. The .7 and older feeds/archives remain unchanged. There was no commit, remote publication, tag or deployment.

| Candidate archive | SHA-256 |
| --- | --- |
| Arkheide.Flourish.Blazor.1.4.4-preview.shell.20261027.8.nupkg | `1063e57332ece43a62854761dd8a30ade2965d80baa18947fbb8c2057098b695` |
| Arkheide.Flourish.Blazor.Abstract.1.4.4-preview.shell.20261027.8.nupkg | `c25071c627e0dadb596683ad77e890ef3a087debf9c432db98bbcd66a5faeb17` |
| Arkheide.Flourish.Blazor.Design.1.4.4-preview.shell.20261027.8.nupkg | `2f160aae96f91fda7ca5ff0ba8370aebc44b395235db90cc7912bb4e59957d13` |
| Arkheide.Flourish.Blazor.Framework.1.4.4-preview.shell.20261027.8.nupkg | `c080bc68eeeb14284387940cb2c46edc676052ba870769f99836b1e95dd93b13` |
| Arkheide.Flourish.Core.1.4.4-preview.shell.20261027.8.nupkg | `ec69f9617f6d1625a80698e05ba3931884e2f804646b3bd881f29df1ee493ada` |
| Arkheide.Flourish.Extensions.Culture.Blazor.1.4.4-preview.shell.20261027.8.nupkg | `0b7479e86b81cf6361f5e31cbc9422eacea18f0279a5150f005a77899b5e33c4` |

| Delivered resource | Bytes | SHA-256 |
| --- | --- | --- |
| Framework / `staticwebassets/framework.css` | 94404 | `c890ae895db285f214bb6501f6a16fbb98f5672f5b7a862f7cadfbf95befee0f` |
| Design / `staticwebassets/design.css` | 135502 | `90b5c131e808498e0f58d6f219b3dc36c938b15977a62eafeb0041d7d65db089` |
| Framework / `staticwebassets/patterns/header-layout.js` | 10340 | `c56db024c7056be1d99efc8dc0261fa4adcdb25a1acec12e7f3be08f9c22f131` |
| Framework / `staticwebassets/icons/material/MaterialSymbolsOutlined.common.woff` | 125404 | `098865209c2ad1173ba59feb02fbb232cb69cb56155cdd263999d84f0a6c65c1` |
| Framework / `staticwebassets/icons/material/MaterialSymbolsOutlined.woff2` | 4003088 | `96c9913f4d415adeefcc3fa13f4f75912648f6f426be8b50e5e20c035b17cdf3` |

The Framework/Design CSS bytes equal the actual bundled Release outputs, and header-layout.js equals maintained source. Both fonts equal source and the previous immutable .7 package byte for byte. The .7 common/full unicode-range selection and dynamic-icon compatibility remain unchanged. Evidence: `artifacts/header-information-20261028/{restore.log,build.log,header-css-tests.log,framework-tests.log,css-bundle-tests.log,pack.log,package-verification.log,package-hashes.json,package-delivery.json}`. See the [wide-header and information-inset report](1_bugreports/2026-10-28_wide-header-menu-and-information-insets.md).

### Historical shell candidate .7 archive and asset evidence

The local **1.4.4-preview.shell.20261027.7** candidate is packed in `artifacts/icon-subset-20261028/nuget`. All six nuspec identities/internal dependency edges passed scripts/Verify-PackageSet.ps1; Essential remains **1.4.0**. The 12 producer assets graphs contain **33 external package/version pairs**, unchanged in identity from the previous scoped candidate. The selected SDK remains **10.0.400** and its web assets package **10.0.11**; no SDK/runtime/tool/package installation occurred.

The producer Release build passed with **zero warnings/errors**. Framework **485/485**, new icon partition/variable-table regressions **3/3**, CSS bundle **21/21**, and existing ChangeLog **177 checks** passed. Regeneration is deterministic, all 78 default/Filled icon outlines match the original, and Windows WOFF decoding reproduces all generated SFNT tables. The package verifier initially rejected a mistakenly listed standalone subsets.css: this is a source partial intentionally inlined by the existing bundle contract, so only the verifier expectations were corrected; the six archives were not replaced. The consuming project reports a final 17-project Release build with zero warnings/errors, relevant Web checks **264/264** with zero skips, and all five consumed archive/cache metadata entries matching this new feed. Running-browser font selection remains separate acceptance. No browser timing, LCP/CWV, remote tag, publication or deployment is claimed.

Flourish has no debug-handler entry. The existing publish-helper.bat Prepare route was attempted and its stable-only guard rejected the preview before any package-directory cleanup; it explicitly directs local previews to build/verify packages separately. The candidate was therefore built and packed explicitly into this new feed, then checked through the existing package verifier. Older shell feeds/archives remain intact.

| Candidate archive | SHA-256 |
| --- | --- |
| Arkheide.Flourish.Blazor.1.4.4-preview.shell.20261027.7.nupkg | `3f072c0c9fefaffa037da50772f01cf80aed4ced4d3e410ad85fbc1a6fd1cc69` |
| Arkheide.Flourish.Blazor.Abstract.1.4.4-preview.shell.20261027.7.nupkg | `806a8b409881603b90634be94acfb0f136474c3ab99ce712ea2d9b5b636c8a49` |
| Arkheide.Flourish.Blazor.Design.1.4.4-preview.shell.20261027.7.nupkg | `b769bb5be531a799a02495289f887cd6151d599337ece2d31dfb8ce8e4dfa2fa` |
| Arkheide.Flourish.Blazor.Framework.1.4.4-preview.shell.20261027.7.nupkg | `7b763b0e5825b540d54b10ed464ad869e601b0a5c883394d6cf1b3e57df29f44` |
| Arkheide.Flourish.Core.1.4.4-preview.shell.20261027.7.nupkg | `9838cba3857344a32dbc8c935615bcf4f3210b75b1e174cef82c1f5b5564e2aa` |
| Arkheide.Flourish.Extensions.Culture.Blazor.1.4.4-preview.shell.20261027.7.nupkg | `465e824f801077a146fc23ca5c3d1b9770ac7202726d325406765587741f0670` |

| Delivered Framework resource | Bytes | SHA-256 |
| --- | --- | --- |
| `staticwebassets/framework.css` | 94281 | `bd71dc0e7ee200d1c80f05c43cffa3860ac6dcb84f08a5f3faecd0a9b5076728` |
| `staticwebassets/icons/material/MaterialSymbolsOutlined.common.woff` | 125404 | `098865209c2ad1173ba59feb02fbb232cb69cb56155cdd263999d84f0a6c65c1` |
| `staticwebassets/icons/material/MaterialSymbolsOutlined.woff2` | 4003088 | `96c9913f4d415adeefcc3fa13f4f75912648f6f426be8b50e5e20c035b17cdf3` |
| `staticwebassets/icons/material/common-icons.json` | 1006 | `1b87f43ad88e57e5f2eaf161d4138a10ed8ab60a2f18298e62d0aca3205bfac7` |
| `staticwebassets/icons/material/LICENSE.txt` | 11558 | `49bbe9114e49214df2ccc324cb3ac8d1d1aa1c3a0947f94c286765e86647b32e` |

All listed resources equal their maintained source or actual bundled Release output. The two font faces are in the single bundled framework.css; source subsets.css is not a separate runtime stylesheet. Common-only glyphs select **125,404 bytes**, a **96.87% reduction** from the previous 4,003,088-byte font requirement. Introducing an uncommon dynamic glyph legitimately selects the retained full file as well; that compatibility cost is explicit, and the same full URL is not duplicated into several faces. This request-selection conclusion follows the nonoverlapping unicode-range declarations; physical browser network acceptance remains manual.

Local evidence: `artifacts/icon-subset-20261028/{generation.json,regeneration-verification.json,font-tests.log,framework-tests.log,css-bundle-tests.log,build.log,package-verification.log,package-hashes.json,font-delivery.json,publish-helper-prepare.log}`. See the [common-font report](1_bugreports/2026-10-28_common-icon-font-subset.md) for regeneration, standards references, boundary cases and manual checks. The .6 and older archive/runtime evidence below is historical.

### Historical shell candidate .6 archive and asset evidence

Final read-only capture at **2026-10-27T19:14:12.339739Z** confirms all six **1.4.4-preview.shell.20261027.6** archives in `artifacts/shell-20261027/nuget`, including nuspec identities and same-version internal dependencies; the Culture bridge retains Essential 1.4.0. The [compact brand Button and header-height report](1_bugreports/2026-10-27_compact-brand-button-and-header-height.md) records this candidate's scope and manual checks. Candidate .5 hashes below remain historical.

The coordinated **12-project producer Release build and six-package pack completed with zero build warnings/errors**. Framework checks passed **485/485**, header mock-DOM checks **7/7**, and native-select/CSS-cascade checks **31/31**. The 17-project consumer restore and Release build completed with zero warnings/errors; five focused consumer header classes passed **95/95**, with no failed or skipped cases. Gallery JSON parsed and Gallery compiled, but its suite and broader whole-solution suites were not rerun; the .5 Gallery result remains historical. The consumer logs are `../Colligere/artifacts/compact-brand-20261027/build.log` and its focused/TRX output. The owned unified restart completed: all eight readiness probes returned HTTP 200, and the running Debug dependency manifest selects the same five .6 Flourish IDs. Physical browser measurements, remote publication, tag and deployment are not asserted.

The **12 producer graphs contain 33 external package/version pairs**. The **17 consumer graphs contain 168 pairs**, including five Flourish IDs at .6 and Essential Core/Blazor/Generator at **1.4.0**. Comparing the actual .5 and .6 package sets changes only those five Flourish versions; every other identity is unchanged. Five consumed archives match the ordinary global NuGet cache byte for byte, and all five acquisition metadata records name this local feed. The umbrella archive is packed but not consumed by Colligere. The selected local SDK remains **10.0.400**, and the resolved web-assets package is **Microsoft.AspNetCore.App.Internal.Assets/10.0.11**; no SDK/runtime installation was performed.

| Candidate archive | SHA-256 | Consumer acquisition |
| --- | --- | --- |
| Arkheide.Flourish.Blazor.1.4.4-preview.shell.20261027.6.nupkg | `b73ca62660d8531612e574888a1c5b6965d193a12810cbb6d89cbf01fce2df0a` | Umbrella not consumed by Colligere |
| Arkheide.Flourish.Blazor.Abstract.1.4.4-preview.shell.20261027.6.nupkg | `375eb4faafc9a0eb4770b7231ea4787e1f062cd5d703d2de33a00990a44aa975` | Matches ordinary consumer cache |
| Arkheide.Flourish.Blazor.Design.1.4.4-preview.shell.20261027.6.nupkg | `8fe086f860d0af9e4e4d23cf23d56875c38daf0595c2b3fab7a34893bc40c0d9` | Matches ordinary consumer cache |
| Arkheide.Flourish.Blazor.Framework.1.4.4-preview.shell.20261027.6.nupkg | `972989dc7797bd9b62179cf54916b16fab371e16c3291aeee56a2884cdb19c58` | Matches ordinary consumer cache |
| Arkheide.Flourish.Core.1.4.4-preview.shell.20261027.6.nupkg | `580930a8f3431daeec1755e2f5a05b80d5b42b6147ed91aeef99ec403a079f4e` | Matches ordinary consumer cache |
| Arkheide.Flourish.Extensions.Culture.Blazor.1.4.4-preview.shell.20261027.6.nupkg | `4bc4262cc106e00f1d1a5ba83f2c7f874d677ac81fd3036e2c5df92f10485344` | Matches ordinary consumer cache |

The six library entries below match **package ZIP = producer output = ordinary consumer cache**. Bundled CSS is compared with actual Release outputs; the Framework DLL confirms the compiled component assembly. The host culture-script row is its maintained source identity, outside Flourish packaging. All five library browser entries and the host culture script additionally match their saved HTTP 200 response bytes after that restart. The Framework DLL is a stored assembly check rather than an HTTP asset.

| Delivered resource | SHA-256 |
| --- | --- |
| Framework / `staticwebassets/framework.css` | `38a7bf184f27a13e7987c4e2790d921257a33b6b3c254ec9721195b749476fec` |
| Design / `staticwebassets/design.css` | `5878d76488e276bf74b4509a087f26077a54a09cb1a47f0e6c036e2aba7d8e0e` |
| Framework / `staticwebassets/patterns/header-layout.js` | `cc5af804cdd8f481c4c45947cc4429d149cb0b1b1e8edab2d1a37c8dd2c6a662` |
| Framework / `staticwebassets/controls.js` | `b685bbe4d2a83c62439e27d861128461c40c1cfad22624d090c3d57e1864bd5b` |
| Framework / `staticwebassets/presentation/progress.js` | `41a32212e750166151b27e14838e7fbbe82d21add6acbb1aab77f18310adbaa9` |
| Framework / `lib/net10.0/Flourish.Blazor.Framework.dll` | `fb00509a56757a6180897b501b27813db75fa86efe35f622f9dc06be3f754c2a` |
| Consumer / `wwwroot/culture-picker.js` | `170ba75da1cbd33baca62c0cde749cfeab17c9bdf1aef3540f5ebfaf9682346b` |

The independent package/cache/runtime capture is `artifacts/shell-20261027/dependency-provenance-6.json`. Current consumer readiness, asset and SSR captures are `../Colligere/artifacts/compact-brand-20261027/health.json`, `runtime-assets.json` and `runtime-markup.json`, with saved response files alongside them. Four public SSR roots confirm real Quiet brand Buttons, native 24px image attributes, three structured portal rows and Admin Center root navigation. These structure and byte checks do not measure a rendered 68px header or establish physical layout/interaction acceptance. The inspected consumer Web assets file is `../Colligere/src/Colligere.Web/obj/project.assets.json`, SHA-256 `c24f8cc47a9f9ffa74477abb9d77bb7f0c828d899b0cec90534830c01aaadee8`. Generated archives, caches, object files and captures remain excluded from the maintained tree. Older dated evidence below does not identify .6 bytes.

### Historical shell candidate .5 archive and asset evidence

Final read-only capture at **2026-10-27T18:56:40.405149Z** confirms all six **1.4.4-preview.shell.20261027.5** archives in `artifacts/shell-20261027/nuget`, including nuspec identities and same-version internal dependencies; the Culture bridge retains Essential 1.4.0. The [information grids and header-consistency report](1_bugreports/2026-10-27_information-grids-and-header-consistency.md) records this candidate's scope and manual checks. Candidate .4 hashes below remain historical.

The coordinated **12-project producer Release build and six-package pack completed with zero build warnings/errors**. Framework checks passed **484/484** and Gallery checks **8,645/8,645**; six header mock-DOM and 31 native/popup CSS-cascade checks passed. Eleven producer JS/MJS files passed syntax checks, and Gallery ChangeLog JSON parsed. The initial 483/484 Framework result was an empty-alt serialization assertion mismatch, fixed only in test source before the final pass. The 17-project consumer restore and final Release build completed with zero warnings/errors; its focused organization administration, details and onboarding regressions passed **229/229**. The final consumer logs are `../Colligere/artifacts/information-grid-20261027/build-final.log` and `focused-tests-final.log`. The coordinated unified restart completed: all eight readiness probes returned HTTP 200, and the running Debug dependency manifest selects the same five .5 Flourish IDs. Physical browser acceptance, remote publication, tag and deployment are not asserted.

The **12 producer graphs contain 33 external package/version pairs**. The **17 consumer graphs contain 168 pairs**, including five Flourish IDs at .5 and Essential Core/Blazor/Generator at **1.4.0**. Five actual consumed archives match the ordinary global NuGet cache byte for byte, and all five acquisition metadata records name this local feed. The umbrella archive is packed but not consumed by Colligere. The selected local SDK remains **10.0.400**, and the resolved web-assets package is **Microsoft.AspNetCore.App.Internal.Assets/10.0.11**; no SDK/runtime installation was performed.

| Candidate archive | SHA-256 | Consumer acquisition |
| --- | --- | --- |
| Arkheide.Flourish.Blazor.1.4.4-preview.shell.20261027.5.nupkg | `fc25821368493ec63ad62733736305087e0fb335c8dfd47e00d1b667090c49a9` | Umbrella not consumed by Colligere |
| Arkheide.Flourish.Blazor.Abstract.1.4.4-preview.shell.20261027.5.nupkg | `54586fea636e11b0c46cca60562ffbcc486dabece1f7bf9b5e271d9957740a93` | Matches ordinary consumer cache |
| Arkheide.Flourish.Blazor.Design.1.4.4-preview.shell.20261027.5.nupkg | `198e18605f18f2eec1b1d26e9e9a49ec892dbba0d0a25c3bbee5db631eb3e6a2` | Matches ordinary consumer cache |
| Arkheide.Flourish.Blazor.Framework.1.4.4-preview.shell.20261027.5.nupkg | `81b87256af188f4db41bb26a13595c453c709cdb8cf13a9d5478fecbadf7b3ad` | Matches ordinary consumer cache |
| Arkheide.Flourish.Core.1.4.4-preview.shell.20261027.5.nupkg | `287cb0de686883208305d97030838e672b07e281006b0675aaf9008239b651ff` | Matches ordinary consumer cache |
| Arkheide.Flourish.Extensions.Culture.Blazor.1.4.4-preview.shell.20261027.5.nupkg | `87e7edfc17ea8779cb85de4b65bed7807951c0b2b92b219fac349f550e08ce72` | Matches ordinary consumer cache |

The six library entries below match **package ZIP = producer output = ordinary consumer cache**. Bundled CSS is compared with actual Release outputs; the Framework DLL confirms the compiled component assembly. The host culture-script row is its maintained source identity, outside Flourish packaging. All five library browser entries and the host culture script additionally match their saved HTTP 200 responses after that restart. The Framework DLL is a stored assembly check rather than an HTTP asset.

| Delivered resource | SHA-256 |
| --- | --- |
| Framework / `staticwebassets/framework.css` | `ed70ad3bf637e87ca2810b7b416a292f66b96c3e8fe40e22c6061c443828d9b3` |
| Design / `staticwebassets/design.css` | `de72da256a76d906d0441051718d762bb3a5af76d5f447ec86af124ceafc1409` |
| Framework / `staticwebassets/patterns/header-layout.js` | `cc5af804cdd8f481c4c45947cc4429d149cb0b1b1e8edab2d1a37c8dd2c6a662` |
| Framework / `staticwebassets/controls.js` | `b685bbe4d2a83c62439e27d861128461c40c1cfad22624d090c3d57e1864bd5b` |
| Framework / `staticwebassets/presentation/progress.js` | `41a32212e750166151b27e14838e7fbbe82d21add6acbb1aab77f18310adbaa9` |
| Framework / `lib/net10.0/Flourish.Blazor.Framework.dll` | `fc9787cbe101556140907165e4e61c4623e72fa88337d37a6415166572de22bc` |
| Consumer / `wwwroot/culture-picker.js` | `170ba75da1cbd33baca62c0cde749cfeab17c9bdf1aef3540f5ebfaf9682346b` |

The independent package/cache capture is `artifacts/shell-20261027/dependency-provenance-5.json`, augmented by the coordinator's runtime evidence. Current consumer readiness, asset and SSR captures are `../Colligere/artifacts/information-grid-20261027/health.json`, `runtime-assets.json` and `runtime-markup.json`, with saved response files alongside them. Four public SSR roots confirm actual Quiet brand Buttons, three structured portal actions, localized Management/Organization management labels and Admin Center root navigation. These local structure/byte checks do not establish physical browser geometry or interaction. The inspected consumer Web assets file is `../Colligere/src/Colligere.Web/obj/project.assets.json`, SHA-256 `41f3d5a95a94561049534f578857bf3178f36d54bd794d32d2bb489a1898bca8`. Generated archives, caches, object files and captures remain excluded from the maintained tree. Older dated evidence below does not identify .5 bytes.

### Historical shell candidate .4 archive and asset evidence

Read-only inspection at **2026-10-27T17:36:34.978722Z** confirms all six **1.4.4-preview.shell.20261027.4** archives in `artifacts/shell-20261027/nuget`, including nuspec identities and same-version internal dependency edges. The Culture bridge retains Essential 1.4.0. Candidate .4 adds the responsive footer minimum/shared foreground alignment and presentation-progress selection refinements described in the [footer alignment and progress-threshold report](1_bugreports/2026-10-27_footer-alignment-and-progress-threshold.md). Candidate .3 hashes below remain historical.

The coordinated **12-project producer Release build and six-package pack completed**, and the **17-project Colligere restore and Release build completed**, with zero build warnings/errors. The coordinator's focused mock-DOM checks passed **31 progress + 6 header = 37**, including asynchronous intermediate scrolling and footer lifecycle/geometry. Eleven producer and two consumer JS/MJS files passed syntax checks; Gallery JSON parsed and C# regression sources compiled. No full functional suite or physical browser acceptance ran. This remains a local candidate with no remote publication, tag or deployment asserted.

The 12 producer assets graphs contain **33 external package/version pairs**. The 17 consumer assets graphs contain **168 package/version pairs**, including five consumed Flourish IDs at .4 and Essential Core/Blazor/Generator at **1.4.0**. All five consumed archive SHA-256 values equal the feed bytes; their ordinary global-cache acquisition metadata names this local feed. The umbrella archive is packed but is not referenced by Colligere. The selected local SDK remains **10.0.400**, and the SDK-selected web-assets package is **Microsoft.AspNetCore.App.Internal.Assets/10.0.11**; no SDK/runtime installation was performed.

| Candidate archive | SHA-256 | Consumer acquisition |
| --- | --- | --- |
| Arkheide.Flourish.Blazor.1.4.4-preview.shell.20261027.4.nupkg | `d7bcde06a213ce3b9531176ae8f4a246df0442de9fd605cdbd677b7592da554f` | Umbrella not consumed by Colligere |
| Arkheide.Flourish.Blazor.Abstract.1.4.4-preview.shell.20261027.4.nupkg | `99be9734db187960e42ed984d0e09bb095cec31499fd7eccc6d250d383f43907` | Matches ordinary consumer cache |
| Arkheide.Flourish.Blazor.Design.1.4.4-preview.shell.20261027.4.nupkg | `817aec5e53ca5768978c5ae9c3cf05036e92ad0e7d12f5ba760175e8e73f6b98` | Matches ordinary consumer cache |
| Arkheide.Flourish.Blazor.Framework.1.4.4-preview.shell.20261027.4.nupkg | `312f03e847739da5cad9976f898ff38d3572c464bc44ad51b3d57f7ef890de4f` | Matches ordinary consumer cache |
| Arkheide.Flourish.Core.1.4.4-preview.shell.20261027.4.nupkg | `de6848832f8ad55960618de24088d403dfed9e76e3d8c0408c80b5c7771f0d12` | Matches ordinary consumer cache |
| Arkheide.Flourish.Extensions.Culture.Blazor.1.4.4-preview.shell.20261027.4.nupkg | `a53c18c086a052e9cc0c000dd6d71a191a4ae2fdaf04ca490eac1dffd834100c` | Matches ordinary consumer cache |

The following library resources match **package ZIP = producer output = ordinary consumer cache**. CSS is checked against actual Release bundled outputs; the Framework DLL confirms the compiled component assembly belongs to the same delivered package. The five library browser resources, including `presentation/progress.js`, additionally match the six saved HTTP 200 responses together with the host culture script after the unified consumer restart.

| Delivered resource | SHA-256 |
| --- | --- |
| Framework / `staticwebassets/framework.css` | `2b726e9eb89ce94bd7f888341f1e6bf6b9866d0ecdad7fbe234d37ef3b608500` |
| Design / `staticwebassets/design.css` | `c19bdeb2378f5c897dcfbe37865ee3d7f63ab4ff93a95f15856d39fc80b276f2` |
| Framework / `staticwebassets/patterns/header-layout.js` | `cc5af804cdd8f481c4c45947cc4429d149cb0b1b1e8edab2d1a37c8dd2c6a662` |
| Framework / `staticwebassets/controls.js` | `b685bbe4d2a83c62439e27d861128461c40c1cfad22624d090c3d57e1864bd5b` |
| Framework / `staticwebassets/presentation/progress.js` | `41a32212e750166151b27e14838e7fbbe82d21add6acbb1aab77f18310adbaa9` |
| Framework / `lib/net10.0/Flourish.Blazor.Framework.dll` | `2830fc00af2b8426741f26082e36c89a96f855d82a0ea1bc9286c40ef858ad05` |
| Consumer / `wwwroot/culture-picker.js` | `170ba75da1cbd33baca62c0cde749cfeab17c9bdf1aef3540f5ebfaf9682346b` |

The consumer host culture-script row is **maintained host source = saved HTTP 200 response**, rather than a Flourish package entry. All eight saved readiness probes returned HTTP 200, and the running Debug dependency manifest selects the same five .4 package IDs. Four public SSR pages expose their owned **4/3/3/3** semantic bands, a footer outside main, independent watermark/foreground, and progress bootstrap markers; these structure checks do not assert computed browser geometry or interaction. The coordinator's runtime captures are `../Colligere/artifacts/shell-20261027/health-4.json`, `runtime-assets-4.json` and `runtime-markup-4.json`; exact package/cache/stored-response evidence is preserved in the ignored local `artifacts/shell-20261027/dependency-provenance-4.json`. Physical pointer, keyboard, theme and responsive acceptance remain user-run checks.

The inspected consumer Web assets file is `../Colligere/src/Colligere.Web/obj/project.assets.json`, SHA-256 `42ece80fd8bef238217eaab5b63c2d778c2f542e6951d8006facd0063dda9462`. Generated archives, caches, object files and verification captures remain excluded from the maintained tree. Candidate .3 and older dated evidence below do not identify .4 bytes.

### Historical shell candidate .3 archive and asset evidence

Read-only inspection at **2026-10-27T17:24:04.889109Z** confirms all six **1.4.4-preview.shell.20261027.3** archives in `artifacts/shell-20261027/nuget`, including their nuspec identities and same-version internal dependency edges. The Culture bridge retains Essential 1.4.0. The candidate contains the genuine bounded Quiet-brand Button and standard dropdown/language body typography; the [current implementation and acceptance record](1_bugreports/2026-10-27_standard-brand-button-and-structured-menu-rows.md) distinguishes these refinements from the earlier shell redesign.

The coordinated **12-project producer Release build and six-package pack completed**, and the **17-project Colligere restore and Release build completed**, with zero build warnings/errors. C# regression sources compiled; nine library JS/MJS files and two consumer JS/MJS files passed syntax checks. No functional suite or physical browser acceptance ran. This remains a local candidate with no remote publication, tag or deployment asserted.

The 12 producer assets graphs contain **33 external package/version pairs**. The 17 consumer assets graphs contain **168 package/version pairs**, including five consumed Flourish IDs at .3 and Essential Core/Blazor/Generator at **1.4.0**. All five consumed archive SHA-256 values equal the feed bytes; their ordinary global-cache acquisition metadata names this local feed. The umbrella archive is packed but is not referenced by Colligere. The selected local SDK is **10.0.400**, and the SDK-selected web-assets package is **Microsoft.AspNetCore.App.Internal.Assets/10.0.11**; no SDK/runtime installation was performed.

| Candidate archive | SHA-256 | Consumer acquisition |
| --- | --- | --- |
| Arkheide.Flourish.Blazor.1.4.4-preview.shell.20261027.3.nupkg | `5823df3fdf60eb31f77c45f27b6b8acd019d47df89050e68d7482768ab903897` | Umbrella not consumed by Colligere |
| Arkheide.Flourish.Blazor.Abstract.1.4.4-preview.shell.20261027.3.nupkg | `3d3914c7f2578ef6fa1c2fa5d0b8838d5850d83a227ccd6c96ce6d8d87771be0` | Matches ordinary consumer cache |
| Arkheide.Flourish.Blazor.Design.1.4.4-preview.shell.20261027.3.nupkg | `33c703ea937222fddc2df179ab2bda7d4aa85ed8df63b2435989725279f571c3` | Matches ordinary consumer cache |
| Arkheide.Flourish.Blazor.Framework.1.4.4-preview.shell.20261027.3.nupkg | `3939d311f9d56b1bb2504547beffca696a3602427ffb682b0a6f3c62ecc2348b` | Matches ordinary consumer cache |
| Arkheide.Flourish.Core.1.4.4-preview.shell.20261027.3.nupkg | `f59f250db7fbc10e6167fa4ae02adae68cd9cfbb9ae33c6015142143816fb7ed` | Matches ordinary consumer cache |
| Arkheide.Flourish.Extensions.Culture.Blazor.1.4.4-preview.shell.20261027.3.nupkg | `895cba714d227d68b8c82f5def94ea8282fdcc973fc7fd7ce4a50f3a04cfc364` | Matches ordinary consumer cache |

The following library resources match **package ZIP = producer output = ordinary consumer cache**. CSS is checked against the actual Release bundled outputs; the Framework DLL confirms the compiled component assembly belongs to the same delivered package. The four browser resources additionally match the saved HTTP 200 responses after the unified consumer restart.

| Delivered resource | SHA-256 |
| --- | --- |
| Framework / `staticwebassets/framework.css` | `9b0d0235172aa36d9a174c84955f39786f83d098a9358536826ef9fef32377fc` |
| Design / `staticwebassets/design.css` | `6eeb00b80f3f62ab318a57e9f2150810bf40b059a118131cb1455bf0e381c45c` |
| Framework / `staticwebassets/patterns/header-layout.js` | `5a2671094dd77da5f9450e705850d30434b9c58ab5909924d3cf98d32fb8aca7` |
| Framework / `staticwebassets/controls.js` | `b685bbe4d2a83c62439e27d861128461c40c1cfad22624d090c3d57e1864bd5b` |
| Framework / `lib/net10.0/Flourish.Blazor.Framework.dll` | `4d1fb0702d6dc44cbba99fb8609c3f9ccd67dab86a9f7f6c0c72159cf45b5035` |
| Consumer / `wwwroot/culture-picker.js` | `170ba75da1cbd33baca62c0cde749cfeab17c9bdf1aef3540f5ebfaf9682346b` |

The consumer host culture script row is **maintained host source = saved HTTP 200 response**, rather than a Flourish package entry. All eight saved readiness probes returned HTTP 200, and the running Debug dependency manifest selects the same five .3 package IDs. The coordinator's runtime captures are `../Colligere/artifacts/shell-20261027/health-3.json` and `runtime-assets-3.json`; exact stored-byte evidence is also preserved in the ignored local `artifacts/shell-20261027/dependency-provenance-3.json`. These checks establish local resource delivery, while physical pointer, keyboard, theme and responsive acceptance remain user-run checks.

The inspected consumer Web assets file is `../Colligere/src/Colligere.Web/obj/project.assets.json`, SHA-256 `9d3fd3fc13072c89825bf54bb81e5e867a7813372674b2b5ca3a88e22df67e86`. Generated archives, caches, object files and verification captures remain excluded from the maintained tree. Candidate .2 and older dated evidence below remain historical and do not identify .3 bytes.

### Historical shell candidate .2 archive and asset evidence

Read-only archive/asset inspection at **2026-10-27T13:24:14.336079Z** confirms the six candidate archives in `artifacts/shell-20261027/nuget`. All nuspec identities/internal dependency declarations use 1.4.4-preview.shell.20261027.2; the Culture bridge still declares Essential 1.4.0. The coordinated 12-project producer Release build completed with zero warnings/errors and packing completed. C# test sources compiled, but no suite ran. The consuming 17-project Colligere restore and final Release build also completed with zero warnings/errors. The coordinator ran syntax checks only for eight library JS/MJS files and two host culture-script/regression files; no functional suite ran. Running-portal provenance and physical acceptance remain separate evidence. This is a local candidate, with no public publication asserted.

The 12 producer assets graphs contain **33 external package/version pairs** and candidate-version source references. The 17 consumer assets graphs contain **168 package/version pairs**, including the five consumed Flourish IDs at 1.4.4-preview.shell.20261027.2 and Essential Core/Blazor/Generator at 1.4.0. Five cached archive SHA-256 values match the feed exactly; each acquisition metadata entry names this local feed. The umbrella archive is packed but not referenced by Colligere. The current SDK-selected web assets package is `Microsoft.AspNetCore.App.Internal.Assets/10.0.11`, distinct from the historical audit's 10.0.12 observation. The selected local SDK is 10.0.400; no SDK/runtime installation was performed.

| Candidate archive | SHA-256 | Consumer acquisition |
| --- | --- | --- |
| Arkheide.Flourish.Blazor.1.4.4-preview.shell.20261027.2.nupkg | `9779b0c5233a7b10144336bb7936f812d0d31874e8f57f4496f897556d10c3fb` | Umbrella not consumed by Colligere |
| Arkheide.Flourish.Blazor.Abstract.1.4.4-preview.shell.20261027.2.nupkg | `cec860b02a6b42bdcd721c7e4847787d8dd68e5a5e8a912102e021d15b0b38c9` | Matches ordinary consumer cache |
| Arkheide.Flourish.Blazor.Design.1.4.4-preview.shell.20261027.2.nupkg | `13fcc8c9b8676a6b4457f8265a65c66c3519c76020e32649547e6bb0c347f58f` | Matches ordinary consumer cache |
| Arkheide.Flourish.Blazor.Framework.1.4.4-preview.shell.20261027.2.nupkg | `f57672aaf2067e012c03e4c60a0f1fb0eb0e29dfd8fcd5f2bd42e1eae47a2ed8` | Matches ordinary consumer cache |
| Arkheide.Flourish.Core.1.4.4-preview.shell.20261027.2.nupkg | `aa21205cfa40ed5338e208a71375897208bbd0c6ea8e6e91b21542fce95d2f87` | Matches ordinary consumer cache |
| Arkheide.Flourish.Extensions.Culture.Blazor.1.4.4-preview.shell.20261027.2.nupkg | `c98d828aa7787f0e262a8298c9b2e24bf4ddeced3dbb2bd8a6aa6836a5fc2394` | Matches ordinary consumer cache |

The following resources match **package ZIP = producer = ordinary consumer cache**. CSS is compared with the actual Release bundled outputs, not the source import entry. These are stored-byte checks, not proof that a running portal already serves the candidate.

| Delivered resource | SHA-256 |
| --- | --- |
| Framework / `staticwebassets/framework.css` | `dc5e17c8613a64d81598a3270d9bd9d8d0599f067499d29535f75b3b4fb3c114` |
| Design / `staticwebassets/design.css` | `419bb6ef754ffa0fe84766217a81a7de75b2d7371bcc17e4d567605a3f63c17f` |
| Framework / `staticwebassets/patterns/header-layout.js` | `5a2671094dd77da5f9450e705850d30434b9c58ab5909924d3cf98d32fb8aca7` |
| Framework / `staticwebassets/controls.js` | `b685bbe4d2a83c62439e27d861128461c40c1cfad22624d090c3d57e1864bd5b` |

The inspected consumer Web assets file is `../Colligere/src/Colligere.Web/obj/project.assets.json`, SHA-256 `22653c872a4cb16bb94667d7fe0ed0ab32991fc665cdbc3e92cf486cd2e59e27`. Generated package/obj artifacts remain excluded from the maintained tree. Older package hashes, suite results and public-index evidence below remain explicitly historical.

**Historical first shell candidate (.1), 2026-10-27:** The initial local archives and zero-warning producer/consumer builds were superseded by candidate .2 after dialog-color isolation was completed; .1 bytes remain separate history, and no suite was run.

**Historical stable 1.4.3 publication, 2026-10-26:** The same six Core/Blazor packages are publicly available at 1.4.3, retaining Essential.Culture 1.4.0. The exact tag workflow succeeded and all six public indexes were present at 11:46:01 UTC. Independent consumers passed **143/143** with fresh package/HTTP caches; metadata confirms all six Flourish and three Essential packages came from NuGet.org. Public archive hashes and provenance are preserved in [completed release verification](release-verification.md#completed-flourish-143-release-on-2026-10-26), separate from historical local preview hashes below.

**Historical stable 1.4.3 preparation, 2026-10-26:** Root `VersionPrefix=1.4.3` has no suffix. All six existing Core/Blazor package IDs retain their dependency graph and Essential.Culture 1.4.0. The notification test uses the existing .NET 10 `TimeProvider`/`ITimer` APIs with a small in-test controlled timer, adding no test package or production dependency. Public NuGet indexes still held 1.4.2 during the preparation check; tag/publication is coordinated separately and must be verified before claiming public availability. Historical preview archive hashes below remain historical and are not stable-package identities.

**Historical Field-hover preview, 2026-10-25:** Root is **1.4.3-preview.portals.18**. A Design-owned empty direct-label ::before box bridges the existing Field gap, preserving native hover association and label measurements. The existing gap token drives both spacing and bridge size; actions/errors remain outside. No maintained source file, API, external dependency or production JavaScript was added, so tree and dependency direction are unchanged. Framework **474/474**, Node **35/35**, access spacing **4/4**, six package checks and consuming Release/Web verification passed (**2605 passed, 0 conditional skips**). [Delivery record](1_changelogs/2026-10-25_233308_bridge-field-label-hover-preview-eighteen.md) preserves evidence and manual limits.

| Local .18 package | SHA-256 |
| --- | --- |
| Arkheide.Flourish.Blazor.1.4.3-preview.portals.18.nupkg | `016499057d1ad1f741f8df30f3c2aa7b82396973a59d832eaeda0af0f52781a0` |
| Arkheide.Flourish.Blazor.Abstract.1.4.3-preview.portals.18.nupkg | `9fae55c0d0394be90c69788636101b355b1ab16d6c86982b1e4ceeb438a7e769` |
| Arkheide.Flourish.Blazor.Design.1.4.3-preview.portals.18.nupkg | `c67d619f628d0da9fd46e74b9a13ddd58e35fe361e7c1572eb8c5bcf8c69072f` |
| Arkheide.Flourish.Blazor.Framework.1.4.3-preview.portals.18.nupkg | `8182828dd3f05610787d13601b4143341737c17f6193de26777604b3ac6e8dcd` |
| Arkheide.Flourish.Core.1.4.3-preview.portals.18.nupkg | `cbc56d323d72f940f9c716c9e806cc837efa6fc4e3dfb7cd550bce6e8ccf080b` |
| Arkheide.Flourish.Extensions.Culture.Blazor.1.4.3-preview.portals.18.nupkg | `3cf47183d2a88658124e26ddf04a25fca2e6fbf0f5879bacf370071de0f4a178` |

**Previous unified-Notice preview, 2026-10-25:** Root is **1.4.3-preview.portals.17**. Information is white/blue/dark; warning orange/white, success green/white and error red/white. All Notices share body-sized title/body, 700/400 weights, a separate title row and existing control shadow; Subtle no longer selects a gray skin. Fixed semantic status paints remain stable across themes and host branding. Existing palette values are reused except a strong orange for readable white text; no external dependency or maintained source file was added, so tree and dependency direction are unchanged. Framework **474/474**, palette Node **33/33** (64 mode/brand/status/Subtle combinations), six package checks and consuming Release/Web verification passed (**2601 passed, 187 conditional skips**). See [delivery record](1_changelogs/2026-10-25_224053_unified-notice-colors-preview-seventeen.md) for evidence and manual checks.

| Local .17 package | SHA-256 |
| --- | --- |
| Arkheide.Flourish.Blazor.1.4.3-preview.portals.17.nupkg | `8ab9079144de63b4afcbf8cd0a6999f6c9db0afdc7107dcebe562fe2ea32feb9` |
| Arkheide.Flourish.Blazor.Abstract.1.4.3-preview.portals.17.nupkg | `95dd6bda720621a839d70de05ced918911c40281f5ab5c01e67a4c6f0c43c0ec` |
| Arkheide.Flourish.Blazor.Design.1.4.3-preview.portals.17.nupkg | `2a45bd6195f63e292cd3b8c42ca32da6c5e88ed03874c28e48af27950de2f9b4` |
| Arkheide.Flourish.Blazor.Framework.1.4.3-preview.portals.17.nupkg | `2841fe68b50c407f0670c67e330609f4faa29a51bbb3252a3a5bf4acff8488a1` |
| Arkheide.Flourish.Core.1.4.3-preview.portals.17.nupkg | `f0764e440115d662c9482a6c2542d1b5f905cf997dc77239f6b03a31c820fe31` |
| Arkheide.Flourish.Extensions.Culture.Blazor.1.4.3-preview.portals.17.nupkg | `a4a56595e86480edcd54d66a1c4c2415788186992edfd2c01fdedfb66aa41b3c` |

**Previous Notice-title preview, 2026-10-25:** Root is **1.4.3-preview.portals.16**. Blazor Notice uses an explicit summary or a localized severity title, separate title/body rows, unchanged body font size and a bold semantic title, including subtle notices. Inline body fragments stay together. Framework **474/474**, CSS bundle **21/21**, six package verifications and consuming Colligere Release/Web checks passed (2601 passed, 187 conditional skips). No maintained source file or external dependency was added; the repository tree and dependency direction remain unchanged. Essential remains **1.4.0**. Six local archives have not been publicly published; [delivery record](1_changelogs/2026-10-25_222029_notice-titles-preview-sixteen.md) documents manual acceptance and evidence.

| Local .16 package | SHA-256 |
| --- | --- |
| Arkheide.Flourish.Blazor.1.4.3-preview.portals.16.nupkg | `2d64b17712fd1169421a0c9e8ee83fe1496d593420d89b36e917136c84459eae` |
| Arkheide.Flourish.Blazor.Abstract.1.4.3-preview.portals.16.nupkg | `96aa624a5be1a0d89debf740e654d8a867631a2c114cc32c91ad06024df90d87` |
| Arkheide.Flourish.Blazor.Design.1.4.3-preview.portals.16.nupkg | `0888b4232f9691b102e60c8fcfc48286d3c7047b90d5eb2b278175d90fb690e2` |
| Arkheide.Flourish.Blazor.Framework.1.4.3-preview.portals.16.nupkg | `ef9b9f0f9e28f0ecf40880dfad402fd522918e55263044e7ae89f00fc30f8d57` |
| Arkheide.Flourish.Core.1.4.3-preview.portals.16.nupkg | `dc4fe92ba9b763f3ddcb85d7fa5266cc00583d86b9e166f0959170ef74d4b93d` |
| Arkheide.Flourish.Extensions.Culture.Blazor.1.4.3-preview.portals.16.nupkg | `6dc1db9c10de281661d3c5c5a7b429764cf043880af6a09bc058fa27f87d7d00` |

**Previous unframed-menu preview, 2026-10-25:** Root `VersionPrefix=1.4.3` and `VersionSuffix=preview.portals.15` identify the local candidate. ActionMenu's optional `Unframed=false` and `PanelColumns=null` (positive values only), library-owned grid/decoration styles and direct standalone portal tiles add no external package, SDK, runtime, service, font, icon or maintained media asset. All six Core/Blazor package IDs and their dependency direction remain unchanged; Essential remains **1.4.0**. Framework **472/472** (83 components, 765 rows, 355 documented defaults), focused complete CSS/placement Node **52/52**, and ActionMenu DOM-only **28/28** passed; **14** real-browser checks were explicitly skipped. The [scoped implementation record](1_changelogs/2026-10-25_003131_unframed-action-menu-columns-preview-fifteen.md) distinguishes the initial 471/472 strict cascade-fixture rejection from the final pass. The coordinating task owns local .15 package/cache/asset verification and Colligere delivery results; library checks alone do not establish what an already running host serves. This candidate has not been publicly published.

**Verified earlier heading-loading preview, 2026-10-23:** Root `VersionPrefix=1.4.3` and `VersionSuffix=preview.portals.14` identify the final local candidate. The six Core/Blazor package IDs/dependency direction remain unchanged. PageHeading's `Loading=false`, CSS observable-state feedback and Button asynchronous busy lifecycle add no external package, SDK, runtime, service, icon, font or maintained media asset; Essential remains **1.4.0**. Native CSS `:has()` observes control markers/`aria-busy`, including root-level reconnect dialogs, with no new browser library/network interceptor. The maintained Node entry points include the two regressions through existing palette/page-body-actions imports. Final Release built with zero warnings/errors; Framework **469/469** (83 components, 763 rows, 354 defaults), Gallery **8,585** (474 post-event/379 ChangeLog subsets), Node **193/193** rerun on the final body/.f-root source plus focused final heading checks **3/3**, CSS bundle/SDK **21/194**, preview **12** and ChangeLog **148** passed. Six exact .14 archives in `../Colligere/artifacts/local-nuget/1.4.3-preview.portals.14` passed identity/internal-dependency/required-asset verification and were not publicly published. The superseded intermediate .13 bytes are retained rather than overwritten. Archive SHA-256 below was captured at **2026-10-23T23:39:46Z**. Consumer .14 deployment is now verified separately: after the debug-handler restart all eight ready probes return HTTP 200, and actual HTTP Framework/Design CSS hashes match producer and packed bundles. Consumer `artifacts/heading-loading-20261023/inventory.dependencies.json` and `runtime-resources.json` retain package graph/cache/resource and running-delivery evidence; see [its implementation report](../../../Colligere/docs-ai/current/1_bugreports/2026-10-23_page-heading-loading-and-progress-rendering.md). Physical animation and first-paint priority remain manual acceptance. See the [diagnosis](1_bugreports/2026-10-23_page-heading-loading-and-progress-rendering.md) and [change record](1_changelogs/2026-10-23_203946_add-heading-loading-and-stabilize-progress.md); earlier preview/public-release evidence below remains historical.

| Local .14 candidate package | SHA-256 |
| --- | --- |
| Arkheide.Flourish.Blazor.1.4.3-preview.portals.14.nupkg | `9699c13447cb90656e871da2ab403bca3e450b18d0b7f6bb424b4b6671526a35` |
| Arkheide.Flourish.Blazor.Abstract.1.4.3-preview.portals.14.nupkg | `3d3bd61238cf1f08c990f497258d5fa1dc1eb88ff182ed9caa4f849fb0f9a97d` |
| Arkheide.Flourish.Blazor.Design.1.4.3-preview.portals.14.nupkg | `af6181c1c8b92fe60e57b2f4e1bf21b650db2a7f661318ed55aa55960082fbb7` |
| Arkheide.Flourish.Blazor.Framework.1.4.3-preview.portals.14.nupkg | `3350459490126a3e2b3ed5d26770771f48618342667ce42e368dec26761f9672` |
| Arkheide.Flourish.Core.1.4.3-preview.portals.14.nupkg | `63afed3e2160b8504497ffe41661781800e5781401606e600328ac32acd79994` |
| Arkheide.Flourish.Extensions.Culture.Blazor.1.4.3-preview.portals.14.nupkg | `af53ed041a5cd715c9f4cc6810f6edb42b777c669cae72fe873576d1f8327309` |

**Historical compact-choice preview, 2026-10-23:** Root `VersionPrefix=1.4.3` and `VersionSuffix=preview.portals.12` identify source base `734124f` plus the scoped choice-size/rounding update. Shared choice defaults are 28px/9px; standard controls stay 48px and popup options use compact 40px minimum rows with natural text growth. No external dependency, SDK, runtime, service, maintained media asset or public component parameter was added; Essential remains 1.4.0. Framework **460/460**, focused Node **76/76**, Gallery **8,565** (474 post-event, 371 ChangeLog), CSS bundle **21**, CSS SDK integration **194**, preview identity/fences **12** and ChangeLog validation **142** passed. Physical browser acceptance remains manual. Package creation and Colligere consumer verification are recorded separately by the coordinating task; this source change does not itself establish that a running consumer serves .12. See the [compact-choice implementation and verification record](1_changelogs/2026-10-23_195028_compact-choice-indicators-preview-twelve.md). This candidate has not been publicly published.

**Verified earlier local popup preview, 2026-10-22:** Root `VersionPrefix=1.4.3` and `VersionSuffix=preview.portals.11` identify **`c2207ec` plus the scoped alignment/measurement/marker update**. Item content always aligns left; physical popup placement and vertical visibility remain adaptive. Native picker inline size is prepared from actual content in an offscreen inert measurement before opening, and its choice marker uses the standard 48px control size in a fixed column. Native form/selection/keyboard semantics are retained. No third-party package, SDK, service, maintained source file or public parameter is added; Essential remains 1.4.0. Blazor Release built with zero warnings/errors. Framework **460/460**, Culture **27**, Gallery **8,557** (474 post-event and 367 ChangeLog included), and the complete Node runner **169/169** passed; preview identity/publication-fence checks also passed **12/12**. The first Node run passed 168/169 because an existing access-form test fixture omitted the browser's `window`/`defaultView`; its corrected fixture passed the targeted 4/4 and full repeat. Physical native-picker first-frame rendering was not browser-tested. See the [popup alignment/width report](1_bugreports/2026-10-22_popup-left-alignment-and-native-picker-width.md). This candidate was not publicly published.

Six exact .11 packages in `../Colligere/artifacts/local-nuget/1.4.3-preview.portals.11` passed identity/internal-dependency/required-asset verification. Actual delivery hashes were captured at **2026-10-22T20:55:28Z**. Colligere baseline `db08924` plus package/test changes restored **17 active projects/168 package-version pairs** and built Release with zero warnings/errors. Five cache acquisition records name that local feed and their cached archives match it. Only the five Flourish versions differ from the frozen .10 baseline; the remaining **163 identities are unchanged**. Consumer full Web verification passed **2,328**, with **135 conditional PostgreSQL skips** and **zero failures** (2,463 total); its report retains the raw TRX/log evidence separately from library checks. These package checks do not infer the version of an existing Aspire process.

| Local candidate package | SHA-256 |
| --- | --- |
| Arkheide.Flourish.Blazor.1.4.3-preview.portals.11.nupkg | `3b6d9f536a09d2d3cf458b322440aecf3842680f773846dd8412796ab23e43ae` |
| Arkheide.Flourish.Blazor.Abstract.1.4.3-preview.portals.11.nupkg | `9b66c9e28710441e91710837793630243398d546b16eb033b7901665f14514d6` |
| Arkheide.Flourish.Blazor.Design.1.4.3-preview.portals.11.nupkg | `fd9dc29ec4bb31965518e11ebc67583138aa6243eae80bc971a2f1885d79ea49` |
| Arkheide.Flourish.Blazor.Framework.1.4.3-preview.portals.11.nupkg | `cd52ed2f1aea23ff7ddd7d35f4e2b1424b93cfe58f0a7ec4dd1f8dab011c32c9` |
| Arkheide.Flourish.Core.1.4.3-preview.portals.11.nupkg | `186eee167fec303aa0315da71612adc5b92450cbd39a24535be8332e651be637` |
| Arkheide.Flourish.Extensions.Culture.Blazor.1.4.3-preview.portals.11.nupkg | `beae143662e339af6f90b0ed87a32de6034e0c63c7356e0a658dadf36c1513e5` |

Seventeen ZIP entries match final producer and consumer-cache bytes: five DLLs and twelve static resources. CSS uses `obj/Release/net10.0/bundled-css/` outputs rather than modular entry files. The [captured consumer evidence](../../../Colligere/artifacts/popup-left-11/inventory.dependencies.json) retains all resource hashes, 17 graphs, five acquisition records and frozen .10 comparison as ignored local artifacts. Representative current hashes are:

| Delivered resource | SHA-256 (ZIP = producer = consumer cache) |
| --- | --- |
| Arkheide.Flourish.Blazor.Framework: `staticwebassets/framework.css` | `869f768073cf77963b8798f0da6bf501379f314f43c133102bbc5110169bf4b8` |
| Arkheide.Flourish.Blazor.Framework: `staticwebassets/controls.js` | `5bb0c9c51196d4bd7914a3e4c025520846be863146692e903a5010b056c9c479` |
| Arkheide.Flourish.Blazor.Framework: `staticwebassets/primitives/input-behaviors.js` | `75ccc82279d889daef8323878be59b00fa42c52a2eac52e189d8b36da9599b23` |
| Arkheide.Flourish.Blazor.Framework: `staticwebassets/primitives/reference-dropdown.js` | `23f076a124080adb648858fb6a114424875c29ac1ca939f7930e056e5cf998d1` |
| Arkheide.Flourish.Blazor.Design: `staticwebassets/design.css` | `b8b04287660da29d11bc355ac447233e75ef09c11746ffdaba2b536458375b62` |

**Verified earlier hidden-state preview, 2026-10-22:** Root `VersionPrefix=1.4.3` and `VersionSuffix=preview.portals.10` identify source **`07b64e0` plus the scoped visibility fix**. Production changes only Framework CSS's ordinary-hidden contract; no public parameter, maintained source file, third-party package, SDK, service or asset is added. Essential remains 1.4.0. Final Blazor solution Release build passed with zero warnings/errors; Framework 460/460 (83 components, 762 rows, 353 defaults), Gallery 8,549 (474 post-event and 363 ChangeLog), hidden/surface Node 35/35 (19 native cases included), controls 77/77 and preview identity 12/12 passed. The old CSS reproduced 16 passed/3 failed in the new native group. The [registration hidden-state report](1_bugreports/2026-10-22_registration-invitation-hidden-state.md) retains the separate initial Framework 458/459 parser failure, strict nested-selector parser correction and added fail-closed regression; production CSS did not change for that test correction. This local candidate was not publicly published.

Six exact .10 packages in `../Colligere/artifacts/local-nuget/1.4.3-preview.portals.10` passed identity/dependency/required-asset verification. Actual hashes were captured at `2026-10-22T20:13:19Z`. Colligere source base `b4e6e36` plus package/test changes restored 17 active projects and 168 package/version pairs, with five consumed Flourish acquisition records naming that checkout-local feed. Their cached archives match the feed. Against the frozen .9 baseline only five Flourish versions changed; the remaining 163 identities are unchanged. Colligere's 17-project Release build passed with zero warnings/errors, and its registration/password-reset/organization-setup Node groups passed 30/46. Final full Web verification checked at `2026-10-22T20:16:13Z` passed 2,317 with 135 conditional PostgreSQL skips and zero failures (2,452 total); raw TRX/log evidence is maintained in its report. At that historical .10 verification, the existing Aspire process served .9 because it had not been restarted; this is not a current runtime probe.

| Local candidate package | SHA-256 |
| --- | --- |
| Arkheide.Flourish.Blazor.1.4.3-preview.portals.10.nupkg | `01aa355b235b2f2c294325736782a58fc4db442755c1e98496f23e5a06180855` |
| Arkheide.Flourish.Blazor.Abstract.1.4.3-preview.portals.10.nupkg | `f30c4ccdab4e170abe5a815a58a36375642fba19c6075af08437298ab9c59767` |
| Arkheide.Flourish.Blazor.Design.1.4.3-preview.portals.10.nupkg | `8ad0c280b51ee7f49aa9e3ac57783e06d2a0bb82e1c771c81f7d6ad3f5b03db0` |
| Arkheide.Flourish.Blazor.Framework.1.4.3-preview.portals.10.nupkg | `61ace4725c18c757aa244e7de2fdc5f403434faf04892260fd8bfd7135f68eae` |
| Arkheide.Flourish.Core.1.4.3-preview.portals.10.nupkg | `4266309b54d7abb04ae2ac8576efbba69b505136d8863d04540f98db81c98427` |
| Arkheide.Flourish.Extensions.Culture.Blazor.1.4.3-preview.portals.10.nupkg | `d0ed549d6684564beaab123a643031d1170d5078e345f5b87a89065e5cbd141b` |

Seventeen ZIP entries match final producer and consumer-cache bytes: five DLLs and twelve static resources. CSS is compared with `obj/Release/net10.0/bundled-css/` output, not modular source entries. The [captured consumer evidence](../../../Colligere/artifacts/registration-hidden-10/inventory.dependencies.json) preserves each resource hash and .9 baseline comparison as ignored local artifacts. Representative current hashes are:

| Delivered resource | SHA-256 (ZIP = producer = consumer cache) |
| --- | --- |
| Arkheide.Flourish.Blazor.Framework: `staticwebassets/framework.css` | `a90deccfad7eaf4470357518e67249c11840a3ddf29e2a272d5daf4d551688b3` |
| Arkheide.Flourish.Blazor.Framework: `staticwebassets/controls.js` | `4f3ae13ef00f6652ed78b16c43781294854b7651fc0a14426f4e459da443fe09` |
| Arkheide.Flourish.Blazor.Framework: `staticwebassets/primitives/input-behaviors.js` | `75ccc82279d889daef8323878be59b00fa42c52a2eac52e189d8b36da9599b23` |
| Arkheide.Flourish.Blazor.Framework: `staticwebassets/primitives/reference-dropdown.js` | `23f076a124080adb648858fb6a114424875c29ac1ca939f7930e056e5cf998d1` |
| Arkheide.Flourish.Blazor.Design: `staticwebassets/design.css` | `bbb88903048df55514e3b706bbc1d9b77734fb04190f36aff495477094fd85f5` |

**Verified earlier selection/identity preview, 2026-10-22:** Root `VersionPrefix=1.4.3` and `VersionSuffix=preview.portals.9` identified that historical source candidate based on `2a00a23` plus pending local changes. Native option automatic sizing, independent choice-marker columns and optional `LogoDisplayer.ContextPrimary` (default false) add no third-party package, SDK, service or asset; Essential remains 1.4.0. ContextPrimary with nonblank context promotes the context title above brand scale, while Compact aligns the image to the context row and retains independent LogoSize. Final Blazor solution Release build passed with zero warnings/errors; Framework 459/459 (83 components, 762 parameter rows, 353 defaults), Gallery 8,541 (474 post-event and 359 ChangeLog), access Node 13/13, native/placement/palette 61/61, controls 77/77, split 5/5 and preview identity 12/12 passed. The [selection-column and service-identity report](1_bugreports/2026-10-22_selection-columns-and-service-identity.md) records source diagnosis, verification and the unperformed physical first-frame acceptance. This candidate was not publicly published.

Historical .9 package evidence: Six exact .9 packages in `../Colligere/artifacts/local-nuget/1.4.3-preview.portals.9` passed package identity/dependency/required-asset verification. Actual hashes were captured at `2026-10-22T12:57:48Z`. Colligere's 17 active restored graphs contain 168 package/version pairs; its five consumed Flourish IDs have acquisition metadata naming that checkout-local feed, and all five cached archives match the feed. Against the frozen .8 baseline only those five versions changed; the remaining 163 identities are unchanged. Its 17-project Release build passed with zero warnings/errors; final Web verification checked at `2026-10-22T13:02:03Z` passed 2,311 with 135 conditional PostgreSQL skips and zero failures (2,446 total). The consumer report retains the first stale percentage-width guard failure, strict test correction, targeted pass and final TRX/log evidence; production packages were unchanged. Consumer results remain separate from library checks.

| Local candidate package | SHA-256 |
| --- | --- |
| Arkheide.Flourish.Blazor.1.4.3-preview.portals.9.nupkg | `05c9387de782cd75b80cfefaf8a69871933c6e411b57d37511e82301f9fa2fdc` |
| Arkheide.Flourish.Blazor.Abstract.1.4.3-preview.portals.9.nupkg | `48dec8e1679cd1dfe541181b6078e6e49c43a516729ba2fed5e7c7b622cece90` |
| Arkheide.Flourish.Blazor.Design.1.4.3-preview.portals.9.nupkg | `3e38fd654108c6cb988ed6c631fd95d678349f2a090e33a15fb7e6b5a834de64` |
| Arkheide.Flourish.Blazor.Framework.1.4.3-preview.portals.9.nupkg | `8af61a821356544c0a1e342006f0b75d5da1bdb73282c84e0a6c8156f6ad93e2` |
| Arkheide.Flourish.Core.1.4.3-preview.portals.9.nupkg | `d225d97c4d4c441b78f3385f2ae5809825abe1884b7c309d55ebf9a949dfe75e` |
| Arkheide.Flourish.Extensions.Culture.Blazor.1.4.3-preview.portals.9.nupkg | `953e06f4ae5d77b750734ec988560717ef4a822dcd6f9bda931c244d08b7b94a` |

Seventeen ZIP entries match the final producer and consumer cache: five DLLs and twelve static resources. The CSS comparison uses actual `obj/Release/net10.0/bundled-css/` output rather than modular source entries. The [captured consumer evidence](../../../Colligere/artifacts/popup-identity-9/inventory.dependencies.json) preserves every resource hash and .8 baseline comparison as ignored local artifacts. Representative delivered hashes are:

| Delivered resource | SHA-256 (ZIP = producer = consumer cache) |
| --- | --- |
| Arkheide.Flourish.Blazor.Framework: `staticwebassets/framework.css` | `f72159c66f2aaa1908bfb29b742914c73ae44e57130cadf86759fa31949d80e9` |
| Arkheide.Flourish.Blazor.Framework: `staticwebassets/controls.js` | `4f3ae13ef00f6652ed78b16c43781294854b7651fc0a14426f4e459da443fe09` |
| Arkheide.Flourish.Blazor.Framework: `staticwebassets/primitives/input-behaviors.js` | `75ccc82279d889daef8323878be59b00fa42c52a2eac52e189d8b36da9599b23` |
| Arkheide.Flourish.Blazor.Framework: `staticwebassets/primitives/reference-dropdown.js` | `23f076a124080adb648858fb6a114424875c29ac1ca939f7930e056e5cf998d1` |
| Arkheide.Flourish.Blazor.Design: `staticwebassets/design.css` | `bbb88903048df55514e3b706bbc1d9b77734fb04190f36aff495477094fd85f5` |

**Verified earlier presentation preview, 2026-10-21:** Root `VersionPrefix=1.4.3` and `VersionSuffix=preview.portals.8` identified that historical source candidate based on `2a00a23` plus pending local changes. Optional `BackgroundImageUrl` on PresentationBand/PresentationHero, library-owned Tone gradients and shared Compact/ordinary LogoDisplayer title typography add no third-party package, SDK, service or media asset. Essential remains 1.4.0; Gallery demonstrates the API with its existing local `gallery.svg`. The image URL is a consumer choice, while Framework owns cover geometry and Design owns semantic masking. Final source/C# Release build passed with zero warnings/errors; Framework 458/458 (83 components, 761 parameter rows, 352 defaults), Gallery 8,541 (474 post-event and 359 ChangeLog), Node 36/36 (10 access + 26 palette, including page-body checks) and preview identity 12/12 passed. Six exact .8 packages passed identity, dependency and asset verification. The [product-media and access-title report](1_bugreports/2026-10-21_product-hero-media-and-access-titles.md) records the initial guard-only failures and corrected strict empty-alt/URL-encoding assertions; production was unchanged by that correction. This candidate was not publicly published.

Historical .8 package evidence: The six exact .8 packages below were generated in `../Colligere/artifacts/local-nuget/1.4.3-preview.portals.8`; their SHA-256 values were captured at `2026-10-21T21:41:11Z`. Colligere restored all 17 active projects with 168 package/version pairs and five consumed Flourish IDs; all five acquisition metadata records identify that checkout-local feed. Compared with .7 only those five Flourish versions differ, while the other 163 identities remain unchanged. Colligere's local first-party product Hero SVG introduces no external media dependency. Its 17-project Release build passed with zero warnings/errors and full Web checks passed 2,311 with 135 conditional PostgreSQL skips and zero failures; detailed consumer evidence and manual acceptance belong to its report. Library source checks do not certify physical browser appearance.

| Local candidate package | SHA-256 |
| --- | --- |
| Arkheide.Flourish.Blazor.1.4.3-preview.portals.8.nupkg | `34336951446e1ef7bc28f4d5a048a72808b6b28e7b9ed25a864c5411697a9655` |
| Arkheide.Flourish.Blazor.Abstract.1.4.3-preview.portals.8.nupkg | `c66f9a2d231a283a9bb724177b29de67205d490cbc680f0a156f3964c22e1787` |
| Arkheide.Flourish.Blazor.Design.1.4.3-preview.portals.8.nupkg | `7f8cc7fef04bf92978834b4d4ae7bb8193bba531e6bc0986051493a38cd67b6e` |
| Arkheide.Flourish.Blazor.Framework.1.4.3-preview.portals.8.nupkg | `f161eb8a0020b74ff59deec8a898af0920e11bb27acea842640c9ca83223fb03` |
| Arkheide.Flourish.Core.1.4.3-preview.portals.8.nupkg | `7192958e7a627676fac570d434c628415ca2094f1feec4c96a0b6e43af74e3de` |
| Arkheide.Flourish.Extensions.Culture.Blazor.1.4.3-preview.portals.8.nupkg | `07131addf85c77c51dedd6b6ee4301171e54fa23647eac0f5502cfb143d25fb9` |

**Verified earlier anchored-popup preview, 2026-10-21:** Root `VersionPrefix=1.4.3` and `VersionSuffix=preview.portals.7` identified that historical source candidate based on `2a00a23` plus pending local changes. Shared content-width/longest-item sizing, physical edge alignment, native pre-open direction synchronization and ReferenceDropdown/SearchAutocomplete manual-popover placement add no third-party package, SDK, service, asset or public parameter; Essential remains 1.4.0. The new `popup-placement.test.mjs` uses Node built-in modules and is registered in `scripts/ReleaseSettings.psd1`. Final complete Blazor solution Release build passed with zero warnings/errors; Framework 456/456, Gallery 8,529 (474 post-event and 355 ChangeLog), focused Node 70/70 (19 placement + 9 native + 9 split + 26 palette + 7 access), controls 77/77 and preview identity 12/12 passed. ActionMenu DOM-only passed 28/28 with 14 real-browser checks explicitly skipped. The first Framework run's stale literal-alignment guard failure and corrected final guard are separate evidence in the [anchored-popup report](1_bugreports/2026-10-21_anchored-popup-placement.md). Stable/publication fences and the Gallery 1.4.3-preview aggregate are unchanged; this candidate was not publicly published.

Historical .7 package evidence: The six exact .7 packages in the generated consumer feed `../Colligere/artifacts/local-nuget/1.4.3-preview.portals.7` passed package identity/dependency/asset verification. Their SHA-256 values were captured at `2026-10-21T21:07:15Z`; the feed is excluded generated output. Colligere's 17 active restored graphs contain 168 package/version pairs and consume five Flourish IDs; all five acquisition metadata records identify that local feed. Compared with .6 only those five versions differ, while the other 163 identities remain unchanged. Consumer build/Web results and manual acceptance remain in its own report, independently of these library/source checks.

| Local candidate package | SHA-256 |
| --- | --- |
| Arkheide.Flourish.Blazor.1.4.3-preview.portals.7.nupkg | `a0eccbe945f195adfe18095593422213b07de2dac3efe2e4cc6dba15457e90cb` |
| Arkheide.Flourish.Blazor.Abstract.1.4.3-preview.portals.7.nupkg | `9a39d5229bb74e6d8c109b0147e2ad65100846d470249ccd7841c1c6898d9e06` |
| Arkheide.Flourish.Blazor.Design.1.4.3-preview.portals.7.nupkg | `c006815ed390847a2f3648947c7b1cce327156416dd011b90893db0648bec287` |
| Arkheide.Flourish.Blazor.Framework.1.4.3-preview.portals.7.nupkg | `940360af1e0e308bd3688bb92a60e65079cb1d5e9b584999dd88bd982e7ad8f9` |
| Arkheide.Flourish.Core.1.4.3-preview.portals.7.nupkg | `40f9d76c8b84895c7c1c3ed2dac55d0ba63d2ba4b0b662333492960609eba3d9` |
| Arkheide.Flourish.Extensions.Culture.Blazor.1.4.3-preview.portals.7.nupkg | `403b0ffdc9b654da8dc7af056640973969969320fb14a975530bd7ecd32dec20` |

**Verified earlier control-state preview, 2026-10-21:** Root VersionPrefix 1.4.3 and VersionSuffix preview.portals.6 identified that historical candidate. Shared native-select state paint, canonical native picker roles, equal Sidebar surfaces and optional LogoDisplayer.LogoSize add no external package, SDK, service or asset; Essential remains 1.4.0. The new native-select-trigger Node matrix is registered in ReleaseSettings.JavaScriptTests. Actual focused Node checks passed 41/41 (8 native + 26 palette + 7 access), final production/full solution Release builds passed zero warnings/errors, Gallery passed 8,521 and preview identity fences passed 12/12. Framework passed 456/456 and all six exact .6 packages passed identity, dependency and asset verification. See [final library verification](1_changelogs/2026-10-21_103800_native-select-preview-six-final-verification.md) for the local feed and log evidence; consumer results are maintained separately. Stable/publication fences and the Gallery 1.4.3-preview aggregate are unchanged. See [implementation record](1_changelogs/2026-10-21_103624_native-select-state-and-identity-preview-six.md).

**Verified earlier bounded-operation preview, 2026-10-20:** Root VersionPrefix 1.4.3 plus VersionSuffix preview.portals.5 identifies the corrected local candidate. The .4 host text-scale failure and its original artifacts remain preserved. AccessLayout, NavigationChoices.FillHeight and LogoDisplayer.Compact add no external package, SDK, asset or service; Essential remains 1.4.0. Gallery retains its 1.4.3-preview aggregate, and complete preview identity/stable-publication fences remain in force. The focused access-layout Node regression is registered in ReleaseSettings.JavaScriptTests and passed with the existing palette group (33/33). All six exact .5 local packages passed package-set verification; Flourish Release build, Framework 455/455, Gallery 8,513 and preview-version 12/12 passed. Colligere restored its 17-project graph, built Release with zero warnings/errors and passed its full Web project (2,307 passed, 135 conditional PostgreSQL skips, zero failed). See [final .5 local verification](1_changelogs/2026-10-20_213621_bounded-access-preview-five-final-verification.md) for package/log evidence and checks not rerun. Earlier .3 public/preview evidence remains separately dated; .5 was not publicly published.

**Verified earlier local preview, 2026-10-19:** Root VersionPrefix was 1.4.3 with VersionSuffix preview.portals.3. This NavigationChoices increment adds no external dependency; Essential stays 1.4.0. Package identity includes the complete suffix for local verification; the stable release helper explicitly rejects it. Gallery groups these changes under 1.4.3-preview without a fictitious stable 1.4.3 record. The coordinating task verified all six exact .3 packages in Colligere/artifacts/local-nuget/1.4.3-preview.portals.3, restored its 17-project graph and completed its Release build with zero warnings/errors. The [final local verification](1_changelogs/2026-10-19_162544_portal-preview-final-verification.md) records the library and consumer tests with their separate database scopes. Stable 1.4.2 publication and resolved-package evidence below remain dated public-release facts; this local preview was not publicly published or subjected to a new public-feed consumer test.


## Historical published release 1.4.2, 2026-10-19

Public release VersionPrefix **1.4.2** was published from commit `c8616f0f8712a5ff78b21612f6886004366fe414` and tag v1.4.2 after [run 39254689722](https://github.com/Evigila/Flourish/actions/runs/39254689722) passed build verification and Trusted Publishing. The same six Core/Blazor package IDs are public. EssentialCultureVersion and ReleaseSettings.EssentialVersion remain **1.4.0**; no external dependency or publication ID changed. Native WPF/WinUI remain outside the publication manifest.

Local and hosted complete gates passed, with zero build warnings/errors. The six production project.assets.json files retain the same 16 external package/version pairs: three Essential Culture packages at 1.4.0 and thirteen Microsoft.Extensions packages at 10.0.8. Referenced Flourish projects identify 1.4.2. This does not claim a fresh desktop-project restore outside the release gate.

All six public indexes contain 1.4.2. `build/Test-BlazorPackageConsumers.ps1 -PublicSource -Version 1.4.2` passed 143 checks with new package and HTTP caches, using NuGet.org as the only source. Fixture `artifacts/package-consumers/a0169ef69519433cb66f88f96b4a443e` records all six Flourish 1.4.2 and three Essential 1.4.0 package metadata sources as `https://api.nuget.org/v3/index.json`. `artifacts/release-1.4.2-public/public-consumer-provenance.json` retains actual downloaded archive SHA-256 values and NuGet content hashes. Local candidate bytes are separate evidence and are not substituted for the published archives.

The retained v1.4.1 tag is unpublished because its [CI run](https://github.com/Evigila/Flourish/actions/runs/39253305330) failed on a browser-startup file lock before package upload. Version 1.4.2 includes the fixture recovery and four intended popup/presentation notes; Gallery labels 1.4.1 honestly and previews 1.4.3. See [release verification](release-verification.md#completed-flourish-142-release-on-2026-10-19). Dated local preview and audit graphs below remain historical evidence.

**Audit baseline:** 2026-10-10 (America/Sao_Paulo, UTC-03:00), working tree based on `3e861b346a145240b8cdabbfe9b39cdd785fc819`. That inventory included the then-uncommitted Essential.Culture 1.4.0 bridge/module update and the installed AGENTS framework 1.0.0. It recorded inspection of declarations and available resolved artifacts; no new restore, build, runtime test, package publication or remote framework version check occurred during that audit.

**Release update, 2026-10-10:** Current root properties declare Flourish `VersionPrefix=1.4.0`, with Essential.Culture still 1.4.0. The same six Core/Blazor packages are published from tag v1.4.0; WPF and its Culture bridge remain outside that public manifest. Local/CI verification, public indexing and fresh public-only consumers passed; see [release verification](release-verification.md#completed-flourish-140-release-on-2026-10-10). The earlier 22-project audit snapshots below remain dated 1.1.3 source-version evidence; only separately identified release artifacts establish a new restore.

## Historical local popup candidate (.4), 2026-10-12

The final local candidate is 1.4.1-local.popup.20261012.4. It retains .3's standard native selection, FloatingActions and tutorial-segment APIs, adding one Design paint correction: enabled customized Quiet selects in ShellHeader inherit chrome foreground instead of the language-option foreground. Native pickers keep their independent option colors. The actual strict browser regression reproduces .3's black icon on blue chrome and verifies the correction while preserving black-on-white picker/options.

All six packages were built into fresh artifacts/local-popup-4/build, cache and nuget with the five version overrides below. Release compiled with zero warnings/errors; package verification and 143 independent consumers passed (fixture artifacts/package-consumers/28122b76194c45efa95982304ec491a6). The expanded actual menu/native/browser file passed 38/38 with zero browser skips. Colligere restored this exact closure and its final Release/full Web regression passed 2379 with 132 existing database/shared-fixture skips and zero failures. Earlier source-behavior checks remain documented under historical .3; this change only adds the verified chrome paint rule. Stable 1.4.0, Essential 1.4.0 and all earlier candidate bytes are preserved. No public push/tag was performed. See [the follow-up record](1_changelogs/2026-10-12_192640_preserve-native-quiet-select-chrome-colors.md).

| Produced package | SHA-256 |
| --- | --- |
| Arkheide.Flourish.Blazor.1.4.1-local.popup.20261012.4.nupkg | 94C503955B298F9085489CA7AE9DCDCC2FD180C153A47327E7B38EBBED5395F6 |
| Arkheide.Flourish.Blazor.Abstract.1.4.1-local.popup.20261012.4.nupkg | DA502CB167E5B96A56449A6FCB8C2D52F276892EFF1799F8A90DF9E971EF4F2E |
| Arkheide.Flourish.Blazor.Design.1.4.1-local.popup.20261012.4.nupkg | C0B9F4A1AA57D7A91049B5AEC4AC0CD72743DC062497BD1C8F1801645F229B77 |
| Arkheide.Flourish.Blazor.Framework.1.4.1-local.popup.20261012.4.nupkg | 53842EDBBA71090E83431887B6646F72940F06C273411C9642A37FDB1153537C |
| Arkheide.Flourish.Core.1.4.1-local.popup.20261012.4.nupkg | 60A47BBFD231E82F9909A076193D39B4C09123ABE30194339A9B96A7C1982035 |
| Arkheide.Flourish.Extensions.Culture.Blazor.1.4.1-local.popup.20261012.4.nupkg | 11C7BDCCACD18F6BDB60050D6C22789B7A7F6169D66DFA4D0228B39422FAA028 |

## Historical local popup candidate (.3), 2026-10-12

The current candidate is 1.4.1-local.popup.20261012.3, built from the modified master checkout based on 0597f0a00640d686b0bf2bb4158fe59c60382bfb. It adds optional Icon/TriggerVariant to SelectBox, StandaloneSelectBox and LanguagePicker, plus ContentSurface.FloatingActions. It fixes percentage popup minimum width and reuses TutorialBoard segments for floating presentation navigation, with no footer tail row. Native selection remains browser-owned; unsupported rendering engines retain a readable text selector. No external dependency family or stable version declaration changed.

The same six-package Core/Blazor closure was restored and built with the five isolated candidate overrides below in artifacts/local-popup-3/build, cache and nuget. SDK 10.0.401 Release compilation had zero warnings/errors. Package verification passed for all six identities/dependencies/assets. Four package-only consumers passed 143 checks (fixture artifacts/package-consumers/acb146adc8dc4fbf8dc46e86eed7eccb). The preserved .1/.2 and public 1.4.0 bytes were not overwritten, and no package/tag was published. Candidate provenance and logs are in artifacts/local-popup-3; all 322 captured package/build inputs still match the packed source. A subsequent Gallery sample qualification fix is outside all six package inputs and does not alter their bytes.

Verified source checks: 451 Framework console checks, 27 Culture bridge checks, 8346 Gallery checks, 25 progress-controller checks and 37 menu/native/browser checks with zero browser skips. The other 18 maintained JavaScript suites passed. Tests use installed Edge with isolated profiles and DevTools trusted input for actual native picker selection/cancellation; nothing was installed. Colligere consumes this candidate and passed 2379 Web tests with 132 existing database/shared-fixture skips and zero failures. The focused host selection passed 103/103. See [the correction record](1_changelogs/2026-10-12_191504_correct-standard-select-and-floating-layout.md).

| Produced package | SHA-256 |
| --- | --- |
| Arkheide.Flourish.Blazor.1.4.1-local.popup.20261012.3.nupkg | 3EB00696E304F016BEC27DACABE0D0C00CDD975012C3EBAB26245F3FB315D08D |
| Arkheide.Flourish.Blazor.Abstract.1.4.1-local.popup.20261012.3.nupkg | 6A5C1C1051A5712FD69FF04E14EDF9E6627C6A9E955D883CB851625754181F92 |
| Arkheide.Flourish.Blazor.Design.1.4.1-local.popup.20261012.3.nupkg | B05C19494C9C9EB250B2954DE5422F44FA8FDF582AF630D552BDF5C2CC0EDA81 |
| Arkheide.Flourish.Blazor.Framework.1.4.1-local.popup.20261012.3.nupkg | 046194C823C54C34B084888920DEB852DFC97A4DB962C9AC3BDE8299A2D918DE |
| Arkheide.Flourish.Core.1.4.1-local.popup.20261012.3.nupkg | 60A4D41CA1C4144ECE9EB0FF6F2E2192EA29A6E05F33BBB60472704024AC5AE2 |
| Arkheide.Flourish.Extensions.Culture.Blazor.1.4.1-local.popup.20261012.3.nupkg | B73E2390CB263D7DB3AAE7DF7818E6FB72ABC01E154EFA2D253E361702C10449 |

## Historical local popup candidate (.2), 2026-10-12

The currently verified local candidate is `1.4.1-local.popup.20261012.2`. It includes the subsequent fixed-surface sizing, popup top-layer/fallback markup, catalog and locale-description corrections from the frozen modified working tree based on `0597f0a00640d686b0bf2bb4158fe59c60382bfb`. The base commit alone does not identify that working-tree content. No further public API was added in this second candidate. Published 1.4.0 and the first `.1` candidate remain separate historical artifacts; do not rebuild `.1` from the current implementation.

Stable `Directory.Build.props` still declares `VersionPrefix=1.4.0` and Essential `1.4.0`. These candidate commands alone override `Version` and `PackageVersion` to `.2`, `VersionPrefix` to `1.4.1`, and `VersionSuffix` to `local.popup.20261012.2`. SDK `10.0.401` restored the six-project Core/Blazor closure into new isolated `artifacts/local-popup-2/build` and `artifacts/local-popup-2/cache` directories using the generated NuGet.org-only `artifacts/local-popup-2/NuGet.Config`. Release build passed with zero warnings and zero errors. Six packages were packed into `artifacts/local-popup-2/nuget`; `scripts/Verify-PackageSet.ps1` passed for their identities, candidate dependency versions, managed/metapackage boundaries and required assets. WPF/WinUI are outside this package manifest. No public push, tag or stable-version edit was performed.

| Package | Version | Direct candidate Flourish dependencies | Produced `.nupkg` SHA-256 |
| --- | --- | --- | --- |
| `Arkheide.Flourish.Core` | `1.4.1-local.popup.20261012.2` | None | `0F903CE73FF81ED002756071F6FFE189F030DEB90213599A6A4E765FF1979519` |
| `Arkheide.Flourish.Blazor.Abstract` | `1.4.1-local.popup.20261012.2` | Core | `7ADD47965486F7584C292DEC839E22A4729AADD4F36A196F7CF2A02AB175CACB` |
| `Arkheide.Flourish.Blazor.Framework` | `1.4.1-local.popup.20261012.2` | Abstract | `17FB2E6E3FD809F0515C66E19B4165F526CA9867B44BD83F37942B11C5CC3D18` |
| `Arkheide.Flourish.Blazor.Design` | `1.4.1-local.popup.20261012.2` | Framework, Abstract | `598836638210A08CC9F2F61199BBB85160C2FAB9A8D354326A17F2C3A8F6ABE6` |
| `Arkheide.Flourish.Extensions.Culture.Blazor` | `1.4.1-local.popup.20261012.2` | Framework | `08FE2B29A914F586C9DCE540C790275C5B0ADA900A700A86BCB6DD169DC53C16` |
| `Arkheide.Flourish.Blazor` | `1.4.1-local.popup.20261012.2` | Abstract, Framework, Design, Culture.Blazor | `FD06275017574BB019F41BAC743B270B0F4837760489C6462CA9CB7837902C35` |

The six new `build/obj/<project>/project.assets.json` files resolve the same **16 external package ID/version pairs** as `.1`: three Essential Culture packages at `1.4.0` and 13 Microsoft.Extensions packages at `10.0.8`, enumerated in the first-candidate section and the package tables below. Only the five referenced Flourish source libraries now identify `.2` in that production graph. This supplements the dated 22-project audit and does not establish a fresh restore of its remaining 16 projects.

Four fresh package-only modes (FrameworkOnly, MetaNative, MetaDesign and MetaCulture) passed **143 checks** via `build/Test-BlazorPackageConsumers.ps1`. The fixture is `artifacts/package-consumers/5531a25ec1aa4c958524bb2b4342f378`; all six candidate cache metadata entries identify `artifacts/local-popup-2/nuget`, with no source project references. Registration, SSR, publication assets and optional Culture contracts passed. Logs are `artifacts/local-popup-2/logs/{restore,build,pack,verify-package-set,package-consumers}.log`; local machine-readable provenance, source CSS input hashes and historical before-hashes are in `artifacts/local-popup-2/candidate-provenance.json`.

| Delivered asset | Actual producer input | SHA-256, identical in input/bundle, package and published consumers |
| --- | --- | --- |
| Framework `framework.css` | `build/obj/Flourish.Blazor.Framework/release/bundled-css/framework.css`, generated from maintained modular `wwwroot` CSS by `build/CssBundle.targets` | `056EC3E54A3930AB49EEA553EC22A9A5FCFCC677180EED150E367C6ADADA3D84` |
| Design `design.css` | `build/obj/Flourish.Blazor.Design/release/bundled-css/design.css`, generated by the same bundler | `36A63548077D1BF6B1BBB56733CBB03C605BBCBF32CA806754591D26CF85BA11` |
| Framework `controls.js` | Framework `wwwroot/controls.js` | `ED7C01FC25C0E32FA8E70F9EBF5495FDD162208B167C098F95BB6925325D8BF8` |
| Framework `presentation/progress.js` | Framework `wwwroot/presentation/progress.js` | `BC4FCC2FD287314E2AE3C7807B0A7B77897E79FC2F28E98251039A5C571F315E` |

The Framework assets match all four consumers; Design CSS matches the three meta consumers that carry its package. CSS hashes identify the generated bundle, not the entry file containing modular imports; provenance separately inventories the source files. All six `.1` feed archives, the five installed `.1` and five installed stable 1.4.0 global-cache archives, both absent metapackage cache entries, and `Directory.Build.props` retain their pre-build state. The `.2` cache and outputs are independent. A fresh host requires these six locally built packages and an explicit candidate source mapping/version; NuGet.org and the stable 1.4.0 declaration alone cannot restore this unpublished candidate.

The action-menu regression now optionally measures real SSR geometry using an already installed Chromium-compatible browser, without Playwright or a downloaded browser. `tests/Tests.Flourish.Blazor/action-menu-dom.mjs` accepts `FLOURISH_HEADLESS_BROWSER`, then probes existing Edge/Chrome/Chromium locations. The observed Windows tool is Microsoft Edge `155.0.4283.45`; the task's controller/installed-browser regression passed 33 checks (24 controller plus 9 geometry), with zero skips. If no supported executable is installed, the script explicitly reports `SKIP real-browser SSR geometry` for nine checks and still executes the controller checks. This is an optional test-tool dependency, not a production/browser-version requirement or a new npm/NuGet package.

### Reproduce the local candidate

Run these PowerShell commands from the Flourish repository root containing the `.4` implementation. They use the existing [six-package manifest](../../scripts/ReleaseSettings.psd1), [package verifier](../../scripts/Verify-PackageSet.ps1) and [four-mode consumer checks](../../build/Test-BlazorPackageConsumers.ps1), without a production cleanup/release script. Version overrides must remain five separate arguments. Treat consumed candidate versions as immutable: subsequent source changes require another version rather than replacement of cached bytes.

```powershell
$ErrorActionPreference = 'Stop'
$repo = (Get-Location).Path
$version = '1.4.1-local.popup.20261012.4'
$local = Join-Path $repo 'artifacts/local-popup-4'
$build = Join-Path $local 'build'
$cache = Join-Path $local 'cache'
$feed = Join-Path $local 'nuget'
$logs = Join-Path $local 'logs'
$config = Join-Path $local 'NuGet.Config'
$entry = Join-Path $repo 'src/Flourish.Blazor/Flourish.Blazor/Flourish.Blazor.csproj'
$settings = Import-PowerShellDataFile (Join-Path $repo 'scripts/ReleaseSettings.psd1')
New-Item -ItemType Directory -Path @($build, $cache, $feed, $logs) -Force | Out-Null
$nugetConfig = '<configuration><packageSources><clear /><add key="nuget.org" value="https://api.nuget.org/v3/index.json" /></packageSources><packageSourceMapping><clear /><packageSource key="nuget.org"><package pattern="*" /></packageSource></packageSourceMapping></configuration>'
[IO.File]::WriteAllText($config, $nugetConfig)
$properties = @(
    "-p:Version=$version"
    '-p:VersionPrefix=1.4.1'
    '-p:VersionSuffix=local.popup.20261012.4'
    "-p:PackageVersion=$version"
    '-p:ContinuousIntegrationBuild=true'
)
& dotnet restore $entry --artifacts-path $build --packages $cache --configfile $config @properties 2>&1 | Tee-Object -FilePath (Join-Path $logs 'restore.log')
if ($LASTEXITCODE -ne 0) { throw 'Candidate restore failed.' }
& dotnet build $entry --configuration Release --no-restore --artifacts-path $build @properties -p:TreatWarningsAsErrors=true 2>&1 | Tee-Object -FilePath (Join-Path $logs 'build.log')
if ($LASTEXITCODE -ne 0) { throw 'Candidate build failed.' }
foreach ($package in $settings.Packages) {
    $project = Join-Path $repo $package.Project
    & dotnet pack $project --configuration Release --no-build --no-restore --artifacts-path $build --output $feed @properties 2>&1 | Tee-Object -FilePath (Join-Path $logs 'pack.log') -Append
    if ($LASTEXITCODE -ne 0) { throw "Candidate pack failed: $($package.Id)" }
}
& pwsh -NoLogo -NoProfile -File (Join-Path $repo 'scripts/Verify-PackageSet.ps1') -Version $version -PackageDirectory $feed 2>&1 | Tee-Object -FilePath (Join-Path $logs 'verify-package-set.log')
if ($LASTEXITCODE -ne 0) { throw 'Candidate package verification failed.' }
& pwsh -NoLogo -NoProfile -File (Join-Path $repo 'build/Test-BlazorPackageConsumers.ps1') -Version $version -PackageDirectory $feed 2>&1 | Tee-Object -FilePath (Join-Path $logs 'package-consumers.log')
if ($LASTEXITCODE -ne 0) { throw 'Candidate package consumers failed.' }
```

After verification, map only `Arkheide.Flourish.*` at `.4` to this local feed in the consuming host; keep other dependencies on their existing source. The feed and proof files are ignored local artifacts, so independent machines/remote CI must receive or build the candidate feed explicitly, or await a subsequent authorized public release.

## Local popup candidate, 2026-10-12

**Historical candidate .1:** The following results and bytes precede `.2` and remain retained for provenance.

The authorized popup/presentation implementation was built from the frozen modified working tree based on `0597f0a00640d686b0bf2bb4158fe59c60382bfb`. The base commit alone does not identify the modified candidate content. The stable declaration remains `VersionPrefix=1.4.0`; only these local commands override `Version` and `PackageVersion` to `1.4.1-local.popup.20261012.1`, `VersionPrefix` to `1.4.1`, and `VersionSuffix` to `local.popup.20261012.1`, with `ContinuousIntegrationBuild=true`. No public package push or tag was performed. This candidate does not replace the published 1.4.0 evidence above.

The six-package Core/Blazor closure below was restored and built in Release with SDK `10.0.401`, using isolated `artifacts/local-popup/build`, `artifacts/local-popup/cache`, and a generated restore configuration containing only NuGet.org. The build passed with zero warnings and zero errors. Six packages were packed into `artifacts/local-popup/nuget`, then `scripts/Verify-PackageSet.ps1 -Version 1.4.1-local.popup.20261012.1 -PackageDirectory <local-feed>` passed for all six identities, internal dependency versions, managed/metapackage boundaries and required static assets. WPF and WinUI remain outside this local package manifest; their source changes are not delivered by these six packages.

| Locally built package | Candidate version | Direct Flourish package dependencies at the same candidate version | SHA-256 of the produced `.nupkg` |
| --- | --- | --- | --- |
| `Arkheide.Flourish.Core` | `1.4.1-local.popup.20261012.1` | None | `B8E30D1271D473AD3B0A1503007D0C456A3D2E8A52F0A7B84F01EE24EDABE988` |
| `Arkheide.Flourish.Blazor.Abstract` | `1.4.1-local.popup.20261012.1` | Core | `0C934E99B9E7CD2F207DC0011E17BC9B2814027B269490DD4A2B5740E47A9DF7` |
| `Arkheide.Flourish.Blazor.Framework` | `1.4.1-local.popup.20261012.1` | Abstract | `C1BD233BC86ACA616CE4C4DF1BA762F7FA8E2FE3B767C2CEE5E4BE00D4B6AEFA` |
| `Arkheide.Flourish.Blazor.Design` | `1.4.1-local.popup.20261012.1` | Framework, Abstract | `9925DBFF1A44334D15804ACA7A80F303E22260482F1CFBB8A3C6863B551D0BE4` |
| `Arkheide.Flourish.Extensions.Culture.Blazor` | `1.4.1-local.popup.20261012.1` | Framework | `0F3B2BAC6E19BDA69DF1E08C43E953D2FBE38D9BEEF8CA3F243DAB538E4473C3` |
| `Arkheide.Flourish.Blazor` | `1.4.1-local.popup.20261012.1` | Abstract, Framework, Design, Culture.Blazor | `268300BD657F09A49C763293CD459E6FE594104261A611FE3681A3B55FB72517` |

The six fresh `artifacts/local-popup/build/obj/<project>/project.assets.json` files resolve **16 external package ID/version pairs**: the Essential Culture, Culture.Blazor and Culture.Generator packages at `1.4.0`, plus the 13 Microsoft.Extensions packages already listed below at `10.0.8` (Configuration, Configuration.Abstractions, Configuration.FileExtensions, Configuration.Json, DependencyInjection.Abstractions, Diagnostics.Abstractions, FileProviders.Abstractions, FileProviders.Physical, FileSystemGlobbing, Hosting.Abstractions, Logging.Abstractions, Options and Primitives). The five referenced Flourish source libraries resolve at the candidate version in that build graph. Production declarations, external package versions, source-reference edges and license defaults are unchanged. These six fresh snapshots supplement the dated 22-project audit; they do not establish a new restore of the other 16 maintained projects.

`build/Test-BlazorPackageConsumers.ps1` passed **143 checks** in four independently restored/published package-only modes: FrameworkOnly, MetaNative, MetaDesign and MetaCulture. Their fresh fixture is `artifacts/package-consumers/a6d1c533d44148a4981bb04bba1a1b3c`; its isolated cache metadata records the local candidate feed for all six Flourish packages. It verifies package graphs without source project references, registration, SSR, published assets and optional Culture behavior. The Culture dependency remains Essential `1.4.0`. A supplementary generated package-only probe in `artifacts/local-popup/api-probe` passed **45 exported API checks**, verifying the two `PopupStrength` values, all 14 maintained components' public `Strength` parameters and Weak defaults/Strong assignments, and the packaged `PresentationProgress` type. This probe checks the delivered API; popup interaction and viewport behavior are covered by the separate source/DOM suites recorded in the task verification report.

The new Framework `staticwebassets/presentation/progress.js` is present in the candidate archive and all four consumers' published assets. Its bytes match the frozen source and every published copy, SHA-256 `BC4FCC2FD287314E2AE3C7807B0A7B77897E79FC2F28E98251039A5C571F315E`. Evidence is saved locally in `artifacts/local-popup/candidate-provenance.json` and `artifacts/local-popup/logs/{restore-2,build,pack,verify-package-set,package-consumers,api-probe-2}.log`. All five previously installed Flourish 1.4.0 package archives retain their pre-build SHA-256 values; the 1.4.0 metapackage remains absent from that cache, and `Directory.Build.props` retains its pre-build hash. The candidate restore and probe caches are isolated; an authorized consuming host may separately install this new version in its own cache.

Reproduction requires building this candidate package closure before restoring a host configured to use this local feed. The feed and generated evidence are ignored local artifacts, not a published remote distribution. An independent machine or default remote CI cannot restore this candidate from NuGet.org; it needs the explicitly supplied candidate feed or a subsequent authorized public release. Use the dedicated local build/pack commands with all version overrides; the stable release/cleanup scripts target the declared 1.4.0 production output and were not used for this candidate.

### First candidate reproduction (historical)

The following `.1` commands are preserved as historical execution evidence. They require the first candidate implementation and must not be run against the current `.2` source tree.

Run the following PowerShell commands from the Flourish repository root containing the candidate implementation. They use the existing [package manifest](../../scripts/ReleaseSettings.psd1), [package verifier](../../scripts/Verify-PackageSet.ps1) and [package consumer checks](../../build/Test-BlazorPackageConsumers.ps1). The version parameters are separate quoted array entries; joining them into one argument does not perform a valid candidate restore. These commands do not call a production cleanup script, change `Directory.Build.props`, or publish packages. Keep a candidate version immutable after it has been consumed; a later implementation requires a different version rather than replacing cached candidate bytes.

```powershell
$ErrorActionPreference = 'Stop'
$repo = (Get-Location).Path
$version = '1.4.1-local.popup.20261012.1'
$local = Join-Path $repo 'artifacts/local-popup'
$build = Join-Path $local 'build'
$cache = Join-Path $local 'cache'
$feed = Join-Path $local 'nuget'
$restore = Join-Path $local 'restore'
$config = Join-Path $restore 'NuGet.Config'
$entry = Join-Path $repo 'src/Flourish.Blazor/Flourish.Blazor/Flourish.Blazor.csproj'
$settings = Import-PowerShellDataFile (Join-Path $repo 'scripts/ReleaseSettings.psd1')
New-Item -ItemType Directory -Path @($build, $cache, $feed, $restore) -Force | Out-Null
$nugetConfig = '<configuration><packageSources><clear /><add key="nuget.org" value="https://api.nuget.org/v3/index.json" /></packageSources><packageSourceMapping><clear /><packageSource key="nuget.org"><package pattern="*" /></packageSource></packageSourceMapping></configuration>'
[IO.File]::WriteAllText($config, $nugetConfig)
$properties = @(
    "-p:Version=$version"
    '-p:VersionPrefix=1.4.1'
    '-p:VersionSuffix=local.popup.20261012.1'
    "-p:PackageVersion=$version"
    '-p:ContinuousIntegrationBuild=true'
)
& dotnet restore $entry --artifacts-path $build --packages $cache --configfile $config @properties
if ($LASTEXITCODE -ne 0) { throw 'Candidate restore failed.' }
& dotnet build $entry --configuration Release --no-restore --artifacts-path $build @properties -p:TreatWarningsAsErrors=true
if ($LASTEXITCODE -ne 0) { throw 'Candidate build failed.' }
foreach ($package in $settings.Packages) {
    $project = Join-Path $repo $package.Project
    & dotnet pack $project --configuration Release --no-build --no-restore --artifacts-path $build --output $feed @properties
    if ($LASTEXITCODE -ne 0) { throw "Candidate pack failed: $($package.Id)" }
}
& pwsh -NoLogo -NoProfile -File (Join-Path $repo 'scripts/Verify-PackageSet.ps1') -Version $version -PackageDirectory $feed
if ($LASTEXITCODE -ne 0) { throw 'Candidate package verification failed.' }
& pwsh -NoLogo -NoProfile -File (Join-Path $repo 'build/Test-BlazorPackageConsumers.ps1') -Version $version -PackageDirectory $feed
if ($LASTEXITCODE -ne 0) { throw 'Candidate package consumers failed.' }
```

After successful verification, a consuming host can map only `Arkheide.Flourish.*` at this candidate version to the generated local feed while continuing to obtain other dependencies from NuGet.org. The host must explicitly configure that source and version; the stable Flourish version declaration and NuGet.org alone do not select this candidate.

## Evidence and interpretation

All 22 maintained `.csproj` files under `src/` and `tests/` were inspected during the audit. Each had a matching `obj/project.assets.json` whose `project.restore.projectPath` identified that project (the WinUI Gallery path differed only in case). The 20 non-WinUI audit snapshots were written at approximately `2026-10-10T03:06:36Z`; the two WinUI snapshots were written at approximately `2026-10-09T20:30:53Z`. They are not evidence that the normal source `obj/` trees were restored at 1.4.0. Obsolete generated projects, renamed-project caches and temporary `artifacts/` consumers are excluded from the active source inventory.

These audit project snapshots contain **49 distinct package ID/version pairs**: 10 explicitly referenced IDs, one SDK-injected ID, and 38 IDs that are only transitive. WinUI additionally requests 10 SDK/runtime download packs outside the `libraries` graph. The local DocFX/CSharpier net10 tool payloads contain 68 distinct bundled external package ID/version pairs; these belong to development/documentation tooling, not the application package graph.

| Evidence | What it establishes |
| --- | --- |
| Current `Directory.Build.props`; `src/Flourish.Extensions/Directory.Build.props` | Flourish `VersionPrefix=1.4.4`, `VersionSuffix=preview.shell.20261027.10`, Essential property `1.4.0`, inherited version/license defaults; extension props import the root props. The selected six-package candidate is local; dated public 1.4.3 evidence remains separate. |
| Audited `.csproj` declarations and their dated matching `obj/project.assets.json` | Direct package requirements, project edges, target frameworks, exact observed package versions/parents and SDK download requests at the audit baseline. |
| `artifacts/release-1.4.0/obj/Gallery.Flourish.Blazor/project.assets.json`, written `2026-10-10T04:10:14.3513983Z` | Fresh release restore identifies five referenced Flourish libraries as source projects at 1.4.0 and Essential Core/Blazor/Generator as packages at 1.4.0. It does not establish build/test completion or publication. |
| Fresh public consumer `artifacts/package-consumers/9f393f06c97c43efb834ffe4b84510d1`; `artifacts/release-1.4.0/public-consumer-provenance.json` | Four independent package-only consumer modes passed 143 checks. All six Flourish and three Essential 1.4.0 package metadata entries identify NuGet.org; no local feed or source project is used. |
| NuGet cache `<lowercase-id>/<version>/*.nuspec` and `.nupkg.metadata` | Package-declared licenses/source repositories and recorded acquisition feeds. All 49 application package pairs have local metadata recording NuGet.org. |
| `global.json`; `dotnet --version`; `dotnet --list-runtimes`; SDK `Microsoft.NETCoreSdk.BundledVersions.props` | SDK constraint and locally selected SDK/runtime/targeting pack observations. |
| `.config/dotnet-tools.json`; cached tool `.nuspec`, `.deps.json`, `.runtimeconfig.json` | Tool pins, available net10 payloads and their bundled dependencies. Tool package acquisition source is not present in their `.nupkg.metadata`. |
| `.github/workflows/build.yml`, `docs.yml`; `scripts/ReleaseSettings.psd1`; launch/release/build scripts | CI actions, hosted runners, Node requirement, feeds, package verification and optional diagnostic tooling. |
| Icon `source.json`, license files, SVG attribution comments; documentation CSS/configuration | Vendored icon provenance, OS font usage, DocFX template and online Roboto boundary. |
| `AGENTS.framework.json`, `AGENTS.lock.json`, `scripts/fetch-agents.ps1` | Installed instruction framework version, central source commit and optional Git synchronization contract. |

A plain NuGet `Version="x.y.z"` is recorded as the declared lower-bound requirement (`>= x.y.z`), separately from the version currently resolved. There are no maintained `packages.lock.json`, `Directory.Packages.props`, `package.json` or repository-wide NuGet.Config pins. Release checks additionally require the expected Essential 1.4.0 identities. Shared framework references can satisfy and prune Microsoft.Extensions packages in ASP.NET graphs: the consumer lists below describe the package entries actually present, not all projects that use the corresponding framework assemblies.

Project aliases below abbreviate complete consumer sets without omitting project identities. `Assets(Pnn)` means that row's `obj/project.assets.json`. `Cache(id/version)` means its local NuGet nuspec and metadata; cache evidence is generated/local, not a maintained repository file.

## Projects and source reference graph

| Alias | Maintained project | Target framework | Direct source project references |
| --- | --- | --- | --- |
| P01 | `src/Flourish.Blazor/Flourish.Blazor.Abstract/Flourish.Blazor.Abstract.csproj` | `net10.0` | P05 (`Flourish.Core`) |
| P02 | `src/Flourish.Blazor/Flourish.Blazor.Design/Flourish.Blazor.Design.csproj` | `net10.0` | P03 (`Flourish.Blazor.Framework`)<br>P01 (`Flourish.Blazor.Abstract`) |
| P03 | `src/Flourish.Blazor/Flourish.Blazor.Framework/Flourish.Blazor.Framework.csproj` | `net10.0` | P01 (`Flourish.Blazor.Abstract`) |
| P04 | `src/Flourish.Blazor/Flourish.Blazor/Flourish.Blazor.csproj` | `net10.0` | P01 (`Flourish.Blazor.Abstract`)<br>P03 (`Flourish.Blazor.Framework`)<br>P02 (`Flourish.Blazor.Design`)<br>P06 (`Flourish.Extensions.Culture.Blazor`) |
| P05 | `src/Flourish.Core/Flourish.Core.csproj` | `net10.0` | None |
| P06 | `src/Flourish.Extensions/Flourish.Extensions.Culture.Blazor/Flourish.Extensions.Culture.Blazor.csproj` | `net10.0` | P03 (`Flourish.Blazor.Framework`) |
| P07 | `src/Flourish.Extensions/Flourish.Extensions.Culture.WPF/Flourish.Extensions.Culture.WPF.csproj` | `net10.0-windows` | P11 (`Flourish.WPF.Framework`) |
| P08 | `src/Flourish.WinUI3/Flourish.WinUI3.csproj` | `net10.0-windows10.0.26100.0` | None |
| P09 | `src/Flourish.WPF/Flourish.WPF.Abstract/Flourish.WPF.Abstract.csproj` | `net10.0-windows` | None |
| P10 | `src/Flourish.WPF/Flourish.WPF.Design/Flourish.WPF.Design.csproj` | `net10.0-windows` | P09 (`Flourish.WPF.Abstract`)<br>P11 (`Flourish.WPF.Framework`) |
| P11 | `src/Flourish.WPF/Flourish.WPF.Framework/Flourish.WPF.Framework.csproj` | `net10.0-windows` | P09 (`Flourish.WPF.Abstract`) |
| P12 | `src/Flourish.WPF/Flourish.WPF/Flourish.WPF.csproj` | `net10.0-windows` | P09 (`Flourish.WPF.Abstract`)<br>P11 (`Flourish.WPF.Framework`)<br>P10 (`Flourish.WPF.Design`) |
| P13 | `src/Gallery.Flourish.Blazor/Gallery.Flourish.Blazor.csproj` | `net10.0` | P03 (`Flourish.Blazor.Framework`)<br>P02 (`Flourish.Blazor.Design`)<br>P06 (`Flourish.Extensions.Culture.Blazor`) |
| P14 | `src/Gallery.Flourish.WinUI3/Gallery.Flourish.WinUI3.csproj` | `net10.0-windows10.0.26100.0` | None |
| P15 | `src/Gallery.Flourish.WPF/Gallery.Flourish.WPF.csproj` | `net10.0-windows` | P12 (`Flourish.WPF`)<br>P07 (`Flourish.Extensions.Culture.WPF`) |
| P16 | `tests/Tests.Flourish.Blazor.Native/Tests.Flourish.Blazor.Native.csproj` | `net10.0` | P03 (`Flourish.Blazor.Framework`) |
| P17 | `tests/Tests.Flourish.Blazor/Tests.Flourish.Blazor.csproj` | `net10.0` | P03 (`Flourish.Blazor.Framework`)<br>P02 (`Flourish.Blazor.Design`)<br>P06 (`Flourish.Extensions.Culture.Blazor`) |
| P18 | `tests/Tests.Flourish.Core/Tests.Flourish.Core.csproj` | `net10.0` | P05 (`Flourish.Core`) |
| P19 | `tests/Tests.Flourish.Extensions.Culture.Blazor/Tests.Flourish.Extensions.Culture.Blazor.csproj` | `net10.0` | P06 (`Flourish.Extensions.Culture.Blazor`) |
| P20 | `tests/Tests.Flourish.Extensions.Culture.WPF/Tests.Flourish.Extensions.Culture.WPF.csproj` | `net10.0-windows` | P07 (`Flourish.Extensions.Culture.WPF`) |
| P21 | `tests/Tests.Flourish.WPF/Tests.Flourish.WPF.csproj` | `net10.0-windows` | P09 (`Flourish.WPF.Abstract`)<br>P11 (`Flourish.WPF.Framework`)<br>P10 (`Flourish.WPF.Design`)<br>P15 (`Gallery.Flourish.WPF`) |
| P22 | `tests/Tests.Gallery.Flourish.Blazor/Tests.Gallery.Flourish.Blazor.csproj` | `net10.0` | P13 (`Gallery.Flourish.Blazor`) |

Each edge above is a `ProjectReference` declared in the consuming project. These are repository source dependencies, not externally restored Flourish packages. Their shared inherited source version is now 1.4.4-preview.shell.20261027.10 for this local candidate; the selected package manifest still contains only the six Core/Blazor IDs above. The dated audit snapshots observed 1.1.3 and later 1.4.1 before subsequent release changes. The Blazor Abstract -> Core edge supplies platform-neutral services/contracts; Framework -> Abstract supplies production controls; Design -> Framework/Abstract supplies optional visual resources; Culture bridges -> their Framework preserve optional localization. Metapackages compose these entries. Gallery/test edges consume the actual entries. WPF's native Abstract/Framework/Design graph is independent of Core/Blazor. Neither WinUI placeholder project currently references a Flourish source project. Core has no source project dependency.

## Direct and SDK-injected NuGet packages

The exhaustive rows below preserve the dated 22-project audit capture described above. The current candidate verification is recorded separately at the top; restoring its affected Blazor graphs does not recapture the untouched WPF/WinUI graphs or certify every historical SDK-injected pack row.

All rows use NuGet.org (`https://api.nuget.org/v3/index.json`) as the recorded acquisition feed. Licenses and repository links below come from each exact cached nuspec; a package license filename is not treated as a verified SPDX expression.

| Package | Declared requirement | Observed version | Direct consumers; all observed package consumers | Scope and purpose | Source / license | Evidence |
| --- | --- | --- | --- | --- | --- | --- |
| `Arkheide.Essential.Culture.Blazor` | `$(EssentialCultureVersion)` = 1.4.0; restore >= 1.4.0 | `1.4.0` | Direct: P06<br>Observed: P04, P06, P13, P17, P19, P22 | Production: Scoped localization contexts and request-localization adapters used by the optional Blazor bridge. | [repository](https://github.com/Evigila/Essential); `MIT` | Direct consumers' .csproj (or Web SDK), Assets(P04, P06, P13, P17, P19, P22), Cache(`Arkheide.Essential.Culture.Blazor/1.4.0`) |
| `Arkheide.Essential.Culture.Wpf` | `$(EssentialCultureVersion)` = 1.4.0; restore >= 1.4.0 | `1.4.0` | Direct: P07<br>Observed: P07, P15, P20, P21 | Production: WPF localization provider, culture events and XAML integration used by the optional native bridge. | [repository](https://github.com/Evigila/Essential); `MIT` | Direct consumers' .csproj (or Web SDK), Assets(P07, P15, P20, P21), Cache(`Arkheide.Essential.Culture.Wpf/1.4.0`) |
| `Microsoft.AspNetCore.App.Internal.Assets` | Web SDK implicit >= 10.0.12 in observed SDK graph; no host PackageReference | `10.0.12` | Direct: P13, P16 (SDK)<br>Observed: P13, P16 | Production: Web SDK supplied Blazor bootstrap and browser runtime static assets. | [repository](https://github.com/dotnet/dotnet); `MIT` | Direct consumers' .csproj (or Web SDK), Assets(P13, P16), Cache(`Microsoft.AspNetCore.App.Internal.Assets/10.0.12`) |
| `Microsoft.Extensions.Configuration.Json` | >= `10.0.8` | `10.0.8` | Direct: P05<br>Observed: P04, P05, P18, P19 | Production: Core JSON configuration loading. | [repository](https://github.com/dotnet/dotnet); `MIT` | Direct consumers' .csproj (or Web SDK), Assets(P04, P05, P18, P19), Cache(`Microsoft.Extensions.Configuration.Json/10.0.8`) |
| `Microsoft.Extensions.Hosting.Abstractions` | >= `10.0.8` | `10.0.8` | Direct: P05<br>Observed: P04, P05, P18, P19 | Production: Core host lifecycle abstractions. | [repository](https://github.com/dotnet/dotnet); `MIT` | Direct consumers' .csproj (or Web SDK), Assets(P04, P05, P18, P19), Cache(`Microsoft.Extensions.Hosting.Abstractions/10.0.8`) |
| `Microsoft.Extensions.Logging.Abstractions` | >= `10.0.8` | `10.0.8` | Direct: P05<br>Observed: P04, P05, P18, P19 | Production: Core logging contracts. | [repository](https://github.com/dotnet/dotnet); `MIT` | Direct consumers' .csproj (or Web SDK), Assets(P04, P05, P18, P19), Cache(`Microsoft.Extensions.Logging.Abstractions/10.0.8`) |
| `Microsoft.NET.Test.Sdk` | >= `17.14.1` | `17.14.1` | Direct: P18, P20, P21<br>Observed: P18, P20, P21 | Test: VSTest discovery and execution for xUnit test projects. | [repository](https://github.com/microsoft/vstest); `MIT` | Direct consumers' .csproj (or Web SDK), Assets(P18, P20, P21), Cache(`Microsoft.NET.Test.Sdk/17.14.1`) |
| `Microsoft.WindowsAppSDK` | >= `2.3.1` | `2.3.1` | Direct: P08, P14<br>Observed: P08, P14 | WinUI production/build: WinUI 3 library and unpackaged placeholder Gallery platform/tooling entry. | [package project](https://github.com/microsoft/windowsappsdk); Package license file `license.txt`; SPDX identity unknown | Direct consumers' .csproj (or Web SDK), Assets(P08, P14), Cache(`Microsoft.WindowsAppSDK/2.3.1`) |
| `Moq` | >= `4.20.72` | `4.20.72` | Direct: P18<br>Observed: P18 | Test: Mocking Core service contracts in tests. | [repository](https://github.com/moq/moq); `BSD-3-Clause` | Direct consumers' .csproj (or Web SDK), Assets(P18), Cache(`Moq/4.20.72`) |
| `xunit.runner.visualstudio` | >= `3.1.4` | `3.1.4` | Direct: P18, P20, P21<br>Observed: P18, P20, P21 | Test: VSTest xUnit adapter; PrivateAssets=all in all three direct consumers. | [repository](https://github.com/xunit/visualstudio.xunit); `Apache-2.0` | Direct consumers' .csproj (or Web SDK), Assets(P18, P20, P21), Cache(`xunit.runner.visualstudio/3.1.4`) |
| `xunit` | >= `2.9.3` | `2.9.3` | Direct: P18, P20, P21<br>Observed: P18, P20, P21 | Test: xUnit test framework metapackage. | [repository](https://github.com/xunit/xunit); `Apache-2.0` | Direct consumers' .csproj (or Web SDK), Assets(P18, P20, P21), Cache(`xunit/2.9.3`) |

The `PrivateAssets=all` xUnit runner references are test-only. The Culture generator is delivered transitively by Essential Core; it is enabled for Gallery.Blazor and the WPF bridge's module fixture, while library/test projects explicitly disable generation where their resources are manually supplied. Gallery.Blazor uses seven multilingual functional modules, generated typed keys and `CultureResources.Files`; it does not depend on a sibling Essential source checkout.

## Resolved transitive NuGet packages

Complete enumeration of the remaining 38 ID/version pairs in the 22 current project graphs. Parent edges are the union observed across targets; SDK/platform payloads being present does not establish that Gallery explicitly invokes every optional Windows feature.

| Package | Observed version | Consumers | Introduced by | Scope and purpose | Source / license | Evidence |
| --- | --- | --- | --- | --- | --- | --- | --- |
| `Arkheide.Essential.Culture.Generator` | `1.4.0` | P04, P06, P07, P13, P15, P17, P19, P20, P21, P22 | `Arkheide.Essential.Culture/1.4.0` | Build/code generation: Transitive Roslyn/MSBuild key generation, module manifest and build/publish resource deployment. | [repository](https://github.com/Evigila/Essential); `MIT` | Assets(P04, P06, P07, P13, P15, P17, P19, P20, P21, P22), Cache(`Arkheide.Essential.Culture.Generator/1.4.0`) |
| `Arkheide.Essential.Culture` | `1.4.0` | P04, P06, P07, P13, P15, P17, P19, P20, P21, P22 | `Arkheide.Essential.Culture.Blazor/1.4.0`<br>`Arkheide.Essential.Culture.Wpf/1.4.0` | Production: Catalog parsing/merging, fallback lookup, culture policy and isolated/static localization contexts. | [repository](https://github.com/Evigila/Essential); `MIT` | Assets(P04, P06, P07, P13, P15, P17, P19, P20, P21, P22), Cache(`Arkheide.Essential.Culture/1.4.0`) |
| `Castle.Core` | `5.1.1` | P18 | `Moq/4.20.72` | Test: Dynamic proxy implementation used by Moq. | [repository](https://github.com/castleproject/Core); `Apache-2.0` | Assets(P18), Cache(`Castle.Core/5.1.1`) |
| `Microsoft.CodeCoverage` | `17.14.1` | P18, P20, P21 | `Microsoft.NET.Test.Sdk/17.14.1` | Test: VSTest code coverage tooling. | [repository](https://github.com/microsoft/vstest); `MIT` | Assets(P18, P20, P21), Cache(`Microsoft.CodeCoverage/17.14.1`) |
| `Microsoft.Extensions.Configuration.Abstractions` | `10.0.8` | P04, P05, P18, P19 | `Microsoft.Extensions.Configuration.FileExtensions/10.0.8`<br>`Microsoft.Extensions.Configuration.Json/10.0.8`<br>`Microsoft.Extensions.Configuration/10.0.8`<br>`Microsoft.Extensions.Hosting.Abstractions/10.0.8` | Production: Configuration contracts. | [repository](https://github.com/dotnet/dotnet); `MIT` | Assets(P04, P05, P18, P19), Cache(`Microsoft.Extensions.Configuration.Abstractions/10.0.8`) |
| `Microsoft.Extensions.Configuration.FileExtensions` | `10.0.8` | P04, P05, P18, P19 | `Microsoft.Extensions.Configuration.Json/10.0.8` | Production: File-backed configuration sources and reload support. | [repository](https://github.com/dotnet/dotnet); `MIT` | Assets(P04, P05, P18, P19), Cache(`Microsoft.Extensions.Configuration.FileExtensions/10.0.8`) |
| `Microsoft.Extensions.Configuration` | `10.0.8` | P04, P05, P18, P19 | `Microsoft.Extensions.Configuration.FileExtensions/10.0.8`<br>`Microsoft.Extensions.Configuration.Json/10.0.8` | Production: Configuration builder/provider implementation. | [repository](https://github.com/dotnet/dotnet); `MIT` | Assets(P04, P05, P18, P19), Cache(`Microsoft.Extensions.Configuration/10.0.8`) |
| `Microsoft.Extensions.DependencyInjection.Abstractions` | `10.0.8` | P04, P05, P18, P19 | `Microsoft.Extensions.Diagnostics.Abstractions/10.0.8`<br>`Microsoft.Extensions.Hosting.Abstractions/10.0.8`<br>`Microsoft.Extensions.Logging.Abstractions/10.0.8`<br>`Microsoft.Extensions.Options/10.0.8` | Production: Dependency injection contracts. | [repository](https://github.com/dotnet/dotnet); `MIT` | Assets(P04, P05, P18, P19), Cache(`Microsoft.Extensions.DependencyInjection.Abstractions/10.0.8`) |
| `Microsoft.Extensions.Diagnostics.Abstractions` | `10.0.8` | P04, P05, P18, P19 | `Microsoft.Extensions.Hosting.Abstractions/10.0.8` | Production: Hosting diagnostics contracts. | [repository](https://github.com/dotnet/dotnet); `MIT` | Assets(P04, P05, P18, P19), Cache(`Microsoft.Extensions.Diagnostics.Abstractions/10.0.8`) |
| `Microsoft.Extensions.FileProviders.Abstractions` | `10.0.8` | P04, P05, P18, P19 | `Microsoft.Extensions.Configuration.FileExtensions/10.0.8`<br>`Microsoft.Extensions.Configuration.Json/10.0.8`<br>`Microsoft.Extensions.FileProviders.Physical/10.0.8`<br>`Microsoft.Extensions.Hosting.Abstractions/10.0.8` | Production: File provider contracts. | [repository](https://github.com/dotnet/dotnet); `MIT` | Assets(P04, P05, P18, P19), Cache(`Microsoft.Extensions.FileProviders.Abstractions/10.0.8`) |
| `Microsoft.Extensions.FileProviders.Physical` | `10.0.8` | P04, P05, P18, P19 | `Microsoft.Extensions.Configuration.FileExtensions/10.0.8` | Production: Physical file provider for file-backed configuration. | [repository](https://github.com/dotnet/dotnet); `MIT` | Assets(P04, P05, P18, P19), Cache(`Microsoft.Extensions.FileProviders.Physical/10.0.8`) |
| `Microsoft.Extensions.FileSystemGlobbing` | `10.0.8` | P04, P05, P18, P19 | `Microsoft.Extensions.FileProviders.Physical/10.0.8` | Production: File matching used by physical file providers. | [repository](https://github.com/dotnet/dotnet); `MIT` | Assets(P04, P05, P18, P19), Cache(`Microsoft.Extensions.FileSystemGlobbing/10.0.8`) |
| `Microsoft.Extensions.Options` | `10.0.8` | P04, P05, P18, P19 | `Microsoft.Extensions.Diagnostics.Abstractions/10.0.8` | Production: Options contracts and infrastructure. | [repository](https://github.com/dotnet/dotnet); `MIT` | Assets(P04, P05, P18, P19), Cache(`Microsoft.Extensions.Options/10.0.8`) |
| `Microsoft.Extensions.Primitives` | `10.0.8` | P04, P05, P18, P19 | `Microsoft.Extensions.Configuration.Abstractions/10.0.8`<br>`Microsoft.Extensions.Configuration.FileExtensions/10.0.8`<br>`Microsoft.Extensions.Configuration/10.0.8`<br>`Microsoft.Extensions.FileProviders.Abstractions/10.0.8`<br>`Microsoft.Extensions.FileProviders.Physical/10.0.8`<br>`Microsoft.Extensions.Options/10.0.8` | Production: Change tokens and shared extension primitives. | [repository](https://github.com/dotnet/dotnet); `MIT` | Assets(P04, P05, P18, P19), Cache(`Microsoft.Extensions.Primitives/10.0.8`) |
| `Microsoft.TestPlatform.ObjectModel` | `17.14.1` | P18, P20, P21 | `Microsoft.TestPlatform.TestHost/17.14.1` | Test: VSTest object model and adapter interfaces. | [repository](https://github.com/microsoft/vstest); `MIT` | Assets(P18, P20, P21), Cache(`Microsoft.TestPlatform.ObjectModel/17.14.1`) |
| `Microsoft.TestPlatform.TestHost` | `17.14.1` | P18, P20, P21 | `Microsoft.NET.Test.Sdk/17.14.1` | Test: VSTest test host. | [repository](https://github.com/microsoft/vstest); `MIT` | Assets(P18, P20, P21), Cache(`Microsoft.TestPlatform.TestHost/17.14.1`) |
| `Microsoft.Web.WebView2` | `1.0.3719.77` | P08, P14 | `Microsoft.WindowsAppSDK.WinUI/2.3.0` | WinUI production/build: WinUI package dependency for WebView integration; native WPF implementation does not depend on it. | [package project](https://aka.ms/webview); Package license file `LICENSE.txt`; SPDX identity unknown | Assets(P08, P14), Cache(`Microsoft.Web.WebView2/1.0.3719.77`) |
| `Microsoft.Windows.AI.MachineLearning` | `2.1.74` | P08, P14 | `Microsoft.WindowsAppSDK.ML/2.1.74` | WinUI production/build: Windows ML payload brought in by WindowsAppSDK.ML. | [package project](https://github.com/microsoft/windowsappsdk); Package license file `license.txt`; SPDX identity unknown | Assets(P08, P14), Cache(`Microsoft.Windows.AI.MachineLearning/2.1.74`) |
| `Microsoft.Windows.SDK.BuildTools.MSIX` | `1.7.251221100` | P08, P14 | `Microsoft.WindowsAppSDK.Base/2.0.4` | WinUI build/deployment: MSIX build tooling brought in by WindowsAppSDK.Base. | [package project](https://aka.ms/WinSDKProjectURL); Package license file `sdk_license.txt`; SPDX identity unknown | Assets(P08, P14), Cache(`Microsoft.Windows.SDK.BuildTools.MSIX/1.7.251221100`) |
| `Microsoft.Windows.SDK.BuildTools` | `10.0.26100.4654` | P08, P14 | `Microsoft.WindowsAppSDK.Base/2.0.4` | WinUI build/deployment: Windows SDK build tools brought in by WindowsAppSDK.Base. | [package project](https://aka.ms/WinSDKProjectURL); License identity unknown; nuspec supplies [license URL](https://aka.ms/WinSDKLicenseURL) | Assets(P08, P14), Cache(`Microsoft.Windows.SDK.BuildTools/10.0.26100.4654`) |
| `Microsoft.WindowsAppSDK.AI` | `2.3.4` | P08, P14 | `Microsoft.WindowsAppSDK/2.3.1` | WinUI production/build: Windows App SDK AI feature payload. | [package project](https://github.com/microsoft/windowsappsdk); Package license file `license.txt`; SPDX identity unknown | Assets(P08, P14), Cache(`Microsoft.WindowsAppSDK.AI/2.3.4`) |
| `Microsoft.WindowsAppSDK.Base` | `2.0.4` | P08, P14 | `Microsoft.WindowsAppSDK.AI/2.3.4`<br>`Microsoft.WindowsAppSDK.DWrite/2.1.0`<br>`Microsoft.WindowsAppSDK.Foundation/2.3.5`<br>`Microsoft.WindowsAppSDK.InteractiveExperiences/2.1.3`<br>`Microsoft.WindowsAppSDK.ML/2.1.74`<br>`Microsoft.WindowsAppSDK.Runtime/2.3.1`<br>`Microsoft.WindowsAppSDK.Widgets/2.0.5`<br>`Microsoft.WindowsAppSDK.WinUI/2.3.0`<br>`Microsoft.WindowsAppSDK/2.3.1` | WinUI production/build: Shared Windows App SDK build/runtime foundation. | [package project](https://github.com/microsoft/windowsappsdk); Package license file `license.txt`; SPDX identity unknown | Assets(P08, P14), Cache(`Microsoft.WindowsAppSDK.Base/2.0.4`) |
| `Microsoft.WindowsAppSDK.DWrite` | `2.1.0` | P08, P14 | `Microsoft.WindowsAppSDK/2.3.1` | WinUI production/build: Windows App SDK DirectWrite feature payload. | [package project](https://aka.ms/WindowsAppSDK); Package license file `license.txt`; SPDX identity unknown | Assets(P08, P14), Cache(`Microsoft.WindowsAppSDK.DWrite/2.1.0`) |
| `Microsoft.WindowsAppSDK.Foundation` | `2.3.5` | P08, P14 | `Microsoft.WindowsAppSDK.AI/2.3.4`<br>`Microsoft.WindowsAppSDK.ML/2.1.74`<br>`Microsoft.WindowsAppSDK.WinUI/2.3.0`<br>`Microsoft.WindowsAppSDK/2.3.1` | WinUI production/build: Windows App SDK foundational platform payload. | [package project](https://aka.ms/windowsappsdk); Package license file `license.txt`; SPDX identity unknown | Assets(P08, P14), Cache(`Microsoft.WindowsAppSDK.Foundation/2.3.5`) |
| `Microsoft.WindowsAppSDK.InteractiveExperiences` | `2.1.3` | P08, P14 | `Microsoft.WindowsAppSDK.Foundation/2.3.5`<br>`Microsoft.WindowsAppSDK.WinUI/2.3.0`<br>`Microsoft.WindowsAppSDK/2.3.1` | WinUI production/build: Windows App SDK interaction feature payload. | [package project](https://aka.ms/windowsappsdk); Package license file `license.txt`; SPDX identity unknown | Assets(P08, P14), Cache(`Microsoft.WindowsAppSDK.InteractiveExperiences/2.1.3`) |
| `Microsoft.WindowsAppSDK.ML` | `2.1.74` | P08, P14 | `Microsoft.WindowsAppSDK/2.3.1` | WinUI production/build: Windows App SDK ML feature payload. | [package project](https://github.com/microsoft/windowsappsdk); Package license file `license.txt`; SPDX identity unknown | Assets(P08, P14), Cache(`Microsoft.WindowsAppSDK.ML/2.1.74`) |
| `Microsoft.WindowsAppSDK.Runtime` | `2.3.1` | P08, P14 | `Microsoft.WindowsAppSDK/2.3.1` | WinUI production/build: Windows App SDK runtime distribution. | [package project](https://github.com/microsoft/windowsappsdk); Package license file `license.txt`; SPDX identity unknown | Assets(P08, P14), Cache(`Microsoft.WindowsAppSDK.Runtime/2.3.1`) |
| `Microsoft.WindowsAppSDK.Widgets` | `2.0.5` | P08, P14 | `Microsoft.WindowsAppSDK/2.3.1` | WinUI production/build: Windows App SDK widget feature payload. | [package project](https://github.com/microsoft/windowsappsdk); Package license file `license.txt`; SPDX identity unknown | Assets(P08, P14), Cache(`Microsoft.WindowsAppSDK.Widgets/2.0.5`) |
| `Microsoft.WindowsAppSDK.WinUI` | `2.3.0` | P08, P14 | `Microsoft.WindowsAppSDK/2.3.1` | WinUI production/build: WinUI controls and XAML tooling. | [package project](https://github.com/microsoft/windowsappsdk); Package license file `license.txt`; SPDX identity unknown | Assets(P08, P14), Cache(`Microsoft.WindowsAppSDK.WinUI/2.3.0`) |
| `Newtonsoft.Json` | `13.0.3` | P18, P20, P21 | `Microsoft.TestPlatform.TestHost/17.14.1` | Test: VSTest test-host JSON serialization. | [repository](https://github.com/JamesNK/Newtonsoft.Json); `MIT` | Assets(P18, P20, P21), Cache(`Newtonsoft.Json/13.0.3`) |
| `System.Diagnostics.EventLog` | `6.0.0` | P18 | `Castle.Core/5.1.1` | Test: Castle platform event-log dependency; not a Gallery business service. | [repository](https://github.com/dotnet/runtime); `MIT` | Assets(P18), Cache(`System.Diagnostics.EventLog/6.0.0`) |
| `System.Numerics.Tensors` | `9.0.0` | P08, P14 | `Microsoft.Windows.AI.MachineLearning/2.1.74` | WinUI production/build: Windows ML tensor dependency; not directly called by Gallery. | [repository](https://github.com/dotnet/runtime); `MIT` | Assets(P08, P14), Cache(`System.Numerics.Tensors/9.0.0`) |
| `xunit.abstractions` | `2.0.3` | P18, P20, P21 | `xunit.extensibility.core/2.9.3` | Test: xUnit abstraction contracts. | [package project](https://github.com/xunit/xunit); License identity unknown; nuspec supplies [license URL](https://raw.githubusercontent.com/xunit/xunit/master/license.txt) | Assets(P18, P20, P21), Cache(`xunit.abstractions/2.0.3`) |
| `xunit.analyzers` | `1.18.0` | P18, P20, P21 | `xunit/2.9.3` | Test: xUnit compile-time analyzers. | [repository](https://github.com/xunit/xunit.analyzers); `Apache-2.0` | Assets(P18, P20, P21), Cache(`xunit.analyzers/1.18.0`) |
| `xunit.assert` | `2.9.3` | P18, P20, P21 | `xunit/2.9.3` | Test: xUnit assertions. | [repository](https://github.com/xunit/xunit); `Apache-2.0` | Assets(P18, P20, P21), Cache(`xunit.assert/2.9.3`) |
| `xunit.core` | `2.9.3` | P18, P20, P21 | `xunit/2.9.3` | Test: xUnit test construction/build entry. | [repository](https://github.com/xunit/xunit); `Apache-2.0` | Assets(P18, P20, P21), Cache(`xunit.core/2.9.3`) |
| `xunit.extensibility.core` | `2.9.3` | P18, P20, P21 | `xunit.core/2.9.3`<br>`xunit.extensibility.execution/2.9.3` | Test: xUnit discovery/extensibility contracts. | [repository](https://github.com/xunit/xunit); `Apache-2.0` | Assets(P18, P20, P21), Cache(`xunit.extensibility.core/2.9.3`) |
| `xunit.extensibility.execution` | `2.9.3` | P18, P20, P21 | `xunit.core/2.9.3` | Test: xUnit execution implementation. | [repository](https://github.com/xunit/xunit); `Apache-2.0` | Assets(P18, P20, P21), Cache(`xunit.extensibility.execution/2.9.3`) |

## SDKs, shared frameworks, operating systems and runtime packs

The observed SDK/pack/runtime versions in this table are the historical audit environment, not an assertion that those versions remain installed for the current local candidate. The candidate retains the existing `global.json` constraint; its actual selected SDK/assets are recorded with the coordinating build evidence above. No SDK or runtime installation is performed by this update.

| Dependency / kind | Declared constraint | Observed version or contract | Consumers / scope / purpose | Evidence / source / license |
| --- | --- | --- | --- | --- |
| .NET SDK / build toolchain | `10.0.400`, `rollForward=latestPatch`, `allowPrerelease=false`; CI requests `10.0.x` | Locally selected `10.0.401`; `10.0.301` is also installed but does not satisfy this feature-band pin | P01–P22; build, restore, pack, publish, Roslyn, Razor/XAML compilation | `global.json`, CLI, CI; local Microsoft SDK distribution; its installed `LICENSE.txt` states Microsoft .NET Library terms, not an inferred uniform SPDX license for every SDK payload. |
| `Microsoft.NET.Sdk`, `.Web`, `.Razor` / SDK components | Included in selected .NET SDK; all projects target .NET 10 | SDK 10.0.401 | `.Web`: P13/P16; `.Razor`: P02/P03/P06; other projects `.Sdk`; build | `.csproj` Sdk attributes. `build/CssBundle.targets` uses SDK `RoslynCodeTaskFactory`/`Microsoft.Build.Tasks.Core.dll`; no CSS bundler package. |
| `Microsoft.NETCore.App` / shared runtime and targeting pack | `net10.0` family; SDK default runtime baseline 10.0.0 | 10.0.12 targeting pack / installed runtime; 10.0.9 also installed | All managed projects; application/test runtime | CLI, current assets frameworkReferences, SDK BundledVersions.props. Framework-delivered assemblies are covered by this distribution rather than separate NuGet rows. |
| `Microsoft.AspNetCore.App` / shared web framework | Explicit FrameworkReference in P01/P02/P03/P06/P17/P22, implicit in P13/P16 | 10.0.12 targeting pack / installed runtime; 10.0.9 also installed | Blazor controls, interactive server Gallery and console renderer tests; production/build/test | Current project assets, .csproj, SDK/CLI. Its runtime supplies hosting, routing, DataProtection, logging, JS interop and Blazor SignalR transport. |
| `Microsoft.WindowsDesktop.App.WPF` / native desktop framework | `UseWPF=true`, net10.0-windows | 10.0.12 targeting/runtime distribution; 10.0.9 also installed | P07/P09/P10/P11/P15/P20/P21; Windows-only WPF build/runtime/test | .csproj/current assets and CLI. P12 is a dependency-only metapackage. |
| Windows desktop OS / platform contract | WPF requires Windows; WinUI minimum `10.0.17763.0`, target `10.0.26100.0` | Installed OS version not inspected; declared minimum and target recorded separately | Native WPF/WinUI projects; production/build/test | UseWPF/UseWinUI and WinUI TargetPlatformMinVersion/SupportedOSPlatformVersion; vendor OS license/version not audited. |
| Windows XAML/WinUI build tools / SDK contract | Windows-compatible XAML tooling and architecture; WinUI library allows AnyCPU/x86/x64/ARM64; Gallery x86/x64/ARM64 | Runtime IDs `win-x86`, `win-x64`, `win-arm64`; tool install/version beyond resolved NuGet packs unknown | P08/P14; WinUI build/deployment. Gallery sets WindowsPackageType=None and WindowsAppSDKSelfContained=true | WinUI .csproj and assets. WindowsAppSDKSelfContained concerns that payload; it does not by itself prove a .NET self-contained release. |
| Windows DirectWrite / native asset conversion API | IDWriteFactory5.UnpackFontFile and IDWriteFactory6.CreateFontFaceReference in `dwrite.dll` | OS API contract; no package version | `scripts/Convert-MaterialIcons.ps1`; optional maintenance/build asset conversion | P/Invoke in the converter, icon source manifests. Not a downloaded conversion tool and not a separate WPF browser runtime. |
| Common icon asset regeneration / optional maintenance | Existing Python 3.12.14 standard-library zlib/struct; PowerShell 7 and Windows PowerShell/WPF GlyphTypeface.ComputeSubset | No new package or installation; native/standard-library tools only | scripts/Subset-MaterialIcons.py, common-icons.json; original glyph IDs and all retained variation deltas checked independently | Checked-in WOFF assets mean normal source builds and consumers need no Python, WPF or regeneration step. |
| Browser DOM/CSS APIs / client runtime | Browser supporting the library's ResizeObserver, MutationObserver, IntersectionObserver, pointer/focus, clipboard and modern CSS contracts | Browser/version matrix unknown; no browser package pin | P03/P06/P13/P16 and JS tests; production client behavior | Framework wwwroot modules, bridge browser-preferences.js and Design CSS. HTTP/browser protocol inputs do not establish external business services. |
| Tool framework runtimes / development | DocFX net10 payload needs NETCore.App/ASP.NETCore.App 10.0.0; CSharpier net10 payload needs NETCore.App 10.0.0; both Major roll-forward | Matching 10.0.12 runtimes available; DocFX MSBuild BuildHost requests NETCore.App 8.0.0 with Major roll-forward | Documentation/formatting tools only | Cached tool runtimeconfigs. The BuildHost can roll forward; exact runtime chosen for it was not measured. |

WinUI's current `project.frameworks[*].downloadDependencies` asks for the following **ten exact SDK-selected packs**. These are recorded separately because download packs are not application PackageReference graph entries. The nine .NET packs have cached MIT nuspec expressions; Windows SDK reference license identity is unknown (its nuspec supplies a Windows SDK license URL). All ten cache metadata files record NuGet.org.

| Download pack | Exact requested/resolved cache version | Consumers / purpose | Evidence |
| --- | --- | --- | --- |
| `Microsoft.AspNetCore.App.Runtime.win-arm64` | `10.0.12` | P08/P14; SDK platform restore/runtime distribution | Assets(P08/P14) downloadDependencies and Cache(`Microsoft.AspNetCore.App.Runtime.win-arm64/10.0.12`) |
| `Microsoft.AspNetCore.App.Runtime.win-x64` | `10.0.12` | P08/P14; SDK platform restore/runtime distribution | Assets(P08/P14) downloadDependencies and Cache(`Microsoft.AspNetCore.App.Runtime.win-x64/10.0.12`) |
| `Microsoft.AspNetCore.App.Runtime.win-x86` | `10.0.12` | P08/P14; SDK platform restore/runtime distribution | Assets(P08/P14) downloadDependencies and Cache(`Microsoft.AspNetCore.App.Runtime.win-x86/10.0.12`) |
| `Microsoft.NETCore.App.Runtime.win-arm64` | `10.0.12` | P08/P14; SDK platform restore/runtime distribution | Assets(P08/P14) downloadDependencies and Cache(`Microsoft.NETCore.App.Runtime.win-arm64/10.0.12`) |
| `Microsoft.NETCore.App.Runtime.win-x64` | `10.0.12` | P08/P14; SDK platform restore/runtime distribution | Assets(P08/P14) downloadDependencies and Cache(`Microsoft.NETCore.App.Runtime.win-x64/10.0.12`) |
| `Microsoft.NETCore.App.Runtime.win-x86` | `10.0.12` | P08/P14; SDK platform restore/runtime distribution | Assets(P08/P14) downloadDependencies and Cache(`Microsoft.NETCore.App.Runtime.win-x86/10.0.12`) |
| `Microsoft.WindowsDesktop.App.Runtime.win-arm64` | `10.0.12` | P08/P14; SDK platform restore/runtime distribution | Assets(P08/P14) downloadDependencies and Cache(`Microsoft.WindowsDesktop.App.Runtime.win-arm64/10.0.12`) |
| `Microsoft.WindowsDesktop.App.Runtime.win-x64` | `10.0.12` | P08/P14; SDK platform restore/runtime distribution | Assets(P08/P14) downloadDependencies and Cache(`Microsoft.WindowsDesktop.App.Runtime.win-x64/10.0.12`) |
| `Microsoft.WindowsDesktop.App.Runtime.win-x86` | `10.0.12` | P08/P14; SDK platform restore/runtime distribution | Assets(P08/P14) downloadDependencies and Cache(`Microsoft.WindowsDesktop.App.Runtime.win-x86/10.0.12`) |
| `Microsoft.Windows.SDK.NET.Ref` | `10.0.26100.57` | P08/P14; Windows SDK .NET compile-time references | Assets(P08/P14) downloadDependencies; cached nuspec/metadata |

## Development and documentation tool packages

| Direct tool | Declared pin | Observed package | Consumer and purpose | Source / license / evidence |
| --- | --- | --- | --- | --- |
| DocFX | `2.78.5`, tool manifest rollForward=false | Cached `docfx/2.78.5`, with net8/net9/net10 payloads | Documentation workflow and human DocFX configs; metadata generation, templates and static-site build | `.config/dotnet-tools.json`; cached nuspec links `https://github.com/dotnet/docfx`, MIT; acquisition feed unknown in the cached tool metadata. |
| CSharpier | `1.3.0`, tool manifest rollForward=false | Cached `CSharpier/1.3.0`, with net8/net9/net10 payloads | Optional repository code formatting; not invoked by current release/build CI | Tool manifest; cached nuspec links `https://github.com/belav/csharpier`, MIT; acquisition feed unknown in the cached tool metadata. |

The next table completely enumerates the **68 distinct external bundled package pairs** in the available net10 tool payloads, including DocFX's nested MSBuild BuildHost. Relationship is *bundled transitive development dependency*, not a new project PackageReference. Versions are observed in the relevant `.deps.json`; matching cache nuspec metadata is used only when present. Missing per-package source/license metadata remains unknown rather than inheriting the tool's license. The net8/net9 alternative tool payloads are not the selected SDK's normal tool target and are outside this net10 observation.

`D` = cached `docfx/2.78.5/tools/net10.0/any/docfx.deps.json`; `H` = its `BuildHost-netcore/...BuildHost.deps.json`; `C` = cached `csharpier/1.3.0/tools/net10.0/any/CSharpier.deps.json`. `DocFXBuildHost` is DocFX's metadata extraction subprocess.

| Bundled package | Observed version | Tool consumer / purpose | Source / license | Evidence |
| --- | --- | --- | --- | --- |
| `Acornima` | `1.2.0` | DocFX: JavaScript parsing/execution support | Source repository unknown; License identity unknown | D |
| `HtmlAgilityPack` | `1.12.4` | DocFX: Markdown/HTML/template/text processing | Source repository unknown; License identity unknown | D |
| `Humanizer.Core` | `2.14.1` | DocFX: Markdown/HTML/template/text processing | [repository](https://github.com/Humanizr/Humanizer); `MIT` | D; matching cached nuspec |
| `ICSharpCode.Decompiler` | `9.1.0.7988` | DocFX: Code analysis and API metadata/formatting | Source repository unknown; License identity unknown | D |
| `ini-parser-netstandard` | `2.5.3` | CSharpier: Structured document/configuration parsing | Source repository unknown; License identity unknown | C |
| `Jint` | `4.6.0` | DocFX: JavaScript parsing/execution support | Source repository unknown; License identity unknown | D |
| `Json.More.Net` | `2.1.1` | DocFX: Structured document/configuration parsing | Source repository unknown; License identity unknown | D |
| `JsonPointer.Net` | `5.3.1` | DocFX: Structured document/configuration parsing | Source repository unknown; License identity unknown | D |
| `JsonSchema.Net` | `7.4.0` | DocFX: Structured document/configuration parsing | Source repository unknown; License identity unknown | D |
| `LovettSoftware.XmlDiff` | `1.1.0` | CSharpier: Structured document/configuration parsing | Source repository unknown; License identity unknown | C |
| `Markdig` | `1.0.0` | DocFX: Markdown/HTML/template/text processing | Source repository unknown; License identity unknown | D |
| `Microsoft.Bcl.AsyncInterfaces` | `9.0.0` | DocFX: Tool runtime support | Source repository unknown; License identity unknown | D |
| `Microsoft.Build.Framework` | `18.0.2` | DocFX: MSBuild/project/solution loading | [repository](https://github.com/dotnet/msbuild); `MIT` | D; matching cached nuspec |
| `Microsoft.Build.Locator` | `1.10.2` | DocFXBuildHost: MSBuild/project/solution loading | Source repository unknown; License identity unknown | H |
| `Microsoft.Build` | `18.0.2` | DocFX: MSBuild/project/solution loading | Source repository unknown; License identity unknown | D |
| `Microsoft.CodeAnalysis.Common` | `5.0.0` | DocFX: Code analysis and API metadata/formatting | [repository](https://github.com/dotnet/roslyn); `MIT` | D; matching cached nuspec |
| `Microsoft.CodeAnalysis.Common` | `5.3.0` | CSharpier: Code analysis and API metadata/formatting | [repository](https://github.com/dotnet/roslyn); `MIT` | C; matching cached nuspec |
| `Microsoft.CodeAnalysis.CSharp.Workspaces` | `5.0.0` | DocFX: Code analysis and API metadata/formatting | [repository](https://github.com/dotnet/roslyn); `MIT` | D; matching cached nuspec |
| `Microsoft.CodeAnalysis.CSharp` | `5.0.0` | DocFX: Code analysis and API metadata/formatting | [repository](https://github.com/dotnet/roslyn); `MIT` | D; matching cached nuspec |
| `Microsoft.CodeAnalysis.CSharp` | `5.3.0` | CSharpier: Code analysis and API metadata/formatting | [repository](https://github.com/dotnet/roslyn); `MIT` | C; matching cached nuspec |
| `Microsoft.CodeAnalysis.VisualBasic.Workspaces` | `5.0.0` | DocFX: Code analysis and API metadata/formatting | Source repository unknown; License identity unknown | D |
| `Microsoft.CodeAnalysis.VisualBasic` | `5.0.0` | DocFX: Code analysis and API metadata/formatting | Source repository unknown; License identity unknown | D |
| `Microsoft.CodeAnalysis.Workspaces.Common` | `5.0.0` | DocFX: Code analysis and API metadata/formatting | [repository](https://github.com/dotnet/roslyn); `MIT` | D; matching cached nuspec |
| `Microsoft.CodeAnalysis.Workspaces.MSBuild` | `5.0.0` | DocFX: Code analysis and API metadata/formatting | [repository](https://github.com/dotnet/roslyn); `MIT` | D; matching cached nuspec |
| `Microsoft.Extensions.Configuration.Abstractions` | `8.0.0` | CSharpier: Tool configuration/DI/options/logging | Source repository unknown; License identity unknown | C |
| `Microsoft.Extensions.Configuration.Binder` | `8.0.2` | CSharpier: Tool configuration/DI/options/logging | Source repository unknown; License identity unknown | C |
| `Microsoft.Extensions.Configuration` | `8.0.0` | CSharpier: Tool configuration/DI/options/logging | Source repository unknown; License identity unknown | C |
| `Microsoft.Extensions.DependencyInjection.Abstractions` | `9.0.10` | CSharpier: Tool configuration/DI/options/logging | Source repository unknown; License identity unknown | C |
| `Microsoft.Extensions.DependencyInjection` | `9.0.10` | CSharpier: Tool configuration/DI/options/logging | Source repository unknown; License identity unknown | C |
| `Microsoft.Extensions.Logging.Abstractions` | `9.0.10` | CSharpier: Tool configuration/DI/options/logging | Source repository unknown; License identity unknown | C |
| `Microsoft.Extensions.Logging.Configuration` | `8.0.1` | CSharpier: Tool configuration/DI/options/logging | Source repository unknown; License identity unknown | C |
| `Microsoft.Extensions.Logging` | `9.0.10` | CSharpier: Tool configuration/DI/options/logging | Source repository unknown; License identity unknown | C |
| `Microsoft.Extensions.Options.ConfigurationExtensions` | `8.0.0` | CSharpier: Tool configuration/DI/options/logging | Source repository unknown; License identity unknown | C |
| `Microsoft.Extensions.Options` | `9.0.10` | CSharpier: Tool configuration/DI/options/logging | Source repository unknown; License identity unknown | C |
| `Microsoft.Extensions.Primitives` | `9.0.10` | CSharpier: Tool configuration/DI/options/logging | Source repository unknown; License identity unknown | C |
| `Microsoft.NET.StringTools` | `18.0.2` | DocFX: MSBuild/project/solution loading | Source repository unknown; License identity unknown | D |
| `Microsoft.Playwright` | `1.58.0` | DocFX: Bundled documentation rendering/diagram/PDF support; feature use not established | Source repository unknown; License identity unknown | D |
| `Microsoft.VisualStudio.SolutionPersistence` | `1.0.52` | DocFX: MSBuild/project/solution loading | [repository](https://github.com/microsoft/vs-solutionpersistence); `MIT` | D; matching cached nuspec |
| `Newtonsoft.Json` | `13.0.3` | DocFXBuildHost: Structured document/configuration parsing | [repository](https://github.com/JamesNK/Newtonsoft.Json); `MIT` | H; matching cached nuspec |
| `Newtonsoft.Json` | `13.0.4` | DocFX: Structured document/configuration parsing | [repository](https://github.com/JamesNK/Newtonsoft.Json); `MIT` | D; matching cached nuspec |
| `NReco.Logging.File` | `1.2.2` | CSharpier: Tool configuration/DI/options/logging | Source repository unknown; License identity unknown | C |
| `OneOf` | `3.0.271` | DocFX: Tool runtime support | Source repository unknown; License identity unknown | D |
| `PdfPig` | `0.1.13` | DocFX: Bundled documentation rendering/diagram/PDF support; feature use not established | Source repository unknown; License identity unknown | D |
| `PlantUml.Net` | `1.4.80` | DocFX: Bundled documentation rendering/diagram/PDF support; feature use not established | Source repository unknown; License identity unknown | D |
| `Spectre.Console.Cli` | `0.53.1` | DocFX: Tool command-line infrastructure | Source repository unknown; License identity unknown | D |
| `Spectre.Console` | `0.54.0` | DocFX: Tool command-line infrastructure | Source repository unknown; License identity unknown | D |
| `Stubble.Core` | `1.10.8` | DocFX: Markdown/HTML/template/text processing | Source repository unknown; License identity unknown | D |
| `System.Collections.Immutable` | `9.0.0` | DocFXBuildHost: Tool runtime support | [repository](https://github.com/dotnet/runtime); `MIT` | H; matching cached nuspec |
| `System.CommandLine` | `2.0.0-beta4.22272.1` | CSharpier: Tool command-line infrastructure | Source repository unknown; License identity unknown | C |
| `System.CommandLine` | `2.0.0-rtm.25509.106` | DocFXBuildHost: Tool command-line infrastructure | Source repository unknown; License identity unknown | H |
| `System.Composition.AttributedModel` | `10.0.3` | DocFX: Tool component composition | Source repository unknown; License identity unknown | D |
| `System.Composition.Convention` | `10.0.3` | DocFX: Tool component composition | Source repository unknown; License identity unknown | D |
| `System.Composition.Hosting` | `10.0.3` | DocFX: Tool component composition | Source repository unknown; License identity unknown | D |
| `System.Composition.Runtime` | `10.0.3` | DocFX: Tool component composition | Source repository unknown; License identity unknown | D |
| `System.Composition.TypedParts` | `10.0.3` | DocFX: Tool component composition | Source repository unknown; License identity unknown | D |
| `System.Composition` | `10.0.3` | DocFX: Tool component composition | Source repository unknown; License identity unknown | D |
| `System.Configuration.ConfigurationManager` | `9.0.0` | DocFX: Tool runtime support | Source repository unknown; License identity unknown | D |
| `System.Diagnostics.EventLog` | `9.0.0` | DocFX: Tool runtime support | Source repository unknown; License identity unknown | D |
| `System.IO.Abstractions.TestingHelpers` | `22.0.16` | CSharpier: Tool file-system abstraction | Source repository unknown; License identity unknown | C |
| `System.IO.Abstractions` | `22.1.1` | CSharpier: Tool file-system abstraction | Source repository unknown; License identity unknown | C |
| `System.IO.Hashing` | `9.0.10` | CSharpier: Tool file-system abstraction | Source repository unknown; License identity unknown | C |
| `System.Reflection.MetadataLoadContext` | `9.0.0` | DocFX: Code analysis and API metadata/formatting | Source repository unknown; License identity unknown | D |
| `System.Security.Cryptography.ProtectedData` | `9.0.6` | DocFX: Tool runtime support | [repository](https://github.com/dotnet/runtime); `MIT` | D; matching cached nuspec |
| `TestableIO.System.IO.Abstractions.TestingHelpers` | `22.0.16` | CSharpier: Tool file-system abstraction | Source repository unknown; License identity unknown | C |
| `TestableIO.System.IO.Abstractions.Wrappers` | `22.1.1` | CSharpier: Tool file-system abstraction | Source repository unknown; License identity unknown | C |
| `TestableIO.System.IO.Abstractions` | `22.1.1` | CSharpier: Tool file-system abstraction | Source repository unknown; License identity unknown | C |
| `Testably.Abstractions.FileSystem.Interface` | `10.1.0` | CSharpier: Tool file-system abstraction | Source repository unknown; License identity unknown | C |
| `YamlDotNet` | `16.3.0` | DocFX, CSharpier: Structured document/configuration parsing | [repository](https://github.com/aaubry/YamlDotNet); `MIT` | D, C; matching cached nuspec |

Internal tool assemblies marked `type=project` in those payloads are also accounted for: `Docfx.App/2.78.5` (DocFX), `Docfx.Build.Common/2.78.5` (DocFX), `Docfx.Build.ManagedReference/2.78.5` (DocFX), `Docfx.Build.OverwriteDocuments/2.78.5` (DocFX), `Docfx.Build.RestApi/2.78.5` (DocFX), `Docfx.Build.SchemaDriven/2.78.5` (DocFX), `Docfx.Build.UniversalReference/2.78.5` (DocFX), `Docfx.Build/2.78.5` (DocFX), `Docfx.Common/2.78.5` (DocFX), `Docfx.DataContracts.Common/2.78.5` (DocFX), `Docfx.DataContracts.RestApi/2.78.5` (DocFX), `Docfx.DataContracts.UniversalReference/2.78.5` (DocFX), `Docfx.Dotnet/2.78.5` (DocFX), `Docfx.Glob/2.78.5` (DocFX), `Docfx.MarkdigEngine.Extensions/2.78.5` (DocFX), `Docfx.MarkdigEngine/2.78.5` (DocFX), `Docfx.Plugins/2.78.5` (DocFX), `Docfx.YamlSerialization/2.78.5` (DocFX), `docfx/2.78.5` (DocFX), `Microsoft.CodeAnalysis.Workspaces.MSBuild.BuildHost/5.0.0-2.25567.12` (DocFXBuildHost), `CSharpier.Core/1.3.0` (CSharpier), `CSharpier/1.3.0` (CSharpier). They are assemblies shipped by the pinned tool package, not local Flourish source projects. DocFX default/modern templates and their bundled browser assets are covered by that pinned tool distribution; individual embedded UI library provenance/versions were not extracted during this inspection.

## Build, test, CI and optional development tools

| Dependency / kind | Declared constraint | Observed version | Consumers / scope / purpose | Source / license / evidence |
| --- | --- | --- | --- | --- |
| Node.js / JS test runtime | CI major `24`; no npm/package-lock manifest | Local `v26.4.0` | Node built-in test runner and assertion modules in tests/*.mjs/.js; build/test | build.yml, ReleaseSettings.psd1, CLI. Node distribution source/license not independently inspected. Built-in modules are covered by Node. |
| PowerShell / script runtime | Launcher states 5.1+; fetch skill uses #requires 5.1; release helper invokes pwsh | Local PowerShell `7.6.5`; Windows PowerShell version not separately measured | start/fetch/release/check scripts, CI pwsh shell; build/dev/test | .bat launchers, Start-Project.ps1, fetch-agents.ps1 and workflows; vendor/runtime license identity unknown. |
| Git / source/release tool | No version pin | Local `2.56.0.windows.1` | Framework sync, release ancestry/tag checks, changelog tag discovery; development/release | git CLI, fetch-agents.ps1, Release-Common.ps1, Publish-Helper.ps1; distribution license/source not independently inspected. |
| Visual Studio / optional IDE | No version pin or required IDE startup path | Installed version unknown | Optional editing/debugging and WinUI XAML tool provisioning; no Blazor/WPF launcher requirement | scripts/StartSettings.json and launcher diagnostics. Development BrowserLink/Hot Reload assets are IDE/framework supplied, not project package additions. License unknown. |
| Playwright JS module / optional diagnostic | `require(PLAYWRIGHT_MODULE \|\| 'playwright')`; not declared in a repository package manifest | Unknown/unpinned | Only optional `build/Test-CultureIntegration.cjs` browser integration harness; not ReleaseSettings JavaScriptTests | Harness source. It is distinct from DocFX's bundled `Microsoft.Playwright` NuGet package. Source/license unknown for the caller-supplied JS installation. |
| Installed Edge/Chromium executable / optional headless test tool | No package/version pin; action-menu checks accept `FLOURISH_HEADLESS_BROWSER`, then probe installed Edge/Chrome/Chromium; Culture harness uses `BROWSER_EXECUTABLE` | Microsoft Edge `155.0.4283.45` observed from its installed Windows executable on 2026-10-12; other browsers unknown | `action-menu-dom.mjs`: optional nine SSR geometry regressions alongside controller checks; optional Culture integration harness | Executable file version and test source. No browser/package download; missing executable explicitly skips nine geometry checks. Browser vendor license/distribution identity beyond the installed Microsoft Edge executable remains unverified. |
| Installed AGENTS instruction framework / development workflow | Framework `1.0.0`; source ref `main` | Imported commit `03ec2f6ebacb443585056b83c581f89a99c8ee36` | Router, common standards, audit/sync skills; optional Git-based framework check/sync | AGENTS.framework.json/lock.json; `https://github.com/Evigila/AGENTS.md.git`; source license identity unknown locally. No remote update check performed here. |
| GitHub-hosted Windows/Linux runners / CI platform | `windows-latest` for build/publish/docs build; `ubuntu-latest` for Pages deploy | Image versions and preinstalled tool versions unknown | GitHub Actions workflows; build/test/publish/deployment | Workflow YAML. Floating runner labels are constraints, not exact environment locks; service terms/license identity not inspected. |

CI actions are version-tagged remote dependencies, not exact SHA pins. The complete set of distinct action/tag pairs is below. Their source repository identities are directly given by `uses:`; individual licenses and the commits currently behind tags are unknown in this audit.

| Action | Declared ref | Workflow consumer / purpose |
| --- | --- | --- |
| `actions/checkout` ([source](https://github.com/actions/checkout)) | `v7` | build.yml: source checkout for build/publish |
| `actions/checkout` ([source](https://github.com/actions/checkout)) | `v5` | docs.yml: source checkout |
| `actions/setup-dotnet` ([source](https://github.com/actions/setup-dotnet)) | `v5` | both workflows: .NET 10 installation |
| `actions/setup-node` ([source](https://github.com/actions/setup-node)) | `v7` | build.yml: Node 24 |
| `actions/upload-artifact` ([source](https://github.com/actions/upload-artifact)) | `v7` | build.yml: verified NuGet artifact transfer |
| `actions/download-artifact` ([source](https://github.com/actions/download-artifact)) | `v8` | build.yml: NuGet publication artifact retrieval |
| `NuGet/login` ([source](https://github.com/NuGet/login)) | `v1` | build.yml: OIDC Trusted Publishing login |
| `actions/configure-pages` ([source](https://github.com/actions/configure-pages)) | `v6` | docs.yml: GitHub Pages configuration |
| `actions/upload-pages-artifact` ([source](https://github.com/actions/upload-pages-artifact)) | `v5` | docs.yml: static-site artifact transfer |
| `actions/deploy-pages` ([source](https://github.com/actions/deploy-pages)) | `v5` | docs.yml: GitHub Pages deployment |

## External services and assets

| Dependency / kind | Version or contract | Consumers / scope / purpose | Source / license / declaration evidence |
| --- | --- | --- | --- |
| NuGet.org v3 / package feed | `https://api.nuget.org/v3/index.json`; package-style service version not applicable | Restore for all external package consumers; CI Trusted Publishing; build/release | Workflows and package-consumer/Release scripts. Service terms not audited. Cache metadata verifies the application packages' actual acquisition feed. No credentials retained here. |
| Local Flourish/optional Essential package directories / test feeds | Per-run local feed paths and declared release package IDs/versions | scripts/Test-Release.ps1 and build/Test-BlazorPackageConsumers.ps1; isolated build/test consumption | Source mappings route Flourish candidates locally; optional EssentialPackageDirectory routes explicitly supplied Essential candidates. These are local files, not an external live service. |
| GitHub source/artifact/Actions/OIDC services | Git remote and workflow artifact/OIDC contracts; not a package version | Repository checkout, optional AGENTS synchronization, release automation | Workflow YAML and configured source URLs; no runtime Gallery GitHub API dependency was found. |
| GitHub Pages / static documentation hosting | GitHub Pages deployment contract | docs.yml deploy job; optional documentation deployment | Environment github-pages and deploy-pages action. It is not a production Gallery backend requirement. |
| Google Fonts Roboto / remotely loaded documentation font | CSS API request `family=Roboto:wght@100;400;700&display=swap`; exact delivered font build unpinned | Human documentation's DocFX Material theme only; browser font request | `docs/material/material/public/main.css` imports fonts.googleapis.com; Roboto license identity/version not present in maintained evidence and remains unknown. It is not a Flourish Gallery runtime font dependency. |
| DocFX Material / vendored documentation template | Upstream revision/version unknown | DocFX docs templates; documentation build/display | `docs/material/LICENSE` verifies MIT, copyright Oscar Vásquez; `docs/material/docs/docfx.json` identifies `https://github.com/ovasquez/docfx-material`. Human documentation remains unchanged. |
| Material Symbols Outlined / vendored variable icon font and codepoint map | Upstream commit `737e3324305806514d7909874fa1818ae1808232`; axes FILL 0–1, GRAD -50–200, opsz 20–48, wght 100–700 | P03/P11 and Gallery consumers; production icon rendering | Both icon source.json files link google/material-design-icons; retained Apache-2.0 license. Blazor ships a derived common WOFF with disjoint ranges and the unchanged complete WOFF2 fallback; WPF ships DirectWrite-derived TTF and FILL=1 Brotli geometry. SHA-256/byte provenance exists in source manifests; no Google CDN request for these icons. |
| Material Symbols `browse` SVG / vendored icon derivative | Same upstream commit, symbols/web/browse/materialsymbolsoutlined/browse_24px.svg | Framework browse.svg and Gallery logo/favicon SVGs; production assets | Per-SVG attribution comments verify Google source and Apache-2.0. Gallery logo applies the documented scale derivative. |
| Segoe UI and system fallback fonts / OS browser font resource | OS font identity; no bundled font version or license redistribution | Blazor Design `--f-font: 'Segoe UI',system-ui,sans-serif`; WPF resources use Segoe UI | Design foundation.css and WPF Themes/Resources.xaml. Source is installed OS/browser fallback; exact file/version and OS font license are unknown. |

No database, cloud storage, cache server, telemetry endpoint, authentication provider or other mandatory external business service was found in maintained production source/configuration. Gallery data and translation resources are local. `example.test`/`example.invalid` URLs and localhost test endpoints are fixtures; XML namespace/schema URLs and outbound documentation links are identifiers or navigation, not live application service dependencies.

## Active constraints and unresolved facts

- Essential.Culture 1.4.0 is the current authorized dependency. Runtime adapters and generator are external NuGet packages even though Essential is maintained by the same organization. No sibling Essential source reference is present. Modular resources are selected through Essential CultureModule generation/deployment; no additional JSON merge or filesystem discovery dependency was introduced.
- Current source is the local **1.4.4-preview.shell.20261027.15** candidate based on **aaac88c** with the retained .13/.14 refinements, after the verified public **stable 1.4.3** release, for the same six Core/Blazor package IDs and Essential.Culture 1.4.0. Current artifact evidence belongs to the explicitly labeled current-candidate section; .14 and older preview hashes remain historical. No new remote publication has occurred. Native WPF and its Culture bridge remain outside this candidate package manifest. The dated audit and public-release records do not certify consumption or verification of this local candidate.
- No dependency adoption or upgrade was performed for this documentation audit. Unknown licenses, source commits, runner/browser/OS versions and unpinned optional tools remain explicitly unknown. The scope is the resolved project/tool distributions above; it does not enumerate every component internal to an OS, browser, SDK or CI runner image.
- The human-maintained `docs/docfx.en-us.json` and `docs/docfx.zh-cn.json` currently name missing `src/Flourish.WPF/Flourish.WPF.csproj` and `src/Flourish.WPF/Assets/favicon.ico`. The actual meta project is P12. Successful documentation API/resource generation against those stale paths is unverified; ownership rules prevent silently editing the configs during this audit.
- Root `LICENSE.txt` contains MIT terms but still has `[year] [fullname]` attribution placeholders. Package MIT/Apache declarations and the retained third-party icon licenses are recorded as actual evidence; this inventory does not invent ownership attribution or repair the root license.
- Optional `build/Test-CultureIntegration.cjs` requires separately available JS Playwright and Edge/Chromium and has no maintained lock/installation manifest. This audit did not install or invoke them. Other Node regressions use built-in modules and do not need npm dependencies.
- Local Node 26.4.0 differs from CI major 24; the currently installed local SDK 10.0.400 satisfies global.json, while CI's floating 10.0.x install alone does not guarantee it chooses the same feature band required by latestPatch. No compatibility failure is claimed without an actual build evidence change.
- Licenses for cached Microsoft.Windows SDK/App SDK payloads use their actual file/URL metadata, including Microsoft vendor terms; they must not be uniformly relabeled MIT. Unknown bundled tool dependency metadata is not a reason to guess or add packages.
