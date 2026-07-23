---
title: Chunk
description: Use HeaderChunk and Chunk as the full-width section hierarchy inside a PageBody.
---

# Chunk

`HeaderChunk` and `Chunk` are the direct layout units of a Flourish content page. A standard page begins with one `HeaderChunk` and continues with one or more `Chunk` controls. Both fill their row, so do not arrange them side by side.

Place them directly in [PageBody](page-body.md). `PageBody` rejects every other direct child, more than one `HeaderChunk`, and a `HeaderChunk` that is not first.

> [!NOTE]
> Shell-owned transient surfaces such as the profile flyout, a popup, or a dialog are not content pages and do not need the complete PageBody hierarchy. Their internal content still follows the Flourish typography, spacing, control-selection, and Button-family rules.

## Chunk

Every `Chunk` needs a concise `Title` and a real `Body`. `Content` is optional: add it only when the title cannot communicate the section's essential context on its own.

`Body` is the default XAML content property. Write one child directly inside `Chunk`, or use a layout container when the section needs several peer controls.

```xml
<flourish:Chunk
  Title="Recent projects"
  Content="Continue where you left off, or open another project.">
  <UniformGrid Columns="2">
    <flourish:CardButton
      Command="{Binding OpenProjectCommand}"
      Content="Last edited today"
      Title="Flourish" />
    <flourish:CardButton
      Command="{Binding BrowseCommand}"
      Content="Choose a project from disk"
      Title="Open another project" />
  </UniformGrid>
</flourish:Chunk>
```

| Property | Type | Default | Purpose |
| --- | --- | --- | --- |
| `Title` | `string` | `""` | Required section heading that identifies the subject. |
| `Content` | `string?` | `null` | Optional supporting copy below the heading. |
| `Body` | `object?` | `null` | Required section content and the default XAML content property. |
| `ChunkMargin` | `Thickness` | `0,32,0,0` | Supplies the large separation from the preceding page section. |
| `ChunkSpacing` | `Thickness` | `0,12,0,0` | Supplies internal separation between populated regions. |

An empty or `null` optional region collapses completely, including associated spacing. Keep the standard `ChunkMargin` and `ChunkSpacing` in ordinary pages so all sections follow the same vertical rhythm.

`Chunk` defines layout only. Choose a [Card](card.md), [Document](document.md), [Presenter](presenter.md), or another purpose-built control for `Body` rather than making `Chunk` present content itself.

## HeaderChunk

`HeaderChunk` is the single page-leading section and a larger [Presenter](presenter.md) specialization. Explicitly supply `Title`, `Content`, `PresenterMode`, and `PresenterPosition`. Its `Body` supports the copy while `Presentation` holds the presented visual.

Unlike an ordinary `Presenter`, direct XAML content is assigned to `HeaderChunk.Body`. Assign presentation content explicitly with `HeaderChunk.Presentation`.

```xml
<flourish:HeaderChunk
  Title="Welcome to Flourish"
  Content="Build WPF applications with one consistent layout system."
  PresenterMode="Split"
  PresenterPosition="Right">
  <flourish:HeaderChunk.Body>
    <StackPanel Orientation="Horizontal">
      <flourish:Button
        Variant="Filled"
        Command="{Binding CreateProjectCommand}"
        Content="Create project" />
      <flourish:Button
        Margin="12,0,0,0"
        Command="{Binding OpenDocumentationCommand}"
        Content="Read the documentation" />
    </StackPanel>
  </flourish:HeaderChunk.Body>
  <flourish:HeaderChunk.Presentation>
    <Image Source="Assets/flourish-header.png" Stretch="Uniform" />
  </flourish:HeaderChunk.Presentation>
</flourish:HeaderChunk>
```

| Property | Type | Default | Purpose |
| --- | --- | --- | --- |
| `Title` | `string` | `""` | Required page heading rendered with the HeaderSize tier. |
| `Content` | `string?` | `null` | Required supporting copy for the page heading. |
| `Body` | `object?` | `null` | Supporting controls in the same region as the copy; the default XAML content property. |
| `Presentation` | `object?` | `null` | Image, icon group, illustration, or other presented content. |
| `PresenterMode` | `PresenterMode` | `Split` | Explicit composition choice: `Split`, `TopDown`, or `Overlay`. |
| `PresenterPosition` | `PresenterPosition` | `Right` | Explicit presentation-side choice for `Split`. |

An absent `Body` or `Presentation` leaves no placeholder or spacing. `Split` keeps title, content, and body together on one side and presentation content on the other. `TopDown` places the presentation above a left-aligned copy-and-body region. `Overlay` places copy and body over the presentation. HeaderChunk remains full-width and alone in its row in every mode, including TopDown. Continue to declare `PresenterPosition` in every mode even though `TopDown` and `Overlay` do not use it for placement.

## Complete page structure

```xml
<flourish:PageBody>
  <flourish:HeaderChunk
    Title="Design system"
    Content="Foundations and reusable controls for this application."
    PresenterMode="Split"
    PresenterPosition="Right"
    Presentation="{StaticResource DesignSystemIllustration}" />

  <flourish:Chunk Title="Foundations">
    <flourish:Card Content="Color, typography, spacing, and motion." />
  </flourish:Chunk>

  <flourish:Chunk Title="Components">
    <flourish:Card Content="Reusable controls built on those foundations." />
  </flourish:Chunk>
</flourish:PageBody>
```

Do not add a second `HeaderChunk`, place it after a `Chunk`, or put peer content outside the PageBody sections.

## Related content

- [PageBody](page-body.md) enforces the page root and direct-child contract.
- [Presenter](presenter.md) defines the composition model inherited by `HeaderChunk`.
- [Document](document.md) presents several text paragraphs as a chunk's sole body.
- [Card](card.md) presents concise information inside a chunk.
- [Typography](../articles/configure-font.md) describes the font-size tiers.
- The [Chunk API](xref:ArkheideSystem.Flourish.Controls.Chunk) and [HeaderChunk API](xref:ArkheideSystem.Flourish.Controls.HeaderChunk) list all members.
