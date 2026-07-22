---
title: 设计理念
description: 统一应用 Flourish 的页面层级、字体、间距、展示、交互、主题和可访问性规范。
---

# 页面与控件设计理念

Flourish 将布局和控件选择视为应用语义的一部分。页面应在装饰之前表达清晰层级：区块定义主题，内容控件呈现信息，按钮表达操作。以下规则是主导航内容页面的默认设计约定。

## 建立唯一的页面层级

每个主导航内容页面都以且仅以一个 `ChunkHero` 开头，随后放置一个或多个 `Chunk`。`ChunkHero`、`Chunk` 和 `Presenter` 都是全宽控件，各自独占一行。

所有页面内容都应位于这个头部或这些区块中。不要用游离标题、卡片或手工间距面板建立另一套层级；不要添加第二个头部区块，也不要并排放置 Chunk。

Shell 自有的瞬时表面（包括 Profile 浮出层、Popup 和 Dialog）不是主导航内容页面。不要强行在其中加入超大 `ChunkHero` 或完整页面骨架；其内部内容仍需遵循本文的字体、间距、内容控件选择、Button 家族、主题和可访问性规范。

应明确使用 `Chunk` 的三个区域：

| 区域 | 规则 |
| --- | --- |
| `Title` | 必需。用简洁标题说明区块主题。 |
| `Description` | 可选。只补充优秀标题仍无法覆盖的关键信息。 |
| `Body` | 必需。在这里放置实际内容控件或布局树；`Chunk` 自身不负责呈现内容。 |

`ChunkHero` 继承 [Presenter](../controls/presenter.md) 的字段。每个头部声明都必须显式提供必需的 `Title`、`Description`、`PresenterMode` 和 `PresenterPosition`。`Body` 在文案区域辅助表达信息，`Presentation` 则提供展示视觉，而不会创建另一套区块层级。

## 按角色使用字号

Flourish 有六种字号层级。控件或文本元素没有显式选择层级时，使用 `Standard`。

| 层级 | 预期角色 |
| --- | --- |
| `Small` | 紧凑的辅助文本，包括导航栏分组标签和 `OutputCard` 输出。 |
| `Standard` | 默认正文和控件文字。没有专门角色时始终使用它。 |
| `Icon` | 通用图标字形。专用图标控件可根据自身几何要求设置局部字号。 |
| `Large` | 卡片标题级强调。 |
| `ExtraLarge` | 区块标题一族，包括 `Chunk.Title`。 |
| `HeaderSize` | 仅用于 `ChunkHero` 中的页面标题。 |

不要仅为了让内容更醒目而选择更大的字号；应通过正确控件和文本角色表达层级。

## 按用途选择内容控件

| 需求 | 控件 |
| --- | --- |
| 一个标题和一段正文 | `Card` |
| 一个标题、一段正文和一个图标 | `IconCard` |
| 多个文本段落 | 作为区块唯一主体的 `Paragraph` |
| 图片、多个图标、预览或组合展示内容 | `Presenter` |
| 一个带右侧局部控件的紧凑设置 | `ListCard` |
| 原始输出、日志、进度、结果或错误 | `OutputCard` |
| 整个表面均可点击的一项操作 | `CardButton` |

一个表面只应表达一个主题或行为。应将无关主题拆分为同级控件，不要构建大型嵌套表面。

## 统一应用间距与折叠

布局区域之间通常都有明确间距。应使用布局控件提供的资源和默认值，不要累积彼此无关的局部边距。

- 每两个 Chunk 之间，以及 `ChunkHero` 与第一个 `Chunk` 之间，都保留标准大间距。
- 相关 ListCard 之间使用更紧凑的 `FlourishListCardPeerMargin`，使它们形成一个整体。
- 多个 Presenter 组成纵向堆叠时，从第二个开始使用 `FlourishPresenterPeerMargin`，不要复用 Card 间距。
- 让 `Paragraph` 在每两个段落之间创建间距。
- 可选区域为空字符串或 `null` 时，其呈现器及相关间距必须完全折叠。
- 卡片换行形成网格时，行间距与列间距保持一致。

## 让卡片专注于呈现

`Card`、`IconCard` 和 `ListCard` 具有可选的 `Title` 和 `MainText` 字段，但没有通用 `Body`。它们是内容呈现的终端表面，而不是任意控件的容器。

普通 `Card` 呈现一段正文。`IconCard` 增加一个图标，并允许 `Left`、`Top`、`Right` 或 `Bottom` 图标位置；它不接受图片、图标组、叠加层或其他组合展示内容。空间允许时，Card 和 IconCard 可以排列为两列或更多列。

`ListCard` 固定采用左侧图标、中间文案、右侧操作的排列。`Title` 和 `MainText` 都是单行并在溢出时显示省略号，因此应保持简短。`ActionBody` 中只放一个局部交互控件，保留 `Standard` 变体，并让选择、开关和编辑立即生效。不要添加单独的“应用”操作。ListCard 应独占一列纵向堆叠，并使用紧凑的同级间距。

`OutputCard` 没有标题、描述或任意主体。使用 `WriteLine` 追加每条消息，不要替换较早的进度或结果。它的紧凑可滚动输出视口使用 `Small` 字号层级，并且不应决定相邻 ListCard 列的高度。

## 使用 Paragraph 呈现连续正文

`Paragraph` 使用透明背景和带圆角且细而低对比度的边框，并且没有标题、描述或变体。每个直接子 `TextBlock` 都是一个段落。控件会在段落之间提供间距、为每段添加等同于四个标准空格的开头视觉缩进，并通过小顶部外边距将表面与 Chunk 文案分开。其固定呈现字号使用 Large 层级。不要添加字面空格、逐段边距、覆盖控件外边距或相冲突的子项字号。

应将 `Paragraph` 作为 Chunk 的唯一主体。内容只有一段时使用 Card；内容需要控件或视觉组合时，改用合适的卡片或 Presenter。

需要呈现可精确复制的源代码或命令文本时使用 `CodeSpace`。它与 Paragraph 共用圆角轻量描边表面、外部间隔和 Large 字号，但将单一 `Text` 值固定呈现为 Normal 字形、Bold 字重、蓝色 Consolas，并自带右上角复制操作。复制提示使用共享 Tip 字体规范，不继承代码字重。具备词法感知的语法高亮会在后续补充。

## 使用 Presenter 呈现丰富内容

`Presenter` 分离三项职责：必需的 `Title` 与 `Description` 提供文案，`Body` 在文案区域承载辅助控件，`Presentation` 承载图片、图标组、插图或组合视觉内容。每个声明还必须显式写出 `PresenterMode` 和 `PresenterPosition`。运行时回退值仍是 `Split` 和 `Right`，但不能代替作者显式声明。

标准 `Split` 使用固定的横向组合：文案和 `Body` 始终位于左侧，`Presentation` 位于右侧。显式使用 `PresenterPosition="Left"` 时只交换两侧区域，不改变各区域的内部结构。`Overlay` 使用 `Presentation` 填满 Presenter，并将文案和 `Body` 呈现在上方；此时位置设置会被忽略，但仍需声明。使用者不应另建 Grid 或依赖局部 Margin 手工移动这些区域。

`Presentation` 是默认 XAML 内容属性。请通过显式的 `Presenter.Body` 属性元素为 `Body` 赋值，避免直接内容从展示侧悄然进入文案列。普通 Presenter 是全宽控件：文案与 Body 一侧保持透明，Title、Description 和 Body 共用左侧对齐线；只有展示区域使用随主题变化的浅层中性灰圆角表面，Split 展示内容默认在其中居中。

`ChunkHero` 是页面级 Presenter 特化。它使用相同字段和模式，但增加强调头部背景、HeaderSize 标题和页面开头语义。

## 按操作类型使用按钮家族

每个按钮的完整视觉边界都可交互。

| 控件 | 规则 |
| --- | --- |
| `Button` | 默认的无图标文本操作。 |
| `IconButton` | 带图标的操作；仅图标按钮可以省略内容。 |
| `CardButton` | 使用类似 IconCard 呈现的整卡操作。 |
| `WindowCaptionButton` | 仅用于窗口标题栏操作。 |

不要给非交互卡片添加指针处理器来模拟按钮。每个仅图标按钮都应有可见工具提示和有意义的 `AutomationProperties.Name`。

按钮变体表达操作层级：一组操作通常只有一个 `Filled` 主操作，辅助操作使用较低强调变体，破坏性操作使用 `Danger`。卡片变体独立表达表面强调，绝不暗示可点击性。

## 保持主题与可访问性

需要局部覆盖时，通过 `DynamicResource` 使用 Flourish 主题资源。避免只在一种主题下有效的颜色，也不要让颜色、位置或图标成为语义的唯一载体。使用 Overlay 展示时，应在两种主题下分别检查视觉内容中最亮和最暗区域上的文本。

保持键盘焦点顺序、阅读顺序和视觉顺序一致。避免使用会裁切本地化文字或放大文字的固定高度。当周围 Chunk 和操作标签仍不足以标识历史时，为 `OutputCard` 提供可访问名称。

## 一致性检查清单

页面完成前，应确认：

1. 页面以且仅以一个 `ChunkHero` 开头，随后是全宽 Chunk，且它们之外没有同级内容。
2. 每个 `Chunk` 都有简洁 `Title` 和实际 `Body`；描述只在必要时出现，空可选区域不留下间距。
3. 每个 `ChunkHero` 和 `Presenter` 都显式声明 `Title`、`Description`、`PresenterMode` 和 `PresenterPosition`；标准 Split 将文案和 Body 固定在左侧，将 Presentation 放在右侧。
4. 未显式指定的文本使用 `Standard`，专用字号层级遵循各自角色。
5. 单段正文使用 Card，多段正文使用 Paragraph，可精确复制的代码使用 CodeSpace，单图标使用 IconCard，图片或组合视觉内容使用 Presenter。
6. 卡片没有任意 Body；ListCard 只使用一个 `ActionBody` 控件并立即应用变更。
7. Chunk 间距较大，ListCard 同级间距紧凑，Paragraph 自己负责外部间隔、段落间距和缩进。
8. 完整交互表面使用按钮家族中的正确成员。
9. 变体、主题资源、对比度、可访问名称和焦点顺序在语义上保持一致。

## 相关内容

- [Chunk](../controls/chunk.md) 说明页面层级与头部规则。
- [Card](../controls/card.md) 说明 Card、IconCard 和 ListCard。
- [Paragraph](../controls/paragraph.md) 说明多段文本布局。
- [CodeSpace](../controls/code-space.md) 说明精确代码文本与内置复制操作。
- [Presenter](../controls/presenter.md) 说明 Split、Overlay 和展示内容。
- [OutputCard](../controls/output-card.md) 说明紧凑可滚动输出。
- [Button](../controls/button.md) 说明操作控件。
- [字体](../articles/configure-font.md) 说明六种字号层级和全局配置。
