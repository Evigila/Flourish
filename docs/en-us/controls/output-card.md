---
title: OutputCard
description: Append operation messages to a compact scrolling history that does not determine a peer layout's height.
---

# OutputCard

`OutputCard` displays raw messages, ongoing progress, completed results, and failures in one read-only history. It owns a themed neutral viewport inside the standard card spacing, uses compact text, and scrolls when the history exceeds the arranged height.

> [!IMPORTANT]
> `OutputCard` has no `Title`, `Text`, or arbitrary `Body`. Keep explanatory copy and actions in the containing `Chunk`, `Card`, or `ListCard`, then append each observable outcome as a message.

## Basic usage

Place `OutputCard` beside the actions that produce its messages. In a two-column composition, use one auto-sized `Grid` row so the action column establishes the height and `OutputCard` stretches to match it.

```xml
<flourish:Chunk
  ChunkTitle="Report generation"
  ChunkDescription="Generate a report and review the complete operation history.">
  <Grid>
    <Grid.ColumnDefinitions>
      <ColumnDefinition Width="*" />
      <ColumnDefinition Width="16" />
      <ColumnDefinition Width="*" />
    </Grid.ColumnDefinitions>

    <StackPanel>
      <flourish:ListCard
        Presenter="&#xE9D2;"
        Title="Generate report"
        Text="Start a report generation pass.">
        <flourish:ListCard.Body>
          <flourish:Button Click="GenerateReport_Click" Content="Generate" />
        </flourish:ListCard.Body>
      </flourish:ListCard>
      <flourish:ListCard
        Margin="{DynamicResource FlourishListCardPeerMargin}"
        Presenter="&#xE74D;"
        Title="Output history"
        Text="Remove all recorded messages.">
        <flourish:ListCard.Body>
          <flourish:Button Click="ClearOutput_Click" Content="Clear" />
        </flourish:ListCard.Body>
      </flourish:ListCard>
    </StackPanel>

    <flourish:OutputCard
      x:Name="ReportOutput"
      Grid.Column="2"
      AutomationProperties.Name="Report output"
      VerticalAlignment="Stretch" />
  </Grid>
</flourish:Chunk>
```

Append rather than replace messages in the event handler:

```csharp
private void GenerateReport_Click(object sender, RoutedEventArgs e)
{
    ReportOutput.WriteLine("Report generation started.");
    ReportOutput.WriteLine("Report generation completed.");
}

private void ClearOutput_Click(object sender, RoutedEventArgs e) =>
    ReportOutput.Clear();
```

## Message history

Every `WriteLine` call adds the supplied message as the next line and scrolls the viewport to the end. Passing `null`, or using the overload without a message, adds an empty line. Completed results and failures follow the same append-only rule as progress messages; do not replace the existing history with the latest state.

`Output` returns the complete history as a read-only string. Read it when another operation needs a snapshot, but use `WriteLine` to add content. `Clear` removes the complete history and returns the viewport to its initial position.

| Member | Type | Behavior |
| --- | --- | --- |
| `Output` | `string` | Gets the complete output history. The dependency property is read-only. |
| `WriteLine(string? message)` | Method | Appends one message, or an empty line for `null`, and scrolls to the end. |
| `Clear()` | Method | Removes every message and returns the viewport to the top. |

## Height and scrolling

With automatic height, the output history does not increase `OutputCard`'s desired height. Its minimum height still participates in measurement, while a stretching parent or an explicit height determines the arranged viewport size.

For a ListCard-plus-output layout, place the complete ListCard column and `OutputCard` in the same auto-sized `Grid` row and leave `OutputCard.VerticalAlignment` as `Stretch`. The ListCard column then determines the row height. Additional output remains inside the viewport and uses its vertical scrollbar instead of making the ListCards taller. Do not recalculate `Height` from `Output` or wrap `OutputCard` in another scrolling container.

Long lines do not wrap and use the horizontal scrollbar when needed. The viewport uses compact scrollbars so both axes remain available without adding substantial visual weight.

## Viewport and typography

The output viewport uses a rounded neutral background to distinguish messages from ordinary card content. It sits inside the outer card spacing rather than replacing that spacing, and its text uses the compact Small typography tier to minimize the control's required height.

Give the control an `AutomationProperties.Name` when the surrounding section and action labels do not already identify the output clearly.

## Related content

- [Card](card.md) covers explanatory and display-oriented information surfaces.
- [Chunk](chunk.md) explains how to group actions and their output in one page section.
- [ScrollViewer](scroll-viewer.md) describes the underlying Flourish scrolling behavior.
- The [OutputCard API](xref:ArkheideSystem.Flourish.Controls.OutputCard) lists the complete member signatures.
