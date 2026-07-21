---
title: Presenter
description: Use Presenter for full-width Split or Overlay compositions that combine copy, controls, and rich presentation content.
---

# Presenter

`Presenter` is a full-width, three-part layout for copy, supporting controls, and rich presentation content. Use it for an image, several icons, an illustration, a preview, or another composed visual. Only one `Presenter` belongs in a row.

Use [IconCard](card.md#iconcard) when a card needs one icon. Use `Presenter` when the presentation needs an image, an icon group, or its own content tree.

## Content regions

| Property | Type | Default | Purpose |
| --- | --- | --- | --- |
| `Title` | `string` | `""` | Optional presentation heading. |
| `Description` | `string?` | `null` | Optional supporting copy below the title. |
| `Body` | `object?` | `null` | Controls or supporting content arranged in the same copy region as the text. It is the default XAML content property. |
| `Presentation` | `object?` | `null` | The image, icon group, illustration, preview, or composed content being presented. |
| `PresenterMode` | `PresenterMode` | `Split` | Selects a side-by-side or overlaid composition. |
| `PresenterPosition` | `PresenterPosition` | `Right` | Places `Presentation` at `Left` or `Right` in `Split` mode. |

Every optional region collapses completely when it is empty or `null`, including its associated spacing. `Presenter` is transparent and borderless by default; set inherited surface properties only when the surrounding design calls for a distinct treatment.

## Split mode

`Split` is the default mode. `PresenterPosition` always describes the presentation region, not the text:

| Position | Arrangement |
| --- | --- |
| `Right` | Copy and `Body` are on the left; `Presentation` is on the right. This is the default. |
| `Left` | `Presentation` is on the left; copy and `Body` are on the right. |

```xml
<flourish:Presenter
  Title="Workspace overview"
  Description="See activity and open the complete report."
  PresenterMode="Split"
  PresenterPosition="Right">
  <flourish:Presenter.Body>
    <flourish:Button
      Command="{Binding OpenReportCommand}"
      Content="Open report" />
  </flourish:Presenter.Body>
  <flourish:Presenter.Presentation>
    <Image Source="Assets/workspace-overview.png" Stretch="Uniform" />
  </flourish:Presenter.Presentation>
</flourish:Presenter>
```

`PresenterPosition` accepts only `Left` and `Right`. For an icon above, below, or beside short card copy, use `IconCard.IconPosition` instead.

## Overlay mode

In `Overlay` mode, `Presentation` fills the control and the title, description, and body render above it. `PresenterPosition` has no visual effect.

```xml
<flourish:Presenter
  MinHeight="240"
  Title="Release highlights"
  Description="Explore what changed in this version."
  PresenterMode="Overlay">
  <flourish:Presenter.Presentation>
    <Image Source="Assets/release-highlights.png" Stretch="UniformToFill" />
  </flourish:Presenter.Presentation>
</flourish:Presenter>
```

Choose or compose presentation content that keeps overlaid text readable in both light and dark themes. A `Grid` assigned to `Presentation` can combine an image with a contrast layer when necessary.

## Present several elements

`Presentation` accepts one WPF content tree. Wrap several icons or visual elements in a layout container:

```xml
<flourish:Presenter Title="Supported formats">
  <flourish:Presenter.Presentation>
    <UniformGrid Columns="3">
      <flourish:FlourishTextBlock Role="Icon" Text="&#xE8A5;" />
      <flourish:FlourishTextBlock Role="Icon" Text="&#xE7C3;" />
      <flourish:FlourishTextBlock Role="Icon" Text="&#xE8B7;" />
    </UniformGrid>
  </flourish:Presenter.Presentation>
</flourish:Presenter>
```

## ChunkHero

`ChunkHero` inherits `Presenter` and uses the same fields and modes, including `Presentation`. It is a page-level peer of `Chunk`, uses an emphasized background and the dedicated HeaderSize title, and appears exactly once at the beginning of every content page. An ordinary `Presenter` remains the smaller, transparent presentation layout used inside a `Chunk` body.

## Related content

- [Chunk](chunk.md) defines the page hierarchy and the specialized `ChunkHero`.
- [Card](card.md) explains when concise text or one icon belongs on a card.
- [Paragraph](paragraph.md) presents several text-only paragraphs.
- [Button](button.md) defines controls that may appear in `Body`.
- The [Presenter API](xref:ArkheideSystem.Flourish.Controls.Presenter), [PresenterMode API](xref:ArkheideSystem.Flourish.Controls.PresenterMode), and [PresenterPosition API](xref:ArkheideSystem.Flourish.Controls.PresenterPosition) list all members.
