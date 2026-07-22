---
title: OutputCard
description: Append operation messages to a compact scrolling history that does not determine a peer layout's height.
---

# OutputCard

`OutputCard` displays raw messages, ongoing progress, completed results, and failures in one read-only history. It owns a themed neutral viewport inside the standard card spacing, uses compact text, and scrolls when the history exceeds the arranged height.

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

Every `WriteLine` call adds the supplied message as the next line and scrolls the viewport to the end. Passing `null`, or using the overload without a message, adds an empty line. Completed results and failures follow the same append-only rule as progress messages; do not replace the existing history with the latest state.

`Output` returns the complete history as a read-only string. Read it when another operation needs a snapshot, but use `WriteLine` to add content. `Clear` removes the complete history and returns the viewport to its initial position.

| Member | Type | Behavior |
| --- | --- | --- |
| `Output` | `string` | Gets the complete output history. The dependency property is read-only. |
| `WriteLine(string? message)` | Method | Appends one message, or an empty line for `null`, and scrolls to the end. |
| `Clear()` | Method | Removes every message and returns the viewport to the top. |

## Height and scrolling

With automatic height, the output history does not increase `OutputCard`'s desired height. Its minimum height still participates in measurement, while a stretching parent or an explicit height determines the arranged viewport size.

For an ActionCard-plus-output layout, place the complete ActionCard column and `OutputCard` in the same auto-sized `Grid` row and leave `OutputCard.VerticalAlignment` as `Stretch`. The ActionCard column then determines the row height. Additional output remains inside the viewport and uses its vertical scrollbar instead of making the ActionCards taller. Do not recalculate `Height` from `Output` or wrap `OutputCard` in another scrolling container.

Long lines do not wrap and use the horizontal scrollbar when needed. The viewport uses compact scrollbars so both axes remain available without adding substantial visual weight.

When the pointer is over overflowing output, the inner viewport consumes the mouse wheel while it can move in that direction. At the top or bottom boundary, outward wheel input continues to the containing page instead of trapping scrolling inside the card.

## Viewport and typography

The output viewport uses a darker rounded neutral background and reduced outer spacing so messages occupy more of the card surface. Output uses the compact Small typography tier, the dedicated `FlourishOutputFontFamily` monospaced family (Consolas by default), and a theme-specific green foreground. Set `FontFamily` or `Foreground` on an individual `OutputCard` when a different output treatment is required.

Give the control an `AutomationProperties.Name` when the surrounding section and action labels do not already identify the output clearly.

## Related content

- [Card](card.md) covers explanatory and display-oriented information surfaces.
- [Chunk](chunk.md) explains how to group actions and their output in one page section.
- [ScrollViewer](scroll-viewer.md) describes the underlying Flourish scrolling behavior.
- The [OutputCard API](xref:ArkheideSystem.Flourish.Controls.OutputCard) lists the complete member signatures.
