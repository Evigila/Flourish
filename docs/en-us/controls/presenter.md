---
title: Presenter
description: Combine copy, controls, and rich content in three layouts.
---

# Presenter

`Presenter` combines copy, supporting controls, and rich content. Split and Overlay are full-width; only TopDown may share a row.

Use [Card](card.md) when a surface needs at most one icon and one paragraph. Use `Presenter` when the presentation needs an image, an icon group, or its own content tree.

Every Presenter declaration explicitly supplies `Title`, `Content`, `PresenterMode`, and `PresenterPosition`. The runtime fallback values are `Split` and `Left`, but naming them at the call site keeps the intended composition clear.

## Content regions

| Property | Type | Default | Purpose |
| --- | --- | --- | --- |
| `Title` | `string` | `""` | Required presentation heading. |
| `Content` | `string?` | `null` | Required supporting copy below the title. |
| `Body` | `object?` | `null` | Controls or supporting content arranged with the copy; assign it explicitly with `Presenter.Body`. |
| `Presentation` | `object?` | `null` | Image, icon group, illustration, preview, or composed visual; the default XAML content property. |
| `PresentationMinHeight` | `double` | `0` | Minimum height of the built-in presentation surface. The default Presenter style supplies a 160 DIP baseline. |
| `PresenterMode` | `PresenterMode` | `Split` | Explicit composition choice: `Split`, `TopDown`, or `Overlay`. |
| `PresenterPosition` | `PresenterPosition` | `Left` | Explicit presentation-side choice for `Split`. |

An absent `Body` collapses. Only `Presentation` supplies the neutral rounded surface; fixed content centers and stretchable content fills it. Match `PresentationMinHeight` across peers when needed.

When several Presenters are stacked vertically in one section, apply `FlourishPresenterPeerMargin` to each Presenter after the first.

## Split mode

`Split` uses a horizontal two-region layout. `Title`, `Content`, and `Body` remain together on one side; the rounded `Presentation` surface fills the other.

| Position | Arrangement |
| --- | --- |
| `Left` | `Presentation` is on the left; copy and `Body` are on the right. This is the standard arrangement and runtime fallback. |
| `Right` | Copy and `Body` are on the left; `Presentation` is on the right. |

```xml
<flourish:Presenter
  Title="Workspace overview"
  Content="See activity and open the complete report."
  PresenterMode="Split"
  PresenterPosition="Left">
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

`TopDown` places the `Presentation` region across the top and the copy-and-body region below it. The lower region keeps `Title`, `Content`, and `Body` aligned together on the left. Peer TopDown Presenters may share a multi-column row when every presentation and description remains readable.

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

`Presentation` accepts one WPF content tree and fills its region. Vertical `StackPanel` content stretches horizontally unless centered explicitly, while other auto-sized groups center by desired bounds. A filling `CodeSpace` needs `IsExpanded="True"`; content clips to the shared radius, so do not add a decorative `Border`.

```xml
<flourish:Presenter
  Title="Supported formats"
  Content="Review the formats available for this export."
  PresenterMode="Split"
  PresenterPosition="Right">
  <UniformGrid
    Columns="3"
    HorizontalAlignment="Center"
    VerticalAlignment="Center">
    <flourish:TextBlock Role="Icon" Text="&#xE8A5;" />
    <flourish:TextBlock Role="Icon" Text="&#xE7C3;" />
    <flourish:TextBlock Role="Icon" Text="&#xE8B7;" />
  </UniformGrid>
</flourish:Presenter>
```

The direct `UniformGrid` is assigned to `Presentation`. Always use an explicit `<flourish:Presenter.Body>` element when the copy side also needs supporting controls.

## HeaderChunk

`HeaderChunk` inherits the same regions and layouts but appears once at the page start and always fills a row. Its Split fallback is `Right`, and its default XAML content is `Body`; assign `HeaderChunk.Presentation` explicitly.

## Related content

- [Chunk](chunk.md)
- [Card](card.md)
- [Document](document.md)
- [Button](button.md)
- [Presenter API](xref:ArkheideSystem.Flourish.Controls.Presenter), [PresenterMode API](xref:ArkheideSystem.Flourish.Controls.PresenterMode), [PresenterPosition API](xref:ArkheideSystem.Flourish.Controls.PresenterPosition), and [HeaderChunk API](xref:ArkheideSystem.Flourish.Controls.HeaderChunk)
