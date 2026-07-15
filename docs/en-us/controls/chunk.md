---
title: Chunk
description: Use Chunk and ChunkHero to establish Flourish page sections, page-leading hero regions, and standard spacing.
---

# Chunk

`Chunk` is the root layout unit of a Flourish page. Like a section in an article, it organizes related content into a title, an optional description, and a body while standardizing the spacing within and between sections.

> [!IMPORTANT]
> Page content belongs in a `Chunk`. `ChunkHero` is the only content control that can be a peer of `Chunk`, and it should only appear as the leading hero region at the top of a page.

A `Chunk` is not a card. It defines content hierarchy and layout rules. Place the appropriate card surface, list, or other layout inside `ChunkBody` when a section needs one.

## Basic usage

`ChunkBody` is the default XAML content property of `Chunk`, so one child can be written directly inside the element. To present several children, make a `StackPanel`, `Grid`, or another container the root of that single content tree.

```xml
<flourish:Chunk
  ChunkTitle="Recent projects"
  ChunkDescription="Continue where you left off, or open another project.">
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

### Chunk properties

| Property | Type | Default | Purpose |
| --- | --- | --- | --- |
| `ChunkTitle` | `string` | `""` | The section heading. Give every section a concise heading that describes its content. |
| `ChunkDescription` | `string?` | `null` | Optional supporting text below the heading. |
| `ChunkMargin` | `Thickness` | `0,32,0,0` | Establishes the default separation from the preceding section or the top of the page. |
| `ChunkSpacing` | `Thickness` | `0,12,0,0` | Applied as the `Margin` of both `ChunkDescription` and `ChunkBody`, establishing title-to-description and preceding-header-element-to-body spacing. |
| `ChunkBody` | `object?` | `null` | The section content tree and the default XAML content property. |

Keep the default `ChunkMargin` and `ChunkSpacing` in most pages so neighboring sections share one vertical rhythm. Override both as a group only when an entire page needs a clearly defined alternative density.

## ChunkHero

`ChunkHero` is the leading focal section of a page. It combines a title, an optional description, and an action region with a presenter. The presenter is not limited to images: it can host an `Image`, a solid-color `Border`, an illustration, a live preview, or any other content. When `Presenter` is not set, the text region automatically spans both columns and the default themed hero background remains visible.

`ChunkHeroBody` is also a default XAML content property. A hero with only body content can place its child directly inside `ChunkHero`. When a hero has both body and presenter content, the explicit `ChunkHero.ChunkHeroBody` and `ChunkHero.Presenter` property elements make the two roles clearer.

```xml
<flourish:ChunkHero
  ChunkHeroDescription="Build WPF applications with one consistent layout system."
  PresenterMode="Split"
  PresenterPosition="Right"
  ChunkHeroTitle="Welcome to Flourish">
  <flourish:ChunkHero.ChunkHeroBody>
    <StackPanel Orientation="Horizontal">
      <flourish:Button
        Variant="Filled"
        Command="{Binding CreateProjectCommand}"
        Content="Create project" />
      <flourish:Button
        Command="{Binding OpenDocumentationCommand}"
        Content="Read the documentation" />
    </StackPanel>
  </flourish:ChunkHero.ChunkHeroBody>
  <flourish:ChunkHero.Presenter>
    <Border Background="{DynamicResource FlourishPrimarySurfaceBrush}" />
  </flourish:ChunkHero.Presenter>
</flourish:ChunkHero>
```

### ChunkHero properties

| Property | Type | Default | Purpose |
| --- | --- | --- | --- |
| `ChunkHeroTitle` | `string` | `""` | The hero heading. |
| `ChunkHeroDescription` | `string?` | `null` | Optional supporting text below the heading. |
| `ChunkHeroBody` | `object?` | `null` | Actions or other supporting content associated with the hero message. |
| `PresenterMode` | `PresenterMode` | `Split` | Selects whether the presenter has a separate region or fills the hero behind its copy. |
| `PresenterPosition` | `PresenterPosition` | `Right` | Locates `Presenter` in `Split` mode. `ChunkHero` accepts only `Left` and `Right`. |
| `Presenter` | `object?` | `null` | An image, color surface, or any other presentation content for the hero. |
| `Margin` | inherited `Thickness` | `0,32,0,0` | Preserves the standard page-top inset; the first ordinary `Chunk` supplies the following separation. |

`PresenterMode` and `PresenterPosition` describe the presenter rather than the text region:

| Settings | Arrangement |
| --- | --- |
| `PresenterMode="Split"`, `PresenterPosition="Right"` | `Presenter` is on the right; the text and `ChunkHeroBody` are on the left. This is the default. |
| `PresenterMode="Split"`, `PresenterPosition="Left"` | `Presenter` is on the left; the text and `ChunkHeroBody` are on the right. |
| `PresenterMode="Overlay"` | `Presenter` fills the background; the text and `ChunkHeroBody` are overlaid on it. `PresenterPosition` is ignored. |

In `Overlay` mode, choose presenter content that keeps the text readable in both light and dark themes. If an image needs extra contrast treatment, compose it with a translucent color layer inside a `Grid` assigned to `Presenter`.

## Page structure

A typical page consists of one optional `ChunkHero` followed by any number of `Chunk` controls:

```xml
<ScrollViewer>
  <StackPanel>
    <flourish:ChunkHero
      ChunkHeroTitle="Design system"
      PresenterMode="Split"
      PresenterPosition="Right" />

    <flourish:Chunk ChunkTitle="Foundations" />
    <flourish:Chunk ChunkTitle="Components" />
    <flourish:Chunk ChunkTitle="Practices" />
  </StackPanel>
</ScrollViewer>
```

Do not place several `ChunkHero` controls on one page, and do not replace sections with peer-level free text and manual margins. When a page does not need a hero, begin directly with its first `Chunk`.

## Related content

- [Button](button.md) explains how to represent actions in `ChunkBody` and `ChunkHeroBody`.
- [Card](card.md) explains how to group information inside a `ChunkBody`.
- [Typography](../articles/configure-font.md) covers global and page-specific font configuration.
- The [Chunk API](xref:ArkheideSystem.Flourish.Controls.Chunk), [ChunkHero API](xref:ArkheideSystem.Flourish.Controls.ChunkHero), [PresenterMode API](xref:ArkheideSystem.Flourish.Controls.PresenterMode), and [PresenterPosition API](xref:ArkheideSystem.Flourish.Controls.PresenterPosition) list all members.
