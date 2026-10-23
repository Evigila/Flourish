# 浮层内容恒左对齐与原生选择菜单宽度修正

状态：已实现并通过构建、源代码/模拟 DOM 和宿主自动化检查；原生浏览器首帧视觉效果待手动验收。

时间：2026-10-22 17:57:10，America/Sao_Paulo（UTC−03:00），对应 2026-10-22T20:57:10Z。

## 症状与规则变更

用户明确将原来的“选项内容跟随浮层左右定位”改为**选项内容始终左对齐**；浮层相对按钮的左右对齐及上下自适应仍保留。同时报告语言菜单刚展开时宽度短暂缩小，选择标记尺寸与其他选择器不一致。

此前 .9/.10 的修订与自动化测试没有完成原生浏览器首帧验收，不能据此认定该视觉症状已经消失。本报告保留此限制，替代此前现行文档中的位置相关内容对齐规则。

## 已确认的诱因与证据边界

- Colligere 交互式语言入口使用 Flourish `LanguagePicker`，静态入口使用 `StandaloneSelectBox`；二者最终都是标准原生 `<select>`，没有宿主自制选项控件。
- 旧 Framework 选项使用 flex，选中标记 `::checkmark` 绝对定位，仅规定 48px 宽度，再通过额外左 padding 留位。标记没有标准方形选择器的高度、边框、背景和字体，造成可确认的尺寸/外观差异。
- 旧 CSS 仍通过 `width:max-content` 让原生 top-layer picker 自行完成内容估宽。初始化脚本只同步左右锚点，**没有已确认的展开后 JS 改宽代码**。因此无法仅凭源码把用户看到的首帧缩短归因于某一次特定事件、滚动条或浏览器缺陷。
- 原生选择菜单及其选中标记由浏览器管理；标准允许定制其外观，同时保留原生选择语义。参考 [HTML rendering：select](https://html.spec.whatwg.org/multipage/rendering.html#the-select-element-2) 和 [Chromium 默认样式](https://raw.githubusercontent.com/chromium/chromium/main/third_party/blink/renderer/core/html/resources/html.css)。本次修复不依赖对浏览器特定缺陷的推断。

## 实现

1. Framework 和 Design 的普通菜单、服务菜单、SplitButton、原生单选、多选、ReferenceDropdown、SearchAutocomplete 选项与完成按钮统一 `text-align:left`。移除旧 `--f-popup-align` 文字布局变量；`data-f-popup-align` 继续只决定浮层位置。普通页面 Button 的居中行为不变。
2. 原生 option 改为固定选择器列加文字列的 grid，不再绝对定位勾号或叠加左 padding。默认选择框为标准 Checkbox 的 48×48 盒模型，边框、圆角、颜色及选中勾号字体一致；未选中显示空框，选中显示勾号。选项行高容纳完整选择框，所有文字从同一列开始。多选继续使用真实 Checkbox 输入。
3. 共享 `controls.js` 在发现原生选择控件及指针/焦点/按键默认打开动作前，使用临时、不可聚焦、不可见的离屏副本测量固有内容宽度。副本保留实际选项文字、富内容子元素、非空 label 属性、分组标题，以及字体、字距、选择框列、间距、padding 和边框。测量文字轨道采用 max-content，避免继承上一轮已解析的文字列像素宽度。
4. 测得宽度向上取整后写入控件的私有 `--f-picker-inline-size`，未变化则不重复写入。Framework picker 消费该宽度并保留视口上限和零锚点最小宽度；Design 只负责外观。临时副本立即移除；重复 DOM 发现不会因为移除副本而再次测量。下一次打开重新反映文字和字体变化，已经打开的 picker 不重新测量。没有打开后动画帧校正、canvas 文字估算、公开宽度参数、第二套选择状态或自制弹窗替代。
5. Colligere 不增加 CSS 覆盖或语言按钮专用外观，继续保留原来的文化保存、值变更、键盘、禁用和表单协议。

## 本地包与实际消费

生成独立的 **Flourish `1.4.3-preview.portals.11`**，未覆盖 .10 或公开发布。Colligere 的中央版本和本地 NuGet 源切换到 `artifacts/local-nuget/1.4.3-preview.portals.11`。六个包的身份、内部依赖与必须资产通过验证。

17 个项目恢复图仍为 168 个包/版本组合，只有五个实际消费的 Flourish 版本变化，其余 163 个保持不变。五个缓存归档来源与本地 feed 相符；17 个包内资源条目与最终生产输出、消费缓存哈希一致。消费证据在 Colligere 的 `artifacts/popup-left-11/inventory.dependencies.json`，捕获 UTC20:55:28Z，SHA-256 `8148c9e0c3b151b9b07738c7827333bb8439c5a99e785df82ba0a8e053c93029`。.10 基线单独冻结，不覆盖原来的历史证据。

没有新增第三方依赖、服务、资产或维护源文件。两仓架构、依赖和 Flourish 当前控件契约已同步；审计状态不变。

## 验证

| 检查 | 实际结果 |
| --- | --- |
| Flourish Blazor 解决方案 Release | 0 警告、0 错误 |
| Framework 控件/契约 | 460/460；83 components、762 parameter rows、353 defaults |
| Culture 扩展 | 27/27 |
| Gallery | 8557；包含 474 post-event 与 367 ChangeLog 子集，勿重复相加 |
| Framework 全部 `.mjs` 自动化 | 169/169；含 native 22 和 popup-placement 25 |
| 预览版本与稳定发布隔离 | 12/12 |
| 六个本地包 | 身份/依赖/资产验证通过 |
| Colligere 17 项目 Release | 0 警告、0 错误 |
| Colligere 完整 Web | 2328 通过、135 条件 PostgreSQL 跳过、0 失败，共2463 |
| 两仓 `git diff --check` | 通过 |

最初完整 `.mjs` 运行有168通过、1失败：既有 access-form-spacing 测试的假文档缺少浏览器 defaultView。补齐该夹具后定向4/4通过，完整重跑169/169通过。另保留实现过程中的旧宽度位置断言失败；修订为 Framework 的实际几何规则后通过。生产代码没有为旧断言改回旧布局。

以上源码、CSS 级联、模拟 DOM 与 SSR 资源检查不等于原生首帧浏览器验证。本次没有使用 Computer Use，也没有实际浏览器测量帧、点击菜单或提交语言偏好。未重启 Aspire；运行实例是否加载 .11 需要用户重启后确认。未涉及数据库重建、Azure、远端推送或公开发布。

## 手工验收

1. 停止并通过项目调试入口重新构建/启动，使页面交付 .11；刷新后第一次打开右上角语言菜单，观察出现瞬间是否仍会缩短。
2. 切换不同语言再打开菜单，尤其比较最长与最短文字；文字均左对齐，选择框尺寸一致且保持同一列，宽度仅随真实内容变化。
3. 分别检查左侧与右侧按钮：浮层仍与按钮对应边缘对齐，内部内容都左对齐。检查“服务”、SplitButton、多选、引用与搜索浮层。
4. 缩窄窗口或放大字体，确认选项在视口内换行；在窗口底部打开，确认仍可向上弹出并滚动。
5. 用 Tab、方向键、Enter、Escape 验证原生选择及关闭；检查语言保存失败提示、禁用状态及浅/深主题选择框。

如首帧仍抖动，应记录具体浏览器版本、首次/重复打开区别及当前页面，再针对实际帧继续调查；本报告未将尚未观察到的视觉结果写为已通过。
