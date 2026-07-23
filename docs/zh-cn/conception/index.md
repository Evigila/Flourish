---
title: 设计理念
description: 统一应用 Flourish 的页面层级、字体、间距、呈现、交互、主题和可访问性规范。
---

# 页面与控件设计理念

Flourish 将布局和控件选择视为应用语义的一部分。页面应先表达清晰层级：PageBody 建立根结构，区块定义主题，内容控件呈现信息，按钮表达操作。

## 建立严格的页面层级

每个主导航内容页面都使用 `PageBody` 作为根容器。其直接子项只能是 `HeaderChunk` 与 `Chunk`：标准页面以一个 HeaderChunk 开头，随后放置一个或多个 Chunk。HeaderChunk 最多出现一次且必须位于最前方，两种区块都全宽独占一行。Split 与 Overlay Presenter 同样全宽独占一行；只有 TopDown Presenter 可以在保持可读的前提下分列。

所有实际页面内容都应位于这些区块中。不要把游离标题、卡片、Presenter、Document 或手工间距面板作为 PageBody 的直接子项，也不要并排放置 Chunk。

Shell 自有的瞬时表面（包括 Profile、Popup 和 Dialog）不是主导航内容页面，不需要加入完整 PageBody 或 HeaderChunk。它们仍需遵循字体、间距、控件选择、按钮和可访问性规范。

`Chunk` 的三个区域具有明确职责：

| 区域 | 规则 |
| --- | --- |
| `Title` | 必需。用简洁标题说明区块主题。 |
| `Content` | 可选。只补充标题仍无法覆盖的关键信息。 |
| `Body` | 必需。放置实际内容控件或布局树；Chunk 自身不负责内容呈现。 |

`HeaderChunk` 是页面级 Presenter 特化。每个声明都应显式提供 `Title`、`Content`、`PresenterMode` 和 `PresenterPosition`；Body 与文案处于同一区域，Presentation 提供展示视觉。

## 按角色使用字号

Flourish 有六种字号层级。控件或文本元素没有显式选择层级时，使用 `Standard`。

| 层级 | 预期角色 |
| --- | --- |
| `Small` | 紧凑辅助文本，包括导航栏分组标签和 OutputCard 输出。 |
| `Standard` | 默认正文和控件文字。没有专门角色时始终使用它。 |
| `Icon` | 通用图标字形；专用图标控件可按自身几何进行局部校正。 |
| `Large` | 卡片标题与 Document 中的 Paragraph 正文。 |
| `ExtraLarge` | 区块标题一族，包括 `Chunk.Title`。 |
| `HeaderSize` | 仅用于 HeaderChunk 的页面标题。 |

不要只为制造视觉强调而选择更大字号；应通过正确控件和文本角色表达层级。

## 按用途选择内容控件

| 需求 | 控件 |
| --- | --- |
| 标题、单段正文或单个图标的组合 | `Card` |
| 多个连续文本段落 | 作为 Chunk 唯一 Body 的 `Document`，内部包含多个 `Paragraph` |
| 图片、多个图标、预览或组合展示内容 | `Presenter` |
| 一个带局部交互控件的固定结构 | `ActionCard` |
| 原始输出、日志、进度、结果或错误 | `OutputCard` |
| 整个卡片都执行同一操作 | `CardButton` |

一个表面只表达一个主题或行为。应将无关主题拆分为同级控件，不要构建职责混杂的大型嵌套表面。

## 统一间距与折叠

布局区域之间通常都有明确间距。应使用控件提供的资源和默认值，不要累积彼此无关的局部 Margin。

- HeaderChunk 与第一个 Chunk 之间、每两个 Chunk 之间都保留标准大间距。
- 相关 ActionCard 使用更紧凑的 `FlourishActionCardPeerMargin`，使它们形成一个整体。
- 多个 Presenter 纵向堆叠时，从第二个开始使用 `FlourishPresenterPeerMargin`。
- Document 统一提供与 Chunk 文案的外部间距及各 Paragraph 之间的段落间距。
- 可选区域为空字符串或 `null` 时，其呈现区域及相关间距完全折叠。
- 卡片组成网格时，行间距与列间距保持一致。

## 让卡片专注于呈现与局部操作

`Card` 具有可选的 `Title`、`Content` 和 `Icon`。它只呈现单段正文与单个图标，不具有 Body，也不接受图片、图标组或任意子控件。`IconPosition` 可将图标放在文案的 Left、Top、Right 或 Bottom。空间允许时，Card 可以排列为两列或更多列。

`ActionCard` 在相同的可选信息区域之外提供一个 `Body`，其中只放一个按钮、下拉框、选择框、输入框或同类局部交互控件。`Horizontal` 是默认结构：Icon 在左，Title 与 Content 在中间纵向排列，Body 在右，整体垂直居中。`Vertical` 将四个区域从上到下排列并统一靠左，适合浮窗卡片。相关 ActionCard 应使用紧凑同级间距。

当只有 Body 中的一小部分需要交互时使用 ActionCard；当整个卡片都执行同一操作时使用 `CardButton`。CardButton 支持 Card 的 Standard、Elevated、Tonal 与 Filled 视觉变体，并让完整表面参与点击、命令、键盘与自动化行为。

`OutputCard` 没有标题、正文、图标或任意 Body。使用 `WriteLine` 追加每条消息，不要用最新状态替换已有历史。其紧凑可滚动视口使用 Small 字号，并且不应决定相邻 ActionCard 列的高度。

## 使用 Document 呈现连续正文

`Document` 使用透明背景、圆角和细而低对比度的边框，没有标题、Content 或变体。它只接受 `Paragraph` 直接子项；每个 Paragraph 是经过规范化的文本块，始终使用 Large 字号并默认使用 Regular 字重。

Document 会在段落之间提供统一间距，并为每个非空段落的首行添加四个普通空格。不要手工添加前导空格、逐段 Margin 或相冲突的字号。正文只有一段时使用 Card；内容需要交互或视觉组合时使用 ActionCard 或 Presenter。

需要呈现可精确复制的源代码或命令文本时使用 `CodeSpace`。它与 Document 共用圆角轻量描边表面、外部间隔和 Large 字号，但把单一 `Text` 固定呈现为 Normal 字形、Bold 字重、蓝色 Consolas，并提供右上角复制按钮。

## 使用 Presenter 呈现丰富内容

`Presenter` 分离三项职责：`Title` 与 `Content` 提供文案，`Body` 在文案区域承载辅助控件，`Presentation` 承载图片、图标组、插图或组合视觉内容。每个声明都应显式设置 `PresenterMode` 与 `PresenterPosition`。

- `Split` 横向分隔两侧。默认 Presentation 在左、文案与 Body 在右；`PresenterPosition="Right"` 交换两侧。
- `TopDown` 把 Presentation 放在上方，把 Title、Content 与 Body 放在下方并统一靠左，也是普通 Presenter 唯一允许分列的模式。
- `Overlay` 让 Presentation 填满控件，并把文案与 Body 呈现在其上方。

Presentation 是默认 XAML 内容属性；Body 应通过显式的 `Presenter.Body` 属性元素赋值。Split 与 Overlay Presenter 全宽独占一行，TopDown Presenter 可按需分列；文案与 Body 一侧保持透明并共用左侧对齐线，Presentation 区域使用浅灰色圆角背景、填满其分配空间，并让展示内容居中。HeaderChunk 即使使用 TopDown 也始终独占一行。

HeaderChunk 使用相同字段和三种模式，但增加强调背景、HeaderSize 标题和页面开头语义。

## 按操作类型使用按钮

| 控件 | 规则 |
| --- | --- |
| `Button` | 常规按钮；可同时或分别提供可选 Icon 与 Content，仅图标操作也使用它。 |
| `CardButton` | 具有 Card 视觉变体且完整表面可交互的操作。 |
| `WindowCaptionButton` | 仅用于窗口标题栏操作。 |

Button 的 Icon 或 Content 为空时，对应区域及间距完全折叠。不要给非交互 Card 添加指针处理器来模拟按钮。仅图标按钮应有可见 ToolTip 和有意义的 `AutomationProperties.Name`。

按钮变体表达操作层级：一组操作通常只有一个 Filled 主操作，辅助操作使用较低强调变体，破坏性操作使用 Danger。卡片变体表达表面强调；它本身不暗示可点击性。

## 协调滚动与浮层

DataGrid 在内部仍可滚动时优先响应鼠标滚轮；到达顶部或底部边界后，把继续向外滚动的输入交给 PageBody。不要编写额外的 PreviewMouseWheel 转发逻辑。

Overlay 通常承载 `ActionCard Variant="Vertical"` 作为标准浮窗视图，也允许 Profile 等场景使用自定义布局。Temporary Overlay 在指针离开触发器与表面后请求关闭；Strong Overlay 由宿主通过明确操作、<kbd>Esc</kbd> 或外部点击关闭。

## 保持主题与可访问性

需要局部覆盖时，通过 `DynamicResource` 使用 Flourish 主题资源。避免只在一种主题下有效的颜色，也不要让颜色、位置或图标成为语义的唯一载体。Overlay Presenter 中的叠加文案应在浅色和深色主题下都保持可读。

保持键盘焦点顺序、阅读顺序和视觉顺序一致。避免使用会裁切本地化文字或放大文字的固定高度。当周围 Chunk 和操作标签仍不足以标识历史时，为 OutputCard 提供可访问名称。

## 一致性检查清单

页面完成前，应确认：

1. 页面使用 PageBody 作为根容器，直接子项只有一个开头 HeaderChunk 和后续全宽 Chunk。
2. 每个 Chunk 都有简洁 Title 和实际 Body；Content 只在必要时出现，空可选区域不留间距。
3. 每个 HeaderChunk 和 Presenter 都显式声明 Title、Content、PresenterMode 与 PresenterPosition，并使用 Split、TopDown 或 Overlay 的标准结构。
4. 未显式指定的文本使用 Standard；Document Paragraph 使用 Large，专用字号遵循各自角色。
5. 单段正文或单图标使用 Card，多段正文使用 Document，图片或组合视觉使用 Presenter。
6. Card 没有 Body；ActionCard 的 Body 只放一个局部交互控件，Horizontal 与 Vertical 结构选择正确。
7. 完整交互表面使用 CardButton，普通操作使用支持可选 Icon 的 Button。
8. Chunk 间距较大，ActionCard 同级间距紧凑，Document 自己负责外部间隔、段落间距和缩进。
9. DataGrid 到达滚动边界后页面仍能继续滚动；Overlay 使用合适的内容结构和关闭语义。
10. 变体、主题资源、对比度、可访问名称和焦点顺序在语义上保持一致。

## 相关内容

- [PageBody](../controls/page-body.md)说明严格页面根容器。
- [Chunk](../controls/chunk.md)说明 HeaderChunk 与 Chunk 页面层级。
- [Card](../controls/card.md)说明 Card 与 ActionCard。
- [Document](../controls/document.md)说明多段文本布局。
- [CodeSpace](../controls/code-space.md)说明精确代码文本与内置复制操作。
- [Presenter](../controls/presenter.md)说明 Split、TopDown 与 Overlay 展示布局。
- [OutputCard](../controls/output-card.md)说明紧凑可滚动输出。
- [Button](../controls/button.md)说明操作控件。
- [排版](../articles/configure-font.md)说明六种字号层级和全局配置。
