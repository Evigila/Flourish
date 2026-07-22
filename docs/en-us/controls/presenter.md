---
title: Presenter
description: Use Presenter for full-width Split, TopDown, or Overlay compositions that combine copy, controls, and rich presentation content.
---

# Presenter

`Presenter` is a full-width, three-part layout for copy, supporting controls, and rich presentation content. Use it for an image, several icons, an illustration, a preview, or another composed visual. Only one Presenter belongs in a row.

Use [Card](card.md) when a surface needs at most one icon and one paragraph. Use `Presenter` when the presentation needs an image, an icon group, or its own content tree.

Every Presenter declaration explicitly supplies `Title`, `Content`, `PresenterMode`, and `PresenterPosition`. The runtime fallback values are `Split` and `Right`, but naming them at the call site keeps the intended composition clear.

## Content regions

| Property | Type | Default | Purpose |
| --- | --- | --- | --- |
| `Title` | `string` | `""` | Required presentation heading. |
| `Content` | `string?` | `null` | Required supporting copy below the title. |
| `Body` | `object?` | `null` | Controls or supporting content arranged with the copy; assign it explicitly with `Presenter.Body`. |
| `Presentation` | `object?` | `null` | Image, icon group, illustration, preview, or composed visual; the default XAML content property. |
| `PresenterMode` | `PresenterMode` | `Split` | Explicit composition choice: `Split`, `TopDown`, or `Overlay`. |
| `PresenterPosition` | `PresenterPosition` | `Right` | Explicit presentation-side choice for `Split`. |

An absent `Body` collapses with its spacing. The copy-and-body region stays transparent and aligns its contents together to the left. Only the `Presentation` region uses the adaptive light-neutral background and shared surface corner radius. That region fills its allocated space while centering the presented content within it.

When several Presenters are stacked vertically in one section, apply `FlourishPresenterPeerMargin` to each Presenter after the first.

## Split mode

`Split` uses a horizontal two-region layout. `Title`, `Content`, and `Body` remain together on one side; the rounded `Presentation` surface fills the other.

| Position | Arrangement |
| --- | --- |
| `Right` | Copy and `Body` are on the left; `Presentation` is on the right. This is the standard arrangement and runtime fallback. |
| `Left` | `Presentation` is on the left; copy and `Body` are on the right. |

```xml
<flourish:Presenter
  Title="Workspace overview"
  Content="See activity and open the complete report."
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

`PresenterPosition` always describes the presentation region, not the text. Reversing the position does not change the shared left alignment of title, content, and body.

## TopDown mode

`TopDown` places the `Presentation` region across the top and the copy-and-body region below it. The lower region keeps `Title`, `Content`, and `Body` aligned together on the left.

```xml
<flourish:Presenter
  Title="Release summary"
  Content="Review the main changes, then open the full notes."
  PresenterMode="TopDown"
  PresenterPosition="Right">
  <flourish:Presenter.Body>
    <flourish:Button
      Command="{Binding OpenReleaseNotesCommand}"
      Content="Open release notes" />
  </flourish:Presenter.Body>
  <flourish:Presenter.Presentation>
    <Image Source="Assets/release-summary.png" Stretch="Uniform" />
  </flourish:Presenter.Presentation>
</flourish:Presenter>
```

`PresenterPosition` does not change TopDown placement, but declare it as part of the standard Presenter contract.

## Overlay mode

In `Overlay` mode, `Presentation` spans the complete control while title, content, and body render above it. `PresenterPosition` has no visual effect, but the declaration still supplies it.

```xml
<flourish:Presenter
  MinHeight="240"
  Title="Release highlights"
  Content="Explore what changed in this version."
  PresenterMode="Overlay"
  PresenterPosition="Right">
  <flourish:Presenter.Presentation>
    <Image Source="Assets/release-highlights.png" Stretch="UniformToFill" />
  </flourish:Presenter.Presentation>
</flourish:Presenter>
```

Choose presentation content that keeps overlaid text readable in both light and dark themes. A `Grid` assigned to `Presentation` can combine an image with a contrast layer when necessary.

## Present several elements

`Presentation` accepts one WPF content tree. Wrap several icons or visual elements in a layout container:

```xml
<flourish:Presenter
  Title="Supported formats"
  Content="Review the formats available for this export."
  PresenterMode="Split"
  PresenterPosition="Right">
  <UniformGrid Columns="3">
    <flourish:FlourishTextBlock Role="Icon" Text="&#xE8A5;" />
    <flourish:FlourishTextBlock Role="Icon" Text="&#xE7C3;" />
    <flourish:FlourishTextBlock Role="Icon" Text="&#xE8B7;" />
  </UniformGrid>
</flourish:Presenter>
```

The direct `UniformGrid` is assigned to `Presentation`. Always use an explicit `<flourish:Presenter.Body>` element when the copy side also needs supporting controls.

## HeaderChunk

`HeaderChunk` inherits `Presenter` and uses the same explicit title, content, mode, position, body, and presentation contract. It is a page-level peer of `Chunk`, uses an emphasized background and the HeaderSize title, and appears once at the beginning of a standard content page. Unlike `Presenter`, its default XAML content property is `Body`, so assign `HeaderChunk.Presentation` explicitly.

## Related content

- [Chunk](chunk.md) defines the page hierarchy and the specialized `HeaderChunk`.
- [Card](card.md) explains when concise text or one icon belongs on a card.
- [Document](document.md) presents several text-only paragraphs.
- [Button](button.md) defines controls that may appear in `Body`.
- The [Presenter API](xref:ArkheideSystem.Flourish.Controls.Presenter), [PresenterMode API](xref:ArkheideSystem.Flourish.Controls.PresenterMode), [PresenterPosition API](xref:ArkheideSystem.Flourish.Controls.PresenterPosition), and [HeaderChunk API](xref:ArkheideSystem.Flourish.Controls.HeaderChunk) list all members.
