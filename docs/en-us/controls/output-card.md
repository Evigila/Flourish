---
title: OutputCard
description: Append messages to a compact scrolling history.
---

# OutputCard

`OutputCard` is an append-only, read-only history for progress, results, and failures.

> [!IMPORTANT]
> `OutputCard` has no `Title`, `Content`, `Icon`, or arbitrary `Body`. Keep explanatory copy in the containing `Chunk` and actions in a purpose-built action control, then append each observable outcome as a message.

## Basic usage

Place `OutputCard` beside the actions that produce its messages. In a two-column composition, use one auto-sized `Grid` row so the action column establishes the height and `OutputCard` stretches to match it.

```xml
<flourish:Chunk
  Title="Report generation"
  Content="Generate a report and review the complete operation history.">
  <Grid>
    <Grid.ColumnDefinitions>
      <ColumnDefinition Width="*" />
      <ColumnDefinition Width="16" />
      <ColumnDefinition Width="*" />
    </Grid.ColumnDefinitions>

    <StackPanel>
      <flourish:ActionCard
        Icon="&#xE9D2;"
        Title="Generate report"
        Content="Start a report generation pass.">
        <flourish:ActionCard.Body>
          <flourish:Button Click="GenerateReport_Click" Content="Generate" />
        </flourish:ActionCard.Body>
      </flourish:ActionCard>
      <flourish:ActionCard
        Margin="{DynamicResource FlourishActionCardPeerMargin}"
        Icon="&#xE74D;"
        Title="Output history"
        Content="Remove all recorded messages.">
        <flourish:ActionCard.Body>
          <flourish:Button Click="ClearOutput_Click" Content="Clear" />
        </flourish:ActionCard.Body>
      </flourish:ActionCard>
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

`WriteLine` appends a line and scrolls to the end; `null` or no argument adds an empty line. Append results and failures instead of replacing history.

`Output` returns the complete history as a read-only string. Read it when another operation needs a snapshot, but use `WriteLine` to add content. `Clear` removes the complete history and returns the viewport to its initial position.

| Member | Type | Behavior |
| --- | --- | --- |
| `Output` | `string` | Gets the complete output history. The dependency property is read-only. |
| `WriteLine(string? message)` | Method | Appends one message, or an empty line for `null`, and scrolls to the end. |
| `Clear()` | Method | Removes every message and returns the viewport to the top. |

## Height and scrolling

With automatic height, the output history does not increase `OutputCard`'s desired height. Its minimum height still participates in measurement, while a stretching parent or an explicit height determines the arranged viewport size.

Place ActionCards and `OutputCard` in the same auto-sized `Grid` row, leaving `VerticalAlignment="Stretch"`. The actions set the row height and output scrolls within it. Do not derive `Height` from `Output` or add another scroll container.

Long lines do not wrap and use the horizontal scrollbar when needed. The viewport uses the standard Flourish scrollbars, whose narrow visible thumbs keep both axes available without adding substantial visual weight.

When the pointer is over overflowing output, the inner viewport consumes the mouse wheel while it can move in that direction. At the top or bottom boundary, outward wheel input continues to the containing page instead of trapping scrolling inside the card.

## Viewport and typography

The output viewport uses a darker rounded neutral background with no outer padding, so it is the complete visible card surface. Output uses the compact Small typography tier, the dedicated `FlourishOutputFontFamily` monospaced family (Consolas by default), and a theme-specific green foreground. Set `FontFamily` or `Foreground` on an individual `OutputCard` when a different output treatment is required.

Give the control an `AutomationProperties.Name` when the surrounding section and action labels do not already identify the output clearly.

## Related content

- [Card](card.md)
- [Chunk](chunk.md)
- [ScrollViewer](scroll-viewer.md)
- [OutputCard API](xref:ArkheideSystem.Flourish.Controls.OutputCard)
