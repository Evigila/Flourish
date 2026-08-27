---
title: OutputCard
description: 将操作消息追加到紧凑滚动历史中，同时避免输出内容决定同行布局的高度。
---

# OutputCard

`OutputCard` 在可滚动的只读历史中显示消息、进度、结果与失败信息。

> [!IMPORTANT]
> `OutputCard` 没有 `Title`、`Content`、`Icon` 或任意 `Body`。解释文案应放在所属 `Chunk` 中，操作应放在专用操作控件中，再将每个可观察结果追加为一条消息。

## 基本用法

将 `OutputCard` 放在产生消息的操作旁边。双列组合应使用同一个自动高度 `Grid` 行，让操作列建立高度，再由 `OutputCard` 拉伸到相同高度。

```xml
<flourish:Chunk
  Title="生成报告"
  Content="生成报告并查看完整的操作历史。">
  <Grid>
    <Grid.ColumnDefinitions>
      <ColumnDefinition Width="*" />
      <ColumnDefinition Width="16" />
      <ColumnDefinition Width="*" />
    </Grid.ColumnDefinitions>

    <StackPanel>
      <flourish:ActionCard
        Icon="&#xE9D2;"
        Title="生成报告"
        Content="启动一次报告生成过程。">
        <flourish:ActionCard.Body>
          <flourish:Button Click="GenerateReport_Click" Content="生成" />
        </flourish:ActionCard.Body>
      </flourish:ActionCard>
      <flourish:ActionCard
        Margin="{DynamicResource FlourishActionCardPeerMargin}"
        Icon="&#xE74D;"
        Title="输出历史"
        Content="移除所有已记录消息。">
        <flourish:ActionCard.Body>
          <flourish:Button Click="ClearOutput_Click" Content="清空" />
        </flourish:ActionCard.Body>
      </flourish:ActionCard>
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

ActionCard 与 `OutputCard` 应位于同一自动高度 `Grid` 行，并设置 `OutputCard.VerticalAlignment="Stretch"`。ActionCard 决定行高，新增输出在内部滚动，不会撑高同级内容。不要根据 `Output` 计算 `Height`，也不要外套滚动容器。

长行不会换行，并会在需要时使用横向滚动条。视口使用标准 Flourish 滚动条；其较窄的可见滑块使两个方向的滚动能力不会增加过多视觉重量。

当鼠标位于超出视口范围的输出上时，只要内部视口在滚轮方向上仍可移动，它就会优先响应滚轮。到达顶部或底部边界后，继续向外滚动的输入会交给所属页面，不会被卡片内部截留。

## 视口与排版

输出视口使用无外层 Padding 的深色圆角中性背景。文字使用 Small、`FlourishOutputFontFamily` 等宽字体（默认 Consolas）和主题绿色前景；可在单个 `OutputCard` 上覆盖 `FontFamily` 或 `Foreground`。

当周围章节和操作标签仍不足以明确输出含义时，应为控件设置 `AutomationProperties.Name`。

## 相关内容

- [Card](card.md)
- [Chunk](chunk.md)
- [ScrollViewer](scroll-viewer.md)
- [OutputCard API](xref:ArkheideSystem.Flourish.Controls.OutputCard)
