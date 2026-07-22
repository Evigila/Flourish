---
title: Presenter
description: Use Presenter for full-width Split or Overlay compositions that combine copy, controls, and rich presentation content.
---

# Presenter

`Presenter` is a full-width, three-part layout for copy, supporting controls, and rich presentation content. Use it for an image, several icons, an illustration, a preview, or another composed visual. Only one `Presenter` belongs in a row.

Use [IconCard](card.md#iconcard) when a card needs one icon. Use `Presenter` when the presentation needs an image, an icon group, or its own content tree.

Every Presenter declaration explicitly supplies `Title`, `Description`, `PresenterMode`, and `PresenterPosition`. The runtime fallback values are `Split` and `Right`, but declarations still name both values so the intended composition remains visible at the call site.

## Content regions

| Property | Type | Default | Purpose |
| --- | --- | --- | --- |
| `Title` | `string` | `""` | Required presentation heading. Declare it explicitly. |
| `Description` | `string?` | `null` | Required supporting copy below the title. Declare it explicitly. |
| `Body` | `object?` | `null` | Controls or supporting content arranged in the same copy region as the text. Assign it with `Presenter.Body`; it is not the default XAML content property. |
| `Presentation` | `object?` | `null` | The image, icon group, illustration, preview, or composed content being presented. It is the default XAML content property. |
| `PresenterMode` | `PresenterMode` | `Split` | Required explicit composition choice. The runtime fallback is `Split`. |
| `PresenterPosition` | `PresenterPosition` | `Right` | Required explicit presentation-side choice. The runtime fallback is `Right`. |

An absent `Body` collapses completely together with its spacing. The copy-and-body side remains transparent. Only the `Presentation` region uses the adaptive light-neutral background and shared surface corner radius, making the two sides visually distinct without adding another card around `Body`.

When several Presenters are stacked vertically in one section, apply `FlourishPresenterPeerMargin` to each Presenter after the first. This keeps Presenter rhythm separate from Card spacing without requiring a hard-coded margin.

## Split mode

`Split` is the standard mode and always uses a fixed horizontal two-region layout. `Title`, `Description`, and the separate `Body` host share the same left alignment in the copy region. The rounded `Presentation` surface fills the other region, while its content is centered within that complete surface by default. `PresenterPosition` always describes the presentation region, not the text:

| Position | Arrangement |
| --- | --- |
| `Right` | Copy and `Body` are on the left; `Presentation` is on the right. This is the standard arrangement and runtime fallback. |
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

In `Overlay` mode, the rounded `Presentation` surface and its root content span the complete control, while the title, description, and body render above it. `PresenterPosition` has no visual effect, but the declaration still supplies it as part of the Presenter contract.

```xml
<flourish:Presenter
  MinHeight="240"
  Title="Release highlights"
  Description="Explore what changed in this version."
  PresenterMode="Overlay"
  PresenterPosition="Right">
  <flourish:Presenter.Presentation>
    <Image Source="Assets/release-highlights.png" Stretch="UniformToFill" />
  </flourish:Presenter.Presentation>
</flourish:Presenter>
```

Choose or compose presentation content that keeps overlaid text readable in both light and dark themes. A `Grid` assigned to `Presentation` can combine an image with a contrast layer when necessary.

## Present several elements

`Presentation` accepts one WPF content tree. Wrap several icons or visual elements in a layout container:

```xml
<flourish:Presenter
  Title="Supported formats"
  Description="Review the formats available for this export."
  PresenterMode="Split"
  PresenterPosition="Right">
  <UniformGrid Columns="3">
    <flourish:FlourishTextBlock Role="Icon" Text="&#xE8A5;" />
    <flourish:FlourishTextBlock Role="Icon" Text="&#xE7C3;" />
    <flourish:FlourishTextBlock Role="Icon" Text="&#xE8B7;" />
  </UniformGrid>
</flourish:Presenter>
```

The direct `UniformGrid` is assigned to `Presentation`, the default XAML content property. Always use an explicit `<flourish:Presenter.Body>` element when the copy side also needs supporting controls.

## ChunkHero

`ChunkHero` inherits `Presenter` and uses the same explicit `Title`, `Description`, `PresenterMode`, and `PresenterPosition` contract, including the same `Body` and `Presentation` regions. It is a page-level peer of `Chunk`, uses an emphasized background and the dedicated HeaderSize title, and appears exactly once at the beginning of every content page. An ordinary `Presenter` remains the smaller layout whose rounded neutral surface belongs only to its presentation region.

## Related content

- [Chunk](chunk.md) defines the page hierarchy and the specialized `ChunkHero`.
- [Card](card.md) explains when concise text or one icon belongs on a card.
- [Paragraph](paragraph.md) presents several text-only paragraphs.
- [Button](button.md) defines controls that may appear in `Body`.
- The [Presenter API](xref:ArkheideSystem.Flourish.Controls.Presenter), [PresenterMode API](xref:ArkheideSystem.Flourish.Controls.PresenterMode), and [PresenterPosition API](xref:ArkheideSystem.Flourish.Controls.PresenterPosition) list all members.
