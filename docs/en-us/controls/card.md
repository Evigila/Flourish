---
title: Card
description: Use Card and IconCard to group information on themed, non-interactive surfaces with optional visual presenters.
---

# Card

`Card` is a non-interactive surface for grouping related information. It provides built-in title and supporting-text regions, can host arbitrary WPF content, and adapts its colors to the active Flourish theme.

> [!IMPORTANT]
> Use `CardButton` instead when clicking anywhere on the surface performs one action. Do not add mouse handlers to `Card` to reproduce button behavior.

## Basic usage

Set `Title` and `Text` for ordinary informational cards. There is no need to construct text blocks for these two roles.

```xml
<flourish:Card
  Title="Account status"
  Text="Your workspace is synchronized." />
```

`Card` derives from `ContentControl`, so it can also host one arbitrary content tree. Wrap multiple children in a `Grid`, `StackPanel`, or another layout container.

```xml
<flourish:Card
  Title="Storage"
  Text="Manage files associated with this workspace.">
  <StackPanel Margin="0,12,0,0">
    <ProgressBar Maximum="100" Value="64" />
    <flourish:Button
      Margin="0,12,0,0"
      HorizontalAlignment="Left"
      Content="Review files" />
  </StackPanel>
</flourish:Card>
```

### Card properties

| Property | Type | Default | Purpose |
| --- | --- | --- | --- |
| `Variant` | `CardVariant` | `Standard` | Selects the semantic surface treatment. |
| `Title` | `string` | `""` | The card heading. |
| `Text` | `string` | `""` | Optional supporting text displayed with the title. |
| `ContentHorizontalAlignment` | `HorizontalAlignment` | `Stretch` | Horizontally positions the built-in `Title` and `Text` region. |
| `ContentVerticalAlignment` | `VerticalAlignment` | `Stretch` | Vertically positions the built-in `Title` and `Text` region. |
| `Content` | inherited `object?` | `null` | An optional arbitrary WPF content tree. |
| `HorizontalContentAlignment` | inherited `HorizontalAlignment` | `Stretch` | Horizontally positions arbitrary `Content`; it does not position `Title` or `Text`. |
| `VerticalContentAlignment` | inherited `VerticalAlignment` | `Stretch` | Vertically positions arbitrary `Content`; it does not position `Title` or `Text`. |

## Variants

`CardVariant` has four values:

| Variant | Use |
| --- | --- |
| `Standard` | The default surface for ordinary grouped information. |
| `Tonal` | A pale neutral gray surface for supporting information with lower emphasis. |
| `Filled` | A blue tonal surface for information that needs stronger visual emphasis. |
| `Elevated` | A raised surface for information that must remain visually separate from its background. |

```xml
<UniformGrid Columns="2">
  <flourish:Card Variant="Standard" Title="Standard" Text="Ordinary information" />
  <flourish:Card Variant="Tonal" Title="Tonal" Text="Supporting information" />
  <flourish:Card Variant="Filled" Title="Filled" Text="Emphasized information" />
  <flourish:Card Variant="Elevated" Title="Elevated" Text="Separated information" />
</UniformGrid>
```

`Filled` uses the same primary filled color family as `Button`. Set a local `Background` when another fill is required. A local value takes precedence over the variant default; use a dynamic theme resource if the replacement must adapt to light and dark themes.

```xml
<flourish:Card
  Variant="Filled"
  Background="{DynamicResource FlourishSecondaryBrush}"
  Foreground="{DynamicResource FlourishForegroundOnSecondaryBrush}"
  Title="Custom filled surface"
  Text="This replacement follows the active theme." />
```

## Align the copy region

`ContentHorizontalAlignment` and `ContentVerticalAlignment` position the built-in title and text together. They do not change the presenter or arbitrary `Content`.

```xml
<flourish:Card
  MinHeight="160"
  ContentHorizontalAlignment="Center"
  ContentVerticalAlignment="Center"
  Title="Centered copy"
  Text="The title and text remain one aligned region." />
```

## IconCard

`IconCard` has the same title, text, content, alignment, and variant contract as `Card`. Its `Presenter` can hold an icon, an image, an illustration, or any other WPF content.

```xml
<flourish:IconCard
  PresenterMode="Split"
  PresenterPosition="Left"
  Title="Reports"
  Text="Review generated reports and recent exports.">
  <flourish:IconCard.Presenter>
    <TextBlock
      FontFamily="Segoe Fluent Icons"
      FontSize="32"
      Text="&#xE8A5;" />
  </flourish:IconCard.Presenter>
</flourish:IconCard>
```

### IconCard properties

| Property | Type | Default | Purpose |
| --- | --- | --- | --- |
| `Presenter` | `object?` | `null` | The icon, image, illustration, or other visual content. |
| `PresenterMode` | `PresenterMode` | `Split` | Selects a separate presenter region or a full-card overlay. |
| `PresenterPosition` | `PresenterPosition` | `Left` | Locates `Presenter` in `Split` mode. It is ignored in `Overlay` mode. |

In `Split` mode, `PresenterPosition` always describes the presenter location. The title, text, and content are arranged on the opposing side.

| Position | Presenter placement |
| --- | --- |
| `Left` | Centered along the left side. |
| `LeftTop` | At the upper-left side. |
| `LeftBottom` | At the lower-left side. |
| `Top` | Centered along the top. |
| `Bottom` | Centered along the bottom. |
| `Right` | Centered along the right side. |
| `RightTop` | At the upper-right side. |
| `RightBottom` | At the lower-right side. |

## Overlay presenters

In `Overlay` mode, `Presenter` fills the card and remains centered while the title, text, and content are rendered above it. `PresenterPosition` has no effect. Choose or compose a presenter that keeps the overlaid copy readable in both themes.

```xml
<flourish:IconCard
  MinHeight="240"
  PresenterMode="Overlay"
  Title="Project preview"
  Text="The presenter fills the complete card.">
  <flourish:IconCard.Presenter>
    <Image Source="Assets/project-preview.png" Stretch="UniformToFill" />
  </flourish:IconCard.Presenter>
</flourish:IconCard>
```

## Related content

- [Chunk](chunk.md) explains how cards participate in page sections.
- [Button](button.md) explains when an information surface should instead be an interactive `CardButton`.
- The [CardVariant API](xref:ArkheideSystem.Flourish.Controls.CardVariant), [Card API](xref:ArkheideSystem.Flourish.Controls.Card), [IconCard API](xref:ArkheideSystem.Flourish.Controls.IconCard), [PresenterMode API](xref:ArkheideSystem.Flourish.Controls.PresenterMode), and [PresenterPosition API](xref:ArkheideSystem.Flourish.Controls.PresenterPosition) list all members.
