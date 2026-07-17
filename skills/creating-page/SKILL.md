---
name: creating-page
description: Create or refactor Flourish WPF pages with full-width Chunk hierarchy, focused Card-family surfaces, compact ListCard configuration rows, dedicated OutputCard message histories, equal-height peer layouts, and appropriate interactive controls. Use when adding or editing Gallery/Page XAML, reviewing page information architecture, standardizing ChunkTitle or ChunkDescription text, choosing between Card, ListCard, OutputCard, IconCard, and CardButton, deciding how many surfaces a section needs, or validating a Flourish page before delivery.
---

# Creating Flourish Pages

Build each page as a clear hierarchy: page introduction, concise sections, and focused content surfaces. Keep structural copy short and place details in the content that owns them.

## Build the page hierarchy

1. Place page content in a Flourish scroll viewer and use the standard page margin resource.
2. Use at most one `ChunkHero`, and place it before every ordinary `Chunk`.
3. Give each distinct topic or task one `Chunk`.
4. Make every `Chunk` full-width. Never place two chunks beside each other in one row.
5. Use the Flourish surface whose semantics match the content inside an ordinary `Chunk`.
6. Use several peer cards or a column of `ListCard` rows inside one chunk when a section describes several related behaviors, states, settings, or tasks.

Do not make a `Chunk` and a single oversized card represent unrelated concepts. Split unrelated topics into full-width chunks; split related behaviors into peer cards inside one chunk.

## Write Chunk copy

- Write `ChunkTitle` as a short noun phrase or action-oriented label. Prefer established control or feature names.
- Keep `ChunkDescription` to one direct sentence that states the section's purpose.
- Exclude exhaustive behavior, property lists, implementation details, and multi-step instructions from `ChunkDescription`.
- Move detailed explanation to the chosen surface's `Title`, `Text`, or `Body`, or to a deliberate plain text block in the chunk body. `OutputCard` is the exception: it contains messages only, so its context belongs in the action surface or Chunk copy.
- Avoid repeating the card title or text in the chunk description.

Use a plain text block instead of a card when the content is continuous explanatory prose and does not need a bounded surface.

## Compose cards

Use the three Card regions consistently:

- `Title`: identify the card's single subject or behavior. Use exactly one title.
- `Text`: serve as an ordinary card's single Description region for supporting explanatory copy.
- `Body`: host examples, controls, status, actions, lists, media, or any other detailed content. It may be empty.

Do not add another heading or explanatory paragraph inside an ordinary card's `Body`. Necessary field labels, option labels, result data, and list items are content rather than competing card copy and may remain in `Body`.

Move changing response text out of an action surface and place an adjacent `OutputCard`. Append raw messages, progress, completed results, and failures through `WriteLine` so earlier outcomes remain visible. `OutputCard` has no `Title`, `Text`, or arbitrary `Body`; keep its explanatory context in the action surface or containing Chunk. Use `Clear` only for an explicit history-reset action. Keep the action and output surfaces in the same chunk.

Prefer explicit `Card.Body` property elements for complex XAML. Keep one card focused on one behavior. Use a `UniformGrid` or another parent layout that gives ordinary peer cards in a row the same arranged height. For a two-column ListCard-plus-OutputCard composition, put the complete ListCard column and `OutputCard` in one auto-sized `Grid` row. Let the ListCard column determine the row height and stretch `OutputCard` to match it; its history must scroll internally instead of increasing the row's desired height. Never calculate `Height` from `Output`. Use the same gap between columns and rows; prefer a shared spacing resource or one consistent value.

Choose the correct semantic control:

- Use `ListCard` for one compact configuration option or local configuration action.
- Use `Card` for longer explanatory or display-oriented grouped information.
- Use `OutputCard` for append-only raw messages, progress, completed results, and failures.
- Use `IconCard` when an icon, image, illustration, or preview is part of longer explanatory or display-oriented information.
- Use `CardButton` when invoking the entire surface is the action.
- Use an ordinary `Button` or `IconButton` inside `Body` when only a local action is interactive.

Do not add click handlers to `Card`, `ListCard`, or `IconCard`, or use visual variants to imply behavior that belongs to another control type.

## Compose ListCards

Treat `ListCard` as a compact, non-interactive row for one independent setting. It inherits `Title`, `Text`, and `Body` from `Card`, adds an optional `Presenter`, and always uses the `Standard` variant. The presenter stays on the left with deliberately generous horizontal breathing room, the title and short description form a vertical copy stack in the center, and `Body` stays on the right; all three regions are vertically centered. Do not reduce the presenter spacing with local margins or attempt to reposition the regions with Card alignment properties.

Keep both `Title` and `Text` concise. Each is limited to one line and overflows with an ellipsis; rewrite copy that would depend on wrapping. Put exactly one interactive control in each `ListCard.Body`. Do not combine several inputs or actions in a panel inside one row.

Stack related ListCards so each row fills its column. Use `FlourishListCardPeerMargin` between consecutive rows to create a compact visual group while preserving distinct surfaces. Do not mix `ListCard` with any other card type in that same column. Prefer a single-column chunk containing only ListCards. A chunk may still use multiple columns when another column has a different purpose, such as an adjacent `OutputCard`.

Prefer `FlourishComboBox`, `FlourishCheckBox`, or `Button` in `ListCard.Body`; use `FlourishTextBox` or `FlourishRadioButton` when the option genuinely needs them. Apply selections, toggles, and edits immediately. Never add a separate Apply action to a ListCard.

## Example

```xml
<flourish:Chunk
  ChunkTitle="Synchronization"
  ChunkDescription="Review and control workspace synchronization."
>
  <Grid>
    <Grid.ColumnDefinitions>
      <ColumnDefinition Width="*" />
      <ColumnDefinition Width="16" />
      <ColumnDefinition Width="*" />
    </Grid.ColumnDefinitions>
    <StackPanel>
      <flourish:ListCard
        Title="Sync mode"
        Text="Choose when workspace changes synchronize."
      >
        <flourish:ListCard.Presenter>
          <flourish:FlourishTextBlock
            AutomationProperties.Name="Synchronization"
            Role="Icon"
            Text="&#xE895;"
          />
        </flourish:ListCard.Presenter>
        <flourish:ListCard.Body>
          <flourish:FlourishComboBox
            Width="160"
            ItemsSource="{Binding SynchronizationModes}"
            SelectedItem="{Binding SynchronizationMode, Mode=TwoWay, UpdateSourceTrigger=PropertyChanged}"
          />
        </flourish:ListCard.Body>
      </flourish:ListCard>
      <flourish:ListCard
        Margin="{DynamicResource FlourishListCardPeerMargin}"
        Title="Refresh now"
        Text="Request a new synchronization pass."
      >
        <flourish:ListCard.Body>
          <flourish:Button Click="Refresh_Click" Content="Refresh" />
        </flourish:ListCard.Body>
      </flourish:ListCard>
    </StackPanel>
    <flourish:OutputCard
      x:Name="SynchronizationOutput"
      Grid.Column="2"
      AutomationProperties.Name="Synchronization output"
      VerticalAlignment="Stretch"
    />
  </Grid>
</flourish:Chunk>
```

Append a message for each observable outcome instead of replacing the existing history:

```csharp
private void Refresh_Click(object sender, RoutedEventArgs e)
{
    SynchronizationOutput.WriteLine("Synchronization requested.");
    SynchronizationOutput.WriteLine("Synchronization completed.");
}
```

## Validate before delivery

- Confirm every page element belongs to a `Chunk` or the single leading `ChunkHero`.
- Confirm every chunk spans the available row and no parent arranges two chunks side by side.
- Confirm ordinary chunks use appropriate Flourish surfaces as their primary content containers unless continuous prose is intentional.
- Confirm each `ChunkDescription` is concise and details live in cards or body text.
- Confirm each ordinary card has one title and one Description region, with no competing headings or explanatory paragraphs in `Body`.
- Confirm independent behaviors are split across peer surfaces and changing output is isolated in an adjacent `OutputCard` with context in the action surface or Chunk.
- Confirm every raw message, progress update, completed result, and failure is appended through `WriteLine` instead of replacing earlier history.
- Confirm `OutputCard` has no attempted `Title`, `Text`, or `Body` content, and `Clear` is reserved for an explicit history-reset action.
- Confirm compact settings use one `ListCard` per independent option and longer copy or display content remains in `Card` or `IconCard`.
- Confirm each ListCard keeps its presenter left, copy centered, Body right, all regions vertically centered, and its variant `Standard`.
- Confirm each ListCard Title and Text is concise, limited to one line, and safe to trim with an ellipsis.
- Confirm presenter spacing remains generous on both horizontal sides and local margins do not collapse it.
- Confirm each ListCard Body contains exactly one interactive control.
- Confirm consecutive ListCards use `FlourishListCardPeerMargin` between rows and do not add that margin before the first row.
- Confirm a ListCard column contains only ListCards; place `OutputCard` or other surface types in a separate column.
- Confirm ListCard selections, toggles, and edits apply immediately and no ListCard adds an Apply action.
- Confirm a two-column ListCard-plus-OutputCard composition uses one auto-sized Grid row, lets the ListCard column determine its height, stretches `OutputCard`, and keeps overflow inside the output viewport.
- Confirm peer cards in a row share a height and horizontal and vertical gaps use the same spacing.
- Confirm ordinary Card-family controls use `Body`, not a manually constructed catch-all text stack, for detailed content; `OutputCard` contains only messages appended through `WriteLine`.
- Confirm interactive semantics, automation names, keyboard access, and tooltips remain correct.
- Build the Gallery and run the page architecture tests after structural changes.
