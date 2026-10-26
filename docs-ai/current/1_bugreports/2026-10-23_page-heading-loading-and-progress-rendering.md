# 页面大标题自动加载反馈与横线进度渲染修正

时间：2026-10-23 20:39:46，America/Sao_Paulo（UTC−03:00）；UTC：2026-10-23T23:39:46Z。
状态：Flourish 最终 `1.4.3-preview.portals.14` 源码、库构建、统一自动化入口和本地包验证完成；实际消费及浏览器视觉验收由宿主另行记录。完整文档审计时间不变。

## 请求与症状

PageHeading 需要在现有标题、返回、主要操作和滚动收缩能力之外，提供底部细横线反馈：宽度跟随正文容器，贴下方分割线，无 margin；其他加载控件及异步按钮自动联动，并允许仅显示标题加载。没有 PageHeading 的页面无需新增标题。

截图另显示服务器重连的标准 ProgressBar 右侧出现重复碎片，倒计时分成三行。两者分别判断，不合并为同一个已证实的浏览器缺陷。

## 诊断与证据边界

- 原 PageHeading 没有加载状态、条形结构或联动。LoadingState 内 spinner 自身 aria-hidden；只监听其可访问状态会漏掉实际加载，现由控件根发布活动标记。
- 旧标准 ProgressBar 的不确定进度块和内部高光均使用 transform，又叠加圆角裁剪。它是碎片/合成残影的可疑路径；本次去除双层 transform 作针对性修复，**尚未真实浏览器复现并确认唯一根因**。
- 倒计时分成三行有确定结构原因：Notice 是 grid，宿主直接放入前缀文字、数字 span、后缀文字，分别形成网格项。协调方将该句包在一个 p 中，恢复内部文字与数字的同行排版。该修正在 Colligere 重连组合，本报告仅解释证据，没有修改宿主源码。
- Colligere 的 ReconnectModal 在 Routes/surface 之外；最终监听包含 body/.f-root，根级弹层也可触发。多个 PageHeading 同在一个文档时可能一起响应，这是页面级保守聚合，不声称 Gallery 示例隔离。
- 自动联动观察 DOM 活动状态，不拦截任意 fetch、原生导航或不可见 Task。可排除语义 hidden、aria-hidden、inert、关闭 dialog 和未打开 popover；不声称检测任意外部样式造成的不可见状态。

## 实现

1. PageHeading 新增 `Loading`，默认 false。常驻独立条形 DOM，非活动 display:none；条绝对定位、高 4px、bottom 0、无 margin/padding，不改变标题或正文布局。使用独立动画/类，不继承标准粗条。
2. Framework CSS 通过 `:has()` 在同次样式计算中观察 `data-f-loading=true` 与 `aria-busy=true`，不等待 Observer 或异步回调。手动 Loading 仅补充 OR 信号，false 不抑制自动状态；标题不发布来源标记，避免自反馈。
3. LoadingState、未完成 ProgressBar、未停止且未完成 ProgressRing 发布活动状态。Button 异步 OnClick 自动进入 Busy、等待回调、阻止重复触发，在 finally 释放，失败也显式安排渲染。宿主无需专门操作标题。
4. body/.f-root 页面级聚合及 PageBody/shell/content/navigation 范围覆盖旁置和根级 Dialog。没有 PageHeading 的页面不会创建标题或条形结构。
5. 左右边距复用正文 `--f-content-gutter`，保留 Standard/Expanded/Fluid 算法；FullWidth 按 ContentContainer 宽度约束，560px 以下沿用 18px 内边距。Compact/滚动收缩保持；DocumentFlow 改 relative 锚定条，继续非 sticky。
6. Framework/Design 新增同名 `wwwroot/page-heading-loading.css`，分别拥有状态几何和主题颜色，支持 reduced motion/forced colors。Gallery 更新样例、参数元数据及三语言变更说明。
7. 标准 ProgressBar 仍高 40px。35% 不确定块通过逻辑 inset 在 0 至 65% 间移动；固定高光只改变 background-position，保留圆角裁剪、确定值真实宽度过渡及 reduced-motion 静态回退。

## 验证

| 检查 | 最终结果与证据 |
| --- | --- |
| 完整 Blazor solution Release | 0 警告、0 错误；`artifacts/heading-loading-build-final.log` |
| Framework 控件/契约 | 469/469；83 components、763 parameter rows、354 defaults；`artifacts/heading-loading-framework-final.log` |
| Gallery | 8,585，包含 474 post-event、379 ChangeLog 子集，不重复相加；`artifacts/heading-loading-gallery-final.log` |
| 统一维护 Node 入口 | 193/193、0 失败、0 跳过，在最终根范围扩展前通过；`artifacts/heading-loading-node-final.log` |
| 最终 body/.f-root 范围 | 定向标题检查 3/3；`artifacts/heading-loading-final-scope.log` |
| CSS bundle / SDK integration | 21 / 194；`artifacts/heading-loading-css-bundle.log`、`artifacts/heading-loading-css-assets.log` |
| 预览身份/稳定发布隔离、ChangeLog | 12 / 148，协调方完成检查 |
| 六个 .14 本地包 | pack 并通过 Verify-PackageSet 身份/内部依赖/必需资产验证；归档哈希见依赖清单 |
| 完整维护树 | 实际枚举并逐路径核对：701 文件、121 目录，22 项目、5 solution；新增下列 4 文件 |

新 Node 回归经现有入口纳入：`palette-checks.mjs` 导入 `progress-rendering.test.mjs` 和 `page-body-actions.test.mjs`，后者导入 `page-heading-loading.test.mjs`。无需重复登记相同夹具。

初跑的新 helper 命名、旧 position:static 断言及 PowerShell 5 编码问题均修正后重跑，上表明确区分完整入口与最终范围扩展后的定向复核，不是遗留失败，也不等于物理视觉验证。中间 .13 已产出后才发现根级重连边界，最终 .14 加入 body/.f-root 聚合并独立产包，保留 .13 证据而不覆盖缓存。

新增维护路径：

- `src/Flourish.Blazor/Flourish.Blazor.Framework/wwwroot/page-heading-loading.css`
- `src/Flourish.Blazor/Flourish.Blazor.Design/wwwroot/page-heading-loading.css`
- `tests/Tests.Flourish.Blazor/page-heading-loading.test.mjs`
- `tests/Tests.Flourish.Blazor/progress-rendering.test.mjs`

协调方另已验证 Colligere 的 17 项目 Release（0 警告、0 错误）及完整 Web（2,445 通过、153 条件跳过、0 失败）；其 `artifacts/heading-loading-20261023/inventory.dependencies.json` 保留 17 恢复图、168 包/版本、仅五个 Flourish 版本变化及其余 163 项不变、五个缓存归档匹配 feed、五项资源（含两份 CSS bundle）匹配生产输出的独立证据。此时的运行实例更新由协调方继续处理，本报告不据此声称实际运行页面已加载新版。

没有新增外部依赖，没有公开 NuGet 发布、远端 tag 或 Git 提交。未使用 Computer Use。源码、CSS 断言、SSR 和模拟 DOM 不证明物理浏览器已无残影；宿主恢复、运行版本和用户回归以宿主报告为准。

## 手动验收

1. 打开数据页面并延迟请求：其他加载提示出现时标题细条同时或提前出现，全部加载结束后消失。直接 Loading 能单独显示，关闭它不遮蔽仍活动的其他加载。
2. 触发异步 Button：无需绑定标题状态即联动，处理中无法重复操作；成功和异常后均释放 Busy，可以再次触发。
3. 检查 Standard、Expanded、Fluid、FullWidth、窄屏和缩放：条对齐正文容器且贴分割线，标题折叠无跳动；DocumentFlow 标题随文档滚动。
4. 对比隐藏、inert、关闭 Dialog/Popover 与打开的旁置/根级 Dialog：不活动来源不触发；真实弹层加载能联动标题。无 PageHeading 的展示页保持原结构。
5. 重现服务器重连，在浅/深主题、缩放、不同浏览器及 reduced-motion 下观察标准 ProgressBar：应只有一个完整移动块，无碎片/裁剪残影。仍有问题时保留浏览器版本、缩放及重现条件继续定位。
6. 确认倒计时前缀、数字、后缀在三语言下同行并自然换行，数字更新不破坏内容。

## 2026-10-23 20:44:53 最终接入与复核补证

时间：America/Sao_Paulo（UTC−03:00），UTC：2026-10-23T23:44:53Z。本节更新前述阶段性验证边界，保留早先记录，不修改既有 append-only changelog。

- 最终 body/.f-root 源码上重新执行统一 Node 入口：**193/193**，0 失败、0 跳过，证据 `artifacts/heading-loading-node-final.log`；此前扩展后定向 3/3 仍保留。Gallery .14 最终复核 **8,585**，含 474 post-event 和 379 ChangeLog 子集，证据 `artifacts/heading-loading-gallery-final.log`。
- Colligere 已通过 `debug-handler.bat` 重启并运行 .14，八个端口的 ready 探针均为 **HTTP 200**。实际 HTTP 返回的两份 CSS 与最终 producer 和包内 bundle SHA-256 一致：Framework `9b84a7f13107475e24ab1f5bb9a28f20be9321c5d7d979516d59a9063ade5cbd`；Design `3b34115d4e33ef8d59706f58342fd1057c30afea3d3019b1b3af291c3fcc6a97`。
- 完整消费证据位于 Colligere 的 `artifacts/heading-loading-20261023/runtime-resources.json` 与 `inventory.dependencies.json`，实际接入和回归详见[宿主问题报告](../../../../Colligere/docs-ai/current/1_bugreports/2026-10-23_page-heading-loading-and-progress-rendering.md)。实际交付验证不等于物理浏览器的动画、无残影或加载首帧优先级验收；这些仍按上方手动清单确认。

本次补证仅更新架构、依赖和本报告，完整维护树仍为 701 文件/121 目录，审计状态和已有变更记录不变。
