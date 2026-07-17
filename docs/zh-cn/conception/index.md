---
title: 理念
description: 组合 Flourish 界面时，应遵循统一的页面层级、表面语义、对齐、主题、可访问性与一致性规范。
---

# 页面与控件设计理念

Flourish 将布局与控件选择视为应用语义的一部分。在考虑颜色和装饰以前，页面就应当能够表达清晰层级：`Chunk` 标识章节，卡片组织信息，按钮表达操作。以下规则是构建应用页面时的默认设计约定。

## 使用 Chunk 建立页面层级

每个普通章节都应位于 `Chunk` 中。需要页首焦点信息时，一个页面可以用一个 `ChunkHero` 开头；否则直接从第一个 `Chunk` 开始。不要在 `Chunk` 旁放置游离标题和手工间距面板，从而建立第二套页面层级。

`Chunk` 始终占满可用内容宽度，同一行不能并排放置两个 Chunk。需要并排展示相关信息时，应使用一个全宽 Chunk，并在其 Body 中排列多张卡片。只有主题相互独立、确实需要各自章节标题时，才拆成多个全宽 Chunk。

应明确使用 `Chunk` 的三个区域：

| 区域 | 规则 |
| --- | --- |
| `ChunkTitle` | 为章节设置简洁、直接的默认标题。它是用户浏览页面结构时首先扫描的标签；优先使用名词短语或简短的操作型短语，而不是完整句子。 |
| `ChunkDescription` | 最多使用一句简短文字说明章节目的。标题已经足够明确时应省略。 |
| `ChunkBody` | 放置章节的实际信息和控件。详细说明不应写入 `ChunkDescription`。 |

```xml
<flourish:Chunk
  ChunkTitle="存储空间"
  ChunkDescription="查看用量并管理保留的文件。">
  <UniformGrid Columns="2">
    <!-- 详细信息属于 ChunkBody 中的卡片。 -->
  </UniformGrid>
</flourish:Chunk>
```

详细内容应通过语义合适的卡片家族表面及其 `Title`、`Text` 和 `Body` 表达。当信息属于连续正文、卡片表面不能带来有效分组时，可以使用纯 `TextBlock`，但它仍应位于 `ChunkBody` 内。不要把 `ChunkDescription` 写成段落，也不要用多条简介来回避主体结构。

## 每个表面只承载一个主题或行为

一张卡片应当能够作为一个完整单元被理解。章节中存在多个相互独立的行为时，应拆分为多张卡片，而不是将若干无关控件和说明塞入一个大表面。这样可以让浏览、尺寸适配、键盘导航和后续重排更加可预测。

根据语义选择表面：

| 控件 | 语义 |
| --- | --- |
| `Card` | 用于较长说明或展示型信息的非交互式表面。它的 `Body` 可以包含按钮、链接或输入控件，但卡片表面本身不调用操作。 |
| `ListCard` | 用于单个独立设置的紧凑型非交互配置行。本地控件放在 `Body` 中，配置行表面本身不调用操作。 |
| `OutputCard` | 用于原始消息、进度、完成结果与失败信息的只读追加式历史。内部视口负责滚动，历史内容不会决定表面高度。 |
| `CardButton` | 整张卡片表示一个操作。用于导航、选择或其他单一调用；不要在其中嵌套彼此独立的交互控件。 |
| `IconCard` | 将图标、图片、插图或预览作为较长说明或展示型信息组成部分的非交互式卡片。`Presenter` 提供视觉上下文，不会让卡片变为可点击控件。 |

每张普通卡片只能有一个 `Title` 和一个主题。`Text` 是普通卡片唯一的 Description 区域，所有辅助解释文字都应写在这里；不要在它的 `Body` 中再加入另一个标题或解释段落。字段标签、选项标签、列表数据和结果值属于内容本身，可以保留在 `Body` 中，因为它们不会建立第二套文案层级。

如果一张卡片需要多个主题或标题，应将其拆成多个同级表面。操作产生动态响应文字时，操作应留在自己的表面中，并在旁边放置 `OutputCard`。通过 `WriteLine` 追加原始消息、进度、完成结果与失败信息，让更早的结果保持可见。`OutputCard` 没有 `Title`、`Text` 或任意 `Body`，其上下文由操作表面和所属 `Chunk` 提供。两个表面应位于同一个全宽 Chunk 中。

每个独立配置项使用一个 `ListCard`，相关 ListCard 纵向堆叠且每一行铺满所在列。连续行之间使用紧凑的 `FlourishListCardPeerMargin`，让整列形成相关组合。同一列中不能混用 `ListCard` 与任何其他卡片类型。每个 Chunk 优先采用仅含 ListCard 的单列布局；章节还需要展示输出时，同一 Chunk 可以增加另一列放置 `OutputCard`。

`ListCard.Title` 与 `ListCard.Text` 都必须简洁。两者各自最多一行，溢出时显示省略号；不要依赖换行，应重新精简过长文案。Presenter 区域在左右两侧保留刻意加宽的水平留白，不要用局部 Margin 压缩。每个 `ListCard.Body` 必须且只能放一个交互控件；不要在一行内用面板组合多个输入或操作。

`ListCard.Body` 优先使用 `FlourishComboBox`、`FlourishCheckBox` 与 `Button`；选项确有需要时再使用 `FlourishTextBox` 与 `FlourishRadioButton`。选择、开关与编辑必须立即应用，ListCard 中绝不增加独立的 Apply 操作。

## 使用 Variant 表达语义层级

`Variant` 表达用途和强调层级，不是选择偏好颜色或修改尺寸的捷径。同一变种应在整个应用中保持相同含义。

| Card 变种 | 语义用途 |
| --- | --- |
| `Standard` | 普通分组信息的默认表面，应优先从这里开始。 |
| `Tonal` | 强调程度较低的安静辅助信息。 |
| `Filled` | 通过主要色调获得更强强调的信息。 |
| `Elevated` | 必须与复杂背景或相近颜色背景清晰分离的表面，应谨慎使用。 |

按钮变种则独立表达操作层级。一组操作通常只有一个 `Filled` 主要操作，辅助操作使用较低强调变种，破坏性操作使用 `Danger`。不要用 Card 变种暗示可点击性，也不要用按钮变种解决外部布局或尺寸问题。

`ListCard` 继承自 `Card`，但其紧凑配置语义要求始终使用 `Standard`。控件会将 `Variant` 强制转换为 `Standard`；需要 Tonal、Filled 或 Elevated 强调时，不应使用 ListCard。

## 遵循主题和颜色角色

确实需要局部覆盖时，通过 `DynamicResource` 使用 Flourish 主题资源。不要硬编码只适合一种主题的颜色。`Filled` Card 可以覆盖 `Background`，但替换方案必须在亮色和暗色主题中同时保持前景、边框、禁用状态和焦点状态清晰可读。

颜色只能强化语义，不能独自承载语义。状态颜色应同时配有文字或可访问标签。使用 `Overlay` Presenter 时，应选择在两种主题的遮罩后都保持文字可读的图片或组合视觉；需要检查图片中最亮和最暗的区域，而不只是平均颜色。

## 分离不同的对齐职责

父级 `Grid`、`StackPanel` 或其他布局容器决定控件在页面中的位置；控件的内容对齐属性负责组织内部文字与 Body。不要使用 `Variant`、Presenter 位置或重复的局部 Margin 代替父级布局。

同一行的同级卡片必须具有相同的布局高度。双列 ListCard 加 OutputCard 的组合应把完整 ListCard 列与 `OutputCard` 放入同一个自动高度 `Grid` 行。让 ListCard 列决定行高，再把 `OutputCard` 拉伸到相同高度；输出历史不会增加控件的期望高度，而是在内部视口中滚动。不要根据 `Output` 计算行高。普通同级卡片换成多行时，行间距必须与列间距一致。应优先使用共享间距资源，或者使用能够强制统一间距的布局容器，避免为各张卡片设置彼此无关的 Margin。

`Card` 的常规排列是文字区在上、`Body` 在下。`ContentVerticalAlignment="Bottom"` 会交换两者顺序；当两个内容对齐属性都设为 `Center` 时，文字区和 Body 会作为一个整体居中。

`ListCard` 使用固定布局：可选 `Presenter` 始终在左侧并保留更宽的水平留白，纵向排列的标题与简短说明位于中间，`Body` 始终在右侧。Presenter、文字区与 Body 都纵向居中；Card 的内容对齐属性不会改变这些区域的位置。

`IconCard` 在 `Split` 模式下始终使用 `PresenterPosition` 描述 Presenter 的位置，文字与 `Body` 一起位于对立侧。Presenter 位于 `Top` 或 `Bottom` 时，对立区域中的文字与 Body 水平排列；其他 Split 位置使用纵向排列。`Overlay` 模式下 Presenter 铺满表面，`PresenterPosition` 被忽略，文字与 Body 恢复为 Card 式纵向排列。

`ChunkHero` 也遵循同一套以 Presenter 为主体的描述方式：`PresenterPosition="Right"` 表示 Presenter 在右侧，而不是文字在右侧。

## 从一开始就考虑可访问性

- 保持清晰的标题层级。简洁且互不重复的 `ChunkTitle` 与 Card `Title` 有助于视觉浏览和辅助技术浏览。
- 不要只通过颜色、位置或图标传递状态与操作；应提供可见文字或等效的可访问名称。
- 当相邻标题不足以说明 ListCard Presenter 的含义时，应为其提供有意义的可访问名称，并确保 Body 的焦点顺序位于配置行文案之后。
- 当周围 Chunk 与操作标签仍不足以明确历史含义时，应为 `OutputCard` 提供可访问名称。
- 每个仅图标 `IconButton` 都应设置有意义的 `AutomationProperties.Name` 和可见工具提示；名称描述操作，而不是字形外观。
- 只有整个表面确实表示同一个操作时才使用 `CardButton`，让焦点与调用语义和视觉边界一致。
- 键盘焦点顺序应与视觉顺序和阅读顺序一致。将独立行为拆成独立卡片通常可以自然满足这一点。
- 在两种主题下验证文字对比度、焦点指示、禁用状态以及缩放或放大后的文字；避免用固定高度裁剪本地化或换行内容。

## 一致性检查清单

页面完成前，应确认：

1. 每个普通页面章节都是全宽 `Chunk`，最多只有一个位于开头的 `ChunkHero`，且不存在并排 Chunk。
2. 每个 `ChunkTitle` 都足够简洁，每个 `ChunkDescription` 都不超过一句简短的目的说明。
3. 每张普通卡片只有一个标题，所有解释文案都位于 `Text` Description 区域。
4. 独立主题或行为已拆成独立表面；变化的操作消息使用 `OutputCard`，每条原始消息、进度、完成结果与失败信息都采用追加方式，而不是替换历史。
5. 紧凑设置使用一项一个的 Standard `ListCard`；Card 与 IconCard 保留给较长文案或展示内容。
6. 每个 ListCard 都保持 Presenter 在左、Body 在右且所有区域纵向居中，保留宽松的 Presenter 留白，并且所在列不包含其他卡片类型。
7. 每个 ListCard 的 Title 与 Text 都简洁且各自只有一行，溢出使用省略号，Body 中只有一个交互控件。
8. ListCard 中的选择、开关与编辑立即应用，不存在独立 Apply 操作。
9. 同行的同级卡片具有相同布局高度；ListCard 列决定自动行高，相邻 `OutputCard` 拉伸并在内部滚动，行列间距保持一致。
10. 非交互信息、紧凑配置、操作历史、整卡操作和视觉展示场景分别使用 `Card`、`ListCard`、`OutputCard`、`CardButton` 与 `IconCard`。
11. 同一变种在不同页面中保持相同语义，且 ListCard 始终保持 Standard。
12. 主题覆盖同时适用于亮色与暗色模式，并且颜色不是语义的唯一载体。
13. 内部对齐、Presenter 位置、阅读顺序和键盘顺序彼此一致。

## 相关内容

- [Chunk](../controls/chunk.md) 说明页面章节控件及其属性。
- [Card](../controls/card.md) 说明 Card、ListCard 与 IconCard 的语义、变种、`Body` 和 Presenter 排列。
- [OutputCard](../controls/output-card.md) 说明追加式操作历史与滚动布局行为。
- [Button](../controls/button.md) 说明操作层级与专用按钮控件。
