---
name: creating-page
description: Create or refactor Flourish WPF pages with full-width Chunk hierarchy, single-description Card copy, dedicated Output or Result cards, equal-height peer layouts, and appropriate interactive controls. Use when adding or editing Gallery/Page XAML, reviewing page information architecture, standardizing ChunkTitle or ChunkDescription text, deciding how many cards a section needs, or validating a Flourish page before delivery.
---

# Creating Flourish Pages

Build each page as a clear hierarchy: page introduction, concise sections, and focused content surfaces. Keep structural copy short and place details in the content that owns them.

## Build the page hierarchy

1. Place page content in a Flourish scroll viewer and use the standard page margin resource.
2. Use at most one `ChunkHero`, and place it before every ordinary `Chunk`.
3. Give each distinct topic or task one `Chunk`.
4. Make every `Chunk` full-width. Never place two chunks beside each other in one row.
5. Use `Card` as the primary content surface inside an ordinary `Chunk`.
6. Use several peer cards inside one chunk when a section describes several related behaviors, states, or tasks.

Do not make a `Chunk` and a single oversized card represent unrelated concepts. Split unrelated topics into full-width chunks; split related behaviors into peer cards inside one chunk.

## Write Chunk copy

- Write `ChunkTitle` as a short noun phrase or action-oriented label. Prefer established control or feature names.
- Keep `ChunkDescription` to one direct sentence that states the section's purpose.
- Exclude exhaustive behavior, property lists, implementation details, and multi-step instructions from `ChunkDescription`.
- Move detailed explanation to `Card.Title`, `Card.Text`, `Card.Body`, or a deliberate plain text block in the chunk body.
- Avoid repeating the card title or text in the chunk description.

Use a plain text block instead of a card when the content is continuous explanatory prose and does not need a bounded surface.

## Compose cards

Use the three Card regions consistently:

- `Title`: identify the card's single subject or behavior. Use exactly one title.
- `Text`: serve as the card's single Description region for all supporting explanatory copy.
- `Body`: host examples, controls, status, actions, lists, media, or any other detailed content. It may be empty.

Do not add another heading or explanatory paragraph inside `Body`. Necessary field labels, option labels, result data, and list items are content rather than competing card copy and may remain in `Body`.

Move changing response text out of an action card. Place an adjacent `Output` card for raw or ongoing output, or a `Result` card for a completed result. Keep both cards in the same chunk.

Prefer explicit `Card.Body` property elements for complex XAML. Keep one card focused on one behavior. Use a `UniformGrid` or another parent layout that gives every card in a row the same height. Use the same gap between columns and rows; prefer a shared spacing resource or one consistent margin value.

Choose the correct semantic control:

- Use `Card` for non-interactive grouped information.
- Use `IconCard` when an icon, image, illustration, or preview is part of the information hierarchy.
- Use `CardButton` when invoking the entire surface is the action.
- Use an ordinary `Button` or `IconButton` inside `Card.Body` when only a local action is interactive.

Do not add click handlers to `Card` or use visual variants to imply behavior that belongs to another control type.

## Example

```xml
<flourish:Chunk
  ChunkTitle="Synchronization"
  ChunkDescription="Review and control workspace synchronization."
>
  <UniformGrid Columns="2">
    <flourish:Card
      Margin="0,0,8,0"
      Title="Manual refresh"
      Text="Request a new synchronization pass."
    >
      <flourish:Card.Body>
        <flourish:Button
          HorizontalAlignment="Left"
          Command="{Binding RefreshCommand}"
          Content="Refresh"
        />
      </flourish:Card.Body>
    </flourish:Card>
    <flourish:Card
      Margin="8,0,0,0"
      Title="Result"
      Text="The most recent synchronization result."
    >
      <flourish:Card.Body>
        <flourish:FlourishTextBlock
          Role="Status"
          Text="{Binding SynchronizationResult}"
        />
      </flourish:Card.Body>
    </flourish:Card>
  </UniformGrid>
</flourish:Chunk>
```

## Validate before delivery

- Confirm every page element belongs to a `Chunk` or the single leading `ChunkHero`.
- Confirm every chunk spans the available row and no parent arranges two chunks side by side.
- Confirm ordinary chunks use Card surfaces as their primary content containers unless continuous prose is intentional.
- Confirm each `ChunkDescription` is concise and details live in cards or body text.
- Confirm each card has one title and one Description region, with no competing headings or explanatory paragraphs in `Body`.
- Confirm independent behaviors are split across peer cards and changing output is isolated in an adjacent Output or Result card.
- Confirm peer cards in a row share a height and horizontal and vertical gaps use the same spacing.
- Confirm cards use `Body`, not a manually constructed catch-all text stack, for detailed content.
- Confirm interactive semantics, automation names, keyboard access, and tooltips remain correct.
- Build the Gallery and run the page architecture tests after structural changes.
