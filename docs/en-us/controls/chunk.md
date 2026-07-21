---
title: Chunk
description: Use Chunk and ChunkHero to build the required full-width section hierarchy of a Flourish page.
---

# Chunk

`Chunk` is the root layout unit of a Flourish main-navigation content page. Every such page starts with exactly one `ChunkHero` and continues with one or more `Chunk` controls. Both controls fill their row, so do not place two of them side by side.

> [!IMPORTANT]
> Put all page content inside the leading `ChunkHero` or a following `Chunk`. Do not add peer-level text, cards, or manually spaced regions outside that structure.

> [!NOTE]
> This page skeleton does not apply to Shell-owned transient surfaces such as the profile flyout, a popup, or a dialog. Those surfaces are not main-navigation content pages and should not be given an oversized `ChunkHero`. Their internal content still follows the Flourish typography, spacing, control-selection, and Button-family rules.

## Basic usage

Every `Chunk` needs a concise `Title` and a `Body`. `Description` is optional: add it only when the title cannot communicate the section's essential context on its own.

`Body` is the default XAML content property. One child can therefore be written directly inside `Chunk`; use a layout container when the section needs several children.

```xml
<flourish:Chunk
  Title="Recent projects"
  Description="Continue where you left off, or open another project.">
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
| `Title` | `string` | `""` | Required section heading that identifies the section's subject. |
| `Description` | `string?` | `null` | Optional supporting copy below the heading. |
| `Body` | `object?` | `null` | Required section content and the default XAML content property. |
| `ChunkMargin` | `Thickness` | `0,32,0,0` | Supplies the large separation from the preceding page section. |
| `ChunkSpacing` | `Thickness` | `0,12,0,0` | Supplies the internal separation between populated regions. |

An empty or `null` optional region collapses completely, including its associated spacing. Keep the default `ChunkMargin` and `ChunkSpacing` in ordinary pages so all sections follow the same vertical rhythm.

`Chunk` defines layout only. Choose a [Card](card.md), [Paragraph](paragraph.md), [Presenter](presenter.md), or another purpose-built control for `Body` rather than making `Chunk` present the content itself.

## ChunkHero

`ChunkHero` is the single page-leading hero section. It inherits the complete [Presenter](presenter.md) contract: `Title`, `Description`, `Body`, `Presentation`, `PresenterMode`, and `PresenterPosition`. Its larger title and emphasized background distinguish it from an ordinary `Presenter`.

```xml
<flourish:ChunkHero
  Title="Welcome to Flourish"
  Description="Build WPF applications with one consistent layout system."
  PresenterMode="Split"
  PresenterPosition="Right">
  <flourish:ChunkHero.Body>
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
  </flourish:ChunkHero.Body>
  <flourish:ChunkHero.Presentation>
    <Image Source="Assets/flourish-hero.png" Stretch="Uniform" />
  </flourish:ChunkHero.Presentation>
</flourish:ChunkHero>
```

| Property | Type | Default | Purpose |
| --- | --- | --- | --- |
| `Title` | `string` | `""` | Required page heading, rendered with the dedicated HeaderSize tier. |
| `Description` | `string?` | `null` | Optional supporting copy for the page heading. |
| `Body` | `object?` | `null` | Supporting controls or content arranged with the hero copy. It is the default XAML content property. |
| `Presentation` | `object?` | `null` | An image, icon group, illustration, or other presented content. |
| `PresenterMode` | `PresenterMode` | `Split` | Places the presentation beside the copy or behind it. |
| `PresenterPosition` | `PresenterPosition` | `Right` | Places the presentation at `Left` or `Right` in `Split` mode. |

Optional hero regions follow the same collapse rule as `Chunk`: an absent `Description`, `Body`, or `Presentation` leaves no empty placeholder or spacing. In `Overlay` mode, choose presentation content that keeps all overlaid copy readable in light and dark themes.

## Page structure

The canonical page skeleton contains one leading `ChunkHero` followed by several full-width `Chunk` controls. Each ordinary chunk still supplies a real body.

```xml
<ScrollViewer>
  <StackPanel>
    <flourish:ChunkHero
      Title="Design system"
      Description="Foundations and reusable controls for this application."
      Presentation="{StaticResource DesignSystemIllustration}" />

    <flourish:Chunk Title="Foundations">
      <flourish:Card MainText="Color, typography, spacing, and motion." />
    </flourish:Chunk>

    <flourish:Chunk Title="Components">
      <flourish:Card MainText="Reusable controls built on those foundations." />
    </flourish:Chunk>
  </StackPanel>
</ScrollViewer>
```

Do not add a second `ChunkHero`, omit the leading hero, or place multiple chunks in the same row.

## Related content

- [Presenter](presenter.md) defines the presentation model inherited by `ChunkHero`.
- [Paragraph](paragraph.md) presents several text paragraphs as a chunk's sole body.
- [Card](card.md) presents concise information inside a chunk.
- [Button](button.md) defines actions used in chunk and hero bodies.
- [Typography](../articles/configure-font.md) describes the six font-size tiers.
- The [Chunk API](xref:ArkheideSystem.Flourish.Controls.Chunk) and [ChunkHero API](xref:ArkheideSystem.Flourish.Controls.ChunkHero) list all members.
