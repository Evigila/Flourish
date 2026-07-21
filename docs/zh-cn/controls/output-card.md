---
title: OutputCard
description: 将操作消息追加到紧凑滚动历史中，同时避免输出内容决定同行布局的高度。
---

# OutputCard

`OutputCard` 在同一份只读历史中显示原始消息、持续进度、完成结果与失败信息。它在标准卡片间距内提供适配主题的中性视口，使用紧凑文字，并在历史超过布局高度时滚动。

> [!IMPORTANT]
> `OutputCard` 没有 `Title`、`Description`、`MainText` 或任意 `Body`。解释文案应放在所属 `Chunk` 中，操作应放在专用操作控件中，再将每个可观察结果追加为一条消息。

## 基本用法

将 `OutputCard` 放在产生消息的操作旁边。双列组合应使用同一个自动高度 `Grid` 行，让操作列建立高度，再由 `OutputCard` 拉伸到相同高度。

```xml
<flourish:Chunk
  Title="生成报告"
  Description="生成报告并查看完整的操作历史。">
  <Grid>
    <Grid.ColumnDefinitions>
      <ColumnDefinition Width="*" />
      <ColumnDefinition Width="16" />
      <ColumnDefinition Width="*" />
    </Grid.ColumnDefinitions>

    <StackPanel>
      <flourish:ListCard
        Icon="&#xE9D2;"
        Title="生成报告"
        MainText="启动一次报告生成过程。">
        <flourish:ListCard.ActionBody>
          <flourish:Button Click="GenerateReport_Click" Content="生成" />
        </flourish:ListCard.ActionBody>
      </flourish:ListCard>
      <flourish:ListCard
        Margin="{DynamicResource FlourishListCardPeerMargin}"
        Icon="&#xE74D;"
        Title="输出历史"
        MainText="移除所有已记录消息。">
        <flourish:ListCard.ActionBody>
          <flourish:Button Click="ClearOutput_Click" Content="清空" />
        </flourish:ListCard.ActionBody>
      </flourish:ListCard>
    </StackPanel>

    <flourish:OutputCard
      x:Name="ReportOutput"
      Grid.Column="2"
      AutomationProperties.Name="报告输出"
      VerticalAlignment="Stretch" />
  </Grid>
</flourish:Chunk>
```

在事件处理程序中追加消息，而不是替换现有内容：

```csharp
private void GenerateReport_Click(object sender, RoutedEventArgs e)
{
    ReportOutput.WriteLine("报告生成已开始。");
    ReportOutput.WriteLine("报告生成已完成。");
}

private void ClearOutput_Click(object sender, RoutedEventArgs e) =>
    ReportOutput.Clear();
```

## 消息历史

每次调用 `WriteLine` 都会把传入消息添加为下一行，并将视口滚动到末尾。传入 `null` 或使用不带消息的重载会添加一个空行。完成结果和失败信息与进度消息遵循相同的追加规则；不要用最新状态替换已有历史。

`Output` 以只读字符串形式返回完整历史。其他操作需要历史快照时可以读取它，但新增内容应使用 `WriteLine`。`Clear` 会移除完整历史，并让视口返回初始位置。

| 成员 | 类型 | 行为 |
| --- | --- | --- |
| `Output` | `string` | 获取完整输出历史；对应依赖属性为只读。 |
| `WriteLine(string? message)` | 方法 | 追加一条消息；`null` 表示空行，并在写入后滚动到末尾。 |
| `Clear()` | 方法 | 移除所有消息并让视口返回顶部。 |

## 高度与滚动

使用自动高度时，输出历史不会增加 `OutputCard` 的期望高度。控件的最小高度仍参与测量，拉伸父级或显式高度则决定视口的最终布局尺寸。

对于 ListCard 加输出的布局，应将完整 ListCard 列与 `OutputCard` 放在同一个自动高度 `Grid` 行中，并保持 `OutputCard.VerticalAlignment="Stretch"`。此时由 ListCard 列决定行高，新增输出留在内部视口中并使用纵向滚动条，不会迫使 ListCard 变高。不要根据 `Output` 重新计算 `Height`，也不要在 `OutputCard` 外再嵌套滚动容器。

长行不会换行，并会在需要时使用横向滚动条。视口使用紧凑滚动条，使两个方向的滚动能力不会增加过多视觉重量。

当鼠标位于超出视口范围的输出上时，只要内部视口在滚轮方向上仍可移动，它就会优先响应滚轮。到达顶部或底部边界后，继续向外滚动的输入会交给所属页面，不会被卡片内部截留。

## 视口与排版

输出视口使用更深的圆角中性灰色背景，并缩小外层间距，让消息占据更多卡片空间。输出文字使用紧凑的 Small 字号层级、独立的 `FlourishOutputFontFamily` 等宽字体（默认为 Consolas）以及随主题变化的绿色前景。需要不同输出样式时，可以在单个 `OutputCard` 上设置 `FontFamily` 或 `Foreground`。

当周围章节和操作标签仍不足以明确输出含义时，应为控件设置 `AutomationProperties.Name`。

## 相关内容

- [Card](card.md) 说明解释型和展示型信息表面。
- [Chunk](chunk.md) 说明如何在同一页面章节中组合操作与输出。
- [ScrollViewer](scroll-viewer.md) 说明底层 Flourish 滚动行为。
- [OutputCard API](xref:ArkheideSystem.Flourish.Controls.OutputCard) 列出完整成员签名。
