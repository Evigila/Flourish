---
name: creating-page
description: Create or refactor Flourish WPF pages with consistent Chunk hierarchy, concise section copy, focused Card bodies, and appropriate interactive controls. Use when adding or editing Gallery/Page XAML, reviewing page information architecture, standardizing ChunkTitle or ChunkDescription text, deciding how many cards a section needs, or validating a Flourish page before delivery.
---

# Creating Flourish Pages

Build each page as a clear hierarchy: page introduction, concise sections, and focused content surfaces. Keep structural copy short and place details in the content that owns them.

## Build the page hierarchy

1. Place page content in a Flourish scroll viewer and use the standard page margin resource.
2. Use at most one `ChunkHero`, and place it before every ordinary `Chunk`.
3. Give each distinct topic or task one `Chunk`.
4. Use `Card` as the primary content surface inside an ordinary `Chunk`.
5. Use several peer cards when a section describes several independent behaviors, states, or tasks.

Do not make a `Chunk` and a single oversized card represent unrelated concepts. Split the section or split the card set according to the user's mental model.

## Write Chunk copy

- Write `ChunkTitle` as a short noun phrase or action-oriented label. Prefer established control or feature names.
- Keep `ChunkDescription` to one direct sentence that states the section's purpose.
- Exclude exhaustive behavior, property lists, implementation details, and multi-step instructions from `ChunkDescription`.
- Move detailed explanation to `Card.Title`, `Card.Text`, `Card.Body`, or a deliberate plain text block in the chunk body.
- Avoid repeating the card title or text in the chunk description.

Use a plain text block instead of a card when the content is continuous explanatory prose and does not need a bounded surface.

## Compose cards

Use the three Card regions consistently:

- `Title`: identify the subject or behavior.
- `Text`: explain the subject briefly and precisely.
- `Body`: host examples, controls, status, actions, lists, media, or any other detailed content. It may be empty.

Prefer explicit `Card.Body` property elements for complex XAML. Keep one card focused on one behavior. Use a `WrapPanel`, `UniformGrid`, or another parent layout to arrange peer cards.

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
      Title="Current state"
      Text="All files are synchronized."
    />
    <flourish:Card
      Margin="8,0,0,0"
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
  </UniformGrid>
</flourish:Chunk>
```

## Validate before delivery

- Confirm every page element belongs to a `Chunk` or the single leading `ChunkHero`.
- Confirm ordinary chunks use Card surfaces as their primary content containers unless continuous prose is intentional.
- Confirm each `ChunkDescription` is concise and details live in cards or body text.
- Confirm independent behaviors are split across peer cards.
- Confirm cards use `Body`, not a manually constructed catch-all text stack, for detailed content.
- Confirm interactive semantics, automation names, keyboard access, and tooltips remain correct.
- Build the Gallery and run the page architecture tests after structural changes.
